# Carne cultivada: el laboratorio que alimenta a los depredadores

El santuario tiene un área propia para esto, el **Laboratorio de carne cultivada** (`CulturedMeatLab`, nivel 3; ver
[world-simulation.md](world-simulation.md) y [area-missions-spec.md](area-missions-spec.md)). No va en la enfermería.
La enfermería solo aporta la **biopsia**: unos gramos de músculo de un animal vivo, con anestesia local. El laboratorio
produce carne para los osos y los demás carnívoros **sin matar a ningún animal**.

Está en el casquete Este, entre la sala de yoga y la mecánica. Las raciones salen al **comedero de los depredadores**,
junto al territorio de los osos. Funciona con la energía solar del santuario. Código: `Assets/Scripts/World/CulturedMeatLab.cs`.

## Cómo se hace la carne cultivada (proceso real)

1. **Biopsia y aislamiento.** Del músculo se separan las **células satélite** (las células madre del músculo) y las
   precursoras de grasa (fibroblastos y preadipocitos). Se trabaja en una **campana de flujo laminar**, con aire
   filtrado HEPA, para que no entren bacterias.
2. **Banco de células.** Las células se multiplican un poco y se congelan en viales dentro de **nitrógeno líquido**
   (−196 °C). Cada lote empieza descongelando un vial, así que no hace falta otra biopsia cada vez.
3. **Medio de cultivo.** Es la "comida" de las células:
   - aminoácidos, glucosa, vitaminas y sales;
   - **factores de crecimiento** como FGF2, IGF, insulina, transferrina y TGF-β.

   Hoy existen medios **sin suero fetal** (por ejemplo, el medio "Beefy-9"), de modo que no hace falta ningún animal
   sacrificado. El medio se esteriliza con un **filtro de 0,2 µm**, y el material, en el **autoclave**.
4. **Proliferación.** Se hace en **biorreactores de tanque agitado**: primero uno de "semilla" (pequeño) y luego el de
   producción (grande). Las células crecen pegadas a **microportadores**, unas bolitas en suspensión.
   - Condiciones: 37 °C, pH 7,2–7,4 y oxígeno disuelto controlado con sondas y una camisa térmica.
   - Las células se **duplican cada ~24 h**. Al comer glucosa sueltan lactato, que baja el pH.
   - Por eso el control **renueva el medio** (perfusión) y añade bicarbonato.
   - Cuentas: un gramo de músculo tiene unos **2–3·10⁸ células**. Para 2 kg se necesitan ~5·10¹¹; desde un vial de
     10⁹ son unas **9 duplicaciones**.
5. **Diferenciación.** Se bajan los factores de crecimiento y las células se colocan sobre un **andamio comestible**
   (alginato, proteína vegetal texturizada). Ahí se **fusionan en fibras musculares** (miotubos). El **estiramiento
   mecánico** hace que maduren mejor. La grasa se diferencia aparte y se mezcla después.
6. **Cosecha.** El tejido se separa y se lava en la **centrífuga**, se forma en piezas en la **formadora** y se guarda
   en la **cámara fría** (4 °C).

Para los depredadores del hielo tiene sentido cultivar las células de **sus presas naturales**: foca para el oso, y
pescado para focas y pingüinos.

## En el juego

| Fase (`CulturedMeatLab.Stage`) | Máquina | Tiempo de juego por defecto |
|---|---|---|
| Biopsia | Campana de flujo laminar, microscopio | 20 s |
| Banco | Tanque criogénico (N₂ líquido), incubadora de CO₂ | 10 s |
| Medio | Depósito con agitador, filtro de 0,2 µm, autoclave | 15 s |
| Proliferación | Biorreactor de semilla (50 L) y de producción (500 L) con sondas | 12 s por duplicación (≈ 9 duplicaciones) |
| Diferenciación | Biorreactor de perfusión con andamio y estirador | 30 s |
| Cosecha | Centrífuga, formadora, cámara fría | 10 s → 4 raciones de 500 g al comedero |

La consola del laboratorio muestra en vivo la fase, las células, la glucosa, el pH y el O₂. El comedero acumula como
máximo 6 raciones; son `FoodItem` de carne que los carnívoros encuentran y comen.

**Pendiente:**
- La misión de microcosmos del área: "Nutrir el cultivo", a escala celular ([area-missions-spec.md](area-missions-spec.md)).
- Que Linduarda haga la biopsia en la enfermería.
- Que la máquina se repare o mejore en la Mecánica ([forge-simulation.md](forge-simulation.md)).
