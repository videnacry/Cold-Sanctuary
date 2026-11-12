# Goluis

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Goluis` (en la escena, `Goluis_Post`). Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Hombre, 25 años. «El personaje de la fuerza». *documentada · [creature-stats.md §Goluis](../creature-stats.md)*
- 1,75 m, corpulento; degradado con la parte de arriba en punta, perilla, pendiente, tatuajes en los antebrazos, polo claro. *atestiguada (su captura) · [diseno-personajes.md §Fichas](../diseno-personajes.md) · [CubeHair.cs:249](../../Assets/Scripts/Avatar/CubeHair.cs#L249)*
- Nacionalidad y ojos: **Falta** ([diseno-personajes.md §Pendiente](../diseno-personajes.md)).

## 2. Origen e historia

- Trabajo de fuerza del mismo tipo toda su vida; dejó de estudiar a los 17. *documentada · [creature-stats.md §Goluis](../creature-stats.md)*
- De adolescente estuvo en un grupo de bandidos; no ha dado muchos detalles. Le dejó desconfianza y temple en el peligro. *documentada · ídem*. Lo delicado de ese pasado se queda en esa ficha.
- Tiene una hija. *documentada · ídem*
- Trabajó en los cierres de Five Guys con Leo, Andrea y Gohageneis. *documentada · [character-andrea.md §Historia](../character-andrea.md) 4*
- En el santuario trabajó meses, o un año, ajeno a todo, hasta decidir ir más profundo. *documentada · [world-topology-and-planes.md §5](../world-topology-and-planes.md)*
- «La primera mesa del santuario la hizo Goluis con madera que trajo el mar». *inventada (rumor de las charlas) · [SampleSceneBuilder.Conversaciones.cs:102](../../Assets/Editor/SampleSceneBuilder.Conversaciones.cs#L102)*

## 3. Personalidad y frases

- Rápido y muy atento al detalle en su trabajo, tanto que a veces se escaquea («al baño» a mirar el móvil) sin que se note; flojo en lo académico. *documentada · [creature-stats.md §Goluis](../creature-stats.md)*
- Mira con desconfianza; gran temple bajo estrés. *documentada · ídem*
- Mete prisa cuando hay mucho trabajo, va sin dormir o tiene que salir al otro turno (la presión sale de su estado, no es un rasgo fijo). *recreada · [Goluis.cs:4](../../Assets/Scripts/Companion/Companions/Goluis.cs#L4)*
- Escéptico del yoga; enseña con brusquedad. *recreada · [asana-system.md](../asana-system.md) · [architecture.md](../architecture.md)*
- Anclas: `yoga_skepticism` −0,9, `work_hard` 0,85, `family_first` 0,95, `trust_people` −0,4. *recreada · [SampleSceneBuilder.cs:3310-3313](../../Assets/Editor/SampleSceneBuilder.cs#L3310)*
- Vivencias: la carga del trabajo, el pasado en la banda, ser padre. *recreada · [PhrasePools.cs:33-41](../../Assets/Scripts/Mind/PhrasePools.cs#L33)*
- Frases de diálogo: «No toques nada todavía.», «Esta cocina tiene un orden…», «Más rápido.». *inventadas · [DialogueAssetCreator.cs:198](../../Assets/Editor/DialogueAssetCreator.cs#L198) · [DialogueAssetCreator.cs:264](../../Assets/Editor/DialogueAssetCreator.cs#L264)*
- Atención visual «esteta». *recreada · [SampleSceneBuilder.cs:2222](../../Assets/Editor/SampleSceneBuilder.cs#L2222)*

## 4. Stats, margas y aptitudes

- Las 12 aptitudes: fuerza 1,5, masa 1,3, resistencia 1,4, agilidad 0,9, percepción 1,1, adaptabilidad 0,6, temple 1,5, razonamiento 0,7, memoria 0,8, creatividad 0,7, sociabilidad 0,7, disciplina 1,3. *recreada · [creature-stats.md §Perfiles de humanoides](../creature-stats.md) · [Archetypes.cs:139](../../Assets/Scripts/Soul/Archetypes.cs#L139) · [Archetypes.cs:173](../../Assets/Scripts/Soul/Archetypes.cs#L173)*
- Tono elemental: Tierra. *recreada · [Archetypes.cs:173](../../Assets/Scripts/Soul/Archetypes.cs#L173)*
- Margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- **Encargado de la cocina**; hoy los rangos más altos de la cocina son los de Maite y Amykhuz. *documentada · Berón 2026-11-08 (respuesta a esta ficha)*. Su ficha vieja y [world-simulation.md](../world-simulation.md) («Cocina. Dominio de Goluis») lo hacían jefe.
- Profesión en el código: «encargado de cocina», área Cocina (una misión de cocina al día en la ronda de profesiones), con su base. *código · [Professions.cs:50](../../Assets/Scripts/Progression/Professions.cs#L50) · [:128](../../Assets/Scripts/Progression/Professions.cs#L128)*
- Sabe toda la cocina y la enseña a los recién llegados (sígueme → limpiar → recetas). *código · [KitchenBoss.cs:4](../../Assets/Scripts/World/KitchenBoss.cs#L4) · [kitchen-simulation.md §1](../kitchen-simulation.md)*
- Fue el primer entrenador del gimnasio unas horas del 2026-11-04. *código · [GymProgramming.cs:29](../../Assets/Scripts/Progression/GymProgramming.cs#L29) · [gimnasio-maquinas.md §15.5](../gimnasio-maquinas.md)*

## 6. Puesto y áreas

- Sale en la Cocina, con el rótulo «Goluis (encargado de cocina)» (2026-11-08; antes «jefe de cocina»; se ve al reconstruir la escena). *código · [SanctuaryDirector.cs:159](../../Assets/Scripts/World/SanctuaryDirector.cs#L159) · [SampleSceneBuilder.cs:2789](../../Assets/Editor/SampleSceneBuilder.cs#L2789)*
- En la brigada del restaurante, en la línea con Maite y Amykhuz. *recreada · [personajes-mesocosmos.md §La jornada](../personajes-mesocosmos.md)*
- En la jornada rota como todos (J30); sin profesión, no tiene ronda propia.

## 7. Relaciones

- Su hija, y la madre de su hija. *documentada · [creature-stats.md §Goluis](../creature-stats.md)*. Si están en el santuario: **Falta**.
- Compañeros de Five Guys: Leo, Andrea, Gohageneis y Panterilia. *documentada · [character-andrea.md §Historia](../character-andrea.md)*
- Tiene vínculo con Gohageneis: si están cerca tienden a interactuar. *recreada · [review-checklist.md](../review-checklist.md)*
- Vive solo en su casa del árbol ([ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16)).

## 8. Horario y costumbres

- Gimnasio: torso/pierna de fuerza, lunes, martes, jueves y viernes. *recreada · [GymProgramming.cs:204](../../Assets/Scripts/Progression/GymProgramming.cs#L204) · [gimnasio-maquinas.md §15](../gimnasio-maquinas.md)*
- Invierte mucho en comida. *documentada · [creature-stats.md §Goluis](../creature-stats.md)*
- Horario libre: el reparto común ([LeisureDirector.cs:86](../../Assets/Scripts/Progression/LeisureDirector.cs#L86)). Aficiones: **Falta**.

## 9. Microcosmos y santuario

- En los microcosmos es un **escarabajo pelotero** (fuerza y cocina). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Los recién llegados como él empiezan con hechizos humildes. *documentada · [world-topology-and-planes.md](../world-topology-and-planes.md)*
- Por la nota de Berón del 2026-11-08 (los creados antes de Linduarda, Irosene y Lizmibet), probablemente santuario 1, nivel 1. *documentada (nota de Berón; «probablemente»)*

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo, alma y anclas | [SampleSceneBuilder.cs:3306](../../Assets/Editor/SampleSceneBuilder.cs#L3306) |
| Encargado de cocina (rótulo y conducta de iniciación) | [SampleSceneBuilder.cs:2789](../../Assets/Editor/SampleSceneBuilder.cs#L2789) · [KitchenBoss.cs](../../Assets/Scripts/World/KitchenBoss.cs) |
| Su conducta (presión) | [Goluis.cs](../../Assets/Scripts/Companion/Companions/Goluis.cs) |
| Arquetipo | [Archetypes.cs:139](../../Assets/Scripts/Soul/Archetypes.cs#L139) · [Archetypes.cs:173](../../Assets/Scripts/Soul/Archetypes.cs#L173) |
| Vivencias | [PhrasePools.cs:33](../../Assets/Scripts/Mind/PhrasePools.cs#L33) |
| Diálogos | [DialogueAssetCreator.cs:198](../../Assets/Editor/DialogueAssetCreator.cs#L198) |
| Aspecto | [CubeHair.cs:251](../../Assets/Scripts/Avatar/CubeHair.cs#L251) |
| Puesto y su base | [Professions.cs:50](../../Assets/Scripts/Progression/Professions.cs#L50) · [Professions.cs:128](../../Assets/Scripts/Progression/Professions.cs#L128) |
| Personal, salida, gimnasio y casa | [WorkdayDirector.cs:19](../../Assets/Scripts/Progression/WorkdayDirector.cs#L19) · [SanctuaryDirector.cs:159](../../Assets/Scripts/World/SanctuaryDirector.cs#L159) · [GymProgramming.cs:204](../../Assets/Scripts/Progression/GymProgramming.cs#L204) · [ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16) |

## 11. Preguntas para Berón

1. ~~¿Sigue de jefe de cocina?~~ Contestado (2026-11-08): es encargado; Maite y Amykhuz tienen los rangos más altos. ¿Qué hace un encargado que no hagan los demás (turnos, pedidos, la iniciación de la cocina)?
2. ¿Su nacionalidad?
3. ¿Su hija está en el santuario o en algún sitio del juego?
4. ¿Es verdad que estuvo en los cierres de Five Guys con Leo, y desde cuándo está en el santuario?
5. ¿Qué le gusta hacer en su tiempo libre?
