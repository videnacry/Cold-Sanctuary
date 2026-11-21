---
resumen: "Rama claude/ui-codeship (2026-11-18). Port a C#/IMGUI de CodeShip (origin/integracion), según el plan plan-microcosmos-realismo.md §4. Peticiones: PB-161 (UI) y PB-163 (misiones)."
temas: [ui, microcosmos, jornada]
estado: vigente
codigo: [MissionLibrary, CodeChallenges, MissionSites, ChoreLibraryTest, AreaFunctionSite, AreaFunctionStation, CodeChallengeSheet, CodeShipLogicTest]
---
# CodeShip en los microcosmos: UI y misiones por retos de código

Rama `claude/ui-codeship` (2026-11-18). Port a C#/IMGUI de CodeShip (`origin/integracion`), según el plan
[plan-microcosmos-realismo.md](plan-microcosmos-realismo.md) §4. Peticiones: PB-161 (UI) y PB-163 (misiones).

> **Estado (2026-11-19):** Berón decidió rehacer el microcosmos de Musk desde cero. La biblioteca y la UI son genéricas y se quedan; las
> **ligaduras de Hawthorne** (`MissionSites`, escena `Microcosmos_ElonMusk`) y los dos retos de la historia (`CodeChallenges`) dependen de
> los nombres de la escena vieja: al rehacerla, hay que revisarlas o borrarlas.

## Código (`Assets/Scripts/CodeShip/`)

| Pieza | Archivo | Qué hace |
|---|---|---|
| Reto de código | `CodeExercise.cs` | Ordenar (Parsons) o elegir; pseudocódigo/JS; `CodeAttempts` (repetir un fallo no cuenta) |
| Hoja del reto | `CodeChallengeSheet.cs` | Hoja inferior IMGUI; reto suelto o tarea (`OpenRun`: reto i de n, fallos k de 2, Reintentar); se recoge mientras se anima el paso |
| Retos de la historia | `CodeChallenges.cs` | Catálogo por escena; `Inject` mete pasos `StoryMission.Kind.Code` tras su paso |
| Tareas e «Ir» | `TaskBoard.cs`, `AutoTravel.cs`, `MicroTaskWindows.cs` | Ventana Tareas (F6), viaje por NavMesh/alfombras con tope y «Parar», teletransporte opcional |
| Áreas | `AreaCatalog.cs` | Ventana Áreas (F7) |
| Menú árbol | `MicroRootMenu.cs` | «☰ Menú» (F5): Misiones, Moverse, Trabajar, Necesidades, Hablar, Ventanas |
| HUD | `ContextColumn.cs`, `HudNotices.cs`, `MicroOnboarding.cs` | Acción principal abajo al centro (R) y marcador del destino; avisos arriba; guía de inicio |
| Misiones de puesto | `Chores/ChoreRun.cs` | `ChoreTemplate`, `ChoreRun` (2 fallos), `IdeaDuel`, `DifficultyProfile` (curva de CodeShip), `ChoreKit.StepChain` |
| Biblioteca | `Chores/MissionLibrary.cs`, `MissionLibrary.Hawthorne.cs` | Las plantillas (abajo), con fuente |
| Ligaduras | `Chores/MissionSites.cs` | Qué plantilla va en qué mueble de cada escena |
| En la escena | `Chores/StationMissions.cs` | Liga cada escena cargada, abre la tarea, da experiencia, la repite al día siguiente; marcador «(supuesto)» si falta el mueble |
| Animación | `Chores/StepAnimator.cs` | Regla §4.2: cada paso acertado, animado en el puesto (gestos de `PlayerTasks`, yoga, paseo del perro, tumbarse) |

## Plantillas (`MissionLibrary`, 43)

| Grupo | Plantillas (id · clase) |
|---|---|
| Comida | `cocinar-huevos-revueltos`, `cocinar-ensalada`, `cocinar-tortilla-papa`, `cocinar-chapati`, `cocinar-tacos-carne-asada` · cocinar; `camara-frigorifica`, `inspeccion-sanitaria` · seguridad-alimentaria; `recibir-proveedor` · recepcion |
| Salud | `atender-herida`, `atender-golpe`, `atender-estres-agudo` · atender-paciente; `medico-tension` · medico; `yoga-upa-cuello`, `yoga-saludo-al-sol` · yoga; `ejercicio-intervalos-4x4`, `ejercicio-fuerza-4x8`, `ejercicio-bici-30` · ejercicio; `dormir-catre` · dormir; `asearse-manos`, `gel-manos` · asearse; `sala-limpia-vestirse` · sala-limpia |
| Animales | `perro-alimentar` · alimentar-perro; `perro-jugar` · jugar-perro; `perro-programar-bordillo` · programar-animal |
| Campo | `campo-regar` · regar; `campo-curar` · curar-planta; `campo-cosechar` · cosechar |
| Taller | `mecanizar-torno` · mecanizar; `avionica-caja` · avionica; `composite-cofia` · composite; `reparar-brida-merlin`, `reparar-coche-aceite` · reparar-maquina; `fsw-tanque-falcon9` · soldar-fsw |
| Ciencia | `ensayo-traccion`, `microscopio-soldadura` · ensayo-materiales; `datos-prueba-merlin` · analizar-datos |
| Seguridad | `garita-tarjeta` · control-acceso; `incendios-extintor`, `incendios-hidrante` · incendios; `cruzar-crenshaw` · seguridad-vial |
| Ideas | `idea-reutilizar`, `idea-secuencia` · plantar-idea (duelo con la curva de dificultad); `pista-hawthorne` · aeronautica |

Las genéricas (cocinar, curar, yoga, ejercicio, perro, campo, dormir, asearse, sala limpia, comida segura, incendios…) valen para cualquier
microcosmos. Las de la fábrica de 2008 (FSW, Merlin, cofia, Crenshaw, la pista) son de Hawthorne.

## Ligaduras de Hawthorne (`MissionSites`, escena `Microcosmos_ElonMusk`)

| Mueble (nombre exacto o, si no, clave/edificio) | Plantillas |
|---|---|
| `Cocina de la nave (supuesto)` (clave `cocina`/`fogon`) | huevos revueltos, tortilla, chapati |
| `Catre de los turnos de noche (supuesto)` | dormir-catre |
| `Lavamanos` | asearse-manos |
| `Botiquín de primeros auxilios` | atender-herida, atender-golpe, atender-estres-agudo, medico-tension |
| `Torno de control numérico` | mecanizar-torno |
| `Banco de aviónica` · `Banco de composite (la cofia)` · `Banco del Merlin 1C` | avionica-caja · composite-cofia · reparar-brida-merlin |
| `Cubículo de Elon Musk (con los pósteres de Marte)` · `Pizarra: la secuencia del vuelo 4` | idea-reutilizar · idea-secuencia |
| `Rincón de gimnasio` (clave `muneco`) | los tres de ejercicio |
| `Esterilla de yoga` (clave `esterilla`) | los dos de yoga |
| `Plancha de los tacos` · `Lonchera (camión de comida)` | cocinar-tacos-carne-asada |
| `Banco del mecánico` (edificio Taller) | reparar-coche-aceite |
| frutales (`MicroPiece.kind = frutal`) | campo-regar, campo-curar, campo-cosechar |
| `Cochinilla hormiguera` | los tres del perro |
| `Soldadura por fricción (FSW): tanque del Falcon 9` | fsw-tanque-falcon9 |
| `Laboratorio de materiales` | ensayo-traccion, microscopio-soldadura |
| `Sala de datos de pruebas (propulsión)` | datos-prueba-merlin |
| `Vestuario de la sala limpia` · `Dispensador de gel hidroalcohólico` | sala-limpia-vestirse · gel-manos |
| `Cámara frigorífica de la cocina` · `Tarjeta de inspección sanitaria (A)` · `Furgoneta del proveedor de comida (supuesto)` | camara-frigorifica · inspeccion-sanitaria · recibir-proveedor |
| `Garita de seguridad (1 Rocket Road)` · `Bomberos` · `Hidrante` · `Cruce de Crenshaw Boulevard y Rocket Road (semáforos)` | garita-tarjeta · incendios-extintor · incendios-hidrante · cruzar-crenshaw |
| `Aparcamiento de SpaceX` | pista-hawthorne (el aeropuerto es fondo) |

## Cómo poner misiones en una escena nueva

1. **Construir los muebles con un nombre fijo** (el `GameObject.name`; en los StorySceneBuilder, el rótulo de `Piece`/`Spot`) y, si es un
   puesto, su `AreaFunctionStation.station`.
2. **Ligarlos** en `MissionSites.Build()`: `_scenes["<NombreDeEscena>"] = new List<Binding> { N("<plantilla>", "<nombre exacto>"), … }`.
   Un `Binding` busca por `names` (exacto, luego contenido), `buildings` (`AreaFunctionSite.building`) y su puesto, `stationKeys`,
   `pieceKinds`; si nada aparece, pone el marcador `fallback` junto a `anchor` + `offset` y lo avisa en el registro.
3. **Plantillas nuevas** en `MissionLibrary` (o un `partial` como `MissionLibrary.Hawthorne.cs`): `Add(id, clase, grupo, título, escenario,
   fuente, retos)`. Un procedimiento real va con `StepChain` (un reto por paso; `{bien|mal|mal}` pide el valor con su unidad); si no,
   `Order`/`Choice`. Toda plantilla lleva fuente; lo adaptado, `label = "licencia"`.
4. **Una clase nueva necesita su animación** en `StepAnimator.MotionFor` y en `Known` (lo comprueba CS-95).
5. **Retos de la historia** (pasos `Kind.Code`): `CodeChallenges.Build()`, con la escena, el paso tras el que van y el sitio.
6. **Probar:** añadir sus checks a `ChoreLibraryTest` y registrar los casos en `docs/qa` (casos.md, registro.md; PB en peticiones.md);
   `tools/qa-check.sh` debe dar 0 problemas.

## Pruebas

| Grupo | Unidad | Casos | Qué |
|---|---|---|---|
| 119 | `CodeShipLogicTest` | CS-01…CS-42 | Retos, intentos, barajado, orden de la lista de tareas, punto de llegada y tope del viaje, el reto de la historia de Musk (Probado en rp4) |
| 120 | `ChoreLibraryTest` | CS-50…CS-96 | Plantillas, ChoreRun, StepChain, curva y duelo de ideas, ligaduras de Hawthorne, sitios en la escena, animación de cada clase (Solo código) |

Las dos van en la lista rápida (`TestPresetsData.Rapida`). `tools/qa-check.sh` corre el comprobador de QA sin Unity.
