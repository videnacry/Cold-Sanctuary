# Propuesta: sistema de QA y hallazgos para Cold Sanctuary

Fecha: 2026-11-13. Solo análisis, sin cambios de código. Fuentes: el QA de CodeShip (`557805b`, rama
`ingravidez-flechas`: docs/qa/*, scripts/qa-check.mjs, docs/conocimiento/README.md), `origin/claude/edificios`
(`e35cea2`) y el scratchpad del probador (r70–r83.out, vivo82.log, cadena83.sh, tester-state.md,
analisis-pruebas.md, audit-asks.md).

## 0. Antes de nada: el checklist y la auditoría están vacíos en edificios

El commit `82878fc` («Auditoría y checklist: #1, #2, #4, #58 y #152 verificados en vivo con el grupo 117»)
dejó **`docs/checklist.md` (231 KB, 1322 líneas) y `docs/auditoria-peticiones.md` (66 KB) en 0 bytes**, y
`e35cea2` (la punta) sigue así. Hay que restaurarlos desde `82878fc^` y volver a aplicar las 5 marcas de ese
commit. Es justo el tipo de fallo que el comprobador de la §6 debe parar (un doc de QA que encoge > 50 %).

## 1. Qué se toma de cada lado

**De CodeShip (tal cual):**
- Catálogo `casos.md` con **IDs estables** (área + número), que no se renumeran ni se borran (se tachan).
- `registro.md` aparte: estado + fecha + commit + run log. **Sin run log no hay «Probado».**
- Un run log por ejecución en `runs/` con plantilla fija y un campo «Acción a mitad» (si no se cruzaron:
  «no reproducido», nunca OK).
- `hallazgos.md` con `Hn` → acaban en **`B-nn`** (fallo) o **`HC-nn`** (hueco o decisión pendiente).
- `entrelazamiento.md`: mapa de estado compartido → un caso **CV** por cada cruce de escritores.
- El ID en el título del test; tests que esperan un arreglo nombran su `B-nn`.
- `qa:check` que hace cuadrar catálogo, registro y tests.
- `docs/conocimiento/` para lo que «volverá a morder» (Síntoma / Medido / Cómo se evita / Dónde).

**De Cold Sanctuary (se conserva):**
- La **regla del 2026-11-10**: toda petición de Berón se anota el mismo día; programado ≠ hecho.
- La **auditoría numerada** de peticiones (#1–#155) y su clase A/B/C/D.
- El **manual de diseño** con IDs de regla (S, F, T, C, G, H, M, A, J, W, E, P…) y la columna «Se
  comprueba en (grupo N)». W1 («cada regla tiene su prueba») ya es la mitad del sistema.
- El arnés: `TestRunner` por grupos, `Sanear` y sus «FUGA», `Logs/test-results.tsv`
  (`grupo unidad pass fail segundos estado fallos`), el preset «rápida» (`TestPresetsData`, ~1 000 checks en
  2–3 min), `TestImpact` (archivos cambiados → grupos; «centros» → batería entera), `IFastGameTime`.
- Las rondas del probador: sync → compilar (`verify-wt.sh`) → rápida → grupos oficiales → «N días en vivo ×6»
  → FAIL completos, con marcas de estado para reanudar.

**Lo que ninguno tiene y nuestra historia pide:**
1. **Entrelazamiento de Unity**: dueños de `NavMeshAgent`, `timeScale`/`maximumDeltaTime`, franjas de
   la jornada, topes de `PopulationPlan`, materiales en editor y en juego, tallado del NavMesh.
2. **Inestables y crónicos**: g50 «fauna atascada» falló en **12 rondas seguidas** (r72–r83) sin `B-nn`
   ni dueño; nadie lo distinguía de lo intermitente.
3. **Contadores por ronda**: «atascado > 20 s» fue 12, 11, 9, 17, **27, 34**, 5, 6, 10, 7, 14 (r72–r83).
   El salto de r76–r77 coincide con el cambio del Warp (`52d2949`, «fuera el Warp»), pero solo se ve
   poniendo las rondas en fila.
4. **Reglas operativas** escritas como casos: un solo probador, `unity.lock`, esperas acotadas, cadenas
   reanudables, PC de 16 GB, archivos sueltos de `TmpCheck`, ramas mezcladas.
5. **Peticiones ligadas a casos**: una petición está hecha cuando sus casos están probados, y lo calcula una
   máquina, no una persona que marca `[x]`.

## 2. Estructura de ficheros

```
docs/qa/
  README.md            flujo, «dónde va cada cosa», niveles de prueba (§5)
  peticiones.md        PB-nnn: cada petición de Berón, el día que llega (sustituye a auditoria-peticiones.md)
  casos.md             catálogo por área + CV + tabla «Bugs y huecos» (B-nn, HC-nn)
  entrelazamiento.md   estado compartido de Unity y quién lo escribe (§4)
  registro.md          estado por caso (§3) + «Inestables y crónicos»
  contadores.md        una fila por ronda: [ECO], [Fauna], FUGA, TIMEOUT, FPS, memoria, duración
  hallazgos.md         Hn
  operacion.md         reglas del PC, Unity, ramas y cadenas (lo de §7, con su ID OP-nn)
  runs/PLANTILLA.md    run log de una ronda de Unity
  runs/AAAA-MM-DD-HHMM-rNN-<modo>.md   modo: rapida | grupos | suite | vivo | retro
docs/conocimiento/     unity-navmesh.md, unity-tiempo.md, editor-y-pc.md, ramas.md
Assets/Scripts/QaCheck.cs            lógica pura del comprobador (como TestImpact)
Assets/Scripts/Tests/QaCheckTest.cs  unidad de 0 s en la lista rápida
Assets/Editor/QaCheckTool.cs         menú «Tools/Cold Sanctuary/Pruebas/Comprobar QA»
```

`checklist.md` sigue siendo el tablero de trabajo (qué toca hacer), pero su sección «Pendiente de
verificar en vivo» pasa a ser un enlace a `registro.md`, filtrado por `Solo código` y `Sin probar`. Así un
estado no vive en dos sitios. `manual-de-diseno.md` sigue siendo la fuente de las reglas.

## 3. IDs y estados

**Cadena de IDs:** `PB-nnn` (petición) → regla del manual (`J1`, `S5`…) → caso (`JOR-03`) → unidad y grupo
(`JornadaTest`, g50) → run log → `Hn` → `B-nn` / `HC-nn`.

- **PB-nnn** = el `#n` de la auditoría, con tres cifras (#152 → PB-152). Las nuevas siguen desde PB-156.
- **Casos**: 3 letras + 2 cifras. Las letras no chocan con las de una letra del manual. Áreas y grupos:

| Área | Qué | Grupos (TestRunner) | Reglas del manual |
|---|---|---|---|
| FAU | fauna, hábitos, viajes, ecología | 1, 2, 4–9, 23, 71, 109, 111 | E, A |
| JOR | jornada, reparto, clones, horario libre | 50, 65, 113 | J, W9 |
| YOG / GYM | yoga / gimnasio | 15, 42, 51 / 49, 87 | J |
| ARB | árboles, plataformas, alfombras | 53, 57, 55 (frutales) | T, S, C, G |
| MIC | microcosmos, mapas a mano, historias | 43, 62, 63, 66–68, 115, 116 | M, F |
| HOS / NUC / ENE | hospital y submarinos / nuclear y refugio / energía | 81, 82 / 73, 90, 94, 100 / 101 | H, … |
| PER / UI | personajes y charlas / interfaz | 52, 95 / 93 | P |
| REN | rendimiento, lotes, materiales | 112, 117 (parte) | W12–W15 |
| CV | convivencia de procesos | los de cada fila | — |
| PX | recorridos en vivo («2 días en vivo ×6», Kushal, fotos) | 40 + vivo | — |
| OP | reglas operativas (§7) | — | W3–W7 |

- **En las pruebas**, el ID va al principio del texto del `TestProbe.Check`: `"[CV-03] la fauna con viaje…"`.
  Así sale en la columna `fallos` de `test-results.tsv` y el FAIL se liga solo a su caso. El grupo 117
  (`PeticionesSondaTest`) usa `[PB-152]`.
- **Estados del registro** (los de CodeShip, más tres de Unity):
  `Probado (suite gN)` · `Probado (en vivo)` · `Probado (retro)` (solo en la migración, con run log
  reconstruido) · `Parcial` · `FAIL` · **`Inestable`** (pasa y falla en el mismo commit) · **`Crónico`**
  (el mismo check falla en ≥ 3 rondas seguidas: pide `B-nn` y dueño) · `Solo test` · **`Solo código`**
  (programado y compilado, sin ejecutar: lo que antes era «coded») · `Sin probar` · `No aplica` · `Retirado`.
- **Una evidencia caduca** si `TestImpact` dice que un commit posterior toca su grupo. El comprobador la
  marca «a revisar» (no la borra). Es la lección de la velocidad del juego, el gimnasio y el Warp: un
  arreglo que pasa su grupo puede romper otro.
- **Estado de una PB, calculado**: `Hecha` si todos sus casos están `Probado (…)`; `Doc` si solo pedía un
  doc o una regla; `Sin verificar` si hay alguno en `Solo código`/`Sin probar`; `Abierta` si tiene un `HC`
  o le falta código. Nadie lo escribe a mano: lo comprueba el comprobador.

## 4. Entrelazamiento de Unity (primeras filas)

| Estado compartido | Escriben | Leen / sufren | Casos |
|---|---|---|---|
| `Time.timeScale`, `Time.maximumDeltaTime`, `captureDeltaTime` | `IFastGameTime` (tiempo de juego rápido, `5347100`), `GymUsabilityTest` (0,04), tiempo ×N del jugador, `Sanear` (restaura) | `NavMeshAgent`, andadores cinemáticos, puertas y disparadores, jornada | CV-01, CV-02 |
| Reloj del día / `GameTick` | director de la jornada, saltos de día en pruebas | jornada, horario libre (desde las 16), cena de las 19, crías, celo | CV-07, CV-09 |
| **Cuerpo y `NavMeshAgent` de un animal** (un solo dueño, `d44f7e8`) | deseo (Volition), hábito, ciclo de vida, viajes, `Forager`, `Fight`, `Locomotion.CorrectMedium`, Warp, `NavAgentSafe` | `NavTrace`, detector de atascos de g1/g50 | CV-03, CV-04, CV-05, CV-14 |
| Tallado del NavMesh | árbol que gira (obstáculo de la raíz, `2847127`, `473aaec`), paredes de la raíz, enlaces para trepar | fauna, crías (`PenWalker`, `76db8a8`), personas | CV-06 |
| Agenda de una persona (`WorkRoster`, franjas de la jornada) | reparto por santuarios, opcionales, clones de la Magnate, iniciaciones, ronda de frutales, clase de yoga, comidas (`MealRitual`), horario libre, `RemoteStaff`, `TestDuties` | jornada, detector de «dar vueltas», pruebas de yoga | CV-07, CV-08, CV-09, CV-10, CV-11, CV-15 |
| Topes de `PopulationPlan` (fauna = K × humanos, sin insectos en s1) | plan, crías, icebergs, cría en cautividad | `[ECO]`, g113, g111 | CV-12 |
| Materiales | `CheapMaterialsPass` (editor, al final del Blockout), `CheapMaterialsSweeper` (en juego), constructores de microcosmos, emisión (medusas) | g117, fotos de brillos | CV-13 |
| Lotes estáticos | `LoteEstatico` frente a clases que mueven piezas hijas (W12) | dibujado en Play | CV-16 |
| `TestRunner.Running` y Play | pruebas, jugador (W4), puerta de la enfermería, jornada | todo | CV-17 |

Regla para un proceso nuevo: si toma el agente, el reloj o una franja, se añade como escritor en su fila y
se escribe un caso CV contra el escritor que ya estaba, **en la misma rama**.

## 5. Niveles de prueba (los minutos son de este PC)

| Nivel | Qué corre | Tiempo | Cuándo |
|---|---|---|---|
| N0 `Comprobar QA` | `QaCheck` (sin Play) | segundos | cada commit (y en `verify-wt.sh`) |
| N1 rápida | `TestPresetsData.Rapida` + `QaCheckTest` | 2–3 min | cada ronda |
| N2 impacto | grupos de `TestImpact` sobre el diff + **todos los CV cuyas «Áreas» toquen esos grupos** | 5–25 min | cada ronda con código |
| N3 vivo | «N días en vivo ×6», con contadores | 10–30 min | si toca fauna, jornada o árbol |
| N4 suite | todos los grupos | 1 h 20–1 h 36 | una vez al día (hora acordada con Berón) y antes de fusionar a master |

## 6. Comprobador automático (equivale a `qa:check`)

`QaCheck.cs` es lógica pura sobre texto (lee los .md y los .cs, sin Unity), como `TestImpact`. Lo usan tres
sitios: `QaCheckTest` (lista rápida, falla si hay problemas), el menú del editor y un `qa-check.sh` del
probador (grep y awk) que corre sin Unity antes de compilar. Falla si:
1. una unidad citada en `casos.md` no existe en `Assets/Scripts/Tests` o su `int Group =>` no es el citado;
2. un caso no tiene fila en el registro, o el registro tiene un ID que no está en el catálogo;
3. una fila está en `Probado (…)` sin enlace a un run log que exista, o sin commit;
4. un caso CV no tiene ningún `Check` con `[CV-nn]`, o un `Check` cita un ID desconocido;
5. un `B-nn`/`HC-nn` citado no está en la tabla de bugs, o un caso en `Crónico` no tiene `B-nn`;
6. una PB no tiene casos ni la marca `Doc`, o su estado escrito no coincide con el calculado;
7. una regla del manual citada no existe, o una regla con «Se comprueba en grupo N» no tiene caso (aviso);
8. una unidad de `TestPresetsData` ya no existe;
9. **un doc de `docs/qa/`, el checklist o el manual encoge más de un 50 %** respecto al commit padre (lo
   comprueba el `.sh` con `git show HEAD^:`), o queda vacío. Es el caso de `82878fc`;
10. hay ficheros en `Assets/Editor/TmpCheck/` registrados en git (son del probador y no van al repo).

## 7. Reglas operativas (operacion.md, OP-nn; salen de lo vivido)

- **OP-01** Un solo probador fusiona y prueba (W3). El run log anota las ramas fusionadas con su SHA
  (`git merge-base --is-ancestor` como en `cadena83.sh`).
- **OP-02** `unity.lock` con dueño y hora; Unity es compartido con Berón: se pregunta antes de una suite.
- **OP-03** Toda espera tiene tope y deja «PARO: motivo» en el run log (W7). Nunca se deja Play encendido.
- **OP-04** Las cadenas se pueden reanudar: un paso hecho deja su marca en `estado/` y, al reiniciar, la
  cadena salta lo ya hecho. Si el PC se reinicia, el run log lo anota como «Interrumpido en <paso>».
- **OP-05** Sincronizar con `--prune`: cada archivo que sobra en Unity sin estar en git (TmpCheck) se lista
  en el run log. Nunca se commitea `TmpCheck`.
- **OP-06** Memoria por debajo de ~11 GB; reconstruir por partes, un microcosmos cada vez, y guardar la
  SampleScene (W5, W6).
- **OP-07** El watchdog de cada hora se vuelve a crear en cada sesión.
- **OP-08** Los trabajadores programan, compilan y escriben su caso y su test en rojo. No marcan nada como
  `Probado`.

## 8. Run log de una ronda (runs/PLANTILLA.md)

```
# rNN · <IDs> · AAAA-MM-DD HH:MM · <rapida|grupos|suite|vivo|retro>
Commit: <sha> · Rama: claude/edificios · Fusionadas: <rama@sha, …> · Ejecutor: probador (agente)
Escena: SampleScene <reconstruida (Blockout solo santuario) | sin reconstruir>; microcosmos rehechos: <…>
Preparación: <preset / onlyGroups / Temp/runner-groups.txt / días en vivo ×6 / estado puesto por código>
Tiempo: timeScale <x> · maxDelta <y> · FPS media <z> · Unity <GB> · duración <min>
Acción a mitad: <qué se cruzó y cuándo; «no reproducido» si no llegaron a cruzarse>
Resultado: OK | FAIL | Parcial · <PASS/FAIL por grupo, de test-results.tsv> · TIMEOUT: <…> · FUGA: <…>
Contadores: [ECO] vivos <n> (plan K×humanos <a>/<b>) · [Fauna] atascado>20 s <n> · da vueltas <n> ·
  volcado <n> · sin camino <n> · NavTrace cambios de camino <n> · [Charla]/[Cuerpo] <si aplica>
Sobrantes: <ficheros TmpCheck u otros que sobraban en Unity>
Interrupciones: <reinicio / PARO / reanudado desde estado/…>
Casos: <ID → OK/FAIL/no reproducido> · Hallazgos: <Hn o «-»>
## Fallos (filas FAIL de test-results.tsv, tal cual)
## Notas
```

El probador copia los contadores a `contadores.md` (una fila por ronda). Si un contador sube más de un 50 %
respecto a la media de las 3 rondas anteriores, se abre un `Hn` aunque todo pase.

## 9. Cómo entra una petición de Berón (el mismo día)

1. Fila nueva en `peticiones.md`: `PB-156 | 11-13 | cita corta | regla (nueva en el manual o la que ya hay) | casos | estado`.
2. Si es una regla, va también a su fila del manual (como hasta ahora).
3. Se escribe el caso o casos con el siguiente ID libre, se mira `entrelazamiento.md` y se añade un CV por
   cada estado compartido que toque. En el registro entran como `Sin probar`.
4. El trabajador escribe el test en rojo con `[ID]` y lo arregla: `Solo código` → el probador corre su
   nivel → `Probado (…)` con run log. Si falla, `Hn` → `B-nn`.
5. Si Berón tiene que decidir algo (por ejemplo #140, los bonusPacks), se abre un `HC-nn` y la PB queda
   `Abierta`.

## 10. Migración

**Paso 0.** Restaurar `checklist.md` y `auditoria-peticiones.md` desde `82878fc^` y volver a aplicar
#1, #2, #4, #58 y #152 (§0).

**Paso 1: peticiones.** Las 155 filas de la auditoría (el encargo dice 135, pero en `82878fc^` hay 155)
pasan a `PB-001…PB-155` con su clase:

| Clase en la auditoría | Filas | Qué pasa |
|---|---|---|
| Verificado en vivo | 116 | Cada una lleva su caso (el área sale de su regla del manual) y `Probado (retro)`, con un run log retro por ronda citada (suite 63 `d0542db`, rondas 13, 30, 34, 66–68, grupo 117 en r70–r83). Si un commit posterior toca su grupo, la evidencia se marca «a revisar» |
| Hecho (doc) | 8 | `Doc`, sin caso |
| Sin anotar | 15 | Caso nuevo cada una; `Solo código` si hay código (por ejemplo #3 `CarpetPad.Route`, #9 `StructuralLimits`) y `Sin probar` si no |
| Sin verificar en vivo (+1 con una parte abierta) | 11 | Caso con `Solo código`, el primero de la cola del probador. La parte abierta pasa a `HC` |
| Abierto | 5 | `Abierta`; #140 pasa a `HC` (decide Berón); #38, #84, #107 y #155 se quedan con un caso en `Sin probar` |

**Paso 2: casos del checklist.** Cada `- [ ]` o `- [x]` de «Pendiente de verificar en vivo» (secciones 1–4)
se convierte en caso. La línea `L` de `b0add6e` se guarda en la columna «Origen». Las reglas del manual con
«Se comprueba en grupo N» citan su caso; las de «solo en docs» de proceso (W2–W8) pasan a `OP`.

**Paso 3: primeros casos CV, sacados de fallos que tuvimos:**

| ID | Procesos | Qué tiene que pasar | De dónde sale |
|---|---|---|---|
| CV-01 | Tiempo de juego rápido (`IFastGameTime`, maxDelta 1) × puertas, disparadores y agentes | Ningún paso grande hace que un agente se salte una puerta o un disparador; el grupo que lo usa y los siguientes, igual de verdes | `5347100`, g50 y g40 |
| CV-02 | maxDelta 0,04 del gimnasio × el grupo siguiente | `Sanear` lo restaura; 0 «FUGA» de tiempo | `13a213a`, g49 |
| CV-03 | Warp de la fauna × viaje en marcha | Quitar o cambiar el Warp no sube «atascado > 20 s» respecto a la media de las 3 rondas anteriores | `52d2949`; r76–r77 dieron 27 y 34 |
| CV-04 | Pausa de otra conducta × velocidad de un viaje | Un viaje en marcha nunca queda con velocidad «pedida 0,0» | `52d2949` |
| CV-05 | Hábito × deseo × ciclo de vida × viajes | El cuerpo tiene un solo dueño (el deseo que gana); el hábito no cuenta como atasco | `d44f7e8`, `5d89067` |
| CV-06 | Árbol que gira × NavMesh × crías del recinto | La raíz vuelve a tallar el NavMesh tras cada paso; las crías del recinto andan sin NavMesh y no se quedan quietas | `2847127`, `76db8a8`, `4990e1c` |
| CV-07 | Jornada × ronda de frutales × detector de «dar vueltas» | La ronda de frutales no es «dar vueltas» | `f64fad3` |
| CV-08 | Jornada × clase de yoga | Las conductas que dejó la jornada se pausan en la clase | `e2c0c7c` |
| CV-09 | Jornada × horario libre (16 h) × cena (19 h) | La barra se vuelve a llenar a las 19 h; nadie se queda sin franja | `2d9a0aa`, `31df64e` |
| CV-10 | Reparto × opcionales × clones × iniciaciones | Primero las obligadas; si nadie está libre, las opcionales no se hacen ni piden clon; antes una iniciación que un clon | `16cb73e`, `6a8b400`, `19740d1` |
| CV-11 | Clon de la Magnate × misión con herramienta (alimentador) | El clon con el alimentador da de comer | `0cd78a8`, `4299722` |
| CV-12 | `PopulationPlan` × crías, icebergs y otras especies | Vivos ≤ K × humanos en el total que cuenta; el `[ECO]` lo dice | #151, g113 |
| CV-13 | Pasada de materiales del editor × barredor en juego × constructores × emisión | Mates también en juego y tras reconstruir un microcosmos | `46f92ea`, `42b0def`, #152 |
| CV-14 | `NavAgentSafe` × alfombra, Línea, muelle aéreo, en brazos | El agente vuelve a encenderse sobre el NavMesh | `63de4fd` |
| CV-15 | `RemoteStaff` (santuario 2) × `TestRunner` × jornada | Con las pruebas en marcha no hay nadie apagado; sin pruebas, la jornada manda | g50, Lizmibet y Linduarda |

**Paso 4: registro inicial.** g50 «fauna atascada» pasa a `Crónico` con su `B-01`, dueño y la tendencia de
`contadores.md` (r72–r83). Los dos FAIL de g1 de r77 («paseo sin salto posible», «gira ≥ 90°») pasan a
`Inestable` si pasaron en el mismo commit.

**Paso 5.** `QaCheck` + `QaCheckTest` en la lista rápida; `qa-check.sh` en la cadena del probador antes de
`verify-wt.sh`. La primera ronda con el sistema deja su run log y llena `contadores.md`.

**Orden y esfuerzo:** paso 0 (minutos) → `docs/qa/` con la plantilla, operacion.md y entrelazamiento (½ día) →
peticiones y casos (1 día; las PB se pueden generar con un script desde la tabla de la auditoría) →
QaCheck (½–1 día) → etiquetas `[CV-nn]` en los checks a medida que se tocan las pruebas.
