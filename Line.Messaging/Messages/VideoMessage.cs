namespace Line.Messaging;

/// <summary>
/// Video
/// https://developers.line.me/en/docs/messaging-api/reference/#video
/// </summary>
public class VideoMessage : Message
{
    /// <summary>
    /// URL of video file (Max: 1000 characters)
    /// HTTPS
    /// mp4
    /// Less than 1 minute
    /// Max: 10 MB
    /// </summary>
    public string OriginalContentUrl { get; }

    /// <summary>
    /// URL of preview image (Max: 1000 characters)
    /// HTTPS
    /// JPEG
    /// Max: 240 x 240
    /// Max: 1 MB
    /// </summary>
    public string PreviewImageUrl { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="originalContentUrl">
    /// URL of video file (Max: 1000 characters)
    /// HTTPS
    /// mp4
    /// Less than 1 minute
    /// Max: 10 MB
    /// </param>
    /// <param name="previerImageUrl">
    /// URL of preview image (Max: 1000 characters)
    /// HTTPS
    /// JPEG
    /// Max: 240 x 240
    /// Max: 1 MB
    /// </param>
    /// <param name="quickReply">
    /// QuickReply
    /// </param>
    public VideoMessage(string originalContentUrl, string previerImageUrl, QuickReply? quickReply = null) {
        Type = MessageType.Video;
        OriginalContentUrl = originalContentUrl;
        PreviewImageUrl = previerImageUrl;
        QuickReply = quickReply;
    }
}