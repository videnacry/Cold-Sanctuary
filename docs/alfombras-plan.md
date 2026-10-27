# Plan: transporte con alfombras voladoras (sin escaleras ni puentes)

Pedido de Berón (2026-10-27). Este documento recoge cómo queda el santuario y qué cambia en el código.

## Lo que queda

**Escaleras y bordes.**
- Solo conservan escalera las **plataformas hijas**: los anillos (`RingDeck`) y las entreplantas (`Deck`). Son:
  - el comedor del restaurante;
  - los anillos de camas del hospital y la subplataforma de aislamiento;
  - el anillo de experimentos de Física;
  - la entreplanta del gimnasio.
- Se quitan todas las escaleras verticales (`Ladder`):
  - las de las áreas elevadas, los pisos superiores, los huertos de gremio y las pistas;
  - las de la residencia y la casa de árbol del guarda.
- Se quitan las barreras invisibles de los bordes, para que se pueda saltar de una plataforma a otra. El cojín de musgo de abajo se queda, para que la caída no haga daño.

**Puentes.** Se quitan todos: los puentes entre áreas, los de pista y las pasarelas de la residencia.

**Pisos.**
- Todos los pisos de un edificio miden lo mismo que la planta baja (`StackOn` sin retranqueo). Por ejemplo, el hospital pasa a tener 3 pisos de 32×26.
- Cada piso tiene un **área de alfombras de edificio**: una zona marcada en el suelo, de unos 7×4 m, con 2 alfombras. Desde ella se va a cualquier otro piso del mismo edificio, incluido el techo y el suelo de hielo de abajo.

**Techo de cada edificio: el piso de alfombras.**
- Es un piso más, del mismo tamaño que los demás, con dos áreas:
  1. **Alfombras de edificio**, para ir a los pisos de este edificio.
  2. **Alfombras de santuario**, para ir al techo de cualquier otro edificio o a la plataforma central de la residencia.
- Así el selector de cada alfombra muestra pocas opciones: los pisos de un edificio, o los edificios.
- Los edificios que hoy no tienen pista sobre el techo también la tienen: Huerto, Planta Atómica, Química, Biología y Central Nuclear.

**Suelo.** Al pie de cada edificio hay un área de alfombras de edificio en el hielo (la «planta 0»). Desde ahí se sube a cualquier piso, porque ya no hay escalera.

**Residencia: casas de árbol.**
- Cada habitación es una **casa de árbol**, en árboles distintos o en el mismo árbol a distinta altura.
- Ninguna tiene escalera ni puente.
- Cada casa tiene **su alfombra**, que también es su **esterilla de yoga**: se puede hacer yoga en casa. La alfombra solo va a la plataforma central.
- La **plataforma central** tiene dos áreas:
  1. **Alfombras de residencia**: una por habitación, cada una con el nombre de su dueño.
  2. **Alfombras de santuario**: a los techos de los edificios.

**Ejemplo: un trabajador del hospital (piso 1) vuelve a casa.**
1. En el área de alfombras de su piso toma una alfombra de edificio hasta el techo.
2. En el techo camina a las alfombras de santuario y toma la de «Residencia».
3. Llega a la plataforma central, camina a las alfombras de residencia y toma la de su habitación.
4. Para volver lo hace al revés.

## Cambios en el código

| Pieza | Cambio |
|---|---|
| `Levels.StackOn` | `SetBack` = 0: pisos iguales. Se revisan `Research.cs:286`, el hospital y `UpperGarden`. |
| `SampleSceneBuilder.Elevate` | Sin `Ladder.Build` ni `Barandilla_Barrera`. Se queda el cojín de musgo. |
| `Transport.BuildBridgesAndPads` | Sin puentes. Por edificio: un área de alfombras de edificio en cada piso y en el suelo, y un piso-techo con las dos áreas. Desaparecen `_landings` y `ClearBridgeLandings`. |
| `CarpetPad` | Gana `building` (el edificio), `kind` (Edificio / Santuario / Residencia / Habitación) y `floor`. `Route` sigue la cadena piso → techo → techo destino → piso, caminando dentro de cada piso. |
| `FlyingCarpet.Offer` | Solo ofrece los destinos de su tipo: pisos del edificio, edificios, o habitaciones. |
| `ResidentialBuilder` | Casas de árbol a distintas alturas, sin pasarelas ni escaleras. Cada una con su alfombra-esterilla y la plataforma central con sus dos áreas. |
| `NeedsDriver` (ir a la cama) | Usa las alfombras como la jornada (`CarpetPad.Route`). |
| Restaurante | El comedor (anillo, plataforma hija) conserva sus escaleras interiores. El turista llega en alfombra al área de la cocina. |
| `GuardTower` | Sin escalera: alfombra en el suelo y en la casa del guarda. |
| Tests | Los de escaleras (IslandsTest), puente cocina–hospital y comedor desde el puente se cambian por: cada edificio tiene techo de alfombras con dos áreas, cada piso tiene su área, y una alfombra lleva a un trabajador de su piso a su habitación. |

## Orden de trabajo

1. Pisos iguales, sin escaleras ni bordes ni puentes.
2. Áreas de alfombras por piso y techo, y rutas de la gente.
3. Residencia en casas de árbol con sus alfombras-esterilla.
4. Tests y prueba jugando en Unity, con fotos.
