---
resumen: "Una fila por ronda de Unity. El probador la copia del run log. Si un contador sube más de un 50 % sobre la media de las 3 rondas anteriores, se abre un Hn en hallazgos.md aunque todo pase."
temas: [pruebas, unity]
estado: vigente
---
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
| r93 | 2026-11-13 | c8d5e09 | grupos + vivo | 1 50 | 86/1 | 4 | 2 | 0 | 0 | — | — | — | 46 min | [r93](runs/2026-11-13-2235-r93-grupos-vivo.md) |
| r94 | 2026-11-14 | 9a12e54 | grupos + vivo | 1 50 53 | 133/1 | 2 | 5 | 0 | 0 | — | — | — | 49 min | [r94](runs/2026-11-14-2335-r94-grupos-vivo.md) |
| r95 | 2026-11-14 | 3a3456c | vivo | — | — | — | — | 0 | 0 | — | — | — | — | [r95](runs/2026-11-14-0057-r95-vivo.md) |
| r96 | 2026-11-14 | 6f30db6 | grupos + vivo | 1 50 53 | 133/1 | 14 | 4 | 0 | 0 | — | — | — | 50 min | [r96](runs/2026-11-14-0132-r96-grupos-vivo.md) |
| r97 | 2026-11-14 | e510a17 | grupos + vivo | 1 50 53 | 134/1 | 2 | 3 | 0 | 0 | — | — | — | 49 min | [r97](runs/2026-11-14-0232-r97-grupos-vivo.md) |
| r98 | 2026-11-14 | 15021c8 | grupos + vivo | 1 50 53 | 134/1 | — | 3 | 0 | 0 | — | — | — | 50 min | [r98](runs/2026-11-14-0332-r98-grupos-vivo.md) |
| r99 | 2026-11-14 | 7ae301b | rápida + grupos + vivo | 1 50 53 | rápida 1070/0 · 134/2 | 2 | — | 0 | 0 | — | — | — | 55 min | [r99](runs/2026-11-14-0425-r99-rapida-grupos-vivo.md) |
| r100 | 2026-11-14 | 64e2bd7 | grupos + vivo | 1 50 53 | 136/1 | 5 | — | 0 | 0 | — | — | — | 51 min | [r100](runs/2026-11-14-0523-r100-grupos-vivo.md) |
| r101 | 2026-11-14 | 526ce1f | grupos + vivo | 1 50 53 | 136/1 | 4 | 2 | 0 | 0 | — | — | — | 55 min | [r101](runs/2026-11-14-0617-r101-grupos-vivo.md) |
| r102 | 2026-11-14 | b184a31 | grupos + vivo | 1 50 53 | 136/1 | 3 | — | 0 | 0 | — | — | — | 52 min | [r102](runs/2026-11-14-0714-r102-grupos-vivo.md) |
| r103 | 2026-11-14 | 3555f59 | grupos + vivo | 1 50 53 | 136/1 | 4 | — | 0 | 0 | — | — | — | 51 min | [r103](runs/2026-11-14-0809-r103-grupos-vivo.md) |
| r104 | 2026-11-14 | ca2612b | grupos + vivo | 1 50 53 | 136/1 | 5 | — | 0 | 0 | — | — | — | 52 min | [r104](runs/2026-11-14-0902-r104-grupos-vivo.md) |
| r105 | 2026-11-14 | 6a792ac | rápida + grupos + vivo | 1 50 53 | rápida 1071/0 · 136/1 | — | 1 | 0 | 0 | — | — | — | 55 min | [r105](runs/2026-11-14-0957-r105-rapida-grupos-vivo.md) |
| r106 | 2026-11-14 | c555fe1 | grupos + vivo | 1 50 53 | 136/1 | 1 | 1 | 0 | 0 | — | — | — | 51 min | [r106](runs/2026-11-14-1055-r106-grupos-vivo.md) |
| r107 | 2026-11-14 | 102e3e5 | grupos + vivo | 1 50 53 | 136/1 | 2 | 2 | 0 | 0 | — | — | — | 52 min | [r107](runs/2026-11-14-1149-r107-grupos-vivo.md) |
| r108 | 2026-11-14 | 580c6bf | rápida + grupos + vivo | 1 50 53 | rápida 1071/0 · 136/1 | 2 | — | 0 | 0 | — | — | — | 55 min | [r108](runs/2026-11-14-1244-r108-rapida-grupos-vivo.md) |
| r109 | 2026-11-14 | 3fdee7f | rápida + grupos + vivo | 1 50 53 | rápida 1071/0 · 136/1 | 1 | — | 0 | 0 | — | — | — | 122 min | [r109](runs/2026-11-14-1342-r109-rapida-grupos-vivo.md) |
| r110 | 2026-11-14 | 1d2053b | grupos + vivo | 1 50 53 | 135/2 | — | 1 | 0 | 0 | — | — | — | 52 min | [r110](runs/2026-11-14-1546-r110-grupos-vivo.md) |
| r111 | 2026-11-14 | 954795e | grupos + vivo | 1 50 53 | 135/2 | — | 1 | 0 | 0 | — | — | — | 47 min | [r111](runs/2026-11-14-1641-r111-grupos-vivo.md) |
| r112 | 2026-11-14 | 6a0526f | rápida + grupos + vivo | 1 50 53 | rápida 1071/0 · 136/1 | 1 | — | 0 | 0 | — | — | — | 54 min | [r112](runs/2026-11-14-1732-r112-rapida-grupos-vivo.md) |
| r113 | 2026-11-14 | 5481d81 | grupos + vivo | 1 50 53 | 136/1 | 1 | 2 | 0 | 0 | — | — | — | 48 min | [r113](runs/2026-11-14-1832-r113-grupos-vivo.md) |
| r114 | 2026-11-14 | 1e3f4f2 | suite | todos | suite 4756/2 | — | 2 | 0 | 0 | — | — | — | 82 min | [r114](runs/2026-11-14-1927-r114-suite.md) |
| r115 | 2026-11-14 | d6c0d5f | rápida + grupos + vivo + suite | 1 50 53 | rápida 1071/0 · 138/0 · suite 4753/3 | — | — | 0 | 0 | — | — | — | 125 min | [r115](runs/2026-11-14-2051-r115-rapida-grupos-vivo-suite.md) |
| r116 | 2026-11-15 | 05cc591 | rápida + grupos + vivo + suite | 1 50 53 | rápida 1071/0 · 136/2 · suite 4756/2 | 1 | 12 | 0 | 0 | — | — | — | 123 min | [r116](runs/2026-11-15-2337-r116-rapida-grupos-vivo-suite.md) |
| r117 | 2026-11-15 | 49274ea | rápida + grupos + vivo + suite | 1 50 53 | rápida 1071/0 · 136/2 · suite 4759/1 | — | 12 | 0 | 0 | — | — | — | 124 min | [r117](runs/2026-11-15-0213-r117-rapida-grupos-vivo-suite.md) |
| r118 | 2026-11-15 | aaf1d5f | rápida + grupos + vivo + suite | 1 50 53 | rápida 1071/0 · 137/1 · suite 4756/2 | — | 2 | 0 | 0 | — | — | — | 127 min | [r118](runs/2026-11-15-0450-r118-rapida-grupos-vivo-suite.md) |
| r119 | 2026-11-15 | 9a7fbe6 | rápida + grupos + vivo + suite | 1 50 53 | rápida 1072/0 · 137/2 · suite 4757/2 | 1 | 1 | 0 | 0 | — | — | — | 128 min | [r119](runs/2026-11-15-0730-r119-rapida-grupos-vivo-suite.md) |
| r120 | 2026-11-15 | a813bb6 | rápida + grupos + vivo + suite | 1 50 53 | rápida 1072/0 · 138/1 · suite 4756/2 | 1 | 4 | 0 | 0 | — | — | — | 127 min | [r120](runs/2026-11-15-0947-r120-rapida-grupos-vivo-suite.md) |
| r121 | 2026-11-15 | 1b4f1c1 | rápida + grupos + vivo + suite | 1 50 53 | rápida 1073/0 · 139/1 · suite 4761/2 | — | 1 | 0 | 0 | — | — | — | 127 min | [r121](runs/2026-11-15-1237-r121-rapida-grupos-vivo-suite.md) |
| r122 | 2026-11-16 | 403473b | rápida + grupos + vivo + suite | 1 50 53 | rápida 1074/0 · 414/2 · suite 4766/1 | 1 | 2 | 0 | 0 | — | — | — | 133 min | [r122](runs/2026-11-16-1848-r122-rapida-grupos-vivo-suite.md) |
| r123 | 2026-11-17 | 56a862e | rápida + grupos + vivo + suite | 1 50 53 113 28 | rápida 1074/0 · 415/1 · suite 4766/2 | 1 | 3 | 0 | 0 | — | — | — | 129 min | [r123](runs/2026-11-17-2346-r123-rapida-grupos-vivo-suite.md) |
| r124 | 2026-11-17 | f4c3f4e | rápida + grupos + vivo + suite | 1 50 53 113 28 | rápida 1072/2 · 415/1 · suite 4763/4 | 1 | 1 | 0 | 0 | — | — | — | 133 min | [r124](runs/2026-11-17-1351-r124-rapida-grupos-vivo-suite.md) |
| r125 | 2026-11-17 | 4127332 | rápida + grupos + vivo + suite | 1 50 53 113 28 | rápida 1075/0 · 415/2 · suite 4766/2 | — | 1 | 0 | 0 | — | — | — | 144 min | [r125](runs/2026-11-17-1900-r125-rapida-grupos-vivo-suite.md) |
| r126 | 2026-11-18 | 801b23c | rápida + grupos + vivo + suite | 1 50 53 113 28 | rápida 1075/1 · 417/1 · suite 4769/1 | 1 | — | 0 | 0 | — | — | — | 127 min | [r126](runs/2026-11-18-0310-r126-rapida-grupos-vivo-suite.md) |
| r127 | 2026-11-18 | 40f86d1 | rápida + grupos + vivo + suite | 1 50 53 113 28 | rápida 1076/1 · 419/0 · suite 4770/0 | — | — | 0 | 0 | — | — | — | 142 min | [r127](runs/2026-11-18-0612-r127-rapida-grupos-vivo-suite.md) |
| r131 | 2026-11-19 | f92acc1 | rápida + grupos + vivo + suite | 1 50 53 113 28 63 84 106 | rápida 1147/0 · 482/0 | — | — | 0 | 0 | — | — | — | 50 min | [r131](runs/2026-11-19-0640-r131-rapida-grupos-vivo-suite.md) |
| rp7 | 2026-11-19 | 9516ed1 | rápida + grupos | 122 125 124 120 119 1 53 | rápida 1147/0 · 184/0 | — | — | 0 | 0 | — | — | — | — | [rp7](runs/2026-11-19-0154-rp7-rapida-grupos.md) |
| rp4 | 2026-11-18 | 585235b | rápida + grupos | 125 124 69 119 121 122 63 1 53 | rápida 1115/0 · 289/1 | — | — | 0 | 0 | — | — | — | — | [rp4](runs/2026-11-18-2150-rp4-rapida-grupos.md) |

**Lo que se ve en fila** (propuesta §1.3): «atascado > 20 s» saltó a 27 y 34 en r76–r77, justo cuando se quitó el Warp (`52d2949`), y
bajó a 5–6 en r78–r79. Es el caso CV-03. En todas las rondas que corrieron g50 desde r71, el check de fauna falló: B-01, crónico.
