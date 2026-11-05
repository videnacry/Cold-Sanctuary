# La Residencia, un edificio-árbol en la isla (2026-11-05)

Berón: «la residencia se puede pasar a un arbol-edificio y las casas arbol quedan para otro santuary».

Código: `SampleSceneBuilder.ResidenceTower.cs`, `ResidentialBuilder.FurnishRoomFloor` y `FurnishPlaza`. Las casas de árbol: [casas-de-arbol.md](casas-de-arbol.md).

## Dónde

**Un edificio-árbol nuevo en Isla:Cabo, en (−66, −212).** No cabe en pisos libres: pide 6 y no hay ninguno.

| Opción | Por qué no |
|---|---|
| Pisos libres de una torre | Servicios ya no tiene; a Producción le sobra uno, y es el garaje del submarino |
| Un piso más en cada torre | Repartiría los dormitorios y subiría torres vecinas |
| Mover Producción a un casquete | **No hace falta y no cabe bien**: sus puntas y las de Entrenamiento o el Árbol de los Campos se quedan bajo los 16 m de S4. La isla tiene sitio de sobra, así que Producción no se mueve |

## El edificio

- 3 niveles, 6 pisos de 40 × 40 m. Techo a 131 m.
- **La plaza** (piso 1): ring de almohadas, 4 bancos de lectura para 48, red de alfombras y aseos.
- **5 pisos de habitaciones**, con 7 por piso: las 30 de antes. Cada habitación es la misma de siempre: cama-litera, tobogán, armario, esterilla y terraza frutal. Abiertas, sin muros (S6). 5 libres.
- Aseos en cada piso.

## Lo que cambia

| Tema | Ahora |
|---|---|
| **Alfombras** | Se llega como a cualquier torre: suelo o techo, y nadando. Dentro, las alfombras de habitación siguen y vuelan a la plaza. Es la única torre con alfombras en sus pisos; la prueba 57 lo admite solo para esta red interior |
| **Ring** | Su pista pasa a ser de la red interior (antes «de edificio») |
| **Pijamadas** | Igual: dependen de `Bedroom`, no del árbol |
| **Frutales** | Igual, una maceta por terraza. La ronda sigue contando un vuelo entre habitaciones: es de más, porque ahora se puede andar. La ronda puede salir larga en el cálculo |
| **Lectura** | En la plaza |
| **Viaje** | De Servicios, ~309 m en vez de ~115: unos 20 s de vuelo reales, menos de 0,15 h de juego |
| **Osos y lobos** | La cueva de los lobos queda a ~150 m y la de la osa a ~170 m. Nadie duerme a ras de suelo. La pista del pie sí está al alcance de los animales: mirarlo en vivo |
| **Alimentador y cadena alimentaria** | Sin cambios: siguen a los depredadores por todo el mapa |

## Los árboles de la isla

Son **solo flora**: 6 abedules con hasta 6 panales, en (−125, −165). No sostienen nada.

| Árbol | Altura | Nota |
|---|---|---|
| **Abedul de montaña** (*Betula pubescens*) | 4–12 m en Islandia | **El elegido**: el árbol real del borde del Ártico. Fuente: [estudio de Annals of Botany](https://pmc.ncbi.nlm.nih.gov/articles/PMC4242221) |
| Pícea de Sitka | Unos 75–85 m los mayores | [Llega hasta Kodiak](https://en.wikipedia.org/wiki/Picea_sitchensis). En Islandia, plantada, [30 m](https://grapevine.is/?p=10060434). Demasiado alta para flora de tundra |
| Lenga | Sin fuente | La búsqueda no dio datos |

**La tundra real no tiene árboles altos.** Esta isla es tundra, así que un abedul de ~13 m ya es generoso: a esa altura va en los valles abrigados. Se construyen a 13 m porque por debajo no llevan panal.

**Decisión pendiente**: Berón puede preferir la pícea.

## Riesgos

- S4 solo se comprobó a mano. Mirar la prueba 57.
- Hay vuelos de alfombra dentro de la torre, entre plaza y pisos: comprobar que no atraviesan losas.
- `FurnitureSpacing` nunca ha visto estos pisos: la prueba 28 lo dirá.
