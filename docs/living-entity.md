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

## Plan de migración

**No migrar ahora.** El sistema de `Animal` funciona. La migración rompe escenas existentes.

### Secuencia correcta

1. **Ahora (este doc):** Diseño reservado, interfaces alineadas.
2. **Primer NPC con drives reales** (hambre/amenaza):
   - Crear `LivingEntity` como clase abstracta real
   - Migrar un animal de prueba (Bunny o Deer — los más simples)
   - Verificar que `IBondable`, `LifeStage` y los behaviors siguen funcionando
3. **Después de validar:** migrar el resto de `Animal`
4. **Cuando existan companions jugables:** `NPCBase` hereda de `LivingEntity`

### Qué migrar de Animal

| Animal actual | LivingEntity |
|---|---|
| `hunger` (si existe) | `float hunger` en base |
| `ThreatResponse.cs` | `RespondToThreat()` + `EvaluateThreat()` |
| `LifeStage` lifecycle | `LifeStage lifeStage` en base |
| `IBondable` impl | `IBondable bonds` en base |
| `bodyMass` | queda en `Animal` (no aplica a NPCs) |

### Qué NO sube a LivingEntity

- `bodyMass` — sólo animales
- `Family`, `FamilyGenerator` — solo animales (NPCs tienen grupos distintos)
- Spawning logic — sólo animales
- `Carnivore`/`Herbivore` split — sólo animales

## Relación con IBody / IMind

`LivingEntity` no implementa `IBody` ni `IMind` directamente.
Los drives de `LivingEntity` son comportamentales (qué decide hacer el ser).
`IBody`/`IMind` son capacitivos (qué puede hacer físicamente/mentalmente).

```
LivingEntity.RespondToHunger()
    → consulta IMind.stress para modular urgencia
    → consulta IBody.postureStress para ver si puede moverse

NPCBase : LivingEntity, IBody, IMind  (implementa los tres)
Animal  : LivingEntity                 (solo drives, sin IMind sofisticado)
PlayerEntity : LivingEntity            (IBody via PlayerStats, IMind pendiente)
```

## Archivos relacionados

- `docs/ibody-imind.md` — especificación de IBody/IMind
- `docs/behavior-system.md` — sistema presa/amenaza/vínculos actual (base de Animal)
- `Assets/Animals/Animal.cs` — clase actual a migrar eventualmente
- `Assets/Scripts/IAnimal.cs` — interfaz que LivingEntity absorberá
