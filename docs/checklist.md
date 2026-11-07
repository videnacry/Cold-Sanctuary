# Checklist — continuar

Tablero para retomar. Última sesión: 2026-08-28. Marca lo que completes.
Contexto de fondo: [`AUDIT-2026-07-09.md`](AUDIT-2026-07-09.md), [`gaps-vs-planteamiento.md`](gaps-vs-planteamiento.md),
[`world-topology-and-planes.md`](world-topology-and-planes.md) (visión del mundo grande / los 3 planos).

> **HECHO 2026-11-05 (ronda 12, rama claude/fix-ronda12; solo código, sin probar en vivo):** alfombra de la plataforma fuera del Santuario Aéreo, evacuación de todos los de la zona (Linduarda incluida), cordones de Ø 1,8 m en las ramas del Sitio Nuclear, y pruebas 100, 110, 73 y 94 al día con la separación de escenas.
> - [ ] Ascensor del tronco en los dos árboles del Aéreo: subir en persona del andén a la plataforma (lanzadera) y al techo (parque).
> - [ ] Reconstruir SampleScene (solo el santuario) y el Santuario Aéreo.
> - [ ] Probar los grupos 100, 110, 73, 94 y 28.
> **HECHO 2026-11-06 (suite, rama claude/fix-suite; solo código, sin probar en vivo):** `WorkdayDirector` conduce a cada persona en cada frame y solo reparte la búsqueda de misiones (regla W14); `JornadaTest` usa `TickLive`. Las pruebas por tramos deben correr con `TestRunner.Running` encendido (`Temp/solo-running.txt`).
> - [ ] Comprobar en vivo que el personal anda a su velocidad y volver a medir `WorkdayDirector.Update`.
> - [ ] Decidir: el O₂ de la fachada (faltan 425 m²) y el Huerto (faltan 1290 m²) para 31 personas.
>
> **HECHO 2026-11-05 (rendimiento, rama claude/rendimiento; solo código, sin probar en vivo):** lote estático en Play sin reconstruir nada (las piezas siguen con nombre, collider y `enabled`), detalle apagado a más de 120 m, cuerpos y grupos sin dibujar a más de 130 m, sonda con A/B y renderers por builder, menú con los ajustes de URP. Detalle y riesgos: [rendimiento.md](rendimiento.md).
> - [ ] Medir en SampleScene: *Perfilar scripts* (da el coste por tipo), `rendimiento-ab` y el build de medida (`tools/build-perf.sh`).
> - [ ] Probar los grupos 112, 28, 43, 53, 71, 80, 84, 99 y 110 (lote, árbol que gira y las pruebas que miden piezas).
> - [ ] Fotos: colores de la paleta contra sin lotes; una torre vista a 100 y 140 m; un perchero tras coger máscaras.

> **HECHO 2026-11-05 (interfaz y personajes, rama claude/edif-interfaz; solo código, sin probar en vivo):** pila de botones sobre F10 con ventanas de horario (F2), recursos (F3) y misiones y jornada (F4), y el panel de recursos ya no está siempre a la vista ([interfaz-botones.md](interfaz-botones.md)). **Consol**, maestra jefa de Ashtanga, con su clase profunda; **Josedrilo**, jefe de Hatha; **Panterilia** deja el yoga. **Fernando**, paneles solares, con las misiones `sol_` de la rama de energía, ya integrada. Quinta casa por árbol. **Reglas de personajes** P1–P10 y una ficha por persona ([reglas-personajes.md](reglas-personajes.md), `docs/personajes/`).
> - [ ] Reconstruir SampleScene (solo el santuario) y guardarla: personas, casas y árboles nuevos.
> - [ ] Probar los grupos 93, 95, 101, 64, 65, 87, 89, 75, 50 y 28.
> - [ ] Fotos: la pila de botones y sus 3 ventanas; las 3 variaciones de Consol; la quinta casa.
> - [ ] **Berón**: fotos de Consol y Fernando, y la lista de puestos sin confirmar de [reglas-personajes.md](reglas-personajes.md).
> - [ ] Comprobar en vivo que Fernando coge sus `sol_` (rama de energía ya integrada aquí; grupo 101).

> **MANUAL DE DISEÑO (2026-11-04):** todas las reglas de Berón están en [`manual-de-diseno.md`](manual-de-diseno.md): 119 reglas,
> dónde se comprueba cada una, las 2 que quedan pendientes del 2026-11-04 (M9 y el gancho de §11 para los demás pisos) y la
> tabla piso → microcosmos para revisarlos cuando cambia un piso. Al recibir una regla nueva, añadirla allí.

> **HECHO 2026-11-05 (ronda 9, rama claude/ronda9; integra histo, interfaz, energía, nuclear y territorio; solo compilación, sin probar en vivo):** reglas renumeradas: S15 (energía), T18 (Parque), P12–P19 (territorio y producción); pruebas: `AlimentadorTest` 105, `JornadaSaberTest` 104, `GardenInitiationTest` 46, `GymUsabilityTest` 49, `InsectOnlyTest` 106. `PopulationPlan.Humans` ya no es 11: personal + 2. La isla: placa de 1,5 m y roca hasta el lecho; los 300 m son del hielo.
> - [ ] **Probar en vivo** (probador): reconstruir y correr los grupos de la lista de la ronda 9.
> - [ ] **Revisar con 32 humanos:** el grupo 103 (carne, cocina) y cuánta fauna sale al construir.

> **HECHO 2026-11-05 (tanda 3 de historias, rama claude/edif-histo; solo código y compilación, sin probar en vivo):** 12 microcosmos con su historia de
> 8 pasos, su pueblo, sus cuatro categorías y su mapa a mano HECHO ([`microcosmos-historias-3.md`](microcosmos-historias-3.md)): Eloy Alfaro, Galvarino, Atahualpa, la Gran
> Ley de la Paz, Cleopatra, las calculadoras de Langley, Beethoven, Descartes, Ernesto Guevara, Pedro Sánchez, Lincoln y la Feria de las Hazañas. Lincoln, decidido: C-SPAN 1.º,
> Siena 2.º. Pruebas: grupos 78 y 79.
> - [ ] **Probar en vivo** (probador): reconstruir de una en una, en el orden del doc, y «Build Sala de Eras»; grupos 78, 79, 43, 63 y 87; fotos.
> - [x] **Berón** (2026-11-05): el líder asiático es Lee Kuan Yew, confirmado.
> - [x] Atahualpa rehecho: cómo vivían los incas, obras sin terminar y la bienvenida (el día antes de la captura, 1532).
> - [x] Regla W10 (manual §11): revelar lo altruista, bello o de superación; lo inventado, marcado; nada inventado de personas vivas. Las 12 revisadas.
> - [x] **W10 en las anteriores** (2026-11-05): Pitágoras, Heródoto, Eratóstenes, Newton, Nietzsche, al-Juarismi, Einstein, Tesla, Jenner y Curie con su momento; Gates, Buffett y Traoré con hechos públicos. Reconstruirlos de uno en uno cuando se puedan probar.
> - [x] Regla W11 (auditar al cambiar una regla), primera auditoría hecha; reglas nuevas M36–M38. Propuestas sin hacer en [`microcosmos-historias-3.md`](microcosmos-historias-3.md) §W11.
> - [x] **Sala de asambleas** ([`piso-asambleas.md`](piso-asambleas.md)): nivel 4 de Servicios, 72 plazas; Angostura, Onondaga, Madrid, Gettysburg y la Feria re-apuntadas. Reglas S14 y F14, y `AreaDesign`.
> - [x] **Veracidad de las frases** ([`veracidad-frases.md`](veracidad-frases.md), regla M39).
> - [ ] **Sala de asambleas: probar en vivo**: Blockout de solo santuario, «Build Microcosmos…» de las 5 y grupos 77, 43, 28, 57, 90.
> - [x] **S6**: la tapa de cristal de la cabina y los listones de la toguna, permitidos (2026-11-05).
> - [x] **S14/F14 en Huerto y Construcción**: seis métodos y cinco técnicas del mundo, a la vista, con su origen ([`huertos-y-cultivos.md`](huertos-y-cultivos.md), [`construccion-sostenible.md`](construccion-sostenible.md)).
> - [x] **M39 en lo que faltaba**: Le Chambon, Sendler, Egina, Sakshi, Ambrosio y Cocina; cuatro frases de la música con fuente en `Quoted`.
> - [ ] **Huerto y Construcción: probar en vivo**: Blockout de solo santuario; grupos 77, 28; reconstruir Huerto, Palenque, Uagadugú, Egina y Colorado Springs (F6); Música por las frases de Marley y Gabriel.
> - [x] **Sala de guardia** ([`sala-de-guardia.md`](sala-de-guardia.md)): nivel 4 de Servicios, misión `gua_sala`.
> - [x] **Árbol gigante retirado**; sus récords siguen en [`arbol-guardian.md`](arbol-guardian.md).
> - [x] **El cazador, en los 6 árboles de la residencia**; casquetes silvestres con estructuras para la fauna ([`territorio-polar.md`](territorio-polar.md)).
> - [x] **Sin bosque** ([`ice-sanctuary-ecology.md`](ice-sanctuary-ecology.md) §Sin bosque).
> - [x] **Residencia en edificio-árbol** ([`residencia.md`](residencia.md)); casas de árbol guardadas ([`casas-de-arbol.md`](casas-de-arbol.md)); cazador en su celda ([`contencion-arbol-cazador.md`](contencion-arbol-cazador.md)).
> - [x] **El cazador, bajo el agua**: Santuario Marino, con abejas y arañas y ciclo cerrado. Producción vuelve a 2 niveles.
> - [ ] **Probar en vivo sala, residencia, celda y casquetes**: Blockout de solo santuario; grupos 91, 61, 57, 55, 53, 28, 77, y los de pijamadas y almohadas; medir FPS y renderers antes y después.
> - [ ] **Decidir con Berón**: si bastan los abedules como árboles de la isla.
> - [ ] Mezcla de las 15 áreas pendientes de S14.
> - [x] Regla de oro J30 / P11: todos saben hacer de todo. `Mission.owner` ya no es exclusivo.
> - [x] Berón (2026-11-05): los vivos no políticos conservan sus frases, marcadas (M39).
> - [ ] **Decidir con Berón**: lo que el jugador aún no puede hacer (P11).
> - [ ] **Revisar la ficha de Pedro Sánchez tras las elecciones del 29-11-2026** (`Historia.PedroSanchez.cs`, último párrafo).
> - [ ] Repasar los [verificar] del doc (día del clavo de oro, fecha de Lagunillas, número de enfermos de San Pablo y fecha del cruce a nado, cifras de Singapur).
> **HECHO 2026-11-05 (requisitos e iniciaciones como tareas, rama claude/edif-energia; solo código, sin probar en vivo):** cada misión tiene requisitos (`MissionRequirements`). Entrenamiento 8–9 y 14–15, 5 h de tareas. Las iniciaciones son tareas: lista diaria, primero a quien menos sabe (recién llegado, 5 h; luego, 2). Hoy inicia la Magnate; `WorkRoster.instructorsTeach` lo pasa a los compañeros. Reglas J31 y J32; auditoría en [`jornada-y-rotacion.md`](jornada-y-rotacion.md) §Auditoría.
> - [ ] Probar en vivo, sin reconstruir nada: grupos 104, 50, 28, 55, 65, 87, 96 y 97. En el registro, `[Iniciación]` y el balance: cuántos clones hacen falta los primeros días.
> - [ ] Mirar si llegan a tiempo: el viaje entre árboles se come parte de cada hora, y ahora hay iniciaciones por toda la jornada.

> **HECHO 2026-11-05 (la energía, rama claude/edif-energia; solo código, sin probar en vivo):** el **Parque Energético**, un casquete artificial al noroeste con su árbol bajo:
> campo solar con las mesas que piden los pisos, 3 aerogeneradores de 330 kW con pala negra, cámaras y radar, turbina mareal piloto con sonar, baterías y silo de arena. Cada piso declara
> su demanda (`FloorEnergy`) y paga sus paneles; el validador comprueba oferta ≥ demanda. Las misiones `sol_*` y `ene_*` son de Fernando. Investigación y decisión en [`energia.md`](energia.md);
> reglas S15 y T18 del manual. Prueba nueva `EnergyTest` (grupo 101).
> - [ ] **Probar en vivo:** reconstruir SampleScene con «Build Sample Scene Blockout (solo el santuario, sin microcosmos)», guardarla, y mirar el registro `[Energía]` (la regla debe cumplir, con los kW de verdad).
> - [ ] Grupos 101, 96 (pasos con E), 97 (actuación y uniforme), 61 (catálogo: quité `sol_instalar` y `sol_reparar`), 57 (primer piso a 12 m), 90 (turbinas en T13), 75 (capacidad) y 50 (jornada).
> - [ ] Fotos: el campo desde el techo del árbol, una torre con la pala negra y su detector, la pontona y el rotor bajo el agua, el almacén, Fernando con el uniforme en una mesa.
> - [ ] Aves (Berón, 4 ideas, [`energia.md`](energia.md) §7): el refugio en la planta veterinaria (mirar en `[Refugio de aves]` que no cayó sobre un mueble); la ronda; las figuras y el dragón de drones al saltar un detector (el dragón es un adorno del juego, sin pruebas).

> **HECHO 2026-11-04 (entrenamientos reales en el gimnasio, rama claude/edif-entreno; solo código, sin probar en vivo):** cada
> humano del personal tiene un **plan semanal** (reparto, días, objetivo) y en sus días de gimnasio sigue la rutina del día en
> orden: calentamiento, series × repeticiones contadas en cada máquina, descanso entre series, cambio de máquina si está ocupada
> (alternativa, espera acotada o al final) y vuelta a la calma; progresión doble semanal y descarga cada 4.ª semana. **Entrenador:
> Andrea** (el de más fuerza; el yoga pasa a Josedrilo en la rama de personajes) (misión de papel fijo «gym_planes», clase de calentamiento, correcciones y spotter). Panel: consola `rutina`.
> Investigación con fuentes y planes asignados en [`gimnasio-maquinas.md`](gimnasio-maquinas.md) §15; reglas J19–J23 del manual. **Microcosmos (§16)**: los insectos-humanos entrenan como en su época (23 clases de lugar de entrenar de los pueblos, ejercicios con sus años, el maestro del pueblo), con el mismo motor.
> Prueba nueva `GymTrainingTest` (grupo 87).
> - [ ] **Probar en vivo** (reconstruir SampleScene con «Build Sample Scene Blockout (solo el santuario, sin microcosmos)» y guardarla: la fuerza 1,6 de Andrea y que salga en el gimnasio están en el constructor; luego la escena `Prueba_Yoga`): grupos 87 y 50 (`GymUsabilityTest`), y 28/50
>   (`SanctuaryLifeTest`, `JornadaTest`: el reparto de las 14 h cambia con los días de cada plan y Andrea tiene la misión de 1 h).
> - [ ] Fotos: la clase de calentamiento de Andrea en la zona libre (que las filas no caigan dentro de una máquina) y una serie
>   con su descanso; abrir `rutina(Amahuz)` durante el bloque.
> - [ ] **Microcosmos**: reconstruir uno a uno (regla W5) Crotona, Velliangiri y Sils Maria y mirar en la práctica de las 14 h la clase del maestro del pueblo y las series en sus aparatos (`[Entreno micro]` en la consola).
> - [ ] Si Berón quiere: que cada personaje tenga su propio perfil afinado (ahora los objetivos salen de sus fichas).

> **HECHO 2026-11-04 (hilo del yoga, rama claude/edif-yoga; sin probar en vivo):** **sin anillo de yoga** y **dos pisos de yoga**
> en la torre del Entrenamiento (sube a 4 niveles: P6 Hatha = Sala de Yoga, P9 Ashtanga = `Planta_ashtanga`, P14 libre), cada
> uno con su sala **occidental** (madera, esterillas, accesorios, Buda/Ganesha, sin espejo) y su sala **tradicional** (tierra,
> algodón, kusha, lámparas de aceite, linga como el Dhyanalinga o Adiyogi y Patanjali, y un **kund** con agua del tronco para
> sumergirse antes de practicar). Tarima en el centro y 8 esterillas en un solo anillo (líneas de visión medidas,
> `YogaHall.BlockedSightLines`). La clase guiada es **Hatha** y el entrenamiento de las 14 h **Ashtanga**; la sala del día alterna
> (tradicional los pares); puede haber una clase por sala a la vez. Ocio: Hatha tradicional y Ashtanga occidental. Investigación
> con fuentes en [`yoga-pisos.md`](yoga-pisos.md); reglas F7–F12 del manual. Prueba nueva `YogaFloorsTest` (grupo 64).
> - [ ] **Probar en vivo**: reconstruir la torre del Entrenamiento y los microcosmos de la Sala de Yoga (río de Sakshi, Patanjali,
>   Nietzsche); grupos 64, 51, 42, 57, 28 y el horario libre; fotos de las cuatro salas.
> - [ ] Ver si la Ashtanga Primaria cabe en el bloque de entrenamiento (si no, una versión corta para la clase).

## Actividades de verdad, la orquesta de la comida y los uniformes (2026-11-04, rama claude/edif-actividades)

Doc: [`actividades-y-uniformes.md`](actividades-y-uniformes.md) (auditoría, pasos, uniformes, pendientes); manual J22–J25. Sin probar en vivo.
- [x] **Auditoría** de cada misión de la jornada, actividad libre, recuperación y jornada del insecto-humano (doc §1): casi todo «llegaba y se quedaba».
- [x] **Orquesta de la comida** (`MealRitual`): bandeja → sentarse → cubiertos → bocados (la comida mengua, saciedad y nutrientes por bocado) → beber → recoger → devolver; en el restaurante (`DineSpell`), en casa (`HomeNeedSpell`) y en el comedor de la era (`MicroJornada`, cuchara).
- [x] **Hacer la actividad** (`ActivityPerformer`, `Performances`): 51 de las 55 misiones del catálogo diario con pasos propios (las 4 de la clase de Hatha ya las hace `YogaClass`), las del día y de prefijo, ballet/calistenia/dojo, el ocio que era solo una postura y la brigada del restaurante con su utensilio; la cola larga (funciones de los microcosmos) con el bucle de su clase.
- [x] **Uniformes** (`Uniforms`, `UniformWearer`): por tarea al llegar, su ropa al acabar, neopreno al nadar; pelo, cara y piel intactos; delantal de su oficio y era para el insecto-humano.
- [x] Prueba **`ActividadesTest` (grupo 97)**.
- [ ] **Probar en vivo** (probador): Play en SampleScene un día de jornada y un microcosmos civilizado ya construido, de uno en uno; grupos 97, 50, 28, 65, 41; fotos del doc §6.
- [ ] Pasos propios para las funciones de los microcosmos más frecuentes (mercado, alfarería, bomberos, comisaría…): hoy hacen el bucle genérico de su clase.
- [ ] Turistas del restaurante con la orquesta de la comida; el jugador (lo lleva el hilo de la interacción E); `TreatPatient` desde la actuación del hospital.

## Suelo, flora encima, elementos del piso, alfombras, escala cívica y estatuas (2026-11-04, rama claude/edif-micro3)

Doc: [`microcosmos-suelo-y-elementos.md`](microcosmos-suelo-y-elementos.md); manual F2, F3, C7, H8, M9, M13, M30, M32, M33, M34, §11.
- [x] **Flora encima, nunca debajo** (`MicroGround.Settle`/`ValidateFlora`, regla 7 de `MicroFloors.Validate`); causa de Ambrosio: la flora iba al suelo bajado del área, bajo las losas del hospital.
- [x] **Capa de tierra de 10 cm** con ruido, cúmulos y piedras, en mosaicos combinados y con collider (`MicroSoilBuilder`, regla 8); la historia y el pueblo encima (`RaiseTown`).
- [x] **Tercera forma «Forma_microcosmos»** (`FloorForms.MicroName`, `MicroFloorForm`) y **catálogo de elementos** (`MicroFloorElements`) con versión meso y micro; vestida en su sitio (`MicroFloorDress`); cambios de historia (`Overrides`).
- [x] **Hospital propio y sincronía para todos los pisos**: la forma de microcosmos se hace desde el inventario del meso; `Missing`, firma, registro `UserSettings/MicroFloorDeps.txt`, aviso «REVISAR MICROCOSMOS» y menú «Pisos: microcosmos por revisar».
- [x] **Alfombras de 7 cm** con núcleo de circuitos y costura de LED (`FlyingCarpet.Thickness`).
- [x] **Escala cívica por pieza** (no ×2): asientos utilizables (`MicroSeat`, `PublicSeatSpell`), bancos en la plaza, plato y olla por raciones, fuente y estatua monumentales (`MicroCivicScale`, reglas 13–15).
- [x] **Estatuas de insecto-humano** (`MicroStatue`) en la plaza y colosos en la forma de microcosmos.
- [ ] **Verificar en vivo** (probador): reconstruir SampleScene y luego, UNO A UNO, Bolívar, Ambrosio, Hospital, Sakshi y Cocina (y el resto cuando se pueda); grupos 68 y 67, y 43, 60, 62, 63, 66 (las reglas nuevas 7–9 y 13–15 harán fallar los microcosmos aún sin reconstruir); fotos con `Tools/TmpCheck/Microcosmos suelo fotos` y `Alfombra canto foto`.
- [x] **Capa de tierra APAGADA** por defecto (`MicroGround.SoilEnabled`; colgaba el NavMesh de la Cocina) y, encendida, con collider liso aparte.
- [x] **Mapas a mano** (`MicroStoryMaps`, regla M9, [`microcosmos-mapas-a-mano.md`](microcosmos-mapas-a-mano.md)): Angostura hecho (flora, fauna, piso, edificios); lo colocado cae desde el aire (`MicroGround.Drop`); la siembra al azar es respaldo y se registra.
- [ ] Hacer los mapas a mano de las demás historias (esbozos con lo que les falta); el gancho de Claude Code para los demás pisos.

> **HECHO 2026-11-04 (hilo de personajes, rama claude/edif-personajes):** la **familia y los amigos de Berón** (Nesvitor, Amykhuz y Karen, Alicita, Ducristin, Josedrilo, Maite, Shalva, Adaguchi) en el santuario con su aspecto de sus fotos, Tote mejorado, y los **famosos** (Sadhguru y los seis del mesocosmos) **fuera, a sus microcosmos**, con sus papeles repartidos (yoga → Andrea). *Falta:* la verificación en vivo del tester y que Berón confirme los nombres de `cumbaya.jpg`. Ver la sección «Familia y amigos de Berón» abajo.>
>
> **HECHO 2026-11-03 (hilo del hospital, rama claude/edif-hospital):** **jardines frutales en la cima** de cada edificio-árbol en **bancales de tierra** por gremios (compañeras, flores, insectos útiles, abejorros, lombrices; tierra frente a maceta, con fuentes), **submarino-ambulancia anfibio** que entra por el tronco a por el equipo y lleva personas y grandes animales a urgencias o veterinaria, el **Leviatán** (hospital submarino de ballenas y orcas con pozo médico de suelo elevable), **banco de sangre** y **paredes de cristal** (la única pared permitida, con sus 2 m) en las salas aisladas del hospital. Pruebas 80–84; [`submarinos-medicos.md`](submarinos-medicos.md), [`hospital.md`](hospital.md) §4–§6.
>
> **HECHO 2026-11-03 (hilo de los árboles, rama claude/edif-arboles):** **máscaras de oxígeno** en los edificios-árbol (perchero en cada puerta, estación dentro, oxígeno ilimitado, rescate; [`mascaras-de-oxigeno.md`](mascaras-de-oxigeno.md)), **puertas cuadradas de 7 × 7 m** (8 personas y el submarino), **fondo abierto** al mar con cortinas de fitoplancton, **tareas del jardinero** (máscaras y fitoplancton; rondas repartidas de nuevo), **grupos de animales** con individuos de verdad (peces plateados, kril, medusas de McMurdo y enjambres de abejas de árbol en árbol; [`grupos-de-animales.md`](grupos-de-animales.md)) y el **submarino de obra** anfibio con su garaje ([`submarino-de-obra.md`](submarino-de-obra.md)). Pruebas 70–73 y la escena `Prueba_Mascaras`. *Falta:* lo que diga el informe del hilo (verificación en vivo).
> **HECHO 2026-11-04 (rama claude/edif-cohetes):** **lanzadera de cohetes** en un casquete artificial a ≥ 300 m del santuario, con un árbol-edificio reforzado (pisos «Fábrica de cohetes», «Control de misión» y garaje), la corona de lanzamiento con deflector, trinchera de llamas, diluvio, cubeta y escudo, y la **regla de altura** (plataforma ≥ máx(lo más alto, aves) + 30 m; se valida y sube sola). [`lanzadera-de-cohetes.md`](lanzadera-de-cohetes.md), prueba 90. *Falta:* verificación en vivo, el aviso y el lanzamiento en la jornada.
> **HECHO 2026-11-04 (rama claude/edif-cohetes):** **árbol guardián, el más grande de la historia**: injerto de la base del castaño de los cien caballos (18,4 m, acanalada y con contrafuertes como el Tule) y del fuste y la copa de la secuoya (Hyperion / el récord de Ferguson, 132,6 m; alto = lo mayor de eso y del edificio más alto), con **dos pasadizos de 7 × 7 m** en cruz, colores de hielo y panales. [`arbol-guardian.md`](arbol-guardian.md), prueba 91. *Falta:* verificación en vivo.
> **HECHO 2026-11-05 (rama claude/edif-nuclear):** **sitio nuclear**: Central Nuclear y Planta Atómica (con ciclotrón y sala de experimentos) se mudan a su casquete artificial en (510, −510), a ≥ 500 m y a sotavento, en un árbol bajo: reactores a 12 m, contención, tronco como depósito por gravedad, chimenea filtrada, vigilancia, boyas y simulacro semanal. **Sin gran altura**: se explica por qué en [`sitio-nuclear.md`](sitio-nuclear.md). Manual T14, prueba 94. Ciencias queda con 3 pisos libres. Después (2026-11-05): **el microrreactor y los residuos, en cavernas con 60 m de roca bajo el lecho**; el TRIGA, en su cajón de doble casco apoyado en el lecho; evacuación real y sin tiempo libre en la zona (§3 y §4). **Sin energía sísmica** (§5). Propulsores y polvorín, al aire en el casquete de la lanzadera (§6, T15). *Falta:* rehacer el santuario y verificación en vivo.
> **HECHO 2026-11-05 (rama claude/edif-nuclear, sin probar en vivo):** **la roca de las cavernas y la línea** ([`sitio-nuclear.md`](sitio-nuclear.md) §7–§8): holgura medida de ≥ 3 m, caverna de residuos más grande, pilar de 18 m, instrumentos y panel de la roca, pilotes e hitos en el cajón del TRIGA; misiones `nuc_roca`, `nuc_bajada` y `nuc_refuerzo`; funicular sellado del borde del casquete a la galería (~53 s, con pista de alfombras) y pozo de escape propio. Prueba: `NuclearAccessTest` (grupo 100). Reglas T16 y T17.
> **HECHO 2026-11-05 (rama claude/edif-nuclear, sin probar en vivo):** **mar de 500 m en todo el mapa** (`SeaRule`), **viaje a la galería en ~58 s** (20 m/s, 1,5 m/s², esclusas de 8 s; [`sitio-nuclear.md`](sitio-nuclear.md) §9), cajón del TRIGA colgado con tendones, casquetes atados al lecho, **salas de 44 × 40 m con columnas de roca**, bulones y cables (§10), y revisión de lo existente (§11). Sin revisar: el hielo de las islas sobre un mar de 500 m. Reglas T14, T16, T17 y T19.
> **HECHO 2026-11-05 (rama claude/edif-nuclear, sin probar en vivo):** **refugio del fondo** (12 plazas, 7 días, `nuc_refugio`), **plataforma de hielo de 300 m** con el submarino de obra por debajo, **escudo, compuerta, radar y sonar** en el tubo, intercambiador en un hoyo y **dos icebergs** con kril, remolque (`nuc_remolque`) y desconexión de la línea ([`sitio-nuclear.md`](sitio-nuclear.md) §12–§15). Pendiente: la fauna que pasa bajo las placas y los respiraderos de focas. Reglas T20–T22.
> **HECHO 2026-11-05 (rama claude/edif-nuclear, sin probar en vivo):** **refugio para un mes** (ósmosis, electrólisis, sondeos, perforadora, hojas y espirulina; sin gallinas), **protección de icebergs en los tres casquetes** y **tubo que se suelta por encima de −420 m** ([`sitio-nuclear.md`](sitio-nuclear.md) §16–§19).
> **HECHO 2026-11-05 (rama claude/edif-nuclear, sin probar en vivo):** **dos santuarios**: `Santuario_Marino` con su menú (lecho a 500 m, salas, refugio) y santuario 1 con mar de 16 m, placas de 1,5 m, falda de roca y submarino a −12 m. Los trabajadores bajan sin cargar la escena; Kushal, en persona, con carga aditiva dentro de los ~58 s. Sitio nuclear a (360, −360) ([`sitio-nuclear.md`](sitio-nuclear.md) §20–§22). **Sustituye** a lo de abajo: el mar de 500 m, la plataforma de 300 m y los túneles de la isla. Falta construir la escena y medir su memoria.
> **HECHO 2026-11-05 (rama claude/edif-nuclear, sin probar en vivo):** **Santuario Aéreo**: escena `Santuario_Aereo` con su menú, con la lanzadera, lo que se ve del parque y las aves frías; muelle aéreo en el santuario 1; los trabajadores van sin cargarla y Kushal en persona; la energía llega por cable ([`sitio-nuclear.md`](sitio-nuclear.md) §23). Falta construir la escena y medir la memoria.
> **HECHO 2026-11-05 (ronda 10, rama claude/ronda10; solo compilado, sin probar en vivo):** integrados nuclear, territorio, energia, histo y fix-ronda8. Hielo fijo a 1,5 m, placas como pared por diseño (P20), `IceOverBody` junto a las paredes, **inician los trabajadores y la Magnate es el último recurso** (`instructorsTeach`), pasos virtuales de las misiones de roca e iceberg, y cómo volver a juntar los santuarios en [sitio-nuclear.md](sitio-nuclear.md) §24. 201 reglas.
> **HECHO 2026-11-04 (rama claude/edif-cohetes):** **obras que avanzan a la vista** («el progreso de las obras podría ser visual?»): las piezas del acabado salen día a día por etapas (cimientos y suelo → estructura → instalaciones → muebles y máquinas → acabados y decoración) durante cada turno de «Trabajar la obra», con martillo y polvo, palés que menguan, andamio que va a lo que se construye y cartel con etapa y %; maqueta sin colisiones ni conductas hasta terminar; se guarda por días hechos. [`construction-simulation.md`](construction-simulation.md) §7, manual F13, prueba 98. *Falta:* verificación en vivo.
> **HECHO 2026-11-05 (ronda 7, rama claude/edif-cohetes):** el **cohete con forma de cohete** (`RocketModel`: toberas, faldón, aletas, etapas, cofia ojival), torre umbilical de celosía y escudo con contrafuertes (hace falta rehacer el santuario); **abejas y medusas con luz propia suave** para que no salgan negras a contraluz; la **araña ya no mata a la reina** dentro de su colonia (solo en el vuelo nupcial) y la muerte de una reina deja su registro `[Colmena]`. Pruebas corregidas: 98 (los nombres de pieza se repiten), 28 (colmena con reina viva) y 50 (si la misión recorre áreas, se escapa al hielo). *Falta:* verificación en vivo; una colonia sin reina sigue sin criar otra.
>
>
>
> **ESTADO (2026-08-28) — MUNDO VIVO.** Cerrado el gran arco **emergente + ciclo de vida** (PRs #128–#162, todo
> documentado): capacidad=hechizo con gate de **doble vía** (anatomía ∨ `Grimoire`), **temperamento histórico**
> (confianza-por-uso), sentidos/armamento, **navegación por rastros** (`TraceField`/stigmergy), y **ciclo de vida
> completo** (reproducción runtime, celo, enfermedad, **inanición→muerte**). Los flags **`AiBrain.useVolition` y
> `Reproduction.Enabled` están ON** (#159) → ecosistema auto-sostenible; reversibles poniéndolos en false.
> **Testing:** `TestProbe`/`TestRunner` (grep `[TEST]`), 8 sandboxes en `TestRunner_AUTO`, misión-observación
> `EcosystemObservation` (`[ECO]`) + misión WASD. Diseño vivo en `docs/capabilities-and-embodiment.md`,
> `docs/volition-selection-engine.md`, `docs/environmental-navigation.md`. **Validación en Unity:** el compañero
> confirmó compilación limpia + 0 regresiones hasta #148. **2026-10-12: master hasta #217 (7eb29ec) COMPILA con 0 errores**
> (`tools/verify-compile.sh`, Unity 6000.5.1f1: runtime 0 err/43 warnings, editor 0 err/3 warnings). Falta **probar en
> escena** (regresiones) lo de #149 en adelante. En el proyecto Unity sobran 3 `.cs` fuera de git: `Assets/Editor/HeadlessLive.cs`
> y `Assets/Scripts/ChecklistAuto.cs` (commits locales sin pushear) y `Assets/Scripts/Microcosmos/WalkToGoal.cs` (nunca en git).
> **Siguiente sugerido:** Enfermería (curar enfermos/inanición vía `SicknessState.Heal`), N2/N4 (que el saciado gravite
> a lugares), o status del jugador (atacado/huida) en el HUD.
>
> *(Arco previo cerrado — magia/metabolismo/descomposición #45–#61: sandboxes `Descomposicion_AUTO`/`Magia_AUTO`,
> guion en [`testing-checklist.md` §19](testing-checklist.md).)*
>
> **HECHO 2026-10-12 (PRs #222–#231, pendiente verify-compile del compi):** habilidades/enseñanza como hechizos
> (`CleanSpell`/`TeachSpell`); **migración postnatal→deseos** (`nurse`/`homebound` + flag `delegateMovementToVolition`);
> **escenarios dedicados** (apareamiento/caza/1v1/pack); **flora/fauna de hielo** (`IceCarnivorousTree`+`SnowBee`);
> superhechizos clínicos (`análisis`/`vacunar`); **stock SIN tope + equilibrio** (varios obreros por producto);
> **UIs por hechizo** (`status`/`open`/`close`; `AnimaStatusHUD` on-demand); **modelos corporales de CUBOS** por anyma
> (`CubeBody`, con partes para asanas) + escala real del restaurante; **Linduarda** guardiana de salud emergente +
> **guion emergente** documentado. Detalle en [`sanctuary1-level-and-live-tests.md`](sanctuary1-level-and-live-tests.md).
> *Falta para Nivel 1 completo:* guion/triggers de enfermería→micro→yoga→cocina→mundo abierto, gating de misiones
> (diarias/prioridad/equipo/automisiones/muestras/salto-temporal), guardería.
>
> **HECHO 2026-10-12 (PRs #234–#239, pendiente verify-compile del compi):** **inventario como hechizo**
> (`StockShelf`+`StockSpell`, con rango/aprendible); **grimorio con autocompletado** (sugerencias difusas + plantillas)
> y `status({visible,seconds})` que muestra Y oculta con el mismo hechizo; **formas por especie** en `CubeBody`
> (`Differentiate`: oso voluminoso/orejas redondas, lobo puntiagudo, conejo con orejotas, ciervo/suricata); **onboarding
> al santuario** (`SanctuaryOnboarding`: apto tras análisis+vacuna+**3 misiones** → Linduarda certifica → la Magnate
> **presenta** al nuevo y le enseña upa-yoga); **"todo es superhechizo"** (`ActionSteps`/`ActionSpells`: caminar/trepar
> componen el paso compartido **`levantarse`** = `GetUpStep`, heredable; `Anima.prone`); **yoga live-test**
> (`YogaTest`: la postura rota el rig, Saludo al Sol = 12 asanas); **escala precisa de la cocina** (huevo 5 cm, olla 22 cm).
>
> **HECHO 2026-10-12 (PRs #241–#242, pendiente verify-compile del compi):** **escenarios por especie**
> (`BuildSpeciesScenarios`: oso-pesca, foca↔orca, colonia pingüino+krill, ballena-krill, zorro↔conejo, ciervo↔lobo,
> malamute; corrige `PolarBear`→`Bear` en población/interacciones); **la Magnate GUÍA al nuevo integrante**
> (`LeadToYogaThenKitchen`: camina a `SalaYoga` → Saludo al Sol → `CocinaSpot`), cerrando la cadena
> apto→presentar→yoga→cocina. *Falta (escenario-guion grande):* elenco del Microcosmos Nivel 1 (Sakshi/Ambrosio/…)
> en posiciones+status+pensamientos (otra escena, `MicrocosmosSceneBuilder`; ver `microcosmos-level1.md`).
>
> **HECHO 2026-10-16 (PRs #244–#248, pendiente verify-compile del compi) — YOGA a fondo:** sin bucle de la asana
> (arbitraje por hueso `CreatureRig.LockPose`: yoga manda sobre emoción/andar) + **timing por RESPIRACIONES** (fase de
> 8 s; Saludo al Sol = 1 fase llegar + 2 mantener = 24 s/postura); **asanas más exactas** (brazos arriba paralelos por
> flexión, Ashtanga cadera alta) + **ficha anatómica** [`yoga-asanas-spec.md`](yoga-asanas-spec.md); **3 prácticas**:
> `UpaYoga.NeckSequence` (6 ejercicios de cuello/hombros, respiración silenciosa; + articulación nueva `ScapulaLeft/Right`),
> `YogaNamaskar.Sequence` (palmas juntas, Ujjayi, Balasana), `SunSalutation.Sequence`; **Magnate enseña por etapas**
> (upa-yoga cuello→namaskar→saludo); **stance por postura** (`AsanaPoseSpell.stanceWidth`, talones juntos) + **alcance
> IK** (`TwoBoneIK` + `reaches`: palmas juntas/dedos al suelo, opt-in).
>
> **HECHO 2026-10-16 (PRs #249–#255, pendiente verify-compile del compi) — YOGA + NIVEL 1:** **escena por sesión**
> (`YogaLibrary` + `YogaSessionRunner` + `Yoga_UpaCuello/Namaskar/SaludoSol`, hoy `Prueba_YogaSesiones`: una por esterilla en la sala de yoga, docs/escenas-de-prueba.md); **Ashtanga Serie Primaria** (núcleo +
> [`ashtanga-primary-series.md`](ashtanga-primary-series.md) + `Yoga_Ashtanga1`). **CABLEADO Nivel 1 (plan completo):**
> **(1)** enseñanza interactiva `TeachingSession` (el maestro pide y ESPERA al alumno; `LessonPrompt`) → la Magnate la usa;
> **(2)** hechizos **enseñables/aprendibles con requisitos** (`SpellCatalog` prerequisitos+items, `Grimoire.CanLearn`/`knowsAll`,
> `TeachSpell` gateado, recetas con `RequiredItems`); **(3)** `FollowSpell` + **Linduarda iniciación** (curar→«sígueme»→
> máquina de virtualización→Micro); **(4)** **Goluis jefe de cocina** (`KitchenBoss`: sígueme→limpiar→recetas); **(5)**
> **salto temporal al dormir** (cuerpo del jugador duerme→amanece, `SleepSpell`+`SetHour`). *Falta:* spawnear al jugador
> en la enfermería como paciente de Linduarda; migrar limpieza/recetas de Goluis a `TeachingSession` interactiva; afinar
> ángulos/IK en Unity; sesiones de yoga futuras (Ashtanga 2–4, Shambhavi, brazos).

## Pueblos desde su mapa, reglas de los microcosmos y Pakal de Palenque (2026-11-03, rama `claude/edif-pueblos`)
Docs: [`microcosmos-pueblos-mapas.md`](microcosmos-pueblos-mapas.md), [`microcosmos-reglas-edificacion.md`](microcosmos-reglas-edificacion.md) (reglas 8–12),
[`microcosmos-areas.md`](microcosmos-areas.md#reglas-de-los-microcosmos-2026-11-03) (reglas de los microcosmos y tabla de pisos),
[`microcosmos-actividades.md`](microcosmos-actividades.md), [`microcosmos-maya-pakal.md`](microcosmos-maya-pakal.md),
[`construcciones-elevadas.md`](construcciones-elevadas.md#las-dos-formas-de-cada-piso-2026-11-03), [`microcosmos-bruce-lee.md`](microcosmos-bruce-lee.md) §6.
- [x] **Pueblos sobre su plano real** (24 planos: calles, río, lago, colinas, montes, campos, murallas, hitos), barrio ×60 (1,4–2,8 km), ≤ 150 edificios.
- [x] **Edificios del pueblo** (plaza, mercado, escuela, estación, comisaría, bomberos, museo, guardería, baños, hotel, carpintería, obra, cuartel, centro de tecnología, parque, monumento, pileta, edificios de varias plantas en terraza) con su nombre de época y lugar (`MicroStyles.CivicName`).
- [x] **Etiquetas de actividad** (trabajo / descanso / recuperación / ocio, capacidad, interacción, cuerpos) en cada `AreaFunctionSite` — gancho para el horario semanal.
- [x] **Reglas 8–12** en `MicroBuildRules.ValidateSites` (actividad, adaptado a personas e insectos-humanos, estilo del catálogo, frutal en cada residencia, terrazas sin techo).
- [x] **Reglas de los microcosmos** (`MicroFloors`: piso, montado encima, forma finalizada, estructuras pobladas con miradores, pueblo entero, pendiente de piso representativo) + **9 pisos que faltan** (menú «Pisos que faltan»).
- [x] **Dos formas de cada piso** (`FloorForms`, `EnsureFloorForms`): 17 pisos (2 en obras, 15 con su forma en obra oculta). Bruce Lee, verificado: antes 3 conos de obra en el Dojo gigante; ahora forma finalizada.
- [x] **Frutales** de época y lugar en todas las residencias (19 estilos).
- [x] **Pakal de Palenque** (`Microcosmos_Pakal`, `PakalStoryTest` grupo 66; región `Maya`, estilo `maya_clasico`, bioma, plano de Lakamha').
- [x] **Microcosmos de los famosos** (2026-11-04, [`microcosmos-famosos.md`](microcosmos-famosos.md); rama `claude/edif-famosos`): Sadhguru (Velliangiri 1999, Sala de Yoga), Elon Musk (Hawthorne 2008, Mecánica, pendiente), Goku (aldea de la montaña, homenaje de ficción, Dojo), Bill Gates (Albuquerque 1975, Mecánica, pendiente), Warren Buffett (Omaha 1983, Estudio Textil, pendiente), MacKenzie Scott (Prairie View 2020, Química, pendiente) e Ibrahim Traoré (Uagadugú 2024, acto civil, Huerto). Código, plano, bioma, estilos (`india_moderna`, `sahel_moderno`, `norteamerica_interior`), menú por escena y `FamososStoryTest` (grupo 69). Los pisos que faltan pasan de 9 a 13.
- [ ] **Famosos: verificar en vivo** (lo hace el probador): reconstruir de una en una las 7 escenas (menú «Build Microcosmos de famosos/…») y la Sala de Eras; grupo 69; fotos. Repasar los [verificar] del doc (direcciones de MITS y del motel, cifras de la compra de la Nebraska Furniture Mart, día exacto del Dhyanalinga).
- [x] **Microcosmos de la música** (2026-11-05, [`microcosmos-musica.md`](microcosmos-musica.md); rama `claude/edif-musica`; solo código): Shakira (La Playa, Barranquilla, 2009), Bad Bunny (Vega Baja, 2016), John Lennon y Yoko Ono (Montreal, 1969), los Jackson (Gary, 1966), BLACKPINK (Seúl, 2016), Bob Marley y los Wailers (Kingston, 1978), U2 (Belfast, 1998), Public Enemy (Long Island, 1989) y «Grandes aportes de las canciones» (lugar inventado, seis canciones). Cada una con historia de 8 pasos y fuentes, pueblo con plano y las cuatro categorías, **mapa a mano Hecho**, ninguna letra ni logotipo. Prueba `MusicaStoryTest` (grupo 86). Su piso: **Música** (ver abajo).
- [ ] **Música: verificar en vivo** (lo hace el probador): reconstruir de una en una las 9 escenas (orden en el doc), «Build Sala de Eras» y pasar 86, 62, 43, 63, 87, 68 y 76; fotos del doc.
- [ ] **Música: contrastar** lo marcado [verificar] en el doc (flora y fauna, calles de los planos, algunos datos biográficos) y que Berón decida sobre Michael Jackson niño (doc, «Michael Jackson: cómo se trata»).
- [x] **Piso de Música** (2026-11-05, [`piso-musica.md`](piso-musica.md); rama `claude/edif-musica`; solo código): el área `Music_Area` en la torre del Entrenamiento, encima del Aula (hoy 3 pisos: 2 escenarios, 4 estudios y 8 salas de ensayo en doble cristal con tapa), con misiones `son_*`, pasos con «E», uniforme y concierto en el horario libre. 8 microcosmos re-apuntados a él, Viena incluida. Cada historia de la música nombra su momento altruista, bello o de superación.
- [x] **Música: resuelto** (Berón 2026-11-05): tapas de cristal permitidas en S6, rampas en los escenarios, puertas correderas (`SoundDoor`), huecos de cristal de 2 m en todo el santuario y S4 por cálculo ([`piso-musica.md`](piso-musica.md)).
- [ ] **Piso de Música: verificar en vivo** (lo hace el probador; grupos en el doc) y, al unir `claude/edif-energia`, añadir `SanctuaryAreaType.Music` a los 18 W/m² de `EnergyRule.WattsPerM2`.
- [x] **Pisos nuevos (2026-11-04, rama `claude/edif-pisos`)**: **Informática** ([`piso-informatica.md`](piso-informatica.md): clúster de IA local, refrigeración líquida en circuito cerrado, calor a la red de distrito, SAI; `AiCluster`; misiones `ifo_*`) en el piso libre de Ciencias, y el **Aula** ([`piso-aula.md`](piso-aula.md): clases y sala de estudio, plazas para todos los miembros, pisos según la lista del personal; misiones `aul_*`) en el piso libre del Entrenamiento. **Regla de capacidad** (manual S13, `AreaCapacity`, grupo 75) y **alerta de piso nuevo** (manual §11, `MicroFloors.AlertFloors`, grupo 76; gancho ampliado): Gates → Informática y Scott → Aula. Pruebas 74, 75 y 76.
- [ ] **Pisos nuevos: verificar en vivo** (probador): antes, «Build escenas de virtualización (una por misión)» (las 5 misiones nuevas tienen ya sus pasos y anclas); reconstruir SampleScene (Blockout solo santuario) y después Gates y Scott; grupos 74, 75, 76, 61 (registro con las 5 misiones nuevas), 28, 69; fotos. Ver los `[Capacidad]` que fallen (gimnasio y yoga frente a 18 miembros: si no caben, otro piso, regla S13) y construir el **ring de almohadas**.
- [x] Menfis, Alejandría, Bagdad, Crotona y Sils Maria re-apuntados al Aula y Hawthorne (Musk) al control de misión de la lanzadera (2026-11-04, la alerta de piso nuevo). Reconstruirlos de uno en uno; grupos 69 y 76.
- [x] S13 = J16 (manual): un solo validador, `AreaCapacity` (también el ring de almohadas y `CheckActivities` para el catálogo del horario libre).
- [x] **Verificado en vivo** (2026-11-03): grupos 43 (365/0), 60 (48/0), 62 (373/0), 63 (16/0), 66 (23/0); 0 violaciones de las reglas 1–12 y de las de los microcosmos en los 23 pueblos; fotos en `Temp/Pueblos/`.
- [x] `Microcosmos_Egina` con su ciudad (`CivicBuilder.EginaTown`; el templo del mito y la stoa, marcados como historia).
- [ ] Crear en el santuario los **pisos representativos pendientes** (biblioteca/observatorio, campo de deportes, aula de música, sala de asambleas, taller de electricidad, sendero de montaña, templo de los antepasados).
- [ ] Que el **horario semanal** (otro hilo) use `AreaFunctionSite.activity` para mandar a cada uno a sus edificios de ocio, descanso y recuperación.
- [ ] Palenque sobre el Huerto (24 × 9 m) queda a escala 1:2,5: con su piso propio (más ancho) iría a 1:1.
- [x] **El piso no se rompe** (2026-11-03, 2.ª vuelta): sin despeje del barrio ni apartar muebles; el pueblo los rodea o sube a sus mesetas (barrio alto) trepando (NavMeshLink + Climbable); `MicroFloor.floorPieces` en `MicroFloors.Validate` y `BruceLeeStoryTest`.
- [x] **Relieve real low-poly** (`CivicBuilder.Terrain`): montes y colinas con ruido, roca y nieve; lagos; ríos con riberas; acantilados (6 pueblos).
- [x] **Frutales en las residencias del santuario**: terraza frutal en cada casa de árbol (`ResidentialBuilder`), en la ronda del jardinero; `FruitOrchardTest` (55) lo mira.
- [ ] **Verificar en vivo la 2.ª vuelta** (lo hace el probador): reconstruir SampleScene y todos los microcosmos; grupos 43, 55, 60, 62, 63, 66; fotos. Probador (2026-11-04): 829/6; arreglado en código: el banco de la casa redonda del Huerto a 3,5 m (1,93 m del hogar) y `Layout` con la caja girada y 10 cm de holgura; la prueba de la flora ya no cuenta como solar el de un edificio del barrio alto sobre un mueble (3 plantas de Bruce Lee bajo el tablero). Falta repasar 43 y 62.
- [x] **Entrenamiento de cada época y las cuatro categorías** (2026-11-04, regla 16, manual M35): el entrenamiento es ahora categoría propia y tiene su clase estable (`AreaFunctionSite.training`, `MicroStyles.TrainingKind`), con aparatos nativos (`MicroFurniture.Training`, funciones `ent_*`). Cada hueco del plano histórico se rellena con una posición de un mapa actual, marcada en el plano (`| actual: fuente`): entrenamiento en 10 pueblos, recuperación en 3 y ocio en la Tregua. `MicroFloors.ValidateCategories` lo comprueba y `CivilizedErasTest` (62) también. Docs: [`microcosmos-entrenamiento-eras.md`](microcosmos-entrenamiento-eras.md), regla 16, [`microcosmos-pueblos-mapas.md`](microcosmos-pueblos-mapas.md) §Huecos, [`gimnasio-maquinas.md`](gimnasio-maquinas.md) §Microcosmos y [`microcosmos-actividades.md`](microcosmos-actividades.md).
- [ ] **Verificar en vivo la regla 16** (lo hace el probador): reconstruir los microcosmos de historias, de compasión, de la Planta Atómica, Egina y los famosos; grupos 62 y 43.
- [ ] Afinar con OpenStreetMap las posiciones «aproximadas» del mapa actual (Berkeley, Angostura, Tregua, Planta Atómica, Varsovia, Le Chambon, Uagadugú, Dayton) cuando se haga el mapa a mano de cada historia (M9).
- [ ] El hilo «entreno»: planes de entrenamiento de los insectos-humanos por clase de lugar (`training`), según [`gimnasio-maquinas.md`](gimnasio-maquinas.md) §Microcosmos.

## Microcosmos — eras/especies + observación (diseño en [`microcosmos-eras-and-observation.md`](microcosmos-eras-and-observation.md))
- [ ] **Fauna de la era del fuego** (a escala insecto, sabana Pleistoceno): arquetipos de alma +
      clases/config de **hormiga cosechadora, termita cosechadora, escarabajo pelotero, hormiga león,
      mantis, grillo, saltamontes, áfido, colémbolo, araña**; + skin **suricata** (render de la máquina).
- [ ] **Dos generators** en builders: familias (`FamilyGenerator`: hormiga/termita) + individuos
      (`Generator`: araña/mantis/hormiga león) — en `SampleSceneBuilder` **y** `MicrocosmosSceneBuilder`.
- [ ] **`scena2Enfermería1`**: nivel con **cronómetro** de limpieza de filamentos/orificios de maquinaria
      (Kushal insecto) — reusar `DirtArea`/`Cleaner` + `VirtualizationMachine` + `WorldExitPortal`.
- [ ] **`scena3Enfermería1`**: misión **probar-alimentos** (sobre identificar/clasificar del
      `DecompositionMinigame`) + skin **suricata** (fallo máquina) + un consumible **adictivo**
      (`AddictionState`/`NarcoticSpell`) + uno **venenoso** (`SicknessState`→enfermar/**morir**→`GriefSpell`).
- [ ] **Eje C de la máquina de avatares** (traducción/skin de especie; fallo que espeja la sociología)
      — enchufar a `EmotionReader`/`BodyPartReactor`. Ver `magic-plane-and-meditation.md` Eje C.
- [ ] **Kushal conector**: un bloque `FollowingArrays` visible con menú cerrado = **pensamiento actual**
      de Kushal (fuente `Mind`/`PhraseLibrary`), persistente ~10 min; + **área de automisiones** (un bloque
      por misión, como la tabla periódica).
- [ ] **Linduarda** (doctora, **hechicera nivel 4/S4**): clase `Companion` + rol de puerta al Micro. Ficha:
      [`character-linduarda.md`](character-linduarda.md).

## Nivel de los peloteros + mecánicas nuevas (diseño en [`microcosmos-dungbeetle-level.md`](microcosmos-dungbeetle-level.md))
Orden sugerido; cada una con su veredicto de estado en el doc (EXISTE/PARCIAL/FALTA):
- [x] **UI de pensamiento del ser controlado** (`AnimaThoughtHUD`, OnGUI, ~10 min) — HECHO 2026-09-23.
      `Mind` expone `lastThought`/`lastIntensity`/`lastPositive`. *Siguiente: versión FollowingArrays.*
- [x] **Control por necesidad + idle + adrenalina** (`AcuteStressResponse`) — HECHO 2026-09-23:
      apremio→relevancia IA→toma de mando; `Anima.physicalBoost` (química) + `Anima.mindStatusCalculationFog`
      (Yerkes-Dodson), enchufada a `SenseThreats`. *Pendiente: veto de acciones contrarias + niebla en más cálculos.*
- [x] **Guard del Animator** (`ActionPrep.Prep` con `HasState`) — HECHO: una especie sin modelo ya no revienta.
- [x] **Campo de pensamientos ← entorno + necesidades** (`Mind.SenseSurroundings`) — HECHO 2026-09-27:
      percibe animas cercanas (amiga/depredador/neutra) + hambre/sueño/estrés → tiñe humores → tono/valencia.
- [x] **Control por necesidad en TODAS las animas** — HECHO: `AcuteStressResponse` auto-añadido en `Animal.Init`.
      *(Companions/PlayerStats: añadir a mano si se quiere el mismo efecto.)*
- [x] **Necesidad de defecar** (`DefecationNeed`, fibra→tránsito→excremento+olor) — HECHO (opt-in). *Falta:*
      auto-añadir + prefab de excremento (Unity) + que sea la acción natural del SpawnSpell.
- [x] **Defecación reprimible** (peligro/sueño/control mental: `aware`/`alertness`/`asleep` + disciplina+compostura
      = continencia) — HECHO 2026-10-06. Aguantar sube el estrés → compite en el campo de pensamientos.
- [x] **Control-por-necesidad también en animas COMPUESTAS** (`AcuteStressResponse` en `Cast()`) — HECHO.
      *(Jugador: decisión aparte — el "body" le quitaría el mando bajo estrés extremo, que es el diseño.)*
- [x] **Restaurante JUGABLE** (`RestaurantKitchen`/`CookingStation`/`CookingRecipe`): cocinar para todos →
      stock equilibrado (multi-anima) → jugador avanza con E → XP + porción al inventario — HECHO 2026-10-06.
      *Falta:* estaciones manipulables reales (Virtualization `StationPart`, prefabs Unity).

## Hechizos por teclado + colas (diseño en [`typed-spells-and-queues.md`](typed-spells-and-queues.md))
- [x] **Etiquetas de nombre** (`AnimaNameplate`) — HECHO 2026-10-10.
- [x] **Consola de hechizos por teclado** (`SpellConsole`) + `walkTo` (destino anima-visible/área; "¿Dónde está X?") — HECHO.
- [x] **Cola de acciones 2-carriles** (`ActionQueue`: `own` prioritaria + `player` editable) + `QueuedSpell`/`WalkToSpell` — HECHO.
- [x] **`AcuteStressResponse` siempre activo y universal** (fauna+compuestas+jugador) — HECHO (no es switch).
- [x] **Cocinar por teclado** (base): restaurante físico con primitivas etiquetadas (`BuildRestaurantRoom`) +
      `WorldLabel` en nameplates + consola `step`/`cook` + `walkTo({destiny:"Nevera"})` — HECHO 2026-10-10.
      *Falta:* selector de reemplazo de primitivas (take/putIn/beat HECHOS).
- [x] **Platillos** yogur/kéfir + bowl quinoa+lentejas + menestra — HECHO (presets + sandbox, ~7 pasos c/u).
- [x] **`AnymaSpec` + `AnymaFactory`** (spec opcional → random 0.8..2; `CreateMany`) + `StatPhysique` (apariencia por
      stats) + **`BondSpell`/`ThoughtSpell`** (modificación de vínculo/mente; stats ya=`TransformationSpell`) — HECHO.
- [x] **22 platillos** (6 base + 16 bomba por país, `CookingRecipe.All`) + **cierre nocturno** (`AreaHours`+`IsDay`) — HECHO 2026-10-10.
- [x] **Guía de pruebas para Unity** ([`testing-recent-features.md`](testing-recent-features.md)) — HECHO (montaje + tests por feature).
- [x] **Hechizos de cocina por PASO** (`take`/`putIn`/`beat` ligados a estación exacta, con RANGO = alcance del miembro; a distancia irreal con magia) — HECHO 2026-10-12 (`Assets/Scripts/Kitchen/KitchenStepSpells.cs`: manos `KitchenHands`, recipientes `KitchenVessel`, avanzan la receta si casan con el verbo del paso).
- [ ] **Selector de reemplazo de primitivas** del restaurante (cambiar los cubos por prefabs).
- [x] **Magnate** (1ª rebanada): estado infinito + sin necesidades + imposeíble de poseer + **pacifica** + va al
      **rezagado** y le **enseña yoga** (Grimoire+Saludo al Sol+XP) — HECHO 2026-10-12 (`Assets/Scripts/World/Magnate.cs`).
      *Falta:* marca por anima, bond alto por defecto (Anima→ITarget), llevar a enfermería, guardia nocturna, clon por sanctuary.
- [x] **Cocina con RANGO** (`use`/`take` falla si no alcanzas la estación) — HECHO (1er paso de "steps=hechizos con alcance").
- [x] **Insectos neutros de ambiente** (factory en Micro Scene0; se saltan sin .fbx) — HECHO.
- [x] **`SuperSpell`** (suprahechizo = cola de sub-hechizos; `IsSatisfied`=check, `Destabilize(power)`=equilibrio) — HECHO 2026-10-12.
- [x] **Cuenta de pensamientos** (`Mind.CountOf`/`Reinforce`/`Erode`/`ShareOf`; selección por cuenta; expresar refuerza) — HECHO.
- [x] **`PacifySpell`** (quita humores negativos, sube positivos) + la Magnate lo usa — HECHO.
- [x] **Independencia de modelos 3D** (`AnymaFactory.CreateCubeAnima`: prefab vacío → cubo) — HECHO.
- [x] **Volición por la cola** — HECHO 2026-10-12: `Volition` encola un `DesireSpell` (envuelve el deseo elegido) en la
      `ActionQueue` (carril opcional) en vez de despachar directo; `useQueue` (fallback directo si no hay cola). El
      carril `own` del cuerpo (`AcuteStressResponse`) tiene prioridad → un apremio pausa la volición solo.
- [x] **Deseos = SUPRAHECHIZOS** — HECHO 2026-10-12: `Desire.Conjure()` devuelve un `SuperSpell` (por defecto 1 paso =
      `DispatchStep` de la acción actual; descomponible vía `StepsBuilder`). `eat` ya tiene 2 pasos (analizar→comer).
      La `Volition` encola el `Conjure` del deseo elegido. *Siguiente:* descomponer el resto (acercarse/levantarse/pedir-ayuda) + usar thoughtCounts.
- [x] **MIGRACIÓN postnatal→deseos (1ª rebanada)** — HECHO 2026-10-12: deseo **`nurse`** (padre alimenta cría
      `lifeStage='c'` vinculada+hambrienta) compite en la arena y corre por la cola. *Falta:* retirar los drives
      directos de `PostNatalManager` (guardCub/fetchCub/cub-side) — el calendario de etapas se queda. Ver
      [`sanctuary1-level-and-live-tests.md`](sanctuary1-level-and-live-tests.md) §3.
- [x] **Prueba en vivo de vida familiar** (`BuildFamilyLifeTest`: lobos+carne / conejos+pasto, aislados) — HECHO.
- [ ] **Escenarios en vivo que faltan**: apareamiento · caza · 1v1 · pack-vs-1 · pack-vs-pack (patrón aislado+comida). §2.
- [ ] **Nivel 1 jugable — cablear el onboarding** (enfermería→análisisClínico/vacunar→micro→limpieza+automisiones→
      muestras→yoga→comer→paseo nocturno/sueño→día2 cocina→mundo abierto→guardería). Lo existente vs lo que falta:
      [`sanctuary1-level-and-live-tests.md`](sanctuary1-level-and-live-tests.md) §4.
- [ ] **Área de Guardián / torre de vigía** (puesto de misiones: patrullar/alimentar/llevar a enfermería/enseñar) — no existe; ver spells-suprahechizo §5b.
- [x] **Prefabs-cubo por defecto** en `AnimalPrefabGenerator` cuando falta el `.fbx` — HECHO 2026-10-12: `Generate
      Animal Prefabs` crea un prefab-CUBO (misma pipeline de componentes) para las especies sin modelo → las 21 tienen prefab.
- [x] **Yoga: Saludo al Sol** (`PostureSpell`/`AsanaPoseSpell`/`SunSalutation` 12 asanas + consola `yoga`; "no tengo X") — HECHO 2026-10-10.
- [x] **`own`-lane** (`AcuteStressResponse`→`BodyReflexSpell` bloquea al jugador en toma de mando) — HECHO.
- [x] **Carril opcional** (`IdleBehavior`→`WanderSpell`, auto en animas compuestas) — HECHO.
- [x] **`bond`/`thought` por nombre en cualquier parte** (amistad remota) + `amount` — HECHO.
- [ ] **Enrutar la Volición de la fauna por la cola** (NPCs "escriben" cada acción) — refactor mayor pendiente.
- [ ] **UI de colas del poseído** + cast time NPC (~40 ppm).
- [ ] **Factory en niveles del Microcosmos**: poblar con insectos neutros de ambiente (+ sonidos, necesita audio en Unity).

## Anyma factory + control (diseño en [`anyma-factory-and-control.md`](anyma-factory-and-control.md))
- [x] **`AnymaSpec` + `AnymaFactory`** (props opcionales → random 0.8..2) + `CreateMany` — HECHO (falta: roles compuestos + sexualidad/pensamientos random; asignar prefabs en Unity).
- [x] Hechizos de **modificación de vínculos (`BondSpell`) y mente (`ThoughtSpell`)** — HECHO (stats ya: `TransformationSpell`).
- [x] **Apariencia por stats** (`StatPhysique`, auto en `Animal.Init`) — HECHO.

## Génesis de hechizos + progresión (diseño en [`spell-genesis-and-sanctuary-progression.md`](spell-genesis-and-sanctuary-progression.md))
- [ ] **`SpellLadder`** (escalera de intensidad por array + hechizos compuestos + `requires`) → caminar→correr→teletransporte.
- [ ] **`EatSpell : SpellBase`** (comer = descomponer anima por intensidad/stats: platillo/compuestos/elementos/quarks;
      afecta gramos del objetivo + llena pools). Envuelve `Forager`/`Metabolism`/`DecompositionMinigame`.
- [ ] **Misiones → XP de margas** (`CharacterLevel.GainXp` desde cocina/enfermería/yoga/vínculos) → subir nivel
      integral → capacidad de pools → **evento de cambio de santuario** al reunir puntos.
- [ ] **Restaurante + planta atómica en todos los santuarios**; misiones gateadas por hechizos del jugador;
      "cuanto más preparada, más provecho" (purificar toxinas cocinando). Historia de área (planta/restaurante).
- [x] **`CommunicationSpell`** (emparejar dos animas cercanas → alternar pensamientos + contagio emocional ×vínculo
      + crece amistad) — HECHO 2026-10-06. `Mind.SpeakNow()`. *Siguiente: frases de respuesta; turnos largos.*
- [ ] **¿Push-ups/abs?** decidir si se añaden como acciones/misiones o el físico sale del trabajo/farming.
- [ ] **`SpawnSpell : SpellBase`** (generar prefab en un lugar; `duration`/timing ya existen) + **sueño
      por agotamiento** (`exhaustion` alto → dormir).
- [ ] **Evento `livetogether`** (bond>umbral → comparar `bond_hogar − peligro`; gate por bond mutuo →
      unificar `HomeOrigin`) = semilla de familias/tribus.
- [ ] **`sleepDistribution`** (turnos de vigilia en el pack) + llevar al miembro al `HomeOrigin` más
      seguro del pack (extiende PostNatal).
- [ ] **`StinkSpell`/reduce-comestibilidad** (crías apestosas por edad) + **depredador de miedo
      diferencial** (aterra al pelotero, no a Kushal; `Archetypes._relations` + veneno/hipnosis).
- [ ] **`AnticipationGrowth`** (el deseo intenso hace crecer stats; patrón `ObservationSkill.Train`/`Confidence`).
- [ ] **Modelos/escena** del pelotero (con su bola) + crías — ver [`unity-editor-manual.md`](unity-editor-manual.md).

## Decisiones abiertas (rápidas)
- [~] **Microcosmos = mundo de INSECTOS (DECIDIDO opción B)** — [`microcosmos-insects.md`](microcosmos-insects.md).
      Históricos encarnados como insectos (violencia = "volverse salvaje", sin trauma humano). **Hormigas**
      civilización primaria; **cría = mirmecofilia** (pulgón = ganado/mascota); su "fuego" = **feromonas**;
      **hongos** (Physarum oráculo, Ophiocordyceps amenaza); abeja/avispa/termita = otras ciudades. **Pipeline:
      área humana (Meso) → transformar a insecto (Micro).** **1ª misión hecha (scaffold)** `MicrocosmosSandbox_AUTO`
      (amanecer: **cueva** natural, sin reina/hormiguero/feromonas; pulgón-mamá guía + banda de 7 hormigas).
      **Hecho (PR #62):** `WeaknessEffect` (hormigas viejas), `HoneydewSpell` (maleza Ambrosio = primer consumible),
      `PullSpell` (Jalar, primer hechizo), `FormicAcidSpray` (defensa Kushal), `SpellBase` (base de hechizos),
      `CombatTargetSelector` generalizado a `ITarget`.
      **Falta:** jugador-avatar guía/carga; sandbox escena Nivel 1 (suelo de bosque + cueva checkpoint + zonas de depredadores);
      colonia real; feromonas como mecánica; dispatch meso→micro; conectar `HoneydewSpell` al inventario.
- [x] **Aptitudes adicionales**: set cerrado — `endurance/reasoning/memory/creativity/sociability/discipline`
      añadidas a `CompanionBase`; `flexibility` → `BodyPartStats` (pendiente de conectar).
- [ ] **Economía circular** (aprobada): cerrar la tabla final residuo→subproducto→área
      (ver [`mission-mode.md`](mission-mode.md)).
- [x] **`Generator.cs`**: revisado 2026-07-23 — **se mantiene**. No es redundante con `FamilyGenerator`
      (uno genera familias, el otro es un spawner por área vía `IFactory`/`Animal.StaticGenerateSquareRange`);
      sin invocadores en código pero es una herramienta scene-wireable útil.
- [x] **Malamute: mascota** (decidido) — fuera de `nestSpecies`; sigue siendo presa potencial. Colocación
      como compañero: pendiente (parte del modelo de personajes/`NPCBase`).

## Aptitudes (creature-stats.md)
- [~] **Bucle de evolución**: *animales hecho* (agilidad←movimiento, percepción←alerta, en `Animal.Restore`
      con `AptitudeEvolution`). **Falta humanoides**: engancharla a tareas/misiones (físico→fuerza/resistencia,
      estudio→razón/memoria, variedad→adaptabilidad, sedentarismo→↓).
- [ ] Conectar aptitudes a mecánicas: `agility`→velocidad/maniobra, `perception`→detección+calidad
      de asana, `strength`→daño/carga, `bodyMass`→física/saciedad, `adaptability`→velocidad de aprendizaje.
- [ ] Unificar con `PlayerStats` (`observationRadius`↔`perception`, `velocity`↔`agility`) y
      `BodyPartStats` (`flex`/`str`). **Ya consolidado en `Anima`** (migración #14); queda mapear
      `observationRadius`/`velocity`↔aptitudes del todo.
- [ ] Modelar el rasgo mental de **Panterilia** (exageración de la realidad / influencia de terceros).
- [ ] Revisar `adaptability` para animales (hoy solo en companions).
- [ ] Conectar `flexibility` a la dimensión `flex` de `BodyPartStats` (se entrena con yoga; universal).
- [ ] **Modificadores de medio (resto)**: afinidades para humanoides (nadar) al llegar `NPCBase`;
      extender `MediumFactor` a `strength`/`endurance`. (Detector `MediumZone` ya hecho.)
- [x] **Ahogo/asfixia** (PR #139): `Animal.Suffocate()` en `Restore` — si `MediumFactor < asphyxiaThreshold` (0.3),
      daño progresivo `(umbral−MediumFactor)×asphyxiaRate×masa` vía `Hurt` (muerte por el mismo camino). Pez fuera del
      agua / ballena varada se ahogan; el terrestre en agua (afinidad 0.4) nada torpe pero NO se ahoga (umbral 0.3);
      los que pescan deben tener `waterAffinity ≥ umbral`. `CorrectMedium` intenta sacarlo antes → esto le da consecuencia.
- [x] **Lógica de agua/tierra**: detector (`MediumZone`) + comportamiento (`Animal.CorrectMedium`:
      acuáticos buscan agua, terrestres salen) + **asfixia** (PR #139). Evitación *proactiva* → parte del sistema
      mayor de **navegación por lectura del entorno** ([`environmental-navigation.md`](environmental-navigation.md),
      rebanada N2): el overworld navegará por **impulsos** leyendo el entorno (como las hormigas), no siguiendo aves.
      **N0 hecho (PR #140):** `Wander` deambula local, ya no hacia un ave al azar.
- [~] **Refinar selección de caza**: **hecho (PR #32)** `Predation` — `SelectPrey` ya no caza lo que no puede
      vencer (masa/fuerza/textura/tamaño) y `EvaluateThreat` escala por poder de stats (temer al más poderoso);
      el tamaño invierte presa↔depredador; el farol de transformación no engaña, la real sí. **Falta:** manada
      (masa aliada) y **aura/estatus mágico** del humano (requiere sistema de magia/maestría).
- [x] **Influencia de manada en la caza** (PR #33/#34): `Predation.EffectivePower` suma el poder de los
      **aliados** (misma facción) en radio, ponderado por el **`PackFactor` por especie** (Lobo 0.8/Malamute 0.9
      mucho; Oso 0.3/Zorro 0.2 poco; Conejo/Ciervo/Ballena 0 nada) → `SelectPrey` usa el poder de manada del
      cazador; `EvaluateThreat` compara poder efectivo de ambos. Dinámico y calibrado por especie.
- [~] **Aura/estatus mágico del humano** (PR #33): **mecanismo hecho** — `Anima.magicAura` (firmado, decae con
      `MagicAura`): − destructiva → más temido (`EvaluateThreat`); + benevolente → bonds fáciles (`GrowBond`).
      **Falta:** que el **sistema de magia** llame a `Register*` al lanzar hechizos.
- [~] **Modelo de personaje unificado** — `NPCBase` **descartado**; ya es la **clase única `Anima`**
      (migración #14). Falta: `ITarget` + población en `Anima` (para que companions/otros NPCs sean presa) y
      el **control aparte** (cerebro enchufable: input o IA) para que todo `Anima` sea jugable/intercambiable.
- [ ] **Territorios + poblaciones**: `Territory` trigger con `residents` (enter/exit, como `MediumZone`/
      `SanctuaryArea`) → escaneos locales (arregla el perf de `SenseThreats`/banco); barreras/conectores para estabilidad.
- [ ] **Montaje de escena** (`SampleSceneBuilder`): crear **nidos/madrigueras primero**, luego poblar
      familias en torno a ellos, con nidos **fuera del alcance de depredadores**. Ver [`refuge-and-adult-behavior.md`](refuge-and-adult-behavior.md).
- [x] **Dietas** revisadas (todos los carnívoros): oso +ciervo/zorro/malamute/**humano**; lobo +zorro/malamute/oso(manada)/humano; zorro/malamute sin cambios.
- [x] **Banco de peces como organismo** (hecho): `FishSchool` es entidad viva (deriva/huye/crece/autoregenera)
      + `ITarget`/`IEdible`; en dietas de oso/zorro; herbívoros marinos lo depletan al pastar. **Pendiente:
      robo/hurto del zorro** (llevarse comida ajena con `ICarrier`; liga con sigilo).
- [ ] **Zorro: robar comida** (hurto de `FoodItem`/presa ajena vía `ICarrier` en vez de cazar de frente); liga con sigilo.
- [ ] **Personajes/mascotas como presa**: `PlayerTarget` hoy = solo el jugador. Que companions/malamute-mascota sean `ITarget` con población objetivo (futuro, `NPCBase`).
- [~] **Comportamiento adulto / territorialidad**: hecho el evitar-depredadores (`SenseThreats`). Falta:
      idle enriquecido (explorar/jugar), separación *espacial* (montaje nidos-primero), refugio/ocultarse
      (árboles/arbustos) y memoria de lugares (solo documentado). Ver [`refuge-and-adult-behavior.md`](refuge-and-adult-behavior.md).

## Aclaraciones → siguiente paso (learning-unlocks.md)
- [ ] Crear un **registro de "aprendidos"** por jugador (elementos, posturas, habilidades) + evento
      `OnLearned(...)`.
- [ ] Hacer que **al aprender** se reconstruya la UI afectada al vuelo (Palette / Hologram / AbilityBar).
- [ ] Sustituir el gating por `unlockLevel` de `CombatAbility` por gating por **"aprendido"**.
- [ ] `CombatAbilityBar.RefreshBar()`: suscribirlo a `OnLearned` + `PeriodicTableManager.OnElementDiscovered`
      (arregla el "no refresco en vivo").
- [x] **Cocina**: resuelto 2026-07-23 — `KitchenEntrance`/`KitchenScaleController` (y su auto-trigger)
      **borrados**; la entrada es ahora una sola ruta vía `VirtualizationMachine` (Microcosmos).

## Modo misión / economía (mission-mode.md)
- [ ] Implementar `KitchenCombatManager`: arranca modo misión, activa mobs, bloquea salida del área,
      reporta a `MissionTracker` y suma a los contadores.
- [ ] **Sistema de contadores por área** + **conscription loop** (obligar a personajes ociosos cuando
      un contador baja del umbral).
- [ ] **Trigger-personaje**: componente en un NPC + prompt `ConfirmationPanel` + bloqueo/desbloqueo
      de la salida del `SanctuaryArea`.
- [ ] Implementar la economía circular (mobs → abono + subproductos → contadores de otras áreas).

## Huérfanos (gaps-vs-planteamiento.md §B) — cuando toque
- [ ] `TeacherNPC` → `Palette.OnFormulaEvaluated`; `Say()` → `DialogueManager.Play`.
- [ ] `BondActivityManager` → UI (`BuildPaletteConfig`/`Practice`); consumir `Goluis.resistanceBuilt`.
- [ ] `BlockSpellEvaluator` (desbloquea `MaterializationExecutor` + `ArrangementPattern`).
- [ ] `NPCCombatBehavior`, `NPCEconomy`/`AreaVendor`, `ClothingCraftingArea` (cerrar loop).

## Bugs abiertos (known-issues.md)
- [x] `ShipCtrl` `if(1==1)` (83/95) → pitch de cámara acotado (`minCamPitch`/`maxCamPitch`). (2026-10-12)
- [x] `PullDoor.OnCollissionEnter` (typo) → `OnCollisionEnter(Collision)`; la rotura por golpes (`lp`) ya corre. (2026-10-12)
- [x] `Respawn` off-by-one → `Random.Range(0, beasts.Length)` incluye `beasts[0]`. (2026-10-12)
- [ ] `BirdBehavior` altura sin clamp; `ActionPrep` `energyCost` sin límites.

## Mundo grande — los 3 planos (world-topology-and-planes.md)
Escalera de implementación propuesta (2026-07-23). Empezar por **A** (columna vertebral).
- [~] **A — Backbone de recursos + HUD (Mesocosmos).** `SanctuaryResource`/`SanctuaryResources` (ledger),
      `AreaProducer` (pasivo + bonus por personaje asignado), `SanctuaryResourceHUD` (prototipo OnGUI),
      cableado en `SampleSceneBuilder`. **Primera pasada hecha**; falta: asignación real de personajes
      (enganchar a `SanctuaryDirector`), regla de visibilidad multi-santuario en guerra, y sustituir el
      HUD prototipo por UI declarativa.
- [~] **B — Farming-como-juego (tensión).** **MVP + feel de combo hechos (2026-07-23):**
      `PlayableCreature` (tensión = `Anima.stress` o local) + `PlayController` (tecla V);
      al serenarse suelta recursos (`SanctuaryResources`) + monedas; `IInteractable` "dar comida y agua"
      → sacia (`fatReserves`) y descansa. **V2 feel gato/perro hecho:** excitación/combo (descarga
      escala con excitación), **atrapada** (quedarte pegado resetea el combo), reacción de la criatura
      (te mira/se acerca, con correa) + rebote de escala. Sandbox en `SampleSceneBuilder`.
      **V3 hecho (2026-07-23):** tabla de **drops** (`ItemDrop[]` → `Inventory.AddItem`, con consumible y
      artefacto placeholder vía `FarmingSandboxItems`), y **XP → leveling** (`CharacterLevel`: xp/nivel →
      +vida/+maná + vida actual/daño, mostrado en el HUD). **Refinamiento hecho:** el juego es
      **desbloqueable** (gateo `PlayUnlocked` = criada/relajada/vínculo; si no, ley natural) y **puede
      hacer daño** (pérdida de control al excitarse → `CharacterLevel.TakeDamage` si no esquivas).
      **Falta:** leer el vínculo real de `Anima.bonds` (hoy flags placeholder), conmutar de verdad
      a la depredación de `Animal` cuando no está desbloqueada, mecánica de **esquiva** propia,
      generalizar el target de combate (`CurrentTarget` es `IngredientMob`) y enganchar vida/maná al combate.
- [ ] **C — Teletransportador "aeropuerto".** `SanctuaryTeleporter` bidireccional entre santuarios
      (lava↔subterráneo primero); reusa `MobWorldLoader`.
- [ ] **D — Construcción en el tiempo.** `Construction`: progreso consumiendo recursos+tiempo (lento en
      Meso, rápido en Macro); 4 áreas + 1 en obra. Depende de A.
- [ ] **E — Macrocosmos (RTS).** Unidades/roles (peón/héroe), árbol de construcción, cámara 2D-ish,
      IA de guerra, agujero-negro de estructuras. Sistema grande; al final.
- [~] **Pools derivados de aptitudes** (creature-stats.md §Pools derivados). **Hecho (2026-07-24):**
      módulo `DerivedStats` (funciones puras aptitudes→vida/energía/maná/defensa/poder) + `Aptitudes`
      struct; `CharacterLevel` reescrito para derivar los pools (energía nueva, defensa aplicada al daño
      en `TakeDamage`, `SpendEnergy`), lee de cualquier `IAptitudes` (opt-in); HUD muestra energía/def/poder.
      **Trepar hecho (MVP, verificar feel/física):** `Climbable` + `PlayerClimber` (Espacio; altura ∝
      fuerza/peso, velocidad ∝ fuerza·agilidad, coste de energía ∝ peso/fuerza vía `SpendEnergy`).
      **Falta:** unificar del todo la fuente de aptitudes con `NPCBase`.
- [~] **Aptitudes universales** (2026-07-24): **hecho vía interfaz `IAptitudes`** (12 getters) que
      implementan `Anima` (ahora hogar de las 12 — agility/perception activas, resto latentes),
      `CompanionBase` y `PlayerStats` (mapeo parcial). `DerivedStats.From(IAptitudes)` y `CharacterLevel`
      (opt-in `deriveAptitudesFromComponent`) leen cualquier ser vivo uniforme.
- [x] **Migración a clase única `Anima`** — **PR #14 mergeada a master (2026-07-28)**; falta que TÚ compiles.
      Hecho (2026-07-28): (1) **`LivingEntity` renombrada a `Anima`** en todo el código (`Anima.cs`), 34 refs;
      (2) **`CompanionBase : Anima`** — quitadas las 12 aptitudes duplicadas + `stress` (heredados), 3 hooks
      abstractos implementados (stubs), `fatigue`/`mood` propios; (3) **`IMindSimple.cs` borrado** (sin uso);
      (4) **`MigrationDiagnostics`** que vuelca por consola la validación en Play (herencia, aptitudes por
      campo y vía `IAptitudes`, puntos del alma/margas del jugador); (5) **`PlayerStats : Anima`** hecho —
      `stress` y las 12 aptitudes heredadas (retirado el mapeo velocity→Agility; el jugador tiene aptitudes
      reales), `velocity`/`physicalResistance` quedan de movimiento/combate, 3 hooks abstractos (stubs).
      (6) **`WorldCharacter` consolidado (2026-07-28):** las drives `satisfaction/mentalFatigue/sleepiness/
      observationRadius/velocity/physicalResistance` se **movieron a `Anima`** (hogar único; `stress` ya
      estaba); `PlayerStats` las hereda (retiradas sus copias; `playerStats.X` sigue funcionando); y
      `WorldCharacter` lee/escribe el `Anima` del objeto (borrados sus lightweight + el bridge a
      PlayerStats). **Migración ESTRUCTURAL COMPLETA:** todo ser (jugador/compañeros/animales) es `Anima`
      con una sola fuente de stats. *(Nota: los NPC ahora arrancan con `physicalResistance=1` en vez de la
      antigua `strength=0` → puede requerir re-tunear `promotionStrength`.)* **Falta menor:** docs
      históricos que aún dicen "LivingEntity"; el pilar `Mind` (drives complejos) es trabajo aparte.
- [~] **Margas del alma = tracks INDEPENDIENTES** (creature-stats §Progresión). **Hecho (2026-07-24):**
      `SoulMarga` (pool XP+nivel+curva) y `CharacterLevel` con 3 margas (**Stats**/**Yoga**/**Vínculos**);
      los **puntos del alma escalan por TODAS las margas** (`SoulLevels`); **maná gateado por Yoga≥2**;
      HUD; **XP de Yoga cableada** (`MeditationReward.yogaXp` → `GainYogaXp` en misiones de yoga); **XP de
      Stats desde ganancia de aptitudes** (`GainAptitude` → `GainStatsXp`); **XP de Vínculos cableada**
      (`BondActivity.TryPractice` → `GainBondXp`); **base-bump al subir de nivel** (`OnMargaLevelUp` sube
      la base de todas las aptitudes vía `Aptitudes.AddAll`). **Falta:** XP de yoga por **práctica directa**
      (`AsanaQueue.OnLimitReached`, requiere compilación/orphans) y la futura marga de **Hechizos**.
- [~] **Misiones de simulacro que dan aptitudes** — **hecho (2026-07-24):** `SanctuaryMission` tiene
      `rewardAptitude`/`rewardAptitudeAmount`; `MissionTracker` los aplica vía `CharacterLevel.GainAptitude`
      (sube aptitud + alimenta la marga de Stats). Falta poblar valores por misión cuando existan más.
- [ ] **Extender el yoga — orphans** (revisión 2026-07-24, known-issues §Yoga; **requiere compilación**):
      resolver `AsanaEvaluator` (no se instancia) y `AccumulatePostureStress` (0 invocadores), y hacer
      **persistente** la maestría (`Asana.*` son `NonSerialized`). Wiring de gameplay; mejor con Rider.
- [ ] **Maná latente → el yoga desbloquea su barra** — **hecho** en `CharacterLevel.ManaUnlocked`
      (Yoga≥2; solo visibilidad/uso, incrementos = stats+alma). *(marcado completo.)*
- [ ] **Niveles por plano:** Mesocosmos autoritativo (reglas profundas); Macro/Micro pueden abstraer
      nivel+XP si el rendimiento lo exige.
- [ ] **Decisión clave:** modelo de guerra en modo Meso (encarnada recomendada vs tiempo comprimido) — §9.

## Mente emergente (anima-architecture.md §6/§11 · mind-model.md)
Pilar `Mind` como componente enchufable. **MVP + composición hechos (2026-07-24…28).**
- [x] **Mind MVP** (PR #13): `Mind` piensa por `thinkInterval`, elige FRASE por tono elemental
      (aptitudes+humores), la expresa hasta su PODER MENTAL (`Depth`); `Humores` (bioquímica),
      `ElementalTone`, `PhraseLibrary` (4 frases elementales).
- [x] **Clasificación de frases + campos** (PR #15): `PhraseCategory {Vivencia,Asana,Hechizo,Elemental,Deseo}`
      + flags `randomAssignable`/`reusable`; `ThoughtField` (empuja tono / nudge de humores por radio);
      `Mind.PickTone` suma aptitudes+humores+campos+**vivencias propias**.
- [x] **Pools de frases** (PR #15): `PhrasePools` — 14 vivencias FIELES a `creature-stats.md`
      (Goluis→Tierra, Panterilia→Viento, Gohageneis→Agua, Irosene→Fuego) + 6 deseos base; `DealVivencias`
      (reparto al azar respetando flags); `VivenciasOf(source)`; **Ötzi** autorado (histórico, `Historico()`).
- [x] **Propiedad + reparto por modo** (PR #15): `Mind` tiene `identity`/`thoughts`/`thoughtsLocked`;
      `PhraseDistribution` con `NarrativeMode {Estricta, Libre}` — estricta conserva la propiedad; libre
      vuelca los no-bloqueados a un pool público y reparte al azar; **Magnate/históricos bloqueados** quedan
      fuera y conservan los suyos. `Plan()` puro/loggeable + `Distribute()` a los `Mind` reales.
- [ ] **A PROBAR en el editor** (sandbox `MindSandbox_AUTO` de `SampleSceneBuilder`): en Play, cada ánima
      tiende a su tono y dentro del `ThoughtField(Agua)` se inclina a Agua + sube serotonina; consola
      `[Frases]` imprime conteos de pools, biografías por fuente, y el reparto **Estricta vs Libre**
      (Magnate + Ötzi 🔒 conservan lo suyo; compañeros + anónimo reciben del pool).
- [ ] **Siguiente**: controlador intercambiable (jugador-como-input ↔ IA) y **posesión dinámica**
      (insertar instancia/madre en runtime con relevancia); asana/hechizo como frases reales; multi-instancia.
- [ ] **Flag futuro** `absorbsPublic` si algún día un bloqueado debe además recibir del pool (hoy: no recibe).

## Testing — validado en Unity (detalle en `testing-checklist.md` §10–12)
Compila 0 errores; validados migración Anima, progresión, farming, Mind y cocina. Por probar: virtualización/
dispatch (§11/§12).
- [ ] **Pendiente — solo con JUEGO MANUAL**: daño de "Dura" (perder control); **trepar** (mantener Espacio);
      4 esferas de Mesopotamia + `YogaPortal`; **misión de yoga** (→ marga Yoga + maná); `ThoughtField_Agua`;
      velocidad de tiempo; y todo el §12 (Mecánica/Construcción arranque + dispatch del grifo).
- [ ] **Cambio conocido**: NPC arrancan `physicalResistance=1` → posible re-tuning de `promotionStrength`.

## La Cocina — primera simulación (nivel de referencia)
Diseño completo en [`kitchen-simulation.md`](kitchen-simulation.md). Escalera de construcción (§12):
- [~] **★ INTERACCIÓN DE VIRTUALIZACIÓN (el núcleo jugable, kitchen §3b)** — **MOTOR + FEEL + TYPING HECHOS**
      (`Assets/Scripts/Virtualization/`): **`VirtualPointer`** = **mira FIJA en el centro** (no se mueve);
      apuntas girando la **cabeza=cámara** con **`HeadLook`** (I/K/J/L, con **restricciones** de yaw/pitch);
      confirmas con **F** (Espacio=salto); **ratón/touch = su propio cursor**. **Resaltado** de la parte
      apuntada. **`ProductionOrder`** (receta por pasos → producto → **cuota** = sustento). Recetas:
      **cocina** (`VirtualizationSandbox_AUTO`) y **huerto** (`GardenVirtualization_AUTO`). **Mecanografía**:
      **`TypingChallenge`** — el fogón es una acción temporizada que se acelera tecleando (cook/eggs/protein/
      b2…); congela cámara/mira mientras se teclea (`.Active`, gateado también en `PlayerController`).
      **Falta:** enganchar la cuota a la **misión real**; palabras **sobre el objeto** + bancos localizables
      (en/fr) y por compuesto; estaciones licuadora/horno; animaciones; activar `HeadLook` al entrar a estación.
- [x] **A — Paseo + limpieza mancha-a-mancha (Meso)**: limpieza (`DirtArea`/`DirtSpot`/`Cleaner`) + **PASEO**
      (`GuidedTour`/`TourStation`: el anfitrión recorre estaciones enseñando cada área, con el novato de
      alma compartida vía `HelpRequest`). Sandbox `KitchenSandbox_AUTO` + `KitchenOnboarding_AUTO`.
- [~] **B — Loop de desayuno + contenedor**: **MVP hecho** — `BreakfastCook` recorre la cadena
      (nevera→huevos→plancha→revolver→especiar→contenedor) y rellena un `FoodContainer`; `Eater` come del
      contenedor. **Falta:** hacer la cadena **espacial** (caminar a las estaciones con `FollowBrain`).
- [ ] **C — Alimentación por humores**: personajes eligen contenedor por utilidad+humores; comer aplica
      `compuestos` → nudge de humores. *(Introduce `FoodCompound` mínimo — modelo de §8: compuestos→humores→aptitudes.)*
- [ ] **D — Puente Micro/Meso**: mancha del Meso = región del MicroKitchen; minidrones extraen → desaparece (suelo primero).
- [ ] **E — Mundo-insecto vivo**: gusano→formas; conflictos fuerte/débil; misiones; enganchar al Guardián del Fuego.
- [ ] **F/G — Recetas ricas (química) + puente al santuario** (alimentar carnívoros con bonds).
- **Mínimo jugable:** A + B → luego D (la unión Micro/Meso).
- **Próxima área (principiante): el HUERTO** (kitchen-simulation §13) — continúa el microworld (fuego→
  agricultura, La Sembradora) y cierra el bucle con la cocina (produce ingredientes). 2ª simulación.
  Misiones de virtualización DISEÑADAS: [`garden-simulation.md`](garden-simulation.md) §8 (abonar→arar→
  trasplantar→regar→proteger→cosechar) y §9 (**mundo-insecto: misiones de guardián**, proteger las plantas
  de otros insectos).

## Áreas / historia — roadmap (area-progression.md)
Orden alineado con la línea temporal del microworld (una época por área). Ver
[`area-progression.md`](area-progression.md), [`garden-simulation.md`](garden-simulation.md).
- [~] **0. PRÓLOGO — Enfermería** (apertura del juego): a Kushal (viene de fuera) le hacen exámenes+vacuna;
      curiosea → **máquina de avatares** (`VirtualizationMachine`) → **Microcosmos PRE-FUEGO** (plantas
      medicinales, **La Recolectora** — autorada); apoya y **lleva a los débiles a la cueva** (`CarryToRefuge`/
      `WeakOne`); vuelve por la **sala de meditación** (`YogaPortal`, puerta bidireccional ya existente) →
      primer trabajo (Cocina). **Scaffold hecho** (`PrologueSequence`, `PlaneMessenger` mensajes del Mesocosmos,
      `CarryToRefuge`; sandbox `PrologueSandbox_AUTO`). **Falta**: montar la ESCENA real en Unity.
- [~] 1. **Cocina** (Paleolítico/fuego) · 2. **Huerto** (Neolítico/agricultura) — diseñadas.
- [~] **3. CRÍA** (corazón del santuario; **confirmada área 3, tras el Huerto** → Construcción=4, Mecánica=5,
      Enfermería=6, Yoga=7) — [`cria-simulation.md`](cria-simulation.md) + `CriaBeginner_AUTO` (limpiar→
      abastecer→rutina→nido). **Enganche a drives REALES**: `CriaCareTarget` toca `Animal.stress`/`hungry` y
      `GrowBond` (que ya factoriza trauma); el bond **se gana** (estrés alto → rechaza). **La Recolectora**
      (raíz pre-fuego) con cadena de misión (founding-trio §7.4). **Histórico fundacional: El Perro de
      Oberkassel** (real ~14.200 años; cachorro salvado del moquillo por amor → mito del santuario; autorado
      con vivencias POV-perro + cadena de misión, cria-simulation §5). **Falta**: cablear a crías reales
      (`Animal` del `FamilyGenerator`) y el gateo de actividades por bond.
- [~] 3. **Construcción (Meso) / Levantar refugio (Micro)** ([`construction-simulation.md`](construction-simulation.md))
      — **va ANTES que la Mecánica** (refugio antes que metal, Neolítico). Meso = hub de **estructuras** por
      **dispatch/tickets**. **Hecho**: arranque `ConstructionBeginner_AUTO` (limpiar→abastecer→cimentar/muro/
      techar), **El Tallador** autorado + base de historia (§5). **Falta**: recetas de estructura reales por
      área, misiones-historia cableadas, históricos posteriores (Maestro de catedrales/Brunelleschi).
- [~] 4. **Mecánica (Meso) + Forja (Micro)** ([`forge-simulation.md`](forge-simulation.md)): la área Meso
      es la **Mecánica** (reparar/mejorar **máquinas** de todas las áreas + vehículos/drones/teleportadores).
      **Arranque cableado** (`MechanicsBeginner_AUTO`): **limpiar** → **abastecer** → **reparación simple**.
      **1ª reparación real de máquina = CAMBIO DE RUEDA del camión** (`TruckMaintenance_AUTO`: aflojar→gato→
      quitar→poner→apretar→bajar; + aceite + agua). El **camión** lleva las cajas de suministros a las áreas.
      Tracción del Huerto = **bueyes** (decidido). La **forja** de bronce → **Microcosmos**
      (`ForgeVirtualization_AUTO`). **Frontera Construcción/Mecánica** definida (forge §1c). **Falta**:
      máquinas por área, más vehículos (globos/submarinos), teleportadores, decisión arado/espada, misiones-historia.
- [ ] **Onboarding genérico por área**: limpiar → **abastecer** (`StockingTask`, cajas→despensas) → producir.
      Ya en cocina (§2) y mecánica (§1b). `StockingTask`/`ProductionOrder` comparten base `VirtualTask`.
- [ ] 5. **Enfermería/Farmacia** (salud; hereda las enfermedades del sedentarismo; Imhotep/Hipócrates).
- [ ] 6. **Sala de Yoga** (Aliento y Mente; reusa meditación/Microcosmos; Buda/Sócrates; desbloquea maná).
- [x] **Reparación por DISPATCH (tickets)** — **MVP hecho** (`DispatchDemo_AUTO`): `ServiceHub` lista tickets
      + banco de herramientas (`Toolbox` tomar/devolver); la receta `requiresTools` → sin herramientas se
      rechaza. **Primera reparación real: GRIFO QUE GOTEA** de la Cocina (fontanería, la avería más típica/
      simple): cerrar llave → desmontar → cambiar junta → montar → abrir y probar. Bucle: ticket → tomar
      herramientas → ir al grifo → reparar → devolver. **Falta**: UI de tablero, tickets dinámicos, más averías.
- [ ] **Arquetipo de misión: DETENER CONFLICTOS (mediación)** (garden §5) — entre integrantes del mismo
      equipo (dominio vs autonomía) o entre tribus de la misma especie (territorio); todo desde
      pensamientos/humores. Resoluble por campo de calma / posesión-mediación / satisfacer la raíz / bond.
      Reutilizable en todas las áreas; debut en el Huerto. Núcleo: "que los fuertes no se coman a los débiles".

## Microcosmos — era pre-Ambrosio (origen de Sakshi) — DISEÑO acordado, a construir por rebanadas
Docs: [`microcosmos-sakshi-origin.md`](microcosmos-sakshi-origin.md) (nivel/era) + [`apremios-guardian-observacion.md`](apremios-guardian-observacion.md) (subsistema).
Decisión del usuario (2026-09-11): **diseñar a fondo primero** (hecho) → **empezar por el RÍO**.
- [x] **`RiverCurrent`** (1er build) ✅ **PR #182**: componente-zona que arrastra por empuje neto = fuerza − masa·resist (grande aguanta, cría no) + **debilita** (drena ATP + estrés). Test `RiverCurrentTest` (grupo 9). **Falta:** colocarlo en la escena del nivel (cuando montemos el nivel de Sakshi).
- [ ] **Preferencia de dieta** en `Forager.SelectPrey`: peso de apetencia por material/especie (el depredador **prefiere hormiga a gusano**) → va a Sakshi, no a Kushal.
- [x] **Escena BLOCKOUT del nivel** ✅ **PR #183**: `MicrocosmosSceneBuilder.BuildScene0` (escena propia `Microcosmos_Scene0_SakshiRio`, hermana ANTERIOR a Scene1) — orilla/entrada + río (`RiverCurrent`) + Sakshi cría debilitada (alma-mezcla) + depredador débil + **anillo de depredadores** (vértice seguro = orilla) + tribu que **converge sola** (`ThreatScanner`+querencia) + spawn/portal/offset/navmesh.
- [ ] **Mecánicas pendientes del nivel** (sobre la escena): **preferencia de dieta** (el depredador prefiere hormiga a gusano) en `Forager.SelectPrey`; **empujar** del gusano-Kushal; **escalada de hambre** que vence al miedo.
- **Decisiones (2026-09-11):** (a) nombre = **"apremio"** ✅; (b) umbrales = ciencia real (**alostasis** + presupuesto **BMR(Kleiber masa^¾)+AEE(`Exertion`)+termorregulación**, fuentes=`Metabolism`); (c) stats mentales = **PAD +Dominancia** (nueva, ligada a Observación), Big Five derivado de aptitudes, CHC de referencia; (d) **todo es Anima** → el río afecta también a objetos. Ver `apremios-guardian-observacion.md`.
- [ ] **Subsistema apremios/Guardián/Observación** por rebanadas: Guardián (generalizar `MoodDynamics`) → registro de apremios (hambre/miedo) → aptitud `observacion` → `Observar` pasivo (ojos, `grants`) → adormecer al Guardián (químico). Ver el doc.
- [ ] Ley de la saga: **≥6 personajes vivos por historia, misma alma en cuerpos distintos**. En esta era, Momo/Medea = almas en **ancianas** que mueren cerca de su nacimiento.
- [ ] Escenarios INTERMEDIOS entre este nivel y el alba de Ambrosio (a definir).

## Hospital, jardines en la cima y submarinos médicos (2026-11-03; [`hospital.md`](hospital.md) §4–§6, [`submarinos-medicos.md`](submarinos-medicos.md), [`jardines-frutales.md`](jardines-frutales.md) §Jardines en la cima)
- [x] Jardines frutales en la cima (el techo de alfombras) de cada edificio-árbol: **bancales de tierra** por gremios de pH (cítricos, andino de semisombra, bayas ácidas, mediterráneo, trópico húmedo) con compañeras, flores de néctar, hotel de insectos, abejorros, caja nido, lombrices y goteo; investigación con fuentes y decisión tierra frente a maceta (`GardenBed`, `RoofGardenTest`, grupo 80).
- [x] La jornada del jardinero: los frutales de la cima entran solos en su ronda y van primero (`TipOrder`).
- [x] **Submarino-ambulancia anfibio** (14 × 5 × 5 m, el vehículo de diseño de los árboles): por el tronco a por el equipo del hospital, al paciente, a la puerta de urgencias o de veterinaria; bodega para oso o elefante, O₂, monitor, desfibrilador, ventilador, sangre O−, fluidos (`HospitalSubmarine`, grupo 81).
- [x] Investigación de la atención a ballenas y orcas (varamientos, sedación, campo, rehabilitación) y el **Leviatán**: hospital submarino colosal atracado a ras del hielo, con su pozo médico de suelo elevable; la ambulancia le hace de lanzadera; escenario de una orca herida (`WhaleHospitalSubmarine`, grupo 82).
- [x] **Banco de sangre**: almacén por temperaturas, ABO/Rh, perros DEA 1 y gatos A/B, caducidad, donación (sillón de donante), transfusión desde el puesto y en el quirófano, ronda diaria dentro del inventario del hospital que repone el submarino (`BloodBank`, `BloodLoss`, grupo 83).
- [x] **Paredes de cristal** (`GlassWall`): la única pared permitida; guarda los 2 m (`FurnitureSpacing` y `MicroBuildRules`). En el hospital: 4 quirófanos, UCI, 4 aislamientos con esclusa, 4 quirófanos veterinarios, sala fría de pingüinos y aislamiento veterinario (plantas 1 y 2 redistribuidas en el piso de 40 m); revisión de los hospitales de los microcosmos (grupo 84).
- [ ] Verificación en vivo (la hace el agente probador, 2026-11-03): grupos 80–84, frutales (55), jornada (50, línea base 196/1), vida del santuario (28) y experiencia del jugador (40), con fotos en `Temp/Hospital/` (sonda `scratchpad/tmpcheck/HospitalFotos.cs`).
- [ ] La ducha de descontaminación de Prípiat (1986) con cristal: es del constructor de los pueblos.
- [ ] Una alarma que llame sola a la ambulancia cuando alguien cae herido lejos; las ballenas mayores, junto al casco con los pontones.

## Horario libre semanal y los seis del mesocosmos (2026-11-03; [`horario-libre.md`](horario-libre.md), [`personajes-mesocosmos.md`](personajes-mesocosmos.md))
- [x] Reparto semanal de las horas libres (16, 17, 18, 20 y 21 h) con la lógica de la jornada (`LeisureRoster`): catálogo que rota por franja, a quien menos la ha hecho, con plazas.
- [x] Horario en el juego por escena (`LeisureDirector`): santuario (personal de la jornada) y microcosmos (insectos-humanos con `MicroJornada`); andar al sitio y hacer la actividad; las necesidades van antes.
- [x] Etiquetas de actividad de los edificios (ocio, recuperación, descanso, capacidad, interacción): el tiempo libre no va a edificios de trabajo.
- [x] Panel «Horario libre» (botón abajo a la izquierda / consola `horario`).
- [x] Elon Musk, Goku, Bill Gates, Warren Buffett, MacKenzie Scott e Ibrahim Traoré: papel, aspecto de cubos, casa del árbol (3.ª casa por árbol) y jornada (2026-11-04: ya no están; ver abajo).
- [x] Prueba `HorarioLibreTest` (grupo 65).
- [x] Retratos de los seis (Temp/Horario/Mesocosmos.png y MesocosmosCaras.png) con la escena reconstruida (Blockout + microcosmos) y guardada.
- [ ] **Ronda 8, arreglos (2026-11-05, claude/fix-ronda8), en código y compilados, sin probar en vivo:** 75, 92, 97, 88, 28, Angostura en pueblo, 7 esbozos de famosos, tornapuntas del Sitio Nuclear, araña al acecho, abeja fuera del tronco, orca bajo la placa, colmenas color miel y vóxel de 0,25 m en microcosmos. Queda el pingüino que da vueltas: hace falta su detalle del vigía.
- [ ] Verificación en vivo de los grupos 65, 28, 62 y 50 (línea base 196/1) y fotos del panel: la hace el tester único. 1.ª pasada (2026-11-04): 65 = 30/1 (Kapilavastu sin franjas: el reloj no llegaba a las 16 h con la escena cargada; corregido con tope de tiempo mayor y las franjas en marcha) y 50 = 148/2 (Elon Musk no llegaba a la escalera horizontal; corregido: dobla las rodillas). 2.ª (lote F): 43 = 6/4 (inicia el huerto Warren Buffett desde la ronda de frutales del techo de Servicios; corregido: inicia quien está en el área). Ronda 2: 43 y la escalera OK; 65 = 34/2 (en Kapilavastu se les ponía en su sitio a los 60 s y el transform peleaba con el NavMeshAgent; corregido: paso ligero según la distancia, tiempo según la distancia y Warp). Ronda 3: 65 = 35/1 (0/5 llegan andando: el agente se quedaba al final de un camino parcial; corregido: puesto con camino completo) y fotos con la escena equivocada (corregido: cámara propia y panel de la escena de la prueba). Repetir 65.
- [ ] Ballet, Dojo y Calistenia entran solos en el catálogo al terminarse sus obras (hoy el baile y las artes marciales van al Gimnasio).
- [ ] Horario libre de Kushal como sugerencia (hoy el jugador es libre y no entra en el reparto).
- [x] ~~Los seis en el santuario~~ → **2026-11-04: salen a sus microcosmos** (ver la sección siguiente).

## Familia y amigos de Berón; los famosos, a sus microcosmos (2026-11-04; rama `claude/edif-personajes`; [`diseno-personajes.md`](diseno-personajes.md) §Familia y amigos, [`personajes-mesocosmos.md`](personajes-mesocosmos.md))
- [x] Aspecto de cubos de Nesvitor, Amykhuz, Karen, Alicita, Ducristin, Josedrilo, Maite, Shalva y Adaguchi, de sus fotos; Tote mejorado según `tote.jpg`. Piezas nuevas en `CubeHair`/`CubeHumanoid` (peinados, bigote, gafas de sol, chaqueta, cuadros/rayas/canalé, falda, reloj, bolso…).
- [x] En la escena (`FamilyCharacters`), `PlaceStaff`, `WorkdayDirector.staff`, `HomeArea`, horario libre y galería («Character Gallery (familia y amigos de Berón)»).
- [x] Casas del árbol: las 18 para el personal que queda y los nueve; Amykhuz y Karen comparten casa (`Bedroom.partner`). No queda casa libre.
- [x] Sadhguru, Elon Musk, Goku, Bill Gates, Warren Buffett, MacKenzie Scott e Ibrahim Traoré fuera del santuario (escena, jornada, casas, horario libre, galería, turistas, pruebas). Su aspecto y sus datos (`MesoCharacters`) se quedan para sus microcosmos.
- [x] Papeles: yoga → Andrea (y Irosene, segunda maestra); mecánica → Ducristin; registros y vacunación → Maite; cuentas → Nesvitor; huerto → Alicita; guardia → Josedrilo con Gohageneis; artes marciales → Gohageneis.
- [x] Pruebas: `HorarioLibreTest` (65) comprueba a los nueve, la casa compartida y que no quede ningún famoso; `SanctuaryLifeTest` (28), `YogaClassTest`, `YogaFloorsTest` y `RestaurantServiceTest` actualizadas.
- [ ] Tester: reconstruir SampleScene (Blockout, solo el santuario) y guardarla; grupos 28, 50, 65, el de yoga y el del restaurante; retratos de la galería de la familia.
- [ ] Berón: confirmar el reparto de nombres de `cumbaya.jpg` (supuesto de izquierda a derecha: Josedrilo, Maite, Shalva) y los papeles heredados.
- [ ] Microcosmos propios de Sadhguru y los seis (otro hilo).
- [ ] Vivencias (frases de conversación, `PhrasePools`) de la familia y los amigos: hacen falta datos de Berón sobre cada uno.
- [ ] Más casas del árbol (una 4.ª por árbol, con árboles más altos) para los recién llegados.

## Camas de 4 y guerra de almohadas (2026-11-04; rama `claude/edif-personajes`; [`guerra-de-almohadas.md`](guerra-de-almohadas.md))
- [x] Camas de 4 plazas (colchón de 3,60 × 2,10) en las 18 casas del árbol; casa de 9 × 7 m con los 2 m de paso; cada uno duerme en su plaza (la pareja, en plazas distintas).
- [x] Ring de almohadas sobre la plataforma central: cama colosal de n × n celdas de 2,5 m (n = ⌈√residentes⌉: hoy 5 × 5, caben 25 para 19), borde blando, almohadones, 50 almohadas sueltas, pista de alfombras propia.
- [x] Regla y validador (`PillowRing.Validate`, manual J15): falla si no caben los residentes o el personal; al reconstruir crece.
- [x] Actividad de todos a la vez en el horario libre (sábado 20 h; `LeisureRoster.Activity.together`, etiqueta del ring) con `PillowFight` (coger almohada, almohadazo, risa, vínculo, acotado).
- [x] Prueba `PillowFightTest` (grupo 89).
- [ ] Tester: reconstruir SampleScene (solo el santuario) y guardarla; grupos 89, 65, 28 y 55 (frutales de las terrazas); fotos del ring y de una casa.
- [x] Regla J16 en el resto de centros ([`centros-para-todos.md`](centros-para-todos.md)): pisos según los miembros (yoga, 2 de Hatha y 2 de Ashtanga; gimnasio, calistenia, dojo y ballet, 1), clases de Ashtanga que se reparten por las salas, ocio para todos (crías y textil por turnos, justificado); lo comprueba la regla de capacidad (`AreaCapacity`, grupo 75). Las alfombras rodean el ring de almohadas (`FlyingCarpet.Legs`).
- [ ] Tester: reconstruir SampleScene (la torre del Entrenamiento sube a 5 niveles) y pasar 75, 89, 65, 50 y las de yoga (`YogaFloorsTest`, `YogaClassTest`); fotos de los pisos de yoga nuevos y de un vuelo de alfombra desde la plataforma central.
- [x] Pijamadas ([`pijamadas.md`](pijamadas.md), manual J17): una sola cama-litera por casa, de dos pisos de 4, con tobogán para bajar del piso de arriba (8 por casa; la litera triple aparte se quitó por indicación de Berón), pijamada semanal con anfitrión que rota, dormir en su plaza esa noche y volver a casa a la mañana; prueba `SleepoverTest` (grupo 92).
- [ ] Tester: con la escena reconstruida, grupo 92 y fotos de una casa con su cama-litera y de una pijamada.
- [x] Profesiones y parejas (Berón 2026-11-04; [`personajes-mesocosmos.md`](personajes-mesocosmos.md) §Profesiones y parejas, manual J14 y J18): Josedrilo maestro de yoga (y bioconstrucción), Maite cocina, Shalva cultivo y bioconstrucción, Adaguchi submarinismo y cocina, Amykhuz cocina y química, Karen asuntos legales y lenguas (Aula), Alicita contabilidad y proyectos; Nesvitor consejero; Gallarle nuevo (pareja de Ducristin, pendiente de foto y de profesión). Parejas en una casa: Lizmibet y Tote, Amykhuz y Karen, Ducristin y Gallarle, Josedrilo y Maite (16 de 18 casas).
- [ ] Berón: confirmar que Amykhuz y Karen son pareja; la foto y la profesión de Gallarle.
- [ ] Tester: reconstruir SampleScene; grupos 50 (jornada con profesiones), 51 y 64 (Josedrilo de maestro), 45 (brigada de cocina), 65 (horario libre, Gallarle), 28, 89 y 92 (casas y ring con 20 residentes).
- [x] Más datos de Berón (2026-11-04; [`personajes-mesocosmos.md`](personajes-mesocosmos.md) §Profesiones y parejas, manual J14, J18 y J19): Amahuz → **Amykhuz** (alias para escenas viejas); parejas Amykhuz y Karen (confirmada), Alicita y Julian, Irosene y Fausto; Julian y Fausto nuevos (pendientes de foto; Fausto, mecánico); Josedrilo jefe de obra nueva, Tote jefe de reparaciones y guardia, Nesvitor guardia y agricultor, Ducristin gerente de banca y profesor, Andrea comunicación, Maite jefa de cocina y curandera, Irosene letras en el Aula, Panterilia segunda maestra de yoga; nada de ocio ni entrenamiento en pisos en obras; frutal de la terraza a 5,5 m.
- [x] Maestros de baile (Panterilia, también finanzas, y Gohageneis) que se turnan por días: guían el baile de las 20 h (en la Sala de Ballet o en la de reserva mientras está en obras) y el entrenamiento de ballet si la sala está terminada; Julian, dirección de proyectos con Alicita (`con_proyectos2`). Prueba en `HorarioLibreTest` (65).
- [ ] Berón: fotos de Gallarle, Julian y Fausto; profesión de Gallarle.
- [x] Seis nuevos (Berón 2026-11-04; [`personajes-mesocosmos.md`](personajes-mesocosmos.md) §Los seis nuevos): David y Adonis (informática), Yulo y Lale (biotecnología), Ivelyna (pediatra), Luciana (piloto); aspecto provisional pendiente de foto (Fem por el nombre, pendiente de confirmar); cuarta casa por árbol (árboles de 38 m, 24 casas); ring 6 × 6; 27 miembros (dojo 2 pisos, Aula 2; la torre del Entrenamiento, 7 niveles; simulador de holguras pasado).
- [x] Neo, niño, hijo de Ducristin y Gallarle: vive en su casa, sin jornada, va con su madre ([`personajes-mesocosmos.md`](personajes-mesocosmos.md) §Neo). Lale es hombre (confirmado).
- [x] Lale y Yulo (antes Ale y Julito), Malena (pediatra; 28 miembros, 23 casas ocupadas) y aspecto de las fotos de Yulo, Lale, Neo, Gallarle, Luciana, Ivelyna, Malena y Ducristin ([`personajes-mesocosmos.md`](personajes-mesocosmos.md) §Lale, Yulo, Malena y las fotos).
- [x] Ronda 7: 89 (necesidades tras el salto de reloj), 92 (alfombra y tobogán), 50 (tumbado en su cama) y `AreaCapacity` (ring fuera de Play).
- [ ] Tester: reconstruir el santuario; grupos 89, 92, 50 y 75; galería de la familia.
- [x] Berón confirmó el orden de las fotos `gallarle_luciana` e `ivelyna_malena` y la grafía «Ducristin». Luciana, también artes marciales (el dojo, con Gohageneis).
- [ ] Berón: fotos de David, Adonis, Julian y Fausto.
- [ ] Guardería o juego de niños para Neo, con Ivelyna (hoy no existe: solo sigue a su madre).
- [ ] Tester: reconstruir SampleScene; mirar la lanzadera (T13: debe subir con el nuevo techo del Entrenamiento, `LaunchSiteTest` 90), y grupos 28, 50, 55, 65, 75, 89 y 92.
- [ ] Tester: reconstruir SampleScene (la torre del Entrenamiento sube a 6 niveles: el ballet pide 2 pisos con 21 miembros); grupos 45, 50, 51, 55, 64, 65, 75, 89 y 92.

## Tareas e iniciaciones del jugador con «E» (2026-11-04; rama `claude/edif-tareasE`; [`tareas-del-jugador.md`](tareas-del-jugador.md), manual J18)
- [x] Estudiado el modelo de Ambrosio (`AmbrosioStory.Act`: una E que hace lo que toca según el sitio) y generalizado: `PlayerTasks` (catálogo de pasos «acción · puesto · objeto · gesto» y localizador de sitios) y `PlayerTaskRunner` (marcadores «E — acción», aviso en el sitio, pista de ruta, progreso, gesto 2,5 s, objeto en la mano, efecto al cerrar).
- [x] Las 66 misiones del catálogo de la jornada, las del material, fotobiorreactores y lanzadera con pasos escritos; elaboraciones, obras, reparaciones y frutales por prefijo; el resto, pasos genéricos de su clase.
- [x] Jornada de Kushal: al empezar la hora de su misión salen los marcadores y la pista; hecha solo con todos los pasos (o virtualizada); si la ignora caduca sin efecto; `WorkdayDirector.Finish` ya no aplica el efecto de sus misiones; el piloto de `JornadaTest` pulsa la E en su puesto.
- [x] Iniciaciones de cocina, huerto y sala de yoga: cada paso pedido sale como «E — paso» en el sitio del alumno (`PlayerTaskRunner.Live`); `step` y el asana siguen valiendo.
- [x] Prueba `PlayerTasksTest` (grupo 96).
- [ ] Tester: con SampleScene reconstruida (solo el santuario), grupos 96, 50, 41, 42 y 43; fotos de un marcador «E — acción» en el telar, en el bancal y en la mesa de emplatado durante la iniciación, y del panel de la tarea con la pista de ruta.
- [ ] Pendiente: pasos con E en el examen médico de llegada (`NewcomerInitiator`), si Berón lo quiere; efectos propios por paso (regar de verdad, servir un plato); que Kushal dirija la clase de yoga real cuando le toca `yog_guia`. Hecho: «Manos» pasa a la N y la melaza al punto (.); la E es solo la de interactuar (reconstruir SampleScene).

## Charlas entre personas (2026-11-04; [`conversaciones.md`](conversaciones.md))
- [x] Resumen del sistema que había (`CommunicationSpell`, `Mind`, `SocialField`, vínculos): nadie llevaba el hechizo, solo Mentes, no se veía, sin contexto.
- [x] `ConversationDirector` (uno por escena con personas): humanos e insectos-humanos (`PersonLife`), personal de la jornada y `Conversable`; humano↔humano, insecto↔insecto y humano↔insecto en la misma escena; cercanía, vínculo, actividad compartida (puesto, comida, franja libre) y ganas de hablar.
- [x] Turnos A/B de 4–6 frases, bocadillo con el tema sobre quien habla; vínculo, estrés, humores, cara y memoria.
- [x] Temas del contexto (`ConversationTopics`): trabajo, comida, ocio, familia, linaje de insecto, era/lugar (`MicroEra`/`ConversationContext`), historia del sitio, hambre, cansancio, la mente.
- [x] Prioridades: no mueve a nadie; una necesidad urgente, dormir, una iniciación o la historia la cortan; menos en turno. Tope de 4 a la vez, búsqueda cada 2 s.
- [x] Escena `Prueba_Conversaciones` (mesa, dos bancos, seis personas) y prueba `ConversationTest` (grupo 88).
- [ ] Verificación en vivo del grupo 88 y fotos de los bocadillos: la hace el tester único.
- [ ] Ver charlas en SampleScene y en Kapilavastu (comida, franjas libres) y que el registro no se llena.
- [ ] `ConversationContext` con su historia en los microcosmos de historia.

## Animales polares y territorio (2026-11-05; rama `claude/edif-territorio`; [`reglas-animales.md`](reglas-animales.md), [`territorio-polar.md`](territorio-polar.md))

Código y compilación hechos; falta la prueba en vivo.

- [x] Reglas para crear animales, escritas y en el manual (§13, E1–E14 y P12–P18).
- [x] Morsa, emperador, reno, liebre ártica, zorro ártico y buey almizclero con todas sus tablas, ficha con fuentes y escena de ciclo de vida; lobo ártico activado.
- [x] `Penguin` separado: pasa a pingüino de Adelia (0,6 m, nido de guijarros); el emperador tiene clave propia.
- [x] Bivalvos (`SwarmKind.Clam`) para la morsa.
- [x] Isla de tierra (58 %) con orilla, zócalo, lomas, charcas y tundra; reemplaza a los casquetes Sur y Este.
- [x] Producción y los osos en la isla (−180, −140); holgura 28,1 m.
- [x] Comedero retirado; alimentador por rotación con lista compartida y distancia segura; `LabShare` en vez del 80 % (grupo 105, [`alimentador.md`](alimentador.md)).
- [x] Deshierbe con raíz: malas hierbas de verdad, vivero, trasplante a la tundra e incinerador para las no nativas (grupo 102, [`deshierbe-cuidadoso.md`](deshierbe-cuidadoso.md)).
- [x] La lista del alimentador solo se renueva completa; quien no se alcanza vuelve a la cola.
- [x] Regla P19: maquinaria a la altura de lo que se pide (grupo 103, [`produccion-maquinaria.md`](produccion-maquinaria.md)). Laboratorio ampliado.
- [x] La isla es roca hasta el lecho; el hielo flota; túneles para el submarino, fauna fuera de la plataforma y respiraderos en hielo fijo (grupo 107, [`hielo-y-respiraderos.md`](hielo-y-respiraderos.md)).
- [x] `FaunaScale`: la fauna no sigue a la plantilla en Play.
- [x] Icebergs con focas y pingüinos de verdad, con su motivo y su estancia (grupo 108, [`vida-en-icebergs.md`](vida-en-icebergs.md)).
- [x] Petrel de las nieves real: vuela, se posa en icebergs y anida en la roca (grupo 109).
- [x] Fauna fría de los santuarios marino y aéreo (grupo 111, [`fauna-fria-santuarios.md`](fauna-fria-santuarios.md)); `PlaceAirColdFauna` ya se llama desde el Santuario Aéreo.
- [ ] Auditar la energía (falta su demanda total) y dar máquinas a las cifras de `AreaProducer`.
- [x] Cadena alimenticia (`FoodChain`, regla E15, grupo 85) y especie real para todo pez, crustáceo y medusa: 6 especies nuevas.
- [x] Frentes de barrera, 5 cuevas, acantilados, colonia del emperador, playa de morsas y nidos de Adelia.
- [x] Pruebas: grupos 99 (`TerritorioTest`) y 85 (`CadenaAlimenticiaTest`).
- [ ] **Probar en vivo:** «Rebuild Animal Prefabs» (el cuerpo de `Penguin` cambia), Blockout, escenas de ciclo de vida y grupos 99, 85, 105, 102, 103, 107, 58, 57 y 89.
- [ ] Alimentador de dos en dos con el oso; aves en los acantilados; bosque de algas; corro del buey como conducta.
- [ ] Lemming, la presa que le falta a la tundra (recomendaciones en `reglas-animales.md`).

## Siguiente (código)
- [ ] **Alma compartida (resto)**: `HelpRequest` ya hace "ir juntos" (MVP); falta **compartir pensamientos**
      (misma frase → instancia de mente/madre compartida) y que el sí/no lea bond/humores/inclinaciones.
- [ ] **Re-autorar vivencias** de Ötzi/Sembradora/Guardián con el canon de `founding-trio-stories.md`
      (la coneja, el señuelo, la planta favorita, las piedras) al cerrar sus misiones.
- [ ] **Posesión real (resto)**: `PlayerBrain.Act()` ya mueve el cuerpo poseído; falta **interactuar**
      (F/clic enrutado al cuerpo) e integrar `PossessionSpell` con el hechizo real del jugador (crecer power/range).
- [ ] **Asana/hechizo como frases reales** (hoy solo categoría); **multi-instancia** (madre flyweight + relevancia).
- [ ] **Flag futuro** `absorbsPublic` si un bloqueado debe además recibir del pool (hoy: no recibe).
- [ ] **Más históricos** con `Historico()` (quedan Gilgamesh/Enheduanna y eras posteriores;
      docs/mob-characters.md, mob-epochs-matrix.md).
- [ ] **Históricos ANIMALES** (POV animal, docs/animal-heroes.md): autorados El Perro de Oberkassel, Togo,
      Hachikō, Cher Ami; **candidatos** Balto, Laika, Clever Hans (→Mente), Koko, Jumbo, Greyfriars Bobby.
- [ ] **Enfermería "Una Salud"** (área 6): **una sola** clínica que cuida a **personas Y animales** (lado
      veterinario dominante; también los voluntarios). Basada en el marco real One Health; el mismo sanador
      trataba a ambos. El Perro de Oberkassel es su fundacional-animal. (Diseño: area-progression §6.)
- [ ] **Upa-yoga (resto)** (upa-yoga-mission §6): *(rebind del input ✔ PR #22)*; **asignar los
      huesos del rig** cuando el avatar rigged esté en escena (el driver ya los mueve), cablear efectos a
      `PlayerStats`/humores, mapeo elemento→compuesto (`Chemistry`), QTE de hombros (3+3), verificar reps (vídeo Isha).
- [ ] **Transformar la Cocina** (hoy humana) → reencarnaciones (viaje 2: Nasatya llevando restos) + viaje 3
      (Enfermería insecto: guiar al refugio + motivar a los deprimidos). Cerrar §13.4 (nombre anciano-pintor;
      vidas 2 de Atlas/Sakshi/Momo; ubicación de reencarnaciones).
- [ ] **Stats-as-truth — rebanadas** (docs/stats-as-truth.md §8): `CreatureRig` ✔ · `BodyPart` única ✔ ·
      `ScreenEffects` ✔ · **emotion-slice ✔** (`EmotionExpression`/`BodyPartReactor`/`EmotionReader`; circumplex+
      Laban+frases+legibilidad; `afabilidad`/`sensibilidad`; `emotion-model.md`) → **quedan**: **composición**
      (fases 1-2 ✔ `CharacterComposition`: adornos/ropa/miembros→visual+delta gestionado, host-mod, injerto progresivo; fase 3: BodyPartStats/assets)
      → **depredación por stats ✔** (`Predation`:
      masa/fuerza/textura/tamaño; el tamaño invierte presa↔depredador) → **hechizos** (transformación
      3-niveles/farol-vs-real, bond por stats,
      lector-de-mentes) sobre `PossessionSpell`+energía-timer. **Transformación 3-niveles/farol-vs-real ✔**
      (`TransformationSpell`); falta ligarla a energía-timer y a la depredación. Monetización cosmética al final.

## ⚠ Prefabs pendientes de crear (el compañero, en Unity — el repo NO versiona prefabs)

Los `Animal` NUEVOS **no spawnean** hasta que exista `Assets/Animals/{Especie}/{Especie}.prefab`; hoy `AddFamily` los
**omite con un warning** (graceful). El fitoplancton/krill/peces **NO** necesitan prefab (se generan con **primitivas** en
`BuildPhytoplankton`/`BuildKrill`/`BuildSwarms`; `Swarm.unitPrefab` es opcional para el pez-hijo). Pendientes:
- [ ] **Pingüino** y **Orca** (fauna de hielo): ya registrados en `Tools ▸ Cold Sanctuary ▸ Generate Animal Prefabs` →
      los crea automáticamente **en cuanto pongas un `.fbx`** en `Assets/Animals/{Penguin,Orca}/Models/`. Sin modelo, se saltan.
- [ ] **5 insectos** (Ant/Aphid/Ladybug/Spider/Cricket) para el **Microcosmos Scene1** (NO van al Santuario 1): mismo patrón,
      falta su `.fbx`/prefab. Hoy el contenido usa **primitivas** (`SimpleAnima`), así que la escena funciona sin ellos.
- [ ] Los ya existentes (PolarBear/Whale/Seal) deben tener su prefab en tu proyecto (validados antes); si no aparecen, córrelo.

## Escenas del MICROCOSMOS (son propias, hermanas — no van en el mesocosmos)

El microcosmos es su **propio plano**: sus niveles son **escenas separadas**, NO objetos dentro de la escena del
Santuario 1 (mesocosmos). Ya reorganizado así (PR #174):
- [ ] **Scene1 (Ambrosio / el alba)** — `Tools ▸ Cold Sanctuary ▸ Build Microcosmos Scene1 (Ambrosio)` → genera
      `Assets/Scenes/Microcosmos_Scene1_Ambrosio.unity` (cueva/pulgón-guía + mapa de hormigas/depredadores/hechizos).
- [ ] **Mesopotamia** (la ciudad-insecto) — `Build MobWorld Mesopotamia` → `MobWorld_Mesopotamia.unity`. **Es parte del
      microcosmos**; Scene1 es **anterior** en la historia (hermana). "Build Sample Scene Blockout" regenera AMBAS de un click.
- [x] **Offset + entrada del jugador** ✅ (PR #175): Scene1 se desplaza a un origen lejano (`O = -5000,0,5000`, distinto del
      de Mesopotamia) → no solapa el mundo base al cargar additive; y lleva `MobSpawnPoint` (el `MobWorldLoader` teletransporta
      ahí) + `YogaPortal` (salida) — carga genérica: `MobWorldLoader.Instance.EnterMobWorld("Microcosmos_Scene1_Ambrosio")`.
- [ ] **Falta el DISPARADOR de entrada** a Scene1 (qué trigger del prólogo/alba llama a `EnterMobWorld`) — a diferencia de
      Mesopotamia, que ya la abre la `VirtualizationMachine` de la cocina. Cablearlo cuando toque el prólogo.

## ⚠ Compilar y PROBAR en Unity (PRs #16–#96 en master)

`Control/` + `Kitchen/` + `Virtualization/` + `Prologue/` + `Microcosmos/` + extensiones de Mind. **Guion de
prueba en [`testing-checklist.md`](testing-checklist.md) §11–§14.** Bugs ya arreglados por el equipo:
`AnimaController.PickBest` (Object null-check), `GuidedTour.stopDistance`, y `CarryToRefuge`/`PrologueSequence`
`UnityEvent` sin inicializar (abortaba `Build()`). *(Pendiente reportado: los `AddListener` puestos en el
editor-script no sobreviven a Play → el aviso de `PlaneMessenger` del sandbox no dispara; no bloquea.)*
- [ ] **Compila** tras `pull`; si algo falla, pégame el error. Sandboxes: `PossessionSandbox_AUTO`,
      `KitchenSandbox_AUTO`, `KitchenOnboarding_AUTO`, `VirtualizationSandbox_AUTO`, `GardenVirtualization_AUTO`,
      `MechanicsBeginner_AUTO`, `ConstructionBeginner_AUTO`, `TruckMaintenance_AUTO`, `DispatchDemo_AUTO`,
      `ForgeVirtualization_AUTO`, `PrologueSandbox_AUTO`, `CriaBeginner_AUTO`, `MicrocosmosSandbox_AUTO`
      (ahora tableau §13: Ambrosio/Sakshi/Héspero/Medea/Momo/Ruth/Atlas + `SoulRecord`),
      `Nivel1Microcosmos_AUTO` *(pendiente: escena abierta suelo-de-bosque + hormigas con `WeaknessEffect` + checkpoint cueva)*,
      `UpaYogaSandbox_AUTO`,
      `ScreenEffectsSandbox_AUTO`, `EmotionOrchestraSandbox_AUTO`, `EmotionReader_AUTO`.
      Pools `Total≈59 Vivencia≈49`.

## Historial (hecho) — detalle en docs/`DEVLOG.md`/git
- **03-nov: horario libre semanal y los seis del mesocosmos.** Las horas libres de cada personaje de cada escena se reparten una vez por semana con la lógica de la jornada (el catálogo de ocio y cuidado rota por franja; cada actividad, a quien menos la ha hecho; plazas por sitio): `LeisureRoster`, `LeisureDirector`, `HorarioPanel`, prueba del grupo 65 (docs/horario-libre.md). Elon Musk (mecánica), Goku (artes marciales), Bill Gates (hospital), Warren Buffett (cuentas), MacKenzie Scott (huerto) e Ibrahim Traoré (guardia) se suman al santuario (docs/personajes-mesocosmos.md).
- **01-sep (PRs #166–#171): cadena trófica del Santuario 1 (HIELO) + sueño día/noche.** (#166) limpieza del bioma:
  fuera la fauna templada (conejo/ciervo/lobo/zorro → reservados a un santuario templado); base viva = marina. (#167/#168)
  **banco como ORGANISMO**: `FishSchool` con crías-hijas que se multiplican con el tamaño; **mordisco-por-colisión**
  (el depredador choca con el banco → desaparece un pez → nutrientes), `Forager.Graze` reconciliado (sin doble ingesta).
  (#169) **`FishSchool`→`Swarm`** (nombre genérico, `[FormerlySerializedAs]` no rompe prefabs) + **`Phytoplankton`**
  (productor "césped del mar", se autoregenera, IEdible; el enjambre lo pasta para crecer). (#170) **Krill** (`Swarm`
  `kind=Krill`, **dieta propia**: pasta fitoplancton, el pez pasta krill; `eatsKrill`≠`eatsFish`), **Pingüino** y **Orca**
  (dos `Animal` nuevos con catálogo completo; orca apex depreda foca/ballena/pingüino), y **reloj del día** (`TimeController`
  lleva la hora + `Clock` da el tic; la velocidad corre las horas). (#171) **Sueño día/noche** (`SleepCycle`: dispara
  `asleep` por el reloj, diurno/nocturno, **despierta ante amenazas**; peces/krill derivan más lento de noche); test
  `SleepTest` (grupo 8). Cadena real cerrada: **fitoplancton→krill→pez→foca/pingüino/ballena→oso/orca**. Falta el prefab de
  Penguin/Orca (los crea el compañero en Unity). **Siguiente**: las MUTACIONES del santuario (árbol carnívoro + miel letal
  `Torpor` + abejas de las nieves), y opcionalmente sueño por fatiga. Ver `docs/ice-sanctuary-ecology.md`.
- **01-sep (PR #165):** **LOD de simulación + limpieza de bioma + doc de ecología de hielo**. **`Lod`** (estático): las ánimas LEJOS del foco (jugador, tag Player) "piensan más lento" — se escala el intervalo de los ticks caros (`Volition.Tick`/`ActiveBehaveTick` — donde corre `SenseThreats` O(n²) — y el `Restore` de mantenimiento) por `SlowFactor` (1 cerca → `maxSlow`=8 lejos). No altera la lógica, solo la frecuencia → **muchas más ánimas** sin caída de FPS (complementa la deuda de partición espacial). **Insectos retirados** de `BuildWildlifePopulation` (#164): son del Microcosmos Nivel 1, no del Santuario 1 (hielo). **`docs/ice-sanctuary-ecology.md`** (nuevo): ecología de hielo + spec de especies (árbol carnívoro de hielo con rastro `Nectar`→miel letal `Torpor`→muerte→abejas hacen nidos de los nutrientes→árbol absorbe; abeja/araña de nieves, pingüino, foca/oso ya coherentes; plancton base). Análisis de escala (cuántas ánimas: alto solo el `Animal` completo; inanimadas/`SharedSoul` casi gratis). **Respuestas:** los insectos no trepan/vuelan (falta cablear el modo del hechizo de locomoción); `FishSchool` tiene comportamiento (deriva/huye/crece) y está en `refuge-and-adult-behavior.md` pero NO es Anima (podría volverse integral); `SharedSoul`/`SoulComposition`/`BlendSlot` (ánima integral + padres de alma por pilar mente/cuerpo/bond) YA existen (scaffold).
- **01-sep (PR #164):** **cablear los 5 insectos en la generación de fauna**. `SampleSceneBuilder.BuildWildlifePopulation` añade una "pradera micro" en el noreste terrestre (~100,0,130) con la cadena trófica: **Aphid** (savia, base, ×2) ← **Ant/Ladybug** (cazan pulgones) ← **Spider** (tope); **Cricket** omnívoro grazea. Cluster APRETADO (radios 6–10) para que el corto rango de percepción del insecto alcance a su presa. Aphid/Cricket como `Prey` → reciben `GrassPatch`. Los 5 ya están completos en los catálogos (`SpeciesProfile`/`StageProfile`/`Physiognomy`/`Family`/`Archetypes`/`Forager`, del commit del usuario `5a892f5`); **falta el prefab** `Assets/Animals/{especie}/{especie}.prefab` de cada uno → hoy `AddFamily` los omite con warning (graceful), spawnean en cuanto exista el prefab (mismo patrón que Wolf/Bunny). Al spawnear, salen en el HUD `[ECO]` categorizados por sus flags de `Forager`. *(Bugs previos: el compañero arregló en #0191129 el choque de nombre `TestRunner`↔`UnityEditor.TestRunner` (CS0434) y un guard nulo en `ReachGoalMission.OnGUI`; el harness dio `[TEST] TOTAL: 42 PASS / 0 FAIL` en Unity.)*
- **28-ago (PR #162):** **inanición (hambre prolongada → enfermedad → muerte)** (testing §40). `Animal.Starve()` en el tick `Restore` (junto a `Suffocate`): si el hambre supera `starvationMeals` comidas máximas → (1) gasta **grasa** (`fatReserves`); (2) sin grasa, **consume masa** (`rig.mass`↓ → menos poder/defensa = debilitado); (3) el hambre prolongada **lanza el hechizo de enfermedad** (`SicknessState.MakeSick` — el "punto físico" que necesita tratamiento, idea del usuario); (4) bajo `baseMass × lethalMassFrac` → **muere** por el camino normal (`Hurt`). Comer la detiene; recuperarse pide tratamiento (comer + curar la enfermedad). Cierra el bucle de balance que observa el HUD `[ECO]` (herbívoros sin pasto mueren, no solo pasan hambre). `WeaknessEffect` (viejos de Ambrosio) es un HECHIZO de drenaje de ATP→inmovilizar, aparte (no age-based). `StarvationTest` (grupo 7) asevera masa↓+enferma, no destructivo. Nota: el HUD del jugador ya tiene barras de energía/vitalidad (`CharacterLevel`).
- **28-ago (PR #161):** **misión-observación del ecosistema** (reencuadre WASD; testing §39). `EcosystemObservation` (WASD): no un check mecánico sino una EXPERIENCIA — el jugador recorre el santuario y un **HUD de status global** muestra, por especie, etapas (cría/juv/adulto) + hambrientos/celo/enfermos + totales herbívoros vs depredadores. Loguea `[ECO]` cada ~10s con la **tendencia** (Δ vivos) y emite **`[ECO] ALERTA`** (warning) ante desbalances (especie extinta, colapso de herbívoros/depredadores) → delata bugs de balance del mundo vivo (#159) que un test automático no capta. Solo lectura (agrega `Animal.wholePopulation`, barato). Uso QA (visión del usuario): el compañero la juega/vive, documenta fricción y arregla lo que impida completarla. `SampleSceneBuilder` la añade como `EcoObservation_AUTO`. (Status jugador atacado/huida = futuro.)
- **28-ago (PR #160):** **cortejo rico + enfermedad (hechizo-estado)**. **Cortejo:** `Reproduction.FindMate` elige la pareja compatible de **mayor vínculo (bond)** + más cercana (no la primera) → los emparejamientos siguen los bonds. **Enfermedad:** campo `Anima.sickness` (0–1) que `Predation.PredatorPower`/`Defense` leen para **debilitar** (un enfermo es **presa fácil** y peor cazador); `SicknessState` (auto-add) lo gestiona: onset espontáneo RARO, deposita el canal `Sickness` en la `TraceField` (los demás lo evitan / el depredador lo prioriza), **se recupera** solo, y `Heal(amount)` lo cura → **abre la Enfermería**. Balance-safe: `sickness` default 0. `SicknessTest` (grupo 6) asevera MakeSick→debilita→rastro→Heal. Otro hechizo-estado tras el celo, y primer gancho de la Enfermería.
- **28-ago (PR #159):** **MUNDO VIVO — flags activados + N8 (wander con propósito)**. Fin de "dormido": **`AiBrain.useVolition = true`** (la volición D3b conduce la conducta reactiva: comer+amenaza en paridad + deseo `mate` en celo; `ActiveBehaveTick` queda de fallback) y **`Reproduction.Enabled = true`** (población auto-sostenible; contenida por celo+saciada+pareja+cooldown+`softPopulationCap` → crecimiento lento, sin explosión). **N8 — impulsos de base del ser saciado**: `LifeStage.Wander` deja de ser random puro — sesga hacia **adelante** (alternación espontánea: explora sin volver por donde vino, etología §2.1) y hacia el **olor de comida** (contrafreeloading, N1). Reversible: poner los flags en false. Con esto todo el arco emergente (capacidades/temperamento/navegación/reproducción) pasa de dormido a **en juego**. A vigilar por el compañero en la próxima validación (población + paridad de volición).
- **28-ago (PR #158):** **reproducción (cortejo→concepción→parto) — cierra el ciclo de vida** (testing §36). `Reproduction` (auto-add en `Init`, **GATEADA OFF** por `Reproduction.Enabled` por seguridad de población): una HEMBRA adulta en celo (`EstrusState.InEstrus`), saciada, con pareja compatible cerca (misma especie, sexo opuesto, adulto) concibe → gestación (acelerada por velocidad de juego) → **parto** vía `SpawnOffspring` = `Instantiate(progenitor)` puesto en etapa `child` (crece por `Animal.Init`→`LifeStage`, como en `FamilyGenerator`). Condiciones estrictas + cooldown + `softPopulationCap` → no explota. `LifeCycleAuto` (grupo 5) asevera el mecanismo: `SpawnOffspring` produce una cría en CHILD de la misma especie (se destruye tras el test). **Cierra el hueco del ciclo**: nacer/crecer/morir (vejez `adult→soul`/comido/daño) ya existían; reproducir faltaba → ahora población auto-sostenible (al activar `Enabled`). Balance-safe: `Enabled=false` por defecto → sin cambio.
- **28-ago (PR #157):** **celo (estro) como hechizo-estado + `TraceField` persistente** (reproducción paso 1; testing §35). `EstrusState` (auto-add en `Animal.Init`): un ADULTO entra en celo por ciclo rítmico (fase desincronizada) y, mientras dura, deposita el canal `Estrus` en la `TraceField`; expone `InEstrus`. Deseo `mate` en `DesireCatalog` (buscar pareja siguiendo el gradiente de `Estrus`) — **dormido** hasta activar `Volition`. **Enciende el sistema de trazas**: `SampleSceneBuilder` añade un `TraceField_AUTO` PERSISTENTE (antes no había ninguno → `Leave`/`Trail` eran no-op en juego). Para reconciliarlo, `TraceField.Awake` deja **coexistir** varias (la 1ª es el singleton; `TraceFieldTest` usa la suya por métodos de instancia `Deposit`/`Read`/`Gradient`). `EstrusTest` (grupo 4) lo asevera. **Balance-safe**: nada consume `InEstrus`/`Estrus` para reproducirse aún (cortejo/gestación = pasos 2-3) → sin cambio de población. Cierra hacia el ciclo de vida completo (falta reproducir; nacer/crecer/morir ya están).
- **28-ago (PR #156):** **Grimoire — gate de repertorio de doble vía** (capabilities-and-embodiment.md §2, testing §34). `Anima.CanUse(spellId, bodyEnabled) = bodyEnabled ∨ KnowsSpell(spellId)` + `KnowsSpell` (lee el `Grimoire` del ser; universal pero **vacío/bloqueado** sin componente o sin `Learn`). Realiza el modelo de doble vía: cada hechizo global se habilita por **anatomía O magia** (`Morder`=`CanUse("morder", armament>0)`, `Ver`=`CanUse("ver", tieneOjo)`); una roca con `Mind`+`Grimoire` actúa por magia sin cuerpo. Balance-safe/**dormido** (nada llama `CanUse` aún). `GrimoireTest` (grupo 3 del TestRunner) lo asevera (vacío→Learn→conoce; vía corporal; vía mágica). Falta: que los `SpellBase`/frases-hechizo consulten `CanUse` + la animación degradada (llamadas a partes + fallback nube).
- **28-ago (PR #155):** **primera misión WASD-test — `ReachGoalMission`** (testing-checklist §33). El frente de juego real: misión mínima jugable ("llega al refugio") que reporta por el **mismo `TestProbe`** que los sandboxes → laboratorio y juego real por el mismo canal `[TEST]`. Coloca un marcador ~12 m adelante del jugador; al llegar (WASD), asevera `el jugador se movió (hubo WASD)` (recorrido > umbral → anti-falso-positivo de spawn-sobre-meta) + `llegó al refugio`. `SampleSceneBuilder` la añade como `WasdMission_AUTO`; SKIP si no hay Player. No es `ITestUnit` (la conduce el jugador, no el `TestRunner`) → emite sus `[TEST]` al completarse. Semilla del frente WASD (las misiones-test de hoy = avance del juego de mañana).
- **28-ago (PR #154):** **rename `PheromoneField`→`TraceField` (campo de stigmergy) + modelo de hechizos de doble vía**. Rename mecánico (clase+archivo `TraceField.cs`/`TraceFieldTest.cs` + refs en `SpellBase`/`LifeStage`/`SampleSceneBuilder`/docs; `TraceChannel` intacto): "pheromone" se quedaba corto (lleva amenaza/comida/celo…); "stigmergy" = el término científico (Grassé 1959) documentado en el header. **Corrección de diseño** (capabilities-and-embodiment.md §2): NO hay "hechizos corporales vs mágicos" — cada hechizo es global con **dos vías siempre**: anatomía **o** grimorio (ver=ojo∨grimorio; morder=colmillo∨grimorio; moverse=piernas∨levitación). El **repertorio vive en el alma** (`Grimoire` universal+vacío hasta `Learn` / `spellConfidence` / `MagicReserves`); la anatomía es una vía, no la sede → una roca con Mind+Grimoire se mueve por magia. `armament` = habilitador corporal del subconjunto ofensivo (por eso 0: lo pone la anatomía). **Animación degradada**: el hechizo llama a partes; sin ellas, animación global (la nube del mago / en la base de la roca).
- **28-ago (PR #153):** **`TestRunner` — orquestación de tests (array-de-arrays)** (testing-checklist §32). Idea del usuario: los `*_AUTO` dejan de auto-correr descoordinados; `ITestUnit` (Group + ParallelSafe + `Run()`) + `TestRunner` que reúne las unidades y las corre **grupo a grupo EN SERIE** (paralelo dentro de un grupo solo si todas `ParallelSafe` — default false porque casi todo muta estado compartido: la rejilla es singleton, la fauna se muta/restaura). Convertidos los 3 sandboxes en unidades (grupos 0/1/2); `SampleSceneBuilder` pone un solo `TestRunner_AUTO`. `TestProbe` gana totales globales (`ResetTotals`/`Total`) → un `[TEST] ═══ TOTAL: X PASS / Y FAIL` al final = **un veredicto ordenado**. Las **misiones WASD** reportarán por el mismo `TestProbe` → el TOTAL unirá laboratorio y juego real. Serie por defecto evita choques de singleton y de mutación concurrente.
- **28-ago (PR #152):** **`EmergenceAuto_AUTO` — bucle de temperamento** (testing-checklist §31). Tercer sandbox con `TestProbe`; asevera el bucle emergente **multi-paso** (lo que `FaunaChecks` de una-aserción no cubre), conducido de forma **determinista** (no espera cazas reales → no flaky): productor (`RecordUse` sube confianza) → consumidor→conducta (con `aggressiveness=0`, `Decide` frente a la presa más blanda pasa de ≠Fight con confianza 0 a **Fight** con confianza 80 = temperamento histórico) → decaimiento (`DecayConfidence`). Muta y restaura en el mismo frame → no altera la sim; SKIP si no hay par con ventaja de poder. `SampleSceneBuilder` lo añade como `EmergenceAuto_AUTO`. Los tests con TIMING real (caza que sube la confianza sola; asfixia mata) y los WASD (misiones) van aparte. **Nota:** `armament` es universal (campo de `Anima`, incl. herbívoros) pero NADIE lo fija a ≠0 por especie aún → hoy todos 0 (deriva de anatomía/composición E1, o futuro por especie: astas del ciervo = arma defensiva).
- **28-ago (PR #151):** **`FaunaChecks_AUTO` — checks de conducta/stats sobre la fauna REAL** (testing-checklist §30). Segundo sandbox con `TestProbe`, sobre la escena horneada + los 66 animales de `FamilyGenerator` (espera ~1.5s a que asienten). Asevera **funciones deterministas** (no la emergencia con timing → no flaky): wiring del refactor por componentes (ThreatResponder/Locomotion/Forager/SpeciesBody/AiBrain/AnimaController + `strength>0`), armamento ⟂ masa (subir `armament` sube `PredatorPower`, restaurado en el frame), confianza de combate en 0 al nacer, depredación por stats (el más fuerte PUEDE con la presa más blanda; la presa NO puede al depredador), y `Assess` gateado por sentidos (menos percepción → menos amenaza). `SampleSceneBuilder` lo añade como `FaunaChecks_AUTO`; veredicto en Editor.log (grep `[TEST]`). **Enciende** los tests de conducta que el smoke-test no aseveraba (§23/§24). Muta puntualmente armament/percepción y restaura en el mismo frame (no altera la sim). Los tests de EMERGENCIA con timing (caza→confianza sube→se envalentona; asfixia) y los WASD (misiones) van aparte.
- **28-ago (PR #150):** **primer sandbox `*_AUTO` con `TestProbe` — `TraceFieldTest`** (testing-checklist §26/§29). Self-test AUTOEJECUTABLE de la rejilla en pura lógica (sin NavMesh/Animals): se auto-crea un `TraceField` y asevera por `TestProbe` depósito/lectura/lejos-es-0/gradiente-hacia-el-depósito/decaimiento-temporal/`volumetric`-separa-por-`y`, luego se destruye. `SampleSceneBuilder` lo añade como `NavTests_AUTO` → corre al entrar en Play y el compañero lee el veredicto en `Editor.log` (grep `[TEST] ... TraceField`, `SUMMARY: X PASS / Y FAIL`). **Enciende** el primer test interactivo que el smoke-test no tocaba (la rejilla era invisible). Los checks de CONDUCTA (animal deriva al rastro / huye) necesitan NavMesh horneado → sandbox/misión aparte (siguiente).
- **28-ago (PR #149):** **`TestProbe` — mini-harness PASS/FAIL por consola** (testing-checklist §29). La capa que faltaba: los sandboxes/drivers conducen o vuelcan valores (`MigrationDiagnostics`), pero ninguno aseveraba. `TestProbe` (estático): `Begin(grupo)` · `Check/Near/Greater/NotNull` · `End()` → loguea `[TEST] PASS/FAIL · nombre` + `SUMMARY: X PASS / Y FAIL` (grep `[TEST]` en Editor.log). **FAIL = `LogWarning`, no `LogError`** (no contamina el "0 errores"). Reutilizable por (1) sandboxes `*_AUTO` autoejecutables y (2) **misiones jugables (WASD)** que reportan sus criterios de éxito por el mismo canal → verdad de laboratorio y de juego unificadas. Base para NavAuto/EmergenceAuto/VolitionAuto (encender los tests interactivos que el smoke-test del compañero no toca).
- **26-ago (PR #148):** **N7 (1er tramo) — subproducto = extensión de hechizo** (environmental-navigation.md §4.2/N7). `SpellBase` gana campos opt-in `leavesByproduct`/`byproductChannel`/`byproductStrength`/`byproductInterval` + `LeaveByproduct()` (deposita en `TraceField`, rate-limited, O(1)), auto-llamado en el dispatch de cast (Instant/Repeat/Channel). **Cierra el bucle** del sistema de rastros: un hechizo opt-in deposita en la rejilla → `Wander` (N1) lo lee y deriva hacia él. **Balance-safe:** `leavesByproduct` default OFF → todos los hechizos actuales (Pull/Walk/Fire…) sin cambio; y sin `TraceField` en escena, `LeaveByproduct` es no-op. Realiza la corrección del usuario (el subproducto es parte del hechizo, no una interfaz aparte; enfermar/defecar/celo/marcar serán hechizos que sueltan su canal). Falta (N7 resto): unificar `ScentEmitter`/`ThreatEmitter` bajo `Trace`, modelar esos hechizo-estado, seguir el gradiente (ya hay `TraceField.Trail`). Tests: testing-checklist §28.
- **26-ago (PR #147):** **N1 — leer el entorno para dirigirse** (environmental-navigation.md §4 N1). Sin grafar el `ImpulseController` (que POSEE el NavMeshAgent → pelearía con Wander/Flee/Forager de `Animal`): se aplica "leer → dirigirse" inline manteniendo el ownership. **`Animal.Flee` huye LEJOS de la amenaza** (dir = pos − amenaza, proyectada `fleeStep`; re-orienta cada tick) — ya no hacia un **ave al azar** (el placeholder podía llevar la presa HACIA el peligro). **`LifeStage.Wander` sesga su destino con `TraceField.Trail(ScentFood)`** (deriva hacia el olor de comida) — dormido/balance-safe hasta que N7 deposite (sin rejilla, `Trail`=0 → sin cambio). Deferido: graft completo de impulsos (pendiente decisión de ownership del agente). Tests: testing-checklist §27.
- **26-ago (PR #145):** **`TraceField` — la rejilla de rastros** (primitiva de la navegación por lectura del entorno; environmental-navigation.md §4.3). Campo de depósito para subproductos continuos (caminar) SIN un GameObject por traza: mundo→celdas, cada celda un `float[]` por `TraceChannel` (olor-comida/marca/amenaza/celo/enfermo/heces/dar-vueltas). Depósito **O(1)** (`Leave`), lectura **O(1)** (`Sniff`), gradiente "seguir el rastro" **O(9-27)** (`Trail` → vector-impulso). **Decaimiento perezoso** (`Settle` aplica el decay acumulado solo al tocar la celda) + poda de baja frecuencia → coste **plano** (independiente de la longitud del rastro), ~60 KB, memoria auto-podada. Diccionario disperso con clave 3 coords empaquetadas. **`volumetric`** (opt-in) incluye la profundidad `y` para **mar/aire** (gradiente 3D, 26 vecinas; el disperso no infla memoria); tierra = 2D exacta, misma ruta. **Aislada y DORMIDA** (fachada estática no-op sin instancia; nada deposita/lee todavía) → 0 riesgo de conducta; se enchufa en N1 (impulsos leen `Trail`) y N7 (`SpellBase.byproduct` llama `Leave`).
- **25-ago (PR #140):** **navegación por lectura del entorno — diseño + N0** (`environmental-navigation.md`). Hallazgo: el sistema que quiere el usuario (leer el entorno con sentidos → dirigirse a lo propicio/lejos del peligro) **ya existe en el Microcosmos** como **impulsos** (`ImpulseController` suma vectores ponderados: `HomeImpulse` ×estrés×bond, `Scent`/`ThreatScanner`, `BondEscapeReader`) — es lo que lleva a las hormigas de Ambrosio a la cueva. El overworld NO lo usa (deambulaba/huía hacia un **ave al azar**). Doc que unifica: mapea las señales (comida/refugio/no‑depredador = existentes; **confort/humedad**, **colegueo** = leer inofensivos calmados a la vista, **experiencia**/memoria, **engaño** = nuevas) a impulsos gateados por sentidos (C)+legibilidad, y las conecta con deseos (D3b: el deseo dice QUÉ, la navegación DÓNDE) y medio (asfixia). Plan N0–N5. **N0 (PR #140):** `LifeStage.Wander` deambula a un punto local (`HomeRadius` acotado 5–25) en vez de a un ave al azar (`Homebound` lo devuelve si se aleja); retira el placeholder sin sentido. `Animal.target` solo lo usaba `Wander`.
- **25-ago (PR #139):** **ahogo/asfixia por medio de baja afinidad** (era "solo documentado"). `Animal.Suffocate()` (en el tick pasivo `Restore`): si la afinidad del medio actual (`MediumFactor`) cae por debajo de `asphyxiaThreshold` (0.3, tunable), aplica daño progresivo `(umbral − MediumFactor) × asphyxiaRate × masa` vía `Hurt` → muere por el mismo camino que cualquier daño. Da consecuencia a quedarse varado donde no se puede respirar/mover (pez fuera del agua, ballena en tierra), mientras `CorrectMedium` sigue intentando llevarlo a su medio primero. Balance-safe por diseño: umbral 0.3 < afinidad-agua por defecto (0.4) → un terrestre nada torpe pero NO se ahoga, y los depredadores que pescan no se ahogan si su `waterAffinity ≥ umbral`. Solo `Animal` (universal a `Anima` = futuro).
- **25-ago (PR #137):** **D-cola — productor de FIGHT + decaimiento pasivo de la confianza** (capabilities-and-embodiment.md §4). Cierra el bucle de la confianza-por-uso: **productor de FIGHT** (`Animal.Fight` finally → `RecordUse(Capability.Combat, true)` al abatir la amenaza; solo victoria clara, sin penalizar que se aleje) — el que vence se envalentona, como en la caza (D2). **Decaimiento pasivo** (`Anima.DecayConfidence(amount)` que baja todas las capacidades; llamado en `Animal.Restore` con 0.02/tick) — la maestría se enfría sin uso, y el uso activo la supera. Ahora la confianza sube (cazar/pelear con éxito) Y baja (fallar/no usar) → temperamento verdaderamente histórico. Balance-safe (arranca en 0; el decaimiento sobre 0 no hace nada).
- **25-ago (PR #136):** **rebanada D3b1 — scaffold del motor de volición tras flag (paridad)** (volition-selection-engine.md §7). `DesireCatalog` (deseo = `NeedProbe` de drives + `Dispatch` a handler existente; hoy solo `eat` → `Feed`), componente `Volition.Tick` (mismo throttle que `ActiveBehaveTick`: deseo de mayor necesidad si libre + `SenseThreats` como reflejo siempre), y flag `AiBrain.useVolition` (**default OFF** → `ActiveBehaveTick` intacto). Con el flag ON reproduce la conducta actual (comer con hambre + sensar amenaza) — **balance-safe**. `Animal.SenseThreats`/`Feed` expuestos public. **No cambia conducta** hasta activar el flag y validar paridad en Unity. Pendiente D3b2 (deseos rest/wander + amenaza como deseo con piso de seguridad; flip del flag; retirar la rama fija) y D3b3 (hechizos como acciones → despiertan D3a/E2).
- **24-ago (PR #135):** **doc de diseño de D3b — motor de volición** (`docs/volition-selection-engine.md`). Contrato del pivote que conecta `Mind`→acción: un **deseo** (`PhraseCategory.Deseo` = frase + binding vía `DesireCatalog`: `NeedProbe`+`Dispatch`) se elige por `necesidad × EffectiveWeight` (que ya incluye confianza D3a) filtrado por `PassesGate` (aptitud + receptor E2), con **piso de seguridad** para amenazas (la supervivencia no sale a la suerte), y despacha los handlers EXISTENTES (`RespondToHunger`/`SenseThreats`/…) — reemplazando la prioridad fija de `ActiveBehaveTick`. Selector `Volition` que `AiBrain` invoca; deseos base sembrados por `SpeciesBody`. **Migración por rebanadas tras flag con paridad** (D3b1 catálogo+selector OFF por defecto → validar en Unity → D3b2 flip+retirar ramas → D3b3 hechizos como acciones + activar D3a/E2 en vivo). No toca conducta todavía (solo doc). Indexado en CLAUDE.md.
- **23-ago (PR #134):** **rebanadas D3a + E2 — el motor de selección de `Mind` gana confianza + receptor** (capabilities-and-embodiment.md §2/§5/§8). **D3a:** `MindPhrase.capability` (opcional) + `Mind.EffectiveWeight` pondera esa frase por `Anima.Confidence(capability)` (el bond-hacia-el-hechizo; piso `minConfidenceFactor=0.25` para poder probar lo nuevo). **E2:** `CompositionPart.grants` (ojo→`"see"`, colmillo→`"bite"`) + `CharacterComposition.Grants(cap)` (la query "¿tengo receptor?") + `MindPhrase.gateCapability` que `Mind.PassesGate` comprueba (reusa el patrón del gate por aptitud). Ambos mecanismos **DORMIDOS y balance-safe**: ninguna frase actual fija `capability`/`gateCapability` → conducta idéntica; se activan cuando existan las frases de categoría `Hechizo`/`Deseo` (D3b). Falta el salto grande **D3b** (que la selección de `Mind` CONDUZCA la acción, reemplazando la prioridad fija de `ActiveBehaveTick`; conviene doc de diseño antes).
- **23-ago (PR #133):** **rebanada E1 — anatomía→stats: el arma sale de una bodyPart** (capabilities-and-embodiment.md §2/§8). `StatBonus.armament` + su aplicación por el delta gestionado de `CharacterComposition` (modulada por la vitalidad del huésped, como lo biológico; `_aWpn` accumulator + `Step` sobre `anima.armament`). Loop completo: un **colmillo/veneno como `CompositionPart`** → sube `Anima.armament` → `Predation.PredatorPower` lo lee. Cierra B desde la anatomía (el arma ya es una PARTE, no un escalar suelto); realiza la idea de "crear el colmillo/veneno como bodyparts". Balance-safe (armament default 0 en `StatBonus`). Falta **E2**: el tag de receptor/capacidad en `CompositionPart` (ojo→`Ver`, aguijón→`Picar`) + query "¿tengo receptor?", que se cablea con las entidades de hechizo (D3).
- **23-ago (PR #132):** **rebanada D2 — confianza-por-uso conectada a la conducta** (capabilities-and-embodiment.md §4/§8). Primer bucle productor→consumidor de la confianza-por-hechizo. **Productor:** `Forager.Hunt` llama `RecordUse(Capability.Combat, victim.Dead)` solo si la presa estaba VIVA al empezar (una carroña no cuenta como caza) → abatirla refuerza, rendirse (agotado) merma. **Consumidor:** `ThreatResponder.Decide` calcula **agresividad efectiva = `aggressiveness` innata + `Confidence(Combat)/100 × confidenceAggressionWeight`** para el gate de atacar-si-más-fuerte. Nueva clave central `Capability.Combat` (evita magic-strings). **Balance-safe:** confianza 0 al nacer → conducta idéntica; el temperamento se vuelve HISTÓRICO (el depredador que caza con éxito se envalentona; el herbívoro nunca). Falta (D-cola): productor de FIGHT, decaimiento pasivo, y la selección ponderada (D3).
- **23-ago (PR #131):** **rebanada D1 — infra de confianza-por-hechizo** (capabilities-and-embodiment.md §4/§8). `Anima` gana `spellConfidence` (dict nombre→0–100) + `Confidence(spell)` + `RecordUse(spell, success, amount)` (éxito refuerza —más rápido de joven, vía `EffectiveBondGrowthRate`— / fracaso merma). Es el **bond hacia el hechizo**: la maestría/confianza como histórico, calcada del sistema de bonds. Motor del temperamento emergente (el tiburón que muerde con éxito gana confianza en "Morder"). *Falta D2:* productores (registrar éxito/fracaso al cazar/pelear) + consumidores (agresividad efectiva y selección leen la confianza). También documentada la rebanada **E** (anatomía→capacidad: `armament`/`perception`/receptores desde `CompositionPart` — colmillo/veneno/ojo/oído habilitan hechizos y componen quimeras; falta `armament` en `StatBonus` + tag de receptor).
- **23-ago (PR #130):** **rebanada B — eje de armamento ⟂ masa** (capabilities-and-embodiment.md §2/§8). Nuevo campo `Anima.armament` (arma ofensiva desacoplada de la masa: colmillo/veneno/garra/aguijón/parásito; default 0 = **sin cambio de balance**). `Predation.PredatorPower += armament × ArmamentPower` (1f, tunable). Un cuerpo pequeño-pero-armado (avispa, garrapata) puede **cazar** a uno mayor y además **ser temido** (el arma propaga por `EffectivePower`→`Assess` con un solo cambio). La pone la anatomía/composición, como `armadura`. Matchup tipado (veneno vs quitina) = hook. También anotado: la **confianza en un hechizo = un bond hacia el hechizo** (reusa `GrowBond`) — mecanismo de la rebanada D.
- **23-ago (PR #129):** **rebanada C — `Assess` gateado por sentidos** (capabilities-and-embodiment.md §3/§8). Leer la amenaza deja de ser información perfecta: `threat = Lerp(unawareThreat, real, clarity)` con `clarity = Clamp01(percepción / perceptionForFullRead) × legibilidad`. Con la percepción base de la fauna (≥1) claridad = 1 → **cero cambio de balance**; un ser de percepción baja (garrapata/ciego) no percibe el peligro y ataca "a ciegas", y subirle la percepción (darle ojos) recalibra su respuesta. El **bond** se aplica DESPUÉS del gateo (es memoria, no percepción). Legibilidad del otro = hook (`1f`; a futuro baja con tamaño diminuto/quietud/camuflaje). Reusa el principio de `EmotionReader` (fidelidad ∝ percepción).
- **23-ago (PR #128):** **rebanada A — `canHitAndRun` DISUELTO** (capabilities-and-embodiment.md §4/§8). El "pegar-y-correr" deja de ser un flag de especie: al defender crías, la elección Fight vs **acoso** EMERGE del margen de poder (`myPower > enemyPower × fightPowerMargin ? Fight : HitAndRun`) — si no puedo con ella, acoso (golpe + retirada hacia el nido, repetido; la coneja que muerde la cola de la serpiente). Borrado el campo `ThreatResponder.canHitAndRun` + las 7 asignaciones por especie en `ConfigureThreat`. `Reaction.HitAndRun` (corrutina en `Animal`) intacta. Queda `aggressiveness` como único campo por especie (→ histórico, rebanada D).
- **23-ago (PR #127):** **docs — modelo capacidad=hechizo + embodiment**. Nueva doc-fuente `capabilities-and-embodiment.md` (capacidad=hechizo con receta+id; lectura gateada por sentidos; arma ⟂ masa; temperamento=histórico; cuerpo=cableado / cambio de cuerpo=re-blend+reaprendizaje; thoughts por el mismo motor; plan A–D). Reconciliados `stats-as-truth.md`/`behavior-system.md`/`anima-dissolving-animal.md` con el estado actual (Diet/Carnivore/Herbivore borradas, Animal concreta) + índice en CLAUDE.md.
- **21-ago (PR #126):** **etapa 5 — BORRADA la jerarquía de subclases de forrajeo**. `Carnivore`/`Herbivore` eliminadas; las 8 especies heredan ya directo de `Animal`. Los flags de comida (`eatsPrey`/`eatsGrass`/`eatsFish`) los fija `Forager.ConfigureForSpecies(SpeciesArchetype)` en `Init` (data por especie: Wolf/Malamute→presa; Bear/Fox→presa+pesca; Bunny/Deer→pasto; Whale/Seal→pesca) → retirados el hook `ConfigureForager` (virtual de `Animal` + overrides de Bear/Fox) y `GrazesOnLand` (Whale/Seal). Los 2 `GetComponent<Carnivore>()` (detección de depredador en `Animal.SenseThreats` y `FishSchool.NearestPredator`) recableados a `Forager.eatsPrey`. Quedan solo `Anima → Animal → XBehavior` (las 8 clases son ya `SpeciesArchetype` + `Start` + `ConfigureThreat`); las refs restantes a Carnivore/Herbivore son solo comentarios históricos. Falta E6: mover `ConfigureThreat` a data + `SpeciesArchetype` settable → borrar también las 8 clases (dejar `Animal` + data).
- **21-ago (PR #125):** **`Animal` CONCRETA** (hito). Resuelto el último `abstract` (`Feed()` → concreto por flags: `Forage.eatsPrey ? Hunt : Graze`), y quitado `abstract` de la clase. Retirados `Carnivore.Feed`/`Herbivore.Feed` (redundantes). Toda la conducta/data ya está en componentes+catálogos, así que `Animal` es instanciable. **Reencuadre de "desacoplar":** sacar `LifeStage`/`Family`/`PostNatal` del tipo `Animal` es inviable (~15 miembros) y conceptualmente incorrecto (son sistemas del ser-animal, no universales) → el desacople correcto es `Animal` concreto + borrar la jerarquía de subclases (pendiente: recablear 2 `GetComponent<Carnivore>()`, mover hooks a data, borrar Carnivore/Herbivore/especies).
- **20-ago (PR #124):** **etapa 5 — `PostNatalStages` a data (`PostNatalProfile`)**. El último blob (secuencias de crianza anidadas por especie) se movió VERBATIM (copia del literal, sin reparsear valores) a `PostNatalProfile.Of(species)` (switch); `Animal.PostNatalStages` lo lee. Retirados los `_postNatalStages` + overrides + limpieza cosmética. **Las 8 clases de especie quedan casi vacías** (Wolf: `SpeciesArchetype` + `Start` + `ConfigureThreat`). Toda la DATA de especie es ya catálogo/componente. Falta: desacoplar `LifeStage`/`Family`/`PostNatal` del tipo `Animal`, hacer `Animal` concreto, borrar la jerarquía.
- **20-ago (PR #123):** **etapa 5 — ciclo de vida (stages + events) a data (`StageProfile`)**. Las etapas (`Childhood`/`Adolescence`/`Adulthood`, cada una `stageDays/minScale/maxScale`) y los 3 arrays de `Events` (varían: HomeBound territorial, Feed adulto) dejan de ser campos por clase y pasan a `StageProfile.Of(species)` (extraído 1:1 por script). `Animal` crea las etapas en `Init` (antes de `Fatten`, que usa `ChildStage`; auto-property porque se mutan) y los Events son get-only del perfil. Retirados stages+events+overrides de las 8 especies → clases DIMINUTAS (Seal 12 llaves). Solo queda `PostNatalStages` + `SpeciesArchetype` + hooks.
- **20-ago (PR #122):** **etapa 5 — gaits (`ActsPrep`) a data**. Los `actsPrep` (idle/walk/run con aniName + navSpeed/aniSpeed/energyCost) dejan de ser un campo por clase y pasan a `ActionsPrep.Of(species)` (catálogo extraído 1:1 por script; Bunny reusa "RunBunny" para andar). `Animal.ActsPrep` (antes abstract) se fija en `Init` antes de la config de `WalkSpell`. Retirados los 8 campos + overrides. Especies mínimas (Seal 21 llaves). Queda: stage params + Events + PostNatalStages.
- **19-ago (PR #121):** **etapa 5 — Preps de etapa a default de `Animal`**. Los 24 `*Preparations` (child/teen/adult × 8 especies) eran IDÉNTICOS (`{SetScale, SetRemainingStageDays}`) → `Animal.ChildPreps/TeenPreps/AdultPreps` concretos (array estático compartido, solo-lectura). Retirados los 24 campos + 24 overrides. Los `*Events` varían (HomeBound por territorio) → siguen como data por especie.
- **19-ago (PR #120):** **radio de caza POR STATS + scavenging**. El radio de detección de presa deja de ser fijo y pasa a `perception × huntRangePerPerception` (deriva de la percepción, crece con la evolución). Las **carcasas** cercanas son seleccionables siempre (comida gratis), **incluso de la propia especie** (scavenging ≠ canibalismo; el bloqueo de la-propia-especie ya solo aplica a presa VIVA).
- **19-ago (PR #119):** **presa por PROXIMIDAD + STATS; `Diet` RETIRADA**. `Forager.SelectPrey` busca `Anima`s comestibles cercanas (`Physics.OverlapSphere(huntRadius)`) —incluidas carcasas y el JUGADOR—, descarta la propia especie (no canibalismo) y las protegidas por vínculo (`CanHarm`); una presa VIVA solo cuenta si mi poder EFECTIVO (con manada) supera su defensa (`Predation`). Preferencia EMERGENTE (más fácil/cercano). Borrada la clase `Diet`/`PreyEntry` + las tablas de las 4 carnívoras + `Carnivore.Diet`. Oso/Zorro marcan `eatsFish` (pescan). El jugador-como-presa se cubre solo (es un `Anima`); se pierde caza de aves (no-Anima). Knobs: `Forager.{huntRadius, distanceWeight}`. Alinea con "depredación por stats" y borra 2 blobs de camino a E6.
- **19-ago (PR #118):** **etapa 5 — `Population` a registro central**. Los `static HashSet population` por clase de especie pasan a `AnimalPopulations.Of(species)` (dict por nombre). `Animal.Population` (get-only) lo lee; las `Diet` de los carnívoros re-cablean `XBehavior.population` → `AnimalPopulations.Of("X")` (mismo set → altas/bajas y targeting coinciden). Retirados 8 estáticos + 8 overrides. (`BirdBehavior.population` NO se toca: aves = clase aparte, no del roster.) Re-cableado mecánico verificado por grep.
- **19-ago (PR #117):** **etapa 5 — `Group`/`Family` a data**. La estructura familiar por especie (`new Family(size, parentsRate, parentalCare)`) deja de ser un `defaultGroup` estático por clase y pasa a un catálogo `Family.Of(species)` (copia por ser). `Animal.Group` (antes abstract) es auto-property concreta, fijada en `Init` desde el catálogo (RenderFamily la reemplaza). Retirados `defaultGroup`/`group`/override `Group` de las 8 especies. (Population sigue en las clases: está referenciada cross-especie por las `Diet`.)
- **19-ago (PR #116):** **etapa 5 — boilerplate de estado a `Animal`/data**. `animationsName` (no lo lee nadie) pasa a derivado del nombre de especie (`new AnimationsName(SpeciesArchetype)`); `HomeOrigin` (estado por-instancia) a auto-property concreta; `HomeRadius` a data de especie (`SpeciesBody.homeRadius` + catálogo). Retirados los 8+8+8 overrides/campos de las especies. Las clases de especie quedan ya solo con la DATA compleja (stages/preps/events/ActsPrep/Diet/Population/Group) + Start/Configure hooks — lo último para E6.
- **19-ago (PR #114):** **etapa 5 — config escalar de especie a data (`SpeciesProfile`)**. Los 11 escalares por especie (PackFactor, HarmVsBond, BondGrowthRate, BiteSize, Toughness, BaseStressLevel, VocalizationThreshold, NestSecurityLevel, MaxFatReserves, FatAccumulationRate, ThreatThreshold) + Material dejan de ser overrides por clase y pasan a un catálogo `SpeciesProfile.Of(species)` (valores extraídos por script, idénticos). `SpeciesBody.profile` lo provee; los virtuals de `Animal` (accesor `Prof`, nunca null) lo leen. Retirados ~88 overrides de las 8 especies. Las clases de especie quedan ya solo con boilerplate de estado + stage/diet/animations data.
- **19-ago (PR #113):** **etapa 5 — bases evolutivas a `SpeciesBody`**. `BaseAgility`/`BasePerception`/`BaseSensibility` dejan de ser overrides por especie y pasan a campos de `SpeciesBody` + catálogo por especie; `Apply` las fija y escribe `agility/perception/sensibility` iniciales (se retiran esas líneas de `Init`). Los virtuals de `Animal` (y `EvolveAptitudes`) las leen del componente. Retirados 16 overrides (8 especies × 2). `SpeciesBody` cubre ya: arquetipo→stats + pensamientos + medio + bases evolutivas.
- **19-ago (PR #112):** **etapa 5 — config de amenaza al componente**. `Aggressiveness`/`CanHitAndRun` dejan de ser virtuals de `Animal` y pasan a campos de `ThreatResponder` (`aggressiveness`/`canHitAndRun`); `ResolveReaction` ya no se los pasa a `Decide`. Cada especie los fija en un hook `ConfigureThreat` (llamado en `Init`) → retirados los 7 overrides (Seal ya usaba la base). `ThreatResponder` queda autocontenido. Un paso más para vaciar las clases de especie hacia data/componentes.
- **18-ago (PR #111):** **etapa 5 — `Physiognomy` desacoplada del tipo `Animal`**. `GetMealWeight/Max/Min` toman ahora `(float mass)` en vez de `(Animal)`; los ~10 llamantes (Forager/ActionPrep/PostNatalManager/LifeStage) pasan `rig.mass`. `Physiognomy` queda como clase de datos+math pura, usable por cualquier ser. Anotada la idea de que la physiognomy EMERJA de stats/químicos/hormonas (cuerpo como resultado). Falta E6: desacoplar `LifeStage`/`Family`/`PostNatal` y reconstruir el lobo por composición.
- **18-ago (PR #110):** **etapa 5 — `Physiognomy` data-driven**. El físico de especie (escala/masa/pesos de comida) deja de ser un `defaultBody` por clase y pasa a un catálogo `Physiognomy.Of(species)` (copia por ser). `Animal.Body` (antes abstract) se fija en `Init` desde ahí, antes de `Fatten`. Retirados `defaultBody`/`body`/override `Body` de las 8 especies. Con esto la identidad física de especie (stats+pensamientos+medio+físico) es ya toda DATA → las clases de especie quedan casi sin config. Falta E6: desacoplar `Physiognomy`/lifecycle/family del tipo `Animal` y reconstruir un lobo por composición.
- **18-ago (PR #109):** **etapa 5 — medio data-driven**. La afinidad por medio (land/water/air) pasa de overrides por clase (Whale/Seal) a **data del arquetipo** (`ArchetypeProfile` + `Archetypes` fija Whale/Seal acuáticos); `Anima` gana campos settables (`landAffinity/waterAffinity/airAffinity`, los virtuals los leen) y `SpeciesBody.Apply` los escribe. Retirados los overrides de Whale/Seal. Anotada la idea de **lifecycle por química/hormonas/bonds** (no solo temporizadores). Falta: `Physiognomy` como data + desacoplar lifecycle/family.
- **18-ago (PR #108):** **etapa 5 — `SpeciesBody`**. La identidad de especie (nombre de arquetipo → stats base físicas/mentales + pensamientos base) sale de `Animal.ApplySpeciesArchetype` a un componente `SpeciesBody` (`species` + `Apply(anima)`). `Animal` lo auto-añade en `Init` sembrado con su `SpeciesArchetype`; `SpeciesName` (karma/relaciones) lo lee. Así la especie deja de ser el TIPO de clase y pasa a ser data de componente → paso clave para reconstruir un lobo por composición. Falta: medio/`Physiognomy` y desacoplar lifecycle/family.
- **18-ago (PR #107):** **etapa 4 — reconciliar IA**. Las decisiones ACTIVAS del animal (forrajeo + amenaza) salen de `Restore` a `Animal.ActiveBehaveTick`, conducidas por `AiBrain` (auto-añadido junto a `AnimaController` en `Init`, con `RefreshBrains`). La POSESIÓN (`PlayerBrain`) las suprime → el jugador toma el mando del mismo cuerpo/componentes. `Restore` queda solo con lo PASIVO (metabolismo/evolución/medio/velocidad + decaimiento). Una sola IA. *Falta:* al poseer, evitar que WalkSpell (Transform) pelee con el NavMeshAgent.
- **18-ago (PR #106):** **etapa 3 COMPLETA — persecución extraída**. `Carnivore.Feed` movido entero a `Forager.Hunt(self)` (elegir presa + perseguir por `Locomotion` + herir al alcance + comer al abatir + llevar sobras a las crías). Ahora `Forager` posee TODO el forrajeo (select omnívoro + Hunt + Graze + Eat); `Carnivore`/`Herbivore` quedan en ~18 líneas (solo config `Diet`/`GrazesOnLand` + delegación de `Feed`). Siguiente: reconciliar IA (`AiBrain` conduce ThreatResponder/Locomotion/Forager) → reconstruir un lobo por composición → borrar `Animal`.
- **18-ago (PR #105):** **etapa 3 — pastar extraído**. `Herbivore.Feed` movido entero a `Forager.Graze(self)` (selección + ir a la fuente por `Locomotion` + comer + reducir el banco de peces). `Herbivore.Feed` queda como delegación de una línea. Falta solo la persecución del carnívoro (`Carnivore.Feed` → `Forager.Hunt`).
- **18-ago (PR #104):** **etapa 3 — extraído el COMER** (carnívoro) a `Forager.Eat`: mordisco (`IEdible.Consume`) + nutrición (hambre + `Metabolism` opt-in) + **bond** con quien dejó la comida + 1ª sólida de cría. `Carnivore.Feed` delega. Confirmado **multi-consumo** (varios animales muerden el mismo `IEdible`, pool compartido de gramos) → "dejar la presa en el nido" funciona (soltar un `FoodItem`, los demás lo forrajean). Falta extraer la persecución y el pastar del herbívoro.
- **18-ago (PR #103):** **`Forager` — modo mixto/omnívoro**. Sustituido el enum `FoodMode` por flags **combinables** (`eatsPrey`/`eatsGrass`/`eatsFish`); `SelectTarget` elige la fuente **más cercana** de las que come → un omnívoro (varios flags) come presa Y pasto. Carnívoro=eatsPrey+Diet, herbívoro=eatsGrass/eatsFish.
- **18-ago (PR #102):** **etapa 3 — semilla de `Forager`**. Componente `Forager` (`FoodMode` Prey/Grass/Fish + `Diet`) con la POLÍTICA "qué/dónde comer": carnívoro→`Diet.SelectPrey`, herbívoro→pasto/banco más cercano. `Animal.ConfigureForager` (hook por especie) lo configura en `Init`; `Carnivore`/`Herbivore.Feed` usan `Forage.SelectTarget`. Un paso hacia disolver la subclase `Carnivore`/`Herbivore` (el qué-come pasa a config de componente). Persecución/comer siguen en `Feed` (siguiente).
- **18-ago (PR #101):** **`Locomotion` — resto de comportamientos migrados**. `Carnivore.Feed` (persecución/comer), `Flee`/`Fight`/`HitAndRun`, wander y salidas de amenaza pasan por `Loco.*` (`GoTo`/`SetGait`/`Idle`/`Walk`/`Run`). Ya NO hay `nav.SetDestination`/`ActsPrep.*.Prep` en conductas (solo dentro del wrapper + lecturas de config). Falta reconciliar IA (que `AiBrain` conduzca) y sacar `ActsPrep` a data de arquetipo.
- **18-ago (PR #100):** **etapa 2 — semilla de `Locomotion`** (+ reorden del plan: `Locomotion` antes que `Forager`, porque `Feed` es sobre todo locomoción). Componente `Locomotion` (envuelve NavMesh + gait `ActionPrep`: `Walk`/`Run`/`Idle`/`Move`). `Animal` lo auto-añade en `Init`. Migrados los sitios emparejados (SetDestination+gait): `CorrectMedium` (agua↔tierra) y `Herbivore.Feed` (ir a comer + pastar). El resto (`Carnivore.Feed`, `Flee`/`Fight`/`HitAndRun`) coexisten con el patrón viejo hasta el siguiente PR.
- **17-ago (PR #98, docs):** ampliada `sanctuary-second-lap-and-fear.md` — S3 aire V2 = ciudad flotante (esfera de aire artificial) con insectos gigantes; S2 abisal V2 = laboratorio + burbuja prehistórica (megalodón/reptiles marinos/calamar gigante); S4 volcán V2 = quimeras + caracoles de fuego; S1 hielo CONFIRMADO (conejo→liebre ártica, +pingüino, +leopardo). **Santuario de la Magnate** (núcleo plasma+diamante): desierto artificial (sol/luna = plasma), puerta al infierno (desierto)/paraíso (plasma), demonios/ángeles, arco de Kushal (radar/evitar/misión secreta). **Amplificación V1 = `bonusPack`** (evento de entrenamiento, no multiplicador; falta enchufarlo a `Animal`). Australia por ubicar (¿cuarteles de jefes?).
- **17-ago (PR #97, docs):** `docs/sanctuary-second-lap-and-fear.md` — 2ª vuelta por los santuarios (V1→V2, inversión de poder, guerras, biomas por elemento, especies en peligro) + **miedo como efecto de hechizo** (magnitud+AoE, escalado por temple → stress + bond negativo; `auraFear` pasa a ser del hechizo) + control de AoE/potencia (futuro) + **hechizo de bloqueo de la Magnate** (cancela hechizos aterradores + daña al lanzador). Decisiones de especies/biomas **por confirmar**.
- **17-ago (PR #96):** **amenaza 100% stat-based**. Migrado el último punto ciego: la alerta de proximidad de `Escape` (antes `Rigidbody.mass × NavMeshAgent.speed`) ahora usa `EvaluateThreat` (Assess) + `alertReach` (tunable en `ThreatResponder`). Todo el sistema de amenaza (evaluación/decisión/detección/alerta) depende de stats; solo quedan las coroutines de acción (locomoción) para la etapa de `Locomotion`.
- **17-ago (PR #95):** **etapa 1 completada (assessment)** — `EvaluateThreat` → `ThreatResponder.Assess` plenamente stat-based (ratio `Predation.EffectivePower`, sin `rig.mass`/NavMesh; escala = fracción de mi poder, `ThreatThreshold` recalibrable). **Defensa de crías emergente**: retirado el flag `DefendsCubs` (base + 8 overrides) → sale del vínculo (cubBond) + autoabandono vs peligro. Dirección: el CUIDADO de crías (PostNatal) también debería volverse emergente en su etapa.
- **17-ago (PR #94):** **etapa 1 disolución — `ThreatResponder`**. Extraída la POLÍTICA de decisión luchar/huir/pegar-y-correr de `Animal.ResolveReaction` a un componente `ThreatResponder` (portable: `Predation.EffectivePower` + `autoabandono` + bonds). `Animal` lo auto-añade en `Init` y le pasa el contexto de crías (`defendingCubs`/`cubBond`, aún de `Family`/`Group`). `enum Reaction` movido. *Pendiente en etapa 1:* `EvaluateThreat`→`Assess` (tras recalibrar), detección y coroutines (con `Locomotion`).
- **17-ago (PR #93):** **plan de disolución de `Animal`** (`docs/anima-dissolving-animal.md`: disolver > extender+renombrar, kit-objetivo, qué método va a qué componente, orden por etapas) + **Fase 0 inicio** — `Animal.ResolveReaction` (fight/flee) migrado a `Predation.EffectivePower` (poder por stats + manada) × `(1+autoabandono)`, en vez de `rig.mass × NavMeshAgent.speed` + bucle de manada propio (scale-invariant, sin recalibrar). *Siguiente:* `EvaluateThreat` base y alerta de proximidad (cambian de escala → recalibrar con test).
- **17-ago (PR #92):** **rollout locomoción — animales (opt-in)**. El `NavMeshAgent` sigue navegando (pathfinding); si el animal lleva un `WalkSpell`, su `nav.speed` sale del hechizo cada tick (`Animal.FeedWalkSpeed` → `WalkSpell.StepSpeed`): correr (`Running`→channeling) sube gradual a la punta, andar decae, con la lógica del `powerBonus`. `ActionPrep.Prep` fija `Animal.Running` (¿es la acción de correr?). Auto-cablea base=walk.navSpeed, punta=run−walk. **Cero regresión** (solo cambia si se añade el WalkSpell). Los animales corren para huir/cazar/salir del agua/alerta-random. *Pendiente:* que la IA humana navegue por NavMesh con esta misma velocidad (AiBrain→NavMesh).
- **17-ago (PR #91):** **rollout locomoción — jugador**. `PlayerController` delega la VELOCIDAD horizontal al `WalkSpell` (carga-postura + punta + decaimiento + ATP), conservando dirección cámara-relativa, gravedad, salto y `_cc.Move`. `WalkSpell.StepSpeed(charging,channeling,moving,dt)` provee la velocidad sin mover. LeftShift parado = postura de salida; RightShift = punta. Builder añade el `WalkSpell` al jugador (base=walkSpeed, topes=sprint−walk). *Pendiente:* animales (NavMesh) al mismo hechizo.
- **17-ago (PR #90):** **locomoción universal** — `WalkSpell` conducible por brains. `SpellBase.TickPowerBonus(c,dt,charging,channeling)` explícito; `WalkSpell` gana `selfDriven`, `DriveFromInput()` (input propio) y `Drive(dir,...)` (programático). `PlayerBrain` conduce el WalkSpell si existe (ESDF + carga/correr); `AiBrain` lo conduce hacia `moveTarget` (rellena el hueco de locomoción IA). Andar/correr = un hechizo que cualquier brain mueve. Sandbox `WalkUniversal_AUTO` (caminante IA→destino). *Pendiente:* migrar el jugador principal (`PlayerController`) y los animales (NavMesh) a este hechizo.
- **17-ago (PR #89):** **pensamientos base de especie** (data-driven). `ArchetypeProfile.basePhrases` + `PhraseCategory.Especie` + `PhrasePools.Especie()` (Human/Bear/Wolf/Bunny/Fox/Deer/Seal/Whale/Lion/Malamute, tono por especie). Se siembran en `Mind.thoughts` al componerse: `SoulComposition.Resolve` (mente dominante) y `Animal.ApplySpeciesArchetype` (su especie). `Archetypes.BaseThoughtsOf` / `Mind.SeedThoughts`. NO hay clases de mente por especie: es el arquetipo (datos) el que declara stats mentales + pensamientos base.
- **16-ago (PR #88):** **animación de carga por hechizo** en `SpellBase` (`chargeAnimator` + `chargeAnimState`/`releaseAnimState`), disparada por `IsCharging`/`OnChargeReleased` — cada hechizo su animación (postura de salida del velocista, carga de la esfera…). `WalkSpell`/`FireSpell` auto-cablean el `Animator` local; los estados se nombran en el Inspector.
- **16-ago (PR #87):** **sistema de bonos de hechizo unificado** (`SpellBase`). Charge, channeling y forcejeo pasan a un único `powerBonus` que **decae con el tiempo**. `CastMode.Charge` **retirado**; charge=**LeftShift** (acumula sin aplicar; al soltar, burst que dispara), channeling=**RightShift** (suelo/tope dinámico `maxPowerWithChanneling`). Topes por hechizo (`maxPowerWithCharge`/`Channeling`/`Forcejeo`) escalados por aptitudes (físicas charge/forcejeo, mentales channeling). `WalkSpell` a **ESDF** con carga=postura de salida (arranque) + channel=punta. `FireSpell`: LShift carga la esfera gigante, RShift la sostiene. HUD/sandbox `SpellDemo_AUTO` actualizados.
- **14-ago (PR #86):** **jugador integrado en el modelo de ánimo/estrés**. `MoodDynamics`/`Exertion` ahora corren sobre **cualquier `Anima`** (el jugador ya lo era desde 2026-07-28): con `Mind` usan humores; **sin `Mind` (jugador)** derivan el estrés de las drives universales (`mentalFatigue`/`sleepiness`/hambre). En el builder el jugador lleva `MoodDynamics` y es `worker` de la receta de cocina → cocinar lo desgasta y estresa. Unifica el modelo (el jugador es «solo un Anima» más).
- **14-ago (PR #85):** **desgaste por trabajo (vía A)** `Exertion`/`ExertionCost`: las acciones de trabajo declaran coste (glucosa/minerales/fatiga/sueño) y gastan reservas del `Anima` → `MoodDynamics` lo vuelve estrés. Enganchado en `VirtualTask.worker`/`exertionPerStep`, `ProductionOrder.stepExertion[]` (cargar/forjar pesan más) y `StockingTask`. Sandbox `Desgaste_AUTO` (obrero Goluis que se cansa→estresa→`UnderPressure`).
- **14-ago (PR #84):** **dinámica de humores/estrés** `MoodDynamics`: el estrés (cortisol) es químico y **sube por el estado** (fatiga/sueño/hambre/reservas bajas), baja descansado; `Anima.stress` lo refleja. El mal humor de **Goluis es situacional** (`UnderPressure` emerge de `fatigue`/`stress`, ya no es un flag fijo). Enganchado en el núcleo de compañeros del builder. *Pendiente:* desgaste de reservas por acciones de trabajo (de pie/fuerza).
- **14-ago (PR #83):** **humores derivados de la personalidad** `HumorProfile.Apply`: los humores base salen de los stats (adrenalina ← sociability+creatividad+agilidad−disciplina; serotonina ← afabilidad+sociability; cortisol ← sensibilidad−composure; glucosa ← endurance+masa; calcio ← disciplina+composure). Se aplican al resolver el blend (`SoulComposition.WriteStats`, compañeros) y en `Animal.Init` (animales con `Mind`). Así el `SocialField` **diferencia las actitudes solo** (Gohageneis alto adrenalina/serotonina → fiesta; Panterilia más cortisol → calma). *Falta:* reducir los componentes bespoke de compañero a lo puramente mecánico.
- **14-ago (PR #82):** **(c) campo social global emergente** `SocialField`. Cada anima **contagia su ánimo** (sube serotonina/adrenalina) a las animas cercanas **con bond ≥ umbral y sin threat**, escalado por su `sociability` × (positividad+energía de sus humores). Generaliza `ThoughtField`; el jugador es una anima más. Enganchado a los compañeros (en `MakeCompanionCore`) → su "actitud" (fiesta/calma) **emerge** de sus stats/humores/bonds en vez de hardcodearse. *Falta:* derivar los humores por-compañero de su perfil (hoy default) y reducir los componentes bespoke (celebración/observación) a lo puramente mecánico.
- **14-ago (PR #81):** **(a) autoabandono derivado + (b) huir/ayudar por bonds+threat+autoabandono en `Animal`**. `Autoabandono.From(anima)` = entrega/(entrega+autoconservación) [entrega ← afabilidad+sensibilidad+bond medio; autoconservación ← composure+disciplina+instinto], ya no un campo crudo; `Anima.RecomputeAutoabandono` (llamado en `Animal.Init` y al decidir). `Animal.EvaluateThreat`: el **bond baja la amenaza** (bond 100 → amenaza 0, confianza no huida). `ResolveReaction`: el `autoabandono` envalentona (myPower×(1+autoabandono)) y por las crías planta cara si **(autoabandono + vínculo) > peligro** (desventaja). *Falta:* ayudar a un aliado LEJANO en peligro (navegación de `PackAwareness`); unificar con las hormigas.
- **14-ago (PR #79):** **`CompanionBase` RETIRADO (fase 5 completa)**. Borrado `CompanionBase`; su maquinaria compartida (vínculo con el jugador + mood/fatiga + anchors + efecto de proximidad) → componente **`MoodState`** (parametrizable por curvas, `IBondable`). Goluis/Panterilia/Gohageneis/Irosene reescritos como componentes finos (`: MonoBehaviour`) que solo llevan su **conducta propia** (presión+resistencia / bonus de observación / burst de celebración / motivación+diálogo) leyendo de `MoodState`; sus stats vienen del arquetipo (`SoulComposition`). `BuildCharacters` recableado (SimpleAnima+SoulComposition+Mind+MoodState+comportamiento) con las curvas/anchors por compañero; `MigrationDiagnostics` usa `MoodState` en vez de `is CompanionBase`. Radio de tipo mínimo (resto eran strings/comentarios). **Refactor grande → validar en Unity.**
- **14-ago (PR #78):** **captura de los 4 compañeros como arquetipos** (fase 5, aditivo). Goluis/Gohageneis/Irosene (+ Panterilia ya estaba) tienen ahora arquetipo cuerpo+mente con sus **`Base*` reales** (incl. `adaptability` — `MakeBody` ahora la acepta, default 1). Sandbox: `Goluis_SinClase`/`Gohageneis_SinClase`/`Irosene_SinClase` (+`Panterilia_SinClase`) = `SimpleAnima`+`SoulComposition`(su arquetipo)+`Mind`+`BondPillar`+speciesBonds Human → reproducen a la companion **sin heredar `CompanionBase`**. No se retira `CompanionBase` todavía (siguen en uso). *Falta (con compilador):* mover su `Update` de estado interno a un pilar, recablear su creación + UI de bonds, y retirar `CompanionBase`.
- **14-ago (PR #77):** **openness + efecto del bond**. `Archetypes.NetDisposition(especie)` (suma de sus relaciones) + `SpeciesKarma.Openness(me)`: al conocer una **especie nueva** (sin relación específica) el arranque se resuelve por la **disposición GENERAL** del ser (si en total sus relaciones son + o −). **Efecto** en `BondPillar`: la buena compañía **calma** (baja `stress`), la mala **inquieta** (lo sube) — proporcional a la relación con cada vecino; sigue reconfortando la mente del vecino por el bond. `stressEasePerPoint`.
- **14-ago (PR #76):** **base kármica por especie (`speciesBond`)**. Arquetipos de relación (`Archetypes.RelationValue` / `_relations`: foca→oso −40, conejo→lobo −50, perro↔humano +45, lealtad de manada…), `SpeciesKarma.RelationOf` (mezcla los `speciesBonds` del ser por dominio, o su `Anima.SpeciesName` directa), `SoulComposition.speciesBonds` (mezclable), y `BondPillar` **arranca el bond por karma** al conocer a alguien (solo la positiva siembra confianza; la negativa la lleva el THREAT por poder, separado). `Anima.SpeciesName` (Animal→su arquetipo). Especie nueva (lobo↔komodo) = 0. *Falta:* karma negativa→amplifica threat; circunstancias (ayuda/depredación); openness.
- **14-ago (PR #75):** **corrección — `BondPillar` universal (sin vía directa al jugador)**. Reescrito: cada `Anima` **familiariza con cualquier `ITarget` cercano según las circunstancias** (cercanía) usando el sistema de bonds **universal de `Anima`** (`GrowBond`/`GetBond`, etapa×trauma×aura) — el jugador es un `ITarget` más (`PlayerTarget`), no un caso especial. Se quitó el `bondWithPlayer`/player-first copiado de `CompanionBase`. La "comodidad" a la mente de un vecino escala por el bond mutuo. *Falta:* base kármica por especie (foca↔oso−, perro↔humano+) como punto de partida (soul-relations §2).
- **14-ago (PR #74):** **migración fase 5 (preparada) — disolver `CompanionBase`**. `Soul/BondPillar` implementa `IBondable` (vínculo con el jugador + efecto de proximidad, extraído de `CompanionBase`) como **componente** — igual patrón que `WorldBondable`. Así un compañero = `Anima + SoulComposition + BondPillar` (+ `Mind`), **sin clase especial**. Arquetipo `Panterilia` (cuerpo+mente con sus 12 valores reales). Sandbox: `Panterilia_SinClase` reproduce a Panterilia sin heredar `CompanionBase`. Aditivo (no toca los compañeros actuales). *Falta:* migrar las 4 companions a este patrón y **retirar** `CompanionBase` (con compilador/coordinación); mover su `Update` de estado interno al pilar.
- **14-ago (PR #73):** **migración fase 3 (mitad segura) — animales por arquetipo**. `Animal.Init()` llama a `ApplySpeciesArchetype()`: llena las 10 aptitudes NO gestionadas por `Base*` (fuerza/masa/aguante/adaptabilidad + las 6 mentales) desde el arquetipo de especie; `SpeciesArchetype` overridado en las 8 especies (Bear/Wolf/Fox/Malamute/Bunny/Deer/Seal/Whale). Antes los animales tenían aptitudes planas (todas 1); ahora difieren por especie (alimenta emoción/Mind/DerivedStats/magia). Respeta `agility`/`perception` (que las maneja `Base*`+evolución) y NO toca el huir/predación (siguen por `Physiognomy`/`rig.mass`). + arquetipos Fox/Deer/Seal/Whale/Malamute (cuerpo+mente). *Falta (mitad delicada, coordinar):* hormigas→`Animal`; que huir/predación lean el stat `bodyMass`; disolver `CompanionBase` (fase 5); mando (fase 4).
- **14-ago (PR #72):** **migración fase 2 — Mente por blend**. `SoulComposition.WriteStats` resiembra `Mind.aptitudes` con el resultado del blend (en Resolve/ConvertTo/SharedSoul) → el **tono y las decisiones** de la `Mind` **emergen del blend** (`Mind.PickTone` deriva de aptitudes; se resiembra porque su `Awake` corre antes del `Resolve`). Sandbox `AlmaBlend_AUTO`: `OsoMenteHumana` y `Oso_bonusPack3` llevan `Mind` → el de mente humana piensa Viento/Fuego, el de mente-oso Tierra. Testing §20. *Siguiente (fases 3-5):* especies→cuerpos reusando huir/manada de `Animal` (coordinar con el compañero), mando (mente interrumpe body), disolver `CompanionBase`.
- **14-ago (PR #71):** **`SharedSoul` — alma compartida entre reencarnaciones**. Varios cuerpos (`SoulComposition`) referencian una sola alma: comparten **forma** (identidad, canónica) + **poder** (magnitud) + **bonds**. `Register`/`ApplyTo` (convierte la forma al cuerpo, Literal/B, × poder), `GainPower` (entrenar/lesionarse → propaga a TODOS), `ReshapeFrom`, `AddBond` (lo tienen todas las reencarnaciones). `SoulComposition` + `sharedSoul` + `ComputeBaseStats()`. `SoulMath.Scale`. Sandbox `AlmaCompartida_AUTO` (melaza Toro+Bear / hormiga Ant+Human, una alma) + HUD `SharedSoulDemo` (entrenar/lesionar/+bond). Testing §21. Cierra la prioridad "bonds acumulables + conexión espaciotemporal". *Falta:* propagación PEREZOSA por era (hoy propaga a los cuerpos registrados en escena).
- **14-ago (PR #70):** **conversión + blend por DISTRIBUCIÓN**. `Soul/SoulMath` (`Remap` A/relativa y B/literal; `RescaleShape`; `Physical`/`Mental`/`All`). `SoulComposition` reescrito: el blend ahora es por **distribución** (cada arquetipo reescalado al presupuesto del primario → un 1% empuja la forma un 1%, ya no despreciable) + `ConvertTo(cuerpo,mente,modo)` (transformación/reencarnación: reexpresa la identidad actual en la base nueva; **B para reencarnaciones**) + `ReadStats`/`WriteStats`. Arquetipo `Ant`. Sandbox: `Ambrosio_Convert` (Toro+Bear) con HUD `SoulConvertDemo` (A/B/reset). Testing §20. *Siguiente:* `SharedSoul` perezoso (propagación de stats/bonds entre reencarnaciones).
- **14-ago (PR #69, doc):** **`soul-relations-reincarnation.md`** — diseño avanzado del alma: (1) blend/conversión por **DISTRIBUCIÓN** (una función reusable; tu fórmula simplifica a "reescalar cada arquetipo al presupuesto de la base × dominio"; sirve para blend Y transformación; decisión abierta A/B); (2) **relaciones por especie/karma** (`speciesBond` mezclables; base evolutiva foca↔oso−, perro↔humano+; `openness` para especies nuevas; → inclinación/thoughts/autoabandono); (3) **pensamientos por capacidad** (`floor(cap·dom/100)`, umbral entero; 2 pools; coste `K`); (4) **reencarnación por ALMA COMPARTIDA** (varios cuerpos = una anima; stats/bonds se propagan; lesiones→reinicios; nombres idénticos; **perezoso** por era). *Siguiente:* cerrar sabor de conversión → `SoulMath.Remap`.
- **13-ago (PR #68):** **alma por MEZCLA — FASE 1 (el motor)**. `Soul/BlendSlot` (arquetipo + dominio % +
  `shareDomain`), `Soul/Archetypes` (perfiles en código: cuerpos Human/Bear/Wolf/Bunny/Lion/Toro/Gallina/Mono +
  mentes Human/Bear/Lion/Rock/Fire/Agua/Mono + bonusPack1-4), `Soul/SoulComposition.Resolve()` (escribe las 12
  aptitudes: físicas←blend(cuerpos), mentales←blend(mentes) + Σ bonusPacks; + tamaño por altura mezclada). Sandbox
  `AlmaBlend_AUTO` (Panterilia Human90+Lion5, oso-mente-humana, oso+bonusPack3). Testing §20. **Nada migrado aún:
  esto es el MOTOR.** *Siguiente (fases 2-5):* migrar especies→cuerpos (reusar huir/manada de `Animal`), tono/
  thoughts a `Mind`, semilla de `FamilyGenerator` por blend, disolver `CompanionBase`, mente activa interrumpe body.
- **13-ago (PR #66, doc):** **`soul-composition-blend.md`** — arquitectura de **alma por MEZCLA**: cada ser se
  compone de arquetipos de **cuerpo** y **mente** con **dominio (%)** + `shareDomain`; stats/tono/thoughts finales
  = suma ponderada. Añadir/quitar arquetipos = híbridos/reencarnación/transformación. Reconcilia la corrección de
  arquitectura: **el diseño unificado (Anima raíz + pilares) está documentado pero a medio migrar**; `Animal` YA
  tiene huir+manada (las hormigas en `SimpleAnima` lo saltan — a corregir); especies = cuerpos; `CompanionBase`
  se disuelve; mente activa interrumpe body (vía Control/`IBrain`). Plan por fases + decisiones (arquetipos en
  código; array+shareDomain). *Siguiente:* fase 1 = `SoulComposition.Resolve` + `Archetypes` (blend de aptitudes).
- **13-ago (PR #65, doc):** **`microcosmos-level1.md`** — consolidación + **fichas del elenco** del Nivel 1
  (Sakshi/Ambrosio/Medea/Momo/Héspero/Ruth/Atlas + ancianos: alma·aptitudes·firma emocional·relaciones·impulsos·
  beat·tell). Decisiones: **emergencia dirigida** (los mueven stats+thoughts; el director solo siembra
  circunstancias/pensamientos, estilo `MobWorldDirector` — no marioneteo); **composición, NO clase `Ant`**
  (hormiga = `SimpleAnima` + IA del compañero + `Mind`/`SoulRecord`/emoción + ficha; el sistema unificador ya
  está: Anima raíz + pilares por composición + quark→elemento→compuesto→stat). Lista de impulsos sociales que
  faltan + beats + orden de construcción. *Siguiente:* aplicar fichas a las hormigas (coordinar con el compañero).
- **13-ago (PR #64):** **Forcejeo/Channeling unificados en `SpellBase` + `FireSpell`→`SpellBase` + sandbox**.
  `SpellBase`: dos bonos de poder — **forcejeo** (físico; sube solo al no lograr el efecto, `ReportResult`;
  persiste) y **channeling** (mental; sube al canalizar con `channelKey`/Shift, decae al soltar), `BonusPower` =
  suma escalada por aptitudes. `FireSpell` migrado a `SpellBase`: **Repeat** (mantener G = fuego múltiple, cada
  llama sube el forcejeo) + **Charge** (G+Shift = canalizar → soltar Shift dispara cargado); el bonus multiplica
  la intensidad. `WalkSpell` reescrito sobre los bonos (forcejeo al bloquearse, Shift=esprint) + `SpellDemoHUD`.
  Sandbox **`SpellDemo_AUTO`**. Testing §19g. *Falta:* detección de impacto real para el forcejeo del fuego;
  propagar forcejeo/channeling a `PullSpell`/`TransformationSpell`.
- **13-ago (PR #63):** **`CastMode` en `SpellBase` + `PullSpell` fusionada + `WalkSpell`**. (1) Arreglado el
  **choque de compilación**: había dos `class PullSpell` (Microcosmos + Transformation) → CS0101; borrada la
  huérfana (`Transformation/PullSpell.cs`, NavMesh, sin cablear). (2) `SpellBase` + `CastMode`
  (Instant/Repeat/Channel/Charge) + `PollInput()` opt-in + hooks `OnCast*` → mantener-tecla configurable por
  hechizo (fireball=Repeat, jalar/caminar=Channel, transformación=Charge/Instant). (3) `PullSpell` **fusiona**
  las dos verdades: forcejeo emergente por `ImpulseController` (Micro) + física por stats (tope = `force+Strength`,
  arranque = `max(force, masaObjetivo)`, sube solo si NO hay **progreso hacia el caster**, ambos gastan ATP ∝
  fuerza). (4) `WalkSpell` (contraparte a uno mismo: mínimo para mover el propio peso, sube si bloqueado, cansa).
  *Falta:* cablear `WalkSpell` a un sandbox; migrar `FireSpell`→Repeat / `TransformationSpell`→Charge si se quiere.
- **12-ago (PR #62):** **Sistema de hechizos Nivel 1 Microcosmos** (hormiga / debilitamiento).
  `SpellBase` (abstracta: `range`/`force`/`duration`, `CanCast`/`Cast`, `InRange`, `EffectiveForce` = fuerza+Strength−masa-target−resistencia).
  `WeaknessEffect` (hechizo de vejez; drena `currentEnergy` a ritmo fijo; deshabilita `NavMeshAgent` en 0;
  el drenaje sigue aunque se rellene la energía externamente; `Cancel`/`Resume`/`ApplyImmediate`).
  `PullSpell` ("Jalar": primer hechizo real del jugador; arrastra `ITarget` hacia el caster; física vs masa +
  velocidad-de-huida como resistencia; coroutine desactiva el agente durante el arrastre).
  `HoneydewSpell` ("Maleza de Ambrosio": **primer consumible del juego**; restaura `currentEnergy` vía
  `CharacterLevel`; auto-uso si no hay target; interactúa con `WeaknessEffect.Resume()`).
  `FormicAcidSpray` (habilidad corporal, **no magia**; AoE de estrés en radio; cargas limitadas con recarga
  gradual; respeta facción; tecla Q).
  `CombatTargetSelector` generalizado de `IngredientMob` a `ITarget` (backward compat: `CurrentIngredientMob`,
  `SelectMob`, `SelectAndOpenPalette`, `OnMobTargetChanged`; `GetSortedTargets` busca todo `MonoBehaviour`
  que implemente `ITarget`). **Sync**: ambos repos (GitHub + COLD-SANCTUARY).
- **10-ago (PR #61):** **topes de reservas derivados de los STATS** (no escala fija por santuario; stats-as-truth).
  `MagicReserves.EffectiveCapPerElement` = base × `MaxHealth/100` (materia ← resistencia+fuerza+masa) y
  `EffectiveEnergyCap` = base × `MaxMana/50` (energía ← razón+memoria), × niveles de alma (`CharacterLevel` opc.);
  `Store`/`StoreEnergy` ya los usan. Quarks sin tope (los limita lo reunido). Sandbox `Magia_AUTO`: botón "Subir
  stats" + lectura de topes efectivos. Doc §16, testing §19f. Cierra el pendiente de "escalado de topes".
- **07-ago (PR #60):** **hechizo de abastecimiento / "trasplante"** `SupplySpell` (generaliza el `ChimeraFeed`):
  el mago se llena de quarks/energía y los **introduce en otro personaje** (energía+elementos+quarks de sus
  reservas → las del objetivo). Sirve para quimeras, otro jugador, uno mismo y sobre todo **rol HEALER en la
  guerra**. Sandbox: `Magia_AUTO` + nuevo `Magia_AUTO_Objetivo` + botón «Abastecer objetivo». + doc §16 (abastecer
  = trasplante; regreso a S1 reconstruido + reparto de magos débiles→S4/fuertes→S1; osos entrenados por quimeras).
  Testing §19c. *Falta:* que abastecer quimera suba sus Humores+sacie Metabolism (domesticación); UI de objetivo.
- **07-ago (PR #58):** DECISIÓN (doc): **se descarta el gate de energía por física** (`energyPhysicsId` +
  `OnLearned`). El jugador **entra a cada santuario con la química/biología/física necesaria** (la aprende al
  terminar el anterior), así que la energía de la descomposición se capta **por defecto** (el campo queda como
  palanca opcional). Docs `magic-metabolism §14/§16` + `testing §19d`.
- **07-ago (PR #57):** **hechizos elementales basicos** `ElementalSpell` (agua/tierra/viento) con perfiles de
  coste distintos: agua (H2O ~1kg: 11%H+89%O + ~200J), tierra (SiO2 ~5kg: materia-pesado + poca energia), viento
  (materia 0 = aire gratis + ~450J = energia-pesado). Coste energia = 1/2 m v2. Junto al `FireSpell` completan los
  4 basicos que quedamos en probar. Anadidos al HUD `Magia_AUTO` (+ boton "Cargar reservas de prueba"). Doc §16,
  testing §19c. *Falta:* 1 hechizo por elemento + contras para los elementales del S4.
- **07-ago (PR #56):** **`Magia_AUTO` con `Anima` real**. `SimpleAnima` (Anima concreto minimo: hooks no-op, sin
  Awake/Start → seguro en sandbox) enchufado al `Magia_AUTO`; ahora comer sube stats via `Constitution` (gradual)
  y el exceso→grasa (`fatReserves`), ademas de rellenar las pools. `MagicSandboxDriver` muestra fuerza/masa/grasa.
  Testing §19a. (Topes derivados de stats: hecho PR #61.)
- **07-ago (PR #54):** **sandboxes de prueba** en `SampleSceneBuilder` (salen con `Build Sample Scene
  Blockout`): **`Descomposicion_AUTO`** (`DecompositionJob` workerCut=0 + `DecompositionMinigame` con batch
  agua/sal/CO₂ → al terminar la jornada suben Elements/Energy en el HUD de recursos; no necesita `Anima`) y
  **`Magia_AUTO`** (cápsula con `Metabolism`+`Constitution`+`MagicReserves`+`QuarkReserve`+`Grimoire`+`FireSpell`
  + `MagicSandboxDriver`, HUD OnGUI: aprender 1er hechizo → comer/quarks rellenan pools → lanzar fuego químico/
  masa-energía; sin `Anima`, null-safe). Guion en `testing-checklist §19`. *Falta:* `Magia_AUTO` con `Anima` real
  (grasa/stats), abastecer-quimera (Humores+Metabolism; base `SupplySpell` hecha). (Topes de stats: hecho #61.)
- **07-ago (PR #52):** **minijuego de descomposición** (`DecompositionMinigame`). La jornada corre 3 FASES sobre
  las MISMAS muestras y el MISMO orden: (1) **identificar** —elegir el nombre correcto entre nombres al azar,
  contra reloj—, (2) **romper** —calidad por timing 0..1 → energía capturada—, (3) **clasificar** —arrastrar
  cada componente a su contenedor; al acabar el tiempo se guardan los bien clasificados y pasa a la siguiente.
  Un componente cuenta en el `yield` si su muestra fue identificada+rota+bien clasificada (×calidad); al terminar
  vuelca `yield`/`energyJoules` en `DecompositionJob` y `Complete()` reparte economía+paga. Más rápido = más
  muestras/jornada = más progreso. Prototipo OnGUI. Doc §17. *Falta:* UI de arrastre real; meltdown; sembrar el
  batch desde la materia del área.
- **07-ago (PR #51):** **sustrato de quarks del S4 (`QuarkReserve`) + aclaraciones del modelo**. `QuarkReserve`:
  quarks crudos flexibles → `MakeElement` (quarks→gramos de cualquier elemento, para el MIX pre-crear/dinámico)
  y `Restitute` (quarks→energía, E=mc²) + `AtomsAvailable` (UI: átomos por elemento). Identidad **1 g ≈ 3·N_A =
  1,8×10²⁴ quarks** (independiente del elemento). + doc §16 (todo hechizo gasta elementos **y** energía; 2 vías
  de llenar energía: cocina-separación vs restitución; el MIX de pools flexibles; topes progresan → S4 debe
  poder crear una casa 10⁷ g; alimentar quimeras cuesta aparte —quarks/elementos+energía—; elementales = 1
  hechizo por elemento con perfiles de coste) + §17 (**minijuego de descomposición**: romper-por-timing +
  clasificar-por-carriles tipo Guitar-Hero → setea `yield`/`energyJoules` de `DecompositionJob`). *Falta:*
  casteo que tire de quarks al faltar elemento. (`DecompositionMinigame` PR #52; `SupplySpell` PR #60; topes de
  stats PR #61.)
- **07-ago (PR #50):** **`FireSpell` con coste físico REAL** + cálculo del S4. `FireSpell`: coste = potencia×
  tiempo → energía → **combustible** (C 85,7%/H 14,3%; O₂ del aire gratis) + ignición; presets `FireTier`
  (chispa 0,018g / lanzallamas 22g / aliento de dragón 2,2kg **inviable** por química). **Modo masa-energía**
  (S4): aniquila µg + paga toda la energía del pool (E=mc²). Pinta aura destructiva. + doc §15: **quarks del S4
  llenan LOS DOS pools** (bio `Constitution` + magia `MagicReserves`) y el excedente→energía; tabla de
  supervivencia (1 quimera ~10¹⁰J≈100 alientos al entrar; grupo ~10¹¹J≈1000 al terminar; 1 g de materia≈900k
  alientos → la masa NO es el cuello de botella, sí `energyCap`+maestría); quimeras (dragón/hidra/elemental) =
  los "osos" del minijuego de domesticación. *Falta:* escalar `energyCap` por nivel; enganchar Quimera a `PlayableCreature`.
- **07-ago (PR #49):** **trabajo de descomposición + coste de energía en hechizos**. `DecompositionJob`: al
  completar la misión de la cocina reparte el lote → **mayoría a la economía** (`SanctuaryResources` +enum
  `Elements`/`Energy`) + **`workerCut` de paga** al obrero (`MagicReserves`); la **energía solo la capta el
  obrero si conoce la física** del nivel (`Grimoire`/`energyPhysicsId`), si no va toda a la economía. Enum
  `DecompositionLevel` (catabolismo/ionización/fisión/aniquilación). **Coste materia+energía**: los hechizos
  declaran `cost`(elementos)+`energyCost`(J) y pagan con `MagicReserves.Pay(costs, energy)` (todo o nada);
  `TransformationSpell`/`PossessionSpell` ya lo usan. *Falta:* minijuego que rellene el lote (hecho, PR #52).
  (Gate `energyPhysicsId` por física: **descartado** — entras a cada santuario con la física necesaria, §14.)
- **07-ago (PR #48):** **pools de magia + primer hechizo + reserva de energía**. `Grimoire` (registro de
  hechizos aprendidos, `OnLearned`): el 1er hechizo `awaken-reserves` llama a `MagicReserves.Unlock()` → crea
  las pools vacías con tope (siembra H/C/N/O). `MagicReserves`: + reserva de **energía (julios)**
  (`energy`/`energyCap`/`StoreEnergy`/`PayEnergy`) — la que libera **descomponer** materia (química→nuclear→
  masa-energía), paga los hechizos grandes (bosones). + doc `magic-metabolism §14`: cocinas = trabajo
  (descomposición→energía+materia→economía+paga), física en la cocina, catabolismo/fisión/aniquilación (no
  "catalización"), revelación de energía gateada por física de otras áreas. *Falta:* `DecompositionJob`
  (reparte a `SanctuaryResources` + cut al obrero); gatear la absorción de energía por física aprendida.
- **07-ago (PR #47):** **comer rellena las reservas de magia** (`Metabolism`→`MagicReserves.Store`, hasta el
  tope, una vez `unlocked` por el 1er hechizo; el resto→grasa) → cierra el bucle comer↔lanzar. + doc
  `magic-metabolism §13`: coste REAL de hechizos con numeros (fuego: vela 0,018g/lanzallamas 22g/dragon 2,2kg;
  quimica da para lo pequeño, los grandes exigen nuclear/masa-energia E=mc2=90TJ/g), materia-vs-energia
  (bosones=energia), escalera de hechizos, niveles de energia (fisica en Mecanica/Forja), UI atomica.
- **07-ago (PR #46):** los hechizos **declaran coste y pagan de `MagicReserves`**: `TransformationSpell` y
  `PossessionSpell` tienen `cost` (List<ElementCost>) y llaman a `Pay` antes de actuar → sin reservas de ese
  elemento, el hechizo **no se lanza** (opt-in: sin `MagicReserves` es gratis). `magic-metabolism §7`.
- **07-ago (PR #45):** **reserva de magia + coste de hechizo** (`MagicReserves`/`ElementCost`: stock por elemento
  = lo que ve el jugador en la tabla; agotado → no lanza ese hechizo) + **clima/actividad mueven la necesidad**
  (`Metabolism`: frío→mas gasto de grasa/termogenesis → el oso craves grasa por el clima; esfuerzo→mas gasto).
  + doc `magic-metabolism-progression` MUY expandido: adiccion (RPE/tolerancia/contexto), nivel atomico,
  comer=combate de elementos, jerarquia completa de la materia (electrones=leptones aparte; bosones=fuerzas),
  minijuegos de desintegracion, organos como componentes (meso/micro, Da Vinci), trascender (espacio/dark/gravedad), MCP/fuentes.
- **07-ago (PR #44):** **la necesidad decide la depredacion** — `SelectPrey` sesga la presa por el `Craving`
  (x`Selectivity`: saciado exquisito/hambriento come todo) y `AbsorbFood` come mas de la parte que le falta
  (oso craves grasa -> foca -> blubber). + **`docs/magic-metabolism-progression.md`**: arco de progresion del
  hechizo de comer (comida->compuestos->elementos->quarks; luego invertir hacia super-celulas->quimeras;
  magia=reservas quimicas; adiccion). Cocinas por santuario (cocina/tienda/laboratorio/planta nuclear).
- **06-ago (PR #43):** **apetito por NUTRIENTES** (`Metabolism` reescrito, base cientifica): pools por nutriente
  (proteina/grasa/carb/minerales) que se agotan → deuda → `Appetite` (protein leverage) atenuado por leptina;
  `Craving` (lo que mas falta → que cazar/que parte, p.ej. oso polar→grasa) y `Selectivity` (saciado→exquisito/
  hambriento→come todo). Techo de utilizacion → stats; exceso → grasa. `AbsorbFood` reparte por material.
  Falta: `Craving`/`Selectivity` sesguen `SelectPrey`/parte; herbivoros/Eater/ElementFragment.
- **06-ago (PR #42):** **comer → absorber** enganchado (carnívoros): `Carnivore.Feed` llama a `Metabolism.
  AbsorbFood(nutrition, material)` (Meat/Fish→N, Fruit/Grass→C) → lo útil a `Constitution` (stats), el exceso a
  grasa; cierra el bucle jugable de la química/absorción. Falta: herbívoros/`Eater`/`ElementFragment` (mismo one-liner).
- **06-ago (PR #41):** **límite de absorción** (`Metabolism`, base científica leptina/ghrelina/set-point +
  techo de síntesis proteica): la absorción no tiene tope, la **utilización** sí (techo ~0.4 g/kg escalado por
  masa + adaptación por uso); **exceso → grasa (`fatReserves`), no stats** → no se come todo el día para
  hacerse fuerte. `Appetite` = hambre(ghrelina) − reservas(leptina). `Absorb`→`Constitution`. `stats-as-truth §9`.
  Falta: enganchar comer/`ElementFragment`→`Absorb`.
- **06-ago (PR #40):** `Constitution` enganchada a **`Chemistry`** — los elementos del Nivel 1 son ahora
  **símbolos reales de la tabla periódica** (validados contra `PeriodicTableManager.GetData`) y **alimentables**
  (`AddElement(symbol, delta)`) desde el juego (absorber/comer elementos → cambia la constitución → mueve los
  stats base). `stats-as-truth §9`. Falta: que `ElementFragment`/comer llame a `AddElement`; recetas reales.
- **06-ago (PR #39):** **química como fundamento (por niveles)** — `Constitution`: elementos→compuestos→
  células→stats base (glóbulos=Fe+proteína, etc.), delta gestionado, neutro por defecto. + **hechizo
  inerte→vivo** (`CharacterComposition.Animate`) + **aura energiza los componentes propios aun inertes**
  (×(1+magicAura)). `stats-as-truth §9`. Falta: elementos desde `Chemistry`; aura→aliados (contagio).
- **06-ago (PR #38):** **tejido vivo (doble sentido)** — `CompositionPart.living`: vivo = modulado por el
  huésped + injerto progresivo (miembro/ropa-viva); rígido = aporte plano (metal/coraza). + **dirección
  documentada** (`stats-as-truth §9`): la **química como fundamento** (2 capas: `Humores` transitoria→emoción/
  pensamientos ✔; `Constitution` estructural→stats base, por hacer). Química→pensamientos ya parcial vía `Mind`.
- **06-ago (PR #37):** **composición fase 2** — `CharacterComposition` + `StatBonus`: las partes aportan un
  **delta gestionado** de stats sobre la constitución (campos de `Anima`) **sin pisar evolución/transformación**;
  aporte **biológico modulado por la vitalidad del huésped** (el mismo brazo rinde distinto según el cuerpo) e
  **injerto progresivo** (`adaptSpeed`: converge/decae). Base/efectivo resuelto de forma aditiva y segura.
  Fase 3: `BodyPartStats`, miembros como assets, base desde `Humores`/`Chemistry`.
- **06-ago (PR #36):** **composición fase 1** — `CharacterComposition`+`CompositionPart` (`Assets/Scripts/
  Composition/`): partes slotables (adornos/ropa) → activan su visual y la **defensa de la ropa suma a
  `Anima.armadura`** (que `Predation` lee → vestir armadura = peor presa). `Equip`/`Unequip` por slot; reutiliza
  `ClothingSlot` (ampliado con Hair/Eyebrows/Eyes) y `ClothingRecipe`. Cierra el runtime que faltaba a la ropa.
  **NO toca el modelo de stats** (base/efectivo = fase 2; miembros con stats = fase 3).
- **06-ago (PR #35):** afinar emoción — **freeze/inmovilidad tónica** (`BodyPartReactor.freezeOnFear` + `Emotion
  Expression.Fear`: el miedo intenso congela la parte, conejo/ciervo) + **contagio emocional** (si hay
  `ThoughtField`, la emoción intensa lo proyecta con tono/humor por cuadrante → los `Mind` cercanos lo cogen).
  El diálogo interno ya sale emocionado vía humores compartidos (`Mind.Think`). Sandbox: `Conejo_freeze` + field.
- **05-ago (PR #34):** manada por especie — `Predation.EffectivePower` ahora pondera a los aliados por el
  **`PackFactor` de la propia especie** (ya calibrado: Lobo 0.8/Malamute 0.9/Oso 0.3/Zorro 0.2/Conejo·Ciervo·
  Ballena 0) en vez de un 0.5 fijo; alinea con la convención de `Animal.DecideReaction`.
- **05-ago (PR #33):** **remate de depredación** — **manada** (`Predation.EffectivePower`: suma poder de aliados
  por facción en radio → `SelectPrey`/`EvaluateThreat` dinámicos: lobo solo no puede con el oso, la manada sí;
  el oso evita al lobo con manada) + **aura mágica** (`Anima.magicAura` firmado + `MagicAura` que decae:
  − destructiva → más temido; + benevolente → bonds fáciles vía `GrowBond`). Falta: que la magia llame a `Register*`.
- **05-ago (PR #32):** **depredación por stats** (`stats-as-truth §2`): `Predation` (`Assets/Scripts/
  Transformation/`) + `Anima.armadura` (textura/coraza). `Diet.SelectPrey` no caza lo invencible; `Animal.
  EvaluateThreat` escala la amenaza por poder de stats. El **tamaño invierte presa↔depredador**; el **farol**
  de transformación no engaña (no cambia stats), la **real** sí. Falta: manada + aura mágica.
- **05-ago (PR #31):** **transformación por combate de stats** (`stats-as-truth §4`): `TransformationSpell` +
  `StatProfile`/`TransformPreset` (`Assets/Scripts/Transformation/`). `Cast` → Failed / VisualOnly (farol,
  conserva stats) / Full (cuerpo+stats) según potencia vs coste (resistencia+inyectado por `Might`); revert
  por duración; bidireccional. Se cablea en `Anima`s reales (sin sandbox: Anima abstracta). Pendiente: ligar a
  energía del hechizo + que la depredación "huela" el resultado.
- **05-ago (PR #29):** **legibilidad** — `EmotionReader` lee la orquesta de otros `Anima` (siente/quiere/hará
  + aproximabilidad; alcance/detalle por **percepción**) → base del lector-de-mentes y del vínculo con crías.
  Sandbox `EmotionReader_AUTO`. Cierra la emotion-slice (Laban/frases/legibilidad).
- **05-ago (PR #28):** emoción — cualidades **Laban** (`Quickness/Heaviness/Boundness` desde el circumplex →
  el *cómo* del movimiento) + etiqueta `Emotion` + `BodyPart` gana orejas (`EarLeft/EarRight`) + **tabla
  por-especie** con NUESTROS animales (Oso/Lobo/Zorro/Malamute/Conejo/Ciervo/Foca/Ballena/Pájaro; etología
  real → parte + rol, incl. freeze/inmovilidad tónica) — `emotion-model.md §4c`.
- **05-ago (PR #27):** **emotion-slice (orquesta)**: `EmotionExpression` (conductor: `Humores`→valencia/
  activación/tensión+`Jolt`, sesgo por aptitudes) + `BodyPartReactor` (instrumento: reacción pasiva/violenta
  por parte, traducción entre especies orejas↔brazos↔antenas↔alas) + aptitudes nuevas en `Anima`
  (`afabilidad`, `sensibilidad`) + `UpaYogaSession` resuelve huesos vía `CreatureRig`/`BodyPart`. Sandbox
  `EmotionOrchestraSandbox_AUTO`, `EmotionReader_AUTO`. (`docs/emotion-model.md §4b`.)
- **05-ago (PR #26):** **`docs/emotion-model.md`** — modelo de emoción para **toda Anima** con base científica
  (circumplex=`Humores.Positividad×Energia` ya en código; Laban→`CreatureRig`; Big Five→aptitudes; etología/
  Darwin→señales animales+legibilidad). Aptitudes nuevas propuestas (`afabilidad`; promover `sensibilidad`).
  La **firma emocional = tell de reencarnación**. (Doc; código = `EmotionExpression` a continuación.)
- **05-ago (PR #25):** **fuente única de partes**: se retira `RigPart` y se **amplía el `BodyPart` de Asana**
  (0–7 = regiones de yoga intactas; + huesos finos L/R + insecto/quimera). `CreatureRig` tira de ella;
  `bodyStats[8]` sigue igual (los huesos finos no se indexan → `GetBodyPartStats` vacío por el guard). Base
  para la emotion-slice (`docs/stats-as-truth.md §8`).
- **05-ago (PR #24):** **`ScreenEffects`** (cámara artística: tintes por estado sueño/fatiga/estrés/
  satisfacción + `Fade` que completa el blackout de `CameraManager`) + **hotfix:** el `enum` de `CreatureRig`
  colisionaba con `BodyPart` de Asana → renombrado (y en **#25 unificado** en una sola `BodyPart`) (master #23
  no compilaba). Sandbox
  `ScreenEffectsSandbox_AUTO`, `EmotionOrchestraSandbox_AUTO`, `EmotionReader_AUTO`.
- **05-ago (PR #23):** **`docs/stats-as-truth.md`** (la ley única: `stats→frases→todo`; hechizo=modo/energía=
  timer; transformación por combate de stats con farol-vs-real; composición por componentes tipo CodeShip;
  stats bidireccionales; Quimeras) + **`CreatureRig`** (parte lógica→hueso; auto `HumanBodyBones` humanoide /
  manual insecto — un motor de yoga/emoción para cualquier cuerpo).
- **03-ago (PR #22):** upa-yoga **(a)** — rebind del input (`HeadLook`/`PlayerController` ceden a
  `UpaYogaSession.Active`, como con `TypingChallenge`) + **hooks de rig** null-safe (neck/hombros; el driver
  rota el hueso asignado con las teclas + jitter de temblor, no-op si vacío) + refinos (tileMode Grow/Fall;
  Grow muestra elemento; temblor **localizado** en la parte que se mueve).
- **31-jul (PR #21):** **nivel introductorio canónico** (`microcosmos-insects.md` §13: historia reescrita
  —Sakshi cría al pulgón Ambrosio, la cueva, la muerte—, mapa de almas y **reencarnación** con *tell*, **armas
  químicas/veneno**, género neutro) + **`novela.md`** (narración íntegra, semilla de la novela del juego) +
  **`SoulRecord`** (ficha de alma) + **1ª virtualización de yoga** (`UpaYogaSession`: upa-yoga de cuello,
  **control-por-partes** + **ritmo Guitar-Hero** con fichas/aliento/temblor; `upa-yoga-mission.md`).
- **30-jul (PR #20):** **El Perro de Oberkassel** + animales-héroe (Togo/Hachikō/Cher Ami, POV animal);
  **Enfermería "Una Salud"** (personas+animales); **Microcosmos INSECTO** (opción B: hormigas, mirmecofilia/
  pulgón, feromonas=su "fuego", hongos); **1ª misión del micro** (cueva + pulgón-guía + banda de 7 hormigas).
- **30-jul (PR #19):** **área de CRÍA** (rutina de cuidado enganchada a drives reales; El Perro fundacional) +
  **prólogo Enfermería** (pre-fuego, La Recolectora) + **camión** (1ª reparación = cambio de rueda) + reorden
  (Cría=3). Fixes del equipo: `PickBest`/`GuidedTour`/`UnityEvent`.
- **30-jul (PR #18):** **Mecánica** (arranque limpiar→abastecer→reparar) + **Forja del Micro**;
  **dispatch/tickets** (`RepairTicket`/`ServiceHub`/`Toolbox`; 1ª reparación real = **grifo que gotea**);
  **Construcción** (arranque + El Tallador); reorden Construcción→Mecánica; 1ª persona reconciliada
  (`AutoCameraZone`/`HeadLook`).
- **29-jul (PR #17):** **motor de virtualización** (`VirtualPointer` mira central, `StationPart`,
  `ProductionOrder`, `TypingChallenge`/mecanografía, `StockingTask`/abastecer); cocina paso A (limpieza+paseo) y B (desayuno).
- **29-jul (PR #16):** **controlador intercambiable + posesión** (`AnimaController`/`IBrain`/`PlayerCore`/
  `FollowBrain`/`HelpRequest`); **pensamientos escalables** (weight/lifecycle/gate); históricos
  (Guardián/Sembradora/Alfarero/Nasatya); nombres (Ötzi→Nasatya).
- **28-jul (PR #14/#15):** migración a clase única **`Anima`**; **pilar Mente** (frases/campos/pools/reparto
  Estricta-Libre) + `anima-architecture §11`.
- **23-jul:** cocina jugable (Micro); limpieza legacy (`KitchenScale`/`Kitchen*`, `IAnimal`/`IVital`); tríada
  Micro/Meso/Macro; diseño del mundo grande (`world-topology`); backbone de recursos + HUD.
- **09/10-jul:** auditoría + sync de 18 docs; retirada del disparo (no-violencia); **12 aptitudes** + perfiles;
  medios tierra/agua/aire; evolución de aptitudes; dietas/territorialidad; `FishSchool`; Malamute.
