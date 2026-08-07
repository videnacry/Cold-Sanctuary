# Checklist — continuar

Tablero para retomar. Última sesión: 2026-08-06. Marca lo que completes.
Contexto de fondo: [`AUDIT-2026-07-09.md`](AUDIT-2026-07-09.md), [`gaps-vs-planteamiento.md`](gaps-vs-planteamiento.md),
[`world-topology-and-planes.md`](world-topology-and-planes.md) (visión del mundo grande / los 3 planos).

## Decisiones abiertas (rápidas)
- [~] **Microcosmos = mundo de INSECTOS (DECIDIDO opción B)** — [`microcosmos-insects.md`](microcosmos-insects.md).
      Históricos encarnados como insectos (violencia = "volverse salvaje", sin trauma humano). **Hormigas**
      civilización primaria; **cría = mirmecofilia** (pulgón = ganado/mascota); su "fuego" = **feromonas**;
      **hongos** (Physarum oráculo, Ophiocordyceps amenaza); abeja/avispa/termita = otras ciudades. **Pipeline:
      área humana (Meso) → transformar a insecto (Micro).** **1ª misión hecha (scaffold)** `MicrocosmosSandbox_AUTO`
      (amanecer: **cueva** natural, sin reina/hormiguero/feromonas; pulgón-mamá guía + banda de 7 hormigas).
      **Falta:** jugador-avatar guía/carga; colonia real; feromonas como mecánica; dispatch meso→micro.
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
- [~] **Refinar selección de caza**: **hecho (PR #32)** `Predation` — `SelectPrey` ya no caza lo que no puede
      vencer (masa/fuerza/textura/tamaño) y `EvaluateThreat` escala por poder de stats (temer al más poderoso);
      el tamaño invierte presa↔depredador; el farol de transformación no engaña, la real sí. **Falta:** manada
      (masa aliada) y **aura/estatus mágico** del humano (requiere sistema de magia/maestría).
- [x] **Influencia de manada en la caza** (PR #33/#34): `Predation.EffectivePower` suma el poder de los
      **aliados** (misma facción) en radio, ponderado por el **`PackFactor` por especie** (Lobo 0.8/Malamute 0.9
      mucho; Oso 0.3/Zorro 0.2 poco; Conejo/Ciervo/Ballena 0 nada) → `SelectPrey` usa el poder de manada del
      cazador; `EvaluateThreat` compara poder efectivo de ambos. Dinámico y calibrado por especie.
- [~] **Aura/estatus mágico del humano** (PR #33): **mecanismo hecho** — `Anima.magicAura` (firmado, decae con
      `MagicAura`): − destructiva → más temido (`EvaluateThreat`); + benevolente → bonds fáciles (`GrowBond`).
      **Falta:** que el **sistema de magia** llame a `Register*` al lanzar hechizos.
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
- [ ] **Históricos ANIMALES** (POV animal, docs/animal-heroes.md): autorados El Perro de Oberkassel, Togo,
      Hachikō, Cher Ami; **candidatos** Balto, Laika, Clever Hans (→Mente), Koko, Jumbo, Greyfriars Bobby.
- [ ] **Enfermería "Una Salud"** (área 6): **una sola** clínica que cuida a **personas Y animales** (lado
      veterinario dominante; también los voluntarios). Basada en el marco real One Health; el mismo sanador
      trataba a ambos. El Perro de Oberkassel es su fundacional-animal. (Diseño: area-progression §6.)
- [ ] **Upa-yoga (resto)** (upa-yoga-mission §6): *(rebind del input ✔ PR #22)*; **asignar los
      huesos del rig** cuando el avatar rigged esté en escena (el driver ya los mueve), cablear efectos a
      `PlayerStats`/humores, mapeo elemento→compuesto (`Chemistry`), QTE de hombros (3+3), verificar reps (vídeo Isha).
- [ ] **Transformar la Cocina** (hoy humana) → reencarnaciones (viaje 2: Nasatya llevando restos) + viaje 3
      (Enfermería insecto: guiar al refugio + motivar a los deprimidos). Cerrar §13.4 (nombre anciano-pintor;
      vidas 2 de Atlas/Sakshi/Momo; ubicación de reencarnaciones).
- [ ] **Stats-as-truth — rebanadas** (docs/stats-as-truth.md §8): `CreatureRig` ✔ · `BodyPart` única ✔ ·
      `ScreenEffects` ✔ · **emotion-slice ✔** (`EmotionExpression`/`BodyPartReactor`/`EmotionReader`; circumplex+
      Laban+frases+legibilidad; `afabilidad`/`sensibilidad`; `emotion-model.md`) → **quedan**: **composición**
      (fases 1-2 ✔ `CharacterComposition`: adornos/ropa/miembros→visual+delta gestionado, host-mod, injerto progresivo; fase 3: BodyPartStats/assets)
      → **depredación por stats ✔** (`Predation`:
      masa/fuerza/textura/tamaño; el tamaño invierte presa↔depredador) → **hechizos** (transformación
      3-niveles/farol-vs-real, bond por stats,
      lector-de-mentes) sobre `PossessionSpell`+energía-timer. **Transformación 3-niveles/farol-vs-real ✔**
      (`TransformationSpell`); falta ligarla a energía-timer y a la depredación. Monetización cosmética al final.

## ⚠ Compilar y PROBAR en Unity (PRs #16–#47 en master)

`Control/` + `Kitchen/` + `Virtualization/` + `Prologue/` + `Microcosmos/` + extensiones de Mind. **Guion de
prueba en [`testing-checklist.md`](testing-checklist.md) §11–§14.** Bugs ya arreglados por el equipo:
`AnimaController.PickBest` (Object null-check), `GuidedTour.stopDistance`, y `CarryToRefuge`/`PrologueSequence`
`UnityEvent` sin inicializar (abortaba `Build()`). *(Pendiente reportado: los `AddListener` puestos en el
editor-script no sobreviven a Play → el aviso de `PlaneMessenger` del sandbox no dispara; no bloquea.)*
- [ ] **Compila** tras `pull`; si algo falla, pégame el error. Sandboxes: `PossessionSandbox_AUTO`,
      `KitchenSandbox_AUTO`, `KitchenOnboarding_AUTO`, `VirtualizationSandbox_AUTO`, `GardenVirtualization_AUTO`,
      `MechanicsBeginner_AUTO`, `ConstructionBeginner_AUTO`, `TruckMaintenance_AUTO`, `DispatchDemo_AUTO`,
      `ForgeVirtualization_AUTO`, `PrologueSandbox_AUTO`, `CriaBeginner_AUTO`, `MicrocosmosSandbox_AUTO`
      (ahora tableau §13: Ambrosio/Sakshi/Héspero/Medea/Momo/Ruth/Atlas + `SoulRecord`), `UpaYogaSandbox_AUTO`,
      `ScreenEffectsSandbox_AUTO`, `EmotionOrchestraSandbox_AUTO`, `EmotionReader_AUTO`.
      Pools `Total≈59 Vivencia≈49`.

## Historial (hecho) — detalle en docs/`DEVLOG.md`/git
- **07-ago (PR #47):** **comer rellena las reservas de magia** (`Metabolism`→`MagicReserves.Store`, hasta el
  tope, una vez `unlocked` por el 1er hechizo; el resto→grasa) → cierra el bucle comer↔lanzar. + doc
  `magic-metabolism §13`: coste REAL de hechizos con numeros (fuego: vela 0,018g/lanzallamas 22g/dragon 2,2kg;
  quimica da para lo pequeño, los grandes exigen nuclear/masa-energia E=mc2=90TJ/g), materia-vs-energia
  (bosones=energia), escalera de hechizos, niveles de energia (fisica en Mecanica/Forja), UI atomica.
- **07-ago (PR #46):** los hechizos **declaran coste y pagan de `MagicReserves`**: `TransformationSpell` y
  `PossessionSpell` tienen `cost` (List<ElementCost>) y llaman a `Pay` antes de actuar → sin reservas de ese
  elemento, el hechizo **no se lanza** (opt-in: sin `MagicReserves` es gratis). `magic-metabolism §7`.
- **07-ago (PR #45):** **reserva de magia + coste de hechizo** (`MagicReserves`/`ElementCost`: stock por elemento
  = lo que ve el jugador en la tabla; agotado → no lanza ese hechizo) + **clima/actividad mueven la necesidad**
  (`Metabolism`: frío→mas gasto de grasa/termogenesis → el oso craves grasa por el clima; esfuerzo→mas gasto).
  + doc `magic-metabolism-progression` MUY expandido: adiccion (RPE/tolerancia/contexto), nivel atomico,
  comer=combate de elementos, jerarquia completa de la materia (electrones=leptones aparte; bosones=fuerzas),
  minijuegos de desintegracion, organos como componentes (meso/micro, Da Vinci), trascender (espacio/dark/gravedad), MCP/fuentes.
- **07-ago (PR #44):** **la necesidad decide la depredacion** — `SelectPrey` sesga la presa por el `Craving`
  (x`Selectivity`: saciado exquisito/hambriento come todo) y `AbsorbFood` come mas de la parte que le falta
  (oso craves grasa -> foca -> blubber). + **`docs/magic-metabolism-progression.md`**: arco de progresion del
  hechizo de comer (comida->compuestos->elementos->quarks; luego invertir hacia super-celulas->quimeras;
  magia=reservas quimicas; adiccion). Cocinas por santuario (cocina/tienda/laboratorio/planta nuclear).
- **06-ago (PR #43):** **apetito por NUTRIENTES** (`Metabolism` reescrito, base cientifica): pools por nutriente
  (proteina/grasa/carb/minerales) que se agotan → deuda → `Appetite` (protein leverage) atenuado por leptina;
  `Craving` (lo que mas falta → que cazar/que parte, p.ej. oso polar→grasa) y `Selectivity` (saciado→exquisito/
  hambriento→come todo). Techo de utilizacion → stats; exceso → grasa. `AbsorbFood` reparte por material.
  Falta: `Craving`/`Selectivity` sesguen `SelectPrey`/parte; herbivoros/Eater/ElementFragment.
- **06-ago (PR #42):** **comer → absorber** enganchado (carnívoros): `Carnivore.Feed` llama a `Metabolism.
  AbsorbFood(nutrition, material)` (Meat/Fish→N, Fruit/Grass→C) → lo útil a `Constitution` (stats), el exceso a
  grasa; cierra el bucle jugable de la química/absorción. Falta: herbívoros/`Eater`/`ElementFragment` (mismo one-liner).
- **06-ago (PR #41):** **límite de absorción** (`Metabolism`, base científica leptina/ghrelina/set-point +
  techo de síntesis proteica): la absorción no tiene tope, la **utilización** sí (techo ~0.4 g/kg escalado por
  masa + adaptación por uso); **exceso → grasa (`fatReserves`), no stats** → no se come todo el día para
  hacerse fuerte. `Appetite` = hambre(ghrelina) − reservas(leptina). `Absorb`→`Constitution`. `stats-as-truth §9`.
  Falta: enganchar comer/`ElementFragment`→`Absorb`.
- **06-ago (PR #40):** `Constitution` enganchada a **`Chemistry`** — los elementos del Nivel 1 son ahora
  **símbolos reales de la tabla periódica** (validados contra `PeriodicTableManager.GetData`) y **alimentables**
  (`AddElement(symbol, delta)`) desde el juego (absorber/comer elementos → cambia la constitución → mueve los
  stats base). `stats-as-truth §9`. Falta: que `ElementFragment`/comer llame a `AddElement`; recetas reales.
- **06-ago (PR #39):** **química como fundamento (por niveles)** — `Constitution`: elementos→compuestos→
  células→stats base (glóbulos=Fe+proteína, etc.), delta gestionado, neutro por defecto. + **hechizo
  inerte→vivo** (`CharacterComposition.Animate`) + **aura energiza los componentes propios aun inertes**
  (×(1+magicAura)). `stats-as-truth §9`. Falta: elementos desde `Chemistry`; aura→aliados (contagio).
- **06-ago (PR #38):** **tejido vivo (doble sentido)** — `CompositionPart.living`: vivo = modulado por el
  huésped + injerto progresivo (miembro/ropa-viva); rígido = aporte plano (metal/coraza). + **dirección
  documentada** (`stats-as-truth §9`): la **química como fundamento** (2 capas: `Humores` transitoria→emoción/
  pensamientos ✔; `Constitution` estructural→stats base, por hacer). Química→pensamientos ya parcial vía `Mind`.
- **06-ago (PR #37):** **composición fase 2** — `CharacterComposition` + `StatBonus`: las partes aportan un
  **delta gestionado** de stats sobre la constitución (campos de `Anima`) **sin pisar evolución/transformación**;
  aporte **biológico modulado por la vitalidad del huésped** (el mismo brazo rinde distinto según el cuerpo) e
  **injerto progresivo** (`adaptSpeed`: converge/decae). Base/efectivo resuelto de forma aditiva y segura.
  Fase 3: `BodyPartStats`, miembros como assets, base desde `Humores`/`Chemistry`.
- **06-ago (PR #36):** **composición fase 1** — `CharacterComposition`+`CompositionPart` (`Assets/Scripts/
  Composition/`): partes slotables (adornos/ropa) → activan su visual y la **defensa de la ropa suma a
  `Anima.armadura`** (que `Predation` lee → vestir armadura = peor presa). `Equip`/`Unequip` por slot; reutiliza
  `ClothingSlot` (ampliado con Hair/Eyebrows/Eyes) y `ClothingRecipe`. Cierra el runtime que faltaba a la ropa.
  **NO toca el modelo de stats** (base/efectivo = fase 2; miembros con stats = fase 3).
- **06-ago (PR #35):** afinar emoción — **freeze/inmovilidad tónica** (`BodyPartReactor.freezeOnFear` + `Emotion
  Expression.Fear`: el miedo intenso congela la parte, conejo/ciervo) + **contagio emocional** (si hay
  `ThoughtField`, la emoción intensa lo proyecta con tono/humor por cuadrante → los `Mind` cercanos lo cogen).
  El diálogo interno ya sale emocionado vía humores compartidos (`Mind.Think`). Sandbox: `Conejo_freeze` + field.
- **05-ago (PR #34):** manada por especie — `Predation.EffectivePower` ahora pondera a los aliados por el
  **`PackFactor` de la propia especie** (ya calibrado: Lobo 0.8/Malamute 0.9/Oso 0.3/Zorro 0.2/Conejo·Ciervo·
  Ballena 0) en vez de un 0.5 fijo; alinea con la convención de `Animal.DecideReaction`.
- **05-ago (PR #33):** **remate de depredación** — **manada** (`Predation.EffectivePower`: suma poder de aliados
  por facción en radio → `SelectPrey`/`EvaluateThreat` dinámicos: lobo solo no puede con el oso, la manada sí;
  el oso evita al lobo con manada) + **aura mágica** (`Anima.magicAura` firmado + `MagicAura` que decae:
  − destructiva → más temido; + benevolente → bonds fáciles vía `GrowBond`). Falta: que la magia llame a `Register*`.
- **05-ago (PR #32):** **depredación por stats** (`stats-as-truth §2`): `Predation` (`Assets/Scripts/
  Transformation/`) + `Anima.armadura` (textura/coraza). `Diet.SelectPrey` no caza lo invencible; `Animal.
  EvaluateThreat` escala la amenaza por poder de stats. El **tamaño invierte presa↔depredador**; el **farol**
  de transformación no engaña (no cambia stats), la **real** sí. Falta: manada + aura mágica.
- **05-ago (PR #31):** **transformación por combate de stats** (`stats-as-truth §4`): `TransformationSpell` +
  `StatProfile`/`TransformPreset` (`Assets/Scripts/Transformation/`). `Cast` → Failed / VisualOnly (farol,
  conserva stats) / Full (cuerpo+stats) según potencia vs coste (resistencia+inyectado por `Might`); revert
  por duración; bidireccional. Se cablea en `Anima`s reales (sin sandbox: Anima abstracta). Pendiente: ligar a
  energía del hechizo + que la depredación "huela" el resultado.
- **05-ago (PR #29):** **legibilidad** — `EmotionReader` lee la orquesta de otros `Anima` (siente/quiere/hará
  + aproximabilidad; alcance/detalle por **percepción**) → base del lector-de-mentes y del vínculo con crías.
  Sandbox `EmotionReader_AUTO`. Cierra la emotion-slice (Laban/frases/legibilidad).
- **05-ago (PR #28):** emoción — cualidades **Laban** (`Quickness/Heaviness/Boundness` desde el circumplex →
  el *cómo* del movimiento) + etiqueta `Emotion` + `BodyPart` gana orejas (`EarLeft/EarRight`) + **tabla
  por-especie** con NUESTROS animales (Oso/Lobo/Zorro/Malamute/Conejo/Ciervo/Foca/Ballena/Pájaro; etología
  real → parte + rol, incl. freeze/inmovilidad tónica) — `emotion-model.md §4c`.
- **05-ago (PR #27):** **emotion-slice (orquesta)**: `EmotionExpression` (conductor: `Humores`→valencia/
  activación/tensión+`Jolt`, sesgo por aptitudes) + `BodyPartReactor` (instrumento: reacción pasiva/violenta
  por parte, traducción entre especies orejas↔brazos↔antenas↔alas) + aptitudes nuevas en `Anima`
  (`afabilidad`, `sensibilidad`) + `UpaYogaSession` resuelve huesos vía `CreatureRig`/`BodyPart`. Sandbox
  `EmotionOrchestraSandbox_AUTO`, `EmotionReader_AUTO`. (`docs/emotion-model.md §4b`.)
- **05-ago (PR #26):** **`docs/emotion-model.md`** — modelo de emoción para **toda Anima** con base científica
  (circumplex=`Humores.Positividad×Energia` ya en código; Laban→`CreatureRig`; Big Five→aptitudes; etología/
  Darwin→señales animales+legibilidad). Aptitudes nuevas propuestas (`afabilidad`; promover `sensibilidad`).
  La **firma emocional = tell de reencarnación**. (Doc; código = `EmotionExpression` a continuación.)
- **05-ago (PR #25):** **fuente única de partes**: se retira `RigPart` y se **amplía el `BodyPart` de Asana**
  (0–7 = regiones de yoga intactas; + huesos finos L/R + insecto/quimera). `CreatureRig` tira de ella;
  `bodyStats[8]` sigue igual (los huesos finos no se indexan → `GetBodyPartStats` vacío por el guard). Base
  para la emotion-slice (`docs/stats-as-truth.md §8`).
- **05-ago (PR #24):** **`ScreenEffects`** (cámara artística: tintes por estado sueño/fatiga/estrés/
  satisfacción + `Fade` que completa el blackout de `CameraManager`) + **hotfix:** el `enum` de `CreatureRig`
  colisionaba con `BodyPart` de Asana → renombrado (y en **#25 unificado** en una sola `BodyPart`) (master #23
  no compilaba). Sandbox
  `ScreenEffectsSandbox_AUTO`, `EmotionOrchestraSandbox_AUTO`, `EmotionReader_AUTO`.
- **05-ago (PR #23):** **`docs/stats-as-truth.md`** (la ley única: `stats→frases→todo`; hechizo=modo/energía=
  timer; transformación por combate de stats con farol-vs-real; composición por componentes tipo CodeShip;
  stats bidireccionales; Quimeras) + **`CreatureRig`** (parte lógica→hueso; auto `HumanBodyBones` humanoide /
  manual insecto — un motor de yoga/emoción para cualquier cuerpo).
- **03-ago (PR #22):** upa-yoga **(a)** — rebind del input (`HeadLook`/`PlayerController` ceden a
  `UpaYogaSession.Active`, como con `TypingChallenge`) + **hooks de rig** null-safe (neck/hombros; el driver
  rota el hueso asignado con las teclas + jitter de temblor, no-op si vacío) + refinos (tileMode Grow/Fall;
  Grow muestra elemento; temblor **localizado** en la parte que se mueve).
- **31-jul (PR #21):** **nivel introductorio canónico** (`microcosmos-insects.md` §13: historia reescrita
  —Sakshi cría al pulgón Ambrosio, la cueva, la muerte—, mapa de almas y **reencarnación** con *tell*, **armas
  químicas/veneno**, género neutro) + **`novela.md`** (narración íntegra, semilla de la novela del juego) +
  **`SoulRecord`** (ficha de alma) + **1ª virtualización de yoga** (`UpaYogaSession`: upa-yoga de cuello,
  **control-por-partes** + **ritmo Guitar-Hero** con fichas/aliento/temblor; `upa-yoga-mission.md`).
- **30-jul (PR #20):** **El Perro de Oberkassel** + animales-héroe (Togo/Hachikō/Cher Ami, POV animal);
  **Enfermería "Una Salud"** (personas+animales); **Microcosmos INSECTO** (opción B: hormigas, mirmecofilia/
  pulgón, feromonas=su "fuego", hongos); **1ª misión del micro** (cueva + pulgón-guía + banda de 7 hormigas).
- **30-jul (PR #19):** **área de CRÍA** (rutina de cuidado enganchada a drives reales; El Perro fundacional) +
  **prólogo Enfermería** (pre-fuego, La Recolectora) + **camión** (1ª reparación = cambio de rueda) + reorden
  (Cría=3). Fixes del equipo: `PickBest`/`GuidedTour`/`UnityEvent`.
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
