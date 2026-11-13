# Catálogo de casos

<!-- GENERADO por docs/qa/migracion/generar.pl el 2026-11-13 desde docs/auditoria-peticiones.md, docs/manual-de-diseno.md y los tests. Desde aquí se mantiene a mano. -->

IDs estables: **no se renumeran ni se borran** (un caso que ya no vale se tacha y pasa a `Retirado` en el registro). Formato del ID: 2–3 letras de área + 2 cifras.
El estado de cada caso está en [registro.md](registro.md). «Unidad» y «Grupo» son la prueba del `TestRunner` que lo comprueba (`int Group =>`); «—» si aún no tiene.
En la prueba, el ID va al principio del texto del `TestProbe.Check`: `"[CV-03] …"`.

## FAU · Fauna

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| FAU-01 | FAU | Relieve realista (montes, lagos, acantilados) | TerritorioTest | 99 | — | PB-030 | auditoría #30, L277, L279 |
| FAU-02 | FAU | Árboles sin suelo, para bajar nadando al mar y que entre la fauna marina | — | — | — | PB-041 | auditoría #41, L166 |
| FAU-03 | FAU | Peces, kril, medusas y abejas como GRUPOS | GroupAnimalsTest | 71 | — | PB-043 | auditoría #43, L166 |
| FAU-04 | FAU | Morsa y pingüino emperador | — | — | — | PB-095 | auditoría #95, L790, L808 |
| FAU-05 | FAU | Casquetes: cuevas, mezcla 50/50 con isla, renos/lobos/conejos, flora de la isla, reubicar edificios | TerritorioTest | 99 | — | PB-097 | auditoría #97, L793, L806, L808, L89, L810 |
| FAU-06 | FAU | Regla de la cadena alimenticia; especie real para peces y medusas | CadenaAlimenticiaTest | 85 | — | PB-103 | auditoría #103, L805, L808 |
| FAU-07 | FAU | Deshierbe cuidadoso | DeshierbeTest | 102 | — | PB-109 | auditoría #109, L796, L808 |
| FAU-08 | FAU | Fuera el comedero del oso; alimentador por rondas con lista compartida | AlimentadorTest | 105 | — | PB-114 | auditoría #114, L795, L797, L808, L809 |
| FAU-09 | FAU | Ningún depredador con hambre; la lista se renueva solo cuando está completa | CadenaAlimenticiaTest | 85 | — | PB-122 | auditoría #122, L797, también g105 |
| FAU-10 | FAU | Lanzadera protegida de icebergs; fauna bajo el hielo y respiraderos | HieloMarinoTest | 107 | — | PB-124 | auditoría #124, L173, L799, L172, también g90 |
| FAU-11 | FAU | Icebergs con animales reales y sus motivos | IcebergVidaTest | 108 | — | PB-129 | auditoría #129, L801 |
| FAU-12 | FAU | Fauna fría nativa en los santuarios 2 y 3 | FaunaFriaTest | 111 | — | PB-132 | auditoría #132, L803, L18 |
| FAU-13 | FAU | Para subir hay que superar al animal más poderoso; la magia como camino | — | — | — | PB-141 | auditoría #141 |
| FAU-14 | FAU | Sin insectos en el santuario 1 por ahora | — | — | — | PB-153 | auditoría #153 |
| FAU-15 | FAU | Regla A1: Peces, kril y medusas son GRUPOS: individuos realistas bajo un padre; el depredador caza al padre y cada mordisco quita UN individuo; el grupo se rehace con el tiempo; sin ciclo de vida individual. | GroupAnimalsTest | 71 | A1 | — | manual §«Se comprueba en» |
| FAU-16 | FAU | Regla A2: Las abejas forman enjambres que de día vuelan de árbol en árbol y de noche vuelven a la colmena. | GroupAnimalsTest | 71 | A2 | — | manual §«Se comprueba en», también g3 |
| FAU-17 | FAU | Regla A3: Los grupos marinos entran en los árboles por el fondo abierto. | GroupAnimalsTest | 71 | A3 | — | manual §«Se comprueba en» |
| FAU-18 | FAU | Regla A4: Especies reales del lugar (mar de Ross, McMurdo): pez plateado antártico, kril antártico, *Diplulmaris* y *Desmonema*. | GroupAnimalsTest | 71 | A4 | — | manual §«Se comprueba en» |
| FAU-19 | FAU | Regla A5: Abejas y medusas mates, sin deslumbrar ni verse negras: sin brillo especular ni reflejos y con una luz propia suave, muy por debajo del umbral del posprocesado (`SwarmModels.Matte`, `SoftGlow`; ronda 7: a contraluz salía… | ObservationTest | 11 | A5 | — | manual §«Se comprueba en» |
| FAU-20 | FAU | Regla E2: La especie real manda: si el catálogo mezcla dos, se separan. | TerritorioTest | 99 | E2 | — | manual §«Se comprueba en» |
| FAU-21 | FAU | Regla E3: Todas las tablas, misma clave: faltar en una cae en silencio a un valor por defecto. | TerritorioTest | 99 | E3 | — | manual §«Se comprueba en» |
| FAU-22 | FAU | Regla E5: Apariencia: cubos a tamaño real, con los rasgos que la identifican. | TerritorioTest | 99 | E5 | — | manual §«Se comprueba en» |
| FAU-23 | FAU | Regla E6: Medidas reales respecto al humano; cría 40 %, joven 70 %. | TerritorioTest | 99 | E6 | — | manual §«Se comprueba en» |
| FAU-24 | FAU | Regla E7: Nombres sin subcadenas traicioneras («ant», «bee», «seal»); lo específico antes que lo general. | TerritorioTest | 99 | E7 | — | manual §«Se comprueba en» |
| FAU-25 | FAU | Regla E8: Ciclo de vida completo en unidades reales; el parto es 1 cría. | CicloVidaTest | 58 | E8 | — | manual §«Se comprueba en» |
| FAU-26 | FAU | Regla E9: Dieta real: lo que no sea presa, pez o kril va en el `Ethogram` sobre un `Affordance`. | TerritorioTest | 99 | E9 | — | manual §«Se comprueba en» |
| FAU-27 | FAU | Regla E12: Escena de ciclo de vida para cada especie. | CicloVidaTest | 58 | E12 | — | manual §«Se comprueba en» |
| FAU-28 | FAU | Regla E14: Mente propia: `Archetypes` y `PhrasePools`. | TerritorioTest | 99 | E14 | — | manual §«Se comprueba en» |
| FAU-29 | FAU | Regla E16: Sin comederos fijos para depredadores: los atiende el alimentador por rotación, con lista compartida, a distancia segura y sin compartir espacio. | AlimentadorTest | 105 | E16 | — | manual §«Se comprueba en» |
| FAU-30 | FAU | Regla E17: Deshierbe con raíz: lo nativo al vivero y a la tundra; lo no nativo a bolsa cerrada e incinerador, nunca al suelo. | DeshierbeTest | 102 | E17 | — | manual §«Se comprueba en» |
| FAU-31 | FAU | Regla E10: hábitat real: vive donde vive; si hace falta, el mundo se adapta | TerritorioTest | 99 | E10 | — | manual §«Se comprueba en» (también fotos); añadido a mano el 2026-11-13 |
| FAU-32 | FAU | Regla E15: cadena alimenticia: FoodChain lee dietas y poblaciones y sube bancos, plantas o presas para que nadie pase hambre | CadenaAlimenticiaTest | 85 | E15 | — | manual §«Se comprueba en» (también el registro [Cadena]); añadido a mano el 2026-11-13 |

## JOR · Jornada y horario libre

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| JOR-01 | JOR | Las iniciaciones las da el trabajador de turno (la Magnate solo acompaña) | — | — | — | PB-011 | auditoría #11, L176, L97 |
| JOR-02 | JOR | Iniciaciones como «orquesta» (≤ 2 m, cada uno anda solo, sin tirones ni teletransportes) | — | — | — | PB-013 | auditoría #13 |
| JOR-03 | JOR | Cada estructura con su interacción (trabajo/descanso/recuperación/ocio) y en las actividades semanales | — | — | — | PB-033 | auditoría #33, L253, L713, L274 |
| JOR-04 | JOR | Horario libre semanal con el reparto rotatorio | HorarioLibreTest | 65 | — | PB-034 | auditoría #34, L711, L716, L719 |
| JOR-05 | JOR | Una cama por casa: litera de 2 × 4 con tobogán | — | — | — | PB-065 | auditoría #65, L738, L746, L747 |
| JOR-06 | JOR | Ring de guerra de almohadas para todos los residentes | PillowFightTest | 89 | — | PB-066 | auditoría #66, L739, L742, L743 |
| JOR-07 | JOR | Pijamadas con literas | SleepoverTest | 92 | — | PB-073 | auditoría #73, L746, L747 |
| JOR-08 | JOR | La comida, orquestada (utensilios, bocados) | ActividadesTest | 97 | — | PB-076 | auditoría #76, L139, L143 |
| JOR-09 | JOR | Toda actividad se HACE en su sitio (humanos e insectos-humanos) | ActividadesTest | 97 | — | PB-077 | auditoría #77, L140, L143, L144 |
| JOR-10 | JOR | Uniformes por tarea | ActividadesTest | 97 | — | PB-080 | auditoría #80, L141, L143 |
| JOR-11 | JOR | Guardería o juego para Neo (con Ivelyna) | — | — | — | PB-084 | auditoría #84, L761 |
| JOR-12 | JOR | Las tareas tienen requisitos | JornadaSaberTest | 104 | — | PB-115 | auditoría #115, L96, L97 |
| JOR-13 | JOR | 1.ª hora de entrenamiento y 2.ª opcional → 2 h al día | JornadaTest | 50 | — | PB-116 | auditoría #116, L96, L97 |
| JOR-14 | JOR | Iniciación como tarea; primero quien menos sabe; nadie tiene hora fija de clase | RepartoSantuariosTest | 113 | — | PB-126 | auditoría #126, L96, L97, L98, también g104 |
| JOR-15 | JOR | Casa en santuarioN nivelM; solo hacia atrás; reparto diario por sitios | RepartoSantuariosTest | 113 | — | PB-139 | auditoría #139, L15, L16, L17 |
| JOR-16 | JOR | Horas a partes iguales, clones, nada a mañana, áreas de varios pisos cuentan como una | RepartoSantuariosTest | 113 | — | PB-143 | auditoría #143, L15, L16 |
| JOR-17 | JOR | Varias tareas a la vez en un área; las opcionales se saltan si nadie está libre | RepartoSantuariosTest | 113 | — | PB-148 | auditoría #148 |
| JOR-18 | JOR | Fauna = K × humanos (K = 3), por la pirámide trófica | RepartoSantuariosTest | 113 | E11 | PB-151 | auditoría #151 |
| JOR-19 | JOR | Regla J1: Jornada de 8 h (cambiada el 2026-11-05, J32): entrenamiento 8–9, tareas 9–11, comida 11–12, tareas 12–14, entrenamiento 14–15, tareas 15–16, libre desde las 16. 5 h de tareas y 2 h de entrenamiento. | JornadaTest | 50 | J1 | — | manual §«Se comprueba en» |
| JOR-20 | JOR | Regla J2: Reparto en vueltas por áreas con rotación diaria: a cada uno, la misión que nunca hizo, o la que menos, o la que hace más días que no hace; nadie pasa de 5 h. Con el reparto por santuarios, cada tarea a quien menos lleva… | SanctuaryLifeTest | 28 | J2 | — | manual §«Se comprueba en», también g113 |
| JOR-21 | JOR | Regla J3: Horario libre semanal con la misma lógica de reparto: ocio y cuidado en las horas libres, solo en edificios de ocio, recuperación o descanso. | HorarioLibreTest | 65 | J3 | — | manual §«Se comprueba en» |
| JOR-22 | JOR | Regla J4: Nadie queda encerrado: quien deja su puesto en la jornada es devuelto por la Magnate; su puesto es el espacio que pide la tarea. | SanctuaryLifeTest | 28 | J4 | — | manual §«Se comprueba en», también g50 |
| JOR-23 | JOR | Regla J5: Las iniciaciones de llegada a un área las da el trabajador de turno que está en ella; la Magnate solo acompaña (las tareas siguen el mismo principio: J32, las da quien sabe y ella es el último recurso). Se hacen «en orqu… | KitchenInitiationTest | 41 | J5 | — | manual §«Se comprueba en», también g42, g46 |
| JOR-24 | JOR | Regla J6: Las figuras reales se tratan con respeto, por su trabajo público, sin caricatura ni juicio político. Los famosos viven en su microcosmos de historia (no en el personal del santuario): una escena de su vida pública, docum… | FamososStoryTest | 69 | J6 | — | manual §«Se comprueba en», también g86, g79, g78 |
| JOR-25 | JOR | Regla J7: Humanos e insectos-humanos conversan (humano↔humano, insecto↔insecto, humano↔insecto) cuando comparten escena y están a ≤ 2,6 estaturas; surge por cercanía, vínculo, actividad compartida (puesto, comida, franja libre) y … | ConversationTest | 88 | J7 | — | manual §«Se comprueba en» |
| JOR-26 | JOR | Regla J8: Una charla no manda: no mueve a nadie ni cambia lo que hace (capa encima de jornada, horario libre y necesidades); una necesidad urgente (≥ 0,85), dormir, una iniciación o la historia la cortan; en turno se habla menos. | ConversationTest | 88 | J8 | — | manual §«Se comprueba en» |
| JOR-27 | JOR | Regla J9: Se turnan y se ve: A, B, A, B; 4–6 frases cortas, una por minuto de juego, en un bocadillo con el tema sobre quien habla. | ConversationTest | 88 | J9 | — | manual §«Se comprueba en» |
| JOR-28 | JOR | Regla J10: Los temas salen del contexto (trabajo, comida, ocio, familia, linaje de insecto, era y lugar, historia del sitio, necesidades, la mente), con plantillas cortas × datos de los dos; nunca un árbol de diálogo escrito entero… | ConversationTest | 88 | J10 | — | manual §«Se comprueba en» |
| JOR-29 | JOR | Regla J11: Toda charla deja huella (vínculo, estrés y humores, memoria) y su coste está acotado: ≤ 4 a la vez por escena, ≤ 2 nuevas por vuelta, búsqueda cada 2 s, esperas entre charlas, tope de 20 min de juego. | ConversationTest | 88 | J11 | — | manual §«Se comprueba en» |
| JOR-30 | JOR | Regla J12: Los famosos viven en su microcosmos, no en el santuario (Berón 2026-11-04): Sadhguru, Elon Musk, Goku, Bill Gates, Warren Buffett, MacKenzie Scott, Ibrahim Traoré y los que vengan. El personal del santuario es la gente d… | HorarioLibreTest | 65 | J12 | — | manual §«Se comprueba en», también g28 |
| JOR-31 | JOR | Regla J14: Una pareja comparte casa del árbol (`Bedroom.partner`), y sus hijos viven con ella (`Bedroom.kids`; 2026-11-04: Neo, con Ducristin y Gallarle): Lizmibet y Tote, Amykhuz y Karen, Ducristin y Gallarle, Josedrilo y Maite, A… | HorarioLibreTest | 65 | J14 | — | manual §«Se comprueba en» |
| JOR-32 | JOR | Regla J15: El ring de almohadas cabe a todos los residentes: colchón de n × n celdas de 2,5 m con n = ⌈√residentes⌉, borde blando, almohadones contra el cabecero, 2 almohadas sueltas por luchador, 2 m de paso y su pista de alfombra… | PillowFightTest | 89 | J15 | — | manual §«Se comprueba en» |
| JOR-33 | JOR | Regla J16: Los centros de entrenamiento y de ocio dan servicio a todos los miembros a la vez; si los pisos de un área no llegan, se añade otro piso de la misma área. Es la MISMA regla que S13: un solo validador, `AreaCapacity` (`Ch… | CapacidadPisosTest | 75 | J16 | — | manual §«Se comprueba en», también g89 |
| JOR-34 | JOR | Regla J17: Pijamadas en las habitaciones: cada habitación duerme a 8 en UNA sola cama-litera de dos pisos de 4 (sin escalera: se sube trepando por su poste y se baja por su tobogán; los que viven allí, abajo; los invitados, primero… | SleepoverTest | 92 | J17 | — | manual §«Se comprueba en» |
| JOR-35 | JOR | Regla J18: Comer es una orquesta: bandeja (o plato de la despensa) → sentarse con el plato y el vaso en la mesa → cubiertos en la mano → bocados con el brazo a la boca, la comida del plato que mengua y la saciedad (y los nutrientes… | ActividadesTest | 97 | J18 | — | manual §«Se comprueba en» |
| JOR-36 | JOR | Regla J19: Nadie llega y se queda quieto: en su puesto, cada misión de la jornada, actividad libre o entrenamiento se HACE (postura, herramienta en la mano, gesto y efecto visible) mientras dura; al irse, la deja y suelta la herram… | ActividadesTest | 97 | J19 | — | manual §«Se comprueba en» |
| JOR-37 | JOR | Regla J20: Uniforme por tarea, identidad intacta: al llegar a su tarea se pone el uniforme de esa tarea y al acabar vuelve a su ropa; el pelo, la cara y la piel no cambian; nadando, neopreno. | ActividadesTest | 97 | J20 | — | manual §«Se comprueba en» |
| JOR-38 | JOR | Regla J21: El insecto-humano trabaja con lo de su era: su actuación, sus herramientas y su delantal son de la época de su microcosmos (sin tabletas antes de 1900, sin jeringa antes de 1850). | ActividadesTest | 97 | J21 | — | manual §«Se comprueba en» |
| JOR-39 | JOR | Regla J22: Las tareas y las iniciaciones del jugador se hacen con «E» en los sitios marcados (el modelo de Ambrosio): cada misión de su jornada es una secuencia de pasos con marcador «E — acción» en el mueble de su puesto, aviso en… | PlayerTasksTest | 96 | J22 | — | manual §«Se comprueba en», también g50, g3 |
| JOR-40 | JOR | Regla J23: Entrenamientos reales en el gimnasio: cada humano del personal tiene un plan semanal (reparto —cuerpo entero, torso/pierna, empuje/tirón/pierna o Weider—, días y objetivo —fuerza, hipertrofia, resistencia o salud— según … | GymTrainingTest | 87 | J23 | — | manual §«Se comprueba en» |
| JOR-41 | JOR | Regla J24: Una persona por máquina: si todas las copias están ocupadas, se cambia por una alternativa del mismo músculo; si no hay, espera (acotado, 8 s) y la deja para el final; a la segunda, la salta. | GymTrainingTest | 87 | J24 | — | manual §«Se comprueba en» |
| JOR-42 | JOR | Regla J25: Progresión semanal: progresión doble (más repeticiones dentro del rango; arriba del rango, más carga y vuelta abajo), +2,5 kg tren superior y +5 kg inferior (la mitad en «salud»); descarga cada 4.ª semana (mitad de serie… | GymTrainingTest | 87 | J25 | — | manual §«Se comprueba en» |
| JOR-43 | JOR | Regla J26: El entrenador del gimnasio es Andrea, el de más fuerza del personal (Berón 2026-11-04: «el personaje con más fuerza es Andrea»; antes, unas horas, Goluis). No da a la vez la clase de yoga de las 14 h (el yoga pasa a Jose… | GymTrainingTest | 87 | J26 | — | manual §«Se comprueba en» |
| JOR-44 | JOR | Regla J27: Los humanos-insecto entrenan como en su época (Berón 2026-11-04: «desde la antigua Grecia se entrenaba físicamente»): en el lugar de entrenar de su pueblo (su clase estable, `AreaFunctionSite.training`, M35), cada uno co… | GymTrainingTest | 87 | J27 | — | manual §«Se comprueba en», también g23 |
| JOR-45 | JOR | Regla J28: Cada uno tiene su profesión (Berón 2026-11-04; `Professions`): cada día, una misión de cada área de su profesión antes de la rotación; una profesión puede nombrar solo algunas misiones del área (Tote, las reparaciones `r… | JornadaTest | 50 | J28 | — | manual §«Se comprueba en», también g51, g64, g45 |
| JOR-46 | JOR | Regla J29: Nada en un piso en obras: el ocio y el entrenamiento no van a un área cuyo `ConstructionProject` no está terminado (usan el de reserva: el baile, la calistenia o el gimnasio) y la regla de capacidad no cuenta sus puestos… | HorarioLibreTest | 65 | J29 | — | manual §«Se comprueba en», también g75 |
| JOR-47 | JOR | Regla J30: REGLA DE ORO: todos los personajes del santuario saben hacer de todo (Berón 2026-11-05: «las tareas de todas las áreas van por rotación normal, los puestos de nivel son un extra pero la regla de oro es que todos los pers… | GymTrainingTest | 87 | J30 | — | manual §«Se comprueba en» |
| JOR-48 | JOR | Regla J31: Tras comer, Kushal no va a un sitio lejano: a las 12 no se le da misión en el Sitio Nuclear, el Santuario Aéreo ni el parque energético: el viaje se comería la hora. | JornadaTest | 50 | J31 | — | manual §«Se comprueba en» |
| JOR-49 | JOR | Regla J32: La iniciación es una tarea de la jornada (Berón 2026-11-05): cada día se hace la lista de quién no sabe qué de lo que hay hoy, y se reparte antes que nada y de quien menos sabe a quien más, llenando primero sus horas de … | JornadaSaberTest | 104 | J32 | — | manual §«Se comprueba en», también g50 |
| JOR-50 | JOR | Regla J33: Cada humano tiene su casa: santuario y nivel, y sube por PODER (Berón 2026-11-08). Santuario 1 = la tierra, 2 = el Marino, 3 = el Aéreo. Sale de su santuario solo cuando su poder supera al jefe, el animal más poderoso de… | RepartoSantuariosTest | 113 | J33 | — | manual §«Se comprueba en» |
| JOR-51 | JOR | Regla J34: El reparto es por sitios y cambia cada día (Berón 2026-11-08). Cada uno pasa el día en un sitio al que llega. Primero se cubren las horas de cada sitio; los que sobran van a un segundo reparto. El orden del personal es u… | SanctuaryLifeTest | 28 | J34 | — | manual §«Se comprueba en» |
| JOR-52 | JOR | Regla J35: Partes iguales y nada a mañana (Berón 2026-11-08, corregido). Las horas de tareas de cada sitio se reparten por igual entre sus humanos, tan parejas como dejan las tareas, que no se parten (diferencia ≤ la tarea más larg… | SanctuaryLifeTest | 28 | J35 | — | manual §«Se comprueba en» |
| JOR-53 | JOR | Regla J36: Quien pasa el día en otro santuario no se simula aquí (Berón 2026-11-08): su cuerpo se apaga y sus tareas corren por el reloj; vuelve el día que le toca la tierra y en las fiestas (actividades de todos a la vez). Las sal… | RepartoSantuariosTest | 113 | J36 | — | manual §«Se comprueba en», también g75 |
| JOR-54 | JOR | Regla J37: Solo lo básico queda terminado (Berón 2026-11-09): los servicios básicos de humanos y animales (hospital, cocina, guardia, huerto, cría, carne cultivada, energía, mecánica), la construcción, el gimnasio y el yoga. Lo dem… | RepartoSantuariosTest | 113 | J37 | — | manual §«Se comprueba en» |

## YOG · Yoga

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| YOG-01 | YOG | Yoga: sin anillo, pisos Hatha y Ashtanga, occidental/tradicional, inmersión en el río | YogaFloorsTest | 64 | — | PB-052 | auditoría #52, L123, L130, L131 |
| YOG-02 | YOG | Consol (maestra de Ashtanga, 2.º puesto de yoga o más) | — | — | — | PB-091 | auditoría #91, L50, L53, L54 |

## GYM · Gimnasio

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| GYM-01 | GYM | Gimnasio real (series, repeticiones, rutinas semanales) y el entrenador | GymTrainingTest | 87 | — | PB-079 | auditoría #79, L109, L115, L116 |

## ARB · Árboles, edificios y alfombras

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| ARB-01 | ARB | Sin escaleras, puentes ni barreras de borde; se salta entre plataformas sobre cojines de musgo | PeticionesSondaTest | 117 | S5 | PB-001 | auditoría #1 |
| ARB-02 | ARB | Todos los pisos del mismo tamaño (SetBack 0) | PeticionesSondaTest | 117 | — | PB-002 | auditoría #2 |
| ARB-03 | ARB | Alfombras de 4 tipos (Edificio/Santuario/Residencia/Habitación) y rutas de NPC por `CarpetPad.Route` | — | — | — | PB-003 | auditoría #3, L154 |
| ARB-04 | ARB | Muebles para la persona más alta (1,85 m: camilla 2,00 × 0,70, asientos a 0,45) | PeticionesSondaTest | 117 | — | PB-004 | auditoría #4 |
| ARB-05 | ARB | 2 m de paso entre lo que no va junto | — | — | S7 | PB-007 | auditoría #7 |
| ARB-06 | ARB | Anillo (RingDeck) opcional, sin escaleras, unido por alfombras | — | — | S5 | PB-008 | auditoría #8 |
| ARB-07 | ARB | Octógonos con megacolumna CFST por esquina para 30 pisos y aviso si se pasa | — | — | S9 | PB-009 | auditoría #9 |
| ARB-08 | ARB | Plazas reservadas en cada piso para flora, fauna, personajes e historia | — | — | S10 | PB-010 | auditoría #10 |
| ARB-09 | ARB | Pocas torres temáticas con muchos pisos (Servicios, Entrenamiento, Ciencias, Producción) | — | — | S2 | PB-014 | auditoría #14 |
| ARB-10 | ARB | Árbol v2: tronco cuadrado, hélice, primer piso ≥ 50 m, entradas solo abajo y en el techo, nado entre niveles | TreeBuildingsTest | 57 | T1, T2 | PB-015 | auditoría #15, también g53 |
| ARB-11 | ARB | Ramas 5 × 5 y puertas 3 × 3 → sustituido por puertas de 7 × 7 (8 personas y submarino) | — | — | — | PB-016 | auditoría #16, L166 |
| ARB-12 | ARB | Migrar todos los edificios al modelo árbol ANTES de arreglar los fallos de pruebas | — | — | S3 | PB-017 | auditoría #17 |
| ARB-13 | ARB | Llenar las plataformas grandes de alfombras con un frutal de cada tipo, experta, sala de cuidados y jardinero diario | FruitOrchardTest | 55 | G1 | PB-018 | auditoría #18, L700, L701, L706 |
| ARB-14 | ARB | Árbol guardián (excepción de altura → injerto récord → sin túnel → retirado) | — | — | — | PB-019 | auditoría #19, L168, L83 |
| ARB-15 | ARB | Frutales en las residencias del SANTUARIO | FruitOrchardTest | 55 | — | PB-031 | auditoría #31, L278, L743, L279 |
| ARB-16 | ARB | «Pendiente de piso representativo» → crear esos pisos | — | — | — | PB-038 | auditoría #38, L273 |
| ARB-17 | ARB | Máscaras de oxígeno en entradas + estaciones; pruebas del jugador y de Kushal bajo el agua | TreeMaskTest | 70 | — | PB-039 | auditoría #39, L166 |
| ARB-18 | ARB | Tareas del jardinero: revisar máscaras y fitoplancton; reparto regenerado | FruitOrchardTest | 55 | — | PB-040 | auditoría #40, L166 |
| ARB-19 | ARB | Puertas y ramas para 8 personas a la vez | — | — | — | PB-042 | auditoría #42, L166 |
| ARB-20 | ARB | Jardines frutales en los techos de alfombras; compañeras, insectos, tierra frente a maceta | — | — | — | PB-046 | auditoría #46, L700, L701, L706 |
| ARB-21 | ARB | Logística de obra: submarino anfibio de obra, tronco y ramas más grandes, garaje, prueba de entrega | SubmarineTest | 73 | — | PB-047 | auditoría #47, L166, L42 |
| ARB-22 | ARB | Alfombras más gruesas | PeticionesSondaTest | 117 | — | PB-058 | auditoría #58, L154, L157 |
| ARB-23 | ARB | Piso de Informática (IA local, agua y energía) | InformaticaAulaTest | 74 | — | PB-067 | auditoría #67, L267, L268, L7 |
| ARB-24 | ARB | Aula universitaria | InformaticaAulaTest | 74 | — | PB-068 | auditoría #68, L267, L268, L7 |
| ARB-25 | ARB | Capacidad: maquinaria para varios a la vez | CapacidadPisosTest | 75 | — | PB-071 | auditoría #71, L267, L270 |
| ARB-26 | ARB | Los centros de entrenamiento sirven a todos a la vez | CapacidadPisosTest | 75 | — | PB-072 | auditoría #72, L744, L745, también g87 |
| ARB-27 | ARB | Las obras avanzan a la vista, pieza a pieza | ConstructionProgressTest | 98 | — | PB-085 | auditoría #85, L177 |
| ARB-28 | ARB | Una torre en la isla (transporte aéreo) | — | — | — | PB-102 | auditoría #102, L794, L86 |
| ARB-29 | ARB | Área de música (escenarios, estudios, cristal) | — | — | — | PB-104 | auditoría #104, L264, L266, L7 |
| ARB-30 | ARB | Columnas y techo en las salas subterráneas | — | — | T16 | PB-113 | auditoría #113, L171, L13, L12 |
| ARB-31 | ARB | Áreas mezcla de lo mejor del mundo (S14/F14) | — | — | S14, F14 | PB-118 | auditoría #118, L79, L81, L90 |
| ARB-32 | ARB | Techos de cristal y rampas permitidos | — | — | — | PB-120 | auditoría #120, L78, L265 |
| ARB-33 | ARB | Producción: la maquinaria crece con lo que se pide | ProduccionTest | 103 | — | PB-123 | auditoría #123, L798 |
| ARB-34 | ARB | Guardia en un piso de torre con pantallas (icebergs, sismos, tsunamis) | GuardRoomTest | 91 | — | PB-133 | auditoría #133, L82 |
| ARB-35 | ARB | Quitar los árboles artificiales (menos insectos) | — | — | — | PB-134 | auditoría #134, L85 |
| ARB-36 | ARB | Casquetes silvestres con estructuras para la fauna polar | GuardRoomTest | 91 | — | PB-135 | auditoría #135, L84 |
| ARB-37 | ARB | Árbol cazador: residencia → piso de investigación → bajo el agua con abejas y arañas | — | — | — | PB-136 | auditoría #136, L84, L86, L87, L88 |
| ARB-38 | ARB | Residencia a la isla y en edificio-árbol; casas de árbol guardadas | — | — | — | PB-137 | auditoría #137, L86, L88 |
| ARB-39 | ARB | No encoger salas ni maquinaria (fiestas para todos) | CapacidadPisosTest | 75 | J16 | PB-144 | auditoría #144, L15 |
| ARB-40 | ARB | Segunda fachada de fitoplancton y Huerto LED | FotobioFacadeTest | 56 | — | PB-147 | auditoría #147, L38, L40, L41, L43, también g103 |
| ARB-41 | ARB | Solo lo básico terminado; ciencias, asambleas, estudio… en obra | — | — | — | PB-149 | auditoría #149, L7, L8 |
| ARB-42 | ARB | Control de misión también en obra | — | — | — | PB-150 | auditoría #150 |
| ARB-43 | ARB | Se pueden quitar las sombras o las columnas de los árboles | — | — | — | PB-154 | auditoría #154 |
| ARB-44 | ARB | Regla S1: Lo construido no ocupa el suelo de la naturaleza: todo va elevado; el suelo queda para árboles, animales e insectos. | SanctuaryLifeTest | 28 | S1 | — | manual §«Se comprueba en», también g3 |
| ARB-45 | ARB | Regla S2: Pocas torres temáticas con muchos pisos: Servicios, Entrenamiento, Ciencias y Producción; los cultivos en el Árbol de los Campos; la Central Nuclear aparte (sitio de exclusión). | SanctuaryLifeTest | 28 | S2 | — | manual §«Se comprueba en», también g4 |
| ARB-46 | ARB | Regla S3: Todo edificio es un árbol (modelo `HelixTower`), salvo la Central Nuclear (1 piso a 12 m). | TreeBuildingsTest | 57 | S3 | — | manual §«Se comprueba en» |
| ARB-47 | ARB | Regla S4: ≥ 16 m en 3D entre torres (pisos, anillos, puntas, ramas y troncos; el Árbol de los Campos, girando). Dos vecinos con anillo necesitan al menos 8 + 8 m. | TreeBuildingsTest | 57 | S4 | — | manual §«Se comprueba en» |
| ARB-48 | ARB | Regla S5: Sin escaleras, sin puentes y sin barreras invisibles en los bordes. Se va en alfombra o, dentro de un árbol, nadando. Se puede saltar entre plataformas: debajo hay cojín de musgo. | SanctuaryLifeTest | 28 | S5 | — | manual §«Se comprueba en» |
| ARB-49 | ARB | Regla S6: Sin muros ni techos. La única pared permitida es la de cristal, solo en salas que deben aislarse, y guarda los 2 m, con hueco de puerta de 2 m. El techo de cristal se permite donde una función lo necesita (Berón 2026-11-… | GlassWallTest | 84 | S6 | — | manual §«Se comprueba en» |
| ARB-50 | ARB | Regla S7: 2 m de paso entre todo lo que no va junto (dos personas pasan a la vez). Lo que va unido cuenta como un mueble: piezas de un mueble, sillas con su mesa, una línea de encimeras, la cama y su mesilla. | SanctuaryLifeTest | 28 | S7 | — | manual §«Se comprueba en», también g0 |
| ARB-51 | ARB | Regla S9: Estructura para 30 pisos: `StructuralLimits.MaxFloors` = 30; el tronco de cada árbol se calcula (`RootCheck`) y se refuerza (`Reinforce`): avisa por encima del 80 %, error por encima del 100 %. | SanctuaryLifeTest | 28 | S9 | — | manual §«Se comprueba en» |
| ARB-52 | ARB | Regla S10: Plazas reservadas en cada piso (`PlatformSlots`) para la flora, la fauna, los personajes y la historia de su microcosmos. Cambiar un piso obliga a revisarlas. | SanctuaryLifeTest | 28 | S10 | — | manual §«Se comprueba en», también g40 |
| ARB-53 | ARB | Regla S11: Energía solar, sin gas: paneles en cada techo y el grueso en el Parque Energético (S15); cocinas de inducción. El reactor es la carga base: en la noche polar el sol da cero. | SanctuaryLifeTest | 28 | S11 | — | manual §«Se comprueba en» |
| ARB-54 | ARB | Regla S12: Una habitación por habitante en la Residencia, un edificio-árbol de la isla (Berón 2026-11-05: «la residencia se puede pasar a un arbol-edificio y las casas arbol quedan para otro santuary»; una pareja comparte la suya: … | SanctuaryLifeTest | 28 | S12 | — | manual §«Se comprueba en», también g55 |
| ARB-55 | ARB | Regla S13: Capacidad: varias personas a la vez (Berón 2026-11-04; = J16). Cada piso o área tiene máquinas e infraestructura para que varias personas hagan la misma tarea a la vez, como el restaurante o el hospital (≥ 2 puestos por … | CapacidadPisosTest | 75 | S13 | — | manual §«Se comprueba en», también g16, g9 |
| ARB-56 | ARB | Regla S14: Las áreas del santuario son una mezcla de lo mejor del mundo (Berón 2026-11-05): maquinaria y diseño con componentes de varios países y culturas (≥ 3), con el origen de cada uno apuntado. | AsambleaTest | 77 | S14 | — | manual §«Se comprueba en», también g8, g7 |
| ARB-57 | ARB | Regla S15: Cada piso paga su energía (Berón 2026-11-05): declara su demanda (`FloorEnergy`: área × W/m² de su clase + lo fijo), también los pisos sin área (residencia, sala de guardia, laboratorio), y su creación añade paneles en p… | EnergyTest | 101 | S15 | — | manual §«Se comprueba en» |
| ARB-58 | ARB | Regla S16: El árbol cazador, solo en su celda bajo el agua (Berón 2026-11-05): un árbol, entero, con sus abejas y arañas, en el Santuario Marino. Se alimenta sobre todo de los cadáveres de sus insectos, que circulan; lo nuevo lo fi… | GuardRoomTest | 91 | S16 | — | manual §«Se comprueba en» |
| ARB-59 | ARB | Regla T1: Tronco cuadrado de agua transparente (20 × 20 m de agua, pared de acrílico casi invisible, exoesqueleto de nervios CFST que carga). Entre pisos se nada. | TreeOfFieldsTest | 53 | T1 | — | manual §«Se comprueba en», también g57 |
| ARB-60 | ARB | Regla T2: Hélice: en cada nivel, pisos en dos caras opuestas y ramas a un campo en las otras dos; el nivel siguiente alterna (molinete). Plataformas, ramas y tronco, cuadrados. | TreeOfFieldsTest | 53 | T2 | — | manual §«Se comprueba en», también g57 |
| ARB-61 | ARB | Regla T3: Primer piso ≥ 50 m y por encima del árbol más alto (hoy 64 m). No hay árboles artificiales: en el suelo, solo abedules de la isla, que no sostienen nada; el árbol cazador vive contenido en el Santuario Marino (S16). El á… | TreeOfFieldsTest | 53 | T3 | — | manual §«Se comprueba en» |
| ARB-62 | ARB | Regla T4: Entradas solo en el suelo y en el techo: las alfombras solo están ahí. Se entra de frente por la puerta del techo. | TreeBuildingsTest | 57 | T4 | — | manual §«Se comprueba en» |
| ARB-63 | ARB | Regla T5: Medidas que mandan: niveles cada 22 m; ramas de 10 × 10 m; campos de punta de 20 × 20 m, 12 m sobre su nivel, con su anillo encima; anillo de un piso a 8 m sobre el piso. | SubmarineTest | 73 | T5 | — | manual §«Se comprueba en», también g57 |
| ARB-64 | ARB | Regla T6: Puertas cuadradas de 7 × 7 m: caben 8 personas a la vez y el submarino de obra. Las ramas, cuadradas y más anchas que las puertas. | TreeBuildingsTest | 57 | T6 | — | manual §«Se comprueba en», también g73 |
| ARB-65 | ARB | Regla T7: Sin suelo: el agua del tronco baja al mar por un hueco en el hielo; entran peces, kril y medusas. En el hueco cuelgan cortinas de fitoplancton. | GroupAnimalsTest | 71 | T7 | — | manual §«Se comprueba en», también g70 |
| ARB-66 | ARB | Regla T8: Máscaras de oxígeno: un perchero fuera de cada puerta y una estación dentro, en cada nivel. Con la máscara puesta el oxígeno es ilimitado. Quien acaba en el agua sin máscara nada a la estación más cercana. | TreeMaskTest | 70 | T8 | — | manual §«Se comprueba en» |
| ARB-67 | ARB | Regla T9: Vía marina de obra: la maquinaria pesada y el material viajan por agua en un submarino anfibio (14 × 5 × 5 m) que aparca en el piso de obra o en el garaje. | SubmarineTest | 73 | T9 | — | manual §«Se comprueba en» |
| ARB-68 | ARB | Regla T10: Solo gira el Árbol de los Campos (una vuelta cada 6 h de juego, contra el sol); las torres no giran. | TreeOfFieldsTest | 53 | T10 | — | manual §«Se comprueba en» |
| ARB-69 | ARB | Regla T11: Viajes sin teletransportes: se anda, se vuela o se nada; si alguien se atasca, se queda donde está y avisa. | TreeOfFieldsTest | 53 | T11 | — | manual §«Se comprueba en» |
| ARB-70 | ARB | Regla T13: Regla de altura de la lanzadera de cohetes (está en el Santuario Aéreo, T24; la regla cuenta también lo del santuario 1): plataforma ≥ máx(lo más alto del santuario —techos de los edificios-árbol con la columna de su pue… | LaunchSiteTest | 90 | T13 | — | manual §«Se comprueba en» |
| ARB-71 | ARB | Regla T14: Regla del sitio nuclear (2026-11-08: ver T25): la planta (Central Nuclear y Planta Atómica) va entera bajo el mar, en el Santuario Marino, junto a las cavernas; arriba, a ≥ 300 m de todo edificio e isla y a sotavento, so… | NuclearSiteTest | 94 | T14 | — | manual §«Se comprueba en», también g100, g114 |
| ARB-72 | ARB | Regla T15: Cada cosa, donde lo pide su peligro, no su tema. Lo que quiere encierro (reactor, residuos) va bajo el lecho; lo que quiere aire y distancia (oxígeno líquido, RP-1, pirotécnicos) va al aire, separado y en el casquete de … | LaunchSiteTest | 90 | T15 | — | manual §«Se comprueba en», también g94 |
| ARB-73 | ARB | Regla T16: La roca de las cavernas se mide, no se supone: salas grandes con columnas de roca, bulones y cables, y luz libre ≤ 14 m; holgura ≥ 3 m del equipo al forro y a las columnas, medida al construir; pilar entre salas no menor… | NuclearAccessTest | 100 | T16 | — | manual §«Se comprueba en» |
| ARB-74 | ARB | Regla T17: Línea de las cavernas: una estación en el casquete y los primeros 14 m de tubo; lo demás, en el Santuario Marino. Menos de 60 s, esclusas y carga incluidas (20 m/s, 1,5 m/s²). Cabina nivelada, sin aceite ni calefactor, m… | NuclearAccessTest | 100 | T17 | — | manual §«Se comprueba en» |
| ARB-75 | ARB | Regla T18: Regla del Parque Energético (lo que se ve está en el Santuario Aéreo, T24; la lógica, en el santuario 1): casquete artificial a ≥ 300 m de todo edificio e isla, árbol bajo (primer piso a 12 m, como T14). Aerogeneradores:… | EnergyTest | 101 | T18 | — | manual §«Se comprueba en» |
| ARB-76 | ARB | Regla T19: Dos mares: el santuario 1 tiene 16 m (`SeaRule.Depth`); el Santuario Marino, 500 m (`SeaRule.MarineDepth`), la media real del mar de Ross. A nado no se pasa del fondo somero: es un colisionador real, con su aviso. Los ca… | NuclearAccessTest | 100 | T19 | — | manual §«Se comprueba en», también g94 |
| ARB-77 | ARB | Regla T20: Refugio del fondo: junto al pie del pozo de escape, centrado en el pilar y fuera de las salas, para 12 personas: un mes con corriente del reactor por la roca (ósmosis inversa, electrólisis, depurador, dos sondeos a la ma… | NuclearAccessTest | 100 | T20 | — | manual §«Se comprueba en» |
| ARB-78 | ARB | Regla T21: Hielo fino y roca corta en el santuario 1: placas de 1,5 m (`SeaRule.PlateThickness`), falda de roca hasta −6 m y submarino de obra a −12 m. Nada hondo en esta escena. La plataforma de 300 m se descartó: no la necesitaba… | NuclearAccessTest | 100 | T21 | — | manual §«Se comprueba en», también g73, g107 |
| ARB-79 | ARB | Regla T22: Iceberg: ver, desviar, desconectar, en los tres casquetes: radar, sonar y tendones en cada uno. El escudo solo para trozos (≤ 100 000 t). Cada sitio tiene su radio de alerta (remolque obligatorio, que puede fallar, 10 %)… | NuclearAccessTest | 100 | T22 | — | manual §«Se comprueba en» |
| ARB-80 | ARB | Regla T23: Santuarios separados: lo de abajo es otra escena (`Santuario_Marino`), con su propio menú. Nada de arriba apunta a ella: la lógica va arriba, y lo de abajo cruza por `MarineBase.Instance`. Los trabajadores viajan sin car… | NuclearAccessTest | 100 | T23 | — | manual §«Se comprueba en» |
| ARB-81 | ARB | Regla T24: Santuario Aéreo: la lanzadera y el parque son otra escena (`Santuario_Aereo`), con su menú y en sus coordenadas de siempre, sobre el mismo mar. Se llega por el muelle: los trabajadores, sin cargarla; Kushal, en persona. … | AereoTest | 110 | T24 | — | manual §«Se comprueba en», también g90, g101 |
| ARB-82 | ARB | Regla T25: La planta nuclear, bajo el mar (Berón 2026-11-08): la Central Nuclear y la Planta Atómica, con todos sus pisos, puestos y maquinaria, son del santuario 2 (`Santuario_Marino`, `BuildMarinePlant`): salas a la cota de la ga… | NuclearSubmarinaTest | 114 | T25 | — | manual §«Se comprueba en», también g100 |
| ARB-83 | ARB | Regla C1: Cuatro clases de zona de alfombras (`CarpetPad.Kind`): Edificio (sus pisos), Santuario (otros techos y la residencia), Residencia (las habitaciones) y Habitación (vuelve a la plataforma central). | SanctuaryLifeTest | 28 | C1 | — | manual §«Se comprueba en» |
| ARB-84 | ARB | Regla C2: Una zona en cada piso, anillo y techo; el techo lleva la de edificio y la de santuario. En los árboles, solo en el techo y al pie (T4). | SanctuaryLifeTest | 28 | C2 | — | manual §«Se comprueba en», también g57 |
| ARB-85 | ARB | Regla C3: Cada pista sabe qué alfombra ocupa cada hueco y apila las que sobran sin solaparlas (`CarpetPad.Claim`). | SanctuaryLifeTest | 28 | C3 | — | manual §«Se comprueba en» |
| ARB-86 | ARB | Regla C5: Todo el que anda usa la red: `CarpetPad.Route` (y `ArbolCampos.Route` en los árboles) decide andar, volar o nadar; lo usan la jornada, los hechizos y las iniciaciones. | JornadaTest | 50 | C5 | — | manual §«Se comprueba en», también g57 |
| ARB-87 | ARB | Regla C6: La alfombra de cada casa es también su esterilla de yoga. | SanctuaryLifeTest | 28 | C6 | — | manual §«Se comprueba en» |
| ARB-88 | ARB | Regla C7: Alfombras más gruesas (`FlyingCarpet.Thickness` = 7 cm): tejido abajo y arriba, NÚCLEO de circuitos con pistas doradas que asoman por el canto y una COSTURA DE LED alrededor; quien va sentado o se baja, encima de su gros… | MicroSueloTest | 68 | C7 | — | manual §«Se comprueba en» |
| ARB-89 | ARB | Regla G1: El espacio libre de los techos se llena de frutales, uno de cada especie y otra vuelta, con una experta, una sala de cuidados y un jardinero del día que va de edificio en edificio. | FruitOrchardTest | 55 | G1 | — | manual §«Se comprueba en» |
| ARB-90 | ARB | Regla G2: Riego por goteo: el agua llega sola; la ronda revisa, abona y trata plagas. | FruitOrchardTest | 55 | G2 | — | manual §«Se comprueba en» |
| ARB-91 | ARB | Regla G3: Ninguna ronda pasa de 3 h (se planifica a 2,9 h); las macetas de un edificio no se parten; las tareas de la semana (polinizar, podar) se reservan en cada ronda. | FruitOrchardTest | 55 | G3 | — | manual §«Se comprueba en» |
| ARB-92 | ARB | Regla G4: Cima de los árboles: bancales de tierra por gremios (frutales que comparten pH), con plantas compañeras, flores de néctar, insectos útiles, abejorros, lombrices y caja nido; las puntas siguen en maceta. | RoofGardenTest | 80 | G4 | — | manual §«Se comprueba en» |
| ARB-93 | ARB | Regla G5: El jardinero también revisa máscaras y fitoplancton (panel en la puerta del suelo, 1/7 de las estaciones por rotación semanal y las que marcan rojo). | TreeMaskTest | 70 | G5 | — | manual §«Se comprueba en» |
| ARB-94 | ARB | Regla G6: Cada habitación de la Residencia tiene su terraza frutal, con un frutal en maceta a ≥ 2 m de sus muebles. | FruitOrchardTest | 55 | G6 | — | manual §«Se comprueba en» |
| ARB-95 | ARB | Regla G7: Los cultivos van en el Árbol de los Campos, no en parcelas en el suelo. | TreeOfFieldsTest | 53 | G7 | — | manual §«Se comprueba en», también g28 |
| ARB-96 | ARB | Regla G8: Un bancal o una maceta es un mueble: guarda los 2 m. | FruitOrchardTest | 55 | G8 | — | manual §«Se comprueba en», también g80 |

## MIC · Microcosmos

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| MIC-01 | MIC | Todo microcosmos sale de la plantilla (fecha, hora, piso) | — | — | M1 | PB-005 | auditoría #5 |
| MIC-02 | MIC | El microcosmos es SOLO su piso, amurallado; nada fuera | — | — | M5 | PB-012 | auditoría #12 |
| MIC-03 | MIC | Reglas 1–4 de los micro: paso de 2 personas, sin muros ni techos, muebles detallados con utensilios, necesidades básicas | — | — | — | PB-020 | auditoría #20, L271 |
| MIC-04 | MIC | Pueblos de época que llenan el piso; solo insectos-humanos civilizados; fauna solo si la época la tenía | — | — | — | PB-021 | auditoría #21, L251, L271 |
| MIC-05 | MIC | 12 figuras: Newton, Einstein, Tesla, Curie, Nietzsche, Bolívar, Pitágoras, Ananenko y su escuadra, Jenner, Patterson, Heródoto, al-Juarismi | — | — | — | PB-022 | auditoría #22, L73, L75, L159 |
| MIC-06 | MIC | Un HOGAR para cada personaje del pueblo, más trabajo y servicios | — | — | M18 | PB-023 | auditoría #23 |
| MIC-07 | MIC | Ciclo de vida igual para humanos e insectos-humanos, con prueba como las escenas CicloVida | CicloHumanoTest | 59 | M19 | PB-024 | auditoría #24 |
| MIC-08 | MIC | Escala ×100 (todo relativo a `AreaScale`) | MicrocosmosFloorTest | 43 | M11 | PB-025 | auditoría #25 |
| MIC-09 | MIC | Se permiten los muros históricos que pide la historia (gueto, reactor) | — | — | — | PB-026 | auditoría #26 |
| MIC-10 | MIC | Camas y servicios adaptados al insecto-humano y del estilo de la época; catálogo de estilos reutilizable | — | — | — | PB-027 | auditoría #27, L254, L271 |
| MIC-11 | MIC | El piso nunca se rompe; el pueblo se adapta | — | — | — | PB-028 | auditoría #28, L276, L279 |
| MIC-12 | MIC | Los insectos-humanos trepan; los muebles son suelo elevado | — | — | — | PB-029 | auditoría #29, L276, L279 |
| MIC-13 | MIC | Pueblos rehechos sobre mapas reales con edificios de actividades | — | — | — | PB-032 | auditoría #32, L251, L252, L271, L279 |
| MIC-14 | MIC | Figuras del mesocosmos: Musk, Goku, Gates, Buffett, MacKenzie Scott, Traoré (luego, a sus microcosmos) | FamososStoryTest | 69 | — | PB-035 | auditoría #35, L715, L728, L259, L260 |
| MIC-15 | MIC | Microcosmos maya | PakalStoryTest | 66 | — | PB-036 | auditoría #36, L258, L271 |
| MIC-16 | MIC | Microcosmos sobre la forma TERMINADA del piso; dos formas de cada piso | — | — | — | PB-037 | auditoría #37, L255, L256 |
| MIC-17 | MIC | Frutales en los hogares de los microcosmos | — | — | — | PB-044 | auditoría #44, L257, L254, L271 |
| MIC-18 | MIC | La vegetación SIEMPRE encima, nunca debajo (Ambrosio) | — | — | — | PB-053 | auditoría #53, L150, L157 |
| MIC-19 | MIC | Capa de tierra de 10 cm → luego apagada por pesada | MicroSueloTest | 68 | — | PB-054 | auditoría #54, L151, L158 |
| MIC-20 | MIC | 3 versiones de piso; elementos meso/micro; muebles como terreno | — | — | — | PB-055 | auditoría #55, L152, L157 |
| MIC-21 | MIC | Una historia puede rehacer su microcosmos desde la plantilla del piso | — | — | — | PB-056 | auditoría #56, L152 |
| MIC-22 | MIC | Hospital propio del microcosmos y revisión al cambiar el piso meso; gancho que lo recuerde | MicroSueloTest | 68 | M9 | PB-057 | auditoría #57, L153, L160, L58 |
| MIC-23 | MIC | Plaza de Bolívar: asientos usables, platos y ollas, escala, estatua y fuente | — | — | — | PB-060 | auditoría #60, L155, L157 |
| MIC-24 | MIC | Versiones insecto-humano de las estatuas | — | — | — | PB-061 | auditoría #61, L156, L157 |
| MIC-25 | MIC | Al cambiar piso/área/árbol/microcosmos, revisar jornadas, reparto y horario libre (gancho W9) | — | — | — | PB-064 | auditoría #64 |
| MIC-26 | MIC | Alerta de piso nuevo para los microcosmos pendientes | AlertaPisoNuevoTest | 76 | — | PB-070 | auditoría #70, L267, L268 |
| MIC-27 | MIC | Flora y fauna nunca bajo el piso: los pobladores sueltan desde el aire | MicroSueloTest | 68 | — | PB-074 | auditoría #74, L159 |
| MIC-28 | MIC | Mapa hecho a mano por historia; la siembra al azar, solo de respaldo | MapasAManoTest | 115 | — | PB-075 | auditoría #75, L159, L66, L261, L160 |
| MIC-29 | MIC | Lugares de entrenamiento de época en los pueblos y planes para insectos-humanos | CivilizedErasTest | 62 | — | PB-081 | auditoría #81, L280, L114, L281, L120, L283 |
| MIC-30 | MIC | Rellenar huecos históricos con mapas actuales (solo posiciones) | — | — | — | PB-082 | auditoría #82, L280, L282 |
| MIC-31 | MIC | Microcosmos de Eloy Alfaro, Galvarino, Pedro Sánchez, Atahualpa, Cleopatra, Beethoven, Descartes, Che, Lincoln | HistoriasStoryTest | 79 | — | PB-087 | auditoría #87, L65, L69 |
| MIC-32 | MIC | Shakira, Bad Bunny, John Lennon, Michael Jackson | MusicaStoryTest | 86 | — | PB-088 | auditoría #88, L261, L262, L263 |
| MIC-33 | MIC | Nativos americanos (Gran Ley de la Paz) y las calculadoras de la NASA | HistoriasStoryTest | 79 | — | PB-089 | auditoría #89, L67, L69 |
| MIC-34 | MIC | BLACKPINK, una banda de rock, una de rap, «grandes aportes de las canciones» | MusicaStoryTest | 86 | — | PB-100 | auditoría #100, L261, L262 |
| MIC-35 | MIC | Microcosmos de presidentes (hazañas) y Lee Kuan Yew | HistoriasStoryTest | 79 | — | PB-101 | auditoría #101, L67, L70 |
| MIC-36 | MIC | Atahualpa: la vida inca y la bienvenida | HistoriasStoryTest | 79 | — | PB-105 | auditoría #105, L71 |
| MIC-37 | MIC | Ideas para que las turbinas no maten aves (cría de aves, figuras, dragón de drones, aves que avisan) | — | — | — | PB-107 | auditoría #107, L107 |
| MIC-38 | MIC | Mejorar los microcosmos viejos | — | — | — | PB-110 | auditoría #110, L73 |
| MIC-39 | MIC | Sala de asambleas investigada por época | AsambleaTest | 77 | — | PB-117 | auditoría #117, L75, L77, L7 |
| MIC-40 | MIC | Regla F1: Todo piso tiene forma finalizada y forma en construcción (`FloorForms`): una obra tiene su «Acabado» oculto; un piso terminado tiene su «Forma_en_obra» oculta (andamio, palés, conos, muretes). | BruceLeeStoryTest | 60 | F1 | — | manual §«Se comprueba en» |
| MIC-41 | MIC | Regla F2: Tercera versión: el microcosmos. Todo piso tiene tres versiones: terminada, en construcción y microcosmos (`FloorForms.MicroName` = «Forma_microcosmos»: en el santuario, oculta, con su inventario `MicroFloorForm`; en el … | MicroSueloTest | 68 | F2 | — | manual §«Se comprueba en», también g9 |
| MIC-42 | MIC | Regla F3: En la versión micro, los muebles son terreno o edificios (tabla de `MicroFloorElements.Catalog`): fregadero = lago (cascada y río si el grifo está abierto); cajones y armarios = edificios de la era; caja = cueva (antes d… | MicroSueloTest | 68 | F3 | — | manual §«Se comprueba en» |
| MIC-43 | MIC | Regla F4: Todos los pisos de un edificio miden lo mismo: el cuadrado del lado mayor de su área más grande (`SetBack` = 0). Cada área va centrada en su piso, con su lado −Z contra el tronco. | TreeBuildingsTest | 57 | F4 | — | manual §«Se comprueba en» |
| MIC-44 | MIC | Regla F5: Las puertas de los pisos van donde su umbral queda libre de muebles. | TreeBuildingsTest | 57 | F5 | — | manual §«Se comprueba en» |
| MIC-45 | MIC | Regla F6: Al cambiar un piso: comprobar los 2 m, la zona de alfombras, las plazas reservadas, los pisos de la torre y los microcosmos que dependen de él (§11). | SanctuaryLifeTest | 28 | F6 | — | manual §«Se comprueba en», también g11 |
| MIC-46 | MIC | Regla F7: Yoga: sin anillo, porque desde el anillo no se ve al maestro. | YogaFloorsTest | 64 | F7 | — | manual §«Se comprueba en», también g28 |
| MIC-47 | MIC | Regla F8: Pisos de yoga separados: Hatha en un piso (el de la Sala de Yoga) y Ashtanga en otro (su planta propia), y en cada uno una sala occidental y una tradicional (2 pisos × 2 salas, no 4 pisos: Berón pidió «otro piso» y la to… | YogaFloorsTest | 64 | F8 | — | manual §«Se comprueba en» |
| MIC-48 | MIC | Regla F9: El maestro, visible desde cada esterilla: tarima en el centro de la sala, esterillas en un solo anillo a su alrededor (nadie detrás de otro alumno), lo alto (estatuas, estantes, el kund) en las bandas de los extremos, y … | YogaFloorsTest | 64 | F9 | — | manual §«Se comprueba en» |
| MIC-49 | MIC | Regla F10: Sala tradicional: suelo de tierra, alfombras de algodón, asiento de kusha y algodón (sin piel animal), lámparas de aceite y su elemento (linga como el Dhyanalinga en Hatha; Adiyogi y Patanjali en Ashtanga). | YogaFloorsTest | 64 | F10 | — | manual §«Se comprueba en» |
| MIC-50 | MIC | Regla F11: Inmersión antes de la práctica tradicional: un kund con agua del tronco del árbol, sujeta por el campo magnético (sin escalones); cada alumno se sumerge antes de ir a su esterilla (como en los Theerthakund de Isha antes … | YogaFloorsTest | 64 | F11 | — | manual §«Se comprueba en» |
| MIC-51 | MIC | Regla F12: Sala occidental: suelo de madera, esterillas, accesorios (bloques, cinturones, bolsters, mantas; en Ashtanga, alfombras de Mysore), Buda o Ganesha, y sin pared de espejo (distrae de la práctica interior; y no hay muros). | YogaFloorsTest | 64 | F12 | — | manual §«Se comprueba en» |
| MIC-52 | MIC | Regla F14: Toda área y todo microcosmos investigan su diseño por época y lugar antes de construirse (Berón 2026-11-05): cómo es de verdad, con fuentes en su doc, y lo que no se pudo comprobar marcado [verificar]. | AsambleaTest | 77 | F14 | — | manual §«Se comprueba en» |
| MIC-53 | MIC | Regla F13: Las obras avanzan a la vista (Berón 2026-11-04: «el progreso de las obras podría ser visual?»). Las piezas del «Acabado» se reparten por los días de obra en orden de etapas (cimientos y suelo → estructura → instalaciones… | ConstructionProgressTest | 98 | F13 | — | manual §«Se comprueba en» |
| MIC-54 | MIC | Regla M2: Pertenece a un piso y se monta ENCIMA: el piso ×100 es su suelo y la historia va a ras de él. | MicrocosmosFloorTest | 43 | M2 | — | manual §«Se comprueba en» |
| MIC-55 | MIC | Regla M3: Siempre sobre la forma FINALIZADA del piso (nada de andamio ni conos). | BruceLeeStoryTest | 60 | M3 | — | manual §«Se comprueba en» |
| MIC-56 | MIC | Regla M5: El microcosmos es SOLO su piso, cerrado por muros en el borde; fuera no se puebla nada. | MicrocosmosFloorTest | 43 | M5 | — | manual §«Se comprueba en» |
| MIC-57 | MIC | Regla M6: El piso no se rompe: ninguna pieza se quita ni se mueve; el pueblo y la historia se adaptan, rodeando los muebles o subiéndose a ellos. | BruceLeeStoryTest | 60 | M6 | — | manual §«Se comprueba en» |
| MIC-58 | MIC | Regla M7: Todas las estructuras del piso son superficie utilizable: flora y fauna encima de los muebles gigantes; barrio alto y miradores en sus mesetas. | MicrocosmosFloorTest | 43 | M7 | — | manual §«Se comprueba en» |
| MIC-59 | MIC | Regla M9: Toda historia tiene su mapa HECHO A MANO (Berón 2026-11-04: «se puede crear a mano cada mapa de cada historia del microcosmos, de modo que la flora y fauna se colocan a mano y de paso se organizan los muebles y demás com… | MicroSueloTest | 68 | M9 | — | manual §«Se comprueba en» |
| MIC-60 | MIC | Regla M12: La flora del pueblo es de pueblo humano: a escala real, escasa, fuera de calles y solares, ninguna planta > 25 m ni flotando. | MicrocosmosFloorTest | 43 | M12 | — | manual §«Se comprueba en» |
| MIC-61 | MIC | Regla M13: Escala por pieza, no ×2 global (`MicroCivicScale`): lo que el cuerpo USA, a la medida del cuerpo (asiento al vientre, mesa a las manos: un ×2 los haría inservibles); lo que ALIMENTA, por raciones (plato ≥ 0,26 m, olla de… | IdealStateTest | 13 | M13 | — | manual §«Se comprueba en», también g15 |
| MIC-62 | MIC | Regla M14: Eras avanzadas (desde −3500): el pueblo o la ciudad real ENTERA, investigada y levantada sobre su plano real (calles, ríos, lagos, montes, sitio de cada edificio). | CivilizedErasTest | 62 | M14 | — | manual §«Se comprueba en», también g12 |
| MIC-63 | MIC | Regla M15: Solo insectos-humanos civilizados; flora y fauna salvajes solo si el lugar y la época las tenían. | MicrocosmosFloorTest | 43 | M15 | — | manual §«Se comprueba en» |
| MIC-64 | MIC | Regla M16: El pueblo se amplía con edificios de actividades del santuario (estudio, entrenamiento, baile, plazas, estaciones, parques, museos, investigación, guarderías, estatuas, policía, bomberos, mercados, talleres, milicia, pis… | MicroBuildRulesTest | 63 | M16 | — | manual §«Se comprueba en», también g10 |
| MIC-65 | MIC | Regla M17: Cada edificio nace con su interacción y su categoría (trabajo, descanso, recuperación, ocio o entrenamiento), su capacidad y los cuerpos que lo usan. | HorarioLibreTest | 65 | M17 | — | manual §«Se comprueba en», también g8, g63 |
| MIC-66 | MIC | Regla M18: Un hogar para cada personaje (una cama por persona, retrete, lavabo, mesa con sillas, despensa), además de su trabajo y los servicios básicos; las parejas comparten casa. | CivilizedErasTest | 62 | M18 | — | manual §«Se comprueba en» |
| MIC-67 | MIC | Regla M19: El ciclo de vida de humanos e insectos-humanos es el mismo (necesidades, etapas, vejez, pareja, nacimiento, muerte). | CicloHumanoTest | 59 | M19 | — | manual §«Se comprueba en», también g62 |
| MIC-68 | MIC | Regla M20: Toda residencia tiene su frutal, de su época y lugar. | ObservationTest | 11 | M20 | — | manual §«Se comprueba en», también g63 |
| MIC-69 | MIC | Regla M21: Edificios de varias plantas en terraza, con rampa y sin techos. | StateSpellsTest | 12 | M21 | — | manual §«Se comprueba en», también g63 |
| MIC-70 | MIC | Regla M39: Toda frase tiene su nivel de veracidad y su fuente (Berón 2026-11-05): documentada, atestiguada, recreada o inventada. Las frases se conservan, también las de personas vivas no políticas; se ve en la nota de cada frase y… | MicrocosmosFloorTest | 43 | M39 | — | manual §«Se comprueba en», también g77 |
| MIC-71 | MIC | Regla M36: Una ficha con datos de actualidad lleva su fecha y su aviso (2026-11-05, por la auditoría W11): si una ficha habla de un cargo o un hecho en curso, dice «a fecha de…» y el checklist apunta cuándo revisarla. | HistoriasStoryTest | 79 | M36 | — | manual §«Se comprueba en» |
| MIC-72 | MIC | Regla M35: Todo pueblo tiene trabajo, ocio, ENTRENAMIENTO y recuperación (2026-11-04; el entrenamiento es ahora categoría propia). Lo que el plano histórico no trae se coloca en una posición de un mapa de hoy, marcada en el plano (… | CivilizedErasTest | 62 | M35 | — | manual §«Se comprueba en», también g43 |
| MIC-73 | MIC | Regla M23: 2 m de paso entre objetos (dos insectos-humanos a la vez); calles de 4 m. | MicroBuildRulesTest | 63 | M23 | — | manual §«Se comprueba en» |
| MIC-74 | MIC | Regla M24: Sin muros ni techos, salvo los muros que la historia necesita (gueto de Varsovia, bloque 4 de Chernóbil: `MicroPiece.kind = "historico"`) y el cristal de salas aisladas. | MicroBuildRulesTest | 63 | M24 | — | manual §«Se comprueba en» |
| MIC-75 | MIC | Regla M25: Muebles detallados, con sus utensilios (sartén, olla, cuchillo…): nada de cubos lisos. | MicroBuildRulesTest | 63 | M25 | — | manual §«Se comprueba en» |
| MIC-76 | MIC | Regla M26: Necesidades básicas siempre: dormir, aseo, lavarse, comer, sentarse y cocinar. | CivilizedErasTest | 62 | M26 | — | manual §«Se comprueba en», también g63 |
| MIC-77 | MIC | Regla M27: Muebles a la medida de quien los usa (`MicroBody`: centauro, cúpula, alado, mantis, araña, ciempiés, humano). | RiverCurrentTest | 9 | M27 | — | manual §«Se comprueba en», también g63 |
| MIC-78 | MIC | Regla M28: Adaptado a personas y a insectos-humanos: cada edificio tiene al menos un mueble para cada uno. | RiverCurrentTest | 9 | M28 | — | manual §«Se comprueba en», también g63 |
| MIC-79 | MIC | Regla M29: Estilo de época y lugar desde el catálogo (`MicroStyles`: año + región → cada mueble); épocas y lugares parecidos reutilizan la entrada y la plantilla la elige sola. | DietPreferenceTest | 10 | M29 | — | manual §«Se comprueba en», también g63 |
| MIC-80 | MIC | Regla M32: La vegetación crece siempre ENCIMA de las superficies, nunca debajo, y nunca dentro de un collider: cada planta (y cada bicho) se apoya en la superficie más alta de encima de su pie (`MicroGround.Settle`: un rayo desde a… | StarvationTest | 7 | M32 | — | manual §«Se comprueba en» |
| MIC-81 | MIC | Regla M33: Capa de tierra de 10 cm (APAGADA por defecto desde el 2026-11-04: `MicroGround.SoilEnabled = false`, colgaba el NavMesh de la Cocina; Berón: «se puede desactivar si es tan pesado») (`MicroGround.SoilThickness`, metros de… | SleepTest | 8 | M33 | — | manual §«Se comprueba en» |
| MIC-82 | MIC | Regla M34: Toda estatua humana tiene su versión insecto-humano en los microcosmos (`MicroStatue`): el cuerpo de un insecto-humano de verdad (su especie por el papel que representa), de pie, de piedra o bronce, en la misma pose y co… | YogaTest | 15 | M34 | — | manual §«Se comprueba en» |
| MIC-83 | MIC | Regla M30: sillas y bancos se pueden usar: cada asiento lleva su MicroSeat (una silla, un sitio; un banco de plaza, tres asientos) | MicroSueloTest | 68 | M30 | — | manual §«Se comprueba en» (también ValidateCivicScale regla 13 y g67); añadido a mano el 2026-11-13 |

## HOS · Hospital

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| HOS-01 | HOS | Submarino-hospital para personas y animales grandes | HospitalSubmarineTest | 81 | — | PB-048 | auditoría #48, L702, L706 |
| HOS-02 | HOS | Atención a ballenas y orcas; hospital submarino colosal | WhaleHospitalTest | 82 | — | PB-049 | auditoría #49, L703, L706 |
| HOS-03 | HOS | Banco de sangre | BloodBankTest | 83 | — | PB-050 | auditoría #50, L704, L706 |
| HOS-04 | HOS | Paredes de cristal en salas aisladas | GlassWallTest | 84 | — | PB-051 | auditoría #51, L705, L706 |
| HOS-05 | HOS | Regla H3: No se opera sin alguien en la anestesia, ni a quien perdió ≥ 30 % de sangre sin sangre compatible. | BloodBankTest | 83 | H3 | — | manual §«Se comprueba en» |
| HOS-06 | HOS | Regla H4: Banco de sangre con las reglas reales (temperaturas, caducidades, compatibilidad, donantes del santuario). | BloodBankTest | 83 | H4 | — | manual §«Se comprueba en» |
| HOS-07 | HOS | Regla H5: Paredes de cristal solo en salas que deben aislarse (quirófanos, UCI, aislamientos, sala fría) y solo en épocas en que existían. | GlassWallTest | 84 | H5 | — | manual §«Se comprueba en» |
| HOS-08 | HOS | Regla H7: Submarino-ambulancia para personas y animales grandes, y el Leviatán para ballenas y orcas. | HospitalSubmarineTest | 81 | H7 | — | manual §«Se comprueba en», también g82 |
| HOS-09 | HOS | Regla H8: El hospital del microcosmos es una construcción propia que incluye TODOS los instrumentos, muebles y decoraciones del hospital meso, en su versión micro: es la «Forma_microcosmos» del piso del hospital, que se hace desde… | MicroSueloTest | 68 | H8 | — | manual §«Se comprueba en», también g0 |

## NUC · Nuclear, refugio y santuarios 2 y 3

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| NUC-01 | NUC | Lanzadera de cohetes en un casquete y regla de altura | LaunchSiteTest | 90 | — | PB-069 | auditoría #69, L167, L175 |
| NUC-02 | NUC | Planta atómica lo más segura posible (bajo el lecho o el suelo) | NuclearSubmarinaTest | 114 | — | PB-086 | auditoría #86, L169, L174, L10, L13, L11, L12 |
| NUC-03 | NUC | Sísmica: no; túnel-tren rápido al lecho | NuclearAccessTest | 100 | — | PB-108 | auditoría #108, L169, L171 |
| NUC-04 | NUC | Profundidad real del lecho y túnel de menos de 1 min | NuclearAccessTest | 100 | — | PB-112 | auditoría #112, L171, L174 |
| NUC-05 | NUC | «Hazlo real»: el hielo flota, la roca llega al lecho, refugio en el fondo | — | — | — | PB-121 | auditoría #121, L172, L799 |
| NUC-06 | NUC | Refugio para 1 mes (desalinizar, aire, huerto, huevos) | — | — | — | PB-125 | auditoría #125, L173 |
| NUC-07 | NUC | Santuario 1 = tierra y mar somero; el mar profundo, al Santuario Marino | — | — | — | PB-127 | auditoría #127, L174, L11 |
| NUC-08 | NUC | El refugio crece hasta que se pueda vivir en él, con áreas nuevas | — | — | — | PB-128 | auditoría #128, L172, L173 |
| NUC-09 | NUC | Santuario 3 Aéreo con cohetes y viento/agua | AereoTest | 110 | — | PB-131 | auditoría #131, L175, L33, L35 |
| NUC-10 | NUC | Planta nuclear = santuario 2, entera bajo el agua; solo la estación arriba | — | — | — | PB-142 | auditoría #142, L10, L13, L11, L12 |

## ENE · Energía

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| ENE-01 | ENE | Fernando y las tareas solares en la jornada | — | — | — | PB-092 | auditoría #92, L50, L55 |
| ENE-02 | ENE | Paneles solares en cada piso nuevo según su demanda | EnergyTest | 101 | — | PB-093 | auditoría #93, L100, L103, L104 |
| ENE-03 | ENE | Casquete de energía lejano (solar, eólica, hidráulica seguras) | EnergyTest | 101 | — | PB-094 | auditoría #94, L100, L104, L106 |

## PER · Personajes y charlas

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| PER-01 | PER | Conversaciones por las ánimas y escena de prueba | ConversationTest | 88 | — | PB-062 | auditoría #62, L774, L780, L781, L783 |
| PER-02 | PER | Personajes: parejas, profesiones, Andrea la más fuerte y entrenadora, 6 nuevos, Neo, renombres | PersonajesReglasTest | 95 | — | PB-083 | auditoría #83, L748, L759, L113, L750, L758 |
| PER-03 | PER | Frases con fuente (verificada o inventada) | — | — | — | PB-119 | auditoría #119, L76, L80, L92 |
| PER-04 | PER | Nombres reales ocultos pero documentados | — | — | — | PB-145 | auditoría #145, L25, L27 |
| PER-05 | PER | Cocina: Goluis encargado; Maite y Amykhuz los rangos más altos | PersonajesReglasTest | 95 | — | PB-146 | auditoría #146, L25, L28 |
| PER-06 | PER | Regla P2: Cada persona del mesocosmos tiene su ficha `docs/personajes/<nombre>.md`. | PersonajesReglasTest | 95 | P2 | — | manual §«Se comprueba en» |
| PER-07 | PER | Regla P4: Profesión en la tabla `Professions.All`, con su área de salida y su rótulo (J28). | SanctuaryLifeTest | 28 | P4 | — | manual §«Se comprueba en» |
| PER-08 | PER | Regla P5: Parejas e hijos comparten casa (J14); si no queda casa, una tanda de 6, una por árbol. | PillowFightTest | 89 | P5 | — | manual §«Se comprueba en» |
| PER-09 | PER | Regla P8: W9 al llegar o irse alguien: jornadas, reparto y horario libre. | HorarioLibreTest | 65 | P8 | — | manual §«Se comprueba en» |
| PER-10 | PER | Regla P9: J16 al llegar alguien: recuento de centros, ring y camas. | PillowFightTest | 89 | P9 | — | manual §«Se comprueba en» |
| PER-11 | PER | Regla P11: Regla de oro: todos saben hacer de todo (= J30). | GymTrainingTest | 87 | P11 | — | manual §«Se comprueba en» |
| PER-12 | PER | Regla P24: Cada persona tiene su ficha con la plantilla de personajes/README.md (Berón 2026-11-08): las 11 secciones, cada dato con su veracidad (M39) y su fuente (doc y sección, o fichero y línea), y «Falta» para lo que no se sabe… | PersonajesReglasTest | 95 | P24 | — | manual §«Se comprueba en» |
| PER-13 | PER | Regla P12: Mitad tierra, mitad hielo (40–60 %); la isla reemplaza casquetes (Sur y Este) y tiene un edificio con alfombras. Las distancias entre masas están comprimidas para el juego, y se dice. | TerritorioTest | 99 | P12 | — | manual §«Se comprueba en» |
| PER-14 | PER | Regla P13: `NaturalIce` = suelo firme; `IceAt` y `LandAt` dicen cuál. Colliders: `Casquete`, `Tierra`, `Tierra_orilla`. | TerritorioTest | 99 | P13 | — | manual §«Se comprueba en» |
| PER-15 | PER | Regla P14: La isla respeta los sitios lejanos: ≥ 300 m de la lanzadera y ≥ 300 m del sitio nuclear (T14). | NuclearSiteTest | 94 | P14 | — | manual §«Se comprueba en» |
| PER-16 | PER | Regla P15: Cuevas en las que se entra andando: ≥ 4 × 3 m de boca, techo, NavMesh hasta el fondo. | TerritorioTest | 99 | P15 | — | manual §«Se comprueba en» |
| PER-17 | PER | Regla P16: Flora sobre el suelo, de especies reales; en la isla no crecen árboles de hielo. | TerritorioTest | 99 | P16 | — | manual §«Se comprueba en» |
| PER-18 | PER | Regla P18: Al mover una torre, se lleva lo que depende de ella: alfombras al suelo, animales, horario libre; holguras con `mig/Clear.cs`. | TreeBuildingsTest | 57 | P18 | — | manual §«Se comprueba en» |
| PER-19 | PER | Regla P20: Las placas de hielo y la isla son paredes para la fauna; solo se respira en agua abierta, canales o hielo fijo. Con 1,5 m ya no es física, sino diseño: las placas llevan los edificios y la fauna no se mete debajo. Bajo e… | HieloMarinoTest | 107 | P20 | — | manual §«Se comprueba en» |
| PER-20 | PER | Regla P21: Quien usa un iceberg tiene su motivo: por qué va, cuánto se queda y por qué se va, según la especie y la estación; nada de figuras, también en las aves; contado en la cadena alimenticia. | IcebergVidaTest | 108 | P21 | — | manual §«Se comprueba en» |
| PER-21 | PER | Regla P22: El O₂ y la verdura se dimensionan para las personas de la lista (`PopulationPlan.People`: el personal de `WorkdayDirector.Staff`, la Magnate y el recién llegado; hoy 32), al construir y en juego, no para los ya iniciados… | ProduccionTest | 103 | P22 | — | manual §«Se comprueba en» |
| PER-22 | PER | Regla P23: Cada carga de cultivo con luz paga su energía y cada cultivo tiene su cuidado diario de ≤ 1 h: las fachadas de fotobiorreactores declaran sus LED (potencia media del año; en la noche polar, la entera, que el reactor debe… | AreaFunctionRegistryTest | 61 | P23 | — | manual §«Se comprueba en» |
| PER-23 | PER | Regla P7: ningún puesto sin base documentada (Professions.Basis); lo deducido, en Professions.Unconfirmed, hasta que Berón confirme | PersonajesReglasTest | 95 | P7 | — | manual §«Se comprueba en» (también tools/check-personajes.sh); añadido a mano el 2026-11-13 |
| PER-24 | PER | Regla P19: las áreas de producción escalan su maquinaria a lo que se les pide (ProductionAudit) | ProduccionTest | 103 | P19 | — | manual §«Se comprueba en» (también el registro [Producción]); añadido a mano el 2026-11-13 |

## UI · Interfaz

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| UI-01 | UI | UI: horario sobre F10, recursos en una ventana, ventana de misiones y jornada | HudModalsTest | 93 | F10 | PB-090 | auditoría #90, L50, L53 |

## REN · Rendimiento y pruebas

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| REN-01 | REN | Las abejas del santuario deslumbran | — | — | — | PB-059 | auditoría #59, L178 |
| REN-02 | REN | Focos brillantes en vivo: revisar medusas y todo, verificado en vivo | — | — | — | PB-138 | auditoría #138, L178 |
| REN-03 | REN | Todo opaco, sin brillo, materiales eficientes | PeticionesSondaTest | 117 | — | PB-152 | auditoría #152 |
| REN-04 | REN | ¿Por qué pesa más que GTA5? (medido: 395 ms/frame de scripts) | — | — | — | PB-155 | auditoría #155, L46 |
| REN-05 | REN | Regla W12: Una clase que mueva u oculte piezas hijas por script va en `LoteEstatico.Blocked` (o en `Anchors` si se mueve entera y rígida); sin eso funciona, pero la pieza tarda hasta una vuelta de barrida en reaccionar. | LoteEstaticoTest | 112 | W12 | — | manual §«Se comprueba en» |
| REN-06 | REN | Regla W14: Repartir trabajo entre frames, solo si no integra tiempo: lo que usa el `Time.deltaTime` de un frame (andar, entrenar, actuar) corre en cada frame; se reparte la búsqueda cara o se acumula el tiempo con `GameTick`. | JornadaTest | 50 | W14 | — | manual §«Se comprueba en», también g112 |
| REN-07 | REN | Regla W15: Un MonoBehaviour que se guarda en la escena va en su propio archivo, con el nombre de la clase. | JornadaTest | 50 | W15 | — | manual §«Se comprueba en» |
| REN-08 | REN | Regla W16: sistema de QA: catálogo, registro, peticiones, manual y pruebas cuadran (QaCheck) | QaCheckTest | 118 | W16 | — | manual §10; añadido a mano el 2026-11-13 |

## PX · Recorridos en vivo

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| PX-01 | PX | Ambrosio en el hospital al alba de la taiga; Sakshi segunda anciana; mariquita de 2,4 m como depredador | ExperienciaJugadorTest | 40 | — | PB-006 | auditoría #6, L157 |
| PX-02 | PX | Tareas e iniciaciones del jugador con E (modelo Ambrosio) | PlayerTasksTest | 96 | — | PB-078 | auditoría #78, L765, L770, L771 |

## CV · Convivencia de procesos (entrelazamiento.md)

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| CV-01 | CV | IFastGameTime (maxDelta 1) × puertas, disparadores y agentes: Ningún paso grande hace que un agente se salte una puerta o un disparador; el grupo que lo usa y los siguientes, igual de verdes | JornadaTest | 50 | — | — | 5347100, g50 y g40 |
| CV-02 | CV | maxDelta 0,04 de GymUsabilityTest × el grupo siguiente: TestRunner.Sanear lo restaura; 0 «FUGA» de tiempo | GymUsabilityTest | 49 | — | — | 13a213a, g49 |
| CV-03 | CV | Warp de la fauna × viaje en marcha: Quitar o cambiar el Warp no sube «atascado > 20 s» respecto a la media de las 3 rondas anteriores (contadores.md) | JornadaTest | 50 | — | — | 52d2949; r76–r77 dieron 27 y 34 |
| CV-04 | CV | Pausa de otra conducta × velocidad de un viaje: Un viaje en marcha nunca queda con velocidad «pedida 0,0» | FaunaChecks | 1 | — | — | 52d2949 |
| CV-05 | CV | Habit × Volition (deseo) × ciclo de vida × viajes: El cuerpo tiene un solo dueño (el deseo que gana); el hábito no cuenta como atasco | FaunaChecks | 1 | — | — | d44f7e8, 5d89067 |
| CV-06 | CV | TreeRotator × NavMesh (StaleNavCarver) × crías del recinto (PenWalker): La raíz vuelve a tallar el NavMesh tras cada paso; las crías del recinto andan sin NavMesh y no se quedan quietas | TreeOfFieldsTest | 53 | — | — | 2847127, 76db8a8, 4990e1c |
| CV-07 | CV | WorkdayDirector × ronda de frutales (FruitGardener) × detector de «dar vueltas»: La ronda de frutales no es «dar vueltas» | JornadaTest | 50 | — | — | f64fad3 |
| CV-08 | CV | WorkdayDirector × clase de yoga: Las conductas que dejó la jornada se pausan en la clase | YogaClassTest | 51 | — | — | e2c0c7c |
| CV-09 | CV | Jornada × horario libre (16 h) × cena (19 h, MealRitual): La barra se vuelve a llenar a las 19 h; nadie se queda sin franja | HorarioLibreTest | 65 | — | — | 2d9a0aa, 31df64e |
| CV-10 | CV | WorkRoster: reparto × opcionales × clones de la Magnate × iniciaciones: Primero las obligadas; si nadie está libre, las opcionales no se hacen ni piden clon; antes una iniciación que un clon | RepartoSantuariosTest | 113 | — | — | 16cb73e, 6a8b400, 19740d1 |
| CV-11 | CV | Clon de la Magnate × misión con herramienta (alimentador): El clon con el alimentador da de comer | AlimentadorTest | 105 | — | — | 0cd78a8, 4299722 |
| CV-12 | CV | PopulationPlan × crías, icebergs y otras especies: Vivos ≤ K × humanos en el total que cuenta; el [ECO] lo dice | RepartoSantuariosTest | 113 | — | — | PB-151, g113 |
| CV-13 | CV | CheapMaterialsPass (editor) × CheapMaterialsSweeper (juego) × constructores de microcosmos × emisión: Mates también en juego y tras reconstruir un microcosmos | PeticionesSondaTest | 117 | — | — | 46f92ea, 42b0def, PB-152 |
| CV-14 | CV | NavAgentSafe × alfombra, Línea, muelle aéreo, en brazos: El agente vuelve a encenderse sobre el NavMesh | JornadaTest | 50 | — | — | 63de4fd |
| CV-15 | CV | RemoteStaff (santuario 2) × TestRunner × jornada: Con las pruebas en marcha no hay nadie apagado; sin pruebas, la jornada manda | JornadaTest | 50 | — | — | g50, Lizmibet y Linduarda |

## OP · Reglas operativas (operacion.md)

| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |
|---|---|---|---|---|---|---|---|
| OP-09 | OP | Dejar siempre los docs y el checklist al día | — | — | — | PB-045 | auditoría #45 |
| OP-01 | OP | Regla operativa OP-01 (operacion.md) | — | — | W3 | — | qa-propuesta.md §7 |
| OP-02 | OP | Regla operativa OP-02 (operacion.md) | — | — | — | — | qa-propuesta.md §7 |
| OP-03 | OP | Regla operativa OP-03 (operacion.md) | — | — | W7 | — | qa-propuesta.md §7 |
| OP-04 | OP | Regla operativa OP-04 (operacion.md) | — | — | — | — | qa-propuesta.md §7 |
| OP-05 | OP | Regla operativa OP-05 (operacion.md) | — | — | — | — | qa-propuesta.md §7 |
| OP-06 | OP | Regla operativa OP-06 (operacion.md) | — | — | W5, W6 | — | qa-propuesta.md §7 |
| OP-07 | OP | Regla operativa OP-07 (operacion.md) | — | — | — | — | qa-propuesta.md §7 |
| OP-08 | OP | Regla operativa OP-08 (operacion.md) | — | — | W3 | — | qa-propuesta.md §7 |

## Bugs y huecos

`B-nn` = fallo con dueño; `HC-nn` = hueco o decisión pendiente. Salen de un `Hn` de [hallazgos.md](hallazgos.md).

| ID | Tipo | Qué | Casos | Dueño | Estado | Hallazgo |
|---|---|---|---|---|---|---|
| B-01 | Fallo | g50: fauna atascada > 20 s, dando vueltas o volcada. Falla en todas las rondas que corrieron g50 desde r71 (crónico) | CV-03, CV-05, ARB-86, JOR-19, JOR-22, JOR-39, JOR-45, JOR-48, JOR-49, REN-06, REN-07 | probador → trabajador de fauna | Abierto | H1 |
| B-02 | Fallo | El script de marcas vació docs/checklist.md y docs/auditoria-peticiones.md (82878fc): leía un temporal que no existía | OP-04 | probador | Arreglado en 1bdfe1c; falta el aviso automático (QaCheck §6.9) | H2 |
| B-03 | Fallo | Ficheros sueltos en Unity tras cambiar de rama: TmpCheck y .cs que ya no están en git | OP-05 | probador | Arreglado con sync --prune (r83, r84); falta el aviso automático (QaCheck §6.10) | H3 |
| HC-01 | Hueco | Valores de los bonusPacks de Irosene, Lizmibet y Linduarda: los decide Berón | PB-140 | Berón | Abierto | — |
| HC-02 | Hueco | Afinar con OpenStreetMap los huecos históricos (la parte abierta de PB-082, L282) | PB-082 | sin dueño | Abierto | — |
| HC-03 | Hueco | ¿«Alicia» es el nombre real de Alicita? Se quitó del rótulo por defecto (P24); docs/personajes/alicita.md | — | Berón | Abierto | — |
| HC-04 | Hueco | Hora de la suite diaria (nivel N4) acordada con Berón (Unity es compartido) | — | Berón | Abierto | — |
