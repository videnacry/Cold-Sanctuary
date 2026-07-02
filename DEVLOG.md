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

### Stat: Satisfacción

La Satisfacción es la capacidad interna del jugador para celebrar la vida — desde el simple hecho de respirar hasta los grandes momentos. No es una emoción pasajera: es una stat que crece y que tiene efectos mecánicos reales.

**Estructura:**
- El jugador tiene una **barra de Satisfacción** propia, independiente de cualquier compañero
- El **tamaño** de la barra crece con el trabajo del umbral → más capacidad = mayor resistencia a ataques fuertes directos y a ataques continuos sostenidos
- La **velocidad de llenado pasivo** también crece → en niveles altos la barra se restaura sola con el simple hecho de existir, sin necesidad de acción activa
- La restauración pasiva **se suma** a restauraciones externas (compañeros, comida, descanso, yoga) — no las reemplaza
- En niveles muy desarrollados puede incorporar un **multiplicador a las restauraciones externas**: las fuentes externas curan más porque el cuerpo ya está en mejor disposición de recibir

**Formas de restauración — varios canales:**
Distintos compañeros y actividades activan distintos canales de restauración. El jugador gravitará naturalmente hacia los que más le resuenen, y eso define parte de su perfil:
- **Celebración / alegría compartida** — canal de Gohageneis
- **Calma contemplativa** — canal de Panterilia (Observación, consciencia)
- **Desahogo / esfuerzo físico** — canal de Goluis (resistencia)
- *(Más canales por definir con futuros compañeros)*

**Restauración por cercanía a compañeros:**
Estar cerca de ciertos compañeros restaura al jugador pasivamente, sin necesidad de completar una misión o activar nada. Cada compañero tiene su propio radio y ritmo de influencia.

**Implicaciones de rol:**
Las stats que el jugador priorice determinan su rol natural a futuro:
- Satisfacción alta + restauración externa maximizada → **Healer / Tank**
- Velocidad + resistencia al desgaste (Goluis) → **Damage / Support**
- Balance entre todas → perfil versátil, menos pronunciado en cada área

---

### El Papi Gohageneis — Crías

Artista de la vida cotidiana. Narra historias, da consejos, se une a bromear, cantar, bailar, actuar, chismosear, hacer el tonto, quejarse, repartir cariño, lloriquear, dar ánimos, exigir, perdonar, lanzarse rosas, caer en la tentación de comer ingredientes sin permiso, adorar cada detalle de sus romances, disfrutar cada nuevo invento o creación. Lleva ese gusto innato por festejar la vida.

Y otras veces cae: le han llamado la atención, le han ofendido, está cansado, siente que le dejan la parte más difícil del trabajo. Se queja — y al poco rato ya está cantando otra vez.

**Trasfondo:**
De pequeño vio que había peligros reales. Un amigo suyo fue matado por estar con una chica que tenía pareja. En el bar donde trabajaba, un hombre muy borracho le dijo que le hubiera gustado ser como él — ganarse la vida, crear buen ambiente, hacer felices a los clientes — pero que le tocó ser secuestrador. Gohageneis aprendió pronto que la alegría no es ingenuidad: es una postura filosófica frente a lo que podría haber sido.

**Tareas con las crías:**
También cuida crías. Cuando pide ayuda al jugador, lo que ocurre es que él se toma su tiempo con cada una — les habla, les hace bromas, las mima, convierte cada cría en un personaje. La tarea que debería durar diez minutos dura el doble porque Gohageneis "dirige la escena". El jugador hace el trabajo mientras él actúa. Resultado curioso: sus crías tienen vínculos excepcionalmente fuertes — el método funciona, solo que a su ritmo.

**Mecánica — CelebrationCharge:**
- Una barra que se llena mientras el jugador está cerca de Gohageneis o participando en sus momentos (bromas, canciones, tareas compartidas)
- Al alcanzar el umbral, activa un efecto de sanación interna suave y pasiva: reducción en la acumulación de fatiga mental o pequeña recuperación de desgaste ya acumulado
- Trabajar este umbral repetidamente desarrolla la **stat de Satisfacción** del jugador

**Ganancias del jugador:**
- **Satisfacción** — la stat principal: barra más grande, llenado más rápido, multiplicador a restauraciones externas en niveles altos
- **Resistencia al Desgaste emocional** — los errores y las presiones del día acumulan menos peso

**Arco:**
Gohageneis da alegría con generosidad — pero cuando cae, se levanta solo rápido, de vuelta a la actuación. Su superación es aprender a dejarse sostener: encontrar personas muy afines que lo amen también cuando está abajo, y permitirse estar en ese lugar un momento sin volver inmediatamente a la música. Alguien que da tanto necesita también aprender a recibir.

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
| `AnimalRadar.cs` | Rastrea animales con botón flotante (extiende `TrackerBehavior`) |
| `TrackerBehavior.cs` | Base reutilizable para rastrear cualquier GO: minerales, comida, amenazas |
| `FollowingArrays.cs` | Sistema base de menús flotantes: instancia, cola, mostrar/ocultar |
| `Palette.cs` | Extiende FollowingArrays con modos Direct / Formula / Hybrid / Dialogue y grupos |
| `PaletteElement.cs` | Botón individual; puede materializarse como bloque físico en el mundo |
| `MaterializationExecutor.cs` | Ejecuta la materialización de elementos de la paleta en patrones espaciales |
| `ArrangementPattern.cs` | Patrones: Stairs, Barrier, Platform, FallBreaker |

---

## Propuesta de arquitectura — IBody / IMind / IBondable

Propuesta de refactorización profunda de las interfaces de stats. **No implementar hasta
confirmar diseño completo**, pero documentar aquí para que guide las decisiones de corto plazo.

### El problema actual

`PlayerStats` mezcla stats físicas (velocity, physicalResistance) con stats mentales/emocionales
(satisfaction, mentalFatigue, stress, sleepiness, observationRadius) en un solo componente.
`IAnimal` describe el cuerpo físico de los animales, pero el jugador y los NPCs también tienen cuerpo.
`CompanionBase` duplica parcialmente stats mentales (fatigue, stress, mood) sin interfaz común.

### La propuesta

Tres interfaces ortogonales que cualquier entidad puede implementar:

```
IBody      — estado físico: velocidad, resistencia, temperatura, reservas de grasa, hambre
IMind      — estado mental/emocional: satisfacción, fatiga mental, estrés, sueño, observación
IBondable  — capacidad de vínculo: bond con el jugador, efecto por proximidad
```

**Quién implementa qué:**

| Entidad | IBody | IMind | IBondable |
|---|---|---|---|
| Jugador | ✅ | ✅ | — (es quien forma los vínculos, no quien los recibe) |
| Animal / Cría | ✅ | parcial (stress, vocalización) | ✅ |
| Compañero (CompanionBase) | parcial (fatigue) | ✅ (fatigue, stress, mood) | ✅ |
| Objeto / Lugar (WorldBondable) | — | — | ✅ |

### IAnimal → IBody

`IAnimal` es en realidad una descripción del cuerpo físico, no de los animales como concepto.
Renombrarlo a `IBody` lo hace aplicable al jugador, NPCs y compañeros sin cambiar su semántica.

```csharp
// Actual:
public interface IAnimal {
    AnimationsName animationsName { get; }
    bool aware { get; set; }
    void Hurt(float damage);
    IEnumerator Escape(bool team, List<GameObject> enemies);
}

// Propuesto:
public interface IBody {
    // Stats físicas
    float velocity          { get; }
    float physicalResistance { get; }
    float temperature       { get; }     // ya existe en Animal.cs
    float fatReserves       { get; }     // ya existe en Animal.cs
    // Acciones físicas
    void Hurt(float damage);
    IEnumerator Escape(bool team, List<GameObject> enemies);
}
```

### IMind (nueva interfaz)

Stats mentales/emocionales que actualmente viven en `PlayerStats` y en `CompanionBase`:

```csharp
public interface IMind {
    float satisfaction      { get; }
    float mentalFatigue     { get; }
    float stress            { get; }
    float sleepiness        { get; }
    float observationRadius { get; }

    void RestoreMind(float amount, MindChannel channel);
    void DrainMind(float amount, MindChannel channel);
}

public enum MindChannel { Satisfaction, MentalFatigue, Stress, Sleepiness, Observation }
```

### Routing en Restore()

En lugar de un solo `PlayerStats.Restore(amount, StatChannel)` que hace todo,
la lógica de restauración consulta qué interfaces implementa el target:

```csharp
// Ejemplo conceptual:
void ApplyEffect(GameObject target, EffectData effect) {
    IBody  body = target.GetComponent<IBody>();
    IMind  mind = target.GetComponent<IMind>();
    IBondable bond = target.GetComponent<IBondable>();

    if (effect.isPhysical && body != null)
        body.RestoreBody(effect.amount, effect.bodyChannel);

    if (effect.isMental && mind != null)
        mind.RestoreMind(effect.amount, effect.mindChannel);

    if (effect.growsBond && bond != null)
        bond.GrowBondWithPlayer(effect.bondAmount);
}
```

Esto permite que una sola acción (p.ej. una asana) afecte simultáneamente al cuerpo,
a la mente y al vínculo, sin que ningún sistema sepa nada de los otros.

### Stats que se mueven de PlayerStats a IMind

| Stat actual en PlayerStats | Va a |
|---|---|
| satisfaction, satisfactionCapacity, satisfactionPassiveRate, restorationMultiplier | IMind |
| mentalFatigue | IMind |
| stress | IMind |
| sleepiness | IMind |
| observationRadius | IMind |
| velocity | IBody |
| physicalResistance | IBody |

`PlayerStats` podría quedar como MonoBehaviour que implementa tanto `IBody` como `IMind`,
o dividirse en dos componentes (`PlayerBody`, `PlayerMind`) que se enlazan en el mismo GameObject.

### Implicación en los compañeros

`CompanionBase` ya tiene `fatigue`, `stress`, `mood` — podría implementar `IMind`.
Cuando el jugador está cerca de Panterilia, en lugar de `playerStats.Restore(...)` directamente,
Panterilia aplica el efecto a través de la interfaz: `mind.RestoreMind(amount, MindChannel.MentalFatigue)`.
Esto hace que el sistema de compañeros funcione para cualquier entidad con `IMind`, no solo el jugador.

### Pendientes antes de implementar

- [ ] Definir qué stats de `IAnimal` migran a `IBody` vs quedan solo en `Animal.cs`
- [ ] Decidir si `PlayerStats` se divide en dos componentes o implementa ambas interfaces
- [ ] Definir `MindChannel` y `BodyChannel` como reemplazos de `StatChannel`
- [ ] Evaluar impacto en `CameraManager` (lee directamente de `PlayerStats` — necesitará `IMind`)
- [ ] Evaluar impacto en `CompanionBase.ApplyProximityEffect()` (actualmente llama `playerStats.Restore`)
- [ ] Evaluar si `BondActivity` registra trauma leyendo `IMind.stress` en lugar de `PlayerStats.stress`

---

## Sistema de Bloques — Propuestas pendientes de evaluación

Las ideas siguientes **no están implementadas**. Se documentan para la próxima tarea.

### A. Bloques con propiedades físicas por elemento de la tabla
Cada elemento químico confiere al bloque materializado una propiedad distinta:

| Familia | Comportamiento del bloque |
|---|---|
| Gases nobles (He, Ne, Ar…) | Livianos, flotan hacia arriba lentamente — ideales para alfombra |
| Metales densos (Pb, Au, Fe) | Pesados, máxima estabilidad, resisten impactos |
| Carbono (C) | Configurable: grafito = flexible/apilable; diamante = irrompible |
| Elementos reactivos (Na, K) | Inestables — se disuelven antes si reciben un impacto o agua |

Implicación: añadir campo `BlockProperties` a `PaletteElementData` con masa, durabilidad, etc.
El prefab del bloque lee estas propiedades al materializar.

### B. El jugador elige el material uniforme para todos sus bloques
En lugar de que cada bloque sea del elemento que representa, el jugador puede
seleccionar un elemento y aplicarlo a toda su paleta. Todos los bloques adquieren
las propiedades de ese elemento hasta que se cambie la selección.

Implicación: un `GlobalBlockMaterial` en la paleta que sobreescribe `BlockProperties`
de todos los elementos activos.

### C. Expansión de bloque — un solo bloque cubre toda una superficie
Un bloque puede expandirse para cubrir un área mayor (p.ej., toda una pared como barrera).
El jugador activa la expansión y el bloque escala hasta dimensiones útiles.

Implicación: `PaletteElement.Expand(Vector3 targetScale)` con animación SmoothStep,
límite de expansión configurable por elemento.

### D. Plataforma móvil (alfombra mágica)
El jugador se coloca sobre bloques dispuestos bajo sus pies (`PlatformPattern`) y los
mueve como unidad. Implica un coroutine continua que desplaza los bloques y arrastra
al jugador con ellos.

**Sobre poner al jugador dentro del GameObject:**
Poner al jugador como hijo del bloque (parenting) es eficiente — Unity lo resuelve con
una simple actualización de jerarquía de Transform. El jugador no nota diferencia si la
animación es suave. **Caveat importante**: si el `PlayerCtrl` usa `CharacterController`
(el setup más común para primera persona), el parenting NO arrastra al personaje
automáticamente porque `CharacterController` opera siempre en espacio de mundo.
La solución estándar: cada frame, calcular el `deltaPosition` del bloque y aplicarlo
manualmente al `CharacterController.Move()` del jugador.
Si `PlayerCtrl` usa `Rigidbody`, el parenting funciona directamente pero puede provocar
artefactos de física en colisiones. Evaluar al implementar `PlayerCtrl`.

Implicación: nuevo modo `DynamicPlatform` en `ArrangementPattern` + lógica en
`MaterializationExecutor` para la coroutine de movimiento continuo.

---

## Propuestas de balance — pendientes de evaluación

### Multiplicador de fórmula (Palette, modo Formula / Hybrid)

El `formulaMultiplier` en `PaletteConfig` está en `1.5f`. El objetivo es que usar la fórmula
en lugar del botón directo se **sienta distinto y valga la pena** — no que sea levemente mejor.

**Propuesta:** subir el rango a **2.5×–4×** dependiendo de la complejidad de la fórmula:

| Tipo de acción | Multiplicador sugerido |
|---|---|
| Asana (2–3 posiciones) | ×2.5 |
| Encantamiento simple (2–3 elementos) | ×3 |
| Encantamiento avanzado (5+ elementos) | ×4 |
| Cuidado de cría (Direct, sin fórmula) | ×1 (sin bonus — es directo por diseño) |

Criterio de calibración: el jugador que practica la fórmula debe notar que llega a umbrales de efecto
que el modo directo no alcanza aunque lo use varias veces seguidas. La diferencia debe ser visible sin
necesitar explicación.

**Pendiente:** calibrar con feedback de jugabilidad real. No cambiar hasta tener sesiones de prueba.

---

### Apilamiento de asanas (stack de fórmula durante postura activa)

Mientras una asana está activa (el beneficio se está entregando — segundos a minutos),
el jugador puede **volver a introducir la misma fórmula**. Cada completación adicional
multiplica el beneficio total acumulado:

- Completar la fórmula 1 vez → beneficio normal × `formulaMultiplier`
- Completar 2 veces → beneficio × `formulaMultiplier²`
- Completar N veces → beneficio × `formulaMultiplier^N`

**Propósito:** recompensar la práctica sostenida y la atención. El jugador que sigue presente
y sigue repitiendo la fórmula en lugar de dejarse llevar pasivamente saca más.

**Implicación de código:**
`AsanaQueue.benefitRate` podría escalar por un `stackMultiplier` que sube con cada
completación de fórmula mientras la asana está activa.

**Precaución:** definir un tope de stack (p.ej. ×4 máximo) para evitar explotación.

**Pendiente:** definir si el stack se mantiene entre lados (posturas bilaterales) o se reinicia.

---

## Análisis de sistemas incorporados

Sistemas añadidos con el último pull. Se analizan en relación al código ya existente.

### Resumen de qué llegó

| Archivo | Qué hace |
|---|---|
| `PlayerStats.cs` | Stats del jugador: satisfaction, mentalFatigue, stress, sleepiness, observationRadius, velocity, physicalResistance. API: `Restore()` / `Drain()` via `StatChannel` |
| `IBondable.cs` | Interfaz genérica para todo lo que puede formar vínculo con el jugador: compañeros, animales, objetos, lugares |
| `CompanionBase.cs` | Base para compañeros: estado interno (fatigue/stress/mood), proximidad → restaura o drena stats del jugador, ThoughtAnchors |
| `ThoughtAnchor.cs` | Creencia que sesga el comportamiento de un compañero; se suaviza con el arco narrativo |
| `Gohageneis.cs` | Canal: Satisfaction. CelebrationCharge → burst de restauración al alcanzar umbral |
| `Goluis.cs` | Canal: MentalFatigue. Presión → sube estrés del jugador. `yoga_skepticism` bloquea asanas hasta que avanza el arco |
| `Panterilia.cs` | Canal: MentalFatigue. Añade bonus a `observationRadius` del jugador al estar cerca |
| `BondActivity.cs` | ScriptableObject. Práctica que construye bond con cualquier IBondable. Sistema de trauma/bloqueo/desbloqueo por satisfacción |
| `BondActivityManager.cs` | Attached al Player. Registra IBondables por ID, activa prácticas activas/pasivas, tick de trauma diario |
| `WorldBondable.cs` | Attach a cualquier objeto/lugar del mundo para que forme vínculo con el jugador |
| `CameraManager.cs` | Singleton. 1ª/3ª persona, robos de cámara, efectos visuales por PlayerStats |
| `CameraZoneTrigger.cs` | Trigger de zona → CameraManager.OnEnterCubArea / RequestRobbery |
| `Asana.cs` | ScriptableObject. Posiciones requeridas, StatType, maestría, shortcut unlock a 10 prácticas |
| `AsanaDetector.cs` | Attached al Player. Escucha pulsaciones de BodyPosition, detecta match de asana |
| `AsanaQueue.cs` | Gestiona la asana activa y cola. Entrega beneficio continuo a sus propios contenedores |
| `BodyPosition.cs` / `BodyPositionButton.cs` | Datos de posición corporal + botón UI que notifica al detector |

---

### Gaps y conflictos a resolver

#### 1. StatType (Asana) vs StatChannel (PlayerStats) — desconectados

`AsanaQueue` entrega beneficios a su propio `Dictionary<StatType, float> _containers`
(Strength/Flexibility/Balance/Endurance). `PlayerStats` tiene stats completamente distintas
(Satisfaction/MentalFatigue/Stress/Sleepiness via `StatChannel`).

**El beneficio de las asanas no llega a PlayerStats actualmente.**

Propuesta de mapeo (a evaluar según diseño):

| StatType (asana) | StatChannel (player) |
|---|---|
| StrengthAndGrounding | MentalFatigue (reduce) |
| Flexibility | Stress (reduce) |
| Balance | Sleepiness (reduce) |
| Endurance | (no mapeo directo — puede afectar physicalResistance) |

Implementación mínima: en `AsanaQueue.DeliverBenefit()`, además de llenar el contenedor propio,
llamar a `playerStats.Restore(gain, mappedChannel)`. Requiere referencia a `PlayerStats` en `AsanaQueue`.

#### 2. `Palette.GetPlayerBond()` devuelve 0

Hay un `TODO` explícito. Ahora existe `BondActivityManager` con el registro de IBondables.
La paleta necesita conocer el bond del contexto activo (p.ej., la cría que se está cuidando).
Solución: pasar el IBondable relevante al abrir la paleta (`Open(config, ctx, IBondable target)`)
o leer el bond directamente del `user` si implementa IBondable.

#### 3. `Goluis.resistanceBuilt` no está conectado a `PlayerStats.physicalResistance`

`resistanceBuilt` (0–1) se acumula en Goluis pero `PlayerStats` no lo lee.
Propuesta: en `CompanionBase.OnPlayerLeft()` o en un tick periódico,
`playerStats.physicalResistance = 1f + goluis.resistanceBuilt`.

#### 4. `CameraManager.RequestRobbery` no está conectado a los hitos de las crías

`setup-camera-system.md` lo lista como pendiente explícito.
`Animal.firstNestExit` y `Animal.firstSolidEaten` ya existen (añadidos en la Fase 6).
Cuando esos booleans cambien a `true`, deberían disparar un robo de tipo `OrbitTarget`
con la cría como target, para que la cámara celebre el momento.

Propuesta mínima: en `PostNatalManager.Update()`, cuando se detecte el cambio →
`CameraManager.Instance?.RequestRobbery(new CameraRobbery { type = RobberyType.OrbitTarget, orbitTarget = cub.transform, holdDuration = 3f })`.

#### 5. `ScreenEffects.cs` — placeholder

`BlackoutPulse` en `CameraManager` es solo un `Debug.Log`.
Requiere un componente separado con overlay de pantalla completa (URP o canvas de UI).
No bloqueante ahora, pero necesario antes de testing de jugabilidad.

---

### Alineaciones limpias (encajan sin cambios)

- **`Asana.ShortcutUnlocked` ↔ `PaletteElementData.isDirectUnlocked`**: cuando el jugador
  practica una asana 10 veces, `ShortcutUnlocked` es `true` → marcar `isDirectUnlocked = true`
  en el `PaletteElementData` correspondiente → el modo Hybrid la activa directamente.
  Un sistema de progresión puede sincronizar ambos automáticamente.

- **`Goluis.yoga_skepticism` → `Asana.isUnlocked`**: el anchor `yoga_skepticism` es el gate narrativo
  para las asanas. Mientras sea fuertemente negativo (antes de que el arco avance), las asanas no
  se desbloquean. El sistema ya tiene `isUnlocked` en `Asana` — solo necesita que algo lo fije.
  Propuesta: un `GoluisArcWatcher` que observe el anchor y, al cruzar el umbral (`> -0.4`),
  llame al desbloqueador de asanas.

- **BondActivity + Palette cuidado de crías**: la paleta de cuidado en modo Direct es exactamente
  la UI de `BondActivities` para animales. Al pulsar "Presencia tranquila" en la paleta →
  llamar `BondActivityManager.Practice("Presencia tranquila")`. La actividad ya sabe a qué IBondable
  apuntar (por `targetId`). El vínculo se construye sin lógica extra.

- **`CameraZoneTrigger` + `BondActivityManager.SetContextTag`**: al entrar al área de crías,
  el trigger ya llama a `CameraManager.OnEnterCubArea()`. Añadir en el mismo trigger →
  `bondActivityManager.SetContextTag("cub_area")` → activa pasivamente actividades con ese trigger.
  Un solo evento de zona alimenta dos sistemas a la vez.

- **`PlayerStats.observationRadius` + `Panterilia`**: ya funciona. Panterilia suma el bonus al
  entrar y lo quita al salir. A futuro, las BondActivities pasivas que requieran un radio mínimo
  pueden consultarlo directamente desde `PlayerStats`.

---

### Nuevas propuestas emergentes de este análisis

#### Pipeline de zona completo
Un trigger de zona puede activar simultáneamente:
1. `CameraManager` → switch de perspectiva / robo
2. `BondActivityManager` → context tags → prácticas pasivas
3. Otros sistemas futuros (audio, iluminación)

Un componente `ZoneActivator` que agregue estas llamadas sería más limpio que
tener todo en `CameraZoneTrigger`. Propuesta de bajo impacto, alta utilidad.

#### Satisfacción como desbloqueador de la paleta artística de cámara
`docs/camera-system.md` propone paletas visuales desbloqueables por Satisfacción
(Sueño, Estrés, Alegría, Normal). Estas paletas son exactamente elementos de una
`Palette` en modo `Direct` con `PaletteElementData` por cada modo visual.
`PlayerStats.satisfactionCapacity` puede funcionar como umbral de desbloqueo:
a mayor capacidad (que crece con Gohageneis), más paletas artísticas disponibles.

#### `WorldBondable` como IBondable para la esterilla
La esterilla de yoga es el ejemplo más natural. Añadir `WorldBondable` al prefab de la esterilla,
asignar `bondableId = "yoga_mat"`. Una `BondActivity "Presencia con la esterilla"` con
`passiveTrigger: "mat_idle"` se activa sola cuando el jugador está sobre ella sin hacer nada.
El bond con la esterilla crece — coherente con el alma contemplativa del juego.

---

### Notas de rendimiento

| Punto | Evaluación |
|---|---|
| `CompanionBase.Update()` — `Vector3.Distance` cada frame | Bajo costo. Aceptable con N < 10 compañeros activos |
| `WorldBondable.Update()` — distancia cada frame | Si hay muchos objetos WorldBondable (> 20), migrar a OverlapSphere con capas o check cada 0.1s |
| `BondActivityManager.TickPassiveActivities()` | Ya tiene guard `if (_activeContextTags.Count == 0) return` — bien |
| `AsanaDetector.CheckForMatch()` | Solo se llama al presionar posición, N de asanas pequeño — negligible |
| `CameraManager.ApplyStatEffects()` — `Camera.main.fieldOfView` cada frame | Debería cachear `Camera.main` en `Awake`. Campo menor pero buena práctica |

---

## Ideas / Backlog sin prioridad

- Definir lista completa de posiciones corporales (los "botones") que componen cada asana y ejercicio
- Decidir distribución visual de la tabla periódica en pantalla
- Explorar si la mecánica de asanas tiene feedback visual/sonoro mientras se ejecuta
- Evaluar rendimiento del cálculo de espacio libre para posturas (ver decisión de diseño abajo)
- Definir lista estándar de `passiveTriggers` (tags de contexto) y qué sistemas los emiten
- Decidir si el trauma de BondActivity se muestra explícitamente al jugador o solo como "actividad bloqueada"
- Conectar arco de Goluis con desbloqueo de asanas (`GoluisArcWatcher` o lógica en `LevelScript`)
- Implementar `ScreenEffects.cs` para efectos visuales reales de cámara (requiere URP)
