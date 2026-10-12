using UnityEngine;

/// <summary>
/// HECHIZO DE ABASTECER (inventario, docs/sanctuary1 §4): abastece una unidad del <see cref="StockShelf"/> más cercano
/// dentro del alcance (`range`, ampliable con magia → a distancias irreales). Como la limpieza: aprendible (`Grimoire`)
/// y enseñable (el de guardia lo demuestra con `TeachSpell`). Misión de inventario = llenar los estantes que lo necesiten.
/// </summary>
public class StockSpell : SpellBase
{
    Anima _self;
    void Awake() { _self = GetComponent<Anima>(); }

    public override bool CanCast(Anima caster, ITarget target) => (caster ?? _self) != null;

    public override void Cast(Anima caster, ITarget target)
    {
        Anima c = caster ?? _self;
        if (c == null) return;
        float r = range > 0f ? range : 2.5f;
        StockShelf best = null; float bestSq = r * r;
        foreach (StockShelf s in FindObjectsOfType<StockShelf>())
        {
            if (!s.Needs) continue;
            float sq = (s.transform.position - c.transform.position).sqrMagnitude;
            if (sq <= bestSq) { bestSq = sq; best = s; }
        }
        if (best != null) { best.StockOne(); LeaveByproduct(); }
        else Debug.Log($"[Inventario] «{c.name}»: no hay estante que abastecer al alcance.");
    }
}
