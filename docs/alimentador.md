# El alimentador: ronda de los depredadores

Berón (2026-11-05): «retira el comedero, crea la tarea de alimentador (supongo que sería parte de ser guardia?) que traquearía a los depredadores y cuando
los encuentra evalúa si tienen hambre y los alimenta o los marca y va por el siguiente, se puede resolver en grupo porque comparten la misma lista con lo que
saben que depredador ya fue checkeado/alimentado».

## Qué hace

- **No hay comedero.** El laboratorio guarda las raciones en `Almacen_raciones`, en su piso, y no al aire: una ración suelta atraía a los carnívoros.
- **Misiones:** `gua_alimentador` (obligatoria) y `gua_alimentador_2`, en el área Guardia. Sin papel exigido: entran por la rotación, como todo. El uniforme es el de guardia.
- **La ronda (`PredatorRound`, cada 8 h de juego):** la lista son todos los carnívoros de tierra del santuario, sin crías, sin marinos y sin los de la Cría.
- **El reparto:** cada alimentador se reserva el pendiente más cercano. Nadie más va a por él, así un grupo se reparte el trabajo solo. Si alguien deja la misión, su reserva vuelve a la lista.
- **Al llegar (`PredatorFeeder`):** evalúa `hungry`. Con hambre lanza su ración; sin hambre, lo marca. Sin raciones, lo apunta y va a por más.
- **Límite:** 90 s siguiendo a uno; luego «no alcanzado» y al siguiente.
- **Jugador:** recoge en el almacén y va a los pendientes de la lista; cada E atiende a uno. Si el animal se ha movido, atiende al pendiente más cercano.
- **El guardia** (`CarnivoreKeeper`, `gua_carnivoros`) solo patrulla los puestos.

## Seguridad: un oso no es un perro

| Fuente | Qué dice |
|---|---|
| Svalbard (Ley de Medio Ambiente) | 300 m del oso, 500 m del 1 de marzo al 30 de junio; prohibido atraerlo |
| Parks Canada | No acercarse; ir en grupo y con vigía de osos; dar de comer está prohibido, porque liga personas y comida |
| USFWS | Vigías, sistema de parejas, vehículo o edificio como refugio |
| Zoos (AZA, NC Zoo, ABQ BioPark) | Contacto protegido: el cuidador nunca comparte espacio; la comida entra por una trampilla |
| Polar Bears International | Ningún país tiene programas formales de alimentar osos salvajes |

**Lo que eso significa aquí.** En la naturaleza esta misión estaría prohibida. El santuario es más parecido a un zoo enorme que a la naturaleza, así que tomamos
la regla de los zoos: nunca el mismo espacio. No hay vehículo porque el mapa no lo tiene a ras de suelo; hay distancia:
- **Oso 18 m, lobo 10 m, resto 5 m (`SafeDistance`).** Los 300 m reales no caben en un mapa de 500 m. La escala es del juego, no una recomendación.
- **La ración se lanza** y cae a 3 m del animal, del lado del alimentador. Nunca se da en la mano.
- **Si se le acerca a menos de la mitad de su distancia,** se retira, sin ración, y lo apunta.
- **Pendiente:** ir de dos en dos con el oso, y una alfombra como plataforma. Se reparten la lista, pero no van juntos.

**Riesgo de diseño, y Berón debe saberlo:** lanzar comida cerca de personas es justo lo que crea osos habituados. El juego no modela la habituación. Si algún día
lo hace, la ronda debería pasar a dejar la comida sin que el animal vea quién.

## Los osos sin comedero

- Sin punto fijo se reparten por su territorio y no se concentran en un sitio: el alimentador los sigue.
- Con hambre y sin ración, cazan (`Forager`), como cualquier depredador.
- **El laboratorio pasa de 2 a 16 kg por lote** (`batchGrams`; 32 raciones) y el almacén admite 40. Con 2 kg no alcanzaba: un oso come 6 kg al día. Se supone un lote al día.
- **`FoodChain` ya no supone el 80 %:** `LabShare` = lo que da el laboratorio entre lo que necesitan los depredadores de tierra. Estimación a mano: ~58 % con 3 osos, 3 lobos y 2 zorros. El resto lo cazan o lo pescan, y suben las presas si hace falta. El número real sale en el log `[Cadena]`.
