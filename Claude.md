# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a learning/training project demonstrating Anthropic Claude API integration with three parallel implementations: Python (Jupyter notebook), a .NET console app, and a Blazor Server chat UI. All three share the same `AnthropicShared` class library.

## Build & Run Commands

### Solution

```bash
# Build everything
dotnet build "dotnet/Claude with Anthropic API.slnx"
```

### BlazorChat (primary UI)

```bash
# Set API key (one-time setup)
dotnet user-secrets set "Anthropic:ApiKey" "your-key" --project dotnet/BlazorChat

# Run (Production — uses Sonnet model)
dotnet run --project dotnet/BlazorChat

# Run in Development (uses Haiku model, enables SSE trace logging)
ASPNETCORE_ENVIRONMENT=Development dotnet run --project dotnet/BlazorChat
```

### Console app (AnthropicApiClient)

```bash
# Set API key (one-time setup)
dotnet user-secrets set "Anthropic:ApiKey" "your-key" --project dotnet/AnthropicApiClient

# Run (Production - uses Sonnet model by default)
dotnet run --project dotnet/AnthropicApiClient

# Run in Development (uses Haiku model)
ASPNETCORE_ENVIRONMENT=Development dotnet run --project dotnet/AnthropicApiClient
```

### Python

```bash
cd python
python -m venv .venv
.venv/Scripts/activate        # Windows
pip install requests python-dotenv jupyter
jupyter notebook 001-requests.ipynb
```

API key for Python goes in `python/.env` as `ANTHROPIC_API_KEY=your-key`.

## Architecture

### Solution layout

```
dotnet/
  AnthropicShared/      — shared class library (client, options, types)
  AnthropicApiClient/   — console app
  BlazorChat/           — Blazor Server chat UI
python/
  001-requests.ipynb    — raw HTTP calls to Anthropic API (no SDK)
```

### AnthropicShared (class library)

Shared between both .NET projects. Key types:

- **`IAntropicClient`** — conversation interface; holds rolling `Context` (user+assistant turns), exposes `SendMessage` (blocking) and `StartStreamingMessageAsync` (SSE). Note: intentional typo — missing 'h' in `IAntropicClient`. Keep consistent.
- **`AnthropicClient`** — implements the above. Uses `IHttpClientFactory` + `IOptionsMonitor<AnthropicOptions>` (monitor, not snapshot, so live config updates take effect). Streaming path uses `HttpCompletionOption.ResponseHeadersRead` and returns an `AnthropicStreamingResponse` (owns both the `HttpResponseMessage` and body `Stream` — caller must dispose).
- **`AnthropicStreamingResponse`** — thin wrapper: owns `HttpResponseMessage` + `Stream`, implements `IDisposable`.
- **`AnthropicOptions`** — typed config from `"Anthropic"` appsettings section: `ApiKey`, `Model`, `ApiVersion`, `MaxTokens`, `Temperature`, `SystemPrompt`.
- **`AnthropicMessagesApiRequest`** — internal; maps to the POST `/v1/messages` JSON body.
- **`IAnthropicRequestTelemetry`** / **`NullAnthropicRequestTelemetry`** — observer interface for outgoing requests (used by BlazorChat's log panel; no-op in the console app).

### AnthropicApiClient (console app)

Manual DI setup (no generic host) following ASP.NET Core conventions:

- **Program.cs** — `ConfigurationBuilder` + `ServiceCollection`, delegates to `Startup`
- **Startup.cs** — DI registration
- **Application.cs** — business logic entry point resolved from DI

### BlazorChat (Blazor Server UI)

Interactive Server rendering. Entry point is `Program.cs` → `Startup.ConfigureServices`.

**DI lifetimes (important):**
- `IAntropicClient` / `AnthropicClient` — **Singleton**: conversation context persists for the app lifetime.
- `RuntimeAnthropicOptions` — **Singleton**: registered as both `IOptions<AnthropicOptions>` and `IOptionsMonitor<AnthropicOptions>`; merges appsettings baseline with live browser overrides from the config panel.
- `AnthropicRequestLog` — **Singleton**: in-memory ring buffer (max 100 entries) for the dev log panel, also registered as `IAnthropicRequestTelemetry`.
- `IAnthropicUserSettingsService` / `IAnthropicStreamProgressService` — **Scoped** (per circuit).

**Streaming flow:**
1. `Home.razor.cs` calls `IAntropicClient.StartStreamingMessageAsync` → gets `AnthropicStreamingResponse`.
2. Passes `response.Body` stream to `IAnthropicStreamProgressService.Start(stream, ct)`.
3. `AnthropicStreamProgressService` reads SSE lines on a background task, raises `ProgressChanged` (per `content_block_delta`) and `ProgressCompleted` (on `message_stop`, fault, or cancel).
4. `Home.razor.cs` calls `AnthropicClient.AppendLastAssistantMessageText(delta)` on each delta event, then awaits `_streamDoneTcs.Task` for completion.

**Configuration panel** (`AnthropicConfigurationForm.razor`):
- Settings (model, temperature, system prompt) are persisted to browser `localStorage` via `IAnthropicUserSettingsService`.
- On load, browser overrides are merged into `RuntimeAnthropicOptions` via `MergePersistedOverrides`.
- Live changes call `RuntimeAnthropicOptions.ApplyFormState`, which notifies `IOptionsMonitor` listeners so `Home.razor.cs` re-renders.

**Slash commands** (typed into the chat input, not sent to the API):
- `/exit` — closes the browser window
- `/error [msg]` — injects a test error message into the UI

**Environment-based model selection** via `ASPNETCORE_ENVIRONMENT`:
- Production (default): `claude-sonnet-4-5` (`appsettings.json`)
- Development: `claude-haiku-4-5` (`appsettings.Development.json`); also enables `Trace`-level SSE event logging for `AnthropicStreamProgressService`.

## Documentation Standards

- **All documentation, code comments, variable/function names, and error messages must be in English.**
- **Use reference-style links** in all Markdown files — define link references at the top of each file with descriptive names and tooltips. Inline `[text](url)` links are incorrect per project standards.

```markdown
<!-- ✅ Correct -->
[anthropic-console]: https://console.anthropic.com/ "Anthropic Console"
Visit the [Anthropic Console][anthropic-console].

<!-- ❌ Incorrect -->
Visit the [Anthropic Console](https://console.anthropic.com/).
```

## Code Standards

- .NET: PascalCase for public members, camelCase for private; XML doc comments on public APIs
- Python: snake_case; docstrings on functions/classes
- All code comments in English
- Error returns use `FluentResults` (`Result<T>`) — not exceptions — in `AnthropicShared`
