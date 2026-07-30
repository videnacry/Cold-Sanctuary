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
- **Vehículos/tracción** (construir/reparar): **orgánico vs mecánico** —
  - **Huerto:** **bueyes/vacas** *(DECIDIDO 2026-07-30 — rama orgánica: bonds/no-violencia).* El tractor
    queda como rama mecánica **opcional/futura** (estilo alternativo).
  - **Aéreo:** **globos aerostáticos** (santuario aéreo vertical). **Marino:** **submarinos** (santuario
    marino/subterráneo). Coherentes con los 5 santuarios (`world-topology`). *Más adelante.*

### 1b. Arranque de la Mecánica (tareas simples para principiantes)
Igual que la cocina (limpiar → abastecer → cocinar), la Mecánica se **empieza sin experiencia** con las
tareas más simples y se **asciende** a lo complejo (vehículos, drones, teleportadores):
1. **Limpiar el taller** — barrer virutas/aceite (reusa el sistema de suciedad `DirtArea`/`Cleaner`).
2. **Abastecer / ordenar** — **cajas dejadas en la puerta** → colocar cada pieza/herramienta en su
   **estante correcto** (`StockingTask`) → el jugador **aprende dónde va cada cosa** (igual que las
   despensas de la cocina). Coger de la caja, colocar en el estante que acepta ese ítem.
3. **Reparación simple** — receta corta (diagnosticar → destornillar → sustituir pieza → probar), con el
   **diagnóstico tecleado** (mecanografía, kitchen §4b).
Luego: reparar/mejorar las **máquinas** de las áreas, y las tareas avanzadas (vehículos, drones, etc.).
Sandbox: `MechanicsBeginner_AUTO`.
- **Drones y avatares**: se **fabrican/reparan** aquí los **minidrones/miniavatares** (`RobotAvatar`/
  `AvatarController`) que interactúan con el **Microcosmos** (p. ej. limpiar manchas, extraer manjares).
- **Mecánica + magia (futuro):** **teleportadores** ("aeropuerto", `world-topology` §C) y artículos que
  **mezclan mecánica y magia**; su **investigación/mejora** vive aquí.
- **Recetas de la Mecánica** = ensamblar/reparar (piezas + energía + tiempo, con el mismo motor de
  virtualización; algunos pasos temporizados con typing = "apretar tornillos"/diagnosticar).
- **Elemento/tema:** Fe (hierro) y compuestos metálicos; *la técnica al servicio del cuidado, no del poder*.

## 1c. ¿Construcción o Mecánica? (qué arregla cada una)
Frontera clara (2026-07-30) para no confundir tickets:
- **CONSTRUCCIÓN = el edificio y sus instalaciones FIJAS** (lo que no te llevarías): paredes, puertas,
  ventanas, tejado, cimientos; **fontanería** (tuberías, grifos, desagües); **instalación eléctrica fija**
  (cableado, enchufes, interruptores, cuadro/**diferencial**, luminarias y **bombillas**).
- **MECÁNICA = aparatos con MECANISMO y vehículos** (partes móviles / desmontables): nevera (compresor),
  horno, **licuadora**, telar, **motor de la bomba de agua**, extractor; vehículos (arado de bueyes, carro,
  globo, submarino); **drones y avatares**.
- **Regla:** *¿parte del inmueble (fijo)? → Construcción. ¿aparato con mecanismo? → Mecánica.* El **cableado
  y las luces** son Construcción; el **motor dentro** de un electrodoméstico es Mecánica.
- Reparaciones más fáciles de cada una: **Construcción** → grifo que gotea (hecho) / **bombilla o
  diferencial** (electricidad). **Mecánica** → **cuchilla floja de la licuadora** / **engrasar un cojinete
  que chirría** / filtro atascado.

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
  - Tracción del Huerto: **bueyes/vacas DECIDIDO** (orgánico); tractor = rama opcional futura.
  - ¿**Globos/submarinos** ya, o más adelante? *(encajan con los santuarios aéreo/marino.)*
  - ¿**Teleportadores** y objetos mecánica+magia — cuándo entran?
  - Lista de **máquinas por área** (carne cultivada, textiles, enfermería, cocina) a reparar/mejorar.
- **Por construir**: recetas de **reparación de máquinas** de la Mecánica (Meso) — distintas de la forja;
  la decisión **arado vs espada**; misiones-historia (Primer Herrero/Ötzi/Sargón); cuota↔misión.

## 5. Reparación por DISPATCH (tickets) — cruza áreas
La Mecánica **no solo tiene virtualizaciones en su sala**: **despacha** al jugador a **otras áreas** a
reparar. Bucle propuesto (idea del usuario):
1. **Llega un ticket** al tablero de la Mecánica: *"la nevera de la Cocina no funciona"*, *"el horno"*,
   *"el telar del Textil"*, etc. (avería = máquina marcada como estropeada en su área).
2. El jugador **toma las herramientas** (en un banco de la Mecánica — gate: sin herramientas no repara).
3. **Va al área** y **repara** la máquina (una **receta de virtualización** en esa máquina — el mismo
   motor `ProductionOrder`/`StationPart`/typing).
4. **Vuelve a la Mecánica** y **deja las herramientas** → ticket cerrado.
- Efecto: el jugador **recorre el santuario** y aprende dónde está todo; la Mecánica queda como **hub de
  mantenimiento** de todas las áreas (nevera/horno de Cocina, telar de Textil, equipos de Enfermería,
  máquina de carne cultivada…).
- **Construcción comparte este dispatch** para **estructuras**: tickets de **tuberías, electricidad,
  puertas, paredes, ventanas** por todas las áreas (ver [`construction-simulation.md`](construction-simulation.md)).
- **Modelo (HECHO, MVP):** `RepairTicket` (avería en un área + su receta; abierta→cerrada), `ServiceHub`
  (tablero + banco de herramientas tomar/devolver), `Toolbox` (gate: sin herramientas no se repara —
  `ProductionOrder.requiresTools`). Sandbox `DispatchDemo_AUTO`.
- **1ª reparación de ESTRUCTURA (Construcción/fontanería) — el GRIFO QUE GOTEA** (la avería más típica/
  simple; sirve igual en Cocina —fregadero— y Huerto —manguera/riego—): **cerrar la llave → desmontar la
  maneta → cambiar la junta gastada → montar → abrir y probar**. Requiere herramientas (llave inglesa). HECHO.
- **1ª reparación eléctrica (CONSTRUCCIÓN) — la BOMBILLA FUNDIDA / el DIFERENCIAL saltado** (la instalación
  eléctrica es del edificio): **quitar bombilla vieja → poner nueva → encender**; variante con gracia: rearmar
  el **diferencial** (1 acción; enseña *diagnosticar antes de desmontar*; reactiva varias luces a la vez).
- **1ª reparación de MÁQUINA (MECÁNICA) — la LICUADORA con la cuchilla floja** (aparato con mecanismo, la más
  fácil): **desenchufar → reencajar/desatascar la cuchilla → probar**. O **engrasar un cojinete que chirría**.
  *(Por cablear — candidatas a la próxima.)* Luego: **filtro atascado** (extractor/bomba), **correa suelta**.
