# Bugs y deuda técnica conocida

Detectado en el análisis inicial (2026-06-25). Las referencias `archivo:línea` son
aproximadas; verifícalas antes de actuar porque el código cambia. Marca aquí lo que se
vaya resolviendo.

## Bugs probables

- **`BearBehaviour.cs:~87`** — `Random.Range(Respawn.birds.Count, 0)` está al revés
  (max < min). Probable bug.
- **`PlayerCtrl.cs:~149`** — `StartCoroutine("Point")` referencia una coroutine cuyo nombre
  no coincide con el método definido. Revisar el nombre.
- **`ShipCtrl.cs:~82–106`** — placeholders `if(1==1)` en lugar de condiciones reales.
- **`DrivePreparation.cs:~95`** — `Exit()` compara `transform.position` con
  `Transform.TransformDirection(exitPlace)`, pero `TransformDirection` devuelve dirección,
  no posición. Debería ser `TransformPoint`.
- **`BirdBehavior.cs:~39`** — `Altura()` devuelve `int` pero se usa como `float` en
  `Move()` (~línea 63). Sin clamp de altura mínima: los pájaros podrían caer bajo el terreno.
- **`Respawn.cs:~39`** — `Random.Range(1, beasts.Length)` se salta `beasts[0]` (off-by-one).
- **`ActionPrep.cs:~18`** — `energyCost` se divide por `TimeSpeedMinuteSecs/4` sin límites;
  a 1x ≈ /0.25 y a 30x ≈ /15, lo que puede provocar inestabilidad.

## Deuda estructural

- **`BearBehaviour`** no hereda de `Animal`: duplica gestión de estado (asleep, busy, aware,
  LP) y su `Escape()` está incompleto (~183–186). Candidato a consolidar en la jerarquía `Animal`.
- **`WolfBehavior.cs:~145–275`** — gran bloque de `Escape()` legacy comentado. Limpiar o
  reactivar.
- **Máquinas de estado por flags booleanos** (`BearBehaviour`) en vez de enums; sin
  protección contra coroutines concurrentes.
- **Sin null-checks** antes de `GetComponent<>()` ni en accesos a array/lista
  (`Generator`, `FamilyGenerator`, etc.).

## Nombres confusos / cosmético

- **`ActionPrep` vs `ActionsPrep`** — uno es una acción, el otro un contenedor de tres.
  Considerar renombrar (p.ej. `Action` / `ActionSet`).
- **`FollowingArrayInArray .cs`** — el archivo tiene un **espacio** antes de `.cs`. Renombrar
  a `FollowingArrayInArray.cs` (cuidado: en Unity hay que mover también el `.meta`).
- **Comentarios mezclados** inglés/español (`DrivePreparation`, `PlayerCtrl`, `ShipCtrl`).

## Race conditions en UI/FollowingArrays

- `FollowingElement.followingElementBehavior` asignado tras instanciar vs `Init()` en `Start()`.
- `DestroyUIElements()` captura la cola por referencia.
- `RenderUIElements()` no persiste la referencia al padre.

Ver [`ui-following-arrays.md`](ui-following-arrays.md) para contexto.
