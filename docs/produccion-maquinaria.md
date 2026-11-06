# La maquinaria de las áreas de producción

Berón (2026-11-05): «recuerda que la maquinaria debe actualizarse para poder abastecer la producción requerida y esto es una regla de las áreas de producción para que se actualicen y creen correctamente».

**Regla P19 del manual.** Lo que un área produce sale de sus máquinas. Cada área tiene una línea en `ProductionAudit`: qué se le pide al día, qué da cada máquina de cada etapa. Se cuentan las máquinas que hay en la escena y la capacidad es la de la etapa más corta. Se comprueba al crear un área (un área con `AreaProducer` sin línea ni exención da aviso), al subir la demanda (censo real en Play), al final del Blockout (`[Producción]`) y en el grupo 103.

## Auditoría del 2026-11-05 (regla W11)

Los resultados vienen de leer el código con 11 humanos, y desde la ronda 9 el plan usa el personal real (32): **hay que volver a medir con el grupo 103**. Las capacidades por máquina son valores de diseño `[A]`, salvo la carne.

| Área | Pide al día | Da | Resultado |
|---|---|---|---|
| Carne Cultivada | 48–49 kg: el techo de depredadores (5 osos, 5 lobos y hasta 6 zorros) y 2 de cocina | 2 kg antes; ahora 60 kg | **FALLABA; arreglada** |
| Cocina | 33 comidas | 10 por equipo | Por medir (cuenta `Equipo_` en la escena) |
| Huerto | 24,8 kg de verdura (31 personas) | ~3 kg/m² al año por superficie de bancales: 14,2 kg con 1728 m² | **Falta**: hacen falta 3017 m², 1290 más (ver abajo) |
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
