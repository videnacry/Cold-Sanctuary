# Checklist — continuar

Tablero para retomar. Última sesión: 2026-07-10. Marca lo que completes.
Contexto de fondo: [`AUDIT-2026-07-09.md`](AUDIT-2026-07-09.md), [`gaps-vs-planteamiento.md`](gaps-vs-planteamiento.md).

## Decisiones abiertas (rápidas)
- [x] **Aptitudes adicionales**: set cerrado — `endurance/reasoning/memory/creativity/sociability/discipline`
      añadidas a `CompanionBase`; `flexibility` → `BodyPartStats` (pendiente de conectar).
- [ ] **Economía circular** (aprobada): cerrar la tabla final residuo→subproducto→área
      (ver [`mission-mode.md`](mission-mode.md)).
- [ ] **`Generator.cs`**: ¿borrar? (legacy redundante con `FamilyGenerator`).
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
- [ ] **Modificadores de medio**: detector de medio para animales (setear `currentMedium`); afinidades
      para humanoides (nadar) al llegar `NPCBase`; extender `MediumFactor` a `strength`/`endurance`.
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
- [ ] **Modelo de personaje unificado (`NPCBase`)**: hogar de stats + `ITarget` + población + aptitudes;
      prerequisito para que companions/otros NPCs sean presa. Crear las poblaciones de personajes aquí.
- [ ] **Montaje de escena** (`SampleSceneBuilder`): crear **nidos/madrigueras primero**, luego poblar
      familias en torno a ellos, con nidos **fuera del alcance de depredadores**. Ver [`refuge-and-adult-behavior.md`](refuge-and-adult-behavior.md).
- [x] **Dietas** revisadas (todos los carnívoros): oso +ciervo/zorro/malamute/**humano**; lobo +zorro/malamute/oso(manada)/humano; zorro/malamute sin cambios.
- [ ] **Banco de peces como organismo**: `FishSchool` → entidad viva que se mueve, huye, crece en nº (`lp`)
      y es presa (oso/foca/zorro). Barato (1 entidad/banco). Ver [`refuge-and-adult-behavior.md`](refuge-and-adult-behavior.md).
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
- [ ] **Cocina**: con el trigger-personaje, colapsar a **una sola ruta de control** (interacción) y
      quitar/guardar el auto-trigger por collider (evita la desincronización).

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
