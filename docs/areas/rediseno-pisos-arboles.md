---
resumen: "PB-170, plan W2 (2026-11-25): portar de CodeShip el rediseño de pisos y edificios-árbol (pisos de 40 × 40 m, un piso por actividad y por afinidad, pisos solo del tronco con tornapuntas y rejilla de madera, árboles en un anillo exterior sobre bases negras con un mar central, globo de agua con el pulpo, cima con castillos y toboganes, reorganización por cálculo), con su reajuste de jornada, reparto y horario libre (W9). D1 decidido por Berón (2026-11-25): uno por actividad, por plazas; D2–D6 según la recomendación («Sigue», 2026-11-25). F0 hecha (reglas, marcas, reparto por plazas; sin Unity). F1a hecha (losas de 40 m, avisos de pisos a medias, un piso por centro y ocio por plazas; sin Unity); F1b (las fusiones de pisos) pide el Blockout."
temas: [construccion, jornada, codeship, agua, reglas]
estado: vigente
codigo: [FloorOrganization, FurnitureSpacing, LeisureDirector, HelixTower, SampleSceneBuilder, TrainingCentres, AreaCapacity, WorkRoster, LeisureDirector, LeisureRoster, SanctuaryArea, StructuralLimits, CarpetPad, PolarLandscapeBuilder, Bedroom, PlatformSlots, AreaMicrocosmos, VirtualizationTank]
retos: []
---
# Plan: rediseño de pisos y edificios-árbol (portado del santuario 1 de CodeShip)

Pedido de Berón (2026-11-24, PB-170): «en CodeShip se han realizado importantes cambios sobre su sanctuary1, quizás se
podrían aplicar aquí». Eligió portar primero el **rediseño de pisos y árboles**. Por la regla **W2** esto es solo el
**plan**: investigación, choques con lo que hay y fases. No se construye ni se abre Unity hasta que Berón lo apruebe.

**Fuente:** `Catravcul/CodeShip`, rama `integracion` (`63a5b86`, 2026-11-24): `docs/design/santuario1.md` (listas 8.ª a
16.ª, pedidos #64–#134), `docs/design/reglas.md` §2.7, 7b, 7c, 2.9, 2.10 y 2.13, `docs/design/organizacion.md` y
`src/modules/sanctuary-one/data/site.ts` (`TOWERS_PREVIOS`, `PISO_TAGS`). Resumen del porte: [portar-codeship-s1.md](../sistemas/portar-codeship-s1.md).

## 1. Qué hizo CodeShip (lo que se porta)

| Pedidos | Cambio | Regla de CodeShip |
|---|---|---|
| #64, #65, #82, #98, #102 | **Edificios para los miembros actuales**: un piso por actividad (un gimnasio, una sala de yoga, un aula…) y **pisos por afinidad** (las salas pequeñas se juntan: calistenia + parkour + crossfit, yoga + música + ballet + bailes, combate, química + biología, mecánica + garaje, textil + huerto vertical, asambleas + guardia, cría + carne cultivada). Los edificios amplios de antes **se guardan** (`TOWERS_AMPLIOS`) para cuando haya gente. 63 → 33 → 23 pisos | 7b |
| #82, #92, #104, #105 | **Todos los pisos de 40 × 40 m** (el de la cocina), ninguno medio vacío (la mitad del piso con algo a menos de 3 m) y sin grandes huecos (ningún punto a más de 6 m de algo, salvo 2,5 m de borde para tirarse); lo pequeño se separa hasta ×1,8 | 2.9, 2.10 |
| #66–#70, #76 | **Los pisos solo nacen del tronco** (2 por nivel, lados opuestos, en molinete), **sin patas**: los sostienen **dos tornapuntas** diagonales al tronco. **Las ramas solo van a las puntas de frutales**, con el tubo de Cold Sanctuary. **Armazón de madera**: postes en las esquinas, viga por nivel y **rejilla cada 4 m** (ajustable 4/8/16 m), que nunca cruza una puerta. Aros delgados y suelos de madera marrón con textura de tablas | 2.7 |
| #71–#74, #78, #83, #88 | **Mapa en anillos**: en el centro un **mar circular** sin hielo (ballenas, orcas, focas, delfines); alrededor, casquetes e isla; **en el borde, los árboles**, cada uno sobre su **base negra** cuadrada en el mar (casco de 3 m, cubierta a la altura del hielo, 10 m para andar) con su estación de alfombras (muelle si la copa la tapa). Ningún piso ni punta sobre un casquete: el hielo es de los animales | 7c |
| #75, #79, #86, #87, #99, #107, #108 | **Globo de agua**: sobre una boquilla que sale del mar, una esfera de agua de ~140 m de radio (más alta que cualquier árbol), hecha de capas de celdas de 8 m. Se nada dentro; la luz baja con la profundidad. Ballenas arriba, delfines abajo, focas y delfines suben y bajan por la boquilla. Las alfombras vuelan por encima | — |
| #119, #123 | **El pulpo que se desarrolla** dentro de su huevo transparente en el centro del globo: estadios de Naef I–XX de *Octopus vulgaris* (Naef 1928), dos inversiones, ojos que se pigmentan, cromatóforos, vitelo que se gasta, eclosión y paralarva; luego otro huevo. Ajuste de duración (1, 3, 10 min o 2 h). Paralarva = 1/3 del globo | — |
| #84 | Alfombras al **doble de velocidad** (12 m/s) acelerando y frenando como un coche (2,5 m/s²) | — |
| #129–#132 | **Cima gigante** (110 × 110 m) en la copa: la Residencia con **4 castillos** (30 aposentos con sus literas), **toboganes reales** (gravedad con rozamiento μ = 0,25, EN 1176-3) al **ring de almohadas**, aula, lectura, cocina compartida, aseos y **jardín en el borde**; entrada por el centro (la copa del tronco) y la estación de alfombras como apéndice del borde. Guerra de almohadas con tesoros | 2.13 |
| #124, #133 | Una sola **gran pecera de simulación** (9 × 9 m, 9 máscaras), en el piso de Informática | — |
| #134 | **Reorganización por cálculo**: cada piso lleva marcas (componentes con categoría y **cuota**, 1 = un piso; puro o mixto) y unas reglas deciden: cuota ≤ 1 piso · ≤ 1,9 piso con anillo · ≤ 9 cima · ≤ 11,25 cima con anillo · más, se reparte; un mixto desbordado manda fuera lo que no crece; un árbol que se queda solo con su cima la cede al de menos pisos y desaparece. Nunca se reorganiza a mano | 2.13 |

**No se porta** (ya decidido en [portar-codeship-s1.md](../sistemas/portar-codeship-s1.md)): la «distribución nuestra» (puesto
fijo por turno, choca con la regla de oro) y los 4 animales por persona (aquí el multiplicador es 3).

## 2. Cómo está hoy Cold Sanctuary

Todos los edificios ya son edificios-árbol (`HelixTower`, [edificio-arbol.md](edificio-arbol.md)): tronco de agua de
20 × 20 m, pisos pegados a sus caras en molinete (2 por nivel, cada 22 m, primero a 64 m), puntas con frutales al final de
ramas de agua de 10 × 10 m, anillos en U 8 m sobre algunos pisos, techo con alfombras y paneles. Es decir, **la mitad de la
regla 2.7 ya la tenemos** (pisos del tronco, molinete, tornapuntas CFST, exoesqueleto de nervios).

| Árbol | Pisos hoy (30 miembros) | Medida de piso |
|---|---|---|
| Servicios | Restaurante · Hospital: urgencias · hospitalización · veterinario · Cría · Carne cultivada · Asambleas · Guardia (8) | 40 × 38 |
| Entrenamiento | Gimnasio · Cardio · Calistenia · Hatha ×2 · Ashtanga ×2 · Ballet ×2 · Dojo ×2 · Aula ×N · Música ×N (~15, techo ~240 m) | 36 × 26 |
| Ciencias | Química ×2 · Biología ×2 · Informática · fachada de fotobiorreactores (la física se fue al Santuario Marino) | 32 × 20 |
| Producción | Construcción · Mecánica · Textil · Huerto vertical ×N · Garaje del submarino | 26 × 20 |
| Árbol de los Campos | 4 pisos y 4 puntas de cultivos (gira) | 32 / 16 |
| Residencia (Isla:Cabo) | Plaza con el ring · 5 pisos de habitaciones (30 camas-litera con tobogán) | 40 × 40 |

Los árboles están **sobre lóbulos de hielo** (`PolarLandscapeBuilder.Islands`), entre los casquetes y la laguna.

## 3. Choques con las reglas vigentes (hay que decidirlos antes de construir)

| # | Choque | Regla aquí | Propuesta |
|---|---|---|---|
| C1 | **Un piso por actividad** frente a «los centros de entrenamiento y estudio sirven a TODOS los miembros a la vez» y «las salas no se encogen» | **S13 = J16**, **J36** (`AreaCapacity`, `TrainingCentres.FloorsFor`) | Reescribir J16: **la suma** de plazas de los centros de entrenamiento abiertos ≥ miembros (no cada centro); el entrenamiento de las 8 y las 14 reparte a la gente **por plazas** entre los centros, como ya hace el horario libre. Lo de «todos a la vez» (J36: fiestas con los de todos los santuarios) se queda en dos sitios que sí miden para todos: la **cima** (ring y plaza) y el piso de **Asambleas** |
| C2 | Pisos de 40 × 40 para todos frente a «del lado mayor de su área más grande» | edificio-arbol §Migración | Adoptar 40 × 40 en todos los árboles salvo el de los Campos (sus bancales ya están medidos); las salas pequeñas se separan hasta ×1,8 con `FurnitureSpacing` |
| C3 | Armazón de **madera** frente al exoesqueleto de **acero CFST** que carga el tronco (73 % del acrílico) | T1, `StructuralLimits.RootCheck` | No se quita acero: la madera es **revestimiento** de los nervios y zunchos que ya hay (mismos ejes) y la rejilla cada 4 m se alinea con ellos. Así no cambian las cuentas ni se cruza una puerta (ya probado por `TreeBuildingsTest`, grupo 57) |
| C4 | Árboles en el **mar** sobre bases negras frente a troncos en hielo firme | edificio-arbol (sitios) | Base flotante como casco (ficción marcada, como en CodeShip); el pie del tronco baja al agua. El radio del anillo se calcula con la fórmula de las islas (sin Unity), a mitad de camino entre el borde de los casquetes y el del mapa, con ≥ 16 m en 3D entre árboles |
| C5 | Un **globo con toda la vida marina** en el santuario 1 frente al **Santuario Marino** (s2), que es el del mar profundo | progresion-santuarios, memoria del proyecto | En el globo solo las especies de mar somero del s1 que ya hay (ballenas, orcas, focas, morsas); el delfín mular de CodeShip no vive en el polo: no se añade. Del pulpo, la ciencia es la de *O. vulgaris* (la mejor estudiada); se marca que no es polar, o se cambia por un pulpo antártico (*Pareledone*) con los mismos estadios si Berón lo prefiere |
| C6 | La **Residencia desaparece** y su cima pasa a Ciencias | residencia.md, pijamadas, `Bedroom` | Las 30 camas-litera pasan a los **aposentos** de los castillos (mismo `Bedroom`, 4 plazas, tobogán propio sustituido por los dos del castillo). Pijamadas y «descansar en casa» siguen a `Bedroom`, no al árbol |
| C7 | **Niebla** `fogDensity = 0,008`: a 300 m solo se ve el 9 % | plano-edificios §Riesgos | Bajarla o usar niebla por distancia lineal: si no, los árboles del anillo exterior desaparecen desde el centro |
| C8 | Menos pisos = menos **plazas del microcosmos** (`PlatformSlots`: 4 historias, 8 personajes, 12 fauna y 60 flora por piso) | microcosmos | Las plazas pasan a ir **por componente** del piso (cuota × plazas), no por piso; así no se pierden historias |

## 4. Los árboles propuestos (30 miembros)

Partiendo de `TOWERS_PREVIOS` de CodeShip, adaptado a lo que aquí existe:

| Árbol | Pisos (de abajo arriba) | Antes → ahora |
|---|---|---|
| **Servicios** | Restaurante (anillo del comedor) · **Hospital** (urgencias, consultas, paritorios, 2 quirófanos, UCI y aislamiento; camas en su anillo) · Hospital veterinario (anillo de grandes animales) · **Cría y carne cultivada** · **Asambleas y guardia** | 8 → 5 |
| **Entrenamiento** | **Gimnasio** (placas, peso libre, abdominales, spinning y cardio en todo el piso; 2–3 de cada máquina) · **Calistenia, parkour y crossfit** · **Combate** (judo, karate, kickboxing, MMA) · **Salas** (yoga, música, ballet y bailes sociales, una por cuarto) | ~15 → 4 |
| **Ciencias** + la **cima** | **Laboratorios** (química y biología) · **Informática** con la gran pecera · fachada de fotobiorreactores · **cima de la Residencia** (castillos, ring, aula, lectura, cocina compartida, jardín) | 5 → 2 + cima |
| **Producción** | Construcción · **Mecánica y garaje** · **Textil y huerto vertical** | 5–6 → 3 |
| **Árbol de los Campos** | Sin cambios (sus cultivos) | igual |
| Residencia | Desaparece: sube a la cima de Ciencias por la regla 3 de la reorganización | 6 → 0 |

Las **marcas** (cuotas) van en datos, como `PISO_TAGS`: p. ej. Asambleas 0,75 + Guardia 0,25; Cría 0,6 + Carne 0,4;
Informática 0,8 + Pecera 0,15; Residencia = 110² / 40² ≈ 7,6 → cima. Los edificios de hoy se guardan como «amplios»
(una constante o una rama `edificios-amplios`), para cuando haya más gente.

**Lo que hay que comprobar al fusionar:** el hospital v2 pedía ≥ 2 médicos (mejor 4) a la vez en cada tarea y camas por
servicio ([hospital-plan.md](hospital-plan.md)); se cuenta que el piso único y su anillo los admiten antes de fusionar. Las
salas de música aisladas (`SoundRoom`) caben en un cuarto de «Salas» solo con su caja de cristal; el auditorio de 60
butacas no: va a la cima o a Asambleas (decisión D4).

## 5. Reajuste de jornada, reparto y horario libre (W9)

El trabajo sale de las **horas de tarea de cada área** (`AreaFunctions`), no de los pisos, y se divide por igual entre los
humanos disponibles. Ninguna área desaparece, así que **las horas no cambian**; cambia dónde se hacen y cuánto se viaja.

| Qué | Cambio | Código |
|---|---|---|
| **Puesto = edificio entero** | Con los pisos mixtos, cada área sigue siendo una (`WorkRoster.AreaGroup`); el Aula pasa a Ciencias (la cima), la Residencia deja de ser un edificio. `SanctuaryArea.InBuilding` y `InfirmaryGate` se recalculan con los pisos nuevos (el hospital en un piso facilita el `Contains`) | `SanctuaryArea`, `InfirmaryGate` |
| **Entrenamiento (8–9 y 14–15)** | Hoy rota `(i + d) mod n` porque cada centro cabe entero (J16). Con un piso por actividad, el reparto respeta **plazas** (gimnasio 30, funcional 40, combate 25, salas ≈ 4 × 8): es la misma lógica que `LeisureRoster` (menos veces, más días sin hacerlo). El gimnasio sigue yendo por el plan de cada uno (`GymProgramming`) | `WorkRoster`, `TrainingCentres`, `AreaCapacity` |
| **Viajes** | Árboles más lejos (anillo exterior) pero alfombras al doble y con aceleración; se recalcula el tiempo de viaje que la jornada descuenta y los 90 s «de camino» de la Magnate (en vuelo ya no trae a nadie) | `CarpetPad`, `MagnateWarden` |
| **Misiones con `also`** | Cosecha → Cocina, carne → Cocina y Cría (ahora en el mismo piso), Tc-99m → Hospital: solo cambian rutas | `Mission.also` |
| **Ronda de frutales** | Las terrazas de la Residencia pasan al **jardín del borde de la cima** (una ruta andando en vez de vuelos entre habitaciones: más corta) | ronda de frutales |
| **Iniciación** | Sin cambios de orden (enfermería → tanque → yoga); la iniciación de yoga va a la sala de yoga de «Salas» | iniciaciones |
| **Clones de la Magnate** | Siguen llenando huecos; si un piso mixto deja un área sin nadie, su clon va a ese cuarto | `WorkRoster` |

**Horario libre** (`LeisureDirector.SanctuaryActivities`): los sitios y plazas cambian así.

| Actividad | Hoy | Después |
|---|---|---|
| Pesas y saco | Gimnasio | Gimnasio |
| Hatha libre / Ashtanga autoguiado | Pisos de Hatha y Ashtanga (8 + 8) | Cuarto de yoga de «Salas» (a sus horas, como en CodeShip) |
| Bailar | Ballet → Calistenia → Gimnasio | **Bailes sociales** (nueva sala) → Ballet |
| Artes marciales | Dojo → Gimnasio | Combate |
| Leer | Plaza de la Residencia | Lectura de la cima |
| Jugar con las crías | Cría | Cría y carne cultivada |
| Manualidades | Estudio Textil | Textil y huerto |
| Tertulia | Restaurante | Restaurante |
| **Guerra de almohadas** (sábado 20 h, todos) | Ring de la plaza (25 plazas: **no llegan a 30**) | Ring de la cima, medido para todos los miembros de todos los santuarios (J36) |
| **Pijamada** (viernes 21 h) | Cama-litera del anfitrión | Aposento del anfitrión en su castillo |
| Descansar en casa | Su habitación | Su aposento |
| **Nuevas** | — | Bajar por los **toboganes**; guerra de almohadas con **tesoros** por castillos |

La **regla de oro** no cambia: los pisos por afinidad juntan actividades, no personas; todo va por rotación.

## 6. Fases (cada una, su PR; en cada una: código, CODEMAP, docs, prueba en escena)

| Fase | Qué | Unity |
|---|---|---|
| **F0 · Reglas y datos** | Reescribir J16/S13/J36 y T1 en el manual; marcas de pisos (componentes, cuota, puro/mixto) y la **reorganización por cálculo** como clase pura (`FloorOrganization`, sin escena) con su prueba; capacidad del entrenamiento por plazas en `WorkRoster`; guardar los edificios de hoy como «amplios» | No (compila en el PC de Berón) |
| **F1 · Pisos 40 × 40 y fusiones** | `BeginTreeTower` con piso fijo de 40 × 40; pisos mixtos por cuartos o mitades; reglas de «no medio vacío» y «ningún punto a más de 6 m» como avisos del constructor (como `FurnitureSpacing`); borde libre de 2,5 m para tirarse | Sí: Blockout solo del santuario (W5) |
| **F2 · Estructura** | Sin patas (si quedan), dos tornapuntas por piso, ramas solo a las puntas, revestimiento de madera y rejilla cada 4 m sobre el exoesqueleto, aros delgados, suelos de tablas; `StructuralLimits` revisa cada piso de 40 m | Sí |
| **F3 · Mapa en anillos** | Mar central sin hielo, casquetes movidos con sus nidos y colonias (`Islands`, `PolarHabitats`), árboles en el anillo exterior sobre bases negras con su estación de alfombras; niebla; alfombras ×2 con aceleración | Sí (NavMesh nuevo) |
| **F4 · La cima** | Cima de 110 × 110 sobre Ciencias con tornapuntas, castillos, aposentos (`Bedroom`), toboganes con física real, ring y guerra con tesoros; la Residencia desaparece; pijamadas y lectura reubicadas | Sí |
| **F5 · Globo y pulpo** | Boquilla y globo de capas de agua (`WaterZone` por capa, sin un sólido por celda), luz por profundidad, fauna marina del s1 repartida; el huevo con los estadios de Naef y su ajuste de duración | Sí; **medir rendimiento** antes (W5, W13) |
| **F6 · Prueba en vivo** | Un único probador (W3): grupos 53, 57, 65 (horario libre), 75 (capacidad), 113 (reparto), jornada de un día completo | Sí, con permiso |

**Rendimiento** (el PC tiene 16 GB): F1 y F4 bajan objetos (de ~40 pisos a ~15 y una cima); F5 es el riesgo: un globo de
240 m es mucho agua transparente. Se construye por capas, con LOD y sin colisionadores por celda; si pasa del presupuesto,
el globo se hace más pequeño.

## 6 bis. F0 hecha (2026-11-25)

Solo código y docs; falta compilar en el PC de Berón (`tools/verify-compile.sh`) y correr los grupos 142, 75, 104 y 113.

| Qué | Dónde |
|---|---|
| Reglas reescritas: **J16** (reparto por plazas; la suma llega a todos), **S13** (= J16), **J36** (para todos a la vez: la cima y Asambleas), **P9**, **T1** (madera = revestimiento del acero) y nueva **T26** (marcas y reorganización) | [manual-de-diseno.md](../reglas/manual-de-diseno.md) |
| **Marcas de cada piso** (componentes, categoría, cuota, plazas; puro/mixto) y las 4 reglas de la reorganización (dónde cabe, mixto desbordado, solo con cima, desequilibrio), portadas de `organizacion.ts`. `Proposed()` son los árboles del §4; `Reorganize(Proposed())` sube la Residencia a una cima y se la cede a Ciencias (D5) | `FloorOrganization` |
| **Entrenamiento por plazas** (8–9 y 14–15): cada uno al centro de su rotación y, si está lleno, al siguiente con plaza; quien elige primero rota cada día; los maestros primero; sin plazas para todos, al menos lleno (nadie sin entrenar). La jornada lee las plazas una vez al día de cada centro abierto (máquinas, esterillas, `AreaSeat`). El gimnasio respeta sus plazas en los días del plan. Kushal, al incorporarse, también | `WorkRoster.SeatTraining`/`SeatFor`, `AreaCapacity.SeatsOf`, `WorkdayDirector.OpenTraining`, `GymProgramming.ApplyGymDays`, `PlayerWorkday` |
| **Validador J16**: cada centro ≥ 1 plaza y la línea «Entrenamiento (suma de plazas)» ≥ miembros; Aula, ring y Asambleas siguen para todos | `AreaCapacity.Check`, `CapacidadPisosTest` (75) |
| **Edificios amplios**: `TrainingCentres.Amplios = true` sigue levantando los de hoy (⌈miembros / plazas⌉ pisos por centro); F1 lo apaga y usa `FloorsBySeats` (un piso por centro; si la suma no llega, otro al de más plazas) | `TrainingCentres` |
| Prueba | `OrganizacionPisosTest` (grupo 142) |

**W9 en F0:** las horas de tarea no cambian (salen de `AreaFunctions`); cambia solo el reparto del entrenamiento (por plazas en vez de «cada centro cabe entero») y, con los edificios amplios aún en pie, el resultado es el mismo que antes (las plazas sobran). El horario libre (`LeisureRoster`) ya iba por plazas: se reajusta en F1 con las salas nuevas (tabla del §5).

## 6 ter. F1a hecha (2026-11-25); F1b, con el Blockout

F1 se partió en dos: lo que se puede hacer y medir sin Unity (**F1a**, este PR) y las **fusiones de pisos** (**F1b**), que piden
rehacer distribuciones a ojo en el Blockout (W5) porque las de hoy no caben juntas tal cual (medido abajo).

| Qué (F1a) | Dónde |
|---|---|
| **Losa de 40 × 40 m en todas las torres-árbol** (`BeginTreeTower`; antes la del área más grande: Entrenamiento 36, Ciencias 32, Producción 26). El área se amuebla en su planta de siempre, centrada en la losa; el Árbol de los Campos, el parque de energía y la lanzadera no cambian | `SampleSceneBuilder.BeginTreeTower`, `FloorOrganization.FloorSide` |
| **Árboles apartados** para guardar los 16 m en 3D (simulación de losas, puntas, anillos de punta, ramas y troncos, la misma cuenta que `TreeBuildingsTest` §3; reproduce sus 16,3 y 28,1 m de hoy): con pisos de 40 m el Entrenamiento quedaba a 12,3 m del Árbol de los Campos y Producción a 14,4 m. **Entrenamiento (−77, 52) → (−77, 57)** y **Producción (−180, −140) → (−183, −140)**: 17,3 m los dos (Servicios–Entrenamiento 17,7; Residencia–Campos 17,5). Siguen en sus lóbulos (Norte:Bosque_Comestible, r 26; Isla:Tundra, r 66) | `SampleSceneBuilder.Towers` |
| **Avisos de pisos a medias** (reglas 2.9 y 2.10 de CodeShip, nueva **T27**): al final del Blockout, cada piso de 40 m de una torre-árbol con menos de la mitad del piso con algo a ≤ 3 m, o con un punto a más de 6 m de todo (salvo la franja de 2,5 m del borde, para tirarse), sale como `[Pisos]` con su ocupación por **cuartos** (para saber dónde cabe un componente). El tronco cuenta como «algo». Aviso, no error | `FurnitureSpacing.FillOf`/`Footprints`, `SampleSceneBuilder.ReportFloorFill` |
| **Un piso por centro**: `TrainingCentres.Amplios = false`. Con 30 miembros: Gimnasio 1, Calistenia 1, Hatha 1, Ashtanga 1, Ballet 1, Dojo 1 (antes 1, 1, 2, 2, 2, 2): el Entrenamiento baja de ~16 pisos a ~12 (con cardio, Aula y Música) | `TrainingCentres` |
| **W9, horario libre por plazas**: pesas, Hatha, Ashtanga, baile y artes marciales ya no son «para todos»: van por las plazas del piso de su centro (16 esterillas por estilo de yoga, 20 ballet, 25 dojo, 30 gimnasio) y quien no cabe lo hace otro día. Para todos quedan los sitios que miden para todos (huerto, plaza, restaurante, auditorio, Asambleas; J36) | `LeisureDirector.AtCentre`/`CapacityOf` |
| Prueba | `PisosF1Test` (grupo 143) |

**W9 en F1a:** las horas de tarea no cambian (salen de `AreaFunctions`); el entrenamiento de las 8 y las 14 ya iba por plazas (F0) y
ahora las plazas son reales (16 por estilo de yoga en vez de 32): con 30 miembros, la suma (147) sigue llegando. El ocio de los centros
pasa a ir por turnos. Los viajes casi no cambian (5 y 3 m). La iniciación de yoga sigue en la Sala de Yoga.

**F1b (pide el Blockout, W5): las fusiones de §4.** Medido sobre las distribuciones de hoy (el umbral del tronco, 4,6 m en −Z, queda
libre para la puerta: en un piso de 40 m quedan 40 × 35,4 m útiles):

| Fusión | Problema medido | Cómo |
|---|---|---|
| Cría + carne cultivada | Las dos usan la planta de 26 × 20 m: juntas (girada cada una a 20 × 26 en una mitad) piden 20 + 2 + 20 = 42 m de ancho | **HECHA 2026-11-25**: la Carne cultivada es una ZONA del piso de la Cría (su área cuelga del piso, como el Huerto en el Árbol de los Campos; `CriaMeatZone`). La Cría (26 × 20) va girada en la mitad −X (x −20…0) y el laboratorio en la +X (18 × 30 m, `CulturedMeatLab.CompactSize`, distribución en filas con 2 m entre equipos y la esquina +X +Z para su tanque); el pasillo de 2,3 m sube desde la puerta del tronco, con su umbral libre (z ≥ −11). Blockout: `[Espacio]` 0, el piso de la Cría pasa del 19 % al 55 %; Servicios sigue en 4 niveles con un piso libre (7 pisos), para Asambleas + guardia |
| Asambleas + guardia | Las dos llenan su piso de 40 × 38 con coordenadas fijas (el muro de pantallas a z = 14,5) | **HECHA 2026-11-25** (en el PC de Berón el 27): la Sala de guardia es una ZONA del piso de Asambleas, en su cuarto +X +Z (16,5 × 18 m, `GuardZoneSize`; el objeto «Guardia_AUTO» con su `GuardRoom`, hijo del piso): muro de 8 pantallas en dos alturas de 4, dos filas de 4 consolas, radar, carta de hielo y sismógrafo en una fila y radio, tablero, respaldo y mareógrafo en otra; sin aseos propios (los del piso, entre las dos). La asamblea, en los otros tres cuartos: el círculo centrado en (−8, −2), la toguna, la mesa, el archivo y los retoños al norte, el marcador, el kleroterion y la cabina de intérpretes al sur; 72 plazas para 30. Servicios baja de 4 niveles a 3 (6 pisos). Blockout: `[Espacio]` 0, el piso de Asambleas del 70 % al 84 % |
| Gimnasio + cardio | El gimnasio ocupa 36 × 26 con sus 30 máquinas; al cardio le quedarían ~9 m | **HECHA 2026-11-27**: un piso de 40 × 40 con las máquinas de placas (10 tipos × 2) en el cuarto −X +Z, el peso libre (2 estantes, 2 bancos, 2 jaulas, press plano e inclinado, Smith, plataforma) en el +X +Z, el cardio (3 cintas, 2 elípticas, 2 escaladoras, 2 bicis, 2 reclinadas, 2 remos) en el −X −Z, la zona libre en el +X −Z y 6 esterillas entre medias; huellas medidas en el Blockout. W9: el Gimnasio da 20 plazas (antes 30). Entrenamiento: 6 pisos de centros (uno libre arriba). El pórtico y sus sacos van juntos (`FurnitureSpacing.Sets`). Revisión: la caja del gimnasio pasa a 40 × 38 (`FitAreaBox`, como el Textil) |
| Salas (yoga, música, ballet, bailes) | Cada sala de yoga mide 18 × 26 (no cabe en un cuarto de 20 × 20); el auditorio de 60 butacas no cabe en un cuarto (D4: a Asambleas) | Salas de yoga de 8 esterillas en 20 × 20; cabina de música (`SoundRoom`) en un cuarto; ballet (18 × 8,6 m) en un cuarto; bailes nuevo |
| **Piso «Salas» (HECHO 2026-11-28, opción 3: sin yoga ni auditorio)** | El último piso de estudios de Música (69 %) tenía sitio en +Z; el Ballet (en obras) llenaba un piso entero | El último piso de estudios es el piso «Salas»: sus 4 salas de ensayo y 2 estudios bajan junto al umbral, y en la mitad +Z el **Ballet** (zona de su área, 18,4 × 14 m, en obras con su Acabado: `BalletZone`) y la **sala de bailes** (pista de 14 × 11 m, 20 plazas de entrenamiento: `FurnishBailes`). El Entrenamiento baja un piso (5 niveles). Las salas de yoga y el auditorio esperan la decisión de Berón (4 salas de 20 × 20 no caben: una sala con su kund y su cabecera pide ~20 × 24,3 m) |
| **Mecánica + garaje (HECHA 2026-11-25)** | El Blockout dio la Mecánica al 14 % y el garaje (un piso libre) al 5 % | La plaza del submarino (7 × 16 m) en el centro del piso de la Mecánica (`SubmarineGarage`); Producción baja de 3 niveles a 2. Probado en el PC de Berón: compila, 73 (submarino), 75, 113 y 143 en verde (175/0). La plaza va sobre la alfombra del área (la tapaba) y los microcosmos de Mecánica no cuentan el garaje (`MicroFloorElements.Skip`). Falta amueblar el taller (el piso sigue al 14 %) |
| **Textil + huerto vertical (HECHA 2026-11-27)** | El Blockout dio los dos al 34 % | El huerto vertical es una ZONA del piso del Estudio Textil, en su mitad +X (18 × 30 m, `FarmZoneSize`; sus bloques con `PackFloor` y sus aseos en la esquina −X +Z de la zona); el textil (26 × 20), girado, en la −X; si la demanda pidiera más estantes, los pisos de más van encima. Producción: 3 pisos en 2 niveles (uno libre). Revisión: la caja del Textil pasa a 40 × 38 (como la de la Cría: sus bordillos de 26 × 20 cruzaban las dos zonas) |
| Hospital (urgencias + hospitalización), laboratorios (química + biología) | Varios pisos con distribución propia cada uno | Uno a uno, con los avisos `[Pisos]` del Blockout como guía |

**Primer Blockout de F0 + F1a (2026-11-25, PC de Berón):** compila sin errores; `[Capacidad]` 0 por debajo; `[Centros]` un piso por
centro (Entrenamiento: 7 pisos en 6 niveles); `[Pisos]` 40 de 40 pisos a medias (esperado: se amueblan en su planta de antes; los más
vacíos, los pisos libres y las obras al 5 %, la cocina al 7 %, la Mecánica al 14 %; los más llenos, hospitalización 81 %, veterinaria 71 %,
gimnasio 70 %). Pruebas: 244 PASS / 3 FAIL: 57, 65, 142 y 143 en verde; en 75 el Aula e Informática, con todos sus puestos en obras al
empezar, no tenían línea (arreglado: `AreaCapacity.Check` da la línea con 0 hechos y los de la obra); en 113, «nadie trabaja fuera de su
sitio» falló sin detalle (ahora lo da, para la próxima pasada).

Cada fusión es un piso menos: se hacen de una en una en el Blockout del santuario (pide permiso a Berón: él juega en ese Editor),
mirando sus avisos `[Pisos]` y `[Espacio]`, y su prueba (grupos 53, 57, 75, 113, 143).

## 7. Decisiones para Berón

| # | Pregunta | Mi recomendación |
|---|---|---|
| **D1** | ¿Pisos para los miembros actuales (uno por actividad, entrenamiento repartido por plazas) o mantener J16/J36 (cada centro para todos a la vez)? | **DECIDIDO (Berón 2026-11-25): uno por actividad, por plazas**; la cima y Asambleas, para todos a la vez (C1) |
| D2 | ¿Árboles al anillo exterior sobre bases negras en el mar (los casquetes, solo de los animales)? | Sí (C4) · **«Sigue» 2026-11-25: según la recomendación (D2–D6)** |
| D3 | ¿Globo con el pulpo en el santuario 1, o en el Santuario Marino? | En el s1, solo con su fauna somera (C5) |
| D4 | ¿Dónde va el auditorio de música (60 butacas)? | En Asambleas (sala para todos) |
| D5 | ¿La Residencia sube a la cima de Ciencias, como en CodeShip? | Sí: que lo decidan las reglas de reorganización |
| D6 | ¿Orden de fases? | F0 → F1 → F2 → F4 → F3 → F5: primero lo que baja objetos, el globo al final |
