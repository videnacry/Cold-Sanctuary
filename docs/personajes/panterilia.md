# Panterilia

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Panterilia` (en la escena, `Panterilia_Post`). Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Mujer, 42 años. «El personaje de la percepción». *documentada · [creature-stats.md §Panterilia](../creature-stats.md)*
- Delgada. *documentada · ídem*
- 1,68 m, melena leonada, ojos ámbar (el toque del león), ropa verde oliva. *inventada: **sin foto ni referencia** · [diseno-personajes.md §Fichas](../diseno-personajes.md) · [CubeHair.cs:252](../../Assets/Scripts/Avatar/CubeHair.cs#L252)*
- Nacionalidad: **Falta** ([diseno-personajes.md §Pendiente](../diseno-personajes.md)).

## 2. Origen e historia

- Nunca dejó de estudiar y ha tenido muchos puestos y papeles; lejos del trabajo físico, su fuerza ha bajado. *documentada · [creature-stats.md §Panterilia](../creature-stats.md)*
- En Five Guys hacía más aperturas que cierres. *documentada · [character-andrea.md §Historia](../character-andrea.md) 4*
- Cómo llegó al santuario: **Falta**.

## 3. Personalidad y frases

- Atención al detalle muy alta y cierta lógica de negocio. *documentada · [creature-stats.md §Panterilia](../creature-stats.md)*
- Bajo estrés agudo tiende a bloquearse. *documentada · ídem*
- Libera mucha energía mental: puede exagerar la realidad en su mente y se deja influir por ideas ajenas a su experiencia (teorías, imaginación de otros). Aún sin mecánica. *documentada · ídem*
- Anclas: `work_hard` 0,85, `protect_daughter` 0,95, `trust_nature` 0,3 (crece), `chemical_reliance` 0,6 (se desvanece). *recreada · [SampleSceneBuilder.cs:3321-3324](../../Assets/Editor/SampleSceneBuilder.cs#L3321) · [Panterilia.cs:3](../../Assets/Scripts/Companion/Companions/Panterilia.cs#L3)*. **Ninguna ficha habla de una hija ni de dependencia química**: esas dos anclas no tienen base escrita.
- Vivencias: estudiar sin parar, el detalle, bloquearse, exagerar. *recreada · [PhrasePools.cs:44-55](../../Assets/Scripts/Mind/PhrasePools.cs#L44)*
- Frases de diálogo propias: **Falta**.

## 4. Stats, margas y aptitudes

- Las 12 aptitudes: percepción 1,7, fuerza 0,7, masa 0,8, agilidad 0,95, adaptabilidad 1,4, temple 0,7, resistencia 0,9, razonamiento 1,6, memoria 1,4, creatividad 1,4, sociabilidad 1,1, disciplina 1,5. *recreada · [creature-stats.md §Perfiles de humanoides](../creature-stats.md) · [Archetypes.cs:138](../../Assets/Scripts/Soul/Archetypes.cs#L138) · [Archetypes.cs:172](../../Assets/Scripts/Soul/Archetypes.cs#L172)*
- Alma: 90 % humana con un 5 % de león. *documentada (ficha) · [soul-composition-blend.md](../soul-composition-blend.md) · [diseno-personajes.md §Fichas](../diseno-personajes.md)*
- Tono elemental: Viento. *recreada · [Archetypes.cs:172](../../Assets/Scripts/Soul/Archetypes.cs#L172)*
- Margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Maestra de baile y finanzas (la mesa de cuentas del restaurante, `coc_cuentas`). *documentada (Berón 2026-11-04) · [Professions.cs:42](../../Assets/Scripts/Progression/Professions.cs#L42) · [Professions.cs:123](../../Assets/Scripts/Progression/Professions.cs#L123)*
- Guía el baile un día de cada dos, con Gohageneis. *código · [Professions.cs:88](../../Assets/Scripts/Progression/Professions.cs#L88)*
- Su ficha antigua: dirige la limpieza y la farmacia (nivel 2). *recreada, sin base de Berón: `Professions.Unconfirmed` · [world-simulation.md](../world-simulation.md) · [area-missions-spec.md](../area-missions-spec.md) · [Professions.cs:157](../../Assets/Scripts/Progression/Professions.cs#L157)*
- Fue segunda maestra de yoga hasta el 2026-11-05; se quitó por no tener base (P7). *documentada · [reglas-personajes.md §Auditoría](../reglas-personajes.md)*
- Sube el radio de observación del jugador (+1,5) mientras está cerca. *código · [Panterilia.cs](../../Assets/Scripts/Companion/Companions/Panterilia.cs) · [observation-system.md](../observation-system.md)*

## 6. Puesto y áreas

- Sale en el huerto, por la farmacia, que no tiene área. *código · [SanctuaryDirector.cs:160](../../Assets/Scripts/World/SanctuaryDirector.cs#L160)*
- En la brigada del restaurante, en el obrador con Adaguchi. *recreada · [personajes-mesocosmos.md §La jornada](../personajes-mesocosmos.md)*
- Su profesión le da `coc_cuentas` en la Cocina; el baile, en la Sala de Ballet cuando esté terminada. *código · [Professions.cs:42](../../Assets/Scripts/Progression/Professions.cs#L42)*

## 7. Relaciones

- Compañera de Five Guys de Leo, Andrea, Goluis y Gohageneis. *documentada · [character-andrea.md §Historia](../character-andrea.md)*
- Irosene la visita en Limpieza. *documentada · [character-irosene.md §Lugar en el santuario](../character-irosene.md)*
- Una hija: solo en el ancla `protect_daughter` ([SampleSceneBuilder.cs:3322](../../Assets/Editor/SampleSceneBuilder.cs#L3322)). *inventada, sin fuente escrita*
- Pareja: **Falta**. Vive sola en su casa del árbol ([ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16)).

## 8. Horario y costumbres

- Gimnasio: cuerpo entero, martes, jueves y sábado, para ganar músculo. *recreada · [GymProgramming.cs:205](../../Assets/Scripts/Progression/GymProgramming.cs#L205) · [gimnasio-maquinas.md §15](../gimnasio-maquinas.md)*
- Horario libre: guía «Bailar» a las 20 h los días que le tocan. *código · [personajes-mesocosmos.md §Maestros de baile](../personajes-mesocosmos.md)*
- Estudia sin parar. *documentada · [creature-stats.md](../creature-stats.md)*

## 9. Microcosmos y santuario

- En los microcosmos es una **hormiga león** (la percepción, el acecho del león). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- En la observación: primera misión en la que ella detecta plantas e insectos; en el nivel 2, formadora. *recreada · [observation-system.md](../observation-system.md)*
- Cámara del «modo contemplativo» a su lado. *recreada · [camera-system.md](../camera-system.md)*
- Por la nota de Berón del 2026-11-08, probablemente santuario 1, nivel 1. *documentada (nota de Berón; «probablemente»)*

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo, alma y anclas | [SampleSceneBuilder.cs:3317](../../Assets/Editor/SampleSceneBuilder.cs#L3317) |
| Su conducta (observación) | [Panterilia.cs](../../Assets/Scripts/Companion/Companions/Panterilia.cs) |
| Arquetipo | [Archetypes.cs:138](../../Assets/Scripts/Soul/Archetypes.cs#L138) · [Archetypes.cs:172](../../Assets/Scripts/Soul/Archetypes.cs#L172) |
| Vivencias | [PhrasePools.cs:44](../../Assets/Scripts/Mind/PhrasePools.cs#L44) |
| Aspecto | [CubeHair.cs:253](../../Assets/Scripts/Avatar/CubeHair.cs#L253) |
| Profesión, base y lo sin confirmar | [Professions.cs:42](../../Assets/Scripts/Progression/Professions.cs#L42) · [Professions.cs:123](../../Assets/Scripts/Progression/Professions.cs#L123) · [Professions.cs:157](../../Assets/Scripts/Progression/Professions.cs#L157) |
| Maestra de baile | [Professions.cs:88](../../Assets/Scripts/Progression/Professions.cs#L88) |
| Personal, salida, gimnasio y casa | [WorkdayDirector.cs:19](../../Assets/Scripts/Progression/WorkdayDirector.cs#L19) · [SanctuaryDirector.cs:160](../../Assets/Scripts/World/SanctuaryDirector.cs#L160) · [GymProgramming.cs:205](../../Assets/Scripts/Progression/GymProgramming.cs#L205) · [ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16) |

## 11. Preguntas para Berón

1. Su foto o una referencia de su aspecto, y su nacionalidad.
2. ¿Tiene una hija? El código le pone el ancla «proteger a su hija», sin nada escrito.
3. ¿Y la «dependencia química» del código, de dónde sale? ¿Se quita?
4. ¿Sigue con la limpieza y la farmacia? Por eso sale en el huerto.
5. ¿Cómo llegó al santuario?
