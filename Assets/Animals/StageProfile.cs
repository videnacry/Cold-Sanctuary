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
/// </summary>
public class StageProfile
{
    public enum Development : byte { Direct, Incomplete, Complete }
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
            { "Meerkat", new StageProfile { childDays=20, childMin=98, childMax=99, teenDays=40, teenMin=70, teenMax=80, adultDays=180, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
        };
    }
}
