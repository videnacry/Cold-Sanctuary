# Progresión entre santuarios: poder, jefes, magia y bonusPacks

Berón, 2026-11-08: «un humano sale de su santuario solo cuando su poder supera al animal más poderoso de ese santuario». En s1n1 es la ballena, la orca o el oso polar: el más fuerte de la fauna que hay.

Código: `SanctuaryPower` (el poder y los jefes) y `SanctuaryPlacement` (la casa, el acceso y el guardado). Prueba: grupo 113 (`RepartoSantuariosTest`). Reparto del día: [jornada-y-rotacion.md](jornada-y-rotacion.md) §Reparto por santuarios. Manual: J33.

## 1. Santuarios y niveles

- Santuario 1 = la tierra (`SampleScene`), 2 = el Marino ([sitio-nuclear.md](sitio-nuclear.md) §20), 3 = el Aéreo (§23).
- La **posición** es un número: (nivel − 1) × 3 + (santuario − 1). s1n1 = 0, s2n1 = 1, s3n1 = 2, s1n2 = 3…
- **Acceso:** se va a sitios anteriores, nunca a los siguientes (`CanAccess`).
- **Subir:** al salir del último santuario de un nivel se pasa al primero del nivel siguiente.

## 2. La regla del jefe

- Para pasar de santuario hay que demostrar ser **más fuerte que TODOS sus animales** ([sanctuary-second-lap-and-fear.md](sanctuary-second-lap-and-fear.md) §3).
- Se sube de forma **natural**, con misiones, yoga y meditaciones que desbloquean hechizos ([spell-genesis-and-sanctuary-progression.md](spell-genesis-and-sanctuary-progression.md) §3).
- **El jefe de una posición** (`SanctuaryPower.Bosses`) es el mayor de dos valores:
  - el animal más poderoso de los santuarios hasta el suyo, en su nivel;
  - un humano corriente con el `bonusPack` de esa posición (bonusPack1 para s1n1… bonusPack4 para s1n2).
- **La fauna:**
  - En el santuario 1 se lee de la escena: el `Animal` vivo con más `PredatorPower`.
  - Sin escena se usa la lista de especies (`SanctuaryPower.FaunaOf`) con sus arquetipos (`Archetypes.BodyOf`). Nada está fijado a mano: el valor sale de la fórmula.
  - **Inferido:** la lista de la tierra son las especies polares de `SampleScene`; el Aéreo, sus aves frías; el Marino aún no tiene animales propios.
- **Sin pack para una posición, no se sube más.** Hoy hay 4 packs, así que el techo es s2n2.
- Cada día la jornada escribe los jefes en el log, con la línea `[Progresión] … jefes:`.

## 3. El poder de un humano

Todo va en la escala de `Predation.PredatorPower`, con **una sola fórmula**, `Predation.Power`:

```
poder = 0,4 × fuerza + 0,4 × masa + 0,15 × coraza + 0,05 × agilidad + arma        (× (1 − enfermedad))
```

`SanctuaryPower.OfHuman` suma tres partes:
- **Cuerpo:** `PredatorPower` de su `Anima` en la escena. Sin cuerpo, su arquetipo, por su nombre o «Human».
- **Magia:** lo que sus margas suben su poder de hechizo, `SpellPower(nivel) − SpellPower(0)` (`DerivedStats.SpellPower`). Es 0 con las tres margas a nivel 1.
- **bonusPacks:** los que lleve y su cuerpo aún no tenga (`SoulComposition.bonusPacks`). Cada pack suma `Predation.Power` de sus aptitudes.

**El camino:** tareas, entrenamiento, yoga y meditación suben las margas. Después se enseña magia. Magia y bonusPacks son lo que deja a un humano superar a una ballena (113: el pack solo no basta; con magia, sí).

**La subida** (`SanctuaryPlacement.Next`):
- La primera vez, la casa es la mayor entre su casa de inicio y lo que su poder ya supera.
- Después, como mucho **un santuario por día**: si su poder pasa al jefe de su casa, sube.
- Nunca baja.
- Se guarda en PlayerPrefs (`cs_santuarios_poder`). Es clave nueva: la de antes guardaba las casas de la tabla de puntuación.

## 4. bonusPacks

- **Qué son:** vectores aditivos de aptitudes que no tocan la personalidad ([soul-composition-blend.md](soul-composition-blend.md) §bonusPacks, l. 76–84 y 115).
- **Para qué:** «bonusPackN ≈ los stats necesarios para superar el santuario N».
- **Valores:** son **provisionales**, `Archetypes` l. 224–230.
- Cada pack suma lo mismo a todas las aptitudes. Su poder sale de fuerza + masa + agilidad, 0,85 por punto.

| Pack | Suma a cada aptitud | Poder que suma |
|---|---|---|
| bonusPack1 | 0,5 | 0,43 |
| bonusPack2 | 1,2 | 1,02 |
| bonusPack3 | 2,5 | 2,13 |
| bonusPack4 | 4,5 | 3,83 |

## 5. Jefes y casas de inicio (calculado con los arquetipos, sin escena)

El jefe de cada casa es lo que hay que superar para salir de ella. Un humano corriente tiene 0,87.

| Casa | Jefe | Poder |
|---|---|---|
| s1n1 | Ballena | 2,37 (orca 2,24, morsa 2,13, oso polar 1,93) |
| s2n1 | Ballena (el Marino no tiene fauna; el humano con bonusPack2 da 1,89) | 2,37 |
| s3n1 | Humano + bonusPack3 (las aves del Aéreo dan 0,4) | 2,99 |
| s1n2 | Humano + bonusPack4 | 4,69 |

Casas de inicio (Berón 2026-11-08, `SanctuaryPlacement.StartingHome`):

| Persona | Poder (sin escena) | Casa | Por qué |
|---|---|---|---|
| Irosene | 1,08 + bonusPack2 = 2,10 | s2n1 | Berón: planeada con la magia de s3n1, empieza en s2. Su pack, [soul-composition-blend.md](soul-composition-blend.md) l. 83 |
| Lizmibet | 0,87 + bonusPack2 = 1,89 | s2n1 | Igual que Irosene. **Inferido:** el mismo pack, porque los docs no dicen otro |
| Linduarda | 0,87 | s3n1 | Hechicera de nivel 4, y el santuario más alto que hay es el 3 |
| Goluis | 1,22 | s1n1 | Su poder |
| Gohageneis | 1,04 | s1n1 | Su poder |
| Panterilia | 0,74 | s1n1 | Su poder |
| Kushal y todos los demás (Fernando y Luciana incluidos) | 0,87 | s1n1 | Su poder: un humano corriente |

En la escena el cuerpo manda, y el log `[Progresión] … poder:` dice el poder de cada uno.

## 6. Lo que esto cambia en el reparto

- Irosene y Lizmibet son las únicas que llegan al Marino, donde están la planta nuclear y la Línea. Lo más probable es que pasen allí el día entero, apagadas en `SampleScene`.
- Linduarda es la única del Aéreo.
- Las áreas de allí que no cubren ellas las hacen clones.
- Kushal y los demás, en la tierra.

## 7. Corrección

`SanctuaryPlacement` decía que el personal no llevaba `CharacterLevel`. **Sí lo lleva**: `SampleSceneBuilder.DressHumanoids` se lo pone a todo humano con mente (l. 2201–2238). La «tabla de inicio como suelo» de la ronda en vivo ya no existe: la casa sale del poder.

## Pendiente con Berón

- Los valores de los packs.
- La fauna del Marino y del Aéreo, y si sus animales se amplifican (docs: «S2/S3/S4 son amplificaciones crecientes»).
- Si Linduarda, la médica que recibe a los recién llegados, debe pasar el día en el Aéreo.
