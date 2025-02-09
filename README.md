# (RZ) LINE Messaging API

[Original work by @pierre3](https://github.com/pierre3/LineMessagingApi/)

This is a C# implementation of the [LINE Messaging API](https://developers.line.me/messaging-api/overview).

This is a revamped version of the original library. This library targets .NET 9 and so on.

## Getting Started
This repository contains SDK itself, as well as base samples and Visual Studio templates.

# Usage
Basically, your can communicate with LINE API by passing `HttpClient` to `LineMessagingClient` constructor.

## LineMessagingClient Class

This is a class to communicate with LINE Messaging API platform. It uses HttpClient-based asynchronous methods such as followings.

```cs
Task<BotInfo> GetBotInfo()
Task ReplyMessageAsync(string replyToken, IEnumerable<ISendMessage> messages)
Task ReplyMessageAsync(string replyToken, params string[] messages)
Task ReplyMessageWithJsonAsync(string replyToken, params string[] messages)
Task PushMessageAsync(string to, IEnumerable<ISendMessage> messages)
Task PushMessageAsync(string to, params string[] messages)
... (more in `ILineMessagingClient` interface)
```

## Parse and process Webhook-Events
Use GetWebhookEventsAsync extension method for incoming request to parse the LINE events from the LINE platform. See [FunctionAppSample/HttpTriggerFunction.sc](https://github.com/pierre3/LineMessagingApi/blob/master/FunctionAppSample/HttpTriggerFunction.cs) as an example.

```cs
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionAppSample;

public class WebHookHandler(ILogger<WebHookHandler> logger, HttpContext ctx)
{
    [HttpPost]
    public void Handle(){
        using var reader = new StreamReader(ctx.Request.Body);
        var body = await reader.ReadToEndAsync();

        #if DEBUG
        Console.WriteLine("Headers:\n==========");
        foreach (var header in ctx.Request.Headers) {
            Console.WriteLine($"{header.Key}: {header.Value}");
        }

        Console.WriteLine("\nBody:\n==========");
        Console.WriteLine(body);
        Console.WriteLine("==========");
        #endif

        if ((await ctx.Request.TryGetWebhookMessage(body)).IfSuccess(out var m, out var error)){
            if (!WebhookRequestMessageHelper.VerifySignature(integration.ChannelSecret, m.Signature, body)){
                logger.LogError("Signature validation failed");
                return;
            }

            foreach(var @event in m.Message.Events)
                switch (@event){
                    case FollowEvent follow:
                        // do follow event
                        break;

                    case MessageEvent text:
                        // do message event
                        break;
                    
                    // more if needed

                    default:
                        logger.LogDebug("Discard LINE event: {EventType}", @event.Type);
                        break;
                }
        }
        else if (error.Code == StandardErrorCodes.NotFound)
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            logger.LogWarning(error.Message);
        else
            logger.LogError("Body is not a valid WebhookMessage: {@Error}", error);
    }
}
```
