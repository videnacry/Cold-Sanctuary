# Fausto

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Fausto`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real. *documentada · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md)*
- **Pendiente de foto.** Aspecto provisional del código: 1,70 m, pelo rizado castaño, ropa de taller gris verdosa. *inventada · [CubeHair.cs:237](../../Assets/Scripts/Avatar/CubeHair.cs#L237)*. El doc de diseño dice 1,72 m y «ropa de taller gris»: [diseno-personajes.md §Familia y amigos](../diseno-personajes.md).
- Edad y origen: **Falta**.

## 2. Origen e historia

- **Falta**.

## 3. Personalidad y frases

- **Falta**.

## 4. Stats, margas y aptitudes

- Fuerza 1,05 · agilidad 1,0 · resistencia 1,0 · masa 1,0. *inventada · [SampleSceneBuilder.Construction.cs:210](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L210)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- **Mecánico**. *documentada (Berón 2026-11-04: «Irosene y Fausto son pareja (Fausto es mecánico)») · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [Professions.cs:134](../../Assets/Scripts/Progression/Professions.cs#L134)*
- Lleva el área Mecánica entera, que antes llevaba Ducristin. *recreada · [Professions.cs:62](../../Assets/Scripts/Progression/Professions.cs#L62)*

## 6. Puesto y áreas

- Mecánica (el taller de vehículos); sale allí. *recreada · [SanctuaryDirector.cs:172](../../Assets/Scripts/World/SanctuaryDirector.cs#L172) · [SampleSceneBuilder.Construction.cs:178](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L178)*

## 7. Relaciones

- Pareja de **Irosene**; comparten casa del árbol. *documentada (Berón 2026-11-04) · [personajes-mesocosmos.md §Parejas](../personajes-mesocosmos.md) · [ResidentialBuilder.cs:42](../../Assets/Editor/ResidentialBuilder.cs#L42) · [character-irosene.md §Lugar en el santuario](../character-irosene.md)*
- Familia y amigos: **Falta**. (La ficha de Irosene habla de un sobrino de su primer esposo que se volvería su pareja unos 20 años después, ⚠️; no dice que sea Fausto: ver §11.)

## 8. Horario y costumbres

- Gimnasio: **sin plan propio** (genérico). *código · [GymProgramming.cs:237](../../Assets/Scripts/Progression/GymProgramming.cs#L237)*. El plan de Ducristin aún dice «Mecánico (levantar piezas)» ([GymProgramming.cs:214](../../Assets/Scripts/Progression/GymProgramming.cs#L214)): es el texto viejo y le correspondería a Fausto.
- Horario libre: el reparto común. *recreada · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*

## 9. Microcosmos y santuario

- Insecto: ninguno propio. *recreada · [microcosmos-civilizaciones.md §5](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo y stats | [SampleSceneBuilder.Construction.cs:210](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L210) |
| Aspecto provisional | [CubeHair.cs:237](../../Assets/Scripts/Avatar/CubeHair.cs#L237) |
| Profesión y base | [Professions.cs:62](../../Assets/Scripts/Progression/Professions.cs#L62) · [Professions.cs:134](../../Assets/Scripts/Progression/Professions.cs#L134) |
| Personal | [WorkdayDirector.cs:21](../../Assets/Scripts/Progression/WorkdayDirector.cs#L21) |
| Área de salida | [SanctuaryDirector.cs:172](../../Assets/Scripts/World/SanctuaryDirector.cs#L172) |
| Casa | [ResidentialBuilder.cs:42](../../Assets/Editor/ResidentialBuilder.cs#L42) |

## 11. Preguntas para Berón

1. Su foto.
2. ¿Es Fausto el sobrino de Néstor del que habla la ficha de Irosene? (Solo si Berón quiere contarlo.)
3. ¿Un plan de gimnasio propio (el de «mecánico» lo tiene aún Ducristin)?
4. ¿Algo de su carácter o su historia?
