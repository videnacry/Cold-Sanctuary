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

## Épocas (10)

1. Edad de Piedra (Paleolítico) · 2. Neolítico · 3. Edad de los Metales (Cobre/Bronce/Hierro) ·
4. Antigüedad clásica · 5. Edad Media · 6. Renacimiento · 7. Ilustración / Revolución científica ·
8. Revolución industrial (s.XIX) · 9. Siglo XX · 10. Actualidad / futuro cercano.

## Hilos (N = 6)

| Hilo | Tema | Área/dominio | Alineación típica |
|---|---|---|---|
| **A · Fuego y Estrellas** | energía, luz, física, cosmos | FuelLab / Guardia nocturna | héroe |
| **B · Barro y Metal** | materia, metalurgia, construcción, ingeniería | Mecánica / Textil | héroe |
| **C · Semilla y Vida** | agricultura, biología, medicina | Huerto / Enfermería / Farmacia | héroe (con nodo oscuro) |
| **D · Trazo y Símbolo** | arte, escritura, cultura | Estudio Textil | héroe/neutral |
| **E · Corona y Espada** | poder, guerra, ley | MonsterSection / hub | villano-capaz / héroe↔villano |
| **F · Aliento y Mente** | cuerpo, espíritu, mente | Sala de Yoga / Enfermería | contemplativo |

## Matriz (época × hilo) — esqueleto

Nombres confiables puestos; 🔲 = por decidir juntos. Los sobrenombres arquetípicos (sin nombre real)
van en *cursiva*.

| Época | A Fuego/Estrellas | B Barro/Metal | C Semilla/Vida | D Trazo/Símbolo | E Corona/Espada | F Aliento/Mente |
|---|---|---|---|---|---|---|
| Piedra | *Guardián del Fuego* | *El Alfarero* | *La Sembradora* | *La Mano de Lascaux* | *El Primer Jefe* | *El Chamán* |
| Neolítico | *El Lector del Cielo* | *El Constructor de Dólmenes* | 🔲 | 🔲 | 🔲 | 🔲 |
| Metales | *El Astrónomo de Babel* | *El Fundidor* / Ötzi | 🔲 | Enheduanna | Gilgamesh/Sargón | 🔲 |
| Antigüedad | Ptolomeo (o Aristarco) | Imhotep | Hipócrates | Homero | Alejandro / César | Buda / Sócrates ✳ |
| Media | Alhacén (óptica) | *Maestro de catedrales* | Avicena (Ibn Sina) | 🔲 | Gengis Kan | 🔲 |
| Renacimiento | Galileo | Brunelleschi | Vesalio | Leonardo da Vinci | 🔲 | 🔲 |
| Ilustración | **Newton** | 🔲 | 🔲 | 🔲 | 🔲 | 🔲 |
| Industrial | Herschel/Laplace 🔲 | Watt (vapor) | Pasteur | 🔲 | Napoleón | 🔲 |
| Siglo XX | Einstein | von Braun (héroe↔villano) | "Los Nombres Robados" ⚠ | Picasso 🔲 | *(dictador)* villano | Freud/Jung 🔲 |
| Actualidad | **Hawking** | 🔲 | 🔲 | 🔲 | 🔲 | 🔲 |

> El nº por época debe igualarse (6) al rellenar; hoy hay huecos a propósito.

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

## Método de afinidad (para encadenar columnas)

Para unir un slot de una época con el de la siguiente, buscar **descendencia causal/temática**:
- **causal**: el descubrimiento de A hizo posible a B (fuego → horno → fundición…).
- **temática**: mismo dominio/pregunta (mirar el cielo) aunque el medio cambie.
- **de arco**: la alineación evoluciona (un héroe que cae = héroe→villano; un villano que se redime).
La afinidad decide qué figura real "hereda el mantel" del sobrenombre en cada época.

## Pendiente

- [ ] Confirmar N=6 hilos y sus temas (o ajustar).
- [ ] Rellenar la matriz igualando el nº por época (los 🔲).
- [ ] Asignar alineación por celda (héroe/villano/neutral/híbrido) y marcar los arcos que viran.
- [ ] Elegir sobrenombre "de piedra" de cada hilo (el que usará el jugador siempre).
- [ ] Detallar como en `mob-quests-early.md` las 5 fases de cada etapa cuando toque producir.
- [ ] Validar ✳ (religioso) y ⚠ (tragedia real) con Berón.
