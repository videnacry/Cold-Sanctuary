using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EL SITIO NUCLEAR (Berón 2026-11-05; docs/sitio-nuclear.md). Desde el 2026-11-08 (§25: «mover la planta nuclear completamente bajo el agua, como parte del
/// santuario 2»), la Central Nuclear y la Planta Atómica, con todos sus pisos y su maquinaria, están en el SANTUARIO MARINO, junto a las cavernas
/// (<see cref="BuildMarinePlant"/>). En el santuario 1, en el sitio de siempre, solo queda la ESTACIÓN: un casquete pequeño con la cabeza de la Línea, su pista
/// de alfombras de la red del santuario (como el muelle del Aéreo), la tapa del pozo de escape, la chimenea de la ventilación filtrada de las salas, el radar
/// y el sonar, y la LÓGICA (T23): <see cref="NuclearSite"/>, <see cref="CavernRock"/>, <see cref="Refuge"/>; y en el agua, la vigilancia, la zona de exclusión
/// y la línea submarina al santuario.
/// </summary>
public static partial class SampleSceneBuilder
{
    // La cara de la estación: todo dentro de ±StationHalf (NuclearSiteRule).
    static float NuclearCapHalf => NuclearSiteRule.StationHalf;
    static Transform _nuclearRoot;
    public const string EmergencyFloorName = NuclearSiteRule.EmergencyName;

    /// <summary>LA ESTACIÓN del sitio nuclear en el santuario 1, con su lógica. Lo de abajo lo hace <see cref="BuildMarineScene"/>.</summary>
    static void BuildNuclearStation(Transform parent)
    {
        _nuclearRoot = new GameObject("Sitio_nuclear_AUTO").transform; _nuclearRoot.SetParent(parent, false);
        Vector3 site = NuclearSiteRule.Center;
        Transform st = new GameObject(NuclearSiteRule.StationName).transform; st.SetParent(_nuclearRoot, false); st.position = site;
        float H = NuclearSiteRule.StationHalf;
        Color capC = new Color(0.82f, 0.85f, 0.88f);
        // El casquete pequeño: una losa flotante de 40 × 40 m, su pretil y sus 4 tendones al fondo somero (docs/sitio-nuclear.md §9).
        Transform cap = new GameObject("Casquete_artificial").transform; cap.SetParent(st, false); cap.localPosition = Vector3.zero;
        Solid(cap, new Vector3(0f, -0.75f, 0f), new Vector3(2f * H, 1.5f, 2f * H), capC, "Casquete_artificial");
        for (int s = 0; s < 4; s++)
            KitchenProps.C(cap, Quaternion.Euler(0f, s * 90f, 0f) * new Vector3(0f, 0.6f, H - 0.5f), new Vector3(2f * H, 1.2f, 1f), new Color(0.55f, 0.57f, 0.6f), s * 90f, "Pretil_casquete");
        float tendon = -SeaRule.FloorY;
        for (int a = -1; a <= 1; a += 2)
            for (int b = -1; b <= 1; b += 2)
                PolarLandscapeBuilder.IgnoreForNav(KitchenProps.C(cap, new Vector3(a * (H - 3f), -tendon * 0.5f - 1f, b * (H - 3f)), new Vector3(0.8f, tendon, 0.8f), new Color(0.3f, 0.32f, 0.35f), 0f, "Tendon_casquete").gameObject);
        // LA DETECCIÓN (§14): radar en un mástil y sonar de quillas bajo el borde.
        Vector3 mast = new Vector3(H - 4f, 0f, 0f);
        KitchenProps.C(cap, mast + new Vector3(0f, 5f, 0f), new Vector3(0.3f, 10f, 0.3f), TreeSteel, 0f, "Mastil_radar");
        KitchenProps.C(cap, mast + new Vector3(0f, 10.2f, 0f), new Vector3(2.4f, 0.4f, 0.4f), new Color(0.25f, 0.27f, 0.3f), 0f, "Radar_hielo");
        KitchenProps.C(cap, mast + new Vector3(0f, -6f, 0f), new Vector3(1f, 2f, 1f), new Color(0.95f, 0.8f, 0.2f), 0f, "Sonar_quillas");
        Label(KitchenProps.C(st, new Vector3(0f, 4.5f, -H + 3f), Vector3.one * 0.1f, TreeSteel, 0f, "Cartel_estacion"), $"{NuclearSiteRule.Building}: estación de la Línea. La Central Nuclear y la Planta Atómica están abajo, en el Santuario Marino", 1f);

        // LA LÓGICA (T23): arriba, aunque lo que vigila esté abajo.
        NuclearSite ns = _nuclearRoot.gameObject.AddComponent<NuclearSite>();
        ns.gravityWaterM3 = NuclearSiteRule.SeaSinkM3;   // la toma pasiva de cada sala abre al mar: el calor residual se va al agua sin bombas
        _nuclearRoot.gameObject.AddComponent<CavernRock>();
        _nuclearRoot.gameObject.AddComponent<Refuge>();
        // Las cabezas de las dos válvulas de la toma pasiva (las mueve la lógica; el cuerpo está abajo), bajo la losa: no asoman.
        for (int i = 0; i < 2; i++)
        {
            Transform v = new GameObject("Valvula_pasiva").transform; v.SetParent(st, false); v.localPosition = new Vector3(6f + i * 3f, -1.1f, -6f);
            KitchenProps.C(v, Vector3.zero, new Vector3(1f, 0.6f, 0.8f), new Color(0.8f, 0.2f, 0.15f), 0f, "Actuador_valvula");
            ns.valves.Add(v);
        }
        // Lo que piden los pisos de abajo sigue en el balance del santuario 1 (docs/energia.md).
        GameObject pf = new GameObject("Pisos_planta_marina"); pf.transform.SetParent(_nuclearRoot, false);
        FloorEnergy.DeclareFloor(pf, NuclearSiteRule.PlantFloorsName, 0f, 0f, NuclearSiteRule.PlantFloorsKW);
    }

    /// <summary>La línea, la chimenea de la ventilación, la vigilancia, la zona de exclusión y la línea submarina, con la estación ya puesta.</summary>
    static void FinishNuclearSite()
    {
        if (_nuclearRoot == null) return;
        Transform root = _nuclearRoot, st = root.Find(NuclearSiteRule.StationName);
        NuclearSite ns = root.GetComponent<NuclearSite>();
        Vector3 site = NuclearSiteRule.Center;
        float H = NuclearSiteRule.StationHalf;
        Color steel = TreeSteel, red = new Color(0.8f, 0.2f, 0.15f), yel = new Color(0.95f, 0.8f, 0.2f);
        _nuclearLine = BuildCavernLine(root, site, NuclearSiteRule.Building);
        ValidateCavernLine(_nuclearLine);

        // ── LA CHIMENEA DE LA VENTILACIÓN FILTRADA de las salas de abajo (el aire sube por el pozo de escape): en la esquina de sotavento de la estación ──
        Transform stack = new GameObject("Chimenea_filtrada").transform; stack.SetParent(st, false); stack.position = site + new Vector3(H - 5f, 0f, -(H - 5f));
        const float stackH = 30f;
        Transform shaft = new GameObject("Mastil").transform; shaft.SetParent(stack, false);
        shaft.gameObject.AddComponent<MeshFilter>().sharedMesh = RocketModel.Lathe("Chimenea_mastil", new[] { (0f, 0f), (1.4f, 0f), (0.7f, stackH), (0f, stackH) }, 12);
        shaft.gameObject.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(new Color(0.78f, 0.78f, 0.76f));
        for (float y = stackH - 2f; y > 6f; y -= 10f) KitchenProps.C(stack, new Vector3(0f, y, 0f), new Vector3(1.8f, 0.5f, 1.8f), red, 0f, "Franja_chimenea");
        KitchenProps.C(stack, new Vector3(-3.5f, 1.5f, 0f), new Vector3(3f, 3f, 3f), steel, 0f, "Filtros_HEPA_carbon");
        ElevatedElement ee = stack.gameObject.AddComponent<ElevatedElement>(); ee.label = "la chimenea del sitio nuclear"; ee.top = stackH;
        Label(stack, "Chimenea de la ventilación filtrada (HEPA y carbón) de las salas de abajo, a sotavento del santuario", stackH * 0.3f);

        // ── VIGILANCIA: 8 estaciones de radiación en el borde de la estación, cada una con su baliza ──
        for (int i = 0; i < 8; i++)
        {
            Vector3 d = Quaternion.Euler(0f, i * 45f, 0f) * Vector3.forward;
            float k = (H - 2f) / Mathf.Max(Mathf.Abs(d.x), Mathf.Abs(d.z));
            Transform m = new GameObject("Estacion_radiacion").transform; m.SetParent(st, false); m.position = site + d * k;
            KitchenProps.C(m, new Vector3(0f, 1.5f, 0f), new Vector3(0.2f, 3f, 0.2f), steel, 0f, "Mastil_estacion");
            KitchenProps.C(m, new Vector3(0f, 3.1f, 0f), new Vector3(0.7f, 0.5f, 0.7f), yel, 0f, "Detector");
            Transform b = KitchenProps.C(m, new Vector3(0f, 3.6f, 0f), new Vector3(0.4f, 0.4f, 0.4f), red, 0f, "Baliza_alarma");
            b.gameObject.SetActive(false);
            ns.monitors.Add(m); ns.beacons.Add(b.gameObject);
        }
        // ── SONDAS EN EL AGUA DE MAR (bajo el agua) ──
        for (int i = 0; i < 4; i++)
        {
            Transform p = KitchenProps.C(root, root.InverseTransformPoint(site + Quaternion.Euler(0f, 45f + i * 90f, 0f) * Vector3.forward * (H + 8f) + Vector3.down * 6f), new Vector3(0.5f, 2f, 0.5f), yel, 0f, "Sonda_agua_mar");
            ns.seaProbes.Add(p);
        }
        // ── ZONA DE EXCLUSIÓN: el arco de boyas ──
        for (int i = 0; i < 36; i++)
        {
            Vector3 p = site + Quaternion.Euler(0f, i * 10f, 0f) * Vector3.forward * NuclearSiteRule.ExclusionRadius;
            if (Mathf.Abs(p.x) > 590f || Mathf.Abs(p.z) > 590f) continue;
            Transform b = KitchenProps.C(root, root.InverseTransformPoint(p + Vector3.up * 0.4f), new Vector3(0.9f, 1.4f, 0.9f), yel, 0f, "Boya_exclusion");
            PolarLandscapeBuilder.IgnoreForNav(b.gameObject);
        }
        // ── LÍNEA SUBMARINA al santuario: electricidad y calor de distrito (circuito intermedio) ──
        Vector3 from = site, to = CienciasCenter;
        float len = Vector3.Distance(from, to);
        for (float t = H + 20f; t < len; t += 40f)
        {
            Vector3 p = Vector3.MoveTowards(from, to, t);
            if (PolarLandscapeBuilder.NaturalIce(p.x, p.z)) break;
            Transform b = KitchenProps.C(root, root.InverseTransformPoint(p + Vector3.up * 0.3f), new Vector3(0.6f, 0.8f, 0.6f), new Color(0.9f, 0.45f, 0.15f), 0f, "Boya_linea_submarina");
            PolarLandscapeBuilder.IgnoreForNav(b.gameObject);
        }
        ValidateNuclearSite("al terminar el sitio");
    }

    /// <summary>Una CAVERNA en la roca bajo el lecho: revestimiento de hormigón y su forro de acero. Va en el Santuario Marino.</summary>
    static Transform BuildCavern(Transform root, string name, Vector3 center, Vector3 size, string sign)
    {
        Transform c = new GameObject(name).transform; c.SetParent(root, false); c.position = center;
        Hull(c, size, 0.8f, new Color(0.55f, 0.55f, 0.53f), "Revestimiento_caverna");
        Hull(c, size - Vector3.one * 1.2f, 0.1f, new Color(0.62f, 0.64f, 0.66f), "Forro_caverna");
        Label(c, sign, size.y * 0.5f + 1f);
        return c;
    }

    // Una caja hueca de seis planchas.
    static void Hull(Transform c, Vector3 s, float t, Color col, string name)
    {
        for (int k = -1; k <= 1; k += 2)
        {
            KitchenProps.C(c, new Vector3(k * s.x * 0.5f, 0f, 0f), new Vector3(t, s.y, s.z), col, 0f, name);
            KitchenProps.C(c, new Vector3(0f, k * s.y * 0.5f, 0f), new Vector3(s.x, t, s.z), col, 0f, name);
            KitchenProps.C(c, new Vector3(0f, 0f, k * s.z * 0.5f), new Vector3(s.x, s.y, t), col, 0f, name);
        }
    }

    /// <summary>LA REGLA (docs/sitio-nuclear.md): distancia, sotavento y agua pasiva. Si no cumple, error en el registro.</summary>
    static void ValidateNuclearSite(string when)
    {
        if (_nuclearRoot == null) return;
        Vector3 site = NuclearSiteRule.Center;
        float nearest = NearestToSite(site, NuclearSiteRule.MinDistance + 100f, null, out string near);
        float cos = NuclearSiteRule.DownwindCosine(site), water = NuclearSiteRule.SeaSinkM3, hours = NuclearSiteRule.PassiveHours(water);
        var bad = new List<string>();
        if (nearest < NuclearSiteRule.MinDistance) bad.Add($"a {nearest:0} m de {near} (pide ≥ {NuclearSiteRule.MinDistance:0} m)");
        if (cos < 0.7f) bad.Add($"no queda a sotavento (coseno {cos:0.00})");
        if (hours < NuclearSiteRule.ReferenceHours) bad.Add($"el agua pasiva solo da {hours:0} h");
        if (bad.Count > 0) Debug.LogError($"[Sitio nuclear] Regla ({when}): {string.Join("; ", bad)}.");
        else Debug.Log($"[Sitio nuclear] Regla ({when}): estación a {nearest:0} m de {near}, a sotavento (coseno {cos:0.00}); la planta, abajo, con {water:0} m³ de mar por la toma pasiva (≥ {NuclearSiteRule.ReferenceHours:0} h: {hours:0} h).");
    }
}
