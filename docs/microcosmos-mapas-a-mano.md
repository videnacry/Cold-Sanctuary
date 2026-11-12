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

### Dayton (los Wright), sobre la Mecánica
**Dayton y la duna de Kill Devil Hills, 17 de diciembre de 1903.** La historia junta los dos sitios: el taller de bicicletas de West Third Street (el
escenario) y la duna donde volaron. El mapa sigue el plano de Dayton y pone la duna **al sureste, en su dirección** (está a ~800 km).
- **El piso:** las alfombras apiladas son Big Kill Devil Hill; los bancos del taller, los bancos de arena de los Outer Banks; los lavabos, el Great Miami
  y el Wolf Creek; las máquinas, roca.
- **La flora (122):** en Dayton, los árboles **sin hoja** de diciembre (olmos en West Third Street, arces en Hawthorn Street, sicómoros en el Great Miami
  y el Wolf Creek); en la duna, barrón y avena de mar sobre la arena, brezo de playa, nopal, conchas, y del lado del estrecho el mirto de cera y la marisma.
- **La fauna (16):** caballos de carro en West Third y Williams Street, un gato en Hawthorn Street; en la duna, hormigas león aletargadas, arañas lobo
  de playa, mariquitas invernando en el mirto y cangrejos fantasma en su madriguera.
- **El bioma** gana los árboles de Dayton (olmo, arce plateado, sicómoro).
- **Fuentes:** NPS, Dayton Aviation Heritage (1127 West Third Street; 7 Hawthorn Street); el plano del pueblo (Sanborn, Dayton 1897); NPS, Wright Brothers
  National Memorial (Big Kill Devil Hill y su vegetación); el diario de Orville Wright del 17-12-1903. **Inferido:** las especies de los árboles de
  cada calle (las típicas de Ohio en 1900).
- **Menú:** `Build Microcosmos de historias: Wright (Dayton y Kill Devil Hills)`.

### Alejandría (Eratóstenes), sobre el Aula
**Entre el Gran Puerto y el lago Mareotis, solsticio de verano de ~240 a. C.**
- **El piso:** pupitres y mesas son la colina del Paneion y la isla de Faros; la pizarra y los paneles, las murallas; las máquinas, roca.
- **La flora (114):** los jardines del barrio real junto al Gran Puerto (palmeras datileras, sicómoros, grama); la orilla norte del Mareotis (papiro,
  tarays, halfa) y, al otro lado, sus **viñedos**; el **rastrojo** de trigo (junio: ya segado) junto a la puerta de Canopo; las boñigas de los asnos
  en la Vía Canópica; al borde sur, **hacia Siene** (en su dirección), el desierto con espina de camello, algodón de Sodoma y granito.
- **La fauna (24):** asnos de la Vía Canópica (pulgones de carga) con sus escarabajos sagrados; abejas egipcias y mantis en los jardines; termitas en
  el taray; langostas en la halfa; hormigas del desierto, hormigas león y escorpiones hacia Siene.
- **El bioma** gana la palmera datilera, el sicómoro egipcio y la vid de Mareotis.
- **Fuentes:** Estrabón, *Geografía* XVII.1.6–10 y 14; McKenzie, *The Architecture of Alexandria and Egypt* (2007); Cleómedes, *De motu circulari* I.10
  (el pozo de Siene); el plano del pueblo. **Inferido:** la extensión de los jardines del Brucheion.
- **Menú:** `Build Microcosmos de historias: Eratóstenes (Alejandría)`.

### Bagdad (al-Juarismi), sobre el Aula
**La Ciudad Redonda de al-Mansur y el barrio de al-Karj, hacia 820.** Una ciudad: solo lo plantado y sus animales.
- **El piso:** pupitres y mesas son los *tells* (montículos de ciudades antiguas) de la llanura; la pizarra y los paneles, la muralla de la Ciudad
  Redonda; las máquinas, roca.
- **La flora (100):** palmerales en las orillas de los canales **Sarat** e **Isa** y del **Tigris**; en los patios entre las murallas, granados,
  naranjos amargos, rosales de Damasco y setos de arrayán; el ladrillo cocido de la gran plaza.
- **La fauna (12):** asnos de los aguadores en los dos canales, camellos de caravana en el zoco de al-Karj y gatos de patio (pulgones de carga y
  cochinillas hormigueras en el microcosmos).
- **Edificios:** el palacio de la Puerta de Oro y la gran mezquita en el centro, las puertas de Jorasán, Basora, Kufa y Siria, el puente de barcas
  y el zoco de al-Karj.
- **Fuentes:** Lassner, *The Topography of Baghdad in the Early Middle Ages* (1970); Le Strange, *Baghdad during the Abbasid Caliphate* (1900);
  al-Yaʿqubi, *Kitab al-Buldan* (s. IX); Ruggles, *Islamic Gardens and Landscapes* (2008); el plano del pueblo. **Inferido:** el sitio de la Casa
  de la Sabiduría (el escenario, en el origen) y qué jardín tenía cada patio.
- **Menú:** `Build Microcosmos de historias: Al-Juarismi (Bagdad)`.

### Escútari (Florence Nightingale), sobre la Enfermería
**El cuartel de Selimiye sobre el Bósforo, invierno de 1854–55.**
- **El piso:** las camas son las alas del cuartel con sus filas de catres; los lavabos, el Bósforo y las fuentes del cuartel; las máquinas, roca.
- **La flora (92):** hiedra y musgo de pared en las alas y las torres; cebada ratonera y malvas en el camino de Üsküdar y la calle de Selimiye;
  caléndulas silvestres en la ribera del Bósforo; cipreses jóvenes, ciclámenes de invierno y lápidas en el **cementerio británico** (al sur, hacia
  Haydarpaşa) y en el de **Karacaahmet** (al este); malvas en la ladera de Çamlıca.
- **La fauna (22):** cucarachas orientales en las tres alas del cuartel, arañas de sótano en la torre de Nightingale, caracoles turcos sellados en
  la hiedra, cochinillas bajo las lápidas, mariquitas invernando en los cipreses y hormigas segadoras en la ribera en un día templado.
- **Edificios:** el cuartel, la torre de Nightingale, la mezquita de Selimiye, el embarcadero y los dos cementerios.
- **Fuentes:** Royal Collection Trust, «The Military Hospital, Scutari» (W. Simpson, 1855); Plan d'Üsküdar (1850); Commonwealth War Graves
  Commission, Haidar Pasha Cemetery; Nightingale, *Notes on Matters Affecting the Health … of the British Army* (1858); el plano del pueblo.
  **Inferido:** la distancia a los dos cementerios (acortada al borde del mapa, en su dirección) y qué torre era la de Nightingale (la del noroeste).
- **Menú:** `Build Microcosmos de historias: Nightingale (Escútari)`.

### Egina (Éaco y los mirmidones), sobre la Construcción
**El puerto, la colina de Kolona y el monte Oros, final de la primavera (la Grecia heroica).**
- **El piso:** las escaleras son las terrazas de piedra seca del Oros; las cajas de herramientas, las casas apiñadas de Kolona; las máquinas, roca.
- **La flora (80):** tomillo sobre la caliza de Kolona; lentisco en la costa del Sarónico; anémonas y tierra roja en los olivares; los campos de
  cebada ya agostados (hierba seca); gamones; en el Oros, jara al pie, pimpinela espinosa y salvia en la ladera y caliza en la cima.
- **La fauna (26):** **las hormigas segadoras del mito** en fila por el camino del Oros y en los campos de cebada; abejas en el tomillo de Kolona,
  cigarras y langostas en los olivares, una mantis en la salvia, escarabajos peloteros en el camino del puerto, hormigas león en la arena y
  escorpiones bajo las piedras de la cima.
- **Edificios:** el santuario de Apolo en Kolona, el puerto, el santuario de Zeus Helenio en el Oros, el palacio de Éaco y los talleres de alfareros.
- **Fuentes:** Pausanias II.29–30; Ovidio, *Metamorfosis* VII.614–660; Estrabón, *Geografía* VIII.6.16; Walter y Felten, *Alt-Ägina* III.1 (1981);
  el plano del pueblo. **Inferido:** el sitio del palacio de Éaco (mítico) y el de los talleres.
- **Menú:** ya tenía uno propio, `Build Egina (mirmidones)` (`MythSceneBuilder`).

### Crotona (Pitágoras), sobre el Aula
**La ciudad, el río Esaro y el camino del cabo Lacinio, verano de ~530 a. C.**
- **El piso:** pupitres y mesas son las manzanas de la retícula entre las *plateiai*; la pizarra y los paneles, la muralla; las máquinas, roca.
- **La flora (76):** olivares hacia la Sila y al norte del Esaro, y los olivos de la palestra de Milón; el **campo de habas** fuera de la ciudad;
  tomillo sobre la arenisca de la acrópolis; lentisco en el acantilado del cabo y arena del Jónico en la playa; esparto seco en el camino del
  Lacinio y en los cerros del suroeste.
- **La fauna (19):** cigarras en los olivos, hormigas segadoras en el esparto, abejas en el tomillo de la acrópolis, una mantis en el lentisco,
  langostas en el camino del cabo y escorpiones bajo la arenisca.
- **Edificios:** la escuela de Pitágoras, la palestra de Milón, el ágora, la acrópolis y el camino al santuario de Hera Lacinia (en el borde,
  en su dirección: está a 12 km).
- **Fuentes:** Estrabón VI.1.12; Tito Livio XXIV.3; Jámblico, *Vida pitagórica*; Diógenes Laercio VIII.1; Spadea (ed.), *Il tesoro di Hera*
  (1996); el plano del pueblo. **Inferido:** el sitio de la escuela (no se ha hallado) y el del campo de habas.
- **Menú:** `Build Microcosmos de historias: Pitágoras (Crotona)`.

### Menfis (Heródoto), sobre el Aula
**El recinto de Ptah, el Nilo y el borde de Saqqara, al empezar la crecida (~450 a. C.).**
- **El piso:** pupitres y mesas son los *koms* (montículos de la ciudad vieja); la pizarra y los paneles, la Muralla Blanca (Ineb-hedj); las
  máquinas, roca.
- **La flora (90):** papiro en la orilla del Nilo y caña en el canal del puerto; limo negro y loto azul en los campos que la crecida empieza a
  cubrir y en el lago sagrado de Ptah; palmerales; el rastrojo del trigo emmer; boñigas de asno en la avenida procesional; arena en Saqqara.
- **La fauna (19):** escarabajos sagrados en la avenida y en el borde del desierto, langostas en el rastrojo, grillos en los campos, y en la
  meseta la hormiga *Cataglyphis* del mediodía, la mantis del desierto y el escorpión amarillo.
- **Edificios:** el templo de Ptah, el coloso de Ramsés II, el patio del buey Apis, el puerto de Peru-nefer y la necrópolis de Saqqara.
- **Fuentes:** Heródoto II.14, II.99–101 y II.153; Jeffreys, *The Survey of Memphis* (EES); Bonneau, *La crue du Nil* (1964); el plano del
  pueblo. **Inferido:** el sitio del patio de Apis respecto al templo y la distancia a Saqqara (acortada, en su dirección).
- **Menú:** `Build Microcosmos de historias: Heródoto (Menfis)`.

### Sils Maria (Nietzsche), sobre el Aula
**El pueblo, Chastè, el valle de Fex y la roca de Surlej, agosto de 1881.**
- **El piso:** pupitres y mesas son los nevados (Corvatsch y Lagrev); la pizarra y los paneles, las paredes de roca del valle; las máquinas, roca.
- **La flora (76):** pastos de cervuno con gencianas en el Fex y junto al pueblo; rododendro en las laderas del Corvatsch y del Lagrev; pinos
  cembros en Chastè; edelweiss y gneis arriba; **la roca de Surlej** (gneis con líquenes de mapa) en la orilla de Silvaplana.
- **La fauna (16):** saltamontes alpinos sin alas en el Fex, abejorros alpinos en el rododendro, hormigueros de *Formica lugubris* bajo los cembros,
  arañas lobo al sol sobre la roca, mariquitas en los pastos y la mosca escorpión de la nieve, quieta en el musgo hasta el invierno.
- **Edificios:** la casa Durisch (la habitación de Nietzsche), la roca de Surlej, Chastè, la entrada del Fex y la pensión Edelweiss.
- **Fuentes:** Nietzsche, *Ecce Homo* («Así habló Zaratustra» §1); Siegfriedkarte 1:50 000, hoja 518 (1878); Stiftung Nietzsche-Haus; Info Flora;
  el plano del pueblo. **Inferido:** el sitio exacto de la roca (al suroeste del lago, hacia Surlej).
- **Menú:** `Build Microcosmos de historias: Nietzsche (Sils Maria)`.

### Chernóbil (Ananenko, Bespalov y Baranov), sobre la Central Nuclear
**La central, el estanque de refrigeración y Prípiat, principios de mayo de 1986.** Prípiat va a la mitad de su distancia real (como en el plano).
- **El piso:** los lavabos son el estanque de refrigeración; la bañera, **la piscina de burbujeo** bajo el reactor que el escuadrón vació; las
  máquinas, roca.
- **La flora (66):** el pinar del oeste (el futuro **Bosque Rojo**, aún verde en mayo) y el del sur con su musgo; abedules con hoja nueva en el
  parque de Prípiat y dientes de león en sus céspedes; hierba de arenal en el aluvión del río; la arena de las obras de los bloques 5 y 6.
- **La fauna (17):** hormigueros de hormiga roja en los dos pinares, hormigas león en la arena, mariquitas en los dientes de león, abejorros en los
  abedules y arañas lobo en la arena de las obras.
- **Edificios:** el bloque 4, el estanque, el pinar, el Palacio de Cultura Energetik, la piscina Lazurny, la noria y la estación de Yániv.
- **Fuentes:** Leatherbarrow, *Chernobyl 01:23:40* (2016); Plokhy, *Chernobyl: History of a Tragedy* (2018); IAEA, *Chernobyl's Legacy* (2006);
  el mapa de Prípiat de 1986; el plano del pueblo. **Inferido:** la extensión de los pinares en el mapa acortado.
- **Menú:** `Build Microcosmos de historias: Chernóbil (Ananenko)`.

### Palenque (Pakal), sobre el Huerto
**Lakamha': la Gran Plaza, el Otolum, la sierra y las milpas, 28 de agosto de 683 (lluvias).**
- **El piso:** las macetas y los bancales son la selva alta de la sierra de Don Juan; las cajas del huerto, las plataformas escalonadas del Palacio
  y de los templos; las máquinas, roca.
- **La flora (98):** en la sierra, ramones, ceibas, bromelias y hongos; helechos arborescentes en el Otolum; musgo sobre la caliza del escarpe y de
  la colina de las Cruces; **las milpas** del llano y del oeste (maíz con calabaza); cacao y aguacate en los solares; la ceiba de la plaza.
- **La fauna (24):** hormigas cortadoras en la selva y en la milpa, abejas meliponas en los solares, cigarras gigantes y arañas de seda dorada en
  la selva, luciérnagas en el Baño de la Reina, saltamontes en el llano; perros y pavos (cochinillas y mariquitas oscuras en el microcosmos).
- **Edificios:** el Templo de las Inscripciones, el Palacio, el Grupo de las Cruces, el Juego de Pelota, el Grupo Norte y el Baño de la Reina.
- **Fuentes:** Barnhart, *The Palenque Mapping Project* (2001); Stuart y Stuart, *Palenque: Eternal City of the Maya* (2008); Martin y Grube,
  *Chronicle of the Maya Kings and Queens* (2008); Ford y Nigh, *The Maya Forest Garden* (2015); el plano del pueblo. **Inferido:** el sitio de
  los solares con cacao (los huertos no se conservan).
- **Menú:** `Build Microcosmos de historias: Pakal (Palenque)`.

## Tanda 5: los famosos

### Velliangiri (Sadhguru), sobre la Sala de Yoga
**El Isha Yoga Center al pie de los montes Velliangiri, 24 de junio de 1999 (la consagración del Dhyanalinga).**
- **El piso:** las esterillas son los cocotales y arrozales del llano; los cojines, las siete colinas de Velliangiri; las máquinas, roca.
- **La flora (84):** en la falda y el monte, bambú, teca, hierba del monte sobre el granito, lantana en los caminos y la flor de la gloria; en la
  aldea, cocotales, platanares, nims y el patio; el jazmín y un nim junto al Dhyanalinga.
- **La fauna (20):** hormigas tejedoras en la teca y en los nims, abejas gigantes en las peñas, cigarras en el bambú, luciérnagas en el arroyo,
  una mantis en la lantana; las vacas y los perros de la aldea (pulgones y cochinillas en el microcosmos).
- **Edificios:** el Dhyanalinga, la sala de práctica, las cabañas, la aldea de Ikkarai Boluvampatti y los montes.
- **Fuentes:** Isha Foundation, «Dhyanalinga»; Tamil Nadu Forest Department, Boluvampatti Range; OpenStreetMap («Isha Yoga Center»); el plano del
  pueblo. **Inferido:** qué había construido en 1999 además del Dhyanalinga (la sala y las cabañas, aproximadas).
- **Menú:** ya tenía uno propio, `Build Microcosmos de famosos/Sadhguru (Velliangiri, 1999)`.

### Hawthorne (Elon Musk), sobre el Control de Misión
**La sede de SpaceX en Rocket Road y sus calles, 28 de septiembre de 2008 (el cuarto Falcon 1, el primero en órbita).** Una ciudad: solo lo
plantado y sus animales.
- **El piso:** las mesas de las consolas son las naves de la fábrica y del aeropuerto; las pantallas y los paneles, los muros de sonido de la
  autopista 105; las máquinas, roca.
- **La flora (66):** palmeras de abanico en Crenshaw Boulevard; jacarandas, césped y buganvillas en las calles de casas; el césped entre las pistas
  del aeropuerto; los bordillos de Rocket Road.
- **La fauna (9):** perros y gatos de las casas (cochinillas hormigueras en el microcosmos).
- **Edificios:** la nave de SpaceX, su control de misión, el aeropuerto municipal, la autopista 105 y las casas de estuco.
- **Fuentes:** SpaceX, «Falcon 1 Flight 4»; Vance, *Elon Musk* (2015); OpenStreetMap; City of Hawthorne; el plano del pueblo. **Inferido:** las
  especies de cada calle (las típicas del sur de California).
- **Menú:** ya tenía uno propio, `Build Microcosmos de famosos/Elon Musk (Hawthorne, 2008)`.

### Aldea de la montaña (Goku, homenaje), sobre las Artes Marciales
**Un amanecer de primavera. FICCIÓN:** el lugar no existe (ni en la obra ni en el mundo); el mapa sigue su **plano inventado** y lo que se coloca
es la naturaleza **real** del monte subtropical de China central, como en las aldeas de los montes Wudang y Song.
- **El piso:** los tatamis son los bancales de arroz y de té; los espejos, la poza de la cascada; las máquinas, roca.
- **La flora (80):** pinos de Masson en la colina y en el monte del maestro, azaleas y granito arriba; bambú y helechos junto al torrente, musgo
  en el paredón de la cascada; los bancales de té, el bambú de los patios y la hierba del camino de la cumbre.
- **La fauna (20):** cigarras en los pinos, luciérnagas y saltamontes en los bancales de arroz, una mantis china en el té, hormigas en la cumbre;
  gallinas y perros de la aldea (mariquitas pardas y cochinillas en el microcosmos).
- **Edificios:** la cabaña del maestro, el templo, la cascada, la cumbre del amanecer y el campo de entrenar.
- **Fuentes:** el plano inventado (CivicBuilder.Famosos); *Flora of China*; UNESCO, montes Wudang (1994). **Inferido:** todo el lugar (es ficción);
  solo las especies y su hábitat son reales.
- **Menú:** ya tenía uno propio, `Build Microcosmos de famosos/Goku (aldea de la montaña, homenaje)`.

### Albuquerque (Bill Gates), sobre la Informática
**La Central Avenue (Ruta 66) junto a MITS, primavera de 1975 (Micro-Soft nace el 4 de abril).** Una ciudad, con el alto desierto en el borde.
- **El piso:** los escritorios son las mesetas del alto desierto; los paneles, la sierra de Sandía; las máquinas, roca.
- **La flora (76):** al este, grama azul, chamisa y cholla sobre la grava; al oeste, el bosque de álamos del Río Grande; olmos siberianos en la
  Central y en Lomas; enebros y céspedes en las casas.
- **La fauna (9):** perros y gatos de las casas (cochinillas hormigueras en el microcosmos).
- **Edificios:** MITS, el motel de la Ruta 66, la Universidad de Nuevo México, la sierra de Sandía y el bosque del Río Grande.
- **Fuentes:** Allen, *Idea Man* (2011); *Popular Electronics*, enero de 1975; Microsoft (fundación); USDA PLANTS; OpenStreetMap; el plano del
  pueblo. **Inferido:** las direcciones de MITS y del motel (no verificadas, como en el plano).
- **Menú:** ya tenía uno propio, `Build Microcosmos de famosos/Bill Gates (Albuquerque, 1975)`.

### Omaha (Warren Buffett), sobre el Taller Textil
**La Nebraska Furniture Mart en la calle 72, Farnam Street y Memorial Park, 30 de agosto de 1983.** Plano acortado al este (como el plano del
pueblo). Una ciudad: solo lo plantado y sus animales.
- **El piso:** las alfombras son el gran almacén de alfombras de la señora B; las mesas de corte, las manzanas de la cuadrícula; las máquinas, roca.
- **La flora (70):** un resto de pradera de hierba alta (andropogon y equináceas) al oeste; fresnos verdes en Dodge Street; arces plateados y
  petunias en Farnam Street, frente a la casa de Buffett; robles en Memorial Park; céspedes; los bordillos del aparcamiento de la tienda.
- **La fauna (9):** perros y gatos de las casas (cochinillas hormigueras en el microcosmos).
- **Edificios:** la Nebraska Furniture Mart, la casa de Buffett, Kiewit Plaza, Memorial Park y la Universidad de Nebraska en Omaha.
- **Fuentes:** Berkshire Hathaway, carta de 1983; Lowenstein, *Buffett* (1995); Nebraska Furniture Mart (historia); OpenStreetMap; Nebraska
  Statewide Arboretum; el plano del pueblo. **Inferido:** las especies de cada calle.
- **Menú:** ya tenía uno propio, `Build Microcosmos de famosos/Warren Buffett (Omaha, 1983)`.

### Prairie View (MacKenzie Scott), sobre el Aula
**El campus de Prairie View A&M, la granja y el pueblo, 15 de diciembre de 2020 (el día del anuncio).**
- **El piso:** los pupitres son los edificios del campus; la pizarra y los paneles, el pinar de la loma; las máquinas, roca.
- **La flora (66):** la sabana de robles post con sus pastos al noreste; pinos taeda y yaupon en la loma del norte; los pastos de la granja
  experimental; en el campus, encinas de Virginia, árboles de Júpiter y el césped de la plaza.
- **La fauna (14):** hormigas rojas de fuego en los pastos y en la granja, arañas lobo en el pinar y en los pastos, y los perros del pueblo
  (cochinillas hormigueras en el microcosmos).
- **Edificios:** la administración, la biblioteca John B. Coleman, la granja experimental, el pueblo y la autopista 290.
- **Fuentes:** PVAMU, «Campus Map» y el anuncio del donativo (2020); Scott, «384 Ways to Help» (15-12-2020); Texas Parks and Wildlife (sabana de
  robles post); OpenStreetMap; el plano del pueblo. **Inferido:** la posición de cada edificio del campus (plano aproximado).
- **Menú:** ya tenía uno propio, `Build Microcosmos de famosos/MacKenzie Scott (Prairie View, 2020)`.
