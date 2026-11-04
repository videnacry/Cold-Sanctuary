# Microcosmos de los famosos (2026-11-04)

Berón (2026-11-04): «los personajes famosos, elon musk, sadhguru, y demás sácalos del mesocosmos y crea sus microcosmos
respectivos». Otro hilo (rama `claude/edif-personajes`) los saca del personal del santuario; aquí cada uno recibe **su microcosmos de
historia**: una escena de su vida pública, bien documentada, respetuosa e inspiradora, en el **pueblo real** donde ocurrió (o, para
Goku, una aldea inventada), hecha como los científicos ([microcosmos-cientificos.md](microcosmos-cientificos.md)) y el de Pakal
([microcosmos-maya-pakal.md](microcosmos-maya-pakal.md)), con las reglas del [manual de diseño](manual-de-diseno.md) §7.

## Código

| Pieza | Archivo | Qué hace |
|---|---|---|
| Las 7 historias | `Assets/Editor/StorySceneBuilder.Famosos.cs` | Sitios de la misión en el claro, la gente (insectos-humanos con su especie explícita y los colores de su versión humana), la misión (`StoryMission`, 8 pasos) y la ficha con fuentes (`HistoryCard`). Menús: «Build Microcosmos de famosos/<figura>» (uno por escena) y «Build Microcosmos de famosos (todos, uno tras otro)». |
| Los 7 pueblos | `Assets/Editor/CivicBuilder.Famosos.cs` | `FamousTown` (lo llama `CivilizeEra` en su `default`), un `…Town` por figura con los edificios de la historia (`Lot`, con su plantilla de actividad), el relleno de su lugar y su gente (`W`, `Townsfolk`); los planos en `FamousPlans` (mismo formato que `PlanText`; `PlanOf` mira los dos). Estilos: `IshaBrick`, `TamilTile`, `FactorySteel`, `MountainTimber`, `PuebloStucco`, `OmahaBrick`, `CampusBrick`, `SahelBanco`. |
| Flora y fauna | `Assets/Editor/MicroBiome.Famosos.cs` | `FamousBiome` (lo llama `MicroBiome.For` antes de su `switch`). Ciudades (Hawthorne, Albuquerque, Omaha) sin naturaleza salvaje: solo lo plantado y los animales de casa con el cuerpo de su análogo. |
| Estilo de época y lugar | `MicroStyles.cs`, `MicroStyles.Civic.cs` | Entradas nuevas `india_moderna` (Sur de Asia, 1900–), `sahel_moderno` (región nueva `Sahel`, 1960–) y `norteamerica_interior` (región nueva `NorteamericaInterior`, 1920–), con sus frutales y los nombres de época de los edificios del pueblo («Place», «Akhāṛā», «Firehouse»…). |
| Fecha, lugar, luz | `StorySceneBuilder.Dates`, `Eras.OfMicrocosmos`, `SolarPosition.GeoOf` | Fecha, hora, año y coordenadas de cada una. |
| Pisos | `MicroFloors.Table` | Ver la tabla de abajo. |
| Entradas | `VirtualizationMachine.StoryScenes`, `EraHubSceneBuilder.All` | La máquina del área de su piso y la Sala de Eras. |
| Su insecto | `InsectCast` | El porqué de cada especie (las escenas la pasan explícita: no dependen del personal). |
| Prueba | `FamososStoryTest` (**grupo 69**) | Ver «Qué probar». |

## Las siete

| Figura (escena) | Historia | Lugar, fecha y hora | Piso del santuario | Insecto (por qué) |
|---|---|---|---|---|
| **Sadhguru** (`Microcosmos_Sadhguru`) | El Dhyanalinga: termina su consagración; el silencio abierto a todos | Isha Yoga Center, al pie de los montes Velliangiri (Coimbatore), 24-6-1999, 6:00 | **Sala de Yoga** (`YogaRoom_Area`), representativo | Cigarra: años quieta bajo tierra y una muda (la transformación interior) |
| **Elon Musk** (`Microcosmos_ElonMusk`) | El cuarto Falcon 1 llega a órbita tras tres fallos | La fábrica de SpaceX en Hawthorne (California), 28-9-2008, 16:15 (23:15 UTC) | **Mecánica** (`VehicleWorkshop_Area`), **pendiente**: «Fábrica de cohetes y control de misión» | Saltamontes: el salto es un lanzamiento |
| **Goku** (`Microcosmos_Goku`) | Homenaje ORIGINAL (ficción): un día de entrenamiento sirviendo a la aldea | Aldea de montaña inventada (China tradicional, año de estilo 1930), un amanecer de primavera, 6:00 | **Dojo** (`MartialArts_Area`), representativo; entrada también por el Gimnasio mientras el Dojo siga en obras | Escarabajo pelotero: el más fuerte por su peso |
| **Bill Gates** (`Microcosmos_BillGates`) | El BASIC del Altair funciona en MITS; nace Micro-Soft (con Paul Allen) | Albuquerque (Nuevo México), primavera de 1975, 10:00 | **Mecánica**, **pendiente**: «Sala de informática (ordenadores y programación)» | Hormiga: la colonia reparte el problema en pasos sencillos |
| **Warren Buffett** (`Microcosmos_WarrenBuffett`) | Compra la Nebraska Furniture Mart a Rose Blumkin con una página y un apretón de manos | Omaha (Nebraska), 30-8-1983, 10:00 | **Estudio Textil** (`TextileStudio_Area`: la tienda empezó con alfombras), **pendiente**: «Mercado y oficina de comercio (economía)» | Hormiga segadora: guarda para los años malos |
| **MacKenzie Scott** (`Microcosmos_MacKenzieScott`) | Su donación sin condiciones a Prairie View A&M y las ayudas de emergencia a los estudiantes | Prairie View A&M University (Texas), 15-12-2020, 10:00 | **Química** (`Chemistry_Area`: aulas de ciencias), **pendiente**: «Aula y biblioteca universitaria» | Abeja: da sin pedir nada a cambio |
| **Ibrahim Traoré** (`Microcosmos_IbrahimTraore`) | Acto CIVIL: la entrega de tractores para labrar gratis los campos; la siembra y el zaï | Uagadugú (Burkina Faso), 6-5-2024, 9:00 | **Huerto** (`Garden_Area`), representativo | Termita: las Macrotermes cultivan su propio hongo |

## Las historias (pasos)

Todas: 8 pasos (`Talk` / `Reach` / `Visit` / `Collect` / `Hold`), 60 XP; al terminar, la ficha.

- **Sadhguru** (Yoga): habla con Sadhguru → la columna Sarva Dharma Stambha → lleva 3 ladrillos de barro a la cúpula → seva en cocina, huerto y arroyo → siéntate en silencio bajo la cúpula → el vecino de Ikkarai Boluvampatti → el mirador de los siete montes → Sadhguru.
- **Musk** (Stats): Musk junto al cohete (agosto) → los datos del vuelo 3 (empuje residual) → revisa motor, interetapa y secuencia → el técnico (siete semanas) → el control de misión → la cuenta atrás y el ascenso → la soldadora de tanques → Musk habla al equipo.
- **Goku** (Stats): la maestra → sube 3 cubos de agua al aljibe → leña, bancal y cerca → bajo la cascada → el compañero de entrenamiento → la cumbre → la forma con el sol → la maestra («ser fuerte es poder ayudar»).
- **Gates** (Stats): Paul Allen (el cargador escrito en el avión) → teletipo, lector de cinta y Altair → «MEMORY SIZE?» → Ed Roberts → Gates (Micro-Soft) → 3 cintas a la mesa de envíos → de noche en el motel → Gates.
- **Buffett** (Bonds): Buffett en su despacho → la tienda → la señora B («vende barato y di la verdad») → alfombras, muebles y electrodomésticos → Louie Blumkin → el contrato de una página → el apretón de manos → Buffett.
- **Scott** (Bonds): Scott en su mesa → escribe el ensayo → el despacho de la presidenta → Ruth Simmons → 3 estudiantes con deudas → 3 cartas de ayuda → la biblioteca → Simmons.
- **Traoré** (Bonds): la horticultora del barrage → la explanada → Traoré → tractor, motocultor y motobomba → el ministro (las brigadas, labrado gratis) → semillas de maíz, sorgo y niébé → labrar un surco → la agrónoma (el zaï).

## Los pueblos

| Pueblo | Edificios de la historia (con su plantilla de actividad) | Relleno | Plano |
|---|---|---|---|
| Velliangiri | Dhyanalinga (Templo), sala de práctica (Sala de Yoga), comedor (Restaurante), casa de Sadhguru, cabañas de voluntarios (Casa), huerto (Huerto), escuela de la aldea (Escuela) | casas encaladas con teja, tienda, herrero, cocotal | aproximado: el monte al oeste, el arroyo de la Noyyal, la aldea y la carretera de Coimbatore al este |
| Hawthorne | nave de montaje (Taller), control de misión (Centro de tecnología), banco del Merlin (Taller), cafetería (Restaurante), apartamentos de ingenieros (Casa) | casas de estuco, taller de coches, tienda, taquería | aproximado: Crenshaw, calle 120, autopista 105, El Segundo, el aeropuerto municipal |
| Aldea de la montaña | patio de entrenamiento (Centro de entrenamiento), casa de la maestra, cabaña de Goku (Casa), cocina (Cocina), templo (Templo), bancales de arroz (Huerto), leñera (Taller) | casas de piedra y pino, huertos de té, molino, tienda | inventado: torrente y cascada al norte, monte, bancales |
| Albuquerque | MITS (Taller), oficina de Micro-Soft (Centro de tecnología), motel (Hotel), diner de la Ruta 66 (Restaurante), piso de Gates y Allen (Casa), tienda de electrónica (Mercado) | casas de estuco color adobe, talleres, tiendas, huertos de chile y albaricoque | aproximado: Central Avenue (Ruta 66), Lomas, San Mateo, Carlisle, la sierra de Sandía; **la dirección de MITS y el motel no están verificados** |
| Omaha | Nebraska Furniture Mart (Mercado), despacho de Berkshire (Taller de papeles), casa de Farnam Street (Casa), Gorat's (Restaurante), almacén y tapicería (Taller), casa de los Blumkin (Casa) | casas de ladrillo, tiendas de Dodge Street, talleres, iglesias | **acortado** al este (×1/3): la calle 72, Dodge, Farnam, Pacific, Memorial Park |
| Prairie View | presidencia y oficina de ayudas (Taller de papeles), biblioteca John B. Coleman (Escuela), aulas de ciencias (Laboratorio), comedor (Restaurante), residencia (Casa), granja experimental (Huerto) | casas, iglesia, tienda, taller | aproximado: University Drive, autopista 290, FM 1098 |
| Uagadugú | parque de maquinaria (Taller), huertos del barrage (Huerto), mercado Rood Woko (Mercado), centro de formación agrícola (Escuela), maquis (Restaurante), cour familiale (Casa), molino (Granero) | cours familiales, talleres de soldadura, mercados y huertos de barrio | **acortado** al norte: los barrages de Tanghin, Bangr-Weoogo, la avenida Kwame Nkrumah, la estación |

Reglas del manual que cumplen: el piso no se rompe (M6: los pueblos rodean los muebles); vegetación encima de las superficies (M32:
lo hace `AreaMicrocosmos.Nature`); edificios con su etiqueta de actividad (M17: la plantilla de cada `Lot`); hogar para cada
personaje (M18: `Homes`); frutal en cada residencia (M20: el de su entrada del catálogo); paso de 2 m y calles de 4 m (M23); **sin
muros ni techos**: ninguna de las siete historias necesita muros (M24); muebles a la medida de insectos-humanos y personas (M27–M28).
En Uagadugú no hay comisaría ni cuartel ni armería; en Hawthorne y Albuquerque tampoco cuartel.

## Cómo se eligió cada historia (respeto y neutralidad)

- **Una sola escena de su vida pública**, la mejor documentada y la que inspira: construir, perseverar, confiar, dar, cultivar. Las
  fichas nombran también lo discutido (las críticas a Isha; que Musk es una figura polémica; cómo llegó Traoré al poder) en una línea
  y sin juicio, y dicen que la historia se limita a ese día.
- **Las frases son de juego**, en la voz de cada uno; no son citas, salvo dos cortas y señaladas en la ficha («uno de los mejores
  días de mi vida», Musk; «vende barato y di la verdad», el lema de la señora B).
- **Goku es ficción**: el microcosmos es un homenaje original; no copia escenas, diálogos, lugares, nombres ni técnicas de Dragon Ball.
- **Traoré**: solo lo civil y agrícola; los protagonistas son los productores; ningún arma, uniforme de combate ni edificio militar; la
  ficha dice que no hay evaluación independiente de los resultados de la Ofensiva agropastoral.

## Fuentes

**Sadhguru.** Isha Foundation, «Dhyanalinga – His Only Obsession» (isha.sadhguru.org); Isha Foundation, «Dhyanalinga Consecration
Anniversary»; Covai Post (24-6-2019), «20th anniversary of Isha Dhyanalinga celebrated»; Sadhguru (2016), *Inner Engineering*, Spiegel &
Grau. [verificar] el día (23 o 24 de junio: las fuentes difieren) y el año de fundación del centro (1993 o 1994).

**Elon Musk.** Space.com (29-9-2008), «SpaceX Successfully Launches Falcon 1 Rocket Into Orbit» (los ~500 empleados en Hawthorne y la
frase de Musk); Spaceflight Now, «Falcon 1 flight 4»; Aviation Week (2008), «Falcon 1 Makes Orbit On Fourth Attempt»; Koenigsmann et
al. (2009), SmallSat Conference (la órbita, 621 × 643 km; el empuje residual del vuelo 3).

**Goku.** Toriyama, A. (1984–1995), *Dragon Ball*, Shūeisha; Wu Cheng'en (atrib.), *Viaje al Oeste* (trad. A. C. Yu, 2012); Shahar, M.
(2008), *The Shaolin Monastery*, University of Hawai'i Press; UNESCO, «Ancient Building Complex in the Wudang Mountains».

**Bill Gates.** Allen, P. (2011), *Idea Man*, Portfolio; Microsoft, «History of Microsoft: 1975» (Allen en MITS el 1-3-1975, Micro-Soft el
4-4-1975, la licencia el 22-7-1975); Computer History Museum, «Altair BASIC Interpreter Source Tape»; Freiberger y Swaine (2000), *Fire
in the Valley*. [verificar] la fecha exacta de la demostración (febrero o marzo), la dirección de MITS y el nombre del motel.

**Warren Buffett.** Buffett, W., carta a los accionistas de Berkshire Hathaway de 1983; The New York Times (agosto de 1998), obituario de
Rose Blumkin; Lowenstein, R. (1995), *Buffett: The Making of an American Capitalist*; Benzinga (2018), «This Day In Market History».
[verificar] el precio y el porcentaje (55–60 millones, 80–90 %: las fuentes difieren) y las páginas del contrato (1 a 2).

**MacKenzie Scott.** Scott, M. (15-12-2020), «384 Ways to Help», Medium; Prairie View A&M University (15-12-2020), «Donor announces landmark
$50 million gift»; The Texas Tribune (15-12-2020); The Chronicle of Philanthropy (2020), «Gifts From MacKenzie Scott Put Overlooked
Colleges and Universities in the Spotlight» (los 10 millones de ayudas de emergencia; el fondo +40 %).

**Ibrahim Traoré.** Sidwaya (31-5-2024), «Déploiement des tracteurs pour l'offensive agropastorale et halieutique 2023-2025» (la entrega del
6-5-2024 en Uagadugú, 400 tractores, el labrado gratuito desde el 30-5-2024, las brigadas de mecanización); Sidwaya (12-6-2024),
«Editorial : De la houe au tracteur» (motocultores y motobombas); Right Livelihood Foundation, «Yacouba Sawadogo» (el zaï); FAO,
perfil de Burkina Faso. [verificar] el lugar exacto de la entrega en la ciudad (no lo dice la fuente).

**Flora y fauna.** Tamil Nadu Forest Department (Nilgiri Biosphere Reserve); City of Hawthorne / LA County street trees; City of
Albuquerque «Street Tree Guide», Dick-Peddie (1993) *New Mexico Vegetation*; City of Omaha «Street Trees»; Texas Parks and Wildlife
«Post Oak Savannah»; Arbonnier (2004) *Trees, Shrubs and Lianas of West African Dry Zones*.

## Qué probar en Unity (lo hace el probador)

1. Con `SampleScene` abierta, **de una en una** (la memoria: un rebuild entero de microcosmos ya tumbó el PC de 16 GB):
   «Tools/Cold Sanctuary/Build Microcosmos de famosos/<figura>», en este orden: Sadhguru, Goku, Traoré (pisos representativos), Musk,
   Gates, Buffett, Scott. Después «Build Sala de Eras». Guardar SampleScene si se tocó.
2. Consola: `[Pueblos] <pueblo>: …` (edificios de la historia, del pueblo y relleno), `[Reglas] Microcosmos_X: OK`, `[Pisos] …: OK`
   (o «PENDIENTE de piso representativo» en Musk, Gates, Buffett y Scott).
3. Grupo **69** (`FamososStoryTest`); y, si hay tiempo, 43 (`MicrocosmosFloorTest`) y 63 (`MicroBuildRulesTest`).
4. Fotos (ver el informe del hilo): el protagonista con sus colores; el sitio clave de cada historia; el pueblo llenando el piso;
   frutales en los patios; ningún muro ni techo; nada flotando ni bajo el suelo.
