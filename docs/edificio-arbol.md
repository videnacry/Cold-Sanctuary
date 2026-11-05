# El Árbol de los Campos — v2, la hélice cuadrada (Berón 2026-10-30)

Idea de Berón (2026-10-29): juntar los campos en un solo edificio con forma de árbol: una raíz-columna de agua por la que se
nada de piso en piso, pisos de cultivo, ramas con algo en la punta, y que todo gire para repartir sol, sombra y lluvia.

## Lo que decidió Berón

**2026-10-29 (v1, sigue valiendo):** adelante con el árbol; **alfombras solo arriba y abajo** (en el techo, que gira con el
árbol, y en el suelo, al pie); entre pisos se va **nadando por dentro** del tronco, que es un **depósito de agua
TRANSPARENTE** como el tanque de virtualización (se ve nadar a la gente), con una **puerta con campo magnético** en cada piso
que sujeta el agua; el tronco mide **unos 20 m de ancho** y es al menos tan ancho como la plataforma de alfombras de su techo.

**2026-10-30 (v2, esta):**
- «Cambiamos los anillos por los **pisos de campo** que teníamos, y **encima de esos campos, anillos a la medida**.»
- «El árbol debe tener **una entrada en el suelo y otra en el techo**.»
- «El tronco pasa a ser **CUADRADO** y las plataformas **salen de sus lados**. Para que no se tapen, se alternan lados vacíos
  y lados con piso: en un tramo del tronco con lados a, b, c, d, a y c tienen piso y b y d quedan vacíos; en el siguiente
  nivel, b y d tienen pisos de campo y a y c quedan libres. **Como una hélice.**»
- «El primer piso empieza a **50 m**… y debe quedar **por encima del árbol más alto del santuario**: su suelo queda libre
  para poblarlo de árboles.»
- «Las ramas: **una en cada nivel en cada lado que queda sin piso**, que lleva a un campo de cultivo. Así cada nivel de la
  hélice tiene 2 pisos de plantas pegados al tronco y 2 ramas a un campo.»
- «Las ramas y el tronco deben ser **CUADRADOS** para encajar con las bases cuadradas de las plataformas. Las plataformas
  vuelven a ser **cuadradas**.»
- Fallo: «Entré en una rama de anillo: **pude nadar por la rama pero no pude salir de ella**.»
- «El siguiente paso será **migrar los otros edificios a este modelo de árbol**.» (Aún no: el modelo ya es genérico, ver
  §Modelo genérico.)
**2026-11-01:** «Duplica el grosor de las ramas y el espacio vertical entre pisos»: ramas de 10 × 10 m, niveles cada 22 m (ver §Ramas y niveles al doble, que manda sobre las medidas de abajo).

## El árbol más alto (medido el 2026-10-30)

En los constructores: los árboles de hielo crecen por edades (`PolarLandscapeBuilder.HeightFor`: brote 2,5–4,5 m, joven
9–14, adulto 22–32, **viejo 32–40 m**) y `CubeTree` los hace ×1,3 → **52 m de tronco y ~55,5 m con la copa** (los del bosque,
los de junto a las áreas —24–36 m ×1,3 = 47 m— y los de la arboleda residencial —26 ×1,3 = 34 m— quedan por debajo). El único
más alto es el **árbol de la guardia** (83 m, ~88 m con la copa), que es único, fijo en su isla del noroeste y no crece: no
cuenta. `TreeSpec.TallestTree` = 55,5 m; la prueba mide los árboles de la escena y comprueba que ninguno pasa de ahí.

**Primer piso = máx(50 m, 55,5 + 4 m de holgura + 4 m de tornapuntas) = 64 m.** Lo más bajo de los pisos y las ramas (las
tornapuntas) queda a ~60 m: 4 m por encima de la copa más alta. Abajo solo pasa el tronco (y su puerta del suelo): el hielo
alrededor queda libre, sin pavimento, para los árboles.

## Cómo está hecho

Código: `Assets/Editor/SampleSceneBuilder.Tree.cs` (lo construye), `Assets/Scripts/World/TreeSpec.cs` (`HelixTower`, el modelo
genérico, y `TreeSpec`, las medidas del árbol), `ArbolCampos.cs` (niveles y rutas), `TreeTrip.cs` (el viaje a nado),
`TreeDoorExit.cs` (salir del agua por una puerta), `TreeWalk.cs` (el hechizo «piso»), `CampoMagnetico.cs`, `TreeFloor.cs`,
`TreeRotator.cs` (el giro), `StructuralLimits.RootCheck` (el tronco aguanta) y `Assets/Scripts/Tests/TreeOfFieldsTest.cs`
(la prueba, grupo 53).

| Parte | Medidas | Notas |
|---|---|---|
| **Tronco** | Cuadrado, 0–92 m; agua de 20 × 20 m (de 0,05 a 91,5 m); pared de acrílico de 1 m (22 × 22 m) | Cristal casi invisible y agua azul (`TreeGlassColor`, alfa 0,06, liso 0,6; `TreeWaterColor`, alfa 0,45), sin sombra (2026-11-01: con el cristal del tanque y el agua a 0,2 los troncos se veían de lejos como rejillas blancas opacas; ver §En vivo). El agua es un `WaterZone` (caja): se nada con `SwimBody`. |
| **Exoesqueleto** | 24 nervios CFST (4 esquinas y 5 por cara, uno cada ~3,7 m) y zunchos de canto (0,25 × 0,36 m) cada 6 m | Una sola malla; no cruza las puertas, las bocas de las ramas ni las losas. Es lo que carga: el acrílico no se cuenta como portante. |
| **Puertas** | Suelo, 4 pisos, 4 campos de punta, 4 anillos y el techo; **3 × 3 m** (Berón 2026-10-31: más anchas) | Hueco con bobinas de cobre y el **campo magnético** en la cara de dentro. Los cuerpos lo cruzan; el agua no: el hueco es una **campana de aire**. Cada una lleva su `TreeDoorExit`. |
| **Hélice** | 2 niveles: **64 m** (pisos a y c) y **75 m** (pisos b y d) | Lado a = +Z, b = +X, c = −Z, d = −X del árbol (gira). |
| **Pisos pegados** | 4 cuadrados de **32 × 32 m**, planos | Nacen en su cara del tronco y se corren 10 m hacia la derecha (mirando hacia fuera): un **molinete**. Así ningún piso tapa a otro en planta, ni los de un nivel a los del otro. En voladizo: 4 ménsulas y 4 tornapuntas CFST que bajan 4 m a la cara. Puerta frente a su centro, a 5 m del eje de la cara. |
| **Ramas** | 4 tubos **cuadrados** de agua con un hueco de **5 × 5 m** (Berón 2026-10-31: caben varios a la vez), paredes de 0,3 m, a la altura de su nivel | Salen rectas de la cara que no tiene piso (8 m) y giran a la derecha (20 m) hasta la columna de la punta. Celosía de acero de 5 m de canto (2 cordones abajo, 2 arriba y diagonales). |
| **Columna de la punta** | Agua de **5 × 5 m** (paredes de 0,3 m), del fondo de la rama al anillo (agua 0,6 m por encima de la puerta del anillo) | En la esquina de dentro del campo. **Puerta al campo** (+X, al nivel del campo) y **puerta al anillo** (−X, al nivel del anillo). |
| **Campos de punta** | 4 cuadrados de **16 × 16 m**, a **70 m** (nivel 1, lados b y d) y **81 m** (nivel 2, lados a y c) | En la esquina libre del molinete: 4 m más allá del borde del piso de ese lado y 4 m fuera del tronco. 3 tornapuntas a la columna. |
| **Anillos** | 4 `RingDeck` de 4 m de ancho, 4 m encima de cada campo (22,4 m de lado; 0,8 m sobre el campo y el resto volando) | `Nido_anillo_N`: frutales enanos en las esquinas, flores para las abejas de las nieves y nidos de aves. Sin alfombras: se llega nadando por la columna. |
| **Techo** | Cuadrado plano de 22 m a 92 m | Tan ancho como el tronco. Alfombras de edificio y del santuario, y la **PUERTA DEL TECHO** (Berón 2026-10-31: como la de los anillos, se entra de frente): una columna de agua de 5 × 5 m que asoma 3,6 m sobre la cubierta, encima del agua del tronco, con su puerta de 3 × 3 m hacia las alfombras. (Antes, una trampilla sobre el agua: se caía dentro.) Gira con el árbol. |
| **Suelo** | Alfombras en el hielo, a 24 m del eje (al NE) | No giran. La **puerta del suelo** está en la cara a (gira con el tronco). |

**Alcance** (lo que barre al girar): 55,9 m (la esquina de un anillo). **Reparto, de abajo arriba:**
P1 Bosque comestible (piso a, 64 m) · P2 Bambusal (piso c, 64 m) · P3 Arrozal (punta b, 70 m) · P4 Milpa (punta d, 70 m) ·
P5 **Huerto** (`Garden_Area`, 24 × 9 m, con las hortalizas, su tanque de virtualización y su iniciación; piso b, 75 m) ·
P6 Bosque comestible (piso d, 75 m) · P7 Chacra andina (punta a, 81 m) · P8 Rotación (punta c, 81 m). Lo alto (el bosque,
~10 m de dosel; la guadua, 9 m) va en los pisos grandes, donde nada lo tapa; lo bajo, en las puntas. Cada cultivo conserva su
gremio y su semilla (`GardenPlot`): hortalizas 11, milpa 12, chacra 13, arrozal 14, rotación 15, bosque 16 y 18, bambusal 17.
Bancal de 22 × 18 m en cada piso (lejos de la puerta y de la rama que pasa 11 m por encima del borde pegado al tronco) y de
10 × 15 m en cada punta (a lo ancho del campo, filas a lo largo: caben todas las del gremio; con 9 × 9 la chacra perdía el cedrón y la rotación el garbanzo, el sésamo y el maní). El arrozal planta su alga en el mar más cercano (ahora en un grupo quieto, `Algas_…`, que no gira con el
árbol). Cada plataforma tiene sus plazas reservadas del microcosmos (`PlatformSlots`).

### Plano (desde arriba; cada carácter, ~2 × 4 m; T = tronco)

```
  nivel 1 (64 m): pisos 1 (a) y 2 (c); ramas de b y d a las puntas 3 y 4 (70 m)
  nivel 2 (75 m): pisos 5 (b) y 6 (d); ramas de a y c a las puntas 7 y 8 (81 m)

                     +Z (lado a)
  ................1111111111111111...........
  ......44444444..1111111111111111...........
  ......44444444..1111111111111111..77777777.
  ......44444444..1111111111111111..77777777.
  66666666666666661111111111111111..77777777.
  6666666666666666TTTTTTTTTTT5555555555555555
  6666666666666666TTTTTTTTTTT5555555555555555   +X (lado b)
  6666666666666666TTTTTTTTTTT5555555555555555
  ...........22222222222222225555555555555555
  .88888888..2222222222222222................
  .88888888..2222222222222222..33333333......
  ...........2222222222222222..33333333......
```

De lado (corte por la cara b): la rama de b sale a 64 m por debajo del piso 5 (75 m) y lleva a la punta 3 (70 m) en la esquina
libre; su anillo, a 74 m. Encima de cada piso no hay nada (salvo, 11 m más arriba y solo sobre su franja pegada al tronco, la
rama del otro nivel).

## Moverse

- **Personajes**: `ArbolCampos.Route` va dentro de `CarpetPad.Route` (la usan `PathMover`, la jornada, las iniciaciones y los
  hechizos), antes que las alfombras. En el mismo piso, andando; a otro nivel, primero a la puerta de su piso y después un
  **viaje a nado** (`TreeTrip`): puerta → agua → [rama: tramo recto, codo, tramo a la derecha → columna] → por el tronco a 6 m
  del eje → [rama y columna] → puerta → fuera, nadando con `SwimBody.SwimTo` a 2,5 m/s y, en los tramos verticales (el tronco
  y las columnas), llevados además por la **corriente** (1,4 m/s). Del campo a su anillo (o al revés), por la misma columna,
  sin bajar a la rama. Nada de teletransportes; si se atasca, se queda donde está y avisa.
- **Entradas**: desde fuera del árbol, las alfombras llevan **al techo** (los campos están a 64–85 m: es la entrada más
  cercana) y se entra de frente por la puerta del techo, nadando hacia abajo; para ir al suelo del árbol, al pie. Para salir, se nada al techo (sus
  alfombras), salvo si el destino está en el hielo junto al pie: por la puerta del suelo. Entre el suelo y el techo, en
  alfombra.
- **El jugador**: nada libremente dentro (el cuerpo de nado del mar). En cada puerta puede respirar. **Salir del agua**
  (el fallo de Berón: en la campana de la v1 el agua llegaba casi al dintel y se quedaba flotando contra él):
  `TreeDoorExit` — nadando delante de cualquier puerta y empujando hacia ella, sale de pie al umbral de fuera (como al trepar a
  un canto). Y ya no se sale por donde no hay puerta: `PlayerController.TryClimbOut` (trepar a un canto) solo sube si no hay un techo ni una pared entre el nadador y el canto (antes, flotando contra el techo de un tubo, subía encima del tubo: era el «salir por las paredes» de Berón; igual bajo la losa del techo). Para entrar, se anda hacia la puerta y se cae al agua.
  El hechizo `piso({n:5})` (0–8, `anillo1`…`anillo4`, `techo`) le lleva andando a su puerta y nadando.

## Giro

`TreeRotator` gira el árbol en sentido **contrario al sol**, por defecto **una vuelta cada 6 horas del juego** (15 min
reales). Hechizo `arbol({vueltaHoras:N})`; `0` lo para. Se mantiene en la v2: el tronco cuadrado gira entero con su hélice
(las esquinas, a 15,6 m del eje, barren a 0,1 m/s al pie). Quien está de pie en un piso, un campo, un anillo o el techo **gira
con él**. El árbol tiene **su propio NavMesh** (que se recoloca con él cada 0,25 s); al pie, en el NavMesh general, el círculo
inscrito del tronco (11,8 m) no se anda. Los muebles se miden en el marco del piso (`FurnitureSpacing.Frame`).

## Estructura (`StructuralLimits.RootCheck(HelixTower, …)`)

4 pisos, 4 puntas con sus ramas y columnas, el techo y la pared: **174 MN** sobre 24 nervios (921 MN) → **19 %**. El agua
(361 MN) apoya en el fondo y va al cimiento. El agua tira de cada cara p·a en el fondo (9,0 MN/m) contra el acrílico y los
zunchos (15,3) → **59 %**. El acrílico, como placa entre nervios (3,7 m) y zunchos: 7,3 MPa de 10 → **73 %** (lo que manda:
por eso 24 nervios). **Tornapuntas** (2026-11-05): 4, y de 2 en 2 mientras el voladizo pase del 80 % (`StructuralLimits.FloorStruts`; el Sitio Nuclear, pisos de 46 m, lleva 8; `HelixSlab` pone las mismas). **Voladizo** de un piso (su peso a 16 m de la cara): 185 MN·m contra 4 tornapuntas CFST Ø 0,7 m →
**41 %**. **Rama** (punta, columna y tubo a ~30 m): (con los tubos de 5 × 5 m) contra su celosía de ~6,4 m de canto con cordones CFST Ø 1 m (ver el registro de la construcción). Avisa por encima
del 80 % (con 30 niveles de hélice, avisa) y da error por encima del 100 %.

## Dónde

En el lóbulo **Sur:Milpa**, centro (−50, −84). Distancias con el simulador (`-tree`, puntos reales de las mallas, el árbol
girando: para cada altura, el círculo que barre):

| Vecino | Hueco 3D | En planta | Notas |
|---|---|---|---|
| (Antes de la migración) Producción (18, −95) | 19,4 m | −0,9 m | Ya no está ahí: ver §Migración. |
| Entrenamiento-árbol (−77, 52) | **25,9 m** | | (simulador de la migración, piezas en caja) |
| Ciencias-árbol (92, −107) | 43,1 m | | |
| Servicios-árbol (50, 92) | 77,0 m | | |
| Producción-árbol (114, 0) | ≈ 70 m | | |
| Central Nuclear | 78,3 m | 51,5 m | |

## Modelo genérico (ya lo usan todas las torres: ver §Migración)

`HelixTower` (en `TreeSpec.cs`) es el edificio en hélice con sus medidas: medio lado del agua y pared del tronco, primer nivel y
paso entre niveles, nº de niveles, lado de los pisos y de las puntas, hueco y subida de las puntas, anillo, tubo, columna y
puertas. Da los marcos de cada lado (`SideRot`, `FromSide`), qué lado tiene piso en cada nivel (`HasFloor`), dónde va cada pieza
(`FloorCenterS`, `TipCenterS`, `ColumnS`, `DoorS`), su huella (`PlanRect`, `Overlap`) y el alcance (`Reach`).
`StructuralLimits.RootCheck` recibe un `HelixTower`. El constructor del árbol aún lleva cosas propias (cultivos, Huerto,
nidos): para migrar una torre, sacar de `BuildTree` lo que es del modelo (tronco, puertas, pisos, ramas, columnas, techo) a un
constructor que tome un `HelixTower` y una lista de lo que va en cada plataforma.

## Migración: todos los edificios son árboles (Berón 2026-10-31)

«Migrar TODOS los otros edificios del santuario al modelo del árbol», antes de seguir arreglando fallos. Y: el árbol de la
guardia (83 m, en su isla) es una EXCEPCIÓN a «el primer piso por encima del árbol más alto» (es un árbol modificado): que crezca
tan alto como el edificio más alto.

**Código.** El constructor del árbol pasó a ser GENÉRICO: `SampleSceneBuilder.Helix.cs` (`BeginHelix` → `HelixFloor` por piso →
`FinishHelix`: tronco, puertas, ramas, puntas con su anillo, techo y alfombras del suelo). El Árbol de los Campos
(`SampleSceneBuilder.Tree.cs`) solo pone sus cultivos; cada torre (`BeginTreeTower` en `SampleSceneBuilder.Towers.cs`) pone sus
áreas con el mismo `TowerFloor` de antes, que ahora manda cada área al siguiente piso pegado al tronco. En juego, `ArbolCampos.All`
(un componente por árbol) y `ArbolCampos.Route` van de un árbol a otro (se sale nadando al techo, alfombra al techo del otro,
se baja nadando); `SanctuaryArea.Contains/InBuilding/Building` entienden los pisos en hélice.

**Lo que elegí (por defecto, a falta de que Berón diga otra cosa):**
- Cada torre es SU árbol, con sus áreas en sus pisos **en el mismo orden** (nivel 1: lados a y c; nivel 2: b y d; nivel 3: a y c…).
  Los pisos son cuadrados del lado mayor de su área más grande; cada área se amuebla igual que antes, centrada en su piso, con
  su lado −Z contra el tronco (ahí quitamos su contorno: es por donde se entra).
- Las **puntas** de las torres (16 × 16 m, al aire) llevan los **frutales en maceta** (una de cada especie, y otra vuelta…, con su
  punto de cuidados y su goteo; la sala de la experta, en la primera punta de Servicios) y, encima, su anillo con nidos. Los
  techos de las torres son los del árbol (22 m: alfombras, puerta de frente y una fila de paneles solares).
- Un área con **ANILLO** (el comedor del restaurante, las camas del hospital, el de la sala de experimentos; el de yoga se quitó el 2026-11-04, [yoga-pisos.md](yoga-pisos.md))
  lo conserva, en **U** (abierto al tronco, sus alas hasta la cara del tronco) con un **BALCÓN** de 3 m contra el tronco: el anillo
  tiene su propia puerta (3 × 3 m) y se llega nadando, como a los anillos de las puntas. Lo que iba en el lado del tronco del
  anillo (algunas camas, la esquina de vinos, los corrales de foca) se corre a los otros tres lados.
- Las **puertas** de los pisos (y de los balcones) van donde su umbral (3,4 × 4,4 m) queda libre de muebles.
- Entre torres hay **≥ 16 m en 3D** (pisos con sus anillos, puntas, ramas y troncos; el Árbol de los Campos, girando). En sus sitios
  de antes no cabían (cada árbol alcanza 51–71 m y estaban a 70–100 m): se buscó sitio en las islas (tronco en hielo firme) con un
  simulador; Producción, además, sube un tramo.
- Las torres **no giran** (giraría la cocina, el hospital…): solo el Árbol de los Campos.
- La **Central Nuclear** NO se migra: es de un piso (no es una torre), a 12 m, y necesita su sitio de exclusión.

| Edificio | Sitio (antes → ahora) | Giro | Niveles · primer piso · techo | Piso / puntas | Pisos (en orden) | Puntas |
|---|---|---|---|---|---|---|
| **Árbol de los Campos** | (−50, −84), igual | gira | 2 · 64 m · 92 m | 32 / 16 m (hueco 4) | P1 bosque · P2 bambusal · P5 Huerto · P6 bosque | P3 arrozal · P4 milpa · P7 chacra · P8 rotación |
| **Servicios** | (5, 94) → **(50, 92)** (lóbulo de la antigua enfermería) | 90° | 3 · 64 m · 103 m | 40 / 16 m (hueco 10) | P1 Restaurante (anillo comedor) · P2 Hospital: urgencias (anillo) · P5 hospitalización (anillo) · P6 veterinario (anillo) · P9 Cría · P10 Carne cultivada | 6 de frutales (sala de la experta en P3) |
| **Entrenamiento** | (10, 0) → **(−77, 52)** (Norte:Bosque_Comestible) | 0° | **4** (2026-11-04) · 64 m · techo **153 m** con los niveles de 22 m | 36 / 16 m (hueco 10) | P1 Gimnasio · P2 Cardio · P5 Calistenia · P6 Yoga: Hatha · **P9 Yoga: Ashtanga** · P10 Ballet · P13 Dojo · P14 libre (sin anillo) | **8** de frutales |
| **Ciencias** | (92, −108) → (92, −107) | 0° | 4 · 64 m · 114 m | 32 / 16 m (hueco 10) | P1 Planta atómica (TRIGA) · P2 ciclotrón · P5 sala de experimentos (anillo y Foucault) · P6 Química · P9 análisis · P10 Biología · P13 molecular · P14 libre | 8 de frutales |
| **Producción** | (18, −95) → **(114, 0)** (Este:Paso) | 90° | 2 · **96 m** · 124 m | 26 / 16 m (hueco 4) | P1 Construcción · P2 Mecánica · P5 Textil · P6 libre | 4 de frutales |
| Central Nuclear | (60, −162), igual | — | no se migra (1 piso a 12 m) | | | |

(Pk = plataforma k en orden de altura: en cada nivel, sus 2 pisos y luego sus 2 puntas.)

## Riesgos y pendientes

- (Resuelto con la migración: Producción ya no está junto al Árbol de los Campos.)
- Las torres grandes van justas de estructura (Servicios, 40 m de piso: acrílico 82 %, voladizo 80 %, rama 87 %; avisa, no falla). Si
  Berón quiere más margen: más nervios en el tronco o pisos algo más pequeños.
- Para que quepan con ≥ 16 m se movieron Servicios, Entrenamiento y Producción, y Producción sube a 96 m: si Berón prefiere otros
  sitios, el simulador (scratchpad/mig, `clear.dll`) busca combinaciones; con todo a 64 m hay que fusionar Producción con el Árbol
  de los Campos (niveles 3 y 4).
- Los anillos de los pisos en U pierden su lado del tronco: algunas camas, mesas o boxes se corrieron a los otros lados; la piscina
  del reactor TRIGA ya no baja al suelo: cuelga 4 m bajo su losa (como las tornapuntas), con ~4 m de agua sobre el núcleo.
- Las esterillas de yoga siguen siendo alfombras dentro de su sala (no llevan a otros pisos). Desde el 2026-11-04 no hay anillo de yoga: el yoga son dos pisos (Hatha P6 y Ashtanga P9) con dos salas cada uno; la torre del Entrenamiento sube a 4 niveles (simulador `mig`: las holguras con Servicios, 16,5 m, y con el Árbol de los Campos, 16,3 m, no cambian; [yoga-pisos.md](yoga-pisos.md)).
- La salida del agua por las puertas es una «ayuda» (como trepar a un canto): saca de pie al umbral sin animación de trepar.
- El agua del tronco (20 × 20 × 91 m) es mucha: si Berón lo prefiere, el tronco podría ser hueco entre niveles (no se ha hecho).

## En vivo (2026-10-31)

- `TreeOfFieldsTest` sola: **46/46** (8 plataformas cuadradas, hélice alterna, nada tapa a nada, primer piso 64 m sobre el árbol más alto medido en la escena, 49,6 m; 14 puertas de 3 × 3 m; 13 viajes a nado sin saltos ni atascos; 4 a la vez por una rama; el jugador sale por las puertas del campo, del anillo, del techo y de un piso, entra desde el campo y no sale por 10 empujones contra paredes, techos y juntas; giro y NavMesh).
- Jornada sola 46/47 (lo que falla es la fauna atascada, no el árbol), clase de yoga 16/16, iniciación del huerto 10/10 (antes de arreglarlo, 7/10: el agua del tanque de virtualización del piso contaba como agua del árbol, y el tanque quedaba a 1 m del borde del piso).
- Tronco: carga 210 MN / 921 = 23 %; zuncho 59 %; acrílico 73 % (lo que manda); voladizo de piso 41 %; rama 375/785 MN·m = 48 %.
- Fotos en `COLD-SANCTUARY/Temp/Arbol2/`.

## En vivo — la migración (2026-10-31)

Fotos en `COLD-SANCTUARY/Temp/Migracion/<edificio>_*.png` (entero, de lado, en planta, el piso 1 y su puerta, la entrada del techo y
alguien entrando por ella, y nadando del techo al piso 5 y del piso 5 al 1). Cada torre, con sus pruebas, antes de su commit:

| Edificio | Nado (techo → piso 5 → piso 1) | Pruebas |
|---|---|---|
| Servicios | 14 s + 14 s de juego | restaurante 15/15 (los turistas llegan al pie, suben al techo y nadan al anillo), experiencia del jugador 29/29, iniciación de la cocina 15/15 |
| Entrenamiento | 15 s + 15 s | vida del santuario 155/155, iniciación de yoga 12/12, clase de yoga 16/16, gimnasio 142/143 → 150/150 (con la losa 1 m más ancha) |
| Ciencias | 18 s + 14 s | vida 156/156, microcosmos 144/144 |
| Producción | 14 s + 12 s | vida 157/157, islas 11/11, Árbol de los Campos 46/46, edificios-árbol 31/31 |

- `TreeBuildingsTest` (grupo 57): hélice, primer piso ≥ 64 m, áreas en sus pisos, 21–31 puertas de 3 × 3 m por árbol, umbrales libres,
  alfombras solo en el suelo y en el techo, holgura mínima medida entre árboles **18,2 m** (Entrenamiento – Servicios), nado techo →
  último piso → primer piso → suelo en cada torre, y el árbol de la guardia a **128,6 m**.
- Estructura (24 nervios hasta 100 m de tronco, 28 por encima): Servicios acrílico 82 %, voladizo 80 %, rama 87 % (avisa);
  Entrenamiento acrílico 82 %; Ciencias y Producción ~66–72 % con 28 nervios.
- **Batería completa** (con todo migrado y el árbol de la guardia a 128,6 m): **1162 PASS / 21 FAIL** (la primera, 1155/28; se
  arreglaron el «teletransporte» de 64 m de quien se quedaba en una puerta, el nado frenado y la orquesta de la cocina). Quedan:
  - **Jornada (13)**: la gente tarda más en ir de un edificio a otro (nadar al techo, volar 100–200 m, bajar nadando: 35–95 s de
    juego, medidos) y no siempre llega a su puesto, a comer o a entrenar; varios se quedan quietos en la Cocina (el comedor del
    anillo) o «no avanzan hacia los aseos»; + 2 de fauna (pingüinos en círculos y uno contra el tronco del Árbol de los Campos, que
    gira: ya fallaba antes).
  - **Clase de yoga (4)**: Lizmibet no llega a su esterilla en los 120 s de la clase (viene de Servicios a Entrenamiento).
  - **Gimnasio (3)**: dos máquinas de otro piso de la torre no se alcanzan en 60 s (el nado entre pisos cuenta).
  - **Frutales (1)**: con el nado estimado entre puntas y torres salen 7 jardineros al día (17,1 h) en vez de ~3.

## Ramas y niveles al doble (Berón 2026-11-01)

«Duplica el grosor de las ramas y el espacio vertical entre pisos.» Vale para TODOS los árboles (el Árbol de los Campos,
`TreeSpec.Helix`, y las torres, `TowerHelix`: los dos toman las medidas por defecto de `HelixTower`). **Estas medidas mandan sobre
las de las tablas de arriba** (que quedan como historia de la v2):

| Pieza | Antes | Ahora |
|---|---|---|
| Rama (tubo) | hueco 5 × 5 m (`tubeHalf` 2,8), boca a 6,5 m del eje de la cara | hueco **10 × 10 m** (`tubeHalf` 5,3, pared 0,3), boca a **4,7 m** (`mouthX`: la boca, de −0,6 a 10 m, cabe en la cara de 20 m de agua) |
| Celosía de la rama | cordones CFST Ø 1 m | Ø **1,6 m** (`chordD`, chapa del 4 %): con el canto del tubo de 10 m |
| Columna de la punta | agua 5 × 5 m | agua **10,6 × 10,6 m** (`colHalf` 5,3): recibe el tubo entero |
| Campo de punta | 16 × 16 m, 6 m sobre su nivel | **20 × 20 m**, **12 m** sobre su nivel (por encima del tubo). Libre de columna: 275 m² (antes 225) |
| Niveles de la hélice | cada 11 m | cada **22 m** (`levelGap`) |
| Alto del tronco | último nivel + 17 m | último nivel + punta (12) + anillo (4) + 7 m (`HelixTower.RootHeight`) |
| Bancal de las puntas del Árbol | 8,6 × 15 m | 7,2 × 15 m (la franja junto a la columna; las mismas filas, algo más cortas) |
| Quillas de fotobiorreactores | 1,7 m bajo el nivel | 2,4 m (bajo los cordones de 1,6 m); lo más bajo, a 59,6 m (sobre el árbol más alto + 4 m) |

**Alturas**: Árbol de los Campos 64 / 86 m, techo a **109 m** (antes 92) · Servicios 64 / 86 / 108, techo **131 m** · Entrenamiento (desde 2026-11-04, con el piso de Ashtanga) 64 / 86 / 108 / 130, techo **153 m** ·
Ciencias 64 / 86 / 108 / 130, techo **153 m** · Producción 64 / 86, techo **109 m** (ya no sube: ver abajo). El árbol de la guardia
crece hasta el techo más alto con la columna de su puerta (~158 m).

**Estructura** (`StructuralLimits.Reinforce`: nervios de 4 en 4 desde 24 y, si no basta, zunchos más juntos, hasta quedar bajo el
80 %; estimado con las fórmulas de `RootCheck`): Árbol 109 m, 28 nervios y zunchos cada 6 m (rama 46 %) · Servicios 131 m, zunchos
cada 4 m (voladizo de sus pisos de 40 m, 80 %, como antes; rama 79 %) · Entrenamiento 131 m (rama 71 %; con 4 niveles, 153 m como Ciencias: lo recalcula `Reinforce`, sin medir aún) · Ciencias 153 m, zunchos
cada 3 m (rama 63 %) · Producción 109 m (rama 37 %). Los tubos de 10 m llevan 4 veces más agua: por eso los cordones de 1,6 m.

**Sitios** (con las piezas más grandes los árboles alcanzan 57–76 m; nueva búsqueda con el simulador, scratchpad/mig, ≥ 16 m en 3D
y el tronco en hielo firme): **Servicios** (50, 92) girado 90° → **(55, 102) sin giro** · **Entrenamiento** (−77, 52), girado **90°**
(antes 0°) · **Ciencias** (92, −107) → **(92, −117) girada 90°** · **Producción** (114, 0) con el primer piso a 96 m → **(113, −15)
con el primer piso a 64 m** (ya no hace falta subirla). (El comedero se retiró el 2026-11-05: [alimentador.md](alimentador.md).)
Holguras del simulador: Árbol–Entrenamiento 16,3 m, Servicios–Entrenamiento 16,5, Ciencias–Producción 17,2, Servicios–Producción 18,0.

## Después de la migración: arreglos (2026-11-01)

**¿Quedan edificios con el aspecto viejo?** No. Las fotos `Temp/Migracion/Servicios_entero*.png` (las primeras, de las 18:05) se
hicieron cuando solo Servicios era un árbol: los edificios de marco abierto con columnas y pisos apilados que salen en ellas son
las torres de entonces —Entrenamiento en (10, 0), 6 pisos con su techo de frutales (a la derecha), y Producción en (18, −95), 3
pisos, al fondo a la izquierda— que se migraron después (19:51 y 21:00). Con un buscador de `Estructura_Elevada` fuera de los
árboles y rayos desde las mismas cámaras, en la escena de ahora solo queda la **Central Nuclear** (1 piso a 12 m, su sitio de
exclusión: no se migra). Fotos nuevas desde las mismas cámaras: `Temp/PostMig/Servicios_entero.png` y `Servicios_lado.png`.

**El tronco, agua transparente.** De lejos los troncos salían como rejillas blancas opacas: el cristal del tanque (alfa 0,18, liso
0,92 y con el brillo sin multiplicar por el alfa) en caras de 20 m reflejaba el cielo y, con el sol, se quemaba a blanco; el agua
(alfa 0,2) era casi incolora; y los dos echaban una sombra maciza. Probado quitando por turnos cristal, agua y exoesqueleto
(cualquiera de los dos solo ya salía blanco). Ahora (`TrunkLook`): cristal casi invisible (alfa 0,06, liso 0,6, el brillo se
desvanece con el alfa), agua de un azul más hondo (alfa 0,45) y sin sombra; los nervios y zunchos no cambian (son la estructura,
`RootCheck`). De lejos se ve a través del tronco (a 170 m la niebla, ExponentialSquared 0,008, se come el azul); de cerca, azul, y
se ve nadar a la gente. Fotos con el posprocesado del juego en `Temp/PostMig/`: `antes_todo_lejos/cerca.png`,
`antes_sin_{cristal,agua,exo}_*.png`, `despues_tronco_lejos/cerca.png` y `despues_Produccion_nado_techo_a_piso5_01.png` (un nadador).

**Los fallos de la batería con los viajes más largos** (de 21):
- **La mesa del comedor** (la causa de casi toda la jornada): PathMover calculaba el camino desde el punto del NavMesh más cercano
  al pivote (1 m sobre los pies); junto a una mesa del anillo del comedor era la isla de NavMesh sobre el tablero (los muebles sin
  collider también cuentan al hornear). Al levantarse, el camino salía de la mesa y era parcial: 4 personas 30 min quietas en la
  Mesa_6 («pedir en la barra», «ir al baño»), sin llegar a sus puestos ni a entrenar; Lizmibet no llegaba a la clase de yoga, y dos
  máquinas del gimnasio no se alcanzaban. Ahora sale del NavMesh a la altura de los pies (`FeetOnMesh`).
- **Kushal y la misión de las 12 en otro árbol**: sale antes contando el viaje (`ArbolCampos.TripSeconds`) y la comida es un
  puesto más (`PlayerWorkday.HeadingEarly`; ver el commit).
- **Viajes que ya no van adonde iban** (`TreeTrip.StillFor`), **alfombras a ninguna parte** desde un árbol (`CarpetPad.ForceHop`),
  **el jugador en la puerta del techo** (su cápsula), **la clase de yoga espera a quien viene de lejos** (`YogaClass`).
- **El comedero de los depredadores** (retirado el 2026-11-05) estaba a 1 m de la cara Este del tronco de Producción: ahora a 9 m. **Rodeos que se
  alternan** al pie de un tronco (PathMover solo cambia de camino si el nuevo es claramente más corto) y **el guardia** deja la
  ración cerca si el animal se mueve.
- **Frutales**: un jardinero por edificio (docs/jardines-frutales.md): 5 rondas, 9,0 h.

## Historia

- **v1 (2026-10-29/30)**: raíz redonda de Ø 22 m y 101 m, 8 pisos octogonales de ~51 m inclinados 3° y girados 45° (acordeón),
  6 ramas redondas a 35° con anillos de nidos a ~91 m; `TreeOfFieldsTest` 28/28. Se cambió por la v2 (arriba).

## Máscaras, puertas de 8 personas y sin suelo (Berón 2026-11-03)

Detalle en [mascaras-de-oxigeno.md](mascaras-de-oxigeno.md). **Estas medidas mandan sobre las de arriba**:
- **Puertas cuadradas de 7 × 7 m** (antes 3 × 3): 8 personas a la vez (8 × 0,70 m de cápsula + 7 × 0,05 m = 5,95 m) y el submarino de obra (5 m de ancho y alto + 1 m a cada lado). Todas: tronco,
  columnas de las puntas y techo (su columna, 9,2 × 9,2 m de agua). Las **ramas** siguen en 10 × 10 m (cuadradas y más anchas que
  las puertas: caben 8 nadadores en fila con margen).
- **Máscaras**: un perchero fuera de cada puerta y una estación dentro, en el agua, junto a ella (una por nivel). Con la máscara
  puesta el aliento no se gasta; se coge al entrar y se deja al salir; quien acaba en el agua sin máscara nada a la estación más cercana.
- **Sin suelo**: el agua del tronco baja hasta −1,6 m y el hielo de debajo tiene un hueco (14 × 14 m en el Árbol de los Campos, que
  gira; 19,6 × 19,6 m en las torres): se baja nadando al mar y entran peces, kril y medusas (docs/grupos-de-animales.md). En el hueco
  cuelgan 4 cortinas de fitoplancton que cuida el jardinero del edificio.

## Medidas para el submarino de obra y el submarino hospital (Berón 2026-11-03, tarde) — MANDAN

Detalle y cuentas en [submarino-de-obra.md](submarino-de-obra.md). Vehículo de diseño: **14 × 5 × 5 m** con su carga (excavadora de
20 t, contenedor de 20 pies o la camilla de un elefante), 1 m de holgura a cada lado (`TreeSpec.VehicleLength/Beam/Height/Clearance`).
- **Puertas: 7 × 7 m** (`TreeSpec.DoorWidth/DoorHeight`, `HelixTower.doorWidth/doorHeight`), todas cuadradas.
- **Ramas: 10 × 10 m** (sin cambio: el vehículo gira su codo con 8,5 m). **Tronco: 20 × 20 m** de agua (sin cambio: el submarino
  pasa junto a un banco de peces). **Columna del techo**: 9,2 × 9,2 m de agua (`TreeSpec.HatchHalf` 4,6).
- **Entre piso y piso**: 22 m (sin cambio). **Entre piso y anillo**: los anillos de los pisos de un árbol van a **8 m** sobre el piso
  (`TreeSpec.RingOverFloor`; antes 4), por encima de la puerta del piso.
- El submarino llega a todos los **pisos** por el tronco; no entra en las columnas de las puntas. Garaje: el piso libre de Producción.
