# La 1ª virtualización de YOGA — upa-yoga de cuello (2026-07-31)

La **primera virtualización de yoga** del Mesocosmos: la práctica de **cuello de upa-yoga** ("Yoga for
Success" de Sadhguru). Va **tras dormir** (ver [`novela.md`](novela.md) "orden de revelación"), y enseña al
jugador a **habitar el cuerpo por partes**. Código: `Assets/Scripts/Virtualization/UpaYogaSession.cs`.
*(Los conteos exactos de repeticiones conviene verificarlos contra el vídeo oficial de Isha.)*

## 1. Mecánica: **poseer el cuerpo por partes** (relevancia/orquesta)
Extiende la posesión (`PossessionSpell`/`AnimaController` por relevancia): de *poseer un cuerpo* a **poseer un
miembro**. En cada fase, los **dos grupos de teclas cambian de dueño** y controlan una parte distinta; así el
jugador la **siente**. Los grupos son los que ya existen:
- **WASD** = grupo izquierdo (las de caminar/`PlayerController`).
- **IJKL** = grupo derecho (las de la cabeza/`HeadLook`: I arriba, K abajo, J izq., L der.).

## 2. La postura base (antes de mover cuello/hombros)
1. **Pies:** WASD → **pie izquierdo**, IJKL → **pie derecho**. Sepáralos cómodos a la altura de las caderas,
   mirando al frente.
2. **Hombros:** bájalos y alinéalos. (**Manos** caen y **espalda** se endereza **automáticas**.)

## 3. La secuencia de cuello (respiración = el eje)
| # | Movimiento | Inhala | Exhala |
|---|---|---|---|
| 1 | Cabeceo adelante/atrás | arriba | abajo |
| 2 | **Giro puro** (barbilla al hombro, o rebasándolo) | a los lados | al centro |
| 3 | Oreja al hombro (inclinación) | al centro | bajando la oreja al hombro |
| 4 | **Rotación de cuello** (= la "diagonal": arriba-y-atrás → abajo-y-adelante) | 1ª media vuelta (sube por atrás) | 2ª media vuelta (baja por delante) |
| 5 | Rotación de **hombros** | subiendo | bajando · **3 por atrás + 3 por delante** |

- **La respiración es el ritmo del reto**: mueves en la dirección enseñada **en la fase correcta del aliento**
  (`UpaYogaSession` muestra una **barra inhala/exhala**). En cuello, el grupo activo es **IJKL**; en hombros
  (mov. 5), **IJKL → hombro derecho** y **WASD → hombro izquierdo**.
- **Nada de círculos descuidados**: la única rotación (mov. 4) es **lenta y consciente, partida por el
  aliento** — no los "neck rolls" rápidos contra los que se advierte.

## 4. Mecánica de **ritmo (tipo Guitar-Hero)** + paneles-tecla
La UI explicativa se vuelve **jugable**: sobre los paneles-tecla (con **forma de teclado**: W sobre A-S-D, e I
sobre J-K-L) **caen fichas**. Si pulsas la tecla **justo** cuando la ficha llega a su panel → **acierto**
(+punto); si no → **fallo** (−punto). Encima, **título** = parte del cuerpo, **subtítulo** = instrucción, y una
**barra de aliento** (inhala/exhala) que es el **pulso** de las fichas.

- **El cuerpo va orquestado** (siempre sale bien): fallar el ritmo **no rompe la postura**, solo hace
  **temblar** al jugador (la UI se sacude) y le afecta **aliento / energía / fatiga**. Acertar da **energía y
  recuperación**; fallar **gasta**, hasta **descansar / comer / otra actividad** que reponga. **Más puntos =
  más recompensa.** Esto **facilita** la simulación y la vuelve juego.
- **Las fichas son elementos.** Por dentro cada ficha es un **elemento de la tabla periódica** que, **en
  orden**, formaría el **compuesto** que ese movimiento **libera** (enlaza con `Chemistry`). Por fuera:
  - **Opción A (elegida):** la ficha muestra **solo la letra de la tecla** (W/S… alineadas vertical, se sabe
    cuál es por la letra); el elemento queda **oculto**.
  - **Opción B (variante, un flag `showElement`):** muestra el **elemento**; y en vez de caer, puede
    **aparecer pequeño sobre la tecla y crecer** hasta llenar el panel (pulsar antes de que se llene). *(Hoy
    implementada la A; la B-creciente queda como variante a añadir.)*
- Bajo cada cluster, **qué parte controla** ese grupo. Las teclas **se resaltan al pulsarlas**.

**UI-mix (decidido):** paneles y textos van con **OnGUI** aquí (ocasión de mix aceptada); **candidatos a
migrar a `FollowingArrays`** si se quieren en el Canvas persistente. No es UI suelta por descuido.

## 5. Encaje en el flujo
Kushal hace el upa-yoga de cuello **tras dormir**; después va como **avatar a la Enfermería** (viaje 3: guiar
a las hormigas a un refugio seguro mientras se desinfecta para tratar a un herido). Salida del Micro por
**auto-expulsión** en beats guionizados (ver `novela.md`).

## 6. Estado
- **Hecho (scaffold jugable):** `UpaYogaSession` — 7 fases (postura base ×2 + cuello ×4 + hombros), **remapeo
  de qué parte controla cada cluster**, **motor de ritmo Guitar-Hero** (fichas caen por carril, ventana de
  acierto, puntuación, aciertos/fallos), **temblor** en la UI al fallar + medidores internos de **energía/
  fatiga**, **barra de aliento** como pulso, **paneles-tecla con forma de teclado + letra** (Opción A;
  `showElement` para la B), compuesto liberado por movimiento, avance con **F**/auto, `Active` estático.
  Sandbox `UpaYogaSandbox_AUTO` en `SampleSceneBuilder`.
- **Falta:** articular el **avatar rigged** (mover cuello/hombros/pies) y **suprimir el input normal** mientras
  dura; cablear los efectos a **`PlayerStats`/humores** de verdad (hoy son medidor interno + logs); la
  **Opción B creciente**; el mapeo **elemento→compuesto** real (con `Chemistry`); el **QTE de hombros** (3+3);
  verificar repeticiones con el vídeo de Isha; opción de migrar paneles a `FollowingArrays`.
