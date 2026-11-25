---
resumen: "PB-170 (Berón 2026-11-24): qué cambió en el santuario 1 de CodeShip entre el 20 y el 24 de noviembre (listas 6.ª a 16.ª, #64–#134) y qué se puede portar a Cold Sanctuary, por prioridad, respetando la regla de oro y el manual."
temas: [codeship, retos, microcosmos, construccion]
estado: vigente
codigo: [CodeChallengeSheet, VirtualizationTank]
---
# Portar a Cold Sanctuary los cambios del santuario 1 de CodeShip (PB-170)

Petición de Berón (2026-11-24): «en CodeShip se han realizado importantes cambios sobre su sanctuary1, quizás se
podrían aplicar aquí».

**Origen revisado:** `Catravcul/CodeShip`, rama `integracion`, de `f4ddd17` (2026-11-20, lo último ya portado en
PB-169) a `63a5b86` (2026-11-24): 42 commits, PR #11 a #24. Su lista de pedidos está en `docs/design/santuario1.md`
(listas 6.ª a 16.ª, #64–#134), los microcosmos en `docs/design/microcosmos.md` y el relevo en `docs/relevo.md`.

## Qué se porta, por prioridad

| # | Cambio en CodeShip | Dónde (CodeShip) | En Cold Sanctuary | Unity |
|---|---|---|---|---|
| 1 | **Cada objeto con su reto propio** (≈290 objetos con fuente: banda, dojo, gimnasio, hospital, cocina, cría, carne cultivada, laboratorios, obra, taller, textil, huertos…) y **una animación por paso** que se repite hasta el siguiente | `data/retos-objetos*.ts`, `retos.ts`, `animaciones.ts` | Cubre PB-163 (retos por actividad). Exportar a JSON (como `codeship/musk-retos.json`) y abrirlos desde el mueble con `CodeChallengeSheet`; las animaciones, como posturas de cubos | No para escribirlo; sí para probarlo |
| 2 | **Diálogos como retos** («Hablar con…», ordenar la conversación) | `docs/design/microcosmos.md` (Academia) | Cubre PB-166 en el resto de microcosmos | Igual |
| 3 | **Retos de equipo en la máquina de simulación**: otros personajes en sus papeles y **recompensa para todos** los que participan; una gran pecera para muchos | `data/simulacion.ts` | Encaja con la regla de oro (todos hacen todo, por rotación). Se monta sobre `VirtualizationTank`, sin sistema paralelo | Sí |
| 4 | **Ropa según el horario** (pijama y gorro en el hospital, bata en laboratorios, casco en obra…) y la del microcosmos | `domain/ropa.ts` | Se engancha a la jornada (W9): la ropa sigue a la tarea de la rotación, no a un puesto fijo | Sí |
| 5 | **Revisión de realismo con puntaje** (fuente, forma, color, apoyo; 80/100 y nada en el aire) como regla de creación y prueba | `data/realismo.ts`, `reglas.md` §1.7 | Regla nueva del manual + comprobación sin Unity sobre el volcado de `VisorExport` | No |
| 6 | **Pantalla de carga**, **día y noche** marcados (luna, estrellas, sol 6–20 h) | `ui/loading-screen.ts`, `adapters/three/day-night.ts` | Pequeños y sin choque | Sí |
| 7 | **Tirarse de pisos y anillos** sin barreras, toboganes reales | `caidas.test.ts`, `domain/tobogan.ts` | Ya rige «sin barandillas»; comprobar en vivo que se cae | Sí |

## Qué necesita la decisión de Berón (rediseño del mesocosmos, W2 y W9)

Cambian pisos y edificios-árbol, así que cada uno obliga a reajustar jornada, reparto y horario libre:
pisos de 40 × 40 m más pequeños para los miembros actuales (#64, #82), un piso por actividad y pisos por afinidad
(#65, #98), los pisos solo nacen del tronco con tornapuntas diagonales y rejilla de madera (#66–#70, #76), cada árbol
sobre su base negra en un anillo exterior con un mar central (#71–#74, #83), el globo de agua con el pulpo que se
desarrolla (#75, #79, #86–#87, #108, #119, #123), la cima gigante con residencia, plaza y castillos con toboganes
(#130, #132) y las reglas de reorganización (#134). Más el microcosmos nuevo de la **Academia de Platón** (#115).

**Plan (W2, 2026-11-25):** [rediseno-pisos-arboles.md](../areas/rediseno-pisos-arboles.md): choques con J16/J36 y T1, árboles propuestos, reajuste W9, fases F0–F6 y decisiones D1–D6.

## Qué no se porta

- «Distribución nuestra» de CodeShip: el puesto fijo por turno choca con la regla de oro (aquí manda `WorkRoster`
  por rotación).
- Los parámetros propios de su web (4 animales por persona por defecto; aquí el multiplicador es 3).

## Nota (V8 de PENDIENTES)

El documento de CodeShip sobre subagentes y ahorro de tokens es `docs/relevo.md` (fusionado el 2026-11-21, PR #12).
