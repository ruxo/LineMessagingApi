namespace Line.Messaging.Webhooks;

/// <summary>
/// Media event message (Image, Video or Audio)
/// </summary>
public class MediaEventMessage : EventMessage
{
    public required ContentProvider ContentProvider { get; init; }
}

public class ImageEventMessage : MediaEventMessage
{
    public ImageEventMessage()
    {
        Type = EventMessageType.Image;
    }
}

public class VideoEventMessage : MediaEventMessage
{
    public int Duration { get; init; }

    public VideoEventMessage()
    {
        Type = EventMessageType.Video;
    }
}

public class AudioEventMessage : MediaEventMessage
{
    public int Duration { get; init; }

    public AudioEventMessage()
    {
        Type = EventMessageType.Audio;
    }
}