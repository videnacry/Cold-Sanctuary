# Arquitectura — Cold Sanctuary

Detalle por sistema. Para el panorama general ver [`../CLAUDE.md`](../CLAUDE.md).

## Jerarquía de clases

```
LivingEntity (abstract MonoBehaviour)
│  drives: stress, trauma, fatReserves, temperature, bonds, aware
│  hooks: RespondToHunger(), EvaluateThreat(), RespondToThreat()
│
├─ Animal (abstract, ITarget, IEdible, ICarrier, IFactory)
│  ├─ Carnivore (abstract) → WolfBehavior, BearBehaviour
│  └─ Herbivore (abstract) → BunnyBehavior, DeerBehavior, SealBehavior
│
├─ NPCBase : IMind, IBondable  [pendiente de implementar]
│  ├─ CompanionBase
│  │  ├─ Goluis
│  │  ├─ Panterilia
│  │  ├─ Gohageneis
│  │  └─ Irosene
│  └─ (futuros NPCs con drives reales)
│
├─ BirdBehavior (IFactory)
├─ ShipCtrl  (PlayerCtrl retirado 2026-07-09; jugador = Player/PlayerController)
├─ SlideDoor, PullDoor, DrivePreparation
├─ Respawn, FamilyGenerator, Generator
└─ Test (registro de debug)

LifeStage (abstract serializable)
├─ Childhood
├─ Adolescence
└─ Adulthood

[Serializable] / utilidades
├─ Physiognomy, Family, ActionPrep, ActionsPrep
├─ Sex (utilidad abstracta), AnimationsName
└─ TimeController (singleton estático)
```

### Interfaces

```
IBody  (Assets/Scripts/IBody.cs)
  ├─ GetBodyPartStats(BodyPart): BodyPartStats
  ├─ TrainBodyPart(BodyPart, BodyStatDimension, float): void
  ├─ postureStress: float { get; }
  ├─ AccumulatePostureStress(float): void
  └─ ReleasePostureStress(float): void
  Implementa: PlayerStats

IMind  (Assets/Scripts/IMind.cs)
  ├─ satisfaction, satisfactionCapacity, mentalFatigue, stress, sleepiness, observationRadius
  ├─ RestoreMind(float, MindChannel): void
  └─ DrainMind(float, MindChannel): void
  Implementa: PlayerStats. Implementará: NPCBase

IMindSimple  (Assets/Scripts/IMindSimple.cs)
  ├─ fatigue, stress, mood
  Implementa: CompanionBase (transitorio hasta que llegue NPCBase)

IBondable  (Assets/Scripts/Companion/IBondable.cs)
  ├─ GetBondStrength(): float
  ├─ GrowBond(float amount): void
  └─ GetProximityEffect(MindChannel): float
  Implementa: CompanionBase, WorldBondable

IFactory
  └─ GenerateSquareRange(GameObject, GameObject, int): GameObject[]
```

## Animales (`Assets/Animals/`)

- **`Animal.cs`** — base abstracta. Estado central (hambre, agotamiento, LP, conciencia),
  etapas de vida, familia, conteo de población, navegación NavMesh, daño/muerte y huida.
  Spawning vía `StaticGenerateSquareRange()`. Registra en `Population` y `wholePopulation`.
- **`Carnivore.cs`** — depredador. `Feed()` caza la presa más cercana de la población
  (`BunnyBehavior.population`), gestiona distancia de persecución (700 u), fatiga y daño letal.
- **`Herbivore.cs`** — presa. `Feed()` pasta en casa por el 60% del peso de comida,
  respetando el estado "alimentado" de la familia.
- **`Physiognomy.cs`** — spec corporal serializable (masa base, escala, ratios de comida)
  para calcular nutrición dinámica según masa individual.
- **`ActionPrep.cs`** — estado de UNA acción (idle/walk/run): nombre de animación, velocidad
  NavMesh, velocidad de animación y coste de energía por tick. `Prep()` aplica el estado.
- **`ActionsPrep.cs`** — contenedor de tres `ActionPrep` (idle, walk, run). Ojo con la
  confusión de nombres (ver known-issues).

### Especies

- **`BunnyBehavior.cs`** — herbívoro, crianza materna (familias de 5, 40% padres, escala 8x).
- **`WolfBehavior.cs`** — carnívoro, crianza biparental (familias de 6, 30% padres, escala
  2.5x). Tiene un `Escape()` legacy grande comentado (líneas ~145–275).
- **`BearBehaviour.cs`** — `IAnimal` independiente. Máquina de estados por flags booleanos
  (gender, moving, adult, hunting, alone). Duplica gestión de estado de `Animal`. Incompleto.

## Etapas de vida (`Assets/Scripts/LifeStage/`)

- **`LifeStage.cs`** — base abstracta. Lleva días de etapa, días vividos, potencial de tamaño
  y márgenes de escala. `Live()` es una coroutine que itera por tercios de día ejecutando
  eventos aleatorios (Wander, Rest, Feed, etc.) y avanza a la siguiente etapa.
  Progresión: child → teen → adult → soul (muerte).
- **`Childhood/Adolescence/Adulthood.cs`** — implementan `GrowScale()` (escalado lineal hacia
  el potencial de la siguiente etapa; en Adulthood es no-op).

Los eventos son pools de delegados (`SubEvent`), lo que permite componer comportamientos.

## Familia y genética (`Assets/Scripts/`)

- **`Family.cs`** — estructura serializable: miembros, crías alimentadas, alimentadores
  (padres), alfa macho/hembra, modo de crianza ('p' paterna, 'm' materna, 'b' biparental).
  `RenderFamily()` → `RenderGroup()` + `SetGendersRate()` + `SetParents()`.
- **`Sex.cs`** — utilidad abstracta con constantes 'm'/'f' y `SwitchSex()`.
- **`FamilyGenerator.cs`** — MonoBehaviour que instancia familias preconfiguradas en `Start()`.

## Tiempo (`Assets/Scripts/Time/`)

- **`Time.cs`** — `TimeController` estático. Velocidad como multiplicador (1x–30x+).
  `TimeSpeedMinuteSecs = 60/TimeSpeed` para intervalos de coroutine.
- **`TimeTest.cs`** — componente de debug para ajustar la velocidad desde el Inspector.

## Jugador y nave (`Assets/Scripts/`)

- **`Player/PlayerController.cs`** — controlador **activo** (CharacterController): WASD + salto +
  gravedad, mouse-look al mantener Mouse1, toggle 1ª/3ª persona con 'V' (vía `CameraManager`),
  natación desde `WaterZone`, bloqueo durante diálogo. Sin combate/disparo. Es el que cablea
  `SampleSceneBuilder`.
- **`PlayerCtrl.cs`** — *retirado 2026-07-09*. Era el FPS legacy con disparo/dardo e IK de rifle;
  el santuario es no-violento. Recuperable desde el historial de la rama de auditoría si se
  reusa el rig de puntería.
- **`ShipCtrl.cs`** — helicóptero: WASD pitch/roll/yaw/velocidad, mouse0 free-look, scroll
  zoom, 'F' anima disparo. Fuerza ascendente constante. Bug: placeholders `if(1==1)` (líneas 83/95).

## Entorno (`Assets/Scripts/`)

- **`SlideDoor.cs`** — puerta corredera; `OnMouseDown()` abre/cierra. Gestiona `NavMeshObstacle`.
- **`PullDoor.cs`** — puerta de tirar-y-deslizar en dos fases. `OnCollisionEnter()` baja LP;
  a LP=0 llama `Fall()`.
- **`DrivePreparation.cs`** — transición sentarse/salir de la nave. `SitDown()`/`Exit()`.

## Generación (`Assets/Scripts/`)

- **`Generator.cs`** — array de pares (GameObject, cantidad) + área objetivo. `Fill()` usa
  `IFactory.GenerateSquareRange()` si está disponible, si no `Animal.StaticGenerateSquareRange()`.
- **`BirdBehavior.cs`** — `IFactory`. Bucle de vuelo `Fly()`/`Move()`. Pobla `Respawn.birds`.
- **`Respawn.cs`** — spawner aleatorio por intervalos (60–360s). Listas estáticas: birds,
  bears, rabbits.

## Patrones

- **Factory**: `IFactory.GenerateSquareRange()`, despachado desde `Generator`.
- **Spawning**: posición/escala aleatorias dentro del área; familias vía `RenderFamily()`.
- **Ciclo de vida por delegados**: `LifeStage.Live()` ejecuta pools de eventos por tercio de día.
- **Acciones**: `ActionPrep.Prep()` actualiza animación + NavMesh + energía de forma atómica
  (sin cola de acciones tradicional).

---

## Stats del jugador (`Assets/Scripts/Player/`)

- **`PlayerStats.cs`** — MonoBehaviour en el Player. Stats: `satisfaction` / `satisfactionCapacity` /
  `satisfactionPassiveRate` / `restorationMultiplier`, `mentalFatigue`, `stress`, `sleepiness`,
  `observationRadius`, `velocity`, `physicalResistance`.
  API: `Restore(float, StatChannel)` aplica `restorationMultiplier`; `Drain(float, StatChannel)` lo ignora.
  Candidata a refactorizarse en `IBody` + `IMind` (ver [`ibody-imind.md`](ibody-imind.md)).

---

## Sistema de compañeros (`Assets/Scripts/Companion/`)

- **`IBondable.cs`** — interfaz para cualquier entidad que forme vínculo con el jugador:
  `BondWithPlayer`, `GrowBondWithPlayer(float)`, `GetProximityEffect(StatChannel)`.
- **`ThoughtAnchor.cs`** — `[Serializable]`. Creencia con `key`, `weight` (−1 bloquea, +1 impulsa),
  `changeRatePerDay`. `ShiftToward(float, float)` suaviza el cambio.
- **`CompanionBase.cs`** — base abstracta que implementa `IBondable`. Simula `fatigue`/`stress`/`mood`.
  En Update, aplica efecto por proximidad al Player: restaura o drena según `GetProximityEffect()`.
  Solo `SetupAnchors()` es **abstracto**; `GetMoodModifier()`, `FatigueRatePerSecond()` y
  `GetRestingMood()` son **`virtual`** con default (`1f`, `0.0001f`, `0.7f` respectivamente;
  `CompanionBase.cs:191-206`), igual que los hooks `OnPlayerNearby()`/`OnPlayerLeft()`.
  Además expone ~12 aptitudes `Base*` `virtual` (`CompanionBase.cs:41-52`: `BaseAgility`,
  `BasePerception`, `BaseStrength`, `BaseBodyMass`, `BaseAdaptability`, `BaseComposure`,
  `BaseEndurance`, `BaseReasoning`, `BaseMemory`, `BaseCreativity`, `BaseSociability`,
  `BaseDiscipline`) que cada compañero sobrescribe y `Start()` copia a los campos de instancia.
- **`Companions/Goluis.cs`** — canal MentalFatigue. Drena estrés del jugador por presión. Anchor
  `yoga_skepticism` bloquea asanas hasta que el arco narrativo avanza.
- **`Companions/Panterilia.cs`** — canal MentalFatigue. Añade `+1.5` a `observationRadius` mientras
  el jugador está cerca. Anchor `chemical_reliance` fades, `trust_nature` crece.
- **`Companions/Gohageneis.cs`** — canal Satisfaction. `CelebrationCharge` acumula y libera bursts
  de restauración de Satisfaction + MentalFatigue al alcanzar umbral.
- **`Companions/Irosene.cs`** — canal **Satisfaction** (excepción: la base es MentalFatigue). Su
  pasión llena la moral: `encouragementBurst` inmediato al entrar en rango y `GetMoodModifier()` que
  puede superar `1`. Aptitudes que codifican origen→presente transformado (sociabilidad/creatividad/
  adaptabilidad altas). Anchors (`alpha_and_omega`, `pursue_dreams`, `rebellion`, `pride`…) que se
  suavizan con el vínculo. Ficha en [`character-irosene.md`](character-irosene.md).

---

## Sistema de bond activities (`Assets/Scripts/Bond/`)

- **`BondActivity.cs`** — ScriptableObject. Práctica que construye bond con un `IBondable` por `targetId`.
  Sistema de trauma: estrés alto durante la práctica acumula trauma → cuando `trauma >= blockThreshold`,
  la actividad se bloquea. Se desbloquea cuando `satisfaction >= UnblockThreshold`.
  `passiveTriggers`: lista de context tags que la activan automáticamente.
- **`BondActivityManager.cs`** — MonoBehaviour en el Player. Registra `IBondable`s por ID.
  Gestiona context tags (`SetContextTag` / `ClearContextTag`). Tick pasivo cada frame y tick de trauma
  cada día. `BuildPaletteConfig()` genera `PaletteConfig.Direct` con actividades bloqueadas marcadas.
- **`WorldBondable.cs`** — `IBondable` para objetos y lugares (esterilla, sol, montaña).
  Auto-registra en `BondActivityManager` en Start. Restaura al jugador por proximidad escalado por bond.

Ver diseño completo en [`bond-activity-system.md`](bond-activity-system.md).

---

## Sistema de cámara (`Assets/Scripts/Camera/`)

- **`CameraManager.cs`** — Singleton. Switch entre 3ª y 1ª persona con transición suave.
  `RequestRobbery(CameraRobbery)` para robos cinemáticos (SwitchPerspective / OrbitTarget / CutToAndBack).
  `ApplyStatEffects()`: temblor por fatiga, FOV por estrés, apagones por sueño.
- **`ZoneActivator.cs`** — reemplaza `CameraZoneTrigger`. Wirea en un solo componente: acción de cámara
  (`SwitchMode` / `Robbery` / `None`) + `contextTags` para `BondActivityManager`. Visualiza bounds
  del collider como Gizmo.
- **`CameraZoneTrigger.cs`** — legacy. No añadir más zonas con él; usar `ZoneActivator`.

Ver guía de setup en [`setup-camera-system.md`](setup-camera-system.md) y diseño en [`camera-system.md`](camera-system.md).

---

## Sistema de UI / Palette (`Assets/Scripts/UI/Palette/`)

- **`PaletteConfig.cs`** — configuración del menú de paleta. Modos: `Direct` (botones), `Formula`
  (combinación evaluada por `IPaletteEvaluator`), `Hybrid` (shortcuts directos al llegar a maestría),
  `Dialogue`. Soporte para grupos con `groupThreshold`.
- **`IPaletteEvaluator.cs`** — interfaz de evaluación. `Evaluate(List<PaletteElementData>, GameObject)`
  → `PaletteResult { success, outcomeId, magnitude, spatial }`.
  Implementaciones: `AsanaEvaluator`, `EnchantmentEvaluator` (pendiente), `EnrichmentEvaluator` (pendiente),
  `BlockSpellEvaluator` (pendiente).
- **`AsanaEvaluator.cs`** — implementa `IPaletteEvaluator` para el sistema de asanas. Extrae `BodyPart`s
  del payload, verifica match contra asanas desbloqueadas, maneja shortcut mode con `RollErrors()`.
- **`TrackerBehavior.cs`** — base para botones UI que siguen un target en pantalla (p.ej. `AnimalRadar`).

Ver diseño del sistema Palette en DEVLOG.md §Análisis de sistemas incorporados.

---

## Sistema de observación (`Assets/Scripts/Observation/`) — pendiente de implementar

- **`ObservableObject.cs`** (pendiente) — marker en objetos que el jugador puede percibir dentro de su `observationRadius`.
- **`ObservationManager.cs`** (pendiente) — attached al Player. Detecta objetos en radio y emite señales diegéticas.
- Modo contemplativo (pendiente) — quietud del jugador sube `observationRadius` pasivamente.

Ver diseño completo en [`observation-system.md`](observation-system.md).

---

## Sistema de asanas (`Assets/Scripts/Asana/`)

- **`Asana.cs`** — ScriptableObject. `requiredPositions[]`, `StatType`, maestría, `ShortcutUnlocked`
  a los 10 prácticas. `RollErrors()` para shortcut mode.
- **`AsanaDetector.cs`** — attached al Player. Escucha `BodyPosition` pulsadas, detecta match.
- **`AsanaQueue.cs`** — gestiona la asana activa y cola. Entrega beneficio continuo a sus contenedores
  propios (`Dictionary<StatType, float>`). **Gap actual**: no conectado a `PlayerStats` todavía
  (ver DEVLOG.md §Gaps y conflictos — StatType vs StatChannel).
- **`BodyPosition.cs`** / **`BodyPositionButton.cs`** — datos de posición corporal + botón UI.

---

## Sistemas incorporados no documentados hasta ahora (auditoría 2026-07-09)

Estos sistemas existen en código pero no estaban en esta doc. Estado verificado en
[`AUDIT-2026-07-09.md`](AUDIT-2026-07-09.md). "Cableado" = lo monta `SampleSceneBuilder`.

- **Combate (`Assets/Scripts/Combat/`)** — *implementado; jugador cableado*. Melee por `SphereCast`
  (`PlayerCombat`), tab-target (`CombatTargetSelector`), barra de 10 habilidades con afinidad
  elemental y AOE (`CombatAbilityBar`), enemigos `IngredientMob`. Pendiente: cablear
  `NPCCombatBehavior`, animaciones/VFX (muchos TODO), refresco en vivo de la barra.
- **Economía (`Assets/Scripts/Economy/`)** — *implementado; núcleo jugador cableado*. `CoinWallet`,
  `Inventory` (equipar herramienta/armadura, consumibles), `ItemData`. Pendiente de cablear:
  `NPCEconomy` y `AreaVendor` (economía viva de NPCs, hoy inertes).
- **Química / tabla periódica (`Assets/Scripts/Chemistry/`)** — *implementado y cableado*.
  `PeriodicTableManager` como "Pokédex" de ~55 elementos por grupo; `ElementFragment` como pickup.
  Alimenta el menú holográfico y desbloqueos de combate.
- **Cocina / miniaturización (`Assets/Scripts/Kitchen/`)** — *implementado y cableado*. Escala la
  sala ×8 con lerp de FOV; entrada vía `IInteractable` + `ConfirmationPanel`. Riesgo de doble-trigger.
- **Ropa / crafting (`Assets/Scripts/Clothing/`)** — *parcial, sin cablear*. Pistas Textil/Química;
  el crafting nunca entrega el item (`ClothingRecipe` no tiene `resultItem`) ni consume materiales.
- **Diálogo (`Assets/Scripts/Dialogue/`)** — *implementado y cableado*. `DialogueManager` singleton,
  `DialogueSequence`/`DialogueLine` (ScriptableObjects), `DialoguePanel` (typewriter, efectos),
  `DialogueTrigger`. Assets creados por `Editor/DialogueAssetCreator`.
- **Misiones (`Assets/Scripts/Mission/`)** — *implementado parcial*. `MissionTracker` para
  `IngredientCollection` y `YeastControl`; `AreaClear` incompleto (falta `KitchenCombatManager`).
- **Interacción (`Assets/Scripts/Interaction/`)** — *implementado y cableado*. `IInteractable` +
  `InteractionController` (proximidad + raycast, bloquea durante diálogo/confirmación).
- **Herramientas de Editor (`Assets/Editor/`)** — `SampleSceneBuilder` (blockout + cableado de casi
  todos los sistemas), `AnimalPrefabGenerator`/`AnimalModelImporter` (pipeline de modelos),
  `DialogueAssetCreator`, `SceneDiagnostics`.

### Huérfanos completos (implementados, pendientes de cablear)

No borrar: son trabajo listo para activar. `NPCCombatBehavior`, `NPCEconomy`, `AreaVendor`,
`ClothingCraftingArea`, `Generator` (legacy), `MaterializationExecutor` + `ArrangementPattern`
(inalcanzables hasta que exista `BlockSpellEvaluator`), `TeacherNPC`/`MaestraTeacher` (sin conectar
a `Palette.OnFormulaEvaluated`), `BondActivityManager.Practice()`/`BuildPaletteConfig()`.

---

## Meditación / Microcosmos (`Assets/Scripts/Meditation/`, 23 archivos)

El sistema que lleva al jugador al Microcosmos (el plano interior; uno de los tres planos — ver
[`world-topology-and-planes.md`](world-topology-and-planes.md)), el mundo mob, y ejecuta las misiones mob. Detalle
completo en [`magic-plane-and-meditation.md`](magic-plane-and-meditation.md).

- **`VirtualizationMachine.cs`** — `IInteractable`, el **trigger universal** de misiones mob presente
  en cada área (reemplaza el antiguo flujo de puertas `KitchenEntrance`, borrado 2026-07-23). Al interactuar: `ConfirmationPanel`
  → `onEnterAnimation` → `MeditationSession.Open()`. Tiene dos modos: **in-place** (menú de misiones +
  `RealityShiftController`) o **escena propia** (si `mobWorldSceneName` está relleno, delega en
  `MobWorldLoader.EnterMobWorld`). Interactuar de nuevo durante una misión in-place la termina.
- **`MeditationSession.cs`** — singleton (auto-bootstrap) que orquesta el flujo compartido por la
  máquina y el loto: animación de entrada → **fade a negro** (`ScreenFader`) → `MissionSelectMenu` →
  al elegir, `RealityShiftController.ShiftIn` (snap de escala mientras la pantalla está negra) →
  fade de vuelta → juega la misión. `EndMission()` lo revierte. Expone `IsInMission`/`IsBusy`.
- **`MeditationMissionBase.cs`** — `[RequireComponent(MobMission)]`. Lógica común de toda misión mob:
  spawnea N mobs de a uno en anillo alrededor del jugador, los cuenta al disolverse, aplica
  `MeditationReward` (maestría de asana + Observación + monedas) y llama `EndMission()` al llegar a
  `targetCount`. Las subclases solo deciden **qué arquetipo** crear (`CreateMob`). Se autocablea vía
  `MobMission.OnBegan/OnEnded`.
  - **Subclases de misión**: `HealingMission`, `ProtectionMission`, `ChannelMission`,
    `AsanaFormationMission`, `PostureVisualizationMission`, `RootInquiryMission`.
  - **Arquetipos de mob** (`MeditationMob` base): `EphemeralThoughtMob`, `PostureFormMob`,
    `ChannelMob`, `AbsorbentThoughtMob`, `HealMob`, `ProtectMob`, `WardAttackerMob`.
- **`RealityShiftController.cs`** — **generaliza la antigua `KitchenScaleController`** (borrada 2026-07-23): la miniaturización ahora
  es genérica por área. Escala el `environmentRoot` hacia arriba (no encoge al jugador, para no romper
  el `CharacterController`) y ensancha el FOV. La transición es un **snap** (sin lerp/shader) porque
  ocurre siempre tras el fade a negro. Uno por área, asignado a su `VirtualizationMachine`.
- **`LotusMeditationAbility.cs`** — camino alternativo (endgame) al Microcosmos: tras aprender el loto
  y el "hechizo de misiones-mob" (`hasMobMissionSpell`), el jugador medita en cualquier sitio con una
  tecla y corre el **mismo** flujo que la máquina.
- Apoyo: `MobMission` (descriptor), `MissionSelectMenu`, `MeditationReward`, `ScreenFader`.

---

## Mundo mob (`Assets/Scripts/MobWorld/`, 5 archivos)

El santuario fractal por escala: cada mundo mob es su propia escena aditiva. Detalle en
[`mob-world-architecture.md`](mob-world-architecture.md).

- **`MobWorldLoader.cs`** — singleton `DontDestroyOnLoad`. Carga/descarga **aditiva** de la escena del
  mundo mob **tras el fundido a negro** (`ScreenFader`), teletransporta al jugador a su `MobSpawnPoint`
  (guardando la transform de retorno) y lo devuelve al salir. La escena mob se autora en un origen
  lejano para no solaparse con el mundo base, que sigue cargado y simulando. La miniaturización pasa a
  ser narrativa. Expone `IsInMobWorld`/`IsBusy`.
- **`MobWorldDirector.cs`** — singleton (auto-bootstrap), contraparte por **eventos** del
  `SanctuaryDirector` humano. `TriggerEvent(MobWorldEventType)` (`Migration`/`Invasion`/`Festival`)
  mueve/sube de nivel a los `MobResident` registrados y notifica `OnEvent` para que otros sistemas
  reordenen contenido. `autoEvents` dispara eventos aleatorios por timer para prototipado.
- **`MobResident.cs`** — NPC ligero (`IInteractable`), contraparte barata de `WorldCharacter`. Estático
  en su puesto; solo se mueve/sube de nivel cuando un evento lo dispara (`MoveTo`/`ReturnHome`/`LevelUp`).
  Ofrece un rol/servicio estilo tienda WoW. Se auto-registra en `MobWorldDirector`.
- **`YogaPortal.cs`** — `IInteractable` de **salida** del mundo mob (la entrada es por la máquina/loto).
  Sale de la escena mob vía `MobWorldLoader.ExitMobWorld()` si la hay; si no, del modo misión in-place
  vía `MeditationSession.EndMission()`.
- **`MobSpawnPoint.cs`** — marcador del punto donde `MobWorldLoader` teletransporta al jugador al entrar.
  Uno por escena mob (junto a la entrada/yoga-portal).
- Builder de escenas: `Assets/Editor/MobWorldSceneBuilder.cs` (genera la escena de una civilización por
  código, versionable).

---

## Avatares (Microcosmos) (`Assets/Scripts/Avatar/`, 3 archivos)

Los avatares-robot que el jugador pilota dentro del Microcosmos (Eje A: planos suelo/pared/techo/aire).
Detalle en [`magic-plane-and-meditation.md`](magic-plane-and-meditation.md) §4.

- **`SurfaceWalker.cs`** — locomoción kinemática por superficie: la gravedad es **relativa a la normal
  de la superficie**, así que suelo, pared y techo comparten código (el "arriba" del cuerpo se realinea
  a la superficie a la que se pega). Al quedarse **sin superficie cae** hacia abajo-mundo. Modos según
  el avatar activo: solo-suelo (gusano), trepar (araña, `canClimb`) y vuelo (mosco, `flight`, 3D libre).
- **`RobotAvatar.cs`** — `[Serializable]`; un avatar configurable en el Inspector. Enum
  **`AvatarLocomotion`** (`Ground` gusano / `Climb` araña / `Flight` mosco), `moveSpeed`, `turnSpeed`,
  `bodyScale` y flag `unlocked` (desbloqueo por progresión).
- **`AvatarController.cs`** — `[RequireComponent(SurfaceWalker)]`. Gestiona qué avatar se pilota y
  aplica su config al `SurfaceWalker` (canClimb/flight/escala). Ciclado con tecla (G por defecto),
  gated: dentro de la simulación siempre entre los desbloqueados; fuera, solo con `canSwitchOutsideSim`.
