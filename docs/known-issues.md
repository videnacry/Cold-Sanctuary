# Bugs y deuda técnica conocida

Detectado en el análisis inicial (2026-06-25); **revisado contra el código el 2026-07-09**
(ver [`AUDIT-2026-07-09.md`](AUDIT-2026-07-09.md), que tiene el detalle con `archivo:línea`
verificado). Los ítems ~~tachados~~ se resolvieron o dejaron de aplicar. Las referencias
`archivo:línea` son aproximadas; verifícalas antes de actuar.

## Bugs abiertos (verificados 2026-07-09)

- **`Animal.cs` — `lp` sin inicializar** 🔴 — `lp` queda en `0` por defecto; `Hurt()` comprueba
  `lp < rig.mass*0.7` (cierto desde 0) → **cualquier `Hurt()` mata al instante**. Inicializar `lp`
  al nacer (p.ej. a partir de la masa).
- **`Animal.cs` — `sensibility` siempre 0** — se lee en `Escape()` pero nunca se asigna.
- **`ShipCtrl.cs:83,95`** — placeholders `if (1 == 1)`; las ramas de seguimiento de cámara
  (`else if`/`else`) son inalcanzables.
- **`PullDoor.cs:90` — `OnCollissionEnter()`** mal escrito (doble s); Unity nunca lo llama →
  la rotura de puerta por golpes (`lp`) es código muerto. Renombrar a `OnCollisionEnter`.
- **`Respawn.cs:39`** — `Random.Range(1, beasts.Length)` se salta `beasts[0]` (off-by-one).
- **`CombatAbilityBar.cs` — `RefreshBar()`** solo se llama en `Awake()`; nada lo re-llama al subir
  nivel o descubrir elemento → la barra no crece en vivo como promete el diseño.
- **`KitchenScaleController.cs`** — doble entrada posible: `OnTriggerEnter/Exit` (collider) y
  `EnterKitchen()/ExitKitchen()` (desde `KitchenEntrance`) sin protección mutua.
- **`BirdBehavior.cs:~39`** — `Altura()` devuelve `int` usado como `float`; sin clamp de altura
  mínima (pájaros podrían caer bajo el terreno). Coroutine por string `"Move"`; `population` sin
  limpiar al destruir.
- **`ActionPrep.cs:~18`** — `energyCost` dividido por `TimeSpeedMinuteSecs/4` sin límites (a 1x ≈ /0.25,
  a 30x ≈ /15) → posible inestabilidad.

### Sin verificar en la auditoría (revisar)
- **`DrivePreparation.cs` — `Exit()`** comparaba `transform.position` con `TransformDirection` (que
  devuelve dirección, no posición); debería ser `TransformPoint`. No re-confirmado en 2026-07-09.

## Resueltos / ya no aplican

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
