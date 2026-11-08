# Linduarda

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Linduarda`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Mujer: Berón usa el femenino («la doctora», «curiosa», «enamorada»). *documentada · [character-linduarda.md](../character-linduarda.md), nota final*
- Ecuatoriana. *documentada ⚠️ · [character-linduarda.md §Biografía](../character-linduarda.md)*
- 1,60 m, coleta alta negra, gafas redondas doradas, piel clara-morena; pijama sanitario azul celeste sobre camiseta blanca de manga larga. *atestiguada (su captura) · [diseno-personajes.md §Fichas](../diseno-personajes.md) · [CubeHair.cs:150](../../Assets/Scripts/Avatar/CubeHair.cs#L150)*
- Edad y ojos: **Falta**.

## 2. Origen e historia

- Antes de la medicina fue camarera en EE. UU. y vendedora online por su cuenta. *documentada ⚠️ · [character-linduarda.md §Biografía](../character-linduarda.md)*
- Estudió medicina en Ucrania. A mitad de carrera empezó la guerra con Rusia: volvió a Ecuador, hizo prácticas de hospital y regresó a Ucrania a terminar. Al graduarse, voluntariado. *documentada ⚠️ · [character-linduarda.md §Resumen, §Biografía](../character-linduarda.md)*
- Cruzó de Ucrania a Ecuador «para ayudar a sus amigos». *documentada · [character-linduarda.md §Personalidad y tono](../character-linduarda.md)*
- Hechicera de nivel 4 (S4): come a nivel de quarks, crea y deshace materia, sana abasteciendo y está en el umbral de la teletransportación. *documentada (ficha) · [character-linduarda.md §Resumen](../character-linduarda.md) · [magic-metabolism-progression.md](../magic-metabolism-progression.md) §3 y §15*
- Cómo y cuándo llegó al santuario: **Falta**.

## 3. Personalidad y frases

- Curiosa y perfeccionista, buena observadora; leal y tenaz, no abandona lo que empieza; relajada, aventurera y emprendedora. *documentada · [character-linduarda.md §Personalidad y tono](../character-linduarda.md)*
- Devota de su familia: escribe a sus padres cada día; conoce los «lenguajes del amor» y con ellos armonizó a su familia. *documentada · ídem*
- A la moda, deportista y disciplinada: gimnasio a diario. *documentada · ídem*
- Le gustan la fotografía y la arquitectura; viaja a España por sus edificios y calles; «enamorada de Sitges». *documentada · ídem*
- Su sueño: teletransportarse por el mundo. *documentada · [character-linduarda.md §Resumen](../character-linduarda.md)*
- Frases de ejemplo («Yo estudié entre bombas y volví a terminar…»). *inventadas, borrador ⚠️ · [character-linduarda.md §Estilo de diálogo](../character-linduarda.md)*. No están en ningún `DialogueSequence` ni en `PhrasePools`: **en el juego no dice nada propio**.
- Anclas de pensamiento, vivencias y humor: **Falta** en el código (no tiene `MoodState` ni arquetipo).

## 4. Stats, margas y aptitudes

- Cuerpo: fuerza 1,15 · agilidad 1,35 · resistencia 1,45 · masa 1,05, por «deportista, gimnasio a diario». *recreada · [SampleSceneBuilder.cs:3036](../../Assets/Editor/SampleSceneBuilder.cs#L3036) · [PoseGallery.cs:139](../../Assets/Editor/PoseGallery.cs#L139)*
- Aptitudes mentales propuestas en palabras (disciplina, resistencia, percepción, razonamiento, sociabilidad y adaptabilidad «altas»; creatividad y temple «media-alta»). *recreada · [character-linduarda.md §Mapeo a aptitudes](../character-linduarda.md)*. **No están en el código**: no hay arquetipo `Linduarda` en [Archetypes.cs](../../Assets/Scripts/Soul/Archetypes.cs).
- Margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Médica: «la doctora del santuario», Una Salud (personas y animales). *documentada · [character-linduarda.md](../character-linduarda.md), cabecera*
- Analiza (muestra de sangre al analizador) y vacuna, con sus hechizos. *código · [Linduarda.cs:3](../../Assets/Scripts/World/Linduarda.cs#L3) · [ClinicalSpells.cs:5](../../Assets/Scripts/Soul/ClinicalSpells.cs#L5)*
- Opera la máquina de virtualización y da acceso al Microcosmos. *documentada · [character-linduarda.md §Rol narrativo](../character-linduarda.md)*
- **No tiene entrada en `Professions.All`** ni base en `Professions.Basis`: su profesión no le da misiones en la ronda de profesiones. Está en `Professions.Medical`, la lista que busca primero el submarino-ambulancia. *código · [Professions.cs:26](../../Assets/Scripts/Progression/Professions.cs#L26)*

## 6. Puesto y áreas

- Médica del hospital (enfermería). *documentada · [personajes-mesocosmos.md §Profesiones](../personajes-mesocosmos.md)*
- Lleva los registros y la vacunación del hospital con Lizmibet (heredado de Bill Gates). *recreada · [personajes-mesocosmos.md §Por qué así](../personajes-mesocosmos.md)*
- Área de salida: **ninguna** en `SanctuaryDirector.HomeArea` ([SanctuaryDirector.cs:157](../../Assets/Scripts/World/SanctuaryDirector.cs#L157)); la escena la pone en la enfermería ([SampleSceneBuilder.cs:3027](../../Assets/Editor/SampleSceneBuilder.cs#L3027)).
- En la jornada rota como todos (J30): le tocan misiones de cualquier área, también obras ([WorkdayDirector.cs:586](../../Assets/Scripts/Progression/WorkdayDirector.cs#L586)). Su conducta de médica solo se enciende en el hospital o si hay un recién llegado ([WorkdayDirector.cs:625-627](../../Assets/Scripts/Progression/WorkdayDirector.cs#L625)).
- No cuida a los animales ingresados: la jornada la excluye de `VetCare` ([WorkdayDirector.cs:621](../../Assets/Scripts/Progression/WorkdayDirector.cs#L621)), aunque su ficha dice Una Salud.
- Iniciación de recién llegados: en la jornada la hace quien tiene esa misión; **ella solo fuera de la jornada** (pruebas). *código · [NewcomerInitiator.cs:7](../../Assets/Scripts/World/NewcomerInitiator.cs#L7) · [Linduarda.cs:36](../../Assets/Scripts/World/Linduarda.cs#L36)*

## 7. Relaciones

- Sus padres: les escribe a diario. *documentada · [character-linduarda.md §Personalidad y tono](../character-linduarda.md)*
- Kushal: le concede entrar al Microcosmos a cambio de tareas de la enfermería. *documentada · [character-linduarda.md §Rol narrativo](../character-linduarda.md) · [microcosmos-eras-and-observation.md](../microcosmos-eras-and-observation.md)*
- La Magnate: Linduarda certifica al recién llegado y la llama para que lo inicie. *código · [Linduarda.cs:44](../../Assets/Scripts/World/Linduarda.cs#L44)*
- Lizmibet: atiende con ella en la enfermería. *recreada · [SampleSceneBuilder.cs:3040](../../Assets/Editor/SampleSceneBuilder.cs#L3040)*
- Pareja y amigos: **Falta**. Vive sola en su casa del árbol ([ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16)).

## 8. Horario y costumbres

- Gimnasio: empuje/tirón/pierna de lunes a sábado, hipertrofia. *recreada · [GymProgramming.cs:202](../../Assets/Scripts/Progression/GymProgramming.cs#L202) · [gimnasio-maquinas.md](../gimnasio-maquinas.md) §15*
- Horario libre: el reparto común ([LeisureDirector.cs:86](../../Assets/Scripts/Progression/LeisureDirector.cs#L86)); si hay un recién llegado lo atiende y no va a su ocio ([LeisureDirector.cs:590](../../Assets/Scripts/Progression/LeisureDirector.cs#L590), [horario-libre.md](../horario-libre.md)).
- La fotografía y los viajes **no están en el juego**: ninguna actividad libre suya.

## 9. Microcosmos y santuario

- En los microcosmos es una **luciérnaga** (las médicas: la luz). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Encarga misiones a Kushal en el Microcosmos: limpiar los conductos de una máquina, investigar hongos, extraer un cabello de cada pelotero, recuperar la semilla de la rueda. *recreada · [microcosmos-eras-and-observation.md](../microcosmos-eras-and-observation.md) · [microcosmos-dungbeetle-level.md §3 y §4](../microcosmos-dungbeetle-level.md) · [novela.md](../novela.md)*
- Nivel: hechicera de nivel 4. Berón (2026-11-08) cree que los creados **antes** de Linduarda, Irosene y Lizmibet son de santuario 1, nivel 1. *documentada (nota de Berón)*. Su santuario y su nivel exactos: **Falta**.
- **Recuerdo de Berón, por confirmar** (2026-11-08): Linduarda vive en el santuario 3 o en uno más alto. *documentada ⚠️*

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo, stats y conducta en la escena | [SampleSceneBuilder.cs:3027](../../Assets/Editor/SampleSceneBuilder.cs#L3027) (`BuildLinduarda`) |
| Su conducta: la salud del santuario | [Linduarda.cs](../../Assets/Scripts/World/Linduarda.cs) |
| Aspecto | [CubeHair.cs:151](../../Assets/Scripts/Avatar/CubeHair.cs#L151) |
| Personal y horario libre | [WorkdayDirector.cs:19](../../Assets/Scripts/Progression/WorkdayDirector.cs#L19) · [LeisureDirector.cs:86](../../Assets/Scripts/Progression/LeisureDirector.cs#L86) |
| Personal sanitario | [Professions.cs:26](../../Assets/Scripts/Progression/Professions.cs#L26) |
| Gimnasio | [GymProgramming.cs:202](../../Assets/Scripts/Progression/GymProgramming.cs#L202) |
| Casa | [ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16) |

## 11. Preguntas para Berón

1. ¿Confirma lo marcado ⚠️ de su biografía (camarera en EE. UU., la vuelta a Ucrania, el voluntariado)? ¿Su edad?
2. ¿Cómo y cuándo llegó al santuario?
3. Su ficha dice Una Salud, pero la jornada no la deja cuidar a los animales ingresados. ¿Debe hacerlo?
4. ¿Le damos una entrada de profesión (médica) con su base, como a Ivelyna y Malena? Hoy le tocan obras y no hace consultas por profesión.
5. ¿Debe iniciar ella a los recién llegados también en la jornada, como cuentan su ficha y la novela, o vale el de turno (J5, J32)?
6. ¿La fotografía y la arquitectura deben ser actividades de su horario libre?
7. ¿En qué santuario y nivel está? Berón recuerda (2026-11-08) el santuario 3 o uno más alto: ¿cuál, y qué nivel?
8. ¿Su ropa es el pijama sanitario de la captura? Un comentario viejo de [CubeHair.cs:149](../../Assets/Scripts/Avatar/CubeHair.cs#L149) dice «bata blanca».
