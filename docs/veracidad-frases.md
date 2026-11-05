# Veracidad de las frases (regla M39, 2026-11-05)

Berón: «las frases se pueden conservar pero como regla general se deben tener la property que ofrezca fuentes para saber si tiene nivel de veracidad o es inventado».

## Los niveles

| Nivel | Qué significa |
|---|---|
| **Documentada** | Palabras reales, con su documento |
| **Atestiguada** | La dicen testigos o crónicas; no hay documento exacto |
| **Recreada** | Frase de juego de una persona real, sobre hechos de su ficha |
| **Inventada** | La dice un personaje inventado o una voz de juego |

## Cómo funciona

- `StoryMission.StoryStep` lleva `truth` y `truthSource`; la misión, `finalTruth` y `finalTruthSource`.
- `LineTruth.Fill` los pone. Mira primero `LineTruth.Quoted` (frases con base, por su comienzo). Si habla alguien de `LineTruth.Real`, es Recreada, con la primera fuente de la ficha. Si no, Inventada.
- Corre al construir y **al arrancar la escena**: las escenas ya construidas la tienen sin reconstruirse.
- **El jugador lo ve** en la nota de cada frase («… [recreada]») y, en la ficha final, con la tecla **V**, la lista de frases con nivel y fuente.
- **Todas las historias con `StoryMission`**, también la música y los vivos no políticos, que conservan sus frases, marcadas como recreadas.

## Lo que queda fuera

- Las charlas de la gente (J7) no llevan nivel: son de plantilla. Pendiente.
- `Quoted` son 19 frases. El resto de la música sale como recreada (ver abajo).
- **Un nombre que falte en `Real` saldría «inventada»** y la prueba no lo vería, porque solo mira los que están en la lista. Revisé la lista de todos los que hablan, una vez; un nuevo personaje real debe añadirse.

## Historias sin `StoryMission` (2026-11-05)

`LineTruth.Scripted` tiene una fila por historia, porque todas sus frases son de un mismo nivel. **Ninguna es una cita**, así que todas son inventadas, con esa fuente.

| Historia | Dónde se ve |
|---|---|
| Le Chambon y Sendler (`CompassionMission`) | En la ficha, con V |
| Egina (`MythMission`) | En la ficha, con V, y en cada frase |
| Sakshi, Ambrosio, Cocina | En cada subtítulo («… [inventada]»). No tienen ficha. El narrador no lleva marca |

## La música: lo que tiene fuente (2026-11-05)

Solo cuenta la **primera frase** de la línea; lo demás sigue siendo recreado. No se citan letras de canciones.

| Frase | Base | Nivel |
|---|---|---|
| Lennon: vender la paz como el jabón | [The David Frost Show, 1969](https://lifeofthebeatles.blogspot.com/2024/06/the-david-frost-show-june-14-1969.html); [Canadian Press](https://beatles.ncf.ca/paxromana.html). La redacción varía | Atestiguada |
| Yoko: las bellotas a los jefes de Estado | [La carta, parafraseada](https://mentalfloss.com/article/548071/when-john-lennon-and-yoko-ono-mailed-acorns-world-leaders) | Atestiguada |
| Marley: «tenemos que estar juntos» (**frase añadida** a la línea) | [One Love Peace Concert](https://en.wikipedia.org/wiki/One_Love_Peace_Concert). Sin transcripción comprobada | Atestiguada |
| Chuck D: el avión sobre Italia | [Rolling Stone](https://www.rollingstone.com/feature/riot-on-the-set-how-public-enemy-crafted-the-anthem-fight-the-power-244152/) | Atestiguada |

**Corregido**: Gabriel decía «oí por la radio»; las fuentes solo dicen [las noticias](https://en.wikipedia.org/wiki/Biko_(song)). Ahora dice eso y sigue como recreada.

**Sin fuente, siguen recreadas**: Shakira (no encontré su fundación ni el parque), Bono (no encontré sus palabras exactas en la sala), Trimble, Hume, Tosh, Baca y el resto de Lennon y Marley.

## Pruebas

`MicrocosmosFloorTest` (43) mira en cada escena que toda frase tenga nivel y fuente y que nadie de `Real` salga como inventado. `AsambleaTest` (77) prueba la clasificación sin escenas.
