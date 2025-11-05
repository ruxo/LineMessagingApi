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
[JsonDerivedType(typeof(ImageEventMessage), "image")]
[JsonDerivedType(typeof(VideoEventMessage), "video")]
[JsonDerivedType(typeof(AudioEventMessage), "audio")]
[JsonDerivedType(typeof(StickerEventMessage), "sticker")]
public class EventMessage
{
    /// <summary>
    /// Message ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// EventMessageType
    /// </summary>
    [JsonIgnore]
    public EventMessageType Type { get; init; }

    public string? QuoteToken { get; init; }
    public string? MarkAsReadToken { get; init; }
}