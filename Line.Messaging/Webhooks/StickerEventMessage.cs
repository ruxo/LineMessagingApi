namespace Line.Messaging.Webhooks;

/// <summary>
/// Message object which contains the sticker data sent from the source. For a list of basic LINE stickers and sticker IDs, see sticker list.
/// </summary>
public class StickerEventMessage : EventMessage
{
    public required string PackageId { get; init; }

    public required string StickerId { get; init; }

    public required string StickerResourceType { get; init; }

    public required string[] Keywords { get; init; }

    public StickerEventMessage()
    {
        Type = EventMessageType.Sticker;
    }
}