# Los pueblos de los microcosmos desde su mapa real (2026-11-03)

Berón: «sería ideal acceder a mapas de esos pueblos, de modo que se amplía su estructura y así entran actividades del santuario …
y estructuras naturales como ríos, lagos, montañas, etc., con su estilo de la época-lugar y adaptación para humanos-insectos y
humanos».

## El tamaño elegido

- Los insectos-humanos miden como una persona (1,6–1,85 m), así que **un pueblo real cabe a escala 1:1**: 1 m del plano = 1 m del
  microcosmos.
- El **barrio del pueblo** pasa de ser el piso real × 10 (240–460 m) a **× 60** (`AreaMicrocosmos.TownScale`): el **60 % del
  lado del piso ×100**, de **1,4 a 2,8 km** según el piso (24–46 m), centrado en la plaza de la historia y dentro de los muros.
  El 40 % que queda alrededor es el santuario gigante (sus muebles, mesetas de 75 m), que también se puebla (reglas de los
  microcosmos, [microcosmos-areas.md](microcosmos-areas.md#reglas-de-los-microcosmos-2026-11-03)).
- El plano va **1:1 si cabe** lo construido (calles, murallas, edificios); si no, se encoge entero lo justo y se dice en el
  registro (`[Pueblos] X: plano a escala 1:N`). Ríos, lagos, montes, campos y el mar se **recortan** en el borde del barrio.
- **Rendimiento**: piezas de pocos cubos; las calles y los ríos en tramos de 40–60 m (una caja cada uno), lagos de un cilindro
  aplanado, colinas y montes de una esfera medio enterrada con collider de malla; **como mucho 150 edificios por pueblo**
  (historia + pueblo + relleno); la flora del pueblo con rejilla proporcional (≤ ~28 000 celdas) y celdas de 12 m para no
  amontonarse. Recuento por escena en [§Recuento](#recuento).

## Cómo se construye (`Assets/Editor/CivicBuilder.Plans.cs`)

Cada pueblo tiene su plano en texto (`PlanText`), una línea por elemento, en metros reales con **el claro de la historia en (0, 0)**,
x = este, z = norte:

```
calle Nombre | ancho | x z, x z…      rio / canal Nombre | ancho | x z…      muralla Nombre | 0 | x z…
lago Nombre | rx rz | x z             colina / monte Nombre | radio alto | x z   campo / mar Nombre | ancho fondo | x z
hito i | x z        (el edificio i de la historia)
Tipo | x z          (un edificio del pueblo: Plaza, Mercado, Estudio, Estacion, Policia, Bomberos, Museo, Piscina, Hotel…)
relleno | n | x z   (n casas y talleres de la era a lo largo de las calles, empezando cerca de x z)
```

1. `BuildPlan` levanta el **paisaje** («Paisaje de X»: ríos y canales de agua, lagos, colinas y montes caminables, campos, el mar,
   el cimiento de las murallas) y las **calles** («Calles de X (plano)») ANTES que los edificios: los solares (`Lots`) los
   esquivan tramo a tramo, y la flora del pueblo también (`AreaMicrocosmos.InTown` / `TownFootprints`).
2. Los edificios de la historia van a su **hito** del plano (el solar libre más cercano: `Lots.Take`, ahora en anillos y con un
   índice de celdas de 40 m).
3. `PlanCivic`: los **edificios del pueblo** de la lista, cada uno con el nombre de su época y lugar (`MicroStyles.CivicName`; los
   que no existían no se levantan y se dice cuáles), sus puestos interactuables, sus muebles para personas e insectos-humanos y sus
   etiquetas de actividad (reglas 8–12 de [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md)).
4. `PlanFill`: **las casas y los talleres a lo largo de las calles**, cada 16 m por cada lado, de frente a la calle (el lado de uso
   de sus muebles mira a ella), empezando por los más cercanos al centro del pueblo. Cada casa con su frutal.
5. Los edificios de varias plantas van en **terraza** (`Storeys`, regla 12). Sobre los muebles gigantes de fuera, **miradores**.

## Los planos (fuentes)

| Pueblo | Lo que tiene su plano | Fuentes |
|---|---|---|
| **Crotona** (−530) | retícula de plateiai E-O y stenopoi; el río Esaro; el mar Jonio; la acrópolis del puerto; la Sila; el templo de Hera Lacinia en el cabo | Spadea 1997, *Crotone*; Wikipedia «Crotone»; Burkert 1972 |
| **Menfis** (−450) | el Nilo y el canal del puerto; el recinto de Ptah con su lago sagrado; el coloso de Ramsés II; campos de la crecida; la meseta de Saqqara | Jeffreys, *The Survey of Memphis* (EES); Heródoto II.99–101 |
| **Bagdad** (~820) | la Ciudad Redonda (su muralla a ras de suelo) y sus cuatro avenidas a las puertas; el Tigris y el puente de barcas; al-Karj entre los canales Sarat e Isa | Lassner 1970, *The Topography of Baghdad*; Le Strange 1900 |
| **Woolsthorpe** (1666) | la casa, el granero y el huerto; Water Lane; el río Witham; Colsterworth con su iglesia y la Great North Road; los campos abiertos | National Trust; OS 1:2500 de 1887 |
| **Berkeley** (1796) | High Street, Canonbury y Salter Street; la iglesia y The Chantry; el castillo; el Little Avon; el estuario del Severn | Jenner Museum; *VCH Gloucestershire* vol. 5 |
| **Angostura** (1819) | la cuadrícula colonial, la Plaza Mayor, el Paseo Orinoco; el Orinoco en su estrechura y la Piedra del Medio; el cerro El Zamuro | plano de Angostura de 1819 (AGI); Lynch 2006 |
| **Sils Maria** (1881) | la Via da Marias; los lagos de Sils y Silvaplana; el arroyo de Fex; la península de Chastè; el Corvatsch y el Lagrev | Siegfriedkarte 1:50 000, hoja 518 (1878) |
| **París 1898** | el Barrio Latino: rue Lhomond, Mouffetard, d'Ulm, Saint-Jacques, el boulevard Saint-Michel y Port-Royal; la Montagne Sainte-Geneviève; el Sena; el Luxemburgo y el Jardin des Plantes; la rue de la Glacière | *Plan de Paris* (Alphand 1889); Quinn 1995 |
| **Colorado Springs** (1899) | Pikes Peak Avenue, Cascade, Tejon, Nevada (las avenidas de 30 m de Palmer); el Monument Creek y el ferrocarril; Knob Hill; el Pikes Peak | Sanborn Map, Colorado Springs 1897; Tesla 1978 |
| **Berna** (1905) | el casco viejo en la península del Aar: Kramgasse (n.º 49) y Gerechtigkeitsgasse, Marktgasse y Spitalgasse, sus paralelas; los puentes de Nydegg, Kirchenfeld y Kornhaus; el Gurten | Stadtplan Bern 1905; Einstein-Haus Bern |
| **Pasadena** (1953) | Caltech; California, Del Mar y Colorado Boulevard; Lake, Hill, Wilson, Allen y Arroyo Parkway; el Arroyo Seco; los montes de San Gabriel | USGS Pasadena 1:24 000 (1953) |
| **Prípiat y la central** (1986) | el bloque 4, el canal de toma y el estanque de refrigeración; el río Prípiat; la avenida de Lenin y la plaza central (Energetik, Polissia, Lazurny, parque de atracciones); la estación de Yániv (la ciudad, a la mitad de su distancia real para caber) | mapa de Prípiat de 1986; Leatherbarrow 2016 |
| **Escútari** (1854) | la planta del cuartel de Selimiye a ras de suelo; el Bósforo; el camino de Üsküdar; la mezquita y su meydan; Çamlıca | plan d'Üsküdar (1850); Royal Collection Trust |
| **Dayton** (1903) | West Third Street y Hawthorn Street; Williams y Broadway; el Great Miami y el Wolf Creek | Sanborn Map, Dayton 1897; NPS |
| **Kapilavastu** (−530) | Tilaurakot: la muralla de ladrillo (cimiento) con sus cuatro puertas, las calles en cuadrícula, los estanques; el Banganga; los arrozales | Coningham et al. 2017 (proyecto Tilaurakot-Kapilavastu, Durham) |
| **Alejandría** (−240) | la Vía Canópica (30 m) y la calle del Soma; el Gran Puerto, el Heptastadion y Faros; el Paneion; el lago Mareotis | Estrabón XVII.1.6–10; McKenzie 2007 |
| **Patanjali** (−150) | el āśrama y su ghat en el Ganges; la aldea y sus campos | (aldea genérica de la llanura del Ganges) |
| **Tregua 1914** | las dos líneas de trincheras (a 50–100 m) y sus trincheras de comunicación; el bosque de Ploegsteert; el Douve; la colina de Messines; la aldea evacuada | Brown y Seaton 1984; IWM |
| **Planta Atómica** | Woolsthorpe (el Witham), la rue Lhomond y la Kramgasse (el Aar) | los tres de arriba |
| **Uruk** (−3500) | el Éufrates y los canales; la terraza de Anu (13 m); el Eanna; campos de cebada | Nissen 1988; Crüsemann et al. 2019 |
| **Varsovia 1942** | el Vístula, Leszno, Marszałkowska, Aleje Jerozolimskie, el Jardín Sajón (lejos del gueto de la historia) | Plan Warszawy 1939 |
| **Le Chambon** (1942) | el Lignon, la calle mayor, la estación del tren del Vivarais, la carretera de Le Mazet, el monte Lizieux | Carte de l'État-major; Hallie 1979 |
| **Kowloon y Seattle** (Bruce Lee) | Nathan Road, Temple Street, Shanghai Street, Jordan y Waterloo Road, el puerto Victoria, la Roca del León; Broadway, Jefferson Street, University Way, el lago Union, el monte Rainier | Hong Kong Map 1957 (Crown Lands and Survey Office); Kroll's Map of Seattle (1960) |
| **Palenque** (683) | el Palacio y el Templo de las Inscripciones sobre la Gran Plaza; el Otolum y su acueducto; la colina del Grupo de las Cruces; el Juego de Pelota y el Grupo Norte; el Baño de la Reina; la sierra; las milpas | Barnhart 2001 (Palenque Mapping Project); Stuart y Stuart 2008 |

Las coordenadas son aproximaciones de esos planos (distancias y orientaciones redondeadas a decenas de metros), no un
levantamiento; donde dos lugares de la historia están a kilómetros (Kill Devil Hills y Dayton, Argonne y Pasadena, la Academia de
Ciencias en el quai Conti), el edificio va al borde del barrio o junto al de la historia, y se dice en su rótulo.

## Recuento

(Se rellena con la verificación en vivo: edificios, piezas de paisaje, objetos y plantas de cada pueblo.)
