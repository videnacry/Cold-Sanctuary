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

## Reparto (rotación)

1. Cada día hay una **lista de misiones por área**. Primero van las que quedaron **pendientes** ayer, luego las **obligatorias** del día y después el resto.
2. Se recorre el **orden de áreas**. De cada área se toma una misión y se le da a la **primera persona** (orden fijo del personal) a la que aún le quepa en sus 5 h. Después se pasa a la siguiente área, y así en vueltas.
3. A esa persona se le da primero una misión del área que **nunca haya hecho**. Si no hay, la que **menos veces** haya hecho; si empatan, la que **hace más días** que no hace.
4. Las misiones se reparten por **tiempo**: cada una tiene su duración y no se parte en la comida.
5. El **orden de áreas rota** una posición cada día: la última pasa a ser la primera. El del personal no cambia, así la primera persona empieza cada día por un área distinta.
6. **Obligatorias sin cubrir**: la Magnate se **divide**. Cada clon toma 5 h de huecos, va a esos puestos y **desaparece** al terminar. Si mañana hay más personal, ya no hacen falta clones.
7. **El resto** pasa a pendientes y es lo primero que se asigna al día siguiente. Siempre se terminan todas las misiones.
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
