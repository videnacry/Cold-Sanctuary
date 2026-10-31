# Fachada de fotobiorreactores del Árbol de los Campos (investigación → plan → construcción)

Pedido de Berón (2026-10-31): aprobó la **«fachada de fotobiorreactores»** para el Árbol de los Campos. El fitoplancton da
más o menos la mitad del O₂ de la fotosíntesis del planeta. En un fotobiorreactor, las microalgas (espirulina, Chlorella)
rinden mucho más por m² que los árboles. La ESA usa *Arthrospira* en MELiSSA para dar O₂ a la tripulación, y la espirulina se
come. **No va en el agua del tronco**: se pondría verde y taparía a los nadadores. Va en una fachada de tubos y paneles en
las caras libres del tronco y en las ramas, mirando al sol, con LED en la noche polar y un cuidado diario.

Código:
- `Assets/Scripts/World/FotobioFacade.cs`: `FotobioSpec` (medidas y números) y `FotobioFacade` (el cultivo en juego, su
  cuadro, los LED, el cuidado y la misión).
- `Assets/Editor/SampleSceneBuilder.Fotobio.cs`: la construye, después de `BuildTree`.
- `AreaWork.Today`: la misión diaria.
- `Assets/Scripts/Tests/FotobioFacadeTest.cs`: la prueba (grupo 56).

## 1. Investigación

### Qué alga: *Arthrospira* (espirulina) mejor que *Chlorella*

| | *Arthrospira platensis* (espirulina) | *Chlorella vulgaris* |
|---|---|---|
| Medio | Alcalino: **pH 9,5–10,5**, mucho bicarbonato (medio de Zarrouk) | Neutro (pH 6–8) |
| Contaminación | El pH alto frena a casi todo. A pH 10,5 Chlorella crece mucho menos, aunque acaba apareciendo. Bicarbonato alto y poco CO₂ gaseoso la mantienen fuera (Richmond et al. 1982; estudio de BEA 005B) | Muy invadible (es, de hecho, el contaminante típico de la espirulina) |
| Cosecha | **Filamentos** de 0,3–0,5 mm: se cosecha con **malla** de 30–50 µm, se lava y se prensa | Células de 2–10 µm: hace falta centrífuga o floculante |
| Comida | Comestible tal cual (proteína ~60 %, ficocianina); la ESA la eligió como alimento | Comestible, pero su pared celular se digiere mal sin romperla |
| Vida en el espacio | **MELiSSA, compartimento IVa**: produce O₂ para la tripulación (planta piloto en Barcelona con ratas como tripulación) | — |

**Elegida: espirulina.** Por su pH se cuida sola de los contaminantes, se cosecha con un tamiz y se come.

### Condiciones de cultivo

- **Temperatura**: óptimo **30–35 °C** (Tomaselli et al. 1987: máximo a 35 °C; otros, 35–38 °C al aire libre). Por debajo de
  ~20 °C apenas crece, y por debajo de 12–15 °C no aguanta luz fuerte (fotoinhibición). Por la noche conviene dejarla
  enfriar: la respiración nocturna gasta hasta un 30 % de lo crecido de día si se mantiene a 31 °C, y menos del 20 % si la
  noche sigue al ambiente (estudios TURP).
- **pH**: 9,5–10,5. El bicarbonato es la fuente de carbono y el tampón.
- **Medio de Zarrouk** (g/L): NaHCO₃ 16,8 · NaNO₃ 2,5 · K₂HPO₄ 0,5 · K₂SO₄ 1,0 · NaCl 1,0 · MgSO₄·7H₂O 0,2 ·
  CaCl₂·2H₂O 0,04 · FeSO₄·7H₂O 0,01 · EDTA 0,08, más micronutrientes (Egyptian Journal of Botany 2021, «normal» Zarrouk).
- **Densidad de trabajo**: 0,5–1,5 g/L. Por encima, el cultivo se hace sombra a sí mismo.

### Productividad

| Sistema | Productividad | Fuente |
|---|---|---|
| Tubos ondulados al aire libre (TURP-10r), verano | 35 g/m²/día; 2,2 g/L/día en el TURP-5r | Carlozzi et al. 2000, CNR Florencia |
| TURP-10r, por m² de instalación | 47,7 g/m²/día | CNR (registro *Hydrodynamic aspects…*) |
| Tubos de 5 cm (Florencia): otoño, invierno, marzo | 0,83 · 0,44 · 0,61 g/L/día | Torzillo et al. (CNR) |
| Tubos vs paneles planos | Los tubos aprovechan mejor el sol: su curva «diluye» la luz y satura menos | Tredici y Chini Zittelli 1998 (*Biotechnol. Bioeng.*) |
| Fachada de paneles BIQ (Hamburgo, 200 m²) | ~23 kWh/m²/año en biomasa: un 4 % de conversión, la mitad de lo previsto | Arup / SSC; Ingenia, RIBA Journal |

**Diseño: 20 g/m²/día** por m² iluminado, con luz plena (sol o LED). Es prudente: por debajo de los tubos al aire libre de
Italia, porque una fachada vertical rinde menos, y por encima de la fachada BIQ, porque aquí los LED dan luz todo el año y
el cultivo está templado. La prueba exige que quede entre 10 y 48 g/m²/día.

### O₂ por gramo de biomasa

- **Estequiometría**: la biomasa de *Arthrospira* es CH₁,₆₅O₀,₅₃N₀,₁₇ (Cornet, MELiSSA). Con **nitrato** como fuente de
  nitrógeno, su grado de reducción es 5,4: salen **1,36 mol de O₂ por C-mol**, unos **1,6 g de O₂ por g** (sin ceniza).
  Con amonio, ~1,2 g/g.
- **Medido en MELiSSA**: cociente fotosintético 1,2 ± 0,1 (literatura: 1,29–1,40).
- **Diseño: 1,5 g de O₂ por g de biomasa seca** (con ~7 % de ceniza). La prueba exige 1,2–1,8.

### Una persona

- **0,84 kg de O₂ al día** (NASA RP-1324; BVAD).
- **MELiSSA**: un fotobiorreactor *air-lift* de 100 L acoplado a ratas. Daba 1,2–2,8 g de O₂ por hora y ajustaba la
  producción con la **intensidad de luz**. Meta del proyecto: el O₂ de una persona y ≥ 20 % de su comida.
- **BIORAT**: estimó **~475 L** por persona con su reactor.

### Cosecha, contaminación y frío

- **Cosecha diaria** por semicontinuo: se saca cultivo hasta dejarlo en ~0,7 g/L y se repone con medio nuevo.
  - Se filtra por **malla de 30–50 µm**, se lava (quita las sales) y se prensa a pasta (~20 % de materia seca).
  - Una malla de 100 µm dejaba crecer la Chlorella.
  - Con un **filtro de 0,2 µm** en la recirculación desaparecía, y la espirulina rendía un 15 % más.
- **Contaminantes**: Chlorella, rotíferos y amebas. La cura es el pH alto, el bicarbonato alto, el filtrado y **mirar al
  microscopio cada día**.
- **Frío** (−77,85°, −18 °C de media fuera):
  - el cultivo va **cerrado y templado**: tubos de vidrio con el calor de la central (calefacción de distrito);
  - **33 °C de día y ~22 °C de noche**, para respirar menos;
  - nunca por debajo de 15 °C con luz fuerte.
  - El hielo de fuera no toca el cultivo: los tubos son de vidrio de borosilicato (como los de los TURP), con una
    envolvente de aire templado. **[Supuesto de diseño]**: no se ha calculado el aislamiento.

### Luz: sol y LED

- **Noche polar**: unos **4 meses sin sol** (24 abr – 19 ago). En verano, unos 4 meses de sol de medianoche, aunque bajo:
  **≤ 35,6°** en el solsticio.
- **Rendimiento con luz**: el máximo teórico es ~1,8 g de biomasa por mol de fotones (8 fotones por O₂). En reactores se
  mide bastante menos. Perego, Converti y Bezerra lo dan en moles de fotones por C-mol: no varía con la intensidad, así que
  manda la luz. **Diseño: 0,6 g/mol (prudente).**
- **LED hortícolas**:
  - DLC exigía ≥ 2,3 µmol/J (v3.0) y ≥ 2,5 µmol/J (v4.0);
  - los buenos dan 2,7–3,0 y el techo está en 3,4–4,1 (Kusuma, Pattison y Bugbee 2020);
  - la funda estanca quita ~10 %. **Diseño: 2,5 µmol/J.**
- **Con poca luz rinde más cada vatio**: por eso se baja la luz cuando el cultivo está claro (Niangoran et al., OCL 2021).
- **MELiSSA** regula el O₂ con la luz: es lo que hace aquí el cuadro.

## 2. Plan

### Dónde: fuera del agua y sin tapar a los nadadores

**Opciones que se descartaron**:
- **Todo el tronco**: lo taparía.
- **Solo la parte baja**: es la que se ve desde el hielo, de donde se mira nadar.
- **Paneles sobre los lados de las ramas**: taparían el tubo por el que se nada.

**Elegida**:

| Parte | Qué | Dónde | Por qué ahí |
|---|---|---|---|
| **Tronco** | **Arpas de tubos** horizontales de vidrio (Ø 7 cm, cada 14 cm: 28 por módulo) en módulos de **3 × 4 m** con sus colectores. 4 módulos por columna (37,6–56 m) en 2 vanos de cada cara: **32 módulos, 384 m²** | Vanos **0 y 2** de los 6 de cada cara (entre nervios), a **1,4 m de la pared** (por delante de los nervios, que sobresalen 0,6 m) | El **carril de nado**, las **puertas** (x = 5 m) y las **bocas de las ramas** (x = 6,5 m) van por el vano 4: quedan libres los vanos 3, 4 y 5 en toda la altura (el módulo más cercano, a 2,8 m del carril). De **0 a 37,6 m** el tronco se ve entero desde el hielo, y de **56 a 92 m**, desde los pisos. Cada cara queda cubierta un **5,5 %**. El tubo es medio hueco: entre tubo y tubo se ve el agua |
| **Ramas** | Una **quilla** de 6 paneles planos (2,4 × 2 m, 5 cm de cultivo) por rama: **24 paneles, 115 m²** | Colgada bajo el **tramo largo** de cada rama, entre los cordones de abajo de su celosía, de x = 10 a 25 m (la boca está en 9 y la columna en 26) | Los **lados del tubo** quedan libres (se ve nadar por la rama). Lo más bajo queda a **60,3 m**, por encima del árbol más alto más su holgura (59,5 m). Quedan 5,3 m sobre los pisos de debajo y 8,4 m bajo los de encima |

- **Total**: **499 m²** iluminados y **13,4 m³** de cultivo (20 L/m² en los tubos, 50 L/m² en los paneles).
- **Orientación**: el árbol **gira** (una vuelta cada 6 h de juego), así que cada cara pasa por el sol. El tronco es de agua
  transparente, y el sol que entra por una cara alumbra también la arpa de la cara de enfrente.

### O₂ frente a la gente del santuario

**Gente**: el personal de la jornada (`WorkdayDirector.staff`) son 10:
- Kushal (el jugador);
- Linduarda, Lizmibet («Liz»), Goluis, Panterilia, Gohageneis e Irosene;
- Tofelite («Tote»), Sadhguru y Andrea.

Más **la Magnate** y **el recién llegado** de turno: **12 personas** (`PopulationPlan.Humans` = personal + 2). Respiran
12 × 0,84 = **10,1 kg de O₂ al día**.

| | A pleno rendimiento (salud 100 %) | Con salud del 85 % (lo normal cuidándolo) |
|---|---|---|
| Biomasa | 499 m² × 20 g = **10,0 kg/día** (0,75 g/L/día: dentro de lo de los tubos, 0,4–2,2) | 8,5 kg/día |
| O₂ | × 1,5 = **15,0 kg/día** | **12,7 kg/día** |
| Personas | **17,8** | **15,1** (de 12: sobra un 26 %) |
| m² por persona | **28 m²** (dentro de los 15–30 que se esperaban) | — |

- El aire del santuario es la atmósfera: nadie se ahoga si el cultivo falla.
- La fachada cubre **el balance de la gente**: el O₂ que respiran y el CO₂ que sueltan, como el lazo de MELiSSA.
- Su cuadro dice cuántas personas cubre ahora.

### LED de la noche polar

Dar la luz de diseño sin sol cuesta:
- **Fotones**: 10,0 kg/día ÷ 0,6 g/mol = 16 640 mol de fotones/día.
- **Energía**: ÷ 2,5 µmol/J = 6,7 GJ/día ≈ **77 kW** seguidos (**1,85 MWh/día**). Es **~385 µmol/m²/s** en la cara del
  cultivo, por debajo de donde la espirulina se satura.
- **En la noche polar** (~117 días): ~216 MWh.
- **Frente a la central**: es el 1,5 % del microrreactor (eVinci, 5 MWe) y un 6–10 % de lo que gasta el santuario de media
  (0,8–1,2 MWe; docs/edificios-investigacion.md).

En juego (`FotobioFacade.Light`):
- el sol en una fachada vertical que gira cuenta como sin(altura)/sin(25°) × 0,9;
- si no llega al 75 %, se encienden los LED y rellenan;
- el cuadro dice los kW.

Las regletas van **detrás de cada arpa** (entre los tubos y el acrílico) y a los dos lados de cada quilla. Se ven magenta
(rojo + azul) cuando están encendidas.

### Peso (el tronco lo carga)

| Pieza | Peso |
|---|---|
| Un módulo de tubos | 0,55 t: 238 L de cultivo, 103 kg de vidrio, colectores, marco, LED y ménsulas |
| Bombas, desgasificadores y tuberías | 3 t |
| **Tronco** | 32 × 0,55 + 3 = **20,6 t** |
| Un panel de quilla | 0,39 t |
| **Quilla de una rama** | 6 × 0,39 = **2,3 t**, a ~19 m de la cara: **0,44 MN·m** más en el voladizo de la rama |
| **Total** | ~30 t = **0,29 MN**: un **0,2 %** más sobre los ~174 MN del tronco |

- Lo comprueba `FotobioSpec.Structure`: `StructuralLimits.RootCheck` del árbol más este peso (axil y voladizo de la rama).
  Sale en el registro `[Estructura] Tronco con la fachada…`.
- **Las ménsulas**: cada módulo cuelga de 4 ménsulas atornilladas a 2 nervios (~1,4 kN cada una).
- **El viento catabático** manda más que el peso. Con 50 m/s, solidez 0,5 y Cd 1,2 son ~11 kN por módulo.
  **[Supuesto]**: las ménsulas son para eso, pero no se han calculado.

### La sala de cuidados: el laboratorio de Biología (Ciencias)

**Se eligió el laboratorio de Biología de la torre de Ciencias**, no el pie del árbol:
- El pie del árbol debe quedar libre para los árboles de hielo.
- El laboratorio ya tiene lo que hace falta: microscopio, balanza, cabina, autoclave para el medio, pHmetro e incubador
  para la cepa de reserva.

**Al pie**, colgado en la cara a del tronco (lejos de la puerta del suelo), va solo el **cuadro de control**: bombas, pH,
temperatura y LED, con la etiqueta del cultivo.

**El cultivo es automático**: bombas, desgasificador en lo alto de cada columna, dosificador y tamiz de cosecha. La persona
hace lo que una máquina no hace bien:
- mirar al microscopio;
- medir y corregir;
- preparar el medio;
- vaciar el tamiz y prensar la pasta.

### El trabajo diario

Misión **`bio_fotobio`** de la jornada (`AreaWork.Today` → `FotobioFacade.Missions`): **«Fotobiorreactores del Árbol:
muestra (densidad, pH y microscopio), medio de Zarrouk, cosecha de espirulina y lavado»**. Es **obligatoria** (un cultivo
sin cosechar se espesa y se contamina en pocos días), dura **1 h** y es de **Biología**. La reparte la rotación como las
demás: cualquiera puede tenerla, y la Magnate inicia al que la hace por primera vez.

**El trabajo** (1 h):

| Tarea | Tiempo |
|---|---|
| Muestra: densidad por turbidez, pH y temperatura | 10 min |
| Microscopio: filamentos sanos, sin Chlorella ni rotíferos | 15 min |
| Preparar y dosificar el medio de Zarrouk | 15 min |
| Vaciar el tamiz de cosecha, lavar y prensar (~50 kg de pasta) | 20 min |

**En juego**:
- Cuando su turno termina, si quien la tiene estuvo en el laboratorio, se aplica `FotobioFacade.Care`:
  - cosecha hasta 0,7 g/L;
  - medio nuevo;
  - contaminación ×0,25;
  - salud +15 %.
- Si no fue, el cultivo se queda sin cuidar y lo avisa.
- No hay esperas: lo mira cada segundo en `Update`.
- Sin cuidar unos días, el cultivo se espesa (> 1,4 g/L: se hace sombra), se queda sin medio y se contamina. La salud baja y
  los tubos amarillean.

### La espirulina para la cocina

**Cuánta sale**: ~10 kg secos al día (~50 kg de pasta), mucho más de lo que se come. Una ración es de 3–10 g; hasta
~20 g/persona/día.

**Reparto propuesto**:
- **Cocina**: ~0,25 kg/día (batidos, pasta, el azul de la ficocianina como colorante).
- **Pienso** de la Cría y de las aves.
- **Abono** (biofertilizante) de los cultivos del árbol.
- **Hidrolizado** para el medio del laboratorio de carne cultivada (hay medios de cultivo celular con hidrolizados de algas).

**No se ha añadido como ingrediente**: en `Gastronomy.Raw` cada materia prima sale de un cultivo de `GardenCrop` (o del
laboratorio). La espirulina no es una planta del huerto, y añadirla bien es una tarea propia (receta real, nutrientes y
fuente nueva).

## 3. Construcción y prueba

**Construcción**: `SampleSceneBuilder.BuildFotobio(...)`, llamada justo después de `BuildTree` en `Build`. Monta la fachada
`Fachada_fotobiorreactores`, hija del árbol (gira con él). Lleva estas mallas, cada una de un solo dibujo:
- `Fotobio_cultivo`: los tubos y los paneles; su color pasa de verde azulado a amarillo con la salud;
- `Fotobio_marco`: colectores, montantes, ménsulas, colgadores y tuberías de subida;
- `Fotobio_LED`;
- `Fotobio_bombas`: una bomba abajo y un desgasificador arriba de cada columna, y una bomba por quilla.

También pone el `Cuadro_fotobio` con su etiqueta y el componente `FotobioFacade`.

**Registro**:
- `[Fotobio] Fachada: 32 módulos … 24 paneles en 4 ramas: 499 m², 13,4 m³ …`;
- `[Estructura] Tronco con la fachada…`.

**Etiqueta**: «Fotobiorreactores de espirulina · 499 m² · densidad 0,90 g/L · pH 9,9 · 33 °C · salud 85 % · O₂ 12,7 kg/día
(15,1 de 12 personas) · LED 77 kW · cuidado: …».

**Prueba**: `FotobioFacadeTest`, **grupo 56**. Se añade sola al pedir las pruebas.
1. **Existe**: cuelga del árbol con 32 módulos y 24 paneles; la superficie y las piezas están; la etiqueta enseña densidad,
   salud y O₂.
2. **Libre**:
   - ningún módulo a menos de 2 m de una puerta o de una boca de rama;
   - el carril de nado queda libre en cada cara (≥ 2 m);
   - cada cara del tronco, cubierta ≤ 15 %;
   - en la malla, los tubos del tronco solo entre 37,6 y 56 m;
   - las quillas, por encima de 59,5 m y bajo su tubo, entre la boca y la columna;
   - nada del árbol (copas de los cultivos, losas…) a menos de 1 m de una quilla.
3. **Números**:
   - productividad de 10–48 g/m²/día y O₂ de 1,2–1,8 g/g;
   - 15–60 m² por persona y 0,3–2,5 g/L/día;
   - LED de 150–600 µmol/m²/s y < 100 kW;
   - con salud del 85 % cubre a toda la gente;
   - el tronco aguanta.
4. **Cuidado**:
   - tres días sin cuidar: se espesa, se queda sin medio y baja la salud;
   - `Care` sube la salud, cosecha a 0,7 g/L y repone el medio;
   - cuidado cada día, la salud sigue subiendo y el O₂ pasa de lo que respira la gente;
   - la jornada tiene la misión (obligatoria, 1 h, en Biología) y existe el laboratorio.
5. **Espacio**:
   - las quillas, a 2 m de las plataformas de encima y de debajo;
   - la fachada, a ≥ 2 m de las zonas de alfombras;
   - nada fuera del árbol a menos de 2 m del círculo que barre la fachada al girar (~16,6 m de radio, entre 35,6 y 58 m).

## Riesgos y pendientes

- **Las quillas de nivel 2** pasan a 3 m del borde del bancal del bosque comestible del piso de debajo, a 5,3 m sobre la
  losa. Si el dosel crece hasta ahí, la prueba lo dirá; se puede subir la quilla o dejar solo las de nivel 1.
- **El viento y el aislamiento** de los tubos son supuestos de diseño, no calculados.
- **La cocina** no tiene aún la espirulina como ingrediente (ver arriba).
- **La sala de cuidados** es el laboratorio de Biología; no se le han añadido muebles propios (tamiz, prensa, tanque de medio).
  Si Berón lo quiere, van ahí como una línea de equipos.

## Fuentes

- Tredici, M. R. y Chini Zittelli, G. (1998). «Efficiency of sunlight utilization: tubular versus flat photobioreactors».
  *Biotechnol. Bioeng.* 57: 187–197. [unibo](https://acnpsearch.tweb-dev.unibo.it/singlejournalindex/50026165)
- Carlozzi, P. et al.: tubos ondulados TURP (35 g/m²/día; 2,2 g/L/día).
  - [Springer BF02178572](https://link.springer.com/article/10.1007/BF02178572)
  - [CNR](https://iris.cnr.it/handle/20.500.14243/127446)
  - [FAO AGRIS: «Hydrodynamic aspects and Arthrospira growth in two outdoor TURP»](https://agris.fao.org/search/zh/records/65df586f4c5aef494fe1e627)
- MELiSSA (ESA), compartimento IVa:
  - planta piloto con ratas (100 L, 1,2–2,8 g O₂/h, control por la luz): [melissafoundation.org/download/211](https://melissafoundation.org/download/211);
  - [download/670](https://melissafoundation.org/download/670);
  - BIORAT (~475 L por persona): [ECI](https://dc.engconfintl.org/biochem_xx/114);
  - cociente fotosintético 1,2 ± 0,1: [download/683](https://melissafoundation.org/download/683);
  - Cornet, Marty y Dussap 1998, O₂ y CO₂ en el compartimento de espirulina: [download/501](https://melissafoundation.org/download/501);
  - nitrato como fuente de N: [UMONS, *Bioresour. Technol.* 2018](https://orbi.umons.ac.be/bitstream/20.500.12907/40592/1/10.1016j.biortech.2018.07.101.pdf).
- Medio de Zarrouk (composición): [Egyptian Journal of Botany 2021](https://ejbo.journals.ekb.eg/article_45748.html).
- Temperatura y pH:
  - [Encyclopedia MDPI, «Growth Conditions … of Spirulina»](https://encyclopedia.pub/entry/38195);
  - [Plants 2022, 11:3063](https://mdpi.com/2223-7747/11/22/3063);
  - frío y luz: [PMC8700078](https://pmc.ncbi.nlm.nih.gov/articles/PMC8700078);
  - [Jourdan, «Grow your own spirulina»](https://we.riseup.net/assets/242053/Jourdan+UK.pdf).
- Contaminación por Chlorella:
  - pH 10,5 y filtro de 0,2 µm (+15 %): [Strategies to control unwanted microalgae in *A. platensis* BEA 005B](https://library.kab.ac.ug/Record/doaj-art-e8e86161e3f54b8f9967a7dad025d565/Description);
  - bicarbonato y CO₂ (Richmond et al.): [BGU](https://cris.bgu.ac.il/en/publications/effects-of-bicarbonate-and-carbon-dioxide-on-the-competition-betw-2/).
- Fachada BIQ (Hamburgo):
  - [Ingenia, «Algae-powered architecture»](https://ingenia.org.uk/articles/algae-powered-architecture/);
  - [RIBA Journal](https://www.ribaj.com/buildings/little-green-cells/);
  - [POCACITO](https://pocacito.eu/node/1164.html).
- O₂ por persona (0,84 kg/día; NASA RP-1324):
  - [Lunarpedia, «Needs and Effluents»](https://lunarpedia.org/w/Needs_and_Effluents);
  - [Wikipedia, «Life-support system»](https://en.wikipedia.org/wiki/Life-support_system).
- LED:
  - [MSU, «LED efficacy»](https://www.canr.msu.edu/floriculture/uploads/files/LED%20efficacy.pdf);
  - Kusuma, Pattison y Bugbee 2020 (límites de 3,4–4,1 µmol/J): [USU](https://digitalcommons.usu.edu/psc_facpub/983);
  - DLC v3.0 y v4.0: [UL](https://www.ul.com/news/changes-dlc-horticultural-requirements-v30), [LightNOW](https://www.lightnowblog.com/2025/02/dlc-finalizing-horticultural-v4-0-technical-requirements/).
- Energía de espirulina con LED (luz modulada, más eficiente con menos luz): [Niangoran et al., OCL 2021](https://www.ocl-journal.org/articles/ocl/full_html/2021/01/ocl210023/ocl210023.html).
- Fotones por C-mol (Perego, Converti; Bezerra): [FAPESP](https://bv.fapesp.br/en/publicacao/109659/growth-and-composition-of-arthrospira-spirulina-platensis).
- **Conocimiento general (sin ficha consultada)**:
  - estequiometría CH₁,₆₅O₀,₅₃N₀,₁₇ y el cálculo del grado de reducción;
  - Chisti 2007 (*Biotechnol. Adv.* 25: 294) para ~1,8 kg de CO₂ por kg de biomasa;
  - vidrio de borosilicato 2230 kg/m³;
  - viento de diseño.
