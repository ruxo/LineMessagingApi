using System.Text.Json;
using System.Text.Json.Serialization;
using RZ.Foundation.Json;

namespace Line.Messaging;

public static class LineJson
{
    public static readonly JsonSerializerOptions Options
        = new JsonSerializerOptions {
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            RespectNullableAnnotations = true,
            Converters = {
                new TypedClassConverter([typeof(LineJson).Assembly]),
                new ToStringJsonConverter<AspectRatio>()
            }
        }.UseRzRecommendedSettings();
}