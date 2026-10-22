# Egina, el reino de los mirmidones (microcosmos de la Construcción)

Escena `Microcosmos_Egina` (`Assets/Editor/MythSceneBuilder.cs`, menú *Tools/Cold Sanctuary/Build Egina (mirmidones)*;
también la genera el blockout). Se entra por la máquina de virtualización de la **Construcción**: «Simulación: Egina, el
reino de los mirmidones». Como todo microcosmos, es **el área del santuario agrandada ×10** (aquí la obra: levantar un
reino es construir), con la historia en un hueco libre (`AreaMicrocosmos.Build(..., storyOverride)`).

## La historia (versión humana, pedida por Berón 2026-10-22)

Éaco, un insecto-humano joven, sueña bajo la encina con un **reino lleno de virtud y grandeza**. Su padre, **Zeus**, no
hace magia: es alguien con **muchos contactos e influencia** que, conmovido por el sueño de su hijo, va a ver a cada
insecto-humano que conoce y le pide que se una y trabaje duro **por el bien común**. Unidos, traen piedra de la cantera
y alzan un templo dórico. Al terminarlo, Éaco es «rey de Egina» y dice: «No es mío: es de todos los que lo levantaron».

## Mecánica

| Pieza | Qué hace |
|---|---|
| `MythMission` | HUD (unidos / etapa del templo), **E** junto a alguien que aún no se ha unido = hablarle (se une o da su motivo), final: XP de Vínculos + tarjeta. |
| `MythRecruiter` (Zeus) | Va solo al más cercano que esté dispuesto, le habla 3 s y lo suma; si nadie lo está, vuelve a la encina. |
| `MythRecruit` | Condición para unirse: fracción del pueblo ya unida (`joinsAtFraction`) o etapa del templo (`joinsAtStage`). Al unirse: cargadores → `MicroForager` (cantera → obra); el resto → su puesto. |
| `GreekBuildSite` | `MicroLair` de la obra: cada bloque entregado suma; etapas crepis (10) → columnas (35) → cella (50) → entablamento (65) → frontón y tejado (80). |
| `HistoryCard` | Intro al entrar y, al final, el mito y el templo reales con fuentes (compartida con las simulaciones de compasión). |

## El elenco (reencarnaciones de la enfermería, mismos nombres)

| Personaje | Papel en Egina | Condición |
|---|---|---|
| Ambrosio | cargador («yo ya subía grano por la encina») | desde el principio |
| Atlas | cargador fuerte (más rápido) | desde el principio |
| Héspero | guarda el **hogar común** (pritaneo) | desde el principio |
| Ruth | sube tierra a las **terrazas** | desde el principio |
| Sakshi | escribe las **leyes** justas | cuando se ha unido ¼ |
| Shanidar | anciano: enseña en la **stoa** | desde el principio |
| Medea | jefa de clan, cargadora | cuando se ha unido la mitad («mi clan no trabaja para el sueño de otro») |
| Momo | el bromista, cargador | cuando se alzan las **columnas** («¡primero que Zeus cargue una piedra!») |
| 6 mirmidones | cargadores | algunos esperan a ¼ o ½ |

## Lo real (para la tarjeta final)

- **Mito**: Ovidio, *Metamorfosis* VII 517–660: tras una peste, Éaco reza junto a una encina nacida de una semilla de
  Dodona, por la que suben hormigas con grano; pide tantos ciudadanos como hormigas; sueña que crecen y se ponen en pie;
  los llama mirmidones (*mýrmēx*, hormiga). «Trabajan con paciencia, guardan el fruto de su trabajo y lo defienden con
  valor». Versiones: Apolodoro, *Biblioteca* III 12, 6; Higino, *Fábulas* 52. Éaco, por justo, juez de los muertos;
  los mirmidones siguen luego a Aquiles. (El motivo de Hera para la peste viene de otras fuentes, no del pasaje de Ovidio.)
- **Templo de Afaya** (Egina, c. 500–490 a. C.): dórico períptero 6 × 12 columnas, estilóbato 28,8 × 13,7 m,
  columna = 5,33 × diámetro de la base, caliza local estucada, tejas de arcilla. En la escena: diámetro 1 m
  (**supuesto**, para que la columna mida 5,33 m), crepis de 3 escalones de 0,35 m, tejado a 15°, frontón escalonado.

Fuentes: [Ovidio, Met. VII (trad. Brookes More, Perseus)](https://fgh.perseids.org/read/latinLit/phi0959/phi006/perseus-eng3/7.552-7.661) ·
[Aegina (mythology), Wikipedia](https://en.wikipedia.org/wiki/Aegina_(mythology)) ·
[Temple of Aphaia, Brown/Joukowsky Institute](https://webhelper.brown.edu/joukowsky/courses/greekpast/4775.html) ·
[Temple of Athena Aphaia, Athens & Attica](https://athensattica.com/point/temple-of-athena-aphaia)

## Pendiente

- Quitones y corona de encina: ahora son cubos sencillos sobre `CubeInsect.DressMyrmidon`; que el hilo de Unity los
  integre en el cuerpo de cubos.
- La encina con hormigas subiendo grano es decorado: podría ser un `MicroForager` que trepe (`Climbable`).
