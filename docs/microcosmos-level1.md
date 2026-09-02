# Microcosmos — Nivel 1 (El alba / la cueva): consolidación + fichas

Doc de **consolidación** del Nivel 1 (enriquecer + unir las dos versiones existentes) y **fichas** del elenco.
Fuentes de verdad: [`novela.md`](novela.md) (tono, manda en la ficción), [`microcosmos-insects.md`](microcosmos-insects.md) §13
(mapa de almas), [`emotion-model.md`](emotion-model.md), [`anima-architecture.md`](anima-architecture.md).

> **Arquitectura de escenas (PR #174):** el microcosmos es su **propio plano** → sus niveles son **escenas separadas**,
> NO objetos dentro de la escena del Santuario 1 (mesocosmos). **Scene1 (Ambrosio / el alba)** es esta — se genera con
> `Tools ▸ Cold Sanctuary ▸ Build Microcosmos Scene1 (Ambrosio)` (`MicrocosmosSceneBuilder` → `Microcosmos_Scene1_Ambrosio.unity`).
> **Mesopotamia** (`MobWorldSceneBuilder`, la ciudad-insecto) también **es del microcosmos**; Scene1 es **anterior** en la
> historia → son **escenas hermanas**. Reutilizan los builders `BuildMicrocosmosSandbox`+`BuildNivel1Sandbox` (ahora en su
> escena, ya no en el mesocosmos). Falta (follow-up): offset de posición si se cargan additive a la vez + entrada del jugador.

## Principio rector: emergencia dirigida (por stats + thoughts, NO marioneteo)
A los personajes **los mueven sus stats + su Mente** (thoughts/`Humores`/`ThoughtField`). El "director" de la
historia **no mueve a nadie**: solo **siembra circunstancias y pensamientos** (patrón ya existente:
`MobWorldDirector`, "mundo vivo por eventos"). Cada beat = introducir un **estímulo** (aparece alguien, Ambrosio
se agota) y/o **sembrar un deseo/pensamiento**; el personaje decide solo. Así "parece que actúan por su cuenta"
y a la vez recrean lo prescrito.

## Arquitectura: composición, NO una clase `Ant`
Todo ser es un **`Anima`** (raíz única, 12 aptitudes + drives). Los pilares son **componentes** (`Mind`,
emoción, etc.), no herencia; no hay clase `Soul`/`Body` (el alma = `SoulRecord` + `SoulMarga`/`CharacterLevel` +
`DerivedStats`). Por eso una hormiga = **`SimpleAnima` + composición**, diferenciada por **datos (ficha)**:
```
Hormiga = SimpleAnima
        + [IA emergente del compañero] ImpulseController · HomeImpulse · ThreatScanner · PackAwareness · WeaknessEffect
        + [identidad/mente]           Mind · SoulRecord · EmotionExpression
        + [datos]                     perfil de aptitudes · relaciones (bonds) · impulsos sociales dominantes
```
La "hormiguidad" compartida = una **receta de montaje** (`MakeAnt(...)` en el builder), no una clase. Los 7
personajes NO son 7 clases: son la misma receta con **fichas** distintas. (Ambrosio es la excepción: es un
**pulgón**, no hormiga → `HoneydewProducer`.)

## Impulsos sociales que faltan (aditivos al `ImpulseController`)
Nuevos `MovementImpulse` derivados de **stats + bonds + thoughts** (no rompen la IA emergente, la enriquecen):
| Impulso | Deriva de | Efecto |
|---|---|---|
| `FollowImpulse` | bond con la tribu · sociability | seguir al grupo/líder |
| `TendImpulse` | bond alto + `sensibilidad` + deseo "ayudar" | acercarse y **cuidar** al vinculado (trofalaxis) |
| `AdoreImpulse` | bond máximo + serotonina | orbitar/servir al adorado (Medea→Ambrosio) |
| `ObserveImpulse` | `perception`/`reasoning` altas | **detenerse a mirar** (Sakshi se queda "varada") |
| `CullImpulse` | discipline/composure · afabilidad baja | dejar atrás a las débiles (Héspero) |
| `GatherImpulse` | endurance + rol | recolectar/acarrear (Ruth → hongo) |
| `ObeyImpulse` | bond-jerárquico + discipline | obedecer al líder aun contra el querer (Ruth↔Héspero) |
| `GriefImpulse` | pérdida de un bond fuerte | histeria/llanto/aferrarse (clímax) |
La **cohesión de manada** (contador) baja cuando el `FollowImpulse` de alguien pierde repetidamente contra otro
(Tend/Observe) → dispara el **abandono** de forma legible.

## Fichas del elenco
Formato: **alma** (`SoulRecord`) · **aptitudes marcadas** (de las 12 + afabilidad/sensibilidad) · **firma
emocional** (`Humores`/tono) · **relaciones** · **impulsos dominantes** · **rol/beat** · **tell**.

### Sakshi — hilo **F** (Aliento y Mente) → *El Chamán*
- **Alma:** observadora de la mente; crió a Ambrosio. **Aptitudes:** `perception`↑↑, `reasoning`/`memory`↑,
  `sensibilidad`↑↑; `agility`/`strength`↓ (débil, lenta); `sociability`↓ (no encaja).
- **Emoción:** tono Viento/Agua; introspectiva; **se congela mirando hacia dentro** (freeze). **Relaciones:**
  bond fuerte→Ambrosio; bond↓ con la tribu. **Impulsos:** `ObserveImpulse` + `TendImpulse`(Ambrosio) > `Follow`.
- **Rol/beat:** halla al pulgón deforme → lo cuida → **la tribu la abandona** (su Tend gana al Follow).
- **Tell:** quedarse quieta mirando; la pausa/el aliento. **Ironía (parkeada):** hace lo mismo que la tribu
  —empujar al que no encaja.

### Ambrosio — **pulgón**, sin hilo (centro sagrado) → *Nasatya*
- **Alma:** mártir nutridor; cuerpo **blando, panzudo, grande**; vive al extremo (como un perro de 18 años).
  **Aptitudes:** `bodyMass`↑↑, `agility`↓ (lento, fácil de ignorar); `endurance` cae cerca del final.
- **Emoción:** manso, sereno (serotonina↑, adrenalina↓); tono Tierra/Agua. **Relaciones:** **nutre a todos**
  (melaza/trofalaxis); bonds con Sakshi (cuidadora), Medea (adoración), la tribu. **Impulsos:** dar melaza; casi
  inmóvil. **Rol/beat:** nutre → **se agota** (`HoneydewProducer.interval`↑ = "días sin jugos") → **colapsa cerca
  de la cueva** → muere aliviado de haberlos puesto a salvo. **Tell:** cuerpo grande manso y fortísimo, postura rara.

### Medea — hilo **B** (El Tallador) → **E**; *tirana del veneno/feromonas* (héroe→villano)
- **Alma:** gemela **débil**; adora a Ambrosio; forja veneno/armas; **se endurece** tras la muerte.
  **Aptitudes:** `strength`/`bodyMass`↓ (más pequeña de lo normal), `discipline`↑↑ (adicta al trabajo/estudio),
  `creativity`/`reasoning`↑ (forja veneno), `sensibilidad`↑ (adoración→histeria).
- **Emoción:** serotonina↑ hacia Ambrosio → tras la muerte cortisol/resentimiento, **endurece**; tono Fuego.
  **Relaciones:** bond MÁXIMO→Ambrosio; a Atlas lo aparta. **Impulsos:** `AdoreImpulse`(Ambrosio) · Work; luego
  Harden. **Rol/beat:** adora → **no logra levantar** al Ambrosio muerto (es grande) → histeria *"no me dejes /
  llévame contigo"* → endurece justo cuando la tribu al fin se acercaba. **Tell:** la adicción al trabajo; el veneno.

### Momo — hilo **G** (El Bromista) → *el bufón* (**no reencarna: sigue siendo Momo**)
- **Alma:** bromista magnética; **sisa** comida/baratijas; se queda pese a los ruegos. **Aptitudes:**
  `sociability`/`afabilidad`↑↑ (magnetismo), `creativity`↑, **`strength` latente** (levanta a Ambrosio sin
  esfuerzo), `discipline`↓ (nada en serio — *aparente*).
- **Emoción:** positividad↑ (juego) que **oculta** profundidad; tono Viento; el **llanto = ruptura de la
  máscara**. **Relaciones:** magnetismo con todos (la tribu **perdona a Medea con tal de tener a Momo**).
  **Impulsos:** Play/Steal · Charm; en el clímax → `GriefImpulse`. **Rol/beat:** se queda con el grupo; en la
  muerte **se desborda en llanto** y **levanta a Ambrosio sin esfuerzo**; toma un "trono" infantil (Atlas lo
  enmienda). **Tell:** sigue siendo el bufón.

### Héspero — hilo **A** (Guardián del Fuego, **constante**) → *Señor del Fuego* (villano→héroe)
- **Alma:** vigía de las estrellas; líder que **abandona a las débiles**. **Aptitudes:** `perception`↑ (vigía),
  `discipline`/`composure`↑ (rígido), `afabilidad`↓ (frío), `strength` media.
- **Emoción:** contenido/rígido; **"un brillo de dolor"** al ver caer a Ambrosio → **se le afloja la quijada y
  se voltea** (grieta en la coraza); tono Fuego. **Relaciones:** lidera la tribu; deja atrás a las débiles;
  respeta a Ambrosio (nutrió a algunos suyos). **Impulsos:** Lead · `CullImpulse` · Cohesion. **Rol/beat:** su
  tribu **abandona al grupo** (4 desertan); **presencia la muerte y se ablanda** (semilla villano→héroe).
  **Tell:** la rigidez; el fuego.

### Ruth — hilo **C** (La Recolectora) → *La Sembradora*
- **Alma:** recolectora **sumisa**; acarrea **hongo**; **la última en comer**; quiere a Ambrosio pero **no puede
  desobedecer a Héspero**. **Aptitudes:** `endurance`↑ (acarrea), `discipline`↑ (obediente), `afabilidad`↑
  (cariño), `sociability` sumisa.
- **Emoción:** cálida pero **reprimida** (obedece contra su querer); tono Tierra/Agua. **Relaciones:**
  `ObeyImpulse`(Héspero) **>** Care(Ambrosio) → conflicto interno. **Impulsos:** `GatherImpulse`(hongo) · Obey ·
  Care reprimido. **Rol/beat:** vela desde la cueva; su hongo alimenta; el conflicto obediencia↔cariño se lee.
  **Tell:** acarrear/recolectar; el hongo.

### Atlas — hilo **E** (Corona y Espada) → *fuerza benévola / orden justo* (abierto)
- **Alma:** **el más fuerte**; sostén/enmendador; **pilar de Medea**. **Aptitudes:** `strength`/`bodyMass`↑↑,
  `endurance`↑, `composure`↑, `discipline`↑, `afabilidad`↑ (consuela).
- **Emoción:** firme, protector, sereno; tono Tierra. **Relaciones:** sostiene a Medea (ella lo aparta);
  enmienda a Momo. **Impulsos:** Protect/Support · Order. **Rol/beat:** intenta **consolar a Medea** (rechazado);
  **enmienda el "trono" infantil** de Momo. **Tell:** la fuerza benévola; sostener.

### Los ancianos (banda de Ambrosio) + el anciano-pintor
- **4 ancianos** que **desertaron** para quedarse con el grupo: frágiles (`WeakOne`), en el clímax **no pueden ni
  moverse** → son los que Kushal debe **guiar al refugio** (Viaje 3). **Anciano-pintor** (hilo **D** arte,
  *abierto*): el primero que **marca la cueva** (primer arte) — o lo lleva Ambrosio (decisión pendiente).

## Beats del Nivel 1 (lo que el "director" programa como circunstancias)
1. **Sakshi observa** (ObserveImpulse la para) → halla al **pulgón deforme Ambrosio** (estímulo colocado).
2. **Cuidado**: Sakshi lo tiende (TendImpulse) hasta que **sana**; su Follow pierde contra Tend → **cohesión↓**.
3. **Abandono**: la tribu la deja (cohesión bajo umbral) — legible por emoción/`ThoughtField` (resentimiento).
4. **La familia elegida**: aparecen **crías supervivientes** (estímulo) → las gemelas **Medea/Momo**; Ambrosio
   las nutre (trofalaxis).
5. **La tribu de Héspero** (estímulo): se unen; Ambrosio nutre a algunos; Momo magnetiza; Medea rebelde tolerada.
6. **La deserción**: la tribu abandona; **4 ancianos desertan**; Momo se queda.
7. **Clímax / muerte**: Ambrosio agotado **colapsa cerca de la cueva**; Medea no logra levantarlo → histeria;
   Atlas rechazado; **Momo llora y lo levanta**; Héspero se ablanda; **muerte**; Medea endurece; Momo "trono".
   → aquí **se consolida el tableau** (`MicrocosmosSandbox_AUTO`) como **beat final** del mapa jugable.
- **Papel de Kushal (jugador):** en Viaje 3 **guía a los ancianos al refugio** mientras el área se "desinfecta"
  (guiar/pastorear con `PullSpell`, alma compartida `HelpRequest`, seguir `FollowBrain`, poseer `PossessionSpell`).

## Mecánicas que faltan (resumen para construir)
> **Construyéndose POR REBANADAS** (el usuario lo pidió, 2026-09-02). Estado abajo.
- **Capa de identidad** (fichas → datos): `SoulRecord` + `Mind` + perfil de aptitudes + relaciones. *(aditivo)*
  → **✅ REBANADA 1 (PR #176)** para el ELENCO del tableau (`BuildMicrocosmosSandbox`, la receta `Cast(...)`): cada
  personaje es ya un **Ánima real** (`SimpleAnima` + `Mind` con las aptitudes de su ficha + su **voz** como pensamiento
  `Vivencia`) además del `SoulRecord`. Falta: **relaciones** por-individuo (hacia Ambrosio/Héspero…) — irán con los
  impulsos (las usan Tend/Adore/Grief); y aplicar lo mismo a las hormigas del **mapa jugable** (coordinar con el compañero).
- **Impulsos sociales** (`Tend/Adore/Observe/Cull/Gather/Obey/Grief/Follow`) como fuentes de impulso (sobre `ImpulseController`). → **rebanada 2** (pendiente).
- **Cohesión de manada + tolerancia/abandono** (contador legible). → **rebanada 2** (pendiente).
- **Director de circunstancias** (`Level1Director` estilo `MobWorldDirector`): programa los estímulos/beats y
  siembra pensamientos/`ThoughtField`; **no mueve a nadie**. → **rebanada 3** (pendiente).
- **Enganchar emoción** (`EmotionExpression`/`EmotionReader`) a las hormigas (legibilidad). → **rebanada 3** (pendiente).
- **Consolidar** el tableau de la muerte como beat final del mapa jugable (unir los dos sandboxes). → **rebanada 4** (pendiente).

## Orden de construcción sugerido
1. **Este doc** (fichas + impulsos + beats) — ✅ hecho.
2. **Capa de identidad**: fichas (SoulRecord + Mind + perfiles) → ✅ **hecho para el elenco del tableau** (receta `Cast`,
   PR #176). Pendiente: aplicar a las hormigas del mapa jugable vía `MakeAnt(...)` (coordinar con el compañero: es su mapa).
3. **Impulsos sociales** + cohesión/abandono. ← **siguiente rebanada**.
4. **`Level1Director`** (circunstancias/beats) + enganchar emoción.
5. **Consolidar** el tableau como beat final.
