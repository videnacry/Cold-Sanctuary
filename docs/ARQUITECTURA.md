# Arquitectura de Cold Sanctuary

Mapa para **no re-explorar el código** en cada sesión. Leer solo la sección que haga falta.
Para ubicar una **clase concreta**, no leas este archivo entero: `grep -n "NombreClase" docs/CODEMAP.md`
(índice generado de todas las clases, verificado por CI).

## Cómo encontrar algo rápido

| Busco… | Haz esto |
|---|---|
| Dónde está una clase | `grep -n "Clase" docs/CODEMAP.md` |
| Quién usa una clase | `grep -rln "Clase" Assets --include=*.cs` |
| Una regla de diseño | `grep -n "palabra" docs/manual-de-diseno.md` (reglas con id: W1, J18…) |
| Si una tarea está pendiente | `grep -n "palabra" docs/checklist.md` |
| Qué doc trata un tema | [`docs/README.md`](README.md) (índice de docs) |
| Un menú del Editor | `grep -rn 'MenuItem("Tools/Cold Sanctuary' Assets/Editor` |
| Qué cambió y cuándo | [`CHANGELOG.md`](../CHANGELOG.md) y `git log --oneline -- <ruta>` |

**Archivos grandes (no leerlos enteros, usar `grep`/rangos):** `docs/checklist.md` (~230 KB),
`docs/manual-de-diseno.md` (~110 KB), `docs/CODEMAP.md` (~105 KB), `DEVLOG.md` (~100 KB),
`Assets/Editor/SampleSceneBuilder*.cs` y los `CivicBuilder*.cs` (miles de líneas).

## Carpetas (2026-11-18: 972 `.cs`, ~150 000 líneas)

| Carpeta | Archivos | Líneas |
|---|---|---|
| `Assets/Editor` | 116 | 36800 |
| `Assets/Scripts/World` | 137 | 22974 |
| `Assets/Scripts/Tests` | 84 | 16751 |
| `Assets/Scripts/Microcosmos` | 85 | 11280 |
| `Assets/Scripts/Progression` | 31 | 8286 |
| `Assets/Animals` | 81 | 6609 |
| `Assets/Scripts/*.cs` (raíz) | 59 | 3536 |
| `Assets/Scripts/UI` | 42 | 3520 |
| `Assets/Scripts/Kitchen` | 25 | 3510 |
| `Assets/Scripts/Farming` | 16 | 3014 |
| `Assets/Scripts/Meditation` | 27 | 2761 |
| `Assets/Scripts/Avatar` | 14 | 2742 |
| `Assets/Scripts/Control` | 23 | 2517 |
| `Assets/Scripts/Soul` | 30 | 1955 |
| `Assets/Scripts/Body` | 10 | 1877 |
| `Assets/Scripts/Activities` | 8 | 1732 |
| `Assets/Scripts/Asana` | 17 | 1713 |
| `Assets/Scripts/Transformation` | 16 | 1640 |
| `Assets/Scripts/Player` | 7 | 1479 |
| `Assets/Scripts/Mind` | 10 | 1445 |
| `Assets/Scripts/AtomicPlant` | 18 | 1212 |
| `Assets/Scripts/Combat` | 6 | 1163 |
| `Assets/Scripts/Economy` | 10 | 1091 |
| `Assets/Scripts/Social` | 5 | 956 |
| `Assets/Scripts/Dialogue` | 6 | 893 |
| `Assets/Scripts/Virtualization` | 11 | 853 |
| `Assets/Scripts/Composition` | 6 | 737 |
| `Assets/Scripts/Mission` | 5 | 733 |
| `Assets/Scripts/Camera` | 5 | 722 |
| `Assets/Scripts/Companion` | 10 | 709 |
| `Assets/Scripts/LifeCycle` | 3 | 556 |
| `Assets/Scripts/MobWorld` | 7 | 551 |
| `Assets/Scripts/PostNatal` | 4 | 515 |
| `Assets/Scripts/Compassion` | 9 | 490 |
| `Assets/Scripts/Bond` | 3 | 414 |
| `Assets/Scripts/Time` | 6 | 383 |
| `Assets/Scripts/LifeStage` | 4 | 378 |
| `Assets/Scripts/Chemistry` | 3 | 378 |
| `Assets/Scripts/Prologue` | 6 | 302 |
| `Assets/Scripts/Clothing` | 2 | 284 |
| `Assets/Scripts/Emotion` | 3 | 250 |
| `Assets/Scripts/Interaction` | 2 | 200 |

- `Assets/Editor/` = **constructores de escenas** (`SampleSceneBuilder*` = mesocosmos/Santuario 1;
  `MicrocosmosSceneBuilder`, `MobWorldSceneBuilder`, `CivicBuilder*` = pueblos de los microcosmos;
  `InicioSceneBuilder` = pantalla de inicio) + herramientas (`HeadlessVerify`, `HeadlessLive`,
  `AnimalPrefabGenerator`). Todo bajo el menú `Tools/Cold Sanctuary/…`.
- `Assets/Scripts/World/` = santuario: edificios-árbol, pisos/áreas, jornada y reparto de tareas,
  alfombras, fauna en grupo, Magnate.
- `Assets/Scripts/Tests/` = pruebas en escena (`TestProbe`/`TestRunner`, grep `[TEST]`). Solo corren a petición.
- `tools/` = scripts de shell: `sync-to-unity.sh`, `verify-compile.sh`, `run-live-test.sh`,
  `build-perf.sh`, `check-personajes.sh`, `gen_codemap.py`.

## Mapa de sistemas

| Sistema | Archivos clave | Estado |
|---|---|---|
| Animales | `Assets/Animals/Animal.cs` (concreta) + componentes (`Forager`/`ThreatResponder`/`Locomotion`/`SpeciesBody`/`EstrusState`/`Reproduction`/`SicknessState`) + catálogos + clases mínimas por especie (~22) | Funciona. **`Carnivore`/`Herbivore` BORRADAS** (#126): todas las especies heredan de `Animal`; qué come = data (`Forager.ConfigureForSpecies`). **Fauna sabana Pleistoceno** (era del fuego, a escala insecto): +Termite/DungBeetle/Antlion/Mantis/Grasshopper/Meerkat (skin suricata) — ver `docs/microcosmos-eras-and-observation.md`. **Modelo EMERGENTE** (ver `docs/capabilities-and-embodiment.md`): temperamento HISTÓRICO (confianza-por-uso `spellConfidence`→agresividad efectiva; `canHitAndRun` disuelto), `Assess` **gateado por sentidos**, eje de **armamento ⟂ masa**, gate de **doble vía** (`Anima.CanUse` = anatomía ∨ `Grimoire`). **Depredación por stats** (`Predation`, +`armament`, −`sickness`). **Ciclo de vida COMPLETO** (insectos: larva/pupa/ninfa vía `Metamorphosis`, pelotero come excremento, ver `docs/insect-biology-audit.md`): nacer (reproducción runtime `Reproduction`, gateada por celo `EstrusState`) · crecer (`LifeStage`) · **enfermar** (`SicknessState`→presa fácil) · **inanición** (`Animal.Starve`→enfermedad→muerte) · **dormir** (`SleepCycle`: sueño día/noche por el reloj `Clock`, despierta ante amenazas) · morir (vejez/comido/daño/asfixia). **Mundo VIVO** (#159): `AiBrain.useVolition`+`Reproduction.Enabled` = ON. **Especies de las eras** (escorpión, solífugo, cochinilla, caracol, babosa, cigarra, luciérnaga, tijereta, cucaracha, cangrejo fantasma, pulga de las nieves, ácaro de terciopelo, lombriz): conducta por stats con `PassiveDefence` (bola/concha), `MatingSignal` (destellos/canto), `BroodRide` + deseo `brood` (cría a lomos/nido), hermafroditas, `SidewaysGait` — ver `docs/era-species-behavior.md`. Ver `docs/stats-as-truth.md` §2, `docs/environmental-navigation.md` |
| Navegación ambiental (stigmergy) | `Assets/Scripts/World/TraceField.cs` + `Assets/Scripts/Microcosmos/` (`ImpulseController`/`Home`/`Scent`/`ThreatScanner`) | `TraceField` = **rejilla de rastros** (feromonas/stigmergy): depósito O(1), gradiente O(9), decaimiento perezoso, `volumetric` para mar/aire. Los hechizos dejan rastro (`SpellBase.byproduct`), el celo/enfermedad depositan su canal, el `Wander` deriva por olor (contrafreeloading) y hacia adelante (alternación). Diseño N0–N9 en `docs/environmental-navigation.md` |
| Testing (in-scene) | `Assets/Scripts/TestProbe.cs`/`TestRunner.cs`/`ITestUnit.cs` + `*Test`/`*Auto` + `EcosystemObservation` | **`TestProbe`** (PASS/FAIL por consola, grep `[TEST]`) orquestado por **`TestRunner`** (grupos en serie). Unidades: `TraceFieldTest`/`FaunaChecks`/`EmergenceAuto`/`GrimoireTest`/`EstrusTest`/`LifeCycleAuto`/`SicknessTest`/`StarvationTest`/`SleepTest`. **Misión-observación** `EcosystemObservation` (HUD + log `[ECO]` + alertas de balance). Reusable por misiones WASD. Ver `docs/testing-checklist.md` §29–§40 |
| Post-natal | `Assets/Scripts/PostNatal/` | Funciona; enums `nestType/fatherRole/weaningType` decorativos (no leídos) |
| Etapas de vida | `Assets/Scripts/LifeStage/` | Completo, buena composición |
| Familia/genética | `Family.cs`, `Sex.cs`, `FamilyGenerator.cs` | Completo |
| Tiempo | `Assets/Scripts/Time/Time.cs`, `TimeTest.cs` | Funciona; `TimeTest` es debug |
| Jugador | `Assets/Scripts/Player/PlayerController.cs`, `PlayerStats.cs` | Activo y cableado. `PlayerCtrl` retirado 2026-07-09 |
| Nave/Entorno | `ShipCtrl.cs`, `SlideDoor.cs`, `PullDoor.cs`, `DrivePreparation.cs` | Funciona (bugs `ShipCtrl`/`PullDoor` resueltos 2026-10-12) |
| Cámara | `Assets/Scripts/Camera/` | Funciona (robberies, FOV/shake por stress/fatiga). **`ScreenEffects`** (nuevo): tintes de pantalla por estado (sueño=párpados / fatiga=gris / estrés=viñeta roja / satisfacción=cálido) + `Fade` (completa el blackout de `CameraManager`). 2º actuador de emoción (junto a la postura del `CreatureRig`). Ver `emotion-model.md` |
| Generación | `IFactory.cs`, `BirdBehavior.cs`, `Respawn.cs` | Funciona. `Generator.cs` (spawner por área, scene-wireable) sin invocadores en código |
| Mundo/áreas | `Assets/Scripts/World/`, `Mission/` | Funciona; `AreaClear` incompleto (falta `KitchenCombatManager`) |
| Compañeros/Bond | `Assets/Scripts/Companion/`, `Bond/` | Funciona; `BondActivityManager` sin cablear a UI. `NPCBase` pendiente |
| Diálogo | `Assets/Scripts/Dialogue/` | Completo y cableado |
| Asanas | `Assets/Scripts/Asana/` | Funciona; conectado a `PlayerStats`. **Yoga por hechizos-postura** (`YogaSpells.cs`): `PostureSpell` (1 bodyPart), `AsanaPoseSpell` (asana = varias partes), **`SunSalutation`** (Saludo al Sol, 12 asanas) → consola `yoga`; parte ausente → "no tengo X disponible". Ver `docs/typed-spells-and-queues.md` §4.1 |
| Combate | `Assets/Scripts/Combat/` | **Implementado; jugador cableado.** `NPCCombatBehavior` sin cablear |
| Economía | `Assets/Scripts/Economy/` | **Implementado; núcleo jugador cableado.** `NPCEconomy`/`AreaVendor` inertes. **+ recursos de santuario** (`SanctuaryResources`/`AreaProducer` + HUD; enum ampliado con **Elements/Energy**, docs world-topology §4/§7). **+ trabajo de descomposición**: `DecompositionJob` (lote→economía mayoritaria + `workerCut` de paga; energía gateada por física) y `DecompositionMinigame` (jornada de 3 fases identificar/romper/clasificar, prototipo OnGUI). Ver `magic-metabolism §14/§17`. Sandbox **`Descomposicion_AUTO`** (PR #54) |
| Química (tabla periódica) | `Assets/Scripts/Chemistry/` | Implementado y cableado (~55 elementos) |
| Cocina (miniaturización) | (retirada) | La cocina legacy (`KitchenEntrance`/`KitchenScaleController`) se **borró** 2026-07-23. La entrada migró al trigger universal `VirtualizationMachine` (Meditation) + `RealityShiftController` (miniaturización genérica por área) + `MobWorldLoader` (mundo mob en escena). Ver fila **Meditación / Microcosmos** |
| Ropa/crafting + Composición | `Assets/Scripts/Clothing/`, `Assets/Scripts/Composition/` | `ClothingRecipe`/`ClothingCraftingArea` (crafting, aún no entrega item). **`CharacterComposition`+`CompositionPart`+`StatBonus`** (fase 2): partes slotables (adornos/ropa/miembros) → visual + **delta de stats GESTIONADO** sobre la constitución (no pisa evolución/transform), **modulado por la vitalidad del huésped** + **injerto progresivo** (`adaptSpeed`); la ropa suma a `Anima.armadura` (que `Predation` lee). `ClothingSlot` ampliado (Hair/Eyebrows/Eyes). Fase 3: `BodyPartStats`, miembros como assets, base desde `Humores`/`Chemistry`. Ver `docs/stats-as-truth.md` §5 |
| UI (FollowingArrays/Palette/Hologram) | `Assets/Scripts/UI/` | Complejo; `MaterializationExecutor` inalcanzable (faltan evaluadores). **HUDs OnGUI**: `AnimaStatusHUD` (drives/actividad/elementos) + **`AnimaThoughtHUD`** (pensamiento del ser que maneja el jugador — `Mind.lastThought`+intensidad, ~10 min) + **`AnimaNameplate`** (nombres sobre las animas, para dirigir hechizos por teclado). Ver `docs/microcosmos-dungbeetle-level.md` §5, `docs/typed-spells-and-queues.md` |
| Interacción | `Assets/Scripts/Interaction/` | Funciona; cableado |
| Meditación / Microcosmos | `Assets/Scripts/Meditation/` (23 archivos) | Implementado. `VirtualizationMachine` (trigger universal de misiones mob), `MeditationSession`, `MeditationMissionBase` + misiones (Healing/Protection/Channel/AsanaFormation/PostureVisualization/RootInquiry), arquetipos de mob, `RealityShiftController`, `LotusMeditationAbility`. Ver `docs/magic-plane-and-meditation.md` |
| Mundo mob | `Assets/Scripts/MobWorld/` (5 archivos) | Implementado. `MobResident`, `MobWorldDirector`, `WorldExitPortal` (ex-`YogaPortal`), `MobWorldLoader`, `MobSpawnPoint`; builder en `Assets/Editor/MobWorldSceneBuilder.cs`. Ver `docs/mob-world-architecture.md` |
| Avatares (Microcosmos) | `Assets/Scripts/Avatar/` | `SurfaceWalker`, `AvatarController`, `RobotAvatar` (enum `AvatarLocomotion`: Ground/Climb/Flight) + **`CreatureRig`** (parte lógica `BodyPart`→hueso; auto `HumanBodyBones` humanoide / manual insecto — **fuente única** que usan yoga/emoción). Ver `docs/magic-plane-and-meditation.md` §4, `docs/stats-as-truth.md` |
| Emoción (deep-sim) | `Assets/Scripts/Emotion/` | `EmotionExpression` (conductor: `Humores`→circumplex valencia/activación + `Jolt` + cualidades **Laban**), `BodyPartReactor` (**orquesta** de partes; traducción entre especies orejas↔brazos↔antenas↔alas), `EmotionReader` (**legibilidad**: leer la orquesta ajena, gradúa por `perception`). Aplica a **toda `Anima`**; motor de **tells de reencarnación**. Aptitudes `afabilidad`/`sensibilidad`. Ver `docs/emotion-model.md` |
| Progresión / alma | `Assets/Scripts/Progression/` | `CharacterLevel` (**margas del alma**: Stats/Yoga/Vínculos, tracks independientes) + `DerivedStats` (aptitudes→**puntos del alma**: vida/energía/maná/defensa/poder) + `SoulMarga`. Aptitudes universales vía `IAptitudes`. Ver `docs/creature-stats.md` §Progresión |
| Farming (juego no-violento) | `Assets/Scripts/Farming/` | `PlayableCreature`/`PlayController` (bajar tensión jugando → recursos/XP/items; gateado por bond, puede dañar), `FarmingSandboxItems`, `Climbable`/`PlayerClimber` (trepar). Ver `docs/world-topology-and-planes.md` §4.1 |
| Mente (pilar) | `Assets/Scripts/Mind/` | `Mind` (piensa por tono/frases; **expone `lastThought`+intensidad** para la UI; **`SenseSurroundings`**: el campo de pensamientos **percibe animas cercanas + necesidades** → tiñe humores → tono/valencia del pensamiento), `Humores`, `ElementalTone`, `MindPhrase`/`PhraseLibrary`+`PhrasePools`, `ThoughtField` (zona social/semántica), `PhraseDistribution`. Ver `docs/anima-architecture.md` §11, `docs/spell-genesis-and-sanctuary-progression.md` §7-8 |
| Control / posesión | `Assets/Scripts/Control/` | `AnimaController` cede el mando al `IBrain` de mayor relevancia; `AiBrain` (IA) vs `PlayerBrain` (input, mueve el cuerpo); `PlayerCore` (input persistente + cambio de cuerpo); `FollowBrain` (seguir); `PossessionSpell` (posee en runtime); `HelpRequest`/`HelpResponder` (petición sí/no → alma compartida). **Control por necesidad** (`AcuteStressResponse`, `Assets/Scripts/Soul/`): el apremio (estrés/hambre/alerta) sube la relevancia de la IA → al cruzar el umbral **el cuerpo toma el mando** con **adrenalina** (`Anima.physicalBoost`) + **niebla cognitiva** (`Anima.mindStatusCalculationFog`). **SIEMPRE activo** (fauna+compuestas+jugador) — el jugador nunca tiene control total. **Hechizos por teclado + colas tipo Sims**: `SpellConsole` (escribe `walkTo({destiny:"Marilia"})`), `ActionQueue` (carril `own` prioritario del cuerpo + carril `player` editable), `QueuedSpell`/`WalkToSpell`; consola: `walkTo`/`step`/`bond`/`thought`. **Factory por spec** (`AnymaFactory`+`AnymaSpec`+`AnymaSpecApplier`: props opcionales→random 0.8..2; `CreateMany`). **Modificación (control=alinear)**: `TransformationSpell` (stats) + `BondSpell` (vínculo) + `ThoughtSpell` (mente). **Apariencia por stats** (`StatPhysique`, `Assets/Scripts/Soul/`). Ver `docs/typed-spells-and-queues.md`, `docs/anyma-factory-and-control.md` |
| Transformación (hechizo) | `Assets/Scripts/Transformation/` | `TransformationSpell` (combate de stats: Failed/VisualOnly-farol/Full-cuerpo+stats según potencia vs coste; revert por duración; bidireccional) + `StatProfile`/`TransformPreset`. En `Anima`s reales. Ver `docs/stats-as-truth.md` §4 |
| Magia / metabolismo (reservas) | `Assets/Scripts/Transformation/` (magia) + `Assets/Scripts/Composition/` (metab.) | **Bucle comer→lanzar.** `MagicReserves` (pools por elemento + reserva de **energía** J; `Pay(costs,energy)`), `Grimoire` (hechizos aprendidos; el 1º `Unlock()`ea las pools), `QuarkReserve` (S4: quarks flexibles→cualquier elemento/energía, 1 g≈1,8e24 quarks), `FireSpell` (coste físico real: químico C+H+ignición / masa-energía E=mc²), `MagicAura`. `Metabolism`/`Constitution` (nutriente→elemento→stats; exceso→reservas). Opt-in; sandbox de prueba **`Magia_AUTO`** (PR #54, HUD OnGUI sin `Anima`). Ver `docs/magic-metabolism-progression.md` |
| Cocina (simulación) | `Assets/Scripts/Kitchen/` | Paso A: `DirtArea`/`DirtSpot`/`Cleaner` + paseo `GuidedTour`/`TourStation`. Paso B: `BreakfastCook`→`FoodContainer`→`Eater`. **Restaurante JUGABLE** (`RestaurantKitchen`+`CookingStation`+`CookingRecipe`): no se come, se **cocina para vender** → misión = llenar el **stock** de cada platillo (huevos revueltos/ensalada/avena) **para todos los personajes**; el gestor **reparte por equilibrio** (asigna a cada anima el platillo más necesitado; varias cocinan a la vez); el jugador **avanza cada paso con E** (HUD) o por teclado (`step`), NPCs auto; al cumplir → **XP (marga Stats) + una porción de cada platillo al inventario**. **22 platillos** (6 base + 16 "bomba nutricional" por país: dal/milpa/injera/bibimbap/mezze/curry/tajín/gado-gado/feijoada/pho/mercimek/congee/… en `CookingRecipe.All`). **Sala física con primitivas etiquetadas** (`BuildRestaurantRoom`: nevera/planchas/ollas/bols/mesa + ingredientes) → cocinar escribiendo `walkTo({destiny:"Nevera"})`→`step`. **Cierra de noche** (`AreaHours` + `TimeController.IsDay`). Sandbox `Restaurante_AUTO`. Ver `docs/kitchen-simulation.md`, `docs/typed-spells-and-queues.md`, `docs/testing-recent-features.md` |
| Virtualización (interacción) | `Assets/Scripts/Virtualization/` | Motor genérico (todas las áreas): `VirtualPointer` (**mira fija al centro**; ratón/touch = su cursor), `HeadLook` (giro de cabeza restringido; `.Active` → `PlayerController` cede el look; el cambio 1ª/3ª lo hacen `CameraManager`/`AutoCameraZone`), `StationPart` (parte manipulable, `.timed` opcional), `VirtualTask` (base) → `ProductionOrder` (receta→cuota; `requiresTools`) y `StockingTask` (abastecer: cajas→estantes), `TypingChallenge` (acción temporizada acelerada tecleando; `.Active` congela cámara/mira), **dispatch**: `RepairTicket`/`ServiceHub`/`Toolbox` (reparación cruza-áreas: ticket→tomar herramientas→ir→reparar→devolver). Ver `docs/kitchen-simulation.md` §3b/§4b, `docs/forge-simulation.md` §5 |
| Planta atómica | `Assets/Scripts/AtomicPlant/` + `Assets/Editor/SampleSceneBuilder.AtomicPlant.cs`/`PhysicsSceneBuilder.cs` | Reactor de investigación TRIGA elevado (`AtomicPlant_Area`, «Planta Atómica»): `ResearchReactor` (cinética puntual, barras, temperatura, SCRAM, Cherenkov) + consola, conejo neumático/celda caliente (Mo-99→Tc-99m para el hospital, activación), protección radiológica (dosímetro, pórtico). Microcosmos `Microcosmos_PlantaAtomica`: Newton (órbita), Curie (electrómetro), Einstein (fisión). Ver `docs/atomic-plant.md` |
| Prólogo | `Assets/Scripts/Prologue/` | `PrologueSequence` (guion por beats), `PlaneMessenger` (avisos Mesocosmos↔Micro), `CarryToRefuge`/`WeakOne` (misión de cuidado: llevar débiles al refugio; semilla de cría). Entrada/salida del Micro = `VirtualizationMachine`/`WorldExitPortal` (ya existían). Ver `docs/area-progression.md` "Apertura" |
| Herramientas Editor | `Assets/Editor/` | `SampleSceneBuilder` cablea el **mesocosmos** (Santuario 1). El **microcosmos son escenas propias** (hermanas): `MicrocosmosSceneBuilder` (Scene1/Ambrosio, el alba) y `MobWorldSceneBuilder` (Mesopotamia, ciudad-insecto) — cada una su `.unity`; "Build Sample Scene Blockout" las regenera. `AnimalPrefabGenerator` (prefabs desde `.fbx`). **Inicio** (`InicioSceneBuilder`, índice 0 de Build Settings): una casilla «Kushal iniciado» (vale para el botón «Entrar al Santuario» y para la lista de escenas por grupos, `StartScreen`); los microcosmos y virtuales se abren sobre el Santuario, para que siempre esté Kushal; en el juego F10 o el botón de pausa vuelven a Inicio sin salir del Play (`SceneLauncher`) |
| Debug | `Test.cs` | Sin uso en flujo principal |


## Abstracciones principales

- `Anima` (**antes `LivingEntity`**, renombrada 2026-07-28) — **clase única de todo ser** (animado o inanimado-despertable; ver [`docs/anima-architecture.md`](anima-architecture.md)): drives (`stress`, `trauma`, `fatReserves`, `aware`), bonds, hooks abstractos (`RespondToHunger`, `RespondToThreat`, `EvaluateThreat`), y **hogar de las 12 aptitudes** (implementa `IAptitudes`). `Animal` y `PlayerStats` heredan de ella; los compañeros son **composición sobre `Anima`** (+ pilares Mente/Bond; `SimpleAnima` y `CompanionBase` **retiradas** — todo es `Anima`). El **auto-add** de componentes opcionales (p. ej. `AcuteStressResponse`) vive en `Animal.Init` (vía de la fauna); el jugador/compañeros se montan en su propio setup (añadir ahí si se quiere el mismo opt-in).
- `IAptitudes`: las **12 aptitudes universales** (agility/perception/strength/bodyMass/adaptability/composure/endurance/reasoning/memory/creativity/sociability/discipline). Implementan `Anima` (y por herencia `Animal`/`CompanionBase`) y `PlayerStats` (mapeo parcial). `DerivedStats` deriva de ellas los **puntos del alma**.
- `CharacterLevel` + `SoulMarga`: progresión por **margas del alma** (tracks independientes: Stats/Yoga/Vínculos) → suben los puntos del alma. Ver `docs/creature-stats.md` §Progresión.
- `IMind`: stats mentales. `PlayerStats` implementa `IMind` completa. (`IMindSimple`/`CompanionBase` **eliminadas** — los compañeros usan el pilar `Mind` por composición sobre `Anima`.)
- `IBody`: stats físicas por extremidad + estrés postural (sistema de asanas). Implementa: `PlayerStats`.
- `IBondable`: vínculo con el jugador y efecto por proximidad. Implementa: `CompanionBase`, `WorldBondable`.
- Jerarquía animal: `Anima` → `Animal` (concreta) → `WolfBehavior`/`BunnyBehavior`/`BearBehaviour`/… (una clase mínima por especie, ~22: `SpeciesArchetype` + `Start` + `ConfigureThreat`; `Carnivore`/`Herbivore` borradas). Todo lo demás es data (catálogos) + componentes. Y `Anima` → `CompanionBase` → `Goluis`/`Panterilia`/…
- `NPCBase` **descartado** como clase intermedia (2026-07-25): se va a **una sola `Anima`** + pilares/mente opcionales. `IMindSimple` eliminado (2026-07-28).
- `LifeStage` (abstracta) → `Childhood`/`Adolescence`/`Adulthood`.
- **Control intercambiable**: `AnimaController` + `IBrain` (`AiBrain`/`PlayerBrain`) — el ser lo conduce el
  cerebro de mayor **relevancia**; el jugador es "solo un input" (un `PlayerBrain`) que la **posesión**
  (`PossessionSpell`, power/range) inyecta en runtime. Ver `docs/anima-architecture.md` §11.5.

