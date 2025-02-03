namespace Line.Messaging;

/// <summary>
/// Text
/// https://developers.line.me/en/docs/messaging-api/reference/#text
/// </summary>
public class TextMessage : ISendMessage
{
    public MessageType Type { get; init; } = MessageType.Text;

    /// <summary>
    /// These properties are used for the quick reply feature
    /// </summary>
    public QuickReply? QuickReply { get; init; }

    /// <summary>
    /// Message text
    /// Max: 2000 characters
    /// </summary>
    public required string Text { get; init; }
}