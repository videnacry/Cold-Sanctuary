# Tareas del jugador con «E» (el modelo de Ambrosio)

Berón (2026-11-04): «de momento para todas las tareas e iniciaciones vamos a utilizar el modelo de Ambrosio de modo que se
pueden resolver pulsando "E" en los lugares indicados … quizás solo el jugador no los está haciendo?».

Tenía razón: hasta ahora la misión de jornada de Kushal se daba por hecha **solo con estar en su puesto** la mitad de la hora,
y su **efecto** en el santuario (el barro del almacén, el servicio de la carta, el lote del biorreactor…) lo aplicaba la
jornada al acabar la hora **aunque no hubiera hecho nada**. En las iniciaciones de área el jugador tenía que escribir `step`
(o el asana); si no, a los 30 s el trabajador lo hacía con él.

## El modelo de Ambrosio

En el primer microcosmos (`AmbrosioStory`, escena `Microcosmos_Scene1_Ambrosio`) **una sola tecla, la E**, hace lo que toca
según dónde está el jugador (`AmbrosioStory.Act`): junto a Shanidar, «empujas a Shanidar»; junto a Ambrosio caído, «tomas su
melaza»; junto a Shanidar con la melaza, «se la das». El objetivo sale en pantalla (`LessonPrompt`) y lejos del sitio la E
contesta «acércate…». Lo mismo hacen `KitchenOriginStory`, `SakshiRiverStory` y `StoryMission`.

## Lo que se ha generalizado

Código: `Assets/Scripts/Progression/PlayerTasks.cs` (el catálogo y los sitios) y `PlayerTaskRunner.cs` (quien los ejecuta).

1. **Una misión = una secuencia de pasos.** Cada paso tiene su **acción** («Riega el bancal»), su **puesto** (la clave genérica
   del mueble, las mismas de `AreaFunctions`: `bancal`, `fogon`, `camilla`, `esterilla`, `telar`, `consola`…), el **objeto**
   que se le da («Regadera») y el **gesto** (dos fotogramas, según la clase de la función: cocinar, limpiar, cultivar,
   curar, operar, enseñar, servir, vigilar…).
2. **Marcadores en el mundo**: un poste de luz con un rombo que gira y su rótulo **«E — acción»** en el sitio de cada paso.
   El actual es grande y ámbar; los siguientes, pequeños y azules; los hechos desaparecen.
3. **Aviso y pista**: en el sitio (≤ 3 m en planta, mismo piso) sale en grande «E — acción»; lejos, el panel de la tarea da la
   **pista de ruta**: «→ Telar: 23 m, ↗ delante a la derecha · sube 8 m (otro piso: alfombra o nadando)».
4. **Pulsar E en el sitio hace el paso**: el cuerpo de Kushal hace el gesto 2,5 s (quieto, como al empujar a Shanidar), se le
   da el objeto (en la mano y en el inventario de tareas), se aplica el efecto del paso si lo tiene y se pasa al siguiente
   sitio. Lejos del marcador la E no hace nada (pide acercarse).
5. **Progreso**: panel a la izquierda con «✓» los hechos, «▶» el actual y «·» los que faltan.
6. **Cierre**: al hacer el último paso se aplica **el efecto de la misión** (`AreaFunctions.Run`: el mismo que antes aplicaba
   la jornada), se marca en el progreso (`Progress.MarkMission`), Kushal da las gracias y la misión cuenta como hecha.

### Dónde está cada sitio

`PlayerTasks.Resolve` busca el mueble del puesto **en el área de la misión** (todas las `SanctuaryArea` con ese nombre; el
Huerto también en `Garden_Area`; la Cocina también en el restaurante):
- primero por componente: bancal → `GardenPlot` (en su borde), biorreactor → `CulturedMeatLab`, máquina → `GymStation`
  (su punto de acceso), esterilla → la sala de Hatha del día (`YogaHall`), obra → la `ConstructionProject` de la misión;
- si no, por nombre (sin tildes): «Telar», «Fogón», «Camilla», «Pizarra», «Consola»… (tabla `Keywords`);
- el paso n-ésimo con el mismo puesto va al n-ésimo mueble (los pasos se reparten por el área);
- si el área aún no tiene ese mueble, **un punto del área** (y si el área no está en la escena, junto al jugador): la tarea
  nunca se queda sin sitio. La prueba cuenta cuántos marcadores caen en un mueble y cuántos en un punto del área.

## La jornada de Kushal

`PlayerWorkday`:
- Al **empezar la hora** de cada misión suya empieza su tarea: salen los marcadores y la pista hacia el primer sitio, y el
  aviso «Ahora: … sigue los marcadores y pulsa E en cada sitio (n pasos)».
- La misión está **hecha** si hizo **todos los pasos** con E (o si la practicó en la máquina de virtualización). Estar en el
  puesto ya no basta (se sigue midiendo, para el registro y para salir antes hacia la siguiente).
- Si la **ignora**, al acabar su hora **caduca** como antes («NO completa»): fuera los marcadores y **sin efecto**.
- **Nadie la hace por él**: `WorkdayDirector.Finish` ya no aplica el efecto de las misiones de Kushal (lo aplica su tarea o la
  virtualización). Los demás no reciben su misión: la jornada nunca movía al jugador y `DoingNow`/`InArea` no lo cuentan.
- El **piloto de las pruebas** (`PlayerWorkday.autopilot`, `JornadaTest`) es el jugador simulado: ya en su puesto, pulsa la E
  de cada paso (`PlayerTaskRunner.PressAnywhere`, sin ir a cada marcador).

## Las iniciaciones de área

`AreaInitiator` (cocina, huerto y sala de yoga) ofrece **cada paso que pide** como paso en vivo
(`PlayerTaskRunner.Live`): un marcador «E — paso» en el sitio del alumno (la mesa de emplatado, el borde del bancal, la
esterilla) y la E allí lo lanza; es lo mismo que escribir `step` o el asana, que siguen valiendo.
- **Cocina**: la E en la mesa hace `CookingStation.AdvanceStep` (la E «en cualquier sitio» de la estación del jugador se
  apaga mientras dura la iniciación) y el cuerpo hace el gesto de cocinar.
- **Huerto**: la E en el borde del bancal hace `AreaInitiator.StudentStep` (abono, esparcir, azada, surcos, rastrillo).
- **Sala de yoga**: la E en la esterilla lanza el asana que pide el maestro (`TeachingSession.CastCurrentStep`).
- La ayuda del trabajador al vencer el tiempo (30 s por paso) se mantiene para que nunca se atasque.

## Misiones cubiertas

Con pasos escritos (`PlayerTasks.AuthoredIds`): **todo el catálogo de la jornada** (`AreaFunctions.SanctuaryMissions`, 66)
— hospital (10), cocina (8), huerto (3), sala de yoga (6), mecánica (4), estudio textil (3), carne cultivada (4), construcción (3),
cría (3), gimnasio (2), planta atómica (5), química (2), biología (2), central nuclear, guardia, informática (3), aula (5) y control de misión (1) —,
las del **material** (`mat_guadua`, `mat_bahareque`, `mat_micelio`, `mat_cuerda`), los **fotobiorreactores**
(`bio_fotobio`) , la **lanzadera** (`lanz_control`, `lanz_seguridad`, `lanz_fabrica`) y el **Parque Energético** (`sol_mantenimiento`, `sol_instalacion`, `sol_reparacion`, `ene_control`, `ene_eolica`, `ene_mareal`, `ene_almacen`; sus puestos, en el casquete: `panel`, `aerogenerador`, `detector`, `pontona`, `sonar`, `bateria`, `silo`). Por prefijo: las **elaboraciones**
(`prep_*`), las **obras** (`obra_material_*`, `obra_trabajo_*`), las **reparaciones** (`rep_*`, el paso de reparar va al área
rota) y la **ronda de frutales** (`jar_frutales*`). Cualquier otra (las de los edificios de los microcosmos) sale con tres
pasos genéricos de su clase y su puesto: nunca sin pasos. Una misión nueva del santuario debe traer sus pasos escritos: si no, la
prueba 96 falla y la nombra (id, título y área).

Iniciaciones: **cocina, huerto y sala de yoga**.

**Pendiente**:
- La **iniciación médica** de llegada (`NewcomerInitiator`, el examen de la enfermería): ahí el recién llegado no hace pasos
  (se sienta, se tumba; lo examina el trabajador). Si Berón quiere que el jugador colabore, se añadirían pasos con E.
- El **gimnasio** no tiene iniciación (las rutinas son de otro hilo); la **clase de yoga en grupo** (`yog_guia`, `yog_demo`,
  `yog_corrige_*`) se hace con E pero no mueve la clase real (`YogaClass`) cuando el que la da es Kushal.
- Efectos propios por paso (regar el bancal de verdad, servir un plato concreto): hoy el paso da el objeto y el gesto, y el
  efecto en el santuario llega al cerrar la misión.

## La E es solo la de interactuar

Desde el 2026-11-04 la E no hace nada más en el santuario: la parte del cuerpo «Manos» del menú de posturas pasa a la
**N** (su botón y su holograma muestran «[N]») y el hechizo de la melaza de Kushal (`HoneydewSpell.castKey`) al **punto (.)**.
Las teclas libres se buscaron entre todas las `KeyCode` del código (letras libres: solo N; además «,», «.» y F2–F9). Los
microcosmos de historia (Ambrosio, la cocina del fuego, el río, `StoryMission`) ya usaban la E solo para interactuar. Las
escenas guardan la tecla: hay que reconstruir SampleScene para que cambie.

## Prueba

`PlayerTasksTest` (**grupo 96**): catálogo y marcadores de todas las misiones posibles; tres misiones en tres áreas
(Construcción `mat_bahareque`, Estudio Textil `mat_cuerda`, Huerto `mat_guadua`): la E lejos no hace nada, en cada sitio hace
el paso (gesto y objeto) y avanza, al final se cierra y el almacén de material sube; una tarea que caduca se quita sin efecto;
las iniciaciones del **huerto** y la **cocina** hechas solo con E en vivo (todos los pasos del jugador, ninguno ayudado; suelo
preparado / stock arriba). `JornadaTest` (50) pide ahora que el piloto haga los pasos con E.
