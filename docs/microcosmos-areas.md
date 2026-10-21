# El microcosmos de cada área

Es como si de pronto el mundo fuera de insectos.

## Escala

- **El jugador (≈ 1,8 m) es una hormiga.**
- **Las hormigas son los humanos del microcosmos:**
  - bípedas, sobre las patas traseras;
  - con brazos y bracitos;
  - con cara (ojos con pupila y brillo, cejas, boca, mandíbulas) y antenas;
  - con intelecto humano: recogen, cultivan y lideran.
- **El área del santuario se amplía ×10:** sus muebles son montañas.
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

## Historias por área

- **Enfermería — el alba:** la muerte de Ambrosio camino de la cueva ([microcosmos-level1.md](microcosmos-level1.md)).
  - Se quitaron los duplicados del mapa de juego: otro Ambrosio y otra Kushal-hormiga (Kushal eres tú).
- **Cocina — la era del fuego: las reencarnaciones** ([microcosmos-insects.md §13.2](microcosmos-insects.md)).
  - La tribu recoge restos de comida y los lleva a la guarida.
  - **La Tirana de las Feromonas** (Medea renacida) es la líder: muy poderosa. Cada poco va a ver a **Nasatya** (Ambrosio renacido: enorme, manso, recostado en una postura rara) y con él es blanda.
  - Están también **El Señor del Fuego** (Héspero), junto a la brasa, **El Chamán** (Sakshi) y **El Bufón** (Momo).
- **Huerto:** **La Sembradora** (Ruth renacida) cuida el bancal de hongos.
  - Cada poco enseña a una recolectora a cultivar: deja de recoger y lleva hojas al bancal, y los hongos crecen.
  - Así la tribu pasa de recolectar a cultivar.
- **Sala de Yoga, Estudio Textil y Mecánica:** la tribu vive en su área gigante, con su guarida y sus recolectoras; las misiones siguen en el tablero.

**Forma real de los insectos** ([insect-morphology.md](insect-morphology.md)): `CubeInsect.Dress` construye cada especie con los datos de la guía (`InsectMorphology`): segmentos del cuerpo, patas de 5 segmentos (8 de 7 en arañas) en su pose de reposo y marcha en trípode o tetrápodo; antenas acodadas, mandíbulas que se abren, élitros, dos pares de alas en la abeja, patas raptoras (mantis) y saltadoras (saltamontes, grillo), sifones y estilete (pulgón), quelíceros y pedipalpos (arañas). La fauna del microcosmos usa este cuerpo; su conducta, ciclo de vida y biología siguen siendo los del prefab. Las hormigas humanas llevan antena acodada, mandíbulas que se abren y patas con las proporciones de la hormiga.

Código: `AreaMicrocosmos` (menú «Build Microcosmos por área»), `CubeInsect.DressHuman`, `MicroForager`, `MicroTeacher`,
`MicroGardener`, `MicroLeader`, `MicroLounger`, `MicroSize`.

## Virtualizaciones

- Cada virtualización tiene **su propia escena** (`Virtual_<clave>`), para desarrollarla por separado.
- La escena trae una **copia del área real** del santuario, con los mismos muebles.
- Al cargarse llegan **copias de la gente** que trabaja allí.
- Los pasos se anclan a los muebles reales: la silla de exámenes, el analizador, el fogón, el comedor…
