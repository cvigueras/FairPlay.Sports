**Template name:** `Bug`
**Story title (placeholder):** `What is broken + where (e.g. Team list returns 500 when filtering by name)`
**Type:** Bug
**Suggested label:** `bug`

**Description:**

```markdown
## Summary
<What is broken, in one sentence>

Example: The team list returns a 500 error when filtering by a name containing an apostrophe.

## Steps to reproduce
1. <...>
2. <...>

## Current behaviour
<What happens now, with the exact error or status code>

## Expected behaviour
<What should happen>

## Where
- Layer: <backend | frontend | both>
- Endpoint / screen: <...>
- Environment: <local | production>

## Evidence
<Logs, stack trace, screenshot>

## Acceptance criteria
- [ ] The steps above no longer reproduce the bug
- [ ] A regression test covers it (when applicable)
- [ ] `dotnet build` / `dotnet test` / `npm run build` pass
```

**Usage notes:**
- Anything in `<...>` is what you replace when creating each story.
- Steps to reproduce and the exact error are the most valuable parts. Without them the bug has to be guessed.
- If the fix needs a new feature or a design decision, split it into a separate feature story.
