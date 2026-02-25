namespace Line.Messaging;

[PublicAPI]
public interface ILineDataClient
{
    /// <summary>
    /// Retrieve image, video, and audio data sent by users as Stream
    /// https://developers.line.me/en/docs/messaging-api/reference/#get-content
    /// </summary>
    /// <param name="messageId">Message ID</param>
    /// <returns>Content as ContentStream</returns>
    ValueTask<Outcome<ContentStream>> GetContentStreamAsync(string messageId);
}

public class LineDataClient(HttpClient http) : ILineDataClient
{
    public const string OfficialUri = "https://api-data.line.me/v2/";

    public async ValueTask<Outcome<ContentStream>> GetContentStreamAsync(string messageId)
    {
        if (Fail(await http.Get($"bot/message/{messageId}/content").ConfigureAwait(false), out var e, out var response)
         || Fail(await response.ReadStream().ConfigureAwait(false), out e, out var stream)) return e.Trace();

        return new ContentStream(stream, response.Content.Headers);
    }
}