# Ibrahim Traoré

> Ficha con la plantilla de [README.md](README.md). Id en el código: `IbrahimTraore`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón o lo dice una fuente
> pública citada en el doc; **atestiguada** = se ve en su imagen de referencia; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. **Falta** = no se sabe.
> **Persona real y viva, líder político** (J6, J12): solo un acto civil, sin nada militar; **no se le inventan frases** (W11): lo explican personajes inventados y él solo dice lo que es de registro público; la protagonista es la institución. Ya no vive en el santuario.

## 1. Identidad y aspecto

- Piel oscura, pelo negro muy corto y barba corta, boina roja, uniforme de campaña verde oliva y botas (aspecto guardado; su microcosmos no lleva nada militar). *atestiguada (rasgos públicos) · [CubeHair.cs:186](../../Assets/Scripts/Avatar/CubeHair.cs#L186) · [personajes-mesocosmos.md §Historia: los seis del mesocosmos](../personajes-mesocosmos.md)*
- Edad y estatura exacta: no se registran (solo su trabajo público).

## 2. Origen e historia

- Su microcosmos: acto civil: la entrega de tractores para labrar gratis los campos; la siembra y el zaï, Uagadugú (Burkina Faso), 6-5-2024. *documentada (fuentes citadas) · [microcosmos-famosos.md §Las siete, §Fuentes](../microcosmos-famosos.md)*
- La ficha dice cómo llegó al poder, en una línea y sin juicio, y que no hay evaluación independiente de la Ofensiva agropastoral. *documentada · [microcosmos-famosos.md §Cómo se eligió](../microcosmos-famosos.md)*

## 3. Personalidad y frases

- Sus frases son **de juego**, en su voz; no son citas; en la auditoría W11 se quitaron las frases inventadas suyas y de su ministro. *recreada · [microcosmos-famosos.md §Cómo se eligió](../microcosmos-famosos.md) · [microcosmos-historias-3.md §Regla W11](../microcosmos-historias-3.md)*
- Carácter: solo lo público; nada más en los docs.

## 4. Stats, margas y aptitudes

- Fuerza 1,25 · agilidad 1,2 · resistencia 1,3 · masa 1,05. *inventada · [SampleSceneBuilder.Construction.cs:236](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L236) · [personajes-mesocosmos.md §Historia](../personajes-mesocosmos.md)*
- Aptitudes mentales y margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Capitán, presidente de transición de Burkina Faso. *documentada · [SampleSceneBuilder.Construction.cs:236](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L236) (su rótulo)*

## 6. Puesto y áreas

- **Ya no tiene puesto** (Berón 2026-11-04: «los personajes famosos … sácalos del mesocosmos y crea sus microcosmos respectivos»; J12). En el santuario hacía: coordinar la guardia. Ese papel lo heredó otro ([personajes-mesocosmos.md §Reparto de papeles](../personajes-mesocosmos.md)). *documentada · [personajes-mesocosmos.md §2026-11-04](../personajes-mesocosmos.md)*

## 7. Relaciones

- Solo las de su historia: la horticultora del barrage, una funcionaria inventada y la agrónoma del zaï. *recreada · [microcosmos-famosos.md §Las historias](../microcosmos-famosos.md)*
- Con el personal del santuario: ninguna.

## 8. Horario y costumbres

- **Falta**: su microcosmos es un solo día (6-5-2024).

## 9. Microcosmos y santuario

- Microcosmos `Microcosmos_IbrahimTraore` (Uagadugú), en el piso de Huerto. *documentada · [microcosmos-famosos.md](../microcosmos-famosos.md) · [microcosmos-areas.md](../microcosmos-areas.md)*
- Su insecto: termita: las Macrotermes cultivan su propio hongo. *recreada · [microcosmos-famosos.md §Las siete](../microcosmos-famosos.md)*

## 10. En el código

| Qué | Dónde |
|---|---|
| Datos guardados (stats, ropa, rótulo) | [SampleSceneBuilder.Construction.cs:236](../../Assets/Editor/SampleSceneBuilder.Construction.cs#L236) |
| Aspecto | [CubeHair.cs:186](../../Assets/Scripts/Avatar/CubeHair.cs#L186) |
| Su historia | [StorySceneBuilder.Famosos.cs:491](../../Assets/Editor/StorySceneBuilder.Famosos.cs#L491) |
| Su pueblo | [CivicBuilder.Famosos.cs:503](../../Assets/Editor/CivicBuilder.Famosos.cs#L503) |
| Persona real (veracidad) | [LineTruth.cs:67](../../Assets/Scripts/Mission/LineTruth.cs#L67) |

## 11. Preguntas para Berón

1. Su aspecto guardado lleva uniforme de campaña y boina: ¿lo cambiamos por ropa civil, ya que su microcosmos es solo civil?
2. El lugar exacto de la entrega de tractores sigue sin verificar.
