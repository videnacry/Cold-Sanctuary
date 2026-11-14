# r99 · 1 50 53 · 2026-11-14 04:25 · rápida + grupos + vivo
Commit: 7ae301b · Rama: claude/edificios · Fusionadas: peticiones-sonda@8249fec · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 53 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 55 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1070/0 · 134/2 · FaunaChecks (escena real) = 41 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 47 PASS / 2 FAIL → FALLOS ✗ · Árbol de los Campos v2: tronco cuadrado, = 46 PASS / 0 FAIL → OK ✓ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s 2 · da vueltas — ·
  volcado — · sin camino 53 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRotulos.cs, Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: ARB-23, ARB-24, ARB-25, ARB-26, ARB-33, ARB-34, ARB-36, ARB-39, ARB-55, ARB-56, ARB-58, ARB-59, ARB-60, ARB-61, ARB-68, ARB-69, ARB-81, ARB-95, CV-01, CV-04, CV-05, CV-06, CV-07, CV-11, CV-14, CV-15, FAU-01, FAU-05, FAU-06, FAU-08, FAU-09, FAU-11, FAU-12, FAU-19, FAU-20, FAU-21, FAU-22, FAU-23, FAU-24, FAU-26, FAU-28, FAU-29, FAU-31, FAU-32, JOR-33, MIC-15, MIC-26, MIC-39, MIC-40, MIC-52, MIC-55, MIC-57, MIC-61, MIC-64, MIC-68, MIC-69, MIC-73, MIC-74, MIC-75, MIC-77 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — atascado > 20 s: 2 (Wolf «Wolf(Clone)» #75, Bear «Bear(Clone)» #61)
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — ArcticFox «ArcticFox(Clone)» #27: dentro de un objeto («Tierra» en fuera de las áreas (-43,0,-181) (moviéndose (explore)))

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=112 NavReach.BadPaths=2 HomeLegs=324 (rectos 8) EdgeNudges=33 FrozenRestarts=85 FrozenTrips=64 FrozenWarps=31 ComidasRepetidas=0 BadPaths(viajes)=0 NoPathTrips=53 IdleRefused=33 Vigilados=34753 PenWalker=2480/2477/3 (empezados/llegados/sin sitio)

Salida del probador: `r99.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
