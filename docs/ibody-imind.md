# IBody / IMind / IBondable — Estado actual

## Estado de implementación

| Interfaz | Estado | Implementa |
|---|---|---|
| `IBody` | ✅ Implementada | `PlayerStats` |
| `IMind` | ✅ Implementada | `PlayerStats` |
| `MindChannel` | ✅ Implementado | — |
| `IBondable` | ✅ Implementada | `CompanionBase`, `WorldBondable` |
| `IMindSimple` | ⚠️ Transitoria | `CompanionBase` (hasta NPCBase) |

---

## Tres interfaces ortogonales

```
IBody      — stats físicas por extremidad y estrés postural (sistema de asanas)
IMind      — estado mental/emocional: satisfacción, fatiga, estrés, sueño, observación
IBondable  — capacidad de vínculo con el jugador y efecto por proximidad
```

Cualquier entidad puede implementar cualquier combinación.

---

## Quién implementa qué (estado actual)

| Entidad | IBody | IMind | IBondable |
|---|---|---|---|
| `PlayerStats` | ✅ | ✅ | — |
| `CompanionBase` | — | `IMindSimple` (transitorio) | ✅ |
| `WorldBondable` | — | — | ✅ |
| `NPCBase` (pendiente) | tbd | ✅ planificado | ✅ planificado |
| `Animal` | — | — | ✅ (via LivingEntity.bonds) |

---

## IBody

```csharp
public interface IBody {
    BodyPartStats GetBodyPartStats(BodyPart part);
    void          TrainBodyPart(BodyPart part, BodyStatDimension dimension, float delta);
    float         postureStress { get; }
    void          AccumulatePostureStress(float amount);
    void          ReleasePostureStress(float amount);
}
```

`PlayerStats` lo implementa. Los stats de extremidades (flexibilidad, fuerza, estabilidad)
son el sustrato del sistema de asanas.

---

## IMind

```csharp
public interface IMind {
    float satisfaction       { get; }
    float satisfactionCapacity { get; }
    float mentalFatigue      { get; }
    float stress             { get; }
    float sleepiness         { get; }
    float observationRadius  { get; }

    void RestoreMind(float amount, MindChannel channel);
    void DrainMind(float amount, MindChannel channel);
}

public enum MindChannel {
    Satisfaction, MentalFatigue, Stress, Sleepiness, Observation
}
```

Implementado en `PlayerStats` con explicit interface members. Sistemas que afectan
stats mentales reciben `IMind` en lugar de `PlayerStats` directamente.

---

## IMindSimple — transitoria

```csharp
public interface IMindSimple {
    float fatigue { get; set; }
    float stress  { get; set; }
    float mood    { get; set; }
}
```

Implementada por `CompanionBase`. Se elimina cuando `CompanionBase` migre a `NPCBase`,
que implementará `IMind` completa con sus propios stats reales.

---

## Routing desacoplado

Sistemas que afectan stats del jugador ya no dependen de `PlayerStats`:

```csharp
// CompanionBase.ApplyProximityEffect():
_playerMind.RestoreMind(amount, primaryChannel);   // IMind, no PlayerStats

// PostureStressHandler:
_stats.DrainMind(amount, MindChannel.MentalFatigue); // via IMind
```

---

## Panterilia — excepción temporal

`Panterilia.cs` mantiene una referencia directa a `PlayerStats` para escribir
`observationRadius` (IMind solo tiene getter). Pendiente: añadir setter o
`SetObservationBonus(float)` a IMind.

---

## Conexión AsanaQueue → IMind — hecha

> **Estado real (auditoría 2026-07-09):** esta conexión ya no está pendiente.
> `AsanaQueue.DeliverBenefit()` llama `PlayerStats.RestoreMind(gain, activeAsana.channel)`
> (`AsanaQueue.cs:66`).

---

## Referencias

- `Assets/Scripts/IBody.cs`
- `Assets/Scripts/IMind.cs`
- `Assets/Scripts/IMindSimple.cs`
- `Assets/Scripts/MindChannel.cs`
- `Assets/Scripts/Player/PlayerStats.cs`
- `Assets/Scripts/Companion/CompanionBase.cs`
- `docs/review-checklist.md` — pendientes de diseño abiertos
