---
description: Crea una Pull Request de la rama actual hacia main a partir de sus commits
argument-hint: [nota opcional sobre el enfoque de la PR]
allowed-tools: Bash(git branch:*), Bash(git status:*), Bash(git log:*), Bash(git diff:*), Bash(git push:*), Bash(git fetch:*), Bash(gh pr create:*), Bash(gh pr view:*), Bash(gh pr list:*)
---

## Contexto (se rellena solo al invocar el comando)

- Rama actual: !`git branch --show-current`
- Estado: !`git status --short`
- Commits de esta rama respecto a main: !`git log --pretty=format:'%h %s%n%b' origin/main..HEAD`
- Diff frente a main: !`git diff --stat origin/main...HEAD`
- PRs abiertas en el repo (localiza la de la rama actual por el nombre de rama de arriba): !`gh pr list --state open`

## Tarea

Crea **una Pull Request** para mergear la rama actual en `main`.

### Reglas de parada (comprobar en este orden)

1. **Si la rama actual es `main`**: no hagas nada. Indícame textualmente que
   no es posible crear la PR estando en `main` y que debo cambiarme a una
   rama de trabajo primero (`git switch -c <rama>`). Termina aquí.
2. Si no hay commits en `origin/main..HEAD`: avísame de que la rama no
   tiene nada que mergear y termina.
3. Si ya hay una PR abierta para esta rama: no crees otra, muéstrame su URL
   y termina.

### Preparación

- Ejecuta `git push -u origin HEAD` para publicar la rama y fijar upstream.

### Contenido de la PR

El cuerpo de la PR se construye **a partir de todos los commits de la rama**
(`git log origin/main..HEAD`), pero **redactado como una PR**, no como un
volcado de mensajes de commit:

- Lee el asunto y el cuerpo de cada commit y sintetiza la intención global.
- Agrupa los cambios por tema/capa, no por commit.
- Reescribe en prosa + bullets orientados al revisor (qué aporta el conjunto
  y por qué), en inglés, envolviendo a ~72 columnas.
- No incluyas hashes ni una lista literal de commits.

Estructura del cuerpo (Markdown, en inglés):

```
## Summary
Uno o dos párrafos cortos con el objetivo global de la rama.

## Changes
- Bullet por área/capa tocada, derivado del conjunto de commits.

## Testing
- Cómo se ha validado (tests, build, manual), o "not run" si no aplica.
```

Si $ARGUMENTS no está vacío, úsalo como indicación sobre en qué centrar la PR.

### Título de la PR: Conventional Commits

`<type>(<scope>): <descripción>`

- Mismos `type`/`scope` que `/commit` (`feat`, `fix`, `refactor`, `test`,
  `docs`, `build`, `ci`, `chore`, `perf`, `style`; scopes `api`,
  `application`, `infrastructure`, `domain`, `products`, `frontend`, `ci`,
  `deps`).
- Si la rama tiene un solo commit, reutiliza su asunto.
- Si tiene varios, resume el conjunto con el `type` de mayor impacto
  (feat > fix > refactor > test > docs > build > ci > chore).
- Imperativo, en inglés, minúscula inicial, sin punto final, <= 72 caracteres.

### Ejecución

```
gh pr create --base main \
  --title "<título>" \
  --body "$(cat <<'EOF'
<cuerpo>

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

### Después de crear

- Muéstrame la URL de la PR.
- **No hagas merge** (lo haré yo desde GitHub).
