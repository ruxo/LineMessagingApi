using System.Text.Json.Serialization;

namespace Line.Messaging.Webhooks;

[PublicAPI]
public class DeviceEvent : WebhookEvent
{
    public required Things Things { get; init; }

    [JsonIgnore]
    public bool IsLink => Things.Type == ThingsType.Link;

    public DeviceEvent() {
        Type = WebhookEventType.Things;
    }
}