using UnityEngine;

public class DungBeetleBehavior : Animal
{
    protected override string SpeciesArchetype => "DungBeetle";

    // Escarabajo pelotero (Scarabaeus): descomponedor, rueda y entierra estiércol (nidos fósiles reales en
    // niveles australopitecinos). Fuerza extraordinaria para su tamaño; navega por señales celestes. Solitario.
    // Detritívoro (aquí come materia vegetal/detrito). Élitros duros = toughness alta.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.1f;   // no defiende; se retrae
        armadura = 0.55f;          // élitros muy duros
        armament = 0.05f;          // sin arma ofensiva
    }
}
