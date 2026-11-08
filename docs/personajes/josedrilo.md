# Josedrilo

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Josedrilo` (`WorkdayDirector.YogaTeacher`). Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real, de la familia o los amigos de Berón (de la foto «cumbaya», con Maite y Shalva). *documentada · Berón 2026-11-04 · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md)*
- Hombre. Melena larga castaña oscura recogida atrás; chaqueta de cuero negra; pantalón granate. *atestiguada ⚠️ (`cumbaya.jpg`, a la izquierda: **supuesto**, sin confirmar quién es quién) · [diseno-personajes.md §Pendiente de confirmar](../diseno-personajes.md) · [CubeHair.cs:209](../../Assets/Scripts/Avatar/CubeHair.cs#L209)*
- Estatura 1,75 m: estimada. *inventada · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md)*
- Edad y nacionalidad: **Falta**.

## 2. Origen e historia

- La ficha de Irosene lo nombra padre de Leo y exesposo de Alicia, hija de Irosene; también le atribuye otro hijo (ver la ficha). *documentada ⚠️ (árbol familiar «a confirmar») · [character-irosene.md §Relaciones](../character-irosene.md)*
- Cómo y cuándo llegó al santuario: **Falta**.

## 3. Personalidad y frases

- Carácter y frases: **Falta**. Sin vivencias, anclas ni conducta propia.

## 4. Stats, margas y aptitudes

- Fuerza 1,05 · agilidad 1,0 · resistencia 1,0 · masa 1,0: «stats cercanas a 1». *inventada · [SampleSceneBuilder.Construction.cs:204](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L204)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- «El profesional de yoga es Josedrilo»; también profesional de bioconstrucción y jefe de obra nueva. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [Professions.cs:116](../../Assets/Scripts/Progression/Professions.cs#L116)*
- Desde el 2026-11-05, jefe de Hatha; Consol, jefa de Ashtanga. Berón: Consol es «candidata a ser superior a Josedrilo». Decisión nuestra: cada uno jefe de su estilo (P6). *documentada (Berón) + recreada (la decisión) · [personajes-mesocosmos.md §Consol y Fernando](../personajes-mesocosmos.md) · [reglas-personajes.md §Auditoría](../reglas-personajes.md)*
- En el reparto provisional fue guardia, con Gohageneis. Ya no vale. *inventada · [personajes-mesocosmos.md §Reparto de papeles](../personajes-mesocosmos.md)*
- Qué estilo de yoga practica y desde cuándo: **Falta**.

## 6. Puesto y áreas

- Profesión en el código: Sala de Yoga (guía la clase de Hatha, `yog_guia`) y Construcción (`obra_` las obras, `mat_` el material). Dirige las obras de grupo. *código · [Professions.cs:31](../../Assets/Scripts/Progression/Professions.cs#L31) · [personajes-mesocosmos.md §La jornada](../personajes-mesocosmos.md)*
- Sale en la tarima de la sala de Hatha tradicional, junto al linga. *código · [SampleSceneBuilder.Construction.cs:169](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L169) · [SanctuaryDirector.cs:170](../../Assets/Scripts/World/SanctuaryDirector.cs#L170)*
- Es `YogaTeacher`: el maestro que eligen las pruebas de yoga. *código · [WorkdayDirector.cs:777](../../Assets/Scripts/Progression/WorkdayDirector.cs#L777) · [yoga-pisos.md](../yoga-pisos.md)*

## 7. Relaciones

- Pareja de Maite; comparten casa. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [ResidentialBuilder.cs:44](../../Assets/Editor/ResidentialBuilder.cs#L44)*
- Padre de Leo, según la ficha de Irosene. *documentada ⚠️ · [character-irosene.md §Relaciones](../character-irosene.md)*
- Con Consol, la otra jefa de yoga. *documentada · [personajes-mesocosmos.md §Consol y Fernando](../personajes-mesocosmos.md)*

## 8. Horario y costumbres

- En su tiempo libre medita en la tarima de Hatha, junto al linga. *recreada (heredado de Sadhguru) · [WorkdayDirector.cs:635](../../Assets/Scripts/Progression/WorkdayDirector.cs#L635) · [yoga-pisos.md](../yoga-pisos.md)*
- Gimnasio: empuje/tirón/pierna de hipertrofia martes, jueves y sábado; los demás días, el yoga. *recreada · [GymProgramming.cs:215](../../Assets/Scripts/Progression/GymProgramming.cs#L215) · [gimnasio-maquinas.md](../gimnasio-maquinas.md) §15*
- Horario libre: el reparto común. *código · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*

## 9. Microcosmos y santuario

- En los microcosmos: una especie variada por su nombre (no tiene una propia). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Datos (rótulo, ropa, stats) | [SampleSceneBuilder.Construction.cs:204](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L204) |
| Maestro de Hatha | [WorkdayDirector.cs:777](../../Assets/Scripts/Progression/WorkdayDirector.cs#L777) |
| Aspecto | [CubeHair.cs:209](../../Assets/Scripts/Avatar/CubeHair.cs#L209) |
| Profesión y base | [Professions.cs:31](../../Assets/Scripts/Progression/Professions.cs#L31) · [Professions.cs:116](../../Assets/Scripts/Progression/Professions.cs#L116) |
| Gimnasio | [GymProgramming.cs:215](../../Assets/Scripts/Progression/GymProgramming.cs#L215) |
| Casa (con Maite) | [ResidentialBuilder.cs:22](../../Assets/Editor/ResidentialBuilder.cs#L22) · [ResidentialBuilder.cs:44](../../Assets/Editor/ResidentialBuilder.cs#L44) |

## 11. Preguntas para Berón

1. ¿Es Josedrilo el de la izquierda en `cumbaya.jpg`?
2. ¿El Josedrilo de la ficha de Irosene (padre de Leo) es este? ¿Qué parte de esa historia puede ir al juego?
3. ¿Quién dirige el yoga en conjunto, él o Consol?
4. ¿De dónde es, qué yoga practica y qué le gusta?
