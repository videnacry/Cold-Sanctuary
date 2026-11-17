# r124 · 1 50 53 113 28 · 2026-11-17 13:51 · rápida + grupos + vivo + suite
Commit: f4c3f4e · Rama: claude/edificios · Fusionadas: peticiones-sonda@d19e178 · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 53 113 28 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 133 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1072/2 · 415/1 · suite 4763/4 · FaunaChecks (escena real) = 46 PASS / 0 FAIL → OK ✓ · Vida del santuario (elevado, musgo, dorm = 167 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 48 PASS / 1 FAIL → FALLOS ✗ · Árbol de los Campos v2: tronco cuadrado, = 46 PASS / 0 FAIL → OK ✓ · Reparto por santuarios: casa por estadís = 108 PASS / 0 FAIL → OK ✓ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s 1 · da vueltas 1 ·
  volcado 1 · sin camino 69 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterTierra.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: ARB-01, ARB-02, ARB-04, ARB-10, ARB-13, ARB-15, ARB-17, ARB-18, ARB-21, ARB-22, ARB-23, ARB-24, ARB-25, ARB-26, ARB-27, ARB-33, ARB-34, ARB-36, ARB-39, ARB-40, ARB-44, ARB-45, ARB-46, ARB-47, ARB-48, ARB-49, ARB-50, ARB-51, ARB-52, ARB-53, ARB-54, ARB-55, ARB-56, ARB-57, ARB-58, ARB-59, ARB-60, ARB-61, ARB-62, ARB-63, ARB-64, ARB-65, ARB-66, ARB-67, ARB-68, ARB-69, ARB-70, ARB-71, ARB-72, ARB-73, ARB-74, ARB-75, ARB-76, ARB-77, ARB-78, ARB-79, ARB-80, ARB-81, ARB-82, ARB-83 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g112 LoteEstaticoTest · devuelta al perchero, se dibuja por sí misma
    [TEST] FAIL · g112 LoteEstaticoTest · una máscara cogida sale del lote — devueltas 1 en 0,6 s
    [TEST] FAIL · g112 LoteEstaticoTest · una máscara cogida sale del lote — devueltas 1 en 0,8 s
    [TEST] FAIL · g41 KitchenInitiationTest · el stock sube con TODAS las raciones de la tanda por mayor — 0 → 29 (×30)
    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — atascado > 20 s: 1 (Orca «Orca(Clone)» #43); da vueltas en círculo: 1 (Reindeer «Reindeer(Clone)» #81)
    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 1 (Penguin «Penguin(Clone)» #91)

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=1354 NavReach.BadPaths=89 HomeLegs=545 (rectos 36) EdgeNudges=44 FrozenRestarts=92 FrozenTrips=70 FrozenWarps=26 ComidasRepetidas=0 BadPaths(viajes)=5 NoPathTrips=69 IdleRefused=39 Vigilados=32981 Rescates=98 (repetidos 20) CaminosMantenidos=20088 AlMasCercano=0 (sin sitio 0) Montículos=2 Lomas=8 ParedesCueva=15 MarinosBajoHielo=748 MarinosVarados=0 BocasMovidas=1 PenWalker=2600/2595/4 (empezados/llegados/sin sitio)

Salida del probador: `r124.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
