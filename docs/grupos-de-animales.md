# Grupos de animales: peces, kril, medusas y abejas (Berón 2026-11-03)

Pedido de Berón: «Los peces ahora mismo creo que parecen plataformas: deberían tener la apariencia de peces. Se debe crear cada pez
integrante y a todos agruparlos en un GameObject, de modo que van todos juntos y se cumple la lógica de grupo donde un depredador
caza al GameObject contenedor y el daño hace que desaparezca uno de los peces, y cada cierto tiempo incrementa el número de peces
(la lógica para que los peces no tengan ciclo de vida y optimizar el juego). Revisa si está bien implementada. La misma lógica se usa
en los kril… A las abejas del mesocosmos también… serían más visibles al verlas volar en grupo de un árbol a otro; deben volar de un
árbol a otro durante el día. ¿Existen medusas en los polos? También se las podría crear como grupos… y también podrían entrar en
los árboles-edificio.»

Código: `Assets/Scripts/World/Swarm.cs` (el grupo), `SwarmModels.cs` (los individuos), `BeeSwarm.cs` (el enjambre de una colmena),
constructores en `SampleSceneBuilder.cs` (`BuildSwarms`, `BuildKrill`, `MiniSwarm`), `SampleSceneBuilder.CicloVida.cs` (`School`) y
`SampleSceneBuilder.Masks.cs` (`BottomLife`, los de debajo de cada árbol). Prueba: `GroupAnimalsTest` (grupo 71).

## Auditoría (cómo estaba)

| Qué | Antes | Ahora |
|---|---|---|
| El padre del grupo | Un **cubo translúcido** escalado 10 × 2 × 10 m (8 × 1,5 × 8 el kril) con su malla: **se veía como una plataforma** | Un punto vacío a escala 1, sin malla (los grupos de escenas viejas se arreglan solos al empezar) |
| Los individuos | Cubos de 0,4 que **heredaban la escala del padre**: losas de 4 × 0,8 × 4 m | Modelos low-poly de su especie (`SwarmModels`), mallas compartidas |
| El trigger del mordisco | Esfera de radio 4 × escala 10 = **40 m** | Del tamaño del grupo (3–5 m) |
| Un mordisco | `count − 1`, pero cada individuo valía 5 de la cuenta: **ningún pez desaparecía a la vista** hasta el 5.º mordisco | Un individuo por unidad: **un mordisco = un individuo menos, al momento** |
| Movimiento | El padre derivaba; los hijos iban con él (bien), sin orientarse | El grupo se orienta hacia donde va; cada individuo con su sitio, su vaivén y su gesto (coleteo, latido, aleteo) |
| Regeneración | `growthPerSecond` × velocidad del juego + pastar (bien) | Igual: **un individuo nuevo cada 1/`growthPerSecond` s de juego** (20 s los peces), y lo que pasten |
| Depredadores | Chocan con el padre (`OnTriggerStay`) y muerden (bien); el trigger se movía sin cuerpo | Igual, con un cuerpo cinemático (un trigger que se mueve) |

La regla se mantiene: **sin ciclo de vida por individuo**. El grupo es UN organismo: crece con el tiempo y pastando, mengua al ser
comido. Los depredadores cazan al padre.

## Las especies (mar de Ross: el santuario está a −77,85°, el de McMurdo)

| Grupo | Especie | Talla real | En el juego | Come / lo comen |
|---|---|---|---|---|
| Banco de peces | Pez plateado antártico (*Pleuragramma antarctica*): el pez pelágico que forma bancos en la plataforma antártica | 15–25 cm | 20 cm × 1,4 a la vista; lomo gris azulado, vientre plateado, caudal ahorquillada | kril / focas, pingüinos, orcas, oso |
| Kril | Kril antártico (*Euphausia superba*) | 5–6 cm | 6 cm × 2 a la vista; translúcido rojizo, ojos negros, patas en cesta | fitoplancton / peces, medusas, ballena, pingüino |
| Medusas | *Diplulmaris antarctica* (umbrela 10–18 cm, anaranjada; registrada en McMurdo) y *Desmonema glaciale* (la grande: umbrela de hasta ~1 m, pardo rojiza, tentáculos de metros) | | a su talla; laten (la umbrela se contrae) y derivan, no huyen | kril / pingüinos (el de Adelia come medusas) y quien come peces |
| Abejas | Abeja de las nieves del santuario | ~20 cm | cabeza, tórax, abdomen a bandas y dos alas que aletean | néctar / la araña de las nieves |

**¿Medusas en los polos?** Sí: en el Antártico hay escifozoos grandes como *Desmonema glaciale* y *Diplulmaris antarctica* (las dos
vistas en el estrecho de McMurdo; Verhaegen, Cimoli y Lindsay 2021), y *Periphylla periphylla* en aguas profundas de los dos polos; en
el Ártico, *Cyanea capillata* y *Chrysaora melanaster*. Como el santuario está en el mar de Ross, van las dos de McMurdo.
(La talla de *Desmonema* de ~1 m sale de fuentes divulgativas, no de una medida publicada: se usa como orden de magnitud.)

## Dónde están

- La laguna central: 2 bancos de peces, 2 enjambres de kril, un grupo de *Diplulmaris* y otro de *Desmonema*.
- **Bajo cada edificio-árbol** (su fondo está abierto al mar, docs/mascaras-de-oxigeno.md): un banco, un enjambre de kril y un grupo de
  medusas (de una u otra especie, alternando), junto a las cortinas de fitoplancton. **Entran al tronco**: en cada nuevo destino,
  con probabilidad 0,5 (0,35 los de la laguna, si hay un árbol a < 60 m), suben por debajo del hueco a un punto del tronco entre 3 y
  18 m y luego vuelven a salir por el fondo.
- Los escenarios por especie y la escena del ciclo de vida, como antes (ahora con individuos).

## El enjambre de abejas (`BeeSwarm`)

Cada colmena (`Hive`, en un árbol carnívoro) crea al empezar su **enjambre de pecoreadoras**: un grupo de ~18 abejas (hasta 24).
- **De día** vuela de copa en copa entre los árboles a < 80 m de su colmena (a 4 m/s, en arco por encima de las copas), se queda 6 s
  pecoreando en cada una y lleva el néctar a la colmena.
- **De noche** vuelve a la piquera y se queda allí, en racimo.
- Las abejas de dentro de la colmena (reina, nodrizas, limpiadoras, cereras, guardianas: `IceInsectLife`) siguen siendo individuos
  con su oficio (la prueba del ciclo de vida, grupo 58, no cambia): el enjambre es la parte de FUERA, el pecoreo, que es la que se ve
  volar de árbol en árbol.
- La araña de las nieves y quien caza presas al vuelo las muerden como a los peces (un mordisco, una abeja menos).

## Prueba (grupo 71)

Ningún grupo es una plataforma; los peces, el kril, las medusas y las abejas son de su especie (con sus piezas y su talla); al
moverse el banco van con él todos sus peces; un depredador que come peces choca con el banco y cada mordisco quita UN pez; el banco
se rehace solo; hay grupos de las dos medusas y laten; un grupo marino sube al tronco del Árbol de los Campos por su fondo abierto;
y el enjambre de una colmena visita de día al menos 3 árboles distintos.

## 2026-11-04: sin deslumbrar (Berón: «las abejas y las medusas brillan tanto que deslumbran»)

No era emisión: los individuos usaban `KitchenProps.Mat`, que para un color con alfa < 0,99 hace CRISTAL (transparente, liso 0,92), y
la abeja era casi blanca. Docenas de brillos especulares superpuestos se leían como luces. Ahora `SwarmModels.Matte`: lisura 0,15
(opacos) / 0,2 (translúcidos: alas, umbrela, brazos, tentáculos, kril), sin reflejos especulares ni del entorno, sin emisión y con la
luminancia como mucho en 0,55. La abeja de las nieves es de **pelusa gris blanquecina** mate con bandas oscuras y alas grises; las
abejas de la colmena (prefab) usan esos colores (`CubeCreature`) y, aunque su prefab sea el de antes, se pasan a mate al empezar
(`SwarmModels.Dim` en `IceInsectLife`). Las medusas siguen translúcidas pero mates; ninguna brilla (los escifozoos antárticos no se
conocen por su bioluminiscencia).
