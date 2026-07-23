# Mecánicas de Jugabilidad y Loops de Enganche — Cold Sanctuary

## Por qué los MMORPGs enganchan (y qué tomamos de ellos)

Los juegos de mayor retención a largo plazo combinan cuatro loops:

1. **Progresión visible** — siempre hay un siguiente nivel a un paso. El jugador nunca está
   "atascado sin objetivo". En Cold Sanctuary: la tabla periódica como barra de progresión
   permanente (118 elementos), más los 5 tiers del santuario.

2. **Recompensa variable** — el drop aleatorio es más adictivo que el garantizado (mecánica
   de tragaperras documentada en psicología conductista). En CS: descubrimiento de elementos
   con `elementDiscoveryChance` — el jugador nunca sabe si esta tarea le dará el siguiente.

3. **FOMO — el mundo avanza sin ti** — si el jugador no juega tres días, los NPCs habrán
   subido de nivel, habrán llegado nuevos personajes, quizás la Magnate haya reorganizado
   zonas. Implementado directamente en `AutonomousTaskLoop` + `SanctuaryDirector`.

4. **Múltiples vectores de progresión en paralelo** — si hoy no puedes hacer una mazmorra,
   farmeas crafting. En CS: combate de cocina, asanas, observación, vínculos con NPCs,
   colección de elementos — nunca hay excusa para no jugar.

---

## La Tabla Periódica como Pokédex

**Concepto central**: los 118 elementos son el coleccionable definitivo del juego.

- Cada área del santuario tiene elementos exclusivos o de alta probabilidad.
- Algunos elementos solo aparecen en el jardín submarino, otros solo en el lab de carne cultivada.
- **Completar grupos de la tabla** desbloquea habilidades de alquimia únicas (encantamientos).
- Los elementos más difíciles (platino, radio, elementos sintéticos) requieren acceso al
  Tier 4–5 y horas de juego.

**Descubrimiento orgánico** — el jugador no "estudia" química, la vive:
- Cortar ajo → mob Alliina → daño de azufre (S descubierto)
- Análisis de agua marina → mob Cloruro → (Na, Cl descubiertos)
- Electrólisis en FuelLab → combo de H y O liberados simultáneamente
- Bioluminiscencia en la UnderwaterGarden → P (fósforo, elemento raro)

**Registro en `PeriodicTableManager`** (singleton) — ver código en `Assets/Scripts/Chemistry/`.

---

## Biomas de Combate por Área

Cada zona tiene su propia mecánica de interacción activa (el equivalente de "combat" de WoW).
El jugador no mata nada — procesa, domina, canaliza.

### Kitchen — El Mundo de la Hormiga

**Premisa visual**: al entrar en la cocina, el jugador es miniaturizado. Los ingredientes
son del tamaño de edificios. La cocina es una mazmorra gigante.

**Mecánica**:
- Los ingredientes son `IngredientMob`s con stats propias (health, aggression, element).
- El jugador los "procesa" (no mata): combate cuerpo a cuerpo que termina en un estado de
  "procesado" — el mob colapsa y libera `ElementFragment`s.
- Los mobs tienen patrones de ataque simples (rango corto, área, veneno).
- La levadura se **reproduce** si no la controlas — mecánica de control de masas.
- Ingredientes difíciles (cebolla con aura de azufre) requieren equipamiento o stats mínimas.

**Loop de la cocina**:
1. Entrar en la cocina → miniaturización (ShaderGraph/Scale effect)
2. Aparecer los `IngredientMob`s del día (varía con el tiempo del juego)
3. El jugador los procesa → recoge `ElementFragment`s
4. Suficientes fragmentos → `PeriodicTableManager.Discover(elementSymbol)`
5. Plato completado → stat de cocina +XP, satisfacción restaurada, posible drop de receta

> **Estado real (auditoría 2026-07-09):** el cierre de misión `AreaClear` depende de un
> `KitchenCombatManager` que **no existe** en el repo; `MissionTracker.ReportMobProcessed`/
> `ReportAreaCleared` no tienen invocadores. El loop está diseñado pero aún no cableado.

**Mobs de la cocina** (ejemplos):

| Mob | Elemento | Patrón | Dificultad |
|-----|----------|--------|-----------|
| Levadura (reproduce) | C | Área, se multiplica | Media |
| Alliina (ajo) | S | Daño veneno en aura | Media |
| Cloruro Sódico (sal) | Na, Cl | Carga directa | Baja |
| Capsaicina (chile) | C, H | Daño en área, quemadura | Alta |
| Ácido Láctico (leche fermentada) | C, H, O | Slow + daño continuo | Media-alta |

### AlchemyLab — Radicales Libres

Los mobs son radicales libres e iones inestables. Explotan si mezclas incorrectamente.
Mecánica: combinar mobs correctamente (drag-and-drop o secuencia de teclas) los neutraliza
y libera los fragmentos. Combinarlos incorrectamente → daño de área al jugador.

### Garden / Huerto — Plagas con Territorio

Los mobs son plagas (hongos, insectos invasores, nemátodos). Tienen zonas de control
territorial que el jugador debe limpiar. Mecánica Tower Defense ligera — el jugador planta
defensas naturales y luego elimina las plagas restantes cuerpo a cuerpo.

### FuelLab — Combos de Reacción

No hay mobs como tal. La mecánica es combo-based: el jugador ejecuta secuencias de
reacciones (inputs en tiempo) — electrólisis bien ejecutada = multiplicador de XP + drops
de H y O. Mal ejecutada = explosión menor + penalización.

### SubmarineAccess / UnderwaterGarden — Criatura Submarina

Los organismos marinos raros son mobs con mecánica de taming (no kill). El jugador
debe leer patrones de comportamiento y presentar el "estímulo correcto" para domesticar
o calmar a la criatura → elemento marino raro descubierto.

### MonsterSection — Taming y Lenguaje

Los monstruos son el end-game. Mecánica de comunicación: el jugador aprende un lenguaje
primitivo de gestos/elementos químicos para hablar con ellos. Procesar un monstruo = 
entender lo que necesita, no vencerlo.

---

## Loop Principal de Enganche

```
Entra al santuario
  └─ La Magnate evalúa → asigna área inicial
       └─ Tareas autónomas → stats suben lentamente
            └─ Combate activo en el área → stats suben más rápido + elementos descubiertos
                 └─ Tabla periódica avanza → encantamientos desbloqueados
                      └─ Magnate detecta promotion threshold → lleva al siguiente área
                           └─ Nuevo bioma de combate, nuevos elementos, nuevo FOMO
```

**El ciclo de retención:**
- **Corto plazo**: siguiente elemento, siguiente nivel de maestría
- **Medio plazo**: siguiente área, nuevo bioma de combate, arco de NPC
- **Largo plazo**: completar la tabla periódica, resolver el misterio de la Magnate,
  llegar a la MonsterSection

---

## La Narrativa como Gancho Permanente

El contenido de gameplay solo retiene si hay una pregunta abierta. Para CS:

- **¿Por qué el santuario es secreto?** — cada área nueva revela un fragmento de lore.
- **¿Quién es realmente la Magnate?** — sus conversaciones gotean información contradictoria.
- **¿Qué son los monstruos?** — la MonsterSection es el misterio central.
- **¿Qué pasó aquí antes?** — el santuario tiene historia enterrada en su entorno.

Cada elemento descubierto añade una línea al diario del personaje. El jugador sigue
jugando porque quiere **saber**, no solo porque quiere subir de nivel.

---

---

## Sistema de Vestimenta (reemplaza "armadura" genérica)

Las prendas se crean en el `ClothingCraftingArea` — un área del santuario dedicada.
Dos tracks de crafting que pueden combinarse (Hybrid):

**Textile** — materiales recolectados:
- Lino (huerto), Lana de oveja (cuidado animal), Seda bio-marina (jardín submarino)

**Chemical** — sintetizados desde elementos descubiertos:
- Grafeno (C × 10 fragmentos) → defensa máxima + ligereza total
- Aerogel (Si + O) → aislamiento térmico, velocidad +0.3
- Bio-polímero (C + N + H) → flexibilidad, observación +1
- Kevlar-análogo (C + N) → resistencia a impactos

Cada prenda tiene `defenseRating`, `velocityBonus`, `observationBonus`, `specialEffect`.
Los NPCs también confeccionan, compran y venden ropa (via `NPCEconomy`).

---

## Economía NPC

Los NPCs no son statuas. Con `NPCEconomy`:
- Reciben parte de los coins de misión (`ReceiveMissionReward`)
- Venden excedente de items automáticamente cada `economyTickInterval` segundos
- Compran ropa cuando tienen suficientes coins y la actual es inferior
- Venden la prenda antigua antes de equipar la nueva

Esto crea economía circular viva incluso cuando el jugador no está en escena.

---

---

## Economía de Áreas — Reglas de Precio

### Tipología de áreas según función económica

**Talleres productivos (Tier 1–2):** cocina, vestimenta, suplementos, lab de combustible.
- Producen ítems consumibles o vendibles como resultado de misiones.
- Tienen un **vendor propio** que solo acepta ítems de su área.
- El excedente se puede vender aquí o llevar al mercadillo.

**Producción estratégica (Tier 3–4):** carne cultivada, jardín submarino.
- Producen ingredientes raros para otras áreas. Su excedente vale más fuera que dentro.

**Progresión pura (Tier 4–5):** guardia nocturna, sección de monstruos.
- No producen ítems vendibles. La recompensa es lore, habilidades y acceso.

**Mercadillo (hub):** área dedicada de comercio, también taller productivo.
- Acepta cualquier tipo de ítem de cualquier zona.
- Comprar aquí cuesta más (prima de conveniencia por disponibilidad cross-zona).
- Vender aquí genera menos (el mercado se queda una comisión).

### Reglas de precio en talleres de zona

Cada taller solo acepta ítems que corresponden a su área (`sourceArea` del ítem).
El precio varía según el origen del ítem:

| Situación | Precio de venta |
|-----------|----------------|
| Vender ítem propio del área en su taller | Precio base |
| Vender ítem de otra zona en este taller | **Precio base × bonus de rareza** (más alto) |
| Vender cualquier ítem en el mercadillo | Precio base × comisión de mercado (más bajo) |
| Comprar en el taller de origen | Precio estándar |
| Comprar en el mercadillo | Precio estándar × prima de mercado (más alto) |

**Ejemplo concreto:**
- Un fragmento de Fósforo (P) del jardín submarino vendido en la cocina (raro allí) → bonus.
- Un fragmento de Azufre (S) del ajo vendido en la cocina → precio base.
- Cualquier ítem vendido en el mercadillo → comisión del 30% sobre precio base.

### Incentivo de movimiento

Las diferencias de precio invitan al jugador (y a los NPCs con `NPCEconomy`) a mover
ítems entre zonas estratégicamente. Un NPC en el jardín submarino puede recoger
fragmentos de P y llevarlos a la cocina para venderlos al precio premium.

Esto crea rutas comerciales emergentes entre personajes sin que el juego las imponga.

---

---

## Layout de Controles — Filosofía Mecanografía

Cold Sanctuary diseña sus controles para mantener ambas manos en posición de home row, promoviendo la postura correcta de teclado. Todos los atajos son configurables en el Inspector.

| Mano | Teclas default | Función |
|------|---------------|---------|
| Izquierda | W A S D | Movimiento |
| Izquierda | Shift izq | Sprint |
| Izquierda | Space | Salto / avanzar diálogo |
| Derecha | J / L | Cámara izq/der |
| Derecha | I / K | Cámara arriba/abajo |
| Derecha | 1 – 0 | Habilidades de combate |
| Derecha | Tab | Cycle target (mob siguiente) |
| Derecha | Enter | Avanzar diálogo (alternativo) |
| Cualquiera | V | Toggle 3ª ↔ 1ª persona |
| Cualquiera | Escape | Deseleccionar target / desbloquear cursor |

El ratón actúa como alternativa al look de teclado, no como requisito. Un jugador puede jugar completamente con teclado manteniendo ambas manos en home row.

---

## Referencia Técnica

| Sistema | Archivo |
|---------|---------|
| Tabla periódica (colección) | `Assets/Scripts/Chemistry/PeriodicTableManager.cs` |
| Fragmento de elemento (drop) | `Assets/Scripts/Chemistry/ElementFragment.cs` |
| Mob de ingrediente | `Assets/Scripts/Combat/IngredientMob.cs` |
| Tab-targeting | `Assets/Scripts/Combat/CombatTargetSelector.cs` |
| Habilidades de combate | `Assets/Scripts/Combat/CombatAbility.cs` + `CombatAbilityBar.cs` |
| Ataque del jugador | `Assets/Scripts/Combat/PlayerCombat.cs` |
| Combate autónomo NPC | `Assets/Scripts/Combat/NPCCombatBehavior.cs` |
| Miniaturización (por área) | `Assets/Scripts/Meditation/RealityShiftController.cs` |
| Misiones | `Assets/Scripts/Mission/SanctuaryMission.cs` + `MissionTracker.cs` |
| Economía base | `Assets/Scripts/Economy/CoinWallet.cs`, `Inventory.cs`, `ItemData.cs` |
| Economía NPC | `Assets/Scripts/Economy/NPCEconomy.cs` |
| Vestimenta | `Assets/Scripts/Clothing/ClothingRecipe.cs` + `ClothingCraftingArea.cs` |
| Simulación de mundo | `Assets/Scripts/World/SanctuaryDirector.cs` |
| Zonas y tareas | `Assets/Scripts/World/SanctuaryArea.cs`, `AreaTask.cs` |
