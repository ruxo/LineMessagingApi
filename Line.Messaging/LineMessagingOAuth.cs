using System.Net.Http.Headers;
using System.Text.Json;

namespace Line.Messaging;

[PublicAPI]
public static class LineMessagingOAuth
{
    const string DefaultUri = "https://api.line.me/v2";

    public static HttpClient Base(this HttpClient client, string uri) {
        client.BaseAddress = new Uri(uri);
        return client;
    }

    #region OAuth
    // https://developers.line.me/en/docs/messaging-api/reference/#oauth

    /// <summary>
    /// Issues a short-lived channel access token.
    /// Up to 30 tokens can be issued. If the maximum is exceeded, existing channel access tokens will be revoked in the order of when they were first issued.
    /// https://developers.line.me/en/docs/messaging-api/reference/#oauth
    /// </summary>
    /// <param name="httpClient">HttpClient with <c>BaseAddress</c> configured</param>
    /// <param name="channelId">ChannelId</param>
    /// <param name="channelAccessToken">ChannelAccessToken</param>
    /// <returns>ChannelAccessToken</returns>
    public static async Task<ChannelAccessToken> IssueChannelAccessTokenAsync(this HttpClient httpClient, string channelId, string channelAccessToken)
    {
        var response = await httpClient.PostAsync("/oauth/accessToken",
                                                  new FormUrlEncodedContent(new Dictionary<string, string>
                                                  {
                                                      ["grant_type"] = "client_credentials",
                                                      ["client_id"] = channelId,
                                                      ["client_secret"] = channelAccessToken
                                                  })).ConfigureAwait(false);
        await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<ChannelAccessToken>(json, SnakeCaseOptions)!;
    }

    static readonly JsonSerializerOptions SnakeCaseOptions = new(LineJson.Options) {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    /// <summary>
    /// Revokes a channel access token.
    /// https://developers.line.me/en/docs/messaging-api/reference/#revoke-channel-access-token
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="channelAccessToken">ChannelAccessToken</param>
    public static async Task RevokeChannelAccessTokenAsync(this HttpClient httpClient, string channelAccessToken)
    {
        var response = await httpClient.PostAsync("/oauth/revoke",
                                                  new FormUrlEncodedContent(new Dictionary<string, string>
                                                  {
                                                      ["access_token"] = channelAccessToken
                                                  })).ConfigureAwait(false);
        await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Instantiate LineMessagingClient by using OAuth.
    /// https://developers.line.me/en/docs/messaging-api/reference/#oauth
    /// </summary>
    public static async Task<LineMessagingClient> CreateAsync(HttpClient client, string channelId, string channelSecret) {
        var accessToken = await client.IssueChannelAccessTokenAsync(channelId, channelSecret);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);
        return new LineMessagingClient(client);
    }

    #endregion
}