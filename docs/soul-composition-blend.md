# Alma por MEZCLA: cuerpos y mentes con dominio (%)

Diseño (2026-08-13). Modelo para que **cada ser se componga por mezcla de arquetipos** (mentes y cuerpos) en
vez de configurar stats a mano. Generaliza lo ya escrito en [`anima-architecture.md`](anima-architecture.md)
(las "madres con pesos": *piedra con `madre-roca=2`, `madre-Magnate=0`*; *roca "tierra+fuego mitad y mitad"*) →
aquí **cualquier** arquetipo (mente o cuerpo) es una fuente con **peso de dominio**, y el ser final = **suma
ponderada**.

## La idea
Un ser es un `Anima` (raíz) cuya **composición** son dos listas:
- **Cuerpos** (`BodyArchetype`): lo **físico** — tamaño del GameObject, **peso** (`bodyMass`), **velocidad**,
  **fuerza**, agilidad, resistencia, percepción — **y la automatización corporal** (huir/cazar/pastar/manada de
  `Animal`). Bear/Wolf/Bunny/Human… definen "lo característico físicamente" de una especie.
- **Mentes** (`MindArchetype`): lo **mental** — aptitudes mentales (razón/memoria/creatividad/sociabilidad/
  disciplina/compostura), **tono elemental** y **thoughts** (pools de `PhraseLibrary`). Bear/Human/Rock/Fire…
  definen "cómo piensa/decide".

Cada arquetipo entra con un **peso de dominio**. El ser **computa** sus aptitudes/tono/thoughts finales como
**mezcla ponderada** de sus arquetipos. Así:
- **Panterilia** = `HumanMind 90` + `LionMind 5` + (otras en `shareDomain`) → 90% persona con un rastro felino.
- **Oso con mente humana** = `BearBody 100` + `HumanMind 90`/`BearMind 10` → cuerpo de oso que **se comporta
  como humano** → puede aprender **yoga/meditación** → stats que crecen de forma **única** vs otros osos.
- **Reencarnación / transformación** = **añadir/quitar** arquetipos: persona-que-fue-lobo = `HumanBody` +
  `HumanMind 80`/`WolfMind 20`; el rastro lobo asoma como **tell** (postura/emoción). Cambia con el tiempo.
- **Roca que piensa** = solo `RockMind` (sin cuerpo con valores > 0) → barata, se le puede hablar, no hace asanas.

## El dominio (%) y `shareDomain`
Cada slot: `{ archetype, domain (0–100), shareDomain (bool) }`, en dos arrays (mentes y cuerpos).
- **Explícitos primero:** se suman los `domain` de los slots con valor. 
- **`shareDomain`:** los marcados se **reparten uniformemente lo que quede sin reclamar** (100 − suma explícita).
- Ejemplos: uno a 100 y el resto a 0 (puro); o 80/20; o `Human 90` + varios `shareDomain` que se reparten el 10.
- **Normalización:** si los explícitos pasan de 100, se re-escala a 100 (o se avisa). Si suman <100 y no hay
  `shareDomain`, el resto se ignora (el ser es "menos", coherente con "limitado por stats").

## Cómo se computa (blend)
Para cada aptitud/canal:
```
final[k] = Σ_slots ( peso_slot_normalizado × archetype_slot.valor[k] )
```
- **Físicas** (agility/strength/bodyMass/endurance/perception + tamaño/velocidad) ← blend de **Cuerpos**.
- **Mentales** (reasoning/memory/creativity/sociability/discipline/composure) ← blend de **Mentes**.
- **Tono elemental** ← blend ponderado de los tonos de las Mentes → "persona con Fuego predominante", etc.
- **Thoughts** ← unión ponderada de los pools de frases de las Mentes (un lobo-persona suelta a veces ideas de lobo).
- **afabilidad/sensibilidad** ← mente (temperamento).
Ventaja: **componer por arquetipos es más fácil y expresivo que teclear 14 números**; y modela identidad/mezcla
de forma natural.

## Quién MANDA (mente activa vs body automático)
El **dominio de las Mentes** decide cuánto pesa la **decisión mental** frente a la **automatización corporal**:
- Dominante `BearMind` → manda el Body (huir/cazar de `Animal` casi sin interrupción).
- Dominante `HumanMind` → la mente **interrumpe/modula** el Body → rutinas nuevas (yoga, cuidar, planear).
Engancha con el **sistema de Control** existente (`AnimaController` + `IBrain` por *relevancia*): la mente activa
= el "cerebro" de mayor relevancia que conduce el cuerpo. *Falta:* subordinar los bucles autónomos de `Animal`
(`SenseThreats`/`Flee`) a ese mando (hoy corren siempre).

## Variedad corporal
- **Intra-especie** (dos osos algo distintos): mejor por **jitter genético** sobre el mismo `BearBody`
  (semilla: `FamilyGenerator`), o un pequeño % de otro cuerpo en el blend.
- **Entre-especies / híbridos / reencarnación**: el **blend de cuerpos** (y mentes) es la herramienta.

## Reconciliación con lo que hay (y qué falta)
- **`Anima`** raíz + 12 aptitudes fusionadas: ✅ es donde se **escriben** los stats finales del blend.
- **Especies `Bear/Wolf : Animal`**: hoy la automatización corporal está por **herencia**. Migración: que cada
  especie **exponga un `BodyArchetype`** (perfil físico + comportamientos) reutilizable por el blend; a plazo,
  `Animal` = el **pilar Body**. Las hormigas serían cuerpos que **reusan** el huir/manada de `Animal` (no un
  `SimpleAnima` que lo reimplementa).
- **`Mind`** ya es componente (tono + thoughts): se factoriza en **`MindArchetype`s** mezclables.
- **`CompanionBase`** se **disuelve**: Panterilia = un `Anima` con su blend (no una clase compañera).
- **Restricción del repo:** solo se versiona `.cs` → los **arquetipos viven en CÓDIGO** (perfiles estáticos o
  serializables), **no** en ScriptableObjects/.asset (que no se versionan).

## Modelo de datos propuesto
```
[Serializable] class BlendSlot { public string archetype; [Range(0,100)] public float domain; public bool shareDomain; }
class SoulComposition : MonoBehaviour {          // sobre el Anima
    public List<BlendSlot> bodies;               // arquetipos de cuerpo + %
    public List<BlendSlot> minds;                // arquetipos de mente + %
    public void Resolve();                        // computa el blend → escribe aptitudes/tono en el Anima
}
static class Archetypes {                         // perfiles en código (repo solo versiona .cs)
    static BodyArchetype Body(string name);       // Bear/Wolf/Bunny/Human… (físicas + tamaño/vel + comportamientos)
    static MindArchetype Mind(string name);       // Bear/Human/Rock/Fire… (mentales + tono + thoughts)
}
```

## Plan por fases (el propio anima-architecture avisa: "no de golpe")
1. **Blend de aptitudes** (lo más barato/valioso): `BlendSlot` + `SoulComposition.Resolve()` + `Archetypes` con
   unos pocos perfiles (Human/Bear/Bunny/Lion + Rock/Fire) → escribe las 12 aptitudes por mezcla. Sandbox
   demostrando Panterilia (Human 90 + Lion 5 + shareDomain) y "oso con mente humana".
2. **Mente**: tono + thoughts por blend (une pools de `PhraseLibrary`).
3. **Cuerpo**: especies exponen `BodyArchetype`; el blend elige tamaño/velocidad; automatización corporal reusada.
4. **Mando**: dominio de mente → interrumpe el Body (subordinar `SenseThreats`/`Flee` al `IBrain`).
5. **Disolver `CompanionBase`**; reencarnación/transformación = añadir/quitar arquetipos.

## Decisiones abiertas (recomendación)
- **Arquetipo = datos en código** (no SO) — obligado por el repo. **Recomendado.**
- **Reparto**: array único por lista + `domain` + `shareDomain` (lo que pediste, "lo que facilita las cosas") en
  vez de `primary 90% / secundarios`. **Recomendado** (más flexible; "primary" = solo un slot con % alto).
- **Intra-especie**: jitter genético por defecto; blend disponible para híbridos.
