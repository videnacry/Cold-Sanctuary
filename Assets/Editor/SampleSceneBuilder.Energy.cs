using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL PARQUE ENERGÉTICO (Berón 2026-11-05; docs/energia.md): un CASQUETE ARTIFICIAL en la esquina noroeste del mar con su árbol bajo. En el árbol, el control, el taller
/// de los paneles y el garaje; en el casquete, el campo solar con las mesas que piden los pisos (y las plazas para los que vengan), 3 aerogeneradores en las esquinas con su
/// pala negra, cámaras y radar, la turbina mareal piloto con sonar y el almacén (baterías y silo de arena). Mismo casquete y pruebas de sitio que la lanzadera y el sitio
/// nuclear (<see cref="BuildArtificialCap"/>, <see cref="NearestToSite"/>).
/// </summary>
public static partial class SampleSceneBuilder
{
    public const string EnergyFloorName = "Control_de_energia", EnergyGarageName = "Garaje_energia";
    static Transform _energyRoot;
    static Helix _energyHx;

    /// <summary>Empieza el parque: su casquete, su árbol y sus pisos (el campo y las máquinas, al final: <see cref="FinishEnergySite"/>).</summary>
    static SanctuaryArea BeginEnergyTree(Transform parent)
    {
        _energyRoot = new GameObject("Parque_energetico_AUTO").transform; _energyRoot.SetParent(parent, false);
        Vector3 site = EnergyRule.Center;
        HelixTower h = TowerHelix(new Vector2(32f, 32f), 2, 0f, 10f);
        h.firstY = EnergyRule.FirstFloorY;   // (después de TowerHelix: su mínimo es por los árboles que crecen en el suelo, y en el casquete no hay)
        Helix hx = BeginHelix(_energyRoot, EnergyRule.Building, TowerRoot(EnergyRule.Building), site, 0f, h, rotates: false);
        hx.pickDoors = true;
        hx.tipContent = EnergyTip;
        hx.santuarioLabel = $"{EnergyRule.Building}: paneles, viento, mareal y almacén (docs/energia.md)";
        _energyHx = hx;
        BuildArtificialCap(_energyRoot, hx, site, EnergyRule.CapHalf);
        Transform f = HelixFloor(hx, 1, null, new Vector2(h.floorSize, h.floorSize), EnergyFloorName);
        FurnishEnergyFloor(f, h.floorSize);
        Transform gar = HelixFloor(hx, 2, null, new Vector2(h.floorSize, h.floorSize), EnergyGarageName);
        SubmarineBay(gar, "Garaje del parque: el submarino de obra trae las mesas solares y las palas por la vía marina");
        return f.GetComponent<SanctuaryArea>();
    }

    // Las puntas: miradores con las cámaras que vigilan la fauna del parque.
    static string EnergyTip(int k, Transform tip)
    {
        tip.name = $"Mirador_fauna_{k}";
        KitchenProps.C(tip, new Vector3(0f, 2f, 0f), new Vector3(0.2f, 4f, 0.2f), TreeSteel, 0f, "Mastil_camara");
        KitchenProps.C(tip, new Vector3(0f, 4.2f, 0f), new Vector3(0.6f, 0.4f, 0.8f), new Color(0.2f, 0.21f, 0.23f), 0f, "Camara_fauna");
        return "mirador de fauna";
    }

    // CONTROL DE ENERGÍA: los puestos del reparto de cargas, el taller de paneles (donde Fernando prepara las mesas) y los repuestos.
    static void FurnishEnergyFloor(Transform f, float S)
    {
        Color desk = new Color(0.32f, 0.34f, 0.38f), screen = new Color(0.12f, 0.35f, 0.45f), chair = new Color(0.18f, 0.18f, 0.2f), wood = new Color(0.55f, 0.42f, 0.28f), cell = new Color(0.16f, 0.26f, 0.45f);
        KitchenProps.C(f, new Vector3(0f, 0.01f, 0f), new Vector3(S - 1f, 0.02f, S - 1f), new Color(0.72f, 0.75f, 0.78f), 0f, "Mat");
        for (int i = 0; i < 4; i++)
        {
            Transform p = new GameObject("Puesto_control_energia").transform; p.SetParent(f, false); p.localPosition = new Vector3(-9f + i * 6f, 0f, 7f);
            KitchenProps.C(p, new Vector3(0f, 0.38f, 0f), new Vector3(2.2f, 0.76f, 0.9f), desk, 0f, "Mesa_control");
            KitchenProps.C(p, new Vector3(0f, 1.05f, 0.25f), new Vector3(1.8f, 0.6f, 0.06f), screen, 0f, "Pantallas");
            KitchenProps.C(p, new Vector3(0f, 0.45f, -0.9f), new Vector3(0.6f, 0.9f, 0.6f), chair, 0f, "Silla");
            AreaSeat.Add(p.gameObject, "energía", AreaSeat.Category.Trabajo);
        }
        Label(KitchenProps.C(f, new Vector3(0f, 3f, 13f), new Vector3(14f, 4f, 0.3f), screen, 0f, "Pantalla_balance"), "Oferta y demanda: reactor, sol, viento, marea, baterías y paradas por fauna", 2.6f);
        for (int i = 0; i < 3; i++)
        {
            Transform b = new GameObject("Banco_taller_paneles").transform; b.SetParent(f, false); b.localPosition = new Vector3(-8f + i * 8f, 0f, -3f);
            KitchenProps.C(b, new Vector3(0f, 0.45f, 0f), new Vector3(3f, 0.9f, 1.2f), wood, 0f, "Tablero");
            KitchenProps.C(b, new Vector3(0f, 0.93f, 0f), new Vector3(2f, 0.05f, 1f), cell, 0f, "Panel_en_el_banco");
            AreaSeat.Add(b.gameObject, "energía", AreaSeat.Category.Trabajo);
        }
        Transform rack = KitchenProps.Root(f, "Estante_repuestos_paneles", new Vector3(10f, 0f, -11f));
        KitchenProps.C(rack, new Vector3(0f, 1.2f, 0f), new Vector3(6f, 2.4f, 0.8f), wood, 0f, "Estante");
        for (int i = 0; i < 5; i++) KitchenProps.C(rack, new Vector3(-2f + i, 1.3f, -0.45f), new Vector3(0.12f, 2f, 0.08f), cell, 0f, "Panel_repuesto");
        Label(rack, "Módulos, inversores y cable de repuesto", 2.8f);
        SanctuaryArea a = f.gameObject.AddComponent<SanctuaryArea>();
        a.areaType = SanctuaryAreaType.EnergyPark; a.displayName = EnergyRule.Area; a.progressionTier = 3;
        a.description = "Control de la energía y taller de los paneles solares (docs/energia.md)";
    }

    /// <summary>Con todos los pisos puestos: declara sus demandas, levanta el campo con sus mesas, las turbinas, la mareal, el almacén y la línea al santuario.</summary>
    static void FinishEnergySite()
    {
        if (_energyRoot == null || _energyHx == null) return;
        Transform root = _energyRoot; Helix hx = _energyHx;
        EnergySite es = root.gameObject.AddComponent<EnergySite>();
        EnsureFloorEnergy();
        BuildSolarField(es, root, hx);
        float H = EnergyRule.CapHalf - 16f;
        (Vector3 p, string n)[] corners = { (new Vector3(-H, 0f, H), "noroeste"), (new Vector3(H, 0f, H), "noreste"), (new Vector3(-H, 0f, -H), "suroeste") };
        for (int i = 0; i < EnergyRule.Turbines; i++) es.turbines.Add(BuildTurbine(root, hx.site + corners[i].p, i + 1));
        for (int i = 0; i < EnergyRule.TidalUnits; i++) es.tidal.Add(BuildTidalUnit(root, hx.site + new Vector3(-EnergyRule.CapHalf - 7f, 0f, -30f + i * 60f), i + 1));
        es.storage = BuildEnergyStorage(root, hx.site + new Vector3(EnergyRule.CapHalf - 60f, 0f, -EnergyRule.CapHalf + 60f));
        BuildDeterrents(es, root, hx.site);
        BuildBirdRehab();
        BuildEnergyLine(root, hx.site);
        ValidateEnergySite("al terminar el parque");
    }

    /// <summary>Cada piso declara su demanda (la residencia, la suya, por casas).</summary>
    static void EnsureFloorEnergy()
    {
        foreach (SanctuaryArea a in Object.FindObjectsByType<SanctuaryArea>(FindObjectsInactive.Include, FindObjectsSortMode.None)) FloorEnergy.Declare(a);
        Bedroom[] homes = Object.FindObjectsByType<Bedroom>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (homes.Length > 0)
        {
            FloorEnergy r = _energyRoot.gameObject.GetOrAdd<FloorEnergy>();
            r.floor = "Casas del árbol"; r.areaM2 = 0f; r.fixedKW = homes.Length * EnergyRule.HomeKW;   // (no es un piso: lo cuelga el parque, que es quien lo alimenta)
        }
        FloorEnergy.Refresh();
    }

    // El CAMPO SOLAR: la rejilla de plazas fuera del pie del árbol, de las torres y del almacén, de dentro hacia fuera; las primeras, montadas, y las demás, a la espera.
    static void BuildSolarField(EnergySite es, Transform root, Helix hx)
    {
        Transform field = new GameObject("Campo_solar").transform; field.SetParent(root, false); field.position = hx.site;
        float H = EnergyRule.CapHalf, L = EnergyRule.TableLength, keep = hx.h.OuterHalf + 23f;   // el tronco, sus tornapuntas y las alfombras del suelo (bajo los pisos caben: la sombra ya la descuenta el rendimiento)
        var spots = new List<Vector3>();
        for (float x = -H + 6f + L * 0.5f; x + L * 0.5f <= H - 6f; x += L + EnergyRule.TableGap)
            for (float z = -H + 6f; z <= H - 6f; z += EnergyRule.RowPitch)
            {
                if (Mathf.Abs(x) - L * 0.5f < keep && Mathf.Abs(z) < keep) continue;   // el pie del árbol
                if (Mathf.Abs(x) + L * 0.5f > H - 34f && Mathf.Abs(z) > H - 34f) continue;   // las esquinas: las torres
                if (x + L * 0.5f > H - 76f && z < -H + 76f) continue;   // el almacén
                spots.Add(new Vector3(x, 0f, z));
            }
        spots.Sort((a, b) => a.sqrMagnitude.CompareTo(b.sqrMagnitude));
        Color cell = new Color(0.14f, 0.22f, 0.42f), rail = new Color(0.72f, 0.74f, 0.78f), post = new Color(0.55f, 0.56f, 0.6f), red = new Color(0.9f, 0.2f, 0.15f);
        for (int i = 0; i < spots.Count; i++)
        {
            Transform t = new GameObject($"Mesa_solar_{i + 1}").transform; t.SetParent(field, false); t.localPosition = spots[i];
            float h = EnergyRule.TableHeight;
            KitchenProps.C(t, new Vector3(0f, 0.6f + h * 0.5f, 0f), new Vector3(L, h, 0.05f), cell, 0f, "Celulas_bifaciales");
            KitchenProps.C(t, new Vector3(0f, 0.6f + h + 0.04f, 0f), new Vector3(L, 0.08f, 0.1f), rail, 0f, "Marco");
            foreach (float px in new[] { -L * 0.4f, 0f, L * 0.4f }) KitchenProps.C(t, new Vector3(px, 0.3f + h * 0.5f, 0.06f), new Vector3(0.1f, h + 0.6f, 0.08f), post, 0f, "Poste");
            KitchenProps.C(t, new Vector3(0f, 0.6f + h + 0.4f, 0f), new Vector3(0.4f, 0.4f, 0.4f), red, 0f, "Aviso_averia").gameObject.SetActive(false);
            BoxCollider bc = t.gameObject.AddComponent<BoxCollider>(); bc.center = new Vector3(0f, 0.3f + h * 0.5f, 0f); bc.size = new Vector3(L, h + 0.6f, 0.2f);
            es.tables.Add(t);
        }
        EnergyRule.Report r = EnergyRule.Check(0f, 0, spots.Count);
        es.installed = Mathf.Min(spots.Count, r.tablesRequired);
        for (int i = es.installed; i < spots.Count; i++) es.tables[i].gameObject.SetActive(false);
        Debug.Log($"[Energía] Campo solar: {es.installed} mesas montadas de {spots.Count} plazas ({es.FieldKWp:0} kWp): las que piden {r.floors} pisos.");
    }

    // Un AEROGENERADOR: la E-33 de Ross Island, con su pala negra, sus cámaras y su radar.
    static WindTurbineUnit BuildTurbine(Transform root, Vector3 at, int n)
    {
        Color white = new Color(0.92f, 0.93f, 0.94f), black = new Color(0.08f, 0.08f, 0.09f), steel = TreeSteel, dark = new Color(0.2f, 0.21f, 0.23f);
        Transform t = new GameObject($"Aerogenerador_{n}").transform; t.SetParent(root, false); t.position = at;
        t.rotation = Quaternion.Euler(0f, -45f, 0f);   // de cara al viento dominante, del noroeste
        float hub = EnergyRule.HubH, R = EnergyRule.RotorD * 0.5f;
        Solid(t, new Vector3(0f, 0.5f, 0f), new Vector3(6f, 1f, 6f), new Color(0.6f, 0.6f, 0.58f), "Zapata");
        Transform tower = new GameObject("Torre").transform; tower.SetParent(t, false);
        tower.gameObject.AddComponent<MeshFilter>().sharedMesh = RocketModel.Lathe("Torre_aerogenerador", new[] { (0f, 0f), (1.5f, 0f), (0.8f, hub), (0f, hub) }, 12);
        tower.gameObject.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(white);
        KitchenProps.C(t, new Vector3(0f, hub, -0.6f), new Vector3(1.8f, 2f, 4.4f), white, 0f, "Gondola");
        Transform rotor = new GameObject("Rotor").transform; rotor.SetParent(t, false); rotor.localPosition = new Vector3(0f, hub, 2f);
        KitchenProps.C(rotor, Vector3.zero, new Vector3(1.2f, 1.2f, 1.2f), white, 0f, "Buje");
        Transform black0 = null;
        for (int b = 0; b < 3; b++)
        {
            Transform arm = new GameObject("Pala").transform; arm.SetParent(rotor, false); arm.localRotation = Quaternion.Euler(0f, 0f, b * 120f);
            Transform blade = KitchenProps.C(arm, new Vector3(0f, R * 0.5f + 0.4f, 0f), new Vector3(1.1f, R - 0.8f, 0.2f), b == 0 ? black : white, 0f, b == 0 ? "Pala_negra" : "Pala_blanca");
            if (b == 0) black0 = blade;
        }
        // Las CÁMARAS y el RADAR, en su mástil al pie: paran esta turbina si un ave se acerca.
        Transform det = new GameObject("Detector_aves").transform; det.SetParent(t, false); det.localPosition = new Vector3(6f, 0f, 0f);
        KitchenProps.C(det, new Vector3(0f, 5f, 0f), new Vector3(0.25f, 10f, 0.25f), steel, 0f, "Mastil_detector");
        KitchenProps.C(det, new Vector3(0f, 10.2f, 0f), new Vector3(1f, 0.5f, 1f), dark, 0f, "Camaras_estereo");
        KitchenProps.C(det, new Vector3(0f, 11f, 0f), new Vector3(1.6f, 0.15f, 0.5f), new Color(0.85f, 0.2f, 0.15f), 0f, "Radar_aves");
        Label(det, "Cámaras y radar: un ave a 200 m para el rotor", 11.8f);
        ElevatedElement ee = t.gameObject.AddComponent<ElevatedElement>(); ee.label = $"el aerogenerador {n}"; ee.top = EnergyRule.TurbineTop;
        WindTurbineUnit u = t.gameObject.AddComponent<WindTurbineUnit>();
        u.rotor = rotor; u.blackBlade = black0; u.detector = det;
        return u;
    }

    // La TURBINA MAREAL piloto: el rotor lento dentro de su anillo, colgado de una pontona pegada al casquete, y su sonar.
    static TidalTurbineUnit BuildTidalUnit(Transform root, Vector3 at, int n)
    {
        Color deck = new Color(0.55f, 0.57f, 0.6f), yel = new Color(0.95f, 0.8f, 0.2f), blade = new Color(0.85f, 0.85f, 0.8f), steel = TreeSteel;
        Transform p = new GameObject($"Pontona_mareal_{n}").transform; p.SetParent(root, false); p.position = at;
        Solid(p, new Vector3(0f, -0.1f, 0f), new Vector3(14f, 0.6f, 16f), deck, "Pontona");
        Transform u = new GameObject($"Turbina_mareal_{n}").transform; u.SetParent(p, false); u.localPosition = new Vector3(0f, -7f, 0f);
        u.localRotation = Quaternion.Euler(0f, 90f, 0f);
        float R = EnergyRule.TidalRotorD * 0.5f;
        for (int i = 0; i < 16; i++)
        {
            float a = i * 22.5f * Mathf.Deg2Rad;
            KitchenProps.C(u, new Vector3(Mathf.Sin(a) * (R + 0.5f), Mathf.Cos(a) * (R + 0.5f), 0f), new Vector3(2.2f, 0.5f, 1.2f), yel, new Vector3(0f, 0f, -i * 22.5f), "Anillo_mareal");
        }
        Transform rotor = new GameObject("Rotor").transform; rotor.SetParent(u, false);
        KitchenProps.C(rotor, Vector3.zero, new Vector3(1.2f, 1.2f, 1.6f), steel, 0f, "Buje");
        for (int b = 0; b < 3; b++)
        {
            Transform arm = new GameObject("Pala").transform; arm.SetParent(rotor, false); arm.localRotation = Quaternion.Euler(0f, 0f, b * 120f);
            KitchenProps.C(arm, new Vector3(0f, R * 0.5f + 0.3f, 0f), new Vector3(0.8f, R - 0.6f, 0.15f), blade, 0f, "Pala");
        }
        KitchenProps.C(p, new Vector3(0f, -3.5f, 0f), new Vector3(0.4f, 7f, 0.4f), steel, 0f, "Colgante");
        Transform sonar = KitchenProps.C(p, new Vector3(4f, -1.5f, 0f), new Vector3(1f, 1f, 1f), yel, 0f, "Sonar_mareal");
        KitchenProps.C(p, new Vector3(4f, -0.75f, 0f), new Vector3(0.2f, 1.5f, 0.2f), steel, 0f, "Soporte_sonar");
        Label(KitchenProps.C(p, new Vector3(0f, 1.6f, 0f), new Vector3(1.2f, 1.2f, 0.6f), deck, 0f, "Cuadro_mareal"), "Mareal piloto: punta a ≤ 5 m/s y sonar que para a 50 m", 1f);
        TidalTurbineUnit t = u.gameObject.AddComponent<TidalTurbineUnit>();
        t.rotor = rotor; t.sonar = sonar;
        return t;
    }

    // El ALMACÉN: 6 contenedores de baterías con 3 m entre ellos, el silo de arena y la subestación.
    static Transform BuildEnergyStorage(Transform root, Vector3 at)
    {
        Color box = new Color(0.86f, 0.87f, 0.88f), heat = new Color(0.85f, 0.45f, 0.2f), sand = new Color(0.75f, 0.68f, 0.52f), steel = TreeSteel;
        Transform s = new GameObject("Almacen_energia").transform; s.SetParent(root, false); s.position = at;
        float cw = 6.1f, cd = 2.5f, gap = EnergyRule.ContainerGap;
        for (int i = 0; i < EnergyRule.BatteryContainers; i++)
        {
            Transform c = new GameObject($"Bateria_LFP_{i + 1}").transform; c.SetParent(s, false);
            c.localPosition = new Vector3(4f + (i % 2) * (cw + gap) + cw * 0.5f, 0f, -4f - (i / 2) * (cd + gap) - cd * 0.5f);
            Solid(c, new Vector3(0f, 1.3f, 0f), new Vector3(cw, 2.6f, cd), box, "Contenedor");
            KitchenProps.C(c, new Vector3(-cw * 0.5f + 0.6f, 2.8f, 0f), new Vector3(0.9f, 0.4f, 0.9f), heat, 0f, "Calefaccion");
        }
        Label(KitchenProps.C(s, new Vector3(8f, 0.1f, -1.5f), new Vector3(10f, 0.2f, 0.6f), heat, 0f, "Cartel_baterias"), "Baterías: 30 MWh calefactados; si para el reactor, sostienen lo esencial 72 h", 3.4f);
        Transform silo = new GameObject("Silo_arena").transform; silo.SetParent(s, false); silo.localPosition = new Vector3(30f, 0f, -14f);
        Transform shell = new GameObject("Cuerpo").transform; shell.SetParent(silo, false);
        shell.gameObject.AddComponent<MeshFilter>().sharedMesh = RocketModel.Lathe("Silo_arena_cuerpo", new[] { (0f, 0f), (6f, 0f), (6f, 12f), (2f, 14f), (0f, 14f) }, 16);
        shell.gameObject.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(sand);
        shell.gameObject.AddComponent<CapsuleCollider>().radius = 6f; shell.GetComponent<CapsuleCollider>().height = 14f; shell.GetComponent<CapsuleCollider>().center = new Vector3(0f, 7f, 0f);
        Label(silo, "Silo de arena: 30 MWh de calor para los días que sobra", 15f);
        KitchenProps.C(s, new Vector3(30f, 1.25f, -30f), new Vector3(6f, 2.5f, 3f), steel, 0f, "Subestacion");
        return s;
    }

    // LA DISUASIÓN (docs/energia.md §7): figuras con ojos, ocultas hasta que salta un detector, el dragón de drones en su hangar y la caja para las aves heridas. Ni las figuras ni el dragón cuentan para la seguridad.
    static void BuildDeterrents(EnergySite es, Transform root, Vector3 site)
    {
        float H = EnergyRule.CapHalf - 3f;
        for (int i = 0; i < 8; i++)
        {
            Vector3 d = Quaternion.Euler(0f, i * 45f + 22.5f, 0f) * Vector3.forward;
            es.figureSpots.Add(site + d * (H / Mathf.Max(Mathf.Abs(d.x), Mathf.Abs(d.z))));
        }
        Color black = new Color(0.07f, 0.07f, 0.08f), yellow = new Color(0.98f, 0.8f, 0.1f), steel = TreeSteel;
        for (int i = 0; i < 4; i++)
        {
            Transform f = new GameObject($"Figura_aterradora_{i + 1}").transform; f.SetParent(root, false); f.position = es.figureSpots[i * 2];
            f.rotation = Quaternion.LookRotation(f.position - site);
            KitchenProps.C(f, new Vector3(0f, 6f, 0f), new Vector3(0.3f, 12f, 0.3f), steel, 0f, "Mastil_figura");
            KitchenProps.C(f, new Vector3(0f, 12f, 0f), new Vector3(9f, 3f, 0.2f), black, 0f, "Alas");
            KitchenProps.C(f, new Vector3(0f, 12.5f, 0f), new Vector3(3.6f, 3.6f, 0.2f), black, 0f, "Cabeza");
            foreach (float x in new[] { -0.9f, 0.9f })
            {
                KitchenProps.C(f, new Vector3(x, 13f, 0.15f), new Vector3(1.2f, 1.2f, 0.1f), yellow, 0f, "Ojo");
                KitchenProps.C(f, new Vector3(x, 13f, 0.22f), new Vector3(0.5f, 0.5f, 0.06f), black, 0f, "Pupila");
            }
            f.gameObject.SetActive(false);
            es.figures.Add(f);
        }
        Transform pad = new GameObject("Hangar_drones").transform; pad.SetParent(root, false); pad.position = site + new Vector3(52f, 0f, -98f);
        KitchenProps.C(pad, new Vector3(0f, 0.1f, 0f), new Vector3(14f, 0.2f, 10f), new Color(0.3f, 0.32f, 0.35f), 0f, "Plataforma_drones");
        Label(pad, "Dragón de drones: adorno del juego, sin pruebas de que ahuyente aves. Las turbinas paran igual", 3f);
        DroneDragon dd = pad.gameObject.AddComponent<DroneDragon>();
        const int n = 48;
        Color head = new Color(0.9f, 0.15f, 0.1f), tail = new Color(0.95f, 0.6f, 0.15f);
        for (int i = 0; i < n; i++)
        {
            float k = i / (n - 1f), s = Mathf.Lerp(i == 0 ? 2.4f : 1.2f, 0.4f, k);
            Transform dr = KitchenProps.C(pad, new Vector3(-6f + (i % 8) * 1.7f, 0.6f, -4f + (i / 8) * 1.5f), Vector3.one * s, Color.Lerp(head, tail, k), 0f, $"Dron_{i + 1}");
            dd.drones.Add(dr);
        }
        es.dragon = dd;
        Transform box = KitchenProps.C(root, root.InverseTransformPoint(site + new Vector3(40f, 0.5f, -60f)), new Vector3(1.2f, 1f, 0.9f), new Color(0.75f, 0.85f, 0.78f), 0f, "Caja_transporte_aves");
        Label(box, "Caja para aves heridas: van al refugio del Hospital, no se quedan aquí", 1.4f);
    }

    // EL REFUGIO DE AVES: 6 cajas en la planta veterinaria, donde quede libre, lejos de las turbinas.
    static void BuildBirdRehab()
    {
        Transform up = null;
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (t.name == "Planta_veterinaria") { up = t; break; }
        if (up == null) { Debug.LogError("[Refugio de aves] No encuentro la planta veterinaria."); return; }
        const float w = 8.4f, d = 2.2f, need = 2f;
        var blockers = new List<Bounds>();
        foreach (Renderer r in up.GetComponentsInChildren<Renderer>()) if (r.enabled && r.bounds.size.y < 20f && r.bounds.max.y > up.position.y + 0.3f) blockers.Add(r.bounds);
        float half = 15f; Vector3? spot = null;
        for (float z = -half; z <= half && spot == null; z += 1f)
            for (float x = -half; x <= half && spot == null; x += 1f)
            {
                Vector3 c = up.TransformPoint(new Vector3(x, 1f, z));
                var box = new Bounds(c, new Vector3(w + 2f * need, 2f, d + 2f * need));
                if (!blockers.Exists(b => b.Intersects(box))) spot = new Vector3(x, 0.1f, z);
            }
        if (spot == null) { spot = new Vector3(0f, 0.1f, -half); Debug.LogWarning("[Refugio de aves] Sin sitio con 2 m libres en la planta veterinaria: va al fondo."); }
        Transform rp = new GameObject("Refugio_aves").transform; rp.SetParent(up, false); rp.localPosition = spot.Value;
        BirdRehabPoint bp = rp.gameObject.AddComponent<BirdRehabPoint>();
        Color wood = new Color(0.62f, 0.5f, 0.36f), straw = new Color(0.85f, 0.75f, 0.45f), bird = new Color(0.55f, 0.45f, 0.38f);
        for (int i = 0; i < 6; i++)
        {
            Transform c = new GameObject($"Caja_recuperacion_{i + 1}").transform; c.SetParent(rp, false); c.localPosition = new Vector3(-3.5f + i * 1.4f, 0f, 0f);
            KitchenProps.C(c, new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 1.2f), wood, 0f, "Caja");
            KitchenProps.C(c, new Vector3(0f, 0.82f, 0f), new Vector3(1.05f, 0.04f, 1.05f), straw, 0f, "Paja");
            Transform b = new GameObject("Ave_en_recuperacion").transform; b.SetParent(c, false); b.localPosition = new Vector3(0f, 0.95f, 0f);
            KitchenProps.C(b, Vector3.zero, new Vector3(0.3f, 0.2f, 0.45f), bird, 0f, "Cuerpo_ave");
            KitchenProps.C(b, new Vector3(0f, 0.14f, 0.2f), new Vector3(0.14f, 0.14f, 0.16f), bird, 0f, "Cabeza_ave");
            KitchenProps.C(b, new Vector3(0.2f, 0.02f, 0f), new Vector3(0.1f, 0.05f, 0.3f), new Color(0.95f, 0.95f, 0.92f), 0f, "Venda_ala");
            b.gameObject.SetActive(false);
            bp.birds.Add(b.gameObject);
        }
        Label(rp, "Refugio de aves: se curan y se sueltan aquí, lejos de las turbinas. Con guantes y mascarilla: gripe aviar", 2f);
    }

    // La LÍNEA SUBMARINA al santuario, señalizada con boyas (como la del sitio nuclear).
    static void BuildEnergyLine(Transform root, Vector3 site)
    {
        Vector3 to = EntrenamientoCenter;
        float len = Vector3.Distance(site, to);
        for (float t = EnergyRule.CapHalf * 1.42f + 20f; t < len; t += 40f)
        {
            Vector3 p = Vector3.MoveTowards(site, to, t);
            if (PolarLandscapeBuilder.NaturalIce(p.x, p.z)) break;
            Transform b = KitchenProps.C(root, root.InverseTransformPoint(p + Vector3.up * 0.3f), new Vector3(0.6f, 0.8f, 0.6f), new Color(0.2f, 0.55f, 0.9f), 0f, "Boya_linea_energia");
            PolarLandscapeBuilder.IgnoreForNav(b.gameObject);
        }
    }

    /// <summary>LA REGLA (docs/energia.md): distancia, turbinas lejos de las islas y del árbol, y oferta ≥ demanda. Si no cumple, error en el registro.</summary>
    static void ValidateEnergySite(string when)
    {
        if (_energyHx == null || _energyRoot == null) return;
        Helix hx = _energyHx;
        EnergySite es = _energyRoot.GetComponent<EnergySite>();
        if (es == null) return;
        EnsureFloorEnergy();
        float nearest = NearestToSite(hx.site, EnergyRule.MinDistance + EnergyRule.CapHalf, hx, out string near);
        var bad = new List<string>();
        if (nearest < EnergyRule.MinDistance) bad.Add($"a {nearest:0} m de {near} (pide ≥ {EnergyRule.MinDistance:0} m)");
        foreach (WindTurbineUnit u in es.turbines)
        {
            Vector3 p = u.transform.position;
            for (float a = 0f; a < 360f; a += 10f)
                for (float r = 0f; r < EnergyRule.IslandBuffer; r += 10f)
                {
                    Vector3 q = p + TreeSpec.Dir(a) * r;
                    if (PolarLandscapeBuilder.NaturalIce(q.x, q.z)) { bad.Add($"{u.name} a {r:0} m de una isla"); a = 360f; break; }
                }
            float toTree = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(hx.site.x, hx.site.z));
            if (toTree < EnergyRule.IceThrowRadius) bad.Add($"{u.name} a {toTree:0} m del árbol (el hielo llega a {EnergyRule.IceThrowRadius:0} m)");
        }
        BirdRehabPoint rehab = Object.FindAnyObjectByType<BirdRehabPoint>(FindObjectsInactive.Include);
        if (rehab == null) bad.Add("no hay refugio de aves");
        else foreach (WindTurbineUnit u in es.turbines) if (Vector3.Distance(u.transform.position, rehab.transform.position) < EnergyRule.RehabMinDistance) bad.Add($"el refugio de aves está a menos de {EnergyRule.RehabMinDistance:0} m de {u.name}");
        EnergyRule.Report rep = EnergyRule.Check(es.FieldKWp + EnergyRule.RooftopKWp(), es.installed, es.tables.Count - es.installed);
        bad.AddRange(rep.problems);
        if (bad.Count > 0) Debug.LogError($"[Energía] Regla ({when}): {string.Join("; ", bad)}.");
        else Debug.Log($"[Energía] Regla ({when}): a {nearest:0} m de {near}; {rep}. Sol ~{EnergyRule.YearYield():0} kWh/kWp, viento {EnergyRule.Turbines * EnergyRule.WindMeanKW():0} kW de media, mareal piloto.");
    }
}
