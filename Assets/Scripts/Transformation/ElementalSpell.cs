using System.Collections.Generic;
using UnityEngine;

/// <summary>Elemento de un hechizo elemental básico.</summary>
public enum SpellElement { Water, Earth, Wind }

/// <summary>
/// Hechizos elementales básicos — **agua / tierra / viento** — con coste físico (docs/magic-metabolism §16
/// "elementales"). Complementan al <see cref="FireSpell"/> para probar el bucle. Cada elemento tiene un PERFIL
/// de coste distinto (tal como se planteó: unos pesan en materia, otros en energía):
///   • **Agua** (H₂O): materia media (proyectas agua) + energía cinética.
///   • **Tierra** (SiO₂): MUCHA materia (roca) + poca energía.
///   • **Viento**: casi TODA energía, materia ~0 (mueve el aire ambiente, gratis) → hechizo "de bosones".
/// Coste materia = desglose por elemento de la masa proyectada; coste energía = **½·m·v²** (cinética). Se paga de
/// `MagicReserves.Pay(cost, energyCost)`. Opt-in; sin `MagicReserves` es gratis (prototipado).
/// </summary>
public class ElementalSpell : MonoBehaviour
{
    public Anima caster;
    public SpellElement element = SpellElement.Water;
    [Tooltip("Masa proyectada (g). Agua ~1000; tierra ~5000; viento = aire (gratis).")]
    public float matterGrams = 1000f;
    [Tooltip("Velocidad de proyección (m/s) → energía cinética ½·m·v².")]
    public float velocity = 20f;
    [Tooltip("La materia es aire ambiente (gratis) → el hechizo solo cuesta energía (viento).")]
    public bool matterFromAir = false;

    void Awake() { if (caster == null) caster = GetComponent<Anima>(); }

    /// <summary>Energía cinética a pagar del pool (J) = ½·m·v².</summary>
    public float EnergyCost => 0.5f * (Mathf.Max(0f, matterGrams) / 1000f) * velocity * velocity;

    /// <summary>Presets por elemento (materia/velocidad/aire).</summary>
    public void SetElement(SpellElement e)
    {
        element = e;
        switch (e)
        {
            case SpellElement.Water: matterGrams = 1000f; velocity = 20f; matterFromAir = false; break;  // 1 L a 20 m/s
            case SpellElement.Earth: matterGrams = 5000f; velocity = 10f; matterFromAir = false; break;  // 5 kg de roca
            case SpellElement.Wind:  matterGrams = 1000f; velocity = 30f; matterFromAir = true;  break;  // 1 kg de aire, gratis
        }
    }

    List<ElementCost> BuildCost()
    {
        List<ElementCost> c = new List<ElementCost>();
        if (matterFromAir || matterGrams <= 0f) return c;   // viento: sin coste de materia
        float g = matterGrams;
        switch (element)
        {
            case SpellElement.Water:   // H₂O: 11,19% H + 88,81% O (por masa)
                c.Add(new ElementCost { symbol = "H", amount = g * 0.1119f });
                c.Add(new ElementCost { symbol = "O", amount = g * 0.8881f });
                break;
            case SpellElement.Earth:   // SiO₂: 46,74% Si + 53,26% O
                c.Add(new ElementCost { symbol = "Si", amount = g * 0.4674f });
                c.Add(new ElementCost { symbol = "O",  amount = g * 0.5326f });
                break;
            case SpellElement.Wind:
                break;
        }
        return c;
    }

    /// <summary>Lanza el hechizo si hay reservas (materia + energía). Devuelve si salió.</summary>
    public bool Cast()
    {
        MagicReserves mr = caster != null ? caster.GetComponent<MagicReserves>() : GetComponent<MagicReserves>();
        List<ElementCost> cost = BuildCost();
        float e = EnergyCost;
        if (mr != null && !mr.Pay(cost, e))
        {
            Debug.Log($"[{element}] «{name}» sin reservas: {DescribeCost(cost)} + {e:0} J.");
            return false;
        }
        if (caster != null) caster.GetComponent<MagicAura>()?.RegisterDestructiveUse(Mathf.Log10(e + 1f) * 0.5f);
        Debug.Log($"[{element}] «{name}» lanza {element}: " +
                  $"{(matterFromAir ? "aire (gratis)" : $"{matterGrams:0} g")} a {velocity:0} m/s = {e:0} J. {DescribeCost(cost)}.");
        return true;
    }

    static string DescribeCost(List<ElementCost> cost)
    {
        if (cost == null || cost.Count == 0) return "sin materia";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (ElementCost c in cost) sb.Append($"{c.symbol} {c.amount:0.#}g  ");
        return sb.ToString().TrimEnd();
    }
}
