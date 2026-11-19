# r118 · 1 50 53 · 2026-11-15 04:50 · rápida + grupos + vivo + suite
Commit: aaf1d5f · Rama: claude/edificios · Fusionadas: peticiones-sonda@aab2024 · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 53 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 127 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1071/0 · 137/1 · suite 4756/2 · FaunaChecks (escena real) = 43 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 48 PASS / 1 FAIL → FALLOS ✗ · Árbol de los Campos v2: tronco cuadrado, = 46 PASS / 0 FAIL → OK ✓ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s — · da vueltas 2 ·
  volcado 2 · sin camino 58 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRotulos.cs, Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: ARB-01, ARB-02, ARB-04, ARB-10, ARB-13, ARB-15, ARB-17, ARB-18, ARB-21, ARB-22, ARB-23, ARB-24, ARB-25, ARB-26, ARB-27, ARB-33, ARB-34, ARB-36, ARB-39, ARB-40, ARB-44, ARB-45, ARB-46, ARB-47, ARB-48, ARB-49, ARB-50, ARB-51, ARB-52, ARB-53, ARB-54, ARB-55, ARB-56, ARB-57, ARB-58, ARB-59, ARB-60, ARB-61, ARB-62, ARB-63, ARB-64, ARB-65, ARB-66, ARB-67, ARB-68, ARB-69, ARB-70, ARB-71, ARB-72, ARB-73, ARB-74, ARB-75, ARB-76, ARB-77, ARB-78, ARB-79, ARB-80, ARB-81, ARB-82, ARB-83 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 2 (Reindeer «Reindeer(Clone)» #97, Reindeer «Reindeer(Clone)» #81)
    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 5 (ArcticFox «ArcticFox(Clone)» #17, Wolf «Wolf(Clone)» #3, Reindeer «Reindeer(Clone)» #92, Wolf «Wolf(Clone)» #57, Bear «Bear(Clone)» #42)
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — Wolf «Wolf(Clone)» #57: dentro de un objeto («Tierra» en fuera de las áreas (-202,0,-154) (moviéndose (follow)) [cuerpo: centro y 0,53, pies 0,09, raíz y 0,09, nadando False · agente: en NavMesh True, camino True, pendiente False, estado PathComplete, velocidad 3,00 (pedida 3,0), energía —, interno a 0,00 m del cuerpo, evitación HighQualityObstacleAvoidance (prioridad 50), en enlace False, updatePosition True, agente más cercano a 13,8 m, borde a 9,59 m, destino a 36,9 m, parado False · deseada 3,00, rumbo a 36,94 m, esquinas 2, caducado False, aceleración 8,0, frenada 0,0 m (autoBraking True), queda 36,9 m, altura interna 0,00 m, cuerpo cinemático · último cambio: «Locomotion.GoTo ← <Fight>d__182.MoveNext» hace 23,4 s hacia (-208,6,0,1,-190,7)])

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=1587 NavReach.BadPaths=6 HomeLegs=492 (rectos 7) EdgeNudges=25 FrozenRestarts=69 FrozenTrips=47 FrozenWarps=22 ComidasRepetidas=0 BadPaths(viajes)=6 NoPathTrips=58 IdleRefused=33 Vigilados=34330 Rescates=86 (repetidos 12) CaminosMantenidos=19860 AlMasCercano=0 (sin sitio 0) Montículos=2 BocasMovidas=1 PenWalker=2634/2633/0 (empezados/llegados/sin sitio)

Salida del probador: `r118.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
