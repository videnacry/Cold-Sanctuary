# La forma del cuerpo a partir de las stats físicas

Cómo `BodyShape` convierte fuerza, peso, agilidad y resistencia en la silueta del cuerpo de cubos (humanos y compañeros).

## Lo que dice la anatomía

- **El peso del cuerpo está repartido por segmentos.** En las tablas antropométricas clásicas (Dempster; Winter, *Biomechanics and Motor Control of Human Movement*), en un adulto típico:
  - el tronco es alrededor del 50 %;
  - cada muslo, ~10 %;
  - cada pierna (rodilla a tobillo), ~4,7 %;
  - cada pie, ~1,5 %;
  - cada brazo, ~2,8 %;
  - cada antebrazo, ~1,6 %;
  - cada mano, ~0,6 %;
  - cabeza y cuello, ~8 %.

  Por eso el tronco y los muslos son lo que más cambia de volumen cuando cambia el peso.
- **El músculo crece donde se entrena** (hipertrofia específica). Velocistas y saltadores tienen muslos y glúteos grandes. Escaladores, lanzadores y remeros tienen hombros, espalda y brazos grandes. Los fondistas tienen extremidades finas y fibrosas.
- **La grasa no se acumula en el miembro entrenado.** La «reducción localizada» es un mito: entrenar un miembro no le quita grasa a ese miembro. Dónde se deposita la grasa depende del patrón de cada persona:
  - *androide*: abdomen y tronco;
  - *ginoide*: caderas, glúteos y muslos.

  En ambos casos hay poca en antebrazos, pantorrillas, manos y pies. Lo que se ve más grande en un miembro trabajado es músculo.
- **La estatura explica peso y fuerza a la vez.** El peso crece aproximadamente con la altura al cubo y la fuerza con la sección del músculo (altura al cuadrado). Una persona alta puede ser pesada, fuerte y aún ágil sin ser «gruesa».

## Cómo lo aplica el juego

`BodyShape` reparte el peso (`bodyMass`) en tres componentes:

| Componente | De dónde sale | Qué cambia en el cuerpo de cubos |
|---|---|---|
| **Armazón (estatura)** | El peso que sostienen juntas la fuerza y la agilidad: `min(peso, (fuerza+agilidad)/2)` | Escala del cuerpo entero, hasta ±12 % |
| **Músculo** | La fuerza por encima de lo que trae el armazón | Grosor de los segmentos, más en los grupos entrenados |
| **Grasa** | El peso que no explican ni el armazón ni el músculo, dividido por la resistencia | Grosor (más en el fondo, la «tripa») según el reparto androide/ginoide |

**Dónde va el músculo:**
- Si el ser tiene regiones entrenadas (`PlayerStats.bodyStats`, las del yoga), el músculo va donde están:
  - hombros, codos y manos → brazos;
  - caderas, rodillas y pies → piernas;
  - espalda → tronco.
- Además, la **agilidad** desvía el músculo hacia las piernas (correr y saltar), y la **fuerza sin agilidad** hacia hombros y brazos.
- La **resistencia** lo hace fibroso: menos grosor por la misma fuerza.

**Reparto de la grasa por segmento:**

| Segmento | Peso |
|---|---|
| Tronco y espalda | 1,0 |
| Cadera | 0,9 |
| Muslos | 0,7 |
| Brazos | 0,45 |
| Cuello | 0,35 |
| Antebrazos y piernas | 0,2 |
| Manos y pies | 0,05 |

**Ejemplos:**
- Pesado, fuerte y ágil → alto y con piernas grandes; poca grasa.
- Pesado y fuerte, poco ágil → hombros y brazos grandes.
- Pesado y débil → tronco, cadera y muslos más anchos (grasa).
- Ligero y resistente → fino y fibroso.

**Límites:**
- La Magnate queda fuera: sus stats «infinitas» vienen de sus células, no de su musculatura.
- Los animales mantienen las proporciones de su especie: sus stats se miden en otra escala.
- El cuerpo se recalcula cada 2 s y cambia despacio, así que entrenar o engordar se va notando poco a poco.
