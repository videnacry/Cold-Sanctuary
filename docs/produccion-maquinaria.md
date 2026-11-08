# La maquinaria de las áreas de producción

Berón (2026-11-05): «recuerda que la maquinaria debe actualizarse para poder abastecer la producción requerida y esto es una regla de las áreas de producción para que se actualicen y creen correctamente».

**Regla P19 del manual.** Lo que un área produce sale de sus máquinas. Cada área tiene una línea en `ProductionAudit`: qué se le pide al día, qué da cada máquina de cada etapa. Se cuentan las máquinas que hay en la escena y la capacidad es la de la etapa más corta. Se comprueba al crear un área (un área con `AreaProducer` sin línea ni exención da aviso), al subir la demanda (censo real en Play), al final del Blockout (`[Producción]`) y en el grupo 103.

## Auditoría del 2026-11-05 (regla W11)

Los resultados vienen de leer el código con 11 humanos, y desde la ronda 9 el plan usa el personal real (32): **hay que volver a medir con el grupo 103**. Las capacidades por máquina son valores de diseño `[A]`, salvo la carne.

| Área | Pide al día | Da | Resultado |
|---|---|---|---|
| Carne Cultivada | 48–49 kg: el techo de depredadores (5 osos, 5 lobos y hasta 6 zorros) y 2 de cocina | 2 kg antes; ahora 60 kg | **FALLABA; arreglada** |
| Cocina | 33 comidas | 10 por equipo | Por medir (cuenta `Equipo_` en la escena) |
| Huerto | 24,8 kg de verdura (31 personas) | ~3 kg/m² al año por superficie de bancales: 14,2 kg con 1728 m² | **Faltaba**: hacían falta 3017 m², 1290 más; **resuelto el 2026-11-08** con el Huerto vertical (ver abajo) |
| Estudio Textil | 0,18 prendas | 2 telares: 1 prenda | Bien |
| Mecánica | Servicio | Puestos | Exenta: la mide S13 |
| Hospital, Química, Biología, Planta Atómica | Investigación | — | Exentas |
| Energía | La punta de los pisos (`EnergyRule.DemandKW` × 1,6) × 24 h | 5 MWe × 24 h | Por medir (línea en `ProductionAudit`; el balance completo es `EnergyRule.Check`) |

Además, las cifras de `AreaProducer` (recursos por minuto de juego) son valores provisionales y no salen de las máquinas: esa parte de la regla sigue sin cumplirse. Convertirlas es un trabajo aparte.

## El laboratorio de carne

Primero puse un lote de 16 kg sin máquinas, que incumplía esta regla. La línea pide lo que pueden llegar a ser los depredadores (techo de reproducción), y no los de hoy: si el censo real lo pasa, manda el censo.

| Máquina | Cuántas | Cuenta | Da |
|---|---|---|---|
| Biorreactor de producción, 500 L | 3 (antes 1) | 10⁷ células/mL ÷ 2,5·10⁸ células/g = 40 g/L | **60 kg, el cuello de botella** |
| Perfusión con andamio, dos alturas | 6 (antes 1) | 12 kg por unidad `[A]` | 72 kg |
| Centrífuga | 2 (antes 1) | 40 kg al día `[A]` | 80 kg |
| Formadora | 2 (antes 1) | 40 kg al día `[A]` | 80 kg |

`batchGrams` se calcula de esas máquinas. Un lote da 120 raciones y el almacén guarda 160. La densidad de 10⁷ células/mL es una cifra de referencia, sin fuente directa.

**Quién cuenta (ronda 9):** las líneas de Cocina, Huerto y Textil usan las personas reales (`Humans`, ~32): 96 comidas, 25,6 kg y 0,53 prendas. Los depredadores usan `FaunaScale` (11): ver [hielo-y-respiraderos.md](hielo-y-respiraderos.md). El Huerto queda muy justo: 8 pisos de 396 m² dan ~26 kg al día, sin contar los bancales de las puntas.

## 2026-11-06: el Huerto para 31 personas

A 3 kg/m² al año, 24,8 kg/día piden **3017 m²**; los 8 bancales del Árbol de los Campos dan 1728 (14,2 kg). Faltan **1290 m²**. No cabe en lo que hay: o dos niveles más del Árbol (sus 8 plataformas dan 216 m² de bancal de media: un nivel de 4 plataformas sumaría unos 860 m², y no basta), o un piso de estantes con LED. La decisión es de Berón.

## 2026-11-08: el Huerto vertical (Berón lo aprobó)

**Dónde.** Un piso nuevo de la **torre de Producción** (la de la isla, en la tundra, −180, 0, −140), encima del Estudio Textil: `VerticalFarm_Area`, «Huerto vertical» (`SampleSceneBuilder.BuildVerticalFarm`, medidas en `VerticalFarm`). La torre pasa de 2 a **3 niveles** (sus pisos: Construcción, Mecánica, Estudio Textil, Huerto vertical y el garaje del submarino en el primer piso libre; queda otro libre). Los niveles salen de los pisos que haga falta: `⌈(3 + pisos del huerto + garaje) / 2⌉`. El nivel 3 tiene la misma planta que el 1, 44 m más arriba: no se acerca en planta a ningún vecino.

**Cuánto: sale de la demanda (regla P19).**
- Demanda: `PopulationPlan.People` × 0,8 kg = **32 × 0,8 = 25,6 kg/día**. `People` cuenta la lista del personal (30) + la Magnate + el recién llegado, también al construir (antes la línea usaba `Humans`, que al construir vale 11).
- Bancales del Árbol: 1728 m² × 3 kg/m²/año = **14,2 kg/día** (los mide la escena: `VerticalFarm.FieldsKgDay`).
- Estantes: `VerticalFarm.RacksFor(demanda, bancales)` = ⌈(25,6 × 1,1 − 14,2) / 1,91⌉ = **8 estantes** (la reserva del 10 % es un supuesto de diseño: una bandeja perdida no deja a nadie sin verdura).
- Cada estante: NFT de **4 bandejas de 3,0 × 1,2 m = 14,4 m²** de cultivo, la de arriba a 1,9 m (la alcanza la persona más alta sin subirse a nada; sin escaleras). Da 14,4 × 48,3 / 365 = **1,91 kg/día**.
- Total: 8 × 1,91 = **15,2 kg/día** de los estantes + 14,2 de los bancales = **29,4 kg/día** para 25,6 que se piden.
- Los 1290 m² de bancal que faltaban (31 personas; con 32, 1387) se cubren con **115 m² de bandeja**: una granja vertical rinde ~16 veces más por m² de bandeja que un bancal al aire libre a −78°.
- Si la demanda sube y no caben en un piso (`RacksPerFloor` = 12), el constructor pone otro piso y la torre sube los niveles que haga falta.

**Rendimiento y energía (fuente):**
- **Pennisi, G. et al. (2025), «Vertical farming…», *Agronomy for Sustainable Development* 45:57** (doi 10.1007/s13593-025-01055-w): lechuga en granja vertical, literatura científica **media 59,7 ± 5,8 kg de peso fresco/m²/año, mediana 48,3 (n = 54)**; lo que declaran las empresas, 105 de media. Se usa la **mediana, 48,3** (`VerticalFarm.YieldKgM2Year`): prudente.
- La misma revisión da **0,08–0,13 kg por kWh** (7,7–12,5 kWh/kg). Diseño: **10 kWh/kg** (`VerticalFarm.KWhPerKg`), luz, clima y bombas juntos. 8 estantes: 15,2 kg × 10 / 24 = **6,4 kW de media**, más 5 W/m² del piso (pasillos, vivero, lavado): ~2,6 kW. **~9 kW en total.**
- Otras cifras para comparar (no usadas): 10–18 kWh/kg (benchmark de 2025) y 18 kWh/kg de mediana (revisión de Heuvelink y otros, sin revisar por pares).
- **Conocimiento general, sin ficha consultada:** EC 1,2–1,8 mS/cm y pH 5,5–6,5 para la solución de lechuga en NFT; los tacos de lana de roca del semillero.

**El piso** (reglas de los pisos: muebles detallados y con herramientas, 2 m de paso, necesidades básicas, sin muros ni techos propios):

| Bloque | Qué lleva | Puestos |
|---|---|---|
| Estantería LED × 8 | 4 montantes, 4 bandejas NFT con sus lechugas, una barra LED sobre cada una, tubería de riego, canal de retorno y su driver | 1 cada una («cultivo vertical») |
| Vivero de germinación | Mesa con 4 bandejas de semillero, 2 cúpulas de humedad, barra LED, sobres de semillas, tacos de lana de roca | 2 («siembra») |
| Dosificador de solución nutritiva | Depósitos A y B, depósito de retorno, controlador de EC y pH con pantalla, bomba dosificadora, filtro y UV | 2 |
| Lavado y embolsado | Pila con grifo, mesa de embolsado, báscula, centrifugadora de ensalada, bolsas compostables | 2 («cosecha») |
| Cámara de verdura | Cámara a 4 °C con su puerta, termómetro y cajas de cosecha | — |
| Banco de herramientas | Cuchillo de cosecha, tijeras de poda, medidor de EC y pH, cepillo de canales, cubo, panel de herramientas, carro de bandejas | 2 |
| Compostador de raíces | Las raíces y hojas viejas, para abono de los bancales del Árbol | — |
| Descanso y fuente de agua | Banco y fuente | — |
| Aseos | Una cabina y la accesible (como los demás pisos) | — |

**Las tareas diarias** (catálogo de la jornada, `AreaFunctions`; cada una ≤ 1 h; pasos en `PlayerTasks`):
- `huv_siembra` (1 h): llenar bandejas, sembrar, trasplantar las plántulas a los canales;
- `huv_cosecha` (1 h, **obligatoria**, también va a la Cocina): cortar, lavar, embolsar y llevar a la cámara de la cocina;
- `huv_solucion` (0,5 h): medir EC y pH, dosificar, limpiar canales y filtros.
Uniforme del Huerto. El catálogo pasa de 77 a 80 misiones (89,5 h, 21 obligatorias; grupo 61).

**Prueba (grupo 103):** el Huerto pide 25,6 kg/día (32 × 0,8) y lo da; los estantes que hay ≥ los que pide la demanda; el piso es de la torre de Producción, con vivero, lavado, cámara, dosificador, herramientas y aseos; declara su energía; sus tareas están en el catálogo y ninguna pasa de 1 h.

**Pendiente:** el Huerto vertical no tiene aún mezcla de culturas (`AreaDesign`: Falta) ni microcosmos propio (la alerta `[Pisos]` lo dirá). Las cifras de `AreaProducer` siguen siendo provisionales (ver arriba).
