# Checklist de pruebas — features recientes (para el compi de Unity)

> Para **montar escenas y verificar** lo construido 2026-09/2026-10 (PRs #203–#212). El repo solo versiona `.cs`+docs,
> así que primero hay que **regenerar escenas/prefabs** en el editor. Marca lo que pase. Guía de montaje general:
> [`unity-editor-manual.md`](unity-editor-manual.md). Diseño de cada sistema en los docs enlazados.

## 0. Montaje base (una vez)
- [ ] **COMPILAR PRIMERO** (el proyecto Unity es otra carpeta, `C:/Users/Blein/COLD-SANCTUARY`): `tools/sync-to-unity.sh`
      (`--prune` si hubo borrados/renombres) y luego **`tools/verify-compile.sh`** → compila de verdad sin abrir el
      Editor. **Hazlo tras cada PR mío**: yo no puedo compilar en mi máquina (solo verifico llaves/refs), así que este
      paso es el que caza errores reales de compilación. Repórtame lo que salga y lo arreglo.
- [ ] Dentro del Editor: `Tools/Cold Sanctuary/Verify` (`HeadlessVerify.Run`) monta el blockout → `[VERIFY] PASS|FAIL`.
- [ ] Paquetes instalados: **URP** y **AI Navigation** (si no, los builders no compilan / no hay NavMesh).
- [ ] Menú **`Tools/Cold Sanctuary/Build Sample Scene Blockout`** → genera el mesocosmos + regenera escenas del Microcosmos.
- [ ] Guardar la escena. Debe existir un GameObject **`Player`** con tag `Player`.
- [ ] Objetos esperados en escena (los crea el builder): `Clock_AUTO`, `TraceField_AUTO`, HUDs
      `AnimaStatusHUD_AUTO`/`AnimaThoughtHUD_AUTO`/`AnimaNameplate_AUTO`, `SpellConsole_AUTO`, `Restaurante_AUTO`.
- [ ] **Prefabs de fauna**: `Tools/Cold Sanctuary/Audit Animal Prefabs` → ver qué falta; poner los `.fbx` en
      `Assets/Animals/{Especie}/Models/` (rig **Generic**, clips `Idle/Walk/Run`) y `Generate Animal Prefabs`.
      Pendientes: Termite/DungBeetle/Antlion/Mantis/Grasshopper/Meerkat. **Sin modelo ya NO crashea** (ActionPrep
      tiene guard): se mueven sin animar.

## 1. HUDs (OnGUI, salen solos al dar Play)
- [ ] **Estado** (`AnimaStatusHUD`): drives/actividad/elementos con color del ser objetivo.
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

## 13. Insectos neutros de ambiente (Microcosmos Scene0)
- [ ] Regenera `Build Microcosmos Scene0`; si los `.fbx` de Cricket/Aphid/Ladybug/Grasshopper existen, verás unos
      pocos dispersos por la zona norte (ambiente). Si faltan, la consola loguea que se omiten (no crashea).

## Qué reportar
Consola de Unity: busca `[Cocina]`, `[Restaurante]`, `[Yoga]`, `[Hechizo]`, `[Charla]`, `[Mente]`. Anota crashes,
NPEs, o comportamientos raros (jitter de movimiento, escalas deformadas, colas atascadas) con el ser/escena.
