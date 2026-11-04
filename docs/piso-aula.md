# Piso del Aula: clases y sala de estudio para todos (2026-11-04)

Berón: un aula universitaria con biblioteca o sala de estudio. Y la **regla de capacidad** (manual S13): los centros de
entrenamiento y de estudio sirven a TODOS los miembros a la vez; **si un piso no basta, otro piso de esa área**, como el segundo
piso de yoga; el número sale de la lista del personal.

## Dónde y cuántos pisos

- La **torre del Entrenamiento** (es un centro de estudio, de la misma categoría que el entrenamiento), en su piso libre, encima
  del Dojo: planta de 36 × 26 m (`SampleSceneBuilder.Informatica.cs`, `BuildClassroom`). Área `Classroom_Area`.
- **Plazas por piso: 24** en el aula y 24 en la sala de estudio. **Pisos = ⌈miembros / 24⌉** (`AulaFloors`, con
  `WorkdayDirector.Staff`: hoy 18 miembros → **1 piso**). Si el personal pasa de 24, se añade «Planta_aula_2» (planta propia del
  Aula, como «Planta_ashtanga» del yoga) y la torre sube los niveles de hélice que hagan falta.

## Lo que hay en cada piso

- **Aula de clases** (−X): la tarima del profesor con su mesa, la pizarra y la pantalla del proyector (un solo mueble) y 4 filas de
  6 pupitres con su silla, mirando a la tarima, con 2 m de paso entre filas.
- **Sala de estudio y biblioteca** (+X): 4 mesas de estudio de 6 plazas (con libros y lámparas), las estanterías contra la pared (un
  solo mueble) y 2 puestos del catálogo y de ordenador (con la IA del santuario, piso de Informática).
- Aseo, el área de alfombras y la máquina de virtualización (con la historia de MacKenzie Scott en Prairie View).
- Puestos (`AreaSeat`): «clase» y «estudio» (categoría Estudio: ≥ miembros), «dar clase» (2) y «catálogo» (2).

## Misiones

`aul_clase` (dar o recibir una clase, 1 h, papeles maestro y alumno) y `aul_estudio` (estudiar en la sala de estudio, 1 h).

## Pruebas

- **Grupo 74** (`InformaticaAulaTest`): el Aula encima del Dojo, filas y mesas para todos los miembros, los pisos que tocan, la tarima
  y la biblioteca.
- **Grupo 75** (`CapacidadPisosTest`): «clase» y «estudio» ≥ miembros; y el resto de los centros de entrenamiento (gimnasio con su
  cardio, las dos salas de yoga) frente a los miembros. El **ring de almohadas** aún no existe: el validador lo apunta como pendiente.
- **Grupo 76** (`AlertaPisoNuevoTest`): Scott re-apuntada aquí; la alerta señala a Menfis, Alejandría, Bagdad, Crotona y Sils Maria.
