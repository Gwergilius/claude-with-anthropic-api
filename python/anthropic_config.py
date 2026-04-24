"""Anthropic API configuration."""

from __future__ import annotations

import os
from pathlib import Path

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
