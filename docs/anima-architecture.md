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

## 9. Decisiones abiertas
- Nombre de la clase raíz (`Anima`/`Ser`/`Entity`).
- ¿Los pilares son componentes MonoBehaviour o módulos puros referenciados por el Anima?
- Formato de la **biblioteca de frases** (ScriptableObjects, tablas) y el mapeo aptitud→elemento.
- ¿Instancia compartida de elemento como singleton por-elemento, o un Anima "madre" referenciado?
- Regla exacta "poder mental → hasta qué parte de la frase".
