namespace Line.Messaging.Webhooks;

/// <summary>
/// Event object for when your account is blocked.
/// </summary>
public class UnfollowEvent : WebhookEvent
{
    public UnfollowEvent()
    {
        Type = WebhookEventType.Unfollow;
    }
}