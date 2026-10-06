**Template name:** `Chore`
**Story title (placeholder):** `Verb + technical outcome (e.g. Upgrade Npgsql to the latest patch version)`
**Type:** Chore
**Suggested label:** `chore`

**Description:**

```markdown
## Goal
<What technical outcome is wanted and why, in 1-3 sentences>

Example: Upgrade the EF Core and Npgsql packages to the latest patch version to pick up security fixes.

## Scope
- Layer: <backend | frontend | CI | tooling | docs>
- Areas affected: <projects, folders or files>
- Out of scope: <what NOT to touch>

## What must not change
- <Behaviour, public API or data that has to stay exactly the same>

## Approach
- <Steps or constraints, if known. Otherwise write "up to you" and a plan will be proposed first>

## Acceptance criteria
- [ ] <Concrete, verifiable outcome>
- [ ] No behaviour change (existing tests pass without modification)
- [ ] `dotnet build` and `dotnet test` pass (backend)
- [ ] `npm run build` passes (frontend)

## References
- Links: <docs, changelogs, related stories>
```

**Usage notes:**
- Anything in `<...>` is what you replace when creating each story.
- A chore changes how the code is built or organised, not what it does for users. If behaviour changes, use the feature template instead.
- "What must not change" is the key section: it tells me which behaviour the existing tests must keep protecting.
