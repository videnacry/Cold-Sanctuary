# Morfología de insectos y arañas para el cuerpo de cubos

Ficha para modelar los artrópodos del microcosmos con **cubos articulados** (`CubeInsect`). Recoge cada segmento, su
largo y ancho relativos al cuerpo, el ángulo de reposo y el rango de cada articulación. Solo trae datos: `CubeInsect` y
las escenas los lleva el hilo de pruebas en vivo en Unity.

Las hormigas del microcosmos son **mirmidones**, hormigas humanas (bípedas, con cara: `CubeInsect.DressHuman`). En ellas la morfología
real se aplica a los **segmentos de las patas y a la antena acodada**, no a la postura.

## Convenciones

- **L** es el largo del cuerpo, del frente de la cabeza a la punta del abdomen, sin antenas, mandíbulas ni patas. Todos
  los largos y anchos se dan como fracción de L, igual que el parámetro `length` de `CubeInsect.Dress`.
- Ejes como en `CubeInsect`: **z** hacia delante, **y** hacia arriba y **x** hacia el lado derecho.
- Ángulos en grados. El **reposo** es la pose de pie, quieta. El **rango** es el máximo fisiológico, no el de la marcha.
- Patas: **P1** delanteras, **P2** medias y **P3** traseras (en arañas, hasta **P4**).
- Fuentes:
  - **[Pn]**: artículo verificado en PubMed (lista al final).
  - **[A]**: anatomía general de entomología. Es una aproximación de diseño y conviene refinarla con medidas de la especie
    concreta cuando haga falta más precisión.

## Pata de insecto (plantilla común)

Un insecto tiene seis patas con **5 segmentos** cada una. Cada especie solo cambia los largos y algún rango.

| Segmento | Articulación con el anterior | Eje principal | Reposo | Rango | Notas |
|---|---|---|---|---|---|
| Coxa | Tórax–coxa | Yaw (pro/retracción) | P1 +30°, P2 0°, P3 −30° | ±40° | Mueve la pata adelante y atrás en cada paso. También eleva ±20° (pitch) [A] |
| Trocánter | Coxa–trocánter | Pitch (eleva/baja) | 40° hacia arriba | 0–90° | Levanta la pata en la fase de vuelo [A] |
| Fémur | Trocánter–fémur | Casi fija | 0° | ±10° | En la práctica, trocánter y fémur se mueven juntos [A] |
| Tibia | Fémur–tibia (la "rodilla") | Pitch | 100° de flexión | 30–160° | Es la articulación que más se ve [A] |
| Tarso | Tibia–tarso | Pitch | 20° | ±30° | El tarso se apoya en el suelo con 2 a 5 tarsómeros según el grupo, más uñas [A] |

**Marcha:**
- **Trípode alterno**: P1 izquierda, P2 derecha y P3 izquierda se mueven a la vez, y luego las otras tres. A poca
  velocidad la marcha pasa a "onda" (de atrás hacia delante, una pata cada vez) [A].
- **Velocidad de las hormigas del desierto**: llegan a **80 largos de cuerpo por segundo** con zancadas de **hasta 40
  Hz**. A máxima velocidad añaden fases aéreas, en las que ninguna pata toca el suelo [P1].

## Hormiga (`Ant`)

Sirve para las patas y la antena de los mirmidones, y para las hormigas reales (`Ant`) si alguna escena las pide.

| Segmento | Largo/L | Ancho/L | Reposo | Rango | Fuente |
|---|---|---|---|---|---|
| Cabeza | 0,22 | 0,18 | Pitch 0° | Pitch ±30°, yaw ±30° | [A] |
| Mandíbulas (×2) | 0,08 | 0,03 | Cerradas, 10° | 0–70° de apertura cada una | [A] |
| Antena: escapo | 0,20 (≈ largo de la cabeza) | 0,02 | 30° hacia arriba y delante | Yaw ±60°, pitch ±45° | [A] |
| Antena: codo + funículo (11 artejos) | 0,24 | 0,02 | Codo a 90° | Codo 40–150° | La antena es **acodada**, con 12 segmentos en obreras de *Formica* [A] |
| Tórax (mesosoma) | 0,30 | 0,12 | — | — | [A] |
| Pecíolo (cintura) | 0,06 | 0,06 | — | Pitch ±30° | [A] |
| Gáster (abdomen) | 0,36 | 0,22 | — | Pitch −20° a +90°; lo levanta al amenazar | [A] |
| P1 fémur / tibia / tarso | 0,22 / 0,20 / 0,18 | 0,03 | Plantilla | Plantilla | [A] |
| P2 fémur / tibia / tarso | 0,24 / 0,22 / 0,18 | 0,03 | Plantilla | Plantilla | [A] |
| P3 fémur / tibia / tarso | 0,30 / 0,28 / 0,22 | 0,03 | Plantilla | Plantilla | [A] |

Marcha en trípode, muy rápida [P1].

## Pulgón (`Aphid`, Ambrosio)

| Segmento | Largo/L | Ancho/L | Reposo | Rango | Fuente |
|---|---|---|---|---|---|
| Cabeza | 0,14 | 0,16 | Pitch −20°, con el pico hacia la planta | Pitch ±20° | [A] |
| Estilete (pico) | 0,20 | 0,02 | Hacia abajo, plegado bajo el cuerpo | Pitch 0–80°; se clava al alimentarse | [A] |
| Antenas (6 artejos) | 0,70 | 0,015 | Hacia atrás sobre el cuerpo, 30° | Yaw ±40°, pitch ±40° | [A] |
| Cuerpo (tórax + abdomen en pera) | 0,86 | 0,55 | — | — | [A] |
| Sifones (×2) | 0,15 | 0,03 | 30° hacia atrás y arriba | Fijos | Sirven para defensa (feromona de alarma) y para darse la vuelta si queda patas arriba [P2] |
| Cauda | 0,06 | 0,05 | — | — | [A] |
| Patas P1–P3: fémur / tibia / tarso | 0,22 / 0,30 / 0,06 | 0,02 | Plantilla | Plantilla | Patas finas; tarso de 2 artejos; camina lento [A] |

## Mariquita (`Ladybug`)

| Segmento | Largo/L | Ancho/L | Reposo | Rango | Fuente |
|---|---|---|---|---|---|
| Cabeza | 0,15 | 0,30 | Medio escondida bajo el pronoto | Pitch ±15° | [A] |
| Pronoto | 0,18 | 0,55 | — | — | [A] |
| Élitros (×2, cúpula) | 0,70 | 0,45 cada uno; alto 0,45 | Cerrados | Se abren 0–60° hacia fuera y arriba para volar | [P3] |
| Alas traseras (×2) | 1,3 desplegadas | 0,50 | Plegadas bajo los élitros | Se despliegan al abrir los élitros y se repliegan después | Se pliegan con ayuda del borde de los élitros [P3] |
| Antenas (en maza) | 0,15 | 0,02 | Hacia delante | ±30° | [A] |
| Patas P1–P3: fémur / tibia / tarso | 0,18 / 0,16 / 0,10 | 0,03 | Recogidas bajo la cúpula | Plantilla | Patas cortas, tarsos de 4 artejos que parecen 3 [A] |

## Escarabajo pelotero (`DungBeetle`, Momo y Medea)

| Segmento | Largo/L | Ancho/L | Reposo | Rango | Fuente |
|---|---|---|---|---|---|
| Cabeza (clípeo en pala, dentado) | 0,18 | 0,45 | Pitch 0° | Pitch ±30°; cava con ella | [A] |
| Pronoto | 0,28 | 0,60 | — | — | [A] |
| Élitros | 0,54 | 0,58 | Cerrados | 0–45° | [A] |
| P1 fémur / tibia / tarso | 0,20 / 0,24 / 0,04 | 0,05 | Plantilla | Plantilla | La tibia es ancha y **dentada** para cavar y moldear la bola [A] |
| P2 fémur / tibia / tarso | 0,22 / 0,22 / 0,10 | 0,04 | Plantilla | Plantilla | [A] |
| P3 fémur / tibia / tarso | 0,30 / 0,35 / 0,12 | 0,04 | Plantilla | Plantilla | Patas largas y curvas para abrazar la bola [A] |

Al rodar la bola, el escarabajo **va hacia atrás**: cabeza abajo, P1 en el suelo y P3 empujando la bola [A]. Rueda en
línea recta orientándose por el sol, la luna, la luz polarizada y hasta la Vía Láctea [P4]. Antes de rodar se sube a la
bola y gira sobre ella para orientarse, un comportamiento bonito de mostrar.

## Saltamontes (`Grasshopper`)

| Segmento | Largo/L | Ancho/L | Reposo | Rango | Fuente |
|---|---|---|---|---|---|
| Cabeza | 0,15 | 0,15 | Pitch 0° | ±20° | [A] |
| Antenas (cortas, filiformes) | 0,35 | 0,01 | Hacia delante y arriba | ±40° | [A] |
| Pronoto + tórax | 0,25 | 0,16 | — | — | [A] |
| Alas (tegmina) | 0,70 | 0,12 | Plegadas sobre el abdomen | Se abren 0–90° | [A] |
| Abdomen | 0,60 | 0,13 | — | Pitch ±15° | [A] |
| P1 / P2 fémur / tibia / tarso | 0,15 / 0,14 / 0,08 | 0,03 | Plantilla | Plantilla | [A] |
| P3 fémur | 0,50 | 0,09 (muy grueso) | Paralelo al cuerpo, 30° hacia arriba | — | Almacena la energía del salto [P5] |
| P3 tibia | 0,48 | 0,025 | **Plegada del todo sobre el fémur (≈5°)** | 5–160° | Se extiende de golpe al saltar [P5] |
| P3 tarso | 0,15 | 0,03 | — | ±30° | [A] |

**Salto** (langosta *Schistocerca*): el impulso dura **25–30 ms**, alcanza **3,2 m/s** con una aceleración pico de
**180 m/s²**, y la energía se almacena antes del salto en la cutícula del fémur [P5].

## Grillo (`Cricket`)

Igual que el saltamontes, pero con antenas **más largas que el cuerpo** (≈1,2 L) y cuerpo más cilíndrico [A].

- **Patas traseras**: miden **1,5 veces el cuerpo** y **más de 4 veces las delanteras**.
- **Salto**: la tibia se extiende del todo en **30 ms**, a **13 500 °/s**.
- **Patada**: la tibia se extiende en **10 ms**, a **41 800 °/s**.

Todos estos datos son del grillo de matorral [P6].

| P3 fémur / tibia / tarso | Largo/L | Fuente |
|---|---|---|
| | 0,65 / 0,70 / 0,15 (suma ≈1,5 L) | Total [P6]; reparto [A] |

## Mantis (`Mantis`)

| Segmento | Largo/L | Ancho/L | Reposo | Rango | Fuente |
|---|---|---|---|---|---|
| Cabeza (triangular, ojos grandes) | 0,08 | 0,14 | Mira a la presa | **Yaw ±90°** (gira la cabeza casi de lado a lado), pitch ±40° | [A] |
| Protórax (el "cuello" largo) | 0,30 | 0,06 | 30° hacia arriba | Pitch 0–60° | [A] |
| Resto del tórax + abdomen | 0,62 | 0,12 | — | — | [A] |
| Alas | 0,55 | 0,14 | Plegadas | Se abren 0–90° y se despliegan como amenaza | [A] |
| P1 coxa (alargada) | 0,18 | 0,04 | Hacia delante y abajo | Pitch ±60° | [A] |
| P1 fémur raptor (con espinas) | 0,22 | 0,05 | Vertical, "rezando" | Pitch 0–110° | [A] |
| P1 tibia raptora | 0,12 | 0,03 | **Plegada contra el fémur (≈10°)** | 10–140°; se cierra como una navaja | [A] |
| P2 / P3 fémur / tibia / tarso | 0,22 / 0,22 / 0,10 | 0,02 | Plantilla | Plantilla | [A] |

**Ataque**: es una única secuencia de **las seis patas**. Antes, la mantis gira el cuerpo para poner a la presa en su
plano medio; la secuencia es demasiado rápida para corregirla con la vista [P7]. En el juego: alinear, golpear de una
vez y cerrar la tibia contra el fémur.

## Hormiga león (`Antlion`, larva)

| Segmento | Largo/L | Ancho/L | Reposo | Rango | Fuente |
|---|---|---|---|---|---|
| Cabeza (plana) | 0,14 | 0,18 | Al ras de la arena | Pitch −10° a +60° (lanza arena) | [P8] |
| Mandíbulas en hoz (×2, con dientes) | 0,30 | 0,03 | Abiertas en V, 60° cada una | 0–80°; se cierran sobre la presa | [A] |
| Protórax (cuello estrecho) | 0,08 | 0,08 | — | Yaw ±40° | [A] |
| Abdomen (ovalado, peludo) | 0,62 | 0,48 | Enterrado | — | [A] |
| Patas P1–P3: fémur / tibia / tarso | 0,12 / 0,10 / 0,04 | 0,025 | Recogidas | Plantilla | Camina **hacia atrás** [A] |

Lanza arena a la presa con la cabeza y las mandíbulas como una **catapulta dirigida hacia atrás**. Si la vibración viene
de fuera de ese ángulo, la larva gira por el camino más corto hasta encararla [P8]. El adulto es otro cuerpo (alado,
como un caballito del diablo); si hace falta se añade aparte.

## Termita (`Termite`)

| Segmento | Obrera largo/L | Soldado largo/L | Ancho/L | Rango | Fuente |
|---|---|---|---|---|---|
| Cabeza | 0,20 | **0,40** (esclerotizada) | 0,18 / 0,22 | Pitch ±20° | [A] |
| Mandíbulas | 0,05 | **0,25** (cruzadas) | 0,03 | 0–40° | [A] |
| Antenas en rosario (13–20 artejos) | 0,35 | 0,30 | 0,015 | ±40° | [A] |
| Tórax | 0,22 | 0,18 | 0,14 | — | [A] |
| Abdomen (blando, **sin cintura**) | 0,58 | 0,42 | 0,22 | Pitch ±10° | [A] |
| Patas P1–P3: fémur / tibia / tarso | 0,18 / 0,18 / 0,08 | Igual | 0,025 | Plantilla | Tarso de 4 artejos [A] |

## Abeja de las nieves (`SnowBee`)

Se modela como un abejorro adaptado al frío: cuerpo robusto y peludo [A].

| Segmento | Largo/L | Ancho/L | Reposo | Rango | Fuente |
|---|---|---|---|---|---|
| Cabeza | 0,18 | 0,24 | — | ±20° | [A] |
| Antenas acodadas | 0,30 | 0,015 | Codo a 90° | Codo 40–150° | [A] |
| Tórax (peludo) | 0,32 | 0,38 | — | — | [A] |
| Abdomen (con aguijón) | 0,50 | 0,40 | — | Pitch ±30°; curva hacia abajo para picar | [A] |
| Ala anterior (×2) | 0,65 | 0,22 | Plegadas sobre el abdomen | **Amplitud del batido ≈90°, a ≈230 Hz** (abeja melífera en vuelo estacionario) | [P9] |
| Ala posterior (×2) | 0,45 | 0,15 | Unida a la anterior en vuelo | Se mueve con la anterior | [A] |
| Patas P1–P3: fémur / tibia / tarso | 0,22 / 0,25 / 0,20 | 0,04 | Plantilla | Plantilla | P3 con la tibia ensanchada en "cesta" de polen [A] |

En Unity no hay que animar 230 Hz. Basta un aleteo rápido de amplitud ≈90°, por ejemplo 15–20 Hz visuales con desenfoque.
La amplitud sí es la real; si la abeja carga más peso, la amplitud sube cerca de un 50 % [P9].

## Araña y araña de las nieves (`Spider`, `SnowSpider`)

Las arañas no son insectos. Tienen **8 patas de 7 segmentos**: coxa, trocánter, fémur, **patela**, tibia,
**metatarso** y tarso [A].

| Segmento | Largo/L | Ancho/L | Reposo | Rango | Fuente |
|---|---|---|---|---|---|
| Cefalotórax | 0,45 | 0,38 | — | — | [A] |
| Quelíceros (×2, con colmillo) | 0,12 | 0,06 | Hacia abajo | Colmillo 0–90° | [A] |
| Pedipalpos (×2) | 0,25 | 0,02 | Delante, tanteando | ±40° | [A] |
| Abdomen (opistosoma) | 0,55 | 0,45 | — | Pitch ±20° | [A] |
| Pata (total) | 1,0–1,6 según la especie | 0,03 | Abiertas en estrella; P1 y P2 hacia delante, P3 y P4 hacia atrás | — | [A] |
| Fémur / patela / tibia / metatarso / tarso | 0,28 / 0,10 / 0,24 / 0,24 / 0,10 del largo de la pata | — | Fémur 45° hacia arriba; patela doblada 90° | Fémur–patela 30–170°, tibia–metatarso 0–90° | [A] |

**Extensión hidráulica**: las arañas **no tienen músculos extensores** en dos articulaciones clave, fémur–patela y
tibia–metatarso. Las extienden por **presión de hemolinfa** y las doblan con músculos flexores [P10][P11]. Por eso una
araña muerta o deshidratada queda con las patas **encogidas**: es una pose de muerte realista.

**Marcha**: tetrápodo alterno. Se mueven a la vez P1 izquierda, P2 derecha, P3 izquierda y P4 derecha, y luego las otras
cuatro [A].

## Fuentes (PubMed)

- **[P1]** Tross, Wolf & Pfeffer (2021). *Allometry in desert ant locomotion*. J Exp Biol 224.
  [doi:10.1242/jeb.242842](https://doi.org/10.1242/jeb.242842)
- **[P2]** Alfaress et al. (2018). *Laser surgery reveals the biomechanical and chemical signaling functions of aphid
  siphunculi*. PLoS One 13. [doi:10.1371/journal.pone.0204984](https://doi.org/10.1371/journal.pone.0204984)
- **[P3]** Saito et al. (2017). *Investigation of hindwing folding in ladybird beetles*. PNAS 114:5624.
  [doi:10.1073/pnas.1620612114](https://doi.org/10.1073/pnas.1620612114)
- **[P4]** Dacke et al. (2013). *Dung beetles use the Milky Way for orientation*. Curr Biol 23:298.
  [doi:10.1016/j.cub.2012.12.034](https://doi.org/10.1016/j.cub.2012.12.034)
- **[P5]** Bennet-Clark (1975). *The energetics of the jump of the locust*. J Exp Biol 63:53.
  [doi:10.1242/jeb.63.1.53](https://doi.org/10.1242/jeb.63.1.53)
- **[P6]** Burrows & Morris (2003). *Jumping and kicking in bush crickets*. J Exp Biol 206:1035.
  [doi:10.1242/jeb.00214](https://doi.org/10.1242/jeb.00214)
- **[P7]** Corrette (1990). *Prey capture in the praying mantis*. J Exp Biol 148:147.
  [doi:10.1242/jeb.148.1.147](https://doi.org/10.1242/jeb.148.1.147)
- **[P8]** Lo Bello, De Agrò & Vallortigara (2026). *Antlion larvae follow optimality rules in body orientation during
  sand tossing*. J Exp Biol 229. [doi:10.1242/jeb.252138](https://doi.org/10.1242/jeb.252138)
- **[P9]** Altshuler et al. (2005). *Short-amplitude high-frequency wing strokes determine the aerodynamics of
  honeybee flight*. PNAS 102:18213. [doi:10.1073/pnas.0506590102](https://doi.org/10.1073/pnas.0506590102)
- **[P10]** Göttler et al. (2021). *Fluid mechanics and rheology of the jumping spider body fluid*. Soft Matter 17:5532.
  [doi:10.1039/d1sm00338k](https://doi.org/10.1039/d1sm00338k)
- **[P11]** Liu et al. (2019). *The art of a hydraulic joint in a spider's leg*. J Comp Physiol A 205:491.
  [doi:10.1007/s00359-019-01336-2](https://doi.org/10.1007/s00359-019-01336-2)

**Pendiente de refinar:** las proporciones marcadas [A] salen de anatomía general. Para afinarlas, busca la morfometría
de la especie concreta con los MCP `openalex`/`PubMed` (y `gbif` para el nombre científico); el skill `species-sheet`
explica cómo.
