# Refugio, ocultarse y comportamiento adulto (diseño)

Fecha: 2026-07-10. **Estado: casi todo sin implementar** — captura de diseño para no perderlo.
Ver también [`behavior-system.md`](behavior-system.md) y [`creature-stats.md`](creature-stats.md).

## Estado actual (verificado en código)

- **No existe** lógica de esconderse / madrigueras / refugio / árboles / arbustos (grep vacío).
- Los animales navegan **solo por NavMesh**; no evitan ni entran al agua de forma explícita. Las
  especies marinas van con guarda `nav.isOnNavMesh`. El nuevo `MediumZone` ya fija `currentMedium`,
  pero el **comportamiento** de evitar/entrar según el medio aún no está.
- Eventos de adulto actuales: `Wander`, `Rest`, `Feed` (entrega comida al nido), `Homebound`.
- Los **padres** cuidan crías vía `PostNatal` (limpiar, estimular, guiar, amamantar; el ciervo tiene
  `MarkHidingSpot`). **Falta**: qué hacen los adultos **sin crías** a su cargo.
- **Bug corregido (2026-07-10):** `Homebound` mandaba al punto exacto `HomeOrigin` → los adultos se
  amontonaban. Ahora van a un punto **dentro del radio del nido** (offset aleatorio).

## Comportamiento adulto sin crías (a diseñar)

- Deambular, explorar, jugar entre sí, buscar/mantener nido o madriguera.
- **Territorialidad/hábitat:** hoy todas las especies pueblan áreas abiertas contiguas y se quedan,
  así que se ven conejos correteando junto a lobos. Falta separación por territorio/hábitat y por
  relación depredador-presa (las presas evitan zonas de depredadores).

## Ocultarse / refugio (a diseñar; necesita contenido: árboles y arbustos)

- **Motivos:** ocultarse **para cazar** (emboscada), **para sobrevivir** (presa), **ambos**, o
  **por época** (la osa en guarida/letargo invernal).
- **Tipos por especie (propuesta):** zorro → madriguera y/o trepar árboles; conejo → madriguera;
  osa → guarida estacional; presas en general → matorral/arbusto.
- **Cobertura:** estar tras un árbol/arbusto reduce la probabilidad de ser detectado; un cazador
  oculto decide cuándo atacar (distancia + seguir sin ser detectado).
- **Descubrimiento:** valorar si un oculto es descubierto = `perception` del observador vs cobertura
  + quietud del oculto. Según complejidad, empezar documentando y luego implementar.

## Memoria de lugares (nidos/madrigueras) — SOLO DOCUMENTADO por ahora

- Idea: los animales **recuerdan ubicaciones** (nido, comida) y van a ellas al tener hambre.
- **Riesgos:** coste de rendimiento (estructuras de memoria + búsquedas); y requiere que la presa
  pueda **descubrir que su nido fue detectado** y **mudarse**.
- **Decisión:** dejarlo documentado hasta evaluar impacto; empezar por **refugio estático** (zonas
  fijas) antes que memoria dinámica de lugares.

## Enganches con lo ya construido

- `currentMedium` + afinidad de medio (`MediumZone`) → base para evitar/entrar al agua por especie.
- `perception` y `EffectiveAgility` (evolución de aptitudes) → alimentan detección/sigilo.
- `HomeOrigin`/`HomeRadius` → base para la zona de nido; `PostNatal` (`MarkHidingSpot`) ya toca el
  escondite de crías del ciervo.

## Peces (decisión)

Se mantiene `FishSchool` como **marcador de zona abstracto** (no entidades) por rendimiento: crear
peces individuales serían muchísimos y con alto trasiego de aparición/desaparición. Las marinas
"pescan" en el `FishSchool` más cercano.

**Dietas revisadas (2026-07-10)** — árbol trófico coherente:
- **Oso** (apex/oportunista): foca, conejo, ciervo, lobo, zorro, husky.
- **Lobo**: ciervo, conejo, zorro, husky, y **oso** (solo en manada y con mucha hambre). Depredación
  **mutua** oso↔lobo: la selección solo elige la presa; **el combate lo decide la masa + `PackFactor`**
  (un lobo solo pierde; una manada cercana suma masa aliada en `Fight` y puede con el oso).
- **Zorro** (pequeño): conejo, pájaro — sin cambios, ya coherente.
- **Husky** (mal cazador por diseño): conejo (difícil) — sin cambios.
- Herbívoros (conejo, ciervo, foca, ballena) no tienen `Diet`: pastan en `GrassPatch`/`FishSchool`.
