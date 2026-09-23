# Manual de Unity — qué recrear a mano (por si se pierde el proyecto Unity)

> **Por qué existe este doc:** el repo **solo versiona `.cs` + docs**. NO están en git: escenas
> (`.unity`), prefabs, modelos `.fbx`, materiales, `.meta`, `ProjectSettings/`, `Packages/manifest.json`.
> Si el proyecto Unity se pierde, **este manual + el código bastan para reconstruirlo**. Verificado
> contra el código (2026-09-21). Fuentes: `Assets/Editor/AnimalPrefabGenerator.cs`,
> `AnimalModelImporter.cs`, `Assets/Animals/*`, `Assets/Editor/*SceneBuilder.cs`.

---

## 0. Paquetes y render pipeline (requisitos de proyecto)

- **URP** (Universal Render Pipeline): los builders crean materiales con
  `Shader.Find("Universal Render Pipeline/Lit")` con fallback a `"Standard"`. Instalar URP (o aceptar
  el fallback a Standard, más feo).
- **AI Navigation** (`com.unity.ai.navigation`): los builders usan `NavMeshSurface` para hornear el
  NavMesh. **Sin este paquete no compilan los builders ni hay navegación.**
- `Packages/manifest.json` NO está versionado → hay que reinstalar estos paquetes a mano.

---

## 1. Modelos 3D (`.fbx`) — qué necesita cada animal

Ubicación: **`Assets/Animals/{Especie}/Models/*.fbx`** (una carpeta `Models` por especie).
El importador `AnimalModelImporter.cs` se ejecuta **solo al importar** y fija:
- **Rig = Generic** (no Humanoid) para todo lo que esté bajo `Assets/Animals/` (Humanoid solo si la
  ruta contiene "Player").
- Read/Write ON, escala global 1, UVs de lightmap, `importAnimation = true`, materiales **External**
  (se generan como assets aparte al lado del modelo), `bakeAxisConversion = true` con movimiento de
  raíz. → **el FBX necesita: rig Generic, un hueso RAÍZ, y clips de animación embebidos.**
- Requiere **al menos un MeshRenderer/SkinnedMeshRenderer** (si no, el collider no se puede ajustar).

### Animaciones requeridas (recomendado; ya no es crítico)
El sistema **no usa parámetros/estados del AnimatorController**: reproduce clips **por NOMBRE** con
`Animator.Play("<estado>")` + ajusta `Animator.speed`. Lo ideal es que cada prefab lleve un `Animator`
con un controller que contenga los estados con el nombre exacto.

> **Fragilidad RESUELTA (2026-09-23):** `ActionPrep.Prep` ahora **comprueba** que exista `Animator` +
> `runtimeAnimatorController` + el estado (`HasState`) antes de `ani.Play` (y guarda `nav`) → si falta el
> modelo/controller o el estado, **NO revienta**: se salta la animación y sigue con nav/exhaustion. Así
> puedes probar una especie **sin modelo/animaciones** (se moverá sin animar). Poner el `.fbx` +
> AnimatorController con `Idle/Walk/Run` sigue siendo lo deseable para que se vea bien.

Nombre del estado = `gait + Especie`. Catálogo en `ActionsPrep.cs`:
- **Mamíferos/fauna de hielo con entrada propia** (Bear/Deer/Fox/Malamute/Seal/Whale/Wolf/Penguin/Orca):
  `Idle{Especie}`, `Walk{Especie}`, `Run{Especie}` (Bunny reutiliza `RunBunny` para andar y correr).
- **TODOS los insectos** (Ant/Aphid/Ladybug/Spider/Cricket/**Termite/DungBeetle/Antlion/Mantis/
  Grasshopper/Meerkat**) **no tienen entrada** en `ActionsPrep` → caen al **default** → necesitan clips
  llamados literalmente **`Idle`, `Walk`, `Run`**.
- No hay clips de atacar/dormir/morir cableados (esos estados son por stats, no animados).

**Pasos:** autor el FBX (rig Generic, hueso raíz, clips `Idle`/`Walk`/`Run`) → arrástralo a
`Assets/Animals/{Especie}/Models/` → crea un **AnimatorController** con esos estados → asígnalo al
Animator del FBX/prefab.

### Rig lógico para partes del cuerpo (`CreatureRig`) — solo si hace falta
Para yoga/emoción/posesión que apunten a partes concretas (antenas/patas/alas), añade `CreatureRig` y
**rellena a mano la lista `bindings`** (`BodyPart`→hueso) en el inspector: los rigs Generic (insectos/
quimeras) **no** se auto-mapean (eso solo ocurre con Humanoid). Para **locomoción básica NO hace falta**
(solo los estados `Idle/Walk/Run`).

---

## 2. Prefabs — generación automática

Todo en `Assets/Editor/AnimalPrefabGenerator.cs`, menús bajo **`Tools/Cold Sanctuary/`**:
- **`Audit Animal Prefabs (modelos 3D)`** — reporta por especie: cuántas tienen prefab, cuántas tienen
  modelo pero no prefab, cuántas no tienen modelo. **Empieza por aquí.**
- **`Generate Animal Prefabs`** — por cada especie del registro `Species`: busca el `.fbx` en
  `{Especie}/Models/`, crea **`Assets/Animals/{Especie}/{Especie}.prefab`** con estos componentes:
  **Rigidbody + BoxCollider + Animator + NavMeshAgent + PostNatalManager + `{Especie}Behavior`**.
  **Si el prefab ya existe, lo SALTA** (no pisa ajustes manuales). Si falta el FBX o la clase, avisa y salta.
- **`Fix Animal Colliders And Rigidbodies`** — re-aplica escala + física + orientación a los prefabs ya
  existentes (útil tras cambiar un modelo).

**Física que fija el generador** (`ApplyPhysicsDefaults`): `Rigidbody.isKinematic = true`,
`useGravity = false` (un rigidbody con gravedad pelea con el NavMeshAgent y el animal atraviesa el
suelo; la masa la maneja `LifeStage` en runtime). **BoxCollider** ajustado a los bounds de los
renderers. Gira los hijos 180° en Y (los FBX miran a −Z; la raíz no se gira porque el NavMeshAgent usa
+Z como frente).

**Registro de especies** (`Species`, tuplas `(carpeta, clase)`): ya incluye las 6 nuevas
(Termite/DungBeetle/Antlion/Mantis/Grasshopper/Meerkat) + insectos + mamíferos. Para una especie nueva:
añade su tupla aquí (carpeta `PolarBear` → clase `BearBehaviour` es el ejemplo de carpeta≠clase).

---

## 3. Escala

Dos fuentes; se usan distinto:
- **`RealisticScaleFactor`** (dict en `AnimalPrefabGenerator`): SOLO 8 mamíferos
  (Wolf/Deer/Fox/Malamute/Bunny/Whale/PolarBear/Seal). Si la especie no está, el generador **no toca la
  escala** del prefab. Cuando está, escribe el `localScale` del prefab **y** el `Physiognomy.baseScale`
  serializado.
- **`Physiognomy.baseScale`** (catálogo `Physiognomy.cs`, en código): en runtime `LifeStage` escala el
  cuerpo y calcula la masa desde aquí.

**Insectos** (incl. los 6 nuevos): dependen de **`Physiognomy.baseScale`** (ya tienen entrada, tamaño
insecto — p. ej. Meerkat `(0.006, 0.008, 0.010)`, 0.004 kg = "suricata a escala insecto"). **NO** añadir
al `RealisticScaleFactor`. En el editor el prefab se ve a escala de import; **al pulsar Play** toma su
tamaño real.

---

## 4. Escenas — cómo se construyen y qué necesitan

### Mesocosmos (Santuario 1)
Menú **`Tools/Cold Sanctuary/Build Sample Scene Blockout`** (`SampleSceneBuilder.cs`). Construye
GameObjects **en la escena abierta** (no crea ni guarda `.unity`, no se añade a Build Settings) →
**abre una escena y GUÁRDALA a mano**. Objetos que necesita la escena:
- Suelo `Ground_Placeholder` con `MeshCollider` + `NavMeshSurface` → **hornear NavMesh** (sin NavMesh
  los NavMeshAgent fallan en silencio). El builder hornea con `surface.BuildNavMesh()`.
- Singletons que crea el builder: `Clock_AUTO` (`Clock`), `TraceField_AUTO` (`TraceField` — **sin él el
  sistema de rastros/feromonas es no-op**). Debe existir un **`TimeController`** (lo usa `ActionPrep`).
- Un GameObject **`Player`** con el **tag `"Player"`** (lo buscan `MobWorldLoader`, combate, economía;
  sin él se saltan esos sistemas).

**Poblar fauna** — `FamilyGenerator` (familias) y `Generator` (individuos sueltos). **Los prefabs se
cargan por RUTA** (`Assets/Animals/{Especie}/{Especie}.prefab`); si falta, esa familia se salta con
warning. **La zona se ve VACÍA en el editor: los animales aparecen al pulsar Play** (los spawnea
`Start()`).
- `FamilyGenerator.families[]` — campos por entrada: `animalPrefab` (prefab con componente `Animal`),
  `minParentsCount` (0 = default de especie), `quantity` (0 = tamaño de familia de la especie),
  `radius` (0 = quantity×2), `position`, `renderHeight`.
- `Generator` — `subjects[]` (`{gameObject, quantity}`) + `area` (un GameObject con `Collider`);
  dispersa individuos por el área al Play. **Hoy sin invocadores en código: solo se cablea en escena.**
- Nota: los insectos **no** se ponen en el mesocosmos (van en el Microcosmos).

### Microcosmos (escenas propias)
`MicrocosmosSceneBuilder.cs` **sí crea/guarda `.unity` y los añade a Build Settings**:
- **`Build Microcosmos Scene0 (Sakshi / el rio)`** → `Assets/Scenes/Microcosmos_Scene0_SakshiRio.unity`.
- **`Build Microcosmos Scene1 (Ambrosio)`** → `Assets/Scenes/Microcosmos_Scene1_Ambrosio.unity`.
- `MobWorldSceneBuilder.cs` → `MobWorld_Mesopotamia.unity`.
- Cada escena: luz direccional `Sun`, un `MobSpawnPoint`, un `WorldExitPortal` (cubo trigger), NavMesh
  horneado sobre `ForestFloor`, guardado + `AddToBuildSettings`.

**Carga en runtime:** por **nombre de escena, additive** (`MobWorldLoader.EnterMobWorld(nombre)` →
`LoadSceneAsync(..., Additive)`). **La escena debe estar en Build Settings** o no carga. Al jugador se le
teletransporta al `MobSpawnPoint`; el `WorldExitPortal` llama a `ExitMobWorld()`.

---

## 5. Materiales / tags / capas

- **Tag `"Player"`** en el GameObject del jugador (obligatorio para mob-world/combate/economía).
- **Capa de mobs**: `PlayerCombat`/`NPCCombatBehavior` exponen un `LayerMask mobLayer` → crea una capa
  para los mobs y asígnala (solo si usas combate).
- **Materiales**: los animales importan materiales **External** (assets aparte al lado del `.fbx`). Los
  builders generan materiales URP/Standard en runtime para el blockout.
- **LOD**: es un `Lod.cs` propio (por distancia, decide cuánto "piensan" los lejanos), **no** `LODGroup`
  de Unity; no requiere setup en el prefab.

---

## 6. Checklist rápido para una especie nueva

1. FBX (rig **Generic**, hueso raíz, clips **`Idle`/`Walk`/`Run`** para insectos) → `Assets/Animals/{Especie}/Models/`.
2. AnimatorController con esos estados → asignar al Animator.
3. `Tools/Cold Sanctuary/Generate Animal Prefabs` → crea el `.prefab` con todos los componentes.
4. Escala: ya sale del catálogo `Physiognomy` (insectos) al Play; no tocar `RealisticScaleFactor`.
5. (Opcional) `CreatureRig` + `bindings` a mano para yoga/emoción.
6. Poblar escena: `FamilyGenerator.families[]` (familias) o `Generator.subjects[]` (individuos) apuntando
   al prefab; la escena necesita suelo+NavMesh horneado, `Clock`, `TraceField`, `TimeController` y un
   objeto con tag `Player`.
7. Microcosmos: añade el spawn en el builder de esa escena y regénérala (se auto-guarda + Build Settings).

---

## 7. Modelos 3D pendientes ahora mismo (fauna era del fuego)

Faltan los `.fbx` (y su AnimatorController con `Idle/Walk/Run`) para las 6 especies nuevas — clases,
catálogos y registro ya están en código:

| Especie | Carpeta del modelo | Referencia visual real |
|---|---|---|
| Termite | `Assets/Animals/Termite/Models/` | termita obrera/soldado (Hodotermes/Macrotermes) |
| DungBeetle | `Assets/Animals/DungBeetle/Models/` | escarabajo pelotero (Scarabaeus) + su bola |
| Antlion | `Assets/Animals/Antlion/Models/` | larva de hormiga león (mandíbulas grandes) |
| Mantis | `Assets/Animals/Mantis/Models/` | mantis religiosa |
| Grasshopper | `Assets/Animals/Grasshopper/Models/` | saltamontes/langosta |
| Meerkat | `Assets/Animals/Meerkat/Models/` | suricata (a escala insecto = skin de la máquina) |

Corre **`Audit Animal Prefabs`** para ver el estado, pon los FBX, y **`Generate Animal Prefabs`**.
