# Reglas de las plataformas (pisos, anillos y microcosmos)

Reglas que debe cumplir **cada piso** del santuario (Berón, 2026-10-22 … 2026-10-28). Cada una tiene, o tendrá, su prueba:
si un cambio rompe una regla, la prueba debe fallar antes de que se note jugando.

| # | Regla | Dónde vive en el código | Prueba |
|---|---|---|---|
| 1 | **Medida humana**: cada mueble y máquina se usa sin problemas por la persona más grande (≈ 2 m de altura, hombros de 0,55 m). Camas, sillas, máquinas y mesas con sus medidas reales. | Los `Furnish*` de `Assets/Editor/SampleSceneBuilder*.cs`; `docs/gimnasio-maquinas.md`, `docs/entrenamiento-areas.md` | Pendiente: medir asientos, camas y puestos contra el cuerpo más grande. |
| 2 | **2 m de paso** entre todo lo que no va junto: dos personas pasan a la vez. No se separa lo que va unido (piezas de un mueble, sillas con su mesa, una línea de encimeras, estanterías contra la pared, una cama y su mesilla). | `Assets/Scripts/World/FurnitureSpacing.cs` (+ `FurnitureLine.cs`) | `SanctuaryLifeTest`: 0 violaciones por área. |
| 3 | **Sin escaleras ni puentes.** De un piso a otro, a su anillo o a otro edificio se va en **alfombra voladora**. | `CarpetPad`, `FlyingCarpet`, `SampleSceneBuilder.Transport.cs` (`PlaceCarpetAreas`) | `SanctuaryLifeTest`: ninguna «Escalera»; cada piso y cada anillo con su zona de alfombras. |
| 4 | **Anillo opcional** (`RingDeck`) para ganar suelo: 20 % sobre el piso y el resto volando; entre anillos vecinos ≥ 16 m. | `SampleSceneBuilder.Levels.cs` | Separación de anillos (hospital–restaurante). |
| 5 | **Zona de alfombras** en cada piso, cada anillo y el techo (alfombras de edificio y de santuario). Cada pista sabe qué alfombra ocupa cada hueco y apila las que sobran. | `CarpetPad.Claim`, `PlaceCarpetAreas` | Alfombras por piso; apilado sin solaparse. |
| 6 | **Plataforma octogonal** con **una megacolumna sísmica en cada punta** (tubo de acero relleno de hormigón, aislador de caucho y plomo, anillo-cinturón, riostras BRB). | `SampleSceneBuilder.Elevate`, `docs/columnas-sismicas.md` | — |
| 7 | **Columnas para 30 pisos.** Si un edificio pasa de `StructuralLimits.MaxFloors`, la construcción da error y la prueba falla: hay que reforzarlas antes. | `Assets/Scripts/World/StructuralLimits.cs` | `SanctuaryLifeTest`: «ningún edificio pasa de 30 pisos». |
| 8 | **Posiciones reservadas** en cada piso para la **flora**, la **fauna**, los **personajes** y las **estructuras de la historia** del microcosmos. La plantilla de microcosmos (`MicrocosmTemplate`, con fecha, hora y plataforma) coloca lo que pida cada historia en esas posiciones. Cambiar una plataforma obliga a revisar si siguen cabiendo. | Pendiente: `PlatformSlots` (por piso) y su uso en `AreaMicrocosmos` | Pendiente: cada microcosmos usa sus posiciones; ninguna queda fuera del suelo; la flora crece sobre el suelo del área (ya: `ExperienciaJugadorTest`). |
| 9 | **Agrupación por producción e investigación** (Granja, Talleres, Entrenamiento, Yoga…), plano aprobado antes de mover áreas. | `BuildAreaStructures`, `docs/plano-edificios.md` | `SanctuaryLifeTest`: edificios agrupados. |
| 10 | **Iniciaciones**: las da el trabajador de turno del área (yoga, cocina); la Magnate solo acompaña. | `AreaInitiator`, `Magnate` | `KitchenInitiationTest`; pendiente la del yoga de punta a punta. |

## Al cambiar una plataforma

1. Comprobar los 2 m (regla 2) y que la zona de alfombras sigue libre (regla 5).
2. Revisar las posiciones reservadas del microcosmos (regla 8): si faltan, ampliar el piso o recolocarlas.
3. Si se añaden pisos, mirar la regla 7.
4. Pasar las pruebas (`ed.sh test`) con Berón sin jugar en ese Unity.
