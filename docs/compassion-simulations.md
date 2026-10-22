# Simulaciones de compasión

Microcosmos que reencarnan un **momento histórico real** en el que una persona o un pueblo protegió a otros arriesgándolo
todo. Enseñan historia con datos verificados (cada tarjeta final cita sus fuentes) y hacen jugable el cuidado: guiar,
esconder, alimentar, conseguir papeles, poner a salvo.

## Principio: un microcosmos es un área del santuario agrandada

Igual que los demás microcosmos (`docs/microcosmos-areas.md`), cada simulación se monta con `AreaMicrocosmos.Build` sobre
un **área real ×10**. El pueblo de los años 40 se levanta en el hueco libre del suelo del área, y el resto del área sigue
ahí, gigante. La lógica del santuario se reutiliza:

| Simulación | Área ×10 | Por qué |
|---|---|---|
| **Le Chambon-sur-Lignon, 1942** | Cocina | El pueblo que alimentaba y escondía: la cocina de Magda Trocmé, la panadería, las granjas |
| **Irena Sendler, Varsovia 1942–43** | Enfermería | La trabajadora social con pase del servicio de epidemias; ambulancias, orfanatos, conventos |

Se entra desde la máquina de virtualización de esa área (opción «Simulación: …»). Las escenas son
`Microcosmos_LeChambon` y `Microcosmos_IrenaSendler`. Se generan con «Tools/Cold Sanctuary/Build simulaciones de compasión»,
o junto con el blockout.

Los personajes son **mirmidones** (insecto-humanos) con ropa de los años 40: abrigo, bufanda, sombrero o gorra. Las
patrullas son genéricas, sin uniformes ni símbolos, y **no hay violencia en pantalla**: si una patrulla ve a alguien
expuesto, esa persona vuelve a su punto de partida.

## Mecánica (una tecla: E; el HUD dice qué hace E en cada sitio)

| Pieza | Script | Qué hace |
|---|---|---|
| Refugiado/niño | `Refugee` (+`WeakOne`) | E: te sigue. Expuesto = fuera de su zona de partida, sin escondite y sin papeles |
| Escondite | `HidingPlace` | Esconde a los que te siguen (capacidad). Despensa: una ración por persona y día; sin comida, alguien sale. Algunos cuentan ya como a salvo (las familias de Le Chambon acogían hasta el final) |
| Pan | `BreadSource` | Coges raciones y las dejas en la despensa de un escondite |
| Papeles falsos | `PaperForge` | Impresos limitados. Con papeles, las patrullas no los detienen |
| Paso | `Passage` | Tribunal de dos puertas, alcantarillas, ambulancia (pocos por viaje y espera entre viajes). La puerta del gueto solo deja pasar al jugador (`alone`) |
| Lugar seguro | `SafeHaven` | Convento, orfanato, familia, el camino a Suiza: quedan a salvo |
| Patrulla | `Patrol` | Recorre su ruta y ve en un cono frontal, no a través de paredes |
| Frascos | `NameJar` | Irena: cada niño a salvo deja su nombre real en el frasco del manzano |
| Misión | `CompassionMission` | Intro, HUD «A salvo X/N», tarjeta de historia con fuentes al acabar, y XP de la marga de los Vínculos |

## Le Chambon-sur-Lignon (1940–1944)

- Pueblo protestante del plateau de Vivarais-Lignon (Alto Loira). Con los pueblos vecinos acogió a **entre 3.000 y 5.000**
  personas perseguidas, muchas de ellas niños judíos, en casas, granjas, pensiones y escuelas.
- Lo animaban el pastor **André Trocmé**, su esposa **Magda** y el pastor **Édouard Theis**. La Escuela Nueva Cevenol la
  fundaron Trocmé y Theis en 1938.
- **Oscar Rosowsky**, refugiado de 19 años, falsificaba documentos. La **Cimade** guiaba a algunos refugiados a Suiza.
- **Daniel Trocmé**, primo de André, dirigía la Maison des Roches. Fue detenido en 1943 y murió en Majdanek en 1944.
- En 1990 Yad Vashem reconoció a todo el pueblo como **Justos entre las Naciones**, uno de los poquísimos reconocimientos
  colectivos.
- Fuentes: USHMM Holocaust Encyclopedia «Le Chambon-sur-Lignon»; Yad Vashem, Righteous Among the Nations; Philip Hallie,
  *Lest Innocent Blood Be Shed* (1979).

## Irena Sendler (1910–2008)

- Trabajadora social del ayuntamiento de Varsovia. Un pase del servicio de control de epidemias (tifus) le permitía entrar
  al gueto.
- Desde 1942 dirigía la sección infantil de **Żegota** (Consejo de Ayuda a los Judíos). Su red sacó del gueto a **unos
  2.500 niños**: en ambulancias, por el tribunal de la calle Leszno (con puertas a ambos lados), por pasos subterráneos y
  alcantarillas, y escondidos en cajas y sacos.
- Los colocó con familias, en conventos y en orfanatos (p. ej. el del Padre Boduen), con identidades nuevas. Los
  **nombres reales** los apuntó en listas enterradas en **frascos bajo un manzano**, en el jardín de su compañera Jadwiga
  Piotrowska.
- En 1943 la detuvo la Gestapo y fue condenada a muerte. Żegota sobornó a un guardia y logró salir. Yad Vashem la reconoció
  como Justa entre las Naciones en 1965.
- Fuentes: Yad Vashem «Irena Sendler»; POLIN / Polscy Sprawiedliwi «Irena Sendlerowa»; USHMM «Irena Sendler».

## Shanidar (enfermería, simulación 1)

El anciano frágil de la historia de Ambrosio se llama **Shanidar** y lleva las heridas del neandertal **Shanidar 1**
(`BodyInjuries`):

- sin antebrazo ni mano derechos;
- el orbital izquierdo aplastado, con visión reducida;
- marcha alterada (cojera);
- pérdida auditiva de conducción.

Vivió hasta viejo porque su grupo lo cuidó, y por eso es el primer gesto de compasión de la saga.

- Fuente: Trinkaus E. y Villotte S. (2017), «External auditory exostoses and hearing loss in the Shanidar 1 Neandertal»,
  *PLoS ONE* 12(10): e0186684, doi 10.1371/journal.pone.0186684.
- `BodyInjuries` baja las aptitudes afectadas (percepción por el ojo y el oído, agilidad por la cojera, fuerza por el
  brazo) y deja los datos para que el cuerpo de cubos dibuje el brazo que falta, el ojo cerrado y la marcha coja.

## Reencarnaciones (ficción, 2026-10-22)

Como en todo microcosmos, el elenco de la enfermería renace aquí **con su nombre** (los históricos reales conservan el suyo):

| | Le Chambon | Irena Sendler |
|---|---|---|
| Ruth | granjera que esconde en su granja | madre de acogida |
| Atlas | el panadero (pan para los escondidos) | conductor de la ambulancia |
| Medea | guía hacia Suiza | madre superiora del convento |
| Héspero | jefe de estación | guía de las alcantarillas |
| Sakshi | cuenta a los que llegan | la red de Irena |
| Shanidar | anciano de la meseta | anciano del orfanato |
| Ambrosio, Momo | entre los refugiados | entre los niños |

Las dos usan `HistoryCard` (intro + historia real con fuentes), compartida con Egina ([`microcosmos-myrmidons.md`](microcosmos-myrmidons.md)).

## Pendiente

- Cuerpo de cubos (hilo de Unity):
  - ropa de los años 40 en `CubeInsect` (ahora son accesorios sencillos y el color del abrigo);
  - las heridas de Shanidar.
- Más simulaciones candidatas: la red de Nicholas Winton (Kindertransport), Aristides de Sousa Mendes, Chiune Sugihara.
