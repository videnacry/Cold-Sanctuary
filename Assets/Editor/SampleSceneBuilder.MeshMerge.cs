using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// RENDIMIENTO (Berón 2026-10-24: «el juego carga más y no he podido mover al personaje»): los árboles de hielo son cubos,
/// y el bosque llegaba a ~50 000 renderers de los ~83 000 de la escena (8 FPS en el editor). Aquí, al final del blockout,
/// cada árbol pasa a UNA malla por material, hija de su raíz. Los cubos se quedan como objetos con su collider (se trepa,
/// las ramas siguen ahí para quien las busque por nombre), solo pierden su MeshRenderer. El árbol carnívoro que crece al
/// cazar escala su raíz, y la malla combinada crece con él. Las celdillas de las colmenas no se tocan: cambian de color.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly HashSet<string> KeepRendererNames = new HashSet<string> { "Celdilla", "Piquera" };

    /// <summary>Combina las mallas de cada hijo directo de <paramref name="forest"/> (un árbol). Devuelve cuántos renderers quitó.</summary>
    internal static int MergeTreeMeshes(Transform forest)
    {
        if (forest == null) return 0;
        int removed = 0;
        foreach (Transform tree in forest)
        {
            var byMat = new Dictionary<Material, List<CombineInstance>>();
            var done = new List<MeshRenderer>();
            foreach (MeshRenderer mr in tree.GetComponentsInChildren<MeshRenderer>())
            {
                if (KeepRendererNames.Contains(mr.name) || mr.GetComponentInParent<Hive>() != null || mr.GetComponentInParent<Anima>() != null) continue;
                MeshFilter mf = mr.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null || mr.sharedMaterials.Length != 1 || !mr.enabled) continue;
                Material m = mr.sharedMaterial;
                if (!byMat.TryGetValue(m, out var list)) byMat[m] = list = new List<CombineInstance>();
                list.Add(new CombineInstance { mesh = mf.sharedMesh, transform = tree.worldToLocalMatrix * mf.transform.localToWorldMatrix });
                done.Add(mr);
            }
            if (done.Count < 2) continue;
            int i = 0;
            foreach (var kv in byMat)
            {
                var mesh = new Mesh { name = $"{tree.name}_{i}", indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(kv.Value.ToArray(), true, true);
                var go = new GameObject($"Malla_combinada_{i++}");
                go.transform.SetParent(tree, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = kv.Key;
            }
            foreach (MeshRenderer mr in done)
            {
                MeshFilter mf = mr.GetComponent<MeshFilter>();
                Object.DestroyImmediate(mr);
                if (mf != null) Object.DestroyImmediate(mf);
                removed++;
            }
        }
        return removed;
    }
}
