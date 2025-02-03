using System.Text.Json.Serialization;

namespace Line.Messaging.Webhooks;

/// <summary>
/// Webhook Event Source Type.
/// </summary>
[PublicAPI]
public enum EventSourceType
{
    [JsonStringEnumMemberName("user")] User,
    [JsonStringEnumMemberName("group")] Group,
    [JsonStringEnumMemberName("room")] Room
}