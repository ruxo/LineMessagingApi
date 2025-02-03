namespace Line.Messaging.Webhooks;

/// <summary>
/// Event object which contains the sent message. The message field contains a message object which corresponds with the message type. You can reply to message events.
/// </summary>
public class MessageEvent : ReplyableEvent
{
    /// <summary>
    /// Contents of the message
    /// </summary>
    public required EventMessage Message { get; init; }

    public MessageEvent()
    {
        Type = WebhookEventType.Message;
    }
}