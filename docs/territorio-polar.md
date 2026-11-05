# Territorio polar: la isla de tierra y los casquetes vivos

Berón (2026-11-05): «quizás debemos crear cuevas o similar en los casquetes del santuario? se debe investigar lugares reales llenos de
vida […] también es posible que alguno de los casquetes no sea casquete sino una isla de tierra […] se puede distribuir 50/50 […] imagino
que en el mundo hay casquetes cerca de islas o tierra firme solo que a muchos más km no?»

Especies y sus fichas: [reglas-animales.md](reglas-animales.md). Reglas de diseño: [manual §13](manual-de-diseno.md#13-animales-y-territorio-polar).

## Lugares reales llenos de vida

| Lugar | Por qué es rico | Datos | Qué tomamos |
|---|---|---|---|
| **Georgia del Sur** | Agua templada circumpolar contra agua de Weddell: plataforma rica y kril. A 1 550 km de la Antártida. | Pingüino rey > 450 000 parejas, 22 M de priones (Clarke et al. 2012, *British Birds* 105:118); 3,5 M de lobos marinos, ~98 % del mundo (Forcada et al. 2023, doi 10.1111/gcb.16947). | Playas de grava y tussac en la orilla de la isla. Allí los renos introducidos sobrepastorearon el tussac. |
| **Mar de Ross** | Polinia de ~396 000 km² abierta por viento catabático: agua libre y luz en primavera. | Frente de plataforma de 600+ km y 15–50 m de alto; 1/3 de los Adelia y más de 1/4 de los emperadores; cabo Crozier: ~272 000 parejas (2012). | Frentes de barrera, colonias de Adelia y emperador. |
| **Svalbard** | Glaciares que llegan al mar y mezcla de aguas; el guano de aves fertiliza la tundra. | 57 % glaciar, 163 frentes en el mar (860 km); 60 000+ parejas de arao en Alkefjellet (100 m de basalto); 5 503 morsas (2018); más de 1 M de parejas de mérgulo; ~60 t/km² de guano. | Acantilados de aves, morsas en la playa, tundra verde al pie. |
| **Canal Beagle** | Bosques de *Macrocystis* más australes, vivero de peces. | Foca, lobo marino y papúa. | Pendiente: bosque de algas (ver abajo). |
| **Polinia North Water, Pribilof, isla Wrangel** | Polinia, plataforma de Bering, refugio sin hielo glaciar. | ~33 M de parejas de mérgulo (extrapolación incierta); 50 % de los lobos marinos del mundo; 350–600 guaridas de oso en Wrangel. | La isla Wrangel confirma que oso, morsa y tierra conviven. |

**Lo común:** la vida se concentra donde se encuentran agua rica, luz y un sitio firme para criar: bordes de hielo, polinias, acantilados y
playas. Por eso el santuario la pone en los bordes y no reparte animales por igual.

**Sin fuente:** no se encontró nada citable sobre cuevas de hielo o de mar como refugio de fauna. Las guaridas de oso y de lobo sí están
documentadas; las cuevas de hielo del juego son una licencia del santuario artificial, y como tal se dicen.

## ¿Hay casquetes cerca de tierra? ¿A cuántos km?

Depende, y Berón tenía razón en lo general:
- **Pegados:** la plataforma de Ross está unida a la isla de Ross por el sur y el este; distancia casi cero.
- **Muy lejos:** Georgia del Sur está a 1 550 km de la Antártida. En 2020 el borde del hielo de verano quedó a unos 470 km al norte de Svalbard.

**La isla y los casquetes del santuario están a 8–30 m, comprimidos para el juego.** Es como todo aquí: el mapa mide 500 m. El santuario es
artificial, obra de la maestra, y por eso junta morsas árticas con pingüinos antárticos. Ningún dato de distancia del juego es real.

## El reparto

| Suelo | Masas | Área |
|---|---|---|
| **Hielo** (`Casquete`) | Norte, Centro, Este, Bosque Oeste y Bosque Norte | ~40 650 m² (47 %) |
| **Tierra** (`Tierra`, `Tierra_orilla`) | el antiguo casquete Sur y 8 lóbulos `Isla:` | ~45 800 m² (53 %) |

```
  z
 200          oooo                       o hielo
 100    oooo   oo ooo  ooo               # tierra
   0   oooo    oooooooo     oooooooo
 -50   oooo        ooo        oooo
-100 ####### ##########################
-150  ###########################
-200 ##########################
    -250      -100     0      100    x
```

**Por qué el antiguo casquete Sur y no uno nuevo:** al suroeste solo caben ~30 000 m² de mar libre. Sumando el Sur salen mitad y mitad, y además
el Árbol de los Campos y la torre de Ciencias, que estaban sobre el Sur, **quedan sobre tierra sin mover nada**: los cultivos están donde tiene
sentido que estén.

**Limita con:** la lanzadera (a 335 m, regla ≥ 300 m) y el sitio nuclear (a 535 m, regla ≥ 500 m). Por eso la isla no sigue al sureste.

**Los bosques de árboles carnívoros siguen en hielo:** viven de agua y frío. En la isla no crecen árboles de hielo; los de junto a las áreas
del Sur ya no salen (`BuildIceFlora` pregunta `IceAt`).

### La isla por dentro

- **Suelo:** tundra en el interior, orilla de grava de 5 m y zócalo de roca hasta −6 m, solo visual: sin collider, para que el submarino de obra
  (−12 m) y los peces pasen por debajo. La placa mide 1,5 m como el hielo.
- **Relieve:** cuatro lomas de 3 terrazas de 25 cm, por las que se anda.
- **Charcas:** cuatro, con `WaterSource`; los animales que beben van a ellas.
- **Flora (`PolarUnderstory.Tundra`):** por comunidades, según un ruido suave:
  - pradera húmeda: juncia (15–30 cm), algodón ártico, musgo;
  - brezal: camarina, sauce rastrero, abedul enano (0,5–1 m), dryas;
  - pedregal: liquen de reno, saxífraga, silene, amapola;
  - orilla: tussac (hasta ~2 m).
- **Lo que se come:** juncia y tussac son `Grass`, sauce y abedul son `Willow`, camarina es `Berry`, liquen es `Lichen`. Se gastan y rebrotan muy
  despacio (el liquen real crece 3–11 mm al año).

### Los casquetes

- **Frentes de barrera:** bloques de hielo de 4–10 m en las orillas que dan al mar abierto (Este, Bosque Oeste, Bosque Norte). Este gana una
  plataforma nueva, `Este:Barrera`.
- **Cuevas:** `Cueva_hielo_Este` (guarida de oso), `Cueva_hielo_Oeste`, `Cueva_hielo_Norte` y `Cueva_roca_Poniente` (guarida de lobos). Boca de
  4,4 × 3,2 m y 11 m de fondo, con techo y carámbanos. Dentro hay `Shelter`.
- **Acantilados de aves:** roca de 6–12 m con repisas y guano en el sur de la isla. **Aún sin aves:** son el sitio reservado para el frailecillo.
- **Colonia del emperador:** `Colonia_emperador`, en hielo fijo (lóbulo Este:SalaYoga, a unos 50 m de los bancos de la laguna), al abrigo de cuatro
  icebergs varados. Sin nidos.
- **Colonia de Adelia:** la playa de guijarros de siempre, ahora con nidos en anillo.
- **Playa de morsas:** `Playa_morsas`, grava junto a la laguna con 6 plazas, y bivalvos en los bajíos de grava.

## Qué se movió

**Ninguna torre.** Se estudió llevar Producción a la isla (huertos y construcción sobre tierra) y se descartó:
- Este se quedaría sin alfombras al suelo, y el guardia ya no llegaría al comedero de los osos.
- Las torres lejanas ya son las que peor llegan al horario libre (grupo 89, ronda 6).
- Ciencias y el Árbol de los Campos ya quedan sobre tierra sin moverse.

Como ninguna torre se mueve, `mig/Clear.cs` no hace falta: las holguras son las de siempre.

**Lo que cambia de sitio:** nada construido. Solo cambia el suelo bajo el Sur, y desaparecen los árboles de hielo junto a sus áreas.

**El guardia** no da de comer a lobos y zorros de la isla: cazan por su cuenta.

## Pendiente

- **Parada de alfombra en la isla:** hoy se llega a pie desde el pie del Árbol de los Campos o de Ciencias (120–250 m). Una estación en la tundra
  evitaría el paseo.
- **Bosque de algas** (Beagle) bajo la orilla sur, y aves en los acantilados.
- **Corro del buey** como conducta real.
- **Isla en el mapa del jugador,** si hay.
