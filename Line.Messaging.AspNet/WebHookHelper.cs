using JetBrains.Annotations;
using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Http;
using RZ.Foundation.Types;

namespace Line.Messaging.AspNet;

[PublicAPI]
public static class WebHookHelper
{
    public static async Task<Outcome<WebhookMessage>> GetWebhookEvents(this HttpRequest request, string channelSecret, string? botUserId = null) {
        using var reader = new StreamReader(request.Body);
        var content = await reader.ReadToEndAsync();

        var xLineSignature = request.Headers.Get("X-Line-Signature").ToNullable();
        if (string.IsNullOrEmpty(xLineSignature) || !WebhookRequestMessageHelper.VerifySignature(channelSecret, xLineSignature!, content))
            return new ErrorInfo(StandardErrorCodes.InvalidRequest, "Signature validation failed");

        var result = WebhookMessage.TryParse(content);

        if (result.IfSuccess(out var @event, out _) && !string.IsNullOrEmpty(botUserId) && botUserId != @event.Destination)
            return new ErrorInfo("user-mismatch", "Bot user ID does not match.", data: @event.Destination);

        return result;
    }
}
