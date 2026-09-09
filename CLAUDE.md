# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# FairPlay.Sports

Backend .NET 10, hexagonal architecture + DDD organised as **vertical slices**.
The **Users** slice is the reference implementation: when adding a feature,
mirror its files across the four layers. **Teams** and **Auth** are the other
two slices; Teams follows the same shape (plus binary crest upload/download),
Auth is the one deliberate outlier (see the Api row below).

Frontend (`frontend/`) is out of scope for these notes
unless the task explicitly targets it. When it does: Vue 3 + Vuetify 4
(mdi-svg icons — pass icon paths, no runtime font), Pinia, vue-i18n
(switch/persist the locale via `setLocale` in `src/plugins/i18n.ts`; it is
global, so a change on any screen shows everywhere). All HTTP goes through
`src/lib/http.ts` (`get/post/put/postForm`); the access token lives in
memory in the `auth` store, the refresh token in an `HttpOnly` cookie.
`npm run build` runs the type-check (`vue-tsc`) and the build; there are no
frontend tests. In `handleSubmit`-style flows keep `router.push` **outside**
the try/catch that wraps the API call, so a navigation rejection is not
surfaced as an API error.

## Layers

Dependency direction: `Api → Application → Domain`, `Infrastructure → Application`.
Api composes everything; nothing depends on Api or Infrastructure.

| Project | Responsibility | Reference files |
|---|---|---|
| `Domain` | Aggregates with invariants enforced in factories/methods. No external deps. | `Domain/Users/User.cs` |
| `Application` | CQRS use cases via MediatR. Defines the driven ports. Returns `Result`. | `Application/Users/Register/*`, `Application/Common/*` |
| `Infrastructure` | `internal sealed` driven adapters: EF Core repository, DbContext, configs, migrations. | `Infrastructure/Users/EfUserRepository.cs`, `Infrastructure/Persistence/*` |
| `Api` | Thin controllers: dispatch via `ISender`, translate `Result` to `IActionResult`. | `Api/Users/UsersController.cs`, `Api/Common/ResultExtensions.cs` |

`Api/Auth/AuthController.cs` does not use `ResultExtensions.ToActionResult` — login/refresh
also have to set/clear the `fps_refresh_token` `HttpOnly` cookie, so it maps `Result` to
`IActionResult` by hand. Its cookie is `SameSite=None` in Development only (the Vite dev
server and the API are on different origins/schemes there) and `SameSite=Strict` otherwise;
don't "fix" this into a single constant.

## Conventions (follow the Users slice)

**Domain** — `sealed` class, private ctor, static `Create(...)` factory that
validates every invariant, `private set` properties. Invariant violations throw
`ArgumentException`. **YAGNI on behaviour**: an aggregate gets a mutator *only*
when a use case you were explicitly asked to build calls it (e.g. `Activate()`
backs an activate command). Never add speculative `RenameX` / `ChangeY` /
`Deactivate` methods "just in case" — if nothing calls it, it must not exist.
Everything else an aggregate does is invariant validation, not public API.
No `<summary>` / XML doc comments on aggregates, entities or handlers — the
type, member names and factory speak for themselves.
`User.Create` lower-cases (and trims) the email; the user repository normalises
email lookups the same way, so sign-in is case-insensitive.

**Binary image on an aggregate** (Team crest, User photo — mirror one when
adding another): `byte[]? X` + `string? XContentType` + `bool HasX` +
`void SetX(byte[], string)` on the aggregate (validates non-empty, a byte cap,
an allowed-content-type list); an `<Slice>/XPayload`-style record (`TeamCrest`,
`UserPhoto`); a repo `GetXAsync` projecting straight to that record;
`POST /api/<slice>/{id}/x` (`multipart/form-data`, `IFormFile file`,
`[RequestSizeLimit]`) and an `[AllowAnonymous]` `GET .../{id}/x` returning
`File(bytes, contentType)`. The list DTO carries `HasX`, not the bytes.

**Application** — one folder per use case: `Users/<UseCase>/`.
- `<UseCase>Command` / `<UseCase>Query` — `record`, implements `IRequest<Result<T>>`.
  Writes end in `Command`, reads end in `Query` (the pipeline keys off that).
- `<UseCase>Handler` — `IRequestHandler<,>`, primary constructor, `private readonly`
  field aliases. Talks only to ports. Returns `Result<T>` — never throws for
  business flow (`Result<T>.Failure(...)`, `Result<T>.NotFound(...)`).
- `<UseCase>Validator` — `AbstractValidator<TCommand>` (FluentValidation),
  auto-discovered. Structural validation lives here, not in the handler.
- DTOs: `record` with a static `FromDomain(...)`; never expose `PasswordHash`.
- Ports live here (`Application/Users/IUserRepository.cs`,
  `Application/Common/IUnitOfWork.cs`). `Application/Common/IClock.cs` is
  injected into any handler that needs "now" (registration/activation
  timestamps, refresh-token expiry) instead of calling `DateTime.UtcNow`
  directly, so handler tests can control time.
- Cross-slice application services (not tied to one use case) live at the
  slice root, e.g. `Application/Auth/IAuthTokenIssuer.cs` — shared by the
  login and refresh handlers to mint the access/refresh token pair.

**Infrastructure** — adapters are `internal sealed`. EF mapping via
`IEntityTypeConfiguration<T>` in `Persistence/Configurations/`. Repositories
issue reads with `AsNoTracking()` and **never call `SaveChanges`**. Every
repository exposes both a plain `GetByIdAsync` (no-tracking, for queries) and a
`GetByIdForUpdateAsync` (tracked, for command handlers that mutate the
aggregate and rely on `UnitOfWorkBehavior` to commit) — pick the tracked one
whenever the handler calls a mutator on the aggregate.

**Api** — controller is `sealed`, `[ApiController]`, `[Route("api/[controller]")]`,
primary ctor `(ISender sender)`. Actions build the command/query, `await
_sender.Send(...)`, then `result.ToActionResult(this)` (or `CreatedAtAction`
on a successful create). Inbound request DTOs are separate records in
`Api/Users/` (e.g. `RegisterUserRequest`), mapped to the command in the action.

## Result & MediatR pipeline

- `Result` / `Result<T>` (`Application/Common/Result.cs`), error types
  `None | Validation | NotFound`. Both implement `IResult` so behaviors can
  inspect the outcome generically.
- Pipeline order (`Application/DependencyInjection.cs`): `ValidationBehavior`
  then `UnitOfWorkBehavior`.
- `ValidationBehavior` runs all validators and short-circuits with a failed
  `Result` via `ResultFactory` — it does not throw.
- `UnitOfWorkBehavior` calls `IUnitOfWork.SaveChangesAsync` once, only when the
  request type name ends in `Command` **and** the response is not a failed
  `IResult`. Handlers stay free of persistence calls.

## Persistence

- All slices (Users, Teams, Auth's `RefreshToken`) share one EF Core
  `FairPlaySportsDbContext` against PostgreSQL (Npgsql provider). Local dev DB is
  a PostgreSQL server, connection string `FairPlaySports` in `appsettings.json`.
  Keep entity configs provider-agnostic — no `HasColumnType("varbinary(max)")`
  and the like; let Npgsql map (`byte[]` → `bytea`, `DateTime` → `timestamptz`).
- Migrations:
  `dotnet ef migrations add <Name> -p src/FairPlay.Sports.Infrastructure -s src/FairPlay.Sports.Api -o Persistence/Migrations`.
  Auto-applied on startup only in Development (`app.Services.MigrateAsync()` in
  `Program.cs`); production applies them as an explicit deploy step.
- **Add the migration before you run or test after a model change.** EF 10's
  `MigrateAsync()` throws `PendingModelChangesWarning` (so the
  `PostgreSqlContainerFixture` `[SetUpFixture]` fails for the whole assembly)
  when the model no longer matches the last migration's snapshot.

## Tests (NUnit 4 + NSubstitute + Object Mother)

- Test data **always** comes from an `XMother` in `FairPlay.Sports.TestSupport`
  (`Users/UserMother.cs`: consts + `DomainUser(...)`, `Command()`, `Dto(...)`).
  Api-only request factories stay in `Api.Tests` (`UserRequestMother`) so the
  Api reference is not dragged into `Application.Tests`.
- `Application.Tests/Users/<UseCase>/<Name>HandlerTests.cs` — unit; ports mocked
  with `Substitute.For<...>`. Assert `Result` shape, error type, and port calls.
- `Api.Tests/Users/UsersControllerTests.cs` — `ISender` mocked; assert the right
  request is dispatched and the `Result` maps to the right `IActionResult`.
- `Infrastructure.Tests` — integration against real PostgreSQL via
  **Testcontainers.PostgreSql**. `PostgreSqlContainerFixture` (`[SetUpFixture]`)
  starts one container per assembly and runs `MigrateAsync()` once;
  `RepositoryTestBase` gives fresh `DbContext`s and empties the table between
  tests; `[assembly: NonParallelizable]`. Container reuse is on locally, off on
  CI (`CI` env var). Requires a running Docker daemon.
- Frameworks: `[TestFixture]`, `Assert.That` / `Assert.Multiple`. Prefer the
  behaviour-named test style already in the suite.

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
- Frontend: `cd frontend && npm run dev` (Vite, pinned to
  `http://localhost:5173` for the API's CORS allow-list); `npm run build` type-checks.

## Commits & PRs

Conventional Commits. Use the `/commit` and `/pr` commands. **Never `git commit`,
push or merge until the user explicitly asks.** Make and verify the changes
(build, tests), then stop and wait — leave everything staged/unstaged in the
working tree for the user to review first.
