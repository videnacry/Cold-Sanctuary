using UnityEngine;

/// <summary>
/// EL REFUGIO DEL FONDO y LOS ICEBERGS (docs/sitio-nuclear.md §12 y §14). Pocas piezas y sin colisiones bajo el agua: el PC tiene 16 GB.
/// </summary>
public static partial class SampleSceneBuilder
{
    /// <summary>El refugio: una sala estanca al pie del pozo de escape, en el pilar de roca, con su esclusa, aire, agua y comunicaciones.</summary>
    static void BuildRefuge(Transform root, SiteLine line, Vector3 cableFrom)
    {
        if (line == null || line.escapeBottom == null) return;
        Color wall = new Color(0.82f, 0.45f, 0.15f), steel = TreeSteel, dark = new Color(0.2f, 0.22f, 0.25f), blue = new Color(0.25f, 0.5f, 0.75f), green = new Color(0.25f, 0.7f, 0.4f);
        Vector3 size = new Vector3(20f, 3f, 8f);   // 160 m²: 13 m² por persona, para un mes
        Transform r = new GameObject("Refugio_fondo").transform; r.SetParent(root, false);
        Vector3 along = line.escapeBottom.position - line.bottom.position; along.y = 0f; along = along.sqrMagnitude > 0.01f ? along.normalized : Vector3.right;
        r.position = line.escapeBottom.position + along * (size.x * 0.5f + 3f);   // centrada en el pilar, a lo largo: 8 m de roca a cada sala
        r.gameObject.AddComponent<Refuge>();
        Hull(KitchenProps.Root(r, "Caja_refugio", new Vector3(0f, 1.5f, 0f)), size, 0.2f, wall, "Pared_refugio");
        KitchenProps.C(r, new Vector3(0f, 1.1f, -size.z * 0.5f - 0.9f), new Vector3(2f, 2.2f, 1.6f), steel, 0f, "Esclusa_refugio");
        for (int i = 0; i < 8; i++) KitchenProps.C(r, new Vector3(-3.4f + i * 0.45f, 0.75f, 2f), new Vector3(0.28f, 1.5f, 0.28f), green, 0f, "Botella_oxigeno");
        KitchenProps.C(r, new Vector3(1.6f, 0.5f, 2f), new Vector3(1.6f, 1f, 0.7f), blue, 0f, "Deposito_agua");
        KitchenProps.C(r, new Vector3(3.2f, 0.6f, 2f), new Vector3(1f, 1.2f, 0.6f), dark, 0f, "Depurador_CO2");
        KitchenProps.C(r, new Vector3(3.4f, 1.3f, -1.6f), new Vector3(0.5f, 0.9f, 0.5f), dark, 0f, "Baterias_refugio");
        KitchenProps.C(r, new Vector3(-3.6f, 1.4f, -2.3f), new Vector3(0.4f, 0.4f, 0.1f), new Color(0.9f, 0.3f, 0.2f), 0f, "Telefono_fibra");
        KitchenProps.C(r, new Vector3(-2.8f, 1.4f, -2.3f), new Vector3(0.4f, 0.4f, 0.1f), steel, 0f, "Radio_traves_roca");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(r, new Vector3(0f, 0.45f, s * 0.9f), new Vector3(6.4f, 0.1f, 0.7f), dark, 0f, "Banco_refugio");
        // UN MES: lo que hace agua y aire de la mar, y lo que da de comer. El agua llega sola: a 57 bar el mar ya empuja más que lo que pide la membrana.
        KitchenProps.C(r, new Vector3(5.5f, 1f, 3f), new Vector3(2.4f, 2f, 1.2f), blue, 0f, "Osmosis_inversa");
        KitchenProps.C(r, new Vector3(8.2f, 1f, 3f), new Vector3(2f, 2f, 1.2f), new Color(0.75f, 0.8f, 0.85f), 0f, "Electrolizador");
        KitchenProps.C(r, new Vector3(8.2f, 1f, -3f), new Vector3(2f, 2f, 1.2f), dark, 0f, "Depurador_amina");
        for (int i = -1; i <= 1; i += 2) KitchenProps.C(r, new Vector3(i * 8f, 1.5f, 4.6f), new Vector3(0.25f, 3f, 1.2f), blue, 0f, "Sondeo_mar");
        KitchenProps.C(r, new Vector3(-8f, 1.1f, -2.8f), new Vector3(2.6f, 2.2f, 1.6f), new Color(0.95f, 0.75f, 0.1f), 0f, "Perforadora_sondeos");
        KitchenProps.C(r, new Vector3(-8f, 2.5f, 3.4f), new Vector3(0.9f, 0.6f, 0.9f), new Color(0.8f, 0.2f, 0.15f), 0f, "Preventor_sondeo");
        KitchenProps.C(r, new Vector3(-5.5f, 1.1f, 3f), new Vector3(2.6f, 2.2f, 1f), new Color(0.6f, 0.45f, 0.3f), 0f, "Despensa_refugio");
        for (int i = 0; i < 2; i++) KitchenProps.C(r, new Vector3(-6f + i * 7f, 1.2f, -3.2f), new Vector3(5f, 2.2f, 0.9f), new Color(0.25f, 0.65f, 0.3f), 0f, "Estante_hidroponia");
        for (int i = 0; i < 3; i++) KitchenProps.C(r, new Vector3(-3.5f + i * 3.5f, 1.4f, 3.7f), new Vector3(3f, 2.4f, 0.12f), new Color(0.1f, 0.5f, 0.45f), 0f, "Panel_espirulina");
        KitchenProps.C(r, new Vector3(2.5f, 0.5f, -1.4f), new Vector3(1.2f, 1f, 0.6f), blue, 0f, "Deposito_abono");
        Vector3 reactor = cableFrom - r.position;
        KitchenProps.C(r, reactor * 0.5f + Vector3.up * 2.6f, new Vector3(0.2f, 0.2f, reactor.magnitude), new Color(0.9f, 0.45f, 0.15f), Quaternion.LookRotation(reactor), "Cable_reactor");
        Label(r, $"Refugio del fondo: {RefugeRule.Capacity} plazas, {RefugeRule.Hours / 24f:0} días de aire y agua", 3.4f);
    }

    /// <summary>Dos icebergs en mar abierto: uno varado en un banco, al suroeste; otro a la deriva por el este, que acaba acercándose al sitio. Con aves, focas y kril.</summary>
    static void BuildIcebergs(Transform parent)
    {
        Transform root = new GameObject("Icebergs_AUTO").transform; root.SetParent(parent, false);
        Iceberg far = BuildIceberg(root, "Iceberg_varado", new Vector3(-500f, 0f, -500f), new Vector3(80f, 240f, 60f), true);   // (al suroeste: al noroeste está el parque energético)
        KitchenProps.C(far.transform, new Vector3(0f, -270f, 0f), new Vector3(160f, 60f, 140f), new Color(0.27f, 0.26f, 0.25f), 0f, "Banco_varadero");   // (el fondo no tiene relieve: este es el banco)
        Iceberg drift = BuildIceberg(root, "Iceberg_deriva", new Vector3(530f, 0f, 120f), new Vector3(100f, 300f, 70f), false);
        drift.driftA = new Vector3(530f, 0f, 120f); drift.driftB = new Vector3(530f, 0f, -300f);
        foreach (Iceberg b in new[] { far, drift })
        {
            float nearest = NearestToSite(b.transform.position, 700f, null, out string near);
            if (nearest < IcebergRule.MinFromBuildings) Debug.LogError($"[Iceberg] {b.name}, a {nearest:0} m de {near} (pide ≥ {IcebergRule.MinFromBuildings:0} m).");
            else Debug.Log($"[Iceberg] {b.name}: {b.Tonnes / 1e6f:0.0} Mt, a {nearest:0} m de {near}.");
        }
    }

    // Un tabular: el cuerpo, un escalón y la quilla bajo el agua (sin colisiones); colisión solo en lo que asoma. Unas pocas cajas.
    static Iceberg BuildIceberg(Transform root, string name, Vector3 at, Vector3 size, bool grounded)
    {
        Color ice = new Color(0.86f, 0.93f, 0.97f), deep = new Color(0.55f, 0.75f, 0.85f);
        Transform t = new GameObject(name).transform; t.SetParent(root, false); t.position = at;
        float above = size.y * 0.1f;   // una décima parte fuera del agua
        KitchenProps.C(t, new Vector3(0f, above * 0.5f, 0f), new Vector3(size.x, above, size.z), ice, 0f, "Hielo_iceberg");
        KitchenProps.C(t, new Vector3(size.x * 0.2f, above + 2.5f, -size.z * 0.1f), new Vector3(size.x * 0.4f, 5f, size.z * 0.5f), ice, 0f, "Hielo_iceberg");
        KitchenProps.C(t, new Vector3(0f, -(size.y - above) * 0.5f, 0f), new Vector3(size.x * 0.95f, size.y - above, size.z * 0.95f), deep, 0f, "Quilla_iceberg");
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
