using FluentAssertions;
using JetBrains.Annotations;
using Line.Messaging.Webhooks;

namespace UnitTests;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class WebHookEventTests
{
    [Fact]
    public void DeserializingFollowEvent()
    {
        const string Json = """{"destination":"U12e8145586fe53f6e23ca7b9b2a1a9e3","events":[{"type":"follow","follow":{"isUnblocked":true},"webhookEventId":"01JJZQWE29F3N6RRHHR3YEPS72","deliveryContext":{"isRedelivery":false},"timestamp":1738379507273,"source":{"type":"user","userId":"U7890278f603144376eab736cb6b2b2ec"},"replyToken":"c2e6c120a3be4024b69b97b577a8e574","mode":"active"}]}""";

        var result = WebhookMessage.TryParse(Json);

        result.IsSuccess.Should().BeTrue($"but {result}");
        result.Data.Should().BeEquivalentTo(new WebhookMessage {
            Destination = "U12e8145586fe53f6e23ca7b9b2a1a9e3",
            Events = [
                new FollowEvent{
                    WebhookEventId = "01JJZQWE29F3N6RRHHR3YEPS72",
                    Follow = new(IsUnblocked: true),
                    DeliveryContext = new DeliveryContext(false),
                    Source = new WebhookEventSource {
                        Type = EventSourceType.User,
                        UserId = "U7890278f603144376eab736cb6b2b2ec"
                    },
                    Mode = "active",
                    ReplyToken = "c2e6c120a3be4024b69b97b577a8e574",
                    Timestamp = 1738379507273
                }
            ]
        });
    }

    [Fact]
    public void DeserializingUnfollowEvent() {
        const string Json = """{"destination":"U12e8145586fe53f6e23ca7b9b2a1a9e3","events":[{"type":"unfollow","webhookEventId":"01JJZQVH9GM0A9W31355745H72","deliveryContext":{"isRedelivery":false},"timestamp":1738379477856,"source":{"type":"user","userId":"U7890278f603144376eab736cb6b2b2ec"},"mode":"active"}]}""";

        var result = WebhookMessage.TryParse(Json);

        result.IsSuccess.Should().BeTrue($"but {result}");
        result.Data.Should().BeEquivalentTo(new WebhookMessage {
            Destination = "U12e8145586fe53f6e23ca7b9b2a1a9e3",
            Events = [
                new UnfollowEvent {
                    WebhookEventId = "01JJZQVH9GM0A9W31355745H72",
                    DeliveryContext = new DeliveryContext(false),
                    Source = new WebhookEventSource {
                        Type = EventSourceType.User,
                        UserId = "U7890278f603144376eab736cb6b2b2ec"
                    },
                    Mode = "active",
                    Timestamp = 1738379477856
                }
            ]
        });
    }

    [Fact]
    public void DeserializingMessageEvent() {
        const string Json = """{"destination":"U12e8145586fe53f6e23ca7b9b2a1a9e3","events":[{"type":"message","message":{"type":"text","id":"546179081273082502","quoteToken":"xAf2YfCPQMaPw3ISiq7XsHF_KfZV7X9TPz5rPIJYmACIY99UC3i9P-dRga5M4x4WeD6XEuq8I32TpvDOI1DfodQYP6rQhG44GVJ2zBKjO2A793AfB4mlZFTalpxkWTjrBlB05lpq_AaKzZ9cFYCb6Q","text":"Hello"},"webhookEventId":"01JJZQSA83SBT6T0TXANW77R7K","deliveryContext":{"isRedelivery":false},"timestamp":1738379405363,"source":{"type":"user","userId":"U7890278f603144376eab736cb6b2b2ec"},"replyToken":"f9b293f9630d4a349a9e8296c6588706","mode":"active"}]}""";

        var result = WebhookMessage.TryParse(Json);

        result.IsSuccess.Should().BeTrue($"but {result}");
        result.Data.Should().BeEquivalentTo(new WebhookMessage {
            Destination = "U12e8145586fe53f6e23ca7b9b2a1a9e3",
            Events = [
                new MessageEvent {
                    WebhookEventId = "01JJZQSA83SBT6T0TXANW77R7K",
                    Message = new TextEventMessage {
                        Id = "546179081273082502",
                        QuoteToken = "xAf2YfCPQMaPw3ISiq7XsHF_KfZV7X9TPz5rPIJYmACIY99UC3i9P-dRga5M4x4WeD6XEuq8I32TpvDOI1DfodQYP6rQhG44GVJ2zBKjO2A793AfB4mlZFTalpxkWTjrBlB05lpq_AaKzZ9cFYCb6Q",
                        Text = "Hello"
                    },
                    DeliveryContext = new DeliveryContext(false),
                    Source = new WebhookEventSource {
                        Type = EventSourceType.User,
                        UserId = "U7890278f603144376eab736cb6b2b2ec"
                    },
                    ReplyToken = "f9b293f9630d4a349a9e8296c6588706",
                    Mode = "active",
                    Timestamp = 1738379405363,
                }
            ]
        });
    }

    [Fact]
    public void DeserializeGroupJoin() {
        const string Json = """{"destination":"Uc62c92bf76c1afb8e42d65727e9c00e2","events":[{"type":"join","webhookEventId":"01JRRYMPCWBT8KGGR4K1X8RGN4","deliveryContext":{"isRedelivery":false},"timestamp":1744594163857,"source":{"type":"group","groupId":"Cc2efce7be8f6a1a0409bbf13db3fa56a"},"replyToken":"537f4cde9c394cff99a37cfa004eb967","mode":"active"}]}""";

        var result = WebhookMessage.TryParse(Json);

        result.IsSuccess.Should().BeTrue($"but {result}");
        result.Data.Should().BeEquivalentTo(new WebhookMessage {
            Destination = "Uc62c92bf76c1afb8e42d65727e9c00e2",
            Events = [
                new JoinEvent {
                    WebhookEventId = "01JRRYMPCWBT8KGGR4K1X8RGN4",
                    DeliveryContext = new DeliveryContext(false),
                    Source = new GroupEventSource {
                        Type = EventSourceType.Group,
                        GroupId = "Cc2efce7be8f6a1a0409bbf13db3fa56a"
                    },
                    ReplyToken = "537f4cde9c394cff99a37cfa004eb967",
                    Mode = "active",
                    Timestamp = 1744594163857
                }
            ]
        });
    }
}