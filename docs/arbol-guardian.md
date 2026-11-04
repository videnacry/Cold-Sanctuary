# El árbol guardián, el más grande de la historia (investigación → plan → construcción, Berón 2026-11-04)

Pedido de Berón: «He escuchado que existen árboles super anchos, de modo que se pueden crear pasadizos en sus troncos. El árbol
guardián debe ser el árbol más grande de la historia; lo puedes reconstruir con la apariencia del árbol más alto y del más ancho que
haya existido: sería un injerto, y luego le podrías dar matices de los árboles de hielo, como el color y los panales.»

Código: `Assets/Scripts/World/GuardianTree.cs` (el árbol), `GuardTower.cs` (la guardia en él), `SampleSceneBuilder.BuildGuardTower`
(su altura), `GuardianTreeTest` (grupo 91).

## 1. Investigación

### El más alto
- **Vivo**: *Hyperion*, secuoya roja (*Sequoia sempervirens*), Redwood National Park: **116,07 m** (Guinness, 2019; 115,55 m al medirlo
  Sillett con cinta en 2006). Fuste recto con corteza fibrosa y acanalada, copa **estrecha y cónica** en lo alto (~4,8 m de diámetro a
  la altura del pecho).
- **De la historia**: el **árbol de Ferguson**, un fresno de montaña (*Eucalyptus regnans*) caído en 1872 en el río Watts (Victoria,
  Australia), medido en el suelo con cinta por un inspector forestal: **132,6 m** (se cree que le faltaba la punta). Es el récord que
  se toma (otros, como el abeto de Douglas de Nooksack, de 141 m, no están comprobados).

### El más ancho
- **De la historia**: el **castaño de los cien caballos** (*Castanea sativa*, en las faldas del Etna, Sicilia): **57,9 m de perímetro**
  medido en 1780 (varios troncos de una misma cepa) → **18,4 m de diámetro**.
- **Vivo**: el **Árbol del Tule** (ciprés de Moctezuma, *Taxodium mucronatum*, Oaxaca): **42 m de perímetro**, 11,6 m de diámetro con
  sus contrafuertes (9,4 m de media). Tronco **acanalado** y con **contrafuertes**, de ~35 m de alto.

### Árboles con pasadizos
- Las secuoyas con túnel: el **Wawona Tunnel Tree** (secuoya gigante, Yosemite; túnel abierto en 1881 en una herida de fuego, cayó en
  1969 con una nevada) y el **Chandelier Tree** (secuoya roja, Leggett, California; túnel de ~1,8–2 m de ancho y 2–3 m de alto, abierto en 1937).
- Los **baobabs huecos**: uno con un bar dentro en Sudáfrica (Sunland) y el «baobab prisión» de Derby (Australia).

### Fuentes
- [Guinness — Tallest tree living](https://www.guinnessworldrecords.com/world-records/tallest-tree-living) · [Tallest tree ever measured](https://guinnessworldrecords.com/world-records/66377-tallest-tree-ever-measured)
- [Wikipedia — List of superlative trees](https://www.wikipedia.org/wiki/Tallest_tree)
- [Victoria's forests — tall trees and taller tales (el árbol de Ferguson)](https://victoriasforestsbushfireheritage.com/2023/12/10/tall-trees-and-taller-tales/)
- [WorldAtlas — What and where is the Chandelier Tree?](https://www.worldatlas.com/articles/what-and-where-is-the-chandelier-tree.html) · [Drive-Thru Tree Park — FAQ](https://drivethrutree.com/faq.html)

## 2. Plan (lo elegido)

- **El injerto**: la **base del más ancho** (el castaño de los cien caballos: 18,4 m de diámetro), **acanalada y con contrafuertes** como
  el Tule, que en 26 m se recoge hasta el **fuste de la secuoya** (5,2 m que se estrechan a 1,8 m) con su **copa estrecha y cónica** en el
  tercio de arriba.
- **Alto**: lo mayor del récord de la historia (**132,6 m**) y del **edificio-árbol más alto** con la columna de la puerta de su techo
  (la regla de siempre: el guardián es la excepción a «el primer piso por encima del árbol más alto» y crece tan alto como el edificio más
  alto). **Hoy manda el edificio** (Ciencias: ~162 m con su columna), así que el guardián mide **~162 m**: el árbol más alto de la historia
  (132,6 m) queda por debajo del edificio más alto del santuario. Si el santuario fuera más bajo, mediría 132,6 m.
- **Pasadizos**: dos túneles en cruz (norte–sur y este–oeste) de **7 × 7 m** —cuadrados, como las puertas de los edificios-árbol: caben 8
  personas a la vez y una alfombra voladora (las de verdad, de ~2 m, solo dejan pasar un coche)—, con una **sala** en el cruce, el suelo
  pisado y faroles. El NavMesh pasa por dentro. Las alfombras del pie están **frente al pasadizo sur**.
- **Hielo y panales**: la corteza, las ramas y la escarcha de los árboles de hielo; posaderos en las puntas de las ramas; **panales** de las
  abejas de las nieves (con su colmena y su enjambre, docs/abejas-y-panal.md) en las ramas bajas de la copa. No caza.
- **La guardia**: la plataforma (a 11 m) ahora es un **anillo** alrededor del tronco ancho, con la cabaña y su tablero al norte y la zona
  de alfombras al sur; las alfombras del pie, frente al pasadizo sur. La guardia trabaja igual (W9: sin cambios en su jornada: su punto
  de trabajo es el mismo objeto `Guardia_AUTO`).

## 3. Efectos en lo demás

- **Lanzadera de cohetes (T13)**: el guardián lleva `ElevatedElement`; la regla lo cuenta y la plataforma queda a máx(~166 m —su copa—,
  152 m de aves) + 30 m ≈ **196 m** (antes ~192). Al final del Blockout se valida otra vez.
- **Holguras**: sus ramas siguen esquivando los edificios-árbol (`HelixBlockedAt`, 4 m); la base ancha está en su isla, al noroeste del hub.
  Lo que estuviera a menos de ~12 m de su eje (contrafuertes) hay que mirarlo en la escena (pendiente de la verificación en vivo).
- **Pruebas**: la 57 mide el guardián frente al edificio más alto **sin la lanzadera** (que va por encima por su regla).

## 4. Prueba (grupo 91, `GuardianTreeTest`)
Alto ≥ máx(132,6 m, edificio más alto); base ≥ 18 m a ras de suelo con contrafuertes; los dos pasadizos libres de 7 × 7 m y el NavMesh de
boca a boca por dentro; corteza de hielo, ≥ 4 panales y ≥ 10 posaderos, sin cazar; plataforma en anillo, tablero y alfombras; y la regla de
la lanzadera.

## Rondas 5–6 del probador (2026-11-04)
- **La base a la altura de los pasadizos** ya no son duelas giradas (sus esquinas pisaban los túneles, se quitaban y la base quedaba de
  13,4 m): cada cuarto entre los dos túneles se llena con una rejilla de bloques de 1,5 m alineados con ellos hasta el borde del círculo
  (con acanaladuras en el borde). Por encima, las duelas de siempre.
- **Panales**: el constructor pone 8 (lo dice su registro), pero en el juego no estaban. `GuardianHives` guarda sus sitios y, si al empezar
  faltan, los vuelve a poner y lo avisa (`[Guardia] Faltaban los panales…`): queda por encontrar qué los quita.
