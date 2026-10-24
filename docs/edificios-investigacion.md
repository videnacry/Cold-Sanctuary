# Edificios de investigación y de producción

> Berón, 2026-10-23: un edificio de investigación por materia (Física, Química y Biología) y una central de
> producción. 2026-10-24: «fieles representaciones, con 2 m entre objetos para pasar corriendo, que 4 personas
> puedan hacer lo mismo, con anillos y segundos pisos». Fuentes y medidas: `docs/laboratorios-investigacion.md`.
> Código: `Assets/Editor/SampleSceneBuilder.Research.cs`.

## Dónde están
Están en el casquete Sur, junto a la planta atómica, unidos por puentes. Son tres lóbulos de hielo nuevos en ese casquete (no se añadió ningún casquete):

| Edificio | Centro | Planta | Pisos |
|---|---|---|---|
| Física (planta atómica) | (15, −101) | 24 × 18 | reactor TRIGA · ciclotrón · **sala de experimentos con anillo** |
| Química | (52, −110) | 24 × 20 | síntesis · análisis |
| Biología | (92, −112) | 32 × 20 | cultivo · molecular |
| Central nuclear (producción) | (60, −162) | 46 × 30 | una, vallada |

## Reglas de la distribución
- Cada equipo es un bloque (`Bloque_…`). Entre bloques hay 2 m libres, y 2 m al borde. `FurnitureSpacing` lo comprueba con la regla `Bloque_`.
- Lo que en la realidad va junto es un solo bloque: la campana con su mesa, las tres cámaras frías, el criostato con su bomba.
- Lo que hace una persona está ×4, para que cuatro hagan lo mismo a la vez:
  - en Química: campanas, rotavapores, HPLC y puestos;
  - en Biología: cabinas de seguridad, incubadores, microscopios, centrífugas, pre-PCR, extracciones y puestos.
- Los equipos grandes y caros están una sola vez, como en un laboratorio real: RMN de 400 MHz, DRX, caja de guantes, secuenciador y biorreactor de 50 L.
- En las salas se entra: la de RMN, el cuarto de balanzas, la cámara fría, la sala de control y la garita tienen paredes y la puerta abierta.

## Qué hay en cada uno
- **Química.**
  - Abajo, la síntesis, lo húmedo junto a las duchas: 4 campanas con línea Schlenk, 4 rotavapores, la caja de guantes, los armarios separados por riesgo (inflamables, ácidos y bases), 2 duchas con lavaojos y los residuos.
  - Arriba, el análisis: la sala de RMN con su línea de 5 gauss, la RMN de sobremesa, GC-MS, 4 HPLC, IR/UV-Vis, DRX, hornos, el cuarto de balanzas y 4 puestos.
- **Biología (BSL-2).**
  - Abajo, el cultivo: 4 cabinas de clase II, 4 incubadores de CO₂, 2 microscopios invertidos y 2 de fluorescencia, 4 centrífugas, el biorreactor de carne cultivada de 50 L, 2 bancos de biorreactores, 2 congeladores de −80 °C, la cámara fría, el autoclave con su fregadero y el dewar.
  - Arriba, la parte molecular, en sentido único: 4 pre-PCR, 4 extracciones, termocicladores, secuenciadores (MiSeq y MinION), neveras de −20 °C y 4 puestos de datos.
- **Física.**
  - En la sala, en zona oscura: 2 interferómetros de Michelson, 2 Millikan, 2 espectroscopios de red y la electrónica.
  - En el anillo: la doble rendija de fotón único, las placas de Chladni, el helio superfluido y la balanza de Cavendish.
  - En una esquina del anillo, el péndulo de Foucault: 15 m de hilo en su torre de 17 m.
- **Central nuclear (producción).** Un microrreactor tipo eVinci de 5 MWe que también da calor a la calefacción de distrito. Para el santuario se estiman 0,8–1,2 MWe de media; ver la investigación. Tiene:
  - el puesto de acceso;
  - la sala de control con 4 operadores y panel mímico;
  - la conversión de potencia y el intercambiador de la calefacción;
  - la subestación y el diésel de respaldo;
  - la losa de contenedores, porque el núcleo gastado vuelve a fábrica;
  - el almacén y la valla de seguridad.

## Misiones
| Edificio | Misión | Qué se hace |
|---|---|---|
| Física | `fis_experimentos` | doble rendija, Michelson, Millikan y Foucault |
| Química | `qui_sintesis` | ducha y EPI → campana → rotavapor → HPLC |
| Química | `qui_residuos` | bidones → ácidos → bases → inflamables |
| Biología | `bio_cultivo` | cabina → incubador → microscopio → biorreactor |
| Biología | `bio_pcr` | extracción → pre-PCR → termociclador → secuenciador |
| Central nuclear | `nuc_turno` | acceso → sala de control → conversión → intercambiador |

## Pendiente
- Las misiones aún no tienen interacción propia en los equipos (sus pasos se anclan a los bloques).
- Microcosmos e historia de Química, Biología y la central.
