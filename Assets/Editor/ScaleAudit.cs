using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AUDITORÍA DE ESCALA (menú): mide en la escena abierta el largo de cada especie (adultos, por los bounds de sus
/// renderers) y lo compara con su tamaño real respecto al HUMANO (≈ 1,75 m). Avisa de lo que se sale (×0,7–×1,3).
/// Excepción a propósito: las abejas y arañas de hielo (no son reales) miden como una CABEZA (≈ 25–30 cm).
/// En los microcosmos (2026-11-18, docs/plan-microcosmos-realismo.md §3.1) la referencia es «humano × s»: el insecto-humano
/// (el mirmidón) mide la estatura de una persona × MicroBuildRules.FolkScale, y el mundo (muebles, edificios, árboles) va a 1:1.
/// Se audita también cada insecto-humano de la escena: su estatura frente a 1,6–1,85 m × s y su ancho andando con las patas
/// recogidas frente a MicroBuildRules.TuckedWalkWidth (CubeInsect.Footprint).
/// </summary>
public static class ScaleAudit
{
    // Largo real del adulto (m): cabeza a cola o, en los erguidos (pingüino), la altura. [A] aproximaciones de manual.
    static Dictionary<string, float> Real => _real;
    static readonly Dictionary<string, float> _real = new Dictionary<string, float>(SpeciesScaleFix.Real)
    {
        ["Bear"] = 2.2f, ["PolarBear"] = 2.2f, ["Seal"] = 1.6f, ["Penguin"] = 0.6f, ["Whale"] = 15f, ["Orca"] = 7f,
        ["SnowPetrel"] = 0.78f, ["ArcticTern"] = 0.8f, ["WanderingAlbatross"] = 3f, ["CapePetrel"] = 0.85f, ["PolarSkua"] = 1.35f, ["EmperorPenguin"] = 1.0f, ["Walrus"] = 3.0f, ["Reindeer"] = 2.0f, ["MuskOx"] = 2.3f, ["ArcticHare"] = 0.62f, ["ArcticFox"] = 0.85f,
        ["Wolf"] = 1.4f, ["Malamute"] = 1.0f, ["Fox"] = 0.65f, ["Bunny"] = 0.6f, ["Deer"] = 1.9f, ["Meerkat"] = 0.3f,
        ["SnowBee"] = 0.25f, ["SnowSpider"] = 0.3f,   // de hielo (ficticias): del tamaño de una cabeza
    };

    [MenuItem("Tools/Cold Sanctuary/Auditoría de escala (animales vs humano)")]
    public static void Run()
    {
        var by = new Dictionary<string, List<float>>();
        foreach (Animal a in Object.FindObjectsByType<Animal>(FindObjectsSortMode.None))
        {
            if (a == null || a.lifeStage != LifeStage.adult) continue;
            float len = Length(a.gameObject); if (len <= 0f) continue;
            string sp = a.SpeciesName;
            if (!by.ContainsKey(sp)) by[sp] = new List<float>();
            by[sp].Add(len);
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        float human = player != null ? Height(player) : 1.75f;
        Debug.Log($"[Escala] Humano de referencia: {human:0.00} m de alto ({by.Count} especies con adultos en la escena).");
        int off = 0;
        foreach (var kv in by)
        {
            float avg = 0f; foreach (float x in kv.Value) avg += x; avg /= kv.Value.Count;
            string verdict = "sin referencia";
            if (Real.TryGetValue(kv.Key, out float target))
            {
                float k = avg / target;
                verdict = k > 1.3f ? $"GRANDE ×{k:0.0} (real {target:0.00} m)" : k < 0.7f ? $"PEQUEÑO ×{k:0.0} (real {target:0.00} m)" : $"ok ×{k:0.0}";
                if (k > 1.3f || k < 0.7f) off++;
            }
            Debug.Log($"[Escala] {kv.Key}: {avg:0.00} m de largo de media ({kv.Value.Count} adultos) → {verdict}");
        }
        Debug.Log($"[Escala] {off} especies fuera de escala.");
        AuditFolk();
    }

    // LOS INSECTOS-HUMANOS (humano × s): estatura y ancho andando con las patas recogidas.
    static void AuditFolk()
    {
        float s = MicroBuildRules.FolkScale, lo = 1.6f * s - 0.1f, hi = 1.85f * s + 0.15f;
        int n = 0, tall = 0, wide = 0; float sumH = 0f, sumW = 0f;
        foreach (CubeInsect ci in Object.FindObjectsByType<CubeInsect>(FindObjectsSortMode.None))
        {
            if (ci == null || !ci.folk || ci.GetComponentInParent<MicroPiece>() != null) continue;   // (las estatuas son monumentos)
            float h = Height(ci.gameObject), w = ci.Footprint(true).x;
            n++; sumH += h; sumW += w;
            if (h < lo || h > hi) { tall++; if (tall <= 10) Debug.LogWarning($"[Escala] «{ci.name}» ({ci.species}) mide {h:0.00} m (humano × s: {1.6f * s:0.00}–{1.85f * s:0.00} m)."); }
            if (w > MicroBuildRules.TuckedWalkWidth + 0.05f && ci.species == "Ant") { wide++; if (wide <= 10) Debug.LogWarning($"[Escala] «{ci.name}» anda con {w:0.00} m de ancho (> {MicroBuildRules.TuckedWalkWidth:0.00} recogido)."); }
        }
        if (n > 0) Debug.Log($"[Escala] Insectos-humanos (s = {s}): {n}, de {sumH / n:0.00} m de alto y {sumW / n:0.00} m de ancho andando de media; {tall} fuera de estatura, {wide} hormigas más anchas de lo recogido.");
    }

    static float Length(GameObject g)
    {
        bool any = false; Bounds b = default;
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>()) { if (!r.enabled || r.name == "Etiqueta") continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return any ? Mathf.Max(b.size.x, b.size.y, b.size.z) : 0f;
    }

    static float Height(GameObject g)
    {
        bool any = false; Bounds b = default;
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>()) { if (!r.enabled) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return any ? b.size.y : 1.75f;
    }
}
