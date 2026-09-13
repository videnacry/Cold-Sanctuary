# Microcosmos — Origen de Sakshi (el PRIMER viaje de Kushal, era pre-Ambrosio)

> **Diseño de nivel + era.** Es **anterior** a [`microcosmos-level1.md`](microcosmos-level1.md) (el alba/la cueva de
> Ambrosio): la primera vez que Kushal es enviado al Microcosmos. Mecánicas nuevas en
> [`apremios-guardian-observacion.md`](apremios-guardian-observacion.md) (observar/guardián) y §4 (el río). Fuentes de
> tono: [`novela.md`](novela.md), fichas en [`microcosmos-level1.md`](microcosmos-level1.md) §Fichas.

## 0. Ley de la saga: todos vivos, ≥6 cuerpos, una misma alma

Petición del usuario (regla dura): **cada historia tiene ≥6 personajes VIVOS a la vez, en cuerpos distintos**; la misma
**alma reencarna** (`SoulRecord`/`SoulComposition`): Ambrosio→hormiga (era siguiente); Medea→reina; Ruth→sembradora;
Héspero→ciencia. En ESTA era pre-Ambrosio, **Momo y Medea aún no nacen como gemelas** → sus almas viven en **hormigas
ancianas** que morirán **cerca del nacimiento** de las gemelas (el alma pasa de un cuerpo al otro). Así el elenco de la
era se compone de: **Sakshi (cría)** + la **tribu** (varias hormigas adultas/ancianas, portadoras de las almas del
elenco) + **Kushal (gusano, jugador)** + el **depredador**.

## 1. Semilla narrativa (por qué Sakshi es "así")

Sakshi tiene la costumbre de **alejarse de golpe a observar** algo — asombro, curiosidad, indagación — y eso hizo que
**los demás la dejaran atrás**. Dos eventos formativos, ambos jugables en este nivel:
1. **El río se la llevó.** De cría, el río la arrastró **lejos y rápido**; acabó en **orillas remotas**, mucho más cerca
   del **depredador** que de su familia. Esas orillas son el **primer punto de entrada al Microcosmos**.
2. **Kushal la defendió.** En esas orillas, débil por el arrastre, un depredador la acecha; Kushal (su primer viaje) la
   protege. Ese cuidado en el desamparo marca su forma de mirar el mundo.

## 2. El nivel (beats)

**Punto de entrada = la orilla lejana** donde el río dejó a Sakshi (cría, muy **debilitada** por el arrastre → apenas se
mueve, solo **observa**). Cerca, un **depredador con POCA hambre**.

1. **Llegada / apreciar el lugar.** Kushal aparece en la orilla. El depredador, con poca hambre, se acerca **con sigilo
   y lento** → el jugador tiene **tiempo de apreciar el sitio y de notar el peligro** (y de "meditar" la situación).
2. **La amenaza elige a Sakshi.** El depredador **prefiere hormigas a gusanos** → va a por **Sakshi** (presa predilecta),
   no por Kushal. El jugador debe **proteger a Sakshi**.
3. **Empujar (no cazar).** El depredador es **más débil que Kushal** → Kushal solo tiene que **EMPUJARLO**. Como además
   tiene poca hambre, al asustarse un poco **se aleja**. (Un gusano es lento y no huye cargando a nadie: su verbo es
   *empujar/interponerse*, no cazar ni escapar.)
4. **Reintento + escalada de hambre.** No se asusta del todo: **se aleja y trata de volver por otro lado**. El **hambre
   sigue creciendo** hasta que **le da igual** que Kushal lo empuje → **carga a toda velocidad** hacia Sakshi.
5. **Llega la tribu.** En ese momento **el resto de la tribu entra en escena** a enfrentar al depredador. El depredador
   **mide la fuerza del pack** que lo ataca y **decide huir**. Fin del nivel.

El bond Kushal↔Sakshi que se forja aquí (defenderla mientras ella solo puede observar) es la semilla de su carácter.

## 3. ¿Cómo llega la tribu? (motivación emergente, no guion tramposo)

Pregunta: ¿la tribu viene *a por* Sakshi o *por casualidad*? No hace falta que "sepan" dónde cayó. La gente de esa era
**leía la vida del lugar**: iban hacia donde había **menos señales de depredador**. Diseño:
- Se **rodea el mapa de depredadores** (un **anillo/triángulo**), y el **único vértice seguro** —el punto más lejano de
  todos ellos— es **la orilla de Sakshi**, donde solo está el **depredador débil** (que no da tanto miedo como los del
  anillo). La tribu, huyendo del peligro que percibe (`ThreatScanner`/`ThreatEmitter` **ya existen**), **camina sola
  hacia el punto seguro** = hacia Sakshi. Emergente. Su llegada coincide con el beat 5.

## 4. El RÍO (hechizo de corriente) — arrastra y DAÑA

El río **empuja a las ánimas que entran en él**. Se implementa como **hechizo tipo `PullSpell`** (que ya arrastra por
**combate de stats** + gasto de ATP + no-progresa-si-resistes): un **`RiverCurrent`** =
- **dirección fija** (aguas abajo) en vez de "hacia el lanzador",
- **área** (una zona-trigger del cauce; afecta a quien esté dentro),
- **combate de stats**: una hormiga **grande** aguanta (no la arrastra); una **pequeña como Sakshi (cría)**, sí,
- **daño por arrastre** (nuevo respecto a `PullSpell`): el zarandeo **debilita** (baja energía/masa) → así Sakshi, al
  llegar a la orilla, **no puede huir** (si pudiera, huiría y no habría bond) y **solo observa**. Enlaza con
  `WeaknessEffect`/inanición.

**Es el PRIMER build** (decisión del usuario): reutiliza casi todo `PullSpell`; añade dirección/área/daño.

## 5. Mecánicas que faltan (para construir, en orden)

1. **`RiverCurrent`** (hechizo de corriente): dirección + área + daño por arrastre. *(primer build)* → **✅ HECHO (PR #182)**:
   componente-zona (`Assets/Scripts/Microcosmos/RiverCurrent.cs`) que arrastra aguas abajo por **empuje neto = fuerza −
   masa·resist** (grande aguanta, cría no), vía `ImpulseController` si lo hay o desplazando directo si no, y **debilita**
   (drena ATP + estrés). Test `RiverCurrentTest` (grupo 9). Falta: **colocarlo en la escena del nivel** (la zona del cauce).
2. **Preferencia de dieta** (depredador **prefiere hormigas a gusanos**) → **✅ HECHO (PR #184)**: `DietPreference`
   (apetencia por especie) integrado en `Forager.SelectPrey` (`score = facilidad × apetencia − distancia`). El depredador
   de hormigas prefiere `Ant` (3×) y desdeña `Gusano`/`Worm` (0.3×) → va a Sakshi, no a Kushal. Test `DietPreferenceTest`
   (grupo 10). **Falta en el nivel:** que el depredador débil sea un `Animal` con `Forager` (hoy es blockout `SimpleAnima`).
3. **Depredador débil + escalada de hambre** que vence al miedo: el hambre creciente **sube el peso del apremio de comer
   por encima del miedo** → deja de importarle el empujón (ver `apremios-guardian-observacion.md` §2). Kushal-gusano solo
   **empuja** (interpone su cuerpo / un empujón por impulso, no un ataque).
4. **Anillo de depredadores** → vector seguro hacia Sakshi (posicionamiento en la escena; `ThreatEmitter` por depredador).
5. **Observación de Sakshi debilitada** (ventana de bond): ver el subsistema `apremios-guardian-observacion.md` (mirar
   sostenido baja el sufrimiento y evita la huida). MVP: mientras es observada/observa, no dispara el impulso de huida.
6. **La tribu** = elenco de la era (almas de Momo/Medea en ancianas) con `SocialImpulse`/`ThreatScanner` → llegada emergente.

## 6. Kushal en estas eras = GUSANO (canon)

El avatar **gusano** es el primero de la progresión (gusano→araña→mosco; locomoción `Ground`, no trepa, pequeño y
**lento**). Adulto pero **débil/lento**: no caza ni huye cargando — **empuja/interpone**. Encaja con "defender a Sakshi
empujando al depredador débil". Sus hechizos (ver `BuildNivel1Sandbox`: `FormicAcidSpray`/`PullSpell`/`HoneydewSpell`)
se irán introduciendo; aquí basta **empujar** + (a futuro) el pasivo de **observar**.

## 6.5. Nivel intermedio ABIERTO con temporizador (Kushal explora) — a construir

Entre Sakshi y Ambrosio, un nivel **abierto de merodeo** (idea del usuario): Kushal entra al Microcosmos con un **reloj
de arena** (temporizador) que avisa cuánto le queda antes de salir; en ese lapso **deambula libremente** — observa/huye de
depredadores, se acerca/interactúa con la tribu. Sin objetivo duro; es exploración + descubrimiento.

- **Tamaño del mapa = como el Santuario 1** (~500×500) → cabe el río, la orilla, y **cadenas alimenticias COMPLETAS**
  (no solo depredadores+hormigas): plantas → **herbívoros insectos** (pulgón=savia; y otros) → depredadores (mariquita/
  araña) → apex — igual que la cadena marina del Santuario 1. *(Herbívoros en el mundo insecto: SÍ — pulgón/oruga/grillo
  comen plantas; muchos son oportunistas, pero hay herbívoros claros.)*
- **Reutilizar el spawn por ÁREA**: ya existe **`Generator`** (subjects×área, hoy huérfano) + `FamilyGenerator`/`AddFamily`
  (cada familia con su posición+radio = su área). Plan: poblar el mapa con **una familia/Generator por especie y por ÁREA**
  (opción de "en qué zona aparece cada especie" — ya es intrínseca a AddFamily; se puede formalizar como parámetro). Así se
  crea el mapa con el mismo plan de respawn que el Santuario 1, conservando río/orilla/anillo.
- El **temporizador** = reusar el `Clock`/`TimeController` (un `MobWorldMission`-like con cuenta atrás → salir por el `YogaPortal`).

## 6.6. Patrón de niveles por era: EXPLORACIÓN LIBRE ⨯ CHECKPOINTS

Idea del usuario: cada era tendrá **dos tipos** de nivel, y se puede establecer como patrón:
- **Exploración LIBRE** (temporizador): merodear sin objetivo duro; su función es **explorar entornos nuevos donde el
  peligro SUPERA los stats/hechizos del jugador** → cuesta volver al punto de salida (tensión). El punto de salida es un
  portal (ver §6.7).
- **Exploración por CHECKPOINTS**: el jugador debe **alcanzar checkpoints** del mapa para completar el nivel. Base ya
  existente: `CarryToRefuge` (contar llegadas a un punto) y `ReachGoalMission` (ir a una meta WASD).

## 6.7. Hilo de estos niveles: Kushal DESPIERTA la observación de Sakshi

El PROPÓSITO narrativo de los niveles intermedios (aclaración del usuario): Kushal va **despertando la OBSERVACIÓN de
Sakshi** (con el pasivo `ObserveSpell`: mirada sostenida → sube la observación/ecuanimidad). Eso la convierte, poco a poco,
en la Sakshi que **se ensimisma mirando** lugares/ánimas y **no nota la agenda de la tribu** → **se rezaga**. Entonces los
miembros de la tribu deben **ir a buscarla para "despertarla"** (interrumpir su observación) y que avance con ellos al
siguiente lugar. Bucle jugable (mapea a lo que ya hay):
- **Kushal + Sakshi observan** juntos → `ObservationSkill` de Sakshi sube (y su `alertness`); gana ecuanimidad.
- **Sakshi se absorbe** → deja de seguir a la tribu (su deseo `observe`/quietud gana a `follow`).
- **La tribu se aleja** (cohesión baja, `TribeCohesion`) → un miembro vuelve a por ella (`tend`/`follow` hacia Sakshi) y la
  "despierta" (rompe el gaze) → Sakshi retoma el `follow`. Es el germen de su rareza y del abandono futuro.

## 7. Escenarios intermedios hasta Ambrosio (a definir)

Entre este nivel (origen de Sakshi) y el alba de Ambrosio habrá **varios escenarios** que:
- presentan al resto del elenco de la era (las ancianas portadoras de Momo/Medea; Héspero/Ruth jóvenes),
- muestran la **muerte** de las ancianas **cerca del nacimiento** de las gemelas (el alma pasa de cuerpo),
- encadenan hacia la llegada de Ambrosio (el pulgón) a la cueva.
Se irán detallando; esta es la **cabecera** de la era pre-Ambrosio.
