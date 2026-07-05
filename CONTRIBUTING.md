# Cómo trabajamos en este repositorio

## Ramas

| Rama | Regla |
|---|---|
| `main` | Siempre instalable. **Protegida**: solo entra código por PR aprobado. Nunca se pushea directo. |
| `develop` | Integración diaria. Sale de `main`, recibe los `feature/*`. |
| `feature/<módulo>-<qué>` | Una por tarea de ClickUp. Ej: `feature/pos-cierre-caja-z`. Sale de `develop`, vuelve a `develop` por PR. |
| `fix/<qué>` | Hotfix durante el piloto. Sale de `main`, vuelve a `main` y se mergea a `develop`. |

## Flujo de una tarea

1. Tomar la tarjeta en ClickUp y moverla a **En curso**
2. `git switch develop && git pull && git switch -c feature/...`
3. Programar con commits pequeños y frecuentes
4. `dotnet test` en verde → push → abrir PR a `develop` (la plantilla guía)
5. Mover la tarjeta a **En revisión**; otro integrante revisa (A revisa a B, B a C, C a A)
6. Merge → borrar la rama → tarjeta a **Hecho**

## Commits

Formato: `tipo: descripción en presente`

- `feat:` nueva funcionalidad — `feat: regla FEFO selecciona lote más próximo a vencer`
- `fix:` corrección — `fix: cierre Z no sumaba notas de crédito`
- `refactor:`, `test:`, `docs:`, `chore:`

## Reglas de oro

1. **PR con tests en rojo no se mergea.** Sin excepciones.
2. **Toda regla que mueva stock o dinero nace con su test** (`Casor.Tests`).
3. **Dinero en `decimal`, nunca `float`/`double`.**
4. **Secretos jamás al repo** (el `.gitignore` ayuda, pero la responsabilidad es de cada uno).
5. La UI no toca la base de datos: siempre a través de `Casor.Application`.
