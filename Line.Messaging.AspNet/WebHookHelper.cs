using System.Text.Json;
using JetBrains.Annotations;
using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Http;
using RZ.Foundation.Types;
using LanguageExt;

namespace Line.Messaging.AspNet;

public static class WebHookHelper
{
    /// <param name="request"></param>
    extension(HttpRequest request)
    {
        [PublicAPI]
        public string? GetLineSignature()
            => request.Headers.TryGetValue("X-Line-Signature").ToNullable();

        /// <summary>
        /// Try to get the LINE webhook message from the request.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [PublicAPI]
        public async Task<Outcome<(string Signature, WebhookMessage Message)>> TryGetWebhookMessage(string? body = null) {
            if (request.GetLineSignature() is not { } signature)
                return new ErrorInfo(StandardErrorCodes.InvalidRequest, "Missing LINE signature");

            if (body is null && Fail(await request.ReadBody(), out var error, out body)) return error;

            if (WebhookMessage.TryParse(body).IfSuccess(out var message, out error)){
                if (WebhookMessage.Validator.Validate(message).Errors.FirstOrDefault() is { } validationError)
                    return new ErrorInfo(StandardErrorCodes.InvalidRequest, $"LINE message error: {validationError}", data: JsonSerializer.Serialize(validationError));

                return (signature, message);
            }
            else
                return error;
        }

        [PublicAPI]
        public async ValueTask<Outcome<string>> ReadBody() {
            using var reader = new StreamReader(request.Body);
            try{
                return await reader.ReadToEndAsync();
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }
    }
}
