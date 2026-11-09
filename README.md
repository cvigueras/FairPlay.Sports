# FairPlay Sports

[![CI](https://github.com/cvigueras/FairPlay.Sports/actions/workflows/ci.yml/badge.svg)](https://github.com/cvigueras/FairPlay.Sports/actions/workflows/ci.yml)

Sports league management platform built as a showcase of a **hexagonal architecture** combined
with **vertical slicing** in .NET 10: a REST API backed by PostgreSQL (EF Core) and a Vue 3
frontend (also packaged for iOS/Android with Capacitor).

Features: user registration and JWT authentication, teams (crest upload, members with a role per
team), challenges between teams (create, accept, reject, activate) and league standings.

> 🤖 **Built with Claude AI.** This project was generated entirely with Claude AI, applied
> throughout the whole **SDLC** (*Software Development Life Cycle*): architecture design,
> repository structure definition, backend and frontend implementation, test authoring, Visual
> Studio solution configuration, and version control with Git/GitHub.

## Architecture

- **Hexagonal (ports & adapters):** `Domain` has no dependencies; `Application` defines the ports
  (`IUserRepository`) and the use cases; `Infrastructure` implements the ports with an EF Core /
  PostgreSQL adapter; `Api` is the driving adapter (REST controllers) and acts as the composition
  root.
- **Vertical slicing:** the code is organised by feature (**Users**, **Auth**, **Teams**,
  **Challenges**, **Standings**) across every layer. Inside `Application`, each use case
  (`Register`, `GetPage`, `Accept`...) is an independent folder with its own Command/Query,
  Handler and Validator. **Users** is the reference slice to mirror when adding a new one.
- **CQRS with MediatR:** commands and queries are `IRequest<T>` messages dispatched through
  `ISender`; each slice provides an `IRequestHandler<,>`. Handlers return a `Result`/`Result<T>`
  (never throw for expected outcomes), and the `Api` layer maps that to the HTTP status code.
  FluentValidation runs as a single `ValidationBehavior<,>` MediatR pipeline step, so validation
  is a cross-cutting concern instead of being repeated in every handler.

## Repository structure

```
FairPlay.Sports.slnx               Full solution (backend + web project)
FairPlay.Sports.Backend.slnf       Backend-only filter used by CI
Dockerfile                         API image (includes an EF migrations bundle)
.github/workflows/                 CI: backend tests, frontend build, Playwright e2e, iOS simulator
src/
  FairPlay.Sports.Domain/          Domain entities and business rules
  FairPlay.Sports.Application/     Use cases (vertical slices) + ports
  FairPlay.Sports.Infrastructure/  EF Core / PostgreSQL adapter, JWT and password hashing
  FairPlay.Sports.Api/             REST API (ASP.NET Core)
tests/
  FairPlay.Sports.Domain.Tests/
  FairPlay.Sports.Application.Tests/
  FairPlay.Sports.Infrastructure.Tests/  Repository tests against real PostgreSQL (Testcontainers)
  FairPlay.Sports.Api.Tests/       Controller unit tests + integration tests (WebApplicationFactory)
  FairPlay.Sports.TestSupport/     Shared Object Mothers and helpers
web/                               Vue 3 + TypeScript + Vite + Vuetify + Pinia + vue-i18n (+ Capacitor)
  e2e/                             Playwright tests
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) `^22.18.0` or `>=24.12.0` (includes npm)
- A PostgreSQL server (and Docker, which the infrastructure tests use through Testcontainers)
- Visual Studio 2026 (optional, to open `FairPlay.Sports.slnx` with the full IDE)

## Backend (.NET 10 API)

A PostgreSQL server must be reachable at the `FairPlaySports` connection string. For local
development, a container works:

```powershell
docker run -d --name fairplay-pg -p 5432:5432 -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=FairPlaySports postgres:17-alpine
```

From the repository root:

```powershell
# Restore and build (the same filter CI uses)
dotnet build FairPlay.Sports.Backend.slnf

# Run all tests (NUnit; needs Docker)
dotnet test FairPlay.Sports.Backend.slnf

# Run the API
dotnet run --project src/FairPlay.Sports.Api
```

The API will be available at `https://localhost:7090` and `http://localhost:5228`.

In `Development` the API applies the latest EF Core migrations on startup and takes
`Jwt:SigningKey` from `appsettings.Development.json`. Outside `Development` the signing key must
be provided (environment variable or user-secrets), otherwise startup fails.

Endpoints (all under `api/`): `auth` (login, refresh, logout), `users`, `teams`, `challenges` and
`standings`. Authentication uses a short-lived JWT access token plus a rotating refresh token in
an `HttpOnly` cookie. In `Development`, interactive documentation is available via **Swagger UI**
at `/swagger` (OpenAPI document at `/openapi/v1.json`). You can also use
`src/FairPlay.Sports.Api/FairPlay.Sports.Api.http` (Visual Studio / VS Code HTTP client).

## Frontend (Vue 3 + TypeScript + Vite)

From the repository root:

```powershell
cd web
npm install
npm run dev
```

It is served at `http://localhost:5173` (pinned, because the API's CORS allow-list expects it)
and talks to the API at `VITE_API_BASE_URL` (default `https://localhost:7090`; override it in a
git-ignored `web/.env.local`, see `web/.env.example`). Start the API first.

Routes: `/login`, `/register`, `/privacy`, `/profile`, `/my-teams`, `/teams`, `/teams/:id`,
`/standings` and `/challenges` (`/` is the home page; `/dashboard` redirects to `/profile`). The
UI is available in English and Spanish.

```powershell
npm run build       # type-check (vue-tsc) + production bundle
npm run test:e2e    # Playwright tests (first time: npx playwright install chromium)
```

The e2e tests need neither the API nor a database: they mock every `/api` call. See
`web/README.md` for the Capacitor (iOS/Android) scripts.

## Opening in Visual Studio 2026

Open `FairPlay.Sports.slnx` directly. The solution includes both the .NET projects (`src/`,
`tests/`) and the frontend's JS/TS project (`web/fairplay-sports-web.esproj`),
with native Visual Studio support for running npm scripts (`dev`, `build`) from the IDE.

## Notes

- Everything persists to PostgreSQL via EF Core (Npgsql); set `ConnectionStrings:FairPlaySports`
  to point at your database.
- CORS is enabled for `http://localhost:5173`.
