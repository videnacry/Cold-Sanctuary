# Los mapas a mano de cada historia (2026-11-04)

Berón, 2026-11-04: «el suelo de tierra se puede desactivar si es tan pesado, antes te dije que los microcosmos se deben crear
específicamente para cada microcosmos con lo cual no veo razón para que exista flora nuevamente bajo el suelo, quizás existen scripts
que pueblan de forma random la vegetación? un truco que conozco es crear todos los objetos en el aire para que caigan sobre la
superficie que tenga debajo, pero quizás para crear territorios más accurate/representativos/reales se puede crear a mano cada mapa
de cada historia del microcosmos, de modo que la flora y fauna se colocan a mano y de paso se organizan los muebles y demás
componentes del piso para recrear el mapa».

**Regla M9 del manual: toda historia tiene su mapa hecho a mano.** La siembra al azar es solo el respaldo de las que aún no lo tienen,
y se dice en el registro.

## El formato: `MicroStoryMaps` (`Assets/Scripts/Microcosmos/MicroStoryMaps.cs`)

Una tabla en C# (como los planos de los pueblos, `CivicBuilder.TownPlans`), un `Map` por historia (`display` de `MicroFloors.Table`).
Las posiciones van en **metros desde el origen de la «Historia»**, x al este y z al norte: el mismo marco que el plano del pueblo.

| Campo | Qué es |
|---|---|
| `status` | `Hecho` (el constructor lo usa) o `Esbozo` (aún no: siembra al azar, y lo registra). |
| `place`, `source` | El lugar y la fecha, y de dónde salen (planos, crónicas, botánica). |
| `floor` | Cómo se ordenan las piezas del piso para el mapa real: `Piece(match, acción, por qué, elemento, mover)`. `match` es la clave de la pieza (o su principio) o el id de su elemento del catálogo (`MicroFloorElements`); la acción: `Quitar` (se apaga; sigue contada, el piso no se rompe), `Apilar` (alfombras en un cúmulo), `Mover` (metros del piso real), `Como` (otro elemento: la silla como cerro). Manda sobre la tabla general `MicroFloorElements.Overrides`. |
| `flora` | `Plant(planta, x, z, cuántas, radio)`: la planta del bioma de la historia (`MicroBiome`, por el principio de su nombre) o un rodal en espiral dentro de su radio. |
| `fauna` | `Animal(especie, nombre, x, z, cuántos, largo, radio)`. |
| `buildings` | Los edificios y su sitio, para comprobarlos: los levanta el plano del pueblo (`CivicBuilder.TownPlans` / `Towns`). |
| `todo` | Lo que le falta (los esbozos dicen qué hay que investigar y colocar). |

## Cómo lo usa el constructor

`AreaMicrocosmos.Nature`: con un mapa `Hecho`, NO siembra al azar (ni las plazas de flora, ni las rejillas, ni el pueblo); pone cada
planta y cada bicho del mapa en su sitio y los apunta. (2026-11-05) En un **pueblo** no caen: cada planta va al suelo libre más cercano a su sitio del mapa, hasta ~70 m, fuera de calles y solares; si el mapa la pone sobre una meseta, se queda allí con «(sobre mueble)»; porte máximo, 20 m. Fuera de los pueblos, `AreaMicrocosmos.Build` los **deja caer desde el aire** (`MicroGround.Drop`:
desde justo bajo la tapa de los muros, sobre la superficie más alta de debajo: suelo, alfombra, losa, la meseta de un mueble, la tierra
si está encendida), y luego `MicroGround.Settle` apoya y revisa todo lo demás (ninguna planta bajo una superficie ni dentro de un
collider). Las mesetas de los muebles gigantes siguen poblándose por la regla 4 (`floraOnTops`/`faunaOnTops`), también con mapa.
El registro: `[Mapa] Angostura: mapa a mano (… plantas, … bichos, … órdenes sobre el piso)` o
`[Mapa] X: SIN mapa a mano (esbozo: …): siembra al azar como respaldo`.

## El ejemplo completo: Angostura (Bolívar), 15 de febrero de 1819

Sobre la Cocina (pendiente de su Sala de asambleas). Estación seca: sabana de paja con chaparros, morichales en los caños, araguaneyes en
flor y lajas de granito del escudo guayanés; el Orinoco al norte, el cerro El Zamuro al sur.
- **El piso**: las alfombras, apiladas: el cerro El Zamuro; los fogones (volcanes en el catálogo) son **tepuyes** (no hay volcanes en
  Guayana); las sillas, **cerros de granito** (no hay nieve); los fregaderos, el Orinoco y sus lagunas; armarios y neveras, las casas
  coloniales de dos plantas.
- **La flora** (≈ 190 plantas): 4 araguaneyes en las esquinas de la Plaza Mayor y 8 por el Paseo Orinoco; morichales de 9 y 7 palmas
  al oeste y al este junto a la barranca; paja de sabana (40 + 30 + 30) al sur y en el Zamuro, con chaparros (14 + 10); lajas de
  granito con bromelias en la barranca y playas de arena del Orinoco.
- **La fauna**: bachacos (6), comejenes en los chaparros del Zamuro (4), saltamontes (4) y mantis (2) en la paja, cocuyos por el Paseo
  (4) y arañas de seda dorada en el morichal (2).
- **Los edificios** (del plano): la Plaza (0, −70), la Casa del Congreso, la Catedral, el Puerto, el Mercado y el fortín del Zamuro.

## Las doce de la tanda 3 (2026-11-05)

Eloy Alfaro, Galvarino, Atahualpa, la Gran Ley de la Paz, Cleopatra, Langley, Beethoven, Descartes, Guevara, Sánchez, Lincoln y la Feria: los doce, **Hecho**
(`MicroStoryMaps.Historias*.cs`, [microcosmos-historias-3.md](microcosmos-historias-3.md)). Dos cosas nuevas: `Natural(...)` añade a las historias anteriores a la máquina las órdenes
que hacen roca de las máquinas, antenas y estaciones del piso; y como con un mapa hecho no se siembra nada, **los animales del pueblo (perros, burros, llamas) van también en el mapa**.

## Los esbozos
Hechos además del ejemplo: las **9 historias de la música** (`MicroStoryMaps.Musica.cs`, [microcosmos-musica.md](microcosmos-musica.md); `MicroStoryMaps.For` mira `All` y luego `Music`). Sus coordenadas caben a 1:1 en el barrio de su piso: si un plano se encogiera, el mapa a mano dejaría de coincidir con él.


Todas las demás historias de `MicroFloors.Table` tienen su esbozo con lo que les falta: colocar a mano su flora y fauna, ordenar los
muebles del piso para su mapa real y lo propio de cada una (el gueto de Varsovia, la Kramgasse de Berna, el Nilo en crecida…). Los
cambios de piso que ya estaban en `MicroFloorElements.Overrides` (Tregua, Sils Maria, Chernóbil) siguen ahí hasta que su mapa se haga.

## Dónde se siembra al azar (inventario, 2026-11-04)

| Quién | Qué | Ahora |
|---|---|---|
| `AreaMicrocosmos.Nature` | Flora en las plazas de flora del piso, en la rejilla del suelo libre (6 m / ×K), en la del pueblo (3 m), en las mesetas de los muebles; la fauna del bioma (plazas de fauna, suelo libre, mesetas, calles) | Respaldo sin mapa; con mapa, solo las mesetas. Todo pasa por `MicroGround.Settle` (lo del mapa, antes por `MicroGround.Drop`). |
| `AreaMicrocosmos.Food` (y `ForagingBand`, `KitchenStory`, `TribeStory`) | Restos de comida y hojas al azar en un anillo alrededor de la historia | Son de la historia (a ras de su suelo), no flora; quedan. |
| `CivicBuilder` (frutales de cada casa, parques) | Frutales y árboles como muebles (`MicroFurniture`) sobre la losa del edificio | En su edificio: no caen bajo nada. |
| `MicrocosmosSceneBuilder.BuildAmbientInsects` | Insectos al azar alrededor de Sakshi | Código muerto (nadie lo llama). |
| `StorySceneBuilder` (`Tree`, `Items`) | Árboles y objetos de cada historia en sitios fijos | Ya son a mano. |
| `SampleSceneBuilder` (`PolarUnderstory`, `PolarHabitats`, fauna del santuario) | La flora y fauna del santuario (mesocosmos) | Fuera de los microcosmos. |

La capa de tierra (`MicroGround.SoilEnabled`) está **apagada** por defecto desde el 2026-11-04 (colgaba el NavMesh de la Cocina); ver
[microcosmos-suelo-y-elementos.md](microcosmos-suelo-y-elementos.md).

## Tanda 4 (2026-11-12): las que el jugador ve primero

Petición 75. Mapas en `MicroStoryMaps.Tanda4.cs`; prueba: grupo 115 (`MapasAManoTest`). Nada se siembra sobre el escenario de la historia (a ≤ 30 m de su
origen están sus personas y sus sitios). Las distancias reales que no caben en el mapa (Kudan a 6 km, Lumbini a 25 km) van al borde, en su dirección.

### Kapilavastu (Siddhārtha y los cuatro encuentros), sobre el Hospital
**Tilaurakot, otoño tras el monzón, s. V a. C.** La ciudadela amurallada (~500 × 450 m) del plano, en la orilla este del Banganga.
- **El piso:** las alfombras apiladas son el montículo de la ciudadela; las sillas, las colinas Siwalik (Churia) al norte (no nieva); las camas, los
  diques de los arrozales; bañeras y piletas, los estanques revestidos de ladrillo; los lavabos, el Banganga. Las máquinas, roca (`Natural`).
- **La flora (≈ 220):** arroz granado en los arrozales del este (40) y del oeste (30); caña kans en flor en tres tramos de la orilla del Banganga (36),
  con arena y cantos; grama durva en las calles, tulsi en los patios y pipales junto al estanque de ladrillo y en las puertas este y oeste; mangos fuera
  de la puerta norte; kusha en la orilla del Sagar; boñigas en las puertas norte y sur; al sur, **Kudan**: la arboleda de banianos del
  Nigrodhārāma con su kusha; al este, hacia **Lumbini**, el bosque de sal con sus plántulas, termiteros y su seta (*Termitomyces*); una franja de
  sal al norte de la ciudadela.
- **La fauna (30):** saltamontes del arroz, pulgones y mariquitas en los arrozales, luciérnagas sobre ellos al anochecer, termitas en el bosque de
  sal, peloteros en la boñiga de la puerta sur, hormigas león en la arena del río, mantis en la kans y el ácaro rojo de las lluvias en Kudan.
- **El bioma** gana los árboles del lugar: árbol de sal, baniano, pipal y mango.
- **Fuentes:** Coningham et al., proyecto Tilaurakot-Kapilavastu (Univ. de Durham): ciudadela de ~500 × 400 m, cuadrícula, murallas y foso
  ([Durham](https://dur.ac.uk/departments/academic/archaeology/research/archaeology-research-projects/tilaurakot/)); Lumbini Development Trust
  (Kudan, ~3 km al sur de Taulihawa, [LDT](https://lumbinidevtrust.gov.np/download/1042)); [Wikipedia, «Tilaurakot»](https://en.wikipedia.org/wiki/Tilaurakot)
  (orilla este del Banganga). **Inferido:** Kudan a ~6 km de Tilaurakot (3 km a cada lado de Taulihawa); la flora del Terai (sal, kans, kusha) es la
  del bioma ya investigado, no una lista del sitio.

### Woolsthorpe (Newton), sobre la Planta Atómica
**Woolsthorpe-by-Colsterworth, verano de 1666**, el año de la peste en casa. La casa es el escenario; el mapa, lo de alrededor.
- **El piso** (la Planta Atómica, hoy bajo el mar: su microcosmos abre el Santuario Marino): la piscina del reactor es el remanso del Witham junto al
  molino; bancos y consolas, la loma de caliza y las lindes altas de las franjas; los lavabos, los arroyos que bajan al Witham; las máquinas, roca.
- **La flora (≈ 140):** el huerto de manzanos **delante de la casa** (8, con manzanas caídas); el campo abierto **en franjas** al oeste (cebada y
  trigo, 40) y el campo de cebada al sur (16); el prado del Witham (ballico, trébol, botón de oro); setos de majuelo en Water Lane y en el camino de
  Woolsthorpe; la loma de caliza con sus ovejas; el musgo de los muros de Colsterworth.
- **La fauna (21):** pulgones, tijeretas en las manzanas caídas y mariquitas en el huerto; abejorros en el trébol; saltamontes y hormigas en las
  franjas; arañas en el seto; afodios en el excremento de oveja.
- **El bioma** gana la cebada y el trigo de las franjas.
- **Fuentes:** National Trust, [«The story of our apple tree at Woolsthorpe Manor»](https://www.nationaltrust.org.uk/woolsthorpe-manor/features/the-story-of-our-apple-tree-at-woolsthorpe-manor)
  (el árbol, en el huerto, rebrotó de la raíz tras caer en 1820); [Wikipedia, «Woolsthorpe Manor»](https://en.wikipedia.org/wiki/Woolsthorpe_Manor)
  (el manzano delante de la casa); el plano del pueblo ya citado (National Trust; OS 1:2500 de 1887); Thirsk (ed.), *The Agrarian History of England
  and Wales*, vol. IV (el campo abierto en franjas). **Inferido:** el sitio exacto del huerto (las fuentes dicen «delante de la casa», no a cuántos metros).
