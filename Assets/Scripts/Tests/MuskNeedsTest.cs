using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// FASE 2 DEL PLAN (docs/plan-microcosmos-realismo.md §2 y §4.1: «necesidades verificadas»): LA VIDA DE VERDAD EN EL PILOTO
/// Microcosmos_ElonMusk. Carga la escena (aditiva, espera acotada) y, con el reloj adelantado por bloques de un día simulado y el tiempo
/// a ×4, sigue a los insectos-humanos que trabajan en la nave de SpaceX y viven en ella (el turno de noche: CivicBuilder.NightShiftHome):
/// 1. [MIC-89] TRABAJAN: a las 9 (bloque de trabajo) llegan andando a su puesto de la nave (a su sitio de trabajo, MicroJornada.WorkSpotOf);
/// 2. [MIC-90] COMEN en la MESA ALTA con apoyos de abdomen (C1): a las 11 (la comida) o con hambre en casa, «comer» junto a una mesa
///    que lleva sus «Apoyo de abdomen»;
/// 3. [MIC-91] VAN AL RETRETE de la nave: con ganas (0,95), «bano» en el retrete de su hogar, dentro de la nave;
/// 4. [MIC-92] DUERMEN en su catre de la nave: a las 22:30 con sueño, «dormir» en una cama de la nave;
/// 5. [MIC-93] SIN TELETRANSPORTES: ni la jornada (MicroJornada.Fallbacks) ni el hogar (HomeNeedSpell.Fallbacks) tuvieron que ponerlos
///    en su sitio (los 60 s / 45 s de tope se agotaron andando).
/// Cada espera, acotada en tiempo REAL. Sin la escena en Build Settings, falla con el menú que la genera.
/// </summary>
public class MuskNeedsTest : MonoBehaviour, ITestUnit
{
    public int Group => 125;   // tras el piloto (124); carga y descarga la escena
    public bool ParallelSafe => false;
    public float loadTimeout = 180f;
    const string Scene = "Microcosmos_ElonMusk";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<MuskNeedsTest>() == null) tr.gameObject.AddComponent<MuskNeedsTest>();
    }

    readonly List<(GameObject who, string key, Vector3 at)> _events = new List<(GameObject, string, Vector3)>();
    HashSet<GameObject> _subjects = new HashSet<GameObject>();

    void OnEvent(GameObject who, string key) { if (who != null && _subjects.Contains(who)) _events.Add((who, key, who.transform.position)); }

    public IEnumerator Run()
    {
        TestProbe.Begin("Fase 2: en la nave de SpaceX trabajan, comen de pie, van al retrete y duermen (un día simulado)");
        if (!Application.CanStreamedLevelBeLoaded(Scene))
        {
            TestProbe.Check($"[MIC-89] «{Scene}» está en Build Settings", false, "genera «Tools/Cold Sanctuary/Build Microcosmos de famosos/Elon Musk (Hawthorne, 2008)»");
            TestProbe.End(); yield break;
        }
        TimeController tc = TimeController.timeController;
        if (!TestProbe.Check("[MIC-89] hay reloj de juego (TimeController)", tc != null)) { TestProbe.End(); yield break; }
        Scene s = SceneManager.GetSceneByName(Scene);
        bool loadedHere = false;
        if (!s.isLoaded)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(Scene, LoadSceneMode.Additive);
            float until = Time.realtimeSinceStartup + loadTimeout;
            while (op != null && !op.isDone && Time.realtimeSinceStartup < until) yield return null;
            s = SceneManager.GetSceneByName(Scene);
            if (!TestProbe.Check($"[MIC-89] «{Scene}» se carga", s.isLoaded, $"no cargó en {loadTimeout:0} s")) { TestProbe.End(); yield break; }
            loadedHere = true;
        }
        yield return null; yield return null;

        // Los sujetos: los de la nave que viven en ella (su hogar cuelga de la nave).
        AreaFunctionSite fab = null; var crew = new List<MicroJornada>();
        foreach (GameObject g in s.GetRootGameObjects())
        {
            foreach (AreaFunctionSite a in g.GetComponentsInChildren<AreaFunctionSite>(true)) if (a.title != null && a.title.StartsWith("Fábrica de SpaceX")) fab = a;
            foreach (MicroJornada j in g.GetComponentsInChildren<MicroJornada>(true)) crew.Add(j);
        }
        crew = crew.Where(j => fab != null && j.work == fab && j.TryGetComponent(out PersonLife pl) && pl.home != null && pl.home.transform.IsChildOf(fab.transform)).ToList();
        _subjects = new HashSet<GameObject>(crew.Select(j => j.gameObject));
        if (!TestProbe.Check("[MIC-89] hay trabajadores de la nave que viven en ella (el turno de noche)", crew.Count > 0, fab == null ? "sin la nave" : $"{crew.Count}"))
        { if (loadedHere) SceneManager.UnloadSceneAsync(s); TestProbe.End(); yield break; }

        float oldScale = Time.timeScale; Time.timeScale = 4f;
        int jf0 = MicroJornada.Fallbacks, hf0 = HomeNeedSpell.Fallbacks;
        PersonLife.OnEvent += OnEvent;
        // 1 · Trabajan.
        tc.SetClock(tc.Day + 1, WorkRoster.StartHour + 1.2f);
        foreach (MicroJornada j in crew) j.ChooseNow("trabajo");
        MicroJornada worker = null;
        float stop = Time.realtimeSinceStartup + 90f;
        while (worker == null && Time.realtimeSinceStartup < stop)
        {
            foreach (MicroJornada j in crew)
                if (j.Station != null && Flat(j.transform.position - MicroJornada.WorkSpotOf(j.Station, j.transform.position)) < 1.0f) { worker = j; break; }
            yield return null;
        }
        TestProbe.Check("[MIC-89] trabajan: a las 9 llegan andando a su puesto de la nave", worker != null,
            worker != null ? $"«{worker.name}» en «{worker.Station.station}» ({worker.Doing})" : string.Join("; ", crew.Select(j => $"{j.name}: {(j.Station != null ? j.Station.station : "sin puesto")} a {(j.Station != null ? Flat(j.transform.position - MicroJornada.WorkSpotOf(j.Station, j.transform.position)) : -1f):0.0} m")));

        // 2 · Comen, en la mesa alta con apoyos de abdomen.
        tc.SetHour(WorkRoster.StartHour + WorkRoster.FirstBlock + 0.05f);
        foreach (MicroJornada j in crew) { Body(j, hunger: 0.7f); j.ChooseNow("comida"); }
        yield return WaitFor("comer", 150f);
        var eat = _events.FirstOrDefault(e => e.key == "comer");
        Transform table = eat.who != null ? NearestTable(fab, eat.at) : null;
        bool rests = table != null && table.GetComponentsInChildren<Transform>(true).Any(x => x.name == "Apoyo de abdomen");
        TestProbe.Check("[MIC-90] comen en la mesa alta con apoyos de abdomen", eat.who != null && rests,
            eat.who == null ? "nadie comió" : $"«{eat.who.name}» junto a «{(table != null ? table.name : "ninguna mesa a 3 m")}»");

        // 3 · Van al retrete de la nave.
        tc.SetHour(WorkRoster.StartHour + WorkRoster.FirstBlock + WorkRoster.LunchHours + 0.3f);
        foreach (MicroJornada j in crew) Body(j, toilet: 0.95f);
        yield return WaitFor("bano", 120f);
        var wc = _events.FirstOrDefault(e => e.key == "bano");
        Household home = wc.who != null && wc.who.TryGetComponent(out PersonLife wpl) ? wpl.home : null;
        TestProbe.Check("[MIC-91] van al retrete de la nave", wc.who != null && home != null && home.toilet != null && home.toilet.IsChildOf(fab.transform) && Flat(wc.at - home.toilet.position) < 2.5f,
            wc.who == null ? "nadie fue al baño" : $"«{wc.who.name}» a {(home != null && home.toilet != null ? Flat(wc.at - home.toilet.position) : -1f):0.0} m del retrete");

        // 4 · Duermen en su catre de la nave.
        tc.SetHour(22.5f);
        foreach (MicroJornada j in crew) if (j.TryGetComponent(out Anima a)) a.sleepiness = 0.9f;
        yield return WaitFor("dormir", 150f);
        var sl = _events.FirstOrDefault(e => e.key == "dormir");
        Household sh = sl.who != null && sl.who.TryGetComponent(out PersonLife spl) ? spl.home : null;
        Transform bed = sh != null ? sh.BedOf(sl.who) : null;
        TestProbe.Check("[MIC-92] duermen en su catre de la nave", sl.who != null && bed != null && bed.IsChildOf(fab.transform) && Flat(sl.at - bed.position) < 2.5f,
            sl.who == null ? "nadie se durmió" : $"«{sl.who.name}» a {(bed != null ? Flat(sl.at - bed.position) : -1f):0.0} m de su catre");
        foreach (MicroJornada j in crew) if (j.TryGetComponent(out Anima a)) { a.sleepiness = 0.02f; }

        // 5 · Sin teletransportes.
        int jf = MicroJornada.Fallbacks - jf0, hf = HomeNeedSpell.Fallbacks - hf0;
        TestProbe.Check("[MIC-93] sin teletransportes: todo andando (jornada y hogar)", jf == 0 && hf == 0, $"jornada {jf}, hogar {hf}");

        PersonLife.OnEvent -= OnEvent;
        Time.timeScale = oldScale;
        tc.SetHour(WorkRoster.StartHour + 1f);
        if (loadedHere) SceneManager.UnloadSceneAsync(s);
        TestProbe.End();
    }

    IEnumerator WaitFor(string key, float realSeconds)
    {
        float stop = Time.realtimeSinceStartup + realSeconds;
        while (!_events.Any(e => e.key == key) && Time.realtimeSinceStartup < stop) yield return null;
    }

    static void Body(MicroJornada j, float hunger = -1f, float toilet = -1f)
    {
        if (!j.TryGetComponent(out BodyMetabolism b)) return;
        if (hunger >= 0f) b.hunger = hunger;
        if (toilet >= 0f) b.toilet = toilet;
    }

    // La mesa (MicroPiece «mesa») de la nave más cercana a donde comió, a menos de 3 m.
    static Transform NearestTable(AreaFunctionSite fab, Vector3 at)
    {
        Transform best = null; float bd = 3f;
        foreach (MicroPiece p in fab.GetComponentsInChildren<MicroPiece>(true))
        {
            if (p.kind != "mesa") continue;
            float d = Flat(p.transform.position - at);
            if (d < bd) { bd = d; best = p.transform; }
        }
        return best;
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }
}
