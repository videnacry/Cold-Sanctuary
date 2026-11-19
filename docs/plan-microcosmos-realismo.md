# Plan: microcosmos con escala, edificios y jugabilidad de CodeShip

Encargo de Berón (2026-11-18): las sillas, mesas y platos parecen juguetes. Hay dudas sobre la escala de los árboles. Los microcosmos de Musk, Atahualpa y Descartes no recrean lugares reales: no hay edificios, transporte ni estructuras de época. Además, no se entiende qué hacer. Pide traer a los microcosmos la UI/UX de CodeShip: menú árbol, retos de código, listas de tareas y áreas con viaje automático, y estructuras fieles con objetos escalados.

**Revisión del 2026-11-18:** Berón aprobó las decisiones 2–4 y pidió encoger a los insectos-humanos. Se añaden la huella completa con las patas (§1.1 b) y la escala encogida (§3.1, §3.4), y se rehacen las fases (§4.1) y las decisiones (§5).

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

### 1.1 b Huella completa con las extremidades (revisión del 2026-11-18)

Berón respondió: «el tamaño depende también de las extremidades… y el ancho es importante». Tiene razón. `MicroBody.span` (1,02 m) supone las patas **recogidas**, pero de pie el cuerpo las lleva **abiertas**. Lo he medido con la geometría con que `CubeInsect.Build` y `BuildLeg` (`CubeInsect.cs:248-376`) construyen las patas:

- **Cadera:** a ±0,45 × ancho del tórax (0,12 L) del eje.
- **Inclinación de cada pata hacia delante o atrás (`restYaw`):**
  - par delantero, +30°;
  - par medio, 0°;
  - par trasero, −30°.
- **Ángulos de la pata:**
  - trocánter y fémur suben 40° (`Joint … -40f`);
  - la rodilla baja 100°, así que la tibia acaba 60° por debajo de la horizontal;
  - el tarso queda a −15°.
- **Alcance en planta de cada pata** = coxa 0,05 + trocánter 0,03·cos40 + fémur·cos40 + tibia·cos60 + tarso·cos15:
  - P1: 0,515 L = 1,05 m;
  - P2: 0,541 L = 1,10 m;
  - P3: 0,655 L = 1,33 m.
- **Brazos:** en el insecto-humano el **par delantero es de brazos** (`armPairs = 1`, `CubeInsect.cs:324-334`). Anda con **4 patas** (P2 y P3).
  - Cada brazo mide 0,68 L = **1,38 m** (un brazo humano, ~0,75 m).
  - Está plegado hacia delante (coxa a 168°, tibia a −70°).
- **Antenas acodadas:** 0,44 L = **0,89 m** cada una, hacia arriba y delante desde la cabeza (`CubeInsect.cs:151-166`).
- **Al andar:** la coxa oscila hasta ±25° (`amp = speed/1.5 · 25°`, `CubeInsect.cs:581`).

Medidas resultantes con L = 2,03 m (estatura de 1,75 m):

| Medida | De pie | Andando (barrido) | Persona |
|---|---|---|---|
| **Ancho de lado a lado** (puntas de los tarsos de P3) | **2,52 m** (P2 da 2,42 m) | **2,87 m** (P3 a −5°) | 0,50 m |
| Largo de delante a atrás (brazos plegados → punta del gáster o tarsos traseros) | ~1,8 m (+0,85 … −0,95 m) | ~2,0 m (los tarsos de P3 llegan a −1,13 m) | 0,30 m |
| **Área de la huella** (rectángulo envolvente) | **≈ 4,5 m²** | **≈ 5,7 m²** | 0,15 m² |
| Veces la de una persona | **×30** | **×38** | ×1 |
| Alto total con las antenas | ~2,05 m | | 1,75 m |

**Conclusión.** El problema de escala es sobre todo de **ancho**. Un mirmidón de pie ocupa como un coche pequeño de frente (2,5 m), 2,5 veces lo que suponen `MicroBody.span` y los muebles. Por eso:
- el paso de 2 m (`Passage`) no deja pasar ni a uno andando;
- una puerta real de 0,9 m no le sirve;
- un mueble de persona, a su lado, es de juguete.

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

### 3.1 Escala: encoger a los insectos-humanos (revisión del 2026-11-18)

La primera versión de este plan proponía agrandar el mundo (cuerpo de 2,16 m como referencia y presentación 1,5×). Berón reabrió la decisión: «quizás deberían pasar a ser la mitad de grandes de lo que son ahora para encajar mejor o incluso más peques». Encoger el cuerpo es **mucho más barato** que agrandar el mundo:
- **Encoger:** una constante, los mínimos de `MicroBody` y de los muebles, volver a calcular el NavMesh y unas pruebas.
- **Agrandar:** reconstruir cada objeto y cada edificio de todos los microcosmos.

**Factor s.** Escala uniforme del insecto-humano (estatura, L, patas, brazos y antenas). El mundo (arquitectura, calles, árboles, vehículos, animales de la época y objetos de mano) se queda a **tamaño real humano 1:1**. Visto desde el cuerpo, ese mundo es 1/s veces mayor: es la versión de la regla de CodeShip «objetos más grandes de lo normal», pero sin tocar el mundo.

**Postura recogida (propuesta, todos los factores).** Las patas del insecto-humano se abren hoy 2,5 m. Se propone un `FolkStance` en `CubeInsect.BuildLeg` solo para los vecinos: trocánter a −15° en lugar de −40°, y par trasero a −15° en lugar de −30°. Así las patas van más bajo el cuerpo y el alcance lateral baja a **≈ 0,7×**. Es una estimación; hay que medirlo tras el cambio.

Base, s = 1:
- estatura 1,75 m;
- de pie, 2,52 m de ancho × 1,8 m de largo;
- andando, 2,87 m × 2,0 m;
- manos a 0,96 m y vientre a 0,24 m;
- 2,16 m tumbado;
- alto con antenas, 2,05 m.

| | **s = 0,5** | **s = 0,4** | **s = 0,33** |
|---|---|---|---|
| Estatura del torso | 0,88 m (0,80–0,93) | 0,70 m | 0,58 m |
| Largo tumbado (cama) | 1,08 m | 0,86 m | 0,71 m |
| Ancho de pie / andando, patas abiertas | 1,26 / 1,44 m | 1,01 / 1,15 m | 0,83 / 0,95 m |
| Ancho de pie / andando, **postura recogida** | **0,88 / 1,00 m** | 0,71 / 0,80 m | 0,58 / 0,66 m |
| Huella de pie (abierta / recogida) | 1,13 / 0,79 m² (×7,5 / ×5,3 de una persona) | 0,73 / 0,51 m² (×4,8 / ×3,4) | 0,49 / 0,34 m² (×3,3 / ×2,3) |
| Manos (altura de su mesa) | 0,48 m (mesa de 0,42 m) | 0,38 m (0,34 m) | 0,32 m (0,28 m) |
| Apoyo de abdomen (C1) | 0,16 m | 0,13 m | 0,11 m |
| Puerta mínima (ancho andando recogido + 0,2 × alto con antenas + 0,2) | **1,2 × 1,25 m** | 1,0 × 1,0 m | 0,86 × 0,9 m |
| Pasillo de uno / de dos sentidos (recogido) | 1,4 / **2,4 m** | 1,2 / 2,0 m | 1,06 / 1,7 m |
| Radio del agente NavMesh (medio ancho recogido) | 0,50 m | 0,40 m (el de hoy) | 0,33 m |
| Velocidad del agente (hoy 3 m/s; la amplitud del paso va con `speed/1.5`) | 1,5 m/s | 1,2 m/s | 1,0 m/s |
| Plato real de 0,26 m visto desde su cuerpo | **×2** (lo que pedía la v1: ×1,9) | ×2,5 | ×3 |
| Una sala real de 3 m de techo, para su cuerpo | Como 6 m | Como 7,5 m | Como 9 m |
| ¿Caben por puertas reales (0,8–1,2 m) sin tocar el plano? | Solo las principales y dobles (≥ 1,2 m); las interiores se ensanchan a 1,2 m o se entra trepando | Las de ≥ 1,0 m | Todas las de ≥ 0,9 m |
| Legible en el móvil y en el PC del HD 620 | Bien | Justo (manos de cubos de < 1 cm) | Pequeño: retrato y etiquetas se pierden |

**Recomendación: s = 0,5 con la postura recogida.**
- Es lo que Berón pidió primero («la mitad»).
- El mundo real a 1:1 se ve exactamente 2× mayor: platos, tazas, mesas reales, edificios y árboles reales. Era el objetivo visual de la primera versión (×1,9), ahora **sin agrandar ni un objeto**.
- CodeShip midió que a 2× la sala ya es grande y que más allá «el avatar en tercera persona se ve pequeño» (`escala.md`). Por eso 0,4 y 0,33 harían cavernosas las salas reales y el personaje ilegible en el móvil.
- Con postura recogida mide 1 m de ancho andando. Caben dos de frente en 2,4 m.

Si en el piloto las puertas interiores reales molestan, el siguiente paso es 0,4, que cabe por puertas de 1 m y deja el radio de agente de hoy (0,4 m).

**Reglas de escala con s = 0,5 (sustituyen a E1–E4 de la versión 1):**
- **E1. `MicroBuildRules.FolkScale = 0.5f`, una sola constante.**
  - Estatura de los vecinos: 1,6–1,85 m × s, en `CivicBuilder.cs:710` y las ~10 llamadas a `Folk(…)` de los builders.
  - El avatar del jugador insecto, igual.
  - Las demás especies de insecto-humano: lo mismo, por su `MicroBody`.
- **E2. El mundo, a escala real 1:1:** arquitectura, calles, árboles, vehículos, animales de la época y objetos de mano (plato de 0,26 m, taza, herramientas). Se descarta la constante de presentación 1,5× de la v1. Los mirmidones salvajes y las recolectoras de las eras sin pueblo, que se miden respecto al insecto-humano, también × s.
- **E3. Lo que toca el cuerpo, a la medida del cuerpo, por `MicroBody`:** mesa de 0,42 m, apoyo de abdomen, lecho de 1,4 × 0,8 m, retrete y lavamanos. Ya se calcula midiendo el cuerpo, así que se adapta solo, salvo los **mínimos fijos**, que hay que pasar a × s.
- **E4. Pasos.**
  - `AgentRadius` 0,4 → 0,5 m.
  - `Passage` 2,0 → **2,4 m** (dos cuerpos recogidos andando + 0,4).
  - `Street` se queda en 4 m: caben dos parejas o un carro real.
  - `WorkSpot` 0,9 → 0,6 m.
- **E5. El santuario ×100, oculto desde el pueblo:** niebla u horizonte. Sigue igual que en la v1.
- **E6. Árboles reales (15 m) con copa de varias piezas.** Sigue igual que en la v1. Para el cuerpo encogido un tilo «mide» 30 m, que es justo lo que se espera de un insecto.
- **E7. Puestos reales de altura humana** (consolas de Control de Misión, mostradores, altares): se quedan a su medida real por fidelidad. El insecto-humano trabaja en ellos sobre una **tarima de puesto** (alto = superficie real − manos + 0,1 ≈ 0,37 m) o **subido encima**, porque trepa.

### 3.2 Asientos, camas y verticalidad (reglas C1–C4, aprobadas por Berón)

- **C1. Comer y trabajar de pie.** Una hormiga descansa de pie sobre sus patas. El mueble por defecto es el **mostrador o la mesa alta** a la altura de sus manos (0,42 m con s = 0,5), con un **apoyo de abdomen**: una barra o cojín inclinado bajo el gáster, como una misericordia de coro, de 1,5 × el ancho del cuerpo y visible detrás de cada comensal.
- **C2. La silla de montar solo donde la época lo pide** (trono, tribuna, carruaje): al largo del abdomen + 0,15 m y al vientre + 0,08 m (× s).
- **C3. La cama sigue siendo un lecho de vientre** al largo del cuerpo + 0,3 m × s.
- **C4. Sin escaleras ni ascensores para moverse.** Trepan las paredes (`SurfaceWalker`, enlaces `NavMeshLink` de M8).
  - **Excepción:** con más de **12 m** de altura (rascacielos, torres) hay ascensor o montacargas, si el lugar real lo tenía.
  - Las escaleras reales de un lugar (escalinatas, pirámides) se construyen tal cual, por fidelidad, como **monumento**, no como camino obligado.

### 3.3 Muros y edificios: vuelven, con vista de corte (reglas P1–P7, aprobadas por Berón)

- **P1. Cada microcosmos recrea su lugar real a 1:1** (E2): planta, alzados y materiales documentados, con fuente y etiqueta (real / supuesto / ficción), como CodeShip `reglas.md` §1.1. Se termina la regla 2 («sin muros ni techo») en los microcosmos.
- **P2. Muros y techos visibles desde fuera. Por dentro, vista de corte:** al entrar o al acercar la cámara, el techo y los muros entre la cámara y el jugador se desvanecen.
- **P3. Todo recinto que existe en la realidad se puede recorrer.** Una puerta solo da paso, nunca abre menús (CodeShip §2.1 y §2.2).
- **P4. Puertas.**
  - Las reales, a su medida.
  - Las de menos de **1,2 m** de ancho (el cuerpo recogido andando + 0,2, con s = 0,5) se ensanchan a 1,2 m. Si no, el NavMesh las cruza con un enlace de trepar por la fachada o la ventana.
  - El alto real siempre basta: el cuerpo mide 1,25 m con antenas.
- **P5. Un hito reconocible por microcosmos,** visible desde la llegada: el Falcon 1 y la nave de SpaceX en Hawthorne; la kallanka y la plaza triangular de Cajamarca; el Rapenburg y la universidad de Leiden.
- **P6. Al menos un transporte de época que se use** (se sube, se mueve, lleva carga), a tamaño real: para el cuerpo encogido, un carro real es un vehículo grande y creíble. Se integra con el viaje automático (sección 4) como trayecto.
- **P7. Densidad de época:** rótulos, objetos del oficio y gente en su puesto. Lo que hoy es un «solar abierto con muebles» pasa a ser un interior.

### 3.4 Qué se rompe al encoger (s = 0,5) y hay que ajustar

| Pieza | Dónde | Qué pasa |
|---|---|---|
| Estatura de los vecinos | `CivicBuilder.cs:710` y ~10 llamadas a `AreaMicrocosmos.Folk(…)` | Pasan a × `FolkScale`. |
| Cuerpo del NavMesh | `AreaMicrocosmos.Folk`, `AreaMicrocosmos.cs:1306-1309` y :1320-1321 | Cápsula de radio 0,35, agente de radio 0,4, `speed = 3` y altura de etiqueta: pasan a × s. **Si la velocidad no baja, la amplitud del paso (`speed/1.5`) se satura y las patas patinan.** Hay que volver a calcular el NavMesh de cada microcosmos. |
| Mínimos de `MicroBody` | `MicroBody.ForSpec`, :106-115 | `length` ≥ 1,2 m (con s = 0,5 el cuerpo mide 1,08 m y **se recortaría a 1,2**); `width` ≥ 0,3; `abdomen` ≥ 0,25. `Human()` usa 1,75 por defecto. Los mínimos pasan a × s. |
| Mínimos de los muebles | `MicroFurniture.SeatOn` (:449, y ≥ 0,15, len ≥ 0,4, wid ≥ 0,3), `InsectBenchStep` (+0,35), `TableWithSeats` (:587: **mesa recortada a 0,6–1,0 m**, se quedaría en 0,6 con manos a 0,48), `TD` 0,9, sitio a 0,25 m | Pasan a × s. |
| Objetos de mano | `MicroCivicScale.PlateFor` (`MicroSeat.cs:34`) | Ya tiene mínimo de 0,26 m: el plato se queda real. Revisar que nada multiplique por la estatura. |
| Estatuas y fuentes | `MicroCivicScale.StatueTimes` y `FountainTimes` × estatura | Se calculan desde `Citizen` (1,75 m humano), no desde el cuerpo encogido, para que sigan siendo monumentos. |
| Necesidades y jornada | `HomeNeedSpell` y `MicroSeat` (puntos fijos), `MicroJornada.WorkSpotOf`, umbrales de llegada | Las distancias absolutas pasan a × s. |
| Cámara y etiquetas | Distancias de seguimiento de la cámara del avatar insecto, `WorldLabel.height` | × s. |
| Auditoría de escala | `ScaleAudit.cs` («en los microcosmos, la referencia es la hormiga = un humano») | La referencia pasa a «humano × s». |
| **Pruebas que fallarán** | `InsectOnlyTest.cs:71` (el avatar insecto mide 1,75 ± 0,25 m) | Esperar 1,75 × s. |
| | `ActividadesTest.cs:53` (`standLocal` a 1,6 m) y sus distancias de bandeja y asiento | Revisar las medidas. |
| | `MicroBuildRulesTest` (muros sintéticos de 3,2 m, que con P1 dejan de ser falta) | Reescribir con las reglas nuevas. |
| | `GlassWallTest` y `MicrocosmosFloorTest` si miden pasos | Ídem. |
| | `CicloHumanoTest` | Usa su propia escena; solo si usa `Folk`. |
| Animaciones | Andar (`CubeInsect`): en unidades de L, no cambian; solo la velocidad. Brazos, empujar, bandeja y sentarse: posiciones relativas al cuerpo, salvo las fijas de las acciones (`HomeNeedSpell`, `MicroSeat`). Postura recogida: nuevo ángulo de trocánter y `restYaw` solo para los vecinos (`_dressingFolk`). | Animaciones de pasos: **S**. Postura recogida: **S**, y medirla. |

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
| **6** | **Reglas de escala y detalle.** `WORLD_SCALE = 1.5` (datos reales, presentación ampliada), 2 m entre interactuables medidos en datos, todo con fuente, cada equipo con su forma real y su animación propia. | `src/core/scale.ts` (`WORLD_SCALE`, `toWorld`, `drawnSpacing`), `docs/design/escala.md` (tabla 1×–2×: a 2× «el avatar en tercera persona se ve pequeño»), `docs/design/reglas.md` §1, §2, §4.5 y la lista de revisión final | Con s = 0,5 **no hace falta constante de presentación**: el mundo real a 1:1 ya se ve 2× más grande que el cuerpo. Se porta la idea (una sola constante, datos reales) como `MicroBuildRules.FolkScale = 0.5f`. Se quitan `WallHeight` y `RoofHeight` como prohibición y se añaden la vista de corte en `CivicBuilder.Building` y la lista de revisión. Reglas: **S**. Muros e interiores: **L** (por microcosmos). | **S + L** |

### 4.1 Plan por fases (revisado tras encoger)

| Fase | Qué | Esfuerzo | Criterio de hecho |
|---|---|---|---|
| **0. Reglas** | Reescribir el manual (§7.2–7.4) y `microcosmos-reglas-edificacion.md` con E1–E7, C1–C4 y P1–P7. Cambiar `Validate`, `GlassWallTest` y `MicroBuildRulesTest`, con la prueba que falla escrita primero. | S | Pruebas de reglas en verde con las reglas nuevas. |
| **0b. Encoger el cuerpo** (nueva) | `FolkScale = 0.5` y postura recogida. Mínimos de `MicroBody` y `MicroFurniture` × s. Agente, velocidad, cámara y etiquetas × s. `AgentRadius`, `Passage` y `WorkSpot` nuevos. Ajustar las pruebas de la tabla 3.4. Recalcular el NavMesh **solo del piloto**. Prueba nueva: ancho del vecino recogido andando ≤ 1,05 m, cabe por una puerta de 1,2 m y la mesa queda a las manos ± 0,05 m. | **M** | Las pruebas de cuerpo y de muebles pasan. Una captura del piloto junto a un plato y una mesa reales. |
| **1. Piloto: Microcosmos_ElonMusk** (Hawthorne, 28-09-2008) | Muros y vista de corte. Lo que cambia con el cuerpo encogido: la nave, el Control de Misión, las consolas, el Falcon 1 y el coche o camión se construyen **a su medida real**, sin factor de presentación, y las consolas llevan tarima de puesto (E7). Menú árbol, Tareas y Áreas con «Ir», 3–5 retos de código y carta inicial, como en la v1. | L (algo menos que en la v1: no se agranda ningún objeto) | Berón lo juega y dice «me siento dentro». Las pruebas de escala, necesidades y retos pasan en esa escena. |
| **2. Necesidades verificadas** | Prueba en vivo en el piloto: un vecino duerme, va al retrete, come y trabaja un día entero, con `Fallbacks = 0`. Los personajes de la historia, con `NeedsDriver`. | M | Prueba nueva en verde. |
| **3. Segundo y tercer piloto** | Descartes (Leiden 1637) y Atahualpa (Cajamarca 1532), a 1:1 real. | L | Igual que la fase 1. |
| **4. Despliegue** | El resto, **uno a uno** por el límite de memoria: recalcular el NavMesh con el agente nuevo y repasar muebles, puertas, muros e interiores. Encoger cuesta poco por microcosmos (el builder ya mide el cuerpo); lo caro son los muros y los retos. | M por microcosmos (antes L) | Lista de revisión en cada uno. |

**Por qué Elon Musk de piloto** (aprobado): es el lugar más documentado de 2008, su contenido es el de CodeShip (cohetes, consolas, telemetría) y un interior con muros es el caso que más cambia.

---

## 5. Decisiones

Aprobadas por Berón (2026-11-18): **2** (de pie con apoyo de abdomen), **3** (vuelven muros y techos, con vista de corte) y **4** (piloto Elon Musk). Quedan abiertas:

1. **Factor de encogimiento.**
   - **(Recomendado) s = 0,5 con postura recogida:** 0,88 m de torso, 1 m de ancho andando y el mundo real 2× mayor.
   - Alternativas: s = 0,4 (cabe por puertas de 1 m, salas que se ven 2,5×) o s = 0,33 (cabe por cualquier puerta, pero las salas se ven 3× y el cuerpo apenas se lee en el móvil).
2. **Postura recogida de las patas.**
   - **(Recomendada) Sí:** el ancho baja ~30 %. Sin ella, s = 0,5 abre las patas 1,44 m al andar y no cabe por casi ninguna puerta real.
   - Alternativa: patas abiertas como las hormigas reales y puertas ensanchadas a 1,65 m.
3. **Puertas interiores reales de menos de 1,2 m.**
   - **(Recomendado) Ensancharlas a 1,2 m** y anotarlo como «supuesto» en la ficha del lugar.
   - Alternativa: se dejan reales y se entra trepando por la fachada o la ventana.
