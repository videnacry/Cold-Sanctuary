# Visor web sin build (PB-168, 2026-11-20)

Berón quiere ver el microcosmos de Musk (escenario y retos) sin depender de Unity en el PC, que se cuelga y tarda en reiniciar.

## Por qué la máquina de simulación no abre microcosmos en la web
Cada build WebGL lleva **solo sus escenas** (el `.data` incluye todas, y GitHub Pages no sirve archivos de más de 100 MB; ver
[web-continua.md](web-continua.md)). La build del santuario lleva solo SampleScene, así que su máquina no tiene microcosmos que cargar; los
microcosmos van en builds de grupo con su página propia (`microcosmos/<nombre>/`). Una entrada en Inicio choca con lo mismo, y cualquier
página nueva exige un build de 30–60 min en el PC.

## El visor
- **`Assets/Editor/VisorExport.cs`**: menú `Tools/Cold Sanctuary/Web/Visor sin build: exportar Elon Musk (Hawthorne)` (o «la escena activa»).
  Abre la escena en modo aditivo, la vuelca y la cierra sin guardar: **sin Play ni build**, unos segundos. Sale a
  `Builds/Visor/<escena>/`:
  - `escena.jsonl.gz`: un objeto por línea (cubos y primitivas con color, matriz de mundo `m`, bounds; seres `anima`, edificios `building`).
    Mismo formato que el volcado de SampleScene del 2026-11-19 (`codeship-export.jsonl.gz`), que el visor también lee.
  - `retos.json`: la historia (`StoryMission`: pasos, diálogos, fuentes, sitios), los retos de la historia (`CodeChallenges`), las misiones
    de puesto (`MissionSites` → `MissionLibrary`, con el mueble resuelto por `StationMissions.Resolve`), el banco de ideas y los edificios.
  - Sin Editor abierto: `-batchmode -executeMethod VisorExport.ExportMuskBatch -quit`.
- **`tools/visor/index.html`**: three.js (CDN jsdelivr, 0.170.0), una sola página. Lee `datos/escena.jsonl.gz` y `datos/retos.json`
  (`?d=otra/carpeta/` para otra). Dibuja todo con `InstancedMesh` (51 000 piezas del santuario sin problema), cámara libre (WASD, arrastrar,
  Q/E, Mayús; en el móvil dos joysticks y ▲▼), **Lugares** (ir a cada edificio, sitio de historia o puesto), **Historia** (pasos y
  diálogos), **Retos** (ordenar tipo Parsons o elegir, vista pseudo/JS, 2 fallos por misión, explicación y fuente al acertar), faros
  naranja (historia) y cian (puestos), y clic en una pieza = su nombre y ruta en la escena (para dar parte de un fallo).
- **Probado** (2026-11-20, Playwright, escritorio y Pixel 7) con el volcado de SampleScene: 61 470 objetos, 51 460 piezas dibujadas, retos
  de prueba resueltos. Falta: compilar `VisorExport.cs` en el PC y el primer volcado real de `Microcosmos_ElonMusk`.

## Publicarlo
`tools/web/subir-web.ps1` copia el visor en cada subida (parámetro `-Visor`, por defecto `C:\Users\Blein\Documents\Cold Sanctuary\visor`: `index.html` + `datos/`) a `visor/` del sitio; la exportación de Hawthorne escribe en esa misma carpeta `datos/`. Rehacer
el visor = volver a exportar (segundos), no un build.

## Siguiente paso posible
Los retos de diálogo (PB-166) entran solos cuando estén en `CodeChallenges`. Si se quiere andar (gravedad, colisión) o ver a la gente
moverse, eso ya es CodeShip de verdad (TypeScript) y no compensa duplicarlo: el visor es para revisar escenario y retos.
