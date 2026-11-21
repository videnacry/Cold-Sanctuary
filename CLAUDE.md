# Cold Sanctuary

Juego Unity (C#) en un santuario animal mágico: un grupo llega a aprender de la maestra (la Magnate)
trabajando de voluntario (crías, áreas del santuario, yoga, meditación) y viaja a **microcosmos**
(pueblos históricos a escala insecto). Todo ser es un `Anima`; los stats son la fuente de verdad.
El juego quiere **enseñar ciencia**: química, biología, física e historia con fuentes reales.

Este archivo se carga en **cada** sesión: mantenlo corto, solo enlaces. El detalle va en `docs/`.

## Dónde mirar (antes de explorar)

- **Mapa del código y de los sistemas:** [`docs/ARQUITECTURA.md`](docs/ARQUITECTURA.md)
- **Ubicar una clase:** `grep -n "Clase" docs/CODEMAP.md` (generado; no leerlo entero)
- **Qué doc trata un tema:** `python3 tools/docs.py find <tema>` (o `de <Clase>`: código→doc); índice generado en [`docs/README.md`](docs/README.md)
- **Antes de reintentar algo que falló:** [`docs/conocimiento/`](docs/conocimiento/README.md) (lecciones) y [`docs/qa/hallazgos.md`](docs/qa/hallazgos.md)
- **Docs nuevos:** en su carpeta (`sistemas/`, `areas/`, `fauna/`, `microcosmos/`, `historia/`, `reglas/`, `qa/`, `plataforma/`…), con cabecera `resumen/temas/estado` (+ `codigo/modelos/animaciones/retos`); luego `python3 tools/docs.py index` (CI: `check`)
- **Tareas pendientes:** [`docs/checklist.md`](docs/checklist.md) (enorme: `grep`, no leer entero)
- **Reglas de diseño (W1, J18…):** [`docs/reglas/manual-de-diseno.md`](docs/reglas/manual-de-diseno.md) (enorme: `grep`)
- **Qué cambió:** [`CHANGELOG.md`](CHANGELOG.md)
- Personajes: `docs/personajes/` · Convenciones: [`docs/reglas/conventions.md`](docs/reglas/conventions.md)

## Repo y proyecto Unity

- El repo **solo versiona `.cs` + docs + tools**. Escenas, prefabs, `.meta`, `ProjectSettings/` y
  `Packages/` viven en el proyecto Unity de Berón: **`C:/Users/Blein/COLD-SANCTUARY`** (Unity 6000.5.1f1, URP).
  Este clon no se abre en Unity.
- Sin namespaces (todo global). Comentarios en español o inglés.
- **No tocar sin pedirlo:** escenas/prefabs (no están aquí), `.gitattributes`/`.gitignore`,
  `docs/CODEMAP.md` a mano (se regenera), el archivo `remove-after-send`.

## Comandos

```bash
python3 tools/gen_codemap.py      # obligatorio si tocas .cs: CI falla si docs/CODEMAP.md no está al día
tools/sync-to-unity.sh [--prune]  # copia los .cs al proyecto Unity (--prune si borraste/renombraste)
tools/verify-compile.sh           # compila de verdad sin Editor ni licencia (en el PC de Berón)
tools/run-live-test.sh [seg]      # prueba en vivo (blockout + Play); SOLO con permiso de Berón
```
En el Editor: `Tools/Cold Sanctuary/Verify` → `[VERIFY] PASS|FAIL`. Pruebas en escena: grep `[TEST]`.
Desde la nube no se puede compilar: se compila en el PC de Berón.

## Reglas de trabajo (de Berón)

1. **Responder en español.** Trabajar en autonomía: commit, push y PR sin pedir permiso para lo rutinario.
2. **Unity en el PC de Berón: preguntar antes** de compilar en el Editor, dar Play o reconstruir
   (él juega en ese mismo Editor). Un solo hilo usa Unity a la vez. Los demás programan, compilan y
   empujan; **un único probador** une las ramas y prueba en vivo (manual W3).
3. **Toda petición se apunta en `docs/checklist.md` al momento** (siente que se pierden tareas).
4. Al terminar algo: docs y checklist al día, y el manual si hay regla nueva (W8). Una línea en `CHANGELOG.md`.
5. **Integrar o reemplazar, no duplicar**: nada de sistemas paralelos ni código muerto. Revisar
   dependencias antes de cambiar un diseño.
6. Cambiar un piso/área/edificio-árbol/microcosmos obliga a reajustar jornada, reparto y horario libre (W9).
7. **Regla de oro:** todos los personajes del santuario saben hacer todo; las tareas van por rotación.
8. Todo rediseño: investigación → plan enseñado a Berón → construcción (W2). Contenido con fuentes reales.
9. Una sesión hace el trabajo y **otra lo revisa** contra lo pedido antes de fusionar; fusionar el PR propio
   cuando el CI esté en verde.
10. Rendimiento: nada de `FindObjects`/`GameObject.Find` por frame; reconstruir el santuario y los
    microcosmos por partes (el PC tiene 16 GB) (W5, W13).

## Ahorro de tokens

- **Lee solo lo necesario.** Empieza por `docs/ARQUITECTURA.md` o `grep` en `docs/CODEMAP.md`; no explores carpetas a ciegas.
- **Búsquedas dirigidas:** `grep -n` con la palabra exacta y abre el archivo **por rangos** (`sed -n 'a,bp'`, `Read` con offset/limit).
- **No releas archivos grandes** ni los que ya leíste en la sesión; nunca `cat` de `checklist.md`, `manual-de-diseno.md`, `CODEMAP.md`, `DEVLOG.md` ni de los `*SceneBuilder*.cs`.
- **Hilos cortos, uno por tarea.** Un hilo largo arrastra todo su historial en cada mensaje: cuando una tarea termina, se cierra y la siguiente va en un hilo nuevo.
- Sin subagentes ni workflows salvo que Berón lo pida (multiplican el gasto).
- Salidas de comandos acotadas (`| head`, `| tail`, `--oneline`); no volcar logs enteros de Unity.
- Lo que una sesión futura necesite saber va a `docs/` o a la memoria del proyecto, no se redescubre.
