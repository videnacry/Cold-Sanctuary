# Tofelite «Tote»

> Ficha con la plantilla de [README.md](README.md). Id en el código: `JefeConstruccion`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Se llama Tofelite y lo llaman «Tote». El id del código sigue siendo `JefeConstruccion` (escenas y pruebas). *documentada (Berón 2026-10-31) · memoria `character-roles` · [SampleSceneBuilder.cs:3054](../../Assets/Editor/SampleSceneBuilder.cs#L3054)*
- Hombre: «el esposo de Lizmibet». *documentada · ídem*
- 1,68 m, robusto; piel morena, pelo corto oscuro, gafas de sol envolventes oscuras; camisa rosa pálido de rayas finas, remangada y con el cuello abierto; pantalón gris de vestir con cinturón oscuro, zapatos marrones de cordones y reloj dorado en la muñeca izquierda. *atestiguada (foto `tote.jpg`, 2026-11-04) · [diseno-personajes.md §Fichas](../diseno-personajes.md) · [CubeHair.cs:155](../../Assets/Scripts/Avatar/CubeHair.cs#L155)*
- Antes llevaba polo gris y bermudas, de la foto con Lizmibet. *atestiguada · [CubeHair.cs:155](../../Assets/Scripts/Avatar/CubeHair.cs#L155)*
- Edad, país y ojos: **Falta**.

## 2. Origen e historia

- **Falta**: no hay nada escrito de su vida antes del santuario ni de cómo llegó.

## 3. Personalidad y frases

- Una pose relajada en su foto, que el juego aún no copia. *atestiguada · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md)*
- Carácter, frases, vivencias y anclas: **Falta**.

## 4. Stats, margas y aptitudes

- Cuerpo: fuerza 1,3 · agilidad 0,95 · resistencia 1,25 · masa 1,15 (robusto). *recreada · [SampleSceneBuilder.cs:3055](../../Assets/Editor/SampleSceneBuilder.cs#L3055) · [PoseGallery.cs:142](../../Assets/Editor/PoseGallery.cs#L142)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Jefe de reparaciones (las misiones `rep_`) y guardia. *documentada (Berón 2026-11-04) · [Professions.cs:38](../../Assets/Scripts/Progression/Professions.cs#L38) · [Professions.cs:121](../../Assets/Scripts/Progression/Professions.cs#L121)*
- Antes: jefe de construcción, obras y reparaciones de las áreas humanas. *documentada (Berón 2026-10-31) · memoria `character-roles`*. Desde el 2026-11-04 la obra nueva la dirige Josedrilo. *documentada · [personajes-mesocosmos.md §Profesiones](../personajes-mesocosmos.md)*
- Guardia con Gohageneis y Nesvitor; su profesión incluye la Sala de guardia. *documentada · [sala-de-guardia.md](../sala-de-guardia.md)*

## 6. Puesto y áreas

- Sale en Construcción, junto al banco de trabajo y la pizarra de avisos. *código · [SampleSceneBuilder.Construction.cs:166](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L166) · [SanctuaryDirector.cs:163](../../Assets/Scripts/World/SanctuaryDirector.cs#L163)*
- Áreas de su profesión: Construcción (`rep_`), Guardia y Sala de guardia. *código · [Professions.cs:38](../../Assets/Scripts/Progression/Professions.cs#L38)*
- Su rótulo aún dice «jefe de construcción» ([SampleSceneBuilder.cs:3054](../../Assets/Editor/SampleSceneBuilder.cs#L3054)), y el comentario de su área, «dirige las obras» ([SanctuaryDirector.cs:163](../../Assets/Scripts/World/SanctuaryDirector.cs#L163)).

## 7. Relaciones

- Esposa: Lizmibet; comparten casa del árbol. *documentada (Berón 2026-11-04) · [ResidentialBuilder.cs:39](../../Assets/Editor/ResidentialBuilder.cs#L39) · [manual-de-diseno.md J14](../manual-de-diseno.md)*
- Hijos, nietos (¿Leo?) y amigos: **Falta**.

## 8. Horario y costumbres

- Gimnasio: torso/pierna, martes, miércoles, viernes y sábado, resistencia («robusto, jornadas de obra»). *recreada · [GymProgramming.cs:208](../../Assets/Scripts/Progression/GymProgramming.cs#L208) · [gimnasio-maquinas.md §15](../gimnasio-maquinas.md)*
- Horario libre: el reparto común ([LeisureDirector.cs:86](../../Assets/Scripts/Progression/LeisureDirector.cs#L86)). Aficiones: **Falta**.

## 9. Microcosmos y santuario

- En los microcosmos es una **termita** (constructora de montículos). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Su cuerpo sirve de turista del restaurante. *código · [RestaurantService.cs:571](../../Assets/Scripts/Kitchen/RestaurantService.cs#L571)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo, stats y rótulo | [SampleSceneBuilder.cs:3054](../../Assets/Editor/SampleSceneBuilder.cs#L3054) |
| Aspecto | [CubeHair.cs:159](../../Assets/Scripts/Avatar/CubeHair.cs#L159) |
| Profesión y su base | [Professions.cs:38](../../Assets/Scripts/Progression/Professions.cs#L38) · [Professions.cs:121](../../Assets/Scripts/Progression/Professions.cs#L121) |
| Nombre de la ficha (`tote`) | [Professions.cs:112](../../Assets/Scripts/Progression/Professions.cs#L112) |
| Personal y horario libre | [WorkdayDirector.cs:19](../../Assets/Scripts/Progression/WorkdayDirector.cs#L19) · [LeisureDirector.cs:86](../../Assets/Scripts/Progression/LeisureDirector.cs#L86) |
| Salida | [SampleSceneBuilder.Construction.cs:166](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L166) · [SanctuaryDirector.cs:163](../../Assets/Scripts/World/SanctuaryDirector.cs#L163) |
| Gimnasio | [GymProgramming.cs:208](../../Assets/Scripts/Progression/GymProgramming.cs#L208) |
| Casa, con Lizmibet | [ResidentialBuilder.cs:39](../../Assets/Editor/ResidentialBuilder.cs#L39) |

## 11. Preguntas para Berón

1. ¿Quién es Tote: de dónde es, cómo es, qué edad tiene, cómo llegó al santuario?
2. ¿Es abuelo de Leo, como Lizmibet?
3. ¿Su rótulo pasa a «jefe de reparaciones y guardia»? Aún dice «jefe de construcción».
4. ¿Qué hace como guardia, aparte de las rondas que le toquen?
5. ¿En qué santuario y nivel está?
