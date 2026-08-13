using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ALMA por MEZCLA (docs/soul-composition-blend.md) — FASE 1. Compone el ser a partir de arquetipos de **cuerpo**
/// (físico + tamaño) y **mente** (mental + tono) con **dominio (%)** + `shareDomain`, más **`bonusPacks`** (stats
/// aditivos que NO tocan personalidad). `Resolve()` computa la mezcla y **escribe las 12 aptitudes** en el `Anima`
/// (físicas ← cuerpos, mentales ← mentes) + el **tamaño** (altura mezclada) + suma los packs.
///   `aptitud = blend(cuerpos)[físicas] + blend(mentes)[mentales] + Σ bonusPacks`
/// Opt-in; sustituye a configurar stats a mano. *Falta (fases siguientes):* tono/thoughts a la `Mind`, especies
/// como cuerpos reutilizando la automatización de `Animal`, disolver `CompanionBase`, semilla de `FamilyGenerator`.
/// </summary>
public class SoulComposition : MonoBehaviour
{
    public Anima anima;

    [Header("Cuerpos (físico + tamaño): arquetipo + dominio %")]
    public List<BlendSlot> bodies = new List<BlendSlot>();
    [Header("Mentes (mental + tono): arquetipo + dominio %")]
    public List<BlendSlot> minds = new List<BlendSlot>();
    [Header("bonusPacks (stats aditivos; NO tocan tono/thoughts)")]
    public List<string> bonusPacks = new List<string>();

    [Tooltip("Aplica el tamaño (altura mezclada) al transform.")]
    public bool applyScale = true;
    [Tooltip("Resuelve en Start (crea los stats iniciales).")]
    public bool resolveOnStart = true;

    void Awake() { if (anima == null) anima = GetComponent<Anima>(); }
    void Start() { if (resolveOnStart) Resolve(); }

    /// <summary>Computa la mezcla y escribe las aptitudes/tamaño en el Anima.</summary>
    public void Resolve()
    {
        if (anima == null) anima = GetComponent<Anima>();
        if (anima == null) return;

        Aptitudes b = Blend(bodies, true, out float height, out _);
        Aptitudes m = Blend(minds, false, out _, out ElementalTone tone);

        // Físicas ← cuerpo, mentales ← mente.
        anima.agility = b.agility; anima.perception = b.perception; anima.strength = b.strength;
        anima.bodyMass = b.bodyMass; anima.endurance = b.endurance; anima.adaptability = b.adaptability;
        anima.composure = m.composure; anima.reasoning = m.reasoning; anima.memory = m.memory;
        anima.creativity = m.creativity; anima.sociability = m.sociability; anima.discipline = m.discipline;

        // bonusPacks: aditivo a TODAS las aptitudes (no tocan tono/thoughts).
        foreach (string p in bonusPacks)
            if (Archetypes.TryPack(p, out Aptitudes pk))
            {
                anima.agility += pk.agility; anima.perception += pk.perception; anima.strength += pk.strength;
                anima.bodyMass += pk.bodyMass; anima.adaptability += pk.adaptability; anima.composure += pk.composure;
                anima.endurance += pk.endurance; anima.reasoning += pk.reasoning; anima.memory += pk.memory;
                anima.creativity += pk.creativity; anima.sociability += pk.sociability; anima.discipline += pk.discipline;
            }

        if (applyScale && height > 0f) transform.localScale = Vector3.one * height;

        Debug.Log($"[Alma] «{anima.name}»: cuerpo(str {anima.strength:0.00} masa {anima.bodyMass:0.00} agi {anima.agility:0.00}) " +
                  $"mente(razón {anima.reasoning:0.00} creat {anima.creativity:0.00}) tono {tone} altura {height:0.00}" +
                  (bonusPacks.Count > 0 ? $" +{bonusPacks.Count} pack(s)" : "") + ".");
    }

    // Mezcla ponderada de una lista de ranuras. `asBody` decide si se leen arquetipos de cuerpo o de mente.
    // Devuelve las aptitudes mezcladas; `height` (cuerpos) y `tone` (mente dominante) como extras.
    Aptitudes Blend(List<BlendSlot> slots, bool asBody, out float height, out ElementalTone tone)
    {
        height = 1f;
        tone = ElementalTone.Tierra;
        if (slots == null || slots.Count == 0) return Aptitudes.Default;

        float sumExplicit = 0f;
        int shareCount = 0;
        foreach (BlendSlot s in slots)
        {
            if (s == null) continue;
            if (s.shareDomain) shareCount++;
            else sumExplicit += Mathf.Max(0f, s.domain);
        }
        float remainder = Mathf.Max(0f, 100f - sumExplicit);
        float sharePer = shareCount > 0 ? remainder / shareCount : 0f;
        float total = sumExplicit + sharePer * shareCount;
        if (total <= 0f) return Aptitudes.Default;

        Aptitudes result = new Aptitudes();
        float h = 0f, bestW = -1f;
        foreach (BlendSlot s in slots)
        {
            if (s == null) continue;
            float w = (s.shareDomain ? sharePer : Mathf.Max(0f, s.domain)) / total;
            if (w <= 0f) continue;
            ArchetypeProfile p = asBody ? Archetypes.BodyOf(s.archetype) : Archetypes.MindOf(s.archetype);
            Aptitudes a = p.aptitudes;
            result.agility += w * a.agility; result.perception += w * a.perception; result.strength += w * a.strength;
            result.bodyMass += w * a.bodyMass; result.adaptability += w * a.adaptability; result.composure += w * a.composure;
            result.endurance += w * a.endurance; result.reasoning += w * a.reasoning; result.memory += w * a.memory;
            result.creativity += w * a.creativity; result.sociability += w * a.sociability; result.discipline += w * a.discipline;
            h += w * p.height;
            if (w > bestW) { bestW = w; tone = p.tone; }
        }
        if (asBody) height = h;
        return result;
    }
}
