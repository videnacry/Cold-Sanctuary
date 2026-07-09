# Sistema de Asanas — Cold Sanctuary

Diseño técnico del sistema de asanas y ejercicios: selección de posiciones, evaluación de calidad,
stats por extremidad, daño por mala postura y feedback del profesor.

Ver también: DEVLOG.md §Interfaz de Posturas / Asanas · `docs/ibody-imind.md` (IBody contiene las stats)

---

## Principio central

Una asana no se ejecuta bien o mal — se ejecuta con distinto grado de corrección por cada extremidad.
La calidad de cada posición depende de los stats del jugador para esa parte del cuerpo.
Con el tiempo y la práctica, los stats crecen y las posiciones se ejecutan mejor.

Esto refleja cómo funciona el yoga real: la flexibilidad, la fuerza y la estabilidad se desarrollan
gradualmente; el practicante aprende a tolerar el desconfort productivo y a distinguirlo del dolor
que señala daño.

---

## Stats por extremidad — BodyPartStats

Cada una de las 8 partes del cuerpo (`BodyPart`) tiene tres dimensiones:

| Dimension  | Qué mide | Ejemplo |
|---|---|---|
| **Flexibility** | Rango de movimiento | Sin flexibilidad en caderas → no puede abrir las piernas en Virabhadrasana II |
| **Strength**    | Capacidad de sostener carga | Sin fuerza en hombros → no puede mantener brazos en alto |
| **Stability**   | Control del equilibrio en esa zona | Sin estabilidad en pies → tambalea en posturas de un pie |

Cada dimensión va de 0 a 1. Valores iniciales bajos — el jugador empieza sin práctica.

```csharp
[Serializable]
public class BodyPartStats
{
    public float flexibility; // 0–1
    public float strength;    // 0–1
    public float stability;   // 0–1
}
```

Estas stats viven en `IBody` indexadas por `BodyPart`:
```csharp
public interface IBody {
    BodyPartStats GetBodyPartStats(BodyPart part);
    void TrainBodyPart(BodyPart part, BodyStatDimension dimension, float delta);
    // ... resto de IBody
}

public enum BodyStatDimension { Flexibility, Strength, Stability }
```

---

## Requisitos por posición — BodyPositionRequirement

Cada `BodyPosition` dentro de una asana tiene requisitos mínimos de stats:

```csharp
[Serializable]
public class BodyPosition
{
    public BodyPart bodyPart;
    public string   description;
    public KeyCode  hotkey;

    [Header("Stat requirements to execute correctly")]
    public float requiredFlexibility; // 0–1 — 0 = cualquier jugador puede
    public float requiredStrength;
    public float requiredStability;
}
```

Si el jugador no alcanza el requisito, la posición no se ejecuta correctamente —
pero puede intentarse con consecuencias.

---

## Calidad de posición — PositionQuality

```csharp
public enum PositionQuality
{
    Correct,              // stats suficientes — posición bien ejecutada
    DirectionOkLowStat,   // intención correcta, stats insuficientes — desconfort productivo
    Wrong,                // posición incorrecta — acumula estrés/daño
    Impossible,           // stats tan bajos que el cuerpo no puede intentarlo
}
```

> **Estado real (auditoría 2026-07-09):** el código solo define **3** valores —
> `Correct` / `DirectionOkLowStat` / `Impossible` — con un único umbral 0.7. `Wrong`
> no existe como estado de `PositionQuality`.

### Lógica de evaluación por extremidad

```
gap = max(required - playerStat, 0)   // cuánto falta

gap == 0          → Correct
0 < gap <= 0.3    → DirectionOkLowStat  (desconfort, sin daño inmediato)
0.3 < gap <= 0.7  → DirectionOkLowStat  (desconfort mayor, posible daño acumulado)
gap > 0.7         → Impossible
```

Si la posición seleccionada no corresponde a ninguna requerida → `Wrong`.

> **Estado real (auditoría 2026-07-09):** el código evalúa con un solo umbral 0.7
> (no dos tramos) y no tiene rama `Wrong` para posición seleccionada sin correspondencia.

### Consecuencias por calidad

| PositionQuality | Efecto |
|---|---|
| **Correct** | Beneficio completo. Stats de esa extremidad crecen lentamente. |
| **DirectionOkLowStat** | Beneficio reducido (× 0.5). Disconfort acumulado (→ `IMind.stress`). Stats de esa extremidad crecen más rápido — el esfuerzo entrena. |
| **Wrong** | Sin beneficio. Daño acumulado (→ `IBody.Hurt` o campo `postureStress`). Tambaleo si acumula suficiente. |
| **Impossible** | La extremidad visualmente "no llega". Sin daño pero sin beneficio. El profesor lo señala. |

> **Estado real (auditoría 2026-07-09):** `IBody.Hurt` no existe en la interfaz `IBody`
> del código actual; y el estado `Wrong` de esta tabla no tiene equivalente en `PositionQuality`.

### Acumulación y tambaleo

El campo `postureStress` (en `IBody`) registra el estrés estructural por mala postura.
Al superar un umbral:
- **Umbral 1** (0.5): tambaleo leve — la cámara vibra suavemente
- **Umbral 2** (0.8): tambaleo pronunciado — movimiento reducido
- **Umbral 3** (1.0): caída — la postura se interrumpe forzosamente

El `postureStress` se drena cuando la posición se corrige o al terminar la asana.

---

## Evaluación enriquecida — PositionEvaluation

`AsanaEvaluator.Evaluate()` devuelve, además del `PaletteResult`, una lista de evaluaciones
por posición que el sistema de feedback puede usar:

```csharp
public struct PositionEvaluation
{
    public BodyPart        part;
    public PositionQuality quality;
    public float           statGap;        // cuánto falta (0 si Correct)
    public BodyStatDimension limitingDim;  // qué dimensión limita (Flexibility / Strength / Stability)
}
```

`PaletteResult` incluye esto en su payload:
```csharp
public struct PaletteResult {
    public bool               success;
    public string             outcomeId;
    public float              magnitude;
    public SpatialArrangement spatial;
    public object             payload;     // List<PositionEvaluation> cuando viene de AsanaEvaluator
}
```

---

## Feedback del profesor (Teacher NPC)

Cuando el jugador practica delante de un NPC con rol de profesor, ese NPC observa
la evaluación de posiciones y puede dar tres tipos de feedback:

### 1. Fórmula incorrecta
Las posiciones seleccionadas no corresponden a ninguna asana conocida.
> *"Eso no es ninguna postura que yo conozca."*

### 2. Posición correcta pero stat insuficiente
`PositionQuality == DirectionOkLowStat`.
> *"La dirección de [Hips] va bien — te falta flexibilidad para llegar del todo.
>   Con práctica lo lograrás."*

El profesor distingue entre "estás equivocado" y "estás en camino pero el cuerpo no llega todavía".
Esta distinción es clave para el aprendizaje — y para la narrativa de Goluis (que presiona)
vs la maestra (que guía).

### 3. Posición bien ejecutada
`PositionQuality == Correct` en todas las posiciones.
> *"Bien. Mantente ahí."*

### Implementación del profesor

El NPC profesor tiene acceso a `IBody` del jugador (vía referencia al Player).
Observa el `List<PositionEvaluation>` del `PaletteResult` y elige su línea de diálogo.

> **Estado real (auditoría 2026-07-09):** `TeacherNPC.Say()` solo hace `Debug.Log` —
> no elige/reproduce diálogo real todavía. Además `TeacherNPC.EvaluateResult` nunca se
> conecta a `Palette.OnFormulaEvaluated` en código; solo el Editor instancia estas clases
> (huérfano en runtime).

```csharp
public class TeacherNPC : MonoBehaviour
{
    public void EvaluateStudentPose(PaletteResult result)
    {
        if (!result.success) { SayFormulaMistake(); return; }

        var evals = result.payload as List<PositionEvaluation>;
        if (evals == null) return;

        foreach (PositionEvaluation e in evals)
        {
            if (e.quality == PositionQuality.DirectionOkLowStat)
                SayLowStat(e.part, e.limitingDim, e.statGap);
            else if (e.quality == PositionQuality.Wrong)
                SayWrongPosition(e.part);
        }
    }
}
```

La maestra puede elegir comentar solo el error más grave, o comentar todos en secuencia.
Goluis comenta todos a la vez con más brusquedad. La personalidad del NPC se expresa
en *cómo* da el feedback, no en *qué* sabe.

---

## Crecimiento de stats

Las stats de cada extremidad crecen según cómo se usa:
- **Posición Correct** → crecimiento lento y sostenido (práctica regular = mantenimiento)
- **Posición DirectionOkLowStat** → crecimiento más rápido — el estrés productivo entrena
- **Posición Wrong o Impossible** → sin crecimiento (el cuerpo no aprende de posiciones incorrectas)
- **Ejercicios básicos** (flexiones, abdominales, piernas) → crecimiento específico por extremidades involucradas

Esto significa que el jugador progresa más rápido intentando posturas que le cuestan —
siempre que la dirección sea correcta. El desconfort productivo tiene recompensa.

---

## Relación con el sistema de IBody/IMind

| Stat | Interfaz | Nota |
|---|---|---|
| `BodyPartStats` por extremidad | `IBody` | Stats físicas de cada parte del cuerpo |
| `postureStress` | `IBody` | Estrés acumulado por mala postura — umbral de tambaleo/caída |
| `stress` (mental) | `IMind` | El desconfort prolongado eleva también el estrés mental |
| Tambaleo / caída | `IBody` | Superado el umbral, afecta movimiento del jugador (`PlayerController`) |

NPCs (la maestra, Goluis) también tienen `IBody` con sus propias stats de extremidades.
La maestra tiene `flexibility >= 0.9` en la mayoría de partes — puede demostrar cualquier postura.
Goluis tiene alta `strength` pero `flexibility` media — explica su estilo de enseñanza brusco.

---

## Pendientes de diseño

- [ ] Definir requisitos de stats concretos por posición para cada asana del primer set
- [ ] Definir valores iniciales de `BodyPartStats` para el jugador (todos bajos, diferenciados por ejercicio previo)
- [ ] Definir tasa de crecimiento por stat dimension y nivel de asana
- [ ] Definir umbral de `postureStress` que activa tambaleo y caída
- [ ] Definir líneas de diálogo del profesor para los tres tipos de feedback
- [ ] Decidir si la caída por postura activa una animación específica o solo interrumpe
- [ ] Decidir cómo visualizar la calidad de cada posición en la Palette (color, icono, vibración)
- [ ] Definir si `BodyPartStats` de animales/crías afectan su comportamiento (p.ej. un ciervo con pata dañada)

---

## Código pendiente

| Componente | Descripción |
|---|---|
| `BodyPartStats.cs` | Struct serializable con flexibility/strength/stability |
| `IBody` ampliado | `GetBodyPartStats(BodyPart)` + `TrainBodyPart()` + `postureStress` |
| `BodyPosition` ampliado | Campos `requiredFlexibility/Strength/Stability` |
| `AsanaEvaluator` v2 | Evaluación por calidad + `List<PositionEvaluation>` en payload |
| `PaletteResult` payload | Campo `object payload` para feedback enriquecido |
| `TeacherNPC.cs` | Lee `List<PositionEvaluation>` y elige línea de diálogo |
| `PostureStressHandler.cs` | Attached al Player. Lee `postureStress`, aplica tambaleo/caída |
