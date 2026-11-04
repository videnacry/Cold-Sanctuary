# Los pisos de yoga: Hatha y Ashtanga, occidental y tradicional (2026-11-04)

Berón (2026-11-04): «en la sala de yoga retira el anillo de yoga, pues no creo que desde el anillo puedan ver ni interactuar con
el profesor; en su lugar vamos a crear otro piso de yoga, así también podemos dividir las clases y tener hatha en un piso y
ashtanga en otro. Debes investigar si dichos estilos tienen requisitos o estilos diferentes para crear pisos adaptados, ¿quizás
en la India se hace sobre la tierra?, ¿o siempre sobre suelo con esterillas? Debemos tener la versión más occidental y la versión
más conservadora, ambas con sus estatuas o elementos distintivos. En Isha Yoga Center me parece que tienen un monumento del
Dhyanalinga o algo así en el centro; ¿quizás lo ideal sería entrar en un río y después comenzar la práctica?»

Código: `Assets/Editor/SampleSceneBuilder.Yoga.cs` (los pisos), `Assets/Scripts/World/YogaHall.cs` (cada sala),
`Assets/Scripts/World/YogaClass.cs` (la clase y la inmersión), `Assets/Scripts/World/YogaStatues.cs` (linga, Adiyogi, Ganesha,
lámparas), `WorkdayDirector` (jornada), `LeisureDirector` (ocio). Prueba: `YogaFloorsTest` (grupo 64).

## 1. Investigación

### 1.1 El sitio de práctica en los textos clásicos
- **Hatha Yoga Pradipika 1.12–1.13**: el hatha yogui practica en un **matha** (ermita) pequeño y apartado, sin peligro de
  piedras, fuego ni agua; con una **puerta pequeña y sin ventanas**, y fuera un patio con su plataforma
  y su pozo, rodeado de un muro. (La numeración cambia de una edición a otra.) Fuentes: el texto sánscrito de
  [Dharmamitra (HYP 1.13)](https://dharmamitra.org/db/sa/SA_GSP34_hathyopu/text), la traducción de
  [Muktibodhananda](https://terebess.hu/english/Hatha-Yoga-Pradipika-Muktibodhananda.pdf) y la de
  [Dharmabindu](https://dharmabindu.com/conteudos/artigos/284/ensinamento_artigo_284.pdf).
  → Para el juego: **silencio y recogimiento**, nada que distraiga; el agua (el kund) va aparte de las esterillas, en la banda del
  tronco, como el pozo en el patio.
- **Bhagavad Gita 6.11**: el asiento, en un sitio limpio, ni muy alto ni muy bajo, de **hierba kusha, piel de ciervo y tela**,
  una capa sobre otra. Los comentaristas lo explican como aislamiento del suelo (la kusha), defensa contra bichos (la piel) y
  pureza ritual. Fuentes: [holy-bhagavad-gita.org 6.11](https://www.holy-bhagavad-gita.org/chapter/6/verse/11),
  [asitis.com 6.11–12](https://asitis.com/6/11-12.html), [bhagavad-gita.us](https://www.bhagavad-gita.us/?p=900).
  → Para el juego: el maestro de la sala tradicional se sienta sobre **kusha y algodón**. **Sin piel de ciervo**: es un santuario
  de animales (decisión nuestra, no del texto).
- **¿Tierra o esterilla?** En la tradición, sobre el suelo (tierra o piedra) con una estera de hierba o una alfombra de algodón
  (durrie); la esterilla de goma es moderna. En Ashtanga, la **alfombra de algodón de Mysore** sigue siendo la costumbre (absorbe
  el sudor y agarra más cuanto más mojada), a menudo encima de una esterilla. Fuente (tienda, con interés comercial):
  [Hugger Mugger, «Yoga Practice Rugs»](https://www.huggermugger.com/blog/2017/yoga-practice-rugs). No encontré una fuente
  histórica independiente de cuándo se generalizó.

### 1.2 Hatha frente a Ashtanga (Mysore)
- **Ashtanga, estilo Mysore**: cada alumno hace la **misma serie fija a su ritmo**, en silencio, y el maestro **recorre la sala**
  ajustando a cada uno; además hay clases **guiadas** (el maestro cuenta las respiraciones desde delante). Se suda mucho. Fuentes:
  [Kia Naddermier, Mysore Style](https://kia-naddermier-bphb.squarespace.com/practice/mysore-style),
  [UVA CSC, Ashtanga Mysore](https://csc.virginia.edu/events/ashtanga-yoga-mysore-style-sessions-mon-thu-fall-2025),
  [Yogi Times, Astanga Yoga London](https://www.yogitimes.com/review/astanga-yoga-london-studio-classes-euston-square)
  (sala principal de hasta 18 esterillas, casi en silencio).
  El shala de Pattabhi Jois empezó en una casa de dos habitaciones de Lakshmipuram (1948) y desde 2002–2003 está en Gokulam, Mysore,
  con hasta ~200 alumnos al día ([Wikipedia: K. Pattabhi Jois](https://en.wikipedia.org/wiki/K._Pattabhi_Jois),
  [AYI, padres fundadores](https://www.ashtangayoga.info/ashtangayoga/tradition/162013-the-founding-fathers-of-ashtanga-yoga-part-23/)).
  La práctica se abre con el canto a **Patanjali** (por eso su estatua en la sala tradicional de Ashtanga).
  **Sin verificar**: que Jois mandara bañarse antes de practicar. Hay quien lo cuenta como costumbre de Mysore, pero no lo
  encontré en *Yoga Mala*.
- **Hatha (Isha, clásico)**: los programas de Hatha de Isha (upa-yoga, angamardana, surya kriya, asanas, bhuta shuddhi) se
  practican en **esterilla** (piden traer la suya) y en un espacio consagrado para ello, el **Adiyogi Alayam**. Fuente:
  [Isha, Hatha Yoga Teacher Training](https://isha.sadhguru.org/sg/en/yoga-meditation/yoga-teacher-training/hatha-yoga-teacher-training-V0).
  → En las dos versiones hay esterilla o alfombra: la diferencia es el **material** (goma de colores o algodón) y el **suelo**
  (madera o tierra).
- **Orientación y luz**: el texto pide recogimiento, no una orientación. No encontré una fuente seria que imponga el este o el norte
  a la sala: **no lo usamos como regla**. Luz: natural de día (sin techos, regla del santuario) y, en la tradicional, **lámparas
  de aceite**.
- **Calor**: el Ashtanga calienta el cuerpo (se suda); Mysore es cálido. Las temperaturas de `YogaHall.roomTempC` (Hatha 22 °C
  occidental y 25 °C tradicional; Ashtanga 26 °C y 28 °C) son **valores de diseño**, no medidas de una fuente.

### 1.3 El Isha Yoga Center (Coimbatore)
- **Dhyanalinga**: un linga de **granito negro de 13 pies y 9 pulgadas** (≈ 4,2 m), de una sola piedra, que sale del **avudaiyar**,
  un receptáculo en forma de **serpiente de siete vueltas**; bajo una cúpula elíptica **sin columnas** de 76 pies de diámetro y
  33 de alto, hecha con unos 250 000 ladrillos; del techo de cobre gotea agua sobre él. Es un espacio para **meditar en silencio**,
  no de culto. Fuentes: [Isha, el sanctasanctórum](https://isha.sadhguru.org/en/sadhguru/mystic/dhyanalinga-architecture-sanctum-sanctorum),
  [Isha, Dhyanalinga](https://isha.sadhguru.org/dhyanalinga), [Shubh Panchang](https://shubhpanchang.com/temples/adiyogi)
  (una web de viajes da 9 pies y 3 pulgadas: vale la cifra oficial).
- **Theerthakund**: dos estanques consagrados bajo tierra, el **Suryakund** (hombres, a unos 20 pies, bajo un sol de latón y cobre)
  y el **Chandrakund** (mujeres, a unos 30 pies, por 32 escalones). Se bañan **antes de entrar en el Dhyanalinga**, para estar más
  receptivos; un testimonio habla de agua «helada». Sadhguru lo enlaza con la costumbre india de mojarse entero antes de entrar en
  un templo. Fuentes: [Isha, Theerthakund](https://isha.sadhguru.org/in/en/center/consecrated-spaces/theerthakund),
  [Isha, guía del centro](https://isha.sadhguru.org/blog/article/everything-you-need-to-know-about-the-isha-yoga-center-and-more),
  [extractos de charlas de Sadhguru](https://ayogisguide2.blogspot.com/2018/11/excerpts-from-sadhgurus-talks-question.html).
- **Adiyogi**: busto de **34 m (112 pies)**, de unas 500 toneladas de acero, «el busto más grande» según Guinness; el 112 son las
  112 vías que Adiyogi dio para explorar el sistema humano. Fuentes: [Wikipedia: Adiyogi Shiva bust](https://en.wikipedia.org/wiki/Adiyogi_Shiva_bust),
  [Isha, 12 cosas del Adiyogi de 112 pies](https://isha.sadhguru.org/en/blog/article/12-things-about-112-ft-adiyogi).
- **¿Entrar en un río y luego practicar?** Bañarse antes de entrar en un templo es costumbre india (los ghats de Rishikesh y
  Haridwar son sitios de baño ritual; [Wikipedia: Triveni Ghat](https://en.wikipedia.org/wiki/Triveni_Ghat)). **No encontré** una
  fuente que haga del baño en el río un paso fijo **antes de la práctica de asanas**; sí lo es antes del Dhyanalinga en Isha.
  → En el juego, el baño previo va en la versión **tradicional** (el kund), y en la occidental no.

### 1.4 El estudio occidental típico
- **Suelo de madera**, esterillas de goma, **accesorios** (bloques, cinturones, bolsters, mantas) y decoración de origen indio o
  budista (Buda, Ganesha, plantas).
- **Espejos**: una tesis de la Universidad de Minnesota (2015) halló que las clases con espejo daban **más comparación por el
  aspecto** y recomienda que los alumnos no los miren y atiendan a cómo se siente la postura; un ensayo de Yoga Journal cuenta
  que el espejo lleva la atención hacia fuera. Fuentes: [UMN Conservancy, «The Effect of Mirrors…»](https://conservancy.umn.edu/items/c35d7578-9807-46f1-a88e-0751ae306ed2),
  [Yoga Journal, «Mirror, Mirror, Why Are You Here?»](https://www.yogajournal.com/uncategorized/mirror-mirror-why-are-you-here/?scope=anon).
  → **Sin pared de espejo** en ninguna sala: (1) la evidencia apunta a que distrae de la práctica interior; (2) en el Ashtanga la
  mirada va a su drishti, no a un espejo; (3) el santuario no tiene muros (la única pared permitida es la de cristal, para aislar).

## 2. Decisión: dos pisos con dos salas cada uno (no cuatro pisos)

| | 2 pisos × 2 salas (elegido) | 4 pisos |
|---|---|---|
| Lo que pidió Berón | «crear **otro** piso»: Hatha en uno y Ashtanga en otro | cuatro pisos nuevos |
| Torre del Entrenamiento | de 3 a **4 niveles**: 7 pisos y 1 libre | 5 niveles: 9 pisos y 1 libre (+44 m de alto) |
| Holgura con los vecinos (simulador `mig`, ≥ 16 m) | **sin cambios**: Árbol–Entrenamiento 16,3 m, Servicios–Entrenamiento 16,5 m | no simulado |
| Sitio por sala | 18 × 26 m: 8 esterillas de 3 × 3,6 m a ≥ 2 m, tarima y bandas de 4 m en cada extremo | 36 × 26 m: sobra |
| Clases | una clase por **estilo** y día; la versión (sala) cambia por día | igual, con el doble de nado entre pisos |

Una sala por versión dentro del piso de su estilo deja juntos al maestro y a los alumnos de un estilo, y la torre solo crece un
nivel. El piso libre (P14) queda para lo que venga.

## 3. Cómo es cada piso (planta de 36 × 26 m; el tronco, por −Z)

```
           +Z (cabecera)                               +Z
  ┌──────────────── OCCIDENTAL (−X) ──┬── TRADICIONAL (+X) ───────────────┐
  │ Aseos   Buda/Ganesha   Estante    │  (Patanjali)  Linga/Adiyogi        │
  │                                   │       lámparas de aceite en arco  │
  │      E   E(0,+5,1)  E             │       E    E    E                 │
  │      E    [TARIMA]  E             │       E [TARIMA] E   suelo de      │
  │      E   E(0,−5,1)  E   madera    │       E    E    E    tierra       │
  │                                   │   canal ── KUND (6 × 3, agua 0,9) │
  └───────────────────────────────────┴───────────────────────────────────┘
           −Z (tronco: su puerta, donde quede libre)
```
- **Las dos salas**: la tarima del maestro (2,6 × 2,6 × 0,4 m) en el centro de cada mitad y **8 esterillas en un solo anillo**
  (6 a los lados, a 5,5 m, y 2 delante y detrás, a 5,1 m), cada una mirando a la tarima. Nadie queda detrás de otro alumno.
- **Occidental**: suelo de madera clara con sus juntas, esterillas de colores (las alfombras-esterilla), esterilla del maestro, el
  **estante de accesorios** (Hatha: bloques de corcho, cinturones, bolsters y mantas; Ashtanga: alfombras de Mysore y toallas) y
  la estatua (**Buda** en Hatha, **Ganesha** en Ashtanga). Sin espejo.
- **Tradicional**: suelo de **tierra apisonada**, **alfombras de algodón** (crudo con franjas índigo), el asiento del maestro de
  **kusha y algodón**, **lámparas de aceite** en arco con su luz cálida, y su elemento: en Hatha, un **linga como el Dhyanalinga**
  (piedra negra sobre el avudaiyar de siete vueltas de cobre); en Ashtanga, **Patanjali** y el **busto de Adiyogi**.
- **El kund** (solo en las tradicionales): un estanque de 6 × 3 m con 0,9 m de agua en la banda del tronco. Su agua viene del
  **tronco del árbol** por un canal en el suelo y la sujeta el **campo magnético** entre cuatro bobinas de cobre, como en las
  puertas del árbol: se entra andando, **sin escalones** (regla del santuario). Es un mueble para los 2 m y el tanque de
  virtualización, pero su caja es de disparo: se cruza.
- **Pisos de la torre**: P1 Gimnasio · P2 Cardio · P5 Calistenia · **P6 Hatha (Sala de Yoga)** · **P9 Ashtanga** (`Planta_ashtanga`)
  · P10 Ballet · P13 Dojo · P14 libre.

## 4. Líneas de visión al maestro (`YogaHall.BlockedSightLines`)
De los ojos de cada alumno **sentado** (0,8 m sobre su esterilla) a los del maestro **de pie** en la tarima (0,4 + 1,55 m): nada
del piso en medio (muebles, estatuas, estante, bobinas; no cuentan las esterillas, los suelos, el agua ni las personas) y a
≤ 9 m. Con las esterillas a 3,3–7,3 m del centro, la más lejana queda a ~7,5 m; todo lo alto va en las bandas de los extremos
(a 9 m o más del centro en Z), fuera de cualquier línea. El constructor lo registra (`[Yoga] … esterillas ven al maestro`) y la
prueba lo pide a 0.

## 5. Las clases
- **Hatha** (la clase guiada, misión `yog_guia` con su demostrador y sus dos correctores; `WorkdayDirector.HathaSession` =
  Saludo al Sol): en el piso de Hatha.
- **Ashtanga** (el entrenamiento de las 14 h; `AshtangaSession` = Ashtanga Primaria): en el piso de Ashtanga. La da el primero que
  llega; los demás se unen.
- **La sala del día** (`YogaHall.VersionOn`): la **tradicional los días pares** y la **occidental los impares**. Puede haber una
  clase por sala a la vez (`YogaClass.On`); un alumno solo está en una.
- **La inmersión** (tradicional): cada practicante anda a su sitio en el kund, se sienta en el agua 8 s (`YogaHall.ImmersionSeconds`)
  y sale de pie hacia su esterilla; el maestro espera a que lleguen (la espera crece con el kund). Si no llega al kund a tiempo
  (viaje × 1,3 + 60 s), va a su esterilla sin bañarse y lo dice en el registro. Cada inmersión queda en `YogaClass.Immersions`.
- **Sadhguru** medita en su tiempo libre en la tarima de la sala de Hatha tradicional, junto al linga (si hay clase en ella, en la
  occidental).
- **Ocio** (`LeisureDirector.SanctuaryActivities`): «Hatha libre y meditación junto al linga» (sala de Hatha tradicional) y
  «Ashtanga autoguiado (estilo Mysore)» (sala de Ashtanga occidental); cada plaza es una esterilla de esa sala.
- La **primera clase de upa-yoga** del recién llegado (`AreaInitiator`, con la Magnate) sigue en el punto `SalaYoga` del piso de
  Hatha: el centro del piso, entre las dos salas (3,4 m libres).

## 6. Reglas (también en el [manual de diseño](manual-de-diseno.md), §2 F7–F12)
1. **Sin anillo de yoga**: desde un anillo no se ve ni se trata al maestro.
2. **Un piso por estilo** (Hatha, Ashtanga) y en cada uno **una sala occidental y una tradicional**.
3. **El maestro, visible desde cada esterilla**: tarima en el centro, esterillas en un solo anillo, lo alto fuera de las líneas
   de visión, ≤ 9 m.
4. **Tradicional**: suelo de tierra, alfombras de algodón, asiento de kusha (sin piel animal), lámparas de aceite, su elemento
   (linga como el Dhyanalinga en Hatha; Adiyogi y Patanjali en Ashtanga) y un **kund para sumergirse antes de practicar**, con
   agua del tronco sujeta por el campo magnético (sin escalones).
5. **Occidental**: suelo de madera, esterillas, accesorios, Buda o Ganesha; **sin pared de espejo**.
6. Las reglas de siempre: 2 m entre muebles, sin escaleras, esterillas de su propia red (no vuelan a otros pisos).

## 7. Pendiente y riesgos
- No probado en vivo (lo prueba el probador): reconstruir la torre del Entrenamiento y mirar las fotos.
- La clase de Ashtanga Primaria es larga: en el bloque de entrenamiento puede no acabar (al cambiar de bloque, el maestro la deja).
- El microcosmos de la Sala de Yoga (el río de Sakshi) es solo el piso de Hatha: su planta de Ashtanga ya no se copia
  (`AreaMicrocosmos` quita las plantas propias del área; vale también para el hospital).
