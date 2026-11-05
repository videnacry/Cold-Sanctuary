# Vida bajo el hielo y respiraderos

> **Estado actual (2026-11-05):** el santuario se separó en dos ([sitio-nuclear.md](sitio-nuclear.md) §20). Las placas miden 1,5 m, la falda de roca llega a −6 m y el mar a −16 m. Esta página describe eso.

Berón (2026-11-05) pidió que el hielo de 300 m fuese real, y eso dejó dos cosas mal: la fauna nadaba por dentro de la placa, y las focas tenían respiraderos en 300 m de hielo. Los 300 m ya no existen, pero lo que se investigó y se arregló entonces sigue.

## Cómo respiran las focas de verdad

| Hecho | Fuente |
|---|---|
| La foca de Weddell usa grietas de marea junto a la costa y huecos alrededor de icebergs atrapados, y mantiene los agujeros abiertos royendo con caninos e incisivos. El desgaste de los dientes es causa común de muerte. | [Australian Antarctic Program](https://www.antarctica.gov.au/about-antarctica/animals/seals/weddell-seal/), [Cool Antarctica](https://www.coolantarctica.com/Antarctica%20fact%20file/wildlife/weddell_seals.php) |
| Bajo el hielo marino busca grietas y agujeros, y las inmersiones más largas son buscándolos. | ídem |
| En McMurdo, los agujeros abundan en el hielo fijo anual, fino; las grietas de la plataforma, más gruesa, cuestan mucho más de mantener. | [Kim et al., McMurdo](https://archive.penguinscience.com/reprints/50%20Kim%20et%20al%20AT%20fish%20heads.pdf) |
| La foca anillada mantiene agujeros con las garras en hielo de hasta ~2 m. | [Natural Habitat](https://www.nathab.com/know-before-you-go/alaska-northern-adventures/greenland/wildlife-guide/ringed-seal-facts), [Spitsbergen-Svalbard](https://spitsbergen-svalbard.com/spitsbergen-information/wildlife/ringed-seal.html) |
| La foca barbuda se queda junto al agua abierta: canales y polinias. | [Meereisportal](https://www.meereisportal.de/en/learn-more/sea-ice-biology/sea-ice-organisms/mammals/seals) |

**Conclusión:** las focas respiran en el borde, en agua abierta y en hielo marino de hasta unos 2 m, que es lo que son hoy las placas y las láminas. No confirmé nada específico de la morsa en esto.

## Lo que queda tras la separación

- **Hielo fijo (`Hielo_fijo`):** se queda. Láminas de 1,5 m (`SeaRule.PlateThickness`; antes 2) unidas al frente de las placas, sobre la laguna. Antes eran el hielo fino frente a una plataforma; hoy miden lo mismo que las placas, y lo que las distingue es su función: **aquí hay agujeros y por debajo se nada**. Se igualó el grosor para que la lámina no sea más gruesa que la placa de la que sale. El oso anda encima y acecha; el agua sigue debajo.
- **Respiraderos:** se quedan. Agujeros reales de 1,6 m en esa lámina, a 2 m del frente, junto a la marca oscura de la grieta de marea. Se ve el agua y la foca asoma de morro. La foca anillada los mantiene en hielo de hasta unos 2 m, así que 1,5 m entra en lo real.
- **Fauna (`MarineLife`):** las placas y la isla siguen siendo **paredes** (P20). Nadie pasa por debajo: bordea, y si su destino queda detrás, se da por llegado al borde. **Con 1,5 m esto ya no es física:** un animal real bucearía bajo una placa así, y con 16 m de mar hay sitio. Se deja por diseño: las placas llevan los edificios, los troncos están abiertos al mar por debajo y así la fauna se queda en la laguna y los canales, donde se la ve. Quitarlo es cambiar `ThickAbove` por `IceAbove` en `MarineLife.Move`.
- **Bajo el hielo fijo** sí se nada, por debajo de su cara inferior. Una orca que roza con el cuerpo el borde de una placa también baja (`IceOverBody`): si no, el cuerpo entra en la placa aunque el pivote esté en agua abierta.
- **Submarino de obra:** a −12 m, por debajo de la falda de roca (−6 m) y sobre el fondo (−16 m). **Ya no hay túneles;** cómo eran y cómo rehacerlos está en [sitio-nuclear.md](sitio-nuclear.md) §23.
- **Sin modelar:** que la foca roa el hielo para mantener el agujero, ni que se le gasten los dientes. `Seal` es la foca común del catálogo: hace de la de Weddell en esto.

## La población: personas y fauna son dos números

En la ronda 9 entró el personal real (32) en `PopulationPlan.Humans`, y eso movía también la fauna en Play: el techo de reproducción subía a 1,5 × 32 y los osos podían llegar a 15. Eso contradecía el tope de memoria y dejaba bancos y plantas por debajo de lo calculado.

- **Ahora:** `FaunaScale` = 11 fija toda la fauna y la cadena alimenticia. `Humans` solo cuenta personas (comidas, verdura, prendas).
- **Se pierde:** ya no acoge más animales al llegar más gente. Era una idea escrita en `PopulationPlan`; subirla ahora es decidir subir `FaunaScale`.
