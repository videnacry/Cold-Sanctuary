# Pijamadas en las casas del árbol

Berón (2026-11-04): «las camas son grandes para permitir que los residentes puedan hacer pijamadas, se podría incluso crear literas
para incrementar el número de personas que pueden participar en la pijamada».

Y luego: «la cama de 4 se convierte en litera, de modo que se mantiene solo una cama por residencia».

## Capacidad de cada casa: 8, en una sola cama-litera

| Piso de la cama-litera | Plazas | Medidas |
|---|---|---|
| Abajo | 4 | colchón de 3,60 × 2,10 m: cuatro plazas de 90 cm; la cara de arriba, a 0,75 m del suelo de la casa |
| Arriba | 4 | otro colchón de 3,60 × 2,10 m; la cara de arriba, a 1,85 m (1,1 m entre colchones: unos 0,8 m libres para sentarse abajo) |
| **Total** | **8** | `Bedroom.Capacity` |

Es **una sola pieza por casa** (la «Cama»), en x −4,45…−0,75 y z 1,15…3,45 (casa de 9 × 7 m):
- armazón de 3,70 × 2,30 m en cada piso;
- cabecero alto en +z;
- cuatro postes de 2,1 m;
- barandilla en los lados abiertos de arriba.

La litera triple aparte que hubo un momento ya no está. Su sitio, delante de la cama, queda libre: es donde se juntan los de la
pijamada (`Bedroom.gather`, 8 puntos en dos filas). Los 2 m de paso se mantienen:
- la cama queda a 2,15 m de la alfombra-esterilla;
- la mesilla va pegada a la cama;
- el armario queda a 2,1 m de la mesilla.

**Sin escalera** (reglas de plataformas, regla 3):
- Al piso de arriba se sube **trepando por el poste del pie** (`Climbable`, como a los árboles).
- A dormir, cada uno se tumba en su plaza: `SleepSpell` lo pone sobre su colchón.
- No hay peldaños ni piezas «Escalera».

**Plazas** (`Bedroom.spots`, un punto 0,5 m por debajo de la cara de arriba de cada colchón):
- primero las 4 de abajo, luego las 4 de arriba;
- **los que viven allí duermen abajo**: el dueño en la segunda plaza y su pareja en la tercera;
- **los invitados llenan primero las plazas libres de abajo** (la primera y la cuarta) y después las de arriba.
## La pijamada

- **Cuándo**: una vez por semana, el **viernes en la franja libre de las 21 h** (`Sleepover.Day`, `Slot`).
- **Dónde**: en la casa de un anfitrión. El anfitrión **rota cada semana** por las casas del árbol, en su orden (`LeisureRoster.HostIndex`).
  - Pueden ser anfitriones las casas cuyo dueño está en el horario libre.
  - Quedan fuera Kushal (el jugador es libre) y la Magnate.
- **Quién**:
  - el anfitrión y su pareja (`Activity.hosts`), que van siempre;
  - hasta llenar las 8 plazas, los que menos pijamadas llevan (`LeisureRoster`, actividad `together`).
- **En la franja**:
  - cada uno va a la casa del anfitrión en alfombra, por la red de la Residencia, y se sienta a charlar delante de la cama-litera
    (`Bedroom.gather`);
  - al llegar se apunta a dormir allí (`Sleepover.Join`). A cada invitado le toca la primera plaza libre: las de abajo que no usan
    los que viven allí y luego las de arriba.
- **Esa noche**: `Bedroom.Of` devuelve la casa del anfitrión. Cuando tienen sueño, van a su plaza (`NeedsDriver`) y duermen allí.
- **A la mañana siguiente**, desde las 8:00 (`Sleepover.WakeHour`), la pijamada se olvida sola y cada uno vuelve a su casa.

La pijamada es una actividad **por turnos** (regla J16): caben 8 por casa, no todos los miembros. Para todos a la vez está el ring de
almohadas.

## Prueba: `SleepoverTest` (grupo 92, SampleScene)

1. Cada casa duerme a 8 en una sola cama-litera de dos pisos de 4 (sin litera aparte). Se sube al piso de arriba por su poste, no hay escaleras, y queda a ≥ 2 m de la alfombra.
2. El horario:
   - la pijamada está en el catálogo, con el anfitrión y su pareja;
   - no pasa de 8;
   - el anfitrión de la semana siguiente es otro.
3. En vivo:
   - van a la casa del anfitrión (≥ 80 %), andando y en alfombra, sin saltos;
   - cada invitado se apunta y tiene su plaza: primero las libres de abajo, luego las de arriba;
   - esa noche duermen en su plaza (≥ 80 %);
   - a la mañana siguiente vuelven a tener su casa.
