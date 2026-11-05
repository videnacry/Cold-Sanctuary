# La lanzadera de cohetes (investigación → plan → construcción, Berón 2026-11-04)

Pedido de Berón: «crea el piso de cohetes… ¿como casquete artificial distanciado del santuario? La plataforma debe estar preparada
para que en caso de fallo pueda contener el daño sin afectar al medioambiente… ¿una plataforma artificial con un árbol-edificio
preparado especialmente para sostener cualquier explosión y mantenerla alta? Sería más seguro y respetuoso si se encontrase mucho
más arriba que todos los demás árboles, para no afectar con ruido ni ondas de impacto… ¿a qué altura tendría que crearse para no
afectar a los pájaros ni a los pisos más altos de los edificios-árbol? Debe crearse la regla para que siempre que se agrega un
elemento elevado se valide y se ajuste la altura del lanza-cohetes.»

Código: `Assets/Scripts/World/LaunchSite.cs` (`LaunchHeightRule`, `ElevatedElement`, `LaunchSite`),
`Assets/Editor/SampleSceneBuilder.Launch.cs` (la construye), `ConstructionSubmarine.SendTo` (el submarino a un piso cualquiera),
`LaunchSiteTest` (grupo 90).

## 1. Investigación

### El cohete
Un lanzador **pequeño de la clase Electron** (Rocket Lab): 18 m de alto, 1,2 m de diámetro, ~13 t al despegar, **LOX/RP-1** (~10 t de
propulsante). Es lo que cabe en un santuario: un lanzador mediano (Falcon 9, ~500 t de propulsante) pediría distancias de kilómetros.

### Explosión: distancias de seguridad
- **Equivalencia en TNT**: para situar lanzaderas de LOX/RP-1, la FAA usa un **20 %** del propulsante (14 CFR 420, tabla E-2; guía
  de zonas de seguridad de la FAA). 10,2 t × 20 % = **2 t de TNT**.
- **Distancia de edificio habitado (IBD)**: K40 (pies/libra^1/3), que corresponde a una sobrepresión de **~1,2 psi** (8 kPa): rotura de
  cristales y daños leves (DoD / NDIA). En métrico, 15,9 m·kg^-1/3 × (2040 kg)^1/3 = **201 m**.
- Para cantidades pequeñas, **los cascotes** pueden mandar sobre la sobrepresión (NDIA): se añade un 50 % → **300 m**.

### Ruido
- Cerca del chorro de un cohete grande, **170–175 dB** (Gee, Lubert y James, *Physics Today*). El **diluvio de agua** quita **3–5 dB**
  absorbiendo el sonido con burbujas; en el transbordador dejó la plataforma en **~142 dB**. Los modelos pueden quedarse cortos: en
  Artemis I se midieron casi **20 dB más** de lo previsto a ~5 km (BYU).
- Un cohete pequeño suena bastante menos, pero a 300–500 m sigue siendo un estruendo (del orden de 120–130 dB en el pico). **Ninguna
  distancia que quepa en el mapa lo deja «sin ruido»**: lo que se hace en la realidad es **avisar, cerrar la zona y meter a la gente
  dentro** durante la ventana de lanzamiento. Lo apuntamos como norma de la jornada (pendiente de su aviso en el juego).

### Aves
- En aviación, el **~72 %** de los choques con aves ocurre por debajo de **500 pies (152 m)** y el 92 % por debajo de 3000 pies (914 m)
  (boletín USDA/FAA; estudios de colisiones civiles).
- Las aves marinas antárticas (petreles, págalos) vuelan mayormente bajas sobre el mar (no encontramos medidas publicadas de su altura de
  vuelo; queda como pendiente). Las aves migratorias pueden ir a varios km: **por encima de todas las aves no se puede**. La regla usa
  la franja de 152 m (donde vuela la mayoría) y el techo de vuelo de las aves del juego si es mayor.

### Fuentes
- [FAA — Guide to calculating safety clear zones](https://www.faa.gov/sites/faa.gov/files/space/legislation_regulation_guidance/Guide-Cal-of-Safety-Clear-Zones.pdf)
- [NDIA / DTIC — explosives safety, IBD y K40](https://ndia.dtic.mil/wp-content/uploads/2018/intexpsafety/Luna1Paper.pdf)
- [Gee, Lubert y James, *Physics Today* — el ruido de los cohetes](https://aip.brightspotcdn.com/PTO.v77.i3.46_1.online.pdf)
- [Wikipedia — Sound suppression system](https://en.wikipedia.org/wiki/Sound_suppression_system)
- [USDA — bird strikes (altitudes)](https://www.fs.usda.gov/sites/default/files/media_wysiwyg/ia_ll_18-04_bird_strikes.pdf)
- [AOPA — encounters of the feathered kind](https://www.aopa.org/news-and-media/all-news/2006/march/flight-training-magazine/encounters-of-the-feathered-kind)

## 2. Plan (lo elegido)

- **Distancia**: ≥ **300 m** de cualquier edificio-árbol y de cualquier isla (`LaunchHeightRule.MinDistance`). Sitio: un **casquete
  artificial** de 110 × 110 m en mar abierto, al oeste, en **(−560, 0, 0)**: a ~310 m del borde de las islas y a ~490 m del edificio-árbol
  más cercano (Entrenamiento). Al oeste porque la trinchera de llamas sale hacia mar abierto, lejos del santuario.
- **Altura (LA REGLA)**: **plataforma ≥ máx(lo más alto del santuario, techo de vuelo de las aves) + 30 m**:
  - lo más alto: el techo de cada edificio-árbol con la columna de su puerta y todo `ElevatedElement` (el árbol de la guardia…); hoy,
    el techo de Ciencias (~153 m) con su columna (~162 m);
  - aves: máx(152 m, lo más alto que vuelan las aves del juego);
  - margen 30 m: el chorro de los propulsores, la nube del diluvio y los cascotes no bajan a esa franja.
  Hoy sale **~192 m**. La altura no protege del ruido ni de la onda (eso lo hace la distancia), pero deja la plataforma por encima de
  donde vuelan las aves y de los pisos más altos, que es lo que pidió Berón, y el chorro y los cascotes se van hacia arriba y al mar.
- **La regla siempre**: al construir la lanzadera y otra vez al final del Blockout (cuando ya está todo lo elevado), `ValidateLaunchHeight`
  la comprueba: si no cumple, **da error en el registro y SUBE la corona** (alarga sus pilares). En juego, un `ElevatedElement` que aparece
  vuelve a validar (`LaunchSite.Validate`). Para que algo nuevo cuente, se le pone un `ElevatedElement` (o es un edificio-árbol).

## 3. Construcción

- **Casquete artificial** (`Casquete_artificial`): hielo armado de 1,5 m con un pretil de 1,2 m (contra el oleaje y para contener vertidos),
  con el hueco del tronco en el centro (el árbol tampoco tiene suelo: entra el submarino de obra).
- **Árbol-lanzadera** (`Lanzadera_cohetes`, edificio «Cohetes»): una torre-árbol de 2 niveles con el primer piso a la altura que haga
  falta para que la corona quede donde pide la regla. Pisos:
  - **P1 «Fábrica de cohetes»** (`Fabrica_de_cohetes`): la primera y la segunda etapa en sus cunas, la carga útil, el puente grúa y la plaza
    del submarino de obra.
  - **P2 «Control de misión»** (`Control_de_mision`): 12 puestos con pantallas en tres filas, la pantalla grande; es un área
    (`SanctuaryAreaType.LaunchControl`).
  - **P5 «Garaje de la lanzadera»** (`Garaje_lanzadera`): plaza del submarino de obra.
  - Los demás pisos y las puntas, libres (miradores). Con sus máscaras, su fondo abierto y sus alfombras al suelo y al techo, como todos.
- **Corona de lanzamiento** (`Corona_lanzamiento`, 12 m sobre el techo, sobre 4 pilares de hormigón armado de 3 × 3 m):
  - **plataforma** de 24 × 24 × 2 m con el **hueco de la llama** (4 × 4 m) en el centro y el **cohete** en su mesa (4 vigas y 4 pinzas): modelo low-poly de revolución (`RocketModel`): 9 toberas, faldón, 4 aletas, primera etapa a franjas, interetapa, segunda etapa y cofia ojival con punta roja; al lado, la **torre umbilical** de celosía;
  - **deflector** de acero a 45° bajo el hueco y **trinchera de llamas** de 4 × 4 m que saca el chorro hacia el oeste (mar abierto);
  - **diluvio de agua**: depósito elevado, bajante y 8 rociadores alrededor del hueco (supresión de sonido y refrigeración);
  - **cubeta de contención**: pretil de 1,5 m en todo el borde (recoge combustible y agua; no cae nada al mar);
  - **escudo contra cascotes** de 10 m del lado del santuario (este), con contrafuertes, zunchos y remate de acero;
  - **depósitos de oxígeno líquido y RP-1** al aire en el casquete, a más de 80 m entre sí, y **polvorín** de las cargas de separación en otra esquina ([sitio-nuclear.md](sitio-nuclear.md) §6);
  - **torre umbilical** de 22 m; una zona de alfombras en la plataforma (no hay escaleras).
- **Trabajos** (`LaunchSite.Missions`, en la jornada, no obligatorios): control de misión (1 h), seguridad de la plataforma (diluvio,
  trinchera, escudo y cubeta, 1 h, recorre) y la fábrica (integrar etapas y carga útil, 2 h).
- **El microcosmos de Musk** (rama claude/edif-famosos, «pendiente: Fábrica de cohetes y control de misión»): los pisos se llaman
  **«Fábrica de cohetes»** y **«Control de misión»** (`LaunchSite.FactoryFloor`, `LaunchSite.ControlArea`).

## 4. Prueba (grupo 90, `LaunchSiteTest`)
Sitio a ≥ 300 m de árboles e islas; la regla se cumple en la escena; un elemento elevado nuevo más alto que la plataforma la hace subir
(y alarga los pilares); las piezas de contención y el cohete de 18 m; los dos pisos con el área, los puestos y las misiones; alfombras en
el suelo, el techo y la plataforma; y el submarino de obra va por la vía marina al garaje de la lanzadera y aparca.

## Pendiente
- El aviso de lanzamiento en la jornada (todos dentro durante la ventana) y el lanzamiento en sí (cuenta atrás, encendido, diluvio).
- Medidas publicadas de la altura de vuelo de petreles y págalos antárticos.
- Que los futuros elementos elevados lleven `ElevatedElement` (los edificios-árbol ya cuentan solos).
