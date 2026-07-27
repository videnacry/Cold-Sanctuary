# Arquitectura unificada: todo es un ser (Anima) + pilares + aptitudes

Diseño (2026-07-27). Visión: **todo objeto del juego puede tener/despertar consciencia**, a un nivel
configurable. Una sola clase raíz (hoy `LivingEntity`) modela desde lo inanimado hasta lo plenamente
consciente; una **piedra, el viento, el fuego** son "personajes" tanto como un animal o el jugador.
Conecta con [`mind-model.md`](mind-model.md), [`creature-stats.md`](creature-stats.md),
[`review-checklist.md`](review-checklist.md).

## 1. Idea central
- **Una sola clase para TODO ser** (animado o inanimado-despertable). Ella **decide el nivel de
  consciencia** del objeto (inanimado ↔ despierto). Una piedra puede ser **más poderosa que un jefe
  final** solo por configuración.
- **El jugador es también esa clase.** Controlar = "cambiar de mando" a otro ser (body-swap): al cambiar
  de cuerpo se muestra la UI de habilidades de ESE ser. Todos los seres tienen **registros** como si
  fueran/hubieran sido jugadores.
- **Aptitudes = los pilares de datos de todo ser** (ver §Naming). Definen **qué puede hacer, a qué
  nivel y con qué tonalidad**. Abiertas a crecer (nuevas aptitudes = nuevas áreas de pensamiento/acción).

## 2. Naming (para acabar la confusión)
- **La clase raíz** (hoy `LivingEntity`): candidatos → **`Anima`** (principio que anima; inanimado =
  Anima a consciencia 0; poético, tema de alma/consciencia), `Ser`/`Being` (claro), `Ente`/`Entity`
  (neutro). *A confirmar.* (Ojo: `Anima` suena a "alma", que ya usamos para los pools → posible
  confusión; `Ser` evita eso.)
- **Las variables base** (agility, strength, reasoning… las 12, extensibles): se llaman **APTITUDES**
  (ya es el término del código: `IAptitudes`/`Aptitudes`). **Dejar de decir "stats/habilidades" para
  esto.**
- Taxonomía cerrada:
  - **Aptitudes** — capacidades base (las 12). El pilar de datos.
  - **Margas** — tracks de progresión del alma (Stats/Yoga/Vínculos…).
  - **Puntos del alma** — pools derivados (vida/energía/maná/defensa/poder).
  - **Habilidades** — lo que se APRENDE y ejecuta (asanas, hechizos). ≠ aptitudes.

## 3. Pilares (composición, no herencia rígida)
La lógica autónoma se reparte en **pilares** = componentes conectables sobre el Anima. Un ser **habilita
solo los que tiene**:
- **`IBody`** — cuerpo (por-extremidad, asanas, físico).
- **`IMind`** — mente (pensamientos, ánimo, decisión).
- **`IBondable`** — vínculos.
- (ampliable: locomoción, rutina/autonomía, etc.)

Ejemplos de consciencia por configuración:
- **Animal** = Body + Mind (+ Bond) a tier medio.
- **Piedra parlante** = **solo Mind** (sin Body → no hace asanas, pero **se le puede hablar**).
- **Piedra que hace yoga** = si le das un Body configurado, puede.
- **Jugador** = todos los pilares, tier alto, con mando de input (los demás con mando IA).

**Ventaja clave:** el ser solo ejecuta los pilares que tiene → una piedra-solo-Mind es baratísima; nada
de simulación de cuerpo.

## 4. ¿Por qué existe `WorldCharacter`? → se disuelve en un pilar
Hoy `WorldCharacter` es la **capa de autonomía** (task-loop/rutina) + un **puente de stats** (lee
`PlayerStats` o usa sus propios stats ligeros). **No** es un separador "con trama / sin trama" — eso
sería un flag/config, no una clase. En el rediseño: su **rol** (autonomía/rutina) pasa a ser **un pilar**
sobre el Anima, y sus **stats ligeros duplicados se borran** (lee las aptitudes del Anima). Así
desaparece el "doble juego de stats" que hoy cargan los compañeros (CompanionBase + WorldCharacter).

## 5. Elementos como personalidad + instancias compartidas
- **Elementos naturales** (tierra/roca, viento, agua, fuego…) definen **tonalidad de personalidad** y
  **ánimos aparentes**. Los pensamientos elementales **pertenecen a esos seres** pero son **públicos**:
  cualquier personaje puede "soltar" a veces ideas de viento/roca/fuego.
- La **suma de aptitudes** da a un ser una **tendencia a un elemento** dominante (con destellos cortos de
  otros) → parece tener carácter y cambios de ánimo; la tendencia **se mueve** al cambiar las aptitudes
  con el tiempo. Puede haber seres **balanceados** (todos los elementos por igual) o inclinados a un
  número aleatorio de ellos.
- **Instancia compartida (flyweight) para elementos:** una sola **"roca madre"** (Anima) compartida por
  TODAS las rocas → hablarle a cualquier roca es hablarle a **la misma**. Igual viento/agua/fuego. Los
  **animales** sí tienen instancia propia cada uno. (Alternativa: darle a una roca su **propia** Anima
  para individualizarla.) → **Eficiente**: una mente para todas las rocas.
- Una roca marcada "elemento tierra" tiene **base media tierra**; sus **aptitudes** deciden la otra
  mitad → se puede llegar a una piedra **tierra+fuego** perfecta (mitad y mitad). Sirve para mostrar el
  **ruido de la mente vs el silencio**.

## 6. Las "frases" — modelo de mente por ciclo de vida
El motor de pensamiento se expresa en **frases** que **existen en TODOS los seres** (biblioteca
compartida = datos flyweight, no se guardan por-ser):
- Cada frase tiene forma **positiva y negativa** y un **ciclo de vida** en partes: **nace → crece → se
  reproduce → muere (silencio)** (metáfora del amor: su inicio, unir dos seres, extenderse a más, el
  silencio). Se divide en trazos/partes cada vez más pequeñas.
- El **poder mental** del ser (derivado de aptitudes/margas) decide **hasta qué parte** de la frase
  llega: mente débil no alcanza ni la 1ª parte; media dice 1ª–2ª; fuerte, todas; **la más fuerte alcanza
  el silencio** (puede no tener pensamientos — llegó a la muerte de todas las frases).
- Las **aptitudes/elemento** deciden **a qué frases es proclive** y **cuáles tiene más desarrolladas** →
  una roca "reencarnación de un gato" vs "persona apasionada por su familia" vs "perdida en la duda" =
  **misma biblioteca, distintos pesos**.
- Deseos base (trabajar, cuidar, acompañar, poseer, comer, dormir…) de lo simple a lo complejo, según la
  config de aptitudes.

**Por qué NO es pesado:** las frases son **datos compartidos** (una sola biblioteca para todos); cada
ser solo guarda **pesos + un tope de poder mental**. Elegir es un **pick ponderado** barato. La
"impresión de un alma concreta" sale de los **pesos**, no de contenido único por-ser. Combina con tiers
y con las instancias compartidas de elementos.

## 7. Ventajas del rediseño (por qué hacerlo)
1. **Un solo modelo mental** — jugador/compañero/animal/roca/viento = misma clase; difieren en **config +
   pilares**. Elimina las 3 fuentes de stats dispersas de hoy.
2. **Emergencia para todos** — el sim social (pensamientos/utilidad) aplica a cualquier ser.
3. **Body-swap trivial** — controlar cualquier ser = enchufar el mando de input en vez del de IA.
4. **Poder/carácter por DATOS** — una roca más fuerte que un jefe = solo configuración; los diseñadores
   tunean datos, no código.
5. **Extensible** — nuevas aptitudes = nuevas áreas de pensamiento/acción; nuevos pilares = nuevos
   dominios de capacidad.
6. **Eficiente** — cada ser corre solo sus pilares; elementos comparten instancia; frases compartidas;
   todo por tiers.

## 8. Dudas honestas / cautelas
- **No es pesado en cómputo** si se respeta: flyweight de frases/elementos, pilares opcionales, tiers,
  decisiones por evento. El coste real está en (a) el **refactor incremental disciplinado** y (b)
  **autorar el contenido** (biblioteca de frases, asociaciones de elementos).
- **No hacerlo de golpe** (y menos a ciegas sin compilar). Secuencia sugerida:
  1. Cerrar **naming** (clase + "aptitudes").
  2. **Anima = único hogar de aptitudes**: que `WorldCharacter` lea de ahí (borrar sus stats ligeros);
     `CompanionBase : Anima` (ya empezado).
  3. **Pilares como componentes** (Body/Mind/Bond) enchufables; consciencia = qué pilares activos.
  4. **Mente por frases** (biblioteca + pesos + poder mental) — el `Mind` MVP crece hacia esto.
  5. **Elementos** (tendencia por aptitudes + instancias compartidas).
  6. **Body-swap** (mando input vs IA).
  7. **PlayerStats → Anima** (el más cableado: asanas/IBody/IMind) — al final, con compilador.

> **Nombre de la clase raíz decidido 2026-07-27: `Anima`.** Encaja con niveles de alma (margas) y puntos
> del alma: subir de nivel de alma **desbloquea capacidades/habilidades** del Anima.

## 10. Extensiones (2026-07-27)

### 10.1 Bioquímica: aptitudes estructurales + química dinámica
Dos capas de "las variables del ser" (evita chocar con el sistema de **hechizos** `Chemistry`/
`PeriodicTableManager`, que es la tabla periódica externa):
- **Aptitudes (estructura, estables):** agility/strength/reasoning/memory… = el **techo** de capacidad.
- **Bioquímica (estado, dinámico):** representación de **serotonina, adrenalina, cortisol, calcio,
  glucosa/ATP, hidratación…** Es el **combustible** de energía y regeneración, y la fuente de **ánimos**
  aparentes. (Nombre propuesto: **"bioquímica"** o **"humores"**, para NO chocar con la química-hechizo.)
- **Acciones producen/consumen química:** asanas/ejercicios/misiones dan un **bonus/decremento momentáneo**
  de compuestos (p.ej. adrenalina↑ → +agilidad temporal; serotonina↑ → +ánimo; calcio/hidrógeno↓ =
  gasto) **+ un incremento/decremento mínimo y duradero**. El **decremento se repara con comida y
  descanso** (liga con `fatReserves`/hambre/`Rest`).
- **Reemplazo gradual:** donde tenga sentido, una aptitud abstracta se **deriva** de la bioquímica
  (energía↔glucosa/ATP; estrés↔cortisol/adrenalina); las estructurales (agility/memory/reasoning) se
  mantienen y la química las **modula** temporalmente. → recrear seres únicos por su **química**.

### 10.2 Piscinas de vivencias + habilidades como frases
- **Crear un personaje = darle un nº aleatorio de "vivencias"** de una piscina → define sus **aptitudes
  base**. Un **generador de recuerdos** añade/altera con el tiempo → cambios de comportamiento vía
  cambios en las stats/química.
- **Cada habilidad es una FRASE** guardada en la piscina; al cumplir la **química necesaria** se
  **desbloquea** y **sube de nivel**. → cada **asana es una frase**, con el ciclo de vida mapeado a la
  maestría de la postura:
  - **nace** = poner cada parte del cuerpo en su sitio.
  - **crece** = partes perfectamente alineadas.
  - **se reproduce** = postura sostenida **sin esfuerzo** por control/concentración perfectos.
  - **muere** = quedarse **en blanco** en la postura, experimentándolo todo en la nada.
  (Encaja con `Asana.masteryLevel`/`RegisterPractice` ya existentes.)

### 10.3 Multi-instancia y "madres": posesión / secuestro de mente
- Un **GameObject puede tener VARIAS instancias de Anima**: la **suya propia** (opcional) + instancias
  **"madre"** (compartidas: madre-roca, madre-viento, **madre-Magnate**, madre-Kushal…). Un **booleano**
  marca cada instancia como **madre** (compartida, puede conducir a otros) o **directa** (propia del ser).
- Cada instancia adjunta tiene un **valor de relevancia**; el ser lo **conduce** la de **mayor
  relevancia** en ese momento. Cada madre puede llamarlo con **intensidad inicial distinta** + **bonus
  de evento**.
- **Secuestro/posesión:** un evento (hechizo) **sube la relevancia** de una madre en muchos seres.
  Ej.: piedra con `madre-roca=2`, `madre-Magnate=0`; al lanzar el hechizo, `madre-Magnate→10/100` →
  todos parecen **poseídos por la mente de la Magnate**, sirviéndola **con su propia química/cuerpo**.
  Los **hechizos de la Magnate son supremos** → su nivel de secuestro debe superar cualquier otro.
- **Inmunidad por config:** una piedra con **solo** `madre-roca` (sin ranura de Magnate) **no puede ser
  secuestrada** por nadie más. El nivel más simple de posesión = **sustituir los pensamientos** (la mente
  activa) hasta que la madre lo deshaga; el cuerpo/química siguen siendo del ser.
- **Población reducida por madres:** en vez de N rocas únicas, unas pocas madres (roca-gato,
  roca-apasionada) + algunas rocas con Anima propia. Eficiente (pocas mentes reales).

## 9. Decisiones abiertas
- [x] Nombre de la clase raíz → **`Anima`** (2026-07-27).
- ¿Los pilares son componentes MonoBehaviour o módulos puros referenciados por el Anima?
- **Nombre de la capa química** (bioquímica / humores / compuestos) — sin chocar con `Chemistry` (hechizos).
- ¿Aptitudes estructurales se **mantienen** y la química **modula**, o reemplazo total gradual? (propuesto: híbrido).
- Formato de **piscina de vivencias** y **biblioteca de frases** (ScriptableObjects, tablas) + mapeo aptitud/química→elemento.
- Multi-instancia: ¿lista de `(Anima, esMadre, relevancia)` por GameObject? Resolución = máx relevancia.
- Regla exacta "poder mental → hasta qué parte de la frase"; y "química necesaria → desbloquea frase".
