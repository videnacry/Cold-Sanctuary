# Segunda vuelta por los santuarios + el miedo como efecto de hechizo

**Estado (2026-08-17):** diseño/lore. Captura una sesión de brainstorming. Las especies/biomas marcados
*(propuesta)* están **por confirmar**; las mecánicas son la dirección acordada (implementación futura).

## 1. El MIEDO como efecto de HECHIZO (no un aura pasiva)

Hoy el miedo llega por `Anima.magicAura` (destructiva → temida, `MagicAura`) que lee `ThreatResponder.Assess`
(`auraFear`). **Objetivo:** que el miedo lo genere **cada hechizo**, con su propia **magnitud** (∝ potencia) y
**área** (AoE):

- Al lanzar, el hechizo emite un "pulso de miedo" en su AoE. Cada `Anima` dentro lo siente **escalado por su
  temple/poder** (`composure`/`Predation.PredatorPower`): un **lobo** poco, un **conejo** mucho.
- El miedo **sube `stress`** y **alimenta un BOND NEGATIVO** hacia el lanzador (el que te aterra te cae peor →
  `SpeciesKarma`/bonds ya soportan negativo).
- `Jalar` (PullSpell) = poco miedo, área pequeña. **`Fuego` (FireSpell) = aterrador para todos** según magnitud.
- `auraFear` pasa a ser propiedad **del hechizo** (cuánto miedo emite), no solo del receptor.

**Añadir al plan (futuro):** poder **controlar el AoE y la potencia** de un hechizo (hoy fijos). Con eso, el
mismo Fuego puede ser una chispa inofensiva o un pulso que aterra a media pradera.

## 2. Bloqueo de hechizos letalmente aterradores (hechizo GLOBAL de la Magnate)

Un hechizo global creado por la Magnate protege a los animales. Cuando un hechizo superaría un **umbral de terror
letal** sobre animales protegidos:

1. El hechizo se **CANCELA** (no llega a afectar a los animales).
2. El **lanzador recibe daño** (pérdida de stats).

Enseña **control**: proteger a los débiles es la lección de la segunda vuelta.

## 3. Progresión V1 → V2 (inversión de poder)

- **V1 — fortalecerse:** los animales son **más fuertes que los hechizeros**. Estímulo: rodearte de criaturas
  superiores para crecer. Para pasar de santuario hay que **demostrar ser más fuerte que TODOS sus animales**. Los
  animales se **entrenan/amplifican**: en `4santuaryV1` incluso un **conejo** tiene poder de **dragón**.
  `1santuary` es lo más cercano a la realidad; S2/S3/S4 son **amplificaciones crecientes**.
- **Guerra 1:** algunos hechizeros **entrenan** a las criaturas de cada santuario como defensa poderosa. Tras la
  guerra, esas criaturas **se marchan con sus entrenadores** y se traen **animales nuevos** (siempre en peligro de
  extinción → el santuario siempre alberga especies amenazadas).
- **V2 — controlar (proteger al débil):** se **invierte** el objetivo. Los animales V2 son **progresivamente más
  débiles** (`1V2 > 2V2 > 3V2 > 4V2`) y los **hechizos del jugador más fuertes** (crecen por santuario). Así
  `4santuaryV2` = hechizero **>2× más fuerte** que en `1V2` con animales **mucho más débiles** → obliga a un
  **control extremo** (micro-hechizos; nada aterrador, o el bloqueo de la Magnate te castiga).
- **Guerra 2:** Kushal propone **NO entrenar** a los animales (dejarlos débiles) para que los hechizeros
  atacantes **no puedan usar sus hechizos más poderosos** (el bloqueo los penaliza) → **ventaja defensiva** por
  proteger, no por potencia.

## 4. Temáticas por santuario (V1 → V2)

Reconstruido con magia, cada santuario **cambia de bioma en V2**, adaptado para nutrir al siguiente grupo de
especies en peligro. Regla: los biomas de un santuario no se repiten en otro.

| Santuario | Elemento | V1 (1ª vuelta) | V2 (2ª vuelta) |
|---|---|---|---|
| **S1** | Tierra | **Hielo/polar** | **Era de Hielo** — megafauna extinta (mamut, dientes de sable, perezoso gigante…) |
| **S2** | Agua | **Aguas someras** | **Abisal** — laboratorio superavanzado en lo más profundo + **burbuja "modo prehistórico"** |
| **S3** | Aire | **Montaña** | **Ciudad flotante superavanzada** — burbuja de aire artificial |
| **S4** | Fuego | **Sabana** | **Volcán/geotermal** — quimeras + caracoles de fuego |
| **S5 (núcleo)** | — | (plasma+diamante) | **Santuario de la Magnate** — desierto artificial; ver §6 |

**Detalle de las V2 (reconstruidas con magia, cada una un ecosistema-refugio):**

- **S3 · Ciudad flotante (aire):** vista de lejos es una **esfera** = el **área de aire artificial** que genera el
  santuario. Alberga a los **insectos GIGANTES más grandes que hayan existido** (recreados; el aire especial del
  santuario los sostiene). No salen: **notan que el aire de fuera no les sirve** y, sobre todo, el santuario es su
  **paraíso**. Idea rectora: cada V2 debe ser un edén para su tipo de fauna.
- **S2 · Abisal (agua):** un **laboratorio superavanzado** en el fondo + su **esfera/área en "modo prehistórico"**:
  **megalodón**, **reptiles marinos** (dinosaurios acuáticos) y el **calamar gigante prehistórico** (mayor que el
  actual).
- **S4 · Volcán (fuego):** hogar de las **quimeras** y los **caracoles de fuego** (encaja con el *scaly-foot* de
  ventilas, real y amenazado).
- **S1 · Era de Hielo (tierra):** en vez de un bioma vivo distinto, la 2ª vuelta del santuario de hielo trae de
  vuelta la **megafauna extinta del Pleistoceno** — **mamut lanudo, tigre dientes de sable, perezoso gigante,
  rinoceronte lanudo, lobo gigante (dire wolf)**. Mantiene la identidad de frío y sigue el patrón "V2 = gigantes
  extintos" del resto de santuarios.

**Cuarteles de los jefes = Australia + Amazonía** (localización propia, no un santuario V1/V2). Cubren la fauna
australiana y la selva amazónica. Con Amazonía aquí, la **selva** no hace falta como V2 de S1.

**Ecosistemas y posibles huecos:** el **arrecife de coral** encaja como **S2V1** (aguas someras, biodiverso y
amenazado). Candidatos aún sin ubicar si se quiere ampliar: **humedal/manglar** (cocodrilo, manatí, capibara),
**taiga/bosque templado** (casi el bioma "de partida"). *(Por decidir si entran.)*

## 5. Especies *(propuesta, a confirmar)* — todas en peligro real

Hoy en `1santuary`: **oso, lobo, conejo, foca, ballena** (+ existen zorro, ciervo, malamute).

- **S1 Hielo *(CONFIRMADO):*** oso polar · lobo (ártico) · foca · ballena · **conejo → liebre ártica** · **+
  pingüino** · **+ leopardo (de las nieves)**.
- **S1 Selva (V2):** tigre de Sumatra · orangután · jaguar · tapir · guacamayo.
- **S2 someras:** tortuga marina · dugongo/manatí · foca monje · tiburón. **abisal (V2):** cangrejo yeti ·
  **caracol de ventilas** (scaly-foot / *Chrysomallon* — real y amenazado, encaja con el snail que ya hay) ·
  peces abisales · calamar.
- **S3 montaña:** leopardo de las nieves · cóndor · panda rojo · íbice. **cielo (V2):** águila filipina · cóndor
  de California · grulla.
- **S4 sabana:** elefante · león · rinoceronte negro · guepardo · hipopótamo. **volcán (V2):** extremófilos ·
  insectos · caracol de ventilas. *(volcán tiene poca fauna real → V2 va bien con lo amplificado/mágico)*

## 6. El santuario de la MAGNATE (núcleo: plasma + diamante) — desierto y las puertas

El 5º santuario (el **núcleo de la Tierra**, capa de plasma + capa de diamante) es el de la **Magnate**. Sobre el
**diamante** hay un **desierto** artificial: el **día/noche son artificiales** y el "sol/luna" es en realidad el
**núcleo de plasma** central. Es el **lugar más peligroso de todos** — restringido, peligroso **incluso para los
demás jefes**.

**Lore — cielo e infierno:** la Magnate ha logrado **interactuar con el cielo y el infierno**. El **desierto es la
puerta al INFIERNO**; el **núcleo de plasma es la puerta al PARAÍSO**. Sus criaturas nativas son como **demonios**:

- **Demonios** (de otra dimensión): poder **superior a cualquier hechizero**, aparentemente **ilimitado para
  alterar la realidad**. Quien se acerca **experimenta todo lo que existe** a la vez — mezcla las vivencias más
  dolorosas y las más placenteras en **milisegundos** → se predice que un **anima mortal se desvanecería al
  instante**, solo por su **aura**.
- **Ángeles** (hipótesis; nadie ha visto uno): generarían el **vacío absoluto** → quien se acercase **se
  transformaría literalmente en vacío**.

**Arco de Kushal:** cuando la Magnate lo **elige** para ir con ella a este santuario a **crear quimeras**, Kushal
**no puede defenderse de ninguna manera**. La supervivencia = **mantenerse lejos** de estos seres → usa mucho
**hechizos de tipo RADAR** e **investiga sus patrones** para evitarlos. Además de las misiones de la Magnate, tiene
la **misión secreta**: **neutralizar** a estas criaturas o **ponerlas en su contra** para **derrotarla** junto a
los demás jefes.

## 7. Amplificación de poder = hechizo `bonusPack` (no un multiplicador pasivo)

La amplificación V1 (p. ej. el "conejo con poder de dragón") **no** es un multiplicador estático de santuario: es un
**evento de ENTRENAMIENTO**. En la **guerra de `1santuaryV1`**, los hechizeros presentes **entrenan a todos los
animales** hasta el poder de las criaturas de `4santuaryV1` → se modela **aplicándoles un `bonusPack` de tier alto**
(vectores aditivos ya escalonados `bonusPack1..4`). "Nivel de 4V1" = un tier concreto.

- **Estado:** los `bonusPack` se aplican hoy vía `SoulComposition` (seres compuestos). Los **animales** (jerarquía
  `Animal`) aún no tienen esa vía → la ganan al **migrarlos a composición** (disolución de `Animal`) o con un
  gancho `bonusPack` en `Animal`. El mecanismo existe; falta enchufarlo a los animales.

## Australia (pendiente de ubicar)

Australia "merece su propio lugar". Opción en valoración: los **cuarteles de los jefes** podrían ser ese santuario.
*(Por decidir.)*

## Preguntas abiertas (para confirmar)

- **S1 → V2:** ¿la selva tropical va bien como 2ª vuelta del santuario de tierra? ¿Especies de la selva V2?
- **Australia:** ¿cuarteles de los jefes, o un santuario propio? ¿En qué elemento encaja?
- ¿Se enchufa `bonusPack` a `Animal` ya (gancho pequeño) o se espera a la migración a composición?
