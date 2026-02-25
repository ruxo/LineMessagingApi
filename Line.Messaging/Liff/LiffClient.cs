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
    public async ValueTask<Outcome<string>> AddLiffAppAsync(ViewType viewType, string url)
        => Fail(await client.PostJson("/apps", new { view = new View(viewType, url) }, LineJson.Options).GetLineJsonAsync<LiffInfo>(), out var e, out var info)
               ? e.Trace()
               : info.LiffId;

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
    public ValueTask<Outcome<Unit>> UpdateLiffAppAsync(string liffId, ViewType viewType, string url)
        => client.PutJson($"/apps/{liffId}/view", new { type = viewType, url }, LineJson.Options).CheckSucceed();

    /// <summary>
    /// Gets information on all the LIFF apps registered in the channel.
    /// </summary>
    /// <returns>A JSON object with the following properties.</returns>
    public ValueTask<Outcome<LiffApp[]>> GetAllLiffAppAsync()
        => client.Get("/apps").DeserializedJson<LiffAppInfo>(LineJson.Options).Select(x => x.Apps);

    readonly record struct LiffAppInfo(LiffApp[] Apps);

    /// <summary>
    /// Deletes a LIFF app.
    /// </summary>
    /// <param name="liffId">ID of the LIFF app to be deleted</param>
    public ValueTask<Outcome<Unit>> DeleteLiffAppAsync(string liffId)
        => client.Delete($"/apps/{liffId}").CheckSucceed();
}