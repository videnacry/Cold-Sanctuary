using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Depredación por STATS (docs/stats-as-truth.md §2): quién es presa / depredador / amenaza sale de los
/// **stats** (masa/fuerza/textura), no de una tabla fija. Así el **tamaño invierte presa↔depredador** (un
/// conejo del tamaño de Kushal deja de ser presa; una hormiga con stats de ápex a lo grande aterra a los osos,
/// pero sigue siendo presa de las Quimeras del subsuelo). Engancha con la transformación: el **farol**
/// (visual-only) **NO cambia stats** → no engaña aquí; la transformación **real** sí (los demás "huelen" los
/// stats nuevos). Usado por `Diet.SelectPrey` (no cazar lo invencible) y `Animal.EvaluateThreat` (temer al más
/// poderoso).
/// </summary>
public static class Predation
{
    /// <summary>Poder depredador: capacidad de cazar/intimidar (masa/fuerza/textura/agilidad).</summary>
    public static float PredatorPower(Anima a) => a == null ? 0f
        : a.strength * 0.4f + a.bodyMass * 0.4f + a.armadura * 0.15f + a.agility * 0.05f;

    /// <summary>Defensa: lo difícil que es someterlo/comerlo (masa/fuerza/textura).</summary>
    public static float Defense(Anima a) => a == null ? 0f
        : a.bodyMass * 0.5f + a.strength * 0.3f + a.armadura * 0.2f;

    /// <summary>¿Puede el cazador con la presa? Necesita superar su defensa (margen opcional).</summary>
    public static bool CanHunt(Anima hunter, Anima prey, float margin = 0f)
        => hunter != null && prey != null && PredatorPower(hunter) >= Defense(prey) + margin;

    /// <summary>¿'a' teme a 'other'? (other bastante más poderoso).</summary>
    public static bool Fears(Anima a, Anima other, float margin = 0.5f)
        => a != null && other != null && PredatorPower(other) - PredatorPower(a) > margin;

    /// <summary>
    /// Poder EFECTIVO con **manada**: el propio + una fracción del de los **aliados** (misma facción `ITarget`)
    /// dentro del radio. Dinámico (no un multiplicador fijo de dieta): un lobo solo no puede con el oso, pero
    /// una manada sí; y el oso evita al lobo *con manada*.
    /// </summary>
    public static float EffectivePower(Anima self, float radius = 8f)
    {
        if (self == null) return 0f;
        float p = PredatorPower(self);
        ITarget st = self.GetComponent<ITarget>();
        if (st == null) return p;   // sin facción → sin manada
        HashSet<Anima> seen = new HashSet<Anima>();
        foreach (Collider col in Physics.OverlapSphere(self.transform.position, radius))
        {
            Anima a = col.GetComponentInParent<Anima>();
            if (a == null || a == self || !seen.Add(a)) continue;
            ITarget t = a.GetComponent<ITarget>();
            if (t != null && t.Faction == st.Faction) p += PredatorPower(a) * 0.5f;
        }
        return p;
    }
}
