# Alicita (Alicia)

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Alicita`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real, de la familia o los amigos de Berón. Berón la nombró «Alicita (alicia)». *documentada · Berón 2026-11-04 · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md)*
- Mujer: Berón la nombra en femenino (`Fem()`). *documentada · ídem*
- Piel clara morena; melena larga lisa negra; aros pequeños; chaqueta negra con cremallera, cerrada, con el cuello verde fosforito de la camiseta asomando; sonriente. *atestiguada (`alicia.jpg`) · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md) · [CubeHair.cs:200](../../Assets/Scripts/Avatar/CubeHair.cs#L200)*
- Estatura 1,58 m: estimada. *inventada · ídem*
- Edad y nacionalidad: **Falta**.

## 2. Origen e historia

- Estudió un máster: allí conoció a Julian. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md)*
- Origen y llegada al santuario: **Falta**.

## 3. Personalidad y frases

- Carácter y frases: **Falta**. Sin vivencias, anclas ni conducta propia en el código.

## 4. Stats, margas y aptitudes

- Fuerza 0,9 · agilidad 1,0 · resistencia 1,05 · masa 0,95: «stats cercanas a 1». *inventada · [SampleSceneBuilder.Construction.cs:202](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L202)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Profesional de contabilidad y *project manager* (dirección de proyectos). *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [Professions.cs:129](../../Assets/Scripts/Progression/Professions.cs#L129)*
- En el reparto provisional llevó el Huerto (alimentar a los visitantes, de MacKenzie Scott). Ya no vale. *inventada · [personajes-mesocosmos.md §Reparto de papeles](../personajes-mesocosmos.md)*

## 6. Puesto y áreas

- `coc_cuentas` (las cuentas del santuario en la mesa del restaurante) y `con_proyectos` (dirigir las obras: plan, plazos, material y equipos). *recreada · [Professions.cs:56](../../Assets/Scripts/Progression/Professions.cs#L56) · [AreaFunctions.cs:166](../../Assets/Scripts/Progression/AreaFunctions.cs#L166)*
- Comparte la dirección de proyectos con Julian: dos misiones, y cada uno coge la que menos ha hecho. *recreada · [personajes-mesocosmos.md §Maestros de baile](../personajes-mesocosmos.md) · [Professions.cs:43](../../Assets/Scripts/Progression/Professions.cs#L43)*
- Las cuentas (`coc_cuentas`) las comparten también Panterilia y Ducristin. *código · [Professions.cs:42](../../Assets/Scripts/Progression/Professions.cs#L42) · [Professions.cs:59](../../Assets/Scripts/Progression/Professions.cs#L59)*
- Sale en Construcción. *código · [SanctuaryDirector.cs:172](../../Assets/Scripts/World/SanctuaryDirector.cs#L172) · [SampleSceneBuilder.Construction.cs:174](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L174)*

## 7. Relaciones

- Pareja de Julian (búlgaro); se conocieron en un máster; comparten casa. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Parejas](../personajes-mesocosmos.md) · [ResidentialBuilder.cs:41](../../Assets/Editor/ResidentialBuilder.cs#L41)*
- Familia: **Falta**.

## 8. Horario y costumbres

- Gimnasio: empuje/tirón/pierna de hipertrofia, martes, jueves y sábado. *recreada · [GymProgramming.cs:213](../../Assets/Scripts/Progression/GymProgramming.cs#L213) · [gimnasio-maquinas.md](../gimnasio-maquinas.md) §15*
- Horario libre: el reparto común. *código · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*
- Costumbres propias: **Falta**.

## 9. Microcosmos y santuario

- En los microcosmos: una especie variada por su nombre (no tiene una propia). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Datos (rótulo, ropa, stats) | [SampleSceneBuilder.Construction.cs:202](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L202) |
| Aspecto | [CubeHair.cs:200](../../Assets/Scripts/Avatar/CubeHair.cs#L200) |
| Profesión y base | [Professions.cs:56](../../Assets/Scripts/Progression/Professions.cs#L56) · [Professions.cs:129](../../Assets/Scripts/Progression/Professions.cs#L129) |
| Misiones | [AreaFunctions.cs:166](../../Assets/Scripts/Progression/AreaFunctions.cs#L166) |
| Gimnasio | [GymProgramming.cs:213](../../Assets/Scripts/Progression/GymProgramming.cs#L213) |
| Casa (con Julian) | [ResidentialBuilder.cs:22](../../Assets/Editor/ResidentialBuilder.cs#L22) · [ResidentialBuilder.cs:41](../../Assets/Editor/ResidentialBuilder.cs#L41) |

## 11. Preguntas para Berón

1. ¿De dónde es, qué máster hizo y qué le gusta? ¿Alguna historia suya?
2. La ficha de Irosene ([character-irosene.md §Relaciones](../character-irosene.md)) nombra a una «Alicia», hija de Irosene. ¿Es otra persona? (No lo damos por hecho.)
3. La razón de su plan de gimnasio dice «Huerto (cargar cajas)», del reparto provisional ([GymProgramming.cs:213](../../Assets/Scripts/Progression/GymProgramming.cs#L213)). ¿La cambiamos?
