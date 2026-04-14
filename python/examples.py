"""Example Claude API interactions using the shared client utilities."""

from claude_client import AnthropicClient


def main() -> None:
    """Run the same two-turn example conversation that existed in the notebook."""

    client = AnthropicClient.create()

    print("API key loaded successfully")
    print(f"Using model: {client.model}")

    prompts = [
        "Define quantum computing in one sentence",
        "Write another sentence.",
    ]

    for prompt in prompts:
        response = client.chat(prompt)
        print(f"Message: {prompt}")
        print(f"\nClaude's response: {response}")
        print("\n" + "=" * 60)
        client.print_context()


if __name__ == "__main__":
    main()