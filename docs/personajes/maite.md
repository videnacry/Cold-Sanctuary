# Maite

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Maite`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real, de la familia o los amigos de Berón (de la foto «cumbaya», con Josedrilo y Shalva). *documentada · Berón 2026-11-04 · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md)*
- Mujer: Berón la nombra en femenino (`Fem()`). *documentada · ídem*
- Melena larga ondulada castaña, piel clara; parka con capucha verde oliva parduzco sobre camiseta gris. *atestiguada ⚠️ (`cumbaya.jpg`, en el centro: **supuesto**) · [diseno-personajes.md §Pendiente de confirmar](../diseno-personajes.md) · [CubeHair.cs:212](../../Assets/Scripts/Avatar/CubeHair.cs#L212)*
- Estatura 1,65 m: estimada. *inventada · [diseno-personajes.md §Familia y amigos de Berón](../diseno-personajes.md)*
- Edad y nacionalidad: **Falta**.

## 2. Origen e historia

- Historia, origen y llegada al santuario: **Falta**.

## 3. Personalidad y frases

- Carácter y frases: **Falta**. Sin vivencias, anclas ni conducta propia.

## 4. Stats, margas y aptitudes

- Fuerza 0,9 · agilidad 1,05 · resistencia 1,0 · masa 0,95: «stats cercanas a 1». *inventada · [SampleSceneBuilder.Construction.cs:205](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L205)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Profesional de cocina; jefa de cocina con Amykhuz; curandera. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [Professions.cs:126](../../Assets/Scripts/Progression/Professions.cs#L126)*
- Curandera en el juego: remedios de plantas medicinales (`hos_plantas`: infusiones, cataplasmas y ungüentos). *recreada · [AreaFunctions.cs:175](../../Assets/Scripts/Progression/AreaFunctions.cs#L175)*
- Las curas del hospital (`enf_visita`): **deducido** de «curandera», sin confirmar. *inventada · [Professions.cs:152](../../Assets/Scripts/Progression/Professions.cs#L152)*
- En el reparto provisional llevó los registros y la vacunación del hospital (de Bill Gates). Ya no vale. *inventada · [personajes-mesocosmos.md §Reparto de papeles](../personajes-mesocosmos.md)*

## 6. Puesto y áreas

- Profesión en el código: Cocina y Hospital (`hos_plantas`, `enf_visita`). *código · [Professions.cs:47](../../Assets/Scripts/Progression/Professions.cs#L47)*
- Sale en la cocina. *código · [SanctuaryDirector.cs:170](../../Assets/Scripts/World/SanctuaryDirector.cs#L170) · [SampleSceneBuilder.Construction.cs:170](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L170)*
- En la brigada de la prueba del restaurante, en la línea. *código · [RestaurantServiceTest.cs:40](../../Assets/Scripts/Tests/RestaurantServiceTest.cs#L40)*

## 7. Relaciones

- Pareja de Josedrilo; comparten casa. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [ResidentialBuilder.cs:44](../../Assets/Editor/ResidentialBuilder.cs#L44)*
- Con Amykhuz, el otro jefe de cocina: los dos tienen hoy los rangos más altos de la cocina; Goluis es el encargado. *documentada · ídem · Berón 2026-11-08*
- Familia: **Falta**.

## 8. Horario y costumbres

- Gimnasio: cuerpo entero de resistencia, martes, jueves y sábado. *recreada · [GymProgramming.cs:216](../../Assets/Scripts/Progression/GymProgramming.cs#L216) · [gimnasio-maquinas.md](../gimnasio-maquinas.md) §15*
- Horario libre: el reparto común. *código · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*
- Costumbres propias: **Falta**.

## 9. Microcosmos y santuario

- En los microcosmos: una especie variada por su nombre (no tiene una propia). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Datos (rótulo, ropa, stats) | [SampleSceneBuilder.Construction.cs:205](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L205) |
| Aspecto | [CubeHair.cs:212](../../Assets/Scripts/Avatar/CubeHair.cs#L212) |
| Profesión, base y lo deducido | [Professions.cs:47](../../Assets/Scripts/Progression/Professions.cs#L47) · [Professions.cs:126](../../Assets/Scripts/Progression/Professions.cs#L126) · [Professions.cs:152](../../Assets/Scripts/Progression/Professions.cs#L152) |
| Misión de curandera | [AreaFunctions.cs:175](../../Assets/Scripts/Progression/AreaFunctions.cs#L175) |
| Gimnasio | [GymProgramming.cs:216](../../Assets/Scripts/Progression/GymProgramming.cs#L216) |
| Casa (con Josedrilo) | [ResidentialBuilder.cs:44](../../Assets/Editor/ResidentialBuilder.cs#L44) |

## 11. Preguntas para Berón

1. ¿Es Maite la del centro en `cumbaya.jpg`?
2. ¿Hace curas en el hospital, o solo plantas medicinales? ¿Qué tradición de curandera sigue?
3. La razón de su plan de gimnasio dice «Registros y vacunación (mucho de pie)», del reparto provisional ([GymProgramming.cs:216](../../Assets/Scripts/Progression/GymProgramming.cs#L216)). ¿La cambiamos?
4. ¿De dónde es y qué le gusta cocinar?
