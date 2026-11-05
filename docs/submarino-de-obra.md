# El submarino de obra y las medidas de la vía marina (Berón 2026-11-03)

Pedido de Berón: «Para la construcción de pisos sería necesario mover mucha maquinaria grande y pesada y toneladas de material…
se podría utilizar el camino acuático… agrandar más las ramas y entradas para que la maquinaria y material pueda moverse libremente
del piso de construcción a cualquier piso de cualquier edificio, aprovechando que ahora ya no tienen suelo… verificar si requieren
incremento de espacio entre piso y piso y entre piso y anillo; crea el submarino que se encargue del transporte… el tronco también
se debe agrandar en caso que no sea lo suficientemente grande para que el submarino se traslade en paralelo con los peces y demás
fauna; que no choque con los animales; que pueda entrar por las entradas y aparcarse en el piso de construcción o en un piso garaje;
el submarino debe ser capaz de desplazarse también por tierra.»

Código: `Assets/Scripts/World/ConstructionSubmarine.cs` (el vehículo y sus viajes), `TreeSpec` (el vehículo de diseño y las medidas),
`SampleSceneBuilder.Masks.cs` (`SubmarineGarage`: el garaje), `SampleSceneBuilder.Levels.cs` (`RingDeck`: los anillos de los pisos
de los árboles a 8 m) y la prueba `SubmarineTest` (grupo 73).

## 1. Las cargas reales (investigación)

| Carga | Medidas de transporte | Peso |
|---|---|---|
| Excavadora de cadenas de 20 t (Caterpillar 320, la clase más común en obra) | ~9,5 × 3,0 × 3,0 m (pluma recogida) | ~22 t |
| Contenedor de 20 pies (ISO 668) | 6,06 × 2,44 × 2,59 m | hasta 30 t |
| Vigas de acero | barras de 6 o 12 m (aquí, de 9 m como mucho) | 1–2 t cada una |
| Palés de material (guadua, bahareque, paneles de micelio, cuerda: el «kit del día» del almacén) | 1,2 × 1,0 m | ~1 t cada uno |
| Camilla del submarino hospital (lo construye otro hilo): un elefante tumbado / un oso | ~7 × 3 × 2 m / 3 × 1,5 × 1 m | ~6 t / 0,5 t |

**Bodega**: 10 × 3,4 × 3,4 m (cabe la excavadora o el contenedor de 20 pies, o la camilla de un elefante). **Vehículo de diseño**
(`TreeSpec.VehicleLength/Beam/Height`): **14 × 5 × 5 m** con orugas, hélices y cabina; holgura de **1 m** a cada lado
(`TreeSpec.VehicleClearance`). Las medidas valen para el submarino de obra y para el submarino hospital.

## 2. Medidas de la vía marina (lo mayor de 8 personas y el vehículo)

| Pieza | 8 personas | Vehículo + holgura | Elegido | Antes |
|---|---|---|---|---|
| **Puertas** (todas cuadradas) | 8 × 0,70 + 7 × 0,05 = 5,95 m | 5 + 2 × 1 = 7 m | **7 × 7 m** | 3 × 3 (y 6 × 6 esta mañana) |
| **Ramas** (cuadradas) | 5,95 m | 7 m de paso; girar el codo un vehículo de 14 × 5 m pide (7 + 5)/√2 = 8,5 m | **10 × 10 m** (no cambian) | 10 × 10 |
| **Tronco** | — | submarino (5 m) junto a un banco de peces (~7 m) con 3 holguras de 1 m = 15 m; y el submarino de 14 m sube y baja nivelado | **20 × 20 m** (no cambia) | 20 × 20 |
| Columna del techo | — | puerta de 7 m y su estación de máscaras | **9,2 × 9,2 m** de agua | 5 × 5 |
| Hueco del fondo | — | torres 19,6 × 19,6 m; el Árbol de los Campos, que gira, 14 × 14 m: el submarino lo cruza de morro (5 × 5 m) | | (cerrado) |

- **Entre piso y piso** (22 m por nivel): la puerta de un piso (7 m) y la boca de la rama de la otra cara (10 m) caben de sobra; el
  campo de punta sigue 12 m sobre su nivel (encima del tubo). **No hace falta más.**
- **Entre piso y anillo**: SÍ. Los anillos de los pisos iban a 4 m sobre el piso y su balcón contra el tronco tapaba la parte de arriba
  de la puerta (de 7 m): el submarino (5 m) no salía. Ahora, en los pisos de los árboles, **el anillo va a 8 m** (`TreeSpec.RingOverFloor`)
  y su puerta, encima de la del piso. (Los anillos de las puntas, sobre sus campos, no cambian: sus puertas están en caras opuestas
  de la columna.)
- **Las puntas**: el submarino no entra en sus columnas (10,6 m de agua < 14 m de largo); las puntas son campos y huertos al aire. La
  maquinaria va a los **pisos** (pegados al tronco): a todos se llega por el tronco y su puerta.

## 3. El submarino

- **Anfibio**: anda por tierra (sobre los pisos, con sus orugas, a 2,5 m/s) y nada (a 4 m/s, de morro hacia donde va: así cruza el
  hueco del fondo y sube por el tronco).
- **Garaje**: el piso libre de la torre de **Producción** (la de Construcción) es el **«Garaje de submarinos»**, con su plaza pintada;
  el submarino aparca allí mirando a la puerta.
- **Viaje**: cuando una obra (`ConstructionProject`: las salas en obras, p. ej. ballet o dojo en Entrenamiento) no tiene el material
  del día, carga en el garaje la excavadora y los palés, va por tierra a la puerta del garaje, entra al agua del tronco, baja y sale
  por el fondo al mar, cruza bajo el hielo (a −312 m desde 2026-11-05: la plataforma mide 300; [sitio-nuclear.md](sitio-nuclear.md) §13), sube por el tronco del árbol de la obra, sale por la puerta de ese piso, anda
  hasta 7 m dentro del piso, descarga (`ConstructionProject.DeliverMaterial`: el kit del día sale del almacén de Construcción) y
  vuelve al garaje por el mismo camino.
- **No choca con los animales**: mira los grupos de peces, kril y medusas y la fauna marina por delante (hasta 15 m); si alguno queda a
  menos de 6 m de su casco, frena (hasta un 25 %) y se aparta de lado y en altura. Apunta la holgura mínima (`MinClearance`) de todo
  el viaje. Y los grupos se apartan de él (como de un barco): si su casco pasa a menos de 6 m, se alejan (las medusas, despacio). En el mar
  abierto le basta pasar a 3 m de sus puntos de paso (si un grupo está en el punto, lo rodea); cruza bajo el hielo a −312 m.

## 4. Prueba (grupo 73)

Medidas (puertas cuadradas para 8 personas y el vehículo; ramas que dejan girar al vehículo; tronco con el submarino junto a un
banco; anillos de los pisos por encima de la puerta), el garaje con el submarino, una entrega real a una obra de otro árbol con un
banco de peces y un grupo de medusas puestos en su camino (la obra tiene su material, ha nadado y andado, aparca en el piso de la
obra, la holgura mínima con los animales es > 0) y la vuelta al garaje.
