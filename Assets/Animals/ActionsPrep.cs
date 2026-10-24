using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class ActionsPrep
{
    public ActionPrep idle;
    public ActionPrep walk;
    public ActionPrep run;
    public ActionsPrep(ActionPrep pIdle, ActionPrep pWalk, ActionPrep pRun)
    {
        this.idle = pIdle;
        this.walk = pWalk;
        this.run = pRun;
    }

    // Gaits por especie (etapa 5): antes un `actsPrep` por clase, ahora DATA. Valores extraídos 1:1 de las especies
    // (idle/walk/run con aniName + navSpeed/aniSpeed/energyCost). Bunny reusa "RunBunny" para andar (salta).
    static readonly ActionsPrep _default =
        new ActionsPrep(new ActionPrep("Idle", 0f, 1f, -2f), new ActionPrep("Walk", 3f, 2f, 0.5f), new ActionPrep("Run", 10f, 4f, 2f));

    static Dictionary<string, ActionsPrep> _catalog;
    static void BuildCatalog() => _catalog = new Dictionary<string, ActionsPrep>
    {
        { "Bear", new ActionsPrep(new ActionPrep("IdleBear", 0f, 1f, -2f), new ActionPrep("WalkBear", 3f, 2f, 0.5f), new ActionPrep("RunBear", 12f, 4f, 2f)) },
        { "Bunny", new ActionsPrep(new ActionPrep("IdleBunny", 0f, 1f, -2f), new ActionPrep("RunBunny", 8f, 4f, 1f), new ActionPrep("RunBunny", 22f, 10f, 2f)) },
        { "Deer", new ActionsPrep(new ActionPrep("IdleDeer", 0f, 1f, -2f), new ActionPrep("WalkDeer", 5f, 2f, 0.5f), new ActionPrep("RunDeer", 18f, 4f, 2f)) },
        { "Fox", new ActionsPrep(new ActionPrep("IdleFox", 0f, 1f, -2f), new ActionPrep("WalkFox", 3f, 3f, 0.5f), new ActionPrep("RunFox", 14f, 5f, 2f)) },
        { "Malamute", new ActionsPrep(new ActionPrep("IdleMalamute", 0f, 1f, -2f), new ActionPrep("WalkMalamute", 4f, 3f, 0.5f), new ActionPrep("RunMalamute", 18f, 5f, 2f)) },
        { "Seal", new ActionsPrep(new ActionPrep("IdleSeal", 0f, 1f, -2f), new ActionPrep("WalkSeal", 4f, 2f, 0.5f), new ActionPrep("RunSeal", 10f, 3f, 2f)) },
        { "Whale", new ActionsPrep(new ActionPrep("IdleWhale", 0f, 1f, -2f), new ActionPrep("WalkWhale", 3f, 2f, 0.5f), new ActionPrep("RunWhale", 8f, 3f, 2f)) },
        { "Wolf", new ActionsPrep(new ActionPrep("IdleWolf", 0f, 1f, -2f), new ActionPrep("WalkWolf", 3f, 3f, 0.5f), new ActionPrep("RunWolf", 22f, 5f, 2f)) },
        // Fauna de hielo — pingüino torpe en tierra (lento); orca nadadora veloz
        { "Penguin", new ActionsPrep(new ActionPrep("IdlePenguin", 0f, 1f, -2f), new ActionPrep("WalkPenguin", 2f, 2f, 0.5f), new ActionPrep("RunPenguin", 6f, 4f, 2f)) },
        { "Orca", new ActionsPrep(new ActionPrep("IdleOrca", 0f, 1f, -2f), new ActionPrep("WalkOrca", 4f, 2f, 0.5f), new ActionPrep("RunOrca", 14f, 3f, 2f)) },
        // Especies de las eras del microcosmos (docs/era-species-behavior.md): velocidades RELATIVAS a la hormiga (andar 3,
        // correr 10). El caracol, la babosa y la lombriz no "corren" (reptan por ondas del pie/peristaltismo); el solífugo,
        // la cucaracha y el cangrejo fantasma están entre los artrópodos más rápidos (Burrows & Hoyle 1973; Full & Tu 1991).
        { "Snail",       Slow(0.6f, 0.8f) }, { "Slug",       Slow(0.7f, 0.9f) }, { "Earthworm", Slow(0.8f, 1.3f) },
        { "Woodlouse",   Gaits(2f, 4f) },    { "VelvetMite", Gaits(2f, 3f) },    { "Scorpionfly", Gaits(1.5f, 6f) },
        { "Scorpion",    Gaits(2.5f, 9f) },  { "Earwig",     Gaits(3f, 8f) },    { "Firefly",   Gaits(2f, 8f) },
        { "Cicada",      Gaits(2f, 10f) },   { "CamelSpider", Gaits(4f, 16f) },  { "Cockroach", Gaits(3f, 14f) },
        { "GhostCrab",   Gaits(3f, 16f) },
    };

    static ActionsPrep Gaits(float walk, float run) =>
        new ActionsPrep(new ActionPrep("Idle", 0f, 1f, -2f), new ActionPrep("Walk", walk, 2f, 0.5f), new ActionPrep("Run", run, 4f, 2f));
    // Reptantes (pie de molusco, peristaltismo): "correr" cuesta mucho y apenas acelera.
    static ActionsPrep Slow(float walk, float run) =>
        new ActionsPrep(new ActionPrep("Idle", 0f, 1f, -2f), new ActionPrep("Walk", walk, 1f, 0.8f), new ActionPrep("Run", run, 1.2f, 3f));

    /// <summary>Los gaits (idle/walk/run) de una especie. Desconocida → genérico.</summary>
    public static ActionsPrep Of(string species)
    {
        if (_catalog == null) BuildCatalog();
        return species != null && _catalog.TryGetValue(species, out ActionsPrep a) ? a : _default;
    }
}
