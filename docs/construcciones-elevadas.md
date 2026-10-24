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

- **Áreas elevadas a 6 m:** cocina, hospital (uno solo, para personas y animales: la veterinaria es su planta alta), sala de yoga, mecánica y estudio textil (`SampleSceneBuilder.ElevateArea`).
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
  1. **Norte**: la aldea (cocina, hospital, arboleda residencial, casa del árbol de guardia, bosque comestible).
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

- **Plataforma hija (entreplanta)** de **4 m** dentro de la plataforma (2,2 → 3 → 4 m: Berón 2026-10-23 pidió 1 m más para
  pasar holgado), sobre postes solo en su perímetro, con **escalera inclinada** (38°, rampa para andar y para el NavMesh),
  sin barandilla (`Deck`, `InclinedStair`).
  - **Hospital** (antes Enfermería; [`hospital.md`](hospital.md), v2): ya no tiene galería en U. Cada planta lleva un
    **anillo de camas** (`RingDeck`): plataforma hija cuadrada a 4 m, 10 m de ancho, 20 % sobre la planta y 80 % volando
    fuera, con dos escaleras inclinadas. Planta baja (urgencias + maternidad), planta 1 a 11 m (recuperación +
    hospitalización) y planta 2 veterinaria con el anillo de grandes animales (osos, focas).
  - **Restaurante:** las mesas de los lados suben a dos **galerías de comedor** (oeste y este, 4,6 × 18 m), 3 mesas en
    cada una con vista a la cocina abierta y escalera inclinada al borde de +Z. Abajo queda un paso de ~5 m alrededor de
    la cocina. 12 mesas de 4 = 48 cubiertos.
- **Piso superior a 11 m** (antes 10) (no tapa la vista del de abajo): columnas solo en el perímetro y escalera vertical desde el
  piso de abajo, a un lado del eje (`StackOn`). Es **3 m más corto** por el lado de la entrada: ahí apoya su escalera.
  Sin puente propio.
  - **Sala de Ballet** sobre la **Sala de Yoga**.
  - **Dojo** sobre el **Gimnasio**.
  - **Huerto de rotación** sobre el **Huerto** (dos huertos, una base).
  - **Estudio Textil** sobre la **Construcción** (taller sobre taller; el textil queda de 18 × 11 m). La Mecánica
    (10 × 9) es pequeña para cargar otro piso: sus columnas y la escalera chocarían con los aseos y la máquina.
- **Residencias:** cada cabaña entre árboles tiene **otra encima a 6 m** sobre cuatro columnas, con escalera vertical
  desde su pasarela. Se quitan el anillo exterior de cabañas sobre columnas y sus seis pasarelas. La pista de alfombras
  sale ahora de la cabaña de 0°.

Las misiones del microcosmos pueden usar las galerías como "montañas" con camino (el área ×10 conserva sus escaleras).
