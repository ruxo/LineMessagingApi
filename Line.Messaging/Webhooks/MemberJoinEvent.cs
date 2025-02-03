namespace Line.Messaging.Webhooks;

/// <summary>
/// Event object for when a user joins a group or room that the bot is in.
/// </summary>
public class MemberJoinEvent : ReplyableEvent
{
    /// <summary>
    /// User ID of users who joined
    /// </summary>
    public required WebhookEventSource[] Members { get; init; }

    public MemberJoinEvent()
    {
        Type = WebhookEventType.MemberJoined;
    }
}