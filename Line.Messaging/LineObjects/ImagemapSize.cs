namespace Line.Messaging;

/// <summary>
/// Image size.
/// </summary>
public class ImagemapSize(int width, int height)
{
    /// <summary>
    /// Default rich menu size
    /// </summary>
    public static ImagemapSize RichMenuLong { get; } = new ImagemapSize(2500, 1686);

    /// <summary>
    /// Half rich menu size.
    /// </summary>
    public static ImagemapSize RichMenuShort { get; } = new ImagemapSize(2500, 843);

    /// <summary>
    /// Width
    /// </summary>
    public int Width { get; } = width;

    /// <summary>
    /// Height
    /// </summary>
    public int Height { get; } = height;
}