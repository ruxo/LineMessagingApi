namespace Line.Messaging.Webhooks;

[PublicAPI]
public abstract class ReplyableEvent : WebhookEvent
{
    public required string ReplyToken { get; init; }
}