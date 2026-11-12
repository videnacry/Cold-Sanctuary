# Obras del edificio de Yoga: Sala de Ballet (P1) y Dojo (P2)

Investigación (2026-10-28) para Berón. Los dos pisos siguen **en obras**, pero aquí queda definido cómo quedan
**terminados**. El dojo debe servir para **MMA, kickboxing y karate**.

Código relacionado:
- `Assets/Scripts/World/ConstructionProject.cs`: `daysRequired`, `daysDone` y `WorkDay(who)`, que suma un día.
- `Assets/Editor/SampleSceneBuilder.Training.cs`, `MakeConstructionSite`: andamio, material del día y conos.
- `Assets/Editor/SampleSceneBuilder.cs` (≈ l. 3511): `MakeConstructionSite(ballet, …, "Sala de Ballet", 5)` y `(dojo, …, "Dojo (artes marciales)", 6)`.
- `Assets/Scripts/Progression/AreaWork.cs`: por obra y día, «Llevar el material» (1 h) y «Trabajar la obra» (2 h).
- `docs/jornada-y-rotacion.md` (5 h de trabajo por persona y día) y `docs/construccion-sostenible.md` (materiales y kit del día).

**Coordenadas.** Son locales del piso, con el origen en su centro: x de −9 a 9 y z de −7,5 a 7,5. −Z es el frente.
Cada piso mide 18 × 15 m, sobre una losa elevada, con unos 11 m libres hasta la losa de arriba. Las paredes son de
bahareque en tres lados. El frente queda abierto en la zona de alfombras.

**Reglas que se respetan** (las mismas que en `entrenamiento-areas.md`):
- zona de alfombras de **7,6 × 4,4 m** en el borde;
- **≥ 2 m** libres entre esa zona y el equipo más cercano;
- no hay escaleras: el ring y la jaula van **a ras de suelo**.

---

## 1. Dojo (MMA, kickboxing y karate) — piso 2, `MartialArts_Area`

### 1.1 ¿Caben los tamaños de competición? **No.**

| Zona | Norma | Huella total |
|---|---|---|
| Octágono UFC | Ø interior ≈ 9,1 m (30 ft); versión pequeña de 7,6 m (25 ft); valla de ≈ 1,8 m | ≈ 10 × 10 m con postes y acolchado |
| Ring de kickboxing WAKO | 5,2–6,1 m de lado entre cuerdas, más un delantal de ≥ 0,5 m | 6,2–7,1 m de lado |
| Kumite WKF | Área de 8 × 8 m (el metro exterior en otro color), más 2 m de área de seguridad (1,5 m si falta sitio) | **12 × 12 m** (11 × 11 como mínimo) |

- Solo las tres zonas suman ≈ 100 + 50 + 144 = **294 m²**, y el piso tiene **270 m²**. A eso se añaden las alfombras
  (33 m²) y sus 2 m libres.
- El tatami WKF con su área de seguridad (12 × 12) ocupa por sí solo casi todo el fondo útil.
- Los dojos y gimnasios de entrenamiento casi nunca usan tamaños de competición. Se propone el **tamaño de entrenamiento**.

### 1.2 Distribución recomendada (tamaño de entrenamiento)

```
 z=+7,5 ┌──────────────────────────────────────────────────────────────┐
        │ ┌────────────────────────────┐ ┌VEST.┐ ┌──────────────────┐   │
        │ │                            │ │2,6×2│ │                  │   │
        │ │   TATAMI KARATE 8 × 8      │ ├─────┤ │  RING KICKBOXING │   │
        │ │   (cuadro de 6 × 6 y una   │ │1.ºAUX│ │  5,2 entre cuerdas│  │
        │ │    franja de 1 m en otro   │ └─────┘ │  6,2 con delantal│   │
        │ │    color)                  │ makiwaras└──────────────────┘   │
        │ │                            │▌estante                         │
        │ └────────────────────────────┘     PASILLO 3 m                 │
 z=−1,1 │ - - - - 2 m libres - - - - - - - -  ┌────────────────┐         │
        │ ┌───────────────────────┐   ○ saco  │  JAULA MMA     │         │
        │ │  ALFOMBRAS 7,6 × 4,4  │   ○ saco  │  octógono 5,5  │         │
        │ │  (frente abierto)     │   ○ saco  │  (18 ft)       │         │
 z=−7,5 └─┴───────────────────────┴───────────┴────────────────┴─────────┘
       x=−9                                                          x=+9
```

| Elemento | Tamaño (X × Z) | Centro (x, z) | Límites | Notas |
|---|---|---|---|---|
| **Zona de alfombras** | 7,6 × 4,4 | (−5,2, −5,3) | x −9,0…−1,4 · z −7,5…−3,1 | Esquina delantera izquierda; libre hasta x 0,6 y z −1,1 |
| **Jaula MMA** (octógono) | 5,5 × 5,5 (18 ft entre caras) | (5,8, −4,4) | x 3,05…8,55 · z −7,15…−1,65 | A ras de suelo. Valla de 1,8 m. Dos puertas en la cara −X, hacia el pasillo |
| **Ring de kickboxing** | 6,2 × 6,2 (5,2 entre cuerdas y 0,5 de delantal) | (5,5, 3,6) | x 2,4…8,6 · z 0,5…6,7 | 5,2 m es el mínimo de WAKO. Ring bajo, sin escalones. 4 cuerdas. A 2,15 m de la jaula |
| **Tatami de karate** | 8 × 8 | (−4,6, 3,1) | x −8,6…−0,6 · z −0,9…7,1 | Es el área WKF sin los 2 m de seguridad: cuadro de 6 × 6 y 1 m de franja de otro color. Sirve también de **tatami abierto** (grappling, calentamiento, kata). A 2,2 m de las alfombras |
| **Sacos pesados** ×3 | Ø 0,35, colgados de un pórtico de guadua a 3 m | x 1,6 · z −6,0 / −4,4 / −2,8 | Barrido x 1,0…2,2 | El que golpea se pone en x ≈ 0,9 y mira hacia +X |
| **Makiwaras de pared** ×2 | 0,2 × 0,1 × 1,2 | (0,3, 4,3) · (1,5, 4,3) | En la pared −Z del vestuario | El que golpea se pone en z ≈ 3,6 y mira hacia +Z |
| **Estantería de equipo** | 0,4 × 4,0 | (−0,3, 2,0) | x −0,5…−0,1 · z 0…4 | Guantes, espinilleras, paos, cascos y protectores bucales |
| **Primeros auxilios** | 2,6 × 1,2 | (0,9, 4,9) | x −0,4…2,2 · z 4,3…5,5 | Camilla plegable, hielo, vendas y botiquín |
| **Vestuario** | 2,6 × 2,0 | (0,9, 6,5) | x −0,4…2,2 · z 5,5…7,5 | Banco y taquillas; la puerta da al pasillo |
| **Pasillo central** | 2,5–3 m de ancho | — | x −0,1…2,4 | Por aquí se va de las alfombras a todas las zonas |

- **Paredes acolchadas** de 2 m de alto donde el tatami o el ring quedan a menos de 1 m de la pared: la izquierda, la
  del fondo y la derecha del ring. Son paneles de micelio forrados de lona de cáñamo.
- **Suelo de impacto**: una base amortiguadora de corcho bajo el tatami, la jaula y el ring (unos 130 m²).
- Con 11 m de altura libre sobra techo. Los sacos cuelgan de su propio pórtico a 3 m, no de la losa.

### 1.3 Materiales sostenibles del dojo

| Pieza | Habitual | En el santuario |
|---|---|---|
| Tatami | Espuma de alta densidad con vinilo | Núcleo de corcho y paja prensada con funda de lona de cáñamo y cera. Losetas de 1 × 1 m, encajadas |
| Valla de la jaula | Malla de acero plastificada | **Postes de guadua doble** con red tensada de cuerda de cáñamo y acolchado de micelio. *Aviso:* una red de cáñamo aguanta menos que la malla de acero. Vale para entrenar, no para competir |
| Cuerdas del ring | Cable de acero con funda | Cuerda de cáñamo gruesa con funda acolchada. Tensores atornillados a la losa |
| Lona del ring o de la jaula | Lona de vinilo sobre espuma | Lona de cáñamo sobre 5 cm de corcho |
| Sacos | Cuero o vinilo con relleno textil | Lona de cáñamo rellena de retales del Estudio Textil y cascarilla de arroz |

---

## 2. Sala de Ballet — piso 1, `Ballet_Area`

### 2.1 Pautas (fuentes en §6)

- **Suelo**: sprung (con amortiguación) mejor que solo resiliente, y encima vinilo de danza (Ausdance). Harlequin monta
  las barras de pie sobre la losa y construye el suelo flotante alrededor.
- **Espacio**: un mínimo de **6 m² por alumno**, y lo ideal 9 m² (Ausdance). One Dance UK pide 5 m² por alumno en
  secundaria.
- **Espejos**: con lámina de seguridad, empotrados a ras de pared y a **15 cm del suelo** (Ausdance). Una pared entera
  de espejo, de unos 2,4 m (8 ft) de alto.
- **Barras**:
  - Ausdance: de 82 a 110 cm, a 15–20 cm de la pared y de 4–5 cm de grosor.
  - Harlequin: la baja a 81–86 cm (32–34") y la alta a 112–117 cm (44–46"). Su soporte de suelo deja la barra alta a 105,5 cm.
- **Temperatura**: unos 21 °C (de 20 a 26 °C).

### 2.2 Distribución recomendada

| Elemento | Tamaño (X × Z) | Centro / posición | Límites | Notas |
|---|---|---|---|---|
| **Zona de alfombras** | 7,6 × 4,4 | (−5,2, −5,3) | x −9,0…−1,4 · z −7,5…−3,1 | Igual que en el dojo, para que las alfombras paren en el mismo sitio |
| **Suelo flotante** | 18 × 8,6 = **155 m²** | (0, 3,2) | x −9…9 · z −1,1…7,5 | Unos 140 m² útiles: de **16 a 23 alumnos** (a 9 y a 6 m² por alumno) |
| **Pared de espejo** | 15,6 × 2,4 (13 paneles de 1,2 × 2,4) | Pared del fondo, z = 7,5 | x −7,8…7,8 · y 0,15…2,55 | Con lámina de seguridad y empotrada. Las clases miran hacia +Z |
| **Barras dobles de pared** | ≈ 30 m en total | Fondo: z 7,3, x −7,8…7,8. Laterales: x ±8,8, z −0,6…6,6 | — | Barra baja a **0,85 m** y alta a **1,10 m**; Ø 4,5 cm; a 0,2 m de la pared; soportes cada 1,2–1,5 m (≈ 26) |
| **Barras portátiles** ×4 | 2,4 m cada una | Guardadas en (3,0, −5,5) | — | Para el centro de la sala |
| **Piano vertical** | 1,5 × 0,6 × 1,3 m | (3,0, −2,3), mirando hacia +Z | Fuera del suelo flotante | El pianista ve la sala y al maestro. Está a 2,4 m de la franja libre de las alfombras |
| **Equipo de sonido** | 0,8 × 0,4 | (4,6, −2,3) | — | Altavoces altos en (±8,5, 2,5 m, 7,0) |
| **Caja de resina y fuente** | 0,6 × 0,6 cada una | (1,5, −1,6) · (5,0, −1,6) | — | En la entrada al suelo flotante |
| **Vestuario y botiquín** | 3,6 × 3,2 | (7,2, −5,9) | x 5,4…9,0 · z −7,5…−4,3 | Banco, taquillas, zapatillas de punta, hielo y vendas |
| Banco de calzado | 3,6 × 0,5 | (3,2, −7,0) | — | Para descalzarse antes de pisar el suelo |

### 2.3 El suelo flotante, con materiales del santuario

De abajo arriba:
1. **Tacos elásticos** de corcho o caucho natural, cada 40 cm.
2. **Rastreles** de guadua aplanada.
3. Doble tablero de **esterilla de guadua prensada**, con las capas cruzadas.
4. **Linóleo natural** de aceite de linaza, harina de corcho y base de yute. Es el mismo lino de los huertos que se usa
   para el acabado de la guadua.

*Aviso:* el «Marley» de danza es vinilo. El linóleo da algo más de agarre, así que hay que probarlo con resina y con
zapatillas de punta. Antes de pegarlo debe aclimatarse unas 24–48 h en la sala. Esa espera alarga la obra un día.

---

## 3. Esfuerzo de obra

### 3.1 Horas-persona reales (estimación)

Las cifras externas son pocas:
- Un suelo flotante prefabricado se monta a 80–100 m² al día con 3 instaladores.
- Una jaula prefabricada sin tornillos la montan 2 personas en unas 2 h.
- Un ring prefabricado lo montan 2 personas «en pocas horas».

Aquí todo se hace **in situ con guadua y bahareque**, más despacio. Los ritmos de la tabla son **estimaciones propias**
(marcadas con \*).

| Partida | Ballet (h·p) | Dojo (h·p) | Ritmo usado |
|---|---|---|---|
| Cerramiento: 175 m² de pared de bahareque (estructura de guadua, esterilla, barro y revoque) | 260 | 260 | 1,5 h·p/m²\* |
| Paneles de micelio (acústica y techo bajo) | 24 | 18 | 0,3 h·p/m²\* (≈ 80 y 60 m²) |
| Suelo flotante in situ (155 m²) | 90 | — | 0,6 h·p/m²\* (unas 3 veces el prefabricado) |
| Linóleo (155 m²), aclimatación aparte | 22 | — | 0,15 h·p/m²\* |
| Base de impacto (130 m²) y tatami (64 losetas) | — | 31 | 0,2 h·p/m² de base y 5 h para el tatami\* |
| Espejos: 13 paneles, 2 personas por panel | 20 | — | ≈ 0,75 h por panel y persona\* |
| Barras: 30 m y 26 soportes | 23 | — | 0,5 h por soporte y 0,3 h/m\* |
| Jaula in situ (8 postes de guadua, red, acolchado, 2 puertas, lona) | — | 40 | Unas 10 veces el kit prefabricado\* |
| Ring a ras de suelo (4 postes anclados, 4 cuerdas, esquineras, lona) | — | 24 | \* |
| Pórtico y 3 sacos, 2 makiwaras y la estantería | — | 28 | \* |
| Acolchado de pared (≈ 45 m²) | — | 18 | 0,4 h·p/m²\* |
| Vestuario y botiquín (tabiques de bahareque, bancos, taquillas) | 40 | 40 | \* |
| Piano, sonido y remates | 15 | 10 | \* |
| **Total** | **≈ 494** | **≈ 469** | |

- Con la cuadrilla de 4 a 8 h de `construccion-sostenible.md`, el ballet son unos **15 días reales** y el dojo unos **15**.
- Es la mitad baja del intervalo de 2–4 semanas que ya da ese documento, así que encaja.

### 3.2 Traducción al juego

- En el juego cada obra recibe al día una misión de «Llevar el material» (1 h) y otra de «Trabajar la obra» (2 h). Cada
  persona trabaja 5 h al día (`WorkRoster.WorkHours`).
- Las ≈ 480 h·p reales no caben en la jornada del juego: 4 personas × 5 h = 20 h·p por día de juego darían 24 días. Hay
  que **comprimir**.
- Se recomienda que **1 día de juego = 1 fase real de obra**. Las fases son las de la tabla de abajo, y cada una cuesta
  ~2,5 días reales de la cuadrilla.
- **`crewNeeded` = 3** por obra y día:
  1. uno lleva el material (1 h);
  2. dos trabajan la obra (2 h).
- Hacen falta dos pares de manos porque los espejos, los postes de la jaula, las cuerdas del ring y los tableros del suelo
  no los coloca una persona sola.
- El campo **no existe** en `ConstructionProject`. Hoy `WorkDay` avanza con una sola persona.

| Obra | `daysRequired` hoy | **Recomendado** | `crewNeeded` | Por qué |
|---|---|---|---|---|
| Sala de Ballet | 5 | **6** | 3 | Suelo flotante + espejos + aclimatación del linóleo (24–48 h): es la obra con más horas |
| Dojo | 6 | **6** (se queda) | 3 | Mismo cerramiento; la jaula, el ring y el tatami pesan algo menos que el suelo de ballet |

### 3.3 Fases y material de cada día

El kit de hoy siempre es el mismo: 4 guaduas, 2 barro con paja, 2 micelio y 1 cuerda. Aquí varía por fase.

- Los kits mantienen las unidades del `MaterialStore`.
- Los materiales **nuevos** van en *cursiva*. Ninguno existe hoy en el almacén.

**Sala de Ballet (6 días)**

| Día | Fase | Material del día |
|---|---|---|
| 1 | Zócalo y estructura de guadua (pies derechos y vigas) | 6 guaduas, 2 cuerdas |
| 2 | Esterilla y bahareque de las paredes | 2 guaduas, 4 barro con paja |
| 3 | Revoque y paneles de micelio (acústica y techo) | 2 barro, 4 micelio |
| 4 | Suelo flotante: tacos, rastreles y doble tablero | 4 guaduas, *1 tacos de corcho* |
| 5 | Linóleo (aclimatado) y espejos | *2 linóleo*, *1 espejos* |
| 6 | Barras, piano, sonido, vestuario y botiquín | 2 guaduas, 1 barro, 1 cuerda |

**Dojo (6 días)**

| Día | Fase | Material del día |
|---|---|---|
| 1 | Zócalo y estructura de guadua | 6 guaduas, 2 cuerdas |
| 2 | Esterilla y bahareque | 2 guaduas, 4 barro con paja |
| 3 | Revoque, paneles de micelio y acolchado de pared | 2 barro, 4 micelio, *1 lona de cáñamo* |
| 4 | Base de impacto, tatami y lonas de la jaula y del ring | *2 corcho*, *2 lona de cáñamo* |
| 5 | Jaula (postes y red) y ring (postes y cuerdas) | 4 guaduas, 4 cuerdas |
| 6 | Sacos, makiwaras, estantería, vestuario y botiquín | 2 guaduas, 1 barro, *1 lona* |

Si no se quieren materiales nuevos, se puede dejar el kit fijo de hoy. Se pierde el detalle, pero no cambia nada más.

### 3.4 Choques con la obra actual

- `MakeConstructionSite` pone el andamio en (−hx + 1,4, −hz + 1) = (−7,6, −6,5), dentro de la zona de alfombras.
- El material del día va en (hx − 2, 1) = (7, 1), justo donde irá el ring del dojo.
- Mientras dure la obra no importa, porque el mobiliario final aún no está.
- Si se amuebla durante la obra, conviene mover el andamio a (7,6, −6,5) y el material a (0,9, −5,0).

---

## 4. Posturas A/B para los puestos del dojo

Convención de `entrenamiento-areas.md` §4: Euler locales en grados; los huesos que no aparecen valen 0.
- Solo se dan ángulos donde la geometría es simple: brazo recto al frente, rodilla a 90°.
- El resto se deja para convertirlo con la skill `asana-to-cubes`.

| Puesto | A | B | Ángulos (primera aproximación) |
|---|---|---|---|
| **Saco: directo** (kickboxing y MMA) | Guardia: puños a la cara, codos cerrados | Directo de derecha: brazo extendido al frente | A: ShoulderL/R (−35, 0, ±10), ForearmL/R (−130, 0, 0). B: ShoulderR (−90, 0, 0) y ForearmR (0, 0, 0); el brazo izquierdo sigue en guardia. Hips (0, −20, 0): la cadera gira hacia el golpe |
| **Saco: patada circular** (kickboxing) | Guardia | Pierna de atrás arriba y rodilla extendida a la altura del saco | Sin ángulos: el giro de la cadera y el pivote necesitan IK |
| **Saco: rodillazo** (muay thai y MMA) | Guardia | Muslo adelantado y rodilla doblada | B: ThighR (−90, 0, 0), ShinR (90, 0, 0) |
| **Makiwara: gyaku-zuki** (karate) | Zenkutsu-dachi con *hikite*: puño en la cadera | Puño de atrás extendido; el otro vuelve a la cadera | B: ShoulderR (−90, 0, 0), ForearmR (0, 0, 0). A: ForearmR (−110, 0, 0), ShoulderR (30, 0, 0). La pierna de delante va flexionada: ThighL (−45, 0, 0), ShinL (45, 0, 0) |
| **Tatami: mae-geri** (karate) | Recogida: rodilla alta | Extensión al frente | A: ThighR (−90, 0, 0), ShinR (100, 0, 0). B: ThighR (−80, 0, 0), ShinR (0, 0, 0) |
| **Tatami: kata** (karate) | Kiba-dachi (postura de jinete) | Golpe lateral | Sin ángulos: postura ancha con rodillas abiertas |
| **Jaula: sprawl** (MMA) | Guardia de MMA | Piernas atrás, caderas al suelo y pecho sobre el rival | Sin ángulos: el cuerpo pasa de vertical a casi horizontal y desplaza la raíz |
| **Jaula: entrada a doble pierna** (MMA) | Guardia baja | Rodilla de delante en la lona y hombro al frente | Sin ángulos: necesita `shiftB` y la rodilla en el suelo |
| **Ring: teep** (kickboxing) | Guardia | Patada frontal de empuje con la planta | Como mae-geri B, con Hips (−10, 0, 0): el tronco se echa algo atrás |

---

## 5. Pendiente para cuando se construya

- `crewNeeded` en `ConstructionProject`: que `WorkDay` solo cuente el día si han trabajado N personas.
- Cambiar `5 → 6` en `SampleSceneBuilder.cs` para el ballet. Actualizar también `jornada-y-rotacion.md` y
  `construccion-sostenible.md`.
- Kits de material por fase, y materiales nuevos en `MaterialStore`: *corcho, linóleo, espejos y lona de cáñamo*.
- `FurnishBallet` y `FurnishDojo`, que se llaman cuando `Finished`, con las posiciones de §1.2 y §2.2.
- Mover el andamio y el material de la obra (§3.4).

## 6. Fuentes

- WKF, *Kumite Competition Rules 2024* (área de 8 × 8 m y 2 m de seguridad, 1,5 m si falta sitio): https://karateliitto.fi/app/uploads/2024/02/WKF_Kumite_Competition_Rules_2024.pdf
- WKF, *2026 Kumite Competition Rules*: https://www.wkf.net/files/pdf/documents/WKF_2026_Kumite_Competition_Rules_MASTER_COPY_V12.pdf
- UFC, *The Octagon*: https://www.ufc.com/octagon
- Wikipedia, *Mixed martial arts cage*: https://en.wikipedia.org/wiki/Mixed_martial_arts_cage
- Grounded MMA (octágono de 30 y 25 ft, valla): https://groundedmma.com/?p=3119
- WAKO, *Ring Sports General Rules*, cap. 7 (520–610 cm entre cuerdas, delantal de ≥ 50 cm): https://wakoodisha.in/images/rules/Ring-Sports-General-Rules.pdf
- Ontario, *Amateur combative sport rules: kickboxing* (5–6 m, delantal de 0,4 m): https://www.ontario.ca/document/amateur-combative-sport-rules-kickboxing-ring-sport-disciplines/part-ii-competitive-field
- Ausdance, *Safe spaces for dance* (suelo, 6–9 m² por alumno, espejos a 15 cm, barras de 82–110 cm, 20–26 °C): https://ausdance.org.au/articles/details/safe-spaces-for-dance
- Harlequin, hoja de barras (32–34" y 44–46"): https://www.arcat.com/datasheets/amharleq/Ballet_Barres_Data_Sheet_2_26.pdf
- Harlequin, soporte de barra de suelo (105,5 cm; se monta sobre la losa con el suelo flotante alrededor): https://source.thenbs.com/product/floor-mounted-double-ballet-barre-bracket/mHjqk2EkfDTyWLnsZWeDiz/bqCnLUpMyHQyHhapik15fx
- Royal Academy of Dance, estudios (suelo sprung Harlequin, barras y paredes de espejo): https://www.royalacademyofdance.org/about-us/venue-hire/dance-studios/
- Dance Teacher, *Equipping your first dance studio* (espejo de ≈ 8 ft): https://dance-teacher.com/studio-space-equipping-first-dance-studio/
- SPECTAT Saltis (80–100 m² al día con 3 instaladores, orientativo): https://www.archiexpo.com/prod/spectat/product-4636976-2552156.html
- Monster Rings & Cages (jaula sin tornillos, piezas numeradas): https://omahasportsandgames.com/products/lowboy-mma-gym-cage-by-monster-rings-and-cages
- Everlast (ring montado por 2 personas en pocas horas): https://everlast.com/products/floor-level-full-size-custom-boxing-ring

Las cifras de RAD por alumno no aparecieron publicadas: se usan Ausdance y One Dance UK. Los ritmos marcados con \* son
estimaciones propias, no datos publicados.
