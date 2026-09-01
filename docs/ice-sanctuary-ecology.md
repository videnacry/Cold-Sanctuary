# Ecología del Santuario de Hielo (Santuario 1) — 2026-09-01

> **Diseño/lore + spec de especies.** El Santuario 1 es de **HIELO** (`world-topology-and-planes.md`,
> `sanctuary-second-lap-and-fear.md`). Hoy la escena de prueba (`SampleSceneBuilder`) tiene fauna **templada** (osos
> polares **sobre tierra con pasto**, lobos, conejos…) → **inconsistente**. Este doc fija la ecología de hielo y
> especifica las **ánimas nuevas** para implementarlas por PRs. Los 5 insectos (Ant/Aphid/…) NO son de aquí: son del
> **Microcosmos Nivel 1** (mundo insecto). Encaja con lo ya construido: `TraceField` (atracción por rastro), `Starve`→
> `SicknessState` (miel letal), descomposición/economía (bucle de nutrientes), `SharedSoul` (ánimas integrales baratas).

## 1. El problema: un mundo de hielo ≈ solo agua y frío

Un bioma de hielo real casi no tiene suelo fértil: **agua + frío extremo**. El oso polar habita hielo/mar, no praderas.
Base alimenticia propuesta: **plancton/kril** (marino) — el resto de la cadena cuelga de ahí. Como el santuario es
**artificial** (obra de la maestra), se justifica **flora especial** adaptada: mutaciones vegetales fusionadas con
microorganismos que viven **solo de agua y frío**.

## 2. El árbol carnívoro de hielo (la pieza central)

Un **árbol que sobrevive de agua+frío** (célula vegetal + microorganismo psicrófilo) y que, para conseguir **nutrientes**
en un medio sin suelo, es **carnívoro pasivo** y a la vez **promueve** a los organismos de los que se alimenta:

1. **Atrae desde lejos**: emite un **rastro** potente (canal nuevo `Nectar`/olor dulce en la `TraceField`) que llega a
   km → una criatura **hambrienta** (impulso de comida alto, cf. `Starve`) lo sigue.
2. **Miel letal soporífera**: al llegar, come una **miel** que induce **sueño profundo casi inmediato** (un
   *hechizo-estado* `Torpor`: `asleep` forzado + no despierta) → la criatura **muere cerca del árbol**.
3. **Bucle de nutrientes**: las **abejas de las nieves** (§3) anidan en el árbol y **fabrican los nidos con los
   nutrientes de los muertos** (descomposición/economía); el **árbol absorbe** de los nidos. Más muertes → más nidos →
   árbol más grande → **más ramas largas** para albergar el máximo de nidos → más miel → más muertes. Ciclo cerrado.

**Implementación (ánima):** `SimpleAnima` (inanimada-despertable, barata — NO `Animal`, sin NavMesh/IA) + componentes:
- `IceTree` (emite `Nectar` a la `TraceField`; radio enorme, fuerza alta).
- `LethalHoney` (trigger: al comer, aplica `Torpor` → dormir letal; deja un `FoodItem`/carcasa que alimenta el bucle).
- El "guardar su futuro alimenticio" = un almacén de nutrientes que crece con las carcasas cercanas (enlaza `Metabolism`/
  economía). Ramas = geometría escalable con el nivel de nutrientes (a futuro).

## 2.1. Principio: primero el BIOMA REAL, luego las mutaciones

Realidad del hielo (Antártida): **sí hay flora terrestre** (líquenes/musgo/algas) pero **los consumidores son
micro-invertebrados** (colémbolos, ácaros, tardígrados; el mayor animal puramente terrestre es un mosquito sin alas de
~6 mm). **No hay grandes herbívoros de tierra.** → El hielo es **mayormente MARINO**; la tierra es micro-vida. Orden:
**(A) construir el bioma real** (cadena marina + micro-tierra), **(B) añadir las mutaciones** del santuario artificial
(árbol carnívoro, insectos especiales) encima.

## 2.2. Cadena real (el objetivo del bioma)

**Marina (donde está la vida):**
`Fitoplancton` ("césped del mar": fotosintetiza, vive de agua+luz) → `Krill` → `Peces` → {`Foca`, `Pingüino`, `Ballena`
(barbada come krill directo)} → {`Oso polar`, `Orca` (caza ballenas/focas)}.

**Terrestre (micro, escasa):** `Líquen/musgo` → micro-invertebrados (colémbolo/ácaro) → ácaro depredador. Opcional/tardío.

### El banco como ORGANISMO (spec de `FishSchool`/`Krill`, idea del usuario)

En vez de miles de peces, un **GameObject-banco** con **peces hijos** y una sola ánima (`SimpleAnima`+comportamiento):
- **Lifecycle LINEAL** (sin child/teen/adult): solo **comer / huir / descansar**. Todos se mueven juntos.
- **Crece comiendo** (fitoplancton/krill) y su tamaño **multiplica los GameObjects-hijos** (más peces visibles); mengua al
  ser comido (menos hijos). Se autoregenera (ya lo hace `FishSchool.growthPerSecond`).
- **`IEdible` especial (colisión-mordisco):** el depredador **NO caza un pez concreto** — se acerca y **choca** con el
  banco; al contactar da un mordisco → **desaparece un hijo (un pez)** y el depredador obtiene los nutrientes de un pez.
- **Krill = igual** (banco que come fitoplancton, comido por peces/ballena). **Fitoplancton = "césped" acuático** (un
  `GrassPatch` marino / `SimpleAnima` productor que "vive de agua"), base de todo.

## 3. Especies del bioma (spec para implementar por PRs)

Patrón de cada especie-animal = `XBehavior : Animal` (`SpeciesArchetype` + `Start`+`ConfigureThreat`) **+ 6 entradas de
catálogo** (`Archetypes` body/mind, `SpeciesProfile`, `Physiognomy`, `StageProfile`, `Family`, `Forager`), igual que los
insectos (`5a892f5`). Los **inanimados** (árbol, plancton) son `SimpleAnima` + componente, sin catálogo de especie.

| Ánima | Tipo | Rol / dieta | Notas de stats |
|---|---|---|---|
| **Plancton/kril** | integral (`SharedSoul`) o `FishSchool`-like | **base**: deriva en el agua; comido por casi todo | 1 ánima gobierna el banco (barato); ya hay `FishSchool` reutilizable |
| **Árbol carnívoro de hielo** | `SimpleAnima` + `IceTree`/`LethalHoney` | productor **carnívoro pasivo**: atrae y adormece; alberga abejas | inmóvil; `Nectar` fuerte; enorme |
| **Abeja de las nieves** | `Animal` (vuela) | anida en el árbol; hace miel de nutrientes de muertos; presa de araña/pájaro/pingüino | percepción alta; frágil; **necesita volar/trepar** (ver §4) |
| **Araña de las nieves** | `Animal` | caza abejas cerca del árbol | como `Spider` pero adaptada al frío; `armament` (veneno) |
| **Pingüino** (especial) | `Animal` | come abejas: **deambula cerca del árbol‑con‑cadáver** y espera a que las abejas bajen al cadáver para cazarlas; presa de la **foca** | "conoce el mecanismo" = deseo de merodear el `Nectar`/carcasa; nada bien |
| **Foca / Ballena** | `Animal` (ya existen) | marinos; foca come pingüino/pez | ya en catálogos; encajan en hielo |
| **Oso polar** | `Animal` (ya existe como "PolarBear") | apex; come foca/pez | **coherente aquí** (a diferencia de sobre tierra) |

**Fuera del bioma (guardar para otro santuario):** conejo/ciervo/lobo/zorro (templados), corales/almejas (marino
cálido, 2ª vuelta). Los **insectos** (Ant/Aphid/Ladybug/Spider/Cricket) → **Microcosmos Nivel 1** (a las hormigas se
les pondrá un `Mind`/thoughts que **concuerde con las personas de esa época** — pensamiento paleolítico).

## 4. Ganchos que faltan (para que el bioma funcione)

- **Volar/trepar** para la abeja (y trepar para acceder al árbol): hoy los insectos usan NavMesh plano; falta el
  **modo del hechizo de locomoción** (volar/trepar) cableado — el diseño existe (una locomoción, varios modos), falta
  aplicarlo. La abeja **necesita volar**; el pingüino especial, **trepar** o merodear la base.
- **Canal `Nectar`** en `TraceChannel` + `IceTree` que lo deposita (reusa `TraceField`).
- **`Torpor`** (hechizo-estado: dormir letal) — como `EstrusState`/`SicknessState` pero fuerza `asleep` y daña sin comer.
- **Miel de nutrientes**: el nido/miel se "fabrica" con carcasas cercanas → enlaza `DecompositionJob`/economía.
- **Limpiar `SampleSceneBuilder`**: quitar la fauna templada de la escena del Santuario 1 y poblarla con la de arriba
  (o hacer una escena de hielo aparte). Hoy solo se **quitaron los insectos** (no eran de aquí); el resto, pendiente.

## 5. Escala (por qué cabe todo)

Con **LOD** (`Lod`, los lejanos piensan más lento) + ánimas **inanimadas baratas** (árbol/plancton = `SimpleAnima`) +
**integrales** (`SharedSoul` para un banco/bosque entero) + **población baja por especie** (2–4), el presupuesto da de
sobra: el coste alto es solo el **Animal completo** (NavMesh+IA+O(n²)); lo demás es casi gratis. Ver el análisis en
`docs/checklist.md`. Objetivo: **muchas ánimas** manteniendo pocos "animales completos" simultáneos activos cerca.
