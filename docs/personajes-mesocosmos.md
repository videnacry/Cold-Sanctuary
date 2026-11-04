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

Lo que dijo Berón, en dos tandas:
- «Josedrilo y Maite son pareja, ¿Lizmibet y su esposo comparten residencia?; Ducristin tiene de pareja a Gallarle, el profesional de
  yoga es Josedrilo y también es profesional de bioconstrucción; Maite es profesional de cocina, Shalva es profesional de cultivo y
  bioconstrucción, Adaguchi es profesional de submarinismo y cocina, Amahuz es profesional de cocina y química; Karen es profesional de
  asuntos legales y lenguas; Alicita es profesional de contabilidad y project-manager».
- Después:
  - **Amahuz se llama Amykhuz**, y él y Karen son pareja (confirmado);
  - **Alicita y Julian** son pareja (Julian es búlgaro; se conocieron en un máster);
  - **Irosene y Fausto** son pareja (Fausto es mecánico);
  - Josedrilo es además **jefe de obra nueva**;
  - Tote es **jefe de reparaciones** y **guardia**;
  - Nesvitor es **guardia** y **agricultor**;
  - Ducristin es **gerente de banca** y **profesor**;
  - Andrea es profesional de **comunicación**;
  - Maite es **jefa de cocina** con Amykhuz y **curandera**;
  - Irosene es profesional de **letras** (profesora de poesía y narración);
  - los guardias son Gohageneis, Nesvitor y Tote.

**Esto sustituye al reparto de papeles de abajo**, que fue provisional.

**Cambio de nombre**: `Amahuz` → **`Amykhuz`** en todo: id, rótulo, personal, casas, pruebas y docs.
- Las escenas sin reconstruir guardan el GameObject «Amahuz».
- `WorkdayDirector.LegacyIds` y `CubeHair.For` aceptan el nombre viejo, y al reconstruir sale el nuevo.
- La foto sigue siendo `amahuz_karen.jpg`.

### Profesiones (`Professions`)

La profesión da a cada uno su papel sin quitar la rotación (regla J2):
- Cada día, antes de las vueltas por áreas, `WorkRoster` le da **una misión de cada área de su profesión**: la que menos ha hecho, y si
  la profesión nombra misiones con un prefijo (`rep_` las reparaciones, `obra_` la obra nueva…), una de esas.
- En una misión de **grupo** de su profesión (la clase de yoga, una obra), el profesional entra si está libre y **dirige** el papel que
  se dirige.
- Cada uno **empieza** en el área de su profesión principal (`SanctuaryDirector.HomeArea`, `PlaceStaff`), y su rótulo la dice.

| Persona | Profesión | Misiones de su profesión | Sale en |
|---|---|---|---|
| **Josedrilo** | Maestro de yoga (`YogaTeacher`), **jefe de obra nueva** y bioconstrucción | Sala de Yoga (guía la clase); Construcción: `obra_` (las obras) y `mat_` (el material) | la tarima de Hatha tradicional |
| **Tote** (`JefeConstruccion`) | **Jefe de reparaciones** y **guardia** | Construcción: `rep_`; Guardia | Construcción |
| **Gohageneis** | Guardia, artes marciales y **maestro de baile** | Guardia (y su patrulla, `CarnivoreKeeper`); guía el baile (un día de cada dos) | — |
| **Nesvitor** | **Guardia** y **agricultor** | Guardia; Huerto | el huerto |
| **Maite** | **Jefa de cocina** (con Amykhuz) y **curandera** | Cocina; Hospital: `hos_plantas` (misión nueva: remedios de plantas medicinales) y `enf_visita` (curas) | el restaurante |
| **Amykhuz** | **Jefe de cocina** (con Maite) y química | Cocina; Química | el restaurante |
| Shalva | Cultivo y bioconstrucción | Huerto; Construcción: `mat_` | el huerto |
| Adaguchi | Submarinismo (submarino de obra, ambulancia, tareas bajo el agua) y cocina | Construcción; Cocina | Construcción |
| Karen | Asuntos legales y lenguas | Aula: `aul_legal`, `aul_lenguas` | el Aula |
| Alicita | Contabilidad y dirección de proyectos | Cocina: `coc_cuentas`; Construcción: `con_proyectos` | Construcción |
| **Ducristin** | **Gerente de banca y profesor** | Cocina: `coc_cuentas` (las finanzas); Aula: `aul_clase` | el Aula |
| **Fausto** | **Mecánico** (la mecánica, antes de Ducristin) | Mecánica | el taller |
| **Andrea** | **Comunicación**: los avisos del santuario y la radio | Cocina: `coc_avisos` (misión nueva) | el comedor |
| **Irosene** | **Letras** (poesía y narración) y textil | Aula: `aul_letras` (misión nueva: clase y lecturas); Estudio Textil | el Aula |
| **Panterilia** | **Maestra de baile**, **finanzas** y segunda maestra de yoga (`SecondYogaTeacher`) | Cocina: `coc_cuentas`; guía el baile (un día de cada dos) | el huerto |
| **Julian** | **Dirección de proyectos**, con Alicita | Construcción: `con_proyectos` y `con_proyectos2` (misión nueva: una para cada uno) | Construcción |
| Gallarle | Pendiente de foto y de profesión | — | con su pareja |

**Por qué así**:
- **Ducristin sin banco**: no hay área de banca.
  - Las finanzas van en la mesa de cuentas del restaurante (Servicios), por donde entra el dinero del santuario, la misma que lleva
    Alicita.
  - Su faceta de profesor va en el Aula, con las clases.
- **Andrea en comunicación**: el comedor del restaurante (Servicios) es donde coinciden todos a la hora de comer, así que ahí se dan los
  avisos y se hace la radio. Su papel de **entrenador del gimnasio** lo pone la rama del entrenamiento: aquí no se toca.
- **Irosene al Aula**, como sugirió Berón: da clase de poesía y narración y lee en la sala de estudio. Deja de ser la segunda maestra de
  yoga.
- **Panterilia segunda maestra**: es «la de la percepción», la que observa, y corregir posturas es mirar.
- **Karen en el Aula**: no hay área de asuntos legales. La sala de estudio tiene mesas y biblioteca para el despacho, y la pizarra
  sirve para las lenguas.
- **Los registros y la vacunación** del hospital los llevan las médicas (Lizmibet y Linduarda). Maite, como curandera, hace las plantas
  medicinales y las curas.

### Parejas: una casa del árbol por pareja (`ResidentialBuilder.Partners`, `Bedroom.partner`)

| Casa | Viven |
|---|---|
| Lizmibet y Tote | Lizmibet, `JefeConstruccion` |
| Amykhuz y Karen | (confirmado) |
| Ducristin y Gallarle | Gallarle, pendiente de foto |
| Josedrilo y Maite | |
| Alicita y Julian | Julian, búlgaro, pendiente de foto |
| Irosene y Fausto | Fausto, mecánico, pendiente de foto |

- Hay **16 casas ocupadas de 18**: quedan 2 libres.
- Duermen allí **22 residentes**: 16 dueños y 6 parejas. El ring de almohadas sigue en 5 × 5, caben 25.
- **Miembros**: 21 con Kushal. Con la regla J16, el **ballet pasa a 2 pisos** (20 plazas por piso). El yoga sigue en 2 + 2 pisos.
  - La torre del Entrenamiento tiene ahora 10 pisos de entrenamiento más el Aula: 6 niveles.
  - **Simulador de holguras** (`mig/Clear.cs`) vuelto a pasar con 6 niveles: el techo, a ~197 m. Las holguras entre edificios no cambian
    (árbol–Entrenamiento 16,3 m, Servicios–Entrenamiento 16,5 m, guardián–Entrenamiento 7,0 m) y el tronco del guardián sube a 202 m.

### Maestros de baile (Panterilia y Gohageneis)

Berón: Panterilia es maestra de baile y de finanzas; Gohageneis, maestro de baile. Funciona como Josedrilo con el yoga.
- `Professions.ClassLeads` reparte el baile **un día cada uno** (`LeadOn`).
- **Horario libre**: en la franja de las 20 h (`LeisureRoster.LeadSlot`), el maestro del día guía «Bailar» si está libre.
  - Se pone delante del grupo; los demás se reparten como siempre.
  - El sitio es la Sala de Ballet cuando está terminada y, mientras está en obras, la calistenia o el gimnasio.
- **Jornada**: cuando la Sala de Ballet está terminada y abierta, el maestro del día da su entrenamiento (14–16 h).
  - Hasta entonces entrena como los demás.
- **Panterilia sigue de segunda maestra de yoga.** No choca:
  - el yoga del santuario lo guía Josedrilo;
  - la clase de Ashtanga la abre el primero que llega;
  - el baile es a las 20 h y el entrenamiento de ballet, solo los días que le toca.
- **Julian comparte la dirección de proyectos con Alicita**. Hay dos misiones, `con_proyectos` y la nueva `con_proyectos2`, y cada día
  cada uno coge la que menos ha hecho.

### Los seis nuevos (Berón, 2026-11-04)

| Persona | Profesión | Misiones de su profesión | Sale en |
|---|---|---|---|
| **David** | Informática | Informática: el clúster de IA (`ifo_sysadmin`, `ifo_refrigeracion`, `ifo_energia`) | Informática |
| **Adonis** | Informática | igual que David | Informática |
| **Julito** | Biotecnología | Carne Cultivada (el laboratorio) y Biología (con el cuidado de los fotobiorreactores del árbol) | Carne Cultivada |
| **Ale** | Biotecnología | igual que Julito | Carne Cultivada |
| **Ivelyna** | Pediatra | Hospital: `hos_pediatria` (misión nueva: consulta, vacunas y crecimiento de los niños) y las del hospital `enf_` | el hospital |
| **Luciana** | Piloto | Mecánica: `mec_alfombras` (misión nueva: la flota de alfombras voladoras y el transporte aéreo); Control de misión: `lan_control` (misión nueva: el seguimiento de la lanzadera y de los vuelos) | la Mecánica |

**Por qué así**:
- **Ivelyna en el Hospital, no en la Cría**: Berón preguntó si iba en la Cría, pero la Cría es de crías de animales. La pediatría es de
  los niños de las personas, en el hospital. La Cría le toca en la rotación como a cualquiera.
  - Aún no hay guardería del santuario. Cuando la haya, entra en su profesión.
- **Luciana, piloto**: lo que vuela en el santuario son las alfombras (su flota y el transporte aéreo, que se cuida en la Mecánica, junto
  a los vehículos) y la lanzadera (su control de misión). Los submarinos siguen con Adaguchi.

**Aspecto**: provisional y **pendiente de foto**, como Gallarle (`CubeHair.Placeholder`): pelo corto o media melena castaña, piel media
y ropa lisa de un color por persona.
- El rasgo femenino (`Fem`) solo va donde el nombre lo sugiere (Ivelyna, Luciana), y está **pendiente de que Berón lo confirme**.
- Ale es **hombre** (confirmado por Berón): va sin él.
- Se reconocen por el nombre **exacto**: David o Ale pueden salir en los microcosmos.

## Neo (2026-11-04)

Neo es un **niño, hijo de Ducristin y Gallarle** (pendiente de foto). Vive en la casa de sus padres (`Bedroom.kids`, `Kids` en `ResidentialBuilder`) y cuenta en el aforo del ring. Lleva el ciclo de vida de los humanos: `PersonLife` en etapa de niño, `BodyAging.growth` 0,35, con `mother` (Gallarle) y `father` (Ducristin) enlazados. **No está en la jornada** (`WorkdayDirector.Staff`): su tiempo es libre y va con su madre (`FollowMother`). Todavía no hay guardería ni juego propio de niños; cuando exista, con Ivelyna (pediatra), irá ahí.

**Por confirmar:** Berón escribió «Dukristin»; se conserva el nombre «Ducristin» hasta que lo confirme.

**Casas**: llegan seis y quedaban 2 casas libres.
- Los árboles de la arboleda crecen de 26 a **38 m** y cada uno lleva una **cuarta casa**, del otro lado y 21 m sobre la primera.
  - Hay **24 casas: 22 ocupadas y 2 libres**.
  - Duermen allí **28 residentes**: 22 dueños y 6 parejas.
- El **ring de almohadas** pasa a **6 × 6** celdas (caben 36).
  - Para que siga cabiendo dentro del anillo de casas (esquina ≤ 16,5 m del centro), la franja del cabecero baja de 1,6 a 1,2 m y el
    margen del borde de 0,8 a 0,3. La esquina queda a 16,3 m.
- `AreaCapacity` sale de la lista del personal: **27 miembros**.

**Regla J16 con 27 miembros**:
- El **dojo** (25 por piso) pasa a **2 pisos**, el ballet a 2 y el yoga sigue en 2 + 2.
- El Aula pide 2 pisos (24 por piso).
- La torre del Entrenamiento lleva 11 pisos de entrenamiento más 2 del Aula: **7 niveles**, con el techo a unos 219 m.
- **Simulador de holguras** (`mig/Clear.cs`) con 7 niveles: las holguras entre edificios no cambian (árbol–Entrenamiento 16,3 m,
  Servicios–Entrenamiento 16,5 m, guardián–Entrenamiento 7,0 m) y el tronco del guardián sube a 224 m.
- **Lanzadera** (regla T13): su plataforma debe quedar ≥ 30 m por encima de lo más alto, así que ≥ ~250 m con el nuevo techo del
  Entrenamiento. La regla la valida al final del Blockout y, si hace falta, la sube sola.
  - **Hay que mirarlo al reconstruir**: el registro `[Lanzadera]` y `LaunchSiteTest` (grupo 90).
### Pisos en obras

Berón vio bailar en el piso de ballet sin terminar.
- El ocio y el entrenamiento **no van a un piso en obras**: `LeisureDirector.OpenArea` y `WorkdayDirector.OpenTraining` ya lo
  filtraban por su `ConstructionProject`. Hasta que el ballet se termine, el baile va a la calistenia o al gimnasio.
- `HorarioLibreTest` (65) ahora lo **comprueba**:
  - ninguna actividad libre ni ningún entrenamiento tiene su sitio en un piso en obras;
  - `AreaCapacity` no cuenta los puestos de un piso en obras, que quedan como pendientes.

### La jornada (regla W9)

- Ronda de profesiones antes de las vueltas. Las misiones de grupo de su profesión las dirige el profesional; las obras las dirige
  Josedrilo.
- Hay 7 misiones nuevas de profesión: `aul_legal`, `aul_lenguas`, `aul_letras`, `coc_cuentas`, `coc_avisos`, `con_proyectos` y
  `hos_plantas`. Son unas 6 h al día frente a 21 × 5 h: sobra sitio.
- La brigada de `RestaurantServiceTest`:
  - Goluis, Maite y Amykhuz (los jefes de cocina) en la línea;
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
| **Entrenador del gimnasio** (misión «gym_planes», clase de calentamiento, correcciones y spotter) | — | **Andrea** (`GymProgramming.Trainer`) | Berón (2026-11-04): «el personaje con más fuerza es Andrea» (`Anima.strength` 1,6, el más alto del personal). Sale en el gimnasio; su área es el Gimnasio. Deja el yoga (las dos clases son a las 14 h): el yoga pasa a Josedrilo (rama de personajes). [gimnasio-maquinas.md](gimnasio-maquinas.md) §15.5 |

Amykhuz, Karen, Shalva y Adaguchi siguen de personal general.

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
  - los nueve de las fotos de Berón: Nesvitor, Amykhuz, Karen, Alicita, Ducristin, Josedrilo, Maite, Shalva y Adaguchi.
- Antes eran 16 sin el jugador; ahora, 17. La rotación reparte las misiones de todas las áreas entre todos: cada misión le toca a
  cada uno algo menos a menudo.
- **Horario libre**: el catálogo tiene 54 plazas a la vez, más «Descansar en casa» sin límite. Con 17 personas sobra sitio. Solo
  cambió el maestro de artes marciales (de Goku a Gohageneis).
- **Casas del árbol**: las 18 están ocupadas (10 del personal que queda y 8 de la familia y los amigos de Berón; Amykhuz y Karen
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
