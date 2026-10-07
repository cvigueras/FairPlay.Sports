# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# FairPlay.Sports

Backend .NET 10, hexagonal architecture + DDD organised as **vertical slices**.
The **Users** slice is the reference implementation: when adding a feature,
mirror its files across the four layers. **Teams** and **Auth** are the other
two slices; Teams follows the same shape (plus binary crest upload/download),
Auth is the one deliberate outlier (see the Api row below).

## Rules by topic

The detailed rules live in `.claude/rules/` and are loaded automatically. Files with
`paths` load only when Claude works on matching files; the others load every session.

| File | Covers | Loads when working on |
|---|---|---|
| `domain-application.md` | Domain and Application conventions, `Result`, MediatR pipeline | `src/…Domain/**`, `src/…Application/**` |
| `infrastructure.md` | EF Core adapters, persistence, migrations | `src/…Infrastructure/**`, `src/…Domain/**` |
| `api.md` | Controllers, the Auth cookie exception | `src/…Api/**` |
| `tests.md` | NUnit, Object Mother, Testcontainers | `tests/**` |
| `frontend.md` | Vue 3, Vuetify, Pinia, `http.ts` | `web/**` |
| `shortcut-mcp.md` | Shortcut story workflow | always |
| `github-mcp.md` | PR review with the GitHub MCP | always |

## Layers

Dependency direction: `Api → Application → Domain`, `Infrastructure → Application`.
Api composes everything; nothing depends on Api or Infrastructure.

| Project | Responsibility | Reference files |
|---|---|---|
| `Domain` | Aggregates with invariants enforced in factories/methods. No external deps. | `Domain/Users/User.cs` |
| `Application` | CQRS use cases via MediatR. Defines the driven ports. Returns `Result`. | `Application/Users/Register/*`, `Application/Common/*` |
| `Infrastructure` | `internal sealed` driven adapters: EF Core repository, DbContext, configs, migrations. | `Infrastructure/Users/EfUserRepository.cs`, `Infrastructure/Persistence/*` |
| `Api` | Thin controllers: dispatch via `ISender`, translate `Result` to `IActionResult`. | `Api/Users/UsersController.cs`, `Api/Common/ResultExtensions.cs` |

## Build / run

- Solution: `FairPlay.Sports.slnx`; CI uses the filter `FairPlay.Sports.Backend.slnf`.
  A new project must be added to **both**.
- `dotnet build FairPlay.Sports.Backend.slnf`
- `dotnet test FairPlay.Sports.Backend.slnf`
- One class / test:
  `dotnet test tests/<Project>/<Project>.csproj --filter "FullyQualifiedName~<ClassOrMethod>"`.
- Run the API: `dotnet run --project src/FairPlay.Sports.Api` → `https://localhost:7090`.
  In Development it auto-migrates, so a PostgreSQL server must be reachable at the
  `FairPlaySports` connection string (local dev: a `postgres:17-alpine` container on
  `5432`, `postgres`/`postgres`, db `FairPlaySports`). `Jwt:SigningKey` comes from
  `appsettings.Development.json` (dev only); running outside Development needs it set
  via env var / user-secrets or startup throws.
- Frontend: `cd web && npm run dev` (Vite, pinned to
  `http://localhost:5173` for the API's CORS allow-list); `npm run build` type-checks.

## Commits & PRs

Conventional Commits. Use the `/commit` and `/pr` commands. **Never `git commit`,
push or merge until the user explicitly asks.** Make and verify the changes
(build, tests), then stop and wait — leave everything staged/unstaged in the
working tree for the user to review first.
