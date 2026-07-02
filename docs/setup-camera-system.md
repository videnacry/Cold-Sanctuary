# Setup — Sistema de Cámara

Guía de configuración de GameObjects en Unity para el sistema de cámara de Cold Sanctuary.
Scripts involucrados: `CameraManager`, `CameraZoneTrigger`, `PlayerStats`.

---

## 1. Player

### Componentes requeridos en el GameObject del jugador

| Componente | Notas |
|---|---|
| `PlayerStats` | Añadir directamente al Player. Configurar valores iniciales en Inspector. |
| `PlayerCtrl` | Ya existente. |
| `Animator` | Ya existente. |
| `NavMeshAgent` | Ya existente. |
| **Tag: `Player`** | Obligatorio — `CameraZoneTrigger` lo usa para identificar al jugador. |

### Hijos requeridos del Player

#### `ThirdPersonAnchor`
- GameObject vacío, hijo directo del Player
- Posición sugerida: `(0, 2.5, -4)` — detrás y arriba del jugador
- Rotación sugerida: `(15, 0, 0)` — ligeramente inclinado hacia abajo
- Sin componentes adicionales

#### `FirstPersonAnchor`
- GameObject vacío, hijo del **hueso de la cabeza** del Animator (Head bone)
- Posición sugerida: `(0, 0, 0.1)` — en el centro de los ojos, ligeramente adelante
- Rotación: `(0, 0, 0)` — hereda la rotación de la cabeza
- Sin componentes adicionales

> Si el modelo no tiene hueso de cabeza accesible, usar un hijo del Player con posición `(0, 1.7, 0.1)` como aproximación.

---

## 2. CameraManager

### Dónde colocarlo
Crear un GameObject vacío en la escena llamado `CameraManager`. No debe ser hijo del Player — debe estar en la raíz de la jerarquía para sobrevivir cambios de escena si es necesario.

### Componente: `CameraManager`

| Campo Inspector | Valor / Asignación |
|---|---|
| **Third Person Anchor** | Arrastrar el `ThirdPersonAnchor` hijo del Player |
| **First Person Anchor** | Arrastrar el `FirstPersonAnchor` hijo del hueso de cabeza |
| **Player Stats** | Arrastrar el componente `PlayerStats` del Player |
| **Preferred Mode** | `ThirdPerson` (por defecto) |
| **Allow Camera Robberies** | ✅ activado por defecto |
| **Transition Duration** | `0.4` segundos (ajustar según feel) |
| **Base FOV** | `60` |
| **Max FOV Constrict** | `15` — cuánto se cierra el FOV bajo estrés máximo |
| **Fatigue Shake Threshold** | `0.6` — la cámara empieza a temblar cuando fatiga > 60% |
| **Stress Aberration Start** | `0.5` — el FOV empieza a cerrarse cuando estrés > 50% |
| **Sleepiness Blackout Start** | `0.8` — los apagones empiezan cuando sueño > 80% |
| **Max Shake Amplitude** | `0.05` |

---

## 3. PlayerStats — valores iniciales sugeridos

| Campo | Valor inicial | Notas |
|---|---|---|
| Satisfaction | `0` | El jugador empieza sin satisfacción acumulada |
| Satisfaction Capacity | `1` | Escala con tiempo junto a Gohageneis |
| Satisfaction Passive Rate | `0` | Se desbloquea al superar umbral de Satisfacción |
| Restoration Multiplier | `1` | Sube en niveles altos de Satisfacción |
| Mental Fatigue | `0` | |
| Stress | `0` | |
| Sleepiness | `0` | |
| Observation Radius | `3` | Radio inicial en unidades de mundo |
| Velocity | `1` | Multiplicador base |
| Physical Resistance | `1` | Multiplicador base |

---

## 4. Zona de crías — ZoneActivator (reemplaza CameraZoneTrigger)

`ZoneActivator` es el componente recomendado para nuevas zonas. Wirea en un solo lugar:
cámara + BondActivity context tags + lo que venga.
`CameraZoneTrigger` sigue disponible pero no añadir más zonas con él.

### Configuración del trigger

1. Crear un GameObject vacío llamado `CubAreaTrigger` en la escena, posicionado sobre el área de crías
2. Añadir un componente **Collider** (BoxCollider o SphereCollider) y marcar **Is Trigger = true**
3. Ajustar el tamaño para que cubra toda el área de crías
4. Añadir el componente `ZoneActivator`

### Componente: `ZoneActivator` — zona de crías

| Sección | Campo | Valor |
|---|---|---|
| **Camera** | Action Type | `SwitchMode` |
| **Camera** | Target Mode | `FirstPerson` |
| **Camera** | Revert On Exit | ✅ activado |
| **Bond Context Tags** | contextTags | `cub_area` |

El tag `cub_area` activa automáticamente las BondActivities pasivas con ese trigger
(p.ej. "Presencia tranquila") mientras el jugador esté en la zona.

### Configuración de física requerida

En **Edit → Project Settings → Physics**, asegurarse de que el layer del Player colisiona con el layer del trigger. Si el Player y el trigger están en el layer `Default`, funciona sin cambios adicionales.

---

## 5. Cámara principal (Camera.main)

El `CameraManager` usa `Camera.main` — asegurarse de que la Main Camera tiene el tag **MainCamera** (viene por defecto en Unity).

No es necesario asignar la cámara en ningún Inspector — se obtiene automáticamente en Awake.

---

## 6. Orden de setup recomendado

1. Añadir `PlayerStats` al Player y configurar valores iniciales
2. Crear `ThirdPersonAnchor` como hijo del Player, posicionar en escena
3. Crear `FirstPersonAnchor` como hijo del hueso de cabeza, posicionar
4. Crear el GameObject `CameraManager`, añadir el componente y asignar referencias
5. Crear el `CubAreaTrigger` con collider trigger y componente `CameraZoneTrigger`
6. Hacer Play y entrar al área de crías para verificar el switch de cámara
7. Ajustar posición de los anchors según feel

---

## 7. Pendientes / próximos pasos

- [ ] Implementar `ScreenEffects.cs` para los apagones reales por sueño (actualmente solo hay un `Debug.Log`)
- [ ] Implementar aberración cromática por estrés (requiere URP/Post Processing Stack)
- [ ] Implementar vignette por cansancio (URP)
- [ ] Implementar desaturación progresiva por fatiga mental (URP o shader custom)
- [ ] Conectar `CameraManager.RequestRobbery` a los eventos de hito de las crías (`firstNestExit`, `firstSolidEaten`)
- [ ] Añadir opción en menú de pausa para que el jugador cambie `preferredMode` y `allowCameraRobberies`
