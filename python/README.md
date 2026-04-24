[anthropic-console]: https://console.anthropic.com/ "Anthropic Console"
[main-readme]: ../README.md "Main Project README"
[gh-codespaces-secrets]: https://docs.github.com/en/codespaces/setting-your-user-preferences/managing-your-account-specific-secrets-for-github-codespaces "Managing your account-specific secrets for GitHub Codespaces"
[gh-codespaces-variables]: https://docs.github.com/en/codespaces/setting-up-your-project-for-codespaces/configuring-dev-containers/specifying-recommended-secrets-for-a-repository "GitHub Codespaces repository variables"
[gh-dotfiles]: https://docs.github.com/en/codespaces/setting-your-user-preferences/personalizing-github-codespaces-for-your-account#dotfiles "Personalizing Codespaces with dotfiles"
[devcontainer-spec]: https://containers.dev/implementors/json_reference/ "Dev Container JSON reference"
# Python Implementation - Claude with Anthropic API

This is the Python implementation of the Claude API integration project.

## Features

- Stateful SDK client with rolling conversation context
- Stateless SDK wrapper for single-turn prompt evaluation calls
- Prompt evaluation pipeline: template rendering → Claude → LLM-as-judge grading → HTML report
- Dataset generation via Claude for automated test case creation
- Environment variable configuration through `.env`
- Separate entry points for examples, interactive chat, and evaluation notebooks

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

Run the prompt evaluation pipeline (Jupyter):

```bash
jupyter notebook 002-prompt-evaluation.ipynb
```

## Files

### Client modules

- `anthropic_config.py` — `AnthropicConfig` singleton: reads `Anthropic__*` env vars from `.env`, provides lazy-loaded `client` and `model` properties.
- `anthropic_client.py` — stateless thin wrapper around the Anthropic SDK; no conversation context; used by `grader.py` and `prompt_runner.py`.
- `anthropic_message.py` — immutable `AnthropicMessage` dataclass (role + content).
- `claude_client.py` — stateful SDK client with rolling conversation context; also contains settings loading.
- `immutable_dict.py` — `ImmutableDict` type used for prompt inputs.

### Evaluation helpers

- `grader.py` — `LlmGrader`: LLM-as-judge helper; scores a `(test_case, output)` pair via Claude; returns score + reasoning.
- `prompt_runner.py` — `PromptRunner`: fills `{key}` template placeholders and calls Claude; returns raw output.
- `dataset.json` — evaluation dataset; shared with the `PromptEvaluator` .NET project via `Content Include` in `.csproj`.

### Entry points

- `examples.py` — runs the same example prompts that were previously in the notebook.
- `chat_cli.py` — interactive terminal-based chat loop.

### Notebooks

- `001-requests.ipynb` — raw HTTP calls (no SDK), quick experimentation.
- `001_prompt_evals_complete.ipynb` — complete prompt evaluation walkthrough (all-in-one reference).
- `002-prompt-evaluation.ipynb` — end-to-end evaluation pipeline: render prompts → call Claude → grade → write report.
- `003_dataset_generator.ipynb` — generates `dataset.json` via Claude (diverse test cases for a given task).
- `003_prompting.ipynb` — prompting technique experiments.

### Other

- `requirements.txt` — Python dependencies.

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
├── .env                            # API key and model config
├── README.md
├── requirements.txt
├── claude_client.py                # Stateful SDK client (conversation context)
├── anthropic_client.py             # Stateless SDK wrapper
├── anthropic_config.py             # Config singleton (lazy env var loading)
├── anthropic_message.py            # Immutable AnthropicMessage dataclass
├── examples.py                     # Two-turn example conversation
├── chat_cli.py                     # Interactive terminal chat
├── grader.py                       # LLM-as-judge grader helper
├── prompt_runner.py                # Prompt template runner
├── immutable_dict.py               # ImmutableDict type for prompt inputs
├── dataset.json                    # Evaluation dataset (shared with .NET)
├── 001-requests.ipynb              # Raw HTTP calls (no SDK)
├── 001_prompt_evals_complete.ipynb # Complete evaluation walkthrough
├── 002-prompt-evaluation.ipynb     # End-to-end evaluation pipeline
├── 003_dataset_generator.ipynb     # Dataset generation via Claude
└── 003_prompting.ipynb             # Prompting technique experiments
```

## Tips

- Use `examples.py` when you want a deterministic smoke test.
- Use `chat_cli.py` when you want interactive input without notebook input issues.
- Change `Anthropic__Model` in `.env` to compare supported Claude models.
- Run `003_dataset_generator.ipynb` first to regenerate `dataset.json` before re-running the evaluation pipeline.
- `dataset.json` is shared with the `PromptEvaluator` .NET project — changes here are automatically picked up on the next .NET build.

---

For the complete project including .NET implementation, see the [Main Project README][main-readme].