# Reglas operativas (OP)

Las reglas del PC, de Unity, de las ramas y de las cadenas del probador. Salen de lo vivido (qa-propuesta.md §7). Cada una es un caso
del área OP en [casos.md](casos.md), con estado `No aplica` en el registro: no se prueban en Unity, las mira el probador en cada run log.

| ID | Regla | Por qué | Dónde se ve | Regla del manual |
|---|---|---|---|---|
| OP-01 | **Un solo probador fusiona y prueba.** El run log anota las ramas fusionadas con su SHA (`git merge-base --is-ancestor`, como en `cadena83.sh`). | Varias manos probando a la vez mezclaban ramas y resultados. | Campo «Fusionadas» del run log | W3 |
| OP-02 | **`unity.lock` con dueño y hora.** Unity es compartido con Berón: se pregunta antes de una suite. | Las pruebas abrían el editor de Berón. | `unity.lock`; HC-04 (hora de la suite diaria) | — |
| OP-03 | **Toda espera tiene tope** y deja «PARO: motivo» en el run log. **Nunca se deja Play encendido.** | Esperas sin fin colgaban la cadena (TIMEOUT de 600 s esperando el «Domain Reload», r13, r67, r68). | Campo «Interrupciones» | W7 |
| OP-04 | **Las cadenas se pueden reanudar**: un paso hecho deja su marca en `estado/` y, al reiniciar, la cadena salta lo ya hecho. Si el PC se reinicia, el run log dice «Interrumpido en <paso>». **Un script que escribe un doc comprueba que su entrada existe y que el doc no encoge** antes de escribirlo. | El script de marcas vació el checklist y la auditoría (H2, B-02). | Carpetas `estado*/` del probador; QaCheck §6.9 | — |
| OP-05 | **Sincronizar con `--prune`**: cada archivo que sobra en Unity sin estar en git (TmpCheck y .cs de otras ramas) se lista en el run log. **Nunca se commitea `TmpCheck`.** | Tras cambiar de rama sobraban 99 .cs y un TmpCheck (H3, B-03): un .cs de una clase retirada rompe la compilación. | Campo «Sobrantes»; QaCheck §6.10 | — |
| OP-06 | **Memoria por debajo de ~11 GB**: se reconstruye por partes, un microcosmos cada vez, y se guarda la SampleScene. | Un Blockout entero reinició el PC de 16 GB. | Campo «Tiempo» (Unity GB) | W5, W6 |
| OP-07 | **El watchdog de cada hora se vuelve a crear en cada sesión.** | Se perdía al cerrar la sesión y nadie vigilaba la cadena. | Memoria del probador | — |
| OP-08 | **Los trabajadores programan, compilan y escriben su caso y su test en rojo. No marcan nada como `Probado`.** | Programado no es hecho (regla del 2026-11-10). | registro.md: solo el probador escribe `Probado (…)` | W3 |
| OP-09 | **Toda petición de Berón se anota el mismo día** (en [peticiones.md](peticiones.md) y, como tarea, en el checklist) y **solo cuenta como hecha cuando está verificada en vivo** (sus casos en `Probado`). | Lo pedido del 11-09 no se anotó (PB-045, auditoría del 2026-11-10). | peticiones.md; el comprobador calcula el estado | — |
