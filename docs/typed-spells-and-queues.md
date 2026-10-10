# Hechizos por teclado + colas de acción (jugabilidad tipo Sims)

> Diseño + estado (2026-10-10). Consecuencia del principio **"toda acción es un hechizo"**: el jugador (y los
> NPC) **lanzan hechizos**; el jugador puede hacerlo **pulsando botones O escribiéndolos**. Enlaza con
> [`spell-genesis-and-sanctuary-progression.md`](spell-genesis-and-sanctuary-progression.md) (génesis de hechizos),
> [`anima-architecture.md`](anima-architecture.md) §11.5 (posesión/control), [`anyma-factory-and-control.md`](anyma-factory-and-control.md)
> (el jugador NUNCA tiene control total).

---

## 1. Dos formas, un principio

Caminar/saltar/trepar/agacharse/tomar son **hechizos**. El jugador los lanza de dos maneras:
1. **Botones** (como hoy).
2. **Escribiéndolos** (teclado físico o **teclado virtual**) — un **lanzador de funciones**.

Ejemplo (poseyendo a Kushal): escribir `walkTo({destiny:"Marilia", maxSpeed:true})` → si Kushal **alcanza a
ver** a Marilia, va hacia ella; si no, piensa **"¿Dónde está Marilia?"**. El objeto pasado al hechizo tiene
**propiedades opcionales** (igual que el spec de la factory): `distance` (acercarse manteniendo distancia),
`maxSpeed`, etc. `destiny` (en vez de `character`) permite que el destino sea una **anima** *o* un **área/
gameobject** por su `name` (el restaurante = toda el área del restaurante, el santuario = el área…). Como todo
GameObject de Unity tiene `name`, el destino se resuelve **nativamente por nombre**.

## 2. Etiquetas de nombre (CONSTRUIDO) — `AnimaNameplate`
Sobre cada anima a la vista se muestra su **nombre** → el jugador, parado en el bosque, ve a quién le rodea y
puede dirigir al poseído hacia "un venado", "un oso" o "Marilia". (Hoy OnGUI mundo→pantalla; world-space
`TextMesh` por anima es el siguiente paso.) También se etiquetarán **muebles/ingredientes/utensilios** para las
misiones de cocina (ir a la nevera, tomar los huevos, tomar el plato, batir…).

## 3. La consola de hechizos (CONSTRUIDO, 1ª rebanada) — `SpellConsole`
Tecla `~` abre un campo de texto. Parsea `cmd({clave:valor, …})` (tolerante: `cmd("Nombre")`, `cmd(destiny:Nombre)`,
con/sin llaves/comillas). Resuelve el **destino** y **encola** el hechizo en la **cola del jugador** del poseído.
Hoy implementa **`walkTo`** (destino anima-visible o área por nombre; `maxSpeed`, `distance`; si no lo ve →
"¿Dónde está X?"). Es un **registro de funciones**: se amplía con `take`, `putIn`, `beat`, posturas de yoga, etc.
> **Decisión abierta ⚠️:** ¿permitir **input de código** directo, o mantener una **lista de hechizos + ejecutor**
> (nombre→método real)? La 2ª es más segura y es la montada (un `switch`/registro); la 1ª requeriría un intérprete.

## 4. Colas de acción (CONSTRUIDO, base) — `ActionQueue`
Estilo Sims: el anima sigue una **cola** de hechizos en orden. **Dos carriles**:
- **`own`** — la cola **PROPIA** del anima: **prioritaria y NO editable** por el jugador. La alimenta su
  instinto / <see cref="AcuteStressResponse"/> cuando el cuerpo toma el mando. **Siempre tiene prioridad** → por
  eso el jugador **nunca** tiene control total (programa sobre lo que el cuerpo permite).
- **`player`** — la cola del jugador al poseer: **editable** (encolar / quitar concreto / reordenar / vaciar).
Un anima **libre** (no poseída), si sus necesidades están cubiertas, encola acciones **random** (merodear/
explorar) en un carril **borrable**; al poseerla, el jugador las **vacía** y programa las suyas.

### 4.1 Colas en PARALELO (yoga, posturas)
Se pueden tener **varias colas** a la vez para que ejecuten en paralelo — p. ej. una **por parte del cuerpo**:
`espaldaRecta`, `hombrosAbajo`, `pechoAbierto` → el anima adopta la **postura** enviando señales a sus
`bodyParts` (`CreatureRig`). Si **no tiene** esa parte, la señal no hace nada y el anima responde **"no tengo
pierna disponible"**. Así se desbloquean las **misiones de yoga**: el jugador compone la postura escribiendo un
hechizo por cada parte. *(Diseño; la base `CreatureRig`/bodyParts existe; faltan los hechizos-postura por parte.)*

### 4.2 Velocidad de lanzamiento de los NPC (cast time)
Para las demás animas, lanzar un hechizo **toma tiempo**, análogo a teclear: se usa la **velocidad media real de
mecanografía ≈ 40 palabras/minuto** (≈ **0,67 palabras/s**, ~**1,5 s por palabra**) → `castTime ≈ nº_palabras /
0,67`. (El autor estimó "1 s por 4 palabras" ≈ 240 ppm, que es ritmo de mecanógrafo profesional; se usa el
**promedio real ~40 ppm**.) *(Diseño; hoy la cola ejecuta sin coste de tiempo de "tecleo".)*

## 5. Estado (qué hay / qué falta)
| Pieza | Estado |
|---|---|
| Etiquetas de nombre (`AnimaNameplate`) | **EXISTE** (OnGUI) |
| Consola de hechizos por teclado (`SpellConsole`) + `walkTo` + destino por nombre/área + "¿Dónde está X?" | **EXISTE** (1ª rebanada) |
| Cola de acciones 2-carriles (`ActionQueue`: own prioritaria + player editable) | **EXISTE** (base) |
| `QueuedSpell` + `WalkToSpell` | **EXISTE** |
| Comandos de consola: `walkTo`, **`step`/`cook`** (avanza la receta), **`bond`**, **`thought`** (lanzan `BondSpell`/`ThoughtSpell`) | **EXISTE** |
| **Restaurante físico con primitivas** (suelo + nevera/planchas/ollas/bols/mesa + ingredientes, todos con etiqueta; muchas estaciones para varios cocineros) | **EXISTE** (`BuildRestaurantRoom`) |
| **Cocinar por teclado**: `walkTo({destiny:"Nevera"})` → `step` (estaciones y ingredientes etiquetados) | **EXISTE** (base) |
| Más hechizos encolables específicos por paso (take/putIn/beat con estación exacta) | **FALTA** |
| Hechizos-postura por bodyPart (`PostureSpell`) + asanas (`AsanaPoseSpell`) + **Saludo al Sol** (`SunSalutation`, 12 asanas) + consola `yoga`/`surya`; parte ausente → "no tengo X disponible" | **EXISTE (2026-10-10)** |
| `own`-lane alimentada por `AcuteStressResponse` (`BodyReflexSpell` bloquea al jugador en toma de mando) | **EXISTE (2026-10-10)** |
| **Carril opcional** del anima libre (`IdleBehavior` encola `WanderSpell`; auto en animas compuestas via `Cast`) | **EXISTE (2026-10-10)** |
| `bond`/`thought` resuelven al objetivo por nombre EN CUALQUIER PARTE (crear amistad remota) + `amount` | **EXISTE (2026-10-10)** |
| Enrutar TODA la IA de la fauna (`Volition`) por la cola (NPCs "escriben" cada acción) | **FALTA** (refactor mayor) |
| Cast time NPC (~40 ppm) + ver las colas del poseído en UI | **FALTA** |
| Nameplates/estaciones world-space + teclado virtual | **FALTA** (necesita prefabs Unity) |

## 6. Orden sugerido
1. Hechizos encolables de **cocina** (`take`, `putIn`, `beat`, `emplata`) + etiquetar muebles/ingredientes →
   resolver la misión del restaurante **escribiendo** (conecta con `RestaurantKitchen`).
2. **Hechizos-postura por bodyPart** + colas en paralelo → **misiones de yoga**.
3. `AcuteStressResponse` **encola en `own`** (el cuerpo interrumpe con su urgencia) + UI de colas del poseído.
4. Cast time NPC (~40 ppm) + teclado virtual.
