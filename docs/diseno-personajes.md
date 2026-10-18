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
| **Kushal** (jugador) | Recién llegado; 1,75 m (DEVLOG). Sin edad ni nacionalidad. | **Inferido**: el nombre es nepalí o indio ("hábil, bienestar"). Piel morena, pelo negro corto, ojos marrón oscuro. Parka azul petróleo. |
| **La Magnate** | Maestra del santuario, maga de células con stats infinitas; su base es el 5.º santuario. Sin aspecto ni edad. | **Inferido**: 1,70 m, melena larga plateada, ojos azules, túnica violeta de maestra. |
| **Linduarda** | Médica ecuatoriana que estudió en Ucrania; antes fue camarera en EE. UU. "A la moda, deportista… gimnasio a diario". | 1,60 m, atlética. Coleta alta negra, piel morena, ojos marrones. Bata blanca. |
| **Goluis** | 25 años; el personaje de la fuerza, corpulento; jefe de cocina; de adolescente estuvo en una banda. Sin nacionalidad. | 1,82 m, corpulento (por stats). Cabeza rapada, barba, piel morena. Chaqueta blanca de cocinero. |
| **Panterilia** | 42 años; el personaje de la percepción, delgada; dirige la limpieza y la farmacia; alma 90 % humana y 5 % león. Sin nacionalidad. | 1,68 m, delgada. Melena leonada, ojos ámbar (el toque del león). Ropa verde oliva. |
| **Gohageneis** | 28 años, "el Papi"; dejó su casa a los 16: Venezuela → Colombia → Perú → Ecuador → España → EE. UU.; equilibrado tirando a ágil. | 1,76 m, ágil. Pelo negro rizado, barba corta, piel morena. Chaqueta granate. |
| **Irosene** | Más de 70 años, ecuatoriana, madre de 9; antes con sobrepeso y secuelas de un cáncer, hoy corre, escala y bucea; destaca en el textil. | 1,52 m. Canas cortas, piel morena, ojos marrones. Tejido andino coral. |
| **Cocineros** | Personajes genéricos de la cocina. | Gorro y chaqueta blancos; tono de piel variado. |

## Pendiente de confirmar

1. La nacionalidad de Kushal, Goluis y Panterilia.
2. El aspecto de la Magnate (edad aparente, pelo, ropa).
3. Si alguien lleva algo propio, como gafas, tatuajes o una prenda concreta.
