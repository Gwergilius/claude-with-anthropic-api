---
name: git-commit
description: Automated git commit workflow with intelligent commit splitting
version: 1.0
author: Claude API Integration Project
tags: [git, commit, workflow, automation]
applyTo:
  - "**/*"
tools:
  - run_in_terminal
  - get_changed_files
  - vscode_askQuestions
  - read_file
  - grep_search
---

# Git Commit Workflow SKILL

This skill automates the git commit process by analyzing changes, suggesting logical commit splits, and executing commits with proper messages.

## 🎯 Purpose

Streamline the commit process by:
- Analyzing all pending changes in the repository
- Suggesting logical commit groupings
- Creating concise, descriptive commit messages
- Getting user approval before committing
- Executing commits and pushing changes

## 🔧 Workflow Steps

### 1. **Change Analysis**
- Check git status for modified, added, and deleted files
- Analyze file types and modification nature
- Identify logical groupings (features, fixes, docs, config)

### 2. **Commit Strategy Planning**
- Suggest independent commits based on:
  - File types (.md, .cs, .json, etc.)
  - Change nature (new features, bug fixes, documentation)
  - Logical separation (frontend/backend, config/code)
- Present options to user for approval

### 3. **Commit Message Generation**
- Generate 4-5 line commit messages following conventional format:
  ```
  type(scope): brief description
  
  - Key change 1
  - Key change 2
  - Impact or reason for change
  ```

### 4. **User Interaction**
- Present suggested commit plan
- Allow user to modify or approve commits
- Confirm before executing each commit

### 5. **Execution**
- Stage files for each commit
- Execute commits with generated messages
- Push all commits to remote repository

## 📋 Usage Examples

### Full Repository Commit
```
User: "Commit all changes"
→ Analyzes entire repo
→ Suggests: docs, config, and code commits
→ Executes approved plan
```

### Partial Directory Commit
```
User: "Commit only dotnet folder changes"
→ Analyzes dotnet/ directory
→ Suggests: config and code separation
→ Commits filtered changes
```

### Single Logical Change
```
User: "Commit documentation updates"
→ Identifies *.md changes
→ Creates single doc commit
→ Pushes changes
```

## 🛠 Implementation

### Step 1: Discover Changes
```bash
git status --porcelain
git diff --name-only
git diff --cached --name-only
```

### Step 2: Analyze File Patterns
Group files by:
- **Documentation**: `*.md`, `*.txt`
- **Configuration**: `*.json`, `*.xml`, `*.config`
- **Source Code**: `*.cs`, `*.py`, `*.js`
- **Project Files**: `*.csproj`, `*.sln`, `*.slnx`
- **Build/Deploy**: `Dockerfile`, `*.yml`, `*.yaml`

### Step 3: Suggest Commit Groups
Based on analysis, suggest commits like:
- `docs: Update README and documentation`
- `config: Add environment-based configuration`
- `feat: Implement new AI client with DI`
- `refactor: Modernize to .NET 10 and C# 13`

### Step 4: Execute Workflow
For each approved commit:
1. `git add <files>`
2. `git commit -m "<message>"`
3. Verify with user before push
4. `git push origin main`

## 🎯 Commit Message Format

Follow conventional commit format:

```
type(scope): description

- Specific change or addition
- Key modification or improvement  
- Reason or impact of change
- Related component affected
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `config`: Configuration changes
- `refactor`: Code refactoring
- `style`: Code style changes
- `test`: Tests
- `chore`: Maintenance

## 🔍 Intelligence Features

- **Smart Grouping**: Automatically clusters related changes
- **Context Awareness**: Understands project structure
- **User Confirmation**: Always asks before committing
- **Rollback Safety**: Provides clear commit separation
- **Push Strategy**: Batches commits for efficient pushing

## 🚨 Safety Measures

- Always show diff before committing
- Require user confirmation for each commit
- Verify git repository state
- Check for merge conflicts
- Validate commit message format

## 📝 Example Interaction

```
🔍 Analyzing repository changes...

Found changes in:
├── README.md, Claude.md (docs)
├── appsettings*.json (config)  
└── *.cs files (source code)

💡 Suggested commit strategy:

1. docs: Update project documentation and guidelines
   - README.md: Add reference-style links
   - Claude.md: Add project standards
   
2. config: Add environment-based configuration
   - appsettings.Development.json: Add Haiku model
   - Program.cs: Environment detection logic
   
3. feat: Implement modern .NET patterns
   - Primary constructors in AnthropicClient
   - IOptions configuration pattern

📋 Proceed with this commit plan? [Y/n]
```

## 🔧 Error Handling

- **No Changes**: Inform user, exit gracefully
- **Merge Conflicts**: Pause workflow, show resolution steps
- **Network Issues**: Retry push, show status
- **Invalid Messages**: Re-generate with user input

---

This skill provides intelligent, automated git workflow management while maintaining user control and safety.