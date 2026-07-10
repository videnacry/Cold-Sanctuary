using UnityEngine;

/// <summary>
/// El Papi Gohageneis — celebración, arte de vivir, restauración emocional.
/// Primary channel: Satisfaction (su energía llena la barra de Satisfacción del jugador).
/// CelebrationCharge: barra que se llena por cercanía → al alcanzar umbral activa burst de restauración.
/// Anchor clave: "celebrate_life" — su motor principal.
/// </summary>
public class Gohageneis : CompanionBase
{
    [Header("Gohageneis — Celebration Charge")]
    [Tooltip("Current charge level (0–1). Fills while player is nearby.")]
    [Range(0f, 1f)] public float celebrationCharge;

    [Tooltip("Charge fill rate per second while player is in range.")]
    public float chargeRate = 0.05f;

    [Tooltip("Satisfaction restored in a burst when charge reaches 1.")]
    public float burstAmount = 0.15f;

    [Tooltip("Also reduces mental fatigue on burst — the laugh effect.")]
    public float burstFatigueRelief = 0.08f;

    // Aptitudes (1.0 = media real humana). 28 años; salió de casa a los 16 buscando buena vida;
    // nómada Venezuela→Colombia→Perú→Ecuador→España→USA→santuario con trabajos muy variados
    // (mudanzas, ventas, bartender, Uber, cocina, limpieza, construcción). Fiesta, baile, gym, comer.
    protected override float BaseAgility      => 1.2f;   // físicamente equilibrado tirando a ágil
    protected override float BaseStrength     => 1.1f;   // gym + mudanzas/construcción
    protected override float BaseBodyMass     => 1.1f;   // come bien, cuerpo sólido
    protected override float BasePerception   => 1.05f;  // calle/social (bartender/ventas), no académica
    protected override float BaseAdaptability => 1.7f;   // versatilidad extrema: mil trabajos y países
    protected override float BaseComposure    => 1.2f;   // vida de calle/bartender: maneja el caos

    protected override void SetupAnchors()
    {
        anchors.Add(new ThoughtAnchor("celebrate_life",   0.9f,  0.001f));
        anchors.Add(new ThoughtAnchor("include_everyone", 0.85f, 0.001f));
        anchors.Add(new ThoughtAnchor("hide_pain",        0.7f,  0.005f)); // softens as bond grows
        anchors.Add(new ThoughtAnchor("trust_joy",        0.8f,  0.001f));
    }

    protected override float GetMoodModifier()
    {
        // Gohageneis bounces back fast — mood rarely stays low long
        // Even at low mood he still gives something (0.4 floor)
        return Mathf.Lerp(0.4f, 1.2f, mood);  // can exceed 1 when he's in full festive mode
    }

    protected override float FatigueRatePerSecond() => 0.00008f; // high energy — tires very slowly

    protected override float GetRestingMood()
    {
        // Naturally high baseline — drops after being reprimanded, bounces back
        return Mathf.Lerp(0.65f, 0.9f, bondWithPlayer / 100f);
    }

    protected override void Update()
    {
        base.Update();

        if (_playerMind != null)
        {
            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist <= proximityRadius)
            {
                celebrationCharge += chargeRate * Time.deltaTime;

                if (celebrationCharge >= 1f)
                {
                    TriggerCelebrationBurst();
                    celebrationCharge = 0f;
                }
            }
        }

        // Arc: hide_pain softens as bond deepens — he lets the player in
        if (bondWithPlayer > 70f)
            ShiftAnchor("hide_pain", 0.2f, Time.deltaTime);
    }

    void TriggerCelebrationBurst()
    {
        if (_playerMind == null) return;

        _playerMind.RestoreMind(burstAmount,       MindChannel.Satisfaction);
        _playerMind.RestoreMind(burstFatigueRelief, MindChannel.MentalFatigue);

        // Hook: play a sound, particle effect, dialogue line
        // e.g. AudioManager.Instance.Play("gohageneis_laugh");
        Debug.Log($"[Gohageneis] Celebration burst! Satisfaction +{burstAmount} | Fatigue relief +{burstFatigueRelief}");
    }
}
