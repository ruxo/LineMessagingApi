using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Line.Messaging.Webhooks;

[PublicAPI]
public static class WebhookRequestMessageHelper
{
    /// <summary>
    /// Verify if the request is valid, then returns LINE Webhook events from the request
    /// </summary>
    /// <param name="request">HttpRequestMessage</param>
    /// <param name="channelSecret">ChannelSecret</param>
    /// <param name="botUserId">BotUserId</param>
    /// <returns>List of WebhookEvent</returns>
    public static async Task<IEnumerable<WebhookEvent>> GetWebhookEventsAsync(this HttpRequestMessage request, string channelSecret, string? botUserId = null)
    {
        var content = await request.Content.ReadAsStringAsync();

        var xLineSignature = request.Headers.GetValues("X-Line-Signature").FirstOrDefault();
        if (string.IsNullOrEmpty(xLineSignature) || !VerifySignature(channelSecret, xLineSignature, content))
        {
            throw new InvalidSignatureException("Signature validation faild.");
        }

        dynamic json = JsonConvert.DeserializeObject(content);

        if (!string.IsNullOrEmpty(botUserId))
        {
            if(botUserId != (string)json.destination)
            {
                throw new UserIdMismatchException("Bot user ID does not match.");
            }
        }
        return WebhookEventParser.ParseEvents(json.events);
    }

    /// <summary>
    /// The signature in the X-Line-Signature request header must be verified to confirm that the request was sent from the LINE Platform.
    /// Authentication is performed as follows.
    /// 1. With the channel secret as the secret key, your application retrieves the digest value in the request body created using the HMAC-SHA256 algorithm.
    /// 2. The server confirms that the signature in the request header matches the digest value which is Base64 encoded
    /// https://developers.line.me/en/docs/messaging-api/reference/#signature-validation
    /// </summary>
    /// <param name="channelSecret">ChannelSecret</param>
    /// <param name="xLineSignature">X-Line-Signature header</param>
    /// <param name="requestBody">RequestBody</param>
    /// <returns></returns>
    public static bool VerifySignature(string channelSecret, string xLineSignature, string requestBody)
    {
        var key = Encoding.UTF8.GetBytes(channelSecret);
        var body = Encoding.UTF8.GetBytes(requestBody);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(body, 0, body.Length);
        var (error, xLineBytes) = Try(xLineSignature, Convert.FromBase64String);
        return error is null && SlowEquals(xLineBytes, hash);
    }

    /// <summary>
    /// Compares two-byte arrays in length-constant time.
    /// This comparison method is used so that password hashes cannot be extracted from on-line systems using a timing attack and then attacked off-line.
    /// <para> http://bryanavery.co.uk/cryptography-net-avoiding-timing-attack/#comment-85　</para>
    /// </summary>
    static bool SlowEquals(byte[] a, byte[] b)
    {
        var diff = (uint)a.Length ^ (uint)b.Length;
        var minLength = Math.Min(a.Length, b.Length);
        for (var i = 0; i < minLength; i++)
            diff |= (uint)(a[i] ^ b[i]);
        return diff == 0;
    }
}