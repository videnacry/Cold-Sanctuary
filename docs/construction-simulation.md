# La Construcción (Mesocosmos) y Levantar refugio (Microcosmos)

Diseño (2026-07-30). Área **3** (va **antes que la Mecánica**: primero se aprende a **levantar refugio**,
luego a **trabajar metales**). En el **Mesocosmos** es el **hub de estructuras** del santuario; en el
**Microcosmos** es la historia de **construir a través del tiempo** (choza → casa → templo → catedral).
Base: [`area-progression.md`](area-progression.md) §3, [`forge-simulation.md`](forge-simulation.md) §5
(dispatch), [`kitchen-simulation.md`](kitchen-simulation.md) §3b (motor), [`mob-epochs-matrix.md`](mob-epochs-matrix.md)
(hilo **B · Barro y Metal**).

## 1. Por qué antes que la Mecánica
Cronología del microcosmos: **fuego → agricultura → refugio → metales**. Se **construye la choza/casa antes
de fundir metal** (Neolítico antes que Edad de los Metales), y como onboarding es más básico: mantener y
levantar el sitio antes de maquinar. Comparte el **hilo B** con la Mecánica (ancla *El Tallador* →
*El Alfarero* → *Maestro de catedrales* → Brunelleschi), pero la construcción es su **etapa temprana**.

## 2. La Construcción (Mesocosmos) — hub de estructuras
Da servicio a **todas las áreas**: repara y levanta **estructuras** — **tuberías, electricidad, puertas,
paredes, ventanas**, tejados. Como la Mecánica, funciona por **dispatch/tickets** (`forge-simulation.md`
§5): llega el aviso (*"gotea una tubería en la Enfermería"*, *"puerta rota en el establo"*), **tomas las
herramientas**, **vas al área**, **reparas** (receta de virtualización en esa estructura) y **vuelves a
dejar las herramientas**. Diferencia con la Mecánica: **estructuras** (obra) vs **máquinas** (mecanismos).

## 3. Levantar refugio (Microcosmos) — capa histórica
La historia de construir: **choza de adobe/madera → casa → templo → catedral** (hilo B). Materiales del
**Alfarero** (cerámica/ladrillo). Encaja con la aldea del Huerto que necesita **techo**. Históricos a
autorar: *El Tallador*/*El Alfarero* (ya) → *Maestro de catedrales* → **Brunelleschi** (Renacimiento).
- **Giro:** el refugio **une** a la tribu, pero **el muro también separa** (propiedad, fronteras, murallas)
  — enlaza con el excedente del Huerto y con "detener conflictos".

## 4. Arranque (tareas simples, como todas las áreas)
1. **Limpiar el solar** — despejar escombros (reusa `DirtArea`/`Cleaner`).
2. **Abastecer materiales** — **cajas → almacén** (`StockingTask`: ladrillo, madera, teja) → aprender dónde
   está cada material.
3. **Construir/reparar simple** — receta corta: **cimentar → levantar muro → techar → probar** (con algún
   paso temporizado tecleado, kitchen §4b). Luego: los **tickets de reparación** por áreas (§2).

## 5. Historia — base (El Tallador y el primer refugio)
**El Tallador** (hilo **B**, ancla: el primer útil de piedra) es la **base narrativa** del área.
**Autorado** en `PhrasePools` (Tierra). Kushal: *acarrea las piedras/materiales* (rol secundario).

**Historia.** El que da forma a la piedra descubre que las mismas manos que tallan una herramienta pueden
**apilar piedra sobre piedra** y **levantar un refugio**. Da **techo** a los suyos contra el viento y la
lluvia; la choza se vuelve casa, y la aldea, un lugar fijo. **Giro:** el muro que **abriga** también
**divide** — tras la cerca nace lo "**mío**", la propiedad, la frontera (enlaza con el excedente del Huerto
y con "detener conflictos"). *Aprendizaje: construir es cuidar; que el muro proteja sin encerrar.*

**Misiones (cadena de fases)** — arquetipo *canalizar/construir*:
1. **Tallar** *(recolección/moldear)* → de un canto, una herramienta. *(descubrimiento)*
2. **Levantar el primer muro** *(construir: cimentar→muro)* → refugio contra el clima. *(alivio/hogar)*
3. **Techar la casa** *(construir: techar→probar)* → la aldea se vuelve fija. *(arraigo)*
4. **El muro que divide** *(**detener conflictos** — garden §5)* → mediar cuando la cerca enfrenta a dos
   familias por el "lo mío". *(giro→reconciliación)*
- **Recompensa:** +Satisfacción/Fuerza · abre la Construcción. Continúa en eras posteriores: *Maestro de
  catedrales* (Medieval) → **Brunelleschi** (Renacimiento) — por autorar.

## 6. Estado y pendientes
- **Hecho**: diseño (este doc); reorden (Construcción = área 3); **El Tallador autorado**; **arranque
  `ConstructionBeginner_AUTO`** (limpiar→abastecer→cimentar/muro/techar/probar); **dispatch/tickets**
  (`RepairTicket`/`ServiceHub`/`Toolbox`, `DispatchDemo_AUTO`) compartido con la Mecánica.
- **Por construir/autorar**: recetas de estructura reales por área (tubería/puerta/ventana), misiones-
  historia del Tallador cableadas, e históricos posteriores (Maestro de catedrales, Brunelleschi).

## 7. Obras que avanzan a la vista (2026-11-04)

Berón: «el progreso de las obras podría ser visual? da la impresión de que mágicamente se termina cuando ha llegado el día».
Ahora una obra (`World/ConstructionProject.cs`) se va levantando día a día con las piezas de su «Acabado»:

- **Plan de obra.** Al empezar, la obra copia su «Acabado» en una **maqueta** (`Obra_en_progreso`): solo malla y material, **sin
  colisiones ni conductas**. Cada hijo del acabado es una pieza; si hay pocas para los días de obra, se abren los grupos grandes
  (la jaula → lona, postes, red). Cada pieza cae en una **etapa** por su nombre (o, si el nombre no lo dice, por su forma: lo
  plano y ancho es suelo):
  1. **Cimientos y suelo** (suelo, base, lona, tatami, tarima, parquet…),
  2. **Estructura** (postes, pilares, vigas, muros, marcos, jaula, ring, red, cuerdas…),
  3. **Instalaciones** (luces, lámparas, tubos, grifos, duchas, lavabos, altavoces, ventilación…),
  4. **Muebles y máquinas** (lo demás),
  5. **Acabados y decoración** (carteles, plantas, macetas, cuadros, espejos, cortinas, estatuas…).
  Se ordenan por etapa, luego de abajo arriba, y se reparten por igual entre los `daysRequired` días (todos los días tienen
  alguna si hay al menos tantas piezas como días).
- **El turno.** Al empezar «Trabajar la obra» (`WorkdayDirector.Begin` → `BeginShift`), mientras dura la misión las piezas de
  ese día van saliendo una tras otra (crecen desde una lámina y bajan a su sitio), quien trabaja lleva un **martillo** que
  golpea y se levanta **polvo** donde se construye. Los **palés** del material del día menguan según se gasta (sin material,
  no se avanza). El **andamio** se va junto a la parte que se construye (sin pisar las alfombras) y el **cartel** dice
  «EN OBRAS · etapa · %».
- **El día de obra** cuenta cuando ha trabajado el equipo entero (como antes): las piezas del día quedan puestas y las del día
  siguiente aún no se ven.
- **Nada usable antes de terminar.** Lo que se ve es la maqueta; el «Acabado» de verdad (con sus colisiones, puestos y
  conductas) sigue apagado hasta el último día, y entonces sustituye a la maqueta. Se mantiene la regla de no poner nada en los
  pisos sin terminar.
- **Guardar por etapa.** Los días hechos y si hay material en la obra se guardan entre partidas (`PlayerPrefs`, clave
  `cs_obra_<título>`; nunca mientras corren las pruebas). Al cargar, la obra enseña exactamente las piezas de los días hechos
  (`Restore`). `ConstructionProject.ClearSaved()` las olvida todas.
- **Prueba:** `ConstructionProgressTest` (grupo 98) simula 5 días de obra con un equipo de 2 en una obra de prueba: plan por
  etapas, piezas a media jornada, martillo, material que mengua, cartel, andamio, más piezas cada día y ninguna antes de tiempo,
  nada usable, el último día = el acabado entero, y cargar en el día 3 = lo mismo que la simulada; y revisa las obras de la escena.
- **Pendiente:** cuando llegue la capa de interpretación de `claude/edif-actividades`, el martillo sencillo puede pasar a ser
  una animación del cuerpo.
