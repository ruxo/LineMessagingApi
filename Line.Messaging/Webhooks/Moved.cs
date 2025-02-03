namespace Line.Messaging.Webhooks;

/// <summary>
/// Joined or left Members
/// </summary>
public class Moved
{
    /// <summary>
    /// Joined or left Members
    /// </summary>
    public required WebhookEventSource[] Members { get; init; }
}