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
        foreach (Iceberg b in new[] { far, drift })
        {
            float nearest = NearestToSite(b.transform.position, 700f, null, out string near);
            if (nearest < IcebergRule.MinFromBuildings) Debug.LogError($"[Iceberg] {b.name}, a {nearest:0} m de {near} (pide ≥ {IcebergRule.MinFromBuildings:0} m).");
            else Debug.Log($"[Iceberg] {b.name}: {b.Tonnes / 1e6f:0.0} Mt, a {nearest:0} m de {near}.");
        }
    }

    // Un tabular: el cuerpo, un escalón y la quilla (sin colisiones); colisión solo en lo que asoma. Unas pocas cajas.
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
        // Lo que vive alrededor. Las focas y las aves son figuras, no animales con IA; el kril es un enjambre de verdad (pequeño).
        for (int i = 0; i < 3; i++) KitchenProps.C(t, new Vector3(-size.x * 0.3f + i * 5f, above + 0.25f, size.z * 0.3f - i * 3f), new Vector3(0.7f, 0.5f, 2f), new Color(0.35f, 0.36f, 0.38f), i * 40f, "Foca_iceberg");
        Transform birds = new GameObject("Aves_iceberg").transform; birds.SetParent(t, false); birds.localPosition = new Vector3(0f, above + 14f, 0f);
        for (int i = 0; i < 8; i++)
        {
            Vector3 p = Quaternion.Euler(0f, i * 45f, 0f) * Vector3.forward * (size.x * 0.6f + (i % 3) * 8f) + Vector3.up * ((i % 4) * 3f);
            KitchenProps.C(birds, p, new Vector3(0.9f, 0.08f, 0.35f), Color.white, i * 45f, "Petrel_nival");
        }
        b.birds = birds;
        Vector3 krillAt = at + new Vector3(size.x * 0.5f + 12f, -4f, 0f);
        MiniSwarm(root, $"Kril_{name}", SwarmKind.Krill, krillAt);
        Transform kr = root.Find($"Kril_{name}");
        if (kr != null) b.krill = kr.GetComponent<Swarm>();
        return b;
    }
}
