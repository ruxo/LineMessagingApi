using System.Text.Json;
using RZ.Foundation.Json;

namespace Line.Messaging;

public static class LineJson
{
    public static readonly JsonSerializerOptions Options
        = new JsonSerializerOptions {
            RespectNullableAnnotations = true,
            Converters = {
                new TypedClassConverter([typeof(LineJson).Assembly]),
                new ToStringJsonConverter<AspectRatio>()
            }
        }.UseRzRecommendedSettings();
}