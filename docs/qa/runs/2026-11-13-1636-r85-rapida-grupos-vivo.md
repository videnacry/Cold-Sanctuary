# r85 · 1 50 · 2026-11-13 16:36 · rápida + grupos + vivo
Commit: 41716c9 · Rama: claude/edificios · Fusionadas: peticiones-sonda@d3ca8c7 · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 51 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1018/0 · 82/1 · FaunaChecks (escena real) = 36 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 46 PASS / 1 FAIL → FALLOS ✗ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s 5 · da vueltas 2 ·
  volcado — · sin camino 95 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRotulos.cs, Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: CV-04, CV-05 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g50 JornadaTest · fauna: ningún animal atascado > 20 s, dando vueltas o volcado — atascado > 20 s: 5 (Wolf «Wolf(Clone)» #82, Reindeer «Reindeer(Clone)» #22, Reindeer «Reindeer(Clone)» #54, ArcticFox «ArcticFox(Clone)» #48, MuskOx «MuskOx(Clone)» #66); da vueltas en círculo: 2 (Wolf «Wolf(Clone)» #82, Wolf «Wolf(Clone)» #61)

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=32 NavReach.BadPaths=17 HomeLegs=304 EdgeNudges=0 FrozenRestarts=2 FrozenTrips=0 BadPaths(viajes)=0 NoPathTrips=95 IdleRefused=1857 PenWalker=2582/2578/2 (empezados/llegados/sin sitio)

Salida del probador: `r85.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
