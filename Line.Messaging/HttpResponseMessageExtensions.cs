using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Line.Messaging;

static class HttpResponseMessageExtensions
{
    public static async ValueTask<Outcome<T>> GetLineJsonAsync<T>(this HttpClient client, string requestUri, CancellationToken cancelToken = default) {
        if (Fail(await client.Get(requestUri, cancelToken), out var e, out var response)) return e.Trace();
        using (response)
            if (Fail(await response.DeserializedJson<T>(LineJson.Options), out e, out var result)) return e.Trace();
            else return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Outcome<T>> GetLineJsonAsync<T>(this ValueTask<Outcome<HttpResponseMessage>> task)
        => task.DeserializedJson<T>(LineJson.Options);

    internal static async ValueTask<Outcome<LanguageExt.Unit>> CheckSucceed(this HttpResponseMessage r, JsonSerializerOptions? options = null) {
        using (r){
            if (r.IsSuccessStatusCode) return unit;

            var body = Success(await r.Content.ReadAsString().ConfigureAwait(false), out var b) ? b : string.Empty;
            // An earlier attempt with the same retry key already delivered this request.
            if (LineApiErrors.IsRetryKeyAccepted(r.StatusCode, body)) return unit;

            return ExtractError<LanguageExt.Unit>(r.StatusCode, r.ReasonPhrase, body, options);
        }
    }

    internal static ValueTask<Outcome<LanguageExt.Unit>> CheckSucceed(this ValueTask<Outcome<HttpResponseMessage>> r, JsonSerializerOptions? options = null)
        => from response in r
           from _ in response.CheckSucceed(options)
           select unit;

    static Outcome<T> ExtractError<T>(HttpStatusCode status, string? reasonPhrase, string body, JsonSerializerOptions? options) {
        if (Success(JsonDeserialize<ErrorInfo>(body, options), out var errorInfo))
            if (!string.IsNullOrEmpty(errorInfo.Code))
                return errorInfo.Trace("From HTTP response");
        return LineApiErrors.ToError(status, reasonPhrase, body);
    }
}