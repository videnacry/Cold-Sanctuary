# Pendientes de Berón e índice de documentos

Escrito el 2026-11-18 para **repartir el trabajo en hilos nuevos y cortos**. Recoge lo que llevaba el hilo
«Pruebas en vivo en Unity» (el probador en el PC de Berón) y lo que dicen el sistema de QA y el checklist.
No sustituye a nada: la fuente de verdad sigue siendo [qa/peticiones.md](qa/peticiones.md) (estado por petición, PB-nnn),
[qa/registro.md](qa/registro.md) (estado por caso) y [checklist.md](checklist.md) (tablero). Este archivo es la puerta de entrada.

**Cómo usarlo:** cada fila de las tablas es un encargo que cabe en un hilo propio. Al abrir el hilo, pásale la fila y
los documentos de la columna «Leer». Cuando se termine, se actualiza su PB en `qa/` y se quita o tacha aquí.

Ramas (2026-11-18): la integración es `claude/edificios` (por delante de `master`, que acaba en el PR #306).
`claude/escala-mitad` y `claude/ui-codeship` ya están fusionadas en `edificios`. El PR #307 (docs para ahorrar tokens:
`CLAUDE.md` corto, `docs/README.md`, `docs/ARQUITECTURA.md`, `CHANGELOG.md`) está abierto contra `master`.

---

## 1. En curso en el hilo de pruebas (prioridad alta)

| # | Tarea | Estado | Rama | Leer |
|---|---|---|---|---|
| 0 | **Rehacer DESDE CERO el distrito de Hawthorne 2008** (PB-165, Berón 2026-11-19 06:01): el piloto se veía mal (taquería = cubo sin ventanas, sillas humanas junto a sillas de insecto-humano, pocas mesas; mercado = caja gris con dos mesas en medio de la nada; edificios aislados). Sin reutilizar el piso del árbol-edificio: primero un distrito denso y sin huecos tipo GTA (edificios contiguos, calles, aceras, aparcamientos, paradas de bus, árboles, postes), luego retos de código y jornadas. **Escala (Berón 11-19 06:21): puertas, paredes y ventanas ×3; muebles y objetos ×2 en X y Z, con la altura de las superficies de uso como mucho al pecho del insecto-humano; el cuerpo no cambia.** Cada edificio investigado (para qué sirve y qué necesita) y con detalle al menos como el restaurante, el gym y el hospital del santuario 1. Muebles, objetos y puertas ×2–×3 con la misma separación; en sitios de insecto-humano, solo mobiliario de insecto-humano; ventanas, interiores, materiales y colores reales | En curso (hilo «Rehacer distrito Hawthorne desde cero»). Plan: [hawthorne-distrito.md](hawthorne-distrito.md). Hecho: suelo propio, calles con su mobiliario y 25 edificios con interior (compila; 1.ª ronda en vivo 51/53, arreglada en fa9bf517: paso, puertas, casas, flora, fachadas); la fábrica agrandada a la escala nueva (88 × 60 m, muros 27 m) con el distrito abierto a su alrededor. Falta: 2.ª ronda en vivo, más detalle dentro de la fábrica y quitar el mobiliario humano del resto de microcosmos. Sustituye a la fila 1 y a la parte de Hawthorne de la fila 8 | `claude/project-thread-6zuhz4` → `edificios` | [microcosmos-musk-historia.md](microcosmos-musk-historia.md) §4 |
| 1 | **Piloto Elon Musk (Hawthorne 2008)** (PB-160): más detalle en la nave de SpaceX (máquinas y piezas, colores y materiales reales, gente trabajando, comedor); revisar la captura del interior que sale toda cian; rehacer, capturas y subir la web | En curso. Fases 0/0b y piloto hechos y en verde (1115 + 885), fusionados en `edificios` (860224a). Dos pasadas de detalle hechas; hacen falta más porque aún se ve vacío. Web en [ElonMusk](https://videnacry.github.io/Cold-sanctuary-web/microcosmos/ElonMusk/). Berón aún no ha dicho «me siento dentro» | `claude/escala-mitad` → `edificios` | [plan-microcosmos-realismo.md](plan-microcosmos-realismo.md) §4.1 fase 1 |
| 2 | **Plan de realismo, fases 2–4** (PB-160): fase 2, prueba en vivo de las necesidades básicas en el piloto (dormir, retrete, comer, trabajar un día con `Fallbacks = 0`; `NeedsDriver` para los personajes de historia); fase 3, pilotos Descartes (Leiden 1637) y Atahualpa (Cajamarca 1532); fase 4, despliegue uno a uno al resto (NavMesh, muebles, puertas ≥ 1,2 m, muros con vista en corte, sin escaleras bajo 12 m) | Pendiente. Aprobado: insecto-humano a la mitad (s = 0,5) con patas recogidas, de pie con apoyo de abdomen, vuelven muros y techos | — | [plan-microcosmos-realismo.md](plan-microcosmos-realismo.md) §3–§5 |
| 3 | **Retos de código en el resto de microcosmos** (PB-161): unos 5 por microcosmos con datos reales | Pendiente. La UI de CodeShip (menú árbol, Tareas con «Ir» y viaje automático, ventana de Áreas, retos, iniciación) está hecha y fusionada; CodeShipLogicTest g119 en verde; Musk tiene dos retos | `claude/ui-codeship` → `edificios` | plan §4 |
| 4 | **Web: lo que queda** (PB-156): rehacer los 7 microcosmos sin la fauna recolocada (Bolivar, Egina, IrenaSendler, LeChambon, PlantaAtomica, SalaYoga, Scene0_SakshiRio); fallos reales de la prueba en la página: Goku a 3,0 m (escala), soldados de Tregua 1914 bajo el talud, cocinero de 2,5 m de Cleopatra (revisar), umbral de movimiento bajo en BillGates; que la prueba juzgue también escala y realismo | Pendiente. La web está hecha: [santuario + 61 microcosmos](https://videnacry.github.io/Cold-sanctuary-web/) en 3 builds gzip, una página por microcosmos con `?prueba=1` en Edge sin ventana; cada subida rehace el historial del repo de Pages (Berón, 11-16) | `claude/web-continua` | [web-prueba-hallazgos.md](web-prueba-hallazgos.md), [web-continua.md](web-continua.md), [web-ligera.md](web-ligera.md) |
| 5 | **Rondas automáticas** (PB-158): la cadena «ronda continua» corre con commits nuevos en `peticiones-sonda`, `reparto-santuarios`, `escala-mitad` y `ui-codeship`; el aviso del escritorio, cada 30 min, resuelve choques solo de docs/qa, relanza y avisa al móvil de errores que bloquean | Hecho (11-16, ampliado el 11-18); vigilar que no se pare (se paró el 11-15, 11-16 y 11-18) | — | [qa/operacion.md](qa/operacion.md) OP-07 |
| 6 | **Guardias desactivadas** para todos (PB-159, `WorkRoster.guardShifts` apagado) hasta una lógica mejor; los clones de la Magnate ya no salen en el restaurante (0 medidos) ni cuentan como comensales | Hecho y probado (g113, r127). El alimentador de depredadores se queda (D4, 11-18) | `edificios` | [alimentador.md](alimentador.md) |
| 7 | ~~**Fallo crónico de fauna (B-01)**~~: resuelto con la primera batería limpia, 4770/0 (r127, 2026-11-18). Siguen inestables H6 (animales dentro de muebles) y H5 (paseo sin salto) | Vigilar que B-01 no vuelva (volvió varias veces) | — | [qa/casos.md](qa/casos.md) «Bugs y huecos», [qa/hallazgos.md](qa/hallazgos.md) |
| 8 | **Musk recrea su historia** (PB-162): cada fragmento en su lugar real y fechado, un barrio con todo el detalle (edificios, interiores, muebles, maquinaria, utensilios, calles, vegetación, animales) y la ciudad de fondo; luego el resto, cuando Berón lo apruebe | En curso. Alcance (Berón 11-18 21:54): por ahora solo Hawthorne 2008, todo el esfuerzo en su realismo; Musk es la portada, pero se ven los trabajadores y científicos de los cohetes, la producción de comida, la seguridad urbana y los demás sistemas (animales, personas, bacterias, plantas). Omelek: programado pero aparcado | — | [plan-microcosmos-realismo.md](plan-microcosmos-realismo.md) |
| 9 | **Retos de código para cada actividad** (PB-163): cocinar, curar, plantar, campo, entrenar, yoga, cuidar animales…, con UI/UX como CodeShip | En curso | — | plan §4 |
| 10 | **bonusPacks** (PB-164, PB-140): el bonusPack N suma las stats de la criatura más poderosa del santuario N | En curso | — | [progresion-santuarios.md](progresion-santuarios.md) |
| 11 | **Móvil: mandos Switch, menús como CodeShip y Grimorio** (PB-167): los 16 botones de los Joy-Con sobre la pantalla (sobre `GameKeys`, el teclado sigue igual), el tema de menús `CsTheme` y (11-20) el **Grimorio** (R / F8): todos los hechizos en una ventana, cada uno se lanza resolviendo su reto de código | Mandos y tema: compilan, g141 8/0 (11-20). Grimorio: en código, falta compilar y g141 + prueba en el móvil. Luego: `CsTheme` en el resto de paneles, botones apagados cuando no hacen nada, botón para Atacar | `claude/project-thread-n17elb` (PR #310) | [mandos-tactiles.md](mandos-tactiles.md) |

## 2. Decisiones que esperan a Berón

| # | Decisión | Dónde |
|---|---|---|
| ~~D1~~ | Decidido el 11-18: el bonusPack N suma las stats de la criatura más poderosa del santuario N (PB-164, cierra HC-01) | [qa/peticiones.md](qa/peticiones.md) |
| ~~D2~~ | Decidido el 11-18: «Alicia» es el nombre real y el del juego (cierra HC-03) | [personajes/alicita.md](personajes/alicita.md) |
| D3 | Hora de la suite diaria de pruebas, porque Unity es compartido (HC-04) | [qa/operacion.md](qa/operacion.md) |
| ~~D4~~ | Decidido el 11-18: la Magnate que alimenta a los depredadores se queda (PB-159) | §1 fila 6 |
| ~~D5~~ | Decidido el 11-18: un repo de web por microcosmos solo si dejan de caber juntos o si ayuda al rendimiento (hoy 181 MB de 1 GB) | [web-continua.md](web-continua.md) |

## 3. Peticiones abiertas (falta código)

| PB | Fecha | Petición | Leer |
|---|---|---|---|
| PB-038 | 11-03 | Crear los pisos representativos que faltan: biblioteca u observatorio, deportes, electricidad, sendero, templo | [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md), [manual-de-diseno.md](manual-de-diseno.md) |
| PB-082 | 11-04 | Afinar con mapas actuales (OpenStreetMap, solo posiciones) los huecos históricos (HC-02) | [microcosmos-mapas-a-mano.md](microcosmos-mapas-a-mano.md), [microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md) |
| PB-084 | 11-04 | Guardería o juego para Neo, con Ivelyna | [personajes-mesocosmos.md](personajes-mesocosmos.md) |
| PB-107 | 11-05 | Que las turbinas no maten aves (cría de aves, figuras, dragón de drones, aves que avisan) | [energia.md](energia.md) |
| PB-155 | 11-09 | ¿Por qué pesa más que GTA 5? 395 ms por fotograma en scripts: medir y bajar | [web-ligera.md](web-ligera.md), [known-issues.md](known-issues.md) |
| PB-160 | 11-18 | Realismo de los microcosmos: fases 2–4 (§1 filas 1–2) | [plan-microcosmos-realismo.md](plan-microcosmos-realismo.md) |
| PB-161 | 11-18 | Retos de código en el resto de microcosmos (§1 fila 3) | plan §4 |
| PB-166 | 11-19 | Todo en el microcosmos por retos, también los diálogos: misiones de amistad, ser estudiante, combatir a su lado o proteger; los retos de diálogo enseñan la historia real del escenario (primero en Hawthorne). Hecho en Hawthorne: 4 misiones de vínculo + 2 diálogos en la historia (falta probar en vivo y extender al resto de microcosmos) | [hawthorne-distrito.md](hawthorne-distrito.md) §4.5 |

## 4. Programado pero sin verificar en vivo

Están en código; falta una prueba con run log. Lista completa y al día en [qa/registro.md](qa/registro.md)
(el 2026-11-18: 49 casos `Solo código`, 10 `Sin probar`, 3 `Inestable`, 1 `Crónico`).

| PB | Petición |
|---|---|
| PB-003 | Alfombras de 4 tipos y rutas de NPC por `CarpetPad.Route` |
| PB-005 | Todo microcosmos sale de la plantilla (fecha, hora, piso) |
| PB-009 | Octógonos con megacolumna CFST por esquina para 30 pisos, con aviso si se pasa |
| PB-017 | Todos los edificios en el modelo árbol |
| PB-019 | Árbol guardián (retirado; confirmar que no queda nada) |
| PB-022 | 12 figuras históricas en sus microcosmos (Newton, Einstein, Tesla, Curie…) |
| PB-023 | Un hogar para cada personaje del pueblo, más trabajo y servicios |
| PB-026 | Muros históricos que pide la historia (gueto, reactor) — ahora encaja con el plan de muros |
| PB-045 | Docs y checklist siempre al día |
| PB-059 | Las abejas del santuario deslumbran |
| PB-064 | Gancho W9: al cambiar piso/área/árbol/microcosmos, revisar jornadas, reparto y horario libre |
| PB-092 | Fernando y las tareas solares en la jornada |
| PB-110 | Mejorar los microcosmos viejos (lo cubre el despliegue del plan de realismo) |
| PB-119 | Frases con fuente (verificada o inventada) |
| PB-120 | Techos de cristal y rampas permitidos |
| PB-121 | «Hazlo real»: el hielo flota, la roca llega al lecho, refugio en el fondo |
| PB-125 / PB-128 | Refugio para un mes y el refugio que crece con cinco obras (probar g113 y ver `[Refugio] Obra abierta`) |
| PB-134 | Quitar los árboles artificiales |
| PB-138 | Focos brillantes en vivo: revisar medusas y todo |
| PB-154 | Quitar las sombras o las columnas de los árboles |
| PB-156 | Web ligera: hecha y probada con `?prueba=1` en Edge, fuera del TestRunner (falta run log) |
| PB-160 / PB-161 | Piloto Musk (MIC-84…88) y CodeShip (CS, g119): en verde, falta apuntarlo en el registro con su run log |

## 5. Pendientes de diseño de largo plazo (sin fecha)

Del [checklist.md](checklist.md) §«Siguiente (código)» y §«Decisiones abiertas»: alma compartida (compartir pensamientos),
posesión real (interactuar con el cuerpo poseído), más históricos humanos y animales, Enfermería «Una Salud», resto de
Upa-yoga, transformar la Cocina en reencarnaciones, rebanadas de stats-as-truth. Y en [qa/peticiones.md](qa/peticiones.md)
las PB `Doc` (reglas ya escritas en el manual): PB-063, 096, 098, 099, 106, 111, 130.

---

## 6. Índice de documentos

Generado el 2026-11-18 desde `claude/edificios`. El título es el primer `#` de cada archivo. Los 53 run logs de
[qa/runs/](qa/runs/) no se listan uno a uno. Cuando se fusione el PR #307 habrá además `docs/README.md` (índice comentado)
y `docs/ARQUITECTURA.md`.

**Por dónde empezar:** [PENDIENTES.md](PENDIENTES.md) (este) · [qa/README.md](qa/README.md) · [checklist.md](checklist.md) ·
[manual-de-diseno.md](manual-de-diseno.md) · [CODEMAP.md](CODEMAP.md) · [plan-microcosmos-realismo.md](plan-microcosmos-realismo.md).

### Archivos del proyecto (fuera del repo, en la carpeta compartida de Claude)

La mayoría son copias de docs del repo. Solo existen allí:
- `plan-microcosmos-realismo-20261118T060700-5424.md`: revisión del plan del 2026-11-18 (ya está igual en [plan-microcosmos-realismo.md](plan-microcosmos-realismo.md)).
- `notes/herramientas-ia.md`: skills, MCP y plugins para Cold Sanctuary (2026-10-20).
- `patches/kitchen-step-spells.patch`: parche de los pasos de cocina como hechizos.

Copias en la carpeta: abejas-y-panal, alfombras-plan, body-shape-from-stats, carne-cultivada, columnas-sismicas,
diseno-personajes, entrenamiento-areas, huertos-y-cultivos, luz-microcosmos, manual-de-diseno, medidas-muebles,
obras-dojo-ballet, plano-edificios, plan-microcosmos-realismo, notes/historias-por-area. **Si difieren, manda la del repo.**

### Raíz (2)

| Documento | Título |
|---|---|
| [CLAUDE.md](../CLAUDE.md) | Cold Sanctuary |
| [DEVLOG.md](../DEVLOG.md) | Cold Sanctuary — Dev Log & Contexto de Diseño |

### docs/ (188)

| Documento | Título |
|---|---|
| [abejas-y-panal.md](abejas-y-panal.md) | Abejas de las nieves y su panal |
| [actividades-y-uniformes.md](actividades-y-uniformes.md) | Actividades de verdad, la orquesta de la comida y los uniformes |
| [alfombras-plan.md](alfombras-plan.md) | Plan: transporte con alfombras voladoras (sin escaleras ni puentes) |
| [alimentador.md](alimentador.md) | El alimentador: ronda de los depredadores |
| [anima-architecture.md](anima-architecture.md) | Arquitectura unificada: todo es un ser (Anima) + pilares + aptitudes |
| [anima-dissolving-animal.md](anima-dissolving-animal.md) | Disolver `Animal` → todo ser es un `Anima` + componentes |
| [animal-heroes.md](animal-heroes.md) | Animales-protagonista (historias reales) — históricos con POV animal |
| [anyma-factory-and-control.md](anyma-factory-and-control.md) | Anyma: factory por objeto-plantilla + filosofía de control |
| [apremios-guardian-observacion.md](apremios-guardian-observacion.md) | Apremios, el Guardián de stats y la Observación (ecuanimidad) |
| [arbol-guardian.md](arbol-guardian.md) | El árbol gigante: retirado, con su investigación (2026-11-04/05) |
| [architecture.md](architecture.md) | Arquitectura — Cold Sanctuary |
| [area-missions-spec.md](area-missions-spec.md) | Spec de Misiones por Área |
| [area-progression.md](area-progression.md) | Progresión de áreas — cómo se va formando la historia |
| [areas-en-obra.md](areas-en-obra.md) | Áreas en obra: solo lo básico queda terminado |
| [asana-system.md](asana-system.md) | Sistema de Asanas — Cold Sanctuary |
| [ashtanga-primary-series.md](ashtanga-primary-series.md) | Ashtanga — Serie Primaria (Yoga Chikitsa) = "nivel 1" |
| [atencion-visual.md](atencion-visual.md) | Atención visual y hechizo «enfocar» |
| [atomic-plant.md](atomic-plant.md) | Planta atómica (Mesocosmos) y su microcosmos de la física |
| [AUDIT-2026-07-09.md](AUDIT-2026-07-09.md) | AUDITORÍA DE CÓDIGO — 2026-07-09 |
| [auditoria-peticiones.md](auditoria-peticiones.md) | Auditoría de las peticiones de Berón (2026-10-27 → 2026-11-09) |
| [behavior-system.md](behavior-system.md) | Sistema de comportamiento animal |
| [bird-logic.md](bird-logic.md) | Lógica de pájaros (`BirdBehavior`) — estado actual y por qué probablemente hay que reemplazarla |
| [body-shape-from-stats.md](body-shape-from-stats.md) | La forma del cuerpo a partir de las stats físicas |
| [bond-activity-system.md](bond-activity-system.md) | Sistema de BondActivities — Cold Sanctuary |
| [camera-system.md](camera-system.md) | Sistema de Cámara — Cold Sanctuary |
| [capabilities-and-embodiment.md](capabilities-and-embodiment.md) | Capacidades = hechizos; el cuerpo = cableado (2026-08-23) |
| [carne-cultivada.md](carne-cultivada.md) | Carne cultivada: el laboratorio que alimenta a los depredadores |
| [casas-de-arbol.md](casas-de-arbol.md) | Las casas de árbol: guardadas para otro santuario (2026-11-05) |
| [casos-de-uso.md](casos-de-uso.md) | Casos de uso y cobertura de pruebas |
| [centros-para-todos.md](centros-para-todos.md) | Centros para todos los miembros a la vez (regla J16) |
| [character-andrea.md](character-andrea.md) | Andrea — uno de los dueños del santuario |
| [character-irosene.md](character-irosene.md) | Personaje — Irosene |
| [character-linduarda.md](character-linduarda.md) | Personaje — Linduarda |
| [checklist.md](checklist.md) | Checklist — continuar |
| [ciclo-de-vida.md](ciclo-de-vida.md) | Ciclo de vida: los procesos naturales de cada especie del santuario |
| [cocina-investigacion.md](cocina-investigacion.md) | Cocina de restaurante y talleres artesanales: datos de layout reales (m) |
| [cocina-profesional.md](cocina-profesional.md) | Cocina profesional del restaurante |
| [CODEMAP.md](CODEMAP.md) | CODEMAP — índice de clases del proyecto (GENERADO — no editar a mano) |
| [columnas-sismicas.md](columnas-sismicas.md) | Columnas sísmicas para las plataformas octogonales |
| [compassion-simulations.md](compassion-simulations.md) | Simulaciones de compasión |
| [consciousness-mechanics.md](consciousness-mechanics.md) | Mecánicas de CONSCIENCIA — el interés central del juego |
| [construccion-sostenible.md](construccion-sostenible.md) | Construcción sostenible: materiales y tiempos |
| [construcciones-elevadas.md](construcciones-elevadas.md) | Construcciones elevadas, energía solar y dormitorios |
| [construction-simulation.md](construction-simulation.md) | La Construcción (Mesocosmos) y Levantar refugio (Microcosmos) |
| [contencion-arbol-cazador.md](contencion-arbol-cazador.md) | El árbol cazador, bajo el agua (2026-11-05) |
| [conventions.md](conventions.md) | Convenciones y cómo trabajar aquí |
| [conversaciones.md](conversaciones.md) | Charlas entre personas (humanos e insectos-humanos) |
| [creature-stats.md](creature-stats.md) | Aptitudes de las criaturas — agilidad, percepción, fuerza, masa |
| [cria-simulation.md](cria-simulation.md) | El área de CRÍA — el corazón del santuario |
| [deshierbe-cuidadoso.md](deshierbe-cuidadoso.md) | Deshierbe con cuidado |
| [diseno-personajes.md](diseno-personajes.md) | Diseño de los personajes del santuario |
| [edificio-arbol.md](edificio-arbol.md) | El Árbol de los Campos — v2, la hélice cuadrada (Berón 2026-10-30) |
| [edificios-investigacion.md](edificios-investigacion.md) | Edificios de investigación y de producción |
| [emotion-model.md](emotion-model.md) | Modelo de emoción (deep-sim, para TODA Anima) (2026-08-05) |
| [energia.md](energia.md) | La energía del santuario (investigación → decisión → construcción, Berón 2026-11-05) |
| [entrenamiento-areas.md](entrenamiento-areas.md) | Edificio de entrenamiento: gimnasio, cardio, calistenia, parkour y crossfit |
| [environmental-navigation.md](environmental-navigation.md) | Navegación por lectura del entorno (impulsos) — 2026-08-25 |
| [era-species-behavior.md](era-species-behavior.md) | Comportamiento de las especies de las eras (microcosmos) |
| [eras.md](eras.md) | Eras: la fecha de cada microcosmos y las capacidades por época |
| [escenas-de-prueba.md](escenas-de-prueba.md) | Escenas de prueba |
| [fauna-fria-santuarios.md](fauna-fria-santuarios.md) | Fauna fría de los santuarios marino y aéreo |
| [fauna-gameplay.md](fauna-gameplay.md) | Fauna — Diseño de Gameplay |
| [forge-simulation.md](forge-simulation.md) | La Mecánica (Mesocosmos) y la Forja (Microcosmos) |
| [fotobiorreactores.md](fotobiorreactores.md) | Fachada de fotobiorreactores del Árbol de los Campos (investigación → plan → construcción) |
| [founding-trio-stories.md](founding-trio-stories.md) | El grupo fundador — historias y misiones (Señor del Fuego · Nasatya · La Sembradora) |
| [gameplay-loops.md](gameplay-loops.md) | Mecánicas de Jugabilidad y Loops de Enganche — Cold Sanctuary |
| [gaps-vs-planteamiento.md](gaps-vs-planteamiento.md) | Brechas: "funciona" ≠ "cumple el planteamiento" |
| [garden-simulation.md](garden-simulation.md) | El Huerto — segunda simulación (Neolítico / agricultura) |
| [gimnasio-maquinas.md](gimnasio-maquinas.md) | Máquinas del gimnasio: cómo son de verdad y cómo usarlas en el entrenamiento |
| [grupos-de-animales.md](grupos-de-animales.md) | Grupos de animales: peces, kril, medusas y abejas (Berón 2026-11-03) |
| [guerra-de-almohadas.md](guerra-de-almohadas.md) | Camas de 4 y guerra de almohadas en la arboleda residencial |
| [hielo-y-respiraderos.md](hielo-y-respiraderos.md) | Vida bajo el hielo y respiraderos |
| [historias-por-area.md](historias-por-area.md) | Historias por área: Mecánica, Enfermería, física y grandes figuras (propuesta 2026-10-22) |
| [horario-libre.md](horario-libre.md) | Horario de actividades libre (semanal) |
| [hospital-plan.md](hospital-plan.md) | Hospital: programa de espacios real y redistribución (v2: anillos de camas y capacidad ×4) |
| [hospital.md](hospital.md) | El Hospital del santuario (antes Enfermería) |
| [huertos-y-cultivos.md](huertos-y-cultivos.md) | Huertos y cultivos del santuario |
| [ibody-imind.md](ibody-imind.md) | IBody / IMind / IBondable — Estado actual |
| [ice-sanctuary-ecology.md](ice-sanctuary-ecology.md) | Ecología del Santuario de Hielo (Santuario 1) — 2026-09-01 |
| [iniciacion-medica-investigacion.md](iniciacion-medica-investigacion.md) | Iniciación médica — investigación |
| [insect-biology-audit.md](insect-biology-audit.md) | Auditoría de biología de los insectos (2026-10-21) |
| [insect-morphology.md](insect-morphology.md) | Morfología de insectos y arañas para el cuerpo de cubos |
| [interfaz-botones.md](interfaz-botones.md) | La pila de botones y sus ventanas (2026-11-05) |
| [jardines-frutales.md](jardines-frutales.md) | Frutales en los techos de alfombras (investigación → plan → construcción) |
| [jornada-y-rotacion.md](jornada-y-rotacion.md) | Jornada laboral, rotación de misiones e iniciación |
| [kitchen-simulation.md](kitchen-simulation.md) | La Cocina — primera simulación jugable (nivel de referencia) |
| [known-issues.md](known-issues.md) | Bugs y deuda técnica conocida |
| [laboratorios-investigacion.md](laboratorios-investigacion.md) | Laboratorios de investigación y planta nuclear de producción |
| [lanzadera-de-cohetes.md](lanzadera-de-cohetes.md) | La lanzadera de cohetes (investigación → plan → construcción, Berón 2026-11-04) |
| [learning-unlocks.md](learning-unlocks.md) | Desbloqueo por aprendizaje — elementos y posturas |
| [living-entity.md](living-entity.md) | LivingEntity — base compartida para animales y NPCs |
| [luz-microcosmos.md](luz-microcosmos.md) | Luz por fecha, hora y lugar (microcosmos y santuario polar) |
| [magic-metabolism-progression.md](magic-metabolism-progression.md) | Metabolismo mágico y progresión por santuarios (2026-08-07) |
| [magic-plane-and-meditation.md](magic-plane-and-meditation.md) | Microcosmos, Máquina de Virtualización, Avatares y Meditación |
| [manual-de-diseno.md](manual-de-diseno.md) | Manual de diseño de Cold Sanctuary |
| [mascaras-de-oxigeno.md](mascaras-de-oxigeno.md) | Máscaras de oxígeno, puertas de 8 personas y fondo abierto de los edificios-árbol (Berón 2026-11-03) |
| [medidas-muebles.md](medidas-muebles.md) | Medidas de muebles frente al humano más alto |
| [microcosmos-actividades.md](microcosmos-actividades.md) | Etiquetas de actividad de los edificios de los microcosmos (2026-11-03) |
| [microcosmos-areas.md](microcosmos-areas.md) | El microcosmos de cada área |
| [microcosmos-biomas.md](microcosmos-biomas.md) | Biomas de los microcosmos: flora y fauna nativa de cada era y región |
| [microcosmos-bruce-lee.md](microcosmos-bruce-lee.md) | Microcosmos de Bruce Lee: el Dojo (2026-10-31) |
| [microcosmos-cientificos.md](microcosmos-cientificos.md) | Microcosmos de científicos e históricos (2026-11-01) |
| [microcosmos-civilizaciones.md](microcosmos-civilizaciones.md) | Microcosmos de las eras civilizadas: solo insectos, edificios reales y funciones reutilizables (2026-11-01) |
| [microcosmos-dungbeetle-level.md](microcosmos-dungbeetle-level.md) | Microcosmos — el nivel de los escarabajos peloteros (Momo & Medea) + mecánicas que habilita |
| [microcosmos-enfermeria.md](microcosmos-enfermeria.md) | Microcosmos de la Enfermería: la muerte de Ambrosio |
| [microcosmos-entrenamiento-eras.md](microcosmos-entrenamiento-eras.md) | Lugares de entrenamiento de cada época (microcosmos, 2026-11-04) |
| [microcosmos-eras-and-observation.md](microcosmos-eras-and-observation.md) | Microcosmos — especie por era, mito naturalizado y eventos que forjan la observación |
| [microcosmos-famosos.md](microcosmos-famosos.md) | Microcosmos de los famosos (2026-11-04) |
| [microcosmos-fauna-eras.md](microcosmos-fauna-eras.md) | Fauna de cada era: los insectos-humanos nuevos |
| [microcosmos-historias-3.md](microcosmos-historias-3.md) | Microcosmos de historias, tanda 3 (2026-11-05) |
| [microcosmos-historias.md](microcosmos-historias.md) | Microcosmos de historias (2026-10-22) |
| [microcosmos-insects.md](microcosmos-insects.md) | El Microcosmos como mundo de INSECTOS — propuesta (2026-07-30) |
| [microcosmos-level1.md](microcosmos-level1.md) | Microcosmos — Nivel 1 (El alba / la cueva): consolidación + fichas |
| [microcosmos-mapas-a-mano.md](microcosmos-mapas-a-mano.md) | Los mapas a mano de cada historia (2026-11-04) |
| [microcosmos-maya-pakal.md](microcosmos-maya-pakal.md) | Microcosmos maya: Pakal de Palenque, el rey que renace como el maíz (2026-11-03) |
| [microcosmos-musica.md](microcosmos-musica.md) | Microcosmos de la música (2026-11-05) |
| [microcosmos-myrmidons.md](microcosmos-myrmidons.md) | Egina, el reino de los mirmidones (microcosmos de la Construcción) |
| [microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md) | Los pueblos de los microcosmos desde su mapa real (2026-11-03) |
| [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) | Reglas de edificación de los microcosmos y mesocosmos (2026-11-01) |
| [microcosmos-sakshi-origin.md](microcosmos-sakshi-origin.md) | Microcosmos — Origen de Sakshi (el PRIMER viaje de Kushal, era pre-Ambrosio) |
| [microcosmos-suelo-y-elementos.md](microcosmos-suelo-y-elementos.md) | El suelo y los elementos de los microcosmos (2026-11-04) |
| [mind-model.md](mind-model.md) | Modelo de mente / IA de personajes (emergencia social) |
| [mission-mode.md](mission-mode.md) | Modo misión, disparadores y economía de recursos del santuario |
| [mob-characters.md](mob-characters.md) | Personajes-mob (lista inicial) |
| [mob-epochs-matrix.md](mob-epochs-matrix.md) | Matriz de épocas × hilos — héroes a través del tiempo |
| [mob-quests-early.md](mob-quests-early.md) | Misiones e historia — era temprana (Edad de Piedra → Metales) |
| [mob-world-architecture.md](mob-world-architecture.md) | Arquitectura del Mundo Mob (santuario fractal) |
| [natural-interactions.md](natural-interactions.md) | Interacciones naturales: qué hace cada especie y con qué (2026-10-22) |
| [novela.md](novela.md) | La novela de Cold Sanctuary — narración íntegra (2026-07-31) |
| [obras-dojo-ballet.md](obras-dojo-ballet.md) | Obras del edificio de Yoga: Sala de Ballet (P1) y Dojo (P2) |
| [observation-system.md](observation-system.md) | Sistema de Observación — Cold Sanctuary |
| [personajes-mesocosmos.md](personajes-mesocosmos.md) | Personajes del mesocosmos (santuario): quién vive aquí y quién se fue a su microcosmos |
| [pijamadas.md](pijamadas.md) | Pijamadas en las casas del árbol |
| [pipeline-3d-models.md](pipeline-3d-models.md) | Pipeline de modelos 3D — Cold Sanctuary |
| [piso-asambleas.md](piso-asambleas.md) | Sala de asambleas (2026-11-05) |
| [piso-aula.md](piso-aula.md) | Piso del Aula: clases y sala de estudio para todos (2026-11-04) |
| [piso-informatica.md](piso-informatica.md) | Piso de Informática: el clúster de IA local del santuario (2026-11-04) |
| [piso-musica.md](piso-musica.md) | Área de Música: escenarios, estudios y salas de ensayo (2026-11-05) |
| [plan-microcosmos-realismo.md](plan-microcosmos-realismo.md) | Plan: microcosmos con escala, edificios y jugabilidad de CodeShip |
| [plano-edificios.md](plano-edificios.md) | Plano de edificios: cuatro torres temáticas |
| [produccion-maquinaria.md](produccion-maquinaria.md) | La maquinaria de las áreas de producción |
| [progresion-santuarios.md](progresion-santuarios.md) | Progresión entre santuarios: poder, jefes, magia y bonusPacks |
| [qa-propuesta.md](qa-propuesta.md) | Propuesta: sistema de QA y hallazgos para Cold Sanctuary |
| [refuge-and-adult-behavior.md](refuge-and-adult-behavior.md) | Refugio, ocultarse y comportamiento adulto (diseño) |
| [reglas-animales.md](reglas-animales.md) | Reglas para crear animales del santuario |
| [reglas-personajes.md](reglas-personajes.md) | Reglas para crear personajes del mesocosmos |
| [reglas-plataformas.md](reglas-plataformas.md) | Reglas de las plataformas (pisos, anillos y microcosmos) |
| [rendimiento.md](rendimiento.md) | Rendimiento del santuario |
| [residencia.md](residencia.md) | La Residencia, un edificio-árbol en la isla (2026-11-05) |
| [restaurant-plan.md](restaurant-plan.md) | Restaurante: distribución real y propuesta (cocina en toda la planta, comedor en anillo) |
| [review-checklist.md](review-checklist.md) | Review checklist — Cold Sanctuary |
| [revision-arquitectura-shopify.md](revision-arquitectura-shopify.md) | Revisión de arquitectura: qué aprender de la app Shopify «mahou-location-control» |
| [sala-de-guardia.md](sala-de-guardia.md) | La sala de guardia (investigación → plan → construcción, 2026-11-05) |
| [sanctuary-second-lap-and-fear.md](sanctuary-second-lap-and-fear.md) | Segunda vuelta por los santuarios + el miedo como efecto de hechizo |
| [sanctuary1-level-and-live-tests.md](sanctuary1-level-and-live-tests.md) | Santuario 1 (Nivel 1 jugable) + pruebas en vivo — estado y plan |
| [setup-body-system.md](setup-body-system.md) | Setup — Sistema IBody / Stats por Extremidad |
| [setup-camera-system.md](setup-camera-system.md) | Setup — Sistema de Cámara |
| [sitio-nuclear.md](sitio-nuclear.md) | El sitio nuclear (investigación → decisión → construcción, Berón 2026-11-05) |
| [soul-composition-blend.md](soul-composition-blend.md) | Alma por MEZCLA: cuerpos y mentes con dominio (%) |
| [soul-relations-reincarnation.md](soul-relations-reincarnation.md) | Alma (avanzado): conversión por distribución · relaciones por especie · pensamientos por capacidad · reencarnación compartida |
| [spell-genesis-and-sanctuary-progression.md](spell-genesis-and-sanctuary-progression.md) | Génesis de hechizos + progresión por santuarios (misiones, cocina, planta atómica) |
| [spells-suprahechizo-and-thoughts.md](spells-suprahechizo-and-thoughts.md) | Suprahechizos, cuenta de pensamientos, enseñanza y la Magnate |
| [stats-as-truth.md](stats-as-truth.md) | Los stats como fuente de verdad (2026-08-05) |
| [submarino-de-obra.md](submarino-de-obra.md) | El submarino de obra y las medidas de la vía marina (Berón 2026-11-03) |
| [submarinos-medicos.md](submarinos-medicos.md) | Los submarinos del hospital: la ambulancia anfibia y el Leviatán (Berón 2026-11-03) |
| [tareas-del-jugador.md](tareas-del-jugador.md) | Tareas del jugador con «E» (el modelo de Ambrosio) |
| [tareas-por-area.md](tareas-por-area.md) | Tareas por área: a la vez, seguidas y opcionales |
| [territorio-polar.md](territorio-polar.md) | Territorio polar: la isla de tierra y los casquetes vivos |
| [testing-checklist.md](testing-checklist.md) | Checklist de pruebas (Mesocosmos: progresión, farming, recursos, trepar, cocina) |
| [testing-recent-features.md](testing-recent-features.md) | Checklist de pruebas — features recientes (para el compi de Unity) |
| [transporte-alfombras.md](transporte-alfombras.md) | Transporte entre plataformas: puentes y alfombras voladoras |
| [typed-spells-and-queues.md](typed-spells-and-queues.md) | Hechizos por teclado + colas de acción (jugabilidad tipo Sims) |
| [ui-following-arrays.md](ui-following-arrays.md) | UI / FollowingArrays |
| [ui-holographic-menu.md](ui-holographic-menu.md) | UI — Menú Holográfico (Menú → Magia / Yoga / Sistema) |
| [unity-editor-manual.md](unity-editor-manual.md) | Manual de Unity — qué recrear a mano (por si se pierde el proyecto Unity) |
| [upa-yoga-mission.md](upa-yoga-mission.md) | La 1ª virtualización de YOGA — upa-yoga de cuello (2026-07-31) |
| [veracidad-frases.md](veracidad-frases.md) | Veracidad de las frases (regla M39, 2026-11-05) |
| [vet-hospital-plan.md](vet-hospital-plan.md) | Hospital veterinario: programa real y propuesta |
| [veterinaria-investigacion.md](veterinaria-investigacion.md) | Hospital veterinario de zoo / fauna polar — datos de planificación |
| [vida-en-icebergs.md](vida-en-icebergs.md) | Vida en los icebergs |
| [volition-selection-engine.md](volition-selection-engine.md) | Motor de volición: la selección conduce la acción (D3b) — 2026-08-24 |
| [web-continua.md](web-continua.md) | Web continua: una página por microcosmos, prueba en la página y subidas (2026-11-16) |
| [mandos-tactiles.md](mandos-tactiles.md) | Móvil: mandos tipo Switch (Joy-Con en pantalla) y menús con el tema de CodeShip (2026-11-19) |
| [web-ligera.md](web-ligera.md) | Web ligera: dos builds WebGL para el móvil (2026-11-13) |
| [web-prueba-hallazgos.md](web-prueba-hallazgos.md) | Hallazgos de la prueba en la página (rweb2, 2026-11-17) |
| [world-simulation.md](world-simulation.md) | Sistema de Simulación del Mundo — Cold Sanctuary |
| [world-topology-and-planes.md](world-topology-and-planes.md) | Topología del mundo y planos (Microcosmos / Macrocosmos) |
| [yoga-asanas-spec.md](yoga-asanas-spec.md) | Ficha de asanas (fuente de verdad anatómica) |
| [yoga-pisos.md](yoga-pisos.md) | Los pisos de yoga: Hatha y Ashtanga, occidental y tradicional (2026-11-04) |

### docs/personajes/ (41)

| Documento | Título |
|---|---|
| [adaguchi.md](personajes/adaguchi.md) | Adaguchi |
| [adonis.md](personajes/adonis.md) | Adonis |
| [alicita.md](personajes/alicita.md) | Alicita (Alicia) |
| [amykhuz.md](personajes/amykhuz.md) | Amykhuz (antes Amahuz) |
| [andrea.md](personajes/andrea.md) | Andrea |
| [billgates.md](personajes/billgates.md) | Bill Gates |
| [consol.md](personajes/consol.md) | Consol |
| [david.md](personajes/david.md) | David |
| [ducristin.md](personajes/ducristin.md) | Ducristin |
| [elonmusk.md](personajes/elonmusk.md) | Elon Musk |
| [fausto.md](personajes/fausto.md) | Fausto |
| [fernando.md](personajes/fernando.md) | Fernando |
| [gallarle.md](personajes/gallarle.md) | Gallarle |
| [gohageneis.md](personajes/gohageneis.md) | Gohageneis «el Papi» |
| [goku.md](personajes/goku.md) | Goku |
| [goluis.md](personajes/goluis.md) | Goluis |
| [ibrahimtraore.md](personajes/ibrahimtraore.md) | Ibrahim Traoré |
| [irosene.md](personajes/irosene.md) | Irosene |
| [ivelyna.md](personajes/ivelyna.md) | Ivelyna |
| [josedrilo.md](personajes/josedrilo.md) | Josedrilo |
| [julian.md](personajes/julian.md) | Julian |
| [karen.md](personajes/karen.md) | Karen |
| [kushal.md](personajes/kushal.md) | Kushal (el jugador) |
| [lale.md](personajes/lale.md) | Lale |
| [leo.md](personajes/leo.md) | Leo |
| [linduarda.md](personajes/linduarda.md) | Linduarda |
| [lizmibet.md](personajes/lizmibet.md) | Lizmibet «Liz» |
| [luciana.md](personajes/luciana.md) | Luciana |
| [mackenziescott.md](personajes/mackenziescott.md) | MacKenzie Scott |
| [magnate.md](personajes/magnate.md) | La Magnate |
| [maite.md](personajes/maite.md) | Maite |
| [malena.md](personajes/malena.md) | Malena |
| [neo.md](personajes/neo.md) | Neo |
| [nesvitor.md](personajes/nesvitor.md) | Nesvitor |
| [panterilia.md](personajes/panterilia.md) | Panterilia |
| [README.md](personajes/README.md) | Fichas de personajes |
| [sadhguru.md](personajes/sadhguru.md) | Sadhguru |
| [shalva.md](personajes/shalva.md) | Shalva |
| [tote.md](personajes/tote.md) | Tofelite «Tote» |
| [warrenbuffett.md](personajes/warrenbuffett.md) | Warren Buffett |
| [yulo.md](personajes/yulo.md) | Yulo |

### docs/qa/ (8)

| Documento | Título |
|---|---|
| [casos.md](qa/casos.md) | Catálogo de casos |
| [contadores.md](qa/contadores.md) | Contadores por ronda |
| [entrelazamiento.md](qa/entrelazamiento.md) | Entrelazamiento: el estado de Unity que comparten varios procesos |
| [hallazgos.md](qa/hallazgos.md) | Hallazgos (Hn) |
| [operacion.md](qa/operacion.md) | Reglas operativas (OP) |
| [peticiones.md](qa/peticiones.md) | Peticiones de Berón (PB) |
| [README.md](qa/README.md) | Sistema de QA de Cold Sanctuary |
| [registro.md](qa/registro.md) | Registro de estados |

### docs/conocimiento/ (1)

| Documento | Título |
|---|---|
| [README.md](conocimiento/README.md) | Conocimiento: lo que volverá a morder |
