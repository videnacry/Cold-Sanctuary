using UnityEngine;

// EL UNIFORME DE TRABAJO DEL INSECTO-HUMANO (docs/actividades-y-uniformes.md §4; Berón 2026-11-04: «sería posible que los humanos se
// coloquen uniformes para las tareas … estos arreglos también aplican a los insectos-humanos»). Sobre el cuerpo centauro, como la ropa
// de su era (WearGreek, Wear1940s): un DELANTAL de trabajo en el torso con su cinta, y el tocado del oficio. Las piezas se llaman
// «Uniforme_…» y se quitan todas a la vez (<see cref="RemoveWorkUniform"/>); la ropa de diario (camisa, quitón, abrigo) se queda debajo.
public partial class CubeInsect
{
    public const string UniformPrefix = "Uniforme_";

    /// <summary>Delantal de trabajo (y el tocado <paramref name="hat"/>, si no es None) sobre el torso del insecto-humano.
    /// <paramref name="sleeves"/> añade manguitos en los brazos delanteros (escribanos, boticarios).</summary>
    public static void WearWorkApron(GameObject host, Color apron, Color strap, Hat hat, bool sleeves = false, Color? cap = null)
    {
        RemoveWorkUniform(host);
        if (!Anchors(host, out Transform torso, out Transform neck, out Bounds head)) return;
        float w = head.size.x, h = head.size.y;
        Part(torso, UniformPrefix + "delantal", new Vector3(0f, 0f, 0.1f), new Vector3(w * 1.35f, h * 1.5f, 0.3f), apron);
        Part(torso, UniformPrefix + "peto", new Vector3(0f, h * 0.55f, 0.12f), new Vector3(w * 0.9f, h * 0.5f, 0.06f), apron);
        Part(torso, UniformPrefix + "cinta", new Vector3(0f, 0f, 0.04f), new Vector3(w * 1.42f, h * 1.55f, 0.035f), strap);
        if (sleeves)
            foreach (string arm in new[] { "Pata_0_D", "Pata_0_I", "Brazo_D", "Brazo_I" })
            {
                Transform a = FindDeep(host.transform, arm); if (a == null) continue;
                Transform tib = FindDeep(a, "Tibia_art") ?? a;
                Part(tib, UniformPrefix + "manguito", new Vector3(0f, 0f, h * 0.35f), new Vector3(w * 0.3f, w * 0.3f, h * 0.6f), apron);
            }
        if (hat == Hat.None && cap.HasValue)   // cofia o gorro del oficio (cocina, botica) del color pedido
            Part(neck, UniformPrefix + "gorro", head.center + Vector3.up * head.extents.y * 1.05f, new Vector3(w * 1.1f, h * 0.45f, head.size.z * 1.1f), cap.Value);
        else if (hat != Hat.None)
        {
            int before = neck.childCount;
            WearHat(host, hat);
            for (int i = before; i < neck.childCount; i++) { Transform c = neck.GetChild(i); if (!c.name.StartsWith(UniformPrefix)) c.name = UniformPrefix + c.name; }
        }
    }

    /// <summary>Quita todas las piezas del uniforme (las «Uniforme_…»).</summary>
    public static void RemoveWorkUniform(GameObject host)
    {
        if (host == null) return;
        foreach (Transform t in host.GetComponentsInChildren<Transform>(true))
            if (t != null && t != host.transform && t.name.StartsWith(UniformPrefix)) { t.name = "Retirado"; t.gameObject.SetActive(false); DestroyNow(t.gameObject); }
    }

    /// <summary>¿Lleva ahora un uniforme de trabajo?</summary>
    public static bool HasWorkUniform(GameObject host)
    {
        if (host == null) return false;
        foreach (Transform t in host.GetComponentsInChildren<Transform>(false)) if (t.name.StartsWith(UniformPrefix)) return true;
        return false;
    }
}
