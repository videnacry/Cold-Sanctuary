# Checklist — continuar

Tablero para retomar. Última sesión: 2026-07-23. Marca lo que completes.
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
      `BodyPartStats` (`flex`/`str`). Consolidar en `LivingEntity` al implementar `NPCBase`.
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
- [ ] **Modelo de personaje unificado (`NPCBase : LivingEntity`)**: hogar de stats + `ITarget` + población +
      aptitudes (prerequisito para que companions/otros NPCs sean presa). **Control aparte** (cerebro
      enchufable: input o IA) para que todo NPC sea jugable/intercambiable. **No** heredar de `Animal`.
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
      `PlayableCreature` (tensión = `LivingEntity.stress` o local) + `PlayController` (tecla V);
      al serenarse suelta recursos (`SanctuaryResources`) + monedas; `IInteractable` "dar comida y agua"
      → sacia (`fatReserves`) y descansa. **V2 feel gato/perro hecho:** excitación/combo (descarga
      escala con excitación), **atrapada** (quedarte pegado resetea el combo), reacción de la criatura
      (te mira/se acerca, con correa) + rebote de escala. Sandbox en `SampleSceneBuilder`.
      **Falta (V3):** tabla de drops + consumibles/artefactos (`Inventory`/`ItemData`), **XP → leveling de
      personajes** (+vida/+maná; sistema aparte inexistente), y generalizar el target de combate.
- [ ] **C — Teletransportador "aeropuerto".** `SanctuaryTeleporter` bidireccional entre santuarios
      (lava↔subterráneo primero); reusa `MobWorldLoader`.
- [ ] **D — Construcción en el tiempo.** `Construction`: progreso consumiendo recursos+tiempo (lento en
      Meso, rápido en Macro); 4 áreas + 1 en obra. Depende de A.
- [ ] **E — Macrocosmos (RTS).** Unidades/roles (peón/héroe), árbol de construcción, cámara 2D-ish,
      IA de guerra, agujero-negro de estructuras. Sistema grande; al final.
- [ ] **Decisión clave:** modelo de guerra en modo Meso (encarnada recomendada vs tiempo comprimido) — §9.

## Hecho (2026-07-23)
- [x] Sincronización de docs con el código (cifras, 3 sistemas nuevos, flips de estado).
- [x] **Cocina jugable** (Microcosmos escena): `MissionEndMode.Standalone`, `MobWorldMission`, misión
      "Procesar ingredientes" incrustada en `MobWorldSceneBuilder`.
- [x] **Limpieza legacy**: borrados `KitchenScaleController`/`KitchenEntrance` (+método muerto del builder),
      `IAnimal`/`IVital`. Restaurados `Generator`/`BodyPositionData`. (Perdido `NewFile1.txt`, sin trackear.)
- [x] **Renombrado** plano mágico → **Microcosmos**; tríada **Micro/Meso/Macrocosmos** en docs.
- [x] **Diseño del mundo grande**: `world-topology-and-planes.md` (planos, 5 santuarios, núcleo de 2 capas,
      economía/farming/transporte/cámara del Macrocosmos, arco final con Leo).
- [x] **A (1ª pasada)**: backbone de recursos de santuario + HUD (ver sección "Mundo grande").

## Hecho (2026-07-09/10)
- [x] Auditoría completa + sincronización de 18 docs con el código.
- [x] Retirada de la mecánica de disparo/`Shooted` (santuario no-violento).
- [x] Falso positivo `lp` corregido; `sensibility` inicializado.
- [x] `agility`/`perception` en animales + companions; `adaptability` añadida; Goluis/Panterilia/Gohageneis calibrados.
- [x] Docs de diseño: creature-stats, mission-mode, learning-unlocks, gaps-vs-planteamiento.
- [x] Set de aptitudes cerrado (12) + perfiles calibrados; distinción percepción práctica vs académica.
- [x] Modificadores de medio (tierra/agua/aire): `Medium` + afinidades + `EffectiveAgility`; ballena/foca calibradas.
- [x] Bucle de evolución de aptitudes (animales): `AptitudeEvolution` + tick en `Animal.Restore`.
- [x] `MediumZone` (detector de medio); dietas revisadas (árbol trófico, incl. lobo→oso en manada); fix de amontonamiento en `Homebound`.
- [x] Comportamiento agua/tierra (`Animal.CorrectMedium`): acuáticos buscan agua, terrestres salen.
- [x] Husky → Malamute (rename + dimensiones: masa 36, escala 0.185); jugador (`PlayerTarget`) como presa de oso/lobo.
- [x] Territorialidad (comportamiento): `Animal.SenseThreats` — presas huyen de carnívoros (revive EvaluateThreat/ThreatThreshold).
- [x] Malamute como mascota (fuera de `nestSpecies`; Deer entra para dar presa al lobo).
- [x] Banco de peces como organismo (`FishSchool`): mueve/huye/crece + `ITarget`/`IEdible`; en dietas de oso/zorro.
