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

- Todo lo mergeado hasta la **PR #34** (depredación por stats: `Predation`/`MagicAura`/`StatProfile`/
  `TransformationSpell` — farol vs transformación real, manada por `PackFactor`, commits hasta `9b2dcc2`)
  ya está **sincronizado** repo↔proyecto vivo (`C:\Users\Blein\COLD-SANCTUARY`) y **probado en lo
  automatizable** — ver secciones 11/12/13/14/15/16/17/18. El repo recibe cambios de un compañero de
  equipo (`videnacry`/`beron-gamboa`) en paralelo — **revisar `git log` al retomar** por si hay commits
  nuevos sin sincronizar (buscar archivos `.cs` nuevos/modificados/borrados desde el último hash
  conocido y copiarlos a mano con `cp`/PowerShell `Copy-Item`, replicando borrados también).
- **PRs #35-52 (magia/metabolismo/descomposición/quimeras, `Metabolism`/`Constitution`/`MagicReserves`/
  `Grimoire`/`FireSpell`/`QuarkReserve`/`DecompositionJob`/`DecompositionMinigame`) ya sincronizados
  repo↔proyecto vivo, sin testear todavía** — el compañero dejó la checklist lista para ese arco en la
  **sección 19** (renumerada desde su "15" original al mergear con las secciones 15-18 de esta sesión,
  que cubren un rango de PRs distinto y anterior). Compila; falta cablear sandboxes de demo antes de
  poder probarlo de verdad (ver sección 19).
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

## Notas — lo que NO está cableado aún (no reportar como bug)
- `BondActivity` (marga de Vínculos) aún es huérfano en el juego → la XP de Vínculos fluirá cuando se
  cablee su UI; el gancho ya está puesto.
- Orphans del yoga: `AsanaEvaluator` no se instancia y `AccumulatePostureStress` no se llama → la vía
  `AsanaEvaluator` de cola/entrenamiento y el estrés postural aún no se disparan (ver `known-issues.md`
  §Yoga). Pendiente de arreglar con compilación.
- XP de yoga por **práctica directa** (no por misión) aún no cableada.
- `NPCBase` / mente escalable: solo diseño.
- HUD es **prototipo OnGUI**; se sustituirá por UI declarativa.
