# Copilot Instructions

## Commit messages

When generating a commit message, follow these rules.

### Format

```
<emoji> <type>(<scope>): <description>

<optional body>

<optional footer>
```

Example: `✨ feat(missions): add new reconnaissance mission`

### Types and emojis

| Emoji | Type       | Use for                          |
|-------|------------|----------------------------------|
| ✨    | feat       | New feature                      |
| 🐛    | fix        | Bug fix                          |
| 📚    | docs       | Documentation changes            |
| 💎    | style      | Formatting, whitespace, etc.     |
| ♻️    | refactor   | Code refactoring                 |
| ⚡    | perf       | Performance improvements         |
| ✅    | test       | Add or fix tests                 |
| 🔧    | chore      | Maintenance tasks                |
| 🚀    | ci         | CI/CD changes                    |
| 💥    | BREAKING   | Breaking changes                 |

### Rules

- Keep the first line at 50 characters or fewer.
- Use the imperative mood: "add", not "added".
- Do not end the description with a period.
- Leave a blank line between the title and the body.

### Body (optional)

- Explain WHAT changed and WHY, not HOW.
- Wrap lines at 72 characters.

### Footer (optional)

- Reference issues with `Fixes #123` or `Closes #456`.
- For breaking changes, add `BREAKING CHANGE: description of the change`.
