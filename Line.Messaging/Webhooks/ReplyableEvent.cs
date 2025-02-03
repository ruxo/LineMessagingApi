namespace Line.Messaging.Webhooks;

[PublicAPI]
public abstract class ReplyableEvent : WebhookEvent
{
    public string? ReplyToken { get; init; }
}