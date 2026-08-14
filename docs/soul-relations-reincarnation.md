# Alma (avanzado): conversión por distribución · relaciones por especie · pensamientos por capacidad · reencarnación compartida

Diseño (2026-08-14). Extiende [`soul-composition-blend.md`](soul-composition-blend.md). Todo son mecánicas de
**stats + thoughts** (nada de scripting): las funciones de `Animal` (sentir peligro/huir/ayudar al pack) deben
**migrarse a evaluaciones de stats** — es el mismo comportamiento, movido a datos.

## 1. Blend / Conversión por DISTRIBUCIÓN (una sola función reutilizable)
**Problema de la fase 1** (media ponderada de valores absolutos): un arquetipo al 1% es **casi despreciable**.
**Tu propuesta** lo arregla mezclando **distribuciones** (la *forma* de dónde están los puntos), no valores crudos.

**Tu fórmula, simplificada (álgebra):**
```
contrib_X[k] = (X[k]/ΣX) · Hbase[k] / (Hbase[k]/ΣH) · dominio
             = (X[k]/ΣX) · ΣH · dominio          // Hbase[k] se cancela ↑
```
O sea: **reescala cada arquetipo al "presupuesto" de la base (ΣH), conservando su forma, y pondera por dominio.**
El `×Hbase[k] ÷Hbase[k]%` se anula → no hace falta.

**Son DOS operaciones con la misma idea:**
- **Blend (crear base desde varios arquetipos):** presupuesto de referencia = el del **arquetipo primario** (mayor
  dominio); `final[k] = Σ_X dominio_X · X[k] · (ΣRef/ΣX)`. Así 1% de conejo **empuja la forma** hacia el conejo
  (más agilidad, menos fuerza) al 1%, a escala del oso — ya no es despreciable.
- **Conversión / transformación (un ser → cuerpo nuevo):** **conserva la identidad** (su distribución actual) en el
  cuerpo nuevo. **Dos lecturas** (hay que elegir el "sabor"):
  - **(A) tu fórmula:** `nuevo[k] = (actual[k]/Σactual) · baseNueva[k]` → tu énfasis **× la capacidad del cuerpo
    nuevo** en cada stat (un oso fuerte que se vuelve hormiga = hormiga que tira a fuerte, pero topada por lo
    hormiga). Reencarna "quién eres" *limitado por el nuevo cuerpo*.
  - **(B) reescala uniforme:** `nuevo[k] = (actual[k]/Σactual) · ΣbaseNueva` → conserva tu **forma exacta**,
    solo redimensionada al presupuesto del cuerpo nuevo (una "mini-tú con forma de hormiga").
  Recomendación: **(A)** casa mejor con "las capacidades del cuerpo mandan"; **(B)** es más fiel a la identidad.
- **Una función específica** `SoulMath.Remap(...)`, **recalculable** al cambiar arquetipos (crea los stats base).
- *Decisión abierta:* (A) vs (B) para la conversión; y presupuesto de referencia = **primario** (recomendado).

## 2. Relaciones por especie (bonds / karma)
Cada alma lleva un mapa **`speciesRelations`**: por cada anima-base existente, un número (**0 = base**; **+** buena
relación, **−** mala). Se **rellena** con la vivencia y **decide** acercarse/vincular vs huir/pelear.
- **Arquetipos `speciesBond`** (`bearBond`/`wolfBond`/`humanBond`…): declaran la **base kármica** de relación con
  **todas** las especies (incluida la propia). **Mezclables** por dominio/`shareDomain` igual que cuerpo/mente →
  `humanBond+bearBond` = humano con **más agrado a osos** que `humanBond+bunnyBond`.
- **Base kármica** (evolución): Foca→Oso **negativo** (depredada por generaciones); Conejo→Lobo negativo;
  Perro↔Humano **positivo**. Es la "condición inicial".
- **Se llena con puntos** en el juego: cruzarse (guardas la **clase predominante** del otro), depredación, ayuda,
  y los **bonds individuales** (tu array de personajes) suman a la relación con su especie.
- **Especie NUEVA** (nunca vista, p.ej. oso que ve un lobo por 1ª vez): la relación inicial = **`openness`** =
  `f(media de tus relaciones positivas)` → cuantas más buenas relaciones, **más cálido** con desconocidos (y al revés).
- **Los puntos → stats → inclinación / pensamiento**, y modulan **`autoabandono`**: el **amor** sube autoabandono
  (te sacrificas), el **amor propio** lo baja. (Ojo al equilibrio; quizá reformular como dos ejes.)

**Montado (PR #76-#77):** openness (PR #77) — especie nueva se resuelve por la DISPOSICIÓN neta del ser (Archetypes.NetDisposition + SpeciesKarma.Openness); EFECTO: la buena compañía baja el stress, la mala lo sube (BondPillar). Base (PR #76): arquetipos de relación por especie (`Archetypes.RelationValue`: foca↔oso −, perro↔humano +…), `SpeciesKarma.RelationOf` (blend de `speciesBonds` por dominio, o especie directa), `SoulComposition.speciesBonds`, y `BondPillar` **siembra el bond inicial** por karma (solo la positiva; la negativa la lleva el THREAT, separado). `Anima.SpeciesName`. *Falta:* que la karma negativa amplifique el threat; circunstancias no-proximidad (ayuda/depredación); openness.

**Cálculo propuesto:**
```
relacion(yo, X) = blend(speciesBonds)[X]  +  Σ interacciones(X)  +  Σ bonds_individuales(miembros de X)·k
al conocer especie nueva Y:  relacion(yo, Y) = openness = c · media(relaciones > 0)
inclinación: relacion > θ → acercarse/vincular ;  < −θ → huir/pelear ;  intermedio → cauto
```
Al crecer un bond → generar **thoughts** (amistad/compasión/bienestar) que empujan el temperamento (respeto/deseo
de dominar/cariño/venganza) — vía el **campo mental** (`ThoughtField`) que ya existe.

## 2b. Los efectos son GLOBALES y EMERGENTES (no dirigidos al jugador) — CORRECCIÓN
El jugador **no es especial**: es **una anima más** (la que poseas). Los "efectos de compañero" no van al jugador,
son **mecanismos globales** que cada anima proyecta a **las animas cercanas con las que tiene bond**, atenuados/
vetados por **threat** (poder) y **bonds negativos** (rechazo).
- **La ACTITUD emerge de bonds + stats + humores**, no se hardcodea. Ej.: la "fiesta" de Gohageneis = alta
  `sociability` + **bond alto con Humano** + bonds **positivos (>0, menores)** con perros/crías + **adrenalina/
  positividad** alta → **juega con las animas de alto bond y bajo threat** a su alrededor (les sube el ánimo).
- **Consecuencia:** los componentes por-compañero (celebración/observación/presión) deberían **disolverse** en un
  **campo social** compartido: cada anima influye en el ánimo/humores de sus vecinos con bond según **sus propios
  stats/humores** (generaliza `ThoughtField`). Los compañeros se diferencian por su **perfil de stats/bonds**.
- **HECHO (PR #82):** `SocialField` — cada anima contagia su ánimo (serotonina/adrenalina) a vecinos con bond ≥ umbral y sin threat, escalado por su `sociability` + positividad/energía (humores). Enganchado a los compañeros → su "actitud" emerge. **HECHO (PR #83):** `HumorProfile.Apply` deriva los humores base de la personalidad (adrenalina ← sociability+creatividad+agilidad−disciplina; serotonina ← afabilidad+sociability; cortisol ← sensibilidad−composure; glucosa ← endurance+masa) → se aplica al resolver el blend (compañeros) y en `Animal.Init` (con Mente). Ahora la fiesta de Gohageneis emerge sola. *Falta:* reducir los componentes bespoke (celebración/observación) a lo puramente mecánico.

**Dinámica (PR #84) — `MoodDynamics`:** los humores no se quedan en la base: el **estrés (cortisol) es químico y SUBE con el estado** — fatiga (mucho trabajo/de pie), sueño, hambre (`Metabolism.Appetite`) y reservas bajas (glucosa/minerales) — y BAJA descansado/saciado; la `serotonina` cede ante el cortisol; `Anima.stress` lo refleja. Así los **estados de ánimo emergen del estado**: el mal humor de Goluis es **situacional** (`Goluis.UnderPressure` = override manual O `fatigue`/`stress` altos), no un rasgo. La `sensibilidad` gobierna la reactividad. *Pendiente:* el **desgaste por acciones de trabajo** (de pie/hacer fuerza gastan glucosa/minerales más rápido) que hoy solo llega vía `MoodState.fatigue`/`Metabolism`.

## 2c. Huir / ayudar-al-pack (revisión) — el modelo YA existe, pero está partido
El modelo rico —**bonds + threat + autoabandono (entrega ↔ autoconservación) + pack**— **ya está implementado**
en **`PackAwareness`** (Microcosmos): *ayudar si* `(autoabandono + vínculo) > peligroEspec`, con
`peligroEspec = compañero.PerceivedDanger − selfPower` (si soy poderoso, el peligro neto del compañero baja). El
"modo misión" sube `autoabandono` temporalmente. **Pero:**
- **Gap 1:** `PackAwareness` solo está en las hormigas. El huir/pelear de **`Animal`** (`ResolveReaction`) es
  **solo por PODER** (`masa + masa-manada × PackFactor` vs poder de la amenaza) — **no** usa bonds ni autoabandono.
  → **HECHO parcial (PR #81)**: `Animal.EvaluateThreat` baja la amenaza por el bond (un amigo no asusta); `ResolveReaction` usa `(autoabandono + vínculo-con-crías) > peligro` para plantar cara, y el `autoabandono` envalentona (myPower×(1+autoabandono)). *Falta:* mover a ayudar a un aliado LEJANO en peligro (rol de `PackAwareness`, requiere su navegación).
- **Gap 2:** `Anima.autoabandono` es un **campo crudo (0.3)**, no **deriva** de "entrega vs autoconservación".
  → **HECHO (PR #81)**: `Autoabandono.From` = entrega/(entrega+autoconservación) [entrega ← afabilidad+sensibilidad+bond medio; autoconservación ← composure+disciplina+1]; `Anima.RecomputeAutoabandono`; `Animal.Init`/`ResolveReaction` lo recomputan.

## 3. Pensamientos por capacidad (dos pools)
- **Capacidad** = `f(inteligencia: reasoning+memory+creativity+discipline)` → **entero**.
- **Pool base (biológica/kármica):** por cada `MindArchetype`, `nThoughts = floor(capacidad · dominio/100)`.
  Umbral **entero**: un arquetipo al **1%** no da ningún pensamiento hasta **capacidad ≥ 100**; **entrenar** la mente
  sube la capacidad y **desbloquea** de golpe uno de cada arquetipo al 1%. `dogMind 3%` a capacidad 100 → 3 ideas de perro.
- **Pool actual (aprendida/pre-cargada):** se llena en juego (o de antemano para crear un personaje); **cada
  pensamiento cuesta `K`** de capacidad (`K` = 1/5/10, **perilla de rendimiento**).
- **Función de recálculo** al cambiar capacidad/arquetipos: recomputa `floor(cap·dom/100)` por arquetipo, añade/quita.
- **Valoración:** el `floor` es elegante y da progresión natural. Sugerencia: la capacidad se reparte —primero la
  base kármica (barata), el resto para la actual a `K` cada una— para no doblar coste.

## 4. Reencarnación: ALMA COMPARTIDA (expansión espaciotemporal)
Varios GameObjects (p.ej. `AmbrosioMelaza` en la cueva, `AmbrosioHormiga` en la cocina) **comparten UNA alma**
(stats/bonds canónicos). Es una **"posesión libre"**: la misma `Anima` referida por varios cuerpos.
- Cada cuerpo = `SoulComposition` que **referencia el alma compartida** + sus propios arquetipos (bodyAnt +
  humanMind + ref a Ambrosio) y **convierte** los stats del alma a su cuerpo (§1).
- **Ganar/perder stats** en cualquier cuerpo activo **escribe de vuelta** al alma (en su distribución canónica) →
  **se propaga a todos** (cada cuerpo re-convierte). Lesiones **bajan** stats → **no** es subida constante → permite
  **reinicios**: si la era-3 se debilita, todas las reencarnaciones se debilitan; al visitar la era-2 después, ves a
  Ambrosio **tan débil como el último que tocaste** (era-2). "Conectados desafiando al tiempo y el espacio."
- **Bonds acumulables** en el alma: en la era-3 Medea vincula con la reencarnación de Ruth → **todas las Medeas**
  reciben a Ruth (incrementado) en sus bonds.
- **Nombres idénticos** en todas las reencarnaciones (de momento) → hace **obvio y natural** el renacimiento, ata
  coincidencias, y **facilita las historias** (cada viaje de era = una *continuación* en otro contexto).
- **Rendimiento (clave):** **perezoso** — solo el cuerpo de la **era activa** convierte en vivo; los inactivos
  guardan el valor del alma y **convierten al visitar** esa era. No es un recálculo global por frame → viable.

## Prioridad y orden
- **Lo más deseable ahora (tú):** la **conversión** (§1) + **bonds acumulables** (§4).
- Orden sugerido: (1) cerrar el "sabor" de la conversión (A/B) → (2) `SoulMath.Remap` + `SoulComposition.ConvertTo`
  → (3) alma compartida `SharedSoul` (perezosa) → (4) `speciesRelations`/`speciesBond` → (5) pensamientos por capacidad.
- Antes de construir: **migrar las funciones de `Animal` a evaluaciones de stats** (huir/ayudar-al-pack) y unificar
  con la IA de impulsos del compañero (evitar dos sistemas de amenaza).
