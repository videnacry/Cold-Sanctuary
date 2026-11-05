# La energía del santuario (investigación → decisión → construcción, Berón 2026-11-05)

Pedido de Berón: «agrega a fernando al mesocosmos y es instalador y reparador de paneles solares con lo que se debe agregar el
mantenimiento/instalación de los paneles solares en la creación de jornadas, imagino que cuando se crea un piso nuevo se debe crear
también sus respectivos paneles solares y los paneles solares imagino que se deben crear en misma proporción a la energía que requiera el
piso, quizás se podría crear paneles en los pisos de campos? quizás se podría crear pisos de paneles solares en el edificio de campos?
analiza/investiga y crea la mejor solución, también se puede plantear crear otro casquete artificial alejado que se encargue de producción
energética, imagino que podría tener allí los paneles solares y otros mecanismos, quizás podría obtener energía eólica e hidráulica sin
poner en peligro a los animales? quizás existen normas de seguridad que se pueden seguir».

**Respuesta corta.** Sí al casquete aparte, no a los paneles en los campos. El **Parque Energético** es un casquete artificial en el
noroeste con su árbol bajo. Allí van los paneles de todos los pisos, 3 aerogeneradores pequeños, una turbina mareal piloto y el
almacenamiento. El **reactor sigue siendo la carga base**: en la noche polar el sol da cero, y por eso no hace falta guardar energía de
un verano para el invierno. Cada piso declara su demanda y paga su parte en paneles; un validador comprueba que la oferta cubre la demanda.
La hidráulica sale mal parada: las corrientes reales de McMurdo no llegan ni al arranque de una turbina, así que la mareal es solo un piloto.
Las ideas de Berón para las aves (pisos de cría, figuras, dragón de drones, aves amaestradas) están en §7.

Código: `Assets/Scripts/World/EnergyRule.cs`, `FloorEnergy.cs`, `EnergySite.cs`, `WindTurbineUnit.cs`, `TidalTurbineUnit.cs`,
`Assets/Editor/SampleSceneBuilder.Energy.cs`. Prueba: `EnergyTest` (grupo 101).

## 1. Lo investigado

### El sol a 78° S
- El santuario está en la latitud de McMurdo (`SolarPosition.SanctuaryLat`, −77,85°). Hay sol de medianoche, noche polar y días normales
  entre medias. El sol no pasa nunca de ~35,6° de altura (90 − 77,85 + 23,44): en verano da la vuelta entera al horizonte, bajo.
- **Troll (Noruega, 72° S):** diseñado para **957 kWh/kWp al año**, comparable al norte de Alemania, y la producción real quedó muy cerca
  de la prevista. Es proyección de diseño, no medida anual publicada.
- **Mawson (67,6° S, modelo NASA POWER, no medida):** 7,98 kWh/kW al día en verano, 5,55 en primavera, 1,60 en otoño y **0,24 en invierno**.
- **Concordia:** el potencial fotovoltaico va de octubre a finales de febrero.
- **Bifaciales verticales:** en Alaska, módulos verticales este-oeste produjeron casi lo mismo al año que los inclinados a la latitud
  (Pike et al., 2021), con otro reparto horario. Aprovechan el sol bajo y la nieve, y se cubren menos de nieve. La ganancia bifacial crece
  con la latitud por la nieve y el sol bajo (informe IEA Task 13, 2026; solo leí el resumen de pv magazine).
- **Lo que no encontré:** rendimiento medido en McMurdo, Scott Base o Princess Elisabeth.

**Modelo del juego** (`EnergyRule.SolarKWPerKWp`): filas verticales bifaciales que miran al norte y al sur, con el sol real de
`SolarPosition`, cielo claro de Meinel, albedo 0,7 y un factor de 0,5 (nubes y pérdidas) **calibrado** para dar ~900 kWh/kWp al año, algo por
debajo de Troll. Sin ese factor el modelo da 1815. Reparto mensual simulado: de mayo a julio, **cero**; abril y agosto, casi cero; diciembre
y enero, el máximo. En el juego el año dura 28 días, así que la noche polar son unos 8 días de juego.

### El viento y las aves
- **Ross Island** (Scott Base y McMurdo): 3 Enercon E-33 de 330 kW desde 2009–10, a **7,9 m/s** de media a la altura del buje, y se montaron
  con hasta −57 °C. Dan entre el 11 % y el 22 % de la electricidad según la fuente (las dos cifras circulan). Se van a sustituir por 3 de
  1 MW con batería, que aspiran a más del 90 % de Scott Base. Es el precedente directo: **lleva 15 años funcionando a esta latitud**.
- **Pala pintada (Smøla, Noruega; May et al., 2020):** una pala negra bajó las muertes de aves un **71,9 %** en las 4 turbinas pintadas frente
  a las 4 de control, sobre todo rapaces. **Es poca muestra** (6 muertes frente a 18), en verano las muertes subieron, y sus autores piden
  replicarlo. Rápido y barato solo si se pinta antes de montar.
- **Parada por detección (IdentiFlight, Wyoming; McClure et al., 2021):** cámaras con clasificación automática paran la turbina que corre
  peligro: **82 % menos** de águilas muertas frente al control (IC 95 %: 75–89 %). Un solo sitio, y **no las elimina**. Alcance ~1 km por torre.
- **Murciélagos (Arnett et al., 2011):** subir el arranque a 5,0–6,5 m/s bajó las muertes un 44–93 % de noche, perdiendo ≤ 1 % de energía. Una
  metaanálisis de 2024: −33 % por cada 1 m/s. **En la Antártida no hay murciélagos**, así que aquí no se aplica; queda escrito por si la fauna cambia.
- **Datos de aves antárticas:** no encontré ningún dato de choques de págalos, petreles o pingüinos con las turbinas de Ross Island o Mawson,
  ni la evaluación ambiental. **Que no aparezcan no significa que no pase.** Las medidas de arriba vienen de otras especies.
- **Hielo:** la distancia de cribado es **1,5 × (buje + rotor)** (Seifert/DEWI). Es conservadora y se usa como referencia, no como garantía.

### La mareal, la hidráulica y los animales marinos
- **McMurdo no da para mareal.** Un estudio del Cuerpo de Ingenieros de EE. UU. (ERDC/CRREL TR-16-17, 2016) midió las corrientes y concluyó que
  son demasiado bajas: **la máxima no llega al arranque de la mayoría de las turbinas**. Además el mar de Ross es área marina protegida.
  (Leí el resumen; el PDF no se dejó abrir.) Otro registro antiguo da ~0,5 m/s de media y 1,4 de máxima, y los modelos pasan a veces de 1 m/s en el
  noroeste del mar de Ross. La marea es sobre todo diurna, de 1–3 m.
- **Golpe de pala:** en focas, el trauma grave solo aparece **por encima de ~5,6 m/s** (IC 95 %: 4,4–6,6; Onoufriou et al., 2019). La velocidad
  crece de cero en la raíz al máximo en la punta, y una turbina suele ir a 4,5 veces la corriente.
- **Parada por sonar (SeaGen, Strangford Lough):** la única fuente empírica durante años. Con sonar activo paraba si había un mamífero
  cerca: el radio pasó de **250 m a 50 m** con la experiencia, con 15 paradas el primer año. No se han observado choques con focas en MeyGen,
  Nova ni Orbital según la industria, pero **los reguladores lo siguen viendo sin resolver** y los modelos acumulados llegan a preocupar.
- **Ruido:** con las focas de Weddell, un ruido continuo bajó su ritmo de llamadas (estudio con vehículos Hägglunds). Vale de aviso: hay que medir.
- **Bombeo en el tronco (cuenta propia):** subir 1000 m³ a 60 m guarda ~120 kWh. Todo el tronco (~24 000 m³) daría ~1,5 MWh, frente a los 30 MWh de
  las baterías, y dejaría sin agua el nado y el mar (T1, T7). **Descartado.**

### Almacenar
- **Baterías:** el litio no se debe cargar por debajo de 0 °C (se deposita litio); descargar sí puede. Por eso van en **contenedores calefactados**.
  Un contenedor de 20 pies LFP ronda los **5 MWh** (hoja de un fabricante). Separación: NFPA 855 parte de 3 pies entre unidades y otros documentos
  piden 10 pies (3 m) entre contenedores; no pude leer el texto de 2026. Scott Base estudió hasta 10 MWh para 3–4 MW de viento.
- **Arena (Pornainen, Finlandia):** 1 MW y **100 MWh térmicos** en ~2000 t de esteatita, una semana de invierno de esa red. Guarda calor, no electricidad.
- **Hidrógeno:** es lo que piden los pilotos en islas para pasar semanas o estaciones sin viento ni sol. Aquí el reactor hace ese papel, sin el peligro de
  tener hidrógeno a presión en una base.

### Las normas de la fauna
- **Protocolo de Madrid, Anexo II:** prohíbe la interferencia dañina con la fauna. Las distancias concretas no están en él, sino en guías que lo aplican:
  5 m en general (IAATO), 15 m a focas y aves que crían, **50 m a pingüinos emperador** y **100 m a petreles gigantes y albatros** (programa antártico
  australiano). Las cifras varían entre fuentes.
- **Resolución 2 (2004), aeronaves:** Alemania la resume en no sobrevolar concentraciones de aves y focas por debajo de 610 m. No leí el texto oficial.

### Qué pide cada piso
CBECS 2018 (EE. UU.), tabla C22, intensidad eléctrica media: restaurante **50,7 kWh/ft²**, clínica 43,8, recreación 15,0, servicios 13,2, educación 12,6,
oficina ~12,8 y todos los edificios 12,6. Multiplicado por 1,229 da vatios medios por m². Hospitales y laboratorios no salen en la tabla que pude leer: el
hospital usa el valor de clínica y el laboratorio es **una estimación mía** (~3 veces una oficina). Son edificios cerrados de EE. UU.: una referencia, no una medida.

## 2. La decisión

| Opción | Veredicto | Por qué |
|---|---|---|
| Paneles encima de los campos | **No** | Con el sol a < 36°, un panel de altura h da sombra de 1,4–2,7 h justo en la estación de cultivo, y los campos son la comida. |
| Pisos solares en el Árbol de los Campos | **No** | No hay pisos libres (los 8 tienen cultivo), un piso bajo otro solo recibe luz de lado y 8 pisos son 8000 m². Quitarle un piso a la comida para ganar 1000 m² no compensa. Gira, además: los cables no. |
| Casquete de energía aparte | **Sí** | 57 600 m² sin competir con nada, todo el mantenimiento en un sitio, lejos de las islas, y las turbinas fuera del paso de las aves. |
| Un parque sin reactor | **No** | El sol da cero 3 meses. Haría falta guardar estaciones enteras. El reactor ya existe y cubre la noche polar. |
| Eólica | **Sí, pequeña** | 3 turbinas de 330 kW, las de Ross Island: lo probado a esta latitud. Rotor de 33 m. Cubren cerca de la mitad de la demanda y funcionan de noche. |
| Mareal | **Solo piloto** | No hay corriente: ~5 kW de media entre las dos (28 de pico). Se hace porque Berón la pidió y deja medido su efecto, y va lenta a propósito. **No cuenta para cubrir la demanda firme.** |
| Bombeo | **No** | Ver las cuentas de arriba. |

### El reparto (las cuentas con ~800 kW de demanda media, unos 7 GWh al año)
- **Reactor (5 MWe):** carga base, la noche polar y el calor de distrito. Cubre solo toda la punta: es la **única oferta firme**.
- **Viento (3 × 330 kW):** ~3,3 GWh al año (factor de capacidad ~0,38, supuesto del juego: no encontré el real). Noche y día.
- **Sol (1 de cada 5 kWh de cada piso, ~1,5 MWp):** ~1,4 GWh al año, casi todo de octubre a febrero.
- **Mareal:** simbólica (~0,04 GWh).
- **Baterías (6 × 5 MWh = 30 MWh):** si el reactor para, sostienen lo esencial (hospital, cría, cultivos, cocina y cámaras) **72 h**, la misma referencia que el
  agua pasiva de T14, sin contar con el viento. No están para pasar el invierno.
- **Silo de arena (30 MWh térmicos):** lo que sobra en verano va al calor de distrito en vez de tirarse.

La fracción 1/5 no es una norma: es un reparto que deja libres cerca de un tercio de las ~170 plazas del casquete para los pisos que vengan. Con todas llenas, el validador avisa y hace falta un casquete mayor.

### El sitio
- **Centro (−470, 0, 470)**, esquina noroeste: a **≥ 300 m** de todo edificio e isla (la distancia de la lanzadera, para que un fallo de cohete no le llegue).
  Es el lado de barlovento: el viento dominante (`NuclearSiteRule.PrevailingWindTo`) llega aquí antes de cruzar el santuario, y la pluma de una emergencia
  nuclear se va justo al lado contrario.
- **Casquete de 240 × 240 m** (`BuildArtificialCap`) con su **árbol bajo**: primer piso a 12 m, la excepción de T14 (en un casquete no crece ningún árbol) y sombra corta.
- **Piso 1, Energía:** control, taller de Fernando y repuestos. **Piso 2:** garaje del submarino de obra.
- **Campo solar:** mesas verticales de 36 módulos (14,4 kWp), a 8 m entre filas y 2 m de paso. Las que faltan esperan montadas pero apagadas: es lo que instala Fernando.
- **Aerogeneradores:** en 3 esquinas, a ≥ 200 m de toda isla (el doble de los 100 m de la guía más estricta; **ninguna norma da esta cifra para turbinas**).
  La pala negra, las cámaras y el radar, en cada torre.
- **Baterías y silo:** en la esquina sureste, con 3 m entre contenedores, lejos de las turbinas.
- **Línea submarina** al santuario, señalizada con boyas, como la nuclear.

### La seguridad de los animales
- **Eólica:** una pala negra en cada rotor; parada si un ave vuela a ≤ 200 m (20 s de vuelo a 10 m/s); rearme a los 30 s sin aves; a ≥ 200 m de las islas.
  También para si hay alguien al pie de la torre (la revisión se hace parada) y por encima de 25 m/s.
- **Mareal:** punta a ≤ 5 m/s, por debajo del umbral de 5,6 m/s de las focas; sonar con parada a ≤ 50 m (el radio final de SeaGen); anillo alrededor del rotor; sin
  aceite a la vista (`ene_mareal` revisa). Si el regulador de punta no alcanza, para.
- **Lo que queda sin resolver:** ninguna de estas medidas elimina el riesgo, y ninguna se ha probado con la fauna antártica. Por eso `ene_control` lleva el registro de
  paradas y `ene_eolica` y `ene_mareal` revisan los detectores.

## 3. La regla (T18 y S14 del manual)
1. **Cada piso declara su demanda** (`FloorEnergy`): área × W/m² de su clase (`EnergyRule.WattsPerM2`) + lo fijo (Informática, 95 kW), y si es esencial. Se pone solo al
   construir (`EnsureFloorEnergy`) y al validar si alguien crea un piso en juego (`EnergyRule.DeclareMissing`).
2. **Su creación añade paneles:** `EnergyRule.TablesFor(kW)`. Al construir, el campo sale con las mesas de todos los pisos. En juego, un piso nuevo deja mesas
   pendientes y aparece `sol_instalacion`.
3. **El validador** (`EnergyRule.Check`, al construir y al final del Blockout; en juego, cada día): reactor ≥ punta, paneles instalados ≥ los que piden los pisos,
   las baterías aguantan lo esencial 72 h, y para cada mes, oferta ≥ demanda. Si no cumple, error en el registro.
4. **Lo nuevo y alto** lleva `ElevatedElement`: las turbinas cuentan para la lanzadera (T13).

## 4. La jornada (ids acordados con claude/edif-interfaz)
**Área de las misiones: `Energía`** (`EnergyRule.Area`). **Fernando** llegó de claude/edif-interfaz. Allí traía `sol_instalar` y `sol_reparar` en Construcción; al unir las ramas quedaron sustituidas por las de abajo: su profesión es el área `Energía` entera, empieza en el piso de control y su casa no cambia.

| Id | Cuándo | Horas | Qué hace |
|---|---|---|---|
| `sol_mantenimiento` | cada día | 1 | Quita escarcha y nieve: la suciedad baja la producción hasta un 30 % |
| `sol_instalacion` | si hay mesas pendientes (piso nuevo) | 2 | Monta 4 mesas por turno |
| `sol_reparacion` | si hay mesas averiadas | 1 | Las repone (una avería cada 5 días) |
| `ene_control` | cada día | 0,5 | Reparto de cargas y registro de paradas por fauna |
| `ene_eolica` | cada 3 días | 1 | Palas, cámaras y radar, con la turbina parada |
| `ene_mareal` | cada 3 días | 1 | Sonar, anillo y fugas |
| `ene_almacen` | cada 3 días | 0,5 | Calefacción y temperatura de las baterías, y el silo |
| `ene_ronda_aves` | cada día | 0,5 | Busca aves heridas y retira restos al pie de torres y mesas (§7) |
| `ene_disuasion` | si las aves se acostumbran | 1 | Cambia las figuras de sitio y revisa los drones (§7) |
| `hos_aves` | si hay aves en el refugio | 0,5 | En el Hospital: las cura y las suelta (§7), con uniforme de veterinaria |

Cada una trae sus pasos con E (`PlayerTasks`), su actuación (`Performances`) y el uniforme `Energia` (mono azul, casco, arnés, guantes aislantes). Las de sitio
(paneles, torres, pontona, baterías) mandan a la persona al casquete (`EnergySite.FieldPoint`), no al piso de control.

## 5. Prueba (grupo 101, `EnergyTest`)
Sitio y distancias; todos los pisos declaran; proporción; validador; noche polar a cero y año ~900; suciedad y mantenimiento; piso nuevo → `sol_instalacion` → mesas → cumple;
ave → parada → rearme; pala negra; foca → parada de la mareal; punta ≤ 5 m/s; reactor parado → baterías; misiones con pasos, actuación y uniforme; y que Fernando las tenga.

## 6. Pendiente
- Las medidas de aves y mamíferos son de otras especies y latitudes: si Berón quiere más rigor, buscar la evaluación ambiental de Ross Island y la de Scott Base.
- `SanctuaryResource.Energy` sigue siendo un contador de julios sin gastos: el balance real vive en `EnergySite` y no se mezcla, para no tocar las pruebas de la economía.
- El HUD de energía (mostrar oferta y demanda en el modal de recursos) es del trabajador de la interfaz.
- Sin microcosmos propio todavía: la alerta de piso nuevo avisará si alguno encaja.

## 7. Las cuatro ideas de Berón para bajar el riesgo de las aves

Pedido: «¿se podrían colocar pisos al rededor del piso de hélices? entonces dichos pisos serían de crianza de aves con lo cual también podrían sanar a cualquier herida que encuentren? y podrían realizar
acciones mas originales como crear estructuras aterradoras muy visibles para que las aves rodeen o similar? quizás podrían usar drones para recrear un amenazador dragón? o quizás podrían tener aves
amaestradas que conocen el riesgo de las turbinas y alertan a las aves que se acercan?»

**Respuesta corta.** Curar sí, pero lejos del rotor; criar no. Las figuras y los drones existen en el juego y salen solo al saltar un detector, sin contar para la seguridad. El dragón es un adorno. Las aves
amaestradas no se hacen. **Lo que protege sigue siendo parar la turbina.**

| Idea | Lo que dice la evidencia | Veredicto |
|---|---|---|
| Pisos de cría y cura alrededor del rotor | Va en contra de su objetivo: atrae aves al peligro | **Cría: no. Cura: sí, en el Hospital** |
| Estructuras aterradoras | Débil: en la única prueba en un parque eólico que encontré, sin efecto | **Se hacen, a demanda y móviles; no cuentan** |
| Dragón de drones | Los drones funcionan a corto plazo y en poco espacio; un dragón no está probado | **Adorno del juego, rotulado** |
| Aves amaestradas que avisan | No hay ninguna prueba de que se pueda | **No** |

### Cría y cura: la atracción es el problema
- **Atraen:** en Cerdeña, los buitres leonados concentran sus movimientos en dormideros y comederos, y los autores recomiendan alejar los parques de esas zonas y retirar la carroña (**preprint, sin revisión**).
  No encontré ninguna prueba de campo de que retirar restos baje las colisiones, ni ningún estudio de un centro de recuperación junto a un parque eólico. **Lo de que un refugio atraería aves al rotor es una inferencia de esos datos, no una medida.** Pero es lo que se esperaría, y el coste de equivocarse
  sería matar a las aves que se quieren salvar.
- **Soltar cerca es peor:** un ave recién curada puede estar físicamente tocada sin que se note (águilas reales), y en una revisión sistemática las aves carnívoras y diurnas sobreviven mal al principio tras la suelta. Soltarla junto a un rotor es el peor sitio.
- **Curar funciona, con límites:** un centro de Gran Canaria soltó el 68 % de sus aves marinas. No encontré su supervivencia después.
- **Criar no hace falta:** las aves del santuario no escasean, y un grupo en cautividad junto a aves salvajes es lo que **no** se quiere con la gripe aviar: la guía de SCAR pide que solo personal formado, con protección, toque aves enfermas. La gripe aviar H5N1 ya se ha citado en la Antártida (fuente de 2026 de poca calidad: confirmar con SCAR o WOAH). No encontré guía específica para aves heridas ni para aves en cautividad.
- **Y los pisos:** cualquier piso o pared alrededor de un rotor también le quita viento y mete turbulencia (razonamiento de ingeniería, no una cita).

**Lo construido:** la ronda diaria `ene_ronda_aves` busca aves heridas y retira restos al pie de torres y mesas, y deja las heridas en la `Caja_transporte_aves`. El **Refugio de aves** (`BirdRehabPoint`, 6 cajas) está en la planta
veterinaria del Hospital, a ≥ 400 m de las turbinas (el doble del radio de parada: elección de diseño, lo comprueba `ValidateEnergySite`). `hos_aves` las cura 3 turnos y las suelta allí. Sin comida al aire libre en el parque. La llegada de aves heridas es
un evento fijo del juego (una cada 9 días): con las turbinas paradas por las aves no habría heridas, y la cifra no es una estimación de nada.

### Estructuras aterradoras
- **En un parque eólico:** una prueba piloto en Nueva Escocia con búhos de plástico y gritos de alarma no encontró diferencias significativas con los controles (resultados no concluyentes).
- **Habituación:** una revisión de 2003 para el gobierno británico dice que los métodos sonoros funcionan a corto plazo por la habituación, y que hay poca información. Lo de que un búho de plástico dura días viene de empresas de plagas, no de ensayos.
- **Lo más prometedor** son pruebas de laboratorio: la mirada fija asusta a las rapaces (Hausberger et al., 2018) y aves cautivas evitan palas rojas, negras y amarillas (2026, sin prueba de campo). Son de otras especies.

**Lo construido:** 4 `Figura_aterradora` con ojos, **ocultas**: salen 30 s cuando un detector para una turbina, porque lo que se sabe de la habituación apunta a no enseñarlas siempre. `ene_disuasion` las cambia de sitio. `EnergySite.habituation` sube un 10 % con
cada salida, baja un 2 % al día y un 40 % al moverlas; por debajo de 0,25 de fuerza ya no salen. **Ese modelo es del juego, no una medida.**

### Drones
- **Sí funcionan a corto plazo:** en viñedos (Washington State) hubo 3,5–5 veces menos visitas de aves con dron, sobre todo con gritos de alarma; pero 15 minutos de batería cada vez y con un observador delante. Con un señuelo de cuervo, los cuervos y cacatúas se fueron en ~50 m de radio y tardaron al menos 2 h en volver, y a las aves pequeñas solo las movió un rato (Wang et al., 2019).
- **Dependen de la especie:** en un aeropuerto, efectivos a corto plazo y en poco espacio. Con gansos no sirvió en Cornell. No se ha medido bien la habituación: que un dron «como un depredador» dure más es una opinión. Lo de «sin habituación» del Robird es de su vendedor. No encontré Egan 2020.
- **Con fauna antártica sería otra cosa:** ahuyentar aves con un aparato es justo lo que quieren evitar las normas de §1 (Anexo II y la Resolución 2 de 2004). Haría falta permiso, y puede hacer más daño que la turbina. No encontré la guía de drones de la ATCM.

**Lo construido:** el `DroneDragon`, 48 drones que dibujan una serpiente de 45 s alrededor de la turbina parada y esperan 5 min. **Ninguna fuente dice que un dragón ahuyente aves: es un adorno del juego**, y su hangar lo dice en el cartel.

### Aves amaestradas
- **Cetrería en aeropuertos:** resultados alentadores en unos sitios, pero una evaluación del USDA la llama inviable e ineficaz y dice que las rapaces pueden levantar a las aves y **subir** el riesgo de choque. Además, en la Antártida no hay halcones, y traer especies de fuera va contra el espíritu del Anexo II (no leí el artículo).
- **Aprender de otras aves es real, pero es otra cosa:** las grullas jóvenes aprenden la ruta de las viejas; basta una experta por grupo, y 7 años de experiencia mejoran la precisión un 38 % (Mueller et al., 2013). Es su misma especie, una ruta y años.
- **Lo que no existe:** no encontré nada de un ave que conozca el riesgo de una turbina y avise a otras. La búsqueda sobre alarmas entre especies no devolvió nada, así que **no sé** si serviría; que un ave entienda una turbina y la explique es otra cosa.

**No se construye.**

### Pendiente de estas ideas
- Las figuras y el dragón no mueven a las aves del juego (`BirdBehavior` vuela al azar): se ven y se cuentan, pero no cambian su ruta.
- El Refugio no recibe animales reales de la escena: son cajas con un ave que se enciende.

## Fuentes
- Troll: [pv Europe](https://pveurope.eu/solar-modules/where-trolls-live-pv-panels-installed-antarctic-pilot-solar-power-project-troll-green). Mawson: [Profile Solar](https://profilesolar.com/locations/Antarctica/Mawson-Station/). Antártida: [PV Tech](https://www.pv-tech.org/interviews/how-to-install-solar-in-antarctica), [pv magazine](https://www.pv-magazine.com/?p=33656)
- Bifaciales: [Pike et al., OSTI](https://www.osti.gov/servlets/purl/1781572), [pv magazine, 2026](https://www.pv-magazine.com/2026/03/31/solar-above-60-north-the-arctic-as-pvs-next-frontier/)
- Ross Island: [Antarctica NZ](https://www.antarcticanz.govt.nz/media/news/new-turbines-for-the-windiest-continent), [Scott Base](https://scottbaseredevelopment.govt.nz/wind-farm-upgrade), [Entura](https://entura.com.au/projects/9376/), [Power Technology](https://power-technology.com/projects/rossislandwindfarm/), [Roberts, 2012](https://sundowner.colorado.edu/seefeldt/ptc-2005_2016/2012_pdf/PTC_2012_Roberts.pdf)
- Aves: [Smøla, Smithsonian](https://www.smithsonianmag.com/smart-news/black-wind-turbine-blades-help-birds-avoid-deadly-collisions-180975668), [IdentiFlight, British Ecological Society](https://www.britishecologicalsociety.org/research-shows-identiflight-camera-technology-greatly-reduces-wind-turbine-eagle-fatalities/)
- Murciélagos: [Arnett et al., 2011](https://tethys.pnl.gov/sites/default/files/publications/Arnett-et-al.-2011.pdf), [NREL](https://nrel.gov/docs/fy24osti/88381.pdf)
- Hielo: [Modelling ice throw, UiT](https://munin.uit.no/handle/10037/37831), [revisión de DEWI y GE](https://puc.sd.gov/commission/dockets/electric/2018/EL18-053/comments/icethrow.pdf)
- Mareal: [ERDC/CRREL TR-16-17](https://apps.dtic.mil/sti/pdfs/AD1025148.pdf), [Onoufriou et al., 2019](https://tethys.pnnl.gov/publications/empirical-determination-severe-trauma-seals-collisions-tidal-turbine-blade), [Sparling y Wilson, Annex IV](https://tethys.pnnl.gov/sites/default/files/Sparling-Wilson-AnnexIV-Mar2016.pdf), [SMRU, MRE2](https://synergy.st-andrews.ac.uk/smru/files/2017/09/MRE2-annual-rep-yr2-V5.pdf), [ABPmer](https://abpmer.co.uk/blog/tidal-stream-energy-what-is-the-impact-on-marine-species), [marea del mar de Ross](https://openpolar.no/Record/crcambridgeupr:10.1017%2Fs0954102003001032), [corrientes de McMurdo, PANGAEA](https://doi.pangaea.de/10.1594/PANGAEA.923753)
- Focas de Weddell y ruido: [Wildlife Research](https://connectsci.au/wr/article/33/4/275/273950/An-assessment-of-the-audibility-of-sound-from)
- Almacenar: [pv magazine: arena](https://www.pv-magazine.com/2025/06/11/worlds-largest-1-mw-100-mwh-sand-battery-commissioned-in-finland/), [hoja de contenedor de 5 MWh](https://starcharge.com/am/wp-content/uploads/sites/3/2025/03/5MWh-Datasheet-1-1.pdf), [NFPA 855, resumen](https://www.batterydesign.net/legislation-rules-and-regulations/nfpa-855/), [litio y frío](https://www.practical-sailor.com/systems-propulsion/electrical/lfp-battery-temperature-limits/), [microrredes con hidrógeno](https://iris.unica.it/bitstream/11584/418944/1/1-s2.0-S0360319924025485-main.pdf)
- Disuasión: [Dorey et al., Nueva Escocia](https://tethys.pnnl.gov/publications/testing-efficacy-bird-deterrents-wind-turbine-facilities-pilot-study-nova-scotia), [Bishop et al., 2003](https://brocku.ca/niagara-community-observatory/wp-content/uploads/sites/117/Review-of-international-research-regarding-the-effectiveness-of-auditory-bird-scaring-techniques-and-potental-alternatives.pdf), [palas de colores, en laboratorio](https://www.pulse.bot/science/news/birds-avoid-wind-turbines-painted-like-venomous-snakes-6b693602-26d8-4dc0-82a8-85b3a3fe3e4a/)
- Drones: [viñedos, WSU](https://dronexl.co/2020/06/17/washington-state-university-drones-to-deter-birds), [Wang et al., 2019](https://www.neuroscience.ox.ac.uk/publications/1580415), [resumen](https://newatlas.com/drones/drones-birds-crops/), [aeropuerto, FAA](https://birdstrike.org/wp-content/uploads/2025/10/CS1B-P13-Assesing-the-Effectivess-of-Applying-UAS-Hazing-Near-a-U.S.-Part-139.pdf), [gansos, Cornell](https://www.cornellsun.com/article/2017/01/cornell-researchers-use-drones-lasers-firecrackers-to-haze-ithacas-geese)
- Aves amaestradas: [Flight Safety Foundation](https://flightsafety.org/asw-article/falconry-should-thrive/), [cetrería en aeropuertos](https://escholarship.org/uc/item/4sp906r4), [Mueller et al., 2013, USGS](https://www.usgs.gov/publications/social-learning-migratory-performance)
- Atraer y curar: [buitres y comederos, preprint](https://ecoevorxiv.org/repository/object/5517/download/10822), [revisión de rehabilitación](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8929655/), [centro de Gran Canaria](https://accedacris.ulpgc.es/bitstream/10553/36035/2/A_long_term.pdf), [gripe aviar en el océano Austral, SCAR](https://ecoevorxiv.org/8jrbu/), [COMNAP](https://www.comnap.aq/s/COMNAP-HPAI-Summary-Page-30August2023.pdf)
- Normas: [distancias, programa antártico australiano](https://www.antarctica.gov.au/environment/environment-policy-and-management/code-of-conduct/field-work/guidelines-for-animal-approach-distances/), [IAATO](https://assets.contentstack.io/v3/assets/blt0de87ff52d9c34a8/blt2899f1c8f0af6b14/66d6d58870c2e9e630c3a98c/General_Visitor_Guidelines_IAATO.pdf), [aeronaves, UBA](https://www.umweltbundesamt.de/sites/default/files/medien/2875/dokumente/leaflet_operation_of_aircraft_in_the_vicinity_of_animal_concentrations.pdf)
- Demanda: [CBECS 2018, tabla C22](https://www.eia.gov/consumption/commercial/data/2018/ce/pdf/c22.pdf)
