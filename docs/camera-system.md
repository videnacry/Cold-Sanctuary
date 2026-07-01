# Sistema de Cámara — Cold Sanctuary

Documento de diseño del sistema de cámara, efectos visuales y su relación con el estado interno del jugador.

---

## Principio central

La cámara nunca miente. Lo que le pasa al jugador por dentro se ve en cómo ve el mundo. Los efectos visuales no son decoración — son el lenguaje visual del estado del jugador.

A medida que el jugador desarrolla su stat de **Satisfacción**, deja de ser un receptor pasivo de estos efectos y pasa a poder *elegirlos* — la pantalla se convierte en expresión artística propia.

---

## Modos base de cámara

El jugador elige su modo base en cualquier momento:

| Modo | Descripción |
|---|---|
| **3ª persona** | Vista por defecto. Perspectiva sobre el hombro o aérea. |
| **1ª persona** | Inmersión total. Se activa automáticamente en ciertas zonas (área de crías) o por preferencia del jugador. |

---

## Robos de cámara

Cambios automáticos que el juego propone en momentos clave. El jugador puede **desactivarlos** en opciones si prefiere mantener el control total.

| Situación | Robo |
|---|---|
| Entrar al área de crías | Switch a 1ª persona — "aquí hay que prestar atención de otra manera" |
| Cría hace algo significativo (primer paso, primera exploración) | Órbita cinemática breve alrededor del momento |
| Animal en estado agresivo / ataque inminente | Robo hacia el animal, vibración sutil, pausa de un fotograma — el jugador que aprendió lenguaje corporal lo anticipa, el que no, lo recibe |
| Momento narrativo importante (maestra aparece, primera asana) | Cinemática controlada |

Los robos son sugerencias del juego, no imposiciones. El sistema consulta la preferencia del jugador antes de ejecutar.

---

## Efectos visuales — estado del jugador

### Estados físicos

| Estado | Efecto visual |
|---|---|
| **Frío** | Cristalización en bordes de pantalla, respiración visible, paleta azulada |
| **Cansancio** | Enrojecimiento, bordes borrosos, vignette que pulsa como párpado pesado |
| **Sueño extremo** | Tambaleo de cámara, apagones intermitentes, sonidos distorsionados |
| **Estrés alto** | Vibración de cámara, aberración cromática, latido audible |

### Estados emocionales / de compañía

| Estado | Efecto visual |
|---|---|
| **Goluis activo (presión)** | Cámara más tensa, campo de visión ligeramente cerrado, sensación de urgencia física |
| **Satisfacción alta (Gohageneis)** | Movimientos de cámara más suaves, colores más cálidos y saturados, sin brusquedades |
| **Modo contemplativo (Panterilia)** | Cámara casi estática, profundidad de campo lenta, como respirar hondo |
| **Fatiga mental al límite** | Desaturación progresiva — el mundo pierde color |
| **Vínculo muy alto con una cría** | Momentos clave de la cría activan órbita cinemática breve |

---

## Modo artístico — desbloqueo por Satisfacción

Con la stat de **Satisfacción** suficientemente desarrollada, el jugador desbloquea la capacidad de *elegir* el estado visual de su pantalla — no solo reaccionar al que tiene.

**Paletas seleccionables:**

| Modo | Descripción |
|---|---|
| **Sueño** | Niebla suave, colores desaturados, movimiento fluido como en agua |
| **Estrés** | Vibrante, contrastado, urgente — algunos jugadores trabajan mejor así |
| **Alegría** | Cálido, saturado, movimientos suaves |
| **Normal** | Sin efectos adicionales |
| *(más por definir)* | Cada modo puede desbloquearse por caminos distintos |

La capacidad de celebrar hasta lo mínimo — hasta el simple hecho de respirar — es lo que permite al jugador elegir cómo ve el mundo en lugar de que el mundo decida por él.

---

## Caminos hacia la estabilidad visual

Distintas formas de llegar a un estado visual estable o positivo, porque la felicidad llega por caminos distintos para cada persona:

- **Silencio interno** — meditación, modo contemplativo, dejar de buscar
- **Visión en el objetivo** — foco claro, propósito, avanzar con determinación (canal Goluis)
- **Lazo con algo cercano** — ver §Sistema de vínculos ampliado

---

## Sistema de vínculos ampliado — IBondable

Un vínculo no requiere ser con algo vivo o que devuelva la mirada. La constancia también sostiene.

**Vínculos actuales:**
- Compañeros (Panterilia, Goluis, Gohageneis…)
- Crías del santuario

**Vínculos a futuro:**
- Elementos del entorno natural: el cielo, el sol, una montaña, un árbol concreto
- Objetos con historia personal: un anillo, una calle, una esterilla
- La esterilla de yoga — siempre está ahí, siempre disponible

**Implicación de código:**
El sistema de `Bond` no debe ser exclusivo de `Animal` o `NPC`. Necesita una interfaz genérica:

```csharp
public interface IBondable
{
    float BondValue { get; }
    void GrowBond(float amount);
    BondType BondType { get; } // Companion, Animal, Place, Object, Nature
}
```

Cualquier objeto del mundo puede implementar `IBondable`. Un lazo fuerte con algo cercano estabiliza el estado del jugador — independientemente de si ese algo es una persona, un animal, o una montaña.

---

## Pendientes de diseño

- [ ] Definir umbrales de Satisfacción para desbloquear cada paleta artística
- [ ] Definir qué activa cada robo de cámara a nivel de código (triggers de zona, estado de animal, evento narrativo)
- [ ] Diseñar transiciones entre modos de cámara (corte vs fundido vs interpolación suave)
- [ ] Definir qué objetos/lugares del Nivel 1 pueden volverse IBondable
- [ ] Decidir si los vínculos con objetos/naturaleza tienen mecánica propia o son solo restauración pasiva por proximidad
- [ ] Definir cómo se muestra al jugador que ha formado un vínculo con algo no-vivo
