using System.Text.Json;
using FluentAssertions;
using JetBrains.Annotations;
using Line.Messaging;

namespace UnitTests;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ModelSerializationTests
{
    [Fact]
    public void SerializePolymorphicType() {
        Message[] messages = [new TextMessage { Text = "Hello" }];

        var result = JsonSerializer.Serialize(new { messages }, LineJson.Options);

        result.Should().Be("""{"messages":[{"type":"text","text":"Hello"}]}""", $"but {result}");
    }

    [Fact]
    public void DeserializePolymorphicType() {
        const string Json = """{"messages":[{"type":"text","text":"Hello"}]}""";

        var result = JsonSerializer.Deserialize<Test>(Json, LineJson.Options);

        result.Should().BeEquivalentTo(new Test([new TextMessage { Text = "Hello" }]));
    }
    sealed record Test(Message[] Messages);
}