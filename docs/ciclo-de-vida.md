# Ciclo de vida: los procesos naturales de cada especie del santuario

> Berón (2026-11-01): «una escena por animal del santuario y así se comprobaría que realiza **todos sus procesos
> naturales**». Las escenas (`Assets/Scenes/CicloVida/CicloVida_*.unity`, menú «Tools/Cold Sanctuary/Build escenas de
> ciclo de vida») ponen una pareja adulta en una isla con su hábitat; el panel `CicloVidaPanel` marca con ✓ cada proceso
> en cuanto lo ve y la prueba `CicloVidaTest` (grupo 58) los espera uno por uno. Este documento dice qué hace cada especie,
> de dónde salen los números y cómo está hecho.

Al leer el código tras `deec0ee` faltaban: la sed, la lactancia (solo la tenían algunos artrópodos), la cría que sigue a su
madre en tierra, el huevo del pingüino (se «gestaba» dentro de la madre), la guarida de la osa, los zánganos y el vuelo
nupcial de la abeja, los datos propios de la araña de las nieves y, sobre todo, un **calendario coherente**. Ahora todo eso
existe y se comprueba (ya no hay SKIP).

## Calendario: un solo año para todo

- Un día de juego = **1 hora real** a ×1 (`Clock.realMinutesPerDay = 60`). Un año de vida = **28 días de juego**
  (`TimeController.DaysPerYear`; 4 estaciones de 7 días).
- Las tablas biológicas van en unidades **reales**: la gestación y la incubación en meses (`Reproduction.Calendar`,
  `LifeHistory`), las etapas de vida en días (`StageProfile`). Las dos se pasan al año del santuario con la misma cuenta:
  `SecondsForMonths(m) = m/12 · DaysPerYear · SecondsPerGameDay`, y un **día de vida** =
  `LifeStage.SecondsPerLifeDay = SecondsForMonths(12)/365` ≈ **4,6 min reales** a ×1.
- La gestación y el descanso entre crías (`Reproduction`) se cuentan en **meses transcurridos** (como el huevo,
  `Egg.progress`): si el año cambia a media gestación, lo que falta se recalibra. Antes se fijaba la hora del parto al
  concebir: la foca de su escena concebía al arrancar (año de 2 días) y no paría en la prueba (año de 0,05).
- **Antes** `LifeStage` contaba sus «días» en **minutos de juego** (~2,5 s): el oso adulto vivía 7 300 min (5 días de juego)
  pero gestaba 8 meses (19 días de juego): moría de viejo antes de parir. Y la colmena contaba un «día de abeja» como un
  día de juego entero (1 h): la cría de abejas iba 13 veces más despacio que todo lo demás.
- Ahora `LifeStage.Live` sigue lanzando sus eventos (merodeo, descanso, engorde…) al ritmo de siempre, pero los **días de
  vida** corren con el calendario, que se lee en cada vuelta (cambiar el año los recalibra al momento). `GrowScale` crece en
  proporción a los días pasados (antes crecía el triple de lo debido: 1/stageDays en cada tercio de «día»).
- `Hive.DaySeconds` = un día de vida del calendario (salvo `secondsPerDay > 0`, que sigue siendo un atajo de prueba).
- Escenas de ciclo de vida: el año, **comprimido a 2 días de juego** (botón para volver a 28). La prueba lo baja a 0,05
  días y pone ×8: a ese año una foca vieja moriría en minutos, así que la prueba alarga la vida adulta de los padres y fija
  la infancia de la cría en ~90 s de juego y la juventud en ~30 s.

**Efecto en SampleScene** (año de 28 días): los animales envejecen a ritmo real-comprimido (un oso adulto vive 20 años =
560 días de juego; antes, 5). Nadie muere de viejo en una sesión; las crías crecen en horas reales, no en minutos. La
colmena cría ~13 veces más rápido que antes (una obrera tarda 21 días de vida = ~1 h 37 min reales). Ninguna prueba de
SampleScene dependía de que los animales crecieran o murieran de viejos en minutos (se revisaron JornadaTest,
SanctuaryLifeTest, ChecklistFaunaTest, EraSpeciesTest y LifeCycleAuto); JornadaTest cuenta como «quietos a propósito» los
deseos nuevos de quedarse quieto (`den`, `incubate`, `drink`).

## Las tablas

| Qué | Dónde |
|---|---|
| Agua, crianza, cría que sigue, guarida, huevo (con fuentes) | `Assets/Animals/LifeHistory.cs` (nueva) |
| Gestación / de la cópula a la puesta, intervalo entre crías | `Reproduction.Calendar` |
| Días de cría (= lactancia), joven y adulto | `StageProfile` |
| Qué come | `Forager.ConfigureForSpecies` |
| Quién se vincula con la cría (base de «nurse») | `BroodRide.ParentalCare` (lee `LifeHistory.care`) |

`Animal.LifeArchetype` / `Animal.LifeSpecies`: la clave de las tablas de VIDA cuando una variante vive distinto que su
especie base. Solo la usa la araña de las nieves («SnowSpider»; el resto de tablas sigue con «Spider»).

## Componentes y deseos nuevos

- `Hydration` (se añade a las especies con `LifeHistory.water`): la sed sube con las horas de juego. Los que sacan el agua
  de la comida la reponen al comer; el pingüino bebe agua de mar al nadar o come nieve; el malamute va al bebedero
  (`WaterSource`) o come nieve. Deseo **«drink»**. Una sed muy larga solo incomoda (estrés): en el hielo siempre hay nieve.
- `MaternityDen` + `Den` (sitio): la hembra preñada entra en su guarida antes del parto, pare dentro, amamanta allí y sale
  con las crías. Sin guarida cerca, se hace la suya donde está. Deseo **«den»** (la osa ayuna: vive de su grasa).
- `Egg`: el huevo/saco de verdad (ver pingüino y araña). Deseo **«incubate»** (quien incuba no come).
- Deseo **«heel»**: la cría (vinculada) sigue a su madre o padre en tierra (los marinos ya lo hacían en `MarineLife`).
- `DesireCatalog.FeedCub` + evento `CubFed`: cada toma (amamantar o regurgitar). El recién nacido que lo alimentan sus
  padres nace con hambre (la primera toma llega enseguida); el que no (la arañita), vive del vitelo.
- `Reproduction`: guarda la pareja (`Mate`), pone huevos en los ovíparos (`Egg.Lay`) y hace eclosionar (`Hatch`); vincula
  madre↔cría (y padre↔cría en los biparentales) con `Imprint`.
- `Hive`/`IceInsectLife`: zánganos, vuelo nupcial, huevos sin fecundar (ver abeja).

## Por especie

### Oso polar (`Bear`)
- **Agua**: no bebe; la saca de la grasa de foca (agua metabólica) [A].
- **Guarida de maternidad**: la hembra preñada entra ~1,2 meses antes del parto, pare dentro, ayuna y sale con los oseznos
  a los ~3 meses. Entrada media 11–22 nov (Amstrup & Gardner 1994, *J Wildl Manage* 58:1); emergencia media 15–21 mar
  (Smith et al. 2007, *Arctic* 60:187) y 16 mar (*J Mammal* 105:gyae010, 2024); los oseznos pesan 10–12 kg al salir
  (Ramsay & Stirling 1988, *J Zool*, doi 10.1111/j.1469-7998.1988.tb03762.x).
- **Lactancia**: solo leche los 3 primeros meses; con la madre hasta ~2,5 años (Derocher, Andriashek & Arnould 1993,
  *Can J Zool*: composición de la leche y lactancia). `StageProfile`: cría 900 días (antes 180), joven 730.
- **La cría sigue a la madre** («heel», a ≤ 3 m).

### Foca común (`Seal`)
- **Agua**: de la presa y metabólica; los pinnípedos casi nunca beben (Ortiz 2001, *J Exp Biol* 204:1831).
- **Lactancia** ~24 días (Boness et al. 1994; Thompson et al. 1994; destete y ayuno posterior: Muelbert & Bowen 1993,
  *Can J Zool* 71:1405, doi 10.1139/z93-194). `StageProfile`: cría 24 días (antes 45).
- **La cría nada con la madre** (`MarineLife`) y mama a su lado.

### Ballena jorobada (`Whale`) y orca (`Orca`)
- **Agua**: de la presa (Ortiz 2001).
- **Lactancia**: ballenato ~1 año (cría 365 días, antes 730) [A]; orca 1–2 años (cría 730) [A].
- **La cría nada junto a su madre** (`MarineLife`).

### Pingüino emperador (`Penguin`)
- **Agua**: en el mar bebe agua salada; la glándula de la sal supraorbitaria expulsa el exceso (Schmidt-Nielsen 1960,
  *Circulation* 21:955). En tierra come nieve.
- **Huevo** (`Egg`): de la cópula a la puesta ~0,8 meses (`Calendar`); la hembra pone UN huevo sobre las patas (el emperador
  no hace nido) y se lo pasa al macho en cuanto él llega a su lado; ella, agotada, se va al mar a comer. Él lo incuba
  **2,1 meses** (62–67 días, media 64: récord Guinness; 65 d en Groscolas) **ayunando** (~115 días de ayuno en total,
  Prévost 1961). Si nadie lo cubre, el huevo se hiela en 2 horas de juego. Antes del relevo, la incubación no pasa del 10 %.
- **Crianza** (regurgitación, los dos padres): el pollo va con sus padres (a ≤ 1,2 m: sobre las patas) y lo alimentan los
  dos. Si la hembra llega tarde, el macho lo alimenta con una secreción del esófago (aquí, el mismo «nurse»).
- `StageProfile`: cría 150 días (plumaje impermeable, ~5 meses), joven 1 100 (cría a los 3–5 años), adulto 5 500.

### Malamute (`Malamute`)
- **Agua**: bebe (bebedero de su isla, `WaterSource`) y, sin agua, come nieve como los perros de trineo [A].
- **Nido/guarida**: 12–24 h antes del parto hace el nido en la caseta y pare dentro; los cachorros no salen hasta las ~3
  semanas (0,7 meses) [A]. La perra no ayuna: si tiene mucha hambre sale a comer y vuelve.
- **Lactancia** ~7–8 semanas (cría 60 días); gestación 63 ± 2 días (doi 10.1111/j.1439-0531.2011.01773.x).
- **Los cachorros van detrás de su madre** («heel»).

### Abeja de las nieves (`SnowBee`)
Abeja melífera modificada (docs/abejas-y-panal.md).
- **Agua**: la de la miel/néctar (y la condensación del racimo) [A].
- **Zánganos**: huevo **sin fecundar** (partenogénesis arrenotoca) → macho haploide: huevo 3 días + larva 6–7 + pupa 14,5 =
  **24 días** (obrera 21). La reina fecundada pone uno de cada 8 sin fecundar; la que no se aparea en ~3 semanas (dos tardes
  de vuelo perdidas) se vuelve **zanganera** y solo pone zánganos. El zángano no trabaja: come miel en el panal y, en las
  horas cálidas (11–17 h; vuelan sobre todo de 13 a 16 h), va a la **zona de congregación** (en el aire, a 14 m del árbol y
  12 m de altura) y persigue a la reina virgen.
- **Vuelo nupcial**: la reina virgen (≥ 6 días) sale en las horas cálidas a la zona de congregación; un zángano la alcanza y
  se aparea en el aire (< 5 s) y **muere** (cae). Con al menos una cópula y un rato de vuelo vuelve fecundada y empieza a
  poner. En la naturaleza son 10–20 zánganos en 1–3 vuelos (WSU Extension; los zánganos se juntan por miles en las zonas
  de congregación).
- La muerte del zángano tras aparearse cuenta como natural en el panel (no como «otra causa»).
- Escena: la hembra es la reina **virgen** (`mated = false`) y el macho, un zángano (`drone = true`). En SampleScene las
  reinas ya están fecundadas (`mated = true` por defecto). Con ellos, **7 obreras** de edades repartidas (limpiadora,
  nodrizas, procesadora, guardiana, pecoreadora): una reina no funda colonia sola, y sin nodrizas nadie opercula las larvas
  (no había pupa, ni abeja nueva, ni zángano, ni cambio de oficio).
- **Reversión a nodriza** (`IceInsectLife.Reverts`): si hay larvas y ninguna obrera tiene edad de nodriza (3–10 días),
  hasta 3 obreras mayores vuelven a criar (Robinson et al. 1992, *Ethology* 90:336; Huang & Robinson 1996, *Behav Ecol
  Sociobiol* 39:147). Con el año de la prueba (0,05 días) las 7 obreras eran pecoreadoras antes de la primera larva.

### Araña de las nieves (`SnowSpider`) — especie FICTICIA
`docs/ice-sanctuary-ecology.md` la define como una araña **modificada** para el hielo, de ~30 cm, que teje su **tela
orbicular** junto a los panales y caza abejas de las nieves. No existe, así que su historia de vida se **deriva de arañas
reales**:
- Tamaño y masa de la tarántula Goliat (*Theraphosa blondi*: madura en 3–6 años, la hembra vive 15–25 y el macho 3–6;
  Britannica, Striffler 2005), y el frío, que alarga el desarrollo (las arañas lobo del Ártico tardan ≥ 2 años en lo que en
  clima templado tardan uno: Bowden & Buddle 2012, *Can J Zool* 90:714, doi 10.1139/z2012-038).
- `StageProfile` «SnowSpider»: **cría 30 días** (arañita que vive del vitelo junto al saco, ~5–10 % del tamaño adulto),
  **joven 3 años** de mudas, **adulta 6 años** (compromiso entre hembra y macho; el macho vive menos).
- `Reproduction.Calendar` «SnowSpider»: de la cópula al saco ~1 mes; **un saco al año** (como *Theraphosa*).
- **Saco de huevos** (`Egg`, `LifeHistory.incubator = Female`): la madre lo teje en su tela y lo **guarda**; se incuba
  1,75 meses (*Theraphosa*: 6–8 semanas) y no se hiela (los sacos de las arañas invernan). Las arañitas **no se alimentan**
  (viven del vitelo: nacen sin hambre) y cada una teje luego su tela cerca (dispersión).
- **Cortejo**: el macho adulto saciado va a la tela de la hembra en celo y la **pulsa** (las orbiculares cortejan haciendo
  vibrar los hilos). El canibalismo sexual no se modela.
- **Comida**: `Forager` «SnowSpider» (presa: lo que vuela junto al panal; prefiere la abeja de las nieves, `DietPreference`
  de «Spider»). **Agua**: de la hemolinfa de sus presas [A].
- Todo lo demás (mente, cuerpo de cubos, frases, sueño nocturno) sigue con «Spider».

## La prueba (grupo 58, `CicloVidaTest`)

Por especie y en orden: come → bebe (o el agua de su comida) → duerme → va al baño →
- abeja: vuelo nupcial → puesta → larva → pupa → nace → nace un zángano → la obrera cambia de oficio;
- el resto: celo → cortejo → gestación (o formación del huevo) → [puesta → incubación/relevo o guarda del saco] →
  [entra en la guarida y] pare/eclosiona → crianza (si la hay) → la cría junto a su madre → [sale de la guarida] → joven →
  adulto;
→ envejece y muere de viejo, y nadie muere de otra cosa.

Hay que **reconstruir las escenas** (menú «Build escenas de ciclo de vida») para que tengan la guarida con `Den`, el bebedero
del malamute y la reina virgen con su zángano.

## Pendiente / simplificaciones

- La sed no mata (en el hielo siempre hay nieve); solo incomoda si dura.
- El celo (`EstrusState`) sigue su ciclo corto de siempre; quien manda en las temporadas es el intervalo entre crías del
  calendario.
- Los zánganos no se expulsan en otoño; la reina se aparea con los zánganos que haya (no 10–20).
- `PostNatalManager` (sistema antiguo con las etapas posnatales) sigue sin tocar; la crianza de las crías NACIDAS la llevan
  los deseos (`nurse`, `heel`, `den`, `incubate`).
- [A] = valor de referencia general sin cita primaria verificada.
