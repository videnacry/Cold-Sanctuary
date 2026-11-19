# Exportación de la escena principal para Codeship

`codeship-export.jsonl.gz` es la jerarquía completa de `Assets/Scenes/SampleScene.unity` (el santuario de hielo, «Mesocosmos»), exportada el 2026-11-19 con `Tools › Codeship › Exportar escena` (`Assets/Editor/CodeshipExport.cs`, solo lectura). Sirve para que el port web (Codeship) recree los objetos reales de cada área.

- 61 470 GameObjects, una línea JSON por objeto (activos e inactivos), 27,6 MB sin comprimir.
- Campos: `path` (ruta completa en la jerarquía), `name`, `active` (activeInHierarchy), `activeSelf`, `pos`, `rot` (euler, mundo), `scale` (lossyScale), `bounds` {`size`, `center`} si tiene Renderer, `mesh`, `primitive` (cube/sphere/capsule/cylinder/plane/quad, deducido del nombre de la malla), `material` y `color` (hex RGBA de `_BaseColor`/`_Color`), `collider`, `layer`, `tag`, `components`.
- `codeship-export-summary.json`: resumen con el número de objetos y las raíces.

Leer: `gzip -dc export/codeship-export.jsonl.gz | head`
