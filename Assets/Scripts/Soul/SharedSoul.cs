using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ALMA COMPARTIDA entre reencarnaciones (docs/soul-relations-reincarnation.md §4). Varios cuerpos
/// (`SoulComposition` en distintas eras) referencian **una sola alma**: comparten **forma** (identidad),
/// **poder** (magnitud) y **bonds**. Lo que uno gana/pierde (poder o forma) se **propaga** a todos → "conectados
/// desafiando al tiempo y el espacio". El alma guarda la forma **canónica** (distribución normalizada) + un
/// escalar de **poder**; cada cuerpo la convierte a su presupuesto (modo **Literal/B** = identidad marcada).
/// Rendimiento: en runtime solo conviene tener activos los cuerpos de la **era visitada** (los demás propagan
/// perezosamente al re-registrarse).
/// </summary>
public class SharedSoul : MonoBehaviour
{
    public string soulName = "Ambrosio";

    [System.NonSerialized] public Aptitudes shape;   // forma canónica (distribución, presupuesto CanonBudget)
    [Tooltip("Magnitud/poder acumulado del alma (× sobre la base del cuerpo). Sube al entrenar, baja al lesionarse.")]
    public float power = 1f;
    [Tooltip("Bonds acumulados por TODAS las reencarnaciones (Medea vincula con Ruth → todas las Medeas la tienen).")]
    public List<string> sharedBonds = new List<string>();

    readonly List<SoulComposition> _bodies = new List<SoulComposition>();
    bool _seeded;

    const float CanonBudget = 12f;

    /// <summary>Registra un cuerpo (lo siembra si es el primero) y le aplica la identidad compartida.</summary>
    public void Register(SoulComposition body)
    {
        if (body == null || _bodies.Contains(body)) return;
        if (!_seeded) { SeedFrom(body); _seeded = true; }
        _bodies.Add(body);
        ApplyTo(body);
    }

    void SeedFrom(SoulComposition body)
    {
        Aptitudes s = body.ReadStats();
        shape = SoulMath.RescaleShape(s, SoulMath.All, CanonBudget);
        float baseB = SoulMath.Budget(body.ComputeBaseStats(), SoulMath.All);
        float curB = SoulMath.Budget(s, SoulMath.All);
        power = baseB > 0f ? curB / baseB : 1f;
    }

    /// <summary>Escribe en el cuerpo la FORMA del alma a su presupuesto (Literal) × el PODER.</summary>
    public void ApplyTo(SoulComposition body)
    {
        if (body == null || body.anima == null) return;
        Aptitudes baseB = body.ComputeBaseStats();
        Aptitudes local = SoulMath.Remap(shape, baseB, ConversionMode.Literal);
        local = SoulMath.Scale(local, power);
        body.WriteStats(local);
    }

    /// <summary>Un cuerpo gana/pierde PODER (entrenar/lesionarse) → se propaga a TODAS las reencarnaciones.</summary>
    public void GainPower(float delta)
    {
        power = Mathf.Max(0.05f, power + delta);
        Repropagate();
        Debug.Log($"[AlmaCompartida] «{soulName}» poder → {power:0.00} (propagado a {_bodies.Count} cuerpo(s)).");
    }

    /// <summary>Un cuerpo cambió de FORMA (entrenó algo concreto) → actualiza la forma canónica y propaga.</summary>
    public void ReshapeFrom(SoulComposition body)
    {
        if (body == null) return;
        shape = SoulMath.RescaleShape(body.ReadStats(), SoulMath.All, CanonBudget);
        Repropagate();
    }

    /// <summary>Añade un bond al alma → lo tienen TODAS las reencarnaciones.</summary>
    public void AddBond(string who)
    {
        if (string.IsNullOrEmpty(who) || sharedBonds.Contains(who)) return;
        sharedBonds.Add(who);
        Debug.Log($"[AlmaCompartida] «{soulName}» +bond «{who}» (ahora {sharedBonds.Count}; compartido por todas las reencarnaciones).");
    }

    public void Repropagate() { foreach (SoulComposition b in _bodies) ApplyTo(b); }
    public int BodyCount => _bodies.Count;
}
