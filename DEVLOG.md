# Cold Sanctuary — Dev Log & Contexto de Diseño

## Estado actual

### UI / Interfaces
- [x] **Radar de animales** (`AnimalRadar.cs`, `FollowingArrays.cs`)
  - Botones flotantes frente al jugador, uno por animal cercano
  - Se activa con tecla Z, se cierra con Escape
  - Diseñado para Desktop y Mobile: cada ítem es clickeable Y tiene tecla asociada
  - Las teclas se eligen para alinear al usuario al uso del teclado de escritura (sin flechas)

---

## Narrativa — Nivel 1

**Setup:** Un grupo de amigos llega a un santuario animal con una agenda oculta: aprender la magia de la maestra.

**Secuencia de introducción — Nivel 1:**
1. La maestra recibe al grupo y los felicita por haber encontrado el lugar
2. Les dice que ve potencial en los integrantes
3. Les propone: para conocer los secretos del lugar, deben participar como voluntarios
4. Lleva al equipo a la cabaña
5. Les muestra la esterilla
6. Pide al jugador que se acerque a la esterilla
7. Se revela el **status físico** del jugador (stats iniciales, contenedores vacíos)
8. El jugador hace sus **primeras flexiones, abdominales y piernas** — tutorial del sistema de posiciones corporales
9. La maestra comenta el **nivel actual del jugador y cuánto le falta para avanzar**
10. La maestra lleva al equipo al **área de crías** — primera zona de tareas
11. Primeras tareas: **alimentar, cuidar y jugar** con las crías
12. Objetivo: que las crías se conviertan en **adultos amigables** (con personas y entre sí)
13. Cierre: la **maestra aparece** y le enseña su **primera postura de yoga** — trigger: umbral stats + vínculo crías

---

## Misiones Secundarias — Compañeros (Nivel 1 en adelante)

### Principio de diseño

Los compañeros **crecen delante del jugador**: si el jugador los vuelve a ver en el Nivel 2, ellos habrán mejorado también. Siempre están un paso adelante.

### Especialización por compañía

- Tiempo con compañeros de **velocidad/elegancia** → el jugador gana más velocidad
- Tiempo con compañeros de **fuerza** → el jugador gana más fuerza
- Repartir el tiempo entre todos → progresión más balanceada
- Entrenar en la esterilla → subida de stats según los ejercicios elegidos

### Stat: Observación

Radio de consciencia alrededor del jugador. Objetos observables emiten señales sutiles y diegéticas. Al acercarse, aparece un pensamiento interno breve.

**Modo contemplativo:** mirar sin buscar nada — sube la stat por sí solo. Mirar como práctica.

**Usos en Nivel 1:** notar ingredientes, comportamientos de animales, detalles del entorno
**Usos posteriores:** puente hacia la meditación y hacia las recetas mágicas de la tabla periódica

---

### Compañera: Panterilia

**Trasfondo:** Mujer muy trabajadora que huyó de su país buscando poder llevarse a su hija consigo. No dejó los estudios para abrir oportunidades y finalmente lo logró, pero pasaron años. Relación con su hija distante: se piden permiso para darse abrazos. Problemas con el padre (menor de edad) que no coopera con trámites y la acusa de buscarlo con excusas.

**Stats y nivel de llegada:** Llega poco después del jugador — no viene con stats elevadas.

**Arco en Nivel 1:** De químicos industriales a ingredientes naturales. Al final decide entrar en nutrición animal.

**Ganancias del jugador al acompañarla (Nivel 1):**
- **Observación** — Panterilia enseña literalmente a "ver"
- **Puntos de consciencia** — semilla para la meditación posterior
- **Conocimiento de nutrientes** — base para recetas mágicas de la tabla periódica

**Arco en Nivel 2:** Mejora relación con su hija. Asciende a formadora. Sigue incrementando Observación y consciencia.

---

### El Maestro Goluis — Cocina

Llegó antes que el jugador. Al principio pone presión: comenta sobre prisa y organización.

**Mecánica de presión:**
- Sus comentarios otorgan bono de velocidad → el jugador tiene más tiempo para otra misión
- Pero agotan física y mentalmente
- Si el agotamiento mental supera cierto umbral, el jugador tambalea — afecta el tiempo final de la misión

**Trasfondo:** Antes era bandido. Tiene contactos. Hace dos trabajos. Vive con mamá y dos hermanos que le ayudan con su hija. Ha sufrido problemas de pie, extracción de un testículo, media cara paralizada, y varios accidentes.

**Ganancias del jugador:**
- **Velocidad** — sus comentarios de presión la activan, con riesgo de fatiga
- **Resistencia** — aguante físico y mental. Base para posturas de yoga más exigentes.

**Arco en Nivel 1 y 2:** Escéptico del yoga — al final del Nivel 2 gana humildad, cariño y paciencia suficiente para comenzar a entrenar.

---

## Narrativa — Nivel 2

**Condición de activación:** Vínculo suficiente con las crías (combinación de stats + vínculo).

**Mecánica central — Ciclos de las crías:**
- Las crías alternan entre fases de necesidad y fases de descanso/ciclo autónomo
- Durante las fases autónomas aparece una **cuenta regresiva** visible en pantalla
- El jugador queda **libre para moverse por el mundo** hasta que se agote el tiempo
- Al finalizar el tiempo, el jugador deja lo que esté haciendo y **corre de vuelta al puesto**
- La urgencia crea ritmo y tensión natural sin interrumpir el flujo de exploración

**Actividades durante el tiempo libre:**
- Hablar con **NPCs del mundo** que ofrecen misiones secundarias
- Volver a la **esterilla y entrenar** (ejercicios básicos, asanas desbloqueadas)
- Exploración libre del mapa

**Progresión del nivel:** El sistema de asanas ya está desbloqueado (cierre Nivel 1). El jugador lo integra ahora con libertad.

---

## Comentarios y decisiones de diseño

- **Teclas del teclado**: se alinean a posición de escritura para que el jugador pueda usarlas naturalmente.
- **Encantamientos como tabla periódica**: los elementos químicos como metáfora de poderes/encantamientos crea coherencia temática.
- **Asanas como mecánica de recuperación/fuerza**: requiere atención activa pero no interrumpe el flujo (se puede encolar).
- **Límite de almacenamiento por stat**: cap dinámico — empieza bajo y crece orgánicamente.
- **Disponibilidad de la interfaz de posturas**:
  - Opción A (ideal): calcular espacio libre en tiempo real. Más flexible, puede impactar rendimiento.
  - **Opción B (world design)**: interfaz solo disponible sobre esterillas repartidas por el mundo. Preferida si Opción A afecta rendimiento.
