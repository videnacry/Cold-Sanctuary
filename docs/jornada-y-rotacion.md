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
| 8:00–11:00 | Trabajo (3 h) |
| 11:00–12:00 | Comida, en el restaurante |
| 12:00–14:00 | Trabajo (2 h) |
| 14:00–16:00 | Entrenamiento (2 h): virtualizaciones de yoga, gimnasio… |
| 16:00 → | **Libre**: cada uno va donde quiere y hace lo que quiere (el jugador también) |

Durante las 8 h los personajes van al área de su misión y se quedan allí. **Nadie queda encerrado** (2026-10-22): si alguien abandona su puesto, la **Magnate** (que conoce todos los hechizos, también teletransportarse) aparece de pronto delante de él, le dice que vuelva y lo teletransporta de vuelta dentro del área (`World/MagnateWarden.cs`). El jugador ve su itinerario en pantalla. Consola: `jornada` (plan del día y equilibrio).

## Reparto (rotación)

1. Cada día hay una **lista de misiones por área**. Primero van las que quedaron **pendientes** ayer, luego las **obligatorias** del día y después el resto.
2. Se recorre el **orden de áreas**. De cada área se toma una misión y se le da a la **primera persona** (orden fijo del personal) a la que aún le quepa en sus 5 h. Después se pasa a la siguiente área, y así en vueltas.
3. A esa persona se le da primero una misión del área que **nunca haya hecho**. Si no hay, la que **menos veces** haya hecho; si empatan, la que **hace más días** que no hace.
4. Las misiones se reparten por **tiempo**: cada una tiene su duración y no se parte en la comida.
5. El **orden de áreas rota** una posición cada día: la última pasa a ser la primera. El del personal no cambia, así la primera persona empieza cada día por un área distinta.
6. **Obligatorias sin cubrir**: la Magnate se **divide**. Cada clon toma 5 h de huecos, va a esos puestos y **desaparece** al terminar. Si mañana hay más personal, ya no hacen falta clones.
7. **El resto** pasa a pendientes y es lo primero que se asigna al día siguiente. Siempre se terminan todas las misiones.
8. **Entrenamiento**: rota entre los entrenamientos abiertos (persona *i*, día *d* → *(i + d) mod n*).

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
  - Limpieza e inventario (1 h)
- **Cocina**:
  - Servicio del restaurante* (2 h)
  - Limpieza e inventario* (1 h)
  - Obrador (2 h)
  - Atender en sala (1 h)
- **Huerto**:
  - Riego y deshierbe* (2 h)
  - Cosecha (1 h)
  - Alimentar a los visitantes (1 h)
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

- **Abiertas**: Sala de Yoga y **Gimnasio**: jaulas, bancos, mancuernas, cintas, remo y saco.
- **En obras**:
  - Sala de **Ballet**: 5 días.
  - **Dojo** de artes marciales: 6 días.
  - Parque de **calistenia**: 4 días.
- Cada obra tiene a pie de obra el **material de un día** (ladrillo, cemento, tablones), andamio y paredes a medio levantar.
- Con «llevar material» y «trabajar la obra» las paredes suben y el cartel cuenta los días.
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
