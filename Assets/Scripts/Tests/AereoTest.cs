using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PRUEBA DEL SANTUARIO AÉREO (Berón 2026-11-05; docs/sitio-nuclear.md §23):
/// 1. SANTUARIO 1: sin lanzadera ni parque; con su muelle, la red de alfombras, la lógica de la energía y las misiones.
/// 2. SIN ESCENA: un trabajador vuela y vuelve sin cargar nada; las turbinas paran por un petrel; el cable pierde lo que dice la regla.
/// 3. CON ESCENA: carga aditiva medida, presta sus piezas a la lógica y las recupera al descargar, y la plataforma cumple T13.
/// </summary>
public class AereoTest : MonoBehaviour, ITestUnit
{
    public int Group => 110;   // (libre mirando todas las ramas: hasta la 109)
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<AereoTest>() == null) tr.gameObject.AddComponent<AereoTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Santuario Aéreo: muelle, trabajo sin escena, cable, carga y regla de altura");
        AereoLink link = AereoLink.Instance;
        EnergySite es = EnergySite.Instance;
        if (link == null || es == null) { TestProbe.Check("hay muelle y lógica de la energía en el santuario 1", false); TestProbe.End(); yield break; }

        // ── 1. EL SANTUARIO 1 ──
        TestProbe.Check("arranca sin la escena cargada: ni lanzadera ni árbol del parque", AereoBase.Instance == null && LaunchSite.Instance == null && !ArbolCampos.All.Any(a => a != null && (a.building == EnergyRule.Building || a.building == LaunchHeightRule.Building)));
        CarpetPad pad = FindObjectsByType<CarpetPad>(FindObjectsSortMode.None).FirstOrDefault(p => p != null && p.building == AereoRule.Building);
        TestProbe.Check("el muelle es una pista de la red del santuario", pad != null && pad.kind == CarpetPad.Kind.Santuario && pad.Destinations().Count > 0);
        TestProbe.Check("las áreas de allí están a mano de la jornada", WorkdayDirector.AreaTransform(EnergyRule.Area) == link.transform && WorkdayDirector.AreaTransform(LaunchSite.ControlArea) == link.transform);
        TestProbe.Check("sus misiones siguen sin cargar nada", LaunchSite.Missions().Any(m => m.id == "lanz_control") && EnergySite.Missions(1).Any(m => m.id == "sol_mantenimiento"));
        TestProbe.Check("la energía declara lo de allí", FloorEnergy.All.Any(f => f.floor == AereoRule.Building) && es.tableSlots > es.installed && es.turbines.Count == EnergyRule.Turbines && es.tidal.Count == EnergyRule.TidalUnits);
        TestProbe.Check("las turbinas sin cuerpo cuentan como elementos elevados", es.turbines.All(u => u.GetComponent<ElevatedElement>() != null));

        // ── 2. SIN ESCENA ──
        GameObject body = new GameObject("Prueba_aereo");
        MeshRenderer r = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshRenderer>(); Destroy(r.GetComponent<Collider>()); r.transform.SetParent(body.transform, false);
        body.transform.position = link.transform.position + Vector3.up;
        float oldTs = Time.timeScale; Time.timeScale = 4f;
        AereoLink.Note(body, EnergyRule.Area);
        bool went = AereoLink.Board(body), hidden = !r.enabled, carried = FlyingCarpet.Riding(body);
        float t0 = Time.time, r0 = Time.realtimeSinceStartup;
        while (Time.time - t0 < AereoRule.FlightSeconds + 5f && Time.realtimeSinceStartup - r0 < 30f) yield return null;
        TestProbe.Check("vuela sin cargar nada, oculto, y se queda mientras trabaja allí", went && hidden && carried && AereoLink.IsAway(body) && AereoBase.Instance == null);
        AereoLink.Note(body, "Cocina");
        t0 = Time.time; r0 = Time.realtimeSinceStartup;
        while (AereoLink.IsAway(body) && Time.time - t0 < AereoRule.FlightSeconds + 30f && Time.realtimeSinceStartup - r0 < 30f) yield return null;
        TestProbe.Check("cuando su jornada lo lleva a otra parte, vuelve al muelle", !AereoLink.IsAway(body) && r.enabled && !FlyingCarpet.Riding(body));
        Time.timeScale = oldTs;
        Destroy(body);
        GameObject bird = new GameObject("Petrel_de_prueba"); bird.transform.position = es.turbines[0].transform.position + new Vector3(50f, 20f, 0f);
        es.turbines[0].ClearForTest();
        EnergySite.TestBirds.Add(bird.transform);
        es.ScanNow();
        TestProbe.Check("un ave cerca para la turbina, sin cargar la escena", es.turbines[0].Stopped);
        EnergySite.TestBirds.Remove(bird.transform); Destroy(bird); es.turbines[0].ClearForTest();
        TestProbe.Check("los petreles del santuario cuentan como aves", SnowPetrelLife.FindObjectsByType<SnowPetrelLife>(FindObjectsSortMode.None).All(p => p.GetComponent<BirdBehavior>() != null || p.GetComponent<Animal>() != null));
        TestProbe.Check("LA REGLA DEL CABLE: pierde lo que dice y se ve la línea en el santuario 1", AereoRule.CableEfficiency < 1f && AereoRule.CableEfficiency > 0.9f && link.GetComponentsInChildren<Transform>(true).Count(t => t.name == "Boya_linea_energia") >= 2);
        int installed0 = es.installed, soilOk = es.Maintained;
        AreaFunctions.Run("sol_mantenimiento", "prueba");
        TestProbe.Check("limpiar el campo funciona sin la escena", es.Maintained == soilOk + 1 && es.installed == installed0);

        // ── 3. CON ESCENA ──
        if (!Application.CanStreamedLevelBeLoaded(AereoRule.Scene)) { TestProbe.Check("el Santuario Aéreo está construido y en Build Settings", false, "Tools/Cold Sanctuary/Build Santuario Aéreo"); TestProbe.End(); yield break; }
        long mem0 = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
        float rl = Time.realtimeSinceStartup;
        yield return AereoLink.LoadForTest();
        float loadS = Time.realtimeSinceStartup - rl, mb = (UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() - mem0) / (1024f * 1024f);
        AereoBase b = AereoBase.Instance;
        int n = b != null ? b.GetComponentsInChildren<Transform>(true).Length : 0, drawn = b != null ? b.GetComponentsInChildren<Renderer>(true).Length : 0;
        TestProbe.Check("carga durante el vuelo", b != null && loadS <= AereoRule.FlightSeconds + AereoRule.LoadLimit, $"MEDIDO: {loadS:0.0} s, {mb:0} MB, {n} objetos, {drawn} con malla");
        Debug.Log($"[Aéreo] MEDIDO al cargar: {loadS:0.0} s, {mb:0} MB, {n} objetos, {drawn} con malla.");
        if (b != null)
        {
            TestProbe.Check("presta sus piezas a la lógica", es.tables.Count == es.tableSlots && es.turbines[0].rotor != null && es.storage != null && es.figures.Count > 0, $"{es.tables.Count} mesas");
            TestProbe.Check("las mesas montadas se ven; las demás, no", es.tables.Take(es.installed).All(t => t.gameObject.activeSelf) && es.tables.Skip(es.installed).All(t => !t.gameObject.activeSelf));
            LaunchSite ls = LaunchSite.Instance;
            TestProbe.Check("T13: la plataforma cumple, contando también el santuario 1", ls != null && ls.PadY >= LaunchHeightRule.RequiredPadHeight(out _) - 0.01f, ls != null ? $"{ls.PadY:0} m; {ls.LastWhy}" : "");
            TestProbe.Check("sin alfombras allí: se sale por los andenes", b.GetComponentsInChildren<CarpetPad>(true).Length == 0 && b.launchArrival != null && b.energyArrival != null);
            // LA DETECCIÓN se fue con sus casquetes: un radar y un sonar en cada uno (el del sitio nuclear lo cuenta NuclearAccessTest).
            Transform[] parts = b.GetComponentsInChildren<Transform>(true);
            System.Func<string, IcebergRule.Site, int> onCap = (name, s) => parts.Count(t => t.name == name && IcebergRule.FlatDistance(t.position, s) < IcebergRule.DangerRadius(s));
            int rl1 = onCap("Radar_hielo", IcebergRule.Site.Launch), re1 = onCap("Radar_hielo", IcebergRule.Site.Energy), sl1 = onCap("Sonar_quillas", IcebergRule.Site.Launch), se1 = onCap("Sonar_quillas", IcebergRule.Site.Energy);
            TestProbe.Check("radar y sonar en los casquetes de la lanzadera y del parque", rl1 == 1 && re1 == 1 && sl1 == 1 && se1 == 1, $"lanzadera {rl1}+{sl1}, parque {re1}+{se1}");
            Vector3 launchAt = b.launchArrival.position, energyAt = b.energyArrival.position;
            TestProbe.Check("los andenes, sobre sus casquetes", IcebergRule.FlatDistance(launchAt, IcebergRule.Site.Launch) < 60f && IcebergRule.FlatDistance(energyAt, IcebergRule.Site.Energy) < 150f && AereoRule.InSector(launchAt) && AereoRule.InSector(energyAt));
            TestProbe.Check("sus pisos no se cuentan dos veces", b.GetComponentsInChildren<FloorEnergy>(true).Length == 0);
            TestProbe.Check("las aves frías, lejos de las turbinas al empezar", b.transform.Find("Fauna_fria_aerea") != null && es.turbines.All(u => Vector3.Distance(u.transform.position, b.transform.Find("Fauna_fria_aerea").GetChild(0).position) > EnergyRule.BirdStopRadius));
        }
        yield return AereoLink.UnloadForTest();
        TestProbe.Check("se descarga limpia y la lógica sigue", AereoBase.Instance == null && es.tables.Count == 0 && es.storage == null && EnergySite.Instance == es);
        TestProbe.End();
    }
}
