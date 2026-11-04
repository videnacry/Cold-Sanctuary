# Pijamadas en las casas del árbol

Berón (2026-11-04): «las camas son grandes para permitir que los residentes puedan hacer pijamadas, se podría incluso crear literas
para incrementar el número de personas que pueden participar en la pijamada».

## Capacidad de cada casa: 7

| Dónde | Plazas | Medidas |
|---|---|---|
| Cama de 4 | 4 | colchón de 3,60 × 2,10 m: cuatro plazas de 90 cm ([guerra-de-almohadas.md](guerra-de-almohadas.md) §Camas de 4) |
| Litera triple | 3 | tres colchones de 0,90 × 2,00 m con la cara de arriba a 0,65 · 1,55 · 2,45 m del suelo de la casa; postes de 2,6 m; barandillas en los dos de arriba |
| **Total** | **7** | `Bedroom.Capacity` |

La litera está delante de la cama (casa de 9 × 7 m), en x −3,05…−2,15 y z −3,2…−1,2. Respeta los 2 m de paso:
- a 2,35 m de la cama;
- a 3,3 m de la alfombra-esterilla;
- el armario queda al otro lado de la casa.

**Sin escalera** (reglas de plataformas, regla 3):
- A las plazas de arriba se sube **trepando por su poste** (`Climbable`, como a los árboles).
- A dormir, cada uno se tumba en su plaza como en la cama: `SleepSpell` lo pone sobre su colchón.
- No hay peldaños ni piezas «Escalera».

**Plazas** (`Bedroom.spots`, un punto 0,5 m por debajo de la cara de arriba de cada colchón):
- primero las 4 de la cama, luego las 3 de la litera;
- el dueño duerme en la segunda plaza de la cama y su pareja en la tercera.

## La pijamada

- **Cuándo**: una vez por semana, el **viernes en la franja libre de las 21 h** (`Sleepover.Day`, `Slot`).
- **Dónde**: en la casa de un anfitrión. El anfitrión **rota cada semana** por las casas del árbol, en su orden (`LeisureRoster.HostIndex`).
  - Pueden ser anfitriones las casas cuyo dueño está en el horario libre.
  - Quedan fuera Kushal (el jugador es libre) y la Magnate.
- **Quién**:
  - el anfitrión y su pareja (`Activity.hosts`), que van siempre;
  - hasta llenar las 7 plazas, los que menos pijamadas llevan (`LeisureRoster`, actividad `together`).
- **En la franja**:
  - cada uno va a la casa del anfitrión en alfombra, por la red de la Residencia, y se sienta a charlar en el pasillo entre la cama y la
    litera (`Bedroom.gather`);
  - al llegar se apunta a dormir allí (`Sleepover.Join`). A cada invitado le toca la primera plaza libre: las de la cama que no usan
    los que viven allí y luego las de la litera.
- **Esa noche**: `Bedroom.Of` devuelve la casa del anfitrión. Cuando tienen sueño, van a su plaza (`NeedsDriver`) y duermen allí.
- **A la mañana siguiente**, desde las 8:00 (`Sleepover.WakeHour`), la pijamada se olvida sola y cada uno vuelve a su casa.

La pijamada es una actividad **por turnos** (regla J16): caben 7 por casa, no todos los miembros. Para todos a la vez está el ring de
almohadas.

## Prueba: `SleepoverTest` (grupo 91, SampleScene)

1. Cada casa duerme a 7. La litera se sube por su poste, no hay escaleras, y queda a ≥ 2 m de la cama y de la alfombra.
2. El horario:
   - la pijamada está en el catálogo, con el anfitrión y su pareja;
   - no pasa de 7;
   - el anfitrión de la semana siguiente es otro.
3. En vivo:
   - van a la casa del anfitrión (≥ 80 %), andando y en alfombra, sin saltos;
   - cada invitado se apunta y tiene su plaza;
   - esa noche duermen en su plaza (≥ 80 %);
   - a la mañana siguiente vuelven a tener su casa.
