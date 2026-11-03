# Microcosmos maya: Pakal de Palenque, el rey que renace como el maíz (2026-11-03)

Berón: «investiga sobre los mayas para recrear una de sus historias más bonitas en el microcosmos». Se sigue la regla del proyecto:
investigación → plan → construcción.

## 1. Investigación: qué historia

| Candidata | A favor | En contra |
|---|---|---|
| **Los Gemelos Héroes (Popol Vuh)** | la historia maya más famosa: Hunahpú e Ixbalanqué bajan a Xibalbá, vencen a los señores de la muerte en el juego de pelota y resucitan a su padre, el dios del maíz | es un mito sin lugar ni fecha: no hay un pueblo real que reconstruir (la regla 5 pide la ciudad entera de la historia); el texto es del siglo XVI (k'iche', posclásico) |
| **Pakal y Palenque** | una vida real, fechada al día en las inscripciones (603–683), con una ciudad muy bien cartografiada (Barnhart 2001), llena de estructuras naturales (el Otolum, cascadas, la sierra, la selva); su final es el mismo mito del maíz que renace, tallado en su lápida | — |
| **Copán (Yax Pasaj, la Escalinata Jeroglífica)** | una ciudad bien estudiada y la inscripción más larga | la historia de una dinastía, menos íntima; menos paisaje |
| **Yaxchilán (los dinteles de la Señora K'abal Xook)** | escenas bellísimas (la visión de la serpiente) | el rito de autosacrificio con sangre no es para el tono del santuario |

**Elegida: K'inich Janaab Pakal de Palenque.** Es a la vez la historia más bien documentada (sus fechas están leídas en piedra) y
de las más bellas: una madre que corona a su hijo de doce años; una ciudad que se llama «grandes aguas»; un rey que manda escribir
la historia de su pueblo en 617 glifos; y su muerte, que la lápida cuenta como un renacer: cae al inframundo como cae la semilla y
de él sale el árbol del mundo, como sale el maíz — el mismo renacer del dios del maíz del Popol Vuh, así que la historia de los
Gemelos también está dentro.

### Hechos (con fuentes)

| Hecho | Fuente |
|---|---|
| Nace el 23-3-603 (9.8.9.13.0) | Martin y Grube 2008 |
| Sube al trono el 29-7-615 (9.9.2.4.8), a los 12 años; su madre, **Ix Sak K'uk'**, había gobernado; el **Tablero Oval** de la Casa E la muestra dándole el tocado real | Martin y Grube 2008; Stuart y Stuart 2008 |
| Palenque, saqueada por Calakmul en 599 y 611 | Martin y Grube 2008 |
| Se casa con **Ix Tz'akbu Ajaw** en 626 | Martin y Grube 2008 |
| Dedica la **Casa E** (la «Casa Blanca», Sak Nuk Naah) del Palacio en 654 | Stuart y Stuart 2008 |
| El nombre antiguo de la ciudad: **Lakamha'**, «grandes aguas»; el arroyo **Otolum** corre encauzado en un acueducto abovedado bajo el Palacio (Clásico tardío; sin fecha exacta) | Stuart y Stuart 2008; Barnhart 2001 |
| El **Templo de las Inscripciones**: tres tableros con **617 glifos** (la historia dinástica); nueve cuerpos escalonados | Schele y Mathews 1998; INAH |
| Muere el 28-8-683 (**9.12.11.5.18**); su hijo **K'inich Kan Bahlam** termina el templo | Martin y Grube 2008 |
| 1952: **Alberto Ruz Lhuillier** encuentra la escalera escondida y la cripta; la **lápida** (3,8 × 2,2 m) muestra a Pakal entre las fauces del inframundo y el árbol del mundo con el ave celeste; un tubo de piedra (el «psicoducto») sube de la tumba al templo | Ruz Lhuillier 1973; Schele y Freidel 1990 |
| El maíz y la muerte-renacer del dios del maíz (Hun Hunahpú) | *Popol Vuh* (Tedlock 1996) |

Fuentes:
- Martin, S. y Grube, N. (2008). *Chronicle of the Maya Kings and Queens*, 2.ª ed. Thames & Hudson.
- Stuart, D. y Stuart, G. (2008). *Palenque: Eternal City of the Maya*. Thames & Hudson.
- Schele, L. y Freidel, D. (1990). *A Forest of Kings*. William Morrow.
- Schele, L. y Mathews, P. (1998). *The Code of Kings*. Scribner.
- Ruz Lhuillier, A. (1973). *El Templo de las Inscripciones, Palenque*. INAH.
- Barnhart, E. (2001). *The Palenque Mapping Project: Settlement and Urbanism at an Ancient Maya City*. FAMSI / Univ. of Texas.
- Tedlock, D. (trad.) (1996). *Popol Vuh*. Simon & Schuster.
- Flora y fauna: CONABIO; Reserva de la Biosfera y Zona Arqueológica de Palenque (INAH).

**[verificar]**: la fecha de construcción del acueducto (se dice «Clásico tardío» sin más) y la lectura de la lápida (caída y
renacer como el maíz es la de Schele y Freidel; otros autores la leen solo como el renacer solar).

## 2. Plan

- **Escena**: `Microcosmos_Pakal` (`StorySceneBuilder.Maya.cs`; entra sola en Build Settings y en la Sala de Eras; máquina de
  virtualización del Huerto). Plantilla: fecha «28 de agosto de 683 (9.12.11.5.18 en la Cuenta Larga)», 7:00, Palenque
  (17,48° N, 92,05° O: `SolarPosition`).
- **Piso**: no hay en el santuario un piso que la represente. Se marca **«pendiente de piso representativo: Templo de los
  antepasados (memoria, rito y duelo)»** (`MicroFloors`) y se monta sobre el **Huerto** (la milpa: el maíz de la historia).
- **Estilo**: región nueva `MicroStyles.Region.Maya`, entrada `maya_clasico` (250–900): banca de mampostería enlucida con
  petate para dormir, banca para sentarse, mesa baja, **k'oben** (fogón de tres piedras) con comal, cántaro, tinajas, tea de ocote.
  Sus frutales: cacao, aguacate, ramón, zapote y papaya. Edificios de época: Gran Plaza, estela, pila del acueducto, arboleda de
  cacao y ceibas, mercado de la plaza, **temazcal**, juego de pelota, casa de los guerreros, casa de los escribas, taller de jade y
  obsidiana, obra de un templo (cal y piedra), guardería; ni comisaría, ni bomberos, ni estación, ni carretero (no usaban la rueda).
- **La ciudad** (`CivicBuilder.PalenqueTown`, sobre su plano: [microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md)): el
  **Palacio en terrazas** (planta 1: el trono de la Casa E con el Tablero Oval y los códices; planta 2: el tablero de las cuentas;
  planta 3: la torre), el **Templo de las Inscripciones** en cuatro terrazas con su rampa (los tres tableros arriba), el **Juego
  de pelota**, la casa de los escribas, la cocina del Palacio (comal, metate), el Templo de la Cruz Foliada, la casa de la familia
  real, y el pueblo: casas mayas con su cacao, talleres, milpas, alfarería. El paisaje: el **Otolum**, el **Baño de la Reina**, la
  **colina de las Cruces**, la **sierra de Don Juan** y las milpas del llano.
- **Bioma** (`MicroBiome`, «Palenque»): ceiba, ramón, cacao, helecho arborescente, bromelia, maíz; hormiga cortadora (*Atta
  cephalotes*), la abeja sin aguijón de los mayas (*Melipona beecheii*, xunáan kab), cigarra gigante (*Quesada gigas*),
  luciérnaga, saltamontes de la milpa y araña de seda dorada; en el pueblo, el perro del solar y el pavo de monte (sus análogos).
- **Elenco** (insectos-humanos, el elenco renacido): Pakal, hormiga cortadora (la que cultiva); Ix Sak K'uk', luciérnaga; K'inich
  Kan Bahlam, saltamontes; Ix Tz'akbu Ajaw, mariquita; el escriba, cigarra; la campesina, termita.

### Los 11 pasos

| # | Paso | Dónde |
|---|---|---|
| 1 | Habla con el anciano Pakal (683) | junto al Tablero Oval |
| 2 | 615: ve al Tablero Oval de la Casa Blanca | Tablero Oval |
| 3 | Habla con Ix Sak K'uk', su madre | — |
| 4 | Quieto ante el Tablero: la madre le da el tocado | Tablero Oval |
| 5 | Ve al Otolum, bajo el Palacio | acueducto |
| 6 | Escucha el agua bajo la bóveda | acueducto |
| 7 | La milpa: lleva tres mazorcas al altar de la Cruz Foliada | mazorcas → altar |
| 8 | Toca los tres tableros de las Inscripciones (617 glifos) | tres tableros |
| 9 | 683: habla con K'inich Kan Bahlam | — |
| 10 | Quieto ante la lápida: cae y renace como el maíz, al pie de la ceiba | lápida y ceiba |
| 11 | Habla con Ix Tz'akbu Ajaw (el psicoducto) → final | — |

Al terminar, la `HistoryCard` cuenta lo que pasó de verdad, con las fuentes. Marga: **Bonds** (los lazos: madre e hijo, los
antepasados), 60 XP.

## 3. Construido

- `Assets/Editor/StorySceneBuilder.Maya.cs` — la historia (`Pakal`), en «Build Microcosmos de historias».
- `Assets/Editor/CivicBuilder.Maya.cs` — la ciudad (`PalenqueTown`), con `Storeys` para el Palacio y el Templo.
- `Assets/Editor/CivicBuilder.Plans.cs` — su plano («Palenque»).
- `MicroStyles` (región Maya, `maya_clasico`, frutales), `MicroBiome` («Palenque»), `Eras` (683), `SolarPosition`, `MicroFloors`
  (pendiente), `EraHubSceneBuilder`, `VirtualizationMachine` (Huerto).
- Prueba: **`PakalStoryTest` (grupo 66)**: fecha y región, la historia avanza, el Palacio y el Templo en terrazas, el juego de
  pelota, el Otolum, ≥ 12 edificios, edificios de su época y ninguno anacrónico, cada casa con su frutal maya, el estilo maya en
  los muebles, flora y fauna del lugar, las reglas de edificación (1–12) y las de los microcosmos, y el «pendiente de piso».
  También va en `CivilizedErasTest` (62).
