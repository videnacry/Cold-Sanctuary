using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Aplica un <see cref="AnymaSpec"/> a un anima recién instanciada, UN FRAME después (tras `Animal.Init`),
/// y se autodestruye. Multiplica los stats sobre la media de la especie y fija hambre/sueño/hogar si el spec los da.</summary>
public class AnymaSpecApplier : MonoBehaviour
{
    public AnymaSpec spec;

    void Start() => StartCoroutine(Apply());

    IEnumerator Apply()
    {
        yield return null;   // deja que Animal.Init fije los stats base de la especie
        Anima a = GetComponent<Anima>();
        if (a == null || spec == null) { Destroy(this); yield break; }

        if (spec.randomizeStats)
        {
            float bm = spec.randomStatsLimitations.Roll();   // multiplicador de CUERPO
            float mm = spec.randomStatsLimitations.Roll();   // multiplicador de MENTE
            a.strength *= bm; a.bodyMass *= bm; a.endurance *= bm; a.agility *= bm; a.perception *= bm;
            a.composure *= mm; a.reasoning *= mm; a.memory *= mm; a.creativity *= mm; a.sociability *= mm;
            a.discipline *= mm; a.adaptability *= mm;
        }

        a.sleepiness = spec.sleepiness >= 0f ? spec.sleepiness : Random.value * 0.5f;
        if (a is Animal an)
        {
            an.hungry = spec.hunger >= 0f ? spec.hunger : Random.value * 0.6f;
            if (spec.hasHome) an.HomeOrigin = spec.home;
        }
        // familyRole / sexualidad / pensamientos random: extensión futura (campos preparados en AnymaSpec).
        Destroy(this);
    }
}
