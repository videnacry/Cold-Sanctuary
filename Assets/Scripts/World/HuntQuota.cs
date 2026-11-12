using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CUOTA DE CAZA (docs/reglas-animales.md, «Cadena alimenticia»). <see cref="FoodChain"/> hace el balance de un año del santuario
/// (28 días de juego) suponiendo que una presa da lo que cría: crías al año por hembra × ½ hembras, con un margen de 0,8. Pero la caza
/// no lo respetaba: un cazador se lleva lo que pueda cada vez que tiene hambre, y en una batería (≈1,5 días de juego) caían 3 de las
/// 6 liebres y los 2 zorros del santuario. Ahora en el santuario se caza, de cada especie, lo que esa cuenta da por año, y nunca por
/// debajo de una pareja (E11: «mínimo 2, para que pueda haber pareja»). Lo que falte lo pone el alimentador (<see cref="FoodChain.LabShare"/>).
/// En la realidad los lobos se llevan el 3–7 % de una manada al año (la misma sección del documento).
/// Fuera del santuario (los escenarios de las pruebas) y para especies sin plan, no hay cuota.
/// </summary>
public static class HuntQuota
{
    public const int MinLiving = 2;
    public const float WindowHours = FoodChain.DaysPerYear * 24f;
    static readonly Dictionary<string, List<float>> _kills = new Dictionary<string, List<float>>();
    static readonly Dictionary<string, int> _living = new Dictionary<string, int>();
    static int _livingFrame = -1;

    /// <summary>Presas de la especie que se pueden cazar en un año del santuario (al menos 1). Sin plan: sin límite.</summary>
    public static int PerYear(string species)
    {
        if (string.IsNullOrEmpty(species) || !PopulationPlan.Groups.ContainsKey(species)) return int.MaxValue;
        var (_, interval) = Reproduction.Calendar(species);
        float young = PopulationPlan.Census(species) * 12f / Mathf.Max(1f, interval) * 0.5f;   // crías al año: por hembra × ½ hembras
        return Mathf.Max(1, Mathf.FloorToInt(young * FoodChain.Margin + 1e-4f));
    }

    /// <summary>La regla, sin escena: con <paramref name="living"/> vivos y <paramref name="taken"/> cazados este año, ¿se puede cazar otro?</summary>
    public static bool Allows(string species, int living, int taken)
    {
        if (string.IsNullOrEmpty(species) || !PopulationPlan.Groups.ContainsKey(species)) return true;
        return living > MinLiving && taken < PerYear(species);
    }

    public static bool InSanctuary(Vector3 p) => Mathf.Abs(p.x) <= 250f && Mathf.Abs(p.z) <= 250f;   // (como PredatorRound.Served)

    /// <summary>¿Puede un cazador ir a por esta presa viva ahora?</summary>
    public static bool MayHunt(Anima prey)
    {
        if (prey == null || !InSanctuary(prey.transform.position)) return true;
        string s = prey.SpeciesName;
        return Allows(s, Living(s), Taken(s));
    }

    /// <summary>Una presa viva cazada en el santuario cuenta para la cuota de su especie.</summary>
    public static void Record(string species, Vector3 at)
    {
        if (string.IsNullOrEmpty(species) || !InSanctuary(at) || !PopulationPlan.Groups.ContainsKey(species)) return;
        if (!_kills.TryGetValue(species, out List<float> l)) _kills[species] = l = new List<float>();
        l.Add(Now);
        Debug.Log($"[Caza] cazada una presa de «{species}»: {Taken(species)}/{PerYear(species)} este año del santuario.");
    }

    public static int Taken(string species)
    {
        if (string.IsNullOrEmpty(species) || !_kills.TryGetValue(species, out List<float> l)) return 0;
        float now = Now;
        l.RemoveAll(t => now - t >= WindowHours || t > now + 1f);   // (un reloj puesto atrás por una prueba no deja cuotas «del futuro»)
        return l.Count;
    }

    static float Now => TimeController.timeController != null ? TimeController.timeController.Day * 24f + TimeController.timeController.Hour : Time.time / 60f;

    static int Living(string species)
    {
        if (_livingFrame != Time.frameCount)
        {
            _livingFrame = Time.frameCount; _living.Clear();
            foreach (GameObject go in Animal.wholePopulation)
            {
                if (go == null || !go.TryGetComponent(out Animal a) || a.death || !InSanctuary(go.transform.position) || a.SpeciesName == null) continue;
                _living[a.SpeciesName] = _living.TryGetValue(a.SpeciesName, out int n) ? n + 1 : 1;
            }
        }
        return _living.TryGetValue(species, out int c) ? c : 0;
    }
}
