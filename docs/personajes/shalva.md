# Shalva

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Shalva`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real, de la familia o los amigos de Berón. *documentada · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md)*
- Pelo corto castaño; jersey grueso de canalé verde oliva y vaqueros azul claro. *atestiguada, pero de una foto de tres sin confirmar (`cumbaya.jpg`, a la derecha, **supuesto**) · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md) · [CubeHair.cs:215](../../Assets/Scripts/Avatar/CubeHair.cs#L215)*
- 1,72 m. *inventada (estatura estimada) · ídem*
- Edad, origen y género: **Falta** (no se suponen, J13).

## 2. Origen e historia

- **Falta**.

## 3. Personalidad y frases

- **Falta**. No tiene frases, vivencias ni anclas en el código.

## 4. Stats, margas y aptitudes

- Fuerza 1,05 · agilidad 1,0 · resistencia 1,05 · masa 1,0 («stats cercanas a 1», persona real). *inventada · [SampleSceneBuilder.Construction.cs:206](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L206)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Profesional de **cultivo** y **bioconstrucción**. *documentada (Berón 2026-11-04) · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [Professions.cs:126](../../Assets/Scripts/Progression/Professions.cs#L126)*
- **Jardinero frutal**: cada día una ronda de frutales (`jar_frutales`), heredada de Irosene. *inventada (deducido de «cultivo», sin confirmar) · [Professions.cs:151](../../Assets/Scripts/Progression/Professions.cs#L151) · [jardines-frutales.md §Ronda 7](../jardines-frutales.md) · [FruitGardener.cs:25](../../Assets/Scripts/Farming/FruitGardener.cs#L25)*
- Bioconstrucción en el juego: misiones de material (`mat_`). *recreada · [Professions.cs:51](../../Assets/Scripts/Progression/Professions.cs#L51)*

## 6. Puesto y áreas

- Huerto (`jar_frutales`) y Construcción (`mat_`). *recreada · [Professions.cs:51](../../Assets/Scripts/Progression/Professions.cs#L51)*
- Sale en el huerto. *recreada · [SanctuaryDirector.cs:170](../../Assets/Scripts/World/SanctuaryDirector.cs#L170) · [SampleSceneBuilder.Construction.cs:173](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L173)*
- Su nombre sale en el cartel de la sala de cuidados de los frutales. *recreada · [SampleSceneBuilder.Orchards.cs:274](../../Assets/Editor/SampleSceneBuilder.Orchards.cs#L274) · [jardines-frutales.md §La sala de cuidados](../jardines-frutales.md)*

## 7. Relaciones

- Tiene casa propia en la arboleda. *recreada · [ResidentialBuilder.cs:22](../../Assets/Editor/ResidentialBuilder.cs#L22)*
- Familia, pareja y amigos: **Falta**.

## 8. Horario y costumbres

- Gimnasio: torso/pierna, martes, miércoles, viernes y sábado, hipertrofia («personal general»). *inventada · [GymProgramming.cs:217](../../Assets/Scripts/Progression/GymProgramming.cs#L217) · [gimnasio-maquinas.md §15.6](../gimnasio-maquinas.md)*
- Horario libre: el reparto común. *recreada · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*
- Costumbres propias: **Falta**.

## 9. Microcosmos y santuario

- Insecto en los microcosmos: ninguno propio («cualquier otro»: una especie por su nombre). *recreada · [microcosmos-civilizaciones.md §5](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo y stats | [SampleSceneBuilder.Construction.cs:206](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L206) |
| Aspecto | [CubeHair.cs:215](../../Assets/Scripts/Avatar/CubeHair.cs#L215) |
| Profesión y base | [Professions.cs:51](../../Assets/Scripts/Progression/Professions.cs#L51) · [Professions.cs:126](../../Assets/Scripts/Progression/Professions.cs#L126) · [Professions.cs:151](../../Assets/Scripts/Progression/Professions.cs#L151) |
| Personal | [WorkdayDirector.cs:20](../../Assets/Scripts/Progression/WorkdayDirector.cs#L20) |
| Área de salida | [SanctuaryDirector.cs:170](../../Assets/Scripts/World/SanctuaryDirector.cs#L170) · [SampleSceneBuilder.Construction.cs:173](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L173) |
| Jardinero frutal | [FruitGardener.cs:25](../../Assets/Scripts/Farming/FruitGardener.cs#L25) |
| Gimnasio | [GymProgramming.cs:217](../../Assets/Scripts/Progression/GymProgramming.cs#L217) |
| Casa | [ResidentialBuilder.cs:22](../../Assets/Editor/ResidentialBuilder.cs#L22) |

## 11. Preguntas para Berón

1. En `cumbaya.jpg`, ¿es Shalva el de la derecha?
2. ¿Es el jardinero de los frutales (`jar_frutales`)?
3. ¿Algo de su historia, su carácter o su familia?
