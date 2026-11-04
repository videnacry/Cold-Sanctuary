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

**En Unity** (blockout del 2026-10-31, cajas reales): Servicios 59, Central Nuclear 62, Entrenamiento 38, Ciencias 24 y Producción 21 (copas podadas al 64 %): **204 macetas**, las 19 especies en cada techo y 0 pares sin sus 2 m (`FruitOrchardTest`).

### La sala de cuidados y los puntos de riego

Cada punto es una línea de equipos (`FurnitureLine`, que van juntos) contra un borde libre, mirando hacia dentro, con su
zona de trabajo delante.
- **Servicios: «Sala de cuidados de los frutales · experta: Irosene».**
  - Banco de trasplante con macetas y sustrato.
  - Depósito de fertirriego de 1000 L con dosificador y manguera.
  - Cabezal del riego por goteo (filtro, electroválvula, regulador de presión y programador).
  - Armario de abonos y correctores (NPK, quelatos).
  - Mesa de diagnóstico: medidor de pH y EC, lupa, trampas cromáticas y cuaderno.
  - Estante de herramientas de poda y regaderas.
  - Compostador.
  - Caja de abejorros para polinizar.
  - Mochila pulverizadora.
  - Carro de cosecha.
  - Escalera de tijera.
- **Los demás techos: punto de cuidados** con depósito de 500 L con dosificador, carro de fertirriego y armario de
  herramientas, y el cabezal de su goteo.

### La experta

**Irosene** (docs/character-irosene.md): «tuvo grandes momentos en el huerto/jardinería» (Garden, Tier 2), y su energía
tras la transformación encaja con el trabajo físico.
- Las fichas de `FruitCare` son su saber, y la sala lleva su nombre.
- **Andrea** soñó con «frutales, animales y casas en los árboles» y con un restaurante de platillos de fruta: es el dueño
  que lo pidió.
- Lizmibet (médica), su esposo (obras) y Andrea (yoga, desde el 2026-11-04; antes Sadhguru) ya tienen su papel.

### Riego por goteo (decisión de Berón, 2026-10-31)

Con riego a mano, la ronda era de 12,4 h de juego: **5 personas al día** (lo que se midió en Unity, apartado de abajo).
Berón eligió el **goteo**: el agua llega sola y la ronda solo **revisa y nutre**.

Cada techo tiene su instalación:
- **Cabezal de riego** en su punto de cuidados (un equipo más de la línea): filtro de anillas, electroválvula, regulador
  de presión, programador y colector, junto al depósito con dosificador.
- **Tubería** de polietileno de 16 mm en el suelo (3 cm; se pisa sin problema) que sale del cabezal y va de maceta en
  maceta en el orden de la ronda, en L, hasta una esquina de cada maceta (`Riego_goteo_<torre>`, `Tuberia_goteo`).
- En cada maceta, un **gotero autocompensante** sobre el sustrato y su microtubo (dentro de la huella de la maceta: no
  cambia su caja de mueble ni los 2 m).

En la simulación, `FruitPlant.drip`:
- el gotero mantiene la maceta al **85 %** de humedad (repone lo que gasta la planta, sin encharcar);
- el **abono** sigue bajando cada día y la **plaga** sigue subiendo: eso es lo que hace la persona;
- en la revisión no se riega, salvo que el gotero se haya atascado (si la humedad bajó del 60 %, lo desatasca).

### El jardinero del día: la ronda

Es una misión de la jornada (`AreaWork.Today` → `FruitGardener.Missions`): **obligatoria** (una maceta no aguanta días
sin revisar), de **3 h** y **`roams`** (recorre varias torres; la Magnate no la devuelve). La reparte la rotación como las
demás: cualquiera del personal puede ser el jardinero del día, y la Magnate inicia al que la hace por primera vez.

Quien la tiene:
1. Va al punto de cuidados de su primer techo. En la sala principal prepara el abono y mide pH y EC (5 min); en los
   demás, mira el filtro, la válvula y el programador del goteo (2 min).
2. Recorre las macetas en **zigzag por franjas de 4 m** desde su punto.
3. En cada maceta, la **revisión** de su ficha: **1 min** un árbol o una palmera, **0,75** un arbusto, **0,5** una herbácea.
   - Comprueba el gotero (que gotee y no esté atascado).
   - Mira hojas y envés.
   - Abona y trata la plaga.
   - `FruitPlant.Care`: +50 % de abono si está por debajo del 60 %, plaga −80 %, salud +10 % (y desatasca el gotero si hace falta).
4. Al acabar las macetas de un techo, hace allí las **tareas de la semana** (el tiempo que libera el goteo) mientras
   quepan en su parte del tiempo que sobra de las 3 h:
   - **polinizar a pincel** (o sacudir, o vibrar) cada 2 días las que lo necesitan: mango, aguacate, guanábana, cacao,
     almendro, olivo, dátil, frutilla, sandía y mortiño (`FruitCare.Polinizar`; 1 min). Sube el **cuajado** de la planta,
     que se ve en su etiqueta y baja si nadie poliniza;
   - **podar y anotar el crecimiento** en el cuaderno de investigación cada 14 días (la frutilla, quitar estolones cada 7;
     la piña no se poda; `FruitCare.PodaDias`; 3 min). Las podas están repartidas: cada día le toca a una parte.
   - El tiempo que sobra se reparte por macetas entre lo que queda de la ronda; lo que no cabe, se deja para otro día.
     Se guarda una holgura de 0,05 h (`FruitGardener.ExtraMargin`; era 0,15 h, pero medida en Unity la ronda real va siempre
     algo por debajo de su estimación, y con rondas de 2,9 h esa holgura se comía todo: ni una polinización en el día).
     **Pendiente**: las tareas de la semana piden ~1,5 h al día (≈ 54 polinizaciones y ≈ 15 podas) y a las 3 rondas solo
     les sobran ~0,1 h cada una: se hace una parte cada día. Para hacerlas todas haría falta una cuarta ronda (o techos más
     juntos, que bajan lo que se anda).
5. **Vuela en alfombra al techo siguiente.**

Quedan fuera: la cosecha y los tratamientos de fondo.

**El estado se ve**:
- Tres barras en una esquina de la maceta: azul el agua, ámbar el abono, verde la salud.
- Una etiqueta: «mango · agua 85 % · abono 48 % · salud 71 % · goteo · cuajado 60 % · ¡plaga!».
- El abono baja cada día de juego según la ficha; con goteo, el agua se mantiene.
- La plaga sube 0,12 al día.
- La salud baja con hambre o plaga y sube con cuidado.

### ¿Una persona o varias?

`FruitGardener.Rounds` lo calcula con la escena, en **horas de juego** (1 h de juego = 2,5 min reales). Suma tres cosas:
- la revisión de cada maceta;
- lo que se anda entre ellas a 2,6 m/s (un segundo real son 24 s de juego, así que andar cuenta mucho);
- 45 s reales por vuelo de techo a techo (≈ 0,3 h) y la preparación en cada punto.

Recorre las macetas por el camino más corto desde la sala (Servicios, Entrenamiento, Producción, Central Nuclear y
Ciencias) y corta en rondas **equilibradas**: tantas como pida el total entre 2,9 h, cada corte con su vuelo y su
preparación (si salen más, sube la meta hasta 2,95 h).

**Con riego a mano** (medido en Unity, 2026-10-31): 204 macetas, revisión de 2 / 1,5 / 1 min, **12,4 h → 5 rondas** de
~2,5 h. Un cuerpo de prueba hizo `jar_frutales_3` (36 macetas, dos techos y el vuelo) en 2,25 h de juego (estimada 2,36 h).

**Con goteo** (simulación de los techos):

| Techo | Revisión | Andar (zigzag) | Total |
|---|---|---|---|
| Servicios | 68 min | 416 m ≈ 64 min | 2,2 h |
| Entrenamiento | 40 min | 254 m ≈ 39 min | 1,3 h |
| Ciencias | 26 min | 157 m ≈ 24 min | 0,8 h |
| Producción | 21 min | 124 m ≈ 19 min | 0,7 h |
| Central Nuclear | 54 min | 364 m ≈ 56 min | 1,8 h |

Con los vuelos y las preparaciones, **≈ 8,4 h de juego → 3 rondas de ~2,8 h**: **tres personas al día**. Cada una tiene
una misión de 3 h: la revisión de su tramo y, con lo que sobra, las tareas de la semana. El resto de su jornada (2 h) va
a otras misiones. Andar pesa ahora casi la mitad: con menos macetas por techo, o techos más juntos, bajaría.

**Con los edificios-árbol (2026-11-01): UN JARDINERO POR EDIFICIO.** Desde la migración (docs/edificio-arbol.md) los frutales
van en las PUNTAS de cada torre-árbol (una de cada especie por edificio: 19 en Servicios, Entrenamiento, Ciencias y Producción)
y el resto (61) en el techo de la Central Nuclear. Entrar en un árbol por su techo y salir, nadando, y pasar de una punta a
otra (a la puerta de su columna, por la rama y el tronco, y salir por la otra) cuesta lo suyo: con el llenado seguido de
antes salían **7 rondas (17,1 h)**, porque los edificios se partían (Servicios 16 + 3, Entrenamiento 18 + 1…) y cada trozo
pagaba otra vez el vuelo y el nado; y las especies saltaban de punta en punta. Ahora:
- las macetas de cada edificio van **punta a punta** (`FruitGardener.TipOrder`: todas las de una punta seguidas, empezando
  por la más alta —se entra por el techo— y siguiendo por la más cercana);
- cada edificio es **una unidad** de la ronda (vuelo, nado, preparar en su punto y sus macetas) y solo se parte si él solo
  no cabe en 2,9 h; unidades seguidas del camino se juntan en una ronda mientras quepan;
- el nado se estima con lo medido en Unity (2026-11-01, un jardinero de prueba por ronda): **60 s** de entrar y salir del árbol
  (`TreeSwimSeconds`, antes 170 s) y, de punta a punta, **1,3 × la recta + 10 m** (`Hop`, antes 2 × la recta + 60 m; medido
  ~38 s de punta a punta en Servicios).

Resultado: **9,0 h → 5 rondas**, una por edificio: Servicios 1,9 h · Entrenamiento 1,9 h · Producción 1,5 h · Ciencias 1,6 h
· Central Nuclear 2,1 h (medidas: Producción 1,81 h con sus tareas de la semana, Ciencias 1,41 h). **Cinco personas al
día**, una por edificio con frutales: cada una conoce sus puntas y le sobra ~1 h para polinizar y podar (lo que con 3
rondas no cabía, ver «Pendiente» arriba). La prueba (grupo 55) ya no pide «unas 3 personas»: con 4 árboles (≥ 1 h de nado
cada uno) y el techo de la Central eso no cabe en 3 rondas de 3 h; pide **como mucho una ronda por edificio con frutales y
ningún edificio partido**.
- Se reparte sola: si se añaden o quitan macetas o cambia la duración del día, el reparto se recalcula.
- En el registro: `[Frutales] … ronda completa X h → N personas`.
- **La prueba mide** una ronda real.

## 3. Construcción y prueba

**Construcción**: `SampleSceneBuilder.PlaceRooftopOrchards()` va justo después de los paneles, en `Build`. Planta cada
techo, su punto de cuidados con el cabezal y la red de goteo.
- Registro de cada techo: `[Frutales] Techo de «X» (A×B m): N macetas…` y `… goteo desde el cabezal … a N macetas`.
- Si falta alguna especie, avisa.

**Prueba**: `FruitOrchardTest`, grupo 55. Se añade sola al pedir las pruebas.
1. Cada techo tiene una de cada una de las 19.
2. 0 pares sin sus 2 m (`FurnitureSpacing.Violations` del techo).
3. Las macetas quedan a ≥ 2 m de las alfombras.
4. Cada techo tiene su goteo: cabezal, tubería y un gotero en cada maceta.
5. La sala tiene sus 11 equipos (con el cabezal), y cada techo su punto.
6. Las barras y la etiqueta se ven en cada maceta.
7. Sin revisar un día, el goteo le da el agua, pero pierde abono y salud; con la revisión, el bienestar sube (y la salud
   sigue subiendo al día siguiente).
8. Cada maceta está en una sola ronda, cada ronda dura ≤ 3 h y hay **como mucho una ronda por edificio con frutales**, sin partir ninguno (2026-11-01: antes, ≤ 3 rondas).
9. La jornada las tiene como misiones obligatorias de 3 h y `roams`.
10. **Un cuerpo de prueba hace la ronda que cruza más techos (a ×4)**:
    - revisa ≥ 95 % de sus macetas;
    - pasa de un techo a otro;
    - termina dentro de las 3 h de su misión;
    - con lo que sobra, poliniza o poda alguna;
    - el bienestar medio sube.

## Tareas del árbol: máscaras de oxígeno y fitoplancton (Berón 2026-11-03)

«Se debe crear la tarea de revisar máscaras de oxígeno y la realizaría también la persona que cuida los jardines frutales de los
edificios… y también ahora debe revisar/cuidar los fitoplancton para que estén en óptimas condiciones.» (docs/mascaras-de-oxigeno.md)

- Cada edificio-árbol añade a las rondas una **unidad de tareas del árbol** (`FruitGardener.Leg.tree`): entrar por el techo con una
  máscara, bajar nadando de puerta en puerta revisando el perchero y la estación de cada una (**2 min** de juego por puerta), cuidar
  las 4 cortinas de fitoplancton del fondo abierto (**4 min** cada una) y salir por la puerta del suelo.
- Se estima con el nado de los viajes (3,9 m/s con la corriente, 1,3 × la recta + 6 m entre puertas) y va **detrás de las macetas
  de su edificio**: en la misma ronda si cabe en 2,9 h; si no, es **otra ronda del mismo edificio** (otra persona ese día). El Árbol
  de los Campos, sin frutales, entra en el camino solo por sus tareas. Así la jornada se reparte sola de nuevo: el registro dice
  cuántas rondas salen (`[Frutales] … tareas de N árboles …`).
- La prueba (grupo 55) ya no pide «una ronda por edificio»: pide que las **macetas** de cada edificio no se partan, como mucho una
  ronda más por árbol con tareas, ninguna ronda de más de 3 h y, con un cuerpo de prueba, que las tareas de un árbol dejen todos sus
  puestos de máscaras y sus cortinas al 100 % en ≤ 1,5 × lo estimado, saliendo sin la máscara.

## Jardines en la cima de los edificios-árbol (Berón 2026-11-03)

«Agrega los jardines frutales a las plataformas de alfombras voladoras en la cima de los edificio-arbol y analiza/actualiza la
jornada, investiga si hay plantas, animales, insecto, flores o árboles que puedan facilitar el bienestar de los jardines frutales,
y si sería más conveniente un área de tierra en lugar de macetas.»

Código: `Assets/Scripts/Farming/GardenBed.cs` (el bancal y sus gremios), `FruitPlant.bed` (lo que cambia en tierra),
`Assets/Editor/SampleSceneBuilder.RoofGardens.cs` (los planta, desde `PlaceRooftopOrchards`) y la prueba `RoofGardenTest` (grupo 80).

### 1. Investigación: lo que ayuda a un jardín frutal

| Qué | Para qué | Fuente |
|---|---|---|
| **Abejorros** (*Bombus terrestris*, colonias comerciales) | Polinizan en invernadero (frutilla, sandía, aguacate; el mortiño, por vibración). Sin polinizadores, EDEN ISS perdió las fresas | Velthuis y van Doorn 2006, *Apidologie* 37:421–451 |
| **Abeja albañil** (*Osmia cornuta*, *O. lignaria*), con bloques nido de caña | Poliniza el almendro y los frutales de flor temprana; anida en tubos | Bosch y Kemp 2001, *How to Manage the Blue Orchard Bee* (SARE) |
| **Mosquitas *Forcipomyia*** | Son casi las únicas que polinizan el cacao; crían en pseudotallo de banana y en cáscaras podridas | Young 1994, *The Chocolate Tree* (Smithsonian) |
| **Enemigos naturales comerciales** | *Cryptolaemus montrouzieri* (cochinilla algodonosa de cítricos y café), *Aphytis melinus* (piojo rojo de los cítricos), *Phytoseiulus persimilis* (araña roja de la frutilla), *Amblyseius swirskii* (trips y mosca blanca), *Aphidius colemani* (pulgón), *Orius* (trips) | van Lenteren 2012, *BioControl* 57:1–20 |
| **Flores de néctar** (alyssum, facelia, milenrama, caléndula, borraja; umbelíferas en flor como el cilantro y el perejil) | Alimentan a los sírfidos, crisopas y avispillas parasitoides adultos: así se quedan y crían junto a la plaga | Landis, Wratten y Gurr 2000, *Annu. Rev. Entomol.* 45:175–201 |
| **Tagetes** (aquí, el huacatay, *Tagetes minuta*) | Frena a los nematodos de las agallas (*Meloidogyne*), el enemigo de la naranjilla | Hooks et al. 2010, *Applied Soil Ecology* 46:307–320 |
| **Capuchina** | Planta trampa del pulgón | Landis et al. 2000 |
| **Leguminosas** (haba, arveja, frijol, trébol) | Fijan nitrógeno y cubren el suelo (abono verde y acolchado vivo) | conocimiento agronómico general |
| **Consuelda** | Su hoja cortada es un acolchado rico en potasio | gen. |
| **Árboles de sombra** (la banana en las fincas de cacao y café; *Inga*, *Gliricidia*) | El cacao, el café y la naranjilla piden semisombra; la sombra baja el estrés y da hojarasca | Tscharntke et al. 2011, *J. Appl. Ecol.* 48:619–629 |
| **Lombrices** (vermicompost, *Eisenia fetida*) | Convierten los restos de poda y cosecha en abono y airean el suelo | Edwards, Arancon y Sherman 2011, *Vermiculture Technology* (CRC) |
| **Hongos micorrícicos** (inóculo) | Raíces con más alcance de agua y fósforo; los cítricos dependen mucho de ellos | Smith y Read 2008, *Mycorrhizal Symbiosis*, 3.ª ed. |
| **Carboneros** (caja nido con agujero de 28 mm) | Una pareja con pollos se come miles de orugas: menos daño en los frutales | Mols y Visser 2002, *J. Appl. Ecol.* 39:888–899 |

### 2. ¿Tierra o macetas? Tierra, en bancales por gremios

| | Maceta | Bancal de tierra (cubierta ajardinada intensiva) |
|---|---|---|
| Raíces | Volumen pequeño; sufren los extremos de temperatura del recipiente | Volumen continuo; temperatura y humedad amortiguadas (Mathers et al. 2007, *HortTechnology* 17:151–162) |
| Agua | Se seca antes; más riego | La tierra guarda más agua: menos riego |
| Vida del suelo | Casi ninguna | Lombrices, micorrizas y los microbios del compost |
| Compañeras e insectos | No caben | Franja de flores y aromáticas, hotel de insectos, abejorros: el hábitat de los enemigos naturales |
| pH | Uno por planta (lo mejor de la maceta) | Uno por bancal: **las frutales se agrupan por pH** (gremios) |
| Enfermedades del suelo (*Phytophthora*) | Aislada en su maceta | Se reparte: hace falta drenaje de verdad (capa de drenaje y filtro) |
| Peso | Concentrado | Repartido; el sustrato ligero saturado pesa 1,0–1,4 t/m³ (FLL 2018, *Green Roof Guidelines*; Dunnett y Kingsbury 2008, *Planting Green Roofs and Living Walls*) |

**Decisión**: en la **cima** de cada árbol, **bancales de tierra**: cubierta intensiva con lámina antirraíces, capa de drenaje, filtro
geotextil y **0,8 m** de sustrato ligero (lo que pide un frutal enano). Miden **1,4 m de ancho** (se llega al centro desde los dos
lados) y de 3,2 a 6 m de largo. Cada bancal es un **GREMIO**: frutales que comparten pH y riego, con sus compañeras, flores, insectos y
animales.

Las **puntas** siguen con sus macetas: una de cada especie por edificio, y ahí la maceta da a cada una su pH y su sustrato. Los
techos de la Central tampoco cambian.

El peso es despreciable para el tronco: unas 6–10 t por techo, menos de 0,1 MN frente a los ~174 MN que ya carga.

| Gremio | pH | Frutales | Compañeras (del juego) | Flores | Insectos útiles | Animales |
|---|---|---|---|---|---|---|
| Cítricos con su cortejo | 6–7 | limón, lima, naranja | romero, cilantro, haba | alyssum, caléndula, consuelda | *Cryptolaemus*, *Aphytis*, sírfidos y crisopas, abejorros | lombrices, caja nido |
| Andino de semisombra | 5,5–6,5 | banana (la sombra), café, naranjilla | huacatay (tagetes), haba, cedrón | tagetes, capuchina | *Aphidius*, *Orius*, mariquitas, abejorros | lombrices, micorrizas |
| Bayas de suelo ácido | 4,5–5,5 | mortiño, frutilla, piña | cebollín, albahaca | borraja, alyssum | *Phytoseiulus*, *A. swirskii*, abejorros (vibración) | lombrices |
| Mediterráneo seco | 6,5–8 | olivo, almendro | romero, arveja, perejil | facelia, milenrama, tomillo | *Osmia*, abejorros, crisopas, mariquitas | bloque nido de *Osmia*, caja nido |
| Trópico húmedo | 6–7 | cacao, babaco, guanábana | jengibre, cúrcuma, frijol | montón de pseudotallo de banana (cría de *Forcipomyia*) | *Forcipomyia*, *Cryptolaemus*, abejorros | lombrices, micorrizas |

### 3. Construcción

`PlaceTreeTopGardens` va al final de `PlaceRooftopOrchards`. Recorre el techo de cada árbol (`ArbolCampos.top`, 22 × 22 m), también
el del Árbol de los Campos, que gira: todo se mide en el marco del techo.
1. Aparta lo que ya hay: la columna y la puerta del techo, **su umbral** (de la puerta a 2 m más allá de su punto de fuera), los
   paneles, el cartel y los percheros de máscaras. Las áreas de alfombras, a ≥ 2 m.
2. Coloca bancales del más largo al más corto, cada uno en el hueco más ajustado y con su caja ENTERA (marco, plantas y copas) a ≥ 2 m
   de todo, hasta que no cabe ninguno.
3. Cada techo empieza por un gremio distinto: entre todos los techos salen todos los gremios.

Cada bancal lleva:
- marco de tablones, tierra y el acolchado de su gremio, y **el corte de sus capas** en una punta;
- sus frutales, uno cada ~2 m (`FruitPlant` con `bed`), con un sobre de su insecto útil colgado;
- las compañeras de `GardenCrop` en los bordes y la franja de flores de néctar;
- el **hotel de insectos**, la **caja de abejorros**, la **caja nido** de carbonero y el **vermicompostador**;
- su **goteo**: cabezal en la punta, lateral sobre la tierra y un gotero por frutal.

Es un mueble (guarda los 2 m de paso).

**En la simulación** (`FruitPlant.Tick` con `bed`; [A] aproximación de juego, en el sentido de las fuentes), una planta en tierra:
- gasta el agua al **60 %** (`waterFactor`);
- gasta el abono al **70 %** (`nutrientFactor`: compost, acolchado y leguminosa);
- coge la plaga a la **mitad** de ritmo (`pestFactor`: los enemigos naturales);
- pierde el cuajado a la **mitad** de ritmo sin pincel (`pollinationFactor`: abejorros y flores).

Su etiqueta dice «· bancal».

### 4. La jornada

Los frutales de la cima son `FruitPlant` de su edificio: entran solos en `FruitGardener.Rounds`. El techo es lo más alto del árbol, así
que `TipOrder` los pone **primero**: se llega en alfombra al techo, se cuida la cima y luego se nada a las puntas. Quien los revisa se
pone en el lado largo del bancal (`FruitPlant.StandPoint`), no encima.

Cada edificio suma la revisión de sus frutales de la cima (1 min un árbol, 0,75 un arbusto, 0,5 una herbácea) y unos metros por el
techo. El Árbol de los Campos pasa a tener frutales (los de su cima), además de sus tareas del árbol (máscaras y fitoplancton).

## Frutales en las residencias del santuario (2026-11-03)

Berón: «los frutales en residencias que te pedí era para las residencias del santuario». Cada **casa de árbol** de la Arboleda
residencial (`ResidentialBuilder`, 18 casas: dos o tres por árbol, con la tercera del horario) lleva una **terraza frutal**:

- una plataforma de **7 × 6 m** pegada a la habitación por el lado **contrario al tronco** (`Terraza_frutal`), a la altura de su suelo;
- en su centro, **un frutal en maceta** (`SampleSceneBuilder.PottedFruit`, la misma maceta de los techos, con su `FruitPlant`), de las
  especies de **porte pequeño** (una distinta en cada casa, rotando por `FruitCare.All`);
- a **≥ 2 m** de la cama, el armario, la mesilla y el área de su alfombra (la maceta queda a 5,5 m del borde de la habitación);
- las ramas del árbol esquivan la terraza (`ResidentialBuilder.Built`).

Son macetas del **edificio «Residencia»**: `FruitGardener.Rounds` las mete solas en la ronda del jardinero (una unidad más del camino,
sin punto de cuidados ni goteo: se riegan y abonan a mano en la revisión). `FruitOrchardTest` (grupo 55) comprueba que cada casa tiene
su frutal y que está a ≥ 2 m de sus muebles; las comprobaciones de los techos (una de cada especie, goteo, punto de cuidados) no se
aplican a la Residencia.

Los **microcosmos** conservan sus frutales en cada residencia (regla 11 de
[microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md)): encajan con su historia (el manzano de Woolsthorpe, el cacao de
Palenque).

## Rondas que caben en 3 h con las tareas de la semana (2026-11-04)

Lo midió el probador (grupo 55): con las ~200 revisiones de máscaras de los árboles, la jornada del jardinero salía en **36,5 h →
12 rondas**, tres de ellas de 4,6–5,5 h, y sin tiempo para polinizar ni podar (0 y 0). Tenía dos causas:
- las tareas de un árbol eran **una sola unidad**, que no se partía;
- las tareas de la semana solo se hacían con «lo que sobrara».

Cambios en `FruitGardener.Rounds`:
- **Las tareas de la semana se reservan.** Cada maceta cuenta en su ronda su revisión y, además, la media diaria de lo suyo
  (`WeeklyHoursPerDay`): 1 min de pincel cada 2 días si se poliniza a mano y 3 min de poda cada `PodaDias`, ×1,5 por el paseo. Así
  toda ronda deja sitio para polinizar y podar.
- **Las tareas de un árbol van en tramos** que caben en una ronda con su vuelo. Las puertas van de arriba abajo mientras quepan; en
  cada tramo se entra por el techo y se sale por el suelo. El fitoplancton va en el último tramo, o en uno suyo si no cabe.
- **El reparto llena primero.** Cada tramo va a la primera ronda donde cabe (con preferencia por seguir en su mismo edificio). Solo si
  no cabe en ninguna se abre otra ronda: otro jardinero ese día.

**Ninguna ronda pasa de 2,9 h.** El número de jardineros sale solo del trabajo total. Cuando el hilo de los árboles abarate la
revisión de máscaras (rotación semanal o mirar el indicador), bajará solo.

La prueba 55 pide ahora:
- las macetas de un edificio, sin partir;
- todas las puertas de cada árbol, en algún tramo;
- rondas bien llenas (no más de total ÷ (0,6 × 2,9 h));
- ninguna ronda de más de 3 h.

En la prueba del goteo, la planta de control es ahora una **maceta**. En un bancal el abono y la plaga van más despacio (es lo que hace
el bancal), y por eso la salud no bajaba.

**2026-11-04: tareas del árbol baratas.** Revisar a nado todas las puertas cada día salía a ~200 revisiones (36,5 h de rondas). Ahora el
jardinero lee el panel de máscaras en la puerta del suelo y revisa a mano solo 1/7 de las estaciones (rotación semanal) y las que su luz
marca en rojo; las cortinas de fitoplancton, 2 min cada una junto a la puerta del suelo. ~0,4–0,7 h por árbol. Detalle en
[mascaras-de-oxigeno.md](mascaras-de-oxigeno.md) §El jardinero.

**Ronda 2 del probador (2026-11-04).** Dos cambios más:
- **El reparto no parte las macetas de un edificio.** Con la reserva de la semana sumada a cada maceta, Servicios, Ciencias y la
  Central pasaban de 2,9 h y se partían. Ahora las macetas de un edificio son UN tramo (solo su revisión y su paseo) y la reserva va
  aparte (`Leg.weekly`): entera si cabe en la ronda y, si el edificio llena la ronda él solo, lo que quede hasta las 2,9 h
  (`Leg.reserve`).
- **El jardinero usa esa reserva aunque la revisión vaya lenta** (`StartExtras`: lo reservado como mínimo). Antes, con las tareas
  del árbol detrás, no le quedaba nada. La prueba 55 elige para el jardinero una ronda con macetas que se polinizan o se podan.

**Ronda 3 (2026-11-04): las residencias.** Las macetas de las terrazas de las casas del árbol (ed774ff) se recorren **en alfombra**,
casa a casa. `Hop` cuenta un vuelo (`FlightSeconds`) entre dos casas distintas: antes contaba la recta andando y la ronda de la
Residencia no acababa en su tiempo. La prueba 55 elige para su jardinero una ronda de techos.

**Ronda 5 (2026-11-04).**
- Las **18 casas del árbol** de la Residencia son casas sueltas, a un vuelo una de otra: se reparten en rondas por grupos de casas.
  La prueba 55 ya no las cuenta como un techo que no debe partirse.
- La maceta de control de la prueba del goteo es una **con goteo, de un techo**: las de las casas del árbol no llevan goteo, y el
  mortiño elegido se quedaba sin agua.
