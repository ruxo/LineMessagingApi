namespace Line.Messaging.Webhooks;

/// <summary>
/// Media event message (Image, Video or Audio)
/// </summary>
public class MediaEventMessage : EventMessage
{
    /// <summary>
    /// ContentProvider
    /// </summary>
    public required ContentProvider ContentProvider { get; init; }

    /// <summary>
    /// Length of media file (milliseconds)
    /// </summary>
    public int? Duration { get; init; }
}