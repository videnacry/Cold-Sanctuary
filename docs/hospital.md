# El Hospital del santuario (antes Enfermería)

Pedido de Berón (2026-10-22): la enfermería pasa a **hospital**, porque no hay otro a miles de km. Debe poder tratar
**todos los casos**, tener equipo para **8 médicos a la vez** (v2, 2026-10-23: **4 puestos por servicio**), **quirófano**, cuidar al **mismo número de personas que de
animales**, y crecer **a dos plantas**. Las camas de la plataforma hija suben a **4 m** (a 2,2 m Kushal chocaba; 2026-10-23, +1 m) y **sin
barandillas**.

Código: `Assets/Editor/SampleSceneBuilder.Hospital.cs` (`BuildHospital`), muebles en `InfirmaryProps`, puestos
interactivos en `HospitalStation` (`Assets/Scripts/World/`). El área sigue siendo `Infirmary_Area` /
`SanctuaryAreaType.Infirmary`; su nombre visible y clave de jornada es `AreaWork.Hospital` = `"Hospital"`.

## 1. Qué necesita un hospital aislado (investigación)

| Necesidad | Por qué (fuente) | En el juego |
|---|---|---|
| **Quirófano** capaz de cesárea, laparotomía y fractura abierta | Los 3 procedimientos *bellwether*: quien los hace, puede con casi toda la cirugía urgente (Meara et al., *Lancet Commission on Global Surgery*, 2015, doi:10.1016/S0140-6736(15)60160-X) | Mesa quirúrgica, lámpara, instrumental (`Quirofano`, puesto de cirujano) |
| **Anestesia** con oxígeno y pulsioximetría | *WHO Guidelines for Safe Surgery* / Lista de verificación de cirugía segura (OMS, 2009) | Máquina de anestesia (puesto de anestesista). **No se opera sin alguien en la anestesia** |
| **Esterilización** y lavado quirúrgico | OMS, *Surgical Care at the District Hospital* (2003): autoclave 121 °C/15 min o 134 °C/3 min | Autoclave, lavabo quirúrgico |
| **Urgencias**: triaje y reanimación (ABC, desfibrilador) | OMS, *Emergency and Essential Surgical Care* (EESC) / lista genérica de equipo de urgencias | Recepción y triaje, carro de parada, 3 camas de observación |
| **UCI** (ventilación, monitorización) | Hospital de referencia sin traslado posible | 3 camas con ventilador + control de UCI |
| **Paritorio** con cuna térmica | Parto y prevención de hipotermia neonatal (OMS, cuidados esenciales del recién nacido) | Cama de partos con cuna térmica radiante |
| **Aislamiento** | Precauciones de contacto/gotas; antesala con EPI | Box de cristal con antesala |
| **Imagen** | Rayos X (huesos, tórax) y ecografía (abdomen, embarazo, líquido libre) | Rayos X, ecógrafo |
| **Laboratorio + banco de sangre** | Hemograma, glucosa, grupo; sangre a 2-6 °C; lejos de todo, *banco de sangre andante* (donantes del propio santuario) | Banco de laboratorio, analizador clínico (iniciación), nevera de sangre |
| **Farmacia** | Lista de Medicamentos Esenciales de la OMS | Farmacia con mostrador |
| **Oxígeno propio** | Concentrador PSA (la zeolita retiene N₂ → ~93 % O₂): no depende de bombonas que lleguen de fuera | Planta de oxígeno |
| **Telemedicina** | Como en las bases antárticas: un especialista lejano guía al médico in situ (p. ej. la cirugía de rodilla guiada a la estación del Polo Sur) | Puesto de telemedicina |
| **Una sola salud** | Mismo hospital para personas y animales (docs/area-progression.md) | Planta alta veterinaria |

Para el santuario de hielo se suma la **hipotermia/congelación** (calentamiento activo: mantas térmicas, sueros calientes)
y el **trauma por animales grandes** (osos): por eso urgencias + quirófano + banco de sangre son el núcleo.

Fuentes extra: Princess Elisabeth Antarctica, nuevas instalaciones médicas (polarfoundation.org); *Telemedicine link
with South Pole allows remote knee surgery* (hospimedica.com).

## 2. Distribución v2 (32 × 26 m, entrada por +Z; detalle y fuentes en `docs/hospital-plan.md`)

Berón (2026-10-23) pidió **4 puestos iguales por servicio** (4 médicos pueden hacer la misma tarea a la vez), **2 m
libres entre objetos** y un **anillo de camas por planta**, para que cada paciente se recupere en el piso de su servicio.
La galería en U de v1 (Cama 4-33) **ya no existe**: la sustituyen los anillos.

Cada **anillo** (`RingDeck`, `SampleSceneBuilder.Levels.cs`) es una plataforma hija cuadrada a 4 m sobre su planta, de
10 m de ancho: el 20 % sobre la planta y el 80 % volando fuera. Camas atravesadas a 3,1 m de eje (2 m libres), dos por
esquina, y el hueco de sus dos escaleras inclinadas en el centro de los lados ±Z. Sin barandillas visibles: barrera
invisible en los bordes. `SanctuaryArea` cuenta el anillo como parte del área solo a su altura (`RingReach`), y no lo usa
para decidir qué áreas forman un edificio.

- **Planta baja: urgencias y ambulatorio.** 4 triajes, 4 consultas (la 1 = mesa del médico, junto a la silla de exámenes y
  el analizador de la **iniciación**, intactos), el **lavabotas** de biocontrol de la iniciación (entre los triajes 1 y 2;
  `docs/iniciacion-medica-investigacion.md`), 2 carros de parada, 2 rayos X + 2 ecógrafos, laboratorio, banco de sangre,
  farmacia, espera (10), aseos, telemedicina y 4 paritorios de 32 m². **Anillo bajo:** 6 camas de observación de urgencias
  (Cama 1-6) y 12 de maternidad.
- **Planta 1 (11 m): bloque quirúrgico y críticos.** 4 quirófanos de 6 × 6 m con su anestesia, 2 lavados quirúrgicos,
  autoclave, planta de oxígeno, UCI de 6 boxes de 18,6 m² con ventilador y su control, y 4 aislamientos con esclusa en una
  subplataforma de 1 m. **Anillo alto:** 10 camas de recuperación postquirúrgica y 20 de hospitalización.
- **Planta 2: hospital veterinario** (`docs/vet-hospital-plan.md`). 4 mesas de tratamiento (una con báscula) y 4 quirófanos
  con su anestesia, rayos X blindado, ecógrafo, laboratorio, farmacia, cocina de dietas con congelador, pediluvio,
  16 perreras en batería, 8 jaulas y sala fría de pingüinos con 12 jaulas. **Anillo de grandes animales:** 6 boxes de oso de
  4 × 3 m, 4 de reno, ciervo o lobo, jaula de contención y 2 corrales de foca con piscina salada.

**Personas: 48 camas** (más 6 de UCI). **Animales: 48 plazas.**

## 3. Puestos médicos y los interactuables

`HospitalStation` (F) actúa sobre el paciente más cercano: urgencias estabiliza (−enfermedad, −estrés), UCI y farmacia
curan, el quirófano cura del todo **si hay alguien en la máquina de anestesia**, imagen/laboratorio diagnostican, la
veterinaria atiende solo animales. Cada uno muestra un dato de ciencia (`HospitalStation.Science`).

Puestos médicos (`doctorPost`), 4 por servicio: triaje, consulta, quirófano, anestesia, tratamiento veterinario y cirugía
veterinaria, más los 2 carros de parada y el control de la UCI. `WorkdayDirector.WorkPoint` coloca a cada médico con turno
en el hospital en un puesto libre (`HospitalStation.PostFor`) y lo libera al cambiar de área. Misiones en `AreaWork`:
`hos_urgencias` (obligatoria), `hos_quirofano`, `hos_uci`, `hos_veterinaria`, con sus pasos y anclas en `VirtualMissions`.

## 4. Pendiente

- Los animales grandes (osos) suben a la planta 2 por escaleras: falta un montacargas o que los lleve la alfombra voladora.
- Del plan veterinario falta: UCI veterinaria, pediluvio en cada ala, el edificio aparte de **cuarentena y necropsia** y la
  **piscina médica de cetáceos** en la laguna de las orcas.
- Laboratorio humano: el plan pide 4 puestos y hay uno (más el analizador y el banco de sangre).
- El puente del anillo bajo a la **base de alfombras** llega con el restaurante (`docs/restaurant-plan.md`).
- Morgue y lavandería no están modeladas.
- La cirugía no toca aún `BodyInjuries` (las lesiones de historia, p. ej. Shanidar, se quedan como están).
