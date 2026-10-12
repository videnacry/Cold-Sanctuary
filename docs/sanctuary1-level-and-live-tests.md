# Santuario 1 (Nivel 1 jugable) + pruebas en vivo — estado y plan

> Revisión (2026-10-12) de lo que YA tenemos para el Nivel 1 y las pruebas en vivo, qué falta, y el plan de
> **cableado del onboarding**. Fuente: `Assets/Editor/SampleSceneBuilder.cs` (blockout del mesocosmos),
> `MicrocosmosSceneBuilder.cs`, `Assets/Scripts/Prologue/`, `Virtualization/`, `Kitchen/`, `PostNatal/`.

---

## 1. Pruebas en vivo (sandboxes) — QUÉ TENEMOS
Todas se montan con `Tools/Cold Sanctuary/Build Sample Scene Blockout` (bajo `LevelDressing_AUTO`) y corren al Play:
- **Ecosistema vivo**: `BuildWildlifePopulation` (oso polar/orca/foca/ballena/pingüino por `FamilyGenerator`) +
  `EcosystemObservation` (HUD/log `[ECO]` de balance). **Caza/depredación emergente** ocurre aquí.
- **Vida familiar** (NUEVO 2026-10-12, `BuildFamilyLifeTest`): **lobos (familia + tíos) con carne puesta** y
  **conejos con pasto**, aislados → se observa SOLO pareja/padres/crías (+ el deseo `nurse`), sin que cacen.
- **Restaurante** jugable (`Restaurante_AUTO`): cocinar→stock→XP+inventario, cierre nocturno.
- **Magnate** (`BuildMagnate`): maestra-guardiana (pacifica + enseña yoga al rezagado).
- **Mente** (`BuildMindSandbox`), **posesión/control** (`BuildPossessionSandbox`), **farming** (`BuildFarmingSandbox`),
  **cocina paso A** (`BuildKitchenSandbox`: suciedad/limpieza), **virtualización** (`BuildVirtualizationSandbox`:
  puntero/estaciones/receta), **desgaste** (`BuildExertionSandbox`), **prólogo** (`BuildPrologueSandbox`),
  **upa-yoga** (`BuildUpaYogaSandbox`), **efectos de cámara** (`BuildScreenEffectsSandbox`), **orquesta emocional**
  (`BuildEmotionOrchestraSandbox`), **descomposición** (`BuildDecompositionSandbox`), **magia** (`BuildMagicSandbox`),
  **hechizos** (`BuildSpellDemoSandbox`), **locomoción universal** (`BuildWalkUniversalSandbox`), **alma mezcla**
  (`BuildSoulBlendSandbox`), **alma compartida** (`BuildSharedSoulSandbox`).
- **TestRunner_AUTO** (grupos 0–14, `[TEST]`): TraceField/Fauna/Emergence/Grimoire/Estrus/LifeCycle/Sickness/
  Starvation/Sleep + otros. **Test de observación** `ObservationTest`, estados `StateSpellsTest`, río `RiverCurrentTest`.

## 2. Pruebas en vivo — QUÉ FALTA (escenarios dedicados)
Hoy la caza/lucha ocurre **emergente** en el ecosistema, pero no hay **escenarios aislados** para observarlas:
- [ ] **Apareamiento** (una pareja en celo, aislada, con comida → ver cortejo/concepción; hoy `EstrusTest` es unit).
- [ ] **Caza** (1 depredador + presas + comida para la presa → ver acecho/persecución/depredación limpia).
- [ ] **Lucha 1 vs 1** (dos depredadores por un recurso/territorio).
- [ ] **Pack vs 1** y **Pack vs Pack** (manada(s) con `PackAwareness` → ayuda/defensa emergente).
Patrón: como `BuildFamilyLifeTest`, aislados y con comida puesta para que el test sea **solo** de esa interacción.

## 3. Migración del postnatal a DESEOS — estado
- **HECHO (1ª rebanada):** deseo **`nurse`** en `DesireCatalog` — el padre/madre **alimenta a la cría** (lifeStage='c')
  vinculada y hambrienta; compite en la **arena** (vínculo×hambre×afabilidad) y corre por la **cola** (Conjure→SuperSpell).
  Junto a `tend`/`follow`/`mate` ya existentes, la **decisión** de cuidado está en la arena.
- **FALTA:** retirar los **drives directos** de `PostNatalManager` (corutinas `SetDestination` a `cub.HomeOrigin`,
  nursing por intervalos, roles de padre) y expresarlos como deseos (`guardCub`, `fetchCub`, cub-side `stayClose`/`cry`).
  El **calendario** de etapas/destete de `PostNatalManager` puede quedarse (es la agenda, no la conducta momento-a-momento).

## 4. Onboarding del Nivel 1 (Santuario 1) — PLAN DE CABLEADO
Secuencia objetivo (Kushal), con lo que YA existe y lo que falta cablear:
1. **Enfermería**: Linduarda hace reconocimiento → lanza **`análisisClínico`** (superhechizo) ✱FALTA + **`vacunar`** ✱FALTA.
   (Existe: `Linduarda` ficha, `VirtualizationMachine` de entrada al Micro, `SpellConsole`/`SuperSpell` como base.)
2. **1er vistazo al Microcosmos** (sin misión) → vuelve.
3. Linduarda lo manda con **condición: limpieza** → misiones de **limpieza** (`DirtArea`/`DirtSpot`/`Cleaner` ✓) +
   **inventario** (`StockingTask` ✓), **diarias y con prioridad sobre cocinar**, **en equipo** ✱FALTA (gating diario/prioridad/equipo).
   Kushal además hace sus **automisiones** ✱FALTA (área de menú de automisiones).
4. **Recoger muestras** (otra condición) ✱FALTA (misión recolectar).
5. **Sala de yoga**: se le enseña **upa-yoga** (`UpaYogaSession` ✓ / `SunSalutation` ✓). Se le **da de comer** (`Eater`/restaurante ✓).
6. **Resultados** → puede **pasear**, pero es **de noche**: áreas **cerradas** (`AreaHours` ✓) y con **sueño** →
   arranca la **volición** pidiendo dormir y **se duerme aunque el jugador no quiera** (`AcuteStressResponse` toma el
   mando ✓ + `SleepCycle` ✓). *(Falta: el salto temporal al amanecer y el guion que encadena esto.)*
7. **Día 2 — Cocina**: a primera hora, **limpieza+inventario** (diarias/prioritarias/equipo) → luego **Microcosmos de la
   cocina** (misiones) → al terminar, **liberado** para pasear.
8. **Mundo abierto**: Mecánica y Huerto (**también con limpieza+inventario** ✱ por confirmar) y **guardería/cría** ✱FALTA.

**Lo que hay que construir para el Nivel 1 jugable:** los **superhechizos `análisisClínico` y `vacunar`** (Linduarda),
el **guion/`PrologueSequence` que encadena** los pasos 1–8, el **gating de misiones** (diarias, prioridad limpieza>cocina,
equipo), las **automisiones** (área de menú), **recoger muestras**, **salto temporal al amanecer**, y las **misiones de
guardería**. El resto (limpieza/inventario/yoga/comer/cierre-nocturno/volición-sueño/micro) **ya existe**; es cablearlo.

## 5. Fauna — completitud
- **Definidas** (clase + catálogos): mamíferos/marinos (Wolf/Bunny/Fox/Deer/Bear/Malamute/Seal/Whale/Penguin/Orca) +
  insectos sabana/era-fuego (Ant/Aphid/Ladybug/Spider/Cricket/Termite/DungBeetle/Antlion/Mantis/Grasshopper/Meerkat).
- **Modelos 3D**: el compi reportó **13/21 sin `.fbx`**; con el **prefab-cubo por defecto** (`AnimalPrefabGenerator`)
  ya se generan todas como cubos → **jugable sin modelos**. 
- **Falta**: confirmar que la **flora/insectos de hielo** del diseño (plancton→krill→pez ✓ como Swarm/Phytoplankton;
  **árbol carnívoro de hielo + abejas de las nieves** ✱ aún no montados) estén en la escena del Santuario 1; y poblar
  los **niveles del Microcosmos** con la fauna de su era (ambiente ✓ Scene0; revisar las demás).
