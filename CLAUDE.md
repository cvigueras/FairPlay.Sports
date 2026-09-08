# FairPlay.Sports

Backend .NET 10, hexagonal architecture + DDD organised as **vertical slices**.
The **Users** slice is the reference implementation: when adding a feature,
mirror its files across the four layers.

Frontend (`frontend/fairplay-sports-web`, Vue 3) is out of scope for these
notes unless the task explicitly targets it.

## Layers

Dependency direction: `Api → Application → Domain`, `Infrastructure → Application`.
Api composes everything; nothing depends on Api or Infrastructure.

| Project | Responsibility | Reference files |
|---|---|---|
| `Domain` | Aggregates with invariants enforced in factories/methods. No external deps. | `Domain/Users/User.cs` |
| `Application` | CQRS use cases via MediatR. Defines the driven ports. Returns `Result`. | `Application/Users/Register/*`, `Application/Common/*` |
| `Infrastructure` | `internal sealed` driven adapters: EF Core repository, DbContext, configs, migrations. | `Infrastructure/Users/EfUserRepository.cs`, `Infrastructure/Persistence/*` |
| `Api` | Thin controllers: dispatch via `ISender`, translate `Result` to `IActionResult`. | `Api/Users/UsersController.cs`, `Api/Common/ResultExtensions.cs` |

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
  `Application/Common/IUnitOfWork.cs`).

**Infrastructure** — adapters are `internal sealed`. EF mapping via
`IEntityTypeConfiguration<T>` in `Persistence/Configurations/`. Repositories
issue reads with `AsNoTracking()` and **never call `SaveChanges`**.

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

- **Users** slice: real SQL Server via EF Core. Local dev DB is SQL Express,
  connection string `FairPlaySports` in `appsettings.json`.
- **Products** slice is a seeded **in-memory** adapter on purpose (demo only) —
  do not migrate it to EF.
- Migrations:
  `dotnet ef migrations add <Name> -p src/FairPlay.Sports.Infrastructure -s src/FairPlay.Sports.Api -o Persistence/Migrations`.
  Auto-applied on startup only in Development (`app.Services.MigrateAsync()` in
  `Program.cs`); production applies them as an explicit deploy step.

## Tests (NUnit 4 + NSubstitute + Object Mother)

- Test data **always** comes from an `XMother` in `FairPlay.Sports.TestSupport`
  (`Users/UserMother.cs`: consts + `DomainUser(...)`, `Command()`, `Dto(...)`).
  Api-only request factories stay in `Api.Tests` (`UserRequestMother`) so the
  Api reference is not dragged into `Application.Tests`.
- `Application.Tests/Users/<UseCase>/<Name>HandlerTests.cs` — unit; ports mocked
  with `Substitute.For<...>`. Assert `Result` shape, error type, and port calls.
- `Api.Tests/Users/UsersControllerTests.cs` — `ISender` mocked; assert the right
  request is dispatched and the `Result` maps to the right `IActionResult`.
- `Infrastructure.Tests` — integration against real SQL Server via
  **Testcontainers.MsSql**. `SqlServerContainerFixture` (`[SetUpFixture]`) starts
  one container per assembly and runs `MigrateAsync()` once; `RepositoryTestBase`
  gives fresh `DbContext`s and empties the table between tests;
  `[assembly: NonParallelizable]`. Container reuse is on locally, off on CI (`CI`
  env var). Requires Docker Desktop in **Linux-container** mode.
- Frameworks: `[TestFixture]`, `Assert.That` / `Assert.Multiple`. Prefer the
  behaviour-named test style already in the suite.

## Build / run

- Solution: `FairPlay.Sports.slnx`; CI uses the filter `FairPlay.Sports.Backend.slnf`.
  A new project must be added to **both**.
- `dotnet build FairPlay.Sports.Backend.slnf`
- `dotnet test FairPlay.Sports.Backend.slnf`

## Commits & PRs

Conventional Commits. Use the `/commit` and `/pr` commands; do not push or merge
unless asked.
