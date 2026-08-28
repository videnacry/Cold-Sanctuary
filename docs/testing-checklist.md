# Checklist de pruebas (Mesocosmos: progresión, farming, recursos, trepar, cocina)

Registro de pruebas del build hasta 2026-07-30. **Pasada de testing (automatizada) hecha 2026-07-28/29/30**:
la mayoría confirmado con evidencia de `Logs/Editor.log` (ver "Estado de sesión"). **Lo que queda requiere
juego MANUAL** (WASD / mantener tecla / apuntar+confirmar): rama de daño de "Dura", trepar, 4 esferas de
Mesopotamia + YogaPortal, misión de yoga, `ThoughtField_Agua`, velocidad de tiempo, flujo del motor de
Virtualización en Kitchen/Garden/Mecánica/Construcción/Forja/Dispatch (`VirtualPointer` + estaciones +
receta + tickets). Los ítems `[x]` son historial verificado.

> **Controles nuevos:** `V` = jugar con criatura · `F`/clic = interactuar (dar de comer, máquinas) ·
> `Espacio` = trepar. (Combate/movimiento previos sin cambios.)

## Estado de sesión (para retomar sin contexto previo)

- **CICLO 2026-08-27 — pull de 28 commits (PRs #135-148) + sync (8 .cs, 3 nuevos), 0 errores**. El
  checkout local había quedado 28 commits atrás (git lo detectó como fast-forward puro); mi nota local
  sin comitear en `testing-checklist.md` chocó con `git pull` normal ("would be overwritten by merge")
  porque el compañero también editó ese archivo (solo agregó secciones nuevas al final, sin tocar mi
  parte) — resuelto con `git stash` → `pull` → `stash pop`, sin conflictos. Batch: **motor de volición
  (D3b)** — `Assets/Animals/Volition.cs` + `DesireCatalog.cs` (nuevos): un "deseo" elegido por
  necesidad×capacidad×confianza despacha las respuestas existentes (reemplaza la prioridad fija de
  `ActiveBehaveTick`), migrado tras flag con paridad garantizada (el propio PR dice que el comportamiento
  debe verse IDÉNTICO a antes). **Navegación ambiental (N1/N7)**: `TraceField.cs` (nuevo, rejilla de
  rastros — descrita como "dormida", sin invocadores activos todavía) + ajustes en `LifeStage.cs`
  (huir lejos + deambular por rastro) y `SpellBase.cs` (subproducto = extensión de hechizo). Sync +
  rebuild de escena + compilación → **0 errores** confirmado por `Editor.log`. Testeado en Play:
  `[FamilyGenerator] 11 familia(s) generadas — 66 animales en total` sigue funcionando, 0 excepciones
  reales tras ~45s activo (solo ruido conocido: WebSocketException del relay MCP desconectado). Sin
  regresión visible, consistente con que ambos sistemas nuevos están detrás de flag/dormidos por diseño.

- **CICLO 2026-08-26 — confirmación en vivo del PR #126 (Carnivore/Herbivore) + sync PRs #127-134 (19
  commits, "capacidad=hechizo": eje de armamento, confianza-por-uso, gate por sentidos, disuelve
  canHitAndRun) — 0 errores**. Unity llevaba días cerrado; se reabrió sin problema (bridge MCP y
  computer-use habían estado caídos el ciclo anterior, ahora reconectaron). Con Unity real esta vez:
  **compilación limpia confirmada por Editor.log** (0 `error CS`, el único "1 error" en consola es el
  ruido conocido del Relay/AI Assistant con el MCP desconectado) — esto **confirma retroactivamente**
  la revisión manual del ciclo anterior sobre PR #126 (borrado de `Carnivore`/`Herbivore`), que se había
  hecho sin poder compilar. Reconstruida la escena y testeado en Play: `[FamilyGenerator] 11 familia(s)
  generadas — 66 animales en total` sigue funcionando (el fix de la sesión pasada aguanta), 0
  excepciones reales en toda la sesión de Play (~60k líneas de log). Nuevo batch sincronizado: sistema
  de "capacidad=hechizo" (`Assets/Animals/Capability.cs`, nuevo) — eje de armamento ⟂ masa en
  `Predation`, confianza-por-hechizo/uso alimentando la agresividad (reemplaza `canHitAndRun` estático),
  `Assess` gateado por sentidos (percepción × legibilidad). Ver `docs/capabilities-and-embodiment.md`
  (agregado por el batch) para el diseño completo.

- **CICLO 2026-08-23 — pull revisado: `b75e037` (mis 3 fixes de generación de familias, comiteados por
  el compañero tal cual) + `2e55323`/PR #126 (borra `Carnivore`/`Herbivore`, las 8 especies heredan
  directo de `Animal`; qué comen pasa a ser data vía `Forager.ConfigureForSpecies`)**. Sincronizado al
  proyecto vivo (incl. borrado de `Carnivore.cs`/`Herbivore.cs`). **Sin verificación de compilación en
  vivo esta vez**: tanto el bridge MCP de Unity como las herramientas de control de escritorio estuvieron
  caídas todo el ciclo (problema conocido de este entorno) — no pude forzar un recompile ni tomar
  capturas. En su lugar hice una revisión manual exhaustiva del diff de PR #126 (que el propio commit
  admite no haber podido compilar: "verificado por conteo de llaves y grep de símbolos"): confirmé que
  las 8 clases de especie cambiaron de base correctamente (`Carnivore`/`Herbivore` → `Animal`), que
  `ConfigureForager`/`GrazesOnLand` no tienen ninguna referencia de código sobreviviente (solo quedan en
  comentarios), que los 2 sitios que hacían `GetComponent<Carnivore>()` (`Animal.SenseThreats`,
  `FishSchool.NearestPredator`) se recablearon correctamente a `Forager.eatsPrey`, y que los 3 campos
  `eatsPrey`/`eatsGrass`/`eatsFish` que usa el nuevo `Forager.ConfigureForSpecies()` existen. **No
  reemplaza una compilación real** — pendiente confirmar con Editor.log en el próximo ciclo en que las
  herramientas reconecten.

- **CICLO 2026-08-21 — sync etapa 5 (38 commits, PRs #108-125, 27 .cs) + 3 BUGS REALES encontrados y
  arreglados en la cadena de generación de familias, el más grave de la sesión**: la población de fauna
  auto-generada (`FamilyGenerator`/`WildlifePopulation_AUTO`, 11 familias / 66 animales) estaba
  **crasheando en el primer intento y generando CERO animales**, silenciosamente — probablemente la razón
  real detrás de la nota del propio §23 ("el sandbox apenas dispara caza": no es que la IA no cace, es que
  no había con quién). Antes de esto, sync limpio (0 errores) de la primera mitad del batch (data-driven:
  `SpeciesBody`, `SpeciesProfile`, `StageProfile`, `PostNatalProfile`, `AnimalPopulations`, retiro de
  `Diet.cs`; también arreglado un typo de compilación separado, `SpeciesBody.cs:65` usaba `Anima.
  sensibility` que no existe en la clase base — ver abajo).
  1. **BUG compilación**: `SpeciesBody.cs(65,67)` — `a.sensibility` no existe en `Anima` (es específico de
     `Animal`, y además redundante: `Animal.EvolveAptitudes()` ya lo recalcula cada tick). Fix: quitar esa
     asignación de `SpeciesBody.Apply()`.
  2. **BUG runtime — `IndexOutOfRangeException` en cada Play, 100% reproducible**: `FamilyGenerator.Start()`
     llama `template.RenderFamily(...)` sobre el PREFAB ASSET (nunca instanciado/`Init()`-ado, solo
     template) — `Animal.RenderFamily()` leía `this.Group.familySize` asumiendo que `Group` (dato de
     especie, etapa 5) ya estaba listo. En la práctica `Group` podía quedar no-nulo pero con
     `familySize=0` (estado que persiste entre sesiones de Play en el mismo proceso de Editor — no se
     resetea con "Reload Domain" para objetos que no son parte de la escena, como un prefab asset).
     `RenderGroup(quantity=0)` → array vacío → `SetParents(scripts=[])` → `scripts[0]` explota.
     **Resultado: 0 de 66 animales generados, cada vez.** Fix en `Animal.RenderFamily()`: validar
     `Group.familySize > 0`, no solo `Group != null`, antes de confiar en él (si no, `Family.Of
     (SpeciesArchetype)` fresco).
  3. **BUG runtime — `NullReferenceException`, expuesto al arreglar el #2**: con el familySize ya
     resuelto correctamente, `Family.SetParents()` (Family.cs:84) leía `scripts[0].Group.familySize` —
     pero `scripts[0]` es una criatura RECIÉN `Instantiate()`ada; Unity difiere su propio `Start()`
     (donde correría `Init()` y fijaría `Group`) hasta después de que `FamilyGenerator.Start()` termina
     en el mismo frame. `Group` seguía `null` en ese punto → NRE. Fix: `SetParents`/`RenderFamily` ahora
     reciben `familySize`/`parentalCare` como PARÁMETROS (que el caller ya tiene) en vez de leerlos de
     una instancia que todavía no se inicializó.
  **Confirmado con evidencia de `Editor.log`**: tras los 3 fixes, `[FamilyGenerator] 11 familia(s)
  generadas — 66 animales en total.` (antes: crash inmediato, 0 animales). `Forager.Hunt()` corrió 612
  veces en la sesión de Play siguiente (antes: prácticamente nunca, no había presas/depredadores cerca)
  — la caza real ahora se dispara sola con población real, sin necesidad de forzar el encuentro a mano.
  0 excepciones reales en toda la sesión final (solo ruido conocido: Licensing 404, Relay/AI Assistant
  desconectado). **Los 3 archivos (`Assets/Animals/SpeciesBody.cs`, `Assets/Animals/Animal.cs`,
  `Assets/Scripts/Family.cs`) están trackeados en git — listos para commitear.**

- **CICLO 2026-08-18 16:33 — sync commit `307891c`** (compañero comiteó los 4 archivos que el ciclo
  anterior estaban `staged` sin commitear): `Assets/Scripts/Microcosmos/CaveTrigger.cs`,
  `KitchenFireMission.cs`, `ScentEmitter.cs`, `ScentScanner.cs`. Sincronizados al proyecto vivo,
  compilación completa (~4 min) → **0 errores** confirmado por `Editor.log`. Aún **no están cableados**
  en `SampleSceneBuilder.cs` (no aparecen sandboxes nuevos en la Hierarchy) — son `MonoBehaviour`
  standalone sin invocadores todavía; nada que testear en Play mode por ahora hasta que el compañero
  los integre a la escena.

- **CICLO 2026-08-18 — sync PRs #87-107 (46 commits, 26 .cs), 0 errores tras compilar y testear en
  Play**. Unity llevaba ~4 días cerrado (última actividad 2026-08-14); lo reabrí desde Unity Hub
  (dialog "Recovering Scene Backups" → No, como siempre). Batch: **refactor grande de comportamiento
  animal** — se extraen políticas antes mezcladas en `Animal`/`Carnivore`/`Herbivore`/especies a
  componentes nuevos y composables: `Assets/Animals/Locomotion.cs` (NavMesh+gait, unifica
  wander/flee/fight/hit-and-run), `Forager.cs` (política qué/dónde comer, con modo MIXTO omnívoro por
  flags combinables), `ThreatResponder.cs` (luchar/huir por stats, `EvaluateThreat`→`Assess` 100%
  stat-based). También: **locomoción universal** (`WalkSpell` ahora conduce tanto a `PlayerController`
  como a animales vía `Control/AiBrain.cs`/`PlayerBrain.cs` — el NavMesh sigue navegando, pero la
  velocidad sale del hechizo), **reconciliación IA/posesión etapa 4** (`AiBrain` conduce decisiones
  activas; la posesión las suprime, sin pisarse), animación de carga de hechizos + `powerBonus`
  unificado (charge+channeling+forcejeo) en `SpellBase.cs`/`FireSpell.cs`, pensamientos base de especie
  data-driven en `Soul/Archetypes.cs`. Sync + rebuild de escena + ~4 min de compilación completa
  (26 archivos) → **0 errores** confirmado por `Editor.log`. Testeado en Play mode: animales (`Bunny`)
  spawneando y corriendo su IA (Locomotion/Forager/ThreatResponder) sin excepciones tras ~50s activo.
  Único "error" en consola fue ruido nuevo no relacionado a código: el Relay del AI Assistant de Unity
  fallando al conectar al MCP (`unityMCP` está desconectado esta sesión, confirmado aparte) — no es un
  bug real, análogo al ruido ya conocido de Licensing 404.

- **CICLO 2026-08-14 (tarde) — sync PRs #85-86 (desgaste por trabajo + jugador en modelo de ánimo), 0
  errores, mi fix de `CameraManager` de este mismo día quedó COMITEADO por el compañero** (commit
  `76559a0 "Update CameraManager.cs"` — confirmado vía `git log` que mi cambio llegó al historial tal
  cual, sin conflicto). Igual mi entrada anterior de este checklist sobrevivió intacta (el compañero
  la tocó en un commit "Update testing-checklist.md" sin pisarla). Nuevo: `Assets/Scripts/Soul/
  Exertion.cs` (costo de energía/fatiga por acción, `ExertionCost`), `MoodDynamics.cs` ampliado,
  `ProductionOrder`/`StockingTask`/`VirtualTask` ahora aceptan `worker`+`stepExertion` (el trabajador
  gasta glucosa/minerales/fatiga por paso → sube cortisol → `Goluis.UnderPressure` se activa solo), y
  el **Player ahora es parte del modelo de ánimo** (`MoodDynamics` se le agrega automáticamente en
  `SampleSceneBuilder`, deriva estrés de sus drives sin necesitar `Mind`). Nuevo sandbox de prueba:
  `Desgaste_AUTO` (obrero Goluis con receta de labor pesada — cantera/moler/apilar). Reconstruí escena
  + testeado en Play mode (0 errores, 0 excepciones nuevas, log revisado línea por línea). **Nota
  operativa nueva**: durante este ciclo GitHub Desktop tomó el foco del sistema y bloqueó
  temporalmente el computer-use (usuario denegó acceso a esa app) — se resolvió forzando el foco de
  Unity vía PowerShell (`SetForegroundWindow`/`ShowWindow` de user32.dll) en vez de reintentar clicks a
  ciegas. Anotado por si se repite.

- **CICLO 2026-08-14 — sync PRs #62-84 (55 commits, 43 .cs), 0 errores tras compilar, 1 bug real
  encontrado y arreglado**. El cron de 2h había expirado (7 días) otra vez — recreado (`f6566e93`).
  Batch grande: sistema de "alma" nuevo (`Assets/Scripts/Soul/`: Archetypes, BlendSlot, BondPillar,
  HumorProfile, MoodDynamics, SharedSoul(+Demo), SocialField, SoulComposition, SoulConvertDemo,
  SoulMath, SpeciesKarma), **`CompanionBase` RETIRADO** (borrado del repo, reemplazado por composición
  vía `MoodState.cs` — mis fixes de la sesión anterior a ese archivo quedaron sin efecto, ya no aplica),
  y el compañero **arregló por su cuenta el duplicado `PullSpell`** que yo había resuelto la sesión
  pasada (borró `Transformation/PullSpell.cs` del repo, `Microcosmos/PullSpell.cs` ahora sí trackeado
  con el typo `.Transform`→`.transform` ya corregido) — **ya no hay archivos sueltos sin trackear en
  `Microcosmos/`**, el hueco de repo-hygiene de la sesión anterior quedó resuelto solo. Mis fixes previos
  de `CombatAbilityBar.cs`/`IngredientMob.cs` (generalización a `ITarget`) sobrevivieron intactos.
  Sync + rebuild de escena (`Tools > Cold Sanctuary > Build Sample Scene Blockout`) + compilación
  completa (43 archivos, ~4 min) → **0 errores** confirmado por `Editor.log`. Nuevos sandboxes visibles
  en Hierarchy: `AlmaBlend_AUTO`, `AlmaCompartida_AUTO` (SharedSoul demo), además de los ya existentes
  `SpellDemo_AUTO`/`Magia_AUTO`. Testeados en Play mode (HUDs de `SoulConvert`/`SpellDemo` funcionando).
  **BUG REAL encontrado (misma familia que el de `CompanionBase` de la sesión anterior, pero en
  archivo no tocado por este batch)**: `MissingReferenceException` en `CameraManager.TransitionTo()`
  (`Assets/Scripts/Camera/CameraManager.cs:156`) — el `anchor` (Transform hijo del Player,
  `firstPersonAnchor`/`thirdPersonAnchor`) se cachea al iniciar la corrutina de transición de cámara y
  no se revalida frame a frame; si el Player se destruye/reemplaza a mitad de transición (posesión/
  body-swap), la referencia queda colgante. Solo 1 ocurrencia (no en loop, a diferencia del bug anterior
  de compañeros). Fix: null-check de `anchor` en el loop + antes del snap final, con `yield break`
  limpio. **Trackeado, sincronizado a repo y proyecto vivo, listo para commitear.**

- **CICLO CON 4 BUGS REALES (2026-08-12, commit `44cac82` "SpellBase + hechizos Nivel1 Microcosmos")** —
  el más cargado de la sesión. Unity se había cerrado mal (recuperó backups de escena al reabrir, dije
  que No porque nunca guardamos escena). Al abrir el proyecto, **Safe Mode por errores de compilación**:
  1. **BUG de compilación — duplicado**: el commit trackeado agregó `Assets/Scripts/Transformation/
     PullSpell.cs` (diseño físico viejo: NavMeshAgent+resistencia), pero mi checkout local YA tenía
     (sin trackear, `git status` los marca `??`) un set completo y más nuevo de archivos en
     `Assets/Scripts/Microcosmos/` (`ATPRegenSpell`, `BondEscapeReader`, `HomeImpulse`,
     `HoneydewPickup`, `ImpulseController`, `MovementImpulse`, `PullSpell` con diseño de forcejeo/
     tug-of-war, `ThreatScanner`, `WalkToGoal`) que `SampleSceneBuilder.cs` (sí trackeado) ya
     referenciaba exclusivamente. Los dos `PullSpell` (mismo nombre de clase, sin namespaces en este
     proyecto) chocaban → `error CS0101`. **El compañero probablemente olvidó `git add` los ~9 archivos
     nuevos de `Microcosmos/` al commitear** — un clone limpio de este commit ni siquiera compilaría
     (le faltarían `ATPRegenSpell`/`BondEscapeReader`/etc. enteros, no solo el duplicado). Fix aplicado:
     **borré `Transformation/PullSpell.cs` del proyecto vivo únicamente** (no toqué el commit del repo,
     no es mi lugar decidir eso) — quedó el `Microcosmos/PullSpell.cs` que sí es el que se usa.
  2. **BUG de compilación — typo**: `Microcosmos/PullSpell.cs(115)` usaba `target.Transform` (mayúscula)
     en vez de `target.transform` (la propiedad real de `ITarget`). Arreglado directo en el archivo (no
     trackeado en git, así que el fix vive solo en este checkout local y en el proyecto vivo — avisar al
     compañero para que lo replique cuando comitee sus archivos de verdad).
  3. **BUG de compilación — API generalizada sin actualizar callers**: `CombatTargetSelector.cs` (sí
     trackeado, parte del commit) se generalizó de `IngredientMob` a `ITarget`, pero `CombatAbilityBar.cs`
     seguía llamando `UseAbility(..., IngredientMob)` con el `ITarget` nuevo → `error CS1503` ×3. Fix:
     usar `CombatTargetSelector.Instance?.CurrentIngredientMob` (la propiedad de compat hacia atrás que
     la propia clase ya exponía) en los 2 call-sites de `CombatAbilityBar.cs`. Además `IngredientMob`
     no implementaba `ITarget` pese a que el diseño ya lo asumía (`CurrentIngredientMob`,
     `OnMobTargetChanged` hacen cast `as IngredientMob`) → agregué `: ITarget` a `IngredientMob` con
     `Mass=maxHealth`, `Speed=agent.speed`, `Faction='m'`, `Dead`/`Consumed=_isProcessed`,
     `Hurt()=TakeDamage()`. **Ambos cambios trackeados, listos para commitear** (`CombatAbilityBar.cs`,
     `IngredientMob.cs`).
  4. **BUG REAL de runtime (ya con todo compilando)**: `MissingReferenceException` en bucle (999+
     errores) en los 4 compañeros — `CompanionBase._playerTransform` se cachea una sola vez en `Start()`
     y nunca se revalida; si el GameObject "Player" se destruye/reemplaza en runtime (posible con el
     nuevo sistema de posesión/body-swap, corriendo por primera vez ahora que todo compila), la
     referencia queda colgante. Encontrados 3 puntos con el mismo patrón sin chequeo:
     `CompanionBase.CheckPlayerProximity()`, `Gohageneis.Update()` (override, chequeo de celebración),
     `Goluis.Update()` (override, chequeo de presión). Fix: agregar `_playerTransform == null` al guard
     en los 3. **Trackeados, listos para commitear.** Re-testeado: corrida completa en Play sin
     excepciones nuevas.
  **Archivos con fix pendiente de commitear** (todos ya sincronizados al proyecto vivo):
  `Assets/Scripts/Combat/CombatAbilityBar.cs`, `Assets/Scripts/Combat/IngredientMob.cs`,
  `Assets/Scripts/Companion/CompanionBase.cs`, `Assets/Scripts/Companion/Companions/Gohageneis.cs`,
  `Assets/Scripts/Companion/Companions/Goluis.cs`. **Pendiente de que el compañero resuelva en su lado**:
  comitear los ~9 archivos de `Microcosmos/` que le faltan (con el typo de `.Transform` ya arreglado si
  quiere copiarlo) y decidir qué hacer con el `Transformation/PullSpell.cs` viejo/duplicado (¿borrarlo?).
- **CRON DE 2H MURIÓ (expiró a los 7 días) y no lo detecté hasta que el usuario avisó** — quedaron 21
  commits (PRs #35-55: magia/metabolismo/descomposición + `Descomposicion_AUTO`/`Magia_AUTO`) sin
  procesar por varios ciclos. Recreado al retomar (ver instrucciones de la corutina). **Si volvés a ver
  muchos ciclos seguidos de "sin cambios" sin que el usuario intervenga, sospechá que el cron murió —
  correr `CronList` para confirmar en vez de asumir que sigue vivo.**
- **BUG REAL de compilación encontrado y arreglado, commiteado por el compañero** (`51bb93f`):
  `TransformationSpell.cs(43,35)` — `error CS0844`: la variable local `float cost` (línea 51) tenía el
  mismo nombre que el campo `public List<ElementCost> cost` (línea 27), y una línea anterior del mismo
  método (43) ya usaba el campo `cost` → el compilador no permite la variable local hasta su
  declaración cuando tapa a un campo usado antes. Fix: renombrar la local a `totalCost`. **Este error
  llevaba sin detectarse un buen rato** porque los `Assets > Refresh` incrementales de varios ciclos
  anteriores NO estaban recompilando de verdad `Assembly-CSharp` (ver nota operativa abajo) — recién
  se detectó al forzar `Reimport All`.
- **PRs #56-61 sincronizados y probados (2026-08-12)**: `SimpleAnima` (Anima concreto mínimo) +
  `ElementalSpell` (hechizos agua/tierra/viento) + `SupplySpell` (abastecer/"trasplante" a otro
  personaje, tipo healer) + `Magia_AUTO` actualizado para usar un `Anima` real (antes era null-safe
  sin Anima) — ahora comer sube stats de `Constitution` y el exceso va a grasa, además de rellenar
  las pools de magia. Compila limpio (0 errores), sandbox se reconstruye y corre en Play sin
  excepciones. Igual que siempre, el flujo real (aprender/comer/lanzar por botón) necesita input
  manual — no se pudo ejercitar por automatización, pero se confirmó que no crashea dejándolo correr.
  También llegó `SupplySpell` con un `Magia_AUTO_Objetivo` (otro `SimpleAnima` con reservas ya
  desbloqueadas) para probar el trasplante — sin verificar en profundidad, mismo motivo.
- **HALLAZGO OPERATIVO IMPORTANTE**: tras un `Reimport All`, Unity **reinicia el proceso completo**
  (splash screen, no solo domain reload) y a veces **vuelve a loguear en la ruta global por defecto**
  (`C:\Users\Blein\AppData\Local\Unity\Editor\Editor.log`) en vez de la ruta project-relative
  (`C:\Users\Blein\COLD-SANCTUARY\Logs\Editor.log`) que se venía usando toda la sesión. Si después de
  una acción el log project-relative deja de crecer (mismo `wc -l` en checks sucesivos) pese a que la
  UI de Unity sigue respondiendo, **revisar la ruta global** antes de asumir que no pasó nada — así se
  destapó el bug de `TransformationSpell.cs` (el `Build()` parecía "completar sin generar los sandboxes
  nuevos" cuando en realidad estaba compilando con errores silenciosos para ese log).
- Todo lo mergeado hasta la **PR #55** (`Descomposicion_AUTO`/`Magia_AUTO` — sandboxes testables del
  arco magia/metabolismo, commits hasta `0ccc32e`) ya está **sincronizado** repo↔proyecto vivo
  (`C:\Users\Blein\COLD-SANCTUARY`, confirmado archivo por archivo) — ver secciones 11-19. El repo
  recibe cambios de un compañero de equipo (`videnacry`/`beron-gamboa`) en paralelo — **revisar
  `git log` al retomar** por si hay commits nuevos sin sincronizar.
- **PRs #35-55 (magia/metabolismo/descomposición) — compilan (tras el fix de arriba) y los 2 sandboxes
  nuevos corren sin excepciones**: `Descomposicion_AUTO` (botón «Iniciar jornada» → minijuego 3 fases)
  y `Magia_AUTO` (HUD de prueba: aprender→comer→lanzar fuego) se construyen y no crashean; sin
  interacción manual real terminan con "0 elementos + 0 J" (esperado, no jugado de verdad) — mismo
  patrón de "necesita input manual" que el resto del motor de Virtualización. 0 excepciones en toda la
  corrida. Detalle en sección 19.
- **PRs #31-34 (depredación por stats) compilan y no rompen nada, sin sandbox de demo propio** (no
  tocaron `SampleSceneBuilder.cs` — se integra directo en `Diet.SelectPrey`/`Animal.EvaluateThreat`,
  sin UI ni logs dedicados). 0 excepciones nuevas en una corrida completa; regresión limpia (Prólogo/
  Cría/Microcosmos siguen igual). **Nota de continuidad**: `Diet.SelectPrey` ahora filtra presa por
  `Predation.Defense` además de lo que ya filtraba — refuerza (no crea) el hallazgo ya documentado de
  que `Carnivore.Feed()` casi nunca dispara en el sandbox actual (0 actividad de caza observada, igual
  que en ciclos anteriores) — sigue sin ser un bug confirmado, solo un efecto de las distancias/stats
  actuales; la pregunta de si rebalancear quedó abierta para el usuario. Detalle en sección 18.
- Todo lo mergeado hasta la **PR #30** (sistema de Emoción deep-sim completo: modelo/orquesta/Laban/
  legibilidad + `CreatureRig`/`ScreenEffects`) ya estaba sincronizado — fue un salto grande de una sola
  vez: 19 commits (PRs #22 a #30) sin sincronizar de una sesión previa, probablemente por un gap de
  varios días entre ciclos.
- **PRs #22-30 compilan y los 2 sandboxes nuevos con auto-demo corren sin excepciones, sin bugs nuevos**:
  a diferencia de casi todo el motor de Virtualización, `ScreenEffectsSandbox_AUTO` (`debugAutoCycle`) y
  `EmotionOrchestraSandbox_AUTO` (`debugDrive`) están diseñados para probarse solos — confirmado
  visualmente (tinte de pantalla rojo/gris oscilando, texto "siente: ira/miedo" → "siente: serenidad"
  cambiando dinámicamente). Regresión limpia en Prólogo/Cría/Microcosmos pese a que este batch tocó
  `Anima.cs`, `CameraManager.cs`, `PlayerController.cs`, `BodyPosition.cs` y `HeadLook.cs`. Detalle en
  sección 17.
- **PR #21 reescribió `BuildMicrocosmosSandbox`**: la misión de rescate del `AphidGuide` (PR #20, ver
  sección 15 abajo) fue reemplazada por un **tableau narrativo estático** (8 seres con `SoulRecord`:
  nombre de alma/hilo-arquetipo/reencarnación en la Cocina) — ya no hay rescate automático, es lore en
  escena. Compila y corre sin excepciones (los 8 `[Alma] ...` se registran limpios). Nuevo también:
  `UpaYogaSandbox_AUTO` (`UpaYogaSession`) — UI de paneles-tecla (WASD/IJKL) para yoga de cuello, arranca
  sola y se ve bien en pantalla, pero el flujo real (F para avanzar de postura) necesita **input
  manual** — mismo patrón que el resto del motor de Virtualización. Sin bugs nuevos. Detalle en
  sección 16.
- **PR #20 compila y el `MicrocosmosSandbox_AUTO` corre de punta a punta sin excepciones, sin bugs
  nuevos**: a diferencia de los sandboxes del motor de Virtualización (que necesitan input manual), este
  es completamente autónomo — el `AphidGuide` rescata a las 4 frágiles y las lleva a la Cueva
  (`CarryToRefuge` 4/4) solo, sin intervención. Detalle en sección 15.
- **BUG REAL encontrado y arreglado en PR #19** (commiteado `7be2878`): `CarryToRefuge.onComplete`
  (`Assets/Scripts/Prologue/CarryToRefuge.cs`) era un `UnityEvent` **sin inicializar** — al agregarse el
  componente vía `AddComponent` en código (no desde el Inspector), el campo queda `null`, y
  `SampleSceneBuilder.BuildPrologueSandbox` hace `carry.onComplete.AddListener(...)` sin chequeo →
  `NullReferenceException` que **abortaba `SampleSceneBuilder.Build()` a mitad de camino**: todo lo que
  se construye DESPUÉS de `BuildPrologueSandbox` en la lista (Cría, Construcción, Dispatch,
  `MigrationDiagnostics_AUTO`, `BakeNavMesh`) **no llegaba a crearse**. Fix: inicializar el campo
  (`= new UnityEvent()`); apliqué el mismo fix preventivo a `PrologueSequence.onFinished` (mismo patrón,
  no crasheaba hoy solo porque se invoca con `?.` pero es la misma bomba de tiempo). Re-testeado: el
  `Build()` completo ahora termina sin excepciones y genera los 3 sandboxes nuevos.
- **Hallazgo adicional (no arreglado, no bloquea)**: aun con el fix de arriba, el mensaje cruzado de
  `PlaneMessenger` que `BuildPrologueSandbox` engancha via `carry.onComplete.AddListener(...)` **nunca
  se dispara en Play** — confirmado con evidencia de log: `onComplete?.Invoke()` corre (se ve el log
  "Todos los débiles a salvo en la cueva."), pero el `Debug.Log` interno de `PlaneMessenger.Send(...)`
  jamás aparece. Causa probable: los listeners de un `UnityEvent` agregados con `.AddListener(...)` en
  código que corre en **Editor/Edit mode** (como esta herramienta de menú, que arma la escena ANTES de
  que el usuario le dé a Play) son estado en memoria **no serializado** — Unity solo persiste
  `m_PersistentCalls` (los conectados desde el Inspector); al pasar de Edit mode a Play mode la escena
  se re-serializa/deserializa y esos listeners "runtime" se pierden en el camino, quedando el
  `UnityEvent` vacío (pero no null, por eso no hay excepción, solo silencio). No lo arreglé yo —
  requiere decidir el patrón correcto (p. ej. que `PrologueSequence` u otro `MonoBehaviour` que sí viva
  en Play mode haga el `AddListener` en su propio `Start()`, en vez de un Editor-script). Reportado para
  que el compañero lo revise; no bloquea nada más, solo significa que el aviso de "volvé por el
  YogaPortal" no aparece en este sandbox.
- **PR #18 (Mecánica/Construcción/Dispatch) compila y los 4 sandboxes nuevos corren sin excepciones**,
  pero el flujo real (abastecer/reparar/construir/forjar) necesita **input manual real** (apuntar con
  la mira + confirmar) — igual limitación que el motor de Virtualización de PR #17. Sin bugs nuevos
  encontrados en esta pasada. Detalle en sección 13.
- **Bug real encontrado y arreglado en PR #16** (commiteado `40cfbd1`): `AnimaController.PickBest()`
  no detectaba un `IBrain` destruido (comparaba `b == null` por el tipo interfaz, no
  `UnityEngine.Object`) → `MissingReferenceException` en bucle infinito ~8s después de cualquier
  `HelpRequest` aceptada. Fix: cast a `Object` en el null-check. Detalle en sección 11.
- **Bug real encontrado y arreglado en PR #17** (commiteado `6ef53f9`):
  `GuidedTour`/`Assets/Scripts/Kitchen/GuidedTour.cs` dejaba al anfitrión **atascado para siempre en la
  primera estación** — el `stopDistance` por defecto de `FollowBrain` (2) es mayor que el
  `arriveDistance` del paseo (1.5), así que el anfitrión se paraba en una "zona muerta" (~1.65 unidades)
  y nunca disparaba el `Advance()`. Fix: `GuidedTour.StartTour()` ahora fija
  `_walk.stopDistance = arriveDistance * 0.5f` al crear el `FollowBrain` del paseo. Re-testeado: el
  paseo completa las 4 estaciones y termina correctamente. Detalle en sección 12.
- La **Cocina legacy** (`KitchenScaleController`/`KitchenEntrance`, miniaturización + trigger) fue
  **borrada por el equipo el 2026-07-23** — reemplazada por `VirtualizationMachine`+
  `RealityShiftController`+`MobWorldLoader` (genérico, ver `CLAUDE.md`). Un plan mío pendiente de una
  sesión anterior (`encapsulated-wondering-panda.md`, generalizar Kitchen a 5 áreas más vía
  `AreaCombatManager`/`AreaEntrance`) **queda obsoleto** — apuntaba a los archivos borrados. No se
  ejecutó. Si el usuario todavía quiere mobs+combate en las 5 áreas restantes, hay que rediseñarlo
  contra la arquitectura nueva (`VirtualizationMachine`/`RealityShiftController`), no contra la vieja.
- **Dos errores de compilación** encontrados y arreglados hasta ahora, mismo patrón ambos (falta
  `using UnityEngine;`): `Assets/Scripts/Avatar/RobotAvatar.cs` (usaba `[Tooltip(...)]`) y
  `Assets/Scripts/Mind/MindPhrase.cs` (usaba `Random.Range`). Ya sincronizados.
- **MCP de Unity: progreso parcial (2026-08-05), sigue sin poder usarse desde acá**. Las herramientas
  `mcp__unityMCP__*` aparecieron disponibles por primera vez esta sesión. Seguí el procedimiento
  (`Window > MCP for Unity > Toggle MCP Window` → `Start Server`): el botón se quedó colgado en
  "Starting…" — el puerto 8080 ya estaba ocupado por un proceso zombi. Encontré el comando de lanzamiento
  manual en el panel ("Manual Server Launch") y lo corrí por terminal — falló por el mismo conflicto de
  puerto, pero confirmó que SÍ hay un servidor real escuchando en 8080. Volví al panel de Unity, apareció
  un botón "Connect" nuevo, lo usé, y el panel pasó a mostrar **"Session Active (COLD-SANCTUARY)"** — el
  lado de Unity quedó conectado. Pero **mis propias llamadas a herramientas (`find_gameobjects`,
  `manage_scene get_active`, etc.) siguen devolviendo "No Unity Editor instances found"** incluso después
  de esperar y reintentar — parece un problema de reconexión específico de este cliente/sesión de
  herramientas, no de Unity ni del servidor. Seguí testeando todo por captura de pantalla como siempre.
  Si en una sesión futura las herramientas MCP responden de entrada (sin este baile), usarlas directo
  sería mucho más confiable que las capturas — vale la pena reintentar `mcp__unityMCP__manage_scene`
  (`action: get_active`) al principio de cada ciclo antes de asumir que hay que usar computer-use.
- **La Console del Editor no pinta filas de tipo Info de forma fiable** (bug de UI, no del juego — visto
  varias veces esta sesión). Cuando pase: leer directo
  `C:\Users\Blein\COLD-SANCTUARY\Logs\Editor.log` (ruta nueva de Unity 6.5, ya no
  `%LOCALAPPDATA%\Unity\Editor\Editor.log`) con `grep` — mucho más confiable que pelear con la UI.
- **Bug de aptitudes de compañeros: ARREGLADO por el compañero de equipo** (commit `95fbbc0`,
  `MigrationDiagnostics` ahora vuelca en `Update()` en vez de `Start()`) y **re-testeado y confirmado
  por mí** — ver sección 10. Ya no es un bug pendiente.
- **Sección 3 (Farming) prácticamente completa**: Suave/Media/Salvaje confirmados con evidencia de
  `Editor.log`. Atrapada de Dura confirmada (y descartado que sea el bug de corrutinas de
  `Escape()`/`Fight()`). Solo falta la rama de daño de Dura ("pierde el control") — necesita juego
  real con WASD, no teletransportar el Transform vía Inspector.
- **Sección 5 (Trepar) inconclusa**: el jugador SÍ tiene `PlayerClimber` bien configurado (falsa alarma
  mía al no scrollear el Inspector hasta el final), pero mantener/repetir Espacio con la
  automatización no logró subirlo — sospecho que `Input.GetKey` (mantenida) no se sostiene bien vía
  las herramientas de automatización, a diferencia de `Input.GetKeyDown` (un toque) que sí funciona
  bien con `V`. Necesita prueba manual real.
- **Sección 6 (Cocina→Mesopotamia) confirmada en lo esencial**: entrada por la máquina de
  virtualización, carga aditiva de `MobWorld_Mesopotamia.unity`, y `[Meditación] 1/4 resuelto.`
  confirmados. Falta completar las 4 esferas + salir por `YogaPortal` (necesita jugar manual, son 4
  puntos separados en el espacio).
- **Sección 8/9 (Mind) confirmadas en lo esencial**: frases por tono, pools (`Total=28`), reparto
  libre (vivencia de Ötzi vista repartida a `Anima_Roca`) — todo con evidencia de log. Sin probar:
  `ThoughtField_Agua` (requiere mover una ánima físicamente) y la relación poder-mental→longitud.
- **Sección 4 (Progresión) confirmada con level-up real**: serené Suave→Media→Dura en secuencia,
  crucé el umbral de 100xp, `Stats` subió a nivel 2, HUD y log coinciden exacto (`Vida 135, Energía
  131, Maná 57`). Sin probar: marga de Yoga (necesita completar una misión de yoga) y Vínculos
  (huérfano, documentado como pendiente).
- **Sección 7 (Regresión) confirmada**: 0 excepciones en toda la sesión, animales/compañeros/economía
  funcionando. Asanas sin probar directamente (sin errores relacionados tampoco) — confianza media.
- **CHECKLIST CASI COMPLETO.** Lo que queda, todo requiere jugar manualmente (no más teletransportes
  de Inspector): rama de daño de Dura, trepar (mantener Espacio), completar las 4 esferas de
  Mesopotamia + salir por YogaPortal, misión de yoga, ThoughtField_Agua. Considerar esta sesión de
  testing automatizado sustancialmente terminada — lo que sigue es una pasada de juego manual.

## 0. Preparación
- [x] El proyecto **compila** (0 errores CS tras el fix de `RobotAvatar.cs`).
- [x] `Tools → Cold Sanctuary → Build Sample Scene Blockout` corre sin excepciones (genera
      `MobWorldDirector`, `SanctuaryEconomy_AUTO`, `FarmingSandbox_AUTO`, escena `MobWorld_Mesopotamia`
      en Build Settings).
- [x] Al entrar en Play no hay errores rojos en la Console (solo el ya conocido `WebSocketException` de
      conexión a Unity Cloud, ajeno al proyecto); el jugador (`Player`) existe.
      Movimiento del jugador **aún no probado**.

## 1. HUD (esquina sup. izq., OnGUI prototipo)
- [x] Muestra el santuario ("Santuario Terrestre") y sus **recursos** (Food/Materials/Research).
- [x] Muestra las **margas del alma** de Kushal: Stats / Yoga / Vínculos (niveles + XP de Stats).
- [x] Muestra **Vida / Energía / Maná / Def / Poder** — valores iniciales confirmados: **Vida 115/115,
      Energía 117/117, Def 16** (coincide con lo esperado).
- [x] El **maná aparece como "(bloqueado — practica yoga)"** al inicio.

## 2. Recursos de santuario (economía pasiva)
- [x] Los recursos **suben solos** con el tiempo — confirmado en Play mode: Food 21→350+, Materials
      17→280+, Research 11→180+ solo dejando correr el tiempo (~2h de sesión).
- [ ] Al **subir la velocidad de tiempo** (`TimeController`/`TimeTest` si está), suben más rápido
      (escala Meso-lento / Macro-rápido). **Aún no probado.**

## 3. Farming — jugar / serenar / cuidar (sandbox: 4 cápsulas + reglas)
Cápsulas cerca del origen: **Suave**, **Media**, **Dura** (criadas) y **Salvaje** (no criada).
- [x] Acércate a **Suave** y pulsa **V** repetidamente: baja su "tensión" → **color rojo→verde** —
      confirmado, y también se mueve/gira hacia el jugador al excitarse.
- [x] Al llegar a serena (verde): **suelta recursos + monedas**, da **XP** y a veces **items** —
      confirmado leyendo `Logs/Editor.log` (la Console del editor sigue sin pintar Info de forma
      fiable):
      `[Farming] «PlayCreature_Suave» serena — descargó toda su tensión. Suelta recompensa (15 Food + 2
      monedas).` seguido de `[Farming] Botín: 1× Golosina.` El HUD también subió de **Stats xp 0/100 →
      20/100** en el mismo momento. Coincide exactamente con lo esperado.
- [x] Ya serena, con **F**/clic → queda **descansando** (color azul/violáceo, visualmente confirmado) —
      `Logs/Editor.log`: `[Farming] «PlayCreature_Suave» saciada y descansando.` Coincide.
- [x] **Se acerca/gira al excitarse**: confirmado con Suave/Media — el capsule gira hacia el jugador y
      se acerca (leash respetado). No se probó explícitamente el "rebota/crece" visual del combo.
- [x] **Atrapada**: confirmado con Dura, quedándome quieto pegado a propósito — dispara `GetCaught()`
      (`PlayableCreature.cs:186-209`) repetidamente cada `catchGrace` (1s) mientras el jugador no se
      aleje, con log `[Farming] «PlayCreature_Dura» te atrapó jugando — ¡retrocede y vuelve a
      provocarla!` cada vez y reseteo de excitación. **Leí el código para confirmar que no es el mismo
      bug de corrutinas-en-cascada de `Escape()`/`Fight()` de antes** — acá cada disparo es autónomo
      (sin acumular corrutinas), así que repetirse mientras el jugador se queda quieto es el
      comportamiento **diseñado a propósito** (el propio mensaje te dice "retrocede"), no un bug. El
      salto de retirada de la criatura es de solo 0.5u (`catchRange` default 1.2u) — a propósito, para
      que la solución sea que el JUGADOR se aleje, no la criatura.
- [ ] **Dura** (peligrosa) — **rama de "pierde el control" aún no disparada, dos intentos**:
      1. Spam de `V` quieto pegado a Dura → solo activó la rama segura (`GetCaught()` → "te atrapó"),
         nunca la rama de daño (`canLoseControl && _excitement >= loseControlAbove`, línea 189) porque
         quedarse quieto resetea la excitación a 0 en cada atrapada antes de llegar al umbral de riesgo.
      2. Reposicioné al jugador a 2u de Dura (`playRange=3` permite jugar sin entrar en `catchRange=1.2`
         — código en `PlayController.cs:19`, así debería poder acumular excitación sin que dispare la
         atrapada) y probé `V` de nuevo sin moverme — **esta vez no generó NINGÚN log nuevo** (ni
         atrapada, ni daño, ni descarga), lo cual en sí es sospechoso pero no until confirmado como bug:
         puede ser que a esa distancia particular el objetivo más cercano de `FindNearestPlayable`
         cambiara, o que el teletransporte instantáneo (en vez de acercarse caminando) descolocara algo
         del tracking de posición/objetivo.
      **Conclusión**: esta rama concreta necesita una pasada de juego real (moverse con WASD, no
      teletransportar la posición del Transform), no la puedo confirmar de forma confiable con
      teletransportes instantáneos vía Inspector. Dejar pendiente para una prueba manual.
- [x] **Salvaje** (no criada): **`V` no hace nada** — confirmado, 10 pulsaciones de `V` parado justo al
      lado sin ningún efecto (ni tensión, ni XP, ni log). `grep "Salvaje" Logs/Editor.log` solo
      encuentra la línea de setup del sandbox, cero interacciones de `[Farming]`. Gateo por bond/estado
      funcionando como se espera.

## 4. Progresión — margas y puntos del alma
- [x] **Serenar criaturas sube la marga de Stats — CONFIRMADO CON LEVEL-UP REAL**: serené Suave, Media y
      Dura en secuencia (xp 0→20→55→100+); al cruzar el umbral, HUD pasó a `Stats 2 (xp 15/130)` y
      `Logs/Editor.log`: `[Marga] «Player» Stats nivel 2 — Vida 135, Energía 131, Maná 57.` HUD
      confirma exacto: `Vida 135/135, Energía 131/131, Poder 1.1` (subió de 1.0). Coincide 100% con lo
      esperado.
- [ ] **Misión de yoga → marga de Yoga / maná desbloqueado**: no probado (requeriría completar una
      misión de yoga vía la máquina de virtualización de la Sala de Yoga — no llegué a esa parte).
- [x] **Base-bump — indicio razonable**: Vida subió 115→135 (+17.4%) y Poder 1.0→1.1 en un solo nivel;
      no verifiqué la fórmula exacta línea por línea, pero el salto es coherente con que la base de
      aptitudes también subió (`baseBumpPerLevel=0.02`), no solo el factor de nivel.
- [ ] **Todas las margas suben los puntos del alma**: solo probé Stats; Yoga/Vínculos sin probar (Yoga
      depende del punto anterior sin probar; Vínculos está documentado como huérfano — ver notas al
      final del archivo).

## 5. Trepar (MVP — verificar feel/física)
- [ ] ⚠️ **Inconcluso — necesita prueba manual real, no automatizada**: junto al `ClimbTree` (a 1u de
      distancia, dentro de `detectRange=2`), mantener/repetir `Espacio` (probé `hold_key` 3s y 30
      pulsaciones rápidas) **no subió al jugador ni un solo frame** (Position.y quieto en 1.08,
      Energía intacta en 117/117 — ninguna señal de que `BeginClimb()` haya corrido siquiera una vez).
      Verifiqué TODAS las precondiciones del código (`PlayerClimber.cs`) antes de descartarlo como bug:
      - `PlayerClimber` **sí está** en `Player` (falsa alarma mía al principio — no había scrolleado
        lo suficiente en el Inspector; el componente existe con `climbKey=Space`, `detectRange=2`,
        `minStrengthToClimb=0.8`).
      - `CharacterLevel.aptitudes.strength = 1.2` (perfil de Kushal seteado por el builder) > umbral 0.8.
      - Jugador a 1u del `ClimbTree`, bien dentro de `detectRange=2`.
      **Mi sospecha**: `PlayController` (tecla V) usa `Input.GetKeyDown` (un solo frame "recién
      presionado") que mi automatización simula bien con pulsaciones discretas — por eso V funcionó
      siempre. `PlayerClimber` usa `Input.GetKey` (mantenida) en cambio, que necesita que el SO reporte
      la tecla abajo de forma continua durante varios frames seguidos — sospecho que mi herramienta de
      automatización no logra sostener eso de forma que Unity lo reconozca, más que un bug real del
      juego. **No lo puedo confirmar ni descartar sin que alguien lo prenda y mantenga Espacio con la
      mano.** (Aparte: `Build Sample Scene Blockout` corrido dos veces esta sesión, sin re-detectar el
      problema — los componentes del jugador para Farming/Trepar SIEMPRE estuvieron bien puestos, ese
      susto fue enteramente mío por no scrollear el Inspector hasta el final.)
- [ ] Subir **gasta energía** (baja Energía en el HUD); al agotarse, **deja de subir**. (No probado —
      depende de lo anterior.)
- [ ] No pasa del **tope del árbol** (`Climbable.topY`). (No probado — depende de lo anterior.)
- [ ] Al **soltar Espacio**, el `CharacterController` se reactiva y el jugador **cae/queda** con
      gravedad normal (no se queda flotando ni se rompe el movimiento).
- [ ] *(Tuning esperado:* velocidad/altura pueden necesitar ajuste — anota si se siente raro.)

## 6. Cocina → Microcosmos (mundo mob jugable)
- [x] En el área **Cocina**, interactúa con la **máquina de virtualización** (F/clic) → confirmación
      ("¿Entrar a la simulación de Cocina?") → **Entrar** → confirmado por `Logs/Editor.log`:
      `Loaded scene 'Assets/Scenes/MobWorld_Mesopotamia.unity'` — la escena carga **de forma aditiva**
      (aparece como raíz separada `MobWorld_Mesopotamia` en el Hierarchy, junto a `SampleScene`, no
      reemplaza la escena). El jugador se teletransporta a coordenadas lejanas (~5000,1,4994) dentro de
      ese espacio — no vi el fundido a negro en sí (no práctico de confirmar por captura), pero el
      resultado final es correcto.
- [x] Hay una **misión "Procesar ingredientes"** (`Mission_ProcesarIngredientes`, con descripción
      "Acércate a cada ingrediente y mantente presente hasta procesarlo" — coincide). Acercándome a una
      esfera confirmé `[Meditación] 1/4 resuelto.` en el log — mecánica funciona. No completé las 4 (son
      4 esferas separadas en el espacio, requeriría caminar/teletransportar a cada una — no práctico en
      esta pasada).
- [ ] **Completar la misión + salir por `YogaPortal`**: no probado (depende de completar las 4 esferas
      arriba). Pendiente para una pasada dedicada, jugando manualmente.
- [ ] (Si sale aviso "no se pudo cargar 'MobWorld_Mesopotamia'": correr antes
      `Tools → Cold Sanctuary → Build MobWorld Mesopotamia`.)

## 7. Regresión — que lo previo NO se rompa (tras IAptitudes)
- [x] Los **animales** siguen apareciendo y funcionando: `[Diag]` los lista (Bunny/Wolf/Fox/Deer cubs)
      con `IAptitudes` bien cableado, moviéndose (avistados en Scene view en varias capturas de esta
      sesión), y **0 `NullReferenceException`/`MissingReferenceException` en todo `Editor.log`** pese a
      horas de Play acumuladas — sin señales de rotura.
- [x] Los **compañeros** (`CompanionBase`) funcionan correctamente: los 4 (Goluis/Irosene/Panterilia/
      Gohageneis) aparecen en el diagnóstico con sus aptitudes de perfil exactas tras el fix de la
      sección 10 — confirma que `Start()` (bonds/mood incluidos) corre sin errores por `IAptitudes`.
- [~] Las **asanas/yoga**: no probadas directamente jugando (no entré a la Sala de Yoga a hacer una
      postura), pero tampoco apareció ningún error relacionado con `Asana`/`PostNatalManager`/similares
      en ninguna de las revisiones de log de esta sesión. Confianza media, no alta.
- [x] La **economía de monedas** sigue funcionando: confirmada en vivo — Suave dio "2 monedas", Media
      "4 monedas" al serenarse (`[Farming] … Suelta recompensa …`), sumado a recursos de santuario
      (Food/Materials/Research) subiendo sin parar durante toda la sesión.

## 8. Mind MVP — frases por tono elemental (docs/anima-architecture.md)
Sandbox `MindSandbox_AUTO`: 3 cápsulas con `Mind` + aptitudes distintas (Anima_Roca, Anima_Fuego, Anima_Viento).
- [x] En Play, la Console suelta cada ~4 s frases tipo `[Mente] «Anima_Roca» (Tierra+): "…"` — confirmado
      leyendo `Logs/Editor.log` (decenas de líneas `[Mente]` con timestamps espaciados).
- [x] **Cada ánima tiende a su tono**: confirmado por conteo — `Anima_Roca` con tono Tierra+ 697 veces,
      `Anima_Fuego` con Fuego+ 565 veces, `Anima_Viento` con Viento+ 633 veces (dominante, con destellos
      de otros tonos mezclados en el resto de las líneas).
- [ ] **Poder mental → longitud** y **valencia/humores/glucosa**: no verificado en detalle (requeriría
      comparar longitud de frase por ánima y seguir humores en el tiempo — no crítico, dejar para una
      pasada de balance más adelante).

## 9. Mind — campos, pools de frases y reparto (docs/anima-architecture.md §11)
Mismo sandbox `MindSandbox_AUTO` (ahora con un `ThoughtField_Agua`) + logs `[Frases]` en la Console.
- [ ] **Campo de pensamiento** (`ThoughtField_Agua`): no verificado — requiere mover físicamente una
      ánima dentro/fuera del radio (~6u en (-1,10)) y comparar tono antes/después, no práctico por
      teletransporte instantáneo en esta sesión.
- [x] **Pools cargadas**: confirmado exacto — `[Frases] Total=28 | Elemental=4 Vivencia=18 Deseo=6` y
      líneas por biografía `Goluis: 3`, `Panterilia: 4`, `Gohageneis: 3`, `Irosene: 4`. Coincide con lo
      esperado.
- [ ] **Reparto Estricta**: no verificado directamente (el sandbox usa reparto libre por defecto,
      aparentemente — ver abajo).
- [x] **Reparto Libre — indicio fuerte de que funciona**: encontré la vivencia de Ötzi
      (`"Comprender mi muerte me libera."`, tono Agua) repartida a `Anima_Roca` en el log — coincide con
      "los compañeros/anónimo reciben vivencias barajadas del pool público". No confirmé explícitamente
      que Magnate/Ötzi conserven lo suyo en otro contexto (fuera de alcance de este sandbox).
- [x] **Vivencias fieles**: la frase de Ötzi vista (`"Comprender mi muerte me libera."`) coincide
      textualmente con el ejemplo del checklist.

## 10. Migración a `Anima` — validación por consola (PR #14, recién mergeada)
`MigrationDiagnostics_AUTO` vuelca un bloque `[Diag Anima]` al entrar en Play. Confirma en Console:
- [x] **Compila** tras el renombrado `LivingEntity → Anima` + `CompanionBase`/`PlayerStats : Anima` — **un
      error encontrado y arreglado**: faltaba `using UnityEngine;` en `Assets/Scripts/Mind/MindPhrase.cs`
      (usaba `Random.Range` sin el using — mismo patrón que el bug de `RobotAvatar.cs` de la sesión
      anterior). Ya sincronizado, 0 errores CS tras el fix.
- [x] `[Diag] CompanionBase hereda de Anima: True` y `Animal hereda de Anima: True` — confirmado, leído
      directo de `C:\Users\Blein\COLD-SANCTUARY\Logs\Editor.log` (la Console del editor no estaba
      pintando filas de tipo Info de forma fiable esta sesión — bug de UI ya visto antes, no del juego).
- [x] Lista de **Anima en escena**: 10 total (4 compañeros + 5 crías de animales + Player). `apt(str/agi/rea)`
      **sí coincide** con `IAptitudes(str)` en todos los casos (ambos dan 1.00) — la vía de acceso está
      bien cableada.
- [x] **CONFIRMADO ARREGLADO** (re-test tras el fix de `MigrationDiagnostics` a `Update()`): los 4
      compañeros ahora muestran sus aptitudes de perfil correctas —
      `Goluis_Post str=1.50 agi=0.90 rea=0.70`, `Irosene_Post str=1.20 agi=1.20 rea=1.10`,
      `Panterilia_Post str=0.70 agi=0.95 rea=1.60`, `Gohageneis_Post str=1.10 agi=1.20 rea=1.00` —
      coincide exactamente con lo esperado. Cerrado.
- [x] ~~Falso positivo del diagnóstico~~ (histórico, resuelto): los compañeros salían con aptitudes default
      por una **carrera de `Start()`** (el diagnóstico leía antes que `CompanionBase.Start()` fijara el
      perfil). Arreglado moviendo `MigrationDiagnostics` al **primer `Update()`** (commit `95fbbc0`);
      re-test confirmó los valores de perfil (ver ítem anterior).
- [x] Línea de **Kushal**: `margas Stats L1/Yoga L1/Vínc L1 · Vida 115 Energía 117 Maná 50 Def 16 ·
      manáDesbloqueado=False` — coherente con lo esperado (HUD ya lo confirmaba en §1).
- [ ] **Regresión** (§7): pendiente de probar en profundidad (animales/compañeros se ven en la lista y no
      hay errores, pero no se verificó comportamiento en Play más allá de eso).
- [ ] **Cambio de comportamiento conocido** (`physicalResistance=1` en NPCs): aún no probado.

## 11. Control/posesión · Cocina · Virtualización (PRs #16 y #17)
Sandboxes que genera `Build Sample Scene Blockout`. Todo por consola.

### PR #16 — verificado 2026-07-29 (`PossessionSandbox_AUTO`, `KitchenSandbox_AUTO`)
Evidencia de `Logs/Editor.log` de una corrida completa en Play:
- [x] **Compila** tras sincronizar los 17 archivos nuevos/modificados del PR (`Control/`, `Kitchen/`,
      extensiones de `Mind`, `SampleSceneBuilder.cs`) — 0 errores CS.
- [x] **Posesión por relevancia**: `Anima_Debil` (selfRelevance 1) queda conducida por **Jugador**
      (posesión power 2 > 1) — `[Control] «Anima_Debil» ahora conducido por: Jugador (relevancia 2,00)`.
      `Anima_Fuerte` (selfRelevance 3) **resiste** la misma posesión (2 < 3) — se mantiene en `IA`, sin
      log de cambio a Jugador. Coincide exactamente con el diseño.
- [x] **`FollowBrain`**: `Kushal_Follow` queda conducido por `IA (seguir)` seleccionando el target correcto.
- [x] **Petición → alma compartida (`HelpRequest`/`HelpResponder`)**: `Aldeano_Pide` pide ir juntos,
      `Kushal_Follow` responde SÍ, comparten alma 8s (`FollowBrain` temporal relevancia 5), y al expirar
      **retoma su propia mente** — `[Petición] Fin del alma compartida; «Kushal_Follow» retoma su propia
      mente.` seguido de `[Control] «Kushal_Follow» ahora conducido por: IA (seguir) (relevancia 1,50)`.
- [x] **Cocina paso A**: `KitchenDirtArea` genera manchas; al llegar a 5/5 activa la misión
      (`[Cocina] ¡Suciedad sobre el umbral (5/5)! Misión de limpieza ACTIVA`); `Pinche_Limpia` (`Cleaner`
      auto) las limpia una a una sin parar (30+ `[Cocina] Mancha «Dirt_N» limpiada.` observadas). El
      sandbox regenera suciedad continuamente (por diseño, demo infinita) así que no se ejercitó el
      camino de "misión completa al vaciarse" — no es un bug, solo no aplica en este sandbox.
- [x] **BUG REAL encontrado y arreglado**: `AnimaController.PickBest()` (`Assets/Scripts/Control/
      AnimaController.cs`) comparaba `if (b == null) continue;` sobre una variable tipada `IBrain`
      (interfaz). Esa comparación usa el tipo estático `IBrain`, no `UnityEngine.Object` — así que
      **no detecta un `MonoBehaviour` ya destruido** (Unity solo sobrecarga `==` en `Object`). Cuando
      `HelpRequest.EndShare()` hace `Destroy(shared)` sobre el `FollowBrain` temporal, `PickBest()` lo
      seguía eligiendo como `_active` y `_active.Act(this)` reventaba con `MissingReferenceException`
      **en bucle infinito** (999+ errores/frame, todos los frames desde ese punto) ~8s después de
      cualquier petición aceptada — bloqueaba efectivamente el Play mode. **Fix aplicado**:
      `if ((b as Object) == null) continue;` (cast a `Object` para activar el chequeo "fake null" de
      Unity). Sincronizado repo↔proyecto, recompilado, re-testeado: corrida completa post-petición sin
      ningún error nuevo. **Commiteado** (`40cfbd1`).

### PR #17 — verificado 2026-07-29 (ver detalle completo en sección 12 más abajo)
- [x] **Control/posesión** (`PossessionSandbox_AUTO`): re-confirmado junto con PR #16, sigue funcionando
      igual tras el merge de #17 (sin regresión).
- [x] **Cocina paso A** (`KitchenSandbox_AUTO`): sigue funcionando igual, sin regresión.
- [x] **Cocina paseo + desayuno** (`KitchenOnboarding_AUTO`): probado — encontrado y arreglado un bug real
      (`GuidedTour` atascado en la 1ª estación, ver sección 12); tras el fix completa las 4 estaciones.
      Loop de desayuno (`Cocinero`/`Comensal`) confirmado con decenas de ciclos limpios.
- [~] **Virtualización — cocina/huerto** (mira+apuntar+confirmar, mecanografía del fogón): las estaciones y
      la receta se construyen sin errores y no tiran excepciones corriendo sin input, pero el flujo de
      apuntar/confirmar en sí **requiere juego manual real** (no reproducible por automatización, mismo
      caso que trepar) — pendiente de una pasada jugando.
- [x] **Regresión**: 0 excepciones nuevas en toda la corrida de PR #17.

## 12. Cocina paseo+desayuno (paso B) + motor de Virtualización (PR #17, mergeada 2026-07-29)
`KitchenOnboarding_AUTO` ([Paseo]/[Cocina]), `VirtualizationSandbox_AUTO` ([Virtual]/[Producción]),
`GardenVirtualization_AUTO` (misma mecánica en el Huerto). Evidencia de `Logs/Editor.log`:
- [x] **Compila** tras sincronizar los 13 archivos del PR (`Kitchen/` paso B, `Virtualization/` completo,
      `SampleSceneBuilder.cs`, `PlayerController.cs`) — 0 errores CS.
- [x] **BUG REAL encontrado y arreglado**: paseo guiado (`GuidedTour`) atascado permanentemente en la
      primera estación por un desajuste de umbrales (`FollowBrain.stopDistance`=2 > `arriveDistance`=1.5).
      Ver detalle y fix en "Estado de sesión" arriba. Confirmado con posición exacta del `Anfitrion`
      estancada en (9.6, 1, 15.6) — 1.65u de la Nevera, fuera del radio de "llegada" — durante minutos de
      tiempo de juego sin avanzar, y confirmado resuelto tras el fix: `[Paseo] «Anfitrion» enseña
      «Nevera»/«Plancha»/«Mesones»/«Contenedor»` seguido de `[Paseo] Fin del paseo.` en una sola pasada.
- [x] **Loop de desayuno (paso B)**: `Cocinero`/`BreakfastCook` corre el ciclo completo sin parar
      (nevera→toma huevos→plancha→revuelve→especia→contenedor) y `Comensal`/`Eater` come del
      `FoodContainer` cada vez que se rellena — decenas de ciclos limpios observados, sin excepciones.
- [~] **Motor de virtualización (`VirtualPointer`/`StationPart`/`ProductionOrder`/`TypingChallenge`,
      Kitchen y Garden)**: las estaciones y la receta se construyen sin errores (`ProductionOrder`
      creada con 7 pasos en Kitchen, 9 en Garden), pero **requiere input real** (flechas/ratón/touch +
      confirmar) para ejercitar el flujo — mismo tipo de limitación que trepar (`Input.GetKey`
      sostenido no se reproduce bien vía automatización). No se detectaron excepciones al dejarlo
      correr sin input. **Pendiente de una pasada de juego manual** para confirmar el flujo completo
      (apuntar en orden → confirmar → 3 desayunos/cosechas → misión cumplida).

## 13. Mecánica · Construcción · Dispatch/reparación (PR #18, mergeada 2026-07-30)
Sandboxes de `Build Sample Scene Blockout`. Se apuntan con la **mira central** (girar cámara) + **F**, o
ratón/touch. Evidencia de `Logs/Editor.log` de una corrida en Play:
- [x] **Compila** tras sincronizar los 12 archivos del PR (`Virtualization/` nuevo: `RepairTicket`,
      `ServiceHub`, `StockingTask`, `Toolbox`, `VirtualTask`; + extensiones y `SampleSceneBuilder.cs`) —
      0 errores CS.
- [x] **Los 4 sandboxes nuevos se construyen sin excepciones**: `ForgeVirtualization_AUTO`,
      `MechanicsBeginner_AUTO`, `ConstructionBeginner_AUTO`, `DispatchDemo_AUTO` — confirmado por sus
      logs `[SampleSceneBuilder]` de creación, uno por sandbox.
- [x] **Sub-paso "limpiar" (automático, reutiliza `DirtArea`/`Cleaner`)**: tanto en Mecánica
      (`Taller_Suciedad`) como en Construcción (`Solar_Escombros`) la suciedad sube hasta el umbral y
      activa la misión — `[Cocina] ¡Suciedad sobre el umbral (4/4)! Misión de limpieza ACTIVA en
      «Taller_Suciedad»/«Solar_Escombros»`. Mismo patrón que Cocina, ya probado, sin sorpresas.
- [x] **Dispatch — listado inicial del ticket**: al entrar en Play, `[Servicio]` lista correctamente
      el ticket pendiente — `[Servicio] «Taller (Mecánica/Construcción)»: 1 ticket(s) de avería. Toma
      herramientas y ve al área a reparar.` seguido de `[Servicio]  · grifo que gotea en Cocina`.
- [x] **Regresión**: 0 excepciones nuevas en toda la corrida (`NullReferenceException`/
      `MissingReferenceException`/`error CS` — ninguna desde que arrancaron los sandboxes de PR #18).
- [~] **Abastecer/reparar/construir/forjar (los pasos reales de cada flujo)**: igual que el motor de
      Virtualización de PR #17, estos requieren **apuntar con la mira + confirmar** (input real de
      teclado/ratón/touch) — no se pudieron ejercitar por automatización. **Pendiente de una pasada de
      juego manual** siguiendo el guion ya escrito arriba (StockingTask en Mecánica/Construcción,
      receta del grifo con herramientas en Dispatch, receta de bronce con mecanografía en la Forja).

## 14. Camión de mantenimiento · Prólogo · Cría (PR #19, mergeada 2026-07-30)
Sandboxes de `Build Sample Scene Blockout`: `TruckMaintenance_AUTO`, `PrologueSandbox_AUTO`,
`CriaBeginner_AUTO`. Evidencia de `Logs/Editor.log`:
- [x] **Compila** tras sincronizar los 7 archivos del PR (`Assets/Scripts/Prologue/` nuevo:
      `CarryToRefuge`, `CriaCareTarget`, `PlaneMessenger`, `PrologueSequence`, `WeakOne`;
      + `SampleSceneBuilder.cs`/`PhrasePools.cs`) — 0 errores CS.
- [x] **BUG REAL encontrado y arreglado**: ver detalle completo en "Estado de sesión" arriba
      (`CarryToRefuge.onComplete` sin inicializar → `NullReferenceException` que abortaba
      `SampleSceneBuilder.Build()` a mitad de camino, dejando sin crear Cría/Construcción/Dispatch/
      diagnóstico/NavMesh). Confirmado resuelto: el `Build()` ahora termina completo (`Blockout listo`)
      con los 3 sandboxes nuevos presentes.
- [x] **Prólogo (`PrologueSequence`, `autoDemo=true`) avanza sus 5 beats solo, por tiempo**:
      `[Prólogo] 1/5` → `2/5` → `3/5` → `4/5` → `5/5` → `[Prólogo] Fin del prólogo → a su primer trabajo
      (la Cocina).` — guion completo sin intervención, tal como está diseñado para la demo.
- [x] **`CarryToRefuge`/`WeakOne` (rutina de "llevar a los débiles a salvo")**: ambas instancias
      funcionan correctamente y de forma independiente — el `Nido_Calido` de la Cría (`needed=1`) se
      completa con `Cria_Bebe` (`[Cuidado] «Cria_Bebe» a salvo en el refugio (1/1)`), y la
      `Cueva_Refugio` del prólogo (`needed=2`) se completa con ambos `Debil_0`/`Debil_1`
      (`(1/2)` → `(2/2)` → `Todos los débiles a salvo en la cueva.`). Sin cruce entre ambas (están lo
      bastante separadas en el mapa) — los conteos dan exactos.
- [~] **Aviso cruzado de `PlaneMessenger` no se dispara**: ver "Hallazgo adicional" en Estado de sesión
      arriba — no es un crash, solo una funcionalidad silenciosamente inerte en este sandbox concreto.
- [~] **Camión de mantenimiento (`TruckMaintenance_AUTO`)**: se construye sin errores (recetas de
      cambio de rueda/aceite/agua, `ProductionOrder` con 6/2/1 pasos respectivamente), pero — igual que
      el resto del motor de Virtualización — el flujo real requiere apuntar+confirmar con input manual.
      Sin excepciones dejándolo correr sin input.
- [~] **Cría — abastecer/rutina de cuidado (`CriaBeginner_AUTO`)**: la parte automática (limpiar el
      nido, `DirtArea`/`Cleaner`) funciona igual que en Cocina/Mecánica/Construcción. El abastecer
      (`StockingTask`) y la rutina de cuidado en sí (leer estado→calmar→alimentar→asear→arrullar)
      requieren input manual — pendiente de una pasada jugando. `CriaCareTarget` está enganchado como
      placeholder (el comentario del propio código: "sin Animal → solo registra") — revisar si ya
      corresponde conectarlo a un `Animal`/`Anima` real o si sigue siendo intencionalmente un stub.
- [x] **Regresión**: 0 excepciones nuevas en toda la corrida tras el fix (aparte del hallazgo de
      `PlaneMessenger` ya descripto, que no es una excepción sino un silencio).

## 15. Microcosmos — 1ª misión insecto (PR #20, mergeada 2026-07-30)
`MicrocosmosSandbox_AUTO` ([Micro]/[Cuidado]). Evidencia de `Logs/Editor.log` de una corrida en Play:
- [x] **Compila** tras sincronizar los 4 archivos del PR (`Assets/Scripts/Microcosmos/` nuevo:
      `AphidGuide`, `HoneydewProducer`; + `SampleSceneBuilder.cs`/`PhrasePools.cs`) — 0 errores CS.
- [x] **`Build()` completa sin excepciones** con el nuevo sandbox incluido (no repite el tipo de bug de
      PR #19 — este `CarryToRefuge` no usa `.AddListener()`, así que no dependía del fix anterior).
- [x] **`HoneydewProducer`**: produce melaza cada `interval` (3s) de forma continua y estable — decenas
      de ciclos `[Micro] «Pulgon» (pulgón) produce melaza (N). Las hormigas la ansían.` sin cortes.
- [x] **`AphidGuide` — misión completa de punta a punta, SIN input manual** (a diferencia del motor de
      Virtualización): rescata a las 4 frágiles (`[Micro] «Mayor_tribu»/«Mayor_familiaPulgon_1»/
      «Mayor_familiaPulgon_0»/«SuperMayor» rescatado: ahora sigue al refugio.`), anuncia que guía a la
      familia (`[Micro] El pulgón guía a la familia rescatada hacia el refugio.`), y las 4 llegan a la
      Cueva y completan el `CarryToRefuge` (`[Cuidado] ... (1/4)` → `(2/4)` → `(3/4)` → `(4/4)` →
      `Todos los débiles a salvo en la cueva.`). Sin bugs — este sandbox es autónomo por diseño.
- [x] **Regresión**: 0 excepciones nuevas; los sandboxes previos (Prólogo, Cría) siguen completando
      correctamente en la misma corrida (confirmado en el mismo tramo de log).

## 16. Microcosmos (tableau narrativo) + Upa-yoga (PR #21, mergeada 2026-07-31)
`MicrocosmosSandbox_AUTO` (reescrito) y `UpaYogaSandbox_AUTO`. Evidencia de `Logs/Editor.log`:
- [x] **Compila** tras sincronizar los 3 archivos del PR (`Assets/Scripts/Microcosmos/SoulRecord.cs`,
      `Assets/Scripts/Virtualization/UpaYogaSession.cs` nuevos + `SampleSceneBuilder.cs`) — 0 errores CS.
- [x] **`Build()` completa sin excepciones** con ambos sandboxes.
- [x] **`SoulRecord`/`AddSoul` (tableau del Microcosmos)**: las 8 fichas de alma se registran limpias en
      `Start()` — `[Alma] Hespero · hilo A ...`, `Ruth`, `Ambrosio`, `Medea`, `Momo`, `Atlas`, `Sakshi`,
      `Anciano_Pintor` — sin errores. Es solo dato/lore (no conduce comportamiento), tal como está
      documentado en el propio código.
- [x] **`HoneydewProducer` en `Ambrosio`** (renombrado desde `Pulgon`) sigue funcionando igual que en
      PR #20, ciclos limpios de melaza.
- [~] **`UpaYogaSession`**: arranca solo al entrar en Play y **se ve correctamente en pantalla** — título
      "POSTURA BASE · PIES", paneles-tecla WASD/IJKL, barra de energía/fatiga, indicador "F -> siguiente".
      El avance real de fase (tecla F) requiere **input manual** — no se pudo ejercitar por
      automatización, mismo caso que el resto del motor de Virtualización. Sin excepciones dejándolo
      correr sin input.
- [x] **Regresión**: 0 excepciones nuevas; Prólogo (5/5 beats), Cría (`1/1`) y Cueva del prólogo (`2/2`)
      siguen completando igual que antes en la misma corrida.

## 17. Emoción deep-sim: orquesta + Laban + legibilidad + ScreenEffects + CreatureRig (PRs #22-30, mergeadas 2026-08-05)
`ScreenEffectsSandbox_AUTO`, `EmotionOrchestraSandbox_AUTO` (+ `EmotionReader_AUTO`). Evidencia de una
corrida en Play (captura de pantalla + `Logs/Editor.log`):
- [x] **Compila** tras sincronizar los 12 archivos del batch (`Assets/Scripts/Emotion/` nuevo:
      `BodyPartReactor`, `EmotionExpression`, `EmotionReader`; `Camera/ScreenEffects.cs`,
      `Avatar/CreatureRig.cs` nuevos; + `Anima.cs`, `BodyPosition.cs`, `CameraManager.cs`,
      `PlayerController.cs`, `HeadLook.cs`, `UpaYogaSession.cs`, `SampleSceneBuilder.cs` modificados) —
      0 errores CS. Fue un sync grande de una vez (19 commits, PRs #22 a #30 no sincronizados de un
      gap previo) — sin problemas, se aplicó igual que los batches chicos.
- [x] **`Build()` completa sin excepciones** con ambos sandboxes nuevos incluidos.
- [x] **`ScreenEffects` (`debugAutoCycle=true`)**: confirmado **visualmente** en captura de pantalla —
      la pantalla de juego mostró un tinte rojo/oscuro (viñeta de estrés) que luego pasó a colores
      normales en una captura posterior, confirmando que la oscilación automática de intensidad
      funciona sin jugador real.
- [x] **`EmotionExpression`/`BodyPartReactor`/`EmotionReader` (`debugDrive=true`)**: confirmado
      **visualmente** — el texto de lectura pasó de `"siente: ira/miedo · o dale espacio"` a `"siente:
      serenidad"` entre dos capturas, confirmando que `EmotionReader` lee la orquesta emocional
      (`Quijada`/`Antena`/`Hombro`) y la traduce a texto dinámicamente según los humores oscilantes.
- [x] **Regresión — sin bugs pese a tocar archivos core**: `Anima.cs`, `CameraManager.cs`,
      `PlayerController.cs`, `BodyPosition.cs` y `HeadLook.cs` fueron modificados en este batch, pero
      Prólogo (5/5 beats), Cría/Cueva (`CarryToRefuge` completando) y Microcosmos (`Ambrosio` produciendo
      melaza en loop) siguen funcionando idénticos en la misma corrida — 0 excepciones nuevas.
- [~] **`CreatureRig` (refactor "fuente única de partes", retira `RigPart`)**: no se pudo verificar en
      profundidad más allá de que compila y no rompe nada — es una pieza estructural interna
      (esqueleto→huesos) sin un sandbox propio en `SampleSceneBuilder`; se ejercita indirectamente vía
      `BodyPartReactor`/asanas. Sin evidencia de fallos, pero tampoco un test dedicado.

## 18. Depredación por stats — Predation/MagicAura/TransformationSpell (PRs #31-34, mergeadas 2026-08-05)
Sin sandbox propio en `SampleSceneBuilder` — se integra directo en `Diet.SelectPrey`/
`Animal.EvaluateThreat`, se ejercita con los animales normales de la escena. Evidencia de una corrida:
- [x] **Compila** tras sincronizar los 7 archivos (`Assets/Scripts/Transformation/` nuevo: `MagicAura`,
      `Predation`, `StatProfile`, `TransformationSpell`; + `Animal.cs`/`Diet.cs`/`Anima.cs` modificados)
      — 0 errores CS.
- [x] **Regresión limpia**: 0 excepciones nuevas en toda la corrida; Prólogo/Cría/Cueva/Microcosmos
      (`Ambrosio` produciendo melaza en loop) siguen funcionando idénticos.
- [~] **Sin ejercitar directamente**: no hay logs/UI dedicados a `Predation`/`MagicAura`/
      `TransformationSpell` para confirmar el comportamiento por consola (el diseño usa las mismas rutas
      silenciosas que `Diet.SelectPrey`/`EvaluateThreat` ya tenían). Reconfirma (no crea) el hallazgo ya
      documentado de sesiones anteriores: 0 actividad de `Carnivore.Feed()` observada — el nuevo filtro
      `hunterPower < Predation.Defense(candidateAnima)` en `Diet.SelectPrey` es un filtro ADICIONAL sobre
      el mismo camino que ya casi nunca se disparaba, así que no se puede saber por automatización si
      hoy además bloquea activamente alguna caza que antes sí hubiera pasado. Necesitaría o bien juego
      manual con animales de poder dispar, o pedirle al compañero un sandbox de demo (como los de
      Emoción) para poder confirmarlo por consola.

## 19. Magia · metabolismo · descomposición · quimeras (PRs #35–#54)

**ESTADO:** dos sandboxes `_AUTO` **YA construidos** (PR #54) — se generan con `Build Sample Scene Blockout`:
**`Descomposicion_AUTO`** (minijuego, no necesita `Anima`) y **`Magia_AUTO`** (HUD de prueba del bucle
comer→desbloquear→lanzar, sin `Anima` porque los componentes son null-safe). El resto del arco sigue siendo
código opt-in sin cablear en juego real. Compila verificado por conteo de llaves (no por Unity). Qué verificar:

**Verificado por mí (2026-08-07) — compila de verdad en Unity (0 errores CS tras arreglar
`TransformationSpell.cs`, ver "Estado de sesión") y ambos sandboxes se construyen y corren en Play sin
excepciones**: probé un clic rápido en «Iniciar jornada» de `Descomposicion_AUTO` (sin jugar las 3 fases
de verdad) → terminó limpio con "0 elementos + 0 J" (log `[Descomp-minijuego]`), sin crash. `Magia_AUTO`
construye su HUD sin errores. **Los ítems `[ ]` de abajo (19a-19f) siguen sin verificar en profundidad**
— necesitan jugar cada fase/botón real, no solo confirmar que no crashea.

### 19a. Bucle comer → reservas (`Metabolism`/`Constitution`/`MagicReserves`) — **sandbox `Magia_AUTO`**
El `Magia_AUTO` lleva un **`SimpleAnima`** (Anima concreto mínimo, PR #56) → sí se prueban grasa y stats. HUD
abajo-izq: botones **Aprender 1er hechizo / Comer carne / Comer fruta / quarks / fuego** + lectura de
reservas/energía/quarks **y de stats (fuerza/masa/grasa)**.
- [ ] Comer (`AbsorbFood`) **antes** de desbloquear: **no** llena las pools de magia (lo útil sube stats vía
      `Constitution`, gradual; el exceso → **grasa** `fatReserves`, visible en el HUD).
- [ ] Tras **Aprender 1er hechizo**: el exceso de comer llena las pools (carne→N/H, fruta→C) hasta `capPerElement`,
      y solo lo que aún sobra → grasa.
- [ ] Al comer, **fuerza/masa suben poco a poco** (Constitution converge hacia el objetivo por elementos).
- [ ] Frío (`temperature<37`) y actividad (`exhaustion`) **suben el gasto** (más apetito/antojo de grasa).
- [ ] `Appetite`/`Craving`/`Selectivity` responden a los pools (saciado = selectivo; hambriento = come todo).

### 19b. Primer hechizo desbloquea las pools (`Grimoire`)
- [ ] `Grimoire.Learn("awaken-reserves")` → `MagicReserves.unlocked=true` + siembra H/C/N/O + reserva de energía;
      dispara `OnLearned`. Tras esto, el **exceso de comer llena las pools** (hasta `capPerElement`) y luego grasa.

### 19c. Hechizos: coste materia + energía — **sandbox `Magia_AUTO`** (fuego/agua/tierra/viento)
Botón **«Cargar reservas de prueba (H/C/O/Si +100)»** para tener materia sin depender de comer.
- [ ] `FireSpell` **modo químico** (Chispa/Lanzallamas): cobra combustible (C+H) + ignición (J); sin reservas →
      no sale (log "sin reservas"). Chispa 0,018 g / lanzallamas 22 g.
- [ ] `FireSpell` **modo masa-energía** (Aliento de dragón): cobra ~µg + **toda** la energía del pool (necesita
      restituir energía antes).
- [ ] **Agua** (H₂O): gasta ~11% H + 89% O de 1 kg + ~200 J. **Tierra** (SiO₂): mucha materia (Si+O de 5 kg) +
      poca energía. **Viento**: **materia 0** (aire gratis) + ~450 J (energía-pesado).
- [ ] `TransformationSpell`/`PossessionSpell` con `cost`+`energyCost` → `Pay(costs,energy)` todo-o-nada.
- [ ] Lanzar un hechizo **sube el aura destructiva** (`Predation` lo teme — cruzar con §Animales).
- [ ] **Abastecer/trasplante** (`SupplySpell`, rol healer): botón «Abastecer objetivo» → transfiere energía +
      quarks + C del lanzador al `Magia_AUTO_Objetivo`; el HUD muestra subir la energía/C del objetivo y bajar
      las del lanzador. Sin recursos suficientes → log "sin recursos".

### 19d. Trabajo de descomposición (`DecompositionJob`) — **testable sin `Anima`**
- [ ] Con `workerCut` y **sin** worker/reservas: al `Complete()`, **todo** va a `SanctuaryResources`
      (Elements/Energy) — verlo subir en el **HUD de recursos** (§1/§2).
- [ ] Con worker (reservas unlocked): `workerCut` va a sus pools/energía; el resto a la economía.
- [ ] Por defecto la energía se capta (sin gate). El `energyPhysicsId` es palanca **opcional**: solo si se
      rellena y el `Grimoire` no lo conoce, su parte de energía va entera a la economía (log "energía NO revelada").

### 19e. Minijuego de descomposición (`DecompositionMinigame`) — **testable (OnGUI)**
- [ ] Con un `DecompositionJob` + un `batch` de muestras: botón **"Iniciar jornada"** → 3 fases en orden:
      **identificar** (elegir nombre correcto), **romper** (Perfecto/Regular/Fallo), **clasificar** (arrastrar
      cada componente a su contenedor). Al agotarse el tiempo por muestra, avanza.
- [ ] Solo cuentan en el `yield` los componentes de muestras **identificadas + rotas + bien clasificadas**
      (×calidad de ruptura); al terminar la jornada llama a `Complete()` → sube la economía (§19d).
- [ ] Jugar **más rápido** despacha más muestras en la misma jornada.

### 19f-bis. Topes derivados de STATS (`MagicReserves.EffectiveCap*`) — sandbox `Magia_AUTO`
- [ ] Con aptitudes base, el HUD muestra "Topes (de stats): ~100 g/elem, ~1e6 J". Botón **«Subir stats de
      prueba»** → suben `EffectiveCapPerElement` (con MaxHealth: resistencia/fuerza/masa) y `EffectiveEnergyCap`
      (con MaxMana: razón/memoria). No hay escala fija por santuario: todo sale de los stats.

### 19g. powerBonus unificado (charge + channeling + forcejeo) + FireSpell + WalkSpell — **sandbox `SpellDemo_AUTO`**
Cápsula naranja (~14,1,6) con HUD arriba-centro (bonus/carga del fuego y del andar + ATP). Charge=**LeftShift**,
channeling=**RightShift**, direcciones=**ESDF**. `CastMode.Charge` retirado: charge/channel son bonos ortogonales
sobre un único `powerBonus` que **decae** con el tiempo.
- [ ] **Fuego múltiple**: mantener **G** → salen llamas seguidas; el **forcejeo** sube el `powerBonus` (HUD) y la
      llama se **agranda** (log). Se detiene al soltar / al agotar reservas o energía.
- [ ] **Cargar esfera gigante (LeftShift)**: mantener **G+LShift** → *carga* sube (HUD), **no dispara**; **soltar
      LShift** → sale **una esfera gigante** ∝ carga (tope `maxPowerWithCharge`).
- [ ] **Canalizar (RightShift)**: tras cargar, mantener **G+RShift** mientras disparas → las esferas **mantienen
      tamaño** hasta `maxPowerWithChanneling` (si es < carga, menguan hasta ese suelo y se estancan; si es igual,
      no menguan). Solo RShift sin carga → el bonus **sube gradual** hasta su tope.
- [ ] **Decaimiento**: sin sostener nada, el `powerBonus` **baja** hacia 0 (vuelta al hechizo base).
- [ ] **Caminar (ESDF)**: **e/s/d/f** mueve la cápsula y **gasta ATP**. **LShift parado = postura de salida**
      (quieto, cargando) → **soltar** = arranque con velocidad inicial ∝ carga. **RShift** al correr = subir a la
      **punta**. Contra un obstáculo (sin desplazarte) el **forcejeo** sube (empuja más).
- [ ] Los TOPES escalan por stats: charge/forcejeo con aptitudes **físicas**, channeling con **mentales**.

### 19f. Sustrato de quarks del S4 (`QuarkReserve`)
- [ ] `AddGrams`/`AddQuarks` y `GramsAvailable` (1 g ≈ 1,807×10²⁴ quarks). `AtomsAvailable(symbol)` da un
      número plausible para la UI.
- [ ] `MakeElement(reservas, símbolo, gramos)` transforma quarks→pool de ese elemento (gasta quarks solo por
      lo creado; respeta el tope). `Restitute(reservas, gramos)` → sube el pool de **energía** (E=mc²).

### Sandboxes (PR #54) — cómo entrar
Corre `Tools → Cold Sanctuary → Build Sample Scene Blockout` y dale a Play:
- **`Descomposicion_AUTO`** (~8,1.5,6): HUD «Iniciar jornada» → 3 fases → sube Elements/Energy en el HUD de recursos.
- **`Magia_AUTO`** (~10,1,6): HUD de prueba (abajo-izq) del bucle comer→desbloquear→lanzar (quarks/energía/fuego).
- *Falta (siguiente):* un `Magia_AUTO` con **`Anima` real** para probar también grasa/stats de `Constitution`;
  `ChimeraFeed`; escalado de topes por nivel; sembrar el `batch` desde la materia real del área.

## 20. Alma por MEZCLA — fase 1 (`SoulComposition`) — sandbox `AlmaBlend_AUTO`
Tres cápsulas (~16–21, 1, 6) que se componen por arquetipos y resuelven sus stats en Play.
- [ ] En los logs `[Alma]`: **Panterilia_Blend** (Human 90 + Lion 5 + shareDomain) sale ~humana con rastro felino.
- [ ] **OsoMenteHumana** (cuerpo Bear + mente Human 90/Bear 10): físico de oso (str/masa altas, grande) pero
      mente humana (razón/creatividad altas).
- [ ] **Oso_bonusPack3** (Bear/Bear + bonusPack3): mismas aptitudes de oso **+2.5 a todo** (stats altísimos), sin
      cambiar personalidad.
- [ ] El **tamaño** se ve en escena (el oso más grande que Panterilia; Bunny/Gallina serían pequeños).
- [ ] **Blend por DISTRIBUCIÓN** (PR #70): un arquetipo al 1% empuja la forma (ya no despreciable). Panterilia
      (Human 90 + Lion 5) sale humana con leve sesgo felino.
- [ ] **Conversión (Ambrosio_Convert)**: HUD "SoulConvert". El ser arranca Toro+Bear (grande/fuerte/lento). Botón
      **A/relativa** → hormiga cuya forma la modula la hormiga; **B/literal** → misma forma exacta, tamaño hormiga.
      **Reset** vuelve al original para comparar A vs B. (B es el modo de las reencarnaciones: identidad marcada.)
- [ ] **Mente por blend** (PR #72): `OsoMenteHumana` (mente Human) suelta frases `[Mente]` con tono **Viento/Fuego**; `Oso_bonusPack3` (mente Bear) con tono **Tierra** → el tono emerge del blend, no del cuerpo.
- [ ] **4 compañeros por composición** (PR #78): `Panterilia_SinClase`/`Goluis_SinClase`/`Gohageneis_SinClase`/`Irosene_SinClase` — sus `[Alma]` deben coincidir con los `Base*` reales de cada companion (Goluis str 1.5/disc 1.3; Gohageneis adapt 1.7/soc 1.7; Irosene soc 1.7/cre 1.5…), SIN `CompanionBase`.
- [ ] *(Fase 1 = solo el motor; Bear/Wolf/Panterilia reales AÚN no migrados.)*

## 21. Alma COMPARTIDA (reencarnaciones) — sandbox `AlmaCompartida_AUTO`
Dos cuerpos (melaza Toro+Bear / hormiga Ant+Human) comparten UNA alma. HUD arriba-centro-abajo.
- [ ] Al arrancar, ambos cuerpos muestran stats coherentes con la MISMA identidad (forma compartida, cada uno a su presupuesto/tamaño).
- [ ] **Entrena poder (+0.5)** → suben los stats de LOS DOS cuerpos a la vez (str/masa/agi). **Se lesiona (−0.3)** → bajan ambos (reinicios).
- [ ] **+bond (Ruth)** → el contador de bonds del alma sube y es compartido por todas las reencarnaciones.
- [ ] *(Falta: propagación perezosa por era; hoy ambos están activos en escena.)*

## 22. Compañero por composición (fase 5) — sandbox `AlmaBlend_AUTO`
- [ ] `Panterilia_SinClase` (SimpleAnima + SoulComposition arquetipo Panterilia + Mind + BondPillar): en el `[Alma]` sus stats coinciden con la Panterilia real (per 1.7, rea 1.6, disc 1.5, str 0.7…), SIN heredar `CompanionBase`.
- [ ] `BondPillar` (universal, SIN vía directa al jugador): familiariza con CUALQUIER `ITarget` cercano por cercanía (crece `Anima.bonds`); si el vecino tiene mente (el jugador vía `PlayerTarget`), lo reconforta según el bond. El jugador es un ITarget más.
- [ ] **Karma** (PR #76): `Panterilia_SinClase` (speciesBonds=Human) al cruzarse con un perro/`Malamute` arranca el bond en +45 (agrado dog↔human); con una especie sin relación, en 0. La karma NEGATIVA no siembra bond (el rechazo lo lleva el threat por poder, p.ej. lobo↔komodo).

## 23. Refactor de comportamiento animal — componentes (E1–E5, PRs #94–#114)

El "ser animal" pasó de una jerarquía de clases (`Animal`/`Carnivore`/`Herbivore`/8 especies) a **componentes + data**
sobre `Anima`. La conducta ya no está en `Feed`/`Escape` de la clase, sino en `ThreatResponder`/`Locomotion`/`Forager`
(auto-añadidos en `Animal.Init`), y la identidad de especie en `SpeciesBody`/`SpeciesProfile`/`Physiognomy.Of`.
**OJO (cabo conocido, línea ~165): el sandbox actual apenas dispara caza** → hay que forzar el encuentro para
ejercitar la lógica movida. No basta con "spawnea y no peta".

**Setup sugerido:** en una escena con NavMesh horneado, spawnear un **depredador** (Wolf/Bear) y varias **presas**
(Bunny/Deer) a ~10–20 m, y dejar correr en Play. (Si `SampleSceneBuilder` no lo monta, colocarlos a mano.)

- [ ] **Componentes presentes**: al entrar en Play, cada animal tiene en su GameObject `ThreatResponder`,
  `Locomotion`, `Forager`, `SpeciesBody`, `AiBrain` y `AnimaController` (auto-añadidos). El `[Alma]`/inspector
  muestra stats de su especie (Wolf: str/mass del arquetipo; `SpeciesBody.baseAgility` 1.2, `basePerception` 1.4).
- [ ] **Forrajeo (Forager)**: un herbívoro hambriento (`hungry >= 0`) camina al `GrassPatch`/`FishSchool` más
- [ ] **Presa por PROXIMIDAD + STATS** (Diet retirada, PR #119): un carnívoro hambriento caza al `Anima` comestible más fácil/cercano dentro de `huntRadius` que pueda por stats (`Predation`); NO caza su especie ni a quien tiene vínculo; el OSO/LOBO cazan al JUGADOR si no hay bond. Radio de detección = `perception × huntRangePerPerception` (deriva de STATS, crece con la evolución). Come también CARCASAS cercanas (incl. de su especie: scavenging). Tunables: `Forager.{huntRangePerPerception, minHuntRadius, distanceWeight}`. (Si no caza, subir `huntRangePerPerception`.)
  cercano y come (baja `hungry`); un carnívoro elige presa por `Diet`, la persigue y la muerde. Con **omnívoro**
  (marcar `Forager.eatsPrey`+`eatsGrass` en el inspector) elige la fuente **más cercana**.
- [ ] **Amenaza (ThreatResponder)**: una presa detecta al depredador cercano (`Assess > ThreatThreshold`) y **huye**;
  el depredador **caza**. Un ser con **bond alto** hacia el otro NO huye (el bond desactiva la amenaza). Manada:
  un lobo solo huye del oso, pero **con aliados cerca** (EffectivePower) puede plantar cara.
- [ ] **Defensa de crías EMERGENTE** (sin flag): un adulto con crías propias cerca de la amenaza planta cara si
  `autoabandono + cubBond > peligro` (no depende de una especie "defensora").
- [ ] **Locomoción**: el `NavMeshAgent` sigue navegando (rodea obstáculos); si el animal lleva `WalkSpell`, la
  `nav.speed` sube al correr y decae al andar (opt-in). Sin él, usa los `navSpeed` de `ActsPrep`.
- [ ] **IA + posesión (E4)**: sin poseer, la IA decide sola (forrajea/huye). Al **poseer** un animal (PlayerBrain de
  mayor relevancia), su IA se **suprime** y lo conduce el jugador. (Cabo: al poseer, el WalkSpell/Transform puede
  pelear con el NavMeshAgent — ver plan.)
- [ ] **Medio (SpeciesBody)**: una foca/ballena en agua se mueve normal; en tierra, penalizada (WaterAffinity 1.0 /
  LandAffinity baja, ahora data del arquetipo).
- [ ] **Paridad**: caza/huida/pastoreo/manada se comportan como ANTES del refactor (mismos valores; los escalares
  salen de `SpeciesProfile`, extraídos 1:1 de los overrides).

**Knobs de recalibración** (si algo se siente mal, ajustar en el inspector del componente, NO en código):
`ThreatResponder.{aggressionGate, auraFear, alertReach, fightPowerMargin}` y, por especie, `SpeciesProfile`
(threatThreshold, packFactor…) + `SpeciesBody.{baseAgility, basePerception}`.

## 24. Modelo emergente: temperamento/sentidos/armamento/confianza (PRs #128–#137)

Todo **balance-safe**: al spawn la conducta debe ser **idéntica** a antes; los efectos EMERGEN con el tiempo o son
dormidos. Doc: [`capabilities-and-embodiment.md`](capabilities-and-embodiment.md).

- [ ] **Regresión (lo primero)**: fauna recién spawneada caza/huye/pastorea **igual** que antes de #128 (nada cambia
  al nacer: confianza 0, `armament` 0, percepción ≥1 → sin gateo).
- [ ] **`canHitAndRun` disuelto (#128)**: una madre (coneja/ciervo) con cría, ante una serpiente/depredador que NO puede
  vencer, **acosa** (golpe + retirada) en vez de plantarse; un oso fuerte con cría se **planta** (Fight). No hay flag.
- [ ] **`Assess` gateado por sentidos (#129)**: baja `perception` de una presa por debajo de 1 en el inspector → **tarda
  más o no reacciona** a un depredador (lo "lee" peor); percepción alta → reacciona de más lejos. Con percepción ≥1
  (default de toda la fauna) NO cambia nada.
- [ ] **Armamento ⟂ masa (#130)**: a un ser pequeño y débil súbele `Anima.armament` en el inspector → su `PredatorPower`
  sube y puede **cazar/ser temido** por seres mayores (avispa>cucaracha). Con `armament` 0 (default) sin cambio.
- [ ] **Confianza → agresividad histórica (#131/#132/#137)**: un depredador que **caza con éxito** repetidamente sube
  `spellConfidence["combat"]` → se vuelve **más osado** (ataca en situaciones donde antes dudaba). Un herbívoro nunca
  la sube. Ganar una **pelea** también refuerza (#137). Sin uso, la confianza **decae** lentamente (#137).
- [ ] **D3a/E2 dormidos (#134)**: la fauna con `Mind` **piensa igual** (ninguna frase fija `capability`/`gateCapability`
  todavía). Sin regresión en `Mind`.

## 25. Asfixia + deambular local (PRs #139, #140)

- [ ] **Asfixia (#139)**: una **ballena/foca varada en tierra** (o un terrestre atrapado en agua profunda del que
  `CorrectMedium` no puede sacarlo) **pierde vida progresivamente** (`Suffocate` vía `Hurt`) y acaba muriendo. Un animal
  en **su** medio, sin cambios. Un terrestre que **nada de paso** (waterAffinity 0.4 > umbral 0.3) **NO** se ahoga.
  Tunables: `Animal.{asphyxiaThreshold, asphyxiaRate}`. (Ojo: los que pescan necesitan `waterAffinity ≥ asphyxiaThreshold`.)
- [ ] **Deambular local (#140)**: un animal en `Wander` (evento de etapa) se mueve a un **punto local** dentro de su
  territorio (radio = `HomeRadius` acotado 5–25), **ya no hacia un ave al azar**. Si se aleja, `Homebound` lo devuelve.
  No debería alejarse hacia pájaros voladores.

## 26. Rejilla de feromonas `TraceField` (PR #145)

Primitiva **aislada y dormida** (nada la usa aún). **AUTO (PR #150):** `NavTests_AUTO` (lo añade
`SampleSceneBuilder`) ejecuta `TraceFieldTest` al entrar en Play y **reporta por `TestProbe`** — en `Editor.log`,
grep `[TEST] ... TraceField` (depósito/lectura/gradiente/decaimiento/`volumetric`). Test manual de respaldo:

- [ ] **Sin instancia = no-op**: sin un `TraceField` en escena, `TraceField.Sniff/Trail` devuelven 0/zero y
  `Leave` no hace nada (no peta). No cambia ninguna conducta.
- [ ] **Depósito y lectura**: añade un GameObject con `TraceField`. Desde un script de prueba, `Leave(p, ScentSelf,
  10)` y luego `Sniff(p, ScentSelf)` > 0 en esa celda; en una celda lejana, 0.
- [ ] **Gradiente**: deposita en una celda y `Trail(posVecina, canal)` apunta **hacia** ella (vector no-cero).
- [ ] **Decaimiento + poda**: tras un tiempo (según `decayPerSecond`), `Sniff` baja hacia 0; `ActiveCells` vuelve a 0
  cuando todo se agota (poda cada `pruneInterval`).
- [ ] **`volumetric` (mar/aire)**: con `volumetric = true`, dos depósitos a la **misma x,z pero distinta profundidad y**
  caen en **celdas distintas** y `Trail` da componente vertical; con `false`, comparten celda (2D). Sin impacto de perf.

## 27. Navegación N1 — huir lejos + wander por rastro (PR #147)

- [ ] **Flee correcto**: acércate con un depredador a una presa (o baja el bond) → la presa **huye en dirección
  CONTRARIA** al depredador (se aleja), **ya no** corre hacia un pájaro al azar ni hacia el depredador. Re-orienta si el
  depredador la persigue. Tunable: `Animal.fleeStep`.
- [ ] **Wander por rastro (dormido)**: **sin** un `TraceField` en escena (o sin rastros), `Wander` deambula local
  **igual que en #140** (sin cambio). Con un `TraceField` y un depósito de `ScentFood` cerca (script de prueba
  `TraceField.Leave(p, ScentFood, 20)`), un animal que hace `Wander` **deriva hacia** esa zona. Balance-safe: sin
  rejilla, `Trail` = 0 → sin efecto.

## 28. N7 — subproducto de hechizo → rastro (PR #148)

- [ ] **Regresión**: los hechizos actuales (Pull/Walk/Fire/Honeydew…) funcionan **igual** (todos con `leavesByproduct`
  = false por defecto → `LeaveByproduct` no-op).
- [ ] **Bucle completo (opt-in)**: en un hechizo con input (`spellKey`), marca `leavesByproduct = true`,
  `byproductChannel = ScentFood`, `byproductStrength = 20`. Pon un `TraceField` en escena. Lanza/mantén el hechizo
  moviéndote → deja un **rastro** (verificable con `TraceField.Sniff/ActiveCells`), y un animal cercano que hace
  `Wander` **deriva hacia** el rastro (N1). Al parar, el rastro **decae** y se poda.
- [ ] **Rate-limit**: con `byproductInterval = 0.5`, el depósito no se dispara cada frame (no satura la celda).
- [ ] **Sin TraceField**: con el hechizo opt-in pero **sin** rejilla en escena, no peta (no-op).

## 29. Convención de tests automáticos — `TestProbe` (PASS/FAIL por consola)

Faltaba una capa de **aserto**: los sandboxes/drivers conducen o vuelcan valores, pero nadie decía PASS/FAIL.
**`TestProbe`** (`Assets/Scripts/TestProbe.cs`) lo cubre y es **reutilizable en dos frentes**:
- **Sandboxes autoejecutables** (`NavAuto`/`EmergenceAuto`/`VolitionAuto`…): conducen la situación y luego `TestProbe.Check`ean.
- **Misiones jugables (WASD)**: la misión llama `TestProbe.Check` en sus criterios de éxito → el **juego real** reporta
  pruebas por el mismo canal (visión del usuario: las misiones-test de hoy son el avance del juego de mañana).

**Cómo leerlo** (el compañero, en `Editor.log`): grep **`[TEST]`**. `[TEST] PASS/FAIL · <nombre> — <detalle>`, y el
veredicto por grupo `[TEST] ▲ <grupo> — SUMMARY: X PASS / Y FAIL`. **FAIL usa `LogWarning`, NO `LogError`** → no
contamina el "0 errores de compilación". API: `Begin(grupo)` · `Check/Near/Greater/NotNull(...)` · `End()`.

- [ ] **Humo del propio harness**: un `TestProbe.Begin("demo"); Check("ok", true); Check("no", false); End();` produce en
  consola una línea PASS, una FAIL (warning) y un SUMMARY `1 PASS / 1 FAIL`.

## 30. `FaunaChecks_AUTO` — conducta/stats sobre la fauna real (PR #151)

Complementa §23/§24 automáticamente: `SampleSceneBuilder` añade `FaunaChecks_AUTO`, que tras ~1.5 s (asentar
`FamilyGenerator`+`Init`) asevera por `TestProbe` sobre los animales REALES (grep `[TEST]` en `Editor.log`):
- **Wiring del refactor**: cada animal tiene `ThreatResponder`/`Locomotion`/`Forager`/`SpeciesBody`/`AiBrain`/
  `AnimaController`; `SpeciesBody` sembró `strength > 0`.
- **Armamento ⟂ masa (#130)**: subir `armament` sube `PredatorPower` (restaurado en el mismo frame).
- **Confianza histórica (#131/#132)**: `Confidence("combat")` arranca en 0.
- **Depredación por stats (#119/#32)**: el depredador más fuerte PUEDE con la presa más blanda; la presa NO puede
  cazar al depredador.
- **`Assess` gateado por sentidos (#129)**: bajar la percepción de la presa reduce la amenaza percibida.

Asevera **funciones deterministas** (no la emergencia con timing → no flaky). Omite (SKIP, sin FAIL) lo que la
composición de fauna no permita. **Interpretación**: si `[TEST] ▲ FaunaChecks — SUMMARY` da `0 FAIL`, el modelo
emergente está bien cableado en runtime; un FAIL apunta al building-block exacto.

## 31. `EmergenceAuto_AUTO` — bucle de temperamento (PR #152)

`SampleSceneBuilder` añade `EmergenceAuto_AUTO`; tras ~1.6 s asevera por `TestProbe` el **bucle emergente completo**
(lo que `FaunaChecks` de una-aserción no cubre), conducido de forma determinista (no espera cazas reales → no flaky):
- **Productor**: `RecordUse(Combat, éxito)` sube la confianza.
- **Consumidor → conducta**: con `aggressiveness = 0`, `Decide` frente a la presa más blanda pasa de **≠Fight** (confianza 0)
  a **Fight** (confianza 80) → el temperamento es histórico. (SKIP si el depredador no supera el margen de poder.)
- **Decaimiento**: `DecayConfidence` baja la maestría.

Muta confianza/agresividad y **restaura en el mismo frame** (sin yields) → no altera la sim. Grep `[TEST] ... Emergence`.
Los tests con TIMING real (una caza que sube la confianza sola; muerte por asfixia) y los WASD (misiones) van aparte.

## 32. `TestRunner` — orquestación de tests (PR #153)

Los `*_AUTO` ya **no** auto-corren cada uno por su lado: un solo **`TestRunner_AUTO`** (lo pone `SampleSceneBuilder`)
reúne las `ITestUnit` y las corre **grupo a grupo EN SERIE** (el "array-de-arrays": grupos serie × paralelo-dentro solo
si `ParallelSafe`, que por defecto es false porque casi todo muta estado compartido —la rejilla es singleton, la fauna
se muta/restaura—). Grupos actuales: **0** `TraceFieldTest` (rejilla) → **1** `FaunaChecks` → **2** `EmergenceAuto`.
Emite un **`[TEST] ═══ TOTAL: X PASS / Y FAIL`** al final → **un solo veredicto ordenado** en `Editor.log`.

- [ ] Al entrar en Play, la consola muestra los grupos en orden (0,1,2), cada uno con su `SUMMARY`, y un `TOTAL` final.
- [ ] **Añadir un test** = un componente `ITestUnit` (Group + ParallelSafe + `Run()`) en el `TestRunner_AUTO`. Las
  **misiones WASD** reportan por el mismo `TestProbe` → el `TOTAL` juntará laboratorio y juego real.

## 33. Primera misión WASD-test — `ReachGoalMission` (PR #155)

El frente de **juego real**: una misión mínima jugable que reporta por el mismo `TestProbe`. `SampleSceneBuilder`
añade `WasdMission_AUTO`; coloca un **marcador de refugio** ~12 m adelante del jugador.
- [ ] **Jugar**: con **WASD**, camina hasta el marcador. Al llegar, la consola muestra `[TEST] ▼ Misión WASD: llegar al
  refugio` con `el jugador se movió (hubo WASD)` = PASS y `llegó al refugio` = PASS, y su `SUMMARY`.
- [ ] **Anti-falso-positivo**: si aparecieras sobre la meta sin moverte, `el jugador se movió` daría FAIL (recorrido <
  umbral). Sin `Player` en escena → SKIP (sin FAIL).
- Nota: emite sus `[TEST]` **al completarse** (la conduce el jugador, no el `TestRunner`) → aparece después del `TOTAL`
  del laboratorio; grep `[TEST]` los junta.

## 34. Grimoire — repertorio de doble vía (PR #156)

`GrimoireTest` (grupo 3 del `TestRunner`) asevera por `TestProbe`: el grimorio arranca **vacío/bloqueado** y `Learn`
 desbloquea; sobre un `Anima` real, `CanUse(id, false)` = false sin vía, `CanUse(id, true)` = true por la **vía corporal**,
 `KnowsSpell` = false sin grimorio, y tras `AddComponent<Grimoire>()`+`Learn` → `CanUse(id, false)` = true por la **vía
 mágica** (grimorio transitorio, se destruye). Grep `[TEST] ... Grimoire`.

## 35. Celo (estro) → rastro Estrus (PR #157)

Primer paso de reproducción + **enciende el sistema de trazas** (ahora hay un `TraceField_AUTO` PERSISTENTE en escena;
 antes todo `Leave`/`Trail` era no-op). `EstrusTest` (grupo 4) asevera: hay `TraceField.Instance`; cada animal tiene
 `EstrusState` (auto-add en `Init`); `Emit()` deja rastro `Estrus` legible. Grep `[TEST] ... Estrus`.
- [ ] En Play (tiempo acelerado): un ADULTO entra en celo por ciclo (`EstrusState.InEstrus`) y deposita `Estrus` en la
 rejilla; se puede leer con `TraceField.Sniff(pos, Estrus)`. Balance-safe: nada consume el celo para reproducirse aún
 (cortejo/gestación = pasos 2-3) → sin cambio de población. El deseo `mate` (buscar pareja por el gradiente de Estrus)
 está en `DesireCatalog` pero DORMIDO hasta activar `Volition` (D3b2).

## 36. Reproducción — cortejo/concepción/parto + ciclo de vida (PR #158)

Cierra el ciclo de vida. `Reproduction` (auto-add en `Init`, **GATEADO OFF** por `Reproduction.Enabled`): una HEMBRA
 adulta en celo, saciada y con pareja compatible cerca concibe (gestación) y **pare** una cría (copia del progenitor en
 etapa `child` → crece por el ciclo existente). `LifeCycleAuto` (grupo 5) asevera el MECANISMO por `TestProbe`:
 `SpawnOffspring()` produce una cría en etapa CHILD de la misma especie (se destruye tras el test). Grep `[TEST] ... LifeCycle`.
- [ ] **Activar en Play** (opt-in): poner `Reproduction.Enabled = true` (desde un driver o consola). Con el tiempo
 acelerado, adultos en celo cercanos de sexo opuesto → cría nueva tras la gestación; respeta cooldown + tope de
 población (`softPopulationCap`). Con `Enabled=false` (default): **sin cambio de población**.
- [ ] Nacer/crecer/morir (vejez `adult→soul`, ser comido, daño/asfixia) ya existían; **reproducir** era el hueco.

## Notas — lo que NO está cableado aún (no reportar como bug)
- `BondActivity` (marga de Vínculos) aún es huérfano en el juego → la XP de Vínculos fluirá cuando se
  cablee su UI; el gancho ya está puesto.
- Orphans del yoga: `AsanaEvaluator` no se instancia y `AccumulatePostureStress` no se llama → la vía
  `AsanaEvaluator` de cola/entrenamiento y el estrés postural aún no se disparan (ver `known-issues.md`
  §Yoga). Pendiente de arreglar con compilación.
- XP de yoga por **práctica directa** (no por misión) aún no cableada.
- `NPCBase` / mente escalable: solo diseño.
- HUD es **prototipo OnGUI**; se sustituirá por UI declarativa.
