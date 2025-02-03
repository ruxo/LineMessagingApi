using System.Text.Json.Serialization;

namespace Line.Messaging.Webhooks;

/// <summary>
/// Contents of the message
/// </summary>
[PublicAPI]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextEventMessage), "text")]
[JsonDerivedType(typeof(FileEventMessage), "file")]
[JsonDerivedType(typeof(LocationEventMessage), "location")]
public class EventMessage
{
    /// <summary>
    /// Message ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// EventMessageType
    /// </summary>
    public EventMessageType Type { get; init; }
}