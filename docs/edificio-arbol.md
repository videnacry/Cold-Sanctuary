# El Árbol de los Campos (propuesta, 2026-10-29)

Idea de Berón: juntar los campos en un solo edificio con forma de árbol.
- **Raíz:** una columna central muy fuerte de unos 100 m.
- **Tronco:** pisos de cultivo inclinados, cada uno al revés que el de arriba (un acordeón).
- **Ramas:** columnas hijas inclinadas que nacen de la raíz.
- **Flores o frutos:** en la punta de cada rama, un anillo de nidos como los del hospital y el restaurante.
- **Giro:** la raíz gira y con ella gira todo el árbol, para repartir sol, sombra y lluvia.

## Respuestas

**¿Hace falta que los campos estén separados?** No. Hoy son seis parcelas sueltas a ras de suelo: Hortalizas, Milpa, Chacra andina, Arrozal, Bosque comestible y Bambusal, más la Rotación. Suman unos 2 300 m². Un piso octogonal de 32 m tiene unos 850 m² útiles, así que caben en **3 o 4 pisos** y el suelo vuelve a la naturaleza. Es la misma idea de las torres.

**¿Cómo se ve desde fuera?** Como un **zigzag**: un acordeón visto de lado. Si además cada piso se gira 45° respecto al de abajo (los pisos son octógonos), las esquinas de cada uno asoman bajo las caras del siguiente. Desde lejos se lee como un **muelle cuadrado retorcido**. Propongo esa variante: deja entrar más luz lateral y queda más orgánica.

**¿Un giro por hora del juego?** Un día del juego dura 1 hora real, así que una hora del juego son 2,5 minutos reales. Un giro por hora del juego son 2,4° por segundo: se nota a simple vista, como un tiovivo lento. Es demasiado rápido para quien camina o aterriza en alfombra sobre un piso.

Hay un detalle de nuestra latitud (−77,85°, McMurdo). En verano el sol da la vuelta completa al horizonte cada día, bajo (como mucho unos 35°). Con eso, cada cara del árbol ya recibe sol una vez al día sin girar nada. El giro sirve para:
1. Desfasar la sombra que las ramas y los anillos proyectan sobre los pisos de abajo, para que no caiga siempre sobre los mismos cultivos.
2. Repartir el viento y la nieve que traen las tormentas.

**Propuesta:** girar en sentido **contrario al sol**, por defecto **una vuelta cada 6 horas del juego**, unos 15 minutos reales. Es casi imperceptible para quien camina y cada cara recibe el sol cuatro veces al día. Se puede cambiar en juego con el hechizo `arbol({vueltaHoras: 1})`, y `0` lo para. En la prueba en vivo probaré 1, 3, 6 y 24.

## Estructura

| Parte | Medidas | Notas |
|---|---|---|
| **Columna raíz** | 100 m de alto, Ø 4 m, CFST como las torres | Sostiene todo, así que es la más gruesa. `StructuralLimits` le dará su propio aviso y su propia prueba. |
| **Tronco (acordeón)** | 8 pisos de cultivo cada 7 m (de 6 a 62 m) | Octógono de 32 m. Inclinación de 3° alterna (+3°, −3°…) para que el agua corra hacia el borde bajo y caiga al piso de abajo por una canaleta, y de ahí al siguiente: **riego en cascada**. |
| **Ramas** | 6 columnas hijas desde 66 m, inclinadas 35°, de 28 m | Salen de la raíz y sobresalen más allá del borde de los pisos, como pediste. |
| **Flores** | 6 anillos de nidos, Ø 14 m, en la punta de cada rama | Frutales enanos, flores para las abejas de nieve y nidos de aves. Su sombra cae fuera del tronco porque sobresalen. |
| **Copa** | Mirador y estación del clima en lo alto de la raíz | Opcional. |

**Reparto propuesto, de abajo arriba:**
- Lo que más pesa y más agua necesita va abajo: P1 Arrozal y P2 Bambusal.
- Luego P3 Milpa, P4 Chacra andina, P5 Hortalizas y P6 Rotación.
- Arriba, P7 y P8 Bosque comestible, porque sus árboles necesitan más altura y aguantan más viento.

El **arrozal es la excepción a la inclinación**: va en terrazas planas dentro de su piso, porque el arroz se inunda. El agua sobrante pasa al resto de la cascada.

**Acceso:** sin escaleras, por alfombras voladoras a cada piso y a cada anillo, como mandan las reglas. Cada piso conserva los 2 m entre muebles y sus huecos reservados para flora, fauna, personajes e historia.

## Riesgo técnico

Si el árbol gira, los pisos se mueven. El NavMesh y las alfombras están hechos para un mundo quieto. La solución es darle al árbol su propio NavMesh, que gire con él, y que las alfombras aterricen en puntos que viajan con su piso. Lo pruebo en vivo antes de darlo por bueno; si da problemas, el árbol se queda quieto (vuelta 0) hasta arreglarlo.

## Dónde

En el sitio de la torre de **Producción** (casquete Sur), que hoy contiene el huerto, la rotación, la construcción, la mecánica y el taller textil. Los dos primeros se mudan al árbol. Construcción, mecánica y textil se quedan en una torre de Producción más baja al lado.
