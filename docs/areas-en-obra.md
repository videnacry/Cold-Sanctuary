# Áreas en obra: solo lo básico queda terminado

Berón, 2026-11-09: para que haya menos clones, solo quedan terminadas las áreas básicas. Son los servicios básicos de humanos y animales, la construcción misma, el gimnasio y el yoga. Lo demás vuelve a estar en obras.

El criterio para lo dudoso es **la necesidad básica de humanos o animales**.

Código: `AreasEnObra` (la lista, el filtro y la conversión de la escena), `AreaWork.ObraMissions` y `ConstructionProject.reform`. Prueba: grupo 113. Reparto: [jornada-y-rotacion.md](jornada-y-rotacion.md) §Reparto por santuarios. Tareas por área: [tareas-por-area.md](tareas-por-area.md).

## Qué queda terminado y qué vuelve a obra

| Área | Queda | Por qué |
|---|---|---|
| Hospital | **terminada** | salud de humanos y animales |
| Cocina / restaurante | **terminada** | la comida |
| Guardia y Sala de guardia | **terminadas** | seguridad (depredadores, icebergs, roca, el árbol cazador) |
| Huerto | **terminada** | la comida y el material de obra (guadua) |
| Huerto vertical | **terminada** (ronda 31) | las estanterías LED que eligió Berón: comida |
| Cría | **terminada** | cuidado de las crías |
| Carne Cultivada | **terminada** | alimenta a los depredadores, y el micelio es material de obra |
| Energía, Planta Atómica, Central Nuclear | **terminadas** | la energía (soporte de vida) |
| Línea de las cavernas | **terminada** | seguridad del sitio y el refugio |
| Mecánica | **terminada** (inferido) | las alfombras son el transporte de todos y la maquinaria de todas las áreas |
| Construcción | **terminada** | la obra misma |
| Gimnasio y Sala de Yoga | **terminadas** | Berón |
| Química | **en obra** | ciencia |
| Biología | **en obra** | ciencia; **sus fotobiorreactores (el O₂) siguen funcionando** |
| Informática | **en obra** | ciencia |
| Aula | **en obra** | formación |
| Sala de asambleas | **en obra** | personajes nuevos |
| Música | **en obra** | personajes nuevos |
| Estudio Textil | **en obra** | personajes nuevos; **su cuerda para las obras sigue** |
| Control de misión (Aéreo) | **en obra** (Berón 2026-11-09) | no es básico. Su área está en la escena aérea: la obra se lleva desde el muelle aéreo (`AreasEnObra.RemoteAreas`) y la hacen los del Aéreo (sus tareas de obra son de su área: `ObraArea`). Al cargar la escena se enseña en obras (`SyncLoaded`). **Siguen solo sus emergencias**: el remolque de un iceberg y el vaciado de combustible. La revisión rutinaria de la plataforma (`lanz_seguridad`) no, porque no hay lanzamientos mientras está en obra (ronda 52). |

Sala de Ballet, Dojo y Calistenia ya eran obras y siguen igual.

## Cómo funciona

- **Sin tareas de área.** `AreasEnObra.Filter` quita de la lista del día las tareas de un área en obra. Se quedan dos cosas:
  - el O₂: `bio_fotobio*`;
  - el material de obra: `mat_*`.
- **Sus horas pasan a la obra.**
  - Cada día hay 1 h de material y un equipo de turnos de 2 h (`AreasEnObra.CrewFor`): con la hora de material, tantas horas como pedía el área.
  - Son 6 días de obra (inferido: los del Dojo y el Ballet).
  - Estas tareas de obra son **opcionales**: las hace quien tenga tiempo en el reparto por partes iguales y **nunca piden un clon**. Si nadie tiene tiempo, la obra espera ese día.
- **En el mundo.** Al empezar, `WorkdayDirector` llama a `AreasEnObra.Apply`.
  - Cada área no básica recibe una `ConstructionProject` con `reform`.
  - Enseña su forma en construcción, «Forma_en_obra» (F1): andamio, palés, conos y muretes.
  - Lleva el cartel de obra con su etapa y su porcentaje.
  - Al terminar, la forma en obra se oculta.
  - Los días hechos se guardan como los de cualquier obra (`cs_obra_<título>`).
  - El horario libre ya no usa sus salas mientras están en obras.
- **Limitación:** estas áreas no tienen «Acabado» aparte, así que sus muebles siguen a la vista bajo el andamio. Para que la obra los coloque pieza a pieza habría que reconstruir la escena moviéndolos a un «Acabado», como en el Ballet.
- **En las pruebas no se convierte nada** (salvo `AreasEnObra.InTests`): las pruebas de esas áreas (74, 77…) siguen viéndolas terminadas.
- **Jardines sobre obras** (ronda 33): las macetas de un techo que está sobre un piso en obras (`FruitGardener.OnSite`) no entran en las rondas hasta que la obra acabe, porque nadie cuida un huerto encima de una obra. El registro `[Frutales] N macetas sobre pisos en obras` dice cuántas y de qué techos. Depende de que el techo cuelgue del piso de arriba del edificio: si cuelga de otro sitio, no se detecta.

## Los clones: antes y después (modelo de 113)

El modelo tiene 28 humanos en la tierra, el catálogo fijo, 27 rondas de frutales y 4 obras de 7 h (como en vivo). La prueba escribe los clones de cada día en el log, con la línea `[Reparto] Modelo (28 humanos en la tierra): antes N clones/día … con las áreas en obra M`. Exige que M < N.

Calculado a mano:
- **Antes:** unas 148 h obligadas para 140 h de los humanos, unos 2 o 3 clones al día.
- **Después:** unas 138,5 h obligadas, 0 clones (o 1, según cómo encajen las tareas).

Las obras nuevas son opcionales y se hacen con lo que sobre.
