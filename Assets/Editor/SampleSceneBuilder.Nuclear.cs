using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EL SITIO NUCLEAR (Berón 2026-11-05; docs/sitio-nuclear.md): un CASQUETE ARTIFICIAL en la esquina sureste del mar, lejos y a sotavento del
/// santuario, con su EDIFICIO-ÁRBOL bajo. Abajo, los pisos de los dos reactores (Central Nuclear y Planta Atómica); el TRIGA, en su cajón de doble
/// casco apoyado en el lecho somero (<see cref="BuildCaisson"/>), y el microrreactor y los residuos, en el Santuario Marino (<see cref="BuildMarineScene"/>); en medio, el ciclotrón y la sala de
/// experimentos; arriba, el Centro de emergencias y el garaje. Lo que gana con la altura sube (el agua del tronco por gravedad, la chimenea
/// filtrada, las salas de mando); los núcleos se quedan abajo, en su contención. Mismo casquete y mismas pruebas de sitio que la lanzadera
/// (<see cref="BuildArtificialCap"/>, <see cref="NearestToSite"/>).
/// </summary>
public static partial class SampleSceneBuilder
{
    const float NuclearCapHalf = 85f;
    static Transform _nuclearRoot, _nuclearPlantArea, _nuclearPowerArea;
    static Helix _nuclearHx;
    public const string EmergencyFloorName = "Centro_emergencias", NuclearGarageName = "Garaje_nuclear";

    /// <summary>Empieza el árbol del sitio: sus siguientes <see cref="TowerFloor"/> son sus pisos, de abajo arriba.</summary>
    static Helix BeginNuclearTree(Transform parent)
    {
        _nuclearRoot = new GameObject("Sitio_nuclear_AUTO").transform; _nuclearRoot.SetParent(parent, false);
        Vector3 site = NuclearSiteRule.Center;
        HelixTower h = TowerHelix(new Vector2(NuclearSize.x, NuclearSize.x), 3, 0f, 10f);
        h.firstY = NuclearSiteRule.FirstFloorY;   // (después de TowerHelix: su mínimo es por los árboles que crecen en el suelo, y aquí no hay)
        h.chordD = StructuralLimits.ChordFor(h);   // (sus pisos de 46 m alargan la rama: con los cordones de siempre, al 100 %)
        Helix hx = BeginHelix(_nuclearRoot, NuclearSiteRule.Building, TowerRoot(NuclearSiteRule.Building), site, 0f, h, rotates: false);
        hx.pickDoors = true;
        hx.tipContent = NuclearTip;
        hx.santuarioLabel = $"{NuclearSiteRule.Building}: Central Nuclear, Planta Atómica y el Centro de emergencias (docs/sitio-nuclear.md)";
        _nuclearHx = hx;
        _towerHelix = hx; _towerCenter = site; _towerSize = NuclearSize; _towerTop = null;
        BuildArtificialCap(_nuclearRoot, hx, site, NuclearCapHalf);
        return hx;
    }

    // Las puntas: abajo, los aerocondensadores del microrreactor (aire, sin agua); arriba, el depósito de reposición.
    static string NuclearTip(int k, Transform tip)
    {
        int lv = TreeSpec.Platform(k).level;
        bool air = lv == 1;
        tip.name = air ? $"Aerocondensador_{k}" : $"Punta_tecnica_{k}";
        Color c = air ? new Color(0.62f, 0.66f, 0.7f) : new Color(0.8f, 0.82f, 0.84f);
        KitchenProps.C(tip, new Vector3(5f, 1.5f, 5f), new Vector3(5f, 3f, 5f), c, 0f, air ? "Aerocondensador_pasivo" : "Deposito_reposicion");
        return air ? "aerocondensador pasivo" : "depósito de reposición";
    }

    /// <summary>Los pisos de arriba: el Centro de emergencias y el garaje del submarino de obra.</summary>
    static void BuildNuclearUpperFloors()
    {
        Helix hx = _nuclearHx; float S = hx.h.floorSize;
        Transform em = HelixFloor(hx, 9, null, new Vector2(S, S), EmergencyFloorName);
        Color desk = new Color(0.32f, 0.34f, 0.38f), screen = new Color(0.12f, 0.35f, 0.45f), alarm = new Color(0.8f, 0.2f, 0.15f);
        KitchenProps.C(em, new Vector3(0f, 0.01f, 0f), new Vector3(S - 1f, 0.02f, S - 1f), new Color(0.3f, 0.32f, 0.35f), 0f, "Mat");
        for (int i = 0; i < 4; i++)
        {
            Transform p = new GameObject("Puesto_emergencias").transform; p.SetParent(em, false); p.localPosition = new Vector3(-9f + i * 6f, 0f, 4f);
            KitchenProps.C(p, new Vector3(0f, 0.38f, 0f), new Vector3(2.2f, 0.76f, 0.9f), desk, 0f, "Mesa_control");
            KitchenProps.C(p, new Vector3(0f, 1.05f, 0.25f), new Vector3(1.8f, 0.6f, 0.06f), screen, 0f, "Pantallas");
        }
        Transform panel = KitchenProps.Root(em, "Panel_parada_remota", new Vector3(0f, 0f, 12f));
        KitchenProps.C(panel, new Vector3(0f, 1.2f, 0f), new Vector3(6f, 2.4f, 0.4f), desk, 0f, "Panel");
        KitchenProps.C(panel, new Vector3(0f, 1.6f, -0.25f), new Vector3(0.5f, 0.5f, 0.1f), alarm, 0f, "Pulsador_parada");
        Label(panel, "Panel de parada remota: para el reactor aunque no se pueda bajar a la sala de control", 3f);
        KitchenProps.C(em, new Vector3(-14f, 2.5f, 12f), new Vector3(10f, 4f, 0.3f), screen, 0f, "Plano_emergencia");
        Label(KitchenProps.C(em, new Vector3(0f, 4.5f, 4f), Vector3.one * 0.1f, screen, 0f, "Cartel_emergencias"), "Centro de emergencias: vigilancia radiológica, viento y plan de evacuación", 1f);
        Transform gar = HelixFloor(hx, 10, null, new Vector2(S, S), NuclearGarageName);
        SubmarineBay(gar, "Garaje del sitio nuclear: el submarino de obra trae el material por la vía marina");
    }

    /// <summary>Con el árbol terminado: contención, agua pasiva, sumidero, chimenea filtrada, vigilancia, zona de exclusión y línea al santuario.</summary>
    static void FinishNuclearSite()
    {
        if (_nuclearRoot == null || _nuclearHx == null) return;
        Helix hx = _nuclearHx; HelixTower h = hx.h;
        Transform root = _nuclearRoot; Vector3 site = hx.site;
        NuclearSite ns = root.gameObject.AddComponent<NuclearSite>();
        ns.gravityWaterM3 = NuclearSiteRule.GravityWaterM3(h.waterHalf, h.WaterTop);
        Color conc = new Color(0.6f, 0.6f, 0.58f), steel = TreeSteel, pipe = new Color(0.25f, 0.5f, 0.75f), red = new Color(0.8f, 0.2f, 0.15f), yel = new Color(0.95f, 0.8f, 0.2f);

        // ── CAJONES SUMERGIDOS: cada reactor en el suyo, bajo el casquete, y entre el piso y el casquete el recinto que rodea la piscina ──
        Transform pool = _nuclearPlantArea != null ? _nuclearPlantArea.Find("Planta/Piscina_reactor") : null;
        if (pool == null) Debug.LogError("[Sitio nuclear] No encuentro la piscina del reactor: sin recinto de contención.");
        else
        {
            Transform rc = new GameObject("Recinto_contencion").transform; rc.SetParent(root, false); rc.position = new Vector3(pool.position.x, 0f, pool.position.z);
            float top = pool.parent.position.y - TreeSpec.StrutDrop + 0.5f, half = 4.5f, th = 1.2f;
            BuildCaisson(root, hx, ns, "Cajon_TRIGA", pool.position, new Vector3(14f, NuclearSiteRule.CaissonHeight, 14f), "Cajón del TRIGA: la piscina, el núcleo y su contención, bajo el nivel del mar");
            for (int s = 0; s < 4; s++)
            {
                Quaternion q = Quaternion.Euler(0f, s * 90f, 0f);
                Vector3 size = s % 2 == 0 ? new Vector3(2f * half + th, top, th) : new Vector3(th, top, 2f * half + th);
                Solid(rc, q * new Vector3(0f, top * 0.5f, half), size, conc, "Muro_contencion");
            }
            KitchenProps.C(rc, new Vector3(0f, top + 0.15f, 0f), new Vector3(2f * half + th, 0.3f, 2f * half + th), steel, 0f, "Tapa_contencion");
            Label(KitchenProps.C(rc, new Vector3(0f, 4f, -half - 0.7f), Vector3.one * 0.1f, yel, 0f, "Cartel_contencion"), "Recinto de la piscina: baja por el casquete hasta el cajón sumergido del núcleo", 1f);

            // ── LÍNEAS PASIVAS del tronco a la piscina (dos, redundantes): por gravedad, con válvulas que fallan abiertas ──
            for (int i = 0; i < 2; i++)
            {
                float y = 4f + i * 2f;
                Vector3 a = new Vector3(pool.position.x, y, site.z + Mathf.Sign(pool.position.z - site.z) * h.OuterHalf), b = new Vector3(pool.position.x, y, pool.position.z);
                Transform line = KitchenProps.C(root, root.InverseTransformPoint((a + b) * 0.5f), new Vector3(0.45f, 0.45f, Mathf.Abs(a.z - b.z)), pipe, 0f, "Linea_pasiva_tronco");
                Transform v = new GameObject("Valvula_pasiva").transform; v.SetParent(root, false); v.position = a + (b - a).normalized * 3f;
                KitchenProps.C(v, Vector3.zero, new Vector3(1f, 1f, 0.8f), red, 0f, "Cuerpo_valvula");
                KitchenProps.C(v, new Vector3(0f, 0.8f, 0f), new Vector3(0.9f, 0.1f, 0.1f), yel, 0f, "Volante");
                ns.valves.Add(v);
            }
            // ── SUMIDERO FINAL: la toma de agua de mar y el intercambiador intermedio (el agua de mar nunca toca el primario) ──
            Vector3 off = new Vector3(half + 4f, 0f, 0f);
            KitchenProps.C(rc, off + new Vector3(0f, 1.25f, 0f), new Vector3(3f, 2.5f, 2f), steel, 0f, "Intercambiador_intermedio");
            KitchenProps.C(rc, off + new Vector3(2.5f, -3f, 0f), new Vector3(0.8f, 8f, 0.8f), pipe, 0f, "Toma_agua_mar");
            KitchenProps.C(rc, off + new Vector3(2f, 1.5f, 0f), new Vector3(1.6f, 0.5f, 0.5f), pipe, 0f, "Tuberia_toma");
        }

        // ── EL LECHO SOMERO visible bajo el sitio (el colisionador «FondoMarino» ya estaba; no se veía) ──
        KitchenProps.C(root, root.InverseTransformPoint(new Vector3(site.x, NuclearSiteRule.SeabedY - 0.5f, site.z)), new Vector3(2f * NuclearCapHalf + 80f, 1f, 2f * NuclearCapHalf + 80f), new Color(0.27f, 0.26f, 0.25f), 0f, "Lecho_marino");

        // ── EL MICRORREACTOR ESTÁ EN EL SANTUARIO MARINO (otra escena): aquí queda su pozo, hasta la boca en el fondo somero, y la esclusa desde el tronco ──
        Transform well = _nuclearPowerArea != null ? _nuclearPowerArea.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Brocal_pozo") : null;
        if (well == null) Debug.LogError("[Sitio nuclear] No encuentro el pozo del microrreactor.");
        else
        {
            float wy = well.position.y, by = NuclearSiteRule.SeabedY;
            KitchenProps.C(root, root.InverseTransformPoint(new Vector3(well.position.x, (wy + by) * 0.5f, well.position.z)), new Vector3(2.4f, wy - by, 2.4f), conc, 0f, "Pozo_microrreactor");
            KitchenProps.C(root, root.InverseTransformPoint(new Vector3(well.position.x, by + 0.5f, well.position.z)), new Vector3(6f, 1f, 6f), conc, 0f, "Boca_pozo");
            Vector3 lockA = new Vector3(Mathf.Clamp(well.position.x, site.x - h.waterHalf, site.x + h.waterHalf), -8f, site.z + Mathf.Sign(well.position.z - site.z) * h.OuterHalf), lockB = new Vector3(well.position.x, -8f, well.position.z);
            KitchenProps.C(root, root.InverseTransformPoint((lockA + lockB) * 0.5f), new Vector3(3f, 3f, Mathf.Max(0.5f, (lockB - lockA).magnitude)), steel, Quaternion.LookRotation((lockB - lockA).sqrMagnitude > 0.01f ? lockB - lockA : Vector3.forward), "Esclusa_tronco");
        }
        // LA LÓGICA DE ABAJO se queda aquí: las misiones de la roca y del refugio funcionan sin cargar la otra escena.
        root.gameObject.AddComponent<CavernRock>();
        root.gameObject.AddComponent<Refuge>();
        // EL CAJÓN apoya en el sedimento: pilotes a la roca, hitos de asiento e inclinómetros.
        Transform caissonT = root.Find("Cajon_TRIGA");
        if (caissonT != null)
        {
            float half = 8f, under = -NuclearSiteRule.CaissonHeight * 0.5f;
            for (int a = -1; a <= 1; a += 2)
                for (int b = -1; b <= 1; b += 2)
                {
                    KitchenProps.C(caissonT, new Vector3(a * half * 0.8f, under - 5.6f, b * half * 0.8f), new Vector3(0.8f, 10f, 0.8f), steel, 0f, "Pilote_roca");
                    KitchenProps.C(caissonT, new Vector3(a * (half + 1f), under + 0.5f, b * (half + 1f)), new Vector3(0.35f, 1f, 0.35f), yel, 0f, "Hito_asiento");
                }
            for (int s = -1; s <= 1; s += 2) KitchenProps.C(caissonT, new Vector3(s * 4f, NuclearSiteRule.CaissonHeight * 0.5f + 0.25f, 0f), new Vector3(0.4f, 0.5f, 0.4f), new Color(0.2f, 0.22f, 0.25f), 0f, "Inclinometro_cajon");
        }
        _nuclearLine = BuildCavernLine(root, hx);
        ValidateCavernLine(_nuclearLine);

        // ── CHIMENEA FILTRADA a sotavento, en la esquina del casquete: lo único que se suelta, se suelta en alto y filtrado ──
        Vector3 corner = new Vector3(NuclearCapHalf - 12f, 0f, -(NuclearCapHalf - 12f));
        Transform st = new GameObject("Chimenea_filtrada").transform; st.SetParent(root, false); st.position = site + corner;
        float stackH = h.RootHeight + 16f;
        Transform shaft = new GameObject("Mastil").transform; shaft.SetParent(st, false);
        shaft.gameObject.AddComponent<MeshFilter>().sharedMesh = RocketModel.Lathe("Chimenea_mastil", new[] { (0f, 0f), (2f, 0f), (0.9f, stackH), (0f, stackH) }, 12);
        shaft.gameObject.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(new Color(0.78f, 0.78f, 0.76f));
        for (float y = stackH - 2f; y > 6f; y -= 12f) KitchenProps.C(st, new Vector3(0f, y, 0f), new Vector3(2.6f, 0.5f, 2.6f), red, 0f, "Franja_chimenea");
        KitchenProps.C(st, new Vector3(-6f, 2f, 6f), new Vector3(6f, 4f, 6f), steel, 0f, "Filtros_HEPA_carbon");
        ElevatedElement ee = st.gameObject.AddComponent<ElevatedElement>(); ee.label = "la chimenea del sitio nuclear"; ee.top = stackH;
        Label(st, "Chimenea filtrada (HEPA y carbón), a sotavento del santuario", stackH * 0.3f);

        // ── VIGILANCIA: 8 estaciones de radiación en el borde, cada una con su baliza de alarma ──
        for (int i = 0; i < 8; i++)
        {
            Vector3 d = Quaternion.Euler(0f, i * 45f, 0f) * Vector3.forward;
            float k = (NuclearCapHalf - 4f) / Mathf.Max(Mathf.Abs(d.x), Mathf.Abs(d.z));   // en el borde cuadrado
            Transform m = new GameObject("Estacion_radiacion").transform; m.SetParent(root, false); m.position = site + d * k;
            KitchenProps.C(m, new Vector3(0f, 1.5f, 0f), new Vector3(0.2f, 3f, 0.2f), steel, 0f, "Mastil_estacion");
            KitchenProps.C(m, new Vector3(0f, 3.1f, 0f), new Vector3(0.7f, 0.5f, 0.7f), yel, 0f, "Detector");
            Transform b = KitchenProps.C(m, new Vector3(0f, 3.6f, 0f), new Vector3(0.4f, 0.4f, 0.4f), red, 0f, "Baliza_alarma");
            b.gameObject.SetActive(false);
            ns.monitors.Add(m); ns.beacons.Add(b.gameObject);
        }

        // ── SONDAS EN EL AGUA DE MAR: un casco roto se ve aquí primero ──
        for (int i = 0; i < 4; i++)
        {
            Transform p = KitchenProps.C(root, root.InverseTransformPoint(site + Quaternion.Euler(0f, 45f + i * 90f, 0f) * Vector3.forward * (NuclearCapHalf + 8f) + Vector3.down * 6f), new Vector3(0.5f, 2f, 0.5f), yel, 0f, "Sonda_agua_mar");
            ns.seaProbes.Add(p);
        }

        // ── ZONA DE EXCLUSIÓN: un arco de boyas (hasta el borde del mar) ──
        for (int i = 0; i < 36; i++)
        {
            Vector3 p = site + Quaternion.Euler(0f, i * 10f, 0f) * Vector3.forward * NuclearSiteRule.ExclusionRadius;
            if (Mathf.Abs(p.x) > 590f || Mathf.Abs(p.z) > 590f) continue;
            Transform b = KitchenProps.C(root, root.InverseTransformPoint(p + Vector3.up * 0.4f), new Vector3(0.9f, 1.4f, 0.9f), yel, 0f, "Boya_exclusion");
            PolarLandscapeBuilder.IgnoreForNav(b.gameObject);
        }

        // ── LÍNEA SUBMARINA al santuario: electricidad y calor de distrito (por un circuito intermedio, sin agua del reactor) ──
        Vector3 from = site, to = CienciasCenter;
        float len = Vector3.Distance(from, to);
        for (float t = NuclearCapHalf + 20f; t < len; t += 40f)
        {
            Vector3 p = Vector3.MoveTowards(from, to, t);
            if (PolarLandscapeBuilder.NaturalIce(p.x, p.z)) break;
            Transform b = KitchenProps.C(root, root.InverseTransformPoint(p + Vector3.up * 0.3f), new Vector3(0.6f, 0.8f, 0.6f), new Color(0.9f, 0.45f, 0.15f), 0f, "Boya_linea_submarina");
            PolarLandscapeBuilder.IgnoreForNav(b.gameObject);
        }
        ValidateNuclearSite("al terminar el sitio");
    }

    /// <summary>Un CAJÓN SUMERGIDO bajo el casquete (docs/sitio-nuclear.md §3): doble casco de acero con espacio anular, zapata en el lecho somero y esclusa
    /// de mantenimiento desde el tronco. Sin colisiones: nadie entra andando y el submarino pasa lejos. Centrado en la vertical de <paramref name="over"/>.</summary>
    static Transform BuildCaisson(Transform root, Helix hx, NuclearSite ns, string name, Vector3 over, Vector3 size, string sign)
    {
        Color outer = new Color(0.32f, 0.36f, 0.4f), inner = new Color(0.62f, 0.64f, 0.66f), line = new Color(0.25f, 0.3f, 0.34f);
        Transform c = new GameObject(name).transform; c.SetParent(root, false);
        float top = NuclearSiteRule.CaissonTopY;
        c.position = new Vector3(over.x, top - size.y * 0.5f, over.z);
        Hull(c, size, 0.6f, outer, "Casco_exterior");
        Hull(c, size - Vector3.one * 3f, 0.3f, inner, "Casco_interior");
        KitchenProps.C(c, new Vector3(0f, -size.y * 0.5f - 0.3f, 0f), new Vector3(size.x + 2f, 0.6f, size.z + 2f), line, 0f, "Zapata_lecho");
        // Esclusa de mantenimiento: del tronco (abierto al mar) al cajón.
        Vector3 a = new Vector3(Mathf.Clamp(over.x, hx.site.x - hx.h.waterHalf, hx.site.x + hx.h.waterHalf), c.position.y, hx.site.z + Mathf.Sign(over.z - hx.site.z) * hx.h.OuterHalf), b = c.position;
        KitchenProps.C(root, root.InverseTransformPoint((a + b) * 0.5f), new Vector3(3f, 3f, (b - a).magnitude), line, Quaternion.LookRotation(b - a), "Esclusa_tronco");
        Label(c, sign, size.y * 0.5f + 1f);
        return c;
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

    /// <summary>LA REGLA (docs/sitio-nuclear.md): distancia, sotavento, reactores abajo y agua por gravedad. Si no cumple, error en el registro.</summary>
    static void ValidateNuclearSite(string when)
    {
        if (_nuclearHx == null) return;
        Helix hx = _nuclearHx;
        float nearest = NearestToSite(hx.site, NuclearSiteRule.MinDistance + 100f, hx, out string near);
        float cos = NuclearSiteRule.DownwindCosine(hx.site), water = NuclearSiteRule.GravityWaterM3(hx.h.waterHalf, hx.h.WaterTop), hours = NuclearSiteRule.PassiveHours(water);
        var bad = new List<string>();
        if (nearest < NuclearSiteRule.MinDistance) bad.Add($"a {nearest:0} m de {near} (pide ≥ {NuclearSiteRule.MinDistance:0} m)");
        if (cos < 0.7f) bad.Add($"no queda a sotavento (coseno {cos:0.00})");
        if (hours < NuclearSiteRule.ReferenceHours) bad.Add($"el agua del tronco solo da {hours:0} h");
        if (bad.Count > 0) Debug.LogError($"[Sitio nuclear] Regla ({when}): {string.Join("; ", bad)}.");
        else Debug.Log($"[Sitio nuclear] Regla ({when}): a {nearest:0} m de {near}, a sotavento (coseno {cos:0.00}), {water:0} m³ por gravedad (≥ {NuclearSiteRule.ReferenceHours:0} h: {hours:0} h).");
    }
}
