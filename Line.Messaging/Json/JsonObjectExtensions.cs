using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace Line.Messaging;

public static class JsonObjectExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsonObject Object(this JsonObject json, string key)
        => (JsonObject)json[key]!;

    public static string String(this JsonObject json, string key)
        => json[key]!.GetValue<string>();

    public static T Enum<T>(this JsonObject json, string key) where T : struct
        => System.Enum.TryParse<T>(json[key]!.GetValue<string>(), ignoreCase: true, out var result) ? result : throw new ArgumentOutOfRangeException(nameof(key));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsonObject? TryGetObject(this JsonObject json, string key)
        => json[key] as JsonObject;

    public static T? TryGetEnum<T>(this JsonObject json, string key) where T : struct
        => json[key]?.GetValue<string>() is {} value && System.Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : default;
}