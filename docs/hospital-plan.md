# Hospital: programa de espacios real y redistribución (v2: anillos de camas y capacidad ×4)

> Berón, 2026-10-23.
> - v1: «2 m entre objetos para poder caminar», «el aislamiento con subplataforma», «investiga la distribución real y verifica cuánto espacio tenemos».
> - v2: «reemplaza la planta de camas por el anillo… al menos 2 médicos (mejor 4) deben poder hacer todas las tareas a la vez… quizás dos pisos y un anillo de camas en cada piso, porque hay categorías de camas y cada paciente se recupera en el piso de su servicio… en el hospital de animales también anillo si hace falta (osos), y 4 veterinarios trabajando a la vez en lo mismo».
>
> Estado: v2 en construcción.

## 1. Superficies de referencia (normas de diseño hospitalario)

| Espacio | Mínimo real | Fuente |
|---|---|---|
| Paritorio (LDR/LDRP), individual | 31,6 m² libres, lado ≥ 3,96 m, más 3,7 m² para reanimar al recién nacido | FGI vía PHC Pros; DHA (Dubái) |
| UCI, por cama | 18,6 m² libres | resumen de FGI 2022 |
| Recuperación (URPA), por cama | 7,4 m²; 1,5 m entre camas | FGI 2014 (fuente secundaria) |
| Aislamiento (AII) | habitación individual con antesala-esclusa, presión negativa ≥ 2,5 Pa y ≥ 12 renovaciones por hora | ASHRAE 170; IHFG; Minnesota DOH |
| Quirófano | ~37 m² [? norma local] | — |
| Rayos X / eco | ~20–25 m² blindada / ~12 m² [? orientativo] | — |

## 2. Capacidad: 4 médicos en la misma tarea a la vez

Ante una avalancha de pacientes con el mismo problema, cada servicio tiene **4 puestos** iguales. El hospital humano tiene 4 médicos; los turnos y la Magnate ya los reparten.

| Servicio | Puestos | m² aprox. |
|---|---|---|
| Triaje / urgencias | 4 boxes + 2 carros de parada | 40 |
| Consultas (una con la silla de iniciación) | 4 | 50 |
| Imagen | 2 salas de rayos X + 2 de eco | 75 |
| Laboratorio (con analizador y banco de sangre) | 4 puestos | 35 |
| Paritorio | 4 salas de 32 m² | 128 |
| Quirófano | 4 de 6 × 6 m + lavado y esterilización | 185 |
| UCI | 6 camas de 18,6 m² + control | 120 |
| Aislamiento | 4 habitaciones con esclusa | 90 |
| Farmacia, espera, aseos, telemedicina | — | 75 |
| **Total** | | **≈ 800 m² + 25 % de circulación ≈ 1000 m²** |

## 3. Distribución v2 (dos plantas con su anillo; el veterinario arriba)

Cada **anillo** es una plataforma hija a 4 m sobre su planta, cuadrada y completa. Mide unos 10 m de ancho: la fila de camas en el centro y 4 m libres a cada lado. Está el 20 % dentro del borde y el 80 % volando fuera. Su perímetro da para unas 45 camas a 3,1 m de eje.

1. **Planta baja (832 m²): urgencias y ambulatorio.**
   - 4 boxes de triaje, 4 consultas, 2 rayos X y 2 ecos, laboratorio, farmacia y espera.
   - 4 paritorios.
   - **Anillo bajo:** observación de urgencias y **maternidad**, para que la madre se recupere en el piso de su paritorio.
2. **Planta 1 (736 m²): bloque quirúrgico y críticos.**
   - 4 quirófanos con lavado y esterilización.
   - UCI de 6 camas.
   - 4 aislamientos, cada uno en su subplataforma con esclusa.
   - **Anillo alto:** **recuperación postquirúrgica** y **hospitalización general**, en el piso de los quirófanos.
3. **Planta 2: hospital veterinario** (ver `docs/vet-hospital-plan.md`), con su **anillo** para grandes animales: los osos de 4 × 3 m.

Las escaleras de cada anillo bajan a su planta. El anillo bajo enlaza además, por un puente, con la **base de alfombras voladoras**, que es nueva.

Fuentes:
- PHC Pros, «LDRP basics»
- DHA, Obstetric and Neonatal checklist
- FGI, glosarios 2014/2022 y resumen de UCI 2022
- ASHRAE 170, adenda de 2008
- Minnesota DOH, «negative pressure isolation rooms»
- International Health Facility Guidelines, Isolation Rooms
