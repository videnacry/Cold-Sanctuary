# Navegación por lectura del entorno (impulsos) — 2026-08-25

> **Diseño.** Unificar la navegación: el overworld (`Animal`) deja de "seguir aves al azar" y pasa a **leer el entorno
> con los sentidos** y dirigirse a lo propicio / lejos de lo peligroso, como ya hacen las hormigas del Microcosmos.
> Reutiliza el sistema de **impulsos** que ya existe (`ImpulseController` + emisores/escáneres); añade las señales que
> faltan (confort/humedad, colegueo, experiencia/memoria) y el engaño. Conecta con sentidos (C), legibilidad
> (`EmotionReader`), medio/afinidad (asfixia) y deseos (D3b: el deseo dice QUÉ busca; esto resuelve DÓNDE).

## 1. Dos mundos de navegación hoy

| | Cómo se mueve | Dónde |
|---|---|---|
| **Microcosmos (hormigas)** | **impulsos**: `ImpulseController` suma vectores ponderados (hogar/miedo/olor/grupo) → NavMeshAgent | `ImpulseController`, `HomeImpulse`, `Scent/ThreatScanner`, `BondEscapeReader` |
| **Overworld (`Animal`)** | `LifeStage.Wander` → `target = ave al azar`; `Flee` → huye hacia un ave al azar | `LifeStage.Wander`, `Animal.Flee` |

El sistema bueno (impulsos leyendo el entorno) **ya existe**; el overworld no lo usa. **La cueva de Ambrosio se logra
así** (`HomeImpulse` a la cueva ×estrés×bond + `BondEscapeReader` + escáneres). Objetivo: que el overworld navegue igual.

## 2. El modelo: percibir → impulsos → dirección

Cada ser suma **impulsos** (atractores/repulsores) que salen de **leer el entorno con sus sentidos**. El vector neto lo
dirige. Un impulso = { dirección, peso, tag, decaimiento }. La **lectura está gateada por sentidos** (C) y por
**legibilidad** del emisor (`EmotionReader`): sin olfato no hueles la comida; sin vista no lees al congénere relajado.

### Tus señales → impulsos

| Señal (tu ejemplo) | Impulso | Estado |
|---|---|---|
| **Comida** (olor) | atractor al `ScentEmitter` de comida/cadáver (vía `ScentScanner`) | **existe** |
| **Refugio / hogar** | `HomeImpulse` (×estrés×bond de grupo) | **existe** |
| **Inexistencia de indicios de depredador** | ausencia de impulso `ThreatScanner` (peligro contextual) → no repulsión = zona apetecible | **existe** |
| **Humedad / clima** (el conejo se guía por la humedad) | atractor a un **campo de CONFORT** por medio/microclima (afinidad de medio, cf. asfixia) | **nuevo** (`ComfortField`/emisor) |
| **Colegueo** (percibir animales que no le dañan, **relajados y a la vista** = "aquí hay comida/refugio y no hay depredador") | atractor a **congéneres/inofensivos CALMADOS** (baja activación, no huyen ni se ocultan) — lo INVERSO de `BondEscapeReader` | **nuevo** (`SafetyReader`; usa `EmotionReader`/legibilidad) |
| **Experiencia** (no volver por donde vino, salvo que ahí vio lo que ahora busca) | **memoria de lugares**: repulsor leve de sitios recién visitados vacíos + atractor a donde vio el recurso buscado | **nuevo** (memoria espacial ligera; cf. `SoulRecord`) |
| **Engaño por cambios bruscos** | los emisores pueden **mentir/cambiar** (un olor que aparece de golpe, una trampa que imita una señal de seguridad) → el escáner reacciona a la señal, no a la verdad | **emergente** (extensión de emisores; "trampas") |

**Colegueo, en detalle:** un animal **relajado y a la vista** (activación baja, sin `aware`/flee, sin ocultarse) es una
**baliza de seguridad** para otros de nivel similar: "si ese come tranquilo aquí, no hay depredador y hay recurso". Se lee
con `EmotionReader` (legibilidad × mi percepción): un ser en guardia/oculto no emite colegueo; uno tumbado al sol, sí. Es
el gemelo positivo de `BondEscapeReader` (que lee la huida). También modela el **cebo**: un depredador que finge calma.

## 2.1. Impulsos de BASE — qué mueve a un ser saciado (etología)

Un animal al que **no le falta comida ni descanso NO se queda quieto**: la investigación etológica da un repertorio de
motivaciones "de fondo" que ahora modelamos como **impulsos base** (llenan el vacío cuando los impulsos por necesidad
están bajos). Esto reemplaza el "deambular random" por una **exploración con propósito**:

| Impulso base | Fenómeno real | En el juego |
|---|---|---|
| **Explorar / buscar info** | *latent learning* + *mapas cognitivos* (Tolman); *information primacy* | vagar para **refrescar el mapa** de recursos (llena la memoria, §N4) aunque no haya hambre |
| **Curiosidad / novedad** | *neophilia* (más fuerte de joven, decae con la edad) | acercarse a inspeccionar lo **ambiguo/novedoso** (§curiosidad abajo) |
| **Jugar** | *play* (energía sobrante; correlaciona con contrafreeloading) | de crías/teens sobre todo; gasto de energía sin necesidad |
| **Forrajear aunque sobre comida** | *contrafreeloading* (necesidad conductual de forrajear, no solo de comer) | el olor de comida atrae **aun sin hambre** (peso bajo) → busca por gusto |
| **Patrullar / marcar territorio** | *patrolling*; territorialidad | recorrer el `HomeRadius`, **emitir su propio olor** (marca) |
| **No repetir el camino** | *alternación espontánea* | repulsor leve de la dirección recién visitada (§N4) |
| **Confort / mantenimiento** | acicalarse, tomar el sol, revolcarse | idle "activo" en zonas de confort (§N2) |
| **Afiliarse** | buscar compañía relajada | el **colegueo** como atractor social positivo |

> La suma: **un ser saciado explora, patrulla, juega, se acicala y socializa** — no es un `Idle`. El "director" solo
> siembra circunstancias; estos impulsos base hacen el resto (encaja con la "emergencia dirigida" del Microcosmos).

### Curiosidad / abstracción (tu idea del conejo y la planta)

Una **coincidencia PARCIAL** entre una pista y un recurso recordado (una planta que *se parece* a la comida) genera un
impulso de **acercarse a inspeccionar**: al llegar, **olfatea con más atención** → al acercarse, la percepción (C) sube
la claridad de la lectura → **confirma o descarta** y **actualiza la memoria**. Un `curiosity` (índice, alto de joven —
neophilia) fija cuánto se atreve con lo ambiguo. Modela también **confundir rastros** (dos pistas parecidas) y el
**"ajá"** (descubrir que sí/no era). *Base: neophilia, information-primacy, curiosity→latent-learning.*

### Nostalgia / impronta del lugar de nacimiento (tu idea de las flores)

Tu intuición ("nació sobre flores → siempre se acerca a las flores") es un fenómeno real con nombre: **Natal Habitat
Preference Induction (NHPI) / impronta de hábitat** — la exposición a las señales del entorno natal en un periodo
sensible crea una **preferencia de por vida** por señales similares ("habitat cueing": reduce el coste de buscar). En el
juego: al nacer se guarda una **firma natal** (las señales del sitio) → un **atractor persistente y bajo** hacia pistas
que la reencajan. Es una "nostalgia" emergente y barata (un dato al nacer + un impulso de fondo).

## 3. Cómo encaja con lo demás

- **Deseos (D3b):** el deseo elegido (`eat`/`defend`/buscar‑refugio) fija **qué** busca; la navegación por impulsos
  resuelve **dónde ir** (qué emisores atraen). El deseo puede **ponderar** los impulsos (con hambre, el olor de comida
  pesa más; con miedo, el hogar).
- **Sentidos (C) + legibilidad:** cada lectura se gradúa por mi percepción × la legibilidad del emisor. Sin el sentido,
  la señal no existe para mí (la garrapata ciega no lee el colegueo visual).
- **Medio/afinidad (asfixia):** el campo de confort tira hacia el microclima que mi afinidad prefiere; refuerza a
  `CorrectMedium` (que hoy solo reacciona) con **evitación proactiva** (no meterse en el mal medio de entrada).
- **Manada/bond:** `HomeImpulse`×bond y `BondEscapeReader` ya lo hacen; el colegueo lo extiende a NO‑miembros inofensivos.

## 4. Plan por rebanadas

- **N0 — quitar el sinsentido:** `LifeStage.Wander` deja de apuntar a un ave al azar; deambula a un punto local
  (alrededor de su posición/hogar). Stopgap pequeño y honesto hasta N1.
- **N1 — impulsos en el overworld:** `Animal` puede llevar `ImpulseController`; `Wander`/`Flee` pasan a leer el neto de
  impulsos en vez del ave al azar. Reutiliza `HomeImpulse`/`Scent`/`ThreatScanner` tal cual. (El overworld ya es NavMesh,
  como `ImpulseController`.)
- **N2 — campo de CONFORT (humedad/medio):** emisor de confort por microclima; impulso hacia la afinidad preferida →
  evitación proactiva del mal medio (cierra el tema de la asfixia por el lado de "no entrar").
- **N3 — COLEGUEO (`SafetyReader`):** atractor a inofensivos calmados/visibles (inverso de `BondEscapeReader`, vía
  `EmotionReader`). Incluye el cebo (calma fingida).
- **N4 — EXPERIENCIA (memoria de lugares por recurso):** cada ser lleva **un array corto por tipo de recurso**
  (comida, amigo, refugio…), **máx ~3 ubicaciones** — un canal *top-found-locations*. La entrada más **relevante** =
  más **reciente + cercana**; las nuevas **reemplazan** a las peores. Cada una **decae con el tiempo** (cuanto más pasa,
  menos probable que el recurso siga ahí) hasta caer bajo un **mínimo** que ya no merece ir a buscarlo → se descarta. El
  deseo activo (D3b) consulta el array de SU recurso → atractor a la mejor entrada viva. + **alternación espontánea**:
  repulsor leve de la dirección recién venida, salvo que esa dirección tenga una entrada de lo que ahora busca. Barato
  (≤3×N recursos, floats). Base: *cognitive maps* / *latent learning*.
- **N5 — CURIOSIDAD (`curiosity`) + inspección:** coincidencia PARCIAL pista↔memoria → impulso de acercarse; al llegar,
  la percepción (C) sube la claridad → confirma/descarta → actualiza N4. Índice `curiosity` alto de joven (neophilia).
  Modela confundir rastros parecidos.
- **N6 — IMPRONTA NATAL (nostalgia):** guardar una **firma natal** al nacer (señales del sitio) → atractor persistente
  bajo hacia pistas similares (NHPI/habitat cueing). Un dato + un impulso de fondo.
- **N7 — RASTROS + MARCAR territorio (sobre la interfaz `Trace`, §4.2):** primero **unificar** `ScentEmitter`/
  `ThreatEmitter` bajo `Trace` + `LeaveTrace`; luego seguir el **gradiente** de olor (ir a los puntos de mayor
  intensidad para "descifrar" la dirección, como el perro) hasta una línea de rastro; **emitir** el propio olor (marca);
  y los canales de estado (celo/enfermo/waste/disturbance) + `spell_residue`. Habilita confusión de rastros solapados.
- **N8 — IMPULSOS DE BASE (ser saciado):** cuando los impulsos por necesidad están bajos, activar
  explorar/patrullar/jugar/contrafreeloading/confort (§2.1) → el ser saciado deja de estar `Idle` y explora con
  propósito (refresca N4).
- **N9 — ENGAÑO:** emisores que mienten / cambian de golpe (trampas, cebos, señales que desaparecen) → el escáner puede
  ser engañado; la percepción/experiencia/curiosidad altas lo mitigan.

## 4.2. Subproductos y rastros — la interfaz que falta

**Estado actual (verificado):**
- El **efecto perceptible de un hechizo** es `Anima.magicAura`: un escalar FIRMADO **en el ser** (reputación: +
  inspira bonds, − da miedo), que **decae en el propio ser** (`MagicAura`) y se lee **en vivo** mientras está presente.
  **No es un rastro rastreable** dejado en un lugar — se va con el ser.
- Hay **dos emisores ad-hoc** (`ScentEmitter`, `ThreatEmitter`) **sin base común**, colocados en **ítems** (comida/
  cadáver) o terreno, **no emitidos por una acción/estado**. `ScentEmitter` **ya tiene** lifespan+decaimiento
  (`decayOverTime`/`decayTime`) y caída por distancia (`ScentAt`) → la **primitiva de rastro existe**.
- **No hay interfaz de subproducto.** Correcto: nunca se estableció un contrato para "esta acción/estado deja X".

**Propuesta — `Trace` (interfaz de emisión unificada):** `{ canal, fuerza, radio, falloff, lifespan→decae, IntensityAt(pos) }`.
`ScentEmitter`/`ThreatEmitter` la implementan (unificación); los scanners leen por **canal**. Un ser deja rastro con un
hook **`LeaveTrace(canal, fuerza, lifespan)`**. El **lifespan drena la fuerza** → **rastreable** (persiste tras irse el
ser) y **se desvanece** (viejo = débil; liga con la memoria decayente N4 y los rastros N7).

**Canales = subproductos (tus ejemplos):**

| Canal | Lo deja… | Efecto |
|---|---|---|
| `scent_food` | comida/cadáver (ya) | atrae carroñeros/depredadores |
| `scent_self` | **marcar territorio** (self-emit) | señal a congéneres; identidad del rastro |
| `threat` | depredador/peligro (ya) | repulsor |
| `estrus` (**celo**) | estado reproductivo | atrae parejas; delata presencia |
| `sickness` (**enfermo**) | estado de salud | otros **evitan**; el depredador prioriza al **débil** (presa fácil) |
| `waste` (**heces/digestión**) | tras digerir | rastro **+** material económico (fertilizante → economía circular) |
| `disturbance` (**dar muchas vueltas en radio reducido**) | pacing | pista de presencia/nerviosismo |
| `spell_residue` | **lanzar un hechizo** | generaliza `magicAura`: además de la reputación EN el ser, deja **marca rastreable** en el lugar |

**Doble naturaleza:** un subproducto puede ser **rastro** (olor) **y** **material** (heces = recurso) → un mismo evento
suelta un `Trace` **y** opcionalmente un `FoodItem`/recurso (enlaza con "residuo→subproducto→área" de la economía
circular, tarea abierta). **Estados que además cambian conducta:** `estrus`/`sickness` no solo emiten — modulan deseos
(celo → buscar pareja; enfermo → los demás evitan, el depredador ataca al débil) → tocan lifecycle/reproducción y depredación.

> **Rebanada** (parte de N7): unificar los dos emisores bajo `Trace` (con cuidado: son Nivel 1 VIVO) → añadir canales de
> estado (celo/enfermedad/waste/disturbance) → `LeaveTrace` disparado por acciones/estados → `spell_residue` (el hechizo
> deja marca). Es **la interfaz de subproducto** que hoy no existe.

## 5. Riesgos / abierto

- **No compilable aquí** → cada rebanada tras validación; N1 (grafting de `ImpulseController` al `Animal`) es la de más
  integración (convive con `WalkSpell`/`LifeStage`; decidir quién manda el `NavMeshAgent`).
- **Coexistencia con LifeStage:** `Wander` es evento rítmico (§ volition-selection-engine §6.1). La navegación por
  impulsos es el "cómo" del wander/flee, no un segundo scheduler.
- **Rendimiento:** escáneres por radio; a gran escala, partición espacial (ya anotado en `SenseThreats`).
- **Calibración:** pesos de cada impulso, umbral de "calma" para el colegueo, tamaño de la memoria de lugares.
