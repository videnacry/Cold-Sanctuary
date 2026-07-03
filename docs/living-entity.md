# LivingEntity — base compartida para animales y NPCs

## Premisa de diseño

Animales y personajes comparten los mismos drives fundamentales.
La diferencia no es de naturaleza sino de sofisticación de respuesta.

| Drive | Animal | NPC (refugio) | NPC (área salvaje) |
|---|---|---|---|
| Hambre | Caza directa por masa corporal | Navega al comedor | Forrajeo / negociación mágica |
| Amenaza | Huye si masa depredador > umbral | Conflicto social, tensión interpersonal | Magia + táctica de grupo |
| Vínculos | IBondable + Family.cs | IBondable + grupo de compañeros | IBondable + jerarquía mágica |
| Familia/población | Family, FamilyGenerator, spawning | Grupo de personajes con historia | Grupo + roles de poder |
| Ciclo de vida | LifeStage (Childhood→Adulthood) | Arco narrativo por niveles | Arco de maestría mágica |

## Jerarquía propuesta

```
LivingEntity (abstract MonoBehaviour)
│
│   Drives (datos, todos los seres los tienen)
│   ├── float hunger           // 0 = sin hambre, 1 = crítico
│   ├── float threatLevel      // percepción de peligro actual
│   ├── IBondable bonds        // vínculos con otros
│   └── LifeStage lifeStage   // etapa actual
│
│   Hooks (abstract — cada subclase resuelve a su manera)
│   ├── abstract void RespondToHunger()
│   ├── abstract void RespondToThreat(ThreatSource source)
│   ├── abstract float EvaluateThreat(LivingEntity source)
│   └── abstract void OnLifeStageChanged(LifeStage prev, LifeStage next)
│
├── Animal : LivingEntity
│   RespondToHunger()     → busca presa, ataca si Carnivore
│   EvaluateThreat()      → ratio masa depredador/presa
│   RespondToThreat()     → flee por radio (sistema actual en WolfBehavior etc.)
│
├── NPCBase : LivingEntity
│   RespondToHunger()     → NavMesh al comedor (StatChannel.Hunger)
│   EvaluateThreat()      → evalúa habilidades propias + tamaño del grupo
│   RespondToThreat()     → huida social → magia → tácticas de grupo
│   + ThoughtAnchor       → creencias que modulan las respuestas
│   + IMind               → fatiga, estrés, estado de ánimo
│   + DialogueManager     → verbaliza el drive
│
└── PlayerEntity : NPCBase   (o hereda directo de LivingEntity)
    RespondToHunger()     → dispara evento UI (jugador decide)
    RespondToThreat()     → input del jugador + sistema de magia
    EvaluateThreat()      → habilidades del jugador + compañeros en zona
```

## ThreatResponse: masa vs habilidades

El único campo que cambia semánticamente es `EvaluateThreat()`:

**Animal:**
```csharp
override float EvaluateThreat(LivingEntity source) {
    if (source is Animal predator)
        return predator.bodyMass / this.bodyMass;  // sistema actual
    return 0f;
}
```

**NPCBase (área salvaje):**
```csharp
override float EvaluateThreat(LivingEntity source) {
    float myPower = skills.combat + (groupSize * GroupSynergy);
    float threat = source.GetThreatRating();
    return Mathf.Clamp01(threat / myPower);
}
```

El `RespondToThreat()` puede evolucionar por niveles del arco narrativo:
- Nivel 1-2: huida social (igual que animal pero hacia refugio)
- Nivel 3-4: confrontación verbal / ritual
- Nivel 5+: magia defensiva + coordinación de grupo

## Estado de la migración

**LivingEntity está implementada** (`Assets/Scripts/LivingEntity.cs`).

### Lo que se completó

- `LivingEntity` creada con drives: `stress`, `trauma`, `fatReserves`, `temperature`, `bonds`, `aware`, `death`, `asleep`
- `Animal` migrada a `LivingEntity` — `bonds`, `GrowBond`, `CanHarm`, `EffectiveBondGrowthRate`, `HarmVsBond`, `BondGrowthRate`, `MaxFatReserves`, `FatAccumulationRate` viven en LivingEntity
- Hooks implementados en Animal: `RespondToHunger()`, `EvaluateThreat()`, `RespondToThreat()` (public)
- `IAnimal` eliminada. `IVital` eliminada. `aware` vive en LivingEntity.
- Carnivore usa `GetComponent<LivingEntity>()` + `RespondToThreat()` + `.aware`

### Siguiente fase — NPCBase

Ver `docs/architecture.md` §NPCBase y el diseño en DEVLOG.md.

```
NPCBase : LivingEntity, IMind, IBondable
  └─ CompanionBase (capa de interacción con el jugador)
       └─ Goluis / Panterilia / Gohageneis (solo parámetros)
```

### Qué NO sube a LivingEntity

- `hungry`, `exhaustion`, `lp`, `sensibility` — mecánicas específicas de Animal
- `Family`, `FamilyGenerator` — solo animales
- Spawning logic — solo animales
- `Carnivore`/`Herbivore` split — solo animales

## Relación con IBody / IMind

`LivingEntity` no implementa `IBody` ni `IMind`.
Los drives de `LivingEntity` son comportamentales (qué decide hacer el ser).
`IBody`/`IMind` son capacitivos (qué puede hacer).

```
NPCBase : LivingEntity, IMind, IBondable
Animal  : LivingEntity  (sin IMind sofisticado — solo drives básicos)
PlayerStats             (implementa IBody + IMind; no extiende LivingEntity todavía)
```

## Archivos relacionados

- `Assets/Scripts/LivingEntity.cs` — implementación actual
- `docs/ibody-imind.md` — estado de IMind/IBody
- `Assets/Animals/Animal.cs` — primera subclase de LivingEntity
