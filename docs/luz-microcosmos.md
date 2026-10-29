# Luz por fecha, hora y lugar (microcosmos y santuario polar)

> Propuesta del 2026-10-27. **Todavía no está implementada.** Hoy `MicrocosmTemplate.Sun(light, hour)`
> (`Assets/Editor/MicrocosmTemplate.cs`) usa un arco genérico: el sol sale a las 6, culmina a 60° a las 12 y se pone a
> las 18, igual en todas partes. Berón quiere que dependa de la **fecha, la hora y el lugar** de cada microcosmos, y que el
> santuario, que está en un **polo**, tenga día y noche polares.
> Cálculo: ecuaciones de posición solar de la NOAA (Global Monitoring Laboratory, versión de «año fraccional»). Lo hice
> con un script de **PowerShell 5.1**, porque en esta máquina no hay Python ni Node; la fórmula se copia tal cual en C#.

## 1. Cómo se ilumina hoy
- **Santuario (SampleScene).** No hay ciclo de luz. La escena tiene una «Directional Light» fija con la rotación por defecto de Unity, Euler(50°, −30°): `m_LocalRotation {0.408, −0.235, 0.109, 0.875}`, color #FFF4D6 e intensidad 1. Ningún script la mueve, y `SampleSceneBuilder` no crea ninguna luz (solo la niebla, en `BuildSeaAndFog`).
- **Reloj.** `TimeController` (`Assets/Scripts/Time/Time.cs`) junto con `Clock`:
  - un día de juego dura 60 min reales;
  - el año (`DaysPerYear`) dura **28 días de juego**: 4 estaciones de 7;
  - `IsDay` es fijo, de 6 a 18 h.
  - Lo usan `AreaHours`, el fitoplancton, la colmena, el sueño y la jornada.
- **Paneles.** `SolarArray` produce con un seno de 6 a 18 h, sea cual sea la estación. En su comentario, los paneles miran a +Z, que llama «sur del santuario».
- **Microcosmos.** `MicrocosmTemplate.Sun` hace la luz con `Quaternion.Euler(elev, az, 0)`, con az = 90° a las 6 h («sale por el este»). Pero el *forward* de una luz direccional es hacia donde **va** la luz: Euler(e, 90°) ilumina hacia +X. Si +X es el este, el sol aparece por el **oeste**, así que la orientación está al revés. La correcta es `yaw = az + 180°`, con el convenio +Z = norte y +X = este.
- **Mesopotamia** (`MobWorldSceneBuilder`) también tiene la luz fija en Euler(50°, −30°).

## 2. Microcosmos, lugar, fecha y luz
La hora es la **hora solar media local** (UTC + longitud/15), salvo en dos casos: Dayton usa la hora del reloj EST (UTC−5), que está documentada, y la Tregua usa GMT (la de las líneas británicas).
- Cuando el código solo da una estación, tomé una fecha representativa y la marco con *.
- Las fechas antiguas se calculan con la órbita actual. Para −240 o −150 el error es menor de 0,5°. Para −1 000 000, la oblicuidad era de 22–24,5°, así que el error es de unos ±1–2°, lo que basta para un juego.
- El orto y el ocaso incluyen la refracción (cénit de 90,833°).

| Microcosmos (builder) | Lugar | Lat | Lon | Fecha (día del año) | Hora | Elevación | Azimut | Orto · ocaso | Fase | Luz 0–1 | Color |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Sakshi, el río (`MicrocosmosSceneBuilder.BuildScene0`) | taiga euroasiática; referencia, Urales medios* | 60,00 | 60,00 | 1 may* (121) | 6,0 | 13,1° | 83° | 04:01 · 19:53 | día | 0,66 | #FFDCB8 |
| Ambrosio, nivel 1 (`BuildScene1`) y Hospital (`AreaDates`) | taiga euroasiática* | 60,00 | 60,00 | 1 jul* (182) | 5,5 | 15,9° | 71° | 02:40 · 21:26 | día | 0,69 | #FFDFBE |
| Cocina (`AreaDates`) | cueva de Qesem, Levante* | 32,10 | 34,97 | 15 oct* (288) | 19,0 | −20,1° | 273° | 06:03 · 17:29 | noche | 0,04 | #738CD9 |
| Huerto (`AreaDates`) | Jericó, Levante* | 31,87 | 35,44 | 1 abr* (91) | 9,0 | 38,7° | 113° | 05:50 · 18:19 | día | 0,86 | #FFF6E8 |
| Estudio Textil (`AreaDates`) | Dolní Věstonice, Moravia* | 48,88 | 16,65 | 15 oct* (288) | 15,0 | 18,8° | 232° | 06:19 · 17:13 | día | 0,72 | #FFE2C3 |
| Mecánica (`AreaDates`) | Dolní Věstonice* | 48,88 | 16,65 | 1 jul* (182) | 11,0 | 61,4° | 148° | 04:00 · 20:07 | día | 0,96 | #FFF7EB |
| Mesopotamia (`Eras`; sin fecha en el código) | Uruk | 31,32 | 45,64 | 1 abr* (91) | 10,0* | 50,1° | 127° | 05:50 · 18:19 | día | 0,92 | #FFF7EB |
| Patanjali (`StorySceneBuilder`) | Varanasi, llanura del Ganges* | 25,32 | 83,01 | 15 ene* (15) | 6,5 | −4,4° | 112° | 06:47 · 17:30 | crepúsculo civil | 0,18 | #988CBA |
| Kapilavastu (`Story`) | Tilaurakot, Terai | 27,57 | 83,05 | 15 oct* (288) | 9,0 | 36,4° | 126° | 05:59 · 17:32 | día | 0,85 | #FFF4E4 |
| Egina (`MythSceneBuilder`) | templo de Afaya | 37,75 | 23,53 | 1 jun* (152) | 11,0 | 69,9° | 138° | 04:40 · 19:15 | día | 0,98 | #FFF7EB |
| Alejandría (`Story`) | Alejandría | 31,20 | 29,92 | 21 jun (172) | 12,0 | 82,2° | 178° | 04:56 · 19:07 | día | 1,00 | #FFF7EB |
| (referencia) Siene, el pozo | Asuán | 24,09 | 32,90 | 21 jun (172) | 12,0 | 89,3° | — | 05:13 · 18:50 | día | 1,00 | #FFF7EB |
| Escútari (`Story`) | cuartel de Selimiye, Üsküdar | 41,00 | 29,02 | 15 nov* (319) | 18,5 | −20,3° | 263° | 06:47 · 16:43 | noche | 0,04 | #738CD9 |
| Dayton (`Story`) | Kill Devil Hills, en EST | 36,02 | −75,67 | 17 dic 1903 (351) | 10,58 | 27,4° | 158° | 07:07 · 16:50 | día | 0,79 | #FFEBD3 |
| Tregua 1914 (`Story`) | Ploegsteert, Flandes, en GMT | 50,74 | 2,88 | 25 dic 1914 (359) | 10,0 | 12,1° | 155° | 07:49 · 15:47 | día | 0,65 | #FFDBB6 |
| Le Chambon-sur-Lignon (`CompassionSceneBuilder`) | Le Chambon | 45,06 | 4,30 | 15 oct* 1942 (288) | 16,0 | 12,1° | 245° | 06:14 · 17:17 | día | 0,65 | #FFDBB6 |
| Irena Sendler (`Compassion`) | Varsovia | 52,25 | 21,00 | 15 oct* 1942 (288) | 8,0 | 12,9° | 122° | 06:23 · 17:09 | día | 0,66 | #FFDCB8 |
| Planta Atómica (`PhysicsSceneBuilder`) | Woolsthorpe, de Newton | 52,81 | −0,63 | 10 sep* (253) | 15,0 | 29,5° | 235° | 05:24 · 18:30 | día | 0,81 | #FFEDD7 |
| (referencia) París, de Curie · Berna, de Einstein | — | 48,85 · 46,95 | 2,35 · 7,45 | 10 sep* (253) | 15,0 | 31,7° · 32,8° | 237° · 238° | — | día | 0,82 · 0,83 | — |

Los tres microcosmos del Paleolítico (Yoga, Textil y Mecánica) comparten en `Eras` el lugar «una tribu del Paleolítico». Tomé Dolní Věstonice porque allí están las primeras huellas de tejido. La Sala de Yoga no lleva este sol: su microcosmos es el río de Sakshi.

**Desajustes entre la hora y el relato** (decisión de Berón; las horas propuestas salen del mismo cálculo):
- Ambrosio dice «al alba», pero el 1 de julio a 60° N el sol ya está a 16° a las 5:30. El alba real es a las **2,7 h**; otra opción es retrasar la fecha a primeros de septiembre.
- Sakshi dice «al amanecer», y a las 6 h el sol ya está a 13°. El orto es a las **4,0 h**.
- La Cocina dice «al anochecer», pero a las 19 h es noche cerrada. El ocaso es a las 17:29, así que la hora sería **17,5**.
- Irena Sendler: si las 8:00 son de reloj en 1942 (hora alemana de verano, UTC+2), son las 7:24 de hora solar y el sol está a unos 7°.

## 3. Función `SunFor`
Va en un archivo de runtime, por ejemplo `Assets/Scripts/Time/SolarPosition.cs`. Sirve para el santuario y para el editor.

```csharp
public enum SunPhase { Day, CivilTwilight, NauticalTwilight, AstronomicalTwilight, Night }

public struct SunPos
{
    public float elevation;    // grados sobre el horizonte (sin refracción)
    public float azimuth;      // grados desde el norte, en el sentido de las agujas del reloj (90 = este)
    public float declination;  // grados
    public float sunrise, sunset;   // horas en la misma base que `hour`; NaN si no hay orto u ocaso
    public bool midnightSun, polarNight;
    public SunPhase phase;
}

/// <param name="lat">Latitud en grados (+N, −S).</param>
/// <param name="dayOfYear">Día del año, 1–365. Admite decimales para que la estación avance dentro del día.</param>
/// <param name="hour">Hora 0–24. Por defecto es la hora SOLAR MEDIA LOCAL.</param>
/// <param name="lon">Longitud en grados (+E). Solo hace falta si se da `utcOffset`.</param>
/// <param name="utcOffset">Huso horario de `hour` (p. ej. −5 para EST). Si es null, la hora es la solar media local (lon/15).</param>
public static SunPos SunFor(float lat, float dayOfYear, float hour, float lon = 0f, float? utcOffset = null);
```

Cálculo (NOAA). Los ángulos van en radianes:
1. `tz = utcOffset ?? lon/15` y `γ = 2π/365 · (dayOfYear − 1 + (hour − tz − 12)/24)`.
2. Ecuación del tiempo, en minutos: `eqt = 229.18·(0.000075 + 0.001868 cosγ − 0.032077 sinγ − 0.014615 cos2γ − 0.040849 sin2γ)`.
3. Declinación: `δ = 0.006918 − 0.399912 cosγ + 0.070257 sinγ − 0.006758 cos2γ + 0.000907 sin2γ − 0.002697 cos3γ + 0.00148 sin3γ`.
4. Hora solar verdadera, en minutos: `tst = hour·60 + eqt + 4·lon − 60·tz`. El ángulo horario es `ha = tst/4 − 180` (en grados).
5. Elevación: `cos θz = sinφ sinδ + cosφ cosδ cos ha` y `elevation = 90 − θz`.
6. Azimut: `azimuth = atan2(sin ha, cos ha·sinφ − tanδ·cosφ) + 180`, normalizado a 0–360.
7. Orto y ocaso: `x = cos(90.833°)/(cosφ cosδ) − tanφ tanδ`.
   - Si `x ≤ −1`, es sol de medianoche.
   - Si `x ≥ 1`, es noche polar.
   - Si no, `h0 = acos x` y `sunrise = (720 − 4(lon + h0) − eqt)/60 + tz` y `sunset = (720 − 4(lon − h0) − eqt)/60 + tz`.
8. Fase: día si `el ≥ −0.833`; crepúsculo civil si `≥ −6`; náutico si `≥ −12`; astronómico si `≥ −18`; si no, noche.

**Pasarlo a la luz** (lo comparten `MicrocosmTemplate.Sun` y el santuario):
- `light.transform.rotation = Quaternion.Euler(max(el, 2°), az + 180°, 0)`. Convenio: +Z = norte y +X = este. Bajo el horizonte la luz pasa a ser la **luna**: tenue y fría, a 25° de elevación y con azimut `az` (el lado opuesto del cielo).
- Nivel de luz L (de 0 a 1):
  - si `el ≥ 0`, `L = 0,35 + 0,65·√sin(el)`;
  - si `−6 ≤ el < 0`, `L = 0,12 + 0,23·(el+6)/6`;
  - si `−18 ≤ el < −6`, `L = 0,04 + 0,08·(el+18)/12`;
  - por debajo, `L = 0,04`.
- Intensidad: `light.intensity = 1,2·L`. Ambiente: `RenderSettings.ambientIntensity ≈ 0,3 + 0,7·L`.
- Color, interpolando según la elevación:
  - por debajo de −6°, noche: #738CD9;
  - de −6° a 0°, de la noche al crepúsculo rosado #FF8C66;
  - de 0° a 10°, del dorado #FF9E59 al #FFD9B2;
  - de 10° a 40°, hasta el blanco cálido #FFF7EB;
  - por encima de 40°, #FFF7EB.
- Datos: `MicrocosmSpec` gana `lat`, `lon`, `dayOfYear` y `utcOffset` (opcional). La tabla de la §2 se guarda junto a `Eras.OfMicrocosmos`, por ejemplo como `Eras.GeoOf(display)`. La ventana de la plantilla pide además la latitud, la longitud y el día del año.

## 4. Modelo polar del santuario
**Qué polo.** Los documentos remiten a la **Antártida**:
- la central de producción toma como precedente la PM-3A de McMurdo (`laboratorios-investigacion.md`);
- la telemedicina del hospital, la estación del Polo Sur (`hospital.md`);
- la flora, la antártica (`ice-sanctuary-ecology.md`).

La fauna mezcla los dos polos: osos, zorros árticos y belugas del Ártico, pingüinos de la Antártida.

**Latitud propuesta: 77,85° S, 166,67° E (isla de Ross, la latitud de McMurdo).** En el polo exacto (90° S) el sol no se mueve en altura a lo largo del día: su elevación es igual a la declinación (+23,4° en diciembre, −23,5° en junio) y solo da vueltas alrededor del horizonte, así que la hora no cambiaría la luz. A 78° S hay día y noche polares y, además, el sol sube y baja a lo largo de cada día.
- La alternativa ártica, Svalbard (78,2° N, 15,6° E), da la misma luz con 6 meses de desfase.
- En el hemisferio sur, el sol del mediodía está al **norte** (az ≈ 0°). Los paneles de `SolarArray`, que miran a «+Z = sur», deben mirar al norte.

**Estaciones a 77,85° S** (calculadas):

| Periodo | Días del año | Duración | Mediodía | Medianoche |
|---|---|---|---|---|
| Sol de medianoche | 297 → 51 (24 oct – 20 feb) | 120 d | 23–36° | −1 a 11° |
| Transición de otoño | 52 → 115 | 64 d | 23° → 0° | −1° → −24° |
| Noche polar | 116 → 231 (26 abr – 19 ago) | 116 d | 0° → −11,3° (21 jun) → 0° | hasta −35,6° |
| Transición de primavera | 232 → 296 | 65 d | 0° → 23° | −25° → −1° |

**Del día de juego a la estación.** El año del juego dura `DaysPerYear` = 28 días de juego (28 h reales), así que cada día de juego avanza 365,24/28 = **13,04 días del año**. La fecha avanza de forma continua, también dentro del día:

`dayOfYear = (doy0 + (Day + Hour/24) · 365.2422 / DaysPerYear) mod 365`, con **doy0 = 264** (21 sep, equinoccio de primavera austral). Así la partida empieza con días y noches normales y llega al sol de medianoche enseguida.

| Días de juego (con doy0 = 264) | Qué hay |
|---|---|
| 0 – 2,5 | primavera: días cada vez más largos |
| 2,5 – 11,7 | **sol de medianoche** (9,2 días de juego) |
| 11,7 – 16,6 | otoño: vuelve la noche |
| 16,6 – 25,5 | **noche polar** (8,9 días); al mediodía, crepúsculo civil o náutico |
| 25,5 – 28 | primavera, y vuelta a empezar |

La hora de juego es la hora solar local, así que el mediodía cae a las 12 h. **Elevación del sol por hora** a 77,85° S (L es el nivel de luz):

| Fecha | 0 h | 3 h | 6 h | 9 h | 12 h | 15 h | 18 h | 21 h |
|---|---|---|---|---|---|---|---|---|
| 21 dic (355), sol de medianoche | 11,3° (L 0,64) | 14,7° | 23,0° (0,76) | 31,8° | 35,6° (0,85) | 31,6° | 22,7° | 14,5° |
| 21 feb (52), fin del día polar | −0,9° (0,31) | 2,0° | 10,1° (0,62) | 18,9° | 23,1° (0,76) | 19,9° | 11,4° | 2,7° |
| 21 mar (80), equinoccio | −11,7° | −8,5° | −0,1° | 8,5° | 12,4° | 9,1° | 0,6° | −8,2° |
| 22 abr (112), entrando en la noche polar | −23,7° (0,04) | −20,0° | −11,3° (0,08) | −3,0° | 0,4° (0,40) | −3,2° | −11,7° | −20,4° |
| 21 jun (172), noche polar | −35,6° (0,04) | −31,7° | −23,0° | −14,7° | −11,3° (0,08) | −14,6° | −22,8° | −31,7° |
| 21 sep (264), equinoccio | −13,5° (0,07) | −9,6° | −0,9° (0,31) | 7,6° | 10,9° (0,63) | 7,2° | −1,4° | −9,9° |
| 15 oct (288), primavera | −4,3° (0,19) | −0,2° | 8,5° (0,60) | 17,0° | 20,2° (0,73) | 16,0° | 7,2° | −1,0° |

El azimut sigue a la hora: a las 0 h el sol está al sur (≈180°), a las 6 h al este, a las 12 h al norte y a las 18 h al oeste. El sol nunca sube de 36°, así que la luz polar siempre es rasante y dorada.

**Lo que hay que cambiar** (propuesta, sin implementar):
- Un componente en el santuario, por ejemplo `SunCycle` en `Clock_AUTO`, mueve la «Directional Light» cada frame con `SunFor(-77.85f, dayOfYear, Hour)`. Usa el mismo color y la misma intensidad que los microcosmos.
- `TimeController` gana `SunElevation` e `IsSunUp`. `IsDay` (de 6 a 18) **se mantiene** como horario humano (jornada, sueño, `AreaHours`), para no romper rutinas ni pruebas. Lo que depende de la luz de verdad pasa a `IsSunUp` o a `SunElevation`:
  - el fitoplancton fotosintetiza todo el día en el sol de medianoche y nada en la noche polar;
  - `SolarArray` produce en proporción a `max(0, sin el)`. En la noche polar los paneles no dan nada, y eso justifica la central nuclear.
- Bajo el horizonte, la luz direccional pasa a ser la luna (tenue y fría) y sube la luz ambiente de la nieve, para que la noche polar se pueda jugar.
