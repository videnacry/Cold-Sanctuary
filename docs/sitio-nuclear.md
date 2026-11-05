# El sitio nuclear (investigación → decisión → construcción, Berón 2026-11-05)

Pedido de Berón: «la planta atómica sigue usando el antiguo modelo de edificio? se puede migrar para quedar como el lanza cohetes? en su
propio edificio-árbol en un casquete artificial y a gran altura con medidas de seguridad para tener el menor impacto posible en el
santuario en caso de fallos?»

**Respuesta corta.** Ya eran pisos de árbol (la Planta Atómica) o un piso aparte (la Central Nuclear), pero al lado de la gente. Se mudan
a su propio casquete artificial y a su propio árbol, como la lanzadera; **pero no a gran altura**. Lo que protege de un reactor es la
**distancia, la contención y el agua**, no estar en alto. Los reactores van **abajo** y lo que sube es lo que sí gana con la altura: el
agua de emergencia (por gravedad), la chimenea filtrada y las salas de mando. **Actualizado:** los núcleos van además bajo el mar, en cajones sumergidos (§3).

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

## Fuentes
- [10 CFR 100.11](https://www.law.cornell.edu/cfr/text/10/100.11), [10 CFR 100.3](https://www.law.cornell.edu/cfr/text/10/100.3), [10 CFR 53.530](https://www.law.cornell.edu/cfr/text/10/53.530)
- [NRC: emergencias en reactores de investigación](https://www.nrc.gov/about-nrc/emerg-preparedness/about-emerg-preparedness/research-test.html); [archivo de esa página](https://webarchive.library.unt.edu/eot2008/20080916122805/http:/www.nrc.gov/about-nrc/emerg-preparedness/protect-public/research-test.html)
- [Morgan Lewis: la regla de la NRC para SMR y reactores avanzados (10 CFR 50.160)](https://morganlewis.com/blogs/upandatom/2023/11/nrc-provides-emergency-preparedness-flexibility-for-smrs-and-advanced-reactors)
- TRIGA: [SAR de K-State](https://www.nrc.gov/docs/ML1731/ML17319A307.pdf), [NETL PSAR](https://nuclear-twins.tacc.utexas.edu/static/document/NETL%20PSAR.pdf), [TRIGA Mk II LOCA](https://www.trtr.org/wp-content/uploads/2017/11/TRIGA-mkII-Loss-of-Coolant-Accident-1.pdf), [UC Davis, NRC](https://www.nrc.gov/docs/ML2009/ML20097C651.pdf)
- eVinci: [Westinghouse en el IAEA](https://conferences.iaea.org/event/374/contributions/31535/attachments/18122/30561/TrackB.7_331_spalding_presentation.pdf), [taller de ASME](https://resources.asme.org/hubfs/CS-Codes-Standards/CS-Section%20III%20Div%205%20HTR%20V%20Workshop%20Files/HTR%20Presentations/HTR%20Day%201/14.%20WEC-eVinci.pdf)
- AP1000: [NRC](https://www.nrc.gov/docs/ML1122/ML11221A070.pdf), [Westinghouse: apagón total](https://westinghousenuclear.com/media/3pipeoof/ap1000-station-blackout.pdf)
- PM-3A: [Polar Record, Cambridge](https://www.cambridge.org/core/product/A35009D4403A289EF51E5F61EE54CFF4/core-reader), [Bellona](https://bellona.org/?p=8558)
- Dispersión: [Turner, *Workbook of Atmospheric Dispersion Estimates*](https://archive.org/download/in.ernet.dli.2015.165670/2015.165670.Workbook-Of-Atmospheric-Dispersion-Estimeates_text.pdf)
