# Vida bajo el hielo, respiraderos y túneles de la isla

> **2026-11-05:** el santuario se separó en dos ([sitio-nuclear.md](sitio-nuclear.md) §20). Las placas vuelven a 1,5 m y la roca a una falda: **los túneles ya no existen** y el submarino pasa por debajo, a −12 m. Fauna, hielo fijo y respiraderos siguen como aquí.

Berón (2026-11-05) pidió que el hielo de 300 m fuese real, y eso dejó dos cosas mal ([sitio-nuclear.md](sitio-nuclear.md) §13): la fauna nadaba por dentro de la placa, y las focas tenían respiraderos en 300 m de hielo.

## Cómo respiran las focas de verdad

| Hecho | Fuente |
|---|---|
| La foca de Weddell usa grietas de marea junto a la costa y huecos alrededor de icebergs atrapados, y mantiene los agujeros abiertos royendo con caninos e incisivos. El desgaste de los dientes es causa común de muerte. | [Australian Antarctic Program](https://www.antarctica.gov.au/about-antarctica/animals/seals/weddell-seal/), [Cool Antarctica](https://www.coolantarctica.com/Antarctica%20fact%20file/wildlife/weddell_seals.php) |
| Bajo el hielo marino busca grietas y agujeros, y las inmersiones más largas son buscándolos. | ídem |
| En McMurdo, los agujeros abundan en el hielo fijo anual, fino; las grietas de la plataforma, más gruesa, cuestan mucho más de mantener. | [Kim et al., McMurdo](https://archive.penguinscience.com/reprints/50%20Kim%20et%20al%20AT%20fish%20heads.pdf) |
| La foca anillada mantiene agujeros con las garras en hielo de hasta ~2 m. | [Natural Habitat](https://www.nathab.com/know-before-you-go/alaska-northern-adventures/greenland/wildlife-guide/ringed-seal-facts), [Spitsbergen-Svalbard](https://spitsbergen-svalbard.com/spitsbergen-information/wildlife/ringed-seal.html) |
| La foca barbuda se queda junto al agua abierta: canales y polinias. | [Meereisportal](https://www.meereisportal.de/en/learn-more/sea-ice-biology/sea-ice-organisms/mammals/seals) |

**Conclusión:** a través de una plataforma de cientos de metros no respira ninguna foca. Respiran en el borde, en agua abierta y en hielo marino fino. No confirmé nada específico de la morsa en esto.

## Lo que se ha hecho

- **Hielo fijo (`Hielo_fijo`):** láminas de 2 m unidas al frente de la plataforma, sobre la laguna, como el hielo fijo real. El oso anda encima y acecha; el agua sigue debajo.
- **Respiraderos:** agujeros reales de 1,6 m en esa lámina, a 2 m del frente, junto a la marca oscura de la grieta de marea. Se ve el agua y la foca asoma de morro.
- **Fauna (`MarineLife`):** la plataforma y la isla son **paredes**. Nadie pasa por debajo cerca de la superficie: bordea, y si su destino queda detrás, se da por llegado al borde. Bajo el hielo fijo sí se nada, a la profundidad de su cara inferior.
- **Sin modelar:** que la foca roa el hielo para mantener el agujero, ni que se le gasten los dientes. `Seal` es la foca común del catálogo: hace de la de Weddell en esto.

## Túneles bajo la isla

La isla es un anillo de roca hasta el lecho, y la ruta del submarino de obra es una recta entre árboles a −312 m. `BuildFloes` abre un túnel de 18 m de alto y otros tantos de ancho donde cada recta cruza el anillo. Es un corte limpio en la roca, sin revestir. El interior del anillo sigue siendo una cavidad inundada.

## La población: personas y fauna son dos números

En la ronda 9 entró el personal real (32) en `PopulationPlan.Humans`, y eso movía también la fauna en Play: el techo de reproducción subía a 1,5 × 32 y los osos podían llegar a 15. Eso contradecía el tope de memoria y dejaba bancos y plantas por debajo de lo calculado.

- **Ahora:** `FaunaScale` = 11 fija toda la fauna y la cadena alimenticia. `Humans` solo cuenta personas (comidas, verdura, prendas).
- **Se pierde:** ya no acoge más animales al llegar más gente. Era una idea escrita en `PopulationPlan`; subirla ahora es decidir subir `FaunaScale`.
