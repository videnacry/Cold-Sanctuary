# Gallarle

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Gallarle`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Persona real; llegó el 2026-11-04 como pareja de Ducristin. *documentada (Berón) · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md)*
- Rasgo femenino (`Fem()`), confirmado por su foto. *atestiguada · [personajes-mesocosmos.md §Lale, Yulo, Malena y las fotos](../personajes-mesocosmos.md)*
- Melena oscura a los hombros, pendientes oscuros, americana negra sobre blusa crema. *atestiguada (foto `gallarle_luciana`, a la izquierda; orden confirmado por Berón el 2026-11-05) · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md) · [CubeHair.cs:227-228](../../Assets/Scripts/Avatar/CubeHair.cs#L227)*
- 1,62 m. *inventada (estatura estimada) · ídem*
- Edad y origen: **Falta**.

## 2. Origen e historia

- **Falta**.

## 3. Personalidad y frases

- **Falta**.

## 4. Stats, margas y aptitudes

- Fuerza, agilidad, resistencia y masa 1,0. *inventada · [SampleSceneBuilder.Construction.cs:208](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L208)*
- Lleva el ciclo de vida de los humanos (`PersonLife`), sin embarazos. *recreada · [SampleSceneBuilder.cs:3072](../../Assets/Editor/SampleSceneBuilder.cs#L3072)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- **Sin profesión**: hace la rotación general. *código · [Professions.cs:148](../../Assets/Scripts/Progression/Professions.cs#L148) · [reglas-personajes.md §Auditoría de puestos](../reglas-personajes.md)*

## 6. Puesto y áreas

- Sin área de salida en `HomeArea`; la escena la pone en el Aula, junto a su pareja. *recreada · [SampleSceneBuilder.Construction.cs:190](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L190)*

## 7. Relaciones

- Pareja de **Ducristin**; comparten casa del árbol. *documentada (Berón 2026-11-04: «Ducristin tiene de pareja a Gallarle») · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [ResidentialBuilder.cs:43](../../Assets/Editor/ResidentialBuilder.cs#L43)*
- Madre de **Neo**, que vive con ellos y la sigue de niño (`FollowMother`). *documentada (Berón: Neo, hijo de Ducristin y Gallarle) · [personajes-mesocosmos.md §Neo](../personajes-mesocosmos.md) · [ResidentialBuilder.cs:47-50](../../Assets/Editor/ResidentialBuilder.cs#L47)*. A cuál de los dos sigue Neo es decisión nuestra, «pendiente de confirmar con Berón» ([SampleSceneBuilder.cs:3072](../../Assets/Editor/SampleSceneBuilder.cs#L3072)).
- Amigos: **Falta**.

## 8. Horario y costumbres

- Gimnasio: **sin plan propio**: cuerpo entero 3 días de salud, el genérico. *código · [GymProgramming.cs:237](../../Assets/Scripts/Progression/GymProgramming.cs#L237)*
- Horario libre: el reparto común. *recreada · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*
- Costumbres propias: **Falta**.

## 9. Microcosmos y santuario

- Insecto: ninguno propio. *recreada · [microcosmos-civilizaciones.md §5](../microcosmos-civilizaciones.md)*
- Santuario y nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo y stats | [SampleSceneBuilder.Construction.cs:208](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L208) |
| Aspecto | [CubeHair.cs:228](../../Assets/Scripts/Avatar/CubeHair.cs#L228) |
| Sin profesión | [Professions.cs:148](../../Assets/Scripts/Progression/Professions.cs#L148) |
| Personal | [WorkdayDirector.cs:21](../../Assets/Scripts/Progression/WorkdayDirector.cs#L21) |
| Dónde sale | [SampleSceneBuilder.Construction.cs:190](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L190) |
| Madre de Neo | [SampleSceneBuilder.cs:3072](../../Assets/Editor/SampleSceneBuilder.cs#L3072) |
| Casa | [ResidentialBuilder.cs:43](../../Assets/Editor/ResidentialBuilder.cs#L43) |

## 11. Preguntas para Berón

1. ¿Cuál es su profesión? (pendiente desde el 2026-11-04)
2. ¿Neo va de niño con ella o con Ducristin?
3. ¿Algo de su historia o su carácter?
