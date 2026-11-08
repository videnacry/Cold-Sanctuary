# Ducristin

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Ducristin` (retrato informal: `Ducristin_Casual`). Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real, de la familia o los amigos de Berón. Berón lo nombró «Ducristin (semigod)». *documentada · Berón 2026-11-04 · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md)*
- Grafía: Berón escribió una vez «Dukristin»; confirmó que la buena es «Ducristin». *documentada · Berón 2026-11-05 · [personajes-mesocosmos.md §Lale, Yulo, Malena y las fotos](../personajes-mesocosmos.md)*
- Hombre. De trabajo: pelo oscuro en tupé, traje azul marino, camisa blanca y corbata azul. *atestiguada (`dukristy.png`) · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md) · [CubeHair.cs:203](../../Assets/Scripts/Avatar/CubeHair.cs#L203)*
- Informal (solo en la galería): piel morena, pelo corto castaño oscuro levantado delante, gafas de aviador de cristal degradado, camisa azul clara de cuadros con el cuello abierto, muñequera roja. *atestiguada (`semigod.jpg`) · [CubeHair.cs:206](../../Assets/Scripts/Avatar/CubeHair.cs#L206) · [PoseGallery.cs:111](../../Assets/Editor/PoseGallery.cs#L111)*
- Estatura 1,70 m: estimada. *inventada · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md)*
- Edad y nacionalidad: **Falta**.

## 2. Origen e historia

- Historia, origen y llegada al santuario: **Falta**.

## 3. Personalidad y frases

- Carácter y frases: **Falta**. Su apodo en la foto, «semigod», es lo único. Sin vivencias, anclas ni conducta propia.

## 4. Stats, margas y aptitudes

- Fuerza 1,0 · agilidad 1,0 · resistencia 1,0 · masa 1,05: «stats cercanas a 1». *inventada · [SampleSceneBuilder.Construction.cs:203](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L203)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Gerente de banca y profesor. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [Professions.cs:130](../../Assets/Scripts/Progression/Professions.cs#L130)*
- En el reparto provisional llevó la mecánica (de Elon Musk). Hoy la lleva Fausto. *inventada · [personajes-mesocosmos.md §Reparto de papeles](../personajes-mesocosmos.md)*
- Qué enseña como profesor: **Falta**.

## 6. Puesto y áreas

- No hay banco: lleva las finanzas en la mesa de cuentas del restaurante (`coc_cuentas`) y da clase en el Aula (`aul_clase`). *recreada · [Professions.cs:59](../../Assets/Scripts/Progression/Professions.cs#L59) · [personajes-mesocosmos.md §Por qué así](../personajes-mesocosmos.md)*
- Sale en el Aula. *código · [SanctuaryDirector.cs:172](../../Assets/Scripts/World/SanctuaryDirector.cs#L172) · [SampleSceneBuilder.Construction.cs:177](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L177)*
- Viste siempre su ropa de trabajo en el juego: el cuerpo se viste una vez al construirse. *código · [personajes-mesocosmos.md §Lale, Yulo, Malena y las fotos](../personajes-mesocosmos.md)*

## 7. Relaciones

- Pareja de Gallarle; comparten casa. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [ResidentialBuilder.cs:43](../../Assets/Editor/ResidentialBuilder.cs#L43)*
- Padre de Neo, que vive con ellos; en el ciclo de vida, `father` de Neo. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Neo](../personajes-mesocosmos.md) · [ResidentialBuilder.cs:49](../../Assets/Editor/ResidentialBuilder.cs#L49) · [SampleSceneBuilder.cs:3068](../../Assets/Editor/SampleSceneBuilder.cs#L3068)*
- Familia más allá: **Falta**.

## 8. Horario y costumbres

- Gimnasio: torso/pierna de fuerza, lunes, miércoles, jueves y sábado. *recreada · [GymProgramming.cs:214](../../Assets/Scripts/Progression/GymProgramming.cs#L214) · [gimnasio-maquinas.md](../gimnasio-maquinas.md) §15*
- Horario libre: el reparto común. Vive en una torre lejana y tardaba en llegar al ring. *código · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87) · [guerra-de-almohadas.md](../guerra-de-almohadas.md)*
- Costumbres propias: **Falta**.

## 9. Microcosmos y santuario

- En los microcosmos: una especie variada por su nombre (no tiene una propia). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Datos (rótulo, ropa, stats) | [SampleSceneBuilder.Construction.cs:203](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L203) |
| Aspecto (trabajo e informal) | [CubeHair.cs:203](../../Assets/Scripts/Avatar/CubeHair.cs#L203) · [CubeHair.cs:206](../../Assets/Scripts/Avatar/CubeHair.cs#L206) |
| Profesión y base | [Professions.cs:59](../../Assets/Scripts/Progression/Professions.cs#L59) · [Professions.cs:130](../../Assets/Scripts/Progression/Professions.cs#L130) |
| Gimnasio | [GymProgramming.cs:214](../../Assets/Scripts/Progression/GymProgramming.cs#L214) |
| Casa, pareja e hijo | [ResidentialBuilder.cs:22](../../Assets/Editor/ResidentialBuilder.cs#L22) · [ResidentialBuilder.cs:43](../../Assets/Editor/ResidentialBuilder.cs#L43) · [ResidentialBuilder.cs:49](../../Assets/Editor/ResidentialBuilder.cs#L49) |

## 11. Preguntas para Berón

1. ¿Quién es, de dónde es y qué le gusta? ¿Por qué «semigod»?
2. ¿Qué enseña como profesor?
3. La razón de su plan de gimnasio dice «Mecánico (levantar piezas)» ([GymProgramming.cs:214](../../Assets/Scripts/Progression/GymProgramming.cs#L214)) y un comentario de `HomeArea` dice «Ducristin, la mecánica» ([SanctuaryDirector.cs:167](../../Assets/Scripts/World/SanctuaryDirector.cs#L167)): son del reparto provisional. ¿Los cambiamos?
4. ¿Debe cambiarse a su ropa informal en su tiempo libre?
