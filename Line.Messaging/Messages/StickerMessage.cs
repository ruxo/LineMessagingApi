﻿namespace Line.Messaging;

/// <summary>
/// Sticker. For a list of the sticker IDs for stickers that can be sent with the Messaging API, see Sticker list.
/// </summary>
[PublicAPI]
public class StickerMessage : Message
{
    /// <summary>
    /// Package ID for a set of stickers. For information on package IDs, see the Sticker list.
    /// </summary>
    public string PackageId { get; }

    /// <summary>
    /// Sticker ID. For a list of sticker IDs for stickers that can be sent with the Messaging API, see the Sticker list.
    /// </summary>
    public string StickerId { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="packageId">
    /// Package ID for a set of stickers. For information on package IDs, see the Sticker list.
    /// </param>
    /// <param name="stickerId">
    /// Sticker ID. For a list of sticker IDs for stickers that can be sent with the Messaging API, see the Sticker list.
    /// </param>
    /// <param name="quickReply">
    /// QuickReply
    /// </param>
    public StickerMessage(string packageId, string stickerId, QuickReply? quickReply = null) {
        Type = MessageType.Sticker;
        PackageId = packageId;
        StickerId = stickerId;
        QuickReply = quickReply;
    }
}