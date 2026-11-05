# La maquinaria de las áreas de producción

Berón (2026-11-05): «recuerda que la maquinaria debe actualizarse para poder abastecer la producción requerida y esto es una regla de las áreas de producción para que se actualicen y creen correctamente».

**Regla P19 del manual.** Lo que un área produce sale de sus máquinas. Cada área tiene una línea en `ProductionAudit`: qué se le pide al día, qué da cada máquina de cada etapa. Se cuentan las máquinas que hay en la escena y la capacidad es la de la etapa más corta. Se comprueba al crear un área (un área con `AreaProducer` sin línea ni exención da aviso), al subir la demanda (censo real en Play), al final del Blockout (`[Producción]`) y en el grupo 103.

## Auditoría del 2026-11-05 (regla W11)

Los resultados vienen de leer el código con 11 humanos, y desde la ronda 9 el plan usa el personal real (32): **hay que volver a medir con el grupo 103**. Las capacidades por máquina son valores de diseño `[A]`, salvo la carne.

| Área | Pide al día | Da | Resultado |
|---|---|---|---|
| Carne Cultivada | 29,5 kg (27,5 de depredadores y 2 de cocina) | 2 kg antes; ahora 32 kg | **FALLABA; arreglada** |
| Cocina | 33 comidas | 10 por equipo | Por medir (cuenta `Equipo_` en la escena) |
| Huerto | 8,8 kg de verdura | ~3 kg/m² al año por superficie de bancales | Por medir |
| Estudio Textil | 0,18 prendas | 2 telares: 1 prenda | Bien |
| Mecánica | Servicio | Puestos | Exenta: la mide S13 |
| Hospital, Química, Biología, Planta Atómica | Investigación | — | Exentas |
| Energía | La punta de los pisos (`EnergyRule.DemandKW` × 1,6) × 24 h | 5 MWe × 24 h | Por medir (línea en `ProductionAudit`; el balance completo es `EnergyRule.Check`) |

Además, las cifras de `AreaProducer` (recursos por minuto de juego) son valores provisionales y no salen de las máquinas: esa parte de la regla sigue sin cumplirse. Convertirlas es un trabajo aparte.

## El laboratorio de carne

El lote de 16 kg que puse antes era un número sin máquinas: incumplía esta regla. Ahora:

| Máquina | Cuántas | Cuenta | Da |
|---|---|---|---|
| Biorreactor de producción, 500 L | 2 (antes 1) | 10⁷ células/mL ÷ 2,5·10⁸ células/g = 40 g/L | 40 kg |
| Perfusión con andamio | 4 (antes 1) | 8 kg por unidad `[A]` | **32 kg, el cuello de botella** |
| Centrífuga | 1 | 40 kg al día `[A]` | 40 kg |
| Formadora | 1 | 40 kg al día `[A]` | 40 kg |

`batchGrams` se calcula de esas máquinas al construir. Un lote da 64 raciones y el almacén guarda 80. Con esto `LabShare` llega al 100 % en el plan, y los depredadores no necesitan cazar **si** la ronda llega a todos. La densidad de 10⁷ células/mL es una cifra de referencia, sin fuente directa.
