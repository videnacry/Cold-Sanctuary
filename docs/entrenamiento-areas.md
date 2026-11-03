# Edificio de entrenamiento: gimnasio, cardio, calistenia, parkour y crossfit

Investigación y propuesta del 2026-10-27. Complementa `docs/gimnasio-maquinas.md` (las 10 máquinas de placas y sus
posturas). Petición de Berón:

- el gimnasio pasa a ser un **edificio de pisos completos** (sin entreplanta), con cada máquina separada **unos 2 m**
  de las demás para que funcione lleno de gente;
- una **zona libre** de suelo (abdominales, estiramientos);
- la **calistenia** entra en el edificio del gimnasio;
- zonas profesionales de **calistenia, parkour y crossfit**, repartidas en 2–3 pisos.

Este documento solo propone. No cambia código.

---

## 0. Estado actual

- **Área**: `Gym_Area`, en (26, 0, 6), de 22 × 16 m (`SampleSceneBuilder.cs:3459-3460`), elevada a 12 m
  (`ElevatedHeight`, `SampleSceneBuilder.cs:3596`).
- **Pisos del edificio**: el gimnasio es el piso de abajo. El **Dojo** es su piso de arriba, 11 m más alto
  (`upper = { …, { Dojo, 7 }, … }`, `SampleSceneBuilder.cs:3487-3490`; `UpperFloorGap = 11`,
  `SampleSceneBuilder.Levels.cs:14`). Encima va el techo de alfombras (`SampleSceneBuilder.Transport.cs:85-135`).
- **Dentro del gimnasio** (`FurnishGym`, `SampleSceneBuilder.Training.cs:17-66`):
  - 30 máquinas de placas (10 tipos × 3) en una rejilla de 2,6 × 3,2 m (`:35-36`), 15 abajo y 15 en una
    **entreplanta** de 12,4 × 10 m a 4 m de altura con escalera inclinada (`:28-30`). Hoy la separación real entre
    huellas es de **0,9–1,5 m**, no de 2 m.
  - Zona libre: 4 esterillas (`:45`), 6 kettlebells (`:46-48`), un estante de 3 m con 10 mancuernas (`:49-52`), una
    barra de dominadas a 2,3 m (`:53-56`) y 2 sacos de boxeo (`:57-63`).
- **Calistenia**: es un área aparte, `Calisthenics_Area`, en (−28, 0, −12), de 10 × 9 m (`SampleSceneBuilder.cs:3465`),
  todavía **en obras** («Parque de calistenia», 4 días, `:3499`). Es un tipo de entrenamiento propio
  (`SanctuaryAreaType.Calisthenics`, `WorkdayDirector.cs:48`).
- **Alfombras**: cada piso tiene un área de alfombras de edificio de **7,6 × 4,4 m** (`CarpetPad.BuildZone`,
  `CarpetPad.cs:187-188`). Hoy la sitúa `FreeSpot` donde encuentra hueco (`Transport.cs:127-128`). Aquí se propone una
  posición fija en cada piso.

## 1. Qué hay y qué falta (frente a un gimnasio comercial completo)

| Grupo | Equipo | ¿Existe? |
|---|---|---|
| **Cardio** | Cinta de correr | **No** |
| | Bicicleta estática (vertical y reclinada) | **No** |
| | Remo (ergómetro) | **No** |
| | Elíptica | **No** |
| | Escaladora (stair climber / StepMill) | **No** |
| **Peso libre** | Estante de mancuernas | Sí, pero simplificado: una balda de 3 m con 10 cubos (`Training.cs:49-52`) |
| | Bancos regulables (FID) | **No** |
| | Press de banca plano olímpico | **No** |
| | Press de banca inclinado | **No** |
| | Jaula de sentadillas / power rack (sentadilla trasera) | **No** (la mencionaba el antiguo `<summary>`; nunca se construyó) |
| | Máquina Smith | **No** |
| | Plataforma de peso muerto | **No** |
| | Barras olímpicas y árboles de discos | **No** |
| | Kettlebells | Sí (6) |
| **Máquinas de placas** | Press de pecho, press de hombros, jalón, remo sentado, prensa 45°, extensión de cuádriceps, curl femoral, hip thrust, gemelos, bíceps/tríceps | **Sí**, 3 de cada |
| | Contractora de pecho (pec deck / aperturas) | No |
| | Aductor / abductor | No |
| | Polea doble (cable crossover / functional trainer) | No |
| | Dominadas y fondos asistidos | No |
| **Suelo / abdominales / estiramientos** | Esterillas | Sí (4) |
| | Silla romana (VKR, elevación de piernas) | No |
| | Banco de abdominales declinado | No |
| | Banco de hiperextensiones 45° | No |
| | Espalderas, rodillos, fitballs, bandas | No |
| **Otros** | Barra de dominadas | Sí (1) |
| | Sacos de boxeo | Sí (2) |

**Confirmado**: faltan el **press de banca**, las **cintas**, las **bicis estáticas** y la **jaula de sentadillas**,
como pensaba Berón. Además faltan **todo el cardio** (remo, elíptica y escaladora), los bancos regulables, el press
inclinado, la Smith, la plataforma de peso muerto, el material de abdominales y estiramientos, y cuatro máquinas de
placas habituales: contractora, aductor/abductor, polea doble y dominadas asistidas. La propuesta de abajo añade
todo lo marcado en negrita. Las cuatro máquinas de placas extra quedan como **fase 2** (§3.5).

---

## 2. Cómo son las instalaciones profesionales (investigación)

Las fuentes son normas citadas por terceros, fichas de fabricantes y guías de diseño. Las normas completas (EN 16630,
EN 16899, EN 1177, ASTM F2115) son de pago, así que las cifras normativas vienen de resúmenes y hay que verificarlas si
algún día importan. Las medidas de equipos son de modelos concretos; aquí se toma un valor intermedio.

### 2.1 Separaciones generales del gimnasio

- **Entre estaciones**: unos **0,9 m (36″) como mínimo**, para que pueda pasar el ayudante y se puedan cargar los
  discos. Hay guías que aceptan 0,6 m. Los **pasillos** de circulación miden 1,2–2,1 m (4–7 ft), y al menos uno debe
  cruzar la sala (resúmenes de NSCA y CSCS).
- **Plataformas de halterofilia**: 2,4 × 2,4 m (8 × 8 ft), con un pasillo de **0,9–1,2 m** alrededor.
- **Cardio**: unos 2,2 m² por bici o escaladora, 4,2 m² por cinta y 3,7 m² por remo, contando la separación.
- **Cintas**: **2 m libres detrás** y **0,5 m a cada lado** (ASTM F2115-12; EN 957 también pide los 2 m detrás).
- **Jaula de sentadillas**: la caja mide 1,2–1,3 × 1,2–1,5 m y 2,1–2,3 m de alto. Con la barra olímpica (2,2 m)
  necesita unos 3 × 3 m útiles.
- **Banco olímpico plano** (Life Fitness): 130 × 132 × 130 cm, con un área de uso de **191 × 305 cm**.
- **Smith**: 137–145 × 178 × 211–230 cm.
- **Banco regulable (FID)**: 191 × 71 × 79 cm (Body-Solid FID46).
- **Superficie por persona**: unos **10 m²** en zonas de clase funcional y 14 m² (150 ft²) si hay trineo, cargas o
  movimientos explosivos.

**Decisión de este documento**: **2,0 m libres entre las huellas** de dos equipos vecinos. Es más del doble del mínimo
de la NSCA, y es lo que pide Berón. Los 2 m se pueden **compartir**: el pasillo entre dos máquinas sirve a las dos.
El hueco se cuenta desde la pieza que más sobresale (discos, barra olímpica o palanca). A los bordes de la plataforma se
deja 1 m, que con la holgura de los equipos da más de 2 m hasta el canto.

### 2.2 Parque de calistenia (street workout)

| Elemento | Medidas reales | Fuente |
|---|---|---|
| Barra de dominadas | A **2,3–2,4 m** (KOMPAN: 2,34 m, «todos pueden colgarse sin tocar el suelo»); Redlynch: 2,425 m de alto y 2,32 m de altura de caída libre. Agarre de Ø 38–42 mm. | KOMPAN, Redlynch |
| Barras a varias alturas | Lo habitual son barras escalonadas para distintas estaturas: postes con peldaños para que los bajos alcancen la barra. En un parque de Brisbane se quejan de que una barra a 1,8 m es baja para los altos. | Redlynch, calisthenics-parks.com |
| Paralelas (fondos) | **2,0 × 0,57 × 1,12 m** (Skor), con una zona libre de entrenamiento de **5,06 × 3,56 m**; caída de 1,07–1,30 m. | Skor, KOMPAN, Parko Play |
| Escalera horizontal (monkey bars) | 225 × 146 × 300 cm, con una caída de 1,5 m y usuarios a partir de 1,40 m (Skor). | Skor |
| Combinada con bandera humana | 3,97 × 1,46 × 2,52 m, con una caída libre de 1,40 m (Proludic J5242: fondos, dominadas, flexiones y **human flag**). | Proludic |
| Espalderas | **240 × 90 cm**, 15 cm de fondo y peldaños cada 16–17 cm (13–15 peldaños). Norma DIN EN 1234. | Artimex, Fitwood |
| Anillas | Altura de competición: **2,8 m**. Las anillas cuelgan de una barra separadora unos 3 m más arriba. Aro de Ø 18 cm interior y 2,8 cm de grueso. | Continental Sports, ficha de anillas |
| Paralelas bajas (parallettes) | **25–35 cm** de alto (las de 35 cm sirven para el pino). | Ericflag, YPC |
| Suelo | De **caucho amortiguador**, según la altura de caída de cada aparato (EN 16630 + EN 1177). El grosor no vale por sí solo: cuenta el certificado de altura crítica de caída de la loseta. Orientación: 20–25 mm hasta 0,6 m y 25–35 mm hasta 1,0 m. | Parko Play, Rubberco, CATAS |

### 2.3 Parque de parkour

| Elemento | Medidas reales | Fuente |
|---|---|---|
| Norma | **EN 16899:2016**: alturas de caída, separaciones, atrapamientos y superficies de impacto. Usuarios de 8 años o más. | Parkour UK, iTeh |
| Altura de caída | Altura de caída libre **máxima: 3,0 m**. La versión australiana (AS EN 16899) **no exige superficie amortiguadora por debajo de 1,6 m**; por encima la pide. | Presentación UTS (AS EN 16899) |
| Cajón de salto (vault box) | Promat: 1,2 × 0,9 × 1,2 m y 1,5 × 1,3 × 1,5 m. American Parkour: **1,52 × 0,84 × 0,81 m**. Lappset: caída de 960 mm. | Promat, American Parkour, Lappset |
| Plataformas y muros | Lappset: plataformas a 650, 960 y 1435 mm; estructura de 2450 mm. | Lappset |
| Barandillas | Tubo de **Ø 42 mm** (agarres) a **Ø 60 mm** (barandillas de equilibrio); en barras colgadas, hasta 76 mm. Una barandilla baja «para pisar con precisión, aterrizar y equilibrarse» mide 390 mm. | Lappset, Cloxx |
| Diseño | Las distancias de más de **dos cuerpos** y las alturas de **más de la cabeza** limitan los retos. Las plantas largas y estrechas funcionan mal. Hay que dejar espacio abierto para calentar y montar los recorridos en los dos sentidos (A→B y B→A). | Parkour Park Design Handbook, Parkour Visions |

### 2.4 Box de CrossFit

| Elemento | Medidas reales | Fuente |
|---|---|---|
| Superficie | Box típico de 370–560 m² (4000–6000 ft²). **9–14 m² por atleta** (100–150 ft²; algunas guías piden 14–19 m²). | Skelcore, Glofox, Wodify |
| Rig (estructura) | Rig con anillas y cuerda: desde **3,6 m** de alto, con un techo mínimo de 4 m. Caben 2 atletas por tramo de 1,8 m (6 ft). Rogue USMC: de 8 a 12 atletas en dominadas por tramo de 3,7 m. | KingsBox, Rogue, Kibi |
| Cuerda de trepa | **4,57 m (15 ft)**; techos de 3,7–4,3 m para cuerdas de 12–14 ft. | CrossFit Games, Wodify |
| Diana de wall ball | **3,05 m (10 ft)** para hombres y **2,74 m (9 ft)** para mujeres; balón de 9 y 6 kg (20 y 14 lb). | CrossFit Open 26.1 |
| Cajón pliométrico | **61 cm (24″)** para hombres y **51 cm (20″)** para mujeres; lo habitual es un cajón de 3 alturas: 51, 61 y 76 cm. | CrossFit Open 26.1 |
| Remo Concept2 | Unos 2,2–2,4 × 0,6–0,7 m; asiento a 36 cm (o 51 cm con patas altas). | Concept2 (distribuidor) |
| Assault AirBike | **129 × 59 × 123–127 cm** (Classic); Elite: 140 × 66 × 149 cm. | Spartan Fitness, BLK BOX |
| SkiErg con soporte | **60 × 127 × 216 cm**. | Northern Fitness, Rogue |
| Plataformas | 2,4 × 2,4 m, con 0,9–1,2 m alrededor. | NSCA (resúmenes) |
| Pista de trineo | Ninguna fuente dio una medida. Se propone una pista de césped artificial de **2 × 20 m**: es lo habitual en los boxes, pero no está verificado. | — |

### 2.5 Cardio y peso libre (fichas)

| Equipo | Huella (fondo × ancho × alto) | Fuente |
|---|---|---|
| Cinta | 2,0–2,2 × 0,8–0,9 × 1,4–1,6 m; 2 m detrás y 0,5 m a los lados | Bodycraft T400 (ASTM F2115) |
| Elíptica | **2,09–2,18 × 0,71–0,87 × 1,53–1,68 m** | Life Fitness (Activate, E3, Club Series+) |
| Escaladora | Unos 1,6 × 0,8 × 2,1 m (sin ficha consultada; dato aproximado) | — |
| Bici vertical | Unos 1,2 × 0,6 × 1,4 m (aproximado) | — |
| Bici reclinada | Unos 1,6 × 0,7 × 1,3 m (aproximado) | — |
| Estante de mancuernas | 1,5–2,3 × 0,5–0,85 × 0,85–1,0 m | `gimnasio-maquinas.md` §12 |

---

## 3. Propuesta: edificio de 3 pisos

### 3.1 Huella y edificio

- **Huella por piso: 36 × 26 m** (936 m²), frente a los 22 × 16 de ahora. Todos los pisos son iguales (`SetBack = 0`).
- **Centro**: se propone moverlo de (26, 0, 6) a **(28, 0, 9)**. Así ocupa x de 10 a 46 y z de −4 a 22.
  - Queda a **16 m** del Huerto (z ≤ −20), que es la separación mínima entre vecinos con anillos (memoria
    «restaurant-hospital-gap»).
  - Al oeste, la Construcción termina en x = −11; el resto de áreas están a más de 50 m.
  - **Hay que comprobarlo en escena** (árboles, animales y pistas de alfombras). Si choca algo, la alternativa es dejar el
    centro en (26, 0, 6) y quedarse a 13 m del Huerto.
- **Pisos** (alturas sobre la base elevada a 12 m):

  | Piso | Altura de la losa | Contenido |
  |---|---|---|
  | 0 | 0 m (mundo 12 m) | **Máquinas de placas y peso libre** |
  | 1 | +11 m | **Cardio + zona libre** (abdominales, estiramientos, sacos) |
  | 2 | +22 m | **Calistenia + parkour + crossfit** |
  | Techo | +33 m | Alfombras de edificio y de santuario (como ahora) |

  Con 11 m entre losas sobra altura para el rig (3,6 m), la viga de las cuerdas (5,5 m) y el muro de parkour (2,4 m).
- **El Dojo** deja este edificio. Se recomienda subirlo al de **Yoga + Ballet** como tercer piso:
  `upper = { Ballet → Yoga, Dojo → Ballet }`. Otra opción es hacerlo **piso 3** del gimnasio, con el techo a +44 m.
- **La Calistenia** puede seguir siendo un área propia (`Calisthenics_Area`, con su tipo de entrenamiento y su obra de
  4 días). En vez de estar en (−28, 0, −12), se apila como piso 2 del gimnasio, igual que hoy el Dojo. El piso 1
  (cardio) puede ser otra área `Gym`, por ejemplo `GymCardio_Area`, o un piso que construya `FurnishGym`.
- **Sin entreplanta ni escalera**: desaparecen `Deck("Entreplanta_maquinas")` e `InclinedStair` (`Training.cs:28-30`).
  Se sube de piso en alfombra.

**Coordenadas.** Son locales de cada piso: el origen está en el centro, con +X a la derecha y −Z al **frente**. Los
equipos también miran hacia −Z, igual que en `gimnasio-maquinas.md`. Las posiciones son el **centro de la huella**. El
origen de `GymMachine` no siempre coincide con ese centro: por ejemplo, el de la prensa está unos 0,15 m por delante.
Hay que restar esa diferencia al colocarlas. Las medidas son **ancho en X × fondo en Z**. La zona útil va de
x = −17 a 17 y de z = −12 a 12, dejando 1 m hasta el canto.

### 3.2 Piso 0: máquinas de placas y peso libre

Disposición general:

```
            +Z (fondo)
  ┌────────────────────────────────────────────────┐
  │ F1 Pecho×3  Hombros×3  │pasillo│ Mancuerneros ×2│
  │ F2 Jalón×3  Remo×3     │ 2–3 m │ Bancos FID ×4   │
  │ F3 Prensa×3 Gemelos×3  │central│ Jaulas ×3       │
  │ F4 Extensión×3 Curl×3  │       │ Press banca ×3  │
  │ F5 Hip thrust×3 Bíc.×3 │       │ Smith · P. muerto│
  │      [ALFOMBRAS 7,6×4,4]         Discos/barras   │
  └────────────────────────────────────────────────┘
            −Z (frente)
```

**Bloque de máquinas de placas** (x de −17 a 2, z de −4,5 a 12). Son 5 filas de 6 máquinas, todas mirando hacia −Z,
con 2,0 m entre huellas en X y en Z.

| Fila | z (centro) | Máquina | Huella (X × Z) | x de los centros |
|---|---|---|---|---|
| F1 | 11,3 | Press de pecho 1–3 | 1,0 × 1,3 | −16,5 · −13,5 · −10,5 |
| | | Press de hombros 1–3 | 1,0 × 1,4 | −7,5 · −4,5 · −1,5 |
| F2 | 7,9 | Jalón al pecho 1–3 | 1,2 × 1,2 | −16,4 · −13,2 · −10,0 |
| | | Remo sentado 1–3 | 1,0 × 1,4 | −6,9 · −3,9 · −0,9 |
| F3 | 4,05 | Prensa 45° 1–3 | 1,8 × 2,3 (con discos) | −16,1 · −12,3 · −8,5 |
| | | Gemelos sentado 1–3 | 1,2 × 1,0 | −5,0 · −1,8 · +1,4 |
| F4 | −0,1 | Extensión de cuádriceps 1–3 | 1,3 × 1,2 | −16,35 · −13,05 · −9,75 |
| | | Curl femoral 1–3 | 1,2 × 2,0 | −6,5 · −3,3 · −0,1 |
| F5 | −3,8 | Hip thrust 1–3 | 1,5 × 1,4 (con discos) | −16,25 · −12,75 · −9,25 |
| | | Bíceps y tríceps 1–3 | 1,0 × 1,2 | −6,0 · −3,0 · 0,0 |

- Entre filas quedan 2,0 m libres. F1 ocupa z de 10,6 a 12, F2 de 7,2 a 8,6, F3 de 2,9 a 5,2, F4 de −1,1 a 0,9 y F5 de
  −4,5 a −3,1.
- La fila más ancha es F3, de −17 a 2,0. El **pasillo central** queda entre x = 2 y 4,2.
- El orden por filas permite el circuito «alternar tren superior e inferior» de `gimnasio-maquinas.md` §13 **sin
  cambiar de piso**: las 10 estaciones están abajo.

**Bloque de peso libre** (x de 4 a 17). Las columnas están en x = 5,3 · 9,5 · 13,7, con 2,0 m entre huellas.

| Estación | Cantidad | Huella (X × Z) | Centros (x, z) | Mira hacia | Notas |
|---|---|---|---|---|---|
| Estante de mancuernas (2 baldas a 0,55 y 0,85 m, inclinadas 15°) | 2 | 2,3 × 0,7 | (6,0, 11,6) · (12,5, 11,6) | −Z | Pares de 2 a 40 kg |
| Banco regulable FID (0–85°) | 4 | 0,6 × 1,4 | (4,5, 7,5) · (7,6, 7,5) · (10,7, 7,5) · (13,8, 7,5) | −Z | A 3 m de los estantes |
| **Jaula de sentadillas** (power rack) | 3 | 2,2 × 1,4 (caja de 1,3 × 1,4 más la barra de 2,2) | (5,3, 2,5) · (9,5, 2,5) · (13,7, 2,5) | −Z | Barra a 1,45 m; topes de seguridad a 0,6–0,9 m; 2,3 m de alto |
| **Press de banca plano** olímpico | 2 | 2,2 × 1,7 | (5,3, −2,5) · (9,5, −2,5) | Cabeza hacia +Z | Banco a 0,43 m; soportes a 1,0–1,1 m |
| **Press de banca inclinado** (30–45°) | 1 | 2,2 × 1,7 | (13,7, −2,5) | Mira hacia −Z | Asiento a 0,45 m |
| **Máquina Smith** | 1 | 2,0 × 1,5 | (6,0, −8,0) | −Z | Barra guiada; 2,2 m de alto |
| **Plataforma de peso muerto** (madera y caucho) | 1 | 2,4 × 2,4 | (12,5, −8,0) | −Z | Barra con discos de Ø 0,45 m; 2,0 m libres alrededor |
| Árbol de discos | 2 | 0,6 × 0,6 | (17,1, 2,5) · (17,1, −2,5) | — | Accesorios en la franja del borde |
| Soporte de barras olímpicas | 1 | 0,6 × 0,6 | (17,1, −8,0) | — | |

**Alfombras del piso 0**: zona de **7,6 × 4,4** centrada en (−8,0, −9,8), es decir, x de −11,8 a −4,2 y z de −12 a −7,6.
Queda a 3,1 m de la fila F5. El resto de la franja delantera (x de −17 a −13,8 y de −2,2 a 2) es para la fuente de agua
y el dispensador de toallas.

**Comprobaciones**:
- 30 máquinas de placas + 3 jaulas + 3 bancos de press + Smith + plataforma + 4 bancos FID = **42 puestos**.
- `FurnitureSpacing` (0,9 m) y `SanctuaryLifeTest` (30 `Maquina_*` o más) se siguen cumpliendo.

### 3.3 Piso 1: cardio y zona libre

```
            +Z
  ┌────────────────────────────────────────────────┐
  │   [ALFOMBRAS]          │   Esterillas ×12        │
  │                        │   (3 filas × 4)         │
  │ Remos ×4               │                         │
  │ Bicis ×4 · reclinadas ×2│ VKR ×2 · Banco decl. ×2 │
  │ Elípticas ×4 · Escal. ×2│ Hiperext. ×2 · Sacos ×2│
  │ Cintas ×6  (miran al frente)│ Material · Espalderas│
  └────────────────────────────────────────────────┘
            −Z (frente: vistas)
```

**Cardio** (x de −17 a −1,6). Todos miran hacia −Z, hacia el paisaje. Hay 2 m entre máquinas de lado y **2 m detrás de
cada fila** (lo que pide la ASTM para las cintas; aquí se aplica a todas).

| Fila | z (huella) | Equipo | Cantidad | Huella (X × Z) | x de los centros |
|---|---|---|---|---|---|
| C1 | −12 a −9,9 | **Cinta de correr** | 6 | 0,9 × 2,1 | −16,55 · −13,65 · −10,75 · −7,85 · −4,95 · −2,05 |
| C2 | −7,9 a −5,75 | **Elíptica** | 4 | 0,75 × 2,15 | −16,6 · −13,85 · −11,1 · −8,35 |
| | −7,9 a −6,3 | **Escaladora** | 2 | 0,8 × 1,6 | −5,575 · −2,775 |
| C3 | −3,75 a −2,55 | **Bici estática vertical** | 4 | 0,6 × 1,2 | −16,7 · −14,1 · −11,5 · −8,9 |
| | −3,75 a −2,15 | **Bici reclinada** | 2 | 0,7 × 1,6 | −6,25 · −3,55 |
| C4 | −0,15 a 2,25 | **Remo** (ergómetro) | 4 | 0,6 × 2,4 | −16,7 · −14,1 · −11,5 · −8,9 |

En total son **22 máquinas de cardio**: suficiente para calentar a la vez 22 de los ~30 de un turno lleno.

**Alfombras del piso 1**: zona centrada en (−8,0, 9,8), de x −11,8 a −4,2 y z de 7,6 a 12. Está a 5,3 m de los remos.

**Zona libre** (x de 4 a 17):

| Estación | Cantidad | Huella (X × Z) | Centros (x, z) | Notas |
|---|---|---|---|---|
| **Esterilla** (abdominales, estiramientos, movilidad) | 12 | 1,0 × 2,0 | x 4,5 · 7,5 · 10,5 · 13,5 × z 10,5 · 6,5 · 2,5 | 2 m entre esterillas |
| **Silla romana** (VKR, elevación de piernas) | 2 | 1,0 × 1,0, 2,2 m de alto | (4,5, −2,5) · (7,5, −2,5) | Apoyabrazos a 1,45 m |
| **Banco de abdominales declinado** | 2 | 0,6 × 1,6 | (10,5, −2,8) · (13,1, −2,8) | Declive de 30°; rodillos para los pies en el extremo alto (+Z) |
| **Banco de hiperextensiones 45°** | 2 | 0,8 × 1,3 | (4,5, −7,0) · (7,5, −7,0) | Almohadilla de cadera a ~0,95 m |
| Saco de boxeo (se trae del gimnasio de ahora) | 2 | Ø 0,4, con zona de golpeo de Ø 2,4 | (11,0, −7,0) · (14,0, −7,0) | Colgado de un pórtico de 2,6 m |
| Estantería de material (fitballs, rodillos, bandas, ruedas de abdominales, colchonetas) | 1 | 2,4 × 0,6 | (6,0, −11,2) | |
| Espalderas de estiramiento (2 juntas) | 1 | 1,8 × 0,15, 2,4 m de alto | (12,5, −11,5) | Miran hacia +Z; 1,5 m libres delante |

**Estiramientos**: las 12 esterillas sirven para abdominales y para estirar (bloque final de 15 min,
`gimnasio-maquinas.md` §13). Las espalderas sirven para estirar colgado y de pie.

### 3.4 Piso 2: calistenia, parkour y crossfit

```
            +Z
  ┌───────────────────────────────────────────────────────┐
  │ CALISTENIA (18 × 9,4)                 │ CROSSFIT (14 × 24)│
  │ Dominadas·Monkey bars·Anillas·Parall. │ Rig 10 m + cuerdas│
  │ Paralelas·Fondos·Barras bajas·Bandera·Espald.│ GHD×2 · Wall balls │
  │───────────────────────────────── │ Almacén          │
  │ PARKOUR (11,6 × 12,6)  │ALFOMBRAS│ Plataformas ×3   │
  │ Barandillas·Bloques    │ 4,4×7,6 │ SkiErg×2 · Bikes×2│
  │ Cajones ×2             │         │ Remos ×2         │
  │ Muro alto·Muro medio·Barra lâché│ │ Trineo (2×20 m)  │
  └───────────────────────────────────────────────────────┘
            −Z
```

**Suelo**:
- Caucho de 40 mm en todo el piso.
- Césped artificial en la pista del trineo.
- Bajo los aparatos con **caída de más de 1,6 m** (dominadas, monkey bars, anillas, muro alto, barra de lâché y cuerdas),
  losetas de **80 mm** o colchonetas de aterrizaje de 20 cm. El grosor final lo marca el certificado EN 1177 de la
  altura crítica de caída.

**Alfombras del piso 2**: zona **girada 90°** (4,4 en X × 7,6 en Z) centrada en (−1,2, −8,2), es decir, x de −3,4 a 1,0
y z de −12 a −4,4. Hace de recibidor entre el parkour, a 2 m a su izquierda, y el crossfit, a 2 m a su derecha.
`BuildZone` admite cualquier giro (`rot`).

#### Calistenia (x de −17 a 1, z de 2,6 a 12)

| Estación | Huella (X × Z) | Centro (x, z) | Alturas y medidas | Zona libre |
|---|---|---|---|---|
| **Barras de dominadas a 3 alturas** (3 vanos de 1,2 m) | 3,8 × 0,2 | (−15,1, 10,0) | Barras a **2,0 / 2,3 / 2,5 m**, Ø 38 mm, con peldaños en los postes | 2 m delante y detrás (balanceo) |
| **Escalera horizontal** (monkey bars) | 3,6 × 0,8 | (−9,4, 10,0) | Peldaños a **2,4 m**, cada 0,4 m | 2 m a los lados |
| **Pórtico de anillas** (2 pares) | 3,0 × 0,3, viga a 3,0 m | (−4,1, 10,0) | Anillas regulables de 0,3 a 2,8 m; Ø 18 cm | 2 m delante y detrás |
| **Zona de paralelas bajas** (parallettes) | 1,6 × 2,0 | (0,2, 10,0) | 4 pares de 0,33 m | — |
| **Paralelas largas** (fondos y desplazamientos) | 2,0 × 0,57 | (−16,0, 4,9) | A **1,2 m**, 0,5 m entre barras | Zona libre de 5 × 3,6 m (Skor) |
| **Barras de fondos en V** | 0,8 × 0,6 | (−12,6, 4,9) | Barras a 1,3 m, abiertas de 0,45 a 0,6 m | 2 m |
| **Barras bajas**: remo australiano y front lever | 2,4 × 0,2 | (−9,0, 4,9) | Dos barras: a **1,1 m** (australianas) y a **1,8 m** (front lever agrupado y progresiones) | 2 m delante y detrás (el cuerpo queda en Z) |
| **Poste de bandera humana** | Ø 0,06 con agarres | (−3,8, 5,0) | Poste de 2,6 m con agarres a 0,9 · 1,3 · 1,7 · 2,1 m | Círculo libre de **radio 2 m** (el cuerpo sale horizontal) |
| **Espalderas** (3 cuerpos) | 0,15 × 2,7 (miran hacia −X) | (0,9, 4,95) | 240 × 90 cm cada cuerpo, peldaños cada 16 cm | 1,5 m delante |

#### Parkour (x de −17 a −5,4, z de −12 a 0,6)

En parkour las **distancias entre elementos son el ejercicio** (saltos de 1,2–2,5 m, dos cuerpos como mucho; Handbook).
Por eso aquí la regla de los 2 m no se aplica entre elementos de una misma línea. Se aplica a: la **carrera de impulso**,
la **zona de aterrizaje** detrás de cada elemento y cualquier lado con caída de más de 1,6 m.

| Elemento | Huella (X × Z) | Centro (x, z) | Medidas | Uso y zona libre |
|---|---|---|---|---|
| **Muro alto** (wall run, subida) | 3,0 × 0,3 | (−15,5, −11,6) | **2,4 m** de alto, cara hacia +Z | Carrera de 6,5 m (z de −11,45 a −5) en x de −17 a −14; losetas de 80 mm al pie |
| **Muro medio** (gato, subida) | 2,4 × 0,3 | (−11,0, −11,6) | **1,5 m** | Se llega desde +Z |
| **Pórtico de lâché** (barra colgada) | 0,2 × 2,4 (barra en Z) | (−7,6, −10,8) | Barra a **2,0 m**, Ø 42 mm | Balanceo en X de −9,4 a −5,8 |
| **Cajones de salto** | 0,8 × 1,5 cada uno | (−11,0, −5,5) · (−7,8, −5,5) | **1,0 m** de alto (de 0,8 a 1,2 m) | Impulso de 4 m desde +Z; aterrizaje de 2 m hacia −Z |
| **Barandillas** (equilibrio, precisión) | 2 × (0,06 × 3,0), separadas 1,5 m | x −16,4 y −14,9; z de −2,6 a 0,4 | Una a **0,5 m** y otra a **1,0 m**, Ø 48–60 mm | — |
| **Bloques de precisión** | 6 × (0,5 × 0,5) | x −12,6 · −11,0 · −9,6 · −8,4 · −7,0 · −6,0; z −1,0 (±0,3, al tresbolillo) | Alturas de 0,40 · 0,50 · 0,65 · 0,50 · 0,40 · 0,65 m | Saltos de 1,0–1,6 m |

**Recorrido de ejemplo** (de B a A también):
barandillas → bloques de precisión → cajones (kong o speed vault) → muro medio (subida) → barra de lâché → vuelta.

#### Crossfit (x de 3 a 17, z de −12 a 12; ≈ 336 m², para **16–20 atletas**)

| Estación | Cantidad | Huella (X × Z) | Centros (x, z) | Medidas y notas |
|---|---|---|---|---|
| **Pista de trineo** (césped) | 1 | 2,0 × 20 | (4,0, −1,0): z de −11 a 9 | 2 trineos de 1,0 × 0,7 m; 2 m libres hasta la calistenia |
| **Plataforma** (halterofilia y cargadas) | 3 | 2,4 × 2,4 | (8,2, −9,8) · (8,2, −5,4) · (8,2, −1,0) | Barra con discos de goma (bumpers); 2 m entre plataformas |
| **Remo** | 2 | 0,6 × 2,4 | (11,7, −10,8) · (14,3, −10,8) | (Hay 4 más en el piso 1.) |
| **Assault bike** | 2 | 0,6 × 1,3 | (11,7, −6,95) · (14,3, −6,95) | |
| **SkiErg** con soporte | 2 | 0,6 × 1,3 | (11,7, −3,65) · (14,3, −3,65) | 2,16 m de alto |
| Estante de kettlebells | 1 | 0,5 × 2,0 | (16,9, −7,0) | 8–32 kg; los swings se hacen en x 15–16 |
| **Rig** | 1 | 1,8 × 10 | (12,5, 6,0): z de 1 a 11 | 3,6 m de alto. 5 vanos de 2 m con 2 dominadas por lado a 2,3 y 2,6 m = **20 dominadas**; anillas en un vano; 2 soportes para sentadilla con J-hooks |
| **Cuerdas de trepa** | 2 | En el vano final del rig (z de 9 a 11) | — | Viga a 5,5 m; cuerdas de **4,57 m** con marca |
| **Dianas de wall ball** | 3 | En la cara +X del rig | z 2,5 · 5,0 · 7,5 | Dianas a **3,05 y 2,74 m**; se lanza desde x 13,4 a 15,8 |
| **GHD** (glute-ham developer) | 2 | 0,8 × 1,8 | (8,2, 3,5) · (8,2, 7,5) | Almohadilla de cadera a ~1,0 m |
| Almacén: estante de bumpers | 1 | 1,0 × 1,2 | (16,5, 2,0) | |
| Almacén: cajones pliométricos | 6 | Pila de 0,8 × 0,9 | (16,5, 4,5) | Cajones de 3 alturas: **51 / 61 / 76 cm** |
| Almacén: balones medicinales | 1 | 0,6 × 1,5 | (16,5, 7,0) | 6 y 9 kg |
| Almacén: soporte de barras | 1 | 0,6 × 1,0 | (16,5, 9,5) | |
| Suelo abierto | — | x de 6 a 10,6 · z de 9,6 a 12 (y entre puestos) | — | Burpees, comba, cajones pliométricos |

### 3.5 Fase 2 (opcional): máquinas de placas que faltan

Contractora, aductor/abductor, polea doble (3,6 × 1,2 m) y dominadas asistidas. No caben con 3 copias de todo y 2 m
libres. Hay dos opciones:
1. **Dejar en 2 copias** el hip thrust y los gemelos, que son los tipos menos usados, y poner en esos 2 huecos la
   contractora y el aductor/abductor (huellas de 1,4 × 1,6, dentro de F3/F5).
2. **Ensanchar a 40 × 26 m** y añadir una fila de 4 m en el piso 0.

### 3.6 Resumen de separaciones

| Caso | Separación |
|---|---|
| Entre huellas (máquinas, bancos, jaulas, cardio) | **≥ 2,0 m** compartidos; se mide desde los discos o la barra si sobresalen |
| Detrás de cada fila de cardio | **2,0 m** (las cintas lo exigen por ASTM F2115 y EN 957) |
| Pasillo central del piso 0 | 2,2–3,1 m |
| Plataformas de halterofilia | 2,0 m (la NSCA pide 0,9–1,2) |
| Calistenia y anillas (balanceo) | 2 m delante y detrás de la barra |
| Bandera humana | Círculo de radio 2 m alrededor del poste |
| Parkour | Impulso de 4–6,5 m y aterrizaje de 2 m; dentro de una misma línea, la distancia la marca el salto |
| Áreas de alfombras | ≥ 2 m al equipo más cercano |
| Borde de la plataforma | 1 m de franja libre más la holgura del equipo |

### 3.7 Efectos en el código (para cuando se construya)

- `BuildAreaBox("Gym_Area", …)`: tamaño **36 × 26** y centro **(28, 0, 9)**. También hay que cambiar `sizes[7]`
  (`SampleSceneBuilder.cs:3483`).
- Hay que quitar `{ Dojo, 7 }` de `upper` y llevar el Dojo a otro edificio (§3.1). Luego se apila un piso de cardio y,
  encima, `Calisthenics_Area` (`upper[Calisthenics] = cardio`, `upper[cardio] = Gym`). Al apilar, el área de calistenia
  toma el tamaño de 36 × 26.
- `MakeConstructionSite(… "Parque de calistenia", 4)` sigue valiendo: es la obra del piso 2. El parkour y el crossfit
  pueden ir en esa obra o en una segunda.
- `FurnishGym`: quitar `Deck` e `InclinedStair` (`Training.cs:28-30`) y las 15 máquinas de arriba. Hay que colocar las
  30 máquinas en las filas F1–F5 de §3.2. Nuevos constructores: `GymRack`, `GymBench`, `GymSmith`, `GymPlatform`,
  `Cardio(type)`, `CalistheniaStation(type)`, `ParkourElement(type)` y `CrossfitStation(type)`. Cada uno lleva su
  `Puesto` y su `GymStation` con las posturas de §4.
- Las alfombras de cada piso deberían ir en las posiciones fijas de §3.2–3.4 y no por `FreeSpot`.
- En el horario de `gimnasio-maquinas.md` §13, el bloque «subir a la entreplanta» pasa a ser:
  - calentar en el piso 1 (cardio);
  - hacer el circuito entero en el piso 0;
  - estirar en el piso 1.

  Son 2 viajes en alfombra. La calistenia, el parkour y el crossfit son sesiones propias del piso 2.
- `docs/jornada-y-rotacion.md:91` debe describir el gimnasio nuevo.

---

## 4. Posturas A/B de cada equipo nuevo

Convención idéntica a `gimnasio-maquinas.md` §1:
- Euler (X, Y, Z) locales, en grados.
- `Hips` gira el **cuerpo entero** (X− hacia atrás: −90 es tumbado boca arriba; X+ hacia delante: +90 es boca abajo).
- `Back`/`Chest` X+ flexiona.
- `Neck`/`Head` X+ baja la barbilla.
- `Shoulder` X− sube el brazo por delante (−90 al frente, −175 arriba) y Z lo abre (Left Z+, Right Z−).
- `Forearm` X− dobla el codo.
- `Thigh` X− adelanta el muslo (−90 sentado).
- `Shin` X+ dobla la rodilla.
- `±` en Z se lee izquierda/derecha. Los huesos que no aparecen valen 0.

**`rootHeight`** = altura de la pelvis en unidades nativas (de pie, 0,95; sentado, asiento / s + 0,11; tumbado,
(apoyo + 0,11 s) / s, con s = 0,974 para 1,85 m).

**Cómo se suman los ángulos.** El ángulo en el mundo de un muslo es `Hips.X + Thigh.X`, y el de una espinilla es la suma
de los tres. Con esa suma se han calculado las posturas en las que el tronco se inclina: sentadillas, peso muerto y
remo.

**Puesto que se mueve.** Si la raíz se desplaza durante la repetición, se indica con `shiftB` (en metros, en ejes de la
estación).

Los ángulos son una **primera aproximación** que hay que revisar a ojo. El rig no tiene IK, y los agarres anchos o
girados se resuelven con la variante neutra (como en `gimnasio-maquinas.md`).

### 4.1 Piso 0: peso libre

**Jaula de sentadillas: sentadilla trasera.** Puesto en el centro de la jaula, mirando hacia −Z.
`rootHeight` A **0,95** → B **0,48**. `shiftB` = (0, 0, +0,19): en B la pelvis retrocede.

| Hueso | A · de pie, barra en la espalda | B · abajo (muslo paralelo) |
|---|---|---|
| Hips | (5, 0, 0) | (40, 0, 0): tronco inclinado 40° |
| Back / Chest | 0 / 0 | 0 / 0 |
| Neck / Head | 0 / 0 | (−20, 0, 0) / (−10, 0, 0): mira al frente |
| ShoulderL/R | (20, 0, ±70): manos abiertas en la barra | igual |
| ForearmL/R | (−110, 0, 0) | igual |
| ThighL/R | (−5, 0, ±8) | (−130, 0, ±15): muslo horizontal en el mundo |
| ShinL/R | (0, 0, 0) | (125, 0, 0): espinilla adelantada 35° |

**Press de banca plano.** Tumbado boca arriba con la cabeza hacia +Z y los ojos bajo la barra.
`rootHeight` **0,55** (banco a 0,43 m).

| Hueso | A · barra en el pecho | B · brazos extendidos |
|---|---|---|
| Hips | (−90, 0, 0) | (−90, 0, 0) |
| Back / Chest | 0 / (−5, 0, 0): pecho arriba | 0 / (−5, 0, 0) |
| Neck / Head | 0 / 0 | 0 / 0 |
| ShoulderL/R | (−10, 0, ±55): codos a ~50° del tronco | (−88, 0, ±10): brazo vertical |
| ForearmL/R | (−100, 0, 0) | (−5, 0, 0) |
| ThighL/R | (15, 0, ±20): pies en el suelo | igual |
| ShinL/R | (75, 0, 0) | igual |

**Press de banca inclinado (respaldo a 30° de la horizontal).** Sentado, mirando hacia −Z.
`rootHeight` **0,57** (asiento a 0,45 m).

| Hueso | A · barra en la parte alta del pecho | B · brazos verticales |
|---|---|---|
| Hips | (−60, 0, 0) | (−60, 0, 0) |
| Back / Chest | 0 / 0 | 0 / 0 |
| Neck / Head | (10, 0, 0) / 0 | igual |
| ShoulderL/R | (−20, 0, ±55) | (−120, 0, ±10): brazo vertical en el mundo |
| ForearmL/R | (−110, 0, 0) | (−5, 0, 0) |
| ThighL/R | (−30, 0, ±15): muslo horizontal en el mundo | igual |
| ShinL/R | (90, 0, 0) | igual |

**Máquina Smith: sentadilla con los pies adelantados.** Mirando hacia −Z.
`rootHeight` A **0,95** → B **0,53**. `shiftB` = (0, 0, +0,10).

| Hueso | A · de pie | B · abajo |
|---|---|---|
| Hips | (0, 0, 0) | (25, 0, 0): más erguido que en la jaula |
| Back / Chest | 0 / 0 | 0 / 0 |
| Neck / Head | 0 / 0 | (−15, 0, 0) / 0 |
| ShoulderL/R | (20, 0, ±70) | igual |
| ForearmL/R | (−110, 0, 0) | igual |
| ThighL/R | (−5, 0, ±8) | (−115, 0, ±12) |
| ShinL/R | (0, 0, 0) | (95, 0, 0): espinilla casi vertical |

**Plataforma: peso muerto convencional.** Barra sobre el empeine, mirando hacia −Z.
`rootHeight` A **0,64** → B **0,95**. `shiftB` = (0, 0, −0,10): la cadera avanza.

| Hueso | A · abajo (barra a 0,22 m) | B · bloqueo arriba |
|---|---|---|
| Hips | (65, 0, 0) | (0, 0, 0) |
| Back / Chest | (5, 0, 0) / 0: espalda neutra | 0 / (−5, 0, 0) |
| Neck / Head | (−30, 0, 0) / (−10, 0, 0) | 0 / 0 |
| ShoulderL/R | (−65, 0, ±5): brazos verticales en el mundo | (0, 0, ±5) |
| ForearmL/R | (0, 0, 0) | (0, 0, 0) |
| ThighL/R | (−135, 0, ±10) | (0, 0, ±8) |
| ShinL/R | (95, 0, 0) | (0, 0, 0) |

**Mancuernas y banco FID: remo a una mano.** La rodilla y la mano izquierdas apoyan en el banco y la mano derecha rema.
Mirando hacia −Z, en el lado +X del banco. `rootHeight` **0,95**.

| Hueso | A · brazo colgando | B · codo arriba |
|---|---|---|
| Hips | (80, 0, 0): tronco casi horizontal | igual |
| Back / Chest | 0 / 0 | 0 / (0, −10, 0): leve giro del tronco |
| Neck / Head | (−20, 0, 0) / 0 | igual |
| ShoulderLeft | (−75, 0, 5): apoyo en el banco | igual |
| ForearmLeft | (−20, 0, 0) | igual |
| ShoulderRight | (−80, 0, −5) | (15, 0, −10): codo pegado y por encima de la espalda |
| ForearmRight | (0, 0, 0) | (−90, 0, 0) |
| ThighLeft / ShinLeft | (−80, 0, 0) / (90, 0, 0): rodilla en el banco | igual |
| ThighRight / ShinRight | (−80, 0, −10) / (0, 0, 0): pie en el suelo | igual |

### 4.2 Piso 1: cardio

**Cinta de correr.** Mirando hacia −Z; la cinta se mueve y la raíz no. `rootHeight` **0,93** (con 0,04 de plataforma).
A y B son simétricas, unos 0,35 s cada una.

| Hueso | A · pie izquierdo delante | B · pie derecho delante |
|---|---|---|
| Hips | (8, 0, 0) | (8, 0, 0) |
| ShoulderL/R | (25, 0, 8) / (−40, 0, −8) | (−40, 0, 8) / (25, 0, −8) |
| ForearmL/R | (−90, 0, 0) / (−90, 0, 0) | igual |
| ThighL / ShinL | (−35, 0, 0) / (15, 0, 0) | (20, 0, 0) / (80, 0, 0) |
| ThighR / ShinR | (20, 0, 0) / (80, 0, 0) | (−35, 0, 0) / (15, 0, 0) |

**Bici estática vertical.** Sillín a 0,95 m. `rootHeight` **1,09**. A y B son los pedales arriba y abajo
(1 vuelta ≈ 0,8 s).

| Hueso | A · pedal izquierdo arriba | B · pedal derecho arriba |
|---|---|---|
| Hips | (20, 0, 0) | igual |
| Neck / Head | (−15, 0, 0) / 0 | igual |
| ShoulderL/R | (−60, 0, ±10): manos en el manillar | igual |
| ForearmL/R | (−20, 0, 0) | igual |
| ThighL / ShinL | (−90, 0, 0) / (110, 0, 0) | (−40, 0, 0) / (30, 0, 0) |
| ThighR / ShinR | (−40, 0, 0) / (30, 0, 0) | (−90, 0, 0) / (110, 0, 0) |

**Bici reclinada.** Asiento a 0,50 m y respaldo a 25°. `rootHeight` **0,62**.

| Hueso | A | B |
|---|---|---|
| Hips | (−25, 0, 0) | igual |
| ShoulderL/R | (−10, 0, ±20): asas laterales | igual |
| ForearmL/R | (−40, 0, 0) | igual |
| ThighL / ShinL | (−85, 0, 0) / (100, 0, 0) | (−55, 0, 0) / (25, 0, 0) |
| ThighR / ShinR | (−55, 0, 0) / (25, 0, 0) | (−85, 0, 0) / (100, 0, 0) |

**Elíptica.** Pedales a ~0,25 m. `rootHeight` **1,15**. Brazos y piernas van cruzados.

| Hueso | A | B |
|---|---|---|
| Hips | (5, 0, 0) | igual |
| ShoulderL / ShoulderR | (−20, 0, 10) / (−55, 0, −10) | (−55, 0, 10) / (−20, 0, −10) |
| ForearmL / ForearmR | (−90, 0, 0) / (−45, 0, 0) | (−45, 0, 0) / (−90, 0, 0) |
| ThighL / ShinL | (−30, 0, 0) / (40, 0, 0) | (10, 0, 0) / (20, 0, 0) |
| ThighR / ShinR | (10, 0, 0) / (20, 0, 0) | (−30, 0, 0) / (40, 0, 0) |

**Escaladora (StepMill).** Peldaños de 0,20 m. `rootHeight` **1,10**.

| Hueso | A · pie izquierdo arriba | B · pie derecho arriba |
|---|---|---|
| Hips | (10, 0, 0) | igual |
| ShoulderL/R | (−30, 0, ±15): barandillas | igual |
| ForearmL/R | (−50, 0, 0) | igual |
| ThighL / ShinL | (−50, 0, 0) / (60, 0, 0) | (0, 0, 0) / (5, 0, 0) |
| ThighR / ShinR | (0, 0, 0) / (5, 0, 0) | (−50, 0, 0) / (60, 0, 0) |

**Remo (ergómetro).** Asiento a 0,36 m. `rootHeight` **0,48**. `shiftB` = (0, 0, +0,60): el carro recorre 0,6 m hacia
atrás. Ritmo: A→B en 1 s y B→A en 1,5 s.

| Hueso | A · ataque (catch) | B · final |
|---|---|---|
| Hips | (25, 0, 0) | (−25, 0, 0) |
| Back / Chest | 0 / 0 | 0 / 0 |
| ShoulderL/R | (−105, 0, ±8): brazos al frente | (35, 0, ±12): codos detrás del tronco |
| ForearmL/R | (0, 0, 0) | (−120, 0, 0): mango en el esternón |
| ThighL/R | (−150, 0, ±10): rodillas al pecho | (−60, 0, ±5) |
| ShinL/R | (125, 0, 0): espinilla vertical | (5, 0, 0) |

### 4.3 Piso 1: zona libre

**Esterilla: crunch abdominal.** Tumbado boca arriba. `rootHeight` **0,13**.

| Hueso | A · tumbado | B · encogido |
|---|---|---|
| Hips | (−90, 0, 0) | igual |
| Back / Chest | 0 / 0 | (30, 0, 0) / (20, 0, 0) |
| Neck / Head | 0 / 0 | (15, 0, 0) / 0 |
| ShoulderL/R | (−150, 0, ±60): manos tras la nuca | igual |
| ForearmL/R | (−130, 0, 0) | igual |
| ThighL/R | (−45, 0, ±10): rodillas arriba | igual |
| ShinL/R | (90, 0, 0): pies en el suelo | igual |

**Esterilla: estiramiento de isquios sentado.** Sentado en el suelo. `rootHeight` **0,13**.

| Hueso | A · erguido | B · plegado |
|---|---|---|
| Hips | (0, 0, 0) | (30, 0, 0) |
| Back / Chest | 0 / 0 | (25, 0, 0) / (20, 0, 0) |
| Neck / Head | 0 / 0 | (15, 0, 0) / 0 |
| ShoulderL/R | (−60, 0, ±10) | (−80, 0, ±8): manos a los pies |
| ThighL/R | (−90, 0, ±5) | (−120, 0, ±5): piernas planas en el suelo |
| ShinL/R | (0, 0, 0) | (0, 0, 0) |

**Silla romana (VKR): elevación de rodillas.** Espalda en el respaldo, antebrazos en los apoyos. `rootHeight` **1,30**.

| Hueso | A · piernas colgando | B · rodillas arriba |
|---|---|---|
| Hips | (0, 0, 0) | (−10, 0, 0) |
| Back / Chest | 0 / 0 | (5, 0, 0) / 0 |
| ShoulderL/R | (−10, 0, ±10) | igual |
| ForearmL/R | (−90, 0, 0): sobre los apoyos | igual |
| ThighL/R | (0, 0, ±5) | (−100, 0, ±8) |
| ShinL/R | (0, 0, 0) | (100, 0, 0) |

**Banco de abdominales declinado (30°).** Pies enganchados en el extremo alto. `rootHeight` **0,67**.

| Hueso | A · tumbado (cabeza abajo) | B · incorporado |
|---|---|---|
| Hips | (−120, 0, 0) | (−45, 0, 0) |
| Back / Chest | 0 / 0 | (10, 0, 0) / 0 |
| ShoulderL/R | (−40, 0, ±20): brazos cruzados | igual |
| ForearmL/R | (−130, 0, 0) | igual |
| ThighL/R | (−30, 0, ±5) | (−105, 0, ±5): el muslo no se mueve en el mundo |
| ShinL/R | (90, 0, 0) | igual |

**Hiperextensión 45°.** Cadera en la almohadilla y talones en el rodillo. `rootHeight` **0,95**.

| Hueso | A · abajo (tronco colgando) | B · arriba (cuerpo en línea a 45°) |
|---|---|---|
| Hips | (125, 0, 0) | (45, 0, 0) |
| Neck / Head | 0 / 0 | (−20, 0, 0) / 0 |
| ShoulderL/R | (−40, 0, ±20): brazos cruzados | igual |
| ForearmL/R | (−130, 0, 0) | igual |
| ThighL/R | (−80, 0, ±5): las piernas siguen a 45° | (0, 0, ±5) |
| ShinL/R | (0, 0, 0) | (0, 0, 0) |

**Saco de boxeo: directo (jab).** De perfil, con el pie izquierdo delante. `rootHeight` **0,90**.

| Hueso | A · guardia | B · directo de izquierda |
|---|---|---|
| Hips | (5, −20, 0) | (8, −30, 0) |
| ShoulderLeft / ForearmLeft | (−40, 0, 15) / (−120, 0, 0) | (−90, 0, 5) / (−5, 0, 0) |
| ShoulderRight / ForearmRight | (−30, 0, −15) / (−130, 0, 0) | igual |
| ThighL / ShinL | (−15, 0, 8) / (10, 0, 0) | (−20, 0, 8) / (15, 0, 0) |
| ThighR / ShinR | (10, 0, −8) / (15, 0, 0) | igual |

**Espalderas** de estiramiento: se usan las posturas de §4.4.

### 4.4 Piso 2: calistenia

**Barra de dominadas** (barra a 2,4 m). `rootHeight` A **1,23** → B **1,70**. Para otras alturas, se suma o resta lo
mismo que suba o baje la barra, en unidades nativas.

| Hueso | A · colgado | B · barbilla sobre la barra |
|---|---|---|
| Hips | (0, 0, 0) | (−5, 0, 0) |
| Neck / Head | (−10, 0, 0) / 0 | (−15, 0, 0) / 0 |
| ShoulderL/R | (−175, 0, ±15) | (−30, 0, ±35) |
| ForearmL/R | (0, 0, 0) | (−140, 0, 0) |
| ThighL/R | (−10, 0, 0) | (−15, 0, 0) |
| ShinL/R | (60, 0, 0): pies atrás | (70, 0, 0) |

**Escalera horizontal (monkey bars).** Peldaños a 2,4 m. `rootHeight` **1,23**. Avanza 0,4 m por mano.

| Hueso | A · mano izquierda delante | B · mano derecha delante |
|---|---|---|
| Hips | (5, 0, 0) | (−5, 0, 0) |
| ShoulderLeft / ShoulderRight | (−175, 0, 10) / (−150, 0, −10) | (−150, 0, 10) / (−175, 0, −10) |
| ForearmL/R | (0, 0, 0) | igual |
| ThighL / ShinL | (−20, 0, 0) / (30, 0, 0) | (10, 0, 0) / (30, 0, 0) |
| ThighR / ShinR | (10, 0, 0) / (30, 0, 0) | (−20, 0, 0) / (30, 0, 0) |

**Paralelas y barras de fondos: fondos** (barras a 1,2 m). `rootHeight` A **1,33** → B **1,08**.

| Hueso | A · arriba (brazos rectos) | B · abajo (codo a 90°) |
|---|---|---|
| Hips | (5, 0, 0) | (20, 0, 0) |
| ShoulderL/R | (0, 0, ±5) | (85, 0, ±8): brazo horizontal hacia atrás |
| ForearmL/R | (0, 0, 0) | (−90, 0, 0) |
| ThighL/R | (−10, 0, 0) | (−15, 0, 0) |
| ShinL/R | (70, 0, 0) | (80, 0, 0) |

Los **fondos en anillas** (anillas a 1,25 m) usan las mismas posturas, con `Shoulder` Z ±12.

**Barra baja (1,1 m): remo australiano.** Talones en el suelo y el cuerpo recto. `rootHeight` A **0,30** → B **0,61**.

| Hueso | A · brazos extendidos | B · pecho a la barra |
|---|---|---|
| Hips | (−72, 0, 0) | (−50, 0, 0) |
| ShoulderL/R | (−108, 0, ±10): brazo vertical en el mundo | (−40, 0, ±35) |
| ForearmL/R | (0, 0, 0) | (−120, 0, 0) |
| ThighL/R | (0, 0, ±5) | igual |
| ShinL/R | (0, 0, 0) | igual |

**Barra de 1,8 m: front lever agrupado (tuck).** `rootHeight` A **0,62** → B **1,18**.

| Hueso | A · colgado y agrupado | B · tuck front lever (tronco horizontal, boca arriba) |
|---|---|---|
| Hips | (0, 0, 0) | (−90, 0, 0) |
| ShoulderL/R | (−175, 0, ±12) | (−90, 0, ±12): brazos rectos hacia la barra |
| ForearmL/R | (0, 0, 0) | (0, 0, 0) |
| ThighL/R | (−130, 0, 0) | (−120, 0, 0) |
| ShinL/R | (130, 0, 0) | (130, 0, 0) |

**Espalderas: elevación de piernas colgado.** Espalda a las barras y manos en el peldaño de 2,3 m. `rootHeight`
**1,13**.

| Hueso | A · colgado | B · piernas a 90° |
|---|---|---|
| Hips | (0, 0, 0) | (−5, 0, 0) |
| ShoulderL/R | (−175, 0, ±20) | igual |
| ThighL/R | (0, 0, 0) | (−90, 0, 0) |
| ShinL/R | (0, 0, 0) | (0, 0, 0) |

**Paralelas bajas (0,33 m): L-sit.** Apoyado en las paralelas bajas. `rootHeight` **0,45**.

| Hueso | A · L-sit agrupado | B · L-sit completo |
|---|---|---|
| Hips | (0, 0, 0) | (−5, 0, 0) |
| Back / Chest | (10, 0, 0) / 0 | (5, 0, 0) / 0 |
| ShoulderL/R | (5, 0, ±8): brazos rectos y apoyados | igual |
| ForearmL/R | (0, 0, 0) | igual |
| ThighL/R | (−110, 0, 0) | (−90, 0, 0) |
| ShinL/R | (120, 0, 0) | (0, 0, 0) |

**Poste de bandera humana.** El poste queda a la izquierda de la persona: mano izquierda arriba y derecha abajo.
`rootHeight` A **1,10** → B **1,30**.

| Hueso | A · bandera agrupada (prep.) | B · bandera completa (cuerpo horizontal) |
|---|---|---|
| Hips | (0, 0, −60) | (0, 0, −90): giro lateral; **hay que comprobar el signo en escena** |
| ShoulderLeft (arriba) | (0, 0, 160) | (0, 0, 165) |
| ShoulderRight (abajo) | (0, 0, −130) | (0, 0, −135) |
| ForearmL/R | (−10, 0, 0) / (0, 0, 0) | (0, 0, 0) |
| ThighL/R | (−100, 0, ±5) | (0, 0, ±12): piernas juntas o abiertas |
| ShinL/R | (110, 0, 0) | (0, 0, 0) |

### 4.5 Piso 2: parkour

**Cajón de salto: kong vault.** El recorrido va de +Z a −Z. La raíz avanza 2,2 m sobre el cajón y la secuencia es
A → B → aterrizaje con la postura A de la sentadilla de la jaula. `rootHeight` A **0,93** → B **1,65**.

| Hueso | A · batida (correr) | B · sobre el cajón, manos apoyadas |
|---|---|---|
| Hips | (15, 0, 0) | (80, 0, 0): cuerpo casi horizontal |
| Neck / Head | 0 / 0 | (−40, 0, 0) / 0: mira el aterrizaje |
| ShoulderL/R | (−90, 0, ±15): brazos al cajón | (−80, 0, ±15): verticales en el mundo |
| ForearmL/R | (−10, 0, 0) | (0, 0, 0) |
| ThighL / ShinL | (−50, 0, 0) / (70, 0, 0) | (−110, 0, 0) / (120, 0, 0): rodillas al pecho |
| ThighR / ShinR | (25, 0, 0) / (20, 0, 0) | (−110, 0, 0) / (120, 0, 0) |

**Muro alto (2,4 m): subida (climb-up).** `rootHeight` A **1,23** → B **2,05**.

| Hueso | A · colgado del borde, pies en el muro | B · empuje sobre el borde (mantle) |
|---|---|---|
| Hips | (−10, 0, 0) | (40, 0, 0) |
| ShoulderL/R | (−170, 0, ±20) | (−40, 0, ±15): manos bajo los hombros |
| ForearmL/R | (−10, 0, 0) | (0, 0, 0) |
| ThighL / ShinL | (−60, 0, 0) / (80, 0, 0) | (−90, 0, 0) / (120, 0, 0): rodilla arriba |
| ThighR / ShinR | (−60, 0, 0) / (80, 0, 0) | (10, 0, 0) / (20, 0, 0) |

**Barandilla (0,5 m): equilibrio.** La B es el aterrizaje de precisión. `rootHeight` A **1,46** → B **1,00**.

| Hueso | A · andando con los brazos abiertos | B · en cuclillas sobre la barandilla |
|---|---|---|
| Hips | (0, 0, 0) | (40, 0, 0) |
| ShoulderL/R | (0, 0, ±80) | (−70, 0, ±10) |
| ForearmL/R | (−10, 0, 0) | (−10, 0, 0) |
| ThighL / ShinL | (−20, 0, 0) / (10, 0, 0) | (−130, 0, ±10) / (125, 0, 0) |
| ThighR / ShinR | (10, 0, 0) / (15, 0, 0) | igual que la izquierda |

**Bloques de precisión: salto de precisión.** Entre bloques de 0,5 y 0,65 m; la raíz salta 1,0–1,6 m.
`rootHeight` A **1,11** → B **1,16**.

| Hueso | A · batida (brazos atrás) | B · aterrizaje (brazos delante) |
|---|---|---|
| Hips | (45, 0, 0) | (35, 0, 0) |
| ShoulderL/R | (60, 0, ±10) | (−80, 0, ±10) |
| ForearmL/R | (−10, 0, 0) | (−10, 0, 0) |
| ThighL/R | (−110, 0, ±8) | (−115, 0, ±8) |
| ShinL/R | (100, 0, 0) | (110, 0, 0) |

**Barra de lâché (2,0 m): balanceo.** El balanceo va en X, así que la persona mira hacia ±X. `rootHeight` **0,82**.

| Hueso | A · atrás | B · adelante |
|---|---|---|
| Hips | (20, 0, 0) | (−35, 0, 0) |
| Back / Chest | (−10, 0, 0) / 0 | (10, 0, 0) / 0 |
| ShoulderL/R | (−175, 0, ±12) | igual |
| ThighL/R | (−30, 0, 0) | (−70, 0, 0) |
| ShinL/R | (90, 0, 0) | (30, 0, 0) |

### 4.6 Piso 2: crossfit

**Rig: dominada con kipping.** Barra a 2,3–2,6 m. `rootHeight` **1,23**. El ciclo es A ↔ B y después la B de las
dominadas (§4.4).

| Hueso | A · arco | B · hueco (hollow) |
|---|---|---|
| Hips | (15, 0, 0) | (−20, 0, 0) |
| Back / Chest | (−15, 0, 0) / 0 | (10, 0, 0) / 0 |
| ShoulderL/R | (−175, 0, ±15) | (−160, 0, ±15) |
| ThighL/R | (10, 0, 0) | (−20, 0, 0) |
| ShinL/R | (10, 0, 0) | (0, 0, 0) |

**Plataforma: cargada (clean) con recepción en sentadilla frontal.** La A es la misma que la A del peso muerto (§4.1).
`rootHeight` B **0,50**.

| Hueso | B · recepción (barra en los hombros) |
|---|---|
| Hips | (25, 0, 0) |
| Neck / Head | (−20, 0, 0) / 0 |
| ShoulderL/R | (−90, 0, ±25): codos altos |
| ForearmL/R | (−150, 0, 0) |
| ThighL/R | (−120, 0, ±15) |
| ShinL/R | (120, 0, 0) |

**Assault bike.** Sillín a 0,85 m. `rootHeight` **0,98**. Los brazos empujan y tiran al contrario que las piernas.

| Hueso | A | B |
|---|---|---|
| Hips | (15, 0, 0) | igual |
| ShoulderLeft / ForearmLeft | (−30, 0, 10) / (−90, 0, 0) | (−70, 0, 10) / (−20, 0, 0) |
| ShoulderRight / ForearmRight | (−70, 0, −10) / (−20, 0, 0) | (−30, 0, −10) / (−90, 0, 0) |
| ThighL / ShinL | (−95, 0, 0) / (105, 0, 0) | (−45, 0, 0) / (30, 0, 0) |
| ThighR / ShinR | (−45, 0, 0) / (30, 0, 0) | (−95, 0, 0) / (105, 0, 0) |

**SkiErg.** De pie y mirando a la máquina. `rootHeight` A **0,95** → B **0,80**.

| Hueso | A · brazos arriba | B · tirón abajo (bisagra de cadera) |
|---|---|---|
| Hips | (0, 0, 0) | (45, 0, 0) |
| ShoulderL/R | (−160, 0, ±15) | (−35, 0, ±10): manos junto a los muslos |
| ForearmL/R | (−10, 0, 0) | (−5, 0, 0) |
| ThighL/R | (0, 0, ±5) | (−65, 0, ±5) |
| ShinL/R | (0, 0, 0) | (35, 0, 0) |

**Wall ball** (diana de 3,05 m, a 0,6 m de la cara del rig). `rootHeight` A **0,49** → B **0,95**.

| Hueso | A · sentadilla, balón en el pecho | B · lanzamiento |
|---|---|---|
| Hips | (30, 0, 0) | (0, 0, 0) |
| Neck / Head | (−20, 0, 0) / 0 | (−15, 0, 0) / (−10, 0, 0): mira la diana |
| ShoulderL/R | (−40, 0, ±35) | (−165, 0, ±15) |
| ForearmL/R | (−130, 0, 0) | (−10, 0, 0) |
| ThighL/R | (−125, 0, ±15) | (0, 0, ±8) |
| ShinL/R | (120, 0, 0) | (0, 0, 0) |

**Cajón pliométrico: salto al cajón** (61 cm). `rootHeight` A **0,60** → B **1,24**. La raíz avanza 0,5 m.

| Hueso | A · carga (brazos atrás) | B · aterrizaje en el cajón |
|---|---|---|
| Hips | (45, 0, 0) | (30, 0, 0) |
| ShoulderL/R | (50, 0, ±10) | (−80, 0, ±10) |
| ThighL/R | (−110, 0, ±10) | (−110, 0, ±10) |
| ShinL/R | (100, 0, 0) | (105, 0, 0) |

**Kettlebell: swing.** `rootHeight` A **0,78** → B **0,95**.

| Hueso | A · bisagra (pesa entre las piernas) | B · arriba (brazos al frente) |
|---|---|---|
| Hips | (60, 0, 0) | (0, 0, 0) |
| Neck / Head | (−30, 0, 0) / 0 | 0 / 0 |
| ShoulderL/R | (−40, 0, ±5) | (−90, 0, ±5) |
| ForearmL/R | (0, 0, 0) | (0, 0, 0) |
| ThighL/R | (−80, 0, ±10) | (0, 0, ±10) |
| ShinL/R | (30, 0, 0) | (0, 0, 0) |

**Trineo: empuje** (postes a 1,0 m). `rootHeight` **0,80**. La raíz avanza con el trineo.

| Hueso | A · zancada izquierda | B · zancada derecha |
|---|---|---|
| Hips | (55, 0, 0) | igual |
| ShoulderL/R | (−115, 0, ±10): manos en los postes | igual |
| ForearmL/R | (−20, 0, 0) | igual |
| ThighL / ShinL | (−110, 0, 0) / (100, 0, 0) | (−30, 0, 0) / (20, 0, 0) |
| ThighR / ShinR | (−30, 0, 0) / (20, 0, 0) | (−110, 0, 0) / (100, 0, 0) |

**Cuerda de trepa** (4,57 m, con los pies cruzados). La raíz sube 0,6 m por ciclo; en la primera A la pelvis está a 1,3.

| Hueso | A · manos arriba, piernas recogidas | B · de pie sobre el nudo de pies |
|---|---|---|
| Hips | (0, 0, 0) | (0, 0, 0) |
| ShoulderL / ShoulderR | (−170, 0, 5) / (−150, 0, −5) | (−40, 0, 15) / (−60, 0, −15) |
| ForearmL/R | (−10, 0, 0) | (−130, 0, 0) |
| ThighL/R | (−100, 0, 0) | (−20, 0, 0) |
| ShinL/R | (100, 0, 0) | (20, 0, 0) |

**GHD: abdominal (sit-up).** Cadera en la almohadilla a ~1,0 m y pies enganchados. `rootHeight` **1,14**.

| Hueso | A · extendido por detrás | B · incorporado |
|---|---|---|
| Hips | (−110, 0, 0) | (−10, 0, 0) |
| Back / Chest | (−10, 0, 0) / 0 | (10, 0, 0) / 0 |
| ShoulderL/R | (−170, 0, ±15): manos hacia el suelo | (−90, 0, ±10): hacia los pies |
| ThighL/R | (20, 0, 0): muslo horizontal en el mundo | (−80, 0, 0) |
| ShinL/R | (10, 0, 0) | (10, 0, 0) |

Los **remos** del crossfit usan las posturas del remo de §4.2.

---

## Fuentes

- Separaciones del gimnasio:
  - [Achievable (CSCS): facility design, layout and organization](https://app.achievable.me/study/cscs/learn/facility-design-layout-and-organization)
  - [PT Pioneer: NSCA-CSCS chapter 23](https://www.ptpioneer.com/personal-training/certifications/nsca-cscs/cscs-chapter-23/)
  - [PT Pioneer: NSCA-CPT chapter 24](https://www.ptpioneer.com/nsca-cpt-chapter-24/)
  - [Open Exam Prep: NSCA-CPT exercise environment and equipment](https://open-exam-prep.com/study-guides/nsca-cpt/facility-safety/environment-equipment)
  - [Open Exam Prep: TSAC-F facility design and traffic flow](https://open-exam-prep.com/study-guides/nsca-tsac-f/facility-admin-policies-ethics/facility-design-traffic-flow)
  - [Skelcore: superficie por usuario en clases funcionales](https://www.skelcore.com/blogs/news/how-much-space-should-be-allocated-per-user-in-a-group-functional-training-class-to-maximize-safety-flow-and-results)
  - [Skelcore: superficie por miembro en peso libre](https://www.skelcore.com/guides/whats-recommended-square-footage-per-member)
  - [Glofox: cómo calcular el espacio de un gimnasio](https://www.glofox.com/blog/how-to-calculate-gym-space-requirements)
  - [RitFit: high school weight room layout](https://www.ritfitsports.com/blogs/article/high-school-weight-room-layout)
- Cintas y cardio:
  - [Bodycraft T400: espacios libres ASTM F2115 (PDF)](https://www.bodycraft.com/pdfs/Footprints/t400-dims.pdf)
  - [Skelcore: espacio detrás y a los lados de una cinta](https://www.skelcore.com/guides/how-much-space-should-leave-behind-each-side)
  - [ASTM F2115-25](https://store.astm.org/f2115-25.html)
  - [Life Fitness E1 Elliptical](https://lifefitness.com/en-hk/catalog/cardio/ellipticals/e1-elliptical-cross-trainer)
  - [Life Fitness Club Series+ Elliptical](https://www.gymequipment.co.uk/life-fitness-club-series-elliptical-cross-trainer)
  - [Life Fitness Activate Cross Trainer](https://archipro.co.nz/product/activate-cross-trainer-life-fitness)
- Peso libre:
  - [Life Fitness Olympic Flat Bench](https://www.lifefitness.com/en-hk/catalog/strength-training/benches/olympic-flat-bench)
  - [Body-Solid FID46 Bench](https://www.biggerfasterstronger.com/products/bodysolid-fid46)
  - [Smith machines comparison (PDF)](https://cdn.shopify.com/s/files/1/0862/1694/files/2022_Smith_machines_comparisons_GB.pdf)
  - [TrustySpotter: power rack size](https://trustyspotter.com/blog/power-rack-size)
  - [Power rack sizing guide (GxMMat)](https://www.gxmmat.us/blogs/daily-news/the-power-rack-sizing-guide-every-home-gym-owner-needs-to-read)
- Calistenia:
  - [KOMPAN FSW201 (ficha)](https://assets.kompan.com/$web/prod/e1792b66-a178-43c4-81f7-dd59794d59f8/FSW201_US_ASTM_en-US_Imperial_comma_point.pdf)
  - [Parko Play CL005 (PDF)](https://parkoplay.gr/files/specialconstructions/CL005_DATASHEET.pdf)
  - [Redlynch pull up 2](https://www.redlynchleisure.co.uk/product/pull-up-2/)
  - [Skor: calisthenics parallel bars](https://skor.nl/en/products/calesthenics-parallel-bars)
  - [Skor: monkey bars](https://skor.nl/en/products/monkey-bars)
  - [Skor: multi-power bars](https://skor.nl/en/products/multipowerbars)
  - [Proludic J5242 (bandera humana)](https://www.externalworksindex.co.uk/entry/172088/Proludic-Play-and-Sports-Areas/J5242-Combination-outdoor-street-workout-and-calisthenics-station/)
  - [Parque de calistenia en Brisbane (calisthenics-parks.com)](https://calisthenics-parks.com/spots/3568)
  - [Artimex: wall bars 279 (240 × 90)](https://www.archiexpo.com/prod/artimex-sport/product-64042-1315919.html)
  - [Fitwood AARNI wall bars](https://sharkfitness.ch/en/fitness-equipment/small-training-devices/wall-bars/fitwood-wall-bars-aarni-birch.html)
  - [Continental Sports: anillas regulables](https://www.continentalsports.co.uk/index.php/pdf/?id=4190)
  - [Ficha de anillas de madera (PDF)](https://cdn.shopify.com/s/files/1/2716/9828/files/Gymnastics_Rings_Wooden_Spec_Sheet_d65a664b-2a32-4e19-8456-f4e1e76c75b8.pdf)
  - [Ericflag: parallettes](https://en.ericflag.com/products/parallettes)
- Suelos y alturas de caída:
  - [CATAS: EN 1177 (PDF)](https://catas.com/uploads/media/catas-en-1177-eng.pdf?v2.6)
  - [Rubberco: BS EN 1177 y losetas](https://rubberco.co.uk/blogs/news/playground-safety-surfacing-requirements-uk-bs-en-1177-critical-fall-height-and-rubber-tile-spec-2026)
  - [EN 1177:2018+A1:2023 (iTeh)](https://standards.iteh.ai/catalog/standards/cen/8a9d50c2-fc9a-482a-9481-c0c73d573df1/en-1177-2018a1-2023)
- Parkour:
  - [Parkour Park Design Handbook 2021](https://parkourvisions.org/s/Parkour-Park-Design-Handbook-2021.pdf)
  - [Parkour Visions: principles of space design](https://parkourvisions.org/news/principles-of-space-design-for-parkour-class-training)
  - [EN 16899 (iTeh)](https://standards.iteh.ai/catalog/standards/sist/0d7fefa0-4ea3-4a73-96d4-1e2ab5b848fa/sist-en-16899-2017)
  - [UTS: AS EN 16899 (presentación)](https://opus.lib.uts.edu.au/bitstream/10453/149294/2/_2021%2005%2027%20Eager%20CS-101%20presentation%20v2.pdf)
  - [Lappset Dash Parkour](https://lappset.com/en-GB/article/dash-parkour-equipment-material-specification)
  - [Lappset: ficha 220546](https://webapi.lappset.com/server/api/v1/en/pdf/220546)
  - [American Parkour: Flagship Vault 60](https://store.americanparkour.com/products/vault-box-60-x-33-x-32)
  - [Promat 3 Section Vault Box](https://www.athleticsdirect.co.uk/other-sports/schools-sportshall/promat-3-section-vault-box)
- Crossfit:
  - [KingsBox: diseño de un box](https://kingsbox.com/blog/en/?p=248047)
  - [Wodify: equipamiento de un afiliado](https://www.wodify.com/blog/crossfit-affiliate-gym-equipment)
  - [Missouri Metal Buildings: gimnasios](https://mmb-website.pages.dev/buildings/gym-fitness-buildings)
  - [Rogue USMC Pull-up Rig (Garage Gym Reviews)](https://www.garagegymreviews.com/equipment/rogue-usmc-pull-up-rig)
  - [Rogue MIL Pull-up Station](https://www.roguefitness.com/ca/rogue-mil-pull-up-station)
  - [Kibi: freestanding rig](https://shop.kibisports.com/products/freestanding-crossfit-rig-24-ft)
  - [CrossFit Open 26.1](https://games.crossfit.com/workouts/open?scaled=2)
  - [CrossFit Games 2021: planos y lista de equipo (PDF)](https://s3.amazonaws.com/crossfitpubliccontent/CFG21_QF_IND_Floor_Plans_and_Equip_List_whneby76V4hysge.pdf)
  - [CrossFit Games 2022: Quarterfinals (cuerda de 15 ft)](https://games.crossfit.com/workouts/onlinequalifiers/2022/3?division=1)
  - [Concept2 SkiErg (Northern Fitness)](https://www.northernfitness.ca/products/concept2-skierg-ski-machine)
  - [Concept2 Middle East (PDF)](https://cdn.shopify.com/s/files/1/0441/6798/7367/files/Concept2-Middle-East.pdf)
  - [Assault AirBike (Spartan Fitness)](https://www.spartanfitness.ca/products/diamondback-assault-dual-action-cycle)
  - [Assault Air Bike (BLK BOX)](https://www.blkboxfitness.com/products/assault-air-bike)

**Datos no verificados** (hay que confirmarlos si se necesita precisión):
- la huella de la escaladora y de las bicis vertical y reclinada (aproximada);
- la longitud de la pista de trineo;
- la altura de la barra de front lever (1,8 m es una propuesta de diseño, no una norma);
- la cifra de 1,6 m sin superficie amortiguadora, que viene de la versión australiana de EN 16899;
- los grosores de caucho, porque mandan los certificados EN 1177.
