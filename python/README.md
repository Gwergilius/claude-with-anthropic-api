[anthropic-console]: https://console.anthropic.com/ "Anthropic Console"
[main-readme]: ../README.md "Main Project README"
[gh-codespaces-secrets]: https://docs.github.com/en/codespaces/setting-your-user-preferences/managing-your-account-specific-secrets-for-github-codespaces "Managing your account-specific secrets for GitHub Codespaces"
[gh-codespaces-variables]: https://docs.github.com/en/codespaces/setting-up-your-project-for-codespaces/configuring-dev-containers/specifying-recommended-secrets-for-a-repository "GitHub Codespaces repository variables"
[gh-dotfiles]: https://docs.github.com/en/codespaces/setting-your-user-preferences/personalizing-github-codespaces-for-your-account#dotfiles "Personalizing Codespaces with dotfiles"
[devcontainer-spec]: https://containers.dev/implementors/json_reference/ "Dev Container JSON reference"
# Python Implementation - Claude with Anthropic API

This is the Python implementation of the Claude API integration project.

## Features

- Traditional Python scripts instead of a notebook workflow
- Anthropic SDK integration with conversation context management
- Environment variable configuration through `.env`
- Separate entry points for examples and interactive terminal chat

## Requirements

- Python 3.10+
- `anthropic`
- `python-dotenv`

## Setup

1. Create and activate a virtual environment:
   ```bash
   python -m venv .venv
   .venv\Scripts\activate  # Windows
   # or
   source .venv/bin/activate  # macOS/Linux
   ```

2. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

3. Set your API key and model in `.env`:
   ```bash
   Anthropic__ApiKey=your-api-key-here
   Anthropic__Model=claude-haiku-4-5
   ```

## Usage

Run the two-turn example conversation:

```bash
python examples.py
```

Run the interactive terminal chat:

```bash
python chat_cli.py
```

## Files

- `claude_client.py` contains settings loading, immutable message types, and the chat client wrapper.
- `examples.py` runs the same example prompts that were previously in the notebook.
- `chat_cli.py` provides a normal terminal-based chat loop.
- `requirements.txt` lists the Python dependencies.

## Configuration

### Local development

Create a `.env` file in the python directory:

```text
Anthropic__ApiKey=your-api-key-here
Anthropic__Model=claude-haiku-4-5
```

Get your API key from the [Anthropic Console][anthropic-console].

### GitHub Codespaces

In Codespaces the `.env` file is not needed — use GitHub's built-in mechanisms instead.

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

Example `~/.bashrc` snippet in a [dotfiles repo][gh-dotfiles] for global defaults:

```bash
export Anthropic__Model="claude-haiku-4-5"
```

## Project Structure

```text
python/
├── .env
├── README.md
├── chat_cli.py
├── claude_client.py
├── examples.py
└── requirements.txt
```

## Tips

- Use `examples.py` when you want a deterministic smoke test.
- Use `chat_cli.py` when you want interactive input without notebook input issues.
- Change `Anthropic__Model` in `.env` to compare supported Claude models.

---

For the complete project including .NET implementation, see the [Main Project README][main-readme].