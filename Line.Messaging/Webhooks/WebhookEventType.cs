using System.Text.Json.Serialization;

namespace Line.Messaging.Webhooks;

/// <summary>
/// Webhook Event Type
/// </summary>
public enum WebhookEventType
{
    [JsonStringEnumMemberName("message")] Message,
    [JsonStringEnumMemberName("follow")] Follow,
    [JsonStringEnumMemberName("unfollow")] Unfollow,
    [JsonStringEnumMemberName("join")] Join,
    [JsonStringEnumMemberName("leave")] Leave,
    [JsonStringEnumMemberName("postback")] Postback,
    [JsonStringEnumMemberName("beacon")] Beacon,
    [JsonStringEnumMemberName("accountLink")] AccountLink,
    [JsonStringEnumMemberName("memberJoined")] MemberJoined,
    [JsonStringEnumMemberName("memberLeft")] MemberLeft,
    [JsonStringEnumMemberName("things")] Things,
}