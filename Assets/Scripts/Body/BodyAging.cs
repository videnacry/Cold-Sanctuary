using UnityEngine;

/// <summary>
/// EDAD COMO EFECTO DE LOS STATS (etapa 3 del cuerpo unificado): no hay días fijos por etapa. El cuerpo lleva
/// CRECIMIENTO (0..1, sube comiendo bien y descansando) y DESGASTE (0..1, sube con el gasto metabólico y con la carga
/// alostática —estrés, enfermedad, hambre extrema—; dormir repara un poco). La etapa sale de ahí:
///   crecimiento &lt; 0,6 → niño · &lt; 0,95 → joven · si no adulto · desgaste ≥ 0,7 → anciano · desgaste ≥ 1 → muere.
/// Las CÉLULAS DE MAGO frenan el desgaste igual que las necesidades (la Magnate casi no envejece). El tamaño visible
/// sigue al crecimiento. Para humanos y compañeros; los animales migrarán por etapas desde StageProfile.
/// </summary>
[RequireComponent(typeof(BodyMetabolism))]
public class BodyAging : MonoBehaviour
{
    [Range(0f, 1f)] public float growth = 1f;   // 1 = adulto formado
    [Range(0f, 1f)] public float wear = 0.2f;   // 0 = recién nacido, 1 = fin de la vida

    [Tooltip("Días de juego de vida media con carga normal (calibra el desgaste).")]
    public float lifespanDays = 3650f;
    [Tooltip("Días de juego para crecer de recién nacido a adulto bien alimentado.")]
    public float growDays = 600f;
    [Tooltip("Escala visual del recién nacido respecto al adulto.")]
    public float birthScale = 0.45f;

    public const float Maturity = 0.6f, Adult = 0.95f, Elder = 0.7f;

    Anima _a;
    BodyMetabolism _body;
    Vector3 _adultScale;
    char _lastStage;
    bool _stageKnown;

    void Awake()
    {
        _a = GetComponent<Anima>();
        _body = GetComponent<BodyMetabolism>();
        _adultScale = transform.localScale;
        ApplyScale();
    }

    void Update()
    {
        var tc = TimeController.timeController;
        float minuteSecs = tc != null ? tc.TimeSpeedMinuteSecs : 60f;
        Step(Time.deltaTime / Mathf.Max(0.0001f, minuteSecs) / 60f);
    }

    /// <summary>Etapa derivada del cuerpo (constantes de LifeStage: c/t/a; s = fin).</summary>
    public char Stage => wear >= 1f ? LifeStage.soul : growth < Maturity ? LifeStage.child : growth < Adult ? LifeStage.teen : LifeStage.adult;
    public bool IsElder => wear >= Elder && wear < 1f;

    /// <summary>Avanza <paramref name="gameHours"/> horas (determinista; lo usan los tests).</summary>
    public void Step(float gameHours)
    {
        if (_a == null) _a = GetComponent<Anima>();
        if (_body == null) _body = GetComponent<BodyMetabolism>();
        if (_a == null || _a.death || gameHours <= 0f) return;
        if (!_stageKnown) { _lastStage = Stage; _stageKnown = true; }   // la etapa de partida no es un «cambio»
        float days = gameHours / 24f;
        float cells = _body != null ? _body.NeedsScale : 1f;   // células de mago: 0 = congeladas en el tiempo

        // CRECER: solo con el cuerpo nutrido (poca hambre) y más deprisa durmiendo.
        if (growth < 1f)
        {
            float fed = _body != null ? Mathf.Clamp01(1f - _body.hunger) : 1f;
            growth = Mathf.Clamp01(growth + days / growDays * fed * (_a.asleep ? 1.3f : 1f));
            ApplyScale();
        }

        // DESGASTE: base metabólica + carga alostática (estrés, enfermedad, hambre extrema); dormir repara.
        float load = 1f + 1.5f * _a.stress + 2f * _a.sickness + (_body != null && _body.hunger > 0.9f ? 1f : 0f);
        float repair = _a.asleep ? 0.3f : 0f;
        wear = Mathf.Clamp01(wear + days / lifespanDays * Mathf.Max(0f, load * cells - repair));

        char st = Stage;
        if (st != _lastStage)
        {
            Debug.Log($"[Vida] «{name}» pasa a {Describe(st)} (crecimiento {growth:0.00}, desgaste {wear:0.00}).");
            _lastStage = st;
        }
        if (st == LifeStage.soul) { _a.death = true; Debug.Log($"[Vida] «{name}» muere de vieja (desgaste 1)."); }
    }

    void ApplyScale()
    {
        float s = Mathf.Lerp(birthScale, 1f, growth);
        if (_adultScale == Vector3.zero) _adultScale = transform.localScale;
        transform.localScale = _adultScale * s;
    }

    static string Describe(char st) =>
        st == LifeStage.child ? "niño" : st == LifeStage.teen ? "joven" : st == LifeStage.adult ? "adulto" : "el final de su vida";
}
