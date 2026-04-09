---
name: git-commit
description: Analyze pending changes, suggest logical commit splits, and execute commits with conventional messages
allowed-tools: Bash(git status:*), Bash(git diff:*), Bash(git add:*), Bash(git commit:*), Bash(git log:*), Bash(git push:*)
---

## Context

- Current git status: !`git status`
- Staged and unstaged changes: !`git diff HEAD`
- Recent commits (for style reference): !`git log --oneline -10`

## Your task

1. Analyze the pending changes above and group them into logical commits by:
   - File type (docs, config, source code)
   - Change nature (new feature, bug fix, refactor, documentation)
   - Logical separation (e.g. .NET vs Python, code vs config)

2. Present the suggested commit plan to the user and wait for approval.

3. For each approved commit:
   - Stage only the relevant files with `git add <files>`
   - Create the commit with a conventional message in this format:
     ```
     type(scope): brief description

     - Key change 1
     - Key change 2

     Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
     ```
   - Use types: `feat`, `fix`, `docs`, `config`, `refactor`, `style`, `test`, `chore`

4. After all commits are done, ask the user whether to push.

## Safety rules

- Never commit files that may contain secrets (`.env`, credentials)
- Never use `--no-verify` or force flags unless explicitly asked
- Always show what will be staged before committing
- Never amend already-pushed commits
