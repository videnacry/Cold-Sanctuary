# r117 · 1 50 53 · 2026-11-15 02:13 · rápida + grupos + vivo + suite
Commit: 49274ea · Rama: claude/edificios · Fusionadas: peticiones-sonda@063d93d · Ejecutor: probador (agente)
Escena: SampleScene sin reconstruir; microcosmos rehechos: —
Preparación: preset rápida · Temp/runner-groups.txt = 1 50 53 · 2 días en vivo ×6 · sync con --prune
Tiempo: timeScale — · maxDelta — · FPS media — · Unity — · duración 124 min
Acción a mitad: no reproducido
Resultado: FAIL · rápida 1071/0 · 136/2 · suite 4759/1 · FaunaChecks (escena real) = 43 PASS / 0 FAIL → OK ✓ · Jornada de Kushal: día 1 → tiempo libre = 47 PASS / 2 FAIL → FALLOS ✗ · Árbol de los Campos v2: tronco cuadrado, = 46 PASS / 0 FAIL → OK ✓ · TIMEOUT: 0 · FUGA: 0
Contadores: [ECO] vivos — · [Fauna] atascado>20 s — · da vueltas 12 ·
  volcado 12 · sin camino 198 · NavTrace cambios de camino — · crías de la Cría atascadas 0
Sobrantes: Assets/Editor/TmpCheck/TesterRotulos.cs, Assets/Editor/TmpCheck/TesterRunner.cs, Assets/Editor/TmpCheck/TesterVelocidad.cs, Assets/Editor/TmpCheck/TesterViaje.cs
Interrupciones: —
Casos: ARB-01, ARB-02, ARB-04, ARB-10, ARB-13, ARB-15, ARB-17, ARB-18, ARB-21, ARB-22, ARB-23, ARB-24, ARB-25, ARB-26, ARB-27, ARB-33, ARB-34, ARB-36, ARB-39, ARB-40, ARB-44, ARB-45, ARB-46, ARB-47, ARB-48, ARB-49, ARB-50, ARB-51, ARB-52, ARB-53, ARB-54, ARB-55, ARB-56, ARB-57, ARB-58, ARB-59, ARB-60, ARB-61, ARB-62, ARB-63, ARB-64, ARB-65, ARB-66, ARB-67, ARB-68, ARB-69, ARB-70, ARB-71, ARB-72, ARB-73, ARB-74, ARB-75, ARB-76, ARB-77, ARB-78, ARB-79, ARB-80, ARB-81, ARB-82, ARB-83 → OK · Hallazgos: —
## Fallos (filas FAIL de test-results.tsv, tal cual)

    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 12 (Wolf «Wolf(Clone)» #32, Wolf «Wolf(Clone)» #55, ArcticFox «ArcticFox(Clone)» #75, Reindeer «Reindeer(Clone)» #77, Wolf «Wolf(Clone)» #53, ArcticFox «ArcticFox(Clone)» #60)
    [TEST] FAIL · g50 JornadaTest · [CV-03] fauna: ningún animal atascado > 20 s, dando vueltas o volcado — da vueltas en círculo: 15 (MuskOx «MuskOx(Clone)» #17, Penguin «Penguin(Clone)» #12, MuskOx «MuskOx(Clone)» #63, Reindeer «Reindeer(Clone)» #38, MuskOx «MuskOx(Clone)» #33, Bear «Bear(Clone)» #55)
    [TEST] FAIL · g50 JornadaTest · día 2: nadie dentro de muebles, en el agua, cayendo ni volcado — JefeConstruccion ×1 («Tierra_orilla» en fuera de las áreas (-50,0,-61) (Patrulla de los puestos de vigía) [cuerpo: centro y 0,09, pies -0,38, raíz y 0,16], día 2 12,99 h)

## Notas

Último «[Fauna] contadores»:

    [Fauna] contadores: StaleRepaths=124 NavReach.BadPaths=15 HomeLegs=1211 (rectos 55) EdgeNudges=86 FrozenRestarts=301 FrozenTrips=143 FrozenWarps=57 ComidasRepetidas=0 BadPaths(viajes)=7 NoPathTrips=198 IdleRefused=0 Vigilados=16074 Rescates=559 (repetidos 283) CaminosMantenidos=16609 AlMasCercano=0 (sin sitio 0) Montículos=2 BocasMovidas=1 PenWalker=2607/2602/3 (empezados/llegados/sin sitio)

Salida del probador: `r117.out`. Run log escrito por `qa_runlog.sh` desde esa salida.
