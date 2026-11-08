# Fichas de personajes

Berón (2026-11-08): «hace falta una lista de personajes y que cada uno tenga su ficha, para ver qué personajes están más completos que
otros y equilibrarlos. Los primeros que se crearon dan más información que otros, y hay que seguir rellenándolos todos. Poned enlaces que
repartan la información de los personajes por el código».

Reglas: [manual-de-diseno.md §9.1](../manual-de-diseno.md) (P2, P10 y P22) y [reglas-personajes.md](../reglas-personajes.md).

## La plantilla

Cada ficha tiene estas 11 secciones, en este orden. Copia [linduarda.md](linduarda.md) para empezar una nueva.

| # | Sección | Qué lleva |
|---|---|---|
| 1 | Identidad y aspecto | Quién es, de dónde, edad si se sabe; aspecto de su foto (P3) |
| 2 | Origen e historia | Su vida antes del santuario y cómo llegó |
| 3 | Personalidad y frases | Carácter, gustos, dichos; frases del juego, con su veracidad (M39) |
| 4 | Stats, margas y aptitudes | Cuerpo, mente, arquetipo, anclas, margas |
| 5 | Profesión y lo que sabe hacer | Su profesión con su base (P7) y sus saberes |
| 6 | Puesto y áreas | Dónde sale, qué misiones le da su profesión, qué cambió |
| 7 | Relaciones | Pareja, familia, amigos, casa |
| 8 | Horario y costumbres | Gimnasio, horario libre, rutinas |
| 9 | Microcosmos y santuario | Su insecto, sus misiones del Microcosmos, su santuario y nivel |
| 10 | En el código | Dónde vive cada dato suyo en el código, con enlace a la línea |
| 11 | Preguntas para Berón | Lo que falta o se contradice |

**Cada dato lleva su veracidad y su fuente** (M39, [veracidad-frases.md](../veracidad-frases.md)):

| Veracidad | Cuándo |
|---|---|
| **documentada** | Lo dijo Berón: sus mensajes o la ficha larga que contó él |
| **atestiguada** | Se ve en su foto o captura |
| **recreada** | Decisión de juego hecha a partir de un dato suyo (stats, insecto, plan de gimnasio…) |
| **inventada** | La puso el juego o un agente, sin base (aspecto provisional, estatura media del país…) |
| ⚠️ | La ficha larga lo marca «confirmar» |
| **Falta** | No se sabe. **No se inventa** (P1) |

Las fuentes son un doc y su sección o un fichero de código y su línea. En el código, cada sitio donde se define una persona lleva un
comentario «ficha: docs/personajes/…». La lista de personas sale de `Professions.Mesocosmos` y el nombre del fichero, de
`Professions.DocName` ([Professions.cs:99](../../Assets/Scripts/Progression/Professions.cs#L99), [:110](../../Assets/Scripts/Progression/Professions.cs#L110)).
`tools/check-personajes.sh` y la prueba 95 comprueban que cada una tenga su ficha.

## Lo completas que están (2026-11-08)

**L** = lleno (varios datos con fuente y sin un hueco importante), **P** = parcial, **F** = falta. Puntos: L = 2, P = 1, F = 0, sobre 18
(las secciones 1 a 9). Ordenadas de la más llena a la más vacía; el número de la izquierda es su orden de creación.

### Personas del santuario (mesocosmos)

| Orden | Persona | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | Puntos | Huecos principales |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 7 | [Irosene](irosene.md) | L | L | L | L | L | L | L | P | P | **16** | nacionalidad; su santuario y nivel; diálogos vacíos |
| — | [La Magnate](magnate.md) | L | L | P | L | L | L | L | P | L | **16** | su nombre y edad; su trato con la gente; su origen |
| 1 | [Kushal](kushal.md) (jugador) | L | P | L | P | L | L | L | P | L | **15** | nacionalidad y edad; su vida antes; sus gustos |
| 2 | [Linduarda](linduarda.md) | L | L | L | P | L | L | P | P | L | **15** | edad y llegada; aptitudes y frases en el código; su profesión no tiene entrada |
| 6 | [Gohageneis](gohageneis.md) | L | L | L | L | P | L | P | L | P | **15** | si da artes marciales; cómo llegó; su familia |
| 9 | [Andrea](andrea.md) | L | L | L | P | L | L | P | P | P | **14** | edad; pareja o familia; aptitudes mentales |
| 4 | [Goluis](goluis.md) | P | L | L | L | P | L | P | P | P | **13** | nacionalidad; si sigue de jefe de cocina; su hija |
| 5 | [Panterilia](panterilia.md) | P | P | L | L | L | P | P | P | P | **12** | foto y nacionalidad; cómo llegó; su hija y su «dependencia química» |
| 30 | [Consol](consol.md) | L | F | P | P | L | L | P | L | P | **12** | foto; su historia; su carácter |
| 3 | [Lizmibet](lizmibet.md) | L | P | F | P | P | L | P | P | P | **10** | carácter y frases; su vida de médica; su profesión no tiene entrada |
| 15 | [Josedrilo](josedrilo.md) | P | P | F | P | L | L | P | L | F | **10** | quién es quién en `cumbaya.jpg`; si es el de la ficha de Irosene; su carácter |
| 19 | [Adaguchi](adaguchi.md) | L | F | F | P | L | L | P | L | F | **10** | su historia; su carácter; ninguna misión de submarino es suya |
| — | [Neo](neo.md) (niño) | P | F | F | P | L | P | L | P | P | **9** | su edad; su carácter; si va con su madre o su padre |
| 8 | [Tote](tote.md) (`JefeConstruccion`) | P | F | F | P | L | L | P | P | P | **9** | toda su historia; su carácter; edad y país |
| 14 | [Ducristin](ducristin.md) | L | F | F | P | P | L | L | P | F | **9** | su historia; qué enseña; su carácter |
| 13 | [Alicita](alicita.md) | P | P | F | P | L | L | P | P | F | **9** | su origen y su máster; su carácter; ¿es la «Alicia» de la ficha de Irosene? |
| 28 | [Luciana](luciana.md) | L | F | F | P | L | L | P | P | F | **9** | dónde aprendió a volar y a pelear; su carácter; sus stats no muestran las artes marciales |
| 31 | [Fernando](fernando.md) | P | F | F | P | L | L | P | P | P | **9** | foto; su historia; su carácter |
| 27 | [Ivelyna](ivelyna.md) | L | F | F | P | L | L | P | P | F | **9** | su historia; su carácter; la guardería no existe |
| 21 | [Julian](julian.md) | P | P | F | P | L | P | L | P | F | **9** | foto; su carácter; su máster |
| 16 | [Maite](maite.md) | P | F | F | P | L | L | P | P | F | **8** | quién es quién en `cumbaya.jpg`; si hace curas del hospital; su historia |
| 11 | [Amykhuz](amykhuz.md) | P | F | F | P | L | L | P | P | F | **8** | su historia; qué química; su carácter |
| 17 | [Shalva](shalva.md) | P | F | F | P | L | L | P | P | F | **8** | `cumbaya.jpg`; su historia; su carácter |
| 29 | [Malena](malena.md) | L | F | F | P | P | L | P | P | F | **8** | su historia; su carácter; no está en `Professions.Medical` |
| 26 | [Lale](lale.md) | L | F | F | P | L | P | P | P | F | **8** | su historia; su carácter; si la Biología es suya |
| 22 | [Fausto](fausto.md) | P | F | F | P | L | P | L | P | F | **8** | foto; su historia; su carácter |
| 12 | [Karen](karen.md) | P | F | F | P | P | L | P | P | F | **7** | qué lenguas y qué asuntos legales; su historia; su carácter |
| 20 | [Gallarle](gallarle.md) | L | F | F | P | F | P | L | P | F | **7** | su profesión; su historia; su carácter |
| 25 | [Yulo](yulo.md) | P | F | F | P | L | P | P | P | F | **7** | su historia; su carácter; nada lo distingue de Lale |
| 10 | [Nesvitor](nesvitor.md) | P | F | F | P | P | L | F | P | F | **6** | quién es para Berón; su historia; su carácter |
| 23 | [David](david.md) | P | F | F | P | P | P | P | P | F | **6** | foto; su historia; qué parte de la informática |
| 24 | [Adonis](adonis.md) | P | F | F | P | P | P | P | P | F | **6** | foto; su historia; qué parte de la informática |

**Lo que se ve**: Berón tiene razón. Los ocho primeros (Kushal, Linduarda, Lizmibet, Goluis, Panterilia, Gohageneis, Irosene, la Magnate)
tienen ficha larga, arquetipo o conducta propia y suman 10–16. Andrea tiene ficha larga (14). Los de las fotos del 2026-11-04 y 05 tienen
aspecto y profesión, pero **casi nada en historia (2), carácter (3) y microcosmos (9)**: suman 6–10. Lizmibet y Tote, aunque llegaron
pronto, se quedaron sin ficha larga propia.

**Para equilibrar, lo primero que hay que pedir a Berón**, para todos los de 6–10 puntos: de dónde es cada uno, cómo llegó, cómo es y
qué le gusta (dos o tres rasgos y un dicho), y las fotos que faltan (David, Adonis, Julian, Fausto, Consol, Fernando y Panterilia).

### Otras figuras (fuera del santuario)

No cuentan para equilibrar: viven en su microcosmos (J12) o solo en la historia.

| Figura | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | Puntos | Nota |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [Sadhguru](sadhguru.md) | L | L | P | P | L | L | P | P | L | 14 | persona real; solo su vida pública |
| [Elon Musk](elonmusk.md) | L | L | P | P | L | L | P | F | L | 13 | persona real |
| [Bill Gates](billgates.md) | L | L | P | P | L | L | P | F | L | 13 | persona real |
| [Warren Buffett](warrenbuffett.md) | L | L | P | P | L | L | P | F | L | 13 | persona real |
| [MacKenzie Scott](mackenziescott.md) | L | L | P | P | L | L | P | F | L | 13 | persona real |
| [Ibrahim Traoré](ibrahimtraore.md) | L | L | P | P | L | L | P | F | L | 13 | persona real, político vivo: sin frases inventadas (J6) |
| [Goku](goku.md) | L | P | P | P | L | L | P | F | L | 12 | ficción: homenaje |
| [Leo](leo.md) | F | L | P | F | P | P | L | P | P | 9 | solo en las historias; no tiene cuerpo en el juego |

Los personajes históricos y los insectos-humanos de cada microcosmos (Newton, Curie, Bruce Lee, Ambrosio, Sakshi, el grupo fundador…)
tienen su ficha en el doc de su microcosmos: [microcosmos-famosos.md](../microcosmos-famosos.md),
[microcosmos-historias.md](../microcosmos-historias.md), [microcosmos-historias-3.md](../microcosmos-historias-3.md),
[microcosmos-musica.md](../microcosmos-musica.md), [founding-trio-stories.md](../founding-trio-stories.md),
[mob-characters.md](../mob-characters.md).

## Contradicciones entre fuentes (2026-11-08)

Están en la sección 11 de cada ficha, como preguntas. Las principales:

1. **La familia de Irosene y los nombres del santuario.** La ficha de Irosene nombra a un primer esposo «Néstor», a una hija «Alicia»,
   exesposa de Josedrilo, a Josedrilo padre de Leo y a Lizmibet consuegra ⚠️. En el santuario están Nesvitor «Néstor», Alicita «Alicia»
   (pareja de Julian) y Josedrilo (pareja de Maite). No se da por hecho que sean las mismas personas: pregunta a Berón.
2. **Médicas sin profesión.** Linduarda y Lizmibet no tienen entrada en `Professions.All`; `Professions.Medical` no incluye a Malena,
   aunque [reglas-personajes.md](../reglas-personajes.md) cuenta 4 médicas.
3. **Linduarda y los animales.** Su ficha dice Una Salud, pero la jornada la excluye de cuidar a los animales ingresados.
4. **El santuario y el nivel de Irosene**: Tier 4 submarino, «3er santuario V1» o, según la regla del 2026-11-08, por encima de
   santuario 1 nivel 1. En el juego vive en el santuario 1 como todos.
5. **Goluis, jefe de cocina** según su ficha y `KitchenBoss`, pero Berón nombró jefes a Maite y Amykhuz.
6. **Panterilia, limpieza y farmacia** según su ficha vieja; Berón solo dijo baile y finanzas. Sus anclas `protect_daughter` y
   `chemical_reliance` no tienen fuente escrita.
7. **Gohageneis y las artes marciales**: las da en días alternos con Luciana sin base de Berón; la experta documentada es Luciana.
   Su elemento es Agua en un comentario de `PhrasePools` y Fuego en su arquetipo.
8. **Tote**: su rótulo dice «jefe de construcción», pero desde el 2026-11-04 es jefe de reparaciones y guardia.
9. **Andrea** está en la lista de nombres femeninos de los microcosmos (`AreaMicrocosmos.FemaleNames`), y es un hombre.
10. **Textos viejos** de los planes de gimnasio (Nesvitor «el mayor de las cuentas», Ducristin «mecánico», Alicita «huerto», Maite
    «registros y vacunación») y de `SanctuaryDirector` (Nesvitor «consejero», Ducristin «la mecánica»).
11. **Aspectos provisionales distintos** en el código y en [diseno-personajes.md](../diseno-personajes.md): Julian y Fausto.
12. **Fernando** lleva toda el área de Energía (también las patrullas `ene_`), y Berón solo dijo paneles solares; no está en
    `Professions.Unconfirmed` (P7).
13. **Traoré** guarda un aspecto militar (boina y uniforme) aunque su microcosmos es solo civil (J6).

## Cómo se mantiene

- **Al llegar alguien**: su ficha nueva desde la plantilla, su fila en esta tabla y el comentario «ficha:» en cada sitio del código donde
  se define (lista de [reglas-personajes.md](../reglas-personajes.md) §Lista para añadir a alguien).
- **Al saber algo nuevo** (un mensaje de Berón, una foto, un cambio de puesto): se escribe en su ficha con su veracidad y su fuente, se
  quita el «Falta» o la pregunta que contesta y se recalcula su fila.
- **Al cambiar el código** de una persona: se actualiza el enlace de la sección 10 (los números de línea cambian).
