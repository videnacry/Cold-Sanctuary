# Aptitudes de las criaturas — agilidad, percepción, fuerza, masa

Modelo compartido de aptitudes para **animales y humanoides** (NPCs, y a futuro el jugador).
Introducido el 2026-07-10.

## Premisa

Cada aptitud es un **valor relativo a la media real** de su tipo: `1.0` = media,
`>1` por encima, `<1` por debajo. No son valores absolutos, son múltiplos de "lo normal".

Las aptitudes **no son fijas**: representan el estado actual de la criatura y **deben ir
cambiando conforme gana (o pierde) musculatura, agilidad, inteligencia/percepción**, según:

- **Origen** — de dónde viene la criatura / su historia previa fija el punto de partida.
- **Tareas y acciones del día a día** — lo que hace repetidamente la moldea con el tiempo
  (trabajo físico → fuerza y masa; estudio → percepción; sedentarismo → fuerza/masa a la baja).

Los **animales** tienen un abanico de tareas estrecho (comer, huir, criar…) → su calibración
es sobre todo por especie/origen. Los **humanoides** desempeñan un abanico mucho más amplio de
tareas y orígenes → admiten **progresiones mucho más especiales e individuales**.

> **Estado actual:** los valores base están calibrados a mano ("media real"). El bucle que los
> hace **evolucionar por tareas/tiempo** aún no está implementado — es el siguiente paso de diseño.

## Dónde vive en el código

- **Animales** — `LivingEntity.agility` / `LivingEntity.perception` (campos, 1.0 = media).
  `Animal` expone `virtual BaseAgility` / `BasePerception` por especie; se fijan en `Init()`.
  `sensibility` (umbral de detección de amenazas en `Escape()`) deriva de `perception`:
  `sensibility = BaseSensibility * perception` → más percepción, reacciona antes.
- **Humanoides (companions)** — `CompanionBase.agility/perception/strength/bodyMass`
  (+ `virtual Base*`), fijados en `Start()`. Cuando llegue `NPCBase : LivingEntity` estas
  aptitudes se consolidarán en `LivingEntity` y se eliminará la duplicación.
- **Jugador** — `PlayerStats` ya tiene equivalentes parciales: `observationRadius` (≈ percepción)
  y `velocity` (≈ agilidad); crecen con la práctica (asanas). Pendiente unificar nomenclatura.

## Calibración por especie (animales)

| Especie | agility | perception | Razón |
|---|---|---|---|
| Conejo | 1.5 | 1.6 | presa hipervigilante y veloz |
| Ciervo | 1.4 | 1.5 | presa alerta y rápida |
| Zorro | 1.4 | 1.5 | ágil, sentidos afinados |
| Lobo | 1.2 | 1.4 | cazador de manada, olfato agudo |
| Husky | 1.2 | 1.1 | perro de trineo, resistente |
| Foca | 1.1 | 1.2 | ágil en agua, torpe en tierra |
| Oso | 0.7 | 1.1 | poderoso pero lento; buen olfato |
| Ballena | 0.6 | 1.0 | enorme, poco maniobrable |

## Perfiles de humanoides (companions)

Los valores codifican **origen + historia**, no solo el presente.

### Goluis (25 años) — fuerza
- `strength 1.5`, `bodyMass 1.3`, `agility 0.9`, `perception 0.6`.
- Trabajo de fuerza del mismo tipo toda su vida → **fuerza aumentada**; invierte mucho en comida
  → **masa aumentada**. Dejó de estudiar a los 17 (ahora 25) → **poca costumbre de estudio/detalle**
  → percepción baja. Corpulencia → agilidad ligeramente baja.

### Panterilia (42 años) — percepción
- `perception 1.7`, `strength 0.7`, `bodyMass 0.8`, `agility 0.95`.
- Nunca dejó de estudiar y ha ocupado múltiples puestos/roles → **atención al detalle muy alta**
  y cierta **lógica de negocio**. Alejada del trabajo físico y de la vida corporal → su **fuerza
  incluso ha decrecido** y, como la alimentación es parte de la vida corporal, su **masa está por
  debajo de la media**.
- **Rasgo mental (documentado, aún sin mecánica):** su gran facilidad/capacidad para **liberar
  energía mental** hace que pueda **crear exageraciones de la realidad** en su mente, y que se vea
  muy **influenciada por ideas fuera de su experiencia propia** (teorías, experiencia e imaginación
  de terceros). Candidato a modelarse como un modificador de percepción/creencias o un `ThoughtAnchor`.

### Gohageneis — celebración (provisional)
- `agility 1.1`, `perception 1.1`, `strength 1.0`, `bodyMass 1.05`. Origen aún sin definir;
  valores por revisar cuando se concrete su historia.

## Próximos pasos

1. Bucle de **evolución por tarea/tiempo**: cada tarea repetida ajusta la aptitud correspondiente
   (p.ej. turno de cocina físico → +fuerza/masa; estudio/observación → +percepción; inactividad
   corporal → −fuerza/masa).
2. Conectar aptitudes a mecánicas: `agility` → velocidad/maniobra; `perception` → radio de
   detección/observación y calidad de asana; `strength` → daño/carga; `bodyMass` → física/saciedad.
3. Unificar con `PlayerStats` y consolidar en `LivingEntity` al implementar `NPCBase`.
4. Modelar el rasgo mental de Panterilia (exageración / influencia de terceros).
