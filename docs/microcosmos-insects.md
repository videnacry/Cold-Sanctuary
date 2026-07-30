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

## 4. La 1ª misión — la familia del pulgón (amanecer; refugio = CUEVA natural)
Es el **amanecer** del micro: **aún NO hay reina, ni hormiguero construido, ni feromonas** (eso es el salto
posterior, §7). Solo una **banda pequeña** que se cobija en una **cueva natural**. Encaja con
**meso-micro-macro conectados**: a Kushal lo **envían** (dispatch) al Micro a **ayudar a transportar unos
insectos que agonizan en el suelo hasta su refugio**.

**La familia (la banda):** un **pulgón** (la "**mamá**") y **7 hormigas**, por edad:
- **1 súper mayor** + **3 mayores** (2 de ellas **la primera familia del pulgón** — las que se perdieron de
  pequeñas y **el pulgón crió**) → **4 frágiles** que hay que **cargar**.
- **1 adulta** + **2 casi adultas** → **3 capaces** de mover, pero **pocas y jóvenes**.
- Origen mixto: la tribu al principio **no las aceptaba del todo** (tener un **pulgón como madre** era
  rarísimo), pero **algunas se atrevieron a acercarse y se encariñaron con los años**.
- **La dificultad:** 3 capaces (1 adulta + 2 casi adultas) para trasladar a **4 frágiles** hasta la cueva.

**La misión:** el **pulgón guía** a Kushal (y a las 2 jóvenes) hasta las **mayores caídas**; entre todos las
**rescatan y las llevan a la cueva** (reusa `CarryToRefuge`/`WeakOne`). *Temas:* "**no abandonar al débil**",
la **familia elegida** (bond a través de la diferencia; un pulgón como madre), y el **peso del cuidado sobre
los jóvenes**. Antesala de la cría.
- **Cronología:** amanecer de la domesticación (**post-fuego**, pero **pre-sociedad hormiga**: sin reina/
  feromonas/hormiguero). No es el estrato pre-fuego de La Recolectora.

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
→ **DECISIÓN (2026-07-30): opción B** — los históricos **se encarnan como insectos** en el micro. Razón del
autor: a escala insecto la **violencia se lee como "volverse salvaje / perder el equilibrio"**, no como
trauma humano → permite contar las historias **más sangrientas sin peso traumático**, con distancia mítica
(encaja con el tema de **restaurar el equilibrio**). El "problema del fuego" en B se resuelve con el **salto
de desarrollo propio de las hormigas** (§7): su "fuego" no es fuego, son las **feromonas**.

## 6. Impacto / pendientes (opción B confirmada)
- El mundo-mob (`MobResident`/`MobWorldDirector`) se **puebla de insectos** (hormiga/pulgón primero); los
  avatares (`RobotAvatar`) son la forma del jugador; la cría del micro = mirmecofilia (pulgón).
- **Reinterpretar los históricos como insectos** (no moverlos al Meso): p. ej. el "Señor del Fuego" →
  **la primera hormiga que dominó las feromonas** (su "fuego", §7); La Sembradora/Recolectora → **la que
  domesticó el hongo/el pulgón**; el Jefe/Sargón → **la reina-tirana de las feromonas**. Sus **vivencias ya
  escritas** se conservan como esencia y se re-tematizan a escala insecto.
- Empezar por: **hormiguero + pulgón (cría) + la misión de la mascota-guía**.

## 7. El "fuego" de las hormigas — el salto de desarrollo (ciencia real)
El equivalente hormiga del fuego (el empujón que dispara la civilización) mejor candidato = **el dominio de
las FEROMONAS**. Es su "fuego" **y** su "tecnología de la información", y es **real**:
- **Feromonas = su lenguaje y su poder.** Las hormigas ya viven de señales químicas: rastros de comida,
  alarma, y las **feromonas de la reina** que **organizan la colonia** y suprimen la reproducción de las
  obreras. → **El salto:** aprender a **crear y usar feromonas a voluntad**, en niveles cada vez más altos:
  primero la reina guía; luego se refinan → coordinan agricultura, guerra y dominio → **hormigas ápex del
  microcosmos**, con una **"tecnología basada en feromonas"** (química como herramienta y como control). Es
  justo tu idea, y es lo que las hormigas **realmente** hacen. *(El tirano que abusa de las feromonas para
  someter = el análogo de Sargón/el Jefe → "perder el equilibrio".)*

**Lo que las hormigas YA "descubrieron" (real, = sus eras):**
- **Agricultura:** las **cortadoras cultivan hongo** (~50–60 M años), con **antibióticos** (bacterias) para
  protegerlo. Su "Neolítico". → puente con los **hongos** (§8).
- **Ganadería/domesticación:** **ordeñan pulgones** (mirmecofilia, §2). Su "cría".
- **Guerra e imperio:** **guerras** entre colonias, **incursiones esclavistas** (dulosis), **supercolonias**
  de miles de km ("crecer sin límite").
- **Construcción viva:** **puentes y vivacs** con sus propios cuerpos; nidos ventilados. (+hongos → megaobras.)
- **Electricidad (real, curioso):** hay hormigas (locas/rasperry) **atraídas a los aparatos eléctricos**, que
  se meten en ordenadores pese a las descargas y provocan cortocircuitos → posible beat "descubren la
  electricidad" a su modo.

## 8. Hongos — el otro pilar (agricultura, tecnología, y la sombra)
Las hormigas y los hongos ya son simbiontes → el micro puede tener **tecnología/arquitectura fúngica**
(megaobras, "materiales" de hongo, medicina). Y dos hechos reales potentes:
- **El moho inteligente (`Physarum`):** resuelve **laberintos** y halla rutas óptimas (replicó la red de
  metro de Tokio). → un **"oráculo/ordenador vivo"** del micro. *(Es un protista, coloquialmente "hongo".)*
- **El hongo zombi (`Ophiocordyceps`):** **secuestra** el comportamiento de las hormigas (las hace trepar y
  morder una hoja, y le brota de la cabeza). → la **amenaza oscura** del micro y la imagen de **"perder el
  equilibrio / volverse salvaje"** sin trauma humano (la locura viene de fuera). Encaja con la opción B.

## 9. Castas — mito de origen (por alimentación, real)
La **casta** (reina/obrera/soldado) la decide sobre todo la **ALIMENTACIÓN de la larva** (y feromonas), no la
genética. → Mito fundacional: **la primera diferenciación** — una larva **alimentada distinto** se desarrolla
como la **primera reina** (o el primer soldado). *(De tus dos ideas, esta —casta por alimentación— es la que
coincide con la biología real.)* Es el "amanecer" de la sociedad hormiga.

## 10. Domesticación antes del fuego — matiz (ciencia)
Hay que separar dos cosas:
- **Domesticación como PROCESO** (una especie cambia a lo largo de generaciones por convivir con humanos) →
  el **perro** es **post-fuego** y **datable** (~15–40 k años). Eso es lo "claro" de la ciencia.
- **Domar/hacerse amigo de un INDIVIDUO** → es **muy anterior, esporádico y NO deja rastro fósil** (una
  amistad no se "fosiliza" como domesticación). Las **amistades entre especies existen** de verdad (cuervo y
  gatito; una gata que adopta a otro felino o a un cachorro; una leona que adopta una cría de antílope). → Es
  **plausible que hubiera vínculos individuales pre-fuego** (un perdido + un animal amigo), aunque la ciencia
  no pueda fecharlos. → **Tu intuición vale para el vínculo individual** (no para la especie domesticada). En
  el micro (insecto), el "ganado" es el pulgón (mirmecofilia), antiquísimo en la evolución hormiga.

## 11. Pipeline de producción: HUMANO primero → transformar a INSECTO (2026-07-30)
Para **reconstruir la historia de la humanidad** área por área, el camino más simple (idea del autor):
1. **Crear el área con HUMANOS** primero — su simulación Meso (estaciones, recetas, historia humana). Es lo
   concreto y ya lo venimos haciendo (Cocina, Huerto, Cría, Construcción, Mecánica…).
2. **TRANSFORMARLA al mundo-insecto** — reencarnar sus personajes y su arco como insectos (opción B): mismas
   vivencias/mecánicas, re-tematizadas (fuego→feromonas, agricultura→hongo, ganado→pulgón, muro→hormiguero…).
→ El **Meso** es la plantilla (humana); el **Micro** es su reflejo insecto. Así cada área nace una vez y se
"proyecta" al otro plano sin rehacer el diseño.

## 12. Estado — 1ª misión del Microcosmos (hecho, scaffold)
`MicrocosmosSandbox_AUTO` (amanecer; **refugio = CUEVA natural**, sin reina/hormiguero/feromonas): **pulgón**
(la mamá; `HoneydewProducer` = melaza + `AphidGuide` = mascota-guía), **4 «Mayor» caídas** (`WeakOne`;
2 criadas por el pulgón + 2 de la tribu) y **2 «Joven»** que acompañan al pulgón (los únicos capaces de
cargar — la dificultad). El pulgón guía, rescata a las mayores y las lleva a la **cueva** (`CarryToRefuge`).
**Falta:** que el jugador (avatar-insecto) guíe/cargue (hoy lo hace el pulgón en auto-demo); la sociedad
hormiga posterior (reina/castas/feromonas/hormiguero); y el dispatch meso→micro que dispara la misión.
