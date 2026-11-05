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

- `CompassionMission` (Le Chambon, Sendler) y Egina tienen su propia misión; las historias guionizadas (Sakshi, Ambrosio, Cocina) no usan `StoryMission`; tampoco las charlas de la gente (J7), que son de plantilla. Pendiente.
- `Quoted` son 15 frases: las citas que yo identifiqué. La música puede tener más (letras, declaraciones): las de esa rama salen como recreadas hasta que alguien las añada.
- **Un nombre que falte en `Real` saldría «inventada»** y la prueba no lo vería, porque solo mira los que están en la lista. Revisé la lista de todos los que hablan, una vez; un nuevo personaje real debe añadirse.

## Pruebas

`MicrocosmosFloorTest` (43) mira en cada escena que toda frase tenga nivel y fuente y que nadie de `Real` salga como inventado. `AsambleaTest` (77) prueba la clasificación sin escenas.
