---
description: Añade paginación + filtrado + orden a una entidad existente, espejando el slice Teams/GetPage
argument-hint: [nombre del agregado, singular PascalCase, p. ej. Match]
---

Cablea una entidad **ya existente** al kit genérico de querying, copiando el
slice de referencia **Teams/GetPage**. No inventes patrones nuevos: replica los
ficheros de Teams cambiando el nombre.

## Contexto (se rellena solo al invocar el comando)

- Argumento (agregado, singular PascalCase): $ARGUMENTS
- Rama actual: !`git rev-parse --abbrev-ref HEAD`
- Estado: !`git status --short`
- Kit genérico presente: !`ls src/FairPlay.Sports.Application/Common/Querying 2>/dev/null || echo "NO EXISTE"`

## Antes de empezar

1. Si no hay argumento, pregúntame el nombre del agregado. Llámalo `<X>` (singular)
   y `<Xs>` (plural) a partir de aquí.
2. Si **no existe** `src/FairPlay.Sports.Application/Common/Querying/`, páralo y
   dímelo: hay que extraer primero el kit del slice de Teams (o pídeme que lo haga).
3. Comprueba que el agregado ya tiene slice: `src/**/<Xs>/`, con
   `I<X>Repository`, `<X>Dto` y `<Xs>Controller`. Si no existe, dime que ejecute
   antes `/new-slice <X>`.
4. **Relee la referencia** y cópiale el estilo exacto:
   - `src/FairPlay.Sports.Application/Teams/GetPage/**`
   - `src/FairPlay.Sports.Application/Common/Querying/**`
   - `EfTeamRepository.GetPageAsync` y `ITeamRepository`
   - `src/FairPlay.Sports.Api/Teams/GetTeamsPageRequest.cs` y la acción `GetPage`
     de `TeamsController`
   - `tests/**/Teams/GetPage/**`, los tests de `GetPageAsync` en
     `EfTeamRepositoryTests`, y el test `GetPage` de `TeamsControllerTests`
5. Pregúntame por qué **campos filtrar** y por qué **campos ordenar** (por defecto:
   las propiedades escalares de `<X>Dto`). Pregúntame también si `GET /api/<Xs>`
   ya devuelve una lista completa que haya que **sustituir** por la paginada
   (como se hizo en Teams) o si hay que **añadir** un endpoint aparte.

## Qué crear (en este orden)

### 1. Application — `src/FairPlay.Sports.Application/<Xs>/GetPage/`

- **`<X>Filter.cs`** — `public sealed record <X>Filter(...campos opcionales, todos con default null...) : IQueryFilter<<X>>`.
  En `Apply(IQueryable<<X>> source)`: un `.Where(...)` por campo no nulo,
  combinados con AND. Strings → *contains* case-insensitive con
  `x.Prop.ToLower().Contains(value.Trim().ToLower())` (NO uses `EF.Functions`:
  Application no referencia EF Core). Enums / `bool?` / ids → igualdad exacta.
- **`<X>Sort.cs`** — `public sealed class <X>Sort(string? sort) : IQuerySort<<X>>`.
  `public static readonly IReadOnlySet<string> AllowedFields` (whitelist, minúsculas
  vía `StringComparer.OrdinalIgnoreCase`); `SortSpec.Parse(sort)` en el ctor;
  en `Apply` un `switch` campo→`OrderBy/OrderByDescending`, encadenando `ThenBy`
  (helper `Chain(...)` como en `TeamSort`); campo desconocido se ignora; **siempre
  termina en una clave estable** (`ThenBy(x => x.Id)` o el `Name`) para que la
  paginación sea determinista. Sin sort → `OrderBy(clave estable)`.
- **`Get<Xs>PageQuery.cs`** — `public sealed record Get<Xs>PageQuery(int Page, int PageSize, string? Sort, <X>Filter Filter) : IRequest<Result<PagedResult<<X>Dto>>>, IPagedQuery;`
- **`Get<Xs>PageHandler.cs`** — `IRequestHandler<,>`, ctor primario `(I<X>Repository repository)`.
  `var page = await _repository.GetPageAsync(request.Filter, new <X>Sort(request.Sort), request.Page, request.PageSize, ct);`
  `return Result<PagedResult<<X>Dto>>.Success(page.Map(<X>Dto.FromDomain));`
- **`Get<Xs>PageValidator.cs`** — `: PagedQueryValidator<Get<Xs>PageQuery>`.
  Regla extra: cada campo de `SortSpec.Parse(query.Sort)` debe estar en
  `<X>Sort.AllowedFields`, si no `WithMessage(...)`.

### 2. Repositorio

- **`I<X>Repository`** — añade:
  ```csharp
  Task<PagedResult<<X>>> GetPageAsync(
      IQueryFilter<<X>> filter,
      IQuerySort<<X>> sort,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default);
  ```
  Si sustituimos el listado completo, elimina `GetAllAsync` del puerto, su impl y
  sus tests, y el `GetAll` de Application/Api (como en Teams).
- **`Ef<X>Repository`** — implementa:
  ```csharp
  public Task<PagedResult<<X>>> GetPageAsync(IQueryFilter<<X>> filter, IQuerySort<<X>> sort,
      int page, int pageSize, CancellationToken cancellationToken = default)
  {
      var query = sort.Apply(filter.Apply(_context.<Xs>.AsNoTracking()));
      return query.ToPagedResultAsync(page, pageSize, cancellationToken);
  }
  ```
  `using FairPlay.Sports.Application.Common.Querying;`. `ToPagedResultAsync` ya
  existe en `Infrastructure/Persistence/QueryableExtensions.cs`.

### 3. Api — `src/FairPlay.Sports.Api/<Xs>/`

- **`Get<Xs>PageRequest.cs`** — `record` bindable desde query string:
  `Page` (=1), `PageSize` (=`PaginationDefaults.DefaultPageSize`), `Sort`, y una
  propiedad por filtro (`string? Name`, `SomeEnum? Type`, `bool? Active`, ...).
- **Controlador** — acción `[HttpGet]`:
  ```csharp
  [HttpGet]
  [ProducesResponseType(typeof(PagedResult<<X>Dto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<PagedResult<<X>Dto>>> GetPage(
      [FromQuery] Get<Xs>PageRequest request, CancellationToken cancellationToken)
  {
      var query = new Get<Xs>PageQuery(
          request.Page, request.PageSize, request.Sort,
          new <X>Filter(/* mapear campos del request */));
      var result = await _sender.Send(query, cancellationToken);
      return result.ToActionResult(this);
  }
  ```
  Si sustituye a un `GetAll`, borra la acción vieja y su `using`.

### 4. Tests (NUnit 4 + NSubstitute + Object Mother)

- `Application.Tests/<Xs>/GetPage/Get<Xs>PageHandlerTests.cs` — reenvía
  page/pageSize/filter/sort al repo (`Arg.Is<IQuerySort<<X>>>(s => s is <X>Sort)`),
  mapea la página a DTO conservando `Page/PageSize/TotalCount/TotalPages`, y
  propaga el `CancellationToken`.
- `Application.Tests/<Xs>/GetPage/Get<Xs>PageValidatorTests.cs` — `Page` < 1
  inválido; `PageSize` fuera de `[1, MaxPageSize]` inválido; campo de sort
  fuera de la whitelist inválido; defaults válidos.
- `Application.Tests/<Xs>/GetPage/<X>FilterTests.cs` y `<X>SortTests.cs` — sobre
  `new[]{ ... }.AsQueryable()` con `<X>Mother`: filtro vacío devuelve todo,
  *contains* case-insensitive, igualdad exacta, AND; orden asc/desc, `ThenBy`,
  clave estable de desempate, campo desconocido ignorado.
- `Api.Tests/<Xs>/<X>sControllerTests.cs` — nuevo test: la acción despacha un
  `Get<Xs>PageQuery` con el filtro mapeado y devuelve `Ok` con el `PagedResult`
  del handler.
- `Infrastructure.Tests/<Xs>/Ef<X>RepositoryTests.cs` — reemplaza los tests de
  `GetAllAsync` (si los había) por `GetPageAsync`: skip/take + `TotalCount`
  completo, `HasNext/HasPrevious`, filtro por 1–2 campos, orden asc y `-campo`
  desc, página vacía.
- `<X>Mother` — añade los helpers que necesiten los tests (p. ej. un parámetro
  `createdAtUtc` para poder ordenar por fecha, como se hizo en `TeamMother`).

### 5. Migración

Normalmente **ninguna** (no cambia el modelo). Solo si añades índices para
filtrar/ordenar:
`dotnet ef migrations add <Xs>QueryIndexes -p src/FairPlay.Sports.Infrastructure -s src/FairPlay.Sports.Api -o Persistence/Migrations`.

### 6. Cierre

- `dotnet build FairPlay.Sports.Backend.slnf` y `dotnet test FairPlay.Sports.Backend.slnf`
  (los tests de Infrastructure necesitan Docker levantado).
- Muéstrame el resumen de ficheros creados/modificados y el resultado de los tests.
- **No hagas commit ni push** salvo que te lo pida.
- El frontend queda fuera salvo que te lo pida explícitamente.
