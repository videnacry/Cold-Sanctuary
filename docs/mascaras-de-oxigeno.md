# Máscaras de oxígeno, puertas de 8 personas y fondo abierto de los edificios-árbol (Berón 2026-11-03)

Pedido de Berón:
- «En los edificios-árbol deben existir máscaras para poder respirar, de modo que los personajes pueden nadar indefinidamente. Las
  máscaras están en las entradas y también hay estaciones dentro: si por alguna razón un personaje termina dentro del agua sin
  mascarilla, puede acercarse a una de las estaciones y tomar una para tener oxígeno ilimitado.»
- «Se debe crear la tarea de revisar máscaras de oxígeno; la realizaría también la persona que cuida los jardines frutales de los
  edificios… y también ahora debe revisar/cuidar los fitoplancton para que estén en óptimas condiciones.»
- «Los edificios-árbol no deben tener suelo, de modo que se puede nadar hacia abajo saliendo al mar, de modo que pueden entrar
  animales marinos.»
- «Debido al tráfico… las puertas y ramas… para mínimo poder entrar 8 personas simultáneamente. Las puertas y ramas deben ser
  cuadradas.»

## Máscaras

Código: `Assets/Scripts/World/BreathingMask.cs` (`BreathingMask` en el personaje y `MaskStation` en cada puesto),
`Assets/Editor/SampleSceneBuilder.Masks.cs` (los construye en cada puerta), `SwimBody.Breathe` (con la máscara puesta el aliento no
se gasta), `TreeTrip` (la coge y la deja), `PathMover.StepToward` (rescate) y la prueba `TreeMaskTest` (grupo 70).

- **Qué es**: un *rebreather* de cara completa con su botellita de O₂ (visor amarillo, boquilla, correa y botella; se ve puesta en la
  cabeza). Con ella puesta el aliento no baja: **oxígeno ilimitado**.
- **Perchero de la entrada** (`Perchero_mascaras`): fuera de **cada puerta** del tronco (suelo, pisos, anillos de los pisos), de las
  columnas de las puntas (campo y anillo) y de la columna del techo. 8 máscaras colgadas (se ven las que quedan). Va al lado de la
  puerta hacia el centro de la cara (lejos de las esquinas), a 0,9 m del marco.
- **Estación interior** (`Estacion_mascaras`): dentro del agua, contra la cara de dentro de la pared junto a cada puerta: un panel
  amarillo con su luz verde y 6 máscaras. Así **cada nivel tiene su estación**; la más alejada de cualquier punto del tronco está a
  < 40 m (entre el suelo y el primer piso, 64 m de tronco, el punto medio queda a ~32 m de la del suelo o la del piso 1).
- **Personajes** (`TreeTrip`): al llegar a la puerta, antes de meterse en el agua, cogen una del perchero (1,2 s de juego en ponérsela;
  si el perchero está vacío, de la estación de dentro, junto a la puerta). Al salir por la puerta de llegada, la dejan en su
  perchero. Los percheros guardan hasta el doble de su capacidad (las máscaras van y vienen entre entradas).
- **El jugador**: la coge al pasar junto a un perchero (a < 4,5 m) o al tocar una estación dentro del agua (< 1,8 m); la deja en el
  perchero de la puerta por la que sale (a < 5 m) si ha nadado con ella; si la coge y no entra al agua, la devuelve al alejarse 6 m.
  Bajo el agua sin máscara, la pantalla dice dónde está la estación más cercana («estación de máscaras a 12 m, abajo»).
- **Rescate** (`BreathingMask.RescueStep`): un personaje con la cabeza bajo el agua de un árbol, sin máscara y con una estación a
  < 45 m, nada a la más cercana con máscaras y se la pone; mientras, quien lo conduce (la jornada, un viaje) le cede el paso. Si no
  hay ninguna a mano (el mar abierto), sube a respirar como siempre.
- **Estado de los puestos**: cada uno tiene su `condition` (juntas, visor, válvula y carga de las botellas) que baja un 10 % al día (antes 25 %; ver §El jardinero);
  lo repone el jardinero (abajo).

## Fitoplancton del fondo abierto

`Phytoplankton` (el productor de la cadena marina) en 4 **cortinas de cultivo** colgadas en el hueco del fondo de cada árbol
(`Fondo_abierto_<árbol>/Fitoplancton_N`): marcos con láminas verdes a 2 m bajo el hielo, a la luz que baja por el tronco
transparente. Son de su edificio (`building`): su estado baja un 30 % al día y, descuidadas, crecen hasta 4 veces más despacio
(biofilm en las láminas, sin nitrato, fosfato ni hierro). Alimentan al kril, que atrae a los peces y a las medusas: por eso viven
grupos de los tres bajo cada árbol y suben por el hueco (docs/grupos-de-animales.md). (No va en el agua del tronco: se pondría
verde y taparía a los nadadores; ver docs/fotobiorreactores.md.)

## El jardinero: revisar las máscaras y cuidar el fitoplancton (revisión BARATA, 2026-11-04)

`FruitGardener` (docs/jardines-frutales.md §Tareas del árbol). Cada edificio-árbol tiene una **unidad de tareas del árbol** que va
en la ronda de su jardinero, detrás de sus macetas.

**Por qué cambió (prueba 55, 2026-11-04):** con la primera versión el jardinero nadaba cada día a TODAS las puertas (≈ 200 revisiones
en 5 árboles): 36,5 h de rondas para 12 personas y ni un minuto para polinizar o podar. Ahora la revisión es barata, con dos cosas a
la vez:
- **Rotación semanal**: cada día se revisa a mano 1/7 de las estaciones de cada árbol (la estación nº *i*, de abajo arriba, el día en
  que *i* mod 7 = día mod 7; `MaskStation.Due`). Cada puerta, una vez por semana.
- **Por el indicador**: cada estación tiene una luz (verde ≥ 50 %, ámbar, **roja < 30 %**, `MaskStation.Indicator`) que llega al **panel
  de máscaras** de la puerta del techo. El jardinero lee el panel (**3 min**) y revisa también las que estén en rojo, toque o no.
- Para que una semana baste, el estado de un puesto baja ahora un **10 %** al día (antes 25 %): llega a su revisión con 30–40 %.
- Las máscaras que se acumulan en unas entradas y faltan en otras se reparten **cada noche** en cada edificio (el carro de máscaras:
  los puestos con más de su capacidad pasan las que sobran a los que tienen menos).

Lo que hace cada día:
1. Va al **techo** de su árbol (en alfombra) y lee el panel de máscaras de su puerta (3 min).
2. Coge una máscara del perchero y entra; BAJA nadando (una sola vez, de arriba abajo) por las estaciones de hoy (las ~3 de la rotación y las rojas), por el camino de
   los viajes, y en cada una revisa la estación y el perchero de su puerta (**2 min**; repone las que falten).
3. Vuelve al fondo y cuida cada cortina de fitoplancton (**2 min**: limpiar las láminas, medir y abonar; están junto a la puerta del suelo).
4. Sale por la puerta del suelo y deja la máscara.

Estimación (`FruitGardener.TreeTaskHours`): panel + revisiones + nado a 3,9 m/s (con la corriente) de estación en estación (1,3 × la
recta + 6 m) y de vuelta al fondo + 30 s de entrar y salir: del orden de **0,4–0,7 h por árbol** (antes 2–3 h). **Reparto**: si la
unidad cabe con las macetas en la ronda de ≤ 2,9 h, la hace el mismo jardinero; si no, es otra ronda del mismo edificio. El Árbol de
los Campos (sin frutales) entra en el camino solo por sus tareas. El reparto de las rondas lo lleva el hilo del hospital y los jardines;
los números de cada día salen en el registro: `[Frutales] … tareas de N árboles … → K personas`.

## Puertas y ramas para 8 personas (cuadradas)

- Ancho de un personaje: la cápsula del jugador y de los NavMeshAgent tiene **0,35 m de radio → 0,70 m**; entre uno y otro, 0,05 m.
  **8 × 0,70 + 7 × 0,05 = 5,95 m**; con el submarino de obra (14 × 5 × 5 m con su carga, 1 m de holgura a cada lado: 7 m), **puertas de 7 × 7 m** (docs/submarino-de-obra.md) (`TreeSpec.DoorWidth/DoorHeight`, `HelixTower.doorWidth/doorHeight`;
  antes 3 × 3). Todas: las del tronco, las de las columnas de las puntas y la del techo (su columna pasa a 9,2 × 9,2 m de agua,
  `TreeSpec.HatchHalf` 4,6).
- **Ramas**: ya eran cuadradas, con un hueco de **10 × 10 m** (Berón 2026-11-01): caben 8 nadadores en fila (5,95 m) con 2 m de
  margen a cada lado y más que la puerta; no se agrandan (agrandarlas mueve las bocas de las caras de 20 m y el alcance de cada
  árbol, que con el simulador de sitios deja las torres a ≥ 16 m unas de otras).
- En una cara con piso y anillo, la puerta del anillo se aparta de la del piso (con 6 m de alto se juntarían): `FreeDoorX` la
  penaliza a menos de 6,6 m.
- La prueba pone 8 cuerpos de 0,7 m en fila en el hueco de la puerta del suelo y comprueba que ninguno toca el marco.

## Sin suelo

- El tronco ya no tiene `Fondo_raiz`: el agua del tronco (`Agua_raiz`) baja hasta −1,6 m (la cara de abajo del hielo es −1,5) y el
  hielo de debajo tiene un **hueco** (`CutIceUnder`: los bloques de la banquisa que lo pisan se parten en los trozos de alrededor).
  Cuadrado y alineado con el mundo: en el Árbol de los Campos, que gira, de 14 × 14 m (siempre dentro del agua del tronco, gire como
  gire); en las torres (giro de 0° o 90°), de 19,6 × 19,6 m. Debajo, el mar (16 m de agua hasta el fondo marino).
- El cimiento (`Encepado_raiz`) es ahora un marco alrededor del hueco.
- NavMesh: el pie del tronco ya no se andaba (el círculo inscrito del tronco era «no andable»); el hueco tampoco. La prueba comprueba
  que no hay NavMesh en el hueco y que sigue llegando a la puerta del suelo de cada árbol.

## Escena de prueba

`Prueba_Mascaras` (Assets/Scenes/Pruebas; se regenera con el blockout): solo el Árbol de los Campos, con **Kushal empezando bajo el
agua del tronco, a 33 m, sin máscara**. Grupo 72: nada a la estación más cercana (como haría quien juega siguiendo el aviso de
pantalla), se pone la máscara y pasa 20 s bajo el agua con 4 s de aliento sin gastarlo.

## 2026-11-04 (ronda 2 del probador)

- **Coger y dejar, sin coste de viaje**: en un viaje por el tronco la máscara se coge al pasar por la puerta, ya entrando, **sin pararse**
  (antes, 1,2 s por viaje) y se deja en el perchero de la puerta de salida, sin rodeos. El jugador solo la coge **al meterse en el agua**
  (la del perchero de esa puerta, a < 6 m, o tocando una estación); antes la cogía al pasar junto a cualquier perchero y la devolvía al
  alejarse (en la jornada, Kushal iba y venía con ella). «En el agua» cuenta también la cabeza o el cuerpo sumergidos, aunque el cuerpo
  de nado no esté activo (con la máscara puesta el aliento no baja).
- **Ronda 3**: el jugador solo coge la máscara del perchero en el segundo y medio en que se mete en el agua; ya dentro, de una estación (buceando hacia la estación la cogía del perchero de fuera, a través de la pared). Que el aliento baje mientras va a por ella es lo esperado: la máscara es para quien la lleva puesta. `ArbolCampos.Route` apunta `[Ruta-árbol]` cuando el jugador entra en un tronco o vuela a otro árbol, y por qué.
- **Percheros**: si uno queda dentro del agua al empezar, pasa al otro lado de su puerta (o se aparta de la pared); si se queda sin
  máscaras, el carro le trae la mitad de las del puesto más lleno del edificio (además del reparto de cada noche).
- **Jardinero**: entra por el **techo** (panel en su puerta) y baja una sola vez por las estaciones de hoy hasta el fondo: ~0,7–0,85 h
  por árbol (subir desde el suelo y volver contaba la altura del tronco dos veces: 1,06 h en el árbol más pequeño).

**Ronda 19:** cada tramo de tareas del árbol dura como mucho 1 h de juego (`FruitGardener.TreeStretches`). Lo que no cabe, estaciones en rojo incluidas, pasa al tramo siguiente del mismo día, en otra ronda si hace falta; una estación que ella sola pasara de 1 h va en un tramo propio.
