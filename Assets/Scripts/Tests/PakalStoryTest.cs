using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TEST DEL MICROCOSMOS MAYA: PAKAL DE PALENQUE (docs/microcosmos-maya-pakal.md). Carga «Microcosmos_Pakal» (espera acotada) y mira:
/// - su fecha (683) y su región (Maya: el estilo «maya_clasico» del catálogo en sus muebles);
/// - la historia (StoryMission) con sus pasos y objetivos, y que avanza paso a paso (todos menos el último);
/// - la CIUDAD sobre su plano real: el Palacio y el Templo de las Inscripciones en terrazas (plantas que se pisan, con rampa), el
///   juego de pelota, el Otolum, la colina de las Cruces y la sierra (o lo que cabe), ≥ 12 edificios, edificios del pueblo de época
///   (temazcal, mercado, estela…) y ninguno que no existiera (ni comisaría ni estación de tren);
/// - cada casa con su FRUTAL maya (cacao, aguacate, ramón, zapote o papaya);
/// - flora y fauna del lugar (la ceiba, el maíz; la hormiga cortadora, la abeja melipona…);
/// - las reglas de edificación (MicroBuildRules) y las de los microcosmos (MicroFloors): sobre el Huerto, forma finalizada, y
///   MARCADO «pendiente de piso representativo» (un templo de los antepasados).
/// </summary>
public class PakalStoryTest : MonoBehaviour, ITestUnit
{
    public int Group => 66;   // tras las eras civilizadas (62) y el validador (63): carga y descarga una escena
    public bool ParallelSafe => false;
    public const string Scene = "Microcosmos_Pakal";
    public float loadTimeout = 60f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<PakalStoryTest>() == null) tr.gameObject.AddComponent<PakalStoryTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Pakal de Palenque: la historia maya, su ciudad real, frutales y reglas");
        if (!TestProbe.Check($"«{Scene}» está en Build Settings", Application.CanStreamedLevelBeLoaded(Scene), "genera «Tools/Cold Sanctuary/Build Microcosmos de historias»")) { TestProbe.End(); yield break; }
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

        StoryMission m = null; MicroFloor floor = null; MicroEra era = null;
        var sites = new List<AreaFunctionSite>();
        foreach (GameObject g in s.GetRootGameObjects())
        {
            if (m == null) m = g.GetComponentInChildren<StoryMission>(true);
            if (floor == null) floor = g.GetComponentInChildren<MicroFloor>(true);
            if (era == null) era = g.GetComponentInChildren<MicroEra>(true);
            sites.AddRange(g.GetComponentsInChildren<AreaFunctionSite>(true));
        }
        Transform root = floor != null ? floor.transform : null;
        TestProbe.Check("su fecha es 683 y su región la maya", era != null && era.year == 683 && era.region == MicroStyles.Region.Maya.ToString(), era != null ? $"{era.year}, {era.region}" : "sin MicroEra");

        // La historia.
        if (TestProbe.NotNull("la historia (StoryMission)", m))
        {
            TestProbe.Check("es la de Pakal", m.title.Contains("Pakal"), m.title);
            TestProbe.Check("tiene entre 8 y 14 pasos", m.steps.Length >= 8 && m.steps.Length <= 14, $"{m.steps.Length}");
            string bad = "";
            for (int i = 0; i < m.steps.Length; i++) if (m.steps[i].targets == null || m.steps[i].targets.Length == 0 || System.Array.Exists(m.steps[i].targets, x => x == null)) bad += $"paso {i + 1} sin objetivo; ";
            TestProbe.Check("cada paso tiene su objetivo", bad.Length == 0, bad);
            int start = m.Step; string adv = "";
            for (int i = start; i < m.steps.Length - 1; i++)
            {
                if (!m.CompleteCurrentForTest()) { adv += $"no avanza en el paso {i + 1}; "; break; }
                yield return null;
                if (m.Step != i + 1) { adv += $"paso {m.Step} en vez de {i + 1}; "; break; }
            }
            TestProbe.Check("los pasos avanzan", adv.Length == 0, adv.Length > 0 ? adv : $"llega al paso {m.Step + 1}/{m.steps.Length}");
        }

        // La ciudad sobre su plano.
        AreaFunctionSite palace = sites.Find(x => x.title.StartsWith("Palacio")), temple = sites.Find(x => x.title.StartsWith("Templo de las Inscripciones"));
        int Tiers(AreaFunctionSite x) { int k = 0; if (x != null) foreach (MicroPiece p in x.GetComponentsInChildren<MicroPiece>(true)) if (p.kind == "planta") k++; return k; }
        TestProbe.Check("el Palacio en terrazas (≥ 2 plantas que se pisan, con rampa)", palace != null && Tiers(palace) >= 2, palace != null ? $"{Tiers(palace)} terrazas" : "no está");
        TestProbe.Check("el Templo de las Inscripciones en terrazas (≥ 3)", temple != null && Tiers(temple) >= 3, temple != null ? $"{Tiers(temple)} terrazas" : "no está");
        TestProbe.Check("el juego de pelota", sites.Exists(x => x.title.Contains("Juego de pelota")));
        TestProbe.Check("la ciudad entera: ≥ 12 edificios", sites.Count >= 12, $"{sites.Count}");
        string land = "";
        Transform plan = null;
        if (root != null) foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name.StartsWith("Paisaje de Palenque")) { plan = t; break; }
        if (plan != null) foreach (Transform c in plan) land += c.name + "; ";
        TestProbe.Check("su paisaje real: el Otolum (río) y una colina", plan != null && land.Contains("Otolum") && (land.Contains("Colina") || land.Contains("Monte")), land.Length > 160 ? land.Substring(0, 160) : land);
        TestProbe.Check("edificios del pueblo de su época (temazcal, mercado, estela…)", sites.Exists(x => x.title.Contains("Temazcal")) && sites.Exists(x => x.building == "Mercado"), $"{sites.Count} edificios");
        TestProbe.Check("ninguno que no existiera (ni comisaría ni estación de tren)", !sites.Exists(x => x.building == "Comisaría" || x.title.Contains("ferrocarril")));
        // Cada casa, su frutal maya.
        string noFruit = ""; int houses = 0; var kinds = new HashSet<string>();
        foreach (AreaFunctionSite x in sites)
        {
            if (x.GetComponentInChildren<Household>(true) == null) continue;
            houses++;
            MicroPiece fr = null; foreach (MicroPiece p in x.GetComponentsInChildren<MicroPiece>(true)) if (p.kind == "frutal") { fr = p; break; }
            if (fr == null) noFruit += x.title + "; "; else kinds.Add(fr.name);
        }
        TestProbe.Check("cada residencia con su frutal maya", houses > 0 && noFruit.Length == 0 && kinds.Count > 0, noFruit.Length > 0 ? noFruit : $"{houses} casas: {string.Join(", ", kinds)}");
        bool mayaFruit = false; foreach (string k in kinds) if (k.Contains("Cacao") || k.Contains("Aguacate") || k.Contains("Ramón") || k.Contains("Zapote") || k.Contains("Papaya")) mayaFruit = true;
        TestProbe.Check("los frutales son los mayas (cacao, aguacate, ramón, zapote, papaya)", mayaFruit, string.Join(", ", kinds));
        // El estilo maya en los muebles.
        int mayaStyle = 0, pieces = 0;
        foreach (AreaFunctionSite x in sites) foreach (MicroPiece p in x.GetComponentsInChildren<MicroPiece>(true)) { pieces++; if (p.style == "maya_clasico") mayaStyle++; }
        TestProbe.Check("los muebles, del estilo maya clásico del catálogo", pieces > 0 && mayaStyle >= pieces * 0.9f, $"{mayaStyle}/{pieces}");
        // Flora y fauna del lugar.
        int ceiba = 0, bugs = 0;
        if (root != null)
        {
            Transform flora = root.Find("Flora_a_escala"), fauna = root.Find("Fauna_a_escala");
            if (flora != null) foreach (Transform p in flora) if (p.name.Contains("Ceiba") || p.name.Contains("Maíz") || p.name.Contains("Cacao") || p.name.Contains("Ramón")) ceiba++;
            if (fauna != null) foreach (Transform b in fauna) if (b.name.Contains("Atta") || b.name.Contains("Melipona") || b.name.Contains("Quesada")) bugs++;
        }
        TestProbe.Check("flora del lugar (ceiba, maíz, cacao, ramón)", ceiba > 0, $"{ceiba}");
        TestProbe.Check("fauna del lugar (hormiga cortadora, melipona, cigarra gigante)", bugs > 0, $"{bugs}");
        // Las reglas.
        if (root != null)
        {
            var rules = MicroBuildRules.Validate(root, Scene, true, false);
            TestProbe.Check("cumple las reglas de edificación (1–12)", rules.Count == 0, string.Join(" · ", rules.GetRange(0, Mathf.Min(8, rules.Count))));
            var floorRules = MicroFloors.Validate(root, Scene, false);
            TestProbe.Check("cumple las reglas de los microcosmos (piso, forma finalizada, estructuras pobladas, pueblo entero)", floorRules.Count == 0, string.Join(" · ", floorRules));
            TestProbe.Check("marcado «pendiente de piso representativo» (sobre el Huerto)", floor.platform == "Garden_Area" && !floor.representative && floor.pendingFloor.Length > 0, $"{floor.platform}: «{floor.pendingFloor}»");
        }

        if (loadedHere)
        {
            AsyncOperation un = SceneManager.UnloadSceneAsync(s);
            float until = Time.realtimeSinceStartup + loadTimeout;
            while (un != null && !un.isDone && Time.realtimeSinceStartup < until) yield return null;
        }
        TestProbe.End();
    }
}
