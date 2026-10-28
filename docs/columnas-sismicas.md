# Columnas sísmicas para las plataformas octogonales

Investigación del 2026-10-28 para `Elevate()` en `Assets/Editor/SampleSceneBuilder.cs`. Hoy cada plataforma octogonal se apoya en
**una columna en cada uno de los 8 vértices**: una caja de hormigón de 0,7–1,2 m con zunchos de acero cada 3 m. Los pisos van
cada 11 m, hay hasta 5 o más, y la planta mide de 18 a 40 m. Berón quiere el diseño de columna real más fuerte y seguro (ha oído
lo de las megacolumnas de Asia) y saber **cómo se ve**, para modelar uno representativo.

> Este documento sirve para el juego, no para construir. Las cifras son órdenes de magnitud sacados de proyectos reales y
> publicados. No sustituyen un cálculo estructural.

---

## 0. Lo que hace Asia (resumen de las fuentes)

| Sistema | Ejemplo real | Qué aporta | Fuente |
|---|---|---|---|
| **CFST: tubo de acero relleno de hormigón** | Taipei 101: 8 supercolumnas de cajón de acero soldado de unos 2,4 × 3 m, con chapas de 50–80 mm y rellenas de hormigón de alta resistencia. Goldin Finance 117 (Tianjin): 4 megacolumnas CFST multicelda en las esquinas, de hasta 45 m², con acero Q345GJ y hormigón C50–C70 autocompactante | El tubo confina el hormigón, que así aguanta mucho más, y el hormigón impide que el tubo se abolle. Es la columna más resistente y dúctil por kilo | [Cambridge, guía de Taipei 101](https://www.phase-trans.msm.cam.ac.uk/2026/SG-t101.html), [Designing Buildings: Taipei 101](https://www.designingbuildings.co.uk/wiki/Taipei_101), [Tongji/constructsteel: Tianjin 117](https://constructsteel.org/steel-solutions/steel-composite-concrete-solution/building/column/office-building-in-tianjin/), [CTBUH 117](https://skyscrapercenter.com/pdf/building/73), [Structural Design of Tianjin Goldin Finance 117 (IJHRB)](https://koreascience.or.kr/article/JAKO201208160369091.page?lang=ko) |
| **SRC: hormigón armado con perfil de acero embebido** | Shanghai Tower: megacolumnas mixtas con chapas de acero en forma de «王» que encierran el hormigón C70 en la base. La sección se va reduciendo con la altura | Gran rigidez y protección del acero contra el fuego. Los perfiles internos crean celdas que confinan el hormigón | [Tongji/constructsteel: Shanghai Tower](https://constructsteel.org/steel-solutions/steel-composite-concrete-solution/building/column/complex-building-in-shanghai-shanghai-tower/), [AISC Modern Steel 2013](https://www.aisc.org/globalassets/modern-steel/archives/2013/04/2013v04_shanghai.pdf), [Gong 2017, FEM](https://journal.hep.com.cn/fem/EN/1160045016028603291) |
| **Cinturones (belt trusses) y outriggers** | Shanghai Tower: 8 cinturones de dos plantas de altura en cada planta técnica. Ping An Finance Center: 4 niveles de outriggers de acero que unen el núcleo a 8 supercolumnas mixtas | Hacen que las columnas de esquina trabajen juntas, como un tubo, contra el vuelco | [Structure mag., Shanghai](https://www.structuremag.org/wp-content/uploads/2014/09/SF-Shanghai-Robertson-June071.pdf), [Thornton Tomasetti: Ping An](https://www.thorntontomasetti.com/projects/ping_an_finance_center), [Designing Buildings: Ping An](https://www.designingbuildings.co.uk/wiki/Ping_An_Finance_Centre) |
| **Amortiguador de masa sintonizada (TMD)** | Taipei 101: esfera de acero de 660 t colgada entre los pisos 87 y 92 | Reduce el balanceo por viento y por sismos moderados | [Cambridge](https://www.phase-trans.msm.cam.ac.uk/2026/SG-t101.html), [Britannica](https://www.britannica.com/place/Taipei-101) |
| **Shinbashira (columna central)** | Tokyo Skytree: un núcleo de hormigón de 375 m separado de la estructura de acero. Está fijo hasta los 125 m y por encima se une con **amortiguadores de aceite**. Según sus promotores reduce la vibración hasta un 50 % | Funciona como contrapeso desfasado, igual que el pilar central de las pagodas | [Gobierno de Japón, HLJ](https://gov-online.go.jp/eng/publicity/book/hlj/html/201112/201112_01.html), [Tokyo Updates](https://www.tokyoupdates.metro.tokyo.lg.jp/en/post-1153), [Hokkaido Univ. keynote](https://eprints.lib.hokudai.ac.jp/repo/huscap/all/54505/KeynoteLecture_6.pdf) |
| **Aislamiento de base** | Apoyos de caucho con núcleo de plomo (LRB): capas de caucho alternadas con chapas de acero de 1–1,5 mm y un tapón de plomo de ~0,2 D. Se fabrican de 0,3 a 1,5 m de diámetro y 0,1 a 0,4 m de alto. Péndulo de fricción: platos cóncavos de acero inoxidable | El edificio «flota» y el suelo se mueve debajo. Es la medida que más reduce la fuerza que llega a las columnas | [Dynamic Isolation (catálogo)](https://dynamicisolation.pages.dev/products/), [WCEE 13_607](https://wcee.nicee.org/wcee/article/13_607.pdf), [CED: Base isolation design](https://www.cedengineering.com/userfiles/S01-018%20-%20Design%20of%20Base%20Isolation%20System%20for%20Buildings%20&%20Bridges%20-%20US.pdf), [OpenSees TFP](https://openseespydoc.readthedocs.io/en/latest/src/TFP.html) |
| **Riostras de pandeo restringido (BRB)** | Nippon Steel «Unbonded Brace», finales de los 80: núcleo de chapa de acero dentro de un tubo relleno de hormigón y separado de él por una capa antiadherente | Fluyen igual en tracción y en compresión sin pandear. Son el «fusible» que disipa el sismo y se puede cambiar | [Wikipedia: BRB](https://en.wikipedia.org/wiki/Buckling-restrained_brace), [WCEE 2017-1073](https://wcee.nicee.org/wcee/article/16WCEE/WCEE2017-1073.pdf), [SIE 2000](https://www.siecorp.com/UBB/Imgs/publications/papers/JPCS2000_IDA_PWC.pdf) |
| **Autocentrado / balanceo (rocking)** | Japón (Hayashi et al. 2018): pórtico basculante de columnas CFT de doble piel con fusibles. China/Hong Kong: pilas CFST postesadas por segmentos con cables no adheridos | Tras el sismo la estructura **vuelve a su sitio** y solo se cambian los fusibles | [Hayashi et al. 2018 (resumen NEU)](https://coe.northeastern.edu/research/hajjar-wiki/fuseinfo/47/hayashi-skalomenos-inamasu-luo-2018), [PolyU 10397/119361](https://ira.lib.polyu.edu.hk/handle/10397/119361), [ASCE CFDST postesado](https://ascelibrary.com/doi/10.1061/JBENF2.BEENG-5890) |
| **UHPC (hormigón de ultra alto rendimiento)** | 150–210 MPa con fibras de acero (sin fibras es frágil). Ensayos a escala real: en la rótula plástica evita el aplastamiento y aguanta ~20 % más de aceleración antes del colapso | Columnas más finas para la misma carga y rótulas que no se desmoronan | [Shao 2023, revisión UHPC sísmico](https://iastatedigitalpress.com/uhpc/article/id/16728), [UHP-FRC column (ITAM)](https://invenio.itam.cas.cz/record/19096), [WCEE 17 2i-0156](https://wcee.nicee.org/wcee/article/17WCEE/2i-0156.pdf) |
| **Encamisado CFRP** | Se usa en refuerzo de pilas y en riostras autocentrantes con barras CFRP pretensadas | Confina el hormigón y protege el acero de la corrosión. Es ligero | [FSCE 2025, CFRP self-centring BRB](https://journal.hep.com.cn/fsce/EN/10.1007/s11709-025-1224-1) |

**Una nota honesta:** las búsquedas no confirmaron la sección exacta de las megacolumnas de Shanghai Tower ni las de Ping An, así
que no se dan cifras de ellas. Para Taipei 101 las fuentes dan entre 2,4 × 3 y 2,4 × 3,1 m.

---

## 1. Recomendación

### Sistema: columna CFST circular sobre aislador de base, con anillo-cinturón en cada losa y riostras BRB en caras alternas

1. **Columna = tubo de acero circular relleno de hormigón (CFST)**, con hormigón de alta resistencia C60–C80. En los 2 m inferiores de
   cada tramo, donde se forma la rótula, el relleno es de **UHPC con fibras**. Es la familia de las megacolumnas de Taipei 101 y Tianjin 117.
   El círculo confina mejor que la caja (las cajas de Taipei 101 necesitan diafragmas internos y rigidizadores para igualarlo).
2. **Aislador de base (LRB) bajo cada columna**, entre la columna y el encepado. Es lo que más reduce la fuerza sísmica sobre las 8
   columnas, y es tecnología japonesa estándar (Skytree también apoya en aisladores).
3. **Anillo-cinturón en cada losa**: una viga-celosía de acero de un tramo entre columnas vecinas que recorre el perímetro del octógono.
   Hace que las 8 columnas trabajen como un tubo (es la idea de los cinturones de Shanghai Tower y Ping An). Como no hay núcleo
   central, no hay outrigger hacia dentro: **el anillo hace de outrigger**.
4. **Riostras BRB en X o en V bajo cada losa, en 4 de las 8 caras (caras alternas)**: aportan rigidez lateral y disipan energía, y las
   otras 4 caras quedan libres para las alfombras y para ver por debajo.
5. Opcional (piso alto de 5+ plantas): un **TMD** en el piso superior, o bien un **shinbashira** (columna central de hormigón suelta,
   unida a cada losa con amortiguadores de aceite), que encaja con la estética «santuario/pagoda».

**Por qué:** con ocho apoyos y nada más, la plataforma es un «pórtico de esquinas». El riesgo real es el vuelco y la deriva lateral,
no el aplastamiento. La CFST da la máxima resistencia y ductilidad por sección, el aislador corta la fuerza que entra, el
anillo y las BRB reparten esa fuerza y la disipan, y las BRB se cambian después del sismo sin tocar las columnas.

### Dimensiones a escala de estas plataformas (estimación)

Carga muerta + viva de unos 10 kN/m² por losa. Un octógono de 40 m ronda los 1.400 m², así que son ≈ 14 MN por piso.
Con 5 pisos son ≈ 70 MN repartidos en 8 columnas, ≈ **9 MN por columna en la base**, más el empuje del vuelco sísmico.
Como referencia, una CFST de Ø0,9 m con tubo de 20 mm y hormigón C60 tiene una capacidad de aplastamiento de ~55 MN
(acero ≈ 20 MN + hormigón ≈ 35 MN, sin contar el confinamiento). El margen es amplio, como corresponde a «la más segura».

| Edificio | Ø tubo (base) | Espesor chapa | Relleno | Ø tubo (pisos altos) | Aislador LRB |
|---|---|---|---|---|---|
| 1–2 pisos, planta ≤ 25 m | 0,7 m | 14 mm | C60 | — | Ø 0,9 m × 0,35 m |
| **3 pisos** | **0,9 m** | **20 mm** | C60, UHPC en rótulas | 0,8 m desde el 3.er piso | Ø 1,1 m × 0,40 m |
| **5 pisos** | **1,2 m** | **30 mm** | C80, UHPC en rótulas | 1,0 m (pisos 3–4), 0,8 m (5.º) | Ø 1,5 m × 0,45 m |
| Más de 5 pisos | 1,4–1,6 m | 40 mm | C80 / UHPC | reducción cada 2 pisos | Ø 1,6 m + TMD arriba |

- La esbeltez de un tramo de 11 m con Ø 0,9 m es L/D ≈ 12, muy compacta, así que no hay riesgo de pandeo.
- La sección se **reduce con la altura**, como en Shanghai Tower. En el juego se ve como un «escalón» de 5–10 cm de radio en el
  anillo de cada losa.
- Acero Q355/Q420 (o el equivalente japonés SN490). Galvanizado y pintado (en la Antártida, la protección es contra el frío y la sal).
- El rango actual de `col` (0,7–1,2 m) ya cuadra con la tabla. Sería mejor ligarlo al **número de pisos** y no solo a la planta.

### Antártida: hielo y permafrost (breve)

- **Sobre roca** (como Princess Elisabeth, que se apoya en pilotes sobre una cresta de granito): encepado anclado a roca, sin más.
- **Sobre permafrost**: pilotes de acero hincados o perforados (Ø 0,6–0,9 m, 10–20 m de profundidad, 2–4 pilotes por columna),
  con **termosifones** (tubos de calor bifásicos sellados, de 80–100 mm de diámetro, que sobresalen 2–4 m con aletas de radiador)
  que enfrían el suelo en invierno y aumentan la adherencia hielo-pilote. Es la técnica de la línea eléctrica y el ferrocarril
  Qinghái-Tíbet y del oleoducto Trans-Alaska. Ojo: pueden levantar los pilotes por helada (se han medido hasta 65 mm).
  Fuentes: [Permafrost & Periglacial Proc. 2022](https://ideas.repec.org/a/wly/perpro/v33y2022i3p277-285.html), [frost jacking por TPCT](https://www.aminer.org/pub/652e2b1c939a5f4082ee5cc7/two-phase-closed-thermosyphon-induced-frost-jacking-of-piles-and-foundation-instability), [Adv. Clim. Change Res. 2024](https://doaj.org/article/6d141bc47f4a4b1cb7a18549263d7ead).
- **Sobre plataforma de hielo** (Halley VI, Neumayer III): patas con **gatos hidráulicos** que se recrecen cada año para no quedar
  enterradas en la nieve. Fuentes: [AMRC Halley VI](https://amrc.ssec.wisc.edu/presentations/The-British-Antarctic-Survey’s-new-Halley-VI-station.pdf), [Wired: bases antárticas](https://wirednews.pages.dev/gallery/ffantarctica).
- El **hueco bajo la losa** (todas estas plataformas están elevadas) también es real: deja que el viento barra la nieve en vez de que se acumule.

---

## 2. Cómo se ve: especificación visual para Unity

Todo en metros. `D` = diámetro del tubo en la base de cada tramo (0,9 m para 3 pisos y 1,2 m para 5). `h` = 11 m por piso.

### Columna (un tramo por piso)
- **Fuste**: cilindro de acero de diámetro `D` (en Unity: cilindro con escala `(D, h/2, D)`, porque la primitiva mide 2 de alto).
  Color **gris acero azulado** `#5B6470`, satinado (metallic 0,6, smoothness 0,45).
  Alternativa «japonesa» más reconocible: **revestimiento octogonal** (dos cubos de `D×h×D` girados 0° y 45°), color hormigón
  blanco `#C9C7C1`, con el tubo asomando solo en anillos y base.
- **Anillos de unión (collares)**: uno por empalme de tramo, cada 3–4 m (las piezas reales se transportan en tramos de 8–12 m;
  aquí hay uno a media altura y otro en cada losa). Cilindro `Ø D+0,12 × 0,25 m`, color acero oscuro `#3A3F46`.
  Los **pernos vistos** son textura o 8 cubitos de 0,06 m (opcional).
- **Franja de rótula**: los 2 m inferiores de cada tramo llevan encamisado **CFRP negro mate** `#1E1F22` (cilindro `Ø D+0,04 × 2 m`).
  Le da un aspecto «tecnológico» y además es correcto: es donde se concentra el daño.
- **Reducción con la altura**: cada tramo superior es 0,1–0,2 m más fino. El salto queda oculto por el collar de la losa.

### Base
- **Encepado** (sobre el terreno): cubo `2,6 × 1,2 × 2,6` (3 pisos) o `3,2 × 1,4 × 3,2` (5 pisos), de hormigón `#A9A7A1`,
  medio enterrado (asoma 0,4 m).
- **Pedestal**: cilindro `Ø D+0,6 × 0,5`, de hormigón.
- **Aislador LRB**, la pieza clave:
  - un cilindro **negro caucho** `#1A1A1A` de `Ø 1,1 × 0,40` (3 pisos) o `Ø 1,5 × 0,45` (5 pisos),
  - entre dos **placas de acero cuadradas** `D+0,5` de lado y 0,06 m de grueso, en gris acero,
  - opcional: 3 o 4 aros finos plateados sobre el caucho, que imitan las chapas internas que se ven en el borde.
  - Variante péndulo de fricción: dos discos de acero inoxidable brillante `#C8CCD0` con una «lenteja» en medio.
- **Termosifones** (solo si es permafrost): 4 tubos `Ø 0,1 × 3 m` alrededor del encepado, plateados, con un bloque de aletas
  `0,3 × 1,2 × 0,3` en la punta.

### Unión con cada losa
- **Collar de nudo**: cilindro `Ø D+0,3 × 0,6` en acero oscuro, centrado en la cara inferior de la losa.
- **Anillo-cinturón**: viga perimetral bajo la losa de una columna a la siguiente (8 tramos, siguiendo el octógono).
  Cubo de `largo de cara × 1,2 alto × 0,5 ancho`, en acero gris `#5B6470`. Si se quiere que parezca celosía: 2 cordones de
  `0,35 × 0,35` (arriba y abajo, separados 1,5 m) + 1 diagonal por cara. El cinturón real de Shanghai Tower mide dos plantas;
  a esta escala basta con 1,2–1,5 m de canto.
- Para la **lectura del edificio** lo más eficaz es pintar el anillo de un color de acento (rojo óxido `#8C3B2E` o el color del
  área). Así se ve una «cinta» en cada piso, como las plantas técnicas de los rascacielos.

### Riostras entre columnas
- En **4 caras alternas** del octógono, bajo cada losa: una **X** (o una V invertida) de BRB.
  Cada BRB es un cubo de `0,35 × 0,35 × longitud diagonal`, girado. Color **amarillo seguridad** `#D9A21B` o naranja, que es
  como se suelen pintar en Japón y Taiwán para que se vean.
  Con caras de 8–16 m y 11 m de altura, la diagonal mide 13,6–19,4 m.
- Remates: 2 cubitos `0,5 × 0,5 × 0,3` (las cartelas o gusset plates) en cada extremo.
- Las otras 4 caras quedan **libres** (para las alfombras y la vista).

### Opcional en los pisos altos (5+)
- **TMD**: esfera dorada o de acero de Ø 3–5 m, colgada de 4 cables bajo la losa superior (y visible desde dentro, como en Taipei 101).
- **Shinbashira**: cilindro de hormigón blanco Ø 2–3 m en el centro, de suelo a techo, con un aro de amortiguadores en cada losa.

### Alzado ASCII (una cara braceada, 2 pisos de 11 m)

```
        ══════╦═════════════════════════╦══════   ← losa piso 2 + anillo-cinturón (rojo óxido, 1,2 m)
             [■]  ╲                   ╱  [■]      ← collar de nudo Ø D+0,3
              ║     ╲  BRB amarillo ╱     ║
              ║       ╲           ╱       ║
              ╟─┤       ╲       ╱       ├─╢       ← collar de empalme (cada ~5 m)
              ║           ╲   ╱           ║
              ║            ╳              ║  11 m
              ║          ╱   ╲            ║
              ▓        ╱       ╲          ▓       ← franja CFRP negra (2 m inferiores del tramo)
        ══════╬═════════════════════════╬══════   ← losa piso 1 + anillo-cinturón
             [■]  ╲                   ╱  [■]
              ║     ╲               ╱     ║
              ║       ╲    BRB    ╱       ║       Columna CFST Ø 0,9–1,2 m
              ╟─┤       ╲       ╱       ├─╢       (acero gris azulado)
              ║           ╲   ╱           ║  11 m
              ║            ╳              ║
              ▓          ╱   ╲            ▓
            ┌─┴─┐                       ┌─┴─┐     ← placa de acero superior
            █████  aislador LRB         █████     ← caucho negro Ø 1,1–1,5 × 0,4 m
            └─┬─┘                       └─┬─┘     ← placa de acero inferior
           ┌──┴──┐                     ┌──┴──┐    ← pedestal Ø D+0,6 × 0,5
     ▒▒▒▒▒▒█████████▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒█████████▒▒ ← encepado 2,6–3,2 m (asoma 0,4 m), musgo alrededor
          ┃ ┃ ┃ ┃ (pilotes / termosifones Ø 0,1 asomando con aletas)
```

---

## 3. Qué conservar y qué simplificar para el juego

Lo que hay ahora genera 1 cubo de columna y 3–4 zunchos por piso y columna. El objetivo es **6–9 primitivas por columna y piso**,
y el resto compartido por la plataforma.

**Conservar (se lee de lejos y es lo que la hace «real»):**
1. El fuste **cilíndrico** de acero (1 cilindro): sustituye al cubo de hormigón.
2. El **aislador negro entre dos placas** en la base (3 primitivas, solo en la planta baja): es la firma visual del diseño antisísmico.
3. El **collar** en cada losa (1 cilindro) y **un** collar de empalme a media altura (1 cilindro), que sustituyen a los zunchos cada 3 m.
4. El **anillo-cinturón** de color acento bajo cada losa (8 cubos por piso para toda la plataforma, no por columna).
5. Las **BRB amarillas en X en 4 caras** (8 cubos por piso para toda la plataforma).
6. El **encepado** (1 cubo por columna, solo abajo).

**Simplificar o quitar:**
- Pernos, aros de las chapas del aislador y cartelas de las BRB: fuera, o como textura o normal map.
- Celosía del cinturón: un cubo macizo en vez de cordones y diagonales.
- Franja CFRP: opcional (1 cilindro más). Se puede dejar solo en la planta baja.
- Reducción de diámetro con la altura: solo cada 2 pisos, o ninguna.
- Pilotes y termosifones: no se ven (enterrados). Si acaso, 2 tubos con aletas en una columna de cada edificio, como detalle.
- TMD y shinbashira: solo en el edificio más alto, como pieza «de interés».
- **Rendimiento**: un material compartido por color (acero, caucho, acento, amarillo y hormigón) para que se agrupen en el batching,
  colliders solo en el fuste (capsule) y en el encepado, y los anillos y las riostras sin collider (salvo que deban pisarse).

Cuenta orientativa para una plataforma de 5 pisos: 8 columnas × (1 fuste + 2 collares) × 5 pisos = 120, más la base
8 × (encepado + pedestal + 3 del aislador) = 40, más 5 pisos × (8 cinturones + 8 BRB) = 80. **Total ≈ 240 primitivas.** Con la
caja actual y zunchos cada 3 m salen unas 8 × 4 × 5 = 160, así que el aumento es moderado y gana mucho en lectura.
