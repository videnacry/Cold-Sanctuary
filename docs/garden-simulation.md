# El Huerto — segunda simulación (Neolítico / agricultura)

Diseño (2026-07-29). Segunda área/simulación para principiantes, **después de la Cocina**
([`kitchen-simulation.md`](kitchen-simulation.md)). Continúa la historia del microworld: la era del
**fuego** (cocina) da paso al **Neolítico / revolución agrícola**. Base:
[`mob-epochs-matrix.md`](mob-epochs-matrix.md) (hilos/épocas/regiones),
[`mob-quests-early.md`](mob-quests-early.md) §3 (La Sembradora),
[`mob-characters.md`](mob-characters.md), [`anima-architecture.md`](anima-architecture.md) (mente/pensamientos).

## 1. Territorio y época
- **Época:** Neolítico (revolución agrícola), el escalón siguiente al Paleolítico del fuego.
- **Territorio/región:** la **Creciente Fértil** — una **primera aldea neolítica sedentaria** (tipo
  Çatalhöyük / Jericó): chozas de adobe, campos alrededor, un río/manantial. *Precede* a la Mesopotamia
  urbana de la era de los Metales (la que ya usa la cocina, `MobWorld_Mesopotamia`) → el jugador ve **el
  paso de campamento nómada a aldea fija**. (Principio del matrix: **Área = región · Época = tiempo**.)
- **Elemento de la era:** **N** (nitrógeno — el nutriente del suelo). **Recompensa:** +Satisfacción, abre
  el Huerto.

## 2. Qué pasa en esta etapa (actividades del jugador)
La agricultura es "domesticar la vida", así que las acciones van de cuidar a poseer:
1. **Domesticar** — seleccionar y cuidar las primeras **plantas** (grano) y los **primeros animales**
   (del nómada cazador al pastor). Enlaza con los bonds del santuario (animal criado = bond alto).
2. **Sembrar / atender / regar** — el loop suave del huerto (arquetipo **Curar/atender**): preparar el
   suelo → nutrir los brotes → **cosechar** (abundancia).
3. **Almacenar el excedente** — guardar el grano (aquí entra **El Alfarero**: las vasijas). El excedente
   es el detonante del giro social (§4).
4. **Proteger** — de **plagas** (insectos → puente al mundo-insecto, §6), de **animales** que se comen la
   cosecha, y de **incursiones** de otras aldeas → **conflictos por territorio** (§5).

## 3. El giro (agridulce, de mob-quests-early §3)
Domesticar el grano trae **abundancia y las primeras aldeas** → pero el excedente hay que **guardarlo** →
los vecinos lo **codician** → **cercas, propiedad, jerarquía y las primeras disputas**; la vida sedentaria
trae además **nuevas enfermedades**. **Aprendizaje:** lo que cultivamos moldea la sociedad — **cultivar
generosidad, no solo grano.**

## 4. Personajes históricos (fila Neolítico del matrix; foco Huerto)
El jugador los sigue llamando por su **ancla de piedra** (mob-epochs-matrix). Los del Huerto y su entorno:
- **La Sembradora** (hilo **C · Semilla y Vida**, ancla *La Recolectora*) — **la protagonista del Huerto**.
  Arquetipo Curar/atender. *Héroe con nodo oscuro* (el excedente que trae la codicia). **Ya autorada** en
  `PhrasePools` (Tierra/Agua). Elemento **N**.
- **El Jefe de la Aldea** (hilo **E · Corona y Espada**, ancla *El Primer Jefe*) — **el antagonista del
  giro**: encarna propiedad/jerarquía/dominio. Es quien provoca los **conflictos internos** (§5).
  *Villano-capaz / héroe↔villano.*
- **La Contadora de Granos** (hilo **D · Trazo y Símbolo**, ancla *La Mano de Lascaux*) — cuenta y reparte
  el excedente: **primera contabilidad / proto-escritura**. Ata el almacenaje con lo social.
- **El Alfarero** (hilo **B · Barro y Metal**, ancla *El Tallador*) — las **vasijas** que guardan el grano.
  **Ya autorado** en `PhrasePools`. Puente con la cocina (cerámica) y el almacenaje.
- **El Lector del Cielo** (hilo **A · Fuego y Estrellas**, ancla *Guardián del Fuego*) — lee las
  **estaciones/el calendario** para saber cuándo sembrar. Conecta el fuego/cosmos con el ciclo agrícola.
- (De fondo, la aldea: *El Soñador* — hilo F, y *El Aldeano Burlón* — hilo G.)

> Siguiente autoría en `PhrasePools` (con `Historico()`): **La Contadora de Granos**, **El Jefe de la
> Aldea** y **El Lector del Cielo**. (La Sembradora y El Alfarero ya están.)

## 5. Misiones de DETENER CONFLICTOS (mediación) — arquetipo nuevo
Debut fuerte aquí (el giro neolítico lo pide), pero es un **arquetipo reutilizable en todas las áreas**
(santuario incluido). **Todo nace de los pensamientos/humores** (docs anima §6/§11):
- **Tipos de conflicto:**
  - **Entre integrantes del mismo equipo** — por **dominio** (uno quiere mandar: el deseo `poseer`) vs
    **libertad/autonomía** (otro no quiere ser mandado). El Jefe de la Aldea es el caso canónico.
  - **Entre tribus de la misma especie** — por **territorio/recursos** (dos aldeas, dos manadas).
  - (Enlaza con el núcleo del juego: **que los fuertes no se coman a los débiles** — mundo-insecto y
    santuario.)
- **Cómo surge (emergente):** un conflicto tiene una **causa = un pensamiento/humor**: adrenalina/cortisol
  altos → agresión; un `Deseo` de dominio o de territorio sin satisfacer. Dos ánimas con causas opuestas y
  proximidad → combate.
- **Cómo lo detiene el jugador (varias vías, no una sola):**
  1. **Bajar los humores** con un `ThoughtField` de calma (nudge de serotonina↓adrenalina) — apagar el fuego.
  2. **Mediar por posesión / alma compartida** (docs anima §11.7): poseer momentáneamente a uno y
     redirigir su acción, o "alma compartida" para que ambos vean lo del otro.
  3. **Satisfacer la necesidad de raíz**: dar **autonomía** (liberar al dominado), **territorio/comida**
     (repartir el excedente — cultivar generosidad), o subir el **bond** entre ellos.
  4. **Fuerza/autoridad** (última opción, con coste): separar por dominio → resuelve rápido pero deja poso.
- La **vía elegida cambia el resultado** (el aprendizaje del giro: la generosidad resuelve mejor que la
  fuerza). Es el mismo motor de utilidad+humores, ahora entre pares.

## 6. Conexiones
- **Con la Cocina:** el Huerto **produce los ingredientes** (grano/verduras) que la cocina cocina →
  bucle producción→consumo (contenedores). Reusa `SanctuaryResources`/`AreaProducer` (Food).
- **Con el mundo-insecto (Micro):** las **plagas** del huerto son el puente natural — proteger la cosecha
  = enfrentarse a hormigas/orugas a escala insecto (misma mecánica que las manchas de la cocina).
- **Con el santuario:** la **domesticación** conecta con los bonds y con las misiones de alimentar/cuidar
  animales (un animal criado en la aldea = bond alto, como los recién liberados del santuario).

## 7. Orden de construcción sugerido
1. **A — Loop del huerto (Meso):** sembrar → atender/regar → cosechar; produce Food (AreaProducer).
2. **B — Almacenaje + excedente:** vasijas/silo que se llenan; dispara el giro social.
3. **C — Mediación (detener conflictos):** el arquetipo de §5 con 2 aldeanos (dominio vs autonomía),
   resoluble por campo de calma / bond / reparto. *(El primer arquetipo emergente entre pares.)*
4. **D — Proteger (plagas):** enganche al mundo-insecto (hormigas vs cosecha).
5. **E — Históricos:** autorar y cablear La Contadora de Granos, El Jefe de la Aldea, El Lector del Cielo.
**Mínimo jugable:** A + B (huerto que produce y almacena) → luego C (la mediación, la novedad de diseño).
