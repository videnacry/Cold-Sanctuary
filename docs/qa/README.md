# Sistema de QA de Cold Sanctuary

Aprobado por Berón el 2026-11-13. Diseño completo y por qué: [qa-propuesta.md](../qa-propuesta.md).

La idea en una línea: **una petición de Berón está hecha cuando sus casos están probados, con su run log**, y eso lo calcula una
máquina (`QaCheck`), no una persona que marca `[x]`. Programado no es hecho.

## Dónde va cada cosa

| Fichero | Qué tiene | Quién lo escribe |
|---|---|---|
| [peticiones.md](peticiones.md) | `PB-nnn`: cada petición de Berón, el día que llega, con sus reglas y sus casos | quien recibe la petición |
| [casos.md](casos.md) | El catálogo de casos por área (`FAU-03`, `JOR-12`…), los `CV`, los `OP` y la tabla «Bugs y huecos» (`B-nn`, `HC-nn`) | el trabajador, con su test en rojo |
| [registro.md](registro.md) | El estado de cada caso, con fecha, commit y run log; y «Inestables y crónicos» | **solo el probador** |
| [runs/](runs/) | Un run log por ronda de Unity, con la [PLANTILLA.md](runs/PLANTILLA.md) | el probador |
| [contadores.md](contadores.md) | Una fila por ronda: atascos de la fauna, `[ECO]`, FUGA, TIMEOUT, FPS, memoria, duración | el probador |
| [hallazgos.md](hallazgos.md) | `Hn`: lo que se vio y aún no se sabe qué es. Acaba en un `B-nn` o un `HC-nn` | cualquiera |
| [entrelazamiento.md](entrelazamiento.md) | El estado de Unity que comparten varios procesos (agente, reloj, franjas, topes…) y sus casos `CV` | quien añade un proceso |
| [operacion.md](operacion.md) | Las reglas del PC, Unity, ramas y cadenas (`OP-nn`) | el probador |
| [../conocimiento/](../conocimiento/README.md) | Lo que «volverá a morder»: síntoma, medido, cómo se evita, dónde | cualquiera |
| [migracion/](migracion/) | Los scripts que generaron peticiones, casos, registro y los run logs retro (2026-11-13) | — |

Se quedan donde estaban: las reglas, en el [manual de diseño](../manual-de-diseno.md) (S, T, M, J, W…); el tablero de trabajo, en el
[checklist](../checklist.md); y la [auditoría](../auditoria-peticiones.md) del 2026-11-10, como histórico (sus filas son las PB-001…PB-155).

## La cadena de IDs

`PB-nnn` (petición) → regla del manual (`J1`, `S5`…) → caso (`JOR-03`) → unidad y grupo (`JornadaTest`, g50) → run log → `Hn` →
`B-nn` (fallo) o `HC-nn` (hueco o decisión pendiente).

- **PB**: el número de la auditoría con tres cifras (#152 → `PB-152`). La siguiente libre es `PB-156`.
- **Casos**: 2–3 letras de área + 2 cifras. **No se renumeran ni se borran**: un caso que ya no vale pasa a `Retirado`.
- **En la prueba**, el ID va al principio del texto del `TestProbe.Check`: `"[CV-03] la fauna con viaje…"`. Así sale en la columna
  `fallos` de `Logs/test-results.tsv` y el FAIL se liga solo a su caso. Un test que espera un arreglo nombra su `B-nn`.

| Área | Qué | Grupos (TestRunner) | Reglas del manual |
|---|---|---|---|
| FAU | fauna, hábitos, viajes, ecología | 1–14, 16, 23, 29, 71, 85, 99, 102, 105–109, 111 | E, A |
| JOR | jornada, reparto, clones, horario libre, iniciaciones, restaurante | 28, 41, 45, 46, 50, 65, 89, 92, 97, 104, 113 | J, W9 |
| YOG / GYM | yoga / gimnasio | 15, 42, 51, 64 / 49, 87 | J |
| ARB | árboles, edificios, plataformas, alfombras, frutales, obras | 25, 27, 53–57, 70, 73–75, 80, 91, 98, 103 | S, T, C, G |
| MIC | microcosmos, mapas a mano, historias | 43, 44, 58–63, 66–69, 76–79, 86, 115, 116 | M, F |
| HOS / NUC / ENE | hospital / nuclear, refugio, santuarios 2 y 3 / energía | 81–84 / 90, 94, 100, 110, 114 / 101 | H |
| PER / UI | personajes y charlas / interfaz | 21, 22, 24, 26, 52, 88, 95 / 93 | P |
| REN | rendimiento, materiales, arnés de pruebas, la sonda de peticiones | 0, 17, 20, 112, 117 | W12–W15 |
| PX | recorridos en vivo (días en vivo, Kushal, fotos) | 40, 96 | — |
| CV | convivencia de procesos ([entrelazamiento.md](entrelazamiento.md)) | los de cada fila | — |
| OP | reglas operativas ([operacion.md](operacion.md)) | — | W3–W7 |

## Estados

**De un caso** (en [registro.md](registro.md)): `Probado (suite gN)` · `Probado (en vivo)` · `Probado (retro)` (solo de la migración,
con run log reconstruido) · `Parcial` · `FAIL` · `Inestable` (pasa y falla en el mismo commit) · `Crónico` (el mismo check falla en
≥ 3 rondas seguidas: pide `B-nn` y dueño) · `Solo test` · `Solo código` (programado y compilado, sin ejecutar) · `Sin probar` ·
`No aplica` · `Retirado`.

- **Sin run log no hay «Probado»**, y sin commit tampoco.
- **Una evidencia caduca** si `TestImpact` dice que un commit posterior toca su grupo: el comprobador la marca «a revisar» (no la borra).

**De una petición** (en [peticiones.md](peticiones.md), **calculado**, nadie lo escribe a mano): `Hecha` si todos sus casos están
`Probado (…)`; `Doc` si solo pedía un doc o una regla; `Sin verificar` si alguno está en `Solo código` o `Sin probar`; `Abierta` si
tiene un `HC` o le falta código.

## El flujo

1. **Llega una petición** (el mismo día, regla del 2026-11-10): fila nueva en `peticiones.md` con la siguiente PB libre, la fecha, una
   cita corta, sus reglas y sus casos. Si es una regla, va también al manual.
2. **Casos**: el siguiente ID libre de su área en `casos.md`. Se mira [entrelazamiento.md](entrelazamiento.md): por cada estado
   compartido que toque, un caso `CV` contra los escritores que ya estaban, **en la misma rama**. En el registro entran como `Sin probar`.
3. **El trabajador** escribe el test en rojo con `[ID]`, lo arregla y compila: `Solo código`. No marca nada `Probado` (OP-08).
4. **El probador** corre su nivel (abajo), deja el run log en `runs/` y pone `Probado (…)` con fecha, commit y enlace. Copia los
   contadores a `contadores.md`.
5. **Si falla**: `Hn` en `hallazgos.md`, y de ahí `B-nn` con dueño. Si un contador sube más de un 50 % sobre la media de las 3 rondas
   anteriores, se abre un `Hn` aunque todo pase.
6. **Si Berón tiene que decidir algo**: `HC-nn`, y la PB queda `Abierta`.

## Niveles de prueba (minutos de este PC)

| Nivel | Qué corre | Tiempo | Cuándo |
|---|---|---|---|
| N0 `Comprobar QA` | `QaCheck` (sin Play) | segundos | cada commit (y en `verify-wt.sh`) |
| N1 rápida | `TestPresetsData.Rapida` + `QaCheckTest` | 2–3 min | cada ronda |
| N2 impacto | grupos de `TestImpact` sobre el diff + todos los CV cuyas áreas toquen esos grupos | 5–25 min | cada ronda con código |
| N3 vivo | «N días en vivo ×6», con contadores | 10–30 min | si toca fauna, jornada o árbol |
| N4 suite | todos los grupos | 1 h 20–1 h 36 | una vez al día (hora acordada con Berón: HC-04) y antes de fusionar a master |

## El comprobador (`QaCheck`)

Lo escribe otro trabajador (`Assets/Scripts/QaCheck.cs`, `QaCheckTest`, menú «Tools/Cold Sanctuary/Pruebas/Comprobar QA» y un
`qa-check.sh` sin Unity). Las reglas que mira están en [qa-propuesta.md §6](../qa-propuesta.md). Para que pueda leer estos ficheros:

- **Tablas con cabecera fija**, una fila por ID, el ID en la primera columna y sin `|` dentro de las celdas. Las cabeceras son:
  - peticiones: `| PB | Fecha | Petición | Reglas | Casos | Estado | Origen |`
  - casos: `| ID | Área | Qué tiene que pasar | Unidad | Grupo | Reglas | PB | Origen |`
  - bugs y huecos: `| ID | Tipo | Qué | Casos | Dueño | Estado | Hallazgo |`
  - registro: `| ID | Estado | Fecha | Commit | Run log | B/HC | Nota |`
  - inestables y crónicos: `| Check | Grupo | Estado | Rondas | B/HC | Dueño | Nota |`
- «—» es una celda vacía. Varias cosas en una celda van separadas por «, ».
- El run log se enlaza en Markdown: `[runs/<fichero>.md](runs/<fichero>.md)`.
