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
| **S1** | Tierra | **Hielo/polar** | **Selva tropical** |
| **S2** | Agua | **Aguas someras** | **Aguas profundas/abisal** |
| **S3** | Aire | **Montaña** | **Flotante/cielo** *(desierto y montaña = viento perceptible)* |
| **S4** | Fuego | **Sabana** *(o Australia)* | **Volcán/geotermal** |

## 5. Especies *(propuesta, a confirmar)* — todas en peligro real

Hoy en `1santuary`: **oso, lobo, conejo, foca, ballena** (+ existen zorro, ciervo, malamute).

- **S1 Hielo:** oso polar · foca (anillada de Saimaa / monje — críticamente amenazadas) · ballena (narval/beluga)
  · **+ pingüino** (emperador) · **+ leopardo de las nieves** *o* tigre de Amur/siberiano. *(el conejo → liebre
  ártica, o se retira a favor del pingüino)*. **Recomendación:** dejar oso/foca/ballena/lobo (lobo ártico) y
  **añadir pingüino + leopardo de las nieves**; el conejo pasa a liebre ártica o se va.
- **S1 Selva (V2):** tigre de Sumatra · orangután · jaguar · tapir · guacamayo.
- **S2 someras:** tortuga marina · dugongo/manatí · foca monje · tiburón. **abisal (V2):** cangrejo yeti ·
  **caracol de ventilas** (scaly-foot / *Chrysomallon* — real y amenazado, encaja con el snail que ya hay) ·
  peces abisales · calamar.
- **S3 montaña:** leopardo de las nieves · cóndor · panda rojo · íbice. **cielo (V2):** águila filipina · cóndor
  de California · grulla.
- **S4 sabana:** elefante · león · rinoceronte negro · guepardo · hipopótamo. **volcán (V2):** extremófilos ·
  insectos · caracol de ventilas. *(volcán tiene poca fauna real → V2 va bien con lo amplificado/mágico)*

## Preguntas abiertas (para confirmar)

- ¿Especies exactas por santuario (de la propuesta §5)? ¿Se retira el conejo o pasa a liebre ártica?
- **S1:** ¿hielo → selva? **S4:** ¿sabana → volcán, o **Australia** en algún tramo?
- **S3 aire:** ¿montaña (V1) → cielo/flotante (V2)? ¿El desierto entra en aire o en fuego?
- ¿La amplificación de poder V1 (conejo con poder de dragón en 4V1) se modela como un **multiplicador de santuario**
  sobre los stats base de la especie?
