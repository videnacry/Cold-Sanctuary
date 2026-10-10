using UnityEngine;

/// <summary>
/// HECHIZO DE MANIPULACIÓN DE PENSAMIENTO/ÁNIMO (docs/anyma-factory-and-control.md §3). "Planta" una inclinación en la
/// mente del objetivo moviendo sus **humores** hacia una valencia (calma/serotonina o desasosiego/cortisol), con lo
/// que cambia el **tono y la valencia** de sus pensamientos (PickTone/Think leen humores) — y los **hace aflorar**
/// (`Mind.SpeakNow`). Es la vía "mente" de la tríada de modificación (con `TransformationSpell`=stats y
/// `BondSpell`=vínculo) para **alinear** a un anima en vez de forzarlo. No implanta texto arbitrario (eso sería sembrar
/// `MindPhrase` propias); planta el ÁNIMO del que nacen los pensamientos.
/// </summary>
public class ThoughtSpell : SpellBase
{
    [Tooltip("true = calma/positividad (serotonina); false = desasosiego (cortisol).")]
    public bool positive = true;

    Anima _self;
    void Awake() { _self = GetComponent<Anima>(); }

    public override bool CanCast(Anima caster, ITarget target)
        => target != null && (target as Component) != null && (target as Component).GetComponent<Mind>() != null;

    public override void Cast(Anima caster, ITarget target)
    {
        if (caster != null) _self = caster;
        Mind m = (target as Component)?.GetComponent<Mind>();
        if (m == null) return;
        float amt = Mathf.Clamp01(force * 0.1f);
        if (positive) m.humores.Produce(Humor.Serotonina, amt);
        else          m.humores.Produce(Humor.Cortisol, amt);
        m.SpeakNow();   // que el nuevo ánimo aflore ya como pensamiento
        LeaveByproduct();
    }
}
