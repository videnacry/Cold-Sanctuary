# El árbol cazador, bajo el agua (2026-11-05)

Berón: «el árbol cazador queda solo en un piso específico de investigación de modo que conserva todas sus características» y «quizás el piso del árbol carnivoro se podría realizar bajo el agua? el enjambre no podría salir de alli […] se supone que el árbol se está diseñando para ser autosustentable y require de las abejas y arañas para que asi sea no?».

Código: `World/HunterCell.cs` (`HunterRule`, `HunterLoop`, `HunterCell`, `HunterLab`), `SampleSceneBuilder.HunterLab.cs`. Regla S16. Prueba: grupo 91.

## Dónde

**En el Santuario Marino**, en una sala propia en la roca, a 46 m de las salas nucleares por una galería, con 60 m de roca encima.

| Opción | Por qué |
|---|---|
| **Santuario Marino** | Elegida. Solo se carga cuando baja Kushal, así que el árbol, las colmenas y los bichos no pesan arriba. Ya tiene línea, refugio y esclusas |
| Cámara sumergida en el santuario 1 | Pesaría siempre, y un fallo daría al mar junto a las torres |
| Lo alto de Producción | Era la anterior. Se quita: la torre vuelve a 2 niveles |

## Las puertas al mar: lo que es verdad y lo que no

| Palabra | La verdad |
|---|---|
| **Moon pool** | Es una abertura al agua en el suelo, como en el hábitat Aquarius: el aire de dentro, a la presión del mar, sujeta el agua. **Aquí no se puede**: a 57 bar nadie vive en esa presión. Fuente: [Moonpool](https://www.wikiwand.com/en/articles/Moonpool) |
| **Cámara de salida** | Lo que sí se usa: la sala a una atmósfera, y una cámara que se inunda para salir. Es lo construido |
| **«Puertas magnetizadas»** | En el juego, el `CampoMagnetico` de las puertas de los troncos es una lámina que sujeta el agua. **Eso es ficción**: un campo magnético no sujeta el agua de mar. Lo que existe son cierres y acoplamientos magnéticos en una escotilla. Aquí es eso: una escotilla con cierre magnético. No encontré fuente sobre ellos: **[verificar]** |
| **El agua como barrera** | Es lo que para al enjambre: la salida es un pozo lleno de agua. Una abeja no vuela por el agua. Es razonamiento, no un dato con fuente |

## De dentro afuera

Celda de cristal → antesala → esclusa de aire → cámara de salida inundada → pozo inundado al lecho.

Un enjambre puede llegar a la antesala en una brecha, y ahí acaba. Lo real ya exige más que un cristal: [en invernaderos se escapan abejorros por puertas de doble red](https://conservationevidencejournal.com/actions/40), y por eso la celda sola no basta.

## Lo que conserva

Señuelo de 70 m, sopor letal de 12 m, **cualquier presa** menos sus abejas y arañas, crecimiento a ×1,5, **3 panales y 2 arañas**. El cuerpo sigue en 13 m: a 21 m de celda es lo que cabe.

El sopor deja fuera a abejas y arañas. Es un cambio: en el bosque nada las excluía, y aquí se comerían su propio ciclo.

## El ciclo cerrado (`HunterLoop`)

Cadáveres → nutrientes → néctar y miel → polinización → crecimiento. Lo que muere vuelve al suelo, y las arañas comen abejas. Corre arriba, un día cada vez, aunque nadie baje.

- **Sí es autosuficiente, casi**: el modelo aguanta 60 días sin ración, con colonia llena. Recicla el 97 %, porque nada recicla el 100 %: los nutrientes bajan despacio, y **solo entonces** aparece `cuar_racion`.
- **Las abejas hacen falta de verdad**: sin ellas, no hay polinización ni crece.
- **Son cifras de juego**, no un modelo biológico.
- Las abejas buitre existen y comen carroña: de memoria, **[verificar]**.
- La luz: a esa hondura no llega el sol, así que tiene luces de cultivo.

## Las normas

| Norma | Origen | Fuente |
|---|---|---|
| Acceso restringido, descontaminar lo que sale y manual de prácticas | BL3-P | [Guías NIH, Apéndice P](https://webarchive.library.unt.edu/eot2008/20090131001310/http:/oba.od.nih.gov/oba/rac/guidelines_02/APPENDIX_P.htm) |
| Nadie comparte espacio con él | Contacto protegido | [Cheyenne Mountain Zoo](https://cmzoo.org/news/archive/tag/zoo-information/page/7) |
| Doble puerta | Zoos | [Cleveland Zoo](https://www.clevelandzoosociety.org/Z/2020/03/16/ask-the-expert-how-do-the-keepers-get-into-the-habitats-without-the-animals-coming-out) |
| Simulacros y código de escape | Zoos | [ASIS](https://www.asisonline.org/link/db5bf6ea36204592be7c784764586bf3.aspx) |

**[verificar]**: la presión negativa no salió en las búsquedas. El simulacro semanal es elección mía.

## Misiones

La lógica vive arriba, en `HunterCell`, junto a la línea, como `CavernRock`. Sin cargar la escena. Quien baja usa `SiteLine.Board`, como `nuc_bajada`, y Kushal es el único que carga lo de abajo.

| Misión | Dónde | Cuándo |
|---|---|---|
| `cuar_ronda` (obligatoria) | **Sala de guardia**, en remoto | Cada día |
| `cuar_racion` | Línea | Si faltan nutrientes |
| `cuar_muestra` | Línea | Semanal: poda |
| `cuar_simulacro` | Línea | Semanal |
| `cuar_recaptura` y `_2` | Línea | Solo en una brecha |

Si la línea está cerrada, solo queda la ronda.

## La emergencia: código verde

- **Empieza** tras más de 3 días sin ronda, o por un fallo de sello, cada ~60 días desde el día 30.
- **Mientras dura**: el árbol alcanza a cualquiera en la sala y el enjambre sale a la antesala, sin pasar del agua. La pantalla de la sala de guardia se pone roja, y solo queda la recaptura.
- **Acaba** con **dos personas**, o al día siguiente, porque las compuertas cierran solas. Eso cubre el caso de la línea cerrada.

El piso ya no da miedo arriba: **una brecha no llega al santuario 1**. Por eso el árbol nunca alcanza a nadie si Kushal no está abajo.

## Riesgos

- Sin probar en vivo.
- Las colmenas y bichos de abajo solo corren con la escena cargada. Lo que se ve no cuadra exactamente con `HunterLoop`: la pantalla manda.
- Kushal puede andar hasta la sala, pero sus tareas son las genéricas: aún no hay interacción con portillo y guantes.
- La sala no entra en el control de la roca (`CavernRock`), y su pilar con las salas nucleares es una decisión a ojo.
- Una celda de 21 m de cristal a 57 bar no sería de cristal: va dentro de la sala a una atmósfera, que es la que aguanta.
