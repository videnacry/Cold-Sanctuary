# Anyma: factory por objeto-plantilla + filosofía de control

> Diseño (2026-10-10). Aclaración de arquitectura del autor: **TODO ser es una composición de `Anima`** — incluidos
> Bear/Wolf. Las subclases por especie son solo **plantillas de eficiencia** (compartir la clase para los procesos de
> cuerpo/mente/vínculos) y un sitio donde **dar valores por inputs**. Enlaza con
> [`soul-composition-blend.md`](soul-composition-blend.md), [`stats-as-truth.md`](stats-as-truth.md),
> [`typed-spells-and-queues.md`](typed-spells-and-queues.md).

---

## 1. Todo es composición (Bear/Wolf incluidos)

Un `BearBehaviour` **no** es "otra cosa" que Marilia: es un `Anima` cuya identidad (cuerpo/mente/vínculos) sale de
**datos** (catálogos `Archetypes`/`Physiognomy`/`SpeciesProfile`/…). La subclase existe por **eficiencia** (cada oso
reusa la misma clase para sus procesos) y para **recibir valores por input**. Lo ideal: que **cada oso sea ÚNICO**
(bearAna, bearJhon…), con sus propios stats/pensamientos/vínculos.

**La identidad es DATA y MUTABLE en runtime.** Ya hay hechizos que lo cambian:
- **Stats** → `TransformationSpell` (sube body/mind → un oso puede volverse **quimera de S4**).
- **Vínculos** → `GrowBond`/`Anima.bonds`.
- **Pensamientos/mente** → sembrar/redistribuir (`Mind.SeedThoughts`/`PhraseDistribution`).
Con ellos, un **oso puede pasar a hechicero, o incluso a roca** (re-blend del alma). → confirma que no hacen falta
clases rígidas por especie: basta `Anima` + data.

## 2. Dirección: de classesTemplates → objectTemplates + `AnymaFactory`

En vez de una clase C# por especie, **plantillas como OBJETO** + una factory:
- Una sola clase **`Anima`**; **`AnymaFactory`** alberga las factories por tipo (`bearFactory`, `wolfFactory`…) en un
  **array** y crea variantes **a petición**.
- Se pide con un **SPEC = objeto con propiedades OPCIONALES**. La factory lee el spec para **habilitar/deshabilitar
  caminos**:
  - Spec **vacío** → **todo random dentro de márgenes**: vínculos, stats de intelecto, stats de cuerpo, pensamientos,
    **edad, sexualidad, hambre, sueño, hogar, familia**, etc.
  - Spec con campos → esos se **fijan**; el resto, random.
- Pedidos compuestos: "**8 osos**" = una pareja con 2 hijos + 2 osos amigos + 2 solitarios → la factory arma los
  **vínculos/familias** acorde. (Ya existe `FamilyGenerator`/`Generator` que renderiza familias/individuos en
  posiciones random; la factory lo generaliza con el spec.)

### 1ª rebanada a construir: `AnymaSpec` + factory randomizadora
- `AnymaSpec` (serializable): `species?`, `bodyStats?`, `mindStats?`, `age?`, `sex?`, `hunger?`, `sleep?`, `home?`,
  `familyRole?` (pareja/cría/amigo/solitario), `bonds?` (lista de nombres→valor), `thoughts?`… **todos opcionales**
  (nullable / "sin fijar" = random en rango).
- `AnymaFactory.Create(AnymaSpec spec)` → instancia un `Anima` (hoy, un `Animal` de la especie) y **rellena lo no
  fijado** con random dentro de márgenes (reusa los catálogos como rangos base). `CreateMany(specs[])` para grupos.
- Convive con lo actual: por dentro usa `Animal`+catálogos; por fuera, el spec decide qué es fijo y qué random.
> **Estado:** DISEÑO. Base lista (catálogos = plantillas-data, `FamilyGenerator`/`Generator`, hechizos de
> modificación). Falta `AnymaSpec` + `AnymaFactory.Create(spec)` con randomización por rangos.

## 3. Filosofía de CONTROL (corrección importante del autor)

- **`AcuteStressResponse` está SIEMPRE activo** — **no** es un switch que se enciende/apaga. Es una **fuerza más**
  (el "body") que puja por el mando junto a mente/vínculos/jugador (arbitraje por relevancia, `AnimaController`).
  → montado: se añade en `Animal.Init` (fauna), en `Cast()` (compuestas) y en el **jugador** (`EnsurePlayerSystems`).
- **El jugador NUNCA tiene control total sobre nadie.** Cada `Anima` es un **individuo con pasado/presente/futuro**
  según su configuración/creación. La posesión conduce **solo mientras su relevancia supere** a la del cuerpo/instinto;
  si un apremio es fuerte, **el cuerpo retoma el mando** (cola `own` prioritaria, ver typed-spells §4).
- **La vía real al "control extremo" = los hechizos de MODIFICACIÓN** (pensamientos, vínculos, stats): el jugador
  **prepara** al anima —física, vincular y mentalmente— para que **esté alineado** con lo que quiere que haga. No se
  fuerza una marioneta; se **cultiva** una voluntad afín. (Esto hace del "control" una consecuencia del cuidado/la
  relación, coherente con el tema de consciencia del juego.)

## 4. Estado
| Pieza | Estado |
|---|---|
| Todo es `Anima` (Bear/Wolf/Marilia = composición) | **EXISTE** (clases por especie = plantilla de eficiencia) |
| Identidad mutable por hechizos (stats/vínculos/mente) | **EXISTE** (`TransformationSpell`/`GrowBond`/`SeedThoughts`) |
| `AcuteStressResponse` siempre activo y universal (fauna+compuestas+jugador) | **EXISTE** |
| Render de familias/individuos random (`FamilyGenerator`/`Generator`) | **EXISTE** |
| `AnymaSpec` + `AnymaFactory.Create(spec)` (opcionales → random en rango) | **FALTA** |
| Pedidos compuestos ("8 osos: pareja+2 crías+2 amigos+2 solitarios") | **FALTA** |
