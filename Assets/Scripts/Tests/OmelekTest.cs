using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PRUEBA DEL SEGUNDO ESCENARIO DE LA HISTORIA DE MUSK: MICROCOSMOS_ELONMUSKOMELEK (la isla de lanzamiento, 29-9-2008; docs/microcosmos-musk-
/// historia.md, escena 3). Carga la escena (aditiva, espera acotada; la descarga al acabar) y mira:
/// [MIC-94] su fecha (2008) y que es un PUESTO AISLADO (sin pueblo: MicroFloor.town = false), montado con el cuerpo encogido;
/// [MIC-95] el HITO: el Falcon 1 VERTICAL en su plataforma, y la LANCHA que se usa (MicroVehicle);
/// [MIC-96] los dos edificios cerrados (contenedores y hangar) con su vista de corte y puertas ≥ 1,2 × 1,25 m (la del contenedor, ensanchada);
/// [MIC-97] los SITIOS con su nombre fijo (para los retos de CodeShip) y la historia de 8 pasos con objetivo;
/// [MIC-98] las seis necesidades en los contenedores y 0 faltas de las reglas de edificación.
/// </summary>
public class OmelekTest : MonoBehaviour, ITestUnit
{
    public int Group => 126;
    public bool ParallelSafe => false;
    public float loadTimeout = 180f;
    const string Scene = "Microcosmos_ElonMuskOmelek";
    public static readonly string[] Sites = { "Consola de la plataforma (Omelek)", "Plataforma de lanzamiento", "Tanque de oxígeno líquido (LOX)", "Tanque de queroseno RP-1", "Botellas de helio", "Antena de telemetría", "Muelle de Omelek" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<OmelekTest>() == null) tr.gameObject.AddComponent<OmelekTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Omelek (Kwajalein, 2008): la isla de lanzamiento del Falcon 1");
        if (!Application.CanStreamedLevelBeLoaded(Scene))
        {
            TestProbe.Check($"[MIC-94] «{Scene}» está en Build Settings", false, "genera «Tools/Cold Sanctuary/Build Microcosmos de famosos/Elon Musk: Omelek, el lanzamiento (Kwajalein, 2008)»");
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
            if (!TestProbe.Check($"[MIC-94] «{Scene}» se carga", s.isLoaded, $"no cargó en {loadTimeout:0} s")) { TestProbe.End(); yield break; }
            loadedHere = true;
        }
        yield return null; yield return null;
        try { Checks(s); }
        finally { if (loadedHere) SceneManager.UnloadSceneAsync(s); }
        TestProbe.End();
    }

    static void Checks(Scene s)
    {
        MicroFloor floor = null; MicroEra era = null; StoryMission story = null;
        var sites = new List<AreaFunctionSite>(); var all = new List<Transform>(); var cars = new List<MicroVehicle>();
        foreach (GameObject g in s.GetRootGameObjects())
        {
            if (floor == null) floor = g.GetComponentInChildren<MicroFloor>(true);
            if (era == null) era = g.GetComponentInChildren<MicroEra>(true);
            if (story == null) story = g.GetComponentInChildren<StoryMission>(true);
            sites.AddRange(g.GetComponentsInChildren<AreaFunctionSite>(true));
            all.AddRange(g.GetComponentsInChildren<Transform>(true));
            cars.AddRange(g.GetComponentsInChildren<MicroVehicle>(true));
        }
        TestProbe.Check("[MIC-94] 2008, un puesto aislado (sin pueblo) montado con el cuerpo encogido", era != null && era.year == 2008 && floor != null && !floor.town && Mathf.Approximately(floor.folkScale, MicroBuildRules.FolkScale),
            $"{(era != null ? era.year : 0)}, pueblo {(floor != null && floor.town)}, s = {(floor != null ? floor.folkScale : 0f)}");

        Transform stage = all.FirstOrDefault(t => t.name.StartsWith("Primera_etapa"));
        TestProbe.Check("[MIC-95] el hito: el Falcon 1 vertical en su plataforma (MicroPiece «hito»)", stage != null && stage.lossyScale.y > stage.lossyScale.x * 5f && stage.GetComponentInParent<MicroPiece>() != null && stage.GetComponentInParent<MicroPiece>().kind == MicroBuildRules.Landmark);
        TestProbe.Check("[MIC-95] la lancha del equipo se usa (MicroVehicle con asiento)", cars.Any(c => c.title.Contains("Lancha") && c.seat != null), string.Join(", ", cars.Select(c => c.title)));

        var closed = sites.Where(x => x.title.StartsWith("Contenedores de trabajo") || x.title.StartsWith("Hangar de procesado")).ToList();
        bool cut = closed.Count == 2 && closed.All(x => { CutawayView cv = x.GetComponent<CutawayView>(); return cv != null && cv.walls.Count >= 4 && cv.roofs.Count >= 1; });
        var doors = closed.SelectMany(x => x.GetComponentsInChildren<MicroDoor>(true)).ToList();
        TestProbe.Check("[MIC-96] contenedores y hangar cerrados, con vista de corte", cut, string.Join("; ", closed.Select(x => x.title)));
        TestProbe.Check("[MIC-96] sus puertas miden ≥ 1,2 × 1,25 m y la del contenedor está ensanchada", doors.Count >= 2 && doors.All(d => d.width >= MicroBuildRules.DoorWidth - 0.01f && d.height >= MicroBuildRules.DoorHeight - 0.01f) && doors.Any(d => d.widened),
            string.Join("; ", doors.Select(d => $"{d.width:0.00}×{d.height:0.00}{(d.widened ? " (ensanchada)" : "")}")));

        var lack = Sites.Where(n => !all.Any(t => t.name == n)).ToList();
        TestProbe.Check("[MIC-97] los sitios con su nombre (para los retos de CodeShip)", lack.Count == 0, lack.Count == 0 ? "todos" : "faltan " + string.Join(", ", lack));
        TestProbe.Check("[MIC-97] la historia: 8 pasos, cada uno con su objetivo", story != null && story.steps.Length >= 8 && story.steps.All(p => p.targets != null && p.targets.Length > 0 && p.targets.All(x => x != null)), story != null ? $"{story.steps.Length} pasos" : "sin historia");

        AreaFunctionSite quarters = closed.FirstOrDefault(x => x.title.StartsWith("Contenedores"));
        var need = quarters == null ? MicroBuildRules.Needs.ToList() : MicroBuildRules.Needs.Where(n => !MicroBuildRules.Covers(quarters.transform, n)).ToList();
        TestProbe.Check("[MIC-98] las seis necesidades en los contenedores", need.Count == 0, need.Count == 0 ? "todas" : "faltan " + string.Join(", ", need));
        if (floor != null)
        {
            var bad = MicroBuildRules.Validate(floor.transform, Scene, true, false);
            TestProbe.Check("[MIC-98] cumple las reglas de edificación (paso, vista de corte y puertas, muebles, necesidades)", bad.Count == 0, string.Join(" · ", bad.Take(8)));
        }
    }
}
