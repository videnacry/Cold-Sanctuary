# Cold Sanctuary — Dev Log & Contexto de Diseño

> **Áreas de trabajo paralelas e independientes:**
> - **Fauna** — comportamiento animal, ciclos post-natales, vínculo jugador↔cría.
>   Diseño técnico: [`docs/behavior-system.md`](docs/behavior-system.md) · Gameplay: [`docs/fauna-gameplay.md`](docs/fauna-gameplay.md)
> - **Jugador** — stats, asanas, ejercicios, compañeros, narrativa. Este archivo.
> - **Mundo / Simulación** — áreas, tareas autónomas, promociones, Director. [`docs/world-simulation.md`](docs/world-simulation.md)
> - **Jugabilidad y loops** — mecánicas de combate, tabla periódica como Pokédex, FOMO. [`docs/gameplay-loops.md`](docs/gameplay-loops.md)
> - **Pipeline 3D** — conversión/decimación de modelos, bugs conocidos de Blender. [`docs/pipeline-3d-models.md`](docs/pipeline-3d-models.md)
> - **Pájaros** — estado actual, por qué son una dependencia crítica de `LifeStage.Wander()`, y por qué son candidatos a reemplazo. [`docs/bird-logic.md`](docs/bird-logic.md)
> - **UI** — menú holográfico (Sistema/Hechizos/Yoga), arquitectura `FollowingArrays`. [`docs/ui-holographic-menu.md`](docs/ui-holographic-menu.md) · Base técnica: [`docs/ui-following-arrays.md`](docs/ui-following-arrays.md)

## Estado actual

### Tamaños de animales realistas ✅ (nuevo)
- [x] Los prefabs de animales se importaban con la escala cruda del FBX de origen
  (cada asset con su propia convención de unidades — el Bunny, por ejemplo, venía
  ~100x más grande de lo esperado). `AnimalPrefabGenerator.RealisticScaleFactor`
  (`Assets/Editor/AnimalPrefabGenerator.cs`) ahora define un factor de escala por
  especie calculado con `Measure Raw Animal Sizes (diagnostic)` contra un tamaño
  real objetivo (altura de hombro para cuadrúpedos parados, longitud corporal para
  foca/ballena). `Tools > Cold Sanctuary > Fix Animal Colliders And Rigidbodies`
  aplica el factor tanto al `Transform` del prefab como al campo `body` serializado
  (para que `LifeStage.SetScale()` en Play use el tamaño correcto también).
- ⚠️ Nota técnica: se intentó que `Physiognomy.baseScale` (el campo `defaultBody` de
  cada `*Behavior.cs`) fuera la única fuente de verdad, pero el valor estático
  compilado no reflejaba ediciones de código fuente de forma confiable en este
  entorno (persistía el valor viejo incluso tras limpiar `Library/ScriptAssemblies`
  y `Library/Bee` y reiniciar el Editor por completo — causa no identificada, podría
  ser un problema de caché de Unity ajeno al proyecto). Por eso la tabla de factores
  vive directamente en `AnimalPrefabGenerator.cs` (un script de Editor, que sí
  recompila de forma confiable) en vez de en los `*Behavior.cs`. Si en el futuro se
  edita un `defaultBody` y no parece surtir efecto, este es el motivo — no asumir
  que el bug está en el código.
- [ ] `PolarBear` y `Seal` no tienen factor todavía — sus carpetas `Models/` están
  vacías (sin FBX), pendiente de arte.
- **Generador de familias**: `Assets/Scripts/FamilyGenerator.cs` + `Assets/Scripts/Family.cs`
  ahora sí están enganchados a `SampleSceneBuilder` — ver "Simulación de fauna (biosistema)"
  más abajo.
- [x] **Kushal (jugador) también estaba mal escalado** — medía 3.38m de alto (el FBX
  tenía el mismo problema de unidad de importación que los animales) en vez de los
  ~1.75m de una persona adulta. `SampleSceneBuilder.ApplyRealisticPlayerScale()`
  mide el tamaño crudo y ajusta a `TargetPlayerHeightMeters = 1.75f`, de forma
  idempotente (se puede re-correr el blockout sin re-escalar en cascada). Confirmado
  con `Tools > Cold Sanctuary > Measure Selected Object Size` (nuevo, en
  `Assets/Editor/SceneDiagnostics.cs` — mide cualquier GameObject seleccionado, útil
  para futuros chequeos de escala).
- [x] **Teclas de cámara J/L/I/K reafirmadas en `SampleSceneBuilder`** — el mismo
  problema que `Physiognomy.baseScale`: `PlayerController` ya estaba agregado como
  componente en la escena desde antes de que el default de esas 4 teclas pasara de
  flechas a J/L/I/K, y un campo ya serializado no se actualiza solo cuando cambia el
  default en el código fuente. `EnsurePlayerSystems()` ahora reafirma
  `lookLeftKey/lookRightKey/lookUpKey/lookDownKey` a mano en cada corrida del
  blockout. **Patrón a tener en cuenta**: cualquier campo público con valor default
  en un `MonoBehaviour` que ya esté como componente en la escena puede quedar
  desincronizado así — si se cambia un default y "no hace nada", revisar el YAML de
  la escena (`grep <nombreCampo> Assets/Scenes/SampleScene.unity`) antes de asumir
  que el código está mal.
- **Unity MCP Server (nativo de Unity)**: Unity 6.5 trae un servidor MCP nativo (Project
  Settings > AI > Unity MCP Server) que permitiría controlar el Editor sin depender de
  screenshots/clicks. Investigado pero parece requerir una suscripción de Unity AI
  activa — la pantalla muestra "Up to 0 direct connections allowed at a time",
  consistente con los errores `NoSubscription` / `generators.ai.unity.com` que
  aparecen en consola durante toda la sesión. No se pudo habilitar sin esa suscripción.
- **MCP for Unity (Coplay, de terceros) ✅ instalado, servidor corriendo** — alternativa
  gratuita y de código abierto (`https://github.com/CoplayDev/unity-mcp`, MIT). Instalado vía
  Package Manager (git URL en `Packages/manifest.json`, solo en el proyecto vivo — no
  versionado, mismo criterio que el resto de `Packages/`). Requiere `uv` (gestor de paquetes
  Python) en el PATH del sistema — se instaló con el script oficial de `astral.sh` y se
  reinició el Editor para que recogiera el PATH nuevo. El servidor local HTTP ya está
  arrancado y con sesión activa (`Window > MCP for Unity`, puerto 8080).
  - **Falta un paso manual — vincularlo a Claude Code.** "Configure All Detected Clients" no
    detecta esta instancia de Claude Code (corre vía la app de escritorio, no hay un binario
    `claude` en el PATH de este entorno — confirmado con `which claude`). El comando que hay
    que correr a mano, desde donde SÍ exista el CLI de Claude Code:
    `claude mcp add --scope local --transport http UnityMCP http://127.0.0.1:8080/mcp`.
    Después hace falta reiniciar la sesión de Claude Code (los MCP no se cargan a mitad de
    sesión). Mientras tanto, el servidor sigue disponible para cualquier otro cliente MCP
    que sí se detecte automáticamente.

### Simulación de fauna (biosistema) ✅ (nuevo)
- [x] `FamilyGenerator.cs` reescrito: campo `animalPrefab` (antes `gameObjectMale`, con un
  `gameObjectFemale` muerto que nunca se usaba), nuevo `radius` por familia. `Family.cs` y
  `Animal.RenderFamily()` ahora aceptan y propagan ese `radius` hasta `Family.RenderGroup()`
  (que ya lo soportaba mas nadie se lo pasaba).
- [x] `SampleSceneBuilder.BuildWildlifePopulation()` (nuevo) crea un `WildlifePopulation_AUTO`
  con un `FamilyGenerator` y 9 familias siguiendo una proporción ~70% herbívoros / 30%
  carnívoros (elegida explícitamente para simular un biosistema equilibrado):
  4 familias de Bunny + 2 de Deer (herbívoros terrestres) vs 1 de Wolf + 1 de Fox
  (carnívoros), más 1 familia de Whale sobre `Sea_Placeholder` (marino, aparte). La familia
  de Seal se omite — no existe `Seal.prefab` (carpeta `Animals/Seal/Models/` vacía, sin FBX
  importado todavía), y `BuildWildlifePopulation` lo detecta y loguea un warning en vez de
  fallar (`[SampleSceneBuilder] WildlifePopulation: no se encontro el prefab de Seal, se omite
  esa familia`).
- **Importante — solo se puebla en Play, no en Editor**: a diferencia del resto del blockout,
  `FamilyGenerator.Start()` corre en Play mode. La escena se ve vacía de fauna hasta que se
  presiona Play; no es un bug. Confirmado con el log
  `[FamilyGenerator] 9 familia(s) generadas — 55 animales en total.`
- [x] **Bug encontrado y arreglado al probar esto**: con más animales corriendo a la vez,
  salió a la luz que la escena no tenía NavMesh horneado — cada `NavMeshAgent.SetDestination()`
  fallaba en silencio y saturaba la consola (600+ errores en segundos, creciendo sin parar
  mientras se mantenía Play). Ver sección NavMesh abajo.

### Pipeline 3D — cocina decimada, Maestra convertida ✅ (nuevo)
- [x] `Kitchen.fbx` (8.98M polys, export de 3ds Max) decimado a ~490K vértices vía Blender en
  modo **headless** (`blender --background --python`) — la GUI se colgaba de forma consistente
  con esta densidad de malla
- [x] Encontrados y documentados 2 bugs de Blender 5.1.2 (import FBX crashea con luces;
  export FBX cuelga con muchos material slots) + workarounds. Ver
  [`docs/pipeline-3d-models.md`](docs/pipeline-3d-models.md#decimar-modelos-pesados-en-blender-512--usar-modo-headless-no-la-gui)
- [x] Cocina colocada en `SampleScene` (reemplaza el placeholder), exportada como `.obj` en vez
  de `.fbx` (el export FBX no terminaba; OBJ tardó 2.5s)
- [x] Maestra (`Magnate5.blend` → FBX) convertida y colocada, con `WorldCharacter` conectado a
  `SanctuaryDirector.magnateCharacter`

### Simulación del Mundo ✅ (nuevo)
- [x] `SanctuaryAreaType` — 17 zonas en 5 tiers (+4: `Infirmary`, `VeterinaryClinic`,
  `YogaRoom`, `TextileStudio` — agregadas junto con el rebuild de estructuras, ver abajo)
- [x] `AreaTask` — tareas serializables con efectos de stats + tabla periódica
- [x] `SanctuaryArea` — zona física con requisitos de entrada, tareas y residentes
- [x] `WorldCharacter` — entidad autónoma (jugador o NPC), loop de tareas, eventos de promoción
- [x] `SanctuaryDirector` — singleton de la Magnate: intercepta llegadas, evalúa, promueve individualmente o en grupo
- Ver diseño completo: [`docs/world-simulation.md`](docs/world-simulation.md)

### Combate / Jugabilidad ✅ (nuevo)
- [x] `PeriodicTableManager` — singleton. 118 elementos como Pokédex. `Discover(symbol)`, grupos, `OnElementDiscovered`
- [x] `ElementFragment` — pickup drop de mob procesado. Trigger → intento de descubrimiento
- [x] `IngredientMob` — mob de ingrediente. Estados Idle/Aggro/Attack/Processed. Reproduce (levadura). Drops fragmentos
- [x] `KitchenScaleController` — miniaturización al entrar a la cocina. Escala el root de la cocina ×8, ajusta FOV, activa mobs
- [x] `PlayerCombat` — SphereCast de ataque, daño escalado por Strength, cooldown, armor/tool slots
- [x] `NPCCombatBehavior` — compañeros pelean autónomamente en la misma sala, daño basado en su Strength
- [x] `SanctuaryMission` (ScriptableObject) — misiones tipo IngredientCollection, YeastControl, AreaClear
- [x] `MissionTracker` — singleton. Gestiona progreso, recompensas en coins + items, fallo en YeastControl
- [x] `ItemData` (ScriptableObject) — Tool, Armor, Consumable, Fragment con stats de modificación
- [x] `CoinWallet` — balance de monedas, Earn/Spend con eventos
- [x] `Inventory` — items por cantidad, Equip (→ PlayerCombat), Sell (→ CoinWallet), UseConsumable
- TODOs pendientes: Animator triggers, VFX de golpe/recogida, UI de inventario/vendor
- Ver diseño completo: [`docs/gameplay-loops.md`](docs/gameplay-loops.md)

### Controlador del jugador ✅ (nuevo)
- [x] `PlayerController.cs` — reemplaza `PlayerCtrl.cs` (obsoleto). `CharacterController` con WASD, sprint (LeftShift), salto (Space), mouse look + flechas de teclado, toggle 3ª/1ª persona (V), preferencia guardada en `PlayerPrefs`, cursor auto-gestionado durante diálogos.
- [x] `AutoCameraZone.cs` — volumen trigger que anula la preferencia de cámara del jugador al entrar y la restaura al salir. `lockForDuration` usa el sistema de "robo" de `CameraManager`.
- **Filosofía de input — mecanografía:** Cold Sanctuary diseña los controles para fomentar la postura correcta de teclado (home row). Todos los atajos son configurables en el Inspector.

| Mano | Teclas default | Función |
|------|---------------|---------|
| Izquierda | W A S D | Movimiento |
| Izquierda | Shift izq | Sprint |
| Izquierda | Space | Salto / avanzar diálogo |
| Derecha | J / L | Cámara izq/der |
| Derecha | I / K | Cámara arriba/abajo |
| Derecha | 1 – 0 | Habilidades de combate |
| Derecha | Tab | Cycle target (mob siguiente) |
| Derecha | Enter | Avanzar diálogo (alternativo) |
| Cualquiera | V | Toggle 3ª ↔ 1ª persona |
| Cualquiera | Escape | Deseleccionar target / desbloquear cursor |

### Fixes — animales cayendo, sin controles, error de consola ✅ (nuevo)
- [x] `LifeStage.Wander()` tiraba `ArgumentOutOfRangeException` en cada tick: indexaba
  `BirdBehavior.population` (vacía, no había pájaros) sin chequeo, y con un off-by-one en
  `Random.Range`. Guard clause + fix. Ver [`docs/bird-logic.md`](docs/bird-logic.md) para
  por qué el wander de animales terrestres depende de pájaros en primer lugar.
- [x] Animales atravesaban el piso: `AnimalPrefabGenerator.GenerateOne()` generaba
  Rigidbody con gravedad + BoxCollider `size(1,1,1)` sin ajustar al mesh. Ahora
  `ApplyPhysicsDefaults()` (Rigidbody kinemático + collider ajustado a los Renderers) se
  aplica tanto al generar prefabs nuevos como al arreglar los existentes (`Tools > Cold
  Sanctuary > Fix Animal Colliders And Rigidbodies`) — un mismo bug ya no puede reaparecer
  en una especie nueva.
- [x] `Player` no tenía mesh visual — solo `CharacterController` + scripts. `Kushal.fbx`
  (importado en el pipeline 3D) estaba sin usar. `SampleSceneBuilder.EnsurePlayerVisual()`
  lo instancia como hijo de `Player`.
- [x] Agregados 6 pájaros placeholder (`SampleSceneBuilder.BuildBirds()`) para que el
  wander de animales tenga objetivos reales — son esferas sin modelo, no arte final.
- [x] `Active Input Handling` en Project Settings estaba en "Input System (New)" solamente,
  rompiendo todo el `UnityEngine.Input` legacy del proyecto. Cambiado a "Both".

### NavMesh ✅ (nuevo)
- [x] `IngredientMob` — ahora usa `NavMeshAgent` real. `EnterAggro` hace `SetDestination`. Destino se actualiza cada 0.25 s mientras persigue. `isStopped = true` al atacar o ser procesado.
- [x] `NPCCombatBehavior` — usa `NavMeshAgent` opcional. Throttle de destino cada 0.3 s. `FaceTarget` solo actúa si el agente está parado. Fallback gracioso si el NPC no tiene agente.
- [x] **`Ground_Placeholder` ahora hornea su propio NavMesh** — `SampleSceneBuilder.BuildGround()`
  le agrega un `NavMeshSurface` (paquete `com.unity.ai.navigation`, ya estaba instalado) con
  `collectObjects = All`, y `Build()` llama a `BakeNavMesh()` al final (después de colocar
  estructuras/animales) para hornear con todos los obstáculos ya en la escena. Antes de este
  fix no había NavMesh horneado en absoluto en `SampleScene` — pasaba desapercibido con pocos
  animales, pero con la población de fauna nueva (55 animales) cada `NavMeshAgent` fallaba
  `SetDestination` en cada intento y saturaba la consola. Re-correr
  `Tools > Cold Sanctuary > Build Sample Scene Blockout` rehornea automáticamente.
- **Setup en Unity**: bake NavMesh en la escena de la cocina (miniaturizada) por separado —
  esa escena no pasa por `SampleSceneBuilder`. Añadir `NavMeshAgent` a los prefabs de
  `IngredientMob` y a los NPCs con `NPCCombatBehavior`.

### Diálogo ✅ (nuevo)
- Historia lineal — sin opciones de respuesta. La historia rodea a Kushal; los personajes son épicos, especialmente la Magnate.
- [x] `DialogueLine` — estructura de una línea: `speakerName`, `portrait`, `text`, `typeSpeed`, `pauseAfter`, `screenEffect` (Flash / Shake / Darken), `side` (Left/Right)
- [x] `DialogueSequence` (ScriptableObject) — array de `DialogueLine`, flag `playOnce`, id único. Crear en: `Create → Cold Sanctuary → Dialogue → Sequence`
- [x] `DialogueManager` — singleton. `Play(sequence)` → reproduce líneas en orden. El jugador avanza con Space/Enter/Click. El manager aplica efectos de pantalla (flash blanco, shake de cámara, oscurecer). `IsPlaying` para bloquear doble-disparo.
- [x] `DialoguePanel` — UI MonoBehaviour. Typewriter con `maxVisibleCharacters`. `CompleteTyping()` salta la animación. Fade in/out del `CanvasGroup`. Soporta retrato (izquierda o derecha), overlay de flash y overlay de oscurecer.
- [x] `DialogueTrigger` — `RequireComponent(Collider)`. Al entrar el jugador dispara la secuencia. Ideal para puertas, la cámara de evaluación de la Magnate, descubrimientos ambientales.
- [x] `Goluis` — conectado: `greetingSequence` y `pressureSequence` en el Inspector. `OnPlayerNearby()` los dispara según estado.
- **Setup en Unity**: añadir `DialogueManager` a un GameObject persistente (junto con `PeriodicTableManager`). Crear `DialoguePanel` en el Canvas del juego. Asignar referencias en el Inspector.

### UI / Interfaces
- [x] **Radar de animales** (`AnimalRadar.cs`, `FollowingArrays.cs`)
  - Botones flotantes frente al jugador, uno por animal cercano
  - Se activa con tecla Z, se cierra con Escape
  - Diseñado para Desktop y Mobile: cada ítem es clickeable Y tiene tecla asociada
  - Las teclas se eligen para alinear al usuario al uso del teclado de escritura (sin flechas)
  - Actualmente representado como "encantamiento" — la presentación visual cambiará
- [x] **Menú holográfico (Menú → Magia / Yoga / Sistema) ✅ (nuevo, rediseñado)** — un
  holograma pequeño y plano anclado a la esquina inferior derecha de la pantalla (no un
  objeto 3D en el mundo — ver nota histórica abajo) abre Magia (tabla periódica, solo
  elementos descubiertos, + esquina "Hechizos" para lanzar combos completos desbloqueados),
  Yoga (las 8 posturas corporales vía `AsanaDetector`, teclas Q/E/R/T/Y/U/O/P, + esquina
  "Asanas" para posturas completas) y Sistema (pausa real + cámara 1ª/3ª + volumen). Sistema
  nuevo y propio (`Hologram`/`HologramPool`/`HologramMenuController`) con un pool fijo de
  118 tarjetas reutilizables (tamaño de la tabla periódica completa — el listado más grande
  que el menú necesita mostrar a la vez), en vez de instanciar/destruir objetos por panel.
  Probado en Play mode con clics reales: navegación completa Menú→Magia→Hechizos→volver→
  cerrar confirmada, pausa real confirmada, 0 errores de consola. Yoga y el slider de volumen
  no se llegaron a probar de forma interactiva (mismo camino de código que lo ya probado, sin
  motivo para sospechar que fallen). Diseño completo, incluidas las preguntas abiertas
  (accesos directos de asana/hechizo, iconos vs. texto, geometría exacta de tabla periódica):
  [`docs/ui-holographic-menu.md`](docs/ui-holographic-menu.md).
  - **Nota histórica**: la primera versión (misma sesión, unas horas antes) usaba cajas 3D
    flotando en el mundo delante del jugador vía `FollowingArrays`/`UI`. Se descartó por
    completo tras feedback directo — no podía anclarse a una esquina, ocupaba demasiada
    pantalla, y el cursor capturado todo el tiempo (ver abajo) impedía clicarla. `FollowingArrays`
    sigue intacto, usado solo por el radar de animales (su caso de uso original) — el fix de
    instancia-compartida y el `virtual` en `HideArrays()` de esa ronda siguen vigentes ahí.
  - **Cursor libre + cámara con clic derecho sostenido ✅**: `PlayerController` capturaba el
    cursor todo el tiempo desde `Start()`, haciendo imposible clicar cualquier UI. Ahora el
    cursor arranca libre; mover la cámara con el ratón requiere mantener presionado el botón
    **derecho** (`cameraLookButton`, configurable). El izquierdo queda libre para la UI y para
    `PlayerCombat.attackKey` (ya estaba en `Mouse0` — por eso la cámara tuvo que ir al derecho,
    no había otra opción sin romper el combate existente).
  - **FOV separado 1ª/3ª persona ✅**: `CameraManager` tenía un solo `baseFOV` (60°) para ambos
    modos. En primera persona, con Kushal ya a 1.75m reales, se sentía anormalmente cerrado
    ("todo se ve pequeño"). Ahora `firstPersonFOV = 82°` (típico de shooters) vs.
    `thirdPersonFOV = 60°`, aplicado al instante al cambiar de modo.

### Estructuras de áreas (rebuild) + Sistema de nado ✅ (nuevo)
- [x] **Modelos 3D feos reemplazados por estructuras primitivas** — a pedido explícito, las
  7 áreas de juego (Cocina, Enfermería, Veterinaria, Sala de Yoga, Mecánica, Estudio Textil,
  Huerto) ya no cargan `.fbx`/`.obj` (esos assets se dejan en `Assets/Environment/Buildings/`
  por si se reusan a mano más adelante, pero `SampleSceneBuilder` ya no los instancia). Cada
  área ahora es una caja: una "esterilla" (mat) horizontal fina de piso + paredes en forma de
  U (3 lados, entrada abierta hacia +Z) hechas con cajas verticales estiradas, más una
  etiqueta de texto flotante con el nombre. `BuildAreaStructures()` +
  `BuildAreaBox()`/`CreateWallSegment()`/`CreateAreaLabel()` en `Assets/Editor/SampleSceneBuilder.cs`.
  Cada área tiene color distinto y una posición fija en el mundo (7 áreas repartidas en tiers 1 y 2).
- [x] **Conectadas al `SanctuaryDirector` (antes sin usar)** — `SanctuaryArea`/`SanctuaryAreaType`/
  `SanctuaryDirector` ya existían del trabajo de "Simulación del Mundo" pero `allAreas[]` nunca
  se llenaba (no hay auto-discovery, hay que asignarlo a mano). Cada `BuildAreaBox()` agrega y
  configura un `SanctuaryArea` real (`areaType`, `displayName`, `progressionTier`), y
  `WireSanctuaryDirector()` los asigna todos a `director.allAreas` al final del blockout — la
  simulación de mundo pasa de inerte a funcional como efecto secundario natural de este rebuild.
- [x] **Sistema de nado** — `WaterZone.cs` (nuevo, `RequireComponent(Collider)`) en un trigger
  `BoxCollider` nuevo (`Sea_SwimZone`, hermano de `Sea_Placeholder` — el `Plane` del mar es una
  superficie 2D, no sirve como volumen; se le quitó su collider y se agregó esta caja aparte
  cubriendo el área visual del mar con margen). Al entrar, llama
  `PlayerController.SetSwimming(true)` y tiñe `RenderSettings.fogColor` de azul (guarda/restaura
  el color original al salir). `PlayerController.HandleMovement()` bifurca a
  `HandleSwimMovement()` mientras `_isSwimming`: movimiento libre 3D sin gravedad, mismas teclas
  de mano izquierda que en tierra (WASD horizontal, `jumpKey`=Space también sirve para subir,
  nueva `swimDownKey`=Ctrl izq para bajar) — nada nuevo que aprender al entrar al agua.
  - **Verificado en Play mode**: teletransportado el jugador dentro de `Sea_SwimZone`
    (`Position = 200, -5, 0`), la física se estabilizó en `Y = -7.08` y se mantuvo perfectamente
    quieta durante varios segundos (sin nado, la gravedad la habría seguido bajando — confirma
    que el modo nado se activó). Al presionar Space, `Y` subió a `-7.0045`, confirmando que el
    input vertical de nado también funciona.
- [x] **Bug encontrado al probar esto — el menú holográfico desapareció**: `BuildUI()` y
  `BuildHolographicMenu()` localizaban "el" Canvas de la escena con
  `Object.FindAnyObjectByType<Canvas>()`, asumiendo que solo existiría uno. Pero
  `CreateAreaLabel()` (parte de este mismo rebuild) le pone un `Canvas` world-space
  ("Label_Canvas") a cada una de las 7 áreas para el cartel flotante con el nombre — con 7
  canvases nuevos en la escena antes de que `BuildUI()` corriera, `FindAnyObjectByType`
  devolvía uno cualquiera de esos (en la práctica, el de `TextileStudio_Area`) en vez del
  `Canvas_AUTO` de pantalla completa. Efecto: `BuildUI()` creía que "ya había un Canvas" y
  nunca creaba `Canvas_AUTO`, y `BuildHolographicMenu()` colgaba el pool de 118 hologramas +
  el controller entero del cartel de Estudio Textil — world-space, lejos de cámara, en vez de
  screen-space en la esquina. El menú "Menú" seguía existiendo en la escena (confirmado con
  `Object.FindAnyObjectByType`/búsqueda por tipo en el Hierarchy) pero invisible en el Game
  view. **Fix**: ambos métodos ahora buscan por nombre exacto (`GameObject.Find("Canvas_AUTO")`)
  en vez de por tipo — inmune a cuántos Canvas world-space adicionales haya en la escena.
  Verificado: rebuild limpio, log confirma `"Canvas + EventSystem minimos creados"` (antes
  decía `"Ya existe un Canvas"`), y en Play mode el holograma "Menú" volvió a aparecer en la
  esquina y el flujo Menú→abrir→cerrar funciona.

### Menú holográfico jugable 100% por teclado ✅ (nuevo)
- [x] **Pedido explícito**: el juego debe poder jugarse solo con mouse O solo con teclado, no
  únicamente con mouse. Antes de esto, el menú holográfico (Menú/Magia/Yoga/Sistema) solo se
  podía abrir con clic — sin forma de navegarlo desde el teclado.
- [x] `HologramMenuController.menuToggleKey = KeyCode.M` (nuevo campo, configurable en el
  Inspector) — abre/cierra el menú raíz sin mouse. `Update()` nuevo: `M` alterna
  `OpenRoot()`/`CloseAll()`; `Escape` cierra si el menú está abierto (además de su uso ya
  existente en `PlayerController` para soltar el cursor).
- [x] **Navegación entre hologramas reutiliza el sistema de Unity, no uno propio**: cada
  `Hologram` ya era un `Button` de uGUI — `Navigation.Mode.Automatic` (el default de `Button`,
  nunca se había tocado) calcula vecinos arriba/abajo/izq/der solo con la posición en pantalla
  de cada tarjeta activa, así que las flechas ya funcionan sin cablear nada a mano. Lo que
  faltaba era darle un punto de partida al `EventSystem`: `FocusFirstActive()` (nuevo,
  `HologramMenuController.cs`) selecciona el primer holograma de cada lista recién construida
  (se llama al final de cada `Open*()`), y `Start()`/`CloseAll()` seleccionan el holograma
  "Menú" de la esquina — así las flechas y Enter/Espacio (acción "Submit" del Input Manager,
  ver `ProjectSettings/InputManager.asset`) funcionan desde el primer frame, sin necesitar un
  clic previo.
- [x] **Foco visible para navegación por teclado**: un `Button` creado por `AddComponent`
  nunca recibe `targetGraphic` (solo lo autoasigna el menú "Crear > UI > Button" del editor),
  así que el tinte de foco/hover no se veía. `HologramPool.CreateHologramInstance()` ahora
  asigna `button.targetGraphic = background` y un `ColorBlock` con `highlightedColor`/
  `selectedColor` bien por encima de blanco — el holograma con foco de teclado se ve
  claramente más brillante y opaco que el resto.
- **Verificado en Play mode**: `M` abre el menú raíz (Magia/Yoga/Sistema + X), `Enter` activa
  el holograma con foco (confirmado abriendo "Magia" → tabla periódica). `Escape`/clic en "X"
  no se pudieron re-confirmar en esta sesión por un problema de entorno (ver nota abajo), pero
  la lógica es simétrica a `M` (mismo `CloseAll()`) y ya estaba probada por clic antes de este
  cambio.
  - **Nota de entorno, no de código**: durante esta ronda de pruebas, el toggle de Play/Stop del
    Editor (vía clics de automatización o Ctrl+P) se volvió errático — cada intento de detener
    Play parecía alternar en vez de asentarse, y la Game view daba la impresión de quedarse
    "congelada" mostrando un estado viejo mientras la lógica de fondo seguía corriendo (los
    contadores de warnings de consola seguían subiendo). Coincide con el patrón ya documentado
    en este mismo archivo sobre clics intermitentes en el Editor vía la herramienta de
    automatización — no se identificó ningún error de compilación ni de consola asociado. Si
    esto se repite jugando a mano (sin automatización), sí sería motivo de investigación real.

### Lista raíz y Sistema centradas (a pedido) ✅ (nuevo)
- [x] El menú raíz (Magia/Yoga/Sistema) y la lista de Sistema (Cámara + volumen) usaban
  `AddVertical(menuCorner, ...)`, pegadas al mismo borde derecho que el lanzador "Menú" —
  correcto según el pedido original ("el holograma de menú debe estar en un rincón"), pero
  al usarlo se pidió que la lista en sí se viera más centrada. Nuevo helper
  `HologramMenuController.AddCenteredVertical()`/`CenteredVerticalPosition()`: agrupa los
  items verticalmente centrados en pantalla (no anclados a esquina). El lanzador "Menú" y los
  botones de cierre/atajo (X, Hechizos, Asanas, etc.) siguen en sus esquinas — eso no cambió.
  `AddVertical`/`VerticalOffset` (ya sin usuarios) se eliminaron.
- **Verificado en Play mode**: Magia/Yoga/Sistema aparecen agrupados en el centro de la
  pantalla al abrir "Menú"; la X de cierre sigue en su esquina.

### Sistema de forrajeo real para herbívoros terrestres + cardúmenes para marinos ✅ (nuevo)
- [x] **Pedido explícito**: los conejos/venados no buscaban pasto de verdad — `Herbivore.Feed()`
  era un timer abstracto sin ubicación. `GrassPatch.cs` (nuevo, `Assets/Scripts/World/`) —
  registro estático simple (`GrassPatch.All`) de áreas de pasto en la escena;
  `GrassPatch.Nearest(pos)` devuelve la más cercana. `Herbivore.Feed()` ahora camina
  (`nav.SetDestination`) hasta la más cercana antes de la lógica de alimentación de siempre.
  `SampleSceneBuilder.BuildGrassPatches()` coloca 3 cajas planas verdes (`GrassPatch_AUTO`,
  18×18) repartidas sobre el territorio donde ya viven las familias de Bunny/Deer.
- [x] **Pedido explícito — pescado para las ballenas**: `FishSchool.cs` (nuevo, mismo patrón
  que `GrassPatch`) — equivalente marino para Whale/Seal. `Herbivore.Feed()` elige la fuente
  según `GrazesOnLand`: `GrassPatch` en tierra, `FishSchool` en el mar, mismo bucle de "caminar
  hasta ahí" para ambos casos. `WhaleBehavior`/`SealBehavior` overridean `GrazesOnLand =>
  false` (son marinas — filtran/pescan, no pastan). `SampleSceneBuilder.BuildFishSchools()`
  coloca 2 cardúmenes (`FishSchool_AUTO`, cajas planas azules) cerca de donde viven esas
  familias.
- **Nota sobre las ballenas**: son belugas (`Delphinapterus leucas`, ya documentado en
  `WhaleBehavior.cs`), que comen pescado por filtrado, no krill — el krill es dieta de
  ballenas barbadas (azul, jorobada), una especie distinta a la que hay en el santuario.
- [x] **Bug encontrado y arreglado al probar esto**: `nav.SetDestination` tiraba
  `"'SetDestination' can only be called on an active agent that has been placed on a NavMesh"`
  a cada frame para Whale/Seal — nadan en mar abierto, donde no hay NavMesh horneado (solo el
  suelo lo tiene), así que su `NavMeshAgent` nunca queda realmente "on mesh". Fix: `Feed()`
  ahora chequea `nav.isOnNavMesh` antes de llamar `SetDestination` — si el agente no está en
  malla (marino o no), simplemente come sin caminar, en vez de tirar el error a cada frame.
- **Verificado en Play mode**: rebuild limpio, 0 errores de compilación, 0 errores de
  `NavMeshAgent` tras el fix (confirmado buscando "NavMesh" en la consola — sin resultados).
  Los únicos "errores" restantes en consola (`[RelayService] Bus validation failed` /
  `Unity.AI.Tracing`) son del paquete interno de Unity AI Assistant fallando al conectar con
  sus servicios cloud (licencia/red) — no tienen relación con el proyecto ni con este cambio.

### Oso Polar agregado como especie jugable/poblada ✅ (nuevo)
- [x] **Pedido explícito**: "¿los osos han sido agregados al juego?" → no lo estaban; el GLB del
  oso ya estaba en Descargas (`polar-bear.zip`, Sketchfab por kenchoo, CC-BY-4.0, uso comercial
  permitido con atribución) — exactamente el asset que `docs/pipeline-3d-models.md` ya marcaba
  como pendiente. Pipeline completo:
  1. Conversión GLB → FBX con el mismo script headless de Blender usado para Kitchen (sin
     decimar — 10.352 vértices, 1 armature, <1s).
  2. Import a `Assets/Animals/PolarBear/Models/PolarBear.fbx` (+ texturas) — solo en el proyecto
     vivo, no en el repo git (regla de siempre: binarios/modelos no se suben).
  3. Medición de tamaño crudo con Blender (headless, mismo patrón que el diagnóstico del
     proyecto): altura 4.870m, longitud 7.424m. `RealisticScaleFactor["PolarBear"] = 0.267f` en
     `AnimalPrefabGenerator.cs` (objetivo 1.3m de hombro → longitud resultante ~2.0m, consistente
     con un oso polar real).
  4. `Tools > Cold Sanctuary > Generate Animal Prefabs` → `Assets/Animals/PolarBear/PolarBear.prefab`
     (usa `BearBehaviour`, ya existía de antes — carpeta "Bear" pero especie registrada como
     "PolarBear").
  5. Familia agregada a `SampleSceneBuilder.BuildWildlifePopulation()`: posición `(-15, 0, -50)`,
     radio 22 — al margen costero del bioma terrestre (más cerca de x=0, rumbo a la zona marina),
     coherente con que su `Diet` prioriza Seal > Bunny > Wolf.
- [x] **Bug encontrado y arreglado al probar esto** (mismo patrón que el fix de forrajeo marino,
  pero en otro lado del código): con el oso poblado, `NavMeshAgent.SetDestination` volvió a tirar
  `"can only be called on an active agent that has been placed on a NavMesh"` a cada frame — esta
  vez desde `PostNatalManager.MotherDecisionTick/CubSoloCycle` (5 sitios) y desde
  `Animal.Flee/Fight/HitAndRun` + `Carnivore.Hunt` + `LifeStage.Wander/Homebound/Feed` (otros 9
  sitios) — un problema **compartido por todas las especies**, no exclusivo del oso, que
  simplemente no se había disparado antes. Fix: mismo guard `nav != null && nav.isOnNavMesh` antes
  de cada `SetDestination`, aplicado en los 14 sitios sin proteger (`PostNatalManager.cs`,
  `Animal.cs`, `Carnivore.cs`, `LifeStage.cs`).
- **Verificado en Play mode**: rebuild limpio, 0 errores de compilación. `[FamilyGenerator] 10
  familia(s) generadas — 58 animales en total` (incluye la familia de oso). 0 errores de
  `NavMeshAgent` sostenido durante >90s de Play tras el fix. Único error residual: el ya conocido
  `connection.state_change ... WebSocketException` del Unity AI Assistant intentando conectar a su
  servicio cloud — ajeno al proyecto.
- **Pendiente** (no pedido esta vez, solo documentado): Foca (Seal) sigue sin FBX — mismo GLB
  pendiente de convertir, fuente Sketchfab (rkuhlf) — ver `docs/pipeline-3d-models.md`.

### Bug: todos los animales corrían "de espaldas" (mesh mirando al lado contrario del movimiento) ✅
- [x] **Reportado por el usuario**: "fíjate las direcciones que están tomando los animales para
  moverse, pues parece que los venados corren de espaldas". Inspección de gizmos en Scene view
  (Lobo, Ciervo, Oso Polar) confirmó que el frente visual del modelo quedaba opuesto al eje +Z
  local del root — el eje que usa `NavMeshAgent.updateRotation` para orientar al animal según su
  velocidad de movimiento. Bug sistémico de todos los FBX importados (confirmado en assets tanto de
  Quaternius como de Sketchfab), no de una especie puntual.
- [x] **Fix**: nuevo `CorrectMeshFacing()` en `AnimalPrefabGenerator.cs` — rota 180° en Y los hijos
  directos del prefab (mesh/armature), nunca el root (rotar el root rompería la semántica de
  `updateRotation`, que reorienta el root hacia la dirección de movimiento cada frame). Asignación
  absoluta, no acumulativa, para que sea idempotente si se re-corre sobre un prefab ya corregido.
  Enganchado tanto en `GenerateOne()` (prefabs nuevos) como en `FixExistingPrefabs()` (prefabs ya
  creados).
- **Verificado**: `Tools > Cold Sanctuary > Fix Animal Colliders And Rigidbodies` aplicado a los 7
  prefabs existentes (Wolf, Deer, Fox, Malamute, Bunny, PolarBear, Whale) — los 7 muestran
  `m_LocalRotation.y: 1` en el YAML del prefab tras el fix.

### Bug crítico: `Escape()` podía apilar corrutinas sin límite y colgar la máquina — arreglado ✅
- [x] **Reportado por el usuario**: tras poblar la escena con el oso y probar en Play, la máquina
  completa (no solo Unity) se quedó bloqueada. Diagnóstico en frío (sin Unity abierto) sobre el
  código pulleado esta sesión (territorialidad/`SenseThreats`):
  - `Animal.SenseThreats()` corre en **cada tick de `Restore()`** (potencialmente muy frecuente) y
    solo se frena si `aware == true`: `if (busy || aware || asleep || rig == null) return;`.
  - `Animal.Escape()` tenía una rama de salida temprana (depredador detectado pero por debajo de
    un umbral de peligro *distinto* al de `SenseThreats`) que hacía `yield return
    new WaitForSeconds(...); yield break;` **sin haber marcado `aware = true` nunca** — solo la
    rama "de verdad peligroso" lo marcaba, más abajo en el método.
  - Resultado: con un depredador cerca que no superaba ese segundo umbral, cada tick de `Restore()`
    volvía a ver `aware == false`, y `SenseThreats()` lanzaba **otro** `StartCoroutine(Escape(...))`
    encima del anterior (que seguía vivo en su propio `WaitForSeconds`) — apilamiento sin límite,
    sin cancelar las corrutinas previas. Con la escena nueva (58 animales, varios depredadores cerca
    de presas por el amontonamiento ya conocido) esto escaló de "lento" a colgar el proceso y,
    finalmente, la máquina entera.
  - **Fix**: `aware = true` se marca **al entrar** a `Escape()` (antes de cualquier rama) dentro de
    un `try`, y se limpia en un `finally` — así queda marcado en *todas* las salidas (incluida la
    temprana) y `SenseThreats()` ya no puede relanzar una segunda corrutina mientras la primera
    sigue viva. Cambio en `Assets/Animals/Animal.cs` (método `Escape`), sincronizado al proyecto
    vivo.
  - **Segundo bug, más grave, encontrado al reverificar en Play mode**: con el fix de `Escape()`
    puesto, la memoria de Unity se mantuvo estable un minuto (buena señal), pero luego volvió a
    crecer y el proceso volvió a colgarse. El `Editor.log` mostraba miles de repeticiones de un
    stack trace `<Fight>...StartCoroutine(IEnumerator) -> <Escape>...MoveNext()` — el verdadero
    culpable era `Animal.Fight()`: al entrar, recluta aliados con
    `ally.StartCoroutine(ally.Fight(threat))` para **todo** `Group.members` dentro del `HomeRadius`,
    **sin comprobar si ese aliado ya estaba peleando**. Cada aliado reclutado volvía a reclutar al
    resto de la manada (incluido a quien lo reclutó), y como esto se repite en cada tick mientras el
    combate sigue activo, generaba una **explosión combinatoria** de `StartCoroutine` — con una
    manada de N lobos, el número de corrutinas activas crece sin límite. Con las dietas nuevas del
    pull (oso↔lobo depredación mutua en manada, lobo vs zorro/malamute) los combates en manada son
    mucho más frecuentes que antes, así que este bug —que probablemente ya existía— nunca se había
    disparado con fuerza hasta ahora.
  - **Fix del segundo bug**: nuevo campo `Animal.fighting` (paralelo a `aware`, no se reutiliza el
    mismo porque `Escape()` ya lo deja en `true` antes de invocar `Fight()`, así que reusar `aware`
    habría hecho que `Fight()` nunca arrancara). `Fight()` ahora: no reentra si `fighting` ya es
    `true`, lo marca `true` al entrar (con `try`/`finally` para garantizar que se limpia), y solo
    recluta aliados que **no** estén ya `fighting`. Mismo archivo, mismo patrón que `Escape()`.
  - **Verificado**: memoria de Unity estable tras el segundo fix (checkeado con PowerShell
    `Get-Process` en frío, no solo mirando la UI) — pendiente una sesión de Play mode más larga para
    confirmar del todo con manadas en combate activo.
  - **Nota para el futuro**: cualquier `SubEvent`/tick que dispare `StartCoroutine` de un
    comportamiento "largo" (huir, cazar, pelear, reclutar aliados) desde un bucle recurrente **o**
    desde otro comportamiento largo necesita un guard que se marque **incondicionalmente** al entrar
    (con `try`/`finally`) y que se **respete también al reclutar a otros** — no alcanza con
    protegerse a uno mismo si se puede arrancar la misma corrutina en otro objeto sin comprobar su
    estado. Van tres casos de esta familia de bug en la sesión (`NavMesh`, `Escape`, `Fight`).

### Nidos primero + hábitat de cobertura para la fauna terrestre ✅ (nuevo)
- [x] **Pedido explícito**: el usuario notó que las 8 familias terrestres aparecían todas
  amontonadas en una esquina del mapa (80×90 unidades de un suelo de 250×250), que faltaban
  árboles/arbustos para "esconder" un poco los nidos, y que los miembros de una misma familia no
  compartían un centro de nido real. Diseño completo en
  `C:\Users\Blein\.claude\plans\encapsulated-wondering-panda.md`.
- [x] **`SampleSceneBuilder.cs`** — `BuildWildlifePopulation()` reescrito: nuevo `struct NestSpec`
  (especie, posición, radio, `NestKind` presa/depredador, altura) y las 10 familias terrestres +
  marinas redistribuidas por las tres bandas norte/centro/sur del lado oeste del suelo (todo el
  espacio ya disponible, sin agrandar el terreno). `ValidateNestSeparation()` nueva: por cada nido
  de presa de rango pequeño (hoy solo Bunny, `homeRadius=20`), `Debug.LogWarning` si queda a menos
  de `homeRadius + 40` de un nido de depredador — solo aviso, sin reubicar automático.
  `BuildGrassPatches()` ahora itera los nidos de presa en tierra en vez de 3 posiciones
  hardcodeadas. Nuevo `BuildHabitatCover()` + `BuildTree()`/`BuildBush()`: 3–5 props primitivos
  (cilindro+cono para árbol, esfera achatada para arbusto) alrededor de cada nido de presa, sin
  collider (decorativo, mismo patrón que `GrassPatch`/`FishSchool` — no debe estorbar al
  `NavMeshAgent`).
- [x] **`FamilyGenerator.cs`** — fix real de `HomeOrigin` compartido, con una vuelta de tuerca no
  anticipada en el plan original: la primera versión (sobreescribir `member.HomeOrigin =
  family.position` en el mismo `Start()`, justo después de `RenderFamily()`) **no funcionaba**,
  porque cada especie fija `HomeOrigin = transform.position` desde su propio `Start()` (vía
  `Animal.Init()`), y Unity **difiere el `Start()` de los objetos recién instanciados** hasta
  después de que el `Start()` que los creó (`FamilyGenerator.Start()`) ya terminó — así que el
  `Init()` de cada animal corría *después* de nuestra asignación y la volvía a pisar con el punto
  de aparición individual. Confirmado con logs de diagnóstico: `HomeOrigin` valía `(0,0,0)` en el
  mismo `Start()` (Init() aún no había corrido) y ya tenía el punto individual una vez transcurrido
  un frame. **Fix definitivo**: `FamilyGenerator` junta todos los miembros pendientes en una lista
  y usa `StartCoroutine` con `yield return null` (espera un frame) antes de fijar el `HomeOrigin`
  compartido — para ese momento todos los `Start()` del frame (incluidos los de los animales recién
  instanciados) ya corrieron, así que la asignación de `FamilyGenerator` es la que gana. Verificado
  con logs temporales (retirados tras confirmar) y directamente en el Inspector: un Bunny cub en
  `Position (-27.17, 0, 101.22)` mostró `Home Origin (-35, 0, 100)` — la posición del nido de la
  familia, no la suya propia — y se mantuvo así al menos 3s después sin volver a cambiar.
- ⚠️ **Gotcha de entorno reencontrado** (ya documentado arriba para `Physiognomy.baseScale`, pasa
  de nuevo acá): tras sincronizar `FamilyGenerator.cs` al proyecto vivo con `cp` mientras Unity
  estaba en Play mode, el Editor **no recompiló** al parar Play — el código viejo siguió
  ejecutándose varias vueltas de Play/Stop seguidas (confirmado porque los `Debug.Log` nuevos nunca
  aparecían en la consola). Solo se resolvió forzando `Assets > Refresh` (Ctrl+R) manualmente desde
  el menú — mucho más liviano que el reinicio completo del Editor que hizo falta la vez anterior
  con `Physiognomy.baseScale`. Anotado para no repetir la confusión: si un cambio de script no
  parece surtir efecto pese a estar bien sincronizado en disco, probar `Assets > Refresh` antes de
  asumir que el código está mal.
- **Verificado en Play mode**: rebuild limpio, 0 errores. Familias visiblemente repartidas por las
  tres bandas en la Scene view (ya no amontonadas). Árboles/arbustos de `HabitatCover_AUTO`
  visibles alrededor de los nidos de presa. Monitoreo de memoria por PowerShell `Get-Process`
  durante ~140s de Play: ambos procesos de Unity estables/decrecientes (2389→2283MB y 940→1018MB),
  `Responding=True` sostenido. `Editor.log` sin **ninguna** ocurrencia de los stack traces de
  `Fight`/`Escape` del bug de corrutinas anterior — con más separación real entre nidos de
  depredador y presa, cero combates en esta sesión (antes había decenas incluso con el fix puesto).

### Foca (Seal) convertida e integrada — especie completa, ya poblada ✅ (nuevo)
- [x] **Pedido explícito**: "convierte la foca y revisa la migración completa, scripts, población,
  gameobject con dimensiones realistas relativas a los demás". El GLB (en rigor glTF+bin, mismo
  caso) ya estaba en Descargas (`seal.zip`, Sketchfab por rkuhlf, CC-BY-4.0, uso comercial permitido
  con atribución) — el asset que `docs/pipeline-3d-models.md` marcaba pendiente. Pipeline idéntico
  al Oso Polar:
  1. Conversión con el mismo script headless de Blender (`bpy.ops.import_scene.gltf()` +
     `bpy.ops.export_scene.fbx()`, sin decimar — 1.265 vértices, 2 mallas, 1 armature, <1s).
  2. Import a `Assets/Animals/Seal/Models/Seal.fbx` — solo en el proyecto vivo (regla de siempre:
     binarios/modelos no se suben a git). Advertencia menor de importación ("64 de 6969 vértices sin
     peso de hueso, asignados a bone #0") — cosmético, <1% de la malla, no bloqueante.
  3. Medición de tamaño crudo con Blender headless: X 6.359m, Y 10.329m, Z 4.075m. A diferencia de
     los cuadrúpedos parados (que se referencian por altura de hombro), la foca no se sostiene de
     pie — se referencia por longitud corporal como ya se hacía con Whale.
     `RealisticScaleFactor["Seal"] = 0.165f` en `AnimalPrefabGenerator.cs` (longitud cruda 10.329m
     → objetivo 1.7m, foca ártica genérica). `SealBehavior.cs` ya existía completo de una sesión
     anterior (post-natal, dieta, homeRadius=150) — no necesitó ningún cambio de código, solo le
     faltaba el modelo.
  4. `Tools > Cold Sanctuary > Generate Animal Prefabs` → `Assets/Animals/Seal/Seal.prefab`.
     `CorrectMeshFacing()` (fix de la sesión anterior) se aplicó automáticamente sin cambios — el
     mismo problema de orientación de mesh confirmado también acá.
  5. La familia de Seal ya estaba cableada en `SampleSceneBuilder.BuildWildlifePopulation()` desde
     la sesión anterior (`AddFamily("Seal", ...)` existía pero fallaba en silencio con "no se
     encontró el prefab" porque el prefab todavía no existía) — con el prefab ya generado, un
     rebuild del blockout la population automáticamente sin tocar más código.
- **Verificado — dimensiones relativas**: comparando el `BoxCollider` en espacio de mundo (tamaño
  local del collider × `localScale` del prefab) contra el resto del roster: Wolf 0.33×0.80×1.48m,
  Deer 0.56×1.20×1.19m, Fox 0.18×0.40×0.79m, PolarBear 0.82×1.59×2.27m, **Seal 1.31×0.82×1.82m**.
  La foca queda más larga que el lobo y más chica que el oso polar — proporción realista para el
  roster ártico (el ancho mayor a lo esperado viene de los flippers extendidos en la pose de rest
  del rig, no es un error de escala).
- **Verificado en Play mode**: rebuild limpio, 0 errores nuevos (el 1 error persistente es el
  artefacto de entorno ya documentado, presente desde antes de esta sesión). Confirmado en el
  Inspector del `FamilyGenerator` (`WildlifePopulation_AUTO`) que el Element 10 referencia
  correctamente el prefab de Seal.

### Nota de sesión — Play mode atascado alternando con la automatización (entorno, no código)
Durante buena parte de esta sesión, el toggle Play/Stop del Editor se volvió consistentemente
errático al operarlo con la herramienta de automatización (clic, Ctrl+P, Edit > Play Mode >
Play) — alternaba en vez de asentarse, y se vio de refilón una notificación de Windows robando
foco durante uno de los intentos, lo que encaja con el patrón ya documentado antes en este
archivo sobre clics intermitentes en el Editor. El usuario detuvo Play manualmente (un solo
clic, sin problema) y desde ahí se pudo rebuildear, verificar en Play mode y guardar con
normalidad — confirma que era un problema del entorno de automatización, no del Editor ni del
código.

---

## En progreso / Por hacer

### 1. Interfaz de Encantamientos (rediseño)
- [x] Layout resuelto — panel "Hechizos" del menú holográfico, un botón por elemento
  descubierto. Ver [`docs/ui-holographic-menu.md`](docs/ui-holographic-menu.md).
- [ ] El jugador selecciona elementos químicos de la tabla periódica por cada encantamiento
  (mecánica de encantamiento en sí — el panel es de solo lectura por ahora)
- [x] Un botón por elemento (similar a como ahora hay uno por animal)
- [ ] Botones más pequeños que los del radar, con distribución diferente para no cubrir toda la pantalla
- [ ] Mantener compatibilidad Desktop (tecla) + Mobile (click)

### 2. Interfaz de Posturas / Asanas

- [x] Layout resuelto — panel "Yoga" del menú holográfico, un botón por posición corporal
  (`BodyPositionButton`, ya existía pero sin ningún panel que lo mostrara). Ver
  [`docs/ui-holographic-menu.md`](docs/ui-holographic-menu.md).

**Progresión de desbloqueo:**
- [ ] El jugador empieza SIN ninguna asana desbloqueada
- [ ] Las asanas se desbloquean a través de un profesor o texto/escrito encontrado en el mundo
- [ ] Las primeras actividades disponibles son ejercicios básicos (ver sección Ejercicios Básicos)

**Asanas disponibles (primer set):**
| Nombre | Nombre sánscrito correcto | Beneficio esperado |
|---|---|---|
| Urdhva Hastasana | Urdhva Hastāsana | (por definir) |
| Janu Sirsasana | Jānu Śīrṣāsana | (por definir) |
| Paschimottanasana | Paścimottānāsana | (por definir) |
| Virabhadrasana I | Vīrabhadrāsana I | (por definir) |
| Virabhadrasana II | Vīrabhadrāsana II | (por definir) |
| Sarvangasana | Sarvāṅgāsana | (por definir) |
| Matsyasana | Matsyāsana | (por definir) |

**Mecánica:**
- [ ] Un botón por posición corporal — las posiciones son siempre las mismas (absolutas, no contextuales)
- [ ] El jugador selecciona todas las posiciones que forman la asana
- [ ] Si el match es perfecto → empieza a recibir el beneficio
- [ ] Los beneficios continúan hasta alcanzar el límite del contenedor
- [ ] Si la asana tiene dos lados → cambia de lado automáticamente y continúa recibiendo beneficios
- [ ] Se puede encolar la siguiente asana mientras se está en una activa → transición automática al terminar
- [ ] Mantener compatibilidad Desktop (tecla) + Mobile (click)

### 3. Ejercicios Básicos (disponibles desde el inicio, también en esterilla)

Los ejercicios básicos no son asanas pero usan el mismo sistema de selección de posiciones corporales. Su función es:
- Liberar tensión
- Ganar fuerza
- **Agrandar el límite de los contenedores de stats** (fuerza, enraizamiento, etc.)

**Ejercicios iniciales:**
- Flexiones (push-ups)
- Piernas
- Abdominales

**Ejemplo de posiciones para flexiones:**
- "Manos a altura de hombros"
- "Codos detrás"
- "Pies juntos, talón hacia arriba"

- [ ] Definir posiciones completas para cada ejercicio básico
- [ ] Definir qué contenedor agranda cada ejercicio y en cuánto

### Partes del cuerpo a posicionar (sistema de posiciones)

Cada postura/ejercicio define el estado de estas partes:
- Codos
- Manos
- Rodillas
- Pies
- Cadera
- Espalda
- Hombros
- Cabeza

*Nota: simplificar según feedback de jugabilidad — estas 8 partes cubren toda postura relevante.*

### Sistema de simplificación por práctica

- [ ] Tras ejecutar una postura **10 veces**, se desbloquea una **opción directa** a esa postura (sin seleccionar cada posición individualmente)
- [ ] Con la opción directa el personaje puede cometer errores en alguna posición
- [ ] El jugador solo selecciona los **arreglos** necesarios (no reconstruye la postura entera)
  - Ejemplo: si el pie de atrás no está a 45°, aparece la opción "pie de atrás a 45°" — si era el único fallo, la postura se completa
- [ ] El número/probabilidad de errores podría estar vinculado a la maestría — a mayor maestría, menos errores automáticos
- [ ] Esto también funciona como mecánica de "profesor interior": el jugador aprende a observar qué falla

### 4. Sistema de Maestría
- [ ] Cada asana tiene un nivel de maestría que sube con la práctica
- [ ] A mayor maestría: más tiempo en postura + mayor capacidad de almacenamiento de beneficios
- [ ] Ejemplo — "Fuerza y Enraizamiento":
  - Capacidad inicial: 50
  - Incremento: +1 cada 5 prácticas
  - Capacidad máxima: 100
- [ ] Definir qué otros beneficios/stats siguen esta progresión

---

## Narrativa — Nivel 1

**Setup:**
Un grupo de amigos llega a un santuario animal. En el santuario se trabaja para hacer a los animales amigables entre sí y con las personas.

**Motivación del grupo:**
La mayoría no llegó al santuario buscando formar parte de él — llegaron con una agenda: tienen pruebas de que la maestra del lugar realiza cosas paranormales (magia) y quieren aprender sus secretos.

**Tensión central:**
- Los personajes viven la experiencia del santuario (cuidado de animales, convivencia, naturaleza)
- Pero por debajo operan con curiosidad/ambición hacia la magia de la maestra
- Esto crea una dualidad entre lo que hacen (cuidar animales, practicar yoga) y lo que buscan (acceso a conocimiento prohibido o secreto)

**Secuencia de introducción — Nivel 1:**
1. La maestra recibe al grupo y los felicita por haber encontrado el lugar
2. Les dice que ve potencial en los integrantes
3. Les propone: para conocer los secretos del lugar, deben participar como voluntarios
4. Lleva al equipo a la cabaña
5. Les muestra la esterilla
6. Pide al jugador que se acerque a la esterilla
7. Se revela el **status físico** del jugador (stats iniciales, contenedores vacíos)
8. El jugador hace sus **primeras flexiones, abdominales y piernas** — tutorial del sistema de posiciones corporales
9. La maestra comenta el **nivel actual del jugador y cuánto le falta para avanzar** (feedback de progresión en pantalla)
10. La maestra lleva al equipo al **área de crías** — primera zona de tareas
11. Primeras tareas: alimentar, cuidar y jugar con las crías (ver [`docs/fauna-gameplay.md`](docs/fauna-gameplay.md) para el detalle de actividades)
12. Objetivo: que las crías se conviertan en **adultos amigables** (con personas y entre sí)
13. Cuando el jugador alcanza un umbral de estadísticas + vínculo con las crías, la **maestra aparece** y le enseña su **primera postura de yoga** — cierre del Nivel 1 y desbloqueo del sistema de asanas

**Misiones secundarias del Nivel 1:**
Ver sección "Misiones Secundarias — Compañeros" más abajo.

**Implicaciones de diseño:**
- El santuario es el hub principal del nivel 1
- Los animales son personajes con los que el jugador interactúa (sistema de fauna ya en código)
- La maestra es un NPC clave — fuente de desbloqueo de asanas y conocimiento mágico
- Los encantamientos (tabla periódica) probablemente se descubren a través de la maestra o de escritos del santuario
- La tensión grupo/maestra puede generar misiones, diálogos y momentos narrativos

---

## Misiones Secundarias — Compañeros (Nivel 1 en adelante)

### Principio de diseño

No todos los compañeros llegan al santuario al mismo tiempo ni con el mismo nivel:

- **Antiguos del santuario** — llevan más tiempo y en general tienen stats más elevadas, aunque no necesariamente en todas las áreas. Algunos serán claramente superiores al jugador; otros, sorprendentemente cercanos o incluso inferiores en alguna stat concreta.
- **Compañeros del grupo de llegada** — llegan junto al jugador o poco después. Pueden tener stats similares, menores o mayores dependiendo de su historia. No ser "del grupo" no implica ser peor.
- **Compañeros que aparecen después** — pueden llegar en cualquier momento del juego con cualquier nivel. Un recién llegado puede resultar extraordinariamente hábil en algo; otro puede estar empezando desde cero.

Esto elimina la lógica de "más antiguo = mejor" y hace que cada encuentro sea una sorpresa. Cada compañero destaca en una o más estadísticas y tiene una **historia de trasfondo** que explica (o sugiere) su nivel — el jugador la va descubriendo al pasar tiempo con ellos.

Los compañeros **crecen mientras el jugador no los ve**: al reencontrarlos en el Nivel 2, habrán avanzado a su propio ritmo — lo que puede crear admiración, sorpresa o incluso la sensación de haberles alcanzado.

### Especialización por compañía

El tiempo que el jugador pasa con cada compañero influye en qué estadísticas sube más. No hay una elección explícita — emerge del comportamiento:

- Pasar tiempo con compañeros de **velocidad/elegancia** → el jugador gana más velocidad
- Pasar tiempo con compañeros de **fuerza** → el jugador gana más fuerza
- Repartir el tiempo entre todos → progresión más balanceada pero menos pronunciada en ninguna
- Dedicarse principalmente a **entrenar en la esterilla** → subida de stats según los ejercicios elegidos, independiente de los compañeros

Esto crea perfiles de jugador distintos sin forzar una elección: el jugador simplemente vive la experiencia y el resultado refleja sus prioridades.

### Stat: Observación

La Observación es la capacidad de leer el entorno con detalle. No es una herramienta para conseguir cosas — es una práctica en sí misma. A veces puedes mirar sin buscar algo, simplemente mirar por ver qué hay allí.

**Implementación — Radio de consciencia:**
- La stat define un radio alrededor del jugador
- Dentro del radio, los objetos observables emiten una señal sutil y diegética: un brillo leve, una partícula, un sonido suave — nada de borde de UI genérico
- Al acercarse lo suficiente, aparece un pensamiento interno breve (no de UI): *"esto huele diferente"*, *"las hojas de esta planta tienen algo"*
- El radio crece visiblemente con la stat — el jugador siente el cambio
- Jugadores con poca Observación pasan por alto cosas que están ahí

**Modo contemplativo:**
- El jugador puede detenerse y simplemente mirar el entorno — sin objetivo, sin timer
- El mundo devuelve algo: no siempre un ítem o pista, a veces un detalle, una textura, un comportamiento de animal que no vería en movimiento
- Practicar el modo contemplativo sube la stat de Observación por sí solo, independientemente de los compañeros
- Mirar como práctica — coherente con el alma del juego

**Usos en Nivel 1:** notar ingredientes, comportamientos de animales, detalles del entorno
**Usos posteriores:** puente hacia la meditación (observación interior) y hacia las recetas mágicas de la tabla periódica

---

### Compañera: Panterilia

**Trasfondo:**
Mujer muy trabajadora que huyó de su país buscando poder llevarse a su hija consigo. No dejó los estudios para abrir oportunidades y finalmente lo logró, pero pasaron años en el proceso — con lo cual la relación con su hija no es la más cercana. Se piden permiso para darse abrazos. La hija parece más conectada con su abuela, que la conoce más.

Panterilia no es capaz de hablarle mal del padre a su hija, ni de explicarle que él es una de las razones por las que sintió la necesidad de huir. Tiene que seguir en contacto con él porque la hija es menor de edad y necesita permisos. Si hubiera sido por Panterilia, nunca hubiera hecho saber al padre de la existencia de la hija — pero cuando dio a luz, su madre la convenció de agregarle el apellido del padre. Lo considera un gran error: ahora debe contactarlo ocasionalmente y él no coopera, se hace de rogar incluso con trámites beneficiosos para su hija, y la acusa de buscarle con excusas.

**Stats y nivel de llegada:**
Llega al santuario poco después del jugador — no viene con stats elevadas. Es trabajadora, no una experta; su crecimiento se verá durante el juego.

**Arco en Nivel 1:**
Panterilia era la reina de la limpieza — pero usaba muchos químicos y les tenía asco a los insectos. En el santuario llega a encariñarse con ellos y empieza a investigar componentes naturales que mantengan la limpieza sin dañar ninguna forma de vida ni el ecosistema. Esto la lleva a realizar tareas de limpieza del lugar y búsquedas de ingredientes naturales.

Tiene un talento innato para la nutrición — fruto de años preocupándose por la alimentación de su hija. Al final del Nivel 1 decide que quiere entrar en el mundo de la nutrición animal. Si el jugador pasa mucho tiempo con ella, también se preocupa por su alimentación.

**Ganancias del jugador al acompañarla (Nivel 1):**
- **Observación** — aprender a distinguir plantas, insectos, ingredientes; Panterilia enseña literalmente a "ver": la primera misión podría ser que el jugador no nota nada y ella sí
- **Puntos de consciencia** — semilla para la meditación posterior; aún sin uso pleno en Nivel 1 pero se acumula
- **Conocimiento de nutrientes** — base para recetas mágicas de la tabla periódica en niveles posteriores

**Arco en Nivel 2:**
Se está llevando mejor con su hija y quiere darle más confianza — abrirse a ella como compañeras que trabajan codo con codo. Asciende a formadora y da clases a nuevos participantes, o a antiguos de otras áreas que se acercan a su área de limpieza y nutrición. Sigue incrementando sus stats de Observación, consciencia y meditación — con cambio visual/energético perceptible tanto en ella como en el jugador que la acompaña.

**Misión de ejemplo — Nivel 1:**
El jugador la encuentra limpiando una zona del santuario. Ella detecta plantas y insectos que el jugador ni ve. Al unirse, el jugador aprende a mirar — primer ejercicio práctico de Observación.

---

### Stat: Satisfacción

La Satisfacción es la capacidad interna del jugador para celebrar la vida — desde el simple hecho de respirar hasta los grandes momentos. No es una emoción pasajera: es una stat que crece y que tiene efectos mecánicos reales.

**Estructura:**
- El jugador tiene una **barra de Satisfacción** propia, independiente de cualquier compañero
- El **tamaño** de la barra crece con el trabajo del umbral → más capacidad = mayor resistencia a ataques fuertes directos y a ataques continuos sostenidos
- La **velocidad de llenado pasivo** también crece → en niveles altos la barra se restaura sola con el simple hecho de existir, sin necesidad de acción activa
- La restauración pasiva **se suma** a restauraciones externas (compañeros, comida, descanso, yoga) — no las reemplaza
- En niveles muy desarrollados puede incorporar un **multiplicador a las restauraciones externas**: las fuentes externas curan más porque el cuerpo ya está en mejor disposición de recibir

**Formas de restauración — varios canales:**
Distintos compañeros y actividades activan distintos canales de restauración. El jugador gravitará naturalmente hacia los que más le resuenen, y eso define parte de su perfil:
- **Celebración / alegría compartida** — canal de Gohageneis
- **Calma contemplativa** — canal de Panterilia (Observación, consciencia)
- **Desahogo / esfuerzo físico** — canal de Goluis (resistencia)
- *(Más canales por definir con futuros compañeros)*

**Restauración por cercanía a compañeros:**
Estar cerca de ciertos compañeros restaura al jugador pasivamente, sin necesidad de completar una misión o activar nada. Cada compañero tiene su propio radio y ritmo de influencia.

**Implicaciones de rol:**
Las stats que el jugador priorice determinan su rol natural a futuro:
- Satisfacción alta + restauración externa maximizada → **Healer / Tank**
- Velocidad + resistencia al desgaste (Goluis) → **Damage / Support**
- Balance entre todas → perfil versátil, menos pronunciado en cada área

---

### El Papi Gohageneis — Crías

Artista de la vida cotidiana. Narra historias, da consejos, se une a bromear, cantar, bailar, actuar, chismosear, hacer el tonto, quejarse, repartir cariño, lloriquear, dar ánimos, exigir, perdonar, lanzarse rosas, caer en la tentación de comer ingredientes sin permiso, adorar cada detalle de sus romances, disfrutar cada nuevo invento o creación. Lleva ese gusto innato por festejar la vida.

Y otras veces cae: le han llamado la atención, le han ofendido, está cansado, siente que le dejan la parte más difícil del trabajo. Se queja — y al poco rato ya está cantando otra vez.

**Trasfondo:**
De pequeño vio que había peligros reales. Un amigo suyo fue matado por estar con una chica que tenía pareja. En el bar donde trabajaba, un hombre muy borracho le dijo que le hubiera gustado ser como él — ganarse la vida, crear buen ambiente, hacer felices a los clientes — pero que le tocó ser secuestrador. Gohageneis aprendió pronto que la alegría no es ingenuidad: es una postura filosófica frente a lo que podría haber sido.

**Tareas con las crías:**
También cuida crías. Cuando pide ayuda al jugador, lo que ocurre es que él se toma su tiempo con cada una — les habla, les hace bromas, las mima, convierte cada cría en un personaje. La tarea que debería durar diez minutos dura el doble porque Gohageneis "dirige la escena". El jugador hace el trabajo mientras él actúa. Resultado curioso: sus crías tienen vínculos excepcionalmente fuertes — el método funciona, solo que a su ritmo.

**Mecánica — CelebrationCharge:**
- Una barra que se llena mientras el jugador está cerca de Gohageneis o participando en sus momentos (bromas, canciones, tareas compartidas)
- Al alcanzar el umbral, activa un efecto de sanación interna suave y pasiva: reducción en la acumulación de fatiga mental o pequeña recuperación de desgaste ya acumulado
- Trabajar este umbral repetidamente desarrolla la **stat de Satisfacción** del jugador

**Ganancias del jugador:**
- **Satisfacción** — la stat principal: barra más grande, llenado más rápido, multiplicador a restauraciones externas en niveles altos
- **Resistencia al Desgaste emocional** — los errores y las presiones del día acumulan menos peso

**Arco:**
Gohageneis da alegría con generosidad — pero cuando cae, se levanta solo rápido, de vuelta a la actuación. Su superación es aprender a dejarse sostener: encontrar personas muy afines que lo amen también cuando está abajo, y permitirse estar en ese lugar un momento sin volver inmediatamente a la música. Alguien que da tanto necesita también aprender a recibir.

---

### Misiones secundarias (otros compañeros — por definir)

**Misión: Barrer con [Nombre]**
- Compañero barre con velocidad y elegancia que parece sobrenatural
- Al ayudar, el jugador gana **velocidad** y **coordinación**
- Historia de trasfondo: por definir
- Reencuentro en Nivel 2: aún más rápido — el jugador puede seguirle el ritmo un poco mejor

**Misión: Mover material con [Nombre]**
- Compañero carga materiales con fuerza llamativa
- Al ayudar, el jugador gana **fuerza**
- Historia de trasfondo: por definir
- Reencuentro en Nivel 2: levanta más — el jugador nota que él también puede más

---

### El Maestro Goluis — Cocina

Respetado por todos en la cocina. Llegó al santuario antes que el jugador — es un nivel más antiguo. Al principio puede ser de los que más presión ponga: le dirá al jugador que no le gusta enseñar y soltará comentarios sobre prisa y organización.

**Mecánica de presión:**
- Sus comentarios otorgan un pequeño bono de velocidad para terminar la tarea en curso → el jugador tiene más tiempo para otra misión
- Pero agotan física y mentalmente
- Si el agotamiento mental supera cierto umbral, el jugador tambalea — lo que afecta el tiempo final de la misión
- Riesgo real: seguirle el ritmo puede salir caro si ya se viene cargado

**Trasfondo:**
Se trajo a la madre de su hija de vacaciones al santuario. La madre no quiso quedarse y volvió al país con la niña. Le llegaron noticias de que la madre no cuida bien a la niña. La niña le pidió quedarse con él. Cuando tuvo la oportunidad, se la trajo — y la niña le convenció de quedarse definitivamente.

La madre amenazó con una denuncia. Goluis era bandido antes y tiene contactos que disuadieron a la madre de tomar acciones legales. Intenta hablar con ella por teléfono para reconciliarse y ser una buena familia para la niña, pero ella no quiere — dice que solo se acostó con él por el alcohol y ahora sale con otro. Goluis hace dos trabajos en el santuario para aportar más económicamente. Vive con su mamá y dos hermanos; su madre le ayuda a cuidar a la niña. Trabaja muy rápido para no tener problemas en ninguno de los dos trabajos por falta de sueño. Ha sufrido varias bajas: problemas de pie, extracción de un testículo, media cara paralizada, y varios accidentes.

**Ganancias del jugador al acompañarle:**
- **Velocidad** — sus comentarios de presión la activan, con riesgo de fatiga
- **Resistencia** — aguante físico y mental ante la carga. Pasar tiempo con alguien que trabaja doble turno con el cuerpo dañado y sigue en pie enseña algo sobre límites. Base para posturas de yoga más exigentes en niveles posteriores.

**Arco en Nivel 1 y 2:**
Goluis empezó solo por dinero, sin interés en el entrenamiento ni fe en el yoga. Con el tiempo empieza a soltar mensajes amigables: le dice al jugador que lo considera su amigo, que quiere que vaya a su cumpleaños, lo recibe con una sonrisa o cara cómica y un abrazo. Le pregunta directamente al jugador si lo considera su amigo. Sale con una mujer mayor y va aprendiendo a tratar a todos con más paciencia. Al final del Nivel 2 gana humildad, cariño y paciencia suficiente para comenzar a entrenar — y así apuntar a algo más.

---

### Diseño de los compañeros

- [ ] Definir nombres y perfiles de cada compañero restante
- [ ] Definir la stat principal (y secundaria) de cada uno
- [ ] Escribir trasfondo de cada uno (puede revelarse en fragmentos)
- [ ] Definir cómo y cuándo reaparecen en Nivel 2 mostrando su crecimiento
- [ ] Establecer el umbral de "tiempo con compañero" que influye en la especialización del jugador

---

## Narrativa — Nivel 2

**Condición de activación:**
El jugador ha fortalecido suficientemente el lazo con las crías a su cargo (métrica de vínculo por definir). Las crías entran en sus ciclos naturales — momentos en que no necesitan atención.

**Mecánica central — Ciclos de las crías:**
- Las crías alternan entre fases de necesidad y fases de descanso/ciclo autónomo
- Durante las fases autónomas aparece una **cuenta regresiva** visible en pantalla
- El jugador queda **libre para moverse por el mundo** hasta que se agote el tiempo
- Al finalizar el tiempo, el jugador deja lo que esté haciendo y **corre de vuelta al puesto**
- La urgencia crea ritmo y tensión natural sin interrumpir el flujo de exploración

**Actividades durante el tiempo libre:**
- Hablar con **NPCs del mundo** que ofrecen misiones secundarias
  - Las misiones otorgan mejoras en estadísticas (fuerza, enraizamiento, etc.)
- Volver a la **esterilla y entrenar** (ejercicios básicos, asanas desbloqueadas)
- Exploración libre del mapa

**Progresión del nivel:**
- El jugador sigue subiendo estadísticas y fortaleciendo el lazo con las crías
- La frecuencia y duración de los ciclos puede variar — más tiempo libre conforme crece el vínculo, o menos si las crías se vuelven más demandantes al crecer
- El sistema de asanas ya está desbloqueado (la maestra lo enseñó al cierre del Nivel 1) — el jugador lo integra ahora con libertad

---

## Comentarios y decisiones de diseño

- **Teclas del teclado**: se alinean a posición de escritura para que el jugador pueda usarlas naturalmente, sin tener que mover las manos a las flechas.
- **Encantamientos como tabla periódica**: idea interesante — los elementos químicos como metáfora de poderes/encantamientos crea coherencia temática (naturaleza, ciencia, magia).
- **Asanas como mecánica de recuperación/fuerza**: elegante porque requiere atención activa del jugador (seleccionar posturas) pero no interrumpe el flujo del juego (se puede encolar). La progresión por maestría premia la práctica constante.
- **Límite de almacenamiento por stat**: buen sistema de cap dinámico — empieza bajo para no abrumar al jugador nuevo y crece orgánicamente.
- **Disponibilidad de la interfaz de posturas — dos opciones**:
  - **Opción A (ideal)**: calcular en tiempo real si hay espacio suficiente para ejecutar la postura sin colisionar con el entorno. Más flexible, pero puede impactar rendimiento — evaluar antes de implementar.
  - **Opción B (world design)**: la interfaz solo está disponible sobre esterillas (`yoga mat`) repartidas por el mundo. Elimina el cálculo de colisión, añade intención de diseño (el jugador busca y descubre esterillas), y puede usarse para guiar al jugador por zonas del mapa. **Preferida si Opción A afecta rendimiento.**
- **Posturas**: siempre las mismas opciones independientemente del contexto (posición del jugador, terreno, etc.)

---

## Arquitectura relevante

### UI / Menús

| Archivo | Responsabilidad |
|---|---|
| `FollowingArrays.cs` | Sistema base de menús flotantes: instancia, cola, mostrar/ocultar |
| `Palette.cs` | Extiende FollowingArrays — modos Direct / Formula / Hybrid / Dialogue, grupos |
| `PaletteConfig.cs` | Descriptor de un menú Palette: modo, elementos, evaluador, grupos |
| `PaletteElement.cs` | Botón individual; puede materializarse como bloque físico en el mundo |
| `PaletteElementData.cs` | Datos de un elemento: label, icon, shortcut, bondMin, payload |
| `PaletteGroup.cs` | Agrupación de elementos para navegación por categoría |
| `IPaletteEvaluator.cs` | Interfaz para evaluar una fórmula acumulada (asanas, encantamientos) |
| `MaterializationExecutor.cs` | Ejecuta materialización de elementos en patrones espaciales |
| `ArrangementPattern.cs` | Patrones: Stairs, Barrier, Platform, FallBreaker |
| `TrackerBehavior.cs` | Base para botones flotantes que rastrean un GO (apunta + sigue) |
| `AnimalRadar.cs` | Rastrea animales; extiende TrackerBehavior |

**Cómo se abre un menú Palette desde cualquier sistema:**
```csharp
Palette.Open(new PaletteConfig {
    mode     = PaletteConfig.Mode.Direct,   // o Formula, Hybrid, Dialogue
    elements = miArrayDeElementos,
    evaluator = miEvaluador                 // solo en Formula / Hybrid
});
```

**Usos previstos:**
| Sistema | Modo | Evaluador |
|---|---|---|
| Asanas | Formula | `AsanaEvaluator : IPaletteEvaluator` — verifica match de posiciones |
| Encantamientos | Formula / Hybrid | `EnchantmentEvaluator` |
| Cuidado de crías | Direct | — |
| BondActivities activas | Direct | — |
| Diálogo | Dialogue | — |

### Jugador

| Archivo | Responsabilidad |
|---|---|
| `PlayerStats.cs` | Stats del jugador: Satisfacción, Fatiga Mental, Estrés, Sueño, Observación, Velocidad, Resistencia |
| `PlayerCtrl.cs` | Movimiento, animaciones, apuntado |
| `BondActivityManager.cs` | Gestiona BondActivities: práctica activa/pasiva, trauma, desbloqueo por Satisfacción |

### Cámara

| Archivo | Responsabilidad |
|---|---|
| `CameraManager.cs` | Switch 3ª/1ª persona, robos de cámara, efectos por stats (shake, FOV, apagones) |
| `CameraZoneTrigger.cs` | Trigger de zona → notifica al CameraManager (e.g. área de crías → 1ª persona) |

### Compañeros

| Archivo | Responsabilidad |
|---|---|
| `CompanionBase.cs` | Simulación interna (fatiga, estrés, mood), proximidad con restauración/descarga, anclas de pensamiento |
| `IBondable.cs` | Interfaz para cualquier entidad que pueda tener bond con el jugador |
| `ThoughtAnchor.cs` | Creencia/patrón con peso (−1 bloquea, +1 impulsa) y velocidad de cambio de arco |
| `Goluis.cs` | Presión activa, fatiga de doble turno, ancla `yoga_skepticism` |
| `Panterilia.cs` | Restauración estable, bonus de Observación por proximidad, arco `chemical_reliance` |
| `Gohageneis.cs` | CelebrationCharge → burst de Satisfacción + alivio de fatiga |
| `WorldBondable.cs` | IBondable para objetos/lugares: esterilla, sol, montaña, etc. |

### Vínculos

| Archivo | Responsabilidad |
|---|---|
| `BondActivity.cs` | ScriptableObject: práctica que construye bond; gestiona trauma y bloqueo por Satisfacción |
| `BondActivityManager.cs` | Gestiona todas las actividades del jugador: activa/pasiva, tags de contexto, tick diario |
| `Bond.cs` | Vínculo afectivo animal→objetivo (0–100); tipos Imprint / Friend |

### Animales

| Archivo | Responsabilidad |
|---|---|
| `Animal.cs` | Estado base: hungry, stress, bond, fase de vida |
| `PostNatalManager.cs` | Ciclos de las crías: fases de necesidad y fases autónomas (base del Nivel 2) |
| `PostNatalStage.cs` | Etapa post-natal por especie; flags firstSolidEaten, firstNestExit |
| `ICarrier.cs` | Interfaz para transportar FoodItems (pendiente implementación en PlayerCtrl) |
| `FoodItem.cs` | Ítem de comida con referencia a quien lo depositó (droppedBy) |

---

## Propuesta de arquitectura — IBody / IMind / IBondable

Propuesta de refactorización profunda de las interfaces de stats. **No implementar hasta
confirmar diseño completo**, pero documentar aquí para que guide las decisiones de corto plazo.

### El problema actual

`PlayerStats` mezcla stats físicas (velocity, physicalResistance) con stats mentales/emocionales
(satisfaction, mentalFatigue, stress, sleepiness, observationRadius) en un solo componente.
`IAnimal` describe el cuerpo físico de los animales, pero el jugador y los NPCs también tienen cuerpo.
`CompanionBase` duplica parcialmente stats mentales (fatigue, stress, mood) sin interfaz común.

### La propuesta

Tres interfaces ortogonales que cualquier entidad puede implementar:

```
IBody      — estado físico: velocidad, resistencia, temperatura, reservas de grasa, hambre
IMind      — estado mental/emocional: satisfacción, fatiga mental, estrés, sueño, observación
IBondable  — capacidad de vínculo: bond con el jugador, efecto por proximidad
```

**Quién implementa qué:**

| Entidad | IBody | IMind | IBondable |
|---|---|---|---|
| Jugador | ✅ | ✅ | — (es quien forma los vínculos, no quien los recibe) |
| Animal / Cría | ✅ | parcial (stress, vocalización) | ✅ |
| Compañero (CompanionBase) | parcial (fatigue) | ✅ (fatigue, stress, mood) | ✅ |
| Objeto / Lugar (WorldBondable) | — | — | ✅ |

### IAnimal → IBody

`IAnimal` es en realidad una descripción del cuerpo físico, no de los animales como concepto.
Renombrarlo a `IBody` lo hace aplicable al jugador, NPCs y compañeros sin cambiar su semántica.

```csharp
// Actual:
public interface IAnimal {
    AnimationsName animationsName { get; }
    bool aware { get; set; }
    void Hurt(float damage);
    IEnumerator Escape(bool team, List<GameObject> enemies);
}

// Propuesto:
public interface IBody {
    // Stats físicas
    float velocity          { get; }
    float physicalResistance { get; }
    float temperature       { get; }     // ya existe en Animal.cs
    float fatReserves       { get; }     // ya existe en Animal.cs
    // Acciones físicas
    void Hurt(float damage);
    IEnumerator Escape(bool team, List<GameObject> enemies);
}
```

### IMind (nueva interfaz)

Stats mentales/emocionales que actualmente viven en `PlayerStats` y en `CompanionBase`:

```csharp
public interface IMind {
    float satisfaction      { get; }
    float mentalFatigue     { get; }
    float stress            { get; }
    float sleepiness        { get; }
    float observationRadius { get; }

    void RestoreMind(float amount, MindChannel channel);
    void DrainMind(float amount, MindChannel channel);
}

public enum MindChannel { Satisfaction, MentalFatigue, Stress, Sleepiness, Observation }
```

### Routing en Restore()

En lugar de un solo `PlayerStats.Restore(amount, StatChannel)` que hace todo,
la lógica de restauración consulta qué interfaces implementa el target:

```csharp
// Ejemplo conceptual:
void ApplyEffect(GameObject target, EffectData effect) {
    IBody  body = target.GetComponent<IBody>();
    IMind  mind = target.GetComponent<IMind>();
    IBondable bond = target.GetComponent<IBondable>();

    if (effect.isPhysical && body != null)
        body.RestoreBody(effect.amount, effect.bodyChannel);

    if (effect.isMental && mind != null)
        mind.RestoreMind(effect.amount, effect.mindChannel);

    if (effect.growsBond && bond != null)
        bond.GrowBondWithPlayer(effect.bondAmount);
}
```

Esto permite que una sola acción (p.ej. una asana) afecte simultáneamente al cuerpo,
a la mente y al vínculo, sin que ningún sistema sepa nada de los otros.

### Stats que se mueven de PlayerStats a IMind

| Stat actual en PlayerStats | Va a |
|---|---|
| satisfaction, satisfactionCapacity, satisfactionPassiveRate, restorationMultiplier | IMind |
| mentalFatigue | IMind |
| stress | IMind |
| sleepiness | IMind |
| observationRadius | IMind |
| velocity | IBody |
| physicalResistance | IBody |

`PlayerStats` podría quedar como MonoBehaviour que implementa tanto `IBody` como `IMind`,
o dividirse en dos componentes (`PlayerBody`, `PlayerMind`) que se enlazan en el mismo GameObject.

### Implicación en los compañeros

`CompanionBase` ya tiene `fatigue`, `stress`, `mood` — podría implementar `IMind`.
Cuando el jugador está cerca de Panterilia, en lugar de `playerStats.Restore(...)` directamente,
Panterilia aplica el efecto a través de la interfaz: `mind.RestoreMind(amount, MindChannel.MentalFatigue)`.
Esto hace que el sistema de compañeros funcione para cualquier entidad con `IMind`, no solo el jugador.

### IMindSimple — subconjunto para compañeros

`CompanionBase` no necesita implementar `IMind` completa. Solo necesita las stats que ya tiene.
`IMind` puede extender `IMindSimple` para no duplicar declaraciones:

```csharp
public interface IMindSimple { float mentalFatigue { get; set; } float stress { get; set; } float mood { get; set; } }
// IMind extends IMindSimple and adds satisfaction, sleepiness, observationRadius...
```

Ver `docs/ibody-imind.md` para la propuesta completa.

### NPCs — IBody e IMind para todos

Los NPCs implementan las mismas interfaces. No hay lógica especial "solo para el jugador":

- **La maestra**: `IBody` con `flexibility >= 0.9` — puede demostrar cualquier asana
- **Goluis**: `IBody` alta `strength`, `flexibility` media — su estilo de enseñanza refleja sus límites
- **Panterilia**: `IMind` con bajo estrés — su bonus de Observación surge de sus propias stats
- **Animales/crías**: `IBody` (via IAnimal→IBody rename), `IMind` parcial (stress, vocalización)

Ver `docs/asana-system.md` §TeacherNPC y §NPCs — IBody e IMind para todos.

### Pendientes antes de implementar

- [ ] Definir qué stats de `IAnimal` migran a `IBody` vs quedan solo en `Animal.cs`
- [ ] Decidir si `PlayerStats` se divide en dos componentes o implementa ambas interfaces
- [ ] Definir `MindChannel` y `BodyChannel` como reemplazos de `StatChannel`
- [ ] Confirmar que `IMind` extiende `IMindSimple` (no duplicar declaraciones)
- [ ] Evaluar impacto en `CameraManager` (lee directamente de `PlayerStats` — necesitará `IMind`)
- [ ] Evaluar impacto en `CompanionBase.ApplyProximityEffect()` (actualmente llama `playerStats.Restore`)
- [ ] Evaluar si `BondActivity` registra trauma leyendo `IMind.stress` en lugar de `PlayerStats.stress`
- [ ] Añadir `BodyPartStats` per-extremidad a `IBody` (flexibility/strength/stability) — ver `docs/asana-system.md`

---

## Sistema de Bloques — Propuestas pendientes de evaluación

Las ideas siguientes **no están implementadas**. Se documentan para la próxima tarea.

### A. Bloques con propiedades físicas por elemento de la tabla
Cada elemento químico confiere al bloque materializado una propiedad distinta:

| Familia | Comportamiento del bloque |
|---|---|
| Gases nobles (He, Ne, Ar…) | Livianos, flotan hacia arriba lentamente — ideales para alfombra |
| Metales densos (Pb, Au, Fe) | Pesados, máxima estabilidad, resisten impactos |
| Carbono (C) | Configurable: grafito = flexible/apilable; diamante = irrompible |
| Elementos reactivos (Na, K) | Inestables — se disuelven antes si reciben un impacto o agua |

Implicación: añadir campo `BlockProperties` a `PaletteElementData` con masa, durabilidad, etc.
El prefab del bloque lee estas propiedades al materializar.

### B. El jugador elige el material uniforme para todos sus bloques
En lugar de que cada bloque sea del elemento que representa, el jugador puede
seleccionar un elemento y aplicarlo a toda su paleta. Todos los bloques adquieren
las propiedades de ese elemento hasta que se cambie la selección.

Implicación: un `GlobalBlockMaterial` en la paleta que sobreescribe `BlockProperties`
de todos los elementos activos.

### C. Expansión de bloque — un solo bloque cubre toda una superficie
Un bloque puede expandirse para cubrir un área mayor (p.ej., toda una pared como barrera).
El jugador activa la expansión y el bloque escala hasta dimensiones útiles.

Implicación: `PaletteElement.Expand(Vector3 targetScale)` con animación SmoothStep,
límite de expansión configurable por elemento.

### D. Plataforma móvil (alfombra mágica)
El jugador se coloca sobre bloques dispuestos bajo sus pies (`PlatformPattern`) y los
mueve como unidad. Implica un coroutine continua que desplaza los bloques y arrastra
al jugador con ellos.

**Sobre poner al jugador dentro del GameObject:**
Poner al jugador como hijo del bloque (parenting) es eficiente — Unity lo resuelve con
una simple actualización de jerarquía de Transform. El jugador no nota diferencia si la
animación es suave. **Caveat importante**: si el `PlayerCtrl` usa `CharacterController`
(el setup más común para primera persona), el parenting NO arrastra al personaje
automáticamente porque `CharacterController` opera siempre en espacio de mundo.
La solución estándar: cada frame, calcular el `deltaPosition` del bloque y aplicarlo
manualmente al `CharacterController.Move()` del jugador.
Si `PlayerCtrl` usa `Rigidbody`, el parenting funciona directamente pero puede provocar
artefactos de física en colisiones. Evaluar al implementar `PlayerCtrl`.

Implicación: nuevo modo `DynamicPlatform` en `ArrangementPattern` + lógica en
`MaterializationExecutor` para la coroutine de movimiento continuo.

---

## Propuestas de balance — pendientes de evaluación

### Multiplicador de fórmula (Palette, modo Formula / Hybrid)

El `formulaMultiplier` en `PaletteConfig` está en `1.5f`. El objetivo es que usar la fórmula
en lugar del botón directo se **sienta distinto y valga la pena** — no que sea levemente mejor.

**Propuesta:** subir el rango a **2.5×–4×** dependiendo de la complejidad de la fórmula:

| Tipo de acción | Multiplicador sugerido |
|---|---|
| Asana (2–3 posiciones) | ×2.5 |
| Encantamiento simple (2–3 elementos) | ×3 |
| Encantamiento avanzado (5+ elementos) | ×4 |
| Cuidado de cría (Direct, sin fórmula) | ×1 (sin bonus — es directo por diseño) |

Criterio de calibración: el jugador que practica la fórmula debe notar que llega a umbrales de efecto
que el modo directo no alcanza aunque lo use varias veces seguidas. La diferencia debe ser visible sin
necesitar explicación.

**Pendiente:** calibrar con feedback de jugabilidad real. No cambiar hasta tener sesiones de prueba.

---

### Apilamiento de asanas (stack de fórmula durante postura activa)

Mientras una asana está activa (el beneficio se está entregando — segundos a minutos),
el jugador puede **volver a introducir la misma fórmula**. Cada completación adicional
multiplica el beneficio total acumulado:

- Completar la fórmula 1 vez → beneficio normal × `formulaMultiplier`
- Completar 2 veces → beneficio × `formulaMultiplier²`
- Completar N veces → beneficio × `formulaMultiplier^N`

**Propósito:** recompensar la práctica sostenida y la atención. El jugador que sigue presente
y sigue repitiendo la fórmula en lugar de dejarse llevar pasivamente saca más.

**Implicación de código:**
`AsanaQueue.benefitRate` podría escalar por un `stackMultiplier` que sube con cada
completación de fórmula mientras la asana está activa.

**Precaución:** definir un tope de stack (p.ej. ×4 máximo) para evitar explotación.

**Pendiente:** definir si el stack se mantiene entre lados (posturas bilaterales) o se reinicia.

---

## Análisis de sistemas incorporados

Sistemas añadidos con el último pull. Se analizan en relación al código ya existente.

### Resumen de qué llegó

| Archivo | Qué hace |
|---|---|
| `PlayerStats.cs` | Stats del jugador: satisfaction, mentalFatigue, stress, sleepiness, observationRadius, velocity, physicalResistance. API: `Restore()` / `Drain()` via `StatChannel` |
| `IBondable.cs` | Interfaz genérica para todo lo que puede formar vínculo con el jugador: compañeros, animales, objetos, lugares |
| `CompanionBase.cs` | Base para compañeros: estado interno (fatigue/stress/mood), proximidad → restaura o drena stats del jugador, ThoughtAnchors |
| `ThoughtAnchor.cs` | Creencia que sesga el comportamiento de un compañero; se suaviza con el arco narrativo |
| `Gohageneis.cs` | Canal: Satisfaction. CelebrationCharge → burst de restauración al alcanzar umbral |
| `Goluis.cs` | Canal: MentalFatigue. Presión → sube estrés del jugador. `yoga_skepticism` bloquea asanas hasta que avanza el arco |
| `Panterilia.cs` | Canal: MentalFatigue. Añade bonus a `observationRadius` del jugador al estar cerca |
| `BondActivity.cs` | ScriptableObject. Práctica que construye bond con cualquier IBondable. Sistema de trauma/bloqueo/desbloqueo por satisfacción |
| `BondActivityManager.cs` | Attached al Player. Registra IBondables por ID, activa prácticas activas/pasivas, tick de trauma diario |
| `WorldBondable.cs` | Attach a cualquier objeto/lugar del mundo para que forme vínculo con el jugador |
| `CameraManager.cs` | Singleton. 1ª/3ª persona, robos de cámara, efectos visuales por PlayerStats |
| `CameraZoneTrigger.cs` | Trigger de zona → CameraManager.OnEnterCubArea / RequestRobbery |
| `Asana.cs` | ScriptableObject. Posiciones requeridas, StatType, maestría, shortcut unlock a 10 prácticas |
| `AsanaDetector.cs` | Attached al Player. Escucha pulsaciones de BodyPosition, detecta match de asana |
| `AsanaQueue.cs` | Gestiona la asana activa y cola. Entrega beneficio continuo a sus propios contenedores |
| `BodyPosition.cs` / `BodyPositionButton.cs` | Datos de posición corporal + botón UI que notifica al detector |

---

### Gaps y conflictos a resolver

#### ~~1. StatType vs StatChannel — desconectados~~ ✅ RESUELTO (03/07/2026)

`Asana.cs` ahora tiene `StatChannel channel` — el diseñador asigna por asana qué stat del jugador restaura.
`AsanaQueue.DeliverBenefit()` llama `_playerStats.Restore(gain, activeAsana.channel)` en cada tick.
Referencia a `PlayerStats` resuelta vía `GetComponent<PlayerStats>()` en `Start()`.

#### 2. `Palette.GetPlayerBond()` devuelve 0

Hay un `TODO` explícito. Ahora existe `BondActivityManager` con el registro de IBondables.
La paleta necesita conocer el bond del contexto activo (p.ej., la cría que se está cuidando).
Solución: pasar el IBondable relevante al abrir la paleta (`Open(config, ctx, IBondable target)`)
o leer el bond directamente del `user` si implementa IBondable.

#### 3. `Goluis.resistanceBuilt` no está conectado a `PlayerStats.physicalResistance`

`resistanceBuilt` (0–1) se acumula en Goluis pero `PlayerStats` no lo lee.
Propuesta: en `CompanionBase.OnPlayerLeft()` o en un tick periódico,
`playerStats.physicalResistance = 1f + goluis.resistanceBuilt`.

#### 4. `CameraManager.RequestRobbery` no está conectado a los hitos de las crías

`setup-camera-system.md` lo lista como pendiente explícito.
`Animal.firstNestExit` y `Animal.firstSolidEaten` ya existen (añadidos en la Fase 6).
Cuando esos booleans cambien a `true`, deberían disparar un robo de tipo `OrbitTarget`
con la cría como target, para que la cámara celebre el momento.

Propuesta mínima: en `PostNatalManager.Update()`, cuando se detecte el cambio →
`CameraManager.Instance?.RequestRobbery(new CameraRobbery { type = RobberyType.OrbitTarget, orbitTarget = cub.transform, holdDuration = 3f })`.

#### 5. `ScreenEffects.cs` — placeholder

`BlackoutPulse` en `CameraManager` es solo un `Debug.Log`.
Requiere un componente separado con overlay de pantalla completa (URP o canvas de UI).
No bloqueante ahora, pero necesario antes de testing de jugabilidad.

---

### Alineaciones limpias (encajan sin cambios)

- **`Asana.ShortcutUnlocked` ↔ `PaletteElementData.isDirectUnlocked`**: cuando el jugador
  practica una asana 10 veces, `ShortcutUnlocked` es `true` → marcar `isDirectUnlocked = true`
  en el `PaletteElementData` correspondiente → el modo Hybrid la activa directamente.
  Un sistema de progresión puede sincronizar ambos automáticamente.

- **`Goluis.yoga_skepticism` → `Asana.isUnlocked`**: el anchor `yoga_skepticism` es el gate narrativo
  para las asanas. Mientras sea fuertemente negativo (antes de que el arco avance), las asanas no
  se desbloquean. El sistema ya tiene `isUnlocked` en `Asana` — solo necesita que algo lo fije.
  Propuesta: un `GoluisArcWatcher` que observe el anchor y, al cruzar el umbral (`> -0.4`),
  llame al desbloqueador de asanas.

- **BondActivity + Palette cuidado de crías**: la paleta de cuidado en modo Direct es exactamente
  la UI de `BondActivities` para animales. Al pulsar "Presencia tranquila" en la paleta →
  llamar `BondActivityManager.Practice("Presencia tranquila")`. La actividad ya sabe a qué IBondable
  apuntar (por `targetId`). El vínculo se construye sin lógica extra.

- **`CameraZoneTrigger` + `BondActivityManager.SetContextTag`**: al entrar al área de crías,
  el trigger ya llama a `CameraManager.OnEnterCubArea()`. Añadir en el mismo trigger →
  `bondActivityManager.SetContextTag("cub_area")` → activa pasivamente actividades con ese trigger.
  Un solo evento de zona alimenta dos sistemas a la vez.

- **`PlayerStats.observationRadius` + `Panterilia`**: ya funciona. Panterilia suma el bonus al
  entrar y lo quita al salir. A futuro, las BondActivities pasivas que requieran un radio mínimo
  pueden consultarlo directamente desde `PlayerStats`.

---

### Nuevas propuestas emergentes de este análisis

#### Pipeline de zona completo
Un trigger de zona puede activar simultáneamente:
1. `CameraManager` → switch de perspectiva / robo
2. `BondActivityManager` → context tags → prácticas pasivas
3. Otros sistemas futuros (audio, iluminación)

Un componente `ZoneActivator` que agregue estas llamadas sería más limpio que
tener todo en `CameraZoneTrigger`. Propuesta de bajo impacto, alta utilidad.

#### Satisfacción como desbloqueador de la paleta artística de cámara
`docs/camera-system.md` propone paletas visuales desbloqueables por Satisfacción
(Sueño, Estrés, Alegría, Normal). Estas paletas son exactamente elementos de una
`Palette` en modo `Direct` con `PaletteElementData` por cada modo visual.
`PlayerStats.satisfactionCapacity` puede funcionar como umbral de desbloqueo:
a mayor capacidad (que crece con Gohageneis), más paletas artísticas disponibles.

#### `WorldBondable` como IBondable para la esterilla
La esterilla de yoga es el ejemplo más natural. Añadir `WorldBondable` al prefab de la esterilla,
asignar `bondableId = "yoga_mat"`. Una `BondActivity "Presencia con la esterilla"` con
`passiveTrigger: "mat_idle"` se activa sola cuando el jugador está sobre ella sin hacer nada.
El bond con la esterilla crece — coherente con el alma contemplativa del juego.

---

### Notas de rendimiento

| Punto | Evaluación |
|---|---|
| `CompanionBase.Update()` — `Vector3.Distance` cada frame | Bajo costo. Aceptable con N < 10 compañeros activos |
| `WorldBondable.Update()` — distancia cada frame | Si hay muchos objetos WorldBondable (> 20), migrar a OverlapSphere con capas o check cada 0.1s |
| `BondActivityManager.TickPassiveActivities()` | Ya tiene guard `if (_activeContextTags.Count == 0) return` — bien |
| `AsanaDetector.CheckForMatch()` | Solo se llama al presionar posición, N de asanas pequeño — negligible |
| `CameraManager.ApplyStatEffects()` — `Camera.main.fieldOfView` cada frame | Debería cachear `Camera.main` en `Awake`. Campo menor pero buena práctica |

---

## Ideas / Backlog sin prioridad

- Definir lista completa de posiciones corporales (los "botones") que componen cada asana y ejercicio
- Decidir distribución visual de la tabla periódica en pantalla
- Explorar si la mecánica de asanas tiene feedback visual/sonoro mientras se ejecuta
- Evaluar rendimiento del cálculo de espacio libre para posturas (ver decisión de diseño abajo)
- Definir lista estándar de `passiveTriggers` (tags de contexto) y qué sistemas los emiten
- Decidir si el trauma de BondActivity se muestra explícitamente al jugador o solo como "actividad bloqueada"
- Conectar arco de Goluis con desbloqueo de asanas (`GoluisArcWatcher` o lógica en `LevelScript`)
- Implementar `ScreenEffects.cs` para efectos visuales reales de cámara (requiere URP)
