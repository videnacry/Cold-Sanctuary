# Centros para todos los miembros a la vez (regla J16)

Berón (2026-11-04):
- «los centros de entrenamiento y de ocio deben dar servicio a todos los miembros a la vez»;
- «ten en cuenta que algunas áreas tienen más de un piso justo para abastecer a todos los miembros y se pueden agregar pisos, si las 2
  de yoga existentes no son suficientes se crea una más y lo mismo con el dojo y demás áreas de estudio y entrenamiento».

## Los miembros

Son la lista del personal (`WorkdayDirector.Staff`, con Kushal). Hoy son 18.
- Nunca se pone un número fijo: el constructor usa `AreaCapacity.DefaultMembers` y el juego, `AreaCapacity.Members()`.
- Es la misma cuenta que la regla de capacidad (S13) y el Aula.

## Pisos por centro: ⌈miembros / plazas de un piso⌉

`TrainingCentres.FloorsFor`. Las plazas de un piso salen de su distribución.

| Centro | Plazas por piso | De dónde salen | Pisos hoy |
|---|---|---|---|
| Gimnasio | 30 estaciones | 10 tipos de máquina de placas × 3 copias (el cardio y el peso libre, aparte) | 1 |
| Calistenia, parkour y crossfit | 40 | calistenia ~18 (dominadas 3, anillas 2, paralelas bajas 4, espalderas 3…), parkour ~6, crossfit ~16 | 1 |
| Yoga Hatha | 16 esterillas | dos salas de 8; con 2 m entre esterillas y a ≤ 9 m del maestro no caben más en 18 × 26 m | **2** |
| Yoga Ashtanga | 16 esterillas | igual | **2** |
| Dojo | 25 | tatami de 8 × 8 (16 a 2 m), jaula 2, ring 2, sacos 3, makiwaras 2 | 1 |
| Ballet | 20 | suelo de 18 × 8,6 m con barras dobles (16–23 alumnos) | 1 |
| Aula | (su constructor, `AulaFloors`, en `claude/edif-pisos`) | pupitres de clase y estudio | 1 |

### Cómo se añaden los pisos

El constructor de la torre del Entrenamiento (`SampleSceneBuilder.cs`, bloque ENTRENAMIENTO):
- crea las copias de cada centro (`Planta_hatha_2`, `Planta_ashtanga_2`, `Planta_gimnasio_2`…) como hijas de su área;
- las apila **justo encima del primer piso del centro** con `TowerFloor`.

Cada piso nuevo tiene lo mismo que cualquier piso del edificio-árbol:
- la hélice, 22 m entre niveles, los anillos, las puertas con sus **máscaras** y las **alfombras**;
- sin escaleras, y con los 2 m de paso en su amueblado.

La torre sube los niveles que haga falta: ⌈(pisos + pisos del Aula) / 2⌉.
- Hoy son 9 pisos de entrenamiento más 1 de Aula: **5 niveles**.
- Antes eran 4 niveles con 7 pisos más el Aula.

Amueblado de cada copia:
- **Yoga**: `FurnishYogaFloors`, con sus dos salas y sus aseos. `YogaHall.floorNo` dice qué piso es.
- **Gimnasio**: `FurnishGym` con `copyBase` = 3 × (piso − 1), así que las copias de cada máquina siguen numerándose.
  - La jornada reparte entre todas las copias (`GymStation.CopiesOf`).
- **Calistenia**: `FurnishCalisthenics`.
- **Ballet y dojo**: su obra con su acabado (`PrepareSite`).
- Cada piso de calistenia, ballet y dojo lleva su `AreaSeat` (categoría Entrenamiento) con las plazas de su ficha.

## La comprobación: la regla de capacidad (S13, grupo 75)

No hay un segundo validador: es `AreaCapacity` con `CapacidadPisosTest` (grupo 75).
- Cuenta en todos los pisos:
  - las estaciones del gimnasio;
  - las esterillas por **estilo de su sala** (`YogaHall.style`: los pisos de Ashtanga ya no se buscan por nombre);
  - los `AreaSeat` de la calistenia, el dojo, el ballet y el Aula;
  - el **ring de almohadas** (`PillowRing.All`).
- El catálogo del horario libre se comprueba con `AreaCapacity.CheckActivities` (lo llama `PillowFightTest`, grupo 89).
- Falla si un centro de entrenamiento o de estudio no llega a los miembros.

## Las clases y el ocio

- **Ashtanga** (entrenamiento de las 14 h): si la clase de la sala del día está llena, el siguiente que llega abre clase en la
  siguiente sala del estilo (`YogaHall.Ordered`): primero la otra versión, luego los demás pisos.
- **Ocio «para todos»** (`LeisureDirector.AllMembers`): su capacidad son los miembros.
  - Son pesas, Hatha libre, Ashtanga autoguiado, huerto, lectura, tertulia, baile y artes marciales.
  - El yoga reparte sus plazas entre las esterillas de **todas** las salas del estilo, en todos los pisos.
  - La lectura usa ahora 4 columnas en los lados de la plataforma central de la arboleda: caben 40.
- **Por turnos**, con justificación:
  - «Jugar con las crías» (4): pocas personas por camada, para no agobiarlas.
  - «Manualidades» (4): una herramienta por persona (los telares y la rueca del estudio textil).
  - Berón no las nombró como centros de entrenamiento.

## Altura de las torres (simulador de holguras)

La del Entrenamiento crece 22 m (techo a ~175 m).

El simulador `mig/Clear.cs` se volvió a pasar con los sitios de hoy:
- Entrenamiento (−77, 52) a 90°, Servicios (55, 102), Ciencias (92, −117) a 90°, Producción (113, −15) a 90°.
- Las holguras entre edificios no cambian, porque solo crece hacia arriba:
  - Árbol–Entrenamiento 16,3 m, Servicios–Entrenamiento 16,5 m;
  - guardián–Entrenamiento 7,0 m (planta 1), como antes.
- El tronco del guardián, que mide lo que el techo más alto, pasa de 158 a 180 m.
