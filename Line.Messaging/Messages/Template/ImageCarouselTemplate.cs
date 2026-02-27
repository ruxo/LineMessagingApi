namespace Line.Messaging;

/// <summary>
/// Template with multiple images which can be cycled like a carousel.
/// https://developers.line.me/en/docs/messaging-api/reference/#image-carousel
/// </summary>
[PublicAPI]
public class ImageCarouselTemplate(IReadOnlyList<ImageCarouselColumn> columns) : ITemplate(TemplateType.ImageCarousel)
{
    /// <summary>
    /// Array of columns
    /// Max: 10
    /// </summary>
    public IReadOnlyList<ImageCarouselColumn> Columns => columns;
}