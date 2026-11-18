# Plan: microcosmos con escala, edificios y jugabilidad de CodeShip

Encargo de Berón (2026-11-18): las sillas, mesas y platos parecen juguetes. Hay dudas sobre la escala de los árboles. Los microcosmos de Musk, Atahualpa y Descartes no recrean lugares reales: no hay edificios, transporte ni estructuras de época. Además, no se entiende qué hacer. Pide traer a los microcosmos la UI/UX de CodeShip: menú árbol, retos de código, listas de tareas y áreas con viaje automático, y estructuras fieles con objetos escalados.

Este documento es solo investigación y plan; no se ha cambiado código. Las referencias de Cold Sanctuary son de `claude/edificios`. Las de CodeShip son de `origin/integracion` @ `1198d23`; la copia local en `develop` es de 2023 y no sirve de referencia.

---

## 1. Auditoría de escala, medida en el código

### 1.1 El cuerpo del mirmidón (hormiga, plan Centauro)

Datos de origen:
- `InsectMorphology.cs:93`: segmentos en unidades de L (cabeza 0,22, tórax 0,30, pecíolo 0,06, gáster 0,36) y `bodyHeight` 0,12.
- `CivicBuilder.cs:710`: estatura erguida `Random.Range(1.6f, 1.85f)`.
- Fórmulas de `MicroBody.OfSpecies` y `ForSpec` (`Assets/Scripts/Microcosmos/MicroBody.cs:89-115`).

Calculado para la estatura de referencia, 1,75 m (L = 1,75 / 0,86 = 2,03 m):

| Medida | Valor | Comparación |
|---|---|---|
| Estatura erguida (torso) | 1,60–1,85 m | Igual que una persona |
| **Largo del cuerpo** (cabeza → gáster + 0,25) | **2,16 m** | Una persona tumbada ocupa ~1,95 m |
| Ancho del cuerpo | 0,45 m | Hombros humanos ~0,45 m |
| Ancho con las patas recogidas | 1,02 m | Una persona, 0,5 m |
| Altura del vientre | 0,24 m | |
| Largo del abdomen (gáster) | 0,73 m | |
| Manos (mesa, lavabo) | 0,96 m | En una persona sentada, 0,70 m |
| Radio del agente NavMesh | 0,40 m (`MicroBuildRules.AgentRadius`) | **Es un círculo de persona; el cuerpo mide 2,16 m de largo** |

### 1.2 Muebles y objetos

| Objeto | Medida actual | Fuente | Qué falla |
|---|---|---|---|
| Asiento de mirmidón («silla de montar») | Altura `belly + 0,08` = **0,32 m**; tablero **0,34 × 0,83 m** | `MicroFurniture.SeatOn`, :449 | Es un taburete bajo y estrecho bajo un cuerpo de 2,16 × 1,02 m. Desde la cámara apenas se ve: es la «silla minúscula» de la que habla Berón. |
| Silla simple (`PlainChair`) | Asiento 0,42 × 0,42 m a 0,45 m; respaldo a 0,72 m | `MicroFurniture.cs:1096` | Medida humana con respaldo; un centauro no cabe. |
| Banco (`Bench`) | Asiento a 0,44 m, 0,32 m de fondo | `MicroFurniture.cs:1105` | Medida humana. |
| Mesa de mirmidones | Altura `manos − 0,12` = 0,84 m; fondo **0,9 m**; largo ≥ 1,2 m | `TableWithSeats`, :587-592 | El fondo es humano; el largo solo crece con el ancho de los asientos. |
| Plato | **0,26 m** (mínimo de `MicroCivicScale.PlateFor`) | `MicroSeat.cs:34`, `KitchenProps.Plate` (0,24) | Tamaño real de un plato humano. Junto a un cuerpo de 2 m y a cámara alejada parece de juguete. |
| Cuenco | 0,16 m | `KitchenProps.Bowl` | Ídem. |
| Jarra o taza | 0,14 × 0,10 m | `TableWithSeats`, :606 | Ídem. |
| Cuchara o palillos | 0,20 × 0,01 m | :604 | Casi invisible (1 cm de grueso). |
| Sartén y olla | 0,26 m y 0,22 × 0,14 m | `KitchenProps.Pan/Pot` | Medida real, sin aumentar. |
| Paso libre entre muebles | 2,0 m (`Passage`) | `MicroBuildRules.cs:24` | Calculado para dos personas erguidas de 0,8 m. **Un mirmidón de 2,16 m no gira en 2 m.** |
| Calle | 4,0 m (`Street`) | :26 | Un carro de época mide 2–2,5 m de ancho. |
| Muro | Prohibido por encima de **1,2 m** (`WallHeight`) | :30 | Impide cualquier edificio. |
| Techo | Prohibido por encima de **2,2 m** (`RoofHeight`) | :32 | Ídem. |
| Puertas | **No hay**: sin muros no hacen falta | regla S6 del manual | Tampoco hay umbrales ni portadas que marquen un lugar. |
| Árbol del pueblo | Escala real 1:1 (tilo de 15 m, seto de 0,8 m, césped de 5 cm), uno por cada ~25 m², tope de 25 m | `AreaMicrocosmos.cs:1460-1520`, manual M12 | **La escala del árbol es correcta.** Lo que lo hace parecer de juguete: es un tronco y dos cubos de copa (:1532-1534), y el pueblo está rodeado por el **santuario a ×100** (una cama de 200 m, la silla convertida en «nevado»; manual M11 y F3). |
| Transporte | Solo aparecen «Coche» y «Carro» en `CivicBuilder.Plans.cs:1218` | grep | Ningún microcosmos tiene vehículos de época que se usen. |

### 1.3 Diagnóstico

1. **El problema no es una medida suelta, es la referencia.** Todo se dimensiona para una persona de 1,75 m (`MicroCivicScale.Citizen`), pero el cuerpo que se ve es un centauro de **2,16 m de largo y 1 m de ancho**. A su lado, un mueble humano parece pequeño al ojo aunque sea del tamaño correcto.
2. **El asiento a medida es demasiado pequeño para leerse.** Un taburete de 0,32 m bajo el abdomen queda tapado por el propio cuerpo. Las hormigas reales descansan de pie sobre seis patas, así que el asiento es redundante.
3. **No hay factor de presentación.** CodeShip comprobó con medidas en el móvil que a 1× la maquinaria se ve pequeña y fijó **1,5×** (sección 4.5). Cold Sanctuary dibuja platos, tazas y herramientas a 1×.
4. **El contexto empequeñece.** Un pueblo a 1:1 dentro de un piso a ×100 (camas de 200 m) se ve como una maqueta.
5. **Sin muros no hay lugar.** La regla 2 («sin muros ni techo», `MicroBuildRules.Validate` y `GlassWallTest`) borra lo que hace reconocible un sitio: fachadas, portadas, naves y torres. Por eso Hawthorne, Cajamarca y Leiden parecen el mismo descampado amueblado.

---

## 2. Auditoría de necesidades básicas

Hay tres clases de figuras en un microcosmos, y no todas viven igual.

| Figura | Qué la mueve | Trabajar | Comer | Dormir | Retrete |
|---|---|---|---|---|---|
| **Insecto-humano civilizado** (los vecinos que añade `CivicBuilder.Civilize`) | `MicroJornada` (`Assets/Scripts/Microcosmos/MicroJornada.cs:83-91`: trabajo, comida, trabajo, práctica, casa) y `PersonLife.Equip` → `NeedsDriver.HomeNeeds` (`Assets/Scripts/Body/NeedsDriver.cs:83-109`) → `HomeNeedSpell` (`Household.cs:211`) | Implementado; **en el microcosmos solo se prueba que encuentre su puesto** | Implementado y **verificado** en el comedor (`ActividadesTest`, :314) | Implementado, unido a su cama; **sin prueba en el microcosmos** | Implementado, unido a su retrete; **sin prueba en el microcosmos** |
| **Personaje de la historia** (Musk, Atahualpa, Descartes…) | `MicroRoutine` (`MicroRoutine.cs:9-30`): anda de parada en parada | Decorado | No | No | No |
| **Recolectora / Mirmidón_N** (eras sin pueblo) | `MicroForager` (`AreaMicrocosmos.cs:1209-1293`) | Lleva comida al nido | No | No | No |

**Los muebles están enlazados.** `CivicBuilder.MakeHousehold` (`CivicBuilder.Homes.cs:138-198`) llena `Household` con camas, retrete, lavabo, mesa y despensa, y `MicroJornada` dirige a cada uno a su `AreaFunctionStation`. Las letrinas públicas, los bancos de la plaza y las paradas de `MicroRoutine` son decorado. `MicroBuildRules.Validate` (:270, :355) solo comprueba que la pieza de cada necesidad exista, no que alguien la use.

**Pruebas que hay:**
- `CivilizedErasTest`: estructural (horario :40, puesto :106, hogar completo :137, componentes :139).
- `ActividadesTest`: come y trabaja con herramienta (:276-314). Si no hay microcosmos cargado, la prueba se salta en silencio (:308).
- `CicloHumanoTest` (:58-67): comer, retrete, lavarse y dormir, pero en la escena `CicloVida_InsectoHumano`, no en un microcosmos.

**Qué falta:**
1. Una prueba en vivo dentro de un microcosmos que vea a un vecino dormir en su cama, ir al retrete y trabajar una jornada entera. Debe fallar si `MicroJornada.Fallbacks` (los teletransportes al puesto tras 60 s, :128-132) es mayor que 0.
2. Los personajes de la historia no tienen cuerpo con necesidades. Deben pasar a `MicroJornada` y `NeedsDriver`, y su historia se convierte en el «trabajo» de su jornada.
3. Que `ActividadesTest` falle, y no se salte, cuando no hay microcosmos cargado.

---

## 3. Nueva escala y reglas de diseño para insectos-humanos

Esta propuesta cambia reglas. Según la regla de cambios de reglas, cada cambio aprobado obliga a auditar lo que ya existe y a reescribir las reglas y pruebas afectadas: manual §7.4 (S6), `MicroBuildRules`, `GlassWallTest` y `MicroBuildRulesTest`.

### 3.1 Escala (reglas E1–E6)

- **E1. Referencia de cuerpo = largo.** Todo lo que en el mundo humano se mide «para una persona de 1,75 m» (puertas, pasos, camas, mesas, vehículos) se mide para el **largo del cuerpo** del mirmidón, `MicroBody.Fit.length`, unos 2,16 m. Esto da un factor de cuerpo **k_c = largo / 1,75 ≈ 1,25**. El torso sigue a estatura humana, así que lo que se toca con las manos (alturas de mesa y de mostrador) se queda en `Fit.hands`.
- **E2. Escala de presentación 1,5×, como CodeShip.** Es una sola constante, `MicroBuildRules.Presentation = 1.5f`. Multiplica la **planta y lo que se ve**: largo y fondo de muebles, platos, tazas, herramientas, pantallas, rótulos, fachadas y salas. **No multiplica** las alturas de contacto (manos, apoyo del abdomen, cama). Es la regla de CodeShip `escala.md` («no se escalan los personajes ni donde su cuerpo toca la máquina»).
- **E3. Factor total de objetos de mano.** Platos, tazas, cubiertos y herramientas = real × k_c × 1,5 ≈ **real × 1,9**. Un plato pasa de 0,26 a **0,49 m**, una taza de 0,10 a 0,19 m y una cuchara mide al menos 3 cm de grueso. Lo mismo para todo `KitchenProps` usado en microcosmos.
- **E4. Pasos para el cuerpo largo.** `AgentRadius` pasa a medio largo del cuerpo (≈ 1,1 m), o se usa un agente de NavMesh propio para centauros. Paso libre = largo + 0,6 m = **2,8 m** en datos. Calle = 2 pasos + un carro = **8 m**. Dibujados a 1,5×.
- **E5. El pueblo no comparte encuadre con el santuario ×100.** Alrededor del barrio del pueblo, el mundo gigante se tapa con niebla de distancia, o con un horizonte (colinas, muralla) que lo oculta desde el suelo del pueblo. Así no hay una cama de 200 m junto a un plato de 0,5 m.
- **E6. Árboles.** Se mantiene la escala real (15 m), pero con copa de varias piezas (ramas y 3–5 masas de follaje) en lugar de dos cubos. La presentación 1,5× no se aplica a árboles ni a edificios en altura (su altura ya es la real), solo a su planta si hace falta para el paso.

### 3.2 Asientos, camas y verticalidad (reglas C1–C4)

- **C1. Comer y trabajar de pie.** Una hormiga descansa de pie sobre seis patas. El mueble por defecto es el **mostrador o la mesa alta** a la altura de sus manos (~0,95 m), con un **apoyo de abdomen**: una barra o cojín inclinado bajo el gáster, como una misericordia de coro. Mide 1,5 × el ancho del cuerpo y queda a la vista detrás de cada comensal.
- **C2. La silla de montar solo donde la época lo pide** (trono, tribuna, carruaje). Su medida será el largo del abdomen + 0,3 m y la altura del vientre + 0,15 m, con presentación 1,5×, para que se lea como mueble.
- **C3. La cama sigue siendo un lecho de vientre** al largo del cuerpo + 0,3 m, sin cambios de forma, con presentación 1,5× en planta.
- **C4. Sin escaleras ni ascensores para moverse.** Trepan las paredes (`SurfaceWalker`, enlaces `NavMeshLink` de la regla M8). **Excepción:** con más de **12 m** de altura (más de 3–4 plantas, como un rascacielos o una torre) hay ascensor o montacargas, si el lugar real lo tenía. Las escaleras históricas famosas (escalinatas, pirámides) se construyen como **monumento**, no como camino obligado.

### 3.3 Muros y edificios: vuelven, con vista de corte (reglas P1–P7)

- **P1. Cada microcosmos recrea su lugar real** con planta, alzados y materiales documentados, cada dato con su fuente y su etiqueta (real / supuesto / ficción), igual que CodeShip `reglas.md` §1.1. Se termina la regla 2 («sin muros ni techo») en los microcosmos.
- **P2. Muros y techos visibles desde fuera. Por dentro, vista de corte.** Al entrar el jugador o acercarse la cámara, el techo y los muros entre la cámara y el jugador se desvanecen, como en una casa de muñecas. Se cumple a la vez «los muros tapan la vista» y «se reconoce el edificio».
- **P3. Todo recinto que existe en la realidad se puede recorrer.** Es CodeShip §2.1. Una puerta solo da paso, nunca abre menús (CodeShip §2.2).
- **P4. Puertas a medida del cuerpo:** ancho = ancho con patas + 0,8 m ≈ **1,8 m**; alto = estatura + 0,6 m ≈ **2,4 m**; presentación 1,5× en el ancho. Para entrar no hace falta puerta: también se puede trepar por la fachada.
- **P5. Un hito reconocible por microcosmos,** visible desde la llegada. Por ejemplo, el Falcon 1 en Omelek o la nave de SpaceX en Hawthorne; la kallanka y la plaza triangular de Cajamarca; el Rapenburg y la universidad de Leiden.
- **P6. Al menos un transporte de época que se use** (se sube, se mueve y lleva carga): carro, barca de canal, litera inca, camión o coche. Se integra con el viaje automático (sección 4.2) como trayecto.
- **P7. Densidad de época:** rótulos, carteles, objetos del oficio y gente trabajando en su puesto (la regla de dos personas por puesto sigue). Lo que hoy es un «solar abierto con muebles» pasa a ser un **interior** con muebles.

---

## 4. Sistemas de CodeShip para portar, por prioridad

CodeShip es TypeScript con three.js y UI en HTML; no se puede copiar código. Se porta el **diseño, los datos y la lógica pura** a C# en Unity. La UI de Cold Sanctuary es IMGUI: hay 55 archivos con `OnGUI`, entre ellos `Assets/Scripts/UI/HudModals.cs`, `HudGate.cs`, `ChoiceDialog.cs` y `LessonPrompt.cs`. Se sigue en IMGUI para no abrir otro frente.

Tamaños de esfuerzo: S (1–2 días), M (3–6 días), L (más de una semana).

| # | Sistema | En CodeShip (origin/integracion) | Dónde se engancha en Cold Sanctuary | Esfuerzo |
|---|---|---|---|---|
| **1** | **Lista de tareas con «Ir» y viaje automático.** Viaje por el camino real o teletransporte opcional; durante el viaje se ocultan los mandos y aparece «Parar»; la cámara sigue libre. | `src/ui/task-board.ts` (141 líneas), `src/modules/tasks/application/task-list.ts` (231), `src/modules/tasks/domain/journey.ts` (137, grafo de lugares con duración real), `src/ui/task-settings.ts`; docs `docs/design/viaje-a-una-tarea.md`, `reglas.md` §3 | Tareas = los pasos de `StoryMission` (`Assets/Scripts/Mission/StoryMission.cs`, kinds Talk/Reach/Collect/VisitAll/Hold) + `MissionTracker.cs` + los bloques de `MicroJornada`. «Ir» lleva el avatar con su `NavMeshAgent` (`CubeGait.cs:21`) y trepa con `SurfaceWalker`. El NavMesh ya resuelve el camino, así que no hace falta el grafo de `journey.ts`. | **M** |
| **2** | **Ventana de Áreas.** Esquema de nodos del lugar, «Aquí» donde está ella; tocar un nodo = ir allí. | `src/modules/tasks/application/areas.ts` (150: `placeNodes`, nodos ≥ 44 px), `src/ui/areas-window.ts`; doc `docs/design/areas.md` | Nodos = `AreaFunctionSite` y `AreaFunctionStation` de cada edificio de `CivicBuilder`, más los hitos (P5). Comparte el motor «Ir» del sistema 1. | **S** |
| **3** | **Menú árbol.** Un engranaje arriba a la derecha que se vuelve X; de él brotan Vista, Tareas, Áreas y lo que aplique ahora; cada icono que abre ventana se vuelve X; se recoge a los 5 s. | `src/ui/root-menu.ts` (198: `RootItem`, `RETRACT_MS = 5000`, `LONG_PRESS_MS = 450`), `src/ui/root-menu.css`, `src/ui/hud-windows.ts`; reglas `reglas.md` §5.1 y §5.8 | Nuevo `Assets/Scripts/UI/MicroRootMenu.cs` (IMGUI) que registra sus ventanas en `HudModals` y `HudGate`. Las acciones del personaje (cocinar, trabajar, dormir, hablar) salen de la máquina o el mueble 3D en contexto (CodeShip §5.1), no de teclas sueltas. | **S** |
| **4** | **Retos de código.** Ordenar bloques (problema de Parsons) o elegir opción (completar, predecir, reparar, especificar); bloques en pseudocódigo español con vista JS; intentos y dificultad; cada paso del reto animado en su sitio. | `src/modules/mind-duel/domain/exercise.ts` (`OrderExercise`, `ChoiceExercise`, `isCorrect`), `attempts.ts`, `difficulty.ts`, `src/modules/chores/domain/chore.ts` (catálogo de tareas → reto), `src/modules/station/application/challenges.ts` (retos con datos reales: masa, rendimiento), `src/modules/chores/application/step-visuals.ts`; reglas `reglas.md` §4 | Nuevo `StoryMission.Kind.Code` con un `Exercise` en C# (datos puros, fácil de probar en EditMode) y una hoja de reto IMGUI basada en `ChoiceDialog`. Al acertar se dispara la animación del paso en el puesto (`AreaFunctionStation`). Motor: **M**. Contenido: unos 5 retos por microcosmos con datos reales, **M por microcosmos**. | **M + M/microcosmos** |
| **5** | **HUD y orientación.** Una sola acción principal, siempre en el mismo sitio (columna de contexto); estado arriba a la izquierda; objetivo visible; toques ≥ 44 px; nada se solapa; fallos con forma y color; carta o encargo que abre cada misión. | `docs/design/ux-acciones.md` (§1 reglas, §3 composición), `src/ui/hud.ts`, `src/ui/mission-log.ts`, `docs/design/misiones.md` (cartas de Nevaeh → objetivo marcado) | `HistoryCard.cs` (intro) pasa a ser la «carta» con el primer objetivo; objetivo actual fijo en la columna de contexto; marcador en el mundo sobre el destino de la tarea; `WorldLabel.cs` para los rótulos. | **S–M** |
| **6** | **Reglas de escala y detalle.** `WORLD_SCALE = 1.5` (datos reales, presentación ampliada), 2 m entre interactuables medidos en datos, todo con fuente, cada equipo con su forma real y su animación propia. | `src/core/scale.ts` (`WORLD_SCALE`, `toWorld`, `drawnSpacing`), `docs/design/escala.md` (tabla de medidas 1×–2× en iPhone 13), `docs/design/reglas.md` §1, §2, §4.5 y la lista de revisión final | `MicroBuildRules` (`Presentation`, nuevos `Passage` y `Street`, quitar `WallHeight` y `RoofHeight` como prohibición), `MicroFurniture`, `KitchenProps` (factor E3), `CivicBuilder.Building` (muros y vista de corte), `MicroBuildRules.Validate` y sus pruebas. Reglas: **S**. Reconstrucción: **L** (todos los microcosmos). | **S + L** |

### 4.1 Plan por fases

| Fase | Qué | Esfuerzo | Criterio de hecho |
|---|---|---|---|
| **0. Reglas** | Aprobar las decisiones de la sección 5. Reescribir el manual (§7.2–7.4) y `microcosmos-reglas-edificacion.md`. Añadir las constantes `Presentation` y `BodyRef` en `MicroBuildRules`. Cambiar `Validate`, `GlassWallTest` y `MicroBuildRulesTest`, con la prueba que falla escrita primero. | S | Pruebas de reglas en verde con las reglas nuevas. |
| **1. Piloto: Microcosmos_ElonMusk** (Hawthorne, 28-09-2008) | Nave de SpaceX con muros y vista de corte, sala de Control de Misión con consolas y pantallas reales, Falcon 1 o su etapa en la nave como hito, coche o camión de la época. Muebles a E1–E3 y C1. Menú árbol, Tareas y Áreas con «Ir». 3–5 retos de código: secuencia de cuenta atrás (ordenar), comprobar valores de telemetría (predecir), reparar un fallo del vuelo 3 (reparar). Carta inicial. | L | Berón lo juega y dice «me siento dentro». Las pruebas de escala, necesidades y retos pasan en esa escena. |
| **2. Necesidades verificadas** | Prueba en vivo en el microcosmos: un vecino duerme, va al retrete, come y trabaja un día entero, con `Fallbacks = 0`. Los personajes de la historia, con `NeedsDriver`. | M | Prueba nueva en verde en el piloto. |
| **3. Segundo y tercer piloto** | `Microcosmos_Descartes` (Leiden 1637: canales, barcas, casas de ladrillo con frontón escalonado) y `Microcosmos_Atahualpa` (Cajamarca 1532: plaza, kallankas, literas). Prueban época antigua y transporte. | L | Igual que la fase 1. |
| **4. Despliegue** | El resto de microcosmos, **uno a uno** por el límite de memoria del PC. Retos con datos de cada lugar. | L (repetido) | Lista de revisión, como la de CodeShip, en cada uno. |

**Por qué Elon Musk de piloto:** es el lugar más documentado (planos, fotos y vídeos de 2008). Su contenido es el de CodeShip (cohetes, consolas, telemetría), así que los retos y la escala 1,5× de las consolas se portan casi directamente. Un interior con muros (fábrica y control) es el caso que más cambia respecto a «sin muros».

---

## 5. Decisiones abiertas para Berón

1. **¿Qué referencia de escala usamos?**
   - **(Recomendada) A:** el mirmidón se queda como está (torso de 1,75 m, cuerpo de 2,16 m) y el mundo se dimensiona por su largo (k_c ≈ 1,25) y se dibuja a 1,5×. Los objetos de mano quedan ≈ 1,9× más grandes.
   - **B:** encoger al mirmidón para que su *largo* mida 1,75 m (torso ≈ 1,42 m) y dibujar el mundo a 1,5×.

   A no toca cuerpos, animaciones ni pruebas de cuerpo.
2. **¿Cómo se sientan?**
   - **(Recomendada) De pie, con mesa alta y apoyo de abdomen** (C1). La silla de montar queda solo para tronos y vehículos.
   - Alternativa: sillas de montar más largas y altas en todas partes.
3. **¿Vuelven los muros y techos?**
   - **(Recomendada) Sí, en todos los microcosmos,** con vista de corte al entrar (P2), puertas de 1,8 × 2,4 m y escaleras solo por encima de 12 m.
   - Alternativa: solo fachadas hasta 1,2 m más el hito, sin interiores cerrados.
4. **¿Qué microcosmos es el piloto?**
   - **(Recomendado) Elon Musk / Hawthorne 2008.**
   - Alternativas: Descartes / Leiden (más «pueblo histórico» con canales y barcas) o Atahualpa / Cajamarca.
