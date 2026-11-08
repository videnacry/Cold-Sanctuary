# Andrea

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Andrea`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Hombre, italiano. *documentada (Berón 2026-10-18) · [character-andrea.md §Quién es](../character-andrea.md)*
- 1,78 m, joven; pelo castaño oscuro ondulado y rizado con volumen arriba, sonrisa amplia. *atestiguada (su foto) · [diseno-personajes.md §Fichas](../diseno-personajes.md) · [CubeHair.cs:164](../../Assets/Scripts/Avatar/CubeHair.cs#L164)*
- Siempre elegante: camisa negra de cuello, pantalón de vestir gris marengo, zapatos de piel; le gustan los anillos, las cadenas (plata, con cruz) y los pendientes; pulsera. *documentada y atestiguada · [character-andrea.md §Quién es](../character-andrea.md) · [CubeHair.cs:164](../../Assets/Scripts/Avatar/CubeHair.cs#L164)*
- Edad exacta y ojos: **Falta**.

## 2. Origen e historia

- Heredó muchas tierras con un río; cree que sus antepasados quizá fueron condes. Las tierras del santuario son suyas: es uno de los dueños. *documentada · [character-andrea.md §Quién es](../character-andrea.md)*
- Erasmus; una relación a distancia; a los 23 viajó a España para verla, ella lo dejó y él se quedó a trabajar. *documentada · [character-andrea.md §Historia](../character-andrea.md) 1-3*
- En Five Guys conoció a Leo (con Goluis, Gohageneis y Panterilia). Fue el único que hizo yoga de verdad con Leo; salían a correr, al gimnasio, a comer y de fiesta. *documentada · [character-andrea.md §Historia](../character-andrea.md) 4-5*
- Corriendo le contó a Leo lo de sus tierras y soñaron el santuario: frutales, animales, casas en los árboles, una comunidad con los compañeros de Five Guys; él quería un restaurante de lujo con platillos de fruta y pizza. *documentada · [character-andrea.md §Historia](../character-andrea.md) 6*
- Le contó a Leo la realidad de las mafias de donde vivía. Volvió a Italia prometiendo preparar los terrenos y llamarlos algún día. *documentada · [character-andrea.md §Historia](../character-andrea.md) 7-8*

## 3. Personalidad y frases

- Risueño, muy alegre y acogedor; le gusta hablar con desconocidos; «chill»: se toma su tiempo y saborea lo que hace (correr, bailar, cocinar). *documentada · [character-andrea.md §Quién es](../character-andrea.md)*
- Respeto y cariño por los animales, los insectos, el medio ambiente y las personas. *documentada · [character-andrea.md §Historia](../character-andrea.md) 8*
- Atención visual «afectiva». *recreada · [SampleSceneBuilder.cs:2223](../../Assets/Editor/SampleSceneBuilder.cs#L2223)*
- Frases propias: **Falta** (pendiente en su ficha: diálogos alegres con desconocidos). Sin vivencias ni anclas en el código.

## 4. Stats, margas y aptitudes

- Cuerpo: fuerza 1,6 (la más alta del personal) · agilidad 1,2 · resistencia 1,3 · masa 1,05. *recreada de Berón 2026-11-04: «el personaje con más fuerza es Andrea» · [SampleSceneBuilder.cs:3056](../../Assets/Editor/SampleSceneBuilder.cs#L3056) · [PoseGallery.cs:142](../../Assets/Editor/PoseGallery.cs#L142)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Entrenador del gimnasio: la misión `gym_planes` (revisar el registro de series y ajustar los planes), la clase de calentamiento de las 14 h, corregir y hacer de spotter. *documentada (Berón 2026-11-04) · [GymProgramming.cs:32](../../Assets/Scripts/Progression/GymProgramming.cs#L32) · [gimnasio-maquinas.md §15.5](../gimnasio-maquinas.md)*
- Comunicación: los avisos del santuario y la radio, en el comedor (`coc_avisos`). *documentada (Berón 2026-11-04) · [Professions.cs:65](../../Assets/Scripts/Progression/Professions.cs#L65) · [Professions.cs:135](../../Assets/Scripts/Progression/Professions.cs#L135)*
- Corre trayectos de unos 25 km a ritmo suave y graba vídeos artísticos de lo que descubre. *documentada · [character-andrea.md §Quién es](../character-andrea.md)*
- Hace yoga (con Leo). Fue maestro de yoga unas horas del 2026-11-04, tras irse Sadhguru; pasó a Josedrilo. *documentada · [character-andrea.md §En el juego](../character-andrea.md)*

## 6. Puesto y áreas

- Sale en el gimnasio y su área es el Gimnasio (antes, el comedor). *código · [SampleSceneBuilder.Construction.cs:165](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L165) · [SanctuaryDirector.cs:162](../../Assets/Scripts/World/SanctuaryDirector.cs#L162)*
- Su profesión le da `coc_avisos` en la Cocina. *código · [Professions.cs:65](../../Assets/Scripts/Progression/Professions.cs#L65)*
- `gym_planes` es suya primero y, sin él, va por rotación (regla de oro). *código · [reglas-personajes.md P11](../reglas-personajes.md)*

## 7. Relaciones

- Leo: amigo y compañero de Five Guys, con admiración mutua; soñaron juntos el santuario. *documentada · [character-andrea.md §Historia](../character-andrea.md)*
- Goluis, Gohageneis y Panterilia: compañeros de Five Guys. *documentada · ídem*
- Pareja: **Falta**. Vive solo en su casa del árbol ([ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16)).

## 8. Horario y costumbres

- Gimnasio: torso/pierna de fuerza, lunes, miércoles, jueves y sábado; hace su rutina desde las 15:00. *recreada · [GymProgramming.cs:209](../../Assets/Scripts/Progression/GymProgramming.cs#L209) · [gimnasio-maquinas.md §15.5](../gimnasio-maquinas.md)*
- Horario libre: el reparto común ([LeisureDirector.cs:86](../../Assets/Scripts/Progression/LeisureDirector.cs#L86)). Su ruta de correr de 25 km **no está en el juego**. *documentada como pendiente · [character-andrea.md §Pendiente](../character-andrea.md)*

## 9. Microcosmos y santuario

- En los microcosmos es una **mariquita** (elegante). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- En los microcosmos su nombre está en la lista de **nombres femeninos** ([AreaMicrocosmos.cs:1224](../../Assets/Editor/AreaMicrocosmos.cs#L1224)), aunque es hombre.
- Su cuerpo sirve de turista del restaurante ([RestaurantService.cs:571](../../Assets/Scripts/Kitchen/RestaurantService.cs#L571)) y de figura en la misión virtual de la Cocina ([VirtualMissionSceneBuilder.cs:142](../../Assets/Editor/VirtualMissionSceneBuilder.cs#L142)).
- Su santuario y su nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo, stats y rótulo | [SampleSceneBuilder.cs:3056](../../Assets/Editor/SampleSceneBuilder.cs#L3056) |
| Aspecto | [CubeHair.cs:167](../../Assets/Scripts/Avatar/CubeHair.cs#L167) |
| Entrenador | [GymProgramming.cs:26-32](../../Assets/Scripts/Progression/GymProgramming.cs#L26) |
| Profesión y su base | [Professions.cs:65](../../Assets/Scripts/Progression/Professions.cs#L65) · [Professions.cs:135](../../Assets/Scripts/Progression/Professions.cs#L135) |
| Personal y horario libre | [WorkdayDirector.cs:19](../../Assets/Scripts/Progression/WorkdayDirector.cs#L19) · [LeisureDirector.cs:86](../../Assets/Scripts/Progression/LeisureDirector.cs#L86) |
| Salida | [SampleSceneBuilder.Construction.cs:165](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L165) · [SanctuaryDirector.cs:162](../../Assets/Scripts/World/SanctuaryDirector.cs#L162) |
| Gimnasio | [GymProgramming.cs:209](../../Assets/Scripts/Progression/GymProgramming.cs#L209) |
| Casa | [ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16) |

## 11. Preguntas para Berón

1. ¿Su edad?
2. Su sueño era el restaurante de lujo: ¿debe tener algún papel allí (la carta de pizza y fruta) además de los avisos?
3. ¿Hacemos su ruta de correr de 25 km en el horario libre?
4. ¿Tiene pareja o familia en el santuario?
5. ¿En qué santuario y nivel está?
6. En los microcosmos su nombre se toma por femenino: ¿lo corregimos a masculino?
