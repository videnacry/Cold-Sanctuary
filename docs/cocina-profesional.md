# Cocina profesional del restaurante

El restaurante es una vía de ingresos del santuario: recibe turistas (base de 30 comensales, igual que la enfermería
y la sala de yoga). La cocina es **abierta al público**: está en la misma plataforma que el comedor, con la barra de
sushi y la cafetera de cara a las mesas. Todo se elabora en casa, desde la harina hasta el queso.

Código: `Assets/Scripts/Kitchen/Gastronomy.cs` (recetario), `KitchenLarder.cs` (despensa y obrador),
`Assets/Editor/SampleSceneBuilder.ProKitchen.cs` (equipo) y `CulturedMeatLab.cs` (carne cultivada).

## Origen de los ingredientes (decisión por defecto)

- **Vegetales**: de los huertos por gremio (`GardenCrop`, `GardenPlot`). Se añadieron 23 cultivos para la carta:
  trigo, maíz morado, haba, arveja, melloco, papa, perejil, romero, azafrán, achiote, sésamo, wasabi, pepino,
  frutilla, hierbaluisa, cedrón, caña de azúcar, naranjilla, mortiño, babaco, clavo, café y aguacate.
  - Hortalizas asociadas: pepino, perejil, frutilla, romero y papa dentro de las filas existentes.
  - Chacra andina: haba, melloco, maíz morado, papa, arveja, hierbaluisa, cedrón.
  - Rotación: trigo (dos franjas), arveja, haba, sésamo.
  - Milpa: borde de achiote y azafrán. Arrozal: wasabi en la orilla húmeda.
  - Bosque comestible: aguacate, clavo, café, babaco; borde de caña, naranjilla y mortiño.
- **Carne**: solo **carne cultivada**. El laboratorio rota nueve líneas celulares (res, pollo, salmón, cerdo, atún,
  conejo, bacalao, calamar, gamba). Cada lote deja raciones en el comedero de los depredadores y 4 en la cocina.
  Las líneas de pez y marisco crecen a menor temperatura (≈27 °C) que las de mamífero y ave (37 °C).
- **Leche y huevo**: sin animales, por **fermentación de precisión** (levaduras que producen caseína, suero y
  ovoalbúmina). Con ellas se hacen leche, nata, mantequilla, yogur y quesos con **cuajo microbiano**.
- **Proteína vegetal**: tofu (leche de soja cuajada con nigari), tempeh (soja fermentada con *Rhizopus*) y seitán
  (gluten de trigo). El bowl de tofu y tempeh es la opción vegana fija; cualquier platillo de carne cultivada puede
  servirse con tofu, tempeh o seitán.

## Elaboraciones base y sus tiempos

Los tiempos corren en **horas de juego** (`TimeController`), así que se ajustarán solos al calibrar el reloj.

| Elaboración | Equipo | Reposo | Cómo |
|---|---|---|---|
| Harina, sémola | molino de piedra | — | moler el grano y tamizar |
| Masa madre | cámara de fermentación | 5 días | harina y agua alimentadas cada día |
| Yogur | yogurtera | 4–8 h | leche a 85 °C, enfriar a 43 °C, cultivos lácticos |
| Queso fresco / quesillo | cuba de quesería | 2 h | leche a 32 °C + cuajo, cortar y desuerar |
| Mozzarella | cuba de quesería | 4 h | cuajada ácida hilada en agua a 80 °C |
| Mascarpone | marmita | 12 h | nata a 85 °C con limón, escurrir en frío |
| Parmesano | cava de maduración | 12 meses | cocida a 55 °C, salmuera 20 días, cava a 12 °C |
| Pan de masa madre | horno mixto | 16 h | bloque 4 h, retardo en frío 12 h, 230 °C |
| Masa de pizza | amasadora espiral | 24–72 h | 65 % de hidratación, maduración en frío |
| Pasta fresca | laminadora | 30 min | sémola y huevo |
| Pasta seca | extrusora y secadero | 24–48 h | molde de bronce, secado lento |
| Fideos de ramen | laminadora | 1 día | agua alcalina (kansui) |
| Tofu | prensa de tofu | 30 min | leche de soja a 75 °C + nigari |
| Tempeh | incubadora | 24–48 h | soja cocida + *Rhizopus* a 30 °C |
| Shoyu, miso | barricas | 6–12 meses | soja con koji y sal |
| Vinagre de arroz | barricas | 1 mes | sake de arroz y fermentación acética |
| Masa de maíz | nixtamalizadora | 8–12 h | maíz cocido con cal, reposo, lavado y molido |
| Caldo tonkotsu | marmita a presión | 8–12 h | huesos de cerdo cultivado hasta emulsionar |

La bodega arranca con piezas ya curadas (parmesano, shoyu, miso, vinagre, masa madre), como cualquier restaurante.

## Carta

Colada morada con guaguas de pan, fanesca, tres leches, pastel de maíz, humitas, lasaña, ramen tonkotsu, pizza
napolitana, tacos al pastor, enchiladas, paella valenciana, arroz negro, arroz chaufa, tiramisú, pastel de queso,
sushi, fideuà, espaguetis al pomodoro, bowl de tofu y tempeh y pan de masa madre con aceite.

Cada platillo lista sus componentes. Si uno es una elaboración (queso, pan, pasta…), el obrador la prepara antes,
y encadena las que esta necesite (el queso pide leche; la leche, proteína de fermentación).

## Equipo instalado (29 máquinas)

- **Sala (dentro)**: horno mixto, horno de pizza, arrocera, vaporera, batidora planetaria, amasadora espiral,
  laminadora; isla de fuego de inducción con wok, paellero, trompo, fogón y comal; barra de sushi y cafetera
  espresso de cara al público. No hay gas: todo es eléctrico y solar.
- **Obrador oeste (fuera de la sala, misma plataforma)**: molino de piedra, trapiche y paila, almazara, barricas,
  cava de maduración, extrusora y secadero, nixtamalizadora.
- **Obrador este**: cámara de fermentación, incubadora de tempeh, yogurtera, cuba de quesería, prensa de tofu,
  abatidor, marmita a presión y marmita.

Cada máquina se llama `Equipo_<nombre>` en la escena y lleva su cartel.
