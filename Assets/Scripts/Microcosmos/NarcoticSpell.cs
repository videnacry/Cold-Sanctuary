using UnityEngine;

/// <summary>
/// Hechizo ESTUPEFACIENTE (docs/apremios-guardian-observacion.md §3): SEDA al Guardián — añade `Sedation` al hub
/// alostático del objetivo → **amortigua el sufrimiento** (como la observación, pero por química). Alivio inmediato…
/// con dos costes: (1) **enmascara el daño real** (el estrés dejaba de avisar), y (2) **alimenta la ADICCIÓN**
/// (`AddictionState`: tolerancia ↑ → cada vez seda menos; y abstinencia cuando falta). `force` = potencia sedante.
/// `EffectTiming.Sustained/Periodic` para mantener/repetir la dosis; la tolerancia reduce el efecto con el uso.
/// </summary>
public class NarcoticSpell : SpellBase
{
    public override bool CanCast(Anima caster, ITarget target) => target != null;

    public override void Cast(Anima caster, ITarget target)
    {
        Anima a = target != null ? target.transform.GetComponentInParent<Anima>() : caster;
        if (a == null) return;

        AddictionState add = a.GetComponent<AddictionState>();
        float tolerance = add != null ? add.Tolerance : 0f;
        float effective = force * (1f - tolerance);          // la tolerancia recorta el efecto

        AllostaticState.Of(a).AddSedation(Mathf.Max(0f, effective));
        (add != null ? add : a.gameObject.AddComponent<AddictionState>()).Dose();   // registra la dosis (sube deseo/tolerancia)
    }
}
