# Vida en los icebergs

Berón (2026-11-05): «los iceberges deben poderse poblar por animales reales, imagino que existe una razón/lógica que lleva a que algunos animales vayan a icebergs y se queden por momento o indefinidamente».

## Por qué van, según lo investigado

| Quién | Por qué | Cuánto | Fuente |
|---|---|---|---|
| Foca común, Alaska | Al criar, en icebergs lentos y estables; al mudar, en los que se mueven rápido junto a la pluma del glaciar, que trae plancton y peces | Por temporadas | [Glacier Bay, 2024](https://ecomagazine.com/news/research/seals-strategically-use-icebergs-as-safe-platforms/) |
| Pinnípedos en general | Descanso, temperatura, evitar depredadores, muda, cría y lactancia | Según el motivo | [Hauling out](https://en.wikipedia.org/wiki/Hauling_out) |
| Kril y aves | Más clorofila, kril y aves hasta ~3,7 km de cada iceberg, por el hierro y el material que sueltan | Mientras dura el iceberg | [Smith et al. 2007, *Science* 317:478](https://scholarcommons.sc.edu/chem_facpub/308) |
| Petrel de las nieves | Asociado a hielo a la deriva e icebergs; se ven bandadas posadas en ellos | Se posa; anida en tierra, en grietas | [Australian Antarctic Program](https://www.antarctica.gov.au/about-antarctica/animals/flying-birds/petrels-and-shearwaters/snow-petrel/) |
| Pingüinos | Descansar y huir de la foca leopardo | — | **Sin fuente sólida**: solo pies de foto de stock. Es verosímil y se deja marcado |

**Indefinidamente, nadie.** Ninguna fuente muestra animales que vivan siempre en un iceberg: se derrite y se mueve. Lo que dura es la zona rica de alrededor. Las horas de estancia son valores de diseño.

## Lo que se ha hecho

- **Fuera las figuras de foca.** En su lugar, 4 focas y 4 pingüinos de Adelia de verdad, `Animal` completos, repartidos entre los dos icebergs.
- **`Repisa_iceberg`:** las paredes miden 24–30 m y nadie las escala, así que cada uno lleva un pie de hielo a ras de agua con su descansadero.
- **Va:** tras comer o de noche, igual que en la laguna.
- **Se queda (`IcebergVisitor.Stay`):**
  - foca, en primavera, en el varado: 72 h, cuidado de crías;
  - foca, en verano, en el que deriva: 48 h, muda;
  - el resto, y los pingüinos: 6–8 h, descanso.
- **Se va:** al acabar, o antes si tiene hambre.
- **Deriva con él:** mientras descansa, va con el iceberg.
- **Comida:** a cada iceberg lo sigue un banco de peces además del kril de antes, que es lo que dice el estudio de Smith.
- **`FoodChain`:** cuenta a los visitantes y suma esos bancos.

## Lo que no

- **No crían aquí.** Con el motivo «cuidado de crías» solo se queda más: no hay parto. Evita ciclos de vida lejos de todo.
- **Sin depredadores en los icebergs:** las orcas siguen en la laguna, así que nadie huye. Por eso el motivo «refugio» no está.
- **Los petreles ya son reales.** `SnowPetrel` vuela (`SnowPetrelLife`), se posa unas horas arriba del iceberg y va con él; anida en la roca de la isla, nunca en el hielo.
- **Coste:** 8 `Animal` más, 2 bancos más y 2 repisas, sin NavMesh, y `Lod` los trata como lejanos. Se puede vigilar en la primera reconstrucción.
