---
paths:
  - "src/FairPlay.Sports.Domain/**"
  - "src/FairPlay.Sports.Application/**"
---

# Domain & Application conventions (follow the Users slice)

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
