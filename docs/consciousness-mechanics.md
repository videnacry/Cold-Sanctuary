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

## 3. OBSERVACIÓN — minijuego de destello + preguntas

Idea del usuario: se muestra un **entorno por unos segundos**, luego se presentan **preguntas de opción múltiple** para
que el jugador **identifique y memorice rápido** lo que vio. Entrena **percepción + memoria de trabajo** (y la observación).
- Enlaza con el sistema: acertar sube `ObservationSkill`/`perception`/`memory` (evolución por uso), y la observación
  **amortigua el sufrimiento** (Guardián) — el jugador entrena la ecuanimidad "mirando bien".
- Es la versión JUGADOR del pasivo `ObserveSpell` (que ya hace "mirar sostenido → sube observación") de las ánimas.

## 4. Directriz de ARQUITECTURA — `Anima` debe bastar (por configuración) para TODO

Decisión del usuario (2026-09-13): **las Ánimas deben ser capaces de TODA la simulación social con solo configuración**.
Si no lo son, significa que **`Anima` no está completo** y hay que **ampliar su sistema de componentes** para darle esa
capacidad. Corolario: **`SimpleAnima` no debería usarse para PERSONAJES** — un personaje es un ser completo con todas sus
facultades/stats configuradas según la realidad. `SimpleAnima` queda solo para lo verdaderamente **inanimado**.

**El conflicto a resolver (por qué hoy no se cumple):** la sim social del microcosmos (rebanadas 2-3: `SocialImpulse`
Tend/Cull/cohesión) se construyó sobre `SimpleAnima` + `ImpulseController`, mientras que `Animal` mueve por su propia IA
de NavMesh (`AiBrain`/`Volition`/`ThreatResponder`). Un ser con AMBOS tendría **dos sistemas de movimiento peleando** por
el `NavMeshAgent`.

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

## 5. Herramienta de contexto: CODEMAP + CI

Para que el contexto del proyecto esté siempre completo (y no haya que "redescubrir" leyendo): `docs/CODEMAP.md`
(índice de TODAS las clases con su resumen, generado por `tools/gen_codemap.py`) + un **CI** (`.github/workflows/codemap.yml`)
que lo **regenera y verifica en cada PR**. Complementa la tabla de sistemas de `CLAUDE.md` y el índice de `docs/`.
