using System.Text.Json.Serialization;
using FluentValidation;

namespace Line.Messaging.Webhooks;

/// <summary>
/// Webhook Event Source. Source could be User, Group or Room.
/// </summary>
[PublicAPI]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(UserEventSource), "user")]
[JsonDerivedType(typeof(GroupEventSource), "group")]
[JsonDerivedType(typeof(RoomEventSource), "room")]
public class WebhookEventSource
{
    public EventSourceType Type { get; init; }

    /// <summary>
    /// UserId of the Group or Room
    /// </summary>
    public string? UserId { get; init; }

    public static readonly IValidator<WebhookEventSource> Validator = new ValidatorType();

    public class ValidatorType : AbstractValidator<WebhookEventSource>
    {
        public ValidatorType()
        {
            RuleFor(x => x.Type).IsInEnum();
        }
    }
}

public class UserEventSource : WebhookEventSource
{
    public UserEventSource() {
        Type = EventSourceType.User;
    }
}

public class GroupEventSource : WebhookEventSource
{
    public GroupEventSource() {
        Type = EventSourceType.Group;
    }

    public required string GroupId { get; init; }
}

public class RoomEventSource : WebhookEventSource
{
    public RoomEventSource() {
        Type = EventSourceType.Room;
    }

    public string? RoomId { get; init; }
}