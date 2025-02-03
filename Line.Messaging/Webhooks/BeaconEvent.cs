namespace Line.Messaging.Webhooks;

/// <summary>
/// Event object for when a user enters or leaves the range of a LINE Beacon. You can reply to beacon events.
/// https://developers.line.me/en/docs/messaging-api/reference/#beacon-event
/// </summary>
public class BeaconEvent : ReplyableEvent
{
    public required string Hwid { get; init; }
    public BeaconType BeaconType { get; init; }
    public required string Dm { get; init; }

    public BeaconEvent()
    {
        Type = WebhookEventType.Beacon;
    }
}