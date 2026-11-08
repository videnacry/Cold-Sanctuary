# Consol

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Consol`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Mujer (Berón la nombra en femenino), española. *documentada (Berón 2026-11-05) · [personajes-mesocosmos.md §Consol y Fernando](../personajes-mesocosmos.md) · memoria de Berón, sanctuary-2026-11-05-asks*
- Unos 50 años, de aspecto joven; muy flexible. *documentada (Berón 2026-11-05) · ídem · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md)*
- **Pendiente de foto.** Aspecto provisional: 1,63 m, media melena oscura, ropa de yoga lisa morada y pantalón oscuro, sin rasgos inventados; `Fem()`. *inventada · [CubeHair.cs:240-241](../../Assets/Scripts/Avatar/CubeHair.cs#L240)*

## 2. Origen e historia

- **Falta** (dónde aprendió Ashtanga, cómo llegó al santuario).

## 3. Personalidad y frases

- Sus clases son profundas: posturas largas, con variaciones y secuencias como en Ashtanga. *documentada (Berón 2026-11-05) · [ashtanga-primary-series.md §Clase profunda de Consol](../ashtanga-primary-series.md)*
- Carácter y frases: **Falta**.

## 4. Stats, margas y aptitudes

- Fuerza 1,0 · agilidad 1,35 («muy flexible») · resistencia 1,15 · masa 0,95. *recreada · [SampleSceneBuilder.Construction.cs:220](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L220)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Profesora de yoga, experta en Ashtanga. *documentada (Berón 2026-11-05) · [Professions.cs:117](../../Assets/Scripts/Progression/Professions.cs#L117)*
- Berón: «ahora que ha llegado Consol el segundo puesto se le pondría a ella siendo que existe documentación de su gran habilidad, de hecho su descripción la hace candidata a ser superior a Josedrilo». *documentada · [personajes-mesocosmos.md §Consol y Fernando](../personajes-mesocosmos.md) · [reglas-personajes.md §Auditoría de puestos](../reglas-personajes.md)*
- Su clase, «Ashtanga profundo» (`AshtangaPrimary.Deep`): Surya Namaskara A, tres posturas de 8 respiraciones con su variación de 5 (Paschimottanasana B, Baddha Konasana B, Prasarita Padottanasana C) y Savasana; la postura que abre rota con el día. *recreada · [ashtanga-primary-series.md §Clase profunda de Consol](../ashtanga-primary-series.md) · [AshtangaPrimary.cs:67](../../Assets/Scripts/Asana/AshtangaPrimary.cs#L67) · [YogaLibrary.cs:23](../../Assets/Scripts/Asana/YogaLibrary.cs#L23) · [YogaClass.cs:38](../../Assets/Scripts/World/YogaClass.cs#L38)*

## 6. Puesto y áreas

- **Maestra jefa de Ashtanga** (`AshtangaTeacher`); Josedrilo, jefe de Hatha. Cada uno jefe de su estilo. *recreada (decisión nuestra sobre lo que dijo Berón) · [WorkdayDirector.cs:777](../../Assets/Scripts/Progression/WorkdayDirector.cs#L777) · [personajes-mesocosmos.md §Consol y Fernando](../personajes-mesocosmos.md) · [yoga-pisos.md §5](../yoga-pisos.md)*
- Profesión en el juego: la Sala de Yoga, solo `yog_ashtanga` (0,5 h: preparar la clase). No entra en la clase de grupo de Hatha (`clase_yoga`). *recreada · [Professions.cs:34](../../Assets/Scripts/Progression/Professions.cs#L34) · [AreaFunctions.cs:173](../../Assets/Scripts/Progression/AreaFunctions.cs#L173)*
- Abre la clase de Ashtanga de las 14 h en la primera sala del día, aunque no haya llegado aún. *código · [WorkdayDirector.cs:783](../../Assets/Scripts/Progression/WorkdayDirector.cs#L783) · [Professions.cs:92-93](../../Assets/Scripts/Progression/Professions.cs#L92)*
- Sale en el piso de la Sala de Yoga. *recreada · [SanctuaryDirector.cs:178](../../Assets/Scripts/World/SanctuaryDirector.cs#L178) · [SampleSceneBuilder.Construction.cs:187](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L187)*

## 7. Relaciones

- Con Josedrilo: los dos maestros jefes de yoga. *recreada · [WorkdayDirector.cs:777](../../Assets/Scripts/Progression/WorkdayDirector.cs#L777)*
- Tiene casa propia: la última de la cuarta tanda. *recreada · [ResidentialBuilder.cs:34](../../Assets/Editor/ResidentialBuilder.cs#L34) · [personajes-mesocosmos.md §Consol y Fernando](../personajes-mesocosmos.md)*
- Familia, pareja y amigos: **Falta**.

## 8. Horario y costumbres

- Lunes a viernes da la clase de Ashtanga de las 14 h; a las 20 h guía el Ashtanga autoguiado. *recreada · [personajes-mesocosmos.md §Consol y Fernando](../personajes-mesocosmos.md) · [Professions.cs:91-93](../../Assets/Scripts/Progression/Professions.cs#L91)*
- Sábado y domingo, gimnasio suave: cuerpo entero, salud. *recreada · [GymProgramming.cs:219](../../Assets/Scripts/Progression/GymProgramming.cs#L219)*
- La clase profunda pasa a Savasana a las 14:45 ([YogaClass.cs:41](../../Assets/Scripts/World/YogaClass.cs#L41)); el doc aún dice 15:45 ([ashtanga-primary-series.md §Clase profunda](../ashtanga-primary-series.md)).

## 9. Microcosmos y santuario

- Insecto: ninguno propio. *recreada · [microcosmos-civilizaciones.md §5](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo y stats | [SampleSceneBuilder.Construction.cs:220](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L220) |
| Aspecto provisional | [CubeHair.cs:241](../../Assets/Scripts/Avatar/CubeHair.cs#L241) |
| Profesión y base | [Professions.cs:34](../../Assets/Scripts/Progression/Professions.cs#L34) · [Professions.cs:117](../../Assets/Scripts/Progression/Professions.cs#L117) |
| Maestra de Ashtanga | [WorkdayDirector.cs:777](../../Assets/Scripts/Progression/WorkdayDirector.cs#L777) · [Professions.cs:92](../../Assets/Scripts/Progression/Professions.cs#L92) |
| Su clase | [AshtangaPrimary.cs:67](../../Assets/Scripts/Asana/AshtangaPrimary.cs#L67) · [YogaClass.cs:38](../../Assets/Scripts/World/YogaClass.cs#L38) |
| Personal | [WorkdayDirector.cs:23](../../Assets/Scripts/Progression/WorkdayDirector.cs#L23) |
| Área de salida | [SanctuaryDirector.cs:178](../../Assets/Scripts/World/SanctuaryDirector.cs#L178) |
| Gimnasio | [GymProgramming.cs:219](../../Assets/Scripts/Progression/GymProgramming.cs#L219) |
| Casa | [ResidentialBuilder.cs:34](../../Assets/Editor/ResidentialBuilder.cs#L34) |
| Prueba | `PersonajesReglasTest` (grupo 95) |

## 11. Preguntas para Berón

1. Su foto.
2. ¿Queda como jefa de todo el yoga, por encima de Josedrilo, o cada uno de su estilo?
3. ¿De qué parte de España es, y cómo llegó al santuario?
4. ¿Algo de su carácter?
