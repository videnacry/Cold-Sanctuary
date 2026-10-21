# Auditoría de biología de los insectos (2026-10-21)

Compara la configuración de los 11 artrópodos del microcosmos con su biología real: comportamiento, ciclo de vida,
dieta y reproducción. La forma va en [`insect-morphology.md`](insect-morphology.md).

Leyenda: ✅ arreglado en este PR · 🔴 incorrecto · 🟠 incompleto · 🟢 correcto.

## Qué tablas tiene cada especie

| Especie | SpeciesProfile | Physiognomy | StageProfile | Reproduction.Calendar | Family | Forager | DietPreference | PhrasePools | Nocturna |
|---|---|---|---|---|---|---|---|---|---|
| Ant | ✓ | ✓ | ✓ | · | ✓ | ✓ | ✓ | · | · |
| Antlion | ✓ | ✓ | ✓ | · | ✓ | ✓ | · | · | · |
| Aphid | ✓ | ✓ | ✓ | · | ✓ | ✓ | (presa) | · | · |
| Cricket | ✓ | ✓ | ✓ | · | ✓ | ✓ | ✓ | · | · |
| DungBeetle | ✓ | ✓ | ✓ | · | ✓ | ✓ | · | · | · |
| Grasshopper | ✓ | ✓ | ✓ | · | ✓ | ✓ | · | · | · |
| Ladybug | ✓ | ✓ | ✓ | · | ✓ | ✓ | ✓ | · | · |
| Mantis | ✓ | ✓ | ✓ | · | ✓ | ✓ | · | · | · |
| SnowBee | ✓ | ✓ | **·** | · | ✓ | ✓ | (presa) | · | · |
| Spider / SnowSpider | ✓ | ✓ | ✓ | · | ✓ | ✓ | ✓ | · | · |
| Termite | ✓ | ✓ | ✓ | · | ✓ | ✓ | · | · | · |

`·` significa que la especie no está en la tabla y usa el valor por defecto, que es de mamífero.

## Hallazgos

### Dieta
- ✅ **La hormiga cazaba pulgones.** `DietPreference` daba a la hormiga un peso de 1,5 por el pulgón. `Forager.SelectPrey`
  multiplica por ese peso, así que la hormiga lo elegía como presa favorita. En la realidad la hormiga **cuida y ordeña**
  al pulgón (mirmecofilia: melaza, `HoneydewProducer`). Ahora el peso es 0,05: solo lo come si no queda otra cosa.
- ✅ **La mariquita cazaba grillos** (peso 0,6). Come pulgones, cochinillas y ácaros; un grillo le queda grande. Ahora 0,05.
- 🔴 **El escarabajo pelotero come "pasto".** Se alimenta de **excremento**: lo come y con él hace la bola donde pone el
  huevo. `OrganicMaterial` no tiene `Dung`, aunque `DefecationNeed` ya suelta excremento (`fecesPrefab`). Propuesta: añadir
  `OrganicMaterial.Dung`, hacer que el excremento sea un `FoodItem` de ese material y añadir `eatsDung` en `Forager`.
  Cierra un ciclo real: animal → excremento → pelotero → suelo.
- 🟢 La termita corta hierba (como las termitas cosechadoras de la sabana), la mantis y la araña cazan, el pulgón chupa savia.

### Ciclo de vida
- 🔴 **No hay metamorfosis.** El modelo tiene 3 etapas (cría, juventud y adulto) con el mismo cuerpo, y eso no encaja con
  todos los grupos:
  - **Metamorfosis completa** (hormiga, abeja, mariquita, pelotero, hormiga león): huevo → **larva** (otro cuerpo, otra
    dieta, otro comportamiento) → **pupa** (inmóvil) → adulto. Esto falta. `Hive.cs` ya lo modela para las abejas
    (huevo 3 días, larva, pupa 12 días); puede servir de patrón.
  - **Metamorfosis incompleta** (grillo, saltamontes, mantis, termita, pulgón): ninfa → adulto, con mudas; las alas solo
    aparecen en el adulto. Las 3 etapas actuales encajan si la ninfa no tiene alas.
  - **Arañas**: crecen con mudas, sin metamorfosis. Encajan.
- 🔴 **Hormiga león.** La especie es la **larva** (embudo, mandíbulas), pero `StageProfile` le da 90 días de adulto con
  ese mismo cuerpo. En la realidad la larva vive **1–3 años** en el embudo y luego pupa. El adulto es un insecto alado,
  parecido a un caballito del diablo, que vive **unas semanas** y no caza en embudo. Propuesta: que la larva sea la etapa
  larga y el adulto alado un cuerpo distinto (o que la escena no lo muestre).
- 🟠 **SnowBee no tiene `StageProfile`** y usa el ciclo por defecto (de mamífero). Las obreras de abejorro viven
  semanas; la reina, un año. `Hive.cs` lleva la cría del panal; falta enlazarlo con `StageProfile`.
- 🟠 **Pulgón.** La partenogénesis está aproximada en `Family`, bien. Además, el pulgón real es **vivíparo** (pare ninfas
  vivas, sin huevo) y una ninfa madura en ~7–10 días. Los adultos viven 20–30 días, no 10.

### Reproducción
- 🔴 **Ningún insecto tiene `Reproduction.Calendar`.** Todos caen en el valor por defecto: 2 meses de "gestación" y un
  parto al año. En la realidad ponen **huevos** (ooteca en mantis y cucarachas, saco en arañas, bola de excremento en el
  pelotero) y tienen **varias generaciones al año**: pulgón, decenas; mariquita, 2–5; grillo y saltamontes, 1–2. Las
  reinas de hormiga y termita ponen **sin parar**. Cambiar esto toca la dinámica de población del ecosistema, así que hay
  que calibrarlo en una prueba en vivo y no ajustarlo a ciegas.
- 🟢 Mantis y araña solitarias (`Family(1)`), con canibalismo sexual anotado.

### Ritmo diario
- 🟠 **`SleepCycle.nocturnal` no se activa en ninguna especie.** Grillos (cantan de noche), muchas arañas y la hormiga
  león son **nocturnos**; abejas, mariquitas y saltamontes son **diurnos**. Propuesta: un flag por especie (en
  `SpeciesProfile` o en la clase de la especie) que `Animal.Init` copie a `SleepCycle`.

### Mente
- 🟠 **Ningún insecto tiene `PhrasePools`**, así que piensan con frases genéricas. Propuesta: unas pocas frases por especie
  ligadas a su biología (feromona, melaza, embudo, bola, canto nocturno).

### Tamaño
- 🟠 **La araña de las nieves usa el arquetipo `Spider`.** Hereda tamaño y masa de una araña normal, aunque el diseño
  dice que mide unos 30 cm (`docs/ice-sanctuary-ecology.md`). Le falta su propia entrada en `Physiognomy`, o que
  `SpeciesBody.species` sea `SnowSpider`.

## Hormigas humanas

En el microcosmos, las "hormigas" son **personas con forma de hormiga** (bípedas, con cara: `CubeInsect.DressHuman`), no
hormigas reales. Conviene darles un nombre propio, para no mezclarlas con la especie `Ant` del ecosistema.

Propuesta: **mirmidones**. En el mito griego, Zeus convirtió a las hormigas de la isla de Egina en personas para el rey
Éaco: los mirmidones, el pueblo que después siguió a Aquiles. Ovidio lo cuenta en las *Metamorfosis* (libro VII). Es el
nombre exacto para "hormigas humanas" y encaja con el hilo de mitos y metamorfosis del juego.
