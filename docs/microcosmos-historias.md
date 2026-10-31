# Microcosmos de historias (2026-10-22)

Grandes figuras de la historia recreadas en el **microcosmos** del área del santuario que mejor las cuenta (el área
×10, ver `microcosmos-areas.md`). Cada historia es **su propia escena** y se entra por la **máquina de virtualización**
de su área (opción «Simulación: …»). El elenco de la enfermería renace con su nombre en cada una. Propuesta de origen:
[`historias-por-area.md`](historias-por-area.md).

| Escena | Área (máquina) | Historia | Marga |
|---|---|---|---|
| `Microcosmos_Patanjali` | Sala de Yoga | Patanjali y los ocho miembros del yoga (Yoga Sūtras I.2, II.29, II.46, III.1–3) | Yoga |
| `Microcosmos_Tregua1914` | Gimnasio | La tregua de Navidad, Flandes 1914 (Primera Guerra Mundial) | Vínculos |
| `Microcosmos_Nightingale` | Enfermería | Florence Nightingale en Escútari, 1854–55 | Vínculos |
| `Microcosmos_Buda` | Enfermería | Siddhārtha y los cuatro encuentros → camino del medio → meditación | Yoga |
| `Microcosmos_Wright` | Mecánica | Los hermanos Wright: de la bicicleta al avión (1892–1903) | Stats |
| `Microcosmos_Eratostenes` | Planta Atómica | Eratóstenes mide la Tierra (~240 a. C.), primer tramo de la escalera de la física | Stats |
| `Microcosmos_BruceLee` | Dojo (por ahora, desde la máquina del Gimnasio) | Bruce Lee: Wing Chun con Yip Man, «sé agua», Seattle, el golpe de una pulgada, el Jeet Kune Do y sus notas de 1970 ([`microcosmos-bruce-lee.md`](microcosmos-bruce-lee.md)) | Stats |

La entrada de Eratóstenes en el menú y el área `AtomicPlant_Area` las pone el trabajo de la planta atómica
(`PhysicsSceneBuilder`); si el área aún no está en la escena, el microcosmos se genera sobre suelo libre.

## Por qué estas elecciones

- **La guerra = la tregua de Navidad de 1914.** Es real y está muy documentada (cartas, Imperial War Museums), cuenta
  la guerra entera (trincheras, tierra de nadie, millones de muertos) y su giro enseña lo central del juego: el enemigo
  también es una persona. No necesita violencia en pantalla. Va en el **Gimnasio** porque su imagen más conocida es el
  partido de fútbol en tierra de nadie. Siguientes candidatas: Termópilas (Grecia, sobre Egina), la guerra de Crimea ya
  aparece desde el hospital (Nightingale).
- **Patanjali en la Sala de Yoga**: la historia es la del propio texto que se practica allí. Se cuenta como lo que se
  sabe (fecha discutida, casi nada de su vida) y la leyenda del nombre (Gonikā, «pata» + «añjali») como leyenda.
- **Buda en la Enfermería**: vejez, enfermedad y muerte son lo que se ve en una enfermería; acaba meditando (yoga).

## Cómo funciona

- **`StoryMission`** (`Assets/Scripts/Mission/`): misión genérica por PASOS con una sola tecla (E): `Talk` (hablar con
  alguien), `Reach` (llegar), `Collect` (recoger y, si hay `deliverTo`, llevarlo: va sobre la cabeza, a la vista),
  `VisitAll` (atender uno a uno: fregar, abrir ventanas, la ronda de noche) y `Hold` (quedarse quieto: meditar, escuchar,
  medir una sombra). Cada paso puede hacer aparecer/desaparecer cosas (la rosa de Nightingale, el Flyer en el raíl y
  volando). Al terminar: XP de su marga y la `HistoryCard` con lo que pasó de verdad y sus fuentes. Egina y las
  simulaciones de compasión mantienen su misión propia (mecánica distinta).
- **`MicroRoutine`** (`Assets/Scripts/Microcosmos/`): la jornada de los mirmidones de la historia: van de un puesto a
  otro (río, taller, trinchera, lavandería…) y se quedan un rato. Así el pueblo vive aunque no se le hable.
- Ropa: `CubeInsect.WearHat` (tocados sueltos) + nuevos `Hat.Bonnet` (cofia), `Hat.Spike` (casco alemán 1914),
  `Hat.Turban`.
- Builder: `Assets/Editor/StorySceneBuilder.cs` (menú `Tools/Cold Sanctuary/Build Microcosmos de historias`; también lo
  llama «Build Sample Scene Blockout»).

## Pendiente

- Probar en el Editor (no hay Unity en la nube): generar, entrar por cada máquina y jugar los pasos.
- Las cifras de las tarjetas están contrastadas con las fuentes citadas; las que son aproximadas lo dicen («alrededor
  de», «se calcula»).
- Siguientes: Semmelweis (hospital), Arquímedes/Galileo (planta), Bolívar (Construcción), Aristóteles (Cría).
