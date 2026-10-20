using UnityEngine;

/// <summary>
/// ESCALERA EN TORRE para las plataformas altas (12 m): en vez de una escalera recta de 17 m que se sale del hielo, tramos
/// de ~3 m que van y vuelven (en zigzag) con un rellano en cada extremo, en una huella compacta (≈ 5 × 6,6 m) delante de
/// la plataforma. Cada tramo lleva su rampa invisible (para andar y para el NavMesh), peldaños, zancas y pasamanos; un
/// muro invisible rodea la torre y separa los dos carriles para que nadie caiga.
/// </summary>
public static class StairTower
{
    public const float LaneOffset = 1.25f, LaneWidth = 2.3f, Landing = 1.2f, Rise = 0.25f, Tread = 0.35f;

    /// <summary>Monta la torre. <paramref name="edge"/> = punto local (en <paramref name="parent"/>) del borde de la
    /// plataforma por donde se baja (y = suelo de la plataforma); <paramref name="dir"/> = +1 baja hacia +Z, −1 hacia −Z.
    /// Devuelve el punto local del PIE de la escalera (a ras de suelo).</summary>
    public static Vector3 Build(Transform parent, Vector3 edge, float h, Color wood, Color woodDark, float dir = 1f)
    {
        Transform st = new GameObject("Escalera").transform; st.SetParent(parent, false);
        st.localPosition = edge;
        if (dir < 0f) st.localRotation = Quaternion.Euler(0f, 180f, 0f);
        int flights = Mathf.Max(1, Mathf.CeilToInt(h / 3f));
        float hf = h / flights;
        int n = Mathf.Max(1, Mathf.RoundToInt(hf / Rise));
        float rise = hf / n, run = n * Tread;
        float len = Mathf.Sqrt(run * run + hf * hf), ang = Mathf.Atan2(hf, run) * Mathf.Rad2Deg;
        Vector3 foot = Vector3.zero;
        for (int k = 0; k < flights; k++)
        {
            bool outward = k % 2 == 0;                     // los pares se alejan de la plataforma; los impares vuelven
            float x = outward ? -LaneOffset : LaneOffset;
            float yTop = -k * hf;
            float zTop = outward ? 0f : run;               // dónde empieza (arriba) el tramo
            float zMid = run * 0.5f;
            Transform ramp = Solid(st, new Vector3(x, yTop - hf * 0.5f - 0.05f, zMid), new Vector3(LaneWidth, 0.1f, len), wood, "Rampa");
            ramp.localRotation = Quaternion.Euler(outward ? ang : -ang, 0f, 0f);
            ramp.GetComponent<Renderer>().enabled = false;
            for (int i = 0; i < n; i++)
            {
                float y = yTop - (i + 0.5f) * rise;
                float z = outward ? (i + 0.5f) * Tread : run - (i + 0.5f) * Tread;
                KitchenProps.C(st, new Vector3(x, y, z), new Vector3(LaneWidth, rise, Tread * 1.02f), i % 2 == 0 ? wood : woodDark, 0f, "Peldaño");
            }
            for (int s = -1; s <= 1; s += 2)
            {
                Transform hr = Solid(st, new Vector3(x + s * (LaneWidth * 0.5f), yTop - hf * 0.5f + 1f, zMid), new Vector3(0.06f, 0.06f, len), woodDark, "Pasamanos");
                hr.localRotation = Quaternion.Euler(outward ? ang : -ang, 0f, 0f);
            }
            // Rellano al final del tramo (en el otro extremo), cruzando los dos carriles.
            float yBot = yTop - hf;
            bool last = k == flights - 1;
            float zLand = outward ? run + Landing * 0.5f : -Landing * 0.5f;
            if (!last) Solid(st, new Vector3(0f, yBot - 0.1f, zLand), new Vector3(LaneWidth * 2f + 0.2f, 0.2f, Landing), woodDark, "Rellano");
            else foot = new Vector3(x, yBot, outward ? run + 0.8f : -0.8f);
            // Postes del rellano (lo sostienen).
            if (!last) for (int s = -1; s <= 1; s += 2) Solid(st, new Vector3(s * (LaneWidth + 0.05f), (yBot - h) * 0.5f, zLand), new Vector3(0.18f, Mathf.Max(0.1f, h + yBot), 0.18f), wood, "Poste");
        }
        // Muros invisibles: alrededor de la torre y entre los carriles (salvo en los rellanos).
        float zMax = run + Landing, height = h + 3f, under = h - 0.5f;
        for (int s = -1; s <= 1; s += 2)
        {
            Barrier(st, new Vector3(s * (LaneWidth + 0.15f), -h * 0.5f + 1.5f, zMax * 0.5f), new Vector3(0.1f, height, zMax));                 // lados (fuera de la plataforma)
            Barrier(st, new Vector3(s * (LaneWidth + 0.15f), (-h - 0.5f) * 0.5f, -Landing * 0.5f), new Vector3(0.1f, under, Landing));         // lados bajo el borde (sin asomar al suelo de la plataforma)
        }
        float open = 2.5f;   // abajo, los fondos quedan ABIERTOS 2,5 m: por ahí se entra y se sale de la escalera
        Barrier(st, new Vector3(0f, (-h + open + 3f) * 0.5f, zMax + 0.05f), new Vector3(LaneWidth * 2f + 0.4f, h + 3f - open, 0.1f));          // fondo lejano
        Barrier(st, new Vector3(0f, (-h + open - 0.5f) * 0.5f, -Landing - 0.05f), new Vector3(LaneWidth * 2f + 0.4f, h - 0.5f - open, 0.1f));  // fondo bajo la plataforma
        Barrier(st, new Vector3(0f, -h * 0.5f + 1.5f, run * 0.5f), new Vector3(0.1f, height, run));                                             // entre carriles
        Barrier(st, new Vector3(LaneOffset, 1.4f, -0.05f), new Vector3(LaneWidth + 0.1f, 3.2f, 0.1f));                                            // arriba, el carril de subida no es un agujero
        return st.localPosition + st.localRotation * foot;
    }

    /// <summary>Lo que ocupa la torre a lo largo de la bajada (para colocar cosas al pie).</summary>
    public static float Depth(float h)
    {
        int flights = Mathf.Max(1, Mathf.CeilToInt(h / 3f));
        int n = Mathf.Max(1, Mathf.RoundToInt(h / flights / Rise));
        return n * Tread + Landing;
    }

    static Transform Solid(Transform parent, Vector3 local, Vector3 size, Color c, string name)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
        return g.transform;
    }

    static void Barrier(Transform parent, Vector3 local, Vector3 size)
    {
        Transform b = Solid(parent, local, size, Color.white, "Escalera_Barrera");
        b.GetComponent<Renderer>().enabled = false;
    }
}
