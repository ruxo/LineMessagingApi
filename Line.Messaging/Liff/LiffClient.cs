using System.Net.Http.Json;
using LanguageExt;

namespace Line.Messaging.Liff;

/// <summary>
/// HTTP Client for the LINE Front-end Framework (LIFF) API
/// </summary>
[PublicAPI]
public class LiffClient(HttpClient client)
{
    public const string DefaultUri = "https://api.line.me/liff/v1";

    /// <summary>
    /// Adds an app to LIFF. You can add up to 10 LIFF apps on one channel.
    /// </summary>
    /// <param name="viewType">
    /// Size of the LIFF app view. Specify one of the following values
    /// </param>
    /// <param name="url">
    /// URL of the LIFF app. Must start with HTTPS.
    /// </param>
    /// <returns>
    /// LIFF app ID
    /// </returns>
    public Task<string> AddLiffAppAsync(ViewType viewType, string url)
        => client.PostAsJsonAsync("/apps", new { view = new View(viewType, url) }, LineJson.Options)
                 .EnsureSuccessStatusCodeAsync()
                 .GetLineJsonAsync<LiffInfo>()
                 .Select(x => x.LiffId);

    readonly record struct LiffInfo(string LiffId);

    /// <summary>
    /// Updates LIFF app settings.
    /// </summary>
    /// <param name="liffId">ID of the LIFF app to be updated</param>
    /// <param name="viewType">
    /// Size of the LIFF app view. Specify one of the following values
    /// </param>
    /// <param name="url">
    /// URL of the LIFF app. Must start with HTTPS.
    /// </param>
    public Task UpdateLiffAppAsync(string liffId, ViewType viewType, string url)
        => client.PutAsJsonAsync($"/apps/{liffId}/view", new { type = viewType, url }, LineJson.Options)
                 .EnsureSuccessStatusCodeAsync();

    /// <summary>
    /// Gets information on all the LIFF apps registered in the channel.
    /// </summary>
    /// <returns>A JSON object with the following properties.</returns>
    public Task<LiffApp[]> GetAllLiffAppAsync()
        => client.GetFromJsonAsync<LiffAppInfo>("/apps", LineJson.Options).Select(x => x.Apps);

    readonly record struct LiffAppInfo(LiffApp[] Apps);

    /// <summary>
    /// Deletes a LIFF app.
    /// </summary>
    /// <param name="liffId">ID of the LIFF app to be deleted</param>
    public Task DeleteLiffAppAsync(string liffId)
        => client.DeleteAsync($"/apps/{liffId}").EnsureSuccessStatusCodeAsync();
}