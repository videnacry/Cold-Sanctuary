# Mandos táctiles tipo Switch y menús como CodeShip (móvil)

Petición de Berón (2026-11-19, **PB-167**): «en CodeShip se ha pasado a recrear los mandos de la Nintendo Switch sobre la pantalla
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

## 6. Rehechos según el santuario 1 de CodeShip (2026-11-20, PB-169)

Berón: «los mandos de switch están muy pequeños y se deben ocultar cuando se muestran modales; quizás lo mejor para recrear la ui/ux
de coldsanctuary1 es mirarlo en vivo y de allí sacar información… montar de cero los mandos, modales y menús; también puedes reutilizar
si eso no disminuye la fidelidad».

**Cómo se midió.** CodeShip (`Catravcul/CodeShip`, rama `integracion`, f4ddd17) clonado y levantado con `vite` en local (la web de
Pages no se alcanza desde el contenedor), santuario 1 abierto desde su pantalla de inicio y medido con Playwright en un iPhone 13
(390×664 px CSS en vertical, 750×342 en horizontal): la caja de cada botón del mando, las ventanas de Misiones (+), Áreas (−) y los
Ajustes generales (inicio). Los números salen de su CSS: `src/ui/action-pad.css`, `style.css` (`.vstick`, `.duel.sheet`),
`map-window.css`, `nav-map.css`, `settings.css`, `task-board.css`, `areas-window.css`.

**Por qué se veían pequeños.** El mando medía ~10 % del lado corto de la pantalla en píxeles de Unity, y la web del móvil va a densidad
≤ 1,5: un botón de 41 px CSS, joysticks de 86 y nombres de 7 px. En CodeShip el botón mide 50, el joystick 120 y el nombre 9.

### 6.1 La medida (`CsUi.Dp`)

Todo se dibuja en **píxeles CSS de CodeShip** pasados a la pantalla por `CsUi.Dp = min(ancho / 390, alto / 664)` en vertical
(`750` y `342` en horizontal): la pantalla virtual mide al menos lo que el iPhone 13 de CodeShip, así que el mando y las ventanas ocupan
**la misma parte de la pantalla** que allí y lo que cabe allí cabe aquí (en una tableta, algo más grande). En el escritorio, 1 px por px.
La piel `CsTheme` también pone la letra a 13 px CSS en el móvil, para los menús que aún no usan el kit.

### 6.2 Los mandos (`TouchControls`, rehecho)

| Pieza | CodeShip | Aquí |
|---|---|---|
| Rombo y cruceta | 50 px, a 49 px del centro en una caja de 148; cruceta con radio 12 | igual |
| Gatillos L/ZL/R/ZR | píldoras 64×44 a −50 y −100 sobre la caja | igual |
| «−», captura, «+», inicio | 44 px; captura con radio 8 | igual |
| Colocación vertical | cajas a 14 px de los lados y 160 de abajo | igual (UI-13 compara las 16 cajas con las medidas en vivo) |
| Colocación horizontal | cajas a 154 px de los lados y 20 de abajo; gatillos 140 px hacia fuera | igual |
| Aspecto | `radial-gradient(at 40% 35%, #4a4f57cc, #1c1f24cc 70%)`, borde 2 px `#ffffff40`, opacidad 0,62; puesto en naranja; pulsado se hunde (0,92) | igual (texturas generadas) |
| Letras | marca 17 px negrita (cruceta 14, gatillos 13, pequeños 15), nombre 9 px debajo | igual (fuente de Unity, no la monoespaciada) |
| Joysticks | 120 px fijos a 22 px de los lados y 26 de abajo; pomo de 50 hasta el borde; «MOVERSE»/«MIRAR» debajo | igual; la cámara con la curva v·\|v\| de CodeShip |
| Mirar arrastrando la pantalla | no (reglas.md §5.7) | quitado |
| Joystick de andar flotante | no | quitado (fijo como en CodeShip) |

### 6.3 Ocultarlos con cualquier modal

`CsUi.ModalOpen`: toda ventana dibujada con el kit se apunta sola al dibujarse (`CsUi.MarkModal`), y además cuentan los que ya
llevaban indicador: `GameKeys.DialogOpen` (pausa, Grimorio, lecciones, consola del reactor, diálogo de opciones, panel de confirmación,
menú de misiones, reto de código), las ventanas de `HudModals` (Horario, Recursos, Misiones, Tareas, Áreas), el lanzador, el panel de
progreso y los diálogos con personajes. Con uno abierto el mando no se dibuja ni recibe toques, y suelta lo que tuviera pulsado (Correr,
si estaba puesto, se queda puesto). **El toque que pulsó un botón del mando no llega como clic a la ventana que abre** (CodeShip
`swallowGhostClick`): `TouchControls.PadTouch` y el kit descartan su clic simulado. Excepción: el viaje automático no lo oculta, porque
aquí se corta tocando el joystick.

### 6.4 Las ventanas (`CsUi`, rehechas)

| Ventana | Pieza de CodeShip | Archivo |
|---|---|---|
| Grimorio (R / F8) | hoja (`.settings-sheet`): abajo desde el 38 % en vertical, a la derecha en horizontal (min(480 px, 46 %)); cabecera en versalitas lila con su ✕; cada hechizo una fila con «Lanzar» | `Control/Grimorio.cs` |
| Ajustes generales (inicio / F10) | la misma hoja; filas `.set-row` con su valor en verde o ámbar; «Seguir» en píldora | `UI/SceneLauncher.cs` |
| Diálogo de opciones | la misma hoja; cada opción una fila | `UI/ChoiceDialog.cs` |
| Reto de código | `.duel.sheet` (desde el 30 % en un móvil bajo) | `CodeShip/CodeChallengeSheet.cs` |
| Misiones y jornada (+ / F4) | ventana a pantalla completa (`.nav-map`): fondo radial, botones cuadrados de 44 px, ✕ | `UI/HudModals.cs` |
| Tareas (+ / F6) y Áreas (− / F7) | la misma ventana, chips de grupo con su cuenta (`.ar-chips`), filas `.mw-row` con «Ir» cian | `CodeShip/MicroTaskWindows.cs` |

Lo que no se trajo: el mapa tridimensional de arriba en Misiones/Áreas (aquí no hay mapa de áreas), la fuente monoespaciada (en la web
de Unity no hay fuentes del sistema) y el botón Y en naranja cuando hay algo con qué interactuar (falta una señal única de «hay algo al
alcance»).

### 6.5 Pruebas (g141)

- UI-04 (ampliada): cabe y no se pisa también en el iPhone a densidad 1,5 (585×996 y 1266×585); «+» nunca baja de 44 px CSS.
- UI-13: a 390×664 las 16 cajas y el joystick están donde las midió Playwright en CodeShip; al doble de densidad, el mismo dibujo al doble.
- UI-14: el mando se oculta con Misiones, el Grimorio y un diálogo de opciones, y vuelve al cerrarlos.
- UI-15: las hojas van donde en CodeShip (vertical abajo desde el 38 % o el 30 %, horizontal a la derecha).

Compilado aquí contra las referencias de Unity 2021.3 (NuGet `UnityEngine.Modules`) con stubs de UI/TMP/URP: sin errores en los
archivos tocados. Falta: `tools/verify-compile.sh` en el PC, correr g141 y probar en el móvil (vertical y horizontal).
