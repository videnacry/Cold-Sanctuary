# Modo misión, disparadores y economía de recursos del santuario

Diseño (2026-07-10). Recoge la visión del jugador; el `MissionTracker` actual es un
**bosquejo** — esto es hacia dónde debe evolucionar. Marca lo que se vaya implementando.

## Disparador de misión (trigger)

- El disparador ya **no está fuera del área**, sino **dentro** de ella.
- La idea es que el disparador **sea un personaje** (un NPC), no un volumen invisible
  (aún por concretar del todo).
- Al encontrarse con él, se **pregunta al jugador** si quiere hacer la misión
  (usar `ConfirmationPanel`, ya existente).
- Si **acepta** → entra en **modo misión**.

## Modo misión

- Mientras dura, el jugador **no puede salir del área sin completar o cancelar** la misión.
- **Cancelar** sale del modo misión (y del bloqueo del área).
- El diseño interno del modo aún es un bosquejo. Sirve para crear **interacciones de simulacro**
  (p.ej. criar crías, o tareas de cocina) y también combate tipo "wow".

### Dos arquetipos de misión

1. **Simulacro** — interacción diegética con reglas propias. Ejemplos:
   - **Crianza de crías** (cuidado, presencia, alimentación).
   - **Cocina**: preparar pizzas, estofados, postres… con **ingredientes, tiempos y temperaturas**.
2. **"Wow"** — combate contra *mobs* (ver abajo).

## Cocina: mobs como economía circular (propuesta)

Para dar coherencia, los **mobs de la cocina son alimentos podridos y suciedad/manchas
acumuladas**. El jugador los combate para **transformarlos en abono (compost)**, y en el proceso
**a veces obtiene gramos de material reutilizable en otras áreas**.

> **Propuesta de realismo (pedida por el usuario): una economía cero-residuos.** Encaja con el
> tono contemplativo/ecológico del santuario y hace que las áreas se necesiten entre sí. Mapeo
> realista residuo → subproducto → área que lo usa:

| Residuo (mob) | Subproducto | Área que lo reutiliza | Realismo |
|---|---|---|---|
| Restos orgánicos en general | **Abono/compost** | Jardín / Huerto submarino | ✔ base del sistema |
| Grasa/aceite de cocina usado | **Biodiésel** | Taller de vehículos | ✔ (grasa→biodiésel es real) |
| Grasa/sebo | **Jabón** | Estudio textil / Limpieza | ✔ (saponificación real) |
| Huesos y cáscaras de huevo | **Suplemento mineral/calcio** | Jardín (enmienda) / Farmacia | ✔ |
| Fruta/verdura fermentable podrida | **Vinagre / etanol** | Laboratorio de alquimia / Limpieza | ✔ (fermentación) |
| Posos de café/té, restos verdes | **Nitrógeno (compost premium)** | Huerto | ✔ |
| Ceniza/carbón de residuos quemados | **Filtro / enmienda de suelo** | Agua / Jardín | ✔ |

Así "aceite para los coches" pasa a ser **biodiésel a partir de grasa de cocina** (realista), y
la **sal** puede venir de salmueras/conservas estropeadas (realismo más flojo — opcional).

## Contadores de recursos

- El **abono generado** se suma a un **contador**; forma parte de la **economía del santuario**.
- Igual con **cocinar**: toda la comida preparada va a un **contador de comida**.
- Habría **contadores por área** (compost, comida, biodiésel, jabón, minerales, …).
- **Bucle de mantenimiento:** cuando un contador **baja de su límite**, el jugador y otros
  personajes que **no estén en una misión** en ese momento pueden ser **obligados** a realizar las
  misiones que generan ese recurso. El santuario "pide" trabajo según lo que escasea.

## Notas de implementación (estado actual → objetivo)

- `MissionTracker` ya soporta `IngredientCollection` y `YeastControl`; `AreaClear` está incompleto.
  El comentario referencia un `KitchenCombatManager` **inexistente** que debería:
  1. arrancar el modo misión de cocina (activar mobs, bloquear salida),
  2. al procesar cada mob, llamar `MissionTracker.ReportMobProcessed`,
  3. sumar abono/subproductos a los contadores,
  4. cerrar la misión (`ReportAreaCleared`) al limpiar el área.
- Falta el **sistema de contadores** (por área) y el **conscription loop** (obligar a personajes
  ociosos cuando un contador baja del umbral).
- El disparador-personaje necesita: componente en un NPC, prompt vía `ConfirmationPanel`,
  y bloqueo/desbloqueo de la salida del `SanctuaryArea`.

Ver también [`gameplay-loops.md`](gameplay-loops.md) y [`world-simulation.md`](world-simulation.md).
