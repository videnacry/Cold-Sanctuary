# IBody / IMind / IBondable — Propuesta de arquitectura

Propuesta de refactorización de interfaces de stats para Cold Sanctuary.
**No implementar hasta confirmar diseño completo.** Leer primero: DEVLOG.md §Propuesta de arquitectura — IBody / IMind / IBondable.

---

## El problema actual

`PlayerStats` mezcla stats físicas y mentales/emocionales en un solo componente.
`IAnimal` describe un cuerpo físico pero su nombre implica "solo animales".
`CompanionBase` duplica parcialmente stats mentales (`fatigue`, `stress`, `mood`) sin interfaz común con el jugador.

Resultado: los sistemas que quieren restaurar o drenar stats del jugador tienen que conocer `PlayerStats` directamente, lo que crea acoplamiento innecesario.

---

## La propuesta — tres interfaces ortogonales

```
IBody      — estado físico: velocidad, resistencia, temperatura, reservas de grasa
IMind      — estado mental/emocional: satisfacción, fatiga mental, estrés, sueño, observación
IBondable  — capacidad de vínculo: bond con el jugador, efecto por proximidad
```

Cualquier entidad puede implementar cualquier combinación de las tres.

---

## Quién implementa qué

| Entidad | IBody | IMind | IBondable |
|---|---|---|---|
| Jugador (`PlayerStats`) | ✅ velocity, physicalResistance | ✅ satisfaction, mentalFatigue, stress, sleepiness, observationRadius | — (es quien forma los vínculos) |
| Animal / Cría | ✅ (migrado de IAnimal) | parcial — stress, vocalización | ✅ bond crece con cuidado |
| Compañero (`CompanionBase`) | parcial — fatigue física | ✅ fatigue, stress, mood | ✅ restaura o drena al jugador por proximidad |
| Objeto / Lugar (`WorldBondable`) | — | — | ✅ esterilla, sol, montaña |

---

## IAnimal → IBody

`IAnimal` es en realidad una descripción del cuerpo físico. Renombrarlo a `IBody` lo hace aplicable al jugador, NPCs y compañeros sin cambiar su semántica.

```csharp
// Actual:
public interface IAnimal {
    AnimationsName animationsName { get; }
    bool aware { get; set; }
    void Hurt(float damage);
    IEnumerator Escape(bool team, List<GameObject> enemies);
}

// Propuesto:
public interface IBody {
    float velocity           { get; }
    float physicalResistance { get; }
    float temperature        { get; }   // ya existe en Animal.cs
    float fatReserves        { get; }   // ya existe en Animal.cs
    void  Hurt(float damage);
    IEnumerator Escape(bool team, List<GameObject> enemies);
}
```

`animationsName` y `aware` son implementaciones internas de `Animal.cs` y no necesitan estar en la interfaz pública.

---

## IMind (nueva interfaz)

```csharp
public interface IMind {
    float satisfaction      { get; }
    float mentalFatigue     { get; }
    float stress            { get; }
    float sleepiness        { get; }
    float observationRadius { get; }

    void RestoreMind(float amount, MindChannel channel);
    void DrainMind(float amount, MindChannel channel);
}

public enum MindChannel {
    Satisfaction,
    MentalFatigue,
    Stress,
    Sleepiness,
    Observation
}
```

`PlayerStats` implementaría `IMind` directamente. `CompanionBase` podría implementarla para sus propias stats mentales.

---

## Routing — una acción afecta cuerpo, mente y vínculo a la vez

En lugar de que cada sistema conozca `PlayerStats`, el efecto se enruta por interfaces:

```csharp
// Ejemplo conceptual:
void ApplyEffect(GameObject target, EffectData effect) {
    IBody     body = target.GetComponent<IBody>();
    IMind     mind = target.GetComponent<IMind>();
    IBondable bond = target.GetComponent<IBondable>();

    if (effect.isPhysical && body != null)
        body.RestoreBody(effect.amount, effect.bodyChannel);

    if (effect.isMental && mind != null)
        mind.RestoreMind(effect.amount, effect.mindChannel);

    if (effect.growsBond && bond != null)
        bond.GrowBondWithPlayer(effect.bondAmount);
}
```

Una asana puede restaurar `MentalFatigue` (IMind), subir `physicalResistance` (IBody) y crecer el bond (IBondable) en una sola llamada, sin que ningún sistema sepa nada de los otros.

---

## Migración de stats desde PlayerStats

| Stat actual en PlayerStats | Migra a |
|---|---|
| `satisfaction`, `satisfactionCapacity`, `satisfactionPassiveRate`, `restorationMultiplier` | `IMind` |
| `mentalFatigue` | `IMind` |
| `stress` | `IMind` |
| `sleepiness` | `IMind` |
| `observationRadius` | `IMind` |
| `velocity` | `IBody` |
| `physicalResistance` | `IBody` |

`PlayerStats` puede quedar como MonoBehaviour que implementa ambas interfaces, o dividirse en `PlayerBody` + `PlayerMind` en el mismo GameObject.

---

## Implicación en los compañeros

`CompanionBase` ya tiene `fatigue`, `stress`, `mood`. Si implementa `IMind`, el efecto por proximidad se vuelve genérico:

```csharp
// Antes (acoplado a PlayerStats):
playerStats.Restore(amount, StatChannel.MentalFatigue);

// Después (desacoplado):
IMind mind = player.GetComponent<IMind>();
mind?.RestoreMind(amount, MindChannel.MentalFatigue);
```

Esto permite que el sistema de compañeros funcione para cualquier entidad con `IMind`, no solo el jugador — útil si crías o NPCs también tienen stats mentales.

---

## IMindSimple — versión mínima para compañeros

Para `CompanionBase` no hace falta implementar `IMind` completa. Puede implementar solo un subconjunto con las stats que ya tiene:

```csharp
public interface IMindSimple {
    float mentalFatigue { get; set; }
    float stress        { get; set; }
    float mood          { get; set; }
}
```

`CompanionBase` implementa `IMindSimple`. El jugador implementa `IMind` (que podría extender `IMindSimple`).

---

## Impacto en sistemas existentes

| Sistema | Impacto del cambio |
|---|---|
| `CameraManager` | Lee `PlayerStats` directamente. Necesitará referencia a `IMind` en lugar de (o además de) `PlayerStats` |
| `CompanionBase.ApplyProximityEffect()` | Actualmente llama `playerStats.Restore()`. Migrar a `IMind.RestoreMind()` |
| `BondActivity.RegisterExperience()` | Lee `playerStats.stress`. Migrar a `IMind.stress` |
| `Panterilia` | Modifica `playerStats.observationRadius`. Migrar a `IMind` |
| `AsanaQueue.DeliverBenefit()` | Actualmente solo llena contenedores propios. Necesita referencia a `IMind` para restaurar al jugador |
| `BondActivityManager.TickBlockCheck()` | Lee `playerStats.satisfaction`. Migrar a `IMind.satisfaction` |

---

## Pendientes antes de implementar

- [ ] Decidir si `PlayerStats` implementa ambas interfaces o se divide en dos componentes
- [ ] Definir `BodyChannel` como reemplazo de `StatChannel` para stats físicas
- [ ] Definir si `IMindSimple` existe como subconjunto de `IMind` o son independientes
- [ ] Evaluar si `IAnimal.animationsName` y `IAnimal.aware` migran a `Animal.cs` directamente o necesitan estar expuestos en `IBody`
- [ ] Auditar todos los sitios que llaman `playerStats.*` directamente y catalogar qué interfaz necesitan
- [ ] Decidir si `BearBehaviour` (que implementa `IAnimal` independiente) migra a `IBody` en la misma tarea

---

## Referencias

- `Assets/Scripts/Player/PlayerStats.cs` — implementación actual con `StatChannel`
- `Assets/Scripts/Companion/IBondable.cs` — ya implementada
- `Assets/Scripts/Companion/CompanionBase.cs` — candidata a `IMind`/`IMindSimple`
- `Assets/Animals/IAnimal.cs` — candidata a renombrar como `IBody`
- DEVLOG.md §Propuesta de arquitectura — IBody / IMind / IBondable
- `docs/architecture.md` §Interfaces actuales
