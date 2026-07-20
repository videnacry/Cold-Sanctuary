# Cold Sanctuary

Juego Unity (C#) ambientado en un santuario animal mágico. Un grupo llega buscando
aprender los secretos de la maestra del lugar; para acceder a ellos deben trabajar como
voluntarios cuidando crías, conviviendo con compañeros y practicando yoga.

El jugador tiene stats físicas (por extremidad, estrés postural) y mentales (satisfacción,
fatiga mental, estrés, sueño, observación) que crecen con la práctica y el tiempo con
compañeros. Los animales nacen en familias, pasan por etapas de vida, comen, forman
vínculos y responden a amenazas. La UI declarativa ("FollowingArrays / Palette") expone
asanas, encantamientos por tabla periódica y actividades de vínculo.

## Estado del repositorio

- **Solo se versiona código.** El repo contiene únicamente los `.cs` bajo `Assets/`,
  más `.gitignore` y `.gitattributes`. **No** están en git: escenas (`.unity`), prefabs,
  `ProjectSettings/`, `Packages/manifest.json` ni archivos `.meta`. La configuración del
  proyecto Unity y los assets visuales viven fuera de este repositorio.
- ~15.200 líneas, 144 scripts, 30 commits. Verificación completa en [`docs/AUDIT-2026-07-09.md`](docs/AUDIT-2026-07-09.md).
- Idioma de comentarios mezclado: inglés y español. Sin namespaces (todo global).

## Mapa de sistemas

| Sistema | Archivos clave | Estado |
|---|---|---|
| Animales | `Assets/Animals/Animal.cs`, `Carnivore.cs`, `Herbivore.cs` + 8 especies | Funciona; `BearBehaviour` ya hereda de `Carnivore`, completo |
| Post-natal | `Assets/Scripts/PostNatal/` | Funciona; enums `nestType/fatherRole/weaningType` decorativos (no leídos) |
| Etapas de vida | `Assets/Scripts/LifeStage/` | Completo, buena composición |
| Familia/genética | `Family.cs`, `Sex.cs`, `FamilyGenerator.cs` | Completo |
| Tiempo | `Assets/Scripts/Time/Time.cs`, `TimeTest.cs` | Funciona; `TimeTest` es debug |
| Jugador | `Assets/Scripts/Player/PlayerController.cs`, `PlayerStats.cs` | Activo y cableado. `PlayerCtrl` retirado 2026-07-09 |
| Nave/Entorno | `ShipCtrl.cs`, `SlideDoor.cs`, `PullDoor.cs`, `DrivePreparation.cs` | Bugs: `ShipCtrl` `if(1==1)`, `PullDoor.OnCollissionEnter` mal escrito |
| Cámara | `Assets/Scripts/Camera/` | Funciona (robberies, FOV/shake). Modo artístico y `ScreenEffects` pendientes |
| Generación | `IFactory.cs`, `BirdBehavior.cs`, `Respawn.cs` | Funciona; off-by-one en `Respawn`. `Generator.cs` legacy sin uso |
| Mundo/áreas | `Assets/Scripts/World/`, `Mission/` | Funciona; `AreaClear` incompleto (falta `KitchenCombatManager`) |
| Compañeros/Bond | `Assets/Scripts/Companion/`, `Bond/` | Funciona; `BondActivityManager` sin cablear a UI. `NPCBase` pendiente |
| Diálogo | `Assets/Scripts/Dialogue/` | Completo y cableado |
| Asanas | `Assets/Scripts/Asana/` | Funciona; conectado a `PlayerStats` (ya no pendiente) |
| Combate | `Assets/Scripts/Combat/` | **Implementado; jugador cableado.** `NPCCombatBehavior` sin cablear |
| Economía | `Assets/Scripts/Economy/` | **Implementado; núcleo jugador cableado.** `NPCEconomy`/`AreaVendor` inertes |
| Química (tabla periódica) | `Assets/Scripts/Chemistry/` | Implementado y cableado (~55 elementos) |
| Cocina (miniaturización) | `Assets/Scripts/Kitchen/` | Implementado y cableado; riesgo de doble-trigger |
| Ropa/crafting | `Assets/Scripts/Clothing/` | **Parcial/sin cablear**: crafting no entrega item |
| UI (FollowingArrays/Palette/Hologram) | `Assets/Scripts/UI/` | Complejo; `MaterializationExecutor` inalcanzable (faltan evaluadores) |
| Interacción | `Assets/Scripts/Interaction/` | Funciona; cableado |
| Herramientas Editor | `Assets/Editor/` | `SampleSceneBuilder` cablea casi todo el escenario |
| Debug | `Test.cs` | Sin uso en flujo principal |

## Abstracciones principales

- `LivingEntity` (base compartida para Animal y futuros NPCs): drives (`stress`, `trauma`, `fatReserves`, `aware`), bonds, hooks de respuesta (`RespondToHunger`, `RespondToThreat`, `EvaluateThreat`).
- `IMind` / `IMindSimple`: stats mentales. `PlayerStats` implementa `IMind` completa; `CompanionBase` usa `IMindSimple` (transitoria hasta NPCBase).
- `IBody`: stats físicas por extremidad + estrés postural (sistema de asanas). Implementa: `PlayerStats`.
- `IBondable`: vínculo con el jugador y efecto por proximidad. Implementa: `CompanionBase`, `WorldBondable`.
- Jerarquía animal: `LivingEntity` → `Animal` → `Carnivore`/`Herbivore` → `WolfBehavior`/`BunnyBehavior`/`BearBehaviour`.
- `NPCBase` (pendiente): extiende `LivingEntity`, implementa `IMind` + `IBondable`. Compañeros = parámetros solamente.
- `LifeStage` (abstracta) → `Childhood`/`Adolescence`/`Adulthood`.

## Documentación detallada (leer bajo demanda)

- [`docs/AUDIT-2026-07-09.md`](docs/AUDIT-2026-07-09.md) — verdad del código verificada archivo a archivo (bugs, huérfanos, prometido-vs-hecho).
- [`docs/checklist.md`](docs/checklist.md) — **empezar aquí para continuar**: tablero de tareas pendientes.
- [`docs/gaps-vs-planteamiento.md`](docs/gaps-vs-planteamiento.md) — sistemas "hechos" que no cumplen el diseño + cómo cablear los huérfanos.
- [`docs/creature-stats.md`](docs/creature-stats.md) — aptitudes (agilidad/percepción/fuerza/masa) de animales y NPCs; perfiles Goluis/Panterilia/Gohageneis/Irosene.
- [`docs/character-irosene.md`](docs/character-irosene.md) — ficha del personaje Irosene (compañera motivacional; biografía, diálogo, árbol familiar, arco).
- [`docs/mission-mode.md`](docs/mission-mode.md) — modo misión, disparador-personaje, contadores y economía circular del santuario.
- [`docs/magic-plane-and-meditation.md`](docs/magic-plane-and-meditation.md) — plano mágico, máquina de virtualización (trigger universal), avatares-robot (gusano/araña/mosco/loto), capas de escala, meditación en yoga y arquetipos de mob.
- [`docs/area-missions-spec.md`](docs/area-missions-spec.md) — spec consolidada de misiones por área (simulacro + mob; ejes escala/plano; dificultad = requisitos + habilidad); base para cablear todas las áreas.
- [`docs/mob-world-architecture.md`](docs/mob-world-architecture.md) — santuario fractal por escala (un mundo a la vez), yoga-portal, áreas-tienda, NPCs mob ligeros (`MobResident`), mundo vivo por eventos (`MobWorldDirector`), radio expansible, balance tonal y contenido narrativo (reutilizar la Historia).
- [`docs/mob-characters.md`](docs/mob-characters.md) — lista inicial de personajes-mob (históricos/mito de dominio público) con su arco agridulce→aprendizaje, área y misión de ayuda.
- [`docs/mob-quests-early.md`](docs/mob-quests-early.md) — misiones e historia de la era temprana (Edad de Piedra→Metales): cadena de fases y qué revela cada una (Guardián del Fuego, Ötzi detallados; Sembradora/Herrero/Gilgamesh en esquema). Capa previa al diálogo.
- [`docs/mob-epochs-matrix.md`](docs/mob-epochs-matrix.md) — héroes a través del tiempo: matriz época×hilo (10 épocas × 7 hilos, incl. humor "El Bromista"), áreas=civilizaciones/regiones, afinidad y alineación variable; hilos completos A (Fuego→Hawking) y F (Aliento/Mente), cortes de Antigüedad (Grecia) y Metales (Mesopotamia/Egipto).
- [`docs/learning-unlocks.md`](docs/learning-unlocks.md) — desbloqueo por aprendizaje (elementos/posturas aparecen en su UI al aprenderlos).
- [`docs/refuge-and-adult-behavior.md`](docs/refuge-and-adult-behavior.md) — refugio/ocultarse, comportamiento adulto, memoria de lugares (diseño).
- [`docs/architecture.md`](docs/architecture.md) — jerarquía de clases y cada sistema a fondo.
- [`docs/ui-following-arrays.md`](docs/ui-following-arrays.md) — el sistema de UI declarativa.
- [`docs/known-issues.md`](docs/known-issues.md) — bugs y deuda técnica con `archivo:línea`.
- [`docs/conventions.md`](docs/conventions.md) — estilo, patrones y cómo trabajar aquí.
- [`docs/behavior-system.md`](docs/behavior-system.md) — diseño técnico del sistema presa/amenaza/vínculos/post-natal + checklist.
- [`docs/fauna-gameplay.md`](docs/fauna-gameplay.md) — visión y mecánicas de cuidado de crías desde perspectiva de jugador.
- [`DEVLOG.md`](DEVLOG.md) — stats del jugador, asanas, compañeros, narrativa, niveles.
- [`docs/review-checklist.md`](docs/review-checklist.md) — pendientes de implementación, diseño abierto y deuda técnica.

## Cómo mantener esta documentación

Cuando cambie la arquitectura o se resuelvan bugs listados, actualiza el `docs/`
correspondiente y, si afecta al panorama general, este `CLAUDE.md`. Mantén este archivo
**corto** (se carga en cada sesión); el detalle va en `docs/`.
