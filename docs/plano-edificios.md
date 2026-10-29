# Plano de edificios: menos edificios, por producción e investigación

> Propuesta del 2026-10-27, que aplica la decisión de Berón del 2026-10-23 (`producción + investigación`). **Todavía no se
> ha construido.** Hay que enseñárselo a Berón antes de mover ninguna área.
> Código: `Assets/Editor/SampleSceneBuilder.cs` (`BuildAreaStructures`, ~l. 3426), `.Hospital.cs`, `.Research.cs`,
> `.AtomicPlant.cs`, `.Restaurant.cs`, `.Training.cs`, `.Levels.cs` (`StackOn`) y `.Transport.cs` (`RegisterBuildings`, `FloorsOf`).

## Reglas que se mantienen
- La planta baja de cada edificio está a 12 m (`ElevatedHeight`). Cada piso de encima sube 11 m más (`UpperFloorGap`).
- Todos los pisos de un edificio miden lo mismo (`SetBack = 0`). Por eso la planta del edificio es la del área más grande que contiene.
- No hay escaleras ni puentes: se pasa de un piso a otro, y de un edificio a otro, en alfombra voladora. Encima del último piso va el techo de alfombras, con sus paneles solares.
- El restaurante (cocina) y el hospital no se mueven, porque de ellos dependen muchos sistemas.
- Calistenia pasa al edificio del gimnasio.

## Antes: 14 edificios (y 5 parcelas elevadas)
Las cotas son las de la cara de cada piso: P0 = 12 m, P1 = 23 m, P2 = 34 m, P3 = 45 m y P4 = 56 m.

| # | Edificio (planta baja) | Centro (x, z) | Planta (m) | Pisos, de abajo arriba |
|---|---|---|---|---|
| 1 | Restaurante · `Kitchen_Area` | (5, 95) | 40 × 38 | P0 cocina, con el anillo del comedor |
| 2 | Hospital · `Infirmary_Area` | (63, 97) | 32 × 26 | P0 urgencias y ambulatorio, con anillo · P1 quirófanos, UCI y aislamiento, con anillo · P2 hospital veterinario, con anillo de grandes animales |
| 3 | Sala de Yoga · `YogaRoom_Area` | (105, 35) | 18 × 15 | P0 yoga, con anillo de estatuas · P1 `Ballet_Area` (en obras) |
| 4 | Mecánica · `VehicleWorkshop_Area` | (105, −25) | 10 × 9 | P0 |
| 5 | Carne Cultivada · `CulturedMeatLab_Area` | (118, 6) | 16 × 12 | P0. El comedero de los depredadores está en el suelo, en (126, −6) |
| 6 | Construcción · `Construction_Area` | (−20, 10) | 18 × 14 | P0 construcción · P1 `TextileStudio_Area` |
| 7 | Gimnasio · `Gym_Area` | (26, 6) | 22 × 16 | P0 gimnasio, con entreplanta de 4 m · P1 `MartialArts_Area` (dojo, en obras) |
| 8 | Calistenia · `Calisthenics_Area` | (−28, −12) | 10 × 9 | P0 (en obras) |
| 9 | Cría · `CubCare_Area` | (55, −55) | 26 × 20 | P0 |
| 10 | Huerto · `Garden_Area` | (5, −30) | 24 × 20 | P0 huerto de hortalizas, con su máquina de virtualización · P1 parcela de rotación (`UpperGarden`) |
| 11 | Física · `AtomicPlant_Area` | (15, −101) | 24 × 18 | P0 reactor TRIGA · P1 ciclotrón · P2 sala de experimentos, con anillo y la torre de Foucault |
| 12 | Química · `Chemistry_Area` | (52, −110) | 24 × 20 | P0 síntesis · P1 análisis |
| 13 | Biología · `Biology_Area` | (92, −112) | 32 × 20 | P0 cultivo · P1 molecular |
| 14 | Central Nuclear · `NuclearPower_Area` | (60, −162) | 46 × 30 | P0 |

Las parcelas elevadas (`MakePlot` con `withSoil`) están sobre su propia plataforma de tamaño + 2 m, cada una con su andén de alfombras:
- Milpa en (−45, −75), 20 × 20;
- Chacra andina en (−10, −118), 18 × 14;
- Arrozal en (−60, −15), 22 × 16;
- Bosque comestible en (−78, 45), 30 × 26;
- Bambusal en (−42, 0), 16 × 12.

El campo solar (3 filas, en (26, −26…−34)) está en el suelo.

## Después: 9 edificios

| Grupo | Edificio | Centro (x, z) | Planta (m) | Pisos, de abajo arriba | Por qué van juntos |
|---|---|---|---|---|---|
| **Producción · alimentación** | **Restaurante** (sin cambios) | (5, 95) | 40 × 38 | P0 cocina, con el anillo del comedor | Muchos sistemas dependen de él: la jornada, la Magnate, el comedor, el microcosmos de la Cocina y Le Chambon. No se le añaden pisos para no tocar su anillo ni el hueco con el hospital. |
| **Producción · alimentación** | **Granja** (en el centro del Huerto) | (5, −30) | **26 × 20**, la de la Cría | P0 Huerto (hortalizas y máquina) · P1 Rotación · P2 Cría · P3 Carne Cultivada | Aquí se produce todo lo que da de comer, salvo la cocina, como dice la nota del 23-10: huertos, cría y carne cultivada. La tierra, que es lo que más pesa, va abajo, y el laboratorio limpio arriba. La carne cultivada se produce aquí y se investiga en Biología. |
| **Producción · salud** | **Hospital** (sin cambios) | (63, 97) | 32 × 26 | P0 urgencias · P1 quirófanos y UCI · P2 veterinario | Ya cumple el criterio: es un edificio entero dedicado a la salud. |
| **Producción · oficios** | **Talleres** (en el centro de la Construcción) | (−20, 10) | 18 × 14 | P0 Construcción · P1 Mecánica · P2 Estudio Textil | Son los oficios que mantienen el santuario. La maquinaria pesada (miniexcavadora, hormigonera) va abajo. La Mecánica pasa de 10 × 9 a 18 × 14 y gana sitio. |
| **Producción · energía** | **Central Nuclear** (sin cambios) | (60, −162) | 46 × 30 | P0 | Es el microrreactor de producción y lleva su límite del sitio pintado. Debe quedarse aparte. El campo solar sigue en el suelo. |
| **Investigación · Física** | **Física** (sin cambios) | (15, −101) | 24 × 18 | P0 TRIGA · P1 ciclotrón · P2 sala de experimentos | Una materia por edificio. |
| **Investigación · Química** | **Química** (sin cambios) | (52, −110) | 24 × 20 | P0 síntesis · P1 análisis | Una materia por edificio. |
| **Investigación · Biología** | **Biología** (sin cambios) | (92, −112) | 32 × 20 | P0 cultivo · P1 molecular | Una materia por edificio. |
| **Entrenamiento** (ni producción ni investigación) | **Entrenamiento** (en el centro del Gimnasio) | (26, 6) | 22 × 16 | P0 Gimnasio · P1 Calistenia · P2 Dojo · P3 Ballet · P4 Yoga | Las 2 h diarias de entrenamiento se hacen en un solo edificio. Calistenia entra en el gimnasio, como pidió Berón. Lo ruidoso y pesado (las máquinas) va abajo; lo silencioso (yoga, con su anillo de estatuas) va arriba, con vista. |

Resultado: de 14 a 9 edificios. Se liberan las plataformas de (105, 35), (105, −25), (118, 6), (−28, −12) y (55, −55); ese suelo vuelve a la naturaleza.

**Variante B (10 edificios):** si 5 pisos (el último a 56 m, el techo de alfombras a 67 m) le parecen demasiados a Berón, Yoga + Ballet se quedan en su edificio de (105, 35). El Entrenamiento quedaría entonces con Gimnasio, Calistenia y Dojo.

**Parcelas (fase 2, opcional):** están junto al agua y son campos, no edificios, así que esta propuesta no las toca. Si hiciera falta reducir más, Arrozal y Bambusal (a 23 m de distancia, las dos al oeste) podrían ir en una sola base: el Arrozal abajo, porque necesita la lámina de agua, y el Bambusal arriba.

## Cómo se construye (resumen para cuando Berón lo apruebe)
1. Las áreas conservan sus nombres de GameObject (`Calisthenics_Area`, `CubCare_Area`…). Solo cambian de base, con `StackOn`. Así siguen funcionando todos los `GameObject.Find`.
2. El diccionario `upper` pasa a describir **cadenas**: Calistenia → Gimnasio, Dojo → Calistenia, Ballet → Dojo, Yoga → Ballet; Mecánica → Construcción, Textil → Mecánica; Cría → Rotación, Carne → Cría. Hay que apilar en orden, de abajo arriba, porque el orden de iteración de un `Dictionary` no está garantizado.
3. La planta baja del Huerto se eleva a 26 × 20 en vez de 24 × 20. `UpperGarden` ya hace la Rotación de esa medida (tamaño − 2 + 2 m de suelo).
4. La Cría se apila sobre la plataforma de la Rotación, que es un `GardenPlot` registrado en `_upperOf`, no sobre un `SanctuaryArea`.
5. Ballet, Dojo y Calistenia siguen en obras (`MakeConstructionSite`), ahora en P1–P3.
6. El comedero de los depredadores se queda en (126, −6), junto a los osos. `CulturedMeatLab.Build` recibe el comedero igual que ahora: las raciones viajan en alfombra.
7. Después hay que guardar la escena: el Blockout no la guarda, así que hay que mandar «save» al buzón.

## Riesgos
**Código por índice o por posición**
- `BuildAreaStructures` usa índices fijos:
  - `areas[2]` (Yoga), `[4]` (Textil), `[5]` (Carne), `[6]` (Construcción), `[7]` (Gimnasio), `[8..10]` (obras) y `[11]` (Cría);
  - `areas[areas.Count - 1]` es el Huerto;
  - las constantes `Ballet = 8`, `Dojo = 9` y `Textile = 4`;
  - y `sizes[]`, que tiene 12 entradas mientras que las áreas son 13, porque el Huerto va aparte.

  Conviene pasar a un mapa por nombre antes de mover nada.
- El bucle de techos (`int top = i; foreach (var kv in upper) if (kv.Value == i) top = kv.Key;`) solo sube **un** piso. Con cadenas de 3–5 pisos encontraría mal el piso más alto, y el techo de alfombras y los paneles quedarían a media altura. Hay que buscar el piso más alto de forma recursiva, igual que ya hace `BaseOf`/`FloorsOf` en `.Transport.cs`.
- Se seguirán creando `Aseos` y muebles con medidas relativas a `sizes[i]`. Los que tengan coordenadas fijas pensadas para la medida antigua hay que revisarlos: Mecánica pasa de 10 × 9 a 18 × 14, Carne de 16 × 12 a 26 × 20, Calistenia de 10 × 9 a 22 × 16 y Yoga de 18 × 15 a 22 × 16.
- Los lóbulos de hielo del paisaje (`PolarLandscapeBuilder`) se hicieron para los centros actuales. Los centros nuevos son centros que ya existían, así que no hace falta ningún lóbulo nuevo; los cinco que se liberan quedan vacíos.

**Nombres de edificio y pruebas**
- `RegisterBuildings` llama a cada edificio por el `displayName` de su planta baja. La Sala de Yoga pasa a estar en el edificio «Gimnasio», y la Cría y la Carne en el edificio «Huerto».
  - `SanctuaryLifeTest` comprueba `CarpetPad.building == "Sala de Yoga"` para las alfombras del anillo. **Esa prueba falla** en la variante A; en la B, no. Hay que cambiar la prueba o dar al edificio un nombre propio.
- Otras pruebas que tocan las áreas movidas:
  - `SanctuaryLifeTest`: `Gym_Area` → `MartialArts_Area` con `InBuilding` (sigue valiendo, porque el dojo sigue en el edificio del gimnasio), `FurnitureSpacing` del gimnasio, «ballet, dojo y calistenia en obras», `Construction_Area` con la maquinaria, y `Garden_Area`/`Kitchen_Area`.
  - `ExperienciaJugadorTest` solo usa el hospital, que no cambia.
  - Las pruebas solo corren con `ed.sh test`; hay que parar el Play antes de las pasadas completas.

**Jornada, Magnate y misiones**
- «El puesto es el edificio entero» (`SanctuaryArea.InBuilding`). Quien tiene turno en el Gimnasio contará como en su puesto también en Yoga, y quien lo tiene en la Cría, también en el Huerto. Hay que revisar `WorkdayDirector`, la Magnate y `VisualAttention.AreaTransform`.
- Las misiones se anclan por `displayName` y `VirtualMissionSceneBuilder.AreaGo` traduce el nombre al objeto. Mientras no cambien los nombres, siguen funcionando. Las obras (`ConstructionProject`) dejan su material «a pie de obra» en pisos altos: el equipo de Construcción tendrá que subirlo en alfombra.

**Microcosmos**
- `AreaMicrocosmos.Build` clona `GameObject.Find(areaGo)` a ×10 y le quita `Estructura_Elevada`. Los pisos de `StackOn` son **hermanos**, no hijos, así que el clon de un área copia solo su piso. Si al rehacer el código algún piso se cuelga como hijo de la base, el microcosmos copiaría el edificio entero.
- El microcosmos crece con la planta del área:
  - el río de Sakshi (Yoga) pasa de 180 × 150 m a 220 × 160 m;
  - el de Mecánica, que es la historia de Dayton, pasa de 100 × 90 m a 180 × 140 m.

  Las historias están colocadas en coordenadas del área antigua, así que hay que revisar el hueco libre del suelo.
- `MicrocosmTemplate.Stamp` busca `GameObject.Find(s.platform)` para el nombre; con los nombres conservados, sigue funcionando.

**NavMesh y alturas**
- Hay que volver a hornear el NavMesh. Cada piso es una isla, y el paso entre pisos lo hacen las alfombras. Hay que comprobar que el horneado cubre 56–67 m (antes el máximo eran 34 m, en el hospital y en Física).
- Los anillos vuelan 8 m (`RingDeck`: 10 m de ancho, el 20 % por dentro). El anillo de Yoga, en P4 del Entrenamiento, llega hasta z = −10, y la Granja empieza en z = −20: quedan 10 m, menos de los 16 m de la regla entre anillos vecinos. Como están a distinta altura (60 m contra 45–56 m) no chocan, pero hay que comprobarlo con la vista.
- La Granja (x de −8 a 18) queda a 3,6 m del campo solar (x de 21,6 a 30,4), que está en el suelo. No se tocan, pero hay que vigilar la sombra.
- El techo de alfombras del Entrenamiento queda a 67 m. Con la niebla actual (`fogDensity = 0.008`), desde el suelo se verá difuminado. Además, los viajes en alfombra serán más largos.
