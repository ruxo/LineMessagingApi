namespace Line.Messaging.Webhooks;

/// <summary>
/// Message object which contains the location data sent from the source.
/// </summary>
public class LocationEventMessage : EventMessage
{
    /// <summary>
    /// Title
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Address
    /// </summary>
    public required string Address { get; init; }

    /// <summary>
    /// Latitude
    /// </summary>
    public decimal Latitude { get; init; }

    /// <summary>
    /// Longitude
    /// </summary>
    public decimal Longitude { get; init; }

    public LocationEventMessage()
    {
        Type = EventMessageType.Location;
    }
}