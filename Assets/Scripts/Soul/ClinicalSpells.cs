using System.Text;
using UnityEngine;

/// <summary>
/// HECHIZO DE ANÁLISIS CLÍNICO (docs/sanctuary1-level-and-live-tests §4) — el que lanza Linduarda en la enfermería al
/// recibir a Kushal: **revela el estado** del objetivo (drives + composición de elementos ideal por masa). Como
/// `ObserveSpell` apunta a la percepción, éste apunta al **cuerpo/salud** (stats/elementos/humores). Se puede lanzar
/// sobre uno mismo o sobre un paciente. Por consola: `analysis({destiny:"Kushal"})`.
/// </summary>
public class ClinicalAnalysisSpell : SpellBase
{
    public override bool CanCast(Anima caster, ITarget target) => target != null;

    public override void Cast(Anima caster, ITarget target)
    {
        Anima a = (target as Component)?.GetComponentInParent<Anima>();
        if (a == null) return;
        var sb = new StringBuilder();
        sb.Append($"[Clínico] «{a.name}»: estrés {a.stress:0.00} · sueño {a.sleepiness:0.00} · enfermedad {a.sickness:0.00}");
        if (a is Animal an) sb.Append($" · hambre {Mathf.Clamp01(an.hungry):0.00}");
        float kg = Mathf.Max(0.001f, a.BodyMass);
        sb.Append($" · ideal(kg {kg:0.##}): ");
        foreach (string el in new[] { "O", "C", "H", "N", "Ca" })
            sb.Append($"{el} {ElementsStatus.Format(ElementsStatus.IdealGrams(el, kg))}  ");
        Debug.Log(sb.ToString());
    }
}

/// <summary>
/// HECHIZO DE VACUNAR (docs/sanctuary1-level-and-live-tests §4) — Linduarda inmuniza a Kushal: **cura la enfermedad
/// actual** y deja al objetivo más resistente (baja `sickness` y le da un empujón de bienestar). Intensidad por `force`.
/// </summary>
public class VaccinateSpell : SpellBase
{
    public override bool CanCast(Anima caster, ITarget target) => target != null;

    public override void Cast(Anima caster, ITarget target)
    {
        Anima a = (target as Component)?.GetComponentInParent<Anima>();
        if (a == null) return;
        a.sickness = 0f;                                          // cura/inmuniza
        a.satisfaction = Mathf.Clamp01(a.satisfaction + 0.1f);
        Mind m = a.GetComponent<Mind>();
        if (m != null) m.humores.Produce(Humor.Serotonina, Mathf.Clamp01(force * 0.1f));
        Debug.Log($"[Vacuna] «{a.name}» vacunado (enfermedad a 0, más resistente).");
    }
}
