# La Mecánica (Mesocosmos) y la Forja (Microcosmos)

Diseño (2026-07-29; **reencuadre 2026-07-30**). Tercera área. **Corrección**: una **forja de bronce** no
encaja bien en el santuario a pie. La 3ª área del **Mesocosmos** es la **MECÁNICA** —reparar/construir/
investigar **máquinas y vehículos**—; la **forja/fundición** literal pertenece al **Microcosmos** (era de
los metales, capa histórica). Base: [`area-progression.md`](area-progression.md),
[`kitchen-simulation.md`](kitchen-simulation.md) §3b (motor de virtualización),
[`world-topology-and-planes.md`](world-topology-and-planes.md), [`mob-epochs-matrix.md`](mob-epochs-matrix.md).

## 1. La MECÁNICA (Mesocosmos) — el taller del santuario
El **hub técnico** que da servicio a **todas** las áreas: se **repara, mejora e investiga** la **maquinaria**
de cada una y se **construyen/reparan vehículos, drones y avatares**. Encaja en el santuario porque **todas
las áreas tienen máquinas** que se estropean y mejoran.
- **Máquinas de otras áreas** (reparar/mejorar en la Mecánica): **carne cultivada**, **textiles**,
  **enfermería**, **cocina**… Cada avería = una **misión de reparación** (receta de virtualización:
  diagnosticar → sustituir pieza → calibrar → probar).
- **Vehículos** (construir/reparar): decisión de diseño **orgánico vs mecánico** —
  - **Huerto:** **tractor** *(rama mecánica)* **o bueyes/vacas** *(rama orgánica, bonds/no-violencia)*.
  - **Aéreo:** **globos aerostáticos** (santuario aéreo vertical). **Marino:** **submarinos** (santuario
    marino/subterráneo). Coherentes con los 5 santuarios (`world-topology`). *A confirmar qué se incluye.*
- **Drones y avatares**: se **fabrican/reparan** aquí los **minidrones/miniavatares** (`RobotAvatar`/
  `AvatarController`) que interactúan con el **Microcosmos** (p. ej. limpiar manchas, extraer manjares).
- **Mecánica + magia (futuro):** **teleportadores** ("aeropuerto", `world-topology` §C) y artículos que
  **mezclan mecánica y magia**; su **investigación/mejora** vive aquí.
- **Recetas de la Mecánica** = ensamblar/reparar (piezas + energía + tiempo, con el mismo motor de
  virtualización; algunos pasos temporizados con typing = "apretar tornillos"/diagnosticar).
- **Elemento/tema:** Fe (hierro) y compuestos metálicos; *la técnica al servicio del cuidado, no del poder*.

## 2. La FORJA (Microcosmos) — capa histórica (Edad de los Metales)
La **fundición de bronce** (cobre+estaño; luego hierro) es la **historia** de los metales → vive en el
**Microcosmos** (Mesopotamia; el mob-world también tiene **sus propias estructuras y virtualizaciones**).
Continúa el hilo del **fuego** del **Señor del Fuego** (la **pirita** → **hierro**). Aquí:
- **Ötzi histórico** (hacha de cobre → misión "**buscar la raíz**"), **El Primer Herrero** (protagonista) y
  **Sargón de Acad** (primer imperio → el dominio que somete). **Autorados** en `PhrasePools`.
- **Giro:** la misma forja hace **arado y espada** → escala el conflicto (Sargón; "detener conflictos").
  *El que forja, elige.*
- **Receta de forja** (virtualización del Microcosmos): `TomarMineral → Fundir(typing) → Verter → Forjar →
  Templar` → herramienta de bronce. Sandbox `ForgeVirtualization_AUTO` (crisol acelerable tecleando
  bronze/copper/tin/cu/sn/melt) — demuestra el motor; es la forja **del Microcosmos**, no del santuario.

## 3. Cómo se conectan Meso ↔ Micro
- La **Forja** (Micro) produce el **saber/los metales**; la **Mecánica** (Meso) los usa para **máquinas y
  vehículos** del santuario. El progreso histórico del Microcosmos **desbloquea** capacidades en la Mecánica.
- Los **drones/avatares** fabricados en la Mecánica son los que **entran al Microcosmos** → bucle cerrado.
- Idea confirmada: **el Microcosmos también tiene virtualizaciones** (no solo el Mesocosmos) — cada plano
  tiene sus estaciones/recetas.

## 4. Estado y decisiones abiertas
- **Autorado**: El Primer Herrero, Sargón (+ Ötzi). Receta de forja cableada (`ForgeVirtualization_AUTO`).
- **Por decidir contigo** (diseño de vehículos/máquinas):
  - ¿**Tractores** (mecánica) **vs bueyes/vacas** (orgánico) para el Huerto? *(propongo: ambos, como ramas
    de estilo — orgánico casa con el tema animal/no-violencia; mecánico con el tema técnico.)*
  - ¿**Globos/submarinos** ya, o más adelante? *(encajan con los santuarios aéreo/marino.)*
  - ¿**Teleportadores** y objetos mecánica+magia — cuándo entran?
  - Lista de **máquinas por área** (carne cultivada, textiles, enfermería, cocina) a reparar/mejorar.
- **Por construir**: recetas de **reparación de máquinas** de la Mecánica (Meso) — distintas de la forja;
  la decisión **arado vs espada**; misiones-historia (Primer Herrero/Ötzi/Sargón); cuota↔misión.
