# Restaurante: distribución real y propuesta (cocina en toda la planta, comedor en anillo)

> Berón, 2026-10-23: «el restaurante me parece muy alejado de la realidad… investigar cómo son las áreas donde se prepara
> el queso, el kéfir, la pasta, el pan… que la cocina ocupe la planta entera y las mesas vayan sobre una plataforma hija
> que cubra todo el borde, un cuadrado completo, a caballo del borde; la escalera empieza desde un puente».
> Datos y fuentes: `docs/cocina-investigacion.md`. Estado: **propuesta para aprobar**.

## 1. Lo que hay hoy
- **Plataforma:** 40 × 38 m (`SampleSceneBuilder.Restaurant.cs`).
- **Cocina abierta en el centro**, en `ProKitchen.cs`:
  - hornos mixto y de pizza;
  - isla de inducción (wok, paellero, trompo, fogón, comal);
  - amasadora, laminadora, batidora, arrocera y vaporera;
  - barra de sushi y cafetera.
- **Obrador a los lados**, mezclado con el comedor:
  - lado oeste: molino, trapiche, almazara, extrusora, nixtamalizadora, barricas y cava;
  - lado este: cámara de fermentación, incubadoras de yogur y tempeh, cuba de quesería, prensa de tofu, abatidor y marmitas.
- **Comedor:** 12 mesas de 4 (48 cubiertos). Seis están en el suelo y seis en dos galerías laterales a 4 m.
- **Lo que no es real:**
  - no hay recepción de mercancías;
  - no hay cámaras frías ni congelador, ni almacén seco;
  - no hay flujo crudo → preparación → fuego → pase → sala;
  - no hay zona de lavado sucio → limpio;
  - los obradores (quesería, panadería, pasta, fermentos) no tienen sala propia, y la quesería no tiene cava de maduración con su clima.

## 2. Superficies reales (resumen de `docs/cocina-investigacion.md`)

| Zona | Superficie / medidas | Clima |
|---|---|---|
| Cocina para 50 cubiertos | 0,6–0,8 m² por cubierto: 35–45 m² de producción + 15–25 m² de almacenes y lavado | — |
| Cámara de refrigeración | 5–6,3 m² | 1,7–3,3 °C (máx. 5 °C) |
| Congelador | [?] ~4 m² | −18 °C |
| Línea caliente | fogón de 6 fuegos 0,90 × 0,80; freidora 0,40 × 0,85; horno mixto 6 GN 0,87 × 0,78 | — |
| Lavado | mesa sucia 1,1 × 0,65 → lavavajillas → mesa limpia 0,65 × 0,65 | — |
| Quesería | sala de elaboración 20–30 m² [?] (cuba, moldeo y desuerado, prensa, salmuera de 18–20 % de sal) | 10–14 °C |
| Cava de maduración | 10–20 m² [?] | 10–13 °C, 85–95 % de humedad |
| Fermentos | — | yogur 40–45 °C; kéfir 22–25 °C durante 18–24 h; chucrut 18–22 °C |
| Pasta fresca | mesa, laminadora (0,51 × 0,70), secaderos, frío | — |
| Panadería | amasadora espiral 0,5 × 0,9; horno de 3 pisos 1,8 × 1,3; fermentadora 1,07 × 1,31; carros de 0,60 × 0,80 | — |
| Comedor | 1,1–1,4 m² por plaza (informal) o 1,7–1,9 (alta cocina); mesa de 4: 0,76 × 1,22; paso de servicio 1,57 m | — |

Pasillos reales: 0,9–1,07 m para un cocinero, 1,22 m de doble sentido y 1,83 m cómodo entre dos líneas enfrentadas. **La regla de 2 m es más que lo normal**: la investigación propone aplicarla entre líneas o bloques, dejando los equipos de una misma línea uno junto a otro, como en una cocina real.

## 3. Propuesta

**Planta (40 × 38 m): solo cocina y obradores**, ordenados por el flujo. La entrada de mercancías va por el puente:
1. **Recepción** junto al puente: báscula y mesa de control.
2. **Almacenes:** seco con estanterías, cámara de refrigeración, congelador y cámara de lácteos.
3. **Preparación:** verdura y carne o pescado en mesas separadas, con su fregadero.
4. **Línea caliente:** isla de inducción, fogón, horno mixto, horno de pizza, plancha y freidora. Delante, el **pase**, a pie de la escalera del comedor.
5. **Lavado**, separado de la comida: sucio → lavavajillas → limpio, en el camino de vuelta de la sala.
6. **Obradores con sala propia** y su clima:
   - quesería con cava de maduración;
   - fermentos: kéfir, yogur, tempeh y chucrut;
   - pasta fresca;
   - panadería: amasadora, mesa, fermentadora y horno de pisos;
   - el obrador «antiguo»: molino, trapiche, almazara, extrusora y nixtamal.
7. **Barra de sushi y cafetera:** suben al anillo, de cara a los comensales.

**Comedor en un ANILLO cuadrado completo**, como plataforma hija a 4 m, a caballo del borde: mitad sobre la planta, mitad volando por fuera.
- Con un ancho de 5 m, el anillo tiene unos 165 m de largo y más de 800 m². Caben de sobra las 12 mesas de 4 a 2 m entre sí, y se puede subir a 60–80 cubiertos.
- Su **escalera nace del puente** que ya llega a la plataforma del restaurante. No cruza la cocina.
- Debajo del anillo, la planta queda libre para la cocina: el anillo solo pisa una franja de 2,5 m junto al borde.

**El mismo anillo en el hospital** (pendiente de la respuesta de Berón): las 30 camas en el anillo, con su escalera desde un puente nuevo hacia una **base de alfombras voladoras**, que hay que construir.

Preguntas abiertas:
- Regla de 2 m dentro de una línea de cocina: ¿entre bloques, como en la realidad, o entre todos los equipos?
- ¿Anillo de camas en lugar de la planta 1 del hospital, o además de ella?

## 4. Medidas del anillo (Berón, 2026-10-23)
- **Ancho:** unos 10 m. Fila central de mesas de 4 con sus sillas (unos 2,2 m) y **4 m libres a cada lado**.
- **Posición:** solo el **20 % sobre la planta** (unos 2 m) y el **80 % volando por fuera** (unos 8 m), para no quitar sitio a la cocina.
- **Anillo de camas del hospital:** lo mismo, con 4 m de paso a cada lado de la cama.

## 5. Personal y puestos (Berón, 2026-10-23)
- **8 personas:**
  - 4 cocineros: 3 en la línea y 1 en el pase, que sube los platos a la sala;
  - 2 en preparación: panadería y pasta, y lácteos y fermentos;
  - 2 en limpieza: la cocina y el lavado, y el comedor del anillo.
- **CAJA** en el anillo, junto a la entrada desde el puente. La atiende el cocinero que esté libre.
- En el restaurante todos saben cocinar. Si la cocina se satura, los de preparación y limpieza **entran a cocinar**. Por eso la línea tiene **8 puestos de trabajo**, uno por persona:
  - 2 fuegos e inducción;
  - 2 horno y plancha;
  - 2 frío y sushi;
  - 2 pase.
- **Obradores DUPLICADOS:** cada estación, por dos, para hacer la misma tarea a la vez:
  - 2 puestos de panadería: amasadora y mesa;
  - 2 de pasta: mesa y laminadora;
  - 2 cubas de quesería;
  - 2 puestos de fermentos.

  Los equipos grandes y de uso compartido van una sola vez: el horno de pisos, la cava, las cámaras y el lavavajillas.

## 6. Puestos de cocina COMPLETOS en anillo (Berón, 2026-10-23)
- Cada uno de los **8 puestos** es una **estación integrada completa**, con todo lo que pide la carta, para que los 8 cocineros puedan hacer **el mismo platillo a la vez**. Los equipos de la carta (`Gastronomy.Equipment`) son:
  - horno mixto;
  - marmita y marmita a presión;
  - paellero, wok y fogón de inducción;
  - trompo vertical;
  - horno de pizza;
  - vaporera;
  - abatidor;
  - barra fría (sushi y montaje);
  - fregadero de puesto.
- **Forma de cada puesto:** una U de unos 11 m de encimera (la línea caliente pegada, como en la realidad) con un pasillo interior de 1,2 m. Huella de unos 5 × 4 m (≈ 25 m²).
- **Distribución:** los 8 puestos en **anillo** alrededor del **pase central**, con 2 m libres entre puestos (≈ 200 m² más el paso). Los obradores, almacenes y lavado van por fuera del anillo, y el comedor está en el anillo de arriba.
