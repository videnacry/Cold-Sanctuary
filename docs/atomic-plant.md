# Planta atómica (Mesocosmos) y su microcosmos de la física

> Hilo de la planta atómica, 2026-10-22. Código: `Assets/Scripts/AtomicPlant/`, builders
> `Assets/Editor/SampleSceneBuilder.AtomicPlant.cs` (área) y `Assets/Editor/PhysicsSceneBuilder.cs` (microcosmos).
> Encaja con `spell-genesis-and-sanctuary-progression.md` §5.1 (la planta evoluciona con la historia de la física) y
> `historias-por-area.md` §3 (la escalera de la física vive aquí).

## 1. Qué es (y qué no)

Una planta atómica en un santuario aislado y solar no es una central eléctrica: es un **reactor de investigación de
piscina tipo TRIGA**, como los de las universidades (UT-Austin, 1,1 MW; Penn State, 1 MW). Para qué sirve aquí:

- **Isótopos médicos para el hospital** (no hay otro a miles de km): Mo-99 → generador de Tc-99m.
- **Análisis por activación neutrónica** (qué elementos lleva una muestra: pelo de foca, agua, suelo).
- **Enseñar física**: el arranque del reactor es la mejor clase de física nuclear que existe.

La electricidad sigue siendo solar. La descomposición a quarks del nivel S4 (`spell-genesis` §4) se apoyará más
adelante en esta planta; esta entrega cubre los peldaños 2–4 de la historia (Curie → fisión → reactor).

## 2. Interactuables (prioridad sobre decorado)

Todo lo que hay en el área se usa (F). No hay objetos solo de adorno.

| Equipo | Qué haces | Física que enseña |
|---|---|---|
| Taquilla de dosímetros (`DosimeterRack`) | coger/devolver el dosímetro (lees tu dosis) | sin dosímetro no se opera nada; límite 20 mSv/año |
| Consola (`ReactorConsole` + `ResearchReactor`) | rearmar, sacar barras, subir a potencia, automático, SCRAM, bomba | subcrítico con fuente → crítico → periodo → realimentación negativa |
| Puente de la piscina (`PlantEquipment.PoolBridge`) | mirar el núcleo a 7 m bajo el agua | brillo azul de Cherenkov (sube con la potencia); el agua modera, enfría y blinda |
| Conejo neumático (`PneumaticTransfer`) | mandar una cápsula al núcleo y recuperarla | activación A = Nσφ(1−e^(−λt)) |
| Celda caliente (`HotCell`) | cargar el generador de Tc-99m · medir con detector de germanio | semividas y rayos gamma propios de cada isótopo |
| Purificación (`PlantEquipment.Purification`) | cambiar resinas si la conductividad pasa de 2 µS/cm | química del agua del reactor |
| Bomba e intercambiador | arrancar/parar el primario | dos circuitos que nunca se mezclan; agua de mar polar de foco frío |
| Torre de refrigeración | revisión diaria del secundario | evaporación y purga |
| Pórtico de contaminación (`ContaminationMonitor`) | medirte al salir, lavarte | irradiar ≠ contaminar |
| Cámara de niebla, contador Geiger | ver trazas, medir tasa | el fondo natural; 1/r² |

Distribución: el reactor y sus equipos en la mitad oeste; consola y celda caliente al este; entrada (+Z) con taquilla y
pórtico a los lados del paso de la escalera; cada equipo con ≥ 1 m de paso alrededor. La piscina es un pilar de
hormigón que baja hasta el suelo (el núcleo queda a ~7 m bajo el agua, como en los TRIGA de 7,3–8,2 m): la plataforma
sigue a 12 m como el resto de áreas. Ubicación: lóbulo «Sur:Rotacion» del casquete Sur, (15, −101).

### Modelo del reactor (`ResearchReactor`)

- Cinética puntual, un grupo de precursores, salto inmediato: `P = (λS + q)/(β − ρ)`, `dS/dt = βP − λS`;
  β = 0,0073, λ = 0,0767 s⁻¹, fuente de arranque q = 1e-3 W·Δk (subcrítico ≈ 0,02 W con todas las barras dentro).
- Exceso del núcleo 0,022 Δk/k (~3 $). Barras: regulación 0,008, shim 0,020 ×2, transitoria 0,017 (curva en S).
- Coeficiente de temperatura del combustible −8e-5 Δk/k/°C; a 1 MW el combustible está ~200 °C sobre la piscina.
- Piscina ~270 m³ (1,13e9 J/°C): sin bomba sube ~3 °C/h por MW. Intercambiador UA = 7e4 W/°C con agua de mar a 4 °C.
- Disparos: > 1,1 MW, combustible > 550 °C, piscina > 48 °C, ρ ≥ 0,95 β, manual. Rearme solo con barras dentro.
- Valores de diseño de juego cuando la fuente no da la cifra (rod worths, coeficiente, constantes térmicas): orden de
  magnitud de un TRIGA, no los de una instalación concreta.

### Isótopos (`PneumaticTransfer`, `HotCell`, `MedicalIsotopes`)

- Mo-98 (24,39 % del Mo, σ = 0,13 b) → Mo-99 (T½ 65,94 h). Saturación de 1 g a 1e13 n/cm²·s ≈ 2,0e9 Bq.
  Una gammagrafía ósea ≈ 740 MBq de Tc-99m. `MedicalIsotopes.Generators` es lo que leerá el hospital.
- Pelo de foca: Cl-38 (1642,7 keV, 37,2 min), Na-24 (1368,6 keV, 14,96 h), K-42 (1524,6 keV, 12,36 h), Hg-203
  (279,2 keV, 46,6 d), Se-75 (264,7 keV, 119,8 d), Zn-65 (1115,5 keV, 244 d). Los pesos por elemento son orientativos.
- Tiempo comprimido en la irradiación: 1 s real = 10 min.

## 3. Microcosmos: Newton, Curie, Einstein (`Microcosmos_PlantaAtomica`)

La planta ×10 (la piscina es una montaña) y, en el hueco de su suelo, tres estaciones jugables (E junto al maestro):

1. **Newton — el cañón de la montaña** (`NewtonCannonLesson`): ↑/↓ velocidad, E dispara. Trayectoria con la gravedad
   real (GM = 3,986e14) sobre una Tierra a escala: cae, órbita (~7,8 km/s a 200 km) o escapa (energía ≥ 0). Se
   completa al lograr una órbita.
2. **Marie Curie — el electrómetro y el cuarzo** (`CurieElectrometerLesson`): 7 muestras con las corrientes de su
   tabla de 1898 (×1e-12 A: uranio 24, óxido negro 27, torio 53, calcolita artificial 9, natural 52, pecblendas 67 y 83).
   Mantén ESPACIO para soltar la carga del cuarzo y que el punto de luz siga en cero; I = Q/t. Se completa al medir
   uranio, las dos calcolitas y una pecblenda: la pista del elemento nuevo.
3. **Einstein — la balanza de la fisión** (`EinsteinMassLesson`): U-235 + n frente a Ba-141 + Kr-92 + 3n;
   faltan 0,186 u → 173 MeV (≈ 200 MeV con las desintegraciones); 1 g de U-235 ≈ 2,8 t de carbón.

Al completar las tres: `HistoryCard` con la historia y fuentes + 60 XP (marga Stats). El elenco de la enfermería
renacido escucha alrededor. **Eratóstenes** (primer tramo de la física) lo construye el hilo «Microcosmos de
historias» en `Microcosmos_Eratostenes`; la máquina de la planta lo ofrece en cuanto la escena existe.

## 4. Misiones de trabajo (`AreaWork`, área «Planta Atómica», no obligatorias)

`pla_operar` (arrancar, subir, parar) · `pla_isotopos` (molibdeno → generador) · `pla_ronda` (resinas, bomba, torre,
Geiger). Pasos y anclas en `VirtualMissions`.

## 5. Fuentes

- UT-Austin NETL, «TRIGA facilities» (nuclear-twins.tacc.utexas.edu/facility): 1,1 MW, tanque de 8,2 m, 4 barras
  (regulación, 2 shim, transitoria), pulsos hasta 2,2 % Δk/k.
- Penn State Breazeale Reactor (rsec.psu.edu): 1 MW, piscina de 24 ft (~71 000 gal), flujo 1e13 n/cm²·s en el borde.
- M. Skłodowska Curie, Comptes Rendus 126, 1101–1103 (1898), trad. C. Giunta (web.lemoyne.edu/giunta/EA/CURIEann.HTML):
  condensador de 8 cm, 3 cm, 100 V; tabla de corrientes; conclusión sobre la pecblenda.
- Musée Curie / Google Arts & Culture: el método del cuarzo piezoeléctrico (cuarzo de Pierre y Jacques Curie, 1880).
- A. Einstein, Annalen der Physik 18, 639 (1905); L. Meitner y O. Frisch, Nature 143, 239 (1939).
- Masas atómicas (AME2020); semividas y energías gamma (NNDC); ICRP 103 (20 mSv/año).

## 6. Pendiente

- Que el hospital lea `MedicalIsotopes` (gammagrafías) y que una NPC opere la consola en su turno.
- Pulsos del TRIGA (cinética completa, sin salto inmediato) para la demostración de seguridad inherente.
- Fermi y la pila de Chicago (1942), Rutherford, Becquerel: más peldaños de la escalera.
- Ropa de época propia (peluca de Newton, vestido de Curie): hoy usan el abrigo de `Wear1940s`.

## 7. Planta de arriba: el ciclotrón médico (`Cyclotron`, `CyclotronConsole`, `FdgSynthesis`)

> Berón, 2026-10-23: «¿acelerador de partículas en la planta atómica?». Un acelerador grande (sincrotrón, LHC) no cabe
> en el santuario; lo que tienen los hospitales es un **ciclotrón compacto para el PET**, y complementa al reactor:
> el reactor da isótopos ricos en NEUTRONES (Mo-99 → Tc-99m); el ciclotrón, ricos en PROTONES (F-18).

- **Dónde**: un piso apilado sobre la planta (`Planta_ciclotron`, `StackOn`, 24 × 15 m), con su escalera. Ciclotrón
  autoblindado al oeste, consola en medio, celda de síntesis de FDG al este, unidas por la línea blindada del blanco.
- **La máquina** (tipo GE PETtrace): iones H⁻ de una fuente PIG interna; campo medio ≈ 1,8 T; RF 27,2 MHz (protones);
  «Des» a ≈ 35 kV; protones de 16,5 MeV; extracción por lámina de carbono (arranca los electrones, la carga cambia de
  signo y la órbita se abre hacia el blanco). Autoblindaje pegado a la máquina (como los modelos de hospital): fuera se
  mide poco aunque dentro haya neutrones.
- **Física que enseña** (calculada en `Cyclotron`): f = qB/2πm ≈ 27,4 MHz a 1,8 T (no depende de la velocidad);
  radio final r = p/qB ≈ 0,33 m; ≈ 236 vueltas (2 × 35 keV por vuelta).
- **Producción**: ¹⁸O(p,n)¹⁸F sobre agua enriquecida (> 97 %). A = Y_sat·I·(1 − e^(−λt)), Y_sat ≈ 8,3 GBq/µA,
  T½ = 109,77 min. A 60 µA y 2 h ≈ 265 GBq al final de la irradiación; más de 2–3 h apenas suma. Tiempo comprimido
  como el conejo neumático (1 s real = 10 min).
- **FDG**: síntesis automática de ~25 min con rendimiento ~60 %; una PET de adulto ≈ 370 MBq. Los lotes van a
  `MedicalIsotopes.Fdg` para el hospital (caducan en horas: por eso se hace al lado).
- **Misión**: `pla_ciclotron` (Planta Atómica, también Hospital): dosímetro → arrancar el haz → parar y mandar el F-18
  → sintetizar FDG.
- **Pendiente**: que el hospital haga PET con `MedicalIsotopes.Fdg`; un acelerador lineal o un sincrotrón pequeño para el
  peldaño de los quarks (S4).

Fuentes del §7: WBIC Cambridge, «PETtrace» (wbic.cam.ac.uk/pettrace): 1,8 T, 27,2 MHz, 35 kV, 16,5 MeV, láminas de
carbono; GE HealthCare, PETtrace 800 (capacidades de F-18 por 2 h); IJRR (ijrr.com, a_id=17), blanco de agua de 18 MeV:
saturación ≈ 8,3 GBq/µA; IBA Cyclone KIUBE (18 MeV, autoblindado); NNDC (T½ del F-18).
