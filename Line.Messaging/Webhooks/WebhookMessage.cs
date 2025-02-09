using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using RZ.Foundation.Types;

namespace Line.Messaging.Webhooks;

[PublicAPI]
public class WebhookMessage
{
    public required string Destination { get; init; }
    public required WebhookEvent[] Events { get; init; }

    [JsonExtensionData] public IDictionary<string, JsonElement>? ExtraData { get; init; }

    public static Outcome<WebhookMessage> TryParse(string jsonText) {
        var (error, message) = Try(jsonText, s => JsonSerializer.Deserialize<WebhookMessage>(s, LineJson.Options));
        return error is null ? message! : ErrorFrom.Exception(error);
    }

    public static readonly IValidator<WebhookMessage> Validator = new ValidatorType();

    public class ValidatorType : AbstractValidator<WebhookMessage>
    {
        public ValidatorType() {
            RuleFor(x => x.Destination).NotEmpty();
            RuleFor(x => x.Events).NotNull();
            RuleForEach(x => x.Events).SetValidator(WebhookEvent.Validator);
        }
    }
}