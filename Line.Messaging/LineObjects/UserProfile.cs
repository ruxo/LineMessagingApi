namespace Line.Messaging;

/// <summary>
/// Response from Get User Profile API.
/// https://developers.line.me/en/docs/messaging-api/reference/#get-profile
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Display name
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Image URL
    /// </summary>
    public string? PictureUrl { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Language tag as suggested by BCP-47
    /// </summary>
    public string? Language { get; set; }
}