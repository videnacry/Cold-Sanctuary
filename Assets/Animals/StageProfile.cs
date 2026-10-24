using System.Collections.Generic;

/// <summary>
/// Perfil de CICLO DE VIDA por especie (docs/anima-dissolving-animal.md, etapa 5): duraciones/escala de las 3 etapas
/// (Childhood/Adolescence/Adulthood) + las listas de EVENTS por etapa (que varían: las territoriales llevan HomeBound;
/// los adultos, Feed). Antes eran campos por clase; ahora DATA (valores extraídos 1:1 por script). `Animal` crea las
/// etapas en `Init` desde aquí (cada ser las suyas, porque se mutan: sizePotential). Los PREPS son default de Animal.
///
/// INSECTOS (docs/insect-biology-audit.md): las 3 etapas se LEEN según el desarrollo de la especie (`development`):
/// - `Complete` (holometábolo: hormiga, abeja, mariquita, pelotero, hormiga león): cría = **larva**, juvenil = **pupa**
///   (inmóvil, no come; la lleva <see cref="Metamorphosis"/>), adulto = imago. La pupa ya tiene casi el tamaño final.
/// - `Incomplete` (hemimetábolo: grillo, saltamontes, mantis, termita, pulgón): cría y juvenil = **ninfas** sin alas.
/// - `Direct` (mamíferos, arañas): crecen sin cambiar de forma.
/// - `Larval` (cangrejo: zoea → megalopa; ácaro de terciopelo: larva de 6 patas → ninfa): cría = **larva** activa,
///   juvenil = **ninfa** (forma intermedia), sin pupa.
/// FAUNA DE CADA ERA (docs/microcosmos-fauna-eras.md): escorpión, solífugo, ácaro, cochinilla, cangrejo fantasma,
/// caracol, babosa, lombriz, cigarra, luciérnaga, tijereta, cucaracha y Boreus, con sus duraciones reales.
/// </summary>
public class StageProfile
{
    public enum Development : byte { Direct, Incomplete, Complete, Larval }
    public Development development = Development.Direct;
    /// <summary>Solo `Complete`: la LARVA busca comida por su cuenta (mariquita, hormiga león). false = la alimentan
    /// (hormiga, abeja) o come la provisión que le dejaron (la bola del pelotero).</summary>
    public bool larvaForages;
    public short childDays, teenDays, adultDays;
    public int childMin, childMax, teenMin, teenMax, adultMin, adultMax;
    public byte[] childEvents, teenEvents, adultEvents;

    static readonly StageProfile _default = new StageProfile
    {
        childDays = 60, childMin = 60, childMax = 90, teenDays = 500, teenMin = 60, teenMax = 85, adultDays = 3650, adultMin = 0, adultMax = 20,
        childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest },
        teenEvents  = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest },
        adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed },
    };

    static Dictionary<string, StageProfile> _catalog;

    public static StageProfile Of(string species)
    {
        if (_catalog == null) BuildCatalog();
        return species != null && _catalog.TryGetValue(species, out StageProfile p) ? p : _default;
    }

    static void BuildCatalog()
    {
        _catalog = new Dictionary<string, StageProfile>
        {
            { "Bear", new StageProfile { childDays=180, childMin=98, childMax=99, teenDays=900, teenMin=70, teenMax=78, adultDays=7300, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            { "Bunny", new StageProfile { childDays=50, childMin=50, childMax=80, teenDays=680, teenMin=20, teenMax=40, adultDays=2190, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            { "Deer", new StageProfile { childDays=60, childMin=60, childMax=85, teenDays=540, teenMin=65, teenMax=85, adultDays=4380, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            { "Fox", new StageProfile { childDays=80, childMin=98, childMax=99, teenDays=270, teenMin=70, teenMax=80, adultDays=1095, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            { "Malamute", new StageProfile { childDays=60, childMin=98, childMax=99, teenDays=300, teenMin=70, teenMax=85, adultDays=4380, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            { "Seal", new StageProfile { childDays=45, childMin=50, childMax=80, teenDays=365, teenMin=60, teenMax=80, adultDays=6000, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            { "Whale", new StageProfile { childDays=730, childMin=90, childMax=99, teenDays=2555, teenMin=60, teenMax=90, adultDays=14600, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // Fauna de hielo
            { "Penguin", new StageProfile { childDays=60, childMin=50, childMax=80, teenDays=365, teenMin=60, teenMax=80, adultDays=5000, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            { "Orca", new StageProfile { childDays=730, childMin=90, childMax=99, teenDays=3650, teenMin=60, teenMax=90, adultDays=18000, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            { "Wolf", new StageProfile { childDays=77, childMin=98, childMax=99, teenDays=730, teenMin=70, teenMax=78, adultDays=3285, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // Insectos (días de juego). [A] = aproximación de entomología general; ver docs/insect-biology-audit.md
            // Hormiga: larva (≈2 sem, la alimentan las obreras en el nido) → pupa (≈2 sem) → obrera (meses) [A]
            { "Ant", new StageProfile { development=Development.Complete, childDays=14, childMin=98, childMax=99, teenDays=14, teenMin=5, teenMax=15, adultDays=90, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // Pulgón: nace NINFA viva (vivíparo); 4 mudas en ~7–10 días; el adulto vive 20–30 días [A]
            { "Aphid", new StageProfile { development=Development.Incomplete, childDays=4, childMin=70, childMax=90, teenDays=4, teenMin=50, teenMax=80, adultDays=25, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            // Mariquita: la larva caza pulgones (≈2–3 sem) → pupa (≈1 sem) → adulto (meses) [A]
            { "Ladybug", new StageProfile { development=Development.Complete, larvaForages=true, childDays=14, childMin=98, childMax=99, teenDays=7, teenMin=5, teenMax=15, adultDays=90, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            { "Spider", new StageProfile { childDays=20, childMin=98, childMax=99, teenDays=60, teenMin=70, teenMax=80, adultDays=180, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            { "Cricket", new StageProfile { development=Development.Incomplete, childDays=14, childMin=98, childMax=99, teenDays=30, teenMin=70, teenMax=80, adultDays=60, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            { "Termite", new StageProfile { development=Development.Incomplete, childDays=10, childMin=98, childMax=99, teenDays=25, teenMin=70, teenMax=80, adultDays=120, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // Pelotero: la larva crece DENTRO de la bola de excremento (la come; no sale) → pupa en la bola → adulto [A]
            { "DungBeetle", new StageProfile { development=Development.Complete, childDays=21, childMin=98, childMax=99, teenDays=10, teenMin=5, teenMax=15, adultDays=120, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            // Hormiga león: la LARVA es la del embudo y vive 1–3 años quieta al acecho (sin Wander) → pupa (≈1 mes) →
            // adulto ALADO que vive unas semanas y ya no hace embudo [A]
            { "Antlion", new StageProfile { development=Development.Complete, larvaForages=true, childDays=365, childMin=98, childMax=99, teenDays=26, teenMin=5, teenMax=15, adultDays=25, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            { "Mantis", new StageProfile { development=Development.Incomplete, childDays=15, childMin=98, childMax=99, teenDays=30, teenMin=70, teenMax=80, adultDays=60, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            { "Grasshopper", new StageProfile { development=Development.Incomplete, childDays=14, childMin=98, childMax=99, teenDays=25, teenMin=70, teenMax=80, adultDays=50, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            // Abeja de las nieves (abeja melífera): huevo 3 d + larva 6 d (la alimentan las nodrizas; Hive.cs) → pupa 12 d →
            // obrera. En frío viven como «abejas de invierno»: varios meses (verano ≈1 mes). doi 10.1242/jeb.247314
            { "SnowBee", new StageProfile { development=Development.Complete, childDays=9, childMin=98, childMax=99, teenDays=12, teenMin=5, teenMax=15, adultDays=150, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // ── Fauna de cada era (docs/microcosmos-fauna-eras.md) ──
            // Escorpión (buthido): vivíparo; las crías viajan sobre la madre hasta la 1.ª muda (~8–15 d, sin comer) → 5–6 mudas;
            // madura en ~8–10 meses y vive ~3 años (Centruroides gracilis en laboratorio: Francke & Jones 1982, J. Arachnol. 10:223)
            { "Scorpion", new StageProfile { childDays=12, childMin=98, childMax=99, teenDays=290, teenMin=70, teenMax=80, adultDays=1100, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // Solífugo: huevos en la madriguera; la postembrión no come; ~8–10 mudas; vive ≈1 año, muy estacional [A]
            { "CamelSpider", new StageProfile { childDays=20, childMin=98, childMax=99, teenDays=180, teenMin=70, teenMax=80, adultDays=150, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            // Ácaro de terciopelo: la LARVA (6 patas) se agarra a un saltamontes o langosta ~1–2 sem; las ninfas (con dos
            // fases inmóviles, calyptostasis) son depredadoras como el adulto, que vive ~1 año y sale tras las lluvias [A]
            { "VelvetMite", new StageProfile { development=Development.Larval, larvaForages=true, childDays=10, childMin=98, childMax=99, teenDays=120, teenMin=70, teenMax=80, adultDays=365, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            // Cochinilla (Porcellio scaber): la manca sale del marsupio (~3–5 sem de incubación) y gana el 7.º par tras la
            // 1.ª muda (~2–3 sem); madura a los ~15 meses (Armadillidium ~25) y vive hasta 30–41 meses (scaber.org)
            { "Woodlouse", new StageProfile { childDays=20, childMin=98, childMax=99, teenDays=430, teenMin=70, teenMax=80, adultDays=450, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // Cangrejo fantasma (Ocypode quadrata): 5 zoeas en el plancton + megalopa (≥35 d), madura al año y vive ~3 años;
            // hiberna en la madriguera de octubre a abril (Animal Diversity Web)
            { "GhostCrab", new StageProfile { development=Development.Larval, larvaForages=true, childDays=30, childMin=98, childMax=99, teenDays=330, teenMin=70, teenMax=80, adultDays=730, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // Caracol (Helix lucorum / Cepaea nemoralis): eclosiona a los 15–20 d; joven sin labio 1–3 años; vive ~2–6 años (ADW)
            { "Snail", new StageProfile { childDays=30, childMin=98, childMax=99, teenDays=700, teenMin=70, teenMax=80, adultDays=1100, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            // Babosa negra (Arion ater): eclosiona a las ~6 sem, naranja → pajiza → negra; anual: pone y muere (ADW) [A]
            { "Slug", new StageProfile { childDays=40, childMin=98, childMax=99, teenDays=200, teenMin=70, teenMax=80, adultDays=130, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            // Lombriz (Lumbricus terrestris): sale del capullo (1 huevo, 5–10 al año) tras semanas–meses; sin clitelo hasta
            // madurar (~1 año); vive 4–8 años (Vigie-Nature École)
            { "Earthworm", new StageProfile { childDays=60, childMin=98, childMax=99, teenDays=300, teenMin=70, teenMax=80, adultDays=2200, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // Cigarra (Cicada orni): huevos en ramitas (6–10 sem), la ninfa (4 estadios) varios años bajo tierra con la savia
            // de las raíces (sin Wander: en su galería); el adulto vive ~6 semanas (Wikipedia; Clemson HGIC)
            { "Cicada", new StageProfile { development=Development.Incomplete, childDays=365, childMin=98, childMax=99, teenDays=730, teenMin=70, teenMax=80, adultDays=42, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            // Luciérnaga (Luciola): la larva, cazadora de caracoles y luminosa, ~1 año; pupa ~1–2 sem; el adulto, 1–3 sem [A]
            { "Firefly", new StageProfile { development=Development.Complete, larvaForages=true, childDays=300, childMin=98, childMax=99, teenDays=10, teenMin=70, teenMax=80, adultDays=18, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            // Tijereta (Forficula auricularia): la madre cuida los huevos y la 1.ª ninfa en el nido; 4 estadios (~1–2 meses);
            // el adulto (la hembra inverna) vive casi un año (Wikipedia)
            { "Earwig", new StageProfile { development=Development.Incomplete, childDays=15, childMin=98, childMax=99, teenDays=45, teenMin=70, teenMax=80, adultDays=300, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // Cucaracha oriental: ooteca de ~16 huevos (42–81 d); ninfa 164–542 d; el adulto vive 160–180 d (UF/IFAS)
            { "Cockroach", new StageProfile { development=Development.Incomplete, childDays=60, childMin=98, childMax=99, teenDays=400, teenMin=70, teenMax=80, adultDays=170, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // Boreus hyemalis: la larva come musgo ~1–2 años; pupa en el suelo en septiembre; el adulto sale sobre la nieve
            // de octubre a marzo (Wikipedia) [A]
            { "Scorpionfly", new StageProfile { development=Development.Complete, larvaForages=true, childDays=540, childMin=98, childMax=99, teenDays=30, teenMin=70, teenMax=80, adultDays=150, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            { "Meerkat", new StageProfile { childDays=20, childMin=98, childMax=99, teenDays=40, teenMin=70, teenMax=80, adultDays=180, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
        };
    }
}
