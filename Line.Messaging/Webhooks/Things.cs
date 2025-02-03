namespace Line.Messaging.Webhooks;

/// <summary>
/// LINE Things
/// </summary>
[PublicAPI]
public class Things(string deviceId, ThingsType type)
{
    /// <summary>
    /// Device ID of the LINE Things-compatible device that was linked with LINE
    /// </summary>
    public string DeviceId => deviceId;

    /// <summary>
    /// Link or Unlink
    /// </summary>
    public ThingsType Type => type;
}