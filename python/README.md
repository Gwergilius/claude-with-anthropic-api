[anthropic-console]: https://console.anthropic.com/ "Anthropic Console"
[main-readme]: ../README.md "Main Project README"
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
   ANTHROPIC_API_KEY=your-api-key-here
   ANTHROPIC_MODEL=claude-haiku-4-5
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

Create a `.env` file in the python directory:

```text
ANTHROPIC_API_KEY=your-api-key-here
ANTHROPIC_MODEL=claude-haiku-4-5
```

Get your API key from the [Anthropic Console][anthropic-console].

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
- Change `ANTHROPIC_MODEL` in `.env` to compare supported Claude models.

---

For the complete project including .NET implementation, see the [Main Project README][main-readme].