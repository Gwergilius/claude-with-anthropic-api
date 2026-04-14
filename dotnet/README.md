[anthropic-console]: https://console.anthropic.com/ "Anthropic Console"
[main-readme]: ../README.md "Main Project README"
[user-secrets]: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets "ASP.NET Core User Secrets"
[gh-codespaces-secrets]: https://docs.github.com/en/codespaces/setting-your-user-preferences/managing-your-account-specific-secrets-for-github-codespaces "Managing your account-specific secrets for GitHub Codespaces"
[gh-dotfiles]: https://docs.github.com/en/codespaces/setting-your-user-preferences/personalizing-github-codespaces-for-your-account#dotfiles "Personalizing Codespaces with dotfiles"
[gh-actions-secrets]: https://docs.github.com/en/actions/security-for-github-actions/security-guides/using-secrets-in-github-actions "Using secrets in GitHub Actions"

# .NET Implementation - Claude with Anthropic API

This is the .NET implementation of the Claude API integration project.

## 🔷 Features

- **.NET 10.0** Console application
- **C# 13** with modern language features (Primary constructors)
- **Enterprise-grade architecture**:
  - Dependency Injection (Microsoft.Extensions.DependencyInjection)
  - Configuration management (IOptions pattern)
  - Environment-based configuration (Development vs Production)
  - Structured logging (ILogger)
  - HTTP client factory pattern
  - User Secrets for API key security

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

The .NET implementation follows clean architecture principles with environment-based configuration:

**Core Components:**
- **Program.cs**: Application bootstrap with environment detection
- **Startup.cs**: Dependency injection configuration
- **Application.cs**: Business logic orchestration  
- **AnthropicClient.cs**: API client with modern .NET patterns
- **AnthropicOptions.cs**: Configuration model
- **IAntropicClient.cs**: Interface for testability

**Configuration Files:**
- **appsettings.json**: Production settings (Sonnet model)
- **appsettings.Development.json**: Development settings (Haiku model)

**Environment Strategy:**
- **Production** (Default): High-quality responses with Sonnet model  
- **Development**: Fast iteration with cost-effective Haiku model
- **ASP.NET Core Compatible**: Matches WebHost/HostBuilder default behavior
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
- Configured for **C# 13** with **.NET 10.0**
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
└── AnthropicApiClient/             # Console application
    ├── AnthropicApiClient.csproj
    ├── Program.cs                  # Entry point with environment detection
    ├── Application.cs              # Main business logic
    ├── AnthropicClient.cs          # API client implementation
    ├── Startup.cs                  # DI configuration
    ├── appsettings.json            # Production configuration (Sonnet)
    └── appsettings.Development.json # Development configuration (Haiku)
```

---

For the complete project including Python implementation, see the [Main Project README][main-readme].