# Diseño de los personajes del santuario

Aspecto de cada persona en el cuerpo de cubos: estatura, complexión, piel, pelo, ojos y ropa. Sale de lo que dicen
sus fichas. Donde la ficha no dice nada, se marca como **inferido** para que se confirme o se corrija. El código
está en `Assets/Scripts/Avatar/CubeHair.cs` (`CubeHair.For`). La complexión (músculo y grasa) la pone `BodyShape`
a partir de sus stats ([body-shape-from-stats.md](body-shape-from-stats.md)).

Para ver los retratos: menú **Tools > Cold Sanctuary > Character Gallery**, que genera
`Temp/PoseGallery/Personajes.png` (de frente y de lado, a escala real).

## Criterios de la investigación

- **Estatura**: media adulta del país de origen; si la ficha no dice el país, una media del personaje según su papel.
  - Ecuador: mujeres ≈ 1,55 m, hombres ≈ 1,67 m.
  - Venezuela: hombres ≈ 1,72 m.
  - Sur de Asia: hombres ≈ 1,65–1,70 m.
- **Piel, pelo y ojos**: Ecuador y Venezuela tienen una población mayoritariamente mestiza, de piel morena clara a
  media, pelo negro o castaño oscuro y ojos marrones. Se usan esos tonos salvo que la ficha diga otra cosa.
- **Ropa**: la de su oficio en el santuario: bata en la enfermería, chaqueta blanca en la cocina, tejido en el textil.
- **Edad**: se nota en el pelo (canas) y, cuando la ficha lo dice, en la complexión.

## Fichas

| Persona | Lo documentado | Diseño |
|---|---|---|
| **Kushal** (jugador) | Recién llegado; 1,75 m (DEVLOG). Sin edad ni nacionalidad. | **Según su hoja de diseño:** media melena negra, gorro de lana rojo, gafas redondas, barba de unos días, piel clara rosada. Chaleco morado con cuello de borrego y chapas sobre camiseta azul marino, vaqueros verde oscuro con cadena, muñequera, zapatillas grises de bota, tatuaje en el brazo y un rifle colgado a la espalda. |
| **La Magnate** | Maestra del santuario, maga de células con stats infinitas; su base es el 5.º santuario. Sin aspecto ni edad. | **Según su hoja de diseño:** 1,66 m. Bob azul noche con flequillo recto y mechones largos delante, ojos marrones, labios morados, piel clara amarillenta. Blusa blanca de cuello alto, bolero morado de mangas abullonadas, corsé granate con ribete dorado, pantalón bombacho celeste y botas altas azul marino. |
| **Linduarda** | Médica ecuatoriana que estudió en Ucrania; antes fue camarera en EE. UU. "A la moda, deportista… gimnasio a diario". | **Según su captura de referencia:** 1,60 m. Coleta alta negra, gafas redondas doradas, piel clara-morena. Pijama sanitario azul celeste sobre camiseta blanca de manga larga. |
| **Goluis** | 25 años; el personaje de la fuerza, corpulento; jefe de cocina; de adolescente estuvo en una banda. Sin nacionalidad. | **Según su captura:** 1,75 m, corpulento. Pelo degradado con la parte de arriba en punta, perilla, pendiente, tatuajes en los antebrazos. Polo claro. |
| **Panterilia** | 42 años; el personaje de la percepción, delgada; dirige la limpieza y la farmacia; alma 90 % humana y 5 % león. Sin nacionalidad. | 1,68 m, delgada. Melena leonada, ojos ámbar (el toque del león). Ropa verde oliva. |
| **Gohageneis** | 28 años, "el Papi"; dejó su casa a los 16: Venezuela → Colombia → Perú → Ecuador → España → EE. UU.; equilibrado tirando a ágil. | **Según su captura:** 1,76 m, atlético. Pelo corto, sin barba, gorra azul con la «V» amarilla y roja (la de Venezuela). Camiseta blanca y pantalón corto rojo. |
| **Irosene** | Más de 70 años, ecuatoriana, madre de 9; antes con sobrepeso y secuelas de un cáncer, hoy corre, escala y bucea; destaca en el textil. | **Según su captura:** 1,55 m. Melena larga y ondulada, rubia con canas, con flequillo. Piel clara, ojos azul grisáceo. Tejido andino coral. |
| **Lizmibet** | Más de 70 años, ecuatoriana, médica; la gran amiga de Irosene. | **Según su foto:** 1,55 m. Pelo corto rizado y voluminoso castaño cobrizo, gafas finas plateadas, piel clara. Vestido blanco campesino de manga larga hasta los tobillos y sandalias blancas. Trabaja en la enfermería. |
| **Tofelite «Tote»** (esposo de Lizmibet, jefe de construcción; id `JefeConstruccion`) | Dirige las obras y reparaciones de las áreas humanas (`construction-simulation.md`). Comparte casa con Lizmibet (2026-11-04). | **Según su foto `tote.jpg` (2026-11-04; antes, la foto con Lizmibet):** 1,68 m, robusto. Piel morena, pelo corto oscuro, gafas de sol envolventes oscuras. Camisa rosa pálido de rayas finas con las mangas remangadas y el cuello abierto, pantalón gris de vestir con cinturón oscuro, zapatos marrones de cordones y reloj dorado en la muñeca izquierda. Trabaja en el área de Construcción. |
| **Andrea** | Italiano, uno de los dueños del santuario (heredó las tierras); risueño y acogedor, corre 25 km suave; hacía yoga con Leo; siempre elegante (`character-andrea.md`). Fue maestro de yoga unas horas el 2026-11-04; ahora el maestro es Josedrilo y a Andrea le toca el gimnasio (rama del entrenamiento). | **Según su foto y su ficha:** 1,78 m, joven. Pelo castaño oscuro ondulado y rizado con volumen arriba. Camisa negra de cuello, pantalón de vestir gris marengo, zapatos de piel, cadena plateada con cruz, anillos, pendientes y pulsera. En la tarima de la sala de Hatha tradicional. |

**Ya no viven en el santuario** (2026-11-04, se fueron a sus microcosmos): Sadhguru y los seis del mesocosmos (Elon Musk, Goku,
Bill Gates, Warren Buffett, MacKenzie Scott, Ibrahim Traoré). Su aspecto sigue en `CubeHair.For`; quién lleva ahora sus papeles:
[personajes-mesocosmos.md](personajes-mesocosmos.md) §Reparto de papeles.

## Familia y amigos de Berón (2026-11-04)

Berón: «crea a Nesvitor (nestor), amahuz y karen, Alicita (alicia), Ducristin (semigod), josedrilo-maite-shalva (cumbaya) y mejora
el modelo del esposo de lizmibet (tote)». Mandó también la foto de Adaguchi, y confirmó después que se queda.

Son personas reales:
- solo rasgos visibles en sus fotos (pelo, piel, ropa, complementos, complexión), con respeto y sin caricatura;
- sin suponer edades: estaturas estimadas y stats cercanas a 1;
- `Fem()` (cejas finas, pestañas) solo para quien Berón nombra en femenino: Karen, Alicita y Maite.

Retratos: menú **Tools > Cold Sanctuary > Character Gallery (familia y amigos de Berón)** → `Temp/PoseGallery/Familia.png` y
`FamiliaCaras.png`. Datos en `SampleSceneBuilder.FamilyCharacters`; aspecto en `CubeHair.For`.

| Id | Foto | Papel | Aspecto (cubos) | Estatura |
|---|---|---|---|---|
| `Nesvitor` (Néstor) | `nestor.jpg` | **Consejero** (personal general; las cuentas pasan a Alicita) | Piel curtida morena; pelo canoso y negro peinado hacia atrás (`Style.SweptBack`); bigote espeso canoso; forro polar negro abierto sobre camisa blanca de cuadros finos azules y camiseta amarilla; gafas colgadas de la camisa | 1,68 m |
| `Amahuz` | `amahuz_karen.jpg` (derecha) | **Cocina y química**; pareja de Karen, comparten casa (de la foto: pendiente de que Berón lo confirme) | Delgado, piel clara morena; pelo castaño oscuro; bigote fino; polo azul marino de manga corta; pantalón oscuro; bolso negro cruzado | 1,76 m |
| `Karen` | `amahuz_karen.jpg` (izquierda) | **Asuntos legales y lenguas** (su mesa, en el Aula); pareja de Amahuz | Piel morena cálida; melena larga castaña oscura con pinza de flor rosa y amarilla; pendientes pequeños; blusa blanca sin mangas con volantes; falda berenjena con flores rosas y lilas; reloj negro | 1,58 m |
| `Alicita` (Alicia) | `alicia.jpg` | **Contabilidad y dirección de proyectos** (las cuentas del santuario y las obras) | Piel clara morena; melena larga lisa negra; aros pequeños; chaqueta negra con cremallera, cerrada, con el cuello verde fosforito de la camiseta asomando | 1,58 m |
| `Ducristin` | `semigod.jpg` | **Mecánica**: vehículos y maquinaria (antes Elon Musk); pareja de Gallarle, comparten casa | Piel morena; pelo corto castaño oscuro levantado delante (`Style.Quiff`); gafas de aviador de cristal degradado; camisa azul clara de cuadros con cuello abierto; muñequera roja | 1,70 m |
| `Josedrilo` | `cumbaya.jpg` (izquierda, **supuesto**) | **Maestro de yoga** (el profesional; guía la clase) y **bioconstrucción**; pareja de Maite, comparten casa | Melena larga castaña oscura recogida atrás (`Style.TiedBack`); chaqueta de cuero negra; pantalón granate | 1,75 m |
| `Maite` | `cumbaya.jpg` (centro, **supuesto**) | **Cocina**; pareja de Josedrilo | Melena larga ondulada castaña; piel clara; parka con capucha verde oliva parduzco sobre camiseta gris | 1,65 m |
| `Shalva` | `cumbaya.jpg` (derecha, **supuesto**) | **Cultivo y bioconstrucción** (el huerto y las obras) | Pelo corto castaño; jersey grueso de canalé verde oliva; vaqueros azul claro | 1,72 m |
| `Adaguchi` | `adaguchi.png` | **Submarinismo** (submarino de obra, ambulancia, tareas bajo el agua) y **cocina** | Piel clara morena; pelo corto ondulado castaño oscuro con volumen arriba (`Style.Fade`); un pendiente pequeño; sudadera negra sin mangas con capucha; collar de cuentas oscuras | 1,65 m |
| `Gallarle` | **pendiente de foto** | Pareja de Ducristin (comparten casa); **sin profesión** hasta que Berón la diga | Aspecto neutro provisional: pelo corto castaño, piel media, camiseta gris azulada, pantalón oscuro (sin rasgos inventados ni `Fem()`) | 1,70 m |

Piezas nuevas de `CubeHair.Look` (reutilizables):
- **Pelo**: `SweptBack` (hacia atrás), `Quiff` (tupé), `TiedBack` (melena recogida en la nuca).
- **Cara**: `Moustache(color, fino)`, `Shades(montura, cristal, aviador)` (envolventes o de aviador), `HairClip`, `Earrings(aro)`.
- **Ropa** (dibujada por `CubeHumanoid.FamilyDetails`): `Jacket(camisa, camiseta, cerrada)`, `Pattern(color, Weave.Checks/Stripes/Ribs)`,
  `Collar(abierto)`, `RolledSleeves`, `Ruffles`, `Hood`, `Skirt(color, flores)` (muslos al aire), `Belt`, `Bag`, `Watch`,
  `Band(color)`, `HangingGlasses`, `Beads`.

Sin hacer: la pose relajada de Tote en su foto (no hay pose propia por persona) y las sonrisas amplias de Karen y Alicita (la cara
de cubos cambia de expresión según el ánimo, no por persona).

## Pendiente de confirmar

1. La nacionalidad de Kushal, Goluis y Panterilia.
2. El aspecto de Panterilia (no hay referencia todavía).
3. ¿El rifle de Kushal es de dardos tranquilizantes? El santuario no mata animales.
4. ~~El nombre del esposo de Lizmibet.~~ Tofelite, «Tote»; a Lizmibet la llaman «Liz» (Berón 2026-10-31).
5. **Cumbaya.jpg**: se supuso, de izquierda a derecha, Josedrilo, Maite y Shalva. Berón tiene que confirmarlo.
