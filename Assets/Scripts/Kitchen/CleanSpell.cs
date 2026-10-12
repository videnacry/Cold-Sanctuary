using UnityEngine;

/// <summary>
/// HECHIZO DE LIMPIEZA (docs/spells-suprahechizo §; sanctuary1 onboarding): limpiar una mancha (<see cref="DirtSpot"/>)
/// pasa a ser un **hechizo** — como toda acción. Se lanza sobre uno mismo (limpia la mancha más cercana dentro del
/// alcance) y, al tener RANGO (`range`, ampliable con magia), **falla** si no estás cerca → hay que acercarse. Es
/// aprendible (`Grimoire`) y **enseñable** (el pinche de guardia lo demuestra con <see cref="TeachSpell"/> sobre sí mismo
/// y el aprendiz, que así se lo lleva de recuerdo). Reutiliza `DirtArea.CleanNearest`.
/// </summary>
public class CleanSpell : SpellBase
{
    Anima _self;
    void Awake() { _self = GetComponent<Anima>(); }

    public override bool CanCast(Anima caster, ITarget target) => (caster ?? _self) != null;

    public override void Cast(Anima caster, ITarget target)
    {
        Anima c = caster ?? _self;
        if (c == null) return;
        float r = range > 0f ? range : 2.5f;
        foreach (DirtArea a in FindObjectsOfType<DirtArea>())
            if (a.CleanNearest(c.transform.position, r)) { Debug.Log($"[Limpieza] «{c.name}» limpia una mancha."); LeaveByproduct(); return; }
        Debug.Log($"[Limpieza] «{c.name}»: no hay mancha al alcance.");
    }
}
