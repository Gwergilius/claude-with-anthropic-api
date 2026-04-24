# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a learning/training project demonstrating Anthropic Claude API integration with parallel implementations: Python (Jupyter notebooks + traditional scripts) and .NET (a console app, a Blazor Server chat UI, and a prompt evaluation console app). All three .NET projects share the same `AnthropicShared` class library.

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

### PromptEvaluator

```bash
# Set API key (one-time setup)
dotnet user-secrets set "Anthropic:ApiKey" "your-key" --project dotnet/PromptEvaluator

# Run (uses Sonnet model; reads Data/prompt.yaml + Data/dataset.json)
dotnet run --project dotnet/PromptEvaluator
# Outputs: Data/output.json (raw results) and Data/output.html (report)
```

### Python

```bash
cd python
python -m venv .venv
.venv/Scripts/activate        # Windows
pip install -r requirements.txt

# Traditional scripts
python examples.py            # sample prompts
python chat_cli.py            # interactive terminal chat

# Jupyter notebook
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
  PromptEvaluator/      — prompt evaluation console app
python/
  001-requests.ipynb              — raw HTTP calls (no SDK), quick experimentation
  001_prompt_evals_complete.ipynb — complete prompt evaluation walkthrough
  002-prompt-evaluation.ipynb     — end-to-end evaluation pipeline
  003_dataset_generator.ipynb     — dataset generation via Claude
  003_prompting.ipynb             — prompting technique experiments
  claude_client.py                — reusable SDK client with context management
  anthropic_client.py             — stateless SDK wrapper (no conversation context)
  anthropic_config.py             — config singleton (env var loading, lazy client init)
  anthropic_message.py            — immutable AnthropicMessage dataclass
  grader.py                       — LLM-as-judge grader helper
  prompt_runner.py                — prompt template runner ({key} placeholder filling)
  immutable_dict.py               — ImmutableDict type for prompt inputs
  dataset.json                    — evaluation dataset (shared with PromptEvaluator .NET)
  chat_cli.py                     — interactive terminal chat
  examples.py                     — sample prompts
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

### PromptEvaluator (console app)

Generic host (`IHostBuilder`) setup. Shares the same `AnthropicShared` class library.

- **Program.cs** — `IHostBuilder` with `ConfigurationBuilder`, delegates to `Startup`
- **Startup.cs** — DI registration; `IAntropicClient`/`AnthropicClient` registered as **Transient** (fresh, context-free client per scope — no conversation carryover between evaluation calls)
- **Application.cs** — entry point resolved from DI; loads YAML prompt + JSON dataset, drives evaluation, writes `output.json` and `output.html`

**DI lifetimes (all Transient):**
- `IAntropicClient` / `AnthropicClient` — fresh instance per scope; each test case gets its own stateless client
- `PromptEvaluatorService` — orchestrates concurrent evaluation pipeline
- `Application` — entry point

**Key types:**
- **`PromptEvaluatorService`** — fans out over the dataset with `SemaphoreSlim(maxConcurrentTasks)`; uses `IServiceScopeFactory` to create a fresh scope per test case; each case triggers two Claude calls: one for the prompt-under-test, one for LLM-as-judge grading
- **`ReportGenerator`** — static; converts `IReadOnlyList<EvaluationResult>` to a self-contained HTML report
- **`EvaluatorOptions`** — typed config from `"Evaluator"` section: `DatasetFile`, `PromptFile`, `JsonOutputFile`, `HtmlOutputFile`, `MaxConcurrentTasks`
- **`PromptConfig`** — YAML-deserialized prompt definition: `TaskDescription`, `Prompt` (with `{key}` placeholders), optional `ExtraCriteria` (hard requirements; any violation → score ≤ 3)
- **`TestCase`** — one dataset entry: `Scenario`, `TaskDescription`, `PromptInputs` (placeholder values dict), `SolutionCriteria` (list)
- **`EvaluationResult`** — result of one evaluated case: `Output`, `TestCase`, `Score` (1–10), `Reasoning`

**Evaluation flow:**
1. `Application.Run` deserializes `prompt.yaml` → `PromptConfig` and `dataset.json` → `List<TestCase>`
2. `PromptEvaluatorService.RunEvaluationAsync` fans out over all cases with bounded concurrency
3. Per case: `{key}` placeholders in `PromptConfig.Prompt` filled from `TestCase.PromptInputs` → Claude call → raw output
4. Grader: second Claude call (`system="you are an expert evaluator"`, `temperature=0`) scores 1–10 against `SolutionCriteria` and optional `ExtraCriteria`; response parsed from JSON
5. Results written to `Data/output.json` (raw array) and `Data/output.html` (self-contained HTML report)

**Data files (`Data/`):**
- `prompt.yaml` — prompt under test; YAML with `taskDescription`, `prompt` (template), and optional `extraCriteria`
- `dataset.json` — evaluation cases; linked from `../../python/dataset.json` via `Content Include` in `.csproj` (generated by Python dataset generator notebook)
- `output.json` — raw `EvaluationResult[]` (overwritten on each run)
- `output.html` — HTML evaluation report (overwritten on each run)

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

### Python (python/)

**Core client layer:**
- **`anthropic_config.py`** — `AnthropicConfig` singleton: reads `Anthropic__*` env vars from `.env`, exposes lazy-loaded `client` and `model` properties
- **`anthropic_client.py`** — stateless thin wrapper around the Anthropic SDK; no conversation context; used by `grader.py` and `prompt_runner.py`
- **`anthropic_message.py`** — immutable `AnthropicMessage` dataclass (role + content)
- **`claude_client.py`** — stateful SDK client with rolling conversation context; `examples.py` runs sample prompts; `chat_cli.py` provides interactive terminal chat

**Prompt evaluation suite** (notebooks + helpers):
- **`002-prompt-evaluation.ipynb`** — end-to-end evaluation pipeline: renders prompts, calls Claude, grades with LLM-as-judge, writes results
- **`003_dataset_generator.ipynb`** — generates `dataset.json` via Claude (creates diverse test cases for a given task)
- **`003_prompting.ipynb`** — prompting technique experiments
- **`001_prompt_evals_complete.ipynb`** — complete evaluation walkthrough (reference/all-in-one)
- **`grader.py`** — `LlmGrader`: LLM-as-judge helper; scores a `(test_case, output)` pair via Claude; returns score + reasoning
- **`prompt_runner.py`** — `PromptRunner`: fills `{key}` template placeholders and calls Claude; returns raw output
- **`immutable_dict.py`** — `ImmutableDict`: frozendict-like type used for prompt inputs
- **`dataset.json`** — evaluation dataset shared with the `PromptEvaluator` .NET project (linked via `Content Include` in `.csproj`)

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
