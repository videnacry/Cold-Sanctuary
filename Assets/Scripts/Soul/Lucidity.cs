using UnityEngine;

/// <summary>
/// LUCIDEZ de los insectos con mente humana (los entomántropos: mirmidones, arañas, abejas…). Viven entre dos mundos:
/// - **Lúcido** (en calma: sin estrés, saciado, sin alerta, sin correr): la mente manda. El cuerpo se ENDEREZA como un
///   centauro: camina sobre las patas de atrás, con la cabeza alta y el primer par libre como **brazos** (hormiga: 4
///   patas + 2 brazos; araña: 6 + 2). Piensa más a menudo.
/// - **Animal** (estrés, hambre, amenaza, huida o carrera; o `AcuteStressResponse.InTakeover`): el cuerpo toma el
///   control. Baja al suelo y usa TODAS las patas (hormiga 6, araña 8), y piensa menos.
/// Con histéresis, para no parpadear entre los dos. Larvas, pupas y dormidos nunca están lúcidos (no tienen postura
/// erguida). El cuerpo de cubos lee <see cref="Lucid"/> y <see cref="LegsOnGround"/> para la postura (eso lo dibuja
/// `CubeInsect`). Auto-añadido en `Animal.Init` a los artrópodos, junto con la inteligencia humana
/// (<see cref="GiveHumanIntellect"/>). Ver docs/insect-biology-audit.md.
/// </summary>
[RequireComponent(typeof(Animal))]
public class Lucidity : MonoBehaviour
{
    [Tooltip("Claridad (0..1) a partir de la cual se endereza.")]
    [Range(0f, 1f)] public float enterAt = 0.6f;
    [Tooltip("Claridad por debajo de la cual el cuerpo vuelve a mandar.")]
    [Range(0f, 1f)] public float leaveAt = 0.4f;
    [Tooltip("Velocidad (× la de andar de su especie) que ya cuenta como correr: corre a cuatro… o a seis patas.")]
    [Min(1f)] public float runFactor = 1.3f;
    [Tooltip("Lúcido piensa más a menudo (× thinkInterval); en modo animal, menos.")]
    [Min(0.1f)] public float lucidThinkFactor = 0.6f, animalThinkFactor = 1.5f;

    /// <summary>0..1: cuánta calma tiene la mente ahora (1 − el apremio más fuerte).</summary>
    public float Clarity { get; private set; }
    /// <summary>Erguido, pensativo, con brazos (true) o a cuatro patas… o a seis, como un animal (false).</summary>
    public bool Lucid { get; private set; }
    /// <summary>Patas que tocan el suelo ahora: todas en modo animal; dos menos (los brazos) en modo lúcido.</summary>
    public int LegsOnGround => Lucid ? Mathf.Max(2, Limbs - 2) : Limbs;
    /// <summary>Patas de la especie (artrópodos: 6; arañas: 8).</summary>
    public int Limbs { get; private set; } = 6;

    Animal _a;
    AcuteStressResponse _acute;
    Mind _mind;
    float _baseThink = -1f;

    void Awake()
    {
        _a = GetComponent<Animal>();
        _acute = GetComponent<AcuteStressResponse>();
        _mind = GetComponent<Mind>();
        if (_mind != null) _baseThink = _mind.thinkInterval;
    }

    void Start() { if (_a != null) Limbs = LimbsOf(_a.SpeciesName); }

    void Update()
    {
        if (_a == null || _a.death) return;
        Clarity = ClarityOf(_a);
        bool canStand = !_a.asleep && Metamorphosis.PhaseOf(_a) != Metamorphosis.Phase.Larva && !Metamorphosis.IsPupa(_a);
        bool bodyRules = !canStand || _a.aware || Running() || (_acute != null && _acute.InTakeover);
        if (bodyRules || Clarity < leaveAt) Set(false);
        else if (Clarity >= enterAt) Set(true);
    }

    void Set(bool lucid)
    {
        if (lucid == Lucid) return;
        Lucid = lucid;
        if (_mind != null && _baseThink > 0f) _mind.thinkInterval = _baseThink * (lucid ? lucidThinkFactor : animalThinkFactor);
    }

    bool Running()
    {
        if (_a.nav == null || !_a.nav.isOnNavMesh || _a.ActsPrep == null || _a.ActsPrep.walk == null) return false;
        return _a.nav.velocity.magnitude > _a.ActsPrep.walk.navSpeed * runFactor;
    }

    /// <summary>Calma de la mente: 1 − el apremio más fuerte (estrés, hambre, alerta, niebla cognitiva).</summary>
    public static float ClarityOf(Animal a)
    {
        if (a == null) return 0f;
        float urge = Mathf.Max(a.stress, Mathf.Clamp01(a.hungry), a.alertness, a.mindStatusCalculationFog);
        return 1f - Mathf.Clamp01(urge);
    }

    /// <summary>Artrópodos del juego (los que pueden tener mente humana y postura de centauro).</summary>
    public static bool IsArthropod(string species)
    {
        switch (species)
        {
            case "Ant": case "Aphid": case "Ladybug": case "Spider": case "SnowSpider": case "Cricket": case "Termite":
            case "DungBeetle": case "Grasshopper": case "Antlion": case "Mantis": case "SnowBee":
                return true;
            default: return false;
        }
    }

    public static int LimbsOf(string species) => species == "Spider" || species == "SnowSpider" ? 8 : 6;

    /// <summary>INTELIGENCIA HUMANA: razonamiento, memoria y creatividad suben al menos al nivel del arquetipo humano.
    /// El temperamento (calma, sociabilidad, disciplina) y el tono siguen siendo los de su especie: una araña lista
    /// sigue siendo paciente y solitaria; una hormiga lista, disciplinada y social.</summary>
    public static void GiveHumanIntellect(Anima a)
    {
        if (a == null) return;
        Aptitudes h = Archetypes.MindOf("Human").aptitudes;
        a.reasoning  = Mathf.Max(a.reasoning,  h.reasoning);
        a.memory     = Mathf.Max(a.memory,     h.memory);
        a.creativity = Mathf.Max(a.creativity, h.creativity);
    }
}
