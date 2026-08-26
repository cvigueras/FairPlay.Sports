# FairPlay Sports

[![CI](https://github.com/cvigueras/FairPlay.Sports/actions/workflows/ci.yml/badge.svg)](https://github.com/cvigueras/FairPlay.Sports/actions/workflows/ci.yml)

Sample project demonstrating a **hexagonal architecture** combined with **vertical slicing** in
.NET 10, with a dummy RESTful CRUD (in-memory data) and a Vue 3 frontend with dummy login and
register screens.

> 🤖 **Built with Claude AI.** This project was generated entirely with Claude AI, applied
> throughout the whole **SDLC** (*Software Development Life Cycle*): architecture design,
> repository structure definition, backend and frontend implementation, test authoring, Visual
> Studio solution configuration, and version control with Git/GitHub.

## Architecture

- **Hexagonal (ports & adapters):** `Domain` has no dependencies; `Application` defines the ports
  (`IProductRepository`) and the use cases; `Infrastructure` implements the ports with a dummy
  in-memory adapter; `Api` is the driving adapter (REST controllers) and acts as the composition
  root.
- **Vertical slicing:** inside `Application`, each CRUD operation (`Create`, `Update`, `Delete`,
  `GetById`, `GetAll`) is an independent folder with its own Command/Query, Handler and Validator.

## Repository structure

```
FairPlay.Sports.slnx
src/
  FairPlay.Sports.Domain/          Product entity and business rules
  FairPlay.Sports.Application/     Use cases (vertical slices) + ports
  FairPlay.Sports.Infrastructure/  Dummy in-memory adapter
  FairPlay.Sports.Api/             REST API (ASP.NET Core)
tests/
  FairPlay.Sports.Domain.Tests/
  FairPlay.Sports.Application.Tests/
  FairPlay.Sports.Infrastructure.Tests/
  FairPlay.Sports.Api.Tests/       Controller unit tests + integration tests (WebApplicationFactory)
frontend/
  fairplay-sports-web/             Vue 3 + TypeScript + Vite + Pinia (dummy login/register)
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20 or higher (includes npm)
- Visual Studio 2026 (optional, to open `FairPlay.Sports.slnx` with the full IDE)

## Backend (.NET 10 API)

From the repository root:

```powershell
# Restore and build the whole solution
dotnet build FairPlay.Sports.slnx

# Run all tests (NUnit)
dotnet test FairPlay.Sports.slnx

# Run the API
cd src/FairPlay.Sports.Api
dotnet run
```

The API will be available at:

- `http://localhost:5228`
- `https://localhost:7090`

In the `Development` environment, interactive API documentation is available via **Swagger UI** at
`/swagger`, backed by the OpenAPI document generated at `/openapi/v1.json`. You can also use
`src/FairPlay.Sports.Api/FairPlay.Sports.Api.http` (compatible with the Visual Studio / VS Code
HTTP client) to try out the `api/products` CRUD without a browser.

## Frontend (Vue 3 + TypeScript + Vite)

From the repository root:

```powershell
cd frontend/fairplay-sports-web

# Install dependencies
npm install

# Run in development mode
npm run dev
```

By default it is served at `http://localhost:5173`. Available routes:

- `/login` — sign-in screen (dummy)
- `/register` — registration screen (dummy)
- `/dashboard` — protected screen reachable after signing in/registering

To build the production bundle:

```powershell
npm run build
```

> Login/register are **dummy**: there is no real authentication backend or persistence; the
> "user" only lives in memory (Pinia store) for the duration of the browser session.

## Opening in Visual Studio 2026

Open `FairPlay.Sports.slnx` directly. The solution includes both the .NET projects (`src/`,
`tests/`) and the frontend's JS/TS project (`frontend/fairplay-sports-web/fairplay-sports-web.esproj`),
with native Visual Studio support for running npm scripts (`dev`, `build`) from the IDE.

## Notes

- The product CRUD data is **dummy**: it is seeded in memory when the API starts and lost when it
  restarts (no real database).
- CORS is enabled for `http://localhost:5173`, in case the frontend is later wired up to the real
  backend.
