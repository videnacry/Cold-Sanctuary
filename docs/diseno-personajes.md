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
| **Tofelite «Tote»** (esposo de Lizmibet, jefe de construcción; id `JefeConstruccion`) | Dirige las obras y reparaciones de las áreas humanas (`construction-simulation.md`). | **Según su foto:** 1,68 m, robusto. Pelo corto oscuro entrecano peinado hacia atrás, gafas de sol oscuras, piel morena clara. Polo gris claro, bermudas verde salvia con cinturón, chanclas negras y reloj. Trabaja en el área de Construcción. |
| **Sadhguru** | Da las clases en el centro de yoga (la práctica de cuello de upa-yoga, `upa-yoga-mission.md`). | **Según su foto:** 1,72 m. Turbante crema con una franja granate, barba larga blanca y bigote poblado, piel morena. Camiseta sin mangas verde agua, dhoti blanco, chal blanco con ribete turquesa, guirnalda de flores, descalzo. En la tarima de la sala de yoga. |
| **Andrea** | Italiano, uno de los dueños del santuario (heredó las tierras); risueño y acogedor, corre 25 km suave; siempre elegante (`character-andrea.md`). | **Según su foto y su ficha:** 1,78 m, joven. Pelo castaño oscuro ondulado y rizado con volumen arriba. Camisa negra de cuello, pantalón de vestir gris marengo, zapatos de piel, cadena plateada con cruz, anillos, pendientes y pulsera. En el comedor del restaurante. |

## Pendiente de confirmar

1. La nacionalidad de Kushal, Goluis y Panterilia.
2. El aspecto de Panterilia (no hay referencia todavía).
3. ¿El rifle de Kushal es de dardos tranquilizantes? El santuario no mata animales.
4. ~~El nombre del esposo de Lizmibet.~~ Tofelite, «Tote»; a Lizmibet la llaman «Liz» (Berón 2026-10-31).
