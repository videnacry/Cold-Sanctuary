# Personaje — Irosene

> Ficha de personaje (2026-07-15). Compañera humana. Código: `Assets/Scripts/Companion/Companions/Irosene.cs`.
> Aptitudes en [`creature-stats.md`](creature-stats.md). Sistema de compañeros en `CompanionBase`.

---

## Resumen

Irosene, mujer mayor de **70+ años**, **madre de 9 hijos**. Superviviente de dos hogares
violentos (el paterno y el de su primer matrimonio) que acabó levantando un **negocio de dulces**
de la nada y sacando adelante a sus hijos. Carácter fuerte, algo **altiva/soberbia**, pero también
un **peluche cariñoso**: muy querida por sus nietos y por quien la va conociendo.

En el santuario tuvo una **transformación física y mental enorme**: de casi no poder caminar
(sobrepeso + secuelas de cáncer, poca movilidad de toda la vida) a **correr, escalar y bucear
profundo**. Esa energía y pasión la impulsan. Junto a **Lizmibet** pasó mucho más tiempo del
normal en el Nivel 1 y **aprendieron hechizos avanzados** para acelerar el cambio.

---

## Personalidad y tono

- **Muy expresiva de forma no verbal**: cara, manos, brazos; como si emanara fuerza desde dentro
  para sentimentalizar lo que dice.
- **Salta de registro**: alegría → ira → melancolía → sabiduría. Cuenta chistes, se queja del
  mundo, canta, o comparte conclusiones duras ("valora cada día como si fuera el último", "ama a
  tus padres").
- **Motivadora con muchísimo sentimiento y pasión.**
- **Justiciera**: si ve una injusticia en la calle, se mete a hablar con aire molesto. Cuando tuvo
  cáncer, avisaba a los pacientes de la sala de espera de que ciertos medicamentos eran peligrosos
  y sugería recetas naturales (aprende de libros y canales que ve).
- **Le encanta el karaoke.** Canción favorita: *"soy rebelde porque la vida me hizo así"*.
- Le gustan las **tragaperras**; muchas veces sacó de ahí dinero para comer.

## Estilo de diálogo (contenido para autoría de `DialogueSequence`)

El diálogo se autoría como assets `DialogueSequence` (no versionados). La clase `Irosene` expone
tres slots: `greetingSequence`, `motivationalSequence`, `melancholicSequence`. Selecciona según
su `mood`. Líneas de ejemplo:

**Motivacional** (`motivationalSequence`)
- "Repítelo conmigo: *yo soy el alfa y el omega*. ¡Otra vez! ¡Con fuerza!"
- "Persigue tus sueños, mijo. Nadie va a hacerlo por ti."
- "¿Ves estas manos? Levantaron a nueve hijos. Tú puedes con esto y con más."

**Melancólico** (`melancholicSequence`)
- "Soy rebelde porque la vida me hizo así… (suspira) De niña me escondía a echarle azúcar a la
  avena para que supiera a algo."
- "Hay que llenar de amor a la gente que nos importa, mientras estén. Uno no sabe cuánto le queda."

**Saludo / ambiente** (`greetingSequence`)
- "¡Mírate! Vienes con buena cara hoy."
- "Cuenta, cuenta… aunque yo tengo una historia mejor, jaja."

> Sabe **alentar al jugador en misiones de compañeros** (ver Habilidad especial).

---

## Historia (según lo narrado — confirmar detalles marcados con ⚠️)

- Tuvo a sus hijos **a temprana edad** tras **rebelarse contra su padre**, que quería obligarla a
  casarse con un **doctor** mucho mayor mientras ella aún iba al colegio.
- **Hogar paterno violento**: había que trabajar muy rápido para comer y nunca quedaban satisfechos
  (ni en cantidad ni en sabor); castigos físicos muy duros. Con sus hermanos robaba azúcar a
  escondidas o comía cosas poco notorias; a veces los descubrían y los golpeaban.
- El día que **rechazó al doctor**, su **madre la maltrató** gravemente; la salvó su **hermano
  mayor** (ya más grande).
- Aceptó casarse con **Néstor** (su primer esposo) para escapar, **solo por el odio a su padre** →
  salió de un hogar violento a **otro**: golpes del esposo y desprecio de la familia de él…
  **excepto el sobrino de Néstor**, que decía que se casaría con ella de adulto (⚠️ y ~20 años
  después se volverían pareja).
- ⚠️ Tuvo **un hijo con otro hombre** y "lo soltó" unos 30 años después, bajo alcohol (detalle
  ambiguo — aclarar).
- El esposo la golpeaba borracho; un día su **hijo mayor** se metió a defenderla y **acabó muy
  mal**. Harta, **denunció al esposo** para que no se acercara más.
- **Negocio de dulces**: con las recetas y habilidad de su niñez, puso a sus hijos a trabajar
  mientras estudiaban; con ayuda de sus **~6 hermanos** cubría comida, alquiler, escuelas privadas.
- **Envolver dulces de niña** le dio mucha **velocidad y destreza** que hoy le sirven en el **área
  textil**.

---

## Relaciones / árbol familiar (⚠️ confirmar)

- **Lizmibet** — su gran amiga, misma edad y nivel; probablemente **consuegra** (co-abuela de Leo).
  Candidata a futura compañera (ver Pendientes).
- **Alicia** — hija de Irosene; **exesposa de Josedrilo**.
- **Josedrilo** — **padre de Leo**, exesposo de Alicia. ⚠️ También tuvo un hijo con la **penúltima
  hija** de Irosene (se descubrió ~4 años después de nacer el niño).
- **Leo** — **nieto** de Irosene (y de Lizmibet); hijo de Josedrilo. Está en el **último nivel**
  del santuario. Fue **quien las trajo** al santuario. Cuando el niño (hijo de la penúltima hija)
  cumpla 14, Leo tendría 25. Irosene le escribe pidiendo información de su padre (Josedrilo) para
  cobrar una **mensualidad** por el niño; **Leo se ofrece a darles lo que necesiten**.
- Irosene se queja de que **sus hijos no la ayudan** y le toca **suplicar** dinero, **endeudarse
  con la tarjeta** y pedir a sus hermanos.
- ⚠️ Con sus hijas fue **muy violenta**; a veces pegaba incluso a la hija adulta con quien convivía.
  (Tema delicado — decidir cuánto exponer en diálogo y con qué tono de arco/redención.)

> **Contexto de llegada:** Irosene y Lizmibet tomaron el vuelo **desde Ecuador** para ver a su
> nieto (Leo) **por última vez**, pero en el santuario ganaron fuerza para creer que **les quedan
> muchos años**. No ven a Leo en **más de 10 años**. Hace **un año** que ya se mantienen solas.

---

## Lugar en el santuario y arco

- **Ubicación actual:** nivel **submarino** (Tier 4). **Visita a Panterilia** (Cleaning, Tier 1) y
  tuvo **grandes momentos en el huerto/jardinería** (Garden, Tier 2) hasta tener que subir de nivel.
- **Progresión mágica excepcional:** por quedarse tanto en Nivel 1 (llegaron muy mal físicamente),
  ella y Lizmibet **aprendieron hechizos avanzados**; se dice que fueron **las únicas del Nivel 2
  con hechizos de Nivel 4**. ⚠️ Conciliar con "ubicación actual = Tier 4": interpretación propuesta
  → tuvieron hechizos de Tier 4 siendo aún residentes del Tier 2, y desde entonces ella ascendió al
  submarino.
- **Área textil:** su destreza de envolver dulces la hace destacar allí.
- **Transformación como arco jugable:** de casi inmóvil a atlética. Es un espejo del tema del juego
  (el poder crece con la práctica) encarnado en una persona mayor.

---

## Aptitudes (1.0 = media real) — ver `creature-stats.md`

Codifican **origen + presente transformado**. Origen: fuerza/agilidad/resistencia muy bajas
(obesidad, secuelas de cáncer). Presente (santuario + magia): sube con fuerza.

| Aptitud | Valor | Razón |
|---|---|---|
| agility | 1.2 | origen bajo; destreza de envolver dulces + transformación → ahora trepa |
| perception | 1.3 | autodidacta (remedios), lee a la gente, detecta injusticias |
| strength | 1.2 | de casi no caminar a nadar profundo / escalar |
| bodyMass | 1.3 | sobrepeso; masa alta aunque ya funcional |
| adaptability | 1.6 | dos hogares violentos superados + negocio desde cero + grandes cambios |
| composure | 1.4 | carácter fuerte; se enfrenta a la injusticia, firme y orgullosa |
| endurance | 1.3 | ahora esfuerzo sostenido (correr/escalar/bucear); antes casi nula |
| reasoning | 1.1 | sabiduría práctica/autodidacta (recetas, remedios, negocio) |
| memory | 1.3 | narra su vida con gran detalle; recetas |
| creativity | 1.5 | recetas de dulces, karaoke, ingenio para sobrevivir |
| sociability | 1.7 | hiperexpresiva, motivadora, querida por nietos y desconocidos |
| discipline | 1.1 | montó negocio y puso a los hijos a estudiar+trabajar; tragaperras/deudas lo moderan |

## Anchors (creencias que sesgan su conducta; –1…+1, con deriva de arco)

| Anchor | Peso | Deriva | Significado |
|---|---|---|---|
| `alpha_and_omega` | +0.95 | fijo | autoafirmación ("yo soy el alfa y el omega") |
| `pursue_dreams` | +0.9 | fijo | motor motivacional |
| `love_your_people` | +0.85 | crece | llenar de amor a los suyos |
| `rebellion` | +0.8 | se suaviza | "rebelde porque la vida me hizo así"; nace del trauma, sana en el santuario |
| `pride` | +0.6 | se ablanda | altivez/soberbia → su lado peluche, con el vínculo |

## Habilidad especial — Motivación

- **Canal primario: `Satisfaction`.** Estar cerca de ella **restaura satisfacción/moral** del
  jugador (vía `CompanionBase.GetProximityEffect`).
- **`encouragementBurst`**: golpe de satisfacción inmediato al entrar en su rango — su energía
  levanta el ánimo de golpe.
- **`companionMissionSatisfactionBonus`**: bonus por segundo pensado para **misiones de
  compañeros** (a leer por el sistema de misiones cuando exista ese gancho).
- **Diálogo por ánimo**: con `mood` alto areng (motivacional), con `mood` bajo se pone melancólica,
  si no, saluda.

---

## Pendientes / a confirmar

- [ ] **Lizmibet** como personaje/compañera (gran amiga, misma edad/nivel; co-abuela de Leo).
- [ ] Aclarar los puntos ⚠️ del árbol familiar y de la historia (hijo "soltado", sobrino de Néstor,
      hija con Josedrilo).
- [ ] Conciliar "Nivel 2 con hechizos de Nivel 4" vs. "ubicación actual submarino (Tier 4)".
- [ ] Decidir el tono/arco del tema delicado (violencia hacia sus hijas) en diálogo.
- [ ] Autoría de los assets `DialogueSequence` (greeting/motivacional/melancólico) y cableado en
      su GameObject + `SampleSceneBuilder`.
- [ ] Enganche real de `companionMissionSatisfactionBonus` cuando exista el sistema de misiones de
      compañeros.
