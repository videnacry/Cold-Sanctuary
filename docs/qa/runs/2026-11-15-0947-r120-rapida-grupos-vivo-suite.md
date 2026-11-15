# r120 · 1 50 53 · 2026-11-15 09:47 · rápida + grupos + vivo + suite
Commit: a813bb6 · Rama: claude/edificios · Fusionadas: peticiones-sonda@bf9ca08 · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 53 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 127 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1072/0 · 138/1 · suite 4756/2 · FaunaChecks (escena real) = 44 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 48 PASS / 1 FAIL → FALLOS ✗ · Árbol de los Campos v2: tronco cuadrado, = 46 PASS / 0 FAIL → OK ✓ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s 1 · da vueltas 4 ·
  volcado 1 · sin camino 60 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRotulos.cs, Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: ARB-01, ARB-02, ARB-04, ARB-10, ARB-13, ARB-15, ARB-17, ARB-18, ARB-21, ARB-22, ARB-23, ARB-24, ARB-25, ARB-26, ARB-27, ARB-33, ARB-34, ARB-36, ARB-39, ARB-40, ARB-44, ARB-45, ARB-46, ARB-47, ARB-48, ARB-49, ARB-50, ARB-51, ARB-52, ARB-53, ARB-54, ARB-55, ARB-56, ARB-57, ARB-58, ARB-59, ARB-60, ARB-61, ARB-62, ARB-63, ARB-64, ARB-65, ARB-66, ARB-67, ARB-68, ARB-69, ARB-70, ARB-71, ARB-72, ARB-73, ARB-74, ARB-75, ARB-76, ARB-77, ARB-78, ARB-79, ARB-80, ARB-81, ARB-82, ARB-83 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — atascado > 20 s: 1 (Orca «Orca(Clone)» #58); da vueltas en círculo: 4 (Wolf «Wolf(Clone)» #73, Wolf «Wolf(Clone)» #13, Penguin «Penguin(Clone)» #81, EmperorPenguin «EmperorPenguin(Clone)» #44)
    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 1 (ArcticFox «ArcticFox(Clone)» #99)
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — Orca «Orca(Clone)» #69: dentro de un objeto («Hielo_fijo» en fuera de las áreas (52,-2,5) (moviéndose (explore)) [cuerpo: centro y -1,59, pies -1,90, raíz y -1,90, nadando False · agente apagado · sin cambios registrados])

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=1319 NavReach.BadPaths=4 HomeLegs=451 (rectos 21) EdgeNudges=39 FrozenRestarts=80 FrozenTrips=54 FrozenWarps=15 ComidasRepetidas=0 BadPaths(viajes)=12 NoPathTrips=60 IdleRefused=36 Vigilados=30584 Rescates=218 (repetidos 140) CaminosMantenidos=19952 AlMasCercano=0 (sin sitio 0) Montículos=2 Lomas=7 ParedesCueva=15 BocasMovidas=1 PenWalker=2491/2486/0 (empezados/llegados/sin sitio)

Salida del probador: `r120.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
