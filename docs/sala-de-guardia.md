# La sala de guardia (investigación → plan → construcción, 2026-11-05)

Berón: «mueve la guardia a un piso del árbol-edificio que mejor encaje y se equipa con una investigación de lo que debería tener, quizás pantallas y centros de control? aqui se realizaría la observación de icebergs, movimientos císmicos, maremotos y demás?».

Código: `World/GuardRoom.cs` (`GuardWatch`, `GuardWatchRule`, `GuardRoom`, `GuardScreen`), `Editor/SampleSceneBuilder.GuardRoom.cs`. Prueba: grupo 91. Sustituye a `GuardTower.cs` y a la cabaña del árbol.

## Dónde

**El piso libre del nivel 4 de Servicios, a 130 m.** Es el más alto de los libres.

| Opción | Por qué sí o no |
|---|---|
| **Servicios, nivel 4** | Elegida. Más vista que Producción, junto a la Sala de asambleas y encima del hospital. No añade nivel, así que no cambia la altura de la torre ni T13 |
| Producción, nivel 2 | Libre, pero a 86 m y lejos de quien responde a una alarma |
| Ciencias | Es la más alta, pero está llena: subirla cambiaría T13 y el tronco |
| Control de misión | Lo está moviendo otra rama |

No es un `SanctuaryArea`, como el piso de cardio. La jornada lo conoce como un sitio, igual que la línea de las cavernas (`WorkdayDirector.RegisterPlace`). Así no pide microcosmos propio.

## Qué tienen las salas reales

| Sala | Qué se copió | Fuente |
|---|---|---|
| **Tráfico marítimo** (Hong Kong, EE. UU.) | Radar, identificación de barcos y VHF en un centro. El radar sigue como comprobación de lo demás | [Hong Kong](https://www.mardep.gov.hk/en/public-services/port-services/vts/exvts/index.html), [USCG](https://navcen.uscg.gov/node/327), [Proceedings](https://www.usni.org/magazines/proceedings/2023/february/vessel-traffic-service-needs-major-radar-upgrade) |
| **Alerta de tsunamis** (EE. UU., Pacífico) | El sismo abre el aviso; el mareógrafo y la boya DART lo confirman o lo cancelan. Guardia de 24 horas | [Servicio de Investigación del Congreso](https://www.everycrsreport.com/reports/R41686.html) |
| **Agencia meteorológica de Japón** | Vigilancia de 24 horas y dos centros, Tokio y Osaka, por si uno cae | [JMA](https://www.wikipedia.com/wiki/Www.jma.go.jp), [JPGU](https://www2.jpgu.org/meeting/2014/session/S-SS28_e.html) |
| **Patrulla Internacional del Hielo** (EE. UU.) | Analiza datos de hielo a diario y publica el límite del hielo | [Informe de 2019](https://www.navcen.uscg.gov/sites/default/files/pdf/iip/2019_Annual_Report_FINAL.PDF) |
| **Control de misión de Houston** | Muro de pantallas y filas de consolas. **[verificar]**: de memoria, sin fuente nueva | — |

No encontré cómo son por dentro las salas del Pacífico ni la de la Patrulla del Hielo: las pantallas y consolas son diseño nuestro. El detalle de las salas de Japón es de una sola crónica.

## La sala

| Pieza | Origen |
|---|---|
| Muro de 7 pantallas, sobre postes (S6: no es una pared) | Houston |
| 2 filas de 6 consolas | Houston |
| Mesa de radar, con barrido que gira, y radio VHF | Tráfico marítimo |
| Carta de hielo | Patrulla del Hielo |
| Sismógrafo de tambor y puesto de respaldo | Japón |
| Mareógrafo y maqueta de boya | Sistema del Pacífico |
| Tablero de guardia | El de siempre, con un resumen de las pantallas |

## Las pantallas

| Pantalla | Lee | ¿Es real? |
|---|---|---|
| Árbol cazador | `HunterCell.Instance` | Sí |
| Icebergs | `Iceberg.All`, `Iceberg.Threatening` | Sí |
| Radar | Distancia del iceberg más cercano a un sitio | Sí |
| Roca de la caverna | `CavernRock.Instance` | Sí |
| Depredadores | `PredatorRound` | Sí |
| Emergencias | `NuclearSite.Evacuating` | Sí, pero **no hay fuego simulado** en el santuario: la pantalla lo dice |
| Sismógrafo | `GuardWatchRule.Magnitude` | **Simulado** |
| Mareógrafo y boya | `GuardWatchRule.SeaState` | **Simulado** |

- El santuario no tiene terremotos ni maremotos. El parte diario es determinista, sale «(simulado)» en la pantalla y **no tiene efecto en el mundo**.
- Las lecturas usan accesos estáticos: si falta un sistema, la pantalla lo dice. Recorren `IcebergRule.Sites` sin nombrar sitios, porque el lanzamiento y la energía se están moviendo en otra rama.
- Se renuevan cada 2 s y se ponen rojas en alerta.

## La jornada

- **Misión nueva `gua_sala`** (obligatoria, 2 h), en el sitio «Sala de guardia». Al acabar firma el parte en el registro.
- **Sigue igual**: la patrulla de `PuestoVigia_*` (ahora bajo `PuestosVigia_AUTO`), el alimentador, su rotación y el uniforme.
- La profesión de Gohageneis, Nesvitor y Tote incluye la sala: Berón hizo a Tote guardia.
- Sube a 77 misiones y 20 obligatorias (grupo 61).

## Lo que falta

- Las alertas no disparan nada nuevo: los remolques de icebergs ya salían solos, y la sala solo los muestra.
- Sin microcosmos. Ninguno de los pendientes de `MicroFloors` encaja con este piso.
- **Sin probar en vivo.**

## Auditoría (W11)

| Regla | Efecto |
|---|---|
| T3 | Sin excepción: el árbol gigante se retiró |
| T13 | Sin cambio: el nivel 4 ya existía. El gigante se retiró y deja de contar |
| S6 | Pantallas sobre postes; sin muros |
| S13 | 14 puestos de vigilancia; lo mira el grupo 91, no el 75, porque no es un área |
| S14, F14 | Fila nueva en `AreaDesign` |
| F5 | La franja de 4,6 m del tronco queda libre |
| F6 | No hay microcosmos que dependan de este piso |
| J28 | Dos profesiones ampliadas |
