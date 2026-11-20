# Hawthorne 2008, el distrito desde cero (PB-165)

Berón, 2026-11-19 06:01: el piloto viejo se veía mal (la taquería era un cubo sin ventanas, el mercado una caja gris en
medio de la nada y los edificios estaban sueltos). Se rehace **desde cero**, sin el piso del árbol-edificio.
Primero el distrito denso, como en GTA; después los interiores al detalle; al final los retos de código y las jornadas.

Fuentes del barrio: el informe de investigación de este hilo (§6) más lo que ya estaba en
[microcosmos-musk-historia.md](microcosmos-musk-historia.md) §4. [C] = confirmado con fuente, [S] = supuesto.

## 1. Qué cambia respecto al piloto

| Antes | Ahora |
|---|---|
| Montado sobre el piso del árbol-edificio ×100 (muebles gigantes alrededor, huecos) | **Suelo urbano propio**: el microcosmos no carga el piso del santuario. Es un rectángulo de calles y manzanas cerrado por su borde |
| Lotes sueltos de `CivicBuilder.Town` (cajas de 10–24 m con 2 estaciones) | **Cada edificio tiene su constructor**, con fachada (ventanas, escaparates, rótulos, toldos), interior por estancias y maquinaria real |
| Mesas mixtas (sillas de persona + apoyos de insecto) | En Hawthorne todos son insectos-humanos: **solo mobiliario de insecto-humano** (mesa alta con apoyo de abdomen, catre de vientre…). El validador ya no exige muebles de persona (`MicroBuildRules.ValidateSites`) |
| Muebles, objetos y puertas a escala s = 0,5 (se ven diminutos) | Puertas, muros y ventanas ×3; muebles y objetos ×2 en planta y como mucho a la altura del pecho; la misma separación (ver §5) |

## 2. El plano (zona detallada de unos 420 × 340 m)

Ejes: x = este, z = norte. Crenshaw Blvd es el eje N–S del distrito. La fábrica queda al oeste de Crenshaw y el
aeropuerto al norte de la fábrica. La I‑105 elevada, con la estación de la Green Line, cierra el norte.

```
 z
 +170 ══════════ I-105 elevada (muro de sonido) ═══ Estación Crenshaw (andén en la mediana) ══════════
 +150 [Park-and-ride de la estación]   ║ Crenshaw ║  [Casas de Hollypark · 1 planta, estuco]
 +120 ─────────── W 120th St ──────────╬══════════╬─────────── W 120th St ───────────────
 +100 [Aeropuerto: hangares, torre,    ║  6 carr. ║  [Gasolinera esquina] [Strip mall: taquería ·
      terminal con Nat's Airport Cafe] ║ +mediana ║   donuts · lavandería · licorería · tienda]
  +40 [Garita · aparcamiento norte]    ║ paradas  ║  [Supermercado + muelle de carga]
    0 [FÁBRICA DE SPACEX: oficinas,    ║ 210/710  ║  [Taller de coches] [Naves de Wilkie Ave]
      control, nave de montaje, taller]╠══════════╣
  -60 ── Rocket Road (acceso) ─────────╬ semáforo ╬─────────── Wilkie Ave ────────────────
 -120 [Aparcamiento sur de SpaceX]     ║          ║  [Casas y apartamentos de 2 plantas]
 -170 [Naves tilt-up Century Business Center, hacia Jack Northrop Ave]
      x -250 ............................ 0 ......... +40 .......................... +170
```

Fuera de la zona detallada, de fondo y sin interiores: la pista 7/25 entera, el resto de naves de Northrop/Vought,
Hawthorne Plaza cerrado y las colinas de Palos Verdes.

## 3. Edificios, qué son y qué llevan dentro

| # | Edificio | Real o supuesto | Uso | Qué debe tener para usarse |
|---|---|---|---|---|
| 1 | **Fábrica de SpaceX** (1 Rocket Road), solo el tramo este (≈150 × 100 m de los ≈300 × 150 m reales) | [C] uso, 3 turnos, sin ventanas en la nave, grúas puente de Boeing; [S] medidas | Fabricar el Falcon 1 y empezar el Falcon 9 y el Dragon | Recepción y garita; planta abierta de cubículos (el de Musk con pósteres de Marte); control de misión tras cristal; nave de montaje con la etapa del Falcon 1 en su cuna, grúas puente, banco del Merlin 1C, sala limpia de aviónica con su vestuario, banco de composite, soldadura FSW de tanques; taller de mecanizado (tornos y fresadoras CNC, almacén de barras); laboratorio de materiales; cocina y comedor; aseos; muelle de carga con carretilla |
| 2 | **Terminal del aeropuerto con Nat's Airport Cafe** | [C] terminal de 2 plantas de 1966, Nat's desde 1994, torre de 15 m de 1961 | Desayunos con vista a la pista | Barra con taburetes de apoyo, plancha y freidora, cafeteras, vitrina de tartas, mesas junto al ventanal; arriba, salón de banquetes con bar |
| 3 | **Hangares y torre** | [C] torre; [S] hangares de chapa ondulada | Avionetas y helicópteros | Avionetas, carro de combustible, banco del mecánico, mangas de viento |
| 4 | **Estación Crenshaw (Green Line)** | [C] andén central elevado en la mediana de la I‑105, 506 plazas de park-and-ride, azulejos *Crenshaw Stories* | Ir y volver del trabajo | Escaleras y ascensor, máquinas de billetes, validadores, aparcabicis, marquesina del andén, tren estacionado |
| 5 | **Gasolinera de esquina** (Crenshaw/120th) | [S] | Repostar y comprar algo rápido | Marquesina con 4 surtidores, compresor de aire, tienda con neveras de bebidas, mostrador y lotería |
| 6 | **Strip mall de Crenshaw**, locales contiguos de 1 planta | [S] tipología típica de LA | Comer y servicios | **Taquería**: escaparate con rejas, menú iluminado, plancha y comal, trompo al pastor, vaporera, barra de salsas, nevera vertical de refrescos y 8 mesas altas atornilladas con apoyo de abdomen. **Donuts**: vitrina, freidora, cafeteras y mesas. **Lavandería**: 12 lavadoras y 8 secadoras, mesa de doblar y máquina de cambio. **Licorería y tienda**: estanterías y mostrador |
| 7 | **Supermercado** | [S] cadena de 2008 | Comprar comida | 4 cajas con cinta, 6 pasillos de góndolas, frutas y verduras, carnicería, panadería, murales de lácteos y congelados, carros, almacén y muelle con el camión de reparto |
| 8 | **Taller de coches** | [S] | Reparar coches | 2 elevadores, foso, compresor, cambiadora de neumáticos, equilibradora, bancos y armarios de herramientas |
| 9 | **Naves tilt-up** (Century Business Center, Wilkie Ave) | [C] tipo y altura libre de 7,3 m | Proveedores de la industria aeroespacial | Estanterías de palés, puente grúa, oficinas al frente |
| 10 | **Casas de Hollypark y apartamentos** | [C] barrio; [S] tipología de los años 40–50 | Donde vive la gente (los ingenieros jóvenes, en los apartamentos) | Cocina, baño, dormitorios con catre de insecto-humano, salón, garaje, jardín y valla |

**Calle y mobiliario urbano** [S, salvo los bordillos, que son ley]: Crenshaw con 6 carriles, mediana con giros, aceras
de 3 m, semáforos de mástil con brazo, farolas cobra de 9 m, postes de madera con transformador y cables,
hidrantes, marquesinas de Metro (la del Rapid 710 en rojo), bordillos de color (rojo, amarillo, blanco, verde, azul;
California Vehicle Code §21458), pasos de cebra, palmeras washingtonia y jacarandas en alcorques, contenedores,
aparcamientos con plazas pintadas, vallas de malla en la zona industrial y coches de 2008 aparcados.

## 4. Orden de construcción

1. **Suelo urbano propio**: un interruptor del molde de microcosmos (`MicrocosmSpec`) para no cargar el piso del santuario.
2. **Calles y manzanas llenas**: todos los edificios por fuera (fachadas, ventanas, rótulos y azoteas con climatización) y el mobiliario urbano.
3. **Interiores**, de uno en uno: fábrica → taquería → supermercado → gasolinera → Nat's → taller → casas → resto.
4. **Gente**: las jornadas por oficio que ya existen, con cada puesto en su edificio nuevo.
5. **Retos de código**: los enganches de `MissionSites` y `CodeChallenges` pasan a los nombres nuevos.
   - **Todo es un reto, también los diálogos** (Berón, 2026-11-19 19:10). Las misiones de Hawthorne son de trabajar, de entablar amistad con un personaje, de ser su estudiante, de combatir a su lado o de protegerle. Muchas pasan por un diálogo, y ese diálogo es un reto más que hay que resolver. Los retos de diálogo enseñan la historia real del escenario: Hawthorne en 2008, SpaceX antes del vuelo 4, sus trabajadores y el barrio, con fuentes.
6. Compilar con `tools/verify-compile.sh`, push y PR a `edificios`. La prueba en vivo la hace el hilo de pruebas.

## 5. Escala (decidida por Berón, 2026-11-19 06:21)

> «Para que mesas y mostradores y otros elementos no queden encima de la cabeza crecerían menos en vertical pero seguirían
> creciendo en ejes equis y zeta, en vertical pueden llegar hasta el pecho… puertas y paredes y ventanas *3, en objetos *2».

- El cuerpo del insecto-humano **no cambia** (s = 0,5).
- **Puertas, muros y ventanas ×3** (`ShellK`): la altura de cada edificio, sus puertas, sus escaparates y sus ventanas.
- **Muebles y objetos ×2 en planta** (`FurnitureK`, `DPiece`): los muebles del registro, hechos a la medida del cuerpo,
  crecen ×2 en X y Z y conservan su altura. Las máquinas y los objetos propios del distrito van a su medida real, que ya es
  el doble de la del cuerpo encogido.
- **Las superficies de uso, como mucho a la altura del pecho** (`ChestHeight`, `ClampUsable`): mostradores, barras, mesas,
  cintas de caja, vitrinas, fregaderos y lavadoras se bajan al pecho del insecto-humano. Lo que va encima baja con ellas.
- La separación entre piezas no cambia.

## 6. Fuentes del barrio

Crenshaw station y Jack Northrop Field (Wikipedia); cityofhawthorne.org (aeropuerto); hawthornepolice.com/northrop;
Satellite Today 24‑11‑2008; CLUI, «SpaceX headquarters» y «Northrop aircraft plant site»; CEQAnet 2005091123
(Century Business Center); GlobeSt 2010 (Kearny); LA Business Journal; nota de prensa de Metro del 29‑1‑2004 (Rapid 710);
Wikipedia, «Crenshaw Boulevard»; City-Data (Hollypark); activediner (Nat's Airport Cafe).
Pendiente de confirmar con Google Earth (imágenes de 2008): los comercios exactos de Crenshaw y la planta real de la fábrica.

## 7. Código

- `Assets/Editor/HawthorneDistrict.cs`: el distrito (calles, mobiliario urbano, `Shell` = edificio con fachada, y cada edificio
  con su interior), la escala y la entrada delante de la fábrica.
- `MicrocosmSpec.ownGround` + `StorySceneBuilder.OwnGround`: el suelo propio (sin el piso del santuario ni `CivicBuilder.Civilize`).
- `CivicBuilder.HawthornePeople`: los 12 oficios de la nave y 14 trabajadores del barrio, con sus casas, más las parejas de vecinos.
- La fábrica es la de `StorySceneBuilder.Musk*`, montada en su plano de 44 × 30 m y agrandada después por `ScaleFactory`: todo ×2 en
  planta (88 × 60 m: muebles, máquinas y paso) y muros, puertas, ventanas y cubierta ×3 en alto (27 m). Lo que tiene medida real (la
  etapa del Falcon 1, la carretilla, los camiones) solo cambia de sitio. El distrito se abre a su alrededor con `Spread` (el este de
  x 20 +50 m, el oeste de x −30 −46 m, el norte de z 20 y el sur de z −20 ±16 m); el suelo pasa a 496 × 322 m. **Pendiente:** más
  detalle de la nave (sala limpia, taller de mecanizado, laboratorio) según §3.
- Las misiones de CodeShip siguen enganchadas: se han conservado los nombres exactos (`Plancha de los tacos`, `Caja registradora 1`,
  `Frutas y verduras`, `Cámara de lácteos y congelados`, `Banco del mecánico`, `Bomberos`, `Hidrante`,
  `Cruce de Crenshaw Boulevard y Rocket Road (semáforos)`, `Lonchera (camión de comida)`…).

## 8. Ronda de pruebas del 2026-11-19 (PR #309, 96ae284) y arreglos

| Fallo | Causa | Arreglo |
|---|---|---|
| MIC-87: paso de 2,30 m entre la cocina y la mesa de los bomberos | muebles ×2 más grandes de lo calculado | se separan, y cada edificio pasa por `Declutter` (corre el mueble que cabe hasta dejar 2,4 m, con las cajas reales) |
| MIC-93: el maquinista del proveedor no llega a su casa (camino parcial) | la oficina de la nave tapaba la puerta | la oficina se aparta y `ClearDoors` despeja 2,6 m alrededor de cada puerta (la de la calle y las de los tabiques) |
| MIC-93: un vecino no llega a su cocina | casas de 13 × 11 m, demasiado justas con los muebles ×2 | casas de 16 × 13 m cada 18 m, con la puerta en el centro, entrada de coche y frutal en todas |
| Apartamentos cerrados | la puerta estaba en x −12 con una fachada de ±10 | puerta en x −6 |
| g43 y g69: «0 plantas» y «sin piso gigante» | todo el distrito contaba como claro de la historia; las reglas pedían piso gigante | el distrito se llama «Calles de…» (lo reconoce como pueblo), la losa de asfalto va con el suelo del piso, y `MicroFloor.ownGround` exime de las reglas del piso gigante |
| Fachadas: montantes de 9 m sueltos, ventanas laterales flotando, rótulos encimados y en espejo | la vista de corte ocultaba el muro pero no lo que llevaba; las letras no cabían en su panel | todo lo de la fachada se llama «Muro_…» (se desvanece con su muro), montantes ×3 con travesaño, sin ventanas en los muros medianeros del strip mall, letras ajustadas al panel, el número de las casas junto a la puerta |
| Ronda 3 (96c384f) · MIC-87/g69: pasos de 2,22 / 0,16 m | el validador medía el banquete de arriba contra el café de abajo, y el horno de los donuts contra la plancha de la taquería de al lado (tras un muro) | `MicroBuildRules.Validate` solo mide el paso entre muebles que se solapan en altura y del mismo edificio |
| Ronda 3 · mesas de la taquería y de los donuts pegadas | cada mesa con sus apoyos de abdomen ocupa ~2,8 × 4,3 m | taquería: 6 mesas (3 × 2) cada 5,25 × 6,8 m; donuts: 4 mesas cada 5 × 6,8 m, con la puerta despejada |
| Ronda 3 · MIC-93: 15 teletransportes de jornada y 4 de casa | con los muebles ×2 el punto de uso queda fuera del NavMesh o el agente se queda a 1 m rozando el mueble | `WalkWatch.Target` posa el destino en el NavMesh hasta 3 m; `WalkWatch.AtPiece`: junto al mueble (≤ 0,8 m + estatura) ya cuenta como llegado, en `MicroJornada` y en `Household` (asiento, cama, aseo y despensa) |
| Ronda 4 (afd2563) · MIC-87/g69: gasolinera, licorería, supermercado, taller… «no adaptado a insectos-humanos» | solo tenían máquinas y estanterías | `EnsureInsectFit`: el edificio sin mueble a su medida recibe una mesa alta del personal (o un taburete) donde quepa, lejos de las puertas |
| Ronda 4 · MIC-93: 4 de hogar (apoyo de abdomen fuera del NavMesh; aseo de los apartamentos) | se medía la llegada contra el apoyo (sin colisor) y la puerta del tabique quedaba junto a la cocina y el escritorio | `AtPiece` mide contra la mesa entera (o su caja visible) y las puertas de los apartamentos van en el centro del tabique |
| Ronda 4 · g62: «Dependienta de Sears» 2,3 m | el peinado se ponía después de ajustar la estatura | `DressFolk` reajusta la estatura con el pelo puesto |
| Ronda 5 (b49cbc6) · MIC-93: 3 del turno de noche no llegan a su aseo (camino parcial desde y = 1,0) | al levantarse de comer, el punto de pie salía a la altura del apoyo de abdomen: encima, en una isla sin NavMesh | `Household` los levanta al suelo del mueble y `HomeMove.StandUp` los posa en el NavMesh (≤ 3 m) |
| Ronda 5 · g62: «Dependienta de Sears» sigue a 2,3 m | el tope de 2,2 m no dejaba margen para la pose al andar | `CivicBuilder.Worker`: tope de 2,1 m × s |
| Ronda 6 (3706368) · MIC-93: el turno de noche sigue sin llegar al aseo desde y = 1,0 | acaban el turno subidos a la **tarima de puesto** (E7, ~1 m, isla de NavMesh sin rampa) | `WalkWatch.StepDown`: si desde ahí no hay camino completo y desde el suelo de al lado sí, baja; se llama al empezar cada ida a casa y antes de rendirse |
| Ronda 6 · MIC-93: el mecánico «atascado» andando a 1,2 m/s por un camino completo de 100 m | la recta no bajaba en el rodeo y `remainingDistance` sale infinito en caminos largos | andar por un camino completo cuenta como avanzar; el diagnóstico da el largo del camino si es infinito |
