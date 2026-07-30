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
