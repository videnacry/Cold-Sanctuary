using UnityEngine;

public class CamelSpiderBehavior : Animal
{
    protected override string SpeciesArchetype => "CamelSpider";

    // Solífugo / araña camello (Galeodes arabs): no es araña ni tiene veneno; quelíceros enormes (~1/3 del cuerpo) que
    // cortan la presa. Nocturna y lucífuga, corredora muy rápida, depredadora voraz de insectos y hasta pequeños lagartos
    // (Cloudsley-Thompson 1961, «Some aspects of the physiology and behaviour of Galeodes arabs»; Punzo 1998, The Biology of Camel-Spiders). La hembra guarda los
    // huevos en su madriguera. Solitaria.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.75f;  // ataca casi todo lo que se mueve y le cabe
        armadura = 0.25f;          // cuerpo blando y velludo
        armament = 0.7f;           // quelíceros de tijera (sin veneno)
    }
}
