using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// AGUA ALREDEDOR de los mundos virtuales (microcosmos, mundo mob, virtualizaciones): un mar que rodea el suelo, justo por
/// debajo de él, con su propio collider, para que nadie caiga al vacío por el borde. En los MICROCOSMOS queda FUERA de los muros
/// del piso (regla 11): se ve desde dentro, pero no se llega a ella ni se puebla.
/// </summary>
public static class MicroWater
{
    public static void SurroundScene(Scene scene)
    {
        bool any = false; Bounds all = default, floor = default; float floorArea = 0f;
        foreach (GameObject go in scene.GetRootGameObjects())
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
            {
                if (!any) { all = r.bounds; any = true; } else all.Encapsulate(r.bounds);
                float a = r.bounds.size.x * r.bounds.size.z;
                if (r.bounds.size.y < 1f && a > floorArea) { floorArea = a; floor = r.bounds; }   // el suelo: lo más ancho y plano
            }
        if (!any) return;
        float y = floorArea > 0f ? floor.max.y - 0.25f : all.min.y;
        float size = Mathf.Max(all.size.x, all.size.z) + 600f;
        var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
        water.name = "Agua_alrededor";
        SceneManager.MoveGameObjectToScene(water, scene);
        water.transform.position = new Vector3(all.center.x, y - 0.5f, all.center.z);
        water.transform.localScale = new Vector3(size, 1f, size);   // su cara de arriba queda 0,25 m bajo el suelo
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh) { name = "Agua_MAT", color = new Color(0.2f, 0.45f, 0.7f) };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", new Color(0.2f, 0.45f, 0.7f));
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.9f);
        water.GetComponent<Renderer>().sharedMaterial = m;
        Debug.Log($"[Agua] {scene.name}: agua alrededor ({size:0} m) para no caer al vacío.");
    }
}
