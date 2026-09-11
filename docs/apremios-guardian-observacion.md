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

### 3.1. Base científica de los UMBRALES y los gastos/fuentes (respuesta a "¿qué influye?")

El modelo NO es arbitrario: es el de la biología real (**alostasis/carga alostática** + **presupuesto energético**).
- **El Guardián = ALOSTASIS.** La ciencia llama *allostasis* a mantener la estabilidad **prediciendo y anticipando**
  (no solo reaccionando); la **carga alostática** es el **coste acumulado** cuando la regulación del estrés compite con
  crecer/mantener/reparar, y la **sobrecarga** deriva en hipermetabolismo → declive/envejecimiento. → El "sufrimiento"
  que emite el Guardián **es** la carga alostática; la Observación (§4) la **reduce**.
- **Los GASTOS diarios = presupuesto energético (TEE) = BMR + AEE + Termorregulación:**
  - **BMR** (metabolismo basal, solo por estar vivo) escala con la **masa** por la **ley de Kleiber** (`BMR ∝ masa^0.75`):
    un cuerpo grande gasta más en absoluto pero menos por gramo. → el umbral/gasto base sale de la **masa corporal** (y
    de la **anatomía**: más órganos/miembros = más BMR; branquias≠pulmones cambian el coste por medio).
  - **AEE** (actividad) = **`Exertion`, que YA existe** (correr/hacer fuerza gasta glucosa/minerales/fatiga/sueño).
  - **Termorregulación** = el **entorno** (temperatura/medio agua-aire): estar en un medio de baja afinidad cuesta
    extra (enlaza con `MediumFactor`/`Suffocate`, ya existentes). → el **entorno** entra por aquí.
- **Las FUENTES = alimentación** (`Metabolism`: nutriente→elemento/energía→reservas). El **estilo de vida** y la **dieta**
  fijan cuánto y cómo se repone.
- **Resumen accionable:** los umbrales seguros de un ser derivan de **masa (Kleiber) + anatomía + medio**; el **gasto
  diario** = BMR + AEE (`Exertion`) + termorregulación; las **fuentes** = comer (`Metabolism`). El Guardián compara
  reservas vs gasto y, al cruzar el límite, emite carga (estrés/emoción/químico). *(Todo esto ya tiene piezas en el
  código: `Exertion`, `Humores`, `MoodDynamics`, `Metabolism`, `MediumFactor` — el Guardián las unifica bajo Kleiber+alostasis.)*

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

## 8. Los STATS MENTALES — qué tenemos y el esquema científico (respuesta a "¿hay más?")

**Lo que YA hay en el código:**
- **Estados mentales** (`IMind`): `satisfaction`(+capacity), `mentalFatigue`, `stress`, `sleepiness`, `observationRadius`.
- **Aptitudes mentales** (de las 12 de `IAptitudes`): `reasoning`, `memory`, `creativity`, `sociability`, `discipline`,
  `composure`, `perception`, `adaptability` (+ `afabilidad`/`sensibilidad` de `emotion-model.md`).
- **Emoción** (`EmotionExpression`): **circumplejo** valencia×activación (+ Laban) — la base científica del afecto.
- **Químicos** (`Humores`): Adrenalina/Serotonina/Cortisol/Glucosa/Calcio.

**El esquema científico (3 ejes que la psicología usa; encajan con lo nuestro):**
1. **AFECTO = PAD (Pleasure-Arousal-Dominance, Mehrabian-Russell).** Ya tenemos Placer(valencia)+Activación(arousal);
   **falta la 3ª dimensión: DOMINANCIA** (agencia/control percibido sobre la situación). **Recomiendo añadirla** — es
   barata y **temáticamente central** (control, posesión, el hechizo de miedo de la Magnate, la sumisión de Ruth vs el
   mando de Héspero). Y **la Observación ≈ dominancia sobre los PROPIOS apremios** (autorregulación): mirar es ganar
   agencia sobre el estado interno → cierra el círculo con §4.
2. **PERSONALIDAD = Big Five (OCEAN).** Ya está **casi** mapeada por las aptitudes: Apertura≈`creativity`,
   Responsabilidad≈`discipline`, Extraversión≈`sociability`, Amabilidad≈`afabilidad`, Neuroticismo≈inverso de `composure`.
   → No hacen falta stats nuevos; **formalizar el mapeo** (perfil Big Five derivado de aptitudes) da personalidad legible.
3. **COGNICIÓN = CHC (Cattell-Horn-Carroll).** Es la taxonomía más validada (10 amplias, 70+ estrechas; las funciones
   ejecutivas quedan subsumidas). Nuestras `reasoning`(≈Gf fluido), `memory`(≈Gsm/Glr), `perception`(≈Gv/Ga) ya cubren lo
   grueso; posibles añadidos amplios: **velocidad de proceso (Gs)** y **conocimiento/cristalizado (Gc)**. → **Recomiendo NO
   explotar** en 70 sub-habilidades; mantener el set amplio y usar CHC como **referencia** para profundizar si hace falta.

**Decisión propuesta:** (i) añadir **Dominancia** al afecto (PAD completo) y ligarla a la Observación/autorregulación;
(ii) derivar un **perfil Big Five** de las aptitudes (sin stats nuevos); (iii) tratar la cognición con el set amplio
actual, CHC como norte. Así "hay más" pero **sin inflar** — se ordena lo que ya existe bajo marcos reales.

Fuentes: allostasis/carga alostática ([PubMed](https://pubmed.ncbi.nlm.nih.gov/36302295/), [ScienceDirect](https://www.sciencedirect.com/science/article/pii/S030645302200292X)),
Kleiber ([Wikipedia](https://en.wikipedia.org/wiki/Kleiber%27s_law)), PAD ([Wikipedia](https://en.wikipedia.org/wiki/PAD_emotional_state_model)),
Big Five ([Wikipedia](https://en.wikipedia.org/wiki/Big_Five_personality_traits)), CHC ([Wiley](https://onlinelibrary.wiley.com/doi/full/10.1002/9781118660584.ese0431)).
