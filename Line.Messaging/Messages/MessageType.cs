using System.Text.Json.Serialization;

namespace Line.Messaging;

public enum MessageType
{
    [JsonStringEnumMemberName("text")] Text,
    [JsonStringEnumMemberName("image")] Image,
    [JsonStringEnumMemberName("video")] Video,
    [JsonStringEnumMemberName("audio")] Audio,
    [JsonStringEnumMemberName("location")] Location,
    [JsonStringEnumMemberName("sticker")] Sticker,
    [JsonStringEnumMemberName("imagemap")] Imagemap,
    [JsonStringEnumMemberName("template")] Template,
    [JsonStringEnumMemberName("file")] File,
    [JsonStringEnumMemberName("flex")] Flex,
}