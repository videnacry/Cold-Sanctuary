# Microcosmos, Máquina de Virtualización, Avatares y Meditación

> **Estado:** diseño (2026-07-13). Recoge la visión del usuario para el Microcosmos (antes
> «plano mágico»), los
> mobs y la meditación. Casi nada está implementado todavía; lo que sí existe se señala en
> cada sección. Marca lo que se vaya cableando.
>
> Ver también: [`gameplay-loops.md`](gameplay-loops.md) (biomas de combate por área),
> [`observation-system.md`](observation-system.md) (modo contemplativo, aún sin implementar),
> [`mission-mode.md`](mission-mode.md) (trigger y modo misión), [`asana-system.md`](asana-system.md).

---

## 1. La ficción: de la máquina al loto

Para acceder al Microcosmos el jugador entra en una **máquina de virtualización** que
supuestamente lo induce a un sueño hiper-realista. La ficción que se le cuenta:

- Los objetos conseguidos en la simulación **se conservan fuera** de ella, porque la máquina
  realiza de verdad toda la transformación del mundo — pero **necesita un piloto**.
- Para que el pilotaje sea manejable, la navegación y las acciones se presentan como una
  **simulación divertida**: el jugador guía a la máquina a través de un avatar-robot.

Esta ficción es el **andamiaje de onboarding**, y cumple tres funciones de diseño:

1. Justifica en ficción que la miniaturización sea "transformación real" y no un efecto visual.
2. Justifica que **los ítems persistan** fuera de la simulación (racionaliza el inventario).
3. Da un porqué al avatar-robot: el jugador es "el piloto".

**El arco:** conforme el jugador crece, deja de necesitar la máquina. Aprende la postura de
**loto** en la sala de yoga, pero eso **no basta** — debe adquirir además el **hechizo de
misiones-mob**, que lo independiza de la máquina de virtualización. A partir de ahí, sentarse en loto y cerrar los
ojos activa el mismo sistema **sin la máquina y sin robot**: ya es él mismo. Es un hito
narrativo, no un toggle silencioso.

Mecánicamente, máquina y loto disparan **el mismo sistema**; el loto solo retira el andamiaje.

---

## 2. Dos ejes de progresión (no confundirlos)

Toda la progresión del Microcosmos se mueve en **dos ejes independientes** que se combinan.
Ambos crecen con **stats** (cuando se habla de "magia" o "hechizo" es orientativo en la línea
temporal, no un recurso aparte).

### Eje A — Avatar / plano de acceso (¿dónde puedo ir?)

Qué superficies y espacios puede recorrer el piloto. Lo determina el **avatar-robot** activo.

### Eje B — Escala / capa perceptiva (¿con qué interactúo?)

Cómo de pequeño se hace el mundo y, por tanto, qué mobs se revelan. Cuanto más profunda la
capa, más sutil la percepción requerida — y la stat de **Observación** es literalmente "ver lo
que otros no ven". **El tamaño decreciente ES la escalera espiritual**, no un adorno.

Los dos ejes se cruzan: el avatar dice *dónde*, la escala dice *qué*. Una araña (acceso a
paredes) en la capa molecular interactúa con mobs distintos que la misma araña en la capa
insecto.

---

## 3. La máquina de virtualización como trigger universal

- **El trigger de las misiones de mobs / Microcosmos es la máquina de virtualización**, que
  ahora debe existir **en todas las áreas** (no solo en la cocina).
- Esto **reemplazó** el antiguo trigger de puerta de la cocina (`KitchenEntrance` +
  `KitchenScaleController` atados a un volumen de entrada); ambas clases se **borraron** (2026-07-23).
- Convive con el trigger de misión general descrito en [`mission-mode.md`](mission-mode.md)
  (un NPC dentro del área): el **NPC** abre misiones de simulacro; la **máquina** abre la
  misión del Microcosmos / mobs. Un área puede tener ambos.
- Al final del arco, la máquina deja de ser necesaria en las áreas donde el jugador ya domina
  el loto + el hechizo de independencia.

### Flujo de entrada (máquina y loto comparten el mismo flujo)

**Con máquina:**
1. El jugador **se acerca a la máquina e interactúa** con ella.
2. Se le **pregunta si quiere entrar** (usar `ConfirmationPanel`, ya existente).
3. Si acepta → **animación del jugador entrando en la máquina y cerrando los ojos**.
4. La **pantalla se pone negra** (sin mostrar nada).
5. Sobre el negro se muestra el **menú de misiones-mob disponibles**; el jugador **escoge una y
   entra**.

**Con loto (tras obtener el hechizo):**
1. El jugador **realiza el hechizo de misiones-mob** (= el hechizo de independencia de la máquina).
2. Animación de **sentarse en loto** y cerrar los ojos.
3. Igual que con la máquina: **pantalla negra → menú de misiones → escoger y entrar**.

> **Decisión clave: la transición de tamaños NUNCA se muestra al jugador.** El cambio de
> escala ocurre **detrás de la pantalla negra**, mientras se muestra el menú de misiones. Esto
> simplifica mucho `RealityShiftController`: puede **snap** de escala/FOV mientras el fundido a
> negro está activo — no hace falta shader de distorsión ni lerp de escala visible. Al elegir
> misión, se hace fade-out del negro y el jugador ya está en el mundo transformado.

### Arquitectura propuesta (generalizar lo que ya existe)

| Componente | Rol | Estado |
|---|---|---|
| `RealityShiftController` | Generaliza la antigua `KitchenScaleController`: escala el **root del entorno** hacia arriba, ajusta FOV, activa/desactiva el set de mobs de la capa. Reutilizable por cualquier área. | 🔲 Nuevo (extraído de la antigua `KitchenScaleController`, ya borrada) |
| `VirtualizationMachine` | El objeto físico en cada área. Es el trigger. Inicia la cinemática de conexión y llama al `RealityShiftController`. | 🔲 Nuevo |
| `MeditationController` | Evolución del "modo contemplativo". Dispara el mismo shift **sin** la máquina (postura de loto). Primera implementación real del sistema de observación. | 🔲 Nuevo |

> **Nota de código:** la antigua `KitchenScaleController` escalaba el *root del entorno*, no al
> jugador (para no romper el CharacterController/físicas); `RealityShiftController` conserva esa regla.

---

## 4. Avatares-robot y locomoción

El jugador **pilota un avatar-robot** dentro de la simulación. Va desbloqueando avatares al
subir stats; cada avatar define un **plano de acceso**, un **estilo de locomoción** y (opcional)
un **tier de escala** mínimo.

### Escalera de avatares

| Orden | Avatar | Plano de acceso | Locomoción | Notas |
|---|---|---|---|---|
| 1 | **Gusano** | Suelo (y rampas suaves) | Reptar | **No trepa paredes** — a propósito: crea la necesidad de la araña. Solo transforma partículas del suelo. |
| 2 | **Araña** | Suelo + paredes + techo | Trepar (gravedad relativa a la superficie) | Techo = estar de cabeza. Limpia partículas de paredes/techo/plataformas. |
| 3 | **Mosco** | Aire | Vuelo | Captura partículas en el aire; interactúa con insectos/mobs en vuelo. |
| 4 | **(dentro de materiales)** | Interior de sólidos | Inmersión/atravesar | Ver §6. Nuevo plano tras el aire. |
| — | **Loto (sin robot)** | Libre — ya no pilota | Primera persona; es él mismo | Requiere loto (yoga) **+ hechizo de independencia**. Puede elegir cómo se ve: avatar humano u otro. |

### Locomoción: un solo componente

Un componente `SurfaceWalker` (nuevo) resuelve suelo/pared/techo con **gravedad relativa a la
normal de la superficie**:

- Raycast hacia la superficie de apoyo; `transform.up = hitNormal`; la gravedad se aplica a lo
  largo de `-transform.up`. Suelo, pared y techo son **el mismo código**.
- Cada avatar aporta una **máscara de superficies permitidas** (qué normales puede adherir):
  - Gusano → solo normal ≈ hacia arriba.
  - Araña → + laterales + hacia abajo.
  - Mosco → ignora superficies (vuelo).
- Verticalidad progresiva **sin rehacer niveles**: las partículas del techo siempre estuvieron
  ahí; el jugador simplemente no podía llegar hasta desbloquear la araña. Esto habilita
  **volver al primer nivel** y acceder a zonas antes inalcanzables.

### Cambio de avatar

- **Dentro de la simulación:** cambiar de avatar es parte del pilotaje (según lo desbloqueado).
- **Fuera de la simulación:** al adquirir el **hechizo adecuado**, el jugador puede cambiar de
  avatar **incluso en el mundo normal** (p.ej. moverse como mosco fuera de la simulación).
- **En loto:** ya no hay robot; el avatar pasa a ser una **elección estética** (humano u otro),
  desacoplada de la locomoción.

| Componente | Rol | Estado |
|---|---|---|
| `SurfaceWalker` | Locomoción con gravedad relativa a la normal (suelo/pared/techo mismo código) + modos `canClimb`/`flight`. Kinemático; va en el avatar-robot, no en el jugador. | ✅ (1er pase, tunear en editor) |
| `AvatarController` + `RobotAvatar` | Avatar activo, desbloqueos, cambio (tecla G) gated por dentro-de-sim / hechizo. Cada `RobotAvatar` define locomoción (Ground/Climb/Flight), velocidad y escala. | ✅ |

---

## 5. Escalera de capas de escala (Eje B)

Recomendado **de mayor a menor** (insecto primero). Razones: (1) familiaridad — el
mundo-hormiga ya existe en la cocina; (2) cada escalón hacia abajo justifica el gate de poder y
mapea con Observación; (3) la capa molecular/atómica es donde **ya vive la tabla periódica**
(el pokédex de 118 elementos), así que ese tier se escribe casi solo; (4) la capa "invisible"
= plano mental, donde los mobs son **pensamientos** (sala de yoga) = endgame perceptivo.

| Tier | Capa | Mobs típicos | Sistema que alimenta |
|---|---|---|---|
| 1 | Insecto (terrestre) | insectos | proceso/combate básico |
| 2 | Microbio / bacteria | microorganismos | curar / proteger |
| 3 | Molecular / atómico | elementos, iones, moléculas | **tabla periódica** |
| 4 | Energético / cuántico | campos, enlaces, energía | encantamientos |
| 5 | Invisible / mental | **pensamientos** | meditación (yoga) |

La escalera de escala y la escalera narrativa/espiritual son **la misma línea**.

---

## 6. Nuevo plano: "dentro de los materiales" (tras el aire)

Después del nivel aire, el jugador desbloquea trabajar con mobs **dentro de los materiales
sólidos**: acero, madera, los componentes dentro de una manzana, insectos dentro de la tierra,
etc.

- Es un **plano de acceso** (Eje A): el avatar/piloto se **sumerge o atraviesa** el sólido en
  lugar de recorrer su superficie.
- Revela una capa más profunda de la escala: "componentes dentro de una manzana" conecta
  directo con lo **molecular/atómico** (Eje B) y por tanto con la tabla periódica.
- Encaja con el tono cero-residuos del santuario (ver `mission-mode.md`): descomponer/entender
  materiales por dentro.

> **Pendiente:** definir la locomoción de inmersión (¿fase a través del sólido con densidad/
> resistencia?, ¿túneles?) y cómo se representa visualmente el interior de cada material.

---

## 7. Misiones mentales: categorías y catálogo

Las misiones que aparecen en el menú (§3) se agrupan por **categoría de práctica**. Cada
categoría es una familia de misiones, no una sola; cada misión concreta genera su propia mezcla
de mobs y sus reglas.

### Categoría: Visualización

El jugador visualiza algo y, mientras lo sostiene, aparecen mobs. Es una **categoría con muchas
misiones**, por ejemplo:

- **Visualizar una postura de yoga** que domina poco → se ve a sí mismo formar la postura;
  sube el **dominio de esa asana** (engancha con el sistema de Asanas). *(Ejemplo canónico.)*
- **Visualizar un elemento** (de la tabla periódica) o un **hechizo**.
- **Curación de un compañero** / **curarse a sí mismo** / **curar a un animal**.
- **Ver a todos sus compañeros felices**.

En estas misiones los mobs pueden **atacar al jugador**, **huir**, **atacar a otros mobs**, o ser
**mobs heridos que necesitan atención**. Las **posturas mismas pueden ser mobs que huyen** y que
el jugador **persigue para "formarlas"** (cazar/sostener la postura = dominarla).

### Otras categorías (también son misiones del menú)

Las técnicas del §9 son, cada una, su propia categoría de misión:

- **Observar el cuerpo** (escaneo corporal).
- **El espejo** (mob que copia al jugador).
- **No pensar en nada** (dejar pasar / observar sin reaccionar).
- **Buscar la raíz**, **categorizar pasado/futuro/ahora**, **bondad amorosa**, etc.

### Progresión con/sin simulador

En la sala de yoga (y demás áreas) los mobs primero se muestran **con la máquina/simulador** y
más adelante, con dominio suficiente + el hechizo, **sin el simulador** (directamente en loto).
El flujo de menú (§3) es idéntico en ambos casos.

---

## 8. Arquetipos de mob (transversales a todas las áreas)

Formalizar como subtipos de un `MobBase` común (extraído de `IngredientMob`, que ya tiene
NavMeshAgent, drop de elemento, reproducción y estado "procesado" en vez de "muerto").

**Regla de tono (decidida, ver memoria del oso):** ningún arquetipo se resuelve con violencia.
El "combate" del juego es siempre **canalizar / comprender / cuidar** — "procesar, no matar".

| Arquetipo | Comportamiento | Cómo se resuelve | Técnica mental que mapea |
|---|---|---|---|
| **Efímero / distracción** ✅ | Te persigue para pegarse | **Huir / no alimentarlo** → se desvanece si no logra engancharse. *(Mecánica del usuario, confirmada jugable. Implementado: `EphemeralThoughtMob`.)* | Observar y dejar pasar |
| **Absorbente / obsesivo** ✅ | Rápido, se te pega como queriendo absorberte | **Indagar su raíz**: sigues el hilo hasta el nodo-memoria (marcador) y al alcanzarlo "ves a través" → se disuelve. No puedes huir (es más rápido). *(Implementado: `AbsorbentThoughtMob` / `RootInquiryMission`.)* | Auto-indagación / vichara |
| **Postura (huye)** ✅ | Huye del jugador | **Perseguir y sostener** → forma la asana, sube su dominio. *(Implementado: `PostureFormMob`.)* | Visualización |
| **Canalizar / presencia** ✅ | Deriva; no hostil | **Acercarse y mantenerse presente** unos segundos → se resuelve (procesar/calmar/limpiar/neutralizar). Universal, sin violencia. *(Implementado: `ChannelMob`.)* | Concentración / atención |
| **A proteger** ✅ | Frágil, otros mobs lo asedian | Interponerse para repeler oleadas; si lo alcanzan demasiadas veces, se pierde (la misión puede **fallar**). *(Implementado: `ProtectMob` + `WardAttackerMob` / `ProtectionMission`.)* | Compasión |
| **A curar** ✅ | Herido/enfermo; cojea hacia ti buscando consuelo | Mantenerte cerca hasta sanarlo; engancha con Bond/Satisfacción. *(Implementado: `HealMob` / `HealingMission`.)* | Bondad amorosa (metta) |
| **Simbionte / aliado** | Domesticable | Domar (no matar); luego ayuda (limpia, alerta) | — (taming, ya insinuado en submarino) |
| **Mimético / oculto** | Solo visible en el tier de percepción correcto o en meditación | Recompensa por subir Observación | — |
| **Espejo** | Copia las acciones del jugador; "un pensamiento que eres tú" | Endgame mental | "¿Quién observa?" (self-inquiry) |
| **Reproductor** | Se multiplica si no lo controlas | Control de masas | — (ya existe: levadura) |
| **Territorial** | Zona de control | Limpiar territorio | — (ya existe: plagas del huerto) |

Los mobs también crecen en complejidad con el jugador: de insectos **terrestres** a insectos
**en vuelo**; y las misiones con bacterias/átomos se vuelven más complejas conforme el jugador
adquiere el poder para realizarlas.

---

## 9. Técnicas de meditación → mecánicas

El tier mental invierte el combate: **se gana haciendo menos, no más.** Cada técnica real es
una "habilidad":

| Técnica | Mecánica |
|---|---|
| **Observar y dejar pasar** (mindfulness/noting) | Mob efímero: mantener el centro / no reaccionar → se disuelve. Reaccionar lo alimenta. |
| **Buscar la raíz** (auto-indagación) | Mob absorbente: seguir su hilo hasta la fuente → disolución permanente + insight. |
| **Categorizar pasado / futuro / ahora** | Mobs etiquetados por color; clasificarlos en tres cestas; los de "ahora" no molestan. Alimenta un medidor de auto-conocimiento/seguimiento. |
| **Respiración** (ancla) | No es un mob: es el **regen/campo**. Mantener el ritmo inhalar/exhalar crea un campo calmo que ralentiza a los mobs; perder el ritmo los deja enjambrar. |

### Otras técnicas útiles a añadir

- **Escaneo corporal (body scan)** — recorrer el cuerpo por partes. Engancha con `IBody`
  (stats por extremidad / estrés postural): baja estrés postural por zona.
- **Bondad amorosa (metta)** — buena voluntad hacia otros; alimenta mobs "a curar/proteger" y
  el sistema de Bond.
- **Etiquetado (noting)** — nombrar el pensamiento ("planear", "recordar", "juzgar") lo debilita.
- **Concentración en un punto (trataka)** — fijar un objetivo único mientras el resto distrae.
- **Impermanencia / gratitud / escucha de sonidos** — anclas alternativas a la respiración
  para variar misiones sin mecánica nueva.

---

## 10. Impacto en el código

### Cimiento implementado (2026-07-14) — `Assets/Scripts/Meditation/`

Primer slice: el flujo de entrada completo (máquina y loto), con el fundido a negro que oculta
la transición. Singletons con **auto-bootstrap** (se crean en runtime si no están en escena),
así que es probable sin tocar `SampleSceneBuilder`.

| Archivo | Rol | Estado |
|---|---|---|
| `ScreenFader` | Overlay negro a pantalla completa; `FadeToBlack`/`FadeFromBlack`. Oculta el shift (§3). | ✅ |
| `RealityShiftController` | Generaliza la antigua `KitchenScaleController` (borrada 2026-07-23): **snap** de escala del `environmentRoot` + FOV, activa/desactiva los mobs de la misión. Uno por área. | ✅ |
| `MobMission` | Una misión seleccionable: nombre, descripción, `MissionCategory`, `mobSet`, `scaleOverride`, eventos `onBegin/onEnd`. | ✅ |
| `MissionSelectMenu` | UI sobre el negro; lista misiones disponibles (teclas 1–9 / Escape). Construye la UI en runtime si no está cableada. | ✅ |
| `MeditationSession` | Orquestador del flujo compartido: animación → negro → menú → snap → fade-in; `EndMission()` lo revierte. | ✅ |
| `VirtualizationMachine` | Trigger por área (`IInteractable`): `ConfirmationPanel` → `onEnterAnimation` → `MeditationSession.Open()`. | ✅ |
| `LotusMeditationAbility` | Camino del loto: misma llamada a `MeditationSession.Open()`, sin máquina; gated por `hasMobMissionSpell`. | ✅ |

### Mobs y primera misión (2026-07-14)

| Archivo | Rol | Estado |
|---|---|---|
| `MeditationMob` | Base de mobs de meditación (independiente de `IngredientMob`/NavMesh; steering cinemático). `Dissolve()` en vez de "matar" (regla no-violencia). | ✅ |
| `EphemeralThoughtMob` | Arquetipo efímero (§8): persigue al jugador; si no logra tocarlo durante `dissolveDelay` s → se desvanece. Cada toque resetea su timer. | ✅ |
| `PostureVisualizationMission` | Primera misión concreta (§7): spawnea N pensamientos; el jugador huye para disolverlos; al llegar a la meta aplica la recompensa, llama `EndMission()` y dispara `onCompleted`. Se auto-cablea vía `MobMission.OnBegan/OnEnded`. | ✅ |
| `MeditationReward` | Bloque de recompensa reutilizable (serializable): sube el dominio de la asana (`Asana.RegisterPractice`), aumenta `PlayerStats.observationRadius` (permanente) y opcionalmente da monedas (`CoinWallet`). | ✅ |
| `MeditationMissionBase` | Base de misión: spawn N → resolver N → recompensa → `EndMission` + limpieza. Subclases solo definen `CreateMob`. Habilita "una misión por área" por configuración. | ✅ |
| `PostureFormMob` + `AsanaFormationMission` | Arquetipo postura (huye; lo sostienes para formarlo) + su misión. | ✅ |
| `ChannelMob` + `ChannelMission` | Arquetipo universal de presencia + su misión (reskin por área). | ✅ |
| `AbsorbentThoughtMob` + `RootInquiryMission` | Arquetipo absorbente (te persigue rápido; sigues el hilo a su raíz). | ✅ |
| `HealMob` + `HealingMission` | Arquetipo a curar (cojea hacia ti; lo sanas con presencia). | ✅ |
| `ProtectMob` + `WardAttackerMob` + `ProtectionMission` | Arquetipo a proteger (escort/defensa con oleadas; puede fallar). | ✅ |

**Cómo probarlo en escena:** en cada área, crea un GameObject `EnvironmentRoot` con la geometría
y los mobs; añade `RealityShiftController` (asigna `environmentRoot`). Crea un GameObject por
misión con `MobMission` **+ `PostureVisualizationMission`** (deja `thoughtPrefab` vacío para usar
la esfera-placeholder). Coloca la `VirtualizationMachine` (collider trigger) y asígnale el `shift`
y el array `missions`. Acércate e interactúa (F/clic) → elige la misión → **huye de las esferas**
que aparecen; cada una se desvanece si no te toca en unos segundos. Para el loto: añade
`LotusMeditationAbility` al jugador, marca `hasMobMissionSpell` y pulsa `M`.

> Ajuste de balance: `thoughtSpeed` debe ser **menor** que la velocidad del jugador, si no no
> podrá huir. Tunéalo en el `PostureVisualizationMission`.

### Aún pendiente de implementar
- Integrar `SurfaceWalker`/avatares con las misiones: hoy la locomoción del Eje A existe
  (suelo/pared/techo/aire) pero las misiones aún spawnean mobs en el suelo. Falta tunear en
  editor los raycasts y cablear el avatar-robot como controlador dentro de la simulación.
- Arquetipos de mob restantes (§8): espejo, mimético, territorial, y las variantes bespoke de
  áreas complejas (taming submarino, lenguaje de monstruos, combos de FuelLab).
- Resto de misiones de las demás categorías (§7): observar el cuerpo, no pensar, espejo…

### A generalizar / refactorizar
- `KitchenScaleController` y `KitchenEntrance` → **hecho (2026-07-23)**: ambas clases se **borraron**;
  su función la cubren `RealityShiftController` (miniaturización genérica por área) +
  `VirtualizationMachine` (trigger universal) + `MobWorldLoader` (mundo mob en escena).
- Modo contemplativo / `observationRadius` (hoy solo un float) → integrar con `MeditationSession`.

### Bugs/huecos relacionados (ver `known-issues.md`)
- `AreaClear` depende de un `KitchenCombatManager` **que no existe** → el cierre de misión
  "limpiar área" (relevante cuando el robot limpia partículas) aún no está cableado.
- `MissionTracker.ReportMobProcessed` / `ReportAreaCleared` sin invocadores.

---

## 11. Decisiones pendientes

- [ ] Locomoción de inmersión del plano "dentro de materiales" (§6).
- [ ] ¿Cómo se representa visualmente el interior de cada material?
- [ ] Orden fino del Eje B en el tramo energético↔invisible.
- [ ] Feedback visual/sonoro de entrar en meditación (loto) vs. máquina.
- [ ] ¿El "hechizo de independencia" es único o por área/tier?
- [ ] Lista concreta de posturas-mob y su dominio requerido en la sala de yoga.
- [ ] Cómo persisten los ítems de la simulación en el inventario real (racional ya dado por la ficción; falta el cableado).

---

## 12. Matriz de misiones por área (al menos una por área)

Cada área del santuario tendrá **al menos una `MobMission`** disparada por su
`VirtualizationMachine`. Con el framework actual, montar una es **configuración**: un GameObject
con `MobMission` + una subclase de `MeditationMissionBase`, listado en la máquina del área.

Tres arquetipos ya implementados cubren la mayoría de biomas (todos no-violentos):
- **Efímero** (`EphemeralThoughtMob` / `PostureVisualizationMission`) — te persigue, **huyes**.
- **Postura** (`PostureFormMob` / `AsanaFormationMission`) — **huye, lo persigues y sostienes**.
- **Canalizar** (`ChannelMob` / `ChannelMission`) — **presencia**: te acercas y atiendes; universal
  (procesar / calmar / limpiar / neutralizar), reskin por área.

| Área (tier) | Bioma (gameplay-loops) | Misión mínima | Arquetipo | Estado |
|---|---|---|---|---|
| YogaRoom (1) | visualización mental | visualizar postura + formar posturas | Efímero + Postura | ✅ jugable |
| Kitchen (1) | proceso de ingredientes | procesar ingredientes | Canalizar (reskin "procesar") | 🟡 config |
| Cleaning (1) | limpieza | limpiar suciedad | Canalizar (reskin "limpiar") | 🟡 config |
| Garden (2) | plagas territoriales | limpiar plagas | Canalizar (+territorio futuro) | 🟡 config |
| AlchemyLab (2) | radicales libres | neutralizar radicales | Canalizar (reskin "neutralizar") | 🟡 config |
| TextileStudio (2) | destreza fina | (Irosene destaca aquí) | Canalizar / bespoke | 🔲 diseño |
| FuelLab (3) | combos de reacción | secuencia de inputs | **bespoke** (combos) | 🔲 diseño |
| CulturedMeatLab (3) | química avanzada | canalizar cultivos | Canalizar | 🟡 config |
| SupplementsPharmacy (3) | minerales/vitaminas | canalizar | Canalizar | 🟡 config |
| UnderwaterGarden / Submarine (4) | taming marino | calmar criatura | Canalizar → **taming bespoke** | 🟡→🔲 |
| NightWatch (4) | avistamientos | observar/presencia | Canalizar + Mimético (futuro) | 🔲 diseño |
| MonsterSection (5) | lenguaje | comunicar | **bespoke** (lenguaje) | 🔲 diseño |

Leyenda: **✅** jugable · **🟡** montable ya solo con configuración (reskin de `ChannelMission`) ·
**🔲** requiere mecánica bespoke (combos, taming, lenguaje) o arquetipos aún no implementados
(proteger, curar, absorbente, espejo, mimético).

> Las áreas complejas (FuelLab combos, MonsterSection lenguaje, taming submarino) tendrán su
> propia subclase de `MeditationMissionBase` con reglas propias — el patrón ya está listo para
> ellas; solo cambia el `CreateMob`/lógica de resolución.
