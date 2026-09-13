# Mecánicas de CONSCIENCIA — el interés central del juego

> El interés central de Cold Sanctuary es **incrementar la consciencia del jugador**: revelar las propiedades físicas y
> mentales de un ser y **cómo trabajarlas por mecánicas/juegos**. Bajo esa luz, tener MUCHAS propiedades (las 70+ de CHC,
> los humores, los apremios) **no es inflar**: cada una es una **dimensión entrenable que se revela y desarrolla jugando**.
> Diseño de sistema en [`apremios-guardian-observacion.md`](apremios-guardian-observacion.md). Marco de entrenamiento ya
> existente: `AptitudeEvolution` (evolución por uso), `ObservationSkill`, `learning-unlocks.md` (aparecen en la UI al aprenderlas).

## 1. Principio: propiedad ⇄ mecánica que la desarrolla

Regla: **no se añade una propiedad sin la mecánica que la trabaja** (una propiedad sin forma de desarrollarla no enseña
nada). Cada propiedad tiene: (a) **efecto visible** (en ánimo/energía/cámara/postura), (b) una **actividad** que la sube
(directa o indirectamente, a veces por permanencia en un estado a lo largo del tiempo), (c) un **descubrimiento** (el
jugador ve el efecto y aprende la relación). Algunas crecen como **resultado acumulado de un comportamiento sostenido**.

## 2. AUTORREGULACIÓN = el cuidado de Kushal (creature-care loop)

Idea del usuario: la autorregulación es **el cuidado que el jugador ejerce sobre Kushal**. Bucle:
- **Todos los stats VISIBLES** para el jugador (físicos y mentales), con sus **efectos** en ánimo/energía/**cámara**
  (ya existe `ScreenEffects`: sueño=párpados, fatiga=gris, estrés=viñeta) y postura (`CreatureRig`/`EmotionExpression`).
- El jugador decide **comer / descansar / entrenar / formar vínculos / yoga**, y debe **DESCUBRIR los efectos** de cada
  acción y cada comida (**cada comida un efecto distinto** en los stats — ya hay base: `Metabolism` nutriente→elemento→stats).
- Debe poder **ver qué causa el yoga** en los elementos del cuerpo/mente, y lo mismo con las demás actividades (bonds, descanso).
- Al ser todo visible sobre Kushal, el jugador se **alinea a tomar las decisiones correctas** para mantener los stats en
  un estado rico → **autorregulación**: aprender a leer y equilibrar las propias necesidades.
- **Permanencia en bienestar + actividad-tenacidad → efectos ACUMULATIVOS**: el crecimiento de algunas de las 70+
  (unas suben por uso directo, otras como resultado indirecto de un comportamiento sostenido en el tiempo).

Piezas existentes que reusa: `PlayerStats`/`IMind`/`IBody` (stats), `ScreenEffects` (cámara), asanas/yoga, `Bond`/bonds,
`Metabolism`/`Humores` (comida→stats), `MoodDynamics`/`AllostaticState` (el Guardián), `AptitudeEvolution` (crecimiento).
Falta: un **HUD completo de stats de Kushal** (visible + descubrimiento progresivo) y el mapeo **acción/comida → efecto**
por-ítem, más el enganche de "bienestar sostenido → sube aptitud".

### 2.1. Base científica del ESTADO IDEAL (implementada) + la UI de colores

- **Composición de nutrientes (elementos):** el cuerpo es, por % de masa, O 65% · C 18.5% · H 9.5% · N 3.3% · Ca 1.6% ·
  P 1.2% · K 0.4% · S 0.25% · Na 0.15% · Cl 0.15% · Mg 0.05% (≈99.9%). → **`ElementsStatus`** (implementado): `IdealGrams
  (elemento, masa) = fracción × masa`; `Classify(actual, ideal)` por RATIO → Deficiente/Bajo/**Ideal**/Alto/Exceso; `ColorOf`
  (verde/naranja/rojo); `Format` (g/mg/µg/ng). **Es la única función** que la UI llama por elemento: número con unidad +
  color de si está estable/inferior/superior. **Se EXTRAPOLA a cualquier especie/body** con solo su masa (mismas proporciones).
- **Nivel de actividad (el "estado bendecido"):** modelo real del deporte — **carga aguda (7 días)** vs **crónica (28
  días)**; su ratio **ACWR** en **0.8–1.3 = zona óptima/bendecida** (fitness y fatiga en equilibrio). Coincide con
  **Yerkes-Dodson** (rendimiento/creatividad máximos con arousal ÓPTIMO, no máximo → *flow*: "calmado pero energizado").
  **Fitness = carga crónica**, lo que crece despacio con actividad sostenida y alimenta el **crecimiento acumulado** de
  aptitudes. → **`ActivityLevel`** (implementado): `Acute`/`Chronic`/`Acwr`/`Freshness`/`Zone` (Desentrenado/**Bendecido**/
  Sobrecarga), EWMA por día de juego. Cada actividad llama `AddLoad`.
- Test: `IdealStateTest` (grupo 13). Fuentes: composición ([sciencenotes](https://sciencenotes.org/elements-in-the-human-body-and-what-they-do/)),
  ACWR ([scienceforsport](https://www.scienceforsport.com/acutechronic-workload-ratio/)), Yerkes-Dodson/flow ([simplypsychology](https://www.simplypsychology.org/what-is-the-yerkes-dodson-law.html)),
  fitness-fatiga ([humankinetics](https://journals.humankinetics.com/view/journals/ijspp/17/5/article-p810.xml)).

### 2.2. Auditoría de la cadena ELEMENTOS → COMPUESTOS → HUMORES (2026-09-13)

- **Elementos (ampliados, PR #195):** `ElementsStatus` ya no lista solo los 11 mayores — añade los **traza esenciales**
  (`Fe`/`F`/`Zn`/`Cu`/`Mn`/`I`/`Se`/`Co`) que rigen mecánicas concretas (Fe=sangre/O₂, I=tiroides/metabolismo, Zn=inmunidad…).
  No era una muestra parcial a propósito; faltaban y ahora están, con su fracción real por masa (µg–mg/kg). Ampliables.
- **Elementos → compuestos → células → stats:** ✅ **existe y con detalle** en `Constitution`: elementos (símbolos reales,
  validados vs `PeriodicTableManager`) → **compuestos** (proteína/ATP/minerales/lípidos) → **células** (músculo/glóbulos=
  Fe+proteína/neurona/hueso) → **stats base**. Alimentable en juego (`AddElement`).
- **Compuestos → HUMORES:** ✅ **PUENTE HECHO (PR #196)** — `ChemistryHumors`: la composición (`Constitution`: ATP/
  minerales/proteína/neurona) **genera/influye los humores** (`Mind.humores`) a corto plazo (empuje/tick) y largo plazo
  (el objetivo lo fija la composición, que cambia despacio). Mapeo: ATP→Glucosa, minerales→Calcio, proteína+neurona→
  Serotonina (lento), carencia→Cortisol. De ahí el ánimo pesa en la **ARENA** (energía/estrés químicos compiten con los
  pensamientos → tu "enfrentar pensamientos con la composición química"). Getters de compuestos añadidos a `Constitution`.
  Cadena completa: elementos→compuestos→(células→stats **y** →humores). Opt-in (componente).

### 2.3. HUD declarativo (`FollowingArrays`/`Palette`) — huecos y avance

El HUD "declarativo" = la UI de arrays de paneles reutilizables (la misma que la tabla periódica / asanas). Estado:
- **Prototipo OnGUI** (`AnimaStatusHUD`): ✅ funciona ya (drives/actividad/elementos con color; sin dependencias).
- **Adaptador declarativo** (`AnimaStatusPanels`, PR #197): ✅ la mitad de CÓDIGO — colorea PANELES-GameObject por
  elemento (Renderer) + valor en `TextMesh`, con el "actual" desde `Constitution.El()` × ideal por masa.
- **Paneles GENERADOS POR CÓDIGO** (PR #198): `BuildAnimaStatusPanels` crea los paneles-GameObject (Quad+TextMesh) por
  elemento y los vincula con `AnimaStatusPanels` → **el HUD declarativo se genera SIN prefab manual** (todo versionado
  en el `.cs`). Respuesta a "¿prefabs por código?": **SÍ** — o construyendo el GameObject en escena (como aquí y como los
  actores del microcosmos), o con `PrefabUtility.SaveAsPrefabAsset` para un `.prefab` reutilizable (como `AnimalPrefabGenerator`).
  Lo único no versionado es el ARCHIVO `.prefab` (por la lista blanca del `.gitignore`), NO la capacidad de generarlo.
- **Huecos restantes**: `MaterializationExecutor` **inalcanzable** (faltan evaluadores del `Palette` cableados) — el camino
  Palette→materializar sigue con huecos (aparte del HUD). El binding a valores vivos ya lo aporta `AnimaStatusPanels`.
  Integrarlo en los "sheets" de `FollowingArrays` es cosmético ahora (los paneles pueden ser prefab o code-gen como aquí).

## 3. OBSERVACIÓN — por CUALQUIER sentido (no solo la vista) + minijuego

**Observar = percibir por cualquier RECEPTOR que el ser tenga** (PR #198), no solo los ojos. Base científica: el humano
ya tiene &gt;5 sentidos y los animales más — exterocepción (vista/oído/olfato/gusto/tacto/**termocepción**/**ecolocalización**/
**magneto**/**electrorrecepción**) + internos (**propiocepción**/**vestibular**/**interocepción**/**nocicepción**). Catálogo en
`Senses` (sentido→alcance: el olfato llega lejos, el tacto exige contacto). `ObserveSpell` observa por el MEJOR sentido
que tenga el ser → un topo observa por olfato/tacto, un murciélago por ecolocalización, Sakshi debilitada por varios.
Qué sentidos tiene = su CONFIGURACIÓN (receptores de su anatomía) → encaja con "todo es Anima, la config restringe".

Idea del usuario (minijuego): se muestra un **entorno por unos segundos**, luego se presentan **preguntas de opción múltiple** para
que el jugador **identifique y memorice rápido** lo que vio. Entrena **percepción + memoria de trabajo** (y la observación).
- Enlaza con el sistema: acertar sube `ObservationSkill`/`perception`/`memory` (evolución por uso), y la observación
  **amortigua el sufrimiento** (Guardián) — el jugador entrena la ecuanimidad "mirando bien".
- Es la versión JUGADOR del pasivo `ObserveSpell` (que ya hace "mirar sostenido → sube observación") de las ánimas.

### 3.1. Órganos ⨯ canales de información (PR #199)

"Observar" es un HECHIZO que se lanza por CUALQUIER body-part sensorial **o** desde el grimorio (doble vía `CanUse`).
Cada órgano DECLARA qué **información** genera, no su nombre:
- **`PerceptChannel`**: los canales de información — GENERALES (Presence/Distance/Direction/Movement, casi cualquier
  sentido) + ESPECÍFICOS (Color/Shape=vista, Odor=olfato, Flavor=gusto, Sound=oído, Texture/Temperature=tacto/termo,
  Bioelectric=electro) + de alto nivel (Identity/Emotion/Threat).
- **`SenseOrgan`** (la clase de órgano que faltaba): `sense` (fija el alcance) + `provides` (canal→calidad 0-1) +
  `rangeMultiplier`. **Todo es config**, no el tipo → **ojo normal / ojo CIEGO (Color 0) / ojo BORROSO (Color ~0.3) /
  lengua que SABOREA LA LUZ (gusto que provee Color) / oído que escucha la luz…**. Sí, una "super-lengua" que saborea
  partículas del aire (rastrear como el olfato) o la luz (rastrear como la vista) es POSIBLE — se configura sus canales.
  Presets: `Eye/BlindEye/Nose/Ear/Tongue/Skin/LightTastingTongue`.
- **`ObserveSpell.Perceive(target)`**: AGREGA los canales de todos los órganos del ser (máxima calidad, atenuada por la
  distancia) → un dict canal→calidad = QUÉ información saca del objetivo. Vía MÁGICA (grimorio "observar"): añade todos
  los canales a calidad = **confianza del hechicero** → **más habilidad, más información**. Test `SensesTest` (grupo 14).
- Respuestas: ¿el gusto puede dar distancia? **sí, por config** (un órgano de gusto puede listar `Distance`). ¿Lenguas
  que saborean luz / oídos que oyen luz? **sí, por config** (canales sinestésicos). ¿El grimorio también trae la lista?
  **sí**, y escalada por la habilidad.
- **SIN enganche anatómico (decisión 2026-09-14):** cualquier órgano puede ir en cualquier `Anima`, LIBRE — es lo que
  habilita las **quimeras** (un pez con ojos de águila, una lengua que saborea la luz). No hay bloqueo por "receptor real".

### 3.2. Base científica de la percepción (validación de `PerceptChannel`)

`PerceptChannel` es correcto y ahora está FUNDAMENTADO en el modelo real (2 ejes ortogonales):
- **Transducción** (`Senses.Transduction`): la biología clasifica los sentidos por la ENERGÍA que convierte el receptor —
  **FOTO**(vista) · **MECANO**(oído/tacto/propio/vestibular/eco) · **QUIMIO**(olfato/gusto) · **TERMO** · **NOCI**(daño) ·
  **ELECTRO** · **MAGNETO** · **INTERO**(interno). Es el CÓMO. La sinestesia/**sustitución sensorial** (real) = una
  transducción que deriva un percepto de otra modalidad → funda la "lengua que saborea la luz".
- **Nivel del percepto** (`Percept.Kind`): **estímulo** crudo que transduce el receptor (Color/Sonido/Olor…) → **percepto**
  derivado por el cerebro (Distancia/Identidad; el estímulo no basta — la sala con niebla) → **affordance** (Gibson: lo que
  el entorno OFRECE hacer — Amenaza/Emoción). `PerceptChannel` cubre los tres niveles; `Percept.Kind` los etiqueta.

## Estrategia de PREFABS / versionado / UI (respuestas)

- **Escenas y tests: ya son TODO por CÓDIGO** (`SampleSceneBuilder`/`MicrocosmosSceneBuilder`/`MobWorldSceneBuilder` + los
  `*Test : ITestUnit`). No hay `.unity` hechas a mano versionadas → **nada que migrar**; ya es "code-first".
- **Prefabs solo para modelos 3D** (animales): `AnimalPrefabGenerator` monta el prefab desde un `.fbx` en
  `Assets/Animals/{Especie}/Models/` (esa carpeta ES el "setting de modelo 3D") y el builder lo referencia por ruta
  (`LoadAnimalPrefab`). El `.prefab` generado no se versiona (lista blanca del `.gitignore`), pero el CÓDIGO que lo genera sí.
- **UI:** se queda como **`FollowingArrays`** (sistema declarativo de paneles reutilizables); sus paneles se pueden
  **generar por código** (`BuildAnimaStatusPanels`, ya hecho) → no requiere prefabs manuales. El hueco aparte del HUD son
  los **evaluadores del `Palette`** (`MaterializationExecutor`).

## 4. Directriz de ARQUITECTURA — `Anima` debe bastar (por configuración) para TODO

Decisión del usuario (2026-09-13): **las Ánimas deben ser capaces de TODA la simulación social con solo configuración**.
Si no lo son, significa que **`Anima` no está completo** y hay que **ampliar su sistema de componentes** para darle esa
capacidad. Corolario: **`SimpleAnima` no debería usarse para PERSONAJES** — un personaje es un ser completo con todas sus
facultades/stats configuradas según la realidad. `SimpleAnima` queda solo para lo verdaderamente **inanimado**.

**El conflicto a resolver (por qué hoy no se cumple):** la sim social del microcosmos (rebanadas 2-3: `SocialImpulse`
Tend/Cull/cohesión) se construyó sobre `SimpleAnima` + `ImpulseController`, mientras que `Animal` mueve por su propia IA
de NavMesh (`AiBrain`/`Volition`/`ThreatResponder`). Un ser con AMBOS tendría **dos sistemas de movimiento peleando** por
el `NavMeshAgent`.

**Progreso (PR #190):** los **apremios sociales ya son DESEOS** en `DesireCatalog` (`tend` = cuidar al vínculo en apuro,
`follow` = cohesión), compitiendo en `Volition` con `eat`/`mate`, ponderados por afabilidad/sociabilidad → la sim social
es ya **capacidad de cualquier `Animal` por config**, en la MISMA arena que el hambre. Falta: darle esa arena a los
compañeros (hoy `SimpleAnima`, sin agencia — ver §4.3) y afinar los pesos.

### 4.3. Migración de COMPAÑEROS y retirada de `SimpleAnima` — plan preciso

Auditado: `MakeCompanionCore` monta el compañero como **`SimpleAnima` + Mind + SoulComposition + Mood** → tiene mente/
humores/identidad pero **NO agencia**: no decide ni actúa vía la arena, porque **solo `Animal` tiene el bucle que llama a
`Volition`/`SenseThreats`** (`ActiveBehaveTick`/`AiBrain`). Por eso hoy los personajes no-animales quedan "a medias".

**Análisis PROFUNDO de `Anima` (auditado 2026-09-13):** la base `Anima` **ya es el hogar de casi TODO** por config —
las 12+ aptitudes (+ afabilidad/sensibilidad/armadura/armament/sickness/autoabandono/magicAura), los drives (stress/
trauma/fatReserves/temperature/sleepiness/mentalFatigue/satisfaction), afinidad de medio, el sistema de **bonds**, la
**confianza-por-hechizo** (`spellConfidence` = temperamento histórico), y `CanUse`/`KnowsSpell` (la doble vía anatomía∨
magia). Lo ÚNICO que las subclases aportan son **3 hooks abstractos** — `RespondToHunger`, `EvaluateThreat`,
`RespondToThreat` — **más el BUCLE que los ejecuta** (`Animal.ActiveBehaveTick`/`AiBrain`/`Volition` + el cableado de
`Init`: NavMesh/Forager/Locomotion/LifeStage).

**Qué es la AGENCIA:** justo eso — la capacidad de **percibir → decidir → actuar** sobre los propios drives (los 3 hooks
+ el bucle de decisión). Hoy vive baked en `Animal`; `SimpleAnima` implementa los hooks como **no-ops** (por eso "no
decide sola"). **No son dos mundos**: es UN `Anima` al que le falta activar una capacidad. Corrección aceptada: **todo es
`Anima`; lo que restringe es la CONFIGURACIÓN**. `SimpleAnima` es solo "un `Anima` con la agencia apagada" — no algo
"para inanimados"; un inanimado es igualmente un `Anima` con casi todo apagado.

**El nudo real:** para que cualquier `Anima` sea agente por config (y desaparezca la distinción `SimpleAnima`/`Animal`),
hay que **sacar la AGENCIA de `Animal` a un componente/bundle** (los 3 hooks + su bucle) que se añada por configuración.

**Plan recomendado (rebanada dedicada):** extraer el "cableado de agencia" de `Animal.Init` a un **bundle reutilizable**
(un `AgencyCore`/componente) que añade y tickea AiBrain+Volition(+Locomotion/Forager según config). Entonces:
- un COMPAÑERO = `Anima` + `AgencyCore` (por config) + pilares → **completo y agente**, sin ser un "animal".
- `SimpleAnima` se reserva a lo inanimado (o se disuelve: un `Anima` "completo por defecto" cuyas capacidades se apagan por config).
Es un refactor de `Animal.Init` (con test de paridad) → se hace como su propia rebanada para no romper la fauna.

**Plan de unificación (a implementar por rebanadas):**
1. **Un solo sustrato de movimiento.** Que la IA de `Animal` (deseos de `Volition`) **alimente impulsos** en un
   `ImpulseController` unificado (o al revés: que los `SocialImpulse` se expresen como **deseos** del `Volition`). Un solo
   lugar donde se suman hambre + amenaza + impulsos sociales.
2. **La sim social como CAPACIDAD de `Anima`** (componentes configurables), no atada a `SimpleAnima`. Tend/Adore/Grief/
   cohesión disponibles en cualquier ser por config.
3. **Migrar el elenco del microcosmos a `Animal` completo** (con `SoulComposition` como dueña de aptitudes — habilitador
   ya listo, PR #187) → habilita **predación real** (IEdible: el depredador puede cazar a Sakshi) y ciclo de vida.
4. **`SimpleAnima` solo para inanimado** (rocas, plantas, el enjambre `Swarm` no-Anima).

Es el rework más grande pendiente; toca la sim social recién hecha y el mapa del compañero → coordinar. Habilitador ya
puesto: `Animal.Init` cede las aptitudes a `SoulComposition` si está presente (PR #187).

### 4.1. El CHOQUE entre sistemas es el RASGO, no el bug (corrección del usuario, 2026-09-13)

Aclaración importante: pensamientos ⨯ bonds ⨯ stats ⨯ herramientas ⨯ necesidades-biológicas **deben COMPETIR entre sí**;
de esa pugna, resuelta por la **configuración/stats propios** de cada ser, emerge **su respuesta característica** = su
**personalidad**. Por tanto la unificación NO es "elegir un sistema y descartar otro", sino **un único ARENA de
arbitraje** donde todos los tirones (hambre, miedo, apremios sociales, deseos, hábitos) aportan su peso y **ganan según
quién sea el ser**. `Volition` (deseo = necesidad×capacidad×confianza) y el `ImpulseController` (suma de impulsos
ponderados) son dos mitades del mismo arena → se fusionan en uno. La diversidad de resoluciones ES el objetivo.

### 4.2. Estado de la MIGRACIÓN a `Anima` (auditado 2026-09-13)

- **Animales:** `Animal : Anima, ITarget, IEdible, …` ✅ (ya son `Anima`). `Mind` es **opt-in** (no todos lo llevan) → si
  se quiere que "piensen", hay que añadírselo por config (encaja con "capacidad por configuración").
- **Compañeros (Goluis/Panterilia):** ❌ **siguen siendo `MonoBehaviour` sueltos** (`Assets/Scripts/Companion/Companions/`) —
  el "grave error" que mencionas **no está migrado**. El camino nuevo (composición `SimpleAnima`+pilares en el
  SoulBlendSandbox) coexiste con esas clases legacy. Pendiente: migrarlos a `Anima` completo por composición.
- **`SimpleAnima`:** aún se usa para actores del microcosmos + sandboxes. **Es** un `Anima` concreto (mínimo), pero la
  directriz es **retirarlo para personajes**: un personaje = `Anima`/`Animal` completo cuyas capacidades limita su
  CONFIGURACIÓN, no una clase recortada. `SimpleAnima` solo debería quedar (si acaso) para lo verdaderamente inanimado.
- Los **bonds** (registro por-miembro `Anima.bonds` + por-especie `Archetypes.speciesBond`) **ya son** el sustrato de la
  simulación social — encajan con "tenemos los bonds para la sim social".

## 5. Herramienta de contexto: CODEMAP + CI

Para que el contexto del proyecto esté siempre completo (y no haya que "redescubrir" leyendo): `docs/CODEMAP.md`
(índice de TODAS las clases con su resumen, generado por `tools/gen_codemap.py`) + un **CI** (`.github/workflows/codemap.yml`)
que lo **regenera y verifica en cada PR**. Complementa la tabla de sistemas de `CLAUDE.md` y el índice de `docs/`.
