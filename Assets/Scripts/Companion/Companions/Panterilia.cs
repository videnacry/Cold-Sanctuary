using UnityEngine;

/// <summary>
/// Panterilia — limpieza, nutrición, observación.
/// Primary channel: MentalFatigue (su calma y atención al detalle reducen el desgaste).
/// Secondary: sube Observation del jugador por cercanía (ver ObservationProximityBonus).
/// Anchor clave: "work_hard" — siempre encuentra la manera de avanzar.
/// </summary>
public class Panterilia : CompanionBase
{
    [Header("Panterilia — Observation Bonus")]
    [Tooltip("Observation radius bonus added to the player while Panterilia is nearby.")]
    public float observationRadiusBonus = 1.5f;

    bool _bonusApplied;

    protected override void SetupAnchors()
    {
        anchors.Add(new ThoughtAnchor("work_hard",         0.85f, 0.001f));
        anchors.Add(new ThoughtAnchor("protect_daughter",  0.95f, 0.0f));   // fixed
        anchors.Add(new ThoughtAnchor("trust_nature",      0.3f,  0.01f));  // grows during arc
        anchors.Add(new ThoughtAnchor("chemical_reliance", 0.6f,  0.02f));  // fades during arc
    }

    protected override float GetMoodModifier()
    {
        // Panterilia is steady — even when tired she gives something
        // Modifier stays positive unless stress is very high
        return Mathf.Lerp(0.3f, 1f, mood) * Mathf.Lerp(1f, 0.4f, stress);
    }

    protected override float FatigueRatePerSecond() => 0.00015f; // tires slowly — paced worker

    protected override float GetRestingMood()
    {
        // Stable mood, slightly higher with bond
        return Mathf.Lerp(0.55f, 0.8f, bondWithPlayer / 100f);
    }

    protected override void OnPlayerNearby()
    {
        // Temporarily expand the player's observation radius
        if (_playerStats != null && !_bonusApplied)
        {
            _playerStats.observationRadius += observationRadiusBonus;
            _bonusApplied = true;
        }
    }

    protected override void OnPlayerLeft()
    {
        // Remove the observation bonus when player moves away
        if (_playerStats != null && _bonusApplied)
        {
            _playerStats.observationRadius -= observationRadiusBonus;
            _bonusApplied = false;
        }
    }

    protected override void Update()
    {
        base.Update();

        // Arc: chemical_reliance fades as she spends time at the sanctuary
        ShiftAnchor("chemical_reliance", 0f, Time.deltaTime);
        ShiftAnchor("trust_nature",      1f, Time.deltaTime);
    }
}
