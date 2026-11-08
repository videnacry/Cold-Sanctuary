# Irosene

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Irosene` (en la escena, `Irosene_Post`). Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.
>
> Su ficha larga cuenta una historia familiar delicada. Aquí va solo un resumen; el detalle se queda en
> [character-irosene.md](../character-irosene.md), con sus ⚠️.

## 1. Identidad y aspecto

- Mujer de más de 70 años, madre de 9 hijos. *documentada · [character-irosene.md §Resumen](../character-irosene.md)*
- Llegó en avión desde Ecuador. *documentada · [character-irosene.md §Relaciones, «Contexto de llegada»](../character-irosene.md)*. Que sea **ecuatoriana** lo dicen [diseno-personajes.md §Fichas](../diseno-personajes.md) y [CubeHair.cs:257](../../Assets/Scripts/Avatar/CubeHair.cs#L257), pero la ficha larga no lo escribe: *recreada*.
- 1,55 m; melena larga ondulada, rubia con canas y flequillo; piel clara; ojos azul grisáceo; tejido andino coral. *atestiguada (su captura) · [diseno-personajes.md §Fichas](../diseno-personajes.md) · [CubeHair.cs:258](../../Assets/Scripts/Avatar/CubeHair.cs#L258)*

## 2. Origen e historia

- Creció en un hogar con mucha hambre y castigos; con sus hermanos escondía azúcar para comer. *documentada · [character-irosene.md §Historia](../character-irosene.md)*
- Se rebeló contra su padre, que quería casarla siendo colegiala; se casó joven para escapar de casa, y ese primer matrimonio también fue violento. Al final denunció al esposo. *documentada, con detalles ⚠️ · [character-irosene.md §Historia](../character-irosene.md)*
- Montó de la nada un **negocio de dulces** con las recetas de su niñez: sus hijos trabajaban mientras estudiaban y sus unos 6 hermanos la ayudaban. *documentada · ídem*
- Superó un cáncer; tenía sobrepeso y poca movilidad. En el santuario pasó de casi no poder caminar a **correr, escalar y bucear profundo**. *documentada · [character-irosene.md §Resumen](../character-irosene.md)*
- Vino con Lizmibet para ver a su nieto Leo «por última vez»; en el santuario creyeron que les quedan muchos años. No ven a Leo desde hace más de 10 años. *documentada · [character-irosene.md §Relaciones](../character-irosene.md)*
- Con Lizmibet pasó en el Nivel 1 mucho más tiempo de lo normal y aprendió hechizos avanzados: «las únicas del Nivel 2 con hechizos de Nivel 4». *documentada ⚠️ · [character-irosene.md §Lugar en el santuario](../character-irosene.md)*

## 3. Personalidad y frases

- Muy expresiva sin palabras (cara, manos, brazos). Salta de la alegría a la ira, a la melancolía y a la sabiduría; cuenta chistes, se queja del mundo, canta. *documentada · [character-irosene.md §Personalidad y tono](../character-irosene.md)*
- Motivadora, con mucho sentimiento. Carácter fuerte, algo altiva, y a la vez «un peluche cariñoso», muy querida por sus nietos. *documentada · [character-irosene.md §Resumen, §Personalidad](../character-irosene.md)*
- Justiciera: se mete a hablar si ve una injusticia en la calle. Cuando tuvo cáncer, avisaba a otros pacientes de medicamentos que creía peligrosos y sugería recetas naturales que aprende de libros y canales. *documentada · ídem*
- Le encanta el karaoke; su canción favorita es «Soy rebelde» (no se cita la letra, J6). Le gustan las tragaperras. *documentada · ídem*
- Dichos suyos: «valora cada día como si fuera el último», «ama a tus padres», «yo soy el alfa y el omega». *documentada · [character-irosene.md §Personalidad, §Estilo de diálogo](../character-irosene.md)*
- Frases de ejemplo para sus diálogos (motivacional, melancólico, saludo). *recreadas · [character-irosene.md §Estilo de diálogo](../character-irosene.md)*. Sus tres `DialogueSequence` están **vacíos**: no hay assets ([Irosene.cs:9](../../Assets/Scripts/Companion/Companions/Irosene.cs#L9)).
- Cuatro vivencias (motivadora, dulces, sobrevivir, renacer). *recreadas · [PhrasePools.cs:68-80](../../Assets/Scripts/Mind/PhrasePools.cs#L68)*
- Anclas: `alpha_and_omega` 0,95, `pursue_dreams` 0,9, `love_your_people` 0,85 (crece), `rebellion` 0,8 y `pride` 0,6 (se suavizan). *recreada · [character-irosene.md §Anchors](../character-irosene.md) · [SampleSceneBuilder.cs:3334](../../Assets/Editor/SampleSceneBuilder.cs#L3334) · [Irosene.cs:25](../../Assets/Scripts/Companion/Companions/Irosene.cs#L25)*

## 4. Stats, margas y aptitudes

- Cuerpo y mente de su arquetipo: agilidad 1,2 · percepción 1,3 · fuerza 1,2 · masa 1,3 · resistencia 1,3 · adaptabilidad 1,6; temple 1,4 · razonamiento 1,1 · memoria 1,3 · creatividad 1,5 · sociabilidad 1,7 · disciplina 1,1. Tono elemental Fuego. *recreada · [character-irosene.md §Aptitudes](../character-irosene.md) · [creature-stats.md §Irosene](../creature-stats.md) · [Archetypes.cs:141](../../Assets/Scripts/Soul/Archetypes.cs#L141), [:175](../../Assets/Scripts/Soul/Archetypes.cs#L175)*
- Mezcla de alma de ejemplo: `Human 97 + Toro 1 + Mono + Gallina + Agua + Fuego` + `bonusPack2`. *recreada · [soul-composition-blend.md](../soul-composition-blend.md)*
- Canal primario Satisfacción: estar cerca de ella repone la satisfacción del jugador (`encouragementBurst`). *recreada · [Irosene.cs:14-17](../../Assets/Scripts/Companion/Companions/Irosene.cs#L14) · [SampleSceneBuilder.cs:3332](../../Assets/Editor/SampleSceneBuilder.cs#L3332)*
- Atención visual «Esteta». *recreada · [SampleSceneBuilder.cs:2222](../../Assets/Editor/SampleSceneBuilder.cs#L2222) · [atencion-visual.md](../atencion-visual.md)*
- Margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- **Letras**: profesora de poesía y narración. *documentada (Berón 2026-11-04) · [personajes-mesocosmos.md §Profesiones](../personajes-mesocosmos.md) · [Professions.cs:65](../../Assets/Scripts/Progression/Professions.cs#L65), base en [:133](../../Assets/Scripts/Progression/Professions.cs#L133)*
- **Textil**: envolver dulces de niña le dio velocidad y destreza; destaca en el estudio textil. *documentada · [character-irosene.md §Historia](../character-irosene.md) · [area-missions-spec.md §TextileStudio](../area-missions-spec.md)*
- Remedios naturales, recetas de dulces, cocina de su niñez. *documentada · [character-irosene.md](../character-irosene.md)*. En el juego no hay misión que lo use (la farmacia nombra «remedios de Irosene» solo en [area-missions-spec.md §SupplementsPharmacy](../area-missions-spec.md)).

## 6. Puesto y áreas

- Sale en el Aula (`HomeArea` Classroom) y su profesión le da cada día `aul_letras` (1 h) y una misión del Estudio Textil (`tex_`). *código · [SanctuaryDirector.cs:159](../../Assets/Scripts/World/SanctuaryDirector.cs#L159) · [SampleSceneBuilder.Construction.cs:180](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L180) · [AreaFunctions.cs:174](../../Assets/Scripts/Progression/AreaFunctions.cs#L174)*
- El resto de su jornada va por rotación, como todos (J30).
- **Ya no tiene** la segunda plaza de yoga (no tenía base: P7) ni la de experta en frutales (hoy Shalva). *documentada · [reglas-personajes.md](../reglas-personajes.md) · [jardines-frutales.md §La experta](../jardines-frutales.md)*
- En la prueba del restaurante hace la limpieza. *código · [RestaurantServiceTest.cs:40](../../Assets/Scripts/Tests/RestaurantServiceTest.cs#L40)*

## 7. Relaciones

- **Fausto**, mecánico: su pareja; comparten casa del árbol. *documentada (Berón 2026-11-04) · [personajes-mesocosmos.md §Parejas](../personajes-mesocosmos.md) · [ResidentialBuilder.cs:42](../../Assets/Editor/ResidentialBuilder.cs#L42)*
- **Lizmibet**: su gran amiga, de su misma edad y nivel; probable consuegra. *documentada, consuegra ⚠️ · [character-irosene.md §Relaciones](../character-irosene.md)*
- **Leo**: su nieto, que las trajo al santuario y está en el último nivel. *documentada · ídem*
- Una hija «Alicia», exesposa de Josedrilo; Josedrilo, padre de Leo. *documentada ⚠️ · ídem*. Ver la pregunta 3.
- Un primer esposo, «Néstor». *documentada · [character-irosene.md §Historia](../character-irosene.md)*. Ver la pregunta 3.
- Sus hijos y la relación con ellos: ficha larga, con ⚠️ y tema delicado. *documentada ⚠️ · [character-irosene.md §Relaciones](../character-irosene.md)*
- Con Panterilia: la visita en la limpieza. *documentada · [character-irosene.md §Lugar en el santuario](../character-irosene.md)*

## 8. Horario y costumbres

- Gimnasio: cuerpo entero de resistencia, miércoles y sábado («más de 70, corre, escala y bucea»). *recreada · [GymProgramming.cs:207](../../Assets/Scripts/Progression/GymProgramming.cs#L207) · [gimnasio-maquinas.md §15](../gimnasio-maquinas.md)*
- Horario libre: el reparto común ([LeisureDirector.cs:86](../../Assets/Scripts/Progression/LeisureDirector.cs#L86)). El karaoke, el canto, correr, escalar y bucear **no son actividades suyas** en el juego.

## 9. Microcosmos y santuario

- En los microcosmos es una **araña**: tejedora, por el textil. *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Su nivel, según cada doc: Tier 4, «nivel submarino», jardín submarino ([character-irosene.md §Lugar](../character-irosene.md), [area-missions-spec.md §UnderwaterGarden](../area-missions-spec.md)); «3er santuario V1» ([soul-composition-blend.md](../soul-composition-blend.md)); de las antiguas, llega con hechizos potentes ([world-topology-and-planes.md](../world-topology-and-planes.md)). En el juego vive en el santuario 1 con todos.
- Berón (2026-11-08): los creados antes de Linduarda, Irosene y Lizmibet serían de santuario 1, nivel 1. *documentada (nota de Berón)*
- Su cuerpo es el que copian las pruebas para pacientes y donantes. *código · [BloodBankTest.cs:55](../../Assets/Scripts/Tests/BloodBankTest.cs#L55)*. Y es turista del restaurante ([RestaurantService.cs:571](../../Assets/Scripts/Kitchen/RestaurantService.cs#L571)).

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo, ánimo y anclas en la escena | [SampleSceneBuilder.cs:3329](../../Assets/Editor/SampleSceneBuilder.cs#L3329) (`BuildCharacters`) |
| Su conducta (motivación) | [Irosene.cs](../../Assets/Scripts/Companion/Companions/Irosene.cs) |
| Stats | [Archetypes.cs:141](../../Assets/Scripts/Soul/Archetypes.cs#L141), [:175](../../Assets/Scripts/Soul/Archetypes.cs#L175) |
| Vivencias | [PhrasePools.cs:68](../../Assets/Scripts/Mind/PhrasePools.cs#L68) |
| Aspecto | [CubeHair.cs:259](../../Assets/Scripts/Avatar/CubeHair.cs#L259) |
| Profesión y su base | [Professions.cs:65](../../Assets/Scripts/Progression/Professions.cs#L65), [:133](../../Assets/Scripts/Progression/Professions.cs#L133) |
| Área de salida | [SanctuaryDirector.cs:159](../../Assets/Scripts/World/SanctuaryDirector.cs#L159) |
| Gimnasio | [GymProgramming.cs:207](../../Assets/Scripts/Progression/GymProgramming.cs#L207) |
| Casa y pareja | [ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16), [:42](../../Assets/Editor/ResidentialBuilder.cs#L42) |

## 11. Preguntas para Berón

1. ¿Es ecuatoriana? La ficha larga dice que voló desde Ecuador, pero no su nacionalidad.
2. ¿Confirma los ⚠️ de su historia? ¿Cuánto de la parte delicada (la violencia en su familia) debe salir en el juego, y con qué tono?
3. Su ficha nombra a un primer esposo «Néstor», a una hija «Alicia», exesposa de Josedrilo, y a Lizmibet como consuegra. En el santuario están Nesvitor «Néstor», Alicita «Alicia», Josedrilo y Lizmibet. ¿Son las mismas personas o solo coinciden los nombres?
4. ¿Fausto es alguien de su historia? La ficha larga habla de un sobrino de Néstor que acabaría siendo su pareja ⚠️.
5. ¿En qué santuario y nivel está? Los docs dicen Tier 4 (submarino), «3er santuario V1» y, por la regla del 2026-11-08, por encima de santuario 1, nivel 1.
6. ¿Deben estar en su horario libre el karaoke, correr, escalar o bucear? ¿Y sus remedios naturales, en la farmacia o el huerto?
7. ¿Escribimos sus diálogos (los tres `DialogueSequence` están vacíos) a partir de sus dichos documentados?
