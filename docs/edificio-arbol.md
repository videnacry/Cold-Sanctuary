# El Árbol de los Campos (aprobado y construido, 2026-10-29)

Idea de Berón: juntar los campos en un solo edificio con forma de árbol.
- **Raíz:** una columna central muy fuerte de unos 100 m.
- **Tronco:** pisos de cultivo inclinados, cada uno al revés que el de arriba (un acordeón).
- **Ramas:** columnas hijas inclinadas que nacen de la raíz.
- **Flores o frutos:** en la punta de cada rama, un anillo de nidos como los del hospital y el restaurante.
- **Giro:** la raíz gira y con ella gira todo el árbol, para repartir sol, sombra y lluvia.

## Lo que decidió Berón (2026-10-29)

- **Adelante con el árbol.**
- **Alfombras solo arriba y abajo**: en la COPA (la única plataforma plana; gira con el árbol) y en el SUELO, al pie. Entre
  pisos no hay alfombras: se va por dentro de la raíz.
- **La raíz es un depósito de agua**, como el tanque de virtualización: cada piso tiene una puerta a ella; un campo magnético
  impide que el agua salga por las puertas. Se entra en la raíz, se nada hacia arriba o hacia abajo y se sale por la puerta del
  piso que se quiere. Dentro se puede nadar libremente.
- **La raíz mide unos 20 m de ancho**: es el único apoyo de todos los pisos y debe ser al menos tan ancha como la plataforma
  de alfombras de su copa.
- **La raíz es TRANSPARENTE como el tanque de virtualización**: se ve a la gente nadar de piso en piso. Nada de un tubo opaco
  de acero y hormigón: la pared es de acrílico grueso y la carga la lleva un exoesqueleto esbelto de nervios de acero.

## Cómo está hecho

Código: `Assets/Editor/SampleSceneBuilder.Tree.cs` (lo construye), `Assets/Scripts/World/TreeSpec.cs` (las medidas, una sola
vez), `TreeOfFields.cs` (niveles, rutas a nado, el campo magnético), `TreeRotator.cs` (el giro), `StructuralLimits.RootCheck`
(la raíz aguanta) y `Assets/Scripts/Tests/TreeOfFieldsTest.cs` (la prueba, grupo 53).

| Parte | Medidas | Notas |
|---|---|---|
| **Raíz** | 0–101 m; agua Ø 20 m (de 0,05 a 99,6 m); pared de acrílico de 1 m (Ø 22 m) | El mismo cristal que el tanque de virtualización (`VirtualizationTank.GlassColor`, alfa 0,18) y su agua, más clara (alfa 0,2: 20 m de agua). El agua es un `WaterZone` (la superficie es lo alto del volumen): se nada con `SwimBody` como en el mar. Entre el agua y la tapa quedan 1,4 m de aire. |
| **Exoesqueleto** | 16 nervios CFST (Ø 0,8 m) y zunchos cada 2 m | Una sola malla; no cruza las puertas ni las bocas de las ramas. Es lo que carga: el acrílico no se cuenta como portante. |
| **Puertas** | Una por nivel: suelo y 8 pisos (y una en la campana de cada rama); 1,8 × 2,6 m | Hueco en la pared con bobinas de cobre y el **campo magnético** (la lámina de luz del tanque) en la cara de dentro. Los cuerpos lo cruzan; el agua no. El hueco de cada puerta es aire: una **campana de aire** para respirar a mitad de subida. |
| **Tronco** | 8 pisos: 6 · 13 · 24 · 31 · 38 · 45 · 52 · 62 m | Octógonos de ~51 m (rectángulo de 46 × 46 + 2,5 m y chaflanes de 5 m, como las losas de las torres) con el hueco de la raíz. En voladizo: ménsulas y tornapuntas de acero hasta la raíz. **Girados 45°** uno sobre otro e **inclinados 3°** alternando el sentido. Entre el 2 y el 3, 11 m (la guadua llega a 9 m); entre el 7 y el 8, 10 m (el dosel del bosque comestible); los demás, 7 m. |
| **Riego en cascada** | Canaleta en el borde bajo de cada piso | Vierte (una lámina de agua, sin collider) al piso de abajo. El arrozal no: va en terraza plana y su agua vuelve a la raíz. |
| **Ramas** | 6 tubos de agua Ø 3 m, a 35°, de 40 m, desde 66 m | A 0°, 67,5°, 112,5°, 180°, 247,5° y 292,5° (entre los nervios). Collar de acero en la boca. La punta llega a ~44 m del eje y ~89 m de alto: sobresale de los pisos. |
| **Anillos de nidos** | `RingDeck` de 13,4 m alrededor de una campana de agua (Ø 4 m) | Frutales enanos en las esquinas, flores para las abejas de las nieves y nidos de aves. A ~91 m. Se llega nadando: por la raíz, por la rama y por la puerta de la campana. Se llaman `Nido_anillo_N` (no son anillos de un piso: no llevan alfombras). |
| **Copa** | Octógono plano de 22 m a 101,4 m | Tan ancho como la raíz. Alfombras de edificio y del santuario. Gira con el árbol. |
| **Suelo** | Alfombras en el hielo, a 22 m del eje (al NE) | No giran. Desde ahí se anda a la puerta del suelo. |

**Reparto, de abajo arriba:** P1 Arrozal (en terraza plana) · P2 Bambusal · P3 Milpa · P4 Chacra andina · P5 **Huerto**
(`Garden_Area`, con las hortalizas, su tanque de virtualización y su iniciación) · P6 Rotación · P7 y P8 Bosque comestible.
Cada cultivo conserva su gremio y su semilla (`GardenPlot`): hortalizas 11, milpa 12, chacra 13, arrozal 14, rotación 15,
bosque 16 y 18, bambusal 17. Cada bancal es una franja de 38 × 7 m en el lado del piso opuesto a su puerta (el Huerto, un área
de 44 × 9 m con su bancal de 30 × 6 m). Cada piso tiene sus plazas reservadas del microcosmos (`PlatformSlots`), calculadas
con el árbol sin girar.

## Moverse

- **Personajes (`PathMover`)**: `ArbolCampos.Route` va antes que las alfombras. En el mismo piso, andando (sin alfombras); a
  otro nivel, primero a la puerta de su piso y después un **viaje a nado** (`TreeTrip`): puerta → agua → por la columna a 6 m
  del eje → [la rama y su campana] → puerta → fuera, nadando con `SwimBody.SwimTo` a 2,5 m/s (100 m en 40 s: no se quedan sin
  aire; la prueba lo mira). Nada de teletransportes; si se atasca, se queda donde está y avisa. Desde fuera del árbol, las
  alfombras le traen al pie (o a la copa) y entra por el suelo.
- **El jugador**: nada libremente dentro (el cuerpo de nado del mar). En cada puerta puede respirar. El hechizo
  `piso({n:5})` (0–8, `anillo1`…`anillo6`) le lleva andando a su puerta y nadando por la raíz.

## Giro

`TreeRotator` gira el árbol en sentido **contrario al sol** (mira hacia dónde avanza el acimut del sol del santuario), por
defecto **una vuelta cada 6 horas del juego** (15 min reales). Hechizo `arbol({vueltaHoras:N})`; `0` lo para.
- Quien está de pie en un piso, un anillo o la copa **gira con él** (se le aplica el mismo giro cada frame; con
  CharacterController, con `Move`).
- El árbol tiene **su propio NavMesh** (`NavMeshSurface` en su raíz, horneado en sus coordenadas, fuera del horneado general)
  que se recoloca con él cada 0,25 s. Al pie de la raíz, en el NavMesh general, el círculo de la columna no se anda.
- Los muebles de un piso que gira se miden en el marco del piso (`FurnitureSpacing.Frame`): los 2 m de paso no dependen del giro.

## Estructura (`StructuralLimits.RootCheck`)

Con 8 pisos, 6 ramas, la copa y su pared: ~286 MN sobre 16 nervios (~614 MN) → **47 %**. El agua (~308 MN) apoya en el fondo y
va al cimiento. El empuje del agua en el fondo (p·r ≈ 9,8 MN/m) lo resisten el acrílico de 1 m y los zunchos → **38 %**. Avisa
por encima del 80 % (con 30 pisos, avisa) y da error por encima del 100 %.

## Dónde

En el lóbulo **Sur:Milpa**, centro (−50, −84): la raíz y la puerta del suelo (que gira a 13,5 m del eje) quedan sobre hielo
firme. La torre de **Producción** sigue en (18, −95), más baja: Construcción (12 m), Mecánica (23 m), Textil (34 m) y su
techo de alfombras (45 m). Distancias medidas con el simulador (`-tree`, puntos reales de las mallas, el árbol girando):

| Vecino | Hueco a los pisos | Hueco a los anillos |
|---|---|---|
| Producción | 22,3 m | 4,1 m en planta (los anillos van por encima de 82 m; la torre llega a 48 m con sus paneles) |
| Entrenamiento | 41,0 m | 22,9 m |
| Ciencias | 85,4 m | 67,2 m |
| Central Nuclear | 74,7 m | 56,5 m |

## Riesgos y pendientes

- Los anillos pasan a 4 m en planta del techo de Producción (con más de 30 m de altura entre ellos). Si Berón quiere los 16 m
  de la regla 4 también en planta, hay que mover Producción (no cabe en su lóbulo) o acortar las ramas.
- Un jugador que se salga nadando por las juntas de una rama con su campana caería: las juntas están cerradas con collares,
  pero no es una pieza estanca. Probar en vivo.
- El alga del arrozal se planta en el mar más cercano y es hija del bancal: gira con el árbol.
