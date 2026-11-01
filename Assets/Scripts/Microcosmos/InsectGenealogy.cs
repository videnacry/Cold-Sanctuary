using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// REPRODUCCIÓN ENTRE ESPECIES (docs/microcosmos-civilizaciones.md §7; Berón 2026-11-01: «se podrían reproducir entre todos
/// y los hijos podrían salir como cualquier insecto del árbol genealógico, de modo que una mariquita se podría casar con una
/// araña y tener un hijo ciempiés como el tatarabuelo»). En las eras civilizadas los insectos-humanos conviven y se casan
/// entre especies. Cada uno lleva su LINAJE (<see cref="FolkLineage"/>): su especie y la de sus antepasados con su
/// generación. La especie del hijo se SORTEA entre todas las del árbol: cada generación pesa la mitad que la anterior
/// (padres 1 entre los dos, abuelos ½ entre los cuatro, bisabuelos ¼, tatarabuelos ⅛), así lo más probable es salir como
/// uno de los padres, pero cualquier antepasado puede «volver».
/// Datos puros (sin escena): lo prueba <c>InsectOnlyTest</c>.
/// </summary>
public static class InsectGenealogy
{
    public const int MaxDepth = 5;   // hasta los padres de los tatarabuelos

    [System.Serializable]
    public struct Ancestor
    {
        public string name, species;
        public int depth;   // 1 = padre/madre, 2 = abuelo, 3 = bisabuelo, 4 = tatarabuelo
        public Ancestor(string name, string species, int depth) { this.name = name; this.species = species; this.depth = depth; }
    }

    /// <summary>Peso de UN antepasado a la generación <paramref name="depth"/> (con 2^d antepasados en ella, la generación
    /// entera pesa ½^(d−1)).</summary>
    public static float Weight(int depth) => depth < 1 ? 0f : Mathf.Pow(0.5f, depth - 1) / Mathf.Pow(2f, depth);

    /// <summary>Los antepasados del hijo de <paramref name="mother"/> y <paramref name="father"/> (los dos a 1 y los de cada uno, +1).</summary>
    public static List<Ancestor> ChildAncestry(string motherName, string motherSpecies, IEnumerable<Ancestor> motherAnc,
                                              string fatherName, string fatherSpecies, IEnumerable<Ancestor> fatherAnc)
    {
        var l = new List<Ancestor> { new Ancestor(motherName, motherSpecies, 1), new Ancestor(fatherName, fatherSpecies, 1) };
        if (motherAnc != null) foreach (Ancestor a in motherAnc) if (a.depth + 1 <= MaxDepth) l.Add(new Ancestor(a.name, a.species, a.depth + 1));
        if (fatherAnc != null) foreach (Ancestor a in fatherAnc) if (a.depth + 1 <= MaxDepth) l.Add(new Ancestor(a.name, a.species, a.depth + 1));
        return l;
    }

    /// <summary>Probabilidad de cada especie para un hijo con estos antepasados.</summary>
    public static Dictionary<string, float> Odds(IEnumerable<Ancestor> ancestry)
    {
        var d = new Dictionary<string, float>(); float total = 0f;
        foreach (Ancestor a in ancestry)
        {
            if (string.IsNullOrEmpty(a.species)) continue;
            float w = Weight(a.depth);
            d[a.species] = (d.TryGetValue(a.species, out float v) ? v : 0f) + w; total += w;
        }
        if (total > 0f) foreach (string k in new List<string>(d.Keys)) d[k] /= total;
        return d;
    }

    /// <summary>Sortea la especie del hijo (<paramref name="roll"/> en [0, 1)).</summary>
    public static string ChildSpecies(IEnumerable<Ancestor> ancestry, float roll)
    {
        string last = null; float acc = 0f;
        foreach (var kv in Odds(ancestry)) { last = kv.Key; acc += kv.Value; if (roll < acc) return kv.Key; }
        return last ?? "Ant";
    }
}
