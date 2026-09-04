using System.Net;
using JetBrains.Annotations;
using Line.Messaging;
using TUnit.Mocks;
using TUnit.Mocks.Http;

namespace UnitTests;

/// <summary>
/// pace6/cosy#92 — how a push is made safe to retry, and how LINE's two kinds of 429 reach the caller
/// as different failures. The LINE server is faked at the HTTP handler seam.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class LineMessagingClientTests
{
    const string PushPath = "/v2/bot/message/push";
    const string MonthlyLimitBody = """{"message":"You have reached your monthly limit."}""";
    const string RateLimitBody = """{"message":"Too many requests"}""";
    const string RetryKeyAcceptedBody = """{"message":"The retry key is already accepted","sentMessages":[]}""";

    static (LineMessagingClient Client, MockHttpHandler Http) Create() {
        var http = Mock.HttpHandler();
        var client = new LineMessagingClient(http.CreateClient().SetupForLineMessageClient("channel-key"));
        return (client, http);
    }

    [Test, DisplayName("a push carries a retry key that is a UUID")]
    public async ValueTask Push_SendsRetryKey() {
        var (client, http) = Create();
        http.OnPost(PushPath).Respond(HttpStatusCode.OK);
        var keys = CaptureRetryKeys(http);

        var result = await client.PushMessageAsync("U1", "hello");

        await Assert.That(result.IsSuccess).IsTrue().Because($"result = {result}");
        await Assert.That(keys.Count).IsEqualTo(1);
        await Assert.That(Guid.TryParse(keys[0], out _)).IsTrue().Because($"key = {keys[0]}");
    }

    /// <summary>Records every retry key sent. The matcher never matches, so the real setup still answers.</summary>
    static List<string> CaptureRetryKeys(MockHttpHandler http) {
        var keys = new List<string>();
        http.OnRequest(m => m.Matching(r => {
            if (r.Headers.TryGetValues(LineApiErrors.RETRY_KEY_HEADER, out var v)) keys.Add(v.Single());
            return false;
        }));
        return keys;
    }

    [Test, DisplayName("each push batch carries its own retry key")]
    public async ValueTask Push_EachBatchHasItsOwnKey() {
        var (client, http) = Create();
        http.OnPost(PushPath).Respond(HttpStatusCode.OK);
        var keys = CaptureRetryKeys(http);

        var result = await client.PushMessageAsync("U1", "1", "2", "3", "4", "5", "6");

        await Assert.That(result.IsSuccess).IsTrue().Because($"result = {result}");
        await Assert.That(keys.Count).IsEqualTo(2);
        await Assert.That(keys[0]).IsNotEqualTo(keys[1]);
    }

    [Test, DisplayName("a 409 'retry key already accepted' is a delivered push, not a failure")]
    public async ValueTask Push_RetryKeyAlreadyAccepted_IsSuccess() {
        var (client, http) = Create();
        http.OnPost(PushPath).RespondWithJson(RetryKeyAcceptedBody, HttpStatusCode.Conflict);

        var result = await client.PushMessageAsync("U1", "hello");

        await Assert.That(result.IsSuccess).IsTrue().Because($"result = {result}");
    }

    [Test, DisplayName("a monthly-quota 429 fails with the quota code")]
    public async ValueTask Push_MonthlyQuota_IsQuotaExceeded() {
        var (client, http) = Create();
        http.OnPost(PushPath).RespondWithJson(MonthlyLimitBody, HttpStatusCode.TooManyRequests);

        var result = await client.PushMessageAsync("U1", "hello");

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.Error!.Is(LineApiErrors.QUOTA_EXCEEDED)).IsTrue().Because($"error = {result.Error}");
    }

    [Test, DisplayName("any other 429 fails with the rate-limit code")]
    public async ValueTask Push_RateLimit_IsRateLimited() {
        var (client, http) = Create();
        http.OnPost(PushPath).RespondWithJson(RateLimitBody, HttpStatusCode.TooManyRequests);

        var result = await client.PushMessageAsync("U1", "hello");

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.Error!.Is(LineApiErrors.RATE_LIMITED)).IsTrue().Because($"error = {result.Error}");
        await Assert.That(result.Error!.Is(LineApiErrors.QUOTA_EXCEEDED)).IsFalse();
    }

    [Test, DisplayName("a failed response keeps its HTTP status in the error")]
    public async ValueTask Push_ServerError_CarriesStatus() {
        var (client, http) = Create();
        http.OnPost(PushPath).RespondWithJson("""{"message":"boom"}""", HttpStatusCode.InternalServerError);

        var result = await client.PushMessageAsync("U1", "hello");

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.Error!.Is(RZ.Foundation.StandardErrorCodes.HTTP_ERROR)).IsTrue().Because($"error = {result.Error}");
        await Assert.That(result.Error!.Data).Contains("InternalServerError").Because($"data = {result.Error!.Data}");
        await Assert.That(result.Error!.Data).Contains("boom");
    }
}
