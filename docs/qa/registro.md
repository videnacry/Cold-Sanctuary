# Registro de estados

<!-- GENERADO por docs/qa/migracion/generar.pl el 2026-11-13 desde docs/auditoria-peticiones.md, docs/manual-de-diseno.md y los tests. Desde aquí se mantiene a mano. -->

Una fila por caso de [casos.md](casos.md). **Sin run log no hay «Probado»**. Estados:
`Probado (suite gN)` · `Probado (en vivo)` · `Probado (retro)` (solo de la migración, con run log reconstruido) · `Parcial` · `FAIL` · `Inestable` (pasa y falla en el mismo commit) · `Crónico` (el mismo check falla en ≥ 3 rondas seguidas: pide `B-nn` y dueño) · `Solo test` · `Solo código` (programado y compilado, sin ejecutar) · `Sin probar` · `No aplica` · `Retirado`.

Una evidencia **caduca** si `TestImpact` dice que un commit posterior toca su grupo: el comprobador la marca «a revisar».

| ID | Estado | Fecha | Commit | Run log | B/HC | Nota |
|---|---|---|---|---|---|---|
| ARB-01 | Probado (retro) | 2026-11-13 | c8fcfd2 | [runs/2026-11-13-0426-r73-retro.md](runs/2026-11-13-0426-r73-retro.md) | — | auditoría: verificado en vivo 2026-11-13 |
| ARB-02 | Probado (retro) | 2026-11-13 | c8fcfd2 | [runs/2026-11-13-0426-r73-retro.md](runs/2026-11-13-0426-r73-retro.md) | — | auditoría: verificado en vivo 2026-11-13 |
| ARB-03 | Solo código | — | — | — | — | sin anotar en el checklist (auditoría); hay código |
| ARB-04 | Probado (retro) | 2026-11-13 | c8fcfd2 | [runs/2026-11-13-0426-r73-retro.md](runs/2026-11-13-0426-r73-retro.md) | — | auditoría: verificado en vivo 2026-11-13 |
| MIC-01 | Solo código | — | — | — | — | sin anotar en el checklist (auditoría); hay código |
| PX-01 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12; también runs/2026-11-06-0127-r13-retro.md |
| ARB-05 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-06 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-07 | Solo código | — | — | — | — | sin anotar en el checklist (auditoría); hay código |
| ARB-08 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| JOR-01 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12; también runs/2026-11-05-1544-r10-retro.md |
| MIC-02 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| JOR-02 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-09 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-10 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-11 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-12 | Sin probar | — | — | — | — | sin anotar en el checklist (auditoría); sin código localizado |
| ARB-13 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-14 | Solo código | — | — | — | — | auditoría: Sin verificar en vivo |
| MIC-03 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo |
| MIC-04 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo |
| MIC-05 | Solo código | — | — | — | — | auditoría: Sin verificar en vivo |
| MIC-06 | Sin probar | — | — | — | — | sin anotar en el checklist (auditoría); sin código localizado |
| MIC-07 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-08 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12; también runs/2026-11-08-2045-r30-retro.md |
| MIC-09 | Sin probar | — | — | — | — | sin anotar en el checklist (auditoría); sin código localizado |
| MIC-10 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo |
| MIC-11 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-12 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| FAU-01 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| ARB-15 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-13 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo |
| JOR-03 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| JOR-04 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-14 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-15 | Probado (suite g66) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g66 PakalStoryTest 23/0 |
| MIC-16 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo |
| ARB-16 | Sin probar | — | — | — | — | abierta en la auditoría: falta código |
| ARB-17 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-18 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| FAU-02 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-19 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| FAU-03 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-17 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo |
| OP-09 | Sin probar | — | — | — | — | sin anotar en el checklist (auditoría); sin código localizado |
| ARB-20 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-21 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| HOS-01 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| HOS-02 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| HOS-03 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| HOS-04 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| YOG-01 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-18 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12; también runs/2026-11-10-0949-r34-retro.md |
| MIC-19 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-20 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-21 | Probado (retro) | 2026-11-12 | 2ac2bbc | [runs/2026-11-12-2056-r66-retro.md](runs/2026-11-12-2056-r66-retro.md) | — | auditoría: verificado en vivo 2026-11-12; también runs/2026-11-12-2144-r67-retro.md, runs/2026-11-12-2314-r68-retro.md |
| MIC-22 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-22 | Probado (retro) | 2026-11-13 | c8fcfd2 | [runs/2026-11-13-0426-r73-retro.md](runs/2026-11-13-0426-r73-retro.md) | — | auditoría: verificado en vivo 2026-11-13 |
| REN-01 | Solo código | — | — | — | — | auditoría: Sin verificar en vivo |
| MIC-23 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-24 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| PER-01 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-25 | Solo código | — | — | — | — | sin anotar en el checklist (auditoría); hay código |
| JOR-05 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| JOR-06 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-23 | Probado (suite g74) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g74 InformaticaAulaTest 28/0 |
| ARB-24 | Probado (suite g74) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g74 InformaticaAulaTest 28/0 |
| NUC-01 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-26 | Probado (suite g76) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g76 AlertaPisoNuevoTest 13/0 |
| ARB-25 | Probado (suite g75) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g75 CapacidadPisosTest 36/0 |
| ARB-26 | Probado (suite g75) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g75 CapacidadPisosTest 36/0 |
| JOR-07 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-27 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12; también runs/2026-11-10-0949-r34-retro.md |
| MIC-28 | Probado (retro) | 2026-11-12 | 2ac2bbc | [runs/2026-11-12-2056-r66-retro.md](runs/2026-11-12-2056-r66-retro.md) | — | auditoría: verificado en vivo 2026-11-12; también runs/2026-11-12-2144-r67-retro.md, runs/2026-11-12-2314-r68-retro.md |
| JOR-08 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| JOR-09 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| PX-02 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| GYM-01 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| JOR-10 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-29 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-30 | Solo código | — | — | — | HC-02 | auditoría: Sin verificar; una parte abierta |
| PER-02 | Probado (suite g95) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g95 PersonajesReglasTest 12/0 |
| JOR-11 | Sin probar | — | — | — | — | abierta en la auditoría: falta código |
| ARB-27 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| NUC-02 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12; también runs/2026-11-08-2045-r30-retro.md |
| MIC-31 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-32 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-33 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| UI-01 | Probado (suite g93) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g93 HudModalsTest 13/0 |
| YOG-02 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ENE-01 | Solo código | — | — | — | — | auditoría: Sin verificar en vivo |
| ENE-02 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ENE-03 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| FAU-04 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| FAU-05 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| MIC-34 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-35 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-28 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| FAU-06 | Probado (suite g85) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g85 CadenaAlimenticiaTest 58/0 |
| ARB-29 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-36 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-37 | Sin probar | — | — | — | — | abierta en la auditoría: falta código |
| NUC-03 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| FAU-07 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-38 | Solo código | — | — | — | — | auditoría: Sin verificar en vivo |
| NUC-04 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-30 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| FAU-08 | Probado (suite g105) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g105 AlimentadorTest 44/0 |
| JOR-12 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| JOR-13 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| MIC-39 | Probado (suite g77) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g77 AsambleaTest 46/0 |
| ARB-31 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| PER-03 | Solo código | — | — | — | — | auditoría: Hecho (doc); sin verificar |
| ARB-32 | Solo código | — | — | — | — | auditoría: Sin verificar en vivo |
| NUC-05 | Solo código | — | — | — | — | auditoría: Sin verificar en vivo |
| FAU-09 | Probado (suite g85) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g85 CadenaAlimenticiaTest 58/0 |
| ARB-33 | Probado (suite g103) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g103 ProduccionTest 16/0 |
| FAU-10 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| NUC-06 | Solo código | — | — | — | — | auditoría: Sin verificar en vivo |
| JOR-14 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| NUC-07 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| NUC-08 | Sin probar | — | — | — | — | sin anotar en el checklist (auditoría); sin código localizado |
| FAU-11 | Probado (suite g108) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g108 IcebergVidaTest 14/0 |
| NUC-09 | Probado (suite g110) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g110 AereoTest 27/0 |
| FAU-12 | Probado (suite g111) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g111 FaunaFriaTest 22/0 |
| ARB-34 | Probado (suite g91) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g91 GuardRoomTest 45/0 |
| ARB-35 | Solo código | — | — | — | — | auditoría: Sin verificar en vivo |
| ARB-36 | Probado (suite g91) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g91 GuardRoomTest 45/0 |
| ARB-37 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-38 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| REN-02 | Sin probar | — | — | — | — | sin anotar en el checklist (auditoría); sin código localizado |
| JOR-15 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| FAU-13 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| NUC-10 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| JOR-16 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-39 | Probado (suite g75) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g75 CapacidadPisosTest 36/0 |
| PER-04 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12; también runs/2026-11-08-1136-r23-retro.md |
| PER-05 | Probado (suite g95) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g95 PersonajesReglasTest 12/0 |
| ARB-40 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12; también runs/2026-11-08-1136-r23-retro.md |
| JOR-17 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-41 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12; también runs/2026-11-09-1823-r31-retro.md |
| ARB-42 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| JOR-18 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| REN-03 | Probado (retro) | 2026-11-13 | c8fcfd2 | [runs/2026-11-13-0426-r73-retro.md](runs/2026-11-13-0426-r73-retro.md) | — | auditoría: verificado en vivo 2026-11-13 |
| FAU-14 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | auditoría: verificado en vivo 2026-11-12 |
| ARB-43 | Solo código | — | — | — | — | sin anotar en el checklist (auditoría); hay código |
| REN-04 | Sin probar | — | — | — | — | abierta en la auditoría: falta código |
| ARB-44 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-45 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-46 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-47 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-48 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-49 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-50 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-51 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-52 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-53 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-54 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-55 | Probado (suite g75) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g75 CapacidadPisosTest 36/0 |
| ARB-56 | Probado (suite g77) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g77 AsambleaTest 46/0 |
| ARB-57 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-58 | Probado (suite g91) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g91 GuardRoomTest 45/0 |
| MIC-40 | Probado (suite g60) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g60 BruceLeeStoryTest 49/0 |
| MIC-41 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-42 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-43 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-44 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-45 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-46 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-47 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-48 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-49 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-50 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-51 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-52 | Probado (suite g77) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g77 AsambleaTest 46/0 |
| MIC-53 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-59 | Probado (suite g53) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: g53 TreeOfFieldsTest 46/0 |
| ARB-60 | Probado (suite g53) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: g53 TreeOfFieldsTest 46/0 |
| ARB-61 | Probado (suite g53) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: g53 TreeOfFieldsTest 46/0 |
| ARB-62 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-63 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-64 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-65 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-66 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-67 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-68 | Probado (suite g53) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: g53 TreeOfFieldsTest 46/0 |
| ARB-69 | Probado (suite g53) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: g53 TreeOfFieldsTest 46/0 |
| ARB-70 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-71 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-72 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-73 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-74 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-75 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-76 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-77 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-78 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-79 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-80 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-81 | Probado (suite g110) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g110 AereoTest 27/0 |
| ARB-82 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-83 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-84 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-85 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-86 | Parcial | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | B-01 | g50 con 4 FAIL en la suite 63 (fauna atascada, B-01, y las misiones de Kushal) |
| ARB-87 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-88 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-89 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-90 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-91 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-92 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-93 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-94 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| ARB-95 | Probado (suite g53) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: g53 TreeOfFieldsTest 46/0 |
| ARB-96 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| HOS-05 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| HOS-06 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| HOS-07 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| HOS-08 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| HOS-09 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-54 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-55 | Probado (suite g60) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g60 BruceLeeStoryTest 49/0 |
| MIC-56 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-57 | Probado (suite g60) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g60 BruceLeeStoryTest 49/0 |
| MIC-58 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-59 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-60 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-61 | Probado (suite g13) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g13 IdealStateTest 9/0 |
| MIC-62 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-63 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-64 | Probado (suite g63) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g63 MicroBuildRulesTest 16/0 |
| MIC-65 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-66 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-67 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-68 | Probado (suite g11) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g11 ObservationTest 4/0 |
| MIC-69 | Probado (suite g12) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g12 StateSpellsTest 6/0 |
| MIC-70 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-71 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-72 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-73 | Probado (suite g63) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g63 MicroBuildRulesTest 16/0 |
| MIC-74 | Probado (suite g63) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g63 MicroBuildRulesTest 16/0 |
| MIC-75 | Probado (suite g63) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g63 MicroBuildRulesTest 16/0 |
| MIC-76 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| MIC-77 | Probado (suite g9) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g9 RiverCurrentTest 5/0 |
| MIC-78 | Probado (suite g9) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g9 RiverCurrentTest 5/0 |
| MIC-79 | Probado (suite g10) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g10 DietPreferenceTest 5/0 |
| MIC-80 | Probado (suite g7) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g7 StarvationTest 2/0 |
| MIC-81 | Probado (suite g8) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g8 SleepTest 4/0 |
| MIC-82 | Probado (suite g15) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g15 YogaTest 4/0 |
| FAU-15 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| FAU-16 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| FAU-17 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| FAU-18 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| FAU-19 | Probado (suite g11) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g11 ObservationTest 4/0 |
| JOR-19 | Parcial | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | B-01 | g50 con 4 FAIL en la suite 63 (fauna atascada, B-01, y las misiones de Kushal) |
| JOR-20 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-21 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-22 | Parcial | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | B-01 | g50 con 4 FAIL en la suite 63 (fauna atascada, B-01, y las misiones de Kushal) |
| JOR-23 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-24 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-25 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-26 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-27 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-28 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-29 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-30 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-31 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-32 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-33 | Probado (suite g75) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g75 CapacidadPisosTest 36/0 |
| JOR-34 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-35 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-36 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-37 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-38 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-39 | Parcial | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | B-01 | g50 con 4 FAIL en la suite 63 (fauna atascada, B-01, y las misiones de Kushal) |
| JOR-40 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-41 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-42 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-43 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-44 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-45 | Parcial | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | B-01 | g50 con 4 FAIL en la suite 63 (fauna atascada, B-01, y las misiones de Kushal) |
| JOR-46 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-47 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-48 | Parcial | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | B-01 | g50 con 4 FAIL en la suite 63 (fauna atascada, B-01, y las misiones de Kushal) |
| JOR-49 | Parcial | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | B-01 | g50 con 4 FAIL en la suite 63 (fauna atascada, B-01, y las misiones de Kushal) |
| JOR-50 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-51 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-52 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-53 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| JOR-54 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| PER-06 | Probado (suite g95) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g95 PersonajesReglasTest 12/0 |
| PER-07 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| PER-08 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| PER-09 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| PER-10 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| PER-11 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| PER-12 | Probado (suite g95) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g95 PersonajesReglasTest 12/0 |
| REN-05 | Probado (suite g112) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g112 LoteEstaticoTest 16/0 |
| REN-06 | Parcial | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | B-01 | g50 con 4 FAIL en la suite 63 (fauna atascada, B-01, y las misiones de Kushal) |
| REN-07 | Parcial | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | B-01 | g50 con 4 FAIL en la suite 63 (fauna atascada, B-01, y las misiones de Kushal) |
| FAU-20 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| FAU-21 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| FAU-22 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| FAU-23 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| FAU-24 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| FAU-25 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| FAU-26 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| FAU-27 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| FAU-28 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| FAU-29 | Probado (suite g105) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g105 AlimentadorTest 44/0 |
| FAU-30 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| PER-13 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| PER-14 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| PER-15 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| PER-16 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| PER-17 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| PER-18 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| PER-19 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| PER-20 | Probado (suite g108) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g108 IcebergVidaTest 14/0 |
| PER-21 | Probado (suite g103) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g103 ProduccionTest 16/0 |
| PER-22 | Probado (retro) | 2026-11-12 | d0542db | [runs/2026-11-12-1208-r63-retro.md](runs/2026-11-12-1208-r63-retro.md) | — | su grupo en verde en la suite 63 |
| CV-01 | Probado (suite g50) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: check [CV-01] PASS ×3 |
| CV-02 | Sin probar | — | — | — | — | caso nuevo: falta el Check con [CV-02] (lo añade quien toque la prueba) |
| CV-03 | Probado (suite g50) | 2026-11-13 | cfd8be3 | [runs/2026-11-13-1731-r86-rapida-QA.md](runs/2026-11-13-1731-r86-rapida-QA.md) | — | r86: check [CV-03] PASS ×1 |
| CV-04 | Probado (suite g1) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: check [CV-04] PASS ×1 |
| CV-05 | Probado (suite g1) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: check [CV-05] PASS ×1 |
| CV-06 | Probado (suite g53) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: check [CV-06] PASS ×1 |
| CV-07 | Probado (suite g50) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: check [CV-07] PASS ×3 |
| CV-08 | Sin probar | — | — | — | — | caso nuevo: falta el Check con [CV-08] (lo añade quien toque la prueba) |
| CV-09 | Sin probar | — | — | — | — | caso nuevo: falta el Check con [CV-09] (lo añade quien toque la prueba) |
| CV-10 | Sin probar | — | — | — | — | caso nuevo: falta el Check con [CV-10] (lo añade quien toque la prueba) |
| CV-11 | Probado (suite g105) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: check [CV-11] PASS ×3 |
| CV-12 | Sin probar | — | — | — | — | caso nuevo: falta el Check con [CV-12] (lo añade quien toque la prueba) |
| CV-13 | Sin probar | — | — | — | — | caso nuevo: falta el Check con [CV-13] (lo añade quien toque la prueba) |
| CV-14 | Probado (suite g50) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: check [CV-14] PASS ×3 |
| CV-15 | Probado (suite g50) | 2026-11-14 | 1d2053b | [runs/2026-11-14-1546-r110-grupos-vivo.md](runs/2026-11-14-1546-r110-grupos-vivo.md) | — | r110: check [CV-15] PASS ×2 |
| OP-01 | No aplica | — | — | — | — | regla de proceso: la mira el probador en cada run log |
| OP-02 | No aplica | — | — | — | — | regla de proceso: la mira el probador en cada run log |
| OP-03 | No aplica | — | — | — | — | regla de proceso: la mira el probador en cada run log |
| OP-04 | No aplica | — | — | — | — | regla de proceso: la mira el probador en cada run log |
| OP-05 | No aplica | — | — | — | — | regla de proceso: la mira el probador en cada run log |
| OP-06 | No aplica | — | — | — | — | regla de proceso: la mira el probador en cada run log |
| OP-07 | No aplica | — | — | — | — | regla de proceso: la mira el probador en cada run log |
| OP-08 | No aplica | — | — | — | — | regla de proceso: la mira el probador en cada run log |
| MIC-83 | Sin probar | — | — | — | — | añadido a mano el 2026-11-13; su grupo pasó en la suite 63 (d0542db): el probador decide si pasa a Probado |
| PER-23 | Probado (suite g95) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g95 PersonajesReglasTest 12/0 |
| PER-24 | Probado (suite g103) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g103 ProduccionTest 16/0 |
| FAU-31 | Probado (suite g99) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g99 TerritorioTest 80/0 |
| FAU-32 | Probado (suite g85) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g85 CadenaAlimenticiaTest 58/0 |
| REN-08 | Probado (suite g118) | 2026-11-14 | 3fdee7f | [runs/2026-11-14-1342-r109-rapida-grupos-vivo.md](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) | — | r109: g118 QaCheckTest 44/0 |

**Recuento:** No aplica 8 · Parcial 9 · Probado (retro) 285 · Sin probar 31 · Solo código 16 · total 349.

## Inestables y crónicos

Checks concretos (el texto del `TestProbe.Check`), no casos enteros. Se mira en [contadores.md](contadores.md) y en los run logs.

| Check | Grupo | Estado | Rondas | B/HC | Dueño | Nota |
|---|---|---|---|---|---|---|
| fauna: ningún animal atascado > 20 s, dando vueltas o volcado | 50 | Crónico | r71–r80, r82, r83 (12 rondas con g50; r81 no corrió g50) | B-01 | probador → trabajador de fauna | contadores.md: 4, 12, 11, 9, 17, 27, 34, 5, 6, 10, 7, 14 |
| fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo | 50 | Inestable | FAIL en r73, r78, r80, r82; pasa en r74–r77, r79, r83 | — | sin dueño | H6: falló y pasó en commits distintos; falta repetirlo en un mismo commit para confirmarlo |
| un paseo sin salto posible descansa a propósito / …y prueba otra dirección (gira ≥ 90°) | 1 | Inestable | FAIL en r77 (6 líneas); pasa en r73–r75, r78, r79, r82, r83 | — | sin dueño | H5: falló y pasó en commits distintos; falta repetirlo en un mismo commit |
| día 1/2: Kushal completa TODAS sus misiones | 50 | Inestable | FAIL en la suite 63 (d0542db); pasa en la suite r71 y en r72–r83 | — | sin dueño | runs/2026-11-12-1208-r63-retro.md |
