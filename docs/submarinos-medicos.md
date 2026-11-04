# Los submarinos del hospital: la ambulancia anfibia y el Leviatán (Berón 2026-11-03)

Pedido de Berón: «También se tendría que crear un submarino para el hospital de modo que se pueda trasladar personas y animales;
dicho submarino debe estar equipado para brindar servicios de emergencia a los animales y personas más grandes terrestres.
Investiga si actualmente se puede servir asistencia médica a animales gigantes como las ballenas u orcas. ¿Quizás se debería
plantear un hospital especializado para animales gigantes que se encuentre a ras del suelo? ¿Quizás un submarino gigante parqueado a
ras del suelo que va directamente hacia la ballena herida y la tratan en dicho submarino colosal?»

Código:
- `Assets/Scripts/World/HospitalSubmarine.cs`: la ambulancia (`HospitalSubmarine`) y `Carried`, que lleva a un ser dentro de un vehículo.
- `Assets/Scripts/World/WhaleHospitalSubmarine.cs`: el Leviatán.
- `Assets/Editor/SampleSceneBuilder.Submarines.cs`: los dos modelos, su sitio y la búsqueda del atraque.
- Pruebas: `HospitalSubmarineTest` (grupo 81) y `WhaleHospitalTest` (grupo 82).

## 1. El submarino-ambulancia anfibio

**Medidas**: es el **vehículo de diseño** de los edificios-árbol (docs/submarino-de-obra.md, `TreeSpec.VehicleLength/Beam/Height`):
**14 × 5 × 5 m**. Las puertas miden 7 × 7 m: queda 1 m de holgura a cada lado (`HospitalSubmarine.FitsDoor`).

Es anfibio: **orugas** para el hielo (8 m/s) y **hélices en toberas** para el mar (5 m/s sumergido, con el lomo a 1,7 m bajo el agua,
por debajo de las placas de hielo). En el tronco sube y baja a 3 m/s.

**Equipo**, en la bodega de 7,5 × 3,8 × 4 m:
- una **grúa de 8 t** con su polipasto y un **trineo de carga**: cabe un oso polar (~450 kg) o un elefante asiático tumbado (~5 t;
  la camilla del elefante, ~7 × 3 × 2 m, es una de las cargas de diseño del vehículo);
- **camilla** de personas;
- **oxígeno**: un concentrador PSA y 2 bombonas (6800 L);
- **monitor multiparamétrico**, **desfibrilador** y **ventilador**;
- **nevera de sangre O−** (4 unidades). Los helicópteros médicos ya llevan sangre: London's Air Ambulance desde 2012;
- **fluidos** y bombas de infusión (a los animales, fluidos: no hay sangre universal para un oso).

El exterior: casco blanco con franja naranja, ventanal en la bodega (se ve al paciente), rampa de popa, vela con faro azul y la
estrella de la vida.

**Dónde está**: aparcado en el hielo junto a la **puerta del suelo** del árbol del hospital (Servicios), a un lado del paso.

**La misión** (`HospitalSubmarine.Dispatch(paciente)`):
1. **El equipo**: entra por la puerta del suelo, sube por el tronco hasta la puerta del piso de **urgencias** y atraca con la rampa de
   popa en ella. Dos del personal del hospital de turno (`WorkdayDirector.InArea(Hospital)`; si no hay, Lizmibet y Linduarda) andan
   hasta la puerta y suben a sus asientos. Mientras van, la jornada no los mueve (`HospitalSubmarine.Engaged`, en
   `WorkdayDirector.OnOwnDuty`).
2. **Al paciente**: sale por el tronco y la puerta del suelo y va por hielo y mar, rodeando los troncos. Se para con la popa hacia el
   paciente.
3. **La carga**: el paciente entra por la rampa, en camilla o en el trineo de la grúa.
4. **Por el camino**: oxígeno y monitor (baja el estrés y frena el empeoramiento). Si pierde mucha sangre (≥ 20 %): a una persona,
   **sangre O−** de la nevera; a un animal, **fluidos**.
5. **Al hospital**: vuelve por el tronco a la puerta del piso que le toca: **urgencias** si es una persona, **veterinaria** si es un
   animal.
6. **La descarga**: baja el equipo y el paciente va en camilla hasta su puesto (el de triaje o la mesa veterinaria). El equipo lo
   atiende (`HospitalStation.Treat`) y, si sangra, el **banco de sangre** cruza y transfunde (`BloodBank.Transfuse`; docs/hospital.md).
7. **La vuelta**: baja por el tronco, sale y aparca en su sitio.

Nada se teletransporta: el submarino se mueve paso a paso, y a los pacientes y al equipo los mueve la camilla, la grúa o la rampa
(`Carried`). Todas las esperas tienen tope y, si algo no llega, lo dice en el registro (`[Submarino] …`).

**Lanzadera**: el Leviatán no cabe en los árboles. Cuando lo llaman, la ambulancia le lleva al equipo veterinario: lo recoge en
urgencias, va a la pasarela del Leviatán y los sube a sus asientos (`HospitalSubmarine.Ferry`).

## 2. Investigación: ¿se puede atender a una ballena o a una orca?

**Sí, con límites.** Lo que se hace hoy:

| Qué | Cómo | Fuente |
|---|---|---|
| **Varamientos** | Redes de respuesta (en EE. UU., el *Marine Mammal Health and Stranding Response Program* de NOAA). El animal varado se mantiene **mojado y a la sombra**, con las aletas pectorales en hoyos, la cabeza alta y el espiráculo libre. Se refloata con **pontones** y eslingas cuando la marea lo permite (Project Jonah, Nueva Zelanda) | Geraci y Lounsbury 2005, *Marine Mammals Ashore* (2.ª ed.); NOAA Fisheries |
| **Por qué no se saca del agua** | Fuera del agua el propio peso le aplasta el tórax y los órganos y no puede perder calor: se recalienta. Cuanto más grande, peor | Geraci y Lounsbury 2005; Gulland, Dierauf y Whitman 2018, *CRC Handbook of Marine Mammal Medicine* (3.ª ed.) |
| **Sedación** | Los cetáceos respiran **a voluntad**: la anestesia general exige intubar y ventilar (se hizo en delfines desde Ridgway y McCormick 1967, *Science* 158:510). En ballenas grandes no es viable. Se usa **sedación** ligera: midazolam y butorfanol en odontocetos pequeños; en el mar, a una ballena franca enmallada, con dardo (la primera vez, Moore et al. 2010, *PLoS ONE* 5:e9597) | Gulland et al. 2018; Moore et al. 2010 |
| **Tratamiento en el campo** | Antibióticos con **dardo o jeringa de pértiga** (la orca J50 de las residentes del sur recibió antibióticos con dardo en 2018 y se intentó darle salmón medicado); muestras de sangre de los vasos de la aleta caudal o de las pectorales; ecografía portátil; desenmallado | NOAA Fisheries (J50, 2018); Gulland et al. 2018 |
| **Rehabilitación** | En **piscinas médicas de suelo elevable**, que suben al animal hasta el nivel de trabajo sin sacarlo del agua, y en **corrales marinos**. La ballena gris J.J. pasó 14 meses en SeaWorld San Diego (1997–98) y fue liberada; la orca Springer (A73), huérfana, se trató en un corral marino de NOAA en Manchester (Washington) en 2002 y volvió con su grupo en Johnstone Strait; Keiko pasó de la piscina del Oregon Coast Aquarium (1996) a un corral en la bahía de Klettsvik, Islandia (1998) | NOAA; Gulland et al. 2018 |
| **Eutanasia** | En ballenas grandes varadas sin salida, a veces es lo único humano (armas y dosis específicas) | AVMA, *Guidelines for the Euthanasia of Animals* (2020) |

**Tamaños**: orca macho, hasta ~9,8 m y ~10 t; ballena gris, hasta ~15 m; jorobada, ~14–16 m y 25–30 t; azul, ~30 m y más de 100 t
(NOAA Fisheries, fichas de especie). En el santuario viven orcas y belugas (la «ballena» del juego).

**Conclusión de diseño**:
- un hospital de cetáceos no puede estar **en tierra** (no se los puede sacar del agua);
- tiene que **ir hacia ellos**: un varado no viaja;
- tiene que **llevar el agua consigo**: una piscina médica de suelo elevable dentro de un casco que se acerca, la recibe por popa y la
  sube lo justo para que respire y se la pueda tratar con el cuerpo apoyado.

Eso es el **Leviatán**: el «submarino colosal» de Berón.

## 3. El Leviatán: hospital submarino de animales gigantes

- **Casco**: 56 × 16 × 12 m. Atracado **a ras del suelo**: en superficie, con la cubierta a la altura del hielo y una **pasarela** a
  la orilla. El atraque lo busca el constructor (`FindBerth`): mar abierto bajo todo el casco y 3 m alrededor, la orilla a 7 m de un
  costado, a ≥ 45 m de los troncos de los árboles y lo más cerca posible de la laguna de las orcas.
- **El pozo médico**: **20 × 8 m** a popa, inundado y abierto al mar por una **compuerta de popa** abatible. Tiene un **suelo
  elevable** con colchones de espuma y guías, una **pasarela** con barandilla alrededor, un **pórtico grúa**, **rociadores** (piel
  mojada y fresca) y **ventanales de cristal** desde los pasillos de los cascos.
  - Cabe una orca (≤ 9,8 m), una beluga o una ballena gris o jorobada joven (≤ 16 m).
  - Las mayores (rorcuales, azul) se tratan **junto al casco**, con las eslingas de los **pontones** que lleva en cubierta.
- **A bordo**: laboratorio, farmacia de cetáceos, ecógrafo portátil y jeringas de pértiga. En el **puente**, 4 asientos para el equipo.

**La misión** (`WhaleHospitalSubmarine.Dispatch(ballena)`):
1. **Lanzadera**: la ambulancia le trae el equipo veterinario del hospital.
2. **Inmersión**: baja a −7,9 m (el lomo, por debajo de las placas de hielo) y va hacia la ballena.
3. **Junto a ella**: se para con la popa hacia la ballena y, si hay mar abierto, emerge. Bajo el hielo la recibe sumergido: el pozo
   va inundado.
4. **Entrada**: abre la compuerta y la guía al pozo a 1 m/s.
5. **Elevar**: cierra y sube el suelo hasta que **el lomo y el espiráculo salen del agua**, con el vientre apoyado en los colchones.
   Respira sola: **sin anestesia general**.
6. **Tratamiento** (90 min de juego): ecografía desde la pasarela, muestra de sangre de la aleta caudal, antibiótico intramuscular con
   jeringa de pértiga y limpieza y sutura de la herida. Mientras tanto, la piel mojada con los rociadores y la temperatura y la
   respiración vigiladas.
7. **Liberación**: baja el suelo, abre la compuerta y la suelta en el mar, 18 m detrás de la popa.
8. **Vuelta**: vuelve sumergido a su atraque y emerge. El equipo baja por la pasarela y vuelve a su jornada.

## 4. Pruebas

**`HospitalSubmarineTest` (grupo 81)**, en vivo y a ×4:
- el vehículo cabe por las puertas (1 m a cada lado) y lleva todo su equipo;
- el hospital está en un árbol, con sus pisos de urgencias y de veterinaria;
- el submarino está aparcado junto a la puerta del suelo;
- **una persona herida** en el hielo, con un 35 % de sangre perdida (clase III): sale con el equipo y la entrega en el piso de
  urgencias, con sangre O− por el camino y el banco de sangre al llegar. La pérdida baja de la clase III, la estabilizan y el
  submarino vuelve a aparcar;
- **un oso herido**: va en el trineo de la grúa al piso veterinario, con fluidos y consulta veterinaria, y el submarino vuelve.

**`WhaleHospitalTest` (grupo 82)**:
- el Leviatán está atracado a ras del hielo con la pasarela en la orilla, y el pozo cabe la orca (y una ballena de 16 m);
- **escenario**: una orca herida (enfermedad 0,7, estrés 0,8 y un 20 % de sangre perdida) a la deriva en mar abierto;
- la lanzadera trae al equipo, el Leviatán la guía al pozo, la eleva con el lomo fuera del agua y el cuerpo apoyado, la trata (la
  enfermedad baja ≥ 0,3), la libera viva en el mar y vuelve a su atraque.

## 5. Pendiente

- La ruta por tierra y mar es una línea recta que rodea los troncos. Sin colisiones: no esquiva árboles ni animales (el submarino de
  obra sí esquiva la fauna marina; se podría reutilizar su evitación).
- La ambulancia entra por la puerta del suelo. Podría entrar también por el fondo abierto del tronco, como el submarino de obra, si el
  paciente está en el mar.
- Una alarma que llame sola a la ambulancia cuando alguien cae herido lejos del hospital (hoy la llama `Dispatch`: la prueba, un hechizo
  o un guion).
- Las ballenas más grandes, tratadas junto al casco con los pontones: la maniobra está diseñada, pero no construida.

**Ronda 2 del probador (2026-11-04).** La lanzadera del Leviatán llegaba sin equipo: tras la misión del oso, el equipo estaba en el
piso veterinario y no llegaba a la puerta de urgencias en 120 s. Ahora el submarino va a la **puerta del piso donde está cada uno**
(urgencias primero) y allí lo espera (`BoardCrew`). El registro dice quién sube y dónde.

**Ronda 3 del probador (2026-11-04).**
- **El Leviatán, a la medida de su paciente**: 56 × 16 × 12 m con un pozo de **20 × 8 m**, que cabe una ballena de 16 m con 2 m en cada
  punta. Con 72 m y un pozo de 32 m, la orca del santuario (~7 m) se perdía dentro.
- **El vientre, sobre los colchones**: al entrar en el pozo, la orca se baja hasta que lo más bajo de su cuerpo queda a 0,15 m del suelo
  elevable. Antes quedaba a 1,26 m, porque su pivote no está en su centro.
- **La bodega de la ambulancia, a la vista**: estaba dentro del casco bajo (a −1,85 m) y el paciente no se veía por el ventanal. Ahora
  está por encima, a −0,4 m.

**Ronda 5 (2026-11-04): el equipo, por su papel.** Con el reparto nuevo de papeles (docs/personajes-mesocosmos.md), la lanzadera salía
vacía: el equipo se buscaba por nombre y Andrea pasó a maestro de yoga. `HospitalSubmarine.PickCrew` lo elige ahora **por papel**,
en este orden:
1. quien tiene hoy una misión del hospital con papel médico o de enfermería (para un animal, primero las veterinarias: `hos_veterinaria`
   y `enf_animales`);
2. quien está ahora en el hospital;
3. el personal sanitario fijo (Linduarda, quien lleve `VetCare`);
4. y, si aún falta, el personal más cercano.

Siempre elige gente libre y, en cada grupo, a los más cercanos. El registro dice a quién eligió.
