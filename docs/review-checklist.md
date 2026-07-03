# Review checklist — Cold Sanctuary

Lista de trabajo pendiente, decisiones de diseño abiertas y deuda técnica.
Actualizar al resolver cada ítem.

---

## Diseño — NPCBase (visión corregida)

Un NPC no tiene "personalidad scripted". Tiene un **estado acumulado real**:

- Cuerpo entrenado de cierta manera (o no): qué extremidades ha ejercitado, cuánto,
  con qué posiciones. Un NPC que carga sacos tiene brazos fuertes y tensión lumbar.
- Qué come: afecta `fatReserves`, temperatura, energía disponible.
- Qué hace en su tiempo libre: asanas, descanso, socialización → drena o restaura stats.
- Las responsabilidades tienen coste físico/mental. Si no las gestiona, se acumula.
- Su "personalidad" es **cómo se le describe** a partir de ese estado — no es su causa.

**ThoughtAnchors** modulan *cómo responde a las opciones*, no producen efectos hardcodeados.
Un NPC con `yoga_skepticism` alto puede elegir no usar asanas cuando está estresado,
pero no "drena estrés del jugador porque trabaja mucho".

**El jugador experimenta** el estado real del NPC por proximidad — no un efecto scripted
de "este personaje tiene esta personalidad".

```
LivingEntity
  └─ NPCBase : IMind, IBondable
       │  stats reales: mentalFatigue, stress, satisfaction, sleepiness
       │  stats físicas: cuerpo entrenado (IBody?) / alimentación / descanso
       │  responsabilidades → costes de wellbeing
       │  ThoughtAnchors → modulan decisiones, no outputs fijos
       │
       └─ CompanionBase : NPCBase
              ├─ Goluis       (parámetros: drives, anchors, tasas de cambio)
              ├─ Panterilia   (parámetros)
              └─ Gohageneis   (parámetros)
```

---

## Diseño — Mundo subterráneo y escala del juego

La introducción termina cuando se revela la entrada secreta al santuario subterráneo
de monstruos. Cada nivel exige mayor expertise con hechizos. Los personajes (jugador
y compañeros) pueden quedarse en un nivel o bajar según sus stats.

### Tipos de entidad en los niveles más profundos

| Tipo | Base | IMind | IBondable | Notas |
|---|---|---|---|---|
| Animal / monstruo salvaje | `LivingEntity` → `Animal` | ✗ | ✅ bond crece con cuidado | igual que superficie |
| NPC compañero | `NPCBase` | ✅ | ✅ | stats gating por nivel |
| Animal humanizado / marioneta | `LivingEntity` + `IMind` | ✅ | ✅ | criatura modificada, no NPC autónomo |

**Animal humanizado**: extiende `LivingEntity`, implementa `IMind` (drives básicos +
estado mental real), pero NO necesariamente `NPCBase`. Sus ThoughtAnchors son más
rígidos (creados, no formados por experiencia). Podría ser una línea aparte:

```
LivingEntity
  ├─ Animal (drives físicos puros)
  ├─ NPCBase : IMind, IBondable  (autonomía plena)
  └─ HumanizedCreature : IMind, IBondable  (creación, anchors hardwired)
```

O simplemente que `NPCBase` admita un flag `isConstruct` que limite el cambio de anchors.

### Presupuesto de rendimiento con 40 animales/monstruos

- La mayoría correrá solo `LivingEntity` (sin `IMind`) → muy ligero
- Humanizados con `IMind`: presupuesto de 8–12 por zona activa
- Sin zona activa: tick reducido o congelado
- Los bonds entre animales salvajes solo necesitan comprobarse dentro del radio de percepción

---

## Diseño — Sistema de bonds (visión corregida)

Los bonds no son una relación especial NPC→jugador. Son vínculos entre **cualquier par
de entidades**: NPC↔NPC, NPC↔animal, NPC↔objeto inanimado (lugar favorito, herramienta).

- Goluis y Gohageneis tienen bond entre sí. Si están cerca, tenderán a interactuar.
- Trabajar juntos produce stats distintos que trabajar solos.
- Un NPC con bond con un lugar específico (su banco favorito) lo buscará para descansar.
- El jugador NO recibe trato especial — es una entidad más que puede tener bond con NPCs,
  animales y objetos.

**Problema actual**: `IBondable` está hardcodeado para el jugador (`BondWithPlayer`,
`GrowBondWithPlayer`). Necesita generalizarse. El almacenamiento sí es genérico
(`LivingEntity.bonds: List<Bond>` acepta cualquier `ITarget`).

**Cambio necesario en IBondable**:
```csharp
// Reemplaza BondWithPlayer / GrowBondWithPlayer:
float GetBondStrength(ITarget target);
void  GrowBond(ITarget target, float amount);
float GetProximityEffect(ITarget source, MindChannel channel);
```

### Presupuesto de rendimiento (móvil)

Proximidad de bonds = O(N²) pares por tick. Mitigaciones obligatorias:

| Mitigación | Impacto |
|---|---|
| Tick rate bajo (0.5–2s, no frame) | ÷10–60x evaluaciones |
| Radio máximo de bond (solo entidades cercanas) | elimina la mayoría de pares |
| Activación por zona (solo zona activa corre simulación completa) | escala con diseño de nivel |
| LOD de comportamiento (fuera de vista → tick reducido) | ahorra ~75% en NPCs lejanos |

**Presupuesto razonable para móvil**: 8–10 NPCs con simulación completa por zona activa.
El elenco de este juego (3 compañeros + maestra + NPCs menores) entra cómodamente.

---

## Implementación pendiente

- [ ] **NPCBase** — diseño listo, pendiente de implementar
  - Extiende `LivingEntity`, implementa `IMind` + `IBondable`
  - Drives reales: mentalFatigue, stress, satisfaction, sleepiness
  - Responsabilidades como demandas con coste si no se resuelven
  - DriveSelector: evalúa acciones disponibles por estado actual (no por personalidad)
  - ThoughtAnchors modulan el score de acciones, no producen side-effects fijos

- [ ] **CompanionBase → NPCBase** — migrar `CompanionBase` para extender `NPCBase`
  - Los compañeros actuales (Goluis, Panterilia, Gohageneis) se vuelven solo parámetros

- [x] **IBondable — generalizar** — `GetBondStrength(MonoBehaviour)`, `GrowBond(MonoBehaviour, float)`,
  `GetProximityEffect(MonoBehaviour, MindChannel)`. Actualizado: `IBondable`, `CompanionBase`,
  `WorldBondable`, `BondActivity`. El jugador no recibe trato especial en la interfaz.

- [ ] **IMindSimple — eliminar** — cuando CompanionBase migre a NPCBase ya no hace falta

- [ ] **Panterilia fix** — `observationRadius` necesita escritura desde Panterilia
  - Opciones: añadir setter a IMind, o método `SetObservationBonus(float)` en IMind
  - Ver `Assets/Scripts/Companion/Companions/Panterilia.cs`

- [ ] **AsanaQueue → PlayerStats/IMind** — AsanaQueue acumula beneficios pero no los entrega
  - Brecha: `StatType` (asanas) vs `MindChannel` (IMind)
  - Ver `docs/known-issues.md`

- [ ] **BearBehaviour** — incompleto, usa `IAnimal` directamente e ignora `LivingEntity`
  - Opciones: migrar a `LivingEntity` junto con NPCBase, o como tarea separada

- [ ] **ObservationManager + ObservableObject** — sistema de observación pendiente de implementar
  - Ver `docs/observation-system.md`

- [ ] **NPCRelation** — clase para relaciones NPC-a-NPC (no solo NPC-jugador)
  - Necesaria para que los compañeros se afecten mutuamente

---

## Decisiones de diseño abiertas

- [ ] **¿`HumanizedCreature` como clase aparte o `NPCBase` con flag `isConstruct`?**
  - Clase aparte: más limpio conceptualmente, menos herencia compleja
  - Flag: menos clases, más parámetros — riesgo de condicionales en NPCBase
  - Afecta: cómo se gestan los anchors y si pueden cambiar con la narrativa

- [ ] **¿Qué stats gatean el acceso a un nivel?**
  - ¿Un threshold de IMind (mentalFatigue < X, stress < Y)?
  - ¿Un skill de hechizo específico?
  - ¿Un bond con un NPC que ya bajó?
  - Afecta: diseño de progression y cómo CompanionBase expone nivel actual del personaje

- [ ] **¿Dónde vive la lógica de proximidad de bond?**
  - Opción A: cada entidad evalúa sus propios bonds en su tick (distribuido)
  - Opción B: un `BondProximityManager` central evalúa todos los pares (centralizado, más fácil de optimizar con spatial bucketing)
  - La opción B permite aplicar las mitigaciones de rendimiento en un solo lugar

- [ ] **¿Qué pasa cuando dos NPCs con bond positivo trabajan juntos?**
  - ¿Se reduce la tasa de acumulación de fatigue?
  - ¿Se restaura satisfaction más rápido?
  - ¿El efecto escala con el bond strength o es binario (cerca/lejos)?

- [ ] **¿`hungry` float en NPCBase o en LivingEntity?**
  - LivingEntity ya tiene `RespondToHunger()` — mover el float allí unificaría con Animal.
  - Contra: Animal usa `hungry` con semántica diferente (hambre de caza vs hambre social).

- [ ] **NPCAction — ¿ScriptableObject o clase C#?**
  - ScriptableObject: configurable desde Inspector, fácil de iterar por diseñadores.
  - Clase C#: más rápido de implementar, sin overhead de serialización.
  - Híbrido: clase C# con datos en SO (ScriptableObject como config, clase como lógica).

- [ ] **¿NPCBase implementa IBody?**
  - Los compañeros tienen cuerpos entrenados relevantes para asanas (lesiones, capacidades).
  - Contra: agrega complejidad; se puede diferir a una segunda fase.

- [ ] **IMind — ¿setter para observationRadius?**
  - Opción A: `float observationRadius { get; set; }` (setter en interfaz).
  - Opción B: método `SetObservationBonus(float delta)` — más controlado, permite stacking.
  - Afecta: Panterilia fix + diseño de NPCBase.

- [ ] **PlayerStats — ¿mantener como clase única o dividir?**
  - Actual: un MonoBehaviour implementa IBody + IMind.
  - Alternativa: `PlayerBody` + `PlayerMind` en el mismo GameObject.
  - No hay urgencia; la interfaz ya abstrae el acceso.

- [ ] **Histéresis en DriveSelector**
  - ¿Cuánto tiempo mínimo en una acción antes de poder cambiar?
  - ¿Cómo afectan ThoughtAnchors a la histéresis (un NPC con alta voluntad cambia menos)?

---

## Documentación pendiente

- [ ] **`docs/npcbase.md`** — crear cuando se implemente NPCBase
- [ ] **`CLAUDE.md` — abstracciones** — actualizar cuando NPCBase esté implementada
  - Sacar `IVital` de la lista (ya eliminada)
  - Añadir NPCBase cuando exista

---

## Deuda técnica conocida

Ver `docs/known-issues.md` para lista completa con `archivo:línea`.
Items principales:

- `WolfBehavior.cs ~145–275` — `Escape()` legacy comentado, código muerto
- `BearBehaviour.cs` — máquina de estados por flags booleanos, duplica lógica de `Animal`
- `PlayerCtrl.cs` — lógica placeholder y bugs de posicionamiento
- `ActionsPrep.cs` / `ActionPrep.cs` — confusión de nombres documentada
- Idioma de comentarios mezclado (inglés/español)

---

## Completado recientemente

- [x] `LivingEntity` implementada y `Animal` migrada
- [x] `IAnimal` eliminada (archivo vacío, solo para git history)
- [x] `IVital` eliminada — `aware` y `RespondToThreat()` viven en `LivingEntity`
- [x] `IMind` + `MindChannel` implementadas; `PlayerStats` las implementa
- [x] `CompanionBase` desacoplada de `PlayerStats` — usa `IMind` + `IBondable`
- [x] `IBondable` actualizada a `MindChannel`
- [x] `Carnivore` usa `LivingEntity` en lugar de `IVital`/`IAnimal`
- [x] `docs/architecture.md` actualizada con jerarquía actual
- [x] `docs/living-entity.md` actualizada con estado completado + siguiente fase
- [x] `docs/ibody-imind.md` actualizada de "propuesta" a "implementado"
