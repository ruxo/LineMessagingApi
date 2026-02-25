using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Line.Messaging;

static class HttpResponseMessageExtensions
{
    internal static async Task<HttpResponseMessage> EnsureSuccessStatusCodeAsync(this Task<HttpResponseMessage> task) {
        var response = await task.ConfigureAwait(false);
        return await response.EnsureSuccessStatusCodeAsync();
    }

    public static async ValueTask<Outcome<T>> GetLineJsonAsync<T>(this HttpClient client, string requestUri, CancellationToken cancelToken = default) {
        if (Fail(await client.Get(requestUri, cancelToken), out var e, out var response)) return e.Trace();
        using (response)
            if (Fail(await response.DeserializedJson<T>(LineJson.Options), out e, out var result)) return e.Trace();
            else return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Outcome<T>> GetLineJsonAsync<T>(this ValueTask<Outcome<HttpResponseMessage>> task)
        => task.DeserializedJson<T>(LineJson.Options);

    /// <summary>
    /// Validate the response status.
    /// </summary>
    /// <param name="response">HttpResponseMessage</param>
    /// <returns>HttpResponseMessage</returns>
    internal static async ValueTask<HttpResponseMessage> EnsureSuccessStatusCodeAsync(this HttpResponseMessage response) {
        if (response.IsSuccessStatusCode)
            return response;
        else{
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var (error, errorMessage) = Try(() => JsonSerializer.Deserialize<ErrorResponseMessage>(content, LineJson.Options)!);
            if (error is not null)
                errorMessage = new ErrorResponseMessage { Message = content, Details = [] };

            throw new LineResponseException(errorMessage.Message) { StatusCode = response.StatusCode, ResponseMessage = errorMessage };
        }
    }

    internal static async ValueTask<Outcome<LanguageExt.Unit>> CheckSucceed(this HttpResponseMessage r, JsonSerializerOptions? options = null) {
        using (r)
            return r.IsSuccessStatusCode
                       ? unit
                       : ExtractError<LanguageExt.Unit>(Success(await r.Content.ReadAsString().ConfigureAwait(false), out var body) ? body : string.Empty, options);
    }

    internal static ValueTask<Outcome<LanguageExt.Unit>> CheckSucceed(this ValueTask<Outcome<HttpResponseMessage>> r, JsonSerializerOptions? options = null)
        => from response in r
           from _ in response.CheckSucceed(options)
           select unit;

    static Outcome<T> ExtractError<T>(string body, JsonSerializerOptions? options) {
        if (Success(JsonDeserialize<ErrorInfo>(body, options), out var errorInfo))
            if (!string.IsNullOrEmpty(errorInfo.Code))
                return errorInfo.Trace("From HTTP response");
        return new ErrorInfo(HttpError, data: body);
    }
}