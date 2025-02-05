using System.Net.Http.Headers;

namespace Line.Messaging.AspNet;

public static class HttpExtensions
{
    public static HttpClient SetupForLineMessageClient(this HttpClient http, string apiKey) {
        http.BaseAddress = new Uri(LineMessagingClient.OfficialUri);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return http;
    }
}