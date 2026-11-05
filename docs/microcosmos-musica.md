# Microcosmos de la música (2026-11-05)

Berón (2026-11-05) pidió microcosmos de **Shakira**, **Bad Bunny**, **John Lennon** y **Michael Jackson**, «black pink tiene una buena
historia? o existe otra banda musical con mejor historia? crea microcosmos de ambos si ambos son buenos», «entiendo que el rock denuncia
situaciones del mundo junto al rap» y uno general, **«Grandes aportes de las canciones»**. Son 9 escenas, hechas como las de los
[famosos](microcosmos-famosos.md) y con las reglas del [manual de diseño](manual-de-diseno.md) §7. Solo código: **sin probar en Unity**.

**Reglas de esta tanda (van en la regla J6 del manual):**
- **Ninguna letra de canción** en ningún sitio: título, autor y tema contado con palabras propias. La ficha de cada historia lo avisa («sin
  reproducir sus letras») y la prueba lo mira.
- **Ninguna carátula, logotipo ni vestuario** de escenario: ropa de diario con los colores de cada uno.
- **Personas reales, con respeto**: escenas civiles o humanas; lo discutido, en una línea y sin juicio; las frases son de juego.
- Sin armas en escena, también donde la historia habla de violencia (Kingston, Belfast): se cuenta de palabra.

## Código

| Pieza | Archivo | Qué hace |
|---|---|---|
| Las 9 historias | `Assets/Editor/StorySceneBuilder.Musica.cs` | Sitios de la misión, gente (insectos-humanos), `StoryMission` de 8 pasos, `HistoryCard` con fuentes. Menús: «Build Microcosmos de música/<historia>» (uno por escena) y «Build Microcosmos de música (todos, uno tras otro)». |
| Los 9 pueblos | `Assets/Editor/CivicBuilder.Musica.cs` | `MusicTown` (lo llama `FamousTown` en su `default`), un `…Town` por historia, los planos en `MusicPlans` (`PlanOf` los mira) y los estilos `CaribeBlock`, `SchoolBlock`, `MontrealBrick`, `SteelFrame`, `SeoulConcrete`, `YardBlock`, `BelfastBrick`, `SuburbFrame`. |
| Flora y fauna | `Assets/Editor/MicroBiome.Musica.cs` | `MusicBiome` (lo llama `MicroBiome.For`). |
| Mapas a mano | `Assets/Scripts/Microcosmos/MicroStoryMaps.Musica.cs` | Los 9 mapas, todos `Hecho`: orden del piso, cada rodal de flora, cada bicho y los edificios (regla M9). `MicroStoryMaps.For` mira `All` y luego `Music`. |
| Estilo de época y lugar | `MicroStyles.cs`, `MicroStyles.Civic.cs` | Regiones nuevas `CaribeHispano`, `Jamaica`, `Corea`, `IslasBritanicas`, `NorteamericaNorte`, `Sudafrica`; entradas `caribe_hispano`, `jamaica_moderna`, `corea_moderna`, `britanico_moderno`, `norteamerica_norte`, `sudafrica_moderna`, con sus frutales y sus lugares de entrenar (`estadio` en el Caribe y Sudáfrica). |
| Instrumentos | `Assets/Editor/MicroFurniture.Music.cs` | `piano`, `bateria`, `amplificador`, `microfono`, `mesa_mezclas` (cuatro pistas antes de 1990; portátil después), `tarima`. Acompañan a un puesto (`CivicBuilder.NotStation`): no son puestos. |
| Rincones de otra época | `Lot.Era(año, región)` en `CivicBuilder.Towns.cs` | Un edificio con otro año y región dentro del mismo pueblo (solo con plano, `PlannedTown`). Lo usa «Grandes aportes». |
| Fecha, lugar, luz | `StorySceneBuilder.Dates`, `Eras.OfMicrocosmos`, `SolarPosition.GeoOf` | Fecha, hora, año y coordenadas. |
| Pisos | `MicroFloors.Table`, `Match.MusicFloor` | Ver la tabla. El piso que falta se escribe sin la palabra «estudio» (si no, la alerta de piso nuevo lo cruzaba con el Aula y el Estudio Textil). |
| Entradas | `VirtualizationMachine.StoryScenes`, `EraHubSceneBuilder.All` | La máquina del área y la Sala de Eras. La Sala de Ballet sigue en obras: mientras, también se entra desde el Gimnasio, como el Dojo. |
| Su insecto | `InsectCast` | El porqué de cada especie (las escenas la pasan explícita). |
| Prueba | `Assets/Scripts/Tests/MusicaStoryTest.cs` (**grupo 86**) | Ver «Qué probar». También añadidas a 62 (`CivilizedErasTest`) y 87 (`GymTrainingTest`); `MicrocosmosFloorTest` (43) sube a 70 escenas. |

## Las nueve

| Historia (escena) | Lugar y fecha | Piso | Insecto (por qué) |
|---|---|---|---|
| **Shakira** (`Microcosmos_Shakira`) | Colegio Pies Descalzos de La Playa, Barranquilla, 2009, 8:00 | **Aula** (`Classroom_Area`), representativo | Abeja: baila para contar dónde hay comida |
| **Bad Bunny** (`Microcosmos_BadBunny`) | Vega Baja, Puerto Rico, 2016, 17:00 | **Informática** (`Informatics_Area`), **pendiente** | Grillo: canta de noche desde su madriguera |
| **John Lennon y Yoko Ono** (`Microcosmos_JohnLennon`) | Queen Elizabeth Hotel, Montreal, 1-6-1969, 19:00 | **Sala de Ballet** (`Ballet_Area`), **pendiente** | Cigarra (años quieta: quedarse quieto es la protesta); Yoko, luciérnaga |
| **Michael Jackson y los suyos** (`Microcosmos_MichaelJackson`) | Gary, Indiana, 1966, 17:30 | **Sala de Ballet**, **pendiente** | Luciérnaga: cada especie tiene su coreografía de destellos |
| **BLACKPINK** (`Microcosmos_Blackpink`) | Hapjeong, Seúl, 8-8-2016, 11:00 | **Sala de Ballet**, representativo (es una sala de ensayo) | Mariquita asiática: nativa de Corea, pasa el invierno apiñada |
| **Bob Marley y los Wailers** (`Microcosmos_Wailers`) | Kingston, 22-4-1978, 17:00 | **Sala de Ballet**, **pendiente** | Grillo (Tosh, mantis) |
| **U2** (`Microcosmos_U2`) | Waterfront Hall, Belfast, 19-5-1998, 19:00 | **Sala de Ballet**, **pendiente** | Bono, cigarra; Hume y Trimble, dos hormigas de la misma especie |
| **Public Enemy** (`Microcosmos_PublicEnemy`) | Roosevelt y Hempstead, Long Island, primavera de 1989, 16:00 | **Aula**, **pendiente** (nació en una radio universitaria) | Chuck D, escarabajo pelotero: convierte lo que otros tiran (el sampler) |
| **Grandes aportes de las canciones** (`Microcosmos_GrandesCanciones`) | La Plaza de las Canciones, **lugar inventado**, un 21 de junio, 18:00 | **Aula**, **pendiente** | Cronista, grillo; cada figura, el suyo |

**Piso pendiente nuevo:** «Sala de grabación y escenario de conciertos (música)» (`MicroFloors.Match.MusicFloor`). Es uno más de la lista. Lo piden Vega Baja, Montreal, Gary, Kingston, Belfast, Long Island y Grandes aportes. Si se crea, `AlertFloors` los señala.

## Las historias (pasos)

Todas: 8 pasos, 60 XP; al terminar, la ficha.

- **Shakira** (Bonds): Shakira en la entrada → la mesa del censo → cuadernos al aula → comedor, cancha y taller de las familias → el rector → el ensayo de danza → la vecina → Shakira.
- **Bad Bunny** (Stats): Benito en la caja → la compra al carro de doña Carmen → la iglesia → su madre → radio, discos y carretera → de noche sube «Diles» → DJ Luian → Benito.
- **Lennon** (Bonds): John en la cama → la mesa de prensa → bellotas a correos → entrevista, guitarra y puerta → Yoko → André Perry → la grabación → John.
- **Gary** (Bonds): Katherine → Tito (la cuerda rota) → la puerta de la acería → instrumentos a la sala → el ensayo → la maestra → el concurso → Michael.
- **BLACKPINK** (Stats): Jennie → la evaluación del mes → cuadernos al aula de coreano → Lisa → Rosé → ante el espejo → Jisoo → 8 de agosto: Jennie.
- **Wailers** (Bonds): Massop y Marshall → Bob → el patio de Trench Town (Joe Higgs) → sobres de la taquilla → Tosh, Miller y Brown → las manos en alto → la vendedora → Bob.
- **U2** (Bonds): Tim Attwood → entradas a la puerta → Maysfield → la bandera blanca → Trimble (la chaqueta) → Hume → el concierto → Bono.
- **Public Enemy** (Stats): Chuck D → WBAU → cajas de discos → Hank Shocklee → cancha, parada y biblioteca → Flavor Flav → la grabación → Chuck D.
- **Grandes aportes** (Bonds): la cronista → Guy Carawan (We Shall Overcome) → la mondina (Bella Ciao) → el piano de Imagine → Peter Gabriel (Biko) → la guitarra de Redemption Song → Susana Baca (Latinoamérica) → la cronista.

## BLACKPINK: ¿tiene una buena historia? La comparación

| Banda | Qué tiene su historia | Qué le falta | Documentación | ¿Microcosmos? |
|---|---|---|---|---|
| **BLACKPINK** | Años de aprendizaje (Jennie 2010; Lisa, 1 de 4000 en Tailandia; Rosé, 1 de 700 en Australia), lejos de casa, 14 h diarias, evaluaciones mensuales, de 9 previstas a 4. Después, COP26 y MBE (2021–2023). | No hay adversidad exterior ni un acto cívico propio en el centro; es la historia de un sistema, criticado por la presión a menores, y casi todo sale de la agencia y de ellas. | Buena en fechas; pobre en fuentes independientes sobre los años de aprendizaje. | **Sí**: buena historia de disciplina y de amistad. |
| **Bob Marley y los Wailers** | De un patio común de Trench Town a un estadio; tiroteado en 1976 y canta igual; dos jefes de banda rivales lo traen del exilio; une las manos de los líderes enfrentados en 1978. | La paz no duró (y la historia lo dice). | Muy buena: prensa jamaicana, crónicas, museos. | **Sí**: **la mejor historia**. |
| **BTS** | Agencia casi en quiebra, siete chicos de provincias. | Las fuentes no coinciden en la fecha de la crisis, y su cara cívica (ONU, UNICEF) no quedó verificada en esta investigación. | Media. | No, por ahora: se parecería a BLACKPINK (misma ciudad, mismo sistema). |
| **Queen, Beatles** | Live Aid; Hamburgo. | Los Beatles ya entran con Lennon. Queen no se investigó a fondo. | — | No. |

**Veredicto:** BLACKPINK sí tiene una buena historia, pero de **esfuerzo**, no de cambio; la banda con mejor historia es **Bob Marley y los Wailers**.
Como las dos son buenas, se hicieron las dos, tal como pidió Berón.

## Rock y rap: los elegidos

**Rock: U2.** Candidatos: U2, Rage Against the Machine. Se eligió U2 porque cierra el círculo: **denuncia** (Sunday Bloody Sunday, 1983, contra la violencia de
los dos lados) y luego **actúa** en un hecho concreto y fechado (el concierto del Sí, 1998), con dos líderes rivales en el escenario y un resultado medible. Rage
Against the Machine denuncia sin tregua, pero no tiene una escena única, civil y bien documentada que no gire en torno a un choque con la policía o a una protesta contra algo.

**Rap: Public Enemy.** Candidatos: Public Enemy, N.W.A., Calle 13. Se eligió Public Enemy porque toda su obra se plantea como denuncia y **noticiero del barrio**, tiene una escena
civil (una radio universitaria, una mesa de dibujo, un encargo de Spike Lee) y reconocimiento institucional (Registro Nacional de Grabaciones, 2005). N.W.A. es tan importante como
ellos, pero sus letras más conocidas y las violencias de sus miembros fuera del escenario hacen difícil una escena respetuosa sin blanquear nada. **Calle 13** entra en «Grandes
aportes» con Latinoamérica; Puerto Rico ya está en Vega Baja.

## Lennon: por qué Montreal y no Liverpool

Liverpool (el encuentro con McCartney en 1957) es una historia de juventud, privada y sobre todo beatle. El Bed-In de Montreal es un acto **público, cívico y
no violento**, muy documentado, hecho a dos con Yoko Ono, y de esa habitación salió una canción que en noviembre de 1969 cantó medio millón de personas. Imagine va en «Grandes aportes».

## Michael Jackson: cómo se trata

No existía microcosmos suyo (comprobado en `MicroFloors.Table`, `Eras` y las ramas). Se eligió **Gary en 1966**: la historia es de una familia obrera, Michael tiene 7 años y nada
de su vida adulta entra en la escena. La ficha nombra en un párrafo, sin juicio, la violencia de la que él habló y las acusaciones de abusos a menores (acuerdo de 1994, absolución de
2005, acusaciones posteriores). **Punto a revisar por Berón:** poner en juego a Michael niño junto a una ficha que menciona esas acusaciones es una decisión sensible; si lo prefiere, se
puede sustituir por una historia de We Are the World (1985).

## Los pueblos

Cada uno lleva los edificios de su historia (hitos), los del pueblo y relleno por sus calles, con trabajo, ocio, entrenamiento y recuperación:

| Pueblo | Edificios de la historia | Entrenamiento / recuperación | Plano |
|---|---|---|---|
| La Playa | colegio, comedor, centro comunitario, cancha, sala de danza, taller de familias, puesto de salud, 2 casas | cancha / puesto de salud | aproximado; **acortado** al este; calles interiores sin verificar |
| Vega Baja | Econo, iglesia, casa de Almirante Sur, escuela, chinchorro, barbería, estudio casero, hospital, casa | parque atlético (mapa actual) / hospital | **acortado** N-S; **situación del Econo sin verificar** |
| Montreal | hotel, Gare Centrale, estudio de Perry, Beaver Club, McGill, Place des Arts, YMCA, Royal Victoria, 2 casas | YMCA / Royal Victoria | aproximado y acortado; **sitio del estudio de Perry sin verificar** |
| Gary | casa de Jackson St., Roosevelt, Mr. Lucky's, acería, Steeltown, Sears, gimnasio de boxeo, Mercy, Salón del Reino | boxeo / Mercy | **acortado** al norte; dirección de Steeltown y Grant Street sin verificar |
| Seúl | YG, sala de ensayo, residencia, comedor, aula, Mangwon, jjimjilbang, Hongdae, clínica | sala de ensayo / jjimjilbang y clínica | aproximado; **residencia y clínica representativas** |
| Kingston | Estadio, Yard de First St., patio de Higgs, Studio One, Hope Road, Coronation Market, cookshop, KPH, campo de fútbol | campo / KPH | **acortado a un tercio** |
| Belfast | Waterfront, oficina del Sí, Maysfield, Queen's, Royal Victoria, 2 casas, Harland & Wolff, Crown | Maysfield / Royal Victoria | **acortado**; oficina representativa; la línea de paz es una calle, sin muro |
| Long Island | Adelphi, WBAU, South Franklin, casa Ridenhour, cancha, Coliseum, diner, hospital, iglesia | cancha / hospital | **acortado** (campus a ~6 km); casa representativa |
| Plaza de las Canciones | 6 rincones (`Lot.Era`) y sala de conciertos, escuela de música, comedor, casa, gimnasio, casa de salud | gimnasio / casa de salud | **inventado** |

Todos caben a 1:1 en su barrio (±1000 × ±720 m; Vega Baja, sobre Informática, ±900 × ±560 m), para que el mapa a mano y el plano coincidan.

## Fuentes

Consultadas en esta investigación:

**Shakira.** Fundación Pies Descalzos, «Historia»; El Heraldo (6-10-2012); El Universal (2023, Nuevo Bosque); Noticias RCN (2023). **Bad Bunny.** Wikipedia, «Bad Bunny»; AOL (2025);
LOS40 (2025). **Lennon.** Wikipedia, «Bed-in» y «Give Peace a Chance»; Radio Canada International (2016); Global News (2019); uDiscover Music. **Jackson.** Wikipedia, «The Jackson 5», «Michael Jackson»,
«Steeltown Records»; WBEZ (2017); Soultracks; Britannica. **BLACKPINK.** Wikipedia, «Blackpink»; Vogue Singapore, The Korea Times, Jezebel (2020); Soompi (2021); The Borneo Post (2023).
**Wailers.** Wikipedia, «One Love Peace Concert», «Trenchtown», «Joe Higgs», «Simmer Down»; Biography.com; Jamaica Gleaner (2009); History.com. **U2.** The Irish Times (2023, 2025); The Irish News
(2018); Hot Press; Wikipedia, «Sunday Bloody Sunday». **Public Enemy.** Wikipedia, «Public Enemy», «Fight the Power»; Encyclopedia.com; Okayplayer. **Canciones.** Library of Congress Folklife Today (2014);
Wikipedia, «Bella ciao», «Biko (song)», «Latinoamérica (song)»; The Beatles Bible; MusicRadar; CBC (2019); Britannica y The Nation (LBJ, 1965).

**[verificar]: lo que NO salió de esas fuentes** (conocimiento general, sin abrir fuente en esta tanda; hay que contrastarlo):
- **Flora y fauna de los 9 biomas**: especies y referencias botánicas (Little y Wadsworth 1964, Adams 1972, Humboldt 2014, Brack Egg 1999, DRNA, Belfast Hills Partnership, Seoul Metropolitan Government, Ente Risi…) son de memoria.
- **Planos**: calles y posiciones de memoria, sin abrir OpenStreetMap ni planos de la época; por eso todos dicen «aproximado».
- **Biografía**: la anécdota del parque y el año del álbum Pies Descalzos (Shakira); su acuerdo fiscal de 2023; Heal the World (1992); el mes y día del tiroteo a Marley (1976) y Smile Jamaica; Saville (2010) y las cifras del Domingo Sangriento; el Nobel de Hume y Trimble; Beaver Club, YMCA de la rue Drummond, Mr. Lucky's en Grant Street, 510 South Franklin Street, Maysfield en diciembre de 1982.
- **Fuentes citadas en fichas sin abrirlas**: UK Government (informe Saville), Library of Congress (registro de 2005) y El Colombiano; Tattrie (2016) solo por su reseña.

## Qué probar en Unity (lo hace el probador)

1. Con `SampleScene` abierta, **de una en una** (W5), con «Tools/Cold Sanctuary/Build Microcosmos de música/<historia>», en este orden: **Shakira, BLACKPINK** (pisos representativos), Bad Bunny, Lennon, Gary, Kingston, Belfast, Public Enemy, Grandes aportes. Después «Build Sala de Eras» y guardar SampleScene si se tocó. La Sala de Ballet tiene que estar construida con su «Acabado».
2. Consola: `[Pueblos] …` (escala **1:1** en todos), `[Mapa] …: mapa a mano (…)` y ninguna `la planta «…» no está en su bioma`; `[Reglas] Microcosmos_X: OK`; `[Pisos] …: OK` o «PENDIENTE».
3. Grupos: **86** (`MusicaStoryTest`: las 9 escenas), 62, 43 (`MicrocosmosFloorTest`), 63, 87 (entrenamiento de época), 68 (toda historia con mapa) y 76 (alerta de piso nuevo).
4. Fotos: el protagonista con su ropa; el sitio clave de cada historia (colegio, caja del súper, cama y carteles, sala de ensayo con su espejo, telones, butacas, cabina); los mogotes de Vega Baja sobre las máquinas; la acería de Gary sobre el espejo; los seis rincones de «Grandes aportes» con su propio mobiliario; frutales en los patios; ningún muro ni techo; nada flotando.

## Riesgos conocidos

- **Elementos del mapa**: `Piece` con `Como` usa elementos del catálogo; en la Sala de Ballet solo se ha comprobado por nombres. Si «mesa» no empareja el piano, se queda en su versión general (peñascos).
- **`Lot.Era`** solo cambia el año y la región al levantar el edificio; los que luego retocan muebles (`Homes`) solo tocan casas, y ningún rincón lo es.
- **Conflicto con `claude/edif-histo`**: las dos ramas añaden filas tras las mismas anclas (`Stories`, `Dates`, `Eras`, `GeoOf`, `MicroFloors.Table`, regiones): al unirlas hay que **conservar las dos**.
