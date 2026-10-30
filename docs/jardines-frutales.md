# Frutales en los techos de alfombras (investigación → plan → construcción)

Pedido de Berón (2026-10-30): «Algunas plataformas de alfombras son muy grandes. El espacio libre se llena con una planta
frutal de cada tipo hasta que no quepa más. Hay que investigar los frutales que ya existen y saber qué cuidados necesitan:
ese será uno de los trabajos del edificio. Hace falta una experta en plantas y árboles que sepa cuidarlos y maximizar su
bienestar. Se ha preparado un espacio con todo el equipo necesario, y se contrata a una persona que revise y nutra el
jardín cada día. Si una persona termina rápido, pasa al edificio siguiente; según lo que tarde, el cuidado de todos los
edificios lo hace una persona al día o se reparte entre varias».

Código: `Assets/Scripts/Farming/FruitCare.cs` (fichas), `FruitPlant.cs` (la maceta y su estado), `FruitGardener.cs` (la
ronda y el punto de cuidados), `Assets/Editor/SampleSceneBuilder.Orchards.cs` (los planta), `AreaWork.Today` (las
misiones), `WorkdayDirector.Drive` (quien tiene la ronda la hace) y `Assets/Scripts/Tests/FruitOrchardTest.cs` (grupo 55).

## 1. Investigación

### Qué frutales hay en el juego

Se buscó en `GardenCrop.Kind`, los gremios de `GardenPlot`, `Gastronomy` y las recetas. Son **frutales** las plantas que
se cultivan por su fruto o su fruto seco. Hay **19**:

- **Árboles**: mango, aguacate, guanábana, cacao, almendro, olivo, naranja, limón y lima.
- **Palmeras**: coco y dátil.
- **Arbustos**: café, babaco, banana, naranjilla y mortiño.
- **Herbáceas**: piña y frutilla.
- **Rastrera**: sandía.

No entran:
- Los árboles de **especia**: canela (corteza), laurel (hoja), clavo (botón floral) y pimienta (enredadera de especia).
- Las **hortalizas de fruto** (tomate, pimiento, berenjena, pepino, calabaza…), que son del Huerto.

Todas ya existen en `GardenCrop` (forma, colores y nombre) y en la cocina (`Gastronomy`). Lo que faltaba eran los
**cuidados**. Van en una sola tabla nueva, `FruitCare.All`, que no repite nada de lo que ya hay.

### Dónde crecen: el santuario a −77,85°

Fuera, la vida de estas plantas es imposible:
- **Temperatura en McMurdo**: −18 °C de media al año. En enero, unos −3 °C; en agosto, unos −28 °C. Mínima de −50 °C
  (USAP Participant Guide; NSF).
- **Noche polar**: unos **3,8 meses** sin sol (24 abr – 19 ago). En verano, unos 4 meses de sol de medianoche.

**Precedentes antárticos.** Todos cultivan plantas anuales pequeñas en hidroponía, con LED, a 20–25 °C. Ninguno ha
cultivado frutales leñosos.

| Instalación | Qué hizo | Lección |
|---|---|---|
| **EDEN ISS** (Neumayer III, 2018) | 268 kg en 9,5 meses sobre 12,5 m²: lechuga, pepino, tomate, rábano y hierbas | **Las fresas y los pimientos fracasaron por falta de polinización** |
| **Invernadero de McMurdo** | Hidropónico, con luz artificial | — |
| **South Pole Food Growth Chamber** (U. Arizona) | ~2,8 kg al día, **fresas incluidas** | — |

**En el juego**, la flora del santuario es *modificada* (crece sobre el hielo: docs/huertos-y-cultivos.md), pero los
cuidados son los reales. El techo es un jardín de clima controlado: LED en la noche polar, calefacción y abejorros.

### Ficha de cada especie

Valores para una maceta grande, de variedad enana o sobre patrón enanizante. Fuentes:
- UF/IFAS EDIS.
- Morton 1987, *Fruits of Warm Climates*.
- UC Davis / UC ANR.
- RHS.
- Cenicafé.
- Kempler y Kabaluk 1996 (*HortScience* 31:785).
- Frontiers in Plant Science 2017.
- space.com (EDEN ISS).

Cada ficha de `FruitCare.cs` lleva su fuente. Lo marcado «gen.» es conocimiento hortícola general (sin ficha consultada).

| Frutal | Agua | Luz | Temperatura | Sustrato | Nutrientes | Poda / plagas | Cosecha · polinización | En el santuario |
|---|---|---|---|---|---|---|---|---|
| Mango | húmedo creciendo, seco antes de florecer; 2–4 L/día | pleno sol | 24–30 °C; daño < 1 °C; noches < 15 °C inducen flor | drenado, pH 5,5–7 | N moderado, K, B, Zn | despunte tras cosecha · antracnosis, oídio, cochinilla, trips | injertado 3–4 años; 3–5 meses de flor a fruto · casi autofértil, pincel | posible con esfuerzo (LED intensos) |
| Aguacate | húmedo, drenaje perfecto; 2–4 L/día | pleno sol | 20–28 °C | aireado, pH 5,5–6,5 | N, Zn, Fe; sensible a sal y B | bajo · *Phytophthora*, trips, ácaros | 3–4 años · flores A/B: dos cultivares o pincel | posible con esfuerzo (gen.) |
| Guanábana | parejo, sin encharcar | sol a sombra ligera | 25–30 °C; daño < 5 °C | drenado, pH 5,5–6,5 | NPK equilibrado | formar bajo · cochinillas | 3–5 años; 4–6 meses · protógina: **a mano** | posible con esfuerzo |
| Cacao | siempre húmedo; HR 70–80 % | **semisombra** | 18–32 °C; daño < 10 °C | orgánico, pH 6–7 | mucho N y K; Zn | horqueta · mazorca negra, míridos | 3–5 años · cuaja 0,5–5 %: **a mano** | posible con esfuerzo |
| Almendro | seco entre riegos | pleno sol | **350–500 h de frío** | drenado, pH 6–8 | N, Zn, B | vaso · araña roja, monilia | 3–4 años · autoincompatible: abejas o pincel | difícil (zona de frío) |
| Olivo | seco entre riegos | pleno sol | **fresco oscilante ~2 meses (2–15 °C)** para florecer | muy drenado, pH 6,5–8,5 | poco: N, B | aclarar · repilo, cochinilla | 3–4 años · 'Arbequina' autofértil | difícil (zona de frío) |
| Naranja | secar los primeros cm; 1–3 L/día | pleno sol | 22–30 °C; un fresco colorea la piel | drenado, pH 6–7 | N + quelatos Fe/Zn/Mn | ligera · cochinilla, mosca blanca, *Phytophthora* | ~3 años; 7–12 meses · autofértil | posible |
| Limón | igual | pleno sol | 20–32 °C; florece todo el año | drenado, pH 6–7 | N + quelatos | ligera · igual | 2–3 años; 6–9 meses · autofértil | realista ('Improved Meyer' / Flying Dragon) |
| Lima | igual | pleno sol | 22–32 °C; la más friolera | drenado, pH 6–7 | igual; clorosis férrica | ligera | 2–3 años; 3–5 meses · partenocárpica | realista |
| Coco enano | mucha agua (3–5 L/día) | luz muy alta | 27–32 °C; sufre < 10–13 °C | arenoso, pH 5,5–8 | K, Cl, Mg, B | hojas secas · ácaros | 3–4 años; 12 meses · autopolinización | **difícil**: aun enano llega a 5–10 m (aquí, joven) |
| Dátil | riego profundo, tolera sal | pleno sol | **mucho calor y aire seco** al madurar | arenoso, pH 7–8,5 | K, N | hojas viejas · pudrición con humedad | 4–8 años · **dioica: a mano** | **difícil**: de 15–25 m; en maceta, decorativa |
| Café | parejo; estrés hídrico → flor | **semisombra** | ~21 °C (10–32) | orgánico, pH 5–6,5 | N, K, Zn, B | despunte · roya, cochinilla | 3–4 años; ~2500 grados-día · autógamo | realista ('Caturra') |
| Babaco | húmedo, muy propenso a pudrición | luz viva | 10–18 °C (subtropical fresco) | ligero, pH 6–6,5 | mucho K y Ca | tallo único · *Fusarium*, *Phytophthora* | **10 meses**, ~40 frutos · partenocárpico | realista: ideal en maceta |
| Banana enana | muy sedienta (5–10 L/día) | pleno sol | 26–30 °C; para < 14 °C | rico, pH 5,5–7 | **K:N ≈ 3:1**, Mg | madre + hijo · trips, pulgón, mal de Panamá | 9–12 + 3–4 meses · partenocárpica | realista (gen.) |
| Naranjilla | húmedo, aire húmedo | **semisombra** | **17–19 °C**; sufre > 29 °C | orgánico, pH 5–6,5 | NPK | ligera · **nematodo**: sustrato estéril | ~1 año · autofértil | realista |
| Mortiño | húmedo, nunca encharcado | sol a sombra ligera | páramo: 8–16 °C | **ácido, pH 4,5–5,5** | poco: N amoniacal, Fe | mínima · botritis | 2–3 años · **por vibración** | realista |
| Piña | secar entre riegos (CAM) | pleno sol | 20–30 °C; noches frescas → flor | arenoso, **pH 4,5–5,5** | N, K, Fe | hijuelos · cochinilla (virus) | 18–24 meses (inducida 5–7) · sin polinizar | realista |
| Frutilla | húmedo, goteo | pleno sol; **día neutro** | 15–25 °C | turba, pH 5,5–6,5 | K, Ca, B | estolones · botritis, oídio, araña roja | 3 meses · **abejas o pincel** (si no, deforme) | realista (Polo Sur) |
| Sandía | húmedo hasta cuajar | pleno sol | 21–32 °C | franco-arenoso, pH 6–6,8 | K, Ca | espaldera, 1–2 frutos · oídio, araña roja | 80–100 días · **abejas o a mano** | realista en espaldera (gen.) |

**Lecciones para el cuidado**:
- **Polinizar.** Fresa, sandía, mortiño, almendro, aguacate, guanábana, cacao y dátil necesitan abejas o pincel (la
  lección de EDEN ISS). Por eso la sala lleva una **caja de abejorros**.
- **Frío estacional.** Almendro y olivo necesitan un invierno frío para florecer. Hay que reservar un rincón que se enfríe
  en invierno, y el frío de fuera sobra.
- **Semisombra.** Cacao, café y naranjilla piden **semisombra**.
- **Sustrato ácido.** Mortiño y piña, pH 4,5–5,5.
- **Drenaje.** Cítricos y aguacate necesitan drenaje perfecto contra la *Phytophthora*.

### Frutales que faltan y encajarían

Se proponen; **no se han añadido**.
- **Papaya**: fruta en 9–12 meses; los hermafroditas se autopolinizan; probada en invernadero.
- **Uvilla** (*Physalis peruviana*): andina como la naranjilla; 13–18 °C; ya se cultiva en invernadero hidropónico.
- **Maracuyá**: trepadora para una espaldera; da fruto en menos de un año.
- **Higo**: los de tipo común no necesitan avispa; maceta y poco frío; dos cosechas al año.
- **Tomate de árbol**: andino, autofértil.

## 2. Plan

### Qué plataformas tienen sitio

Los **techos de alfombras** de las torres son plataformas del tamaño de la planta de la torre. Llevan:
- dos áreas de alfombras de 7,6 × 4,4 m en el centro;
- una fila de paneles en un borde.

El resto estaba vacío.

| Techo | Tamaño | Cota |
|---|---|---|
| Servicios | 40 × 38 m | 78 m |
| Central Nuclear | 46 × 30 m | 23 m |
| Entrenamiento | 36 × 26 m | 78 m |
| Ciencias | 32 × 20 m | 89 m |
| Producción | 26 × 20 m | 45 m |

**No se usan**:
- **Los anillos**: ya están amueblados y tienen plazas de microcosmos.
- **La copa del Árbol de los Campos**: de 22 m, casi toda es alfombras; además, otro agente rediseña el árbol.
- **Los pisos con área**: sus plazas de flora son del microcosmos (regla 8).

Los techos **no tienen plazas reservadas** (`PlaceReservedSlots` las salta), así que no compiten con ninguna.

### Cuántas caben

Reglas de colocación:
- Cada maceta es un **mueble**: una raíz con `BoxCollider` de la maceta y sin renderer.
- Guarda los **2 m** de `FurnitureSpacing` con todo, medidos sobre la caja **entera** (maceta y copa), como la prueba.
- Queda a ≥ 2 m de las áreas de alfombras y de los paneles, y a 0,4 m del borde del techo.

Cómo se planta:
1. **Primera vuelta**: una de cada especie, de la más grande a la más pequeña.
2. Luego otra vuelta, y otra, hasta que una vuelta entera no coloca nada.
3. Cada maceta va al **hueco más ajustado**: el que deja menos holgura con lo que tiene al lado. Así no quedan huecos
   sueltos.
4. Si en la primera vuelta no caben las 19, la experta hace una **poda de contención**: las copas de árboles, palmeras,
   banana y sandía se reducen un 12 % cada vez, hasta un 64 %, y se vuelve a plantar.

Tamaños (variedades enanas en maceta):

| Tipo | Maceta | Escala de su forma | Alto |
|---|---|---|---|
| Árbol | 1,2 × 1,2 × 0,7 m (1 m³) | ×0,45–0,55 | 1,5–2,5 m (mango 2,5 m) |
| Arbusto | 0,9 m | — | — |
| Herbácea | 0,8 m | — | — |
| Sandía | 1,4 m, en espaldera | — | — |

**Simulación** (techos con sus alfombras y paneles, huellas de `GardenCrop`; `scratchpad/simroof`):

| Techo | Macetas | Especies | Violaciones de 2 m |
|---|---|---|---|
| Servicios | 67 | 19/19 | 0 |
| Central Nuclear | 59 | 19/19 | 0 |
| Entrenamiento | 39 | 19/19 | 0 |
| Ciencias | 28 | 19/19 | 0 |
| Producción | 22 | 19/19 | 0 |
| **Total** | **215** | — | — |

### La sala de cuidados y los puntos de riego

Cada punto es una línea de equipos (`FurnitureLine`, que van juntos) contra un borde libre, mirando hacia dentro, con su
zona de trabajo delante.
- **Servicios: «Sala de cuidados de los frutales · experta: Irosene».**
  - Banco de trasplante con macetas y sustrato.
  - Depósito de fertirriego de 1000 L con dosificador y manguera.
  - Armario de abonos y correctores (NPK, quelatos).
  - Mesa de diagnóstico: medidor de pH y EC, lupa, trampas cromáticas y cuaderno.
  - Estante de herramientas de poda y regaderas.
  - Compostador.
  - Caja de abejorros para polinizar.
  - Mochila pulverizadora.
  - Carro de cosecha.
  - Escalera de tijera.
- **Los demás techos: punto de cuidados** con depósito de 500 L con dosificador, carro de fertirriego y armario de
  herramientas.

### La experta

**Irosene** (docs/character-irosene.md): «tuvo grandes momentos en el huerto/jardinería» (Garden, Tier 2), y su energía
tras la transformación encaja con el trabajo físico.
- Las fichas de `FruitCare` son su saber, y la sala lleva su nombre.
- **Andrea** soñó con «frutales, animales y casas en los árboles» y con un restaurante de platillos de fruta: es el dueño
  que lo pidió.
- Lizmibet (médica), su esposo (obras) y Sadhguru (yoga) ya tienen su papel.

### El jardinero del día: la ronda

Es una misión de la jornada (`AreaWork.Today` → `FruitGardener.Missions`): **obligatoria** (una maceta no aguanta días
sin revisar) y **`roams`** (recorre varias torres; la Magnate no la devuelve). La reparte la rotación como las demás:
cualquiera del personal puede ser el jardinero del día, y la Magnate inicia al que la hace por primera vez.

Quien la tiene:
1. Va al punto de cuidados de su primer techo. En la sala principal, prepara la solución de abono y mide pH y EC
   (15 min). En los demás puntos, llena el carro (5 min).
2. Recorre las macetas en **zigzag por franjas de 4 m** desde su punto.
3. En cada maceta, el tiempo de su ficha: **2 min** un árbol o una palmera, **1,5** un arbusto, **1** una herbácea.
   - Mira hojas y envés.
   - Sondea la humedad.
   - Riega o fertirriega con la lanza.
   - Trata la plaga.
   - Lo anota.
   - `FruitPlant.Care`: agua al 100 %, +50 % de abono si está por debajo del 60 %, plaga −80 %, salud +10 %.
4. Al acabar un techo, **vuela en alfombra al siguiente**.

Las tareas que no son diarias quedan fuera de la ronda: poda, polinización con pincel, tratamientos de fondo y cosecha.

**El estado se ve**:
- Tres barras en una esquina de la maceta: azul el agua, ámbar el abono, verde la salud.
- Una etiqueta: «mango · agua 62 % · abono 48 % · salud 71 % · ¡plaga!».
- Agua y abono bajan cada día de juego según la ficha. El plátano y el coco gastan mucha agua; el olivo, el dátil y la
  piña, poca.
- La plaga sube 0,12 al día.
- La salud baja con sed, hambre o plaga y sube con cuidado.

### ¿Una persona o varias?

`FruitGardener.Rounds` lo calcula con la escena, en **horas de juego** (1 h de juego = 2,5 min reales). Suma tres cosas:
- la revisión de cada maceta;
- lo que se anda entre ellas a 2,6 m/s (un segundo real son 24 s de juego, así que andar cuenta mucho);
- 45 s reales por vuelo de techo a techo (≈ 0,3 h).

| Techo | Revisión | Andar (zigzag) | Total |
|---|---|---|---|
| Servicios | 125 min | 408 m ≈ 63 min | 3,1 h |
| Entrenamiento | 68 min | 247 m ≈ 38 min | 1,8 h |
| Ciencias | 52 min | 156 m ≈ 24 min | 1,3 h |
| Producción | 44 min | 132 m ≈ 20 min | 1,1 h |
| Central Nuclear | 104 min | 370 m ≈ 57 min | 2,7 h |

Con los 5 vuelos, la ronda entera es de **≈ 11,4 h de juego**. Cada persona tiene **5 h** de trabajo, y una misión no se
parte en la comida: como mucho, el bloque de la mañana de **3 h**. **Una sola persona no llega.**

La ronda se parte en tramos de ≤ 3 h, por techos seguidos del camino más corto desde la sala:
- Servicios, Entrenamiento, Producción, Ciencias y Central Nuclear.
- Servicios, que pasa de 3 h, se parte en dos.

Salen **5 rondas, de 2 a 3 h**: **cinco personas al día**, cada una con un tramo, y el resto de su jornada para otras
misiones.
- Se reparte sola: si se añaden o quitan macetas o cambia la duración del día, el reparto se recalcula.
- En el registro: `[Frutales] … ronda completa X h → N personas`.
- **La prueba mide** una ronda real y la compara con la estimada.

**Cómo bajarlo, si Berón prefiere menos personas**: con goteo automático por maceta, la revisión diaria baja a mirar y
ajustar (~1 min). Serían ~3 rondas, y el riego a mano quedaría para las de mucha sed.

## 3. Construcción y prueba

**Construcción**: `SampleSceneBuilder.PlaceRooftopOrchards()` va justo después de los paneles, en `Build`. Planta cada
techo y su punto de cuidados.
- Registro de cada techo: `[Frutales] Techo de «X» (A×B m): N macetas…`.
- Si falta alguna especie, avisa.

**Prueba**: `FruitOrchardTest`, grupo 55. Se añade sola al pedir las pruebas.
1. Cada techo tiene una de cada una de las 19.
2. 0 pares sin sus 2 m (`FurnitureSpacing.Violations` del techo).
3. Las macetas quedan a ≥ 2 m de las alfombras.
4. La sala tiene sus 10 equipos, y cada techo su punto.
5. Las barras y la etiqueta se ven en cada maceta.
6. Sin cuidar, el agua y la salud bajan; con la revisión, el bienestar sube (y la salud sigue subiendo al día siguiente).
7. Cada maceta está en una sola ronda, y cada ronda dura ≤ 3 h.
8. La jornada las tiene como misiones obligatorias y `roams`.
9. **Un cuerpo de prueba hace la ronda que cruza más techos (a ×4)**:
   - revisa ≥ 95 % de sus macetas;
   - pasa de un techo a otro;
   - termina dentro de las horas de su misión;
   - el bienestar medio sube.
