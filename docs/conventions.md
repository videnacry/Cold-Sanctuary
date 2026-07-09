# Convenciones y cómo trabajar aquí

## El repositorio solo contiene código

Este repo versiona únicamente los `.cs` de `Assets/`. Las escenas, prefabs,
`ProjectSettings/`, `Packages/` y los `.meta` **no** están en git. Implicaciones:

- No puedes deducir el cableado de escenas ni los valores del Inspector desde el repo:
  muchos campos `[SerializeField]`/públicos se configuran fuera de aquí.
- Al renombrar/mover un `.cs`, en un proyecto Unity completo hay que mover también su `.meta`
  (no presente en este repo) para no romper referencias.
- No esperes archivos de versión de Unity ni `manifest.json` aquí.

## Estilo de código

- C# estilo Unity (`MonoBehaviour`, coroutines, `[Serializable]`).
- Comentarios mezclan inglés y español; al añadir, mantén coherencia con el archivo.
- Patrón Factory vía `IFactory`; comportamiento de animales vía herencia de `Animal`.
- Lógica temporal centralizada en `TimeController` (`Assets/Scripts/Time/Time.cs`); usa
  `TimeSpeedMinuteSecs` para intervalos de coroutine en vez de constantes.
- Sin namespaces: todo el código vive en el espacio global (0 namespaces confirmados en
  los 144 `.cs` del proyecto).

> **Estado real (auditoría 2026-07-09):** `ClothingCraftingArea.CraftRoutine` usa
> `WaitForSeconds(minutes*60)` sin escalar por `TimeController` (tiene su propio TODO
> admitiéndolo) — viola esta convención de tiempo centralizado.

## Al modificar comportamiento de animales

- Prefiere extender la jerarquía `Animal` → `Carnivore`/`Herbivore` antes que crear clases
  independientes (el caso `BearBehaviour` es el antipatrón a no repetir).
- El ciclo de vida lo dirige `LifeStage.Live()` con pools de delegados; añade comportamientos
  como eventos (`SubEvent`) en vez de cablearlos en `Update()`.

## Al tocar la UI

- Lee primero [`ui-following-arrays.md`](ui-following-arrays.md): hay race conditions
  conocidas alrededor de la inicialización de `FollowingElementBehavior` y la destrucción de
  `uiElements`.

## Mantener la documentación viva

- `CLAUDE.md` (raíz) se carga automáticamente cada sesión → mantenerlo **corto**.
- El detalle vive en `docs/`. Al resolver un bug de [`known-issues.md`](known-issues.md),
  bórralo o márcalo como resuelto. Al cambiar arquitectura, actualiza
  [`architecture.md`](architecture.md).
