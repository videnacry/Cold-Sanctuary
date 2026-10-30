# Plano de edificios: cuatro torres temáticas

> Decisión de Berón del 2026-10-29: «lo ideal son POCOS edificios con MUCHOS pisos — para eso se prepararon las columnas
> para 30 pisos». Sustituye al plano del 2026-10-27 (9 edificios por producción e investigación).
> Código: `Assets/Editor/SampleSceneBuilder.Towers.cs` (sitios, plantas, `BeginTower`/`TowerFloor`/`InnerFloor`,
> `BuildingOf`, `FloorIndex`), `SampleSceneBuilder.cs` (`BuildAreaStructures`), `.Hospital.cs`, `.AtomicPlant.cs`,
> `.Research.cs`, `.Levels.cs` (`StackOn`, `RingCarpets`) y `.Transport.cs` (`RegisterBuildings`, `FloorsOf`, `FloorName`).

## Reglas
- La planta baja de cada torre está a 12 m (`ElevatedHeight`); cada piso sube 11 m más (`UpperFloorGap`).
- **Todos los pisos de una torre miden lo mismo**: la planta del área más grande de la torre. Así las 8 megacolumnas de las
  puntas del octógono son continuas de abajo arriba. Cada área amuebla su piso centrada en él (la Cría, la Carne Cultivada,
  el hospital y la planta atómica conservan su distribución de antes, centrada; las demás se reparten por el piso).
- **Áreas con plantas propias** (hospital 3, física 3, química 2, biología 2): sus plantas son hijas del área (el
  microcosmos del área las copia con ella), pero cuentan como pisos de la torre: la siguiente área va **11 m encima de la
  más alta** de ellas, no 11 m sobre su planta baja (`TowerFloor`/`InnerFloor`). El hospital deja montadas sus plantas 1 y 2
  en `BuildAreaStructures` y las amuebla después `BuildHospital`.
- Los pisos de áreas distintas son **hermanos**, no hijos: el microcosmos que clona un área (`AreaMicrocosmos`, por nombre de
  GameObject) copia solo sus plantas, no la torre entera.
- Sin escaleras ni puentes: cada piso, cada anillo y el suelo tienen su zona de alfombras de la red de la torre
  (`CarpetPad.building` = nombre de la torre). Encima del último piso, el techo de alfombras (las de edificio y las del
  santuario) con los paneles solares. Columnas: `StructuralLimits.MaxFloors` = 30 (contando el techo).
- Los anillos de pisos distintos quedan uno encima de otro, a 11 m (el anillo vuela a 4 m sobre su piso; lo que llevan
  encima mide ≤ 3 m). Entre la punta del anillo de una torre y la torre vecina, ≥ 16 m.
- Sitios: en lóbulos de hielo (`PolarLandscapeBuilder.Islands`) con los encepados de las 8 columnas sobre hielo firme
  (comprobado con la fórmula de las islas).

## Las torres

Cotas de la cara de cada piso: P0 = 12 m, P1 = 23, P2 = 34, P3 = 45, P4 = 56, P5 = 67, P6 = 78.

| Torre | Sitio (x, z) · lóbulo | Planta (m) | Pisos, de abajo arriba | Techo de alfombras |
|---|---|---|---|---|
| **Servicios** (personas y animales) | (5, 94) · Norte:Cocina | 40 × 38 (el restaurante) | P0 Restaurante (`Kitchen_Area`, con el anillo del comedor) · P1 Hospital: urgencias y ambulatorio (`Infirmary_Area`, anillo de observación y maternidad) · P2 Hospital: quirófanos, UCI y aislamiento (`Planta_hospitalizacion`, anillo de recuperación) · P3 Hospital veterinario (`Planta_veterinaria`, anillo de grandes animales) · P4 Cría (`CubCare_Area`) · P5 Carne Cultivada (`CulturedMeatLab_Area`) | 78 m |
| **Entrenamiento** | (10, 0) · Centro:Hub | 36 × 26 (`GymSize`) | P0 Gimnasio (`Gym_Area`) · P1 Cardio y zona libre · P2 Calistenia, parkour y crossfit (`Calisthenics_Area`) · P3 Sala de Yoga (`YogaRoom_Area`, anillo de esterillas y estatuas) · P4 Sala de Ballet (en obras) · P5 Dojo (en obras) | 78 m |
| **Ciencias** | (92, −108) · Sur:Biologia | 32 × 20 (biología) | P0 Física: reactor TRIGA (`AtomicPlant_Area`; la piscina baja como pilar hasta el suelo) · P1 ciclotrón · P2 sala de experimentos (anillo y torre de Foucault) · P3 Química: síntesis (`Chemistry_Area`) · P4 Química: análisis · P5 Biología: cultivo (`Biology_Area`) · P6 Biología: molecular | 89 m |
| **Producción** (construcción y talleres) | (18, −95) · Sur:Rotacion | 26 × 20 | P0 Construcción (`Construction_Area`) · P1 Mecánica (`VehicleWorkshop_Area`) · P2 Estudio Textil (`TextileStudio_Area`) | 45 m |
| **Árbol de los Campos** (los cultivos; docs/edificio-arbol.md) | (−50, −84) · Sur:Milpa | octógonos de ~51 m sobre una raíz de agua de Ø 22 m | P1 Arrozal · P2 Bambusal · P3 Milpa · P4 Chacra andina · P5 Huerto (`Garden_Area`) · P6 Rotación · P7–P8 Bosque comestible (6 a 62 m); 6 ramas con anillos de nidos (~91 m) | copa a 101 m (y alfombras en el suelo) |
| Central Nuclear (aparte) | (60, −162) · Sur:Central | 46 × 30 | P0 microrreactor de producción | 23 m |

**Por qué así**
- *Servicios*: lo que atiende a personas y animales a diario en una sola columna: comer abajo (lo más concurrido, con el
  comedor en anillo), el hospital encima, con lo veterinario arriba de sus tres plantas, y justo encima los cachorros (la
  Cría) y el laboratorio que da de comer a los depredadores. No se inventan pisos de servicio vacíos.
- *Entrenamiento*: las 2 h diarias en un edificio. Lo pesado y ruidoso (las máquinas) abajo; lo silencioso (yoga con su
  anillo) arriba; las dos salas en obras, arriba del todo (su material sube en alfombra).
- *Ciencias*: el reactor TRIGA tiene que ir en la planta baja (su piscina es un pilar hasta el suelo); encima, química y
  biología. La torre de Foucault (17 m sobre el anillo de P2) queda fuera del octógono, junto a P3–P4, sin tocar su losa.
- *Producción*: la maquinaria pesada abajo y el taller textil arriba, en el casquete Sur. Los cultivos se mudaron al
  Árbol de los Campos, en el lóbulo vecino (Sur:Milpa): la torre queda más baja (3 pisos y el techo, 45 m).
- **Central Nuclear, aparte**: un reactor de producción necesita su propio sitio de exclusión (el límite pintado alrededor)
  y no debe compartir columnas ni pisos con nadie. Es la única excepción.

**Distancias** (punta de anillo → torre o parcela vecina): Árbol de los Campos ↔ Producción: pisos 22 m, anillos de nidos 4 m en planta pero más de 30 m por encima de su techo; Árbol ↔ Entrenamiento: anillos 23 m;
Ciencias (anillo de física hasta z −126) ↔ Central Nuclear 18,6 m; Ciencias ↔ Producción 35 m; Servicios ↔ Entrenamiento
46 m.

**Frutales en los techos** (2026-10-30, docs/jardines-frutales.md): el resto de cada techo de alfombras se llena de frutales en maceta (una de cada una de las 19 especies por vuelta, a 2 m), con la sala de cuidados de la experta (Irosene) en el de Servicios y un punto de riego en los demás.

**Parcelas**: ya no hay. Desde el 2026-10-29 los campos (milpa, chacra andina, arrozal, bosque comestible, bambusal, el
huerto y la rotación) están en los pisos del **Árbol de los Campos** (docs/edificio-arbol.md); sus lóbulos vuelven a la naturaleza. El campo solar sigue en el
suelo del hub, en (26, −26…−34).

Quedan libres los lóbulos de la antigua enfermería (55, 97), la sala de yoga (105, 35) y química (52, −110): vuelven a la
naturaleza.

## Riesgos (comprobar en Unity)
- **Jornada** (`SanctuaryArea.InBuilding`, «el puesto es el edificio entero»): ahora el edificio es la torre. Quien tiene
  turno en la Cocina cuenta como en su puesto también en el hospital, la Cría o la Carne; en el Gimnasio, en todo el
  Entrenamiento. La salida del hospital (`InfirmaryGate`) ya no usa el edificio sino el área del hospital con sus plantas
  (`Contains`): el recién llegado no puede irse al restaurante ni a la Cría antes de iniciarse.
- **Microcosmos**: los pisos son más grandes (Cría y Carne: 40 × 38; Yoga, Ballet y Dojo: 36 × 26; Mecánica y Textil:
  26 × 20; física: 32 × 20) y el área gigante ×10 crece con ellos. Las historias colocadas en coordenadas del piso antiguo
  hay que revisarlas a ojo (sus plazas reservadas sí se recalculan: todos los pisos tienen 4 historias, 8 personajes,
  12 fauna y 60 flora).
- **NavMesh**: hay que volver a hornear; el techo más alto queda a 89 m (Ciencias). Cada piso es una isla y se pasa de uno a
  otro en alfombra.
- **Niebla** (`fogDensity = 0.008`): desde el suelo, los pisos altos (60–90 m) se verán difuminados; los viajes en alfombra
  son más largos.
- Las columnas del piso de arriba atraviesan los anillos del de abajo (ya pasaba en el hospital): mirar que no pisen camas.
- Después de construir hay que guardar la escena (el Blockout no la guarda: «save» al buzón).
