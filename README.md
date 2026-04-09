# Claude with Anthropic API

This project demonstrates how to integrate with the Anthropic Claude API using both Python and .NET implementations.

## 📁 Project Structure

```
├── python/                     # Python implementation
│   ├── 001-requests.ipynb     # Jupyter notebook with API examples
│   └── .venv/                 # Virtual environment
├── dotnet/                     # .NET implementation
│   ├── Claude with Anthropic API.slnx  # Solution file (.slnx format)
│   └── AnthropicApiClient/    # Console application
│       ├── AnthropicApiClient.csproj
│       ├── Program.cs         # Entry point
│       ├── Application.cs     # Main business logic
│       ├── AnthropicClient.cs # API client implementation
│       ├── Startup.cs         # DI configuration
│       └── appsettings.json   # Configuration file
└── README.md                  # This file
```

## 🐍 Python Implementation

Located in the `python/` directory.

### Features
- Jupyter notebook with interactive examples
- Direct API calls using requests library
- Environment variable configuration

### Setup
```bash
cd python
python -m venv .venv
.venv\Scripts\activate  # Windows
pip install -r requirements.txt
```

### Usage
Open `001-requests.ipynb` in Jupyter and run the cells.

## 🔷 .NET Implementation

Located in the `dotnet/` directory.

### Features
- **.NET 10.0** Console application
- **C# 13** with modern language features (Primary constructors)
- **Enterprise-grade architecture**:
  - Dependency Injection (Microsoft.Extensions.DependencyInjection)
  - Configuration management (IOptions pattern)
  - Structured logging (ILogger)
  - HTTP client factory pattern
  - User Secrets for API key security

### Technologies Used
- .NET 10.0
- Microsoft.Extensions.* ecosystem (10.0.0 packages)
- System.Text.Json for JSON serialization
- HttpClientFactory for HTTP communication

### Setup
```bash
cd dotnet
dotnet user-secrets set "Anthropic:ApiKey" "your-api-key-here" --project AnthropicApiClient
dotnet build "Claude with Anthropic API.slnx"
```

### Usage
```bash
cd dotnet
dotnet run --project AnthropicApiClient
```

### Architecture

The .NET implementation follows clean architecture principles:

- **Program.cs**: Application bootstrap
- **Startup.cs**: Dependency injection configuration
- **Application.cs**: Business logic orchestration
- **AnthropicClient.cs**: API client with modern .NET patterns
- **AnthropicOptions.cs**: Configuration model
- **IAntropicClient.cs**: Interface for testability

## 🔑 Configuration

Both implementations require an Anthropic API key:

### Python
Set environment variable:
```bash
export ANTHROPIC_API_KEY="your-api-key-here"
```

### .NET
Use User Secrets (recommended for development):
```bash
dotnet user-secrets set "Anthropic:ApiKey" "your-api-key-here" --project dotnet/AnthropicApiClient
```

## 🚀 Getting Started

1. Get your API key from [Anthropic Console](https://console.anthropic.com/)
2. Choose your preferred implementation (Python or .NET)
3. Follow the setup instructions above
4. Run the application and see Claude AI responses!

## 📋 Example Output

Both implementations make API calls to Claude and display responses for:
- Question about quantum computing
- Question about dependency injection

Sample output:
```
API request successful!
Claude's response: Quantum computing is a type of computation that harnesses quantum mechanical phenomena...

Making second API call
API request successful!
Claude's response: Dependency injection is a design pattern where an object receives its dependencies...
```

## 🛠 Development

### .NET Development
- Uses modern .slnx solution format
- Configured for C# 13 with .NET 10.0
- All build artifacts (`bin/`, `obj/`) are git-ignored
- Follows Microsoft's latest best practices

### Python Development
- Uses virtual environment for isolation
- Jupyter notebook for interactive development
- Environment variables for configuration

---

Built with ❤️ for learning Anthropic Claude API integration