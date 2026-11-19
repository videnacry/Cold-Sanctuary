# r122 · 1 50 53 · 2026-11-16 18:48 · rápida + grupos + vivo + suite
Commit: 403473b · Rama: claude/edificios · Fusionadas: peticiones-sonda@10fd0de · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 53 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 133 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1074/0 · 414/2 · suite 4766/1 · FaunaChecks (escena real) = 46 PASS / 0 FAIL → OK ✓ · Vida del santuario (elevado, musgo, dorm = 167 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 47 PASS / 2 FAIL → FALLOS ✗ · Árbol de los Campos v2: tronco cuadrado, = 46 PASS / 0 FAIL → OK ✓ · Reparto por santuarios: casa por estadís = 108 PASS / 0 FAIL → OK ✓ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s 1 · da vueltas 2 ·
  volcado 2 · sin camino 64 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRotulos.cs, Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: ARB-01, ARB-02, ARB-04, ARB-10, ARB-13, ARB-15, ARB-17, ARB-18, ARB-21, ARB-22, ARB-23, ARB-24, ARB-25, ARB-26, ARB-27, ARB-33, ARB-34, ARB-36, ARB-39, ARB-40, ARB-44, ARB-45, ARB-46, ARB-47, ARB-48, ARB-49, ARB-50, ARB-51, ARB-52, ARB-53, ARB-54, ARB-55, ARB-56, ARB-57, ARB-58, ARB-59, ARB-60, ARB-61, ARB-62, ARB-63, ARB-64, ARB-65, ARB-66, ARB-67, ARB-68, ARB-69, ARB-70, ARB-71, ARB-72, ARB-73, ARB-74, ARB-75, ARB-76, ARB-77, ARB-78, ARB-79, ARB-80, ARB-81, ARB-82, ARB-83 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — atascado > 20 s: 1 (Orca «Orca(Clone)» #98)
    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 2 (Wolf «Wolf(Clone)» #18, Wolf «Wolf(Clone)» #7)
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — Wolf «Wolf(Clone)» #2: dentro de un objeto («Tierra» en fuera de las áreas (-229,0,-147) (quieto (explore)) [cuerpo: centro y 0,41, pies 0,10, raíz y 0,10, nadando False · agente: en NavMesh True, camino True, pendiente False, estado PathComplete, velocidad 0,00 (pedida 3,0
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — Wolf «Wolf(Clone)» #2: dentro de un objeto («Tierra» en fuera de las áreas (-229,0,-147) (quieto (explore)) [cuerpo: centro y 0,41, pies 0,10, raíz y 0,10, nadando False · agente: en NavMesh True, camino True, pendiente False, estado PathComplete, velocidad 0,00 (pedida 3,0), energía —, interno a 0,00 m del cuerpo, evitación HighQualityObstacleAvoidance (prioridad 50), en enlace False, updatePosition True, agente más cercano a 42,2 m, borde a 5,59 m, destino a 12,7 m, parado False · deseada 0,00, rumbo a 12,65 m, esquinas 2, caducado False, aceleración 8,0, frenada 0,0 m (autoBraking True), queda 12,7 m, altura interna 0,00 m, cuerpo cinemático · último cambio: «deseo: SetPath (PathComplete, 2 esquinas, 12,7 m)» hace 0,0 s hacia (-230,5,0,1,-134,4)])

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=919 NavReach.BadPaths=21 HomeLegs=512 (rectos 44) EdgeNudges=42 FrozenRestarts=102 FrozenTrips=81 FrozenWarps=39 ComidasRepetidas=0 BadPaths(viajes)=6 NoPathTrips=64 IdleRefused=71 Vigilados=34497 Rescates=209 (repetidos 131) CaminosMantenidos=18807 AlMasCercano=0 (sin sitio 0) Montículos=2 Lomas=8 ParedesCueva=15 MarinosBajoHielo=1221 MarinosVarados=0 BocasMovidas=1 PenWalker=2672/2669/9 (empezados/llegados/sin sitio)

Salida del probador: `r122.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
