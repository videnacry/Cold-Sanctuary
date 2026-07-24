# Aptitudes de las criaturas — agilidad, percepción, fuerza, masa

Modelo compartido de aptitudes para **animales y humanoides** (NPCs, y a futuro el jugador).
Introducido el 2026-07-10.

## Premisa

Cada aptitud es un **valor relativo a la media real** de su tipo: `1.0` = media,
`>1` por encima, `<1` por debajo. No son valores absolutos, son múltiplos de "lo normal".

Las aptitudes **no son fijas**: representan el estado actual de la criatura y **deben ir
cambiando conforme gana (o pierde) musculatura, agilidad, inteligencia/percepción**, según:

- **Origen** — de dónde viene la criatura / su historia previa fija el punto de partida.
- **Tareas y acciones del día a día** — lo que hace repetidamente la moldea con el tiempo
  (trabajo físico → fuerza y masa; estudio → percepción; sedentarismo → fuerza/masa a la baja).

Los **animales** tienen un abanico de tareas estrecho (comer, huir, criar…) → su calibración
es sobre todo por especie/origen. Los **humanoides** desempeñan un abanico mucho más amplio de
tareas y orígenes → admiten **progresiones mucho más especiales e individuales**.

> **Yoga/asanas y magia son universales:** todos los personajes (jugador, NPCs, companions) pueden
> practicar yoga/asanas y —cuando exista esa área— magia; no están gateados por personaje. La práctica
> de yoga entrena sobre todo `flexibility` (dimensión `flex` de `BodyPartStats`) y `composure`.

> **Estado actual:** los valores base están calibrados a mano ("media real"). El bucle que los
> hace **evolucionar por tareas/tiempo** aún no está implementado — es el siguiente paso de diseño.

## Dónde vive en el código

- **Animales** — `LivingEntity.agility` / `LivingEntity.perception` (campos, 1.0 = media).
  `Animal` expone `virtual BaseAgility` / `BasePerception` por especie; se fijan en `Init()`.
  `sensibility` (umbral de detección de amenazas en `Escape()`) deriva de `perception`:
  `sensibility = BaseSensibility * perception` → más percepción, reacciona antes.
- **Humanoides (companions)** — `CompanionBase.agility/perception/strength/bodyMass/adaptability/composure`
  (+ `virtual Base*`), fijados en `Start()`. Cuando llegue `NPCBase : LivingEntity` estas
  aptitudes se consolidarán en `LivingEntity` y se eliminará la duplicación.
- **Jugador** — `PlayerStats` ya tiene equivalentes parciales: `observationRadius` (≈ percepción)
  y `velocity` (≈ agilidad); crecen con la práctica (asanas). Pendiente unificar nomenclatura.
- **Intención (decidido 2026-07-24):** las 12 aptitudes pertenecen a **todo ser vivo**; su hogar es
  `LivingEntity`. El split actual (animales 2 + Physiognomy, companions 12, jugador parcial,
  `WorldCharacter` aparte) es **transitorio** hasta `NPCBase`. Los animales usan pocas activamente
  (agility/perception evolucionan); el resto quedan latentes.

## Calibración por especie (animales)

| Especie | agility | perception | Razón |
|---|---|---|---|
| Conejo | 1.5 | 1.6 | presa hipervigilante y veloz |
| Ciervo | 1.4 | 1.5 | presa alerta y rápida |
| Zorro | 1.4 | 1.5 | ágil, sentidos afinados |
| Lobo | 1.2 | 1.4 | cazador de manada, olfato agudo |
| Malamute | 1.1 | 1.1 | perro de carga, fuerte y resistente |
| Foca | 1.1 | 1.2 | ágil en agua, torpe en tierra |
| Oso | 0.7 | 1.1 | poderoso pero lento; buen olfato |
| Ballena | 0.6 | 1.0 | enorme, poco maniobrable |

## Perfiles de humanoides (companions)

Los valores codifican **origen + historia**, no solo el presente. Tabla consolidada (1.0 = media real):

| Aptitud | Goluis | Panterilia | Gohageneis | Irosene |
|---|---|---|---|---|
| agility | 0.9 | 0.95 | 1.2 | 1.2 |
| perception | 1.1 | 1.7 | 1.05 | 1.3 |
| strength | 1.5 | 0.7 | 1.1 | 1.2 |
| bodyMass | 1.3 | 0.8 | 1.1 | 1.3 |
| adaptability | 0.6 | 1.4 | 1.7 | 1.6 |
| composure | 1.5 | 0.7 | 1.2 | 1.4 |
| endurance | 1.4 | 0.9 | 1.3 | 1.3 |
| reasoning | 0.7 | 1.6 | 1.0 | 1.1 |
| memory | 0.8 | 1.4 | 1.0 | 1.3 |
| creativity | 0.7 | 1.4 | 1.2 | 1.5 |
| sociability | 0.7 | 1.1 | 1.7 | 1.7 |
| discipline | 1.3 | 1.5 | 0.6 | 1.1 |

### Irosene (70+ años) — pasión / sociabilidad
- `sociability 1.7`, `creativity 1.5`, `adaptability 1.6`, `composure 1.4`, `memory 1.3`, `perception 1.3`.
- **Codifica origen + presente transformado.** Origen: obesidad y poca movilidad + secuelas de cáncer
  → `strength`/`agility`/`endurance` muy bajas. Presente (santuario + magia): corre, escala y bucea
  profundo → suben con fuerza (1.2–1.3). `bodyMass 1.3` (sobrepeso, ya funcional).
- **Sociabilidad y creatividad máximas:** hiperexpresiva y motivadora; recetas de dulces y karaoke.
  **Adaptabilidad 1.6:** sobrevivió a dos hogares violentos y levantó un negocio desde cero.
- **Destreza de envolver dulces** de niña → velocidad/habilidad que hoy destacan en el **área textil**.
- Ficha completa: [`character-irosene.md`](character-irosene.md).

### Goluis (25 años) — fuerza
- `strength 1.5`, `bodyMass 1.3`, `agility 0.9`, `perception 1.1`, `adaptability 0.6`, `composure 1.5`.
- Trabajo de fuerza del mismo tipo toda su vida → **fuerza aumentada**; invierte mucho en comida
  → **masa aumentada**. Un solo tipo de trabajo → adaptabilidad baja; corpulencia → agilidad algo baja.
- **Atención al detalle práctica, no académica:** en su trabajo es **rápido y muy atento al detalle**
  (`perception 1.1`); tanto, que a veces se escaquea (se va "al baño" a mirar el móvil) sin que se
  note — encaja con su picardía/`composure`. Lo bajo en él es lo **académico/abstracto** (dejó de
  estudiar a los 17): `reasoning`/`memory` bajos. Distinción clave del modelo: **percepción (atención
  práctica/sensorial) ≠ razonamiento/memoria (estudio abstracto).**
- **Pasado (adolescencia):** formó parte de un grupo de bandidos. No ha dado muchos detalles, pero se
  sabe que **les pidió ayuda para que la madre de su hija no presentara una denuncia en su contra**.
  Ese pasado lo lleva a **mirar con desconfianza** (refleja el anchor `trust_people = -0.4`) y le da
  **temple/control en situaciones de peligro o estrés** (`composure 1.5`).

### Panterilia (42 años) — percepción
- `perception 1.7`, `strength 0.7`, `bodyMass 0.8`, `agility 0.95`, `adaptability 1.4`, `composure 0.7`.
- Bajo estrés agudo **tiende a bloquearse** (`composure 0.7`), a diferencia de Goluis.
- Nunca dejó de estudiar y ha ocupado múltiples puestos/roles → **atención al detalle muy alta**
  y cierta **lógica de negocio**. Alejada del trabajo físico y de la vida corporal → su **fuerza
  incluso ha decrecido** y, como la alimentación es parte de la vida corporal, su **masa está por
  debajo de la media**.
- **Rasgo mental (documentado, aún sin mecánica):** su gran facilidad/capacidad para **liberar
  energía mental** hace que pueda **crear exageraciones de la realidad** en su mente, y que se vea
  muy **influenciada por ideas fuera de su experiencia propia** (teorías, experiencia e imaginación
  de terceros). Candidato a modelarse como un modificador de percepción/creencias o un `ThoughtAnchor`.

### Gohageneis (28 años) — celebración / versatilidad
- `agility 1.2`, `strength 1.1`, `bodyMass 1.1`, `perception 1.05`, `adaptability 1.7`.
- Salió de casa a los 16 buscando una buena vida; **nómada**: Venezuela → Colombia → Perú →
  Ecuador → España → USA → santuario, encadenando trabajos muy variados (mudanzas, ventas,
  bartender, Uber, cocina, limpieza, construcción). Fiesta, baile, gimnasio y comer → **físicamente
  equilibrado tirando a ágil**. Percepción de calle/social (dejó de estudiar a los 16, no académica).
  Su rasgo definitorio es la **versatilidad / velocidad de adaptación** altísima (`adaptability 1.7`);
  la vida de calle/bartender le da además buen temple (`composure 1.2`).

## Aptitudes propuestas (menú para decidir)

Aptitudes actuales: `agility`, `perception`, `strength`, `bodyMass` (atributo), y
`adaptability` (añadida para Gohageneis). Propuesta de las que faltan, por familia:

**Físicas**
- **endurance / resistencia** — esfuerzo sostenido (turnos largos, yoga, trabajo). Recomendada.
- **flexibility / flexibilidad** — directamente relevante para asanas. **Ojo: ya existe parcialmente**
  en `BodyPartStats` (dimensión `flex` por extremidad); conectar ahí, no duplicar.

**Cognitivas / mentales**
- **reasoning / lógica** — la "lógica de negocio" de Panterilia; razonamiento y planificación.
- **memory / memoria** — velocidad de aprender y retener (podría alimentar el desbloqueo por aprendizaje).
- **creativity / imaginación** — ligada al rasgo mental de Panterilia (exageración, ideas de terceros).

> **Distinción práctica vs académica:** `perception` es atención **práctica/sensorial** (fijarse en
> detalles del trabajo, del entorno); `reasoning`/`memory` son el eje **académico/abstracto**. Un
> personaje puede ser muy observador en lo práctico y flojo en lo académico — es el caso de Goluis
> (percepción alta, razonamiento/memoria bajos).

**Conductuales / meta**
- **adaptability / versatilidad** — ✅ añadida (velocidad de adaptación a tareas/contextos nuevos).
- **composure / temple** — ✅ añadida (funcionar bajo peligro/estrés; Goluis alto por su pasado en una
  banda, Panterilia bajo → se bloquea). Distinto del estrés-estado: es la *capacidad* de no colapsar.
- **sociability / sociabilidad** — bartender/ventas/fiesta; podría modular crecimiento de bond y trato con NPCs.
- **discipline / constancia** — mantener rutinas (Panterilia estudia sin parar; Goluis trabaja sin parar).

> **Adoptado (2026-07-10):** implementadas como datos en `CompanionBase`: `endurance`, `reasoning`,
> `memory`, `creativity`, `sociability`, `discipline` (además de agility/perception/strength/bodyMass/
> adaptability/composure). `flexibility` **no** se duplica: vive en la dimensión `flex` de
> `BodyPartStats` (asanas) y se entrena con yoga. Falta el bucle que las hace **evolucionar**.

> **No confundir con estado emocional:** `mood`/`stress`/`fatigue`/`satisfaction`/`sleepiness` **no
> son aptitudes** sino estado; ya viven en `IMind`/`IMindSimple`. Las aptitudes son capacidades estables.

Mapeo con lo existente: `perception` ≈ `PlayerStats.observationRadius`; `agility` ≈ `velocity`;
`strength`/`flexibility` ≈ dimensiones de `BodyPartStats`. Unificar al implementar `NPCBase`.

## Evolución de aptitudes

Las aptitudes **cambian con el uso**: crecen con la actividad que las ejercita y decaen con el desuso,
dentro de una banda alrededor de su base. Motor: `AptitudeEvolution.Step(current, base, useSignal, dt)`.

- **Animales (implementado):** tick en `Animal.Restore()`. La **agilidad** sube con la intensidad de
  movimiento (`nav.velocity / run.navSpeed` → correr/perseguir/huir la ejercita) y decae en reposo; la
  **percepción** sube mientras el animal está alerta (`aware`, respondiendo a amenazas) y decae si no.
  `sensibility` sigue a la percepción evolucionada.
- **Humanoides (pendiente):** enganchar a tareas/misiones — cada tarea entrena ciertas aptitudes al
  completarse (trabajo físico → `strength`/`endurance`; estudio/observación → `reasoning`/`memory`/
  `perception`; variedad de tareas → `adaptability`; sedentarismo → decae). Requiere unificar las
  aptitudes en el portador que ejecuta tareas (`NPCBase`) o puentear desde `WorldCharacter.ApplyTaskEffects`.
- **Calibración:** `gainRate`/`decayRate` y la banda `[0.7×, 1.6×]` del base son parámetros por afinar.

## Modificadores de medio (tierra / agua / aire)

El **rendimiento físico** depende del medio en que está la criatura: un gato es ágil en tierra pero
torpe en el agua; una ballena, al revés. Se modela como un **multiplicador por afinidad de medio**:

    valor efectivo = aptitud base × afinidad(medio actual)

- **Código**: `Medium { Land, Water, Air }`; en `LivingEntity`: `currentMedium`,
  `LandAffinity`/`WaterAffinity`/`AirAffinity` (virtuales, override por especie), `MediumFactor` y
  `EffectiveAgility` (= `agility × MediumFactor`). Las mecánicas físicas deben leer el valor
  **efectivo**, no la aptitud base.
- **Afinidades calibradas**: por defecto terrestre (`Land 1.0`, `Water 0.4`, `Air 0`). Ballena
  `Water 1.0 / Land 0.1`; Foca (anfibia) `Water 1.0 / Land 0.6`.
- **Alcance**: hoy afecta a `agility`; a futuro también `strength`/`endurance` (todo lo locomotor/físico).
  Percepción y cognición no se escalan por medio salvo casos concretos (p. ej. ecolocación).
- **Detector**: `MediumZone` (trigger sobre volúmenes de agua) fija `currentMedium` al entrar/salir
  de cualquier `LivingEntity` (generaliza el `WaterZone` del jugador).
- **Pendiente**: comportamiento agua/tierra (evitar/entrar según afinidad, ver
  [`refuge-and-adult-behavior.md`](refuge-and-adult-behavior.md)); afinidades para humanoides al nadar
  (`NPCBase`); extender el factor a `strength`/`endurance`.

> **Ahogo / asfixia (solo documentado, sin implementar):** permanecer en un medio de afinidad muy baja
> más allá de una tolerancia debería causar daño progresivo (la ballena varada en tierra; un terrestre
> demasiado tiempo bajo el agua). Futuro; requiere un temporizador de tolerancia por medio.

## Pools derivados de las aptitudes (Mesocosmos)

Los pools de acción/combate **se derivan de las aptitudes**, no se ponen a mano (docs
[`world-topology-and-planes.md`](world-topology-and-planes.md) §4.1). Un personaje con más resistencia
y fuerza tiene más vida; uno con más razonamiento, más maná. Primer consumidor en código:
`CharacterLevel` (farming). Coeficientes iniciales **ajustables**.

| Pool | Para qué | Aptitudes principales | Regen / notas |
|---|---|---|---|
| **Vida (HP)** | aguantar golpes | `endurance` + `strength` + `bodyMass` | tanque = resistente/fuerte/con cuerpo |
| **Energía** | asanas, correr, **trepar** | `endurance` + `agility` − penalización por `bodyMass` | regen por `discipline` |
| **Maná** | hechizos | `reasoning` + `memory` | regen por `discipline` + `composure` |
| **Defensa pasiva** | resta al daño recibido | `bodyMass` + `strength` + `composure` (+ armadura) | + equipo (`ItemData.damageReduction`) |
| **Poder de hechizo** | daño/efecto mágico | `creativity` + `reasoning` + maestría de elemento | escala con nivel |

**Por qué maná y energía así (respuesta de diseño):**
- **Energía = pool FÍSICO.** Base en `endurance` (esfuerzo sostenido) + `agility` (eficiencia de
  movimiento), penalizada por `bodyMass` (más peso = más coste). Alimenta posturas de asana, correr,
  trepar. La `discipline` mejora la regeneración.
- **Maná = pool MENTAL/MÁGICO.** La magia aquí es intelectual (tabla periódica / encantamientos), así
  que `reasoning` da el tamaño del pool y `memory` el repertorio retenido (liga con learning-unlocks);
  `discipline`/`composure` estabilizan el canal (regen y no fallar bajo presión); `creativity` va al
  **poder** del hechizo más que al pool.

**Trepar (mecánica concreta):**
- Puede trepar si `strength` (y `agility`) superan un umbral.
- **Altura máxima ∝ `strength` / `bodyMass`** (potencia-peso): más fuerza y menos peso → más alto.
- **Velocidad vertical ∝ `strength`·`agility`**; por ramas horizontales, ∝ `agility`·`flexibility`
  (equilibrio).
- **Coste por escalón (energía) ∝ `bodyMass` / `strength`** (pesado/débil = agota antes).
- Como las misiones dan aptitudes, todos acaban trepando alto y con facilidad al progresar.

**Misiones que dan aptitudes:** extender la recompensa (`MeditationReward` / recompensa de `MobMission`)
para otorgar aptitudes (fuerza/resistencia/razonamiento…), no solo observación/monedas. Es el enganche
que faltaba del lado humanoide de la evolución de aptitudes, y lo que hace crecer los pools derivados.

> **Integración:** la derivación limpia necesita un acceso unificado a las aptitudes (hoy viven en
> `CompanionBase`, parcialmente en `PlayerStats`/`LivingEntity`); encaja con el `NPCBase` pendiente. Un
> módulo `DerivedStats` (funciones puras aptitudes→pools) puede escribirse ya y ser consumido por
> `CharacterLevel` y el futuro `NPCBase`.

## Progresión: aptitudes universales, niveles y pools de XP

**¿Más stats?** De momento **bastan las 12** (+ `flexibility` en `BodyPartStats`). La profundidad mágica
NO se modela con stats nuevas, sino con **pools derivados** (maná = `reasoning`+`memory`) y **tracks de
progresión aparte** (maestría de asana, maestría de elemento). Mantener el set pequeño y con sentido.

**Base + ganancias.** Cada aptitud = **base** (con la que nace el personaje) + **ganancias** (puntos/
multiplicadores por practicar/misiones). Los pools se derivan de la aptitud efectiva (§Pools derivados).

**Nivel del "alma" por pool de XP.** Cada **punto de aptitud** ganado suma también a un **pool de XP**.
Al alcanzar el umbral → **sube de nivel**: recompensa/desbloqueo **notorio** + un **incremento directo
de la base** (crecimiento íntegro, no temporal — "un proceso del alma").

**Varios tracks de nivel** (no uno solo):
- **Nivel de personaje/alma** — por ganancia total de aptitudes.
- **Maestría por asana** — *ya existe* (`Asana.masteryLevel`/`RegisterPractice`): cada postura tiene su
  XP y niveles.
- **Maestría de elemento** — *ya existe* el descubrimiento (Chemistry/tabla periódica).

**Maná latente → se DESBLOQUEA con el yoga.** Al principio no es visible; practicar yoga (asanas) lo
revela y lo hace crecer. Igual pueden desbloquearse habilidades/nuevas asanas.

**Niveles por plano.**
- **Mesocosmos = autoritativo:** las reglas profundas (ganancia de aptitudes por hacer, pools derivados,
  maestrías). Es el modelo "real".
- **Macrocosmos / Microcosmos = pueden ABSTRAER:** si el rendimiento lo exige, un nivel+XP simplificado
  (proxy) en vez de simular todo. Meso manda; macro/micro optimizan.

**Estado en código.** `CharacterLevel` (farming) hoy usa un XP→nivel **arbitrario** con pools ya
derivados de aptitudes. Para alinearlo: el XP debe **venir de la ganancia de aptitudes** (no arbitrario)
y el nivel debe **subir la base**. Los asanas ya tienen su track. Falta: misiones de **simulacro** que
den **aptitudes** (hoy `SanctuaryMission` solo da monedas/item; solo `AreaTask.strengthDelta` mueve la
fuerza de los `WorldCharacter` autónomos).

## Próximos pasos

1. **Evolución de aptitudes** — *animales hecho* (ver §Evolución de aptitudes); **falta el lado
   humanoide**: engancharla a tareas/misiones.
2. Conectar aptitudes a mecánicas: `agility` → velocidad/maniobra; `perception` → radio de
   detección/observación y calidad de asana; `strength` → daño/carga; `bodyMass` → física/saciedad.
3. Unificar con `PlayerStats` y consolidar en `LivingEntity` al implementar `NPCBase`.
4. Modelar el rasgo mental de Panterilia (exageración / influencia de terceros).
