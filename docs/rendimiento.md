# Rendimiento del santuario

Medida de partida (2026-11-05, SampleScene en Play, editor, PC de 16 GB): **65 545 renderers activos, 849 en lotes
estáticos, 3,0 FPS** (antes 93 951 y 1,4). El reparto de abajo sale de leer la escena guardada, no de una medida en Play. Lo
que dependa de una medida está marcado.

## Dónde está el coste

Escena guardada (53 128 MeshRenderer, 62 976 GameObjects, 1 134 materiales, 4 383 BoxCollider, 8 836 scripts):

- **51 596 renderers son el cubo primitivo**: el mismo mesh, una llamada de dibujo por cubo.
- Por rama: `Areas_Placed` 42 631 (Torre_Entrenamiento 13 189, Servicios 9 109, Residencia 5 782, Ciencias 4 435, sitio
  nuclear 3 709, Producción 2 929, Árbol 2 278), Enfermería y personas 4 310, Restaurante 2 279, FloraHielo 1 230.
- Por pieza, lo que más sorprende: las **máscaras** de los percheros (2 450 máscaras de 4 cubos: casi 10 000 renderers, el 21 %
  de la escena), `Circuito` de las alfombras 3 816, `Pelo` 3 331, `Flor_nectar` 1 540.
- Otros **13 000** nacen en Play o llegan por prefab (grupos de animales y lo que crean los `Start`). La sonda dice cuáles.

El renderer de PC multiplica ese número:

- **El SSAO pedía profundidad + normales** antes de lo opaco, y eso fuerza un prepaso: toda la escena se dibuja una vez más.
- Sombras a 50 m con **4 cascadas** y sombra suave.
- Niebla exp² de 0,008: a 120 m ya tapa el 60 %, a 280 m casi todo. Cámara con recorte a 1 000 m.

## Medida en vivo (ronda 13, editor)

**La primera lectura de este doc era errónea: el coste principal son los scripts, no el dibujo.**

- Frame: unos 230 ms. `BehaviourUpdate` 125–137 ms, `LateBehaviourUpdate` 20–24 ms.
- El lote funciona: el dibujo baja de 40,6 a 8,3 ms y los renderers dibujados de 66 360 a 18 975. Los FPS apenas cambian (4–6),
  porque el dibujo ya no era lo que limitaba. Con tanto ruido, el A/B de FPS no distingue los ajustes de URP.
- Escena en Play: 85 257 objetos, 14 277 scripts, 131 animas, 1 549 etiquetas.
- El log tuvo **5 `Debug.Log` por frame** (588 en 120 frames; 217 de `[Mente]`), cada uno con su pila.

Por leer el código, sin nombres medidos todavía, los sospechosos son:

- **`TreeRotator`:** con pocos FPS quitaba y ponía el NavMesh del árbol en cada frame. Corregido.
- **Búsquedas por toda la escena** repetidas por persona: `WorkdayDirector.Body` con un nombre que falta, `DailyLife`. Corregido.
- **El goteo:** unos 40 scripts por anima × 131, ninguno caro por sí solo, en código de depuración del editor.

Para el reparto real está el menú *Perfilar scripts*.

## Lo que se hizo

| Cambio | Hace falta | Ganancia | Cómo distinguirla |
|---|---|---|---|
| **Lote estático** (`LoteEstatico`, en Play) | nada: ni rebuild ni tocar la escena | **estimada**: de ~41 100 piezas a ~1 400 celdas (2–3 mil lotes con transparentes y brillos); de 65 545 a unos 27 000 renderers | `rendimiento-ab` (consola) o el menú Medir A y B |
| **Detalle apagado de lejos** (dentro del lote) | nada | **estimada**: 27 500 piezas de < 1,2 m fuera a más de 120 m | línea «lotes de detalle apagados» |
| **Cuerpos y grupos lejanos** (> 130 m) sin dibujar | nada | depende de dónde esté la cámara | línea «cuerpos lejanos sin dibujar» |
| **SSAO sin prepaso, 2 cascadas, 40 m** | menú *Aplicar ajustes ligeros* | **estimada**: fuera una pasada entera; el marcador `DepthNormalPrepass` debe desaparecer | medir antes y después, con *Restaurar* para volver |
| **GPU Resident Drawer** | menú *Activar…* (compila sombreadores la 1.ª vez) | sin estimar: instancia lo que queda fuera de lotes | medir antes y después; ver el riesgo de abajo |
| `EmotionReader` | nada | pequeña | `script EmotionReader.Update()` en la sonda |
| `TreeRotator`: el NavMesh se recoloca cada 0,5° (antes 0,05°) | nada | sin medir; sospechoso principal | `coste TreeRotator.Update()` en *Perfilar scripts* |
| `WorkdayDirector.Body`, `DailyLife`, `SoundDoor`: sin búsquedas repetidas por toda la escena | nada | sin medir | ídem |
| Figura pausada en cuerpos lejanos (`CubeGait`, `CubeWalkCycle`, `BodyPartReactor`, `CubeFace`) | nada | sin medir; 105 de 131 cuerpos estaban lejos | sus líneas `coste` |
| Etiquetas en rejilla: 9 celdas cada 0,25 s, no 1 549 en cada repintado | nada | sin medir | `AnimaNameplate.OnGUI()` |
| Pila desactivada en `Debug.Log` (avisos y errores la conservan) | nada | sin medir | línea `Debug.Log:` de la sonda |
| `MaskStation`, `CampoMagnetico`, `TreeDoorExit`: una de cada N veces, o un solo chequeo por frame | nada | pequeña | sus líneas `coste` |

No se hizo, y por qué:

- **Merge en el Blockout:** obliga a reconstruir, y las copias de piso de los microcosmos y las pruebas leen esas piezas. El
  lote en Play da lo mismo sin tocar la escena. Las escenas de 80 MB y el tiempo de carga siguen igual.
- **`DrawMeshInstanced` a mano para los peces:** con el recorte a 130 m y el Resident Drawer no compensa reescribir `Swarm`.
  Si la sonda los pone arriba, se retoma.
- **Activación por piso:** los pisos no tienen paredes y se ven unos desde otros; ya lo cubren celda y distancia. La oclusión
  por GPU del Resident Drawer sería lo siguiente.
- **Colliders:** los `BoxCollider` son estáticos y no cuestan por frame. Salvo los del Árbol de los Campos, que gira con 142
  colliders dentro: mirar `Physics.SyncTransforms` en la sonda.

## Cómo funciona el lote estático

`LoteEstatico` nace solo al entrar en Play y actúa 1,5 s después de cargar cada escena, en todas, también en las aditivas.

- **Las piezas siguen ahí.** Su GameObject, su nombre, su collider y `r.enabled = true`: solo llevan `forceRenderingOff`. Por eso
  `GameObject.Find`, `transform.Find`, `r.bounds`, las pruebas que leen `r.enabled` y las herramientas del editor no notan nada.
  Las copias de piso de los microcosmos se hacen en el editor, donde no hay lotes.
- **Agrupa** por celda (detalle 20 × 6 m, estructura 48 × 18 m), material, tamaño, sombras y capa.
- **Paleta:** lo opaco y liso de URP/Lit comparte una textura con un texel por color. Los 1 134 materiales casi solo se
  diferencian en el color, y sin esto cada celda serían decenas de dibujados. Se separa por suavidad y metal.
- **Se queda con su material:** lo emisivo, lo texturizado, lo que no es URP/Lit y lo transparente (solo hasta 1,5 m).
- **No entra** nada bajo un `Anima`, `Animator`, `CreatureRig`, `CubeJoint`, `Swarm`, `Hive`, `GardenCrop`, `FruitPlant`,
  `Rigidbody`, agente, puerta, `StationPart`, submarino, ni el texto. Tampoco lo que usa `MaterialPropertyBlock` ni lo que pasa de 60 m.
- **Anclas:** `TreeRotator` y `FlyingCarpet` se mueven enteros; sus lotes cuelgan de ellos.
- **Autocuración:** una barrida de 1 ms por frame mira cada pieza. Si se apagó, se movió, cambió de material o se destruyó, sale
  del lote, vuelve a dibujarse sola y el lote se rehace (3 por frame). Una vuelta entera son unos 70 frames: una máscara cogida
  puede seguir viéndose 2 s a 30 FPS.

Lo nuevo que aparezca después de la pasada no entra en lotes; se dibuja como siempre.

## Apagarlo y medirlo

- Menú *Perfilar scripts (40 frames, con nombres)*: enciende el perfilador en Play y da `coste Clase.Update()` por tipo y `propio`,
  lo más caro por tiempo propio. Es la medida que nombra scripts; `PerfProbe` sin perfilador puede salir vacía.
- Consola: `rendimiento` (una medida) y `rendimiento-ab` (con lotes, sin lotes y con lotes otra vez).
- Menú `Tools/Cold Sanctuary/Rendimiento/`: *Medir A y B*, ajustes ligeros, Resident Drawer y lotes en este equipo.
- La sonda escribe `[Rendimiento]` en el log: FPS, hilo principal y de render, pasadas de URP, scripts más caros, renderers
  por builder y por pieza, y el estado del lote.
- Comparar la 1.ª medida con la 3.ª da el ruido. La cámara no debe moverse durante la medida.

## Perfil por script (ronda 13, *Perfilar scripts*)

Scripts que mandan: `Mind.Update` 52 ms (31 llamadas, casi todo tiempo propio), `WorkdayDirector.Update` 20 ms y 236 KB por frame, `Swarm` 6,5,
`SpeciesScaleFix` 6,1 y 275 KB, `TreeTrip` 6, `VisualAttention` 5.

`Mind.Update` solo hace aritmética y un `Debug.Log` por pensamiento. El log tuvo 1,8 `[Mente]` por frame y el tiempo sale como propio, así que
**el sospechoso es el `Debug.Log`** (hipótesis, sin medir).

| Cambio | Conducta |
|---|---|
| `LogQuieto`: 8 etiquetas charlatanas, 1 mensaje cada 5 s por etiqueta | Solo cambia el log; con TestRunner sale todo. Avisos y errores, intactos |
| `Mind`: humores y campos 1 de cada 6 frames; `ThoughtField.All` en vez de buscar | Mismo tiempo de juego (`GameTick`); un campo nuevo actúa antes, no 1 s después |
| `WorkdayDirector`: 1/8 del personal por frame | Empezar y acabar misiones sigue en cada frame; alguien tarda hasta 8 frames en recibir su orden |
| `SpeciesScaleFix`: ya en su talla, mide cada 20–27 s o al cambiar de etapa | Si otro script lo reescala, tarda más en corregirlo |
| `Swarm`: pasto 1 de cada 6 frames, lista común de cazadores, dueño del collider en caché | Mismo pasto por tiempo de juego |

**`GameTick`** va por frames y no por segundos: a 4 FPS un intervalo de 0,2 s se cumpliría siempre y no ahorraría nada. Entrega todo el
`Time.deltaTime` acumulado. Con TestRunner toca en cada frame, así que las pruebas de conducta no prueban el reparto; lo prueba el grupo 112 con
dt conocidos. `SenseSurroundings` sigue en cada frame con su dt, para no cambiar su efecto.

## Lo que solo cuesta en el editor

Un juego construido no tiene esto, así que los FPS del editor no son los del jugador:

- **C# en modo depuración** y con los marcadores de cada llamada. Es lo que más pesa en un goteo de 14 000 scripts.
- **Ventanas del editor:** Hierarchy con 85 000 objetos, Inspector, Console, Scene si está abierta, y el puente MCP y el buzón.
- **`Debug.Log` hacia Console**, con pila y volcado al log.
- **Comprobaciones de seguridad** nativas y de colecciones.

Se ahorra poco de lo que es trabajo de verdad: el NavMesh, las búsquedas y la física cuestan igual en un build.

## FPS fuera del editor

`tools/build-perf.sh`, solo con SampleScene (las 300 escenas no caben en memoria):

1. `tools/build-perf.sh build final`, con el editor **cerrado** y licencia válida. Con el editor abierto, el menú *Build de medida (final)*.
   Estará parado mientras tanto, y la primera vez compila variantes de sombreadores: pueden ser decenas de minutos.
2. `tools/build-perf.sh run final`: abre una ventana de 1280 × 720, a los 45 s mide con y sin lotes y se cierra. Resultado en `Builds/Perf/perf.log`.
3. Para nombres de scripts: `build dev` y `run dev`, o conectar el Profiler al build de desarrollo. Sus FPS no son los finales.

Sin comprobar: que `LoteEstatico` encuentre legible el cubo primitivo en un build. Si el log dice 0 piezas, hay que resolver eso antes
de comparar. También puede faltar alguna variante de URP/Lit para materiales creados en Play.

## Riesgos

- **`forceRenderingOff` con Resident Drawer sin comprobar:** ningún paquete SRP lo menciona. Si el drawer no lo respeta, las piezas
  se dibujarían además del lote: no se vería mal (salvo los visores transparentes, más opacos), pero no ganaría nada. Por eso va
  aparte y se mide por separado.
- **Que `forceRenderingOff` ahorre todo lo que ahorraría apagar el renderer** está sin medir: la pieza puede seguir pasando
  por el recorte. El dibujado sí se va.
- **Color:** la paleta debe verse igual que el material; revisar en fotos, sobre todo colores muy oscuros.
- **Tirón al cargar:** la pasada corre en un frame, estimado en menos de 1 s. Su duración sale en el log.
- **Memoria:** unos 60 MB de mallas en GPU, y sin copia en CPU.
- **Luz:** cada lote toma una sola muestra de sondas de luz y reflexión.
- **Piezas ocultas con `r.enabled = false`:** se detecta en la barrida, no al instante.
- **Figura pausada de lejos:** al volver, un cuerpo retoma la pose del momento en que se alejó, y el TestRunner desactiva el recorte, salvo en
  el grupo 112. Una prueba que mire la figura de un cuerpo lejano sin TestRunner la verá parada.
- **Pila apagada en los `Debug.Log`:** si hace falta para depurar, `Application.SetStackTraceLogType`.
- **Etiquetas:** aparecen con hasta 0,25 s de retraso, y las nuevas con hasta 5 s.
- **NavMesh del árbol:** va hasta 0,5° por detrás del piso (35 cm a 40 m del eje).
