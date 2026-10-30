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
| **Tronco** | Cuadrado, 0–92 m; agua de 20 × 20 m (de 0,05 a 91,5 m); pared de acrílico de 1 m (22 × 22 m) | El cristal del tanque de virtualización (`VirtualizationTank.GlassColor`) y su agua, más clara (alfa 0,2). El agua es un `WaterZone` (caja): se nada con `SwimBody`. |
| **Exoesqueleto** | 24 nervios CFST (4 esquinas y 5 por cara, uno cada ~3,7 m) y zunchos de canto (0,25 × 0,36 m) cada 6 m | Una sola malla; no cruza las puertas, las bocas de las ramas ni las losas. Es lo que carga: el acrílico no se cuenta como portante. |
| **Puertas** | Suelo, 4 pisos, 4 campos de punta y 4 anillos; 1,8 × 2,6 m | Hueco con bobinas de cobre y el **campo magnético** en la cara de dentro. Los cuerpos lo cruzan; el agua no: el hueco es una **campana de aire**. Cada una lleva su `TreeDoorExit`. |
| **Hélice** | 2 niveles: **64 m** (pisos a y c) y **75 m** (pisos b y d) | Lado a = +Z, b = +X, c = −Z, d = −X del árbol (gira). |
| **Pisos pegados** | 4 cuadrados de **32 × 32 m**, planos | Nacen en su cara del tronco y se corren 10 m hacia la derecha (mirando hacia fuera): un **molinete**. Así ningún piso tapa a otro en planta, ni los de un nivel a los del otro. En voladizo: 4 ménsulas y 4 tornapuntas CFST que bajan 4 m a la cara. Puerta frente a su centro, a 5 m del eje de la cara. |
| **Ramas** | 4 tubos **cuadrados** de agua (3 × 3 m, hueco 2,6 m), a la altura de su nivel | Salen rectas de la cara que no tiene piso (8 m) y giran a la derecha (20 m) hasta la columna de la punta. Celosía de acero de 5 m de canto (2 cordones abajo, 2 arriba y diagonales). |
| **Columna de la punta** | Agua de 3,2 × 3,2 m, del fondo de la rama al anillo (agua 0,6 m por encima de la puerta del anillo) | En la esquina de dentro del campo. **Puerta al campo** (+X, al nivel del campo) y **puerta al anillo** (−X, al nivel del anillo). |
| **Campos de punta** | 4 cuadrados de **16 × 16 m**, a **69 m** (nivel 1, lados b y d) y **80 m** (nivel 2, lados a y c) | En la esquina libre del molinete: 4 m más allá del borde del piso de ese lado y 4 m fuera del tronco. 3 tornapuntas a la columna. |
| **Anillos** | 4 `RingDeck` de 4 m de ancho, 4 m encima de cada campo (22,4 m de lado; 0,8 m sobre el campo y el resto volando) | `Nido_anillo_N`: frutales enanos en las esquinas, flores para las abejas de las nieves y nidos de aves. Sin alfombras: se llega nadando por la columna. |
| **Techo** | Cuadrado plano de 22 m a 92 m | Tan ancho como el tronco. Alfombras de edificio y del santuario, y la **TRAMPILLA** (3 × 3 m) sobre el agua, que queda 0,5 m más abajo: se salta al agua y se sale trepando. Gira con el árbol. |
| **Suelo** | Alfombras en el hielo, a 24 m del eje (al NE) | No giran. La **puerta del suelo** está en la cara a (gira con el tronco). |

**Alcance** (lo que barre al girar): 55,9 m (la esquina de un anillo). **Reparto, de abajo arriba:**
P1 Bosque comestible (piso a, 64 m) · P2 Bambusal (piso c, 64 m) · P3 Arrozal (punta b, 69 m) · P4 Milpa (punta d, 69 m) ·
P5 **Huerto** (`Garden_Area`, 30 × 9 m, con las hortalizas, su tanque de virtualización y su iniciación; piso b, 75 m) ·
P6 Bosque comestible (piso d, 75 m) · P7 Chacra andina (punta a, 80 m) · P8 Rotación (punta c, 80 m). Lo alto (el bosque,
~10 m de dosel; la guadua, 9 m) va en los pisos grandes, donde nada lo tapa; lo bajo, en las puntas. Cada cultivo conserva su
gremio y su semilla (`GardenPlot`): hortalizas 11, milpa 12, chacra 13, arrozal 14, rotación 15, bosque 16 y 18, bambusal 17.
Bancal de 22 × 18 m en cada piso (lejos de la puerta y de la rama que pasa 11 m por encima del borde pegado al tronco) y de
10 × 15 m en cada punta (a lo ancho del campo, filas a lo largo: caben todas las del gremio; con 9 × 9 la chacra perdía el cedrón y la rotación el garbanzo, el sésamo y el maní). El arrozal planta su alga en el mar más cercano (ahora en un grupo quieto, `Algas_…`, que no gira con el
árbol). Cada plataforma tiene sus plazas reservadas del microcosmos (`PlatformSlots`).

### Plano (desde arriba; cada carácter, ~2 × 4 m; T = tronco)

```
  nivel 1 (64 m): pisos 1 (a) y 2 (c); ramas de b y d a las puntas 3 y 4 (69 m)
  nivel 2 (75 m): pisos 5 (b) y 6 (d); ramas de a y c a las puntas 7 y 8 (80 m)

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

De lado (corte por la cara b): la rama de b sale a 64 m por debajo del piso 5 (75 m) y lleva a la punta 3 (69 m) en la esquina
libre; su anillo, a 73 m. Encima de cada piso no hay nada (salvo, 11 m más arriba y solo sobre su franja pegada al tronco, la
rama del otro nivel).

## Moverse

- **Personajes**: `ArbolCampos.Route` va dentro de `CarpetPad.Route` (la usan `PathMover`, la jornada, las iniciaciones y los
  hechizos), antes que las alfombras. En el mismo piso, andando; a otro nivel, primero a la puerta de su piso y después un
  **viaje a nado** (`TreeTrip`): puerta → agua → [rama: tramo recto, codo, tramo a la derecha → columna] → por el tronco a 6 m
  del eje → [rama y columna] → puerta → fuera, nadando con `SwimBody.SwimTo` a 2,5 m/s y, en los tramos verticales (el tronco
  y las columnas), llevados además por la **corriente** (1,4 m/s). Del campo a su anillo (o al revés), por la misma columna,
  sin bajar a la rama. Nada de teletransportes; si se atasca, se queda donde está y avisa.
- **Entradas**: desde fuera del árbol, las alfombras llevan **al techo** (los campos están a 64–84 m: es la entrada más
  cercana) y se baja nadando por la trampilla; para ir al suelo del árbol, al pie. Para salir, se nada al techo (sus
  alfombras), salvo si el destino está en el hielo junto al pie: por la puerta del suelo. Entre el suelo y el techo, en
  alfombra.
- **El jugador**: nada libremente dentro (el cuerpo de nado del mar). En cada puerta puede respirar. **Salir del agua**
  (el fallo de Berón: en la campana de la v1 el agua llegaba casi al dintel y se quedaba flotando contra él):
  `TreeDoorExit` — nadando delante de cualquier puerta y empujando hacia ella, sale de pie al umbral de fuera (como al trepar a
  un canto); en la trampilla, empujando hacia arriba o hacia un lado. Para entrar, se anda hacia la puerta y se cae al agua.
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
por eso 24 nervios). **Voladizo** de un piso (su peso a 16 m de la cara): 185 MN·m contra 4 tornapuntas CFST Ø 0,7 m →
**41 %**. **Rama** (punta, columna y tubo a ~30 m): 212 MN·m contra su celosía de 5 m de canto → **55 %**. Avisa por encima
del 80 % (con 30 niveles de hélice, avisa) y da error por encima del 100 %.

## Dónde

En el lóbulo **Sur:Milpa**, centro (−50, −84). Distancias con el simulador (`-tree`, puntos reales de las mallas, el árbol
girando: para cada altura, el círculo que barre):

| Vecino | Hueco 3D | En planta | Notas |
|---|---|---|---|
| Producción (18, −95) | **19,4 m** | −0,9 m | La torre llega a 45 m (con paneles y frutales del techo, ~48–49 m: ~16–17 m). Lo más bajo del árbol a esa distancia, a 63 m. |
| Entrenamiento | **26,9 m** | 17,9 m | Sube a 78 m. |
| Ciencias | 70,9 m | 62,3 m | |
| Servicios | 100,2 m | 97,5 m | |
| Central Nuclear | 78,3 m | 51,5 m | |

## Modelo genérico (para migrar las otras torres)

`HelixTower` (en `TreeSpec.cs`) es el edificio en hélice con sus medidas: medio lado del agua y pared del tronco, primer nivel y
paso entre niveles, nº de niveles, lado de los pisos y de las puntas, hueco y subida de las puntas, anillo, tubo, columna y
puertas. Da los marcos de cada lado (`SideRot`, `FromSide`), qué lado tiene piso en cada nivel (`HasFloor`), dónde va cada pieza
(`FloorCenterS`, `TipCenterS`, `ColumnS`, `DoorS`), su huella (`PlanRect`, `Overlap`) y el alcance (`Reach`).
`StructuralLimits.RootCheck` recibe un `HelixTower`. El constructor del árbol aún lleva cosas propias (cultivos, Huerto,
nidos): para migrar una torre, sacar de `BuildTree` lo que es del modelo (tronco, puertas, pisos, ramas, columnas, techo) a un
constructor que tome un `HelixTower` y una lista de lo que va en cada plataforma.

## Riesgos y pendientes

- Producción queda a ~16–19 m en 3D (por encima de ella, en planta, pasa la esquina de un anillo). Si Berón quiere 16 m también
  en planta, hay que acortar las puntas o mover Producción.
- La salida del agua por las puertas es una «ayuda» (como trepar a un canto): saca de pie al umbral sin animación de trepar.
- El agua del tronco (20 × 20 × 91 m) es mucha: si Berón lo prefiere, el tronco podría ser hueco entre niveles (no se ha hecho).

## Historia

- **v1 (2026-10-29/30)**: raíz redonda de Ø 22 m y 101 m, 8 pisos octogonales de ~51 m inclinados 3° y girados 45° (acordeón),
  6 ramas redondas a 35° con anillos de nidos a ~91 m; `TreeOfFieldsTest` 28/28. Se cambió por la v2 (arriba).
