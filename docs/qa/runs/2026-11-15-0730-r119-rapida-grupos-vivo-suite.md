# r119 · 1 50 53 · 2026-11-15 07:30 · rápida + grupos + vivo + suite
Commit: 9a7fbe6 · Rama: claude/edificios · Fusionadas: peticiones-sonda@344c457 · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 53 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 128 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1072/0 · 137/2 · suite 4757/2 · FaunaChecks (escena real) = 44 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 47 PASS / 2 FAIL → FALLOS ✗ · Árbol de los Campos v2: tronco cuadrado, = 46 PASS / 0 FAIL → OK ✓ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s 1 · da vueltas 1 ·
  volcado 3 · sin camino 69 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRotulos.cs, Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: ARB-01, ARB-02, ARB-04, ARB-10, ARB-13, ARB-15, ARB-17, ARB-18, ARB-21, ARB-22, ARB-23, ARB-24, ARB-25, ARB-26, ARB-27, ARB-33, ARB-34, ARB-36, ARB-39, ARB-40, ARB-44, ARB-45, ARB-46, ARB-47, ARB-48, ARB-49, ARB-50, ARB-51, ARB-52, ARB-53, ARB-54, ARB-55, ARB-56, ARB-57, ARB-58, ARB-59, ARB-60, ARB-61, ARB-62, ARB-63, ARB-64, ARB-65, ARB-66, ARB-67, ARB-68, ARB-69, ARB-70, ARB-71, ARB-72, ARB-73, ARB-74, ARB-75, ARB-76, ARB-77, ARB-78, ARB-79, ARB-80, ARB-81, ARB-82, ARB-83 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — atascado > 20 s: 1 (ArcticFox «ArcticFox(Clone)» #9); da vueltas en círculo: 1 (Wolf «Wolf(Clone)» #95)
    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 3 (Wolf «Wolf(Clone)» #97, Wolf «Wolf(Clone)» #55, Wolf «Wolf(Clone)» #40)
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — ArcticHare «ArcticHare(Clone)» #74: dentro de un objeto («Tierra» en fuera de las áreas (-43,0,-184) (hábito: ramonea sauce ártico) [cuerpo: centro y 0,19, pies 0,10, raíz y 0,10, nadando False · agente: en NavMesh True, camino True, pendiente False, estado PathComplete, 
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — ArcticHare «ArcticHare(Clone)» #74: dentro de un objeto («Tierra» en fuera de las áreas (-43,0,-184) (hábito: ramonea sauce ártico) [cuerpo: centro y 0,19, pies 0,10, raíz y 0,10, nadando False · agente: en NavMesh True, camino True, pendiente False, estado PathComplete, velocidad 0,00 (pedida 8,0), energía —, interno a 0,00 m del cuerpo, evitación HighQualityObstacleAvoidance (prioridad 50), en enlace False, updatePosition True, agente más cercano a 17,9 m, borde a 1,57 m, destino a 16,1 m, parado False · deseada 8,00, rumbo a 16,09 m, esquinas 2, caducado False, aceleración 8,0, frenada 0,0 m (autoBraking True), queda 16,1 m, altura interna 0,00 m, cuerpo cinemático · último cambio: «Locomotion.GoTo ← SleepCycle.GoingToRest» hace 3,5 s hacia (-28,0,0,0,-178,0)])
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — Wolf «Wolf(Clone)» #40: dentro de un objeto («Pared_cueva» en fuera de las áreas (-202,0,-85) (moviéndose (tend)) [cuerpo: centro y 0,24, pies 0,08, raíz y 0,08, nadando False · agente: en NavMesh True, camino True, pendiente False, estado PathComplete, velocidad 0,00 (pedida 3,0), energía —, interno a 0,00 m del cuerpo, evitación HighQualityObstacleAvoidance (prioridad 50), en enlace False, updatePosition True, agente más cercano a 8,6 m, borde a 0,00 m, destino a 0,7 m, parado False · deseada 0,00, rumbo a 0,67 m, esquinas 2, caducado False, aceleración 8,0, frenada 0,0 m (autoBraking True), queda 0,7 m, altura interna 0,00 m, cuerpo cinemático · último cambio: «deseo: al punto más cercano (camino parcial)» hace 0,0 s hacia (-202,3,0,1,-85,7)])

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=867 NavReach.BadPaths=2 HomeLegs=512 (rectos 36) EdgeNudges=34 FrozenRestarts=76 FrozenTrips=56 FrozenWarps=22 ComidasRepetidas=0 BadPaths(viajes)=4 NoPathTrips=69 IdleRefused=28 Vigilados=34117 Rescates=163 (repetidos 87) CaminosMantenidos=18928 AlMasCercano=1 (sin sitio 0) Montículos=2 Lomas=7 BocasMovidas=1 PenWalker=2636/2631/0 (empezados/llegados/sin sitio)

Salida del probador: `r119.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
