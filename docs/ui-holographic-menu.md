# UI — Menú Holográfico (Menú → Magia / Yoga / Sistema)

Diseño del menú principal del jugador: un holograma pequeño y plano, anclado a una esquina
de la pantalla, que al abrirse revela categorías — cada una con su propio layout — sin
inflar el número de objetos en escena gracias a un pool reutilizable.

> **Nota histórica:** la primera versión de este sistema (misma sesión) usaba cajas 3D
> flotando en el mundo delante del jugador, reutilizando `FollowingArrays`/`UI`
> (ver [`ui-following-arrays.md`](ui-following-arrays.md)). Se reemplazó por completo tras
> feedback directo: los hologramas ocupaban demasiada pantalla, no podían anclarse a una
> esquina de forma fiable (world-space se mueve con la cámara), y el ratón capturado todo
> el tiempo impedía hacer clic en ellos. `FollowingArrays` sigue intacto y en uso — solo para
> el radar de animales, su caso de uso original.

## Por qué screen-space en vez de world-space

- **Anclar a una esquina** es trivial en screen-space (un `RectTransform` con
  `anchorMin=anchorMax=(0,0)` más un offset en píxeles) y frágil en world-space (hay que
  proyectar continuamente desde la cámara).
- **Tamaño pequeño y plano**: un `Image` + `TMP_Text` en un Canvas no tiene el volumen ni el
  peso visual de un cubo 3D con Collider.
- **Clic confiable**: un `Button` de UGUI con `EventSystem` + `GraphicRaycaster` es el camino
  de interacción más probado de Unity — más confiable que `OnMouseDown` vía raycast físico
  sobre un Collider 3D (que además competía con el resto de la física de la escena).
- **Mobile-friendly**: pedido explícito — listas verticales y grids en pantalla se adaptan
  mejor a distintos tamaños de pantalla que objetos posicionados en el espacio 3D.

## El problema del ratón capturado — resuelto aparte

No es specific de este menú, pero es el motivo por el que antes era imposible clicar
cualquier UI: `PlayerController` bloqueaba el cursor (`Cursor.lockState = Locked`) todo el
tiempo desde `Start()`. Ahora:

- El cursor arranca **libre y visible**.
- Mover la cámara con el ratón requiere **mantener presionado el botón derecho**
  (`PlayerController.cameraLookButton`, default `KeyCode.Mouse1`) — se bloquea el cursor
  solo mientras se mantiene, y se libera al soltar.
- El **botón izquierdo queda libre** para clicar la UI y para `PlayerCombat.attackKey`
  (que ya estaba en `Mouse0` — de ahí que la cámara tuviera que ser el botón derecho y no el
  izquierdo, no había otra opción sin romper el combate existente).
- Las flechas J/L/I/K (mirada por teclado) siguen funcionando igual, sin necesitar ningún
  botón del ratón.

## Navegación 100% por teclado (sin ratón)

Requisito explícito: el juego debe poder jugarse solo con ratón O solo con teclado, no
únicamente con ratón. El menú holográfico ahora es completamente navegable sin tocar el
ratón:

- `HologramMenuController.menuToggleKey` (`KeyCode.M` por default, configurable en el
  Inspector) abre/cierra el menú raíz.
- `Escape` cierra el menú si está abierto (además de su uso ya existente para soltar el
  cursor en `PlayerController`).
- Las flechas mueven el foco entre los hologramas activos, y Enter/Espacio (acción "Submit"
  del Input Manager) activa el que tiene foco — **no es un sistema de navegación propio**:
  cada `Hologram` ya es un `Button` de uGUI, y `Navigation.Mode.Automatic` (el default de
  `Button`, nunca se tocó) calcula los vecinos arriba/abajo/izq/der solo con la posición en
  pantalla de cada tarjeta activa. Lo único que hacía falta era darle un punto de partida al
  `EventSystem`: `HologramMenuController.FocusFirstActive()` selecciona el primer holograma
  de cada lista recién construida, y el holograma "Menú" de la esquina queda seleccionado por
  default al arrancar y al cerrar el menú.
- `HologramPool.CreateHologramInstance()` ahora asigna `button.targetGraphic` y un
  `ColorBlock` con colores de foco/hover bien por encima de blanco — antes un `Button`
  creado por `AddComponent` (en vez de por el menú "Crear > UI > Button" del editor) no
  recibía `targetGraphic` automáticamente, así que el tinte de foco no se veía. Sin esto, un
  jugador navegando solo con teclado no tenía forma de saber en qué holograma estaba parado.

## Arquitectura

```
Hologram            — una tarjeta plana reutilizable (Image + TMP_Text + Button).
                       No sabe nada de menús; solo expone SetContent(texto, icono, onClick)
                       y SetAnchoredPosition(pos).

HologramPool         — crea un pool de tamaño fijo de Hologram al arrancar (Awake), todos
                        inactivos. Acquire()/Release()/ReleaseAll() — nunca Instantiate/
                        Destroy en caliente.

HologramMenuController — la lógica de navegación real: qué categoría muestra qué contenido,
                          con qué layout, y qué pasa al hacer clic en cada tarjeta.
```

### El pool — tamaño y por qué no hacen falta más hologramas

`HologramPool.poolSize = 118` — el tamaño completo de la tabla periódica (`Magia`), que es
el listado más grande que el menú necesita mostrar **a la vez**. Ningún otro submenú (Yoga:
8 posturas; Sistema: 2 ajustes; accesos directos: unos pocos) se acerca a esa cifra, así que
un solo pool cubre todos los casos. Si en el futuro algún submenú necesitara más de 118
hologramas simultáneos, subir `poolSize` — no crear un segundo pool.

### Reutilización entre variantes

Pedido explícito: "cada holograma alberga distintos valores... el de 'menu' se puede
transformar en 'yoga'". Esto se resuelve así: el holograma de la esquina (`_menuHologram`)
**no es del pool** — es una instancia fija, siempre visible, que solo abre la navegación.
Todo lo demás (categorías, elementos, botones de esquina) sale del pool: se piden con
`Acquire()`, se configuran con `SetContent()` + `SetAnchoredPosition()` para el rol actual, y
se devuelven con `Release()` al navegar a otra pantalla. La MISMA tarjeta física puede
representar "Magia" en un momento y, tras cerrar y reabrir el menú, un elemento químico
distinto — nunca se sabe cuál físicamente, y no importa, porque el pool es indistinguible
por fuera.

### Layouts por submenú

| Submenú | Layout | Notas |
|---|---|---|
| Menú (raíz) | Lista vertical, esquina inferior derecha | 3 categorías: Magia, Yoga, Sistema |
| Magia | Grid aproximando la tabla periódica (columna = grupo químico) | Solo elementos **descubiertos** (`PeriodicTableManager.AllDiscovered()`) — vacío en partida nueva, no es un bug |
| Magia → Hechizos (esquina) | Grid simple | Hechizos completos desbloqueados — sin datos todavía, ver TODO |
| Yoga | Grid 4 columnas | Las 8 posturas corporales (`BodyPosition`), clic = `AsanaDetector.SelectPosition` |
| Yoga → Asanas (esquina) | Grid simple | Asanas completas desbloqueadas — sin datos todavía, ver TODO |
| Sistema | Lista vertical | Cámara 1ª/3ª (tarjeta) + volumen (slider real, no tarjeta) |

Todas las posiciones se calculan en píxeles relativos a la esquina inferior-izquierda del
Canvas (`GetCornerBase`/`VerticalOffset`/`ArrangeGrid`/`ArrangePeriodicGrid` en
`HologramMenuController.cs`) — ver el código para el detalle exacto, es aritmética simple,
no hace falta reproducirla aquí.

**Aproximación, no exactitud química:** `ArrangePeriodicGrid` agrupa por `ElementGroup`
(columna) y apila dentro del grupo en orden de descubrimiento (fila) — no replica períodos
reales ni el hueco de lantánidos/actínidos. Se ve "como una tabla periódica" a simple vista;
si se quiere exactitud, haría falta posicionar por `atomicNumber` con la geometría real de
la tabla (huecos incluidos) — pendiente, no crítico.

## Nombres — Magia vs. Hechizos

Se renombró la categoría raíz de "Hechizos" a **Magia**, reservando "Hechizos" para el
holograma de esquina que ejecuta hechizos completos ya desbloqueados de un solo clic (sin
pasar elemento por elemento). Dentro de Magia:
- **Tabla** (el grid de elementos — botón para volver desde Hechizos) — la vía de invocación,
  elemento por elemento.
- **Hechizos** (esquina) — la vía directa, un clic, solo para combos ya dominados.

No se me ocurre un nombre claramente mejor que "Magia" para la categoría raíz —
encaja con el mismo registro simple que "Yoga"/"Sistema" y ya se usa informalmente en
DEVLOG.md como "sistema de encantamientos". Si se quiere algo más temático más adelante
("Alquimia" es la alternativa más obvia, dado que el sistema es química/tabla periódica),
es un cambio de una sola palabra en `HologramMenuController.OpenRoot()`.

## Sistema (ajustes)

- **Cámara 1ª/3ª** — tarjeta normal del pool, mismo camino que ya usaba `PlayerController`
  (`CameraManager.SwitchTo` + `PlayerPrefs["CameraMode"]`).
- **Volumen** — un `Slider` de UGUI real (no una tarjeta — no tiene sentido como botón),
  creado una vez en `SampleSceneBuilder.CreateVolumeSlider()`, activado/posicionado por
  `HologramMenuController.OpenSistema()` y ocultado en cualquier otra pantalla.
- **Idioma** — **no implementado**, no existe sistema de localización en el proyecto
  (confirmado buscando "Localization"/"idioma" en todo `Assets/Scripts`). Añadir un dropdown
  sin nada detrás sería falso, así que queda fuera hasta que exista esa capa.
- **Pausa real**: `Time.timeScale = 0f` al entrar a Sistema, `1f` al salir de cualquier
  submenú o cerrar el menú — confirmado en Play mode (los logs de consola dejan de avanzar
  mientras el panel está abierto).

## Símbolos/imágenes vs. palabras

Pedido explícito: usar símbolos/imágenes en vez de palabras cuando sea posible, para que los
hologramas puedan ser pequeños y legibles. `Hologram.SetContent(text, iconSprite, onClick)`
ya soporta un `Sprite` opcional — si se da, se muestra el icono y se oculta el texto. Hoy
**todas las llamadas pasan `null` como icono** (no hay arte de iconos todavía: ni símbolos
de elementos estilizados, ni glifos de posturas, ni iconos de Magia/Yoga/Sistema), así que
todo se ve como texto por ahora. Cuando haya arte, es cuestión de pasar el `Sprite`
correspondiente en cada llamada a `SetContent` — la infraestructura ya está lista, no hace
falta tocar `Hologram`/`HologramPool`.

## Kushal en primera persona — FOV separado

De paso, al revisar la cámara: `CameraManager` ahora tiene `thirdPersonFOV` (60°, igual que
antes) y `firstPersonFOV` (82°) en vez de un solo `baseFOV` compartido. En primera persona,
con Kushal ya reescalado a 1.75m reales, 60° se sentía anormalmente cerrado — todo se veía
pequeño/lejano. 82° es un FOV típico de shooters en primera persona y se aplica al instante
al cambiar de modo (`ApplyMode()` ahora hace `Camera.main.fieldOfView = baseFOV` de entrada,
no solo la corrección gradual por estrés que ya existía en `ApplyStatEffects()`).

## Archivos

**Nuevos:**
- `Assets/Scripts/UI/Hologram/Hologram.cs`
- `Assets/Scripts/UI/Hologram/HologramPool.cs`
- `Assets/Scripts/UI/Hologram/HologramMenuController.cs`

**Modificados:**
- `Assets/Editor/SampleSceneBuilder.cs` — `BuildHolographicMenu()` reescrito por completo
  (crea el pool + controller bajo `Canvas_AUTO`, ya no construye cajas 3D a mano). Limpia el
  `MenuAnchor` huérfano de la versión anterior (colgaba de `CameraPivot`, que no se destruye
  en cada rebuild como `LevelDressing_AUTO`).
- `Assets/Scripts/Player/PlayerController.cs` — cursor libre por defecto,
  `cameraLookButton` (Mouse1) para mirar con el ratón manteniendo presionado.
- `Assets/Scripts/Camera/CameraManager.cs` — `thirdPersonFOV`/`firstPersonFOV` separados.

**Eliminados** (superados por el sistema nuevo — la primera versión world-space de esta
misma sesión):
- `SystemSettingsFollowingArrays.cs`, `PeriodicElementMenuItem.cs`, `BodyPositionMenuItem.cs`,
  `HashSetHolderDiscoveredElements.cs`, `PeriodicElementMarker.cs`, `PeriodicElementRegistry.cs`

**Sin tocar** (siguen usándose para el radar de animales): todo `Assets/Scripts/UI/FollowingArrays/`
— incluye el fix de instancia-compartida y el `virtual` en `HideArrays()` de la sesión
anterior, ambos siguen vigentes y son útiles ahí. `BodyPosition.cs`, `BodyPositionButton.cs`,
`AsanaDetector.cs`, `Asana.cs` tampoco se tocaron — el controller nuevo llama a
`AsanaDetector.SelectPosition()` directo, sin pasar por `BodyPositionButton`.

## Estado de las pruebas (Play mode)

Confirmado en Play mode, 0 errores de consola:
- [x] Holograma "Menú" visible, pequeño, plano, anclado a la esquina inferior derecha
- [x] Clic en "Menú" → abre lista vertical Magia/Yoga/Sistema + X de cierre en esquina superior derecha
- [x] Clic en "Magia" → grid vacío (correcto, sin elementos descubiertos en partida nueva) + esquina "Hechizos" + X
- [x] Clic en "Hechizos" → panel con botón "Tabla" (volver) + X — confirma la navegación de 3 niveles
- [x] Clic en X → cierra y vuelve a "Menú" en cualquier nivel
- [x] Pausa real en Sistema confirmada (logs de consola dejan de avanzar con el panel abierto)
- [ ] Yoga y el slider de volumen no se llegaron a probar de forma interactiva esta sesión
  (se acabó el tiempo de pruebas manuales, no hay motivo para sospechar que fallen —
  usan exactamente el mismo camino de código que Magia/Sistema, ya verificados)
- [x] **Navegación por teclado**: `M` abre el menú raíz, `Enter` activa el holograma con foco
  (confirmado abriendo "Magia" → tabla periódica). `Escape`/clic en "X" no se pudieron
  re-confirmar en la misma sesión por el mismo problema de entorno descrito abajo (esta vez
  además con el propio toggle Play/Stop del Editor volviéndose errático) — la lógica de cierre
  es simétrica a la de `M` (mismo `CloseAll()`) y ya estaba probada por clic en una sesión
  anterior.
- **Nota sobre los clics durante las pruebas**: bastantes clics no se registraron esta sesión
  incluso apuntando bien al centro de la tarjeta — parece un problema del entorno de captura
  (una ventana de Unity Hub con notificaciones tipo "Shop sale" aparecía superpuesta
  intermitentemente y probablemente robaba el foco), no del código: cuando el clic sí
  llegaba, siempre funcionó correctamente. Si en el juego real los clics fallan, no
  deberíà ser por esto — revisar el `GraphicRaycaster`/`EventSystem` primero.

## TODO restantes

- [ ] `AsanaDetector.ExecuteShortcut()` — el "Hechizos"/"Asanas" de esquina hoy no ejecutan
  nada al hacer clic (no existe la noción de "hechizo completo" en el código, y
  `AsanaDetector.unlockedAsanas` está vacío porque no hay `Asana` ScriptableObjects creados)
- [ ] Crear los `Asana` ScriptableObjects reales y asignarlos a `AsanaDetector.unlockedAsanas`
- [ ] Sistema de localización real, luego agregar idioma al panel Sistema
- [ ] Conectar los elementos de Magia a una mecánica de encantamiento real (hoy de solo lectura)
- [ ] Completar el catálogo de `PeriodicTableManager` a los 118 elementos (hoy ~55, curados)
- [ ] Arte de iconos/símbolos para reemplazar el texto — la infraestructura ya soporta
  `Sprite` en `Hologram.SetContent()`, falta el arte en sí
- [ ] Geometría exacta de tabla periódica (hoy es una aproximación por grupo/orden de descubrimiento)
