# Claude Project Guidelines

This document outlines the coding and documentation standards for the Claude API integration project.

## 📝 Documentation Standards

### Language Requirements

- **All documentation must be written in English**
  - README files, code comments, commit messages
  - API documentation, inline comments
  - Variable names, function names, class names
  - Error messages and log output

### Link Formatting Standards

- **Use reference-style links in all Markdown files**
  - Define all links at the top of each document
  - Use descriptive reference names
  - Include tooltips for better accessibility

**✅ Correct Format:**
```markdown
[anthropic-console]: https://console.anthropic.com/ "Anthropic Console"
[dotnet-readme]: dotnet/README.md "Complete .NET Documentation"

Visit the [Anthropic Console][anthropic-console] to get your API key.
See the [.NET Documentation][dotnet-readme] for implementation details.
```

**❌ Incorrect Format:**
```markdown
Visit the [Anthropic Console](https://console.anthropic.com/) to get your API key.
See the [.NET Documentation](dotnet/README.md) for implementation details.
```

### Link Definition Guidelines

1. **Group links by category** at the top of each file:
   - External services (APIs, documentation sites)
   - Internal documentation (other README files)
   - Section anchors within the same document

2. **Use descriptive reference names**:
   - `[anthropic-console]` instead of `[link1]`
   - `[python-config]` instead of `[config]`
   - `[dotnet-examples]` instead of `[examples]`

3. **Include meaningful tooltips**:
   - `"Anthropic Console"` for external services
   - `"Python Configuration Guide"` for documentation sections
   - `"Complete .NET Documentation"` for README files

### Documentation Structure

- **Consistent emoji usage** for section headers
- **Clear hierarchy** with proper heading levels
- **Code blocks** with appropriate syntax highlighting
- **Table of contents** for longer documents

## 💻 Code Standards

### Naming Conventions

- **English names only** for all identifiers
- **Descriptive names** over abbreviated ones
- **Follow platform conventions**:
  - .NET: PascalCase for public members, camelCase for private
  - Python: snake_case for functions and variables

### Comments and Documentation

- **All code comments in English**
- **XML documentation** for .NET public APIs
- **Docstrings** for Python functions and classes
- **Inline comments** for complex logic explanation

### Configuration and Environment

- **English-only** configuration keys and values
- **Descriptive environment variable names**
- **English error messages** and log output

## 🔧 Implementation Examples

### Reference Link Examples

```markdown
<!-- Link definitions at top of file -->
[api-docs]: https://docs.anthropic.com/en/api "Anthropic API Documentation"
[setup-guide]: setup/README.md "Setup and Installation Guide"
[config-section]: #configuration "Configuration Section"

<!-- Usage in document -->
Check the [API documentation][api-docs] for rate limits.
Follow our [setup guide][setup-guide] for quick installation.
See the [configuration section][config-section] below.
```

### Code Comment Examples

```csharp
/// <summary>
/// Sends a message to Claude API and returns the response.
/// </summary>
/// <param name="message">The user message to send</param>
/// <returns>Claude's response content</returns>
public async Task<string> SendMessage(string message)
{
    // Configure request with current model settings
    var request = CreateApiRequest(message);
    
    // Send to Anthropic API with retry logic
    return await SendWithRetry(request);
}
```

```python
def create_api_request(message: str) -> dict:
    """
    Creates a properly formatted API request for Anthropic Claude.
    
    Args:
        message: The user message to send to Claude
        
    Returns:
        Dictionary containing the formatted API request
    """
    # Build request payload with current configuration
    return build_request_payload(message)
```

## 🎯 Compliance Checklist

Before committing any documentation changes, ensure:

- [ ] All text is written in English
- [ ] Reference-style links are used throughout
- [ ] Link definitions are at the top of the file
- [ ] Tooltips are provided for all links
- [ ] Code comments are in English
- [ ] Variable/function names use English terms
- [ ] Error messages are in English
- [ ] Configuration keys use English naming

---

**Note**: This document should be updated as the project evolves. All contributors must follow these guidelines to maintain consistency and accessibility.