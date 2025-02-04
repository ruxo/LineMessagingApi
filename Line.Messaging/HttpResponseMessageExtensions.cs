using System.Net.Http.Json;
using System.Text.Json;

namespace Line.Messaging;

static class HttpResponseMessageExtensions
{
    internal static async Task<HttpResponseMessage> EnsureSuccessStatusCodeAsync(this Task<HttpResponseMessage> task) {
        var response = await task.ConfigureAwait(false);
        return await response.EnsureSuccessStatusCodeAsync();
    }

    public static async Task<T> GetLineJsonAsync<T>(this HttpClient client, string requestUri, CancellationToken cancelToken = default) {
        var response = await client.GetAsync(requestUri, cancelToken).EnsureSuccessStatusCodeAsync();
        return (await response.Content.ReadFromJsonAsync<T>(LineJson.Options, cancellationToken: cancelToken))!;
    }

    public static async Task<T> GetLineJsonAsync<T>(this Task<HttpResponseMessage> task, CancellationToken cancelToken = default) {
        var response = await task.EnsureSuccessStatusCodeAsync();
        return (await response.Content.ReadFromJsonAsync<T>(LineJson.Options, cancellationToken: cancelToken))!;
    }

    /// <summary>
    /// Validate the response status.
    /// </summary>
    /// <param name="response">HttpResponseMessage</param>
    /// <returns>HttpResponseMessage</returns>
    internal static async ValueTask<HttpResponseMessage> EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
    {
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
}