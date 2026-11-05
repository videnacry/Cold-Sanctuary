# Reglas para crear animales del santuario

Berón (2026-11-05): «agrega morsas al santuario (¿tenemos reglas para la creación de animales? debe crearse siguiendo el ejemplo de los
demás para que tenga su realismo en apariencia, medidas y ciclo de vida, investiga)».

**Respuesta: las había repartidas, no escritas en un solo sitio.** Existían la skill `species-sheet` (qué tablas rellenar) y trozos en
[ice-sanctuary-ecology.md](ice-sanctuary-ecology.md) (patrón de especie), [ciclo-de-vida.md](ciclo-de-vida.md) (calendario y procesos),
[stats-as-truth.md](stats-as-truth.md) (masa y stats) y [natural-interactions.md](natural-interactions.md) (etograma). Este doc las
junta. El resumen está en [manual-de-diseno.md §13](manual-de-diseno.md#13-animales-y-territorio-polar).

## Las reglas

| # | Regla | Por qué | Se comprueba en |
|---|---|---|---|
| E1 | **Primero investigar, con fuente.** Cada número lleva cita (autor, año, revista o DOI). Si es un valor general sin fuente directa, se marca `[A]`. Si no se pudo confirmar, se dice. | Lo inventado no se distingue luego de lo real. | Solo en docs |
| E2 | **La especie real manda.** Si el catálogo mezcla dos especies, se separan. Las ficticias (abeja y araña de las nieves) derivan sus datos de parientes reales, citados. | `Penguin` mezclaba al emperador con los guijarros de Adelia. | Grupo 99 |
| E3 | **Mismas tablas, mismo nombre.** Una especie nueva entra en TODAS las tablas de abajo con la misma clave. Faltar en una no da error: cae en un valor por defecto en silencio (2 meses de gestación, 40 kg). | Fallo silencioso. | Grupo 99 (catálogos) |
| E4 | **Clase mínima.** `Assets/Animals/<Especie>/<Especie>Behavior.cs` solo lleva `SpeciesArchetype`, `Start` y `ConfigureThreat`. El comportamiento es data. | Composición, no herencia. | Revisión |
| E5 | **Apariencia: cuerpo de cubos a tamaño real.** Plan de `CubeCreature` (cuadrúpedo, pinnípedo, cetáceo, ave, insecto) con medidas y colores reales en `SpecFor`, y los rasgos que identifican a la especie: colmillos, cuernos, parches, cornamenta en ambos sexos. Si el plan no encaja, se añade un rasgo o un plan, no se deforma. | Se reconoce a la especie sin cartel. | Fotos |
| E6 | **Medidas: tamaño adulto real respecto al humano** (1,75 m). `SpeciesScaleFix.Real` es el largo de cabeza a cola, o la altura en los erguidos. La cría mide 40 % y el joven 70 %. La masa va en `Physiognomy`. | El oso medía 8 m. | Grupo 99; menú «Auditoría de escala» |
| E7 | **Nombres sin sorpresas.** `PlanFor` busca subcadenas: el nombre no puede contener «ant», «bee» ni «seal» por accidente, y la especie específica va ANTES que la general en `SpecFor` («reindeer» antes que «deer»). | «panterilia» caía en insecto. | Grupo 99 (plan) |
| E8 | **Ciclo de vida completo:** agua, celo, cortejo, gestación o huevo, parto, crianza, cría que sigue a su madre, joven, adulto, vejez. Las tablas van en unidades reales (meses, días); el calendario las escala. El parto es siempre 1 cría, aunque la especie tenga camadas: se cuenta en la ficha y se simplifica aquí. | «Que realice todos sus procesos naturales». | Grupo 58, su escena |
| E9 | **Dieta real.** Lo que no sea presa, pez o krill va en el `Ethogram` como `Feed` sobre un `Affordance` (liquen, sauce, juncia, bayas). Si no existe, se crea. | La morsa no come peces; el reno no come «pasto». | Grupo 99; `[ECO]` |
| E10 | **Hábitat real.** La especie vive donde vive de verdad: mar y hielo (`MarineLife`, descansaderos), hielo fijo, isla de tierra. Se documenta el sitio y por qué. El mundo se adapta: si necesita una colonia, cueva o tundra, se construye. | Oso sobre pasto: inconsistente. | Grupo 99, fotos |
| E11 | **Población por humano** (`PopulationPlan.PerHuman`): más presas que depredadores, grupos pequeños y un Animal completo caro. La reproducción respeta el techo de 1,5×. | Rendimiento y equilibrio. | Grupo 99; `[ECO]` |
| E12 | **Escena de ciclo de vida.** Cada especie tiene `CicloVida_<Escena>`: se añade a `CicloVidaPanel.Species` (con su hábitat) y a `StartScreen.Special`; comida inagotable y pareja adulta. | Comprobar cada proceso. | Grupo 58 en esa escena |
| E13 | **Polo mezclado, y dicho.** El santuario es artificial y junta los dos polos (morsas árticas y pingüinos antárticos). Se admite, pero cada ficha dice de dónde es la especie. | Para que no pase por dato. | Solo en docs |
| E14 | **Mente propia.** `Archetypes` (cuerpo, mente, relaciones entre especies) y `PhrasePools` (pensamientos) van con ella. | Todo ser tiene mente. | Grupo 99 (catálogos) |

### Tablas de una especie

| Tabla | Archivo | Qué va |
|---|---|---|
| `SpeciesProfile` | `Assets/Animals/SpeciesProfile.cs` | gregarismo, mordisco, grasa, umbral de amenaza |
| `Physiognomy` | `Assets/Animals/Physiognomy.cs` | masa y tamaño de comida; escala 1 con cubos |
| `StageProfile` | `Assets/Animals/StageProfile.cs` | días de cría, joven y adulto |
| `Reproduction.Calendar` | `Assets/Animals/Reproduction.cs` | gestación (o cópula → puesta) e intervalo |
| `LifeHistory` | `Assets/Animals/LifeHistory.cs` | agua, crianza, guarida, huevo, fuentes |
| `Family` | `Assets/Scripts/Family.cs` | tamaño del grupo y cuidado parental |
| `Forager.ConfigureForSpecies` | `Assets/Animals/Forager.cs` | qué come |
| `Ethogram` | `Assets/Animals/Ethogram.cs` | hábitos y plantas |
| `DietPreference` | `Assets/Animals/DietPreference.cs` | apetencia por cultivos, si es herbívoro |
| `SpeciesBody` | `Assets/Animals/SpeciesBody.cs` | territorio y bases de agilidad y percepción |
| `Archetypes` | `Assets/Scripts/Soul/Archetypes.cs` | cuerpo, mente, relaciones, medio |
| `PhrasePools` | `Assets/Scripts/Mind/PhrasePools.cs` | pensamientos |
| `ActionsPrep` | `Assets/Animals/ActionsPrep.cs` | velocidades |
| `PopulationPlan` | `Assets/Scripts/World/PopulationPlan.cs` | individuos por humano |
| `CubeCreature.SpecFor`, `CubeBody.PlanFor` | `Assets/Scripts/Avatar/` | cuerpo |
| `SpeciesScaleFix.Real`, `ScaleAudit` | `Assets/Scripts/World/`, `Assets/Editor/` | tamaño real |
| `AnimalPrefabGenerator.Species` | `Assets/Editor/AnimalPrefabGenerator.cs` | prefab |
| `CicloVidaPanel.Species`, `StartScreen.Special` | `Assets/Scripts/LifeCycle/`, `Assets/Scripts/UI/` | su escena |
| `BloodBank` | `Assets/Scripts/World/BloodBank.cs` | kg del paciente |
| `SleepCycle.IsNocturnal` | `Assets/Animals/SleepCycle.cs` | solo si es nocturna |

## Fichas (2026-11-05)

Los números se comprobaron contra fuentes el 2026-11-05; WP = Wikipedia, usada como fuente secundaria. Lo modelado está entre paréntesis.
«Cría» y «joven» son las duraciones de `StageProfile`.

### Morsa, *Odobenus rosmarus* (`Walrus`, Ártico; Vulnerable, IUCN)
- **Medidas:** macho 800–1700 kg, hembra ~560–800 kg; 2,2–3,5 m (WP, citando a Fay). **Modelo:** 3 m, 1000 kg.
- **Reproducción:** gestación ~15 meses con 4–5 de implantación diferida (ADW); media en cautividad 423 d, una cría cada ~2,6 años
  (doi 10.1186/s40850-022-00158-1). Lactancia 12–36 meses (ADW). Hembras maduras a los 6–8 años. **Modelo:** (15; 31) meses, cría 730 d, joven 1800 d.
- **Vida:** hasta ~40 años; 20–30 en libertad. **Modelo:** ~35 años.
- **Dieta:** ~97 % invertebrados del fondo, unos 25 kg/día. **Modelo:** `SwarmKind.Clam`, bancos de bivalvos en los bajíos de grava de la laguna.
- **Conducta:** manadas del mismo sexo, ~77 % del tiempo en el agua (Jay, Farley & Garner 2001, doi 10.1111/j.1748-7692.2001.tb01008.x). Depredadores: orca y oso polar.
- **Forma:** colmillos de 50 cm de media (máx. 90–100), bigotera, sin cola, gris claro a canela, sin orejas visibles.

### Pingüino emperador, *Aptenodytes forsteri* (`EmperorPenguin`, Antártida)
- **Medidas:** 22–45 kg. Ojo: 1,1–1,2 m es el largo de pico a cola; de pie mide **0,81–1,08 m** (WP). **Modelo:** 1,0 m, 30 kg.
- **Reproducción:** un huevo sobre las patas; incubación 62–67 d, solo el macho (NZ Birds Online), que ayuna 105–120 d. Emplume ~141 d. Primera cría típica a los 5 años.
- **Conducta:** cría en hielo fijo, hasta a 18 km de mar abierto, al abrigo de acantilados e icebergs; huddling de 10–21 aves/m² (Gilbert et al.; Zitterbart et al. 2011). Bucea hasta 564 m (Wienecke et al. 2007, doi 10.1007/s00300-006-0168-8). ~238 000 parejas en 46 colonias (Fretwell et al. 2012, doi 10.1371/journal.pone.0033751).
- **Forma:** cabeza negra, parches auriculares amarillos, pecho amarillo pálido, vientre blanco, dorso gris plateado oscuro.

### Pingüino de Adelia, *Pygoscelis adeliae* (`Penguin`, Antártida)
- **Por qué cambia:** `Penguin` llevaba el ciclo del emperador y los hábitos de Adelia (guijarros). Al añadir al emperador, se separaron (regla E2).
- **Medidas:** 3,6–5 kg; ~70 cm de largo, ~61 cm de pie (ADW). **Modelo:** 0,6 m, 4,5 kg.
- **Reproducción:** nido de guijarros, 2 huevos (aquí 1), incubación 32–37 d; el macho hace el primer turno largo en el 85 % de los nidos (Davis 1988, *Auk* 105:428); emplume ~53 d; primera cría a los 3–6 años. **Modelo:** el primer turno cubre toda la incubación (sin relevos).
- **Forma:** negro con anillo blanco en el ojo.

### Reno, *Rangifer tarandus* (`Reindeer`, Ártico)
- **Medidas:** macho 159–182 kg, hembra 80–120 kg; 1,62–2,14 m; 0,85–1,5 m a la cruz (WP). **Modelo:** 2 m con cola, 140 kg.
- **Reproducción:** gestación 228–234 d, una cría al año; pasta desde ~45 d y mama hasta el otoño. Primera cría a los 16–28 meses. Vive 10–15 años (ADW).
- **Dieta:** liquen en invierno; sauce, abedul, juncia y algodón ártico en verano. Depredador principal: el lobo.
- **Forma:** cornamenta en los dos sexos, pardo con vientre y cuello claros.

### Lobo ártico, *Canis lupus arctos* (`Wolf`, ya en catálogo)
- **La ficha más débil:** no hay masa medida en revistas; páginas de zoológicos dan 34–50 kg, 0,63–0,8 m a la cruz, 1–1,5 m de largo. Gestación 63 d de *C. lupus* `[A]`.
- **Conducta:** en el NE de Groenlandia ~2 cachorros (Marquard-Petersen 2008, doi 10.22621/cfn.v122i2.573). Guarida en roca o cueva porque el suelo está helado; una en Ellesmere se usó probablemente más de 700 años (Mech & Packard 1990). Caza buey y liebre (Mech 2007, *Arctic*).

### Liebre ártica, *Lepus arcticus* (`ArcticHare`, Ártico)
- **Medidas:** 3–5 kg; 48–66 cm. **Modelo:** 0,62 m, 4 kg.
- **Reproducción:** gestación 50–53 d, 2–8 lebratos (aquí 1), destete 8–9 semanas, cría a los ~315 d, vive 3–5 años (ADW).
- **Dieta:** ~95 % sauce (ADW; Klein & Bay 1994, doi 10.1007/BF00325880, existe, pero no se confirmó la cifra en él).
- **Berón pidió conejo:** el lagomorfo del Ártico es la liebre. `Bunny` sigue siendo el conejo de los microcosmos y las pruebas del huerto.

### Zorro ártico, *Vulpes lagopus* (`ArcticFox`, Ártico)
- **Medidas:** 2,9–3,5 kg de media; cuerpo 41–68 cm; cola ~1/3 del cuerpo (las fuentes discrepan). **Modelo:** 0,85 m con cola, 3,3 kg.
- **Reproducción:** gestación ~52 d (46–58), camada 5–8 (aquí 1), los dos padres; madriguera en montículo de 1–4 m con 4–8 bocas, usada décadas o siglos (ADW; Audet et al. 2002, *Mammalian Species* 713). Destete no aclarado (28–60 d según fuente). **Modelo:** cría 70 d.
- **Sin confirmar:** que siga al oso para aprovechar sus presas. Es un dato muy repetido; solo aparece en sus pensamientos.

### Buey almizclero, *Ovibos moschatus* (`MuskOx`, Ártico)
- **Medidas:** macho 300–400 kg, hembra 180–275 kg; 1,35–2,5 m; ~1,2 m a la cruz (ADW). **Modelo:** 2,3 m, 300 kg.
- **Reproducción:** gestación ~8 meses (235–245 d, GBIF), una cría cada 1–2 años, destete 10–14 meses. Hembra hasta 15–18 años.
- **Dieta:** juncia y gramíneas en verano; sauce, abedul, musgo y liquen en invierno. Depredadores: lobo y osos.
- **Forma:** lana oscura hasta media pata, silla clara, patas claras, cuernos color crema con punta negra.
- **Sin modelar:** el corro defensivo como conducta. Hoy es `packFactor` alto: el grupo se defiende mejor, pero no forma el círculo.

## Especies recomendadas para más adelante

Tope de hoy: 6 especies nuevas más el lobo. Cada Animal completo cuesta, y las de abajo piden vuelo o sistemas nuevos.

| Especie | Por qué | Coste |
|---|---|---|
| Lemming | La base real de la tundra: de él dependen zorro y búho. Como `Swarm` de suelo saldría barato. | Bajo |
| Lagópodo alpino | Come yemas de sauce y cambia de plumaje. | Bajo (ave de suelo) |
| Págalo | Roba huevos y polluelos en las colonias. | Medio (vuelo) |
| Búho nival | Caza lemmings y liebres. | Medio (vuelo) |
| Frailecillo atlántico | Anida en madrigueras de acantilado: los acantilados de aves ya están. | Medio (vuelo y buceo) |
| Charrán ártico | Migra 70 900 km al año (Egevang et al. 2010, *PNAS*). | Medio, estacional |
| Foca leopardo | El depredador real de pingüinos; hoy hace ese papel `Seal`. | Medio |
| Elefante marino | Las playas de Georgia del Sur. | Medio |
| Albatros viajero | 1 550 parejas en Georgia del Sur; anida entre tussac. | Alto (vuelo planeado) |
| Narval | Solo agua: reutiliza `MarineLife`, pero aporta poco a la isla. | Bajo, poco valor |

**El lemming es el primero que falta:** hoy el zorro ártico y el lobo solo tienen liebres, renos, carroña y bayas.

## Fuentes

ADW = Animal Diversity Web (animaldiversity.org), cuentas de cada especie; algunas tienen incoherencias internas. NZ Birds Online (nzbirdsonline.org.nz).
WP = Wikipedia: *Walrus*, *Emperor penguin*, *Reindeer*, *Arctic fox*. Además de las citas de cada ficha: Clarke et al. 2012 (*British Birds* 105:118), para Georgia del Sur.
