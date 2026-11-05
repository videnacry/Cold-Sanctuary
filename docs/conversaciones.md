# Charlas entre personas (humanos e insectos-humanos)

Berón 2026-11-04: «se supone que tenemos un sistema montado para que surjan conversaciones entre ánimas, ¿no? En este caso los
humanos e insectos humanos deberían poder hacerlo; también debería haber una escena específica creada para poder probar esta
lógica al detalle».

Código: `Assets/Scripts/Social/` (`ConversationDirector`, `ConversationTopics`, `ConversationContext`, `Conversable`,
`ConversationSandbox`). Prueba: `Assets/Scripts/Tests/ConversationTest.cs` (grupo 88). Escena: `Assets/Scenes/Pruebas/Prueba_Conversaciones`
(constructor `Assets/Editor/SampleSceneBuilder.Conversaciones.cs`).

## 1. Lo que ya había (antes de este cambio)

| Pieza | Qué hace | Límite |
|---|---|---|
| `CommunicationSpell` (`Mind/CommunicationSpell.cs`) | «Todas las animas pueden hablar entre sí»: cada 12 s (+ azar) busca a la ánima con `Mind` más cercana (a 6 m, prefiriendo la más vinculada); las dos formulan un pensamiento (`Mind.SpeakNow`), hay **contagio emocional** (serotonina o cortisol del que escucha según la valencia del otro, × vínculo) y, si las dos frases son positivas, **crece la amistad** (`GrowBond Friend +0,5`). | **Nadie lo lleva** (ningún constructor lo añade). Solo habla quien tiene `Mind`; un solo intercambio, sin turnos; **no se ve** (solo `[Charla]` en el registro); no sabe del trabajo, la familia, la era ni las necesidades; sin tope de charlas, sin mirar la escena ni las prioridades. |
| `Mind` | Piensa cada 4 s una frase por tono elemental (aptitudes + humores + campos); `SenseSurroundings`: una amiga cerca da calma, un depredador cautela. | El pensamiento solo se ve del ser que maneja el jugador (`AnimaThoughtHUD`). |
| `SocialField`, `ThoughtField`, `BondPillar` | Contagio de ánimo a los vecinos con vínculo; campos que tiñen el pensamiento; vínculo por cercanía. | Opt-in; no son charlas. |
| Vínculos (`Anima.bonds`) | `Bond` 0–100 por `ITarget`, `GrowBond` × `EffectiveBondGrowthRate` (niño ×3, adulto ×0,5, menos con trauma). | Los humanos no son `ITarget`: hace falta el adaptador `AnimaTarget`. |
| `Dialogue/` (`DialogueManager`, `DialoguePanel`, `DialogueTrigger`) | Diálogos guionizados con el jugador (paneles). | No es entre ánimas. |
| `docs/mind-model.md` §5–6 | Diseño: campo social/semántico y diálogo **compuesto de plantillas** (tag × ánimo × relación × aptitud). | No estaba hecho para las personas. |

Resumen: había contagio y amistad por charla para quien tuviera Mente, pero no surgía (nadie llevaba el hechizo), no se veía y no
tenía contexto. Las personas (`PersonLife`: humanos e insectos-humanos, y el personal del santuario) no charlaban.

## 2. Cómo es ahora: `ConversationDirector`

Uno por escena con personas: se crea solo al cargar una escena que tenga `PersonLife`, `WorkdayDirector` o `Conversable`
(`ConversationDirector_AUTO`); en `Prueba_Conversaciones` va puesto a mano para tocarlo en el inspector.

- **Quién**: quien lleve `PersonLife` (humanos e insectos-humanos, en el santuario y en los microcosmos), el personal de la jornada
  (`WorkdayDirector.staff`) y quien lleve `Conversable` (`optOut` lo saca). **Nunca el jugador.** Pares humano↔humano,
  insecto↔insecto y humano↔insecto, **solo dentro de la misma escena**.
- **Cuándo surge** (cada `scanSeconds` = 2 s, un temporizador): de los que están **libres** (ver §4) y **quietos** (se han movido
  menos de un tercio de su estatura desde la vuelta anterior: no se habla de camino al puesto) y fuera de su espera, cada uno busca
  pareja a menos de `rangeHeights` = 2,6 estaturas. Puntuación: cercanía (×2) + vínculo/50 + **actividad compartida** (+0,6: el mismo
  puesto de trabajo, el mismo comedor a la hora de comer, la misma actividad de su franja libre, o los dos en turno del santuario) +
  **ganas de hablar** (suben con el tiempo sin charlar: 2 h de juego hasta el máximo). Probabilidad por vuelta: `startChance` (0,55) ×
  sociabilidad × ganas × (`onShiftFactor` 0,4 si alguno está en turno de trabajo).
- **El guion**: un tema (§3) y **4–6 frases que se turnan** (A abre, B responde, A, B…), una cada `lineMinutes` = 1 min de juego
  (2,5 s con el día de 1 h; entre 1,5 y 6 s). Una charla buena entre quienes ya se aprecian (o muy sociables, o al azar) se alarga con
  su despedida.
- **Se ve**: un **bocadillo** sobre quien habla con su nombre de pila, el tema y la frase (verde si la charla es buena, rojo si son
  quejas: los colores del pensamiento de `AnimaThoughtHUD`), dibujado como los nombres (`AnimaNameplate`: OnGUI proyectado, solo a
  menos de 35 m de la cámara). También en el registro: `[Charla] «Elena» y «Tomás» (humano-humano) hablan de su familia.`, una línea
  por frase y el balance al acabar (vínculo y estrés antes → después).
- **Qué deja**, por frase: el que escucha gana vínculo `bondPerLine` (0,8 × su ritmo de vínculo; un tercio si son quejas) y el que
  habla la mitad; el estrés del que escucha baja `stressPerLine` (0,03) si la charla es buena y sube la mitad si son quejas; contarlo
  alivia al que habla; con `Mind`, serotonina o cortisol (el contagio de `CommunicationSpell`); la cara (`CubeFace`: contenta o triste).
  Al terminar su guion una charla buena suma otro `bondPerLine` a los dos. Queda en la **memoria** de los dos (las 8 últimas: tema y
  con quién; un tema ya hablado con esa persona pesa ×0,25) y en `ConversationDirector.Records` (las 200 últimas).
- **Coste acotado**: `maxConcurrent` = 4 charlas a la vez en la escena, `maxStartsPerScan` = 2 nuevas por vuelta, la búsqueda cada 2 s
  (no por fotograma), la lista de personas se refresca cada 5 s, esperas entre charlas (`personCooldownMinutes` 20, la misma pareja
  `pairCooldownMinutes` 45) y un tope por charla (`maxMinutes` 20).
- **Durante las pruebas** de otras escenas no hay charlas (`AllowDuringTests`, como el horario libre): no tocan sus cuentas.

## 3. Temas (`ConversationTopics`)

Salen del contexto de los dos; cada uno pesa según lo que hay:

| Tema | Cuándo pesa | De dónde salen los datos |
|---|---|---|
| presentarse | aún no se conocen (vínculo < 3 los dos) | su trabajo, o su linaje de insecto |
| su trabajo | comparten puesto o turno (4), o alguno tiene trabajo (1,5) | `MicroJornada.Doing` («Cocinando: …»), su papel, `Conversable.role`, el estado de la jornada del santuario en su rótulo |
| la comida | comen en el mismo comedor a su hora | el lugar |
| su franja libre | la misma actividad del horario libre | `LeisureDirector.StatusOf` |
| su familia | son pareja, madre/padre e hijo (3) o uno tiene familia (1,5) | `PersonLife`: pareja, hijos, embarazo, madre |
| de dónde vienen | pareja mixta (2,5), insectos de especies distintas (2) o iguales (1) | `FolkLineage`: su especie, madre/padre y abuelos (en castellano) |
| la era / el lugar | en un microcosmos o con `ConversationContext` | `MicroEra` (lugar, fecha, año) o `ConversationContext` (lugar, época) |
| una historia del sitio | `ConversationContext.story` con frases | una frase al azar de la historia del microcosmos |
| el santuario | sin era ni contexto | el hielo, el mar, el árbol |
| el hambre (quejas) | quien abre tiene hambre ≥ 0,5 | — |
| el cansancio (quejas) | sueño ≥ 0,55 o estrés ≥ 0,6 | — |
| lo que piensa | quien abre tiene `Mind` | `Mind.SpeakNow()` (su pensamiento del momento y su valencia) |

Con estrés > 0,7 quien abre, la charla es de mal humor (quejas) aunque el tema sea otro.

## 4. Prioridades: una charla nunca manda

- **Nunca mueve a nadie**: hablan donde están (en su puesto, a la mesa, en su actividad libre). La jornada, el horario libre y las
  necesidades siguen llevándolos; si se separan (más de `breakHeights` = 4 estaturas) la charla acaba.
- **No empieza** y **se corta al momento** (se mira cada 0,5 s) si uno: tiene una **necesidad urgente** (hambre, baño o sueño ≥
  `urgentAbove` 0,85, o estrés ≥ 0,92), la está atendiendo (`NeedsDriver.Attending`), duerme, ha muerto, está en una iniciación
  (`Orchestra.Busy`), la historia lo retiene (`MicroJornada.hold`), sirve en el restaurante o va de viaje en alfombra.
- En turno de trabajo se habla menos (×0,4) y de su trabajo.

## 5. Reglas (en el manual de diseño: J7–J11 de [manual-de-diseno.md](manual-de-diseno.md) §9)

1. **Charlan todas las personas**: humanos e insectos-humanos con `PersonLife` y el personal de la jornada; un personaje de historia
   que no deba hablar lleva `Conversable.optOut`. El jugador no entra.
2. **Solo dentro de su escena** y a menos de 2,6 estaturas (la del más alto): la distancia se mide en estaturas, no en metros (la
   escala del microcosmos cambia).
3. **Una charla no mueve a nadie ni cambia lo que hace**: es una capa encima de la jornada, el horario libre y las necesidades.
4. **Una necesidad urgente corta la charla** (≥ 0,85) y nadie charla con ella.
5. **Se turnan**: A, B, A, B; 4–6 frases cortas, una por minuto de juego; cada frase se ve en un bocadillo con el tema.
6. **Los temas salen del contexto** (trabajo, familia, linaje, era, historia del sitio, necesidades, la mente), nunca de un árbol de
   diálogo escrito entero: plantillas cortas × datos de los dos.
7. **Toda charla deja huella**: vínculo (más a quien escucha), estrés/humores (contagio) y memoria (tema y con quién).
8. **Coste acotado**: ≤ 4 charlas a la vez por escena, ≤ 2 nuevas por vuelta, búsqueda cada 2 s, esperas entre charlas y tope de 20
   min de juego por charla.
9. **Un microcosmos con historia** pone un `ConversationContext` con su lugar, su época y frases de su historia.

## 6. Escena de prueba: `Prueba_Conversaciones`

Menú «Tools/Cold Sanctuary/Build Prueba_Conversaciones (charlas entre personas)». Escena propia y pequeña (como las de ciclo de vida
de una persona): no abre ni copia el santuario. Una sala con suelo, una **mesa larga** (9 × 1 m) y **dos bancos**; seis personas por
parejas frente a frente (los humanos sentados en el banco, los insectos-humanos de pie detrás):

| Sitio | Pareja | Clase |
|---|---|---|
| Oeste | Elena y Tomás (humanos; pareja con dos hijos) | humano-humano |
| Centro | Rosa (humana) y Ciro (hormiga; madre hormiga, padre grillo) | humano-insecto |
| Este | Teodora (mariquita) y Bruno (araña) | insecto-insecto |

Con el reloj (día de 1 h real, a las 12:00), el director, el contexto (la sala de charlas, «hoy, en el santuario helado», cuatro
frases de historia), los nombres (`AnimaNameplate`), `ConversationSandbox` (necesidades bajas salvo cuando la prueba quiere una
urgente; humanos sentados), el explorador y el orquestador (solo el grupo 88). Foto al construir: `Temp/Conversaciones/Prueba_Conversaciones.png`.

### Grupo 88 (`ConversationTest`)

1. Reparto: 3 humanos y 3 insectos-humanos, todos personas; insectos de especies distintas.
2. Surge una charla **humano-humano**, **insecto-insecto** y **humano-insecto** en menos de **30 min de juego** cada una.
3. Al menos tres terminan su guion; **por turnos** (A, B, A, B…); dicen todo su guion (4–6 frases); cada frase con texto y tema; los
   **bocadillos se colocan ante la cámara** (`BubblesReady`; lo pintado por OnGUI va en el detalle, porque solo sube si la vista Game repinta).
4. El **vínculo sube** en los dos sentidos y el **estrés cambia** (emoción).
5. Con una charla en marcha, al insecto-humano se le pone **hambre urgente** (0,95; si no hay insecto, sueño al humano): la charla se
   **corta** (motivo «necesidad: hambre») y no vuelve a charlar.
6. Con el **tope en 2**: nunca más de 2 a la vez y se llega a 2; en toda la prueba, nunca más que el tope de la escena (4).
7. **Nadie se teletransporta ni salta** (> 1 m, o > 0,6 m hacia arriba, en un fotograma) en toda la prueba.

El tiempo va a ×2 durante la prueba; cada espera está acotada en tiempo real (120, 90, 60, 40 s).

## 7. Pendiente

- Verificación en vivo del grupo 88 y fotos de los bocadillos (la hace el tester único).
- Ver en el santuario (`SampleScene`) y en un microcosmos civilizado (Kapilavastu) que las charlas surgen en la comida y en las
  franjas libres, y que el registro no se llena (una línea por frase).
- Poner `ConversationContext` con su historia en los microcosmos de historia (Bruce Lee, Nightingale, Pakal…).
- Que la charla elija postura/gesto de hablar (mirar al otro cuando ninguno está en su puesto) sin pelear con la jornada.
- `CommunicationSpell` sigue igual (charla de un intercambio entre Mentes): podría delegar en el director cuando las dos son personas.
