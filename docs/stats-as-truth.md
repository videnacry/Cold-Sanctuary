# Los stats como fuente de verdad (2026-08-05)

**La ley única del juego:** los **stats son la única fuente de verdad**, y **`stats → frases → todo lo demás`**
(identidad, emoción, postura, acción, diálogo interno, depredación, bond, transformación, habilidades). "Ser"
lobo/oso/hormiga/dragón es solo un **preset de stats**; la clase `Wolf`/`Bear` es una **config genérica**. Todos
los modos/hechizos (avatar, emoción, lector-de-mentes, transformación) son **el mismo motor** visto desde
ángulos distintos. Sustrato ya existente: `Anima`+`IAptitudes`+`DerivedStats`, `Mind`/`ThoughtField`/
`MindPhrase` (frases gateadas por stats), `Diet`/`PreyEntry`, `CameraManager` (shake por `mentalFatigue`).

## 1. Stats bidireccionales
Los hechizos **suben Y bajan** stats (no solo suben). Bajar es tan útil como subir: **debilitar** a un rival,
**bajarte tú** para parecer inofensivo (un **farol** sin cambiar de forma), o **subir los stats que generan
bond** en un oso salvaje para volverte de golpe "mamá/hijo/amigo" (o "enemigo"). El **control** que tenga el
lanzador (por sus propios stats) decide si mueve **un stat, un grupo o todos**.

## 2. Las proyecciones (todo es una función de stats)
| Sistema | Función de stats | Estado |
|---|---|---|
| **Emoción / postura** | `f(stats de disposición, humores actuales) → suma de frases → postura (`CreatureRig`) + `ScreenEffects`` | camera-shake ✔; resto nuevo |
| **Identidad / diálogo interno** | suma de frases del ser (`ThoughtField`/`MindPhrase`) → habla/piensa según su mezcla stat | sustrato ✔ |
| **Depredación / miedo** | `f(masa, fuerza, textura/armadura, tamaño)`; **el tamaño invierte presa↔depredador** | **hecho:** `Predation` en `SelectPrey`+`EvaluateThreat` |
| **Bond** | hechizo que sube/baja los stats-que-generan-bond → pase directo a mamá/hijo/amigo/enemigo (temporal) | nuevo, sobre bonds ✔ |
| **Transformación** | combate de stats de 3 niveles (§4) | nuevo |
| **Habilidades** | **mapa de stats**: la receta-de-acciones desbloquea un cluster (ganar fuerza ⇒ cluster distinto que ganar agilidad); el **árbol point-buy** (niveles de magia → puntos) es una **capa opcional/simple** para micro/macrocosmos | primario = emergente |
| **Avatares / worldModes** | preset de stats + **composición** (§5) como piel | nuevo |

## 3. Hechizo = modo; energía = temporizador
"Modo" y "hechizo" son lo mismo: **todo lo sobrenatural es un hechizo**. La **energía de hechizo** fija un
**temporizador** para el efecto → una economía compartida por avatar-mode / emotion-mode / lector-de-mentes /
transformación (cuánto puedes mantenerlo). El árbol de hechizos **avanza por ramas**, pero cada rama depende
de **stats concretos** (concentración, creatividad, perseverancia…). El hechizo puede avanzar también en
**customización** (transformarte en hormiga gigante o pequeña; de conservar stats a poder alterarlos).

## 4. Transformación — combate de stats (3 niveles)
Transformar **a otro** cuesta según cuánto poder le inyectas + su resistencia:
- **Coste** = cuánto subes al objetivo por encima de sí mismo (el poder inyectado) **+ su resistencia** (sus
  stats vs los tuyos).
- **Tu poder** = un stat (control/creatividad/concentración/perseverancia).
- **Niveles:** (1) poder ≥ coste completo → **cuerpo Y stats** (real); (2) poder ≥ resistencia pero < coste →
  **solo visual/avatar** (parece transformado, **conserva sus stats**); (3) poder < resistencia → falla.

**Farol vs verdad (lo potente):** los demás **huelen los stats** → el visual-only es un **farol** (huelen los
stats *reales*), el real cambia los stats (huelen "hormiga genérica de tamaño X"). Así, el **oso ante el
gusano-Kushal** reacciona a lo que **huele**, no a lo que ve; y una **hormiga con stats de apex a tamaño de
oso** aterra a los osos pero sigue siendo presa de las **Quimeras** del santuario subterráneo. (A uno mismo el
hechizo siempre puede en forma; a otros, depende del combate de stats.)

> **Hecho (scaffold):** `TransformationSpell` + `StatProfile`/`TransformPreset` (`Assets/Scripts/Transformation/`).
> `Cast(target, form)` → **Failed / VisualOnly (farol) / Full (cuerpo+stats)** según potencia (compostura+
> disciplina+creatividad + energía) vs coste (resistencia + inyectado por `Might`), con **revert** por duración.
> Bidireccional (bajar stats = debilitar). Se cablea en `Anima`s reales (sin sandbox: `Anima` es abstracta).
> **Pendiente:** ligar la **duración a la energía del hechizo**; que la **depredación "huela"** el resultado
> (visual-only = farol → leen stats reales); customización giant/small ya cubierta por `visualScale`+`bodyMass`.

## 5. Composición por componentes (patrón CodeShip)
Un personaje = **cuerpo base + partes slotables**; cada parte aporta **{malla, posición/escala, stats
opcionales}** (igual que propulsores/alas → nave). Dos capas **ortogonales**:
- **`CreatureRig`** (hecho) = el **esqueleto móvil** (parte lógica → hueso; auto desde `HumanBodyBones` para
  humanoides, manual para insectos/quimeras). Lo mueven yoga/emoción.
- **Composición** (por hacer) = las **partes que se ponen** (peinado/cejas/ojos/ropa) = piel + stats.

**Principio de identidad:** la identidad (los **adornos**) vive en el **alma** (`SoulRecord`), no en el modelo;
el cuerpo (hormiga/humano/dragón) es una **base intercambiable** que el alma "pinta" → al transformar, la
hormiga-Sakshi convertida en humana **conserva su peinado/ropa** y es reconocible (enlaza con el *tell* de la
reencarnación). **Necesitamos avatar humano por ancla** (no uno por hormiga; solo los personajes-ancla).
**Empezar por un slot: peinado**; luego cejas/ojos/ropa; a futuro cada parte con stats (**ropa=defensa ya
existe:** `ClothingRecipe.defenseRating`/`ClothingSlot`).

> **Fases 1–2 hechas:** `CharacterComposition` + `CompositionPart`/`StatBonus` (`Assets/Scripts/Composition/`).
> Partes slotables (adornos/ropa/**miembros**) → **visual** + **stats**. Modelo: la **constitución** (química/base
> = los campos de `Anima`, que evolución/transformación mutan) es el HUÉSPED; cada parte aporta un **delta
> GESTIONADO** (resta viejo/suma nuevo por frame) → **no pisa** evolución/transform (por eso es seguro). El
> aporte **biológico** se **modula por la vitalidad** del huésped (el mismo brazo rinde distinto según el
> cuerpo); **injerto progresivo** (`adaptSpeed`: el delta converge/decae). La armadura (ropa) suma a
> `Anima.armadura` (que `Predation` lee) sin modular. Reutiliza `ClothingSlot`(+Hair/Eyebrows/Eyes)/`ClothingRecipe`.
> **Fase 3:** reconciliar con `BodyPartStats`; miembros perdibles/injertables como assets; **base proyectada
> desde `Humores`/`Chemistry`** (la química real); identidad (adornos) en el `SoulRecord` para que viaje al transformar.

## 6. Legibilidad — load-bearing, no adorno
Si los seres reaccionan a **stats ocultos**, el jugador **debe poder percibirlos**: el **lector-de-mentes** /
**señales posturales** de animales / **objetos luminosos** (identificar recursos) es la **capa de lectura** de
todo el mundo stat-verdad. Sin ella, las reacciones emergentes parecen arbitrarias. Facilita crear **vínculos**
con crías y adultos (leer qué sienten/quieren/van a hacer). Es un **hechizo** más (con su temporizador).

## 7. Las Quimeras (santuario subterráneo)
Experimentos de la magnate = **presets de stats extremos con piel elemental**: dragón de fuego (masa/fuerza/
fuego), hidra-relámpago (velocidad/regeneración), elementales, etc. El nombre es piel; **el stat dominante es
la verdad**. Paraguas: **"Quimeras"**.

## 8. Plan incremental (rebanadas)
1. **`CreatureRig`** ✔ (esqueleto móvil central).
2. **Emotion-slice:** `stats+humores → frases → postura (`CreatureRig`) + `ScreenEffects``; refactor
   `UpaYogaSession` a pedir partes al rig (`rig.Get(BodyPart.Neck)`).
3. **Terminar `ScreenEffects`** (cámara artística: cansancio/sueño/excitación).
4. **Composición:** slot de **peinado** → extender partes → partes con stats (ropa=defensa).
5. **Depredación por stats** ✔ `Predation` (masa/fuerza/textura/tamaño; el tamaño invierte presa↔depredador;
   el farol no engaña, la transformación real sí) en `SelectPrey`+`EvaluateThreat`. **Manada** (`EffectivePower`:
   poder de aliados por facción) ✔ y **aura mágica** (`Anima.magicAura`+`MagicAura`: destructiva→temida,
   benevolente→bonds fáciles) ✔; falta que la magia llame a `MagicAura.Register*`.
6. **Hechizos** (transformación 3-niveles, bond por stats, lector-de-mentes) sobre `PossessionSpell` + energía=timer.
7. **Consumibles/monetización:** gratis + donaciones; cosméticos (avatar de león) = preset-skin, **al final**.
