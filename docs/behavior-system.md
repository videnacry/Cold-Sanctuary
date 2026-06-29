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

- [x] Interfaz `ICarrier` (`Assets/Scripts/ICarrier.cs`): `CarriedFood`, `PickUp`, `Drop`.
- [x] `Animal` implementa `ICarrier`; campo `carriedFood`.
- [x] `PlayerTarget` implementa `ICarrier`.
- [x] `FoodItem.GetAll()` — registro global para scan eficiente sin FindObjectsOfType.
- [x] Refactorizar `LifeStage.Feed()`: lógica de prioridad — ya carga → deposita; hay comida en suelo → PickUp; si no → caza (Restore maneja esto); Carnivore.Feed recoge restos al terminar.
- [ ] `PlayerCtrl`: wirear tecla E → `PickUp(FoodItem cercano)` y Q → `Drop(posiciónJugador + forward)`.
- [ ] Ajustar `Diet` de crías para incluir `FoodItem.GetPopulation(material)` con `difficulty=0` y alta `preference`, para que prioricen comida depositada sobre buscar por su cuenta.
- [ ] Verificar en Unity que `droppedBy` queda asignado correctamente en ambos flujos (adulto y jugador).

### Fase 6 — Etapas post-natales (crianza activa)

#### Objetivo

Recrear el ciclo de vida de las crías desde el nacimiento hasta la separación definitiva.
Cada **etapa** es una fase declarativa con su propio conjunto de comportamientos para la
madre/padre/otros cuidadores **y** para la cría; la transición entre etapas no depende solo
del tiempo sino de condiciones fisiológicas y conductuales.

#### Principio: los comportamientos emergen de las variables

No se hardcodea "el conejo visita dos veces al día". Si `BaseStressLevel` es alto y
`NestSecurityLevel` es bajo, el sistema llega solo a esa conclusión. Tres ejes generan
los parámetros de especie, y los parámetros generan el comportamiento:

| Eje | Determina |
|-----|-----------|
| Posición en la cadena alimenticia | `BaseStressLevel`, `ThreatThreshold`, estrategia de refugio |
| Tamaño corporal / reservas energéticas | duración de lactancia, `maxFatReserves`, frecuencia de caza |
| Estructura social | distribución del cuidado, rol del padre, `VocalizationThreshold` |

#### Etapas

| # | Nombre | Descripción |
|---|--------|-------------|
| 0 | **Nacimiento** | Secuencia fija de la madre (ver abajo); no ponderada. |
| 1 | **Dependencia total** | Cría en letargo casi constante, madre presente. |
| 2 | **Madre ausente / cría en nido** | Madre caza o busca comida; cría espera. |
| 3 | **Alimentación activa** | Madre regresa y amamanta o trae comida sólida. |
| 4 | **Exploración temprana** | Cría empieza a moverse pero regresa al cuidador. |
| 5 | **Juego** | Interacción social, desarrollo motor (clave en carnívoros). |
| 6 | **Introducción a sólidos** | Transición del destete; método de alimentación cambia. |
| 7 | **Independencia gradual** | Períodos solos cada vez más largos. |
| 8 | **Separación definitiva** | La cría ya no regresa al núcleo familiar. |

No todos los animales pasan por todas las etapas, ni en el mismo orden.
Cada especie declara solo las que le corresponden.

#### Estructura por etapa

Cada `PostNatalStage` tiene dos capas de comportamiento:

1. **`entryActions`** — lista ordenada ejecutada una sola vez al entrar al stage (p.ej. nacimiento).
2. **`loopBehaviors`** — comportamientos ponderados que se ejecutan repetidamente durante el stage.

Los comportamientos individuales (`Limpiar`, `Estimular`, `Guiar`, `Acicalar`, `Amamantar`…)
son **universales** — cualquier especie puede usarlos. Lo que varía es si aparecen en
`entryActions` o en `loopBehaviors`, y con qué peso/condición de activación.

```csharp
[Serializable] class PostNatalStage {
    string label;
    float durationDays;
    NestType nestType;
    FatherRole fatherRole;
    MotherPresencePattern presencePattern;
    WeaningType weaningType;
    MotherAction[] entryActions;       // secuenciales, una sola vez
    MotherBehaviorSet loopBehaviors;   // ponderados, bucle durante el stage
    OffspringBehaviorSet cubBehaviors;
    TransitionCondition[] transitions;
    FeedingMethod feedingMethod;
}

enum NestType              { Burrow, OpenField, Beach, SnowDen }
enum FatherRole            { Absent, Provider, ActiveCaregiver }
enum MotherPresencePattern { Continuous, FrequentVisits, MinimalVisits, ProgrammedAbandonment }
enum WeaningType           { Gradual, Abrupt }
enum FeedingMethod         { Nurse, Regurgitate, FoodItem, None }
```

**`TransitionCondition`** puede ser:
- `TimeElapsed(days)` — tiempo mínimo en la etapa.
- `CubWeightAbove(kg)` — la cría alcanzó un peso mínimo.
- `CubReadinessScore(threshold)` — score compuesto (peso de grasa + indicadores de autonomía).
- `MotherFatReservesBelow(threshold)` — reservas propias bajo umbral (abandono de foca).
- `FirstSolidEaten` — la cría consumió un `FoodItem` por primera vez.
- `FirstNestExit` — la cría salió del nido/zona de ocultamiento una vez sola.
- `BondThreshold(value)` — bond cuidador↔cría supera un valor.
- `ThreatLevelBelow(threshold)` — zona lo suficientemente segura para transición.

#### Variables de estado (universales, parámetros distintos por especie)

**Estado continuo** — ambas entidades (madre y cría), distinta velocidad de cambio:

| Variable | Sube cuando… | Baja cuando… |
|----------|-------------|--------------|
| `hunger` | pasa el tiempo | come |
| `energy` | descansa | se mueve, mama, caza |
| `fatReserves` | come más de lo necesario | actividad intensa, letargo, lactancia |
| `temperature` | junto a otro cuerpo, sol | ambiente frío, sola |
| `stress` | amenaza cerca, hambre alta | tranquilidad, proximidad a cuidador |

`fatReserves` es **universal**: todos los animales la tienen, solo difieren en `maxFatReserves`
y en los umbrales que la activan. Un conejo con mucha grasa por intervención humana llega a
un máximo bajo y lo gasta rápido por ser muy activo; nunca alcanza el umbral de letargo salvo
en condiciones extremas. Los parámetros hacen el resto — no hay casos especiales en el código.

**Parámetros de especie** (propiedades virtuales en `Animal`):

```csharp
public virtual float BaseStressLevel       => 0.2f; // estrés en reposo; conejo ~0.8, oso ~0.1
public virtual float ThreatThreshold       => 0.5f; // umbral para despertar; oso en letargo: muy alto
public virtual float VocalizationThreshold => 0.4f; // estrés mínimo para que la CRÍA llore
public virtual float NestSecurityLevel     => 0.5f; // seguridad del refugio; madriguera oso: 0.9
public virtual float MaxFatReserves        => 20f;  // capacidad máxima de reservas
public virtual float FatAccumulationRate   => 1f;   // rapidez de acumulación al comer de más
```

#### Ciclo de decisión de la madre (tick de simulación)

La madre evalúa en orden de prioridad:

1. **¿Hay amenaza externa?** → `ResolveReaction` (Fase 2); todo lo demás espera.
2. **¿Cría vocaliza (`stress > VocalizationThreshold`)?** → amamantar / limpiar / acercar.
3. **¿Tiempo desde última toma > `NurseInterval`?** → amamantar (si `energy > MinEnergyToNurse`).
4. **¿Propia `energy < HuntThreshold`?** → cazar / buscar comida (Fase 5 — `ICarrier`).
5. **Si no** → descansar junto a la cría, acicalar.

El peso de cada acción varía con el estado: una madre hambrienta limpia rápido y se va;
una con energía alta acicala más antes de partir.

#### Sistema motivacional — los comportamientos emergen, no se hardcodean

El principio central del diseño: **no se hardcodea "el conejo visita dos veces al día"**.
Si `baseStressLevel` es alto y `nestSecurityLevel` es bajo, el sistema llega solo a esa
conclusión. Tres ejes generan los parámetros de especie, y los parámetros generan el comportamiento:

| Eje | Determina |
|-----|-----------|
| **Posición en cadena alimenticia** | `baseStressLevel`, `threatThreshold`, estrategia de refugio |
| **Tamaño corporal / reservas energéticas** | duración de lactancia, capacidad de ayuno, frecuencia de caza |
| **Estructura social** | distribución del cuidado, rol del padre, `vocalizationThreshold` permitido |

**Parámetros de especie nuevos** (propiedades virtuales en `Animal`, no estado variable):

```csharp
public virtual float BaseStressLevel    => 0.2f; // estrés basal en reposo; conejo ~0.8, oso ~0.1
public virtual float ThreatThreshold    => 0.5f; // umbral para despertar/reaccionar; oso en letargo: muy alto
public virtual float VocalizationThreshold => 0.4f; // hambre/estrés mínimo para que la CRÍA llore; venado: alto
public virtual float NestSecurityLevel  => 0.5f; // seguridad percibida del refugio; madriguera oso: 0.9, campo venado: 0.1
```

**Variable de estado adicional:**

```csharp
public float fatReserves = 0f; // 0–100; la osa en letargo consume esto en lugar de cazar
```

#### Letargo — comportamiento universal

El letargo no es exclusivo del oso. Es un `Behavior` genérico con parámetros por especie
y etapa de vida:

| Trigger | Animal | Duración | `lethargyDepth` |
|---------|--------|----------|----------------|
| Invierno (`temperature < umbral`) | Oso | Meses | Muy alto — solo despierta con amenaza seria |
| Hambre muy alta + energía baja | Cría (cualquier especie) | Horas | Bajo |
| Estómago lleno (`hungry << 0`) | Carnívoro adulto | Horas | Bajo |
| Juventud (`lifeStage == child`) | Crías altriciales | Constante | Bajo |

`lethargyDepth` determina cuánto hay que superar `ThreatThreshold` para despertar. Un oso
en letargo de invierno necesita una amenaza muy alta; una cría hambrienta que cayó en letargo
despierta con cualquier estímulo. El código es el mismo; solo cambian los parámetros.

#### Stage 0 — Nacimiento (entryActions)

Las primeras horas son una **secuencia ordenada** — la madre no puede saltarse ningún paso
independientemente de su propio estado. Modelado como `entryActions[]` del Stage 0:

1. `Limpiar` — activa respiración y circulación. _(Comportamiento universal: también usado como Groom periódico.)_
2. `Estimular` — empuje con hocico, volteo; activa reflejos vitales. _(Universal.)_
3. `GuiarPezon` — orienta a la cría hacia el pezón. _(Universal.)_
4. `PrimeraToma` — calostro; anticuerpos críticos. Aplica solo en Stage 0.

Los mismos comportamientos (`Limpiar`, `Estimular`) aparecen luego en `loopBehaviors` de
stages posteriores como acicalar periódico, pero con distinto peso y condición de activación.

#### Ciclo de decisión de la madre

La madre evalúa en orden de prioridad cada tick:

1. **¿Hay amenaza externa?** → `ResolveReaction` (Fase 2); todo lo demás espera.
2. **¿`presencePattern` permite estar aquí ahora?** → Si es `MinimalVisits`, solo entra si
   `environmentStress < BaseStressLevel` (noche/depredadores dormidos) Y lleva más de
   `minVisitInterval` desde la última visita. Si no cumple, se aleja y espera.
3. **¿Cría vocaliza (`cubStress > cubVocalizationThreshold`)?** → amamantar / limpiar / acercar.
4. **¿Tiempo desde última toma > `NurseInterval`?** → amamantar (si `energy > MinEnergyToNurse`).
5. **¿Propia `hunger > HuntThreshold`?** → cazar / buscar comida (Fase 5 — `ICarrier`).
   Si `fatReserves > 0` y el animal puede vivir de grasa (oso en letargo), consume reservas.
6. **Si no** → descansar junto a la cría, acicalar (`Limpiar`).

La ventana de visita del conejo se abre naturalmente cuando los depredadores bajan su actividad
nocturna → `environmentStress` baja → condición 2 se cumple. No hay "a las 2am fijo" en código.

`HomeOrigin` es **mutable**: cualquier comportamiento puede llamar `SetHomeOrigin(pos)`.
El oso bebé tiene como `HomeOrigin` la posición donde la madre decidió echarse. El venado
actualiza el `HidingSpot` de la cría cada vez que la deja. La manada de lobos puede relocalizar
si `environmentStress > relocationThreshold` durante N días → todos los miembros actualizan.

#### Ciclo de la cría sola

| Estado interno | Comportamiento |
|----------------|---------------|
| Hambre baja + energía alta | Exploración pequeña, movimiento |
| Hambre media (> `VocalizationThreshold`) | Vocalización en picos, sube `stress` |
| Hambre alta + cansancio alto | Letargo (universal) — quietud, ahorra energía |
| Temperatura baja | Busca calor, se acurruca |
| `stress` alto + `NestSecurityLevel` bajo | Inmovilidad — suprime movimiento y sonido |

#### Foca — abandono como outcome emergente

El abandono no está hardcodeado. Emerge cuando dos condiciones se cruzan:
- `CubReadinessScore > readinessThreshold` — score compuesto de peso de grasa + indicadores
  de autonomía (¿ha mostrado comportamiento acuático?).
- `MotherFatReservesBelow(threshold)` — la madre ya no puede sostener la lactancia.

**Si la madre no pudo acumular grasa suficiente** (por interferencia de depredadores):
- Produce menos leche → cría crece más lenta → `CubReadinessScore` tarda más en madurar.
- Si las reservas se agotan antes de que la cría esté lista → abandono prematuro → cría muere.
- Este es un **outcome emergente del entorno**, no un caso especial en el código. El jugador
  que proteja a la madre foca durante el embarazo impacta directamente la supervivencia.

#### Manada de lobos — roles diferenciados

No todos los adultos ejecutan el ciclo completo de madre. Cada adulto tiene un rol:

| Rol | Comportamiento post-natal |
|-----|--------------------------|
| Madre alpha | Ciclo completo (Fases 0-8) |
| Padre alpha | `Provider`: caza, regresa, regurgita; vigila perímetro |
| Adultos auxiliares | `Guard`: permanecen en la madriguera mientras otros cazan; pueden amamantar (alolactancia) |
| Hermanos de camada anterior | `Guard` + `Provider` secundario según su propio `hungry` |

Los cachorros desarrollan bond con múltiples adultos a través de estas interacciones —
`GrowBond` se llama desde cada acto de cuidado, no solo desde la madre.

#### Fichas por especie

| Especie | `BaseStressLevel` | `NestSecurityLevel` | `VocalizationThreshold` | `MaxFatReserves` | `FatherRole` | `PresencePattern` | `WeaningType` |
|---------|:-----------------:|:-------------------:|:-----------------------:|:----------------:|:------------:|:-----------------:|:-------------:|
| Lobo | 0.3 | 0.7 | 0.3 | 15 | Provider | Continuous | Gradual |
| Oso | 0.1 | 0.9 | 0.5 | 100 | Absent | Continuous | Gradual |
| Conejo | 0.85 | 0.3 | 0.6 | 5 | Absent | MinimalVisits | Gradual |
| Venado | 0.6 | 0.1 | 0.9 | 10 | Absent | FrequentVisits | Gradual |
| Foca | 0.4 | 0.6 | 0.5 | 80 | Absent | Continuous→Abrupt | Abrupt |

#### Relación con sistemas previos

- **Fase 4 (Bond):** cada acto de cuidado (`Nurse`, `Regurgitate`, `Limpiar`) llama
  `GrowBond(cub, Imprint, delta)`. Auxiliares del lobo también crecen bond con los cachorros.
- **Fase 5 (ICarrier / FoodItem):** alimentación tardía usa el mismo flujo `Drop`/`PickUp`.
  `droppedBy` asigna al cuidador; bond crece por la misma ruta que con el jugador.
- **LifeStage existente:** `PostNatalStage[]` es una sub-máquina activa durante `Childhood`
  (y parte de `Adolescence`). No reemplaza `LifeStage`; conviven.
- **`TimeController`:** `presencePattern` consulta la hora del día para determinar si la
  ventana de visita está abierta (conejo, animales nocturnos).

#### Decisiones resueltas

- **Letargo** → comportamiento universal con parámetros de especie. No es un `LifeStage` propio.
- **`HomeOrigin` del venado** → mutable vía `SetHomeOrigin(pos)`; la madre lo actualiza al
  dejar a la cría en cada `HidingSpot`. Misma propiedad, nuevo semántico.
- **Manada de lobos** → roles diferenciados; no todos ejecutan el ciclo completo.
- **Visitas del conejo** → ventana basada en `environmentStress + TimeController`; no timer fijo.
- **`fatReserves`** → universal en `Animal`; `MaxFatReserves` por especie hace el resto.
- **Foca sin grasa** → outcome emergente; no hay caso especial.

#### Checklist

- [x] Propiedades virtuales en `Animal`: `BaseStressLevel`, `ThreatThreshold`,
      `VocalizationThreshold`, `NestSecurityLevel`, `MaxFatReserves`, `FatAccumulationRate`.
- [x] Campos de estado en `Animal`: `stress`, `fatReserves`, `temperature`; decay de `stress` en `Restore()`.
- [x] `PostNatalStages` virtual en `Animal`; `Init()` arranca `PostNatalManager` si presente.
- [x] `Assets/Scripts/PostNatal/PostNatalEnums.cs` — enums + `MotherAction` universales.
- [x] `Assets/Scripts/PostNatal/TransitionCondition.cs` — 8 tipos + `ComputeReadiness`.
- [x] `Assets/Scripts/PostNatal/PostNatalStage.cs` — stage con `entryActions[]` + `transitions[]`.
- [x] `Assets/Scripts/PostNatal/PostNatalManager.cs` — máquina de stages, ciclo madre (prioridad),
      ciclo autónomo crías, acumulación de `fatReserves`, temperatura dinámica, wiring Fase 4 y 5.
- [ ] Definir `PostNatalStage[]` por especie (override `PostNatalStages` + añadir componente
      `PostNatalManager` al prefab madre): lobo, oso, conejo, venado, foca.
- [ ] Roles diferenciados en manada de lobos: `Guard`, `Provider`, `AlloNurse`.
- [ ] Calibrar umbrales `CubReadinessScore` por especie (foca: ~2.5; oso: ~1.8).
- [ ] Integrar ventana `MinimalVisits` con hora del día de `TimeController` (conejo nocturno).
- [ ] Wiring animaciones/sonido de llanto en `CubSoloCycle`.
- [ ] Setear `FirstSolidEaten` desde `Carnivore.Feed` / `Diet` al comer un `FoodItem`.
- [ ] Setear `FirstNestExit` desde nav cuando la cría se aleje > `HomeRadius` por primera vez.
- [ ] Letargo universal: trigger (temperatura, hambre alta + agotamiento, juventud), depth.

### Tareas emergentes
- _(Añadir aquí lo que vaya apareciendo.)_
