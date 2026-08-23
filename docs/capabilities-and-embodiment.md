# Capacidades = hechizos; el cuerpo = cableado (2026-08-23)

> **Fuente de verdad** del modelo emergente de **acción/percepción/temperamento**. Extiende
> [`stats-as-truth.md`](stats-as-truth.md) (la ley `stats→todo`), y cierra los huecos que salieron al disolver
> `Animal` (ver [`anima-dissolving-animal.md`](anima-dissolving-animal.md), [`behavior-system.md`](behavior-system.md)).
> Conecta con [`emotion-model.md`](emotion-model.md) (legibilidad), [`soul-composition-blend.md`](soul-composition-blend.md)
> (blend cuerpo/mente) y [`mind-model.md`](mind-model.md) (thoughts).
>
> **Estado:** diseño/dirección. Buena parte del *sustrato* ya existe (abajo se marca qué); la implementación va por
> **rebanadas** (§8).

## 0. La tesis en una frase

**El cuerpo es el cableado; una capacidad se *descubre y se afina con el uso*; leerla —propia o ajena— está
*gateado por los sentidos*; y el temperamento es un *histórico*, no un rasgo escrito.** Todo ello se expresa con
**un solo mecanismo**: un **repertorio de hechizos** (capacidades para actuar/percibir) y **thoughts** (capacidades
para interpretar), cada uno una **definición global** cuya *disponibilidad y potencia se calculan* por una **receta
sobre stats/anatomía**, no se guardan por especie.

## 1. Capacidad = hechizo

Igual que `WalkSpell`/`PullSpell` ya unificaron caminar/nadar/trepar/jalar, **toda capacidad es un hechizo**:
`Ver`, `Escuchar`, `Oler`, `Morder`, `Arañar`, `Picar`, `Jalar`, `Caminar`, `Trepar`… No hay una clase por especie
que "sepa morder"; hay **un** `Morder` global.

Tres clases de hechizo, mapeadas a los pilares **Body / Mind / Bond**:

| Clase | Actúa sobre | Coste | Ejemplos |
|---|---|---|---|
| **Corporal** | el mundo físico | reservas físicas | jalar, caminar, trepar, morder, arañar, picar, estrangular |
| **De percepción** | recoger información | atención/energía | ver, escuchar, oler (gatean la lectura de stats — §3) |
| **Mental** | lo interno/social | mental | recordar, evaluar, señalar/emocionar |

## 2. Disponibilidad y potencia se **calculan**, no se guardan

Un cuerpo **no** lleva la lista "tengo morder". Lleva **anatomía/stats** (colmillos, veneno, ojos, agilidad), y cada
hechizo tiene una **receta** que los lee:

```
potencia(hechizo, ser) = receta(hechizo) evaluada sobre los stats/anatomía del ser
disponible ⇔ potencia > umbral
```

- Sin colmillos → `Morder` potencia 0 → de facto no disponible.
- Sin ojos → `Ver` potencia 0 → el ser está *ciego* para leer a los demás (§3).

Esto da **gratis** la transformación: le pones colmillos de serpiente a un conejo (cambia la anatomía) → `Morder`
se vuelve potente. Pero **la mente todavía tiene que aprender a usarlo** (§4).

### El eje de **armamento** (el hueco del stat model)

`Predation.EffectivePower`/`Defense` hoy es **poder crudo** (masa/fuerza/manada). Falta un eje **ortogonal a la
masa**: el **arma** (colmillos/veneno/garra/aguijón/estrangulamiento/parasitismo). Por eso, en la naturaleza:

- la avispa esmeralda (*Ampulex*) vence a una cucaracha mayor con **veneno**, no con fuerza;
- la garrapata/mosquito "desafían" gigantes porque su coste es **subletal** y **no los detectan** (§3);
- el parásito que crece dentro de la rana la mata **desde dentro**.

No es "más poder": es **qué arma tienes contra qué defensa** (piedra/papel/tijera). Un arma es un **componente del
cuerpo** (encaja con `CharacterComposition`/quimeras). La depredación deja de ser una sola escala.

## 3. Leer stats está **gateado por los sentidos**

Reaccionar a los stats del otro exige **percibirlos**. Hoy `ThreatResponder.Assess` lee `EffectivePower` del enemigo
con **información perfecta**; el modelo real:

```
amenaza_percibida = Assess(real) filtrado por (mis sentidos × la legibilidad del otro)
```

- El sustrato ya existe: **`EmotionReader` gradúa la legibilidad por `perception`** (emotion-model). Se generaliza a
  toda lectura de stats.
- Una **garrapata sin ojos** lee ~0 → ataca "a ciegas": no es valiente, es que no tiene con qué medir. También cabe
  que un **bond histórico** haga ver a la presa como aliada/familiar (confianza para intentar alimentarse de ella).

### Dar un sentido **recalibra despacio**, no al instante

El caso ideal (el usuario lo señaló): un **ciego que recibe la vista**. La ciencia real —el problema de **Molyneux**;
el *Project Prakash* de Pawan Sinha (MIT)— muestra que **no** se reorganizan las respuestas de golpe: la comprensión
visual y la transferencia entre tacto y vista se construyen en **semanas/meses**. → Dar ojos a un ser **cambia sus
respuestas con el tiempo, no en el frame siguiente**. Es una **curva de reaprendizaje** (§4, §6).

## 4. Temperamento = histórico, no escalar (confianza por uso)

**El ser nace con una *posibilidad corporal*; el *uso* genera el humor.** El tiburón lanza el primer mordisco, el
resultado le favorece → **gana confianza en su arma** (física: colmillos; o intangible: una **estrategia**, como la
araña saltarina que vence a una mayor con táctica). El tiburón que sobrevivió mordiendo **aprendió** el temperamento
agresivo.

Por eso `aggressiveness` **no** es un número de especie. Se parte en:

- **Semilla innata pequeña** (del arquetipo del alma — "desde el útero"; el útero caníbal del tiburón toro es real),
- **+ confianza acumulada por resultados de uso**, que vive en `Humores`/thoughts/`SoulRecord`.

**Nature vs nurture:** un tiburón criado en agua libre, alimentado por sus padres, **organiza su energía hacia otro
temperamento**. Mismo cuerpo, historia distinta → agresividad distinta. La ferocidad es un **histórico**.

### `canHitAndRun` se **disuelve** (no se migra)

La historia de la coneja vs la serpiente (morder la cola, retroceder para arrastrar la atención lejos del nido,
volver, hasta que la serpiente cae) muestra que "pegar y correr" **no es una táctica de cuerpo ágil**: es **acoso
persistente en defensa de la cría** = la **misma lógica de manada** (jalarse hacia un hogar común) aplicada en modo
adversario. Se disuelve en dos cosas que **ya** están en `ThreatResponder.Decide`:

- `defendingCubs + cubBond` (el impulso de proteger),
- el **margen de poder**: *no puedo comprometerme a una pelea plena* → en vez de standup, **acoso** (golpe + retirada
  hacia el nido, repetido). A veces el "hit and run" **es quedarse hasta el final**: es *seguir acosando mientras el
  peligro siga vivo*, una **continuación** del loop, no una rama nueva.

→ El bool `canHitAndRun` desaparece; el acoso **emerge**.

## 5. Thoughts por el **mismo motor**

En vez de rellenar thoughts en cada mente, **calcular → id → apunta a un thought**. Es el **mismo mecanismo** que la
selección de hechizo, aplicado a la interpretación — y media pieza ya existe: `PhraseLibrary`/`PhrasePools`/
`MindPhrase` + `PhraseDistribution` + `ThoughtField`.

```
selección (hechizo o thought) = argmax_sobre_repertorio( necesidad × capacidad )   // el "id"
repertorio = definiciones globales; capacidad = receta sobre (body/mind/bond) + contexto
uso → refina la confianza (temperamento, §4)
```

Un **solo motor de "respuesta por receta"**: repertorio global + receta sobre stats/contexto → id → entrada
(hechizo **o** thought). La mente **escoge por necesidad** entre lo viable; esa selección *es* el número/código que
apunta a la capacidad concreta.

## 6. El cuerpo es el cableado (embodiment)

- **¿Las neuronas ya saben lo que el cuerpo puede hacer?** Las dos cosas: hay **programas motores innatos** (un
  tiburón cría caza sin que le enseñen — desarrollo/ADN), pero los **mapas corporales** se forman *a la par que el
  cuerpo* y **se afinan con el uso** (plasticidad dependiente de actividad). Andamiaje genético + descubrimiento por uso.
- **Los "cables" quedan aunque la parte no exista:** es el **miembro fantasma** (la representación persiste tras la
  pérdida; la corteza se remapea — Ramachandran). → Un self-model puede seguir "cableado" a un arma que ya no está.
- **Self-model:** el ser calcula en `Init` (y **refresca** al crecer/componerse/transformarse) su propio
  `EffectivePower`, a qué puede dañar y su velocidad → una **confianza base** que reemplaza al escalar plano. El
  "escaneo" es **continuo** mientras el cuerpo se forma/cambia (va en el update, con los stats).

### Cambio de cuerpo (conejo → serpiente)

Si el cuerpo ES el cableado y de él nacen los pensamientos, un swap **no** es "misma mente, avatar nuevo": es
**cableado en parte nuevo**. El conejo-serpiente aparece **retorciéndose, redescubriendo** (como el ciego que recibe
vista):

- si el salto es asumible y el reaprendizaje es lo bastante rápido → **aprende** sus capacidades;
- si el hueco es demasiado grande → **falla** (lo narramos como muerte por estrés / incapacidad de habitar el cuerpo).

Las **"neuronas mixtas"** ya tienen casa: es el **blend de arquetipos** de cuerpo+mente con dominio % de
[`soul-composition-blend.md`](soul-composition-blend.md). Un conejo-serpiente = conexiones conejo + conexiones
serpiente. Lo que le falta a ese doc es **la curva de reaprendizaje** (el uso re-afina el mapa), en vez de competencia
instantánea.

## 7. Los `Behavior` de especie: **plantillas de nacimiento**, no jerarquía

Tras disolver `Animal`, las 8 clases (`WolfBehavior`…) quedaron en `SpeciesArchetype` + `Start` + `ConfigureThreat`.
**No son jerarquía** (Carnivore/Herbivore borradas): son el **MonoBehaviour ancla** que el prefab adjunta y que dice
"esto nace siendo un lobo". El destino es dejarlas en **solo `SpeciesArchetype`** (puntero a data) cuando
`ConfigureThreat` sea también data (§4 → temperamento emergente). **No se borran** mientras los prefabs (fuera de git)
las referencien por tipo.

## 8. Plan por rebanadas

De lo más concreto/verificable a lo más profundo:

| # | Rebanada | Qué | Reusa |
|---|---|---|---|
| **A** | **Disolver `canHitAndRun`** | acoso = defensa-de-crías + margen de poder; se borra el bool | `ThreatResponder.Decide` |
| **B** | **Eje de armamento** en `Predation` | arma ⟂ masa (veneno/filo/garra/parásito); avispa>cucaracha creíble | `Predation`, componentes |
| **C** | **`Assess` gateado por sentidos** | leer stats ∝ mi percepción × su legibilidad; garrapata "ciega"; dar ojos recalibra despacio | `EmotionReader` (perception) |
| **D** | **Confianza por uso → temperamento** | `aggressiveness` = semilla innata + histórico de resultados; hechizos con receta+id; thoughts por el mismo motor | `Humores`/`Mind`/`PhraseLibrary`/`SoulRecord` |

**Recomendado:** A primero (cierra el hook pendiente, ya maduro); de las grandes, **C antes que B** (la ceguera/
legibilidad reencuadra toda la decisión y reutiliza `EmotionReader`). D es la sustancia ("¿por qué se matan?") y se
apoya en A–C.

## 9. Sustrato existente (qué ya hay)

- **Hechizos/energía:** `WalkSpell`, `PullSpell`, `Grimoire`, `powerBonus` (carga+canalización+forcejeo), energía=timer.
- **Depredación:** `Predation.EffectivePower`/`Defense`/`CanHunt` (masa/fuerza/textura/tamaño + manada por facción).
  *Falta:* el eje de armamento (§2) y el gateo sensorial (§3).
- **Amenaza:** `ThreatResponder.Assess`/`Decide` — ya por stats+bonds+autoabandono; falta disolver `aggressiveness`/
  `canHitAndRun` y gatear `Assess`.
- **Legibilidad:** `EmotionReader` (gradúa por `perception`) — el patrón para §3.
- **Química/humor:** `Humores` (transitorio) + `Constitution` (estructural) — hogar de la confianza-por-uso (§4).
- **Thoughts:** `Mind`/`PhraseLibrary`/`PhrasePools`/`MindPhrase`/`PhraseDistribution`/`ThoughtField` — media pieza de §5.
- **Blend/transformación:** `soul-composition-blend.md` + `TransformationSpell`/`CharacterComposition` — base de §6.
