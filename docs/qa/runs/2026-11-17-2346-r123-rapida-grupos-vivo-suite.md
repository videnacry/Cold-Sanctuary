# r123 · 1 50 53 113 28 · 2026-11-17 23:46 · rápida + grupos + vivo + suite
Commit: 56a862e · Rama: claude/edificios · Fusionadas: peticiones-sonda@10fd0de · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 53 113 28 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 129 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1074/0 · 415/1 · suite 4766/2 · FaunaChecks (escena real) = 46 PASS / 0 FAIL → OK ✓ · Vida del santuario (elevado, musgo, dorm = 167 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 48 PASS / 1 FAIL → FALLOS ✗ · Árbol de los Campos v2: tronco cuadrado, = 46 PASS / 0 FAIL → OK ✓ · Reparto por santuarios: casa por estadís = 108 PASS / 0 FAIL → OK ✓ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s 1 · da vueltas 3 ·
  volcado — · sin camino 62 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterTierra.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: ARB-01, ARB-02, ARB-04, ARB-10, ARB-13, ARB-15, ARB-17, ARB-18, ARB-21, ARB-22, ARB-23, ARB-24, ARB-25, ARB-26, ARB-27, ARB-33, ARB-34, ARB-36, ARB-39, ARB-40, ARB-44, ARB-45, ARB-46, ARB-47, ARB-48, ARB-49, ARB-50, ARB-51, ARB-52, ARB-53, ARB-54, ARB-55, ARB-56, ARB-57, ARB-58, ARB-59, ARB-60, ARB-61, ARB-62, ARB-63, ARB-64, ARB-65, ARB-66, ARB-67, ARB-68, ARB-69, ARB-70, ARB-71, ARB-72, ARB-73, ARB-74, ARB-75, ARB-76, ARB-77, ARB-78, ARB-79, ARB-80, ARB-81, ARB-82, ARB-83 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g112 LoteEstaticoTest · una máscara cogida sale del lote — devueltas 0
    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — atascado > 20 s: 1 (Orca «Orca(Clone)» #95)
    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — atascado > 20 s: 1 (Wolf «Wolf(Clone)» #56); da vueltas en círculo: 3 (Wolf «Wolf(Clone)» #37, Bear «Bear(Clone)» #8, EmperorPenguin «EmperorPenguin(Clone)» #4)

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=1003 NavReach.BadPaths=28 HomeLegs=538 (rectos 35) EdgeNudges=35 FrozenRestarts=94 FrozenTrips=71 FrozenWarps=36 ComidasRepetidas=0 BadPaths(viajes)=2 NoPathTrips=62 IdleRefused=37 Vigilados=32602 Rescates=136 (repetidos 41) CaminosMantenidos=18890 AlMasCercano=2 (sin sitio 0) Montículos=2 Lomas=8 ParedesCueva=15 MarinosBajoHielo=1289 MarinosVarados=0 BocasMovidas=1 PenWalker=2594/2591/0 (empezados/llegados/sin sitio)

Salida del probador: `r123.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
