# Karen

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Karen`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real, de la familia o los amigos de Berón. *documentada · Berón 2026-11-04 · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md)*
- Mujer: Berón la nombra en femenino (`Fem()`). *documentada · ídem*
- Piel morena cálida; melena larga castaña oscura con una pinza de flor rosa y amarilla; pendientes pequeños; blusa blanca sin mangas con volantes; falda berenjena con flores rosas y lilas; reloj negro; sonrisa amplia. *atestiguada (`amahuz_karen.jpg`, a la izquierda) · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md) · [CubeHair.cs:197](../../Assets/Scripts/Avatar/CubeHair.cs#L197)*
- Estatura 1,58 m: estimada. *inventada · ídem*
- Edad y nacionalidad: **Falta**.

## 2. Origen e historia

- Historia, origen y llegada al santuario: **Falta**.

## 3. Personalidad y frases

- Carácter y frases: **Falta**. Sin vivencias, anclas ni conducta propia en el código.

## 4. Stats, margas y aptitudes

- Fuerza 0,9 · agilidad 1,05 · resistencia 1,0 · masa 0,95: «stats cercanas a 1». *inventada · [SampleSceneBuilder.Construction.cs:201](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L201)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Profesional de asuntos legales y lenguas. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [Professions.cs:128](../../Assets/Scripts/Progression/Professions.cs#L128)*
- Qué lenguas habla: **Falta**.

## 6. Puesto y áreas

- En el Aula, porque no hay área de asuntos legales: `aul_legal` (contratos, permisos, normas, en una mesa de la sala de estudio) y `aul_lenguas` (traducir y dar clase de idiomas). *recreada · [Professions.cs:55](../../Assets/Scripts/Progression/Professions.cs#L55) · [AreaFunctions.cs:164](../../Assets/Scripts/Progression/AreaFunctions.cs#L164) · [personajes-mesocosmos.md §Por qué así](../personajes-mesocosmos.md)*
- Sale en el Aula. *código · [SanctuaryDirector.cs:171](../../Assets/Scripts/World/SanctuaryDirector.cs#L171) · [SampleSceneBuilder.Construction.cs:176](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L176)*

## 7. Relaciones

- Pareja de Amykhuz; comparten casa y cama (confirmado por Berón). *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Parejas](../personajes-mesocosmos.md) · [ResidentialBuilder.cs:40](../../Assets/Editor/ResidentialBuilder.cs#L40)*
- Familia: **Falta**.

## 8. Horario y costumbres

- Gimnasio: cuerpo entero de resistencia, lunes, miércoles y viernes («entrena con Amykhuz los lunes»). *recreada · [GymProgramming.cs:212](../../Assets/Scripts/Progression/GymProgramming.cs#L212) · [gimnasio-maquinas.md](../gimnasio-maquinas.md) §15*
- Horario libre: el reparto común. *código · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*
- Costumbres propias: **Falta**.

## 9. Microcosmos y santuario

- En los microcosmos: una especie variada por su nombre (no tiene una propia). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Datos (rótulo, ropa, stats) | [SampleSceneBuilder.Construction.cs:201](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L201) |
| Aspecto | [CubeHair.cs:197](../../Assets/Scripts/Avatar/CubeHair.cs#L197) |
| Profesión y base | [Professions.cs:55](../../Assets/Scripts/Progression/Professions.cs#L55) · [Professions.cs:128](../../Assets/Scripts/Progression/Professions.cs#L128) |
| Misiones | [AreaFunctions.cs:164](../../Assets/Scripts/Progression/AreaFunctions.cs#L164) |
| Gimnasio | [GymProgramming.cs:212](../../Assets/Scripts/Progression/GymProgramming.cs#L212) |
| Casa (con Amykhuz) | [ResidentialBuilder.cs:40](../../Assets/Editor/ResidentialBuilder.cs#L40) |

## 11. Preguntas para Berón

1. ¿Quién es, de dónde es y qué le gusta? ¿Alguna historia suya?
2. ¿Qué lenguas habla? ¿Qué tipo de asuntos legales lleva?
3. La razón del gimnasio dice «entrena con Amykhuz los lunes»: es nuestra; sus planes coinciden lunes y viernes. ¿Entrenan juntos?
