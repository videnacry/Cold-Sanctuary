using UnityEngine;

/// <summary>
/// HECHIZO DE PACIFICACIÓN (docs/spells-suprahechizo-and-thoughts.md §5) — serena a un objetivo: **elimina los humores
/// negativos** (cortisol/adrenalina) y **impulsa los positivos** (serotonina), y lo saca del estado `aware` (deja de
/// responder a amenazas). Como todo hechizo, se aplica a uno mismo, a un target o (iterando) al mundo cercano. Es el
/// hechizo con el que la Magnate calma el bosque (el lobo suelta al conejo). Intensidad por `force`.
/// </summary>
public class PacifySpell : SpellBase
{
    public override bool CanCast(Anima caster, ITarget target) => target != null;

    public override void Cast(Anima caster, ITarget target)
    {
        var comp = target as Component;
        if (comp == null) return;
        Mind m = comp.GetComponent<Mind>();
        if (m != null)
        {
            m.humores.Consume(Humor.Cortisol,   force * 0.2f);
            m.humores.Consume(Humor.Adrenalina, force * 0.2f);
            m.humores.Produce(Humor.Serotonina, force * 0.1f);
        }
        Anima a = comp.GetComponent<Anima>();
        if (a != null) { a.aware = false; a.stress = Mathf.Max(0f, a.stress - force * 0.1f); }
    }
}
