using UnityEngine;

/// <summary>
/// HECHIZO DE ENSEÑANZA / DEMOSTRACIÓN (docs/sanctuary1-level-and-live-tests §4, spells-suprahechizo §4): los hechizos
/// se aprenden por **demostración/práctica guiada**. Un maestro (la Magnate, el pinche de guardia, cualquiera que sepa)
/// lanza este hechizo sobre el aprendiz y le **añade el recuerdo** (`Grimoire.Learn(skillId)`) y **potencia el
/// pensamiento** (ánimo + cuenta) de practicarlo. Luego el aprendiz ya puede lanzarlo solo desde la consola. Como todo
/// hechizo, se aplica a un target (y en una clase, cada uno es maestro y estudiante). Intensidad por `force`.
/// </summary>
public class TeachSpell : SpellBase
{
    [Tooltip("Qué habilidad/hechizo se enseña (id de Grimoire): clean, stock, yoga, …")]
    public string skillId = "yoga";

    public override bool CanCast(Anima caster, ITarget target) => target != null;

    public override void Cast(Anima caster, ITarget target)
    {
        Component comp = target as Component;
        if (comp == null) return;
        Grimoire g = comp.GetComponent<Grimoire>() ?? comp.gameObject.AddComponent<Grimoire>();
        bool learned = g.Learn(skillId);
        Mind m = comp.GetComponent<Mind>();
        if (m != null) m.humores.Produce(Humor.Serotonina, Mathf.Clamp01(force * 0.1f));   // el ánimo de practicar sube
        Debug.Log($"[Enseñanza] «{(caster != null ? caster.name : name)}» enseña «{skillId}» a «{comp.name}» " +
                  $"({(learned ? "aprendido" : "ya lo sabía, reforzado")}).");
    }
}
