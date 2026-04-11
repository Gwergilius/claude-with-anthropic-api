# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a learning/training project demonstrating Anthropic Claude API integration with two parallel implementations: Python (traditional scripts) and .NET (console app).

## Build & Run Commands

### .NET

```bash
# Set API key (one-time setup)
dotnet user-secrets set "Anthropic:ApiKey" "your-key" --project dotnet/AnthropicApiClient

# Build
dotnet build "dotnet/Claude with Anthropic API.slnx"

# Run (Production - uses Sonnet model by default)
dotnet run --project dotnet/AnthropicApiClient

# Run in Development (uses Haiku model)
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project dotnet/AnthropicApiClient
```

### Python

```bash
cd python
python -m venv .venv
.venv/Scripts/activate        # Windows
pip install -r requirements.txt
python examples.py
```

API key for Python goes in `python/.env` as `ANTHROPIC_API_KEY=your-key`.

## Architecture

### .NET (dotnet/AnthropicApiClient/)

Console app using manual DI setup (no generic host) following ASP.NET Core conventions:

- **Program.cs** — bootstraps `ConfigurationBuilder` + `ServiceCollection`, delegates to `Startup`
- **Startup.cs** — DI registration (mirrors ASP.NET Core `Startup` pattern)
- **Application.cs** — business logic entry point, resolved from DI
- **AnthropicClient.cs** — implements `IAntropicClient`, uses `IHttpClientFactory` + `IOptions<AnthropicOptions>`; posts raw JSON to `https://api.anthropic.com/v1/messages` and returns `JsonDocument`
- **AnthropicOptions.cs** — typed config bound from `appsettings.json` `"Anthropic"` section; validated on startup
- **AnthropicMessage.cs** — simple record for request message serialization

**Environment-based model selection** via `ASPNETCORE_ENVIRONMENT`:
- Production (default): `claude-sonnet-4-5` (`appsettings.json`)
- Development: `claude-haiku-4-5` (`appsettings.Development.json`)

**Note:** There is a typo in the interface name: `IAntropicClient` (missing 'h') — keep it consistent when editing.

### Python (python/)

Traditional Python scripts using the Anthropic SDK. `claude_client.py` contains the reusable client and context model, `examples.py` runs the sample prompts, and `chat_cli.py` provides interactive terminal chat.

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
