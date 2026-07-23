# Topología del mundo y planos (Microcosmos / Macrocosmos)

Doc de diseño de alto nivel: cómo se organizan los santuarios en el espacio, cómo viaja el
jugador entre ellos sin perder el origen, y los dos planos jugables. Consolida decisiones tomadas
el 2026-07-22. Es **diseño** (el "qué"), no arquitectura de código; los sistemas que ya existen se
enlazan al final.

Docs relacionados: [`mob-world-architecture.md`](mob-world-architecture.md),
[`magic-plane-and-meditation.md`](magic-plane-and-meditation.md),
[`mob-epochs-matrix.md`](mob-epochs-matrix.md), [`area-missions-spec.md`](area-missions-spec.md).

---

## 1. Los tres planos (tríada micro / meso / macro)

El juego tiene tres planos jugables, nombrados con la tríada de escala **micro → meso → macro** (y el
eje hermético "como es adentro, es afuera"):

- **Microcosmos** — el plano INTERIOR. Es el antiguo "plano mágico": se entra por la máquina de
  virtualización (o el hechizo de loto), el mundo se miniaturiza y practicas meditación como misiones.
  Dentro viven las civilizaciones a escala insecto (ciudades históricas: Mesopotamia, etc.). Ir al
  Microcosmos es ir *hacia adentro* — la mente, la raíz, lo pequeño.
  - *Nota de nomenclatura:* "mob" (de *mobile object*, jerga de NPC/criatura) queda como **término
    interno de código**. De cara al jugador, los habitantes son **residentes / criaturas**, no "mobs".

- **Mesocosmos** — el plano ESTÁNDAR, a escala humana. Es el modo "normal": el jugador mueve a **Kushal
  a pie** por los santuarios, interactúa con áreas, personajes y criaturas, y desde aquí entra al
  Microcosmos (máquina/loto) o —más tarde— al Macrocosmos. Es el plano encarnado, el punto de contacto
  con el origen (§2). *(También llamable "plano simulacro"; se eligió Mesocosmos por la tríada.)*

- **Macrocosmos** — el plano SUPERIOR/EXTERIOR. Capa tipo RTS (estilo Warcraft): ves a los personajes
  como peones/héroes que envías a construir, investigar, recolectar y guerrear entre santuarios. Ir al
  Macrocosmos es ir *hacia afuera* — el imperio, la vista desde arriba, lo grande. Se desbloquea cuando
  Kushal se vuelve importante para los personajes (ver §5).

**Qué define a cada plano (su esencia):**
- **Microcosmos** → **la Historia**: el mundo/las civilizaciones desarrollándose (el reparto histórico,
  las eras).
- **Mesocosmos** → **el simulacro**: las tareas encarnadas y reales del santuario, a escala humana.
- **Macrocosmos** → **mirar desde arriba como dueño** de un área del mundo (gestión, estrategia).

Los tres planos comparten **el mismo roster** de personajes (Goluis, Panterilia, Gohageneis, Irosene…):
se conocen en el Mesocosmos, aprenden/potencian sus hechizos en el Microcosmos y se despliegan como
tropas/héroes en el Macrocosmos.

---

## 2. Principio rector: hub-and-spoke + gradiente de peligro

Dos reglas gobiernan toda la topología:

1. **Nunca perder el origen.** El santuario terrestre inicial es un **hub permanente**. Las misiones
   siempre cierran el bucle de vuelta al origen antes de mandarte a otro sitio. Progresión:
   - Fase temprana: misiones que te mantienen local, interactuando con lo más cercano.
   - Media: viajes rápidos cortos ("echa un vistazo") que te devuelven enseguida.
   - Tardía: estancias más largas en un lugar nuevo (descubrir), y de vuelta a lugares del pasado.
   - Siempre hay **comunicación constante entre santuarios y áreas** (eventos, migraciones, encargos).

2. **La dispersión de estructuras = función del peligro** (no solo estética). A más peligro, más
   unido el santuario, porque criaturas poderosas roamearían entre estructuras esparcidas:
   - Poco peligro → estructuras esparcidas por el mapa.
   - Mucho peligro → todo concentrado en una sola gran estructura/castillo.

   Esto da variedad topológica **con justificación mecánica** y hace que el santuario más peligroso
   sea, naturalmente, el de "todo en uno".

---

## 3. Los santuarios (topología física)

**Cinco** santuarios sobre un eje de **profundidad = peligro** (decidido 2026-07-23: el Núcleo es un
santuario propio, no una zona del Subterráneo).

| Santuario | Peligro | Topología | Notas |
|---|---|---|---|
| **Terrestre** (origen) | Bajo | Unido alrededor de una plaza central | Hub permanente. Fauna tipo animales reales, inofensiva. |
| **Marino** | Medio→Alto (por profundidad) | Vertical | Burbuja/cúpula de aire bajo el agua. Ver §3.1. |
| **Aéreo** | Medio | Muy disperso | Islas/plataformas separadas; viajes entre ellas. |
| **Subterráneo** | Alto | Unido | Descenso vertical por roca cada vez más caliente; da paso al Núcleo. |
| **Núcleo** | Máximo | Todo en un castillo (2 capas) | Endgame. Ver §3.2. |

### 3.1 Marino — vertical, bajo el agua

El santuario marino **está bajo el agua** (burbuja de aire / cúpula / *moon pool*, alimentada por un
conducto de gas o membrana mágica). No está a una sola profundidad: se **extiende verticalmente**.
- Parte alta: media profundidad, más habitable.
- Borde profundo: el fondo abisal, en oscuridad inmensa, con **criaturas exóticas** y peligrosas.

El peligro crece al descender (coherente con la regla de §2).

### 3.2 El Núcleo — santuario endgame de dos capas

Se llega al Núcleo por el Subterráneo: escarbas roca cada vez más caliente hasta **atravesar la lava**
y salir a un espacio abierto. El Núcleo tiene **dos capas** (las dos variantes de gravedad que se
barajaban ahora **coexisten**, no se elige):

- **Capa 1 — el santuario sobre el diamante (gravedad normal).** Estructuras gigantes de **diamante
  flotando sobre la lava**; encima se asienta el santuario y las **criaturas más poderosas** del juego.
  No se pueden esparcir estructuras (plataformas rodeadas de lava) → es el **"todo en un castillo"**.
- **Capa 2 — el mundo invertido (gravedad invertida, posible área FINAL).** Un **pasadizo secreto en
  el suelo** de la capa 1 lleva al **núcleo hueco**: al centro, una **esfera de plasma gigante** (el
  "sol interior", pequeña por la distancia); a muchos kilómetros, la cara interna de la **coraza de
  diamante** actúa como suelo y la **gravedad se invierte** (caminas sobre el diamante mirando al sol
  de plasma). Es la **posible área final del juego** (ver §5–§6).

Es el santuario **más peligroso**: alberga criaturas tan poderosas que no es admisible que queden fuera
de control.

---

## 4. Macrocosmos — la capa RTS

Cuando se desbloquea, el jugador comanda desde arriba como en Warcraft.

- **Mismo roster, tres roles.** Cada personaje (Goluis, Panterilia…) puede ser:
  - **Peón** — construir / reparar.
  - **Trabajador** — investigar / recolectar recursos.
  - **Héroe** — combatir en el campo de batalla (con sus hechizos, aprendidos en el Microcosmos).
  Se reclutan en la construcción que los identifica (cocina, huerto, mecánica…). Los más antiguos
  (p.ej. Irosene) llegan con hechizos potentes; los nuevos (Goluis) empiezan humildes.

- **Economía: recursos y ejército (como Warcraft).** El bucle central es incrementar recursos y con
  ellos comprar/generar guerreros y héroes y mejorar todo.
  - **Cada área del Mesocosmos = una estructura productora** en el Macrocosmos, que genera el recurso
    que le corresponde:
    - Cocina / huerto → **alimento** (sube el tope de tropas que se pueden generar).
    - Mecánica → **catapultas / máquinas de guerra**.
    - Herrería / textil → **mejoras de armas y armaduras** (más fuerza/defensa de los personajes).
    - Cualquier edificio → **mejoras del propio edificio** (más defensa, incluso más puntos de vida).
  - **Aporte pasivo + activo.** Ya existe una economía global que genera recursos en cada área de forma
    pasiva y activa (misiones del Microcosmos y del Mesocosmos/simulación). En el Macrocosmos, cada
    cierto tiempo cada estructura entrega su **aporte pasivo lineal**; y si el jugador **asigna
    personajes a un área** (como mandar un peón a la mina de oro en Warcraft), esa área **potencia su
    producción pasiva y genera aporte activo** extra.
  - **Farming** — también se obtienen recursos venciendo a las criaturas de los alrededores (§4.1).

- **Guerra no letal.** Nadie muere: al llegar la energía al mínimo, el personaje **se rinde**. La
  destrucción es de **estructuras**:
  - Hechizos muy poderosos van debilitando la existencia de una estructura.
  - Antes de desaparecer, la estructura **emana luz**; sobre ella se forma un **agujero negro** que la
    absorbe hasta que pierde toda su vida.
  - **Reparar** una estructura corta la emanación / disuelve el agujero negro.
  - *(Equivalente al fuego+humo de Warcraft; artísticamente, la luz absorbida hacia arriba, con halos
    negros opcionales.)*
  - **Coherencia temática:** esto rima con el arquetipo `AbsorbentThoughtMob` del Microcosmos (el
    pensamiento que absorbe) — el mismo motivo "absorción" en ambos planos.

- **Territorio.** Ganar territorio = permanecer en un área sin ser derrotado mientras construyes un
  fuerte y obtienes recursos. Genera asaltos pequeños, o la llegada de un ejército poderoso, o acuerdos.
  Puede haber alianzas, 1v1, 2v1, todos-contra-todos.

- **Guerras grandes entre santuarios.** Involucran a la mayoría/totalidad de un santuario, en un área
  intermedia, cediendo/empujando hasta que el ganador llega al asentamiento enemigo y lo destruye.
  Fin de la guerra → **misiones de reconstrucción**.

- **Migración de arquitectos → mezcla de estilos.** Tras las guerras, los personajes migran entre
  áreas/santuarios. Un santuario reconstruido por arquitectos de otro (p.ej. acuáticos) tendrá
  edificios con ese estilo; con arquitectos mezclados, estilos mezclados. Modelar como un **tag
  `style` por constructor** que "estampa" lo que levanta.

- **Reconstrucción → avance de época.** Los edificios nuevos son más modernos. La guerra + la
  reconstrucción son **el mecanismo que hace avanzar de época** a un santuario (conecta con
  [`mob-epochs-matrix.md`](mob-epochs-matrix.md)). Por eso un santuario que se distanció de la magia
  (el terrestre inicial, que siempre acogió a los novatos) es el que **mayor impacto** genera al
  reconstruirse.

### 4.1 Combate como juego — farming no-violento

Los enfrentamientos no buscan dañar, sino **provocar gran alegría** a las criaturas del santuario. La
inspiración es **jugar con un gato o un perro**: mueves la mano muy rápido, como una araña, acercándote
de frente y alejándote de golpe para que se motiven a atraparla; les tomas una pata en un instante y
luego la otra cuando intentan agarrarte, y la cola — todo con suma suavidad pero tan rápido como para
captar su atención y desafiarlas. Se emocionan, muerden y arañan jugando, y al terminar quedan
**exhaustas, serenas y dóciles**, habiendo descargado toda su tensión.

Mecánicamente:
- Cada criatura tiene **puntos de tensión**; el "combate" los va bajando (no puntos de vida).
- Al llegar a cero, la criatura queda **tranquila y serena**, **deja de poder ser objetivo** (target), y
  suelta:
  - **Experiencia** para los personajes → suben de nivel (+puntos de vida, +maná).
  - **Recursos**.
  - **Objetos útiles** — consumibles estilo Warcraft: invocadores de aliados, potenciadores de
    ataque/defensa, restauradores de vida/maná.
- **Cuidado tras el juego (cierra el bucle):** una vez serena, se le **provee comida y agua** para que
  **se sacie y descanse**. Refuerza el pilar de **cuidado** del santuario (jugar + cuidar, no dañar) y
  deja a la criatura en estado dócil/contento (posible bonus de vínculo; no vuelve a ser objetivo por un
  tiempo).
- Encaja con el pilar de no-violencia: la fauna real es inofensiva al principio; las criaturas poderosas
  de los santuarios profundos tienen **más tensión que descargar** (más difícil) y **mejor recompensa**.

**No siempre se puede jugar (dinámica DESBLOQUEABLE):** que una criatura acepte jugar depende de su
**vínculo (bond)** y su **estado**. Solo entran en modo juego las **criadas por humanos**, las de
**vínculo suficiente**, o las que están en **relajación profunda**. Si te acercas a, por ejemplo, un
**oso polar salvaje** que no fue criado por humanos ni está relajado, **NO** hay juego: rige la **ley
natural** (el sistema de depredación/amenaza que ya existe — `Animal.SenseThreats`/`EvaluateThreat`,
dietas con humano como presa), y el carnívoro puede **cazar** al jugador y a otros personajes. Subir el
vínculo (jugar + cuidar) es lo que va **desbloqueando** el juego con criaturas cada vez más peligrosas.

**El juego NO es del todo inofensivo:** aun jugando, la criatura puede **hacer daño si no esquivas**. Y
**cuanta más excitación**, **más probable que pierda el control de su fuerza**: al emocionarse te
consideran un **oponente digno** y se van **liberando de restricciones** (te ven fuerte y no creen que
vayan a lastimarte). Así, el combo tiene tensión real: descargas más rápido con más excitación, pero el
riesgo de un golpe "sin control" también sube → hay que **jugar el ritmo y esquivar**, no solo tapear.
*(La vida que da subir de nivel cobra sentido aquí: aguantar juego más rudo.)*

### 4.2 Transporte entre santuarios separados

¿Cómo se guerrea entre lugares tan separados como tierra firme, cielo, fondo marino y subsuelo? Opciones
(combinables):
- **Vehículos temáticos:** submarinos (marino), globos/dirigibles (aéreo), trenes o elevadores
  (subsuelo). Que **solo embarquen y desembarquen** tropas —sin atacar— y el combate se resuelva **sobre
  el área del santuario**; o bien permitir combates también en aire/agua.
- **Teletransportadores (recomendado).** A esta altura la magia es evidente, así que unos portales de
  teletransporte resuelven el traslado de forma limpia y **sirven en los tres planos** (Mesocosmos para
  viajar, Macrocosmos para mover tropas). Los vehículos quedan como **sabor/alternativa** por santuario
  (p.ej. el marino con submarinos), no como requisito.
  - *Modelo "aeropuerto":* en cada área hay un lugar con **un teletransportador por santuario** (una
    pequeña terminal); son **bidireccionales**.
  - *Rollout (idea):* al principio existe **solo** el enlace **lava (Núcleo) ↔ Subterráneo**; **tras la
    primera guerra** se empiezan a construir los demás como **red de comunicación entre todos los
    santuarios**. (Encaja con reconstrucción→avance de época.) Pendiente confirmar si aparecen ya en la
    1ª guerra Macro o después (§9).

### 4.3 Cámara y mapa

Como en Warcraft, en el Macrocosmos las áreas se muestran **pegadas/contiguas** en un mapa, con un
modelo **tirando a 2D** y **paredes invisibles**, para ver con claridad el interior y sobre todo el
**subsuelo** sin que la geometría estorbe. Contrasta con la cámara 3ª/1ª persona del Mesocosmos.

---

## 5. Progresión narrativa

- El santuario se muestra al principio como un lugar **muy normal**; todo esto es secreto para los
  nuevos. La magnate ofrece a los que llegan subir de puesto con misiones/entrenamiento, pero es
  **opcional** — Goluis trabajó meses/un año ajeno a todo hasta decidir ir más profundo.
- Kushal (el jugador) recorre el mundo poco a poco (§2). Cuando se vuelve importante para los
  personajes, accede al **Macrocosmos** y eventualmente a la misión de **comandar una guerra**.
- Se le revela que esta es **la primera vez** que el primer santuario participa en una guerra (hasta
  ahora solo acogía novatos, distanciándose de la magia).
- **Disparador de la primera guerra:** se desata cuando las misiones resueltas han dado a Kushal el
  **poder** necesario **y** el santuario ha acumulado suficientes **recursos, construcciones e
  investigaciones**. A partir de ahí se le asignan misiones estratégicas muy variadas (ver §7).
- **Endgame — la conspiración y el arco final (idea inicial, puede cambiar):**
  - Los personajes más poderosos del Macrocosmos **poseen partes de los santuarios a su nombre** y son,
    supuestamente, los **socios de la Magnate**.
  - Kushal se gana su confianza: en misiones Macro **derrota a cada socio en guerras** y también **sigue
    sus órdenes**, leal y amigable, ganándose su admiración. Poco a poco le **revelan sus sueños** y se
    atisba que van a **destronar a la Magnate**, hasta que se lo dicen directamente.
  - Se abre la **trama final**: Kushal **comanda ejércitos desde el Macrocosmos**, luego baja al
    **Mesocosmos para liderar el campo de batalla**, y finalmente **derrota al santuario Subterráneo** y
    sus criaturas.
  - Entra al **mundo invertido del Núcleo** (§3.2, capa 2), donde solo están **el sol y la Magnate**.
    Los socios entran **después** de él y empieza la **batalla final**.
  - Al derrotar a la Magnate y **quitarle sus poderes**, una figura se acerca desde la esfera: es
    **Leo**, que toma a la Magnate y **desaparece**. (Gancho para lo que venga después.)

---

## 6. Guionizado vs emergente — híbrido

- **Espina dorsal guionizada:** la **primera guerra** de Kushal es un set-piece con desenlace
  narrativo garantizado, para que la historia aterrice.
- **Resto emergente:** las guerras siguientes se resuelven con **probabilidades ponderadas** — un
  santuario "favorito a perder" puede terminar ganando según el juego del jugador y los eventos.
- **La reconstrucción hace que perder no sea un callejón sin salida**, así que la pérdida emergente
  de un santuario es recuperable (y avanza su época, §4).

---

## 7. Acoplamiento Meso ↔ Macro (tiempo, cambios y guerras)

La lógica del Macrocosmos **también gobierna los cambios que se ven en el Mesocosmos**: son la misma
partida a dos velocidades y escalas.

**Tiempo y construcción (dos velocidades):**
- **Mesocosmos = lento / real.** Las construcciones tardan **muchísimo** (horas, o atadas al avance de
  la narrativa de misiones). El jugador, a pie, ve **de frente** las obras en progreso, a **otros
  personajes participando** en ellas, y los **recursos globales del santuario** creciendo y decreciendo
  según lo que se construye. Se pueden incrustar **misiones de simulacro/Microcosmos dentro de una
  construcción**. Aquí se **maximizan las acciones** (detalle, presencia).
- **Macrocosmos = rápido (estilo WC3).** El tiempo corre muchísimo más: las estructuras se terminan en
  **minutos o segundos**. Es la vista de gestión/estrategia.

**El mundo crece a la vista (Mesocosmos):** desde que el jugador entra ve, **poco a poco**: al principio
p.ej. **4 áreas construidas y 1 en construcción**; luego una nueva empieza a levantarse mientras otra
termina; los personajes se **vuelven más poderosos**; surgen **conflictos por terrenos** y **combates
de arena**. (Motor: el mismo director de eventos `MobWorldDirector`, §8.)

**Visibilidad de recursos.** En el Mesocosmos el jugador ve los **stats de recursos del santuario en el
que se encuentra** (crecen/decrecen a la vista). **Durante una guerra**, solo puede ver los del
**santuario del que forma parte como integrante**, hasta que la guerra termine.

**Guerras en dos modos.** Habrá guerras que se juegan en **modo Meso** y otras en **modo Macro**; ambas
tienen su gracia.
- **Guerra Macro** = RTS clásico: crear ejércitos/estructuras rápido, atacar/defender.
- **Guerra Meso (recomendado: la versión "real", no un RTS lento).** En vez de crear imperios y
  ejércitos deprisa, es la **culminación encarnada** de todo lo que el jugador vio crecer lentamente:
  entra a pie, con misiones estratégicas muy variadas. Esto es lo que hace **distintos** los dos modos.
  - *Alternativa (descartable):* comprimir el tiempo y controlar **un solo personaje** mientras a los
    demás los mueven "jugadores" del nivel Macro. Se anota, pero diluye la distinción entre planos.
    **Pendiente de confirmar** (§9).

**Mecánicas en tiempo de guerra:**
- **Todos los recursos generados se incrementan** y la **velocidad de construcción sube**, pero solo
  para **reparaciones/reconstrucciones** y **estructuras de guerra** (miradores/torres que disparan,
  estilo WC3).
- **Misiones de guerra muy variadas:** obtener recursos en las áreas de producción; **farming de
  criaturas poderosas** (§4.1) para artefactos útiles de guerra, experiencia (subir stats) y recursos;
  y **combates entre ejércitos** como atacante o defensor.

---

## 8. Cómo conecta con el código existente

- **Microcosmos = ya existe.** `VirtualizationMachine` + `MeditationSession`/`MobWorldLoader` +
  misiones de `MeditationMissionBase`. La cocina→Mesopotamia ya es jugable end-to-end
  (ver [`mob-world-architecture.md`](mob-world-architecture.md) §13).
- **Hub-and-spoke = ya soportado.** Cada mundo es una escena aparte a origen lejano con transform de
  retorno (`MobWorldLoader`). Es hub-and-spoke técnico.
- **Mundo vivo / cambios a vista del jugador = `MobWorldDirector`** (eventos Migration/Invasion/
  Festival). Es el motor natural para las migraciones de arquitectos y los cambios de santuario.
- **Épocas = [`mob-epochs-matrix.md`](mob-epochs-matrix.md)**; la reconstrucción es su disparador.
- **Macrocosmos = por construir.** Nada de la capa RTS existe aún (unidades/roles, recursos,
  construcción, agujero-negro de estructuras, IA de guerra).

---

## 9. Decisiones abiertas / TODO

- [x] Núcleo = **5º santuario** de **dos capas**: diamante-sobre-lava (gravedad normal) + mundo
      invertido del núcleo (gravedad invertida, área final). Resuelto 2026-07-23 (§3.2).
- [x] Renombrado **plano mágico → Microcosmos** + tríada Micro/Meso/Macrocosmos en docs (2026-07-23).
      El término de código "mob" se mantiene.
- [ ] **Modelo de guerra en modo Meso** (§7): confirmar la versión "real"/encarnada (recomendada) vs la
      de tiempo comprimido + un solo personaje. **Decisión clave pendiente.**
- [ ] Transporte (§4.2): confirmar **teletransporte** como base y su **rollout** (¿1–2 al inicio y se
      multiplican tras la 1ª guerra como comunicadores entre santuarios?) + qué vehículos de sabor por
      santuario (submarino/globo/tren-elevador).
- [ ] Combate-como-juego (§4.1): números de **puntos de tensión** por criatura, curva de XP y tabla de
      drops/consumibles.
- [ ] Macrocosmos: árbol de construcción y costes; qué recurso produce cada área exactamente; fórmula
      de aporte pasivo lineal + bonus por personaje asignado.
- [ ] Cámara 2D-ish del Macrocosmos (§4.3): cómo se "aplanan" y pegan las áreas y se ocultan paredes.
- [ ] Modelo de datos de `style` por arquitecto y cómo estampa las estructuras (§4).
- [ ] Regla de desbloqueo del Macrocosmos ("volverse importante"): ¿qué lo dispara exactamente?
