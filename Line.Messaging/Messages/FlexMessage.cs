namespace Line.Messaging;

/// <summary>
/// Template messages are messages with predefined layouts which you can customize. There are four types of templates available that can be used to interact with users through your bot.
/// </summary>
[PublicAPI]
public class FlexMessage : Message
{
    /// <summary>
    /// Flex Message container object
    /// </summary>
    public IFlexContainer? Contents { get; set; }

    /// <summary>
    /// Alternative text.
    /// Max: 400 characters
    /// </summary>
    public string AltText { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="altText">
    /// Alternative text.
    /// Max: 400 characters
    ///</param>
    public FlexMessage(string altText) {
        Type = MessageType.Flex;
        AltText = altText.Substring(0, Math.Min(altText.Length, 400));
    }

    public static BubbleContainerFlexMessage CreateBubbleMessage(string altText) => new(altText)
    {
        Contents = new BubbleContainer()
    };

    public static CarouselContainerFlexMessage CreateCarouselMessage(string altText) => new(altText)
    {
        Contents = new CarouselContainer()
    };
}