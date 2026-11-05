# El microcosmos de cada área

Es como si de pronto el mundo fuera de insectos.

## Escala

- **El jugador (≈ 1,8 m) es un mirmidón.**
- **Los mirmidones (hormigas humanas) son los humanos del microcosmos.** El nombre viene del mito: Zeus convirtió a las
  hormigas de Egina en personas (Ovidio, *Metamorfosis* VII). La especie `Ant` del ecosistema es otra cosa: la hormiga real.
  Los mirmidones son:
  - bípedas, sobre las patas traseras;
  - con brazos y bracitos;
  - con cara (ojos con pupila y brillo, cejas, boca, mandíbulas) y antenas;
  - con intelecto humano: recogen, cultivan y lideran.
- **El área del santuario se amplía ×100** (`AreaMicrocosmos.AreaScale`; Berón 2026-11-02, «sigue con la escala sugerida
  (×100)»; antes ×10). Es la escala real: una hormiga de 1,8 cm junto a una cama de 2 m es un insecto-humano de 1,8 m junto a
  una cama de 200 m. Sus muebles son montañas (la mesa, 75 m; la encimera, 90 m) y el piso mide kilómetros: los pisos de
  24–46 m del santuario dan microcosmos de **2,4–4,6 km** de lado (antes 240–460 m).
  - **El barrio de la historia** (`AreaMicrocosmos.District`, `MicroFloor.districtCenter/districtSize`): el piso real × 10
    (lo que medía el piso entero a ×10), centrado en la plaza de la historia. Ahí va lo que antes llenaba el piso: la historia,
    su pueblo o ciudad en las eras avanzadas (`FloorBounds` / `FloorRectLocal` devuelven el barrio), los personajes, y las plazas
    de flora y fauna (a su distancia de la historia de antes, ×10). La flora crece **densa** en el barrio (rejilla de 6 m; la del
    pueblo, de 3 m) y **espaciada** en el resto del piso (×10: cada 60 m), para no multiplicar por cien el número de plantas.
  - **(2026-11-03) En un pueblo** (era avanzada) el barrio es el piso real **× 60** (`AreaMicrocosmos.TownScale`: el 60 % del
    lado, 1,4–2,8 km) y el pueblo va sobre su plano real a escala 1:1 ([microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md)).
  - En las eras avanzadas **ya no se quita ningún mueble** (2026-11-03, «el piso no debe romperse»): el pueblo rodea los muebles
    gigantes o se sube a sus mesetas, trepando (§Reglas, 8 y 9).
  - Las medidas del área gigante que deciden qué es suelo, qué es mueble y qué cara de arriba se puebla escalan con
    `AreaMicrocosmos.K` (= AreaScale / 10); el NavMesh cubre el piso entero (hasta 4,6 km) y 600 m de alto.
  - Lo que no cambia: el tamaño de la flora y de la fauna (relativo a la hormiga) y el de la historia y los pueblos.
- El área queda **entera** (la cocina se reconoce: barra, fogones, mesas del comedor): la historia se coloca en su plaza (el
  hueco más libre de su suelo) y no se aparta ningún mueble (2026-11-03: el piso no se rompe).
- **Flora a escala:**
  - el musgo y los hongos hacen de hierba y arbustos;
  - las briznas de hierba y los helechos, de árboles.
- **Fauna de antes del fuego,** a tamaño relativo a la hormiga:
  - pulgón 1 m, termita 1,6 m, mariquita 2,4 m;
  - hormiga león 3 m, con su embudo;
  - grillo 5 m y pelotero 5 m, con su bola;
  - saltamontes 7 m y mantis 10 m.
  - Cada animal cuelga de un `MicroSize` que lo mantiene a ese tamaño, porque los prefabs fijan su propia escala al nacer y por eso antes se veían diminutos.
- **Hay agua alrededor** para no caer al vacío.
- **El portal de salida** muestra su diálogo al acercarse.

## Reglas de los microcosmos (2026-11-03)

Berón: «recuerda que los microcosmos también tienen reglas y documéntalas si no lo están, todos deben pertenecer a un piso del
santuario y deben montarse encima de dicho piso, todas las estructuras de dicho piso se vuelven superficie utilizable para
poblar con insectos-humanos, flora, edificaciones y fauna, y se deben reconstruir pueblos/ciudades enteras donde se desarrolle la
historia, debe existir una época/lugar … si no existe un piso que encaje con el microcosmos que se quiere crear se puede marcar
como pendiente de obtener piso representativo … todos los pisos deben tener preparada su forma en construcción y su forma
finalizada, y los microcosmos se crean siempre sobre la forma finalizada».

| # | Regla | Dónde se aplica | Dónde se comprueba |
|---|---|---|---|
| 1 | **Pertenece a un piso del santuario.** Tres datos obligatorios: fecha, hora y piso (`MicrocosmSpec`). | `MicrocosmSpec` (sin piso no se crea); `MicroFloor.platform` | `MicroFloors.Validate` |
| 2 | **Se monta ENCIMA de ese piso**: el piso del santuario ×100 es el suelo del microcosmos (`…_gigante`), y la historia y su pueblo van a ras de su suelo. | `AreaMicrocosmos.Build` | `MicroFloors.Validate` (piso gigante presente; la historia a ±2,5 m del suelo) |
| 3 | **Sobre su FORMA FINALIZADA**: todo piso tiene dos formas (ver [construcciones-elevadas.md](construcciones-elevadas.md#las-dos-formas-de-cada-piso-2026-11-03)); el microcosmos se monta siempre sobre la finalizada: el «Acabado» encendido y ni andamio, ni material del día, ni conos. | `FloorForms.ShowFinished`, lo PRIMERO de `AreaMicrocosmos.Build` | `MicroFloor.finishedForm`, `FloorForms.ObraLeft`, `MicroFloors.Validate`; `BruceLeeStoryTest` |
| 4 | **Las estructuras del piso son superficie utilizable**: la cara de arriba de cada mueble gigante (mesas, camas, encimeras, máquinas: mesetas de 75 m) se puebla de flora y fauna; en un pueblo, las de fuera de su barrio también, y en las dos más grandes se levanta un **mirador** de la era (sitio «Mirador», con su monumento, su frutal y bancos para personas e insectos-humanos). | `AreaMicrocosmos.FurnitureTops`, `Nature` (en un pueblo, `TownTopSow`: flora del lugar a escala de pueblo), `CivicBuilder.Outposts` | `MicroFloor.floraOnTops / faunaOnTops / buildingsOnTops`; `MicrocosmosFloorTest` (5) y `MicroFloors.Validate` |
| 5 | **El pueblo o la ciudad ENTERA de la historia**, adaptada al piso (regla 8), sobre su plano real a escala 1:1 en un barrio del piso × 60 (1,4–2,8 km), con sus calles, ríos, lagos, colinas y montes, sus edificios de la historia en su sitio y los del pueblo (plaza, mercado, escuela, estación, comisaría…) | `AreaMicrocosmos.TownScale`, `CivicBuilder.Plans` ([microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md)) | `MicroFloors.Validate` (barrio ≥ 40 % del piso y ≥ 12 edificios) |
| 6 | **Una época y un lugar**: su fecha (`Eras.OfMicrocosmos`), su región (`MicroStyles.RegionOf`) y con ellas el estilo de cada edificio y mueble, su flora y su fauna (`MicroBiome`). | `MicroEra`, `MicroStyles`, `MicroBiome` | reglas 7 y 10 de [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md) |
| 7 | **Si ningún piso encaja, «pendiente de piso representativo»**: se monta sobre el piso existente que más se le parece y se apunta el piso que faltaría crear (`MicroFloors.Table`); el menú «Pisos que faltan» da la lista. | `MicroFloors`, `MicroFloor.representative / pendingFloor` | `MicroFloors.Validate` (si no lo representa, tiene que decir qué piso falta) |
| 8 | **El piso NO se rompe** (2026-11-03): ninguna pieza del piso se quita ni se mueve; el pueblo y la historia se ADAPTAN a él: rodean los muebles que apoyan en el suelo o se suben encima (barrio alto en las mesetas). | `AreaMicrocosmos.Build` (sin despeje), `CivicBuilder.Lots` y el plano (esquivan `FloorFurniture`), `CivicBuilder.Outposts` | `MicroFloor.floorPieces` = piezas ahora (`MicroFloors.Validate`); `BruceLeeStoryTest` |
| 9 | **Los insectos-humanos trepan**: por paredes y patas de mesas, sillas y camas suben a las caras de arriba de los muebles. | un `NavMeshLink` del suelo a cada cara de arriba y `Climbable` en cada mueble alto | `MicroFloor.climbLinks` (si hay edificios encima, &gt; 0) |

### La tabla de pisos (`MicroFloors.Table`)

| Microcosmos | Piso del santuario | ¿Lo representa? | Por qué / piso que falta |
|---|---|---|---|
| Sakshi (el río), Patanjali, Sala de Yoga | Sala de Yoga | sí | la práctica |
| Ambrosio, Hospital, Escútari, Kapilavastu, Irena Sendler | Hospital | sí | cuidar, curar, la muerte y la enfermedad |
| Cocina, Mesopotamia, Le Chambon | Cocina | sí | el fuego, el pan del templo, dar de comer |
| Huerto | Huerto | sí | los primeros cultivos |
| Estudio Textil, Mecánica, Dayton | Estudio Textil / Mecánica | sí | el oficio |
| Bruce Lee | Dojo | sí | las artes marciales |
| Planta Atómica, Woolsthorpe, Berna | Planta Atómica | sí | la física |
| Berkeley | Biología | sí | la primera vacuna |
| París 1898, Pasadena | Química | sí | el laboratorio |
| Chernóbil | Central Nuclear | sí | el reactor |
| Egina | Construcción | sí | los mirmidones levantan su ciudad |
| Tregua 1914 | Gimnasio | **pendiente** | Campo de deportes de equipo (fútbol) |
| Alejandría | Aula | sí | la biblioteca de Alejandría (re-apuntado el 2026-11-04 por la alerta de piso nuevo) |
| Crotona | Aula | sí | la escuela del número y la música (re-apuntado el 2026-11-04) |
| Menfis | Aula | sí | el archivo y la biblioteca (re-apuntado el 2026-11-04) |
| Bagdad | Aula | sí | la Casa de la Sabiduría: biblioteca y sala de cálculo (re-apuntado el 2026-11-04) |
| Angostura | Cocina | **pendiente** | Sala de asambleas (parlamento) |
| Sils Maria | Aula | sí | la biblioteca de filosofía (re-apuntado el 2026-11-04) |
| Colorado Springs | Construcción | **pendiente** | Taller de electricidad (alta tensión) |
| Vega Baja · Long Island · Grandes aportes · Montreal · Gary · Kingston · Belfast | Informática · Aula · Aula · Sala de Ballet (las 4 últimas) | **pendiente** | Sala de grabación y escenario de conciertos (música) |
| Palenque (Pakal) | Huerto | **pendiente** | Templo de los antepasados (memoria, rito y duelo) |
| Velliangiri (Sadhguru) | Sala de Yoga | sí | meditar en silencio: el Dhyanalinga ([microcosmos-famosos.md](microcosmos-famosos.md)) |
| Aldea de la montaña (Goku, homenaje) | Dojo | sí | entrenar sirviendo a la aldea |
| Uagadugú (Ibrahim Traoré) | Huerto | sí | los productores: labrar y sembrar |
| Hawthorne (Elon Musk) | Control de misión (lanzadera) | sí | el lanzamiento seguido desde el control (re-apuntado el 2026-11-04) |
| Albuquerque (Bill Gates) | Informática | sí | el clúster de IA (re-apuntado el 2026-11-04 por la alerta de piso nuevo) |
| Omaha (Warren Buffett) | Estudio Textil | **pendiente** | Mercado y oficina de comercio (economía) |
| Prairie View (MacKenzie Scott) | Aula | sí | la universidad y su biblioteca (re-apuntada el 2026-11-04) |

### Los pisos que faltan por crear (`MicroFloors.Pending`)

1. **Campo de deportes de equipo (fútbol)** — Tregua 1914.
2. **Observatorio y biblioteca** — Alejandría (Eratóstenes). — **cubierto** (2026-11-04: re-apuntado a un piso que ya existe).
3. **Aula de matemáticas y música** — Crotona (Pitágoras). — **cubierto** (2026-11-04: re-apuntado a un piso que ya existe).
4. **Archivo de historia y geografía** — Menfis (Heródoto). — **cubierto** (2026-11-04: re-apuntado a un piso que ya existe).
5. **Casa de la Sabiduría (biblioteca y sala de cálculo)** — Bagdad (al-Juarismi). — **cubierto** (2026-11-04: re-apuntado a un piso que ya existe).
6. **Sala de asambleas (parlamento)** — Angostura (Bolívar).
7. **Sendero de montaña y biblioteca de filosofía** — Sils Maria (Nietzsche). — **cubierto** (2026-11-04: re-apuntado a un piso que ya existe).
8. **Taller de electricidad (alta tensión)** — Colorado Springs (Tesla).
14. **Sala de grabación y escenario de conciertos (música)** — los microcosmos de la música ([microcosmos-musica.md](microcosmos-musica.md)).
9. **Templo de los antepasados (memoria, rito y duelo)** — Palenque (Pakal).
10. **Fábrica de cohetes y control de misión** — Hawthorne (Elon Musk). — **cubierto** (2026-11-04: re-apuntado a un piso que ya existe).
11. ~~Sala de informática~~ — **creado** (piso de Informática, 2026-11-04): Albuquerque (Bill Gates) ya está en él.
12. **Mercado y oficina de comercio (economía)** — Omaha (Warren Buffett).
13. ~~Aula y biblioteca universitaria~~ — **creado** (Aula, 2026-11-04): Prairie View (MacKenzie Scott) ya está en él; la alerta señala también a Menfis, Alejandría, Bagdad, Crotona y Sils Maria.

(Una biblioteca común cubriría 2, 4, 5, 13 y en parte 7.) Menú: «Tools/Cold Sanctuary/Pisos que faltan (microcosmos pendientes de
piso representativo)» los escribe en la consola.

## Historias por área

- **Enfermería — el alba:** la muerte de Ambrosio camino de la cueva ([microcosmos-level1.md](microcosmos-level1.md)).
  - Se quitaron los duplicados del mapa de juego: otro Ambrosio y otra Kushal-hormiga (Kushal eres tú).
- **Cocina — la era del fuego: las reencarnaciones** ([microcosmos-insects.md §13.2](microcosmos-insects.md)).
  - La tribu recoge restos de comida y los lleva a la guarida.
  - **La Tirana de las Feromonas** (Medea renacida) es la líder: muy poderosa. Cada poco va a ver a **Nasatya** (Ambrosio renacido: enorme, manso, recostado en una postura rara) y con él es blanda.
  - Están también **El Señor del Fuego** (Héspero), junto a la brasa, **El Chamán** (Sakshi) y **El Bufón** (Momo).
  - **Historia jugable: «Cómo nació la cocina»** ([novela.md](novela.md)), `KitchenOriginStory` (en el `Elenco`, junto al hogar: piedras alrededor de la brasa, `Piedra_del_hogar` y 10 `Restos_del_hogar`, todo en el claro de la plaza de historia). Beats: Intro → **Carry** (E junto a un resto lo coge, E junto al fuego lo deja; 4 restos; Nasatya lleva los suyos y la Tirana le deja hacer) → **Accident** (el grano del Bufón cae junto a la brasa: lo primero cocinado) → **Vigil** (la vigilia del fuego: E junto al fuego, 3 veces, mientras la llama mengua) → **Circle** (nadie fuera del círculo) → **Farewell** (Nasatya, de lado) → `Progress.MarkStory` y `MobWorldLoader.ExitMobWorld`. Cada espera con tope (si el jugador no llega, Nasatya o el Señor del Fuego terminan la tarea). Prueba: `KitchenOriginStoryTest` (grupo 44).
- **Huerto:** **La Sembradora** (Ruth renacida) cuida el bancal de hongos.
  - Cada poco enseña a una recolectora a cultivar: deja de recoger y lleva hojas al bancal, y los hongos crecen.
  - Así la tribu pasa de recolectar a cultivar.
- **Sala de Yoga, Estudio Textil y Mecánica:** la tribu vive en su área gigante, con su guarida y sus recolectoras; las misiones siguen en el tablero.

**Forma real de los insectos** ([insect-morphology.md](insect-morphology.md)): `CubeInsect.Dress` construye cada especie con los datos de la guía (`InsectMorphology`): segmentos del cuerpo, patas de 5 segmentos (8 de 7 en arañas) en su pose de reposo y marcha en trípode o tetrápodo; antenas acodadas, mandíbulas que se abren, élitros, dos pares de alas en la abeja, patas raptoras (mantis) y saltadoras (saltamontes, grillo), sifones y estilete (pulgón), quelíceros y pedipalpos (arañas). La fauna del microcosmos usa este cuerpo; su conducta, ciclo de vida y biología siguen siendo los del prefab. Los mirmidones llevan antena acodada, mandíbulas que se abren y patas con las proporciones de la hormiga.

Código: `AreaMicrocosmos` (menú «Build Microcosmos por área»), `CubeInsect.DressHuman`, `MicroForager`, `MicroTeacher`,
`MicroGardener`, `MicroLeader`, `MicroLounger`, `MicroSize`.

## Virtualizaciones

- Cada virtualización tiene **su propia escena** (`Virtual_<clave>`), para desarrollarla por separado.
- La escena trae una **copia del área real** del santuario, con los mismos muebles.
- Al cargarse llegan **copias de la gente** que trabaja allí.
- Los pasos se anclan a los muebles reales: la silla de exámenes, el analizador, el fogón, el comedor…
