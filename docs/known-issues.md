# Bugs y deuda técnica conocida

Detectado en el análisis inicial (2026-06-25); **revisado contra el código el 2026-07-09**
(ver [`AUDIT-2026-07-09.md`](AUDIT-2026-07-09.md), que tiene el detalle con `archivo:línea`
verificado). Los ítems ~~tachados~~ se resolvieron o dejaron de aplicar. Las referencias
`archivo:línea` son aproximadas; verifícalas antes de actuar.

## Bugs abiertos (verificados 2026-07-09)

- ~~**`Animal.cs` — `lp` sin inicializar**~~ — **falso positivo del audit**: `lp` SÍ se inicializa
  (`Init()` → `Fatten()` fija `lp = rig.mass`, `LifeStage.cs:133`), así que `Hurt()` no mata de un golpe.
- **`ShipCtrl.cs:83,95`** — placeholders `if (1 == 1)`; las ramas de seguimiento de cámara
  (`else if`/`else`) son inalcanzables.
- **`PullDoor.cs:90` — `OnCollissionEnter()`** mal escrito (doble s); Unity nunca lo llama →
  la rotura de puerta por golpes (`lp`) es código muerto. Renombrar a `OnCollisionEnter`.
- **`Respawn.cs:39`** — `Random.Range(1, beasts.Length)` se salta `beasts[0]` (off-by-one).
- **`CombatAbilityBar.cs` — `RefreshBar()`** solo se llama en `Awake()`; nada lo re-llama al subir
  nivel o descubrir elemento → la barra no crece en vivo como promete el diseño.
- ~~**`KitchenScaleController.cs` / `KitchenEntrance.cs`** — doble entrada.~~ **Resuelto por borrado
  (2026-07-23):** ambas clases (y el método muerto `BuildKitchenContent`/`MakeMobBox`/
  `CreateFragmentPrefabIfNeeded` en `SampleSceneBuilder`) se retiraron. El flujo de cocina lo cubren
  `VirtualizationMachine` + `RealityShiftController` + `MobWorldLoader`. El bug de doble-trigger
  desapareció con la clase.
- **`BirdBehavior.cs:~39`** — `Altura()` devuelve `int` usado como `float`; sin clamp de altura
  mínima (pájaros podrían caer bajo el terreno). Coroutine por string `"Move"`; `population` sin
  limpiar al destruir.
- **`ActionPrep.cs:~18`** — `energyCost` dividido por `TimeSpeedMinuteSecs/4` sin límites (a 1x ≈ /0.25,
  a 30x ≈ /15) → posible inestabilidad.

### Yoga / asanas (revisión 2026-07-24)

El sistema de asanas (datos, evaluación de calidad, entrenamiento por extremidad, cola de beneficio,
maestría) está **implementado y coherente con el diseño**, pero hay dos piezas de runtime **sueltas** y
un problema de persistencia:

- **`AsanaEvaluator` nunca se instancia** — `grep "new AsanaEvaluator"` = 0. Es el único que llama a
  `AsanaQueue.StartActive()` y `body.TrainBodyPart()`, así que por esa vía la cola no arranca ni se
  entrena el cuerpo. La ruta cableada (`AsanaDetector` desde `HologramMenuController`) solo dispara un
  `UnityEvent OnAsanaMatched`. → Dos rutas de detección paralelas; la que mueve stats está sin conectar.
- **`AccumulatePostureStress` sin invocadores** (`IBody.cs:16`, impl. `PlayerStats.cs:120`) — `grep` = 0.
  Consecuencia: `postureStress` **nunca sube**, así que el tambaleo/caída de `PostureStressHandler`
  (`:46-106`) leen un valor que solo baja → el estrés postural está construido pero **no alimentado**.
- **Maestría no persiste** — `Asana.practiceCount`/`masteryLevel`/`containerCurrent` son
  `[System.NonSerialized]` (`Asana.cs:33-35`) → se pierden entre sesiones. Antes de basar progresión en
  la maestría, moverlas a un store serializable.
- **Yoga ↔ maná/XP desconectado** — el maná existe (`CharacterLevel`/`DerivedStats`) y se muestra en HUD,
  pero el yoga no lo desbloquea ni aporta XP de personaje (solo el farming llama a `CharacterLevel.GainXp`,
  `PlayableCreature.cs`). Es lo que el modelo de progresión (creature-stats §Progresión) pide conectar.

## Resueltos / ya no aplican

- ~~**`DrivePreparation.cs` — `Exit()`** comparaba posición contra dirección pura (debía usar
  `TransformPoint`).~~ **Verificado — no es bug**: `Exit()` (`DrivePreparation.cs:95,97`) usa
  `transform.TransformDirection(exitPlace) + transform.position`, así que compara **posición-mundo
  contra posición-mundo** de forma consistente con `SitDown()`/`Update()`. Queda solo latente que
  `pos + TransformDirection` ignora la **escala** del objeto (irrelevante si la nave tiene escala 1).
- ~~**`BearBehaviour` `Random.Range` invertido** y "no hereda de `Animal`, duplica estado por
  flags booleanos".~~ **Resuelto**: `BearBehaviour : Carnivore : Animal`, integrado y completo.
- ~~**`PlayerCtrl.cs` `StartCoroutine("Point")`** y comentarios EN/ES mezclados en `PlayerCtrl`.~~
  **`PlayerCtrl.cs` retirado 2026-07-09** (santuario no-violento; ver `architecture.md`).
- ~~**`BunnyBehavior.Shooted()` referencia `"Hunt"`/`"Sleep"` inexistentes.~~ **Resuelto**: las 8
  `Shooted()` se retiraron 2026-07-09 junto con la mecánica de disparo.
- ~~**Race conditions en UI/FollowingArrays** (`Init()` en `Start()` antes de asignación, etc.).~~
  En su mayoría **ya no aplican**: `Init()` corre síncrono tras `AddComponent`; `FollowingElement.
  Start()/Update()` son no-ops. Persiste solo el hazard latente (sin bug hoy) de `DestroyUIElements`
  capturando la cola por referencia.

## Deuda estructural

- **Sin namespaces** — 144 scripts en el namespace global; riesgo de colisión al crecer.
- **Sin null-checks** antes de `GetComponent<>()` ni en accesos a array/lista (`Generator`,
  `FamilyGenerator`, etc.).
- **Hooks muertos** — `LivingEntity.EvaluateThreat()`/`Animal.EvaluateThreat()` y
  `OnLifeStageChanged()` implementados pero nunca invocados (`ResolveReaction` duplica la fórmula inline).
- **Enums decorativos** — `PostNatalStage.nestType/fatherRole/weaningType` y varios
  `TransitionCondition.Kind` se configuran pero nunca se leen.
- **`WolfBehavior` `Escape()` legacy comentado** — verificar si aún queda el bloque; puede haberse
  limpiado en la migración a `Animal`.
- **Huérfanos completos** (implementados, sin cablear) — ver la sección dedicada en `architecture.md`;
  **no borrar sin decisión**.

## Nombres confusos / cosmético

- **`ActionPrep` vs `ActionsPrep`** — uno es una acción, el otro un contenedor de tres. Renombrar
  (p.ej. `Action` / `ActionSet`).
- **`FollowingArrayInArray .cs`** — el archivo tiene un **espacio** antes de `.cs`. Renombrar (mover
  también el `.meta`).
- **Colisión `Bond`** — `Bond.cs` (raíz, vínculo animal, en uso) vs carpeta `Bond/` (BondActivity).
- **Comentarios mezclados** inglés/español en varios archivos.
