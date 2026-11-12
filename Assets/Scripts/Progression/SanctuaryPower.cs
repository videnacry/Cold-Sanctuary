using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EL PODER QUE HACE SUBIR DE SANTUARIO (Berón 2026-11-08; docs/progresion-santuarios.md). Un humano sale de su santuario solo cuando su poder
/// supera al del animal más poderoso de ese santuario (su JEFE): en s1n1, la ballena, la orca o el oso polar, el que sea más fuerte de la fauna que hay.
/// Todo en la escala de <see cref="Predation.PredatorPower"/> (una sola fórmula: <see cref="Predation.Power"/>):
///   PODER HUMANO = cuerpo (Predation.Power de sus aptitudes: el Anima de la escena o, sin cuerpo, su arquetipo)
///                + MAGIA (lo que las margas suben su poder de hechizo: SpellPower(nivel) − SpellPower(0), <see cref="DerivedStats.SpellPower"/>)
///                + bonusPacks que lleve y su cuerpo aún no tenga (Predation.Power del pack: docs/soul-composition-blend.md §bonusPacks).
///   JEFE de la posición p = el mayor de: el animal más poderoso de los santuarios hasta el suyo (hay que ser más fuerte que TODOS sus animales:
///                docs/sanctuary-second-lap-and-fear.md §3) y un humano corriente con el bonusPack del santuario (bonusPackN ≈ lo que hace falta
///                para superar el santuario N). Sin pack para la posición, no se sube más.
/// Lógica pura salvo <see cref="LiveFauna"/> (lee la fauna de la escena).
/// </summary>
public static class SanctuaryPower
{
    /// <summary>La coraza de un cuerpo sin dato (el valor por defecto de <see cref="Anima.armadura"/>).</summary>
    public const float DefaultArmadura = 0.1f;

    public static float OfAptitudes(Aptitudes a, float armadura = DefaultArmadura, float armament = 0f) => Predation.Power(a.strength, a.bodyMass, armadura, a.agility, armament);

    /// <summary>El poder de un arquetipo de cuerpo (<see cref="Archetypes.BodyOf"/>).</summary>
    public static float OfSpecies(string archetype) => OfAptitudes(Archetypes.BodyOf(archetype).aptitudes);

    /// <summary>Lo que un pack suma al poder (sus aptitudes de fuerza, masa y agilidad, en la misma fórmula; sin coraza ni arma).</summary>
    public static float OfPack(string pack) => Archetypes.TryPack(pack, out Aptitudes p) ? Predation.Power(p.strength, p.bodyMass, 0f, p.agility, 0f) : 0f;

    /// <summary>La magia: lo que sus margas suben su poder de hechizo (0 con las tres a nivel 1).</summary>
    public static float Magic(Aptitudes a, int soulLevels) => DerivedStats.SpellPower(a, soulLevels) - DerivedStats.SpellPower(a, 0);

    /// <summary>Un humano corriente (el arquetipo «Human», sin magia ni packs): la base con la que se calibran los packs.</summary>
    public static float PlainHuman => OfSpecies("Human");

    /// <summary>LOS PACKS DE INICIO (Berón 2026-11-08): Irosene, bonusPack2 (docs/soul-composition-blend.md §bonusPacks); Lizmibet, el mismo (inferido:
    /// los docs no dicen otro). Se suman si su cuerpo aún no los lleva (<see cref="SoulComposition.bonusPacks"/>).</summary>
    public static readonly Dictionary<string, string[]> StartingPacks = new Dictionary<string, string[]> { { "Irosene", new[] { "bonusPack2" } }, { "Lizmibet", new[] { "bonusPack2" } } };

    /// <summary>EL PODER DE UN HUMANO. <paramref name="body"/>: su cuerpo en la escena (o null: su arquetipo por su nombre).</summary>
    public static float OfHuman(GameObject body, string name)
    {
        Anima an = body != null ? body.GetComponent<Anima>() : null;
        float p = an != null ? Predation.PredatorPower(an) : OfSpecies(name);
        CharacterLevel cl = body != null ? body.GetComponent<CharacterLevel>() : null;
        if (cl != null) p += Magic(cl.aptitudes, cl.SoulLevels);
        SoulComposition sc = body != null ? body.GetComponent<SoulComposition>() : null;
        if (name != null && StartingPacks.TryGetValue(name, out string[] packs))
            foreach (string k in packs) if (sc == null || !sc.bonusPacks.Contains(k)) p += OfPack(k);
        return p;
    }

    /// <summary>LA FAUNA DE CADA SANTUARIO, por especie (arquetipo). El 1 se lee de la escena (<see cref="LiveFauna"/>); esto es para cuando no está
    /// cargada y para los otros dos. INFERIDO: la lista de la tierra son las especies del territorio polar de SampleScene; el Aéreo, sus aves frías
    /// (docs/sitio-nuclear.md §23); el Marino aún no tiene animales propios (el árbol cazador es una planta).</summary>
    public static readonly Dictionary<int, string[]> FaunaOf = new Dictionary<int, string[]>
    {
        { SanctuaryPlacement.Land, new[] { "Whale", "Orca", "Bear", "Walrus", "Seal", "Wolf", "Malamute", "Reindeer", "MuskOx", "ArcticFox", "ArcticHare", "EmperorPenguin", "Penguin" } },
        { SanctuaryPlacement.Marine, new string[0] },
        { SanctuaryPlacement.Aerial, new[] { "PolarSkua", "CapePetrel", "WanderingAlbatross", "ArcticTern" } },
    };

    /// <summary>El animal más poderoso de un santuario: (especie, poder). <paramref name="live"/>: si se da, manda para el santuario 1.</summary>
    public static (string who, float power) Strongest(int sanctuary, (string who, float power)? live = null)
    {
        if (sanctuary == SanctuaryPlacement.Land && live.HasValue && live.Value.power > 0f) return live.Value;
        (string, float) best = ("—", 0f);
        if (FaunaOf.TryGetValue(sanctuary, out string[] list)) foreach (string s in list) { float p = OfSpecies(s); if (p > best.Item2) best = (s, p); }
        return best;
    }

    /// <summary>El animal vivo más poderoso de la escena activa (el santuario 1), o null si no hay.</summary>
    public static (string who, float power)? LiveFauna()
    {
        (string, float)? best = null;
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (GameObject go in Animal.wholePopulation)
        {
            if (go == null || go.scene != scene || !go.TryGetComponent(out Animal a) || a.death) continue;
            float p = Predation.PredatorPower(a);
            if (best == null || p > best.Value.Item2) best = (go.name, p);
        }
        return best;
    }

    /// <summary>El pack que pide superar la posición <paramref name="position"/>: bonusPack(posición + 1).</summary>
    public static string PackFor(int position) => $"bonusPack{position + 1}";

    /// <summary>LOS JEFES: el poder que hay que superar para salir de cada posición (0 = s1n1…). Tantas como packs haya; sin pack, no se sube más.</summary>
    public static List<(string who, float power)> Bosses((string who, float power)? live = null)
    {
        var l = new List<(string, float)>();
        for (int p = 0; Archetypes.TryPack(PackFor(p), out _); p++)
        {
            var home = SanctuaryPlacement.Home.At(p);
            (string who, float power) fauna = ("—", 0f);
            if (home.level == 1) for (int s = 1; s <= home.sanctuary; s++) { var f = Strongest(s, live); if (f.power > fauna.power) fauna = f; }   // (los de nivel 2 aún no tienen fauna propia)
            float calibrated = PlainHuman + OfPack(PackFor(p));
            l.Add(fauna.power >= calibrated ? fauna : ($"humano + {PackFor(p)}", calibrated));
        }
        return l;
    }

    /// <summary>La posición que da un poder: cuántos jefes seguidos supera desde s1n1.</summary>
    public static int PositionFor(float power, List<(string who, float power)> bosses)
    {
        int p = 0;
        while (p < bosses.Count && power > bosses[p].power) p++;
        return p;
    }
}
