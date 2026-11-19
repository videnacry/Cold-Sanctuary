# Conocimiento: lo que volverá a morder

Lecciones de Unity, del editor, del PC y de las ramas que ya nos costaron una ronda o más. Una entrada por lección, con cuatro campos
fijos. Cuando un tema crezca, pasa a su propio fichero (`unity-navmesh.md`, `unity-tiempo.md`, `editor-y-pc.md`, `ramas.md`) y aquí
queda el enlace. Lo que aún no se entiende va primero a [hallazgos.md](../qa/hallazgos.md).

**Formato**: Síntoma (lo que se ve) · Medido (el dato y dónde) · Cómo se evita (la regla o el código) · Dónde (fichero, caso, regla).

| Tema | Síntoma | Medido | Cómo se evita | Dónde |
|---|---|---|---|---|
| NavMesh | Un animal se queda quieto o da vueltas con un camino válido | Varias conductas movían el mismo `NavMeshAgent`; atascado > 20 s llegó a 34 (r77) | **Un solo dueño del cuerpo**: el deseo que gana; el hábito, el ciclo de vida y los viajes viejos ceden (`d44f7e8`). Quien añade una conducta que mueve el agente, lo apunta en el entrelazamiento | `Assets/Animals/Volition.cs`; CV-05; [entrelazamiento.md](../qa/entrelazamiento.md) |
| NavMesh | Un viaje en marcha se para con velocidad «pedida 0,0» | La pausa de otra conducta le quitaba la velocidad (ronda 77) | La pausa no toca la velocidad de un viaje en marcha (`52d2949`) | CV-04 |
| NavMesh | Un agente apagado (alfombra, Línea, muelle, en brazos) no vuelve a andar | «Failed to create agent»: 64 y luego 35 (r76) | Encenderlo siempre con `NavAgentSafe` sobre el NavMesh (`63de4fd`) | `Assets/Scripts/World/NavAgentSafe.cs`; CV-14 |
| NavMesh | La fauna atraviesa la raíz del árbol que gira | Animales dentro de `Pared_raiz` (r72) | La raíz vuelve a tallar el NavMesh tras cada paso (`2847127`); las crías del recinto andan sin NavMesh (`PenWalker`, `76db8a8`) | `TreeRotator`, `StaleNavCarver`, `PenWalker`; CV-06 |
| Tiempo | Un grupo de pruebas deja el tiempo cambiado y el siguiente falla | `TestRunner.Sanear` avisa «FUGA del grupo N: maximumDeltaTime …» | Toda prueba que cambia `timeScale` o `maximumDeltaTime` lo restaura; `Sanear` lo deja como estaba entre grupos | `Assets/Scripts/TestRunner.cs`; CV-01, CV-02 |
| Editor y PC | La cadena se cuelga al sincronizar | TIMEOUT de 600 s esperando el «Domain Reload» (r13, r67, r68) | Toda espera con tope y «PARO: motivo» (OP-03) | [operacion.md](../qa/operacion.md) |
| Editor y PC | El PC se reinicia en un Blockout entero | 16 GB llenos | Reconstruir por partes y por debajo de ~11 GB (OP-06) | [operacion.md](../qa/operacion.md) |
| Ramas | No compila tras cambiar de rama | 99 .cs sobraban en Unity (r81); un .cs de una clase retirada rompe la compilación | Sincronizar con `--prune` y listar los sobrantes (OP-05) | H3, B-03 |
| Ramas | Un doc del repo aparece vacío | `82878fc`: checklist y auditoría en 0 bytes | Un script que escribe un doc comprueba su entrada y que no encoge (`marcar_seguro.sh`, OP-04); QaCheck §6.9 | H2, B-02 |
