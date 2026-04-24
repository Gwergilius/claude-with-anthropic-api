"""Immutable dictionary base type."""


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
