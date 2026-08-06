using UnityEngine;

/// <summary>
/// METABOLISMO / límite de absorción (docs/stats-as-truth.md §9). Base científica: la **absorción** no tiene
/// tope duro, pero la **UTILIZACIÓN** que construye tejido/stats SÍ (techo tipo síntesis proteica ~0.4 g/kg por
/// comida; el exceso se **oxida/almacena como GRASA**, no como poder) → **no puedes comer todo el día para
/// hacerte fuerte**. El **apetito** lo regulan dos señales: **ghrelina** (hambre, corto plazo) y **leptina**
/// (saciedad ∝ reservas de grasa, largo plazo/set-point) → con reservas altas, comes menos. El techo **escala
/// con la masa** y **adapta con el uso** (entreno del intestino) → la **capacidad crece con los stats**. Lo útil
/// va a `Constitution` (→ stats base); el exceso a `Anima.fatReserves`. Opt-in.
/// </summary>
public class Metabolism : MonoBehaviour
{
    public Anima anima;
    public Constitution constitution;

    [Header("Techo de utilización (lo que construye stats por 'comida'; escala con la masa ~0.4 g/kg)")]
    public float baseCeiling = 0.5f;
    public float ceilingPerMass = 0.5f;
    [Tooltip("La 'ventana de comida' se recupera con el tiempo (poder volver a utilizar).")]
    public float windowRegenPerSecond = 0.15f;

    [Header("Adaptación (entreno del intestino): el uso sube el techo hacia un tope")]
    public float adaptGain = 0.005f;
    public float adaptMax = 2f;

    [Header("Apetito")]
    [Tooltip("Valor de 'hungry' (Animal) al que el apetito por hambre es máximo.")]
    public float hungerFullAt = 5f;

    float _used;
    float _adapt = 1f;

    public float Ceiling => (baseCeiling + ceilingPerMass * (anima != null ? anima.bodyMass : 1f)) * _adapt;
    public float Remaining => Mathf.Max(0f, Ceiling - _used);

    /// <summary>0..1. Sube con el hambre (ghrelina) y BAJA con las reservas de grasa (leptina/set-point).</summary>
    public float Appetite
    {
        get
        {
            float hunger = 0.5f;
            Animal a = anima as Animal;
            if (a != null) hunger = Mathf.Clamp01(a.hungry / Mathf.Max(0.01f, hungerFullAt));
            float leptin = anima != null ? Mathf.Clamp01(anima.fatReserves) : 0f;
            return Mathf.Clamp01(hunger - leptin);
        }
    }

    void Awake()
    {
        if (anima == null) anima = GetComponent<Anima>();
        if (constitution == null) constitution = GetComponent<Constitution>();
    }

    void Update() { _used = Mathf.Max(0f, _used - windowRegenPerSecond * Time.deltaTime); }

    /// <summary>
    /// Absorber `amount` de un elemento: solo lo que cabe en el techo **construye stats** (→ `Constitution`);
    /// el **exceso** se almacena como **grasa** (energía), no como poder. El uso **entrena** la capacidad.
    /// Devuelve lo aprovechado.
    /// </summary>
    public float Absorb(string symbol, float amount)
    {
        if (amount <= 0f) return 0f;
        float useful = Mathf.Min(amount, Remaining);
        if (useful > 0f)
        {
            _used += useful;
            _adapt = Mathf.Min(adaptMax, _adapt + adaptGain * useful);   // el uso entrena la capacidad (intestino)
            if (constitution != null) constitution.AddElement(symbol, useful * 0.1f);
        }
        float excess = amount - useful;
        if (excess > 0f && anima != null) anima.fatReserves += excess * 0.05f;   // exceso → grasa, no stats
        return useful;
    }

    /// <summary>Absorber COMIDA: mapea el material a su elemento dominante y absorbe (lo llama el acto de comer).</summary>
    public float AbsorbFood(float amount, OrganicMaterial material)
    {
        string sym;
        switch (material)
        {
            case OrganicMaterial.Meat:  sym = "N"; break;   // proteína (nitrógeno)
            case OrganicMaterial.Fish:  sym = "N"; break;   // proteína
            case OrganicMaterial.Fruit: sym = "C"; break;   // azúcares (carbono)
            case OrganicMaterial.Grass: sym = "C"; break;   // fibra (carbono)
            default:                    sym = "C"; break;
        }
        return Absorb(sym, amount);
    }
}
