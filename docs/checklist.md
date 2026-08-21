# Checklist — continuar

Tablero para retomar. Última sesión: 2026-08-07. Marca lo que completes.
Contexto de fondo: [`AUDIT-2026-07-09.md`](AUDIT-2026-07-09.md), [`gaps-vs-planteamiento.md`](gaps-vs-planteamiento.md),
[`world-topology-and-planes.md`](world-topology-and-planes.md) (visión del mundo grande / los 3 planos).

> **PRÓXIMO PASO = TESTING.** El arco **magia/metabolismo/descomposición** (PRs #45–#61) está **mergeado y
> documentado** (`docs/magic-metabolism-progression.md`). **Dos sandboxes ya construidos** (PR #54, salen con
> `Build Sample Scene Blockout`): **`Descomposicion_AUTO`** (minijuego de 3 fases → economía, no necesita
> `Anima`) y **`Magia_AUTO`** (HUD de prueba del bucle comer→desbloquear→lanzar). Guion por sistema en
> [`testing-checklist.md` §19](testing-checklist.md). El resto del arco sigue opt-in sin cablear en juego real.

## Decisiones abiertas (rápidas)
- [~] **Microcosmos = mundo de INSECTOS (DECIDIDO opción B)** — [`microcosmos-insects.md`](microcosmos-insects.md).
      Históricos encarnados como insectos (violencia = "volverse salvaje", sin trauma humano). **Hormigas**
      civilización primaria; **cría = mirmecofilia** (pulgón = ganado/mascota); su "fuego" = **feromonas**;
      **hongos** (Physarum oráculo, Ophiocordyceps amenaza); abeja/avispa/termita = otras ciudades. **Pipeline:
      área humana (Meso) → transformar a insecto (Micro).** **1ª misión hecha (scaffold)** `MicrocosmosSandbox_AUTO`
      (amanecer: **cueva** natural, sin reina/hormiguero/feromonas; pulgón-mamá guía + banda de 7 hormigas).
      **Hecho (PR #62):** `WeaknessEffect` (hormigas viejas), `HoneydewSpell` (maleza Ambrosio = primer consumible),
      `PullSpell` (Jalar, primer hechizo), `FormicAcidSpray` (defensa Kushal), `SpellBase` (base de hechizos),
      `CombatTargetSelector` generalizado a `ITarget`.
      **Falta:** jugador-avatar guía/carga; sandbox escena Nivel 1 (suelo de bosque + cueva checkpoint + zonas de depredadores);
      colonia real; feromonas como mecánica; dispatch meso→micro; conectar `HoneydewSpell` al inventario.
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

## ⚠ Compilar y PROBAR en Unity (PRs #16–#96 en master)

`Control/` + `Kitchen/` + `Virtualization/` + `Prologue/` + `Microcosmos/` + extensiones de Mind. **Guion de
prueba en [`testing-checklist.md`](testing-checklist.md) §11–§14.** Bugs ya arreglados por el equipo:
`AnimaController.PickBest` (Object null-check), `GuidedTour.stopDistance`, y `CarryToRefuge`/`PrologueSequence`
`UnityEvent` sin inicializar (abortaba `Build()`). *(Pendiente reportado: los `AddListener` puestos en el
editor-script no sobreviven a Play → el aviso de `PlaneMessenger` del sandbox no dispara; no bloquea.)*
- [ ] **Compila** tras `pull`; si algo falla, pégame el error. Sandboxes: `PossessionSandbox_AUTO`,
      `KitchenSandbox_AUTO`, `KitchenOnboarding_AUTO`, `VirtualizationSandbox_AUTO`, `GardenVirtualization_AUTO`,
      `MechanicsBeginner_AUTO`, `ConstructionBeginner_AUTO`, `TruckMaintenance_AUTO`, `DispatchDemo_AUTO`,
      `ForgeVirtualization_AUTO`, `PrologueSandbox_AUTO`, `CriaBeginner_AUTO`, `MicrocosmosSandbox_AUTO`
      (ahora tableau §13: Ambrosio/Sakshi/Héspero/Medea/Momo/Ruth/Atlas + `SoulRecord`),
      `Nivel1Microcosmos_AUTO` *(pendiente: escena abierta suelo-de-bosque + hormigas con `WeaknessEffect` + checkpoint cueva)*,
      `UpaYogaSandbox_AUTO`,
      `ScreenEffectsSandbox_AUTO`, `EmotionOrchestraSandbox_AUTO`, `EmotionReader_AUTO`.
      Pools `Total≈59 Vivencia≈49`.

## Historial (hecho) — detalle en docs/`DEVLOG.md`/git
- **21-ago (PR #126):** **etapa 5 — BORRADA la jerarquía de subclases de forrajeo**. `Carnivore`/`Herbivore` eliminadas; las 8 especies heredan ya directo de `Animal`. Los flags de comida (`eatsPrey`/`eatsGrass`/`eatsFish`) los fija `Forager.ConfigureForSpecies(SpeciesArchetype)` en `Init` (data por especie: Wolf/Malamute→presa; Bear/Fox→presa+pesca; Bunny/Deer→pasto; Whale/Seal→pesca) → retirados el hook `ConfigureForager` (virtual de `Animal` + overrides de Bear/Fox) y `GrazesOnLand` (Whale/Seal). Los 2 `GetComponent<Carnivore>()` (detección de depredador en `Animal.SenseThreats` y `FishSchool.NearestPredator`) recableados a `Forager.eatsPrey`. Quedan solo `Anima → Animal → XBehavior` (las 8 clases son ya `SpeciesArchetype` + `Start` + `ConfigureThreat`); las refs restantes a Carnivore/Herbivore son solo comentarios históricos. Falta E6: mover `ConfigureThreat` a data + `SpeciesArchetype` settable → borrar también las 8 clases (dejar `Animal` + data).
- **21-ago (PR #125):** **`Animal` CONCRETA** (hito). Resuelto el último `abstract` (`Feed()` → concreto por flags: `Forage.eatsPrey ? Hunt : Graze`), y quitado `abstract` de la clase. Retirados `Carnivore.Feed`/`Herbivore.Feed` (redundantes). Toda la conducta/data ya está en componentes+catálogos, así que `Animal` es instanciable. **Reencuadre de "desacoplar":** sacar `LifeStage`/`Family`/`PostNatal` del tipo `Animal` es inviable (~15 miembros) y conceptualmente incorrecto (son sistemas del ser-animal, no universales) → el desacople correcto es `Animal` concreto + borrar la jerarquía de subclases (pendiente: recablear 2 `GetComponent<Carnivore>()`, mover hooks a data, borrar Carnivore/Herbivore/especies).
- **20-ago (PR #124):** **etapa 5 — `PostNatalStages` a data (`PostNatalProfile`)**. El último blob (secuencias de crianza anidadas por especie) se movió VERBATIM (copia del literal, sin reparsear valores) a `PostNatalProfile.Of(species)` (switch); `Animal.PostNatalStages` lo lee. Retirados los `_postNatalStages` + overrides + limpieza cosmética. **Las 8 clases de especie quedan casi vacías** (Wolf: `SpeciesArchetype` + `Start` + `ConfigureThreat`). Toda la DATA de especie es ya catálogo/componente. Falta: desacoplar `LifeStage`/`Family`/`PostNatal` del tipo `Animal`, hacer `Animal` concreto, borrar la jerarquía.
- **20-ago (PR #123):** **etapa 5 — ciclo de vida (stages + events) a data (`StageProfile`)**. Las etapas (`Childhood`/`Adolescence`/`Adulthood`, cada una `stageDays/minScale/maxScale`) y los 3 arrays de `Events` (varían: HomeBound territorial, Feed adulto) dejan de ser campos por clase y pasan a `StageProfile.Of(species)` (extraído 1:1 por script). `Animal` crea las etapas en `Init` (antes de `Fatten`, que usa `ChildStage`; auto-property porque se mutan) y los Events son get-only del perfil. Retirados stages+events+overrides de las 8 especies → clases DIMINUTAS (Seal 12 llaves). Solo queda `PostNatalStages` + `SpeciesArchetype` + hooks.
- **20-ago (PR #122):** **etapa 5 — gaits (`ActsPrep`) a data**. Los `actsPrep` (idle/walk/run con aniName + navSpeed/aniSpeed/energyCost) dejan de ser un campo por clase y pasan a `ActionsPrep.Of(species)` (catálogo extraído 1:1 por script; Bunny reusa "RunBunny" para andar). `Animal.ActsPrep` (antes abstract) se fija en `Init` antes de la config de `WalkSpell`. Retirados los 8 campos + overrides. Especies mínimas (Seal 21 llaves). Queda: stage params + Events + PostNatalStages.
- **19-ago (PR #121):** **etapa 5 — Preps de etapa a default de `Animal`**. Los 24 `*Preparations` (child/teen/adult × 8 especies) eran IDÉNTICOS (`{SetScale, SetRemainingStageDays}`) → `Animal.ChildPreps/TeenPreps/AdultPreps` concretos (array estático compartido, solo-lectura). Retirados los 24 campos + 24 overrides. Los `*Events` varían (HomeBound por territorio) → siguen como data por especie.
- **19-ago (PR #120):** **radio de caza POR STATS + scavenging**. El radio de detección de presa deja de ser fijo y pasa a `perception × huntRangePerPerception` (deriva de la percepción, crece con la evolución). Las **carcasas** cercanas son seleccionables siempre (comida gratis), **incluso de la propia especie** (scavenging ≠ canibalismo; el bloqueo de la-propia-especie ya solo aplica a presa VIVA).
- **19-ago (PR #119):** **presa por PROXIMIDAD + STATS; `Diet` RETIRADA**. `Forager.SelectPrey` busca `Anima`s comestibles cercanas (`Physics.OverlapSphere(huntRadius)`) —incluidas carcasas y el JUGADOR—, descarta la propia especie (no canibalismo) y las protegidas por vínculo (`CanHarm`); una presa VIVA solo cuenta si mi poder EFECTIVO (con manada) supera su defensa (`Predation`). Preferencia EMERGENTE (más fácil/cercano). Borrada la clase `Diet`/`PreyEntry` + las tablas de las 4 carnívoras + `Carnivore.Diet`. Oso/Zorro marcan `eatsFish` (pescan). El jugador-como-presa se cubre solo (es un `Anima`); se pierde caza de aves (no-Anima). Knobs: `Forager.{huntRadius, distanceWeight}`. Alinea con "depredación por stats" y borra 2 blobs de camino a E6.
- **19-ago (PR #118):** **etapa 5 — `Population` a registro central**. Los `static HashSet population` por clase de especie pasan a `AnimalPopulations.Of(species)` (dict por nombre). `Animal.Population` (get-only) lo lee; las `Diet` de los carnívoros re-cablean `XBehavior.population` → `AnimalPopulations.Of("X")` (mismo set → altas/bajas y targeting coinciden). Retirados 8 estáticos + 8 overrides. (`BirdBehavior.population` NO se toca: aves = clase aparte, no del roster.) Re-cableado mecánico verificado por grep.
- **19-ago (PR #117):** **etapa 5 — `Group`/`Family` a data**. La estructura familiar por especie (`new Family(size, parentsRate, parentalCare)`) deja de ser un `defaultGroup` estático por clase y pasa a un catálogo `Family.Of(species)` (copia por ser). `Animal.Group` (antes abstract) es auto-property concreta, fijada en `Init` desde el catálogo (RenderFamily la reemplaza). Retirados `defaultGroup`/`group`/override `Group` de las 8 especies. (Population sigue en las clases: está referenciada cross-especie por las `Diet`.)
- **19-ago (PR #116):** **etapa 5 — boilerplate de estado a `Animal`/data**. `animationsName` (no lo lee nadie) pasa a derivado del nombre de especie (`new AnimationsName(SpeciesArchetype)`); `HomeOrigin` (estado por-instancia) a auto-property concreta; `HomeRadius` a data de especie (`SpeciesBody.homeRadius` + catálogo). Retirados los 8+8+8 overrides/campos de las especies. Las clases de especie quedan ya solo con la DATA compleja (stages/preps/events/ActsPrep/Diet/Population/Group) + Start/Configure hooks — lo último para E6.
- **19-ago (PR #114):** **etapa 5 — config escalar de especie a data (`SpeciesProfile`)**. Los 11 escalares por especie (PackFactor, HarmVsBond, BondGrowthRate, BiteSize, Toughness, BaseStressLevel, VocalizationThreshold, NestSecurityLevel, MaxFatReserves, FatAccumulationRate, ThreatThreshold) + Material dejan de ser overrides por clase y pasan a un catálogo `SpeciesProfile.Of(species)` (valores extraídos por script, idénticos). `SpeciesBody.profile` lo provee; los virtuals de `Animal` (accesor `Prof`, nunca null) lo leen. Retirados ~88 overrides de las 8 especies. Las clases de especie quedan ya solo con boilerplate de estado + stage/diet/animations data.
- **19-ago (PR #113):** **etapa 5 — bases evolutivas a `SpeciesBody`**. `BaseAgility`/`BasePerception`/`BaseSensibility` dejan de ser overrides por especie y pasan a campos de `SpeciesBody` + catálogo por especie; `Apply` las fija y escribe `agility/perception/sensibility` iniciales (se retiran esas líneas de `Init`). Los virtuals de `Animal` (y `EvolveAptitudes`) las leen del componente. Retirados 16 overrides (8 especies × 2). `SpeciesBody` cubre ya: arquetipo→stats + pensamientos + medio + bases evolutivas.
- **19-ago (PR #112):** **etapa 5 — config de amenaza al componente**. `Aggressiveness`/`CanHitAndRun` dejan de ser virtuals de `Animal` y pasan a campos de `ThreatResponder` (`aggressiveness`/`canHitAndRun`); `ResolveReaction` ya no se los pasa a `Decide`. Cada especie los fija en un hook `ConfigureThreat` (llamado en `Init`) → retirados los 7 overrides (Seal ya usaba la base). `ThreatResponder` queda autocontenido. Un paso más para vaciar las clases de especie hacia data/componentes.
- **18-ago (PR #111):** **etapa 5 — `Physiognomy` desacoplada del tipo `Animal`**. `GetMealWeight/Max/Min` toman ahora `(float mass)` en vez de `(Animal)`; los ~10 llamantes (Forager/ActionPrep/PostNatalManager/LifeStage) pasan `rig.mass`. `Physiognomy` queda como clase de datos+math pura, usable por cualquier ser. Anotada la idea de que la physiognomy EMERJA de stats/químicos/hormonas (cuerpo como resultado). Falta E6: desacoplar `LifeStage`/`Family`/`PostNatal` y reconstruir el lobo por composición.
- **18-ago (PR #110):** **etapa 5 — `Physiognomy` data-driven**. El físico de especie (escala/masa/pesos de comida) deja de ser un `defaultBody` por clase y pasa a un catálogo `Physiognomy.Of(species)` (copia por ser). `Animal.Body` (antes abstract) se fija en `Init` desde ahí, antes de `Fatten`. Retirados `defaultBody`/`body`/override `Body` de las 8 especies. Con esto la identidad física de especie (stats+pensamientos+medio+físico) es ya toda DATA → las clases de especie quedan casi sin config. Falta E6: desacoplar `Physiognomy`/lifecycle/family del tipo `Animal` y reconstruir un lobo por composición.
- **18-ago (PR #109):** **etapa 5 — medio data-driven**. La afinidad por medio (land/water/air) pasa de overrides por clase (Whale/Seal) a **data del arquetipo** (`ArchetypeProfile` + `Archetypes` fija Whale/Seal acuáticos); `Anima` gana campos settables (`landAffinity/waterAffinity/airAffinity`, los virtuals los leen) y `SpeciesBody.Apply` los escribe. Retirados los overrides de Whale/Seal. Anotada la idea de **lifecycle por química/hormonas/bonds** (no solo temporizadores). Falta: `Physiognomy` como data + desacoplar lifecycle/family.
- **18-ago (PR #108):** **etapa 5 — `SpeciesBody`**. La identidad de especie (nombre de arquetipo → stats base físicas/mentales + pensamientos base) sale de `Animal.ApplySpeciesArchetype` a un componente `SpeciesBody` (`species` + `Apply(anima)`). `Animal` lo auto-añade en `Init` sembrado con su `SpeciesArchetype`; `SpeciesName` (karma/relaciones) lo lee. Así la especie deja de ser el TIPO de clase y pasa a ser data de componente → paso clave para reconstruir un lobo por composición. Falta: medio/`Physiognomy` y desacoplar lifecycle/family.
- **18-ago (PR #107):** **etapa 4 — reconciliar IA**. Las decisiones ACTIVAS del animal (forrajeo + amenaza) salen de `Restore` a `Animal.ActiveBehaveTick`, conducidas por `AiBrain` (auto-añadido junto a `AnimaController` en `Init`, con `RefreshBrains`). La POSESIÓN (`PlayerBrain`) las suprime → el jugador toma el mando del mismo cuerpo/componentes. `Restore` queda solo con lo PASIVO (metabolismo/evolución/medio/velocidad + decaimiento). Una sola IA. *Falta:* al poseer, evitar que WalkSpell (Transform) pelee con el NavMeshAgent.
- **18-ago (PR #106):** **etapa 3 COMPLETA — persecución extraída**. `Carnivore.Feed` movido entero a `Forager.Hunt(self)` (elegir presa + perseguir por `Locomotion` + herir al alcance + comer al abatir + llevar sobras a las crías). Ahora `Forager` posee TODO el forrajeo (select omnívoro + Hunt + Graze + Eat); `Carnivore`/`Herbivore` quedan en ~18 líneas (solo config `Diet`/`GrazesOnLand` + delegación de `Feed`). Siguiente: reconciliar IA (`AiBrain` conduce ThreatResponder/Locomotion/Forager) → reconstruir un lobo por composición → borrar `Animal`.
- **18-ago (PR #105):** **etapa 3 — pastar extraído**. `Herbivore.Feed` movido entero a `Forager.Graze(self)` (selección + ir a la fuente por `Locomotion` + comer + reducir el banco de peces). `Herbivore.Feed` queda como delegación de una línea. Falta solo la persecución del carnívoro (`Carnivore.Feed` → `Forager.Hunt`).
- **18-ago (PR #104):** **etapa 3 — extraído el COMER** (carnívoro) a `Forager.Eat`: mordisco (`IEdible.Consume`) + nutrición (hambre + `Metabolism` opt-in) + **bond** con quien dejó la comida + 1ª sólida de cría. `Carnivore.Feed` delega. Confirmado **multi-consumo** (varios animales muerden el mismo `IEdible`, pool compartido de gramos) → "dejar la presa en el nido" funciona (soltar un `FoodItem`, los demás lo forrajean). Falta extraer la persecución y el pastar del herbívoro.
- **18-ago (PR #103):** **`Forager` — modo mixto/omnívoro**. Sustituido el enum `FoodMode` por flags **combinables** (`eatsPrey`/`eatsGrass`/`eatsFish`); `SelectTarget` elige la fuente **más cercana** de las que come → un omnívoro (varios flags) come presa Y pasto. Carnívoro=eatsPrey+Diet, herbívoro=eatsGrass/eatsFish.
- **18-ago (PR #102):** **etapa 3 — semilla de `Forager`**. Componente `Forager` (`FoodMode` Prey/Grass/Fish + `Diet`) con la POLÍTICA "qué/dónde comer": carnívoro→`Diet.SelectPrey`, herbívoro→pasto/banco más cercano. `Animal.ConfigureForager` (hook por especie) lo configura en `Init`; `Carnivore`/`Herbivore.Feed` usan `Forage.SelectTarget`. Un paso hacia disolver la subclase `Carnivore`/`Herbivore` (el qué-come pasa a config de componente). Persecución/comer siguen en `Feed` (siguiente).
- **18-ago (PR #101):** **`Locomotion` — resto de comportamientos migrados**. `Carnivore.Feed` (persecución/comer), `Flee`/`Fight`/`HitAndRun`, wander y salidas de amenaza pasan por `Loco.*` (`GoTo`/`SetGait`/`Idle`/`Walk`/`Run`). Ya NO hay `nav.SetDestination`/`ActsPrep.*.Prep` en conductas (solo dentro del wrapper + lecturas de config). Falta reconciliar IA (que `AiBrain` conduzca) y sacar `ActsPrep` a data de arquetipo.
- **18-ago (PR #100):** **etapa 2 — semilla de `Locomotion`** (+ reorden del plan: `Locomotion` antes que `Forager`, porque `Feed` es sobre todo locomoción). Componente `Locomotion` (envuelve NavMesh + gait `ActionPrep`: `Walk`/`Run`/`Idle`/`Move`). `Animal` lo auto-añade en `Init`. Migrados los sitios emparejados (SetDestination+gait): `CorrectMedium` (agua↔tierra) y `Herbivore.Feed` (ir a comer + pastar). El resto (`Carnivore.Feed`, `Flee`/`Fight`/`HitAndRun`) coexisten con el patrón viejo hasta el siguiente PR.
- **17-ago (PR #98, docs):** ampliada `sanctuary-second-lap-and-fear.md` — S3 aire V2 = ciudad flotante (esfera de aire artificial) con insectos gigantes; S2 abisal V2 = laboratorio + burbuja prehistórica (megalodón/reptiles marinos/calamar gigante); S4 volcán V2 = quimeras + caracoles de fuego; S1 hielo CONFIRMADO (conejo→liebre ártica, +pingüino, +leopardo). **Santuario de la Magnate** (núcleo plasma+diamante): desierto artificial (sol/luna = plasma), puerta al infierno (desierto)/paraíso (plasma), demonios/ángeles, arco de Kushal (radar/evitar/misión secreta). **Amplificación V1 = `bonusPack`** (evento de entrenamiento, no multiplicador; falta enchufarlo a `Animal`). Australia por ubicar (¿cuarteles de jefes?).
- **17-ago (PR #97, docs):** `docs/sanctuary-second-lap-and-fear.md` — 2ª vuelta por los santuarios (V1→V2, inversión de poder, guerras, biomas por elemento, especies en peligro) + **miedo como efecto de hechizo** (magnitud+AoE, escalado por temple → stress + bond negativo; `auraFear` pasa a ser del hechizo) + control de AoE/potencia (futuro) + **hechizo de bloqueo de la Magnate** (cancela hechizos aterradores + daña al lanzador). Decisiones de especies/biomas **por confirmar**.
- **17-ago (PR #96):** **amenaza 100% stat-based**. Migrado el último punto ciego: la alerta de proximidad de `Escape` (antes `Rigidbody.mass × NavMeshAgent.speed`) ahora usa `EvaluateThreat` (Assess) + `alertReach` (tunable en `ThreatResponder`). Todo el sistema de amenaza (evaluación/decisión/detección/alerta) depende de stats; solo quedan las coroutines de acción (locomoción) para la etapa de `Locomotion`.
- **17-ago (PR #95):** **etapa 1 completada (assessment)** — `EvaluateThreat` → `ThreatResponder.Assess` plenamente stat-based (ratio `Predation.EffectivePower`, sin `rig.mass`/NavMesh; escala = fracción de mi poder, `ThreatThreshold` recalibrable). **Defensa de crías emergente**: retirado el flag `DefendsCubs` (base + 8 overrides) → sale del vínculo (cubBond) + autoabandono vs peligro. Dirección: el CUIDADO de crías (PostNatal) también debería volverse emergente en su etapa.
- **17-ago (PR #94):** **etapa 1 disolución — `ThreatResponder`**. Extraída la POLÍTICA de decisión luchar/huir/pegar-y-correr de `Animal.ResolveReaction` a un componente `ThreatResponder` (portable: `Predation.EffectivePower` + `autoabandono` + bonds). `Animal` lo auto-añade en `Init` y le pasa el contexto de crías (`defendingCubs`/`cubBond`, aún de `Family`/`Group`). `enum Reaction` movido. *Pendiente en etapa 1:* `EvaluateThreat`→`Assess` (tras recalibrar), detección y coroutines (con `Locomotion`).
- **17-ago (PR #93):** **plan de disolución de `Animal`** (`docs/anima-dissolving-animal.md`: disolver > extender+renombrar, kit-objetivo, qué método va a qué componente, orden por etapas) + **Fase 0 inicio** — `Animal.ResolveReaction` (fight/flee) migrado a `Predation.EffectivePower` (poder por stats + manada) × `(1+autoabandono)`, en vez de `rig.mass × NavMeshAgent.speed` + bucle de manada propio (scale-invariant, sin recalibrar). *Siguiente:* `EvaluateThreat` base y alerta de proximidad (cambian de escala → recalibrar con test).
- **17-ago (PR #92):** **rollout locomoción — animales (opt-in)**. El `NavMeshAgent` sigue navegando (pathfinding); si el animal lleva un `WalkSpell`, su `nav.speed` sale del hechizo cada tick (`Animal.FeedWalkSpeed` → `WalkSpell.StepSpeed`): correr (`Running`→channeling) sube gradual a la punta, andar decae, con la lógica del `powerBonus`. `ActionPrep.Prep` fija `Animal.Running` (¿es la acción de correr?). Auto-cablea base=walk.navSpeed, punta=run−walk. **Cero regresión** (solo cambia si se añade el WalkSpell). Los animales corren para huir/cazar/salir del agua/alerta-random. *Pendiente:* que la IA humana navegue por NavMesh con esta misma velocidad (AiBrain→NavMesh).
- **17-ago (PR #91):** **rollout locomoción — jugador**. `PlayerController` delega la VELOCIDAD horizontal al `WalkSpell` (carga-postura + punta + decaimiento + ATP), conservando dirección cámara-relativa, gravedad, salto y `_cc.Move`. `WalkSpell.StepSpeed(charging,channeling,moving,dt)` provee la velocidad sin mover. LeftShift parado = postura de salida; RightShift = punta. Builder añade el `WalkSpell` al jugador (base=walkSpeed, topes=sprint−walk). *Pendiente:* animales (NavMesh) al mismo hechizo.
- **17-ago (PR #90):** **locomoción universal** — `WalkSpell` conducible por brains. `SpellBase.TickPowerBonus(c,dt,charging,channeling)` explícito; `WalkSpell` gana `selfDriven`, `DriveFromInput()` (input propio) y `Drive(dir,...)` (programático). `PlayerBrain` conduce el WalkSpell si existe (ESDF + carga/correr); `AiBrain` lo conduce hacia `moveTarget` (rellena el hueco de locomoción IA). Andar/correr = un hechizo que cualquier brain mueve. Sandbox `WalkUniversal_AUTO` (caminante IA→destino). *Pendiente:* migrar el jugador principal (`PlayerController`) y los animales (NavMesh) a este hechizo.
- **17-ago (PR #89):** **pensamientos base de especie** (data-driven). `ArchetypeProfile.basePhrases` + `PhraseCategory.Especie` + `PhrasePools.Especie()` (Human/Bear/Wolf/Bunny/Fox/Deer/Seal/Whale/Lion/Malamute, tono por especie). Se siembran en `Mind.thoughts` al componerse: `SoulComposition.Resolve` (mente dominante) y `Animal.ApplySpeciesArchetype` (su especie). `Archetypes.BaseThoughtsOf` / `Mind.SeedThoughts`. NO hay clases de mente por especie: es el arquetipo (datos) el que declara stats mentales + pensamientos base.
- **16-ago (PR #88):** **animación de carga por hechizo** en `SpellBase` (`chargeAnimator` + `chargeAnimState`/`releaseAnimState`), disparada por `IsCharging`/`OnChargeReleased` — cada hechizo su animación (postura de salida del velocista, carga de la esfera…). `WalkSpell`/`FireSpell` auto-cablean el `Animator` local; los estados se nombran en el Inspector.
- **16-ago (PR #87):** **sistema de bonos de hechizo unificado** (`SpellBase`). Charge, channeling y forcejeo pasan a un único `powerBonus` que **decae con el tiempo**. `CastMode.Charge` **retirado**; charge=**LeftShift** (acumula sin aplicar; al soltar, burst que dispara), channeling=**RightShift** (suelo/tope dinámico `maxPowerWithChanneling`). Topes por hechizo (`maxPowerWithCharge`/`Channeling`/`Forcejeo`) escalados por aptitudes (físicas charge/forcejeo, mentales channeling). `WalkSpell` a **ESDF** con carga=postura de salida (arranque) + channel=punta. `FireSpell`: LShift carga la esfera gigante, RShift la sostiene. HUD/sandbox `SpellDemo_AUTO` actualizados.
- **14-ago (PR #86):** **jugador integrado en el modelo de ánimo/estrés**. `MoodDynamics`/`Exertion` ahora corren sobre **cualquier `Anima`** (el jugador ya lo era desde 2026-07-28): con `Mind` usan humores; **sin `Mind` (jugador)** derivan el estrés de las drives universales (`mentalFatigue`/`sleepiness`/hambre). En el builder el jugador lleva `MoodDynamics` y es `worker` de la receta de cocina → cocinar lo desgasta y estresa. Unifica el modelo (el jugador es «solo un Anima» más).
- **14-ago (PR #85):** **desgaste por trabajo (vía A)** `Exertion`/`ExertionCost`: las acciones de trabajo declaran coste (glucosa/minerales/fatiga/sueño) y gastan reservas del `Anima` → `MoodDynamics` lo vuelve estrés. Enganchado en `VirtualTask.worker`/`exertionPerStep`, `ProductionOrder.stepExertion[]` (cargar/forjar pesan más) y `StockingTask`. Sandbox `Desgaste_AUTO` (obrero Goluis que se cansa→estresa→`UnderPressure`).
- **14-ago (PR #84):** **dinámica de humores/estrés** `MoodDynamics`: el estrés (cortisol) es químico y **sube por el estado** (fatiga/sueño/hambre/reservas bajas), baja descansado; `Anima.stress` lo refleja. El mal humor de **Goluis es situacional** (`UnderPressure` emerge de `fatigue`/`stress`, ya no es un flag fijo). Enganchado en el núcleo de compañeros del builder. *Pendiente:* desgaste de reservas por acciones de trabajo (de pie/fuerza).
- **14-ago (PR #83):** **humores derivados de la personalidad** `HumorProfile.Apply`: los humores base salen de los stats (adrenalina ← sociability+creatividad+agilidad−disciplina; serotonina ← afabilidad+sociability; cortisol ← sensibilidad−composure; glucosa ← endurance+masa; calcio ← disciplina+composure). Se aplican al resolver el blend (`SoulComposition.WriteStats`, compañeros) y en `Animal.Init` (animales con `Mind`). Así el `SocialField` **diferencia las actitudes solo** (Gohageneis alto adrenalina/serotonina → fiesta; Panterilia más cortisol → calma). *Falta:* reducir los componentes bespoke de compañero a lo puramente mecánico.
- **14-ago (PR #82):** **(c) campo social global emergente** `SocialField`. Cada anima **contagia su ánimo** (sube serotonina/adrenalina) a las animas cercanas **con bond ≥ umbral y sin threat**, escalado por su `sociability` × (positividad+energía de sus humores). Generaliza `ThoughtField`; el jugador es una anima más. Enganchado a los compañeros (en `MakeCompanionCore`) → su "actitud" (fiesta/calma) **emerge** de sus stats/humores/bonds en vez de hardcodearse. *Falta:* derivar los humores por-compañero de su perfil (hoy default) y reducir los componentes bespoke (celebración/observación) a lo puramente mecánico.
- **14-ago (PR #81):** **(a) autoabandono derivado + (b) huir/ayudar por bonds+threat+autoabandono en `Animal`**. `Autoabandono.From(anima)` = entrega/(entrega+autoconservación) [entrega ← afabilidad+sensibilidad+bond medio; autoconservación ← composure+disciplina+instinto], ya no un campo crudo; `Anima.RecomputeAutoabandono` (llamado en `Animal.Init` y al decidir). `Animal.EvaluateThreat`: el **bond baja la amenaza** (bond 100 → amenaza 0, confianza no huida). `ResolveReaction`: el `autoabandono` envalentona (myPower×(1+autoabandono)) y por las crías planta cara si **(autoabandono + vínculo) > peligro** (desventaja). *Falta:* ayudar a un aliado LEJANO en peligro (navegación de `PackAwareness`); unificar con las hormigas.
- **14-ago (PR #79):** **`CompanionBase` RETIRADO (fase 5 completa)**. Borrado `CompanionBase`; su maquinaria compartida (vínculo con el jugador + mood/fatiga + anchors + efecto de proximidad) → componente **`MoodState`** (parametrizable por curvas, `IBondable`). Goluis/Panterilia/Gohageneis/Irosene reescritos como componentes finos (`: MonoBehaviour`) que solo llevan su **conducta propia** (presión+resistencia / bonus de observación / burst de celebración / motivación+diálogo) leyendo de `MoodState`; sus stats vienen del arquetipo (`SoulComposition`). `BuildCharacters` recableado (SimpleAnima+SoulComposition+Mind+MoodState+comportamiento) con las curvas/anchors por compañero; `MigrationDiagnostics` usa `MoodState` en vez de `is CompanionBase`. Radio de tipo mínimo (resto eran strings/comentarios). **Refactor grande → validar en Unity.**
- **14-ago (PR #78):** **captura de los 4 compañeros como arquetipos** (fase 5, aditivo). Goluis/Gohageneis/Irosene (+ Panterilia ya estaba) tienen ahora arquetipo cuerpo+mente con sus **`Base*` reales** (incl. `adaptability` — `MakeBody` ahora la acepta, default 1). Sandbox: `Goluis_SinClase`/`Gohageneis_SinClase`/`Irosene_SinClase` (+`Panterilia_SinClase`) = `SimpleAnima`+`SoulComposition`(su arquetipo)+`Mind`+`BondPillar`+speciesBonds Human → reproducen a la companion **sin heredar `CompanionBase`**. No se retira `CompanionBase` todavía (siguen en uso). *Falta (con compilador):* mover su `Update` de estado interno a un pilar, recablear su creación + UI de bonds, y retirar `CompanionBase`.
- **14-ago (PR #77):** **openness + efecto del bond**. `Archetypes.NetDisposition(especie)` (suma de sus relaciones) + `SpeciesKarma.Openness(me)`: al conocer una **especie nueva** (sin relación específica) el arranque se resuelve por la **disposición GENERAL** del ser (si en total sus relaciones son + o −). **Efecto** en `BondPillar`: la buena compañía **calma** (baja `stress`), la mala **inquieta** (lo sube) — proporcional a la relación con cada vecino; sigue reconfortando la mente del vecino por el bond. `stressEasePerPoint`.
- **14-ago (PR #76):** **base kármica por especie (`speciesBond`)**. Arquetipos de relación (`Archetypes.RelationValue` / `_relations`: foca→oso −40, conejo→lobo −50, perro↔humano +45, lealtad de manada…), `SpeciesKarma.RelationOf` (mezcla los `speciesBonds` del ser por dominio, o su `Anima.SpeciesName` directa), `SoulComposition.speciesBonds` (mezclable), y `BondPillar` **arranca el bond por karma** al conocer a alguien (solo la positiva siembra confianza; la negativa la lleva el THREAT por poder, separado). `Anima.SpeciesName` (Animal→su arquetipo). Especie nueva (lobo↔komodo) = 0. *Falta:* karma negativa→amplifica threat; circunstancias (ayuda/depredación); openness.
- **14-ago (PR #75):** **corrección — `BondPillar` universal (sin vía directa al jugador)**. Reescrito: cada `Anima` **familiariza con cualquier `ITarget` cercano según las circunstancias** (cercanía) usando el sistema de bonds **universal de `Anima`** (`GrowBond`/`GetBond`, etapa×trauma×aura) — el jugador es un `ITarget` más (`PlayerTarget`), no un caso especial. Se quitó el `bondWithPlayer`/player-first copiado de `CompanionBase`. La "comodidad" a la mente de un vecino escala por el bond mutuo. *Falta:* base kármica por especie (foca↔oso−, perro↔humano+) como punto de partida (soul-relations §2).
- **14-ago (PR #74):** **migración fase 5 (preparada) — disolver `CompanionBase`**. `Soul/BondPillar` implementa `IBondable` (vínculo con el jugador + efecto de proximidad, extraído de `CompanionBase`) como **componente** — igual patrón que `WorldBondable`. Así un compañero = `Anima + SoulComposition + BondPillar` (+ `Mind`), **sin clase especial**. Arquetipo `Panterilia` (cuerpo+mente con sus 12 valores reales). Sandbox: `Panterilia_SinClase` reproduce a Panterilia sin heredar `CompanionBase`. Aditivo (no toca los compañeros actuales). *Falta:* migrar las 4 companions a este patrón y **retirar** `CompanionBase` (con compilador/coordinación); mover su `Update` de estado interno al pilar.
- **14-ago (PR #73):** **migración fase 3 (mitad segura) — animales por arquetipo**. `Animal.Init()` llama a `ApplySpeciesArchetype()`: llena las 10 aptitudes NO gestionadas por `Base*` (fuerza/masa/aguante/adaptabilidad + las 6 mentales) desde el arquetipo de especie; `SpeciesArchetype` overridado en las 8 especies (Bear/Wolf/Fox/Malamute/Bunny/Deer/Seal/Whale). Antes los animales tenían aptitudes planas (todas 1); ahora difieren por especie (alimenta emoción/Mind/DerivedStats/magia). Respeta `agility`/`perception` (que las maneja `Base*`+evolución) y NO toca el huir/predación (siguen por `Physiognomy`/`rig.mass`). + arquetipos Fox/Deer/Seal/Whale/Malamute (cuerpo+mente). *Falta (mitad delicada, coordinar):* hormigas→`Animal`; que huir/predación lean el stat `bodyMass`; disolver `CompanionBase` (fase 5); mando (fase 4).
- **14-ago (PR #72):** **migración fase 2 — Mente por blend**. `SoulComposition.WriteStats` resiembra `Mind.aptitudes` con el resultado del blend (en Resolve/ConvertTo/SharedSoul) → el **tono y las decisiones** de la `Mind` **emergen del blend** (`Mind.PickTone` deriva de aptitudes; se resiembra porque su `Awake` corre antes del `Resolve`). Sandbox `AlmaBlend_AUTO`: `OsoMenteHumana` y `Oso_bonusPack3` llevan `Mind` → el de mente humana piensa Viento/Fuego, el de mente-oso Tierra. Testing §20. *Siguiente (fases 3-5):* especies→cuerpos reusando huir/manada de `Animal` (coordinar con el compañero), mando (mente interrumpe body), disolver `CompanionBase`.
- **14-ago (PR #71):** **`SharedSoul` — alma compartida entre reencarnaciones**. Varios cuerpos (`SoulComposition`) referencian una sola alma: comparten **forma** (identidad, canónica) + **poder** (magnitud) + **bonds**. `Register`/`ApplyTo` (convierte la forma al cuerpo, Literal/B, × poder), `GainPower` (entrenar/lesionarse → propaga a TODOS), `ReshapeFrom`, `AddBond` (lo tienen todas las reencarnaciones). `SoulComposition` + `sharedSoul` + `ComputeBaseStats()`. `SoulMath.Scale`. Sandbox `AlmaCompartida_AUTO` (melaza Toro+Bear / hormiga Ant+Human, una alma) + HUD `SharedSoulDemo` (entrenar/lesionar/+bond). Testing §21. Cierra la prioridad "bonds acumulables + conexión espaciotemporal". *Falta:* propagación PEREZOSA por era (hoy propaga a los cuerpos registrados en escena).
- **14-ago (PR #70):** **conversión + blend por DISTRIBUCIÓN**. `Soul/SoulMath` (`Remap` A/relativa y B/literal; `RescaleShape`; `Physical`/`Mental`/`All`). `SoulComposition` reescrito: el blend ahora es por **distribución** (cada arquetipo reescalado al presupuesto del primario → un 1% empuja la forma un 1%, ya no despreciable) + `ConvertTo(cuerpo,mente,modo)` (transformación/reencarnación: reexpresa la identidad actual en la base nueva; **B para reencarnaciones**) + `ReadStats`/`WriteStats`. Arquetipo `Ant`. Sandbox: `Ambrosio_Convert` (Toro+Bear) con HUD `SoulConvertDemo` (A/B/reset). Testing §20. *Siguiente:* `SharedSoul` perezoso (propagación de stats/bonds entre reencarnaciones).
- **14-ago (PR #69, doc):** **`soul-relations-reincarnation.md`** — diseño avanzado del alma: (1) blend/conversión por **DISTRIBUCIÓN** (una función reusable; tu fórmula simplifica a "reescalar cada arquetipo al presupuesto de la base × dominio"; sirve para blend Y transformación; decisión abierta A/B); (2) **relaciones por especie/karma** (`speciesBond` mezclables; base evolutiva foca↔oso−, perro↔humano+; `openness` para especies nuevas; → inclinación/thoughts/autoabandono); (3) **pensamientos por capacidad** (`floor(cap·dom/100)`, umbral entero; 2 pools; coste `K`); (4) **reencarnación por ALMA COMPARTIDA** (varios cuerpos = una anima; stats/bonds se propagan; lesiones→reinicios; nombres idénticos; **perezoso** por era). *Siguiente:* cerrar sabor de conversión → `SoulMath.Remap`.
- **13-ago (PR #68):** **alma por MEZCLA — FASE 1 (el motor)**. `Soul/BlendSlot` (arquetipo + dominio % +
  `shareDomain`), `Soul/Archetypes` (perfiles en código: cuerpos Human/Bear/Wolf/Bunny/Lion/Toro/Gallina/Mono +
  mentes Human/Bear/Lion/Rock/Fire/Agua/Mono + bonusPack1-4), `Soul/SoulComposition.Resolve()` (escribe las 12
  aptitudes: físicas←blend(cuerpos), mentales←blend(mentes) + Σ bonusPacks; + tamaño por altura mezclada). Sandbox
  `AlmaBlend_AUTO` (Panterilia Human90+Lion5, oso-mente-humana, oso+bonusPack3). Testing §20. **Nada migrado aún:
  esto es el MOTOR.** *Siguiente (fases 2-5):* migrar especies→cuerpos (reusar huir/manada de `Animal`), tono/
  thoughts a `Mind`, semilla de `FamilyGenerator` por blend, disolver `CompanionBase`, mente activa interrumpe body.
- **13-ago (PR #66, doc):** **`soul-composition-blend.md`** — arquitectura de **alma por MEZCLA**: cada ser se
  compone de arquetipos de **cuerpo** y **mente** con **dominio (%)** + `shareDomain`; stats/tono/thoughts finales
  = suma ponderada. Añadir/quitar arquetipos = híbridos/reencarnación/transformación. Reconcilia la corrección de
  arquitectura: **el diseño unificado (Anima raíz + pilares) está documentado pero a medio migrar**; `Animal` YA
  tiene huir+manada (las hormigas en `SimpleAnima` lo saltan — a corregir); especies = cuerpos; `CompanionBase`
  se disuelve; mente activa interrumpe body (vía Control/`IBrain`). Plan por fases + decisiones (arquetipos en
  código; array+shareDomain). *Siguiente:* fase 1 = `SoulComposition.Resolve` + `Archetypes` (blend de aptitudes).
- **13-ago (PR #65, doc):** **`microcosmos-level1.md`** — consolidación + **fichas del elenco** del Nivel 1
  (Sakshi/Ambrosio/Medea/Momo/Héspero/Ruth/Atlas + ancianos: alma·aptitudes·firma emocional·relaciones·impulsos·
  beat·tell). Decisiones: **emergencia dirigida** (los mueven stats+thoughts; el director solo siembra
  circunstancias/pensamientos, estilo `MobWorldDirector` — no marioneteo); **composición, NO clase `Ant`**
  (hormiga = `SimpleAnima` + IA del compañero + `Mind`/`SoulRecord`/emoción + ficha; el sistema unificador ya
  está: Anima raíz + pilares por composición + quark→elemento→compuesto→stat). Lista de impulsos sociales que
  faltan + beats + orden de construcción. *Siguiente:* aplicar fichas a las hormigas (coordinar con el compañero).
- **13-ago (PR #64):** **Forcejeo/Channeling unificados en `SpellBase` + `FireSpell`→`SpellBase` + sandbox**.
  `SpellBase`: dos bonos de poder — **forcejeo** (físico; sube solo al no lograr el efecto, `ReportResult`;
  persiste) y **channeling** (mental; sube al canalizar con `channelKey`/Shift, decae al soltar), `BonusPower` =
  suma escalada por aptitudes. `FireSpell` migrado a `SpellBase`: **Repeat** (mantener G = fuego múltiple, cada
  llama sube el forcejeo) + **Charge** (G+Shift = canalizar → soltar Shift dispara cargado); el bonus multiplica
  la intensidad. `WalkSpell` reescrito sobre los bonos (forcejeo al bloquearse, Shift=esprint) + `SpellDemoHUD`.
  Sandbox **`SpellDemo_AUTO`**. Testing §19g. *Falta:* detección de impacto real para el forcejeo del fuego;
  propagar forcejeo/channeling a `PullSpell`/`TransformationSpell`.
- **13-ago (PR #63):** **`CastMode` en `SpellBase` + `PullSpell` fusionada + `WalkSpell`**. (1) Arreglado el
  **choque de compilación**: había dos `class PullSpell` (Microcosmos + Transformation) → CS0101; borrada la
  huérfana (`Transformation/PullSpell.cs`, NavMesh, sin cablear). (2) `SpellBase` + `CastMode`
  (Instant/Repeat/Channel/Charge) + `PollInput()` opt-in + hooks `OnCast*` → mantener-tecla configurable por
  hechizo (fireball=Repeat, jalar/caminar=Channel, transformación=Charge/Instant). (3) `PullSpell` **fusiona**
  las dos verdades: forcejeo emergente por `ImpulseController` (Micro) + física por stats (tope = `force+Strength`,
  arranque = `max(force, masaObjetivo)`, sube solo si NO hay **progreso hacia el caster**, ambos gastan ATP ∝
  fuerza). (4) `WalkSpell` (contraparte a uno mismo: mínimo para mover el propio peso, sube si bloqueado, cansa).
  *Falta:* cablear `WalkSpell` a un sandbox; migrar `FireSpell`→Repeat / `TransformationSpell`→Charge si se quiere.
- **12-ago (PR #62):** **Sistema de hechizos Nivel 1 Microcosmos** (hormiga / debilitamiento).
  `SpellBase` (abstracta: `range`/`force`/`duration`, `CanCast`/`Cast`, `InRange`, `EffectiveForce` = fuerza+Strength−masa-target−resistencia).
  `WeaknessEffect` (hechizo de vejez; drena `currentEnergy` a ritmo fijo; deshabilita `NavMeshAgent` en 0;
  el drenaje sigue aunque se rellene la energía externamente; `Cancel`/`Resume`/`ApplyImmediate`).
  `PullSpell` ("Jalar": primer hechizo real del jugador; arrastra `ITarget` hacia el caster; física vs masa +
  velocidad-de-huida como resistencia; coroutine desactiva el agente durante el arrastre).
  `HoneydewSpell` ("Maleza de Ambrosio": **primer consumible del juego**; restaura `currentEnergy` vía
  `CharacterLevel`; auto-uso si no hay target; interactúa con `WeaknessEffect.Resume()`).
  `FormicAcidSpray` (habilidad corporal, **no magia**; AoE de estrés en radio; cargas limitadas con recarga
  gradual; respeta facción; tecla Q).
  `CombatTargetSelector` generalizado de `IngredientMob` a `ITarget` (backward compat: `CurrentIngredientMob`,
  `SelectMob`, `SelectAndOpenPalette`, `OnMobTargetChanged`; `GetSortedTargets` busca todo `MonoBehaviour`
  que implemente `ITarget`). **Sync**: ambos repos (GitHub + COLD-SANCTUARY).
- **10-ago (PR #61):** **topes de reservas derivados de los STATS** (no escala fija por santuario; stats-as-truth).
  `MagicReserves.EffectiveCapPerElement` = base × `MaxHealth/100` (materia ← resistencia+fuerza+masa) y
  `EffectiveEnergyCap` = base × `MaxMana/50` (energía ← razón+memoria), × niveles de alma (`CharacterLevel` opc.);
  `Store`/`StoreEnergy` ya los usan. Quarks sin tope (los limita lo reunido). Sandbox `Magia_AUTO`: botón "Subir
  stats" + lectura de topes efectivos. Doc §16, testing §19f. Cierra el pendiente de "escalado de topes".
- **07-ago (PR #60):** **hechizo de abastecimiento / "trasplante"** `SupplySpell` (generaliza el `ChimeraFeed`):
  el mago se llena de quarks/energía y los **introduce en otro personaje** (energía+elementos+quarks de sus
  reservas → las del objetivo). Sirve para quimeras, otro jugador, uno mismo y sobre todo **rol HEALER en la
  guerra**. Sandbox: `Magia_AUTO` + nuevo `Magia_AUTO_Objetivo` + botón «Abastecer objetivo». + doc §16 (abastecer
  = trasplante; regreso a S1 reconstruido + reparto de magos débiles→S4/fuertes→S1; osos entrenados por quimeras).
  Testing §19c. *Falta:* que abastecer quimera suba sus Humores+sacie Metabolism (domesticación); UI de objetivo.
- **07-ago (PR #58):** DECISIÓN (doc): **se descarta el gate de energía por física** (`energyPhysicsId` +
  `OnLearned`). El jugador **entra a cada santuario con la química/biología/física necesaria** (la aprende al
  terminar el anterior), así que la energía de la descomposición se capta **por defecto** (el campo queda como
  palanca opcional). Docs `magic-metabolism §14/§16` + `testing §19d`.
- **07-ago (PR #57):** **hechizos elementales basicos** `ElementalSpell` (agua/tierra/viento) con perfiles de
  coste distintos: agua (H2O ~1kg: 11%H+89%O + ~200J), tierra (SiO2 ~5kg: materia-pesado + poca energia), viento
  (materia 0 = aire gratis + ~450J = energia-pesado). Coste energia = 1/2 m v2. Junto al `FireSpell` completan los
  4 basicos que quedamos en probar. Anadidos al HUD `Magia_AUTO` (+ boton "Cargar reservas de prueba"). Doc §16,
  testing §19c. *Falta:* 1 hechizo por elemento + contras para los elementales del S4.
- **07-ago (PR #56):** **`Magia_AUTO` con `Anima` real**. `SimpleAnima` (Anima concreto minimo: hooks no-op, sin
  Awake/Start → seguro en sandbox) enchufado al `Magia_AUTO`; ahora comer sube stats via `Constitution` (gradual)
  y el exceso→grasa (`fatReserves`), ademas de rellenar las pools. `MagicSandboxDriver` muestra fuerza/masa/grasa.
  Testing §19a. (Topes derivados de stats: hecho PR #61.)
- **07-ago (PR #54):** **sandboxes de prueba** en `SampleSceneBuilder` (salen con `Build Sample Scene
  Blockout`): **`Descomposicion_AUTO`** (`DecompositionJob` workerCut=0 + `DecompositionMinigame` con batch
  agua/sal/CO₂ → al terminar la jornada suben Elements/Energy en el HUD de recursos; no necesita `Anima`) y
  **`Magia_AUTO`** (cápsula con `Metabolism`+`Constitution`+`MagicReserves`+`QuarkReserve`+`Grimoire`+`FireSpell`
  + `MagicSandboxDriver`, HUD OnGUI: aprender 1er hechizo → comer/quarks rellenan pools → lanzar fuego químico/
  masa-energía; sin `Anima`, null-safe). Guion en `testing-checklist §19`. *Falta:* `Magia_AUTO` con `Anima` real
  (grasa/stats), abastecer-quimera (Humores+Metabolism; base `SupplySpell` hecha). (Topes de stats: hecho #61.)
- **07-ago (PR #52):** **minijuego de descomposición** (`DecompositionMinigame`). La jornada corre 3 FASES sobre
  las MISMAS muestras y el MISMO orden: (1) **identificar** —elegir el nombre correcto entre nombres al azar,
  contra reloj—, (2) **romper** —calidad por timing 0..1 → energía capturada—, (3) **clasificar** —arrastrar
  cada componente a su contenedor; al acabar el tiempo se guardan los bien clasificados y pasa a la siguiente.
  Un componente cuenta en el `yield` si su muestra fue identificada+rota+bien clasificada (×calidad); al terminar
  vuelca `yield`/`energyJoules` en `DecompositionJob` y `Complete()` reparte economía+paga. Más rápido = más
  muestras/jornada = más progreso. Prototipo OnGUI. Doc §17. *Falta:* UI de arrastre real; meltdown; sembrar el
  batch desde la materia del área.
- **07-ago (PR #51):** **sustrato de quarks del S4 (`QuarkReserve`) + aclaraciones del modelo**. `QuarkReserve`:
  quarks crudos flexibles → `MakeElement` (quarks→gramos de cualquier elemento, para el MIX pre-crear/dinámico)
  y `Restitute` (quarks→energía, E=mc²) + `AtomsAvailable` (UI: átomos por elemento). Identidad **1 g ≈ 3·N_A =
  1,8×10²⁴ quarks** (independiente del elemento). + doc §16 (todo hechizo gasta elementos **y** energía; 2 vías
  de llenar energía: cocina-separación vs restitución; el MIX de pools flexibles; topes progresan → S4 debe
  poder crear una casa 10⁷ g; alimentar quimeras cuesta aparte —quarks/elementos+energía—; elementales = 1
  hechizo por elemento con perfiles de coste) + §17 (**minijuego de descomposición**: romper-por-timing +
  clasificar-por-carriles tipo Guitar-Hero → setea `yield`/`energyJoules` de `DecompositionJob`). *Falta:*
  casteo que tire de quarks al faltar elemento. (`DecompositionMinigame` PR #52; `SupplySpell` PR #60; topes de
  stats PR #61.)
- **07-ago (PR #50):** **`FireSpell` con coste físico REAL** + cálculo del S4. `FireSpell`: coste = potencia×
  tiempo → energía → **combustible** (C 85,7%/H 14,3%; O₂ del aire gratis) + ignición; presets `FireTier`
  (chispa 0,018g / lanzallamas 22g / aliento de dragón 2,2kg **inviable** por química). **Modo masa-energía**
  (S4): aniquila µg + paga toda la energía del pool (E=mc²). Pinta aura destructiva. + doc §15: **quarks del S4
  llenan LOS DOS pools** (bio `Constitution` + magia `MagicReserves`) y el excedente→energía; tabla de
  supervivencia (1 quimera ~10¹⁰J≈100 alientos al entrar; grupo ~10¹¹J≈1000 al terminar; 1 g de materia≈900k
  alientos → la masa NO es el cuello de botella, sí `energyCap`+maestría); quimeras (dragón/hidra/elemental) =
  los "osos" del minijuego de domesticación. *Falta:* escalar `energyCap` por nivel; enganchar Quimera a `PlayableCreature`.
- **07-ago (PR #49):** **trabajo de descomposición + coste de energía en hechizos**. `DecompositionJob`: al
  completar la misión de la cocina reparte el lote → **mayoría a la economía** (`SanctuaryResources` +enum
  `Elements`/`Energy`) + **`workerCut` de paga** al obrero (`MagicReserves`); la **energía solo la capta el
  obrero si conoce la física** del nivel (`Grimoire`/`energyPhysicsId`), si no va toda a la economía. Enum
  `DecompositionLevel` (catabolismo/ionización/fisión/aniquilación). **Coste materia+energía**: los hechizos
  declaran `cost`(elementos)+`energyCost`(J) y pagan con `MagicReserves.Pay(costs, energy)` (todo o nada);
  `TransformationSpell`/`PossessionSpell` ya lo usan. *Falta:* minijuego que rellene el lote (hecho, PR #52).
  (Gate `energyPhysicsId` por física: **descartado** — entras a cada santuario con la física necesaria, §14.)
- **07-ago (PR #48):** **pools de magia + primer hechizo + reserva de energía**. `Grimoire` (registro de
  hechizos aprendidos, `OnLearned`): el 1er hechizo `awaken-reserves` llama a `MagicReserves.Unlock()` → crea
  las pools vacías con tope (siembra H/C/N/O). `MagicReserves`: + reserva de **energía (julios)**
  (`energy`/`energyCap`/`StoreEnergy`/`PayEnergy`) — la que libera **descomponer** materia (química→nuclear→
  masa-energía), paga los hechizos grandes (bosones). + doc `magic-metabolism §14`: cocinas = trabajo
  (descomposición→energía+materia→economía+paga), física en la cocina, catabolismo/fisión/aniquilación (no
  "catalización"), revelación de energía gateada por física de otras áreas. *Falta:* `DecompositionJob`
  (reparte a `SanctuaryResources` + cut al obrero); gatear la absorción de energía por física aprendida.
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
