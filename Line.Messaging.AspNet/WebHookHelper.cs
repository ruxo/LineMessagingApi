using JetBrains.Annotations;
using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Http;
using RZ.Foundation.Types;

namespace Line.Messaging.AspNet;

[PublicAPI]
public static class WebHookHelper
{
    public static string? GetLineSignature(this HttpRequest request)
        => request.Headers.Get("X-Line-Signature").ToNullable();

    /// <summary>
    /// Try to get the LINE webhook message from the request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="body"></param>
    /// <param name="channelSecret"></param>
    /// <returns></returns>
    public static async Task<Outcome<(string Signature, WebhookMessage Message)>> TryGetWebhookMessage(this HttpRequest request, string? body = null) {
        if (request.GetLineSignature() is not { } signature)
            return new ErrorInfo(StandardErrorCodes.InvalidRequest, "Missing LINE signature");

        body ??= await request.ReadBody();

        if (WebhookMessage.TryParse(body).IfSuccess(out var message, out var error)){
            if (WebhookMessage.Validator.Validate(message).Errors.FirstOrDefault() is { } validationError)
                return new ErrorInfo(StandardErrorCodes.InvalidRequest, $"LINE message error: {validationError}", data: validationError);

            return (signature, message);
        }
        else
            return error;
    }

    public static async Task<string> ReadBody(this HttpRequest request) {
        using var reader = new StreamReader(request.Body);
        return await reader.ReadToEndAsync();
    }
}
