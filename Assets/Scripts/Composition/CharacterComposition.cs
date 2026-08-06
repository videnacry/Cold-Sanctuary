using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Una PARTE de composición (docs/stats-as-truth.md §5): adorno o prenda en un slot, con su **visual** y su
/// **aporte de stats**. Fase 1: adornos (0 stats) y ropa (defensa → `Anima.armadura`). Puede referenciar una
/// `ClothingRecipe` (toma su `slot` y `defenseRating`) o dar defensa/slot a mano.
/// </summary>
[System.Serializable]
public class CompositionPart
{
    public string label = "Parte";
    public ClothingSlot slot = ClothingSlot.Accessory;
    [Tooltip("Malla a activar (opcional; los modelos viven fuera del repo).")]
    public GameObject visual;
    [Tooltip("Prenda opcional: si se asigna, usa su slot y aporta su defenseRating.")]
    public ClothingRecipe clothing;
    [Tooltip("Defensa extra directa (si no usas una prenda).")]
    public float extraDefense = 0f;

    public ClothingSlot Slot => clothing != null ? clothing.slot : slot;
    public float Defense => (clothing != null ? clothing.defenseRating : 0f) + extraDefense;
}

/// <summary>
/// COMPOSICIÓN de un ser (docs/stats-as-truth.md §5, **fase 1**): partes slotables (adornos/ropa) que dan
/// **apariencia** (activan su malla) y **stats** — la defensa de la ropa suma a **`Anima.armadura`**, que
/// `Predation` ya lee (vestir armadura = peor presa / más defensa). **No toca el modelo general de stats**
/// (eso es la fase 2: base vs efectivo); aquí solo cachea `armadura = base + Σ defensa`. Un slot = una parte.
/// Reutiliza `ClothingSlot`/`ClothingRecipe`. En `Anima`s reales (jugador/companions/animales).
/// </summary>
public class CharacterComposition : MonoBehaviour
{
    public Anima anima;
    public List<CompositionPart> parts = new List<CompositionPart>();

    float _baseArmadura;
    bool _captured;

    void Awake()
    {
        if (anima == null) anima = GetComponent<Anima>();
        if (anima != null) { _baseArmadura = anima.armadura; _captured = true; }
    }

    void Start() { foreach (CompositionPart p in parts) SetVisible(p, true); Recompute(); }

    /// <summary>Equipa una parte en su slot (reemplaza la que hubiera en ese slot).</summary>
    public void Equip(CompositionPart part)
    {
        if (part == null) return;
        for (int i = parts.Count - 1; i >= 0; i--)
            if (parts[i] != null && parts[i].Slot == part.Slot) { SetVisible(parts[i], false); parts.RemoveAt(i); }
        parts.Add(part);
        SetVisible(part, true);
        Recompute();
    }

    public void Unequip(CompositionPart part)
    {
        if (part == null || !parts.Remove(part)) return;
        SetVisible(part, false);
        Recompute();
    }

    static void SetVisible(CompositionPart p, bool on)
    {
        if (p != null && p.visual != null) p.visual.SetActive(on);
    }

    /// <summary>`armadura = base + Σ defensa de la ropa equipada`. No toca el resto de stats (fase 2).</summary>
    public void Recompute()
    {
        if (anima == null) return;
        if (!_captured) { _baseArmadura = anima.armadura; _captured = true; }
        float armor = _baseArmadura;
        foreach (CompositionPart p in parts) if (p != null) armor += p.Defense;
        anima.armadura = armor;
    }
}
