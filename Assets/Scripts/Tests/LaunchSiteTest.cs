using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA LANZADERA DE COHETES (Berón 2026-11-04; docs/lanzadera-de-cohetes.md):
/// 1. SITIO: en su casquete artificial, a ≥ <see cref="LaunchHeightRule.MinDistance"/> m de cualquier edificio-árbol y de las islas.
/// 2. REGLA DE ALTURA: la plataforma ≥ máx(lo más alto del santuario, techo de vuelo de las aves) + margen, medido en la escena.
/// 3. VALIDADOR: al aparecer un elemento elevado más alto (un árbol que crece por pisos), la plataforma sube sola; al quitarlo se baja
///    a mano a su altura de antes.
/// 4. CONTENCIÓN: deflector y trinchera de llamas, diluvio de agua, cubeta de contención y escudo contra cascotes; el cohete en su mesa.
/// 5. PISOS: «Fábrica de cohetes», «Control de misión» (con su área, puestos y misiones de la jornada) y el garaje del submarino.
/// 6. ACCESO: el submarino de obra llega por la vía marina al garaje de la lanzadera, con alfombras en el suelo, el techo y la plataforma.
/// </summary>
public class LaunchSiteTest : MonoBehaviour, ITestUnit
{
    public int Group => 90;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<LaunchSiteTest>() == null) tr.gameObject.AddComponent<LaunchSiteTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Lanzadera de cohetes: sitio, regla de altura, validador, contención, pisos y acceso");
        LaunchSite ls = FindAnyObjectByType<LaunchSite>();
        ArbolCampos lt = ArbolCampos.All.FirstOrDefault(a => a != null && a.building == LaunchHeightRule.Building);
        if (ls == null || lt == null) { TestProbe.Check("hay una lanzadera de cohetes (su árbol y su plataforma)", false); TestProbe.End(); yield break; }

        // ── 1. SITIO ──
        float nearestTree = ArbolCampos.All.Where(a => a != null && a != lt).Select(a => Vector2.Distance(new Vector2(a.transform.position.x, a.transform.position.z), new Vector2(lt.transform.position.x, lt.transform.position.z))).DefaultIfEmpty(9999f).Min();
        float nearestIce = 9999f;
        GameObject floes = GameObject.Find("Banquisa_Casquetes");
        if (floes != null)
            foreach (Transform b in floes.transform)
            {
                if (b.name != "Casquete") continue;
                Vector3 p = b.position, s = b.localScale, c = lt.transform.position;
                float dx = Mathf.Max(0f, Mathf.Abs(c.x - p.x) - s.x * 0.5f), dz = Mathf.Max(0f, Mathf.Abs(c.z - p.z) - s.z * 0.5f);
                nearestIce = Mathf.Min(nearestIce, Mathf.Sqrt(dx * dx + dz * dz));
            }
        bool cap = lt.transform.parent != null && lt.transform.parent.GetComponentsInChildren<Transform>().Any(t => t.name == "Casquete_artificial");
        TestProbe.Check($"la lanzadera está en su casquete artificial, a ≥ {LaunchHeightRule.MinDistance:0} m de todo edificio-árbol y de las islas (edificio habitado para {LaunchHeightRule.TntKg / 1000f:0.0} t de TNT: {LaunchHeightRule.InhabitedBuildingDistance:0} m, +50 % por cascotes)",
            cap && nearestTree >= LaunchHeightRule.MinDistance && nearestIce >= LaunchHeightRule.MinDistance, $"casquete {cap}; al árbol más cercano {nearestTree:0} m, al hielo del santuario {nearestIce:0} m");

        // ── 2. REGLA DE ALTURA ──
        float need = LaunchHeightRule.RequiredPadHeight(out string why);
        TestProbe.Check("REGLA: la plataforma queda por encima de lo más alto del santuario y del vuelo de las aves, con su margen", ls.PadY >= need - 0.01f, $"plataforma a {ls.PadY:0} m; la regla pide {why}");

        // ── 3. VALIDADOR: un elemento elevado nuevo, más alto ──
        float pad0 = ls.PadY, roof0 = ls.roofY;
        GameObject tall = new GameObject("Elemento_elevado_TEST");
        tall.transform.position = new Vector3(0f, 0f, -300f);
        tall.SetActive(false);
        ElevatedElement ee = tall.AddComponent<ElevatedElement>(); ee.label = "un edificio-árbol que creció por pisos (prueba)"; ee.top = pad0 + 20f;
        int r0 = ls.Raises;
        tall.SetActive(true);   // (al aparecer, valida)
        yield return null;
        float need2 = LaunchHeightRule.RequiredPadHeight(out string why2);
        bool pillarsOk = ls.pillars.All(p => p != null && Mathf.Abs(p.position.y + p.localScale.y * 0.5f - ls.PadY) < 0.1f && Mathf.Abs(p.position.y - p.localScale.y * 0.5f - roof0) < 0.1f);
        TestProbe.Check("VALIDADOR: al aparecer un elemento elevado más alto que la plataforma, la plataforma sube sola (y sus pilares se alargan)",
            ls.Raises > r0 && ls.PadY >= need2 - 0.01f && ls.PadY >= ee.top + LaunchHeightRule.Margin - 0.01f && pillarsOk, $"{pad0:0} → {ls.PadY:0} m; {why2}; pilares {pillarsOk}");
        Destroy(tall); yield return null;
        ls.RaiseTo(pad0);   // a su altura de antes
        TestProbe.Check("sin ese elemento, la plataforma vuelve a cumplir a su altura de antes", ls.PadY >= LaunchHeightRule.RequiredPadHeight(out _) - 0.01f, $"{ls.PadY:0} m");

        // ── 4. CONTENCIÓN ──
        string[] parts = { "Deflector_llamas", "Trinchera_llamas_suelo", "Deposito_diluvio", "Rociador_diluvio", "Cubeta_contencion", "Escudo_cascotes", "Cohete", "Torre_umbilical" };
        var names = ls.GetComponentsInChildren<Transform>(true).Select(t => t.name).ToHashSet();
        var missing = parts.Where(p => !names.Contains(p)).ToList();
        Transform rk = ls.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Cohete");
        float rkH = 0f; if (rk != null) { Bounds b = new Bounds(rk.position, Vector3.zero); foreach (Renderer r in rk.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds); rkH = b.size.y; }
        TestProbe.Check("la plataforma contiene una explosión: deflector y trinchera de llamas (hacia el mar abierto), diluvio de agua, cubeta de contención y escudo contra cascotes del lado del santuario",
            missing.Count == 0, missing.Count == 0 ? $"{parts.Length} piezas" : "faltan: " + string.Join(", ", missing));
        TestProbe.Check("el cohete (clase Electron, 18 m) está en su mesa sobre el hueco de la llama", rk != null && rkH > 16f && rkH < 21f && Mathf.Abs(rk.position.x - ls.deck.position.x) < 0.5f, $"{rkH:0.0} m");

        // ── 5. PISOS ──
        bool fab = lt.GetComponentsInChildren<Transform>().Any(t => t.name == "Fabrica_de_cohetes");
        SanctuaryArea ctl = lt.GetComponentsInChildren<SanctuaryArea>().FirstOrDefault(a => a.displayName == LaunchSite.ControlArea);
        int consoles = ctl != null ? ctl.GetComponentsInChildren<Transform>().Count(t => t.name == "Puesto_control") : 0;
        var missions = LaunchSite.Missions();
        TestProbe.Check($"pisos «{LaunchSite.FactoryFloor}» y «{LaunchSite.ControlArea}» (área, ≥ 8 puestos y sus misiones en la jornada)", fab && ctl != null && consoles >= 8 && missions.Count >= 3 && missions.All(m => m.area == LaunchSite.ControlArea),
            $"fábrica {fab}, control {(ctl != null)}, {consoles} puestos, {missions.Count} misiones");

        // ── 6. ACCESO: alfombras y el submarino de obra ──
        int pads = FindObjectsByType<CarpetPad>(FindObjectsSortMode.None).Count(p => p != null && p.building == LaunchHeightRule.Building);
        TestProbe.Check("se llega en alfombra (suelo del casquete, techo y plataforma; sin escaleras)", pads >= 3, $"{pads} zonas de alfombras");
        ConstructionSubmarine sub = ConstructionSubmarine.Instance;
        TreeLevel garage = lt.levels.FirstOrDefault(l => l.deck != null && l.deck.name == "Garaje_lanzadera");
        if (sub == null || garage == null) TestProbe.Check("el submarino de obra llega por la vía marina al garaje de la lanzadera", false, $"submarino {sub != null}, garaje {garage != null}");
        else
        {
            int d0 = sub.Deliveries; float old = Time.timeScale; Time.timeScale = 4f;
            bool sent = sub.SendTo(lt, garage, "etapas y carga útil");
            float t0 = Time.time, rr = Time.realtimeSinceStartup;
            while (sent && sub.Deliveries == d0 && Time.time - t0 < 900f && Time.realtimeSinceStartup - rr < 300f) yield return null;
            TreeLevel at = lt.LevelAt(sub.transform.position + Vector3.up * 0.5f);
            TestProbe.Check("el submarino de obra lleva las etapas por la vía marina (fondo abierto) al garaje de la lanzadera y aparca", sent && sub.Deliveries > d0 && at == garage,
                $"en {Time.time - t0:0} s de juego; aparcado en {at?.name ?? "ningún nivel"}; holgura mínima con animales {sub.MinClearance:0.0} m");
            float t1 = Time.time, r1 = Time.realtimeSinceStartup;
            while (sub.Now != ConstructionSubmarine.State.Parked && Time.time - t1 < 900f && Time.realtimeSinceStartup - r1 < 300f) yield return null;
            Time.timeScale = old;
        }
        TestProbe.End();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Lanzadera de cohetes (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloLanzadera"); DontDestroyOnLoad(go);
        LaunchSiteTest t = go.AddComponent<LaunchSiteTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(LaunchSiteTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
