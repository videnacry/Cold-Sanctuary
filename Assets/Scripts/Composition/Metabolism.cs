using System.Collections.Generic;
using UnityEngine;

/// <summary>Necesidad de un NUTRIENTE (proteína/grasa/carbohidrato/minerales): un pool que se agota → deuda
/// → apetito. `weight` = prioridad (la proteína manda: *protein leverage*). Distinto por especie/composición.</summary>
[System.Serializable]
public class NutrientNeed
{
    public string nutrient = "protein";
    public float target = 1f;
    public float pool = 1f;
    public float weight = 1f;
}

/// <summary>
/// METABOLISMO / apetito y límite de absorción (docs/stats-as-truth.md §9). Base científica: los animales
/// regulan **nutrientes por separado** (apetitos específicos, geometría nutricional) y la **proteína manda**
/// (*protein leverage*); cada nutriente es un **pool** que se agota (BMR) → **deuda** → **apetito**; el que más
/// falta = **antojo** (`Craving`), que decide **qué cazar y qué parte** (el oso polar craves grasa → come la
/// blubber). **Selectividad**: saciado = exquisito / hambriento = come cualquier cosa (los osos, si hay
/// escasez, comen todo). La **utilización** que construye stats tiene techo (~síntesis proteica, escala con
/// masa + entrena con uso); el **exceso → grasa** (`fatReserves`), no poder → *no comes todo el día para ser
/// fuerte*. Lo útil alimenta `Constitution` (nutriente→elemento→stats). Opt-in.
/// </summary>
public class Metabolism : MonoBehaviour
{
    public Anima anima;
    public Constitution constitution;

    [Header("Necesidades por nutriente (pools → deuda → apetito). Vacío = se siembran los básicos.")]
    public List<NutrientNeed> needs = new List<NutrientNeed>();
    [Tooltip("Desgaste metabólico de los pools por segundo (× masa = BMR).")]
    public float burnPerSecond = 0.02f;

    [Header("Techo de utilización (construye stats; escala con masa ~0.4 g/kg + entrena con uso)")]
    public float baseCeiling = 0.5f;
    public float ceilingPerMass = 0.5f;
    public float windowRegenPerSecond = 0.15f;
    public float adaptGain = 0.005f;
    public float adaptMax = 2f;

    float _used;
    float _adapt = 1f;

    static readonly string[] Basics = { "protein", "fat", "carb", "minerals" };

    public float Ceiling => (baseCeiling + ceilingPerMass * (anima != null ? anima.bodyMass : 1f)) * _adapt;
    public float Remaining => Mathf.Max(0f, Ceiling - _used);

    void Awake()
    {
        if (anima == null) anima = GetComponent<Anima>();
        if (constitution == null) constitution = GetComponent<Constitution>();
        if (needs.Count == 0)
            foreach (string s in Basics)
                needs.Add(new NutrientNeed { nutrient = s, target = 1f, pool = 1f, weight = s == "protein" ? 1.5f : 1f });
    }

    void Update()
    {
        _used = Mathf.Max(0f, _used - windowRegenPerSecond * Time.deltaTime);
        float bmr = burnPerSecond * (anima != null ? anima.bodyMass : 1f) * Time.deltaTime;
        foreach (NutrientNeed n in needs) if (n != null) n.pool = Mathf.Max(0f, n.pool - bmr);
    }

    float Deficit(NutrientNeed n) => n == null || n.target <= 0f ? 0f : Mathf.Clamp01((n.target - n.pool) / n.target);

    /// <summary>0..1. Deuda ponderada de los pools, atenuada por las reservas de grasa (leptina/set-point).</summary>
    public float Appetite
    {
        get
        {
            float w = 0f, d = 0f;
            foreach (NutrientNeed n in needs) if (n != null) { w += n.weight; d += n.weight * Deficit(n); }
            float raw = w > 0f ? d / w : 0f;
            float leptin = anima != null ? Mathf.Clamp01(anima.fatReserves) : 0f;
            return Mathf.Clamp01(raw * (1f - leptin));
        }
    }

    /// <summary>El nutriente que más falta (pondera por prioridad) → qué buscar/cazar y qué parte.</summary>
    public string Craving
    {
        get
        {
            string best = ""; float b = 0f;
            foreach (NutrientNeed n in needs) if (n != null) { float x = n.weight * Deficit(n); if (x > b) { b = x; best = n.nutrient; } }
            return best;
        }
    }

    /// <summary>Saciado → exquisito (solo antojos fuertes); hambriento → come cualquier cosa.</summary>
    public float Selectivity => Mathf.Clamp01(1f - Appetite);

    NutrientNeed Find(string nutrient)
    {
        foreach (NutrientNeed n in needs) if (n != null && n.nutrient == nutrient) return n;
        return null;
    }

    // nutriente → elemento dominante para alimentar la Constitution (química → stats).
    static string ToElement(string nutrient)
    {
        switch (nutrient)
        {
            case "protein":  return "N";
            case "fat":      return "H";
            case "carb":     return "C";
            case "minerals": return "Ca";
            default:         return "C";
        }
    }

    /// <summary>Absorber un NUTRIENTE: llena su pool (sacia la necesidad); lo que cabe en el techo construye
    /// stats (→ `Constitution`); el exceso → grasa. Devuelve lo aprovechado para stats.</summary>
    public float Absorb(string nutrient, float amount)
    {
        if (amount <= 0f) return 0f;
        NutrientNeed n = Find(nutrient);
        if (n != null) n.pool += amount;   // sacia la necesidad (puede rebasar el objetivo)

        float useful = Mathf.Min(amount, Remaining);
        if (useful > 0f)
        {
            _used += useful;
            _adapt = Mathf.Min(adaptMax, _adapt + adaptGain * useful);   // el uso entrena la capacidad
            if (constitution != null) constitution.AddElement(ToElement(nutrient), useful * 0.1f);
        }
        float excess = amount - useful;
        if (excess > 0f && anima != null) anima.fatReserves += excess * 0.05f;   // exceso → grasa, no stats
        return useful;
    }

    /// <summary>Comer: reparte la nutrición en nutrientes según el material; el **antojo** sesga hacia la parte
    /// que da lo que más falta (el oso craves grasa → come la blubber).</summary>
    public void AbsorbFood(float amount, OrganicMaterial material)
    {
        string a, b; float wa, wb;
        switch (material)
        {
            case OrganicMaterial.Meat:
            case OrganicMaterial.Fish:  a = "protein"; b = "fat";      wa = 0.6f; wb = 0.4f; break;
            case OrganicMaterial.Fruit: a = "carb";    b = "minerals"; wa = 0.8f; wb = 0.2f; break;
            case OrganicMaterial.Grass: a = "carb";    b = "minerals"; wa = 0.7f; wb = 0.3f; break;
            default:                    a = "carb";    b = "carb";     wa = 1f;   wb = 0f;   break;
        }
        string c = Craving;   // antojo → come más de la parte que le da lo que le falta
        if (c == a) { wa += 0.3f; wb -= 0.3f; }
        else if (c == b) { wb += 0.3f; wa -= 0.3f; }
        Absorb(a, amount * Mathf.Clamp01(wa));
        if (b != a) Absorb(b, amount * Mathf.Clamp01(wb));
    }

    /// <summary>¿Un material aporta ese nutriente? (para que el antojo sesgue la elección de presa).</summary>
    public static bool Provides(OrganicMaterial material, string nutrient)
    {
        switch (material)
        {
            case OrganicMaterial.Meat:
            case OrganicMaterial.Fish:  return nutrient == "protein" || nutrient == "fat";
            case OrganicMaterial.Fruit:
            case OrganicMaterial.Grass: return nutrient == "carb" || nutrient == "minerals";
            default:                    return nutrient == "carb";
        }
    }
}
