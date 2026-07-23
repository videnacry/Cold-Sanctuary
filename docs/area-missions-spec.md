# Spec de Misiones por Área

> Consolidación (2026-07-16) de todo el diseño de misiones disperso en los docs, en una base
> por área lista para cablear. Profundidad: **progresión curada** (1 simulacro + varias misiones
> mob clave por área, no la rejilla exhaustiva).
>
> Fuentes sintetizadas: [`gameplay-loops.md`](gameplay-loops.md) (biomas de combate),
> [`world-simulation.md`](world-simulation.md) (tareas/`AreaTask`), [`mission-mode.md`](mission-mode.md)
> (simulacro vs wow, cero-residuos, contadores), [`fauna-gameplay.md`](fauna-gameplay.md) (cuidado de
> crías), [`bond-activity-system.md`](bond-activity-system.md) (compañeros), [`magic-plane-and-meditation.md`](magic-plane-and-meditation.md)
> (ejes, arquetipos), [`creature-stats.md`](creature-stats.md).

---

## Modelo: qué dimensiones lleva cada misión

**Tipo** (de `mission-mode.md`):
- **Simulacro** — interacción diegética con reglas propias (hacer la tarea real del área). Sin mobs.
- **Mob / wow** — entrar al Microcosmos (máquina/loto) y proceso-combate contra mobs.

**Solo para misiones Mob, tres dimensiones ORTOGONALES (no una escalera):**
- **Escala / capa** (Eje B): insecto → bacteria → molecular/partícula → energético → invisible/mental.
- **Plano / terreno** (Eje A): suelo · paredes · techo · aire · dentro de material. Es **variedad**,
  gated por avatar (gusano→araña→mosco→…). **NO es un ranking de dificultad.**
- **Dificultad = requisitos + habilidad** (estilo WoW), independiente del plano:
  - Cada misión tiene un **nivel/umbral mínimo**: hechizos + stats para tener *opciones* reales.
  - Bajo el umbral → el jugador debe fortalecerse con otras misiones y **volver** (no es "más de lo mismo").
  - La **habilidad/lectura de patrones** compensa stats: leer el patrón reduce la carga; no leerlo la
    aumenta. Subir por encima del umbral hace la misión más holgada (menos dependiente del patrón).
  - Al ganar poder se desbloquean misiones más difíciles **en todos los planos ya abiertos**, no solo
    "subir al aire".

**Ejemplo canónico (colmena de hormigas — escala insecto):** se desbloquea en un nivel avanzado
porque el jugador ya tiene hechizos para sobrevivir a **10 hormigas simultáneas**. La colmena sigue
un **patrón**: si lo descifras llegas al núcleo con enfrentamientos de ~10; si no, acabas en
enfrentamientos de más. Con más stats/hechizos sobrevives a 20 simultáneas → la completas casi sin
pensar. (Check de "nivel/equipo" + skill de mecánicas.)

**Común**: nombre, categoría mental (§7 del Microcosmos), `targetCount`, recompensa, gating.

---

## Los tres sistemas de misión — cómo encajan (resolución de solapamiento)

Hay tres cosas que los docs llaman "misión/tarea"; se reparten así:

| Sistema | Qué es | Cuándo | Código |
|---|---|---|---|
| **`AreaTask` (autónomo)** | Ciclo de fondo que sube stats poco a poco | Cuando el personaje **no** está en misión (incl. NPCs, FOMO) | `WorldCharacter.AutonomousTaskLoop`, `SanctuaryArea` |
| **Simulacro** | Misión diegética activa del jugador (hacer la tarea de verdad) | El jugador la acepta en el área | pendiente (yoga usa `Asana`; cocina, `KitchenCombatManager` futuro) |
| **Mob / wow** | Misión en el Microcosmos contra mobs | El jugador entra por la **máquina de virtualización** / loto | `MeditationMissionBase` + arquetipos ✅ |

Regla: **el "modo mob" es el estilo *wow* de este juego**; **cada área tiene simulacro Y mob**. Las
`AreaTask` no son misiones de jugador — son el telón de fondo que hace subir stats cuando no estás
en una misión (y el *conscription loop* de `mission-mode` puede forzarlas cuando un contador baja).

---

## Estado — qué es jugable HOY

- **Jugable hoy**: misiones Mob en **plano suelo** con los arquetipos ya implementados
  (`EphemeralThoughtMob`, `PostureFormMob`, `ChannelMob`, `AbsorbentThoughtMob`, `HealMob`,
  `ProtectMob`+`WardAttackerMob`) vía `MeditationMissionBase` (y `ProtectionMission` bespoke).
- **Eje A (planos)**: la locomoción ya existe — `SurfaceWalker` + avatares (`Assets/Scripts/Avatar/`,
  gusano/araña/mosco) están implementados. Falta **definir y cablear** las misiones de pared/techo/aire
  por área y el avatar de "dentro de material" (inmersión).
- **Pendiente (bespoke)**: combos (FuelLab), taming (submarino), lenguaje (MonsterSection),
  arquetipos aún sin código (espejo, mimético, territorial).
- **Pendiente (simulacro)**: casi todo (falta `KitchenCombatManager` y el bucle de simulacro genérico;
  solo yoga tiene base vía `Asana`).

Leyenda de estado por misión: ✅ jugable hoy · 🟡 montable con arquetipos actuales (falta plano/def) ·
🔵 bespoke (mecánica propia) · ⚪ simulacro pendiente.

---

## TIER 1

### CubCare — cuidado de crías (dominio de fauna)
- **Simulacro** (rico, ver `fauna-gameplay`): presencia tranquila, respuesta vocal, alimentar, enriquecimiento
  olfativo/texturas, puzzle de comida, grooming, juego de caza, socialización, entrenamiento de cooperación.
  Gated por **bond** de la cría (0→70). Recompensa: bond, `Ca` (alimentar), satisfacción. ⚪
- **Mob**: no encaja el combate — es nutrición. Posible mob "a curar/proteger" a futuro (cría en peligro).

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Calmar el miedo de la cría | mental · suelo | HealMob (a curar) | stats bajos; leer señales de estrés | bond, satisfacción | ✅ |
| Proteger a la cría de sus miedos | mental · suelo | Protect (oleadas) | interponerse; puede fallar | bond | ✅ |

### Cleaning — limpieza (dominio de Panterilia)
- **Simulacro**: limpiar zonas del santuario (barrer/fregar). Economía: grasa/sebo → **jabón**. ⚪
- **Mob**: escala **bacteria** — gérmenes/suciedad microscópica.

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Despejar colonia de gérmenes | bacteria · suelo | Channel | básico | limpieza (contador), Observación | 🟡 |
| Gérmenes en la pared/techo | bacteria · pared/techo | Channel | avatar araña | limpieza | 🟡→(Eje A) |

### Kitchen — cocina, el mundo de la hormiga (dominio de Goluis) ★ mejor diseñada
- **Simulacro** (`mission-mode`): preparar platos (pizza, estofado, postre) con **ingredientes, tiempos y
  temperaturas**. Economía: restos → **compost**, grasa → **biodiésel**. ⚪ (falta `KitchenCombatManager`)
- **Mob** (`gameplay-loops`): miniaturización ×8; procesar ingredientes (no matar) → `ElementFragment`s.

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Procesar Sal / Levadura / Alliina / Capsaicina | insecto · suelo | Channel/Process | leer patrón de ataque; controlar levadura (se reproduce) | Na, C, S… (tabla) | 🟡 (ya hay `IngredientMob`) |
| Limpiar el área (AreaClear) | insecto · suelo | Channel | procesar todos | compost + fragmentos | 🔵 (falta `KitchenCombatManager`) |
| Plaga en la despensa (vertical) | insecto · pared/techo | Channel | avatar araña | fragmentos raros | 🟡→(Eje A) |

### Infirmary — enfermería
- **Simulacro**: procedimientos de cuidado/curación (vendajes, dosis). ⚪
- **Mob**: escala **bacteria** — patógenos; arquetipo **a curar** (futuro) / Channel "estabilizar".

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Estabilizar una dolencia | bacteria · suelo | HealMob (a curar) | leer el foco de infección | salud, Observación | ✅ |

### VeterinaryClinic — clínica veterinaria
- **Simulacro**: procedimientos veterinarios (liga con "entrenamiento de cooperación" de fauna, bond alto). ⚪
- **Mob**: **calmar al animal** asustado por presencia.

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Calmar al animal | mental · suelo | HealMob (a curar) | leer señales; no forzar | bond, satisfacción | ✅ |

### YogaRoom — sala de yoga ★ (lógica mob ya jugable)
- **Simulacro**: **sostener la asana** (sistema `Asana` real — mantener la postura). ⚪ (base ya existe)
- **Mob**: capa **mental/invisible**; recompensa = **dominio de asana** + Observación (`MeditationReward`).

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Visualizar postura (huir de pensamientos) | mental · suelo | Ephemeral | no reaccionar / mantener distancia | dominio asana, Observación | ✅ |
| Formar posturas (perseguir y sostener) | mental · suelo | PostureForm | acorralar; sostener holdTime | dominio asana | ✅ |
| Buscar la raíz de un pensamiento obsesivo | mental · suelo | Absorbent | no puedes huir; ve a su raíz | Observación, insight | ✅ |
| Pensamientos en vuelo | mental · aire | PostureForm/Ephemeral | avatar mosco | Observación | 🟡→(Eje A) |

---

## TIER 2

### Garden — huerto (trabajo físico, alimento)
- **Simulacro** (`world-simulation`/`mission-mode`): sembrar, regar, cosechar. Economía: **compost**,
  **nitrógeno** (posos), enmienda de calcio (cáscaras). ⚪
- **Mob** (`gameplay-loops`): plagas territoriales, tower-defense ligero.

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Limpiar plaga terrestre | insecto · suelo | Channel/territorial | básico | despeje, N | 🟡 |
| **Infiltrar la colmena de hormigas** | insecto · suelo | bespoke (oleadas) | hechizos para sobrevivir ~10 simultáneas; **descifrar el patrón** para llegar al núcleo con enfrentamientos de 10 (si no, de más) | élite/rara | 🔵 (ejemplo canónico) |
| Plaga voladora (pulgón) | insecto · aire | Channel | avatar mosco | despeje | 🟡→(Eje A) |
| Hongo en la raíz | bacteria · dentro de material | Channel | avatar "inmersión" | enmienda | 🔵→(Eje A) |
| Fijar nitrógeno | molecular · suelo | Channel | escala molecular | N (tabla) | 🟡 |

### VehicleWorkshop — taller (tractor, barco, submarino)
- **Simulacro**: reparar/afinar vehículos. Economía: aceite usado → **biodiésel** (con FuelLab). ⚪
- **Mob**: candidato natural a **"dentro de material"** (acero, componentes).

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Afinar una pieza | molecular · suelo | Channel | básico | pieza | 🟡 |
| Reparar fatiga del metal | molecular · dentro de material | Channel/bespoke | avatar inmersión; leer la grieta | pieza rara | 🔵→(Eje A) |

### AlchemyLab — alquimia vegetal (primera tabla periódica) ★
- **Simulacro** (`world-simulation`): destilación de plantas, bioluminiscencia; mezcla incorrecta → explosión. ⚪
- **Mob** (`gameplay-loops`): **radicales libres / iones inestables**; combinar correctamente los neutraliza,
  mal → daño de área.

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Neutralizar radicales | molecular · suelo | Channel/bespoke (combinar) | conocer combinaciones; error = daño | C, P (tabla) | 🔵 |

### TextileStudio — estudio textil (Irosene destaca aquí)
- **Simulacro**: hilar/tejer (destreza; Irosene tiene velocidad de "envolver dulces"). Economía: **jabón**,
  lino/lana/seda. ⚪
- **Mob**: fibras/hebras a escala molecular.

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Tejer la hebra | molecular · suelo | PostureForm/Channel | seguir la hebra; destreza | prenda/material | 🟡 |

---

## TIER 3

### FuelLab — combustible (biogás, biodiésel, electrólisis)
- **Simulacro** (`world-simulation`): electrólisis, producción de biogás, síntesis de biodiésel. ⚪
- **Mob**: **no hay mobs** — mecánica de **combos** de reacción (inputs en tiempo). 🔵 bespoke.
  Recompensa: H, O, C, N. Escala molecular/energético.

### CulturedMeatLab — carne cultivada (química avanzada + nutrición)
- **Simulacro**: cultivar/nutrir cultivos celulares. ⚪
- **Mob**: escala **bacteria/celular** — canalizar cultivos.

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Nutrir el cultivo | bacteria · suelo | Channel | básico | proteína, elementos | 🟡 |

### SupplementsPharmacy — farmacia (minerales, vitaminas; Panterilia nv.2; remedios de Irosene)
- **Simulacro**: formular suplementos. ⚪
- **Mob**: escala **molecular** — minerales/vitaminas a canalizar.

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Canalizar un mineral | molecular · suelo | Channel | básico | minerales (tabla) | 🟡 |

---

## TIER 4

### SubmarineAccess — pilotaje / misiones submarinas
- **Simulacro**: pilotar el submarino (desbloquea zonas profundas). ⚪
- **Mob**: el **agua es un "plano"** (nadar ≈ volar en 3D). Taming submarino (ver abajo).

### UnderwaterGarden — jardín submarino (organismos raros; Irosene reside aquí)
- **Simulacro**: control de respiración, cultivo submarino. Recompensa: **P** (bioluminiscencia), marinos raros. ⚪
- **Mob** (`gameplay-loops`): criatura submarina con **taming** (leer patrón de comportamiento, presentar el
  estímulo correcto). 🔵 bespoke (taming).

| Misión mob | Escala · Plano | Arquetipo | Requisito / patrón | Recompensa | Estado |
|---|---|---|---|---|---|
| Domesticar la criatura | — · agua | taming (bespoke) | leer patrón; estímulo correcto | elemento marino raro | 🔵 |

### NightWatch — guardia nocturna (primeros avistamientos de monstruos)
- **Simulacro**: rondas de vigilancia/observación nocturna. ⚪
- **Mob**: arquetipo **mimético / oculto** — solo visible con Observación alta. 🔵 (arquetipo pendiente).

---

## TIER 5

### MonsterSection — contención, lenguaje, curación de monstruos ★ end-game
- **Simulacro**: contención/cuidado. ⚪
- **Mob** (`gameplay-loops`): **lenguaje** — aprender gestos/elementos químicos para comunicarse; "procesar"
  un monstruo = entender lo que necesita, no vencerlo. 🔵 bespoke (lenguaje). Espejo/self-inquiry como endgame mental.

---

## Huecos transversales

- **Áreas sin diseño de mob previo** (solo propuesta aquí): Infirmary, VeterinaryClinic, VehicleWorkshop,
  TextileStudio, CulturedMeatLab, SupplementsPharmacy, NightWatch, Cleaning. Todas salen con `ChannelMission`
  reskineado salvo donde marco bespoke.
- **Simulacro casi inexistente en código**: solo yoga vía `Asana`. Falta un bucle de simulacro genérico y el
  `KitchenCombatManager` (bloquea `AreaClear` y los contadores/economía cero-residuos).
- **Contradicción de sistemas resuelta arriba**: `AreaTask` (autónomo) ≠ misión de jugador; el "modo mob" ES
  el wow. `SanctuaryMission.MissionType` (IngredientCollection/AreaClear/YeastControl) es el tracking viejo de
  cocina — habrá que decidir si se integra con `MeditationMissionBase` o coexiste.
- **Eje A (planos)**: la locomoción por planos ya existe (`SurfaceWalker` + avatares en
  `Assets/Scripts/Avatar/`). Lo que queda es **definir y cablear** las misiones concretas por encima de
  "suelo" (pared/techo/aire) en cada área, más el avatar de "dentro de material".
- **Arquetipos pendientes** que varias áreas necesitan: a curar, a proteger, absorbente, espejo, mimético,
  territorial, y las mecánicas bespoke (combos, taming, lenguaje, oleadas de colmena).

---

## Próximos pasos sugeridos

1. Validar este mapa contigo (¿escalas/planos por área correctos?).
2. Elegir 2–3 áreas Tier 1 y **definir sus `MobMission` concretos** (nombre, categoría, `targetCount`, reward)
   — jugables ya en plano suelo con los arquetipos actuales.
3. Cablear en escena (cuando confirmes que `SampleSceneBuilder` está al día).
4. Con `SurfaceWalker`+avatares **ya implementados** (Eje A abierto en código), definir y cablear las
   misiones de pared/techo/aire por área; o priorizar los arquetipos que faltan (a curar/proteger para
   Infirmary/CubCare), según qué quieras jugar antes.
