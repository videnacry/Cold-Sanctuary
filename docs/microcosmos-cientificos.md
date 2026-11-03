# Microcosmos de científicos e históricos (2026-11-01)

Berón (2026-11-01): «debes agregar al microcosmos Newton, Einstein, Tesla, Curie, Nietzsche, Bolívar, Pitágoras, Aleksei
Ananenko con su escuadrón, Edward Jenner, Clair Cameron Patterson, Heródoto y al-Khwarizmi; debes investigar las historias
del microcosmos para poder recrear los pueblos en que suceden». Doce microcosmos nuevos, cada uno en **el pueblo o la
ciudad donde ocurrió**, que llena **el piso entero** (regla 5 de [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md)),
habitado **solo por insectos-humanos civilizados**, con flora y fauna salvajes **solo donde la historia y el lugar las
tenían**.

## Código

| Pieza | Archivo | Qué hace |
|---|---|---|
| Las 12 historias | `Assets/Editor/StorySceneBuilder.Scientists.cs` | Misión (`StoryMission`), sitios del claro, gente (`Who` = su insecto, a su estatura, con su sexo real) y ficha histórica con fuentes (`HistoryCard`). Menú «Build Microcosmos de historias: científicos e históricos (12)». |
| Los 12 pueblos | `Assets/Editor/CivicBuilder.Towns.cs` | `Town`: reparte el piso (`AreaMicrocosmos.FloorRectLocal`) en manzanas con calles de `MicroBuildRules.Street` (4 m), fuera del claro de la historia (radio 26 m). Los edificios de la historia van a las manzanas más cercanas al claro; el resto se llena con solares de la era (casas, tiendas, talleres, huertos), hasta 120 edificios en manzanas densas (del tamaño de su edificio más grande + paso + calle); en los pisos enormes (Angostura, Prípiat) las manzanas más lejanas quedan de campo. Gente con jornada (`Worker`) y parejas con hijos (`Townsfolk`/`Couple`). |
| Fecha y lugar | `Era.cs` (`Eras.OfMicrocosmos`) y `StorySceneBuilder.Dates` | Año, lugar, fecha y hora (el sol) de cada uno. |
| Flora y fauna | `MicroBiome.cs` | Campo: la del lugar y la estación. **Ciudades sin naturaleza salvaje** (París 1898, Berna 1905, Pasadena 1953, Bagdad ~820): solo lo plantado (`Biome.town`) y los animales del pueblo con el cuerpo de su análogo (`Biome.townFauna`, docs/microcosmos-fauna-eras.md), ningún bicho suelto. Toda la flora de pueblo, a escala humana y fuera de calles y solares (reglas de edificación, regla 5). |
| Su insecto | `InsectCast.cs` | Elegido por etología (tabla abajo). |
| Entradas | `VirtualizationMachine.StoryScenes` (la máquina del área), `EraHubSceneBuilder.All` (Sala de Eras) | Química, Biología y Central Nuclear tienen su máquina (solo virtualización + sus historias) desde 2026-11-03: antes Curie, Patterson, Jenner y Chernóbil no tenían entrada en el juego. |
| Prueba | `CivilizedErasTest` (grupo 62) | Carga las 12 escenas: edificios, jornadas, especies y familias. |

## Los doce

| Microcosmos (escena) | Pueblo, fecha y hora | Área del santuario | Insecto (por qué) | Flora y fauna |
|---|---|---|---|---|
| **Pitágoras** (`Microcosmos_Pitagoras`) | Crotona (Calabria), verano ~530 a. C., 7:00 | Gimnasio (Milón y el ternero: la sobrecarga progresiva) | Cigarra: las periódicas salen cada 13 o 17 años, números primos; canta | Maquis mediterráneo: olivo, lentisco, tomillo, habas; cigarra del fresno, hormiga segadora, abeja, mantis, escorpión |
| **Heródoto** (`Microcosmos_Herodoto`) | Menfis y el Nilo, verano (inicio de la crecida) ~450 a. C., 8:00 | Huerto («Egipto es un don del río») | Escarabajo sagrado (el fénix y la bola de mirra, II.73) | Papiro, palmera, loto azul, emmer; escarabajo sagrado, langosta del desierto, *Cataglyphis*, escorpión amarillo |
| **al-Juarismi** (`Microcosmos_AlJuarismi`) | Bagdad, la Ciudad Redonda, ~820, 9:00 | Construcción (medir la tierra, cavar canales) | Abeja: su danza calcula ángulo y distancia; celdas hexagonales | Ciudad: palmeras, granados, naranjos amargos, rosales y arrayán plantados; animales del pueblo: asnos de aguador y camellos (pulgones), gatos (cochinillas hormigueras) |
| **Newton** (`Microcosmos_Newton`) | Woolsthorpe (Lincolnshire), verano de 1666, 16:00 | Planta Atómica (escalera de la física) | Araña (ya en `InsectCast`) | Prados de ovejas, manzanos, majuelo; pulgón del manzano, tijereta en las manzanas, abejorro, afodio |
| **Jenner** (`Microcosmos_Jenner`) | Berkeley (Gloucestershire), 14 de mayo de 1796, 10:00 | Biología | Hormiga: se inmunizan unas a otras con dosis pequeñas de esporas (Konrad et al. 2012) | Prados lecheros en primavera; **las vacas son pulgones** (Blossom); abejorro, afodio en la boñiga |
| **Bolívar** (`Microcosmos_Bolivar`) | Angostura del Orinoco, 15 de febrero de 1819, 10:00 | Cocina (el casabe de la tropa) | Langosta: cruza montañas en enjambre (los Andes) | Sabana seca, moriche, araguaney, lajas de granito; bachaco, comején, cocuyo |
| **Nietzsche** (`Microcosmos_Nietzsche`) | Sils Maria (Alta Engadina), principios de agosto de 1881, 11:00 | Sala de Yoga (caminar y respirar) | Mosca escorpión de la nieve (*Boreus*): sola en el frío de las alturas | Prado alpino, rododendro, genciana, pino cembro; saltamontes alpino, abejorro alpino |
| **Curie** (`Microcosmos_Curie`) | París, rue Lhomond, diciembre de 1898, 16:30 | Química | Luciérnaga (ya en `InsectCast`) | Ciudad: plátanos y castaños sin hoja, céspedes, boj, hiedra; animales del pueblo: caballos de fiacre (pulgones), gatos (cochinillas hormigueras), ratas (cucarachas) |
| **Tesla** (`Microcosmos_Tesla`) | Colorado Springs, 3 de julio de 1899, 18:00 | Construcción (la electricidad) | Abejorro: siente el campo eléctrico de las flores (Clarke et al. 2013) | Pradera de hierba corta, yuca, nopal; saltamontes, hormiga segadora, mariquita convergente |
| **Einstein** (`Microcosmos_Einstein`) | Berna, mayo de 1905, 8:00 | Planta Atómica | Grillo (ya en `InsectCast`) | Ciudad: tilos, castaños en flor, céspedes, boj, geranios; animales del pueblo: caballos de reparto (pulgones), gatos (cochinillas hormigueras) |
| **Patterson** (`Microcosmos_Patterson`) | Pasadena (y Argonne), 1953, 10:00 | Química | Cochinilla: acumula plomo (bioindicador) | Ciudad: palmeras, cedros, encinas, céspedes, rosales, pitósporo; animales del pueblo: perros y gatos (cochinillas hormigueras) |
| **Chernóbil** (`Microcosmos_Chernobil`) | Prípiat y la central, principios de mayo de 1986, 22:00 | Central Nuclear | Termitas soldado: cierran la brecha del termitero | Pinar de Polesia en primavera (el «bosque rojo»): pino, abedul, musgo; hormiga roja, hormiga león |

## Los pueblos (lo que se recrea y de dónde sale)

| Pueblo | Edificios de la historia | Solares de relleno | Fuentes |
|---|---|---|---|
| Crotona | Homakoeion (la escuela pitagórica), palestra de Milón, templo de Hera Lacinia, casa de Milón, herrería | casas griegas de patio, ágora, cerámica, olivar | Burkert 1972; Huffman, SEP; Polibio II.39 |
| Menfis | Templo de Ptah, puerto del Nilo, campos de la crecida con shaduf, panadería-cervecería, casa del escriba | casas de adobe, lino, alfarería, granero | Heródoto II; Lloyd 1975–88; Jeffreys, *Survey of Memphis* (EES) |
| Bagdad | Casa de la Sabiduría, tribunal del cadí, zoco de al-Karj, molino de papel, casa de comidas, bimaristán | casas de patio, talleres, mezquitas de barrio, huertos de palmeras | Lassner 1970; Gutas 1998; Bloom 2001 |
| Woolsthorpe | Woolsthorpe Manor, granero, redil, iglesia de Colsterworth, posada | cabañas de paja, herrería, huertos | National Trust; Westfall 1980 |
| Berkeley | The Chantry, granja lechera, casa de los Phipps, iglesia de Santa María, posada | cabañas, quesería, prados, herrador | Jenner Museum; Jenner 1798; Riedel 2005 |
| Angostura | Casa del Congreso, imprenta del Correo del Orinoco, cocina de la tropa, puerto, catedral, hospital de sangre | casas coloniales, pulperías, conucos | Lynch 2006; Casa del Congreso de Angostura |
| Sils Maria | Casa Durisch (Nietzsche-Haus), tienda de Durisch, fonda, prado de la orilla, iglesia | casas engadinesas, establos, hotel | Nietzsche-Haus; Safranski 2000 |
| París | Cobertizo de la rue Lhomond (ESPCI), casa de la rue de la Glacière, Academia de Ciencias, café, panadería | inmuebles de piedra, tiendas, talleres | Curie 1923; Quinn 1995; Musée Curie |
| Colorado Springs | Estación experimental de Knob Hill, central de El Paso Electric, Alta Vista Hotel, estación del tren, taller de Czito | casas de tablas, almacén, herrería | Tesla 1978 (*Colorado Springs Notes*); Carlson 2013 |
| Berna | Kramgasse 49, Oficina de Patentes, café de la Academia Olimpia, panadería, relojería | casas de soportales, tiendas, escuela | Einsteinhaus Bern; Isaacson 2007; Galison 2003 |
| Pasadena | Laboratorio limpio de Caltech, sala del espectrómetro (Argonne), gasolinera «Ethyl», diner, escuela | casas de estuco, tiendas, taller mecánico | Caltech Archives (Patterson 1995); Needleman 2000 |
| Prípiat | Sótano del bloque 4 (válvulas), parque de bomberos, Hotel Polissia (la comisión), cantina, dosimetría, parque de atracciones | bloques de viviendas evacuados, tienda, escuela | Plokhy 2018; Leatherbarrow 2016; IAEA INSAG-7 |

Los solares de relleno llevan 2–3 muebles de su oficio (puesto + estantería + cajonera, banco + fragua + armario, bancal + pozo…), no uno solo: con uno, el pueblo se veía vacío de lejos (prueba en el PC, 2026-11-03). Todos los edificios usan `CivicBuilder.Building` (abiertos, muebles detallados con el paso de 2 m y, en cada Casa, las
necesidades básicas: cama, retrete, lavabo, mesa con sillas y cocina).

## Cosas a decidir / notas

- **Planta Atómica** (`Microcosmos_PlantaAtomica`, de `PhysicsSceneBuilder`) sigue juntando Woolsthorpe, París y Berna en
  un collage. Ahora Newton, Curie y Einstein tienen cada uno su pueblo: se puede retirar el collage o dejarlo como resumen.
- **Hechos discutidos** que las fichas marcan como tales: la leyenda de los martillos (falsa como física), el
  teorema anterior a Pitágoras, los números de Heródoto sobre las pirámides, el tranvía de Einstein (anécdota sin
  documento), el día exacto de la bajada a la piscina de burbujeo (las fuentes difieren) y el mito de que el escuadrón
  murió al poco (falso).

## Qué probar en Unity (pendiente)

1. Con `SampleScene` abierta: «Tools/Cold Sanctuary/Build Microcosmos de historias: científicos e históricos (12)» y
   «Build Sala de Eras».
2. Consola: `[Pueblos] <pueblo>: n edificios de la historia y m solares…` y `[Reglas] Microcosmos_X: OK`.
3. Pruebas: grupo 62 (`CivilizedErasTest`) y 63 (`MicroBuildRulesTest`).
4. A ojo: entrar por la máquina de cada área (p. ej. Química → «Marie y Pierre Curie»): Kushal llega como hormiga, el pueblo
   llena el piso, sin muros ni techos, y la misión se completa.
