# Construcciones elevadas, energía solar y dormitorios

Principio del santuario: **lo construido no ocupa el suelo que pertenece a la naturaleza** (animales, insectos y plantas). Lo ideal serían estructuras que vuelan, pero el santuario 1 aún no tiene esa tecnología. Por eso todo se eleva: en casas de árbol que se apoyan en uno o varios troncos, o sobre columnas finas apoyadas en el suelo.

## ¿A qué altura?

La altura que deja el entorno sin impacto depende del bioma: hay que dejar libres el **paso de los animales** y la **capa de vegetación** que crece debajo.

| Bioma | Lo que hay que respetar | Altura recomendada |
|---|---|---|
| **Hielo (santuario 1)** | Flora rasa (musgo, líquenes, hierba de escarcha < 1 m). El animal terrestre más alto es el oso polar de pie (~3 m); el reno con astas, ~2,2 m. | **6 m**: el paso es holgado incluso para un oso erguido. |
| **Bosque templado o tropical** | Sotobosque: arbustos y árboles jóvenes de 3–6 m; animales grandes (ciervos, osos) por debajo de 3 m. La **copa** (15–40 m) es hábitat de aves, primates y polinizadores. | **8–10 m**: por encima del sotobosque y por debajo de la copa. No se construye en la copa. |
| **Pradera / sabana** | Hierbas altas (1–2 m); fauna alta como las jirafas (~5,5 m). | 6–7 m |

Además de la altura:
- **Huella mínima en el suelo:** columnas finas y espaciadas, sin losas a ras de tierra.
- **Luz:** suelos de tablones con juntas y cubiertas estrechas, para no dejar a oscuras a las plantas de debajo.
- **Pasarelas colgantes entre edificios,** en vez de caminos que corten el suelo.
- **El huerto sí está a ras de suelo:** cultivar es trabajar la tierra.

## Lo que hay en el juego

- **Áreas elevadas a 6 m:** cocina, enfermería (una sola, para personas y animales: no hay veterinaria aparte), sala de yoga, mecánica y estudio textil (`SampleSceneBuilder.ElevateArea`).
  - Se apoyan en columnas y se sube por una escalera: una rampa invisible para andar y para la navegación, más peldaños, zancas y pasamanos.
  - Tienen barandilla de 1 m (una barrera invisible y unos balaustres visibles).
- **Sala de yoga para 30 personas:** 18 × 15 m, con 30 esterillas en 6 × 5 y cojines, la tarima del maestro y un cuenco tibetano.
- **Casa del árbol de guardia:** el único árbol gigante, de 83 m (el resto de árboles de hielo mide 20–40 m de adulto), que no caza, con su cabaña a 11 m y escalera vertical.
- **Arboleda residencial** (su propia isla, al noroeste), con un dormitorio por persona:
  - una plataforma central a 7 m con su escalera;
  - seis árboles que no cazan;
  - **seis cabañas apoyadas cada una en dos troncos** (con vigas a ambos);
  - **seis cabañas sobre columnas** detrás de ellas;
  - pasarelas colgantes entre todas.

  Cada dormitorio tiene cama, mesilla con lámpara, estante, ventana y un cartel con el nombre de su dueño. Cuando un habitante tiene sueño, va a **su** cama a dormir (`Bedroom`, `NeedsDriver`).
- **Energía solar** (aquí no hay gas):
  - una fila de paneles sobre postes junto a cada edificio y un campo solar en el hub (`SolarArray`);
  - generan energía de día, con un máximo al mediodía y nada de noche;
  - las cocinas son de **inducción**: vitrocerámica con zonas y panel táctil, sin llama, y horno eléctrico.

## Casquetes y árboles (2026-10-18)

- **Seis casquetes como máximo**, cada uno una masa de hielo continua (`PolarLandscapeBuilder.Islands`, lóbulos `Casquete:Nombre`):
  1. **Norte**: la aldea (cocina, enfermería, arboleda residencial, casa del árbol de guardia, bosque comestible).
  2. **Centro**: el hub, la Magnate, el área del huerto, el arrozal, los pingüinos y el textil.
  3. **Sur**: los huertos-gremio (rotación, milpa, andino).
  4. **Este**: sala de yoga, mecánica y los osos.
  5. **Bosque Oeste** y 6. **Bosque Norte**: los árboles carnívoros de hielo, salvajes, con abejas y arañas de las nieves.
- **Árboles y áreas fusionados**: junto a cada área crecen tres árboles grandes que no cazan (24–36 m); el bosque
  carnívoro queda solo en los dos casquetes salvajes. Menos árboles que antes.
- **Las ramas esquivan las áreas** (2026-10-22): cada rama comprueba el volumen de todas las áreas (plataforma, pisos
  superiores como el textil sobre la Construcción, galerías; 3 m de margen) y gira buscando hueco, o no crece. Igual que
  en la arboleda residencial. `SanctuaryLifeTest` verifica que ninguna rama atraviese una plataforma.
- **Alturas**: los adultos miden 22–32 m y los viejos 32–40 m; los jóvenes y brotes, menos porque aún crecen.

## Pisos: plataformas hijas y bases de dos pisos (2026-10-22)

Para tener **caminos amplios y ordenados** y menos plataformas sueltas con puentes, una misma base puede crecer hacia
arriba (`SampleSceneBuilder.Levels.cs`):

- **Plataforma hija (entreplanta)** de **3 m** dentro de la plataforma (debajo se pasa de pie sin chocar con la losa),
  sobre postes solo en su perímetro, con **escalera inclinada** de 38° (rampa para andar y para el NavMesh) y **sin
  barandilla** (`Deck`, `InclinedStair`).
  - **Gimnasio:** 15 máquinas abajo y 15 en una entreplanta sobre la mitad izquierda, con pasillos de 1,5 m.
  - **Enfermería:** las 27 camas de hospitalización suben a una **galería en U** (alas oeste y este de 4,4 m + fondo de
    3,8 m), con dos escaleras (la oeste junto a la entrada; la este, de lado, porque al frente está el tanque). Abajo
    quedan las 3 camas de urgencias, la exploración (oeste), la veterinaria (este) y el paso libre desde la entrada.
  - **Restaurante:** las mesas de los lados suben a dos **galerías de comedor** (oeste y este, 4,6 × 18 m), 3 mesas en
    cada una con vista a la cocina abierta y escalera inclinada al borde de +Z. Abajo queda un paso de ~5 m alrededor de
    la cocina. 12 mesas de 4 = 48 cubiertos.
- **Piso superior a 10 m** (no tapa la vista del de abajo): columnas solo en el perímetro y escalera vertical desde el
  piso de abajo, a un lado del eje (`StackOn`). Es **3 m más corto** por el lado de la entrada: ahí apoya su escalera.
  Sin puente propio.
  - **Sala de Ballet** sobre la **Sala de Yoga**.
  - **Dojo** sobre el **Gimnasio**.
  - **Huerto de rotación** sobre el **Huerto** (dos huertos, una base).
  - **Estudio Textil** sobre la **Construcción** (taller sobre taller; el textil queda de 18 × 11 m). La Mecánica
    (10 × 9) es pequeña para cargar otro piso: sus columnas y la escalera chocarían con los aseos y la máquina.
- **Residencias:** cada cabaña entre árboles tiene **otra encima a 5 m** sobre cuatro columnas, con escalera vertical
  desde su pasarela. Se quitan el anillo exterior de cabañas sobre columnas y sus seis pasarelas. La pista de alfombras
  sale ahora de la cabaña de 0°.

Las misiones del microcosmos pueden usar las galerías como "montañas" con camino (el área ×10 conserva sus escaleras).

## Barandillas, paneles y pistas (2026-10-22, Berón)

- **Sin barandillas visibles** en las áreas humanas. En el borde exterior de cada plataforma elevada queda solo una
  **barrera invisible** de 4 m, para que nadie caiga de 12 m. Las entreplantas no llevan nada.
- **Paneles solares en lo más alto**: cada edificio lleva su fila en el borde de su piso más alto, sobre postes de 3 m
  (`PlaceRooftopSolar`). Se busca un borde libre de muebles: primero el fondo, luego los laterales, nunca el frente.
- **Pista de alfombras como piso superior**: la pista de cada grupo va a 10 m encima del piso más alto de su área
  mayor, si ese piso cabe 14 × 14 m (`PlaceStackedPads`); sus columnas buscan sitio libre de muebles. Así no ocupa
  terreno ni necesita puente; se sube por su escalera vertical. Solo si no cabe en ningún piso del grupo se hace
  aparte, como antes. La de la arboleda residencial sigue aparte.
- **Huertos sin bancales**: las filas van directas en el suelo (los bancales eran solo adorno).
- **Espacio mínimo por tipo de mueble** (`FurnitureSpacing`): máquinas de gimnasio, mesas, camas, puestos de cocina,
  telares y aseos piden un pasillo libre (0,8–0,9 m). El Build avisa con `[Espacio]` de cada par demasiado junto, y
  `SanctuaryLifeTest` exige que las máquinas del gimnasio no se apilen.
