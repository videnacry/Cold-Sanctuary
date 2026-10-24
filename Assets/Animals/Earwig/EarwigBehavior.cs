using UnityEngine;

public class EarwigBehavior : Animal
{
    protected override string SpeciesArchetype => "Earwig";

    // Tijereta (Forficula auricularia): nocturna, se mete en grietas (tigmotaxis); omnívora (pulgones, polen, hojas). La
    // MADRE cuida: junta, limpia (contra hongos) y defiende los huevos todo el invierno y alimenta a las ninfas
    // regurgitando (Kölliker 2007, Behav Ecol Sociobiol 61:1489; Pasquier et al. 2025, doi 10.1016/j.yhbeh.2025.105839).

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.35f;  // amenaza con los cercos; la madre defiende el nido
        armadura = 0.3f;
        armament = 0.3f;           // cercos-pinza
    }
}
