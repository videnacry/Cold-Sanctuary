# Refugio, ocultarse y comportamiento adulto (diseño)

Fecha: 2026-07-10. **Estado: casi todo sin implementar** — captura de diseño para no perderlo.
Ver también [`behavior-system.md`](behavior-system.md) y [`creature-stats.md`](creature-stats.md).

## Estado actual (verificado en código)

- **No existe** lógica de esconderse / madrigueras / refugio / árboles / arbustos (grep vacío).
- Los animales navegan por NavMesh. `MediumZone` fija `currentMedium`. **Comportamiento agua/tierra
  implementado** (`Animal.CorrectMedium`, 2026-07-10): los acuáticos (afinidad agua > tierra) buscan el
  `FishSchool` más cercano si quedan en tierra; los terrestres salen del agua hacia el nido; gated por
  `busy`, así que un cazador sí sigue a su presa al agua. **Pendiente**: evitación *proactiva* (que un
  terrestre desvíe su `Wander` lejos del agua antes de meterse).
- Eventos de adulto actuales: `Wander`, `Rest`, `Feed` (entrega comida al nido), `Homebound`.
- Los **padres** cuidan crías vía `PostNatal` (limpiar, estimular, guiar, amamantar; el ciervo tiene
  `MarkHidingSpot`). **Falta**: qué hacen los adultos **sin crías** a su cargo.
- **Bug corregido (2026-07-10):** `Homebound` mandaba al punto exacto `HomeOrigin` → los adultos se
  amontonaban. Ahora van a un punto **dentro del radio del nido** (offset aleatorio).

## Comportamiento adulto sin crías (a diseñar)

- Deambular, explorar, jugar entre sí, buscar/mantener nido o madriguera.
- **Territorialidad/hábitat:** **parcialmente hecho (2026-07-10)** — `Animal.SenseThreats` hace que las
  presas **huyan proactivamente** de carnívoros cercanos (revive `EvaluateThreat`+`ThreatThreshold`), así
  que ya no se quedan tranquilas junto a lobos; un cánido solo también huye del oso. **Falta** la
  separación *espacial* (territorios/nidos separados al generar la escena; ver "Montaje de escena").

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

## Montaje de escena: nidos antes que familias (diseño)

Hoy `SampleSceneBuilder` puebla familias sin tener en cuenta nidos ni depredadores (cada animal fija
`HomeOrigin` = su posición de spawn en `Init`). Propuesta de orden:

1. **Definir/colocar los nidos/territorios primero** (uno por familia/especie), separando territorios
   de presa de los de depredador.
2. **Validar que cada nido de presa quede fuera del alcance de depredadores** — comprobar distancia
   mínima a las zonas/territorios de depredadores; si no cumple, reubicar.
3. **Poblar cada familia alrededor de su nido** (`RenderFamily` centrada en el nido; `HomeOrigin` de los
   miembros = el nido), en vez de en puntos sueltos contiguos.
4. Colocar depredadores en sus propias zonas, lejos de los nidos de presa.

Esto resuelve de raíz lo observado (conejos correteando junto a lobos) y da sentido a `HomeOrigin`/
`HomeRadius` como zona de nido. Enlaza con **territorialidad** (arriba).

## Personajes como objetivo: modelo y poblaciones (prerequisito)

Para que los animales salvajes puedan cazar/temer a personajes y mascotas, estos deben ser `ITarget`
con una población consultable:
- **Malamute (mascota):** ya es `ITarget` (es un `Animal`) con `MalamuteBehavior.population` → ya puede
  ser presa. Como mascota se coloca como compañero (no vía `nestSpecies`).
- **Jugador:** ya es `ITarget` vía `PlayerTarget` (+ `PlayerTarget.population`).
- **Companions / otros NPCs:** **aún no** son `ITarget` (viven en `CompanionBase`, sin población objetivo).

**¿Dónde viven hoy los stats de personaje? Fragmentado:** `PlayerStats` (jugador: `IBody`+`IMind`),
`CompanionBase` (companions: fatiga/estrés/humor + aptitudes) y `WorldCharacter` (NPC de área:
fuerza/satisfacción/observación; puentea a `PlayerStats`). El hogar natural de un **personaje unificado**
(stats + `ITarget` + población + aptitudes) es el pendiente **`NPCBase : LivingEntity`**.

**Recomendación:** crear las poblaciones de personajes/mascotas **al implementar `NPCBase`** (no antes),
para no duplicar y tener que migrar. Interino posible: un componente ligero `CharacterTarget : ITarget`
(como `PlayerTarget`) que cualquier humanoide/mascota lleve y registre en una población compartida.

## Enganches con lo ya construido

- `currentMedium` + afinidad de medio (`MediumZone`) → base para evitar/entrar al agua por especie.
- `perception` y `EffectiveAgility` (evolución de aptitudes) → alimentan detección/sigilo.
- `HomeOrigin`/`HomeRadius` → base para la zona de nido; `PostNatal` (`MarkHidingSpot`) ya toca el
  escondite de crías del ciervo.

## Peces (decisión)

Se mantiene `FishSchool` como **marcador de zona abstracto** (no entidades) por rendimiento: crear
peces individuales serían muchísimos y con alto trasiego de aparición/desaparición. Las marinas
"pescan" en el `FishSchool` más cercano.

**Evolución propuesta — el banco como organismo (una sola entidad):** en vez de un marcador estático,
`FishSchool` podría ser **una entidad viva**: se mueve, huye de depredadores y tiene un tamaño/`lp`
= número de peces (crece con el tiempo, mengua al ser comido, desaparece a 0 y reaparece por intervalos).
Sigue siendo **barato** (1 entidad por banco, no miles de peces) y lo vuelve dinámico y **presa real**:
- El **oso polar** pescaría en la orilla (banco en su `Diet`), igual que foca y zorro.
- El **zorro** come pescado, pero encaja mejor darle una mecánica de **robo/hurto**: acercarse sigiloso a
  un `FoodItem`/presa muerta ajena (o al banco) y llevárselo con `ICarrier`, en vez de cazar de frente.
  Liga con el futuro sistema de ocultarse (sigilo).

Implementación: `FishSchool` pasaría a `Animal`-like / `LivingEntity` ligero (movimiento de banda, huida,
población/`lp`, reproducción). Trabajo mediano → checklist.

**Dietas revisadas (2026-07-10)** — árbol trófico coherente:
- **Oso** (apex/oportunista): foca, conejo, ciervo, lobo, zorro, malamute, y **humano** (el oso polar
  **sí depreda humanos** → `difficulty` baja; solo lo frena el vínculo).
- **Lobo**: ciervo, conejo, zorro, malamute, **oso** (solo en manada y con mucha hambre) y **humano**
  (los lobos **evitan** al humano → `difficulty` alta; solo muy hambriento). Depredación **mutua**
  oso↔lobo: la selección solo elige la presa; **el combate lo decide la masa + `PackFactor`**.

> **Personajes/mascotas como presa:** `PlayerTarget` entra en las dietas de oso y lobo, pero **hoy solo
> contiene al jugador** (es el único con el componente `PlayerTarget`). `CanHarm` protege a quien tenga
> vínculo con el animal. Los demás personajes/mascotas (`CompanionBase`, un malamute-mascota) **aún no son
> presa**: necesitan ser `ITarget` con su propia población objetivo (futuro, con `NPCBase`).

> **La "predabilidad" del humano no es estática:** conforme el jugador/personaje domina magia y sube
> maestría (niveles altos donde vence a un oso con facilidad), los animales deberían **percibir el peligro**
> y dejar de verlo como presa — mantener distancia/cautela o incluso huir. Modelo: `PlayerTarget` expondría
> un valor de **amenaza/poder** (crece con la maestría mágica) que la selección de caza (`SelectPrey`) y la
> respuesta a amenaza (`EvaluateThreat`, hoy hook muerto) leerían: poder bajo → presa fácil; poder alto →
> no-presa / cautela / huida. Es la versión inversa del refinamiento de caza. Requiere el sistema de magia/maestría.

> **Influencia de manada en la caza (decisión):** NO como multiplicador estático en la dieta, sino como
> **evaluación dinámica al seleccionar/comprometer la caza**: contar la masa aliada cercana de la presa
> y del cazador (reutilizando `PackFactor`). Así un oso evita atacar a un lobo con manada, y si la manada
> es grande, se aleja. Es el mismo **refinamiento de caza** (ver checklist), unificado con el poder del humano.

> **Estatus del humano según su historia mágica (documentado):** las actitudes de los personajes cambian
> cómo los ven los animales. Un contador de **usos destructivos de magia** sube su "aura de peligro"
> (los animales huyen / mantienen cautela) y **decae con el tiempo** (o con actos no destructivos), de modo
> que un personaje puede dejar de ser aterrador e incluso pasar a **inspirador** → **bonds fáciles** (p. ej.
> los osos se vinculan rápido). Alimenta el mismo valor de amenaza/poder que leen `SelectPrey`/`EvaluateThreat`:
> aura alta → no-presa + huida; aura baja + maestría respetada → vínculo. Requiere el sistema de magia.
- **Zorro** (pequeño): conejo, pájaro — sin cambios, ya coherente.
- **Malamute** (perro de carga, mal cazador): conejo (difícil). **Mascota**: no forma familias salvajes
  (fuera de `nestSpecies`); se coloca como compañero, pero sigue siendo presa potencial.
- Herbívoros (conejo, ciervo, foca, ballena) no tienen `Diet`: pastan en `GrassPatch`/`FishSchool`.
