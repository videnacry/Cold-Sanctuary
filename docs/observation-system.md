# Sistema de Observación — Cold Sanctuary

> **Estado real (auditoría 2026-07-09):** este documento es diseño/visión, casi nada está
> implementado. Solo existe el float `observationRadius` en `PlayerStats`. No existen
> `ObservableObject`, `IObservable`, `ObservationManager` ni el modo contemplativo. El
> contenido de visión se mantiene íntegro debajo para referencia de diseño.

Documento de diseño del sistema de Observación: radio de consciencia, señales diegéticas, modo contemplativo y crecimiento de la stat.

Ver también: DEVLOG.md §Stat: Observación, `docs/ibody-imind.md` (la stat vive en `IMind`).

---

## Principio central

La Observación es la capacidad del jugador de leer el entorno con detalle.
**No es una herramienta para conseguir cosas — es una práctica en sí misma.**

A veces puedes mirar sin buscar algo. Simplemente mirar por ver qué hay allí.
Los jugadores con poca Observación pasan por alto cosas que están ahí delante.
Los jugadores con mucha Observación ven el mundo de manera diferente.

---

## La stat — `observationRadius`

`PlayerStats.observationRadius` es el radio, en unidades de mundo, dentro del cual el jugador percibe señales del entorno. Valor inicial: 3 unidades.

El radio crece visiblemente con la stat — el jugador siente el cambio sin que nadie se lo diga. Panterilia suma un bonus mientras el jugador está cerca de ella.

---

## Radio de consciencia — cómo funciona

Dentro del radio, los objetos observables emiten una señal **sutil y diegética**:
- Un brillo leve en el material
- Una partícula suave
- Un sonido ambiental diferente

No hay borde de UI genérico. No hay marcador de objetivo. El jugador aprende a leer el lenguaje del mundo, no un HUD.

Al acercarse lo suficiente al objeto, aparece un **pensamiento interno breve** — no de UI:
- *"esto huele diferente"*
- *"las hojas de esta planta tienen algo"*
- *"este animal está haciendo algo que no he visto antes"*

El pensamiento es del personaje, no un tooltip del sistema.

---

## Modo contemplativo

El jugador puede detenerse y **simplemente mirar el entorno** — sin objetivo, sin timer, sin presión de progresión.

El mundo devuelve algo: no siempre un ítem o pista. A veces un detalle, una textura, un comportamiento de animal que no vería en movimiento. A veces nada — y eso también es válido.

Practicar el modo contemplativo **sube la stat de Observación** por sí solo, independientemente de los compañeros. Mirar como práctica, coherente con el alma del juego.

---

## Usos por nivel

| Nivel | Uso |
|---|---|
| **Nivel 1** | Notar ingredientes naturales, comportamientos de animales, detalles del entorno que Panterilia sí ve y el jugador no. Primera misión: Panterilia detecta plantas e insectos; el jugador aprende a mirar. |
| **Nivel 2+** | Puente hacia la meditación (observación interior). Base para recetas mágicas con la tabla periódica. Panterilia asciende a formadora; su Observación y la del jugador que la acompaña crecen juntas. |

---

## Preguntas de diseño abiertas

### 1. ¿Cómo se marca un objeto observable?

**Opción A — Componente `ObservableObject`:**
```csharp
public class ObservableObject : MonoBehaviour {
    public string innerThought;    // "esto huele diferente"
    public float  signalRadius;    // radio mínimo para disparar el pensamiento
    public ParticleSystem signal;  // emisor de partículas (asignado en Inspector)
}
```
Simple de iterar en escena. Inspector-friendly para Berón. El `ObservationManager` hace `FindObjectsOfType<ObservableObject>()` y filtra por distancia.

**Opción B — Interfaz `IObservable`:**
```csharp
public interface IObservable {
    string GetInnerThought();
    void OnObserved();   // callback cuando el jugador lo percibe
}
```
Más flexible — `Animal.cs` puede implementar `IObservable` directamente sin componente extra.

**Pendiente:** decidir cuál usar, o si `ObservableObject` implementa `IObservable` internamente.

---

### 2. ¿Cómo se muestra el pensamiento interno?

| Opción | Descripción | Ventaja | Riesgo |
|---|---|---|---|
| **Overlay de texto** | Texto en pantalla (estilo subtitle, fondo semitransparente) | Simple de implementar | Rompe inmersión, parece UI |
| **Pausa diegética** | El jugador se detiene 0.5s, el texto aparece como si el tiempo se ralentizara | Integrado al ritmo | Más complejo, puede ser invasivo |
| **Diálogo interno en audio** | Voz over del personaje — susurro breve | Máxima inmersión | Requiere producción de audio, localización |
| **Pensamiento flotante diegético** | Texto que flota en el espacio 3D, atado al objeto | Original | Raro en primera persona, puede marear |

**Recomendación inicial:** overlay de texto sutil (fuente manuscrita, sin fondo, aparece/desaparece en fundido) durante prototipado. Migrar a audio cuando haya producción.

**Pendiente:** decidir estilo visual y si se puede desactivar en opciones (jugadores que prefieren explorar sin texto).

---

### 3. ¿Cómo se activa el modo contemplativo?

| Opción | Input | Descripción |
|---|---|---|
| **Timer de quietud** | Ninguno — auto | Si el jugador está quieto N segundos sin input, entra automáticamente en modo contemplativo |
| **Gesture/botón** | Tecla dedicada (p.ej. `C`) | El jugador activa y desactiva conscientemente |
| **Stance (postura)** | Combinación de teclas | Agacharse + no moverse durante 2s = modo contemplativo |
| **Asana** | Postura de meditación en la Palette | El modo contemplativo es una asana más — integrado al sistema de Asanas |

**Ventaja del timer de quietud:** el juego recompensa no hacer nada, sin que el jugador tenga que "activar" algo. Más elegante.
**Riesgo:** puede activarse accidentalmente durante carga de acción o navegación lenta.

**Recomendación:** timer de quietud con N = 3–4 segundos. Movimiento de ratón o cualquier tecla de movimiento cancela.

**Pendiente:** definir qué pasa exactamente al entrar (efectos visuales, sonido, UI, cámara).

---

### 4. ¿Cómo crece `observationRadius` permanentemente?

| Mecanismo | Descripción | Estado |
|---|---|---|
| **Uso de modo contemplativo** | Cada X segundos en modo contemplativo sube la stat un micro-incremento acumulativo | ✅ Diseñado (DEVLOG §Stat Observación) |
| **Bonus de Panterilia** | +1.5 mientras el jugador está cerca. Temporal, no permanente | ✅ Implementado (`Panterilia.cs`) |
| **Milestone por BondActivity** | Superar umbral de bond con Panterilia desbloquea incremento permanente | 🔲 Pendiente diseño |
| **Desbloqueo narrativo** | Al completar cierta misión con Panterilia el radio sube definitivamente | 🔲 Pendiente diseño |

**Nota importante:** distinguir entre `observationRadius` como stat base (que crece) y el bonus temporal de Panterilia (que se suma encima). El código ya separa los dos — `Panterilia` modifica la stat directamente, lo que requiere cuidado para no acumular bonuses si el jugador entra/sale repetidamente. Posible fix: `observationRadius` tiene una parte base (permanente) y Panterilia trabaja sobre la base, no acumula.

**Pendiente:** decidir si el radio tiene un cap y cuál es.

---

## Código pendiente

| Componente | Descripción |
|---|---|
| `ObservableObject.cs` / `IObservable.cs` | Marker para objetos que el jugador puede percibir dentro del radio |
| `ObservationManager.cs` | Attached al Player. Cada frame (o cada 0.1s) compara `observationRadius` con distancias a objetos observables. Dispara señales visuales y pensamientos internos. |
| Señal diegética | Sistema de partículas / material swap en el objeto observable. Se activa cuando está dentro del radio. |
| Pensamiento interno | UI Canvas (world space o screen overlay) con el texto del pensamiento. Con animación de aparición/desaparición. |
| Modo contemplativo | Detección de quietud en `PlayerCtrl` + coroutine que sube `observationRadius` mientras dura. |
| `ObservationRadius` base vs. bonus | Separar el valor base (permanente) del bonus Panterilia para evitar acumulación incorrecta. |

---

## Relación con otros sistemas

| Sistema | Conexión |
|---|---|
| `PlayerStats.observationRadius` | La stat es el radio. Vive en `PlayerStats` (candidata a migrar a `IMind`) |
| `Panterilia.cs` | Bonus temporal de +1.5 mientras el jugador está cerca |
| Modo contemplativo | Sube `observationRadius` por uso — feedback mecánico de la práctica |
| `BondActivity` | "Observación con Panterilia" puede ser una BondActivity pasiva con trigger `panterilia_nearby` |
| `IPaletteEvaluator` | A futuro: observar cierto número de objetos en un radio podría ser la condición de desbloqueo de alguna fórmula |
| `IMind` | `observationRadius` pertenece al estado mental/perceptivo — migra a `IMind` en la refactorización IBody/IMind |

---

## Pendientes de diseño

- [ ] Decidir entre `ObservableObject` (componente) vs `IObservable` (interfaz)
- [ ] Decidir cómo se muestra el pensamiento interno al jugador
- [ ] Definir input/trigger del modo contemplativo (timer vs botón vs asana)
- [ ] Definir si `observationRadius` tiene cap y cuál
- [ ] Separar `observationRadius` base (permanente) del bonus temporal de compañeros en `PlayerStats`
- [ ] Definir lista de objetos observables en el Nivel 1 y sus pensamientos internos
- [ ] Decidir si el modo contemplativo se puede activar en movimiento (modo "paseo contemplativo") o solo en quietud
- [ ] Definir feedback visual cuando el jugador está en modo contemplativo (sutilidad vs. legibilidad)
