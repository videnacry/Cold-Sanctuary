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

## El yoga va por RESPIRACIONES (no por tiempo fijo)

Una **respiración** = inhalación (8 s) + exhalación (8 s). El cambio de postura ocurre **en una fase** (8 s) y la postura
se **mantiene n respiraciones**. En código, cada fase es un `AsanaPoseSpell` de 8 s con `reachFraction = 1` (el cuerpo se
mueve toda la fase). `SunSalutation.Breath = 8`.

- **Saludo al Sol**: 1 respiración por postura → **1 fase para llegar + 2 de mantenimiento = 24 s/postura** (12×3 = 36 fases).
- **Yoga Namaskar / Upa-yoga de cuello**: cada movimiento ocurre en 1 fase (flujo continuo); sin mantenimiento salvo Balasana.

## Progresión de enseñanza de la Magnate

1. **Upa-yoga de cuello y hombros** (`UpaYoga.NeckSequence`) — lo primero.
2. **Yoga Namaskar** (`YogaNamaskar.Sequence`) — de pie + cuclillas, palmas siempre juntas, Ujjayi.
3. **Saludo al Sol / Surya Namaskar** (`SunSalutation.Sequence`).

## Upa-yoga de cuello y hombros (`UpaYoga`) — 6 ejercicios

De pie, **talones a la anchura de las caderas**, **espalda recta y relajada**, brazos sueltos. Respiración **SIN sonido**,
8 s por fase. Ejercicios 1–4 solo mueven el cuello; 5–6 solo los hombros (escápula). 3 repeticiones c/u; terminan neutros.

1. **Flexión/extensión**: exhala barbilla al pecho · inhala cabeza atrás.
2. **Rotación**: inhala cara a la derecha (+~20° atrás) · exhala al frente · inhala a la izquierda · exhala al frente.
3. **Inclinación lateral** (oreja al hombro, "torsión"): exhala derecha · inhala arriba · exhala izquierda.
4. **Círculo completo**: barbilla→oreja dcha→atrás→oreja izq→barbilla; inhala al subir, exhala al bajar. 3 + 3 (sentido contrario).
5. **Hombros, giros verticales hacia delante**: inhala arriba-atrás · exhala abajo-delante.
6. **Hombros, giros en sentido contrario**.

*Rig:* `Head` (X flexión, Y rotación, Z inclinación) + nueva articulación **`ScapulaLeft/Right`** (entre torso y hombro)
para encoger/rodar el hombro sin mover el brazo. El giro circular se aproxima con 4 cuartos por vuelta.

## Yoga Namaskar (`YogaNamaskar`) — de pie + cuclillas + Balasana

**Palmas siempre juntas en rezo**; **espalda recta salvo en Balasana**; respiración **Ujjayi** (glotis, sonora), 8 s/fase.
De pie ×3 (rezo pecho→cabeza→nuca→cabeza→pecho; la 3ª baja a cuclillas) + cuclillas ×2 (añade brazos al frente) +
**Balasana** (postura del niño) 2 min.

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
