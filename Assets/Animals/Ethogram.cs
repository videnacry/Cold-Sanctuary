using System.Collections.Generic;

/// <summary>Qué le hace al ser una actividad de su etograma.</summary>
public enum HabitEffect
{
    Feed,       // come del interactuable (baja el hambre, gasta existencias)
    Carry,      // recoge y lo lleva a casa (semilla, hierba, bola de estiércol, guijarro)
    Rest,       // descansa (baja el cansancio)
    Shelter,    // se refugia (baja el estrés)
    Watch,      // vigila / patrulla (sube la alerta perceptiva sin asustarse)
    Bask,       // se asolea (recupera energía)
    Ambush,     // espera inmóvil a la presa (sit-and-wait)
    StillHunt,  // acecho junto a un respiradero (oso polar): si sale una foca, la caza
    Rub,        // se frota (muda de piel de la ballena)
    Huddle,     // se apiña con los suyos (pingüinos con frío)
    Signal,     // llama a pareja desde un posadero: destellos (luciérnaga), canto (cigarra). Ver MatingSignal; sin celo = descansa
}

/// <summary>Una actividad natural de una especie: con QUÉ interactúa, cuánto la prefiere y cuánto dura.</summary>
public readonly struct EthoAct
{
    public readonly AffordanceKind kind;
    public readonly float weight, seconds;
    public readonly HabitEffect effect;
    public readonly string verb, source;
    public EthoAct(AffordanceKind kind, float weight, float seconds, HabitEffect effect, string verb, string source)
    { this.kind = kind; this.weight = weight; this.seconds = seconds; this.effect = effect; this.verb = verb; this.source = source; }
}

/// <summary>
/// ETOGRAMA por especie (docs/natural-interactions.md): lo que cada animal o insecto HACE en la naturaleza y con qué
/// elemento del entorno, con su fuente. Es data: el deseo «habit» (<see cref="DesireCatalog"/>) elige una actividad de
/// aquí cuando el ser está en calma y el <see cref="Habit"/> la ejecuta sobre un <see cref="Affordance"/> real. El
/// <see cref="Forager"/> lee <see cref="Eats"/> para saber qué plantas/interactuables cuentan como comida.
/// Las focas, ballenas y orcas nadan fuera del NavMesh: sus actividades las ejecuta <see cref="MarineLife"/> con estos datos.
/// </summary>
public static class Ethogram
{
    static readonly EthoAct[] None = new EthoAct[0];

    static readonly Dictionary<string, EthoAct[]> _bySpecies = new Dictionary<string, EthoAct[]>
    {
        // ── MICROCOSMOS (insectos con mente humana, a escala de la hormiga) ──
        { "Ant", new[] {
            new EthoAct(AffordanceKind.Seed, 0.5f, 6f, HabitEffect.Carry, "cosecha una semilla y la lleva al granero", "MacMahon et al. 2000, Annu Rev Ecol Syst 31:265 (hormigas granívoras)"),
            new EthoAct(AffordanceKind.Honeydew, 0.4f, 10f, HabitEffect.Feed, "ordeña la melaza de un pulgón", "Stadler & Dixon 2005, Annu Rev Ecol Evol Syst 36:345 (mutualismo hormiga-pulgón)"),
            new EthoAct(AffordanceKind.Shelter, 0.1f, 20f, HabitEffect.Shelter, "descansa a la sombra", "") } },
        { "Termite", new[] {
            new EthoAct(AffordanceKind.Grass, 0.6f, 8f, HabitEffect.Carry, "corta hierba y la lleva al termitero", "Hodotermes mossambicus: termita cosechadora de pasto (Wikipedia «Hodotermes»; Leuthold, IUSSI 1973)"),
            new EthoAct(AffordanceKind.Fungus, 0.3f, 15f, HabitEffect.Feed, "atiende el jardín de hongos", "Macrotermitinae-Termitomyces (Aanen et al. 2002, PNAS 99:14887)"),
            new EthoAct(AffordanceKind.Shelter, 0.1f, 20f, HabitEffect.Shelter, "se esconde de la luz", "obreras lucífugas") } },
        { "Aphid", new[] {
            new EthoAct(AffordanceKind.PlantSap, 1f, 40f, HabitEffect.Feed, "chupa savia del tallo", "Dixon 1998, Aphid Ecology (floema; melaza)") } },
        { "Ladybug", new[] {
            new EthoAct(AffordanceKind.PlantSap, 0.5f, 12f, HabitEffect.Watch, "recorre los tallos buscando pulgones", "Hodek, Honěk & van Emden 2012, Ecology and Behaviour of the Ladybird Beetles"),
            new EthoAct(AffordanceKind.Flower, 0.2f, 10f, HabitEffect.Feed, "come polen de una flor", "polen como alimento alternativo (Lundgren 2009)"),
            new EthoAct(AffordanceKind.Shelter, 0.1f, 25f, HabitEffect.Rest, "se resguarda bajo una piedra", "agregación invernal") } },
        { "DungBeetle", new[] {
            new EthoAct(AffordanceKind.Dung, 0.6f, 10f, HabitEffect.Carry, "hace una bola de estiércol y la rueda hasta su madriguera", "Dacke et al. 2013, Curr Biol 23:298 (orientación por la Vía Láctea)"),
            new EthoAct(AffordanceKind.Shelter, 0.2f, 20f, HabitEffect.Shelter, "entierra su bola", "nidificación de Scarabaeinae (Halffter & Edmonds 1982)") } },
        { "Antlion", new[] {
            new EthoAct(AffordanceKind.SandPit, 1f, 60f, HabitEffect.Ambush, "cava su embudo en la arena fina y espera en el fondo", "Scharf & Ovadia 2006, J Insect Behav 19:197 (selección de sitio del embudo)") } },
        { "Mantis", new[] {
            new EthoAct(AffordanceKind.Flower, 0.6f, 40f, HabitEffect.Ambush, "acecha inmóvil junto a una flor", "caza de emboscada cerca de flores (ESA 2010, paper 50791; Behav Ecol 26:194, 2015)"),
            new EthoAct(AffordanceKind.Perch, 0.4f, 40f, HabitEffect.Ambush, "acecha desde lo alto de un tallo", "sit-and-wait (Battiston et al. 2010)") } },
        { "Cricket", new[] {
            new EthoAct(AffordanceKind.Shelter, 0.5f, 30f, HabitEffect.Shelter, "se mete en su madriguera bajo una piedra y canta en la entrada", "Gryllus campestris (Hissmann 1990, Ethology 85:145)"),
            new EthoAct(AffordanceKind.Seed, 0.3f, 8f, HabitEffect.Feed, "roe una semilla", "omnívoro oportunista"),
            new EthoAct(AffordanceKind.Grass, 0.2f, 8f, HabitEffect.Feed, "mordisquea hierba", "") } },
        { "Grasshopper", new[] {
            new EthoAct(AffordanceKind.Grass, 0.5f, 12f, HabitEffect.Feed, "come hojas de pasto", "Joern 1979; acridios graminívoros"),
            new EthoAct(AffordanceKind.Perch, 0.5f, 20f, HabitEffect.Bask, "se asolea en lo alto de un tallo", "termorregulación conductual (Lactin & Johnson 1998; PMC2999429)") } },
        { "Spider", new[] {
            new EthoAct(AffordanceKind.Perch, 0.8f, 50f, HabitEffect.Ambush, "tiende su tela entre tallos y espera en el centro", "selección de sitio por estructura de la vegetación (Ecol Entomol 2000, 25:423)") } },
        { "Meerkat", new[] {
            new EthoAct(AffordanceKind.Perch, 0.5f, 25f, HabitEffect.Watch, "hace de centinela en un punto alto", "Clutton-Brock et al. 1999, Science 284:1640"),
            new EthoAct(AffordanceKind.Shelter, 0.3f, 30f, HabitEffect.Rest, "descansa en la madriguera", "") } },

        // ── ESPECIES DE LAS ERAS (docs/era-species-behavior.md): cada actividad con su fuente ──
        { "Scorpion", new[] {
            new EthoAct(AffordanceKind.Shelter, 0.6f, 60f, HabitEffect.Ambush, "acecha de noche a la entrada de su madriguera bajo una piedra", "sit-and-wait (Polis 1979, J Zool 188:333; Polis & Sissom 1990)"),
            new EthoAct(AffordanceKind.Shelter, 0.4f, 40f, HabitEffect.Shelter, "se refugia bajo la piedra", "excava hasta ~20 cm bajo piedras (Leiurus; Animal Diversity Web, Ceceli & Horsfield 2012)") } },
        { "CamelSpider", new[] {
            new EthoAct(AffordanceKind.Shelter, 0.6f, 15f, HabitEffect.Watch, "corre de sombra en sombra buscando presas", "forrajeo activo nocturno y lucífugo (Punzo 1998, The Biology of Camel-Spiders)"),
            new EthoAct(AffordanceKind.Shelter, 0.4f, 40f, HabitEffect.Rest, "descansa en su madriguera", "") } },
        { "Woodlouse", new[] {
            new EthoAct(AffordanceKind.Leaf, 0.5f, 15f, HabitEffect.Feed, "come hojarasca húmeda", "detritívoros (Zimmer 2002, Biol Rev 77:455)"),
            new EthoAct(AffordanceKind.Fungus, 0.2f, 12f, HabitEffect.Feed, "ramonea hongos", "micófagos facultativos (Zimmer 2002)"),
            new EthoAct(AffordanceKind.Shelter, 0.3f, 40f, HabitEffect.Shelter, "se agrega con otras bajo una piedra húmeda", "agregación contra la desecación (Broly, Deneubourg & Devigne 2013, Insect Soc 60:419)") } },
        { "Snail", new[] {
            new EthoAct(AffordanceKind.Leaf, 0.4f, 25f, HabitEffect.Feed, "raspa hojas con la rádula", "herbívoro generalista"),
            new EthoAct(AffordanceKind.Fungus, 0.2f, 20f, HabitEffect.Feed, "come hongos", ""),
            new EthoAct(AffordanceKind.Flower, 0.15f, 20f, HabitEffect.Feed, "se come una flor", ""),
            new EthoAct(AffordanceKind.Perch, 0.25f, 90f, HabitEffect.Rest, "trepa a un tallo y sella la concha (estivación)", "estivación sobre la vegetación (Cepaea: Jones, Leith & Rawlings 1977, Annu Rev Ecol Syst 8:109)") } },
        { "Slug", new[] {
            new EthoAct(AffordanceKind.Fungus, 0.35f, 20f, HabitEffect.Feed, "come hongos", "omnívora (Rowson et al. 2014, Slugs of Britain and Ireland)"),
            new EthoAct(AffordanceKind.Leaf, 0.35f, 20f, HabitEffect.Feed, "come hojas tiernas", ""),
            new EthoAct(AffordanceKind.Dung, 0.1f, 15f, HabitEffect.Feed, "come excremento", "detritos animales (Rowson et al. 2014)"),
            new EthoAct(AffordanceKind.Shelter, 0.2f, 60f, HabitEffect.Shelter, "se esconde de la luz bajo un tronco húmedo", "nocturna; sale de día con lluvia") } },
        { "Cicada", new[] {
            new EthoAct(AffordanceKind.PlantSap, 0.5f, 40f, HabitEffect.Feed, "clava el estilete y bebe savia del xilema", "cicádidos: xilemófagos"),
            new EthoAct(AffordanceKind.Perch, 0.5f, 60f, HabitEffect.Signal, "canta con los timbales posado en el tronco", "C. orni canta desde posiciones bajas, en troncos (Sueur & Aubin 2003, doi 10.1007/s00114-003-0432-5)") } },
        { "Firefly", new[] {
            new EthoAct(AffordanceKind.Perch, 0.7f, 60f, HabitEffect.Signal, "destella desde lo alto de un tallo", "cortejo por destellos (Lewis & Cratsley 2008, Annu Rev Entomol 53:293)"),
            new EthoAct(AffordanceKind.Shelter, 0.3f, 40f, HabitEffect.Rest, "descansa en la hierba", "") } },
        { "Earwig", new[] {
            new EthoAct(AffordanceKind.Shelter, 0.5f, 40f, HabitEffect.Shelter, "se mete en una grieta estrecha", "tigmotaxis; nocturna"),
            new EthoAct(AffordanceKind.Flower, 0.3f, 15f, HabitEffect.Feed, "come polen y pétalos", "omnívora"),
            new EthoAct(AffordanceKind.Leaf, 0.2f, 15f, HabitEffect.Feed, "mordisquea hojas", "") } },
        { "Cockroach", new[] {
            new EthoAct(AffordanceKind.Shelter, 0.5f, 40f, HabitEffect.Shelter, "se agrega con otras en una grieta oscura", "agregación por feromona y tigmotaxis (Bell, Roth & Nalepa 2007, Cockroaches: Ecology, Behavior, and Natural History)"),
            new EthoAct(AffordanceKind.Leaf, 0.35f, 15f, HabitEffect.Feed, "roe restos vegetales", "prefiere carbohidratos (Sieksmeyer et al. 2022, doi 10.1186/s12862-022-02007-8)"),
            new EthoAct(AffordanceKind.Dung, 0.15f, 12f, HabitEffect.Feed, "rebusca en excrementos", "carroñera omnívora") } },
        { "GhostCrab", new[] {
            new EthoAct(AffordanceKind.Shelter, 0.5f, 40f, HabitEffect.Shelter, "vuelve a su madriguera de la arena", "alta fidelidad a la madriguera (Gül & Griffen 2019, doi 10.1002/ece3.5853)"),
            new EthoAct(AffordanceKind.Shelter, 0.5f, 25f, HabitEffect.Watch, "otea la playa desde la boca de la madriguera", "ojos en pedúnculos, visión de 360°") } },
        { "Scorpionfly", new[] {
            new EthoAct(AffordanceKind.Grass, 0.6f, 20f, HabitEffect.Feed, "come musgo", "briófago (Pyszko et al. 2024, doi 10.3390/insects15110878)"),
            new EthoAct(AffordanceKind.Perch, 0.2f, 25f, HabitEffect.Bask, "se calienta al sol sobre la nieve", "activo en invierno (Zangl et al. 2021, doi 10.7717/peerj.11424)"),
            new EthoAct(AffordanceKind.Shelter, 0.2f, 30f, HabitEffect.Shelter, "se mete en el musgo", "") } },
        { "VelvetMite", new[] {
            new EthoAct(AffordanceKind.Grass, 0.4f, 20f, HabitEffect.Watch, "recorre el suelo buscando huevos de insecto", "adulto depredador de huevos y artrópodos diminutos"),
            new EthoAct(AffordanceKind.Perch, 0.3f, 25f, HabitEffect.Bask, "toma el sol tras la lluvia", "sale en masa tras las lluvias de primavera"),
            new EthoAct(AffordanceKind.Shelter, 0.3f, 40f, HabitEffect.Shelter, "se entierra en la hojarasca", "") } },
        { "Earthworm", new[] {
            new EthoAct(AffordanceKind.Leaf, 0.5f, 15f, HabitEffect.Carry, "arrastra una hoja hasta su galería", "Darwin 1881, The Formation of Vegetable Mould through the Action of Worms"),
            new EthoAct(AffordanceKind.Dung, 0.2f, 15f, HabitEffect.Feed, "come estiércol", "L. terrestris incorpora estiércol al suelo"),
            new EthoAct(AffordanceKind.Shelter, 0.3f, 40f, HabitEffect.Shelter, "se retrae a su galería", "") } },

        // ── SANTUARIO DE HIELO ──
        { "Penguin", new[] {
            new EthoAct(AffordanceKind.Pebble, 0.5f, 6f, HabitEffect.Carry, "lleva un guijarro a su nido (o se lo roba al vecino)", "Adélie: nidos de piedras y robo de guijarros (California Academy of Sciences, 2004; Notornis 22:77)"),
            new EthoAct(AffordanceKind.HaulOut, 0.3f, 25f, HabitEffect.Rest, "se acicala en el borde del hielo", "acicalamiento: hasta ~1/4 del tiempo en tierra"),
            new EthoAct(AffordanceKind.HaulOut, 0.3f, 30f, HabitEffect.Huddle, "se apiña con la colonia", "Gilbert et al. 2006, Physiol Behav 88:479 (huddling del pingüino emperador)") } },
        { "Bear", new[] {
            new EthoAct(AffordanceKind.BreathingHole, 0.6f, 60f, HabitEffect.StillHunt, "acecha inmóvil junto a un respiradero de foca", "Stirling 1974, Can J Zool 52:1191 (still-hunting)"),
            new EthoAct(AffordanceKind.Den, 0.2f, 60f, HabitEffect.Rest, "descansa en su guarida de nieve", "guaridas temporales en ventisqueros (refugio de tormentas y de hembras preñadas)"),
            new EthoAct(AffordanceKind.Berry, 0.15f, 15f, HabitEffect.Feed, "come bayas de camarina", "Gormezano & Rockwell 2013, Ecol Evol 3:3509 (dieta terrestre de verano)") } },
        { "Seal", new[] {
            new EthoAct(AffordanceKind.HaulOut, 0.6f, 90f, HabitEffect.Rest, "sale a descansar al hielo", "haul-out para descansar y mudar (Kelly et al. 2010, Polar Biol 33:1095)"),
            new EthoAct(AffordanceKind.BreathingHole, 0.4f, 15f, HabitEffect.Rest, "respira y mantiene su respiradero", "foca anillada: respiraderos abiertos con las garras (Smith & Stirling 1975)") } },
        { "Whale", new[] {
            new EthoAct(AffordanceKind.RubbingGravel, 0.4f, 25f, HabitEffect.Rub, "se frota contra la grava para mudar la piel", "Fortune et al. 2017, PLoS ONE 12:e0186156 (ballena de Groenlandia); belugas en estuarios") } },
        { "Orca", new[] {
            new EthoAct(AffordanceKind.HaulOut, 0.4f, 20f, HabitEffect.Watch, "patrulla los témpanos donde descansan las focas", "Pitman & Durban 2012, Mar Mamm Sci 28:16 (wave-washing)") } },
        { "Fox", new[] {
            new EthoAct(AffordanceKind.Den, 0.3f, 40f, HabitEffect.Rest, "descansa en su madriguera", "") } },
        { "Bunny", new[] {
            new EthoAct(AffordanceKind.Willow, 0.6f, 15f, HabitEffect.Feed, "ramonea sauce ártico", "liebre ártica: Salix arctica ~95% de la dieta (Klein & Bay 1994)") } },
        { "Deer", new[] {
            new EthoAct(AffordanceKind.Lichen, 0.6f, 20f, HabitEffect.Feed, "pace liquen de reno", "reno: Cladonia en invierno (Heggberget et al. 2002, Rangifer 22:13)") } },
    };

    public static EthoAct[] Of(string species) => species != null && _bySpecies.TryGetValue(species, out EthoAct[] a) ? a : None;

    /// <summary>¿Cuenta este interactuable como COMIDA para la especie? (lo usa el <see cref="Forager"/>).</summary>
    public static bool Eats(string species, AffordanceKind k)
    {
        foreach (EthoAct a in Of(species))
            if (a.kind == k && (a.effect == HabitEffect.Feed || (a.effect == HabitEffect.Carry && k != AffordanceKind.Pebble))) return true;
        return false;
    }
}
