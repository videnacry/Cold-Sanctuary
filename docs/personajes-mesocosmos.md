# Personajes del mesocosmos (santuario): quién vive aquí y quién se fue a su microcosmos

## 2026-11-04: los famosos salen a sus microcosmos

Berón: «los personajes famosos, elon musk, sadhguru, y demás sácalos del mesocosmos y crea sus microcosmos respectivos».

**Salen del santuario** (rama `claude/edif-personajes`):

| Id | Quién | Qué hacía en el santuario |
|---|---|---|
| `Sadhguru` | Sadhguru (persona real) | Maestro de yoga: la tarima de Hatha, la meditación junto al linga |
| `ElonMusk` | Elon Musk (persona real) | Mecánica: vehículos y maquinaria |
| `Goku` | Goku (ficción, Dragon Ball) | Entrenador de artes marciales; «Artes marciales con Goku» en el horario libre |
| `BillGates` | Bill Gates (persona real) | Hospital: registros y vacunación |
| `WarrenBuffett` | Warren Buffett (persona real) | Las cuentas del santuario, desde el restaurante |
| `MacKenzieScott` | MacKenzie Scott (persona real) | Huerto: alimentar a los visitantes |
| `IbrahimTraore` | Ibrahim Traoré (persona real) | Coordinar la guardia |

Se quitaron de:
- la escena del santuario (`MakeCharacter`) y `PlaceStaff`;
- `WorkdayDirector.staff` y `SanctuaryDirector.HomeArea`;
- las casas del árbol (`ResidentialBuilder.Owners`);
- el horario libre (sale de `WorkdayDirector.staff`; la actividad de artes marciales cambió de maestro);
- la galería de retratos (el menú «los seis del mesocosmos» ya no existe);
- los turistas del restaurante (`RestaurantService.TouristBodies`, `RestaurantServiceGame`);
- las pruebas que los buscaban.

**Se conservan para sus microcosmos:**
- su aspecto en `CubeHair.For` (por el id);
- sus datos en `SampleSceneBuilder.MesoCharacters` (id, rótulo, ropa y stats).

Sus microcosmos (rama `claude/edif-famosos`): [microcosmos-famosos.md](microcosmos-famosos.md): Sadhguru en Velliangiri (1999), Elon Musk en Hawthorne (2008), Goku en una aldea de montaña (homenaje), Bill Gates en Albuquerque (1975), Warren Buffett en Omaha (1983), MacKenzie Scott en Prairie View (2020) e Ibrahim Traoré en Uagadugú (2024).

**Otras figuras famosas** buscadas en las listas de personal y en los constructores del santuario: ninguna más.
- Las estatuas de los pisos de yoga (Buda, Ganesha, Adiyogi, Patanjali) son decoración, no personal.
- Newton, Curie, Einstein, Bruce Lee y los demás personajes históricos ya viven en sus microcosmos.

## Profesiones y parejas (Berón, 2026-11-04) — lo que vale ahora

Berón: «Josedrilo y Maite son pareja, ¿Lizmibet y su esposo comparten residencia?; Ducristin tiene de pareja a Gallarle, el
profesional de yoga es Josedrilo y también es profesional de bioconstrucción; Maite es profesional de cocina, Shalva es profesional de
cultivo y bioconstrucción, Adaguchi es profesional de submarinismo y cocina, Amahuz es profesional de cocina y química; Karen es
profesional de asuntos legales y lenguas; Alicita es profesional de contabilidad y project-manager».

**Esto sustituye al reparto de papeles de abajo**, que fue provisional.

### Profesiones (`Professions`)

La profesión da a cada uno su papel sin quitar la rotación (regla J2):
- cada día, antes de las vueltas por áreas, `WorkRoster` le da **una misión de cada área de su profesión** (la que menos ha hecho);
- en una misión de grupo que se **dirige** (la clase de yoga), el profesional del área, si está libre, entra en el grupo y la dirige;
- cada uno **empieza** en el área de su profesión principal (`SanctuaryDirector.HomeArea`, `PlaceStaff`), y su rótulo la dice.

| Persona | Profesión | Áreas en la jornada | Sale en |
|---|---|---|---|
| **Josedrilo** | **Maestro de yoga** (`WorkdayDirector.YogaTeacher`: guía la clase de Hatha, medita junto al linga) y bioconstrucción | Sala de Yoga, Construcción | la tarima de Hatha tradicional |
| Irosene | Segunda maestra de yoga (`SecondYogaTeacher`: la otra clase cuando hay dos a la vez) y su textil | — | Estudio Textil |
| **Maite** | Cocina | Cocina | el restaurante |
| **Shalva** | Cultivo y bioconstrucción | Huerto, Construcción | el huerto |
| **Adaguchi** | Submarinismo (piloto del submarino de obra y de la ambulancia, tareas bajo el agua de los árboles) y cocina | Construcción, Cocina | Construcción |
| **Amahuz** | Cocina y química | Cocina, Química | el restaurante |
| **Karen** | Asuntos legales y lenguas | Aula: misiones nuevas `aul_legal` (contratos, permisos, normas y acuerdos, en una mesa de la sala de estudio) y `aul_lenguas` (traducir y dar clase de idiomas) | el Aula |
| **Alicita** | Contabilidad y dirección de proyectos | Cocina (misión nueva `coc_cuentas`: las cuentas del santuario, que llevaba Nesvitor), Construcción (misión nueva `con_proyectos`: dirigir las obras) | Construcción |
| Ducristin | Mecánica (vehículos y maquinaria) | Mecánica | el taller |
| Nesvitor | **Consejero**: personal general, sin área fija (deja las cuentas a Alicita) | — | el comedor |
| Gallarle | **Pendiente de foto y de profesión**: personal general | — | con Ducristin |
| Andrea | Deja el yoga; su papel de **entrenador del gimnasio** lo pone la rama del entrenamiento | — | el comedor |
| Gohageneis | Guardia (patrulla) y artes marciales; ya no tiene a Josedrilo en la guardia | — | — |

- **Karen en el Aula**: aún no hay un área de asuntos legales. El Aula tiene mesas y biblioteca en su sala de estudio para el trabajo
  de despacho, y pizarra para las clases de lenguas. Y está en la torre del Entrenamiento, que usan todos cada día.
- **Registros y vacunación del hospital** (eran de Maite): vuelven a las médicas, Lizmibet y Linduarda. Siguen siendo misiones del
  Hospital en la rotación.
- **Huerto para los visitantes** (era de Alicita): lo lleva Shalva, profesional de cultivo, con Panterilia.

### Parejas: una casa del árbol por pareja (`ResidentialBuilder.Partners`, `Bedroom.partner`)

| Casa | Viven | Nota |
|---|---|---|
| Lizmibet y Tote | Lizmibet y `JefeConstruccion` | antes, cada uno en la suya |
| Amahuz y Karen | Amahuz y Karen | de su foto: **pendiente de que Berón lo confirme** |
| Ducristin y Gallarle | Ducristin y Gallarle | Gallarle es nuevo |
| Josedrilo y Maite | Josedrilo y Maite | antes, cada uno en la suya |

- Hay **16 casas ocupadas de 18**: quedan 2 libres para recién llegados.
- Duermen allí **20 residentes** (16 dueños y 4 parejas).
- El ring de almohadas se mide con ellos (⌈√20⌉ = 5: caben 25) y `AreaCapacity` también.
- Miembros del santuario: 19 con Kushal (`WorkdayDirector.Staff`). Con la regla J16 siguen saliendo 2 pisos de Hatha y 2 de Ashtanga.

### La jornada (regla W9)

- La ronda de las profesiones va antes que las vueltas por áreas. La rotación de siempre (la misión que menos has hecho) reparte el
  resto, así que todos siguen aprendiendo todo.
- Hay **4 misiones nuevas** (`aul_legal`, `aul_lenguas`, `coc_cuentas`, `con_proyectos`), 3,5 h al día. Son 19 personas × 5 h: sobra
  sitio.
- La brigada del restaurante de `RestaurantServiceTest` lleva a los profesionales de cocina:
  - Goluis, Maite y Amahuz en la línea;
  - Gohageneis en el pase;
  - Adaguchi y Panterilia en el obrador;
  - Irosene en la limpieza;
  - Nesvitor en el comedor.

## Reparto de papeles (2026-11-04, provisional: lo sustituyen las profesiones de arriba)

| Papel | Antes | Ahora | Por qué |
|---|---|---|---|
| **Maestro de yoga** (Hatha y Ashtanga; medita junto al linga en su tiempo libre) | Sadhguru | **Andrea** (`WorkdayDirector.YogaTeacher`) | Fue el único que hizo yoga de verdad con Leo, que practicaba unas 20 h por semana (`character-andrea.md`). Sale en la tarima de Hatha tradicional; su área es la Sala de Yoga. |
| Segunda maestra (para dar las dos clases a la vez) | — | **Irosene** (`WorkdayDirector.SecondYogaTeacher`) | Corre, escala y bucea a sus 70 y tantos: el Ashtanga, dinámico, le va. Sigue en el textil. |
| Mecánica: vehículos y maquinaria | Elon Musk | **Ducristin** | Área: Taller. |
| Artes marciales (horario libre: «Artes marciales con Gohageneis») | Goku | **Gohageneis** | Ágil y atlético, y ya es de la Guardia. |
| Hospital: registros y vacunación | Bill Gates | **Maite** | Área: Hospital. |
| Cuentas del santuario, desde el restaurante | Warren Buffett | **Nesvitor** | El papel de persona mayor que se pedía para él. Área: Cocina. |
| Huerto: alimentar a los visitantes | MacKenzie Scott | **Alicita** | Área: Huerto (con Panterilia). |
| Guardia | Ibrahim Traoré (coordinaba) | **Josedrilo**, con Gohageneis | Patrulla: como Gohageneis, sin área fija. |

Amahuz, Karen, Shalva y Adaguchi siguen de personal general.

**Las clases de yoga** funcionan igual que antes:
- la de Hatha la guía quien tenga la misión «yog_guia» en la rotación;
- la de Ashtanga (14 h) la da el primero que llega.

Lo que pasa a Andrea es lo que era fijo de Sadhguru:
- la salida en la tarima y el área de Yoga;
- la meditación del tiempo libre;
- ser el maestro que eligen las pruebas (`YogaClassTest`, `YogaFloorsTest`, con Irosene de segunda maestra);
- la escena de prueba `Prueba_Yoga` y la misión virtual de la Sala de Yoga.

## La jornada y el horario libre (regla W9)

- **Personal del santuario** (`WorkdayDirector.staff`, 18 con el jugador):
  - Kushal, Linduarda, Lizmibet, Goluis, Panterilia, Gohageneis, Irosene, Tote (`JefeConstruccion`) y Andrea;
  - los nueve de las fotos de Berón: Nesvitor, Amahuz, Karen, Alicita, Ducristin, Josedrilo, Maite, Shalva y Adaguchi.
- Antes eran 16 sin el jugador; ahora, 17. La rotación reparte las misiones de todas las áreas entre todos: cada misión le toca a
  cada uno algo menos a menudo.
- **Horario libre**: el catálogo tiene 54 plazas a la vez, más «Descansar en casa» sin límite. Con 17 personas sobra sitio. Solo
  cambió el maestro de artes marciales (de Goku a Gohageneis).
- **Casas del árbol**: las 18 están ocupadas (10 del personal que queda y 8 de la familia y los amigos de Berón; Amahuz y Karen
  comparten una). Ya no queda casa libre para recién llegados: si llega alguien, hay que añadir casas.

## Historia: los seis del mesocosmos (2026-11-03, ya no están)

Berón (2026-11-03) pidió añadir al santuario a Elon Musk, Goku, Bill Gates, Warren Buffett, MacKenzie Scott e Ibrahim Traoré, con
un papel por su trabajo público y conocido, con respeto y sin caricatura ni juicio político. Se añadieron como el resto del
personal, con dos piezas de aspecto que siguen en `CubeHair`: el pelo en puntas (`Style.Spiky`) y la boina (`Look.Beret`). El
2026-11-04 salieron a sus microcosmos.

| Id | Aspecto (cubos) | Stats (fuerza · agilidad · resistencia · masa) |
|---|---|---|
| `ElonMusk` | 1,88 m, pelo castaño oscuro corto, camiseta negra, vaqueros oscuros | 1,05 · 0,95 · 1,0 · 1,1 |
| `Goku` | Pelo negro en grandes puntas; gi naranja con camiseta, cinturón y botas azules | 1,6 · 1,5 · 1,6 · 1,1 |
| `BillGates` | Gafas finas, pelo claro con canas, jersey azul, pantalón caqui | 0,95 · 0,95 · 1,0 · 1,0 |
| `WarrenBuffett` | Canoso, gafas de montura gruesa, traje gris con camisa blanca | 0,85 · 0,8 · 0,9 · 1,05 |
| `MacKenzieScott` | Media melena castaña, blusa azul claro, pantalón oscuro | 0,95 · 1,05 · 1,05 · 0,95 |
| `IbrahimTraore` | Piel oscura, barba corta, boina roja, uniforme de campaña verde oliva | 1,25 · 1,2 · 1,3 · 1,05 |
| `Sadhguru` | Turbante crema con franja granate, barba larga blanca, camiseta sin mangas verde agua, dhoti y chal blancos, guirnalda | 1,0 · 1,2 · 1,4 · 1,0 |
