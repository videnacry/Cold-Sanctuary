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
- **Tamaño/físico se INTERPOLA por el blend de cuerpos:** `human+bunny` → ~**1,60 m**, ágil/veloz; `human+bear`
  → ~**2,00 m**, más masa/fuerza. El truco es **calibrar los %**. Esto genera los **stats INICIALES** (la base natural).

Fórmula completa (con los `bonusPacks` de abajo):
```
aptitud[k] = blend(cuerpos).físicas[k]  +  blend(mentes).mentales[k]  +  Σ bonusPacks[k]
tono/thoughts = blend(mentes)            // los bonusPacks NO tocan esto
```
Ventaja: **componer por arquetipos es más fácil y expresivo que teclear 14 números**; y modela identidad/mezcla
de forma natural.

**Difuminado → base:** cuantos más arquetipos/% se añaden, más se **diluye** el impacto de cada uno (regresa a la
media) → se puede definir un **arquetipo "base"** (neutro) que **rellene con `shareDomain`** y tire hacia una
línea base común.

## Quién MANDA (mente activa vs body automático)
El **dominio de las Mentes** decide cuánto pesa la **decisión mental** frente a la **automatización corporal**:
- Dominante `BearMind` → manda el Body (huir/cazar de `Animal` casi sin interrupción).
- Dominante `HumanMind` → la mente **interrumpe/modula** el Body → rutinas nuevas (yoga, cuidar, planear).
Engancha con el **sistema de Control** existente (`AnimaController` + `IBrain` por *relevancia*): la mente activa
= el "cerebro" de mayor relevancia que conduce el cuerpo. *Falta:* subordinar los bucles autónomos de `Animal`
(`SenseThreats`/`Flee`) a ese mando (hoy corren siempre).

## Variedad corporal (y genética)
- **Por blend**: el propio blend de cuerpos **es** la herramienta de variación (interpola tamaño/físico según %).
- **Intra-especie** (dos osos algo distintos): **jitter genético** sobre el mismo `BearBody`, o un pequeño % de
  otro cuerpo en el blend.
- **`FamilyGenerator`/genética se actualiza** para **sembrar por blend de los padres + jitter** (herencia = mezcla
  de los arquetipos de cuerpo/mente de los progenitores), en vez de random sobre stats crudos.
- **Entre-especies / híbridos / reencarnación**: el **blend** (cuerpos y mentes) es la herramienta.

## bonusPacks: potencia por NIVEL (sin tocar personalidad)
Los arquetipos dan la **base natural**. Para **personajes mágicos / que han progresado**, se añaden **`bonusPacks`**:
vectores de stats **aditivos** (físicos + mentales) que **suben aptitudes pero NO tocan tono/thoughts**.
- **Calibrados por dificultad**: `bonusPackN` ≈ **los stats necesarios para superar el santuario N** (se obtienen
  analizando qué hace falta para ganarle al **boss final** de cada santuario). Así se **coloca** a un personaje en
  el santuario correcto con la certeza de que sobrevivirá y **representa a alguien que ha llegado hasta ahí**.
- **Ejemplos**:
  - **Irosene** = `Human 97 + Toro 1 + Mono + Gallina + Agua + Fuego` (blend natural) **+ `bonusPack2`** → se la
    coloca en el **3er santuario V1** con garantía de supervivencia.
  - **Oso** = `BearBody + BearMind` **+ `bonusPack3`** → se comporta **como oso** (solo `BearMind`) pero con
    físico/mente **altísimos**. La potencia no cambia quién es.
- **Aditivo y gestionado** (patrón *managed-delta*, como `CharacterComposition`): el pack **se suma encima** de la
  base y **no pisa** evolución/transformación; se puede **poner/quitar** (mejora mágica temporal o permanente).
- El **sistema** solo **aplica** el vector; los **valores** del pack salen del **balance** (análisis del boss).
- Encaja con `DerivedStats`: subir aptitudes → sube vida/energía/maná/poder → sobrevive santuarios más altos.

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
    public List<BlendSlot> bodies;               // arquetipos de cuerpo + %  → físicas + tamaño/velocidad
    public List<BlendSlot> minds;                // arquetipos de mente + %   → mentales + tono + thoughts
    public List<string>    bonusPacks;           // vectores de stats aditivos (NO tocan tono/thoughts)
    public void Resolve();                        // aptitudes = blend(cuerpos)+blend(mentes)+Σ bonusPacks; tono/thoughts = blend(mentes)
}
static class Archetypes {                         // perfiles en código (repo solo versiona .cs)
    static BodyArchetype Body(string name);       // Bear/Wolf/Bunny/Human… (físicas + tamaño/vel + comportamientos)
    static MindArchetype Mind(string name);       // Bear/Human/Rock/Fire… (mentales + tono + thoughts)
    static Aptitudes     Pack(string name);       // bonusPack2/3… (stats para superar el santuario N; del balance)
}
```

## Plan por fases (el propio anima-architecture avisa: "no de golpe")
1. **Blend de aptitudes + tamaño + bonusPacks** — ✅ **HECHO (PR #68)**: `BlendSlot` + `SoulComposition.Resolve()`
   + `Archetypes` (perfiles en código: Human/Bear/Wolf/Bunny/Lion/Toro/Gallina/Mono cuerpos; Human/Bear/Lion/Rock/
   Fire/Agua/Mono mentes; bonusPack1-4) → escribe las 12 aptitudes (físicas←cuerpos, mentales←mentes) + tamaño por
   mezcla, y suma los packs. Sandbox `AlmaBlend_AUTO`: Panterilia (Human 90 + Lion 5 + shareDomain), oso-mente-
   humana, oso + `bonusPack3`. **Nada migrado aún** — esto es el MOTOR; migrar a los seres reales es lo de abajo.
2. **Mente**: tono + decisiones por blend — ✅ **HECHO (PR #72)**: `SoulComposition.WriteStats` resiembra `Mind.aptitudes` con el blend → el tono/pensar (`Mind.PickTone`) emerge del blend. *Falta:* thoughts por arquetipo (va con "pensamientos por capacidad").
3. **Cuerpo**: especies → arquetipos — 🟡 **MITAD HECHA (PR #73)**: `Animal.Init` llena las aptitudes NO gestionadas por `Base*` (fuerza/masa/aguante + mentales) desde el arquetipo de especie (`SpeciesArchetype` overridado en Bear/Wolf/Fox/Bunny/Deer/Seal/Whale/Malamute) → los animales dejan de tener aptitudes planas; reusa el huir/manada de `Animal` (ya por stats). *Falta (mitad delicada):* hormigas→`Animal`, que el huir/predación lean el stat `bodyMass` en vez de `Physiognomy`/`rig.mass`, coordinar con el compañero.
4. **Mando**: dominio de mente → interrumpe el Body (subordinar `SenseThreats`/`Flee` al `IBrain`).
5. **Disolver `CompanionBase`** — ✅ **HECHO (PR #79)**: `CompanionBase` **borrado**. Su maquinaria (bond-con-jugador + mood/fatiga + anchors + proximidad) → componente `MoodState` (parametrizable, IBondable). Los 4 compañeros son ahora componentes finos (`: MonoBehaviour`) con solo su conducta propia (presión/observación/celebración/motivación) sobre `MoodState`; stats desde su arquetipo (`SoulComposition`). `BuildCharacters` recableado; `MigrationDiagnostics` arreglado. **Probar en Unity.**

## Decisiones abiertas (recomendación)
- **Arquetipo = datos en código** (no SO) — obligado por el repo. **Recomendado.**
- **Reparto**: array único por lista + `domain` + `shareDomain` (lo que pediste, "lo que facilita las cosas") en
  vez de `primary 90% / secundarios`. **Recomendado** (más flexible; "primary" = solo un slot con % alto).
- **Intra-especie**: jitter genético por defecto; blend disponible para híbridos.
