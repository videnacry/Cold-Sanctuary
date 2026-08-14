using UnityEngine;

/// <summary>
/// DINÁMICA de humores/estrés (docs/soul-relations-reincarnation §2b). El estrés (**cortisol**) es un resultado
/// **químico** que **SUBE cuando baja el estado**: fatiga (mucho trabajo/de pie), sueño, hambre y reservas bajas
/// (glucosa/minerales gastados haciendo fuerza); **BAJA** descansado/saciado. La `serotonina` cede ante el
/// cortisol sostenido. `Anima.stress` **refleja** el cortisol. Así los **estados de ánimo emergen del estado** —
/// el mal humor de Goluis aparece **cuando está cansado o con prisa**, no es un rasgo fijo. La `sensibilidad`
/// gobierna cuánto oscila. Opt-in; corre sobre **cualquier `Anima`**: con `Mind` usa los humores (más rico);
/// **sin `Mind` (p. ej. el jugador)** deriva el estrés directamente del estado (fatiga/sueño/hambre). La fatiga
/// es la **universal** (`Anima.mentalFatigue`) o la de `MoodState`, la mayor. Lee `Metabolism` si está.
/// </summary>
public class MoodDynamics : MonoBehaviour
{
    public Anima anima;
    [Min(0.1f)] public float tick = 0.5f;
    [Tooltip("Velocidad de ajuste del cortisol hacia su objetivo (× sensibilidad).")]
    public float driftRate = 0.4f;

    [Header("Pesos de las causas de estrés")]
    public float baseStress = 0.15f;
    public float wFatigue = 0.30f;
    public float wSleep = 0.25f;
    public float wHunger = 0.20f;
    public float wLowGlucose = 0.20f;
    public float wLowMinerals = 0.15f;

    float _next;
    Mind _mind;
    MoodState _mood;
    Metabolism _metab;

    void Awake()
    {
        if (anima == null) anima = GetComponent<Anima>();
        _mind = GetComponent<Mind>();
        _mood = GetComponent<MoodState>();
        _metab = GetComponent<Metabolism>();
    }

    void Update()
    {
        if (anima == null || Time.time < _next) return;
        float dt = tick;
        _next = Time.time + tick;

        Humores h = _mind != null ? _mind.humores : null;
        float fatigue = Mathf.Max(Mathf.Clamp01(anima.mentalFatigue), _mood != null ? _mood.fatigue : 0f);
        float hunger = _metab != null ? _metab.Appetite : 0f;
        float glucoseLack = h != null ? (1f - h.glucosa) : 0f;
        float mineralLack = h != null ? (1f - h.calcio) : 0f;

        float target = Mathf.Clamp01(
            baseStress
            + wFatigue * fatigue
            + wSleep * Mathf.Clamp01(anima.sleepiness)
            + wHunger * hunger
            + wLowGlucose * glucoseLack
            + wLowMinerals * mineralLack);

        float rate = driftRate * Mathf.Max(0.2f, anima.sensibilidad) * dt;   // sensibilidad = reactividad emocional
        if (h != null)
        {
            h.cortisol = Mathf.MoveTowards(h.cortisol, target, rate);
            h.serotonina = Mathf.MoveTowards(h.serotonina, Mathf.Clamp01(0.6f - target * 0.4f), rate * 0.5f);
            anima.stress = Mathf.MoveTowards(anima.stress, h.cortisol, rate);   // con Mind: el stress refleja el cortisol
        }
        else
        {
            anima.stress = Mathf.MoveTowards(anima.stress, target, rate);       // sin Mind (jugador): estrés directo del estado
        }
    }
}
