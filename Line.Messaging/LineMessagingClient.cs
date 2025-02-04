using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using LanguageExt;

namespace Line.Messaging;

/// <summary>
/// LINE Messaging API client, which handles request/response to LINE server.
/// </summary>
[PublicAPI]
public class LineMessagingClient(HttpClient http) : ILineMessagingClient
{
    public const string OfficialUri = "https://api.line.me/v2/";

    #region Message
    // https://developers.line.me/en/docs/messaging-api/reference/#message

    public Task<BotInfo> GetBotInfo()
        => http.GetLineJsonAsync<BotInfo>("bot/info");

    public Task ReplyMessageAsync(string replyToken, IEnumerable<ISendMessage> messages)
        => http.PostAsJsonAsync("bot/message/reply", new { replyToken, messages }, LineJson.Options)
                 .EnsureSuccessStatusCodeAsync();

    public Task ReplyMessageAsync(string replyToken, params string[] messages)
        => ReplyMessageAsync(replyToken, from msg in messages select new TextMessage{ Text = msg });

    public Task ReplyMessageWithJsonAsync(string replyToken, params string[] messages)
        => http.PostAsJsonAsync("bot/message/reply", new { replyToken, messages = messages.Join(", ") }, LineJson.Options)
                 .EnsureSuccessStatusCodeAsync();

    public Task PushMessageAsync(string to, IEnumerable<ISendMessage> messages)
        => http.PostAsJsonAsync("bot/message/push", new { to, messages }, LineJson.Options)
                 .EnsureSuccessStatusCodeAsync();

    public Task PushMessageWithJsonAsync(string to, params string[] messages)
        => http.PostAsJsonAsync("bot/message/push", new { to, messages = messages.Join(", ") }, LineJson.Options)
                 .EnsureSuccessStatusCodeAsync();

    public Task PushMessageAsync(string to, params string[] messages)
        => PushMessageAsync(to, messages.Select(msg => new TextMessage{ Text = msg }));

    public Task MultiCastMessageAsync(IEnumerable<string> to, IEnumerable<ISendMessage> messages)
        => http.PostAsJsonAsync("bot/message/multicast", new { to, messages }, LineJson.Options)
                 .EnsureSuccessStatusCodeAsync();

    public Task MultiCastMessageWithJsonAsync(IEnumerable<string> to, params string[] messages)
        => http.PostAsJsonAsync("bot/message/multicast", new {
                      to = (from x in to select $"\"{x}\"" ).Join(", "),
                      messages = messages.Join(", ")
                  }, LineJson.Options)
                 .EnsureSuccessStatusCodeAsync();

    public Task MultiCastMessageAsync(IEnumerable<string> to, params string[] messages)
        => MultiCastMessageAsync(to, messages.Select(msg => new TextMessage{ Text = msg }));

    public async Task<byte[]> GetContentBytesAsync(string messageId)
    {
        var response = await http.GetAsync($"bot/message/{messageId}/content").EnsureSuccessStatusCodeAsync();
        return await response.Content.ReadAsByteArrayAsync();
    }

    #endregion

    #region Profile
    // https://developers.line.me/en/docs/messaging-api/reference/#profile

    public Task<UserProfile> GetUserProfileAsync(string userId)
        => http.GetLineJsonAsync<UserProfile>($"bot/profile/{userId}");

    #endregion

    #region Group
    // https://developers.line.me/en/docs/messaging-api/reference/#group

    public Task<UserProfile> GetGroupMemberProfileAsync(string groupId, string userId, CancellationToken cancelToken = default)
        => http.GetLineJsonAsync<UserProfile>($"bot/group/{groupId}/member/{userId}", cancelToken);

    public Task<GroupMemberIds> GetGroupMemberIdsAsync(string groupId, string? continuationToken, CancellationToken cancelToken = default)
        => http.GetLineJsonAsync<GroupMemberIds>($"bot/group/{groupId}/members/ids" + (continuationToken is null ? string.Empty : $"?start={continuationToken}"), cancelToken);

    public async IAsyncEnumerable<UserProfile> GetGroupMemberProfilesAsync(string groupId, [EnumeratorCancellation] CancellationToken cancelToken)
    {
        string? continuationToken = null;
        do
        {
            var ids = await GetGroupMemberIdsAsync(groupId, continuationToken, cancelToken);

            var tasks = ids.MemberIds.Select(userId => GetGroupMemberProfileAsync(groupId, userId, cancelToken));
            var profiles = await Task.WhenAll(tasks.ToArray());

            foreach (var profile in profiles)
                yield return profile;

            continuationToken = ids.Next;
        }
        while (continuationToken is not null);
    }

    public Task LeaveFromGroupAsync(string groupId)
        => http.PostAsync($"bot/group/{groupId}/leave", null).EnsureSuccessStatusCodeAsync();

    #endregion

    #region Room
    // https://developers.line.me/en/docs/messaging-api/reference/#room

    public Task<UserProfile> GetRoomMemberProfileAsync(string roomId, string userId, CancellationToken cancelToken = default)
        => http.GetLineJsonAsync<UserProfile>($"bot/room/{roomId}/member/{userId}", cancelToken);

    public Task<GroupMemberIds> GetRoomMemberIdsAsync(string roomId, string? continuationToken, CancellationToken cancelToken = default)
        => http.GetLineJsonAsync<GroupMemberIds>($"bot/room/{roomId}/members/ids" + (continuationToken is null ? string.Empty : $"?start={continuationToken}"), cancelToken);

    public async IAsyncEnumerable<UserProfile> GetRoomMemberProfilesAsync(string roomId, [EnumeratorCancellation] CancellationToken cancelToken)
    {
        string? continuationToken = null;
        do
        {
            var ids = await GetRoomMemberIdsAsync(roomId, continuationToken, cancelToken);

            var tasks = ids.MemberIds.Select(userId => GetRoomMemberProfileAsync(roomId, userId, cancelToken));
            var profiles = await Task.WhenAll(tasks.ToArray());

            foreach (var profile in profiles)
                if (cancelToken.IsCancellationRequested)
                    yield break;
                else
                    yield return profile;

            continuationToken = ids.Next;
        }
        while (continuationToken is not null);
    }

    public Task LeaveFromRoomAsync(string roomId)
        => http.PostAsync($"bot/room/{roomId}/leave", content: null).EnsureSuccessStatusCodeAsync();

    #endregion

    #region Rich menu
    // https://developers.line.me/en/docs/messaging-api/reference/#rich-menu

    public Task<ResponseRichMenu> GetRichMenuAsync(string richMenuId)
        => http.GetLineJsonAsync<ResponseRichMenu>($"bot/richmenu/{richMenuId}");

    public Task<string> CreateRichMenuAsync(RichMenu richMenu)
        => http.PostAsJsonAsync("bot/richmenu", richMenu, LineJson.Options)
                 .GetLineJsonAsync<RichMenuInfo>()
                 .Select(x => x.RichMenuId);

    readonly record struct RichMenuInfo(string RichMenuId);

    public Task DeleteRichMenuAsync(string richMenuId)
        => http.DeleteAsync("bot/richmenu/{richMenuId}").EnsureSuccessStatusCodeAsync();

    public Task<string> GetRichMenuIdOfUserAsync(string userId)
        => http.GetAsync($"bot/user/{userId}/richmenu")
                 .GetLineJsonAsync<RichMenuInfo>()
                 .Select(x => x.RichMenuId);

    public Task SetDefaultRichMenuAsync(string richMenuId)
        => http.PostAsync($"bot/user/all/richmenu/{richMenuId}", content: null).EnsureSuccessStatusCodeAsync();

    public Task LinkRichMenuToUserAsync(string userId, string richMenuId)
        => http.PostAsync($"bot/user/{userId}/richmenu/{richMenuId}", content: null).EnsureSuccessStatusCodeAsync();

    public Task UnLinkRichMenuFromUserAsync(string userId)
        => http.DeleteAsync($"bot/user/{userId}/richmenu").EnsureSuccessStatusCodeAsync();

    public async Task<ContentStream> DownloadRichMenuImageAsync(string richMenuId)
    {
        var response = await http.GetAsync($"bot/richmenu/{richMenuId}/content").EnsureSuccessStatusCodeAsync();
        return new ContentStream(await response.Content.ReadAsStreamAsync(), response.Content.Headers);
    }

    public Task UploadRichMenuJpegImageAsync(Stream stream, string richMenuId)
        => UploadRichMenuImageAsync(stream, richMenuId, "image/jpeg");

    public Task UploadRichMenuPngImageAsync(Stream stream, string richMenuId)
        => UploadRichMenuImageAsync(stream, richMenuId, "image/png");

    public async Task UploadRichMenuImageAsync(Stream stream, string richMenuId, string mediaType)
    {
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        await http.PostAsync($"bot/richmenu/{richMenuId}/content", content).EnsureSuccessStatusCodeAsync();
    }

    public async Task<ResponseRichMenu[]> GetRichMenuListAsync(CancellationToken cancelToken) {
        var response = await http.GetAsync("bot/richmenu/list", cancelToken).ConfigureAwait(false);
        return response.StatusCode == System.Net.HttpStatusCode.NotFound
                   ? []
                   : (await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<RichMenuList>(LineJson.Options, cancelToken)).Richmenus;
    }
    readonly record struct RichMenuList(ResponseRichMenu[] Richmenus);

    #endregion

    #region Account Link

    public Task<string> IssueLinkTokenAsync(string userId)
        => http.PostAsync($"bot/user/{userId}/linkToken", content: null)
                 .GetLineJsonAsync<LinkTokenInfo>()
                 .Select(x => x.LinkToken);

    readonly record struct LinkTokenInfo(string LinkToken);

    #endregion

    #region Number of sent messages

    public Task<NumberOfSentMessages> GetNumberOfSentReplyMessagesAsync(DateTime date)
        => http.GetLineJsonAsync<NumberOfSentMessages>($"bot/message/delivery/reply?date={date:yyyyMMdd}");

    public Task<NumberOfSentMessages> GetNumberOfSentPushMessagesAsync(DateTime date)
        => http.GetLineJsonAsync<NumberOfSentMessages>($"bot/message/delivery/push?date={date:yyyyMMdd}");

    public Task<NumberOfSentMessages> GetNumberOfSentMulticastMessagesAsync(DateTime date)
        => http.GetLineJsonAsync<NumberOfSentMessages>($"bot/message/delivery/multicast?date={date:yyyyMMdd}");

    #endregion
}