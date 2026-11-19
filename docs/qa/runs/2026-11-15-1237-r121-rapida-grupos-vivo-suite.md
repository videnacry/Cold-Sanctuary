# r121 · 1 50 53 · 2026-11-15 12:37 · rápida + grupos + vivo + suite
Commit: 1b4f1c1 · Rama: claude/edificios · Fusionadas: peticiones-sonda@8b0c61d · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 53 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 127 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1073/0 · 139/1 · suite 4761/2 · FaunaChecks (escena real) = 45 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 48 PASS / 1 FAIL → FALLOS ✗ · Árbol de los Campos v2: tronco cuadrado, = 46 PASS / 0 FAIL → OK ✓ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s — · da vueltas 1 ·
  volcado 1 · sin camino 67 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRotulos.cs, Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: ARB-01, ARB-02, ARB-04, ARB-10, ARB-13, ARB-15, ARB-17, ARB-18, ARB-21, ARB-22, ARB-23, ARB-24, ARB-25, ARB-26, ARB-27, ARB-33, ARB-34, ARB-36, ARB-39, ARB-40, ARB-44, ARB-45, ARB-46, ARB-47, ARB-48, ARB-49, ARB-50, ARB-51, ARB-52, ARB-53, ARB-54, ARB-55, ARB-56, ARB-57, ARB-58, ARB-59, ARB-60, ARB-61, ARB-62, ARB-63, ARB-64, ARB-65, ARB-66, ARB-67, ARB-68, ARB-69, ARB-70, ARB-71, ARB-72, ARB-73, ARB-74, ARB-75, ARB-76, ARB-77, ARB-78, ARB-79, ARB-80, ARB-81, ARB-82, ARB-83 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 1 (EmperorPenguin «EmperorPenguin(Clone)» #94)
    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 2 (Penguin «Penguin(Clone)» #70, ArcticFox «ArcticFox(Clone)» #18)
    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo — Wolf «Wolf(Clone)» #2: dentro de un objeto («Tierra» en fuera de las áreas (-123,0,-178) (moviéndose (explore)) [cuerpo: centro y 0,53, pies 0,10, raíz y 0,10, nadando False · agente: en NavMesh True, camino True, pendiente False, estado PathComplete, velocidad 0,00 (pedida 22,0), energía —, interno a 0,00 m del cuerpo, evitación HighQualityObstacleAvoidance (prioridad 50), en enlace False, updatePosition True, agente más cercano a 13,5 m, borde a 1,77 m, destino a 8,0 m, parado False · deseada 0,00, rumbo a 8,02 m, esquinas 2, caducado False, aceleración 8,0, frenada 0,0 m (autoBraking True), queda 8,0 m, altura interna 0,00 m, cuerpo cinemático · último cambio: «deseo: SetPath (PathComplete, 2 esquinas, 8,0 m)» hace 0,0 s hacia (-115,8,0,1,-181,3)])

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=1321 NavReach.BadPaths=16 HomeLegs=610 (rectos 48) EdgeNudges=36 FrozenRestarts=74 FrozenTrips=52 FrozenWarps=16 ComidasRepetidas=0 BadPaths(viajes)=3 NoPathTrips=67 IdleRefused=32 Vigilados=36115 Rescates=261 (repetidos 167) CaminosMantenidos=18866 AlMasCercano=0 (sin sitio 0) Montículos=2 Lomas=7 ParedesCueva=15 MarinosBajoHielo=1392 MarinosVarados=0 BocasMovidas=1 PenWalker=2669/2669/5 (empezados/llegados/sin sitio)

Salida del probador: `r121.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
