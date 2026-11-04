# Piso de Informática: el clúster de IA local del santuario (2026-11-04)

Berón: «servidores de punta y demás equipo para tener una IA local del santuario y he escuchado que requiere mucha agua o energía?
se debe crear con sus mecanismos de abastecimiento». Se sigue la regla del proyecto: investigación → plan → construcción.

## 1. Investigación: qué pide un centro de datos de IA

| Tema | Lo que dicen las fuentes | Lo que se toma para el santuario |
|---|---|---|
| **Potencia por servidor y por rack** | Un servidor de 8 GPU NVIDIA DGX H100 consume hasta ~10,2 kW (guía de usuario de NVIDIA). Los racks de IA más nuevos (GB200 NVL72) llegan a ~120 kW por rack y van refrigerados por líquido; con aire, el límite práctico ronda los 20–30 kW por rack. | 2 racks de 4 servidores de 8 GPU (64 GPU): ~41 kW por rack, más red y almacenamiento: **~88 kW de TI** a plena carga. |
| **Refrigeración** | Por encima de ~30–40 kW por rack se pasa a líquido: directo al chip (placas frías) o inmersión. Con agua templada (ASHRAE, clases W32–W45) no hacen falta enfriadoras. | **Directo al chip, en circuito cerrado de agua templada** (entra a ~35 °C, sale a ~50 °C), con una **CDU** por fila (separa el agua limpia del rack del circuito de la planta) y un intercambiador de puerta trasera para el 15 % del calor que sigue yendo al aire. |
| **Agua (WUE)** | La WUE (The Green Grid) son litros de agua por kWh de TI. Con torres de evaporación, la media de la industria ronda **1,8 L/kWh**; los grandes operadores, 0,3–1,1 L/kWh. Los centros de datos de EE. UU. consumieron directamente unos **66 000 millones de litros** en 2023 (LBNL 2024). Los circuitos cerrados con aerorrefrigeradores casi no gastan agua. | **Sin torres de evaporación**: el calor va a la red de distrito y al agua de mar polar (el mismo foco frío que el reactor de investigación). Solo se repone lo que se pierde en purgas y muestreos (~0,5 % del circuito al mes): **WUE ≈ 0,0001 L/kWh**. |
| **Energía y PUE** | La PUE (energía total / energía de TI) media del sector fue **1,58 en 2023** (Uptime Institute); los hiperescala rondan 1,1. Los centros de datos de EE. UU. gastaron **176 TWh** en 2023, el 4,4 % de su electricidad (LBNL 2024). | Con refrigeración líquida y sin enfriadoras: **PUE ≈ 1,07** (bombas, CDU y ventiladores ~4 %, más con los filtros sucios; SAI y distribución ~3 %). **~95 kW en total: un 2 % de la central nuclear** (5 MWe). |
| **Redundancia** | Tier III (Uptime): todo con N+1 y mantenible sin parar. SAI con baterías de litio para aguantar hasta que arranque el respaldo. | **Bombas 2 + 1 de reserva**, **2 SAI** y **3 armarios de baterías de litio** (60 kWh: ~40 min con todo el clúster). Respaldo: la central y los paneles solares; no hay diésel en el santuario. |
| **Red** | Los clústeres de GPU unen sus servidores con InfiniBand o Ethernet de 400 Gb/s. | Un rack de red (InfiniBand 400G y Ethernet) y dos de almacenamiento (NVMe y copias). |
| **El calor que sobra** | Se reutiliza en redes de calor de distrito: el centro de Meta en Odense (Dinamarca) da calor a la red municipal (Fjernvarme Fyn) para unos miles de hogares; Estocolmo (Stockholm Data Parks) compra el calor de los centros de datos. | El agua sale a ~50 °C: **sirve a la red de calor de distrito de la central**, que ya templa los **fotobiorreactores** (quieren 30–35 °C) y los frutales de los techos. En un santuario polar siempre hace falta calor: hasta 120 kW van a la red; lo que sobra, al mar. |

Fuentes:
- NVIDIA, *DGX H100 User Guide* (potencia máxima del sistema: ~10,2 kW).
- NVIDIA, *GB200 NVL72* (~120 kW por rack, refrigerado por líquido); eeNews Europe, «Nvidia offers 120kW liquid-cooled Blackwell rack as industry standard».
- Shehabi, A. et al. (2024). *2024 United States Data Center Energy Usage Report*. Lawrence Berkeley National Laboratory (176 TWh en 2023, 4,4 %; agua directa).
- Uptime Institute (2023). *Global Data Center Survey* e «Intelligence Update 221» (PUE media 1,58).
- The Green Grid (2011). *Water Usage Effectiveness (WUE): A Green Grid Data Center Sustainability Metric*. White Paper 35.
- Koomey, J., «Reviewing fleet average onsite water and infrastructure energy efficiency for the top twenty global data center operators» (koomey.com).
- ASHRAE TC 9.9 (2021). *Liquid Cooling Guidelines for Datacom Equipment Centers* (clases de agua W32–W45).
- Meta, «Odense Data Center» (calor a la red de Fjernvarme Fyn); Stockholm Data Parks.
- Uptime Institute, *Tier Standard: Topology* (Tier III, N+1).

**[verificar]** la cifra exacta de agua de LBNL (las fuentes secundarias dan 64–66 mil millones de litros) y los hogares que calienta
Odense; la PUE de Google de 2023 (1,10 está confirmada para 2019).

## 2. Plan y construcción

- **Dónde:** el piso libre de la **torre de Ciencias** (el 8.º de su hélice, encima de Biología), con su planta de 32 × 20 m
  (`SampleSceneBuilder.Informatica.cs`, `BuildInformatics`). Área `Informatics_Area`, tipo `SanctuaryAreaType.Informatics`.
- **Lo que hay** (bloques con 2 m de paso, `PackFloor`):
  - 2 **filas de 4 racks de GPU**, cada una con su **CDU** y sus colectores (azul, frío; rojo, caliente);
  - **rack de red** y 2 **racks de almacenamiento**;
  - **intercambiador a la red de distrito** y **intercambiador de agua de mar** (foco frío);
  - **bombas** (2 + 1 de reserva) y **depósito de reposición** (agua del tronco de la torre);
  - **cuadro de la acometida** (central nuclear y paneles), 2 **SAI**, 3 **armarios de baterías de litio**;
  - **extinción por gas inerte**; **armario de repuestos**;
  - 4 **puestos de monitorización** (el primero con la pantalla de estado del clúster) y 2 **bancos de mantenimiento de la refrigeración**;
  - aseo y la máquina de virtualización del área (con la historia de Bill Gates).
- **El modelo** (`AiCluster`, en el área): potencia de TI y total, PUE, temperatura de las GPU (sube con la suciedad de filtros y
  placas; por encima de 85 °C bajan potencia), calor a la red de distrito y al mar, litros repuestos y WUE, batería del SAI (si cae
  la acometida aguanta ~40 min y luego apaga ordenadamente). No descuenta del recurso Energía del santuario: ese es el de la
  economía, a otra escala (y lo vigilan las pruebas de la magia).
- **Abastecimiento:** electricidad de la **central nuclear** (y los paneles de día) por su acometida; agua de reposición del **tronco**
  de la torre; calor a la **red de distrito** de la central (fotobiorreactores, frutales) y el resto al **agua de mar**.
- **Trabajos y misiones** (`AreaFunctions`, papel `informatico`): `ifo_sysadmin` (colas, copias, parches, alertas: 1 h),
  `ifo_refrigeracion` (filtros, fugas, bombas, intercambiadores: 1 h), `ifo_energia` (acometida, SAI y baterías: 0,5 h). Cada una
  tiene su efecto en el modelo.
- **Capacidad** (regla S13): 4 puestos de sistemas, 2 de refrigeración y 3 de energía (cuadro y SAI): varias personas a la vez.

## 3. Pruebas

- **Grupo 74** (`InformaticaAulaTest`): el modelo (64 GPU, ~88 kW, PUE 1,03–1,20, WUE < 0,05, calor a la red, la suciedad baja la
  potencia y la misión lo arregla, el SAI aguanta y se apaga ordenadamente, la misión de energía lo repone, misiones en el registro)
  y el piso en la escena (encima de Biología, 8 racks, CDU, intercambiadores, depósito, bombas, acometida, SAI, baterías, gas).
- **Grupo 75** (`CapacidadPisosTest`): sus puestos de trabajo (≥ 2 por actividad).
- **Grupo 76** (`AlertaPisoNuevoTest`): Gates re-apuntado aquí; la alerta señala a Bagdad («sala de cálculo»).
