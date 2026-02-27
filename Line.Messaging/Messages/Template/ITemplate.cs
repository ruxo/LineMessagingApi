using System.Text.Json.Serialization;

namespace Line.Messaging;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ButtonsTemplate), TemplateType.Buttons)]
[JsonDerivedType(typeof(CarouselTemplate), TemplateType.Carousel)]
[JsonDerivedType(typeof(ConfirmTemplate), TemplateType.Confirm)]
[JsonDerivedType(typeof(ImageCarouselTemplate), TemplateType.ImageCarousel)]
public abstract class ITemplate(string type)
{
    [JsonIgnore]
    public string Type => type;
}