# Escenas de prueba

Berón 2026-11-01: «las escenas de prueba de yoga parece que cargan un santuario entero, deberían cargar solo la sala de yoga;
enfermería y cocina de igual manera deben cargar solo su piso respectivo y deberían estar vinculadas a los pisos actuales de
sanctuary, parece que cargan el santuario antiguo».

## Qué pasaba

- Las escenas de `Assets/Scenes/Pruebas` eran **copias de SampleScene + `BuildAreaStructures` entero** (todas las torres, el
  Árbol de los Campos, la central…) más el sandbox de cada una. Cargaban el santuario completo aunque la prueba fuera de una sala.
- Se generaban **solo a mano** («Build Test Scenes») y nadie lo había vuelto a lanzar desde el **21-10**: en disco estaban las
  del santuario de antes de las torres-árbol (sin hélice, sin pisos en el tronco). Por eso «parecen el santuario antiguo».
- La lista y los archivos no coincidían: `Prueba_Enfermeria` (en Build Settings, ya no la generaba nadie: el código la llamaba
  `Prueba_Hospital`), `Yoga_UpaCuello/Namaskar/SaludoSol` en Build Settings sin archivo, `Prueba_Restaurante` en el código.
- Los sandboxes (mente, posesión, magia, alma, fauna…) **no usan ningún área** (van en el suelo junto al hub, en coordenadas
  fijas) y aun así cargaban todas las torres.

## Cómo es ahora (`Assets/Editor/SampleSceneBuilder.TestScenes.cs`)

Una sola tabla (`TestScenes`) y un solo constructor parametrizado. Dos clases de escena:

- **De área** (`keep` = el piso): se **copia el SampleScene recién construido y se poda**. Se queda el piso (el objeto del área:
  losa, sala, anillo, muebles) y **todo lo que está físicamente en ese piso** aunque cuelgue de otro objeto (la cocina real
  `Restaurante_AUTO/Sala`, el comedor del anillo, los puntos `SalaYoga`/`CocinaSpot`…), los personajes que pide la prueba (los
  que estaban en otro sitio se traen al piso, a puntos andables), Kushal, el suelo, la luz, la cámara y los sistemas sin cuerpo
  (reloj, UI, economía, el orquestador de pruebas) y los casquetes de hielo del suelo, debajo. Todo lo demás (otras torres, otros
  pisos, fauna y lo que la puebla al arrancar —familias, enjambres, colmenas—, flora, mar) fuera, y también la **jornada**
  (`WorkdayDirector`): al arrancar coloca a cada uno en el área de su primer turno, que en una escena de un piso no existe.
  Las listas que apuntaban a lo podado se compactan. Se hornea el NavMesh del piso.
  **Se regeneran solas al final de «Build Sample Scene Blockout»** (`AlsoAreaTestScenes`): son el santuario actual, no pueden
  desfasarse. Menú suelto: «Build Test Scenes de área (yoga, enfermería, cocina)».
- **De sandbox** (sin `keep`): suelo + sistemas + el sandbox, **sin torres**. Menú: «Build Test Scenes (una escena por prueba)»
  (las construye todas, también las de área).

En cada escena, «test» (o «Jugar con las pruebas») corre **solo los grupos de su área**: `TestRunner.onlyGroups` (las pruebas que
se registran solas al pedir las pruebas —gimnasio, jornada, árboles…— no corren ahí). Sin grupos, la escena no lleva orquestador.

Desde Inicio se abren como siempre (grupo «Pruebas»), con «Kushal iniciado» o sin él (sin él, en la enfermería Linduarda lo
analiza y lo vacuna).

## Tabla

| Escena | Qué se prueba | Qué carga | Grupos de «test» | Constructor |
|---|---|---|---|---|
| `Prueba_Yoga` | La clase de yoga (Sadhguru en la tarima, cada practicante en su esterilla, repiten los pasos), la iniciación de upa-yoga de cuello (cabeza y cámara) y el minijuego de upa-yoga por teclas | Solo el piso **Sala de Yoga** de la torre de Entrenamiento (esterillas, tarima, anillo de estatuas, aseos) + Sadhguru, Panterilia, Gohageneis, Irosene, Goluis, Lizmibet, la Magnate + `UpaYogaSession` | 15 (yoga: postura en el rig, Saludo al Sol), 42 (iniciación de yoga), 51 (clase de yoga) | área: `keep YogaRoom_Area` + `BuildUpaYogaSandbox` |
| `Prueba_YogaSesiones` | Cada sesión de yoga en bucle para revisar sus posturas: Upa-yoga de cuello, Yoga Namaskar, Saludo al Sol, Ashtanga primaria | Solo el piso **Sala de Yoga** + un yogui por sesión, cada uno en su esterilla | — | área: `keep YogaRoom_Area` + `BuildYogaSessionMats` (sustituye a `Yoga_*`) |
| `Prueba_Enfermeria` | Enfermería: Linduarda y Liz analizan y vacunan al recién llegado (Kushal sin iniciar) y al lobo paciente; la puerta vigilada | Solo la **planta de urgencias del hospital** (torre de Servicios) con su anillo de observación y maternidad; sin hospitalización ni veterinaria + Linduarda, Lizmibet, el lobo paciente, la Magnate | — (no hay prueba automática de la enfermería sola) | área: `keep Infirmary_Area`, `drop Planta_hospitalizacion, Planta_veterinaria` |
| `Prueba_Cocina` | Cocina y restaurante: cocinar para el stock (E/`step`) y la iniciación de la cocina (platillo con menos stock, por mayor → tanque) | Solo el piso del **restaurante** (torre de Servicios): la cocina abierta de 8 puestos, el obrador, el comedor del anillo + el personal (Goluis, Andrea, Linduarda, Liz, Panterilia, Gohageneis, Irosene, Tote, Sadhguru) y la Magnate | 41 (iniciación de la cocina). El servicio con turistas (45) no: los turistas llegan al pie de la torre, suben en alfombra y nadan al anillo, así que corre en SampleScene | área: `keep Kitchen_Area` (sustituye a `Prueba_Restaurante`) |
| `Prueba_CocinaPrototipos` | Los prototipos de la cocina: manchas y limpieza (paso A), paseo guiado y desayuno (paso B), motor de virtualización (puntero, partes, receta, teclear) y la descomposición | Suelo + sistemas + los 4 prototipos (en el hielo, junto al hub) | — | sandbox (antes dentro de `Prueba_Cocina`) |
| `Prueba_TorreGuardia` | El árbol de la guardia (tablero de misiones de vigía) y los puestos de patrulla | Suelo + sistemas + el árbol de la guardia y los 4 puestos | — | sandbox `BuildGuardTower` |
| `Prueba_Farming` | Farming no violento: jugar baja la tensión → recursos, XP, comer | Suelo + sistemas + criaturas de farming | — | sandbox |
| `Prueba_Mente` | Pensamientos por tono, frases, campo de pensamiento | Suelo + sistemas + el sandbox de mente | — | sandbox |
| `Prueba_Posesion` | Ceder el mando a otro cuerpo y volver | Suelo + sistemas + cuerpos poseíbles | — | sandbox |
| `Prueba_Oficios` | Huerto y forja virtualizados, esfuerzo, mecánica, camión, construcción, reparto de reparaciones, cría | Suelo + sistemas + los 8 sandboxes de oficios | — | sandbox |
| `Prueba_Prologo` | Guion por beats, avisos entre planos, llevar a los débiles al refugio | Suelo + sistemas + el prólogo | — | sandbox |
| `Prueba_CamaraEmocion` | Tintes de pantalla por estado y la orquesta emocional | Suelo + sistemas + los dos sandboxes | — | sandbox |
| `Prueba_Magia` | Reservas, fuego con coste físico, demo de hechizos, walkTo universal | Suelo + sistemas + los tres sandboxes | — | sandbox |
| `Prueba_Alma` | Mezclas de alma y alma compartida | Suelo + sistemas + los dos sandboxes | — | sandbox |
| `Prueba_Fauna` | Familias y ciclo de vida, interacciones, escenarios por especie | Suelo + sistemas + la fauna de prueba | — | sandbox |

### Retiradas (se borran del disco y de Build Settings al construir)

| Escena | Por qué |
|---|---|
| `Prueba_Hospital` | Era la enfermería con el santuario entero: ahora `Prueba_Enfermeria` (solo urgencias). |
| `Prueba_Restaurante` | Fusionada en `Prueba_Cocina` (la cocina real es el restaurante). |
| `Yoga_UpaCuello`, `Yoga_Namaskar`, `Yoga_SaludoSol`, `Yoga_Ashtanga1` | Un yogui en el hub con el santuario entero, una escena por sesión: ahora `Prueba_YogaSesiones`, las cuatro en la sala de yoga, una por esterilla. |
| `Microcosmos_SalaYoga` | (Ya retirado: lo sustituyó el río de Sakshi.) El constructor lo vuelve a sacar de Build Settings si reaparece. |

## Las demás escenas de Inicio (no son de `Pruebas`)

| Grupo / escenas | Qué son | Qué cargan | Constructor |
|---|---|---|---|
| Santuario: `SampleScene` | El juego | Todo el santuario | «Build Sample Scene Blockout» |
| Ciclo de vida: `CicloVida_*` | Una isla por especie: dos adultos de distinto sexo y su comida | Su isla y su explorador | `SampleSceneBuilder.CicloVida.cs` (grupo 58) |
| Microcosmos de área: `Microcosmos_Cocina`, `_Hospital`, `_Huerto`, `_Textil`, `_Mecanica` | El microcosmos de cada área (el piso ×10, a escala insecto) al que se entra por su tanque: no son pruebas de la sala | El microcosmos **sobre el Santuario** (se entra desde el tanque, con Kushal) | `MicrocosmosAreaSceneBuilder` (se regeneran con el blockout) |
| Microcosmos de historias: `Microcosmos_Scene0_SakshiRio`, `_Scene1_Ambrosio`, `_Patanjali`, `_Buda`, `_BruceLee`, `_Nightingale`, `_IrenaSendler`, `_LeChambon`, `_Tregua1914`, `_Wright`, `_Eratostenes`, `_Egina`, `_PlantaAtomica`, `MobWorld_Mesopotamia` | Niveles del microcosmos | Sobre el Santuario | `MicrocosmosSceneBuilder`, `StorySceneBuilder`, `MythSceneBuilder`, `PhysicsSceneBuilder`, `MobWorldSceneBuilder` |
| Virtual: `Virtual_*` | Las misiones de virtualización de cada área (una por misión) | Sobre el Santuario (se entra por el tanque del área) | `VirtualMissionSceneBuilder` |
| Otros: `Sala_de_Eras`, `Galeria_InsectosHumanos` | Validación (portales por era, galería de insectos humanos) | Su sala y su explorador | `EraHubSceneBuilder`, `InsectGallerySceneBuilder` |
| Retiradas: `Microcosmos_Enfermeria`, `Microcosmos_Veterinaria` | De antes del hospital | (no se listan) | — |

## Añadir una prueba de área

Una entrada en `TestScenes`: `keep` (el objeto del piso en el santuario), `drop` (plantas hijas que no van), `people` (nombres de
los personajes), `groups` (los `ITestUnit.Group` que corren con «test») y, si hace falta, `build` (un sandbox que se coloca
sobre el piso). Nada de coordenadas: el piso está donde lo pone el santuario.
