# El área de CRÍA — el corazón del santuario

Diseño (2026-07-30). No es un "trabajo" más: es **la razón por la que Kushal está aquí** (el prólogo de la
Enfermería lo prepara para ir a la cría). El **Nivel 1** es cuidar crías hasta ganar su **vínculo (bond)**.
Base: [`fauna-gameplay.md`](fauna-gameplay.md), [`behavior-system.md`](behavior-system.md),
[`refuge-and-adult-behavior.md`](refuge-and-adult-behavior.md), [`area-progression.md`](area-progression.md).

## 1. Lugar en la progresión (antes de la Mecánica)
La **domesticación** precede a los metales y a la medicina profesional (perro **Paleolítico**, ganado
**Neolítico** ~con el Huerto). → La cría va **temprano**, antes de Construcción/Mecánica. Pero, sobre todo,
es el **corazón**: el cuidado/bond está presente desde el prólogo (el **perro** es el primer compañero).
*(Posición exacta a confirmar; ver area-progression §Cría.)*

## 2. Principio: el bond SE GANA, no se da
Cada cría tiene **hambre, temperatura, estrés y bond** reales (fauna-gameplay §). Un `stress` alto rechaza
el contacto aunque haya bond. El jugador debe: **leer el estado → elegir la actividad adecuada → respetar
si la cría se retira**. No se puede forzar el bond.

## 3. Virtualización de la cría (mismo motor)
Arranque como las demás áreas, reutilizando el motor (`StationPart`/`ProductionOrder`/`StockingTask`/
`CarryToRefuge`/`WeakOne`). Sandbox `CriaBeginner_AUTO`:
1. **Limpiar el nido** (`DirtArea`/`Cleaner`).
2. **Abastecer** — cajas → almacén (**biberón / comida / paja-lecho**), `StockingTask`.
3. **Rutina de cuidado** (`ProductionOrder`): **leer estado → calmar (presencia tranquila) → alimentar
   (biberón) → asear (grooming) → arrullar (respuesta vocal)**. Repetir → sube el **bond**. *(Sin typing:
   la cría es calma, no prisa.)* Las actividades se **desbloquean por bond** (fauna-gameplay §): presencia y
   vocal desde bond 0; biberón ≥10; grooming/puzzle a más bond.
4. **Llevar la cría al nido cálido** (`CarryToRefuge`/`WeakOne`) — de noche o ante peligro (temperatura/estrés).

## 4. Enganches
- **Con la Cocina/Huerto:** la comida de las crías **sale de los contenedores** de la Cocina (que a su vez
  surte el Huerto) → bucle producción→cuidado.
- **Con el prólogo:** `CarryToRefuge`/`WeakOne` ya nacieron ahí ("llevar a los débiles a la cueva") → la cría
  es su continuación natural (cuidar al indefenso).
- **Con el objetivo del juego:** al final del Nivel 1, las crías deben alcanzar **bond ≥ umbral** → abre el
  Nivel 2 (fauna-gameplay §Progresión).

## 5. Estado y pendientes
- **Hecho (scaffold):** `CriaBeginner_AUTO` (limpiar→abastecer→rutina→nido) reusando el motor + la fauna
  existente (`Animal`/`LifeStage`/`PostNatal`/`Family`).
- **Falta:** enganchar la rutina a los **drives reales de la cría** (hambre/estrés/bond de `Animal`), el
  gateo por bond de las actividades, y las crías reales (no capsula placeholder).
