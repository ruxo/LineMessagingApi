namespace Line.Messaging.Webhooks;

/// <summary>
/// Event object for when a user performs an action on a template message which initiates a postback. You can reply to postback events.
/// </summary>
public class PostbackEvent : ReplyableEvent
{
    /// <summary>
    /// Postback
    /// </summary>
    public required Postback Postback { get; init; }

    public PostbackEvent()
    {
        Type = WebhookEventType.Postback;
    }
}