# Motor de volición: la selección conduce la acción (D3b) — 2026-08-24

> **Diseño/contrato** de la rebanada **D3b** de [`capabilities-and-embodiment.md`](capabilities-and-embodiment.md) §5/§8:
> el pivote que **conecta `Mind` (que ya selecciona) con la acción** (hoy ramas fijas). El resto del modelo (A–E,
> incluidos D3a=confianza-en-el-peso y E2=gate-por-receptor) **ya está**; falta que ese motor **elija qué hacer**, no
> solo qué pensar. **No es un refactor ciego:** reutiliza el picker y los handlers de acción existentes.
>
> **Estado:** propuesta. Se implementa por rebanadas (§7), la primera **detrás de un flag** para validar PARIDAD en
> Unity antes de retirar nada (no se puede compilar aquí).

## 1. El problema: dos mundos desconectados

| | Hoy | Dónde |
|---|---|---|
| **Pensar** | motor ponderado: `PickTone` (receta por aptitudes+humores+campos) → `PickWeighted` (peso × confianza D3a) → `PassesGate` (aptitud + receptor E2). Pero **solo `Debug.Log`** | `Mind.cs` |
| **Actuar** | ramas `if` **fijas**: `if (hungry) RespondToHunger(); SenseThreats();` — prioridad hambre→amenaza, sin puntuación | `Animal.ActiveBehaveTick` |

El motor con la forma correcta ya existe para pensar; las acciones son ramas sueltas y **nadie las conecta**. `Mind`
piensa por su lado; `AiBrain.Act()` llama a `ActiveBehaveTick()` por el otro. **D3b = unificarlos:** el **deseo** (comer/
dormir/defender/…) se elige por `necesidad × capacidad × confianza` con ESE picker, y ese deseo **despacha** la acción.

## 2. Lo que se reutiliza (no se reescribe)

- **El picker** (`Mind.EffectiveWeight` + `PassesGate`) ya pondera por confianza (D3a) y filtra por aptitud/receptor (E2).
- **`PhraseCategory` ya tiene `Deseo` y `Hechizo`** (el diseño siempre quiso que la acción fuera una frase).
- **Las acciones YA son handlers/corrutinas:** `RespondToHunger`→`Feed`→`Forager.Hunt/Graze`; `SenseThreats`→`Escape`→
  `ThreatResponder.Decide`→`Flee/Fight/HitAndRun`; `WalkSpell` para el deambular. D3b **no las toca** — solo cambia
  **quién las elige y cuándo**.
- **La supresión por posesión** ya existe: `AnimaController` llama al `PlayerBrain` en vez de al `AiBrain` → el motor de
  volición se salta solo cuando el jugador conduce.

## 3. El modelo: Deseo = frase + binding

Un **Deseo** es una `MindPhrase` de categoría `Deseo` (mantiene "todo es una frase": su texto, tono, peso, `capability`,
gates) **más un binding de conducta** resuelto por un catálogo, para no meter lógica en la frase:

```
MindPhrase(Deseo) { …, desireKey = "eat" }          // la frase NOMBRA el deseo
DesireCatalog["eat"] = {
    NeedProbe(Animal) → 0..1,                        // cuánto lo quiere AHORA (de los drives)
    Dispatch(Animal)                                 // corre la acción existente (RespondToHunger/…)
}
```

- `NeedProbe` lee los **drives** ya existentes: `eat` ← `hungry`; `sleep/rest` ← fatiga/`asleep`; `defend/flee` ←
  `ThreatResponder.Assess` (ya gateado por sentidos, C); `wander` ← base baja; a futuro `socialize`←soledad/bonds,
  `careCubs`←`cubBond`+crías.
- `capability`/`gateCapability` de la frase alimentan la selección: `defend` usa `Capability.Combat` (D2/D3a) → un
  depredador con confianza en el combate **desea** más defenderse/atacar; un hechizo sensorial se gatea por su receptor.
- `Dispatch` **llama al handler que ya existe**. D3b no crea conductas nuevas; reubica la DECISIÓN.

## 4. Puntuación y selección

Por cada deseo candidato (los `Deseo` que el ser tiene, que pasan `PassesGate`):

```
score(deseo) = NeedProbe(deseo) × EffectiveWeight(frase)      // EffectiveWeight ya incluye confianza (D3a) y lifecycle
```

**Selección con piso de seguridad** (no es soft-weighting puro — la supervivencia no puede salir a la suerte):

1. Si algún deseo supera `criticalNeed` (p.ej. una amenaza real cerca) → se elige el de **mayor need** entre esos
   (determinista: no ignoras al oso por "elegir comer"). Preserva el "la amenaza se atiende cada tick" de hoy.
2. Si no → **pick ponderado** por `score` entre el resto (variedad emergente: comer/descansar/deambular/socializar).

El deseo elegido se **despacha**; su resultado **realimenta la confianza** (`RecordUse`, ya cableado para la caza en D2;
se extiende a otras capacidades) → el bucle histórico se cierra: **lo que te funciona, lo deseas más** (§5 del modelo).

## 5. Dónde vive (API)

- **`DesireCatalog`** (estático, como `Capability`): `key → { Func<Animal,float> NeedProbe; Action<Animal> Dispatch }`.
  Claves base: `eat`, `sleep`, `rest`, `defend`, `wander`. Extensible.
- **`Volition`** (componente nuevo, ligero) — el selector. `Tick(Animal self)`:
  1. guardas actuales (`death`/`asleep`/`busy` → salir; posesión ya suprime aguas arriba);
  2. reunir Deseos del ser (base + `Mind.thoughts` de categoría `Deseo`) filtrados por `PassesGate`;
  3. puntuar (§4) y elegir (piso de seguridad + pick ponderado);
  4. `Dispatch`.
- **Enganche:** `AiBrain.Act()` (animal) llama a `Volition.Tick()` **en vez de** `ActiveBehaveTick()` cuando el flag
  está activo. `ActiveBehaveTick` se conserva como fallback hasta validar paridad.
- **Deseos base por especie:** los siembra `SpeciesBody` (como ya siembra los pensamientos base) → un lobo desea
  cazar/defender; un conejo, comer-pasto/huir/socializar. Emergente por arquetipo, no por clase.

## 6. Interacciones

- **`busy` / interrupción:** una acción en curso (corrutina que marca `busy`) bloquea la re-selección, como hoy. ABIERTO:
  ¿una amenaza sobre `criticalNeed` puede **interrumpir** un `Feed` en curso? (política de interrupción — §8).
- **Ritmo:** el mismo throttle de `ActiveBehaveTick` (`TimeSpeedMinuteSecs / rand`). La latencia de reacción a una
  amenaza = ese intervalo, igual que hoy.
- **`Mind` sigue pensando:** el pilar `Mind` sigue con sus frases idle (Debug.Log) por su `thinkInterval`; `Volition`
  es la rama de ACCIÓN. Comparten picker y frases, no el temporizador.
- **Posesión:** sin cambios — `PlayerBrain` toma el mando, `Volition` no corre.

## 7. Migración por rebanadas (paridad primero; nada se borra hasta validar)

- **D3b1 — catálogo + selector tras flag (paridad).** `DesireCatalog` con `eat`/`defend`/`rest`/`wander` mapeados a los
  handlers EXISTENTES; `Volition.Tick`; `AiBrain` lo usa si `useVolition` (**default OFF**). Con el flag ON, debe
  **reproducir la conducta actual** (comer con hambre; atender amenaza). *Validar paridad en Unity.*
- **D3b2 — flip + retirar ramas.** Confirmada la paridad, `useVolition` default ON; se retira la prioridad fija de
  `ActiveBehaveTick`. Ahora la prioridad es **emergente** (need×confianza).
- **D3b3 — hechizos como acciones + activar D3a/E2 en vivo.** Frases de categoría `Hechizo` (Morder/Ver…) seleccionables,
  con `capability` (confianza, D3a) y `gateCapability` (receptor, E2) ya **en uso** — despiertan los mecanismos dormidos.
- **D-cola (independiente):** productor de FIGHT para la confianza; **decaimiento pasivo** de `spellConfidence` sin uso.

## 8. Riesgos y preguntas abiertas

- **No compilable aquí** → D3b1 obligatoriamente tras flag + validación de paridad en Unity antes de D3b2. Es el mayor
  cambio de conducta del arco; no hacer big-bang.
- **Calibración de `NeedProbe`:** fórmulas exactas (hambre desde `hungry`; sueño desde qué drive; amenaza desde `Assess`)
  y `criticalNeed` → tuning en editor.
- **Ponderado vs determinista:** el piso de seguridad protege la supervivencia; el resto es emergente. ¿Dónde va el corte?
- **Interrupción de `busy`** por amenaza crítica: ¿se permite? (realismo vs. corrutinas a medias).
- **Deseos base:** ¿lista por arquetipo en `SpeciesBody`, o todos comparten un set y los drives los diferencian?
- **`Volition` vs `ActiveBehaveTick`:** el destino es que `Volition` **sea** el cuerpo de la conducta activa; el flag es
  solo el puente de transición.
