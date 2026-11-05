# Microcosmos de historias, tanda 3 (2026-11-05)

Berón (2026-11-05) pidió nuevos microcosmos de figuras e historias, cada uno con su mapa a mano y todas las reglas, con figuras reales en
escenas civiles y humanas, sin violencia gloriosa. Esta tanda son **12 microcosmos** (rama `claude/edif-histo`). La música (Shakira, Bad Bunny, Lennon,
Michael Jackson…) va aparte, en [microcosmos-musica.md](microcosmos-musica.md) (rama `claude/edif-musica`). Nietzsche ya existía (Sils Maria): no se
toca. Reglas: [manual de diseño](manual-de-diseno.md) §7 y J6, y [microcosmos-mapas-a-mano.md](microcosmos-mapas-a-mano.md) (M9).

**Solo código y compilación: nada de esto se ha probado aún en Unity** (lo hace el probador; ver «Qué probar»).

## Código

| Pieza | Archivo | Qué hace |
|---|---|---|
| Cada historia, en su archivo | `Assets/Editor/Historias/Historia.<Figura>.cs` | Las tres partes juntas: la escena (`StorySceneBuilder`: sitios, gente, misión de 8 pasos, ficha con fuentes), el pueblo (`CivicBuilder`: plano en su `const string Plan…`, `Town`, estilos, gente) y el bioma (`MicroBiome`). |
| Menús y reparto | `Assets/Editor/Historias/Historias.Core.cs` | «Tools/Cold Sanctuary/Build Microcosmos de historias 3/<figura>» (uno por escena) y «… (todos, uno tras otro)»; `HistoTown`, `HistoPlans`, `HistoBiome`. |
| Mapas a mano | `Assets/Scripts/Microcosmos/MicroStoryMaps.Historias*.cs` | `MicroStoryMaps.Historias3`: los 12 con `Status.Hecho`: órdenes sobre el piso, flora, fauna y edificios. `Natural(...)`: en historias sin máquinas, lo que en el piso sería máquina, antena o estación es roca. |
| Estilo de época y lugar | `MicroStyles.cs`, `MicroStyles.Historias.cs` | 11 regiones y 11 entradas nuevas (abajo), y sus nombres de edificios (`HistoNames`: **lo que no está en la tabla no existía** y no se construye) y frutales (`HistoFruits`). |
| Lugares de entrenar | `HistoricTraining.cs`, `CivicBuilder.Plans.cs` (`TrainingKit`), `MicroStyles.Civic.cs` (`TrainingKind`) | 4 clases nuevas: `palin`, `warachikuy`, `tewaarathon`, `pelota_nacional` ([microcosmos-entrenamiento-eras.md](microcosmos-entrenamiento-eras.md)). |
| Fecha, lugar, luz, piso | `StorySceneBuilder.Dates`, `Eras.OfMicrocosmos`, `SolarPosition.GeoOf`, `MicroFloors.Table` | Una fila por historia. |
| Entradas | `VirtualizationMachine.StoryScenes`, `EraHubSceneBuilder.All` | La máquina del área de su piso y la Sala de Eras. Viena, desde el piso de Música. |
| Su insecto | `InsectCast` | El porqué de cada especie (las escenas la pasan explícita). |
| Pruebas | `HistoriasDatosTest` (**grupo 78**, sin escenas), `HistoriasStoryTest` (**grupo 79**, carga las 12 de una en una) | Ver «Qué probar». `GymTrainingTest` (87) incluye las 12 y las 4 clases; `MicrocosmosFloorTest` sube a 70 escenas. |

## Las doce

| Historia (escena) | Qué pasa | Lugar, fecha y hora | Piso del santuario | Insecto (por qué) |
|---|---|---|---|---|
| **Eloy Alfaro** (`Microcosmos_EloyAlfaro`, «Chimbacalle») | Llega el primer tren de Durán a Quito el día de su cumpleaños; la cuadrilla jamaicana, la escuela laica, el clavo de oro | Chimbacalle, Quito, 25-6-1908, 10:00 | **Mecánica**, **pendiente**: «Estación de ferrocarril y transporte» | Hormiga: abre un sendero y por él pasa la colonia |
| **Galvarino** (`Microcosmos_Galvarino`, «Lagunillas») | Vuelve a su lof; la machi, el lawen, su gente lo cuida; habla en el koyag | Un lof de Arauco, noviembre de 1557, 9:00 | **Hospital**, representativo (curarse y seguir) | Grillo: suelta una pata y sigue saltando |
| **Atahualpa** (`Microcosmos_Atahualpa`, «Cajamarca») | El rescate: quipus, colcas, cumbi, chasquis: el Estado inca sigue funcionando | Cajamarca, abril de 1533, 10:00 | **Estudio Textil**, **pendiente**: «Almacén y contaduría (qollqas y quipus)» | Termita: una ciudad de barro sin rueda ni hierro |
| **La Gran Ley de la Paz** (`Microcosmos_GranLeyDeLaPaz`, «Onondaga») | Jigonsaseh, Ayenwatha, el Pacificador y Tadodaho: cinco naciones, una casa larga | Lago Onondaga, hacia 1450 (fecha discutida), 9:00 | **Aula**, **pendiente**: «Sala de asambleas (parlamento)» | Abeja: el enjambre decide por consenso |
| **Cleopatra** (`Microcosmos_Cleopatra`, «Alejandría de Cleopatra») | Dos años sin crecida: abre los graneros y habla a cada barrio en su lengua | Alejandría, verano del 42 a. C., 8:00 | **Cocina**, representativo (dar de comer a la ciudad) | Abeja: el signo del rey del Bajo Egipto |
| **Las calculadoras de Langley** (`Microcosmos_Langley`, «Langley») | Johnson comprueba a mano la órbita de Glenn; Vaughan y FORTRAN; Jackson y el instituto | Hampton (Virginia), 19–20-2-1962, 9:47 | **Informática**, representativo | Pelotero (Johnson: las estrellas), hormiga (Vaughan), tijereta (Jackson) |
| **Beethoven** (`Microcosmos_Beethoven`, «Viena 1824») | El estreno de la Novena: hablarle por escrito, las particellas; Unger lo gira | Viena, 7-5-1824, 19:00 | **Música** (`Music_Area`, [piso-musica.md](piso-musica.md)) | Grillo: oye por las patas |
| **Descartes** (`Microcosmos_Descartes`, «Leiden») | Se imprime el Discurso del método: las cuatro reglas, la rejilla, dudar junto a la estufa | Leiden, 8-6-1637, 10:00 | **Aula**, representativo | Araña: una red de líneas rectas |
| **Ernesto Guevara** (`Microcosmos_CheGuevara`, «San Pablo») | La colonia de leprosos: curas sin guantes, el brindis, el río, la balsa | San Pablo de Loreto, 14-6-1952, 9:00 | **Hospital**, representativo | Langosta: cruza un continente |
| **Pedro Sánchez** (`Microcosmos_PedroSanchez`, «Madrid 2018») | Cómo funciona una moción de censura; la votación; la promesa | Congreso, Madrid, 1-6-2018, 11:00 | **Aula**, **pendiente**: «Sala de asambleas (parlamento)» | Abeja: el pleno cuenta apoyos (también Rajoy y Pastor: el insecto es por la institución) |
| **Lincoln** (`Microcosmos_Lincoln`, «Gettysburg») | Cuatro meses tras la batalla: Wills, Thorn, Biggs, la proclama y los dos minutos | Gettysburg, 19-11-1863, 11:00 | **Aula**, **pendiente**: «Sala de asambleas (parlamento)» | Mantis: alta, delgada y paciente |
| **Grandes hazañas de presidentes** (`Microcosmos_Hazanas`, «Feria de las Hazañas») | Una feria INVENTADA con 7 pabellones: Lee Kuan Yew, Washington, Roosevelt, Figueres, Khama, Mandela, Sirleaf | Inventado, hoy | **Aula**, **pendiente**: «Sala de asambleas (parlamento)» | Cada uno, el suyo; Lee: hormiga tejedora de Singapur |

Cada una lleva sus edificios de la historia con su actividad, su plano, su relleno y las cuatro categorías (trabajo, ocio, entrenamiento,
recuperación). Ninguna tiene `Milicia`.

## Las decisiones

### Lincoln: sí

Berón preguntó si fue tan bueno y, si no, el mejor presidente de EE. UU. **Decisión: Lincoln.** La encuesta C-SPAN a historiadores (142 en 2021) lo pone el **1.º en sus
cuatro ediciones** (2000, 2009, 2017, 2021), con Washington y F. D. Roosevelt detrás; el Siena College (2022) pone 1.º a **Roosevelt** y a Lincoln **2.º**,
con Washington 3.º. Cambia el método (C-SPAN, 10 cualidades de liderazgo; Siena, 20 categorías, con «suerte»). Ninguna lo baja del segundo puesto y nadie más
es primero en las dos; y su hazaña (mantener la Unión y acabar con la esclavitud) encaja con una escena civil. Sus sombras van en la ficha (habeas corpus, los 38
dakota en 1862, la «colonización» de los afroamericanos, que abandonó).

### El relato nativo americano: la Gran Ley de la Paz

Candidatas: la **Gran Ley de la Paz** (Haudenosaunee), el **juicio de Standing Bear** (Ponca, 1879), la **confederación de Tecumseh** (1805–1813), Sequoyah y el silabario
cherokee, Joseph y los nez perce. Se elige la Gran Ley porque:
- **resume**: no es la historia de un hombre contra los colonos, sino cómo se gobernaba un pueblo antes de ellos, y cómo sigue;
- **sus personajes impresionan** y son varios: una mujer que deja de alimentar la guerra (Jigonsaseh), un padre roto por el duelo que inventa el consuelo (Ayenwatha), el
  mediador sin nombre público, el enemigo que no se vence sino que se cura (Tadodaho);
- **es civil por naturaleza**: sus armas se entierran, y encaja en «sin violencia gloriosa»; Tecumseh es guerra;
- da **poder a las mujeres** (las madres de clan nombran y destituyen jefes) y piensa en **siete generaciones**;
- **sigue viva**: el consejo se reúne hoy en Onondaga. Standing Bear es muy buena segunda opción, pero es un caso judicial de un solo lugar y fecha; queda para otra tanda.

Cuidado con el respeto: es una tradición ajena y viva. La escena no recrea ceremonias ni palabras rituales; el Pacificador va por su título y no por su nombre
(muchos haudenosaunee lo reservan para la ceremonia); la ficha dice que las versiones varían y a quién pertenece.

### Pedro Sánchez, neutral

Comprobado el 2026-11-05: sigue siendo presidente; el **Real Decreto 806/2026** (BOE, 6-10-2026) disolvió las Cortes y hay **elecciones generales el 29 de noviembre de 2026**:
**esta ficha caduca entonces y hay que revisarla** (checklist). Berón lo ve «amable y humano»; aquí no se valora: la protagonista es la institución. Se eligió la moción de 2018
porque es un hecho constitucional con acta, y se enseña a votar. A los políticos vivos **no se les inventan frases**: hablan una letrada, un ujier y una periodista inventados;
Ana Pastor solo lee el recuento, y Sánchez solo dice la fórmula legal de la promesa (RD 707/1979). Sánchez y Rajoy llevan el mismo insecto.

### El líder asiático: Lee Kuan Yew

Berón: «el líder asiático que llevó a su país de la pobreza a la riqueza y la educación». Lo más probable: **Lee Kuan Yew**, de Singapur; **fue primer ministro (1959–1990), no
presidente**, y la ficha y sus frases lo dicen. Otros candidatos, descartados y citados en la ficha: Park Chung-hee (Corea del Sur: dictadura militar) y Deng Xiaoping (no era
presidente, y China no era riqueza por persona). Sus sombras también van: detenciones sin juicio, presión a la prensa, demandas a opositores. **A confirmar con Berón** si pensaba en él.

### Che Guevara, solo 1952

Solo el viaje y San Pablo. Ni armas ni uniforme; en la misión es «Ernesto» (la escena se llama `Microcosmos_CheGuevara` para encontrarla, y la ficha explica lo de «el Che» al final). Lo discutido, incluidas las
ejecuciones en Cuba, en una línea de la ficha, sin juicio.

### Galvarino, sin daño

Ni una herida en pantalla: mangas de lana, una ruka, una machi y una reunión. El castigo se dice en una frase en la ficha; lo de Millarapue lo cuenta el kimche. La ficha no repite la
leyenda de los cuchillos atados a los brazos (circula en relatos populares, y es justo la imagen que el encargo no quiere).

## Estilos nuevos (`MicroStyles`)

| Entrada | Región | Años | Cama · asiento · hogar | Entrena en | Frutales |
|---|---|---|---|---|---|
| `andes_republicano` | Andes | 1820–1960 | hierro · butaca · tulpa | `pelota_nacional` | capulí, tomate de árbol, chirimoya |
| `mapuche` | Mapuche | 1000–1900 | estera · wanco · kütral | `palin` | maqui, avellano chileno, murta |
| `inca` | Inca | 1400–1572 | estera · tiana · q'oncha | `warachikuy` | lúcuma, chirimoya, pacay |
| `haudenosaunee` | Haudenosaunee | 1000–1700 | estera · banco · fuego de pasillo | `tewaarathon` | ciruelo americano, nogal, arándano |
| `egipto_ptolemaico` | EgiptoPtolemaico | −332 a −30 | kliné · klismós · brasero | `gimnasio_griego` | palmera, higuera, granado, sicómoro |
| `virginia_1960` | VirginiaTidewater | 1945–1979 | moderna · cromo · eléctrica | `gimnasio_moderno` | melocotonero, caqui de Virginia, higuera |
| `viena_biedermeier` | Viena | 1780–1848 | madera · butaca · Kachelofen | `sala_esgrima` | albaricoquero, cerezo, peral |
| `provincias_1600` | ProvinciasUnidas | 1580–1700 | bedstede · banco · caldero | `sala_esgrima` | manzano, peral, ciruelo |
| `amazonia_1950` | Amazonia | 1900–1980 | hamaca · banco · tres piedras | `estadio` | aguaje, plátano, papayo, camu camu |
| `espana_actual` | EspanaActual | 2000– | moderna · cromo · vitro | `gimnasio_moderno` | almendro, olivo, granado |
| `exposicion_mundial` | Exposicion | 2000– | moderna · cromo · eléctrica | `gimnasio_moderno` | naranjo, mango, olivo |

Gettysburg reutiliza `norteamerica_victoriana`, con un patio de gimnasia propio (el nombre del catálogo, «Gimnasio de la YMCA», no encaja en 1863).

## Mapas

Los doce están **Hechos**. Los planos marcados «aproximado» o «ACORTADO» lo dicen en su comentario: Chimbacalle (el centro de Quito queda fuera), Lagunillas (**recreación**: no hay plano del lof), Cajamarca (Pultumarca
acercada), Onondaga (recreación de aldea sobre el lago real), Alejandría, Langley (Newport News acercada), Viena, San Pablo (río acortado a 280 m), Madrid (la Zarzuela, acercada), Gettysburg; Leiden es el plano de Blaeu simplificado; la Feria es inventada.

## Fuentes

Las de cada historia están en su ficha (`sources` de la misión); las de plano y bioma, en el comentario de su archivo. Lo comprobado en la web el 2026-11-05 y lo que no:

- **Alfaro.** Clark (1998); Ayala Mora (1994); El Telégrafo (2013); Vicepresidencia de Ecuador (2018); Wikipedia, «Empresa de Ferrocarriles Ecuatorianos» (contrato de 1897). [verificar] el día del clavo de oro (17 o ~18 de junio: las fuentes difieren; la ficha lo dice) y el decreto de los empleos para mujeres.
- **Galvarino.** Ercilla; Góngora Marmolejo; Bengoa (2003); biografiasyvidas.com. [verificar] la fecha de Lagunillas (septiembre u 8 de noviembre) y la forma de su muerte: difieren, y la ficha lo dice. No se pudo leer Memoria Chilena.
- **Atahualpa.** Hemming (1970); Xerez (1534); Betanzos (1551); D'Altroy (2014); Urton (2003); Boletín de Arqueología PUCP (2023). [verificar] la altura de la línea y las cifras del rescate, según autores; el ajedrez viene del Inca Garcilaso.
- **Haudenosaunee.** Oneida Indian Nation; NYSED; Onondaga Nation; Skä•noñh; Mann y Fields (1997); Parker (1916); Snow (1994).
- **Cleopatra.** Plutarco, *Antonio* 27; Séneca, *NQ* IV a 2.16; Plinio XVIII 57; Josefo, *Contra Apión* II 60; Roller (2010); Tyldesley (2008); Estela de Calímaco; Fraser (1972). No se pudo comprobar en la web el pasaje de Plutarco ni el de Séneca (de memoria: [verificar] número exacto). Reparto y gestos son recreación, y lo dice la ficha.
- **Langley.** NASA (biografías de Johnson, Vaughan y Jackson, leídas); Shetterly (2016); Space.com (2017), Skopinski y Johnson (1960). La anécdota de Miriam Mann es de Shetterly.
- **Beethoven.** Thayer-Forbes (1967); Albrecht (2024); Solomon (1998); History Today; WETA. La escena de Unger difiere según los testimonios (lo dice la ficha).
- **Descartes.** Discours (1637); Hatfield, SEP (leída); Gaukroger (1995); Watson (2002); ficha de subasta de Sotheby's (8 de junio). [verificar] que Van Schooten dibujó las figuras (de memoria; MacTutor). Descartado, por no poderse comprobar: la anécdota del zapatero Rembrantsz.
- **Guevara.** Guevara (1993); Granado (1978); Anderson (1997); International Leprosy Association; Rev. Cubana de Medicina Militar (2021). [verificar] sobre el libro: el número de enfermos, la fecha del cruce a nado y la orquesta (de memoria de los diarios; la web solo confirma la colonia, la orilla y la balsa).
- **Sánchez.** Constitución arts. 113–114; acta del Congreso; RD 707/1979; prensa del 1 y 2-6-2018; BOE/RD 806/2026, Newtral, El Debate (5 y 6-10-2026). Confirmado.
- **Lincoln.** Copia Bliss; NPS; Wills (1992); McPherson (1988); Foner (2010); C-SPAN (2021); Siena (2022); Quest for Freedom; Gettysburg Daily. Confirmado.
- **Hazañas.** Britannica y Lee (2000); Mount Vernon; SSA; Americas Quarterly, Nueva Sociedad; Acemoglu et al. (2003), Wikipedia «Seretse Khama» (leída); Mandela (1994); Nobel Prize. [verificar] las cifras de renta de Singapur con el Banco Mundial (500 → ~14 000 $ son de fuentes secundarias).

## Qué probar en Unity (lo hace el probador)

1. Con `SampleScene` abierta y **de una en una** (W5, memoria): «Build Microcosmos de historias 3/<figura>»; orden en la lista de abajo. Después «Build Sala de Eras». Guardar SampleScene si se tocó. El pueblo de cada escena es del tamaño de los de los famosos.
   Orden: Galvarino, San Pablo (Hospital, ya probado) → Descartes, Madrid, Gettysburg, Feria, Onondaga (Aula) → Cajamarca (Textil) → Langley (Informática) → Chimbacalle (Mecánica) → Alejandría (Cocina: el piso más grande) → **Viena al final** (la Sala de Ballet nunca se ha montado como microcosmos).
2. Consola: `[Pueblos] <pueblo>: …`, `[Reglas] Microcosmos_X: OK`, `[Pisos] …: OK` (o «PENDIENTE»), `[Mapa] X: mapa a mano (…)` sin «no está en su bioma».
3. Grupos **78** (datos, sin escenas), **79** (las 12), y si hay tiempo 43, 63 y 87.
4. Fotos: el protagonista; el sitio clave de cada historia (la Sala del rescate y su línea, el Árbol de la Paz, el hemiciclo con su marcador, el estrado de Gettysburg, la rejilla de Leiden); el pueblo llenando el piso; ningún muro ni techo; nada flotando; el río de San Pablo y el golfo de Arauco sin tapar edificios.

## Riesgos

- Viena va en el piso de Música ([piso-musica.md](piso-musica.md)): hay que reconstruir SampleScene antes.
- Los Prop con collider de cada historia (los leones, el hemiciclo, el tablado) deben dejar paso a los objetivos: mirar que `Collect` y `Hold` se puedan alcanzar.
- En **`Natural`**, si una pieza del piso se llama de otra forma, cae a su catálogo: en Onondaga podría quedar una fábrica. Mirar las fotos.
- El Hospital y el Aula cargan ya con muchas historias: reconstruir de una en una, nunca todas a la vez (W5).
- La ficha de Sánchez **caduca el 29-11-2026**.

## Pendiente

- Sala de asambleas: si un día se crea, re-apuntar Onondaga, Madrid, Gettysburg, Feria y Angostura (la alerta de piso nuevo avisará).
- Standing Bear, en otra tanda.
