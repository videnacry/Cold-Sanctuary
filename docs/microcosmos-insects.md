# El Microcosmos como mundo de INSECTOS — propuesta (2026-07-30)

Diseño en discusión. Decide el marco del Microcosmos: ¿sus habitantes son personas a pequeña escala o
**insectos**? Recomendación y piezas. Base: [`mob-world-architecture.md`](mob-world-architecture.md),
[`magic-plane-and-meditation.md`](magic-plane-and-meditation.md) (avatares), [`kitchen-simulation.md`](kitchen-simulation.md) §10.

## 1. Recomendación: SÍ, insectos — y **hormigas** como civilización primaria
El jugador ya entra como **avatar-insecto** (`RobotAvatar`: gusano→reptar, araña→trepar paredes,
mosco→volar; `AvatarLocomotion`). Comprometámonos con ello. Y las **hormigas** son la sociedad ideal porque
son **lo más parecido a las personas**: **ciudades** (hormigueros), **reparto de trabajo** (castas), **una
al mando** (la **reina**), y el impulso de **crecer sin límite**. Además, la biología real ya trae nuestras
mecánicas:
- **Hormigas cortadoras CULTIVAN hongo** → **agricultura** a escala insecto (el Huerto del micro).
- **Guerras de hormigas** → **conflicto / fuertes-vs-débiles** (el núcleo).
- **Castas + reina** → jerarquía, dominio (enlaza con "detener conflictos" y Sargón/el Jefe).
- **Nidos** → **construcción**.

## 2. La domesticación a escala insecto = MIRMECOFILIA (¡real!)
Tu intuición del "gusano/oruga que fabrica un líquido codiciado y por eso lo cuidan y alimentan" **existe de
verdad**: las hormigas **protegen y "ordeñan" a los pulgones** (áfidos) por su **melaza** (honeydew) — son su
**ganado**. También cuidan **orugas de licénidas** que segregan néctar. → **El pulgón/oruga es la "mascota/
ganado" del micro** = el análogo perfecto de la **cría/domesticación** a escala insecto.
- **El pet-guía de Kushal** puede ser **ese pulgón/oruga tendido** (o un proto-compañero) que **no quiere
  dejar a su familia** → guía a Kushal hacia ellos.

## 3. Otras ciudades-insecto (para más adelante)
- **Abejas** — colmena, miel, **danza** y **decisión colectiva** (democracia del enjambre). Sociedad "sabia".
- **Avispas** — nido de **papel**, depredadoras. Sociedad "marcial".
- **Termitas** — **montículos** enormes con **aire acondicionado** y ¡también **cultivan hongo**! Sociedad
  "ingeniera". (Sí, las termitas construyen ciudades.)
→ Cada insecto eusocial = un **sabor de civilización** distinto para explorar (hormiga/abeja/avispa/termita).

## 4. La entrada de Kushal (dispatch meso→micro) + la mascota-guía
Encaja con **meso-micro-macro conectados**: a los personajes se les **envía** a un plano a **resolver
misiones** y de paso **descubren la historia** de esa dimensión. Escena propuesta:
1. En el Mesocosmos, envían a Kushal al **Microcosmos** a **transportar unos insectos que agonizan en el
   suelo hasta sus hogares** (caídos del hormiguero/colmena). *(Dispatch, como los tickets de reparación.)*
2. Kushal entra (avatar-insecto). Un **insecto-mascota** (el pulgón/oruga tendido) se le acerca: **no quiere
   dejar a su familia**, a la que **su colonia abandonó** por débil.
3. La mascota **guía** a Kushal hasta esa familia caída → Kushal los **lleva al refugio/cueva** con los demás
   (reusa `CarryToRefuge`/`WeakOne`). *(Antesala de la cría; "no abandonar al débil".)*
- **Cronología:** este beat es el **amanecer de la domesticación** (Paleolítico Superior, **post-fuego**), no
  el estrato pre-fuego de La Recolectora (allí aún no hay mascotas).

## 5. La tensión a resolver (tu decisión)
El Microcosmos hoy mezcla dos cosas: (a) las **historias históricas humanas** (Señor del Fuego, La
Recolectora…) y (b) el **mundo-insecto**. Si los habitantes son insectos, ¿qué pasa con los históricos?
- **Opción A — Historia paralela insecto:** el micro es de insectos y **re-vive el arco de la civilización a
  su modo** (agricultura del hongo, guerras, ciudades, reina) — sin humanos. Los "históricos humanos" viven
  en el **Mesocosmos** (o son un marco narrativo), y el jugador los evoca, pero el micro es insecto puro.
- **Opción B — Históricos como insectos:** los personajes históricos (Señor del Fuego…) **son** insectos en
  el micro (encarnados), y su historia se cuenta a esa escala. *(El "fuego" a escala insecto es difícil de
  justificar → esta opción cojea con la era del fuego.)*
- **Opción C — Dos capas:** el micro tiene su **ecología insecto real** (hormigas/pulgones: cría, conflicto,
  construcción) **y**, aparte, los **mob-históricos** como visitantes/ecos. Más rico, más complejo.
→ **Recomendación:** **A** (insectos con su propia historia-civilización paralela; los históricos humanos son
del Meso/marco). Es lo más limpio y aprovecha la biología real como mecánicas. *A confirmar por ti.*

## 6. Impacto / pendientes
- Si se confirma: el mundo-mob (`MobResident`/`MobWorldDirector`) se **puebla de insectos** (hormiga/pulgón
  primero); los avatares (`RobotAvatar`) son la forma del jugador; la cría del micro = mirmecofilia (pulgón).
- Revisar qué historias humanas ya escritas (grupo fundador, La Recolectora) pasan al **Meso** o se
  reinterpretan.
- Empezar por: **hormiguero + pulgón (cría) + la misión de la mascota-guía**.
