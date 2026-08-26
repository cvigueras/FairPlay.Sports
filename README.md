# FairPlay Sports

Proyecto de ejemplo que demuestra una **arquitectura hexagonal** combinada con **vertical slicing**
en .NET 10, con un CRUD RESTful dummy (datos en memoria) y un frontend en Vue 3 con pantallas
dummy de login y registro.

> 🤖 **Hecho con Claude AI.** Este proyecto se ha generado íntegramente con Claude AI, aplicado a
> lo largo de todo el **SDLC** (*Software Development Life Cycle* — Ciclo de Vida de Desarrollo de
> Software): diseño de la arquitectura, definición de la estructura del repositorio,
> implementación del backend y el frontend, escritura de los tests, configuración de la solución
> de Visual Studio y control de versiones con Git/GitHub.

## Arquitectura

- **Hexagonal (ports & adapters):** `Domain` no depende de nada; `Application` define los puertos
  (`IProductRepository`) y los casos de uso; `Infrastructure` implementa los puertos con un
  adaptador dummy en memoria; `Api` es el adaptador de entrada (controladores REST) y actúa como
  composition root.
- **Vertical slicing:** dentro de `Application`, cada operación del CRUD (`Create`, `Update`,
  `Delete`, `GetById`, `GetAll`) es una carpeta independiente con su propio Command/Query,
  Handler y Validator.

## Estructura del repositorio

```
FairPlay.Sports.slnx
src/
  FairPlay.Sports.Domain/          Entidad Product y reglas de negocio
  FairPlay.Sports.Application/     Casos de uso (vertical slices) + puertos
  FairPlay.Sports.Infrastructure/  Adaptador dummy en memoria
  FairPlay.Sports.Api/             API REST (ASP.NET Core)
tests/
  FairPlay.Sports.Domain.Tests/
  FairPlay.Sports.Application.Tests/
  FairPlay.Sports.Infrastructure.Tests/
  FairPlay.Sports.Api.Tests/       Tests unitarios del controlador + integración (WebApplicationFactory)
frontend/
  fairplay-sports-web/             Vue 3 + TypeScript + Vite + Pinia (login/registro dummy)
```

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20 o superior (incluye npm)
- Visual Studio 2026 (opcional, para abrir `FairPlay.Sports.slnx` con IDE completo)

## Backend (.NET 10 API)

Desde la raíz del repositorio:

```powershell
# Restaurar y compilar toda la solución
dotnet build FairPlay.Sports.slnx

# Ejecutar todos los tests (NUnit)
dotnet test FairPlay.Sports.slnx

# Levantar la API
cd src/FairPlay.Sports.Api
dotnet run
```

La API quedará disponible en:

- `http://localhost:5228`
- `https://localhost:7090`

En entorno `Development`, el documento OpenAPI está en `/openapi/v1.json`. También puedes usar el
archivo `src/FairPlay.Sports.Api/FairPlay.Sports.Api.http` (compatible con el cliente HTTP de
Visual Studio / VS Code) para probar el CRUD de `api/products` sin necesidad de Swagger UI.

## Frontend (Vue 3 + TypeScript + Vite)

Desde la raíz del repositorio:

```powershell
cd frontend/fairplay-sports-web

# Instalar dependencias
npm install

# Levantar en modo desarrollo
npm run dev
```

Por defecto se sirve en `http://localhost:5173`. Rutas disponibles:

- `/login` — pantalla de inicio de sesión (dummy)
- `/register` — pantalla de registro (dummy)
- `/dashboard` — pantalla protegida a la que se accede tras loguearse/registrarse

Para compilar la versión de producción:

```powershell
npm run build
```

> El login/registro son **dummy**: no hay backend de autenticación real ni persistencia; el
> "usuario" solo vive en memoria (store de Pinia) mientras dura la sesión del navegador.

## Abrir en Visual Studio 2026

Abre `FairPlay.Sports.slnx` directamente. La solución incluye tanto los proyectos .NET (`src/`,
`tests/`) como el proyecto JS/TS del frontend (`frontend/fairplay-sports-web/fairplay-sports-web.esproj`),
con soporte nativo de Visual Studio para ejecutar los scripts de npm (`dev`, `build`) desde el IDE.

## Notas

- Los datos del CRUD de productos son **dummy**: se siembran en memoria al arrancar la API y se
  pierden al reiniciarla (no hay base de datos real).
- CORS está habilitado para `http://localhost:5173`, por si en el futuro se quiere conectar el
  frontend al backend real.
