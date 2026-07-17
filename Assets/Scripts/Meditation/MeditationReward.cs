using UnityEngine;

/// <summary>
/// Reusable reward block for meditation / plano-mágico missions (docs §7).
///
/// Serializable (not a component) so any mission can embed one and tune it in the Inspector.
/// Wires into the concrete systems that already exist:
///   - Asana mastery: visualization counts as practice → Asana.RegisterPractice().
///   - Observation: contemplative practice permanently grows PlayerStats.observationRadius
///     (observation-system.md: "practicar el modo contemplativo sube la stat por sí solo").
///   - Coins: optional, via CoinWallet.
/// </summary>
[System.Serializable]
public class MeditationReward
{
    [Header("Asana mastery")]
    [Tooltip("The visualized posture. Each practice moves it toward the next mastery tick " +
             "(Asana.PracticesPerMasteryTick). Leave null for non-posture missions.")]
    public Asana asanaToPractice;

    [Tooltip("How many practices this mission grants on completion.")]
    [Min(0)] public int practices = 1;

    [Header("Observation")]
    [Tooltip("Permanent increase to PlayerStats.observationRadius. 0 = none.")]
    public float observationGain = 0.25f;

    [Header("Coins")]
    [Tooltip("Coins awarded on completion. 0 = none.")]
    [Min(0)] public int coinReward = 0;

    /// <summary>Apply every reward. <paramref name="player"/> is the player's transform (may be null).</summary>
    public void Apply(Transform player)
    {
        if (asanaToPractice != null && practices > 0)
        {
            int before = asanaToPractice.masteryLevel;
            for (int i = 0; i < practices; i++) asanaToPractice.RegisterPractice();
            Debug.Log($"[Recompensa] «{asanaToPractice.displayName}»: práctica x{practices} " +
                      $"→ dominio {before} → {asanaToPractice.masteryLevel}.");
        }

        if (observationGain != 0f && player != null)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.observationRadius += observationGain;
                Debug.Log($"[Recompensa] Observación +{observationGain} → {stats.observationRadius:0.00}.");
            }
        }

        if (coinReward > 0)
            CoinWallet.Instance?.Earn(coinReward);
    }
}
