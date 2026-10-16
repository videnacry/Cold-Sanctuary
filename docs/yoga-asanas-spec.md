# Ficha de asanas (fuente de verdad anatómica)

Esta ficha describe cada postura con **precisión profesional** (respiración, mirada, apoyos, colocación
de manos y pies, columna). Es la **referencia** que las posturas en código (`Assets/Scripts/Asana/YogaSpells.cs`
y `UpaYoga.cs`) aproximan. El rig de cubos (`CubeHumanoid`) coloca los miembros **rotando articulaciones**
(cinemática directa), así que la colocación fina (manos exactamente junto a los pies, talones que se tocan)
es aproximada; lo exacto es la **secuencia, la respiración y los grupos que se mueven**.

## Convención del rig (ejes, verificada en `CubeHumanoid`)

- Ejes **alineados al mundo** en reposo: `X` = derecha, `Y` = arriba, `Z` = adelante.
- Los miembros **cuelgan hacia −Y**; el torso apunta a **+Y**.
- **Hombro** (brazo entero): `X−` sube el brazo **por delante** (a −175 queda vertical arriba, **paralelo** al otro →
  hueco entre manos = anchura de hombros); `X+` lo lleva atrás. `Z` lo abre/cruza lateralmente.
- **Columna** (`Back` lumbar 55% + `Chest` torácica 45%): `X+` flexiona adelante; `X−` extiende (backbend).
- **Muslo**: `X−` adelante, `X+` atrás. **Espinilla**: `X+` dobla la rodilla.
- **Vector3 = 3 ejes disponibles** por hueso; hoy la mayoría de poses usan 1–2 (por eso la precisión es media).

## Lo que el rig NO expresa aún (pendiente, requiere mejora)

1. **Pies juntos / talones que se tocan** (Tadasana y todas las de pie del Saludo al Sol): la anchura de piernas es
   la **posición** de las caderas en `CubeHumanoid` (±0,1), igual para todas las poses. Hace falta una **anchura de
   stance por postura** (mover la raíz de las piernas o aducir), que no existe. *Propuesta:* añadir `stanceWidth` a
   `AsanaPoseSpell` que desplace `ThighLeft/Right` o use `CubeJoint` para acercar los pies.
2. **Manos exactamente junto a cada pie** ("diez dedos sobre el suelo", Uttanasana) y **palmas que se tocan**: es un
   problema de **alcance** (longitud de brazo vs. pliegue), no de un único ángulo. *Propuesta:* una capa de **IK/alcance**
   ligera (apuntar la mano a un objetivo) para las manos y los pies; mientras tanto se aproxima con rotaciones.

Si quieres, implemento (1) y (2) como un subsistema aparte; con eso las posturas pasan de "aproximadas" a "exactas".

---

## Saludo al Sol (Surya Namaskar A) — 12 asanas

Respiración: una por transición (inhala al abrir/subir, exhala al plegar/bajar). Mirada = *drishti*.

1. **Pranamasana** (oración). De pie, **talones juntos**, peso repartido. **Palmas juntas en rezo (Anjali mudra) frente al
   esternón**, codos abajo. Columna neutra. Mirada al frente. *Rig:* hombros aducidos + codos flexionados → manos al centro.
2. **Hasta Uttanasana** (brazos arriba). Inhala. Brazos **extendidos arriba, a la anchura de los hombros** (hueco entre
   manos y entre codos = distancia entre hombros), suave extensión de columna, mirada a las manos. *Rig:* `ArmsUp` por
   flexión (X−175) = paralelos y a la anchura de hombros. ✓ corregido.
3. **Uttanasana** (flexión / "diez dedos al suelo"). Exhala. **Bisagra de cadera** (no redondear), torso sobre las piernas
   rectas, **manos al suelo junto a cada pie**, talones juntos, mirada a las espinillas. *Rig:* flexión + brazos colgando;
   el contacto exacto manos-suelo necesita la capa de alcance (ver pendiente 2).
4. **Ashwa Sanchalanasana** (zancada). Inhala. Pie derecho **atrás**, rodilla al suelo; pie izquierdo **entre las manos**,
   rodilla sobre el tobillo (~90°); pecho abierto, mirada arriba. *Rig:* `Lunge` (rodilla delantera 95°, trasera extendida).
5. **Dandasana** (plancha). Retén. Cuerpo en **línea recta** del talón a la corona, hombros sobre las muñecas, abdomen
   firme, mirada al suelo. *Rig:* `Hips` ~72 (casi horizontal), brazos verticales; pies con dedos recogidos (~+25, no +50).
6. **Ashtanga Namaskara** (ocho miembros). Exhala. Tocan **rodillas, pecho y barbilla**; **cadera ELEVADA** (piqueada);
   codos pegados al cuerpo. *Rig:* cadera alta (`Hips` 48 + root bajo), pecho abajo (Spine adelante), codos doblados. ✓ corregido.
7. **Bhujangasana** (cobra). Inhala. Tumbado, pubis y empeines en el suelo, **pecho arriba por extensión** de la espalda,
   hombros lejos de las orejas, mirada arriba. *Rig:* prono (`Hips` 90) + `Spine(−60)` backbend + manos de apoyo.
8. **Adho Mukha Svanasana** (perro boca abajo). Exhala. **V invertida**, caderas al cielo (punto más alto), **brazos en
   línea con el torso**, talones hacia el suelo, mirada al ombligo. *Rig:* `Spine(80)` pliega torso + `ArmsUp` (X−175) en
   línea con el torso. ✓ (brazos ya no en lateral).
9. **Ashwa Sanchalanasana** (zancada, otra pierna). Inhala. Espejo de la 4 (pie izquierdo atrás).
10. **Uttanasana**. Exhala. = 3.
11. **Hasta Uttanasana**. Inhala. = 2.
12. **Pranamasana**. Exhala. = 1.

---

## Upa-yoga de cuello (lo primero que enseña la Magnate)

Respiración **Ujjayi** ("victoriosa"): se exhala como hacia la boca cerrada y el aire sale por la nariz cerrando
parcialmente la **glotis** → sonido oceánico. Cada inhalación/exhalación dura **8 s** y el rezo **se mueve toda la
respiración** (fluido y lento; en código `reachFraction = 1`). Implementado en `UpaYoga.NeckWarmup()`.

- Preparación: de pie, **piernas a la anchura de los hombros**, rezo al pecho.
- **De pie × 3**: inhala → rezo **sobre la cabeza**; exhala (Ujjayi) → manos a la **nuca**; inhala → arriba; exhala → rezo
  **al pecho**. La **3ª** exhalación al pecho baja además a **cuclillas**.
- **Cuclillas × 2** (plantas completas en el suelo, rodillas adelante, rezo por delante de las piernas a la altura del
  pecho): arriba → nuca → arriba → pecho → inhala **brazos al frente** (rezo extendido) → exhala recoge y apunta arriba.
- Cierre: **Balasana** (postura del niño) **2 min**.

*Rig:* `Overhead` sube por flexión (manos juntas arriba), `Nape` (codos muy flexionados tras el cuello), `Forward`
(brazos horizontales al frente), cuclillas por `root` bajo + muslos/espinillas muy flexionados. Afinado visual pendiente.
