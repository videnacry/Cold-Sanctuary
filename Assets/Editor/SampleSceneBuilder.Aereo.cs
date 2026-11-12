using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// EL SANTUARIO AÉREO, escena propia (Berón 2026-11-05; docs/sitio-nuclear.md §23): la lanzadera y el parque energético, en sus coordenadas de siempre, que al cargarse se ponen sobre el mismo mar.
/// Sin alfombras dentro: se llega y se sale por el muelle. Y en el santuario 1, <see cref="BuildAereoDock"/>: el muelle y la lógica de la energía, que no puede irse porque da luz a todo.
/// </summary>
public static partial class SampleSceneBuilder
{
    public const string AereoScenePath = "Assets/Scenes/" + AereoRule.Scene + ".unity";
    public static readonly Vector3 AereoDock = new Vector3(-300f, 0f, 170f);

    [MenuItem("Tools/Cold Sanctuary/Build Santuario Aéreo (lanzadera y parque)")]
    public static void BuildAereoScene()
    {
        MicrocosmosAreaSceneBuilder.DropUntitled();
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        _platforms.Clear(); _upperOf.Clear(); BuildingNames.Clear(); _helixes.Clear(); _helixFloor.Clear(); _floorSeq.Clear(); _orchardTips.Clear(); _towerHelix = null;
        GameObject go = new GameObject("Santuario_Aereo");
        Transform root = go.transform;
        AereoBase b = go.AddComponent<AereoBase>();

        BuildLaunchSite(root);
        WireMeditationArea(root, "Control_de_mision", LaunchSite.ControlArea, new Color(0.45f, 0.55f, 0.7f), MissionKind.Channel, null, virtualOnly: true);
        BeginEnergyTree(root);
        FinishHelixes();
        Helix launchHx = _launchHx;
        FinishLaunchSite();
        BuildEnergyVisuals(b, _energyRoot, _energyHx);
        // Sin alfombras aquí: se sale por el andén. (Una alfombra de allí que volase al santuario desaparecería al descargar la escena.)
        // Después de FinishLaunchSite, que pone la de la plataforma: antes de él, esa sobrevivía (prueba 110, ronda 12).
        foreach (CarpetPad pad in root.GetComponentsInChildren<CarpetPad>(true)) Object.DestroyImmediate(pad.gameObject);
        ValidateLaunchHeight("al construir el Santuario Aéreo");
        // Las aves frías de territorio, lejos de las turbinas (a un ave que se acerca, paran). El agua es el mar del santuario 1: esta escena siempre se carga encima de él.
        PlaceAirColdFauna(root, LaunchSiteCenter + new Vector3(0f, 0f, -220f), 120f, SeaRule.Surface);

        b.launchArrival = AereoArrival(root, LaunchSiteCenter + new Vector3(LaunchCapHalf - 14f, 0f, 0f), "Andén de la lanzadera: se vuelve al muelle");
        b.energyArrival = AereoArrival(root, EnergyRule.Center + new Vector3(EnergyRule.CapHalf - 12f, 0f, 30f), "Andén del parque: se vuelve al muelle");

        // Sin alfombras, el ascensor del tronco lleva del andén a cada piso, al techo y a la plataforma; no sale nunca de la escena.
        LaunchSite launchSite = root.GetComponentInChildren<LaunchSite>(true);
        BuildAereoLift(root, launchHx, b.launchArrival, launchSite != null ? launchSite.deck : null);
        BuildAereoLift(root, _energyHx, b.energyArrival, null);

        EditorSceneManager.SaveScene(scene, AereoScenePath);
        var bs = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!bs.Exists(s => s.path == AereoScenePath)) bs.Add(new EditorBuildSettingsScene(AereoScenePath, true));
        EditorBuildSettings.scenes = bs.ToArray();
        AssetDatabase.SaveAssets();
        int pieces = root.GetComponentsInChildren<Transform>(true).Length, drawn = root.GetComponentsInChildren<Renderer>(true).Length;
        Debug.Log($"[Aéreo] {AereoScenePath}: {pieces} objetos, {drawn} con malla (lo que sale del santuario 1).");
        _helixes.Clear(); _energyRoot = null; _energyHx = null;
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
        EditorSceneManager.CloseScene(scene, removeScene: true);
    }

    static Transform AereoArrival(Transform root, Vector3 at, string sign)
    {
        Transform t = new GameObject("Anden_aereo").transform; t.SetParent(root, false); t.position = at;
        KitchenProps.C(t, new Vector3(0f, 0.05f, 0f), new Vector3(6f, 0.1f, 6f), new Color(0.25f, 0.27f, 0.3f), 0f, "Piso_anden");
        BoxCollider trig = t.gameObject.AddComponent<BoxCollider>(); trig.isTrigger = true; trig.center = new Vector3(0f, 1f, 3.5f); trig.size = new Vector3(4f, 2f, 1.5f);
        t.gameObject.AddComponent<AereoGate>();
        Label(t, sign, 3f);
        return t;
    }

    // EL ASCENSOR DEL TRONCO: una parada junto a la puerta de cada piso (ese sitio siempre está libre), otra en el techo y, en la lanzadera, otra donde iba la alfombra de la plataforma.
    static void BuildAereoLift(Transform root, Helix hx, Transform arrival, Transform launchDeck)
    {
        if (hx == null || hx.ac == null || arrival == null) { Debug.LogError($"[Aéreo] Sin ascensor en {hx?.building}: falta el árbol o el andén."); return; }
        Transform t = new GameObject($"Ascensor_{hx.rootName}").transform; t.SetParent(root, false);
        AereoLift lift = t.gameObject.AddComponent<AereoLift>();
        lift.building = hx.building;
        lift.stops.Add(arrival); lift.labels.Add("Andén");
        arrival.GetComponent<AereoGate>().lift = lift;
        foreach (TreeLevel l in hx.ac.levels.FindAll(x => x.kind == TreeLevel.Kind.Floor))
            AddLiftStop(lift, l.deck, hx.ac.World(l.outside), l.deck.rotation, char.ToUpper(l.name[0]) + l.name.Substring(1));
        AddLiftStop(lift, hx.ac.Top.deck, hx.ac.World(hx.ac.Top.outside), hx.ac.Top.deck.rotation, "El techo");
        if (launchDeck != null) AddLiftStop(lift, launchDeck, launchDeck.TransformPoint(new Vector3(-7f, 0.03f, -7f)), launchDeck.rotation, "Plataforma de lanzamiento");
        Debug.Log($"[Aéreo] Ascensor de {hx.building}: {lift.stops.Count} paradas, hasta {lift.stops[lift.stops.Count - 1].position.y:0} m.");
    }

    static void AddLiftStop(AereoLift lift, Transform deck, Vector3 at, Quaternion rot, string label)
    {
        Transform s = new GameObject("Parada_ascensor").transform; s.SetParent(deck, true); s.SetPositionAndRotation(at, rot);
        PolarLandscapeBuilder.IgnoreForNav(KitchenProps.C(s, new Vector3(0f, 0.01f, 0f), new Vector3(2.4f, 0.02f, 2.4f), new Color(0.3f, 0.5f, 0.6f), 0f, "Piso_parada").gameObject);
        BoxCollider trig = s.gameObject.AddComponent<BoxCollider>(); trig.isTrigger = true; trig.center = Vector3.up; trig.size = new Vector3(2.4f, 2f, 2.4f);
        s.gameObject.AddComponent<AereoLiftStop>().lift = lift;
        Label(s, $"Ascensor: {label}", 2.6f);
        lift.stops.Add(s); lift.labels.Add(label);
    }

    // Lo que se ve del parque: el campo, las turbinas (sin su lógica: la tienen abajo), la mareal, el almacén y las figuras.
    static void BuildEnergyVisuals(AereoBase b, Transform root, Helix hx)
    {
        BuildSolarField(b, root, hx);
        float H = EnergyRule.CapHalf - 16f;
        for (int i = 0; i < EnergyRule.Turbines; i++)
        {
            WindTurbineUnit u = BuildTurbine(root, hx.site + TurbineCorner(i, H), i + 1);
            b.rotors.Add(u.rotor); b.blades.Add(u.blackBlade); b.detectors.Add(u.detector);
            Object.DestroyImmediate(u);
        }
        for (int i = 0; i < EnergyRule.TidalUnits; i++)
        {
            TidalTurbineUnit t = BuildTidalUnit(root, hx.site + TidalSpot(i), i + 1);
            b.tidalRotors.Add(t.rotor); b.tidalSonars.Add(t.sonar);
            Object.DestroyImmediate(t);
        }
        b.storage = BuildEnergyStorage(root, hx.site + new Vector3(EnergyRule.CapHalf - 60f, 0f, -EnergyRule.CapHalf + 60f));
        BuildDeterrents(b, root, hx.site);
    }

    static Vector3 TurbineCorner(int i, float H) => i == 0 ? new Vector3(-H, 0f, H) : i == 1 ? new Vector3(H, 0f, H) : new Vector3(-H, 0f, -H);
    static Vector3 TidalSpot(int i) => new Vector3(-EnergyRule.CapHalf - 7f, 0f, -30f + i * 60f);

    /// <summary>EL MUELLE, en el santuario 1, y lo que se queda de la energía: la lógica (da luz a todo), las turbinas sin cuerpo, para que las aves y el balance funcionen sin cargar la otra escena.</summary>
    static void BuildAereoDock(Transform parent)
    {
        Transform root = new GameObject("Enlace_aereo").transform; root.SetParent(parent, false); root.position = AereoDock;
        _energyRoot = root;
        Solid(root, new Vector3(0f, -0.75f, 0f), new Vector3(32f, 1.5f, 24f), new Color(0.82f, 0.85f, 0.88f), "Muelle_aereo");
        root.gameObject.AddComponent<AereoLink>();
        BoxCollider trig = root.gameObject.AddComponent<BoxCollider>(); trig.isTrigger = true; trig.center = new Vector3(8f, 1f, 0f); trig.size = new Vector3(5f, 2f, 8f);
        Label(root, "Muelle aéreo: lanzadera y parque energético", 4f);
        Transform padHolder = new GameObject("Muelle_alfombras").transform; padHolder.SetParent(root, false);
        CarpetPad.BuildZone(padHolder, AereoDock + new Vector3(-6f, 0.01f, 0f), Quaternion.identity, CarpetPad.Kind.Santuario, AereoRule.Building, 0, $"{AereoRule.Building}#muelle", AereoRule.Building, 3);
        float nearest = NearestToSite(AereoDock, 60f, null, out string near);
        if (nearest < 30f) Debug.LogWarning($"[Aéreo] El muelle queda a {nearest:0} m de {near}.");

        EnergySite es = root.gameObject.AddComponent<EnergySite>();
        EnsureFloorEnergy();
        FloorEnergy up = new GameObject("Pisos_del_aereo").AddComponent<FloorEnergy>(); up.transform.SetParent(root, false);
        up.floor = AereoRule.Building; up.areaM2 = 0f; up.fixedKW = AereoRule.FloorsKW;
        FloorEnergy.Refresh();
        float H = EnergyRule.CapHalf - 16f;
        Vector3 site = EnergyRule.Center;
        for (int i = 0; i < EnergyRule.Turbines; i++)
        {
            Transform t = new GameObject($"Aerogenerador_{i + 1}").transform; t.SetParent(root, false); t.position = site + TurbineCorner(i, H);
            ElevatedElement ee = t.gameObject.AddComponent<ElevatedElement>(); ee.label = $"el aerogenerador {i + 1}"; ee.top = EnergyRule.TurbineTop;
            es.turbines.Add(t.gameObject.AddComponent<WindTurbineUnit>());
        }
        for (int i = 0; i < EnergyRule.TidalUnits; i++)
        {
            Transform t = new GameObject($"Turbina_mareal_{i + 1}").transform; t.SetParent(root, false); t.position = site + TidalSpot(i);
            es.tidal.Add(t.gameObject.AddComponent<TidalTurbineUnit>());
        }
        es.tableSlots = SolarSpots(TowerHelix(new Vector2(32f, 32f), 2, 0f, 10f)).Count;
        EnergyRule.Report r = EnergyRule.Check(0f, 0, es.tableSlots);
        es.installed = Mathf.Min(es.tableSlots, r.tablesRequired);
        Debug.Log($"[Energía] Campo solar: {es.installed} mesas de {es.tableSlots} plazas ({es.FieldKWp:0} kWp), en el Santuario Aéreo; llegan por cable ({AereoRule.CableEfficiency:P0}).");
        BuildBirdRehab();
        BuildEnergyLine(root, site);
        ValidateEnergySite("al terminar el muelle");
    }

    // LA REGLA DEL CABLE: la línea submarina del parque al santuario, señalizada con boyas. Se queda en el santuario 1: es por donde llega la luz, esté la otra escena cargada o no.
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
}
