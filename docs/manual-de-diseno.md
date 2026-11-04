# Manual de diseño de Cold Sanctuary

Berón (2026-11-04): «estas reglas que te he ido comentando, ¿las tenemos recopiladas en un .md del proyecto? Hacerlo si no es así:
debemos tener un manual de diseño».

Este es el **único sitio** que reúne todas las reglas de diseño y de construcción que ha dado Berón. Cada regla dice qué pide,
por qué (en una línea), **dónde se comprueba** y de qué doc sale. El detalle (medidas, fuentes, historia de cada decisión) sigue
en el doc de su tema; aquí no se repite.

**Cómo leer las tablas**
- **Se comprueba en**: el validador o la prueba que falla si se rompe la regla. «Grupo N» es el grupo del `TestRunner`
  (`ed.sh test` o «Tools/Cold Sanctuary/Jugar con las pruebas»).
- **solo en docs**: la regla no tiene comprobación automática; hay que mirarla a mano (o en fotos) al cambiar algo.
- **PENDIENTE DE IMPLEMENTAR**: Berón la pidió y aún no está en el código. Vale igual como regla de diseño para lo nuevo.
- Si una regla nueva contradice a una de aquí, manda la más reciente (con su fecha) y se corrige esta tabla.

**Al recibir una regla nueva de Berón**: añádela aquí (sección, fila, fuente) y en el doc de su tema, y apunta en
[checklist.md](checklist.md) si queda pendiente. Si cambia un piso del santuario, mira [§11](#11-sincronización-mesocosmos--microcosmos).

## Índice

1. [Santuario (mesocosmos)](#1-santuario-mesocosmos)
2. [Pisos: sus tres versiones](#2-pisos-sus-tres-versiones)
3. [Edificios-árbol](#3-edificios-árbol)
4. [Alfombras voladoras](#4-alfombras-voladoras)
5. [Jardines y frutales](#5-jardines-y-frutales)
6. [Hospital](#6-hospital)
7. [Microcosmos](#7-microcosmos): plantilla y piso · escala · pueblos · reglas de edificación · terreno y vegetación · estatuas y muebles
8. [Animales en grupo](#8-animales-en-grupo)
9. [Personajes, jornada y horario libre](#9-personajes-jornada-y-horario-libre)
10. [Pruebas y flujo de trabajo](#10-pruebas-y-flujo-de-trabajo)
11. [Sincronización mesocosmos ↔ microcosmos](#11-sincronización-mesocosmos--microcosmos)
12. [Recuento y reglas sin comprobación automática](#12-recuento-y-reglas-sin-comprobación-automática)

---

## 1. Santuario (mesocosmos)

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| S1 | **Lo construido no ocupa el suelo de la naturaleza**: todo va elevado; el suelo queda para árboles, animales e insectos. | El santuario es de la naturaleza; las personas viven por encima. | `SanctuaryLifeTest` (grupo 28: cocina y huerto elevados); primer piso de los árboles, ver T3 | [construcciones-elevadas.md](construcciones-elevadas.md) |
| S2 | **Pocas torres temáticas con muchos pisos**: Servicios, Entrenamiento, Ciencias y Producción; los cultivos en el Árbol de los Campos; la Central Nuclear aparte (sitio de exclusión). | Menos edificios y más altos dejan el suelo a la naturaleza y aprovechan columnas para 30 pisos. | `SanctuaryLifeTest` (28: 4 torres con sus pisos, áreas apiladas) | [reglas-plataformas.md](reglas-plataformas.md) #9, [plano-edificios.md](plano-edificios.md) |
| S3 | **Todo edificio es un árbol** (modelo `HelixTower`), salvo la Central Nuclear (1 piso a 12 m). | Un solo modelo: nado entre pisos, suelo libre, sol y lluvia repartidos. | `TreeBuildingsTest` (grupo 57) | [edificio-arbol.md](edificio-arbol.md) §Migración |
| S4 | **≥ 16 m en 3D entre torres** (pisos, anillos, puntas, ramas y troncos; el Árbol de los Campos, girando). Dos vecinos con anillo necesitan al menos 8 + 8 m. | Los anillos vuelan fuera de su piso y se pisaban (restaurante y hospital, 2026-10-24). | `TreeBuildingsTest` (57: holgura mínima medida) | [edificio-arbol.md](edificio-arbol.md), [reglas-plataformas.md](reglas-plataformas.md) #4 |
| S5 | **Sin escaleras, sin puentes y sin barreras invisibles en los bordes.** Se va en alfombra o, dentro de un árbol, nadando. Se puede saltar entre plataformas: debajo hay cojín de musgo. | Moverse fácil, pocas opciones por selector, nada que tape la vista. | `SanctuaryLifeTest` (28: «ninguna área tiene escaleras», «ya no hay puentes», el musgo amortigua) | [alfombras-plan.md](alfombras-plan.md), [reglas-plataformas.md](reglas-plataformas.md) #3 |
| S6 | **Sin muros ni techos.** La única pared permitida es la de **cristal**, solo en salas que deben aislarse, y guarda los 2 m. | Los muros tapan la vista y el paso. | `FurnitureSpacing` (los paños cuentan como muebles); `GlassWallTest` (grupo 84) | [hospital.md](hospital.md) §6 |
| S7 | **2 m de paso** entre todo lo que no va junto (dos personas pasan a la vez). Lo que va unido cuenta como un mueble: piezas de un mueble, sillas con su mesa, una línea de encimeras, la cama y su mesilla. | Que nadie choque ni se atasque; antes los muebles se amontonaban. | `FurnitureSpacing` + `FurnitureLine`; `SanctuaryLifeTest` (28: 0 violaciones por área) | [reglas-plataformas.md](reglas-plataformas.md) #2 |
| S8 | **Medida humana**: cada mueble y máquina sirve a la persona más alta (1,85 m, con margen hasta 1,90 m): camilla de 2,00 × 0,70 m, asientos a 0,45 m. | El jugador tumbado no cabía en la camilla. | **solo en docs** (la prueba que mida asientos, camas y puestos está pendiente) | [medidas-muebles.md](medidas-muebles.md), [reglas-plataformas.md](reglas-plataformas.md) #1 |
| S9 | **Estructura para 30 pisos**: `StructuralLimits.MaxFloors` = 30; el tronco de cada árbol se calcula (`RootCheck`) y se refuerza (`Reinforce`): avisa por encima del 80 %, error por encima del 100 %. | Los edificios crecen hacia arriba; que nada se construya sin aguantar. | `StructuralLimits` al construir; `SanctuaryLifeTest` (28: «ningún edificio pasa de 30 pisos») | [reglas-plataformas.md](reglas-plataformas.md) #7, [edificio-arbol.md](edificio-arbol.md) §Estructura, [columnas-sismicas.md](columnas-sismicas.md) |
| S10 | **Plazas reservadas en cada piso** (`PlatformSlots`) para la flora, la fauna, los personajes y la historia de su microcosmos. Cambiar un piso obliga a revisarlas. | El microcosmos de cada piso necesita sitio garantizado. | `SanctuaryLifeTest` (28: plazas mínimas, ninguna a < 1 m de un mueble); `ExperienciaJugadorTest` (grupo 40) | [reglas-plataformas.md](reglas-plataformas.md) #8 |
| S11 | **Energía solar, sin gas**: paneles junto a cada edificio y en cada techo; cocinas de inducción. | No llega combustible al santuario; coherencia del mundo. | `SanctuaryLifeTest` (28: «las cocinas son de inducción») | [construcciones-elevadas.md](construcciones-elevadas.md) |
| S12 | **Una casa del árbol por habitante**, con su cama, su alfombra-esterilla y su terraza frutal; cada uno duerme en la suya. | Cada personaje tiene su sitio y su vida diaria. | `SanctuaryLifeTest` (28: «Kushal tiene su habitación», cada casa con su alfombra); `FruitOrchardTest` (55) | [alfombras-plan.md](alfombras-plan.md), [jardines-frutales.md](jardines-frutales.md) |

## 2. Pisos: sus tres versiones

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| F1 | **Todo piso tiene forma finalizada y forma en construcción** (`FloorForms`): una obra tiene su «Acabado» oculto; un piso terminado tiene su «Forma_en_obra» oculta (andamio, palés, conos, muretes). | El juego enseña obras y reformas; el microcosmos se monta sobre lo terminado. | `SampleSceneBuilder.EnsureFloorForms` (registro `[Pisos] Formas de N pisos`); `MicroFloors.Validate`; `BruceLeeStoryTest` (grupo 60) | [construcciones-elevadas.md](construcciones-elevadas.md) §Las dos formas |
| F2 | **Tercera versión: el microcosmos.** Todo piso tiene tres versiones: terminada, en construcción y microcosmos. Cada elemento del piso se lista y se construye en versión **meso** (más plana) y versión **micro** (detallada). **PENDIENTE DE IMPLEMENTAR.** | El microcosmos debe ser el mismo piso visto por un insecto, con todo su detalle. | **solo en docs** | memoria 2026-11-04 (Berón) |
| F3 | **En la versión micro, los muebles son terreno o edificios**: fregadero = lago (o fuente o río si está abierto); cajones y armarios = edificios; caja = cueva o fábrica; zapato = cueva o mercado; fogones = volcanes; bombilla = sol; lámparas = farolas; mesas y sillas = montañas o picos nevados; las alfombras se quitan, se apilan o hacen desniveles. **PENDIENTE DE IMPLEMENTAR.** | Que el mundo gigante se lea como paisaje y no como muebles sueltos. | **solo en docs** | memoria 2026-11-04 |
| F4 | **Todos los pisos de un edificio miden lo mismo**: el cuadrado del lado mayor de su área más grande (`SetBack` = 0). Cada área va centrada en su piso, con su lado −Z contra el tronco. | Columnas continuas; ningún piso tapa ni recorta a otro. | `TreeBuildingsTest` (57: áreas en sus pisos) | [alfombras-plan.md](alfombras-plan.md), [edificio-arbol.md](edificio-arbol.md) §Migración |
| F5 | **Las puertas de los pisos van donde su umbral queda libre de muebles.** | Se entra y se sale sin chocar. | `TreeBuildingsTest` (57: umbrales libres) | [edificio-arbol.md](edificio-arbol.md) |
| F6 | **Al cambiar un piso**: comprobar los 2 m, la zona de alfombras, las plazas reservadas, los pisos de la torre y **los microcosmos que dependen de él** (§11). | Un cambio en el meso se nota luego en el micro. | `SanctuaryLifeTest` (28) y las pruebas de §11 | [reglas-plataformas.md](reglas-plataformas.md) §Al cambiar una plataforma |
| F7 | **Yoga: sin anillo**, porque desde el anillo no se ve al maestro. **PENDIENTE DE IMPLEMENTAR** (hoy `SanctuaryLifeTest` aún pide «las estatuas en su anillo»: hay que cambiar la prueba con el piso). | El alumno debe ver al maestro. | **solo en docs** | memoria 2026-11-04 |
| F8 | **Pisos de yoga separados**: hatha en uno y ashtanga en otro; investigar lo que pide cada estilo (tierra o esterilla); una versión **occidental** y otra **tradicional**, cada una con sus estatuas o elementos (p. ej. el Dhyanalinga de Isha); quizá un baño en el río antes de practicar. **PENDIENTE DE IMPLEMENTAR.** | Cada estilo tiene su práctica y su ambiente. | **solo en docs** | memoria 2026-11-04 |

## 3. Edificios-árbol

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| T1 | **Tronco cuadrado de agua transparente** (20 × 20 m de agua, pared de acrílico casi invisible, exoesqueleto de nervios CFST que carga). Entre pisos se nada. | Se ve nadar a la gente; el tronco sustituye a las escaleras. | `TreeOfFieldsTest` (grupo 53), `TreeBuildingsTest` (57) | [edificio-arbol.md](edificio-arbol.md) |
| T2 | **Hélice**: en cada nivel, pisos en dos caras opuestas y ramas a un campo en las otras dos; el nivel siguiente alterna (molinete). Plataformas, ramas y tronco, **cuadrados**. | Que ningún piso tape a otro y se reparta el sol. | 53 y 57 (hélice alterna, nada tapa a nada) | [edificio-arbol.md](edificio-arbol.md) |
| T3 | **Primer piso ≥ 50 m y por encima del árbol más alto** (hoy 64 m). Excepción: el **árbol de la guardia** (árbol modificado) crece tan alto como el edificio más alto. | El suelo queda libre para los árboles. | 53 (primer piso sobre el árbol más alto medido), 57 (árbol de la guardia) | [edificio-arbol.md](edificio-arbol.md) §El árbol más alto, §Migración |
| T4 | **Entradas solo en el suelo y en el techo**: las alfombras solo están ahí. Se entra de frente por la puerta del techo. | Un solo camino claro: alfombra al techo y nado por dentro. | 57 (alfombras solo en el suelo y en el techo) | [edificio-arbol.md](edificio-arbol.md) |
| T5 | **Medidas que mandan**: niveles cada 22 m; ramas de 10 × 10 m; campos de punta de 20 × 20 m, 12 m sobre su nivel, con su anillo encima; anillo de un piso a 8 m sobre el piso. | Caben las personas, la fauna y el submarino de obra. | `TreeSpec`/`HelixTower`; 57 y `SubmarineTest` (grupo 73) | [edificio-arbol.md](edificio-arbol.md) §Ramas y niveles al doble, [submarino-de-obra.md](submarino-de-obra.md) |
| T6 | **Puertas cuadradas de 7 × 7 m**: caben 8 personas a la vez y el submarino de obra. Las ramas, cuadradas y más anchas que las puertas. | Tráfico de muchas personas y de maquinaria. | 57 (puertas por árbol), 73 | [mascaras-de-oxigeno.md](mascaras-de-oxigeno.md), [submarino-de-obra.md](submarino-de-obra.md) |
| T7 | **Sin suelo**: el agua del tronco baja al mar por un hueco en el hielo; entran peces, kril y medusas. En el hueco cuelgan cortinas de fitoplancton. | Unir el edificio con el mar y su vida. | `GroupAnimalsTest` (grupo 71: un grupo marino sube al tronco), `TreeMaskTest` (grupo 70) | [mascaras-de-oxigeno.md](mascaras-de-oxigeno.md) |
| T8 | **Máscaras de oxígeno**: un perchero fuera de cada puerta y una estación dentro, en cada nivel. Con la máscara puesta el oxígeno es ilimitado. Quien acaba en el agua sin máscara nada a la estación más cercana. | Que se pueda nadar sin límite y nadie se ahogue. | `TreeMaskTest` (grupos 70 y 72) | [mascaras-de-oxigeno.md](mascaras-de-oxigeno.md) |
| T9 | **Vía marina de obra**: la maquinaria pesada y el material viajan por agua en un submarino anfibio (14 × 5 × 5 m) que aparca en el piso de obra o en el garaje. | Sin escaleras ni grúas que tapen; el agua ya es el camino. | `SubmarineTest` (73: el material llega) | [submarino-de-obra.md](submarino-de-obra.md) |
| T10 | **Solo gira el Árbol de los Campos** (una vuelta cada 6 h de juego, contra el sol); las torres no giran. | Los campos reparten sol y lluvia; un hospital o una cocina no pueden girar. | 53 (giro y NavMesh) | [edificio-arbol.md](edificio-arbol.md) §Giro, §Migración |
| T11 | **Viajes sin teletransportes**: se anda, se vuela o se nada; si alguien se atasca, se queda donde está y avisa. | Que todo movimiento sea visible y creíble. | 53 (viajes a nado sin saltos ni atascos) | [edificio-arbol.md](edificio-arbol.md) §Moverse |
| T12 | **Un área con anillo lo conserva en U**, abierto al tronco, con su balcón y su puerta; se llega nadando. | Mantener las camas y el comedor de los anillos con el nuevo modelo. | **solo en docs** (lo hace `TowerFloor`; sin prueba propia) | [edificio-arbol.md](edificio-arbol.md) §Migración |

## 4. Alfombras voladoras

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| C1 | **Cuatro clases de zona de alfombras** (`CarpetPad.Kind`): Edificio (sus pisos), Santuario (otros techos y la residencia), Residencia (las habitaciones) y Habitación (vuelve a la plataforma central). | Pocas opciones en cada selector. | `SanctuaryLifeTest` (28: el selector de edificio solo ofrece sus pisos; cada casa con su alfombra) | [alfombras-plan.md](alfombras-plan.md) |
| C2 | **Una zona en cada piso, anillo y techo**; el techo lleva la de edificio y la de santuario. En los árboles, solo en el techo y al pie (T4). | Siempre hay una alfombra a mano. | 28 (cada anillo con su zona), 57 | [alfombras-plan.md](alfombras-plan.md), [reglas-plataformas.md](reglas-plataformas.md) #5 |
| C3 | **Cada pista sabe qué alfombra ocupa cada hueco** y apila las que sobran sin solaparlas (`CarpetPad.Claim`). | Que no se monten alfombras unas sobre otras. | 28 | [reglas-plataformas.md](reglas-plataformas.md) #5 |
| C4 | **El selector del jugador va por áreas** (`CarpetPad.PlayerMenu`): una opción por área, sin «Volar a:», y la alfombra va a la entrada del área. | Menos lista, menos confusión. | **solo en docs** (sin prueba propia) | [alfombras-plan.md](alfombras-plan.md) |
| C5 | **Todo el que anda usa la red**: `CarpetPad.Route` (y `ArbolCampos.Route` en los árboles) decide andar, volar o nadar; lo usan la jornada, los hechizos y las iniciaciones. | Ningún personaje se queda sin camino. | `JornadaTest` (grupo 50), 57 | [alfombras-plan.md](alfombras-plan.md), [edificio-arbol.md](edificio-arbol.md) |
| C6 | **La alfombra de cada casa es también su esterilla de yoga.** | Se puede hacer yoga en casa. | 28 (cada casa con su alfombra) | [alfombras-plan.md](alfombras-plan.md) |
| C7 | **Alfombras más gruesas**: son alfombras tecnológicas con circuitos dentro. **PENDIENTE DE IMPLEMENTAR.** | Que se lea que son máquinas. | **solo en docs** | memoria 2026-11-04 |

## 5. Jardines y frutales

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| G1 | **El espacio libre de los techos se llena de frutales**, uno de cada especie y otra vuelta, con una experta, una sala de cuidados y un jardinero del día que va de edificio en edificio. | Comida fresca y un trabajo de cuidado en el polo. | `FruitOrchardTest` (grupo 55) | [jardines-frutales.md](jardines-frutales.md) |
| G2 | **Riego por goteo**: el agua llega sola; la ronda revisa, abona y trata plagas. | Con riego a mano hacían falta 5 personas al día. | 55 (prueba del goteo) | [jardines-frutales.md](jardines-frutales.md) §Riego por goteo |
| G3 | **Ninguna ronda pasa de 3 h** (se planifica a 2,9 h); las macetas de un edificio no se parten; las tareas de la semana (polinizar, podar) se reservan en cada ronda. | Que la jornada del jardinero sea realista. | 55 | [jardines-frutales.md](jardines-frutales.md) §Rondas que caben en 3 h |
| G4 | **Cima de los árboles: bancales de tierra por gremios** (frutales que comparten pH), con plantas compañeras, flores de néctar, insectos útiles, abejorros, lombrices y caja nido; las puntas siguen en maceta. | La tierra guarda agua y vida, y deja sitio a los aliados del jardín. | `RoofGardenTest` (grupo 80) | [jardines-frutales.md](jardines-frutales.md) §Jardines en la cima |
| G5 | **El jardinero también revisa máscaras y fitoplancton** (panel en la puerta del suelo, 1/7 de las estaciones por rotación semanal y las que marcan rojo). | Mismo oficio de cuidado; sin rondas de 36 h. | 55, `TreeMaskTest` (70) | [jardines-frutales.md](jardines-frutales.md), [mascaras-de-oxigeno.md](mascaras-de-oxigeno.md) |
| G6 | **Cada casa del árbol de la residencia tiene su terraza frutal**, con un frutal en maceta a ≥ 2 m de sus muebles. | Berón pidió los frutales en las residencias del santuario. | 55 | [jardines-frutales.md](jardines-frutales.md) §Frutales en las residencias |
| G7 | **Los cultivos van en el Árbol de los Campos**, no en parcelas en el suelo. | Los lóbulos vuelven a la naturaleza. | `TreeOfFieldsTest` (53), 28 | [plano-edificios.md](plano-edificios.md), [edificio-arbol.md](edificio-arbol.md) |
| G8 | **Un bancal o una maceta es un mueble**: guarda los 2 m. | Que se pueda pasar y trabajar alrededor. | `FurnitureSpacing`; 55 y 80 | [jardines-frutales.md](jardines-frutales.md) |

## 6. Hospital

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| H1 | **Un solo hospital para personas y animales** («una sola salud»), capaz de todos los casos, con tantas plazas para animales como para personas. | No hay otro hospital a miles de km. | **solo en docs** | [hospital.md](hospital.md) §1–2 |
| H2 | **8 puestos médicos: 4 humanos y 4 veterinarios**; 4 puestos iguales por servicio para que 4 médicos hagan la misma tarea a la vez. | Turnos y casos simultáneos. | `HospitalStation.PostFor` en código; **sin prueba propia** | [hospital.md](hospital.md) §3, [hospital-plan.md](hospital-plan.md) |
| H3 | **No se opera sin alguien en la anestesia**, ni a quien perdió ≥ 30 % de sangre sin sangre compatible. | Reglas reales de cirugía segura. | `BloodBankTest` (grupo 83) para la sangre; la anestesia, en `HospitalStation` sin prueba propia | [hospital.md](hospital.md) §3–4 |
| H4 | **Banco de sangre con las reglas reales** (temperaturas, caducidades, compatibilidad, donantes del santuario). | Aislados, el banco es «andante». | 83 | [hospital.md](hospital.md) §4 |
| H5 | **Paredes de cristal solo en salas que deben aislarse** (quirófanos, UCI, aislamientos, sala fría) y solo en épocas en que existían. | Aislar sin tapar la vista. | `GlassWallTest` (84), `MicroBuildRules` | [hospital.md](hospital.md) §6 |
| H6 | **Un anillo de camas por planta**: cada paciente se recupera en el piso de su servicio. | Ganar suelo sin mezclar servicios. | **solo en docs** | [hospital.md](hospital.md) §2 |
| H7 | **Submarino-ambulancia** para personas y animales grandes, y el **Leviatán** para ballenas y orcas. | Llegar a todo el santuario y al mar. | `HospitalSubmarineTest` (81), `WhaleHospitalTest` (82) | [submarinos-medicos.md](submarinos-medicos.md) |
| H8 | **El hospital del microcosmos es una construcción propia** que incluye TODOS los instrumentos, muebles y decoraciones del hospital meso, en su versión micro. **PENDIENTE DE IMPLEMENTAR.** | Hoy el microcosmos es solo la copia gigante del piso. | **solo en docs** | memoria 2026-11-04; ver §11 |

## 7. Microcosmos

### 7.1 Plantilla y piso

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| M1 | **Todo microcosmos sale de la plantilla** (`MicrocosmTemplate` / `MicrocosmSpec`) con tres datos obligatorios: **fecha, hora y piso** del santuario. | Que todos sigan las mismas reglas. | `MicrocosmSpec` (sin piso no se crea); `MicroFloors.Validate` | [microcosmos-areas.md](microcosmos-areas.md) §Reglas, regla 1 |
| M2 | **Pertenece a un piso y se monta ENCIMA**: el piso ×100 es su suelo y la historia va a ras de él. | El microcosmos es el santuario visto por un insecto. | `MicroFloors.Validate`; `MicrocosmosFloorTest` (grupo 43) | [microcosmos-areas.md](microcosmos-areas.md) regla 2 |
| M3 | **Siempre sobre la forma FINALIZADA** del piso (nada de andamio ni conos). | Lo que se recrea es el piso terminado. | `MicroFloors.Validate`, `BruceLeeStoryTest` (60) | [microcosmos-areas.md](microcosmos-areas.md) regla 3 |
| M4 | **Si ningún piso encaja: «pendiente de piso representativo»**; se monta en el más parecido y se apunta el piso que falta. | Saber qué pisos faltan por crear. | `MicroFloors.Validate`; menú «Pisos que faltan» | [microcosmos-areas.md](microcosmos-areas.md) regla 7 |
| M5 | **El microcosmos es SOLO su piso**, cerrado por muros en el borde; fuera no se puebla nada. | Antes la flora y la fauna se escapaban o desaparecían. | `MicrocosmosFloorTest` (43) | [reglas-plataformas.md](reglas-plataformas.md) #11 |
| M6 | **El piso no se rompe**: ninguna pieza se quita ni se mueve; el pueblo y la historia se adaptan, rodeando los muebles o subiéndose a ellos. | En Bruce Lee el dojo parecía romperse para dejar sitio a la ciudad. | `MicroFloors.Validate` (piezas al montar = piezas ahora), 60 | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 13 |
| M7 | **Todas las estructuras del piso son superficie utilizable**: flora y fauna encima de los muebles gigantes; barrio alto y miradores en sus mesetas. | Aprovechar todo el mundo gigante. | `MicroFloors.Validate`, 43 | [microcosmos-areas.md](microcosmos-areas.md) regla 4 |
| M8 | **Los insectos-humanos trepan** paredes y patas de muebles (`NavMeshLink` a cada cara de arriba, `Climbable` para el jugador). | Así llegan a lo construido sobre los muebles. | `MicroFloors.Validate` (enlaces para trepar) | [microcosmos-areas.md](microcosmos-areas.md) regla 9 |
| M9 | **Una historia puede reconstruir su propio microcosmos** a partir de la plantilla del piso. **PENDIENTE DE IMPLEMENTAR.** | Cada historia necesita su versión del lugar. | **solo en docs** | memoria 2026-11-04 |
| M10 | **La luz sale de la fecha, la hora y el lugar** (`SunFor`: elevación, azimut y color reales). | La plantilla pide fecha y hora para eso. | **solo en docs** (sin prueba propia) | [luz-microcosmos.md](luz-microcosmos.md) |

### 7.2 Escala

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| M11 | **Escala ×100** (`AreaMicrocosmos.AreaScale`): un insecto-humano mide como una persona y una cama de 2 m mide 200 m. El barrio de la historia es el piso ×10; el de un pueblo, ×60, con el plano real a 1:1. | A ×10 la escala era irreal (una hormiga de 18 cm junto a una cama). | `MicroFloors.Validate` (barrio ≥ 40 % del piso) | [microcosmos-areas.md](microcosmos-areas.md) §Escala, [microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md) |
| M12 | **La flora del pueblo es de pueblo humano**: a escala real, escasa, fuera de calles y solares, ninguna planta > 25 m ni flotando. | En Bruce Lee las flores gigantes tapaban el pueblo. | `MicrocosmosFloorTest` (43) | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 5 |
| M13 | **Revisar la escala de estatuas, fuentes, platos y ollas** (quizá ×2 todo): el jugador no puede ser más grande que el comedor. **PENDIENTE DE IMPLEMENTAR.** | Visto en la plaza de Bolívar. | **solo en docs** | memoria 2026-11-04 |

### 7.3 Pueblos

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| M14 | **Eras avanzadas (desde −3500): el pueblo o la ciudad real ENTERA**, investigada y levantada sobre su plano real (calles, ríos, lagos, montes, sitio de cada edificio). | Que la historia ocurra en su lugar de verdad. | `MicroFloors.Validate` (≥ 12 edificios), `CivilizedErasTest` (grupo 62) | [microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md), regla 5 |
| M15 | **Solo insectos-humanos civilizados**; flora y fauna salvajes solo si el lugar y la época las tenían. | Fidelidad a la historia (un laboratorio de París de 1898 no tiene fauna). | 43 (animales del pueblo); el resto, **solo en docs** | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 5 |
| M16 | **El pueblo se amplía con edificios de actividades del santuario** (estudio, entrenamiento, baile, plazas, estaciones, parques, museos, investigación, guarderías, estatuas, policía, bomberos, mercados, talleres, milicia, piscinas, hoteles, carpintería, obra, tecnología…), con el nombre de su época y lugar; lo que no existía no se construye. | Que los habitantes trabajen, aprendan y descansen como en el santuario. | `MicroBuildRules.ValidateSites` (regla 10), `MicroBuildRulesTest` (grupo 63) | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) reglas 8–12 |
| M17 | **Cada edificio nace con su interacción y su categoría** (trabajo, descanso, recuperación u ocio), su capacidad y los cuerpos que lo usan. | Entra solo en las actividades semanales. | `ValidateSites` (regla 8), 63; `HorarioLibreTest` (65) | [microcosmos-actividades.md](microcosmos-actividades.md) |
| M18 | **Un hogar para cada personaje** (una cama por persona, retrete, lavabo, mesa con sillas, despensa), además de su trabajo y los servicios básicos; las parejas comparten casa. | Cada uno vive su ciclo de vida completo. | `Household.Check`; 62 | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 6 |
| M19 | **El ciclo de vida de humanos e insectos-humanos es el mismo** (necesidades, etapas, vejez, pareja, nacimiento, muerte). | Son personas con otro cuerpo. | `CicloHumanoTest` (59), 62 | memoria 2026-11-02, [ciclo-de-vida.md](ciclo-de-vida.md) |
| M20 | **Toda residencia tiene su frutal**, de su época y lugar. | Berón lo pidió para las casas del microcosmos. | `ValidateSites` (regla 11), 63 | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 11 |
| M21 | **Edificios de varias plantas en terraza**, con rampa y sin techos. | Ninguna planta tapa a otra. | `ValidateSites` (regla 12), 63 | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 12 |
| M22 | **Como mucho 150 edificios por pueblo**; piezas de pocos cubos. | Rendimiento (35 000–76 000 renderers por escena). | **solo en docs** (el registro `[Pueblos]` lo dice) | [microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md) |

### 7.4 Reglas de edificación (micro y meso)

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| M23 | **2 m de paso entre objetos** (dos insectos-humanos a la vez); calles de 4 m. | Que se crucen sin chocar. | `MicroBuildRules.Validate`, 63 | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 1 |
| M24 | **Sin muros ni techos**, salvo los **muros que la historia necesita** (gueto de Varsovia, bloque 4 de Chernóbil: `MicroPiece.kind = "historico"`) y el **cristal** de salas aisladas. | Visibilidad y paso, como en el santuario. | `Validate`, 63 | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 2 |
| M25 | **Muebles detallados, con sus utensilios** (sartén, olla, cuchillo…): nada de cubos lisos. | En Bruce Lee la cocina eran cubos. | `Validate` (un puesto de un solo cubo falla), 63 | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 3 |
| M26 | **Necesidades básicas siempre**: dormir, aseo, lavarse, comer, sentarse y cocinar. | La gente vive allí. | `Validate` (`Needs`), 62 y 63 | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 4 |
| M27 | **Muebles a la medida de quien los usa** (`MicroBody`: centauro, cúpula, alado, mantis, araña, ciempiés, humano). | Un insecto-humano no cabe en una cama humana. | `ValidateSites` (regla 9), 63 | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 7 |
| M28 | **Adaptado a personas y a insectos-humanos**: cada edificio tiene al menos un mueble para cada uno. | Los dos lo usan. | `ValidateSites` (regla 9), 63 | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 9 |
| M29 | **Estilo de época y lugar desde el catálogo** (`MicroStyles`: año + región → cada mueble); épocas y lugares parecidos reutilizan la entrada y la plantilla la elige sola. | Coherencia histórica sin rehacer cada pueblo. | `ValidateSites` (regla 10), 63 | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 7 y catálogo |
| M30 | **Sillas y bancos se pueden usar** (sentarse de verdad); platos y ollas a su tamaño. **PENDIENTE DE IMPLEMENTAR** (visto en la plaza de Bolívar). | Los muebles son para usarse. | **solo en docs** | memoria 2026-11-04 |

### 7.5 Terreno y vegetación

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| M31 | **Relieve con aspecto real**: montes y colinas facetados con roca, canchal y nieve; lagos de orilla irregular; ríos con riberas; acantilados. Nada de conos grises. | «¿Un cono gris? ¿Es una montaña?» | **solo en docs** (`CivicBuilder.Terrain`; se mira en fotos) | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) regla 14 |
| M32 | **La vegetación crece siempre ENCIMA de las superficies, nunca debajo.** **PENDIENTE DE IMPLEMENTAR** como regla general (pasó en Ambrosio y se repite). Hoy solo se comprueba en los pueblos. | Una planta bajo el suelo no se ve y rompe la escena. | Parcial: 43 («ninguna planta flota» en los pueblos), `ExperienciaJugadorTest` (40: la flora sobre el suelo del área gigante) | memoria 2026-11-04 |
| M33 | **Capa de tierra de ~10 cm** sobre los pisos que usan los microcosmos: irregular, con montículos y piedras. **PENDIENTE DE IMPLEMENTAR.** | Que el suelo parezca tierra y no una losa. | **solo en docs** | memoria 2026-11-04 |

### 7.6 Estatuas

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| M34 | **Toda estatua humana tiene su versión insecto-humano** en los microcosmos. **PENDIENTE DE IMPLEMENTAR.** | Los habitantes del microcosmos son insectos-humanos. | **solo en docs** | memoria 2026-11-04 |

## 8. Animales en grupo

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| A1 | **Peces, kril y medusas son GRUPOS**: individuos realistas bajo un padre; el depredador caza al padre y cada mordisco quita UN individuo; el grupo se rehace con el tiempo; **sin ciclo de vida individual**. | Se veían como plataformas; además, rendimiento. | `GroupAnimalsTest` (grupo 71) | [grupos-de-animales.md](grupos-de-animales.md) |
| A2 | **Las abejas forman enjambres** que de día vuelan de árbol en árbol y de noche vuelven a la colmena. | Se ven mejor volando en grupo. | 71 (visita ≥ 3 árboles) | [grupos-de-animales.md](grupos-de-animales.md) |
| A3 | **Los grupos marinos entran en los árboles** por el fondo abierto. | El edificio está unido al mar. | 71 | [grupos-de-animales.md](grupos-de-animales.md) |
| A4 | **Especies reales del lugar** (mar de Ross, McMurdo): pez plateado antártico, kril antártico, *Diplulmaris* y *Desmonema*. | Fidelidad ecológica. | 71 (de su especie y su talla) | [grupos-de-animales.md](grupos-de-animales.md) |
| A5 | **Abejas y medusas mates, sin deslumbrar**: sin emisión ni brillo especular (`SwarmModels.Matte`); la abeja de las nieves, de pelusa gris blanquecina. | Brillaban tanto que cegaban a quien miraba (2026-11-04). | **solo en docs** (hecho en código el 2026-11-04, sin prueba propia; se mira en fotos) | [grupos-de-animales.md](grupos-de-animales.md) §2026-11-04 |

## 9. Personajes, jornada y horario libre

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| J1 | **Jornada de 8 h**: trabajo 8–11, comida 11–12, trabajo 12–14, entrenamiento 14–16, libre desde las 16. | El ritmo de vida del santuario. | `JornadaTest` (grupo 50) | [jornada-y-rotacion.md](jornada-y-rotacion.md) |
| J2 | **Reparto en vueltas por áreas con rotación diaria**: a cada uno, la misión que nunca hizo, o la que menos, o la que hace más días que no hace; nadie pasa de 5 h. | Todos aprenden todo y el trabajo se reparte por igual. | `SanctuaryLifeTest` (28: rotación, 5 h, nunca hecha primero), 50 | [jornada-y-rotacion.md](jornada-y-rotacion.md) |
| J3 | **Horario libre semanal** con la misma lógica de reparto: ocio y cuidado en las horas libres, solo en edificios de ocio, recuperación o descanso. | Que el tiempo libre también se reparta por igual. | `HorarioLibreTest` (grupo 65) | [horario-libre.md](horario-libre.md) |
| J4 | **Nadie queda encerrado**: quien deja su puesto en la jornada es devuelto por la Magnate; su puesto es el espacio que pide la tarea. | Disciplina sin muros ni guardias que arrastran. | 28 y 50 | [jornada-y-rotacion.md](jornada-y-rotacion.md) |
| J5 | **Las iniciaciones las da el trabajador de turno que está en el área**; la Magnate solo acompaña. Se hacen «en orquesta»: el trabajador se acerca a ≤ 2 m y cada uno anda solo a su sitio, sin arrastrar ni teletransportar. | Que las enseñe quien conoce el área y que se vean naturales. | `KitchenInitiationTest` (41), `YogaInitiationTest` (42), `GardenInitiationTest` (43) | [reglas-plataformas.md](reglas-plataformas.md) #10, [jornada-y-rotacion.md](jornada-y-rotacion.md) |
| J6 | **Las figuras reales se tratan con respeto**, por su trabajo público, sin caricatura ni juicio político. | Son personas reales. | **solo en docs** | [personajes-mesocosmos.md](personajes-mesocosmos.md) |
| J7 | **Humanos e insectos-humanos conversan** con el sistema del anima, con una escena de prueba propia. **PENDIENTE DE IMPLEMENTAR.** | Que la vida social se vea. | **solo en docs** | memoria 2026-11-04 |

## 10. Pruebas y flujo de trabajo

| # | Regla | Por qué | Se comprueba en | Fuente |
|---|---|---|---|---|
| W1 | **Cada regla tiene su prueba**: si un cambio rompe una regla, la prueba debe fallar antes de que se note jugando. | Las regresiones se repetían (flora que desaparecía, escaleras, muebles amontonados). | este manual (columna «Se comprueba en») | [reglas-plataformas.md](reglas-plataformas.md) |
| W2 | **Todo rediseño: investigación → plan enseñado a Berón → construcción.** | Berón decide antes de que se construya. | **solo en docs** (proceso) | [hospital-plan.md](hospital-plan.md), [restaurant-plan.md](restaurant-plan.md) |
| W3 | **Un solo probador**: los trabajadores en paralelo solo programan y compilan (`verify-wt.sh`); un agente une las ramas, pasa las pruebas en vivo y devuelve cada fallo a su dueño. | El candado de Unity por turnos paraba a todos durante horas. | **solo en docs** (proceso) | memoria «Single tester» (2026-11-03) |
| W4 | **Las pruebas solo corren a petición** (`Temp/run-tests`, `ed.sh test`, «Jugar con las pruebas»); nunca un pase completo con Unity en Play. | El jugador no podía moverse mientras las pruebas mandaban. | `TestRunner` | memoria «Tests on request» |
| W5 | **Reconstruir por partes**: «Build Sample Scene Blockout (solo el santuario, sin microcosmos)» y los microcosmos de uno en uno, descargando y con GC entre ellos; Unity por debajo de ~11 GB. | Reconstruirlo todo a la vez reinició el PC de 16 GB de Berón. | **solo en docs** (`SampleSceneBuilder.SkipSiblingScenes`) | memoria «Memory limit on rebuilds» (2026-11-03) |
| W6 | **Guardar SampleScene tras construir** (el Blockout no la guarda: «save» al buzón del editor). | Se perdía lo construido. | **solo en docs** | memoria «Save SampleScene» |
| W7 | **Toda espera acotada** (sin bucles `until`) y avisar en una línea si algo se cuelga. | Un script esperó 40 minutos a una línea que no llegaba. | **solo en docs** | memoria «No stalls» |
| W8 | **Al terminar una implementación, los docs y [checklist.md](checklist.md) quedan al día**, y este manual si hay regla nueva. | Berón lo exige. | **solo en docs** | memoria 2026-11-03 (segunda tanda) |

## 11. Sincronización mesocosmos ↔ microcosmos

**La regla (Berón 2026-11-04):** cuando un piso del santuario (por ejemplo el hospital) cambia su forma, sus instrumentos, sus
muebles o sus decoraciones, **hay que revisar todos los microcosmos que dependen de ese piso**. Un recordatorio automático
(gancho) lo avisa: desde el 2026-11-04 hay un gancho de Claude Code (PostToolUse en Edit|Write, `.claude/hooks/hospital-reminder.sh` en la carpeta de trabajo de Berón) que, al editar un `.cs` del hospital bajo `Assets/`, recuerda revisar los microcosmos de su fila.

**Por qué:** cada microcosmos es la copia ×100 de su piso (M2). Un piso que cambia y un microcosmos que no se reconstruye dejan
un mundo que ya no corresponde, y la versión micro dedicada del hospital (H8) tiene que llevar los mismos instrumentos y
decoraciones que el meso.

### Qué microcosmos depende de cada piso

Sale de `MicroFloors.Table` (`Assets/Scripts/Microcosmos/MicroFloors.cs`), que es la fuente de verdad: si cambia la tabla, se
cambia esta. «Pendiente» = el piso no representa la historia; se monta en él mientras se crea el piso que falta.

| Piso del santuario (GameObject) | Torre | Microcosmos (escena) | Pendiente de piso propio |
|---|---|---|---|
| **Hospital**, planta de urgencias (`Infirmary_Area`) | Servicios | Ambrosio, nivel 1 (`Microcosmos_Scene1_Ambrosio`) · Hospital (`Microcosmos_Hospital`) · Escútari, Nightingale (`Microcosmos_Nightingale`) · Kapilavastu, Buda (`Microcosmos_Buda`) · Irena Sendler (`Microcosmos_IrenaSendler`) | — |
| **Cocina** del restaurante (`Kitchen_Area`) | Servicios | Cocina (`Microcosmos_Cocina`) · Mesopotamia, Uruk (`MobWorld_Mesopotamia`) · Le Chambon-sur-Lignon (`Microcosmos_LeChambon`) · Angostura, Bolívar (`Microcosmos_Bolivar`) | Angostura → Sala de asambleas (parlamento) |
| **Sala de Yoga** (`YogaRoom_Area`) | Entrenamiento | Sakshi, el río, y Sala de Yoga (`Microcosmos_Scene0_SakshiRio`) · Patanjali (`Microcosmos_Patanjali`) · Sils Maria, Nietzsche (`Microcosmos_Nietzsche`) | Sils Maria → Sendero de montaña y biblioteca de filosofía |
| **Gimnasio** (`Gym_Area`) | Entrenamiento | Tregua 1914 (`Microcosmos_Tregua1914`) · Crotona, Pitágoras (`Microcosmos_Pitagoras`) | Tregua → Campo de deportes de equipo; Crotona → Aula de matemáticas y música |
| **Dojo** (`MartialArts_Area`, en obras: su «Acabado») | Entrenamiento | Bruce Lee (`Microcosmos_BruceLee`) | — |
| **Planta Atómica** (`AtomicPlant_Area`) | Ciencias | Planta Atómica (`Microcosmos_PlantaAtomica`) · Woolsthorpe, Newton (`Microcosmos_Newton`) · Berna, Einstein (`Microcosmos_Einstein`) · Alejandría, Eratóstenes (`Microcosmos_Eratostenes`) | Alejandría → Observatorio y biblioteca |
| **Química** (`Chemistry_Area`) | Ciencias | París 1898, Curie (`Microcosmos_Curie`) · Pasadena, Patterson (`Microcosmos_Patterson`) | — |
| **Biología** (`Biology_Area`) | Ciencias | Berkeley, Jenner (`Microcosmos_Jenner`) | — |
| **Construcción** (`Construction_Area`) | Producción | Egina (`Microcosmos_Egina`) · Bagdad, al-Juarismi (`Microcosmos_AlJuarismi`) · Colorado Springs, Tesla (`Microcosmos_Tesla`) | Bagdad → Casa de la Sabiduría; Colorado Springs → Taller de electricidad |
| **Mecánica** (`VehicleWorkshop_Area`) | Producción | Mecánica (`Microcosmos_Mecanica`) · Dayton, Wright (`Microcosmos_Wright`) | — |
| **Estudio Textil** (`TextileStudio_Area`) | Producción | Estudio Textil (`Microcosmos_Textil`) | — |
| **Huerto** (`Garden_Area`, piso 5 del Árbol de los Campos) | Árbol de los Campos | Huerto (`Microcosmos_Huerto`) · Menfis, Heródoto (`Microcosmos_Herodoto`) · Palenque, Pakal (`Microcosmos_Pakal`) | Menfis → Archivo de historia y geografía; Palenque → Templo de los antepasados |
| **Central Nuclear** (`NuclearPower_Area`) | aparte | Chernóbil (`Microcosmos_Chernobil`) | — |

Hoy **no tienen microcosmos** (cambiarlos no obliga a revisar ninguno): las otras plantas del hospital (hospitalización y
veterinaria), el comedor del restaurante, la Cría, la Carne cultivada, Cardio, Calistenia, Ballet, el resto de plantas de
Ciencias y los campos del Árbol de los Campos.

**Ojo:** el microcosmos copia el GameObject del área con sus hijos. Además de los constructores de cada área
(`SampleSceneBuilder.Hospital.cs` e `InfirmaryProps` para el hospital; `Restaurant.cs`, `KitchenPlan.cs`, `ProKitchen.cs` y
`KitchenProps` para la cocina; `AtomicPlant.cs`; `Research.cs` para física, química y biología; `Construction.cs`; `Obras.cs` para
el Dojo…), tocan **todos** los pisos los cambios en `SampleSceneBuilder.Towers.cs`, `Helix.cs`, `Levels.cs`, `Slots.cs`
(plazas reservadas) y `FloorForms.cs`.

### Qué hacer cuando cambia un piso

1. Buscar el piso en la tabla de arriba (o en `MicroFloors.Table` por su `platform`).
2. Reconstruir cada microcosmos que depende de él, **de uno en uno** (W5).
3. Pasar `MicrocosmosFloorTest` (43) y, si los hay, la prueba de su historia (`BruceLeeStoryTest` 60, `PakalStoryTest` 66,
   `KitchenOriginStoryTest` 44) y las de los pueblos (`CivilizedErasTest` 62, `MicroBuildRulesTest` 63).
4. Mirar que las plazas reservadas (S10) siguen cabiendo y que la historia cabe dentro de los muros (M5).
5. Si el piso tiene versión micro dedicada (el hospital, H8, cuando exista), añadirle lo que cambió en el meso.
6. Apuntarlo en [checklist.md](checklist.md).

**El gancho (2026-11-04):** un gancho de Claude Code (`.claude/hooks/hospital-reminder.sh`, PostToolUse en Edit|Write) avisa al editar un `.cs` de `Assets/`:
- del **hospital** → revisar los microcosmos que dependen de su piso;
- de un **piso, área o edificio-árbol** del santuario → revisar que las **jornadas y el reparto de trabajo** (WorkdayDirector, misiones de área, FruitGardener) y el **horario libre** (LeisureRoster) cuadran con los puestos, tiempos de viaje y capacidades nuevos, y los microcosmos de ese piso;
- de un **microcosmos** → revisar las jornadas de sus insectos-humanos (MicroJornada), el reparto de puestos y el horario libre frente a los edificios de trabajo, ocio y recuperación que cambiaron.

**Regla W9 (Berón, 2026-11-04):** todo cambio en un piso, área, edificio-árbol o microcosmos obliga a revisar y reajustar las jornadas, el reparto de trabajo y el horario libre de los personajes que lo usan. Se comprueba con el gancho y con las pruebas 50 (jornada), 65 (horario libre) y 62 (eras civilizadas).

## 12. Recuento y reglas sin comprobación automática

**Total: 110 reglas** (S 12 · F 8 · T 12 · C 7 · G 8 · H 8 · M 34 · A 5 · J 7 · W 8 = 109, más la regla de sincronización
de §11).

**Pendientes de implementar (pedidos del 2026-11-04), 14:** F2, F3, F7, F8, C7, H8, M9, M13, M30, M32 (como regla general),
M33, M34, J7 y el gancho de §11 (la regla de sincronización en sí ya vale como procedimiento manual).

**Solo en docs (sin validador ni prueba), además de las pendientes:** S8 (medida humana), T12 (anillo en U), C4 (selector por
áreas), A5 (abejas mates), H1 (un solo hospital, mismas plazas), H2 (8 puestos; está en el código pero sin prueba), H6 (anillo de camas por
planta), M10 (luz por fecha y lugar), M15 (parte de la fauna salvaje según la época), M22 (tope de 150 edificios), M31 (relieve
realista), J6 (respeto a las figuras reales) y las de proceso W2, W3, W5, W6, W7 y W8.

## Docs de cada tema

[reglas-plataformas.md](reglas-plataformas.md) · [construcciones-elevadas.md](construcciones-elevadas.md) ·
[plano-edificios.md](plano-edificios.md) · [edificio-arbol.md](edificio-arbol.md) · [mascaras-de-oxigeno.md](mascaras-de-oxigeno.md) ·
[submarino-de-obra.md](submarino-de-obra.md) · [alfombras-plan.md](alfombras-plan.md) (sustituye a
[transporte-alfombras.md](transporte-alfombras.md), que tenía puentes) · [jardines-frutales.md](jardines-frutales.md) ·
[hospital.md](hospital.md) · [hospital-plan.md](hospital-plan.md) · [vet-hospital-plan.md](vet-hospital-plan.md) ·
[submarinos-medicos.md](submarinos-medicos.md) · [medidas-muebles.md](medidas-muebles.md) · [microcosmos-areas.md](microcosmos-areas.md) ·
[microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) · [microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md) ·
[microcosmos-actividades.md](microcosmos-actividades.md) · [luz-microcosmos.md](luz-microcosmos.md) ·
[grupos-de-animales.md](grupos-de-animales.md) · [jornada-y-rotacion.md](jornada-y-rotacion.md) · [horario-libre.md](horario-libre.md) ·
[personajes-mesocosmos.md](personajes-mesocosmos.md) · [conventions.md](conventions.md) · [checklist.md](checklist.md)
