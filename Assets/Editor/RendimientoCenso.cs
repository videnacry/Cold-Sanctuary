using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CENSO DE RENDIMIENTO (Berón 2026-11-09): en la escena abierta, sin entrar en juego, cuántos materiales distintos hay y cuántos son caros
/// (<see cref="CheapMaterials.Expensive"/>), y cuántos renderers son de las COLUMNAS de los edificios-árbol (Columna, Collar_losa,
/// Collar_empalme) y qué parte son del total, por edificio. Para comparar antes y después de reconstruir.
/// </summary>
public static class RendimientoCenso
{
    // Las MEGACOLUMNAS SÍSMICAS de las plataformas (SampleSceneBuilder: tubo, collares, aislador, pedestal, encepado, anillo-cinturón y riostras);
    // solo las que cuelgan de «Estructura_Elevada» (las máquinas del gimnasio también tienen piezas «Columna»).
    static readonly string[] ColumnParts = { "Columna", "Collar_losa", "Collar_empalme", "Aislador_caucho_plomo", "Placa_aislador", "Pedestal", "Encepado", "Anillo_cinturon", "Riostra_BRB" };

    [MenuItem("Tools/Cold Sanctuary/Rendimiento/Censo: materiales y columnas de la escena abierta")]
    public static void Run()
    {
        var mats = new HashSet<Material>(); int expensive = 0, total = 0, columns = 0, columnObjects = 0;
        var byBuilding = new Dictionary<string, int>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            foreach (GameObject root in s.GetRootGameObjects())
                foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
                {
                    total++;
                    foreach (Material m in r.sharedMaterials) if (m != null && mats.Add(m) && CheapMaterials.Expensive(m) != null) expensive++;
                    if (System.Array.IndexOf(ColumnParts, r.name) < 0 || r.transform.parent == null || r.transform.parent.name != "Estructura_Elevada") continue;
                    columns++;
                    ArbolCampos tree = r.GetComponentInParent<ArbolCampos>();
                    string b = tree != null ? tree.building : r.transform.root.name;
                    byBuilding[b] = byBuilding.TryGetValue(b, out int c) ? c + 1 : 1;
                }
            foreach (GameObject root in s.GetRootGameObjects())
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (System.Array.IndexOf(ColumnParts, t.name) >= 0 && t.parent != null && t.parent.name == "Estructura_Elevada") columnObjects++;
        }
        var parts = new List<string>(); foreach (var kv in byBuilding) parts.Add($"{kv.Key} {kv.Value}");
        Debug.Log($"[Rendimiento] Censo: {mats.Count} materiales distintos ({expensive} caros); {total} renderers; columnas: {columnObjects} objetos, {columns} renderers " +
                  $"= {(total > 0 ? 100f * columns / total : 0f):0.0} % de los renderers ({string.Join(", ", parts)}).");
    }
}
