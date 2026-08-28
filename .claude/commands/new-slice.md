Anda un slice vertical nuevo espejando el de **Users**. Sigue `CLAUDE.md`.

## Contexto (se rellena solo al invocar el comando)

- Argumento (nombre del agregado, singular PascalCase): $ARGUMENTS
- Rama actual: !`git rev-parse --abbrev-ref HEAD`
- Estado: !`git status --short`

## Tarea

Si no se ha pasado nombre de agregado en el argumento, pregúntamelo antes de
empezar (ej. `Match`, `Team`, `Booking`). Llamémoslo `<X>` (singular) y `<Xs>`
(plural) a partir de aquí.

Antes de tocar nada, **relee el slice de Users** para copiar el estilo exacto:
`src/**/Users/**` y `tests/**/Users/**`, más `src/FairPlay.Sports.Application/Common/**`.

Pregúntame qué casos de uso necesita el slice (por defecto: `GetAll`, `GetById`,
y un `Register`/`Create`). Luego crea, en este orden:

### 1. Domain — `src/FairPlay.Sports.Domain/<Xs>/<X>.cs`
`sealed`, ctor privado, factoría `Create(...)` que valida todas las invariantes
(lanza `ArgumentException`), props con `private set`, mutadores con nombre de
intención. Sin dependencias externas.

### 2. Application — `src/FairPlay.Sports.Application/<Xs>/`
- `I<X>Repository.cs` (puerto) en la carpeta del slice.
- `<X>Dto.cs` — `record` con `FromDomain(...)`, sin datos sensibles.
- Una carpeta por caso de uso:
  - `<UseCase>/<UseCase><X>Query.cs` o `...Command.cs` — `record : IRequest<Result<...>>`.
    Escrituras terminan en `Command`, lecturas en `Query`.
  - `<UseCase>/<UseCase><X>Handler.cs` — `IRequestHandler<,>`, ctor primario,
    solo habla con puertos, devuelve `Result` (nunca lanza para flujo de negocio).
  - `<UseCase>/<UseCase><X>Validator.cs` — `AbstractValidator<TCommand>` para los Command.
- Si el handler nuevo NO está en el mismo assembly ya escaneado, no hace falta
  tocar `AddApplication` (MediatR y FluentValidation escanean por assembly).

### 3. Infrastructure — `src/FairPlay.Sports.Infrastructure/`
- `<Xs>/Ef<X>Repository.cs` — `internal sealed`, implementa `I<X>Repository`,
  lecturas con `AsNoTracking()`, sin `SaveChanges`.
- `Persistence/Configurations/<X>Configuration.cs` — `IEntityTypeConfiguration<<X>>`.
- `DbSet<<X>>` en `FairPlaySportsDbContext`.
- Registro en `AddInfrastructure`: `services.AddScoped<I<X>Repository, Ef<X>Repository>();`.
- Migración:
  `dotnet ef migrations add Create<Xs>Table -p src/FairPlay.Sports.Infrastructure -s src/FairPlay.Sports.Api -o Persistence/Migrations`.

### 4. Api — `src/FairPlay.Sports.Api/<Xs>/`
- `<X>sController.cs` — `sealed`, `[ApiController]`, `[Route("api/[controller]")]`,
  ctor primario `(ISender sender)`. Cada acción arma la query/command, `await
  _sender.Send(...)`, y `result.ToActionResult(this)` (o `CreatedAtAction` en create).
- Request DTOs de entrada como `record` aparte (`Register<X>Request`, etc.).

### 5. Tests (NUnit 4 + NSubstitute + Object Mother)
- `tests/FairPlay.Sports.TestSupport/<Xs>/<X>Mother.cs` — consts + factorías
  (`Domain<X>(...)`, `Command()`, `Dto(...)`), como `UserMother`.
- `tests/FairPlay.Sports.Application.Tests/<Xs>/<UseCase>/<Name>HandlerTests.cs` —
  unit, puertos con `Substitute.For<...>`.
- `tests/FairPlay.Sports.Api.Tests/<Xs>/<X>sControllerTests.cs` — `ISender` mockeado.
  Factoría de request como `<X>RequestMother` en Api.Tests.
- `tests/FairPlay.Sports.Infrastructure.Tests/<Xs>/Ef<X>RepositoryTests.cs` —
  integración, hereda de `RepositoryTestBase`, datos vía `<X>Mother`. Añade el
  `DELETE` de la tabla nueva al `[TearDown]` de `RepositoryTestBase` (o migra a
  Respawn si ya hay varias tablas relacionadas).

### 6. Cierre
- Si has creado proyectos nuevos, regístralos en `FairPlay.Sports.slnx` **y**
  `FairPlay.Sports.Backend.slnf`.
- `dotnet build FairPlay.Sports.Backend.slnf` y `dotnet test FairPlay.Sports.Backend.slnf`.
- Muéstrame un resumen de los ficheros creados y el resultado de los tests.
- No hagas commit ni push salvo que te lo pida.
