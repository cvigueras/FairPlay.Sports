---
description: Crea un commit con los cambios actuales usando Conventional Commits
argument-hint: [nota opcional sobre el enfoque del mensaje]
allowed-tools: Bash(git add:*), Bash(git status:*), Bash(git diff:*), Bash(git log:*), Bash(git commit:*)
---

## Contexto (se rellena solo al invocar el comando)

- Rama actual: !`git branch --show-current`
- Estado: !`git status --short`
- Cambios: !`git diff --stat`
- Diff completo (staged + unstaged): !`git diff HEAD`
- Últimos commits (referencia de estilo): !`git log --oneline -8`

## Tarea

Crea **un único commit** con TODOS los cambios del working tree (`git add -A`).

### Formato del mensaje: Conventional Commits

Asunto: `<type>(<scope>): <descripción>`

- **type** (obligatorio), uno de:
  - `feat` — nueva funcionalidad
  - `fix` — corrección de bug
  - `refactor` — cambio de código que no altera comportamiento observable
  - `test` — añadir o ajustar tests
  - `docs` — solo documentación (README, comentarios)
  - `build` — sistema de build o dependencias (`.csproj`, paquetes NuGet/npm)
  - `ci` — workflows de GitHub Actions
  - `chore` — tareas varias sin impacto en src ni tests
  - `perf` — mejoras de rendimiento
  - `style` — formato, sin cambio de lógica
- **scope** (opcional pero preferible): área tocada. En este repo suele ser:
  `api`, `application`, `infrastructure`, `domain`, `products`, `frontend`, `ci`, `deps`.
  Si el cambio cruza varias capas, usa el scope funcional (`products`) o omítelo.
- **descripción**: imperativo, en inglés, minúscula inicial, sin punto final, <= 72 caracteres.
- **Breaking change**: añade `!` antes de los dos puntos (`feat(api)!: ...`) y un bloque
  `BREAKING CHANGE: <explicación>` al final del cuerpo.

Cuerpo (tras una línea en blanco):
- Bullets (`- ...`) con el *qué* y el *por qué*, no el *cómo*.
- Envuelve a ~72 columnas.
- Trailers al final si aplican (`Refs #123`, `Co-Authored-By: ...`).

Elige el `type` mirando el diff: si toca `src/` y añade capacidad → `feat`; si solo toca
`tests/` → `test`; si solo `*.csproj`/paquetes → `build`; si solo `.github/workflows` → `ci`.
Si hay varios tipos de cambio, prioriza el de mayor impacto (feat > fix > refactor > test > docs > build > ci > chore).

### Ejemplos válidos para este repo

- `feat(products): add stock-adjustment command and endpoint`
- `refactor(application): dispatch product use cases through MediatR`
- `test(application): cover ValidationBehavior short-circuit path`
- `build(deps): pin MediatR to 12.4.1`
- `ci: bump GitHub Actions to v5`
- `docs(readme): document the CQRS + MediatR setup`

### Después de commitear

- Muéstrame `git log --oneline -3`.
- **No hagas push** (lo haré yo).

Si $ARGUMENTS no está vacío, tenlo en cuenta como indicación sobre en qué centrar el mensaje.

<!--
  Ideas para adaptarlo tú luego:
  - Restringir la lista de types/scopes permitidos.
  - Asunto en español.
  - Que haga push automáticamente si la rama no es main.
  - Partir en varios commits por type (p. ej. refactor de src/ y test/ por separado).
  - Añadir validación con commitlint en un hook.
-->
