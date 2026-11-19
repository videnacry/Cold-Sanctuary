# r116 · 1 50 53 · 2026-11-15 23:37 · rápida + grupos + vivo + suite
Commit: 05cc591 · Rama: claude/edificios · Fusionadas: peticiones-sonda@94f2431 · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 53 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 123 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1071/0 · 136/2 · suite 4756/2 · FaunaChecks (escena real) = 43 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 47 PASS / 2 FAIL → FALLOS ✗ · Árbol de los Campos v2: tronco cuadrado, = 46 PASS / 0 FAIL → OK ✓ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s 1 · da vueltas 12 ·
  volcado 12 · sin camino 232 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRotulos.cs, Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: ARB-01, ARB-02, ARB-04, ARB-10, ARB-13, ARB-15, ARB-17, ARB-18, ARB-21, ARB-22, ARB-23, ARB-24, ARB-25, ARB-26, ARB-27, ARB-33, ARB-34, ARB-36, ARB-39, ARB-40, ARB-44, ARB-45, ARB-46, ARB-47, ARB-48, ARB-49, ARB-50, ARB-51, ARB-52, ARB-53, ARB-54, ARB-55, ARB-56, ARB-57, ARB-58, ARB-59, ARB-60, ARB-61, ARB-62, ARB-63, ARB-64, ARB-65, ARB-66, ARB-67, ARB-68, ARB-69, ARB-70, ARB-71, ARB-72, ARB-73, ARB-74, ARB-75, ARB-76, ARB-77, ARB-78, ARB-79, ARB-80, ARB-81, ARB-82, ARB-83 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 12 (Penguin «Penguin(Clone)» #61, EmperorPenguin «EmperorPenguin(Clone)» #59, Wolf «Wolf(Clone)» #37, Reindeer «Reindeer(Clone)» #48, ArcticFox «ArcticFox(Clone)» #92, ArcticFox «ArcticFox(Clone)» #29); atascado > 20 s: 1 (Wolf «Wolf(Clone)» #37)
    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 15 (Bear «Bear(Clone)» #67, Penguin «Penguin(Clone)» #2, Wolf «Wolf(Clone)» #94, MuskOx «MuskOx(Clone)» #71, Bear «Bear(Clone)» #12, ArcticFox «ArcticFox(Clone)» #25)
    [TEST] FAIL · g50 JornadaTest · a las 16:00 es libre: nada lo mueve (ni la jornada ni la Magnate) — se movió 1,2 m solo
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — Bear «Bear(Clone)» #83: dentro de un objeto («Tierra» en fuera de las áreas (-116,0,-93) (hábito: come bayas de camarina) [cuerpo: centro y 0,59, pies 0,13, raíz y 0,14 · agente: en NavMesh True, camino False, pendiente False, estado PathComplete, velocidad 0,00 (pedida 0,0), energía —, interno a 0,00 m del cuerpo, evitación HighQualityObstacleAvoidance (prioridad 50), en enlace False, updatePosition True, agente más cercano a 32,1 m, borde a 10,87 m, destino a 0,0 m, parado False · deseada 0,00, rumbo a 0,00 m, esquinas 0, caducado False, aceleración 8,0, frenada 0,0 m (autoBraking True), queda 0,0 m, altura interna 0,00 m, cuerpo cinemático · último cambio: «Habit.Halt» hace 0,0 s hacia (-116,4,0,1,-93,2)]); Seal «Seal(Clone)» #91: dentro de un ob

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=74 NavReach.BadPaths=13 HomeLegs=1039 (rectos 52) EdgeNudges=81 FrozenRestarts=282 FrozenTrips=147 FrozenWarps=66 ComidasRepetidas=0 BadPaths(viajes)=3 NoPathTrips=232 IdleRefused=0 Vigilados=15211 Rescates=526 (repetidos 261) CaminosMantenidos=17099 AlMasCercano=0 (sin sitio 0) Montículos=2 BocasMovidas=1 PenWalker=2614/2611/0 (empezados/llegados/sin sitio)

Salida del probador: `r116.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
