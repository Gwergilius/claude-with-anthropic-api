"""Interactive terminal chat for Claude."""

from claude_client import AnthropicClient


def main() -> None:
    """Start an interactive console chat loop."""

    client = AnthropicClient.create()

    print(f"Using model: {client.model}")
    print("Claude chat - type 'exit' to quit.")
    print()

    while True:
        try:
            user_input = input("> ").strip()
        except (EOFError, KeyboardInterrupt):
            print("\nExiting chat.")
            break

        if user_input.lower() == "exit":
            break

        if not user_input:
            continue

        response = client.chat(user_input)
        print(response)
        print()


if __name__ == "__main__":
    main()