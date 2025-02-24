﻿namespace Line.Messaging.Webhooks;

/// <summary>
/// Message object which contains the file sent from the source. The binary data can be retrieved from the content endpoint.
/// </summary>
public class FileEventMessage : EventMessage
{
    /// <summary>
    /// file name
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; init; }

    public FileEventMessage()
    {
        Type = EventMessageType.File;
    }
}