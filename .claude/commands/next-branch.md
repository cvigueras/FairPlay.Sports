---
description: Vuelve a main, actualiza, crea la rama del siguiente número y borra las ramas locales de trabajo
argument-hint: [slug opcional para la rama, p. ej. matches-pagination]
allowed-tools: Bash(git checkout:*), Bash(git switch:*), Bash(git pull:*), Bash(git fetch:*), Bash(git branch:*), Bash(git for-each-ref:*), Bash(git status:*), Bash(git rev-parse:*)
---

Prepara una rama nueva limpia para el siguiente trabajo y elimina las ramas
locales de trabajo ya cerradas. Pensado para lanzarlo justo después de `/pr`,
cuando ya has terminado con la rama anterior.

## Contexto (se rellena solo al invocar el comando)

- Argumento (slug opcional, kebab-case): $ARGUMENTS
- Rama actual: !`git branch --show-current`
- Estado: !`git status --short`
- Ramas locales: !`git for-each-ref --format='%(refname:short)' refs/heads/`
- Ramas remotas: !`git for-each-ref --format='%(refname:short)' refs/remotes/origin/`

## Reglas de parada

1. Si el working tree tiene cambios sin commitear (el estado de arriba no está
   vacío): **para** y dime que commitee o haga `git stash` antes. No sigas.

## Pasos

1. `git checkout main`
2. `git pull --ff-only`. Si falla porque `main` local ha divergido, **para** y
   avísame (no fuerces).
3. **Calcula el siguiente número**: de todas las ramas locales y remotas cuyo
   nombre empiece por exactamente 6 dígitos, coge el mayor, súmale 1 y deja 6
   dígitos con ceros a la izquierda. Ej.: si la mayor es `000019-pagination` →
   `000020`.
4. Nombre de la rama nueva: `<NNNNNN>` si no hay argumento, o
   `<NNNNNN>-<slug>` si has pasado uno.
5. `git checkout -b <nombre>`.
6. **Borra todas las ramas locales de trabajo**: por cada rama local que NO sea
   `main` ni la recién creada, `git branch -D <rama>`. Lista las que borras.
   Es *force delete* a propósito: se asume que su trabajo ya está pusheado o
   mergeado; sus ramas remotas y sus PRs **no se tocan**.
7. `git fetch --prune` para limpiar las refs de `origin` de ramas ya borradas
   en el remoto.

## Cierre

- Muéstrame `git branch` (debe quedar solo `main` y la rama nueva) y el nombre
  de la rama nueva.
- **No** commitees ni pushees nada.
