using System.Text.Json;
using JetBrains.Annotations;
using Line.Messaging;

namespace UnitTests;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ModelSerializationTests
{
    [Test]
    public async ValueTask SerializePolymorphicType() {
        Message[] messages = [new TextMessage { Text = "Hello" }];

        var result = JsonSerializer.Serialize(new { messages }, LineJson.Options);

        await Assert.That(result).IsEqualTo("""{"messages":[{"type":"text","text":"Hello"}]}""");
    }

    [Test]
    public async ValueTask DeserializePolymorphicType() {
        const string Json = """{"messages":[{"type":"text","text":"Hello"}]}""";

        var result = JsonSerializer.Deserialize<Test>(Json, LineJson.Options);

        await Assert.That(result).IsEquivalentTo(new Test([new TextMessage { Text = "Hello" }]));
    }

    [Test]
    public async ValueTask SerializeTemplateMessage() {
        var message = new TemplateMessage("Image carousel", new ImageCarouselTemplate([
            new ImageCarouselColumn("https://example.com/image1.jpg", new UriTemplateAction("View 1", "https://example.com/1")),
            new ImageCarouselColumn("https://example.com/image2.jpg", new UriTemplateAction("View 2", "https://example.com/2")),
            new ImageCarouselColumn("https://example.com/image3.jpg", new UriTemplateAction("View 3", "https://example.com/3")),
        ]));

        var result = JsonSerializer.Serialize(new { messages = new Message[] { message } }, LineJson.Options);

        await Assert.That(result).IsEqualTo("""{"messages":[{"type":"template","template":{"type":"image_carousel","columns":[{"imageUrl":"https://example.com/image1.jpg","action":{"type":"uri","label":"View 1","uri":"https://example.com/1"}},{"imageUrl":"https://example.com/image2.jpg","action":{"type":"uri","label":"View 2","uri":"https://example.com/2"}},{"imageUrl":"https://example.com/image3.jpg","action":{"type":"uri","label":"View 3","uri":"https://example.com/3"}}]},"altText":"Image carousel"}]}""");
    }

    sealed record Test(Message[] Messages);
}