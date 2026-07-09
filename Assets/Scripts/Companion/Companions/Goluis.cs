using UnityEngine;

/// <summary>
/// El Maestro Goluis — cocina, doble turno, presión y resistencia.
/// Primary channel: MentalFatigue (su presión sube fatiga, pero con el tiempo
/// te endurece y baja tu límite de fatiga base).
/// Anchor clave: "yoga_skepticism" — bloquea aprendizaje de asanas hasta que el arco lo suaviza.
/// </summary>
public class Goluis : CompanionBase
{
    [Header("Goluis — Dialogue")]
    [Tooltip("Lines Goluis says the first time Kushal enters his range.")]
    public DialogueSequence greetingSequence;

    [Tooltip("Lines Goluis says when pressure is active and Kushal is nearby.")]
    public DialogueSequence pressureSequence;

    [Header("Goluis — Pressure System")]
    [Tooltip("When active, adds a speed bonus comment — raises player stress but also mental resistance.")]
    public bool pressureActive;

    [Tooltip("Stress applied to the player per second when pressure is active.")]
    public float pressureStressRate = 0.005f;

    [Tooltip("Resistance bonus accumulated from surviving Goluis pressure (0–1).")]
    [Range(0f, 1f)] public float resistanceBuilt;

    protected override void SetupAnchors()
    {
        anchors.Add(new ThoughtAnchor("yoga_skepticism",  -0.9f, 0.002f));  // blocks yoga
        anchors.Add(new ThoughtAnchor("work_hard",         0.85f, 0.001f)); // drives double shift
        anchors.Add(new ThoughtAnchor("family_first",      0.95f, 0.0f));   // fixed — doesn't change
        anchors.Add(new ThoughtAnchor("trust_people",     -0.4f,  0.005f)); // starts low, grows with bond
    }

    protected override float GetMoodModifier()
    {
        // Goluis's nature: when he's in good mood, still reserved — modifier capped at 0.7
        // When in bad mood, he can drain the player slightly (negative modifier possible)
        return Mathf.Lerp(-0.3f, 0.7f, mood);
    }

    protected override float FatigueRatePerSecond()
    {
        // Double shift — tires faster than average companions
        return 0.0003f;
    }

    protected override float GetRestingMood()
    {
        // Goluis defaults to a neutral-to-low mood; bond improves this
        return Mathf.Lerp(0.3f, 0.65f, bondWithPlayer / 100f);
    }

    protected override void Update()
    {
        base.Update();

        if (pressureActive && _playerMind != null)
        {
            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist <= proximityRadius)
            {
                _playerMind.DrainMind(pressureStressRate * Time.deltaTime, MindChannel.Stress);
                resistanceBuilt = Mathf.Clamp01(resistanceBuilt + 0.00001f * Time.deltaTime);
            }
        }

        // Arc: if bond is high enough, soften yoga_skepticism over time
        if (bondWithPlayer > 60f)
            ShiftAnchor("yoga_skepticism", -0.2f, Time.deltaTime); // slowly opens up
    }

    protected override void OnPlayerNearby()
    {
        if (DialogueManager.Instance == null || DialogueManager.Instance.IsPlaying) return;

        if (pressureActive && pressureSequence != null)
            DialogueManager.Instance.Play(pressureSequence);
        else if (greetingSequence != null)
            DialogueManager.Instance.Play(greetingSequence);
    }
}
