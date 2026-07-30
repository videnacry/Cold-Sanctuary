# Checklist — continuar

Tablero para retomar. Última sesión: 2026-07-30. Marca lo que completes.
Contexto de fondo: [`AUDIT-2026-07-09.md`](AUDIT-2026-07-09.md), [`gaps-vs-planteamiento.md`](gaps-vs-planteamiento.md),
[`world-topology-and-planes.md`](world-topology-and-planes.md) (visión del mundo grande / los 3 planos).

## Decisiones abiertas (rápidas)
- [x] **Aptitudes adicionales**: set cerrado — `endurance/reasoning/memory/creativity/sociability/discipline`
      añadidas a `CompanionBase`; `flexibility` → `BodyPartStats` (pendiente de conectar).
- [ ] **Economía circular** (aprobada): cerrar la tabla final residuo→subproducto→área
      (ver [`mission-mode.md`](mission-mode.md)).
- [x] **`Generator.cs`**: revisado 2026-07-23 — **se mantiene**. No es redundante con `FamilyGenerator`
      (uno genera familias, el otro es un spawner por área vía `IFactory`/`Animal.StaticGenerateSquareRange`);
      sin invocadores en código pero es una herramienta scene-wireable útil.
- [x] **Malamute: mascota** (decidido) — fuera de `nestSpecies`; sigue siendo presa potencial. Colocación
      como compañero: pendiente (parte del modelo de personajes/`NPCBase`).

## Aptitudes (creature-stats.md)
- [~] **Bucle de evolución**: *animales hecho* (agilidad←movimiento, percepción←alerta, en `Animal.Restore`
      con `AptitudeEvolution`). **Falta humanoides**: engancharla a tareas/misiones (físico→fuerza/resistencia,
      estudio→razón/memoria, variedad→adaptabilidad, sedentarismo→↓).
- [ ] Conectar aptitudes a mecánicas: `agility`→velocidad/maniobra, `perception`→detección+calidad
      de asana, `strength`→daño/carga, `bodyMass`→física/saciedad, `adaptability`→velocidad de aprendizaje.
- [ ] Unificar con `PlayerStats` (`observationRadius`↔`perception`, `velocity`↔`agility`) y
      `BodyPartStats` (`flex`/`str`). **Ya consolidado en `Anima`** (migración #14); queda mapear
      `observationRadius`/`velocity`↔aptitudes del todo.
- [ ] Modelar el rasgo mental de **Panterilia** (exageración de la realidad / influencia de terceros).
- [ ] Revisar `adaptability` para animales (hoy solo en companions).
- [ ] Conectar `flexibility` a la dimensión `flex` de `BodyPartStats` (se entrena con yoga; universal).
- [ ] **Modificadores de medio (resto)**: afinidades para humanoides (nadar) al llegar `NPCBase`;
      extender `MediumFactor` a `strength`/`endurance`. (Detector `MediumZone` ya hecho.)
- [ ] **Ahogo/asfixia** (solo documentado): daño progresivo por permanecer en medio de baja afinidad.
- [x] **Lógica de agua/tierra**: detector (`MediumZone`) + comportamiento (`Animal.CorrectMedium`:
      acuáticos buscan agua, terrestres salen). Pendiente menor: evitación *proactiva* (desviar `Wander`).
- [ ] **Refinar selección de caza**: `SelectPrey` no evalúa si el cazador puede ganar ni el **poder de la
      presa** → un lobo solo se lanza al oso y muere; y un humano de alta maestría mágica seguiría siendo
      "presa fácil". Ponderar ventaja de masa/manada + un valor de **amenaza/poder** del objetivo (humano
      poderoso → no-presa/cautela/huida; ligar con `EvaluateThreat`). Requiere el sistema de magia/maestría.
- [ ] **Influencia de manada en la caza**: parte del refinamiento — evaluar masa aliada de presa y cazador
      (oso evita lobo con manada; manada grande lo ahuyenta). Dinámico, no multiplicador estático de dieta.
- [ ] **Aura/estatus mágico del humano**: contador de usos destructivos de magia (decae con el tiempo) que
      modula si los animales lo temen (huida/cautela) o lo ven como inspirador (bonds fáciles). Requiere magia.
- [~] **Modelo de personaje unificado** — `NPCBase` **descartado**; ya es la **clase única `Anima`**
      (migración #14). Falta: `ITarget` + población en `Anima` (para que companions/otros NPCs sean presa) y
      el **control aparte** (cerebro enchufable: input o IA) para que todo `Anima` sea jugable/intercambiable.
- [ ] **Territorios + poblaciones**: `Territory` trigger con `residents` (enter/exit, como `MediumZone`/
      `SanctuaryArea`) → escaneos locales (arregla el perf de `SenseThreats`/banco); barreras/conectores para estabilidad.
- [ ] **Montaje de escena** (`SampleSceneBuilder`): crear **nidos/madrigueras primero**, luego poblar
      familias en torno a ellos, con nidos **fuera del alcance de depredadores**. Ver [`refuge-and-adult-behavior.md`](refuge-and-adult-behavior.md).
- [x] **Dietas** revisadas (todos los carnívoros): oso +ciervo/zorro/malamute/**humano**; lobo +zorro/malamute/oso(manada)/humano; zorro/malamute sin cambios.
- [x] **Banco de peces como organismo** (hecho): `FishSchool` es entidad viva (deriva/huye/crece/autoregenera)
      + `ITarget`/`IEdible`; en dietas de oso/zorro; herbívoros marinos lo depletan al pastar. **Pendiente:
      robo/hurto del zorro** (llevarse comida ajena con `ICarrier`; liga con sigilo).
- [ ] **Zorro: robar comida** (hurto de `FoodItem`/presa ajena vía `ICarrier` en vez de cazar de frente); liga con sigilo.
- [ ] **Personajes/mascotas como presa**: `PlayerTarget` hoy = solo el jugador. Que companions/malamute-mascota sean `ITarget` con población objetivo (futuro, `NPCBase`).
- [~] **Comportamiento adulto / territorialidad**: hecho el evitar-depredadores (`SenseThreats`). Falta:
      idle enriquecido (explorar/jugar), separación *espacial* (montaje nidos-primero), refugio/ocultarse
      (árboles/arbustos) y memoria de lugares (solo documentado). Ver [`refuge-and-adult-behavior.md`](refuge-and-adult-behavior.md).

## Aclaraciones → siguiente paso (learning-unlocks.md)
- [ ] Crear un **registro de "aprendidos"** por jugador (elementos, posturas, habilidades) + evento
      `OnLearned(...)`.
- [ ] Hacer que **al aprender** se reconstruya la UI afectada al vuelo (Palette / Hologram / AbilityBar).
- [ ] Sustituir el gating por `unlockLevel` de `CombatAbility` por gating por **"aprendido"**.
- [ ] `CombatAbilityBar.RefreshBar()`: suscribirlo a `OnLearned` + `PeriodicTableManager.OnElementDiscovered`
      (arregla el "no refresco en vivo").
- [x] **Cocina**: resuelto 2026-07-23 — `KitchenEntrance`/`KitchenScaleController` (y su auto-trigger)
      **borrados**; la entrada es ahora una sola ruta vía `VirtualizationMachine` (Microcosmos).

## Modo misión / economía (mission-mode.md)
- [ ] Implementar `KitchenCombatManager`: arranca modo misión, activa mobs, bloquea salida del área,
      reporta a `MissionTracker` y suma a los contadores.
- [ ] **Sistema de contadores por área** + **conscription loop** (obligar a personajes ociosos cuando
      un contador baja del umbral).
- [ ] **Trigger-personaje**: componente en un NPC + prompt `ConfirmationPanel` + bloqueo/desbloqueo
      de la salida del `SanctuaryArea`.
- [ ] Implementar la economía circular (mobs → abono + subproductos → contadores de otras áreas).

## Huérfanos (gaps-vs-planteamiento.md §B) — cuando toque
- [ ] `TeacherNPC` → `Palette.OnFormulaEvaluated`; `Say()` → `DialogueManager.Play`.
- [ ] `BondActivityManager` → UI (`BuildPaletteConfig`/`Practice`); consumir `Goluis.resistanceBuilt`.
- [ ] `BlockSpellEvaluator` (desbloquea `MaterializationExecutor` + `ArrangementPattern`).
- [ ] `NPCCombatBehavior`, `NPCEconomy`/`AreaVendor`, `ClothingCraftingArea` (cerrar loop).

## Bugs abiertos (known-issues.md)
- [ ] `ShipCtrl` `if(1==1)` (83/95); `PullDoor.OnCollissionEnter` (typo); `Respawn` off-by-one;
      `BirdBehavior` altura sin clamp; `ActionPrep` `energyCost` sin límites.

## Mundo grande — los 3 planos (world-topology-and-planes.md)
Escalera de implementación propuesta (2026-07-23). Empezar por **A** (columna vertebral).
- [~] **A — Backbone de recursos + HUD (Mesocosmos).** `SanctuaryResource`/`SanctuaryResources` (ledger),
      `AreaProducer` (pasivo + bonus por personaje asignado), `SanctuaryResourceHUD` (prototipo OnGUI),
      cableado en `SampleSceneBuilder`. **Primera pasada hecha**; falta: asignación real de personajes
      (enganchar a `SanctuaryDirector`), regla de visibilidad multi-santuario en guerra, y sustituir el
      HUD prototipo por UI declarativa.
- [~] **B — Farming-como-juego (tensión).** **MVP + feel de combo hechos (2026-07-23):**
      `PlayableCreature` (tensión = `Anima.stress` o local) + `PlayController` (tecla V);
      al serenarse suelta recursos (`SanctuaryResources`) + monedas; `IInteractable` "dar comida y agua"
      → sacia (`fatReserves`) y descansa. **V2 feel gato/perro hecho:** excitación/combo (descarga
      escala con excitación), **atrapada** (quedarte pegado resetea el combo), reacción de la criatura
      (te mira/se acerca, con correa) + rebote de escala. Sandbox en `SampleSceneBuilder`.
      **V3 hecho (2026-07-23):** tabla de **drops** (`ItemDrop[]` → `Inventory.AddItem`, con consumible y
      artefacto placeholder vía `FarmingSandboxItems`), y **XP → leveling** (`CharacterLevel`: xp/nivel →
      +vida/+maná + vida actual/daño, mostrado en el HUD). **Refinamiento hecho:** el juego es
      **desbloqueable** (gateo `PlayUnlocked` = criada/relajada/vínculo; si no, ley natural) y **puede
      hacer daño** (pérdida de control al excitarse → `CharacterLevel.TakeDamage` si no esquivas).
      **Falta:** leer el vínculo real de `Anima.bonds` (hoy flags placeholder), conmutar de verdad
      a la depredación de `Animal` cuando no está desbloqueada, mecánica de **esquiva** propia,
      generalizar el target de combate (`CurrentTarget` es `IngredientMob`) y enganchar vida/maná al combate.
- [ ] **C — Teletransportador "aeropuerto".** `SanctuaryTeleporter` bidireccional entre santuarios
      (lava↔subterráneo primero); reusa `MobWorldLoader`.
- [ ] **D — Construcción en el tiempo.** `Construction`: progreso consumiendo recursos+tiempo (lento en
      Meso, rápido en Macro); 4 áreas + 1 en obra. Depende de A.
- [ ] **E — Macrocosmos (RTS).** Unidades/roles (peón/héroe), árbol de construcción, cámara 2D-ish,
      IA de guerra, agujero-negro de estructuras. Sistema grande; al final.
- [~] **Pools derivados de aptitudes** (creature-stats.md §Pools derivados). **Hecho (2026-07-24):**
      módulo `DerivedStats` (funciones puras aptitudes→vida/energía/maná/defensa/poder) + `Aptitudes`
      struct; `CharacterLevel` reescrito para derivar los pools (energía nueva, defensa aplicada al daño
      en `TakeDamage`, `SpendEnergy`), lee de cualquier `IAptitudes` (opt-in); HUD muestra energía/def/poder.
      **Trepar hecho (MVP, verificar feel/física):** `Climbable` + `PlayerClimber` (Espacio; altura ∝
      fuerza/peso, velocidad ∝ fuerza·agilidad, coste de energía ∝ peso/fuerza vía `SpendEnergy`).
      **Falta:** unificar del todo la fuente de aptitudes con `NPCBase`.
- [~] **Aptitudes universales** (2026-07-24): **hecho vía interfaz `IAptitudes`** (12 getters) que
      implementan `Anima` (ahora hogar de las 12 — agility/perception activas, resto latentes),
      `CompanionBase` y `PlayerStats` (mapeo parcial). `DerivedStats.From(IAptitudes)` y `CharacterLevel`
      (opt-in `deriveAptitudesFromComponent`) leen cualquier ser vivo uniforme.
- [x] **Migración a clase única `Anima`** — **PR #14 mergeada a master (2026-07-28)**; falta que TÚ compiles.
      Hecho (2026-07-28): (1) **`LivingEntity` renombrada a `Anima`** en todo el código (`Anima.cs`), 34 refs;
      (2) **`CompanionBase : Anima`** — quitadas las 12 aptitudes duplicadas + `stress` (heredados), 3 hooks
      abstractos implementados (stubs), `fatigue`/`mood` propios; (3) **`IMindSimple.cs` borrado** (sin uso);
      (4) **`MigrationDiagnostics`** que vuelca por consola la validación en Play (herencia, aptitudes por
      campo y vía `IAptitudes`, puntos del alma/margas del jugador); (5) **`PlayerStats : Anima`** hecho —
      `stress` y las 12 aptitudes heredadas (retirado el mapeo velocity→Agility; el jugador tiene aptitudes
      reales), `velocity`/`physicalResistance` quedan de movimiento/combate, 3 hooks abstractos (stubs).
      (6) **`WorldCharacter` consolidado (2026-07-28):** las drives `satisfaction/mentalFatigue/sleepiness/
      observationRadius/velocity/physicalResistance` se **movieron a `Anima`** (hogar único; `stress` ya
      estaba); `PlayerStats` las hereda (retiradas sus copias; `playerStats.X` sigue funcionando); y
      `WorldCharacter` lee/escribe el `Anima` del objeto (borrados sus lightweight + el bridge a
      PlayerStats). **Migración ESTRUCTURAL COMPLETA:** todo ser (jugador/compañeros/animales) es `Anima`
      con una sola fuente de stats. *(Nota: los NPC ahora arrancan con `physicalResistance=1` en vez de la
      antigua `strength=0` → puede requerir re-tunear `promotionStrength`.)* **Falta menor:** docs
      históricos que aún dicen "LivingEntity"; el pilar `Mind` (drives complejos) es trabajo aparte.
- [~] **Margas del alma = tracks INDEPENDIENTES** (creature-stats §Progresión). **Hecho (2026-07-24):**
      `SoulMarga` (pool XP+nivel+curva) y `CharacterLevel` con 3 margas (**Stats**/**Yoga**/**Vínculos**);
      los **puntos del alma escalan por TODAS las margas** (`SoulLevels`); **maná gateado por Yoga≥2**;
      HUD; **XP de Yoga cableada** (`MeditationReward.yogaXp` → `GainYogaXp` en misiones de yoga); **XP de
      Stats desde ganancia de aptitudes** (`GainAptitude` → `GainStatsXp`); **XP de Vínculos cableada**
      (`BondActivity.TryPractice` → `GainBondXp`); **base-bump al subir de nivel** (`OnMargaLevelUp` sube
      la base de todas las aptitudes vía `Aptitudes.AddAll`). **Falta:** XP de yoga por **práctica directa**
      (`AsanaQueue.OnLimitReached`, requiere compilación/orphans) y la futura marga de **Hechizos**.
- [~] **Misiones de simulacro que dan aptitudes** — **hecho (2026-07-24):** `SanctuaryMission` tiene
      `rewardAptitude`/`rewardAptitudeAmount`; `MissionTracker` los aplica vía `CharacterLevel.GainAptitude`
      (sube aptitud + alimenta la marga de Stats). Falta poblar valores por misión cuando existan más.
- [ ] **Extender el yoga — orphans** (revisión 2026-07-24, known-issues §Yoga; **requiere compilación**):
      resolver `AsanaEvaluator` (no se instancia) y `AccumulatePostureStress` (0 invocadores), y hacer
      **persistente** la maestría (`Asana.*` son `NonSerialized`). Wiring de gameplay; mejor con Rider.
- [ ] **Maná latente → el yoga desbloquea su barra** — **hecho** en `CharacterLevel.ManaUnlocked`
      (Yoga≥2; solo visibilidad/uso, incrementos = stats+alma). *(marcado completo.)*
- [ ] **Niveles por plano:** Mesocosmos autoritativo (reglas profundas); Macro/Micro pueden abstraer
      nivel+XP si el rendimiento lo exige.
- [ ] **Decisión clave:** modelo de guerra en modo Meso (encarnada recomendada vs tiempo comprimido) — §9.

## Mente emergente (anima-architecture.md §6/§11 · mind-model.md)
Pilar `Mind` como componente enchufable. **MVP + composición hechos (2026-07-24…28).**
- [x] **Mind MVP** (PR #13): `Mind` piensa por `thinkInterval`, elige FRASE por tono elemental
      (aptitudes+humores), la expresa hasta su PODER MENTAL (`Depth`); `Humores` (bioquímica),
      `ElementalTone`, `PhraseLibrary` (4 frases elementales).
- [x] **Clasificación de frases + campos** (PR #15): `PhraseCategory {Vivencia,Asana,Hechizo,Elemental,Deseo}`
      + flags `randomAssignable`/`reusable`; `ThoughtField` (empuja tono / nudge de humores por radio);
      `Mind.PickTone` suma aptitudes+humores+campos+**vivencias propias**.
- [x] **Pools de frases** (PR #15): `PhrasePools` — 14 vivencias FIELES a `creature-stats.md`
      (Goluis→Tierra, Panterilia→Viento, Gohageneis→Agua, Irosene→Fuego) + 6 deseos base; `DealVivencias`
      (reparto al azar respetando flags); `VivenciasOf(source)`; **Ötzi** autorado (histórico, `Historico()`).
- [x] **Propiedad + reparto por modo** (PR #15): `Mind` tiene `identity`/`thoughts`/`thoughtsLocked`;
      `PhraseDistribution` con `NarrativeMode {Estricta, Libre}` — estricta conserva la propiedad; libre
      vuelca los no-bloqueados a un pool público y reparte al azar; **Magnate/históricos bloqueados** quedan
      fuera y conservan los suyos. `Plan()` puro/loggeable + `Distribute()` a los `Mind` reales.
- [ ] **A PROBAR en el editor** (sandbox `MindSandbox_AUTO` de `SampleSceneBuilder`): en Play, cada ánima
      tiende a su tono y dentro del `ThoughtField(Agua)` se inclina a Agua + sube serotonina; consola
      `[Frases]` imprime conteos de pools, biografías por fuente, y el reparto **Estricta vs Libre**
      (Magnate + Ötzi 🔒 conservan lo suyo; compañeros + anónimo reciben del pool).
- [ ] **Siguiente**: controlador intercambiable (jugador-como-input ↔ IA) y **posesión dinámica**
      (insertar instancia/madre en runtime con relevancia); asana/hechizo como frases reales; multi-instancia.
- [ ] **Flag futuro** `absorbsPublic` si algún día un bloqueado debe además recibir del pool (hoy: no recibe).

## Testing — validado en Unity (detalle en `testing-checklist.md` §10–12)
Compila 0 errores; validados migración Anima, progresión, farming, Mind y cocina. Por probar: virtualización/
dispatch (§11/§12).
- [ ] **Pendiente — solo con JUEGO MANUAL**: daño de "Dura" (perder control); **trepar** (mantener Espacio);
      4 esferas de Mesopotamia + `YogaPortal`; **misión de yoga** (→ marga Yoga + maná); `ThoughtField_Agua`;
      velocidad de tiempo; y todo el §12 (Mecánica/Construcción arranque + dispatch del grifo).
- [ ] **Cambio conocido**: NPC arrancan `physicalResistance=1` → posible re-tuning de `promotionStrength`.

## La Cocina — primera simulación (nivel de referencia)
Diseño completo en [`kitchen-simulation.md`](kitchen-simulation.md). Escalera de construcción (§12):
- [~] **★ INTERACCIÓN DE VIRTUALIZACIÓN (el núcleo jugable, kitchen §3b)** — **MOTOR + FEEL + TYPING HECHOS**
      (`Assets/Scripts/Virtualization/`): **`VirtualPointer`** = **mira FIJA en el centro** (no se mueve);
      apuntas girando la **cabeza=cámara** con **`HeadLook`** (I/K/J/L, con **restricciones** de yaw/pitch);
      confirmas con **F** (Espacio=salto); **ratón/touch = su propio cursor**. **Resaltado** de la parte
      apuntada. **`ProductionOrder`** (receta por pasos → producto → **cuota** = sustento). Recetas:
      **cocina** (`VirtualizationSandbox_AUTO`) y **huerto** (`GardenVirtualization_AUTO`). **Mecanografía**:
      **`TypingChallenge`** — el fogón es una acción temporizada que se acelera tecleando (cook/eggs/protein/
      b2…); congela cámara/mira mientras se teclea (`.Active`, gateado también en `PlayerController`).
      **Falta:** enganchar la cuota a la **misión real**; palabras **sobre el objeto** + bancos localizables
      (en/fr) y por compuesto; estaciones licuadora/horno; animaciones; activar `HeadLook` al entrar a estación.
- [x] **A — Paseo + limpieza mancha-a-mancha (Meso)**: limpieza (`DirtArea`/`DirtSpot`/`Cleaner`) + **PASEO**
      (`GuidedTour`/`TourStation`: el anfitrión recorre estaciones enseñando cada área, con el novato de
      alma compartida vía `HelpRequest`). Sandbox `KitchenSandbox_AUTO` + `KitchenOnboarding_AUTO`.
- [~] **B — Loop de desayuno + contenedor**: **MVP hecho** — `BreakfastCook` recorre la cadena
      (nevera→huevos→plancha→revolver→especiar→contenedor) y rellena un `FoodContainer`; `Eater` come del
      contenedor. **Falta:** hacer la cadena **espacial** (caminar a las estaciones con `FollowBrain`).
- [ ] **C — Alimentación por humores**: personajes eligen contenedor por utilidad+humores; comer aplica
      `compuestos` → nudge de humores. *(Introduce `FoodCompound` mínimo — modelo de §8: compuestos→humores→aptitudes.)*
- [ ] **D — Puente Micro/Meso**: mancha del Meso = región del MicroKitchen; minidrones extraen → desaparece (suelo primero).
- [ ] **E — Mundo-insecto vivo**: gusano→formas; conflictos fuerte/débil; misiones; enganchar al Guardián del Fuego.
- [ ] **F/G — Recetas ricas (química) + puente al santuario** (alimentar carnívoros con bonds).
- **Mínimo jugable:** A + B → luego D (la unión Micro/Meso).
- **Próxima área (principiante): el HUERTO** (kitchen-simulation §13) — continúa el microworld (fuego→
  agricultura, La Sembradora) y cierra el bucle con la cocina (produce ingredientes). 2ª simulación.
  Misiones de virtualización DISEÑADAS: [`garden-simulation.md`](garden-simulation.md) §8 (abonar→arar→
  trasplantar→regar→proteger→cosechar) y §9 (**mundo-insecto: misiones de guardián**, proteger las plantas
  de otros insectos).

## Áreas / historia — roadmap (area-progression.md)
Orden alineado con la línea temporal del microworld (una época por área). Ver
[`area-progression.md`](area-progression.md), [`garden-simulation.md`](garden-simulation.md).
- [~] **0. PRÓLOGO — Enfermería** (apertura del juego): a Kushal (viene de fuera) le hacen exámenes+vacuna;
      curiosea → **máquina de avatares** (`VirtualizationMachine`) → **Microcosmos PRE-FUEGO** (plantas
      medicinales, **La Recolectora** — autorada); apoya y **lleva a los débiles a la cueva** (`CarryToRefuge`/
      `WeakOne`); vuelve por la **sala de meditación** (`YogaPortal`, puerta bidireccional ya existente) →
      primer trabajo (Cocina). **Scaffold hecho** (`PrologueSequence`, `PlaneMessenger` mensajes del Mesocosmos,
      `CarryToRefuge`; sandbox `PrologueSandbox_AUTO`). **Falta**: montar la ESCENA real en Unity.
- [~] 1. **Cocina** (Paleolítico/fuego) · 2. **Huerto** (Neolítico/agricultura) — diseñadas.
- [~] **3. CRÍA** (corazón del santuario; **confirmada área 3, tras el Huerto** → Construcción=4, Mecánica=5,
      Enfermería=6, Yoga=7) — [`cria-simulation.md`](cria-simulation.md) + `CriaBeginner_AUTO` (limpiar→
      abastecer→rutina→nido). **Enganche a drives REALES**: `CriaCareTarget` toca `Animal.stress`/`hungry` y
      `GrowBond` (que ya factoriza trauma); el bond **se gana** (estrés alto → rechaza). **La Recolectora**
      (raíz pre-fuego) con cadena de misión (founding-trio §7.4). **Histórico fundacional: El Perro de
      Oberkassel** (real ~14.200 años; cachorro salvado del moquillo por amor → mito del santuario; autorado
      con vivencias POV-perro + cadena de misión, cria-simulation §5). **Falta**: cablear a crías reales
      (`Animal` del `FamilyGenerator`) y el gateo de actividades por bond.
- [~] 3. **Construcción (Meso) / Levantar refugio (Micro)** ([`construction-simulation.md`](construction-simulation.md))
      — **va ANTES que la Mecánica** (refugio antes que metal, Neolítico). Meso = hub de **estructuras** por
      **dispatch/tickets**. **Hecho**: arranque `ConstructionBeginner_AUTO` (limpiar→abastecer→cimentar/muro/
      techar), **El Tallador** autorado + base de historia (§5). **Falta**: recetas de estructura reales por
      área, misiones-historia cableadas, históricos posteriores (Maestro de catedrales/Brunelleschi).
- [~] 4. **Mecánica (Meso) + Forja (Micro)** ([`forge-simulation.md`](forge-simulation.md)): la área Meso
      es la **Mecánica** (reparar/mejorar **máquinas** de todas las áreas + vehículos/drones/teleportadores).
      **Arranque cableado** (`MechanicsBeginner_AUTO`): **limpiar** → **abastecer** → **reparación simple**.
      **1ª reparación real de máquina = CAMBIO DE RUEDA del camión** (`TruckMaintenance_AUTO`: aflojar→gato→
      quitar→poner→apretar→bajar; + aceite + agua). El **camión** lleva las cajas de suministros a las áreas.
      Tracción del Huerto = **bueyes** (decidido). La **forja** de bronce → **Microcosmos**
      (`ForgeVirtualization_AUTO`). **Frontera Construcción/Mecánica** definida (forge §1c). **Falta**:
      máquinas por área, más vehículos (globos/submarinos), teleportadores, decisión arado/espada, misiones-historia.
- [ ] **Onboarding genérico por área**: limpiar → **abastecer** (`StockingTask`, cajas→despensas) → producir.
      Ya en cocina (§2) y mecánica (§1b). `StockingTask`/`ProductionOrder` comparten base `VirtualTask`.
- [ ] 5. **Enfermería/Farmacia** (salud; hereda las enfermedades del sedentarismo; Imhotep/Hipócrates).
- [ ] 6. **Sala de Yoga** (Aliento y Mente; reusa meditación/Microcosmos; Buda/Sócrates; desbloquea maná).
- [x] **Reparación por DISPATCH (tickets)** — **MVP hecho** (`DispatchDemo_AUTO`): `ServiceHub` lista tickets
      + banco de herramientas (`Toolbox` tomar/devolver); la receta `requiresTools` → sin herramientas se
      rechaza. **Primera reparación real: GRIFO QUE GOTEA** de la Cocina (fontanería, la avería más típica/
      simple): cerrar llave → desmontar → cambiar junta → montar → abrir y probar. Bucle: ticket → tomar
      herramientas → ir al grifo → reparar → devolver. **Falta**: UI de tablero, tickets dinámicos, más averías.
- [ ] **Arquetipo de misión: DETENER CONFLICTOS (mediación)** (garden §5) — entre integrantes del mismo
      equipo (dominio vs autonomía) o entre tribus de la misma especie (territorio); todo desde
      pensamientos/humores. Resoluble por campo de calma / posesión-mediación / satisfacer la raíz / bond.
      Reutilizable en todas las áreas; debut en el Huerto. Núcleo: "que los fuertes no se coman a los débiles".

## Siguiente (código)
- [ ] **Alma compartida (resto)**: `HelpRequest` ya hace "ir juntos" (MVP); falta **compartir pensamientos**
      (misma frase → instancia de mente/madre compartida) y que el sí/no lea bond/humores/inclinaciones.
- [ ] **Re-autorar vivencias** de Ötzi/Sembradora/Guardián con el canon de `founding-trio-stories.md`
      (la coneja, el señuelo, la planta favorita, las piedras) al cerrar sus misiones.
- [ ] **Posesión real (resto)**: `PlayerBrain.Act()` ya mueve el cuerpo poseído; falta **interactuar**
      (F/clic enrutado al cuerpo) e integrar `PossessionSpell` con el hechizo real del jugador (crecer power/range).
- [ ] **Asana/hechizo como frases reales** (hoy solo categoría); **multi-instancia** (madre flyweight + relevancia).
- [ ] **Flag futuro** `absorbsPublic` si un bloqueado debe además recibir del pool (hoy: no recibe).
- [ ] **Más históricos** con `Historico()` (quedan Gilgamesh/Enheduanna y eras posteriores;
      docs/mob-characters.md, mob-epochs-matrix.md).

## ⚠ Compilar y PROBAR en Unity (PRs #16, #17 y #18 mergeadas a master)
Código nuevo aditivo: `Control/` + `Kitchen/` + `Virtualization/` + extensiones de Mind. **Guion de prueba
en [`testing-checklist.md`](testing-checklist.md) §11** (control/posesión, cocina, virtualización, mira
central + HeadLook, mecanografía) **y §12** (Mecánica/Construcción arranque + dispatch: reparar el grifo).
- [ ] **Compila** tras `pull`; si algo falla, pégame el error. Sandboxes: `PossessionSandbox_AUTO`,
      `KitchenSandbox_AUTO`, `KitchenOnboarding_AUTO`, `VirtualizationSandbox_AUTO`, `GardenVirtualization_AUTO`,
      `MechanicsBeginner_AUTO`, `ConstructionBeginner_AUTO`, `DispatchDemo_AUTO`, `ForgeVirtualization_AUTO`.
      Conteo de pools `Total≈48 Vivencia≈38`.
- [x] **PR #16 ya verificada en Play** (posesión débil/fuerte, follow, petición→alma compartida,
      suciedad→misión→limpieza) antes del merge de PR #17. Detalle en `testing-checklist.md` §11.
- [x] **Bug encontrado y arreglado (PR #16)**: `AnimaController.PickBest()` no detectaba un `IBrain`
      destruido (el `FollowBrain` temporal de `HelpRequest.EndShare`) porque `b == null` compara por el
      tipo estático `IBrain`, no por `UnityEngine.Object` — Unity solo sobrecarga `==` para detectar
      "destruido" en `Object`. Causaba `MissingReferenceException` en bucle infinito (999+ errores/frame)
      ~8s después de cualquier petición aceptada. Fix: `if ((b as Object) == null) continue;`. **Commiteado**
      (`40cfbd1`).

## Historial (hecho) — detalle en docs/`DEVLOG.md`/git
- **30-jul (PR #18):** **Mecánica** (arranque limpiar→abastecer→reparar) + **Forja del Micro**;
  **dispatch/tickets** (`RepairTicket`/`ServiceHub`/`Toolbox`; 1ª reparación real = **grifo que gotea**);
  **Construcción** (arranque + El Tallador); reorden Construcción→Mecánica; 1ª persona reconciliada
  (`AutoCameraZone`/`HeadLook`).
- **29-jul (PR #17):** **motor de virtualización** (`VirtualPointer` mira central, `StationPart`,
  `ProductionOrder`, `TypingChallenge`/mecanografía, `StockingTask`/abastecer); cocina paso A (limpieza+paseo) y B (desayuno).
- **29-jul (PR #16):** **controlador intercambiable + posesión** (`AnimaController`/`IBrain`/`PlayerCore`/
  `FollowBrain`/`HelpRequest`); **pensamientos escalables** (weight/lifecycle/gate); históricos
  (Guardián/Sembradora/Alfarero/Nasatya); nombres (Ötzi→Nasatya).
- **28-jul (PR #14/#15):** migración a clase única **`Anima`**; **pilar Mente** (frases/campos/pools/reparto
  Estricta-Libre) + `anima-architecture §11`.
- **23-jul:** cocina jugable (Micro); limpieza legacy (`KitchenScale`/`Kitchen*`, `IAnimal`/`IVital`); tríada
  Micro/Meso/Macro; diseño del mundo grande (`world-topology`); backbone de recursos + HUD.
- **09/10-jul:** auditoría + sync de 18 docs; retirada del disparo (no-violencia); **12 aptitudes** + perfiles;
  medios tierra/agua/aire; evolución de aptitudes; dietas/territorialidad; `FishSchool`; Malamute.
