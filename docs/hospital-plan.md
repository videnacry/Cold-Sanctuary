# Hospital: programa de espacios real y redistribución propuesta

> Berón, 2026-10-23: «2 m entre objetos para poder caminar», «no se puede pasar entre la camilla de ecografía y el
> paritorio», «no se puede entrar al aislamiento», «investiga la distribución real y verifica cuánto espacio tenemos».
> Estado: **propuesta para aprobar** (no aplicada todavía).

## 1. Lo que hay hoy (`SampleSceneBuilder.Hospital.cs`)

| Planta | Superficie | Contenido |
|---|---|---|
| Baja (32 × 26 m) | 832 m² | triaje, aseos, aislamiento, 3 camas de observación, carro de parada, silla de exámenes, 2 consultas con camilla, telemedicina, UCI (3 camas + control), sala de espera (10 sillas), farmacia, rayos X, ecógrafo + camilla, paritorio, laboratorio, analizador, banco de sangre, quirófano (mesa, lámpara, anestesia, instrumental, lavado, autoclave, oxígeno); pasillo central de 3 m |
| Galerías en U (a 4 m) | ≈ 260 m² (franjas de 4,4 m) | 15 camas encima y 15 debajo |
| Alta (32 × 23 m) | ≈ 736 m² | hospital veterinario: 6 cuadras, 16 boxes… (30 plazas) |

Problemas medidos o señalados:
- La camilla de eco y el paritorio están a ~1,6 m de eje a eje, con unos 0,4 m de paso entre ellas.
- El aislamiento no tiene esclusa de entrada (antesala) y no se puede entrar.
- El paritorio, el quirófano y la UCI son muebles sueltos en una nave abierta. En la realidad son salas cerradas con una superficie mínima.

## 2. Superficies de referencia (normas de diseño hospitalario)

| Espacio | Mínimo real | Fuente |
|---|---|---|
| Paritorio (LDR/LDRP), individual | 31,6 m² libres, lado mínimo 3,96 m, más 3,7 m² para reanimar al recién nacido | FGI vía PHC Pros; DHA (Dubái) |
| UCI, por cama (habitación individual) | 18,6 m² libres (200 sq ft) | resumen de FGI 2022 |
| Recuperación (URPA), por cama | 7,4 m²; 1,5 m entre camas y 1,2 m a la pared | FGI 2014 (fuente secundaria) |
| Aislamiento de infecciosos (AII) | habitación individual con antesala-esclusa de puertas enclavadas, presión negativa ≥ 2,5 Pa (0,01 in c.a.) y ≥ 12 renovaciones por hora | ASHRAE 170; IHFG; Minnesota DOH |
| Quirófano | ~37–56 m² según el tipo [? confirmar con la norma local] | — |
| Rayos X / ecografía | sala blindada ~20–25 m² / ~12 m² [? orientativo] | — |

## 3. Programa con la regla de Berón (2 m libres entre objetos)

- **Cama de sala:** mide 1,1 × 2,3 m. Con 2 m a cada lado salen 3,1 m de eje a eje, y con 2 m de paso a los pies son unos 13 m² por cama. Las 30 camas necesitan unos 400 m².
- **UCI:** 3 camas × 18,6 m², unos 56 m².
- **Aislamiento:** habitación de unos 15 m², antesala de 4 m² y aseo, en total unos 22 m².
- **Paritorio:** 32 m². **Quirófano** con lavado y esterilización: unos 47 m².
- **Imagen** (rayos X y eco): unos 37 m². **Laboratorio y banco de sangre:** unos 20 m².
- **Resto:**
  - farmacia: 12 m²;
  - 2 consultas: 24 m²;
  - triaje y recepción: 10 m²;
  - espera: 20 m²;
  - aseos: 6 m²;
  - telemedicina: 10 m²;
  - iniciación: 10 m².
- **Total:** unos 706 m², más un 30 % de circulación, ≈ **920 m²** para el hospital humano.

Hoy el hospital humano tiene 832 m² en planta baja, más unos 260 m² en galerías estrechas de 4,4 m. Ahí no caben camas con 2 m a ambos lados y paso a los pies. **No cabe bien en una sola planta.**

## 4. Propuesta

1. **Planta baja: ambulatorio y servicios centrales.**
   - En la entrada: triaje, espera, aseos y farmacia.
   - Consultas e iniciación, con la silla de exámenes.
   - Imagen: rayos X y eco en salas separadas.
   - Laboratorio y banco de sangre.
   - Bloque quirúrgico: quirófano, lavado y esterilización.
   - Paritorio como sala propia de 32 m².
   - Todo con 2 m libres y el pasillo central de 3 m.
2. **Planta 1, nueva: hospitalización.**
   - Las 30 camas en salas a 3,1 m de eje y la UCI de 3 camas con su control.
   - El **aislamiento en una subplataforma propia**, con antesala-esclusa y una sola entrada.
   - Desaparecen las galerías en U: la idea de Berón de tener camas en dos niveles se cumple con esta planta completa.
3. **Planta 2:** el hospital veterinario, como hoy.
4. **Regla de 2 m en todo el santuario:** `FurnitureSpacing` pasa de 0,9 m a 2 m y cubre todos los muebles de cada área, no solo camas, mesas y máquinas. Las áreas que no quepan se verifican una a una con el mismo método.

Fuentes:
- PHC Pros, «LDRP basics» (phcppros.com/articles/8265-ldrp-basics)
- DHA, Obstetric and Neonatal checklist
- FGI Guidelines, glosarios 2014/2022 y resumen de UCI 2022 (mcdmag.com)
- ASHRAE 170, adenda de 2008
- Minnesota DOH, «negative pressure isolation rooms»
- International Health Facility Guidelines, Part D Isolation Rooms (DHA)
