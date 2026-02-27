using System.Text.Json.Serialization;

namespace Line.Messaging;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(MessageTemplateAction), TemplateActionType.Message)]
[JsonDerivedType(typeof(PostbackTemplateAction), TemplateActionType.Postback)]
[JsonDerivedType(typeof(UriTemplateAction), TemplateActionType.Uri)]
[JsonDerivedType(typeof(DateTimePickerTemplateAction), TemplateActionType.DatetimePicker)]
[JsonDerivedType(typeof(LocationTemplateAction), TemplateActionType.Location)]
[JsonDerivedType(typeof(CameraTemplateAction), TemplateActionType.Camera)]
[JsonDerivedType(typeof(CameraRollTemplateAction), TemplateActionType.CameraRoll)]
public abstract class ITemplateAction(string type)
{
    [JsonIgnore] public string Type => type;
}