# El Árbol de los Campos (aprobado y construido, 2026-10-29)

Idea de Berón: juntar los campos en un solo edificio con forma de árbol.
- **Raíz:** una columna central muy fuerte de unos 100 m.
- **Tronco:** pisos de cultivo inclinados, cada uno al revés que el de arriba (un acordeón).
- **Ramas:** columnas hijas inclinadas que nacen de la raíz.
- **Flores o frutos:** en la punta de cada rama, un anillo de nidos como los del hospital y el restaurante.
- **Giro:** la raíz gira y con ella gira todo el árbol, para repartir sol, sombra y lluvia.

## Lo que decidió Berón (2026-10-29)

- **Adelante con el árbol.**
- **Alfombras solo arriba y abajo**: en la COPA (la única plataforma plana; gira con el árbol) y en el SUELO, al pie. Entre
  pisos no hay alfombras: se va por dentro de la raíz.
- **La raíz es un depósito de agua**, como el tanque de virtualización: cada piso tiene una puerta a ella; un campo magnético
  impide que el agua salga por las puertas. Se entra en la raíz, se nada hacia arriba o hacia abajo y se sale por la puerta del
  piso que se quiere. Dentro se puede nadar libremente.
- **La raíz mide unos 20 m de ancho**: es el único apoyo de todos los pisos y debe ser al menos tan ancha como la plataforma
  de alfombras de su copa.
- **La raíz es TRANSPARENTE como el tanque de virtualización**: se ve a la gente nadar de piso en piso. Nada de un tubo opaco
  de acero y hormigón: la pared es de acrílico grueso y la carga la lleva un exoesqueleto esbelto de nervios de acero.

## Cómo está hecho

Código: `Assets/Editor/SampleSceneBuilder.Tree.cs` (lo construye), `Assets/Scripts/World/TreeSpec.cs` (las medidas, una sola
vez), `ArbolCampos.cs` (niveles y rutas), `TreeTrip.cs` (el viaje a nado), `TreeWalk.cs` (el hechizo «piso»), `CampoMagnetico.cs`, `TreeFloor.cs`, `TreeRotator.cs` (el giro), `StructuralLimits.RootCheck`
(la raíz aguanta) y `Assets/Scripts/Tests/TreeOfFieldsTest.cs` (la prueba, grupo 53).

| Parte | Medidas | Notas |
|---|---|---|
| **Raíz** | 0–101 m; agua Ø 20 m (de 0,05 a 99,6 m); pared de acrílico de 1 m (Ø 22 m) | El mismo cristal que el tanque de virtualización (`VirtualizationTank.GlassColor`, alfa 0,18) y su agua, más clara (alfa 0,2: 20 m de agua). El agua es un `WaterZone` (la superficie es lo alto del volumen): se nada con `SwimBody` como en el mar. Entre el agua y la tapa quedan 1,4 m de aire. |
| **Exoesqueleto** | 16 nervios CFST (Ø 0,8 m) y zunchos de canto (0,25 m de alto × 0,36 m hacia fuera) cada 6 m | Una sola malla; no cruza las puertas ni las bocas de las ramas. Es lo que carga: el acrílico no se cuenta como portante. |
| **Puertas** | Una por nivel: suelo y 8 pisos (y una en la campana de cada rama); 1,8 × 2,6 m | Hueco en la pared con bobinas de cobre y el **campo magnético** (la lámina de luz del tanque) en la cara de dentro. Los cuerpos lo cruzan; el agua no. El hueco de cada puerta es aire: una **campana de aire** para respirar a mitad de subida. |
| **Tronco** | 8 pisos: 6 · 13 · 24 · 31 · 38 · 45 · 52 · 62 m | Octógonos de ~51 m (rectángulo de 46 × 46 + 2,5 m y chaflanes de 5 m, como las losas de las torres) con el hueco de la raíz. En voladizo: ménsulas y tornapuntas de acero hasta la raíz. **Girados 45°** uno sobre otro e **inclinados 3°** alternando el sentido. Entre el 2 y el 3, 11 m (la guadua llega a 9 m); entre el 7 y el 8, 10 m (el dosel del bosque comestible); los demás, 7 m. |
| **Riego en cascada** | Canaleta en el borde bajo de cada piso | Vierte (una lámina de agua, sin collider) al piso de abajo. El arrozal no: va en terraza plana y su agua vuelve a la raíz. |
| **Ramas** | 6 tubos de agua Ø 3 m, a 35°, de 40 m, desde 66 m | A 22,5°, 90°, 135°, 202,5°, 270° y 315° (entre los nervios y lejos de la puerta del piso 8). Collar de acero en la boca. La punta llega a ~44 m del eje y ~89 m de alto: sobresale de los pisos. |
| **Anillos de nidos** | `RingDeck` de 13,4 m alrededor de una campana de agua (Ø 4 m) | Frutales enanos en las esquinas, flores para las abejas de las nieves y nidos de aves. A ~91 m. Se llega nadando: por la raíz, por la rama y por la puerta de la campana. Se llaman `Nido_anillo_N` (no son anillos de un piso: no llevan alfombras). |
| **Copa** | Octógono plano de 22 m a 101,4 m | Tan ancho como la raíz. Alfombras de edificio y del santuario. Gira con el árbol. |
| **Suelo** | Alfombras en el hielo, a 22 m del eje (al NE): las del edificio (a la copa) y, al lado, las del santuario (a las otras torres) | No giran. Desde ahí se anda a la puerta del suelo. Desde otra torre se vuela directo al pie (antes, a la copa y otra vez al suelo: dos vuelos). |

**Reparto, de abajo arriba:** P1 Arrozal (en terraza plana) · P2 Bambusal · P3 Milpa · P4 Chacra andina · P5 **Huerto**
(`Garden_Area`, con las hortalizas, su tanque de virtualización y su iniciación) · P6 Rotación · P7 y P8 Bosque comestible.
Cada cultivo conserva su gremio y su semilla (`GardenPlot`): hortalizas 11, milpa 12, chacra 13, arrozal 14, rotación 15,
bosque 16 y 18, bambusal 17. Cada bancal es una franja de 7 × 38 m (las filas a lo largo: caben todas las del gremio) en el lado del piso de su
puerta: se sale de la raíz al bancal. El Huerto es un área de 44 × 9 m con su bancal de 6 × 28 m; el segundo bosque
comestible planta las otras especies del dosel (`GardenPlot.speciesOffset`). Cada piso tiene sus plazas reservadas del microcosmos (`PlatformSlots`), calculadas
con el árbol sin girar.

## Moverse

- **Personajes**: `ArbolCampos.Route` va dentro de `CarpetPad.Route` (la usan `PathMover`, la jornada, las iniciaciones y los
  hechizos), antes que las alfombras; un piso del árbol nunca llega como destino a la red de alfombras: desde fuera, el
  destino es el pie del árbol. En el mismo piso, andando (sin alfombras); a
  otro nivel, primero a la puerta de su piso y después un **viaje a nado** (`TreeTrip`): puerta → agua → por la columna a 6 m
  del eje → [la rama y su campana] → puerta → fuera, nadando con `SwimBody.SwimTo` a 2,5 m/s y, a lo largo de la columna, llevados además por la **corriente de la raíz**
  (`ArbolCampos.routeCurrent`, 1,4 m/s: el agua sube por un lado y baja por el otro; en total 3,9 m/s, por debajo de los 4 m/s que las pruebas toman por un salto): del suelo al piso 5 en ~9,5 s en vez de 15
  (no se quedan sin aire; la prueba lo mira). Nada de teletransportes; si se atasca, se queda donde está y avisa. Desde fuera del árbol, las
  alfombras le traen al pie (o a la copa) y entra por el suelo.
- **La jornada de Kushal** (`PlayerWorkday.HeadingEarly`): ir de una torre al huerto del piso 5 cuesta ~1,5 min de juego (alfombra
  al pie, andar a la puerta, nadar), más de la mitad de una misión de 1 h con el reloj ×6. Si ya hizo ≥ 55 % de la misión que
  acaba y la siguiente es en otra área, el piloto sale en los últimos 0,4 h hacia la siguiente (la Magnate no lo devuelve: su
  puesto ya es el siguiente). Así llega a su hora sin alfombras entre pisos.
- **El jugador**: nada libremente dentro (el cuerpo de nado del mar). En cada puerta puede respirar. El hechizo
  `piso({n:5})` (0–8, `anillo1`…`anillo6`) le lleva andando a su puerta y nadando por la raíz.

## Giro

`TreeRotator` gira el árbol en sentido **contrario al sol** (mira hacia dónde avanza el acimut del sol del santuario), por
defecto **una vuelta cada 6 horas del juego** (15 min reales). Hechizo `arbol({vueltaHoras:N})`; `0` lo para.
- Quien está de pie en un piso, un anillo o la copa **gira con él** (se le aplica el mismo giro cada frame; con
  CharacterController, con `Move`).
- El árbol tiene **su propio NavMesh** (`NavMeshSurface` en su raíz, horneado en sus coordenadas, fuera del horneado general)
  que se recoloca con él cada 0,25 s. Al pie de la raíz, en el NavMesh general, el círculo de la columna no se anda.
- Los muebles de un piso que gira se miden en el marco del piso (`FurnitureSpacing.Frame`): los 2 m de paso no dependen del giro.

## Estructura (`StructuralLimits.RootCheck`)

Con 8 pisos, 6 ramas, la copa y su pared: ~286 MN sobre 16 nervios (~614 MN) → **47 %**. El agua (~308 MN) apoya en el fondo y
va al cimiento. El empuje del agua en el fondo (p·r ≈ 9,8 MN/m) lo resisten el acrílico de 1 m y los zunchos (cada 6 m) → **64 %**. Hasta el 2026-10-30 los zunchos iban cada 2 m y de 0,6 m de alto (38 %): desde lejos la raíz se leía como una columna blanca a rayas y de cerca casi no se veía al nadador; con la misma sección de acero puesta de canto y tres veces más espaciada tapa la vista ~8 veces menos y sigue por debajo del 80 %. Avisa
por encima del 80 % (con 30 pisos, avisa) y da error por encima del 100 %.

## Dónde

En el lóbulo **Sur:Milpa**, centro (−50, −84): la raíz y la puerta del suelo (que gira a 13,5 m del eje) quedan sobre hielo
firme. La torre de **Producción** sigue en (18, −95), más baja: Construcción (12 m), Mecánica (23 m), Textil (34 m) y su
techo de alfombras (45 m). Distancias medidas con el simulador (`-tree`, puntos reales de las mallas, el árbol girando):

| Vecino | Hueco a los pisos | Hueco a los anillos |
|---|---|---|
| Producción | 22,3 m | 4,1 m en planta (los anillos van por encima de 82 m; la torre llega a 48 m con sus paneles) |
| Entrenamiento | 41,0 m | 22,9 m |
| Ciencias | 85,4 m | 67,2 m |
| Central Nuclear | 74,7 m | 56,5 m |

## Riesgos y pendientes

- Los anillos pasan a 4 m en planta del techo de Producción (con más de 30 m de altura entre ellos). Si Berón quiere los 16 m
  de la regla 4 también en planta, hay que mover Producción (no cabe en su lóbulo) o acortar las ramas.
- Un jugador que se salga nadando por las juntas de una rama con su campana caería: las juntas están cerradas con collares,
  pero no es una pieza estanca. Probar en vivo.
- El alga del arrozal se planta en el mar más cercano y es hija del bancal: gira con el árbol.

## En vivo (2026-10-29/30)

- `TreeOfFieldsTest` (grupo 53, también sola en «Tools/Cold Sanctuary/Pruebas/Árbol de los Campos (sola)»): 28/28. En la batería
  completa (1010 PASS / 9 FAIL; antes del árbol en vivo, 973 / 36): 27/28 — el tramo del suelo al piso 5 anduvo 94 m para 51 en recta
  (la métrica de las vueltas pide ≤ 84): ~36 m andando por el piso 5 tras salir de la puerta, sin aislar aún.
- Lo que salió en vivo y se arregló: cada MonoBehaviour en su propio archivo (Unity perdía `TreeFloor` al guardar la
  escena); la pista de alfombras del suelo colgada de «Areas_Placed» se hacía pasar por la pista de cualquier piso sin
  pista propia (la gente se iba al pie del árbol desde la Sala de Yoga); desde la copa o desde fuera, el destino «un
  piso del árbol» confundía la red de alfombras (vueltas entre la copa y Producción); `SwimTo` hacia el hueco de una
  puerta (aire) bajaba al nivel del mar; las puertas pasan al lado del cultivo; la rama de 0° tapaba la puerta del piso 8;
  las alfombras posadas en la copa giran con ella; los bancales, con las filas a lo largo (vuelven todos los cultivos).

### 2026-10-30
- Raíz: zunchos de canto cada 6 m (ver Estructura): desde lejos ya no es una columna blanca a rayas. Lo blanco que queda a
  ~200 m es la niebla de la escena (exponencial², 0,008: a 200 m es el 92 % de niebla), como el resto de edificios a esa distancia.
- Batería completa: 1010 PASS / 7 FAIL (antes 1010 / 9). Arreglados: el paseo por el piso 5, Kushal en el huerto (38 % → 100 %),
  la clase de yoga y Linduarda en el examen. Después (solo comprobado en la jornada sola): la comida de Kushal (en el techo de
  Servicios las dos pistas están casi juntas y la ruta lo mandaba de una a otra), la escapada y el vigía con la salida anticipada.
