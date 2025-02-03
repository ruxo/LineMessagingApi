namespace Line.Messaging.Webhooks;

/// <summary>
/// Link Object
/// </summary>
public class Link
{
    /// <summary>
    /// One of the following values to indicate whether the link was successful or not.
    /// <list type="bullet">
    /// <item>
    /// <description>ok: Indicates the link was successful.</description>
    /// </item>
    /// <item>
    /// <description>failed: Indicates the link failed for any reason, such as due to a user impersonation.</description>
    /// </item>
    /// </list>
    /// </summary>
    public LinkResult Result { get; init; }

    /// <summary>
    /// Specified nonce when verifying the user ID
    /// <para>https://developers.line.me/en/docs/messaging-api/linking-accounts#verifying-user-id</para>
    /// </summary>
    public required string Nonce { get; init; }
}