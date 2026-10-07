---
paths:
  - "src/FairPlay.Sports.Infrastructure/**"
  - "src/FairPlay.Sports.Domain/**"
---

# Infrastructure & persistence

**Infrastructure** — adapters are `internal sealed`. EF mapping via
`IEntityTypeConfiguration<T>` in `Persistence/Configurations/`. Repositories
issue reads with `AsNoTracking()` and **never call `SaveChanges`**. Every
repository exposes both a plain `GetByIdAsync` (no-tracking, for queries) and a
`GetByIdForUpdateAsync` (tracked, for command handlers that mutate the
aggregate and rely on `UnitOfWorkBehavior` to commit) — pick the tracked one
whenever the handler calls a mutator on the aggregate.

## Persistence

- All slices (Users, Teams, Auth's `RefreshToken`) share one EF Core
  `FairPlaySportsDbContext` against PostgreSQL (Npgsql provider). Local dev DB is
  a PostgreSQL server, connection string `FairPlaySports` in `appsettings.json`.
  Keep entity configs provider-agnostic — no `HasColumnType("varbinary(max)")`
  and the like; let Npgsql map (`byte[]` → `bytea`, `DateTime` → `timestamptz`).
- Migrations:
  `dotnet ef migrations add <Name> -p src/FairPlay.Sports.Infrastructure -s src/FairPlay.Sports.Api -o Persistence/Migrations`.
  Auto-applied on startup only in Development (`app.Services.MigrateAsync()` in
  `Program.cs`); production applies them as an explicit deploy step, or opts in with
  `Database__MigrateOnStartup=true` for hosts without one (single instance only).
- **Add the migration before you run or test after a model change.** EF 10's
  `MigrateAsync()` throws `PendingModelChangesWarning` (so the
  `PostgreSqlContainerFixture` `[SetUpFixture]` fails for the whole assembly)
  when the model no longer matches the last migration's snapshot.
