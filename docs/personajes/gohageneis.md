# Gohageneis «el Papi»

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Gohageneis` (en la escena, `Gohageneis_Post`). Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o la
> ficha larga que contó él); **atestiguada** = se ve en su foto o captura; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = la ficha larga lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Hombre, 28 años, apodado «el Papi». *documentada (ficha) · [creature-stats.md §Gohageneis](../creature-stats.md) · [diseno-personajes.md §Fichas](../diseno-personajes.md)*
- De origen venezolano: salió de Venezuela. *documentada (ficha) · [creature-stats.md §Gohageneis](../creature-stats.md)*
- 1,76 m, atlético, pelo corto, sin barba, gorra azul con la «V» amarilla y roja de Venezuela, camiseta blanca y pantalón corto rojo. *atestiguada (su captura) · [diseno-personajes.md §Fichas](../diseno-personajes.md) · [CubeHair.cs:255](../../Assets/Scripts/Avatar/CubeHair.cs#L255)*
- Ojos: **Falta**.

## 2. Origen e historia

- Dejó su casa a los 16 buscando una buena vida. Nómada: Venezuela → Colombia → Perú → Ecuador → España → EE. UU. → santuario. *documentada (ficha) · [creature-stats.md §Gohageneis](../creature-stats.md)*
- Encadenó trabajos muy variados: mudanzas, ventas, bartender, Uber, cocina, limpieza y construcción. Dejó de estudiar a los 16. *documentada (ficha) · ídem*
- Trabajó en Five Guys, en los cierres, con Leo, Andrea y Goluis. *documentada · [character-andrea.md §Historia](../character-andrea.md), punto 4*
- Cómo y cuándo llegó al santuario: **Falta**.

## 3. Personalidad y frases

- Fiesta, baile, gimnasio y comer; percepción de calle, no académica; su rasgo es la versatilidad (se adapta muy rápido); buen temple por la vida de calle y de bartender. *documentada (ficha) · [creature-stats.md §Gohageneis](../creature-stats.md)*
- «Social, versátil, poco disciplinado». *recreada · [Archetypes.cs:174](../../Assets/Scripts/Soul/Archetypes.cs#L174)*
- Anclas de pensamiento: celebrar la vida 0,9, incluir a todos 0,85, esconder el dolor 0,7, confiar en la alegría 0,8; humor alto en reposo (0,65–0,9). *recreada · [SampleSceneBuilder.cs:3297](../../Assets/Editor/SampleSceneBuilder.cs#L3297)*
- Vivencias (voz interior): nómada («Me adapto a cualquier lugar»), oficios variados, fiesta y baile («La música me enciende»). *recreadas · [PhrasePools.cs:57](../../Assets/Scripts/Mind/PhrasePools.cs#L57)*
- Su conducta: una barra de celebración que, cerca del jugador, le da un golpe de satisfacción y le quita fatiga; con vínculo alto deja de esconder el dolor. *recreada · [Gohageneis.cs](../../Assets/Scripts/Companion/Companions/Gohageneis.cs)*
- Atención visual «Vigilante» (movimiento y cercanía). *recreada · [SampleSceneBuilder.cs:2222](../../Assets/Editor/SampleSceneBuilder.cs#L2222) · [atencion-visual.md](../atencion-visual.md)*
- Frases suyas contadas por Berón: **Falta**.

## 4. Stats, margas y aptitudes

- Cuerpo: altura 1,0 · agilidad 1,2 · percepción 1,05 · fuerza 1,1 · masa 1,1 · resistencia 1,3 · adaptabilidad 1,7. *recreada (de su ficha) · [Archetypes.cs:140](../../Assets/Scripts/Soul/Archetypes.cs#L140) · [creature-stats.md §Perfiles de humanoides](../creature-stats.md)*
- Mente: temple 1,2 · razonamiento 1,0 · memoria 1,0 · creatividad 1,2 · sociabilidad 1,7 · disciplina 0,6; tono Fuego. *recreada · [Archetypes.cs:174](../../Assets/Scripts/Soul/Archetypes.cs#L174)*
- Margas: **Falta**.

## 5. Profesión y lo que sabe hacer

- Guardia y maestro de baile. *documentada · Berón 2026-11-04 · [personajes-mesocosmos.md §Profesiones y parejas](../personajes-mesocosmos.md) · [Professions.cs:120](../../Assets/Scripts/Progression/Professions.cs#L120)*
- Artes marciales: **sin base de Berón**. Salió del reparto provisional al irse Goku («ágil y atlético, y ya es de la Guardia»). La experta documentada es Luciana. *inventada · [Professions.cs:146](../../Assets/Scripts/Progression/Professions.cs#L146) · [reglas-personajes.md §Auditoría de puestos](../reglas-personajes.md)*
- Por su vida sabe de mudanzas, ventas, bar, conducir, cocina, limpieza y construcción. *documentada (ficha) · [creature-stats.md §Gohageneis](../creature-stats.md)*. El juego no usa nada de eso.

## 6. Puesto y áreas

- Profesión en el código: «guardia, artes marciales y maestro de baile»; áreas Guardia y Sala de guardia. *código · [Professions.cs:40](../../Assets/Scripts/Progression/Professions.cs#L40) · [sala-de-guardia.md](../sala-de-guardia.md)*
- Patrulla (`CarnivoreKeeper`) sin área fija: no está en `HomeArea`. *código · [SampleSceneBuilder.cs:3303](../../Assets/Editor/SampleSceneBuilder.cs#L3303) · [SanctuaryDirector.cs:160](../../Assets/Scripts/World/SanctuaryDirector.cs#L160)*
- Baile: se turna por días con Panterilia. Artes marciales: con Luciana, los días contrarios. *código · [Professions.cs:86](../../Assets/Scripts/Progression/Professions.cs#L86) · [personajes-mesocosmos.md §Maestros de baile](../personajes-mesocosmos.md)*
- En la brigada de la prueba del restaurante va en el pase; su cuerpo es el del camarero con turistas. *código · [RestaurantServiceTest.cs:40](../../Assets/Scripts/Tests/RestaurantServiceTest.cs#L40) · [RestaurantServiceGame.cs:28](../../Assets/Scripts/Kitchen/RestaurantServiceGame.cs#L28)*

## 7. Relaciones

- Compañero de Five Guys de Leo, Andrea, Goluis y Panterilia. *documentada · [character-andrea.md §Historia](../character-andrea.md)*
- Tiene vínculo con Goluis en el sistema de vínculos. *recreada · [review-checklist.md](../review-checklist.md)*
- Familia y pareja: **Falta**. Vive solo en su casa del árbol ([ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16)).

## 8. Horario y costumbres

- Gimnasio: empuje/tirón/pierna de fuerza lunes, miércoles y viernes («el resto, su dojo»). *recreada · [GymProgramming.cs:206](../../Assets/Scripts/Progression/GymProgramming.cs#L206) · [gimnasio-maquinas.md](../gimnasio-maquinas.md) §15*
- Horario libre: guía el baile de las 20 h el día que le toca, y «Artes marciales con Gohageneis y Luciana». *código · [LeisureDirector.cs:178](../../Assets/Scripts/Progression/LeisureDirector.cs#L178) · [horario-libre.md](../horario-libre.md)*
- En su tiempo libre la patrulla espera (antes no llegaba al ring). *código · [CarnivoreKeeper.cs:27](../../Assets/Scripts/World/CarnivoreKeeper.cs#L27) · [guerra-de-almohadas.md](../guerra-de-almohadas.md)*

## 9. Microcosmos y santuario

- En los microcosmos es un **saltamontes** (nómada: la langosta migratoria). *recreada · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Es la persona por defecto de las misiones virtuales de área. *código · [VirtualMissionSceneBuilder.cs:147](../../Assets/Editor/VirtualMissionSceneBuilder.cs#L147)*
- Nivel: de los creados antes de Linduarda, Irosene y Lizmibet, que Berón (2026-11-08) cree de santuario 1, nivel 1. *documentada (nota de Berón)*. Su santuario y su nivel exactos: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Cuerpo, anclas y conducta en la escena | [SampleSceneBuilder.cs:3294](../../Assets/Editor/SampleSceneBuilder.cs#L3294) (`BuildCharacters`) |
| Su conducta (celebración) | [Gohageneis.cs](../../Assets/Scripts/Companion/Companions/Gohageneis.cs) |
| Arquetipo | [Archetypes.cs:140](../../Assets/Scripts/Soul/Archetypes.cs#L140) · [Archetypes.cs:174](../../Assets/Scripts/Soul/Archetypes.cs#L174) |
| Vivencias | [PhrasePools.cs:57](../../Assets/Scripts/Mind/PhrasePools.cs#L57) |
| Aspecto | [CubeHair.cs:256](../../Assets/Scripts/Avatar/CubeHair.cs#L256) |
| Profesión, base y clases | [Professions.cs:40](../../Assets/Scripts/Progression/Professions.cs#L40) · [Professions.cs:86](../../Assets/Scripts/Progression/Professions.cs#L86) · [Professions.cs:120](../../Assets/Scripts/Progression/Professions.cs#L120) |
| Gimnasio | [GymProgramming.cs:206](../../Assets/Scripts/Progression/GymProgramming.cs#L206) |
| Personal y casa | [WorkdayDirector.cs:19](../../Assets/Scripts/Progression/WorkdayDirector.cs#L19) · [ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16) |

## 11. Preguntas para Berón

1. ¿Da artes marciales, o las dirige Luciana (la experta documentada) y él solo guardia y baile?
2. ¿Cómo y cuándo llegó al santuario? ¿Familia, pareja?
3. ¿Su elemento es Agua o Fuego? Las vivencias dicen «Gohageneis → Agua» ([PhrasePools.cs:14](../../Assets/Scripts/Mind/PhrasePools.cs#L14)) y su arquetipo, Fuego ([Archetypes.cs:174](../../Assets/Scripts/Soul/Archetypes.cs#L174)).
4. ¿En la captura lleva pantalón corto o bañador rojo? El doc dice «pantalón corto» y el comentario del código, «bañador» ([CubeHair.cs:255](../../Assets/Scripts/Avatar/CubeHair.cs#L255)).
5. ¿Algún oficio de su pasado (cocina, construcción, bar) debe contar como sabido (J31)?
