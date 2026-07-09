# Sistema de BondActivities — Cold Sanctuary

Documento de diseño e implementación del sistema de actividades de vínculo.
Conecta con: `IBondable`, `PlayerStats` (Satisfacción), `AsanaQueue` (misma lógica de práctica).

---

## Concepto

Una **BondActivity** es una práctica que el jugador puede realizar para construir un vínculo con cualquier entidad del mundo — un compañero, un animal, un objeto, un lugar, un elemento natural. No es solo una mecánica de progresión: refleja cómo las personas realmente forman vínculos, y cómo el contexto emocional de esa práctica la moldea con el tiempo.

---

## Cómo se adquiere una BondActivity

El sistema no necesita saber el porqué — solo registra que la actividad está desbloqueada. Las circunstancias de adquisición son narrativas, no mecánicas:

- **Conocimiento** — el jugador aprende mucho sobre algo y eso genera reverencia natural
- **Experiencia emocional** — el sol dio fuerza en un momento de oscuridad y frío
- **Transmisión social** — alguien enseñó la actividad (en serio, de broma, como orden)
- **Lazo heredado** — era muy preciada para alguien con quien había un vínculo fuerte
- **Malentendido** — el jugador interpretó algo como una señal y formó el hábito

Cada actividad tiene un `acquisitionNote` (string narrativo) que puede mostrarse al jugador como memoria o contexto — no afecta la mecánica.

---

## Cómo se practica

### Práctica activa
El jugador ejecuta la actividad deliberadamente desde su lista de actividades desbloqueadas. Similar al sistema de asanas: selección consciente, ejecución, acumulación de bond.

### Práctica pasiva
Si la actividad tiene `canActivatePassively = true` y el contexto actual coincide con alguno de sus `passiveTriggers`, se practica sola — el jugador la ve ejecutarse sin haberla elegido.

Ejemplo: el jugador tiene desbloqueada "adoración al agua" y entra en una misión acuática (`passiveTrigger: "aquatic_mission"`) → la actividad se activa sola y sube bond con el agua.

---

## Cómo la experiencia modifica la práctica

### Trauma
Cada vez que la actividad se practica, el sistema registra el estado del jugador en ese momento:

- Si `playerStats.stress > stressAcceptanceThreshold` en el momento de la práctica → experiencia negativa → `traumaAccumulated` sube
- El trauma se acumula por repetición — una sola experiencia mala no bloquea, pero un patrón sí

> **Estado real (auditoría 2026-07-09):** el gate de estrés en `RegisterExperience` es un
> `stressThreshold = 0.6f` local hardcodeado, no un campo configurable
> `stressAcceptanceThreshold` en `BondActivity`.

### Bloqueo por trauma
Cuando `traumaAccumulated` supera un umbral (`blockThreshold`):
- `isBlocked = true`
- La actividad aparece visualmente bloqueada en la UI del jugador
- No puede ejecutarse ni activa ni pasivamente

### Desbloqueo por Satisfacción
Para volver a practicar una actividad bloqueada, el jugador necesita alcanzar un nivel de Satisfacción suficiente:

```
unblockThreshold = baseUnlockSatisfaction + (traumaAccumulated * traumaToSatisfactionRatio)
```

- A más trauma acumulado, más Satisfacción necesaria para animarse a intentarlo de nuevo
- Cuando `playerStats.satisfaction >= unblockThreshold` → `isBlocked = false` automáticamente
- La Satisfacción no cura el trauma — solo da el espacio para volver a intentarlo

### Recuperación del trauma
El trauma se reduce lentamente con el tiempo si la actividad no se practica en contextos negativos. También puede reducirse con eventos narrativos específicos (resolución de arco, conversación con un compañero, etc.).

---

## Estructura de datos

```csharp
BondActivity (ScriptableObject)
├── displayName: string
├── acquisitionNote: string          // narrativo, cómo se adquirió
├── targetId: string                 // id de con quién/qué construye bond (resuelto en runtime)
├── isUnlocked: bool
├── bondGainPerPractice: float       // bond que sube al target por práctica
├── canActivatePassively: bool
├── passiveTriggers: string[]        // tags de contexto que activan la práctica

// Trauma / bloqueo
├── traumaAccumulated: float         // 0–1
├── blockThreshold: float            // trauma mínimo para bloquear (default: 0.5)
├── isBlocked: bool
├── baseUnlockSatisfaction: float    // satisfacción mínima para desbloquear (sin trauma extra)
├── traumaToSatisfactionRatio: float // cuánta satisfacción extra exige cada punto de trauma

// Progresión
├── practiceCount: int
├── traumaRecoveryRatePerDay: float  // cuánto baja el trauma sin refuerzo negativo
```

---

## Relación con otras stats

| Stat | Rol en BondActivities |
|---|---|
| **Satisfacción** | Requisito de desbloqueo cuando hay trauma. A más trauma, más satisfacción necesaria. |
| **Estrés** | Si supera `stressAcceptanceThreshold` durante la práctica → acumula trauma |
| **Observación** | Algunas actividades pasivas requieren un radio de observación mínimo para activarse |
| **Bond** | Resultado de la práctica — crece con el target cada vez que se practica sin bloqueo |

> **Estado real (auditoría 2026-07-09):** no existe gating por radio de observación en el
> código — las actividades pasivas solo se activan por coincidencia de `passiveTriggers`
> (tags de contexto). Además, `BondActivityManager.Practice()`/`BuildPaletteConfig()` existen
> pero no están cableados a ninguna UI ni input todavía.

---

## Ejemplos narrativos

### Adoración al agua
- Adquirida: un compañero hacía una reverencia al mar antes de cada inmersión, el jugador lo imitó sin saber por qué
- Pasiva en: misiones acuáticas (`"aquatic_mission"`)
- Si las misiones acuáticas son muy estresantes → trauma acumulado → bloqueo → necesita satisfacción para volver

### Reverencia al sol
- Adquirida: alguien en quien confiaba lo hacía cada mañana, o fue un momento de fuerza en un ambiente oscuro
- Pasiva en: amanecer (`"sunrise"`), zonas abiertas con mucha luz (`"open_sky"`)
- Difícil de traumatizar — pocas situaciones asociadas al sol generan estrés extremo

### Presencia con la esterilla
- Adquirida: tutorial de yoga con la maestra
- Pasiva en: siempre que el jugador esté sobre una esterilla sin hacer nada (`"mat_idle"`)
- Sube bond con la esterilla como objeto — coherente con el alma contemplativa del juego

---

## Pendientes

- [ ] Definir la lista de `passiveTriggers` estándar (tags de contexto) y cómo se activan en el LevelScript
- [ ] Diseñar la UI de actividades bloqueadas — mostrar cuánta satisfacción falta
- [ ] Definir cómo los eventos narrativos pueden reducir trauma directamente (resolución de arco)
- [ ] Decidir si el trauma se muestra al jugador explícitamente o solo como "actividad bloqueada"
- [ ] Conectar con el sistema de recuerdos de IMind (pendiente futuro)
