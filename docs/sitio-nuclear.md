# El sitio nuclear (investigación → decisión → construcción, Berón 2026-11-05)

Pedido de Berón: «la planta atómica sigue usando el antiguo modelo de edificio? se puede migrar para quedar como el lanza cohetes? en su
propio edificio-árbol en un casquete artificial y a gran altura con medidas de seguridad para tener el menor impacto posible en el
santuario en caso de fallos?»

**Respuesta corta.** Ya eran pisos de árbol (la Planta Atómica) o un piso aparte (la Central Nuclear), pero al lado de la gente. Se mudan
a su propio casquete artificial y a su propio árbol, como la lanzadera; **pero no a gran altura**. Lo que protege de un reactor es la
**distancia, la contención y el agua**, no estar en alto. Los reactores van **abajo** y lo que sube es lo que sí gana con la altura: el
agua de emergencia (por gravedad), la chimenea filtrada y las salas de mando. **Actualizado:** el TRIGA, en un cajón apoyado en el lecho; el microrreactor y los residuos, en cavernas bajo el lecho (§3 y §4).

## 1. Lo investigado

### Distancias
- **Centrales (EE. UU., 10 CFR 100):** se define una **zona de exclusión**, una **zona de baja población** a su alrededor y la distancia
  al centro de población. La referencia de la zona de exclusión son 25 rem (250 mSv) TEDE en 2 h; el propio texto avisa de que no es un
  límite aceptable de dosis al público en emergencia.
- **Reactores de investigación (NUREG-0849, ANSI/ANS-15.16, Regulatory Guide 2.6):** la zona de planificación de emergencias crece con la
  potencia térmica: desde el **límite de operaciones** (≤ 2 MW) hasta **800 m** (hasta 50 MW). Por encima de 50 MW, caso por caso. El
  objetivo es no pasar de 1 rem a cuerpo entero o 5 rem a tiroides. *(Los escalones intermedios no los pude confirmar: el apéndice III no
  salió en la búsqueda.)*
- **SMR y microrreactores (10 CFR 50.160, Regulatory Guide 1.242, 2023):** marco basado en resultados, con zona de emergencia **escalable**
  en lugar de las 10 millas de las grandes. Westinghouse dice que la de eVinci quedaría dentro del límite de la unidad («zero EPZ»), pero
  es **su afirmación**: el método seguía en revisión previa con la NRC.

### Los dos reactores no son iguales
| | Planta Atómica (TRIGA) | Central Nuclear (tipo eVinci) |
|---|---|---|
| Potencia | 1 MW térmico | ~15 MW térmicos / 5 MWe |
| Enfriamiento normal | Piscina e intercambiador con agua de mar polar | Tubos de calor, sin bombas |
| Calor residual | Convección natural en la piscina (análisis de hasta 1,9 MW sin ebullición en película) | Pasivo: convección y radiación, sin operador |
| Si se pierde el agua | LOCA a 1 MW: 585 °C de pico frente al límite de 950 °C | No usa agua |
| Combustible | UZrH (coeficiente de temperatura negativo) | TRISO al 19,75 % |

### Pasivo y por gravedad
**AP1000:** depósito **en lo alto** del edificio de blindaje: el agua baja **por gravedad** sobre la vasija de contención, sin operador, al
menos **72 h**. Después hay que reponer agua con medios activos. Es el precedente directo de usar la altura para seguridad.

### El precedente polar
**PM-3A (McMurdo, Antártida, 1962–72):** 1,75 MW; apodado «Nukey Poo» por sus fugas. Se sospechó corrosión bajo tensión por cloruros
(nunca se confirmó). El Tratado Antártico impidió enterrarlo: se desmontó y **se llevó de vuelta con parte del suelo contaminado**.
Lecciones para el sitio: **el agua de mar no toca el circuito primario** (intercambiador intermedio) y **una plataforma artificial
retirable es mejor que dejar el reactor en el hielo natural**.

### ¿Y la altura?
La respuesta es matizada, y por eso no vale copiar la lanzadera:

- **A favor de soltar en alto:** en el modelo gaussiano, la concentración máxima a ras de suelo baja aproximadamente con el cuadrado de la
  altura efectiva de emisión (libro de texto: Turner). Por eso las centrales tienen chimenea. **Pero eso es la salida del gas filtrado, no
  el reactor.**
- **En contra de poner el reactor en alto** (razonamiento de ingeniería, no una cita):
  1. Una piscina de ~270 m³ en voladizo a 64 m es peso y palanca en el edificio.
  2. Si se raja, el agua **se va hacia abajo**. Abajo, la piscina apoya en el suelo y lo que escape queda en la cubeta.
  3. Se pierde el blindaje del entorno: en lo alto no hay nada alrededor del núcleo.
  4. Una fuga sin filtrar se va más lejos: la dilución en altura no compensa no haberla contenido.
  5. El sumidero final (el mar) queda lejos: más tubería y más bombeo, justo lo que un diseño pasivo evita.

La lanzadera sube por otro motivo: lo que ella teme (el chorro, los cascotes) sale **hacia arriba y afuera** y hay que separarlo de las aves
y los pisos. Lo que teme un reactor es **quedarse sin agua**, y el agua cae hacia abajo.

## 2. La decisión

- **Lugar:** esquina sureste del mar, centro en **(510, 0, −510)**: **≥ 500 m** de todo edificio e isla del santuario y a más de 1 km de la
  lanzadera. El mapa no da para más: el mar acaba en ±600 m. Son menos que los 800 m del tope de la escala de reactores de investigación,
  que es para 50 MW; aquí hay 16 MW térmicos, una tercera parte. **Ninguna norma da exactamente esta cifra**: es «lo más lejos que permite
  el mapa».
- **Viento:** el juego no simula viento. Se fija `NuclearSiteRule.PrevailingWindTo` (hacia el sureste, como los vientos catabáticos que
  bajan hacia el mar) y el sitio queda **a sotavento** del santuario. Lo comprueba la regla.
- **Árbol reforzado y bajo:** primer piso a **12 m** (no 64). Es una excepción a la regla de los 50 m, que existe para dejar sitio a los
  árboles que crecen en el suelo; en el casquete no crece ninguno. 3 niveles.
- **De abajo arriba:**
  - nivel 1 (12 m): **Central Nuclear** y **Planta Atómica**, los dos reactores;
  - nivel 2 (34 m): **ciclotrón** y **sala de experimentos**;
  - nivel 3 (56 m): **Centro de emergencias** (panel de parada remota y vigilancia) y **garaje** del submarino.
- **Lo que sube:** el **tronco**, una columna de agua que ya estaba ahí, es el **depósito de emergencia por gravedad** (lo calcula
  `NuclearSiteRule`); las **válvulas fallan abiertas**; la **chimenea filtrada** va en un mástil.
- **Lo que queda abajo:** el núcleo del TRIGA, **bajo la losa**, dentro del **recinto de contención** que apoya en el casquete; su piscina
  deja de colgar en voladizo y descansa en un pedestal. El microrreactor, en su **cámara de contención** con techo.
- **Sumidero final:** el mar, por el **fondo abierto** del tronco y la toma de agua, con **intercambiador intermedio**: el agua de mar nunca
  toca el primario.
- **Vigilancia y plan:** estaciones de radiación en el borde del casquete, boyas de la zona de exclusión, simulacro en la jornada y
  `NuclearSite.DeclareEmergency` (parada, válvulas abiertas, balizas). La evacuación mueve de verdad a la gente (§3).
- **Se mantiene todo:** los pisos, el equipo, las misiones, las máquinas de virtualización y los microcosmos (los nombres de área no
  cambian). El ciclotrón y la sala de experimentos se mudan con la Planta porque son plantas de esa misma área. El F-18 del PET (vida media
  de 110 min) llega al hospital de sobra: unos 40 s en alfombra. La energía y el calor de distrito llegan por una línea submarina señalizada.
- **Ciencias** se queda con Química, Biología e Informática y **3 pisos libres** arriba (no se recorta el árbol: su altura y sus holguras
  siguen igual).

## 3. ¿Al fondo del mar? ¿Bajo tierra? (Berón 2026-11-05)

Pedido: «crea la planta atómica siguiendo la investigación para que sea lo más segura posible, quizás se debería crear al fondo del mar?
bajo tierra? de ser así la puedes crear así».

*(Corregida por §4: esta sección queda como razonamiento; el microrreactor pasó a la roca y el cajón del TRIGA apoya en el lecho.)*

**Decisión: ni al fondo ni bajo tierra, sino BAJO EL MAR Y BAJO EL CASQUETE.** Cada núcleo va en su **cajón sumergido de doble casco**, de 2 a
18 m bajo el nivel del mar, colgado del casquete y atirantado al fondo. Sustituye al recinto sobre el casquete y al pedestal de §2: es la
parte de esa decisión que cambia. **Todo lo demás sigue**: la distancia, el sotavento, el árbol bajo, el agua del tronco, la chimenea y T14.

### Lo investigado
- **Fondo marino: Flexblue (DCNS, 2014).** 160 MWe, cilindro de 150 × 14 m, a 30–100 m de hondo. Su argumento es bueno: el mar alrededor del
  casco es un sumidero prácticamente infinito, y el calor residual sale por circulación natural; hay estudios independientes del apagón total y
  de la convección alrededor del casco. **Pero no pasó de estudio de viabilidad**: no hay ninguno construido que yo haya encontrado.
- **Bajo tierra.** Estudios de los años 70 y 80 lo ven como mejor contención tras un accidente, con un sobrecoste estimado por debajo del
  10 %, cifras antiguas. El interés cayó tras el simposio de Hannover (1981), sobre todo por el coste percibido.
- **Lucens (Suiza, 1969): la advertencia.** Reactor en caverna; agua condensada corroyó piezas de magnesio y hubo fusión parcial. La caverna hizo
  de contención y no hubo daños a personas, **pero no estaba del todo sellada: la radiactividad llegó a la sala de control, a 100 m**. Además la
  caverna quedó inutilizable: se selló, se desmontó hasta 1973 y se rellenó de hormigón en 1988.
- **Deep Fission (reactor en un pozo de una milla).** Promete que la roca y el agua hacen de contención. Son afirmaciones de la empresa; expertos
  independientes señalan incertidumbres y que para repostar o reparar hay que subir el reactor.
- **PM-3A (§1):** el cloruro del mar es enemigo del acero del reactor, y el Tratado Antártico obligó a llevarse el reactor.

### Por qué no el resto
| | Bajo tierra | Fondo del mar | Sobre el casquete (§2) | **Cajón bajo el casquete** |
|---|---|---|---|---|
| ¿Existe aquí? | **No**: bajo el casquete solo hay mar, y bajo las islas, hielo | Sí | Sí | Sí |
| Sumidero de calor | Roca, lento | El mar, en contacto | Tubería hasta el mar | **El mar, en contacto** |
| Tiempo, cohetes, cascotes | Protegido | Protegido | Expuesto | **Protegido** |
| Si se rompe todo | Queda en la roca | **Directo al mar y a su cadena: kril, peces, ballenas** | Al aire | Dos cascos y sondas, y queda retirable |
| Reparar y retirar | Difícil | Muy difícil, y hielo que raspa | Fácil | **Por el tronco; se puede remolcar** |

La pega real del mar es que **el santuario vive de él**, y la del fondo, además, que nadie llega rápido si falla. El cajón bajo el casquete
toma el sumidero de Flexblue y la protección de la caverna, sigue a mano y, si hace falta, se saca entera como exigió el caso PM-3A.

### Lo que se aprende de Lucens y de PM-3A
- **Doble casco** con espacio anular y **sondas en el agua de mar**: ni agua hacia dentro ni radiactividad hacia fuera sin que se vea.
- El agua de mar **nunca** toca el reactor: el circuito intermedio sigue.
- **Las salas de mando, en otro aire**: el Centro de emergencias está a 56 m, fuera del cajón; lo de Lucens (la sala de control contaminada a
  100 m) no se repite. La sala de control de cada planta sigue en su piso, y de ahí **se evacúa**.
- Un cajón por reactor: un fallo no se lleva al otro.

### Cómo queda
- **Planta Atómica:** su piso sigue a 12 m, con la consola y el puente; la piscina baja **20 m**, atraviesa el casquete y llega a su cajón. El
  núcleo queda a −7 m, con casi 19 m de agua encima (los TRIGA reales tienen unos 8: más agua es más blindaje; **ya no es la medida real**).
- **Central Nuclear:** en el piso quedan el pozo y sus equipos de conversión; el módulo del microrreactor está en su cajón, a −12 m.
- **Acceso:** una **esclusa** va del tronco (abierto al mar) a cada cajón, para mantenimiento. **Nadie del personal baja en la jornada**: el
  cajón no tiene colisiones, y el trabajo se hace desde los pisos.
- **Evacuación real:** con `DeclareEmergency`, cada 2 s `WorkdayDirector.EvacuateSite` manda al hospital a quien siga en la zona de exclusión;
  a Kushal lo saca la Magnate. Mientras dure, la jornada no manda a nadie a esos pisos.
- **Tiempo libre:** el ocio no abre ningún área de la zona, y quien acaba allí su turno se va.

### Fuentes de esta sección
- Flexblue: [EPJ Nuclear Sciences & Technologies, 2015](https://www.epj-n.org/10.1051/epjn/e2015-50030-x), [Politecnico di Milano, 2017](https://re.public.polimi.it/bitstream/11311/1046550/1/2017%20PNE%20External%20heat%20transfer%20capability%20of%20a%20submerged%20SMR%20containment%20The%20Flexblue%20case.pdf)
- Bajo tierra: [Aerospace Corp., 1975](https://inis.iaea.org/records/ns5eq-acp87), [Concept of underground siting](https://www.osti.gov/biblio/6277078), [Myers, *Mines to Megawatts*](https://nuclear.mst.edu/media/academic/nuclear/images/minestomegawatts/Mines%20to%20Megawatts%20-%20WesMyers.pdf)
- Lucens: [Swissinfo](https://www.swissinfo.ch/eng/history/historic-nuclear-accident-dashed-swiss-atomic-dreams/44696398), [Wikipedia](https://en.wikipedia.org/wiki/Lucens_reactor)
- Deep Fission: [IEEE Spectrum](https://spectrum.ieee.org/underground-nuclear-reactor-deep-fission)

## 4. Bajo la roca del fondo del mar (corrección de Berón, 2026-11-05)

Berón: «la opción de bajo tierra era bajo tierra en el fondo del mar, supongo que el mar tendrá un fondo y allí habrá tierra y se podría construir
sobre la tierra o debajo o una parte enterrada y otra no según convenga».

**Tiene razón y cambia el diseño de §3.** Yo descarté «bajo tierra» porque bajo el casquete solo había mar, pero el fondo existe (el colisionador
«FondoMarino», a −16,25 m; ni se veía) y debajo puede haber roca. **Decisión: una parte enterrada y otra no, según quién la atienda.**

| Pieza | Dónde | Por qué |
|---|---|---|
| **Microrreactor** (~15 MW térmicos) | **Caverna con 60 m de roca encima** (a unos −81 m) | Es el mayor inventario y nadie lo atiende a diario: enterrarlo no cuesta nada |
| **Residuos y combustible gastado** | **Caverna de al lado** | Lo que más dura; que no toque el agua, y que se pueda sacar |
| **TRIGA** (1 MW) | **Cajón de doble casco apoyado en el lecho** | 15 veces menos potencia y gente a diario en la consola y el puente: enterrarlo alarga la salida justo a quien trabaja ahí |

### Lo investigado
- **Bajo el mar se puede excavar y se trabaja a diario:** el SFR de Forsmark guarda residuos a **60 m bajo el fondo del Báltico** desde 1988; el túnel del
  Canal va a unos 40 m bajo el lecho durante 37,9 km; la mina de Boulby (1100–1400 m) se extiende kilómetros bajo el mar del Norte.
- **Reactores en la roca:** Halden (25 MW térmicos, **a 100 m de roca**, en servicio 1958–2018) y Ågesta (Estocolmo, 1964–74, calor de distrito); ambos
  funcionaron años. Lucens (§3) es la otra cara: la caverna contuvo la fuga, pero no estaba bien sellada.
- **Onkalo (Finlandia):** a unos 430 m, en roca cristalina. Es para siempre; aquí no hace falta tanto, y sería irrecuperable.

### Cajón frente a caverna
| | Cajón | Caverna bajo el lecho |
|---|---|---|
| **Blindaje** | Agua y dos cascos | **60 m de roca** |
| **Si hay fuga** | Dos cascos y luego el mar, directo a la cadena del kril | **La roca retiene y retrasa**; sale, si sale, por fisuras y por el pozo |
| **El punto débil** | La corrosión del casco por fuera | **El pozo y las fisuras**: son el camino del agua y de cualquier fuga; por eso va sellado y se vigila |
| **Agua que entra** | No debería | Siempre entra algo: el túnel del Canal lleva revestimiento y bombas; aquí, **sumidero** |
| **Sumidero de calor** | El mar, tocando el casco | **La roca no se lleva el calor: hace falta un bucle de agua** |
| **Tiempo de acceso** | Minutos, por la esclusa | Un pozo de ~90 m; reparar es lento |
| **Retirada** | Se remolca | Hay que subirlo por el pozo |

**El calor era la pega, y tiene solución pasiva:** la caverna queda **más abajo** que su intercambiador del lecho, así que el agua caliente sube sola
por el **bucle de termosifón** hasta el mar, sin bomba. Es el mismo argumento de §1: el agua fría cae y la caliente sube.

**Lo que no sé:** el juego no tiene geología, así que «roca buena» es un supuesto. En la realidad, antes de excavar habría que estudiar la roca, y
ninguna central moderna se ha construido bajo el mar: esto combina precedentes por separado, no copia uno.

### Cómo queda
Lecho visible bajo el sitio (`Lecho_marino`); pozo del microrreactor desde su piso hasta la caverna, con una esclusa desde el tronco; galería entre las
dos cavernas; `Bucle_termosifon`, `Intercambiador_lecho` y `Sumidero_caverna`. El cajón del TRIGA baja hasta el lecho y apoya en su zapata (ya no lleva
tirantes). Nadie baja a las cavernas en la jornada: no tienen colisiones y la roca no se ve por dentro.

## 5. ¿Energía de los movimientos sísmicos?

**No.** Es la respuesta honesta, aunque la idea sea bonita.

- **Cuánta hay:** con la fórmula de Gutenberg-Richter (log E = 1,5 M + 4,8, en joules), un terremoto de magnitud 9 radia ~2×10¹⁸ J, y la media mundial queda del
  orden de 10¹⁷–10¹⁸ J al año (estimación mía; no encontré una fuente primaria con esa cifra). El mundo usa ~6×10²⁰ J al año: **todos los terremotos del planeta,
  captados al 100 %, serían menos del 0,2 %.**
- **Por qué no se capta:** llega de golpe, sin avisar, repartida en una región enorme y casi toda en terremotos grandes y raros. Cinco expertos consultados
  lo llamaron «concebible» pero «totalmente impracticable» (HowStuffWorks); un estudio de Irán (2025) tampoco lo justifica en la práctica.
- **Lo que sí hay bajo el suelo es calor, no temblor:** el planeta pierde **47 TW** (Davies y Davies, 2010): ~1,5×10²¹ J al año, **más de mil veces** la energía
  sísmica. Pero es muy difuso: 91,6 mW/m² de media. Bajo todo el casquete (170 × 170 m) son **2,6 kW**, cuando la central da 5 MW eléctricos.
- **Geotermia real:** un pozo típico da 5–8 MW, pero **en zonas volcánicas**. El IDDP de Islandia llegó a ~25 MW porque topó con magma. Con el gradiente normal (~25–30 °C/km,
  dato de memoria) harían falta kilómetros de perforación para agua templada.
- **Las fuentes hidrotermales, ni tocarlas:** son ecosistemas únicos (más del 95 % de sus especies eran nuevas para la ciencia, muchas de un solo campo), las Azores ya protegen Lucky Strike y hay fuentes en el océano Austral (East Scotia, con su langosta yeti). El juego no tiene ninguna.

**Recomendación:** nada de sísmica; y **no paso nada a `docs/energia.md`**: el santuario no tiene volcanes, la geotermia aquí daría poco y el reactor ya es la carga base. Si un día se
quiere calor de distrito sin depender del reactor, lo que encajaría sería un pozo **cerrado**, sin extraer agua: no afecta a animales ni plantas. Eso sí respondería a su
pregunta de «sin afectar a los animales».

## 6. Materiales de cohetes y centros de investigación: según su peligro

Regla (manual **T15**): **cada cosa va donde lo pide su peligro, no su tema**. No se mueve lo que no lo necesita.

| Qué | Peligro | Dónde | Cambia |
|---|---|---|---|
| Oxígeno líquido y RP-1 | Fuego y explosión si se mezclan | **Al aire**, en el casquete de la lanzadera, a más de 80 m entre sí, con cubeta | **Nuevo:** no existían |
| Cargas de separación de etapas | Explosivo | **Polvorín** en otra esquina del casquete | **Nuevo** |
| Fábrica de cohetes | Baja: etapas **vacías** | Sigue en su piso | No |
| Control de misión | Ninguno | Sigue en su piso | No |
| Química | Armarios con cantidades de laboratorio | Sigue en Ciencias | No |
| Biología, Informática y demás | Bajo | Siguen | No |
| Ciclotrón y sala de experimentos | Bajo | Siguen en el sitio nuclear | No |
| Residuos y combustible gastado | Alto y duradero | **Caverna bajo el lecho** | Sí |

**Por qué los propulsores NO van bajo tierra**, aunque la pregunta invitaba a ello: bajo tierra el oxígeno y los vapores se acumulan, y una explosión confinada es peor
que una al aire (razonamiento mío de ingeniería). Es el contrario exacto del reactor: el reactor quiere encierro; el explosivo, aire libre y distancia. La norma de EE. UU. (14 CFR 420) mide por
kilos netos y separa lo incompatible; desde 2012 hasta exime de distancias a LOX y RP-1 almacenados por separado, salvo cerca de un incompatible o juntos en el vehículo. Por eso van
separados y lejos, no enterrados. **No encontré normas sobre almacenes de propulsor en caverna**: si los hubiera, habría que revisarlo.

### Fuentes de §4–§6
- [SFR, Forsmark (World Nuclear News)](https://world-nuclear-news.org/articles/expansion-of-swedish-repository-under-way), [Onkalo](https://xenospectrum.com/en/onkalo-deposition-hole-drilling/), [Boulby](https://en.wikipedia.org/wiki/Boulby_Mine), [Túnel del Canal (Vinci)](https://www.vinci-construction-projets.com/es/realisations/tunnel-sous-la-manche/)
- [Halden](https://en.wikipedia.org/wiki/Halden_Reactor), [Ågesta (Vattenfall)](https://history.vattenfall.com/agesta-power-plant)
- [HowStuffWorks](https://science.howstuffworks.com/environmental/energy/harvest-energy-from-earthquake.htm), [Physics World](https://physicsworld.com/a/could-we-generate-useful-energy-from-earthquakes/), [Scientific American](https://www.scientificamerican.com/article/could-we-harness-energy-from-earthquakes/), [Irán (2025)](https://civilica.com/doc/2670044/)
- [Davies y Davies, 2010](https://se.copernicus.org/articles/1/5/2010/), [IDDP (Smithsonian)](https://www.smithsonianmag.com/innovation/deep-heart-iceland-theres-new-way-tap-earths-energy-180961030/), [UC Davis](https://www.ucdavis.edu/news/magma-power-geothermal-energy)
- [Marine Policy: proteger las fuentes activas](https://dukespace.lib.duke.edu/server/api/core/bitstreams/d3f5fcd7-4c15-4ae7-819f-c0d9e10b9bfe/content), [Lucky Strike](https://emso.eu/2026/03/30/life-in-a-patchwork-habitat-mosaics-and-biodiversity-patterns-at-hydrothermal-vents-and-periphery/)
- [FAA: Explosive Siting Requirements, 2012](https://www.federalregister.gov/documents/2012/09/07/2012-21922/explosive-siting-requirements)

## Fuentes
- [10 CFR 100.11](https://www.law.cornell.edu/cfr/text/10/100.11), [10 CFR 100.3](https://www.law.cornell.edu/cfr/text/10/100.3), [10 CFR 53.530](https://www.law.cornell.edu/cfr/text/10/53.530)
- [NRC: emergencias en reactores de investigación](https://www.nrc.gov/about-nrc/emerg-preparedness/about-emerg-preparedness/research-test.html); [archivo de esa página](https://webarchive.library.unt.edu/eot2008/20080916122805/http:/www.nrc.gov/about-nrc/emerg-preparedness/protect-public/research-test.html)
- [Morgan Lewis: la regla de la NRC para SMR y reactores avanzados (10 CFR 50.160)](https://morganlewis.com/blogs/upandatom/2023/11/nrc-provides-emergency-preparedness-flexibility-for-smrs-and-advanced-reactors)
- TRIGA: [SAR de K-State](https://www.nrc.gov/docs/ML1731/ML17319A307.pdf), [NETL PSAR](https://nuclear-twins.tacc.utexas.edu/static/document/NETL%20PSAR.pdf), [TRIGA Mk II LOCA](https://www.trtr.org/wp-content/uploads/2017/11/TRIGA-mkII-Loss-of-Coolant-Accident-1.pdf), [UC Davis, NRC](https://www.nrc.gov/docs/ML2009/ML20097C651.pdf)
- eVinci: [Westinghouse en el IAEA](https://conferences.iaea.org/event/374/contributions/31535/attachments/18122/30561/TrackB.7_331_spalding_presentation.pdf), [taller de ASME](https://resources.asme.org/hubfs/CS-Codes-Standards/CS-Section%20III%20Div%205%20HTR%20V%20Workshop%20Files/HTR%20Presentations/HTR%20Day%201/14.%20WEC-eVinci.pdf)
- AP1000: [NRC](https://www.nrc.gov/docs/ML1122/ML11221A070.pdf), [Westinghouse: apagón total](https://westinghousenuclear.com/media/3pipeoof/ap1000-station-blackout.pdf)
- PM-3A: [Polar Record, Cambridge](https://www.cambridge.org/core/product/A35009D4403A289EF51E5F61EE54CFF4/core-reader), [Bellona](https://bellona.org/?p=8558)
- Dispersión: [Turner, *Workbook of Atmospheric Dispersion Estimates*](https://archive.org/download/in.ernet.dli.2015.165670/2015.165670.Workbook-Of-Atmospheric-Dispersion-Estimeates_text.pdf)
