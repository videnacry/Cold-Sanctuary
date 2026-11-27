# Changelog

Resumen por PR fusionado en `master`, el más reciente arriba. Una línea por cambio; el detalle está en
el PR, en `git log` y en [`docs/checklist.md`](docs/checklist.md) (Historial).
Al fusionar un PR, añade su línea en «Sin publicar» o en el mes en curso.

## Sin publicar
- PB-170 F1b: laboratorios de Química y Biología, cada uno en un piso (antes dos cada uno); Ciencias de 4 niveles a 2.
- PB-170 F1b (W9, #330): la sala de bailes es el primer sitio de «Bailar» en el horario libre (si su piso no está en obras).
- Revisión de #328 (piso «Salas»): fusionado; restos menores en PENDIENTES y checklist.
- PB-170 F1b: piso «Salas» (Ballet en obras y sala de bailes en el último piso de estudios de Música; Entrenamiento con un piso menos); la prueba 64 compara pisos de yoga por índice.
- Revisión de #326: la caja del gimnasio, de 40 × 38 (`FitAreaBox`, compartido con el Textil); sin restos de «Cardio y zona libre» en `AreaCapacity`; `GymCarpetSpot` (sin uso) fuera; gimnasio de 20 plazas también en `FloorOrganization`.
- PB-170 F1b: piso «Gimnasio y cardio» (10 máquinas × 2, cardio y zona libre en un piso de 40 × 40; Gimnasio 20 plazas); TankSpot fuera de las zonas de otras áreas; la guardia ya no sale del piso.
- Revisión de #324: la caja del piso del Textil, de 40 × 38 (sus bordillos cruzaban el huerto y el estudio; su «Mat» dejaba fuera parte del estudio).
- PB-170 F1b: piso mixto «Textil y huerto vertical» (el huerto, zona de 18 × 30 m del piso del Textil).
- PB-170 F1b: piso mixto «Asambleas y guardia» (la guardia, zona de 16,5 × 18 m del piso de Asambleas; Servicios, 4 → 3 niveles).
- PB-170 F1b: piso mixto «Cría y carne cultivada» (el laboratorio, zona de 18 × 30 m del piso de la Cría; Servicios con un piso libre).
- PB-170 F1b: piso mixto «Mecánica y garaje» (Producción, 3 → 2 niveles); `AreaCapacity.Check` da línea a las actividades con todos sus puestos en obras (prueba 75); detalle en la prueba 113.
- PB-170 F1a del rediseño de pisos: losas de 40 × 40 m en todas las torres-árbol (Entrenamiento y Producción apartados 5 y 3 m), regla T27 y avisos `[Pisos]` de pisos a medias (`FurnitureSpacing.FillOf`), un piso por centro de entrenamiento (`Amplios` apagado), ocio de los centros por plazas y prueba 143.
- PB-170 F0 del rediseño de pisos: J16/S13/J36 y T1 reescritas (un piso por actividad, entrenamiento por plazas), regla T26 y `FloorOrganization` (marcas de cada piso y reorganización por cálculo), reparto del entrenamiento por plazas en `WorkRoster` y prueba 142.
- PB-170: plan del rediseño de pisos y edificios-árbol portado de CodeShip, en [`docs/areas/rediseno-pisos-arboles.md`](docs/areas/rediseno-pisos-arboles.md) (espera decisión).
- PB-170: propuesta para portar los cambios del santuario 1 de CodeShip (listas 6.ª–16.ª) en [`docs/sistemas/portar-codeship-s1.md`](docs/sistemas/portar-codeship-s1.md).
- Docs organizados por carpeta (191 movidos con `git mv`, 3568 referencias reescritas en docs y `.cs`), cabecera `resumen/temas/estado/codigo` en cada doc, índice generado y buscador `tools/docs.py` (`find`, `de`, `temas`, `check`) con su CI `docs.yml`.
- Documentación para ahorrar tokens: `CLAUDE.md` corto, [`docs/ARQUITECTURA.md`](docs/ARQUITECTURA.md) (mapa de carpetas y sistemas), [`docs/README.md`](docs/README.md) (índice de docs) y este `CHANGELOG.md`.

## 2026-11
- #306 Edificios: 10 edificios-árbol, gimnasio de 3 plantas, dojo y ballet en obra, 25 pueblos de microcosmos desde mapas reales, horario libre, submarinos de obra y hospital, jardines frutales, reparto de jornada, alimentador de depredadores.
- #301 Microcosmos del río, sala de yoga y gimnasio.

## 2026-10
- #300 Alfombras voladoras en vez de escaleras/puentes, plantilla de microcosmos, iniciación y muebles a medida.
- #299 Experiencia del jugador: examen de llegada, tanque e historia de Ambrosio.
- #298 Hechizo «progreso»: checklist de historias y misiones.
- #297 Revisión del código: 5 fallos y 2 limpiezas.
- #296 Edificios de investigación (Química, Biología, Física) y central nuclear de producción.
- #295 Arranque jugable: pruebas a petición, lanzador sin selección por defecto, mallas combinadas.
- #293, #294 Restaurante real: cocina en toda la planta, comedor en anillo y servicio con camarero.
- #292 Microcosmos: suelo navegable sin islas.
- #290 Hospital v2 (capacidad ×4) + hospital veterinario.
- #287 Hábitos naturales en el mundo principal (LOD, descanso, censo marino, ordeño).
- #286, #288, #289, #291 Fauna y conducta de las eras: 13 insectos-humanos con ciclo de vida, prefabs automáticos.
- #272, #276–#278 Enfermería → Hospital de 2 plantas; huerto asiático; microcosmos sin escenas huérfanas.
- #258 Escaleras verticales en todas las plataformas.
- #247–#256 Yoga por respiraciones, Ashtanga primaria, enseñanza interactiva, hechizos aprendibles, iniciaciones de Linduarda y Goluis, salto temporal al dormir.
- #205–#212 UI de pensamiento, control por necesidad, campo de pensamientos, charlas, restaurante jugable, hechizos por teclado + colas tipo Sims, AnymaFactory, Saludo al Sol, 22 platillos.

## 2026-09
- #204 Manual de Unity + nivel de los peloteros.
- #197–#203 Percepción por órganos sensoriales, HUD declarativo, fauna de la era del fuego, Linduarda.
- #181–#196 Era pre-Ambrosio (origen de Sakshi), Guardián/apremios/observación, modelos científicos (PAD, Big Five), Anima concreta (adiós `SimpleAnima`), CODEMAP + CI.
- #174–#180 Microcosmos como escena propia; Ambrosio en 4 rebanadas.
- #164–#173 Santuario 1 a fauna de hielo: krill, pingüino, orca, fitoplancton, bancos de peces, LOD, sueño día/noche.

## 2026-08
- #135–#163 Motor de volición, navegación por rastros (`TraceField`), `TestProbe`/`TestRunner`, celo, reproducción, enfermedad, inanición: ciclo de vida completo y mundo vivo.
- #126 Se borra la jerarquía `Carnivore`/`Herbivore`.
- #62–#86 `SpellBase` unificada, alma por mezcla, alma compartida, `BondPillar`, retirada de `CompanionBase`, humores y estrés químico.
- #44–#55 Reservas de magia, coste real de hechizos, `QuarkReserve`, minijuego de descomposición.
- #23–#43 Stats como verdad, `CreatureRig`, modelo de emoción, transformación, depredación por stats, composición, química y metabolismo.

## 2026-07
- #1–#22 Plano mágico y meditación, mundo mob, avatares, tríada Micro/Meso/Macrocosmos, farming, aptitudes y margas, mente, control intercambiable, Forja/Mecánica, upa-yoga.
