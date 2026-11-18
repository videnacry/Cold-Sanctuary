using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PRUEBA DEL PILOTO DEL REALISMO: MICROCOSMOS_ELONMUSK (docs/plan-microcosmos-realismo.md, fase 1; Berón 2026-11-18). Carga la escena
/// (aditiva, espera acotada; la descarga al acabar) y mira, con las reglas nuevas:
/// 1. montada con el cuerpo encogido (MicroFloor.folkScale = s) y sus vecinos a la estatura × s;
/// 2. la nave de SpaceX («Fábrica de SpaceX (1 Rocket Road)»): muros y techo, su VISTA DE CORTE con muros y techo reunidos, una puerta
///    de ≥ 1,2 × 1,25 m y la de los aseos ensanchada («supuesto»); y la vista de corte funciona: con el punto de vista dentro, el techo
///    se desvanece;
/// 3. el HITO (P5) y los TRANSPORTES (P6): sin consejos de hito ni de transporte; el Roadster y la carretilla tienen su MicroVehicle;
/// 4. las NECESIDADES BÁSICAS dentro de la nave (dormir, aseo, lavarse, comer, sentarse, cocinar), mesas mixtas con apoyos de abdomen
///    y tarimas de puesto delante de las consolas;
/// 5. el reto de CodeShip: «Consola de datos del vuelo 3» está bajo la historia;
/// 6. cumple las reglas de edificación (MicroBuildRules.Validate: 0 faltas).
/// Sin la escena en Build Settings, falla con el menú que la genera.
/// </summary>
public class MuskPilotTest : MonoBehaviour, ITestUnit
{
    public int Group => 124;
    public bool ParallelSafe => false;   // carga y descarga una escena
    public float loadTimeout = 180f;
    const string Scene = "Microcosmos_ElonMusk";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<MuskPilotTest>() == null) tr.gameObject.AddComponent<MuskPilotTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Piloto del realismo: la fábrica de SpaceX en Hawthorne (2008)");
        if (!Application.CanStreamedLevelBeLoaded(Scene))
        {
            TestProbe.Check($"«{Scene}» está en Build Settings", false, "genera «Tools/Cold Sanctuary/Build Microcosmos de famosos/Elon Musk (Hawthorne, 2008)» y vuelve a hornear el NavMesh");
            TestProbe.End(); yield break;
        }
        Scene s = SceneManager.GetSceneByName(Scene);
        bool loadedHere = false;
        if (!s.isLoaded)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(Scene, LoadSceneMode.Additive);
            float until = Time.realtimeSinceStartup + loadTimeout;
            while (op != null && !op.isDone && Time.realtimeSinceStartup < until) yield return null;
            s = SceneManager.GetSceneByName(Scene);
            if (!TestProbe.Check($"«{Scene}» se carga", s.isLoaded, $"no cargó en {loadTimeout:0} s")) { TestProbe.End(); yield break; }
            loadedHere = true;
        }
        yield return null; yield return null;

        MicroFloor floor = null; StoryMission story = null; AreaFunctionSite fab = null;
        var folk = new List<CubeInsect>();
        foreach (GameObject g in s.GetRootGameObjects())
        {
            if (floor == null) floor = g.GetComponentInChildren<MicroFloor>(true);
            if (story == null) story = g.GetComponentInChildren<StoryMission>(true);
            foreach (AreaFunctionSite a in g.GetComponentsInChildren<AreaFunctionSite>(true)) if (a.title != null && a.title.StartsWith("Fábrica de SpaceX")) fab = a;
            folk.AddRange(g.GetComponentsInChildren<CubeInsect>(true).Where(c => c.folk && c.GetComponentInParent<MicroPiece>(true) == null));
        }
        try { Checks(s, floor, story, fab, folk); }
        finally
        {
            if (loadedHere) SceneManager.UnloadSceneAsync(s);
        }
        TestProbe.End();
    }

    static void Checks(Scene s, MicroFloor floor, StoryMission story, AreaFunctionSite fab, List<CubeInsect> folk)
    {
        {
            // 1 · El cuerpo encogido.
            float k = MicroBuildRules.FolkScale;
            TestProbe.Check("montada con el cuerpo encogido (MicroFloor.folkScale = s)", floor != null && Mathf.Approximately(floor.folkScale, k), floor != null ? $"{floor.folkScale}" : "sin MicroFloor");
            var tall = folk.Where(c => { CapsuleCollider cc = c.GetComponent<CapsuleCollider>(); return cc != null && (cc.height < 1.3f * k || cc.height > 2.3f * k); }).Select(c => c.name).Take(6).ToList();
            TestProbe.Check($"los vecinos, a la estatura × s ({1.3f * k:0.00}–{2.3f * k:0.00} m)", folk.Count > 0 && tall.Count == 0, folk.Count == 0 ? "sin vecinos" : string.Join(", ", tall));

            // 2 · La nave cerrada, con su vista de corte y sus puertas.
            if (!TestProbe.NotNull("la nave de SpaceX (AreaFunctionSite «Fábrica de SpaceX…»)", fab)) return;
            CutawayView cv = fab.GetComponent<CutawayView>();
            TestProbe.Check("muros y techo, con su vista de corte (≥ 4 muros y la cubierta reunidos)", cv != null && cv.walls.Count >= 4 && cv.roofs.Count >= 1, cv != null ? $"{cv.walls.Count} muros, {cv.roofs.Count} techos" : "sin CutawayView");
            var doors = fab.GetComponentsInChildren<MicroDoor>(true);
            TestProbe.Check("sus puertas miden ≥ 1,2 × 1,25 m y la de los aseos está ensanchada («supuesto»)", doors.Length >= 2 && doors.All(d => d.width >= MicroBuildRules.DoorWidth - 0.01f && d.height >= MicroBuildRules.DoorHeight - 0.01f) && doors.Any(d => d.widened && d.source.Contains("supuesto")),
                string.Join("; ", doors.Select(d => $"{d.name} {d.width:0.00}×{d.height:0.00}{(d.widened ? " (ensanchada)" : "")}")));
            if (cv != null)
            {
                Vector3 inside = fab.transform.TransformPoint(cv.interior.center), outside = fab.transform.TransformPoint(cv.interior.center + new Vector3(0f, 0f, -cv.interior.extents.z - 15f));
                cv.Refresh(inside, inside + new Vector3(0f, 4f, -8f));
                bool roofGone = cv.roofs.All(r => cv.IsHidden(r));
                cv.Refresh(outside, outside + Vector3.up * 3f);
                TestProbe.Check("la vista de corte funciona: dentro se desvanece el techo; fuera vuelve", roofGone && cv.HiddenCount == 0);
            }

            // 3 · El hito y los transportes.
            var tips = new List<string>();
            foreach (GameObject g in s.GetRootGameObjects()) if (g.GetComponentInChildren<MicroFloor>(true) != null) tips.AddRange(MicroBuildRules.Advise(g.GetComponentInChildren<MicroFloor>(true).transform));
            TestProbe.Check("tiene su hito (P5) y su transporte de época (P6): sin esos consejos", !tips.Any(x => x.Contains("hito") || x.Contains("transporte")), string.Join(" · ", tips.Take(4)));
            var cars = new List<MicroVehicle>(); foreach (GameObject g in s.GetRootGameObjects()) cars.AddRange(g.GetComponentsInChildren<MicroVehicle>(true));
            TestProbe.Check("el Tesla Roadster y la carretilla se pueden usar (MicroVehicle con asiento; la carretilla con su palé)",
                cars.Any(c => c.title.Contains("Roadster") && c.seat != null) && cars.Any(c => c.cargoPoint != null && c.cargoPoint.childCount > 0), string.Join(", ", cars.Select(c => c.title)));

            // 4 · Las necesidades dentro de la nave, las mesas mixtas y las tarimas.
            var lack = MicroBuildRules.Needs.Where(n => !MicroBuildRules.Covers(fab.transform, n)).ToList();
            TestProbe.Check("dentro de la nave: dormir, aseo, lavarse, comer, sentarse y cocinar", lack.Count == 0, lack.Count == 0 ? "todas" : "faltan " + string.Join(", ", lack));
            int rests = fab.GetComponentsInChildren<Transform>(true).Count(x => x.name == "Apoyo de abdomen");
            int platforms = fab.GetComponentsInChildren<Transform>(true).Count(x => x.name == "Tarima_de_puesto");
            TestProbe.Check("mesas con apoyos de abdomen (C1) y tarimas de puesto (E7) en las mesas y las consolas", rests >= 2 && platforms >= 3, $"{rests} apoyos, {platforms} tarimas");

            // 5 · El reto de CodeShip.
            Transform console = story != null ? story.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == "Consola de datos del vuelo 3") : null;
            TestProbe.Check("«Consola de datos del vuelo 3» está bajo la historia (el reto de código la busca por su nombre)", console != null);

            // 6 · Las reglas.
            if (floor != null)
            {
                var bad = MicroBuildRules.Validate(floor.transform, Scene, true, false);
                TestProbe.Check("cumple las reglas de edificación (paso de 2,4 m, vista de corte y puertas, muebles, necesidades)", bad.Count == 0, string.Join(" · ", bad.Take(8)));
            }
        }
    }
}
