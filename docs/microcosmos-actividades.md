# Etiquetas de actividad de los edificios de los microcosmos (2026-11-03)

Berón: los edificios «deben crearse junto a su interacción con los humanos y humanos-insecto … para poder pertenecer a sus áreas de
trabajo, descanso, recuperación u ocio, de modo que formen parte de sus actividades semanales».

Esto es el **gancho** para el horario semanal (lo lleva otro hilo: el horario de tiempo libre y la jornada). Cada edificio de un
microcosmos (`AreaFunctionSite`, `Assets/Scripts/Microcosmos/AreaFunctionSite.cs`) trae, puesto al construirse
(`CivicBuilder.Tag`; regla 8 de [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md)):

| Campo | Tipo | Qué es |
|---|---|---|
| `activity` | `string` | `AreaFunctionSite.Trabajo` («trabajo»), `Descanso` («descanso»), `Recuperacion` («recuperacion»), `Ocio` («ocio») o `Entrenamiento` («entrenamiento», desde el 2026-11-04): para qué va quien lo VISITA |
| `training` | `string` | (2026-11-04) si es de entrenamiento, su CLASE ESTABLE de época y lugar (`gimnasio_griego`, `akhara`, `furusiyya`, `wuguan`, `turnhalle`…; `MicroStyles.TrainingKind`; ver [microcosmos-entrenamiento-eras.md](microcosmos-entrenamiento-eras.md)) |
| `mapSource` | `string` | (2026-11-04) de dónde sale su posición: vacío = plano histórico o la historia; «mapa actual: …» = de un mapa de hoy (regla 16) |
| `hasWork` | `bool` | si además tiene puestos de trabajo (alguna función que no es descansar ni comer ni de «todos») |
| `capacity` | `int` | cuántas personas lo usan a la vez (camas, asientos de sus mesas, puestos, sitio en lo que se pisa: 4 por tatami, pista, piscina o andén) |
| `interaction` | `string` | lo que se hace allí, en una frase («bailar y aprender los pasos», «bañarse, nadar o sudar en el baño de vapor»…) |
| `bodies` | `string` | para qué cuerpos tiene muebles a la medida («Humano, Centauro (Ant), Cúpula (Ladybug)…»): siempre humanos e insectos-humanos (regla 9) |
| `building` | `string` | la plantilla del registro (`AreaFunctions.Buildings`): sus funciones (`Functions(año)`), cada una con sus puestos (`StationFor`) y papeles |
| `region` | `string` | su región del catálogo de estilos (`MicroStyles.Region`) |

## Qué categoría tiene cada plantilla (`AreaFunctionSite.ActivityOf`)

| Categoría | Plantillas |
|---|---|
| **descanso** | Casa, Orfanato, Hotel |
| **recuperacion** | Hospital, Sala de Yoga, Baños (piscina, hammam, temazcal), Templo |
| **ocio** | Restaurante, Parque, Plaza (y monumento), Museo, Sala de baile, Mirador, Fuente pública |
| **entrenamiento** (2026-11-04) | Centro de entrenamiento (con su clase de época: gimnasio griego, akhāṛā, maydān de furusiyya, Turnhalle, juego de pelota…), Dojo |
| **trabajo** | Cocina, Taller, Escuela, Mercado, Laboratorio, Huerto, Granero, Alfarería, Lavandería, Oficina de patentes, Taller de papeles, Trinchera, Comisaría, Bomberos, Guardería, Estación, Carpintería, Obra, Centro de tecnología, Cuartel |

## Cómo usarlo desde el horario

```csharp
// Los sitios de ocio de la escena cargada con sitio libre para un insecto-humano:
foreach (AreaFunctionSite s in AreaFunctionSite.All)
    if (s.activity == AreaFunctionSite.Ocio && s.bodies.Contains("Centauro")) { /* … */ }
// Qué hacer allí y en qué mueble:
var f = site.Pick(role, salt);              // una función del registro para su papel
AreaFunctionStation st = site.StationFor(f); // su puesto (libre si lo hay)
st.Do(f, who);                               // hacerla (who: el GameObject de quien la hace; cuenta usos)
```

Las funciones nuevas (2026-11-03, `AreaFunctions`): `pol_atender`, `pol_ronda`, `bom_guardia`, `bom_ejercicio`, `mus_visitar`,
`mus_conservar`, `gua_cuidar`, `gua_jugar`, `bai_bailar`, `bai_ensenar`, `est_viajar`, `est_taquilla`, `hot_recepcion`,
`hot_dormir`, `car_madera`, `obr_levantar`, `obr_preparar`, `tec_operar`, `mil_guardia`, `mil_instruccion`, `ban_banarse`,
`ban_vigilar`, `par_pasear`, `par_cosechar`, `pla_reunirse`, `pla_contemplar`, `ent_entrenar`, `ent_ensenar`, `fue_agua`,
`mir_mirar`. Papeles nuevos: guardia, bombero, conservador, cuidador, tecnico, carpintero, albanil (y «todos»).

## Las cuatro categorías de un pueblo (regla 16, 2026-11-04)

Todo pueblo de historia tiene al menos un edificio de trabajo, uno de ocio, uno de entrenamiento y uno de recuperación (`AreaFunctionSite.TownCategories`; lo comprueba `MicroFloors.ValidateCategories`). Si el plano histórico no trae alguno, su posición sale de un mapa de hoy y se recrea con lo nativo de la época. Ver [microcosmos-reglas-edificacion.md](microcosmos-reglas-edificacion.md), regla 16.

**Para el horario libre.** Los sitios de entrenamiento se ofrecen como hasta ahora, del tipo «ocio» del roster (`LeisureDirector`). Los planes de entrenamiento leen `training` y las funciones `ent_*`; ver [gimnasio-maquinas.md](gimnasio-maquinas.md), en «Microcosmos: lugares de entrenar por época».
