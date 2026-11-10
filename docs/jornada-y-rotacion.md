# Jornada laboral, rotación de misiones e iniciación

Diseño de Berón (2026-10-18). Código:
- `Assets/Scripts/Progression/WorkRoster.cs`: reparto, lógica pura.
- `WorkdayDirector.cs`: la jornada en el juego.
- `AreaWork.cs`: catálogo de misiones.
- `World/ConstructionProject.cs`: obras.
- `World/InfirmaryGate.cs`: salida de la enfermería; `World/MagnateWarden.cs`: la Magnate devuelve a quien abandona su puesto.

## La jornada (8 h, todos los días)

| Hora | Qué |
|---|---|
| 8:00–9:00 | **Entrenamiento** (1 h), para todos (Berón 2026-11-05) |
| 9:00–11:00 | Tareas (2 h): iniciaciones y santuario |
| 11:00–12:00 | Comida, en el restaurante |
| 12:00–14:00 | Tareas (2 h) |
| 14:00–15:00 | Entrenamiento (1 h): virtualizaciones de yoga, gimnasio… |
| 15:00–16:00 | Tareas (1 h) |
| 16:00 → | **Libre**: cada uno va donde quiere y hace lo que quiere (el jugador también) |

Durante las 8 h los personajes van al área de su misión y se quedan allí. **Nadie queda encerrado** (2026-10-22): si alguien abandona su puesto, la **Magnate** (que conoce todos los hechizos, también teletransportarse) aparece de pronto delante de él, le dice que vuelva y lo teletransporta de vuelta dentro del área (`World/MagnateWarden.cs`). El jugador ve su itinerario en pantalla. Consola: `jornada` (plan del día y equilibrio).

**Qué cuenta como «su puesto»** (2026-10-23). La Magnate mide el **espacio que necesita la tarea**, no solo un punto (`WorkdayDirector.InWorkspace`):
- **El edificio entero de su área** (`SanctuaryArea.InBuilding`, Berón 2026-10-23): entreplantas, el piso de arriba aunque sea otra área (gimnasio ↔ dojo, yoga ↔ ballet) y su pista de alfombras de arriba. El suelo de debajo no cuenta. También cuentan todas las áreas que se llaman igual. El Huerto incluye además sus parcelas y el bambusal. La salida del hospital antes de la iniciación (`InfirmaryGate`) usa la misma medida.
- **Iniciación:** quien empieza la iniciación de un recién llegado la termina, aunque cambie el turno (`NewcomerInitiator.InProgress`). Mientras el jugador no esté iniciado, la jornada no lo mueve.
- **Las áreas por las que la tarea hace ir** (`Mission.also`):
  - llevar la cosecha a la Cocina;
  - la carne cultivada a la Cocina y la Cría;
  - las reparaciones, del almacén de Construcción al área averiada;
  - el material (guadua, micelio, cuerda) al almacén;
  - el Tc-99m al Hospital;
  - los análisis de animales en la Cría.
- **La obra** en la que se trabaja (`Mission.site`).
- **Cualquier área** si la tarea las recorre todas (`Mission.roams`: reparar maquinaria).
- **De camino**: en un puente, una pista, una alfombra o una escalera se le deja pasar hasta 90 s. Quedarse ahí no es trabajar.
- **Fuera de su alcance** (`MagnateWarden.Blocked`, Berón 2026-10-23): mientras vuela en alfombra o está en el microcosmos (o entrando/saliendo), la Magnate no trae a nadie. Cualquier sistema puede bloquear su alcance para todos con `MagnateWarden.Lock(clave, true)` y quitar su bloqueo con `Lock(clave, false)`.

## Iniciaciones (Berón 2026-11-05)

«son 2 horas de entrenamiento de modo que una de las 2 de final de jornada sería movida; … se crea una lista diaria y se distribuye como tareas a los personajes que sepan iniciar, aqui también se aplica la regla de oro de que todos los personajes deben saber/aprender a iniciar, … con lo que un personaje nuevo podría tener toda su jornada con horas de entrenamiento el primer día … luego estará restringido a solo la mitad de horas disponibles … primero completar las jornadas de los personajes que menos habilidades tienen».

- **Hora movida:** de las dos de la tarde, 15–16 vuelve a ser trabajo, y la que pasa a la mañana es la de las 8. Quedan 5 h de tareas y 2 h de entrenamiento. Elegí 15–16 porque el gimnasio, el yoga y sus clases empiezan a las 14.
- **«Horas de entrenamiento» en la frase del recién llegado** significa horas de iniciación: son tareas, no el entrenamiento físico, que sigue a las 8 y a las 14.
- **Ya no hay hora fija de lección.** La iniciación es una tarea (`aprender_<id>`), en el área de la tarea, con su actuación y su uniforme, sin efecto en el santuario.
- **La lista** (`WorkRoster.PlanInitiations`): entre lo que hay hoy, lo que cada uno no sabe. Se reparte antes que nada, hora a hora desde las 9, de quien menos sabe a quien más.
- **Cuánto:** su primer día (entró después del primer reparto), las 5 h; luego, 2 h; luego, lo que falte. El personal de siempre no cuenta como recién llegado el primer día del juego: si no, nadie haría el santuario ese día.
- **Clases:** ≤ 6 personas y ≤ 6 por hora. Quien pueda entra en una clase que ya exista. Prioridad: lo que nadie sabe, lo que tiene riesgo, lo obligatorio, lo que más gente necesita.
- **Después, las del santuario,** también de quien menos sabe a quien más (`order`). Con 5 h de iniciaciones no le cabe nada más; con 2, sigue con lo que sabe.
- **Quién inicia (diseño final, Berón 2026-11-05):** los trabajadores; la Magnate es el último recurso. `WorkRoster.instructorsTeach` está encendido por defecto: cada clase busca a alguien libre a esa hora que sepa la tarea y sepa iniciar (`instruir_<id>`, una tarea para él), y si no hay nadie sale un cuerpo de la Magnate. Kushal no da clases: el jugador no tiene cómo darla. El primer día casi todas serán de ella, porque nadie sabe iniciar aún.
- **Saber iniciar** (`instruir`) es una tarea más de la lista, para quien ya sabe ≥ 3 tareas. Dar clase no cuenta para el tope de iniciaciones de quien la da.
- **Lo que sabe cada uno** (`Worker.taught`, `WorkRoster.Learned`): lo hecho, lo enseñado y lo de su profesión. **No se guarda en la partida**, igual que las cuentas de la rotación.
- **La iniciación del primer turno** (el clon que aparece a su lado) sigue para lo que nadie le enseñó y no pide lección.

- **Horario del que inicia:** puede dar una clase a cualquier hora que tenga libre, no solo la siguiente de su jornada: esa hora queda reservada y sus tareas se colocan alrededor (`WorkRoster.PlaceFor`). Lo reparte el que menos clases lleve hoy.

### La regla de oro y quién enseña
- **J30: todos saben hacer de todo.** La leo como **el destino y una prohibición de exclusivas**: todos acaban sabiéndolo todo, también iniciar, y ninguna tarea es de nadie. Hasta entonces decide el requisito. Si se leyera literal («ya lo saben hoy»), no habría lista de iniciaciones.
- Solo piden iniciación las tareas con riesgo. Las demás se siguen aprendiendo haciéndolas, y una obligatoria sin nadie que sepa la hace un clon de la Magnate.
- **Iniciaciones de llegada** (cocina, huerto y yoga, `AreaInitiator`): **las dejo con el trabajador de turno (J5)**. Moverlas a la Magnate ahora sería ir y volver: inicia quien pueda, y ellas ya funcionan así. Además son una orquesta con pasos del recién llegado y `SpawnMagnateBody` da un cuerpo sin conducta. Si Berón las quiere con ella hoy, es un cambio aparte.

## Requisitos (Berón 2026-11-05)

`MissionRequirements`, consultado en `WorkRoster.Plan` antes de repartir. Lo que falla queda en `plan.blocked` con su motivo y en el balance.

| Clase | Qué | Hoy |
|---|---|---|
| Conocimiento | Las de riesgo piden saberlas | `hos_` de quirófano, UCI, urgencias, veterinaria y aves; `enf_visita`, `enf_iniciacion`, `enf_animales`; `pla_`, `nuc_`, `fis_`, `qui_`, `bio_`, `lab_`, `ifo_`, `lanz_`, `lan_`, `sol_`, `ene_`, `mec_`, `gua_` |
| Estado del área | Nada en un piso en obras (J29) | Todas, salvo las obras |
| Seguridad | Sin emergencia en el sitio nuclear | `pla_operar`, `pla_isotopos`, `pla_ciclotron`, `nuc_turno` |
| Herramientas | Su equipo existe | `ifo_` (el clúster), `lab_` (un biorreactor) |
| Materiales | Hay qué usar | `sol_instalacion` (mesas por montar) |

Es un **primer juego de requisitos**: la lista de tareas con riesgo es decisión mía, y herramientas y materiales son pocos. Añadir uno es una línea en `MissionRequirements.Of`. No se usa «el mueble existe», porque hoy una tarea sin mueble cae en un punto del área y bloquearla la dejaría sin hacer.

## Auditoría (W11, regla de auditar lo que ya existe al cambiar una regla)

| Qué | Impacto | Hecho |
|---|---|---|
| Horas de trabajo | 5 h en 9–11, 12–14 y 15–16 (`WorkRoster.Place`) | Las iniciaciones van dentro: unas 31 personas dan 155 h, y al principio 62 h son iniciaciones. **Habrá clones los primeros días** |
| Tope de 5 h (J2) | Se mantiene | Prueba 104 |
| Comida y libre | No se mueven | `LeisureRoster` sigue desde las 16 h |
| Entrenamiento de tarde | 14–15 | `GymEndMin` 55, `GymCoachRoundsEnd` 35, `DeepCloseHour` 14,75. **La rutina del gimnasio tiene la mitad de tiempo** |
| Misiones de 3 h | No caben | Rondas de frutales: `MaxRoundHours` 2 y `RoundTarget` 1,9 (era 2,9: salían rondas más largas que su misión). **Más rondas, y un edificio grande ya se parte** |
| Misiones de 2 h | Solo 9–11 o 12–14 | Por `Place` |
| Grupos (yoga, obras) | A las 9 si hay gente libre; si no, a las 12 o a las 15, todos a la vez | `AssignGroups`. Una clase de yoga puede cambiar de hora de un día a otro |
| Reparto del santuario | De quien menos sabe a quien más | Corrige que **el último de la lista se quedara sin nada** (el caso de Kushal) |
| Papel fijo (`gym_planes`) | Igual | — |
| Gimnasio y yoga | Sus clases siguen a las 14 h | `Drive` solo las lanza de tarde |
| Uniformes, actuaciones y sitios | Las lecciones usan los de la tarea | `LessonBase` |
| Kushal | Entrena y aprende como todos; su primer día completo son 5 h de iniciaciones con E; la incorporación tardía usa las franjas nuevas | `PlayerWorkday`, `PlayerTasks` |
| Horario libre | El maestro del baile ya no suma esa clase como ocio suyo | `LeisureRoster` |
| Microcosmos (`MicroJornada`) | **No cambia**: su horario sigue 8–11. Berón habló del santuario | — |
| Iniciaciones de llegada (J5) | No cambian (ver arriba) | Manual |
| Pruebas | `JornadaTest` y otras leen del plan real; solo las de la rotación pura van sin el modelo nuevo | Grupo 104 |

**Auditoría del reparto por santuarios (Berón 2026-11-08).** Lo que cambia la regla nueva en lo que ya existía:

| Qué | Impacto | Hecho |
|---|---|---|
| Tope de 5 h (J2) | **No cambia**: 5 h, y libre a las 16:00. Berón (corrección del 2026-11-08) quitó la jornada estirada | 28 y 104 lo miden; 113 comprueba que nadie trabaja después de las 16:00 |
| Orden fijo del personal | **Cambia**: permutación del día (Kushal, el primero) | `DayOrder`; regla 5 del reparto reescrita |
| Clones por obligatorias | **Cambia**: un clon por área sin nadie, y clones para todo lo que no cabe en los humanos, obligatorio o no | `CloneForRest`. Nada pasa a mañana |
| Kushal y los nueve de siempre | Solo la tierra (s1n1): **ya no reciben misiones del Aéreo ni de la Línea** | J31 sigue, pero casi nunca aplica |
| Fernando y Luciana | s3n1 (inferido): pasan el día en el Aéreo; sus puestos de la tierra (Luciana en Mecánica) no los cogen ese día | `StartingScore` |
| Línea de las cavernas | La mayoría de días no tiene tareas: nadie va al Marino | — |
| Horario libre | No cambia: empieza a las 16:00; quien está fuera no se conduce y vuelve para la fiesta | `LeisureDirector` (salta a `RemoteStaff.Hidden`) |
| HUD | No cambia: «16:00 Libre» | `Itinerary` |
| Capacidad (S13 = J16) | **No cambia**: todo sigue para todos los miembros | `AreaCapacity`; el Marino y el Aéreo no miran el número de miembros |
| Microcosmos (`MicroJornada`) | No cambia | — |
| Pruebas | 113 nueva; en 28, las de la rotación pura dicen que lo son y la del juego pide que nada pase a mañana; 104 y 50 vuelven a las de antes | Ver el informe de la rama |

## Reparto (rotación)

1. Cada día hay una **lista de misiones por área**. Primero van las que quedaron **pendientes** ayer, luego las **obligatorias** del día y después el resto.
2. Se recorre el **orden de áreas**. De cada área se toma una misión y se le da a la **primera persona** (en el **orden del personal del día**, ver 5) a la que aún le quepa en sus 5 h. Con el reparto por santuarios, a la que **menos horas lleva** de su sitio (partes iguales). Después se pasa a la siguiente área, y así en vueltas.
3. A esa persona se le da primero una misión del área que **nunca haya hecho**. Si no hay, la que **menos veces** haya hecho; si empatan, la que **hace más días** que no hace.
4. Las misiones se reparten por **tiempo**: cada una tiene su duración y no se parte en la comida.
5. El **orden de áreas rota** una posición cada día: la última pasa a ser la primera. **El orden del personal también cambia cada día** (Berón 2026-11-08; antes no cambiaba): es una permutación sembrada con el día (`WorkRoster.DayOrder`), y dos días seguidos nunca dan el mismo orden. Kushal va siempre el primero (el jugador tiene su jornada entera). Con el reparto puro (`sanctuaryModel` apagado, solo en pruebas de la rotación) sigue el orden fijo.
6. **Clones de la Magnate** (Berón 2026-11-08): con el reparto por santuarios, un clon por **área que se queda sin nadie** y clones de 5 h para **todo lo que no cabe en los humanos**, obligatorio o no. Las clases de iniciación sin maestro siguen siendo suyas (J32). En el reparto puro, solo para las obligatorias sin cubrir.
7. **Pendientes:** en el reparto puro, el resto pasa a mañana y es lo primero que se asigna. Con el reparto por santuarios **nada pasa a mañana**. Solo queda en el registro (`DayPlan.impossible`, debe estar vacío) lo que ni un clon puede hacer.
8. **Entrenamiento**: rota entre los entrenamientos abiertos (persona *i*, día *d* → *(i + d) mod n*).
   Desde el 2026-11-04, **el gimnasio va por los días del plan de cada uno** (`GymProgramming.ApplyGymDays`; el entrenador, Andrea,
   siempre): quien tiene gimnasio ese día en su plan va; a quien la rotación mandaba un día sin gimnasio en su plan, se le da el
   siguiente entrenamiento abierto ([gimnasio-maquinas.md](gimnasio-maquinas.md) §15).
9. **Misiones de puesto** (`Mission.owner`; regla de oro J30, 2026-11-05: antes solo las hacía su dueño y sin él no se hacían): se le ofrecen primero a su dueño; sin él, siguen por rotación, y la Magnate también las enseña (J32). Texto antiguo: sin él, ese
   día no se hacen ni quedan pendientes. Hoy: «gym_planes», del entrenador del gimnasio.

**Equilibrio** (`WorkRoster.Balance`, en el log y en `jornada`) avisa de tres cosas:
- faltan personas: hubo clones o quedaron pendientes;
- faltan misiones: horas de personal sin asignar;
- un área se queda hoy sin misiones.
Con el reparto por santuarios dice además las horas de cada sitio, para cuántas personas y la parte de cada una, quién va al segundo reparto y qué es imposible.

## Reparto por santuarios (Berón 2026-11-08)

Por rendimiento y por diseño. Lo hace `WorkRoster` con `sanctuaryModel` (lo enciende `WorkdayDirector`); la casa de cada uno, `SanctuaryPlacement`; el apagado de quien está fuera, `RemoteStaff`. Prueba: grupo 113 (`RepartoSantuariosTest`, casi toda sin escena).

**Casa.** Cada humano vive en un santuario y un nivel. Santuario 1 = la tierra (`SampleScene`), 2 = el Marino ([sitio-nuclear.md](sitio-nuclear.md) §20), 3 = el Aéreo (§23).
- **Se sube por poder** (Berón 2026-11-08; [progresion-santuarios.md](progresion-santuarios.md)): un humano sale de su santuario solo cuando su poder supera al **jefe** de ese santuario, el animal más poderoso de su fauna (en s1n1, la ballena, la orca o el oso polar, el más fuerte que haya). Pasa al siguiente santuario del nivel, o al nivel siguiente tras el último. Poder = cuerpo (`Predation.Power`) + magia de las margas + bonusPacks (`SanctuaryPower`).
- **Casa de inicio:** Irosene y Lizmibet en s2n1, Linduarda en s3n1 (`SanctuaryPlacement.StartingHome`). Los demás, por su poder: un humano corriente, s1n1.
- **Acceso:** a sitios anteriores sí, a los siguientes nunca. Va si el nivel del sitio es menor, o si es el mismo y el santuario es menor o igual. s1n1 no va a s1n2 ni a s2n1; s1n2 va a s1n1, s2n1 y s3n1.
- **Guardado:** la casa nunca baja y se guarda entre partidas en PlayerPrefs (`cs_santuarios_poder`), como `Progress`; no hay otro sistema de guardado. Sube como mucho un santuario por día. En las pruebas no se guarda. Ya no hay tabla de puntuación ni regla de 30 días.
- **El santuario de cada área** (`SanctuaryPlacement.SanctuaryOfArea`): Aéreo = Energía, Control de misión y Fábrica de cohetes. Marino = la Línea de las cavernas y **la planta nuclear** (Planta Atómica y Central Nuclear; Berón 2026-11-08: «lejos del santuario y de la vida»; dónde está y su estación de alfombras lo mira otra tarea). El resto, la tierra. Hoy todo es nivel 1.

**El área entera.** Un área de varios pisos o salas es una sola: el hospital con todos sus pisos, el yoga con sus salas (`WorkRoster.AreaGroup`). Las misiones ya usan un nombre por área. En la escena tampoco hay pisos que sean áreas aparte: el hospital y el yoga son un `SanctuaryArea` cada uno, con sus pisos dentro. Las horas de cada área salen de **su propia lista de hoy**: el catálogo más lo del día (obras, cocina, material, sitio, energía, frutales). Cada área pide ⌈horas / 5⌉ personas, y al menos 1. Todo se recalcula en cada reparto con el personal y las tareas de ese día. Una persona nueva, un piso nuevo o una misión nueva cambian las horas y los clones. Nada está fijado a mano.

Horas de hoy, del catálogo fijo (`AreaFunctions`), más lo que se añade según el día:

| Área (sitio) | Catálogo fijo | Lo que se añade según el día |
|---|---|---|
| Hospital (s1) | 15 h | refugio de aves 0,5 |
| Cocina (s1) | 10 h | elaboraciones 0,5–2 h cada una (hasta 10) |
| Huerto (s1) | 5 h | rondas de frutales 2 h cada una, guadua 2 |
| Sala de Yoga (s1) | 6 h (4 de la clase en grupo) | — |
| Guardia (s1) | 6 h | — |
| Planta Atómica (s1) | 5 h | — |
| Aula (s1) | 5 h | — |
| Mecánica (s1) | 4,5 h | — |
| Carne Cultivada (s1) | 4 h | micelio 1 |
| Cría (s1) | 4 h | — |
| Central Nuclear (s1) | 4 h | simulacro 1 (semanal), iceberg y alarma de la roca 2 |
| Construcción (s1) | 3,5 h (con la reparación del día) | por obra abierta 1 + 2 por persona del equipo; bahareque 1 |
| Estudio Textil (s1) | 3,5 h | cuerda 1 |
| Biología (s1) | 2 h | fotobiorreactor 1 |
| Sala de guardia (s1) | 2 h | lectura del árbol cazador 1 |
| Informática, Sala de asambleas, Música (s1) | 2,5 h cada una | — |
| Gimnasio (s1) | 1,5 h | planes del entrenador 1 |
| Química (s1) | 1,5 h | — |
| Control de misión (s3) | 5 h | iceberg 2, vaciado 1 |
| Energía (s3) | 2 h | eólica o mareal 1, almacén 0,5, instalar 2, reparar 1, disuasión 1, iceberg 2, parada 1 |
| Línea de las cavernas (s2) | 0 h | bajada o refugio 2 h, muestras, raciones y simulacros del árbol cazador 1 h |

**El reparto del día** (`WorkRoster.SplitDay`, en cada `Plan`):
1. Cada uno pasa el día en **un** sitio al que llega. Kushal, siempre en la tierra.
2. **Primer reparto:** cada sitio con tareas pide la suma de lo que piden sus áreas. Se cubren empezando por el sitio al que llega menos gente. Para cada sitio se coge primero a quien menos sitios alcanza, en el orden del día.
3. **Segundo reparto:** los que sobran van, uno a uno, al sitio con más horas por persona de los que alcanzan.
4. **Partes iguales** (Berón 2026-11-08, corregido): las horas de tareas del sitio se reparten por igual entre sus humanos. Cada tarea va a quien menos horas lleva hoy de los que pueden hacerla (`RunRounds`). Si la parte pasa de 5 h, cada humano hace 5 h y lo demás, clones. Si no, todos la misma parte, menor. Las tareas no se parten. **Tolerancia:** entre quien más y quien menos lleva del sitio, como mucho la tarea más larga del sitio (113 lo prueba con tareas de 1 h y de 2 h). Iniciaciones, puestos, profesiones y grupos se reparten antes y también cuentan.
   **Sin huecos** (2026-11-09, ronda 31 en vivo: 21 clones al día con 14 h de humanos ociosos):
   - Cada vez, la persona que menos lleva coge de todas las áreas de su sitio la tarea que **empieza justo donde acaba lo suyo**. A igualdad, la más larga (`SharedRounds`).
   - Antes se iba área por área. Las tareas de 1 h ocupaban las franjas de 2 h (9–11 y 12–14), y las de 2 h (rondas de frutales, servicio, guardias, obras) ya no cabían en nadie.
   - Cada clon dice su motivo en el registro: `[Reparto] Día N · clon para «área»: motivo …`. Los motivos son: nadie del sitio, nadie sabe hacerla, todos al tope de 5 h, o sin hueco de su duración en las franjas de quien tiene horas (con quiénes y cuánto les queda).
   - **Las iniciaciones cuentan como horas:** un día con muchas clases (2 h por persona) deja menos horas para las tareas, y esos clones son de verdad.
5. **Sin jornada estirada:** todos acaban a las 16:00 (Berón quitó las horas de más).
6. **Clones** (`CloneForRest`): primero un clon por cada área entera con tareas y sin ningún humano (nadie de su sitio, o nadie pudo iniciarse en ella). Después, clones de 5 h para lo que quede, sitio a sitio. Un área **sin tareas no pide a nadie**. **Nada pasa a mañana**: varios trabajadores o clones en un área se reparten sus tareas. **Varias tareas a la vez en un área** (Berón 2026-11-08; [tareas-por-area.md](tareas-por-area.md)): con varias personas se reparten; con una, ella las encadena (los papeles de un grupo, también), sin un clon por puesto. La obra cuenta turnos, no personas. A igualdad, la tarea más larga primero, para que encajen en las franjas. **Las opcionales** ([tareas-por-area.md](tareas-por-area.md)): primero se reparten las obligadas; después, las opcionales a quien aún tenga tiempo. Si nadie lo tiene, ese día no se hacen: nunca un clon, nunca a mañana.
7. **La regla de oro sigue** (J30): dentro de su sitio todos rotan por todo. Y como cada día cambia el orden y el reparto, todos pasan por todos los sitios que alcanzan.

**Un área que nadie sabe hacer** (2026-11-08, ronda en vivo; regla de oro J30): si todas las tareas de un área piden saber y nadie de su sitio sabe ninguna (la Planta Atómica: `pla_`, `fis_`), sus iniciaciones van **primero** (`PlanInitiations`: +800 de prioridad, y cada área vacía abre su propia clase en la hora). Quien se inicia, la hace después. La Magnate la cubre solo si nadie pudo iniciarse ese día.

**Lo pendiente se cuenta una vez** (2026-11-08): una tarea de cada día que quedó pendiente ayer y hoy vuelve en la lista es la misma tarea. Va la de ayer, primero, y no se suma otra vez. Antes, cada día sin hacer hinchaba las horas del área.

**Huerto y Construcción en vivo** (el registro del 2026-11-08: Huerto 61 h y 13 personas, Construcción 32,5 h y 7):
- **Huerto:** 5 h del catálogo, 2 h de guadua si el almacén la pide, y **2 h por ronda de frutales**. Cada ronda (`FruitGardener.Rounds`) junta tramos de macetas de un techo y tramos de máscaras y fitoplancton de cada árbol (`TreeStretches`, ≤ 1 h cada uno) hasta 1,9 h. Su misión dura 2 h, y lo que sobra de la revisión se usa en polinizar y podar (`StartExtras`). No se cuenta un tramo como una ronda: el tramo va dentro de la ronda. 61 h ≈ 5 + 2 + **27 rondas × 2 h** (inferido de la cuenta: el número de rondas sale de la escena). El peso real son las máscaras de los árboles, unas 200 puertas por árbol. Pendiente con Berón: si una ronda corta debe contar sus horas de verdad y no 2 h.
- **Construcción:** 3,5 h del catálogo (almacén, dos direcciones de proyectos y la reparación del día), más, **por obra abierta**, 1 h de material y 2 h por cada persona del equipo. Las obras del constructor son las salas de Ballet y los Dojos de cada piso, con un equipo de 3: 7 h por obra. 32,5 h ≈ 3,5 + **4 obras × 7 h** + 1 h de bahareque (inferido: cuáles están abiertas sale de la partida guardada). Es demanda real.

**Quien pasa el día fuera** (`RemoteStaff`): no se simula en `SampleScene`. Su cuerpo se apaga (`SetActive(false)`), como el vuelo de `AereoLink` y la bajada de `SiteLine`, y sus misiones corren por el reloj. Vuelve al andén de su santuario (el muelle aéreo o la estación) el día que le toca la tierra, o en una **fiesta**: la franja de una actividad de todos a la vez del horario libre (`LeisureDirector` pone `RemoteStaff.Feast`). El horario libre lo sigue contando y no lo conduce mientras está fuera. En las pruebas no se apaga a nadie (salvo `RemoteStaff.InTests`). Su comida y sus entrenamientos son de allí, sin simular.

**Capacidad.** Las salas y las máquinas **no se encogen**: siguen hechas para todos los miembros de todos los santuarios (`AreaCapacity.Members` = toda la lista del personal), porque en las fiestas todos se juntan en uno. **Los constructores del Marino (`SampleSceneBuilder.Marine`) y del Aéreo (`SampleSceneBuilder.Aereo`) no miran el número de miembros**: sus salas tienen el tamaño que tienen. No se han reconstruido.

**El fin de la jornada.** Las 16:00 para todos: `Where`, `OnShift`, `HeadingEarly`, el itinerario del HUD y el horario libre no cambian.

## Misiones por área (catálogo actual)

Las marcadas con * son obligatorias cada día.

- **Enfermería**:
  - Iniciación de recién llegados y turistas* (1 h)
  - Pasar visita y curas* (2 h)
  - Análisis y vacunas de animales (1 h)
  - Limpieza e inventario (1 h): incluye la **ronda del banco de sangre** (retirar lo caducado, reponer de O− el submarino-ambulancia, llamar a un donante O− si faltan; docs/hospital.md §4)
- **Cocina**:
  - Servicio del restaurante* (2 h)
  - Limpieza e inventario* (1 h)
  - Obrador (2 h)
  - Atender en sala (1 h)
- **Huerto**:
  - Riego y deshierbe* (2 h)
  - Cosecha (1 h)
  - Alimentar a los visitantes (1 h)
  - **Ronda de frutales de los techos*** (`jar_frutales_N`, ≤ 2 h cada una desde el 2026-11-05; recorre varias torres): con riego por goteo, unas 3 al día para revisar y abonar las ~204 macetas y, con lo que sobra, polinizar y podar (docs/jardines-frutales.md). Desde el 2026-11-03 también los **bancales de tierra de la cima** de cada edificio-árbol: van primero en la ronda de su edificio (se llega al techo en alfombra). Desde el 2026-11-04 cada ronda reserva el tiempo de polinizar y podar, y las tareas de un árbol (máscaras, fitoplancton) van en tramos de ≤ 2,9 h: hay tantos jardineros como pida el trabajo del día (docs/jardines-frutales.md §Rondas que caben en 3 h)
- **Sala de Yoga**:
  - Clase para turistas* (1 h)
  - Preparar la sala (1 h)
- **Mecánica**:
  - Vehículos (2 h)
  - Maquinaria (1 h)
- **Estudio Textil**:
  - Tejer y teñir (2 h)
  - Coser y reparar (1 h)
- **Carne Cultivada**: Biorreactores* (1 h)
- **Gimnasio**: Mantenimiento (1 h)
- **Guardia**:
  - Patrulla y raciones a los carnívoros* (2 h)
- **Construcción**, por cada obra abierta:
  - Llevar el material del día* (1 h)
  - Trabajar la obra* (2 h)
  - Además, una **reparación** diaria en alguna área (1 h)

## Áreas de entrenamiento

- **Abiertas**: Sala de Yoga y **Gimnasio**: 30 máquinas (10 tipos × 3, docs/gimnasio-maquinas.md), mancuernas, kettlebells, barra de dominadas y sacos.
- **En obras**:
  - Sala de **Ballet**: 5 días.
  - **Dojo** de artes marciales: 6 días.
  - Parque de **calistenia**: 4 días.
- Cada obra tiene a pie de obra el **material de un día** (ladrillo, cemento, tablones), andamio y paredes a medio levantar.
- Con «llevar material» y «trabajar la obra» las paredes suben y la obra se levanta **a la vista**: cada turno coloca las piezas del día por etapas (cimientos y suelo → estructura → instalaciones → muebles y máquinas → acabados y decoración) mientras el equipo martillea, los palés menguan, el andamio va a lo que se construye y el cartel dice etapa y %. Lo que se ve antes de acabar es una maqueta que no se puede usar (docs/construction-simulation.md §7, prueba 98).
- Terminada, se abre su entrenamiento y entra en la rotación.

## Iniciación (bloqueante)

- **Recién llegados** (Kushal, turistas) no están registrados en el santuario:
  - No pueden **salir de la enfermería** hasta hacerse análisis y vacuna y completar la iniciación con Linduarda.
  - Pueden intentar salir (no hay bloqueo en la puerta), pero la **Magnate** se teletransporta delante del que sale,
    le explica que aún no es del santuario y lo teletransporta de vuelta dentro. Sustituye a los guardias que lo
    perseguían y lo arrastraban («jalar»), que se movían de forma rara.
  - Dentro de la enfermería se mueven libres; Linduarda los busca para seguir.
- **Misiones de iniciación**:
  - La de recién llegados y turistas es una **misión obligatoria de la Enfermería**.
  - Cada día alguien hace cada una, según la rotación.
- **Misión nueva**: si la rotación da a alguien una misión que nunca ha hecho, la **Magnate** aparece a su lado y lo inicia. Es la mímica: ella lanza los hechizos y el otro los repite, como en el yoga.
- **Personajes antiguos**: los que no son recién llegados (Andrea, Goluis…) son miembros desde el principio y entran en la rotación el primer día.

## Las misiones de Kushal se hacen con «E» (2026-11-04)

Berón: «para todas las tareas e iniciaciones vamos a utilizar el modelo de Ambrosio». Cada misión de la jornada de Kushal es
una **tarea de pasos con E** ([`tareas-del-jugador.md`](tareas-del-jugador.md)): al empezar su hora salen los marcadores
«E — acción» en los muebles de su puesto y la pista hacia el primero; la E en cada sitio hace el paso. Cuenta como hecha
**solo con todos los pasos** (o virtualizada); si la ignora, caduca **sin efecto**: la jornada ya no aplica el efecto de las
misiones de Kushal al acabar la hora. Las iniciaciones de cocina, huerto y sala de yoga piden cada paso igual («E — paso»
en el sitio del alumno).

## Máquina de virtualización (y de simulación)

Al usarla se elige:
1. **Virtualización**: las misiones de trabajo del área. Cada misión tiene **su propia escena** (`Virtual_<clave>`, menú «Build escenas de virtualización»): se recorren sus pasos en orden (puestos numerados, F) y al terminar se registra y aplica su efecto; por ejemplo, la obra avanza.
2. **Simulación**: el microcosmos del área (lo de siempre).
3. **Misiones de otras áreas**: todas las del catálogo son accesibles desde cualquier máquina (laboratorio, gimnasio, obra y guardia no tienen máquina propia).

La misión de la **Enfermería** es **iniciar a un turista**: recibirlo, análisis, vacuna, explicarle las normas y acompañarlo a su primera virtualización; el turista te sigue por la escena.

## Pendiente

- ~~Que el jugador quede bloqueado de verdad en su misión durante la jornada~~ Hecho con la Magnate (2026-10-22).
- Que Linduarda persiga al jugador por la enfermería pidiéndole que siga la iniciación.
- Iniciación con TeachingSession real (pasos de la misión) en vez del aviso de la Magnate.
- Turistas que llegan cada día y pasan por la iniciación de la enfermería.
- Misiones de reparación con sus pasos (tuberías, puertas, ventanas, tejados).

## Horario libre (2026-11-03)

Las horas libres (16–19 y 20–22 h) tienen ahora un **horario semanal** de ocio y cuidado con la misma lógica de rotación: docs/horario-libre.md (incluye un resumen de cómo reparte la jornada).

**Quién inicia en un área** (`AreaInitiator.PickWorker`, 2026-11-04): de los que tienen ahora misión en el área, primero quien está físicamente en ella; luego quien no está en la ronda de frutales de los techos (esa misión es del Huerto, pero se hace lejos); y si no hay otro, cualquiera de turno. Antes podía tocarle a quien estaba en el techo de Servicios, a 235 m, que no llegaba (GardenInitiationTest).

> **2026-11-04 — Hacer la actividad, comer con su orquesta y uniformes:** en su sitio cada uno HACE su tarea o su actividad (postura, herramienta en la mano, gesto y efecto), come con la orquesta de la comida (bandeja, cubiertos, bocados, beber, devolver) y lleva el uniforme de su tarea; también los insectos-humanos. Ver [actividades-y-uniformes.md](actividades-y-uniformes.md).

**Ronda 15 (2026-11-06):** el entrenamiento de las 14 es un destino al que se sale antes, como la comida; el de las 8 se pierde si la primera misión queda a más de una hora (`HeadingEarly`).

**Ronda 17 (2026-11-06):** a mediodía se come en lo que queda de la hora (mínimo el 40 por ciento), y la bandeja se deja en la mesa si la barra está en otra planta. Kushal no deja una misión con pasos sin hacer y, hecha su parte del entrenamiento, sale hacia la de las 15. El Sitio Nuclear entra en las rondas de máscaras.
