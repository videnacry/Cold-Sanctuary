# Medidas de muebles frente al humano más alto

Revisión del 2026-10-27, hecha a raíz de la camilla: el jugador tumbado en ella no cabía y quedaba descolocado.
Este documento solo mide y propone; todavía no se ha cambiado nada en el código.

## Humano de referencia: 1,85 m

- **Estaturas declaradas** (`Assets/Scripts/Avatar/CubeHair.cs:87-122`, `docs/diseno-personajes.md`): Andrea 1,78 ·
  Gohageneis 1,76 · Goluis 1,75 · Kushal (jugador) 1,75 (`SampleSceneBuilder.cs:2155`) · Sadhguru 1,72 · Jefe de
  construcción 1,68 · Panterilia 1,68 · Magnate 1,66 · Linduarda 1,60 · Lizmibet 1,55 · Irosene 1,55.
- **`BodyShape` las cambia en juego** (`Assets/Scripts/Avatar/BodyShape.cs:82`): la escala del cuerpo es
  `1 + clamp(0,2·(armazón − 1), ±0,15)`. Con sus stats (`PoseGallery.cs:129`), Goluis (1,5 / 0,9 / 1,4 / 1,29) queda a
  ×1,04, es decir **≈ 1,82 m**, y es el más alto en juego. Gohageneis llega a ≈ 1,79 y Andrea se queda en 1,78.
  **El techo teórico es 2,05 m**: Andrea × 1,15, si sus stats crecieran.
- **La medida real en la escena** (`ScaleAudit.cs:36`, que toma los bounds del jugador con el pelo y el gorro) da
  «Humano de referencia: 1,85 m».
- **Qué se toma como referencia:** **1,85 m** de estatura. Equivale a ≈ p95–p97 de un varón adulto de EE. UU.
  (NHANES 2015-18: p95 = 187,5 cm). Los muebles deben dejar margen hasta ≈ 1,90 m.

**Proporciones del cuerpo de cubos** (`CubeHumanoid.cs:16,73-110`). Mide 1,9 unidades nativas; a 1,85 m la escala es
s = 0,974.

| Medida | Cuerpo de cubos (a 1,85 m) | Persona real de 1,85 m |
|---|---|---|
| Ancho de hombros con los brazos | 0,60 m | — |
| Ancho de cadera | 0,33 m | — |
| Tumbado, de la cadera a la coronilla | 0,935 m | — |
| Tumbado, de la cadera a los talones | 0,906 m | — |
| Largo total tumbado | **1,84 m** | — |
| Sentado: bajo el muslo hasta el suelo (poplíteo) | **0,38 m** | ≈ 0,47–0,50 m |

El cuerpo de cubos tiene la espinilla corta: 0,46 unidades de rodilla al suelo, es decir un 24 % de la estatura,
cuando en una persona real es ≈ 28,5 %. Por eso, en sillas reales de 0,45 m, los pies le cuelgan unos 7 cm.

**Datos antropométricos de referencia** (varón p95 de EE. UU., tablas de NC State / Gordon 1988):

| Medida | Valor p95 |
|---|---|
| Altura poplítea | 47 cm |
| Altura de rodilla sentado | 60 cm |
| Ancho de cadera sentado | 39 cm |
| De la nalga al hueco poplíteo | 55 cm |

## Tabla

Leyenda de la última columna: «**Sí**» hay que cambiarlo, «Sí (posición)» falla la colocación del cuerpo y no el
tamaño, «Opc.» es opcional y «No» está bien así.

| Mueble | Dónde se construye (file:line) | Medida actual (largo × ancho × alto) | Medida real recomendada (para 1,85 m) | Fuente | ¿Cambiar? |
|---|---|---|---|---|---|
| **Camilla de exploración** (y Camilla 2, Camilla 3, Camilla eco) | `Assets/Scripts/World/InfirmaryProps.cs:50-59` (Acolchado :54, Respaldo :55); se coloca en `SampleSceneBuilder.Hospital.cs:92,94,97,134` | Plano acolchado de 1,50 × 0,65 m, con la superficie a 0,82 m (0,83 con el papel). Respaldo de 0,45 m inclinado 30°. Base de 1,60 × 0,60 × 0,70 m | **Plano de 2,00 × 0,70 m** (sin respaldo, o con el respaldo plano para el ECG), con la superficie a 0,80 m | Camillas comerciales de 72 × 24 in a 30–32 in de alto ([Henry Schein](https://www.henryschein.com/us-es/medical/products/medical-equipment/about-medical-equipment/medical-examination-tables-stools.aspx), [School Health](https://www.schoolhealth.com/s-professional-treatment-table-24-w)); «típicamente 32 in» ([ADA National Network](https://AData.org/sites/adata.org/files/files/Medical%20Examination%20Tables%20and%20Chairs%2C%20LP%20final%202018.pdf)); europeas de 575–610 mm de ancho y bariátricas de 800 mm ([MedicalExpo](https://www.medicalexpo.com/medical-manufacturer/fixed-height-examination-table-39283.html)); la regla de cama de estatura + 15 cm | **Sí** |
| Paciente tumbado en la camilla | `Assets/Scripts/World/ArrivalExam.cs:136` (raíz a +0,75 m) y `Assets/Scripts/Body/DailyLife.cs:36` (Lie: cadera a 0,12 u) | La espalda queda a 0,76 m, así que **se hunde ~7 cm** en el acolchado. Con 1,85 m, la cabeza llega a z = −0,94 y **atraviesa el respaldo inclinado** (que empieza en z = −0,65); los talones **sobresalen ~6 cm** del borde (+0,85) | Raíz a la altura de la superficie (+0,83 con la camilla actual, o +0,80 con la nueva) y cadera en el centro de un plano de 2 m | La geometría del cuerpo de cubos | **Sí** (posición) |
| Silla de exámenes (iniciación) | `Assets/Editor/SampleSceneBuilder.Hospital.cs:83-88` (asiento :85, respaldo :86) | Asiento de 0,60 × 0,60 m, superficie a 0,50 m. Respaldo de 0,75 m (llega a 1,22 m), con su cara a z = −0,23 | Asiento a 0,43–0,46 m, de 0,45–0,50 de fondo y ≥ 0,45 de ancho | Sillas de 17–19 in de asiento ([Houzz](https://www.houzz.in/ideabooks/91819484/thumbs/standard-height-for-counters-tables-shelves-lights-more)); fondo ≤ 16,9 in y ancho ≥ 17,7 in ([UC Berkeley](https://uhs.berkeley.edu/sites/default/files/uc_task_chair_requirements_1.pdf)) | Opc. (el tamaño vale) |
| Paciente sentado en la silla | `ArrivalExam.cs:136-137` (raíz a +0,05, `rot = LookRotation(Front(place))`) y `DailyLife.cs:32` (Sit a 0,5 u) | La pelvis queda a 0,44 m con el asiento a 0,50 m: **se hunde ~6–8 cm**. La cadera queda en el centro del asiento, **a 12 cm del respaldo**. Además, el paciente **mira hacia el lado que da al centro del área**, no hacia el frente de la silla, así que puede quedar de lado o de cara al respaldo | Raíz con la cadera a (superficie del asiento + 0,12·s); la cadera retrasada ~0,10 m hacia el respaldo; la rotación con el frente de la silla (su +z local) | La geometría | **Sí** (posición) |
| Cama de hospital (48) y de aislamiento | `InfirmaryProps.cs:18-48` (Somier :27, Colchón :28, Almohada :30) | Colchón de 2,00 × 0,95 m con la superficie a 0,70 m; barandas a 0,82 m | Colchón de **2,10** × 0,90–0,95 m, con la superficie a 0,60–0,70 m (regulable en la realidad de 0,41 a 0,76 m) | 36 × 80 in de serie y 36 × 84 in para pacientes altos ([Amerisleep](https://amerisleep.com/blog/hospital-bed-mattress-sizes-and-dimensions/), [Mattress Miracle](https://mattressmiracle.ca/blogs/mattress-miracle-blog/hospital-bed-dimensions)) | Opc. (2,0 m es justo) |
| Cama de las habitaciones (residencia) | `Assets/Editor/ResidentialBuilder.cs:106-110` (Colchón :108, Almohada :110) | Colchón de 2,00 × 1,30 m, con la superficie a 0,50 m del estilóbato. La almohada está en **+z** | Colchón de **2,10 × 1,35–1,40 m**, superficie a 0,45–0,55 m | Cama = estatura + 15–20 cm; hasta 1,85 m basta con 200 cm y desde 1,85 m se pide 210 ([bett1](https://en.bett1.de/mattress-sizes), [Broring](https://broring.com/en/sleep-knowledge/welke-bedmaat-past-bij-jouw-lichaamslengte/), [Cloudpillo](https://cloudpillo.com/pages/topper-180x210)) | **Sí** |
| El que duerme en esa cama | `Assets/Scripts/Body/NeedsDriver.cs:110-113` | Raíz a +0,45 m, con la espalda a 0,46 m: **se hunde ~4 cm**. La pose Lie pone la cabeza hacia **−z** y la almohada está en **+z**, así que **duerme del revés**, con los pies en la almohada | Raíz a la superficie del colchón (+0,50) y la cabeza hacia la almohada: girar 180° o mover la almohada a −z | La geometría | **Sí** (posición) |
| Mesa de quirófano | `InfirmaryProps.cs:227-236` (Tablero :232, Sábana :233) | Tablero de 2,00 × 0,55 m, superficie a 0,96 m. Reposabrazos de 0,60 m | 2,00–2,10 × 0,50–0,55 m, regulable de 0,65 a 1,05 m (0,95 está dentro del rango) | Mesas de 2000–2100 × 480–500 mm y 670–1050 mm de alto ([MedicalExpo FYM600](https://www.medicalexpo.com/prod/jiangsu-fuyou-medical/product-304790-2799194.html), [HospiMedica SZ-01](https://hospimedica.com/expo-amp/product/13127/operating-and-treatment-table-model-sz-01)) | No |
| Cama de partos | `InfirmaryProps.cs:387-399` (Base :390, Colchón :391, Respaldo :392) | 1,40 m de plano más un respaldo de 0,60 m a 50° (≈ 1,80 m en planta) × 0,80 m; superficie a 0,91 m | ≈ 1,90–2,00 × 0,80 m entre respaldo y plano | Camas de partos comerciales (la pieza de los pies es extraíble) | Opc. |
| Mesa del médico, triajes 1-4 y consultas (escritorio) | `InfirmaryProps.cs:62-78` (Tablero :65) | 1,40 × 0,70 m, superficie a 0,765 m (bajo el tablero hay 0,715) | 0,72–0,76 m; hueco libre bajo el tablero ≥ 0,64–0,69 m | Mesas de 28–30 in ([Hunker](https://www.hunker.com/12422814/standard-height-of-kitchen-table/)) | No |
| Silla de la mesa del médico | `InfirmaryProps.cs:72-74` | Asiento de 0,45 × 0,45 m, superficie a 0,49 m | 0,43–0,46 m | [Houzz](https://www.houzz.in/ideabooks/91819484/thumbs/standard-height-for-counters-tables-shelves-lights-more), [UC Berkeley](https://uhs.berkeley.edu/sites/default/files/uc_task_chair_requirements_1.pdf) | Opc. (bajar a 0,45) |
| Sala de espera del hospital (10 sillas) | `SampleSceneBuilder.Hospital.cs:120-125` | Asiento de 0,50 × 0,50 m, superficie a 0,49 m | 0,43–0,46 m | Ídem | Opc. |
| Mesas del comedor del restaurante | `Assets/Editor/SampleSceneBuilder.Restaurant.cs:58-79` (Mantel :60, Caída :61) | Tablero de 1,30 × 1,30 m, superficie a 0,78 m. La «caída del mantel» es un **bloque macizo** de 1,32 × 1,32 m que va **de 0,45 a 0,75 m**: las rodillas del comensal (a ~0,55 m, 0,12 m dentro del borde) quedan **dentro del bloque** | Superficie a 0,74–0,76 m. Caída del mantel solo como faldón fino (≤ 0,15 m, por encima de 0,62 m) o 4 paneles finos en los bordes | 28–30 in ([Hunker](https://www.hunker.com/12422814/standard-height-of-kitchen-table/)); hueco para las rodillas de 27 in (ADA) | **Sí** (faldón) |
| Sillas del restaurante (48) | `SampleSceneBuilder.Restaurant.cs:72-75` (Asiento :73) | Asiento de 0,45 × 0,45 m, superficie a 0,49 m. La silla **mira hacia fuera** (yaw = s·90 con el asiento en +z): el respaldo está más lejos de la mesa que el asiento, pero `RestaurantServiceGame.cs:285` sienta al comensal con `seat.rotation` y pone el plato en `seat.forward` | Superficie a 0,45 m; el +z local apuntando a la mesa | Ídem | **Sí** (orientación, a verificar en juego) |
| Sentado en el restaurante y en la vida diaria | `RestaurantServiceGame.cs:286` (raíz = asiento − 0,45) y `DailyLife.cs:32` (Sit a 0,5 u) | La pelvis queda a 0,39–0,44 m con el asiento a 0,49 m: **se hunde 5–10 cm** | Cadera a (superficie del asiento + 0,12·s): calcular rootHeight desde la altura del asiento | La geometría | **Sí** (posición) |
| Encimeras de cocina, barra de servicio y emplatado | `Assets/Scripts/Kitchen/KitchenProps.cs:13` (`CounterTop = 0.9`) y `:83-98` | 0,90 m | 0,90–0,91 m (36 in). Para alguien de 1,85 m es ideal ~0,95–1,00 m, pero es una opción, no hace falta | [Asian Paints](https://www.beautifulhomes.asianpaints.com/blogs/standard-height-for-kitchen-counters-tables-and-lights) | No |
| Bancos del laboratorio de investigación | `Assets/Editor/SampleSceneBuilder.Research.cs:115-119` | Superficie a 0,94 m | 0,90–0,95 m (de pie) | Ídem | No |
| Fregaderos | `KitchenProps.cs:139-149` | Encimera a 0,90 m con la pila hundida | 0,90 m | Ídem | No |
| Mesa de corte (taller textil) | `SampleSceneBuilder.Restaurant.cs:242-248` | 2,20 × 1,10 m, superficie a 0,88 m | 0,90–0,95 m (de pie) | Ídem | No |
| Máquina de coser (mesa) | `SampleSceneBuilder.Restaurant.cs:250` | Superficie a 0,76 m | 0,72–0,76 m | [Hunker](https://www.hunker.com/12422814/standard-height-of-kitchen-table/) | No |
| Banco del telar | `SampleSceneBuilder.Restaurant.cs:210` | 1,20 × 0,35 m, superficie a 0,53 m | 0,45–0,50 m | [Houzz](https://www.houzz.in/ideabooks/91819484/thumbs/standard-height-for-counters-tables-shelves-lights-more) | Opc. |
| Inodoro seco (aseos ×4 edificios) | `SampleSceneBuilder.Restaurant.cs:174-175` | Caja de 0,42 × 0,55 m, asiento a 0,465 m | Asiento a 0,38–0,43 m (normal) o 0,43–0,48 m (accesible) | 15–17 in de serie y 17–19 in ADA ([Ferguson](https://www.ferguson.com/content/ideas-and-learning-center/trade-talk/five-tips-for-choosing-ada-compliant-toilets/), [Wayfair](https://www.wayfair.ca/sca/ideas-and-advice/guides/what-is-the-standard-toilet-height-T22530)) | No (sirve como «altura cómoda») |
| Cabina del WC y su puerta | `SampleSceneBuilder.Restaurant.cs:158-171` | Cabina de 0,95 × 1,50 m con techo a 2,20 m. Puerta de 1,98 m de alto (de 0,07 a 2,05) × 0,89 m; la accesible mide 1,44 m de ancho | Puerta ≥ 2,03–2,10 m × 0,80–0,90 m; cabina ≥ 0,90 × 1,50 m | Puertas de 80 in en EE. UU., 2040 mm en Reino Unido y 2100 mm según DIN 18101 ([itemfits](https://itemfits.com/door-guide/standard-door-sizes)) | Opc. (subir la puerta a 2,05 m) |
| Lavabos de los aseos | `SampleSceneBuilder.Restaurant.cs:185` | Superficie a 0,86 m | 0,80–0,90 m | — | No |
| Puertas de las cámaras frías (cocina) | `Assets/Editor/SampleSceneBuilder.KitchenPlan.cs:208` | 2,00 × 0,95 m | ≥ 2,03–2,10 m | [itemfits](https://itemfits.com/door-guide/standard-door-sizes) | Opc. |
| Puertas de repuesto (obra) | `Assets/Editor/SampleSceneBuilder.Construction.cs:99` | 2,10 × 0,95 m | 2,10 m | Ídem | No |
| Esterillas de yoga (30) | `Assets/Editor/SampleSceneBuilder.cs:3739` | 1,80 × 0,70 m | **2,00** × 0,66–0,70 m (largo = estatura + 15 cm; las extralargas miden 84–85 in) | 68 × 24 in es la medida de serie, 72 in la moderna y 78–84 in para altos ([Yoga Design Lab](https://yogadesignlab.com/blogs/yoga-gear/what-size-yoga-mat-for-63-a-guide-for-taller-yogis), [ídem](https://yogadesignlab.com/blogs/yoga-gear/best-yoga-mat-size-find-your-perfect-fit)) | **Sí** |
| Esterilla del maestro | `SampleSceneBuilder.cs:3743` | 2,00 × 0,80 m | 2,00 × 0,70–0,80 m | Ídem | No |
| Esterillas del gimnasio | `Assets/Editor/SampleSceneBuilder.Training.cs:45` | 2,00 × 1,00 m | ≥ 2,00 m de largo | Ídem | No |
| Máquinas del gimnasio: asiento y respaldo | `SampleSceneBuilder.Training.cs:84-85` | Asiento de 0,45 × 0,45 m a 0,55 m de alto; respaldo de 0,80 m | Asiento regulable de 0,43 a 0,55 m | — | No |
| Banco tumbado (curl femoral, hip thrust) | `SampleSceneBuilder.Training.cs:81` | 1,30 × 0,45 m, superficie a 0,56 m | Curl femoral: 1,30–1,40 m (las piernas salen por el rodillo). Hip thrust: banco a 0,40–0,45 m | — | Opc. (hip thrust a 0,43) |
| Barra de dominadas | `SampleSceneBuilder.Training.cs:55` | A 2,30 m | Estatura + alcance: 2,20–2,40 m | — | No |
| Peldaños de las escaleras inclinadas | `Assets/Editor/SampleSceneBuilder.Levels.cs:15` (`StairAngle` 38°) y `:53-66` | Contrahuella de 0,174 m, **huella de 0,223 m** (4 m en 23 peldaños), 38°. 2C + H = 0,57 | Contrahuella ≤ 0,175–0,18 m y **huella ≥ 0,25 m** (vivienda) o ≥ 0,28 m (pública). 2C + H = 0,62–0,64 → ~32–34° | Huella ≥ 10 in y contrahuella ≤ 7¾ in (IRC R311.7.5); huella ≥ 11 in en edificios públicos (IBC) ([Viewrail](https://resources.viewrail.com/code-compliance/stair-code/tread-depth-and-risers), [Ask the Builder](https://www.askthebuilder.com/best-stair-layout/)) | **Sí** (si cabe la escalera más larga) |
| Pasamanos | `SampleSceneBuilder.Levels.cs:72` | 0,90 m sobre la rampa | 0,86–0,97 m | IRC | No |
| Sillas de ruedas y sofás | — | **No existen** en el juego | — | — | — |

## Propuesta

Va por orden de importancia.

1. **La camilla** (`InfirmaryProps.ExamTable`, `InfirmaryProps.cs:50-59`):
   - Acolchado plano de **2,00 × 0,70 m**, centrado en z = 0, con la superficie a **0,80 m**: el cubo de 0,10 m de
     grueso queda a y = 0,75.
   - Quitar el respaldo inclinado de 30°, o dejarlo plano dentro de esos 2 m; el ECG se hace tumbado del todo.
   - Base de 1,80 × 0,60 × 0,70 m y el papel de 0,55 × 1,95 m.
   - En `ArrivalExam.PatientSpot` (`ArrivalExam.cs:136`), para tumbarse, poner la raíz a la altura de la superficie
     (+0,80 m) en lugar de +0,75. Así la espalda apoya sin hundirse y el cuerpo (1,84 m) queda dentro del plano, con unos
     8 cm de margen en cada punta.
2. **Al sentarse, el cuerpo se hunde en todas las sillas.** Sit (`DailyLife.cs:32`) usa la cadera a 0,5 unidades, sea
   cual sea la silla. Hay que calcular rootHeight desde la superficie del asiento: cadera = asiento + 0,12·s. Afecta a
   `ArrivalExam.cs:136`, `RestaurantServiceGame.cs:286` y `DailyLife.cs:96`. También hay que retrasar la cadera unos
   0,10 m hacia el respaldo.
3. **El paciente mira en la dirección de la silla.** En `ArrivalExam.cs:137`, para sentarse, usar la rotación de la
   silla (su +z) y no `Front(place)`.
4. **La cama de la residencia está del revés.** En `NeedsDriver.cs:110-111`, la pose Lie pone la cabeza en −z y la
   almohada de `ResidentialBuilder.cs:110` está en +z. Hay que girar al que duerme 180° o mover la almohada a −0,8.
   Hay que subir la raíz a +0,50 en lugar de +0,45.
5. **Alargar la cama de la residencia a 2,10 × 1,40 m** (`ResidentialBuilder.cs:107-108`: armazón de 2,20 m). Hay que
   comprobar que la mesilla de `:111` sigue sin tocarla.
6. **Comedor del restaurante** (`SampleSceneBuilder.Restaurant.cs:61`): cambiar el bloque macizo de la caída del mantel
   (de 0,45 a 0,75 m) por un faldón de 0,62 a 0,765 m. Bajar el tablero a 0,75 m. Hay que verificar en juego la
   orientación de las sillas (`:73`): el +z del asiento apunta hacia fuera de la mesa.
7. **Esterillas de yoga** (`SampleSceneBuilder.cs:3739`): de 1,80 a **2,00 × 0,66 m**. Hay que comprobar que los anillos
   de 3,4 y 5,4 m siguen sin solaparse; la esterilla mide 0,2 m más.
8. **Escaleras** (`SampleSceneBuilder.Levels.cs:15`): bajar `StairAngle` de 38° a ~33°, para dejar huella de 0,26–0,27
   y contrahuella de ~0,175. La escalera se alarga de 5,1 a 6,2 m en planta, así que antes hay que revisar dónde cabe en
   el gimnasio, el hospital, el restaurante y la investigación.
9. **Cama de hospital** (`InfirmaryProps.cs:27-29`): opcionalmente, somier de 2,15 m y colchón de 2,10 m, con el
   cabecero y el piecero en ±1,075. Así el paciente de 1,85 m tiene margen; ahora cabe justo.
10. **Asientos a 0,45 m** en vez de 0,49 m (la superficie): silla del médico (`InfirmaryProps.cs:72`), sala de espera
    (`SampleSceneBuilder.Hospital.cs:122`), silla de exámenes (`:85`) y sillas del restaurante (`Restaurant.cs:73`).
    Con el arreglo del punto 2 basta con mover el centro del cubo a 0,41.
11. **Detalles:**
    - Puertas de los aseos y de las cámaras frías a ≥ 2,05 m.
    - Banco del hip thrust a 0,43 m.
    - Banco del telar a 0,47 m.
    - Cama de partos: un plano de 1,90–2,00 m.

**Nota sobre el cuerpo de cubos.** Su altura poplítea es solo 0,205 × la estatura (0,38 m a 1,85 m), contra 0,25 en
una persona real. Así que, aun con sillas reales de 0,45 m, los pies del más alto quedan unos 7 cm por encima del suelo,
y los de Lizmibet e Irosene (1,55 m) unos 13 cm. Hay dos salidas:
- alargar la espinilla en `CubeHumanoid.cs:103-106` (de 0,40 a ~0,46 u, compensando el muslo);
- aceptar asientos de ~0,42 m, el límite bajo de lo real.
