# Comportamiento de las especies de las eras (microcosmos)

Especies nuevas de los microcosmos por era/lugar (los **cuerpos** y el **ciclo de vida** —`InsectMorphology`,
`CubeInsect`, `StageProfile`, `MicroBiome`, `Metamorphosis`— los lleva el hilo de cuerpos; aquí va la **conducta**).
Regla de la casa: nada es un guion de especie. Cada conducta es **data** (catálogos por especie) o un **componente**
genérico que decide con los **stats y el estado del individuo** (hambre, celo, estrés, temple, agilidad, afabilidad,
vínculos). Por eso dos cochinillas no se desenrollan igual, y una tijereta hambrienta deja el nido antes que una saciada.

Todas son entomántropos (`Lucidity.HasHumanMind`): erguidas y con mente humana en calma; cuando aprieta el estrés,
el hambre o se acorazan, manda el cuerpo.

## 1. Dónde vive cada cosa

| Qué | Archivo |
|---|---|
| Clase mínima por especie (amenaza: `armadura`/`armament`/agresividad) | `Assets/Animals/{Id}/{Id}Behavior.cs` |
| Qué come | `Forager.ConfigureForSpecies` (+ `carrionOnly`, `eatsPlants`), `DietPreference`, `Ethogram` (Feed) |
| Qué hace en calma | `Ethogram` (actividad + interactuable + fuente) → `Habit` |
| De día o de noche | `SleepCycle.IsNocturnal` |
| Gregarismo, reservas, umbral de amenaza | `SpeciesProfile` |
| Velocidades | `ActionsPrep` (relativas a la hormiga: andar 3, correr 10) |
| Reproducción | `Reproduction.Calendar` + `Reproduction.Hermaphrodite` |
| Cuidado de la cría | `BroodRide` (a lomos), deseo `brood` (guardar el nido) y `nurse` (alimentar) |
| Defensa pasiva (bola/concha) | `PassiveDefence` + `Reaction.Hold` en `ThreatResponder` |
| Señal de cortejo (destellos/canto) | `MatingSignal` + hábito `Signal` + deseo `mate` |
| Carrera de lado | `SidewaysGait` |
| Cuerpo/mente base, relaciones | `Archetypes` |
| Pensamientos | `PhrasePools.Especie` |
| Registro de prefab (sin él `AreaMicrocosmos` no las planta) | `Editor/AnimalPrefabGenerator.Species` |

## 2. Mecánicas nuevas (genéricas)

**Defensa pasiva (`PassiveDefence`, `Reaction.Hold`).** Ante una amenaza que no puede vencer, el ser elige entre huir
y acorazarse según la **agilidad** de los dos: si el otro es más ágil (no puede escapar), se acoraza. Mientras dura,
su `armadura` sube; como `Predation.Defense` la lee, un depredador que ya no puede con ella la descarta solo. Tiempo
acorazado = base × (1 + estrés) ÷ temple, ×1,3 en machos.
- Cochinilla: bola (conglobación). Los machos tardan más en desenrollarse bajo olor de depredador
  ([Zamora-Camacho 2023, PeerJ](https://doi.org/10.7717/peerj.16696)); la latencia depende del estado previo
  ([Matsuno & Moriyama 2012](https://doi.org/10.1556/ABiol.63.2012.Suppl.2.9)).
- Caracol: se mete en la concha. Babosa: se contrae en semiesfera y se balancea minutos (Rowson et al. 2014).
  Lombriz: se retrae.

**Señal de cortejo (`MatingSignal`).** Solo llama el macho **adulto en celo** (`EstrusState`) y solo cuando su hábito
`Signal` gana (está saciado y en calma): el coro sale de los individuos. Las hembras en celo que perciben a uno van
hacia él (deseo `mate`).
- Luciérnaga (*Luciola*): destello verde-amarillo (~560 nm) ~1/s desde un tallo; la hembra contesta con retardo
  (Lewis & Cratsley 2008, Annu Rev Entomol 53:293;
  órgano de luz: [Fu & Zhu 2024](https://doi.org/10.1038/s41467-024-45559-7)). Se cuelga del hueso "Linterna" del cuerpo.
- Cigarra (*Cicada orni*): canto de timbal sintetizado (AudioSource 3D) desde troncos, posada bajo
  ([Sueur & Aubin 2003](https://doi.org/10.1007/s00114-003-0432-5)); la hembra oye bien la frecuencia del macho
  ([Sueur et al. 2008](https://doi.org/10.1242/jeb.018804)). Frecuencia y ritmo del sintetizador: **[A] calibrar**
  con grabaciones ([Fonseca & Revez 2002](https://doi.org/10.1242/jeb.205.9.1285) para *C. barbara*).
- Grillo (*Gryllus campestris*, ya existente): ahora también canta (`Kind.Song`, ~4,5 kHz en chirps de 3–4 sílabas) a la entrada de su madriguera cuando está en celo; la hembra va al canto.

**Cuidado de la cría (`BroodRide`, deseos `brood`/`nurse`).** Al parir, la madre de las especies con cuidado se vincula
con la cría.
- A lomos: escorpión (hasta la 1.ª muda, Polis & Sissom 1990) y araña lobo (~1 semana, Foelix 2011). La cría no come
  mientras va arriba (vive del vitelo).
- Guardar el nido: tijereta (Kölliker 2007, Behav Ecol Sociobiol 61:1489; cuidado regulado por hormona
  juvenil: [Pasquier et al. 2025](https://doi.org/10.1016/j.yhbeh.2025.105839)), solífugo y escorpión. La fuerza del
  deseo = vínculo × afabilidad, así que compite con el hambre.

**Hermafroditas.** Caracol, babosa y lombriz se aparean con cualquier adulto de su especie y **ponen los dos**.

**Carroñeros y omnívoros (`Forager`).** `carrionOnly`: de las presas solo cuenta lo muerto (cucaracha, cochinilla).
`eatsPlants`: el omnívoro (tijereta, cucaracha, cochinilla) también come lo vegetal de su `Ethogram` cuando tiene
hambre, y elige por cercanía entre presa y planta.

**Carrera de lado (`SidewaysGait`).** El cangrejo, cuando manda el cuerpo, gira el costado hacia el avance (cambia de
lado de vez en cuando). Erguido y lúcido camina de frente como una persona.

## 3. Fichas

| Especie (id) | Era / lugar | Actividad | Come | Defensa | Reproducción / cría | Conducta implementada |
|---|---|---|---|---|---|---|
| Escorpión (`Scorpion`) *Leiurus quinquestriatus* (Egipto), *Heterometrus* (Ganges), *Mesobuthus gibbosus* (Egina) | Metales / Antigüedad | nocturno | insectos (grillo, cucaracha, termita); caníbal ocasional | aguijón + quelas (`armament` 0,85) | vivíparo, gestación 122–277 d (Ross 2009, J Venom Anim Toxins 15:347); crías a lomos | acecho en la boca de la madriguera (Polis 1979); guarda la cría |
| Solífugo (`CamelSpider`) *Galeodes arabs* | desiertos de Oriente Medio | nocturno, lucífugo | termitas, insectos, lagartijas | quelíceros (sin veneno) | la hembra guarda la puesta en la madriguera (Punzo 1998) | corre de sombra en sombra; el más rápido junto al cangrejo |
| Cochinilla (`Woodlouse`) *Armadillidium vulgare*, *Porcellio scaber* | templado | nocturna | hojarasca, hongos, carroña (Zimmer 2002, Biol Rev 77:455) | **bola** | marsupio ~3–4 sem | se agrega bajo piedras húmedas (Broly et al. 2013, Insect Soc 60:419) |
| Caracol (`Snail`) *Helix lucorum*, *Cepaea nemoralis* | Anatolia/Grecia, Europa | nocturno, con lluvia | hojas, hongos, flores | **concha** | hermafrodita | estiva sellado en un tallo (Jones et al. 1977) |
| Babosa (`Slug`) *Arion ater* | Europa | nocturna | hongos, hojas, heces | **contracción + balanceo**, moco | hermafrodita; casi anual | se esconde de la luz bajo troncos |
| Cigarra (`Cicada`) *Cicada orni* | Mediterráneo | diurna (calor) | xilema | huye volando | ninfa años bajo tierra | **canta** desde los troncos |
| Luciérnaga (`Firefly`) *Luciola* | templado | nocturna | larva: caracoles y babosas (solo la larva caza: `Forager.preyOnlyAsLarva`); adulto: apenas néctar/rocío | sangre tóxica (lucibufaginas) | — | **destellos** de cortejo |
| Tijereta (`Earwig`) *Forficula auricularia* | Europa | nocturna | pulgones, polen, hojas | cercos-pinza | **madre que cuida** huevos y ninfas | se mete en grietas; guarda el nido; alimenta |
| Cucaracha (`Cockroach`) *Blatta orientalis* | ciudades antiguas | nocturna | carbohidratos, carroña ([Sieksmeyer et al. 2022](https://doi.org/10.1186/s12862-022-02007-8)) | huida veloz (cercos) | ooteca | se agrega en grietas (Bell et al. 2007) |
| Cangrejo fantasma (`GhostCrab`) *Ocypode quadrata* | playas atlánticas | nocturno | almejas, pulgas de mar, huevos y pollos de aves ([Tewfik et al. 2016](https://doi.org/10.1371/journal.pone.0147759); [Call et al. 2024](https://doi.org/10.1371/journal.pone.0307821)) | caparazón + pinzas | larvas planctónicas | **corre de lado**; vuelve a su madriguera ([Gül & Griffen 2019](https://doi.org/10.1002/ece3.5853)) |
| Pulga de las nieves (`Scorpionfly`) *Boreus hyemalis* | invierno, Europa | diurna, en invierno | musgo ([Pyszko et al. 2024](https://doi.org/10.3390/insects15110878)) | salta ([Burrows 2019](https://doi.org/10.1242/jeb.205385)) | ciclo ~2 años | se asolea en la nieve; activa en invierno ([Zangl et al. 2021](https://doi.org/10.7717/peerj.11424)) |
| Ácaro de terciopelo (`VelvetMite`) *Trombidium holosericeum* | templado | diurno, tras la lluvia | huevos de insecto, pulgones; larva parásita ([Shatrov & Felska 2017](https://doi.org/10.1007/s10493-017-0172-4)) | rojo aposemático, sabor | larva busca huésped | recorre el suelo, se asolea |
| Lombriz (`Earthworm`) *Lumbricus terrestris* | todas | nocturna | hojarasca, estiércol | **se retrae** | hermafrodita; capullo ~3 meses (Butt & Nuutinen 1998) | **arrastra hojas** a su galería (Darwin 1881) |

Los enlaces DOI están comprobados en PubMed. **[A]** = valor aproximado de manual, a calibrar en vivo (está marcado así en el código). Las citas sin enlace son libros
o artículos clásicos: Polis 1979 (J Zool 188:333); Polis & Sissom 1990 (*The Biology of Scorpions*); Punzo 1998 (*The
Biology of Camel-Spiders*); Foelix 2011 (*Biology of Spiders*); Rowson et al. 2014 (*Slugs of Britain and Ireland*);
Jones, Leith & Rawlings 1977 (Annu Rev Ecol Syst 8:109); Bell, Roth & Nalepa 2007 (*Cockroaches*); Darwin 1881 (*The
Formation of Vegetable Mould through the Action of Worms*); Butt & Nuutinen 1998 (Can J Zool 76:104); Burrows & Hoyle
1973 (J Exp Biol 58:327).

## 4. Fotos de referencia (para el cuerpo de cubos y para validar la conducta)

Wikimedia Commons (libres) e iNaturalist (observaciones con foto, por lugar):

- Escorpión: [Leiurus quinquestriatus](https://commons.wikimedia.org/wiki/Category:Leiurus_quinquestriatus) · [Heterometrus](https://commons.wikimedia.org/wiki/Category:Heterometrus) · [Mesobuthus gibbosus](https://www.inaturalist.org/taxa/search?q=Mesobuthus+gibbosus) · [madre con crías a lomos](https://commons.wikimedia.org/w/index.php?search=scorpion+babies+on+back&title=Special:MediaSearch&type=image)
- Solífugo: [Galeodes](https://commons.wikimedia.org/wiki/Category:Galeodes) · [Galeodes arabs](https://www.inaturalist.org/taxa/search?q=Galeodes+arabs)
- Cochinilla: [Armadillidium vulgare](https://commons.wikimedia.org/wiki/Category:Armadillidium_vulgare) (bola: [búsqueda](https://commons.wikimedia.org/w/index.php?search=Armadillidium+vulgare+rolled&title=Special:MediaSearch&type=image)) · [Porcellio scaber](https://commons.wikimedia.org/wiki/Category:Porcellio_scaber)
- Caracol: [Helix lucorum](https://commons.wikimedia.org/wiki/Category:Helix_lucorum) · [Cepaea nemoralis](https://commons.wikimedia.org/wiki/Category:Cepaea_nemoralis) · [epifragma](https://commons.wikimedia.org/w/index.php?search=snail+epiphragm&title=Special:MediaSearch&type=image)
- Babosa: [Arion ater](https://commons.wikimedia.org/wiki/Category:Arion_ater)
- Cigarra: [Cicada orni](https://commons.wikimedia.org/wiki/Category:Cicada_orni)
- Luciérnaga: [Luciola](https://commons.wikimedia.org/wiki/Category:Luciola) · [larva comiendo caracol](https://commons.wikimedia.org/w/index.php?search=firefly+larva+snail&title=Special:MediaSearch&type=image)
- Tijereta: [Forficula auricularia](https://commons.wikimedia.org/wiki/Category:Forficula_auricularia) · [madre con huevos](https://commons.wikimedia.org/w/index.php?search=earwig+eggs+mother&title=Special:MediaSearch&type=image)
- Cucaracha: [Blatta orientalis](https://commons.wikimedia.org/wiki/Category:Blatta_orientalis)
- Cangrejo fantasma: [Ocypode quadrata](https://commons.wikimedia.org/wiki/Category:Ocypode_quadrata)
- Pulga de las nieves: [Boreus hyemalis](https://commons.wikimedia.org/wiki/Category:Boreus_hyemalis)
- Ácaro de terciopelo: [Trombidium holosericeum](https://commons.wikimedia.org/wiki/Category:Trombidium_holosericeum)
- Lombriz: [Lumbricus terrestris](https://commons.wikimedia.org/wiki/Category:Lumbricus_terrestris) · [galería con hojas (middens)](https://commons.wikimedia.org/w/index.php?search=Lumbricus+terrestris+midden&title=Special:MediaSearch&type=image)

## 5. Qué mirar en la prueba en vivo

**Verificación automática (sin esperar a la noche ni a un depredador):** `EraSpeciesTest` (grupo 29 del `TestRunner`
de SampleScene, `[TEST]` en consola) monta cada escenario en una losa aparte: pone la hora, mete una araña junto a la
cochinilla, fuerza el celo de luciérnagas/cigarra/grillo y hace parir a un escorpión. Comprueba la bola (`Hold` +
armadura + `Predation.Defense`), destello y respuesta de la hembra, canto, que solo la larva de luciérnaga caza, la cría
al lomo con vínculo, y que la luciérnaga duerme a mediodía. Lo de abajo es para MIRARLO en los microcosmos.

Los prefabs de las especies nuevas ya no hacen falta generarlos a mano: los builders (`AreaMicrocosmos`,
`MicrocosmosSceneBuilder`, `SampleSceneBuilder`) llaman a `AnimalPrefabGenerator.LoadOrCreate`, que crea el que falte.

1. De día: cigarras cantando en troncos (se oye), pulgas de las nieves y ácaros al sol; el resto duerme.
2. Al anochecer: luciérnagas destellando en tallos; si hay hembra en celo cerca, contesta y el macho va hacia ella.
3. Acercar una araña o un escorpión a una cochinilla: se hace bola (si el cuerpo ya lee `PassiveDefence.Holding`) y el
   depredador la deja si su poder no supera la defensa con la bola.
4. Escorpión madre: tras parir, la cría va en su lomo unos días de juego y luego baja.
5. Tijereta madre: se queda en su sitio mientras tenga ninfas pequeñas vinculadas, salvo que el hambre apriete.
6. Lombriz de noche: lleva hojas a su casa. Cangrejo huyendo: corre de lado.
7. Log: `[Hábito]` (actividades), `[Cría]` (baja del lomo), `[ECO]` (poblaciones).

## 6. Pendiente

- Cablear `PassiveDefence.Holding` → `CubeInsect.curl` (lo hace quien mergee segundo, hilo de cuerpos).
- Dieta por etapa (larva de luciérnaga caza, el adulto casi no come) y larva parásita del ácaro sobre un huésped.
- Humedad/lluvia como estado del mundo: hoy el caracol estiva por hábito, no por sequía real.
