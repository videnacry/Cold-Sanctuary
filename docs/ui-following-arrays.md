# UI / FollowingArrays

Sistema de UI declarativo, el más nuevo y complejo del proyecto (último commit grande).
Ubicación: `Assets/Scripts/UI/FollowingArrays/`.

## Propósito

Generar UI dinámica que muestra "following arrays": listas de poblaciones de animales,
GameObjects o botones de UI, renderizadas como elementos por hoja. El usuario conmuta
hojas con las teclas **Z / X / C / V / B** (hasta 5 hojas).

## Jerarquía de clases

```
FollowingArrays (MonoBehaviour base, con Renderer/Canvas/Collider)
├─ ShowArrays()  → instancia UI desde followingArrayInScripts[] + followingArrayInArrays[]
│                  (y si es UI, también uiFollowingArray[])
├─ HideArrays()  → destruye la cola uiElements; opcionalmente reactiva el padre
└─ Update()      → escucha Z (mostrar) / X,C,V,B (atajos de hoja) / Esc (ocultar)

UI (extiende FollowingArrays)
├─ uiFollowingArray: UIFollowingArrayElement[]            (hasta 5 botones-hoja)
├─ uiFollowingArrayElementsFollowingArrays: FollowingArrays[]  (refs a las FollowingArrays hijas)
├─ Update() sobreescribe para mapear X/C/V/B a índices 0–4
└─ ShowArrays() además llama RenderUIElements() para instanciar las pestañas
```

## El feature de "hasta 5 hojas" (teclas Z/X/C/V/B)

En `UI.cs` `Update()` (líneas ~42–52):

- **Z** → índice 0 (hoja principal)
- **X** → índice 1 (si hay ≥ 2 hojas)
- **C** → índice 2 (si hay ≥ 3)
- **V** → índice 3 (si hay ≥ 4)
- **B** → índice 4 (si hay ≥ 5)

Cada hoja es una `FollowingArrays` que se muestra con `ShowArrays()`, desactivando la UI
padre. Se puede anidar arbitrariamente vía la referencia `parentFollowingArrays`.

## Estructuras de datos (serializables)

```
FollowingArrayInScript {
  HashSetHolder script;            // p.ej. HashSetHolderAnimalPopulation envuelve Animal.Population
  GameObject arrayItemTemplate;
  FollowingElementBehavior followingElementBehavior;
  // RenderFollowingArrays(...): por cada elemento de script.GetHashSetHolded(),
  //   instancia arrayItemTemplate, añade FollowingElement e inicializa el behavior
}

FollowingArrayInArray {            // NOTA: el archivo se llama "FollowingArrayInArray .cs" (con espacio)
  GameObject[] array;              // array crudo (p.ej. de FamilyGenerator)
  GameObject arrayItemTemplate;
  FollowingElementBehavior followingElementBehavior;
}

UIFollowingArrayElement {
  GameObject sheetButton;          // prefab de la pestaña
  FollowingArrays followingArrays; // enlace a la FollowingArrays hija de esa hoja
  // RenderUIElements(...): instancia sheetButton por cada uno y rellena elementsFollowingArrays
}
```

## Componentes de comportamiento

- **`FollowingElement.cs`** — wrapper que guarda la referencia al `FollowingElementBehavior`.
- **`FollowingElementBehavior.cs`** — base (virtual `Init(elementReference)`) para
  comportamiento por elemento de UI.
- **`AnimalRadar.cs`** — extiende `FollowingElementBehavior`. `Init()` captura el `Animal`;
  `PointAnimal()` (coroutine) orienta el transform hacia el animal a distancia y se detiene
  cuando `lifeStage == soul`.

## Acceso a datos

- **`HashSetHolder.cs`** — base abstracta, `GetHashSetHolded()` virtual (HashSet vacío).
- **`HashSetHolderAnimalPopulation.cs`** — concreta, envuelve `Animal.Population`.

## Ejemplo de jerarquía en escena

```
UI (principal, parent=null, Renderer on)
  Z habilitado → ShowArrays() renderiza uiFollowingArray[0..4] como pestañas (X/C/V/B las activan)

  FollowingArrays (hoja 1, parent=UI)
    followingArrayInScripts[0]: HashSetHolderAnimalPopulation(BunnyBehavior)
    instancias: FollowingElement(AnimalRadar) por cada conejo
    Esc → HideArrays() → reactiva UI padre y vuelve a llamar UI.ShowArrays()
```

## Puntos frágiles (ver también known-issues.md)

- `FollowingElement.followingElementBehavior` se asigna tras instanciar, pero
  `Init()` puede correr en `Start()` antes de la asignación → posible race condition.
- `RenderUIElements()` no persiste bien la referencia al padre; las hojas pueden perder
  la jerarquía al re-mostrarse.
- `DestroyUIElements()` devuelve un struct para encadenar, pero captura la cola por
  referencia → comportamiento indefinido si se modifica a mitad.
