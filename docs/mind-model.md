# Modelo de mente / IA de personajes (emergencia social)

Diseño (2026-07-25). Cómo hacer que **todos los personajes se desarrollen por su cuenta** para que cada
partida sea distinta (efecto mariposa), **sin diálogos gigantes escritos a mano** y **sin ser inviable**
con decenas de agentes. Conecta con: [`review-checklist.md`](review-checklist.md) (§NPCBase, §mente
escalable), [`creature-stats.md`](creature-stats.md) (aptitudes, margas del alma).

## 0. Objetivo
- Mundo abierto: el jugador elige a qué áreas dar importancia; los personajes tienen **caminos propios**
  (uno a la cocina, otro a la guardería, otro al médico → luego otra misión / descansar / comer).
- **Domino / efecto mariposa:** una acción del jugador afecta a alguien y al mundo en grados distintos
  (el bond es el caso básico). Con el tiempo, las partidas **divergen**.
- **Priors autorados** (pasado/presente de cada personaje) = la **forma inicial**; la simulación la
  **moldea**, no la reemplaza.

## 1. Lo que YA existe (construir encima, no desde cero)
- **Rutina** — `WorldCharacter` corre un *task loop* autónomo (área → tareas → efectos a stats),
  con nivel de progresión y stats (strength/satisfaction/observation/mentalFatigue/stress/velocity).
- **Priors** — `ThoughtAnchor` (clave + peso −1..1 que modula decisiones; se ablanda por arcos).
- **Orquestación** — `SanctuaryDirector` (coloca personajes por área, reuniones de grupo).
- **Relaciones (base)** — `LivingEntity.bonds`/`GrowBond`, `CompanionBase.bondWithPlayer`.
- **Aptitudes + margas del alma** — `IAptitudes` + `CharacterLevel` (valores para la ecuación).

## 1.5 Arquitectura: UNA sola clase + `Mind` opcional (decisión 2026-07-25)

Se **descarta `NPCBase` como clase intermedia** (era del diseño viejo, mente binaria). Como la mente es
**escalable por tiers**, basta:

- **`LivingEntity` = la única clase de ser vivo** (ya es hogar de las 12 aptitudes vía `IAptitudes`).
  Implementa/expone `IBody` + `IMind` según haga falta.
- **La mente vive en un componente `Mind` OPCIONAL** que cualquier `LivingEntity` puede llevar: con
  componente = tiene mente (tier según capacidad); **sin componente = tier 0, gratis**. Los animales
  simples no lo llevan.
- **La mente se puede empezar YA**, sin migración previa: como `IAptitudes` es universal, el componente
  `Mind` lee aptitudes/bonds de cualquier personaje. Lo pendiente (unificar `PlayerStats`/`CompanionBase`
  bajo `LivingEntity`) es la migración de siempre, pero **ya no hace falta una clase `NPCBase`** y **no
  bloquea** empezar la mente.

## 2. El modelo — capas (todas baratas y componibles)

1. **Rutina (agenda)** — dónde va por defecto. Ya existe (`WorldCharacter`). FSM/agenda simple.
2. **Decisión por UTILIDAD** — cuando hay que elegir (tarea terminada, interrumpido, alguien cerca):
   cada acción/respuesta candidata recibe una **puntuación** = f(aptitudes, niveles de marga, mood,
   pensamientos activos, relaciones, anchors, contexto del área). Se elige el top (o **top-k con azar**
   para variedad). **Esta es "la ecuación"** que preguntas (§4).
3. **Pensamientos (memoria acotada)** — eventos (mundo, social, **y las propias acciones recientes**)
   generan *pensamientos* que entran en un buffer pequeño (§5). Alimentan mood y los pesos de utilidad.
4. **Campo social / semántico** — las áreas/objetos emiten *conceptos base*; los presentes los
   **transforman** según su estado → pueden emitir un **mensaje** y **sembrar** pensamientos en sí mismos
   o en otros (§6). Event-driven.
5. **Relaciones (bond)** — valor por par, crece por proximidad / misiones juntas / interacciones; modula
   a quién hablar/ayudar y qué mensaje sale.
6. **Tiers de profundidad** — cuánta mente corre escala con la **capacidad** (memory/reasoning o un tier
   explícito): Tier 0 (animal simple) = solo rutina o nada; Tier alto (personaje) = todo. Presupuesto
   8–12 mentes completas por zona activa (ver `review-checklist`).

## 3. La "ecuación" (utility scoring) — sí, es viable y barato
Formalmente es un **Utility AI** (p.ej. *Infinite Axis Utility System*): cada candidato se puntúa como
combinación de **consideraciones**, cada una una curva sobre una variable:

```
score(accion) = Σ_i  peso_i · curva_i(variable_i)
  variables: aptitudes (reasoning, sociability, composure…), niveles de marga,
             mood, intensidad de pensamientos con tag relevante, bond con el objetivo,
             anchors (p.ej. yoga_skepticism), necesidades (hambre/fatiga), contexto del área.
```
- **Aptitudes y margas = pesos/curvas** en esa suma (un personaje con alta `sociability` puntúa más las
  acciones de "conectar"; alto `reasoning` → transforma un concepto en pregunta; etc.).
- Elegir con **algo de azar ponderado** da la sensación de independencia (no siempre lo mismo).
- Coste: O(candidatos × consideraciones) — decenas de multiplicaciones **por decisión** (no por frame).

## 4. Pensamientos (memoria) — estructura y límites
Un **pensamiento** ≈ `{ concepto/tag, valencia (−1..1), intensidad (0..1), fuente, decay }`.
- **Capacidad acotada** por tier/`memory` (p.ej. 3–8). Al llenarse, evicta el más débil/viejo (ring
  buffer). Baratísimo y suficiente para "impresión de inteligencia".
- **El "pasado" incluye lo inmediato:** las **acciones recientes** generan pensamientos (hice yoga →
  pensamiento "calma"; el jugador me ayudó → "gratitud"), que entran al mismo buffer y sesgan la
  siguiente decisión. El "pasado profundo" (biografía) se modela como **anchors** (priors estables), no
  como miles de pensamientos guardados.
- Los pensamientos **decaen**; el **mood** = suma ponderada de los activos (modelo RimWorld/Dwarf
  Fortress, §7).

## 5. Campo social / semántico (tu idea) — cómo lo haría
- Cada **área/objeto** tiene **conceptos base** (el árbol sagrado → "árbol"; la enfermería → "cuidado").
  Es un **campo de influencia semántica**: se emite cuando hay personajes reunidos / periódicamente,
  con intensidad que **decae** con distancia/tiempo.
- Cada agente presente **transforma** el concepto según su estado (utilidad, §3): en **pregunta** (alto
  reasoning/curiosidad), **adoración** (anchor/valores), **nada** (mente enfocada/fatiga alta),
  **conectar** (alta sociability + mood alto → pregunta a otro por su estado).
- La transformación puede **emitir un mensaje** (§6) y **sembrar** un pensamiento en sí mismo o, si
  conecta, en el interlocutor → propagación tipo dominó, **acotada** por decay/capacidad.
- **Event-driven y por-zona-activa** → no es un coste global por frame.

## 6. Diálogo emergente sin árboles gigantes
El mensaje **no está escrito entero**: se **compone** de plantillas cortas indexadas por
`(tag del pensamiento, mood, relación, aptitud dominante)`. Pocas plantillas × muchas combinaciones =
variedad. Ejemplo: tag "árbol" + mood alto + sociability alta + bond medio → *"¿No sientes algo raro
cerca del árbol? …"*. Es cómo los sims sociales fingen conversación barata.

## 7. Modelos existentes — qué reutilizar (y qué evitar)
| Modelo | Para qué sirve aquí | ¿Usar? |
|---|---|---|
| **Utility AI** (The Sims; IAUS de Dave Mark) | La "ecuación" de decisión (§3) | ✅ base |
| **Thought/mood memory** (RimWorld, Dwarf Fortress) | Pensamientos que decaen → mood (§4) | ✅ reutilizar patrón |
| **Influence maps** (IA táctica clásica) | El campo social/semántico (§5) | ✅ repurposear |
| **Behavior Trees / FSM / agenda** | La rutina (ya en `WorldCharacter`) | ✅ ya |
| **GOAP / HTN** (planificación, F.E.A.R.) | Planes de varios pasos | ⚠️ caro; solo si hace falta |
| **Social sim académico** (Versu, Ceptre, "social practices") | Inspiración para reglas sociales | 📖 referencia |
| **Red neuronal / LLM por NPC en runtime** | "IA general" | ❌ caro + no determinista + difícil de controlar con decenas de agentes. (Útil OFFLINE para redactar plantillas, no en runtime.) |

**Respuesta a "¿modelo propio?":** sí. Lo **eficiente y controlable** es un **modelo propio** que
**componga** Utility AI + memoria-de-pensamientos + campo-de-influencia + agenda, alimentado por tus
datos (aptitudes/margas/anchors/bonds). No es "menos inteligente" que una IA general para este caso: es
más barato, determinista-cuando-quieres y afinable. La "impresión de inteligencia" sale de la
**combinatoria** (pensamientos × mood × relaciones × aptitudes × plantillas), no de cómputo caro.

## 8. Eficiencia (por qué es viable con decenas de agentes)
- **Tiers + presupuesto por zona activa** (8–12 mentes completas; el resto rutina o congelado).
- **Buffers de pensamientos diminutos** (3–8) con eviction.
- **Decisiones event-driven** (al terminar tarea / acercarse alguien), **no por frame**.
- **Campo social por eventos** (al reunirse), con decay.
- Todo es **aritmética + tablas + plantillas**, sin NN por agente.

## 9. Efecto mariposa — cómo diverge sin volverse ruido
- Estado persistente (pensamientos, relaciones, mundo) + priors autorados (anchors) → una acción del
  jugador cambia utilidades → cambia adónde va / qué dice A → siembra pensamiento en B → …
- **Anclado por los priors**: la biografía (pasado/presente que curas) son pesos fuertes que la
  emergencia **perturba** pero no borra → divergencia **con sentido**, no caos.

## 10. Preguntas abiertas / números a decidir
- Capacidad de pensamientos por tier; tasa de decay; nº de consideraciones por decisión.
- Presupuesto de tick (cuántas mentes completas por zona; cada cuánto deciden).
- Set inicial de **conceptos base** por área y de **plantillas** de mensaje.
- ¿`ThoughtAnchor` se generaliza a todo `NPCBase` (no solo companions)?
- Grado de azar en la elección (determinismo vs variedad).
- Encaje con `NPCBase` (dónde viven pensamientos/relaciones/mente): implementar junto a la migración.
