[python-readme]: python/README.md "Complete Python Documentation"
[dotnet-readme]: dotnet/README.md "Complete .NET Documentation" 
[python-config]: python/README.md#configuration "Python Configuration Guide"
[dotnet-config]: dotnet/README.md#configuration ".NET Configuration Guide"
[python-examples]: python/README.md "Complete Python Documentation"
[dotnet-examples]: dotnet/README.md#example-output ".NET Example Output"
[python-dev]: python/README.md "Python Development Guide"
[dotnet-dev]: dotnet/README.md#development ".NET Development Guide"
[anthropic-console]: https://console.anthropic.com/ "Anthropic Console"

# Claude with Anthropic API

This project demonstrates how to integrate with the Anthropic Claude API using both Python and .NET implementations.

## 📁 Project Structure

```
├── python/                             # Python implementation
│   ├── README.md                      # Python-specific documentation
│   ├── claude_client.py               # Stateful SDK client with context management
│   ├── anthropic_client.py            # Stateless SDK wrapper
│   ├── anthropic_config.py            # Config singleton (lazy env var loading)
│   ├── grader.py                      # LLM-as-judge grader helper
│   ├── prompt_runner.py               # Prompt template runner
│   ├── examples.py                    # Example conversation script
│   ├── chat_cli.py                    # Interactive terminal chat
│   ├── 001-requests.ipynb             # Raw HTTP calls (no SDK)
│   ├── 002-prompt-evaluation.ipynb    # End-to-end evaluation pipeline
│   ├── 003_dataset_generator.ipynb    # Dataset generation via Claude
│   ├── 003_prompting.ipynb            # Prompting technique experiments
│   ├── dataset.json                   # Evaluation dataset (shared with .NET)
│   ├── .env                           # Environment variables (API key)
│   └── .venv/                         # Virtual environment
├── dotnet/                             # .NET implementation
│   ├── README.md                      # .NET-specific documentation
│   ├── Claude with Anthropic API.slnx # Solution file (.slnx format)
│   ├── AnthropicShared/               # Shared class library
│   ├── AnthropicApiClient/            # Console application
│   ├── BlazorChat/                    # Blazor Server chat UI
│   └── PromptEvaluator/               # Prompt evaluation console app
└── README.md                          # This file
```

## 🐍 Python Implementation

A simple script-based approach for learning and experimentation.

**Key Features:**
- Shared Anthropic client utilities
- Example and interactive terminal entry points
- Environment variable configuration
- Straightforward local script execution

**Quick Start:**
```bash
cd python
python -m venv .venv
.venv\Scripts\activate
pip install -r requirements.txt
python examples.py
```

📖 **[Complete Python Documentation][python-readme]**

## 🔷 .NET Implementation

Enterprise-grade .NET projects with modern patterns and environment-based configuration.

**Key Features:**
- .NET 10.0 with C# 14
- Dependency Injection & IOptions / IOptionsMonitor pattern
- Environment-based configuration (Development/Production)
- Structured logging & HTTP client factory
- Blazor Server streaming chat UI
- Prompt evaluation with LLM-as-judge grading

**Projects:**
- **AnthropicApiClient** — console app demonstrating basic API usage
- **BlazorChat** — Blazor Server chat UI with real-time SSE streaming
- **PromptEvaluator** — console app: YAML prompts, JSON datasets, HTML reports

**Quick Start (AnthropicApiClient):**
```bash
cd dotnet
dotnet user-secrets set "Anthropic:ApiKey" "your-key" --project AnthropicApiClient
dotnet run --project AnthropicApiClient
```

**Quick Start (PromptEvaluator):**
```bash
cd dotnet
dotnet user-secrets set "Anthropic:ApiKey" "your-key" --project PromptEvaluator
dotnet run --project PromptEvaluator
```

📖 **[Complete .NET Documentation][dotnet-readme]**

## 🔑 Configuration

Both implementations require an Anthropic API key from the [Anthropic Console][anthropic-console].

**Python**: Set `ANTHROPIC_API_KEY` in `.env` file  
**See**: [Python Configuration Guide][python-config]

**.NET**: Use User Secrets for secure configuration  
**See**: [.NET Configuration Guide][dotnet-config]

## 🚀 Getting Started

1. Get your API key from [Anthropic Console][anthropic-console]
2. Choose your preferred implementation (Python or .NET)
3. Follow the setup instructions above
4. Run the application and see Claude AI responses!

## 📋 Example Output

Both implementations demonstrate API calls to Claude with responses about:
- Quantum computing concepts
- Dependency injection patterns

**Python**: Traditional Python scripts with example and chat entry points  
**See**: [Python Examples][python-examples]

**.NET**: Console output with structured logging and environment-specific models  
**See**: [.NET Examples][dotnet-examples]

## 🛠 Development

Choose your preferred implementation approach:

### Python Development
- **Simple script workflow** without notebook-specific input issues
- **Shared client module** for reuse across examples and chat
- **Simple setup** with virtual environments

📖 **[Python Development Guide][python-dev]**

### .NET Development
- **Enterprise architecture** with modern patterns
- **Environment-based configuration** (Development/Production)
- **ASP.NET Core compatible** structure

📖 **[.NET Development Guide][dotnet-dev]**

---

Built with ❤️ for learning Anthropic Claude API integration