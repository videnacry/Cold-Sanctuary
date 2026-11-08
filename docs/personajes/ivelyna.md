# Ivelyna

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Ivelyna`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real; llegó el 2026-11-04. *documentada · [personajes-mesocosmos.md §Los seis nuevos](../personajes-mesocosmos.md) · [§Lale, Yulo, Malena y las fotos](../personajes-mesocosmos.md)*
- Rasgo femenino (`Fem()`), confirmado por su foto. *atestiguada · [personajes-mesocosmos.md §Lale, Yulo, Malena y las fotos](../personajes-mesocosmos.md)*
- Pelo largo negro ondulado, camiseta blanca con un coche rojo estampado, falda blanca de volantes, pulsera de cuentas. *atestiguada (foto `ivelyna_malena`, a la izquierda; orden confirmado por Berón el 2026-11-05) · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md) · [CubeHair.cs:232](../../Assets/Scripts/Avatar/CubeHair.cs#L232)*
- 1,60 m. *inventada (estatura estimada) · ídem*
- Edad y origen: **Falta**.

## 2. Origen e historia

- **Falta**.

## 3. Personalidad y frases

- **Falta**.

## 4. Stats, margas y aptitudes

- Fuerza, agilidad, resistencia y masa 1,0. *inventada · [SampleSceneBuilder.Construction.cs:216](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L216)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- **Pediatra**. *documentada (Berón 2026-11-04) · [Professions.cs:138](../../Assets/Scripts/Progression/Professions.cs#L138)*
- En el juego: `hos_pediatria` (consulta, vacunas y crecimiento de los niños) y las misiones `enf_` del hospital. *recreada · [Professions.cs:73](../../Assets/Scripts/Progression/Professions.cs#L73) · [AreaFunctions.cs:178](../../Assets/Scripts/Progression/AreaFunctions.cs#L178)*
- Está en `Professions.Medical`: el submarino-ambulancia la busca de las primeras. *código · [Professions.cs:26](../../Assets/Scripts/Progression/Professions.cs#L26) · [submarinos-medicos.md](../submarinos-medicos.md)*

## 6. Puesto y áreas

- Hospital, no la Cría (la Cría es de crías de animales). *recreada · [personajes-mesocosmos.md §Los seis nuevos](../personajes-mesocosmos.md)*
- Sale en la enfermería. *recreada · [SanctuaryDirector.cs:176](../../Assets/Scripts/World/SanctuaryDirector.cs#L176) · [SampleSceneBuilder.Construction.cs:185](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L185)*
- Guardería: no existe aún. Berón (2026-11-04) quería a Ivelyna en el hospital y en la guardería. *documentada (memoria de Berón, character-roles) · [personajes-mesocosmos.md §Neo](../personajes-mesocosmos.md)*
- En el santuario solo hay un niño, Neo; nadie le asigna la pediatría a él en concreto. *código · [personajes-mesocosmos.md §Neo](../personajes-mesocosmos.md)*

## 7. Relaciones

- Tiene casa propia en la arboleda (cuarta tanda). *recreada · [ResidentialBuilder.cs:32](../../Assets/Editor/ResidentialBuilder.cs#L32)*
- Comparte foto y profesión con Malena. *atestiguada · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md)*. Qué relación tienen: **Falta**.
- Familia y pareja: **Falta**.

## 8. Horario y costumbres

- Gimnasio: **sin plan propio** (genérico: cuerpo entero 3 días de salud). *código · [GymProgramming.cs:237](../../Assets/Scripts/Progression/GymProgramming.cs#L237)*
- Horario libre: el reparto común. *recreada · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*

## 9. Microcosmos y santuario

- Insecto: ninguno propio (Linduarda y Lizmibet, las otras médicas, sí lo tienen). *recreada · [microcosmos-civilizaciones.md §5](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo y stats | [SampleSceneBuilder.Construction.cs:216](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L216) |
| Aspecto | [CubeHair.cs:232](../../Assets/Scripts/Avatar/CubeHair.cs#L232) |
| Profesión y base | [Professions.cs:73](../../Assets/Scripts/Progression/Professions.cs#L73) · [Professions.cs:138](../../Assets/Scripts/Progression/Professions.cs#L138) |
| Personal sanitario | [Professions.cs:26](../../Assets/Scripts/Progression/Professions.cs#L26) |
| Personal | [WorkdayDirector.cs:22](../../Assets/Scripts/Progression/WorkdayDirector.cs#L22) |
| Área de salida | [SanctuaryDirector.cs:176](../../Assets/Scripts/World/SanctuaryDirector.cs#L176) |
| Casa | [ResidentialBuilder.cs:32](../../Assets/Editor/ResidentialBuilder.cs#L32) |

## 11. Preguntas para Berón

1. ¿Va también a la guardería cuando exista?
2. ¿Atiende también a adultos, o solo a niños?
3. ¿Algo de su historia, su carácter o su familia?
