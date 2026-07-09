# Setup — Sistema IBody / Stats por Extremidad

Guía de configuración en Unity para el sistema de stats físicas por extremidad.
Scripts involucrados: `PlayerStats`, `IBody`, `BodyPartStats`, `AsanaEvaluator`, `BodyPosition`.

---

## 1. Player — configurar PlayerStats

`PlayerStats` ya implementa `IBody`. En el Inspector aparece la sección **Per-Limb Stats (IBody)**
con un array de 8 slots — uno por parte del cuerpo.

### Orden del array (no cambiar — indexado por `(int)BodyPart`)

| Índice | BodyPart | Descripción |
|---|---|---|
| 0 | Elbows | Codos |
| 1 | Hands | Manos |
| 2 | Knees | Rodillas |
| 3 | Feet | Pies |
| 4 | Hips | Cadera |
| 5 | Back | Espalda |
| 6 | Shoulders | Hombros |
| 7 | Head | Cabeza |

### Valores iniciales sugeridos (jugador sin práctica)

| BodyPart | Flexibility | Strength | Stability |
|---|---|---|---|
| Elbows | 0.3 | 0.2 | 0.2 |
| Hands | 0.3 | 0.2 | 0.2 |
| Knees | 0.25 | 0.2 | 0.2 |
| Feet | 0.25 | 0.2 | 0.25 |
| Hips | 0.2 | 0.15 | 0.2 |
| Back | 0.2 | 0.2 | 0.2 |
| Shoulders | 0.3 | 0.2 | 0.2 |
| Head | 0.5 | 0.4 | 0.4 |

> Estos valores son un punto de partida — calibrar según feedback de jugabilidad.
> Un jugador con 0.0 en todo no puede ejecutar ninguna postura de las del primer set.
> Con estos valores puede intentar las más básicas con desconfort.

### PostureStress — umbrales

`postureStress` se muestra como `[HideInInspector]` — no se configura en Inspector, se acumula en runtime.

| Umbral | Valor | Efecto |
|---|---|---|
| `StumbleThreshold` | 0.5 | Tambaleo — cámara vibra, movimiento reducido |
| `FallThreshold` | 1.0 | Caída — postura interrumpida forzosamente |

`PostureStressHandler.cs` ya está implementado y attached junto a `PlayerStats`; lee estos
valores y drives `PlayerStats.velocity`.

> **Estado real (auditoría 2026-07-09):** `RequestCameraShake` sigue pendiente — hoy
> `PostureStressHandler` piggybacks en `DrainMind(MentalFatigue)` en lugar de una API de
> shake real en `CameraManager`.

---

## 2. Asanas — configurar requisitos de posición

Cada `BodyPosition` dentro de una `Asana` tiene tres campos en Inspector:
- **Required Flexibility** — 0 = ningún jugador falla por flexibilidad
- **Required Strength** — 0 = ningún jugador falla por fuerza
- **Required Stability** — 0 = ningún jugador falla por estabilidad

### Referencia de valores para el primer set (tentativo — calibrar)

#### Urdhva Hastasana (brazos arriba)
| Posición | Flex | Str | Stab |
|---|---|---|---|
| Hands (arriba) | 0.3 | 0.25 | 0.1 |
| Shoulders | 0.35 | 0.3 | 0.2 |
| Feet (separados, planta) | 0.1 | 0.1 | 0.3 |

#### Virabhadrasana I (guerrero I)
| Posición | Flex | Str | Stab |
|---|---|---|---|
| Feet (delantero doblado) | 0.3 | 0.3 | 0.35 |
| Hips (abiertos al frente) | 0.5 | 0.2 | 0.3 |
| Shoulders | 0.3 | 0.3 | 0.2 |

#### Virabhadrasana II (guerrero II)
| Posición | Flex | Str | Stab |
|---|---|---|---|
| Feet (separados) | 0.3 | 0.25 | 0.3 |
| Hips (abiertos al lado) | 0.55 | 0.2 | 0.3 |
| Knees (doblada) | 0.2 | 0.35 | 0.3 |

> Posturas más exigentes (Sarvangasana, Matsyasana) tendrán requisitos más altos.
> Rellenar el resto cuando se definan los beneficios por asana (pendiente de diseño).

---

## 3. AsanaEvaluator — uso

`AsanaEvaluator` es una clase plana (no MonoBehaviour). Se instancia desde el sistema que abre la paleta:

```csharp
// Ejemplo desde un AsanaSystem o PlayerController:
AsanaEvaluator evaluator = new AsanaEvaluator(asanaQueue, unlockedAsanas);
PaletteConfig config = new PaletteConfig
{
    mode      = PaletteConfig.Mode.Formula,
    elements  = bodyPositionElements,   // PaletteElementData con payload = BodyPosition
    evaluator = evaluator,
    maxSelection = 3,                   // ajustar según la asana más compleja
};
palette.Open(config, playerGameObject);
```

### Leer el feedback de la evaluación

`PaletteResult.payload` contiene `List<PositionEvaluation>` cuando viene de `AsanaEvaluator`:

```csharp
void OnFormulaEvaluated(PaletteResult result)
{
    var evals = result.payload as List<PositionEvaluation>;
    if (evals == null) return;

    foreach (PositionEvaluation e in evals)
    {
        if (e.quality == PositionQuality.Correct)
            ShowCorrect(e.part);
        else if (e.quality == PositionQuality.DirectionOkLowStat)
            ShowDiscomfort(e.part, e.limitingDim, e.statGap);
        else if (e.quality == PositionQuality.Impossible)
            ShowImpossible(e.part);
    }
}
```

---

## 4. NPCs con IBody (maestra, Goluis, Panterilia)

Cualquier NPC que tenga stats físicas por extremidad añade `PlayerStats` o un componente
dedicado que implemente `IBody`. La maestra, por ejemplo:

1. Crear componente `TeacherStats : MonoBehaviour, IBody`  
   (o reutilizar `PlayerStats` si el NPC tiene el mismo set de stats)
2. Configurar `bodyStats[8]` con valores altos (maestra: flexibility ≥ 0.9 en todo)
3. `TeacherNPC.cs` referencia su propio `IBody` para comparar con el del jugador

---

## 5. TeacherNPC — añadir personalidad

`TeacherNPC` es abstracta. La primera implementación concreta es `MaestraTeacher`.
Para añadir un nuevo profesor:

1. Crear `MiProfesor : TeacherNPC`
2. Sobrescribir los cuatro hooks: `OnWrongFormula`, `OnImpossiblePositions`, `OnShortcutErrors`, `OnSuccess`
3. Añadir el componente al GameObject del NPC en escena
4. Enlazar a la Palette: desde el sistema que abre la paleta, llamar `teacher.EvaluateResult(result)` en el callback `OnFormulaEvaluated`

Los helpers `PartName(BodyPart)` y `DimName(BodyStatDimension)` ya están en español en la clase base.
`Say(string)` usa `Debug.Log` — reemplazar con `DialogueManager` cuando exista.

---

## 6. Pendientes de implementar

- [x] `PostureStressHandler.cs` — tambaleo/caída (afecta a `PlayerStats`/`PlayerController`)
- [x] `TeacherNPC.cs` + `MaestraTeacher.cs` — feedback por calidad de posición
- [ ] Completar requisitos de posición para todas las asanas del primer set
- [ ] Calibrar valores iniciales de `bodyStats` con feedback de jugabilidad
- [ ] Definir cómo se visualiza la calidad de posición en la Palette (color de botón, vibración, icono)
- [ ] `PostureStressHandler.RequestCameraShake` — reemplazar piggyback de `mentalFatigue` con API real de shake en `CameraManager`
