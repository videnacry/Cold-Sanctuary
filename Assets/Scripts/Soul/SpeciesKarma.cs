using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resuelve la RELACIÓN kármica base de un ser hacia una especie (docs/soul-relations-reincarnation §2). Es la
/// **condición inicial** del bond (la evolución/karma): 0 = neutro (especie nueva, p.ej. lobo↔dragón de Komodo),
/// + agrado, − desagrado (foca↔oso −, perro↔humano +). NO reemplaza al **threat** (que es por PODER, en `Animal`):
/// el lobo huye del komodo por fuerza aunque la relación sea 0. Se mezcla por dominio los `speciesBonds` del ser
/// (compuesto) o, si no tiene, se usa su especie directa (`SpeciesName`).
/// </summary>
public static class SpeciesKarma
{
    /// <summary>Relación base de `me` hacia `otherSpecies` (signed).</summary>
    public static float RelationOf(Anima me, string otherSpecies)
    {
        if (me == null || string.IsNullOrEmpty(otherSpecies)) return 0f;

        SoulComposition sc = me.GetComponent<SoulComposition>();
        if (sc != null && sc.speciesBonds != null && sc.speciesBonds.Count > 0)
            return BlendRelations(sc.speciesBonds, otherSpecies);

        return Archetypes.RelationValue(me.SpeciesName, otherSpecies);   // especie directa (animales)
    }

    // Mezcla por dominio (+ shareDomain) de los mapas de relación de los speciesBonds.
    static float BlendRelations(List<BlendSlot> slots, string otherSpecies)
    {
        float sumExplicit = 0f; int shareCount = 0;
        foreach (BlendSlot s in slots)
        {
            if (s == null) continue;
            if (s.shareDomain) shareCount++;
            else sumExplicit += Mathf.Max(0f, s.domain);
        }
        float remainder = Mathf.Max(0f, 100f - sumExplicit);
        float sharePer = shareCount > 0 ? remainder / shareCount : 0f;
        float total = sumExplicit + sharePer * shareCount;
        if (total <= 0f) return 0f;

        float sum = 0f;
        foreach (BlendSlot s in slots)
        {
            if (s == null) continue;
            float w = (s.shareDomain ? sharePer : Mathf.Max(0f, s.domain)) / total;
            if (w <= 0f) continue;
            sum += w * Archetypes.RelationValue(s.archetype, otherSpecies);
        }
        return sum;
    }
}
