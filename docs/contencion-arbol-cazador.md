# El árbol cazador, bajo el agua (2026-11-05)

Berón: «el árbol cazador queda solo en un piso específico de investigación de modo que conserva todas sus características» y «quizás el piso del árbol carnivoro se podría realizar bajo el agua? el enjambre no podría salir de alli […] se supone que el árbol se está diseñando para ser autosustentable y require de las abejas y arañas para que asi sea no?».

Código: `World/HunterCell.cs` (`HunterRule`, `HunterLoop`, `HunterCell`, `HunterLab`), `SampleSceneBuilder.HunterLab.cs`. Regla S16. Prueba: grupo 91.

## Dónde

**En el Santuario Marino**, en una sala propia en la roca, a 46 m de las salas nucleares por una galería, con 60 m de roca encima.

| Opción | Por qué |
|---|---|
| **Santuario Marino** | Elegida. Solo se carga cuando baja Kushal, así que el árbol, las colmenas y los bichos no pesan arriba. Ya tiene línea, refugio y esclusas |
| Cámara sumergida en el santuario 1 | Pesaría siempre, y un fallo daría al mar junto a las torres |
| Lo alto de Producción | Era la anterior. Se quita: la torre vuelve a 2 niveles |

## Las puertas al mar: lo que es verdad y lo que no

| Palabra | La verdad |
|---|---|
| **Moon pool** | Es una abertura al agua en el suelo, como en el hábitat Aquarius: el aire de dentro, a la presión del mar, sujeta el agua. **Aquí no se puede**: a 57 bar nadie vive en esa presión. Fuente: [Moonpool](https://www.wikiwand.com/en/articles/Moonpool) |
| **Cámara de salida** | Lo que sí se usa: la sala a una atmósfera, y una cámara que se inunda para salir. Es lo construido |
| **«Puertas magnetizadas»** | En el juego, el `CampoMagnetico` de las puertas de los troncos es una lámina que sujeta el agua. **Eso es ficción**: un campo magnético no sujeta el agua de mar. Lo que existe son cierres y acoplamientos magnéticos en una escotilla. Aquí es eso: una escotilla con cierre magnético. No encontré fuente sobre ellos: **[verificar]** |
| **El agua como barrera** | Es lo que para al enjambre: la salida es un pozo lleno de agua. Una abeja no vuela por el agua. Es razonamiento, no un dato con fuente |

## De dentro afuera

Celda de cristal → antesala → esclusa de aire → cámara de salida inundada → pozo inundado al lecho.

Un enjambre puede llegar a la antesala en una brecha, y ahí acaba. Lo real ya exige más que un cristal: [en invernaderos se escapan abejorros por puertas de doble red](https://conservationevidencejournal.com/actions/40), y por eso la celda sola no basta.

## Lo que conserva

Señuelo de 70 m, miel letal a 12 m, **cualquier presa** menos sus abejas y arañas, crecimiento a ×1,5, **de 3 a 6 panales y 2 arañas**. El cuerpo sigue en 13 m: a 21 m de celda es lo que cabe.

El sopor deja fuera a abejas y arañas. Es un cambio: en el bosque nada las excluía, y aquí se comerían su propio ciclo.

## El ciclo y el balance (`HunterLoop`)

Berón: el árbol se alimenta sobre todo de «cadáveres de sus pobladores insectos». Lo comparé con §2 de [ice-sanctuary-ecology.md](ice-sanctuary-ecology.md).

| Diseño original | Lo que había | Ahora |
|---|---|---|
| Vive de agua y frío (célula vegetal + psicrófilo); los nutrientes salen de los muertos | Un ciclo con cifras sueltas | Se cuenta el nitrógeno: lo demás lo da el agua, el frío y la luz |
| La **miel letal** es lo que mata: comer, sueño del que no se despierta | Sopor por cercanía | Quien llega con hambre come y entra en letargo hasta morir. Sin hambre, solo adormece |
| Las abejas **hacen los nidos con los muertos** y el árbol absorbe de ellos | Nutrientes sueltos | Los muertos van a los nidos y el árbol absorbe de ahí, solo si hay abejas |
| Más nutrientes → **ramas más largas** → más nidos | Crecía por una cifra diaria | El tamaño sale del nitrógeno del árbol, y los nidos, de 3 a 6, del tamaño: se encienden en la escena |
| Rastro `Nectar` | `ScentFood` | **Sigue siendo `ScentFood`**: es el canal que siguen los animales |

**Fuente principal**: las muertes de sus abejas y arañas, que circulan. **Lo nuevo**: el simbionte, que fija el aire. **Extra**: lo que caza su miel. **Reserva**: `cuar_racion`, solo si el ciclo pierde demasiado.

### ¿Las muertes sostienen el árbol?

**Solas, no**: sus insectos hacen circular el nitrógeno y pierden el 3 % en cada vuelta. Sin nada de fuera, en 10 años baja a ×0,92 y en unas décadas se apaga.

### El simbionte: lo que entra de verdad

Berón: «agrega dicho microbio». Es el psicrófilo del diseño, en nódulos de la raíz. No se inventa nada nuevo: es un *Frankia* de aliso adaptado al frío.

| Quién fija | kg N/ha al año | Sobre sus 81 m² | Fuente |
|---|---|---|---|
| Musgo con *Nostoc* | 1,5–2 | ~0,016 kg | [DeLuca et al., Nature 2002](https://ideas.repec.org/a/nat/nature/v419y2002i6910d10.1038_nature01051.html) |
| **Aliso en Alaska, boreal** | 0,06–11,5, más de 100 en vegas | **0,09 kg: la elegida** | [Estudio de alisos](https://par.nsf.gov/servlets/purl/10212884) |
| Aliso en Quebec | 53 | 0,43 kg | [Revisión](https://www.ejes.cz/index.php/ejes/article/view/1466) |
| Alisos en general | 40–300 | 0,3–2,4 kg | La misma |

- **Frío**: en la tundra de Groenlandia la fijación máxima se midió [por debajo de 14 °C](https://openpolar.no/Record/crwiley:10.1002%2Fecs2.2117). Los fijadores árticos estudiados son más bien tolerantes que psicrófilos: óptimo sobre 20 °C, lentos a 5 °C.
- **Energía**: 16 ATP por N₂, y [unos 4 g de carbono por g de N](https://lab.igb.illinois.edu/long/sites/lab.igb.illinois.edu.long/files/2023-09/New%20Phytologist%20-%202023%20-%20Holland.pdf) (2,8–4,8), medido en soja. Son ~0,6 kg de C al año, menos del 1 % de los ~110 kg que estimo que dan sus 30 kW de luz. No hace falta más energía. No encontré esa cifra en fijadores fríos.
- **De dónde sale el N₂**: del aire. El 78 % del aire de la celda son ~5,9 t de N₂, para miles de años. Sellada no significa sin aire: está llena, y los filtros renuevan el de la sala.
- El abono frena la fijación: aquí se para con nitrógeno de sobra en los nidos.

### El balance con el simbionte

Modelo propio, sin ninguna entrega y con abejas de 237 g:

| Fijación (kg N/ha al año) | A los 10 años | Tope ×1,5 |
|---|---|---|
| 0 | ×0,92, y se apaga | Nunca |
| 2 (musgo) | ×1,01, y se apaga más tarde | Nunca |
| **11,5 (la elegida)** | **×1,37, 36 abejas, 5 nidos** | **A los ~15 años** |
| 30 | ×1,5, 72 abejas | ~4 años |
| 53 | ×1,5 | ~2 años |
| 30, reciclando el 90 % | ×1,5 | ~5 años |

**Respuesta**: **sí es autosuficiente** con un fijador tipo aliso frío, llega a ×1,5 (unos 20 m, el tope de la celda) en unos 15 años, con 72 abejas y 6 nidos, sin pedir entregas. Con uno tipo musgo, no.

**Las abejas siguen haciendo falta**: sin ellas no se absorbe de los nidos, y el árbol baja a ×0,57 en 10 años aunque tenga el simbionte.

**Con cuidado**:

- La incertidumbre es enorme: las cifras reales van de 0,06 a más de 100, y son de un rodal, no de un árbol en una bandeja. El resultado va de «nunca» a «2 años».
- El resto también son estimaciones: ~300 kg secos, 2,5 % de N, el 97 % de reciclado, 40 días de vida.
- La abeja pesa 237 g, por decisión de Berón. `Physiognomy` sigue diciendo 1 g.
- El tope ×1,5 lo pone la celda, no la biología.
- Las abejas buitre existen y comen carroña: de memoria, **[verificar]**. A esa hondura no llega el sol: tiene luces de cultivo.

## Las normas

| Norma | Origen | Fuente |
|---|---|---|
| Acceso restringido, descontaminar lo que sale y manual de prácticas | BL3-P | [Guías NIH, Apéndice P](https://webarchive.library.unt.edu/eot2008/20090131001310/http:/oba.od.nih.gov/oba/rac/guidelines_02/APPENDIX_P.htm) |
| Nadie comparte espacio con él | Contacto protegido | [Cheyenne Mountain Zoo](https://cmzoo.org/news/archive/tag/zoo-information/page/7) |
| Doble puerta | Zoos | [Cleveland Zoo](https://www.clevelandzoosociety.org/Z/2020/03/16/ask-the-expert-how-do-the-keepers-get-into-the-habitats-without-the-animals-coming-out) |
| Simulacros y código de escape | Zoos | [ASIS](https://www.asisonline.org/link/db5bf6ea36204592be7c784764586bf3.aspx) |

**[verificar]**: la presión negativa no salió en las búsquedas. El simulacro semanal es elección mía.

## Misiones

La lógica vive arriba, en `HunterCell`, junto a la línea, como `CavernRock`. Sin cargar la escena. Quien baja usa `SiteLine.Board`, como `nuc_bajada`, y Kushal es el único que carga lo de abajo.

| Misión | Dónde | Cuándo |
|---|---|---|
| `cuar_ronda` (obligatoria) | **Sala de guardia**, en remoto | Cada día |
| `cuar_racion` | Línea | **Reserva**: restos de un animal muerto, si el ciclo pierde demasiado. Con el simbionte, casi nunca |
| `cuar_muestra` | Línea | Semanal: poda |
| `cuar_simulacro` | Línea | Semanal |
| `cuar_recaptura` y `_2` | Línea | Solo en una brecha |

Si la línea está cerrada, solo queda la ronda.

## La emergencia: código verde

- **Empieza** tras más de 3 días sin ronda, o por un fallo de sello, cada ~60 días desde el día 30.
- **Mientras dura**: el árbol alcanza a cualquiera en la sala y el enjambre sale a la antesala, sin pasar del agua. La pantalla de la sala de guardia se pone roja, y solo queda la recaptura.
- **Acaba** con **dos personas**, o al día siguiente, porque las compuertas cierran solas. Eso cubre el caso de la línea cerrada.

El piso ya no da miedo arriba: **una brecha no llega al santuario 1**. Por eso el árbol nunca alcanza a nadie si Kushal no está abajo.

## Riesgos

- Sin probar en vivo.
- Las colmenas y bichos de abajo solo corren con la escena cargada. Lo que se ve no cuadra exactamente con `HunterLoop`: la pantalla manda.
- Kushal puede andar hasta la sala, pero sus tareas son las genéricas: aún no hay interacción con portillo y guantes.
- La sala no entra en el control de la roca (`CavernRock`), y su pilar con las salas nucleares es una decisión a ojo.
- Una celda de 21 m de cristal a 57 bar no sería de cristal: va dentro de la sala a una atmósfera, que es la que aguanta.
