using System.Text.Json.Serialization;

namespace Line.Messaging;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AudioMessage), "audio")]
[JsonDerivedType(typeof(FlexMessage), "flex")]
[JsonDerivedType(typeof(ImageMessage), "image")]
[JsonDerivedType(typeof(ImagemapMessage), "imagemap")]
[JsonDerivedType(typeof(LocationMessage), "location")]
[JsonDerivedType(typeof(StickerMessage), "sticker")]
[JsonDerivedType(typeof(TemplateMessage), "template")]
[JsonDerivedType(typeof(TextMessage), "text")]
[JsonDerivedType(typeof(VideoMessage), "video")]
public abstract class Message
{
    [JsonIgnore]
    public MessageType Type { get; init; }

    /// <summary>
    /// These properties are used for the quick reply feature. Supported on LINE 8.11.0 and later for iOS and Android. For more information, see Using quick replies.
    /// </summary>
    public QuickReply? QuickReply { get; set; }
}