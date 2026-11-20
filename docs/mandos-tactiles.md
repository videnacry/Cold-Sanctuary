# Mandos táctiles tipo Switch y menús como CodeShip (móvil)

Petición de Berón (2026-11-19, **PB-165**): «en CodeShip se ha pasado a recrear los mandos de la Nintendo Switch sobre la pantalla
para los controles, ¿se podría implementar así para mobile aquí? Allí los menús son muy estéticos, ¿sería posible hacer algo similar
aquí para mobile?». Referencia: CodeShip `origin/integracion`, `src/ui/action-pad.ts` + `action-pad.css` (los Joy-Con) y
`src/ui/root-menu.css` (la paleta de los menús).

## 1. De dónde se parte

- **Todo el juego lee el teclado por `GameKeys`** (`Assets/Scripts/Control/GameKeys.cs`, 49 archivos): WASD para andar, J/L/I/K o el
  ratón para mirar, E hablar, F usar, espacio saltar o trepar, Mayús correr, Ctrl bajar, V vista, F1–F7 y F10 para HUD, ventanas y pausa.
  Las flechas son solo para los diálogos.
- **Ya había una capa táctil** (`TouchControls`, 2026-11-13, web ligera): dos joysticks y cuatro botones en arco (E, F, Saltar, Correr) más
  «Intro» arriba. Pulsa teclas virtuales con `GameKeys.SetVirtual` / `TapVirtual` / `VirtualMove` / `VirtualLook`. Solo se crea en la web
  ligera con pantalla táctil (`WebLigera`).
- Los diálogos (`ChoiceDialog`, la hoja del reto de código, la pausa, el lanzador) leían `Input` directamente: un botón táctil no les llegaba.
- Todos los menús son **IMGUI** y crean sus estilos desde `GUI.skin` (la piel gris por defecto de Unity).

## 2. Diseño

**Un solo sistema de entrada.** El mando no tiene lógica propia: cada botón pulsa una tecla que el juego ya lee, por `GameKeys`. El teclado
sigue igual. Así no hay código paralelo y todo lo nuevo que se cablee al teclado funciona en el móvil sin tocar el mando.

### 2.1 Los Joy-Con (`TouchControls` + `JoyConMap`)

Los 16 botones de un par de Joy-Con, como en CodeShip: redondos, oscuros y translúcidos (0,62 en reposo), la marca en blanco y lo que
hacen debajo; el que está puesto o pulsado se enciende en naranja y se hunde al tocarlo.

| Botón | Hace | Tecla | Modo |
|---|---|---|---|
| A | Saltar (mantenido, trepa) | Espacio | mantener |
| B | Correr | Mayús izq. | interruptor (se queda puesto) |
| X | Usar | F | toque |
| Y | Hablar / atender / paso de cocina | E | toque |
| R | Grimorio (la ventana de hechizos, §5) | F8 | toque |
| ZR | Intro (seguir una tarjeta, elegir) | Intro | toque |
| L | Bajar (nadar abajo, descender) | Ctrl izq. | mantener |
| ZL | Cerrar (diálogos, ventanas, reto) | Esc | toque |
| Cruceta | flechas de los diálogos | ↑ ↓ ← → | toque |
| + | Misiones: Tareas (F6) en un microcosmos con tareas; si no, Misiones y jornada (F4) | F6 / F4 | toque |
| − | Áreas | F7 | toque |
| Captura | Vista (1.ª / 3.ª persona) | V | toque |
| Inicio | Menú: el menú árbol de CodeShip (F5) donde lo hay; si no, la pausa (F10, que lleva a Inicio) | F5 / F10 | toque |

A, B, +, −, captura e inicio: igual que CodeShip. El resto, decidido en el hilo (opción «como CodeShip y el resto a mi criterio»).

**Colocación.** Joystick de andar abajo a la izquierda (flotante: el aro aparece donde se apoya el dedo en el 40 % izquierdo) y joystick
de la cámara fijo abajo a la derecha; arrastrar en el resto de la mitad derecha también mira. En **horizontal** la cruceta va al lado del
joystick de andar y el rombo al lado del de la cámara, con L/ZL y R/ZR encima de cada joystick; en **vertical** cada grupo sube encima de
su joystick y los gatillos encima del grupo. Un botón mide ~10 % del lado corto (mín. 36 px); en una tableta 4:3 se encoge para que no se
pisen. Los glifos (▲ ▼ ◄ ► ⌂ ● −) son del juego WGL4, que trae la fuente por defecto también en la web.

### 2.2 Los menús (`CsTheme`)

Una piel IMGUI con la paleta de CodeShip: cristal azul petróleo (`#0b2a36`), bordes y acento cian (`#7fdcff`), texto `#d8f6ff`, verde
`#7dffb0` para lo activo, naranja `#ff9a2e` para lo que pide atención, esquinas redondeadas. Las texturas se generan en código (el repo solo
versiona `.cs`): no hay nada que montar en Unity. Cada menú llama a `CsTheme.Use()` al principio de su `OnGUI` y hereda el tema sin
cambiar su dibujo. `CsTheme.Enabled = false` vuelve a la piel de Unity. Puesto en: menú árbol, Tareas/Áreas, hoja del reto de código,
diálogo de opciones, ventanas del HUD, columna de acción, avisos, guía de inicio, viaje, pausa, pantalla de Inicio y horario libre.

### 2.3 Los diálogos oyen el mando

`GameKeys.UiKeyDown(k)`: la tecla de verdad o la del mando, **sin enmudecer** (el menú abierto es quien la quiere). La usan
`ChoiceDialog` (flechas, Intro, Esc), `CodeChallengeSheet` (Esc, Intro) y `SceneLauncher` (pausa).

## 3. Lista de cambios

| # | Cambio | Estado |
|---|---|---|
| 1 | `JoyConMap`: los 16 botones como datos (tecla, nombre, modo, tecla según contexto) | Hecho (rama `claude/project-thread-n17elb`) |
| 2 | `TouchControls`: dibujo y colocación de los Joy-Con; `Compute(w, h)` puro; botones mantenidos por dedo | Hecho |
| 3 | `GameKeys.UiKeyDown` y su uso en diálogo de opciones, reto de código y pausa | Hecho |
| 4 | `CsTheme` y `CsTheme.Use()` en 12 menús | Hecho |
| 5 | Prueba `JoyConTest` (grupo 141, lógica pura): UI-02…UI-09 | Hecho, **sin compilar ni correr** (falta el PC) |
| 6 | Compilar (`tools/verify-compile.sh`) y probar en vivo: web ligera en el móvil, horizontal y vertical | Pendiente (probador, con permiso de Berón) |
| 7 | El lanzador de hechizos en el móvil: está apagado en la web ligera (no hay teclado). Resuelto con el **Grimorio** (§5) | Hecho en código (2026-11-20) |
| 8 | Los menús que no pasan por `CsTheme` (paneles de `AnimaStatusHUD`, `ProgressPanel`, `GymRoutinePanel`, lecciones de la planta atómica…): añadirles `CsTheme.Use()` uno a uno y revisar contraste | Pendiente |
| 9 | Botones que se apagan cuando no hacen nada en esa escena (p. ej. «−» sin Áreas), como los «free» de CodeShip | Pendiente |
| 10 | Con el mando en pantalla, un toque también llega como clic del ratón (Unity simula el ratón): vigilar que tocar un botón no ataque además (atacar es el clic) | Vigilar en la prueba en vivo |
| 11 | Atacar ya no tiene botón (R pasó a Grimorio): si hace falta en el móvil, darle uno (p. ej. mantener ZR) | Pendiente |

## 4. Pruebas

`Assets/Scripts/Tests/JoyConTest.cs`, grupo 141, lista rápida (no carga escenas):

- UI-02: 16 botones, 8 por lado, cada uno con tecla y nombre.
- UI-03: la asignación de CodeShip (A, B, +, −, captura, inicio).
- UI-04: el mando cabe y nada se pisa en 2340×1080, 1080×2340, 1024×768, 1920×1080 y 360×740.
- UI-05: el tema genera sus texturas (redondas, esquinas transparentes).
- UI-06: tocar X pulsa F un fotograma.
- UI-07: mantener A mantiene espacio y soltarlo lo suelta.
- UI-08: B (Correr) se queda puesto y el segundo toque lo quita.
- UI-09: con un menú abierto, el juego no ve ZL pero los diálogos sí (Esc y la cruceta).
- UI-10: cada hechizo del lanzador tiene su reto de código bien formado.
- UI-11: R abre el Grimorio (F8).
- UI-12: en el Grimorio, elegir un hechizo abre su reto; fallar no lo lanza y acertar sí (con un lanzador de prueba, `Grimorio.CastHook`).

## 5. El Grimorio (2026-11-20)

Berón: «para los hechizos vamos a hacer una prueba del sistema móvil: agregamos el botón de grimorio, debe mostrar una modal con todos los
hechizos existentes y se lanzan con los retos de código». `Assets/Scripts/Control/Grimorio.cs`:

- **Abrir**: R en el mando o F8 en el teclado (se crea solo en cada escena). Cerrar: el botón «Cerrar», R/F8 otra vez, o ZL/Esc.
- **La ventana** (tema `CsTheme`): todos los hechizos del lanzador (`SpellConsole.Catalog`), con su plantilla; los de prueba
  (`reemplazar`, `admin`, `tercera`, `miembro`, `arbol`, `piso`) en su propia sección. Los no aprendidos (`SpellConsole.PlayerKnows`) salen
  apagados; si no se maneja a nadie, lo dice y no deja lanzar.
- **Lanzar**: tocar un hechizo abre su **reto de código** en la hoja de CodeShip (`CodeChallengeSheet`). `SpellChallenge` lo fabrica de la
  línea: con parámetros, **ordenar** nombre(, parámetros y cierre (con una pieza trampa); sin parámetros, **elegir** la línea bien escrita.
  Al acertar se lanza con `SpellConsole.CastFromUi` y la hoja enseña la línea y la respuesta del hechizo.
- **La línea** (`SpellConsole.LineFor`): los parámetros por defecto de la plantilla; un destino vacío se rellena con lo que sugiere el
  interés del poseído (la misión de ahora, la persona más cercana…), y si no hay sugerencia se deja al hechizo.
- Siguiente paso posible: elegir el destino en el reto (una pregunta de elegir con las sugerencias) en vez de tomar la primera.
