using UnityEngine;

/// <summary>
/// Aura mágica de un `Anima` (docs/stats-as-truth.md §2): un contador **firmado** que **decae** con el tiempo.
/// **+** = benevolente/inspirador → **bonds más fáciles** (lo lee `Anima.GrowBond`); **−** = destructivo →
/// **más temido** (lo lee `Animal.EvaluateThreat`). El **sistema de magia** llama a `RegisterDestructiveUse`/
/// `RegisterBenevolentUse` al lanzar hechizos; aquí solo se decae hacia 0 (la fama se olvida).
/// </summary>
public class MagicAura : MonoBehaviour
{
    public Anima anima;
    [Tooltip("Cuánto se acerca a 0 por segundo (la fama se olvida).")]
    public float decayPerSecond = 0.1f;
    public float min = -1f, max = 1f;

    void Awake() { if (anima == null) anima = GetComponent<Anima>(); }

    void Update()
    {
        if (anima == null) return;
        anima.magicAura = Mathf.MoveTowards(anima.magicAura, 0f, decayPerSecond * Time.deltaTime);
    }

    public void RegisterDestructiveUse(float amount)
    {
        if (anima != null) anima.magicAura = Mathf.Clamp(anima.magicAura - Mathf.Abs(amount), min, max);
    }

    public void RegisterBenevolentUse(float amount)
    {
        if (anima != null) anima.magicAura = Mathf.Clamp(anima.magicAura + Mathf.Abs(amount), min, max);
    }
}
