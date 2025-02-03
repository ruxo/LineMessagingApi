namespace Line.Messaging.Webhooks;

/// <summary>
/// Event object for when your account is added as a friend (or unblocked). You can reply to follow events.
/// </summary>
public class FollowEvent : ReplyableEvent
{
    public required FollowInfo Follow { get; init; }

    public FollowEvent()
    {
        Type = WebhookEventType.Follow;
    }

    public record FollowInfo(bool IsUnblocked);
}