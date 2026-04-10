---
name: run
description: Run the Blazor WASM app, .NET CLI console app, or Python Jupyter notebook
allowed-tools: Bash(dotnet run:*), Bash(jupyter:*), Bash(python:*)
---

## Argument

`$ARGUMENTS` — one of: `blazor`, `cli`, `python`

## Commands

| Argument | Command | Type |
|----------|---------|------|
| `blazor` | `dotnet run --project dotnet/BlazorChat` | dev server — run in background |
| `cli`    | `dotnet run --project dotnet/AnthropicApiClient` | interactive console — run in background |
| `python` | `python/\.venv/Scripts/jupyter notebook python/001-requests.ipynb` | notebook server — run in background |

All commands are executed from the repository root:
`D:\OneDrive - Personal\OneDrive\Source\Trainings\AI\Claude with Anthropic API`

## Your task

1. Parse `$ARGUMENTS` (trim whitespace, lowercase).
2. If the value is not one of `blazor`, `cli`, `python`:
   - Tell the user the valid options and stop. Do not run anything.
3. Run the matching command using the Bash tool with `run_in_background: true`.
4. Report back:
   - **blazor** — the dev server URL (typically `https://localhost:5001`)
   - **cli** — note that the app is interactive; the user should watch the terminal output for the prompt
   - **python** — Jupyter opens in the browser automatically; the notebook URL will appear in the background output
