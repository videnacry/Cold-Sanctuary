# Neo

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Neo`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = sin confirmar. **Falta** = no se sabe.
> Persona real (familia o amigos de Berón): solo rasgos visibles, sin suponer edad (J13).

## 1. Identidad y aspecto

- Niño, hijo de Ducristin y Gallarle (Berón 2026-11-04). *documentada · [personajes-mesocosmos.md §Neo](../personajes-mesocosmos.md) · nota de Berón 2026-11-04*
- Pelo castaño oscuro, camiseta roja bajo un chaleco beige; sin `Fem()`. *atestiguada (foto `neo`) · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md) · [CubeHair.cs:226](../../Assets/Scripts/Avatar/CubeHair.cs#L226)*
- ~1,2 m; su cuerpo crece con `PersonLife`. *inventada (estimada) · [CubeHair.cs:225](../../Assets/Scripts/Avatar/CubeHair.cs#L225)*
- Edad: **Falta**.

## 2. Origen e historia

- **Falta**: no hay historia suya.

## 3. Personalidad y frases

- **Falta**.

## 4. Stats, margas y aptitudes

- Cuerpo: fuerza 0,5 · agilidad 1,1 · resistencia 0,8 · masa 0,45. *inventada · [SampleSceneBuilder.cs:3067](../../Assets/Editor/SampleSceneBuilder.cs#L3067)*
- Ciclo de vida humano en etapa de niño (`BodyAging.growth` 0,35). *recreada · [SampleSceneBuilder.cs:3069-3070](../../Assets/Editor/SampleSceneBuilder.cs#L3069)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Ninguna: es un niño. *documentada · [personajes-mesocosmos.md §Neo](../personajes-mesocosmos.md)*

## 6. Puesto y áreas

- No está en la jornada (`WorkdayDirector.Staff`) ni tiene plan de gimnasio. *recreada · [personajes-mesocosmos.md §Neo](../personajes-mesocosmos.md)*
- Sale en el Aula, con su madre. *recreada · [SampleSceneBuilder.Construction.cs:191](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L191)*
- Guardería y juego de niños: aún no existen; con Ivelyna (pediatra), irá allí. *recreada · [personajes-mesocosmos.md §Neo](../personajes-mesocosmos.md)*

## 7. Relaciones

- Padre: Ducristin; madre: Gallarle (enlazados en `PersonLife`). *documentada · [SampleSceneBuilder.cs:3071-3072](../../Assets/Editor/SampleSceneBuilder.cs#L3071)*
- Vive en casa de sus padres (`Bedroom.kids`). *documentada · [ResidentialBuilder.cs:49](../../Assets/Editor/ResidentialBuilder.cs#L49)*

## 8. Horario y costumbres

- De niño sigue a su madre (`FollowMother`). *recreada ⚠️ (el builder dice «pendiente de confirmar con Berón») · [PersonLife.cs:130](../../Assets/Scripts/Body/PersonLife.cs#L130) · [SampleSceneBuilder.cs:3072](../../Assets/Editor/SampleSceneBuilder.cs#L3072)*
- Cuenta en el aforo del ring de almohadas y en los residentes. *recreada · [guerra-de-almohadas.md](../guerra-de-almohadas.md)*

## 9. Microcosmos y santuario

- Es una de las personas del mesocosmos con ficha. *código · [Professions.cs:105](../../Assets/Scripts/Progression/Professions.cs#L105)*
- Su insecto en los microcosmos: el de cualquiera, por su nombre. **Falta** uno propio.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo y ciclo de vida | [SampleSceneBuilder.cs:3067](../../Assets/Editor/SampleSceneBuilder.cs#L3067) |
| Aspecto | [CubeHair.cs:226](../../Assets/Scripts/Avatar/CubeHair.cs#L226) |
| Casa | [ResidentialBuilder.cs:49](../../Assets/Editor/ResidentialBuilder.cs#L49) |
| Seguir a su madre | [PersonLife.cs:130](../../Assets/Scripts/Body/PersonLife.cs#L130) |

## 11. Preguntas para Berón

1. ¿Qué edad tiene, o qué etapa (bebé, niño, adolescente)?
2. De niño, ¿va con su madre o con su padre?
3. ¿Qué le gusta hacer? Sin eso no tiene horario libre propio.
4. ¿Quieres una guardería o un juego de niños para él?
