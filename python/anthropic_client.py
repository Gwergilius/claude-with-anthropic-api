"""Anthropic SDK client with model configuration and context management."""

from __future__ import annotations

import datetime
import textwrap
from pathlib import Path

import anthropic

from anthropic_config import AnthropicConfig
from anthropic_message import AnthropicMessage, AssistantMessage, UserMessage


class AnthropicClient(anthropic.Anthropic):
    """Anthropic SDK client with model configuration and context management."""

    version: str = datetime.datetime.fromtimestamp(
        Path(__file__).stat().st_mtime
    ).strftime("%Y-%m-%d %H:%M:%S")

    def __init__(self, config: AnthropicConfig):
        super().__init__(api_key=config.api_key)
        self._config = config
        self._context: list[AnthropicMessage] = []

    @classmethod
    def create(cls, env_path: Path | None = None) -> "AnthropicClient":
        """Create a client instance from environment settings (factory method)."""

        return cls(config=AnthropicConfig(env_path=env_path))

    @property
    def model(self) -> str:
        return self._config.model

    @property
    def context(self) -> tuple[AnthropicMessage, ...]:
        """Return a read-only snapshot of the conversation context."""

        return tuple(self._context)

    @staticmethod
    def printLong(txt: str, left: int = 0, wrapPos: int = 120):
        """Print text with long-line wrapping at wrapPos characters.
        Each line in the input is wrapped independently.
        """
        indent = " " * left
        for line in txt.splitlines():
            print(
                textwrap.fill(
                    line,
                    width=wrapPos - left,
                    initial_indent=indent,
                    subsequent_indent=indent,
                )
                if line
                else ""
            )

    def print_context(self) -> None:
        """Print the current context in a human-readable format."""

        if not self._context:
            print("Context is empty")
            return

        print(f"Context of ({len(self._context)} messages):")
        print("-" * 50)
        for message in self._context:
            print(message.role.title())
            self.printLong(message.content, left=4, wrapPos=self._config.word_wrap)

        print("-" * 50)

    def reset_context(self) -> None:
        """Remove all messages from the current conversation."""

        self._context.clear()

    def append_message(self, message: AnthropicMessage) -> None:
        """Append a message to the conversation context."""

        self._context.append(message)

    def get_response(self, stop_sequences: list[str] | None = None) -> str:
        """Send the current context to Claude and return the response text."""

        if not self._context:
            raise ValueError("Context is empty - add messages before sending")

        print(f"Sending {len(self._context)} messages to Claude")
        self.print_context()

        response = self.messages.create(
            model=self._config.model,
            max_tokens=self._config.max_tokens,
            temperature=self._config.temperature,
            messages=self._context,
            stop_sequences=stop_sequences,
        )

        response_text = response.content[0].text
        print("API request successful")
        self.printLong(response_text, left=4, wrapPos=self._config.word_wrap)
        return response_text

    def chat(self, message: str) -> str:
        """Send a user message and append Claude's reply to the context."""

        user_message = UserMessage(content=message)
        self._context.append(user_message)
        response = self.get_response()
        assistant_message = AssistantMessage(content=response)
        self._context.append(assistant_message)
        return response
