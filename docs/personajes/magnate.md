# La Magnate

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Magnate`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o un
> doc de diseño que él dictó); **atestiguada** = se ve en su hoja de diseño; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = el doc lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Mujer: «la Magnate», «la maestra». *documentada · [diseno-personajes.md §Fichas](../diseno-personajes.md)*
- 1,66 m. Bob azul noche con flequillo recto y mechones largos delante, ojos marrones, labios morados, piel clara amarillenta; blusa blanca de cuello alto, bolero morado de mangas abullonadas, corsé granate con ribete dorado, bombacho celeste y botas altas azul marino. *atestiguada (su hoja de diseño) · [diseno-personajes.md §Fichas](../diseno-personajes.md) · [CubeHair.cs:148](../../Assets/Scripts/Avatar/CubeHair.cs#L148)*
- Nombre propio y edad: **Falta**.

## 2. Origen e historia

- Su base es el 5.º santuario, el Núcleo de la Tierra (plasma y diamante), con un desierto artificial: el lugar más peligroso, también para los demás jefes. *documentada · [sanctuary-second-lap-and-fear.md §6](../sanctuary-second-lap-and-fear.md)*
- Ha logrado interactuar con el cielo y el infierno: el desierto es la puerta al infierno; el núcleo de plasma, al paraíso. *documentada · ídem*
- Tiene socios, los más poderosos del Macrocosmos, que poseen partes de los santuarios y planean destronarla. *documentada («idea inicial») · [world-topology-and-planes.md §5](../world-topology-and-planes.md)*
- En el final Kushal la derrota y le quita sus poderes; Leo sale de la esfera, la toma y desaparece. *documentada («idea inicial») · ídem*
- De dónde viene y cómo llegó a ser maga: **Falta**.

## 3. Personalidad y frases

- La mueven sus «pensamientos custom», no las necesidades: proteger la vida y hacer crecer a todos como hechiceros. *recreada · [Magnate.cs:4-12](../../Assets/Scripts/World/Magnate.cs#L4)*
- Firme sin violencia: a quien deja su puesto le explica por qué no puede irse y lo devuelve. *documentada (Berón 2026-10-22) · [MagnateWarden.cs:5](../../Assets/Scripts/World/MagnateWarden.cs#L5)*
- Sus pensamientos van bloqueados: nadie los hereda. *recreada · [anima-architecture.md](../anima-architecture.md) §reparto de frases*
- Eje oscuro de la historia (su hechizo de miedo). *documentada · [apremios-guardian-observacion.md](../apremios-guardian-observacion.md)*
- Carácter en palabras de Berón, frases propias: **Falta**.

## 4. Stats, margas y aptitudes

- Estado «infinito»: stats y margas al tope, sin límite numérico; sin necesidades; imposible de poseer. *documentada · [spells-suprahechizo-and-thoughts.md §5](../spells-suprahechizo-and-thoughts.md) · [Magnate.cs:4](../../Assets/Scripts/World/Magnate.cs#L4)*
- Maga de células: sus stats salen de sus células, no de su musculatura (`BodyShape` no la toca). *recreada · [body-shape-from-stats.md](../body-shape-from-stats.md)*
- Sus hechizos son supremos (la ranura «madre-Magnate»). *documentada · [anima-architecture.md](../anima-architecture.md)*

## 5. Profesión y lo que sabe hacer

- La maestra del santuario y máxima autoridad de todas las áreas; sabe todas las tareas e iniciaciones. *documentada (Berón 2026-11-05) · manual J31 y J32 · [jornada-y-rotacion.md](../jornada-y-rotacion.md)*
- Pacifica a los depredadores cercanos y enseña yoga al que se queda atrás. *código · [Magnate.cs:84](../../Assets/Scripts/World/Magnate.cs#L84) · [Magnate.cs:279](../../Assets/Scripts/World/Magnate.cs#L279)*
- Inicia al recién llegado cuando Linduarda lo certifica. *código · [Magnate.cs:101](../../Assets/Scripts/World/Magnate.cs#L101)*

## 6. Puesto y áreas

- Guarda los puestos: se teletransporta ante quien abandona el suyo y lo devuelve (J4). *documentada · [MagnateWarden.cs](../../Assets/Scripts/World/MagnateWarden.cs)*
- Último recurso de las iniciaciones (diseño final, Berón 2026-11-05): inician los trabajadores que saben. *documentada · manual J32*
- Se divide en clones de 5 h si faltan manos para las obligatorias; Berón (2026-11-08): solo cuando un área se quedaría sin trabajador. *documentada · [jornada-y-rotacion.md](../jornada-y-rotacion.md) · nota de Berón 2026-11-08*
- No está en la jornada (`WorkdayDirector.Staff`); sí en las personas del mesocosmos ([Professions.cs:104](../../Assets/Scripts/Progression/Professions.cs#L104)).

## 7. Relaciones

- Kushal: lo elige para ir a su santuario a crear quimeras; él tiene la misión secreta de derrotarla con los demás jefes. *documentada · [sanctuary-second-lap-and-fear.md §6](../sanctuary-second-lap-and-fear.md)*
- Linduarda: la llama cuando un recién llegado está apto. *código · [Magnate.cs:126](../../Assets/Scripts/World/Magnate.cs#L126)*
- Sus socios y Leo (ver §2). *documentada · [world-topology-and-planes.md §5](../world-topology-and-planes.md)*
- Casa propia en la arboleda ([ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16)). Familia: **Falta**.

## 8. Horario y costumbres

- No come ni duerme. *documentada · [spells-suprahechizo-and-thoughts.md §5](../spells-suprahechizo-and-thoughts.md)*
- Cada 30 s busca al rezagado para enseñarle. *código · [Magnate.cs:18](../../Assets/Scripts/World/Magnate.cs#L18)*
- Pendiente de diseño: guardia nocturna alimentando animales. *documentada · [spells-suprahechizo-and-thoughts.md §5](../spells-suprahechizo-and-thoughts.md)*

## 9. Microcosmos y santuario

- Existe, clonada, en todos los santuarios. *documentada · [Magnate.cs:11](../../Assets/Scripts/World/Magnate.cs#L11)*
- En los microcosmos es un **pulgón**: se clona como ella. *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Santuario: el 5.º (el Núcleo). *documentada · [sanctuary-second-lap-and-fear.md §6](../sanctuary-second-lap-and-fear.md)*

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo en la escena | [SampleSceneBuilder.cs:2793](../../Assets/Editor/SampleSceneBuilder.cs#L2793) (`BuildMagnate`) |
| Su conducta | [Magnate.cs](../../Assets/Scripts/World/Magnate.cs) |
| Guardar los puestos | [MagnateWarden.cs](../../Assets/Scripts/World/MagnateWarden.cs) |
| Aspecto | [CubeHair.cs:148](../../Assets/Scripts/Avatar/CubeHair.cs#L148) |
| Personas del mesocosmos | [Professions.cs:104](../../Assets/Scripts/Progression/Professions.cs#L104) |
| Casa | [ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16) |

## 11. Preguntas para Berón

1. ¿Tiene nombre propio y edad? ¿De dónde viene?
2. ¿Cómo es su carácter con la gente del santuario (cálida, distante)?
3. ¿Sigue enseñando yoga al rezagado ahora que el yoga lo dan Josedrilo y Consol?
4. ¿El final con los socios y Leo sigue en pie?
5. Si los clones son solo para áreas sin trabajador (2026-11-08), ¿siguen cubriendo las obligatorias que falten?
