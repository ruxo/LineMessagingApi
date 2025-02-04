using System.Text.Json.Serialization;

namespace Line.Messaging;

public enum ComponentSize
{
    [JsonStringEnumMemberName("xxs")] Xxs,
    [JsonStringEnumMemberName("xs")] Xs,
    [JsonStringEnumMemberName("sm")] Sm,
    [JsonStringEnumMemberName("md")] Md,
    [JsonStringEnumMemberName("lg")] Lg,
    [JsonStringEnumMemberName("xl")] Xl,
    [JsonStringEnumMemberName("xxl")] Xxl,
    [JsonStringEnumMemberName("3xl")] _3xl,
    [JsonStringEnumMemberName("4xl")] _4xl,
    [JsonStringEnumMemberName("5xl")] _5xl,
    [JsonStringEnumMemberName("full")] Full
}