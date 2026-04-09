[python-readme]: python/README.md "Complete Python Documentation"
[dotnet-readme]: dotnet/README.md "Complete .NET Documentation" 
[python-config]: python/README.md#configuration "Python Configuration Guide"
[dotnet-config]: dotnet/README.md#configuration ".NET Configuration Guide"
[python-examples]: python/README.md#example-output "Python Example Output"
[dotnet-examples]: dotnet/README.md#example-output ".NET Example Output"
[python-dev]: python/README.md#development "Python Development Guide"
[dotnet-dev]: dotnet/README.md#development ".NET Development Guide"
[anthropic-console]: https://console.anthropic.com/ "Anthropic Console"

# Claude with Anthropic API

This project demonstrates how to integrate with the Anthropic Claude API using both Python and .NET implementations.

## 📁 Project Structure

```
├── python/                     # Python implementation
│   ├── README.md              # Python-specific documentation
│   ├── 001-requests.ipynb     # Jupyter notebook with API examples
│   ├── .env                   # Environment variables (API key)
│   └── .venv/                 # Virtual environment
├── dotnet/                     # .NET implementation
│   ├── README.md              # .NET-specific documentation
│   ├── Claude with Anthropic API.slnx  # Solution file (.slnx format)
│   └── AnthropicApiClient/    # Console application
│       ├── AnthropicApiClient.csproj
│       ├── Program.cs         # Entry point with environment detection
│       ├── Application.cs     # Main business logic
│       ├── AnthropicClient.cs # API client implementation
│       ├── Startup.cs         # DI configuration
│       ├── appsettings.json   # Production configuration (Sonnet)
│       └── appsettings.Development.json # Development configuration (Haiku)
└── README.md                  # This file
```

## 🐍 Python Implementation

A simple, interactive approach using Jupyter notebooks for learning and experimentation.

**Key Features:**
- Jupyter notebook with step-by-step examples
- Direct API calls using requests library
- Environment variable configuration
- Interactive development environment

**Quick Start:**
```bash
cd python
python -m venv .venv
.venv\Scripts\activate
pip install requests python-dotenv jupyter
jupyter notebook 001-requests.ipynb
```

📖 **[Complete Python Documentation][python-readme]**

## 🔷 .NET Implementation

Enterprise-grade console application with modern .NET patterns and environment-based configuration.

**Key Features:**
- .NET 10.0 with C# 13
- Dependency Injection & IOptions pattern
- Environment-based configuration (Development/Production)
- Structured logging & HTTP client factory
- ASP.NET Core compatible architecture

**Quick Start:**
```bash
cd dotnet
dotnet user-secrets set "Anthropic:ApiKey" "your-key" --project AnthropicApiClient
dotnet run --project AnthropicApiClient
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

**Python**: Interactive Jupyter notebook with step-by-step examples  
**See**: [Python Examples][python-examples]

**.NET**: Console output with structured logging and environment-specific models  
**See**: [.NET Examples][dotnet-examples]

## 🛠 Development

Choose your preferred implementation approach:

### Python Development
- **Interactive learning** with Jupyter notebooks
- **Simple setup** with virtual environments
- **Immediate feedback** for experimentation

📖 **[Python Development Guide][python-dev]**

### .NET Development
- **Enterprise architecture** with modern patterns
- **Environment-based configuration** (Development/Production)
- **ASP.NET Core compatible** structure

📖 **[.NET Development Guide][dotnet-dev]**

---

Built with ❤️ for learning Anthropic Claude API integration