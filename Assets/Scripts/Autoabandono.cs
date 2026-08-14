using UnityEngine;

/// <summary>
/// Deriva el <b>autoabandono</b> (disposición al auto-sacrificio, 0..1) de la tensión <b>entrega ↔
/// autoconservación</b> (docs/soul-relations-reincarnation §2c). No es un número fijo: sale de los stats + bonds.
///   entrega          ← afabilidad + sensibilidad + (bond medio)   (calidez/cooperación/apego)
///   autoconservación ← composure + disciplina + instinto base     (frialdad/control/supervivencia)
///   autoabandono = entrega / (entrega + autoconservación)
/// Lo usan el huir/ayudar-al-pack (`Animal.ResolveReaction`) y `PackAwareness`.
/// </summary>
public static class Autoabandono
{
    public static float From(Anima a)
    {
        if (a == null) return 0.3f;
        float entrega = Mathf.Max(0f, a.afabilidad) + Mathf.Max(0f, a.sensibilidad) + AvgBond(a);
        float conserv = Mathf.Max(0f, a.composure) + Mathf.Max(0f, a.discipline) + 1f;   // +1 = instinto de conservación base
        float total = entrega + conserv;
        return total <= 0f ? 0f : Mathf.Clamp01(entrega / total);
    }

    // Bond medio con todos los seres (0..1), como parte de la "entrega": más lazos → más te entregas.
    static float AvgBond(Anima a)
    {
        if (a.bonds == null || a.bonds.Count == 0) return 0f;
        float s = 0f; int n = 0;
        foreach (Bond b in a.bonds) if (b != null) { s += b.value; n++; }
        return n > 0 ? (s / n) / 100f : 0f;
    }
}
