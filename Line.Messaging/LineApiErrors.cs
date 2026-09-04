using System.Net;

namespace Line.Messaging;

/// <summary>
/// What LINE's failure responses mean, stated once so a caller and any HTTP pipeline in front of the client
/// apply the same reading. LINE uses 429 for two unrelated conditions: per-second rate limiting, which clears
/// on its own, and the channel's monthly message quota, which does not. Only the response body tells them
/// apart. See pace6/cosy#92.
/// </summary>
[PublicAPI]
public static class LineApiErrors
{
    /// <summary>The channel has used its monthly message quota. Retrying is pointless until the quota resets.</summary>
    public const string QUOTA_EXCEEDED = "line-quota-exceeded";

    /// <summary>A 429 that is not the monthly quota: LINE's token-bucket rate limit, worth retrying after a wait.</summary>
    public const string RATE_LIMITED = "line-rate-limited";

    /// <summary>
    /// LINE's idempotency header for push, multicast, narrowcast and broadcast: a UUID the caller generates, so a
    /// re-sent request executes once. A later copy is answered with 409 and <see cref="IsRetryKeyAccepted"/>.
    /// </summary>
    public const string RETRY_KEY_HEADER = "X-Line-Retry-Key";

    const string MONTHLY_LIMIT_SIGNATURE = "monthly limit";
    const string RETRY_KEY_ACCEPTED_SIGNATURE = "retry key is already accepted";

    public static bool IsMonthlyQuota(HttpStatusCode status, string body)
        => status == HttpStatusCode.TooManyRequests && body.Contains(MONTHLY_LIMIT_SIGNATURE, StringComparison.OrdinalIgnoreCase);

    public static bool IsRateLimit(HttpStatusCode status, string body)
        => status == HttpStatusCode.TooManyRequests && !IsMonthlyQuota(status, body);

    /// <summary>The request was already executed by an earlier attempt carrying the same retry key.</summary>
    public static bool IsRetryKeyAccepted(HttpStatusCode status, string body)
        => status == HttpStatusCode.Conflict && body.Contains(RETRY_KEY_ACCEPTED_SIGNATURE, StringComparison.OrdinalIgnoreCase);

    /// <summary>The failure for an unsuccessful LINE response: a LINE-specific code where one applies, else the standard HTTP error carrying the status and body.</summary>
    public static ErrorInfo ToError(HttpStatusCode status, string? reasonPhrase, string body)
        => IsMonthlyQuota(status, body) ? new ErrorInfo(QUOTA_EXCEEDED, "LINE monthly message quota exhausted", data: body)
         : IsRateLimit(status, body)   ? new ErrorInfo(RATE_LIMITED, "LINE rate limit exceeded", data: body)
         : HttpExtension.HttpError(status, reasonPhrase, body);
}
