# Entrelazamiento: el estado de Unity que comparten varios procesos

Cada fila es un estado que escriben **dos o más** procesos. Cada cruce de escritores tiene su caso `CV` en [casos.md](casos.md).

**Regla para un proceso nuevo**: si toma el agente, el reloj, una franja de la jornada o un tope, se añade como escritor en su fila y se
escribe un caso CV contra el escritor que ya estaba, **en la misma rama**.

Los nombres de clase se comprobaron contra el código de `claude/edificios` el 2026-11-13 (fichero entre paréntesis). Lo que no es una
clase va marcado.

| Estado compartido | Escriben | Leen / sufren | Casos |
|---|---|---|---|
| `Time.timeScale`, `Time.maximumDeltaTime` | el tiempo de juego rápido de las pruebas (`IFastGameTime`, interfaz en `Assets/Scripts/TestResults.cs`; lo aplica `TestRunner`, `Assets/Scripts/TestRunner.cs`); `GymUsabilityTest` (maxDelta 0,04); las pruebas que ponen su `timeScale` (`CicloVidaTest`, `ConversationTest`, `FruitOrchardTest`…); la velocidad del jugador (`GameSpeed`, `Assets/Scripts/Time/GameSpeed.cs`); los menús que pausan (`SceneLauncher`, `HologramMenuController`); `TestRunner.Sanear` (método, no clase: restaura y avisa «FUGA») | `NavMeshAgent`, andadores cinemáticos, puertas y disparadores, la jornada | CV-01, CV-02 |
| Reloj del día (`GameTick`, `Assets/Scripts/World/GameTick.cs`) | `WorkdayDirector` (`Assets/Scripts/Progression/WorkdayDirector.cs`), los saltos de día de las pruebas | jornada, horario libre (desde las 16), cena de las 19 (`MealRitual`, `Assets/Scripts/Activities/MealRitual.cs`), crías, celo | CV-07, CV-09 |
| **Cuerpo y `NavMeshAgent` de un animal** (un solo dueño, `d44f7e8`) | el deseo (`Volition`, `Assets/Animals/Volition.cs`, con `DesireCatalog` y `ActionPrep`); el hábito (`Habit`); el ciclo de vida; los viajes; `Forager`; la reacción a una amenaza (`ThreatResponder`, `Assets/Animals/ThreatResponder.cs`: decide `Reaction.Fight`, `Flee`…; las corrutinas `Flee`/`Fight` están en `Animal`; no hay clase `Fight`); `Animal.CorrectMedium` (método de `Assets/Animals/Animal.cs`); los `Warp` (en `DesireCatalog`, `Animal`, `StaleNavCarver`, `NavAgentSafe`…); `NavAgentSafe` (`Assets/Scripts/World/NavAgentSafe.cs`) | `NavTrace` (`Assets/Animals/NavTrace.cs`), el detector de atascos de g1 (`FaunaChecks`) y g50 (`JornadaTest`) | CV-03, CV-04, CV-05, CV-14 |
| Tallado del NavMesh | el árbol que gira (`TreeRotator`, `Assets/Scripts/World/TreeRotator.cs`: obstáculo de la raíz, `2847127`, `473aaec`); `StaleNavCarver` (`Assets/Scripts/World/StaleNavCarver.cs`: paredes de la raíz); los enlaces para trepar | fauna; crías del recinto (`PenWalker`, `Assets/Animals/PenWalker.cs`, `76db8a8`); personas | CV-06 |
| Agenda de una persona (`WorkRoster`, `Assets/Scripts/Progression/WorkRoster.cs`, y las franjas de `WorkdayDirector`) | el reparto por santuarios; las opcionales; los clones de la Magnate (dentro de `WorkdayDirector`, no es una clase); las iniciaciones (`NewcomerInitiator`, `AreaInitiator`); la ronda de frutales (`FruitGardener`); la clase de yoga; las comidas (`MealRitual`); el horario libre (`LeisureDirector`); `RemoteStaff` (`Assets/Scripts/Progression/RemoteStaff.cs`); `TestDuties` (`Assets/Scripts/Tests/TestDuties.cs`) | la jornada, el detector de «dar vueltas», las pruebas de yoga | CV-07, CV-08, CV-09, CV-10, CV-11, CV-15 |
| Topes de `PopulationPlan` (`Assets/Scripts/World/PopulationPlan.cs`: fauna = K × humanos, sin insectos en el santuario 1) | el plan, las crías, los icebergs, la cría en cautividad | `[ECO]`, g113 (`RepartoSantuariosTest`), g111 (`FaunaFriaTest`) | CV-12 |
| Materiales | `CheapMaterialsPass` (`Assets/Editor/CheapMaterialsPass.cs`, al final del Blockout); `CheapMaterialsSweeper` (`Assets/Scripts/World/CheapMaterialsSweeper.cs`, en juego); `CheapMaterials` (`Assets/Scripts/World/CheapMaterials.cs`); los constructores de microcosmos; la emisión (medusas) | g117 (`PeticionesSondaTest`), las fotos de brillos | CV-13 |
| Lotes estáticos | `LoteEstatico` (`Assets/Scripts/World/LoteEstatico.cs`) frente a las clases que mueven piezas hijas (W12) | el dibujado en Play | caso CV por escribir |
| `TestRunner.Running` y Play | las pruebas (`TestRunner`), el jugador (W4), la puerta de la enfermería, la jornada | todo | caso CV por escribir |

## Los casos CV

Están en [casos.md §CV](casos.md) con su unidad y su grupo, y en [registro.md](registro.md) como `Sin probar`: a cada uno le falta su
`TestProbe.Check` con `[CV-nn]` al principio del texto. Lo añade quien toque esa prueba.

| ID | Procesos | Qué tiene que pasar | De dónde sale |
|---|---|---|---|
| CV-01 | Tiempo de juego rápido (`IFastGameTime`, maxDelta 1) × puertas, disparadores y agentes | Ningún paso grande hace que un agente se salte una puerta o un disparador; el grupo que lo usa y los siguientes, igual de verdes | `5347100`, g50 y g40 |
| CV-02 | maxDelta 0,04 del gimnasio × el grupo siguiente | `Sanear` lo restaura; 0 «FUGA» de tiempo | `13a213a`, g49 |
| CV-03 | Warp de la fauna × viaje en marcha | Quitar o cambiar el Warp no sube «atascado > 20 s» respecto a la media de las 3 rondas anteriores | `52d2949`; r76–r77 dieron 27 y 34 ([contadores.md](contadores.md)) |
| CV-04 | Pausa de otra conducta × velocidad de un viaje | Un viaje en marcha nunca queda con velocidad «pedida 0,0» | `52d2949` |
| CV-05 | `Habit` × deseo (`Volition`) × ciclo de vida × viajes | El cuerpo tiene un solo dueño (el deseo que gana); el hábito no cuenta como atasco | `d44f7e8`, `5d89067` |
| CV-06 | `TreeRotator` × NavMesh × crías del recinto (`PenWalker`) | La raíz vuelve a tallar el NavMesh tras cada paso; las crías del recinto andan sin NavMesh y no se quedan quietas | `2847127`, `76db8a8`, `4990e1c` |
| CV-07 | Jornada × ronda de frutales × detector de «dar vueltas» | La ronda de frutales no es «dar vueltas» | `f64fad3` |
| CV-08 | Jornada × clase de yoga | Las conductas que dejó la jornada se pausan en la clase | `e2c0c7c` |
| CV-09 | Jornada × horario libre (16 h) × cena (19 h) | La barra se vuelve a llenar a las 19 h; nadie se queda sin franja | `2d9a0aa`, `31df64e` |
| CV-10 | Reparto × opcionales × clones × iniciaciones | Primero las obligadas; si nadie está libre, las opcionales no se hacen ni piden clon; antes una iniciación que un clon | `16cb73e`, `6a8b400`, `19740d1` |
| CV-11 | Clon de la Magnate × misión con herramienta (alimentador) | El clon con el alimentador da de comer | `0cd78a8`, `4299722` |
| CV-12 | `PopulationPlan` × crías, icebergs y otras especies | Vivos ≤ K × humanos en el total que cuenta; el `[ECO]` lo dice | PB-151, g113 |
| CV-13 | `CheapMaterialsPass` × `CheapMaterialsSweeper` × constructores × emisión | Mates también en juego y tras reconstruir un microcosmos | `46f92ea`, `42b0def`, PB-152 |
| CV-14 | `NavAgentSafe` × alfombra, Línea, muelle aéreo, en brazos | El agente vuelve a encenderse sobre el NavMesh | `63de4fd` |
| CV-15 | `RemoteStaff` (santuario 2) × `TestRunner` × jornada | Con las pruebas en marcha no hay nadie apagado; sin pruebas, la jornada manda | g50, Lizmibet y Linduarda |
