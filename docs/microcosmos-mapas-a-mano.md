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
planta y cada bicho del mapa en su sitio y los apunta. `AreaMicrocosmos.Build` los **deja caer desde el aire** (`MicroGround.Drop`:
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

## Los esbozos

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
