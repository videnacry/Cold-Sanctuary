using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// (2026-11-11) PETICIONES DE BERÓN QUE LA SONDA DE LA RONDA 65 VIO SIN CUMPLIR, en la escena construida:
/// #58 alfombras gruesas (tecnología con circuitos dentro: ≥ 8 cm de canto), #4 muebles para gente de 1,85 m (camas y camillas ≥ 2,00 m),
/// #152 todo opaco y mate (suavidad ≤ 0,2; transparente solo el cristal o el agua), #2 todos los pisos de un mismo edificio del mismo tamaño,
/// #1 sin escaleras, puentes ni pretiles de borde (las barandas de las camas y las barras de ballet sí).
/// </summary>
public class PeticionesSondaTest : MonoBehaviour, ITestUnit
{
    public int Group => 117;
    public bool ParallelSafe => true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<PeticionesSondaTest>() == null) tr.gameObject.AddComponent<PeticionesSondaTest>();
    }

    static Bounds LocalBounds(Transform root)
    {
        bool any = false; Bounds b = new Bounds();
        foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;
            Bounds mb = mf.sharedMesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 c = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                Vector3 p = root.InverseTransformPoint(mf.transform.TransformPoint(c));
                if (!any) { b = new Bounds(p, Vector3.zero); any = true; } else b.Encapsulate(p);
            }
        }
        return b;
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Peticiones de la sonda: alfombras gruesas, camas de 2 m, todo mate, pisos iguales, sin pretiles");
        if (FindAnyObjectByType<Territorio>() == null) { Debug.Log("[TEST] SKIP · Peticiones de la sonda — esta escena no es el santuario"); TestProbe.End(); yield break; }
        yield return null;

        // #58 ALFOMBRAS GRUESAS
        var carpets = FindObjectsByType<FlyingCarpet>(FindObjectsSortMode.None).Where(c => c != null).ToList();
        var thin = carpets.Where(c => LocalBounds(c.transform).size.y < 0.08f).Select(c => $"{c.name} {LocalBounds(c.transform).size.y:0.000}").ToList();
        TestProbe.Check("#58 todas las alfombras tienen ≥ 8 cm de canto (con su núcleo de circuitos)", carpets.Count > 0 && thin.Count == 0,
            $"{carpets.Count} alfombras{(thin.Count > 0 ? "; finas: " + string.Join(", ", thin.Take(6)) : "")}");
        TestProbe.Check("#58 con su núcleo de circuitos a la vista", carpets.All(c => c.transform.Find("Nucleo_circuitos") != null));

        // #4 CAMAS Y CAMILLAS ≥ 2,00 m (no las cámaras frías ni la colchoneta de los cachorros)
        var beds = FindObjectsByType<Transform>(FindObjectsSortMode.None)
            .Where(t => t != null && (t.name == "Cama" || t.name.StartsWith("Cama ") || t.name.StartsWith("Cama_") || t.name.StartsWith("Camilla")) && !t.name.StartsWith("Camara")
                        && (t.parent == null || !(t.parent.name.StartsWith("Cama") || t.parent.name.StartsWith("Camilla"))))
            .ToList();
        var shortBeds = beds.Select(t => (t, len: Mathf.Max(LocalBounds(t).size.x, LocalBounds(t).size.z))).Where(x => x.len < 2.0f - 1e-3f).Select(x => $"{x.t.name} {x.len:0.00}").ToList();
        TestProbe.Check("#4 camas y camillas de ≥ 2,00 m (gente de 1,85 m)", beds.Count > 0 && shortBeds.Count == 0, $"{beds.Count} camas y camillas{(shortBeds.Count > 0 ? "; cortas: " + string.Join(", ", shortBeds.Take(8)) : "")}");

        // #152 TODO MATE Y OPACO (transparente solo cristal o agua: alfa < 0,99)
        var mats = new HashSet<Material>();
        foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None)) if (r != null) foreach (Material m in r.sharedMaterials) if (m != null) mats.Add(m);
        var shiny = mats.Where(m => m.HasProperty("_Smoothness") && m.GetFloat("_Smoothness") > 0.2f && m.shader != null && (m.shader.name.Contains("Lit") || m.shader.name == "Standard")).Select(m => m.name).ToList();
        var fakeGlass = mats.Where(m => m.HasProperty("_Surface") && m.GetFloat("_Surface") > 0.5f && (m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.color).a >= 0.99f).Select(m => m.name).ToList();
        TestProbe.Check("#152 ningún material brillante (suavidad ≤ 0,2)", shiny.Count == 0, $"{mats.Count} materiales{(shiny.Count > 0 ? $"; brillantes {shiny.Count}: " + string.Join(", ", shiny.Take(6)) : "")}");
        TestProbe.Check("#152 transparente solo el cristal y el agua (alfa < 0,99)", fakeGlass.Count == 0, fakeGlass.Count > 0 ? string.Join(", ", fakeGlass.Take(6)) : "");

        // #2 LOS PISOS DE UN EDIFICIO, DEL MISMO TAMAÑO
        var unequal = new List<string>();
        foreach (ArbolCampos t in ArbolCampos.All)
        {
            if (t == null) continue;
            var sizes = t.levels.Where(l => l != null && l.kind == TreeLevel.Kind.Floor && l.deck != null).Select(l => Mathf.Round(l.half * 2f * 10f) / 10f).Distinct().ToList();
            if (sizes.Count > 1) unequal.Add($"{t.building}: {string.Join(" / ", sizes.Select(s => $"{s:0.0} m"))}");
        }
        TestProbe.Check("#2 todos los pisos de cada edificio del mismo tamaño", unequal.Count == 0, string.Join("; ", unequal));

        // #1 SIN PRETILES DE BORDE (las barandas de las camas y las barras de ballet son muebles)
        string[] edge = { "Pretil", "Barandilla_borde", "Baranda_borde", "Escalera_", "Puente_" };
        string[] okNames = { "Escalera_tijera", "Escalera_horizontal", "Puente_piscina" };
        var barriers = FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t != null && edge.Any(e => t.name.StartsWith(e)) && !okNames.Any(o => t.name.StartsWith(o)))
            .Select(t => t.name).ToList();
        TestProbe.Check("#1 sin pretiles ni barandillas de borde, ni escaleras ni puentes", barriers.Count == 0, string.Join(", ", barriers.GroupBy(n => n).Select(g => $"{g.Key} ×{g.Count()}")));
        TestProbe.End();
    }
}
