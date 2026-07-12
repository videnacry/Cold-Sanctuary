# Pipeline de modelos 3D — Cold Sanctuary

## Estructura de carpetas

```
Assets/Animals/
├── Bunny/
│   ├── Models/        ← FBX aquí; el importador se configura solo
│   ├── Animations/    ← AnimationClips extraídos o importados
│   ├── Materials/     ← materiales auto-generados al importar
│   └── Textures/      ← texturas del modelo
├── Bear/
│   ├── Models/
│   ├── Animations/
│   ├── Materials/
│   └── Textures/
├── Wolf/ (ídem)
└── _Shared/
    ├── Animations/    ← clips reutilizables entre especies (idle, walk, run genéricos)
    └── Materials/     ← materiales compartidos (tierra, pelaje base)
```

El script `Assets/Editor/AnimalModelImporter.cs` corre automáticamente cuando Unity detecta
un FBX nuevo en cualquier carpeta `Models/` — configura escala, animaciones, blend shapes
y UVs de lightmap sin intervención manual.

> **Estado real (auditoría 2026-07-09):** `AnimalModelImporter` fija siempre
> `animationType = Generic`; no existe la rama Humanoid/Player pese al comentario de
> cabecera del propio script. El siguiente paso del pipeline tras importar es
> `Assets/Editor/AnimalPrefabGenerator.cs`, con los comandos de menú `Generate Animal
> Prefabs`, `Fix Animal Colliders And Rigidbodies` y `Measure Raw Animal Sizes`.

---

## Especies árticas — lista actualizada

| Especie | Estado | Fuente | Formato |
|---|---|---|---|
| **Lobo** (Wolf) | ✅ Descargado | Quaternius Animal Pack | FBX |
| **Ciervo/Stag** | ✅ Descargado | Quaternius Animal Pack | FBX |
| **Zorro** (Fox) | ✅ Descargado | Quaternius Animal Pack | FBX |
| **Malamute** (mesh husky de stand-in) | ✅ Descargado | Quaternius Animal Pack | FBX |
| **Conejo** (Bunny) | ✅ Extraído | Sketchfab | FBX |
| **Oso Polar** | ✅ Convertido e integrado | Sketchfab (kenchoo, CC-BY-4.0) | GLB → FBX |
| **Foca** (Seal) | ✅ Convertida e integrada | Sketchfab (rkuhlf, CC-BY-4.0) | GLB → FBX |
| **Ballena** (Whale) | 🔽 Por descargar | Quaternius Fish Pack | FBX |

### 🌟 Alternativa recomendada — Quirky Series Arctic Animals Vol 1 (Omabuarts)

**URL Sketchfab:** https://sketchfab.com/3d-models/quirky-series-arctic-animals-vol-1-0db6031197ad4f288d691c14390d677a#download

Un solo pack con **9 animales árticos**, todos rigged con 19 animaciones y 29 blendshapes/shapekeys:
- Oso polar, Foca, Zorro ártico, Reno, Morsa, Búho nival, Armiño, Pingüino, Pájaro
- Descargar desde Sketchfab (requiere login) → formato GLB → convertir con Blender o aspose.app
- Versión FBX disponible en su web de pago: https://www.omabuarts.com

Si se compra la versión de pago ($89 el pack o individual más barato), se obtiene FBX directo sin conversión.

### Conversión GLB → FBX (Oso Polar ✅, Foca ✅)

Los archivos GLB están en tu carpeta de Descargas. Para convertirlos:
- **Online (sin instalar nada):** https://products.aspose.app/3d/conversion/glb-to-fbx
- **Con Blender:** File → Import → glTF 2.0 → luego File → Export → FBX
- Una vez convertidos, mover a `Assets/Animals/PolarBear/Models/` y `Assets/Animals/Seal/Models/`

**Oso Polar — hecho:** convertido con Blender en modo headless (`blender --background --python script.py`,
`bpy.ops.import_scene.gltf()` + `bpy.ops.export_scene.fbx()`, sin decimar — 10.352 vértices, 1 armature,
conversión limpia en <1s). Factor de escala calculado con el diagnóstico del proyecto (altura cruda
4.870m → objetivo 1.3m de hombro, factor 0.267 — ver `RealisticScaleFactor["PolarBear"]` en
`AnimalPrefabGenerator.cs`), longitud resultante ~2.0m consistente con un oso polar real.

**Foca — hecho:** mismo proceso exacto (headless, sin decimar — 1.265 vértices, 2 mallas, 1
armature, <1s). A diferencia de los cuadrúpedos parados, se referencia por longitud corporal (no
altura de hombro) como Whale: longitud cruda 10.329m → objetivo 1.7m, factor 0.165 — ver
`RealisticScaleFactor["Seal"]`. Ver `DEVLOG.md` "Foca (Seal) convertida e integrada" para el
detalle completo (dimensiones relativas al resto del roster, integración en
`SampleSceneBuilder`).

### Ballena — dónde descargar

Carpeta FBX del Fish Pack (CC0):
→ `https://drive.google.com/drive/folders/1L8ovz6ZM1btyW30ZZvf2cZe1GU5_Q_CG`
→ Click derecho en la carpeta FBX → **Descargar**
→ Extraer → mover `Whale.fbx` a `Assets/Animals/Whale/Models/`

---

## Assets de entorno — santuario ártico

### Outdoor / Naturaleza ártica

| Pack | Modelos | Fuente | Cómo descargar |
|---|---|---|---|
| Ultimate Stylized Nature (Quaternius) | 63 FBX | Google Drive | [Carpeta Drive](https://drive.google.com/drive/folders/1IV3bXHzkNvuNWFHPi4KPx-G4ghuxIuT-) → click derecho FBX → Descargar |
| Stylized Nature MegaKit (Quaternius) | 116 modelos | itch.io | [quaternius.itch.io/stylized-nature-megakit](https://quaternius.itch.io/stylized-nature-megakit) → Download Now → $0 |
| Low Poly Winter Pack (Broken Vector) | 12 FBX | itch.io | Buscar "Low Poly Winter Pack" en `itch.io/game-assets/free/tag-3d/tag-winter` |

### Edificios modulares (complejo del santuario)

| Pack | Modelos | Fuente | Cómo descargar |
|---|---|---|---|
| Downtown City MegaKit (Quaternius) | 315 piezas | itch.io | [quaternius.itch.io/downtown-city-megakit](https://quaternius.itch.io/downtown-city-megakit) → Download Now → $0 |

### Interiores (comedor, habitaciones, zona terapia)

| Pack | Modelos | Fuente | Cómo descargar |
|---|---|---|---|
| Sushi Restaurant Kit (Quaternius) | 108 FBX | Google Drive | [Carpeta Drive](https://drive.google.com/drive/folders/1srOaThdSqYBtmrqq9couZShM5r0cw_bH) → click derecho FBX → Descargar |

### Estructura de carpetas de entorno propuesta

```
Assets/Environment/
├── Nature/
│   ├── Trees/
│   ├── Rocks/
│   └── Plants/
├── Buildings/
│   ├── Exterior/      ← piezas modulares del complejo
│   └── Interior/      ← mesas, sillas, camas, cocina
├── Ocean/             ← plano de agua (Unity built-in Water o shader custom)
└── Terrain/           ← terreno ártico con nieve
```

---

## Fuentes de modelos — dónde descargar

### Opción 1 — Meshy.ai (IA generativa, recomendado para bocetos rápidos)
1. Ir a **https://www.meshy.ai** → Sign up gratis
2. Tab **Models** → buscar "rabbit" / "bunny" → filtrar "Free"
3. Descargar como **FBX** (no OBJ — el FBX conserva el rig)
4. Copiar el `.fbx` a `Assets/Animals/Bunny/Models/`
5. Unity re-importa automáticamente

Para modelos expresivos: en Meshy podés pedir "low-poly bunny with ears rig, tail bone,
facial blend shapes" y el modelo generado incluye los huesos si la descripción es específica.

### Opción 2 — Sketchfab (requiere cuenta gratuita)
- Conejo rigged: https://sketchfab.com/3d-models/rabbit-rigged-e7213589744d436b9d96e2dbb31198a5
- Demo conejo WildMesh: https://sketchfab.com/3d-models/rabbit-demo-free-download-54507527ccec44ca9620d4b82da73265
- Descargar como **FBX** → copiar a `Assets/Animals/Bunny/Models/`

### Opción 3 — Kenney.nl (sin cuenta, sin restricciones de licencia)
- Animal Pack Redux: https://kenney.nl/assets/animal-pack-redux
- Modelos low-poly, sin rig — buenos para placeholder visual mientras se trabaja el código

---

## Pipeline completo paso a paso

```
Descarga FBX
     ↓
Assets/Animals/[Especie]/Models/bunny.fbx
     ↓ (Unity auto-importa con AnimalModelImporter.cs)
     ↓
¿Tiene rig? ─── Sí ──→ Revisar huesos en Model Inspector
              └─ No ──→ Blender: Auto-Rig Pro o Rigify → re-exportar FBX
     ↓
¿Tiene blend shapes? ─── Sí ──→ usarlos para expresividad (orejas, ceño)
                       └─ No ──→ agregar en Blender con Shape Keys
     ↓
Crear Animator Controller en Assets/Animals/[Especie]/
     ↓
Conectar parámetros al script de comportamiento (BunnyBehavior.cs, etc.)
     ↓
Testear en escena con NavMeshAgent activo
```

---

## Requisitos del rig para lenguaje corporal (Cold Sanctuary)

Los animales se comunican mediante postura, orejas y movimiento de cola.
El rig mínimo recomendado por especie:

### Conejo
| Hueso | Uso |
|---|---|
| `Spine`, `Spine1`, `Spine2` | curvatura de espalda (susto → encogerse) |
| `EarL`, `EarL_Tip`, `EarR`, `EarR_Tip` | orientación y postura de orejas |
| `Tail`, `Tail_Mid`, `Tail_Tip` | movimiento de cola |
| `Head`, `Neck` | dirección de mirada |
| `HindLegL/R`, `ForeLegL/R` | postura de patas |

**Blend shapes recomendados:** `eyes_squint`, `nose_twitch`, `cheeks_puff`

### Oso
| Hueso | Uso |
|---|---|
| `Spine`–`Spine4` | postura encorvada vs erguida |
| `Head`, `Neck`, `Neck2` | orientación de cabeza |
| `Tail` | cola (vestigial pero visible) |
| `ShoulderL/R` | expansión de pecho (amenaza) |

**Blend shapes recomendados:** `mouth_open`, `brow_raise`, `brow_furrow`

---

## Blend shapes en Unity

Una vez importado el FBX con blend shapes:

```csharp
SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();

// Ejemplo: agitar orejas (índice del blend shape según el modelo)
smr.SetBlendShapeWeight(smr.sharedMesh.GetBlendShapeIndex("ear_perk"), 100f);

// Para interpolación suave:
float target = 100f;
float current = smr.GetBlendShapeWeight(earPerkIndex);
smr.SetBlendShapeWeight(earPerkIndex, Mathf.Lerp(current, target, Time.deltaTime * 5f));
```

---

## Notas sobre Meshy y animaciones expresivas

Meshy genera modelos estáticos en T-pose. Para el lenguaje corporal del juego:
1. Descargar el modelo base de Meshy (FBX)
2. Abrir en **Blender**
3. Usar **Auto-Rig Pro** (paid) o **Rigify** (gratuito) para generar rig de animal
4. Agregar huesos de orejas y cola manualmente
5. Crear **Shape Keys** (= blend shapes) para expresiones
6. Exportar FBX con "Bake Animation" + "Include: Armature + Mesh + Shape Keys"
7. Colocar en `Assets/Animals/[Especie]/Models/`

Para las animaciones de comportamiento (idle ansioso, agacharse, explorar):
- Se animan directamente en Blender y se exportan como clips separados
- O se usan herramientas como **DeepMotion** (IA de animación) para génerar animaciones
  de cuerpo completo a partir de video

---

## Decimar modelos pesados en Blender (5.1.2) — usar modo headless, no la GUI

La cocina (`Kitchen.fbx`, exportado desde 3ds Max) llegó con **8.98M polígonos**. Intentar
arreglarlo a mano en la GUI de Blender (Join + Decimate modifier sobre esa densidad) la dejó
congelada ("No responde") repetidamente durante más de una hora — cualquier click sobre el
objeto pesado (seleccionar, abrir un dropdown, entrar en Edit Mode) podía tardar 30–90s o más
en responder, y el "Add Modifier" ni siquiera llegaba a desplegar el menú.

**La solución fue no tocar la GUI en absoluto** y correr Blender en modo background con un
script Python:

```bash
"C:\Program Files\Blender Foundation\Blender 5.1\blender.exe" --background --python script.py
```

Flujo recomendado para cualquier modelo por encima de ~1M polígonos:
1. `bpy.ops.wm.read_factory_settings(use_empty=True)` — escena vacía, sin overhead de UI.
2. Importar el FBX/GLB de origen.
3. **Decimar objeto por objeto ANTES de hacer Join**, no la malla ya unida. Unir primero y
   decimar después revienta la memoria (ver bug #2 abajo) porque el modifier de Decimate
   necesita construir estructuras auxiliares sobre el total de vértices de una sola vez.
4. Recién ahí, `bpy.ops.object.join()` sobre los objetos ya livianos.
5. Guardar un **checkpoint `.blend`** antes de exportar. La exportación puede fallar o
   colgarse (ver bug #3) y así el reintento no repite el trabajo pesado de decimar.
6. Exportar.

### Bug #1 — el importador FBX crashea con luces: `CyclesLightSettings.cast_shadow`

Si el FBX trae un objeto de tipo Light (típico en exports de 3ds Max con iluminación de
escena), `bpy.ops.import_scene.fbx()` lanza:

```
AttributeError: 'CyclesLightSettings' object has no attribute 'cast_shadow'
```

Es un bug del propio addon `io_scene_fbx` de esta build de Blender — asume un atributo que no
existe en esta versión del engine Cycles. Workaround (parchear la clase en runtime *antes* de
importar):

```python
bpy.ops.preferences.addon_enable(module='cycles')
import cycles.properties as _cycles_props   # OJO: bpy.types.CyclesLightSettings NO es
                                             # la misma clase que esta — hay que parchear
                                             # cycles.properties.CyclesLightSettings,
                                             # si no, el parche no tiene efecto.
if not hasattr(_cycles_props.CyclesLightSettings, 'cast_shadow'):
    _cycles_props.CyclesLightSettings.cast_shadow = bpy.props.BoolProperty(
        name="Cast Shadow", default=True
    )
```

### Bug #2 — Decimate sobre la malla ya unida (7M+ verts) → OOM / crash

Aplicar el modifier Decimate sobre un solo objeto de ~7.1M vértices (después de Join) crashea
con `EXCEPTION_ACCESS_VIOLATION` / "Malloc returns null" incluso con ~5GB ya reservados.
Arreglo: decimar cada uno de los objetos originales por separado (cada uno es pequeño) y
recién después unirlos. Bajó 7.1M → ~490K vértices (ratio 0.05) sin problema.

### Bug #3 — el exportador FBX se cuelga con muchos material slots en una sola malla

Con la malla ya unida (376 material slots en un solo objeto, ~490K vértices),
`bpy.ops.export_scene.fbx()` tardó **más de 30 minutos** sin terminar (ni colgado ni con error
— la CPU seguía subiendo, solo increíblemente lento). Reducir a un único material placeholder
tampoco lo arregló. El exportador FBX de Blender está escrito en Python puro y parece tener
comportamiento cuadrático (o peor) sobre mallas grandes con muchos material slots.

**Arreglo: exportar a OBJ en vez de FBX** (`bpy.ops.wm.obj_export(...)`) — mismo mesh, mismos
376 materiales, terminó en **2.5 segundos**. Unity importa `.obj` sin problema (mismo patrón
ya usado para `YogaStudio.obj`). Si en el futuro hace falta FBX específicamente, probar
primero si el cuello de botella persiste en una build de Blender más reciente antes de asumir
que el modelo es el problema.

Ver también: `docs/architecture.md` — sección Sistema de compañeros (IBondable).
