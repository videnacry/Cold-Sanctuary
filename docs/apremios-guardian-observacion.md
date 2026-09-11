# Apremios, el Guardián de stats y la Observación (ecuanimidad)

> **Diseño de subsistema (base de toda la saga).** Propuesta a acordar antes de implementar. Nace de la sesión de
> diseño del nivel de origen de Sakshi (ver [`microcosmos-sakshi-origin.md`](microcosmos-sakshi-origin.md)), pero es
> **general**: modela CÓMO un ánima siente y se libera de sus pulsiones internas. Encaja con lo ya construido:
> `MoodDynamics` (estrés químico), `EstrusState`/`SicknessState` (hechizos-estado que depositan un canal), la
> evolución de aptitudes por uso (`AptitudeEvolution`), `MindChannel.Observation`, `EmotionExpression`, el `Volition`
> (deseo = necesidad×capacidad×confianza) y los `Impulse*` del Microcosmos.

## 1. El problema y el vocabulario

Un ánima camina un sendero marcado por sus **pulsiones internas**: hambre, miedo, duelo, fatiga, sueño, frío, celo…
Hoy el código las tiene dispersas (`hungry`, `stress`, `trauma`, `EstrusState`, `SicknessState`, los `Impulse*`). Falta
un **modelo unificado** de esa presión y, sobre todo, de la **libertad** frente a ella (la "rara forma de ser" de
Sakshi: pararse de golpe a **observar**, con asombro/curiosidad, en vez de ser arrastrada por la necesidad).

**Nombre (decisión):** llamamos a esas presiones **APREMIOS**. Un apremio = una exigencia interna que **presiona** al
ser (≈ *drive/urge*). Se distingue del **impulso** de locomoción (`MovementImpulse`, un vector de dirección ya existente):
el impulso es el "hacia dónde piso"; el **apremio** es el "qué me exige y cuánto me duele". *(Alternativas descartadas:
"influencia" — demasiado suave; "dictado" — evocador pero raro en código. Si se prefiere otro término, es un rename.)*

## 2. El APREMIO (la pulsión)

Un apremio tiene:
- **fuente** (hambre / miedo / duelo / fatiga / sueño / frío / celo / …),
- **intensidad** `[0,1]` (cuánto exige) — derivada del estado (p.ej. hambre = `Metabolism.Appetite`),
- **un sesgo de conducta**: sube el peso del deseo que lo satisface en el `Volition` (hambre → deseo "comer"),
- **una carga de sufrimiento**: cuánto **estrés/emoción negativa** produce si no se atiende (lo genera el Guardián, §3).

**Implementación propuesta:** los apremios son **hechizos-estado pasivos**, igual patrón que `EstrusState`/`SicknessState`
(componentes que cada tick leen el estado y "depositan" su señal). Un `Apremio` deposita su intensidad en un
**registro de apremios** del ser (un pequeño diccionario `Apremio→intensidad`), que leen el `Volition` (sesgo) y el
Guardián (sufrimiento). Ventaja: reusa un patrón ya validado y es aditivo.

> Clave del modelo del usuario: **el apremio y el sufrimiento son cosas distintas.** Un ser puede tener MUCHA hambre
> (apremio alto → busca comida con todas sus fuerzas) y a la vez **sufrir poco** por ella (si observa / está sereno).
> La Observación (§4) **no baja el apremio** (sigue teniendo hambre); baja el **sufrimiento** que el apremio produce.

## 3. El GUARDIÁN de stats (homeostato)

> Idea del usuario: "un *guard* que genera estrés/emociones/químicos cuando el agotamiento de un stat rebasa los
> límites seguros; un hechizo podría relajar/ampliar esos límites o dormirlo".

**Ya existe a medias:** `MoodDynamics` sube el estrés (cortisol) cuando hambre/fatiga/sueño/reservas se salen de rango.
El Guardián es esa idea **generalizada**:
- Vigila cada **stat vital** (energía, masa/reservas, integridad, temperatura, descanso…) contra un **rango seguro**
  `[min,max]` por especie/individuo.
- Cuando un stat **rebasa** el límite, emite una **carga** proporcional al exceso: **estrés** (cortisol) y/o la emoción
  correspondiente (miedo, ira, tristeza) y/o un **químico** (adrenalina). Esa carga es el "sufrimiento" del §2.
- Es **el mismo actuador de emoción** que ya alimenta `EmotionExpression` (valencia/activación/tensión).

**Hechizo que actúa sobre el Guardián:** un hechizo activo (o un químico) puede **ampliar los límites** o **adormecer**
al Guardián → el ser deja de sufrir por un stat en rojo (el estupefaciente; también el "modo supervivencia" que ignora
el dolor). Riesgo/coste: sin Guardián, el ser puede dañarse sin avisar (se usa para bien —ecuanimidad— o para mal
—adicción/autolesión—).

## 4. La OBSERVACIÓN (ecuanimidad = libertad del apremio)

> Idea del usuario: "cierta paz/energía que permite observar cualquier ánima que tengas delante; si las
> influencias se paran o debilitan, el personaje puede concentrarse en lo que hace o en el ánima con la que interactúa".

**Definición:** la Observación es un **amortiguador del sufrimiento** que el Guardián produce. No apaga los apremios
(el hambre sigue empujando a comer), sino que **reduce su carga emocional** y **libera atención** → el ser puede
CONCENTRARSE (en la caza, el pasto, el sueño, el movimiento, un combate… **siempre hay un ánima delante**: una parte del
cuerpo, un plan, una presa, una estrategia) o **observar** algo al azar con asombro/curiosidad.

**De qué se nutre (los reductores del sufrimiento que enumeró el usuario, unificados):**
1. **Stats de temple** — `composure` (+`discipline`, `reasoning`): un ser sereno amortigua más. → *favorece la observación*.
2. **Químicos** — sueño alto, calma/narcótico: adormecen al Guardián. → *reduce el impacto directo*.
3. **Apremio dominante** — un apremio tan alto **eclipsa** a los demás (el que caza con hambre feroz no siente el frío).
   → *un impacto tan alto quita importancia a los otros*.
4. **Stat holgado** — un stat tan por encima del mínimo que su apremio ni cuenta. → *el stat tan alto que no importa*.
5. **Mirar sostenido** — el **hechizo pasivo de los ojos** (§5): fijar la mirada en un ánima genera Observación **y**
   **sube el stat de observación por uso** (como `AptitudeEvolution` ya sube agilidad/percepción con el uso).

**¿Cómo se calcula?** Propuesta: una **aptitud propia** `observacion` que:
- **base** = función de otros stats (`composure`, `reasoning`, `discipline`) modulada por químicos (sueño/calma) — así
  "es la suma de otros stats" como intuías, pero con su propia identidad evolutiva;
- **evoluciona por uso** (mirar sostenido, meditar) — sube despacio;
- **efecto**: multiplica a la baja la carga que el Guardián convierte en estrés/emoción (`sufrimiento = carga × (1 −
  k·observacion)`), y ensancha un poco los límites del Guardián. El apremio (sesgo de conducta) **no** se toca.

**Relación con el jugador:** el jugador ya tiene `observationRadius` + `MindChannel.Observation`; esto **generaliza** esa
noción a TODA ánima y le da consecuencia mecánica (no solo un número del HUD).

## 5. Hechizos por PARTE DEL CUERPO (los ojos observan)

`CharacterComposition.grants` ya **gatea hechizos por anatomía** ("receptor visual/auditivo…"). Encaja de forma nativa:
- Los **ojos** conceden el **hechizo pasivo `Observar`**: se activa al **mantener la mirada** sobre un ánima (o sobre la
  tarea actual) el tiempo suficiente → produce Observación (§4) y sube el stat por uso.
- Generaliza: otras partes conceden otros pasivos (oídos → alerta; antenas → rastro; etc.). Es el mismo gate anatómico.

## 6. Cómo encaja con lo existente (no reinventar)

| Pieza nueva | Reusa / generaliza |
|---|---|
| `Apremio` (hechizo-estado pasivo) | patrón `EstrusState`/`SicknessState`; fuente = `hungry`/`Metabolism.Appetite`/`ThreatScanner`/`trauma` |
| Guardián (homeostato) | **`MoodDynamics` generalizado** (hoy: hambre/fatiga/sueño→estrés) |
| Carga → emoción | `EmotionExpression` (ya publica valencia/activación/tensión) |
| Sesgo de conducta | `Volition` (deseo = necesidad×capacidad×confianza; el apremio = la "necesidad") |
| `observacion` (aptitud) | `AptitudeEvolution` (evolución por uso), `IAptitudes`, `DerivedStats` |
| `Observar` (ojos) | `CharacterComposition.grants` (gate anatómico de hechizos) |
| Adormecer al Guardián | patrón `SpellBase` + `MagicReserves`/químicos |

## 7. Plan por rebanadas (a implementar tras acordar)

1. **Guardián MVP** — generalizar `MoodDynamics` a umbrales por stat; salida = estrés/emoción proporcional al exceso.
2. **Registro de apremios** — un `Apremio` (hechizo-estado) por hambre y por miedo (los que este nivel necesita), que
   depositan intensidad; el Guardián lee de ahí.
3. **`observacion` (aptitud)** — base por stats + efecto amortiguador sobre la carga del Guardián.
4. **`Observar` pasivo (ojos)** — mirada sostenida → Observación + evolución por uso.
5. **Hechizo de adormecer al Guardián** (químico/estupefaciente) — opcional/tardío.

> Regla de la saga (contexto): **todos los personajes siempre vivos**, ≥6 por historia, la misma alma en cuerpos
> distintos (reencarnación). Ver [`microcosmos-sakshi-origin.md`](microcosmos-sakshi-origin.md) y `soul-composition-blend.md`.
