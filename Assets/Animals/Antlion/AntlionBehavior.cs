using UnityEngine;

public class AntlionBehavior : Animal
{
    protected override string SpeciesArchetype => "Antlion";

    // Hormiga león (larva de Myrmeleontidae): depredador de EMBOSCADA — cava un embudo en la arena y espera;
    // detecta a la presa por vibraciones. Paciencia extrema (composure altísima). Solitario. El embudo = su nido.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.6f;   // ataca lo que cae en el embudo
        armadura = 0.35f;          // cuerpo robusto de larva
        armament = 0.55f;          // mandíbulas inyectoras: arma principal
    }
}
