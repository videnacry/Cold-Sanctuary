using UnityEngine;

/// <summary>
/// El Papi Gohageneis — celebración, arte de vivir, restauración emocional. Comportamiento propio (sobre
/// `MoodState`); stats desde el arquetipo "Gohageneis". Barra de celebración que, al llenarse por cercanía,
/// dispara un burst de Satisfacción + alivio de fatiga. Ver docs/creature-stats.md.
/// </summary>
public class Gohageneis : MonoBehaviour
{
    [Header("Gohageneis — Celebration Charge")]
    [Range(0f, 1f)] public float celebrationCharge;
    public float chargeRate = 0.05f;
    public float burstAmount = 0.15f;
    public float burstFatigueRelief = 0.08f;

    MoodState _mood;

    void Awake() { _mood = GetComponent<MoodState>(); }

    void Update()
    {
        if (_mood == null) return;

        if (_mood.PlayerInRange && _mood.PlayerMind != null)
        {
            celebrationCharge += chargeRate * Time.deltaTime;
            if (celebrationCharge >= 1f) { TriggerCelebrationBurst(); celebrationCharge = 0f; }
        }

        if (_mood.bondWithPlayer > 70f)
            _mood.ShiftAnchor("hide_pain", 0.2f, Time.deltaTime);   // deja entrar al jugador
    }

    void TriggerCelebrationBurst()
    {
        if (_mood.PlayerMind == null) return;
        _mood.PlayerMind.RestoreMind(burstAmount, MindChannel.Satisfaction);
        _mood.PlayerMind.RestoreMind(burstFatigueRelief, MindChannel.MentalFatigue);
        Debug.Log($"[Gohageneis] ¡Burst de celebración! Satisfacción +{burstAmount} | Fatiga −{burstFatigueRelief}");
    }
}
