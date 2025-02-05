namespace Line.Messaging.Webhooks;

public readonly record struct Emoji(int Index, int Length, string? ProductId, string? EmojiId);

/// <summary>
/// Message object which contains the text sent from the source.
/// </summary>
public class TextEventMessage : EventMessage
{
    public required string Text { get; init; }
    public Emoji[]? Emojis { get; init; }

    public TextEventMessage()
    {
        Type = EventMessageType.Text;
    }
}