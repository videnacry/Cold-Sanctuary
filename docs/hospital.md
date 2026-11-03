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
- **Planta 1: bloque quirúrgico y críticos** (redistribuida el 2026-11-03 para las paredes de cristal, en el piso de 40 × 40 m del
  árbol; §6):
  - al sur, **4 quirófanos en fila**, cada uno en su **sala de cristal** con su conjunto (mesa, lámpara, anestesia e instrumental) y la
    puerta al pasillo del norte;
  - en ese pasillo, 2 lavados quirúrgicos entre las puertas, el autoclave y la planta de oxígeno;
  - al noreste, la **UCI** de 6 camas (18,6 m² por cama, con ventilador) en su sala de cristal; el control, fuera, la ve a través;
  - al noroeste, **4 aislamientos de cristal** con presión negativa y su **esclusa** de cristal delante; el armario de EPI, fuera.

  **Anillo alto:** 10 camas de recuperación postquirúrgica y 20 de hospitalización.
- **Planta 2: hospital veterinario** (`docs/vet-hospital-plan.md`):
  - 4 mesas de tratamiento (una con báscula);
  - **4 quirófanos de cristal** en un bloque de 2 × 2;
  - la **sala fría de pingüinos** (0–10 °C, 12 jaulas) y el **aislamiento veterinario** (cuarentena: 2 boxes), los dos de cristal;
  - rayos X blindado, ecógrafo, laboratorio, farmacia y cocina de dietas con congelador;
  - pediluvio, 16 perreras en batería y 8 jaulas.

  **Anillo de grandes animales:** 6 boxes de oso de 4 × 3 m, 4 de reno, ciervo o lobo, jaula de contención y 2 corrales de foca con
  piscina salada.

**Personas: 48 camas** (más 6 de UCI). **Animales: 50 plazas** (48 + los 2 boxes del aislamiento veterinario).

## 3. Puestos médicos y los interactuables

`HospitalStation` (F) actúa sobre el paciente más cercano: urgencias estabiliza (−enfermedad, −estrés), UCI y farmacia
curan, el quirófano cura del todo **si hay alguien en la máquina de anestesia**, imagen/laboratorio diagnostican, la
veterinaria atiende solo animales. Cada uno muestra un dato de ciencia (`HospitalStation.Science`).

Puestos médicos (`doctorPost`), 4 por servicio: triaje, consulta, quirófano, anestesia, tratamiento veterinario y cirugía
veterinaria, más los 2 carros de parada y el control de la UCI. `WorkdayDirector.WorkPoint` coloca a cada médico con turno
en el hospital en un puesto libre (`HospitalStation.PostFor`) y lo libera al cambiar de área. Misiones en `AreaWork`:
`hos_urgencias` (obligatoria), `hos_quirofano`, `hos_uci`, `hos_veterinaria`, con sus pasos y anclas en `VirtualMissions`.


## 4. Banco de sangre (Berón 2026-11-03)

«En el hospital debe existir un banco de sangre.» Código: `Assets/Scripts/World/BloodBank.cs` (`BloodBank`, `BloodLoss`,
`BloodDonorChair`), el equipo en `InfirmaryProps.BloodBankEquipment`/`DonorChair` y la prueba `BloodBankTest` (grupo 83).

**Dónde**: planta baja, junto al laboratorio y el analizador. Es un **tramo corrido** (`FurnitureLine`, 2 m con todo lo demás) con:
- la **nevera de hematíes** a 2–6 °C (el puesto «Banco de sangre: cruzar y transfundir», F);
- el **agitador de plaquetas** a 20–24 °C;
- el **congelador de plasma** a ≤ −25 °C y el **descongelador** a 37 °C;
- la **nevera veterinaria** (perro DEA 1, gato A/B);
- el **sillón de donante** (F: donar una unidad).

**Las reglas reales**:

| Producto | Temperatura | Dura | Fuente |
|---|---|---|---|
| Hematíes (SAGM) | 2–6 °C | 42 días | Council of Europe, *Guide to the preparation, use and quality assurance of blood components* (21.ª ed., 2023); OMS, *Clinical Transfusion Process and Patient Safety* (2010); AABB Standards |
| Sangre total (CPDA-1) | 2–6 °C | 35 días | ídem |
| Plasma fresco congelado | ≤ −25 °C | 36 meses (descongelado, 5 días a 2–6 °C) | ídem |
| Plaquetas | 20–24 °C en agitación | 5–7 días | ídem |

- **Compatibilidad**: en hematíes, O− es el donante universal y un Rh+ no va a un Rh−; en plasma, AB es el universal.
- **El grupo de cada persona** sale de su nombre, con la frecuencia real (Cruz Roja Americana): O+ 38 %, A+ 34 %, B+ 9 %, O− 7 %,
  A− 6 %, AB+ 3 %, B− 2 %, AB− 1 %.
- **Banco de sangre andante**: lejos de todo, los donantes son el propio santuario, como en las bases antárticas y en la medicina
  militar (Joint Trauma System, CPG *Whole Blood Transfusion*, 2018). Una unidad son ~450 mL; el donante espera **56 días** hasta la
  siguiente.
- **Hemorragia** (`BloodLoss`, clases del ATLS, 10.ª ed. 2018): I < 15 %, II 15–30 %, III 30–40 %, IV > 40 %. Desde el 30 % el
  paciente empeora solo (shock).
  - Se transfunde primero **su grupo** y, si no hay, O−; con ≥ 40 %, también plasma 1:1.
  - Cada unidad repone 0,45 L del volumen del paciente: 70 mL/kg en personas y, en animales, según su especie y peso.
- **Animales**: perros, grupo DEA 1 (los DEA 1-negativos sirven para la primera transfusión de cualquiera). Gatos, A/B/AB **sin
  donante universal**: hay que tipar siempre, porque los B tienen anti-A fuerte (Davidow 2013, *Vet Clin North Am Small Anim Pract*
  43:735). El resto (oso, foca), fluidos y sangre de su especie.

**Con el resto del hospital**:
- el puesto del banco cruza y transfunde al paciente de al lado;
- el **quirófano no opera** a quien ha perdido ≥ 30 % sin sangre compatible: primero transfunde; si no hay, lo dice (banco de sangre
  andante);
- la **ronda diaria** va dentro de la misión del inventario del hospital (`enf_inventario`, su efecto en `AreaFunctions`): retira lo
  caducado, repone con O− la nevera del **submarino-ambulancia** y, si quedan menos de 4 O−, llama a donar a un O− del santuario;
- el submarino-ambulancia pone sangre O− por el camino y el banco, al llegar (docs/submarinos-medicos.md).

## 5. Submarinos (Berón 2026-11-03)

El **submarino-ambulancia anfibio**, aparcado junto a la puerta del suelo del árbol, recoge al equipo en urgencias (sube por el
tronco), trae personas y grandes animales terrestres al piso de urgencias o al veterinario y lleva al equipo al **Leviatán**: el
hospital submarino de animales gigantes, atracado a ras del hielo, que trata ballenas y orcas en su pozo médico. Todo en
[submarinos-medicos.md](submarinos-medicos.md).

## 6. Paredes de cristal (Berón 2026-11-03)

«Para salas que deban estar aisladas se pueden utilizar paredes de cristal de modo que no bloquean visibilidad, y se debe mantener
las reglas de espaciado, en este caso a dichas paredes. Se debe revisar la lista de hospitales para agregar las paredes de cristal en
todos los que lo requieran.»

Código: `Assets/Scripts/World/GlassWall.cs` (`GlassWall.Room`: la sala alrededor de sus muebles + 2 m; `GlassWall.Box`), las reglas en
`FurnitureSpacing` y `MicroBuildRules`, y la prueba `GlassWallTest` (grupo 84).

**Cómo es una sala de cristal**:
- 4 paños de cristal transparente (alfa 0,22, sin sombra) de 2,8 m de alto, con perfil y zócalo de aluminio;
- una **puerta corredera** abierta: un hueco de 1,8 m con su dintel;
- caja de colisión en cada paño (no se atraviesa) y el hueco de la puerta libre en el NavMesh;
- el tamaño sale de sus muebles: **su caja + 2 m a cada lado**, así lo de dentro guarda el paso hasta el cristal.

**Las reglas**: el cristal es la **única pared permitida** (la regla general es «sin muros ni techos»: tapan la vista y el paso).
- **Plataformas del santuario** (`FurnitureSpacing`): cada paño es un mueble con nombre (`Pared_cristal…`, en `Named`) que guarda los
  **2 m** con todo, dentro y fuera de su sala. Dos paños pueden ir pegados (`Batteries`): las esquinas de una sala, o dos salas vecinas
  que comparten mampara.
- **Microcosmos** (`MicroBuildRules`): la regla 2 deja pasar el cristal (no es «muro»; tampoco su dintel) y la regla 1 mide su paso de
  2 m contra los muebles.

**Revisión de los hospitales** (qué salas deben aislarse y qué se hizo):

| Hospital | Sala | ¿Cristal? | Por qué |
|---|---|---|---|
| Santuario, planta baja (urgencias) | triajes, consultas, laboratorio, banco de sangre | no | ambulatorio abierto |
| | paritorios | no | intimidad: se quedan como zonas (una mampara de cristal no la da) |
| | rayos X | no | blindaje de plomo, no cristal (en todo caso, una mirilla de vidrio plomado) |
| Santuario, planta 1 | **4 quirófanos** | **sí** | presión positiva y flujo limpio (OMS, cirugía segura) |
| | **UCI** | **sí** | aislamiento de contacto; el control vigila a través del cristal |
| | **4 aislamientos + esclusa** | **sí** | presión negativa (≥ 2,5 Pa, ≥ 12 renovaciones/h; ASHRAE 170) y antesala de EPI |
| Santuario, planta 2 (veterinaria) | **4 quirófanos veterinarios** | **sí** | como los humanos |
| | **sala fría de pingüinos** | **sí** | 0–10 °C (máx. 18): el frío no se escapa |
| | **aislamiento veterinario** (nuevo, 2 boxes) | **sí** | cuarentena de un animal infeccioso (el plan pedía cuarentena aparte) |
| | rayos X veterinario | no | blindaje de plomo |
| Microcosmos del hospital (Ambrosio, ×10) | el piso de urgencias, gigante | — | es el mapa (piezas «_gigante», fuera de las reglas) |
| Microcosmos de Escútari (Nightingale, 1854), bimaristán de Bagdad, consulta de Jenner (1796), hospital de sangre colonial | salas de época | no | anacrónico: en esas fechas no había salas de cristal (Nightingale aislaba con aire y limpieza) |
| Prípiat (1986), puesto de dosimetría | ducha de descontaminación | **propuesta** | sería coherente con la época; ese constructor es del hilo de los pueblos: se lo propongo en el informe |

**Lo que cambió en el plano**: para que cada sala guarde sus 2 m dentro y fuera, la planta 1 y el ala este de la planta 2 se
redistribuyeron en el piso de 40 × 40 m del árbol (antes, 32 × 26 m en el centro). Los aislamientos ya no son el prop
`IsolationRoom` (2,6 × 3,2 m con mamparas pegadas a la cama): ahora son camas en salas de 5,2 × 6,7 m.

## 7. Pendiente

- Del plan veterinario falta la UCI veterinaria, el pediluvio en cada ala y la **necropsia** aparte. La **piscina médica de cetáceos**
  ya existe: es el pozo del Leviatán (§5).
- Laboratorio humano: el plan pide 4 puestos y hay uno (más el analizador y el banco de sangre).
- Morgue y lavandería no están modeladas.
- La cirugía no toca aún `BodyInjuries` (las lesiones de historia, p. ej. Shanidar, se quedan como están).
- Los paritorios podrían llevar cristal esmerilado (intimidad sin muro), si Berón lo quiere.
