namespace Line.Messaging;

public interface ILineDataClient
{
    /// <summary>
    /// Retrieve image, video, and audio data sent by users as Stream
    /// https://developers.line.me/en/docs/messaging-api/reference/#get-content
    /// </summary>
    /// <param name="messageId">Message ID</param>
    /// <returns>Content as ContentStream</returns>
    Task<ContentStream> GetContentStreamAsync(string messageId);
}

public class LineDataClient(HttpClient http) : ILineDataClient
{
    public const string OfficialUri = "https://api-data.line.me";

    public async Task<ContentStream> GetContentStreamAsync(string messageId)
    {
        var response = await http.GetAsync($"/bot/message/{messageId}/content").EnsureSuccessStatusCodeAsync();
        return new ContentStream(await response.Content.ReadAsStreamAsync(), response.Content.Headers);
    }
}