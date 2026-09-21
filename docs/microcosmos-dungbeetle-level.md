# Microcosmos — el nivel de los escarabajos peloteros (Momo & Medea) + mecánicas que habilita

> Diseño (2026-09-21). Dos escarabajos peloteros = **vidas antiguas de Momo y Medea**; su rueda, el
> combate, el bond y la mudanza juntos; y las **dos misiones de Kushal** (extraer un cabello · recuperar
> la semilla). Cada mecánica lleva su **veredicto de estado del código** (EXISTE / PARCIAL / FALTA) para
> saber qué reutilizar y qué construir. Enlaza con [`microcosmos-eras-and-observation.md`](microcosmos-eras-and-observation.md),
> [`character-linduarda.md`](character-linduarda.md), [`anima-architecture.md`](anima-architecture.md) §11.5 (control/posesión).

---

## 1. La rueda: dos mentes, dos lecturas del mismo objeto

El pelotero lleva su bola de un lado a otro. **La lectura del objeto DEPENDE de la mente del ser** —y
eso ES el rasgo (stats→frases→identidad):
- **Momo (lectura trágica):** la rueda es "su yo muerto", una carga honorable que arrastra hasta el
  final; la **vanidad lo aísla** admirando su obra muerta. Pensamientos oscuros → menos desarrollo.
- **Medea (lectura vital):** la rueda es **abastecimiento** — alimento procesado y disponible; corre a
  él con alegría, "la fuente de la vida", futuro prometedor (familia). Pensamientos positivos → más
  desarrollo.

Peloteros reales: **mayormente solitarios**, pero forman **pareja temporal** que coopera rodando/
enterrando la bola-nido y **cuida la cría biparentalmente** dentro de ella (modelados `Family(2,
biparental)`). Perfecto para: encuentro solitario → combate → bond → cría en la rueda.

### La anticipación que hace crecer los stats  → **FALTA (construir; hay patrón)**
La idea: Medea, aguardando taciturna, **imaginó tan claro el futuro** (correr, agarrar, consumir,
tener familia) que **cuerpo y mente se prepararon** → al llegar la oportunidad **sus stats han
crecido**. No existe un sistema de "el deseo intenso hace crecer stats" (no hay `leap` — ese símbolo
**no está en el código**). PERO hay **patrón reutilizable**: `ObservationSkill.Train` (sube mirando),
`Anima.Confidence`/`spellConfidence` (sube por uso), y los stats evolutivos `baseAgility`/`basePerception`
de `SpeciesBody`. → **Construir un componente `AnticipationGrowth`** (o generalizar `ObservationSkill`):
un pensamiento-deseo persistente y de alta intensidad entrena, poco a poco, los stats que lo resolverían
(velocidad/fuerza/energía). Es la versión "por deseo" de lo mismo que ya sube la observación por mirar.

---

## 2. El encuentro, el combate y la mudanza juntos

Medea llega antes, fabrica su rueda y se la lleva. Momo (por azar más cerca y en la dirección de la
casa de Medea) la **intercepta y empuja la rueda hacia sí** → **combate por la rueda**. Mientras están
juntos **crece el bond** — sembrado **positivo, como si se conocieran de otra vida**.

- **Bond entre animas cercanas → impulsos sociales → hablar → pack:** `Anima.bonds`/`BondWith`
  (EXISTE), `PackAwareness` (EXISTE: ayuda/protección emergente), `SocialImpulse`/`TribeCohesion`
  (EXISTE). **Hablar:** la `Mind` **genera** pensamientos con tono/valencia e **intensidad** (`Depth()`
  0–4) pero **solo los vuelca a consola** (`Debug.Log("[Mente]…")`) → **no hay UI de diálogo/pensamiento
  para el jugador** (ver §3 y la mecánica de UI). Como en el mundo insecto todos son humanoides, "hablar"
  = mostrar esos pensamientos. **PARCIAL** (se generan; falta surface).

### Evento `livetogether` (mudarse juntos)  → **FALTA (construir)**
Necesidad fundamental para formar familias/tribus: dos animas se conocen, su bond supera un umbral y
**deciden vivir juntas**. Algoritmo propuesto (según el usuario):
1. Al superar el bond mutuo un **umbral**, disparar el análisis `livetogether`.
2. Cada integrante calcula el **valor de su hogar** = `bond_hacia_su_hogar − peligro_percibido` (usa
   `HomeOrigin` — EXISTE — y `PerceivedDanger`/`alertness` — EXISTE).
3. Comparan: gana el hogar con **valor más alto**.
4. **Gate:** solo se mudan si el **bond entre ellas** supera el **bond de quien tendría que abandonar su
   hogar** (si no, se cancela). Aquí el bond Momo↔Medea debe ganar → se mudan al mejor hogar.
5. Efecto: unifican `HomeOrigin` (semilla de familia/tribu). Base emergente = detección de peligro +
   nostalgia (bond al hogar). Reutiliza `HomeOrigin`, `bonds`, `PackAwareness`.

---

## 3. Misión 1 (Linduarda): extraer un cabello de cada pelotero

**Control por posesión / idle / “el cuerpo toma el mando”**  → arbitraje **EXISTE**, gate por necesidad
y adrenalina **FALTAN**. El control es por **relevancia** (`IBrain.Relevance`; `AnimaController` da el
mando al cerebro de mayor relevancia; la posesión debe superar `selfRelevance`). **No** existe: (a) modo
**idle** libre cuando las necesidades están saciadas, (b) que el ánima **rechace** el control del
jugador cuando una necesidad cruza un umbral, (c) **override por adrenalina** (actuar solo, con boost).
Hook limpio a construir: cuando una necesidad es crítica, **subir `AiBrain.selfRelevance`** (o inyectar
un cerebro-reflejo) para que **gane al `PlayerBrain`** → el ánima "toma el mando". Escalones:
- Necesidad **sobre el mínimo** → el jugador manda (idle libre).
- Necesidad **bajo el mínimo** → el ánima **veta acciones contrarias** (p. ej. no ir en dirección
  opuesta al alimento) salvo que el **peligro > estrés por hambre**.
- Necesidad **muy** bajo el mínimo → **autónomo**: el cuerpo se pasa de sus límites conscientes con
  **adrenalina** (más fuerza/velocidad/agilidad) y **niebla** sobre el análisis.

### `mindStatusCalculationFog` + bonus de pensamiento intenso  → **FALTA (construir)**
Ciencia (confirmada): el estrés agudo (catecolaminas + cortisol) **sube fuerza/velocidad/tolerancia al
dolor** pero **estrecha la atención y degrada el razonamiento** (visión de túnel; curva de
Yerkes-Dodson, ya parcialmente en `ActivityLevel`). Modelo:
- Un **pensamiento muy intenso** (hambre/miedo extremos) **suma bonus** a los stats que lo resolverían
  (velocidad/energía/fuerza bruta/agilidad) **y** eleva `mindStatusCalculationFog`.
- `mindStatusCalculationFog` (nueva property, 0..1) **se suma a los cálculos que producen acciones**,
  degradándolos. **Cálculos de acción que debería enturbiar (los que pediste enlistar):**
  1. `Animal.EvaluateThreat` / `Assess` — **malinterpretar el peligro** (subestimar/sobrestimar).
  2. `SenseThreats` + umbral de `alertness` (`ALERT_TO_REACT`) — reaccionar tarde o en falso.
  3. `Volition.SelectAndDispatch` (deseo × capacidad × confianza + histéresis de disciplina) —
     **elección impulsiva** (se dispara el deseo dominante, se ignoran los demás).
  4. Selección de presa/comida (`Forager.SelectTarget`/`Predation.SelectPrey`).
  5. Condición de ayuda de `PackAwareness` (`autoabandono + vínculo > peligroEspecífico`).
  6. Pensamientos **gateados por aptitud** (`MindPhrase.gated`/`gateMin`): la niebla baja el `reasoning`
     efectivo → **menos pensamientos analíticos**, más reactivos.
  7. Amortiguación por `Observation`/`ObservationSkill` (la niebla reduce la ecuanimidad).
  Es decir: la niebla = **reducción temporal de `reasoning`/`composure`/percepción-para-análisis**,
  mientras el drive **suma a `agility`/`strength`/`endurance`/energía** para la acción que resuelve.

### La escena de la misión (nace natural)
El jugador **no puede acercarse a arrancar un pelo mientras están despiertos y alerta** (Kushal tiene
miedo → al acercarse, **Kushal se para** y su **UI de pensamiento** muestra que no se atreve). Montaje:
1. Peloteros **en sus hogares**, **sin sueño y con hambre**.
2. **Hechizo de generación** de excremento (pasar prefab + lugar) en un punto **opuesto** a las casas,
   para que **vayan y vuelvan por el mismo camino**. → **Generación de prefab: PARCIAL** (no hay un
   `SpawnSpell` dedicado, pero el patrón `Instantiate(prefab, pos)` ya se usa en `Respawn`/`ProtectionMission`/
   `HoneydewProducer`; se envuelve en un `SpawnSpell : SpellBase`). **Temporizador de hechizos: EXISTE**
   (`SpellBase.duration` + `EffectTiming.Instant/Sustained/Periodic`).
3. El jugador llega a una casa y **espera** cerca; tras unos segundos **se lanza la generación** →
   ambos peloteros **corren allí** → el jugador (Kushal) **los sigue** y **presencia** el combate por la
   rueda + hablan + se hacen amigos + van juntos al nuevo hogar (evento `livetogether`, §2).
4. El **sueño llega por el cansancio** (correr/jalar/pelear/hablar). → **Sueño por agotamiento: PARCIAL**
   (`SleepCycle` es por reloj día/noche + despertar ante amenazas; existe `exhaustion` pero se usa en
   lactancia, no dispara el sueño). A construir: `exhaustion` alto → fuerza dormir.
5. Dormidos → **Kushal se acerca y extrae el cabello** (Linduarda se lo pide).

---

## 4. Misión 2 (Linduarda): recuperar la semilla dentro de la rueda

En la rueda van **la semilla + las crías** de pelotero → Kushal debe **esperar a que salgan**.

- **Pack: vigilia contra el sueño / `sleepDistribution` (dormir por turnos)**  → **FALTA (construir)**.
  Hay `PackAwareness` (ir a **ayudar/proteger** al compañero en peligro — EXISTE) y `PostNatalManager`
  (la madre **guarda** a la cría cerca de `HomeOrigin`, `Guard` — protección de crías **PARCIAL**), y
  llevar a la cría al `HomeOrigin` (EXISTE). **No** existe llevar a un miembro al **origen más seguro de
  entre los del pack**, ni **turnos de sueño**. Diseño:
  - Reacción a **entorno peligroso + sin lugar mejor**: el pack lleva al miembro vulnerable al
    `HomeOrigin` **más seguro/cercano** de algún integrante (extensión de lo de PostNatal).
  - Si ahí el peligro percibido por algún miembro sigue alto → **evento `sleepDistribution`**: intercala
    el sueño (mientras un padre duerme el otro vigila; con un solo padre + cría, se alternan) → **la cría
    también percibe y se queja** al padre para que la cuide. → Kushal, más cauteloso, **no dejará al
    jugador ir a la rueda** hasta que los padres salgan.
- **Disparador:** poco después de nacer las crías, llega un **depredador más fuerte que los padres**
  → los padres salen. **Depredador que aterra a los peloteros pero no a Kushal**  → **construible con lo
  que hay**: usar el mapa de **relaciones/karma por especie** (`Archetypes._relations`) + miedo
  diferencial (un insecto **débil pero muy venenoso**, o con **hipnosis/hechizo**, temido por los
  peloteros y no por el gusano Kushal). Puede ir **a por los peloteros** (no por Kushal/crías).
- **Crías bajo "hechizo apestoso" hasta cierta edad**  → **PARCIAL (construir variante)**. Existe el
  sistema de olor `ScentEmitter`/`ScentScanner` (atrae por olor). Falta la variante **repelente /
  reduce-comestibilidad** (recién salidas de la rueda, cubiertas de material → menos apetecibles):
  un `StinkSpell`/emisor negativo + un modificador de `IEdible`/apetito, con `duration`/edad.
- **Resolución:** Kushal **vence al depredador** (puede **ganar bond** con los peloteros por
  defenderlos) y, con los padres lejos de la rueda, **accede a la semilla**. El jugador lo entiende por
  la **UI de pensamiento** de Kushal (intensidad): al acercarse a la rueda protegida, Kushal se **para**
  y la UI muestra que **lo considera demasiado peligroso**.

---

## 5. UI de pensamiento de Kushal  → **FALTA (construir; el motor existe)**
La `Mind` ya **genera** el pensamiento actual (tono/valencia + intensidad `Depth()`), pero **solo a
consola**. Construir: **un bloque de FollowingArrays visible con el menú cerrado** que muestre el
**pensamiento actual** del ánima controlada, con su **intensidad**, persistente ~**10 min** (marca de
tiempo). Es el canal por el que el jugador "lee" a Kushal (miedo a acercarse, deseo, etc.) — pilar del
modelo "jugador = conector, no titiritero".

---

## 6. Tabla resumen de estado (qué reutilizar / qué construir)

| Mecánica | Estado | Base a reutilizar |
|---|---|---|
| Temporizador de hechizos | **EXISTE** | `SpellBase.duration` + `EffectTiming` |
| Bonds + pack (ayuda/protección) | **EXISTE** | `Anima.bonds`/`BondWith`, `PackAwareness`, `TribeCohesion` |
| `HomeOrigin` por anima | **EXISTE** | `Anima.HomeOrigin`, `FamilyGenerator`, PostNatal |
| Generación de pensamientos (tono/valencia/intensidad) | **EXISTE** | `Mind.Think`/`Depth`, `PhraseLibrary` |
| Sistema de olor (atracción) | **EXISTE** | `ScentEmitter`/`ScentScanner` |
| Proteger crías (madre-cría) | **PARCIAL** | `PostNatalManager.Guard`, `HomeOrigin` |
| "Hablar" (surface de pensamientos al jugador) | **PARCIAL** | falta UI (solo `Debug.Log`) |
| Sueño por agotamiento | **PARCIAL** | `SleepCycle` + campo `exhaustion` |
| Generación por hechizo (prefab+lugar) | **PARCIAL** | patrón `Instantiate` → envolver en `SpawnSpell` |
| Olor **repelente**/reduce-comestibilidad (apestoso) | **PARCIAL** | `ScentEmitter` negativo + `IEdible` |
| **`livetogether`** (mudarse juntos por bond) | **FALTA** | `bonds`+`HomeOrigin`+`alertness` |
| **Control por necesidad + idle + adrenalina** | **FALTA** | relevancia `IBrain`/`AiBrain.selfRelevance` |
| **`mindStatusCalculationFog`** + bonus por deseo | **FALTA** | `ActivityLevel`(Yerkes-Dodson), `Humores` |
| **Crecimiento de stats por anticipación** (`leap` NO existe) | **FALTA** | patrón `ObservationSkill.Train`/`Confidence` |
| **UI de pensamiento de Kushal** (bloque 10 min) | **FALTA** | `Mind` + FollowingArrays |
| **`sleepDistribution`** (turnos de vigilia) | **FALTA** | `SleepCycle` + `PackAwareness` |
| Depredador que aterra a peloteros pero no a Kushal | **FALTA** | `Archetypes._relations` + miedo diferencial |

---

## 7. Orden de construcción sugerido (rebanadas)
1. **UI de pensamiento de Kushal** (§5) — desbloquea que el jugador "entienda" todo lo demás.
2. **Control por necesidad + idle + adrenalina/`mindStatusCalculationFog`** (§3) — el corazón del modelo
   jugador-conector; base para que las misiones "nazcan naturales".
3. **`SpawnSpell`** (generación con temporizador, §3) + **sueño por agotamiento**.
4. **Evento `livetogether`** (§2) — semilla de familias/tribus.
5. **`sleepDistribution`** + extensión "origen más seguro del pack" (§4).
6. **`StinkSpell`/reduce-comestibilidad** + **depredador de miedo diferencial** (§4).
7. **`AnticipationGrowth`** (stats por deseo intenso, §1).
8. Modelos/escena (ver [`unity-editor-manual.md`](unity-editor-manual.md)): pelotero con su bola, crías.
