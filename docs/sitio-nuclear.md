# El sitio nuclear (investigación → decisión → construcción, Berón 2026-11-05)

Pedido de Berón: «la planta atómica sigue usando el antiguo modelo de edificio? se puede migrar para quedar como el lanza cohetes? en su
propio edificio-árbol en un casquete artificial y a gran altura con medidas de seguridad para tener el menor impacto posible en el
santuario en caso de fallos?»

**Respuesta corta.** Ya eran pisos de árbol (la Planta Atómica) o un piso aparte (la Central Nuclear), pero al lado de la gente. Se mudan
a su propio casquete artificial y a su propio árbol, como la lanzadera; **pero no a gran altura**. Lo que protege de un reactor es la
**distancia, la contención y el agua**, no estar en alto. Los reactores van **abajo** y lo que sube es lo que sí gana con la altura: el
agua de emergencia (por gravedad), la chimenea filtrada y las salas de mando. **Actualizado:** el TRIGA, en un cajón colgado del casquete (§9); el microrreactor y los residuos, en cavernas bajo el lecho (§3 y §4).

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

*(Corregida por §4 y §9: esta sección queda como razonamiento; el microrreactor pasó a la roca, y con el mar a 500 m el cajón del TRIGA vuelve a colgar, como aquí.)*

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
«FondoMarino», entonces a −16,25 m; ni se veía. **Hoy, a −500 m: §9**) y debajo puede haber roca. **Decisión: una parte enterrada y otra no, según quién la atienda.**

| Pieza | Dónde | Por qué |
|---|---|---|
| **Microrreactor** (~15 MW térmicos) | **Caverna con 60 m de roca encima** (hoy, a unos −566 m: §9) | Es el mayor inventario y nadie lo atiende a diario: enterrarlo no cuesta nada |
| **Residuos y combustible gastado** | **Caverna de al lado** | Lo que más dura; que no toque el agua, y que se pueda sacar |
| **TRIGA** (1 MW) | **Cajón de doble casco** (entonces apoyado en el lecho; hoy colgado, §9) | 15 veces menos potencia y gente a diario en la consola y el puente: enterrarlo alarga la salida justo a quien trabaja ahí |

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
| **Tiempo de acceso** | Minutos, por la esclusa | Menos de un minuto por la línea (§8); reparar sigue siendo lento |
| **Retirada** | Se remolca | Hay que subirlo por el pozo |

**El calor era la pega, y tiene solución pasiva:** la caverna queda **más abajo** que su intercambiador del lecho, así que el agua caliente sube sola
por el **bucle de termosifón** hasta el mar, sin bomba. Es el mismo argumento de §1: el agua fría cae y la caliente sube.

**Lo que no sé:** el juego no tiene geología, así que «roca buena» es un supuesto (§7 dice qué cambia si no lo es). En la realidad, antes de excavar habría que estudiar la roca, y
ninguna central moderna se ha construido bajo el mar: esto combina precedentes por separado, no copia uno.

### Cómo queda
Lecho visible bajo el sitio (`Lecho_marino`); pozo del microrreactor desde su piso hasta la caverna, con una esclusa desde el tronco; galería entre las
dos cavernas; `Bucle_termosifon`, `Intercambiador_lecho` y `Sumidero_caverna`. El cajón del TRIGA bajaba hasta el lecho y apoyaba en su zapata (§9 lo deshace: ya no llevaba
tirantes). *(Actualizado en §8: a la galería se baja por la línea, una vez a la semana o con aviso.)* En las cavernas no se anda: no tienen colisiones y la roca no se ve por dentro.

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

## 7. ¿La tierra podría aplastar las cavernas? (Berón 2026-11-05)

Berón: «bajo tierra podría pasar que la tierra se quiera compactar aplastando las instalaciones? se deberían entonces crear con mucho margen de espacio para reducir el
impacto de reducciones de terreno dando tiempo a actuar?»

**Respuesta corta.** Depende de la roca. **En sal o en arcilla, sí: la excavación se cierra sola, y el margen de espacio compra décadas. En roca dura como la que el juego supone,
casi no pasa: ahí lo que puede fallar es que caiga un bloque o suba el agua, y eso no lo para el espacio libre, lo avisa la lectura.** El juego no tiene geología, así que
«roca dura» es un **supuesto** de diseño. Lo más expuesto a compactarse no era la caverna: era el **cajón del TRIGA**, entonces apoyado en el sedimento (§9 lo cuelga: ya no le afecta).

### Lo investigado
| Sitio | Roca | Lo medido | Qué enseña |
|---|---|---|---|
| **SFR, Forsmark** (60 m bajo el Báltico; excavado hasta 1986) | Cristalina | **18 extensómetros: ≤ 0,30 mm en 1997–2006**, con 0,02 mm de precisión; casi todos, nada. Los 49 bulones de referencia, óptimos en 2006; sin corrosión pese al agua salina. Agua: de 720 l/min (1988) a unos 320–380; **de las cavernas y el silo solo 18 l/min, y unos 200 de los túneles de acceso** | En esta roca, 20 años sin problemas de estabilidad. **El punto débil del agua es el acceso** |
| **Gjøvik** (61 m de luz, 25–50 m de roca encima) | Gneis (de memoria) | **7–8 mm** de techo con la luz entera, y casi todo al excavar | Incluso una caverna cuatro veces más ancha que las nuestras se mueve milímetros |
| **WIPP** (655 m) | Sal | **5–13 cm/año** de cierre habitual; una sala cerró un 25 % en 10 años. En 2016 el Panel 7 pasó a ~90 cm/año y **cayó una losa de unos 60 m de largo**, 20 min después de que la cuadrilla saliera | **La convergencia dio un mes de aviso**; una inspección a la vista 5 días antes no vio nada. *(Cifras de 2016 citadas de la evaluación de MSHA por una web de activistas: no leí el original)* |
| **Minas de sal polacas** (> 900 m) | Sal | 100–600 mm/año los primeros años; 30–100 después | A más hondura, más cierre |
| **Boulby** (1100–1400 m) | Sal y potasa | Se describe un «excessive ground squeezing» | **No conseguí cifras**: están en una tesis de Newcastle que no pude abrir |
| **Ekofisk** (mar del Norte) | Creta y sedimento | **El lecho bajó 3,8 m hasta 1987** y hubo que levantar seis plataformas 6 m | El suelo blando bajo el mar sí se hunde, aunque aquí la causa fue vaciar el yacimiento: **no es nuestro caso, pero el cajón apoya en sedimento** |

También se revisa poco en la realidad: en el SFR, deformación y agua **una vez al año**, inspección mayor cada 5 años, bulones cada 6, saneo cada 5. Aquí un día son 150 s y el
juego no llega a un año: las frecuencias se comprimen.

### Cuánto aplica el miedo
| Si la roca fuera… | Cierre | La holgura de 3 m dura | ¿Sirve el margen? |
|---|---|---|---|
| **Cristalina, como Forsmark (el supuesto)** | ~0,03 mm/año | **~100 000 años** | Sobra; el peligro es otro |
| Sal, como WIPP | ~100 mm/año | **~30 años** | **Sí: es justo lo que pedía Berón** |
| Sal en crisis, como el Panel 7 | ~900 mm/año | ~3 años | Sobre el papel, pero el techo cayó en un mes |
| Sal honda, primeros años | ~600 mm/año | ~5 años | Poco |

**Y en roca dura el margen tiene un límite, pero es de LUZ LIBRE, no de tamaño:** más luz sin apoyo es más techo sin sostener. Con columnas y techo reforzado la sala puede ser grande: **§10 corrige lo que aquí se decía de «más grande no es más seguro»**.

**Lo que de verdad amenaza esta roca** (razonamiento mío, salvo lo citado): una cuña entre fracturas (el túnel de Forsmark 3 tuvo caídas al cruzar la zona de Singö), el agua, y una
falla que nadie vio. Sin estudiar la roca antes, una zona así puede estar justo donde va la caverna. El calor del reactor también carga la roca; aquí no se calcula.

### La decisión
- **Holgura declarada y medida: ≥ 3 m** entre el forro y el equipo, por los lados y por arriba (`CavernRule.ClearanceM`; la mide el constructor y da error si no llega). El reactor ya tenía 3,7 m y los **residuos, 1,4 m**. *(Hoy las salas son de 44 × 40 m y la holgura se mide también a las columnas: 3,5 m. §10.)*
- **Pilar de roca** entre salas de **24 m** (§10; antes, 14 y 18): no menor que la luz libre ni que medio ancho de sala (regla práctica, **de memoria**).
- **Instrumentos** (`CavernRock`): en cada caverna, 4 extensómetros de varilla, 4 dianas de convergencia, 3 células de carga en bulones, 2 geófonos, 2 piezómetros y 1 aforo del sumidero; abajo, un registrador con fibra hasta el **panel de la roca** en la Central.
- **El cajón del TRIGA:** 4 células en sus tendones y 2 inclinómetros (§9; había pilotes e hitos de asiento cuando apoyaba en el sedimento).
- **Niveles** (`CavernRule.Grade`). **Los números son del juego, no de una norma**: no encontré umbrales publicados, cada proyecto fija los suyos.

| Nivel | Convergencia | Ritmo | Agua | Qué se hace |
|---|---|---|---|---|
| **Normal** | < 1 mm | < 0,1 mm/día | < 2 veces | Se lee a diario (`nuc_roca`, 1 h); una bajada a la semana (`nuc_bajada`, 2 h) |
| **Aviso** | ≥ 1 mm | ≥ 0,1 mm/día | ≥ 2 veces | **Bajada obligatoria**: mirar, sanear y reforzar. La roca para; lo gastado no vuelve |
| **Alarma** | ≥ 5 mm | ≥ 1 mm/día | ≥ 5 veces | **Nadie baja** (el carro que iba bajando vuelve) y se refuerza a distancia (`nuc_refuerzo`, obligatoria). A los 3 días sin reforzar, **emergencia del sitio** |

- **Por qué manda el ritmo:** en WIPP lo que avisó fue la aceleración, no el total.
- **Episodios:** alrededor de uno cada 90 días, nunca antes del día 30, fijos para cada día: una cuña (el ritmo se dobla cada día) o agua (sube un 50 % al día). Sin leer, nadie se entera.

## 8. Un acceso rápido al fondo (Berón 2026-11-05)

*(Superada por §9: con el mar a 500 m ya no cabe un funicular. Queda como razonamiento: Kaprun, la segunda salida y el descarte del tubo desde el santuario siguen valiendo; las cifras, no.)*

Berón: «se podría crear un tunnel tren acuatico para llegar al fondo marino super rápido?»

**Sí, pero «súper rápido» aquí es poco: son 82 m de desnivel, y lo que tarda son las puertas, no el vehículo.** La galería queda a **unos 53 s** desde el andén: ~23 s de carro y 30 de esclusas.
Lo que decide la forma no es la velocidad, sino **qué cabe por ahí y por dónde se sale si falla**.

### Cómo se compara
*(Tiempos con 15 s de esclusa a cada lado, que es un valor del juego; recorridos de la escena: la longitud exacta sale al construir, 115–140 m según el lado en que caiga el piso.)*

| | **Funicular sellado (elegido)** | Ascensor rápido | Tubo desde el santuario |
|---|---|---|---|
| Recorrido | ~115–140 m al 60–105 % | 82 m | ~700 m bajo el mar |
| Un viaje | **~53 s** | ~48 s | ~110 s; la alfombra tarda ~43 s |
| Lleva el módulo de 10 m y los contenedores | **Sí, sobre raíles** | No: una jaula no; el pozo actual, de 2,4 m, tampoco | Sí |
| Si se va la luz | Hay un tubo por el que salir | Queda el pozo | Lejísimos de todo |
| Peor defecto | **Tiro de chimenea si hay fuego** (Kaprun) | Un solo hueco vertical | **Un agujero recto de los reactores a la gente** |

- **El ascensor gana 5 s, y no compensa.** §4 y §6 prometen que todo se puede sacar, y por un pozo de 2,4 m no sale un módulo de 10 × 3 × 3,2 m. Esa promesa era falsa hasta ahora: **el funicular la arregla**. El  SFR, de hecho, se sirve por dos túneles y no por un pozo (no encontré si fue por eso).
- **El tubo desde el santuario se descarta**, aunque es lo que más se parece a la idea: no ahorra tiempo y une el reactor con el santuario por dentro, contra toda la distancia que el sitio pagó.
- **El tramo de 16 m de mar sí es «tren acuático»:** de cristal, se ve el agua. Lo demás va revestido por la roca.

### Kaprun: la advertencia
Un funicular en túnel mató a **155 personas** en 2000: un calefactor doméstico encendió aceite hidráulico y el túnel inclinado hizo de chimenea; una puerta abierta arriba empeoró el tiro. No había
detectores de humo, y se daba por imposible un incendio. Por eso aquí: **sin aceite ni calefactor en el carro** y **el motor fuera del tubo** (cabrestante eléctrico en la caseta), **esclusas cerradas en las dos
estaciones**, y **una segunda salida que no es el tubo**. *(El juego no simula fuego ni humo: `OutOfService` representa cerrar la línea.)*

### La decisión
- **Trazado:** del **borde del casquete**, a lo largo de X, hasta la **galería entre las cavernas**, siempre por el pilar de roca: en Z entraría por el techo de una caverna o pasaría por encima del pozo del reactor. Sale por el borde más lejano
  para ir lo más tendido posible.
- **Cabina nivelada**, como en Stoos (110 %) o Gelmerbahn (106 %), porque según el lado la pendiente ronda el 100 %. A 10 m/s, por debajo de los 12 del Sunnegga.
- **Con las alfombras:** a la estación se llega en alfombra, porque es **una pista más de la red del edificio** (`Línea de las cavernas`), a unos 8 m del andén. No hizo falta otra red.
- **Segunda salida:** **pozo propio** con **jaula a baterías** (3 m/s), siguiendo el pilar hacia el lado contrario al tubo, hasta su tapa en el casquete: 45–60 s desde la galería. **No usa el pozo del reactor**: con Lucens (§3), la sala de control se contaminó a 100 m por no estar bien sellada, y una salida no puede pasar por lo que se está huyendo. En Äspö se valora igual tener rampa y pozo.
- **En la práctica:** el carro lleva de uno en uno y **se vuelve desde donde esté** si hay emergencia o alarma. Con la línea cerrada, quien está abajo sale por el pozo.
- **Nadie anda por las cavernas:** siguen sin colisiones. El viaje va del andén a la galería y vuelta, y al jugador se le ofrece así al pisar el andén.

### Con la jornada
Una hora de juego son 150 s.

| Tramo | Tiempo real |
|---|---|
| Del santuario a la estación (`TripSeconds`, 85 s + d/18) | ~125 s |
| Andén → galería | ~53 s |
| Inspección abajo | 90 s |
| Galería → andén | ~53 s |
| **Bajada completa, ida y vuelta en línea** | **~196 s de los 300 de su misión de 2 h** |

La misión aguanta llegar con media alfombra de retraso (~258 s). Son **unas 9 h más de trabajo a la semana** (7 de lectura y 2 de bajada), repartidas como cualquier otra misión.

### Lo que no sé
- **Todo esto se compiló, pero no se ha probado en vivo:** ni las longitudes exactas, ni la pendiente real, ni si un cuerpo se ve bien en el carro.
- **La roca es un supuesto.** En la realidad, antes de excavar habría que estudiar el macizo, y lo primero que miraría serían las fallas.
- **Nada de esto se ha construido junto:** ningún reactor tiene funicular bajo el mar. Son precedentes por separado.
- **Los 15 s de esclusa, los umbrales y la holgura** son decisiones de juego.
- **El estado de la roca no se guarda** en la partida: al cargar vuelve a normal.

### Fuentes de §7–§8
- [SKB R-07-10: experiencia de obra y de explotación del SFR](https://skb.com/publication/1363598/R-07-10.pdf) (la fuente principal: deformación, bulones, agua e inspecciones)
- [Gjøvik: Vik et al., 1993](https://koreascience.or.kr/article/CFKO199311920450141.page?lang=en), [Wikipedia](https://en.wikipedia.org/wiki/Gj%C3%B8vik_Olympic_Cavern_Hall)
- WIPP: [Appendix PCS B](https://wipp.energy.gov/library/cra/CRA-2014/References/CCA/Appendix_PCS_B.PDF), [DOE, 2016](https://wipp.energy.gov/images/wipprecovery/Presentations/Town_Hall_Slides_10_13_16.pdf), [Nuclear Watch New Mexico sobre el informe de MSHA](https://nuclearactive.org/rapid-seven-fold-never-been-observed-at-wipp-increases-in-salt-convergence-in-panel-7-where-waste-is-planned-for-disposal/) (secundaria y crítica)
- [Sal honda: Bieniasz et al., 2011](https://yadda.icm.edu.pl/baztech/element/bwmeta1.element.baztech-article-AGHL-0003-0001/c/Bieniasz.pdf) (solo el resumen de la búsqueda: el PDF no se dejó abrir)
- [Boulby: compresión del terreno](https://journals.sagepub.com/doi/10.1179/1743286313Y.0000000045) y [tesis de Newcastle](https://dspace.ncl.ac.uk/jspui/bitstream/10443/1645/1/Miller%20H.D.S.%2080.pdf) (no se pudo abrir: demasiado grande)
- Ekofisk: [Museo del Petróleo de Noruega](https://ekofisk.industriminne.no/en/jacking-provides-the-solution/), [Wikipedia](https://en.wikipedia.org/wiki/Ekofisk_oil_field)
- Olkiluoto: [vigilancia de la roca, 2012](https://inis.iaea.org/records/tt1d5-axy41) (0,6 mm verticales sobre el repositorio)
- [Kaprun](https://en.wikipedia.org/wiki/Kaprun_disaster), [Stoos (Guinness)](https://www.guinnessworldrecords.com/world-records/553751-steepest-funicular), [Gelmerbahn](https://www.wikipedia.com/wiki/Gelmerbahn), [Sunnegga](https://en.wikipedia.org/wiki/Zermatt%E2%80%93Sunnegga_Funicular)
- [Äspö: evacuación con pozo y rampa](https://www.nap.edu/read/4897/chapter/11), [30 CFR 57.19071: velocidad de izado de personas](https://www.govinfo.gov/content/pkg/CFR-2025-title30-vol1/pdf/CFR-2025-title30-vol1-sec57-19071.pdf)

## 9. Un mar de tamaño real, y todo en menos de un minuto (Berón 2026-11-05)

Berón: «haz dicho que el fondo del mar está a 90m? se debe agregar un tamaño mas real que junto al tunnel de super velocidad no supere el minuto?»

**Aclaración.** Los 90 m no eran el mar: eran la bajada hasta la caverna. El mar de la escena tenía **16 m**, un charco para el mar de Ross. **Decisión: 500 m en todo el mapa, y el viaje entero sale en ~58 s.** Cabe, pero **justo**, y solo con cifras de récord.

### Por qué 500 m
La plataforma del mar de Ross tiene **más de 500 m de media, de menos de 300 a más de 1200**; la media mundial de las plataformas es de unos 130 m. Es lo más realista que cabe en el minuto, y tiene otra ventaja:
las marcas de quillas de iceberg **actuales** llegan a unos **400 m** (Prydz Bay; no encontré la cifra del propio mar de Ross), así que el lecho a 500 m queda por debajo. **Lo que cruza el agua, no:** ver «Lo que no resuelve».

**Todo el mapa, no solo el sitio.** Cambiar la hondura solo alrededor del sitio pedía un escalón en el fondo y dos volúmenes de agua; todo el mapa es **un número** (`SeaRule.Depth`). Revisado lo que depende de él:

| Qué | ¿Depende? | Qué se hizo |
|---|---|---|
| `Sea_SwimZone`, `FondoMarino` | Sí | Bajan a −500 m |
| `Limite_del_mar` (paredes) | **Sí**: acababan en −20 y se habría podido pasar nadando por debajo | Bajan al lecho |
| `Mar_FondoProfundo` (cuenco oscuro) | **No se toca**: es visual. A 500 m la niebla volvería pálido todo el mar | Sigue a −16 m |
| Nado (`SwimBody.Probe`, rayo de 301 m) | No: sin fondo, «hondura 999» | Nada |
| Andar y orillas (`PathMover.GroundY`) | No: ignora lo que quede bajo el agua | Nada |
| Máscaras, respiración | No: miran el volumen de agua | Nada |
| Ballenas, orcas, focas, peces, kril | No: nadan cerca de la superficie (`SwimY` −1 a −3,5) | Nada: **no bucean más** |
| Bancos de peces de los troncos | No: dentro de la columna de agua de cada árbol | Nada |
| Submarino de obra (−12), ambulancia, Leviatán | No: rutas a su hondura; ningún rayo necesita el fondo | Nada |
| Troncos abiertos al mar | No: su agua empieza en −1,6 m | Nada |
| `MarineLife.UnderTree` | Decía −16 a mano | Usa `SeaRule.FloorY` |
| Cajón del TRIGA | **Sí**: ya no llega al lecho | **Cuelga** (abajo) |
| Intercambiador del lecho, bucle, pozos | Se recalculan solos | Siguen; el reactor sigue más abajo que su sumidero |
| Casquetes artificiales (nuclear y lanzadera) | **Sí**: ya nada los sujetaría | **4 tendones al lecho** cada uno |
| Agua en las salas | **Sí**: ~5 veces más carga | Agua de fondo de 12 a 60 l/min (ley de Darcy; estimación mía) |
| `JornadaTest` | **Fallo mío de antes**: «bajo el mundo» por debajo de −15, y ya se bajaba a −82 | No cuenta a quien va en la línea |
| Laguna de la escena del ciclo de vida | Tiene su propio fondo | No es este mar: se queda |

### El cajón del TRIGA vuelve a colgar
Un núcleo a −7 m no puede apoyar en un lecho a −500. Vuelve a §3: **cuelga del casquete, atado al lecho con 4 tendones**, como una plataforma de patas tensadas; las hay a **1580 m** (Big Foot) y 1432 m (Magnolia), bien por encima de lo que se pide aquí.
Sale ganando: **ya no le afecta el asiento del sedimento**, y otra vez se puede remolcar. Los pilotes y los hitos de asiento (§7) se sustituyen por células en los tendones.

### El tiempo, con números
El recorrido son **566 m de desnivel** (500 de mar, 60 de roca y la altura de la sala).

| | Cifra | Precedente |
|---|---|---|
| Velocidad | **20 m/s** | Récord de pasajeros: 20,5 m/s (Shanghái); 16,8 en Taipei 101. Un ingeniero estima el tope en ~24. Pozos de mina más hondos: 19–20 m/s |
| Aceleración | **1,5 m/s²** | Guía de ascensores rápidos: ≤ 1–1,5 m/s²; es **el tope**. Se cita ~2 m/s² como límite sin mareo para Hyperloop, que sigue sin existir con pasajeros |
| Esclusas | **8 + 8 s** | Valor del juego: el carro es estanco y no hay que igualar presión. Antes, 15 + 15 |
| Carro | 28,8 + 13,3 s | |
| **Total** | **~58 s** | Berón pedía < 60 |

**Se cumple con 2 s de margen.** Pero no es holgado: una rampa de sobreaceleración de 2,5 m/s³ suma ~0,6 s; con las esclusas de antes serían 72 s; y a 15 m/s y 1 m/s² (cómodo), 69 s. **Una instalación real andaría por 70–75 s** (estimación mía).

| Mar | Viaje |
|---|---|
| 200 m | ~43 s |
| 300 m | ~48 s |
| **500 m** | **~58 s** |
| ~540 m | 60 s (el límite con estas cifras) |
| 880 m | ~77 s |
| 1200 m | ~93 s |

**Los oídos:** al bajar 566 m de aire la presión sube ~67 mb. Repartidos en el viaje son **1,15 mb/s**, justo por debajo de los **1,26 mb/s** de Taipei 101, que ya controla la presión de cabina con soplantes; por eso aquí también hace falta. Las guías piden ≤ 20 mb sin control y avisan por encima de 7 m/s y 300 m: este viaje pasa de las dos. Esas soplantes no están simuladas.

### Por qué ya no es un funicular
A la pendiente máxima (110 %) harían falta **más de 500 m de recorrido horizontal**, y el casquete mide 170. Sale casi vertical, **unos 10° de la vertical**, con la cabina nivelada: del borde del casquete (lejos de pisos y tronco) a la galería por el pilar. Un ascensor a plomo solo ahorra ~0,5 s. El tubo tiene **5,2 m de hueco**: el módulo (3 × 3,2 m de sección) sigue cabiendo a lo largo, que es lo que justificaba la línea.
El tubo es **de cristal solo en los primeros 16 m**; más abajo son hasta **50 bar**, y va de acero hasta el lecho. Sigue siendo un «tren acuático» donde se ve.

**La salida de escape** no entra en el minuto: es otro pozo, con jaula a baterías a 6 m/s, **unos 2 min**. Abajo, el refugio de §12.

**Con la jornada:** ida y vuelta ~206 s de los 300 de su misión de 2 h, o ~268 s con media alfombra de retraso. Cabe.

### Lo que no resuelve
- **Las quillas de iceberg:** los tubos cruzan los 400 m de agua a los que los icebergs actuales sí pueden llegar. Sigue siendo **el riesgo principal de esta decisión**: §14 dice qué se hizo y qué no se puede hacer.
- **Nadie ha hecho esto:** no encontré ningún tubo de 500 m de mar con pasajeros. Los túneles bajo el mar son otra cosa: van por la roca.
- **Los árboles sobre las islas:** el santuario está sobre hielo que ya no toca fondo. Berón decidió que sea una plataforma flotante: §13.
- **La fauna no aprovecha el mar:** nada nuevo bucea. Si se quiere que las ballenas bajen, es trabajo aparte.

## 10. Columnas y techo: grande y espacioso (Berón 2026-11-05)

Berón: «en la edificación se pueden poner columnas y techo con lo que es mas fácil crearlo grande y espacioso no?»

**Sí, y corrijo §7:** dije que más grande no era más seguro. **Eso vale para la luz sin apoyo.** Lo que limita no es el tamaño de la sala, sino **cuánto techo queda libre entre apoyos**, y eso se arregla con columnas y refuerzo.

### Lo investigado
- **Cámaras y pilares:** así se explotan minas enteras: se deja roca en pie como columnas y se saca del **60 al 75 %**, hasta 70–85 % en Elliot Lake.
- **Luz libre:** en un ejemplo de diseño en roca dura, una gráfica dio **~14 m** sin sostener, y el proyecto adoptó 10.
- **Gjøvik:** 61 m **con** bulones y cables. El refuerzo puede más que las columnas si la roca acompaña.
- **El aviso:** en una mina surafricana de roca dura, con pilares de 8,5 m y salas de 9,7, **fallaron cuatro pilares y siguieron casi 100**. Y **no hay un método aceptado** para dimensionar pilares en roca dura: por eso aquí son generosos y se vigilan.

### La decisión
- **Dos salas de 44 × 40 m** (antes, 20 × 12 y 24 × 16): **más de siete veces la planta** de la del reactor.
- **4 columnas de roca sin excavar**, de 4 × 4 m (no de hormigón: es roca que se deja), a 18 m entre ejes.
- **Luz libre máxima: 14 m**, en todas las direcciones (`CavernRule.MaxFreeSpan`).
- **Techo:** bulones cada 6 m y 4 cables más largos sobre la luz central, además del forro que ya había.
- **Vigilancia:** una célula de carga en cada columna, porque si un pilar falla, carga a los de al lado.
- **Pilar entre salas: 24 m** (`CavernRule.PillarM`): no menor que la luz libre ni que medio ancho de sala (regla práctica, de memoria).
- **Holgura ≥ 3 m:** ahora también a las columnas. Sale a **3,5 m**; los contenedores se acercaron 0,5 m al eje para respetarla.

La sala sigue sin ser para pasear: **no se anda por ella** (no tiene colisiones). El espacio es para el equipo y su margen.

## 11. La revisión que pide cambiar una regla (Berón 2026-11-05)

Regla de Berón: cambiar una regla obliga a revisar lo que ya existe. Lo que tocan estas decisiones:

| Regla o pieza | Antes | Ahora |
|---|---|---|
| Hondura del mar | 16 m | 500 m (`SeaRule`, regla T19); tabla de §9 |
| T14 | Cajón apoyado en el lecho | Colgado, con tendones |
| T16 | «Más luz no es más seguro» | Luz libre máxima, columnas y refuerzo |
| T17 | Funicular al 60–105 %, ~53 s | Tubo a ~10°, 20 m/s, 1,5 m/s², **< 60 s por regla** |
| Casquete de la lanzadera (T13) | Sin sujeción | 4 tendones; **su altura y sus distancias no cambian** |
| Tubo de cristal | Todo el mar | Solo 16 m: abajo va acero |
| Agua de fondo | 12 l/min | 60 l/min; los umbrales son proporciones y no cambian |
| Pasos del jugador y misiones | — | Sin cambios |
| §4, §7, §8 | — | Corregidos o marcados como superados |
| Documentos sueltos | 16 m en máscaras; «apoyado en el lecho» en la planta | Corregidos |

**Sigue aquí como historia:** §3 y §8 razonan con otras cifras, a propósito.

### Fuentes de §9–§11
- Mar de Ross: [Springer: la plataforma](https://rd.springer.com/chapter/10.1007/978-94-011-5820-6_16), [Wikiversity](https://en.wikiversity.org/wiki/Continental_shelves/Antarctic)
- Icebergs: [Cambridge: quillas y marcas](https://www.repository.cam.ac.uk/handle/1810/276147), [AGU 2015](https://agu.confex.com/agu/fm15/webprogram/Paper85172.html)
- Patas tensadas: [Wikipedia](https://en.wikipedia.org/wiki/Tension-leg_platform), [Magnolia](https://en.wikipedia.org/wiki/Magnolia_(oil_platform))
- Ascensores: [Mitsubishi Electric, 2016](https://mitsubishielectric.com/news/2016/0510.html), [CTBUH](https://www.bdcnetwork.com/building-sector-reports/high-rise-construction/news/55161204/ctbuh-determines-fastest-elevators-and-longest-runs-in-the-world-in-new-tbin-study), [Design Challenges of High-Speed Elevators](https://elevatorworld.com/?p=36361), [Taipei 101](https://elevatorworld.com/?p=34049)
- Hyperloop: [Big Think](https://bigthink.com/starts-with-a-bang/elon-musk-hyperloop/), [ERA](https://www.era.europa.eu/system/files/2022-10/Hyperloop%20an%20Innovation%20for%20Global%20Transportation.pdf)
- Cámaras y pilares: [NIOSH: pilares en minas de piedra](https://stacks.cdc.gov/view/cdc/9341/cdc_9341_DS1.pdf), [NIOSH: diseño](https://cdc.gov/Niosh/mining/UserFiles/works/pdfs/psadm.pdf), [ejemplo de diseño, 2024](https://scindeks-clanci.ceon.rs/data/pdf/proc-0026/2024/proc-00262401166J.pdf)

## 12. El refugio del fondo (Berón 2026-11-05)

Berón: «se debe crear el refugio del fondo».

**Por qué:** la salida de escape tarda unos 2 min y depende de que su pozo siga entero. Si se pierden la línea y el pozo a la vez, quien esté abajo necesita un sitio donde **esperar**.

### Lo investigado
| | Mina (EE. UU.) | Submarino hundido | Aquí |
|---|---|---|---|
| Aguantar | **96 h** (48 con suministro preparado desde fuera) | Primer rescate en 96 h; se estudian escenarios de 172 h | **168 h (7 días)** |
| Espacio | ≥ 1,4 m² por persona | — | 40 m² para 12: 3,3 m² |
| Oxígeno | ~37 l/h por persona; 18,5–23 % de O₂, CO₂ ≤ 1 % *(fuente secundaria)* | Generar O₂ y quitar CO₂ | 75 m³ en 8 botellas |
| Agua | 2,1 l al día por persona | — | 180 l |
| Suministro desde fuera | Perforación desde superficie | Cápsulas por la esclusa, con buzos o ROV | Por el pozo de escape, si queda algo |

**Por qué 7 días y no 4:** una mina tiene equipos de rescate cerca; aquí hay 566 m entre medias y el rescate tendría que abrir un pozo. La cifra es **decisión mía**, no una norma.

### Cómo queda
- **`Refugio_fondo`**, al pie del pozo de escape, en el pilar de roca y fuera de las dos salas: el camino de salida pasa por la puerta.
- **12 plazas:** el carro lleno y algo más (también decisión mía; en la jornada baja una persona).
- **Dentro:** esclusa, 8 botellas de oxígeno, depósito de agua, depurador de CO₂, baterías, bancos y **dos comunicaciones**: teléfono por fibra y radio a través de la roca.
- **Mantenimiento:** se revisa cada semana (`nuc_refugio`, 2 h, por la línea). Sin revisar pierde un 2 % al día; **por debajo del 80 % no se baja a trabajar** y la revisión pasa a ser obligatoria, como en las minas.
- **En juego:** con la línea y el pozo cerrados, quien está abajo espera aquí gastando aire y sale en cuanto se abre uno. Si se acaba el aire, el rescate se da por hecho: la espera es finita, pero **ese rescate no se simula**.

## 13. El hielo es una plataforma flotante (Berón 2026-11-05)

Berón: «hazlo real».

**Hecho: cada casquete de hielo mide 300 m de grosor** (`SeaRule.ShelfThickness`, antes 1,5), con 200 m de mar debajo. La isla de tierra no: su placa sigue en 1,5 m y debajo va la roca hasta el lecho ([territorio-polar.md](territorio-polar.md)). **Lo que no es real: el hielo sigue asomando 25 cm.**

### Lo investigado
- **Plataforma de Ross:** varios cientos de metros; unos 330 m hacia 79°S y más de 700 tierra adentro *(fuente floja)*; Scott calculó unos 274. Frente de 15–50 m sobre el agua, y ~90 % bajo ella.
- **El matiz:** en McMurdo el frente se adelgaza hasta unos 3 m (medido en 1957–58). Una plataforma puede ser fina en su borde.
- **Sobre el hielo se construye así:** Halley VI va sobre patas hidráulicas con esquís, deriva ~500 m al año y hubo que moverla por una grieta. Neumayer III sube con la nieve.

**Por qué 300:** es lo que calan los icebergs de estas plataformas (250–300 m), y deja el mar debajo. **Lo que falta:** 300 m de hielo asoman **unos 33 m**, y aquí 0,25 m (con eso flotarían ~2,5 m). Arreglarlo es subir el mundo entero: todos los pisos, alfombras y puertas. **Queda así, a sabiendas.**

### Qué cambia (W11)
| Qué | Efecto | Qué se hizo |
|---|---|---|
| Troncos abiertos al mar | El hueco ya cortaba los bloques: ahora es un **pozo de 300 m** | Automático; la prueba lo comprueba |
| Submarino de obra | Cruzaba a −12, dentro del hielo | **Cruza a −312**: unos 2,5 min más por viaje entre árboles |
| Submarino-ambulancia y Leviatán | Sobre el hielo van en orugas; en el mar, por canales | Nada |
| Fauna en lagunas y canales | Nadan en agua abierta | Nada |
| **Fauna que pasa bajo una placa** | `MarineLife` nadaba a −1,5 m **bajo** la placa: iba **por dentro del hielo** | **Arreglado**: la plataforma es una pared ([hielo-y-respiraderos.md](hielo-y-respiraderos.md)) |
| Nado del jugador | Ya no se pasa bajo las islas: paredes de hielo | Así es el hielo real |
| Respiraderos de focas | Agujeros en 300 m de hielo | **Arreglado**: van en láminas de hielo fijo de 2 m ([hielo-y-respiraderos.md](hielo-y-respiraderos.md)) |
| Laguna del ciclo de vida | Tiene su propia placa | Se queda |
| NavMesh | Cajas de 300 m: **más alto, misma planta** | A vigilar en la primera reconstrucción |
| Islas de tierra | Las lleva la rama de territorio | No tocado |

**Lo que aquí no se simula** (razonamiento mío): una plataforma **se mueve**, y los árboles irían con ella, mientras la isla de roca se queda; **se agrieta y se parte**; y un pozo de 300 m **se volvería a cerrar** si no se calienta. Un árbol de 361 MN sobre hielo flotante pediría cimientos que aquí no hay.

## 14. Icebergs: proteger el tubo y tener alguno (Berón 2026-11-05)

Berón: «el tunnel se puede crear protegido para que los iceberg no afecten? nos convendría crear un iceberg?»

**Respuesta corta. Protegerlo, solo en parte: nada de lo que quepa alrededor de un tubo para un iceberg entero. Lo que protege es verlo venir, desviarlo y, si falla, cerrar y salir. Y sí, conviene tener alguno: son reales y atraen vida.**

### Lo investigado
| Sitio | Qué hace | Lo que enseña |
|---|---|---|
| **Hibernia** | Base de gravedad sobre el fondo, con 16 dientes de hormigón de 1,5 m; diseñada para **6 millones de toneladas** | Funciona porque es **achaparrada y está en 80 m** (dato de memoria), no porque sea un muro: nuestro tubo es lo contrario |
| **Terra Nova** | Pozos en **hoyos** dragados en el fondo; el barco **se desconecta y se va** en ~15 min ante un iceberg de más de 100 000 t, y solo aguanta trozos | Lo que no puede resistir, se esconde o se aparta |
| **Grand Banks** | Radar, y más de 1500 operaciones de remolque registradas: funciona el **85–95 %** | **Puede fallar**; casi siempre, solo se conoce lo que asoma |

### Qué para qué
| Medida | Un trozo (hasta 100 000 t) | Un iceberg de varios millones de toneladas |
|---|---|---|
| Escudo con dientes | **Sí** | **No**: columna esbelta de 400 m. A solo 0,2 m/s, parar 2 Mt en 1 m son ~38 MN a media altura (cálculo mío) |
| Compuerta en el lecho | Sí | **Sí**: si se pierde lo de arriba, lo de abajo sigue seco |
| Hoyo en el lecho | Sí | **Sí, para lo que hay abajo**: un iceberg que arase pasa por encima |
| Radar y sonar | Sí | **Sí**: sin verlo no hay nada más |
| Remolque | Sí | **Sí, el 90 %** |
| Desconectar y salir | Sí | **Sí**: es lo que salva a la gente; el tubo se puede perder |

**Y lo que no protege nada de esto:** el casquete y el árbol. Un iceberg entero que llegue se lleva lo de arriba. Lo que está en la roca y lo que está cerrado sigue ahí.

### Cómo queda
- **`Escudo_tubo` y `Escudo_escape`:** hormigón con dientes de −16 a −400 m, la banda de las quillas actuales.
- **`Compuerta_lecho`** en el tubo y `Collar_lecho` en la roca. **`Intercambiador_lecho` en un hoyo** de 6 m, como en Terra Nova.
- **`Radar_hielo`** y **`Sonar_quillas`** en la estación.
- **Dos icebergs** (`Iceberg`): uno varado en un banco al noroeste (1 Mt) y otro a la deriva por el este (1,9 Mt), a 0,2 m/s, unos 30 m por día de juego. A 500 m de hondo, 240 m de quilla no tocarían fondo: **el banco es una pieza puesta para eso**.
- **A 300 m del sitio:** misión obligatoria de remolque (`nuc_remolque`). **Falla 1 de cada 10**, fijo por día; se repite al día siguiente, más cerca.
- **A 190 m:** se **desconecta la línea**: nadie baja, y quien esté abajo sale por el pozo de escape. Vuelve cuando se aleja.
- **Vida alrededor:** en el mar de Weddell, dos icebergs estudiados tenían más clorofila, kril y aves hasta ~3,7 km, porque sueltan hierro y hacen de refugio. Aquí, un enjambre de kril de verdad por iceberg, peces y animales reales ([vida-en-icebergs.md](vida-en-icebergs.md)); **las focas y los petreles son figuras**, sin IA.
- **Coste para el PC:** unas 15 cajas por iceberg, un enjambre pequeño y un solo giro para las aves; solo se mueve el que deriva.

## 15. La auditoría de estas reglas (W11)

La regla W11 de Berón (cambiar una regla obliga a comprobar lo que ya hay) se aplicó a todo lo anterior: las tablas de §9, §11 y §13 son esa comprobación.

| Regla | Lo existente | Resultado |
|---|---|---|
| T20 refugio | La línea, el pozo y las misiones | Cumplen |
| T21 plataforma | Troncos, submarinos, fauna, nado | **2 sin arreglar**: fauna bajo las placas y respiraderos (§13) |
| T22 icebergs | Sitio y lanzadera | La lanzadera **no tiene** protección contra icebergs: uno que derive hacia el oeste no pediría remolque. Los dos de ahora no van allí |
| Microcosmos | No dependen del mar del santuario | **Ninguna regla nueva** |

### Fuentes de §12–§15
- Refugios: [normas de minas (Mining Monthly)](https://miningmonthly.com/markets/international-coal-news/1305106/refuge-alternatives-final-rules-official), [MineARC](https://minearc.com/?p=10660), [orden de MSHA: agua](https://www.msha.gov/petition-docket-no-m-2024-002-m), [resumen de cifras](https://heavyvehicleinspection.com/industries/mining/mining-equipment-guarding-inspection-compliance) (secundaria)
- Rescate de submarinos: [SRDRS](https://www.globalsecurity.org/military/systems/ship/systems/srdrs.htm), [Navy Medicine: 172 h](https://www.dvidshub.net/news/337024/nmrc-seeks-improve-disabled-submarine-rescue-survival), [Semaphore](https://seapower.navy.gov.au/sites/default/files/documents/Semaphore_2008_7.pdf)
- Plataforma: [Ross Ice Shelf](https://en.wikipedia.org/wiki/Ross_Ice_Shelf), [cerca de Scott Base](https://www.cambridge.org/core/services/aop-cambridge-core/content/view/8D495F8CBF459582040371156FE1775C/S0022143000027234a.pdf/movement-of-the-ross-ice-shelf-near-scott-base.pdf), [Halley VI](https://en.wikipedia.com/wiki/Halley_VI)
- Icebergs: [Hibernia y Hebron](https://explorer.aapg.org/story/articleid/46580/grand-banks-thrives-in-iceberg-alley), [Terra Nova: hoyos y desconexión](https://offshore-mag.com/production/article/16756420/terra-nova-system-designed-for-quick-release-iceberg-scouring), [POAC 2025: remolque](https://www.poac.com/Papers/2025/pdf/POAC25_paper_149.pdf), [30 años de gestión de icebergs](https://openpolar.no/Record/ftnrccanada:oai:cisti-icist.nrc-cnrc.ca:cistinparc:12327552), [C-CORE SIMS](https://c-core.ca/wp-content/uploads/sites/5/2020/10/SIMS-article-in-JOT-Oct2020.pdf)
- Vida: [Smith et al., 2007](https://data-ww3.ifremer.fr/BIB/Smith_etal_SCIENCE2007.pdf), [Vernet et al., 2012](https://tos.org/oceanography/assets/docs/25-3_vernet.pdf)

## Fuentes
- [10 CFR 100.11](https://www.law.cornell.edu/cfr/text/10/100.11), [10 CFR 100.3](https://www.law.cornell.edu/cfr/text/10/100.3), [10 CFR 53.530](https://www.law.cornell.edu/cfr/text/10/53.530)
- [NRC: emergencias en reactores de investigación](https://www.nrc.gov/about-nrc/emerg-preparedness/about-emerg-preparedness/research-test.html); [archivo de esa página](https://webarchive.library.unt.edu/eot2008/20080916122805/http:/www.nrc.gov/about-nrc/emerg-preparedness/protect-public/research-test.html)
- [Morgan Lewis: la regla de la NRC para SMR y reactores avanzados (10 CFR 50.160)](https://morganlewis.com/blogs/upandatom/2023/11/nrc-provides-emergency-preparedness-flexibility-for-smrs-and-advanced-reactors)
- TRIGA: [SAR de K-State](https://www.nrc.gov/docs/ML1731/ML17319A307.pdf), [NETL PSAR](https://nuclear-twins.tacc.utexas.edu/static/document/NETL%20PSAR.pdf), [TRIGA Mk II LOCA](https://www.trtr.org/wp-content/uploads/2017/11/TRIGA-mkII-Loss-of-Coolant-Accident-1.pdf), [UC Davis, NRC](https://www.nrc.gov/docs/ML2009/ML20097C651.pdf)
- eVinci: [Westinghouse en el IAEA](https://conferences.iaea.org/event/374/contributions/31535/attachments/18122/30561/TrackB.7_331_spalding_presentation.pdf), [taller de ASME](https://resources.asme.org/hubfs/CS-Codes-Standards/CS-Section%20III%20Div%205%20HTR%20V%20Workshop%20Files/HTR%20Presentations/HTR%20Day%201/14.%20WEC-eVinci.pdf)
- AP1000: [NRC](https://www.nrc.gov/docs/ML1122/ML11221A070.pdf), [Westinghouse: apagón total](https://westinghousenuclear.com/media/3pipeoof/ap1000-station-blackout.pdf)
- PM-3A: [Polar Record, Cambridge](https://www.cambridge.org/core/product/A35009D4403A289EF51E5F61EE54CFF4/core-reader), [Bellona](https://bellona.org/?p=8558)
- Dispersión: [Turner, *Workbook of Atmospheric Dispersion Estimates*](https://archive.org/download/in.ernet.dli.2015.165670/2015.165670.Workbook-Of-Atmospheric-Dispersion-Estimeates_text.pdf)
