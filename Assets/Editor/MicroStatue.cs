using UnityEngine;

/// <summary>
/// LAS ESTATUAS DE LOS MICROCOSMOS MODELAN INSECTOS-HUMANOS (docs/microcosmos-suelo-y-elementos.md §Estatuas; Berón 2026-11-04: «me ha
/// parecido que la estatua tenía forma humana, todas las estatuas humanas deben tener su versión para el microcosmos donde modelan
/// figuras de insectos-humanos»). La figura es el CUERPO de un insecto-humano de verdad (<see cref="CubeInsect.DressFolk"/>: su especie
/// por el papel que representa —<see cref="InsectCast.SpeciesFor"/>—, erguido, con su cara y su pelo), sin conducta ni colliders y
/// toda de PIEDRA (o bronce): la misma pose de pie y el mismo papel (su nombre) que la estatua humana.
/// La usan la estatua de la plaza (<c>MicroFurniture.Statue</c>) y la versión de microcosmos de las estatuas del piso
/// (<c>MicroFloorDress</c>: Patanjali, Buda y Shiva de la Sala de Yoga, a ×100: colosos).
/// </summary>
internal static class MicroStatue
{
    public const string FigureName = "Figura_insecto_humano";

    /// <summary>Una figura de insecto-humano de <paramref name="height"/> m que representa a <paramref name="role"/>, de pie en
    /// <paramref name="local"/> (locales de <paramref name="parent"/>), mirando a <paramref name="yaw"/>, toda del color
    /// <paramref name="stone"/>.</summary>
    internal static Transform InsectFigure(Transform parent, Vector3 local, float yaw, float height, string role, Color stone)
    {
        var g = new GameObject(FigureName);
        g.transform.SetParent(parent, false); g.transform.localPosition = local; g.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        string sp = InsectCast.SpeciesFor(string.IsNullOrEmpty(role) ? "estatua" : role);
        bool female = AreaMicrocosmos.FemaleByName(role) ?? false;
        try { CubeInsect.DressFolk(g, sp, Mathf.Max(0.6f, height), role ?? "", female); }
        catch (System.Exception e) { Debug.LogWarning($"[Estatuas] No se pudo modelar el insecto-humano de «{role}» ({e.Message}): figura sencilla."); Simple(g.transform, height, stone); }
        Petrify(g, stone);
        var wl = g.AddComponent<MicroPiece>(); wl.kind = "estatua_insecto"; wl.fitFor = $"figura de {sp}";
        return g.transform;
    }

    /// <summary>Quita la conducta y los colliders (es piedra: no anda, no se cuenta como gente) y la pinta de <paramref name="stone"/>.</summary>
    static void Petrify(GameObject g, Color stone)
    {
        for (int pass = 0; pass < 3; pass++)
        {
            var bs = g.GetComponentsInChildren<MonoBehaviour>(true);
            if (bs.Length == 0) break;
            for (int i = bs.Length - 1; i >= 0; i--) if (bs[i] != null) Object.DestroyImmediate(bs[i]);
        }
        foreach (Collider c in g.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
        foreach (Rigidbody rb in g.GetComponentsInChildren<Rigidbody>(true)) Object.DestroyImmediate(rb);
        foreach (Animator an in g.GetComponentsInChildren<Animator>(true)) Object.DestroyImmediate(an);
        Material m = AreaMicrocosmos.Mat("Estatua_" + ColorUtility.ToHtmlStringRGB(stone), stone);
        Material dark = AreaMicrocosmos.Mat("Estatua_sombra_" + ColorUtility.ToHtmlStringRGB(stone), Color.Lerp(stone, Color.black, 0.18f));
        int k = 0;
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = (k++ % 5 == 0) ? dark : m;   // un poco de sombra tallada
            r.sharedMaterials = mats;
            r.enabled = true;
        }
    }

    // Si no se puede vestir el insecto (sin ficha de especie): un insecto-humano de bloques (abdomen detrás, torso erguido, antenas).
    static void Simple(Transform t, float h, Color stone)
    {
        Box(t, "Abdomen", new Vector3(0f, h * 0.28f, -h * 0.22f), new Vector3(h * 0.22f, h * 0.2f, h * 0.38f));
        Box(t, "Torso", new Vector3(0f, h * 0.55f, 0f), new Vector3(h * 0.2f, h * 0.36f, h * 0.16f));
        Box(t, "Cabeza", new Vector3(0f, h * 0.83f, 0f), new Vector3(h * 0.14f, h * 0.14f, h * 0.14f));
        for (int s = -1; s <= 1; s += 2)
        {
            Box(t, "Antena", new Vector3(s * h * 0.04f, h * 0.95f, h * 0.03f), new Vector3(h * 0.015f, h * 0.12f, h * 0.015f));
            for (int k = 0; k < 2; k++) Box(t, "Pata", new Vector3(s * h * 0.12f, h * 0.12f, -h * (0.1f + k * 0.18f)), new Vector3(h * 0.03f, h * 0.24f, h * 0.03f));
            Box(t, "Brazo", new Vector3(s * h * 0.14f, h * 0.55f, 0f), new Vector3(h * 0.04f, h * 0.3f, h * 0.04f));
        }
    }

    static void Box(Transform t, string n, Vector3 p, Vector3 s)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = n; g.transform.SetParent(t, false); g.transform.localPosition = p; g.transform.localScale = s;
    }
}
