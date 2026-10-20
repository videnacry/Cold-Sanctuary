# Atención visual y hechizo «enfocar»

Código: `Assets/Scripts/Mind/VisualAttention.cs`. La cámara usa el resultado en `CameraManager`, y el lanzador en `SpellConsole`: hechizo `enfocar` y sugerencias.

## La idea (y lo que dice la ciencia)

- **Lo que se ve nítido es poco.** La **fóvea** cubre unos 2°. La mirada **salta** (sacadas, 3–4 por segundo) hacia lo más **saliente**, y la sensación de «ver todo» la construye la atención.
- **Saliencia de abajo arriba** (modelo de Itti y Koch): el **contraste** de color e intensidad, el **movimiento** y lo que **se acerca** atraen la mirada sin quererlo. La periferia es muy sensible al movimiento.
- **De arriba abajo**: los objetivos (la tarea, la misión) y la **carga emocional** guían la mirada. Lo que nos importa en los dos extremos llama más: lo que amamos (lo buscamos) y lo que tememos u odiamos (lo enfrentamos o huimos).
- **Diferencias individuales**: hay quien atiende más a lo que se mueve y quien a lo quieto o a los colores apagados, según su experiencia. Por eso cada anima tiene un **perfil** con pesos numéricos.
- **No hay «zoom» en el ojo.** El estrechamiento es de la **atención**. En pantalla se simula con un leve cierre del campo de visión (FOV) hacia lo enfocado.

## El sistema

- **Saliencia** = contraste × peso + cercanía × peso + movimiento × peso + vínculo (|valor|, los dos extremos) × peso + misión × peso.
  - Una presa nota además a un depredador.
  - Solo cuenta lo que está **en su campo visual**, **a su alcance** (escalado por su percepción) y **no tapado** (rayo).
- **Ojos**: cada anima tiene campo visual (grados), alcance (m) y sensibilidad al movimiento y al color.
  - Humanos: 200°.
  - Las presas podrán tener campos muy amplios y los depredadores, frontales.
  - Así hay diversidad de ojos.
- **Perfiles**:

  | Perfil | Atiende sobre todo a |
  |---|---|
  | Equilibrado | todo por igual |
  | Vigilante | movimiento y cercanía (Gohageneis) |
  | Esteta | colores que destacan (Irosene, Goluis) |
  | Contemplativo | lo quieto y apagado: pesos de movimiento y contraste negativos (Sadhguru, la Magnate) |
  | Afectivo | los vínculos (Lizmibet, Andrea) |

- **Enfocar inconscientemente**: cada 0,25 s el anima enfoca lo más saliente (evento `Focused`). Si está dormido, no ve nada.
- **Enfocar a conciencia**: `enfocar({destiny:"Magnate", seconds:8})` fija la atención. Sin destino, enfoca lo más saliente.
- **Cámara en primera persona**: lo enfocado se «acerca» hasta 8° de FOV (14° si es a conciencia) y vuelve a abrirse al soltarlo.
- **Lanzador**: las sugerencias de destino ponen primero lo que más te llama la atención ahora, después tu itinerario y las personas por vínculo.

## Pendiente

- Desenfoque de profundidad (URP Depth of Field) hacia lo enfocado.
- Ojos por especie para la fauna, y que la atención alimente su receptividad al peligro.
- Que la mirada del personaje (cabeza y ojos) gire hacia lo que enfoca.
