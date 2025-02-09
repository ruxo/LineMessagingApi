namespace Line.Messaging;

/// <summary>
/// Text
/// https://developers.line.me/en/docs/messaging-api/reference/#text
/// </summary>
public class TextMessage : Message
{
    /// <summary>
    /// Message text
    /// Max: 2000 characters
    /// </summary>
    public required string Text { get; init; }
}