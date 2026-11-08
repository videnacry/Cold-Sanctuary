# Adaguchi

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Adaguchi`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real, de la familia o los amigos de Berón; Berón mandó su foto y confirmó que se queda. *documentada · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md)*
- Piel clara morena; pelo corto ondulado castaño oscuro con volumen arriba; un pendiente pequeño; sudadera negra sin mangas con capucha; collar de cuentas oscuras. *atestiguada (`adaguchi.png`) · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md) · [CubeHair.cs:216-218](../../Assets/Scripts/Avatar/CubeHair.cs#L216)*
- 1,65 m. *inventada (estatura estimada) · ídem*
- Edad, origen y género: **Falta**.

## 2. Origen e historia

- **Falta**.

## 3. Personalidad y frases

- **Falta**. No tiene frases, vivencias ni anclas en el código.

## 4. Stats, margas y aptitudes

- Fuerza 1,05 · agilidad 1,0 · resistencia 1,0 · masa 1,05. *inventada · [SampleSceneBuilder.Construction.cs:207](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L207)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Profesional de **submarinismo** (submarino de obra, ambulancia, tareas bajo el agua) y de **cocina**. *documentada (Berón 2026-11-04) · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [Professions.cs:130](../../Assets/Scripts/Progression/Professions.cs#L130)*
- Que su profesión cubra **toda el área de Construcción** (porque los submarinos salen de allí). *inventada (deducido, sin confirmar) · [Professions.cs:156](../../Assets/Scripts/Progression/Professions.cs#L156) · [reglas-personajes.md §Auditoría de puestos](../reglas-personajes.md)*

## 6. Puesto y áreas

- Construcción y Cocina. *recreada · [Professions.cs:55](../../Assets/Scripts/Progression/Professions.cs#L55)*
- Sale en Construcción. *recreada · [SanctuaryDirector.cs:171](../../Assets/Scripts/World/SanctuaryDirector.cs#L171) · [SampleSceneBuilder.Construction.cs:175](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L175)*
- En la prueba del restaurante trabaja en el obrador. *recreada · [personajes-mesocosmos.md §La jornada (regla W9)](../personajes-mesocosmos.md)*

## 7. Relaciones

- Tiene casa propia en la arboleda. *recreada · [ResidentialBuilder.cs:22](../../Assets/Editor/ResidentialBuilder.cs#L22)*
- Familia, pareja y amigos: **Falta**.

## 8. Horario y costumbres

- Gimnasio: Weider, lunes a viernes, hipertrofia, «estilo culturista» por la sudadera sin mangas de su foto. *inventada (de su ropa) · [GymProgramming.cs:218](../../Assets/Scripts/Progression/GymProgramming.cs#L218) · [gimnasio-maquinas.md §15.6](../gimnasio-maquinas.md)*
- Horario libre: el reparto común. *recreada · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*
- Costumbres propias: **Falta**.

## 9. Microcosmos y santuario

- Insecto: ninguno propio («cualquier otro»). *recreada · [microcosmos-civilizaciones.md §5](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo y stats | [SampleSceneBuilder.Construction.cs:207](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L207) |
| Aspecto | [CubeHair.cs:218](../../Assets/Scripts/Avatar/CubeHair.cs#L218) |
| Profesión y base | [Professions.cs:55](../../Assets/Scripts/Progression/Professions.cs#L55) · [Professions.cs:130](../../Assets/Scripts/Progression/Professions.cs#L130) · [Professions.cs:156](../../Assets/Scripts/Progression/Professions.cs#L156) |
| Personal | [WorkdayDirector.cs:20](../../Assets/Scripts/Progression/WorkdayDirector.cs#L20) |
| Área de salida | [SanctuaryDirector.cs:171](../../Assets/Scripts/World/SanctuaryDirector.cs#L171) · [SampleSceneBuilder.Construction.cs:175](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L175) |
| Gimnasio | [GymProgramming.cs:218](../../Assets/Scripts/Progression/GymProgramming.cs#L218) |
| Casa | [ResidentialBuilder.cs:22](../../Assets/Editor/ResidentialBuilder.cs#L22) |

## 11. Preguntas para Berón

1. ¿Su profesión es solo lo de los submarinos, o toda Construcción?
2. ¿Hoy conduce él los submarinos? El código no le da ninguna misión propia del submarino de obra ni de la ambulancia.
3. ¿Algo de su historia, su carácter o su familia?
