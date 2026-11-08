# Luciana

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Luciana`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real; llegó el 2026-11-04 con los «seis nuevos». *documentada · [personajes-mesocosmos.md §Los seis nuevos](../personajes-mesocosmos.md)*
- Rasgo femenino (`Fem()`), confirmado por su foto. *atestiguada · [personajes-mesocosmos.md §Lale, Yulo, Malena y las fotos](../personajes-mesocosmos.md)*
- Pelo largo liso oscuro, vestido hueso de mangas de farol. *atestiguada (foto `gallarle_luciana`, a la derecha; orden confirmado por Berón el 2026-11-05) · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md) · [CubeHair.cs:229-230](../../Assets/Scripts/Avatar/CubeHair.cs#L229)*
- 1,63 m. *inventada (estatura estimada) · ídem*
- Edad y origen: **Falta**.

## 2. Origen e historia

- **Falta**. (Dónde aprendió a pilotar y las artes marciales: **Falta**.)

## 3. Personalidad y frases

- **Falta**.

## 4. Stats, margas y aptitudes

- Fuerza, agilidad, resistencia y masa 1,0. *inventada · [SampleSceneBuilder.Construction.cs:217](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L217)*. **No reflejan** que sea experta en artes marciales (Consol, por «muy flexible», sí tiene agilidad 1,35).
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- **Piloto**: alfombras voladoras y transporte aéreo. *documentada (Berón 2026-11-04) · [Professions.cs:140](../../Assets/Scripts/Progression/Professions.cs#L140) · memoria de Berón, character-roles (2026-11-04 15:42)*
- **Experta en artes marciales**. *documentada (Berón 2026-11-05) · [Professions.cs:140](../../Assets/Scripts/Progression/Professions.cs#L140)*
- En el juego: `mec_alfombras` (la flota de alfombras, en la Mecánica). *recreada · [Professions.cs:77](../../Assets/Scripts/Progression/Professions.cs#L77) · [AreaFunctions.cs:179](../../Assets/Scripts/Progression/AreaFunctions.cs#L179)*
- **Control de misión** de la lanzadera (`lan_control`). *inventada (deducido de «piloto», sin confirmar) · [Professions.cs:149](../../Assets/Scripts/Progression/Professions.cs#L149) · [AreaFunctions.cs:180](../../Assets/Scripts/Progression/AreaFunctions.cs#L180)*

## 6. Puesto y áreas

- Mecánica y Control de misión; sale en la Mecánica. *recreada · [SanctuaryDirector.cs:176](../../Assets/Scripts/World/SanctuaryDirector.cs#L176) · [SampleSceneBuilder.Construction.cs:189](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L189)*
- Maestra de artes marciales, en días alternos con Gohageneis: da el entrenamiento del dojo cuando esté terminado y guía las artes marciales de las 20 h. *recreada · [Professions.cs:89-90](../../Assets/Scripts/Progression/Professions.cs#L89) · [LeisureDirector.cs:178](../../Assets/Scripts/Progression/LeisureDirector.cs#L178) · [horario-libre.md §Catálogo y sitios](../horario-libre.md)*
- Por la regla P6, como experta documentada, podría dirigir ella las artes marciales; hoy se turna con Gohageneis, cuyo puesto de artes marciales no tiene base. *documentada (regla) · [reglas-personajes.md §Auditoría de puestos](../reglas-personajes.md) · [Professions.cs:146](../../Assets/Scripts/Progression/Professions.cs#L146)*

## 7. Relaciones

- Tiene casa propia en la arboleda (cuarta tanda). *recreada · [ResidentialBuilder.cs:32](../../Assets/Editor/ResidentialBuilder.cs#L32)*
- Comparte foto con Gallarle. *atestiguada · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md)*. Qué relación tienen: **Falta**.
- Familia y pareja: **Falta**.

## 8. Horario y costumbres

- Gimnasio: **sin plan propio** (genérico: cuerpo entero 3 días de salud). *código · [GymProgramming.cs:237](../../Assets/Scripts/Progression/GymProgramming.cs#L237)*
- Horario libre: guía las artes marciales de las 20 h los días que le tocan. *recreada · [Professions.cs:90](../../Assets/Scripts/Progression/Professions.cs#L90)*

## 9. Microcosmos y santuario

- Insecto: ninguno propio. *recreada · [microcosmos-civilizaciones.md §5](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo y stats | [SampleSceneBuilder.Construction.cs:217](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L217) |
| Aspecto | [CubeHair.cs:230](../../Assets/Scripts/Avatar/CubeHair.cs#L230) |
| Profesión, base y lo deducido | [Professions.cs:77](../../Assets/Scripts/Progression/Professions.cs#L77) · [Professions.cs:140](../../Assets/Scripts/Progression/Professions.cs#L140) · [Professions.cs:149](../../Assets/Scripts/Progression/Professions.cs#L149) |
| Maestra de artes marciales | [Professions.cs:89](../../Assets/Scripts/Progression/Professions.cs#L89) · [LeisureDirector.cs:178](../../Assets/Scripts/Progression/LeisureDirector.cs#L178) |
| Personal | [WorkdayDirector.cs:22](../../Assets/Scripts/Progression/WorkdayDirector.cs#L22) |
| Área de salida | [SanctuaryDirector.cs:176](../../Assets/Scripts/World/SanctuaryDirector.cs#L176) |
| Casa | [ResidentialBuilder.cs:32](../../Assets/Editor/ResidentialBuilder.cs#L32) |

## 11. Preguntas para Berón

1. ¿Lleva el control de misión de la lanzadera?
2. ¿Dirige ella las artes marciales, como experta (P6), o se turna con Gohageneis?
3. ¿Qué arte marcial practica? ¿Le subimos fuerza y agilidad?
4. ¿Algo de su historia, su carácter o su familia?
