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

> **Hecho (B, PR #130):** campo `Anima.armament` (⟂ masa, default 0 → sin cambio de balance); `Predation.PredatorPower
> += armament × ArmamentPower`. Como el poder efectivo alimenta la percepción de amenaza (`EffectivePower`→`Assess`),
> el pequeño-pero-armado **caza Y es temido** con un solo cambio. *Falta:* el **matchup tipado** (veneno vs quitina,
> garra vs coraza) — hoy el arma es un escalar; la piedra/papel/tijera real es un hook.

### Las capacidades salen de **bodyParts** (`armament`/`perception` son el sustrato interino)

`armament` (§2) y `perception` (§3) son hoy **escalares** en `Anima`, pero el end-state es que los **escriba la
anatomía**: un **colmillo/glándula de veneno** sube `armament`, un **ojo** la `perception`, un **oído/antena** habilita
otra modalidad. Una parte del cuerpo (`CompositionPart`) hace **tres cosas a la vez**:

1. **escribe stats** (su `StatBonus`) — ya funciona para `perception`/`armor`; **falta añadir `armament` a `StatBonus`**;
2. **habilita el hechizo / es el RECEPTOR** — tener ojo = existe el receptor de `Ver`; tener aguijón = `Picar`
   disponible. Un hechizo sensorial comprueba **"¿tengo un receptor de esta modalidad?"**, no solo un escalar de
   percepción (un ser sin ojos pero con antena "ve" por otra vía). *Falta:* un tag de **capacidad/receptor** en
   `CompositionPart`;
3. **compone quimeras** — es el mismo sistema `CharacterComposition`/`CompositionPart` (stats-as-truth §5).

Así, crear el colmillo/veneno/ojo como partes **unifica** arma + sentido + quimera + desbloqueo-de-hechizo en un solo
mecanismo (la **rebanada E**, §8). La `perception` sigue graduando **cuán bien** lee; el receptor decide **si** puede.

> **Hecho (E1, PR #133):** el punto 1 para el ARMA. `StatBonus.armament` + su aplicación por el delta gestionado de
> `CharacterComposition` (modulada por la vitalidad del huésped, como lo biológico) → un **colmillo/veneno como
> `CompositionPart`** sube el `Anima.armament` que `Predation` lee. Cierra B desde la anatomía: el arma ya es una PARTE,
> no un escalar suelto.
>
> **Hecho (E2, PR #134):** el punto 2 —el RECEPTOR. `CompositionPart.grants` (ojo→`"see"`, colmillo→`"bite"`) +
> `CharacterComposition.Grants(cap)`; una frase que fije `MindPhrase.gateCapability` solo pasa `Mind.PassesGate` si el
> ser lo concede. Es "la forma de verificar tener receptor visual/auditivo". DORMIDO hasta que existan las frases-hechizo
> (D3b); mientras, ninguna frase lo fija → sin efecto. La `perception` sigue graduando *cuán bien*; el receptor, *si*.

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

### La confianza es un **bond hacia el hechizo** (mecanismo de D)

Ya hay bonds hacia **seres** (`Bond` por `ITarget`, 0–100, crece con `GrowBond`) y hacia **especies**
(`speciesBonds`, relaciones/karma). La **confianza en una capacidad** es **el mismo mecanismo indexado por hechizo**:
un valor 0–100 por hechizo/arma que **crece con el uso exitoso** (el tiburón muerde, le sale bien → sube la confianza
en `Morder`) y **decae** sin uso o tras fracasos. Reutiliza `GrowBond`:

- La **selección** de la mente (§5) pondera por esta confianza: `necesidad × capacidad × confianza(hechizo)` — un ser
  se atreve más con lo que *sabe que le funciona* (colmillos, o una **estrategia** intangible, como la araña saltarina).
- La **agresividad efectiva** = semilla innata + **la confianza en las armas de daño** que tengo. El tiburón "aprende"
  la agresividad = acumula bond con `Morder`; el conejo nunca lo hace (su `Morder` no le da resultados).
- Encaja con la transformación (§6): un conejo con colmillos de serpiente tiene el arma (potencia > 0) pero **confianza
  0** → debe **ganarla usándola** (la curva de reaprendizaje). El arma es *posibilidad*; el bond‑hacia‑el‑hechizo es
  *maestría*.

> **Hecho (D1, PR #131):** infra en `Anima` — `spellConfidence` (dict nombre→0–100), `Confidence(spell)` y
> `RecordUse(spell, success, amount)` (éxito refuerza —más rápido de joven, vía `EffectiveBondGrowthRate`— / fracaso
> merma). Es el sustrato del bond‑hacia‑el‑hechizo.
>
> **Hecho (D2, PR #132):** primer bucle productor→consumidor. **Productor:** `Forager.Hunt` → `RecordUse(Capability.Combat,
> maté?)` solo si la presa estaba VIVA (no carroña). **Consumidor:** `ThreatResponder.Decide` usa **agresividad efectiva
> = `aggressiveness` innata + `Confidence(Combat)/100 × peso`** para el gate de atacar. Confianza 0 al nacer → conducta
> idéntica; sube con la caza exitosa → el depredador se envalentona con el tiempo, el herbívoro (que no caza) nunca.
> *Falta:* productor de FIGHT (ganar/perder enfrentamientos), decaimiento pasivo, y la **selección** ponderada (D3).

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

> **Hecho (D3a + E2, PR #134):** el motor de selección de thoughts de `Mind` ya sabe (1) **ponderar por confianza** —
> `EffectiveWeight` multiplica por `Confidence(capability)` si la frase declara una `capability` (D3a) — y (2) **gatear
> por receptor** — `PassesGate` exige `CharacterComposition.Grants(gateCapability)` si la frase lo pide (E2). Es el
> mismo `Mind` que ya elige por `PickTone`/`PickWeighted`/`PassesGate`. **Dormido** hasta que se creen las frases de
> categoría `Hechizo`/`Deseo` que fijen esas claves. *Falta (D3b):* que la selección **conduzca la acción** (hoy `Mind`
> solo loguea; las acciones son ramas fijas en `ActiveBehaveTick`).

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
| **A ✔** | **Disolver `canHitAndRun`** (PR #128) | acoso = defensa-de-crías + margen de poder; borrado el bool + 7 asignaciones por especie. Fight vs acoso: `myPower > enemyPower × fightPowerMargin ? Fight : HitAndRun` | `ThreatResponder.Decide` |
| **B ✔** | **Eje de armamento** en `Predation` (PR #130) | campo `Anima.armament` (⟂ masa, default 0 = sin cambio); `PredatorPower += armament × ArmamentPower`. Un pequeño-pero-armado (avispa) caza y ES temido (propaga por `EffectivePower`→`Assess`). Matchup tipado (veneno vs quitina) = hook | `Predation`, `CharacterComposition` |
| **C ✔** | **`Assess` gateado por sentidos** (PR #129) | `threat = Lerp(unawareThreat, real, clarity)`, `clarity = Clamp01(percepción/perceptionForFullRead) × legibilidad`. Fauna actual (percepción ≥1) → clarity 1 → sin cambio de balance; percepción baja (garrapata/ciego) → no percibe el peligro. Bond se aplica DESPUÉS (memoria). Legibilidad = hook (1f; a futuro tamaño/quietud/camuflaje) | `EmotionReader` (perception) |
| **D1 ✔** | **Infra de confianza-por-hechizo** (PR #131) | `Anima.spellConfidence` + `Confidence`/`RecordUse` (el bond-hacia-el-hechizo) | `bonds`/`EffectiveBondGrowthRate` |
| **D2 ✔** | **Productores + consumidores** de confianza (PR #132) | `Forager.Hunt` registra `RecordUse(Combat, maté?)` (solo presa VIVA, no carroña); `ThreatResponder.Decide` usa **agresividad efectiva = innata + confianza(Combat)**. Confianza 0 al nacer → sin cambio; sube con la caza exitosa (el depredador se vuelve osado, el herbívoro no) | `Forager`/`ThreatResponder` |
| **D3a ✔** | **Confianza en el peso de selección** (PR #134) | `Mind.EffectiveWeight` pondera una frase-CAPACIDAD por `Confidence(capability)` (piso `minConfidenceFactor` para poder probar lo nuevo). `MindPhrase.capability` opcional; sin clave → sin cambio. Es el puente D2→motor | `Mind`/`MindPhrase` |
| **D3b** | **La selección CONDUCE la acción** | que el deseo elegido por el motor (`necesidad × capacidad × confianza`) despache Feed/Sleep/Escape, reemplazando la prioridad fija de `ActiveBehaveTick`. `PhraseCategory` ya tiene `Hechizo`/`Deseo`. **El salto grande** (conecta `Mind`→acción, hoy solo loguea). **Diseño: [`volition-selection-engine.md`](volition-selection-engine.md)** | `Mind`/`Animal`/`AiBrain` |
| **E1 ✔** | **Anatomía → stats** (bodyParts) (PR #133) | `armament` en `StatBonus`, aplicado por el delta gestionado (modulado por huésped, como lo biológico) → un colmillo/veneno sube `Anima.armament` que `Predation` lee. El arma sale ya de una PARTE | `CharacterComposition`/`StatBonus` |
| **E2 ✔** | **Anatomía → capacidad/RECEPTOR** (PR #134) | `CompositionPart.grants` (ojo→`"see"`, colmillo→`"bite"`) + `CharacterComposition.Grants(cap)`; `MindPhrase.gateCapability` + `Mind.PassesGate` lo comprueban → una frase-hechizo solo es seleccionable si el ser TIENE el receptor. Reusa el patrón del gate por aptitud | `CompositionPart`/`Mind` |

**Progreso:** A ✔ (#128), C ✔ (#129), B ✔ (#130), D1 ✔ (#131), D2 ✔ (#132), E1 ✔ (#133), D3a ✔ · E2 ✔ (#134). El motor
de selección de thoughts ya sabe ponderar por confianza y gatear por receptor/anatomía; los **mecanismos están listos y
DORMIDOS** hasta que existan frases-hechizo que fijen `capability`/`gateCapability`. Sigue el salto grande **D3b** (que
la selección CONDUZCA la acción, reemplazando `ActiveBehaveTick`) — conviene un `docs/` del motor de selección antes de
tocar código. Falta también, en D-cola: **productor de FIGHT** y el **decaimiento pasivo** de la confianza.

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
