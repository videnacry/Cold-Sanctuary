# Desbloqueo por aprendizaje — elementos y posturas

Aclaración de diseño (2026-07-10). Corrige el malentendido del "desbloqueo por nivel".

## El modelo correcto: se desbloquea aprendiendo, no subiendo de nivel

El desbloqueo **no** es por nivel de personaje. Es por **aprender** la cosa concreta:

- **Encantamientos / química** — cuando el jugador **aprende a materializar un elemento**, ese
  elemento **se añade a la UI que corresponde** (menú/paleta de materialización), y a partir de ahí
  **el jugador puede invocarlo** cuando quiera.
- **Asanas / posturas** — cuando el jugador **aprende una postura** (p.ej. "pararse con las piernas
  separadas mirando al frente"), esa **postura de piernas se añade a su UI** correspondiente, y
  puede **invocarla cuando quiera**.

Es decir: **aprender X → X aparece en su UI → X es invocable.** La UI es el reflejo de lo aprendido.

## Implicación para el código actual

- `CombatAbility` hoy se filtra por `unlockLevel` + `requiredElement` en `CombatAbilityBar.RefreshBar()`.
  El eje "nivel" no encaja con este modelo; lo que debe gatear la aparición es **haber aprendido**
  la habilidad/elemento/postura (un set de "aprendidos"), no un nivel numérico.
- **Bug relacionado (refresco en vivo):** `RefreshBar()` solo se llama en `Awake()`. Aunque el
  jugador aprenda algo nuevo en partida, **la barra/UI no se reconstruye** hasta recargar la escena.
  Con este modelo, aprender algo **debe** re-disparar el refresco de la UI correspondiente
  inmediatamente (suscribirse a un evento tipo `OnLearned(elemento/postura/habilidad)`).

## UIs afectadas

- **Paleta de materialización** (`UI/Palette/`) — elementos/hechizos aprendidos.
- **Menú holográfico** (`UI/Hologram/`) — atajos de asanas/encantamientos aprendidos
  (hoy `OpenAsanasShortcuts`/`OpenHechizosShortcuts` son no-ops sin backing de "aprendidos").
- **Detección de asanas** (`Asana/`) — `Asana.ShortcutUnlocked` a las 10 prácticas ya modela una
  forma de "aprendizaje por repetición"; alinear con este modelo (aprendido → aparece/invocable).

## Pendiente

- Un registro de "aprendidos" (por jugador) para elementos, posturas y habilidades.
- Evento `OnLearned(...)` que re-construya la UI afectada al vuelo.
- Sustituir el gating por `unlockLevel` de `CombatAbility` por gating por "aprendido".
