using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Line.Messaging;

/// <summary>
/// LINE Messaging API client, which handles request/response to LINE server.
/// </summary>
[PublicAPI]
public class LineMessagingClient(HttpClient http, ILogger? logger = null) : ILineMessagingClient
{
    public const string OfficialUri = "https://api.line.me/v2/";

    const int MaxMessageBatchSize = 5;

    #region Message

    // https://developers.line.me/en/docs/messaging-api/reference/#message

    public ValueTask<Outcome<BotInfo>> GetBotInfo()
        => http.GetLineJsonAsync<BotInfo>("bot/info");

    public ValueTask<Outcome<LanguageExt.Unit>> ReplyMessageAsync(string replyToken, IEnumerable<Message> messages)
        => http.PostJson("bot/message/reply", new { replyToken, messages }, LineJson.Options).CheckSucceed();

    public ValueTask<Outcome<LanguageExt.Unit>> ReplyMessageAsync(string replyToken, params string[] messages)
        => ReplyMessageAsync(replyToken, from msg in messages select new TextMessage { Text = msg });

    public ValueTask<Outcome<LanguageExt.Unit>> ReplyMessageWithJsonAsync(string replyToken, params string[] messages)
        => http.PostJson("bot/message/reply", new { replyToken, messages = messages.Join(", ") }, LineJson.Options).CheckSucceed();

    /// <summary>
    /// Sends in batches of five, LINE's cap per call. Each batch carries its own <see cref="LineApiErrors.RETRY_KEY_HEADER"/>,
    /// set once on the request so any retry of that request by the HTTP pipeline re-sends the same key and LINE
    /// executes the push only once.
    /// </summary>
    public async ValueTask<Outcome<LanguageExt.Unit>> PushMessageAsync(string to, IEnumerable<Message> messages, CancellationToken cancel = default) {
        foreach(var messageBlocks in messages.Batch(MaxMessageBatchSize)){
            var botMessage = new { to, messages = messageBlocks.AsArray() };
            using var request = new HttpRequestMessage(HttpMethod.Post, "bot/message/push");
            request.Headers.Add(LineApiErrors.RETRY_KEY_HEADER, Guid.NewGuid().ToString());
            request.Content = JsonContent.Create(botMessage, options: LineJson.Options);
            if (Fail(await http.TrySend(request, cancel).CheckSucceed(), out var e)){
                if (logger?.IsEnabled(LogLevel.Debug) == true){
                    var unwrap = JsonSerialize(botMessage, LineJson.Options).Unwrap();
                    logger.LogDebug("Push message failed: {Message} ==> {@Error}", unwrap, e);
                }
                return e.Trace("Push message failed");
            }
        }
        return unit;
    }

    public ValueTask<Outcome<LanguageExt.Unit>> PushMessageWithJsonAsync(string to, params string[] messages)
        => http.PostJson("bot/message/push", new { to, messages = messages.Join(", ") }, LineJson.Options).CheckSucceed();

    public ValueTask<Outcome<LanguageExt.Unit>> PushMessageAsync(string to, params string[] messages)
        => PushMessageAsync(to, messages.Select(msg => new TextMessage { Text = msg }));

    public ValueTask<Outcome<LanguageExt.Unit>> MultiCastMessageAsync(IEnumerable<string> to, IEnumerable<Message> messages)
        => http.PostJson("bot/message/multicast", new { to, messages }, LineJson.Options).CheckSucceed();

    public ValueTask<Outcome<LanguageExt.Unit>> MultiCastMessageWithJsonAsync(IEnumerable<string> to, params string[] messages)
        => http.PostJson("bot/message/multicast", new {
            to = (from x in to select $"\"{x}\"").Join(", "),
            messages = messages.Join(", ")
        }, LineJson.Options).CheckSucceed();

    public ValueTask<Outcome<LanguageExt.Unit>> MultiCastMessageAsync(IEnumerable<string> to, params string[] messages)
        => MultiCastMessageAsync(to, messages.Select(msg => new TextMessage { Text = msg }));

    public async ValueTask<Outcome<(string, byte[])>> GetContentBytesAsync(string messageId) {
        if (Fail(await http.Get($"bot/message/{messageId}/content").ConfigureAwait(false), out var e, out var response)) return e.Trace();

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        using (response){
            if (Fail(await response.ReadByteArray().ConfigureAwait(false), out e, out var data)) return e.Trace();

            return (contentType, data);
        }
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

    public async IAsyncEnumerable<Outcome<UserProfile>> GetGroupMemberProfilesAsync(string groupId, [EnumeratorCancellation] CancellationToken cancelToken) {
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
        } while (continuationToken is not null);
    }

    public ValueTask<Outcome<LanguageExt.Unit>> LeaveFromGroupAsync(string groupId)
        => http.Post($"bot/group/{groupId}/leave", null).CheckSucceed();

    #endregion

    #region Room

    // https://developers.line.me/en/docs/messaging-api/reference/#room

    public ValueTask<Outcome<UserProfile>> GetRoomMemberProfileAsync(string roomId, string userId, CancellationToken cancelToken = default)
        => http.GetLineJsonAsync<UserProfile>($"bot/room/{roomId}/member/{userId}", cancelToken);

    public ValueTask<Outcome<GroupMemberIds>> GetRoomMemberIdsAsync(string roomId, string? continuationToken, CancellationToken cancelToken = default)
        => http.GetLineJsonAsync<GroupMemberIds>($"bot/room/{roomId}/members/ids" + (continuationToken is null ? string.Empty : $"?start={continuationToken}"), cancelToken);

    public async IAsyncEnumerable<Outcome<UserProfile>> GetRoomMemberProfilesAsync(string roomId, [EnumeratorCancellation] CancellationToken cancelToken) {
        string? continuationToken = null;
        do{
            if (Fail(await GetRoomMemberIdsAsync(roomId, continuationToken, cancelToken).ConfigureAwait(false), out var e, out var ids)){
                yield return e.Trace();
                yield break;
            }

            foreach (var userId in ids.MemberIds){
                if (Fail(await GetRoomMemberProfileAsync(roomId, userId, cancelToken).ConfigureAwait(false), out e, out var profile))
                    yield return e.Trace();
                else
                    yield return profile;
            }
            continuationToken = ids.Next;
        } while (continuationToken is not null);
    }

    public ValueTask<Outcome<LanguageExt.Unit>> LeaveFromRoomAsync(string roomId)
        => http.Post($"bot/room/{roomId}/leave", null).CheckSucceed();

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

    public ValueTask<Outcome<LanguageExt.Unit>> DeleteRichMenuAsync(string richMenuId)
        => http.Delete($"bot/richmenu/{richMenuId}").CheckSucceed();

    public async ValueTask<Outcome<string>> GetRichMenuIdOfUserAsync(string userId) {
        if (Fail(await http.GetLineJsonAsync<RichMenuInfo>($"bot/user/{userId}/richmenu"), out var e, out var info))
            return e.Trace();
        return info.RichMenuId;
    }

    public ValueTask<Outcome<LanguageExt.Unit>> SetDefaultRichMenuAsync(string richMenuId)
        => http.Post($"bot/user/all/richmenu/{richMenuId}", null).CheckSucceed();

    public ValueTask<Outcome<LanguageExt.Unit>> LinkRichMenuToUserAsync(string userId, string richMenuId)
        => http.Post($"bot/user/{userId}/richmenu/{richMenuId}", null).CheckSucceed();

    public ValueTask<Outcome<LanguageExt.Unit>> UnLinkRichMenuFromUserAsync(string userId)
        => http.Delete($"bot/user/{userId}/richmenu").CheckSucceed();

    public async ValueTask<Outcome<ContentStream>> DownloadRichMenuImageAsync(string richMenuId) {
        if (Fail(await http.Get($"bot/richmenu/{richMenuId}/content").ConfigureAwait(false), out var e, out var response)
         || Fail(await response.ReadStream().ConfigureAwait(false), out e, out var stream)){
            response?.Dispose();
            return e.Trace();
        }

        return new ContentStream(stream, response.Content.Headers);
    }

    public ValueTask<Outcome<LanguageExt.Unit>> UploadRichMenuJpegImageAsync(Stream stream, string richMenuId)
        => UploadRichMenuImageAsync(stream, richMenuId, "image/jpeg");

    public ValueTask<Outcome<LanguageExt.Unit>> UploadRichMenuPngImageAsync(Stream stream, string richMenuId)
        => UploadRichMenuImageAsync(stream, richMenuId, "image/png");

    public async ValueTask<Outcome<LanguageExt.Unit>> UploadRichMenuImageAsync(Stream stream, string richMenuId, string mediaType) {
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        return await http.Post($"bot/richmenu/{richMenuId}/content", content).CheckSucceed();
    }

    public async ValueTask<Outcome<ResponseRichMenu[]>> GetRichMenuListAsync(CancellationToken cancelToken) {
        if (Fail(await http.Get("bot/richmenu/list", cancelToken).ConfigureAwait(false), out var e, out var response)) return e.Trace();
        using (response)
            return response.StatusCode == System.Net.HttpStatusCode.NotFound
                       ? Array.Empty<ResponseRichMenu>()
                       : Fail(await response.DeserializedJson<RichMenuList>(LineJson.Options).ConfigureAwait(false), out e, out var list)
                             ? e.Trace()
                             : list.Richmenus;
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