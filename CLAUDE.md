# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

(RZ) LINE Messaging API — a C# SDK for the [LINE Messaging API](https://developers.line.me/messaging-api/overview), targeting .NET 10.0. Revamped from the [original by @pierre3](https://github.com/pierre3/LineMessagingApi/). Produces two NuGet packages: `RZ.Line.Messaging` and `RZ.Line.Messaging.AspNet`.

## Commands

```bash
# Build
dotnet build
dotnet build -c Release

# Test (xUnit)
dotnet test
dotnet test --filter "ClassName=ModelSerializationTests"
dotnet test --filter "Name=DeserializingFollowEvent"

# Pack NuGet packages
dotnet pack -c Release Line.Messaging/Line.Messaging.csproj
dotnet pack -c Release Line.Messaging.AspNet/Line.Messaging.AspNet.csproj

# Release script (moves .nupkg files to destination)
.\build.ps1 <destination-path>

# Restore
dotnet restore
```

## Architecture

### Projects

- **`Line.Messaging/`** — Core SDK. Handles message sending, webhook parsing, signature validation.
- **`Line.Messaging.AspNet/`** — ASP.NET Core integration. Adds DI setup and request extension methods.
- **`UnitTests/`** — xUnit tests with FluentAssertions.

### Key Types

| File | Purpose |
|------|---------|
| `LineMessagingClient.cs` | Main HTTP client wrapping the LINE REST API (`api.line.me/v2/`) |
| `ILineMessagingClient.cs` | Interface for all message-sending and profile operations |
| `LineDataClient.cs` / `ILineDataClient.cs` | Separate client for downloading binary content (images, video, audio) |
| `Webhooks/WebhookMessage.cs` | Parses and validates incoming webhook payloads |
| `Webhooks/WebhookRequestMessageHelper.cs` | HMAC-SHA256 signature validation with timing-safe comparison |
| `Json/LineJson.cs` | Custom `JsonSerializerOptions` with `TypedClassConverter` |
| `Messages/Message.cs` | Abstract base for all sendable message types |
| `Webhooks/WebhookEvent.cs` | Abstract base for all webhook event types |
| `LineMessagingConfiguration.cs` (AspNet) | DI registration + connection string config |
| `WebHookHelper.cs` (AspNet) | ASP.NET Core `HttpRequest` extension for webhook parsing |

### Polymorphic JSON Pattern

Both messages and webhook events use `System.Text.Json` polymorphic dispatch via `[JsonPolymorphic]` with a `"type"` discriminator:

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMessage), "text")]
[JsonDerivedType(typeof(ImageMessage), "image")]
// ...
public abstract record Message { ... }
```

The same pattern applies to `WebhookEvent` and its subtypes (FollowEvent, MessageEvent, PostbackEvent, etc.).

### Error Handling

Uses `Outcome<T>` from `RZ.Foundation` (with `LanguageExt` backing) instead of exceptions for expected failures:

```csharp
if (result.IfSuccess(out var value, out var error)) { ... }
else if (error.Code == StandardErrorCodes.NotFound) { ... }
```

### Dependency Injection (ASP.NET Core)

`ILineMessagingClient` and `ILineDataClient` are registered as scoped services via `LineMessagingConfiguration`. Configuration is read from a connection string using key-value pair parsing.

## Conventions

- **Records** with `required` init-only properties for all DTOs.
- **Nullable reference types** enabled globally (`<Nullable>enable</Nullable>`).
- **Language version:** `preview` — cutting-edge C# features are used freely.
- **`[PublicAPI]`** (JetBrains Annotations) on all public surface area.
- **FluentValidation** validators defined as nested or companion classes for domain types.
- **Global usings** in `GlobalUsings.cs` per project — check there before adding `using` statements.
- **Centralized package versions** in `Directory.Packages.props` — add versions there, not in individual `.csproj` files.
- **MinVer** drives NuGet versioning from git tags automatically.
- Tests use triple-quoted JSON string literals (`"""`) to mirror real LINE API payloads.
