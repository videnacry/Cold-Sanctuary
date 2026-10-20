using UnityEngine;

/// <summary>
/// PLANTILLA DE CUERPO por especie (el "template" sobre el que la vida va escribiendo). Humanos y animales comparten el
/// mismo modelo: un cuerpo con masa, metabolismo basal (Kleiber: gasto ∝ masa^0,75) y presión de sueño. La composición
/// que se adquiere (células de mago, entrenamiento…) actúa como MODIFICADOR, no como caso especial.
/// </summary>
[System.Serializable]
public class BodyTemplate
{
    public string species = "Human";
    public float adultMassKg = 65f;
    [Tooltip("Horas de juego despierto (sin comer) para pasar de saciado a hambre plena, para la masa adulta.")]
    public float hoursToHungry = 7f;
    [Tooltip("Horas despierto para llegar a sueño pleno / horas dormido para recuperarse.")]
    public float hoursAwakeToSleepy = 16f, hoursAsleepToRested = 8f;
    [Tooltip("Hora (0–24) en que más pesa el sueño circadiano (diurno ≈ 3 h).")]
    public float sleepiestHour = 3f;

    static readonly BodyTemplate Human = new BodyTemplate();

    /// <summary>Plantilla por especie (hoy: humano; el resto de especies migran por etapas, ver docs/body-metabolism).</summary>
    public static BodyTemplate Of(string species) => Human;
}

/// <summary>Capa CELULAR / "desde la raíz": algo que cambia cuánto pide el cuerpo (hambre/sueño) y a qué coste.</summary>
public interface IMetabolicModifier
{
    /// <summary>Multiplicador de las necesidades (1 = normal, 0 = no necesita comer ni dormir).</summary>
    float NeedsScale { get; }
}

/// <summary>
/// METABOLISMO DEL CUERPO para todo anima que no es un `Animal` (humanos y compañeros, la Magnate, Linduarda…): genera
/// HAMBRE (déficit de energía al ritmo de su metabolismo basal, Kleiber: masa^0,75) y SUEÑO (presión que sube despierto,
/// baja dormido, más un empujón circadiano de madrugada). Antes estos seres no tenían ninguna necesidad: el cuerpo
/// "existía" solo para los animales. Los modificadores (células de mago) escalan las necesidades — así la Magnate no
/// tiene hambre ni sueño por su composición, no por una excepción en el código. Los animales siguen con su vía propia
/// (hungry/ActionPrep/SleepCycle) hasta que migren por etapas.
/// </summary>
public class BodyMetabolism : MonoBehaviour
{
    public string species = "Human";
    [Range(0f, 1f)] public float hunger;   // 0 = saciado, 1 = hambre plena
    [Range(0f, 1f)] public float toilet;   // ganas de ir al baño: suben con el tiempo y con cada comida (≈ cada 5–6 h)

    Anima _a;
    BodyTemplate _t;
    IMetabolicModifier[] _mods;

    void Awake()
    {
        _a = GetComponent<Anima>();
        _t = BodyTemplate.Of(species);
        _mods = GetComponents<IMetabolicModifier>();
        if (_a is Animal) enabled = false;   // fauna: vía legacy por ahora
    }

    void Update()
    {
        var tc = TimeController.timeController;
        float minuteSecs = tc != null ? tc.TimeSpeedMinuteSecs : 60f;
        Step(Time.deltaTime / Mathf.Max(0.0001f, minuteSecs) / 60f, tc != null ? tc.Hour : 12f);
    }

    /// <summary>Factor de necesidades combinado de todos los modificadores (células).</summary>
    public float NeedsScale
    {
        get
        {
            if (_mods == null) _mods = GetComponents<IMetabolicModifier>();
            float s = 1f;
            foreach (IMetabolicModifier m in _mods) if (m != null) s *= Mathf.Clamp01(m.NeedsScale);
            return s;
        }
    }

    /// <summary>Avanza <paramref name="gameHours"/> horas de juego (determinista; lo usan los tests).</summary>
    public void Step(float gameHours, float hourOfDay)
    {
        if (_a == null) _a = GetComponent<Anima>();
        if (_t == null) _t = BodyTemplate.Of(species);
        if (_a == null || _a.death || gameHours <= 0f) return;
        float needs = NeedsScale;

        // HAMBRE: gasto basal relativo a la masa adulta (Kleiber) → más masa, más gasto absoluto pero menos por kg.
        float massKg = Mathf.Max(1f, _a.bodyMass * _t.adultMassKg);
        float kleiber = Mathf.Pow(massKg / _t.adultMassKg, 0.75f);
        float awakeFactor = _a.asleep ? 0.6f : 1f;   // dormido se gasta menos
        hunger = Mathf.Clamp01(hunger + gameHours / _t.hoursToHungry * kleiber * awakeFactor * needs);
        toilet = Mathf.Clamp01(toilet + gameHours / 6f * awakeFactor * needs);

        // SUEÑO: presión homeostática (sube despierto, baja dormido) + empuje circadiano alrededor de sleepiestHour.
        if (_a.asleep) _a.sleepiness = Mathf.Clamp01(_a.sleepiness - gameHours / _t.hoursAsleepToRested);
        else
        {
            float d = Mathf.Abs(Mathf.DeltaAngle(hourOfDay * 15f, _t.sleepiestHour * 15f)) / 180f;   // 0 = madrugada
            float circadian = Mathf.Lerp(1.5f, 0.6f, d);
            _a.sleepiness = Mathf.Clamp01(_a.sleepiness + gameHours / _t.hoursAwakeToSleepy * circadian * needs);
        }
    }

    /// <summary>Comer: baja el hambre según lo nutritivo de la comida (0..1 = fracción de una comida completa).</summary>
    public void Feed(float meal) => hunger = Mathf.Clamp01(hunger - Mathf.Max(0f, meal));
}

/// <summary>
/// CÉLULAS DE MAGO (docs/magic-metabolism-progression.md): la fracción de células sostenidas por magia no pide comida ni
/// sueño; ese mantenimiento lo paga la reserva mágica. Sin energía mágica, la fracción deja de ayudar (vuelven el
/// hambre y el sueño). La Magnate es fracción 1 con reserva inagotable.
/// </summary>
public class MageCells : MonoBehaviour, IMetabolicModifier
{
    [Range(0f, 1f)] public float fraction = 1f;
    [Tooltip("Sin MagicReserves se considera que la reserva no se agota (p. ej. la Magnate).")]
    public float upkeepEnergyPerHour = 1f;

    MagicReserves _res;
    void Awake() => _res = GetComponent<MagicReserves>();

    public float NeedsScale
    {
        get
        {
            if (_res == null) return 1f - fraction;
            return _res.energy > upkeepEnergyPerHour ? 1f - fraction : 1f;   // sin magia, el cuerpo vuelve a pedir
        }
    }
}
