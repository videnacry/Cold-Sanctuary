# Checklist de pruebas — features recientes (para el compi de Unity)

> Para **montar escenas y verificar** lo construido 2026-09/2026-10 (PRs #203–#212). El repo solo versiona `.cs`+docs,
> así que primero hay que **regenerar escenas/prefabs** en el editor. Marca lo que pase. Guía de montaje general:
> [`unity-editor-manual.md`](unity-editor-manual.md). Diseño de cada sistema en los docs enlazados.

## 0. Montaje base (una vez)
- [ ] **COMPILAR PRIMERO** (el proyecto Unity es otra carpeta, `C:/Users/Blein/COLD-SANCTUARY`): `tools/sync-to-unity.sh`
      (`--prune` si hubo borrados/renombres) y luego **`tools/verify-compile.sh`** → compila de verdad sin abrir el
      Editor. **Hazlo tras cada PR mío**: yo no puedo compilar en mi máquina (solo verifico llaves/refs), así que este
      paso es el que caza errores reales de compilación. Repórtame lo que salga y lo arreglo.
> Verificado 2026-10-11 por CLI (`-executeMethod HeadlessLive.PrepareOnly`). Evidencia citada por línea.
- [x] Paquetes instalados: **URP** y **AI Navigation** — `Packages/manifest.json`:
      `"com.unity.render-pipelines.universal": "17.5.0"` y `"com.unity.ai.navigation": "2.0.13"`. Además el
      ensamblado de Editor (que usa `NavMeshSurface`) **compila con 0 errores**, así que los paquetes resuelven.
- [x] Menú **`Build Sample Scene Blockout`** → genera el mesocosmos + regenera escenas del Microcosmos.
      Log: `[LIVE] SampleSceneBuilder.Build() completado.` y las **3 escenas hermanas reescritas** en disco en la
      misma corrida (08:50): `SampleScene.unity` (1.075 KB), `Microcosmos_Scene1_Ambrosio.unity` (306 KB),
      `Microcosmos_Scene0_SakshiRio.unity` (239 KB), `MobWorld_Mesopotamia.unity` (62 KB).
      ⚠ Aviso del propio builder para la escena del Microcosmos: **«Bake NavMesh en Window>AI>Navigation antes
      de Play»** (el mesocosmos sí lo hornea solo con `BakeNavMesh()`).
- [x] Guardar la escena + `Player` con tag `Player` — log: `[LIVE] escena guardada.` y `[LIVE]   OK    tag 'Player'`.
- [ ] Objetos esperados en escena. **PARCIAL**: confirmados por log 4 de ellos —
      `[LIVE] OK TraceField_AUTO` / `Clock_AUTO` / `TestRunner_AUTO` / `EcoObservation_AUTO`, con
      `LevelDressing_AUTO con 714 descendientes`. **SIN verificar** los HUDs
      `AnimaStatusHUD_AUTO`/`AnimaThoughtHUD_AUTO`/`AnimaNameplate_AUTO`, `SpellConsole_AUTO` y
      `Restaurante_AUTO` (el inventario de `HeadlessLive` no los consulta todavía).
- [x] **Prefabs de fauna** — auditado 2026-10-11 cotejando el registro `Species` contra el disco: las **21 clases
      de comportamiento existen**, pero solo **8 especies tienen `.fbx` + `.prefab`** (los mamíferos
      Wolf/Deer/Fox/Malamute/Bunny/PolarBear/Seal/Whale). **Faltan 13**: Penguin, Orca, Ant, Aphid, Ladybug,
      Spider, Cricket, Termite, DungBeetle, Antlion, Mantis, Grasshopper, Meerkat. Confirmado **en vivo** que el
      builder los omite: `[SampleSceneBuilder] WildlifePopulation: no se encontro el prefab de Penguin, se omite
      esa familia.` (ídem Orca) → **el Pingüino y la Orca del Santuario de Hielo no entran en escena**. Es
      autoría de modelos, no código.
- [ ] **Prefabs de fauna**: `Tools/Cold Sanctuary/Generate Animal Prefabs` → ahora crea un **prefab-CUBO** para las
      especies **sin `.fbx`** (independiente de modelos 3D), así que **las 21 quedan con prefab**. Pon `.fbx` (rig
      **Generic**, clips `Idle/Walk/Run`) en `Assets/Animals/{Especie}/Models/` cuando los tengas y regenera esa
      especie (borra su `.prefab`-cubo antes, o se omite por existir). Sin modelo tampoco crashea en runtime (guard en ActionPrep).

## 1. HUDs (OnGUI)
- [ ] **Estado** (`AnimaStatusHUD`): **ON-DEMAND por hechizo** (ya no está siempre visible). Posee a un ser y escribe
      `status({destiny:"self"})` en la consola → aparece su HUD de estado unos segundos. `status({destiny:"Restaurante_AUTO"})`
      → vuelca los **stocks** del área por consola. (El override de inspector `target` lo fuerza visible para depurar.)
- [ ] **Abrir/cerrar áreas por hechizo**: `open({destiny:"Restaurante_AUTO"})` / `close({destiny:"Restaurante_AUTO"})` /
      `open({destiny:"Restaurante_AUTO", reset:true})` (volver al reloj). Con cerrado, los cocineros paran.
- [ ] **Pensamiento** (`AnimaThoughtHUD`): muestra el pensamiento del ser que manejas (posee uno) con intensidad;
      persiste ~10 min; verde=positivo/rojo=negativo. Verifica que cambie al **poseer** otro cuerpo.
- [ ] **Nombres** (`AnimaNameplate`): nombres sobre las animas a la vista **y** sobre muebles/ingredientes del
      restaurante (`WorldLabel`, en azul).

## 2. Campo de pensamientos ← entorno/necesidades (`Mind.SenseSurroundings`)
- [ ] Pon una amiga cerca (bond alto) → los pensamientos del ser tienden a **positivos**; pon un depredador no
      vinculado → tienden a **negativos/alerta**. Con hambre/sueño altos → malestar. (Observa el HUD de pensamiento.)

## 3. Control por necesidad / adrenalina / niebla (`AcuteStressResponse`, SIEMPRE activo)
- [ ] Sube el `stress`/`hungry` de un ser (inspector) por encima de ~0.85 → **el cuerpo toma el mando**: su
      `AiBrain.selfRelevance` sube; `Anima.physicalBoost`>0 (drena glucosa); `Anima.mindStatusCalculationFog`>0.
- [ ] Posee a ese ser y comprueba que, en apremio alto, **no puedes dirigirlo** (la cola `own` con `BodyReflexSpell`
      bloquea tus órdenes) hasta que baje el apremio.

## 4. Hechizos por teclado + colas (tecla `~` abre la consola) — [`typed-spells-and-queues.md`](typed-spells-and-queues.md)
- [ ] Posee a Kushal/otro. Abre consola (`~`). Escribe `walkTo({destiny:"Marilia"})` → si lo ve, va; si no →
      piensa **"¿Dónde está Marilia?"** (en el HUD de pensamiento).
- [ ] `walkTo({destiny:"Nevera"})` → camina a la nevera del restaurante (etiquetada). Prueba `maxSpeed:true` y `distance:2`.
- [ ] `bond({destiny:"<nombre>", amount:30})` → sube el bond hacia ese ser **aunque no lo vea** (crea amistad remota).
- [ ] `thought({destiny:"<nombre>", positive:true})` → cambia su ánimo (mira su HUD de pensamiento).
- [ ] `yoga` → el ser encola el **Saludo al Sol** (12 asanas); si le falta una parte, loguea "no tengo X disponible".

## 5. Cocina del restaurante (jugable) — [`spell-genesis-and-sanctuary-progression.md`](spell-genesis-and-sanctuary-progression.md) §4
- [ ] En `Restaurante_AUTO` hay 6 `Stock_*`, 2 NPC cocineros y una `CookingStation` en el Player, + la **Sala** física
      (nevera/planchas/ollas/bols/mesa + ingredientes, con etiqueta).
- [ ] Da Play: los NPC cocinan solos y llenan el stock; tú con `E` (o consola `step`) avanzas pasos.
- [ ] Al llenar **todo** el stock → log `MISIÓN CUMPLIDA` + **XP** (marga Stats) + **una porción de cada platillo** al
      `Inventory` (asigna `ItemData` por platillo si quieres el item; si no, solo stock).
- [ ] **Cierre nocturno** (`AreaHours`): adelanta el reloj a la noche (Clock startHour ~20, o deja correr) → los
      cocineros **paran** (el restaurante cierra). De día reanudan.
- [ ] Platillos: hay 22 recetas (6 base + 16 "bomba" por país) en `CookingRecipe`; el sandbox usa 6, el resto son
      recetas por nombre.

## 6. Factory de animas por spec (`AnymaFactory` + `AnymaSpec`) — [`anyma-factory-and-control.md`](anyma-factory-and-control.md)
- [ ] Crea un GameObject con `AnymaFactory`; rellena `templates` (especie→prefab) con los prefabs generados.
- [ ] Desde un script/botón de prueba: `factory.Create(new AnymaSpec(), pos)` (spec vacío) varias veces → deben salir
      con **stats variados** (×0.8..2) y tamaños/grosores distintos; hambre/sueño random.
- [ ] `Create(new AnymaSpec{ species="Bear", randomStatsLimitations=new StatRange{min=1.5f,max=3f} })` → osos grandes.
- [ ] **Independencia de modelos:** deja un `templates[]` **con `prefab` vacío** (solo la especie) → al crear sale un
      **CUBO** funcional (Animal + SpeciesBody de esa especie). El juego no requiere los `.fbx`.

## 7. Apariencia por stats (`StatPhysique`, auto en fauna)
- [ ] Sube `strength` o `fatReserves` de un animal (inspector o `TransformationSpell`) → se ve **más grueso** (ancho),
      sin cambiar su altura ni deformar la proporción de la especie.

## 8. Hechizos de modificación (control = alinear)
- [ ] `TransformationSpell` (stats, ya existía), **`BondSpell`** (vínculo; `weaken` resta), **`ThoughtSpell`** (ánimo).
      Verifica vía consola `bond`/`thought` (punto 4) o añadiendo el componente y llamando `Cast`.

## 9. Necesidad de defecar (`DefecationNeed`, opt-in)
- [ ] Añádelo a un animal; con el tiempo `urge` sube (más rápido si es herbívoro `eatsGrass`); al máximo **defeca**
      (log + suelta `fecesPrefab` si lo asignas + baja urge). **Se reprime** si está en peligro (`aware`/`alertness`)
      o dormido, y según disciplina/compostura.

## 10. Comportamiento libre por cola (`IdleBehavior`, en animas compuestas del Microcosmos)
- [ ] En las escenas del Microcosmos, las animas del elenco **libres** (sin poseer, sin apremio) **merodean**
      (encolan `WanderSpell`). Al poseer una, puedes vaciar su cola y programar la tuya.

## 11. La Magnate (`BuildMagnate` → cápsula "Magnate")
- [ ] Da Play: tiene stats al tope, **no le bajan** hambre/sueño/estrés. No puedes poseerla (relevancia altísima).
- [ ] Pon un depredador atacando a una presa cerca de ella → al entrar en su radio, la presa/depredador **dejan de
      estar `aware`** (se calma; el lobo suelta al conejo).
- [ ] Deja correr: cada ~30 s (de juego) **va hacia el ser con menor nivel integral** y, al llegar, loguea
      `[Magnate] enseña el Saludo al Sol a «X»` → ese ser **aprende "yoga"** (Grimoire), **encola el Saludo al Sol** y
      sube XP de Yoga. (Si le falta comer/descansar, lo dice y no enseña.)

## 12. Cocina con RANGO (acción = hechizo con alcance)
- [ ] Posee al cocinero, abre consola (`~`). Lejos de la nevera, escribe `use({destiny:"Nevera"})` → **falla**:
      "no alcanzo Nevera — acércate". Haz `walkTo({destiny:"Nevera"})`, y cuando llegues, `use({destiny:"Nevera"})`
      → **avanza el paso**. (Primer paso del modelo "steps = hechizos con alcance"; la magia ampliará el rango.)

## 12b. Hechizos de cocina por PASO (`take`/`putIn`/`beat`)
- [ ] Posee al cocinero (receta *Huevos revueltos*), consola `~`. Encola de corrido:
      `walkTo("Nevera")` → `take("Nevera")` (solo la abre: paso «ve a la nevera») → `walkTo("Huevos")` → `take("Huevos")` →
      `walkTo("Plancha")` → `step` (calienta) → `putIn("Plancha")` (la receta no tiene paso "pon": avisa sin avanzar) →
      `beat("Plancha")` (revuelve; tarda ~2,5 s ÷ fuerza/agilidad).
      Cada hechizo que **casa con el verbo** del paso actual lo **avanza** (HUD ✔).
- [ ] Fallos esperados (log `[Cocina]` + pensamiento): `take` lejos → "no alcanzo"; `take` con las manos llenas;
      `putIn("Huevos")` → "no es un recipiente"; `beat` en olla vacía → "no hay nada que batir"; hechizo que no casa →
      "hecho, pero el paso que toca es «…»". `"Plancha"` resuelve la «Plancha N» más cercana.
- [ ] Con `MagicReserves` desbloqueada y energía, `take` a distancia **funciona** pagando 50 J/m extra.

## 13. Insectos neutros de ambiente (Microcosmos Scene0)
- [ ] Regenera `Build Microcosmos Scene0`; si los `.fbx` de Cricket/Aphid/Ladybug/Grasshopper existen, verás unos
      pocos dispersos por la zona norte (ambiente). Si faltan, la consola loguea que se omiten (no crashea).

## 14. Volición por la cola (NPCs "lanzan" sus acciones)
- [ ] Un animal **no poseído** con necesidades: su `Volition` ahora **encola un `DesireSpell`** (el deseo elegido:
      comer/seguir/descansar…) en su `ActionQueue` en vez de actuar directo. Comportamiento ≈ igual que antes
      (el `Dispatch` arranca la conducta), pero ahora pasa por la cola. Si subes su apremio corporal (`AcuteStressResponse`
      en toma de mando), el carril `own` **pausa** la volición. (`Volition.useQueue`=false vuelve al despacho directo.)

## Qué reportar
Consola de Unity: busca `[Cocina]`, `[Restaurante]`, `[Yoga]`, `[Hechizo]`, `[Charla]`, `[Mente]`. Anota crashes,
NPEs, o comportamientos raros (jitter de movimiento, escalas deformadas, colas atascadas) con el ser/escena.
