# El suelo y los elementos de los microcosmos (2026-11-04)

Berón, 2026-11-04 (resumido): en Ambrosio la vegetación se crea bajo el suelo; que la vegetación nazca siempre encima y que haya una
capa de tierra de ~10 cm con irregularidades, cúmulos y piedras; un hospital propio para los microcosmos que tenga todo lo del
hospital del mesocosmos y que avise a los microcosmos cuando el piso cambie; tres versiones de cada piso (terminado, en
construcción y microcosmos) con los muebles reinterpretados en terreno y edificios (lavabo = lago, cajonera = edificio, caja =
cueva o fábrica, zapato = cueva o mercado, hornillas = volcanes, foco = sol, lámparas = postes de luz, mesas y sillas = montañas o
nevados, alfombras quitadas, apiladas o como desniveles); alfombras más gruesas, con circuitos; en la plaza de Bolívar todo
parecía pequeño para los insectos-humanos (sillas y bancos que se puedan usar, platos y ollas para un ciudadano, la estatua tenía
forma humana).

Las reglas están en el [manual de diseño](manual-de-diseno.md): F2, F3, C7, H8, M9, M13, M30, M32, M33, M34 y §11.

| Pieza | Dónde | Qué hace |
|---|---|---|
| `MicroGround` | `Assets/Scripts/Microcosmos/MicroGround.cs` | La superficie de encima de un pie (`TopSurface`), la regla de la flora (`ValidateFlora`) y el apoyo (`Settle`). |
| `MicroSoil` / `MicroSoilBuilder` | `MicroGround.cs` / `Assets/Editor/MicroSoilBuilder.cs` | La capa de tierra en mosaicos combinados y el pueblo subido a ella (`RaiseTown`). |
| `MicroFloorElements` | `Assets/Scripts/Microcosmos/MicroFloorElements.cs` | El catálogo de elementos, el inventario de un piso, su firma, la sincronía (`Missing`), los cambios de historia (`Overrides`), las dependencias (`DependentsOf`, `Record`, `Stale`); `MicroElement` y `MicroFloorForm`. |
| `MicroFloorDress` | `Assets/Editor/MicroFloorDress.cs` | La forma de microcosmos vestida: cada pieza, en su sitio, con su versión de microcosmos (geometría combinada). |
| `FloorForms.MicroName` | `Assets/Scripts/World/FloorForms.cs`, `SampleSceneBuilder.FloorForms.cs` | La tercera forma («Forma_microcosmos»): oculta en el santuario con su inventario; el aviso de microcosmos por revisar. |
| `MicroCivicScale`, `MicroSeat`, `PublicSeatSpell` | `Assets/Scripts/Microcosmos/MicroSeat.cs` | Las medidas del mobiliario cívico, los asientos utilizables y sentarse en ellos. |
| `MicroStatue` | `Assets/Editor/MicroStatue.cs` | La figura de insecto-humano de piedra de las estatuas. |
| Pruebas 64 y 67 | `Assets/Scripts/Tests/MicroSueloTest.cs` | Casos sin escenas (64) y los microcosmos construidos, uno a uno (67). |

---

## Flora: encima, nunca debajo

**Por qué salía bajo el suelo (Ambrosio y otros).** `AreaMicrocosmos.Build` baja el suelo «Mat/Suelo» del área gigante 2 cm del piso
real (2 m a ×100) para que quede debajo de todo, y toma como suelo del microcosmos (`groundY`) el MÁS BAJO. La flora se ponía a la
altura del suelo «más cercano a `groundY`» (`FloorNear`), que es ese suelo bajado, aunque encima hubiera otra losa: el pasillo y las
salas del hospital (sus losas, a ×100, de metros), una alfombra, la tarima. La planta quedaba debajo de la losa.

**La regla.** Después de sembrar, `MicroGround.Settle` apoya cada planta (los hijos de «Flora_a_escala», los frutales y plantas de
los edificios, lo que lleva un `Affordance` vegetal) y cada bicho de «Fauna_a_escala» en la superficie MÁS ALTA de encima de su pie:
un rayo desde 6 m (`Probe`) por encima del pie, contra los colliders del microcosmos (no los del santuario: mientras se construye,
el microcosmos está en el origen), que solo cuenta lo que TAPA el pie (una capa apoyada a su altura o por debajo: la tierra, una
alfombra, una losa), no una balda que cuelga por encima; también las caras de arriba de los suelos sin collider. La planta que queda
DENTRO de un collider (una pata, un edificio, una roca) se quita. Ni las plantas ni los bichos ni la gente cuentan como superficie.

**Se comprueba** en `MicroFloors.Validate` (regla 7): ninguna planta con el pie más de 5 cm por debajo de su superficie, ninguna
dentro de un collider. Al construir queda apuntado `MicroFloor.plantsSettled` / `plantsRemoved` y el registro
`[Tierra] … plantas y bichos apoyados …`.

## Capa de tierra

- **Grosor: 10 cm** (`MicroGround.SoilThickness`) en metros del microcosmos, que son los de un insecto-humano (mide como una
  persona): 10 cm reales. No 10 cm del santuario × 100 (serían 10 m de tierra enterrando los muebles bajos y el pueblo). Lo de
  «lo necesario para cubrir las superficies» se cumple porque la tierra **sigue la cara de arriba más alta** de los suelos del piso
  en cada punto (el suelo, las alfombras, las losas): siempre las cubre.
- **Irregular**: ruido Perlin (±4 cm; nunca menos de la mitad del grosor), **cúmulos** (lomas de 15–60 cm y 1,5–4 m de radio en el
  barrio de la historia; de 0,3–2 m y 8–20 m de radio en el resto del piso gigante) y **piedras** medio enterradas (8–50 cm en el
  barrio, 0,3–1,5 m fuera), con su collider (se anda por ella; entra en el NavMesh).
- **Lisa** bajo la historia y su pueblo (sus huellas y calles: ni cúmulos ni piedras) y **sin tierra** sobre el agua de la historia
  (ríos, lagos, el mar del plano).
- **Placas** de tierra sobre las caras de arriba de los muebles poblados (se afinan hacia el borde), salvo donde hay un mirador del
  barrio alto.
- **La historia y su pueblo van encima**: la «Historia» sube el grosor de la tierra, y cada edificio o tramo de calle que quedara con
  la base por debajo de la tierra que tiene debajo (sobre una alfombra) se sube a ella (`MicroSoilBuilder.RaiseTown`; nunca se baja).
- **Renderers**: un mosaico de ≤ 64 × 64 celdas es UNA malla (tierra y piedras en dos submallas) con su MeshCollider. Paso de 2 m o
  más en el barrio (≤ ~240 celdas por lado) y 12–30 m en el resto: unos 30–60 mosaicos por microcosmos.

## Elementos: las tres formas de cada piso

Cada piso tiene **tres formas** (`FloorForms`): la **finalizada**, la **en construcción** y, desde ahora, la de **microcosmos**
(`FloorForms.MicroName` = «Forma_microcosmos»).
- En el **santuario** es un hijo OCULTO del área con su `MicroFloorForm`: la lista de sus piezas, cada una «clave · elemento · versión
  de mesocosmos → versión de microcosmos · ruta», la firma de su inventario, los microcosmos que dependen del piso y las piezas sin
  fila (registro `[Pisos] … sin fila en el catálogo`). La prepara `SampleSceneBuilder.EnsureFloorForms`.
- En el **microcosmos** es un hermano de la copia gigante del piso con un `MicroElement` por pieza (de qué pieza sale, qué elemento
  es, qué es en el microcosmos, el cambio de la historia si lo hay) y la geometría de su versión de microcosmos.

**El inventario** (`MicroFloorElements.Inventory`): cada mueble ENTERO del piso (la raíz que lleva collider o malla, o cuyo nombre es de
un elemento) y cada losa; los contenedores vacíos se recorren; no cuentan la obra, las formas preparadas, los anillos, la estructura
elevada ni la máquina de virtualización. Es el mismo en el piso del santuario y en su copia gigante: el paso meso ↔ micro es **uno a
uno**. La clave de una pieza es su nombre sin el número de copia («Cama_12» → «cama»).

**La versión de microcosmos conserva el sitio de cada pieza** (no se amontonan en los bordes): la pieza gigante se queda y se viste
encima y alrededor. La versión de mesocosmos es la pieza del santuario tal cual (más plana y sencilla); la de microcosmos, el detalle.

### Tabla

Manda la primera fila cuyo nombre encaja (las claves de varias palabras primero: «silla de ruedas» no es una silla; los plurales
cuentan; «pilar» no es una «pila»).

| Elemento | Mesocosmos | Microcosmos | De quién |
|---|---|---|---|
| bombilla, foco | bombilla o foco del techo | el SOL: esfera que brilla y luz puntual de largo alcance | Berón |
| lámpara, farol, flexo | lámpara | POSTE DE LUZ gigante con su farol encendido y diez faroles de 4 m alrededor | Berón |
| hornilla, quemador, fogón, placa, horno | fogón | VOLCANES: uno por fuego (o cuatro), con su cráter de lava y su fumarola | Berón |
| lavabo, fregadero, pila | lavabo | LAGO en su cuenca con su orilla; con el grifo abierto, CASCADA y RÍO que baja y corre por el suelo | Berón |
| cajonera, cómoda, armario, taquilla, estante, nevera | mueble alto | EDIFICIO de la era: una planta cada 3,2 m con su banda de ventanas, pilastras, puerta y azotea (depósito y antena desde 1900; cornisa antes; piedra, ladrillo o acero y cristal según la época) | Berón |
| caja, baúl, cajón | caja | CUEVA (antes de 1750) o FÁBRICA (tejado en diente de sierra, chimeneas con humo, tuberías, portón) | Berón |
| zapato, bota | zapato | CUEVA (antes de las ciudades) o MERCADO (puestos con toldo alrededor) | Berón |
| silla, taburete, banco, sofá | asiento | NEVADO: contrafuertes de roca en las patas, nieve en el respaldo y en el borde del asiento | Berón |
| mesa, escritorio, mostrador, encimera, barra | mesa | MONTAÑA de meseta: contrafuertes de roca en las patas y peñascos en el borde del altiplano (el centro, libre: se puebla) | Berón |
| alfombra, esterilla, tatami | alfombra | DESNIVEL: una terraza con rampas de tierra (sin collider en un pueblo); apiladas, un CÚMULO; o se retira | Berón |
| estatua, busto | estatua humana | COLOSO de insecto-humano (la humana se apaga) | Berón |
| cama, camilla, colchón, cuna | cama | COLINAS de pradera | decidido aquí |
| cojín, almohada | cojín | DUNAS | decidido aquí |
| bañera, ducha, pecera, piscina | recipiente de agua grande | MAR INTERIOR con su playa | decidido aquí |
| olla, sartén, cuenco, taza, plato | recipiente hondo | LAGO DE CRÁTER | decidido aquí |
| botella, jarra, bombona, frasco, silo | lo alto y estrecho | TORRE o FARO | decidido aquí |
| máquina, monitor, ordenador, respirador, analizador… | aparato | FÁBRICA o CENTRAL | decidido aquí |
| gotero, perchero, soporte, antena | lo fino y alto | ANTENA con su luz roja | decidido aquí |
| carro, carrito, silla de ruedas | lo que rueda | ESTACIÓN con marquesina | decidido aquí |
| perrera, jaula, corral | lo que encierra | FORTALEZA con almenas | decidido aquí |
| planta, maceta, árbol, jardinera, bancal | planta | BOSQUE de árboles a escala del pueblo | decidido aquí |
| espejo | espejo | GLACIAR | decidido aquí |
| libro, papel, carpeta | papeles | MESA DE ESTRATOS | decidido aquí |
| pared, panel, pizarra, cuadro, puerta, mampara | lo vertical y plano | ACANTILADO | decidido aquí |
| columna, pilar, viga, poste | pilar | AGUJA DE ROCA | decidido aquí |
| escalera, rampa | peldaños | TERRAZAS | decidido aquí |
| suelo, losa, zona, pasillo | suelo | LLANURA: la capa de tierra | decidido aquí |
| cualquier otra | — | AFLORAMIENTO de peñascos al pie (y se apunta para darle su fila) | decidido aquí |

Las piezas gigantes se quedan (el piso no se rompe, regla M6): lo nuevo va en «Forma_microcosmos», en dos mallas combinadas por
trozo de 60 000 vértices: el **terreno** (laderas, contrafuertes, rampas: con collider, se anda) y el **detalle** (ventanas, nieve,
agua, lava, toldos, humo: sin collider). Pocos renderers aunque el piso tenga cientos de piezas. Las estatuas y los soles son
objetos aparte (una figura, una luz).

### Cambios de historia

`MicroFloorElements.Overrides`: una historia puede **quitar** una pieza (se apaga: sigue contada, el piso no se rompe), **apilar** las
alfombras en un cúmulo, **moverla** o darle **otro elemento**. Cuenta como su versión de microcosmos. Hoy: Angostura apila las
alfombras de la cocina (el cerro de la ciudad), la Tregua de 1914 quita las alfombras (la tierra de nadie), Sils Maria hace nevado la
mesa (la Engadina) y Chernóbil hace mar el lavabo (el estanque de refrigeración). Un microcosmos hecho de cero para una historia
sigue pendiente.

## Hospital (y todos los pisos): sincronía y dependencias

El hospital propio de los microcosmos es la **forma de microcosmos del piso del hospital**: se hace desde el inventario del hospital
del mesocosmos (cada instrumento, mueble y decoración, su versión de microcosmos: la cama, colinas; el monitor y el respirador,
centrales; el gotero, antenas; la silla de ruedas, estaciones; el lavabo, lagos…), así que no puede quedarse atrás. Lo mismo para
cada piso.
- **Sincronía**: `MicroFloorElements.Missing(piso del santuario, forma de microcosmos)` da las claves del piso sin su versión de
  microcosmos. Al construir se avisa (`[Forma microcosmos] … sin su versión de microcosmos`); la prueba 67 lo exige con el piso del
  santuario cargado; la 64 lo prueba con casos.
- **Dependencias**: `MicroFloorElements.DependentsOf(piso)` (de `MicroFloors.Table`) y el registro local
  `UserSettings/MicroFloorDeps.txt` (escena, piso, firma, inventario). Si el piso cambia, `EnsureFloorForms` y el menú
  «Pisos: microcosmos por revisar» avisan qué microcosmos reconstruir y qué cambió (manual §11).

## Alfombras

Las alfombras voladoras (`CarpetPad.BuildCarpet`) miden ahora **7 cm de canto** (`FlyingCarpet.Thickness`): tejido abajo, NÚCLEO de
circuitos (placa oscura) con PISTAS doradas que asoman por los cuatro cantos, tejido y dibujo arriba, flecos, y una COSTURA DE LED
(cian, emisiva) a media altura del canto. Quien vuela va sentado encima de su grosor y al bajarse queda de pie encima. Siguen sin
collider sólido (solo el disparador): el NavMesh no cambia y la pista sigue sabiendo qué alfombra ocupa cada hueco. En el
microcosmos, la copia ×100 de una alfombra mide 7 m: un desnivel (o un cúmulo, si la historia las apila).

## Escala del mobiliario cívico

**Decisión: medidas por pieza, no un ×2 global** (`MicroCivicScale`):
- lo que el cuerpo **usa** se mide con el cuerpo (`MicroBody`): el asiento a la altura de su vientre, la mesa a la de sus manos, la
  cama a su largo. Un ×2 dejaría el asiento al doble del vientre y la mesa a la altura de los ojos: nadie se sentaría ni comería;
- lo que **alimenta**, por raciones: el plato ≥ 0,26 m (una comida de ~600 g para 1,75 m de estatura, proporcional), la olla con
  2 L por comensal (la de una cocina, para 8: ~0,31 m, 16 L; la sartén ×1,3);
- lo que se **mira** es monumental: la estatua ≥ 2,1 × el ciudadano (pedestal de 1,3 m y figura: ~3,7 m), la fuente ≥ 2,4 × de lado
  (~4,2 m, con su taza alta, caños y chorros) y su pretil a 0,45 m, que es un asiento.

**Todo asiento se usa**: cada silla y banco lleva su `MicroSeat` (una silla, un sitio; un banco de insecto-humano, tres asientos de
abdomen sobre un larguero; el pretil de la fuente, ocho sitios). La plaza lleva tres bancos, el monumento dos y el parque uno. En su
tiempo libre de día, los ciudadanos con horario se sientan de vez en cuando en el asiento público libre más cercano
(`PublicSeatSpell`: andar con tope de 45 s, `HomeMove.Sit`, un cuarto de hora, levantarse); los asientos de las casas son de su
familia. Ya existía sentarse en la mesa de casa (`HomeNeedSpell`).

**El jugador más grande que el comedor**: el jugador entra como su insecto a su estatura humana (`InsectOnly.Admit`); si sigue
pareciendo más grande, la foto de la sonda (`TmpCheck/Microcosmos suelo fotos`) registra la estatura del jugador frente al ciudadano
mediano para verlo.

## Estatuas

`MicroStatue.InsectFigure`: el cuerpo de un insecto-humano de verdad (`CubeInsect.DressFolk`, su especie por el papel que representa,
`InsectCast.SpeciesFor`), de pie, sin conducta ni colliders y todo de piedra o bronce, con el nombre de la estatua. Lo usan la
estatua de la plaza (`MicroFurniture.Statue`) y las estatuas del piso en la forma de microcosmos (Patanjali, Buda y Shiva: colosos;
la humana se apaga). La estela maya y el león guardián chino no son figuras humanas y siguen como eran.

## Pruebas y fotos

- **64 · `MicroSueloTest`** (sin escenas): una planta bajo una losa se ve enterrada y Settle la apoya; una dentro de una pata se ve y
  se quita; el grosor de la tierra; el catálogo (las filas de Berón, «pilar», «silla de ruedas», las claves); el inventario, la firma
  y la sincronía; quién depende del hospital; el canto de la alfombra; plato, olla, estatua y fuente; el banco sin y con MicroSeat;
  el asiento libre más cercano; la estatua humana frente a la de insecto-humano.
- **67 · `MicroSueloEscenasTest`** (carga uno a uno Bolívar, Ambrosio, el Hospital, el río de Sakshi y la Cocina): flora encima, la
  tierra con sus mosaicos y la historia encima, la forma de microcosmos con una versión por pieza y en sincronía con el piso del
  santuario, la escala cívica del pueblo y la estatua y la fuente frente al ciudadano, y las alfombras del santuario.
- La regla 7–9 de `MicroFloors.Validate` y las 13–15 de `MicroBuildRules` las ven también 43, 60, 62, 63 y 66.
- **Sonda de fotos** (fuera del repo): `Tools/TmpCheck/Microcosmos suelo fotos` y `Alfombra canto foto`.
