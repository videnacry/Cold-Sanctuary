using UnityEngine;

/// <summary>
/// ARAÑA DE LAS NIEVES (docs/ice-sanctuary-ecology.md §2): araña MODIFICADA para el hielo, de ~30 cm, que vive en los
/// árboles carnívoros y caza a las abejas de las nieves que anidan en ellos. Anima compuesta como cualquier animal:
/// arquetipo de araña (8 ojos = percepción, emboscada = calma), veneno (armamento) y su dieta prefiere la abeja de las
/// nieves (DietPreference). Su vida de acecho en el tronco la lleva <see cref="SnowSpiderLife"/>.
/// </summary>
public class SnowSpiderBehavior : Animal
{
    protected override string SpeciesArchetype => "Spider";
    protected override string BodyArchetype => "SnowSpider";   // todo de araña salvo el físico: ~30 cm, no 7 mm

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.7f;   // emboscadora: ataca a lo que cabe en su presa
        armadura = 0.35f;          // exoesqueleto engrosado contra el frío
        armament = 0.7f;           // veneno paralizante
    }
}
