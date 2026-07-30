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

## 5. Estado y pendientes
- **Diseño** hecho (este doc); reorden aplicado en `area-progression.md` (Construcción = área 3).
- **Por construir**: el sistema de **dispatch/tickets** (`RepairTicket`/`ServiceHub`, compartido con la
  Mecánica), las **recetas de estructura** (obra) y el arranque `ConstructionBeginner` (limpiar→abastecer→
  construir). Reusa el motor de virtualización (`ProductionOrder`/`StationPart`/`StockingTask`/`TypingChallenge`).
- **Históricos** por autorar: Maestro de catedrales, Brunelleschi (hilo B).
