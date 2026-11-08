# Fernando

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Fernando`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Llegó el 2026-11-05, con Consol. *documentada · [personajes-mesocosmos.md §Consol y Fernando](../personajes-mesocosmos.md)*
- **Pendiente de foto.** Aspecto provisional: 1,74 m, pelo corto castaño, chaleco de obra naranja sobre manga larga gris. *inventada · [CubeHair.cs:242-243](../../Assets/Scripts/Avatar/CubeHair.cs#L242) · [diseno-personajes.md §Familia y amigos](../diseno-personajes.md)*
- Edad, origen y género: **Falta** (el nombre es masculino, pero Berón no lo ha dicho).

## 2. Origen e historia

- **Falta**.

## 3. Personalidad y frases

- **Falta**.

## 4. Stats, margas y aptitudes

- Fuerza 1,05 · agilidad 1,0 · resistencia 1,0 · masa 1,0 («neutro»). *inventada · [SampleSceneBuilder.Construction.cs:221](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L221)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- **Instala y repara paneles solares**; Berón pidió añadir la instalación y el mantenimiento a la jornada. *documentada (Berón 2026-11-05) · [Professions.cs:118](../../Assets/Scripts/Progression/Professions.cs#L118) · memoria de Berón, sanctuary-2026-11-05-asks*
- En el juego: `sol_mantenimiento` (cada día, 1 h: escarcha y nieve), `sol_instalacion` (si hay mesas pendientes, 2 h) y `sol_reparacion` (si hay averías, 1 h). *recreada · [energia.md §4](../energia.md) · [EnergySite.cs:283-289](../../Assets/Scripts/World/EnergySite.cs#L283)*
- Su profesión es **toda el área Energía**, también las revisiones del parque (`ene_`: ronda de aves, disuasión…). *inventada (ampliación nuestra, no está en `Professions.Unconfirmed`) · [Professions.cs:35-36](../../Assets/Scripts/Progression/Professions.cs#L35) · [EnergySite.cs:263](../../Assets/Scripts/World/EnergySite.cs#L263)*
- Cómo lo hace (cepillar la escarcha, subir y apretar módulos, cambiar el roto, consola, tablet, linterna). *recreada · [actividades-y-uniformes.md §1.1](../actividades-y-uniformes.md)*

## 6. Puesto y áreas

- Energía: su taller, en el Parque Energético. *recreada · [SanctuaryDirector.cs:178](../../Assets/Scripts/World/SanctuaryDirector.cs#L178) · [energia.md §El sitio](../energia.md)*
- El parque está en el Santuario Aéreo: Fernando sale en el muelle (`Enlace_aereo`) y pasa allí casi todo el día; el viaje son unos 134 s. *recreada · [SampleSceneBuilder.Construction.cs:188](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L188) · [sitio-nuclear.md §23](../sitio-nuclear.md)*

## 7. Relaciones

- Tiene casa propia: la primera de la quinta tanda, que se añadió para él. *recreada · [ResidentialBuilder.cs:34](../../Assets/Editor/ResidentialBuilder.cs#L34) · [ResidentialBuilder.cs:113](../../Assets/Editor/ResidentialBuilder.cs#L113) · [personajes-mesocosmos.md §Consol y Fernando](../personajes-mesocosmos.md)*
- Familia, pareja y amigos: **Falta**.

## 8. Horario y costumbres

- Gimnasio: **sin plan propio** (genérico: cuerpo entero 3 días de salud). *código · [GymProgramming.cs:237](../../Assets/Scripts/Progression/GymProgramming.cs#L237)*
- Horario libre: el reparto común. *recreada · [LeisureDirector.cs:87](../../Assets/Scripts/Progression/LeisureDirector.cs#L87)*

## 9. Microcosmos y santuario

- Insecto: ninguno propio. *recreada · [microcosmos-civilizaciones.md §5](../microcosmos-civilizaciones.md)*
- Trabaja en el Santuario Aéreo (santuario 3) y vive en el santuario 1. *recreada · [sitio-nuclear.md §23](../sitio-nuclear.md)*. Su nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo y stats | [SampleSceneBuilder.Construction.cs:221](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L221) |
| Aspecto provisional | [CubeHair.cs:243](../../Assets/Scripts/Avatar/CubeHair.cs#L243) |
| Profesión y base | [Professions.cs:36](../../Assets/Scripts/Progression/Professions.cs#L36) · [Professions.cs:118](../../Assets/Scripts/Progression/Professions.cs#L118) |
| Sus misiones | [EnergySite.cs:283](../../Assets/Scripts/World/EnergySite.cs#L283) |
| Personal | [WorkdayDirector.cs:23](../../Assets/Scripts/Progression/WorkdayDirector.cs#L23) |
| Dónde sale | [SanctuaryDirector.cs:178](../../Assets/Scripts/World/SanctuaryDirector.cs#L178) · [SampleSceneBuilder.Construction.cs:188](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L188) |
| Casa | [ResidentialBuilder.cs:34](../../Assets/Editor/ResidentialBuilder.cs#L34) |

## 11. Preguntas para Berón

1. Su foto.
2. ¿Lleva también las revisiones del parque (`ene_`: aves y disuasión), o solo los paneles?
3. ¿Algo de su historia, su carácter o su familia?
