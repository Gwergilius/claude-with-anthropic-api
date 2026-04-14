"""Interactive terminal chat for Claude."""

from claude_client import create_client, load_settings


def main() -> None:
    """Start an interactive console chat loop."""

    settings = load_settings()
    client = create_client()

    print(f"Using model: {settings.model}")
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