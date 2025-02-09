namespace Line.Messaging;

/// <summary>
/// Error returned from LINE Platform
/// https://developers.line.me/en/docs/messaging-api/reference/#error-responses
/// </summary>
public class ErrorResponseMessage
{
    /// <summary>
    /// Summary of the error
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Details of the error
    /// </summary>
    public required IReadOnlyList<ErrorDetails> Details { get; init; }

    public override string ToString()
        => Details.Count > 0? $"{Message},[{string.Join(",", Details)}]" : Message;

    /// <summary>
    /// Details of the error
    /// </summary>
    /// <param name="Message">Details of the error</param>
    /// <param name="Property">Location of where the error occured</param>
    public record ErrorDetails(string Message, string Property)
    {
        public override string ToString()
            => $"{{{Message}, {Property}}}";
    }
}