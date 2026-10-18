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
- **Alturas**: los adultos miden 22–32 m y los viejos 32–40 m; los jóvenes y brotes, menos porque aún crecen.
