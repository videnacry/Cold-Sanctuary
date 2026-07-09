# Sistema de Simulación del Mundo — Cold Sanctuary

El mundo del santuario avanza aunque el jugador no esté activo. Los personajes se mueven,
aprenden, y saben cuándo están listos para dar el siguiente paso — sin que el jugador
tenga que gatillar nada.

## Componentes

### `SanctuaryAreaType` (enum)

17 zonas organizadas en 5 tiers de progresión:

> **Estado real (auditoría 2026-07-09):** el enum `SanctuaryAreaType` tiene 17 entradas, no 13.

| Tier | Área | Descripción |
|------|------|-------------|
| 1 | CubCare | Primera zona. Cuidado de crías. Stats mínimos. |
| 1 | Cleaning | Limpieza del santuario. Dominio de Panterilia. |
| 1 | Kitchen | Cocina. Dominio de Goluis. |
| 1 | Infirmary | Enfermería. |
| 1 | VeterinaryClinic | Clínica veterinaria. |
| 1 | YogaRoom | Sala de yoga. |
| 2 | Garden | Huerto. Trabajo físico, producción de alimento. Fuerza rápida. |
| 2 | VehicleWorkshop | Taller. Tractor, barco, submarino. |
| 2 | AlchemyLab | Lab de alquimia vegetal. Primera exposición a la tabla periódica. |
| 2 | TextileStudio | Estudio textil. |
| 3 | FuelLab | Lab de combustible. Biogás, biodiésel, electrólisis. |
| 3 | CulturedMeatLab | Lab de carne cultivada. Química avanzada + nutrición. |
| 3 | SupplementsPharmacy | Farmacia. Vitaminas, minerales. Panterilia nivel 2. |
| 4 | SubmarineAccess | Pilotaje. Misiones submarinas. Desbloquea zonas profundas. |
| 4 | UnderwaterGarden | Jardín submarino. Organismos raros, control de respiración. |
| 4 | NightWatch | Guardia nocturna. Primeros avistamientos de monstruos. |
| 5 | MonsterSection | Contención, lenguaje, curación de monstruos. |

### `SanctuaryArea` (MonoBehaviour)

Zona física en escena. Define:
- **Entry requirements**: `minStrength`, `minSatisfaction`, `minObservation`
- **Tasks pool**: array de `AreaTask` configurado en inspector
- **Spawn points**: transforms donde se posiciona a los residentes
- **Residents list** (runtime): qué `WorldCharacter`s están aquí ahora

### `AreaTask` (Serializable)

Una tarea que un personaje puede ejecutar en un área. Cada tarea tiene:
- `duration` — segundos por ciclo
- `mindChannel` + `mindEffect` — restaura o drena satisfacción/fatiga
- `strengthDelta`, `observationDelta`, `velocityDelta` — efecto físico por ciclo
- `elementSymbol` + `elementDiscoveryChance` — slot de descubrimiento de tabla periódica
- `minProgressionLevel`, `requiresVehicle` — requisitos

### `WorldCharacter` (MonoBehaviour)

Componente en cualquier entidad que participe en la simulación (jugador, NPC, compañero).

- **Bridging**: detecta `PlayerStats` en el mismo GameObject y lo usa. Si no hay, gestiona
  stats ligeras propias (strength, satisfaction, observation, velocity, mentalFatigue).
- **Stat accessors**: `Strength`, `Satisfaction`, `Observation` — always the right source.
- **`PlaceInArea(area, spawnIndex)`** — asignado a un área. Reinicia el task loop.
- **`AutonomousTaskLoop()`** — coroutine. Ciclo: elegir tarea → esperar duración →
  aplicar efectos → tirar dado de descubrimiento → comprobar promoción.
- **`IsReadyForPromotion()`** — true cuando `Strength >= promotionStrength`,
  `Satisfaction >= promotionSatisfaction`, `Observation >= promotionObservation`.
- **Events**: `OnReadyForPromotion`, `OnElementDiscovered`.

### `SanctuaryDirector` (MonoBehaviour — Singleton)

El rol de gestión de mundo de la Magnate.

**Flujo de nueva llegada:**
1. `WorldCharacter.Start()` llama `SanctuaryDirector.Instance?.Register(this)`
2. Si `isNewArrival = true`, Director espera `assessmentDelay` segundos (para cutscenes)
3. Evalúa stats del personaje → coloca en el área más alta que califique
4. Jugador con stats cero → siempre aterriza en Tier 1 (CubCare por defecto)

**Flujo de promoción:**
1. `WorldCharacter` dispara `OnReadyForPromotion`
2. Director espera `groupGatherWindow` segundos (para agrupar personajes listos en la misma ventana)
3. Agrupa por tier de destino
4. Si hay ≥ 2 personajes yendo al mismo tier → dispara `OnGroupPromotion` (cinemática de reunión)
5. Llama `PlaceInArea` en cada personaje → el loop autónomo se reinicia en el nuevo contexto

**API de consulta:**
- `GetArea(SanctuaryAreaType)` — área por tipo
- `GetCharactersInArea(type)` — lista de residentes actuales
- `Reassess(character)` — re-evalúa y recoloca (útil post-escena)

## Setup en Unity

1. GameObject `SanctuaryDirector` en escena con el componente homónimo.
2. Arrastrar todos los `SanctuaryArea` GameObjects al array `allAreas[]`.
3. Añadir `WorldCharacter` al jugador (`isPlayer = true`, `isNewArrival = true`).
4. Añadir `WorldCharacter` a cada NPC. Configurar `promotionStrength/Satisfaction/Observation`
   en función de su rol.
5. Cada `SanctuaryArea` necesita `availableTasks[]` configurado en inspector.

## Diseño de tareas por área (ejemplos)

### CubCare
| Tarea | Duración | Elemento | Efecto |
|-------|----------|----------|--------|
| Alimentar crías | 15s | Ca (0.3) | +satisfaction |
| Jugar con crías | 20s | — | +satisfaction +0.01 |
| Cambiar ropa de cama | 12s | — | +strength +0.005 |

### Kitchen
| Tarea | Duración | Elemento | Efecto |
|-------|----------|----------|--------|
| Cortar ajo | 10s | S (0.4) | +observation +0.1 |
| Amasar pan | 20s | C (0.3) | +strength +0.01 |
| Analizar agua marina | 30s | Na, Cl, Mg (0.5) | +observation |

### AlchemyLab
| Tarea | Duración | Elemento | Efecto |
|-------|----------|----------|--------|
| Destilación de plantas | 45s | C (0.5) | +observation +0.2 |
| Bioluminiscencia | 60s | P (0.3) | +satisfaction |
| Mezcla incorrecta → explosión | 15s | — | -satisfaction drain |

### FuelLab
| Tarea | Duración | Elemento | Efecto |
|-------|----------|----------|--------|
| Electrólisis | 60s | H, O (0.4) | +strength +observation |
| Producción biogás | 90s | C, H, O (0.3) | +observation +0.3 |
| Síntesis biodiésel | 120s | C, H, O, N (0.2) | +strength +0.02 |
