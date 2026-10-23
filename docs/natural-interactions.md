# Interacciones naturales: qué hace cada especie y con qué (2026-10-22)

Pedido de Berón: «que todo lo que se coloque en el juego sea interactuable o descartable; optimizar los recursos para
ponerlos sobre elementos interactuables más que decorativos». Este documento recoge la **etología real** de la fauna del
santuario y de los insectos del microcosmos (con fuentes) y cómo se traduce al juego.

## 1. El sistema (un solo camino, integrado en lo que ya había)

| Pieza | Qué es | Dónde |
|---|---|---|
| `Affordance` | Lo que un elemento del entorno OFRECE (`AffordanceKind`: pasto, savia, semilla, hoja, hongo, flor, baya, sauce, liquen, boñiga, posadero, refugio, arena para embudo, respiradero, descansadero, guarida, grava, guijarro, melaza). Consumibles con existencias que **rebrotan** (perezoso, sin `Update`); lugares que se **ocupan** (capacidad). Un objeto puede llevar varios. | `Assets/Scripts/World/Affordance.cs` |
| `Ethogram` | Data por especie: actividad (con qué interactúa, peso, duración, efecto, verbo, **fuente**). `Ethogram.Eats` dice qué interactuables son comida. | `Assets/Animals/Ethogram.cs` |
| Deseo `habit` | Nuevo deseo de la arena de volición (`DesireCatalog`, peso 0,2 × calma). Gana al merodeo al azar (`explore` 0,08) y a volver a casa (0,12) cuando el ser está saciado y tiene algo que hacer cerca; el hambre, la amenaza, cuidar o nutrir a la cría lo superan. | `Assets/Animals/DesireCatalog.cs` |
| `Habit` | Ejecuta la actividad: va al interactuable (NavMesh o línea recta), lo ocupa, aplica el efecto (comer, descansar, refugiarse, vigilar, asolearse, acechar, acarrear a casa, apiñarse) y lo suelta. Se añade solo. | `Assets/Animals/Habit.cs` |
| `Forager` | Las plantas-interactuable cuentan como comida para quien pasta (savia del pulgón, hierba del saltamontes, hongo de la termita…), se gastan al comerlas y rebrotan. El cazador no se distrae con ellas (el oso come bayas por su hábito, no en lugar de cazar). | `Assets/Animals/Forager.cs` |
| `MarineLife` | Focas, ballenas y orcas nadan fuera del NavMesh: su etograma lo ejecuta MarineLife (respiradero, descansadero, grava, patrulla de témpanos). | `Assets/Scripts/World/MarineLife.cs` |
| Censo | `censo` cuenta «en un hábito natural» y ya no marca como «atascado» al que acecha quieto a propósito. | `AnimalVitals.Census` |

## 2. Santuario de hielo

| Especie | Qué hace en la naturaleza (y con qué) | En el juego | Fuente |
|---|---|---|---|
| **Oso polar** | Caza sobre todo **al acecho junto a los respiraderos** de las focas (still-hunting), inmóvil durante mucho rato; descansa en **guaridas temporales** en ventisqueros; en la estación sin hielo come **bayas** (camarina) y algas, sin compensar la grasa de foca. | `BreathingHole`: el oso espera junto al respiradero; si una foca asoma por ESE agujero, la caza. `Den`: guaridas de nieve. `Berry`: manchas de camarina del sotobosque. | Stirling 1974, *Can J Zool* 52:1191; Stirling & Latour 1978; Gormezano & Rockwell 2013, *Ecol Evol* 3:3509; [Polar Research 2493](https://polarresearch.net/index.php/polar/article/download/2493/5744); [Arctic 64405](https://journalhosting.ucalgary.ca/index.php/arctic/article/download/64405/48340/183598) |
| **Foca (anillada)** | Mantiene **respiraderos** abiertos en el hielo con las garras; sale a **descansar y mudar** al hielo (haul-out); las hembras paren en guaridas de nieve sobre los respiraderos. | `BreathingHole`: sube a respirar (asoma la cabeza) y deja su rastro al oso. `HaulOut`: los descansaderos de la orilla (antes eran puntos inventados). | Smith & Stirling 1975, *Can J Zool*; Kelly et al. 2010, *Polar Biol* 33:1095; [PMC8629220](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8629220/) |
| **Pingüino** | Construye el nido con **guijarros**, que escasean: los acarrea uno a uno y se los **roba** al vecino; se **acicala** mucho; con frío se **apiña** (huddling) y la piña rota. | `Pebble`: playa de guijarros de la colonia; cada uno lleva piedras a su nido, que crece y es robable. `HaulOut`: acicalarse en el borde. Apiñarse de noche hacia el centro de la colonia. | [California Academy of Sciences 2004](https://researcharchive.calacademy.org/calwild/2004spring/stories/materialgirls.html); [Notornis 22:77](https://www.birdsnz.org.nz/wp-content/uploads/2011/03/Notornis_22_1_77-78-1.pdf); Gilbert et al. 2006, *Physiol Behav* 88:479; [arXiv q-bio/0701051](https://arxiv.org/abs/q-bio/0701051) |
| **Ballena** (beluga / Groenlandia) | Se **frota contra grava y rocas** del fondo en aguas someras para mudar la piel. | `RubbingGravel`: dos bancos de grava en el fondo de la laguna; rueda sobre ellos. | Fortune et al. 2017, *PLoS ONE* 12:e0186156 ([PMC5699794](https://pmc.ncbi.nlm.nih.gov/articles/PMC5699794)); [National Geographic 2017](https://www.nationalgeographic.com/news/2017/11/whales-skin-spa-oceans-animals-science/) |
| **Orca** | Patrulla los **témpanos** donde descansan focas y, en grupo, las tira al agua con olas (wave-washing). | Patrulla los descansaderos con focas recientes; la caza sigue en su lógica de hambre. | Pitman & Durban 2012, *Mar Mamm Sci* 28:16; [ScienceDaily 2011](https://www.sciencedaily.com/releases/2011/03/110330150852.htm) |
| Zorro ártico | Sigue a los osos para **carroñear** sus presas; madrigueras. | Carroña ya existía (`Forager`, carcasas). `Den`. | [Arctic (UCalgary)](https://journalhosting.ucalgary.ca/index.php/arctic/article/download/64376/48311/183569); [U. Alberta 2026](https://www.ualberta.ca/en/folio/2026/01/predator-and-provider.html) |
| Liebre / reno | Liebre ártica: **sauce ártico** casi exclusivo; reno: **liquen** (Cladonia) en invierno. | `Willow`, `Lichen` del sotobosque (si se añaden al bioma). | Klein & Bay 1994, *Oecologia* 97:439; Heggberget et al. 2002, *Rangifer* 22:13 |

Las abejas y arañas de las nieves (inventadas) tienen su propia vida en `IceInsectLife`/`Hive`; no se tocan aquí.

## 3. Microcosmos (insectos con mente humana, a escala de la hormiga)

| Especie | Qué hace en la naturaleza (y con qué) | Interactuable | Fuente |
|---|---|---|---|
| **Hormiga / mirmidón** | Las granívoras **cosechan semillas** y las guardan en graneros; muchas **ordeñan pulgones** (melaza) y los protegen; las cortadoras llevan **hojas** a su jardín de hongos. | `Seed` (espigas de las briznas), `Honeydew` (cada pulgón que chupa savia la produce), `Leaf` (helechos, para el bancal de hongos del Huerto). | MacMahon et al. 2000, *Annu Rev Ecol Syst* 31:265; Stadler & Dixon 2005, *Annu Rev Ecol Evol Syst* 36:345; [PMC1685857](https://pmc.ncbi.nlm.nih.gov/articles/PMC1685857) |
| **Termita** | Las cosechadoras (*Hodotermes*) **cortan hierba** y la llevan al nido; las *Macrotermes* cultivan **hongos** (*Termitomyces*). Huyen de la luz. | `Grass` (acarreo a casa), `Fungus`, `Shelter`. | [Hodotermes](https://en.wikipedia.org/wiki/Hodotermes); Aanen et al. 2002, *PNAS* 99:14887; [PMC6523192](https://pmc.ncbi.nlm.nih.gov/articles/PMC6523192) |
| **Pulgón** | Chupa **savia** del floema de tallos; excreta melaza. | `PlantSap` (briznas, helechos). | Dixon 1998, *Aphid Ecology* |
| **Mariquita** | Recorre **tallos** buscando colonias de pulgones; come **polen** cuando faltan; se agrega bajo piedras. | `PlantSap` (patrulla), `Flower`, `Shelter`. | [EJES, revisión de forrajeo de coccinélidos](https://www.ejes.cz/index.php/ejes/article/view/362); Hodek, Honěk & van Emden 2012 |
| **Escarabajo pelotero** | Hace una **bola de estiércol**, se sube encima para orientarse (sol, Vía Láctea) y la rueda en línea recta hasta enterrarla. | `Dung` (boñigas repartidas; acarreo a casa), `Shelter`. | Dacke et al. 2013, *Curr Biol* 23:298 ([PMC3886324](https://pmc.ncbi.nlm.nih.gov/articles/PMC3886324)) |
| **Hormiga león** (larva) | Cava su **embudo en arena fina y seca**, en sitios resguardados, y espera en el fondo. | `SandPit`: manchas de arena fina; las hormigas león nacen sobre ellas. | Scharf & Ovadia 2006, *J Insect Behav* 19:197; [EJE 2008](https://www.eje.cz/pdfs/eje/2008/04/13.pdf) |
| **Mantis** | Depredadora de **emboscada**: elige posaderos en **flores** o tallos altos, donde llegan sus presas. | `Flower`, `Perch`. | [ESA 2010 #50791](https://esa.confex.com/esa/2010/webprogram/Paper50791.html); [Behav Ecol 26:194](https://ideas.repec.org/a/oup/beheco/v26y2015i1p194-199..html) |
| **Grillo** | Vive en una **madriguera bajo piedras** y canta en su entrada; omnívoro (semillas, hierba). | `Shelter` (piedras, musgo), `Seed`, `Grass`. | Hissmann 1990, *Ethology* 85:145; [Gryllus campestris](https://www.bioimages.org.uk/html/Gryllus_campestris.htm) |
| **Saltamontes** | Come **hojas de pasto**; se **asolea** en lo alto de los tallos para regular su temperatura. | `Grass`, `Perch` (asolearse). | [PMC2999429](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC2999429/) |
| **Araña** | Tiende su tela entre **tallos**; el sitio depende de la estructura de la vegetación. | `Perch`. | [Ecol Entomol 2000, 25:423](https://wiki.pestinfo.org/wiki/Ecological_Entomology_(2000)_25,_423-432) |
| **Suricata** (skin) | **Centinela** en un punto alto; madrigueras. | `Perch`, `Shelter`. | Clutton-Brock et al. 1999, *Science* 284:1640 |

### La flora del microcosmos (cambio de diseño)

Antes la flora era un **anillo de adorno** fuera del área (musgo de 4 cubos sueltos, ~1 600 piezas sin uso). Ahora crece
por **toda la superficie hasta el agua** —también por el suelo libre del área, entre sus muebles gigantes— en una rejilla
con jitter (una planta por celda de 12 m, algunas celdas vacías para poder pasar), salvo un claro de 20 m alrededor de la
historia. Cada planta es un nodo sin escala con sus `Affordance` y sus piezas como hijos:

- **Brizna** (pasto): `Grass` + `PlantSap` + `Perch`; la mitad con **espiga** y semillas caídas al pie (`Seed`).
- **Helecho**: `PlantSap` + `Leaf` + `Perch` + `Shelter`. **Hongo**: `Fungus` + `Shelter`. **Flor**: `Flower` + `Perch`.
- **Musgo** y **piedra**: `Shelter`. **Arena fina**: `SandPit`. **Boñiga**: `Dung`.

Los mirmidones (`MicroForager`) ya no solo llevan restos de comida: **cosechan semillas** y **ordeñan pulgones**; las
cultivadoras del Huerto (`MicroGardener`) **cortan hojas de los helechos**.

### Insectos quietos

La fauna insecto cuelga de un padre `MicroSize` escalado cientos de veces; su `NavMeshAgent` heredaba un radio/altura
absurdos y, muy probablemente, no quedaba sobre el NavMesh del microcosmos, así que la IA no podía moverlo (no lo he
podido ver en Unity; es la causa más probable en el código). `MicroSize` ahora le da un tamaño de
agente acorde a su largo y lo posa en el NavMesh (`Warp`). A falta de NavMesh, el `Habit` los mueve en línea recta.

## 4. Pendiente

- Comprobar en Unity (sincronizar + `verify-compile`, reconstruir microcosmos y blockout): `censo` debe mostrar «en un
  hábito natural» > 0 y los logs `[Hábito]`, `[Marinos] … respiradero`, `[Hábitats]`.
- Celo/cortejo y crianza ligados a estos lugares (la foca pare en una guarida de nieve sobre su respiradero; el pingüino
  incuba en su nido de guijarros).
- Liebre y reno no están en el bioma: su sauce y liquen ya son comida si se añaden.

## Arreglo 2026-10-23: hábitos en el mundo principal

La prueba en Unity mostró `[Hábito]` en el microcosmos pero ninguno en el santuario («en un hábito natural» = 0):
- **Permiso caducado por LOD.** `Habit` vive de un permiso que renueva cada tick de volición; los animales lejos del
  jugador deciden hasta `Lod.maxSlow`× más despacio, así que el permiso caducaba antes del siguiente tick y el hábito se
  soltaba antes de llegar a su sitio. Ahora el permiso se estira con `Lod.SlowFactor`.
- **Descanso en su sitio.** Cansado y con guarida/descansadero/refugio cerca, el deseo «habit» absorbe al «rest»: el oso
  va a su guarida de nieve, el pingüino al descansadero o a la piña, en vez de pararse donde esté.
- **Marinos en el censo.** Focas/ballenas/orcas hacen su etograma dentro de `MarineLife` (`InHabitat`), y ahora cuentan.
  El censo añade «con sitio cerca» para distinguir «no encuentra Affordances» de «no gana el deseo».
- **Ordeño.** `MicroForager` va a un pulgón con melaza (≥ 0,3) dentro de `milkRadius` aunque haya semillas más cerca.
- **Animales congelados por comida inalcanzable** (2ª prueba): `Forager.Graze` andaba hacia el banco de peces de la
  laguna sin límite; el pingüino/oso, a pie, se quedaba en la orilla y el bucle no acababa → `busy` para siempre → ni
  volición ni hábito. Ahora se rinde si no avanza en ~4 s y, si quedó en la orilla (< `shoreFishingReach`), **pesca
  desde el borde** un bocado. `Hunt` deja una carcasa inalcanzable tras ~60 intentos.
- **Comer por hábito**: con hambre y comida natural cerca (savia, bayas, liquen, respiradero para el oso), «habit»
  compite con «eat» de igual a igual y prefiere los actos de comer.
- **Censo** (diagnóstico): «ocupados» (`busy`) y el último deseo ganador por especie.
