# Eras: la fecha de cada microcosmos y las capacidades por época

> Berón, 2026-10-23. Código: `Assets/Scripts/Progression/Era.cs` (`Eras`, `MicroEra`). Lo coloca `AreaMicrocosmos.Build`.

Igual que el reloj (`TimeController`) recrea el día y la jornada, `Eras` recrea la **época**:
- Cada microcosmos lleva su fecha y su lugar (`MicroEra`, en su raíz). Mientras está cargado, `Eras.Now` es su año; en el santuario es el presente (2026).
- Cada **capacidad cultural** tiene un año de inicio con fuente (`Eras.Table`). Las conductas preguntan `Eras.Allows(Capability.X)` antes de hacer algo que no existía.
  - Ejemplo: los mirmidones no ordeñan pulgones antes de la ganadería lechera, ~7000 a. C. (Evershed et al. 2008, Nature 455:528). Las hormigas de verdad sí lo hacen por instinto; aquí se trata de la conducta humana.
- Un microcosmos nuevo solo necesita una entrada en `Eras.OfMicrocosmos` (año y lugar) y otra en `MicroBiome` (flora y fauna). De ahí salen su estación, su bioma y lo que su gente sabe hacer.

| Microcosmos | Año | Lugar |
|---|---|---|
| Hospital | 1 000 000 a. C. | taiga, antes del fuego (Ambrosio) |
| Cocina | 400 000 a. C. | la era del fuego |
| Huerto | 9500 a. C. | los primeros cultivos |
| Sala de Yoga, Estudio Textil, Mecánica | 40 000 a. C. | una tribu del Paleolítico |
| Egina | 1250 a. C. | la Grecia heroica del mito |
| Kapilavastu | 530 a. C. | Terai del Nepal |
| Alejandría | 240 a. C. | Alejandría y Siene |
| Patanjali | 150 a. C. | llanura del Ganges |
| Escútari | 1854 | Estambul |
| Dayton | 1903 | Dayton y Kill Devil Hills |
| Planta Atómica | 1905 | Woolsthorpe, París y Berna |
| Tregua 1914 | 1914 | Flandes |
| Irena Sendler, Le Chambon | 1942 | Varsovia; Francia |

Capacidades y años de inicio: ver `Eras.Table`. Cada una lleva su fuente:
- fuego habitual: 400 000 a. C.;
- cerámica: 18 000 a. C.;
- agricultura: 9500 a. C.;
- pastoreo: 8500 a. C.;
- ordeño: 7000 a. C.;
- bronce: 3300 a. C.;
- escritura: 3200 a. C.;
- hierro: 1200 a. C.;
- vacunas: 1796;
- red eléctrica: 1882;
- antibióticos: 1942;
- reactor nuclear: 1942.
