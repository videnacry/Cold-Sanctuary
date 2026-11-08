# Lizmibet «Liz»

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Lizmibet`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Mujer; la llaman «Liz». *documentada (Berón 2026-10-31) · memoria `character-roles` · [SampleSceneBuilder.cs:3048](../../Assets/Editor/SampleSceneBuilder.cs#L3048)*
- Más de 70 años, ecuatoriana. *documentada ⚠️ (ficha de Irosene) · [character-irosene.md §Relaciones](../character-irosene.md) · [diseno-personajes.md §Fichas](../diseno-personajes.md)*
- 1,55 m; pelo corto rizado y voluminoso castaño cobrizo, gafas finas plateadas, piel clara; vestido blanco campesino de manga larga hasta los tobillos y sandalias blancas. *atestiguada (su foto) · [diseno-personajes.md §Fichas](../diseno-personajes.md) · [CubeHair.cs:152](../../Assets/Scripts/Avatar/CubeHair.cs#L152)*
- Ojos: **Falta**.

## 2. Origen e historia

- Llegó en avión desde Ecuador con Irosene para ver a su nieto Leo «por última vez»; en el santuario ganaron fuerza para creer que les quedan muchos años. *documentada ⚠️ · [character-irosene.md §Relaciones](../character-irosene.md), «Contexto de llegada»*
- Llegaron muy mal físicamente y pasaron en el Nivel 1 mucho más tiempo de lo normal: aprendieron hechizos avanzados. Se dice que fueron las únicas del Nivel 2 con hechizos de Nivel 4. *documentada ⚠️ · [character-irosene.md §Resumen, §Lugar en el santuario](../character-irosene.md)*
- Hace un año que ya se mantienen solas. *documentada ⚠️ · [character-irosene.md §Relaciones](../character-irosene.md)*
- Su vida antes del santuario (estudios, dónde ejerció): **Falta**.

## 3. Personalidad y frases

- Atención visual «afectiva»: mira los vínculos. *recreada · [SampleSceneBuilder.cs:2223](../../Assets/Editor/SampleSceneBuilder.cs#L2223) · [atencion-visual.md](../atencion-visual.md)*
- Carácter, gustos, frases, vivencias y anclas: **Falta** (no tiene `MoodState`, arquetipo ni vivencias en [PhrasePools.cs](../../Assets/Scripts/Mind/PhrasePools.cs)).

## 4. Stats, margas y aptitudes

- Cuerpo: fuerza 0,95 · agilidad 1,05 · resistencia 1,2 · masa 1,05 («activa a sus 70 y tantos»). *recreada · [SampleSceneBuilder.cs:3046](../../Assets/Editor/SampleSceneBuilder.cs#L3046) · [PoseGallery.cs:141](../../Assets/Editor/PoseGallery.cs#L141)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Médica. *documentada (Berón) · memoria `character-roles` · [diseno-personajes.md §Fichas](../diseno-personajes.md)*
- Hechicera con hechizos de nivel 4. *documentada ⚠️ · [character-irosene.md §Lugar en el santuario](../character-irosene.md)*
- **No tiene entrada en `Professions.All`** ni base en `Professions.Basis`. Está en `Professions.Medical`: el submarino-ambulancia la busca primero. *código · [Professions.cs:26](../../Assets/Scripts/Progression/Professions.cs#L26) · [submarinos-medicos.md](../submarinos-medicos.md)*

## 6. Puesto y áreas

- Médica de la enfermería; lleva los registros y la vacunación con Linduarda (heredado de Bill Gates). *recreada · [personajes-mesocosmos.md §Por qué así](../personajes-mesocosmos.md)*
- Sale en el hospital ([SanctuaryDirector.cs:161](../../Assets/Scripts/World/SanctuaryDirector.cs#L161)) y pasa visita en el anillo de la planta 1 ([SampleSceneBuilder.Hospital.cs:342](../../Assets/Editor/SampleSceneBuilder.Hospital.cs#L342)).
- Uno de los 4 puestos médicos humanos del hospital. *documentada (petición de Berón) · [reglas-personajes.md §Auditoría](../reglas-personajes.md)*
- En la jornada rota como todos (J30); sin profesión, no tiene ronda propia.

## 7. Relaciones

- Esposo: Tofelite «Tote»; comparten casa del árbol. *documentada (Berón 2026-10-31 y 2026-11-04) · [ResidentialBuilder.cs:39](../../Assets/Editor/ResidentialBuilder.cs#L39) · [personajes-mesocosmos.md §Parejas](../personajes-mesocosmos.md)*
- Irosene: su gran amiga, de su misma edad y nivel; «probablemente consuegra» (co-abuela de Leo). *documentada ⚠️ · [character-irosene.md §Relaciones](../character-irosene.md)*
- Leo: su nieto. *documentada ⚠️ · ídem*. Por qué lado de la familia: **Falta**.
- Linduarda: atiende con ella en la enfermería. *recreada · [SampleSceneBuilder.cs:3040](../../Assets/Editor/SampleSceneBuilder.cs#L3040)*
- Hijos: **Falta**.

## 8. Horario y costumbres

- Gimnasio: cuerpo entero en máquinas guiadas, martes y viernes, objetivo salud (OMS y ACSM para mayores). *recreada · [GymProgramming.cs:203](../../Assets/Scripts/Progression/GymProgramming.cs#L203) · [gimnasio-maquinas.md §15](../gimnasio-maquinas.md)*
- Berón la vio usar las máquinas, y de ahí salieron los entrenamientos reales para todos. *documentada · [gimnasio-maquinas.md §15](../gimnasio-maquinas.md)*
- Horario libre: el reparto común ([LeisureDirector.cs:86](../../Assets/Scripts/Progression/LeisureDirector.cs#L86)). Aficiones: **Falta**.

## 9. Microcosmos y santuario

- En los microcosmos es una **mariquita** (protege el huerto). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Su cuerpo sirve de turista del restaurante. *código · [RestaurantService.cs:571](../../Assets/Scripts/Kitchen/RestaurantService.cs#L571)*
- Nivel: «las únicas del Nivel 2 con hechizos de Nivel 4» ⚠️. Berón (2026-11-08) cree que los creados antes de Linduarda, Irosene y Lizmibet son de santuario 1, nivel 1. *documentada (nota de Berón)*. Santuario y nivel exactos: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo, stats y rótulo | [SampleSceneBuilder.cs:3043](../../Assets/Editor/SampleSceneBuilder.cs#L3043) (en `BuildLinduarda`) |
| Aspecto | [CubeHair.cs:154](../../Assets/Scripts/Avatar/CubeHair.cs#L154) |
| Personal y horario libre | [WorkdayDirector.cs:19](../../Assets/Scripts/Progression/WorkdayDirector.cs#L19) · [LeisureDirector.cs:86](../../Assets/Scripts/Progression/LeisureDirector.cs#L86) |
| Personal sanitario | [Professions.cs:26](../../Assets/Scripts/Progression/Professions.cs#L26) |
| Área de salida | [SanctuaryDirector.cs:161](../../Assets/Scripts/World/SanctuaryDirector.cs#L161) |
| Gimnasio | [GymProgramming.cs:203](../../Assets/Scripts/Progression/GymProgramming.cs#L203) |
| Casa y pareja | [ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16) · [ResidentialBuilder.cs:39](../../Assets/Editor/ResidentialBuilder.cs#L39) |

## 11. Preguntas para Berón

1. No hay nada escrito de su carácter ni de sus gustos. ¿Cómo es ella?
2. ¿Confirma lo marcado ⚠️ de la ficha de Irosene: consuegra de Irosene, abuela de Leo, llegaron juntas desde Ecuador?
3. ¿Cuál es su historia como médica antes del santuario?
4. ¿Le damos una entrada de profesión (médica) con su base, como a Ivelyna y Malena?
5. ¿Tiene hijos u otra familia en el juego?
6. ¿En qué santuario y nivel está (regla del 2026-11-08)?
