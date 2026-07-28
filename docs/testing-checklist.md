# Checklist de pruebas (Mesocosmos: progresión, farming, recursos, trepar, cocina)

Qué probar **en teoría** de lo construido hasta 2026-07-24. Nada de esto se compiló en el entorno de
trabajo, así que el editor es la **primera compilación real**. Marca ✅/❌ y anota el texto de cualquier
error del Console/Rider.

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
- **Bug real encontrado, sin arreglar todavía**: los compañeros no reciben sus aptitudes de perfil tras la
  migración a `Anima` — ver detalle en la sección 10 de abajo.
- **Sección 3 (Farming) prácticamente completa**: Suave/Media/Salvaje confirmados con evidencia de
  `Editor.log`. Atrapada de Dura confirmada (y descartado que sea el bug de corrutinas de
  `Escape()`/`Fight()`). Solo falta la rama de daño de Dura ("pierde el control") — necesita juego
  real con WASD, no teletransportar el Transform vía Inspector (los intentos con teletransporte no la
  dispararon de forma confiable).
- Próximo paso: sección 4 (Progresión a fondo — subir de nivel una marga completa), 5 (Trepar), 6
  (Cocina→Mesopotamia), luego terminar 8/9 (Mind — solo se tocó `MindPhrase.cs` de pasada, falta
  probar en Play), 10 ya casi completa (falta regresión y `physicalResistance` de NPCs).

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
- [ ] Serenar criaturas sube la **marga de Stats** (por la ganancia de aptitudes que da la XP); al
      subir de nivel, Console `[Marga] … Stats nivel N` y **Vida/Energía/Maná crecen** en el HUD.
- [ ] Completar una **misión de yoga** (en la Sala de Yoga vía la máquina de virtualización) sube la
      **marga de Yoga**; al llegar a **Yoga nivel 2** → Console "barra de maná desbloqueada" y el HUD
      pasa a mostrar **Maná: n/max**.
- [ ] **Base-bump:** tras subir de nivel cualquier marga, los máximos crecen **más de lo que daría solo
      el factor de nivel** (porque sube también la base de las aptitudes). *(Difícil de ver a ojo; con
      varios niveles se nota.)*
- [ ] **Todas las margas suben los puntos del alma:** subir Yoga sube la Vida igual que subir Stats.

## 5. Trepar (MVP — verificar feel/física)
- [ ] Junto al **`ClimbTree`** (cilindro marrón), mantén **Espacio** → el jugador **sube**.
- [ ] Subir **gasta energía** (baja Energía en el HUD); al agotarse, **deja de subir**.
- [ ] No pasa del **tope del árbol** (`Climbable.topY`).
- [ ] Al **soltar Espacio**, el `CharacterController` se reactiva y el jugador **cae/queda** con
      gravedad normal (no se queda flotando ni se rompe el movimiento).
- [ ] *(Tuning esperado:* velocidad/altura pueden necesitar ajuste — anota si se siente raro.)

## 6. Cocina → Microcosmos (mundo mob jugable)
- [ ] En el área **Cocina**, interactúa con la **máquina de virtualización** (F/clic) → confirmación →
      **fundido a negro** → apareces en la ciudad de **Mesopotamia** (escena `MobWorld_Mesopotamia`).
- [ ] Hay una **misión "Procesar ingredientes"**: aparecen esferas ámbar; acércate y quédate cerca hasta
      procesarlas (Console `[Meditación] N/… resuelto`).
- [ ] Al completar → Console de misión de mundo mob completada; **sal por el `YogaPortal`** → fundido →
      vuelves al santuario en tu posición previa.
- [ ] (Si sale aviso "no se pudo cargar 'MobWorld_Mesopotamia'": correr antes
      `Tools → Cold Sanctuary → Build MobWorld Mesopotamia`.)

## 7. Regresión — que lo previo NO se rompa (tras IAptitudes)
- [ ] Los **animales** (oso/lobo/conejo/etc.) siguen naciendo, moviéndose, comiendo y huyendo como antes
      (`Anima` —antes `LivingEntity`— implementa `IAptitudes` y tiene 12 campos nuevos, todos con default 1).
- [ ] Los **compañeros** (`CompanionBase`) siguen funcionando (bonds, mood); no hay errores por el
      nuevo `IAptitudes`.
- [ ] Las **asanas/yoga** existentes (entrenamiento por extremidad, restaurar canal mental) siguen igual.
- [ ] La **economía de monedas** (misiones, ventas) sigue funcionando.

## 8. Mind MVP — frases por tono elemental (docs/anima-architecture.md)
Sandbox `MindSandbox_AUTO`: 3 cápsulas con `Mind` + aptitudes distintas (Anima_Roca, Anima_Fuego, Anima_Viento).
- [ ] En Play, la Console suelta cada ~4 s frases tipo `[Mente] «Anima_Roca» (Tierra+): "…"`.
- [ ] **Cada ánima tiende a su tono**: Roca→Tierra, Fuego→Fuego, Viento→Viento (con destellos de otros).
- [ ] **Poder mental → longitud**: la Roca (razón/memoria bajas) dice frases **más cortas**; Fuego/Viento
      (razón/memoria altas) llegan a **más partes** (nace→crece→reproduce).
- [ ] La **valencia** (+/−) sigue a la positividad (serotonina−cortisol) de los humores.
- [ ] Con el tiempo, los **humores** derivan a su base (`Regen`) y pensar **consume glucosa** (energía) →
      afecta la longitud alcanzable.

## 9. Mind — campos, pools de frases y reparto (docs/anima-architecture.md §11)
Mismo sandbox `MindSandbox_AUTO` (ahora con un `ThoughtField_Agua`) + logs `[Frases]` en la Console.
- [ ] **Campo de pensamiento**: las ánimas que entren en el radio del `ThoughtField_Agua` (centro ~(-1,10),
      radio 6) se **inclinan a Agua** y su **serotonina sube** (más frases positivas). Fuera del radio,
      vuelven a su tono propio.
- [ ] **Pools cargadas**: al construir/entrar en Play, la Console imprime
      `[Frases] Total=… Elemental=4 Vivencia=18 Deseo=6` y una línea por biografía
      (`Goluis: 3`, `Panterilia: 4`, `Gohageneis: 3`, `Irosene: 4`, y Ötzi vía histórico).
- [ ] **Reparto Estricta**: cada quien recibe SOLO las vivencias de su identidad (Magnate/Ötzi incluidas).
- [ ] **Reparto Libre**: Magnate 🔒 y Ötzi 🔒 **conservan lo suyo** y NO aparecen repartidos en otros; los
      compañeros (libres) y el **anónimo** reciben vivencias barajadas del pool público (cada run distinto).
- [ ] **Vivencias fieles**: las frases de cada fuente cuadran con su carácter (p. ej. Goluis·Tierra
      "Mis manos conocen el peso."; Ötzi·Agua "Comprender mi muerte me libera.").

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
- [ ] ❌ **BUG REAL (o posible falso positivo del diagnóstico)**: los compañeros **NO** muestran sus
      aptitudes de perfil — se esperaba `Goluis str≈1.5` y en cambio `Goluis_Post` (y
      Irosene/Panterilia/Gohageneis, todos con sufijo `_Post` en el nombre) muestran
      `str=1.00 agi=1.00 rea=1.00` — el valor **default genérico**, igual que las crías de animales y el
      Player.
      **Hipótesis con más chances** (leyendo `CompanionBase.cs:109-115`: `Start() { agility =
      BaseAgility; strength = BaseStrength; ... }` — el código SÍ está ahí, bien escrito): esto es el
      mismo bug de **orden de `Start()` entre objetos hermanos** que ya apareció esta sesión con
      `FamilyGenerator`/`HomeOrigin` (ver `DEVLOG.md`) — si `MigrationDiagnostics.Start()` corre ANTES
      que `CompanionBase.Start()` de cada compañero (Unity no garantiza el orden entre `MonoBehaviour`s
      de distintos GameObjects salvo que se configure `Script Execution Order`), el diagnóstico lee las
      aptitudes en su valor default **antes** de que `CompanionBase.Start()` las sobreescriba con el
      perfil. No pude confirmarlo por Inspector (los campos de `Anima` no aparecen ni en modo Debug —
      probablemente son propiedades, no campos serializados, así que no se pueden inspeccionar así).
      **Para confirmar/descartar**: agregar un log a `CompanionBase.Start()` con un timestamp/frame
      count y comparar contra el de `MigrationDiagnostics.Start()`, o simplemente mover el volcado de
      `MigrationDiagnostics` a `LateUpdate()` (una sola vez, con una bandera) en vez de `Start()`.
      **→ RESUELTO (Claude, 2026-07-28): era falso positivo del diagnóstico, NO bug de juego.** Verificado
      leyendo el código: `Goluis.cs` (y las 4) SÍ sobreescriben `BaseStrength => 1.5f` etc., y
      `CompanionBase.Start()` (`:109-122`) las aplica → en el juego las aptitudes se fijan bien. El
      diagnóstico leía en su propio `Start()`, antes de que corriera el `Start()` de los compañeros (carrera
      de orden entre GameObjects, tal como sospechabas). **Fix aplicado**: `MigrationDiagnostics` ahora
      vuelca en el **primer `Update()`** (con bandera `_done`) — Unity garantiza que todos los `Start()`
      corren antes del primer `Update`. **Re-test**: al re-ejecutar, `Goluis_Post` debe mostrar `str=1.50
      agi=0.90 rea=0.70` (y las demás su perfil). Si es así, marcar este ítem [x].
- [x] Línea de **Kushal**: `margas Stats L1/Yoga L1/Vínc L1 · Vida 115 Energía 117 Maná 50 Def 16 ·
      manáDesbloqueado=False` — coherente con lo esperado (HUD ya lo confirmaba en §1).
- [ ] **Regresión** (§7): pendiente de probar en profundidad (animales/compañeros se ven en la lista y no
      hay errores, pero no se verificó comportamiento en Play más allá de eso).
- [ ] **Cambio de comportamiento conocido** (`physicalResistance=1` en NPCs): aún no probado.

## Notas — lo que NO está cableado aún (no reportar como bug)
- `BondActivity` (marga de Vínculos) aún es huérfano en el juego → la XP de Vínculos fluirá cuando se
  cablee su UI; el gancho ya está puesto.
- Orphans del yoga: `AsanaEvaluator` no se instancia y `AccumulatePostureStress` no se llama → la vía
  `AsanaEvaluator` de cola/entrenamiento y el estrés postural aún no se disparan (ver `known-issues.md`
  §Yoga). Pendiente de arreglar con compilación.
- XP de yoga por **práctica directa** (no por misión) aún no cableada.
- `NPCBase` / mente escalable: solo diseño.
- HUD es **prototipo OnGUI**; se sustituirá por UI declarativa.
