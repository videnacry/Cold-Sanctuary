# r125 · 1 50 53 113 28 · 2026-11-17 19:00 · rápida + grupos + vivo + suite
Commit: 4127332 · Rama: claude/edificios · Fusionadas: peticiones-sonda@14d2026 · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 53 113 28 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 144 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1075/0 · 415/2 · suite 4766/2 · FaunaChecks (escena real) = 47 PASS / 0 FAIL → OK ✓ · Vida del santuario (elevado, musgo, dorm = 167 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 47 PASS / 2 FAIL → FALLOS ✗ · Árbol de los Campos v2: tronco cuadrado, = 46 PASS / 0 FAIL → OK ✓ · Reparto por santuarios: casa por estadís = 108 PASS / 0 FAIL → OK ✓ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s — · da vueltas 1 ·
  volcado 1 · sin camino 46 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterTierra.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: ARB-01, ARB-02, ARB-04, ARB-10, ARB-13, ARB-15, ARB-17, ARB-18, ARB-21, ARB-22, ARB-23, ARB-24, ARB-25, ARB-26, ARB-27, ARB-33, ARB-34, ARB-36, ARB-39, ARB-40, ARB-44, ARB-45, ARB-46, ARB-47, ARB-48, ARB-49, ARB-50, ARB-51, ARB-52, ARB-53, ARB-54, ARB-55, ARB-56, ARB-57, ARB-58, ARB-59, ARB-60, ARB-61, ARB-62, ARB-63, ARB-64, ARB-65, ARB-66, ARB-67, ARB-68, ARB-69, ARB-70, ARB-72, ARB-73, ARB-74, ARB-75, ARB-76, ARB-77, ARB-78, ARB-79, ARB-80, ARB-81, ARB-82, ARB-83, ARB-84 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 1 (EmperorPenguin «EmperorPenguin(Clone)» #41)
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — ArcticFox «ArcticFox(Clone)» #83: dentro de un objeto («Estructuras_Barrera_hielo_MAT» en fuera de las áreas (-142,1,5) (moviéndose (explore)) [cuerpo: centro y 0,87, pies 0,67, raíz y 0,67, nadando False, pieza: MeshCollider centro (-145,5,0,80,9,1) tamaño (32,2×3,13×66,0) «Casquete_silvestre_BosqueOeste» · agente: en NavMesh True, camino True, pendiente False, estado PathComplete, velocidad 0,00 (pedida 3,0), energía —, interno a 0,00 m del cuerpo, evitación HighQualityObstacleAvoidance (prioridad 50), en enlace False, updatePosition True, agente más cercano a 119,6 m, borde a 0,00 m, destino a 5,8 m, parado False · deseada 3,00, rumbo a 5,77 m, esquinas 3, caducado False, aceleración 8,0, frenada 0,0 m (autoBraking True), queda 6,0 m, altura intern
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — Wolf «Wolf(Clone)» #69: dentro de un objeto («Tierra» en fuera de las áreas (-162,0,-146) (moviéndose (mate)) [cuerpo: centro y 0,54, pies 0,10, raíz y 0,10, nadando False, pieza: BoxCollider centro (-138,5,0,38,-104,0) tamaño (21,0×0,25×16,8) «Territorio_AUTO» · agen
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — Wolf «Wolf(Clone)» #69: dentro de un objeto («Tierra» en fuera de las áreas (-162,0,-146) (moviéndose (mate)) [cuerpo: centro y 0,54, pies 0,10, raíz y 0,10, nadando False, pieza: BoxCollider centro (-138,5,0,38,-104,0) tamaño (21,0×0,25×16,8) «Territorio_AUTO» · agente: en NavMesh True, camino True, pendiente False, estado PathComplete, velocidad 3,00 (pedida 3,0), energía —, interno a 0,00 m del cuerpo, evitación HighQualityObstacleAvoidance (prioridad 50), en enlace False, updatePosition True, agente más cercano a 22,3 m, borde a 5,87 m, destino a 11,5 m, parado False · deseada 3,00, rumbo a 11,55 m, esquinas 2, caducado False, aceleración 8,0, frenada 0,0 m (autoBraking True), queda 11,5 m, altura interna 0,00 m, cuerpo cinemático · último cambio: «deseo: SetPath (PathComplete, 2 esquinas, 11,5 m)» hace 0,0 s hacia (-159
    [TEST] FAIL · g94 NuclearSiteTest · hay alguien del personal trabajando abajo, en la Central (en la estación, bajado por la Línea) — «Linduarda»: abajo False

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=1138 NavReach.BadPaths=24 HomeLegs=546 (rectos 47) EdgeNudges=32 FrozenRestarts=86 FrozenTrips=57 FrozenWarps=25 CaminosQueCrecen=85 ComidasRepetidas=0 BadPaths(viajes)=30 NoPathTrips=46 IdleRefused=45 Vigilados=32745 Rescates=91 (repetidos 16) CaminosMantenidos=18732 AlMasCercano=0 (sin sitio 0) Montículos=2 Lomas=8 ParedesCueva=15 MarinosBajoHielo=1211 MarinosVarados=0 MarinosSinAvance=0 BocasMovidas=1 PenWalker=2542/2541/0 (empezados/llegados/sin sitio)

Salida del probador: `r125.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
