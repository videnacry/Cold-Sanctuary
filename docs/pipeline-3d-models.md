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

---

## Especies árticas — lista actualizada

| Especie | Estado | Fuente | Formato |
|---|---|---|---|
| **Lobo** (Wolf) | ✅ Descargado | Quaternius Animal Pack | FBX |
| **Ciervo/Stag** | ✅ Descargado | Quaternius Animal Pack | FBX |
| **Zorro** (Fox) | ✅ Descargado | Quaternius Animal Pack | FBX |
| **Husky** | ✅ Descargado | Quaternius Animal Pack | FBX |
| **Conejo** (Bunny) | ✅ Extraído | Sketchfab | FBX |
| **Oso Polar** | ⚠️ Pendiente conversión | Sketchfab (kenchoo) | GLB → FBX |
| **Foca** (Seal) | ⚠️ Pendiente conversión | Sketchfab (rkuhlf) | GLB → FBX |
| **Ballena** (Whale) | 🔽 Por descargar | Quaternius Fish Pack | FBX |

### 🌟 Alternativa recomendada — Quirky Series Arctic Animals Vol 1 (Omabuarts)

**URL Sketchfab:** https://sketchfab.com/3d-models/quirky-series-arctic-animals-vol-1-0db6031197ad4f288d691c14390d677a#download

Un solo pack con **9 animales árticos**, todos rigged con 19 animaciones y 29 blendshapes/shapekeys:
- Oso polar, Foca, Zorro ártico, Reno, Morsa, Búho nival, Armiño, Pingüino, Pájaro
- Descargar desde Sketchfab (requiere login) → formato GLB → convertir con Blender o aspose.app
- Versión FBX disponible en su web de pago: https://www.omabuarts.com

Si se compra la versión de pago ($89 el pack o individual más barato), se obtiene FBX directo sin conversión.

### Conversión GLB → FBX (Oso Polar y Foca)

Los archivos GLB están en tu carpeta de Descargas. Para convertirlos:
- **Online (sin instalar nada):** https://products.aspose.app/3d/conversion/glb-to-fbx
- **Con Blender:** File → Import → glTF 2.0 → luego File → Export → FBX
- Una vez convertidos, mover a `Assets/Animals/PolarBear/Models/` y `Assets/Animals/Seal/Models/`

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

Ver también: `docs/architecture.md` — sección Sistema de compañeros (IBondable).
