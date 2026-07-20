# Matriz de épocas × hilos — héroes a través del tiempo

> Diseño (2026-07-21). Estructura para generar historias largas y conectadas en el mundo mob.
> Ver [`mob-world-architecture.md`](mob-world-architecture.md) §14 (filosofía tonal, eras),
> [`mob-characters.md`](mob-characters.md) (roster) y [`mob-quests-early.md`](mob-quests-early.md) (misiones).

## Modelo

- **Héroes que se desarrollan con el tiempo** (NO renacimiento biológico). El mundo mob juega con el
  tiempo: los personajes que conociste en la Edad de Piedra **reaparecen en cada era**, con **nuevo
  sobrenombre y apariencia** (según el estilo de la época, a menudo encarnando una **figura real**).
- **El jugador los sigue llamando por el primer sobrenombre** (los conoce desde la piedra).
- **N hilos temáticos**; cada época tiene el **mismo número** de personajes (uno por hilo). La
  **afinidad** temática/causal encadena una columna en un **arco épico** (un héroe a través del tiempo).
- **Alineación** por época, puede virar: **héroe · villano · neutral · héroe→villano · villano→héroe**.
- Cada etapa mantiene el arco **agridulce → aprendizaje** (§14) y entrega el elemento de su era.

## Áreas = civilizaciones/regiones (capa espacial)

Los repartos de una época **comparten territorio** (la Antigüedad salió grecomediterránea). Lo
volvemos principio: **Área = civilización/región** (espacio) · **Época = tiempo** · **Hilo = dominio**.

- Como en la Historia real, **el conocimiento migra entre regiones** cuando el centro del mundo se
  desplaza: la astronomía va **Babilonia → Grecia → mundo islámico → Europa**. El hilo (dominio)
  **cambia de área/región con la era** → el jugador ve **pasar la antorcha entre civilizaciones**.
- **Objetivo: representar el mundo entero**, no solo Occidente. Rotar regiones por época y sumar
  figuras de **China** (Confucio, Cai Lun, Zhang Heng), **India** (Aryabhata, Sushruta, Buda),
  **Mesoamérica** (astrónomos mayas), **África** (Imhotep, Mansa Musa, los dogon), **mundo islámico**
  (al-Khwarizmi, Alhacén, Avicena), etc.

## Épocas (10)

1. Edad de Piedra (Paleolítico) · 2. Neolítico · 3. Edad de los Metales (Cobre/Bronce/Hierro) ·
4. Antigüedad clásica · 5. Edad Media · 6. Renacimiento · 7. Ilustración / Revolución científica ·
8. Revolución industrial (s.XIX) · 9. Siglo XX · 10. Actualidad / futuro cercano.

## Hilos (N = 7) — sobrenombres de piedra FIJADOS

El "sobrenombre de piedra" es el **ancla**: el nombre con el que el jugador conoce al héroe en el
Paleolítico y con el que **lo seguirá llamando** en todas las eras, aunque el mundo lo renombre.

| Hilo | Sobrenombre de piedra (ancla) | Tema | Área/dominio | Alineación típica |
|---|---|---|---|---|
| **A · Fuego y Estrellas** | **Guardián del Fuego** | energía, luz, física, cosmos | FuelLab / Guardia nocturna | héroe |
| **B · Barro y Metal** | **El Tallador** | materia, metalurgia, construcción, ingeniería | Mecánica / Textil | héroe |
| **C · Semilla y Vida** | **La Recolectora** | plantas, agricultura, biología, medicina | Huerto / Enfermería / Farmacia | héroe (con nodo oscuro) |
| **D · Trazo y Símbolo** | **La Mano de Lascaux** | arte, escritura, cultura | Estudio Textil | héroe/neutral |
| **E · Corona y Espada** | **El Primer Jefe** | poder, guerra, ley | MonsterSection / hub | villano-capaz / héroe↔villano |
| **F · Aliento y Mente** | **El Chamán** | cuerpo, espíritu, mente | Sala de Yoga / Enfermería | contemplativo |
| **G · Risa y Enredo** | **El Bromista** | humor, juego, astucia, sátira | Mercadillo / plaza (teatro) | neutral-pícaro |

> B y C: el ancla paleolítica (**El Tallador** = primer útil de piedra; **La Recolectora** = quien
> conocía las plantas que alimentan y curan) **se convierte** en el Neolítico en *El Alfarero* y *La
> Sembradora* respectivamente — el jugador los sigue llamando por el ancla.

## Matriz (época × hilo) — esqueleto

Nombres confiables puestos; 🔲 = por decidir juntos. Los sobrenombres arquetípicos (sin nombre real)
van en *cursiva*.

| Época | A Fuego/Estrellas | B Barro/Metal | C Semilla/Vida | D Trazo/Símbolo | E Corona/Espada | F Aliento/Mente | G Risa/Enredo |
|---|---|---|---|---|---|---|---|
| Piedra | *Guardián del Fuego* | *El Tallador* | *La Recolectora* | *La Mano de Lascaux* | *El Primer Jefe* | *El Chamán* | *El Bromista* |
| Neolítico | *El Lector del Cielo* | *El Alfarero* | *La Sembradora* | *La Contadora de Granos* | *El Jefe de la Aldea* | *El Soñador* | *El Aldeano Burlón* |
| Metales | *El Astrónomo de Babel* | *El Fundidor* | Imhotep | Enheduanna | Sargón de Acad | *El Sacerdote del templo* | *El Bufón de la corte* |
| Antigüedad | Ptolomeo | Arquímedes | Hipócrates | Homero | Alejandro Magno | Sócrates ✳ | Aristófanes |
| Media | Alhacén | *Maestro de catedrales* | Avicena | 🔲 | Gengis Kan | *El Místico* | Nasreddín Hoja |
| Renacimiento | Galileo | Brunelleschi | Vesalio | Leonardo da Vinci | 🔲 | Montaigne | Rabelais |
| Ilustración | **Newton** | 🔲 | 🔲 | 🔲 | 🔲 | Spinoza 🔲 | Voltaire |
| Industrial | 🔲 | Watt (vapor) | Pasteur | 🔲 | Napoleón | *El alienado* 🔲 | Mark Twain |
| Siglo XX | Einstein | von Braun (héroe↔villano) | "Los Nombres Robados" ⚠ | Picasso 🔲 | *(dictador)* villano | Freud/Jung | Chaplin |
| Actualidad | **Hawking** | 🔲 | 🔲 | 🔲 | 🔲 | *neurociencia contempl.* | 🔲 |

> Nº por época a igualar en **7** al rellenar; hoy hay huecos (🔲) a propósito. Ötzi y Gilgamesh
> pasan a ser **personajes sueltos** de los Metales (no atados a un hilo).

## Hilo A completo — "El Fuego y las Estrellas" (arco de 10 épocas)

El héroe que el jugador conoció como **Guardián del Fuego** y sigue llamando así, aunque el mundo lo
nombre distinto en cada era. *"El fuego me llevó a las estrellas; las estrellas, a la astronomía; la
astronomía, a la matemática…"*

| Época | Se le conoce como | Beat agridulce → aprendizaje |
|---|---|---|
| Piedra | Guardián del Fuego | el don que nutre también quema → intención |
| Neolítico | El Lector del Cielo | usa las estrellas para sembrar → y para augurios que atan a la gente → mirar sin miedo |
| Metales | El Astrónomo de Babel | primeros catálogos estelares → astrología al servicio del poder → separar el asombro del miedo |
| Antigüedad | Ptolomeo | ordena el cielo → pone la Tierra en el centro (error que dura 1400 años) → el error humilde también enseña |
| Media | Alhacén | funda la óptica y el método → encarcelado, finge locura para sobrevivir → la luz se estudia, no se teme |
| Renacimiento | Galileo | ve las lunas de Júpiter → **condenado** por la Inquisición → la verdad resiste aunque se la silencie |
| Ilustración | **Newton** | gravedad y luz, la gran síntesis → hombre solitario y áspero → el genio no exime de humanidad |
| Industrial | *(el que midió el cielo)* 🔲 | catálogos, nebulosas → el cosmos se vuelve inmenso y frío → asombro ante lo vasto |
| Siglo XX | Einstein | espacio-tiempo → abre la puerta a la bomba → conocimiento + responsabilidad |
| Actualidad | **Hawking** | agujeros negros, el origen → atrapado en un cuerpo que falla (ELA) → la mente vuela aunque el cuerpo no |

Cierre del hilo: del fuego en el suelo a comprender el universo — la mirada sube del suelo al cosmos
(refleja el Eje A y la esperanza de volvernos plenamente racionales, §14).

## Hilo F completo — "El Aliento y la Mente" (arco de 10 épocas)

El espejo interior del Hilo A: si el Fuego mira **afuera** (el cosmos), el Aliento mira **adentro**
(la mente). El jugador lo conoció como **El Chamán** y lo sigue llamando así. Culmina donde vive el
propio juego: la mente **observándose** (meditación).

| Época | Se le conoce como | Beat agridulce → aprendizaje |
|---|---|---|
| Piedra | El Chamán | entra en trance para sanar el alma de la tribu → sus visiones también siembran miedo → distinguir la visión del miedo |
| Neolítico | El Soñador | da sentido a los sueños → …y usa el augurio para mandar → observar la mente sin someterla |
| Metales | *El Sacerdote del templo* | ritual y consuelo comunitario → …el templo exige sacrificios e impuestos → la compasión no necesita altar |
| Antigüedad | **Sócrates** *(o Buda ✳)* | "conócete a ti mismo" → **condenado a la cicuta** → la verdad interior cuesta cara pero libera |
| Media | *El Místico* (Rumi/al-Ghazali) | el amor y el silencio como camino → sospechoso para la ortodoxia → callar también enseña |
| Renacimiento | Montaigne | se observa a sí mismo (el ensayo) → duda de todo → la humildad del "¿qué sé yo?" |
| Ilustración | Spinoza 🔲 | ética desde la razón → **excomulgado** → pensar libre tiene precio |
| Industrial | *El alienado* 🔲 | la mente entra en la fábrica → el trabajo vacía el alma → recuperar el sentido |
| Siglo XX | **Freud / Jung** | cartografían el inconsciente → teorías fallidas, época oscura → mirar dentro es un método, no un dogma |
| Actualidad | *La neurociencia contemplativa* | la meditación se mide en el cerebro → reducir la mente a datos → el mapa no es el territorio; **observa** |

Cierre del hilo: del trance del chamán a la mente que se observa sin miedo — el mismo gesto que el
jugador practica en la sala de yoga. Fuego (afuera) y Aliento (adentro) se tocan en la meditación.

## Corte completo — Época 4: Antigüedad clásica (los 6 hilos)

El primer elenco donde cada hilo tiene una figura **real y reconocible**. Formato:
**Figura** (hilo · área · alineación) — *ancla que continúa*; agridulce → aprendizaje; ayuda (arquetipo).

- **Ptolomeo** (A · Guardia nocturna · héroe) — *Guardián del Fuego, ya astrónomo*.
  Ordena el cielo en un sistema hermoso → pone la Tierra en el centro (error que dura 1400 años) →
  **el error humilde también enseña**. *Ayuda:* observar/buscar-la-raíz. *(alt: Aristarco.)*
- **Arquímedes** (B · Mecánica · héroe) — *El Tallador, ya ingeniero*.
  "¡Eureka!", palancas, tornillo, ingenios → sus máquinas defienden Siracusa; **muere a manos de un
  soldado** → **el saber frente a la violencia**. *Ayuda:* canalizar/resolver su máquina a tiempo
  (reencuadre esperanzador). Elemento: Au (la corona). *(Imhotep queda como figura B de los Metales.)*
- **Hipócrates** (C · Enfermería · héroe) — *La Recolectora, ya médico*.
  Funda la medicina ética ("primero, no dañar") → su medicina (los humores) es casi impotente ante
  la enfermedad → **el juramento vale aunque el saber sea pobre**. *Ayuda:* curar.
- **Homero** (D · Textil/arte · neutral-héroe) — *La Mano de Lascaux, ya bardo* (ciego).
  Inmortaliza la gloria… **y el duelo** (la Ilíada es ira y pérdida) → **el arte guarda la memoria
  del dolor, no solo de la victoria**. *Ayuda:* proteger/recitar.
- **Alejandro Magno** (E · hub/MonsterSection · **héroe↔villano**) — *El Primer Jefe, ya conquistador*.
  Une el mundo conocido y difunde la cultura → **a sangre y fuego**; muere a los 32 y su imperio se
  rompe → **conquistar no es permanecer; la ambición sin freno se consume**. *Ayuda:* llegar-al-corazón.
- **Sócrates** ✳ (F · Sala de Yoga · héroe) — *El Chamán, ya filósofo*.
  "Conócete a ti mismo", partero de ideas → **condenado a la cicuta** por incomodar a la ciudad →
  **la verdad interior cuesta, pero libera**. *Ayuda:* buscar-la-raíz / no-pensar. *(alt: Buda.)*
- **Aristófanes** (G · Mercadillo/teatro · neutral-pícaro) — *El Bromista, ya comediógrafo*.
  La comedia que se burla del poder y de las modas → su sátira ("Las Nubes") ridiculiza a **Sócrates**
  y ayuda a volver la opinión contra él → **la risa es un arma: ilumina o hiere al inocente**.
  *Ayuda:* enredo. *(Enlace entre hilos: Aristófanes ↔ Sócrates — los personajes se cruzan.)*

> **Región:** grecomediterránea (comparten territorio → esta época "es" la civilización griega).
> Otras regiones tienen su reparto paralelo a añadir (China de las Cien Escuelas, India clásica…).
> Reparto Antigüedad = **7** (uno por hilo), balance tonal cubierto: descubrimiento (Ptolomeo/
> Arquímedes), tragedia (Arquímedes/Sócrates), guerra (Alejandro/Homero), sabiduría (Hipócrates/
> Sócrates), **humor (Aristófanes)**.

## Corte completo — Época 2: Neolítico (los 7 hilos)

**Región:** Creciente Fértil (y cunas paralelas independientes: China del mijo/arroz, Mesoamérica).
Aún **sin nombres propios** (no hay escritura) → todos **arquetipos** de alta fidelidad.

- **El Lector del Cielo** (A · Guardia nocturna · héroe) — *Guardián del Fuego, ya lee las estrellas*.
  Usa el cielo para saber cuándo sembrar → …y para augurios que atan a la gente al ritual → **mirar
  el cielo para prever, no para temer**. *Ayuda:* observar.
- **El Alfarero** (B · Cocina/Textil · héroe) — *El Tallador, ya cuece el barro*. La vasija guarda el
  excedente → …guardar permite **acaparar** → **crear para compartir, no para acumular**. *Ayuda:*
  canalizar/moldear.
- **La Sembradora** (C · Huerto · héroe) — *La Recolectora, ya cultiva*. Domesticar el grano trae
  aldeas → …y **jerarquía, propiedad, primeras guerras y enfermedades** (del ganado) → **cultivar
  bien lo que sembramos, en la tierra y en la sociedad**. *Ayuda:* curar/atender. Elem: N.
- **La Contadora de Granos** (D · Textil/arte · neutral-héroe) — *La Mano de Lascaux, del trazo al
  signo*. Marca fichas de barro para **contar el grano** → nace la **escritura… como contabilidad y
  deuda** → **el signo recuerda, pero también controla**. *Ayuda:* ordenar/proteger los signos.
- **El Jefe de la Aldea** (E · hub · villano-capaz/neutral) — *El Primer Jefe, ya guarda el granero*.
  Organiza defensa y reparto → …**concentra el poder** y la primera coerción → **quien guarda el
  grano guarda el poder**. *Ayuda:* llegar-al-corazón.
- **El Soñador** (F · Yoga/Enfermería · contemplativo) — *El Chamán, ya interpreta sueños*. Da sentido
  a los sueños de la aldea → …y usa el augurio para mandar → **observar la mente sin someterla**.
  *Ayuda:* no-pensar.
- **El Aldeano Burlón** (G · plaza · neutral-pícaro) — *El Bromista, ya se ríe de la aldea*. La burla
  alivia la vida sedentaria y sus reglas → …reírse del jefe es peligroso → **la risa señala lo que
  nadie osa decir**. *Ayuda:* enredo.

> Balance tonal Neolítico cubierto. Con esto **Piedra → Neolítico → Metales** quedan completos (el
> tramo del mínimo jugable).

## Corte completo — Época 3: Edad de los Metales (los 7 hilos)

**Región:** Próximo Oriente del Bronce — **Mesopotamia** (Sumer/Acad) y **Egipto**.

- **El Astrónomo de Babel** (A · Guardia nocturna · héroe) — *Guardián del Fuego, ya mira el cielo
  desde el zigurat*. Primeros catálogos de estrellas y el calendario → la astrología al servicio del
  rey (augurios que deciden guerras) → **separar el asombro del miedo**. *Ayuda:* observar.
- **El Fundidor** (B · Mecánica/FuelLab · héroe) — *El Tallador, ya domina el horno*. Funde
  cobre+estaño (bronce) y luego hierro → el mismo horno hace **arados y espadas** → **el que forja
  elige**. *Ayuda:* canalizar/forjar + proteger la forja. Elem: **Cu, Sn, Fe**.
- **Imhotep** (C · Enfermería/Mecánica · héroe) — *La Recolectora, ya primer médico con nombre* (y
  gran constructor de la primera pirámide). Sana y levanta → **deificado** siglos después →
  **se pierde el hombre tras el dios; el saber es humano**. *Ayuda:* curar.
- **Enheduanna** (D · Textil · héroe) — *La Mano de Lascaux, ya primera autora con nombre*. Firma sus
  himnos → un golpe la exilia; **escribe para volver** → **reivindicación**: recuperó su voz.
  *Ayuda:* proteger sus tablillas.
- **Sargón de Acad** (E · hub · **héroe↔villano**) — *El Primer Jefe, ya emperador*. De copero funda
  el **primer imperio** → unifica por conquista; el imperio se hereda y se pudre → **el poder que no
  se cuida corrompe a quien lo hereda**. *Ayuda:* llegar-al-corazón.
- **El Sacerdote del templo** (F · Yoga/Enfermería · contemplativo) — *El Chamán, ya oficia*. Ritual
  y consuelo comunitario → el templo exige sacrificios e impuestos → **la compasión no necesita
  altar**. *Ayuda:* no-pensar / curar.
- **El Bufón de la corte** (G · hub · neutral-pícaro) — *El Bromista, ya bufón del rey*. Junto al
  trono nace el único que puede **decir la verdad riéndose** → …hasta que el rey lo calla → **la risa
  desarma al poder, y el poder teme a la risa**. *Ayuda:* enredo.

**Personajes sueltos de la era** (no atados a un hilo):
- **Ötzi, el Hombre de Hielo** ⭑ — misterio forense real; *buscar-la-raíz* (detalle en
  [`mob-quests-early.md`](mob-quests-early.md)). Elem: Cu.
- **Gilgamesh** ⭑ — capstone legendario de Uruk; *llegar-al-corazón* (aceptar la mortalidad).

> Balance tonal Metales: descubrimiento (Astrónomo/Fundidor/Imhotep), tristeza/reivindicación
> (Enheduanna), poder/terror (Sargón), sabiduría (Sacerdote), **humor (Bufón)**.

## Método de afinidad (para encadenar columnas)

Para unir un slot de una época con el de la siguiente, buscar **descendencia causal/temática**:
- **causal**: el descubrimiento de A hizo posible a B (fuego → horno → fundición…).
- **temática**: mismo dominio/pregunta (mirar el cielo) aunque el medio cambie.
- **de arco**: la alineación evoluciona (un héroe que cae = héroe→villano; un villano que se redime).
La afinidad decide qué figura real "hereda el mantel" del sobrenombre en cada época.

## Estado, rol y fidelidad — continuidad entre arcos

**Todos son "héroes".** Aunque hagan de villano, **forjan la Historia a su manera**; el rol
(héroe / villano / neutro / víctima…) **no es fijo — es fluido**, otra afinidad más.

**Dos afinidades encadenan a un personaje entre épocas:**
1. **Temática/causal** (ver Método de afinidad) — el dominio/hilo.
2. **De estado** *(nueva)* — el **estado físico/mental con que TERMINA su arco** en una época
   **siembra cómo empieza el siguiente**; refleja el efecto de lo vivido.

**Estado de cierre → semilla del arco siguiente.** Ejemplo: un héroe que **lo sacrificó todo y fue
traicionado** reaparece **nervioso e inseguro**, muy distinto del arco anterior. Desde ahí puede
**recuperar fuerzas**, **hundirse más** (y derivar a un rol más oscuro) o **pasar a un papel
secundario**. Es continuidad emocional, no reinicio.

**Nivel de fidelidad (0–1)** — cuán fiel es el personaje a sus principios/núcleo:
- **Alta fidelidad → poca variedad**: constante pese al sufrimiento (un ancla moral).
- **Baja fidelidad → mucha variedad**: oscila de rol y estado con fuerza (impredecible).
- **Todos varían**; la fidelidad regula la **amplitud** → el elenco tiene constantes y veletas.
- *Tendencia:* los arquetipos tempranos (Piedra/Neolítico) son de **alta fidelidad/baja variedad**
  (son roles); al volverse **individuos con nombre** (Metales en adelante) baja la fidelidad y crece
  la variedad.

**Implementación (nota):** cada nodo de hilo por época lleva `{ rol, estadoCierre, fidelidad }`; el
`estadoCierre` alimenta el arranque del nodo siguiente. Emparenta con `trauma`/`mood` de
`CompanionBase` — reutilizable ese mismo tipo de estado.

## Pendiente

- [ ] Confirmar N=6 hilos y sus temas (o ajustar).
- [ ] Rellenar la matriz igualando el nº por época (los 🔲).
- [ ] Asignar alineación por celda (héroe/villano/neutral/híbrido) y marcar los arcos que viran.
- [ ] Elegir sobrenombre "de piedra" de cada hilo (el que usará el jugador siempre).
- [ ] Detallar como en `mob-quests-early.md` las 5 fases de cada etapa cuando toque producir.
- [ ] Validar ✳ (religioso) y ⚠ (tragedia real) con Berón.
