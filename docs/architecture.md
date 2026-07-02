# Arquitectura — Cold Sanctuary

Detalle por sistema. Para el panorama general ver [`../CLAUDE.md`](../CLAUDE.md).

## Jerarquía de clases

```
MonoBehaviour
├─ Animal (abstract, implements IAnimal, IFactory)
│  ├─ Carnivore (abstract)
│  │  └─ WolfBehavior
│  └─ Herbivore (abstract)
│     └─ BunnyBehavior
├─ BearBehaviour (implements IAnimal por separado — NO hereda de Animal)
├─ BirdBehavior (implements IFactory)
├─ PlayerCtrl, ShipCtrl
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

### Interfaces actuales

```
IAnimal
  ├─ animationsName: AnimationsName { get; }
  ├─ aware: bool { get; set; }
  ├─ Hurt(float damage): void
  └─ Escape(bool team, List<GameObject> enemies): IEnumerator

IFactory
  └─ GenerateSquareRange(GameObject animal, GameObject area, int quantity): GameObject[]

IBondable  (Assets/Scripts/Companion/IBondable.cs)
  ├─ BondWithPlayer: float { get; }
  ├─ GrowBondWithPlayer(float amount): void
  └─ GetProximityEffect(StatChannel channel): float
```

> **Propuesta pendiente — IBody / IMind:**
> `IAnimal` se renombraría a `IBody` (cuerpo físico, aplicable también al jugador y NPCs).
> Se añadiría `IMind` para las stats mentales/emocionales actualmente en `PlayerStats`.
> `Restore()` enrutaría a `IBody`, `IMind` o `IBondable` según el tipo de efecto.
> Ver DEVLOG.md §Propuesta de arquitectura — IBody / IMind / IBondable.

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

- **`PlayerCtrl.cs`** — FPS: WASD, apuntado con ratón (IK de rifle/brazo/mano), mouse3 para
  fijar objetivo (toggle conejo/oso con 'G'), click izq. dispara, 'F' zoom.
- **`ShipCtrl.cs`** — helicóptero: WASD pitch/roll/yaw/velocidad, mouse0 free-look, scroll
  zoom, 'F' anima disparo. Fuerza ascendente constante.

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
