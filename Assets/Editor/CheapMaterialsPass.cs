using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// (2026-11-11) TODO OPACO Y BARATO EN LOS ASSETS (petición #152 de Berón, sonda de la ronda 65: 431 materiales con brillo &gt; 0,2 —«Cubo_621219»…—
/// y 33 transparentes). <see cref="CheapMaterials"/> solo convierte al cargar la escena los materiales que NO son assets; los assets (los
/// «Cubo_» de Assets/Generated/CubeMats que hicieron los prefabs de animales antes del 2026-11-09, y los demás del proyecto) se quedaban como
/// estaban. Esta pasada los convierte en el editor y los guarda: Simple Lit, suavidad 0,1, sin metal ni reflejos. Sigue transparente SOLO lo que
/// es cristal o agua de verdad: un color con alfa &lt; 0,99 (cristales, el acrílico de las raíces y de los tanques, el agua, las peceras); se
/// listan al acabar. El Blockout la llama al final; también tiene menú propio.
/// </summary>
public static class CheapMaterialsPass
{
    [MenuItem("Tools/Cold Sanctuary/Materiales baratos (assets del proyecto)")]
    public static void Menu() => Run(true);

    public static int Run(bool log)
    {
        int changed = 0; var keptTransparent = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("/TextMesh Pro/") || path.Contains("/Plugins/") || path.Contains("/Packages/")) continue;
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null || m.shader == null) continue;
            string sn = m.shader.name;
            if (sn != "Universal Render Pipeline/Lit" && sn != "Universal Render Pipeline/Simple Lit" && sn != "Standard") continue;   // (cielo, partículas, texto: fuera)
            float smooth = m.HasProperty("_Smoothness") ? m.GetFloat("_Smoothness") : m.HasProperty("_Glossiness") ? m.GetFloat("_Glossiness") : 0f;
            bool transparent = m.HasProperty("_Surface") && m.GetFloat("_Surface") > 0.5f;
            Color c = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.color;
            bool needs = sn != "Universal Render Pipeline/Simple Lit" || smooth > CheapMaterials.Smoothness + 1e-3f || (m.HasProperty("_Metallic") && m.GetFloat("_Metallic") > 0.01f) || (transparent && c.a >= 0.99f && !CheapMaterials.IsWater(m));
            if (transparent && (c.a < 0.99f || CheapMaterials.IsWater(m))) keptTransparent.Add(m.name);
            if (!needs) continue;
            CheapMaterials.MakeCheap(m);
            if (transparent && c.a >= 0.99f && m.HasProperty("_Surface") && !CheapMaterials.IsWater(m))   // transparente sin serlo: opaco (el cristal y el agua por su nombre, no: ronda 73, «Arbol_cristal»)
            {
                m.SetFloat("_Surface", 0f); m.SetFloat("_ZWrite", 1f);
                m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One); m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                m.DisableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.SetOverrideTag("RenderType", "Opaque"); m.renderQueue = -1;
            }
            EditorUtility.SetDirty(m);
            changed++;
        }
        AssetDatabase.SaveAssets();
        if (log) Debug.Log($"[Materiales] Pasada de assets: {changed} convertidos a mate y opaco. Siguen transparentes (cristal o agua, alfa < 0,99): {keptTransparent.Count}" +
                           (keptTransparent.Count > 0 ? " — " + string.Join(", ", keptTransparent.Distinct().Take(40)) : "") + ".");
        return changed;
    }
}
