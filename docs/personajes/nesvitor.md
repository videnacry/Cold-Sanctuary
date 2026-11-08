# Nesvitor (Néstor)

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Nesvitor`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real, de la familia o los amigos de Berón. Berón lo nombró «Nesvitor (nestor)». *documentada · Berón 2026-11-04 · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md)*
- Hombre. Su foto lo muestra mayor. *atestiguada (`nestor.jpg`) · [SampleSceneBuilder.Construction.cs:196](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L196)*
- Piel curtida morena; pelo canoso y negro peinado hacia atrás; bigote espeso canoso; forro polar negro abierto sobre camisa blanca de cuadros finos azules y camiseta amarilla; gafas colgadas de la camisa. *atestiguada (`nestor.jpg`) · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md) · [CubeHair.cs:191](../../Assets/Scripts/Avatar/CubeHair.cs#L191)*
- Estatura 1,68 m: estimada. *inventada · ídem*
- Edad y nacionalidad: **Falta**.

## 2. Origen e historia

- Historia, origen y llegada al santuario: **Falta**.

## 3. Personalidad y frases

- Carácter y frases: **Falta**. No tiene vivencias, anclas ni conducta propia en el código.

## 4. Stats, margas y aptitudes

- Fuerza 0,95 · agilidad 0,9 · resistencia 1,05 · masa 1,0: «stats cercanas a 1», algo menos ágil porque la foto lo muestra mayor. *inventada · [SampleSceneBuilder.Construction.cs:199](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L199)*
- Aptitudes mentales y margas: **Falta** (las de humano por defecto).

## 5. Profesión y lo que sabe hacer

- Guardia y agricultor. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [Professions.cs:123](../../Assets/Scripts/Progression/Professions.cs#L123)*
- Antes, en el reparto provisional, llevó las cuentas del santuario desde el restaurante («el papel de persona mayor que se pedía para él», heredado de Warren Buffett). Ya no vale. *inventada · [personajes-mesocosmos.md §Reparto de papeles](../personajes-mesocosmos.md)*

## 6. Puesto y áreas

- Profesión en el código: Guardia, Sala de guardia y Huerto. *código · [Professions.cs:45](../../Assets/Scripts/Progression/Professions.cs#L45) · [sala-de-guardia.md](../sala-de-guardia.md)*
- Sale en el huerto. *código · [SanctuaryDirector.cs:177](../../Assets/Scripts/World/SanctuaryDirector.cs#L177) · [SampleSceneBuilder.Construction.cs:172](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L172)*
- En la brigada de la prueba del restaurante, va en el comedor; su cuerpo es uno de los turistas virtuales. *código · [RestaurantServiceTest.cs:40](../../Assets/Scripts/Tests/RestaurantServiceTest.cs#L40) · [RestaurantService.cs:571](../../Assets/Scripts/Kitchen/RestaurantService.cs#L571)*

## 7. Relaciones

- Familia o amigo de Berón: el parentesco exacto **Falta**.
- Pareja e hijos: **Falta**. Vive solo en su casa del árbol. *código · [ResidentialBuilder.cs:22](../../Assets/Editor/ResidentialBuilder.cs#L22)*

## 8. Horario y costumbres

- Gimnasio: cuerpo entero de salud, lunes y jueves, en máquinas guiadas. *recreada · [GymProgramming.cs:210](../../Assets/Scripts/Progression/GymProgramming.cs#L210) · [gimnasio-maquinas.md](../gimnasio-maquinas.md) §15*
- Horario libre: el reparto común. *código · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*
- Costumbres propias: **Falta**.

## 9. Microcosmos y santuario

- En los microcosmos: una especie variada por su nombre, entre las de su bioma (no tiene una propia). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Datos (rótulo, ropa, stats) | [SampleSceneBuilder.Construction.cs:199](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L199) (`FamilyCharacters`) |
| Dónde sale | [SampleSceneBuilder.Construction.cs:172](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L172) · [SanctuaryDirector.cs:177](../../Assets/Scripts/World/SanctuaryDirector.cs#L177) |
| Aspecto | [CubeHair.cs:191](../../Assets/Scripts/Avatar/CubeHair.cs#L191) |
| Profesión y base | [Professions.cs:45](../../Assets/Scripts/Progression/Professions.cs#L45) · [Professions.cs:123](../../Assets/Scripts/Progression/Professions.cs#L123) |
| Gimnasio | [GymProgramming.cs:210](../../Assets/Scripts/Progression/GymProgramming.cs#L210) |
| Personal y casa | [WorkdayDirector.cs:20](../../Assets/Scripts/Progression/WorkdayDirector.cs#L20) · [ResidentialBuilder.cs:22](../../Assets/Editor/ResidentialBuilder.cs#L22) |

## 11. Preguntas para Berón

1. ¿Quién es para ti, de dónde es y qué le gusta? ¿Alguna historia suya para el juego?
2. La ficha de Irosene ([character-irosene.md §Historia](../character-irosene.md)) nombra a un «Néstor». ¿Es otra persona, verdad? (No lo damos por hecho.)
3. La razón de su plan de gimnasio dice «el mayor de las cuentas», del reparto provisional ([GymProgramming.cs:210](../../Assets/Scripts/Progression/GymProgramming.cs#L210)), y dos comentarios lo llaman «consejero» ([SampleSceneBuilder.Construction.cs:168](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L168), [SanctuaryDirector.cs:168](../../Assets/Scripts/World/SanctuaryDirector.cs#L168)). ¿Lo quitamos?
4. ¿Plan de gimnasio de salud por ser mayor, o como los demás?
