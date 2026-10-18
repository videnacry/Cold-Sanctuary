using UnityEngine;

/// <summary>
/// HECHIZO DE SEGUIR (docs/anima-architecture §11.5): el lanzador hace que el TARGET lo siga — le inyecta un
/// <see cref="FollowBrain"/> apuntando a sí mismo (relevancia por encima del idle → toma el mando mientras haya a quién
/// seguir). Así Linduarda lleva al recién llegado a la máquina de virtualización, o Goluis guía al pinche por la cocina.
/// <see cref="Stop"/> corta el seguimiento. Como todo hechizo: aprendible, con rango/coste.
/// </summary>
public class FollowSpell : SpellBase
{
    public float followerRelevance = 2f;

    public override bool CanCast(Anima caster, ITarget target) => caster != null && target != null;

    public override void Cast(Anima caster, ITarget target)
    {
        Component comp = target as Component;
        if (comp == null || caster == null) return;
        if (!comp.TryGetComponent(out FollowBrain fb)) fb = comp.gameObject.AddComponent<FollowBrain>();
        fb.target = caster.transform;
        fb.relevance = Mathf.Max(fb.relevance, followerRelevance);
        LeaveByproduct();
        Debug.Log($"[Seguir] «{comp.name}» sigue a «{caster.name}».");
    }

    /// <summary>Deja de seguir (el FollowBrain suelta el mando al quedarse sin objetivo).</summary>
    public static void Stop(Component who)
    {
        if (who == null) return;
        FollowBrain fb = who.GetComponent<FollowBrain>();
        if (fb != null) fb.target = null;
    }
}
