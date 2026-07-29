# La Cocina — primera simulación jugable (nivel de referencia)

Diseño (2026-07-28). La **cocina** es la **primera simulación completa**: un nivel de punta a punta que
sirve de plantilla para las demás. Conecta el **Mesocosmos** (la cocina a pie del santuario) con el
**Microcosmos** (el mundo-insecto donde se "limpia" de verdad). Base sobre:
[`world-topology-and-planes.md`](world-topology-and-planes.md) (los 3 planos),
[`magic-plane-and-meditation.md`](magic-plane-and-meditation.md) (virtualización, avatares, arquetipos),
[`mob-world-architecture.md`](mob-world-architecture.md) (santuario fractal, portal, mob-residents),
[`anima-architecture.md`](anima-architecture.md) (mente/frases/campos/control), y
[`mob-quests-early.md`](mob-quests-early.md) (históricos de la era del fuego).

## 1. El paseo (onboarding divertido)
Al entrar por primera vez a la cocina, un **paseo guiado** (compañero anfitrión, p. ej. Goluis — es su
área) señala las **áreas** y **qué puedes hacer/usar** en cada una:
- **Nevera/refrigeradora** — sacar/guardar ingredientes.
- **Plancha/fogón** — cocinar (revolver, sellar, secar).
- **Mesones** — preparar/emplatar.
- **Despensas** — reponer stock.
- **Fregadero/lavaplatos** — (en Meso NO se lava aquí; ver §7 el puente Micro).
- **Contenedores de servicio** — donde queda la comida lista para que los personajes coman.
Cada parada desbloquea su interacción y deja una **misión-tutorial** corta. Reutiliza el sistema de
misiones por área ([`area-missions-spec.md`](area-missions-spec.md)).

**Quién enseña, y cómo.** El **personaje presente** en el área (p. ej. Goluis) procesa el **pensamiento de
"enseñar a los nuevos"** — un pensamiento presente **en todas las áreas** (docs anima §11.7): es un
`MindPhrase` con **`lifecycle = OnceThenGone`** (enseña la primera vez y luego calla) o `DecaysPerUse`
(insiste menos con el tiempo), **peso** alto cuando hay un novato cerca, y **gate por aptitud** (solo
enseña quien tiene `sociability`/`discipline` suficiente). La **introducción** en sí usa la **petición →
alma compartida** (docs anima §11.7): el veterano "invita" al novato (sí/no); al aceptar, comparten alma
un momento y **caminan juntos** al recorrer las paradas del área. Aquí también se puede **liberar la mente
de Kushal** (`FollowBrain`) para que siga al anfitrión por la cocina.

## 2. Progresión de rol (de lavaplatos a cocinero)
El personaje **asciende** haciendo tareas, de lo simple a lo complejo:
1. **Limpieza (pinche/lavaplatos)** — limpiar **suelo → paredes → plancha → mesones → refrigeradora →
   utensilios**; **reponer despensas**; **guardar todo en su lugar**. (Ver §5 y §7.)
2. **Desayunos** — el primer cocinado (ver §3).
3. **Recetas** — combinaciones cada vez más ricas (ver §4).
Cada escalón se **desbloquea por aprendizaje** ([`learning-unlocks.md`](learning-unlocks.md)): la UI de
esa tarea aparece cuando la aprendes.

## 3. El loop de cocinar (ejemplo: huevos revueltos)
Cadena de acciones espaciales, no un botón:
`acercarse a la nevera → abrirla → tomar los huevos → llevarlos a la plancha → revolver hasta que sequen
un poco → especiar → pasar a los contenedores`. Cada paso es un `IInteractable`/estado con su feedback.
El resultado alimenta un **contenedor de servicio** que **siempre se está rellenando** mientras alguien
cocina. Los personajes se acercan a los contenedores a **comer** (§6).

## 3b. Interacción de VIRTUALIZACIÓN (puntero + estaciones funcionales) — GENERALIZABLE
El modelo real de juego (2026-07-29), plantilla para **todas** las áreas:
- **Mira FIJA en el centro** de la cámara (`VirtualPointer`): **nunca se mueve** en pantalla. Para apuntar,
  **giras la cabeza = la cámara** con **I/K/J/L** (`HeadLook`), **con restricciones** de giro (yaw/pitch
  limitados, como una cabeza real). Confirmas con **F** (Espacio es salto). El **ratón** y el **touch**
  conservan su **propio cursor** (clic/toque interactúa donde apuntan). Así se juega **solo con teclado** o
  con ratón/touch. Teclas configurables (ver `DEVLOG.md` §Input). *(Al entrar en una estación se activa
  `HeadLook` y se congela el look libre del jugador; mientras se teclea, cabeza y mira se congelan.)*
- **Estaciones por FUNCIONALIDAD**, cada una con **partes manipulables**: abrir la puerta del **mesón** →
  sacar **sartenes**; abrir la **nevera** → tomar **huevos**; colocar la sartén en la **cocina/fogón**, el
  huevo encima, **encender el fuego**…
- **Recetas = pasos ordenados** sobre estaciones. El paso correcto avanza; el orden importa; feedback si te
  equivocas. Completar la receta = **una ración/producto**.
- **Cuota de producción de la MISIÓN = sustento del santuario**: repetir recetas llena los contenedores/
  recursos hasta la cuota → misión cumplida; la comida **alimenta a todos los personajes** del santuario.
- **Se aplica igual a cada área**: cambian el set de estaciones y las recetas (ver Huerto en
  [`garden-simulation.md`](garden-simulation.md) §8). Es la mecánica de virtualización universal.

> **Estado del código.** El **motor está construido (MVP)** en `Assets/Scripts/Virtualization/`:
> **`VirtualPointer`** (puntero input-agnóstico: **teclado** con las teclas de cámara / **ratón** / **touch**,
> a la vez), **`StationPart`** (parte manipulable: apuntable por el puntero) y **`ProductionOrder`** (receta
> por pasos ordenados → producto → **cuota** = sustento). Sandbox `VirtualizationSandbox_AUTO` (cocina de
> huevos revueltos, 7 pasos, cuota 3). *(`TourStation`/`BreakfastCook` siguen como onboarding/NPC de fondo.)*
> **Falta:** feedback visual (resaltar la parte apuntada, animaciones), enganchar la cuota a la misión real
> del área, y las **recetas de cada área** (huerto en `garden-simulation.md` §8).

## 4. Recetas = química (la "sopa de letras")
- Una **receta** es una combinación de **ingredientes**; cada ingrediente aporta **compuestos** (ver §8).
- Los platillos elaborados se vuelven una **"sopa de letras"**: más ingredientes = más compuestos = más
  efectos. La cocina es, literalmente, química aplicada — enlaza con el sistema `Chemistry` (tabla
  periódica, ~55 elementos) ya existente.
- **La comida forma al personaje**: comer nutre los **humores** (efecto pequeño e incremental) y, a largo
  plazo, empuja las **aptitudes** (dieta constante → cambios lentos). Ver §8.

## 4b. Mecanografía para acelerar + idiomas (typing) — diseño
Algunas acciones son **temporizadas** (cocinar en el fogón, licuar, hornear). Mientras corren, aparecen
**palabras flotando sobre el objeto** (p. ej. sobre el huevo) y **teclearlas acelera** la acción:
- **Banco de palabras por acción/ingrediente**: resuenan con lo que haces — verbo (`cook`), ingrediente
  (`eggs`), **compuesto/nutriente** (`protein`, `B2`), cualidad (`healthy`). Cada palabra correcta **recorta
  tiempo**; hay una **coherencia base** de tiempos, reducida por palabra.
- **Idioma elegible** (en/fr/…): la cocina se vuelve **práctica de mecanografía y de idiomas** (aprender
  inglés/francés cocinando). El banco de palabras se localiza.
- **Otras estaciones temporizadas**: **licuadora**, **horno** (cada una con su banco y su tiempo base).
- **Enlace con la química (§8)**: como muchas palabras son **compuestos/nutrientes**, teclearlas puede
  además **registrar** ese compuesto en el platillo (más nutritivo/afinado) → une el typing con "la comida
  forma al personaje".
- **Estado: MVP HECHO** — `TypingChallenge` (tiempo base + banco de palabras + recorte por palabra;
  captura `Input.inputString`; se completa por tiempo o al teclearlas todas → emite el paso a la
  `ProductionOrder`). Se enchufa a una `StationPart` **temporizada** (`StationPart.timed`); el fogón de la
  cocina ya lo usa (banco `cook/eggs/protein/healthy/tasty/b2`). Mientras corre, `TypingChallenge.Active`
  **congela** cámara/mira/look del jugador. **Falta:** dibujar las palabras **sobre el objeto** (hoy en una
  caja central), **bancos localizables** por idioma (en/fr…) y por **compuesto/nutriente** (aplicar el
  compuesto al platillo), y las estaciones **licuadora/horno**.

## 5. Suciedad como objeto real (misión de limpieza)
La **suciedad se crea literalmente** en el mundo (no es un flag):
- Se **acumula** con el uso; al pasar de un **umbral**, se dispara la **misión de limpiar**.
- La limpieza es **mancha por mancha** (cada mancha = un objeto que se borra al resolverla).
- Esto es lo que permite el **puente Micro/Meso** (§7): cada mancha del Meso es una **región a limpiar**
  en el Micro.

## 6. Alimentación dirigida por HUMORES (elegir con el cuerpo)
Los personajes **eligen qué comer según sus humores** (y necesidades), usando la utilidad del `Mind`:
- p. ej. **glucosa baja → prefieren un contenedor energético**; **cortisol alto → buscan un "comfort
  food"** que suba serotonina; etc.
- Comer aplica los **compuestos** del platillo → nudge de humores (y, acumulado, de aptitudes).
- Ideal a futuro: que **todo** lo elijan así (no solo comida) — el mismo motor de utilidad + humores.

## 7. Puente Micro ↔ Meso (la unión clave)
- En **MesoKitchen** el lugar **se ensucia** (manchas alimenticias). Pero **aquí no se lava nada**.
- Se conecta al **MicroKitchen** (mundo-insecto) mediante **minidrones/miniavatares**
  (`RobotAvatar`/`AvatarController`): en el mundo insecto, los avatares **extraen esos "manjares"** (la
  mancha es comida a escala insecto) → al vaciarla, la mancha del Meso **desaparece**.
- Escalada por niveles (coincide con el desbloqueo de locomoción de los avatares —
  `AvatarLocomotion` Ground → Climb → Flight): primero el **suelo**, luego **paredes** (caminar como
  pared), luego **mesones/altos** (volar). Limpiar arriba requiere haber desbloqueado trepar/volar.

## 8. Humores vs bioquímica vs "compuestos" — decisión de modelo
**Recomendación: NO fundir ni renombrar; tres capas con frontera clara** (extiende
[`anima-architecture.md`](anima-architecture.md) §10.1, modelo híbrido):
1. **Compuestos / química (ENTRADA, muchos)** — lo que traen la comida y el entorno: nutrientes
   (proteína, azúcar, grasa, vitaminas, minerales) y, por extensión, los **elementos** del sistema
   `Chemistry`. Conjunto grande y abierto. Es donde vive la "sopa de letras" de las recetas.
2. **Humores (ESTADO, 5)** — adrenalina/serotonina/cortisol/glucosa/calcio. Pequeño, legible, es lo que
   **lee la Mente** (ánimo/valencia/energía) para decidir. **No se toca su tamaño.**
3. **Aptitudes (CAPACIDAD, 12)** — lo que el ser puede hacer.
**Flujo:** los **compuestos** (comida) → *nudge* de **humores** (rápido, pequeño) → y, acumulados en el
tiempo, empujan **aptitudes** (lento). Así la comida "forma" al personaje sin inflar los humores ni
duplicar la química. → **Los humores se quedan como están; la comida se modela como `compuestos` que los
mueven.** (Un futuro `FoodCompound`/tabla receta→compuestos→ΔHumor es el enganche.)

## 9. Diálogos desde el campo social (ThoughtField)
Los diálogos/pensamientos **no** se escriben todos a mano: emergen del **campo social**
(`ThoughtField`, docs anima §11.4). Varios personajes limpiando juntos comparten un campo que **mezcla
sus pensamientos propios con los del área** → sueltan una mezcla de frases (propias + del entorno). Un
mesón muy sucio, una cocina en hora punta, etc., pueden ser campos con su propio tono/humor.

## 10. El mundo-insecto como santuario fractal
Igual que el 1er santuario tiene osos/focas, el mundo-insecto tiene **gusanos, hormigas, arañas…**. Ahí
arranca la **mecánica típica del juego: resolver conflictos para que los fuertes no se coman a los
débiles**. Está en la **era del fuego** (paleolítico), así que las misiones encajan con
[`mob-quests-early.md`](mob-quests-early.md):
- **buscar las manchas** (los "manjares" a extraer), **protegerse de hormigas/depredadores**, **mudarse
  de un lugar a otro**.
- Se **empieza como gusano** y se asciende de forma (coincide con el desbloqueo de locomoción: reptar →
  caminar paredes → volar).
- Ahí viven los **históricos del área**: **El Guardián del Fuego** (velar/repartir el fuego — ya
  autorado), y más adelante El Alfarero, La Sembradora, etc. (vivencias ya en `PhrasePools`).
- Como el Mesocosmos, puede haber **tareas de construcción e investigación**, **conectadas con la cocina
  del Meso** (lo extraído/investigado abajo repercute arriba).

## 11. Puente al 1er santuario (alimentar carnívoros)
Cuando el personaje sea capaz de **sobrevivir a un oso** (trepar árboles, correr, esquivar), se le
asignan **misiones de alimentar a los carnívoros**:
- El jugador se adentra **con cuidado** para no molestar a ningún depredador.
- **Algunos ya son amigables** por los **bonds** acumulados (todos los personajes hacen estas tareas; un
  animal se encariña de tanto que lo alimentan). **Otros recién liberados** vienen con **bond alto** por
  haber sido criados por personas.
- Enlaza con el refinamiento de caza/amenaza pendiente (ver `checklist.md` §Aptitudes: `SelectPrey`,
  aura/estatus, manada) y con la comida producida en la cocina (los contenedores surten a los animales).

## 12. Orden de construcción sugerido (para "dejar el nivel listo")
No hacerlo de golpe. Escalera propuesta, cada paso jugable y verificable por consola:
1. **A — Paseo + limpieza mancha-a-mancha (Meso). [HECHO]** Suciedad como objeto, umbral → misión, borrar
   manchas (`DirtArea`/`DirtSpot`/`Cleaner`) + **paseo** (`GuidedTour`/`TourStation`, alma compartida).
2. **B — Loop de desayuno + contenedor. [MVP HECHO]** `BreakfastCook` recorre la cadena
   nevera→plancha→especiar→contenedor y rellena el `FoodContainer`; `Eater` come. *(Falta hacerla espacial.)*
3. **C — Alimentación por humores.** Los personajes eligen contenedor por utilidad+humores; comer aplica
   compuestos → nudge de humores. *(Introduce `FoodCompound` mínimo.)*
4. **D — Puente Micro/Meso.** Mancha del Meso = región del MicroKitchen; minidrones extraen → la mancha
   desaparece. Empezar por el **suelo**.
5. **E — Mundo-insecto vivo.** Gusano→formas; conflictos fuerte/débil; misiones (manchas/proteger/mudar);
   enganchar al Guardián del Fuego.
6. **F — Recetas ricas + química completa.** "Sopa de letras", efectos por compuesto, dieta→aptitudes.
7. **G — Puente al santuario.** Misiones de alimentar carnívoros con bonds.

**Mínimo jugable primero:** A + B (cocina Meso jugable de punta a punta) → luego D (la unión Micro/Meso,
que es la gran novedad).

## 13. Próxima área para principiantes: **El Huerto** (recomendada)
Tras la cocina, la 2ª área principiante debería ser **El Huerto/Jardín**, porque:
- **Continúa la historia del microworld**: la era del **fuego** (cocina, Guardián del Fuego) da paso al
  **Neolítico / revolución agrícola** — el siguiente escalón de [`mob-quests-early.md`](mob-quests-early.md).
  Su histórica, **La Sembradora**, ya está autorada (arquetipo **Curar/atender**, elemento **N**).
- **Cierra un bucle económico con la cocina**: el Huerto **produce los ingredientes** que la cocina cocina
  (grano/verduras → contenedores). Enseña producción→consumo de forma tangible.
- **Es suave para principiantes**: sembrar → regar/atender → cosechar (acciones espaciales simples, sin
  peligro), con el mismo molde de misiones por área que la cocina.
- **Reutiliza sistemas ya hechos**: `SanctuaryResources`/`AreaProducer` (Food), farming/`PlayableCreature`.
- **Su giro conecta con el resto**: el excedente trae propiedad/jerarquía/primeras disputas → puente
  natural hacia la mecánica de conflicto (fuertes vs débiles) del mundo-insecto y del santuario.
Alternativas menores: **FuelLab** (Prometeo, sigue el hilo del fuego) o **Estudio/Textil** (arte:
Mano de Lascaux/Enheduanna). Pero el **Huerto** es el que mejor encadena *historia + bucle con la cocina*.

## 14. Piezas de código que ya existen y se reutilizan
- Entrada al Micro: `VirtualizationMachine` + `RealityShiftController` + `MobWorldLoader`.
- Mundo mob: `MobResident`/`MobWorldDirector`/`YogaPortal`/`MobSpawnPoint` (+ builder).
- Avatares: `RobotAvatar`/`AvatarController`/`SurfaceWalker` (`AvatarLocomotion` Ground/Climb/Flight).
- Mente/diálogo emergente: `Mind`/`ThoughtField`/`PhraseLibrary`/`PhraseDistribution`.
- Control/posesión: `AnimaController`/`AiBrain`/`PlayerBrain`/`PossessionSpell`.
- Química: `Chemistry`/tabla periódica (base para los compuestos de la comida).
- Recursos/economía: `SanctuaryResources`/`AreaProducer` (los contenedores pueden ser productores).
