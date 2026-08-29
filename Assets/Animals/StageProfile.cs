using System.Collections.Generic;

/// <summary>
/// Perfil de CICLO DE VIDA por especie (docs/anima-dissolving-animal.md, etapa 5): duraciones/escala de las 3 etapas
/// (Childhood/Adolescence/Adulthood) + las listas de EVENTS por etapa (que varían: las territoriales llevan HomeBound;
/// los adultos, Feed). Antes eran campos por clase; ahora DATA (valores extraídos 1:1 por script). `Animal` crea las
/// etapas en `Init` desde aquí (cada ser las suyas, porque se mutan: sizePotential). Los PREPS son default de Animal.
/// </summary>
public class StageProfile
{
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
            { "Wolf", new StageProfile { childDays=77, childMin=98, childMax=99, teenDays=730, teenMin=70, teenMax=78, adultDays=3285, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            // Insectos (días de juego; ciclos cortos para ver lifecycle completo en sesión)
            { "Ant", new StageProfile { childDays=10, childMin=98, childMax=99, teenDays=20, teenMin=70, teenMax=80, adultDays=90, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed } } },
            { "Aphid", new StageProfile { childDays=3, childMin=70, childMax=90, teenDays=5, teenMin=50, teenMax=80, adultDays=10, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            { "Ladybug", new StageProfile { childDays=10, childMin=98, childMax=99, teenDays=14, teenMin=70, teenMax=80, adultDays=90, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            { "Spider", new StageProfile { childDays=20, childMin=98, childMax=99, teenDays=60, teenMin=70, teenMax=80, adultDays=180, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
            { "Cricket", new StageProfile { childDays=14, childMin=98, childMax=99, teenDays=30, teenMin=70, teenMax=80, adultDays=60, adultMin=0, adultMax=20,
                childEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, teenEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest }, adultEvents = new byte[] { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.Feed } } },
        };
    }
}
