using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TEST DE LOS MICROCOSMOS DE LOS FAMOSOS (docs/microcosmos-famosos.md; grupo 69). Carga UNA A UNA (aditiva, espera acotada, y la
/// descarga antes de la siguiente: la memoria) las siete escenas —Sadhguru, Elon Musk, Goku, Bill Gates, Warren Buffett, MacKenzie
/// Scott e Ibrahim Traoré— y mira en cada una:
/// - su fecha y su región (MicroEra) y su piso del santuario (MicroFloor: el de la tabla, representativo o «pendiente»);
/// - el protagonista (un insecto-humano con su nombre) y la historia (StoryMission: 7–10 pasos con objetivo, que avanzan) con su ficha
///   con fuentes (HistoryCard);
/// - el pueblo entero: ≥ 12 edificios, cada residencia con su frutal de época y lugar (el de su entrada del catálogo);
/// - Goku: la ficha dice que es un homenaje de ficción; Traoré: ningún edificio militar (cuartel, milicia, armería);
/// - las reglas de edificación (MicroBuildRules) y las de los microcosmos (MicroFloors).
/// Sin la escena en Build Settings, la salta con aviso (hay que generarla con su menú).
/// </summary>
public class FamososStoryTest : MonoBehaviour, ITestUnit
{
    public int Group => 69;   // tras Pakal (66) y micro3 (67/68): carga y descarga escenas, de una en una
    public bool ParallelSafe => false;
    public float loadTimeout = 180f;   // los pueblos ×60 tardan en cargar en aditivo (PakalStoryTest)

    // (escena, protagonista, año, región, piso, ¿representativo?, frutales de su entrada)
    static readonly (string scene, string hero, int year, string region, string platform, bool rep, string[] fruits)[] Cases =
    {
        ("Microcosmos_Sadhguru", "Sadhguru", 1999, "SurAsia", "YogaRoom_Area", true, new[] { "Mango", "Cocotero", "Banano", "Yaca" }),
        ("Microcosmos_ElonMusk", "Elon Musk", 2008, "Norteamerica", "VehicleWorkshop_Area", false, new[] { "Limonero", "Manzano" }),
        ("Microcosmos_Goku", "Goku", 1930, "China", "MartialArts_Area", true, new[] { "Lichi", "Níspero", "Longán" }),
        ("Microcosmos_BillGates", "Bill Gates", 1975, "NorteamericaInterior", "VehicleWorkshop_Area", false, new[] { "Manzano", "Melocotonero", "Albaricoquero", "Pacano" }),
        ("Microcosmos_WarrenBuffett", "Warren Buffett", 1983, "NorteamericaInterior", "TextileStudio_Area", false, new[] { "Manzano", "Melocotonero", "Albaricoquero", "Pacano" }),
        ("Microcosmos_MacKenzieScott", "MacKenzie Scott", 2020, "NorteamericaInterior", "Chemistry_Area", false, new[] { "Manzano", "Melocotonero", "Albaricoquero", "Pacano" }),
        ("Microcosmos_IbrahimTraore", "Ibrahim Traoré", 2024, "Sahel", "Garden_Area", true, new[] { "Mango", "Papayo", "Guayabo", "Karité" }),
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<FamososStoryTest>() == null) tr.gameObject.AddComponent<FamososStoryTest>();
    }

    public IEnumerator Run()
    {
        foreach (var c in Cases) yield return One(c.scene, c.hero, c.year, c.region, c.platform, c.rep, c.fruits);
    }

    IEnumerator One(string scene, string hero, int year, string region, string platform, bool rep, string[] fruits)
    {
        TestProbe.Begin($"Famosos: {hero} ({scene})");
        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            TestProbe.Check($"«{scene}» está en Build Settings", false, "genera «Tools/Cold Sanctuary/Build Microcosmos de famosos/…» (de uno en uno)");
            TestProbe.End(); yield break;
        }
        Scene s = SceneManager.GetSceneByName(scene);
        bool loadedHere = false;
        if (!s.isLoaded)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            float until = Time.realtimeSinceStartup + loadTimeout;
            while (op != null && !op.isDone && Time.realtimeSinceStartup < until) yield return null;
            s = SceneManager.GetSceneByName(scene);
            if (!TestProbe.Check($"«{scene}» se carga", s.isLoaded, $"no cargó en {loadTimeout:0} s")) { TestProbe.End(); yield break; }
            loadedHere = true;
        }
        yield return null; yield return null;

        StoryMission m = null; MicroFloor floor = null; MicroEra era = null; HistoryCard card = null; Transform heroT = null;
        var sites = new List<AreaFunctionSite>();
        foreach (GameObject g in s.GetRootGameObjects())
        {
            if (m == null) m = g.GetComponentInChildren<StoryMission>(true);
            if (floor == null) floor = g.GetComponentInChildren<MicroFloor>(true);
            if (era == null) era = g.GetComponentInChildren<MicroEra>(true);
            if (card == null) card = g.GetComponentInChildren<HistoryCard>(true);
            sites.AddRange(g.GetComponentsInChildren<AreaFunctionSite>(true));
            if (heroT == null) foreach (CubeInsect ci in g.GetComponentsInChildren<CubeInsect>(true)) if (ci.name.StartsWith(hero)) { heroT = ci.transform; break; }
        }
        Transform root = floor != null ? floor.transform : null;

        TestProbe.Check($"su fecha es {year} y su región {region}", era != null && era.year == year && era.region == region, era != null ? $"{era.year}, {era.region}" : "sin MicroEra");
        TestProbe.Check($"{hero} está, como insecto-humano", heroT != null);
        if (TestProbe.NotNull("la historia (StoryMission)", m))
        {
            TestProbe.Check("tiene entre 7 y 10 pasos", m.steps.Length >= 7 && m.steps.Length <= 10, $"{m.steps.Length}");
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
        TestProbe.Check("la ficha con su historia y sus fuentes", card != null && card.history.Length >= 3 && card.sources.Length >= 3, card != null ? $"{card.history.Length} párrafos, {card.sources.Length} fuentes" : "sin HistoryCard");
        if (hero == "Goku") TestProbe.Check("la ficha dice que es un homenaje de ficción", card != null && string.Join(" ", card.history).Contains("homenaje"));
        if (hero.StartsWith("Ibrahim"))
            TestProbe.Check("ningún edificio militar (es un acto civil)", !sites.Exists(x => x.building == "Cuartel" || x.title.Contains("Cuartel") || x.title.Contains("milicia") || x.title.Contains("Armería")),
                string.Join(", ", sites.FindAll(x => x.building == "Cuartel").ConvertAll(x => x.title)));

        // El pueblo entero y sus frutales.
        TestProbe.Check("el pueblo entero: ≥ 12 edificios", sites.Count >= 12, $"{sites.Count}");
        string noFruit = ""; int houses = 0; var kinds = new HashSet<string>();
        foreach (AreaFunctionSite x in sites)
        {
            if (x.GetComponentInChildren<Household>(true) == null) continue;
            houses++;
            MicroPiece fr = null; foreach (MicroPiece p in x.GetComponentsInChildren<MicroPiece>(true)) if (p.kind == "frutal") { fr = p; break; }
            if (fr == null) noFruit += x.title + "; "; else kinds.Add(fr.name);
        }
        TestProbe.Check("cada residencia con su frutal", houses > 0 && noFruit.Length == 0, noFruit.Length > 0 ? noFruit : $"{houses} casas: {string.Join(", ", kinds)}");
        bool local = false; foreach (string k in kinds) foreach (string f in fruits) if (k.Contains(f)) local = true;
        TestProbe.Check($"los frutales son los de su época y lugar ({string.Join(", ", fruits)})", local, string.Join(", ", kinds));

        // Las reglas.
        if (root != null)
        {
            var rules = MicroBuildRules.Validate(root, scene, true, false);
            TestProbe.Check("cumple las reglas de edificación", rules.Count == 0, string.Join(" · ", rules.GetRange(0, Mathf.Min(8, rules.Count))));
            var floorRules = MicroFloors.Validate(root, scene, false);
            TestProbe.Check("cumple las reglas de los microcosmos (piso, forma finalizada, estructuras pobladas, pueblo entero)", floorRules.Count == 0, string.Join(" · ", floorRules));
            TestProbe.Check(rep ? $"sobre su piso representativo ({platform})" : $"marcado «pendiente de piso representativo» (sobre {platform})",
                floor.platform == platform && floor.representative == rep && (rep || floor.pendingFloor.Length > 0), $"{floor.platform}: «{floor.pendingFloor}»");
        }
        else TestProbe.Check("tiene su MicroFloor (montado con la plantilla)", false);

        if (loadedHere)
        {
            AsyncOperation un = SceneManager.UnloadSceneAsync(s);
            float until = Time.realtimeSinceStartup + loadTimeout;
            while (un != null && !un.isDone && Time.realtimeSinceStartup < until) yield return null;
        }
        TestProbe.End();
    }
}
