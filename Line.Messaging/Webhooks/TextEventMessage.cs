namespace Line.Messaging.Webhooks;

/// <summary>
/// Message object which contains the text sent from the source.
/// </summary>
public class TextEventMessage : EventMessage
{
    public required string Text { get; init; }
    public required string QuoteToken { get; init; }

    public TextEventMessage()
    {
        Type = EventMessageType.Text;
    }
}