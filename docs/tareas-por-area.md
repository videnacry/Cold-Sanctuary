# Tareas por área: a la vez, seguidas y opcionales

Berón, 2026-11-08: un área puede tener **varias tareas a la vez**.
- Hospital: la iniciación de recién llegados y la guardia.
- Restaurante: la caja, tomar la comanda, cocinar, servir y limpiar.
- Yoga: la clase de Hatha y después la de Ashtanga.

**Con varias personas, se reparten. Con una sola, ella las hace todas seguidas, sin un clon de la Magnate por puesto.**

Código: `WorkRoster` (el reparto por santuarios: [jornada-y-rotacion.md](jornada-y-rotacion.md) §Reparto por santuarios) y `ConstructionProject.ShiftOf` (la obra). Prueba: grupo 113.

## Cómo se reparte (2026-11-08)

1. Las horas de cada sitio se reparten **por igual** entre sus humanos. Cada tarea va a quien menos lleva (partes iguales, ≤ 5 h, nada a mañana).
2. **Los puestos a la vez** (las misiones con `group`: la clase de yoga, el equipo de una obra) se dan a personas distintas **a la misma hora** si hay bastantes libres (`AssignGroups`). Si no, cada papel es una tarea más y **una persona puede encadenarlos**. Ya no se queda ningún papel para un clon por faltar gente.
3. **La obra cuenta turnos, no personas.** Antes el día de obra contaba cuando habían trabajado `crewNeeded` personas distintas. Ahora cuenta cuando se han hecho los `crewNeeded` turnos (#1, #2, #3), los haga quien los haga.
4. **Un clon solo cuando las horas no caben.** Hay dos casos:
   - Las horas del sitio pasan de lo que cubren sus humanos con partes iguales.
   - Un área se queda sin nadie que pueda hacerla (de otro sitio o sin saber, tras las iniciaciones).
5. **Encajar las tareas:** a igualdad de rotación va primero la tarea más larga. Las franjas son de 2, 2 y 1 h. Con la de 1 h delante no cabían dos de 2 h, y salía un clon con horas de sobra. Ahora 1 + 2 + 2 caben en las 5 h de una persona (113).

## Las áreas

**Leyenda.**
- **A la vez**: puestos que en la vida real funcionan simultáneamente. En el código solo lo fuerzan los `group`. Los demás se reparten como tareas.
- **Seguidas**: se hacen una detrás de otra.
- **Opcional**: candidata de la auditoría. **Berón (2026-11-08) decidió que una opcional, si ningún humano tiene tiempo, ese día no se hace**: nunca lleva un clon y no pasa a mañana. Solo se aplica a las de `WorkRoster.OptionalIds` (§Las opcionales aplicadas); las demás candidatas quedan fuera por ser cuidado de seres vivos, comida, obras o seguridad.

Horas del catálogo fijo (`AreaFunctions`); entre paréntesis, lo que se añade según el día. **Ob.** = obligatoria.

### Hospital (s1): 15 h (+ refugio de aves 0,5)

| Tarea | h | Ob. | Tipo | Opcional |
|---|---|---|---|---|
| `enf_iniciacion` Iniciación de recién llegados | 1 | sí | **a la vez** con la guardia | — |
| `hos_urgencias` Guardia de urgencias | 2 | sí | **a la vez** (siempre alguien de guardia) | — |
| `enf_visita` Pasar visita y curas | 2 | sí | seguida | — |
| `hos_quirofano` Quirófano (cirujano + anestesista) | 2 | | a la vez, dos papeles en una misión | **sí**: cirugía programada, se puede aplazar |
| `hos_uci` Cuidados intensivos | 2 | | seguida | — (si hay pacientes, no) |
| `hos_veterinaria` Hospital veterinario | 2 | | seguida | — |
| `enf_animales` Análisis y vacunas de animales | 1 | | seguida | **sí**: rutina que aguanta un día |
| `enf_inventario` Limpieza, esterilización, banco de sangre | 1 | | seguida | — |
| `hos_plantas` Remedios de plantas | 1 | | seguida | **sí**: complemento, no urgente |
| `hos_pediatria` Pediatría | 1 | | seguida | **sí**: consulta programada |

### Cocina / restaurante (s1): 10 h (+ elaboraciones 0,5–2 h cada una)

| Tarea | h | Ob. | Tipo | Opcional |
|---|---|---|---|---|
| `coc_servicio` Cocinar la carta | 2 | sí | **a la vez** (el servicio) | — |
| `coc_atender` Atender la barra (servir) | 2 | sí | **a la vez** (el servicio) | — |
| `coc_sala` Atender a los turistas en sala | 1 | | **a la vez** (el servicio) | **sí**: solo si hay turistas |
| `coc_limpieza` Limpieza e inventario de la cocina | 1 | sí | seguida (tras el servicio) | — |
| `coc_comedor` Limpieza del comedor y los aseos | 1 | sí | seguida | — |
| `coc_obrador` Obrador (pan, quesos, fermentos) | 2 | | seguida | — (lo pide la carta) |
| `coc_cuentas` Las cuentas (la caja) | 0,5 | | seguida | **sí**: se puede hacer otro día |
| `coc_avisos` Avisos y radio | 0,5 | | seguida | **sí**: comunicación, no imprescindible |
| `prep_*` Elaboraciones del día | 0,5–2 | | seguidas | — (lo pide la despensa) |

### Sala de Yoga (s1, todos sus pisos): 6 h

| Tarea | h | Ob. | Tipo | Opcional |
|---|---|---|---|---|
| `yog_guia` Guiar la clase de Hatha | 1 | sí | **a la vez** (`clase_yoga`) | — |
| `yog_demo` Demostrar | 1 | sí | **a la vez** (`clase_yoga`) | — |
| `yog_corrige_1` y `_2` Corregir posturas | 1 + 1 | sí | **a la vez** (`clase_yoga`) | **sí, el segundo corrector**: con poca gente, la clase sale con uno |
| `yog_ashtanga` Preparar la clase de Ashtanga | 0,5 | | **seguida** (después del Hatha) | — |
| `yog_sala` Preparar y limpiar las salas | 1 | | seguida | — |
| `yog_aseos` Aseos de los pisos de yoga | 0,5 | | seguida | — |

### Construcción (s1): 3,5 h (+ por obra abierta: 1 h de material y 2 h por turno de su equipo; bahareque 1)

| Tarea | h | Ob. | Tipo | Opcional |
|---|---|---|---|---|
| `obra_trabajo_*` (#1…#n) Trabajar la obra | 2 cada turno | sí | **a la vez** (equipo `obra_*`); con poca gente, **seguidos** (cuenta por turnos) | — |
| `obra_material_*` Llevar el material del día | 1 | sí | seguida (antes del equipo) | — |
| `con_proyectos` y `con_proyectos2` Dirigir y coordinar los proyectos | 1 + 1 | | seguidas | **sí, la segunda**: con una dirección basta un día |
| `con_almacen` Inventario del almacén | 0,5 | | seguida | **sí** |
| `rep_*` Reparación del día | 1 | | seguida | — |
| `mat_bahareque` Barro con paja | 1 | sí | seguida | — |

### Las demás áreas de la tierra (s1): todo seguido

| Área | h | Tareas (las obligatorias en negrita) | Opcional |
|---|---|---|---|
| Huerto | 5 (+ rondas de frutales 2 h cada una, guadua 2) | **riego 2**, cosecha 1, visitantes 1, trasplante 1; **rondas** (cada una, de un jardinero; varias a la vez si hay gente) | **visitantes** (solo con visitas), **trasplante** (semanal) |
| Guardia | 6 | **patrulla 2**, **alimentador 2** (a la vez con la patrulla), alimentador 2.ª plaza 2 | **segunda plaza del alimentador** |
| Sala de guardia | 2 (+ lectura del árbol cazador 1) | **turno de sala 2** | — |
| Mecánica | 4,5 | vehículos 2, maquinaria 1, alfombras 1, limpieza 0,5 | **vehículos** (si no hay averías) |
| Estudio Textil | 3,5 (+ cuerda 1) | tejer 2, coser 1, limpieza 0,5 | **tejer** |
| Carne Cultivada | 4 (+ micelio 1) | **biorreactores 1**, **iniciar lote 1**, **cosechar 1**, limpieza 1 | — |
| Cría | 4 | **tomas 1**, **recintos 1**, vínculo con una cría 2 | **vínculo** (es ocio con sentido) |
| Gimnasio | 1,5 (+ planes del entrenador 1) | limpieza 0,5, mantenimiento 1 | **mantenimiento** (semanal) |
| Química | 1,5 | síntesis 1, residuos 0,5 | **síntesis** (investigación) |
| Biología | 2 (+ fotobiorreactor 1) | cultivo 1, PCR 1 | **PCR** (investigación) |
| Informática | 2,5 | sistemas 1, refrigeración 1, energía 0,5 | — (el clúster no espera) |
| Aula | 5 | clase 1, estudio 1, legal 1, lenguas 1, letras 1 | **estudio**, **letras**, **lenguas** (formación) |
| Sala de asambleas | 2,5 | orden del día 1, acta 0,5, mediar 1 | **mediar** (solo si hay un desacuerdo) |
| Música | 2,5 | grabar 1, ensayo 1, equipo 0,5 | **grabar**, **ensayo** (cultura) |

### Marino (s2): la planta nuclear y la Línea

| Área | h | Tareas | Tipo | Opcional |
|---|---|---|---|---|
| Planta Atómica | 5 | operar 1, isótopos 1, ronda 1, ciclotrón 1, experimentos de física 1 | seguidas | **experimentos de física** |
| Central Nuclear | 4 (+ simulacro 1 semanal; iceberg y alarma de la roca 2, **ob.**) | turno 1, vigilancia 1, pasivos 1, roca 1 | la **vigilancia** y el **turno**, a la vez en la vida real; aquí seguidas | — (seguridad) |
| Línea de las cavernas | 0 (bajada o refugio 2; muestras, raciones y simulacro del árbol cazador 1) | — | seguidas | — |

### Aéreo (s3)

| Área | h | Tareas | Opcional |
|---|---|---|---|
| Control de misión | 5 (+ iceberg 2, vaciado 1, **ob.**) | control 1, seguridad 1, fábrica 2, seguimiento 1 | **fábrica** (sin lanzamiento cercano) |
| Energía | 2 (+ eólica o mareal 1, almacén 0,5, instalar 2, reparar 1, disuasión 1, iceberg 2, parada 1) | **mantenimiento de paneles 1**, control 0,5, ronda de aves 0,5 | **instalar** (las mesas pueden esperar), **disuasión** |

## Las opcionales aplicadas (Berón 2026-11-08)

**Regla:** primero se reparten por igual las obligadas. Después, las opcionales van a los humanos que aún tienen tiempo en sus 5 h, a quien menos lleva primero. Las que no caben **ese día no se hacen**: nunca un clon, nunca a mañana. La jornada lo escribe cada día: `[Reparto] Día N · opcionales omitidas: …`.

**Aplicadas** (`WorkRoster.OptionalIds`; una obligatoria nunca lo es):
- **Cuentas y avisos:** `coc_cuentas`, `coc_avisos`.
- **Asamblea:** `asa_mediar`, solo si hay un desacuerdo.
- **Mantenimiento que puede esperar:** `mec_vehiculos`, `tex_tejer`, `gym_mantener`, `sol_instalacion` (las mesas nuevas esperan; los paneles se mantienen siempre), `lanz_fabrica`.
- **Investigación:** `qui_sintesis`, `bio_pcr`, `fis_experimentos`.
- **Formación:** `aul_estudio`, `aul_letras`, `aul_lenguas`.
- **Cultura:** `son_grabar`, `son_ensayo`.

**Candidatas de la auditoría que NO se aplican** (la regla de Berón: nunca seguridad, cuidado de seres vivos, comida ni obras):
- **Pacientes y animales:** `hos_quirofano`, `hos_pediatria`, `hos_plantas`, `enf_animales`, `cri_vinculo`, `gua_alimentador_2`, `ene_disuasion` (protege a las aves).
- **Comida:** `coc_sala`, `hue_visitantes`.
- **Plantas:** `hue_transplante`.
- **Obras:** `con_almacen`, `con_proyectos2`.
- **Papeles de la clase de yoga:** el segundo corrector, que es obligatorio.

**Los clones bajan.** Con 5 h obligadas y 2,5 h opcionales para una persona, antes salía 1 clon (las opcionales contaban como obligadas). Ahora salen 0 y se omiten las 3 opcionales (113). En el juego, la línea `[Reparto] … opcionales omitidas: … · clones: N` da la cuenta de cada día.
