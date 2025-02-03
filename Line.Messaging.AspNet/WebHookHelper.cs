using JetBrains.Annotations;
using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Http;

namespace Line.Messaging.AspNet;

[PublicAPI]
public static class WebHookHelper
{
    public static async Task<Outcome<WebhookMessage>> GetWebhookMessage(this HttpRequest request, string channelSecret, string? botUserId = null) {
        using var reader = new StreamReader(request.Body);
        var content = await reader.ReadToEndAsync();

        var xLineSignature = request.Headers.Get("X-Line-Signature").ToNullable();

        return WebhookRequestMessageHelper.GetWebhookMessage(channelSecret, xLineSignature, content, botUserId);
    }
}
