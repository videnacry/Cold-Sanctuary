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

### Berkeley (Jenner), sobre el Laboratorio de Biología
**Berkeley, valle del Severn, 14 de mayo de 1796**: la vacuna de James Phipps con la viruela vacuna de la mano de Sarah Nelmes, que ordeñaba a
«Blossom». The Chantry (la casa de Jenner, junto a Santa María) es el escenario.
- **El piso:** las mesas del laboratorio son la loma del castillo y la de Canonbury; los lavabos, el Little Avon y sus acequias de prado; las sillas, los
  cerros de Stinchcombe al este (no nieva en mayo); las máquinas y armarios, roca.
- **La flora (100):** los prados de las vacas lecheras al suroeste (cola de zorra, botón de oro, boñigas) y otro junto al camino de la granja; el
  majuelo **en flor** (el «may») en los setos del camino de la granja y de Marybrook Street; prímulas en las orillas del Little Avon y helecho en su
  arboleda; arenisca en el lecho y musgo en las piedras del castillo.
- **La fauna (18):** el rebaño de Blossom —en el microcosmos, **pulgones**— con las hormigas amarillas que lo ordeñan, afodios en la boñiga,
  abejorros en las prímulas, mariquitas en el seto, arañas lobo en la orilla y cochinillas en el castillo.
- **Fuentes:** Jenner, *An Inquiry into the Causes and Effects of the Variolae Vaccinae* (1798); Dr Jenner's House (Jenner Museum: The Chantry
  junto a Santa María); *Victoria County History, Gloucestershire*, vol. 5 (Berkeley); el plano del pueblo ya citado. **Inferido:** la granja de
  Sarah Nelmes no tiene sitio documentado: el rebaño va en los prados del plano.

### Lo que no se ha hecho en esta tanda, y por qué
- **Ambrosio (nivel 1) y Sakshi (el río): escenario imaginado (Berón, 2026-10-27), sin mapa real.** Berón eligió para ellos la «taiga al amanecer»: un
  lugar inventado, no uno que recrear. En `MicroStoryMaps` tienen el estado `Imaginado` (no es una tarea pendiente) y su bioma se siembra; el registro
  dice «[Mapa] … escenario imaginado (Berón, 2026-10-27), sin mapa real».
- El resto de los científicos y famosos (Escútari, Dayton, Alejandría, París 1898, Berna, Pasadena, Colorado Springs, Chernóbil, Palenque,
  Velliangiri, Hawthorne, Albuquerque, Omaha, Prairie View, Uagadugú…) siguen en esbozo, con la siembra al azar de respaldo.

### Berna (Einstein), sobre la Planta Atómica
**El casco viejo en la península del Aar, mayo de 1905**, el año milagroso. Una ciudad: solo lo plantado y los animales del pueblo.
- **El piso** (bajo el mar, como el de Newton): la piscina del reactor es el meandro del Aar; bancos y consolas, el Gurten y la meseta del
  Kirchenfeld; los lavabos, las fuentes de la ciudad vieja; las máquinas, roca.
- **La flora (81):** la Münsterplattform con sus castaños de Indias en flor, césped y boj; la Kleine Schanze al suroeste (castaños, césped, boj);
  los tilos de la terraza del Palacio Federal (1902) y de la Bärenplatz; los geranios de las jardineras de la Kramgasse y la Gerechtigkeitsgasse;
  el adoquín de arenisca de la Münstergasse.
- **Los animales del pueblo (13):** caballos de los carros de reparto (pulgones de tiro) en la Kramgasse, en el mercado de la Bärenplatz y ante la
  estación; gatos de los soportales (cochinillas hormigueras).
- **Fuentes:** el plano del pueblo (Stadtplan Bern 1905); [Einstein-Haus Bern](https://www.einstein-bern.ch/) (Kramgasse 49); Stadtgrün Bern
  (Münsterplattform, Kleine Schanze); el Palacio Federal (1902). **Inferido:** el número de árboles de cada plaza.
- **Menú:** `Build Microcosmos de historias: Einstein (Berna)`.

### París 1898 (Marie y Pierre Curie), sobre el Laboratorio de Química
**El Barrio Latino alrededor de la rue Lhomond, diciembre de 1898**: aíslan el radio en el cobertizo de la Escuela de Física y Química (el escenario).
- **El piso:** las poyatas son la Montagne Sainte-Geneviève; las pilas, el Sena al norte; los taburetes, las cúpulas y tejados del barrio (el Panteón,
  la Sorbona); campanas y máquinas, roca.
- **La flora (104):** los plátanos **sin hoja** del boulevard Saint-Michel y de Port-Royal; el Jardín del Luxemburgo (castaños sin hoja, césped de
  invierno, boj) al oeste y el Jardin des Plantes al este (plátanos, césped, hiedra de sus verjas); la hiedra de las verjas del Panteón; el adoquín
  de la rue Mouffetard.
- **Los animales del pueblo (13):** caballos de ómnibus en el Saint-Michel y de fiacre en la plaza del Panteón; gatos de los patios de la
  Mouffetard; ratas del alcantarillado junto al mercado y en los muelles del Sena (cucarachas en el microcosmos).
- **Fuentes:** el plano del pueblo (Plan de Paris, Alphand 1889); Alphand, *Les Promenades de Paris* (1867–1873); Quinn, *Marie Curie: A Life* (1995):
  el cobertizo de la rue Lhomond y la casa de la rue de la Glacière, 24; la Compagnie générale des omnibus (~17 000 caballos hacia 1900).
- **Menú:** `Build Microcosmos de historias: Curie (París 1898)`.

### Colorado Springs (Tesla), sobre la Construcción
**La pradera de Knob Hill y la ciudad de Palmer, julio de 1899.** La estación experimental es el escenario.
- **El piso:** las alfombras apiladas son Knob Hill; las mesas de obra, las mesas (*buttes*) de la pradera; las sillas, las cumbres del Front Range con
  nieve en el Pikes Peak; las máquinas, roca.
- **La flora (109):** la pradera de hierba corta al este y al norte de la estación (grama azul, hierba del búfalo, yucas en flor, nopales,
  margaritas de la pradera); los álamos de las llanuras del Monument Creek, al oeste de la ciudad; la arenisca roja del Garden of the Gods, al noroeste.
- **La fauna (19):** saltamontes, hormigas segadoras con su montículo de grava, mariquitas, abejorros en las margaritas, arañas lobo y peloteros
  en la pradera; caballos de los coches de Pikes Peak Avenue (pulgones de tiro).
- **El bioma** gana el álamo de las llanuras (*Populus deltoides*) de la ribera.
- **Fuentes:** Tesla, *Colorado Springs Notes 1899–1900* (Nolit, 1978); el plano del pueblo (Sanborn 1897); la estepa de hierba corta del este de
  Colorado (USDA-ARS, Central Plains Experimental Range); el bosque de ribera de los arroyos del Front Range. **Inferido:** el sitio del Hotel Alta Vista
  (donde se alojó Tesla) en el plano.
- **Menú:** `Build Microcosmos de historias: Tesla (Colorado Springs)`.

### Pasadena (Patterson), sobre el Laboratorio de Química
**El campus de Caltech y sus calles, 1953.** El laboratorio limpio es el escenario. Una ciudad de coches: solo lo plantado, y perros y gatos.
- **El piso:** las poyatas son los montes de San Gabriel; las pilas, el Arroyo Seco; campanas y máquinas, roca.
- **La flora (106):** los olivos del **Olive Walk** y las encinas de la costa del campus, con su césped; las palmeras de California Boulevard; el encinar
  del Arroyo Seco; las calles de casas al sur y al norte (césped, rosales, setos de pitósporo); al norte, en Altadena, los cedros del Himalaya de la
  **Christmas Tree Lane** (1885).
- **Los animales del pueblo (8):** perros y gatos de las casas (cochinillas hormigueras).
- **El bioma** gana el olivo del Olive Walk.
- **Fuentes:** Caltech Archives (el Olive Walk y el plano del campus); City of Pasadena, «Street Trees»; Christmas Tree Lane (Registro Nacional de
  Lugares Históricos); el encinar del Arroyo Seco (Arroyo Seco Foundation); el plano del pueblo. **Inferido:** las palmeras en California Boulevard.
- **Menú:** `Build Microcosmos de historias: Patterson (Pasadena)`.
