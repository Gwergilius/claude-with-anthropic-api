"""Utilities for chatting with Claude from traditional Python scripts."""

from __future__ import annotations

import os
from dataclasses import dataclass
from pathlib import Path

import anthropic
from dotenv import load_dotenv


@dataclass(frozen=True)
class AnthropicSettings:
    """Application settings loaded from environment variables."""

    api_key: str
    model: str


def load_settings(env_path: Path | None = None) -> AnthropicSettings:
    """Load Anthropic settings from a .env file and process environment."""

    resolved_env_path = env_path or Path(__file__).with_name(".env")
    load_dotenv(resolved_env_path)

    api_key = os.getenv("Anthropic__ApiKey")
    model = os.getenv("Anthropic__Model", "claude-sonnet-4-5")

    if not api_key:
        raise ValueError(
            "Anthropic__ApiKey not found. Set it in python/.env or as an environment variable."
        )

    return AnthropicSettings(api_key=api_key, model=model)


class ImmutableDict(dict):
    """A dictionary that cannot be modified after initialization."""

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        object.__setattr__(self, "_immutable", True)

    def __setitem__(self, key, value):
        if getattr(self, "_immutable", False):
            raise TypeError(f"ImmutableDict is immutable - cannot modify '{key}'")
        super().__setitem__(key, value)

    def __delitem__(self, key):
        if getattr(self, "_immutable", False):
            raise TypeError("ImmutableDict is immutable - cannot delete items")
        super().__delitem__(key)

    def clear(self):
        raise TypeError("ImmutableDict is immutable - cannot clear")

    def pop(self, key, default=None):
        raise TypeError("ImmutableDict is immutable - cannot pop items")

    def popitem(self):
        raise TypeError("ImmutableDict is immutable - cannot pop items")

    def setdefault(self, key, default=None):
        raise TypeError("ImmutableDict is immutable - cannot set defaults")

    def update(self, *args, **kwargs):
        raise TypeError("ImmutableDict is immutable - cannot update")


class AnthropicMessage(ImmutableDict):
    """Immutable message object for Anthropic API calls."""

    def __init__(self, role: str, content: str):
        super().__init__(role=role, content=content)

    @property
    def role(self) -> str:
        return self["role"]

    @property
    def content(self) -> str:
        return self["content"]


class UserMessage(AnthropicMessage):
    """Immutable helper type for user messages."""

    def __init__(self, content: str):
        super().__init__(role="user", content=content)


class AssistantMessage(AnthropicMessage):
    """Immutable helper type for assistant messages."""

    def __init__(self, content: str):
        super().__init__(role="assistant", content=content)


class AnthropicClient(anthropic.Anthropic):
    """Anthropic SDK client with model configuration and context management."""

    def __init__(self, api_key: str, model: str):
        super().__init__(api_key=api_key)
        self.model = model
        self._context: list[AnthropicMessage] = []

    @property
    def context(self) -> tuple[AnthropicMessage, ...]:
        """Return a read-only snapshot of the conversation context."""

        return tuple(self._context)

    def reset_context(self) -> None:
        """Remove all messages from the current conversation."""

        self._context.clear()

    def chat(self, message: str) -> str:
        """Send a user message and append Claude's reply to the context."""

        user_message = UserMessage(content=message)
        self._context.append(user_message)

        print(f"Sending {len(self._context)} messages to Claude")

        response = self.messages.create(
            model=self.model,
            max_tokens=1000,
            temperature=0,
            messages=self._context,
        )

        response_text = response.content[0].text
        assistant_message = AssistantMessage(content=response_text)
        self._context.append(assistant_message)

        print("API request successful")
        return response_text

    def print_context(self) -> None:
        """Print the current context in a human-readable format."""

        if not self._context:
            print("Context is empty")
            return

        print(f"Context ({len(self._context)} messages):")
        print("-" * 50)

        for index, message in enumerate(self._context, start=1):
            print(f"{index}. {message.role.title()}: {message.content}")

        print("-" * 50)


def create_client(env_path: Path | None = None) -> AnthropicClient:
    """Create a configured Anthropic client from environment settings."""

    settings = load_settings(env_path=env_path)
    return AnthropicClient(api_key=settings.api_key, model=settings.model)