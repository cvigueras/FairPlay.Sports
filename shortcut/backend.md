**Template name:** `Backend story`
**Story title (placeholder):** `Verb + outcome (e.g. Allow deactivating a team)`
**Type:** Feature
**Suggested label:** `backend`

**Description:**

```markdown
## Context
<Why this exists, in 1-3 sentences: the problem or business need>

Example: Delegates need to retire teams that no longer compete without deleting them, so their history is kept.

## What to do
<Expected behaviour, in business language>

## Scope
- Layer: backend
- Slice: <Users | Teams | Auth | new>
- Use case type: <Command (writes) | Query (reads)>
- Out of scope: <what NOT to touch>

## Domain rules
- <Invariants and constraints, e.g. a team can only have one Delegate>

## API
- Endpoint: `<METHOD> /api/<slice>/...`
- Request: <fields and validations>
- Responses: <200/201/204 on success, 400 validation, 404 not found>
- Authorization: <authenticated | AllowAnonymous>

## Persistence
- [ ] Does the model change? If so, include a migration
- Affected fields/tables: <...>

## Acceptance criteria
- [ ] <Concrete, verifiable criterion>
- [ ] Error case: <what is returned if it doesn't exist / invalid data>
- [ ] Handler tests (unit, ports mocked)
- [ ] Controller tests
- [ ] Repository tests, only if there is a new query or mapping
- [ ] `dotnet build` and `dotnet test` pass

## References
- Similar code to mirror: <e.g. Teams/GetPage, Users/Register>
- Links: <design, related stories>
```

**Usage notes:**
- The "Domain rules", "API" and "Persistence" sections are the ones that save the most guesswork. If a story doesn't need one, delete it rather than leaving it empty.
- Anything in `<...>` is what you replace when creating each story.
- The template follows the `CLAUDE.md` conventions: Command/Query, `Result`, migration before testing, and tests with Mothers.
