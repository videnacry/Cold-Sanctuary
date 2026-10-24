using UnityEngine;

/// <summary>
/// METAMORFOSIS de los insectos (docs/insect-biology-audit.md). Lee las 3 etapas de `LifeStage` según el desarrollo
/// de la especie (<see cref="StageProfile.development"/>) y da a cada una su conducta real:
/// - **Completa** (holometábolo: hormiga, abeja, mariquita, pelotero, hormiga león): cría = LARVA, juvenil = PUPA,
///   adulto = imago. La pupa **no se mueve, no come y no huye**: este componente la deja quieta (toma el sueño con
///   <see cref="SleepCycle.Suspended"/> y para el NavMesh) hasta que emerge el adulto.
/// - **Incompleta** (grillo, saltamontes, mantis, termita, pulgón): cría y juvenil = NINFAS (sin alas).
/// - **Larvaria** (cangrejo: zoea → megalopa; ácaro de terciopelo: larva de 6 patas → ninfa de 8): cría = LARVA (que
///   se mueve y come, como en el mar o sobre su huésped), juvenil = NINFA (la forma intermedia); sin pupa inmóvil.
/// - **Directa** (mamíferos, arañas, escorpiones, cochinillas, caracoles, lombrices): sin cambio de forma (la cría de
///   cochinilla, la manca, nace con un par de patas menos; el caracol joven, sin labio; la lombriz, sin clitelo).
/// El cuerpo de cubos puede leer <see cref="Current"/> para dibujar larva / pupa / adulto (o ninfa sin alas).
/// Auto-añadido en `Animal.Init` a las especies con metamorfosis.
/// </summary>
[RequireComponent(typeof(Animal))]
public class Metamorphosis : MonoBehaviour
{
    public enum Phase { Juvenile, Nymph, Larva, Pupa, Adult }

    Animal _a;
    SleepCycle _sleep;
    bool _wasPupa;

    void Awake() { _a = GetComponent<Animal>(); _sleep = GetComponent<SleepCycle>(); }

    /// <summary>La fase actual del ser (lo que hay que dibujar).</summary>
    public Phase Current => PhaseOf(_a);

    public static Phase PhaseOf(Animal a)
    {
        if (a == null || a.lifeStage == LifeStage.adult || a.lifeStage == LifeStage.soul) return Phase.Adult;
        switch (StageProfile.Of(a.SpeciesName).development)
        {
            case StageProfile.Development.Complete:   return a.lifeStage == LifeStage.child ? Phase.Larva : Phase.Pupa;
            case StageProfile.Development.Incomplete: return Phase.Nymph;
            case StageProfile.Development.Larval:     return a.lifeStage == LifeStage.child ? Phase.Larva : Phase.Nymph;
            default:                                  return Phase.Juvenile;
        }
    }

    public static bool IsPupa(Animal a) => PhaseOf(a) == Phase.Pupa;

    /// <summary>¿Busca comida por su cuenta en esta etapa? La pupa nunca; la larva solo si la especie lo hace
    /// (`larvaForages`: mariquita, hormiga león); el resto, desde juvenil (la regla de siempre).</summary>
    public static bool CanForage(Animal a)
    {
        if (a == null) return false;
        switch (PhaseOf(a))
        {
            case Phase.Pupa:  return false;
            case Phase.Larva: return StageProfile.Of(a.SpeciesName).larvaForages;
            default:          return a.lifeStage == LifeStage.teen || a.lifeStage == LifeStage.adult;
        }
    }

    void Update()
    {
        if (_a == null || _a.death) return;
        bool pupa = IsPupa(_a);
        if (pupa)
        {
            if (_sleep == null) _sleep = GetComponent<SleepCycle>();
            if (_sleep != null) _sleep.Suspended = true;   // el sueño día/noche no la despierta: está en la crisálida
            _a.asleep = true;                              // quieta: Volition/forrajeo/merodeo ya respetan `asleep`
            if (_a.nav != null && _a.nav.isOnNavMesh) _a.nav.isStopped = true;
        }
        else if (_wasPupa)
        {
            if (_sleep != null) _sleep.Suspended = false;
            _a.asleep = false;
            if (_a.nav != null && _a.nav.isOnNavMesh) _a.nav.isStopped = false;
            Debug.Log($"[Vida] «{_a.name}» sale de la pupa: emerge el adulto.");
        }
        _wasPupa = pupa;
    }
}
