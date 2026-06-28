# Sistema de comportamiento animal

Documento de diseño y constancia del sistema de **decisión de presa, reacción ante
amenazas y vínculos** que atraviesa a todos los animales. Es la fuente de verdad de esta
línea de trabajo: aquí se diseña, se anota y se lleva el checklist. Añade tareas nuevas a
medida que surjan.

Relacionado: [`architecture.md`](architecture.md) (jerarquía base), `Animal`/`Carnivore`/
`Herbivore`.

---

## Objetivo

Hoy el comportamiento está hardcodeado: `Carnivore.Feed()` caza siempre
`BunnyBehavior.population`, y `Animal.Escape()` solo huye. Queremos un modelo donde **cada
animal** (carnívoro, herbívoro, carroñero, homínido) decida, frente a un objetivo, entre:

- **Cazar / comer** (si el objetivo es presa válida) — según prioridad y hambre.
- **Reaccionar a una amenaza**: huir, luchar, o golpear-y-huir.
- **No hacer daño** si existe un vínculo afectivo suficiente.

El **jugador** es un objetivo más para todos estos casos (presa, amenaza o figura vinculada).

---

## Componentes

### A. `Diet` — priorización de presas (Fase 1)

Reemplaza la presa fija de `Carnivore.Feed()`. Cada especie declara una tabla de presas:

```
[Serializable] class PreyEntry {
    HashSet<GameObject> population;  // p.ej. BunnyBehavior.population, WolfBehavior.population
    float preference;                // mayor = presa preferida
    float difficulty;                // hambre mínima necesaria para molestarse en cazarla
    float range;                     // distancia a la que el cazador la detecta/considera
}
[Serializable] class Diet { PreyEntry[] entries; }
```

**Selección (`Diet.SelectPrey(hunter)`):** una entrada es *elegible* si tiene un miembro
dentro de `range` **y** `hunter.hungry >= difficulty`. Entre las elegibles se elige la de
mayor `preference`; dentro de ella, el miembro más cercano.

Esto produce los casos pedidos de forma natural:
- **Oso polar ignora al conejo si hay una foca cerca** → foca con `preference` mayor; ambas
  de baja `difficulty`; gana la foca cuando está en `range`.
- **Oso se come al lobo solo con mucha hambre** → lobo con `difficulty` alta (solo elegible
  cuando `hungry` es alto) y `preference` baja; solo se elige si no hay nada mejor en rango.
- Presa favorita → `difficulty ≈ 0` (siempre elegible) y `preference` alta.

> Gancho futuro: `SelectPrey` debe descartar objetivos con vínculo (ver D) antes de elegir.

### B. `ThreatResponse` — huir / luchar / golpear-y-huir (Fase 2)

Vive en `Animal` (base), **no** en `Carnivore`: los herbívoros también reaccionan.

```
enum Reaction { Flee, Fight, HitAndRun }
```

**Resolución**, con: poder relativo = `(miMasa + masaAliadosEnRango * factorManada)` vs
`masaEnemigo * velocidadEnemigo`; `defiendoCrías` (hay `Group.fed` cerca y amenazadas); y
flags de especie: `agresividad`, `defiendeCrías`, `puedeHitAndRun`.

- Mucho más fuerte (o manada suficiente) y agresivo → **Fight**: perseguir + `Hurt`;
  los aliados de `Group.members` se suman si están en rango. (La manada es **modificador**,
  no requisito: un solitario fuerte también lucha.)
- Emparejado + defendiendo crías + `puedeHitAndRun` → **HitAndRun**: atacar solo por la
  espalda (`dot(amenaza.forward, dirHaciaMí) > 0`), retroceder cuando la amenaza encara.
  Caso conejo vs serpiente que ataca a sus crías.
- Si no → **Flee** (comportamiento base actual).

### C. `ITarget` + `PlayerTarget` — el jugador como objetivo universal (Fase 3)

```
interface ITarget { Transform transform; float Mass; float Speed; char Faction; void Hurt(float); }
```

`Animal` lo implementa; un componente nuevo `PlayerTarget` lo añade al jugador. Las listas
de presa y de amenaza pasan a referenciar `ITarget`, de modo que cualquier animal puede
atacar/cazar al jugador y viceversa. Punto de contacto: `PlayerCtrl` (añadir el componente y,
si el jugador debe ser presa, registrarlo en las poblaciones relevantes).

### D. Vínculos / afinidad (Fase 4, transversal)

Valor **numérico 0–100** por relación (animal → objetivo), que **modula tanto cazar como
atacar**.

```
[Serializable] class Bond { ITarget target; float value; BondType type; }
enum BondType { Imprint, Friend, ... }   // Imprint = "madre"/criado de pequeño
```

Reglas:
- **100 = vínculo incondicional**: nunca causa daño. Es el máximo; hasta el más salvaje
  puede llegar.
- **< 100**: la probabilidad de hacer daño depende del valor **y del temperamento del
  animal** (`harmVsBond`): un animal salvaje puede dañar a un 50 o incluso a un 90 más
  fácilmente que uno dócil.
- **Cómo se forma**: *Imprint* (criar desde muy pequeño — el caso del jugador como madre),
  *Friend* (convivencia desde joven, p.ej. tigre y león criados juntos en un zoo).
- **Velocidad de vínculo** (`bondGrowthRate`) por especie: a cada animal le cuesta más o
  menos llegar a 100.

Gatea: `SelectPrey` (A) y `ThreatResponse` (B) consultan el vínculo antes de decidir dañar.

---

## Estado actual del stub "player como madre"

La intención ya quedó marcada como **campos sueltos sin lógica**:
`BunnyBehavior.cs:110` y `WolfBehavior.cs:94` (`// LEGAZY`) → `public GameObject mom, player;`.
No hay comportamiento de imprinting activo (lo usaba el viejo `Bear.Hunt()`, ya reemplazado).
La Fase 4 lo formaliza vía `Bond { type = Imprint }`. Al implementarla, eliminar esos campos
huérfanos.

---

## Decisiones de diseño abiertas

- ¿`Diet`/`Bond` se configuran en código (como el resto de la config de especie) o como
  `ScriptableObject` reutilizable? Propuesta: `[Serializable]` en código (visible en
  Inspector) por coherencia con lo existente.
- Unidad de `difficulty` vs `hungry`: confirmar la escala real de `hungry` por especie para
  calibrar los umbrales.
- Facciones (`Faction`): ¿bastan presa/depredador/neutral o hace falta algo más rico?

---

## Notas

_(Área libre para apuntes que surjan durante la implementación.)_

- `BunnyBehavior.Shooted()` (`:144-146`) referencia `StopCoroutine("Hunt")` y
  `StartCoroutine("Sleep")`, coroutines que **no existen** en el modelo `Herbivore`. Bug
  latente — alinear con el modelo nuevo (como se hizo en el oso). Pendiente, fuera del foco
  actual.

---

## Checklist

### Reparación previa (hecho)
- [x] Migrar `BearBehaviour` a `Carnivore` (NPC sin actualizar).
- [x] Mover `BearBehaviour` de `Assets/Bear/` a `Assets/Animals/Bear/` (estructura consistente).
- [x] Crear `SealBehavior` en `Assets/Animals/Seal/` (herbívoro; presa favorita del oso polar).
- [x] Actualizar dieta del oso: foca (preference=15) > conejo (10) > lobo (3, difficulty=50).
- [x] Limpiar código deprecado del lobo (`Attack`/`Deffend`/`Escape` comentado/`LEGAZY`/`children`).

### Fase 1 — `Diet` (priorización de presas)
- [x] Crear tipos `PreyEntry` y `Diet` (`Assets/Animals/Diet.cs`).
- [x] Añadir `Diet` como config abstracta en `Carnivore` y reescribir `Feed()` con
      `SelectPrey` (prioridad + hambre + rango).
- [x] Configurar dieta del lobo (venado preferido + conejo alternativa) y del oso (foca > conejo > lobo con `difficulty` alta).
- [ ] Verificar compilación en Unity (el endpoint de diagnósticos del IDE no respondió;
      pendiente de recompilar en el editor). Calibrar el umbral `difficulty=50` del lobo
      en la dieta del oso contra la escala real de `hungry`.

### Fase 2 — `ThreatResponse` (huir/luchar/golpear-y-huir)
- [x] `enum Reaction { Flee, Fight, HitAndRun }` en `Animal.cs`.
- [x] `ResolveReaction(threat)` en `Animal`: poder relativo (`rig.mass + allyMass * PackFactor` vs `enemyMass * enemySpeed`), defender crías, flags de especie.
- [x] Coroutines `Flee`, `Fight` (con rally de aliados del grupo), `HitAndRun` (posicional: dot-product).
- [x] Flags de especie como propiedades virtuales: `Aggressiveness`, `DefendsCubs`, `CanHitAndRun`, `PackFactor`; configuradas en Wolf, Bear, Bunny, Seal.
- [ ] Retirar el código deprecado del lobo (`Attack`/`Deffend`/`Escape` comentado/`LEGAZY`). _Pendiente: esperar feedback de calibración._

### Fase 3 — `ITarget` + `PlayerTarget`
- [x] Interfaz `ITarget` (`Assets/Scripts/ITarget.cs`): `transform`, `Mass`, `Speed`, `Faction`, `Dead`, `Consumed`, `Hurt`.
- [x] `Animal` implementa `ITarget`; `Carnivore.Feed()`, `Fight()`, `HitAndRun()` usan `ITarget`.
- [x] Componente `PlayerTarget` (`Assets/Scripts/PlayerTarget.cs`): `lp`, `mass`, `speed`, `faction`, `population` estático.
- [ ] En Unity: añadir componente `PlayerTarget` al GameObject del jugador. Para que los animales cacen al jugador, añadir `PlayerTarget.population` a su `Diet`.

### Fase 4 — Vínculos / afinidad
- [x] `enum BondType { Imprint, Friend }` + clase `Bond` (`Assets/Scripts/Bond.cs`).
- [x] Lista `bonds` + `GetBond`, `GrowBond`, `CanHarm` en `Animal`.
- [x] Propiedades `HarmVsBond` y `BondGrowthRate` por especie (Wolf, Bear, Bunny, Seal, Deer).
- [x] Gate en `SelectPrey` (salta candidatos con bond bloqueante) y `ResolveReaction` (fuerza Flee si no puede dañar).
- [x] Eliminar campos huérfanos `mom`, `player` de `BunnyBehavior` (Wolf ya limpiado en sesión anterior).
- [ ] Trigger de crecimiento de bond: llamar `GrowBond(target, BondType.Imprint/Friend, delta)` desde lógica de juego (p.ej. Update/coroutine cuando el animal bebé está cerca del jugador o de otro animal).

### Fase 5 — Sistema de carga y alimentación física

#### Diseño

Tanto el jugador como los animales adultos deben poder **cargar alimento** y **depositarlo en
el suelo**. El alimento depositado es un `FoodItem` real en la escena — lo que unifica
completamente el flujo: cría detecta `FoodItem` vía `Diet`, lo come, bond crece si
`droppedBy != null`.

**`ICarrier`** — interfaz para cualquier entidad que pueda cargar comida:

```csharp
interface ICarrier {
    FoodItem CarriedFood { get; }
    bool PickUp(FoodItem food);          // recoge del suelo (retorna false si ya carga algo)
    FoodItem Drop(Vector3 position);     // instancia FoodItem en posición y libera la carga
}
```

`Animal` implementa `ICarrier` (campo `carriedFood`). `PlayerTarget` también lo implementa;
`PlayerCtrl` llama a `Drop()` cuando el jugador pulsa la tecla de soltar.

**Refactor de `LifeStage.Feed()` (evento de adulto):**

El evento actual transfiere hambre directamente a las crías. Debe reemplazarse por una
lógica de prioridad:

1. **¿Ya carga comida?** → ir a la zona de crías y llamar `Drop(position)`. Prioridad máxima.
2. **¿Hay `FoodItem` en el suelo dentro de rango?** → `PickUp` directo sin cazar
   (`Diet.SelectPrey` ya detecta `FoodItem` si está en la dieta; o un scan separado de
   `FoodItem.GetPopulation(material)` dentro de `HomeRadius`). Evita desperdiciar energía
   cazando si ya hay carroña o restos disponibles.
3. **No hay nada disponible** → caza normalmente (`Carnivore.Feed()`); tras matar y comer
   hasta saciarse, recoge los restos (`PickUp`) y vuelve a depositar cerca de las crías.

En los tres casos, el `FoodItem` depositado lleva `droppedBy = adulto.GetComponent<ITarget>()`,
así el bond cría→padre crece por la misma ruta que cría→jugador.

**`droppedBy` en el evento de adulto:**  
Cuando el adulto llama `Drop()`, el `FoodItem` resultante recibe `droppedBy = adulto.GetComponent<ITarget>()`.
Esto hace que la cría crezca bond con el padre igual que con el jugador — mismo código,
misma ruta.

**`PlayerCtrl`:**  
- Nueva tecla (p.ej. `E`) para recoger `FoodItem` cercanos → `playerCarrier.PickUp(food)`.
- Nueva tecla (p.ej. `Q`) para soltar → `playerCarrier.Drop(posiciónJugador + forward)`.
- El `FoodItem` resultante tiene `droppedBy = playerTarget`.

#### Checklist

- [ ] Interfaz `ICarrier` (`Assets/Scripts/ICarrier.cs`): `CarriedFood`, `PickUp`, `Drop`.
- [ ] `Animal` implementa `ICarrier`; campo `carriedFood`.
- [ ] `PlayerTarget` implementa `ICarrier`; `PlayerCtrl` wirea teclas E/Q.
- [ ] Refactorizar `LifeStage.Feed()`: lógica de prioridad — ya carga → deposita; hay carroña/comida en suelo → PickUp; si no → caza, come, recoge restos, deposita.
- [ ] Ajustar `Diet` de crías para incluir `FoodItem.GetPopulation(material)` con `difficulty=0` y alta `preference`, para que prioricen comida depositada sobre buscar por su cuenta.
- [ ] Verificar que `droppedBy` queda asignado correctamente en ambos flujos (adulto y jugador).

### Fase 6 — Etapas post-natales (crianza activa)

#### Objetivo

Recrear el ciclo de vida de las crías desde el nacimiento hasta la separación definitiva.
Cada **etapa** es una fase declarativa con su propio conjunto de comportamientos para la
madre/padre/otros cuidadores **y** para la cría; la transición entre etapas no depende solo
del tiempo sino de condiciones fisiológicas y conductuales.

#### Etapas

| # | Nombre | Descripción |
|---|--------|-------------|
| 1 | **Dependencia total** | Cría inmóvil, madre presente siempre (altriciales). |
| 2 | **Madre ausente / cría en nido** | Madre caza o busca comida; cría espera. |
| 3 | **Alimentación** | Madre regresa y amamanta o trae comida sólida. |
| 4 | **Exploración temprana** | Cría empieza a moverse pero regresa a la madre. |
| 5 | **Juego** | Interacción social, desarrollo motor (clave en carnívoros y primates). |
| 6 | **Introducción a sólidos** | Transición del destete; método de alimentación cambia. |
| 7 | **Independencia gradual** | Períodos solos cada vez más largos. |
| 8 | **Separación definitiva** | La cría ya no regresa al núcleo familiar. |

No todos los animales pasan por todas las etapas, ni en el mismo orden.
Cada especie declara solo las que le corresponden.

#### Estructura por etapa

```
[Serializable] class PostNatalStage {
    string label;                    // p.ej. "Dependencia total"
    float durationDays;              // duración base en días de simulación
    MotherBehaviorSet motherBehaviors;
    OffspringBehaviorSet cubBehaviors;
    TransitionCondition[] transitions; // condiciones que abren la siguiente etapa
    FeedingMethod feedingMethod;     // Nurse, Regurgitate, FoodItem, None
}
enum FeedingMethod { Nurse, Regurgitate, FoodItem, None }
```

**`TransitionCondition`** puede ser:
- `TimeElapsed(days)` — tiempo mínimo en la etapa.
- `CubWeight(threshold)` — la cría alcanzó un peso mínimo.
- `FirstSolidEaten` — la cría consumió un `FoodItem` por primera vez.
- `FirstNestExit` — la cría salió del nido una vez sola.
- `BondThreshold(value)` — bond madre↔cría supera un valor.

#### Variables de estado continuo (madre y cría)

Ambas entidades comparten las mismas variables; difieren en velocidad de cambio:

| Variable | Sube cuando… | Baja cuando… |
|----------|-------------|--------------|
| `hunger` | pasa el tiempo | come |
| `energy` | descansa | se mueve, mama, caza |
| `temperature` | junto a otro cuerpo, sol | temperatura ambiente baja, está sola |
| `stress` | amenaza cerca, hambre alta | tranquilidad, proximidad a cuidador |

#### Ciclo de decisión de la madre (tick de simulación)

La madre evalúa en orden de prioridad:

1. **¿Hay amenaza externa?** → `ResolveReaction` (Fase 2); todo lo demás espera.
2. **¿Cría vocaliza (`stress > VocalizationThreshold`)?** → amamantar / limpiar / acercar.
3. **¿Tiempo desde última toma > `NurseInterval`?** → amamantar (si `energy > MinEnergyToNurse`).
4. **¿Propia `energy < HuntThreshold`?** → cazar / buscar comida (Fase 5 — `ICarrier`).
5. **Si no** → descansar junto a la cría, acicalar.

El peso de cada acción varía con el estado: una madre hambrienta limpia rápido y se va;
una con energía alta acicala más antes de partir.

#### Ciclo de la cría sola en el nido

| Estado interno | Comportamiento |
|----------------|---------------|
| Hambre baja + energía alta | Exploración pequeña, movimiento |
| Hambre media | Vocalización (llanto); sube `stress` |
| Hambre alta + cansancio alto | Letargo, quietud (ahorra energía) |
| Temperatura baja | Busca calor, se acurruca en el nido |

Las crías no lloran continuamente: vocalizan en picos, se cansan, duermen, reinician.
Modelable con umbrales y timers por ciclo de vocalización.

#### Método de alimentación por tipo de animal y etapa

- **Carnívoros (lobo, zorro, oso):** etapas 1-3 → madre regresa y regurgita (`Regurgitate`);
  etapa 6+ → cría come `FoodItem` directamente.
- **Herbívoros y pinnípedos:** etapas 1-3 → lactancia directa (`Nurse`);
  etapa 6 → introducción a sólidos (`FoodItem`).
- La transición `Nurse → FoodItem` es la señal de la **Etapa 6**.

#### Relación con sistemas previos

- **Fase 4 (Bond):** cada acto de cuidado (`Nurse`, `Regurgitate`, `Groom`) llama
  `GrowBond(cub, Imprint, delta)`. El bond madre↔cría crece con el cuidado.
- **Fase 5 (ICarrier / FoodItem):** la alimentación en etapas tardías pasa exactamente
  por el mismo flujo de `Drop` / `PickUp` que el jugador usa para alimentar animales.
  `droppedBy` queda asignado al adulto; el bond crece por la misma ruta.
- **LifeStage existente:** las etapas post-natales conviven con `Childhood`/`Adolescence`/
  `Adulthood`; `PostNatalStage` es una sub-máquina activa mientras la criatura está en
  `Childhood` (y parte de `Adolescence`).

#### Checklist

- [ ] Diseñar `PostNatalStage`, `MotherBehaviorSet`, `OffspringBehaviorSet`,
      `TransitionCondition`, `FeedingMethod` (`Assets/Scripts/PostNatal/`).
- [ ] Añadir variables de estado continuo (`hunger`, `energy`, `temperature`, `stress`)
      a `Animal` con tasas de cambio configurables por especie.
- [ ] Implementar ciclo de decisión de la madre como coroutine con prioridad.
- [ ] Implementar mini-ciclo de la cría sola: umbrales de vocalización, letargo, exploración.
- [ ] Wiring con Fase 4: `GrowBond` en cada acto de cuidado.
- [ ] Wiring con Fase 5: alimentación tardía usa `FoodItem` vía `ICarrier`.
- [ ] Definir secuencia de etapas para lobo, oso, conejo y foca (primeras especies).
- [ ] Condiciones de transición por especie (tiempo, peso, primer sólido, primer abandono del nido).

### Tareas emergentes
- _(Añadir aquí lo que vaya apareciendo.)_
