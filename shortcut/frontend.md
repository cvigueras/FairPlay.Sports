**Template name:** `Frontend story`
**Story title (placeholder):** `Verb + outcome (e.g. Show the team crest in the detail hero)`
**Type:** Feature
**Suggested label:** `frontend`

**Description:**

```markdown
## Context
<Why this exists, in 1-3 sentences: the problem or business need>

Example: Delegates need to retire teams that no longer compete without deleting them, so their history is kept.

## What to do
<Expected behaviour, in user language>

## Scope
- Layer: frontend (`web/`)
- Screen / route: <e.g. /teams/:id>
- Depends on backend story: <link, or "endpoint already exists">
- Out of scope: <what NOT to touch>

## UI
- Components: <Vuetify components or existing ones to reuse>
- States: <loading | empty | error | success>
- Errors: shown as a red `v-alert` (`type="error"`)
- Responsive: <mobile behaviour, if relevant>
- Design / mockup: <link or description>

## Data
- Calls via `src/lib/http.ts`: <METHOD /api/...>
- Store: <Pinia store affected, if any>
- Auth: <requires login | public>

## i18n
- [ ] All user-facing texts go through vue-i18n
- Locales: <e.g. es, en>
- New keys: <...>

## Acceptance criteria
- [ ] <Concrete, verifiable criterion>
- [ ] Loading, empty and error states handled
- [ ] No hardcoded texts
- [ ] `npm run build` passes (type-check)

## References
- Similar screen/component to mirror: <e.g. Teams list, team wizard>
- Links: <design, related stories>
```

**Usage notes:**
- Anything in `<...>` is what you replace when creating each story.
- If a story needs a new endpoint, create a separate backend story and link it in "Depends on backend story".
- There are no frontend tests, so the acceptance criteria should be checkable by looking at the screen.
- The template follows the `CLAUDE.md` frontend notes: Vue 3 + Vuetify 4, Pinia, vue-i18n, all HTTP through `src/lib/http.ts`.
