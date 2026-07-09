# Fauna — Diseño de Gameplay

Documento de visión y diseño del **área de fauna** desde la perspectiva del jugador.
El "cómo" técnico de la simulación está en [`behavior-system.md`](behavior-system.md).
Este archivo responde: ¿qué hace el jugador con los animales, para qué existe este sistema,
y cómo se conecta con la progresión del juego?

---

## Contexto real — Santuario vs. cautiverio

### "Criado en cautiverio" no es lo mismo que santuario

"Criado en cautiverio" describe solo el origen del animal — nació bajo cuidado humano.
Puede ocurrir en zoos, criaderos comerciales, laboratorios o santuarios. No dice nada
sobre el bienestar.

Un **santuario ético** (estándar GFAS — Global Federation of Animal Sanctuaries) define que:
- **No se crían** animales como estrategia — los animales llegan rescatados o confiscados
- **No se usan** para entretenimiento, espectáculos ni contacto pagado
- **No se venden ni trasladan** salvo por bienestar del animal
- El objetivo es **calidad de vida permanente**, no reproducción ni exhibición

El santuario de Cold Sanctuary cae en esta categoría: los animales son residentes
permanentes cuyo objetivo es la convivencia con humanos, no la reintroducción al medio
silvestre. Esto lo distingue de un programa de rehabilitación.

### Rehabilitación vs. residencia permanente — distinción clave para el diseño

| | Rehabilitación | Santuario (este juego) |
|---|---|---|
| **Objetivo** | Devolver al medio silvestre | Convivencia permanente con humanos |
| **Imprinting** | A evitar activamente — puede significar la muerte del animal al ser liberado | Deseable — es la mecánica central del bond |
| **Contacto humano** | Mínimo; algunos usan disfraces de la especie para dar biberón | Progresivo y enriquecido |
| **Éxito** | Animal liberado y autosuficiente | Animal con bond alto, comportamiento natural preservado |

> El famoso programa del cóndor de California usó marionetas de adulto para dar biberón
> a los polluelos — precisamente para evitar que identificaran a los humanos como familia.
> En Cold Sanctuary ocurre exactamente lo contrario: ese imprinting es el objetivo.

---

## Propósito del sistema de fauna en el juego

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

## Principio de diseño — el bond se gana, no se da

En un santuario real, los animales que viven con humanos permanentemente desarrollan
confianza de forma gradual y condicionada a que el humano respete sus señales.
Un animal con `stress` alto rechaza el contacto aunque haya bond previo.

Esto genera la mecánica correcta: **el jugador no puede forzar el bond**. Debe:
1. Leer el estado de la cría antes de interactuar
2. Elegir la actividad apropiada para ese estado
3. Respetar cuando la cría se retira o señaliza incomodidad

Un bond de 100 no significa que el animal sea un peluche — significa que confía
en ese humano específico. Sigue siendo un animal salvaje.

---

## Mecánicas de cuidado — qué puede hacer el jugador

Las actividades evolucionan con el bond: un animal recién llegado no tolera el grooming;
uno con bond medio acepta entrenamiento básico; uno con bond alto coopera en cualquier
procedimiento. Desbloquear actividades es parte de la progresión.

| Actividad | Mecánica de input | Efecto en la cría | Bond mínimo |
|---|---|---|---|
| **Presencia tranquila** | Sentarse cerca sin interactuar (timer pasivo) | Baja `stress` lentamente, sube bond — clave para animales tímidos | 0 |
| **Respuesta vocal** | Reproducir sonido de la especie en respuesta al llanto | Baja `stress` inmediato | 0 |
| **Depositar comida (ICarrier.Drop)** | Tecla Q del jugador | `droppedBy = playerTarget` → bond crece al comer | 0 |
| **Alimentación con biberón / jeringa** | Acercar item, animación cronometrada | Baja `hungry`, sube bond | 10 |
| **Enriquecimiento olfativo** | Acercar objetos (hierbas, tierra, tela, especias) al morro | Sube curiosidad/energía, revela preferencias de la especie | 20 |
| **Exploración de texturas** | Ofrecer elementos de distintas texturas y temperaturas | Estimula sentidos | 20 |
| **Puzzle de comida** | Esconder alimento en objeto de enriquecimiento | Estimula inteligencia, sube bond | 30 |
| **Grooming (cepillar, desparasitar)** | Arrastre suave sobre la cría | Baja `stress`, sube bond, sube salud | 40 |
| **Juego de caza controlado** | Mover objeto (pluma, cuerda, ramita) | Sube energía, desarrolla habilidades — riesgo de agresividad si sobreestimulado | 50 |
| **Socialización entre crías** | Acercar dos crías gradualmente | Sube bond entre ellas; si `stress` es alto puede crear conflicto | 50 (entre ellas) |
| **Entrenamiento de cooperación** | Señal + recompensa para que el animal realice un comportamiento | Sube bond, desbloquea procedimientos veterinarios en etapas avanzadas | 70 |

> Referencia de código: `Animal.stress`, `Animal.hungry`, `GrowBond()`, `ICarrier`, `FoodItem.droppedBy`.

### Estereotipias — consecuencia del descuido

Si el jugador descuida el enriquecimiento durante demasiado tiempo, la cría desarrolla
**estereotipias**: comportamientos compulsivos repetitivos (balanceo, marcha circular,
autolesión leve). Visibles en la animación. Se reducen al retomar el cuidado pero
dejan una marca en el estado base de `stress`.

Son el equivalente animal de la ansiedad crónica — y ocurren en la vida real en
animales en cautiverio con poco enriquecimiento.

### Actividades que NO se hacen (reglas de seguridad)

Las enseñan los compañeros de forma orgánica, no como pantalla de tutorial:
- No mirar fijamente a los ojos
- No levantar del suelo
- No correr cerca
- No alimentar de la mano hasta bond suficiente
- Por más pacífico que parezca, puede atacar si no tiene su espacio
- El juego de caza puede volverlos agresivos si se sobreestimula — parar cuando la cría señalice incomodidad o entre en `Fight`

---

## Cómo se guía al jugador

### Opción A — Compañeros contextuales (ideal, implementar después)

Un compañero cercano observa el estado de la cría y hace un comentario en el momento:
- `cub.stress > 0.5` → *"mira cómo tiene las orejas — está asustado, siéntate despacio"*
- `cub.hungry > VocalizationThreshold` → *"ese sonido significa que tiene hambre, pon la jeringa aquí"*
- `cub.firstNestExit == true` → *"¡salió solo! eso es muy buena señal"*
- Al acercarse demasiado rápido → *"ve más despacio, que te va a ver como una amenaza"*
- Al sobreestimular en juego → *"para, que si le excitas demasiado luego no hay quien le calme"*

Los compañeros también dan las reglas de seguridad de forma orgánica.
Requiere: sistema de diálogo con triggers en variables de estado del animal.

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

## Referencia real por especie

| Especie | Práctica real relevante |
|---|---|
| **Lobo / cánido** | Socialización temprana con múltiples personas y sonidos; jerarquía por presencia tranquila, nunca confrontación directa; los cachorros en santuario suelen vivir en manadas artificiales con adultos ya vinculados |
| **Oso** | Los oseznos rescatados se crían en grupos de hermanos cuando es posible; enrichment muy importante — son los más propensos a estereotipias en cautiverio; entrenamiento de cooperación veterinaria es estándar en santuarios de osos |
| **Conejo / lagomorfo** | Los gazapos son altriciales (nacen ciegos e inválidos); biberón cada 12h con leche específica; mínimo contacto las primeras semanas incluso en santuario de convivencia — son muy frágiles al estrés |
| **Venado / cérvido** | Los cervatillos encontrados solos raramente están abandonados — la madre los deja escondidos; en santuario se usan "madres sustitutas" (peluche grande con olor de la especie) para no ser el único referente; muy sensibles al estrés por captura |
| **Foca** | Alimentación con jeringa de pescado triturado; introducción gradual al agua es una actividad de enriquecimiento clave; responden muy bien a la vocalización humana que imita sus llamadas |
| **Husky / can nórdico** | Especie completa en código (`HuskyBehavior`), sin ficha de diseño aún — completar referencia real y mecánicas |
| **Zorro** | Especie completa en código (`FoxBehavior`), sin ficha de diseño aún — completar referencia real y mecánicas |

> **Estado real (auditoría 2026-07-09):** `HuskyBehavior` y `FoxBehavior` existen y están
> completas en código pero faltaban en esta tabla. Además, el mecanismo de dardo
> tranquilizante/disparo (`Shooted()`, `PlayerCtrl`) fue **eliminado** el 2026-07-09 — el
> santuario es no-violento, ya no hay mecánica de disparo hacia los animales.

---

## Pendientes de diseño

- [ ] Definir qué actividades están disponibles para cada especie y en qué etapa post-natal
      (un gazapo en Stage 1 no puede hacer juego de caza; una cría de foca necesita agua antes)
- [ ] Definir los umbrales concretos de bond y stats para el LevelScript del Nivel 1
- [ ] Diseñar la tarjeta de estado de la cría (Opción B) en el sistema FollowingArrays
- [ ] Definir qué compañero guía qué comportamiento animal (ver personajes en DEVLOG.md)
- [ ] Decidir si la alimentación con biberón es un item separado de FoodItem o una
      variante con `feedingMethod = FeedingMethod.Nurse`
- [ ] Definir qué activa y qué reduce las estereotipias (umbral de tiempo sin enriquecimiento,
      velocidad de recuperación con cuidado)
- [ ] Diseñar el entrenamiento de cooperación veterinaria como mecánica de alto bond
