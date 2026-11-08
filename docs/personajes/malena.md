# Malena

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Malena`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real; llegó el 2026-11-05, después de los seis nuevos. *documentada · [personajes-mesocosmos.md §Lale, Yulo, Malena y las fotos](../personajes-mesocosmos.md)*
- Rasgo femenino (`Fem()`), confirmado por su foto. *atestiguada · [personajes-mesocosmos.md §Lale, Yulo, Malena y las fotos](../personajes-mesocosmos.md)*
- Pelo recogido con gafas de sol sobre la cabeza, abrigo largo beige de botones grandes, blusa blanca de cuello alto y pantalón blanco. *atestiguada (foto `ivelyna_malena`, a la derecha; orden confirmado por Berón el 2026-11-05) · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md) · [CubeHair.cs:234](../../Assets/Scripts/Avatar/CubeHair.cs#L234)*
- 1,63 m. *inventada (estatura estimada) · ídem*
- Edad y origen: **Falta**.

## 2. Origen e historia

- **Falta**.

## 3. Personalidad y frases

- **Falta**.

## 4. Stats, margas y aptitudes

- Fuerza, agilidad, resistencia y masa 1,0. *inventada · [SampleSceneBuilder.Construction.cs:218](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L218)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- **Pediatra**. *documentada (Berón 2026-11-05) · [Professions.cs:142](../../Assets/Scripts/Progression/Professions.cs#L142)*
- En el juego: `hos_pediatria` (consulta, vacunas y crecimiento de los niños) y las misiones `enf_` del hospital. *recreada · [Professions.cs:76](../../Assets/Scripts/Progression/Professions.cs#L76) · [AreaFunctions.cs:178](../../Assets/Scripts/Progression/AreaFunctions.cs#L178)*
- **No está en `Professions.Medical`** (Lizmibet, Linduarda e Ivelyna), aunque [reglas-personajes.md §Auditoría de puestos](../reglas-personajes.md) cuenta cuatro médicas con ella. El submarino-ambulancia no la busca primero. *código · [Professions.cs:26](../../Assets/Scripts/Progression/Professions.cs#L26)*

## 6. Puesto y áreas

- Hospital, no la Cría (la Cría es de crías de animales). *recreada · [personajes-mesocosmos.md §Los seis nuevos](../personajes-mesocosmos.md)*
- Sale en la enfermería. *recreada · [SanctuaryDirector.cs:176](../../Assets/Scripts/World/SanctuaryDirector.cs#L176) · [SampleSceneBuilder.Construction.cs:186](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L186)*
- Guardería: no existe aún. Berón (2026-11-04) la pidió para Ivelyna; para Malena no lo ha dicho: **Falta**. *documentada (memoria de Berón, character-roles) · [personajes-mesocosmos.md §Neo](../personajes-mesocosmos.md)*
- En el santuario solo hay un niño, Neo; nadie le asigna la pediatría a él en concreto. *código · [personajes-mesocosmos.md §Neo](../personajes-mesocosmos.md)*

## 7. Relaciones

- Tiene casa propia en la arboleda (cuarta tanda). *recreada · [ResidentialBuilder.cs:32](../../Assets/Editor/ResidentialBuilder.cs#L32)*
- Comparte foto y profesión con Ivelyna. *atestiguada · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md)*. Qué relación tienen: **Falta**.
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
| Cuerpo y stats | [SampleSceneBuilder.Construction.cs:218](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L218) |
| Aspecto | [CubeHair.cs:234](../../Assets/Scripts/Avatar/CubeHair.cs#L234) |
| Profesión y base | [Professions.cs:76](../../Assets/Scripts/Progression/Professions.cs#L76) · [Professions.cs:142](../../Assets/Scripts/Progression/Professions.cs#L142) |
| Personal sanitario | [Professions.cs:26](../../Assets/Scripts/Progression/Professions.cs#L26) |
| Personal | [WorkdayDirector.cs:22](../../Assets/Scripts/Progression/WorkdayDirector.cs#L22) |
| Área de salida | [SanctuaryDirector.cs:176](../../Assets/Scripts/World/SanctuaryDirector.cs#L176) |
| Casa | [ResidentialBuilder.cs:32](../../Assets/Editor/ResidentialBuilder.cs#L32) |

## 11. Preguntas para Berón

1. ¿Va también a la guardería cuando exista?
2. ¿La añadimos al personal sanitario (`Professions.Medical`), como a Ivelyna?
3. ¿Algo de su historia, su carácter o su familia?
