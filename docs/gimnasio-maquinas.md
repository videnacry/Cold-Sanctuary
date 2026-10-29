# Máquinas del gimnasio: cómo son de verdad y cómo usarlas en el entrenamiento

Investigación del 2026-10-27. Objetivo: que las 30 máquinas del gimnasio (10 tipos × 3) se parezcan a las reales y que
los personajes las **usen** durante el entrenamiento diario (14:00–16:00).

## 0. Estado actual (resumen)

- **Construcción**: `FurnishGym` (`Assets/Editor/SampleSceneBuilder.Training.cs:18`) coloca 30 máquinas en una
  rejilla de 5 × 3 por piso (paso 2,6 m en X y 3,2 m en Z, `:36-37`). Las primeras 15 (`k < 15`), es decir **los tipos
  0–4** (pecho, hombros, jalón, remo y prensa), van **abajo**. Los **tipos 5–9** (extensión, curl femoral, hip thrust,
  gemelos y brazos) van **arriba**, en la entreplanta (`:29-31`, `:38-39`). El área está en (26, 0, 6) y mide 22 × 16 m
  (`SampleSceneBuilder.cs:3459-3460`).
- **Todas las máquinas usan el mismo cuerpo** (`GymMachine`, `Training.cs:71-102`):
  - una base de 1,1 × 0,1 × 1,5 m (`:75`);
  - una torre de 0,36 × 2,1 × 0,2 m detrás, en z = +0,62 (`:76`), con 10 placas de 0,3 × 0,05 × 0,16 (`:77`);
  - un brazo de polea a 2,1 m (`:78`);
  - un **cable vertical en (0, 1,3–2,1, 0)** que **atraviesa la cabeza** de quien se siente (`:79`);
  - un asiento de 0,45 × 0,1 × 0,45 con su parte superior a 0,55 m y un respaldo vertical de 0,8 m en z = +0,27
    (`:84-85`), o un banco de 0,45 × 0,12 × 1,3 para los tipos 6 y 7 (`:80-81`);
  - una única pieza de color que distingue cada tipo (`:87-99`).
- **Fallos concretos**:
  - El soporte de los pies de la prensa usa `-20f` como si fuera una inclinación, pero `KitchenProps.C` solo admite el
    giro sobre la vertical (`Assets/Scripts/Kitchen/KitchenProps.cs:52-60`). Por eso la plataforma queda **torcida en
    horizontal**, no inclinada (`Training.cs:94`).
  - El jalón y el remo tienen la torre **detrás** y un respaldo. En los reales la persona **mira a la torre** y no hay
    respaldo.
  - Hay dos `<summary>` seguidos (`:14-17`) que mencionan una jaula de sentadillas, cintas, espejos y cuadros de gorilas
    que no se construyen. `docs/jornada-y-rotacion.md:91` también promete «jaulas, bancos, mancuernas, cintas, remo y
    saco».
  - El comentario de `:23` dice que la entreplanta está a 3 m, pero `MezzanineHeight` vale 4 m
    (`SampleSceneBuilder.Levels.cs:14`).
- **Nadie usa las máquinas.**
  - El reparto del entrenamiento rota entre las áreas abiertas, de modo que a la persona *i* el día *d* le toca el área
    *(i + d) mod n* (`WorkRoster.cs:127-129`).
  - `WorkdayDirector.Where` devuelve «Gimnasio» de 14 a 16 h (`WorkdayDirector.cs:290`).
  - `Drive` solo tiene un caso especial para la clase de yoga (`YogaClass`, `:339`, `:360-377`). Para el gimnasio
    manda a cada persona a `AreaPoint` (`:412-426`), un punto de una rejilla calculado a partir del hash del nombre,
    donde se queda **de pie sin hacer nada**.
  - No existe ninguna postura de ejercicio. Las posturas cotidianas están en `DailyPoses`
    (`Assets/Scripts/Body/DailyLife.cs:10-52`) y no incluyen ninguna.
  - Las únicas misiones del gimnasio son de trabajo: `gym_limpieza` y `gym_mantener` (`AreaWork.cs:58-59`, pasos en
    `VirtualMissions.cs:67,77`).
- **Tests**: `SanctuaryLifeTest.cs:377-381` cuenta las máquinas con nombre `Maquina_*` (deben ser 30 o más) y
  comprueba su pasillo. `FurnitureSpacing.cs:18` exige 0,9 m entre máquinas.

## 1. Convenciones de este documento

**Ejes de cada máquina.** El origen está en el centro de la base, a ras de suelo, con X a la derecha, Y hacia arriba y
Z hacia delante (−Z es «el frente», donde hoy están los agarres). Las medidas están en metros del mundo.

**Puesto.** Es el punto donde va la **raíz del personaje**: los pies si está de pie, o la vertical de la pelvis si está
sentado o tumbado. Para cada máquina se indica hacia dónde mira la persona. Se propone que cada máquina tenga un hijo
vacío llamado `Puesto` con esa posición y ese giro.

**Ángulos.** Siguen el rig de cubos (`docs/yoga-asanas-spec.md`, «Convención del rig»; `CubeHumanoid.cs:76-110`). Son
Euler (X, Y, Z) en grados, locales y relativos al hueso padre.

| Hueso | X | Z |
|---|---|---|
| `Hips` | Gira el **cuerpo entero**. X− lo inclina hacia atrás (−90 = tumbado boca arriba, como `DailyPoses.Lie`). X+ lo inclina hacia delante (+90 = boca abajo). | — |
| `Back` y `Chest` (columna) | X+ flexiona hacia delante; X− extiende. | — |
| `Neck` y `Head` | X+ baja la barbilla; X− levanta la mirada. | — |
| `ShoulderL/R` (brazo entero) | X− lo sube **por delante** (−90 = horizontal al frente, −175 = vertical arriba); X+ lo lleva atrás. | `ShoulderLeft` Z+ y `ShoulderRight` Z− lo abren hacia fuera. |
| `ForearmL/R` | X− flexiona el codo. | — |
| `ThighL/R` | X− lleva el muslo adelante (−90 = sentado, muslo horizontal); X+ atrás. | Igual que el hombro: abre la pierna. |
| `ShinL/R` | X+ dobla la rodilla (+90 = espinilla vertical si el muslo está horizontal). | — |
| `FootL/R` | X+ baja la punta (flexión plantar); X− la sube (dorsiflexión). Solo se usa en Gemelos. | — |

Cuando se escribe `±` en Z, el primer signo es el de la izquierda y el segundo el de la derecha: (−80, 0, ±12) quiere
decir izquierda (−80, 0, 12) y derecha (−80, 0, −12). Los huesos que no aparecen en una postura valen 0.

**Altura de la pelvis (`rootHeight`).** Se expresa en **unidades del cuerpo nativo**: el cuerpo se construye con 1,9 de
alto y se escala por `s = estatura / 1,9`. De pie la pelvis está a 0,95.

- Sentado: **`rootHeight ≈ altura del asiento / s + 0,11`**. La articulación de la cadera está 0,05 por debajo de la
  pelvis y el muslo mide 0,14 de grueso.
- Tumbado: `rootHeight ≈ (altura del banco + 0,11 × s) / s`.
- Para una persona de **1,85 m**, s = 0,974. Ese es el usuario de referencia de todas las cifras de abajo.

Medidas del cuerpo nativo (`CubeHumanoid.cs`): muslo 0,42, espinilla 0,40, brazo 0,30, antebrazo 0,27 y mano 0,10; el
hombro está a pelvis + 0,56. Las máquinas reales **se ajustan** (asiento y almohadillas), así que la altura del asiento
debe escalar con `s` para personas más bajas. Por ejemplo, para 1,55 m el asiento del press baja unos 9 cm.

Los ángulos son **aproximados**: el rig hace cinemática directa sin IK. Hay que revisarlos a ojo en escena y, si una
mano no llega al agarre, retocar el hombro antes que el codo.

**Espacio libre** (todas las máquinas):

- Entre máquinas de placas, 0,6 m como mínimo y **0,9 m como lo ideal**. ACSM y la guía de CSCS dan unos 0,9 m (36″);
  Livestrong da un mínimo de 24″ y un óptimo de 36″. Es lo que ya usa `FurnitureSpacing`.
- Los fabricantes indican además un «live area», el área de uso, mayor que la huella. Por ejemplo, el Hammer Strength
  Select Chest Press ocupa 104 × 145 cm y su área de uso es de 210 × 145 cm.
- La rejilla actual, de **2,6 × 3,2 m por máquina**, admite todas las huellas propuestas abajo, la mayor de las cuales es
  la prensa (1,5 × 2,2 m), dejando ≥ 1 m de paso.

---

## 2. Press de pecho (tipo 0)

### Cómo es de verdad
- **Huella**: 1,05–1,3 m (fondo) × 0,95–1,45 m (ancho) × 1,5–1,65 m (alto).
  - Decathlon Future Elite: 1290 × 953 × 1550 mm.
  - Life Fitness Insignia: 110 × 140 × 148 cm.
  - Hammer Strength Select: 104 × 145 × 163 cm.
- **Pila de placas**: 100–150 kg, en una torre baja (≈1,5 m) detrás o a un lado del asiento.
- **Piezas**:
  - bastidor en U;
  - asiento regulable en altura (≈0,42–0,55 m);
  - respaldo de unos 0,75 m reclinado unos 10°;
  - **dos brazos de palanca** que pivotan arriba, detrás de los hombros, y bajan hasta los agarres. Hay agarres dobles:
    neutros (verticales) y horizontales;
  - pedal de asistencia o reposapiés delante;
  - cable y poleas dentro de una carcasa.
- **Ajuste**: los agarres quedan a la **altura de media pecho**, a la altura de los pezones, y no más atrás del pecho
  (ACE). Los codos van a unos 45° del cuerpo (NASM). Se empuja sin bloquear los codos.

### Cómo está ahora
- `Training.cs:89-90`: dos agarres sueltos de 0,06 × 0,06 × 0,35 en (±0,35, 1,05, −0,25), sin brazos de palanca.
- Torre de 2,1 m (`:76`), el doble de alta que la real.
- El cable atraviesa la cabeza (`:79`).

### Qué cambiar
1. Bajar la torre a **1,5 m** con carcasa: 0,40 × 1,5 × 0,25 en z = +0,55. Poner unas 13 placas.
2. Quitar el cable y el brazo de polea que cuelgan sobre el asiento. El cable va dentro de la carcasa.
3. Asiento con la parte superior a **0,48 m**, en z = +0,05.
4. Respaldo de 0,42 × 0,75 × 0,08, **reclinado 10°**, de 0,55 a 1,30 m, en z = +0,28.
5. Dos brazos de palanca (0,06 × 0,06 × 0,7) desde un pivote en (±0,35, 1,45, +0,30) hasta los agarres neutros
   (0,04 × 0,18 × 0,04).
   - Al empezar, los agarres están en (±0,28, 1,00, −0,25).
   - Al final del empuje, en (±0,22, 1,02, −0,62).
6. Reposapiés de 0,5 × 0,05 × 0,1 en (0, 0,10, −0,30).
7. Hace falta una variante de `KitchenProps.C` con **Euler completo** para inclinar las piezas. Hoy solo gira en Y.

### Postura de uso
**Puesto**: (0, —, +0,05), mirando hacia −Z. `rootHeight` = **0,60**, es decir, una pelvis a 0,58 m con 1,85 de
estatura.

| Hueso | A · agarres junto al pecho | B · brazos extendidos |
|---|---|---|
| Hips | (−10, 0, 0) | (−10, 0, 0) |
| Back / Chest | 0 / 0 | 0 / (3, 0, 0) |
| Neck / Head | 0 / 0 | 0 / 0 |
| ShoulderL/R | (15, 0, ±45) | (−80, 0, ±12) |
| ForearmL/R | (−95, 0, 0) | (−10, 0, 0) |
| ThighL/R | (−80, 0, ±8) | igual |
| ShinL/R | (90, 0, 0): pies en el reposapiés | igual |

Una repetición dura unos 3 s: de A a B en 1 s y de B a A en 2 s.

## 3. Press de hombros (tipo 1)

### Cómo es de verdad
- **Huella**: 1,45–1,7 × 1,4–1,45 × 1,5–1,65 m.
  - Precor Vitality: 147 × 145 × 155 cm, pila de 91 kg.
  - Hammer Strength Select: 152 × 142 × 163 cm, pila de 95 kg.
  - Spirit: 1707 × 1459 × 1500 mm, pila de 110 kg.
- **Piezas**: asiento regulable, respaldo casi vertical (5–10°) y alto (hasta ≈1,4 m) para apoyar la cabeza, brazos
  de palanca que pivotan detrás y arriba, agarres neutros y horizontales, y torre baja detrás.
- **Movimiento**: los agarres empiezan **justo por encima de los hombros** y terminan con los brazos casi extendidos
  sobre la cabeza.

### Cómo está ahora
- `Training.cs:89-90`: los mismos agarres sueltos que el press de pecho, pero a 1,4 m.
- Tiene el mismo bastidor genérico, la torre de 2,1 m y el cable que atraviesa la cabeza (`:75-79`).

### Qué cambiar
1. Respaldo de 0,42 × 0,95, de 0,55 a 1,50 m, reclinado 8°.
2. Asiento a **0,48 m**.
3. Torre de **1,55 m** detrás.
4. Brazos de palanca desde (±0,30, 1,55, +0,35):
   - agarres neutros al empezar en (±0,25, 1,18, −0,10);
   - al final en (±0,18, 1,75, −0,05).
5. Sin cable visible.

### Postura de uso
**Puesto**: (0, —, +0,05), mirando hacia −Z. `rootHeight` = **0,60**.

| Hueso | A · manos a la altura de los hombros | B · brazos arriba |
|---|---|---|
| Hips | (−8, 0, 0) | (−8, 0, 0) |
| Back / Chest | 0 / 0 | 0 / 0 |
| Neck / Head | 0 / 0 | 0 / (−5, 0, 0) |
| ShoulderL/R | (−60, 0, ±30) | (−165, 0, ±12) |
| ForearmL/R | (−120, 0, 0) | (−10, 0, 0) |
| ThighL/R | (−82, 0, ±8) | igual |
| ShinL/R | (90, 0, 0) | igual |

Se usa el agarre **neutro**: el codo se mueve en el plano X, que es el que el rig expresa bien. El agarre ancho
necesitaría girar el brazo en Y.

## 4. Jalón al pecho (tipo 2)

### Cómo es de verdad
- **Huella**: 1,35–1,6 × 0,8–1,4 × 1,85–2,3 m.
  - Life Fitness Axiom: 152 × 140 × 185 cm, con un área de uso de 213 × 170 cm.
  - Circuit Series: 160 × 109 × 180 cm.
  - York ST: 135 × 77 × 229 cm, con una barra de 1,2 m.
  - Good Life: asiento de 0,43–0,51 m y almohadillas de muslo regulables.
- **Piezas**:
  - **torre delante** de la persona, con la pila de placas;
  - viga superior con **polea alta** (≈2,1–2,3 m);
  - cable;
  - **barra ancha de ≈1,2 m** con las puntas dobladas hacia abajo;
  - asiento corto **sin respaldo**;
  - **almohadillas de muslo** (rodillos) que sujetan las piernas.
- **Técnica**:
  - inclinarse un poco hacia atrás, unos **10–20°** (NASM recomienda 20–30°; Skelcore, 10–15°);
  - bajar la barra a la **parte alta del pecho**;
  - abajo, el codo queda a unos 90° o más cerrado.

### Cómo está ahora
- `Training.cs:92`: una barra de 1,1 × 0,05 × 0,05 en (0, 1,9, −0,15), flotando y sin cable que la sujete.
- Lleva respaldo (`:85`) y la torre está **detrás** (`:76`): la persona daría la espalda a la pila.
- No tiene almohadillas de muslo.

### Qué cambiar
1. **Dar la vuelta a la máquina**: la torre de 0,45 × 2,2 × 0,30 va **delante** de la persona, en z = −0,55.
2. Viga superior de la torre hasta z = −0,10, con la polea en (0, 2,15, −0,12).
3. Quitar el respaldo.
4. Asiento de 0,40 × 0,35 a **0,48 m**, en z = +0,05.
5. Almohadillas de muslo: dos rodillos de 0,10 × 0,10 × 0,30 en (±0,17, 0,68, −0,18).
6. Barra de **1,2 m** con puntas a 20°, colgada de un cable visible:
   - en reposo y al empezar, a **1,80 m**, en z = −0,08;
   - abajo, a **1,08 m**, en z = −0,02.

### Postura de uso
**Puesto**: (0, —, +0,05), mirando hacia −Z, es decir, hacia la torre. `rootHeight` = **0,60**.

| Hueso | A · brazos arriba agarrando la barra | B · barra en la parte alta del pecho |
|---|---|---|
| Hips | (−12, 0, 0) | (−18, 0, 0) |
| Back / Chest | 0 / 0 | 0 / (−10, 0, 0): pecho arriba |
| Neck / Head | 0 / (−10, 0, 0): mira la barra | 0 / 0 |
| ShoulderL/R | (−170, 0, ±12) | (−15, 0, ±20) |
| ForearmL/R | (0, 0, 0) | (−135, 0, 0) |
| ThighL/R | (−78, 0, ±8): bajo los rodillos | (−72, 0, ±8) |
| ShinL/R | (95, 0, 0) | (95, 0, 0) |

Con el rig actual se anima mejor la variante de **agarre cerrado o neutro**. El agarre ancho pide abducir el brazo en
Z y girarlo en Y, o bien IK.

## 5. Remo sentado (tipo 3)

### Cómo es de verdad
- **Huella**: 1,4–1,65 × 0,85–1,2 × 1,65–1,9 m.
  - Precor Vitality: 142 × 122 × 178 cm, pila de 109 kg.
  - Muscle-D: 163 × 84 × 188 cm.
  - Reeplex: 1,38 × 1,18 × 1,83 m.
- **Piezas**: **almohadilla de pecho** regulable delante, asiento regulable, reposapiés, torre delante, y brazos o
  cable con **agarres dobles** (neutros y horizontales) a la altura del pecho.
- **Movimiento**: empieza con los brazos extendidos y las escápulas adelantadas, y termina con los codos atrás, más
  allá del tronco, y las escápulas juntas.

### Cómo está ahora
- `Training.cs:93`: un agarre de 0,45 × 0,06 × 0,06 a **0,85 m** (por debajo del pecho), en z = −0,55.
- Lleva respaldo y la torre está detrás. Le falta la almohadilla de pecho.

### Qué cambiar
1. La torre va **delante**, en z = −0,65 (de 0,45 × 1,8 × 0,30).
2. Almohadilla de pecho de 0,30 × 0,40 × 0,08, de 0,95 a 1,35 m, en z = −0,15.
3. Quitar el respaldo.
4. Asiento a **0,48 m**, en z = +0,10.
5. Reposapiés en (±0,15, 0,22, −0,42).
6. Brazos de remo desde un pivote en la torre (0, 1,6, −0,6) hasta los agarres:
   - con los brazos extendidos, en (±0,20, 1,05, −0,55);
   - al final, en (±0,22, 1,02, −0,05).

### Postura de uso
**Puesto**: (0, —, +0,10), mirando hacia −Z. `rootHeight` = **0,60**.

| Hueso | A · brazos extendidos | B · codos atrás |
|---|---|---|
| Hips | (0, 0, 0) | (0, 0, 0) |
| Back / Chest | (5, 0, 0) / 0: pecho en la almohadilla | 0 / (−5, 0, 0) |
| Neck / Head | 0 / 0 | 0 / 0 |
| ShoulderL/R | (−85, 0, ±8) | (35, 0, ±12) |
| ForearmL/R | (−5, 0, 0) | (−95, 0, 0) |
| ThighL/R | (−90, 0, ±10) | igual |
| ShinL/R | (80, 0, 0): pies adelantados en el reposapiés | igual |

Si se quiere marcar la escápula: `ScapulaL/R` adelantada en A y atrasada en B.

## 6. Prensa de piernas 45° (tipo 4)

### Cómo es de verdad
- **Huella**: 2,1–2,3 × 1,15–1,65 × 1,45–1,57 m.
  - Force USA: 212 × 115 × 152 cm, con una plataforma de 63 × 44 cm.
  - Club 45° de Primal: 231 × 164 × 146 cm, con una plataforma de 90 cm de ancho.
  - TF 1024D: 2200 × 1850 × 1570 mm.
- Es de **discos**: **no tiene torre ni pila**.
- **Piezas**:
  - bastidor largo en el suelo;
  - **dos raíles a 45°** que suben hacia delante;
  - **carro** con la **plataforma de pies perpendicular a los raíles** (60–90 cm de ancho);
  - **dos pares de cuernos** para los discos;
  - **topes de seguridad** con asas a los lados del asiento;
  - asiento bajo con **respaldo muy reclinado** (≈40–50° respecto a la vertical);
  - asas laterales.
- **Movimiento**:
  - arriba, las piernas quedan casi extendidas **sin bloquear las rodillas**;
  - abajo, la rodilla está a **unos 90°** (80–90°) y la cadera no se despega del respaldo.

### Cómo está ahora
- `Training.cs:94`: una placa de 0,7 × 0,6 × 0,06 a 0,9 m, en z = −0,6, con un `yaw` de −20°. Queda **girada en
  horizontal, no inclinada**.
- Tiene asiento recto, respaldo vertical, torre de pila y cable, como las demás. No se parece a una prensa.

### Qué cambiar
1. **Quitar la torre, las placas y el cable.**
2. Bastidor de 2,2 m en Z (de +0,9 a −1,3) y 1,4 m de ancho.
3. Raíles de 0,08 × 0,08 × 1,4 **inclinados 45°**, desde (±0,40, 0,35, −0,15) hasta (±0,40, 1,35, −1,15).
4. Carro de 0,8 × 0,08 × 0,5 sobre los raíles, con una plataforma de 0,75 × 0,55 **perpendicular a ellos** e inclinada
   45°, cuya normal mira hacia el asiento. Recorre unos 0,45 m a lo largo de los raíles:
   - abajo, su centro está en ≈(0, 0,75, −0,55);
   - arriba, en ≈(0, 1,07, −0,87).
5. Cuernos de discos (Ø0,05 × 0,30) a los lados del carro, con dos discos negros de Ø0,45 × 0,05 en cada uno.
6. Asiento de 0,45 × 0,45 a **0,40 m**, en z = +0,35, inclinado 15°.
7. Respaldo de 0,45 × 0,85, **reclinado 50°**, hasta z = +0,85.
8. Asas laterales en (±0,32, 0,45, +0,25).
9. Necesita la inclinación en X (ver «Qué cambiar» del press de pecho).

### Postura de uso
**Puesto**: (0, —, +0,35), mirando hacia −Z, es decir, hacia la plataforma. `rootHeight` = **0,52**.

| Hueso | A · abajo (rodilla 90°) | B · arriba (piernas casi rectas) |
|---|---|---|
| Hips | (−50, 0, 0): tumbado en el respaldo | (−50, 0, 0) |
| Back / Chest | 0 / 0 | 0 / 0 |
| Neck / Head | (15, 0, 0) / (10, 0, 0): mira la plataforma | igual |
| ShoulderL/R | (−20, 0, ±20) | igual |
| ForearmL/R | (−30, 0, 0): manos en las asas | igual |
| ThighL/R | (−115, 0, ±12) | (−80, 0, ±8) |
| ShinL/R | (95, 0, 0) | (10, 0, 0) |

## 7. Extensión de cuádriceps (tipo 5)

### Cómo es de verdad
- **Huella**: 1,2–1,45 × 1,0–1,3 × 1,5 m con pila. Body-Solid Pro Select (combinada con curl): 137 × 137 × 180 cm.
- **Piezas**:
  - asiento regulable en profundidad;
  - respaldo reclinado 10–15°;
  - **palanca con su pivote alineado con la rodilla**, que suele llevar un punto de color de referencia;
  - **rodillo para la espinilla** justo encima del tobillo (Body-Solid: 41 cm entre la parte superior del asiento y la
    inferior del rodillo);
  - asas a los lados del asiento;
  - pila a un lado.
- **Movimiento**: la rodilla pasa de **≈90° a casi 0°** (sin bloquearla de golpe) y el muslo no se mueve.

### Cómo está ahora
- `Training.cs:95`: un rodillo fijo de 0,6 × 0,12 × 0,12 en (0, 0,35, −0,35). Está a media espinilla, sin palanca y
  sin asas.
- Cuerpo genérico (`:75-85`).

### Qué cambiar
1. Asiento a **0,50 m**, de 0,45 × 0,42.
2. Respaldo reclinado 12°, en z = +0,35.
3. Pivote en (+0,32, 0,56, −0,25), marcado con un cubito de color.
4. Palanca de 0,05 × 0,05 × 0,40 desde el pivote hasta el rodillo, que gira en X:
   - rodillo (Ø0,12 × 0,40) **al empezar** en (0, 0,20, −0,32);
   - **al final** en (0, 0,53, −0,62).
5. Asas en (±0,28, 0,50, +0,05).
6. Pila de 1,4 m **a un lado**, en x = +0,55, en vez de detrás.

### Postura de uso
**Puesto**: (0, —, +0,15), mirando hacia −Z. `rootHeight` = **0,62**.

| Hueso | A · rodilla doblada | B · pierna extendida |
|---|---|---|
| Hips | (−12, 0, 0) | (−12, 0, 0) |
| Back / Chest | 0 / 0 | 0 / 0 |
| Neck / Head | 0 / 0 | 0 / 0 |
| ShoulderL/R | (−10, 0, ±18) | igual |
| ForearmL/R | (−20, 0, 0): agarra las asas | igual |
| ThighL/R | (−78, 0, ±6) | igual |
| ShinL/R | (90, 0, 0) | (5, 0, 0) |

## 8. Curl femoral tumbado (tipo 6)

### Cómo es de verdad
- **Huella**: 1,5–1,65 × 0,65–1,1 × 0,7–1,0 m sin pila, y hasta 2,0 m de alto con ella. Por ejemplo:
  - Body-Solid Powerline: 157 × 64 × 69 cm;
  - Power Lift Prone Leg Curl: 163 × 112 × 206 cm.
- **Piezas**:
  - banco de unos 0,65–0,70 m con una **joroba en la cadera**: deja la cadera flexionada 15–20° y la mantiene pegada;
  - palanca con el **pivote a la altura de la rodilla**, justo después del borde del banco;
  - **rodillo detrás de los tobillos**;
  - asideros bajo el extremo de la cabeza.
- **Movimiento**:
  - la rodilla pasa de casi 0° a **90–110°**: los talones se acercan a los glúteos (ACE);
  - la cadera no se despega.

### Cómo está ahora
- `Training.cs:80-81`: un banco plano de 0,45 × 0,12 × 1,3 a 0,5 m.
- `Training.cs:95`: un rodillo en (0, 0,35, −0,35), **por debajo del banco y en el extremo de la cabeza**, que no
  coincide con los tobillos.
- Torre y cable genéricos.

### Qué cambiar
1. Banco de 0,32 de ancho en tres tramos:
   - pecho: de z = −0,85 a −0,10, con la parte superior a 0,62 m;
   - joroba: a 0,68 m, entre z = −0,10 y +0,25;
   - muslos: a 0,66 m, entre z = +0,25 y +0,50.
2. Pivote en (+0,30, 0,70, +0,58).
3. Rodillo de tobillo (Ø0,12 × 0,40):
   - al empezar, en (0, 0,74, +0,98);
   - al final, en (0, 1,10, +0,50).
4. Asideros en (±0,22, 0,45, −0,80).
5. Pila de 1,5 m a un lado, en x = −0,55.

### Postura de uso
**Puesto**: (0, —, +0,10), mirando hacia −Z: la cabeza va hacia los asideros. `rootHeight` = **0,81**.

| Hueso | A · piernas estiradas | B · talones hacia los glúteos |
|---|---|---|
| Hips | (105, 0, 0): boca abajo, cadera en la joroba | (105, 0, 0) |
| Back / Chest | 0 / 0 | 0 / 0 |
| Neck / Head | (−30, 0, 0) / (−15, 0, 0): mira al frente | igual |
| ShoulderL/R | (−90, 0, ±15) | igual |
| ForearmL/R | (−30, 0, 0): agarra los asideros | igual |
| ThighL/R | (−15, 0, ±5) | (−15, 0, ±5) |
| ShinL/R | (5, 0, 0) | (105, 0, 0) |

## 9. Hip thrust (glúteos, tipo 7)

### Cómo es de verdad
- **Huella**: 1,55–1,75 × 1,2–1,5 × 0,9–1,0 m.
  - Nautilus Glute Drive: 157 × 152 × 89 cm.
  - Hammer Strength Glute Drive: 173 × 149 × 102 cm.
- **Piezas**:
  - **almohadilla de espalda** semicilíndrica a la altura de las escápulas, a unos 0,40–0,45 m del suelo;
  - asiento o plataforma bajos;
  - **plataforma de pies** delante;
  - **brazo de palanca** con una **almohadilla o cinturón sobre la cadera**;
  - cuernos de discos o pila lateral.
- **Movimiento**:
  - se empieza con la cadera abajo, flexionada unos 90°, que es donde el momento extensor es mayor (estudio de 2017);
  - se termina con la **cadera extendida**: el tronco y los muslos quedan en línea horizontal, con las **espinillas
    verticales y la rodilla a 90°**.

### Cómo está ahora
- `Training.cs:80-81`: un banco plano de 0,5 m.
- `Training.cs:96`: una «barra de cadera» de 0,7 × 0,08 × 0,2 a 0,75 m.
- La persona tendría que tumbarse encima del banco, que no es como se hace.

### Qué cambiar
1. Quitar la torre y el banco.
2. Almohadilla de espalda (Ø0,20 × 1,0) a lo largo de X, con la parte superior a **0,43 m**, en z = +0,40, sobre dos
   patas.
3. Asiento bajo de 0,45 × 0,08 × 0,35 a **0,18 m**, en z = 0.
4. Plataforma de pies de 0,9 × 0,05 × 0,45 a 0,05 m, en z = −0,55, inclinada 10°.
5. Dos brazos de palanca desde (±0,55, 0,40, +0,40) hasta una almohadilla de cadera (Ø0,15 × 0,6):
   - al empezar, en (0, 0,40, −0,10);
   - al final, en (0, 0,62, −0,20).
6. Cuernos con discos en los extremos de los brazos.

### Postura de uso
**Puesto**: (0, —, −0,10), mirando hacia −Z, con la espalda hacia la almohadilla. `rootHeight` va de **0,25 (A)** a
**0,48 (B)**: la pelvis sube unos 0,22 m.

| Hueso | A · cadera abajo | B · cadera arriba (bloqueo) |
|---|---|---|
| Hips | (−35, 0, 0) | (−90, 0, 0): tronco horizontal |
| Back / Chest | (10, 0, 0) / 0 | 0 / 0 |
| Neck / Head | (20, 0, 0) / 0 | (35, 0, 0) / 0: barbilla metida, mira las rodillas |
| ShoulderL/R | (10, 0, ±25) | (5, 0, ±25) |
| ForearmL/R | (−20, 0, 0): manos en la almohadilla de cadera | igual |
| ThighL/R | (−85, 0, ±12) | (0, 0, ±12) |
| ShinL/R | (110, 0, 0) | (90, 0, 0) |

En B la pelvis queda a unos 0,22 m delante de la almohadilla, en z ≈ −0,20. Es la única máquina en la que el puesto se
desplaza durante la repetición: hay que interpolar la raíz entre z = −0,10 y z = −0,20.

## 10. Gemelos sentado (tipo 8)

### Cómo es de verdad
- **Huella**: 1,2–1,5 × 0,6–0,8 × 0,85–1,0 m. Por ejemplo, Titan: 122 × 60 × 100 cm; Perform Better: 147 × 76 × 85 cm.
- **Piezas**:
  - asiento corto **sin respaldo**;
  - **almohadilla de rodillas** sobre la parte final de los muslos, regulable entre **0,72 y 0,81 m** (Titan) y de
    unos 42 cm de largo;
  - **calzo para las puntas de los pies** delante;
  - palanca con cuerno de discos y una maneta de seguridad.
- **Movimiento**:
  - la rodilla se mantiene a unos 90°, que hace trabajar el **sóleo**;
  - el talón baja hasta estirar del todo (dorsiflexión) y sube hasta contraer del todo (flexión plantar).

### Cómo está ahora
- `Training.cs:97`: un escalón de 0,6 × 0,08 × 0,3 a 0,2 m.
- Asiento y **respaldo** genéricos, con la torre de 2,1 m.
- Le falta la almohadilla de rodillas.

### Qué cambiar
1. Quitar la torre y el respaldo.
2. Asiento de 0,35 × 0,08 × 0,35 a **0,55 m**, en z = +0,05.
3. Calzo de 0,50 × 0,12 × 0,15 en (0, 0,06, −0,47).
4. Almohadilla de rodillas de 0,42 × 0,08 × 0,15 a 0,72 m, en z = −0,32, sobre una palanca que pivota delante y abajo,
   en (0, 0,25, −0,65). Sube ≈0,06 m en cada repetición.
5. Cuerno con discos en el extremo trasero de la palanca.

### Postura de uso
**Puesto**: (0, —, +0,05), mirando hacia −Z. `rootHeight` = **0,67**.

| Hueso | A · talones abajo | B · talones arriba |
|---|---|---|
| Hips | (0, 0, 0) | (0, 0, 0) |
| Back / Chest | (15, 0, 0) / 0: algo inclinado sobre las rodillas | igual |
| Neck / Head | 0 / 0 | 0 / 0 |
| ShoulderL/R | (−35, 0, ±10) | igual |
| ForearmL/R | (−55, 0, 0): manos en la almohadilla | igual |
| ThighL/R | (−90, 0, ±6) | (−96, 0, ±6) |
| ShinL/R | (90, 0, 0) | (96, 0, 0) |
| FootL/R | (−15, 0, 0) | (25, 0, 0) |

## 11. Curl de bíceps y extensión de tríceps (tipo 9)

### Cómo es de verdad
- **Huella**: máquina combinada Hoist HD-3100, de 130–137 × 123 × 140 cm, con pila de 86–102 kg. Otro ejemplo: TKO,
  de 135 × 122 × 150 cm.
- **Piezas**:
  - asiento regulable;
  - **apoyo de brazos inclinado** (tipo predicador, de 35 a 45°) delante del pecho;
  - **leva o palanca con el pivote alineado con el codo**;
  - brazo bidireccional: agarre supino para el curl y neutro para la extensión;
  - pila a un lado o detrás.
- **Movimiento**:
  - el brazo queda quieto sobre el apoyo y solo se mueve el codo;
  - el curl va de casi extendido a unos 125°;
  - la extensión de tríceps hace el recorrido inverso.

### Cómo está ahora
- `Training.cs:89-90`: dos agarres sueltos a 1,05 m. No tiene apoyo de brazos ni leva.

### Qué cambiar
1. Asiento a **0,52 m**, en z = +0,10.
2. Apoyo de brazos de 0,50 × 0,06 × 0,35, **inclinado 40°**: borde alto (axilas) a **1,05 m** en z = −0,10, y borde
   bajo (codos) a 0,85 m en z = −0,35.
3. Pivote y leva en (±0,30, 0,88, −0,38).
4. Barra de agarre en la palanca, de 0,5 × 0,04 × 0,04:
   - en reposo, en (0, 0,62, −0,55), con el brazo extendido;
   - arriba, en (0, 1,05, −0,30).
5. Pila de 1,4 m detrás y a la derecha, en (+0,45, —, +0,40).
6. Quitar el respaldo.

### Postura de uso
**Puesto**: (0, —, +0,10), mirando hacia −Z. `rootHeight` = **0,64**.

| Hueso | A · codo extendido (inicio del curl, final del tríceps) | B · codo flexionado (final del curl, inicio del tríceps) |
|---|---|---|
| Hips | (0, 0, 0) | (0, 0, 0) |
| Back / Chest | (15, 0, 0) / (5, 0, 0): pecho contra el apoyo | igual |
| Neck / Head | 0 / (5, 0, 0) | igual |
| ShoulderL/R | (−30, 0, ±10): brazo sobre el apoyo | igual |
| ForearmL/R | (−15, 0, 0) | (−125, 0, 0) |
| ThighL/R | (−90, 0, ±8) | igual |
| ShinL/R | (90, 0, 0) | igual |

Las 3 máquinas de este tipo pueden alternar el ejercicio: serie de curl (A → B → A) y serie de tríceps (B → A → B).

---

## 12. Zona libre y lo que falta

- **Mancuernas** (`Training.cs:47-49`): hoy son un solo estante de 3 × 0,06 × 0,5 a 0,4 m con 10 cubos negros.
  - Un estante real de 3 pisos mide 1,5–2,3 × 0,5–0,85 × 0,85–1,0 m (Core: 164 × 93 × 102; Hammer Strength:
    229 × 69 × 99; Gym Gear: 198 × 86 × 93), con las bandejas inclinadas de 0 a 30°.
  - Propuesta: 2 bandejas, a 0,55 y 0,85 m, inclinadas 15°, y mancuernas de 3 piezas (dos cabezas hexagonales y un
    mango de 0,13 m).
- **Barra de dominadas** (`:50-53`): 2,3 m es correcto. Una persona de 1,85 m llega a unos 2,35 m de puntillas.
- **Kettlebells y sacos** (`:45-46`, `:54-60`): sirven tal como están.
- **No existen**, aunque los mencionan el `<summary>` de `:14-15` y `docs/jornada-y-rotacion.md:91`: la jaula de
  sentadillas, los bancos, las cintas, el remo de cardio y los espejos. Hay dos opciones: quitar esas menciones o
  añadirlos a la zona libre.
  - Cinta de correr: 2,0–2,2 × 0,8–0,9 × 1,4–1,6 m. Pide **2 m libres detrás y 0,5 m a los lados** (ASTM F2115-12).
  - Zona de jaula o peso libre: 1,8–2,4 m libres alrededor.

## 13. La sesión diaria de entrenamiento en el gimnasio

**Cuánto dura.** El entrenamiento va de 14:00 a 16:00, 2 h de juego. Un día de juego dura 1 h real, así que 1 min de
juego son 2,5 s reales y la sesión entera unos **5 min reales**.

**Quién va.** Las personas a las que `WorkRoster` asigna «Gimnasio» ese día, por la rotación *(i + d) mod n*
(`WorkRoster.cs:127-129`). Con yoga y gimnasio abiertos, va más o menos la mitad del personal. Los clones de la Magnate
no entrenan: solo trabajan.

**Cómo se reparte el tiempo** (en minutos de juego):

| Bloque | Duración | Qué se hace |
|---|---|---|
| Calentamiento | 10 min | Zona libre: esterillas, movilidad, kettlebells ligeras (y la cinta, si se añade). |
| Circuito | **10 estaciones × 9 min = 90 min** | En cada estación, **3 series** de 10–12 repeticiones, con 60–90 s de descanso, más el cambio de máquina. |
| Subir a la entreplanta | 5 min | Pasar del piso de abajo al de arriba a mitad del circuito. |
| Estiramientos | 15 min | Esterillas de la zona libre: se pueden reutilizar las posturas del Saludo al Sol o del upa-yoga. |

**Orden del circuito.** Se adapta a la colocación actual (tipos 0–4 abajo, 5–9 arriba), así que **solo se sube una
vez**. En cada piso se alternan tren superior e inferior:

- **Abajo**: Prensa → Press de pecho → Jalón → Press de hombros → Remo.
- **Arriba**: Extensión de cuádriceps → Curl de bíceps y tríceps → Curl femoral → Gemelos → Hip thrust.

**A qué máquina va cada uno.**

- La persona *i* del grupo del gimnasio empieza en la estación *(i + d) mod 5* de su piso y avanza una estación cada
  9 min. La mitad del grupo empieza abajo y la otra mitad arriba, y a los 45 min se cruzan.
- Usa la copia *(i / 5) mod 3* de cada tipo. Así caben hasta **30 personas a la vez sin colas**, y lo normal (≤ 10)
  queda muy holgado.

**Cómo se ve.**

- Cada estación dura 9 min de juego, unos 22 s reales.
- Se animan **2 series visibles de 3 repeticiones** a ritmo real: unos 3 s por repetición, 1 s de subida y 2 s de
  bajada.
- Entre series hay unos 4 s en la postura A con la cara de esfuerzo (`CubeFace`).
- El resto del tiempo de la estación se da por hecho.
- La primera vez de cada persona, la Magnate la inicia como en el resto de misiones (`WorkdayDirector.Begin`): hace la
  repetición a su lado.

**Qué hace falta en el código.**

1. Un componente `GymStation` en cada `Maquina_*`, al estilo de `HospitalStation`, con:
   - el tipo de máquina;
   - el `Puesto`, que guarda la posición y el giro;
   - las **dos posturas** (`AsanaPoseSpell`, con su `rootHeight`) de este documento;
   - quién la ocupa, con métodos para reservarla y liberarla.
2. En `WorkdayDirector.Drive`, junto a `YogaClass` (`:339`), un caso `GymSession(g)` cuando el área es «Gimnasio» y la
   hora está entre 14 y 16:
   - elegir la estación según el horario de arriba;
   - caminar hasta el `Puesto`;
   - alinear el giro y alternar A ↔ B;
   - liberar la máquina al cambiar de estación.
   Hoy `AreaPoint` (`:412-426`) los deja de pie en una rejilla.
3. Las posturas pueden ir en una clase `GymPoses`, junto a `DailyPoses` (`DailyLife.cs:10`), con
   `Make(nombre, rootHeight, ...)`.
4. Opcional: que cada estación sume fuerza a los grupos de `BodyPart` que trabaja (`docs/asana-system.md`, ejercicios
   básicos):

   | Máquina | Partes que se fortalecen |
   |---|---|
   | Press de pecho | Shoulders, Elbows |
   | Jalón y remo | Back, Shoulders |
   | Prensa, extensión y curl femoral | Knees, Hips |
   | Gemelos | Feet |
   | Hip thrust | Hips |
   | Bíceps y tríceps | Elbows |

5. Corregir el comentario de `Training.cs:23` (la entreplanta está a 4 m, no a 3) y el `<summary>` duplicado
   (`:14-17`).

## Fuentes

- Press de pecho:
  - [Hammer Strength Select Chest Press (Life Fitness)](https://lifefitness.com/en-us/catalog/strength-training/selectorized/hammer-strength-select-chest-press)
  - [Decathlon Future Elite Seated Chest Press](https://www.decathlon.co.uk/p/mp/future-elite-series-commercial-seated-chest-press/33541219-4d88-4d17-b231-ea3ac002a894/novar)
  - [Life Fitness Insignia Chest Press](https://lifefitness.com/en-hk/catalog/strength-training/selectorized/insignia-series-chest-press)
  - [ACE: Seated Chest Press](https://www.acefitness.org/resources/everyone/exercise-library/188/seated-chest-press/)
  - [NASM: Chest Press Machine](https://www.nasm.org/resource-center/exercise-library/chest-press-machine)
- Press de hombros:
  - [Hammer Strength Select Shoulder Press](https://www.lifefitness.com/en-us/catalog/strength-training/selectorized/hammer-strength-select-shoulder-press)
  - [Precor Vitality Shoulder Press (PDF)](https://beyondthebasicsfitness.com/images/pdfUploads/1717120167_Precor%20Vitality%20Shoulder%20Press.pdf)
  - [Spirit 43 Pin Select Shoulder Press](https://gymequipment.co.uk/spirit-43-pin-select-shoulder-press)
- Jalón al pecho:
  - [Life Fitness Axiom Lat Pulldown](https://www.lifefitness.com.au/commercial/axiom-series-lat-pulldown)
  - [York ST Lat Pulldown](https://yorkbarbell.com/product/st-lat-pulldown/)
  - [Good Life Lat Pull-Down (Garage Gym Reviews)](https://www.garagegymreviews.com/?p=206462)
  - [NASM: Biomechanics of the Lat Pulldown](https://blog.nasm.org/biomechanics-of-the-lat-pulldown)
  - [Advanced Human Performance: pull-up technique](https://www.advancedhumanperformance.com/blog/pullups-best-technique)
- Remo sentado:
  - [Precor Vitality Seated Row (PDF)](https://beyondthebasicsfitness.com/images/pdfUploads/1717121744_Precor%20Vitality%20Seated%20Row.pdf)
  - [Muscle-D Classic Seated Row](https://gymstore.com/muscle-d-classic-seated-row)
  - [Reeplex Iron Series Seated Row](https://dynamofitness.com.au/products/reeplex-iron-series-commercial-pin-loaded-seated-row-machine)
- Prensa de piernas:
  - [Force USA 45° Leg Press](https://wholesale.forceusa.com/products/force-usa-45-degree-leg-press-with-calf-block-black)
  - [Primal Club 45° Leg Press](https://fitnesssupply.com/collections/primal-strength/products/club-45-degree-leg-press-psss2208)
  - [TF 1024D](https://intowellness.in/?p=10930)
  - [Técnica de la prensa 45° (EFDeportes)](https://efdeportes.com/efd215/o-leg-press-45-graus.htm)
  - [PureGym: 45 degree leg press](https://one.puregym.com/exercises/legs/quad-exercises/leg-presses/45-degree-leg-press)
- Extensión de cuádriceps y curl femoral:
  - [Body-Solid Pro Select Leg Ext/Curl](https://bodysolid.com/home/GCEC-STK/PRO-Select_Leg_Ext____Leg_Curl_Machine/Technology)
  - [Body-Solid Powerline](https://bodysolid.com/powerline-leg-curl-extension)
  - [Power Lift Prone Leg Curl](https://performaxx.com.au/?p=16822)
  - [ACE: Prone Hamstrings Curl](https://acefitness.org/resources/everyone/exercise-library/131/prone-lying-hamstrings-curl)
  - [Gravitus: Lying Leg Curl](https://gravitus.com/guides/exercises/lying-leg-curl)
- Hip thrust:
  - [Nautilus Glute Drive](https://www.beyondthebasicsfitness.com/products/Nautilus+Glute+Drive/80)
  - [Hammer Strength Plate Loaded Glute Drive](https://lifefitness.com/en-us/catalog/strength-training/plate-loaded/hammer-strength-plate-loaded-glute-drive)
  - [Estudio de biomecánica del hip thrust (core.ac.uk)](https://core.ac.uk/download/326444065.pdf)
  - [Hip thrust frente a puente de glúteo (UCAM)](https://ccd.ucam.edu/visores/19_59/2084_E_CCD/index.html)
- Gemelos:
  - [Titan Seated Calf Raise](https://titan.fitness/products/seated-calf-raise-machine)
  - [Perform Better Seated Calf Raise](https://www.performbetter.com/Signature-Series-Plate-Loaded-Seated-Calf-Raise/)
  - [Calf raises (Wikipedia)](https://en.wikipedia.org/wiki/Calf_raises)
- Bíceps y tríceps:
  - [Hoist HD-3100 Preacher Curl/Triceps Extension](https://hoistfitness.com/products/hd-3100-preacher-curl-triceps-extension)
  - [TKO Bicep Curl/Tricep Extension](https://thefitnessoutlet.com/products/tko-bicep-curl-tricep-extension)
- Mancuernas:
  - [Core 3-Tier Dumbbell Rack](https://www.corehandf.com/products/3-tier-dumbbell-rack)
  - [Hammer Strength 3-Tier Rack](https://gymequipment.co.uk/hammer-strength-dumbbell-rack-3-tier)
  - [Gym Gear Classic 3-Tier](https://gymgear.com/products/classic-series-3-tier-dumbbell-rack-without-saddles)
- Espacios libres:
  - [Livestrong: Gym Equipment Placement Standards](https://livestrong.com/article/415103-gym-equipment-placement-standards)
  - [Achievable (CSCS): facility layout](https://app.achievable.me/study/cscs/learn/facility-design-layout-and-organization)
  - [ACSM CPT study guide: facility safety](https://open-exam-prep.com/study-guides/acsm-cpt/chapter-10-leadership-instruction/equipment-facility-safety)
  - [Bodycraft T400: espacios libres ASTM F2115 (PDF)](https://www.bodycraft.com/pdfs/Footprints/t400-dims.pdf)

Las fuentes son fichas técnicas de fabricantes y guías de entrenadores. Ninguna es la norma ACSM ni la ASTM originales
(son de pago). Las medidas varían según el modelo; aquí se ha elegido un valor intermedio de cada rango.
