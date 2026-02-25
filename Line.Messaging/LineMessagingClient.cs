using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

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

    public ValueTask<Outcome<BotInfo>> GetBotInfo()
        => http.GetLineJsonAsync<BotInfo>("bot/info");

    public ValueTask<Outcome<LanguageExt.Unit>> ReplyMessageAsync(string replyToken, IEnumerable<Message> messages)
        => http.PostJson("bot/message/reply", new { replyToken, messages }, LineJson.Options).CheckSucceed();

    public ValueTask<Outcome<LanguageExt.Unit>>  ReplyMessageAsync(string replyToken, params string[] messages)
        => ReplyMessageAsync(replyToken, from msg in messages select new TextMessage{ Text = msg });

    public Task ReplyMessageWithJsonAsync(string replyToken, params string[] messages)
        => http.PostAsJsonAsync("bot/message/reply", new { replyToken, messages = messages.Join(", ") }, LineJson.Options)
                 .EnsureSuccessStatusCodeAsync();

    public Task PushMessageAsync(string to, IEnumerable<Message> messages)
        => http.PostAsJsonAsync("bot/message/push", new { to, messages }, LineJson.Options)
                 .EnsureSuccessStatusCodeAsync();

    public Task PushMessageWithJsonAsync(string to, params string[] messages)
        => http.PostAsJsonAsync("bot/message/push", new { to, messages = messages.Join(", ") }, LineJson.Options)
                 .EnsureSuccessStatusCodeAsync();

    public Task PushMessageAsync(string to, params string[] messages)
        => PushMessageAsync(to, messages.Select(msg => new TextMessage{ Text = msg }));

    public Task MultiCastMessageAsync(IEnumerable<string> to, IEnumerable<Message> messages)
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

    public async Task<(string, byte[])> GetContentBytesAsync(string messageId)
    {
        var response = await http.GetAsync($"bot/message/{messageId}/content").EnsureSuccessStatusCodeAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var data = await response.Content.ReadAsByteArrayAsync();
        return (contentType, data);
    }

    #endregion

    #region Profile
    // https://developers.line.me/en/docs/messaging-api/reference/#profile

    public ValueTask<Outcome<UserProfile>> GetUserProfileAsync(string userId)
        => http.GetLineJsonAsync<UserProfile>($"bot/profile/{userId}");

    #endregion

    #region Group
    // https://developers.line.me/en/docs/messaging-api/reference/#group

    public ValueTask<Outcome<UserProfile>> GetGroupMemberProfileAsync(string groupId, string userId, CancellationToken cancelToken = default)
        => http.GetLineJsonAsync<UserProfile>($"bot/group/{groupId}/member/{userId}", cancelToken);

    public ValueTask<Outcome<GroupMemberIds>> GetGroupMemberIdsAsync(string groupId, string? continuationToken, CancellationToken cancelToken = default)
        => http.GetLineJsonAsync<GroupMemberIds>($"bot/group/{groupId}/members/ids" + (continuationToken is null ? string.Empty : $"?start={continuationToken}"), cancelToken);

    public async IAsyncEnumerable<Outcome<UserProfile>> GetGroupMemberProfilesAsync(string groupId, [EnumeratorCancellation] CancellationToken cancelToken)
    {
        string? continuationToken = null;
        do{
            if (Fail(await GetGroupMemberIdsAsync(groupId, continuationToken, cancelToken).ConfigureAwait(false), out var e, out var ids)){
                yield return e.Trace();
                yield break;
            }

            foreach (var userId in ids.MemberIds){
                if (Fail(await GetGroupMemberProfileAsync(groupId, userId, cancelToken).ConfigureAwait(false), out e, out var profile))
                    yield return e.Trace();
                else
                    yield return profile;
            }
            continuationToken = ids.Next;
        }
        while (continuationToken is not null);
    }

    public Task LeaveFromGroupAsync(string groupId)
        => http.PostAsync($"bot/group/{groupId}/leave", null).EnsureSuccessStatusCodeAsync();

    #endregion

    #region Room
    // https://developers.line.me/en/docs/messaging-api/reference/#room

    public ValueTask<Outcome<UserProfile>> GetRoomMemberProfileAsync(string roomId, string userId, CancellationToken cancelToken = default)
        => http.GetLineJsonAsync<UserProfile>($"bot/room/{roomId}/member/{userId}", cancelToken);

    public ValueTask<Outcome<GroupMemberIds>> GetRoomMemberIdsAsync(string roomId, string? continuationToken, CancellationToken cancelToken = default)
        => http.GetLineJsonAsync<GroupMemberIds>($"bot/room/{roomId}/members/ids" + (continuationToken is null ? string.Empty : $"?start={continuationToken}"), cancelToken);

    public async IAsyncEnumerable<UserProfile> GetRoomMemberProfilesAsync(string roomId, [EnumeratorCancellation] CancellationToken cancelToken)
    {
        string? continuationToken = null;
        do
        {
            if (Fail(await GetRoomMemberIdsAsync(roomId, continuationToken, cancelToken), out var idsError, out var ids))
                throw new InvalidOperationException(idsError.Message);

            var tasks = ids.MemberIds.Select(userId => GetRoomMemberProfileAsync(roomId, userId, cancelToken).AsTask());
            var outcomes = await Task.WhenAll(tasks.ToArray());

            foreach (var outcome in outcomes) {
                if (Fail(outcome, out var profileError, out var profile))
                    throw new InvalidOperationException(profileError.Message);
                if (cancelToken.IsCancellationRequested)
                    yield break;
                else
                    yield return profile;
            }

            continuationToken = ids.Next;
        }
        while (continuationToken is not null);
    }

    public Task LeaveFromRoomAsync(string roomId)
        => http.PostAsync($"bot/room/{roomId}/leave", content: null).EnsureSuccessStatusCodeAsync();

    #endregion

    #region Rich menu
    // https://developers.line.me/en/docs/messaging-api/reference/#rich-menu

    public ValueTask<Outcome<ResponseRichMenu>> GetRichMenuAsync(string richMenuId)
        => http.GetLineJsonAsync<ResponseRichMenu>($"bot/richmenu/{richMenuId}");

    public async ValueTask<Outcome<string>> CreateRichMenuAsync(RichMenu richMenu)
        => Fail(await http.PostJson("bot/richmenu", richMenu, LineJson.Options).GetLineJsonAsync<RichMenuInfo>(), out var e, out var info)
               ? e.Trace()
               : info.RichMenuId;

    readonly record struct RichMenuInfo(string RichMenuId);

    public Task DeleteRichMenuAsync(string richMenuId)
        => http.DeleteAsync("bot/richmenu/{richMenuId}").EnsureSuccessStatusCodeAsync();

    public async ValueTask<Outcome<string>> GetRichMenuIdOfUserAsync(string userId) {
        if (Fail(await http.GetLineJsonAsync<RichMenuInfo>($"bot/user/{userId}/richmenu"), out var e, out var info))
            return e.Trace();
        return info.RichMenuId;
    }

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

    public async ValueTask<Outcome<string>> IssueLinkTokenAsync(string userId)
        => Fail(await http.Post($"bot/user/{userId}/linkToken", content: null).GetLineJsonAsync<LinkTokenInfo>(), out var e, out var info)
               ? e.Trace()
               : info.LinkToken;

    readonly record struct LinkTokenInfo(string LinkToken);

    #endregion

    #region Number of sent messages

    public ValueTask<Outcome<NumberOfSentMessages>> GetNumberOfSentReplyMessagesAsync(DateTime date)
        => http.GetLineJsonAsync<NumberOfSentMessages>($"bot/message/delivery/reply?date={date:yyyyMMdd}");

    public ValueTask<Outcome<NumberOfSentMessages>> GetNumberOfSentPushMessagesAsync(DateTime date)
        => http.GetLineJsonAsync<NumberOfSentMessages>($"bot/message/delivery/push?date={date:yyyyMMdd}");

    public ValueTask<Outcome<NumberOfSentMessages>> GetNumberOfSentMulticastMessagesAsync(DateTime date)
        => http.GetLineJsonAsync<NumberOfSentMessages>($"bot/message/delivery/multicast?date={date:yyyyMMdd}");

    #endregion
}