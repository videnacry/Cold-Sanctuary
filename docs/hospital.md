# El Hospital del santuario (antes Enfermería)

Pedido de Berón (2026-10-22): la enfermería pasa a **hospital**, porque no hay otro a miles de km. Debe poder tratar
**todos los casos**, tener equipo para **8 médicos a la vez**, **quirófano**, cuidar al **mismo número de personas que de
animales**, y crecer **a dos plantas**. Las camas de la plataforma hija suben a **3 m** (a 2,2 m Kushal chocaba) y **sin
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

## 2. Distribución (32 × 26 m, entrada por +Z)

**Planta baja (personas)** — pasillo central libre de 3 m de la entrada al fondo:

- **Oeste**: recepción/triaje, aseos, aislamiento, 3 camas de observación (Cama 1-3) + carro de parada (urgencias),
  silla de exámenes + mesa del médico + camilla (**iniciación**, intacta), botiquín y lavamanos, consulta 2, telemedicina,
  UCI (3 camas con ventilador) y su control.
- **Este**: sala de espera (10), farmacia, rayos X, ecografía, paritorio, laboratorio, analizador clínico, banco de
  sangre, **quirófano** (mesa, lámpara, anestesia, instrumental), lavado quirúrgico, autoclave y planta de oxígeno.
  El tanque de virtualización sigue en la esquina +X +Z.
- **Galería en U a 3 m** (oeste 10 + fondo 11 + este 9 = **30 camas de planta**, Cama 4-33), sin barandillas, a 2,1 m
  de eje a eje (0,9 m de pasillo entre camas, `FurnitureSpacing`; gotero y monitor detrás del cabecero); el
  cabecero da al borde exterior y por dentro queda un pasillo de ~1,3 m al que llegan las dos escaleras inclinadas.
  Sin postes en el borde exterior (por ahí llegan los puentes).

**Planta alta, a 10 m (animales)** — hija del área (`Planta_veterinaria`, `StackOn`), escalera vertical en su borde +Z:

- **30 plazas**: 6 cuadras (oso, foca, lobo), 16 boxes, 8 jaulas (pingüino, zorro, aves).
- Consulta veterinaria (mesa hidráulica, ecógrafo, báscula: la anestesia va por kilo), quirófano veterinario con su
  lámpara y anestesia, farmacia veterinaria.

## 3. Los 8 puestos médicos y los interactuables

`HospitalStation` (F) actúa sobre el paciente más cercano: urgencias estabiliza (−enfermedad, −estrés), UCI y farmacia
curan, el quirófano cura del todo **si hay alguien en la máquina de anestesia**, imagen/laboratorio diagnostican, la
veterinaria atiende solo animales. Cada uno muestra un dato de ciencia (`HospitalStation.Science`).

Los **8 puestos médicos** (`doctorPost`): urgencias, consulta 1 (mesa del médico), consulta 2, UCI, cirujano,
anestesista, veterinaria y cirugía veterinaria. `WorkdayDirector.WorkPoint` coloca a cada médico con turno en el hospital
en un puesto libre (`HospitalStation.PostFor`) y lo libera al cambiar de área. Misiones nuevas en `AreaWork`:
`hos_urgencias` (obligatoria), `hos_quirofano`, `hos_uci`, `hos_veterinaria`, con sus pasos y anclas en `VirtualMissions`.

## 4. Pendiente

- Los animales grandes suben a la planta alta por la escalera vertical (blockout); falta un montacargas o que los
  lleve la alfombra voladora.
- Morgue y lavandería no están modeladas.
- La cirugía no toca aún `BodyInjuries` (las lesiones de historia, p. ej. Shanidar, se quedan como están).
