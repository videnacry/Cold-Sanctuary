# Fauna de cada era: los insectos-humanos nuevos

> Berón, 2026-10-24: «investiga la flora y fauna de cada era/lugar de los microcosmos y crea los insectos-humanos para
> cada una»; «que investigue los comportamientos y ciclo de vida a detalle, que busque fotos, es ideal recrear al detalle».
>
> Código: fichas en `Assets/Scripts/Microcosmos/InsectMorphology.Fauna.cs`, piezas en `CubeInsect.Fauna.cs`, ciclo en
> `Assets/Animals/StageProfile.cs` (+ `Development.Larval` en `Metamorphosis`), biomas en `Assets/Editor/MicroBiome.cs`.
> La conducta (dieta, etograma, defensa pasiva, cuidado de las crías, destellos, carrera lateral…) es del hilo
> «Recrear comportamiento de especies nuevas» (`docs/era-species-behavior.md`).
> [A] = aproximación de manual, sin fuente primaria comprobada en esta pasada.

Todas son insectos-humanos como el resto (cara, cintura de centauro: erguidos cuando están lúcidos, a todas las patas
cuando el cuerpo manda). Las especies sin patas (caracol, babosa, lombriz) alzan la parte delantera del cuerpo, como una
cobra; las que tienen pinzas (escorpión, cangrejo) o pedipalpos largos (solífugo) los usan de **brazos**.

## Las especies y dónde viven

| Id | Especie real | Microcosmos (bioma) | Activa en la historia |
|---|---|---|---|
| `Scorpion` | *Heterometrus* sp. · *Mesobuthus gibbosus* · *Leiurus quinquestriatus* | Patanjali · Egina · Alejandría | sí, de noche |
| `CamelSpider` | *Galeodes arabs* | Alejandría | sí, de noche |
| `VelvetMite` | *Trombidium* sp. («bir bahuti») | Kapilavastu (tras el monzón) | sí, tras las lluvias |
| `Woodlouse` | *Porcellio scaber* · *Armadillidium vulgare* | Varsovia · Escútari · Flandes | lenta, bajo el escombro |
| `GhostCrab` | *Ocypode quadrata* | Kill Devil Hills | **aletargado** (hiberna oct–abr) |
| `Snail` | *Helix lucorum* · *Cepaea nemoralis* | Escútari (**aletargado**, epifragma) · jardín de la Planta Atómica | Cepaea sí |
| `Slug` | *Arion ater* | Flandes | poco |
| `Earthworm` | *Lumbricus terrestris* | Le Chambon · Flandes · jardín | sí (si no hiela) |
| `Cicada` | *Cicada orni* | Egina | sí (jun–sep) |
| `Firefly` | *Luciola* sp. | Kapilavastu | sí, al anochecer |
| `Earwig` | *Forficula auricularia* | jardín de la Planta Atómica | sí, de noche |
| `Cockroach` | *Blatta orientalis* | Varsovia · Escútari | sí (sótanos, cuarteles) |
| `Scorpionfly` | *Boreus hyemalis* («pulga de nieve») | el santuario de hoy (hielo) | sí, **sobre la nieve** (oct–mar) |

Las aletargadas están en el mundo pero quietas (`Dormant`: duermen sin que el reloj las despierte). El piojo y la pulga
(tifus de Varsovia, Escútari y las trincheras) quedan solo como referencia histórica, sin cuerpo en el juego.

## Forma, ciclo de vida y dimorfismo

**Escorpión.** Prosoma con 4 pares de patas, 7 terguitos del mesosoma (más anchos en medio), la **cola** (metasoma de 5
segmentos, cada uno algo más largo, + telson con la vesícula del veneno y el aguijón) curvada sobre el dorso, las
**pinzas** (pedipalpos) y los **peines** bajo el vientre. *Leiurus*: 30–77 mm, amarillo, pinzas finas (buthido).
Ciclo directo: **vivíparo**; las crías nacen blancas y viajan sobre el dorso de la madre hasta la 1.ª muda (en
*Centruroides gracilis* a los 8 días; se dispersan a los 12–15), 5–6 mudas, maduran a los ~8–10 meses y viven ~3 años
en laboratorio (Francke & Jones 1982, *J. Arachnol.* 10:223). Dimorfismo: el macho con el mesosoma más estrecho, la cola
más larga y los **peines mayores** (en *Heterometrus laoticus* los peines del macho son significativamente más largos);
la hembra, más ancha.

**Solífugo.** Quelíceros **enormes**, de dos piezas, con dientes (hasta 1/3 del cuerpo, los mayores para su talla de los
artrópodos), pedipalpos largos como patas con un **órgano adhesivo** en la punta (con ellos sujeta y trepa: son sus
brazos), maléolos bajo el último par; corre hasta ~16 km/h, sobre todo de noche y en la estación cálida. El **macho**
adulto lleva un **flagelo** sobre los quelíceros (AMNH, «Camel spiders»). Ciclo directo, corto y estacional (≈1 año) [A].

**Ácaro de terciopelo.** Un solo cuerpo velludo rojo, sin cabeza, tórax ni abdomen separados, ni antenas; el 1.er par
de patas tantea como antenas. Ciclo **larvario**: huevo (3–4 d) → **larva de 6 patas, parásita** de saltamontes,
grillos y langostas → protoninfa (inmóvil) → deutoninfa (depredadora, 8 patas) → tritoninfa (inmóvil) → adulto
depredador; la hembra, mayor. El macho teje un «jardín» de esperma tras una danza (Macalester Ordway, *Red velvet mite*).
En el juego: larva → ninfa → adulto (las fases inmóviles se resumen en la ninfa).

**Cochinilla.** Cabeza, **7 placas** del pereion, 2 del pleon, el telson y los **urópodos**; antenas acodadas; 7 pares de
patas. Crustáceo terrestre: la hembra lleva los huevos (~0,7 mm) en el **marsupio** bajo el vientre; las crías salen
como **manca**, con **6 pares de patas** (el 7.º llega con la muda); madura a los ~15 meses (*P. scaber*; ~25 en
*A. vulgare*) y vive 30–41 meses (scaber.org, *Woodlouse life cycle*). *Armadillidium* se hace **bola** (defensa).

**Cangrejo fantasma.** Caparazón casi **cuadrado** (~5 cm), ojos en **pedúnculos** largos en maza (visión de 360°),
**pinzas desiguales** y patas largas; el más rápido de los invertebrados terrestres (>1 m/s), de lado. Ciclo
**larvario**: 5 **zoeas** en el plancton + una **megalopa** (≥35 días, de las mayores de los braquiuros) que se
transforma en la rompiente; madura al año, vive ~3 años; de noche; hiberna en su madriguera de **octubre a abril**. El
macho, algo mayor; la hembra, con el abdomen plegado ancho (Animal Diversity Web; Wikipedia).

**Caracol.** Pie, cabeza con 2 **tentáculos largos con ojos** y 2 cortos, y la **concha en espiral**: *Helix lucorum*
35–60 mm, crema con bandas pardas; *Cepaea nemoralis* amarilla/rosa con hasta 5 bandas y el **labio** pardo.
Hermafrodita (con «dardo del amor»); 30–80 huevos que eclosionan a los 15–20 días; el joven **no tiene labio** y tarda
1–3 años en formarlo; vive ~2–6 años (ADW, *Cepaea nemoralis*). Defensa: se **mete en la concha**.

**Babosa negra.** Sin concha visible (granitos dentro del **manto**), el **pneumostoma** a la derecha del manto, pie
rugoso; 10–15 cm. Hermafrodita; 20–50 huevos que eclosionan a las ~6 semanas; la cría nace **naranja**, pasa a pajiza y se
vuelve negra (ADW, *Arion ater*). Defensa: se **encoge** en semiesfera.

**Lombriz de tierra.** ~150 anillos, el **clitelo** a un cuarto del cuerpo, cola aplanada. Hermafrodita; el clitelo
hace el capullo (un huevo; 5–10 al año en *L. terrestris*); el joven **sin clitelo**; vive 4–8 años (Vigie-Nature École,
*Life cycle of earthworms*).

**Cigarra del fresno.** Cabeza ancha con los **ojos compuestos a los lados**, tórax alto, **alas transparentes con
manchas** negras en tejado, más largas que el cuerpo (25 mm; 70 mm de envergadura), pico para la savia. Ciclo
incompleto: huevos en ramitas (6–10 semanas), la **ninfa** (4 estadios, patas delanteras **excavadoras**) pasa **varios
años bajo tierra** chupando savia de las raíces, sube a un tronco y muda; el adulto vive ~6 semanas. Solo el **macho
canta**, con los **timbales** del abdomen; la hembra pone con el oviscapto (Wikipedia, *Cicada orni*; Clemson HGIC).

**Luciérnaga.** Pronoto en escudo (rosado en muchas *Luciola*) que tapa la cabeza, élitros blandos pardos y la
**linterna** bajo la punta del abdomen: **2 segmentos en el macho, 1 en la hembra** (carácter del género). Ciclo
completo: la **larva**, depredadora de caracoles y babosas, **también brilla** (y en algunas especies, los huevos);
pupa; el adulto vive 1–3 semanas. Los machos destellan buscando a las hembras, que responden desde una hoja
(Banglapedia, *Firefly*; Wikipedia, *Aquatica lateralis*).

**Tijereta.** Plana y alargada, tegminas cortas, alas posteriores grandes plegadas, y los **fórceps** (cercos): **rectos
y finos (~3 mm) en la hembra**, **anchos, curvos y dentados en el macho**. ~50 huevos en un nido; la **madre los limpia
y defiende** y cuida a las ninfas (4 estadios) hasta ~1 mes (Wikipedia, *Forficula auricularia*).

**Cucaracha oriental.** Plana, pronoto en escudo, antenas largas, patas espinosas, cercos cortos. El **macho** (18–29 mm,
más estrecho) con **alas hasta 3/4 del abdomen**; la **hembra** (20–27 mm, más ancha) solo con **muñones**. Ooteca de ~16
huevos (eclosión 42–81 días), ninfa 164–542 días, adulto 160–180 días (UF/IFAS; Wikipedia).

**Pulga de nieve (Boreus).** Un mecóptero de 3–5 mm con **pico largo** hacia abajo, abdomen abultado y patas para saltar;
activo sobre la nieve de octubre a marzo. El **macho** con las alas reducidas a **cerdas curvas** con las que sujeta a la
hembra; la **hembra** con muñones y el **oviscapto en sable**. La larva vive y come en el musgo y pupa en el suelo en
septiembre (Wikipedia, *Boreus hyemalis*, *Snow scorpionfly*).

## Tamaños de las fases (respecto al adulto)

Reales aunque salgan diminutas (`InsectMorphology.PhaseScale`): zoea ×0,03 (~1 mm frente a 5 cm), megalopa ×0,1
(~4,5 mm), manca ×0,15 (~2 mm), larva del ácaro ×0,15 (~0,5 mm), larva madura de luciérnaga ×1,4 (mayor que el adulto),
ninfa de último estadio de cigarra ×0,9, de cucaracha ×0,85, de tijereta ×0,75; los juveniles de desarrollo directo a
media vida (escorpión ×0,45, caracol ×0,4, lombriz ×0,4) [A].

## Fotos de referencia

Para comparar las formas con las fichas (iNaturalist, fotos con licencia libre por especie):
[Leiurus quinquestriatus](https://www.inaturalist.org/taxa/search?q=Leiurus%20quinquestriatus) ·
[Heterometrus](https://www.inaturalist.org/taxa/search?q=Heterometrus) ·
[Galeodes arabs](https://www.inaturalist.org/taxa/search?q=Galeodes) ·
[Trombidium](https://www.inaturalist.org/taxa/search?q=Trombidium) ·
[Porcellio scaber](https://www.inaturalist.org/taxa/search?q=Porcellio%20scaber) ·
[Armadillidium vulgare](https://www.inaturalist.org/taxa/search?q=Armadillidium%20vulgare) ·
[Ocypode quadrata](https://www.inaturalist.org/taxa/search?q=Ocypode%20quadrata) ·
[Helix lucorum](https://www.inaturalist.org/taxa/search?q=Helix%20lucorum) ·
[Cepaea nemoralis](https://www.inaturalist.org/taxa/search?q=Cepaea%20nemoralis) ·
[Arion ater](https://www.inaturalist.org/taxa/search?q=Arion%20ater) ·
[Lumbricus terrestris](https://www.inaturalist.org/taxa/search?q=Lumbricus%20terrestris) ·
[Cicada orni](https://www.inaturalist.org/taxa/search?q=Cicada%20orni) ·
[Luciola](https://www.inaturalist.org/taxa/search?q=Luciola) ·
[Forficula auricularia](https://www.inaturalist.org/taxa/search?q=Forficula%20auricularia) ·
[Blatta orientalis](https://www.inaturalist.org/taxa/search?q=Blatta%20orientalis) ·
[Boreus hyemalis](https://www.inaturalist.org/taxa/search?q=Boreus%20hyemalis).

## Fuentes

- Francke, O. F. & Jones, S. K. (1982). The life history of *Centruroides gracilis*. *J. Arachnol.* 10:223–239. https://www.americanarachnology.org/journal-joa/joa-all-articles/article/download/JoA_v10_p223.pdf
- Sexual dimorphism in *Heterometrus laoticus* (Naresuan Univ. J. Sci.). https://www.sci.nu.ac.th/sciencejournal/index.php/sci/article/view/80
- AMNH, «Camel spiders» (Bird et al. 2015, quelíceros y flagelo). https://www.amnh.org/content/download/99370/1905823/file/Camel%20spiders.pdf
- Macalester Ordway Field Station, «Red velvet mite». https://www.macalester.edu/ordway/?p=256
- scaber.org, «Woodlouse life cycle». https://scaber.org/porcellio-sub/woodlice/lcycle/
- Animal Diversity Web: *Ocypode quadrata* https://animaldiversity.org/accounts/Ocypode_quadrata/ · *Cepaea nemoralis* https://animaldiversity.org/accounts/Cepaea_nemoralis · *Arion ater* https://animaldiversity.org/accounts/Arion_ater/
- Wikipedia: *Ocypode quadrata*, *Leiurus quinquestriatus*, *Helix lucorum*, *Cicada orni*, *Forficula auricularia*, *Oriental cockroach*, *Boreus hyemalis*, *Snow scorpionfly*, *Aquatica lateralis*.
- Vigie-Nature École, «Life cycle of earthworms». https://www.vigienature-ecole.fr/en/node/128
- Clemson HGIC, «Cicadas». https://hgic.clemson.edu/factsheet/cicadas
- UF/IFAS, «Oriental cockroach». https://entnemdept.ufl.edu/projex/gallery/dl/cockroaches/text/oriental_cockroach.htm
- Banglapedia, «Firefly». https://en.banglapedia.org/index.php/Firefly
