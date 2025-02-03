namespace Line.Messaging.Webhooks;

/// <summary>
/// Event object for when a user leaves a group or room that the bot is in.
/// </summary>
public class MemberLeaveEvent : WebhookEvent
{
    /// <summary>
    /// User ID of users who left
    /// </summary>
    public required WebhookEventSource[] Members { get; init; }

    public MemberLeaveEvent()
    {
        Type = WebhookEventType.MemberLeft;
    }
}