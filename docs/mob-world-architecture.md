# Arquitectura del Mundo Mob (santuario fractal)

> Diseño (2026-07-20). Cómo son y funcionan los mundos mob del plano mágico: santuarios
> fractales por escala de consciencia, con NPCs ligeros y un mundo vivo por eventos.
>
> Conecta con: [`magic-plane-and-meditation.md`](magic-plane-and-meditation.md) (ejes, escalas,
> meditación), [`world-simulation.md`](world-simulation.md) (áreas/`WorldCharacter`/director humano),
> [`area-missions-spec.md`](area-missions-spec.md) (misiones simulacro vs mob).

---

## 1. La idea: santuario fractal

Igual que el mundo base es un **santuario de humanos y animales**, cada mundo mob es **otro
santuario** — de insectos, de bacterias, etc. Al entrar como insecto, la **cocina deja de ser una
cocina y pasa a ser una ciudad de insectos**: estructuras, mercados, personajes mob con roles.

El mismo motor (áreas + personajes + misiones) se **reskinea** por escala. No son sistemas nuevos:
son datos temáticos sobre lo que ya existe. La recursión ("cada mundo mob tiene su propia sala de
yoga, cocina, personajes…") es **reskin, no re-simulación**.

## 2. Principio clave: un mundo por escala, de a uno

Cada **escala de consciencia es un mundo autocontenido y lleno de vida**. El jugador **habita uno
a la vez**: elige la escala al meditar, entra a ESE mundo entero, y desciende a la siguiente solo
cuando la desbloquea/elige.

- **Los otros niveles se ocultan** mientras estás en uno. (Decisión firme, no solo "de momento".)
- Motivo: inmersión (no diluir el tema mezclando escalas), coste (solo se carga/simula el mundo
  activo) y claridad narrativa.

Escalera (recap de `magic-plane` §5), habitada de a una:
**insecto → bacteria → molecular/partícula → energético → invisible/mental.**

## 3. Dos mundos por área (recap)

Toda área tiene **dos capas** (ver `area-missions-spec`):
- **Simulacro** — el mundo base (humanos/animales), tarea diegética real.
- **Mob / wow** — el mundo mob de la escala activa, estilo MMO.

## 4. Anatomía de un mundo mob

- **La sala de yoga = el portal.** En el mundo mob es el punto de **entrada/salida** de vuelta al
  mundo normal. Coherente: la meditación te metió, la meditación te saca.
- **Las demás áreas = edificios estilo WoW.** Te acercas, entras, y hay un **mob con ese rol**
  (cocina, jardinería, textil…) que ofrece su servicio/misión. Sin simulación pesada.
- **La ciudad.** El área mob (p. ej. la cocina→ciudad-insecto) es un hub navegable con esos
  edificios-tienda y NPCs mob.

## 5. NPCs mob: modelo ligero (≠ humanos)

Dos niveles de NPC en el juego:

| | Humanos (mundo base) | Mobs (mundo mob) |
|---|---|---|
| Clase | `WorldCharacter` (existe) | `MobResident` (nuevo, ligero) |
| Comportamiento | Simulación autónoma (task loop, promoción, FOMO) | **Estático en su puesto**; ofrece su servicio |
| Movimiento | Continuo, emergente | Solo cuando un **evento** lo dispara |
| Subida de stats | Por tareas en tiempo real | Por **eventos** (todos se esfuerzan y suben) |
| Coste | Alto (pocos personajes) | Bajo (muchos NPCs) |

Esto mantiene la sensación de progresión sin la complejidad de los humanos. Más adelante se puede
dar más realismo al mundo mob si hace falta.

## 6. Mundo vivo por eventos

El mundo mob **también está vivo**, pero movido por un **`MobWorldDirector`** (análogo al
`SanctuaryDirector` humano, pero por **eventos guionizados**, no por simular a cada mob):

- Dispara **migraciones, invasiones, festivales** ligados a la historia.
- Los eventos **mueven poblaciones** de un área a otra, **suben niveles** y **reordenan** qué
  misiones y enemigos hay.
- Los territorios se **mezclan** y transforman → "**vuelves al punto de inicio y todo es nuevo**".
- Principio: **nada se queda estático**; hay eventos que cambian el mundo entero.

## 7. Radio expansible (progresión de acceso)

El mundo mob **crece con el poder del jugador**:
1. Al principio, confinado al **interior de la cocina** (la ciudad-insecto).
2. Con más poder, se abre un **radio alrededor del restaurante** → distritos nuevos, misiones y
   enemigos más difíciles (nuevas áreas mob desbloqueadas).
3. Encaja con los *entry requirements* que `SanctuaryArea` ya tiene (gating por stats).

Combina con el **Eje A** (planos: suelo/pared/techo/aire, ya implementado con `SurfaceWalker`):
más poder = más planos y más radio = más mundo.

## 8. Balance tonal (requisito de contenido)

Las misiones deben **sumergir**: las hay alegres/absurdas, otras hasta dan miedo, tristes, de
descubrimiento, de aventura. **Regla de cobertura por área** (mejor que "misma cantidad exacta"):

- Cinco tonos: **terror · humor · tristeza · descubrimiento · aventura**.
- **Cada área garantiza al menos una misión de cada tono** (y busca equilibrio aproximado).
- Implementación: etiqueta `tone` por misión + auditoría en `area-missions-spec`. Barato y
  verificable. (Candidato a campo en la misión más adelante.)

## 9. Dirección de arte

Sociedades mob como **facciones humanoides estilizadas**: los insectos "de pronto" son casi
humanoides, con estructuras y mercados con mucho estilo — referencia tonal: **no-muertos de
Warcraft / zerg de StarCraft**. Igual para el mundo bacteria y los demás.

## 10. Diferidos (por ahora demasiado abstractos)

- **Mobs planta / inanimados** y **poseer objetos** (traspasar/habitar cosas): se posponen; la idea
  se conserva para cuando el jugador tenga poderes muy avanzados.

## 11. Factibilidad — condiciones

Factible **si**:
1. Los mundos mob **reusan** áreas/personajes/misiones como **datos reskineados**.
2. NPCs mob = **modelo ligero estático** (`MobResident`), no simulación completa.
3. **Un mundo por escala, de a uno** (ocultar los demás).
4. Mundo vivo por **eventos de director**, no por simular cada mob.

**Riesgo principal:** volumen de contenido (santuario temático × 5 escalas × balance tonal). Por
eso se construye **un mundo mob completo primero** y luego se reskinea.

## 12. Objetivo de primera construcción — mínimo jugable (insecto/cocina)

- **Yoga-portal** para entrar/salir del mundo mob. → ✅ `YogaPortal` (salida; entrada ya vía máquina/loto).
- La **ciudad-insecto de la cocina** (hub navegable básico). → 🔲 requiere escena/`SampleSceneBuilder`.
- **2–3 áreas-tienda** con `MobResident` (rol + una misión). → ✅ código (`MobResident`); 🔲 colocación en escena.
- **1 evento** de `MobWorldDirector` (p. ej. una migración que sube niveles y cambia una misión). → ✅ `MobWorldDirector`.

Probar este patrón antes de reskinear a otras áreas/escalas.

> **Estado (2026-07-20):** el código-puro está hecho (`MobResident`, `MobWorldDirector`, `YogaPortal`,
> auto-bootstrap). Falta el **montaje en escena** de la ciudad-insecto y las tiendas, que espera a
> validar el `SampleSceneBuilder`.

## 13. Impacto en el código

### Nuevo
- `MobResident` ✅ — NPC ligero (`IInteractable`): puesto fijo, rol/servicio, `LevelUp`/`MoveTo`/
  `ReturnHome` movidos sólo por eventos. Contraparte barata de `WorldCharacter`.
- `MobWorldDirector` ✅ — singleton (auto-bootstrap) que dispara eventos (`Migration`/`Invasion`/
  `Festival`): mueve habitantes, sube niveles y notifica (`OnEvent`) para reordenar contenido.
  Opción `autoEvents` para prototipado. Contraparte por eventos de `SanctuaryDirector`.
- `YogaPortal` ✅ — `IInteractable` de salida del mundo mob (reusa `MeditationSession.EndMission`).
- Etiqueta `tone` por misión (para el balance tonal) — 🔲 campo o metadato pendiente.

### Reuso / relación
- `SanctuaryArea` + entry requirements → gating del **radio expansible**.
- `MeditationSession` / `VirtualizationMachine` / loto → el **yoga-portal** de entrada/salida.
- `SurfaceWalker` + avatares → planos (Eje A) dentro del mundo mob.
- `MeditationMissionBase` + arquetipos → las misiones mob del mundo.

### A decidir
- Cómo se representa "un mundo por escala" a nivel de escena (escenas separadas por escala vs. roots
  activables). Recomendación inicial: **root activable por escala** (cargar/activar solo el mundo
  activo), coherente con el snap del `RealityShiftController`.

## 14. Contenido narrativo: reutilizar la Historia

**Principio (decisión 2026-07-20):** las historias de los mobs **no se inventan** — se **reutiliza la
Historia** real y la ficción de dominio público: países, guerras, conquistas, descubrimientos
científicos, obras artísticas, avances médicos, investigaciones de dudosa moral, historias de amor.

- **Cada mob = un personaje** (real o ficticio) renderizado como habitante de la escala activa
  (insecto humanoide, etc.).
- **Mecánica (estilo WoW):** el jugador se encuentra al personaje, **habla y lo ayuda** — y ayudar
  ES una misión mob (canalizar/curar/proteger/buscar-la-raíz reencuadrada). Al ayudar, **su historia
  se revela por capas**, ligada al bond / nivel de ayuda. Reusa `DialogueSequence`; el quest-giver
  es un `MobResident` con identidad histórica.

**Por qué encaja:**
- Resuelve el **riesgo de volumen de contenido** (§11): biblioteca infinita ya escrita.
- Satisface el **balance tonal** (§8) casi solo: la Historia trae los cinco tonos de forma natural.
- **Sinergia con la tabla periódica / Observación / learning-unlocks.** Ejemplo canónico:
  **Marie Curie** en el lab → descubre radio y polonio (avance + progreso de tabla) y muere por la
  radiación (tragedia + "investigación de dudosa moral"). El mob es contenido educativo y mecánico.

**Mapeo dominio histórico → área (propuesta):**
| Área | Dominio histórico |
|---|---|
| AlchemyLab / FuelLab | ciencia, química, energía |
| Enfermería / Farmacia | medicina + investigaciones de dudosa moral |
| Estudio Textil | arte y obras |
| Huerto | agricultura, botánica |
| Cocina | gastronomía / oficios |
| MonsterSection / NightWatch | conflictos, lo desconocido, lo "monstruoso" de la historia |

**Filosofía tonal — agridulce que termina en aprendizaje (confirmado por Berón 2026-07-20):**
- ~**La mitad** de las historias alegres/divertidas revelan un **trasfondo que invierte el tono**:
  muchos placeres solo existen por un **sacrificio** (a veces no identificado); el caos puede nacer
  de algo hermoso; se rastrean los errores/influencias que llevaron a lo trágico.
- Pero el **resultado final siempre es aprendizaje**: aprovechar un suceso triste del pasado para
  **forjar algo bueno** hoy — **reivindicación/redención**. Tesis: mundo real donde conviene estar
  **alerta** y cultivar buenos sentimientos/ideas/lógica en uno y en quienes nos rodean; **esperanza
  de crecer como especie hasta ser plenamente racionales**.

**Barrido cronológico (Edad de Piedra → presente):** el mundo mob **avanza muy rápido** en el tiempo,
**acoplado al `MobWorldDirector` (§6)**: sus eventos que "lo cambian todo" **son el avance de las
eras**. Así se ve cómo figuras como Hércules, Buda, Jesús, Einstein o Da Vinci **siguen inspirando**
siglos después.

**Reencuadre esperanzador (contrafactual):** se pueden **retocar sucesos históricos o leyendas** para
que se **perciba lo que realmente pasó** pero el jugador **halle la solución que evita la catástrofe**
— mostrando la capacidad de resolver el mundo alrededor y la esperanza de mejorar.

**Arquetipo de misión "Llegar al corazón":** misiones donde la vida del jugador **depende de alcanzar
el corazón de mobs mucho más poderosos que él**, por su bien y el de otros mobs. No es combate: es
**comprender/alcanzar el núcleo** (pariente del absorbente/buscar-la-raíz, a escala de "jefe").

**Lo turbio, al final:** los casos más oscuros (p. ej. experimentación con personas sin derechos que
derivó en descubrimientos) se **reservan para el tramo final** — menos prioritario, igual importante,
siempre desde la reflexión y la reivindicación de las víctimas.

**Guardarraíles (confirmados por Berón 2026-07-20):**
- **Sensibilidad/tono:** el trasfondo duro es válido (ver filosofía agridulce), pero **siempre cierra
  en aprendizaje/reivindicación**, desde la comprensión — nunca glorificación ni trivialización.
- **Propiedad intelectual:** reales + dominio público = libres. Para figuras **muy importantes o
  sensibles**, **representación abstracta** (arquetipo/esencia) y retoque "hasta el margen legal";
  ficción moderna con copyright = evitar. La idea es representar la Historia **al menos de forma
  abstracta**.
- **Rigor:** basarse en hechos; el reencuadre esperanzador **reinterpreta explícitamente** (no se
  presenta como historia literal).

Lista inicial de personajes: [`mob-characters.md`](mob-characters.md).

**Implementación:** la biografía por capas usa `DialogueSequence` (existe); cada capa se desbloquea
al completar una misión de ayuda. La **etiqueta `tone`** (§8) sale natural del episodio histórico.

## 15. Preguntas abiertas

- [ ] ¿Escenas separadas por escala o roots activables? (reco: roots activables).
- [ ] ¿Cuántas áreas-tienda mínimas hacen que la ciudad-insecto se sienta viva?
- [ ] Catálogo de eventos del `MobWorldDirector` (tipos, disparadores, efectos).
- [ ] Mapear los 5 tonos a misiones concretas por área (auditoría en `area-missions-spec`).
- [ ] ¿El `MobResident` comparte aptitudes con `creature-stats` o usa un set reducido propio?
- [ ] Confirmar guardarraíles de §14 (sensibilidad de historia dura; alcance de IP: reales + dominio público).
- [ ] Primer set de personajes históricos por área (empezar por la cocina-insecto del mínimo jugable).
