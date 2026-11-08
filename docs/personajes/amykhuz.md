# Amykhuz (antes Amahuz)

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Amykhuz` (nombre viejo `Amahuz` en `LegacyIds`). Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real, de la familia o los amigos de Berón. *documentada · Berón 2026-11-04 · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md)*
- Se llama **Amykhuz**; antes lo escribíamos «Amahuz». *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md)*
- Hombre. Delgado, piel clara morena, pelo castaño oscuro, bigote fino; polo azul marino de manga corta, pantalón oscuro y bolso negro cruzado. *atestiguada (`amahuz_karen.jpg`, a la derecha) · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md) · [CubeHair.cs:194](../../Assets/Scripts/Avatar/CubeHair.cs#L194)*
- Estatura 1,76 m: estimada. *inventada · ídem*
- Edad y nacionalidad: **Falta**.

## 2. Origen e historia

- Historia, origen y llegada al santuario: **Falta**.

## 3. Personalidad y frases

- Carácter y frases: **Falta**. Sin vivencias, anclas ni conducta propia en el código.

## 4. Stats, margas y aptitudes

- Fuerza 1,0 · agilidad 1,05 · resistencia 1,0 · masa 0,9 («delgado», de la foto; el resto, cerca de 1). *recreada · [SampleSceneBuilder.Construction.cs:200](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L200)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Profesional de cocina y química; jefe de cocina con Maite; hoy los dos tienen los rangos más altos de la cocina y Goluis es el encargado (Berón 2026-11-08). *documentada · Berón 2026-11-04 y 2026-11-08 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [Professions.cs:125](../../Assets/Scripts/Progression/Professions.cs#L125)*

## 6. Puesto y áreas

- Profesión en el código: Cocina y Química. *código · [Professions.cs:48](../../Assets/Scripts/Progression/Professions.cs#L48) · [AreaFunctions.cs:136](../../Assets/Scripts/Progression/AreaFunctions.cs#L136)*
- Sale en la cocina del restaurante. *código · [SanctuaryDirector.cs:171](../../Assets/Scripts/World/SanctuaryDirector.cs#L171) · [SampleSceneBuilder.Construction.cs:171](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L171)*
- En la brigada de la prueba del restaurante, en la línea con Goluis y Maite. *código · [RestaurantServiceTest.cs:40](../../Assets/Scripts/Tests/RestaurantServiceTest.cs#L40) · [personajes-mesocosmos.md §La jornada](../personajes-mesocosmos.md)*

## 7. Relaciones

- Pareja de Karen; comparten casa y cama (confirmado por Berón). *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Parejas](../personajes-mesocosmos.md) · [ResidentialBuilder.cs:40](../../Assets/Editor/ResidentialBuilder.cs#L40) · [guerra-de-almohadas.md](../guerra-de-almohadas.md)*
- Con Maite, la otra jefa de cocina. *documentada · ídem*
- Familia: **Falta**.

## 8. Horario y costumbres

- Gimnasio: torso/pierna de hipertrofia, lunes, martes, jueves y viernes («delgado: ganar masa»). *recreada · [GymProgramming.cs:211](../../Assets/Scripts/Progression/GymProgramming.cs#L211) · [gimnasio-maquinas.md](../gimnasio-maquinas.md) §15*
- Horario libre: el reparto común. *código · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*
- Costumbres propias: **Falta**.

## 9. Microcosmos y santuario

- En los microcosmos: una especie variada por su nombre (no tiene una propia). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Datos (rótulo, ropa, stats) | [SampleSceneBuilder.Construction.cs:200](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L200) |
| Nombre viejo | [WorkdayDirector.cs:79](../../Assets/Scripts/Progression/WorkdayDirector.cs#L79) (`LegacyIds`) |
| Aspecto | [CubeHair.cs:194](../../Assets/Scripts/Avatar/CubeHair.cs#L194) |
| Profesión y base | [Professions.cs:48](../../Assets/Scripts/Progression/Professions.cs#L48) · [Professions.cs:125](../../Assets/Scripts/Progression/Professions.cs#L125) |
| Gimnasio | [GymProgramming.cs:211](../../Assets/Scripts/Progression/GymProgramming.cs#L211) |
| Casa y pareja | [ResidentialBuilder.cs:22](../../Assets/Editor/ResidentialBuilder.cs#L22) · [ResidentialBuilder.cs:40](../../Assets/Editor/ResidentialBuilder.cs#L40) |

## 11. Preguntas para Berón

1. ¿Quién es, de dónde es y qué le gusta? ¿Alguna historia suya?
2. ¿Qué química hace (laboratorio, cocina, otra)?
3. ~~¿Quién manda en la cocina?~~ Contestado (Berón 2026-11-08): Maite y él tienen los rangos más altos; Goluis es encargado.
4. El doc del gimnasio aún lo llama «Amahuz» ([gimnasio-maquinas.md](../gimnasio-maquinas.md) §15, tabla de planes).
