using UnityEngine;

/// <summary>
/// HECHIZO DE MANIPULACIÓN DE VÍNCULO (docs/anyma-factory-and-control.md §3: "control = alinear"). Modifica el **bond**
/// del lanzador hacia un objetivo (lo acerca o lo aleja). Junto a `TransformationSpell` (stats) y `ThoughtSpell`
/// (mente), es una de las tres vías por las que el jugador **prepara** a un anima para que esté alineado con lo que
/// quiere — en vez de forzar una marioneta. Usa `Anima.GrowBond` (el mismo mecanismo que las amistades emergentes).
/// </summary>
public class BondSpell : SpellBase
{
    [Tooltip("Cuánto cambia el bond por lanzamiento (usa `force` de SpellBase). weaken=true lo RESTA.")]
    public bool weaken = false;
    public BondType bondType = BondType.Friend;

    Anima _self;
    void Awake() { _self = GetComponent<Anima>(); }

    public override bool CanCast(Anima caster, ITarget target) => (caster ?? _self) != null && target != null;

    public override void Cast(Anima caster, ITarget target)
    {
        if (caster != null) _self = caster;
        if (_self == null || target == null) return;
        float amount = (weaken ? -1f : 1f) * Mathf.Max(0f, force);
        _self.GrowBond(target, bondType, amount);
        LeaveByproduct();
    }
}
