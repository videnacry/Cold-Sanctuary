# Auditoría de biología de los insectos (2026-10-21)

Compara la configuración de los 11 artrópodos del microcosmos con su biología real: comportamiento, ciclo de vida,
dieta y reproducción. La forma va en [`insect-morphology.md`](insect-morphology.md).

Leyenda: ✅ arreglado · 🔴 incorrecto · 🟠 incompleto · 🟢 correcto. Todo lo que estaba en rojo o naranja se arregló en el
segundo PR (ver «Qué se cambió»); falta calibrarlo en una prueba en vivo.

## Qué tablas tiene cada especie

| Especie | SpeciesProfile | Physiognomy | StageProfile | Reproduction.Calendar | Family | Forager | DietPreference | PhrasePools | Nocturna |
|---|---|---|---|---|---|---|---|---|---|
| Ant | ✓ | ✓ | ✓ completa | ✓ reina | ✓ | ✓ | ✓ | ✓ | · |
| Antlion | ✓ | ✓ | ✓ completa | ✓ | ✓ | ✓ | · | ✓ | ✓ |
| Aphid | ✓ | ✓ | ✓ incompleta | ✓ partenogénesis | ✓ | ✓ | (presa) | ✓ | · |
| Cricket | ✓ | ✓ | ✓ incompleta | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| DungBeetle | ✓ | ✓ | ✓ completa | ✓ | ✓ | ✓ excremento | · | ✓ | · |
| Grasshopper | ✓ | ✓ | ✓ incompleta | ✓ | ✓ | ✓ | · | ✓ | · |
| Ladybug | ✓ | ✓ | ✓ completa | ✓ | ✓ | ✓ | ✓ | ✓ | · |
| Mantis | ✓ | ✓ | ✓ incompleta | ✓ | ✓ | ✓ | · | ✓ | · |
| SnowBee | ✓ | ✓ | ✓ completa | (panal) | ✓ | ✓ | (presa) | ✓ | · |
| Spider / SnowSpider | ✓ | ✓ (propia) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Termite | ✓ | ✓ | ✓ incompleta | ✓ reina | ✓ | ✓ | · | ✓ | · |

`·` significa que la especie no está en la tabla y usa el valor por defecto. En `DietPreference`, `·` es apetencia 1
(neutra). En «Nocturna», `·` es diurna.

## Hallazgos

### Dieta
- ✅ **La hormiga cazaba pulgones.** `DietPreference` daba a la hormiga un peso de 1,5 por el pulgón. `Forager.SelectPrey`
  multiplica por ese peso, así que la hormiga lo elegía como presa favorita. En la realidad la hormiga **cuida y ordeña**
  al pulgón (mirmecofilia: melaza, `HoneydewProducer`). Ahora el peso es 0,05: solo lo come si no queda otra cosa.
- ✅ **La mariquita cazaba grillos** (peso 0,6). Come pulgones, cochinillas y ácaros; un grillo le queda grande. Ahora 0,05.
- ✅ **El escarabajo pelotero comía "pasto".** Se alimenta de **excremento**: lo come y con él hace la bola donde pone el
  huevo. `OrganicMaterial` no tiene `Dung`, aunque `DefecationNeed` ya suelta excremento (`fecesPrefab`). Propuesta: añadir
  `OrganicMaterial.Dung`, hacer que el excremento sea un `FoodItem` de ese material y añadir `eatsDung` en `Forager`.
  Cierra un ciclo real: animal → excremento → pelotero → suelo.
- 🟢 La termita corta hierba (como las termitas cosechadoras de la sabana), la mantis y la araña cazan, el pulgón chupa savia.

### Ciclo de vida
- ✅ **No había metamorfosis.** El modelo tiene 3 etapas (cría, juventud y adulto) con el mismo cuerpo, y eso no encaja con
  todos los grupos:
  - **Metamorfosis completa** (hormiga, abeja, mariquita, pelotero, hormiga león): huevo → **larva** (otro cuerpo, otra
    dieta, otro comportamiento) → **pupa** (inmóvil) → adulto. Esto falta. `Hive.cs` ya lo modela para las abejas
    (huevo 3 días, larva, pupa 12 días); puede servir de patrón.
  - **Metamorfosis incompleta** (grillo, saltamontes, mantis, termita, pulgón): ninfa → adulto, con mudas; las alas solo
    aparecen en el adulto. Las 3 etapas actuales encajan si la ninfa no tiene alas.
  - **Arañas**: crecen con mudas, sin metamorfosis. Encajan.
- ✅ **Hormiga león.** La especie es la **larva** (embudo, mandíbulas), pero `StageProfile` le da 90 días de adulto con
  ese mismo cuerpo. En la realidad la larva vive **1–3 años** en el embudo y luego pupa. El adulto es un insecto alado,
  parecido a un caballito del diablo, que vive **unas semanas** y no caza en embudo. Propuesta: que la larva sea la etapa
  larga y el adulto alado un cuerpo distinto (o que la escena no lo muestre).
- ✅ **SnowBee no tenía `StageProfile`** y usa el ciclo por defecto (de mamífero). Las obreras de abejorro viven
  semanas; la reina, un año. `Hive.cs` lleva la cría del panal; falta enlazarlo con `StageProfile`.
- ✅ **Pulgón.** La partenogénesis está aproximada en `Family`, bien. Además, el pulgón real es **vivíparo** (pare ninfas
  vivas, sin huevo) y una ninfa madura en ~7–10 días. Los adultos viven 20–30 días, no 10.

### Reproducción
- ✅ **Ningún insecto tenía `Reproduction.Calendar`.** Todos caen en el valor por defecto: 2 meses de "gestación" y un
  parto al año. En la realidad ponen **huevos** (ooteca en mantis y cucarachas, saco en arañas, bola de excremento en el
  pelotero) y tienen **varias generaciones al año**: pulgón, decenas; mariquita, 2–5; grillo y saltamontes, 1–2. Las
  reinas de hormiga y termita ponen **sin parar**. Cambiar esto toca la dinámica de población del ecosistema, así que hay
  que calibrarlo en una prueba en vivo y no ajustarlo a ciegas.
- 🟢 Mantis y araña solitarias (`Family(1)`), con canibalismo sexual anotado.

### Ritmo diario
- ✅ **`SleepCycle.nocturnal` no se activaba en ninguna especie.** Grillos (cantan de noche), muchas arañas y la hormiga
  león son **nocturnos**; abejas, mariquitas y saltamontes son **diurnos**. Propuesta: un flag por especie (en
  `SpeciesProfile` o en la clase de la especie) que `Animal.Init` copie a `SleepCycle`.

### Mente
- ✅ **Ningún insecto tenía `PhrasePools`**, así que piensan con frases genéricas. Propuesta: unas pocas frases por especie
  ligadas a su biología (feromona, melaza, embudo, bola, canto nocturno).

### Tamaño
- ✅ **La araña de las nieves usaba el físico de `Spider`.** Hereda tamaño y masa de una araña normal, aunque el diseño
  dice que mide unos 30 cm (`docs/ice-sanctuary-ecology.md`). Le falta su propia entrada en `Physiognomy`, o que
  `SpeciesBody.species` sea `SnowSpider`.

## Qué se cambió (segundo PR)

- **Metamorfosis.** `StageProfile.development` (`Direct`/`Incomplete`/`Complete`) y `larvaForages`. Nuevo componente
  `Metamorphosis` (en `Assets/Animals/`, se añade solo a los holometábolos): en la **pupa** el ser no se mueve, no come
  y no huye; al salir, emerge el adulto. `Metamorphosis.PhaseOf(animal)` devuelve `Larva`/`Pupa`/`Adult`/`Nymph` para
  que el cuerpo de cubos dibuje cada fase. `Forager.Hunt/Graze` usan `Metamorphosis.CanForage`: la larva de mariquita y
  la de hormiga león cazan; la de hormiga, abeja y pelotero no (las alimentan o comen su provisión).
- **Duraciones.** Hormiga león: larva 365 días, pupa 26, adulto 25. Pulgón: ninfa 8 días, adulto 25. Abeja de las
  nieves: larva 9, pupa 12, adulta 150 (abeja de invierno: las de verano viven ≈1 mes y las de invierno varios meses,
  [doi 10.1242/jeb.247314](https://doi.org/10.1242/jeb.247314)). Mariquita, hormiga y pelotero con larva y pupa propias.
  La pupa ya tiene casi el tamaño adulto.
- **Reproducción.** `Reproduction.Calendar` con los 10 insectos (`[A]`, calibrar en vivo). El pulgón se reproduce sin
  macho (partenogénesis, [doi 10.3897/compcytogen.18.136095](https://doi.org/10.3897/compcytogen.18.136095)). En la
  hormiga y la termita **solo pone la reina** (la hembra adulta más antigua del nido) y no necesita volver a aparearse.
  La abeja nace del panal (`Hive`), no de `Reproduction`.
- **Excremento.** `OrganicMaterial.Dung`. `DefecationNeed` deja una boñiga (cubo pardo, `FoodItem` de excremento) de los
  herbívoros de ≥1 kg, como mucho una cada 2 horas de juego y con un tope de 150 en escena; se deshace en 3 días. El
  pelotero (`Forager.eatsDung`) la huele y va a por ella antes que a nada, y se la lleva a sus crías. El pelotero se
  alimenta sobre todo de excremento de herbívoros ([doi 10.1371/journal.pone.0304908](https://doi.org/10.1371/journal.pone.0304908));
  ese excremento es >86 % celulosa ([doi 10.1371/journal.pone.0079061](https://doi.org/10.1371/journal.pone.0079061)), así
  que en `Metabolism` da carbohidrato y minerales.
- **Nocturnos.** `SleepCycle.IsNocturnal`: grillo, hormiga león y araña.
- **Mente.** Frases de especie (`PhrasePools.Especie`) para los 11 insectos, sacadas de su biología.
- **Araña de las nieves.** `Animal.BodyArchetype` permite que una variante tenga su propio físico: la araña de las nieves
  usa `Physiognomy["SnowSpider"]` (≈30 cm, 175 g, como la tarántula Goliat) y todo lo demás de araña.

- **Reina de la abeja de las nieves.** `IceInsectLife` le alarga la etapa adulta a 3 años (las obreras, 150 días).

## Mirmidones (hormigas humanas)

En el microcosmos, las "hormigas" son **personas con forma de hormiga** (bípedas, con cara: `CubeInsect.DressHuman`), no
hormigas reales. Se llaman **mirmidones**, para no mezclarlas con la especie `Ant` del ecosistema.

El nombre viene del mito griego. Una peste dejó despoblada la isla de Egina. Su rey, Éaco, hijo de Zeus, vio una fila de
hormigas subir por un roble y pidió a su padre tantos súbditos como hormigas. Esa noche soñó que las hormigas caían del
árbol y se levantaban como personas; al despertar, su hijo Telamón le avisó de que había una multitud de hombres. Éaco
los llamó mirmidones, de *mýrmēx*, «hormiga»: un pueblo trabajador, ahorrador y tenaz. Más tarde siguieron a Peleo, y
después a su hijo Aquiles, a la guerra de Troya. Lo cuenta Ovidio en las *Metamorfosis* (libro VII).
