# Suprahechizos, cuenta de pensamientos, enseñanza y la Magnate

> Diseño (2026-10-12). El modelo unificador: **toda acción es un hechizo con animación**; los hechizos se **componen**
> (un suprahechizo desencadena una cola de sub-hechizos); la **maestría sube la cuenta del pensamiento** y la cuenta
> decide qué se hace. Enlaza con [`typed-spells-and-queues.md`](typed-spells-and-queues.md) (colas),
> [`spell-genesis-and-sanctuary-progression.md`](spell-genesis-and-sanctuary-progression.md) (acción→hechizo),
> [`anyma-factory-and-control.md`](anyma-factory-and-control.md) (control=alinear).

---

## 1. Hechizo = animación (y por eso TODO es el mismo hechizo)
Cada hechizo **tiene una animación**; cuando la animación termina, el hechizo termina (da igual su tamaño/duración).
Por eso **quien cura con medicina, tratamiento, rehabilitación, entrenamiento o magia usa EL MISMO hechizo** — solo
cambia la animación/receta. El hechizo de la Magnate enseñando una postura = su animación de enseñar.

## 2. Maestría ⇄ cuenta del pensamiento (números infinitos)
- Los hechizos **se aprenden por maestría**, y la maestría **potencia el pensamiento**: cuanto más fuerte el
  pensamiento, más recurrente su acción; la acción, a su vez, sube el número del pensamiento → bucle.
- **Cada pensamiento tiene un NÚMERO** (cuenta); realizarlo lo **incrementa**. Los números son **infinitos** (no hay
  tope); "subir el %" solo significa que su cuenta creció tanto que las demás **se ven** más bajas (luego se puede
  mostrar como porcentaje = cuenta / Σcuentas).
- **Tambaleos y cambios de dirección** son inevitables: el **entorno, el trauma, los sucesos de la vida** pueden
  **contradecir** un pensamiento y **bajar** su cuenta, mientras otros suben → la personalidad deriva sola.
- **Encaje con el código:** hoy `MindPhrase.weight` es fijo y `Mind` elige por peso; falta una **cuenta mutable por
  pensamiento y por anima** (no existe aún). → *A construir:* `thoughtCounts[phrase]` en `Mind`, que la realización
  (lanzar el hechizo asociado) incrementa y que el entorno/trauma modula; la selección usa la cuenta.

## 3. Suprahechizo (conjuro) = cola de sub-hechizos  — *tu idea, y es la correcta*
Un **suprahechizo** es un hechizo que **desencadena una serie de hechizos** (una **cola invisible** con qué lanzar y
qué ya se lanzó). La `Volition` ya elige el impulso más fuerte (hambre) y lanza lo que sabe (buscar presa); el
suprahechizo formaliza esa cadena:
- `comer` (supra) → `analizar/buscar` → … → hasta poder comer. **No hace falta un "analizar" global antes**: **cada
  hechizo hace SU propio análisis** — en el código eso **ya es `SpellBase.CanCast`** (su análisis/requisito). Un
  requisito puede ser **postura** → al lanzarlo, dispara su propio **suprahechizo de postura** (erguido/agachado), que
  tiene su cola (estirar piernas/espalda/mirar al frente); si ya está de pie, esos sub-hechizos pasan a **check** y se
  salta al siguiente.
- **Potencia y equilibrio:** cada supra tiene una **potencia impuesta**; si recibe un hechizo de **más potencia**,
  **pierde el equilibrio** → **re-ejecuta** el supra (si fue derribado físicamente, vuelve a "levantarse"; si el
  aturdimiento no fue físico, sigue con los demás, p. ej. olfatear).
- **Coste y calentamiento (ya en código):** los hechizos **pagan por lanzamiento aunque fallen**
  (`energyCost`/`cost`); ganan fuerza por **canalización previa** y por **lanzar con más fuerza cada vez** (charging,
  `SpellBase`/`WalkSpell.Drive(charging)`). Sin energía, el hechizo no tiene potencia → el tigre hambriento **no se
  levanta** pero **sigue intentándolo** (gasta y falla) salvo que sepa el supra **pedir ayuda** (gritar o **esperar
  calmado**, que gasta menos → vive más).

**Mi valoración (¿mejor idea?):** tu modelo es sólido y **ya casa con las primitivas existentes** — no hace falta
reinventar: **suprahechizo = `QueuedSpell` que encola sub-`QueuedSpell`s** (la `ActionQueue` ya lo permite: un
`Tick` puede encolar más), **análisis = `CanCast`**, **potencia = `force`**, **calentamiento = charging/channel**,
**equilibrio = un hit de `force` mayor vacía/reinicia la cola**. Refinamiento que propongo: que el **"check"** (sub-
hechizo ya satisfecho) sea simplemente `CanCast==true && yaLogrado` → se consume sin animación. Así no hay dos
sistemas (análisis vs ejecución): **hay hechizos que se auto-analizan y se encolan**. → *A construir:* un
`SuperSpell : QueuedSpell` que, en `Tick`, resuelve su sub-cola (saltando los que ya cumplen) y se reinicia si pierde
el equilibrio.

## 4. Enseñanza de yoga (clase: todos maestros y estudiantes)
- Todos los hechizos **se aplican a uno mismo, a un target o al mundo**. Enseñar yoga = lanzar el hechizo de postura
  **sobre uno mismo Y sobre el target** (p. ej. `manosArriba(self+target)`), y seguir la secuencia
  (`espaldaRecta`→`piernasEstiradas`→`tobillosJuntos`…) hasta la postura perfecta; mientras la mantienen, hay
  **correcciones**. **Cualquiera** puede corregir → autoentrenamiento grupal: cada integrante hace crecer al grupo.
- **Mantener cualquier postura un rato YA es yoga**; se va más allá completando **asanas** o **kriyas**.
- Como es un hechizo, tiene **intensidad** → cuánto sube la cuenta del pensamiento (de 1% a 5–10%…), lo que refuerza
  **volver a practicarlo**. Ejemplo (tu math): si `comer` es el más fuerte (cuenta 2304), la Magnate lanza
  `manosArriba` con potencia que lo lleve a **≥2304** y, si ya iguala, **+30% → ~2995** → sube su correspondencia
  energética en la mente y pasa a competir con los más fuertes.

## 5. La Magnate — maestra-guardiana (CONSTRUIDA, 1ª rebanada)
`Magnate` (`Assets/Scripts/World/Magnate.cs`): **estado infinito** (stats/aptitudes al tope = "sin límite"), **sin
necesidades** (no come/duerme/estresa), **imposeíble de poseer** (relevancia altísima). Trabajo de **guardiana**:
- **Proteger la vida:** a su alrededor **pacifica** (las animas dejan de estar `aware` → el lobo suelta al conejo).
- **Hacer crecer al grupo:** cada `teachInterval` busca al **rezagado** (menor `SoulLevels`), **va a él** y le
  **enseña yoga**: le da el **recuerdo** (`Grimoire.Learn("yoga")`), le **encola el Saludo al Sol**, le sube **XP de
  Yoga** y el **ánimo** de practicar. (Antes revisa que no le falte comer/descansar.)
- Aparece en el santuario como una **compañera más** (en todos los sanctuary, clonada). Sandbox: `BuildMagnate`.

*Falta (rebanadas):* la **marca por anima** (último avance + cuándo) para priorizar mejor; **bond alto por defecto**
desde todas hacia ella (requiere que su `Anima` sea `ITarget`); **llevar a la enfermería** al que está por morir;
**guardia nocturna** alimentando animales (de noche, cuando los demás duermen — encaja con `AreaHours`); y que
enseñar use la **intensidad** para subir la **cuenta del pensamiento** (§2, cuando exista).

## 6. Estado
| Pieza | Estado |
|---|---|
| Hechizo = animación; mismo hechizo para curar/enseñar/entrenar | **MODELO** (SpellBase ya lo soporta) |
| Suprahechizo = cola de sub-hechizos (análisis=CanCast, potencia=force, equilibrio) | **DISEÑO** (`SuperSpell` por construir; primitivas listas) |
| Cuenta mutable por pensamiento (maestría↔recurrencia; trauma baja) | **FALTA** (`Mind.thoughtCounts`) |
| Enseñanza self+target, clase = maestros/estudiantes, intensidad | **PARCIAL** (hechizos self/target ya; Magnate enseña; falta la cuenta) |
| Magnate (infinito, pacifica, enseña al rezagado) | **EXISTE (2026-10-12, 1ª rebanada)** |
| Cocina: acción con RANGO (`use`/`take` falla si no alcanzas) | **EXISTE** (first slice de "steps=hechizos con alcance") |
| Insectos neutros de ambiente (factory en Microcosmos Scene0) | **EXISTE** (se saltan si falta el .fbx) |
