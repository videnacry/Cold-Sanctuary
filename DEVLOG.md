# Cold Sanctuary — Dev Log & Contexto de Diseño

> **Áreas de trabajo paralelas e independientes:**
> - **Fauna** — comportamiento animal, ciclos post-natales, vínculo jugador↔cría.
>   Diseño técnico: [`docs/behavior-system.md`](docs/behavior-system.md) · Gameplay: [`docs/fauna-gameplay.md`](docs/fauna-gameplay.md)
> - **Jugador** — stats, asanas, ejercicios, compañeros, narrativa. Este archivo.

## Estado actual

### UI / Interfaces
- [x] **Radar de animales** (`AnimalRadar.cs`, `FollowingArrays.cs`)
  - Botones flotantes frente al jugador, uno por animal cercano
  - Se activa con tecla Z, se cierra con Escape
  - Diseñado para Desktop y Mobile: cada ítem es clickeable Y tiene tecla asociada
  - Las teclas se eligen para alinear al usuario al uso del teclado de escritura (sin flechas)
  - Actualmente representado como "encantamiento" — la presentación visual cambiará

---

## En progreso / Por hacer

### 1. Interfaz de Encantamientos (rediseño)
- [ ] El jugador selecciona elementos químicos de la tabla periódica por cada encantamiento
- [ ] Un botón por elemento (similar a como ahora hay uno por animal)
- [ ] Botones más pequeños que los del radar, con distribución diferente para no cubrir toda la pantalla
- [ ] Mantener compatibilidad Desktop (tecla) + Mobile (click)

### 2. Interfaz de Posturas / Asanas

**Progresión de desbloqueo:**
- [ ] El jugador empieza SIN ninguna asana desbloqueada
- [ ] Las asanas se desbloquean a través de un profesor o texto/escrito encontrado en el mundo
- [ ] Las primeras actividades disponibles son ejercicios básicos (ver sección Ejercicios Básicos)

**Asanas disponibles (primer set):**
| Nombre | Nombre sánscrito correcto | Beneficio esperado |
|---|---|---|
| Urdhva Hastasana | Urdhva Hastāsana | (por definir) |
| Janu Sirsasana | Jānu Śīrṣāsana | (por definir) |
| Paschimottanasana | Paścimottānāsana | (por definir) |
| Virabhadrasana I | Vīrabhadrāsana I | (por definir) |
| Virabhadrasana II | Vīrabhadrāsana II | (por definir) |
| Sarvangasana | Sarvāṅgāsana | (por definir) |
| Matsyasana | Matsyāsana | (por definir) |

**Mecánica:**
- [ ] Un botón por posición corporal — las posiciones son siempre las mismas (absolutas, no contextuales)
- [ ] El jugador selecciona todas las posiciones que forman la asana
- [ ] Si el match es perfecto → empieza a recibir el beneficio
- [ ] Los beneficios continúan hasta alcanzar el límite del contenedor
- [ ] Si la asana tiene dos lados → cambia de lado automáticamente y continúa recibiendo beneficios
- [ ] Se puede encolar la siguiente asana mientras se está en una activa → transición automática al terminar
- [ ] Mantener compatibilidad Desktop (tecla) + Mobile (click)

### 3. Ejercicios Básicos (disponibles desde el inicio, también en esterilla)

Los ejercicios básicos no son asanas pero usan el mismo sistema de selección de posiciones corporales. Su función es:
- Liberar tensión
- Ganar fuerza
- **Agrandar el límite de los contenedores de stats** (fuerza, enraizamiento, etc.)

**Ejercicios iniciales:**
- Flexiones (push-ups)
- Piernas
- Abdominales

**Ejemplo de posiciones para flexiones:**
- "Manos a altura de hombros"
- "Codos detrás"
- "Pies juntos, talón hacia arriba"

- [ ] Definir posiciones completas para cada ejercicio básico
- [ ] Definir qué contenedor agranda cada ejercicio y en cuánto

### Partes del cuerpo a posicionar (sistema de posiciones)

Cada postura/ejercicio define el estado de estas partes:
- Codos
- Manos
- Rodillas
- Pies
- Cadera
- Espalda
- Hombros
- Cabeza

*Nota: simplificar según feedback de jugabilidad — estas 8 partes cubren toda postura relevante.*

### Sistema de simplificación por práctica

- [ ] Tras ejecutar una postura **10 veces**, se desbloquea una **opción directa** a esa postura (sin seleccionar cada posición individualmente)
- [ ] Con la opción directa el personaje puede cometer errores en alguna posición
- [ ] El jugador solo selecciona los **arreglos** necesarios (no reconstruye la postura entera)
  - Ejemplo: si el pie de atrás no está a 45°, aparece la opción "pie de atrás a 45°" — si era el único fallo, la postura se completa
- [ ] El número/probabilidad de errores podría estar vinculado a la maestría — a mayor maestría, menos errores automáticos
- [ ] Esto también funciona como mecánica de "profesor interior": el jugador aprende a observar qué falla

### 4. Sistema de Maestría
- [ ] Cada asana tiene un nivel de maestría que sube con la práctica
- [ ] A mayor maestría: más tiempo en postura + mayor capacidad de almacenamiento de beneficios
- [ ] Ejemplo — "Fuerza y Enraizamiento":
  - Capacidad inicial: 50
  - Incremento: +1 cada 5 prácticas
  - Capacidad máxima: 100
- [ ] Definir qué otros beneficios/stats siguen esta progresión

---

## Narrativa — Nivel 1

**Setup:**
Un grupo de amigos llega a un santuario animal. En el santuario se trabaja para hacer a los animales amigables entre sí y con las personas.

**Motivación del grupo:**
La mayoría no llegó al santuario buscando formar parte de él — llegaron con una agenda: tienen pruebas de que la maestra del lugar realiza cosas paranormales (magia) y quieren aprender sus secretos.

**Tensión central:**
- Los personajes viven la experiencia del santuario (cuidado de animales, convivencia, naturaleza)
- Pero por debajo operan con curiosidad/ambición hacia la magia de la maestra
- Esto crea una dualidad entre lo que hacen (cuidar animales, practicar yoga) y lo que buscan (acceso a conocimiento prohibido o secreto)

**Secuencia de introducción — Nivel 1:**
1. La maestra recibe al grupo y los felicita por haber encontrado el lugar
2. Les dice que ve potencial en los integrantes
3. Les propone: para conocer los secretos del lugar, deben participar como voluntarios
4. Lleva al equipo a la cabaña
5. Les muestra la esterilla
6. Pide al jugador que se acerque a la esterilla
7. Se revela el **status físico** del jugador (stats iniciales, contenedores vacíos)
8. El jugador hace sus **primeras flexiones, abdominales y piernas** — tutorial del sistema de posiciones corporales
9. La maestra comenta el **nivel actual del jugador y cuánto le falta para avanzar** (feedback de progresión en pantalla)
10. La maestra lleva al equipo al **área de crías** — primera zona de tareas
11. Primeras tareas: alimentar, cuidar y jugar con las crías (ver [`docs/fauna-gameplay.md`](docs/fauna-gameplay.md) para el detalle de actividades)
12. Objetivo: que las crías se conviertan en **adultos amigables** (con personas y entre sí)
13. Cuando el jugador alcanza un umbral de estadísticas + vínculo con las crías, la **maestra aparece** y le enseña su **primera postura de yoga** — cierre del Nivel 1 y desbloqueo del sistema de asanas

**Misiones secundarias del Nivel 1:**
Ver sección "Misiones Secundarias — Compañeros" más abajo.

**Implicaciones de diseño:**
- El santuario es el hub principal del nivel 1
- Los animales son personajes con los que el jugador interactúa (sistema de fauna ya en código)
- La maestra es un NPC clave — fuente de desbloqueo de asanas y conocimiento mágico
- Los encantamientos (tabla periódica) probablemente se descubren a través de la maestra o de escritos del santuario
- La tensión grupo/maestra puede generar misiones, diálogos y momentos narrativos

---

## Misiones Secundarias — Compañeros (Nivel 1 en adelante)

### Principio de diseño

No todos los compañeros llegan al santuario al mismo tiempo ni con el mismo nivel:

- **Antiguos del santuario** — llevan más tiempo y en general tienen stats más elevadas, aunque no necesariamente en todas las áreas. Algunos serán claramente superiores al jugador; otros, sorprendentemente cercanos o incluso inferiores en alguna stat concreta.
- **Compañeros del grupo de llegada** — llegan junto al jugador o poco después. Pueden tener stats similares, menores o mayores dependiendo de su historia. No ser "del grupo" no implica ser peor.
- **Compañeros que aparecen después** — pueden llegar en cualquier momento del juego con cualquier nivel. Un recién llegado puede resultar extraordinariamente hábil en algo; otro puede estar empezando desde cero.

Esto elimina la lógica de "más antiguo = mejor" y hace que cada encuentro sea una sorpresa. Cada compañero destaca en una o más estadísticas y tiene una **historia de trasfondo** que explica (o sugiere) su nivel — el jugador la va descubriendo al pasar tiempo con ellos.

Los compañeros **crecen mientras el jugador no los ve**: al reencontrarlos en el Nivel 2, habrán avanzado a su propio ritmo — lo que puede crear admiración, sorpresa o incluso la sensación de haberles alcanzado.

### Especialización por compañía

El tiempo que el jugador pasa con cada compañero influye en qué estadísticas sube más. No hay una elección explícita — emerge del comportamiento:

- Pasar tiempo con compañeros de **velocidad/elegancia** → el jugador gana más velocidad
- Pasar tiempo con compañeros de **fuerza** → el jugador gana más fuerza
- Repartir el tiempo entre todos → progresión más balanceada pero menos pronunciada en ninguna
- Dedicarse principalmente a **entrenar en la esterilla** → subida de stats según los ejercicios elegidos, independiente de los compañeros

Esto crea perfiles de jugador distintos sin forzar una elección: el jugador simplemente vive la experiencia y el resultado refleja sus prioridades.

### Stat: Observación

La Observación es la capacidad de leer el entorno con detalle. No es una herramienta para conseguir cosas — es una práctica en sí misma. A veces puedes mirar sin buscar algo, simplemente mirar por ver qué hay allí.

**Implementación — Radio de consciencia:**
- La stat define un radio alrededor del jugador
- Dentro del radio, los objetos observables emiten una señal sutil y diegética: un brillo leve, una partícula, un sonido suave — nada de borde de UI genérico
- Al acercarse lo suficiente, aparece un pensamiento interno breve (no de UI): *"esto huele diferente"*, *"las hojas de esta planta tienen algo"*
- El radio crece visiblemente con la stat — el jugador siente el cambio
- Jugadores con poca Observación pasan por alto cosas que están ahí

**Modo contemplativo:**
- El jugador puede detenerse y simplemente mirar el entorno — sin objetivo, sin timer
- El mundo devuelve algo: no siempre un ítem o pista, a veces un detalle, una textura, un comportamiento de animal que no vería en movimiento
- Practicar el modo contemplativo sube la stat de Observación por sí solo, independientemente de los compañeros
- Mirar como práctica — coherente con el alma del juego

**Usos en Nivel 1:** notar ingredientes, comportamientos de animales, detalles del entorno
**Usos posteriores:** puente hacia la meditación (observación interior) y hacia las recetas mágicas de la tabla periódica

---

### Compañera: Panterilia

**Trasfondo:**
Mujer muy trabajadora que huyó de su país buscando poder llevarse a su hija consigo. No dejó los estudios para abrir oportunidades y finalmente lo logró, pero pasaron años en el proceso — con lo cual la relación con su hija no es la más cercana. Se piden permiso para darse abrazos. La hija parece más conectada con su abuela, que la conoce más.

Panterilia no es capaz de hablarle mal del padre a su hija, ni de explicarle que él es una de las razones por las que sintió la necesidad de huir. Tiene que seguir en contacto con él porque la hija es menor de edad y necesita permisos. Si hubiera sido por Panterilia, nunca hubiera hecho saber al padre de la existencia de la hija — pero cuando dio a luz, su madre la convenció de agregarle el apellido del padre. Lo considera un gran error: ahora debe contactarlo ocasionalmente y él no coopera, se hace de rogar incluso con trámites beneficiosos para su hija, y la acusa de buscarle con excusas.

**Stats y nivel de llegada:**
Llega al santuario poco después del jugador — no viene con stats elevadas. Es trabajadora, no una experta; su crecimiento se verá durante el juego.

**Arco en Nivel 1:**
Panterilia era la reina de la limpieza — pero usaba muchos químicos y les tenía asco a los insectos. En el santuario llega a encariñarse con ellos y empieza a investigar componentes naturales que mantengan la limpieza sin dañar ninguna forma de vida ni el ecosistema. Esto la lleva a realizar tareas de limpieza del lugar y búsquedas de ingredientes naturales.

Tiene un talento innato para la nutrición — fruto de años preocupándose por la alimentación de su hija. Al final del Nivel 1 decide que quiere entrar en el mundo de la nutrición animal. Si el jugador pasa mucho tiempo con ella, también se preocupa por su alimentación.

**Ganancias del jugador al acompañarla (Nivel 1):**
- **Observación** — aprender a distinguir plantas, insectos, ingredientes; Panterilia enseña literalmente a "ver": la primera misión podría ser que el jugador no nota nada y ella sí
- **Puntos de consciencia** — semilla para la meditación posterior; aún sin uso pleno en Nivel 1 pero se acumula
- **Conocimiento de nutrientes** — base para recetas mágicas de la tabla periódica en niveles posteriores

**Arco en Nivel 2:**
Se está llevando mejor con su hija y quiere darle más confianza — abrirse a ella como compañeras que trabajan codo con codo. Asciende a formadora y da clases a nuevos participantes, o a antiguos de otras áreas que se acercan a su área de limpieza y nutrición. Sigue incrementando sus stats de Observación, consciencia y meditación — con cambio visual/energético perceptible tanto en ella como en el jugador que la acompaña.

**Misión de ejemplo — Nivel 1:**
El jugador la encuentra limpiando una zona del santuario. Ella detecta plantas y insectos que el jugador ni ve. Al unirse, el jugador aprende a mirar — primer ejercicio práctico de Observación.

---

### Misiones secundarias (otros compañeros — por definir)

**Misión: Barrer con [Nombre]**
- Compañero barre con velocidad y elegancia que parece sobrenatural
- Al ayudar, el jugador gana **velocidad** y **coordinación**
- Historia de trasfondo: por definir
- Reencuentro en Nivel 2: aún más rápido — el jugador puede seguirle el ritmo un poco mejor

**Misión: Mover material con [Nombre]**
- Compañero carga materiales con fuerza llamativa
- Al ayudar, el jugador gana **fuerza**
- Historia de trasfondo: por definir
- Reencuentro en Nivel 2: levanta más — el jugador nota que él también puede más

---

### El Maestro Goluis — Cocina

Respetado por todos en la cocina. Llegó al santuario antes que el jugador — es un nivel más antiguo. Al principio puede ser de los que más presión ponga: le dirá al jugador que no le gusta enseñar y soltará comentarios sobre prisa y organización.

**Mecánica de presión:**
- Sus comentarios otorgan un pequeño bono de velocidad para terminar la tarea en curso → el jugador tiene más tiempo para otra misión
- Pero agotan física y mentalmente
- Si el agotamiento mental supera cierto umbral, el jugador tambalea — lo que afecta el tiempo final de la misión
- Riesgo real: seguirle el ritmo puede salir caro si ya se viene cargado

**Trasfondo:**
Se trajo a la madre de su hija de vacaciones al santuario. La madre no quiso quedarse y volvió al país con la niña. Le llegaron noticias de que la madre no cuida bien a la niña. La niña le pidió quedarse con él. Cuando tuvo la oportunidad, se la trajo — y la niña le convenció de quedarse definitivamente.

La madre amenazó con una denuncia. Goluis era bandido antes y tiene contactos que disuadieron a la madre de tomar acciones legales. Intenta hablar con ella por teléfono para reconciliarse y ser una buena familia para la niña, pero ella no quiere — dice que solo se acostó con él por el alcohol y ahora sale con otro. Goluis hace dos trabajos en el santuario para aportar más económicamente. Vive con su mamá y dos hermanos; su madre le ayuda a cuidar a la niña. Trabaja muy rápido para no tener problemas en ninguno de los dos trabajos por falta de sueño. Ha sufrido varias bajas: problemas de pie, extracción de un testículo, media cara paralizada, y varios accidentes.

**Ganancias del jugador al acompañarle:**
- **Velocidad** — sus comentarios de presión la activan, con riesgo de fatiga
- **Resistencia** — aguante físico y mental ante la carga. Pasar tiempo con alguien que trabaja doble turno con el cuerpo dañado y sigue en pie enseña algo sobre límites. Base para posturas de yoga más exigentes en niveles posteriores.

**Arco en Nivel 1 y 2:**
Goluis empezó solo por dinero, sin interés en el entrenamiento ni fe en el yoga. Con el tiempo empieza a soltar mensajes amigables: le dice al jugador que lo considera su amigo, que quiere que vaya a su cumpleaños, lo recibe con una sonrisa o cara cómica y un abrazo. Le pregunta directamente al jugador si lo considera su amigo. Sale con una mujer mayor y va aprendiendo a tratar a todos con más paciencia. Al final del Nivel 2 gana humildad, cariño y paciencia suficiente para comenzar a entrenar — y así apuntar a algo más.

---

### Diseño de los compañeros

- [ ] Definir nombres y perfiles de cada compañero restante
- [ ] Definir la stat principal (y secundaria) de cada uno
- [ ] Escribir trasfondo de cada uno (puede revelarse en fragmentos)
- [ ] Definir cómo y cuándo reaparecen en Nivel 2 mostrando su crecimiento
- [ ] Establecer el umbral de "tiempo con compañero" que influye en la especialización del jugador

---

## Narrativa — Nivel 2

**Condición de activación:**
El jugador ha fortalecido suficientemente el lazo con las crías a su cargo (métrica de vínculo por definir). Las crías entran en sus ciclos naturales — momentos en que no necesitan atención.

**Mecánica central — Ciclos de las crías:**
- Las crías alternan entre fases de necesidad y fases de descanso/ciclo autónomo
- Durante las fases autónomas aparece una **cuenta regresiva** visible en pantalla
- El jugador queda **libre para moverse por el mundo** hasta que se agote el tiempo
- Al finalizar el tiempo, el jugador deja lo que esté haciendo y **corre de vuelta al puesto**
- La urgencia crea ritmo y tensión natural sin interrumpir el flujo de exploración

**Actividades durante el tiempo libre:**
- Hablar con **NPCs del mundo** que ofrecen misiones secundarias
  - Las misiones otorgan mejoras en estadísticas (fuerza, enraizamiento, etc.)
- Volver a la **esterilla y entrenar** (ejercicios básicos, asanas desbloqueadas)
- Exploración libre del mapa

**Progresión del nivel:**
- El jugador sigue subiendo estadísticas y fortaleciendo el lazo con las crías
- La frecuencia y duración de los ciclos puede variar — más tiempo libre conforme crece el vínculo, o menos si las crías se vuelven más demandantes al crecer
- El sistema de asanas ya está desbloqueado (la maestra lo enseñó al cierre del Nivel 1) — el jugador lo integra ahora con libertad

---

## Comentarios y decisiones de diseño

- **Teclas del teclado**: se alinean a posición de escritura para que el jugador pueda usarlas naturalmente, sin tener que mover las manos a las flechas.
- **Encantamientos como tabla periódica**: idea interesante — los elementos químicos como metáfora de poderes/encantamientos crea coherencia temática (naturaleza, ciencia, magia).
- **Asanas como mecánica de recuperación/fuerza**: elegante porque requiere atención activa del jugador (seleccionar posturas) pero no interrumpe el flujo del juego (se puede encolar). La progresión por maestría premia la práctica constante.
- **Límite de almacenamiento por stat**: buen sistema de cap dinámico — empieza bajo para no abrumar al jugador nuevo y crece orgánicamente.
- **Disponibilidad de la interfaz de posturas — dos opciones**:
  - **Opción A (ideal)**: calcular en tiempo real si hay espacio suficiente para ejecutar la postura sin colisionar con el entorno. Más flexible, pero puede impactar rendimiento — evaluar antes de implementar.
  - **Opción B (world design)**: la interfaz solo está disponible sobre esterillas (`yoga mat`) repartidas por el mundo. Elimina el cálculo de colisión, añade intención de diseño (el jugador busca y descubre esterillas), y puede usarse para guiar al jugador por zonas del mapa. **Preferida si Opción A afecta rendimiento.**
- **Posturas**: siempre las mismas opciones independientemente del contexto (posición del jugador, terreno, etc.)

---

## Arquitectura relevante

| Archivo | Responsabilidad |
|---|---|
| `AnimalRadar.cs` | Posiciona botón flotante apuntando al animal (LookAt + Translate) |
| `FollowingArrays.cs` | Sistema base de menús flotantes: instancia, cola, mostrar/ocultar |
| `FollowingArrayInScript.cs` | Renderiza arrays de UI desde scripts |
| `FollowingArrayInArray.cs` | Renderiza arrays de UI desde arrays |
| `FollowingElementBehavior.cs` | Clase base para comportamientos de elementos UI |
| `HashSetHolder.cs` | Gestión de población de animales para el radar |

---

## Ideas / Backlog sin prioridad

- Definir lista completa de posiciones corporales (los "botones") que componen cada asana y ejercicio
- Decidir distribución visual de la tabla periódica en pantalla
- Explorar si la mecánica de asanas tiene feedback visual/sonoro mientras se ejecuta
- Evaluar rendimiento del cálculo de espacio libre para posturas (ver decisión de diseño abajo)
