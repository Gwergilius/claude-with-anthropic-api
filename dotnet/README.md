[anthropic-console]: https://console.anthropic.com/ "Anthropic Console"
[main-readme]: ../README.md "Main Project README"
[user-secrets]: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets "ASP.NET Core User Secrets"
[gh-codespaces-secrets]: https://docs.github.com/en/codespaces/setting-your-user-preferences/managing-your-account-specific-secrets-for-github-codespaces "Managing your account-specific secrets for GitHub Codespaces"
[gh-dotfiles]: https://docs.github.com/en/codespaces/setting-your-user-preferences/personalizing-github-codespaces-for-your-account#dotfiles "Personalizing Codespaces with dotfiles"
[gh-actions-secrets]: https://docs.github.com/en/actions/security-for-github-actions/security-guides/using-secrets-in-github-actions "Using secrets in GitHub Actions"

# .NET Implementation - Claude with Anthropic API

This is the .NET implementation of the Claude API integration project.

## 🔷 Features

- **.NET 10.0** with **C# 14** (Primary constructors, collection expressions)
- **Enterprise-grade architecture**:
  - Dependency Injection (Microsoft.Extensions.DependencyInjection)
  - Configuration management (IOptions / IOptionsMonitor pattern)
  - Environment-based configuration (Development vs Production)
  - Structured logging (ILogger)
  - HTTP client factory pattern
  - User Secrets for API key security
- **Prompt Evaluation** (PromptEvaluator project):
  - YAML-driven prompt configuration with `{key}` placeholder templating
  - JSON dataset linked from the Python notebook output
  - LLM-as-judge grading with bounded concurrency
  - HTML + JSON evaluation reports

## 🛠 Technologies Used

- .NET 10.0
- Microsoft.Extensions.* ecosystem (10.0.0 packages)
- System.Text.Json for JSON serialization
- HttpClientFactory for HTTP communication

## ⚙️ Setup

1. **Install .NET 10.0** if not already installed
2. **Set your API key** using User Secrets:
   ```bash
   dotnet user-secrets set "Anthropic:ApiKey" "your-api-key-here" --project AnthropicApiClient
   dotnet user-secrets set "Anthropic:ApiKey" "your-api-key-here" --project BlazorChat
   dotnet user-secrets set "Anthropic:ApiKey" "your-api-key-here" --project PromptEvaluator
   ```
3. **Build the solution**:
   ```bash
   dotnet build "Claude with Anthropic API.slnx"
   ```

## 🚀 Usage

**Production (uses Sonnet model for quality) - Default:**
```bash
cd AnthropicApiClient
dotnet run
```

**Development (uses Haiku model for cost efficiency):**
```bash
cd AnthropicApiClient
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

## 🏗 Architecture

### Shared Library (AnthropicShared)

Core types used by all three .NET projects:

- **`IAntropicClient`** — conversation interface: rolling `Context`, `SendMessage` (blocking), `StartStreamingMessageAsync` (SSE)
- **`AnthropicClient`** — HTTP implementation using `IHttpClientFactory` + `IOptionsMonitor<AnthropicOptions>`
- **`AnthropicOptions`** — typed config from the `"Anthropic"` section: `ApiKey`, `Model`, `MaxTokens`, `Temperature`, `SystemPrompt`

### AnthropicApiClient (console app)

Manual DI setup (no generic host):

- **Program.cs** — `ConfigurationBuilder` + `ServiceCollection`, delegates to `Startup`
- **Startup.cs** — DI registration
- **Application.cs** — business logic entry point

### BlazorChat (Blazor Server UI)

Interactive Server rendering with `IAntropicClient` as Singleton (conversation persists for the app lifetime), streaming via SSE, and a configuration panel backed by browser `localStorage`.

### PromptEvaluator (console app)

Generic host (`IHostBuilder`) setup. All services registered as **Transient** so each test case evaluation gets a fresh, context-free `AnthropicClient`.

**Core components:**
- **Program.cs** — `IHostBuilder` with config and `Startup`
- **Startup.cs** — DI registration
- **Application.cs** — loads `Data/prompt.yaml` + `Data/dataset.json`, drives evaluation, writes reports

**Evaluation flow:**
1. `{key}` placeholders in `prompt.yaml` filled from each dataset entry's `prompt_inputs` → Claude call → raw output
2. Second Claude call (LLM-as-judge, `temperature=0`) scores the output 1–10 against per-case criteria
3. Aggregated results written to `Data/output.json` (raw JSON) and `Data/output.html` (self-contained HTML report)

**Configuration (`appsettings.json` → `"Evaluator"` section):**

| Key | Default | Description |
|-----|---------|-------------|
| `DatasetFile` | `Data/dataset.json` | Path to the evaluation dataset (linked from `python/dataset.json`) |
| `PromptFile` | `Data/prompt.yaml` | Path to the prompt-under-test YAML |
| `JsonOutputFile` | `Data/output.json` | Raw results output path |
| `HtmlOutputFile` | `Data/output.html` | HTML report output path |
| `MaxConcurrentTasks` | `3` | Bounded concurrency for parallel API calls |

## 📊 Prompt Quality Evolution

The following table shows the quality improvements of the [prompt.yaml](Data/prompt.yaml) file across iterations, measured by the PromptEvaluator against a standardized test dataset:

| Version | Changes | Score |
|---------|---------|-------|
| 1 | Initial version | 47% |
| 2 | Improve task description + add Extra criteria | 77% |
| 3 | Add specificity guideline | 77% |
| 4 | Chain-of-thought steps with output constraint | 70% |
| 5 | Structure with XML tag | 80% |

**Key learnings:**
- Adding explicit criteria (v2) yielded the largest improvement (+30 percentage points)
- Further specificity (v3) maintained quality without degradation
- Chain-of-thought approach (v4) unexpectedly decreased performance (-7 percentage points), suggesting over-constraining may reduce model flexibility — reverted
- XML tag structure (v5, based on v3) improved performance by +3 percentage points over v3, achieving a new high score of 80%

### Environment Strategy (all projects)

- **Production** (Default): High-quality responses with Sonnet model
- **Development**: Fast iteration with cost-effective Haiku model
- **Automatic switching**: Based on `ASPNETCORE_ENVIRONMENT` variable

## 🔑 Configuration

The .NET implementation uses environment-based configuration with **Production as default** (matching ASP.NET Core behavior):

**Production Environment** (`appsettings.json`) - **Default**:
- Model: `claude-sonnet-4-5` (high-quality responses)
- Used when no `ASPNETCORE_ENVIRONMENT` is set

**Development Environment** (`appsettings.Development.json`):
- Model: `claude-haiku-4-5` (fast and cost-effective)
- Used when `ASPNETCORE_ENVIRONMENT=Development`

**API Key Configuration** ([User Secrets][user-secrets], recommended for local dev):
```bash
dotnet user-secrets set "Anthropic:ApiKey" "your-api-key-here" --project AnthropicApiClient
dotnet user-secrets set "Anthropic:ApiKey" "your-api-key-here" --project BlazorChat
```

### GitHub Codespaces

`dotnet user-secrets` is a local-only mechanism. In Codespaces, use GitHub's native secrets instead — the .NET configuration system reads environment variables automatically, resolving `__` as the hierarchy separator (`Anthropic__ApiKey` → `Anthropic:ApiKey`).

**API key (secret, sensitive)**

Store it as a [user-level Codespace secret][gh-codespaces-secrets] so it is available across all your repositories without duplication:

1. GitHub profile → **Settings** → **Codespaces** → **New secret**
2. Name: `Anthropic__ApiKey`, Value: your key
3. Under **Repository access** select the repositories that may use it

**Non-sensitive config (model, etc.)**

Two options depending on scope:

| Scope | Mechanism | How |
|---|---|---|
| This repo only | `devcontainer.json` `remoteEnv` | Commit the values — they are versioned with the project |
| All your repos | [Dotfiles repo][gh-dotfiles] | Export variables from `~/.bashrc` in your personal `dotfiles` repo |

Example `.devcontainer/devcontainer.json` entry for repo-scoped defaults:

```json
{
  "remoteEnv": {
    "Anthropic__Model": "claude-haiku-4-5"
  }
}
```

### GitHub Actions (CI/CD)

For E2E tests or any pipeline step that calls the API, store the key as a [repository Actions secret][gh-actions-secrets] and inject it via the `env:` block using the same `__` naming convention:

```yaml
jobs:
  e2e-test:
    runs-on: ubuntu-latest
    env:
      Anthropic__ApiKey: ${{ secrets.ANTHROPIC_API_KEY }}
      ASPNETCORE_ENVIRONMENT: Development
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'
      - run: dotnet test "Claude with Anthropic API.slnx"
```

No `dotnet user-secrets` call is needed — `IConfiguration` picks up the environment variable automatically.

## 📋 Example Output

**Production (Sonnet - High Quality) - Default:**
```
Using model: claude-sonnet-4-5
API request successful!
Claude's response: Quantum computing is a type of computation that harnesses quantum mechanical phenomena like superposition and entanglement...
Response time: ~1900ms

Making second API call
API request successful!
Claude's response: Dependency injection is a design pattern where objects receive their dependencies from external sources rather than creating them internally...
Response time: ~1150ms
```

**Development (Haiku - Fast & Cost-effective):**
```
Using model: claude-haiku-4-5
API request successful!
Claude's response: Quantum computing is a type of computing that uses quantum bits (qubits) to process information in multiple states simultaneously...
Response time: ~850ms

Making second API call
API request successful!
Claude's response: Dependency injection is a design pattern where an object receives its dependencies from external sources rather than creating them itself...
Response time: ~800ms
```

## 🛠 Development

- Uses modern **.slnx** solution format
- Configured for **C# 14** with **.NET 10.0**
- **Environment-based configuration**:
  - Production: `claude-sonnet-4-5` (high quality, default)
  - Development: `claude-haiku-4-5` (fast, cost-effective)
- All build artifacts (`bin/`, `obj/`) are git-ignored
- Follows Microsoft's latest best practices

**Switching Environments:**
```bash
# Production (default - no environment variable needed)
dotnet run

# Development
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

## 📁 Project Structure

```
dotnet/
├── Claude with Anthropic API.slnx  # Solution file (.slnx format)
├── AnthropicShared/                # Shared class library
│   ├── AnthropicClient.cs          # HTTP client implementation
│   ├── AnthropicOptions.cs         # Typed configuration model
│   ├── IAntropicClient.cs          # Conversation interface
│   └── _usings.cs                  # Global using directives
├── AnthropicApiClient/             # Console application
│   ├── Application.cs              # Main business logic
│   ├── Program.cs                  # Entry point
│   ├── Startup.cs                  # DI configuration
│   ├── appsettings.json            # Production configuration (Sonnet)
│   └── appsettings.Development.json # Development configuration (Haiku)
├── BlazorChat/                     # Blazor Server chat UI
│   ├── Pages/Home.razor(.cs)       # Chat page with streaming
│   ├── Components/                 # Chat history, input, config panel
│   ├── Services/                   # Stream progress, user settings, options
│   ├── appsettings.json            # Production configuration (Sonnet)
│   └── appsettings.Development.json # Development configuration (Haiku)
└── PromptEvaluator/                # Prompt evaluation console app
    ├── Application.cs              # Entry point: load → evaluate → report
    ├── Program.cs                  # IHostBuilder setup
    ├── Startup.cs                  # DI registration (all Transient)
    ├── Data/
    │   ├── prompt.yaml             # Prompt under test (task + template + criteria)
    │   ├── dataset.json            # Linked from ../../python/dataset.json
    │   ├── output.json             # Raw EvaluationResult[] (generated)
    │   └── output.html             # HTML report (generated)
    ├── Models/
    │   ├── EvaluationResult.cs     # Output + TestCase + Score + Reasoning
    │   ├── EvaluatorOptions.cs     # Typed config from "Evaluator" section
    │   ├── GradeResult.cs          # Grader LLM response (score, strengths, weaknesses)
    │   ├── PromptConfig.cs         # YAML prompt definition
    │   └── TestCase.cs             # One dataset entry (scenario, inputs, criteria)
    ├── Services/
    │   ├── PromptEvaluatorService.cs  # Concurrent evaluation pipeline
    │   └── ReportGenerator.cs         # HTML report builder
    ├── appsettings.json            # Production configuration (Sonnet)
    └── appsettings.Development.json # Development configuration (Haiku)
```

---

For the complete project including Python implementation, see the [Main Project README][main-readme].