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
- ~19.126 líneas, 189 scripts, 115 commits. Verificación completa en [`docs/AUDIT-2026-07-09.md`](docs/AUDIT-2026-07-09.md).
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
| Generación | `IFactory.cs`, `BirdBehavior.cs`, `Respawn.cs` | Funciona; off-by-one en `Respawn`. `Generator.cs` (spawner por área, scene-wireable) sin invocadores en código |
| Mundo/áreas | `Assets/Scripts/World/`, `Mission/` | Funciona; `AreaClear` incompleto (falta `KitchenCombatManager`) |
| Compañeros/Bond | `Assets/Scripts/Companion/`, `Bond/` | Funciona; `BondActivityManager` sin cablear a UI. `NPCBase` pendiente |
| Diálogo | `Assets/Scripts/Dialogue/` | Completo y cableado |
| Asanas | `Assets/Scripts/Asana/` | Funciona; conectado a `PlayerStats` (ya no pendiente) |
| Combate | `Assets/Scripts/Combat/` | **Implementado; jugador cableado.** `NPCCombatBehavior` sin cablear |
| Economía | `Assets/Scripts/Economy/` | **Implementado; núcleo jugador cableado.** `NPCEconomy`/`AreaVendor` inertes. **+ recursos de santuario** (`SanctuaryResources`/`AreaProducer` + HUD, docs world-topology §4/§7) |
| Química (tabla periódica) | `Assets/Scripts/Chemistry/` | Implementado y cableado (~55 elementos) |
| Cocina (miniaturización) | (retirada) | La cocina legacy (`KitchenEntrance`/`KitchenScaleController`) se **borró** 2026-07-23. La entrada migró al trigger universal `VirtualizationMachine` (Meditation) + `RealityShiftController` (miniaturización genérica por área) + `MobWorldLoader` (mundo mob en escena). Ver fila **Meditación / Microcosmos** |
| Ropa/crafting | `Assets/Scripts/Clothing/` | **Parcial/sin cablear**: crafting no entrega item |
| UI (FollowingArrays/Palette/Hologram) | `Assets/Scripts/UI/` | Complejo; `MaterializationExecutor` inalcanzable (faltan evaluadores) |
| Interacción | `Assets/Scripts/Interaction/` | Funciona; cableado |
| Meditación / Microcosmos | `Assets/Scripts/Meditation/` (23 archivos) | Implementado. `VirtualizationMachine` (trigger universal de misiones mob), `MeditationSession`, `MeditationMissionBase` + misiones (Healing/Protection/Channel/AsanaFormation/PostureVisualization/RootInquiry), arquetipos de mob, `RealityShiftController`, `LotusMeditationAbility`. Ver `docs/magic-plane-and-meditation.md` |
| Mundo mob | `Assets/Scripts/MobWorld/` (5 archivos) | Implementado. `MobResident`, `MobWorldDirector`, `YogaPortal`, `MobWorldLoader`, `MobSpawnPoint`; builder en `Assets/Editor/MobWorldSceneBuilder.cs`. Ver `docs/mob-world-architecture.md` |
| Avatares (Microcosmos) | `Assets/Scripts/Avatar/` (3 archivos) | Implementado. `SurfaceWalker`, `AvatarController`, `RobotAvatar` (enum `AvatarLocomotion`: Ground/Climb/Flight). Ver `docs/magic-plane-and-meditation.md` §4 |
| Progresión / alma | `Assets/Scripts/Progression/` | `CharacterLevel` (**margas del alma**: Stats/Yoga/Vínculos, tracks independientes) + `DerivedStats` (aptitudes→**puntos del alma**: vida/energía/maná/defensa/poder) + `SoulMarga`. Aptitudes universales vía `IAptitudes`. Ver `docs/creature-stats.md` §Progresión |
| Farming (juego no-violento) | `Assets/Scripts/Farming/` | `PlayableCreature`/`PlayController` (bajar tensión jugando → recursos/XP/items; gateado por bond, puede dañar), `FarmingSandboxItems`, `Climbable`/`PlayerClimber` (trepar). Ver `docs/world-topology-and-planes.md` §4.1 |
| Mente (pilar) | `Assets/Scripts/Mind/` | `Mind` (piensa por tono/frases), `Humores`, `ElementalTone`, `MindPhrase`/`PhraseLibrary`+`PhrasePools` (vivencias/deseos/históricos), `ThoughtField` (campo social/semántico), `PhraseDistribution` (reparto Estricta/Libre + bloqueo). Ver `docs/anima-architecture.md` §11 |
| Control / posesión | `Assets/Scripts/Control/` | `AnimaController` cede el mando al `IBrain` de mayor relevancia; `AiBrain` (IA) vs `PlayerBrain` (input, mueve el cuerpo); `PlayerCore` (input persistente + cambio de cuerpo); `FollowBrain` (seguir); `PossessionSpell` (posee en runtime); `HelpRequest`/`HelpResponder` (petición sí/no → alma compartida). Ver `docs/anima-architecture.md` §11.5/§11.7 |
| Cocina (simulación) | `Assets/Scripts/Kitchen/` | Paso A: `DirtArea`/`DirtSpot`/`Cleaner` (suciedad real → misión → limpieza) + **paseo** `GuidedTour`/`TourStation`. Paso B (MVP): `BreakfastCook` (cadena de desayuno) → `FoodContainer` (se rellena) → `Eater` (come). Ver `docs/kitchen-simulation.md` |
| Virtualización (interacción) | `Assets/Scripts/Virtualization/` | Motor genérico (todas las áreas): `VirtualPointer` (**mira fija al centro**; ratón/touch = su cursor), `HeadLook` (apuntas girando la cabeza=cámara con I/K/J/L, con restricciones), `StationPart` (parte manipulable, `.timed` opcional), `ProductionOrder` (receta por pasos → cuota = sustento), `TypingChallenge` (acción temporizada que se acelera tecleando; `.Active` congela cámara/mira). Ver `docs/kitchen-simulation.md` §3b/§4b |
| Herramientas Editor | `Assets/Editor/` | `SampleSceneBuilder` cablea casi todo el escenario |
| Debug | `Test.cs` | Sin uso en flujo principal |

## Abstracciones principales

- `Anima` (**antes `LivingEntity`**, renombrada 2026-07-28) — **clase única de todo ser** (animado o inanimado-despertable; ver [`docs/anima-architecture.md`](docs/anima-architecture.md)): drives (`stress`, `trauma`, `fatReserves`, `aware`), bonds, hooks abstractos (`RespondToHunger`, `RespondToThreat`, `EvaluateThreat`), y **hogar de las 12 aptitudes** (implementa `IAptitudes`). `Animal` y `CompanionBase` heredan de ella; `PlayerStats` pendiente.
- `IAptitudes`: las **12 aptitudes universales** (agility/perception/strength/bodyMass/adaptability/composure/endurance/reasoning/memory/creativity/sociability/discipline). Implementan `Anima` (y por herencia `Animal`/`CompanionBase`) y `PlayerStats` (mapeo parcial). `DerivedStats` deriva de ellas los **puntos del alma**.
- `CharacterLevel` + `SoulMarga`: progresión por **margas del alma** (tracks independientes: Stats/Yoga/Vínculos) → suben los puntos del alma. Ver `docs/creature-stats.md` §Progresión.
- `IMind` / `IMindSimple`: stats mentales. `PlayerStats` implementa `IMind` completa; `CompanionBase` usa `IMindSimple` (transitoria hasta NPCBase).
- `IBody`: stats físicas por extremidad + estrés postural (sistema de asanas). Implementa: `PlayerStats`.
- `IBondable`: vínculo con el jugador y efecto por proximidad. Implementa: `CompanionBase`, `WorldBondable`.
- Jerarquía animal: `Anima` → `Animal` → `Carnivore`/`Herbivore` → `WolfBehavior`/`BunnyBehavior`/`BearBehaviour`. Y `Anima` → `CompanionBase` → `Goluis`/`Panterilia`/…
- `NPCBase` **descartado** como clase intermedia (2026-07-25): se va a **una sola `Anima`** + pilares/mente opcionales. `IMindSimple` eliminado (2026-07-28).
- `LifeStage` (abstracta) → `Childhood`/`Adolescence`/`Adulthood`.
- **Control intercambiable**: `AnimaController` + `IBrain` (`AiBrain`/`PlayerBrain`) — el ser lo conduce el
  cerebro de mayor **relevancia**; el jugador es "solo un input" (un `PlayerBrain`) que la **posesión**
  (`PossessionSpell`, power/range) inyecta en runtime. Ver `docs/anima-architecture.md` §11.5.

## Documentación detallada (leer bajo demanda)

- [`docs/AUDIT-2026-07-09.md`](docs/AUDIT-2026-07-09.md) — verdad del código verificada archivo a archivo (bugs, huérfanos, prometido-vs-hecho).
- [`docs/checklist.md`](docs/checklist.md) — **empezar aquí para continuar**: tablero de tareas pendientes.
- [`docs/testing-checklist.md`](docs/testing-checklist.md) — qué probar en el editor (progresión/margas, farming, recursos, trepar, cocina→Microcosmos, regresión).
- [`docs/mind-model.md`](docs/mind-model.md) — modelo de mente/IA emergente de personajes: utility AI + pensamientos (memoria acotada) + campo social/semántico + relaciones, por tiers; efecto mariposa; survey de técnicas.
- [`docs/anima-architecture.md`](docs/anima-architecture.md) — visión unificada: **todo es un ser (Anima)** con nivel de consciencia configurable (piedra/viento incluidos); pilares (Body/Mind/Bond) por composición; elementos como personalidad + instancias compartidas; mente por "frases" (ciclo de vida); body-swap; taxonomía de nombres (aptitudes/margas/puntos del alma/habilidades).
- [`docs/gaps-vs-planteamiento.md`](docs/gaps-vs-planteamiento.md) — sistemas "hechos" que no cumplen el diseño + cómo cablear los huérfanos.
- [`docs/creature-stats.md`](docs/creature-stats.md) — aptitudes (agilidad/percepción/fuerza/masa) de animales y NPCs; perfiles Goluis/Panterilia/Gohageneis/Irosene.
- [`docs/character-irosene.md`](docs/character-irosene.md) — ficha del personaje Irosene (compañera motivacional; biografía, diálogo, árbol familiar, arco).
- [`docs/kitchen-simulation.md`](docs/kitchen-simulation.md) — **la Cocina como primera simulación jugable** (nivel de referencia): paseo/onboarding, progresión de rol (limpieza→cocina→recetas), química de la comida (compuestos→humores→aptitudes), suciedad real, puente Micro/Meso (minidrones), mundo-insecto, puente al santuario; orden de construcción.
- [`docs/garden-simulation.md`](docs/garden-simulation.md) — **el Huerto (2ª simulación, Neolítico)**: territorio (Creciente Fértil), actividades (domesticar/sembrar/cosechar/almacenar/proteger), giro del excedente, históricos (La Sembradora + fila neolítica), y el **arquetipo de misión "detener conflictos" (mediación)** basado en pensamientos.
- [`docs/area-progression.md`](docs/area-progression.md) — **roadmap de áreas** alineado con la línea temporal del microworld (Cocina→Huerto→Forja→Enfermería→Yoga…): región/época/hilo/históricos/giro por área; cómo se encadena la historia.
- [`docs/forge-simulation.md`](docs/forge-simulation.md) — **la Forja/Mecánica (3ª área, Edad de los Metales)**: territorio (Mesopotamia), giro arado/espada, históricos (El Primer Herrero, Ötzi, Sargón), estaciones + receta de bronce (con typing en el crisol); cómo continúa la historia (fuego→metal).
- [`docs/founding-trio-stories.md`](docs/founding-trio-stories.md) — **historias entrelazadas del grupo fundador** (Señor del Fuego · Ötzi · La Sembradora) con Kushal de hilo conductor; misiones que salen de cada beat, y guías/correcciones de historia real y botánica. Ötzi es de la **era temprana** (no de la Forja).
- [`docs/mission-mode.md`](docs/mission-mode.md) — modo misión, disparador-personaje, contadores y economía circular del santuario.
- [`docs/world-topology-and-planes.md`](docs/world-topology-and-planes.md) — visión de mundo: la tríada de planos (**Microcosmos** interior / **Mesocosmos** estándar a pie / **Macrocosmos** exterior tipo RTS), hub-and-spoke con gradiente peligro=dispersión, los 5 santuarios (terrestre/marino vertical/aéreo/subterráneo/núcleo de 2 capas plasma+diamante), acoplamiento Meso↔Macro (tiempo lento vs WC3, guerras en ambos modos), Macrocosmos (economía, farming como juego, transporte, cámara 2D) y progresión guionizada+emergente.
- [`docs/magic-plane-and-meditation.md`](docs/magic-plane-and-meditation.md) — Microcosmos (el plano interior; uno de los tres planos — ver [`docs/world-topology-and-planes.md`](docs/world-topology-and-planes.md)), máquina de virtualización (trigger universal), avatares-robot (gusano/araña/mosco/loto), capas de escala, meditación en yoga y arquetipos de mob.
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
