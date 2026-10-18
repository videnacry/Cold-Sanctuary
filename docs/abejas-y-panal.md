# Abejas de las nieves y su panal

Las abejas de las nieves (20 cm) trabajan su panal como lo hacen las abejas melíferas reales (*Apis mellifera*).
Código:
- `Assets/Scripts/World/Hive.cs`: las celdillas y sus estados.
- `Assets/Scripts/World/IceInsectLife.cs`: la plantilla de la abeja como anima compuesta, con su oficio según la edad.

## Cómo es en la vida real

- **El panal.** Son láminas de cera paralelas que cuelgan, con celdillas hexagonales a ambos lados, separadas por el
  "paso de abeja" (~1 cm en la abeja real).
- **Dónde va cada cosa.** La miel se guarda arriba, en arco; el polen, alrededor de la cría; y la cría, en el centro y
  abajo.
- **La reina** pone un huevo en cada celdilla limpia.
- **Desarrollo de una obrera.** Huevo 3 días → larva 6 días (alimentada por las nodrizas) → la celdilla se tapa (cría
  operculada) → pupa 12 días → **nace a los 21 días**. La celdilla queda con la muda y una limpiadora la deja lista.
- **Oficio por edad** (polietismo):

  | Edad (días) | Oficio | Qué hace |
  |---|---|---|
  | 0–3 | Limpiadora | Limpia las celdillas donde ha nacido una abeja. |
  | 3–10 | Nodriza | Alimenta a las larvas con jalea y pan de abeja (miel + polen) y tapa la celdilla al terminar la fase de larva. |
  | 10–18 | Procesadora / cerera | Recibe el néctar, lo abanica hasta que pierde el agua (≈ 18 %) y lo vuelve miel, y la tapa con cera. |
  | 18–21 | Guardiana | Vigila la piquera y planta cara a los intrusos. |
  | 21+ | Pecoreadora | Sale a por néctar y polen. Al volver hace la **danza del contoneo** sobre el panal: el ángulo del tramo recto respecto a la vertical es la dirección de la fuente respecto al sol, y su duración, la distancia. Después descarga el néctar. |

- **Frío y noche.** Las abejas forman un **racimo** sobre el panal y tiritan con los músculos del vuelo para mantener la
  cría a ~35 °C, gastando miel.

## En el juego

- Cada *Nido_Abejas* tiene una **Hive** con sus celdillas.
  - El color de cada celdilla muestra su estado: vacía (oscura), huevo y larva (blanco), cría operculada (parda),
    néctar (ámbar), miel operculada (cera clara), polen (naranja) y sucia (parda oscura).
  - Empieza con el arco de miel arriba y el polen debajo.
- Un **día de abeja** dura 20 s de juego (`Hive.secondsPerDay`).
- **Las abejas de cada colonia:**
  - La primera es la **reina** (más larga) y las demás, obreras de edades repartidas.
  - Al crecer cambian de oficio solas.
  - Cuando nace una abeja se añade a la colonia, hasta un máximo de 12.
- **Fuentes de néctar** de las pecoreadoras:
  - flores del sotobosque al pie de otros árboles, a 6–18 m;
  - un **cadáver** junto al árbol carnívoro: los nutrientes del muerto (el bucle de nutrientes del ecosistema de
    hielo; ver [ice-sanctuary-ecology.md](ice-sanctuary-ecology.md)).
- **La araña de las nieves** espera en su tela junto al panal (ver abajo). Las obreras huyen y la guardiana le planta cara.

Prueba automática: `SanctuaryLifeTest` (la reina pone huevos en el panal).

## Arañas de las nieves y su tela

La ecología del santuario ([ice-sanctuary-ecology.md](ice-sanctuary-ecology.md)) solo decía que la araña "caza abejas
cerca del árbol"; no hablaba de telas. Ahora es una **tejedora orbicular**, como las *Araneidae*:

- Tiende una tela de radios con espiral pegajosa (`SpiderWeb`) junto al panal, por donde pasan las abejas, y espera en
  el centro.
- La abeja que cruza la tela queda pegada unos segundos, agitándose. La araña va a por ella primero; si no, caza a la
  que pase a su alcance.
- Antes acechaba "en el tronco", en un punto que quedaba dentro del tronco. Abejas y arañas aparecían dentro del árbol:
  ahora la araña nace en su tela y las abejas, en su panal.
