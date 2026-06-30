# Fauna — Diseño de Gameplay

Documento de visión y diseño del **área de fauna** desde la perspectiva del jugador.
El "cómo" técnico de la simulación está en [`behavior-system.md`](behavior-system.md).
Este archivo responde: ¿qué hace el jugador con los animales, para qué existe este sistema,
y cómo se conecta con la progresión del juego?

---

## Propósito del sistema de fauna en el juego

El santuario trabaja para que los animales sean amigables entre sí y con las personas.
El jugador llega como voluntario y su misión central en el **Nivel 1** es cuidar crías
hasta que alcancen un vínculo suficiente con él — lo que abre el Nivel 2 y desbloquea
las asanas.

Los animales no son decorado: son los personajes principales del Nivel 1.
Cada cría tiene hambre, temperatura, estrés y un bond real con el jugador que sube
por actos de cuidado concretos. El `PostNatalManager` ya simula todo esto —
lo que falta es darle al jugador las herramientas para interactuar.

---

## Resultado deseado

Al final del Nivel 1, las crías a cargo del jugador deben haber:
- Alcanzado bond ≥ umbral con el jugador (ver §Progresión de nivel)
- Pasado por sus etapas post-natales naturales sin morir
- Empezado a mostrar comportamientos autónomos (exploración, juego)

El jugador debe haber aprendido a leer el estado del animal y a responder
apropiadamente — sin que nadie le haya dado un manual explícito.

---

## Mecánicas de cuidado — qué puede hacer el jugador

No es solo dar comida. El cuidado que se da en el santuario supera lo que daría
la madre en la naturaleza: incluye enriquecimiento, socialización y aprendizaje mutuo.
Cada actividad sube bond y mejora alguna variable de estado de la cría.

| Actividad | Mecánica de input | Efecto en la cría |
|---|---|---|
| **Alimentación con biberón / jeringa** | Acercar item, animación cronometrada | Baja `hungry`, sube bond |
| **Grooming (cepillar, desparasitar)** | Arrastre suave sobre la cría | Baja `stress`, sube bond, sube salud |
| **Enriquecimiento olfativo** | Acercar objetos (hierbas, tierra, tela) al morro | Sube curiosidad/energía |
| **Juego de caza controlado** | Mover objeto (pluma, cuerda, ramita) | Sube energía, desarrolla habilidades — riesgo de agresividad si sobreestimulado |
| **Puzzle de comida** | Esconder alimento en objeto de enriquecimiento | Estimula inteligencia, sube bond |
| **Presencia tranquila** | Sentarse cerca sin interactuar (timer pasivo) | Baja `stress` lentamente, sube bond — clave para animales tímidos |
| **Exploración de texturas** | Ofrecer elementos de distintas texturas | Estimula sentidos, sube curiosidad |
| **Socialización entre crías** | Acercar dos crías gradualmente | Sube bond entre ellas; si `stress` es alto puede crear conflicto |
| **Respuesta vocal** | Reproducir sonido de la especie en respuesta al llanto | Baja `stress` inmediato |
| **Depositar comida (ICarrier.Drop)** | Tecla Q del jugador | `droppedBy = playerTarget` → bond crece al comer |

> Referencia de código: `Animal.stress`, `Animal.hungry`, `GrowBond()`, `ICarrier`, `FoodItem.droppedBy`.

### Actividades que NO se hacen (seguridad)

Estas reglas las enseñan los compañeros o aparecen como tips:
- No mirar fijamente a los ojos
- No levantar del suelo
- No correr cerca
- No alimentar de la mano hasta bond alto
- Por más pacífico que parezca, puede atacar si no tiene su espacio
- El juego de caza puede volverlos agresivos si se sobreestimula — parar cuando la cría pase a `Fight`

---

## Cómo se guía al jugador

### Opción A — Compañeros contextuales (ideal, implementar después)

Un compañero cercano observa el estado de la cría y hace un comentario en el momento:
- `cub.stress > 0.5` → *"mira cómo tiene las orejas — está asustado, siéntate despacio"*
- `cub.hungry > VocalizationThreshold` → *"ese sonido significa que tiene hambre, pon la jeringa aquí"*
- `cub.firstNestExit == true` → *"¡salió solo! eso es muy buena señal"*
- Al acercarse demasiado rápido → *"ve más despacio, que te va a ver como una amenaza"*

Requiere: sistema de diálogo con triggers en variables de estado del animal.
Los compañeros también dan las reglas de seguridad de forma orgánica, en lugar de
un tutorial de texto.

### Opción B — Tarjeta de estado sobre la cría (fallback, implementar primero)

Icono flotante o tarjeta al seleccionar la cría: imagen expresiva del animal
+ estado en texto simple (Hambre / Estrés / Bond / Temperatura).
Más fácil de implementar y siempre presente como referencia.

**Recomendación:** implementar B primero usando el sistema `FollowingArrays` existente.
Agregar A por encima cuando exista sistema de diálogo.

---

## Progresión de nivel — conexión con el LevelScript

### Propuesta de arquitectura

Un `LevelScript` (MonoBehaviour o ScriptableObject) por nivel centraliza:
- **Requisitos de bond**: N crías con `Bond.value ≥ umbral`
- **Requisitos de stats del jugador**: fuerza, enraizamiento, observación ≥ umbral
- **Desbloqueos**: qué asanas, zonas o NPCs se habilitan al cumplirse

El `LevelManager` lee el `LevelScript` activo y dispara los eventos cuando se cumplen
todas las condiciones. Los compañeros y misiones secundarias aportan stats pero no
son requisito estricto — son el camino natural para llegar al umbral.

### Nivel 1 — ejemplo de requisitos (valores por calibrar)

```
Bond fauna:    5 crías con bond ≥ 100
Stats jugador: cualquier stat ≥ 30 (haber entrenado un mínimo)
```

Al cumplirse → la maestra aparece, enseña primera asana, se desbloquea Nivel 2.

### Vínculo con el PostNatalManager

El `PostNatalManager` genera las ventanas autónomas de las crías que crean el
tiempo libre del Nivel 2. El `LevelScript` solo necesita detectar los bonds
cumplidos y disparar el evento. No hay que añadir lógica nueva a los animales.

---

## Casos reales de referencia

Actividades documentadas en santuarios y criaderos de fauna silvestre:
- **Big cat sanctuaries**: grooming con cepillo, enrichment olfativo (especias, hierbas),
  juego con objetos colgantes, puzzle feeders para estimular caza.
- **Wolf/canid rescues**: socialización temprana con varias personas y sonidos,
  jerarquía establecida por presencia tranquila, never direct eye contact.
- **Seal/sea lion rehab**: alimentación con jeringa/biberón, introducción gradual al agua,
  respuesta a vocalizaciones para reducir stress.
- **Deer fawns**: "ocultamiento" respetado — no moverlas del spot aunque parezcan solas,
  mínimo contacto humano para no imprinting si van a ser liberadas.

> Para las crías del santuario el imprinting es *deseable* (son para convivencia,
> no para reintroducción), así que se pueden humanizar más que en rehabilitación estándar.

---

## Pendientes de diseño

- [ ] Definir qué actividades están disponibles para cada especie y en qué etapa post-natal
      (no todas las actividades aplican a todas las crías — un cachorro de foca en Stage 1
      no puede hacer juego de caza todavía)
- [ ] Definir los umbrales concretos de bond y stats para el LevelScript del Nivel 1
- [ ] Diseñar la tarjeta de estado de la cría (Opción B) en el sistema FollowingArrays
- [ ] Definir qué compañero guía qué comportamiento animal (ver personajes en DEVLOG.md)
- [ ] Decidir si la alimentación con biberón es un item separado de FoodItem o una
      variante con `feedingMethod = FeedingMethod.Nurse`
