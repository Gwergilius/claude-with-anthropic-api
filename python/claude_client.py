"""Utilities for chatting with Claude from traditional Python scripts."""

from __future__ import annotations

import os
import textwrap
from pathlib import Path

import anthropic
from dotenv import load_dotenv


class AnthropicConfig:
    """Anthropic API configuration, loaded lazily from environment variables.

    Reads the following keys from the .env file (or the process environment):

    - ``Anthropic__ApiKey``      (required)
    - ``Anthropic__Model``       (default: ``claude-sonnet-4-5``)
    - ``Anthropic__ApiVersion``  (default: ``2023-06-01``)
    - ``Anthropic__MaxTokens``   (default: ``1000``)
    - ``Anthropic__Temperature`` (default: ``0``)
    - ``Anthropic__WordWrap``    (default: ``120``)
    """

    def __init__(self, env_path: Path | None = None):
        self._env_path = env_path or Path(__file__).with_name(".env")
        self._api_key: str | None = None
        self._model: str | None = None
        self._api_version: str | None = None
        self._max_tokens: int | None = None
        self._temperature: float | None = None
        self._word_wrap: int | None = None
        self._loaded = False

    def _ensure_loaded(self) -> None:
        if self._loaded:
            return
        load_dotenv(self._env_path)
        api_key = os.getenv("Anthropic__ApiKey")
        if not api_key:
            raise ValueError(
                "Anthropic__ApiKey not found. Set it in python/.env or as an environment variable."
            )
        self._api_key = api_key
        self._model = os.getenv("Anthropic__Model", "claude-sonnet-4-5")
        self._api_version = os.getenv("Anthropic__ApiVersion", "2023-06-01")
        self._max_tokens = int(os.getenv("Anthropic__MaxTokens", "1000"))
        self._temperature = float(os.getenv("Anthropic__Temperature", "0"))
        self._word_wrap = int(os.getenv("Anthropic__WordWrap", "120"))
        self._loaded = True

    @property
    def api_key(self) -> str:
        self._ensure_loaded()
        return self._api_key  # type: ignore[return-value]

    @property
    def model(self) -> str:
        self._ensure_loaded()
        return self._model  # type: ignore[return-value]

    @property
    def api_version(self) -> str:
        self._ensure_loaded()
        return self._api_version  # type: ignore[return-value]

    @property
    def max_tokens(self) -> int:
        self._ensure_loaded()
        return self._max_tokens  # type: ignore[return-value]

    @property
    def temperature(self) -> float:
        self._ensure_loaded()
        return self._temperature  # type: ignore[return-value]

    @property
    def word_wrap(self) -> int:
        self._ensure_loaded()
        return self._word_wrap  # type: ignore[return-value]

    def __str__(self) -> str:
        self._ensure_loaded()
        lines = []
        for name, member in type(self).__dict__.items():
            if isinstance(member, property) and not name.startswith("_"):
                value = getattr(self, name)
                if name == "api_key":
                    value = value[:8] + "..."
                lines.append(f"{name}: {value}")
        return "\n".join(lines)


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

    def reset_context(self) -> None:
        """Remove all messages from the current conversation."""

        self._context.clear()

    def chat(self, message: str) -> str:
        """Send a user message and append Claude's reply to the context."""

        user_message = UserMessage(content=message)
        self._context.append(user_message)

        print(f"Sending {len(self._context)} messages to Claude")

        response = self.messages.create(
            model=self._config.model,
            max_tokens=self._config.max_tokens,
            temperature=self._config.temperature,
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
            prefix = f"{index}. {message.role.title()}: "
            print(textwrap.fill(
                message.content,
                width=self._config.word_wrap,
                initial_indent=prefix,
                subsequent_indent=" " * len(prefix),
            ))

        print("-" * 50)
