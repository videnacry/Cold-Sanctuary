using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EQUILIBRIO DE ESPECIES (docs/ice-sanctuary-ecology.md): es un SANTUARIO de animales, no una ciudad, así que la fauna
/// se dimensiona en proporción a los HUMANOS que viven en él (la base). Por cada humano: 0,3 osos, 1,2 pingüinos, 0,6
/// focas, 0,2 ballenas y 0,1 orcas (y 0,2 zorros/lobos donde los haya). Así carnívoros y presas quedan equilibrados y,
/// si llega más gente, el santuario puede acoger más animales. La reproducción respeta un techo de 1,5× ese objetivo.
/// </summary>
public static class PopulationPlan
{
    public static readonly Dictionary<string, float> PerHuman = new Dictionary<string, float>
    {
        { "Bear", 0.3f }, { "PolarBear", 0.3f }, { "Penguin", 1.2f }, { "Seal", 0.6f }, { "Whale", 0.2f }, { "Orca", 0.1f },
        { "Fox", 0.2f }, { "Wolf", 0.2f },
    };

    /// <summary>Humanos del santuario (el personal + la Magnate). Lo actualiza la jornada al cambiar el personal.</summary>
    public static int Humans = 11;

    /// <summary>Cuántos individuos le tocan a la especie (mínimo 2, para que pueda haber pareja).</summary>
    public static int Target(string species) => PerHuman.TryGetValue(species, out float r) ? Mathf.Max(2, Mathf.RoundToInt(Humans * r)) : 0;

    /// <summary>Techo para la reproducción (1,5× el objetivo); 0 = sin plan para esa especie.</summary>
    public static int Cap(string species) { int t = Target(species); return t > 0 ? Mathf.CeilToInt(t * 1.5f) : 0; }

    /// <summary>Reparte <see cref="Target"/> entre <paramref name="groups"/> grupos (los primeros, uno más si no es exacto).</summary>
    public static int[] Split(string species, int groups)
    {
        int t = Target(species); groups = Mathf.Max(1, groups);
        var r = new int[groups];
        for (int i = 0; i < groups; i++) r[i] = t / groups + (i < t % groups ? 1 : 0);
        return r;
    }
}
