using UnityEngine;

/// <summary>
/// LOS ICEBERGS (docs/sitio-nuclear.md §14 y §20). Pocas piezas y sin colisiones bajo el agua: el PC tiene 16 GB. Se dibuja lo que asoma y su quilla hasta el fondo somero; su masa real está en la regla.
/// </summary>
public static partial class SampleSceneBuilder
{
    /// <summary>Dos icebergs: uno varado en el fondo somero, al suroeste; otro a la deriva por el este, que acaba acercándose al sitio nuclear. Con aves, focas y kril.</summary>
    static void BuildIcebergs(Transform parent)
    {
        Transform root = new GameObject("Icebergs_AUTO").transform; root.SetParent(parent, false);
        Iceberg far = BuildIceberg(root, "Iceberg_varado", new Vector3(-500f, 0f, -500f), new Vector3(80f, 240f, 60f), true);
        Iceberg drift = BuildIceberg(root, "Iceberg_deriva", new Vector3(540f, 0f, 200f), new Vector3(100f, 300f, 70f), false);
        drift.driftA = new Vector3(540f, 0f, 200f); drift.driftB = new Vector3(520f, 0f, -260f);
        BuildIcebergVisitors(root, far, 2, 2); BuildIcebergVisitors(root, drift, IcebergVisitor.Seals - 2, IcebergVisitor.Penguins - 2);
        foreach (Iceberg b in new[] { far, drift })
        {
            float nearest = NearestToSite(b.transform.position, 700f, null, out string near);
            if (nearest < IcebergRule.MinFromBuildings) Debug.LogError($"[Iceberg] {b.name}, a {nearest:0} m de {near} (pide ≥ {IcebergRule.MinFromBuildings:0} m).");
            else Debug.Log($"[Iceberg] {b.name}: {b.Tonnes / 1e6f:0.0} Mt, a {nearest:0} m de {near}.");
        }
    }

    // Sus animales: adultos, nadando frente a la repisa (docs/vida-en-icebergs.md).
    static void BuildIcebergVisitors(Transform root, Iceberg b, int seals, int penguins)
    {
        void Put(string species, int n)
        {
            GameObject prefab = LoadAnimalPrefab(species);
            if (prefab == null) return;
            for (int i = 0; i < n; i++)
            {
                Vector3 at = b.Approach + new Vector3(-6f + i * 4f, 0f, species == "Seal" ? 0f : 4f);
                Animal a = Adult(prefab, root, at, i % 2 == 0 ? Sex.female : Sex.male, $"{species}_{b.name}_{i + 1}");
                if (a != null) a.gameObject.AddComponent<IcebergVisitor>().berg = b;
            }
        }
        Put("Seal", seals); Put("Penguin", penguins);
    }

    // Un tabular: el cuerpo, un escalón y la quilla bajo el agua (sin colisiones); colisión solo en lo que asoma. Unas pocas cajas.
    static Iceberg BuildIceberg(Transform root, string name, Vector3 at, Vector3 size, bool grounded)
    {
        Color ice = new Color(0.86f, 0.93f, 0.97f), deep = new Color(0.55f, 0.75f, 0.85f);
        Transform t = new GameObject(name).transform; t.SetParent(root, false); t.position = at;
        float above = size.y * 0.1f, keel = -SeaRule.FloorY;   // una décima parte fuera del agua; la quilla se dibuja hasta el fondo de esta escena
        KitchenProps.C(t, new Vector3(0f, above * 0.5f, 0f), new Vector3(size.x, above, size.z), ice, 0f, "Hielo_iceberg");
        KitchenProps.C(t, new Vector3(size.x * 0.2f, above + 2.5f, -size.z * 0.1f), new Vector3(size.x * 0.4f, 5f, size.z * 0.5f), ice, 0f, "Hielo_iceberg");
        KitchenProps.C(t, new Vector3(0f, -keel * 0.5f, 0f), new Vector3(size.x * 0.95f, keel, size.z * 0.95f), deep, 0f, "Quilla_iceberg");
        BoxCollider col = t.gameObject.AddComponent<BoxCollider>(); col.center = new Vector3(0f, above * 0.5f, 0f); col.size = new Vector3(size.x, above, size.z);
        PolarLandscapeBuilder.IgnoreForNav(t.gameObject);
        Iceberg b = t.gameObject.AddComponent<Iceberg>(); b.grounded = grounded; b.size = size;
        if (!grounded) { Rigidbody rb = t.gameObject.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false; }
        // Lo que vive alrededor: animales de verdad (BuildIcebergVisitors), bancos de verdad.
        Vector3 krillAt = at + new Vector3(size.x * 0.5f + 12f, -4f, 0f);
        MiniSwarm(root, $"Kril_{name}", SwarmKind.Krill, krillAt);
        Transform kr = root.Find($"Kril_{name}");
        if (kr != null) b.krill = kr.GetComponent<Swarm>();
        MiniSwarm(root, $"Peces_{name}", SwarmKind.Fish, at + new Vector3(-size.x * 0.5f - 12f, -2f, 0f));
        Transform fi = root.Find($"Peces_{name}");
        if (fi != null) b.fish = fi.GetComponent<Swarm>();
        // El pie de hielo: lo único a lo que se sube. Los hielos se mueven: esta pieza no es estática.
        Transform ledge = Block(t, "Repisa_iceberg", at + new Vector3(0f, -0.05f, size.z * 0.5f + 7f), new Vector3(size.x * 0.6f, 0.8f, 14f), MakeMaterial("Repisa_iceberg_MAT", ice), 0f, true);
        ledge.gameObject.isStatic = false;
        PolarLandscapeBuilder.IgnoreForNav(ledge.gameObject);
        Affordance.Put(ledge.gameObject, AffordanceKind.HaulOut, 6);
        b.ledge = ledge;
        return b;
    }
}
