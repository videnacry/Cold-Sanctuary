using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ALMA por MEZCLA (docs/soul-composition-blend.md + soul-relations-reincarnation.md §1). Compone el ser desde
/// arquetipos de **cuerpo** (físico + tamaño) y **mente** (mental + tono) con **dominio (%)** + `shareDomain`, más
/// **`bonusPacks`** (stats aditivos que NO tocan personalidad). El blend es por **DISTRIBUCIÓN**: cada arquetipo
/// se reescala al presupuesto del **primario** conservando su forma → un 1% empuja la forma un 1% (no despreciable).
///   `aptitud = blend(cuerpos)[físicas] + blend(mentes)[mentales] + Σ bonusPacks`
/// <see cref="ConvertTo"/> = **transformación/reencarnación**: reexpresa la identidad actual en un cuerpo nuevo
/// (modo Literal B = forma exacta; Relative A = modulada por el cuerpo nuevo). Opt-in.
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

    [Tooltip("Si se asigna, este cuerpo comparte un ALMA con otras reencarnaciones (docs soul-relations §4).")]
    public SharedSoul sharedSoul;

    void Awake() { if (anima == null) anima = GetComponent<Anima>(); }
    void Start()
    {
        if (resolveOnStart) Resolve();          // base desde arquetipos
        if (sharedSoul != null) sharedSoul.Register(this);   // el alma compartida sobrescribe con la identidad común
    }

    /// <summary>La base mezclada (sin bonusPacks) — para el alma compartida y la conversión.</summary>
    public Aptitudes ComputeBaseStats() => ComputeBase(out _, out _);

    /// <summary>Computa la mezcla (por distribución) y escribe las aptitudes/tamaño en el Anima + los bonusPacks.</summary>
    public void Resolve()
    {
        if (anima == null) anima = GetComponent<Anima>();
        if (anima == null) return;

        Aptitudes final = ComputeBase(out float height, out ElementalTone tone);
        AddPacks(ref final);
        WriteStats(final);
        if (applyScale && height > 0f) transform.localScale = Vector3.one * height;

        Debug.Log($"[Alma] «{anima.name}»: str {final.strength:0.00} masa {final.bodyMass:0.00} agi {final.agility:0.00} " +
                  $"razón {final.reasoning:0.00} creat {final.creativity:0.00} · tono {tone} altura {height:0.00}" +
                  (bonusPacks.Count > 0 ? $" +{bonusPacks.Count} pack(s)" : "") + ".");
    }

    /// <summary>CONVERSIÓN a un cuerpo/mente nuevos (transformación/reencarnación). `mode` = A (Relative) / B (Literal).
    /// Conserva la IDENTIDAD (distribución actual) reexpresada en la nueva base. Cambia los arquetipos del ser.</summary>
    public void ConvertTo(string newBody, string newMind, ConversionMode mode)
    {
        if (anima == null) anima = GetComponent<Anima>();
        if (anima == null) return;

        Aptitudes current = ReadStats();
        bodies = new List<BlendSlot> { new BlendSlot { archetype = newBody, domain = 100f } };
        minds  = new List<BlendSlot> { new BlendSlot { archetype = newMind, domain = 100f } };

        Aptitudes newBase = ComputeBase(out float height, out ElementalTone tone);
        Aptitudes converted = SoulMath.Remap(current, newBase, mode);
        WriteStats(converted);
        if (applyScale && height > 0f) transform.localScale = Vector3.one * height;

        Debug.Log($"[Alma] «{anima.name}» CONVERSIÓN {(mode == ConversionMode.Literal ? "B/literal" : "A/relativa")} → " +
                  $"{newBody}+{newMind}: str {converted.strength:0.00} masa {converted.bodyMass:0.00} agi {converted.agility:0.00} " +
                  $"razón {converted.reasoning:0.00} · tono {tone} altura {height:0.00}.");
    }

    // ── stats ↔ Anima ──────────────────────────────────────────────────────────
    public Aptitudes ReadStats()
    {
        Aptitudes a = new Aptitudes();
        a.agility = anima.agility; a.perception = anima.perception; a.strength = anima.strength; a.bodyMass = anima.bodyMass;
        a.adaptability = anima.adaptability; a.composure = anima.composure; a.endurance = anima.endurance; a.reasoning = anima.reasoning;
        a.memory = anima.memory; a.creativity = anima.creativity; a.sociability = anima.sociability; a.discipline = anima.discipline;
        return a;
    }

    public void WriteStats(Aptitudes a)
    {
        anima.agility = a.agility; anima.perception = a.perception; anima.strength = a.strength; anima.bodyMass = a.bodyMass;
        anima.adaptability = a.adaptability; anima.composure = a.composure; anima.endurance = a.endurance; anima.reasoning = a.reasoning;
        anima.memory = a.memory; anima.creativity = a.creativity; anima.sociability = a.sociability; anima.discipline = a.discipline;

        // FASE 2 (mente por blend): la Mente lee las aptitudes del blend → su tono/decisiones emergen de aquí
        // (Mind.PickTone deriva el tono de las aptitudes; se resiembra porque su Awake corrió antes del Resolve).
        Mind mind = GetComponent<Mind>();
        if (mind != null) mind.aptitudes = a;
    }

    // ── blend por distribución ─────────────────────────────────────────────────
    // Base (sin packs): físicas ← blend(cuerpos), mentales ← blend(mentes).
    Aptitudes ComputeBase(out float height, out ElementalTone tone)
    {
        Aptitudes body = BlendPillar(bodies, true, SoulMath.Physical, out height, out _);
        Aptitudes mind = BlendPillar(minds, false, SoulMath.Mental, out _, out tone);
        Aptitudes final = new Aptitudes();
        foreach (AptitudeKind k in SoulMath.Physical) final.Add(k, body.Get(k));
        foreach (AptitudeKind k in SoulMath.Mental)   final.Add(k, mind.Get(k));
        return final;
    }

    void AddPacks(ref Aptitudes a)
    {
        foreach (string p in bonusPacks)
            if (Archetypes.TryPack(p, out Aptitudes pk))
                foreach (AptitudeKind k in SoulMath.All) a.Add(k, pk.Get(k));
    }

    // Mezcla por distribución de una lista de ranuras sobre `kinds`. `asBody` decide cuerpo vs mente.
    // Cada arquetipo se reescala al presupuesto del PRIMARIO (mayor dominio) conservando su forma, y se pondera.
    Aptitudes BlendPillar(List<BlendSlot> slots, bool asBody, AptitudeKind[] kinds, out float height, out ElementalTone tone)
    {
        height = 1f; tone = ElementalTone.Tierra;
        if (slots == null || slots.Count == 0) return Aptitudes.Default;

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
        if (total <= 0f) return Aptitudes.Default;

        // Pass 1: primario (mayor peso) → presupuesto de referencia + tono.
        float bestW = -1f; ArchetypeProfile primary = null;
        foreach (BlendSlot s in slots)
        {
            if (s == null) continue;
            float w = s.shareDomain ? sharePer : Mathf.Max(0f, s.domain);
            if (w <= 0f) continue;
            ArchetypeProfile p = asBody ? Archetypes.BodyOf(s.archetype) : Archetypes.MindOf(s.archetype);
            if (w > bestW) { bestW = w; primary = p; }
        }
        if (primary == null) return Aptitudes.Default;
        float refBudget = SoulMath.Budget(primary.aptitudes, kinds);
        tone = primary.tone;
        if (refBudget <= 0f) return Aptitudes.Default;

        // Pass 2: mezclar formas al presupuesto de referencia, ponderadas.
        Aptitudes result = new Aptitudes();
        float h = 0f;
        foreach (BlendSlot s in slots)
        {
            if (s == null) continue;
            float w = (s.shareDomain ? sharePer : Mathf.Max(0f, s.domain)) / total;
            if (w <= 0f) continue;
            ArchetypeProfile p = asBody ? Archetypes.BodyOf(s.archetype) : Archetypes.MindOf(s.archetype);
            Aptitudes rescaled = SoulMath.RescaleShape(p.aptitudes, kinds, refBudget);
            foreach (AptitudeKind k in kinds) result.Add(k, w * rescaled.Get(k));
            h += w * p.height;
        }
        if (asBody) height = h;
        return result;
    }
}
