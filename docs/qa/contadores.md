# Contadores por ronda

Una fila por ronda de Unity. El probador la copia del run log. **Si un contador sube más de un 50 % sobre la media de las 3 rondas
anteriores, se abre un `Hn`** en [hallazgos.md](hallazgos.md) aunque todo pase.

Las filas r70–r84 se sacaron el 2026-11-13 de las salidas del probador (`r70.out`…`r84.out`) con grep: «atascado > 20 s: N» y «da
vueltas en círculo: N» del primer FAIL de fauna de g50, y las líneas TOTAL. «—» = la ronda no lo midió o no consta. El commit es el de
`claude/edificios` justo antes de la hora del fichero (reconstruido). [ECO], FPS y memoria no constan en esas salidas.

| Ronda | Fecha | Commit | Modo | Grupos | Resultado | Atascado > 20 s | Da vueltas | FUGA | TIMEOUT | [ECO] vivos | FPS | Unity GB | Duración | Run log |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| r70 | 2026-11-13 | 4772b7a | rápida + grupos | 117 | rápida 1001/0 · g117 3/4 | — | — | 0 | 1 | — | — | — | 25 min | — |
| r71 | 2026-11-13 | 4583e5b | suite | todos | 4677/5 | 4 | 11 | 0 | 1 | — | — | — | — | — |
| r72 | 2026-11-13 | ecd1f0b | grupos + vivo | 50 117 | 52/2 | 12 | 7 | 0 | 0 | — | — | — | 41 min | — |
| r73 | 2026-11-13 | c8fcfd2 | rápida + grupos + vivo | 1 50 117 | rápida 1001/0 · 72/2 | 11 | 13 | 0 | 0 | — | — | — | 43 min | [r73](runs/2026-11-13-0426-r73-retro.md) |
| r74 | 2026-11-13 | f37a627 | rápida + grupos + vivo | 1 50 109 117 | rápida 1002/0 · 84/2 | 9 | 14 | 0 | 0 | — | — | — | 45 min | — |
| r75 | 2026-11-13 | 63de4fd | rápida + grupos + vivo | 1 50 117 | rápida 1002/0 · 73/1 | 17 | 3 | 0 | 0 | — | — | — | 48 min | — |
| r76 | 2026-11-13 | 788ac35 | grupos + vivo | 50 | 45/2 | **27** | 3 | 0 | 0 | — | — | — | 30 min | — |
| r77 | 2026-11-13 | 21a5a04 | rápida + grupos + vivo | 1 50 | rápida 1004/2 · 68/3 | **34** | 2 | 0 | 0 | — | — | — | 48 min | — |
| r78 | 2026-11-13 | 1cb357d | rápida + grupos + vivo | 1 50 | rápida 1007/0 · 70/2 | 5 | 5 | 0 | 0 | — | — | — | 47 min | — |
| r79 | 2026-11-13 | 9b1c528 | rápida + grupos + vivo | 1 50 | rápida 1008/0 · 72/1 | 6 | 6 | 0 | 0 | — | — | — | 47 min | — |
| r80 | 2026-11-13 | a6033a3 | suite | todos | 4692/2 | 10 | 3 | 0 | 0 | — | — | — | — | — |
| r81 | 2026-11-13 | — | build WebGL | — | builds OK (65 y 31 min) | — | — | 0 | 0 | — | — | — | 1 h 52 min | — |
| r82 | 2026-11-13 | ee35741 | rápida + grupos + vivo | 1 50 | rápida 1012/0 · 76/2 | 7 | 2 | 0 | 0 | — | — | — | 71 min | — |
| r83 | 2026-11-13 | e35cea2 | rápida + grupos + vivo | 1 50 | rápida 1017/0 · 82/1 | 14 | 3 | 0 | 0 | — | — | — | 51 min | — |
| r84 | 2026-11-13 | 65aecfe (web-ligera) | build WebGL | — | builds OK (29 y 20 min) · cs-web-pages 1193cbb · compila limpio | — | — | 0 | 0 | — | — | — | 62 min | — |
| r85 | 2026-11-13 | 41716c9 | rápida + grupos + vivo | 1 50 | rápida 1018/0 · 82/1 | 5 | 2 | 0 | 0 | — | — | — | 51 min | [r85](runs/2026-11-13-1636-r85-rapida-grupos-vivo.md) |
| r86 | 2026-11-13 | cfd8be3 | rápida + QA | rápida | rápida 1065/0 | — | — | 0 | 0 | — | — | — | 8 min | [r86](runs/2026-11-13-1731-r86-rapida-QA.md) |
| r88 | 2026-11-13 | 9eea10d | grupos + vivo | 1 50 | 84/1 | 20 | 1 | 0 | 0 | — | — | — | 48 min | [r88](runs/2026-11-13-1822-r88-grupos-vivo.md) |
| r90 | 2026-11-13 | b0c7f3c | grupos + vivo | 1 50 | 84/2 | 2 | 4 | 0 | 0 | — | — | — | 47 min | [r90](runs/2026-11-13-1955-r90-grupos-vivo.md) |
| r91 | 2026-11-13 | 8689b5a | grupos + vivo | 1 50 | 85/1 | 3 | 3 | 0 | 0 | — | — | — | 47 min | [r91](runs/2026-11-13-2045-r91-grupos-vivo.md) |
| r92 | 2026-11-13 | 15b15d6 | grupos + vivo | 1 50 | 85/1 | 8 | 4 | 0 | 0 | — | — | — | 47 min | [r92](runs/2026-11-13-2134-r92-grupos-vivo.md) |

**Lo que se ve en fila** (propuesta §1.3): «atascado > 20 s» saltó a 27 y 34 en r76–r77, justo cuando se quitó el Warp (`52d2949`), y
bajó a 5–6 en r78–r79. Es el caso CV-03. En todas las rondas que corrieron g50 desde r71, el check de fauna falló: B-01, crónico.
