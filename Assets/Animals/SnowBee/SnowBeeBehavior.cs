using UnityEngine;

public class SnowBeeBehavior : Animal
{
    protected override string SpeciesArchetype => "SnowBee";

    // Abeja de las nieves (docs/ice-sanctuary-ecology.md): eusocial, anida cerca del árbol carnívoro de hielo y hace
    // miel de los nutrientes de los muertos que el árbol atrae. Pequeña, voladora ágil, adaptada al frío. Pollinizadora/
    // carroñera suave: come néctar (eatsGrass). Colonia = disciplina/sociabilidad altas.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.3f;   // defiende el nido con aguijón; si no, huye
        armadura = 0.10f;          // cutícula ligera
        armament = 0.25f;          // aguijón (arma defensiva)
    }
}
