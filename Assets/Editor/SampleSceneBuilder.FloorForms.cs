using UnityEngine;

/// <summary>
/// LAS DOS FORMAS DE CADA PISO (docs/construcciones-elevadas.md §Formas del piso; Berón 2026-11-03): todo piso del santuario
/// tiene preparada su FORMA FINALIZADA y su FORMA EN CONSTRUCCIÓN (<see cref="FloorForms"/>). Los pisos en obras (Dojo, Sala de
/// Ballet) ya traen las dos (sus piezas de obra y su «Acabado» oculto, <see cref="PrepareSite"/>); a cada piso terminado se le
/// prepara aquí su forma en construcción, OCULTA («Forma_en_obra»: andamio, palés de material, conos y muretes a medio levantar a
/// lo largo de su borde), para el día que se reforme. Los microcosmos se montan siempre sobre la forma finalizada.
/// </summary>
public static partial class SampleSceneBuilder
{
    /// <summary>Recorre todos los pisos (SanctuaryArea) y deja en cada uno su <see cref="FloorForms"/> con sus dos formas.
    /// Devuelve cuántos pisos recibieron una forma en construcción nueva.</summary>
    internal static int EnsureFloorForms()
    {
        int made = 0, obras = 0, total = 0;
        foreach (SanctuaryArea a in Object.FindObjectsByType<SanctuaryArea>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            total++;
            Transform area = a.transform;
            FloorForms ff = a.GetComponent<FloorForms>() ?? a.gameObject.AddComponent<FloorForms>();
            Transform done = area.Find(FloorForms.FinishedName);
            if (done != null)
            {   // En obras: la forma finalizada es su «Acabado»; la de construcción, sus piezas de obra (andamio, material, conos).
                ff.finished = done; ff.construction = area.Find("Andamio_obra"); ff.underConstruction = a.GetComponent<ConstructionProject>() != null;
                obras++;
                continue;
            }
            ff.finished = null; ff.underConstruction = false;
            Transform old = area.Find(FloorForms.ConstructionName);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            ff.construction = BuildConstructionForm(area, FloorSizeOf(area));
            ff.construction.gameObject.SetActive(false);
            made++;
        }
        Debug.Log($"[Pisos] Formas de {total} pisos: {obras} en obras (su «Acabado» es la forma finalizada) y {made} terminados con su forma en construcción preparada (oculta).");
        return made;
    }

    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pisos: preparar sus dos formas (en obra y acabado)")]
    static void EnsureFloorFormsMenu()
    {
        EnsureFloorForms();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    /// <summary>LOS PISOS QUE FALTAN POR CREAR (MicroFloors.Pending): cada uno con los microcosmos que lo esperan.</summary>
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pisos que faltan (microcosmos pendientes de piso representativo)")]
    internal static void PendingFloorsMenu()
    {
        var d = MicroFloors.Pending();
        foreach (var kv in d) Debug.Log($"[Pisos] PENDIENTE «{kv.Key}»: {string.Join("; ", kv.Value)}");
        Debug.Log($"[Pisos] {d.Count} pisos representativos por crear en el santuario.");
    }

    // El tamaño del piso: el de sus plazas (PlatformSlots) o el de su suelo «Mat».
    static Vector2 FloorSizeOf(Transform area)
    {
        PlatformSlots ps = area.GetComponent<PlatformSlots>();
        if (ps != null && ps.floorSize.x > 1f) return ps.floorSize;
        Transform mat = area.Find("Mat");
        if (mat != null && mat.TryGetComponent(out Renderer r)) return new Vector2(r.bounds.size.x, r.bounds.size.z);
        return new Vector2(18f, 15f);
    }

    /// <summary>La forma EN CONSTRUCCIÓN de un piso de <paramref name="size"/>: andamio en una esquina, el material de un día
    /// (palés de guadua, barro con paja y micelio), conos y cuatro muretes a medio levantar en el borde. Pocas piezas.</summary>
    static Transform BuildConstructionForm(Transform area, Vector2 size)
    {
        Transform f = new GameObject(FloorForms.ConstructionName).transform; f.SetParent(area, false); f.localPosition = Vector3.up * 0.1f;
        float hx = size.x * 0.5f, hz = size.y * 0.5f;
        Color chrome = new Color(0.75f, 0.76f, 0.78f), wood = new Color(0.55f, 0.4f, 0.25f), mud = new Color(0.62f, 0.5f, 0.38f), cone = new Color(0.95f, 0.45f, 0.15f);
        Transform scaf = KitchenProps.Root(f, "Andamio_reforma", new Vector3(-hx + 1.4f, 0f, -hz + 1f));
        for (int i = 0; i < 4; i++) KitchenProps.C(scaf, new Vector3(i % 2 == 0 ? -0.9f : 0.9f, 1.5f, i < 2 ? -0.4f : 0.4f), new Vector3(0.05f, 3f, 0.05f), chrome, 0f, "Tubo");
        for (int y = 0; y < 2; y++) KitchenProps.C(scaf, new Vector3(0f, 1.4f + y * 1.4f, 0f), new Vector3(1.9f, 0.05f, 0.85f), wood, 0f, "Plataforma");
        Transform mat = KitchenProps.Root(f, "Material_reforma", new Vector3(hx - 2f, 0f, -hz + 1.2f));
        KitchenProps.C(mat, new Vector3(-0.7f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), wood, 0f, "Pale");
        KitchenProps.C(mat, new Vector3(-0.7f, 0.35f, 0f), new Vector3(1f, 0.45f, 1f), mud, 0f, "Barro_con_paja");
        KitchenProps.C(mat, new Vector3(0.7f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), wood, 0f, "Pale");
        KitchenProps.C(mat, new Vector3(0.7f, 0.3f, 0f), new Vector3(0.95f, 0.36f, 0.45f), new Color(0.93f, 0.9f, 0.82f), 0f, "Panel_micelio");
        for (int i = 0; i < 3; i++) KitchenProps.C(f, new Vector3(-1f + i * 0.8f, 0.2f, -hz + 0.6f), new Vector3(0.25f, 0.4f, 0.25f), cone, 45f, "Cono_reforma");
        // Muretes a medio levantar (0,6 m) en los cuatro bordes, con un hueco de paso en el frente (−z).
        KitchenProps.C(f, new Vector3(0f, 0.3f, hz - 0.15f), new Vector3(size.x - 0.6f, 0.6f, 0.2f), mud, 0f, "Murete_obra");
        KitchenProps.C(f, new Vector3(-hx + 0.15f, 0.3f, 0f), new Vector3(0.2f, 0.6f, size.y - 0.6f), mud, 0f, "Murete_obra");
        KitchenProps.C(f, new Vector3(hx - 0.15f, 0.3f, 0f), new Vector3(0.2f, 0.6f, size.y - 0.6f), mud, 0f, "Murete_obra");
        float side = Mathf.Max(0.5f, (size.x - 3f) * 0.5f);
        KitchenProps.C(f, new Vector3(-(1.5f + side * 0.5f), 0.3f, -hz + 0.15f), new Vector3(side, 0.6f, 0.2f), mud, 0f, "Murete_obra");
        KitchenProps.C(f, new Vector3(1.5f + side * 0.5f, 0.3f, -hz + 0.15f), new Vector3(side, 0.6f, 0.2f), mud, 0f, "Murete_obra");
        foreach (Collider c in f.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);   // oculta y sin estorbar al NavMesh
        var wl = f.gameObject.AddComponent<WorldLabel>(); wl.text = "EN OBRAS (reforma)"; wl.height = 4.5f;
        return f;
    }
}
