# Disolver `Animal` → todo ser es un `Anima` + componentes

**Estado (2026-08-17):** propuesta/plan. Este doc alinea el objetivo antes del refactor.

## Secuencia acordada

1. **Fase 0 — cablear `Animal` a los stats** (primero, independiente de la decisión estructural): que la conducta
   que hoy usa `rig.mass`/`NavMeshAgent.speed`/umbrales sueltos pase a leer las **aptitudes del `Anima`**
   (`bodyMass`, `strength`, `armadura`, `Predation`, `autoabandono`, bonds). Aporta valor se extienda o se disuelva
   después, y de-riesga. **← empezamos aquí.**
2. **Después**, con la conducta ya stat-driven, decidir **extender vs disolver** (el resto de este doc) — se verá
   más claro qué queda "de body" en `Animal` una vez la lógica depende de stats.

### Fase 0 — progreso

- [x] **`ResolveReaction`** (fight/flee): poder por `Predation.EffectivePower` (stats + manada por facción) × `(1+autoabandono)`,
  en vez de `rig.mass × NavMeshAgent.speed` + bucle propio de manada. El ratio `myPower/enemyPower` es **scale-invariant**
  → sin recalibrar umbrales (`>1.5`, `peligro`). (PR #93)
- [x] **Velocidad** (locomoción) ya stat-driven vía `WalkSpell` (opt-in, PR #92).
- [ ] **`EvaluateThreat`** base (línea ~158): aún parte de `rig.mass × NavMeshAgent.speed` antes del ratio de `Predation`.
  Migrar a ratio puro **cambia la escala** del retorno → hay que **recalibrar `ThreatThreshold`** (hoy 0.5). *Paso siguiente, con test.*
- [ ] **Alerta de proximidad** (línea ~371): `enemyMass·(enemySpeed/2) − distancia ≤ sensibility` mezcla masa física y
  metros. Migrar a `Predation.PredatorPower` exige recalibrar `sensibility`/la fórmula. *Paso siguiente, con test.*
- [ ] `Mass`/`Grams`/daño siguen en `rig.mass` (masa física real, dinámica con la etapa de vida) — decidir si el
  daño/tamaño usa el stat `bodyMass` o la masa real (probablemente ambos: stat = norma de especie, rig = escala por etapa).

## Por qué disolver (y NO extender+renombrar)

`Anima` es la base de **todo** ser: jugador, compañeros, animales y lo **inanimado despertable** (roca, viento).
`Animal` (567 líneas) es una especialización pesada: `NavMeshAgent`, `Rigidbody`, `ActionsPrep`, corrutinas de
caza/huida, familia, postnatal, ciclo de vida.

- **Extender+renombrar `Animal → Anima`** forzaría esa maquinaria sobre **todo** ser (una roca con `NavMeshAgent`
  y lógica de caza). Contradice "todo es Anima + **pilares opcionales**". `SimpleAnima` existe justo para ser un
  Anima mínimo sin arrastrar `Animal`.
- **Disolver** mantiene `Anima` **magra** (drives + aptitudes + bonds + hooks) y reparte la conducta en
  **componentes** que cada ser lleva solo si le tocan.

Disolver **no** es reescribir: se **reubica** la lógica de `Animal` en componentes (se reusa el código).

## Kit objetivo (mismo para todos; cambia la data/componentes)

| Ser | Body/Mind (SoulComposition) | Locomoción | Conducta | Vida |
|---|---|---|---|---|
| **Lobo** | Wolf + Wolf | NavMesh + `WalkSpell` | `ThreatResponder` + `Predator` + `PackAwareness` | `LifeStage` + `Family` + `PostNatal` |
| **Conejo** | Bunny + Bunny | NavMesh + `WalkSpell` | `ThreatResponder` + `Forager` | `LifeStage` + `Family` |
| **Humano IA** | Human + Human | NavMesh + `WalkSpell` | (tareas/rol) | `LifeStage` |
| **Jugador** | Human + Human | `CharacterController` + `WalkSpell` | (input) | — |
| **Roca** | Rock body/mind | — | — | — |

Un lobo y un humano son **idénticos en tipo** (`SimpleAnima` + componentes); solo cambian los arquetipos y qué
componentes de conducta llevan.

## Qué sale de `Animal.cs` y a dónde

| Bloque en `Animal`/subclases | Componente destino | Cablear a stats |
|---|---|---|
| `SenseThreats`, `EvaluateThreat`, `ResolveReaction`, `Flee`/`Fight`/`HitAndRun`, `Escape` | **`ThreatResponder`** | huir/luchar por `autoabandono` + bonds + threat (`Predation`) — *iniciado* |
| `RespondToHunger`, `Feed`, `SelectPrey` (Carnivore), pastar (Herbivore) | **`Forager`** / **`Predator`** | depredación por `bodyMass`/fuerza (`Predation.EffectivePower`) |
| `nav`, `ActionsPrep`, `CorrectMedium`, `FeedWalkSpeed` | **`Locomotion`** (NavMesh) + `WalkSpell` | velocidad stat-driven (hecho, opt-in) |
| `ApplySpeciesArchetype`, `Physiognomy`, `rig.mass`, `currentMedium` | **`SpeciesBody`** | ya lee `Archetypes` (datos) |
| `EvolveAptitudes` | **`AptitudeEvolution`** (ya existe) como componente | ya stat-driven |
| `LifeStage`, `Family`, `PostNatalManager` | ya son componentes | desacoplar del tipo `Animal` |
| `Population`, `IFactory`, respawn | **`Spawner`/`Population`** | — |

## Reconciliar las dos IAs

Hoy hay dos sistemas sin unir: los **brains** (`AnimaController`/`AiBrain`/`PlayerBrain`, posesión — casi vacíos)
y las **corrutinas de `Animal`** (la IA real animal). Objetivo: `AiBrain` **conduce los componentes de conducta**
(ThreatResponder/Forager/Locomotion) y `PlayerBrain` los **posee** → una sola IA, el jugador es "solo un input"
sobre el mismo cuerpo/componentes que la IA.

## Orden por etapas (cada una un PR; `Animal` sigue vivo hasta vaciarse)

> **Reorden (2026-08-17):** al leer `Carnivore.Feed`/`Herbivore.Feed` se ve que el forrajeo es **mayoritariamente
> locomoción** (`nav.SetDestination` + `ActsPrep`) + comer, no política portable. Por eso **`Locomotion` va ANTES que
> `Forager`/`Predator`** (perseguir/pastar es locomoción). Orden nuevo: 1) ThreatResponder → 2) Locomotion →
> 3) Forager/Predator → 4) SpeciesBody/lifecycle → 5) reconciliar IA → 6) reconstruir lobo → 7) migrar y borrar.

1. **`ThreatResponder`**: extraer la respuesta a amenaza a componente; `Animal` delega en él. Paridad.
   - [x] **Política de decisión** (`ResolveReaction` → `ThreatResponder.Decide`): luchar/huir/pegar-y-correr por
     `Predation.EffectivePower` + `autoabandono` + bonds. `Animal` la auto-añade en `Init` y le pasa el contexto de
     **crías** (`defendingCubs`/`cubBond`, que aún salen de `Family`/`Group` en `Animal`). `enum Reaction` movido. (PR #94)
   - [x] **Evaluación** (`EvaluateThreat` → `ThreatResponder.Assess`): plenamente **stat-based** (ratio de
     `Predation.EffectivePower`, ya no `rig.mass`/NavMesh). Nueva escala = fracción de mi poder efectivo; `ThreatThreshold`
     (0.5 base, 0.8 oso) sigue coherente y es el knob de **recalibración en Unity**. (PR #95)
   - [x] **Defensa de crías EMERGENTE**: retirado el flag `DefendsCubs` (base + 8 overrides); la defensa sale del
     `cubBond` (vínculo) + `autoabandono` vs peligro. (PR #95)
   - [x] **Detección** (`SenseThreats`) + **alerta de proximidad** (en `Escape`): ya stat-based (usan `EvaluateThreat`/`Assess` + `alertReach` tunable, no `rig.mass`/NavMesh). (PR #96)
   - [ ] **Acciones** (`Flee`/`Fight`/`HitAndRun`, NavMesh/anim): pendientes (son locomoción + máquina de `Animal`; se mueven cuando se extraiga `Locomotion`).
   - Dirección (acordada): el **cuidado** de crías (alimentar/nido, hoy `PostNatal`) debería volverse también
     **emergente** (bonds + pack) como la defensa — se aborda al extraer ese sistema en una etapa posterior.
2. **`Locomotion`** (NavMesh + gait `ActionPrep`: `Walk`/`Run`/`Idle`/`Move`/`SetGait`/`GoTo`). **[x] Comportamientos migrados** (PR #100 semilla + #101 resto): `CorrectMedium`, `Herbivore.Feed`, `Carnivore.Feed`, `Flee`/`Fight`/`HitAndRun`, wander — ya no tocan `nav`/`ActsPrep` directamente (solo lecturas de config). *Falta:* que `AiBrain` lo conduzca (reconciliar IA) y sacar el `ActsPrep` a data de arquetipo para que sea portable.
3. **`Forager`/`Predator`** (SOBRE `Locomotion`): decidir **qué/dónde** comer → ir (Locomotion) → comer.
   - [x] **Selección de objetivo** (`Forager.SelectTarget` + `Animal.ConfigureForager`): flags **combinables** `eatsPrey`/`eatsGrass`/`eatsFish` (omnívoro = varios → la fuente más cercana); carnívoro→`Diet`, herbívoro→pasto/banco. `Carnivore`/`Herbivore.Feed` la usan. (PR #102-#103)
   - [x] **Comer** (carnívoro): `Forager.Eat(self, food, obj, biteSize)` — mordisco + nutrición (hambre + `Metabolism`) + bond con quien dejó la comida + 1ª sólida de cría. Multi-consumo soportado (`IEdible` con pool compartido). `Carnivore.Feed` lo usa. (PR #104)
   - [x] **Pastar/pescar** (herbívoro): `Herbivore.Feed` movido entero a `Forager.Graze(self)` (ir a la fuente por `Locomotion` + comer + reducir banco). `Herbivore.Feed` solo delega. (PR #105)
   - [x] **Persecución/cazar** (carnívoro): `Carnivore.Feed` movido entero a `Forager.Hunt(self)` (elegir presa + perseguir por `Locomotion` + herir + comer + llevar sobras a las crías). `Carnivore.Feed` solo delega. (PR #106)
   - **Etapa 3 COMPLETA:** `Forager` posee select (omnívoro) + `Hunt` + `Graze` + `Eat`. `Carnivore`/`Herbivore` quedan en ~18 líneas (solo `Diet`/`GrazesOnLand` → `ConfigureForager` + delegación) → el "qué come" es ya config de componente.
4. **`SpeciesBody`** + desacoplar `LifeStage`/`Family`/`PostNatal` del tipo `Animal`.
5. **Reconciliar IA**: mover el "cuándo" (decisiones) a `AiBrain`/componentes; retirar las corrutinas de `Animal`.
6. **Reconstruir un lobo** como `SimpleAnima` + componentes (prefab de prueba). Validar **paridad** con el `Animal` actual.
7. Migrar las 9 especies; **borrar** `Animal`/`Carnivore`/`Herbivore` + las clases de conducta por especie.

## Riesgos

- **No testeable aquí** (Unity en Windows). Cada etapa necesita pasada en el editor antes de la siguiente.
- **NavMesh**: la navegación se queda (el WalkSpell solo da la velocidad).
- **Balance**: al cablear a stats, revisar que velocidades/umbrales queden equivalentes.
- Mantener `Animal` funcionando hasta la etapa 6 (paridad) evita un "big bang" irreversible.
