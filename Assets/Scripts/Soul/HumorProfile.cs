using UnityEngine;

/// <summary>
/// Deriva los HUMORES base (adrenalina/serotonina/cortisol/glucosa/calcio) de la **personalidad** (stats) de un
/// ser (docs/soul-relations-reincarnation §2b). Así la actitud no se teclea: un ser muy sociable/creativo y poco
/// disciplinado arranca con adrenalina/serotonina altas (→ "fiesta" vía `SocialField`); uno de baja composure y
/// alta sensibilidad, con más cortisol (tenso). La `sensibilidad` gobierna cuánto OSCILAN luego (no la base).
/// </summary>
public static class HumorProfile
{
    public static void Apply(Anima a, Humores h)
    {
        if (a == null || h == null) return;
        h.adrenalina = Mathf.Clamp01(0.25f + 0.20f * a.sociability + 0.15f * a.creativity + 0.10f * a.agility - 0.15f * a.discipline);
        h.serotonina = Mathf.Clamp01(0.30f + 0.15f * a.afabilidad + 0.15f * a.sociability);
        h.cortisol   = Mathf.Clamp01(0.30f + 0.20f * a.sensibilidad - 0.15f * a.composure);
        h.glucosa    = Mathf.Clamp01(0.40f + 0.15f * a.endurance + 0.10f * a.bodyMass);
        h.calcio     = Mathf.Clamp01(0.40f + 0.10f * a.discipline + 0.10f * a.composure);
    }
}
