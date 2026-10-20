# Transporte entre plataformas: puentes y alfombras voladoras

- Las plataformas **cercanas** (a menos de 32 m) se unen con **puentes de 8 m de ancho** (rectos, a veces en diagonal; primero las más cercanas, sin puentes de sobra):
  por ejemplo el restaurante y la enfermería. Dentro de un grupo se va andando.
- Cada **grupo** de plataformas unidas por puentes tiene **una pista de aterrizaje** (y la arboleda residencial, la suya):
  - es como una sala de yoga con **esterillas grandes**: son **alfombras voladoras**;
  - va pegada a una de sus áreas (la mayor, por un lado libre) con un puente.
- Las alfombras son **automáticas**:
  - Al **tocarla** se muestran los destinos (las otras pistas) en un diálogo: ↑/↓ + Intro o clic.
  - La cámara pasa a **3ª persona**; el pasajero va **sentado** mientras la alfombra sube, cruza y baja.
  - Al llegar se **levanta** y vuelven a mostrarse las opciones (seguir o bajarse).
- Los **trabajadores** la usan solos: si su siguiente área está en otro grupo, van a su pista y vuelan
  (`CarpetPad.Route`, desde la jornada).
- Mientras hay un **diálogo** abierto (alfombra, máquina de virtualización, confirmaciones…) Intro es suyo:
  el lanzador de hechizos se **oculta**.

Código: `SampleSceneBuilder.Transport.cs` (puentes y pistas), `CarpetPad`, `FlyingCarpet`, `ChoiceDialog`.
