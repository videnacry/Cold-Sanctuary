# Web continua: una página por microcosmos, prueba en la página y subidas (2026-11-16)

Berón (2026-11-16) quiere validar en el móvil cada microcosmos que existe y los que vengan: que los personajes, la flora y la fauna se vean
bien, en su sitio y con su comportamiento. Pide **una página por microcosmos** y que la web siga a la rama, con subidas continuas.

La rama `claude/web-continua` sale de `claude/edificios` y lleva fusionada `claude/web-ligera`. Todo lo de la web va tras `WEB_LIGERA` (o
`WebLigera.Active`, constante falsa fuera de la web), así que el editor y el Play no cambian.

## El sitio (`Builds/Web/sitio/` en el proyecto Unity)
| Ruta | Qué es |
|---|---|
| `index.html` | El portal: el santuario y cada microcosmos, con «abrir» y «probar». |
| `santuario/` | El Santuario ligero (SampleScene sola). |
| `grupos/gNN/` | Builds de «santuario + unos cuantos microcosmos». `?escena=<nombre>` elige en cuál entra Kushal. |
| `microcosmos/<nombre>/` | **Una página por microcosmos.** Redirige a su grupo con `?escena=<nombre>` y pasa lo demás de la URL (`?prueba=1`). |
| `escenas.json` | La lista. La lee el probador. |
| `version.txt` | La versión. La espera el subidor. |

- **De dónde sale la lista:** sola, de la carpeta `Assets/Scenes`: `Microcosmos_*` y `MobWorld_*`. Un microcosmos nuevo aparece sin tocar
  código. Su nombre visible y su historia vienen del registro de la Sala de Eras (`EraHubSceneBuilder.All`) si está.
- **El nombre de la URL:** la escena sin prefijo (`Microcosmos_Buda` → `microcosmos/Buda/`).
- **Por qué en grupos:** el `.data` de un build lleva **todas** sus escenas, y GitHub Pages no sirve archivos de más de 100 MB. Los 63
  microcosmos de hoy son ~1,3 GB de escena en texto, unos ~220 MB de `.data`. Cada grupo lleva el santuario (el anfitrión: el jugador, la
  cámara y el reloj viven allí) y los microcosmos que quepan hasta ~80 MB de `.data`.
  - Si un grupo sale de más de 90 MB, se parte en dos y se rehace.
  - Lo medido (la base del santuario y los MB de `.data` por MB de escena) se guarda en `Builds/Web/plan-web.txt` y afina el plan siguiente.
  - Arranca con lo medido el 2026-11-13: santuario 32,2 MB; + Kapilavastu (58,7 MB de texto) → 42,2 MB, o sea ~0,17 MB por MB.
- **Plan de hoy (estimado):** 63 microcosmos en **5–6 grupos** + el santuario, **6–7 builds**.
  - Cada grupo: ~75–80 MB de `.data` + ~35 MB de `.wasm`.
  - El sitio, unos **650–760 MB**: por debajo del límite de 1 GB de un sitio de Pages, pero cerca.
  - **Tiempo:** el primer build WebGL ~30–45 min; los siguientes, con la caché de IL2CPP, ~15–25. En total, unas 2,5–3,5 h en este PC.
  - **Memoria:** un build cada vez, y suelta lo cargado entre build y build. El menú «ver el plan» da el reparto antes de construir.
- **Riesgo:** cada subida añade ~700 MB al historial de `cs-web-pages` (su `.git` ya ocupa 260 MB).
  - Opción a: `subir-web.ps1 -Squash`, un solo commit y `push --force`. Necesita el visto bueno de Berón.
  - Opción b: pasar a compresión gzip con «decompression fallback», que también vale en cualquier servidor estático. Dividiría el tamaño
    por ~3–4 (sitio ~200 MB), a costa de descomprimir en el móvil.

## La prueba en la página (`?prueba=1`, `WebPrueba`)
Corre **en el jugador**, tras abrir la escena (con Kushal dentro del microcosmos). Al final escribe dos líneas en la consola:
`[PRUEBA-JSON] {…}` y `[PRUEBA-TITULO] OK | FALLOS n`. La plantilla WebLigera las pasa a `<pre id="resultado">` y a `document.title`.

Mira lo mismo que las pruebas del editor, sin definiciones nuevas:
- **Presencia** (las reglas de `CivilizedErasTest`):
  - nadie con cuerpo humano, ≥ 2 edificios de la era, ≥ 3 con jornada, estatura 1,3–2,3 m;
  - la flora y la fauna, frente a su **mapa a mano** (`MicroStoryMaps`): al menos la mitad de sus plantas y bichos; sin mapa hecho, que haya;
  - en el santuario: gente, y animales sin pasar del tope de la web.
- **Sitio:** la regla de la flora de `MicroGround` (`TopSurface`, `Inside`), también para bichos y gente. Nadie enterrado bajo la superficie
  de encima, nadie dentro de un collider, nadie fuera del piso (`Suelo_piso`), nadie sin suelo debajo. Admite hasta un 5 % (muestra de
  ≤ 300 por tipo).
- **Sin magenta:** los materiales que `WebLigera.FixBrokenShaders` encontró con el sombreador de error o no soportado.
- **Comportamiento:**
  - en 12 s, al menos un 25 % de la gente y los animales se mueve o gira;
  - nadie cae más de 2 m;
  - los **controles táctiles** existen, y el joystick de andar, empujado sin dedo (`TouchControls.TestMove`), mueve a Kushal.

Todas las esperas están acotadas: 6 s para asentarse, 12 de observación y 2 de joystick. Si la página no carga, el título queda
`FALLOS carga`.

## Los pasos del tester
1. **Sincronizar y generar las escenas:** `tools/sync-to-unity.sh` desde `claude/web-continua`, y en Unity los menús de las escenas que
   hayan cambiado.
2. **Construir:** en el editor, `Tools/Cold Sanctuary/Web/Web continua: ver el plan (sin construir)` y luego
   `… Web continua: construir todo (santuario + todos los microcosmos)`. Con el editor **cerrado**:
   `powershell -ExecutionPolicy Bypass -File tools\web\construir-web.ps1` (Unity en batch, tope de 8 h, registro en
   `Builds\Web\construir.log`). Al terminar, el editor vuelve a **StandaloneWindows64**; los dos menús ligeros también.
3. **Subir:** `powershell -ExecutionPolicy Bypass -File tools\web\subir-web.ps1 -Mensaje "Web: sitio desde web-continua <commit>"`.
   - Copia a `cs-web-pages` (espejo exacto, sin tocar `.git`).
   - Rechaza archivos de más de 100 MB.
   - Hace commit con el trailer y push a `origin main`.
   - Espera a que Pages publique la `version.txt` nueva (≤ 15 min) y prueba la web en vivo.
4. **Probar** (también suelto): `powershell -ExecutionPolicy Bypass -File tools\web\probar-web.ps1`. Admite `-Solo Buda,santuario`,
   `-TimeoutEscena 420`, `-Salida res.json`, `-Urls <dirección>` y `-Modo dumpdom`.
   - Usa el **Edge del PC sin ventana** (`--headless=new`, SwiftShader: WebGL 2 comprobado), con tamaño y toque de móvil (812×375,
     DevTools Protocol por el WebSocket de .NET; nada que instalar).
   - Abre cada página con `?prueba=1`, espera el título `OK`/`FALLOS` (acotado) y lee `#resultado`.
   - Imprime una tabla por escena (estado, fallos, gente, flora y fauna frente a lo esperado, y qué falla). Sale con 0 si todo da OK.
