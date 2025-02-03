namespace Line.Messaging.Webhooks;

/// <summary>
/// Webhook Event Source. Source could be User, Group or Room.
/// </summary>
[PublicAPI]
public class WebhookEventSource
{
    public EventSourceType Type { get; init; }

    /// <summary>
    /// UserId of the Group or Room
    /// </summary>
    public string? UserId { get; init; }
}