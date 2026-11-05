# Sala de asambleas (2026-11-05)

Berón: «crea la sala de asamblea, asegúrate de investigar todo lo referente a diseño (época/lugar), debería ser una de las reglas; en el santuario se crean las áreas como mix de lo
mejor del mundo con lo que puede tener maquinaria/diseño con componentes de diversos países y culturas». Solo código: **compila, pero no se ha construido ni probado en Unity**.

## Dónde va

Torre de **Servicios** (la vida común: comer, curarse, reunirse), encima de la Carne Cultivada. La torre pasa de 3 a **4 niveles de hélice**: el nuevo usa las mismas caras que el nivel 2, así
que la planta no cambia y las holguras horizontales con las otras torres deberían seguir iguales. Queda un piso libre en el nivel 4. Área `Assembly_Area`, «Sala de asambleas»,
`SanctuaryAreaType.Assembly` (`SampleSceneBuilder.Asamblea.cs`). Piso de 40 × 38 m.

## La investigación: cómo se reúne la gente

| Sala | Qué aporta | Fuente | Se usa en |
|---|---|---|---|
| **Cinco tipos de pleno** (193 países): bancos enfrentados, semicírculo, herradura, círculo y aula | Comparar; que la forma no es neutra | XML (2016), *Parliament*; Psarra (UCL). Que el enfrentamiento cause confrontación es una idea extendida, y la Hansard Society la cree simplista | Se elige el **círculo** |
| **Westminster**: bancos enfrentados (de la capilla de San Esteban) | Lo que se evita: dos bandos | Hansard Society; Psarra | No se usa |
| **Hemiciclo**: los parlamentos continentales | Todos ven a quien habla | Wikipedia, *Hemicycle*; XML | Su **marcador de votos** |
| **Gran Consejo haudenosaunee** | Dos lados y un fuego en medio; consenso por turnos; un archivo sin papel | Onondaga Council; NYSED; Wikipedia | El **círculo** y el **archivo** |
| **Kgotla** de Botsuana | Todos pueden hablar y nadie interrumpe, y se decide por consenso. Históricamente, las mujeres quedaban fuera | Wikipedia, Participedia | El **turno de palabra** |
| **Althing** islandés, Lögberg (930–) | Quien habla, de pie sobre una roca, y la roca lleva la voz | Wikipedia, *Lögberg*, hurstwic.org | La **piedra de la palabra** |
| **Toguna** dogon (Mali) | Techo tan bajo que hay que sentarse: así no se llega a las manos | Wikipedia, *Toguna* | La **toguna de mediación** |
| **Reichstag** (Foster) | La cúpula de vidrio; la transparencia como idea. ArchDaily lo discute | ArchDaily (2021) | El **cono de luz** |
| **Atenas**: bouleuterion, kleroterion, clepsidra | Sorteo de cargos y turnos medidos con agua | Aristóteles, *Constitución de los atenienses* 63–69, **de memoria**: [verificar] | El **kleroterion** y la **clepsidra** |
| **ONU** | Interpretación simultánea | de memoria: [verificar] | La **cabina de intérpretes** |
| **Roble de Gernika** y **panchayat** bajo la higuera | Asamblea bajo un árbol | de memoria: [verificar] | Los dos **retoños** |
| **Quipu** inca | El acta en cuerdas | ya en [microcosmos-historias-3.md](microcosmos-historias-3.md) | El **archivo** |

## Qué tiene

- **72 plazas** (`AssemblySeats`; hoy 28 miembros): dos anillos cuadrados de filas que miran al centro, 3 m entre ellos. Sin cabecera y nadie en alto. Filas rectas, no curvas: giradas a 45° darían cajas mayores y romperían los 2 m (S7).
- **Piedra de la palabra** de 12 cm con su rampa (sin escalones: S5), bajo el cono de luz, en 4 postes finos.
- **Mesa de la comisión**, **archivo de actas**, **toguna de mediación** (6 plazas), **marcador de votos**, **cabina de intérpretes** de doble cristal, **kleroterion y clepsidra**, **dos retoños** y aseos.
- La franja del tronco (4,6 m) queda libre (F5). Medidas calculadas a mano.

## Vida en el santuario

- **Misiones** (rotación normal, J30): `asa_orden` (1 h), `asa_acta` (0,5 h), `asa_mediar` (1 h), con pasos en `Performances`, `PlayerTasks` y `VirtualMissions`.
- **Uniforme**: `Uniforms.Kind.Asamblea`. **Horario libre**: «asamblea», para todos, con la Cocina de reserva.
- **Máquina de virtualización**: Angostura, Onondaga, Madrid, Gettysburg y la Feria, ya **representadas** aquí.
- **Energía**: `FloorEnergy` y `EnergyRule` están en `claude/edif-energia`, no aquí. Al unir, añadir `SanctuaryAreaType.Assembly` a `EnergyRule.WattsPerM2`.

## Auditoría frente a las reglas

| Regla | Resultado | Se confirma con |
|---|---|---|
| S1, S2, S3, T3 | Por diseño: piso de la hélice | 28, 57 |
| S4 ≥ 16 m | **Sin comprobar**: planta igual, torre más alta | 57 |
| S5 sin escaleras | Rampa en la piedra | fotos |
| S6 sin muros ni techos | **Dos excepciones que decide Berón**: la tapa de cristal de la cabina (como en Música) y los listones de la toguna, que son un techo bajo por diseño | Berón |
| S7, S13, J16 | Calculado | 28, 75 |
| T13 lanzadera | La torre sube: la corona se valida sola | 90 |
| F5 | Franja libre, calculado | 57 |
| J20, J22, J30 | Uniforme, tareas con «E», rotación | 96, 97 |

Las cinco historias se montan ahora sobre otro piso: **hay que reconstruirlas**. Las órdenes de sus mapas pensadas para el Aula o la Cocina no encuentran pieza y no hacen nada; Angostura lleva dos nuevas. **Riesgo:** sin fogones ni fregaderos, el paisaje de Angostura cambia: mirar las fotos.

## Auditoría W11: S14 y F14 en todas las áreas

Está en `AreaDesign.All`, con su prueba (grupo 77). Criterio: investigado y ≥ 3 culturas. Lo he juzgado leyendo los docs y buscando en los constructores; no es un estudio pieza a pieza.

| Estado | Áreas |
|---|---|
| **Cumple** (5) | Sala de asambleas, Cocina (wok, comal, brigada), Dojo, Música y Aula (estas dos, arregladas ahora) |
| **Parcial** (8) | Sala de Yoga (2 culturas), Gimnasio, Sala de Ballet, Estudio Textil, Planta Atómica, Central Nuclear, Control de misión, Informática |
| **Falta** (9) | Hospital, Química, Biología, Construcción, Mecánica, Huerto, Calistenia, Cría, Carne Cultivada |

**Arreglado (lo barato):** instrumentos del mundo en el banco de Música (sitar, charango, kora, shamisen) y herramientas de saber en la mesa del Aula (ábaco, astrolabio, quipu, esfera). Son piezas dentro de muebles que ya estaban: no cambian los 2 m, pero sí el inventario: reconstruir sus microcosmos (§11).

**Propuesta para lo que falta** (no hecho): Huerto, con chinampa, zaï y terrazas, y Construcción, con bambú, tapia y ensamble japonés: son las que más ganan. El Hospital, con medicina de varias culturas, pide cuidado: solo lo que tenga evidencia.

Los microcosmos ya cumplen F14 en lo principal: cada pueblo lleva su estilo y sus fuentes (`MicroStyles`, planos). Los «esbozos» de `MicroStoryMaps` aún no.
