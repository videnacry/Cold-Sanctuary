# Transporte entre plataformas: puentes y alfombras voladoras

- Las plataformas **cercanas** (a menos de 32 m) se unen con **puentes de 8 m de ancho** (rectos, a veces en diagonal; primero las más cercanas, sin puentes de sobra):
  por ejemplo el restaurante y la enfermería. Dentro de un grupo se va andando.
- Cada **plataforma** tiene **su pista de aterrizaje** encima de su piso más alto, a 10 m, como un piso más de la plataforma multinivel (Berón 2026-10-23). Es también una **vía de escape** por el techo si la estructura falla. La arboleda residencial tiene la suya.
  - Es como una sala de yoga con **esterillas grandes**, que son **alfombras voladoras**.
  - Solo si ninguna plataforma de un grupo tiene sitio para una pista de 14 × 14 m se hace una aparte, unida con un puente.
  - Entre plataformas del **mismo grupo** (unidas por puentes) los trabajadores van andando (`CarpetPad.group`). Las alfombras se usan para ir a otro grupo.
  - Solo hay pistas **arriba** (Berón 2026-10-23: se probó también una pista colgada debajo de los edificios de varios pisos y se quitó, porque complicaba la construcción).
- Las alfombras son **automáticas**:
  - Al **tocarla** se muestran los destinos (las otras pistas) en un diálogo: ↑/↓ + Intro o clic.
  - La cámara pasa a **3ª persona**; el pasajero va **sentado** mientras la alfombra sube, cruza y baja.
  - Al llegar se **levanta** y vuelven a mostrarse las opciones (seguir o bajarse).
- Los **trabajadores** la usan solos: si su siguiente área está en otro grupo, van a su pista y vuelan
  (`CarpetPad.Route`, desde la jornada).
- Mientras hay un **diálogo** abierto (alfombra, máquina de virtualización, confirmaciones…) Intro es suyo:
  el lanzador de hechizos se **oculta**.

Código: `SampleSceneBuilder.Transport.cs` (puentes y pistas), `CarpetPad`, `FlyingCarpet`, `ChoiceDialog`.
