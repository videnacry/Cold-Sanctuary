# Checklist de pruebas (Mesocosmos: progresión, farming, recursos, trepar, cocina)

Qué probar **en teoría** de lo construido hasta 2026-07-24. Nada de esto se compiló en el entorno de
trabajo, así que el editor es la **primera compilación real**. Marca ✅/❌ y anota el texto de cualquier
error del Console/Rider.

> **Controles nuevos:** `V` = jugar con criatura · `F`/clic = interactuar (dar de comer, máquinas) ·
> `Espacio` = trepar. (Combate/movimiento previos sin cambios.)

## 0. Preparación
- [ ] El proyecto **compila** (Rider: Build Solution; o Unity: Console sin errores al cargar).
      Si falla, apuntar archivo:línea (zonas probables: `Progression/*`, `IAptitudes` en
      `LivingEntity`/`CompanionBase`/`PlayerStats`, `PlayerClimber`).
- [ ] `Tools → Cold Sanctuary → Build Sample Scene Blockout` corre sin excepciones (genera también la
      escena `MobWorld_Mesopotamia` y la añade a Build Settings).
- [ ] Al entrar en Play no hay errores rojos en la Console; el jugador (`Player`) existe y se mueve.

## 1. HUD (esquina sup. izq., OnGUI prototipo)
- [ ] Muestra el santuario ("Santuario Terrestre") y sus **recursos** (Food/Materials/Research).
- [ ] Muestra las **margas del alma** de Kushal: Stats / Yoga / Vínculos (niveles + XP de Stats).
- [ ] Muestra **Vida / Energía / Maná / Def / Poder**. Con el perfil de Kushal (str/end 1.2, agi 1.1)
      los valores iniciales ≈ **Vida 115, Energía 117, Maná 50, Def 16** (nivel base).
- [ ] El **maná aparece como "(bloqueado — practica yoga)"** al inicio.

## 2. Recursos de santuario (economía pasiva)
- [ ] Los recursos **suben solos** con el tiempo (aporte pasivo por área: cocina/huerto→Food,
      mecánica/textil→Materials, enfermería/veterinaria→Research).
- [ ] Al **subir la velocidad de tiempo** (`TimeController`/`TimeTest` si está), suben más rápido
      (escala Meso-lento / Macro-rápido).

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

## Notas — lo que NO está cableado aún (no reportar como bug)
- `BondActivity` (marga de Vínculos) aún es huérfano en el juego → la XP de Vínculos fluirá cuando se
  cablee su UI; el gancho ya está puesto.
- Orphans del yoga: `AsanaEvaluator` no se instancia y `AccumulatePostureStress` no se llama → la vía
  `AsanaEvaluator` de cola/entrenamiento y el estrés postural aún no se disparan (ver `known-issues.md`
  §Yoga). Pendiente de arreglar con compilación.
- XP de yoga por **práctica directa** (no por misión) aún no cableada.
- `NPCBase` / mente escalable: solo diseño.
- HUD es **prototipo OnGUI**; se sustituirá por UI declarativa.
