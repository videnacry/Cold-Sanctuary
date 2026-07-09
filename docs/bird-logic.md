# Lógica de pájaros (`BirdBehavior`) — estado actual y por qué probablemente hay que reemplazarla

Este documento existe para que quien decida limpiar/reemplazar el sistema de pájaros
sepa exactamente qué toca, qué depende de qué, y qué está mal antes de empezar.

## Por qué esto importa: la dependencia oculta en `LifeStage`

`LifeStage.Wander()` ([`Assets/Scripts/LifeStage/LifeStage.cs:136-145`](../Assets/Scripts/LifeStage/LifeStage.cs))
es el evento que hace que los animales terrestres (ciervos, zorros, lobos, etc.) caminen
sin rumbo fijo durante su día. Su implementación actual no elige un punto aleatorio del
mapa — elige un pájaro al azar de `BirdBehavior.population` y camina hacia él:

```csharp
List<GameObject> birds = new List<GameObject>(BirdBehavior.population);
if (birds.Count == 0) return;
script.target = birds[Random.Range(0, birds.Count)];
script.nav.SetDestination(new Vector3(script.target.transform.position.x, script.transform.position.y, script.target.transform.position.z));
```

Hay un segundo punto con la misma dependencia: `Animal.Flee()`
([`Assets/Animals/Animal.cs:283-298`](../Assets/Animals/Animal.cs)) — cuando un animal
huye de una amenaza, cada ~10s corre hacia un pájaro al azar en vez de simplemente
alejarse de la amenaza. Mismo guard clause y mismo fix de off-by-one aplicado ahí.

O sea: **los pájaros no son decorativos, son el sistema de "punto de interés" para el
wander de todos los herbívoros/carnívoros terrestres.** Si `BirdBehavior.population`
está vacía (que era el estado real del proyecto hasta ahora — no había ni un pájaro en
la escena), el evento Wander no hace nada. Antes de la corrección de este mismo archivo,
además, tiraba `ArgumentOutOfRangeException` en cada tick porque indexaba una lista vacía
sin chequeo previo (ya arreglado — guard clause + fix del off-by-one en `Random.Range`).

Esto es un acoplamiento raro: un sistema pensado para pájaros de fondo terminó siendo
la única fuente de "hacia dónde camino cuando no tengo nada mejor que hacer" para
cualquier especie terrestre.

## Piezas del sistema actual

### `BirdBehavior` ([`Assets/Scripts/BirdBehavior.cs`](../Assets/Scripts/BirdBehavior.cs))

- `population` (`static HashSet<GameObject>`) — el registro global de pájaros vivos.
  Nada lo limpia cuando un pájaro se destruye (ver "Problemas" abajo).
- `min`, `max` — rango base de altura de vuelo.
- `speed`, `tripTime` — velocidad máxima y duración máxima de un tramo de vuelo.
- `Update()` — si el pájaro no está volando (`moving == false`), sortea:
  - `lapse` — cuántos ticks de 0.1s va a durar el tramo (`Random.Range(30, tripTime)`).
  - `velocity` — velocidad real del tramo (`Random.Range(speed, speed/3) / 60`).
  - `giro` — un entero 0–3 que decide tanto el giro (rotación por tick) como qué
    tan alto puede volar (ver `Altura`).
  - Llama a `Fly()`, que arranca la corrutina `Move` (por **nombre de string**,
    `StartCoroutine("Move")` — frágil ante renombres, no lo detecta el compilador).
- `Altura(int rotation)` — cuanto más alto el valor de `giro`, más alto el rango
  de vuelo permitido (`min` a `min + max * rotation`). Nombre engañoso: no es una
  altura fija, es un rango que después se sortea de nuevo.
- `Move()` (corrutina) — cada 0.1s: rota, avanza hacia adelante, y interpola la Y
  hacia `altura`. Cuando `lapse` llega a 0, `moving = false` y el próximo `Update()`
  sortea un nuevo tramo. No hay evasión de obstáculos ni lógica de bandada (flocking).
- `GenerateSquareRange(animal, area, quantity)` — implementa `IFactory`. Ignora el
  parámetro `animal` (nunca se usa dentro del método). Instancia `quantity` copias de
  **su propio GameObject** (`Instantiate(gameObject, ...)`) en posiciones aleatorias
  dentro del `Bounds` del collider de `area`, a la altura máxima de esos bounds. Cada
  copia se registra en `Respawn.birds` y `BirdBehavior.population`.

### `IFactory` ([`Assets/Scripts/IFactory.cs`](../Assets/Scripts/IFactory.cs))

Interfaz mínima con un solo método, `GenerateSquareRange`. Solo la implementan
`BirdBehavior` y (de forma estática, no vía interfaz) `Animal`.

### `Generator` ([`Assets/Scripts/Generator.cs`](../Assets/Scripts/Generator.cs))

MonoBehaviour standalone: en `Start()`, para cada `Subject` (GameObject + cantidad)
en `subjects`, llama a `Fill()`, que busca `IFactory` en el prefab y si existe llama
`GenerateSquareRange`. **No hay ninguna instancia de `Generator` en la escena
actualmente** — es infraestructura sin usar.

> **Estado real (auditoría 2026-07-09):** `Generator.cs` es legacy/sin uso, confirmado.

### `Respawn` ([`Assets/Scripts/Respawn.cs`](../Assets/Scripts/Respawn.cs))

Otro spawner independiente, con su propia lógica de reaparición periódica de
`beasts` (no de pájaros). Mantiene `Respawn.birds` (`static List<GameObject>`) solo
porque `BirdBehavior.GenerateSquareRange` escribe ahí — `Respawn.cs` en sí no lo lee
ni lo usa. Tiene además un campo `aves` (pájaros) que no se referencia en ningún lado
del archivo: campo muerto.

> **Estado real (auditoría 2026-07-09):** `Respawn.bears`/`rabbits`/`aves` fueron
> eliminados el 2026-07-09; de esos campos hoy solo queda `birds`.

## Lo que se agregó en esta sesión

`SampleSceneBuilder.BuildBirds()` ([`Assets/Editor/SampleSceneBuilder.cs`](../Assets/Editor/SampleSceneBuilder.cs))
crea 6 pájaros **placeholder** (esferas achatadas sin collider, sin modelo 3D real —
no existe ningún asset de pájaro en el proyecto todavía) con `BirdBehavior` configurado
con valores razonables, y los registra a mano en `BirdBehavior.population` /
`Respawn.birds`. Esto no pasa por `GenerateSquareRange`/`Generator` — se hizo directo,
al estilo del resto de `SampleSceneBuilder`, para no depender de un `area` con Collider
extra. Su único propósito es que `LifeStage.Wander()` tenga objetivos reales para
probar el juego; no son arte final.

## Problemas conocidos (por qué esto es candidato a reemplazo)

1. **Sin limpieza de memoria**: ni `population` ni `Respawn.birds` remueven un pájaro
   cuando se destruye. Con el tiempo se acumulan referencias nulas, y cualquier código
   que itere esas colecciones (como `LifeStage.Wander()`) puede toparse con un
   `GameObject` destruido.
2. **Acoplamiento incorrecto**: el wander de *todas* las especies terrestres depende
   de la existencia de pájaros específicamente. Debería ser un sistema genérico de
   "puntos de interés para deambular" del que los pájaros sean un proveedor más (o
   ninguno), no la única fuente.
3. **Corrutina por nombre de string** (`StartCoroutine("Move")`) — no la valida el
   compilador; un rename silencioso de `Move` la rompe en tiempo de ejecución sin error
   de compilación.
4. **Nombres mixtos español/inglés** (`giro`, `altura`, `lapse`, `velocity`) y
   `Altura()` con un nombre que no refleja lo que hace (devuelve un rango, no una
   altura final).
5. **Dos sistemas de spawn paralelos y parcialmente redundantes** (`Generator` y
   `Respawn`), cada uno con sus propias colecciones estáticas (`Generator.bears`/
   `rabbits` vs `Respawn.bears`/`rabbits`, con los mismos nombres pero sin relación
   entre sí), y ninguno de los dos actualmente instanciado en la escena.
6. **Sin flocking/evasión real** — el "vuelo" es rotar + interpolar hacia una Y
   objetivo; no hay cohesión de bandada ni evasión de obstáculos/edificios.

## Recomendación para el reemplazo

Separar la responsabilidad de "punto para deambular" de la de "es un pájaro":
un `WanderTargetRegistry` genérico (o similar) del que `LifeStage.Wander()` lea,
alimentado por cualquier proveedor (pájaros, puntos fijos de diseño, etc.). Así
`BirdBehavior` se puede reescribir o eliminar sin tocar `LifeStage.cs`.
