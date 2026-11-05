# El árbol gigante, el más grande de la historia (2026-11-04; rehecho el 2026-11-05)

Berón (2026-11-05): «el árbol gigante del santuario no debe tener el tunel, solo lo comenté como dato porque quería que dicho arbol se cree con el grosor y altura mas grandes que hayan existido en el mundo ya sea de eras prehistoricas o la actualidad».

Código: `World/GuardianTree.cs`, `SampleSceneBuilder.BuildGiantTree`, `GuardianTreeTest` (grupo 91). **Sin túnel y sin guardia**: la guardia pasó a [sala-de-guardia.md](sala-de-guardia.md).

## Los récords de verdad, y cuánta seguridad hay

**Altura**

| Árbol | Altura | Seguridad |
|---|---|---|
| Abeto de **Nooksack** (Washington, 1897) | 142 m | **Discutido.** Medido con cinta tras talarlo; hay quien solo acepta que pasara de 122 m |
| Fresno de **Ferguson** (Victoria, 1872) | 132,6 m | **El que acepta Guinness.** Medido en el suelo, sin la punta y con un estudio con problemas; pudo pasar de 150 m |
| **Hyperion** (secuoya roja, viva) | 116,07 m | **Seguro.** El más alto vivo |
| Fósil: *Araucarioxylon* (Triásico) | ~59 m | **Estimado** por proporciones, no medido |
| Fósil: *Lepidodendron* (Carbonífero) | ~54 m | Estimado; no es un árbol verdadero |
| Fósil: *Paraphyllanthoxylon* (Cretácico) | ~50–54 m | Estimado |

**Grosor y volumen**

| Árbol | Medida | Seguridad |
|---|---|---|
| **Castaño de los cien caballos** (Sicilia, 1780) | 57,9 m de perímetro | Histórico, y **era una cepa de varios troncos**: no es el fuste más grueso |
| **Árbol del Tule** (vivo) | ~42 m de perímetro | Seguro |
| **Lindsey Creek** (secuoya roja, cayó en 1905) | ~2550 m³, 120 m | **Sin comprobar**, y sin fotos. Otra fuente da 1048 m³ |
| Crannell Creek | Menor que el anterior | El mayor con **fotos y medidas** |
| **General Sherman** (vivo) | 1591 m³ (2013) | **Seguro.** El mayor vivo de un solo tronco. El doc anterior decía 1487 m³ |
| Fósil: *Araucarioxylon* «Old Faithful» | 2,9 m de diámetro | Medido |

**La respuesta a la pregunta de Berón**: los mayores que conozco no son prehistóricos. Los fósiles encontrados se quedan muy por debajo, aunque los troncos fósiles casi siempre están incompletos, así que eso es «los mayores que conocemos», no «los mayores que hubo».

## Lo construido

Se toma el mayor valor registrado de cada medida, aunque no sea el más seguro, porque Berón pidió el máximo.

| Medida | Valor | De dónde |
|---|---|---|
| Base | 18,4 m de diámetro | El castaño (57,9 m) |
| Fuste | 7,7 m al empezar (antes 5,2) | General Sherman, a la altura del pecho. A 40 m mide ~7 m; Lindsey Creek, unos 5,8 m |
| Altura | **142 m**, o el edificio más alto si es mayor | Nooksack (antes 132,6 m). Hoy manda el edificio |
| Volumen del tronco | ~6000–6800 m³ | Más del doble de Lindsey Creek (calculado, no medido en escena) |

Conserva el tronco acanalado y los 12 contrafuertes del Tule, la copa estrecha de la secuoya, los colores del hielo y sus 8 panales. **No caza.**

## Lo que se quitó

- Los dos túneles de 7 × 7 m y la sala del cruce.
- La plataforma, la cabaña, las alfombras («Casa del guarda») y el tablero.
- Se llama `ArbolGigante` (antes `CasaArbolGuardia`).

## Rendimiento

Sus cubos pasan a una malla por material, como antes el bosque. Conservan los colliders. Las pruebas miden el tronco por sus colliders.

## Fuentes

- [Guinness: el más alto medido](https://guinnessworldrecords.com/world-records/66377-tallest-tree-ever-measured) · [el mayor de la historia](https://www.guinnessworldrecords.com/world-records/66381-largest-tree-ever) · [el mayor vivo](https://www.guinnessworldrecords.com/world-records/largest-living-tree-)
- [Nooksack Giant](https://en.wikipedia.org/wiki/Nooksack_Giant) · [Lista de árboles superlativos](https://en.wikipedia.org/wiki/List_of_superlative_trees) · [Lindsay Creek Tree](https://en.wikipedia.org/wiki/Lindsay_Creek_Tree) · [Crannell Creek](https://mdvaden.com/redwood_crannell_creek_giant.shtml)
- [Tall trees and taller tales (Ferguson)](https://victoriasforestsbushfireheritage.com/2023/12/10/tall-trees-and-taller-tales/)
- [Araucarioxylon arizonicum](https://en.wikipedia.org/wiki/Araucarioxylon_arizonicum) · [Lepidodendron, Universidad de Waterloo](https://uwaterloo.ca/earth-sciences-museum/educational-resources/fossils/paleozoic-fossils-ontario/lepidodendron-fossils) · [Tronco de Utah, National Geographic](https://www.nationalgeographic.com/science/article/news-petrified-log-utah-angiosperms-flowers-trees)

## Prueba (grupo 91)

Alto ≥ máx(142 m, edificio más alto); base ≥ 18 m; volumen ≥ 2550 m³; base maciza y sin pasadizos; ≥ 4 panales y ≥ 10 posaderos; no caza; T13.
