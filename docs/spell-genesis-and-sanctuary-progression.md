# Génesis de hechizos + progresión por santuarios (misiones, cocina, planta atómica)

> Diseño (2026-09-27). La **ley que unifica los hechizos** (todo hechizo = una acción natural llevada al
> extremo), **comer como hechizo**, y cómo el jugador **progresa por misiones** (no por gate duro) con el
> **restaurante y la planta atómica presentes en todos los santuarios**. Enlaza con
> [`magic-metabolism-progression.md`](magic-metabolism-progression.md) (comer→compuestos→elementos→quarks),
> [`stats-as-truth.md`](stats-as-truth.md) (todo son stats), [`creature-stats.md`](creature-stats.md)
> (margas/nivel integral), [`capabilities-and-embodiment.md`](capabilities-and-embodiment.md) (capacidad=hechizo).

---

## 1. LEY DE GÉNESIS: todo hechizo es una acción natural llevada a intensidad extrema

Ya estaba decretado que **todas las acciones son hechizos** (caminar/correr/jalar/comer son `SpellBase`).
La idea completa que faltaba enunciar: **todos los hechizos PROVIENEN de una acción natural y cotidiana**,
subida a una **intensidad extrema**. No hay magia "aparte": hay lo mismo de siempre, exagerado.

- **Escalera de intensidad = ARRAY (no uniforme, insertable).** Cada familia de hechizo tiene una lista de
  **peldaños** por intensidad, no niveles fijos globales. Ej.: `caminar → correr → (¿intermedio?) →
  teletransporte`. Usar un **array** permite (a) que cada hechizo tenga sus propios cortes, y (b)
  **descubrir/insertar intermediarios** (un peldaño entre correr y teletransportarse) sin tocar a los demás.
  → Estructura sugerida: `SpellLadder { string action; Rung[] rungs }`, `Rung { float intensity; string spellId; string[] requires }`.
- **Teletransporte** = la acción natural **caminar** a intensidad altísima (caminar → correr → … → teleport).
- **Hechizos COMPUESTOS (producto de otros):** algunos hechizos nacen de **combinar** otros muy elevados.
  Ej.: teletransporte = `caminar + meditación`; o `caminar + meditación + asanas`; o `caminar + meditaciones
  + asanas + músculos + neuronas`. Un hechizo mágico puede tener **varias recetas** (varios caminos que lo
  desbloquean). → `Rung.requires` = lista de hechizos/entrenamientos previos a cierto nivel.
- **Desbloqueo PATH-DEPENDENT (la aventura):** como en el mundo se puede hacer de todo, cada anima desarrolla
  lo que **entrena a lo largo de su vida** — piernas, nado, carga, carrera, socialización, pelea, razón — vía
  **yoga, meditación, trabajo de cuerpo/mente**. Ese camino desbloquea uno o varios hechizos. Dos animas
  distintas llegan al mismo hechizo mágico **por rutas distintas** (o no llegan). Es la aventura del juego.

> **Estado:** el modelo de "acción=hechizo" y la locomoción-hechizo (`WalkSpell`) EXISTEN; la **escalera por
> array + hechizos compuestos + desbloqueo por camino** es **DISEÑO por construir** (`SpellLadder`/recetas).

---

## 2. COMER como hechizo (descomposición por intensidad)

Hoy comer es `Forager`(Hunt/Graze) + `Metabolism` + `Eater` (cocina) — **no es aún un hechizo casteable**.
La acción natural "comer/masticar" debe volverse un **`EatSpell`** que, **según su intensidad (stats del
lanzador)**, **descompone al anima objetivo en sus componentes** y los absorbe:

| Intensidad | Descompone en | Cocina/lugar (magic-metabolism §3) |
|---|---|---|
| baja | **platillo** (comida preparada) | cocina/restaurante |
| media | **compuestos** (proteínas/lípidos…) | tienda de ingredientes |
| alta | **elementos** (átomos) | laboratorio |
| extrema | **quarks** | planta atómica |

- **Cualquier anima lo lanza**; hasta dónde descompone depende de sus **stats/hechizos** (no del santuario).
- **Doble efecto:** sobre los **gramos** del objetivo (lo consume) **y** sobre los **átomos/energía obtenidos**
  (llena las pools bio/elemento/quark/energía, ver magic-metabolism §15). "Todas las cosas son animas" → el
  `EatSpell` descompone **cualquier** anima (planta, roca, cuerpo) a su nivel de intensidad.
- Es la **misma escalera** que ya documenta el arco mago (S1 platillos → S4 quarks); aquí se unifica: la
  progresión mago **es** subir la intensidad del `EatSpell`.

> **Estado:** DISEÑO. `Metabolism`/`Constitution`/`DecompositionMinigame`/`MagicReserves` ya son la base;
> falta el `EatSpell : SpellBase` que envuelva Hunt/Graze y elija nivel de descomposición por stats.

---

## 3. PROGRESIÓN POR MISIONES (no por gate duro)

Antes se dijo "para pasar de santuario hay que superar un mínimo de habilidades". Refinamiento: **se deja al
jugador en un santuario resolviendo misiones** hasta que, de forma **natural**, consigue los hechizos que le
permiten subir. Mecánica:
- **Cada misión = un paquete de PUNTOS** que alimenta las **margas del alma** (nivel integral, §6). Al
  reunir cierta cantidad se **dispara el evento de cambio de santuario**.
- **Yoga y meditaciones**: hoy planteadas como **acciones libres** una vez aprendida una asana/meditación
  (`UpaYogaSession`). Pueden ser también **misiones** (autoimpuestas). → cada una es un paquete de puntos.
- **Push-ups/abs y otros ejercicios:** se plantearon pero **NO están implementados** (solo yoga/asanas/
  meditación). Decisión abierta ⚠️: ¿añadir ejercicios de fuerza como acciones/misiones, o dejar que el
  entrenamiento físico salga del **trabajo/farming/acciones de la vida** (cargar, trepar, correr)?
- Las **misiones suben según los hechizos del jugador** (ver §5): la misma misión "cocinar" da más o hace
  más si el jugador es más capaz.

---

## 4. RESTAURANTE **y** PLANTA ATÓMICA en TODOS los santuarios (corrección)

**Corrección de diseño:** NO se cambia "el restaurante de S1 por una planta atómica". El **restaurante
sigue existiendo** en todos los santuarios (incl. S4-N1) **y** se **añade una planta atómica en todos**
(o el restaurante alberga ambas mesas). Lo que cambia es **qué misiones ofrece según el desarrollo del
jugador**, no el lugar:
- **S1-N1**: se venden **platillos súper nutritivos**; misiones de **cocinar**.
- **S4-N1**: se venden **quarks puros** (para hechiceros que los consumen directo); misiones de **descomponer
  animas en quarks** — PERO el mismo sitio ofrece, a quien no sepa, las **misiones iniciales que no resolvió**
  (cocinar, descomponer en compuestos/elementos).
- Un hechicero que llega a S4 **sin** el hechizo de descomponer a quarks es **dependiente de comprarlos**, y
  sus misiones disponibles son las **básicas** (cocina), no las de quarks.
- **Varios caminos desbloquean el mismo hechizo** (§1): p. ej. la **Enfermería** —que maneja compuestos y
  elementos (suplementos de vitaminas/minerales, análisis químico de la salud del anima)— desarrolla en el
  jugador la capacidad de **descomponer animas en compuestos y elementos** conforme avanza; llevado al
  extremo, misiones fantasiosas de **curar con átomos/quarks**. Así un jugador que solo hizo Enfermería puede
  llegar al mismo punto que uno de cocina/planta.
- **Separación limpia (opción recomendada):** reservar la **descomposición a quarks** a la **planta atómica**
  (presente en todos los santuarios) y dejar el **restaurante** para **cocina** (platillos). Ambos coexisten
  en cada santuario; el jugador elige.

### 4.1 "Cuanto más preparada, más provecho" (refrán del juego)
Mensaje del juego: **un anima, cuanto más preparada, más provecho saca de cualquier acción.** Consecuencias:
- Un mago capaz de **descomponer a quarks** que va a **cocinar** recibe **muchísimos** puntos de magia (su
  precisión extrae más). 
- Puede **purificar alimentos**: los animales llegan con **toxinas** → el mago las **descompone** dentro del
  platillo → aplica su capacidad de alto nivel **mientras hace una misión inicial** de cocina.
- Por eso un **fan de la cocina** puede pasar **todo el juego cocinando** y aun así crecer.

---

## 5. Evolución de un área por su HISTORIA (planta atómica y restaurante)

Se mantiene la ley: **un área del santuario evoluciona según la HISTORIA de lo que representa** (para que el
jugador aprenda los conceptos de forma **experimental**, viviendo cómo la humanidad los descubrió).

### 5.1 Planta atómica (aprender física nuclear viviendo su historia)
Arco propuesto (de rudimentario a sofisticado; de un científico a muchos):
1. **Un estudioso curioso** observa que ciertas piedras "velan" placas → **radiactividad** (Becquerel) →
   misiones básicas: **limpieza de maquinaria, mantenimiento**, medir.
2. **Aislar y medir** lo que emite (Marie & Pierre Curie: polonio/radio) → misiones de **separar/identificar**.
3. **El átomo tiene núcleo** (Rutherford) → romper con precisión.
4. **Reacción en cadena controlada** (la "pila" de Fermi) → el primer **reactor**; misiones de **experimento**
   controlado (evitar el "meltdown" del `DecompositionMinigame`).
5. Reactores modernos → **descomposición total a quarks** (el nivel S4).
El jugador **empieza con un instrumento rudimentario** y va **implementando** aparatos más sofisticados según
avanza — cada avance histórico = un peldaño de misión y un concepto aprendido.

### 5.2 Restaurante (gourmet + conciencia)
Arco: **poner comida al fuego / mezclar frutas y vegetales** → **agua y sopas en caldero** → **horneado,
frito, fermentación** → platillos **difíciles con ciencia** (gastronomía moderna). El eje no es la violencia
sino **gourmet + concientización**: platillos **nutritivos, deliciosos y buenos con el medio
ambiente/animales/plantas**. Se pueden **elegir restaurantes por tipo de platillo**. (La descomposición a
quarks se reserva a la planta atómica, §4; el restaurante enseña **nutrición y cocina consciente**.)

---

## 6. Nivel integral y capacidad de las pools (recordatorio, verificado en código)

- **Nivel integral = margas del alma** (`CharacterLevel`, `Assets/Scripts/Progression/`): tres tracks
  independientes **Stats / Yoga / Vínculos** (`SoulMarga`). `SoulLevels` = suma de niveles ganados en TODAS
  las margas; **cada nivel de cualquier marga** sube los "puntos del alma".
- **Capacidad de las pools de magia** (`MagicReserves`): **escala con los stats y con las margas** —
  `EffectiveCapPerElement = capPerElement × MaxHealth/100`; `EffectiveEnergyCap = energyCap × MaxMana/50`,
  donde `MaxHealth`/`MaxMana = DerivedStats(aptitudes, SoulLevels)`. Los **quarks no tienen tope**.
- **Cómo se sube hoy:** XP de **farming → marga Stats** (`GainXp`). **Yoga y Vínculos aún no dan XP** (están
  por cablear). La barra de **maná se desbloquea** al practicar yoga (marga Yoga ≥ 2).
- **Palanca que pide el usuario ("las misiones suben en base a los hechizos / cada misión da puntos"):**
  hacer que **cada misión otorgue XP a la(s) marga(s)** correspondiente(s) (cocina/enfermería/planta → Stats;
  yoga/meditación → Yoga; convivencia → Vínculos). Así **misiones → margas → SoulLevels → capacidad de pools**
  y el evento de cambio de santuario (§3). → **DISEÑO por cablear** (falta el `GainXp` desde las misiones).

---

## 7. Hechizo de COMUNICACIÓN (todas las animas hablan)

Todas las animas deben poder **hablar entre sí** → **hechizo de comunicación** (su acción natural =
vocalizar/expresar; la `Mind` ya *piensa*, §campo de pensamientos). Con él, dos animas cercanas mantienen
**charlas esporádicas** ligadas al **campo de pensamientos** y a los **pensamientos nativos** de cada una
(mente humanizada, ver [`microcosmos-eras-and-observation.md`](microcosmos-eras-and-observation.md)). En el
mundo insecto (humanoides) esto se ve como diálogo; mecánicamente = intercambiar/mostrar pensamientos.
> **Estado: CONSTRUIDO (2026-10-06, `CommunicationSpell : SpellBase`).** Pasivo: cada rato esporádico empareja
> con el anima cercana **más vinculada** que tenga `Mind` y **alternan un pensamiento** (`Mind.SpeakNow`) como
> charla (`[Charla] A ↔ B`). Hay **contagio emocional** (la valencia del que habla mueve los humores del que
> escucha, **×vínculo** — emoción compartida real) y charlar en positivo **crece la amistad** (`GrowBond`). Su
> acción natural = expresar; opt-in = es un hechizo **aprendido** (no innato), coherente con el desbloqueo por
> camino. *Siguiente:* frases de charla específicas (responder al pensamiento del otro, no solo alternar), y
> emparejar por iniciativa mutua para turnos de diálogo más largos.

---

## 8. Campo de pensamientos conectado al entorno y al estado (CONSTRUIDO 2026-09-27)

La `Mind` ya no piensa solo "hacia dentro": **percibe las animas cercanas** (todo es anima, hasta el suelo)
y sus **propias necesidades**, y eso **tiñe sus humores** → de ahí salen el **tono** (PickTone) y la
**valencia** (Think) de sus pensamientos. Amiga cerca → calma (serotonina); depredador no vinculado →
cautela (adrenalina/cortisol); presencia neutra → pertenencia; hambre/sueño/estrés altos → malestar. Así
los pensamientos **nacen del entorno + del estado físico + del peligro + de los vínculos**, además de los
propios (`Mind.SenseSurroundings`, radio `awarenessRadius`). Es el sustrato del hechizo de comunicación (§7)
y de las charlas del nivel de los peloteros. *Siguiente:* generar frases-contenido específicas por
situación (ver a X → pensar sobre X), no solo teñir tono/valencia.
