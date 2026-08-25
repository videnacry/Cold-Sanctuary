# Navegación por lectura del entorno (impulsos) — 2026-08-25

> **Diseño.** Unificar la navegación: el overworld (`Animal`) deja de "seguir aves al azar" y pasa a **leer el entorno
> con los sentidos** y dirigirse a lo propicio / lejos de lo peligroso, como ya hacen las hormigas del Microcosmos.
> Reutiliza el sistema de **impulsos** que ya existe (`ImpulseController` + emisores/escáneres); añade las señales que
> faltan (confort/humedad, colegueo, experiencia/memoria) y el engaño. Conecta con sentidos (C), legibilidad
> (`EmotionReader`), medio/afinidad (asfixia) y deseos (D3b: el deseo dice QUÉ busca; esto resuelve DÓNDE).

## 1. Dos mundos de navegación hoy

| | Cómo se mueve | Dónde |
|---|---|---|
| **Microcosmos (hormigas)** | **impulsos**: `ImpulseController` suma vectores ponderados (hogar/miedo/olor/grupo) → NavMeshAgent | `ImpulseController`, `HomeImpulse`, `Scent/ThreatScanner`, `BondEscapeReader` |
| **Overworld (`Animal`)** | `LifeStage.Wander` → `target = ave al azar`; `Flee` → huye hacia un ave al azar | `LifeStage.Wander`, `Animal.Flee` |

El sistema bueno (impulsos leyendo el entorno) **ya existe**; el overworld no lo usa. **La cueva de Ambrosio se logra
así** (`HomeImpulse` a la cueva ×estrés×bond + `BondEscapeReader` + escáneres). Objetivo: que el overworld navegue igual.

## 2. El modelo: percibir → impulsos → dirección

Cada ser suma **impulsos** (atractores/repulsores) que salen de **leer el entorno con sus sentidos**. El vector neto lo
dirige. Un impulso = { dirección, peso, tag, decaimiento }. La **lectura está gateada por sentidos** (C) y por
**legibilidad** del emisor (`EmotionReader`): sin olfato no hueles la comida; sin vista no lees al congénere relajado.

### Tus señales → impulsos

| Señal (tu ejemplo) | Impulso | Estado |
|---|---|---|
| **Comida** (olor) | atractor al `ScentEmitter` de comida/cadáver (vía `ScentScanner`) | **existe** |
| **Refugio / hogar** | `HomeImpulse` (×estrés×bond de grupo) | **existe** |
| **Inexistencia de indicios de depredador** | ausencia de impulso `ThreatScanner` (peligro contextual) → no repulsión = zona apetecible | **existe** |
| **Humedad / clima** (el conejo se guía por la humedad) | atractor a un **campo de CONFORT** por medio/microclima (afinidad de medio, cf. asfixia) | **nuevo** (`ComfortField`/emisor) |
| **Colegueo** (percibir animales que no le dañan, **relajados y a la vista** = "aquí hay comida/refugio y no hay depredador") | atractor a **congéneres/inofensivos CALMADOS** (baja activación, no huyen ni se ocultan) — lo INVERSO de `BondEscapeReader` | **nuevo** (`SafetyReader`; usa `EmotionReader`/legibilidad) |
| **Experiencia** (no volver por donde vino, salvo que ahí vio lo que ahora busca) | **memoria de lugares**: repulsor leve de sitios recién visitados vacíos + atractor a donde vio el recurso buscado | **nuevo** (memoria espacial ligera; cf. `SoulRecord`) |
| **Engaño por cambios bruscos** | los emisores pueden **mentir/cambiar** (un olor que aparece de golpe, una trampa que imita una señal de seguridad) → el escáner reacciona a la señal, no a la verdad | **emergente** (extensión de emisores; "trampas") |

**Colegueo, en detalle:** un animal **relajado y a la vista** (activación baja, sin `aware`/flee, sin ocultarse) es una
**baliza de seguridad** para otros de nivel similar: "si ese come tranquilo aquí, no hay depredador y hay recurso". Se lee
con `EmotionReader` (legibilidad × mi percepción): un ser en guardia/oculto no emite colegueo; uno tumbado al sol, sí. Es
el gemelo positivo de `BondEscapeReader` (que lee la huida). También modela el **cebo**: un depredador que finge calma.

## 3. Cómo encaja con lo demás

- **Deseos (D3b):** el deseo elegido (`eat`/`defend`/buscar‑refugio) fija **qué** busca; la navegación por impulsos
  resuelve **dónde ir** (qué emisores atraen). El deseo puede **ponderar** los impulsos (con hambre, el olor de comida
  pesa más; con miedo, el hogar).
- **Sentidos (C) + legibilidad:** cada lectura se gradúa por mi percepción × la legibilidad del emisor. Sin el sentido,
  la señal no existe para mí (la garrapata ciega no lee el colegueo visual).
- **Medio/afinidad (asfixia):** el campo de confort tira hacia el microclima que mi afinidad prefiere; refuerza a
  `CorrectMedium` (que hoy solo reacciona) con **evitación proactiva** (no meterse en el mal medio de entrada).
- **Manada/bond:** `HomeImpulse`×bond y `BondEscapeReader` ya lo hacen; el colegueo lo extiende a NO‑miembros inofensivos.

## 4. Plan por rebanadas

- **N0 — quitar el sinsentido:** `LifeStage.Wander` deja de apuntar a un ave al azar; deambula a un punto local
  (alrededor de su posición/hogar). Stopgap pequeño y honesto hasta N1.
- **N1 — impulsos en el overworld:** `Animal` puede llevar `ImpulseController`; `Wander`/`Flee` pasan a leer el neto de
  impulsos en vez del ave al azar. Reutiliza `HomeImpulse`/`Scent`/`ThreatScanner` tal cual. (El overworld ya es NavMesh,
  como `ImpulseController`.)
- **N2 — campo de CONFORT (humedad/medio):** emisor de confort por microclima; impulso hacia la afinidad preferida →
  evitación proactiva del mal medio (cierra el tema de la asfixia por el lado de "no entrar").
- **N3 — COLEGUEO (`SafetyReader`):** atractor a inofensivos calmados/visibles (inverso de `BondEscapeReader`, vía
  `EmotionReader`). Incluye el cebo (calma fingida).
- **N4 — EXPERIENCIA (memoria de lugares):** repulsor de sitios visitados‑vacíos + atractor a donde vio el recurso;
  "no volver por el mismo camino salvo que ahí esté lo que busco".
- **N5 — ENGAÑO:** emisores que mienten / cambian de golpe (trampas, cebos, señales que desaparecen) → el escáner puede
  ser engañado; la percepción/experiencia altas lo mitigan.

## 5. Riesgos / abierto

- **No compilable aquí** → cada rebanada tras validación; N1 (grafting de `ImpulseController` al `Animal`) es la de más
  integración (convive con `WalkSpell`/`LifeStage`; decidir quién manda el `NavMeshAgent`).
- **Coexistencia con LifeStage:** `Wander` es evento rítmico (§ volition-selection-engine §6.1). La navegación por
  impulsos es el "cómo" del wander/flee, no un segundo scheduler.
- **Rendimiento:** escáneres por radio; a gran escala, partición espacial (ya anotado en `SenseThreats`).
- **Calibración:** pesos de cada impulso, umbral de "calma" para el colegueo, tamaño de la memoria de lugares.
