"""Anthropic API message types."""

from __future__ import annotations

from immutable_dict import ImmutableDict


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
