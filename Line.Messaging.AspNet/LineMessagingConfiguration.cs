using System.Net.Http.Headers;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RZ.Foundation.Helpers;
using RZ.Foundation.Types;

namespace Line.Messaging.AspNet;

[PublicAPI]
public static class LineMessagingConfiguration
{
    public static IHostApplicationBuilder AddLineMessaging(this IHostApplicationBuilder builder, string connectionString = "LineApi") {
        var services = builder.Services;
        services.AddScoped<ILineMessagingClient, LineMessagingClient>()
                .AddScoped<ILineDataClient, LineDataClient>();

        var lineConfig = builder.Configuration.GetConnectionString(connectionString) ?? throw new ErrorInfoException(StandardErrorCodes.MissingConfiguration, "Line API configuration is missing");
        var kv = KeyValueString.Parse(lineConfig);

        // Currently support only the channel API key.
        var apiKey = kv.Get("apiKey").ToNullable() ?? throw new ErrorInfoException(StandardErrorCodes.MissingConfiguration, "Line API key is missing");

        services.AddHttpClient<LineMessagingClient>(opts => {
            opts.BaseAddress = new Uri(LineMessagingClient.OfficialUri);
            opts.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        });
        services.AddHttpClient<LineDataClient>(opts => {
            opts.BaseAddress = new Uri(LineDataClient.OfficialUri);
            opts.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        });
        return builder;
    }
}