# Progresión de áreas — cómo se va formando la historia

Diseño (2026-07-29). Orden en que se abren las **áreas/simulaciones** para principiantes, alineado con la
**línea temporal del microworld** (una época por escalón) para conservar **coherencia + biografías +
historia real**. Base: [`mob-epochs-matrix.md`](mob-epochs-matrix.md) (hilos/épocas/regiones),
[`mob-characters.md`](mob-characters.md), [`mob-quests-early.md`](mob-quests-early.md). Docs por área:
[`kitchen-simulation.md`](kitchen-simulation.md), [`garden-simulation.md`](garden-simulation.md).

## Principio
**Área = región/civilización (espacio)** · **Época = tiempo** · **Hilo = dominio**. Cada área nueva avanza
la época y "pasa la antorcha" a otra región, encarnando **figuras reales** (con su ancla de piedra). El
arco de cada personaje es **agridulce → aprendizaje**; cada era entrega su **elemento**.

## Apertura del juego — prólogo en la ENFERMERÍA (2026-07-30)
El juego **empieza en la Enfermería**, por una razón **narrativa y realista** (no cronológica): Kushal
**viene de fuera**, así que le hacen **exámenes médicos y lo vacunan** para que **no contagie a los animales**
del santuario — más aún porque irá al **área de cría**. Secuencia:
1. **Exámenes médicos** (tutorial suave de la Enfermería; presentación del cuidado/salud).
2. Kushal **pregunta y curiosea** → ve la **máquina de control de avatares** (= la `VirtualizationMachine`
   ya existente + `RobotAvatar`/`AvatarController`). Le **dejan probarla**.
3. **Primera experiencia en el MICROCOSMOS → la era ANTERIOR AL FUEGO: las plantas medicinales.** (El fuego
   es la Cocina; aquí **aún no nace el Señor del Fuego**.) Es el estrato **más antiguo**: el mundo de
   **La Recolectora** (hilo **C**, ancla), donde Kushal ve cómo **algunos llevan plantas consigo** y **se
   aferran a una como algo muy valioso** (la que alimenta y cura). La **comunicación es corporal/gestual**
   (aún sin lenguaje desarrollado): se **mima/actúa** lo que se vio o se siente. Kushal entra a **apoyar y
   animar a los débiles** → **antesala de las misiones de cuidado del área de CRÍA**. La **cueva** cabe como
   **refugio**, pero **todavía SIN pinturas** (ver nota de arte abajo). Tutorial de virtualización + gancho.
4. Pasados los exámenes + vacuna → lo llevan a su **primer trabajo** (1ª simulación de trabajo = la Cocina).

> **Volver / puerta BIDIRECCIONAL (ya existe).** Tras ayudar a **llevar a los débiles a la cueva**, se le
> dice a Kushal que vaya a la **sala de meditación** para **volver al Mesocosmos**. Se **entra** al Microcosmos
> por esa misma área (la `VirtualizationMachine` = máquina de avatares/meditación) y se **sale** por el
> `YogaPortal` (la sala de yoga dentro del Micro) → el jugador **sabe cómo volver** y puede **entrar/salir
> incluso en mitad de misión** (`MobWorldLoader.ExitMobWorld`, ya soportado; docs mob-world §4). Eso hace
> **conveniente separar personajes/lugares por área**. Además, el **Mesocosmos puede enviar avisos** al
> jugador dentro del Micro (`PlaneMessenger`). *(Piezas ya existentes/scaffold: `PrologueSequence`,
> `PlaneMessenger`, `CarryToRefuge`/`WeakOne`; la escena se monta en Unity.)*
>
> **El arte rupestre NO es de aquí (ciencia).** El fuego (~1 M–400 k años) precede al **arte figurativo**
> (~45 k años, Paleolítico Superior) por cientos de miles de años. **No se dibujaba con sangre**: el rojo era
> **ocre** (óxido de hierro), el negro **carbón** (¡requiere fuego!) o manganeso. → El **arte de cueva + la
> misión de conseguir pigmento (OCRE, no sangre)** pertenecen a un **beat POSTERIOR, post-fuego**: el hilo
> **D · Trazo y Símbolo**, ancla **La Mano de Lascaux**. En el prólogo pre-fuego, sí: **gesto/actuación**, no pintura.

→ Resuelve el orden: la **Enfermería es el marco/prólogo** (razón narrativa), y la **historia del microcosmos
arranca en el estrato PRE-FUEGO** (recolección + plantas medicinales, La Recolectora) → luego el **fuego**
(Cocina) → agricultura (Huerto)… La semilla de la medicina queda desde el minuto 1.

## Estrato PRE-FUEGO (el más antiguo del microcosmos) — La Recolectora
Antes del fuego (Australopitecos / primeros *Homo*, ~2–3 M años), lo que la ciencia deduce y que usamos:
- **Bipedismo → manos libres para CARGAR** (comida, crías… y **plantas**): explica que "lleven plantas consigo".
- **Herramientas de piedra (Oldowan, ~2,6–3,3 M años) ANTES del fuego** — cortar, machacar, abrir huesos por
  el tuétano. (El ancla **El Tallador** también es pre-fuego.)
- **Dieta cruda** (fruta, hojas, tubérculos, semillas, algo de carroña); mandíbulas/tripa grandes; **aún no
  se cocina**.
- **Conocimiento de plantas que alimentan y curan** + **zoofarmacognosia** (aprender remedios observando a los
  animales). → el mundo de **La Recolectora**.
- **CUIDADO del enfermo/anciano en el grupo** (hay fósiles de individuos que sobrevivieron heridas/vejez sin
  dientes → alguien los alimentó): **la compasión y el cuidar son anteriores al fuego** → raíz de la Enfermería.
- Bandas pequeñas cooperativas, **reparto de comida**, crías de infancia larga; carroñeo antes que caza organizada.
- **Comunicación pre-lenguaje: corporal/gestual** — mímica, actuar lo visto/sentido, sonidos y gestos (aún
  sin lenguaje articulado ni escritura ni **arte**). → encaja con "las interacciones son más corporales".
(Lo simbólico —**ocre, arte rupestre, entierros**— y la trepanación son **posteriores** (post-fuego), no de
este estrato: el arte va en el hilo **D · La Mano de Lascaux**, Paleolítico Superior.)

## Ya planteadas
0. **La Enfermería (prólogo)** — arranque: exámenes + vacuna + 1er uso de la máquina de avatares → Microcosmos
   **pre-fuego** (plantas medicinales, **La Recolectora**). La Enfermería-área desarrollada (medicina-profesión)
   se retoma más tarde (§6).
1. **La Cocina** — Paleolítico (era del fuego) · FuelLab/Cocina · hilo A (**Señor del Fuego**). Elemento
   **C**. Ver [`kitchen-simulation.md`](kitchen-simulation.md).
2. **El Huerto** — Neolítico (agricultura) · Creciente Fértil / primera aldea · hilo C (**La Sembradora**).
   Elemento **N**. Ver [`garden-simulation.md`](garden-simulation.md).

> **Era temprana — grupo fundador:** Señor del Fuego → **Nasatya** (puente; guardián ficticio, encarna a un
> Ashvin — con Kushal, "los recolectores estrella") → La Sembradora, con **Kushal** de hilo conductor. Sus
> historias entrelazadas y las misiones definidas están en
> [`founding-trio-stories.md`](founding-trio-stories.md). *(El Ötzi histórico va en la Forja, no aquí.)*

## El área de CRÍA — **área 3, tras el Huerto** (confirmado 2026-07-30)
La **cría** (cuidar/criar animales) es el **corazón del santuario** — el jugador viene **para eso** (el
prólogo lleva ahí). Cronología de la **domesticación** (para fijar su lugar):
- **Perro** = **Paleolítico** (~15–40 k años), **antes de la agricultura** → el **primer compañero animal**.
- **Ganado** (cabra/oveja/cerdo/vaca) = **Neolítico** (~10–8 k a.C.), **a la par del Huerto** (se domesticaron
  plantas y animales juntos).
- Ambos **anteriores** a los **metales** (~4 k a.C.) y a la **medicina profesional** (~2,6 k a.C.).
→ **La cría va antes que Construcción/Mecánica.** Recomendado: el **bond/cuidado** desde pronto (perro,
Paleolítico; enlaza con el prólogo y `CarryToRefuge`/`WeakOne`), y la **cría/ganadería** como área ~**Neolítico**
(con el Huerto). **Reutiliza sistemas ya existentes** (`Animal`/`LifeStage`/`PostNatal`/`Family`).
**CONFIRMADO: la cría es el área 3** (tras el Huerto) → Construcción=4, Mecánica=5, Enfermería=6, Yoga=7.
Diseño y virtualización en [`cria-simulation.md`](cria-simulation.md).

## Las siguientes áreas

> **Orden (2026-07-30):** prólogo **Enfermería** → 1) **Cocina** → 2) **Huerto** → **3) Cría** → **4)
> Construcción** → **5) Mecánica** → **6) Enfermería** (profesión) → **7) Yoga**. Racional: cría y refugio
> (Neolítico) **antes** que los metales; la medicina profesional después de los metales; el Yoga cierra.

### 4. La Construcción (Meso) / Levantar refugio (Micro) — Neolítico (chozas → casas)
- **Región/época:** Neolítico — la aldea del Huerto necesita **techo**: chozas de adobe, madera, paja;
  cerámica (El Alfarero) para materiales. Antes que el metal.
- **Hilo/foco:** **B · Barro y Metal** (ancla *El Tallador* → *El Alfarero* → *Maestro de catedrales* →
  Brunelleschi). Comparte hilo con la Mecánica, pero **la construcción es su etapa temprana**.
- **Meso (santuario actual):** **hub de MANTENIMIENTO de estructuras** — reparar **tuberías, electricidad,
  puertas, paredes, ventanas** de todas las áreas (por **tickets/dispatch**, ver `forge-simulation.md` §5:
  llega el aviso → tomas herramientas → vas al área → reparas → vuelves y dejas las herramientas).
- **Micro (histórico):** levantar refugio a través del tiempo (choza → casa → templo → catedral).
- **Onboarding** (como todas): limpiar el solar → **abastecer** materiales (cajas→almacén, `StockingTask`) →
  **construir/reparar simple** (receta: cimentar → levantar muro → techar → probar).
- **Giro:** el refugio une a la tribu, pero **el muro también separa** (propiedad, fronteras) — enlaza con
  el excedente del Huerto y con "detener conflictos".

### 5. La Mecánica (Meso) / La Forja (Micro) — Edad de los Metales (Cobre → Bronce → Hierro)
> **Reencuadre 2026-07-30** (ver [`forge-simulation.md`](forge-simulation.md)): la 3ª área del **Mesocosmos**
> es la **MECÁNICA** (reparar/mejorar máquinas de todas las áreas + vehículos/drones/avatares/teleportadores);
> la **forja de bronce** literal se mueve al **Microcosmos** (capa histórica de los metales). Lo de abajo
> describe la capa histórica (Micro).
- **Región:** **Mesopotamia** (+ Egipto para Imhotep). Ya existe la escena `MobWorld_Mesopotamia` → reuso
  directo; el jugador ve **la aldea convertirse en ciudad**.
- **Hilo/foco:** **B · Barro y Metal** (ancla *El Tallador* → *El Alfarero* → **El Fundidor / Primer
  Herrero**). Arquetipo **Canalizar/forjar** + **Proteger**.
- **Qué pasa:** fundir metal (**cobre+estaño = bronce**, luego **hierro**) → **forjar herramientas**
  (arado, hoz) → y **armas**.
- **Giro (real):** la **misma forja** hace el arado y la espada → **el primer útil de metal fue también la
  primera arma** (enlaza con la muerte de **Ötzi**, flecha de cobre por la espalda). *El que forja, elige.*
- **Históricos:** **El Primer Herrero / El Fundidor** (B), **Ötzi** (el **histórico** del hacha de cobre,
  asesinado — su misión "buscar la raíz"), **Sargón de Acad** (E, primer imperio → conflicto/dominio),
  **Enheduanna** (D, primera autora con nombre; Acadia), **Imhotep** (C, Egipto; ingeniero-médico),
  **El Astrónomo de Babel** (A). Elementos **Cu, Sn, Fe**.
  > **Nota (nombres, 2026-07-29):** el **Ötzi histórico** (hacha de cobre) vive **aquí**; el **guardián
  > ficticio** del grupo fundador ya **no** se llama Ötzi sino **Nasatya**, y es de la **era temprana**
  > (ver [`founding-trio-stories.md`](founding-trio-stories.md)). La Forja hereda además el hilo del fuego
  > (*pirita → hierro* del Señor del Fuego).
- **Encaja con:** las misiones de **detener conflictos** (garden §5) escalan aquí — el metal arma las
  disputas; y el núcleo **fuertes vs débiles**. Capstone de la era temprana: **Gilgamesh** ("llegar al
  corazón").
- **Estado (2026-07-29):** diseñada en [`forge-simulation.md`](forge-simulation.md); **El Primer Herrero** y
  **Sargón de Acad** autorados; receta de bronce cableada (`ForgeVirtualization_AUTO`, crisol con typing).

### 6. La VETERINARIA / Farmacia — salud ANIMAL (renombrada 2026-07-30)
> **Es la VETERINARIA (salud animal), no una enfermería humana.** "Veterinario" = médico de animales, y
> **trata TODO tipo de animales** (esa es su definición) → es el área correcta para el santuario. La
> **enfermería humana** se reduce al **puesto del prólogo** (exámenes/vacuna de los voluntarios; puede ser el
> propio chequeo de ingreso de la clínica). Histórico **fundacional de la veterinaria = El Perro de
> Oberkassel** (primer animal cuidado por amor; cria-simulation §5) → puente con el área de **cría**.
> Historia veterinaria real: el **papiro de Kahun** (~1800 a.C.) ya es un texto veterinario; hubo sanadores
> de animales desde la ganadería. Las figuras de medicina *humana* (Imhotep/Hipócrates) quedan como
> trasfondo del hilo médico, no como el foco del área.
>
> **La Veterinaria también conecta con el PRÓLOGO** (ver "Apertura del juego"): el chequeo del recién llegado
> y el primer uso de la máquina de avatares.
>
> **Cronología (2026-07-30): la medicina básica precede incluso a la agricultura.** El uso de plantas para
> sanar es **anterior a cultivar** (es **recolectar/forrajear**) y hasta **prehumano** — **zoofarmacognosia**:
> los **animales se automedican** comiendo ciertas plantas. → Es hilo **C**, ancla *La Recolectora* (plantas
> que **alimentan y curan**), tan antigua o más que el Huerto. **Gancho de juego:** aprender remedios
> **observando a los animales automedicarse** (encaja con cuidar crías/bonds). La **trepanación** neolítica
> (~7000 a.C.) ya precede al cobre.
> **Sin origen único:** la herbolaria surgió **independiente en todas las regiones** (Egipto, Mesopotamia,
> India, China, América) desde la misma raíz primal — **no** "Egipto antes que Mesopotamia". La **medicina
> como profesión documentada** (Egipto: **Imhotep** ~2600 a.C., primer médico con nombre; Mesopotamia:
> tablillas ~2100 a.C.) aparece **casi en paralelo** en el Bronce.
> → **Desdoblar:** **primeros auxilios/herbolaria = capa TEMPRANA** (junto al Huerto, region-agnóstica, con
> el gancho de los animales); **Enfermería-profesión** rota región (Egipto/Imhotep primero), ~era de los
> metales. Los metales **no** van antes que la medicina *básica*, solo antes que la *profesional*.
>
> **¿Antes que Cocina? (2026-07-30)** El *instinto* de sanar (comer una planta para sentirse mejor) es lo
> **más primal de todo** — precede al fuego y hasta a los humanos (animales). Pero conviene separar:
> ese **instinto = "capa 0"** tejida en el **cuidado de los animales** (está presente desde el primer minuto
> del santuario, observando a los seres que cuidas); la **Cocina** es la primera *simulación-tutorial de
> trabajo*. → Recomendación: **la semilla de la medicina está desde el inicio (vía cuidado/observación de
> animales)**, no como "área Enfermería gateada antes de la Cocina"; la Cocina sigue siendo el tutorial de
> mecánicas, y la Enfermería-profesión se desarrolla después. *(Decisión final tuya.)*
- **Región:** rota (Egipto → Grecia → India → mundo islámico) según la figura.
- **Hilo/foco:** **C · Semilla y Vida** (continuación de La Recolectora: las plantas que **alimentan Y
  curan**) tocando **F · Aliento y Mente**. Arquetipo **Curar/atender**.
- **Qué pasa:** **curar heridas y enfermedades** (¡las **nuevas enfermedades del sedentarismo** que nacen
  en el Huerto §3!), **preparar remedios** con las plantas del Huerto (farmacia), **atender** a los enfermos.
- **Giro (real):** el saber que **cura** también puede **dañar** (venenos, el conocimiento acaparado por
  sacerdotes/gremios); la peste. *Sanar es cuidar, no poseer el saber.*
- **Históricos:** **Imhotep** (Egipto, médico-arquitecto), **Hipócrates** (Grecia, el juramento),
  **Sushruta** (India, cirugía), **Avicena** (mundo islámico, el *Canon*). Ancla de fondo: *El Chamán*.
- **Encaja con:** refuerza el **tema de no-violencia/cuidado** del santuario; recibe del Huerto (plantas)
  y de la Cocina (nutrición → salud); atiende a heridos de los conflictos (área 3).

### 7. La Sala de Yoga — Aliento y Mente (el núcleo espiritual)
- **Región:** rota (India para Buda, Grecia para Sócrates…).
- **Hilo/foco:** **F · Aliento y Mente** (ancla *El Chamán* → *El Soñador* → Buda/Sócrates/Spinoza).
  Arquetipo **contemplativo** (canalizar/meditar).
- **Qué pasa:** **asanas**, **meditación** y **restaurar el canal mental** — **reusa** los sistemas ya
  hechos de yoga y del **Microcosmos/meditación** (`MeditationSession`, arquetipos de mob, avatares).
- **Giro:** la búsqueda de sentido; **desapego vs implicación** (el místico que se aísla demasiado y
  abandona a los suyos). *La paz interior no es huida.*
- **Históricos:** *El Chamán* (piedra), *El Soñador* (neolítico), **Buda** (India), **Sócrates** (Grecia),
  **Spinoza** (Ilustración). Conecta con las **margas del alma** (Yoga) y el desbloqueo del **maná**.
- **Encaja con:** el corazón temático del juego (consciencia/alma); cierra el arco temprano llevando al
  jugador del **cuerpo** (cocina/forja) y la **vida** (huerto/enfermería) a la **mente/espíritu**.

## Lectura del arco (por qué este orden)
**Fuego → Alimento → Herramienta/Metal → Salud → Mente**: del sustento físico a la vida, de la vida a la
técnica (y su doble filo: el conflicto), del conflicto al cuidado, y del cuidado a la consciencia. Cada
área **hereda el giro de la anterior** (excedente→propiedad→conflicto→heridas→sentido) → la historia se
siente **encadenada**, no episódica. Después seguiría la **Antigüedad clásica** (Grecia: Arquímedes,
Homero, Alejandro, Aristófanes) abriendo hilos D/E/G con más fuerza.

## Pendiente de autoría (PhrasePools, con `Historico()`)
Neolítico: La Contadora de Granos, El Jefe de la Aldea, El Lector del Cielo. · Metales: El Primer Herrero,
Sargón, Enheduanna, Imhotep, Gilgamesh. · Salud: Hipócrates, Sushruta, Avicena. · Mente: El Chamán, Buda,
Sócrates. (Ya hechos: Guardián del Fuego, La Sembradora, El Alfarero, Ötzi.)
