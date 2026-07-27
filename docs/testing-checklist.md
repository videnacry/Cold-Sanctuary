# Checklist de pruebas (Mesocosmos: progresión, farming, recursos, trepar, cocina)

Qué probar **en teoría** de lo construido hasta 2026-07-24. Nada de esto se compiló en el entorno de
trabajo, así que el editor es la **primera compilación real**. Marca ✅/❌ y anota el texto de cualquier
error del Console/Rider.

> **Controles nuevos:** `V` = jugar con criatura · `F`/clic = interactuar (dar de comer, máquinas) ·
> `Espacio` = trepar. (Combate/movimiento previos sin cambios.)

## Estado de sesión (para retomar sin contexto previo)

- Los 60+ archivos de la PR (Progresión/Farming/Meditación/MobWorld/Avatares) ya están **sincronizados**
  repo↔proyecto vivo (`C:\Users\Blein\COLD-SANCTUARY`).
- **Un solo error de compilación** encontrado y arreglado: faltaba `using UnityEngine;` en
  `Assets/Scripts/Avatar/RobotAvatar.cs` (usaba `[Tooltip(...)]` sin el using). Ya sincronizado.
- El **MCP de Unity está instalado pero hay que arrancarlo a mano cada sesión nueva**: `Window > MCP for
  Unity > Toggle MCP Window` → botón `Start Server` → botón `Connect`. Sin esto, las herramientas
  `mcp__unityMCP__*` fallan con "No Unity Editor instances found" y hay que usar captura de pantalla
  (mucho más lento).
- Próximo paso: seguir por la sección 3 (Farming) hacia abajo.

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
- [ ] Acércate a **Suave** y pulsa **V** repetidamente: baja su "tensión" → **color rojo→verde**.
- [ ] Al llegar a serena (verde): deja de responder a `V`, **suelta recursos + monedas** (Console
      `[Farming] … serena … suelta recompensa`), da **XP** (sube la marga de Stats) y a veces **items**
      (`[Farming] Botín: … Golosina/Colmillo de guerra`).
- [ ] Ya serena, aparece el prompt **"Dar comida y agua"**; con **F**/clic → queda **descansando**
      (color azul), Console `[Farming] … saciada y descansando`.
- [ ] **Combo/feel:** encadenar `V` acercándote y alejándote serena **más rápido** (excitación); la
      cápsula **rebota/crece** al excitarse y se **gira/acerca** a ti.
- [ ] **Atrapada:** si te quedas pegado a la cápsula, tras ~1 s te "atrapa" (Console `… te atrapó…`) y
      **resetea el combo** (da un saltito atrás).
- [ ] **Dura** (peligrosa): con excitación alta y sin alejarte, **pierde el control y te pega** →
      **baja la Vida** en el HUD (Console `… perdió el control…` / `[Alma] … recibió …`). Con poca
      excitación, la **defensa** puede absorber el golpe (Console `… absorbió el golpe`).
- [ ] **Salvaje** (no criada): **`V` no hace nada** (no es jugable → regiría la ley natural). Demuestra
      el gateo por bond/estado.

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
      (`LivingEntity` ahora implementa `IAptitudes` y tiene 12 campos nuevos, todos con default 1).
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

## Notas — lo que NO está cableado aún (no reportar como bug)
- `BondActivity` (marga de Vínculos) aún es huérfano en el juego → la XP de Vínculos fluirá cuando se
  cablee su UI; el gancho ya está puesto.
- Orphans del yoga: `AsanaEvaluator` no se instancia y `AccumulatePostureStress` no se llama → la vía
  `AsanaEvaluator` de cola/entrenamiento y el estrés postural aún no se disparan (ver `known-issues.md`
  §Yoga). Pendiente de arreglar con compilación.
- XP de yoga por **práctica directa** (no por misión) aún no cableada.
- `NPCBase` / mente escalable: solo diseño.
- HUD es **prototipo OnGUI**; se sustituirá por UI declarativa.
