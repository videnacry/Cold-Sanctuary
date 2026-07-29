# Checklist de pruebas (Mesocosmos: progresión, farming, recursos, trepar, cocina)

Registro de pruebas del build hasta 2026-07-28. **Pasada de testing (automatizada) hecha el 2026-07-28**:
la mayoría confirmado con evidencia de `Logs/Editor.log` (ver "Estado de sesión"). **Lo que queda requiere
juego MANUAL** (WASD / mantener tecla): rama de daño de "Dura", trepar, 4 esferas de Mesopotamia + YogaPortal,
misión de yoga, `ThoughtField_Agua`, velocidad de tiempo. Los ítems `[x]` son historial verificado.

> **Controles nuevos:** `V` = jugar con criatura · `F`/clic = interactuar (dar de comer, máquinas) ·
> `Espacio` = trepar. (Combate/movimiento previos sin cambios.)

## Estado de sesión (para retomar sin contexto previo)

- Todo lo mergeado hasta la PR #15 (Progresión/Farming/Meditación/MobWorld/Avatares/migración `Anima`/Mind)
  ya está **sincronizado** repo↔proyecto vivo (`C:\Users\Blein\COLD-SANCTUARY`). El repo recibe cambios de
  un compañero de equipo (`videnacry`/`beron-gamboa`) en paralelo — **revisar `git log` al retomar** por si
  hay commits nuevos sin sincronizar (buscar archivos `.cs` nuevos/modificados/borrados desde el último
  hash conocido y copiarlos a mano con `cp`/PowerShell `Copy-Item`, replicando borrados también).
- **Dos errores de compilación** encontrados y arreglados hasta ahora, mismo patrón ambos (falta
  `using UnityEngine;`): `Assets/Scripts/Avatar/RobotAvatar.cs` (usaba `[Tooltip(...)]`) y
  `Assets/Scripts/Mind/MindPhrase.cs` (usaba `Random.Range`). Ya sincronizados.
- El **MCP de Unity no se pudo conectar esta sesión** (el botón "Configure" del panel busca un `claude` CLI
  local que no se detecta en este entorno) — se testeó todo por captura de pantalla. Si en una sesión
  futura si está disponible: `Window > MCP for Unity > Toggle MCP Window` → `Start Server` → `Connect`.
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

## 11. Control/posesión · Cocina · Virtualización (PRs #16 y #17) — NUEVO
Sandboxes que genera `Build Sample Scene Blockout`. Todo por consola.
- [ ] **Control/posesión** (`PossessionSandbox_AUTO`, logs `[Control]/[Jugador]/[Posesión]/[Petición]`):
      el jugador posee «Anima_Debil» (2>1) y lo mueve con WASD; «Kushal_Follow» lo sigue; con **Tab**
      intenta saltar a «Anima_Fuerte» pero su IA aguanta (2<3); «Aldeano_Pide» pide a Kushal ir a «HelpGoal»
      → alma compartida ~8 s. *(Nota: WASD mueve también al Player real; comparten input.)*
- [ ] **Cocina paso A** (`KitchenSandbox_AUTO`, logs `[Cocina]`): la `DirtArea` genera manchas; al pasar de
      5 → "misión de limpieza ACTIVA"; el `Pinche_Limpia` (auto) las borra mancha a mancha → "misión COMPLETA".
- [ ] **Cocina paseo + desayuno** (`KitchenOnboarding_AUTO`, logs `[Paseo]/[Cocina]`): «Anfitrion» recorre
      Nevera/Plancha/Mesones/Contenedor "enseñando" cada una con «Novato» siguiéndolo (alma compartida);
      «Cocinero» rellena el contenedor con el loop de desayuno y «Comensal» come.
- [ ] **Virtualización — cocina** (`VirtualizationSandbox_AUTO`, logs `[Virtual]/[Producción]`): la **mira
      está FIJA en el centro**; apunta **girando la cámara** (en el sandbox, con el look del PlayerController)
      a las cajitas EN ORDEN — Mesón(abrir→sartén) → Nevera(abrir→huevos) → Cocina(poner sartén→cascar
      huevo→**encender**) — y confirma con **F**. La parte apuntada se **resalta**. 3 desayunos = misión.
- [ ] **Mecanografía (fogón)**: al confirmar **EncenderFuego** aparece una caja "Cocinando…"; **teclea**
      `cook/eggs/protein/healthy/tasty/b2` → cada palabra **recorta tiempo**; al agotarse (o teclearlas
      todas) se completa y produce el desayuno. **La cámara se congela mientras tecleas** (no debería girar).
- [ ] **Virtualización — huerto** (`GardenVirtualization_AUTO`): misma mecánica, receta agrícola
      (compostero/cobertizo/semillero/agua/parcela) en 9 pasos: abonar→arar→trasplantar→regar→cosechar.
- [ ] **Regresión**: nada de lo anterior (movimiento/cámara/asanas) se rompe con los nuevos scripts.

## Notas — lo que NO está cableado aún (no reportar como bug)
- `BondActivity` (marga de Vínculos) aún es huérfano en el juego → la XP de Vínculos fluirá cuando se
  cablee su UI; el gancho ya está puesto.
- Orphans del yoga: `AsanaEvaluator` no se instancia y `AccumulatePostureStress` no se llama → la vía
  `AsanaEvaluator` de cola/entrenamiento y el estrés postural aún no se disparan (ver `known-issues.md`
  §Yoga). Pendiente de arreglar con compilación.
- XP de yoga por **práctica directa** (no por misión) aún no cableada.
- `NPCBase` / mente escalable: solo diseño.
- HUD es **prototipo OnGUI**; se sustituirá por UI declarativa.
