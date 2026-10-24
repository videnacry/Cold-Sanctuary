using UnityEngine;

public class SlugBehavior : Animal
{
    protected override string SpeciesArchetype => "Slug";

    // Babosa negra (Arion ater): hermafrodita; nocturna (sale de día con lluvia); omnívora (hojas, hongos, carroña, heces).
    // Defensa: se CONTRAE en semiesfera y se balancea de lado a lado varios minutos (Rowson et al. 2014, Slugs of Britain
    // and Ireland); moco pegajoso y desagradable. Deja RASTRO de moco que otras babosas siguen. Ciclo casi anual.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0f;
        armadura = 0.15f;          // sin concha: solo el moco
        armament = 0.05f;          // moco pegajoso (disuade)
    }
}
