﻿using System.Text.Json;
using System.Text.Json.Serialization;
using RZ.Foundation.Types;

namespace Line.Messaging.Webhooks;

/// <summary>
/// The webhook event generated on the LINE Platform.
/// https://developers.line.me/en/docs/messaging-api/reference/#webhook-event-objects
/// </summary>
[PublicAPI]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AccountLinkEvent), "accountLink")]
[JsonDerivedType(typeof(BeaconEvent), "beacon")]
[JsonDerivedType(typeof(DeviceEvent), "things")]
[JsonDerivedType(typeof(FollowEvent), "follow")]
[JsonDerivedType(typeof(JoinEvent), "join")]
[JsonDerivedType(typeof(LeaveEvent), "leave")]
[JsonDerivedType(typeof(MemberJoinEvent), "memberJoined")]
[JsonDerivedType(typeof(MemberLeaveEvent), "memberLeft")]
[JsonDerivedType(typeof(MessageEvent), "message")]
[JsonDerivedType(typeof(PostbackEvent), "postback")]
[JsonDerivedType(typeof(UnfollowEvent), "unfollow")]
public abstract class WebhookEvent
{
    public required string WebhookEventId { get; init; }

    /// <summary>
    /// Identifier for the type of event
    /// </summary>
    public WebhookEventType Type { get; init; }

    /// <summary>
    /// JSON object which contains the source of the event
    /// </summary>
    public required WebhookEventSource Source { get; init; }

    public required string Mode { get; init; }

    public required DeliveryContext DeliveryContext { get; init; }

    /// <summary>
    /// Time of the event in milliseconds
    /// </summary>
    public long Timestamp { get; init; }

    public static Outcome<WebhookEvent> CreateFrom(string jsonText) {
        var (error, @event) = Try(jsonText, s => JsonSerializer.Deserialize<WebhookEvent>(s, LineJson.Options));
        return error is null ? @event! : ErrorFrom.Exception(error);
    }
}

public record DeliveryContext(bool IsRedelivery);