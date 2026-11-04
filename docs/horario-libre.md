# Horario de actividades libre (semanal)

Pedido de Berón (2026-11-03): además de la jornada diaria, un horario de actividades libres. Se fabrica cada semana llenando las horas libres de cada humano e insecto-humano. Sigue la lógica de reparto de la jornada: se recorre la lista de actividades de ocio y cuidado, se da a cada personaje la suya y se rota. Así las actividades se reparten por igual entre los personajes de la escena.

Código:
- `Assets/Scripts/Progression/LeisureRoster.cs`: el reparto, lógica pura.
- `Assets/Scripts/Progression/LeisureDirector.cs`: el horario en el juego y las etiquetas `LeisureTags`.
- `Assets/Scripts/UI/HorarioPanel.cs`: el panel.
- `Assets/Scripts/Tests/HorarioLibreTest.cs`: la prueba, grupo 65.

## 1. Cómo reparte la jornada (investigación)

`WorkRoster.Plan` (docs/jornada-y-rotacion.md) reparte cada día 5 h de trabajo por persona:

- **Pools por área**: primero las misiones pendientes de ayer, después las obligatorias y después el resto.
- **Misiones de grupo** (`AssignGroups`): la clase de yoga, el equipo de una obra. Van primero, todas a las 8:00, cada una a una persona distinta. El equipo empieza en un desplazamiento que avanza con el día (`day * 3`). Un recién llegado solo hace papeles de apoyo.
- **Vueltas** (`RunRounds`), una forma de round-robin por áreas: se recorre el orden de áreas del día. De cada área se toma una misión para la primera persona del orden fijo del personal a la que aún le quepa en sus horas.
  - **Capacidad**: las 5 h. Una misión no se parte en la comida: el bloque 1 dura 3 h y el bloque 2, 2 h.
  - Se pasa a la siguiente área, y así en vueltas mientras haya progreso.
- **Elección** (`Choose`): para esa persona, la misión del área que nunca hizo. Si no hay, la que menos veces hizo. Si empatan, la que hace más días que no hace. Las cuentas (`count`, `lastDay`) se guardan de un día a otro.
- **Rotación**: el orden de áreas rota una posición por día (la última pasa a ser la primera). El orden del personal es fijo, así cada día la primera persona empieza por un área distinta.
- **Huecos**:
  - Obligatorias sin cubrir: la Magnate se divide en clones de 5 h.
  - Resto sin cubrir: pasa a pendientes para mañana.
  - Balance: avisa si faltan personas o misiones.
- **Entrenamiento**: rota de forma modular. La persona *i* el día *d* va al entrenamiento *(i + d) mod n*.

## 2. El horario libre

### Horas libres

Las que no cubren la jornada, las comidas ni el sueño (`LeisureRoster.SlotHours`).

| Hora | Qué |
|---|---|
| 7:15–7:50 | Desayuno |
| 8–16 | Jornada (con la comida de 11 a 12) |
| **16, 17, 18** | **Libre** (franjas de 1 h) |
| 19–20 | Cena: las necesidades del cuerpo la piden |
| **20, 21** | **Libre** |
| 22–7 | Sueño |

Son 5 franjas al día y 35 a la semana por personaje. La semana tiene 7 días: el día 0 es lunes y la semana *w* va del día 7*w* al 7*w*+6.

### Reparto (una vez por semana, `LeisureRoster.Plan`)

Es la misma lógica que la jornada, dada la vuelta: allí se recorren las áreas y se busca persona; aquí se recorre el **catálogo de actividades** y se busca persona.

1. **En cada franja** se recorre el catálogo en su orden del momento. A cada actividad se le da **una** persona de las que aún no tienen actividad en esa franja: la que **menos veces** la ha hecho; si empatan, la que hace **más franjas** que no la hace; y si siguen empatadas, el orden fijo del personal.
2. **Vueltas**: se sigue con la siguiente actividad, y así en vueltas mientras quede gente sin actividad y plazas libres.
3. **Capacidad**: cada actividad tiene sus plazas (las del sitio). Descansar en casa no tiene límite: cada uno en la suya. Por eso nadie se queda sin actividad.
4. **El orden del catálogo rota una posición en cada franja**: la última pasa a ser la primera, como el orden de áreas cada día.
5. **Las cuentas se guardan de una semana a otra**, como `Worker.count`, así la rotación continúa.

**Uniformidad** (`LeisureRoster.Spread`): para cada actividad, la diferencia entre quien más y quien menos veces la hizo en la semana. Es la medida de un reparto uniforme entre personajes.

| Caso | Uniformidad (Δ) |
|---|---|
| Santuario: 15 personas, 10 actividades | ≤ 1 (simulado y en la prueba: 1 y 0) |
| Plazas justas: 40 personas | ≤ 3 |

También se mide el Δ por persona: la diferencia entre su actividad más hecha y la menos hecha. Solo es 0–1 cuando hay plazas de sobra; con plazas justas, casa absorbe el exceso.

### Catálogo y sitios

**Santuario** (`LeisureDirector.SanctuaryActivities`). Cada actividad va a la primera área abierta de su lista; las obras sin terminar no cuentan.

| Actividad | Sitio | Plazas |
|---|---|---|
| Pesas y saco (ejercicio: sentadillas) | Gimnasio | 6 |
| Hatha libre y meditación junto al linga | Sala de Yoga · Hatha tradicional (piso de Hatha; cada plaza, una esterilla; [yoga-pisos.md](yoga-pisos.md)) | 8 |
| Ashtanga autoguiado (estilo Mysore) | Sala de Yoga · Ashtanga occidental (piso de Ashtanga) | 8 |
| Pasear y cuidar el huerto | Huerto | 6 |
| Leer en la plataforma de la arboleda | Arboleda residencial | 6 |
| Jugar con las crías | Cría | 4 |
| Manualidades | Estudio Textil | 4 |
| Tertulia | Restaurante («Cocina») | 8 |
| Bailar | Sala de Ballet → Calistenia → Gimnasio | 6 |
| Artes marciales con Gohageneis (antes con Goku, que se fue a su microcosmos el 2026-11-04) | Dojo → Gimnasio | 6 |
| **Guerra de almohadas** (2026-11-04): TODOS A LA VEZ, solo el sábado a las 20 h (`together`); la añade el ring como etiqueta de actividad ([guerra-de-almohadas.md](guerra-de-almohadas.md)) | Ring de almohadas (cama colosal sobre la plataforma central) | las del ring (hoy 25) |
| **Pijamada** (2026-11-04): el viernes a las 21 h, con el anfitrión de la semana (rota) y los que menos llevan; esa noche duermen allí ([pijamadas.md](pijamadas.md)) | La casa del árbol del anfitrión (la cama-litera de 8) | 8 (por turnos) |
| Descansar en casa (tumbado en su plaza, abajo, de la cama-litera) | Su casa del árbol | sin límite |

**Microcosmos** (`MicroActivities`). La plantilla de edificio y la función del registro que se hace allí. Las plazas son los puestos de esos edificios. Solo entran las plantillas que hay en la escena.

| Actividad | Plantilla del edificio |
|---|---|
| Practicar | Dojo |
| Yoga y meditación | Sala de Yoga |
| Meditar o rezar | Templo |
| Pasear | Huerto |
| Leer y estudiar | Escuela |
| Pasear | Mercado |
| Tertulia | Restaurante |
| Descansar | Su casa |
| Cuidar de la familia | Su casa |

**Etiquetas de actividad** (`AreaFunctionSite.activity`, `capacity` e `interaction`, que pone el constructor de los pueblos; docs/microcosmos-actividades.md). El horario las lee así:

- **Si la escena tiene edificios etiquetados**, el tiempo libre va **solo a los de ocio, recuperación y descanso**, nunca a los de trabajo. En un pueblo etiquetado, la escuela, el mercado y el huerto son trabajo y salen del catálogo libre.
- **Los demás edificios de ocio o recuperación** (parque, plaza, museo, sala de baile, baños, mirador, centro de entrenamiento, fuente…) entran como una actividad por plantilla, con su interacción como título y la función de «todos» para el gesto. El hospital no entra.
- **Las plazas** son la capacidad de la etiqueta; sin etiqueta, los puestos del edificio que sirven para la función.
- **Sin etiquetas** mandan las plantillas de arriba.

Otros sistemas pueden añadir sitios con `LeisureTags.Providers`.

### En el juego (`LeisureDirector`)

**Un grupo por escena**:
- Santuario: el personal de `WorkdayDirector.staff`, sin Kushal; el jugador es libre.
- Cada microcosmos cargado: sus insectos-humanos con `MicroJornada`, sin los niños que siguen a su madre.

Se vuelve a repartir al cambiar la semana o la gente.

**En su franja**, a cada uno:
- Lo lleva a su sitio:
  - en el santuario, andando con `PathMover` y en alfombra con `CarpetPad.Route` si es otro piso;
  - en el microcosmos, con `MicroMove`. Si en 60 s no llega se le pone allí, como en `MicroJornada`, y se cuenta como `fallback`.
- La hace:
  - personas: con postura (sentado, meditar, sentadillas, baile girando, tumbado en la cama);
  - insectos-humanos: con el gesto de la función.
- Lo pone en su rótulo: «Libre: …».

**Respeta a los demás sistemas**:
- **Van antes y el horario espera**:
  - las necesidades del cuerpo (comer, el baño, dormir, ir a la cama);
  - las coreografías (`Orchestra`);
  - la Magnate;
  - el servicio del restaurante;
  - una historia que lo retiene (`MicroJornada.hold`);
  - Linduarda con un recién llegado.
- **Ceden en esas horas**: `WorkdayDirector.Drive`, en la rama de tiempo libre, y `MicroJornada.Update`.
- **Se suspenden y se devuelven al acabar**: el merodeo (`IdleBehavior`) y la conducta propia de Linduarda.
- **Durante las pruebas** el horario no mueve a nadie (como la jornada), salvo si una prueba lo pide (`LeisureDirector.ForceRun`). Por eso la prueba de la jornada (grupo 50) no cambia.

**Registro** (`LeisureDirector.Records`): por franja y persona, qué anduvo, en alfombra, si llegó, cuánto la hizo, si esperó a una necesidad y los saltos.

### Panel y consola

Botón «Horario libre» abajo a la izquierda, o en la consola `horario` (también escribe el horario de hoy). El panel muestra:
- la semana de la escena, con un selector de día y otro de escena;
- por personaje, las franjas del día;
- en la franja de ahora:
  - ✓ la hace;
  - → va hacia allí;
  - ‖ espera a una necesidad;
- el reparto: franjas, sin llenar, capacidad y uniformidad;
- el catálogo con sus plazas.

## 3. Prueba (grupo 65, `HorarioLibreTest`)

1. **Lógica**:
   - 16 personas: las 35 franjas llenas, sin pasar plazas, Δ ≤ 1 dos semanas seguidas, y el catálogo rota.
   - 40 personas: lleno y Δ ≤ 3.
2. **Los seis del mesocosmos**: en escena, con cuerpo de cubos, rótulo con su papel, en la jornada (reciben misiones) y con su casa del árbol. Retratos en `Temp/Horario/<id>.png`.
3. **Santuario en vivo**:
   - el horario de la semana: cada hora libre llena, capacidad y uniformidad medida (Δ ≤ 2);
   - de 16 a 18 h, a ×6:
     - ≥ 70 % de los que no atendían una necesidad llegan;
     - ≥ 80 % de los que llegan, andando o en alfombra;
     - ≥ 90 % de los que llegan, la hacen;
     - sin saltos;
     - el rótulo lo dice.
4. **Microcosmos** (Kapilavastu o el siguiente que haya): lo mismo con sus insectos-humanos, de 16 a 17 h.

La jornada sigue intacta: se vuelve a correr el grupo 50.

**Lejos, en el microcosmos** (2026-11-04): en los pueblos ×100 los sitios quedan a cientos de metros. Si está lejos, el insecto-humano va a **paso ligero** (hasta 6 m/s, para llegar en unos 75 s) y tiene más tiempo para llegar andando (de 60 a 120 s de juego, según la distancia). Si aun así no llega, se le pone allí con `Warp` (NavMeshAgent) y cuenta como `fallback`. Los saltos solo se cuentan mientras lo lleva el horario: lo que lo mueva una necesidad no cuenta.

**A dónde se anda en el microcosmos** (2026-11-04, ronda 3): entre los puestos de los edificios de la actividad se elige uno con **camino completo** por el NavMesh (con sus trepadas) y se anda a su punto del NavMesh. Si no hay ninguno, se va al final del camino parcial si queda junto al edificio (≤ 15 m). Se da por llegado también cuando el agente está al final de su camino a ≤ 4 m del puesto.

**El panel enseña la escena correcta**: la que fija una prueba (`HorarioPanel.FocusScene`), la que se eligió con «otra escena» o, si no, la escena donde está el jugador.
