# La Forja / Mecánica — tercera área (Edad de los Metales)

Diseño (2026-07-29). Tercera simulación, tras la **Cocina** (fuego) y el **Huerto** (agricultura).
Continúa la historia del microcosmos: del **Neolítico** a la **Edad de los Metales** (Cobre→Bronce→Hierro).
Base: [`area-progression.md`](area-progression.md), [`mob-epochs-matrix.md`](mob-epochs-matrix.md) (hilo B),
[`mob-quests-early.md`](mob-quests-early.md), [`founding-trio-stories.md`](founding-trio-stories.md),
[`kitchen-simulation.md`](kitchen-simulation.md) §3b (motor de virtualización).

## 1. Territorio y época
- **Época:** Edad de los Metales. **Región:** **Mesopotamia** — la aldea del Huerto se ha vuelto **ciudad**
  (reusa la escena `MobWorld_Mesopotamia` ya existente). Elementos: **Cu, Sn, Fe**.
- **Cómo continúa la historia:** el hilo del **fuego** del **Señor del Fuego** ("la piedra que escupe
  chispa", la **pirita**) evoluciona en **metalurgia** (pirita → **hierro**). Aquí vive el **Ötzi
  histórico** (el hombre del **hacha de cobre**, asesinado — su misión "buscar la raíz"). No es Nasatya
  (ese es el guardián ficticio de la era temprana; ver `founding-trio-stories.md`).

## 2. El giro (agridulce)
La **misma forja** hace el **arado** (labrar, dar de comer) y la **espada** (herir). El primer útil de
metal fue también la **primera arma** — enlaza con la muerte de Ötzi y **escala el conflicto**: el metal
arma las disputas del excedente que nacieron en el Huerto. **Aprendizaje:** *el que forja, elige.*

## 3. Personajes históricos
- **El Primer Herrero** (hilo **B · Barro y Metal**) — **protagonista**. Domina la forja (bronce=cobre+
  estaño, luego hierro); el dilema arado/espada. **Autorado** en `PhrasePools` (Fuego/Tierra).
- **Ötzi** (histórico, hacha de cobre) — su misión "**buscar la raíz**" (qué le pasó). Ya autorado.
- **Sargón de Acad** (hilo **E · Corona y Espada**) — **primer imperio**; el dominio que **somete**;
  antagonista de las misiones de **detener conflictos** a escala (garden §5). **Autorado** (villano-capaz).
- (Fondo: **Enheduanna** — hilo D, primera autora; **Gilgamesh** — capstone de la era, "llegar al corazón".)

## 4. Estaciones y receta (virtualización — mismo motor, kitchen §3b)
Producir **herramientas de bronce** (sustento/materiales del santuario). Estaciones funcionales:
- **Cantera/Mineralera** — coger **mineral** (cobre + estaño).
- **Crisol** — **fundir** el metal (acción **temporizada** con **mecanografía**: teclear
  `bronze/copper/tin/cu/sn/melt` acelera la fundición; kitchen §4b).
- **Molde** — **verter** el metal fundido.
- **Yunque** — **forjar** a martillazos la herramienta.
- **Temple/Agua** — **templar** en agua (endurece).

**Receta base (5 pasos):** `TomarMineral → Fundir(typing) → Verter → Forjar → Templar` → **1 herramienta**.
Cuota de misión = N herramientas (arados/hoces) para el santuario. *(El "elegir arado vs espada" es una
variante de receta futura: la misma cadena, distinto molde → una decisión moral con consecuencias.)*

## 5. Enganches
- **Con la Cocina/Huerto:** el fuego (Cocina) y las herramientas (arado→Huerto) cierran un bucle: la forja
  **mejora la producción** de las otras áreas.
- **Con el conflicto (mundo-insecto/santuario):** el metal escala las disputas → más misiones de **detener
  conflictos** (Sargón), y el núcleo **fuertes vs débiles**.
- **Con la química:** fundir/alear es química real (Cu+Sn=bronce) → las palabras del typing son elementos/
  compuestos (Cu, Sn, Fe) → une mecanografía + química + "la comida/objeto forma al personaje".

## 6. Estado de código
- **Históricos autorados**: El Primer Herrero, Sargón de Acad (+ Ötzi ya estaba).
- **Receta cableada**: `ForgeVirtualization_AUTO` (sandbox) con las 5 estaciones + el crisol temporizado
  (typing). Reusa `VirtualPointer`/`StationPart`/`ProductionOrder`/`TypingChallenge`.
- **Falta:** decisión arado/espada (variante de receta + consecuencia), misiones-historia del Primer
  Herrero/Ötzi/Sargón (cadenas de fases), y enganchar la cuota a la misión real del área.
