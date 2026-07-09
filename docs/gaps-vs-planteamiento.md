# Brechas: "funciona" ≠ "cumple el planteamiento"

Sistemas que el código marca como hechos/funcionando pero que **aún no cumplen del todo la
intención de diseño**, más **cómo cablear** los sistemas huérfanos completos. Derivado de la
auditoría (ver [`AUDIT-2026-07-09.md`](AUDIT-2026-07-09.md)). Fecha: 2026-07-10.

## A. Sistemas "hechos" que no cumplen el planteamiento

| Sistema | Estado código | Qué falta para cumplir el planteamiento |
|---|---|---|
| **Post-natal** | funciona | `nestType`/`fatherRole`/`weaningType` se configuran pero **nunca se leen**; roles de manada (Guard/Provider/AlloNurse) inertes; el "letargo" del oso es solo duración+grasa, no un sistema real; el comportamiento por pesos (`MotherBehaviorSet`/`OffspringBehaviorSet`) del diseño está hardcodeado. |
| **Asanas** | funciona y conectado | el diseño pide 4 estados de calidad (con `Wrong`) y 2 tramos de gap; el código tiene 3 sin `Wrong` → **no hay penalización por postura incorrecta**; `AsanaDetector.ExecuteShortcut()` no existe; el feedback del profesor (`TeacherNPC`) no está cableado. |
| **Química** | funciona | el código dice "118 elementos"; solo hay ~55 → Pokédex incompleto. |
| **Combate** | funciona (jugador) | sin animaciones/VFX/audio (13 TODOs); `NPCCombatBehavior` sin cablear; la barra no se refresca en vivo → el desbloqueo por aprendizaje no se refleja en partida (ver [`learning-unlocks.md`](learning-unlocks.md)). |
| **Cocina** | funciona | sin transición cinemática (TODO `RequestRobbery`); dos rutas de control (collider + interacción) que pueden **desincronizar** el estado (ver [`mission-mode.md`](mission-mode.md)). |
| **Misiones** | parcial | `IngredientCollection`/`YeastControl` sí; **`AreaClear` no cierra el loop** (falta `KitchenCombatManager`; `ReportMobProcessed`/`ReportAreaCleared` sin llamar). |
| **Cámara** | funciona | el "modo artístico" (paletas Sueño/Estrés/Alegría) no existe; `ScreenEffects`/`BlackoutPulse` es solo `Debug.Log` → efectos de sueño/estrés no se ven. |
| **Ropa/crafting** | presente | **loop roto end-to-end**: nunca entrega el item (`ClothingRecipe` sin `resultItem`), no consume materiales (`ConsumeFragments` stub). |
| **Bond activities** | funciona | el jugador **no puede iniciarlas** aún (`Practice()`/`BuildPaletteConfig()` sin cablear a UI); `Goluis.resistanceBuilt` se acumula pero nunca se consume. |
| **Observación** | float suelto | solo existe `observationRadius`; señales diegéticas, pensamientos y modo contemplativo del diseño **sin construir**. |
| **Materialización de bloques** | codificado | **inalcanzable**: falta `BlockSpellEvaluator` que ponga `PaletteResult.spatial` → ningún hechizo espacial funciona. |
| **Aptitudes (agility/perception)** | recién añadidas | valores base calibrados, pero **no evolucionan aún** por tareas/tiempo (ver [`creature-stats.md`](creature-stats.md)). |

## B. Cómo cablear los sistemas huérfanos completos

Documentados como "implementado, pendiente de cablear" — **no borrar**.

| Sistema | Cableado que le falta |
|---|---|
| `TeacherNPC`/`MaestraTeacher` | Suscribir `EvaluateResult` a `Palette.OnFormulaEvaluated` (paleta de asanas); cambiar `Say()` (`Debug.Log`) por `DialogueManager.Play(secuencia)`. |
| `BondActivityManager` | Abrir la paleta con `BuildPaletteConfig()` desde input/UI y enrutar el elemento elegido a `Practice(id)`; consumir `Goluis.resistanceBuilt` desde `PlayerStats`. |
| `MaterializationExecutor` | Bloqueado: implementar `BlockSpellEvaluator : IPaletteEvaluator` que devuelva `PaletteResult.spatial` con un `ArrangementPattern`; luego funciona solo. |
| `NPCCombatBehavior` | Añadir a los `WorldCharacter`; `EnterCombatMode()`/`ExitCombatMode()` desde el arranque/fin del combate de cocina (futuro `KitchenCombatManager`). |
| `NPCEconomy` / `AreaVendor` | `AreaVendor` en un GameObject por área con interacción (`IInteractable`) contra `Inventory`+`CoinWallet`; que `NPCEconomy` use `ItemData`/`AreaVendor` en vez de su mini-inventario por strings. |
| `ClothingCraftingArea` | Añadir `resultItem` a `ClothingRecipe`, implementar `ConsumeFragments`, descomentar la entrega, escalar tiempo por `TimeController`, cablear `StartCraft` a una interacción. |
| `Generator` | Legacy redundante (`SampleSceneBuilder` usa `FamilyGenerator`). **Candidato a borrar**, no a cablear — decisión pendiente. |
