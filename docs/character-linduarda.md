# Personaje — Linduarda

> Ficha de personaje (2026-09-15). **La doctora del santuario** (rol de enfermería/Una Salud).
> Aptitudes en [`creature-stats.md`](creature-stats.md). Marco de misión en [`cria-simulation.md`](cria-simulation.md)
> (Enfermería) y [`microcosmos-eras-and-observation.md`](microcosmos-eras-and-observation.md) (es quien **da acceso al
> Microcosmos** a Kushal). Compañera humana → `CompanionBase` (código aún por crear:
> `Assets/Scripts/Companion/Companions/Linduarda.cs`).

---

## Resumen

Linduarda, **médica** ecuatoriana. Estudió medicina en **Ucrania**; a mitad de carrera estalló la
**guerra con Rusia**, logró **volver a Ecuador**, hizo prácticas de hospital allí y **regresó a
Ucrania** para terminar los estudios. Devota de sus padres, curiosa y perfeccionista, disciplinada
y aventurera. Su gran deseo: **teletransportarse por el mundo** — resonancia directa con la
**máquina de virtualización / el portal** del juego (ella, que sueña con teleportarse, es quien
opera la puerta entre planos).

En el santuario es **la doctora**: cuida personas *y* animales (Una Salud), y es quien —tras la
insistencia de Kushal— **autoriza su entrada al Microcosmos** para pasar tiempo con Sakshi o hacer
tareas para la enfermería (ver "Rol narrativo").

**Hechicera de nivel 4 (S4).** En la escalera de progresión mágica
([`magic-metabolism-progression.md`](magic-metabolism-progression.md) §3/§15), el nivel 4 es el tope del
1er trayecto: come a **nivel de quarks** y maneja **masa-energía** (E=mc²). Implica que Linduarda es de
las **más avanzadas**: sus **células de maga ya no necesitan comer**, **desintegra/crea materia** a
voluntad, **sana abasteciendo** reservas ajenas (rol *healer*, `SupplySpell`) y está en el umbral de la
**teletransportación** (T5, campo/vacío). Por eso, diegéticamente, **es ella quien opera la máquina de
virtualización/avatares** — el "teleporte" del santuario — y quien **puede conceder** el descenso al
Microcosmos. Encaja con su **sueño de teletransportarse por el mundo**.

---

## Personalidad y tono

- **Curiosa y perfeccionista.** Le importa el detalle; buena observadora (encaja con el pilar de
  **observación** del juego → puede ser mentora sutil de esa habilidad).
- **Leal y tenaz.** Cruzó de Ucrania a Ecuador para **ayudar a sus amigos**; volvió a la guerra a
  terminar lo empezado. No abandona lo que empieza.
- **Relajada y aventurera**, llena de hazañas y **emprendedora**.
- **Devota a la familia**: envía a sus padres mensajes de cariño **todos los días**; sabe de los
  **lenguajes del amor** y con ellos armonizó a su familia.
- **A la moda**, deportista, **disciplinada** (gimnasio a diario).
- **Ama la fotografía** (mirada que encuadra = observación estética) y **la arquitectura**: en
  vacaciones viaja a **España** por sus **edificios y calles**; **enamorada de Sitges**.

## Biografía (según lo narrado — confirmar ⚠️)

- Origen: **Ecuador**.
- Antes de la medicina: **camarera en USA** y **vendedora online autónoma** (emprendimiento).
- Medicina en **Ucrania** → **guerra** → retorno a Ecuador (prácticas de hospital) → **vuelta a
  Ucrania** a terminar → **voluntariado** al graduarse.
- Aficiones: fotografía, deporte/gym diario, viajes (España/Sitges), moda.
- Sueño: **teletransportarse por el mundo**.

## Mapeo a aptitudes (12 universales — `IAptitudes`)

Perfil sugerido (0–100, afinar en `creature-stats.md`):

| Aptitud | Valor | Por qué |
|---|---|---|
| `discipline` | **alta** | gym diario, terminó la carrera en guerra, perfeccionista |
| `endurance` | **alta** | deporte, tenacidad, cruces intercontinentales |
| `perception` | **alta** | fotógrafa, médica, curiosa del detalle → **mentora de observación** |
| `reasoning` | **alta** | formación médica |
| `sociability` | **alta** | lenguajes del amor, armonizó a su familia, voluntariado |
| `adaptability` | **alta** | migró/volvió varias veces, cambió de oficio (camarera→ventas→médica) |
| `creativity` | media-alta | fotografía, emprendimiento |
| `composure` | media-alta | "relajada", operó bajo guerra |
| `memory`/`agility`/`strength`/`bodyMass` | media | — |

Aptitudes extra del modelo emocional: **`afabilidad` alta**, **`sensibilidad` media-alta**.

## Rol narrativo (frame Mesocosmos ↔ Microcosmos)

Es **la bisagra** entre el mundo normal y el Microcosmos. Kushal quiere **darle consciencia a
Sakshi**; se lo plantea a Linduarda y ella **le concede entrar al Microcosmos** —a cambio de o como
parte de— **tareas para la enfermería**. Gancho de gameplay ya soportado por sistemas existentes:

- **Limpieza difícil en orificios de maquinaría**: como **insecto**, a Kushal le resulta más fácil
  esa faena (justifica la miniaturización). Reutiliza `DirtArea`/`DirtSpot`/`Cleaner` +
  `VirtualizationMachine` (entrada) + `WorldExitPortal` (salida).
- Su **sueño de teletransportarse** justifica, en lo diegético, que sea ella quien **maneja la
  máquina de virtualización** (el "teleporte" del santuario).
- Como doctora de **Una Salud** encaja con los **héroes-animales** de la Enfermería
  (ver `docs/animal-heroes.md`) y con el **área de Cría** (`docs/cria-simulation.md`).

## Estilo de diálogo (para autoría de `DialogueSequence`)

Slots sugeridos: `greetingSequence`, `briefingSequence` (encargos de enfermería / permiso al Micro),
`caringSequence`. Líneas de ejemplo (borrador ⚠️):

**Briefing / permiso**
- "¿Quieres darle consciencia a esa cría? Bien. Pero aquí el que ayuda, ayuda de verdad: tengo una
  máquina con conductos imposibles de limpiar a mi tamaño… al tuyo, no."
- "Entra, observa, vuelve por el portal. Y me cuentas *todo* — con detalle."

**Cariño / motivación**
- "Cada día le escribo a mis papás. Cuesta nada y lo cambia todo. Anda, ve a cuidar a los tuyos."
- "Yo estudié entre bombas y volví a terminar. Tú puedes con un conducto sucio."

**Saludo**
- "¡Foto! …es broma. O no. Ven, que tengo trabajo bonito para ti."

---

> **Nota de autoría:** ficha basada en biografía aportada por el autor; los detalles marcados ⚠️
> son borrador para confirmar. Formas femeninas en ficción según lo indicado por el autor
> ("la doctora", "curiosa", "enamorada").
