using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TEST DE LAS ESCENAS DE LA TANDA 3 DE HISTORIAS (docs/microcosmos-historias-3.md; grupo 79). Como FamososStoryTest: carga UNA A UNA (aditiva, espera
/// acotada) y descarga antes de la siguiente, por la memoria. En cada escena mira:
/// - fecha, región (MicroEra) y piso (MicroFloor: representativo o «pendiente»);
/// - el protagonista (insecto-humano) y la historia (8 pasos con objetivo, que avanzan), con su ficha con fuentes;
/// - el pueblo entero (≥ 12 edificios), cada residencia con su frutal de su lugar, y las cuatro categorías con sus lugares de entrenar de época;
/// - sus juicios: ningún edificio militar en ninguna; la ficha de Galvarino no describe la herida; la Feria y Cajamarca dicen lo que son;
/// - las reglas de edificación y las de los microcosmos. Sin la escena en Build Settings, la salta con aviso.
/// </summary>
public class HistoriasStoryTest : MonoBehaviour, ITestUnit
{
    public int Group => 79;   // carga y descarga escenas, de una en una
    public bool ParallelSafe => false;
    public float loadTimeout = 180f;

    static readonly (string scene, string hero, string display, string[] fruits)[] Cases =
    {
        ("Microcosmos_EloyAlfaro", "Eloy Alfaro", "Chimbacalle", new[] { "Capulí", "Tomate de árbol", "Chirimoya" }),
        ("Microcosmos_Galvarino", "Galvarino", "Lagunillas", new[] { "Maqui", "Avellano", "Murta" }),
        ("Microcosmos_Atahualpa", "Atahualpa", "Cajamarca", new[] { "Lúcuma", "Chirimoya", "Pacay" }),
        ("Microcosmos_GranLeyDeLaPaz", "El Pacificador", "Onondaga", new[] { "Ciruelo americano", "Nogal", "Arándano" }),
        ("Microcosmos_Cleopatra", "Cleopatra", "Alejandría de Cleopatra", new[] { "Palmera", "Higuera", "Granado", "Sicómoro" }),
        ("Microcosmos_Langley", "Katherine Johnson", "Langley", new[] { "Melocotonero", "Caqui", "Higuera" }),
        ("Microcosmos_Beethoven", "Ludwig van Beethoven", "Viena 1824", new[] { "Albaricoquero", "Cerezo", "Peral" }),
        ("Microcosmos_Descartes", "René Descartes", "Leiden", new[] { "Manzano", "Peral", "Ciruelo" }),
        ("Microcosmos_CheGuevara", "Ernesto Guevara", "San Pablo", new[] { "Aguaje", "Plátano", "Papayo", "Camu" }),
        ("Microcosmos_PedroSanchez", "Pedro Sánchez", "Madrid 2018", new[] { "Almendro", "Olivo", "Granado" }),
        ("Microcosmos_Lincoln", "Abraham Lincoln", "Gettysburg", new[] { "Manzano", "Melocotonero" }),
        ("Microcosmos_Hazanas", "Lee Kuan Yew", "Feria de las Hazañas", new[] { "Naranjo", "Mango", "Olivo" }),
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<HistoriasStoryTest>() == null) tr.gameObject.AddComponent<HistoriasStoryTest>();
    }

    public IEnumerator Run()
    {
        foreach (var c in Cases) yield return One(c.scene, c.hero, c.display, c.fruits);
    }

    IEnumerator One(string scene, string hero, string display, string[] fruits)
    {
        TestProbe.Begin($"Historias 3: {hero} ({scene})");
        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            TestProbe.Check($"«{scene}» está en Build Settings", false, "genera «Tools/Cold Sanctuary/Build Microcosmos de historias 3/…» (de uno en uno)");
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

        var row = System.Array.Find(HistoriasDatosTest.Cases, x => x.display == display);
        TestProbe.Check($"su fecha es {row.year} y su región {row.region}", era != null && era.year == row.year && era.region == row.region, era != null ? $"{era.year}, {era.region}" : "sin MicroEra");
        TestProbe.Check($"{hero} está, como insecto-humano", heroT != null);
        if (TestProbe.NotNull("la historia (StoryMission)", m))
        {
            TestProbe.Check("tiene 8 pasos", m.steps.Length == 8, $"{m.steps.Length}");
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
        string text = card != null ? string.Join(" ", card.history) : "";
        if (display == "Lagunillas") TestProbe.Check("la ficha dice que no muestra heridas ni combates", text.Contains("no muestra heridas"));
        if (display == "Feria de las Hazañas") TestProbe.Check("la ficha dice que la feria es inventada y que Lee fue primer ministro", text.Contains("inventada") && text.Contains("PRIMER MINISTRO"));
        if (display == "Madrid 2018") TestProbe.Check("la ficha cita su fecha y no valora la gestión", text.Contains("noviembre de 2026") && text.Contains("no opina"));
        if (display == "Gettysburg") TestProbe.Check("la ficha da las encuestas y las sombras", text.Contains("C-SPAN") && text.Contains("Siena") && text.Contains("dakota"));
        TestProbe.Check("ningún edificio militar (historias civiles)", !sites.Exists(x => x.building == "Cuartel" || x.title.Contains("Cuartel") || x.title.Contains("milicia") || x.title.Contains("Armería")),
            string.Join(", ", sites.FindAll(x => x.building == "Cuartel").ConvertAll(x => x.title)));

        // El pueblo entero, sus frutales y sus categorías.
        TestProbe.Check("el pueblo entero: ≥ 12 edificios", sites.Count >= 12, $"{sites.Count}");
        string noFruit = ""; int houses = 0; var kinds = new HashSet<string>(); var cats = new HashSet<string>();
        foreach (AreaFunctionSite x in sites)
        {
            if (!string.IsNullOrEmpty(x.activity)) cats.Add(x.activity);
            if (x.GetComponentInChildren<Household>(true) == null) continue;
            houses++;
            MicroPiece fr = null; foreach (MicroPiece p in x.GetComponentsInChildren<MicroPiece>(true)) if (p.kind == "frutal") { fr = p; break; }
            if (fr == null) noFruit += x.title + "; "; else kinds.Add(fr.name);
        }
        TestProbe.Check("cada residencia con su frutal", houses > 0 && noFruit.Length == 0, noFruit.Length > 0 ? noFruit : $"{houses} casas: {string.Join(", ", kinds)}");
        bool local = false; foreach (string k in kinds) foreach (string f in fruits) if (k.Contains(f)) local = true;
        TestProbe.Check($"los frutales son los de su lugar ({string.Join(", ", fruits)})", local, string.Join(", ", kinds));
        foreach (string cat in AreaFunctionSite.TownCategories) TestProbe.Check($"tiene {cat}", cats.Contains(cat));
        AreaFunctionSite trainSite = sites.Find(x => x.activity == AreaFunctionSite.Entrenamiento);
        TestProbe.Check($"entrena en {row.training}", trainSite != null && trainSite.training == row.training, trainSite != null ? trainSite.training : "sin sitio");

        // Las reglas.
        Transform root = floor != null ? floor.transform : null;
        if (root != null)
        {
            var rules = MicroBuildRules.Validate(root, scene, true, false);
            TestProbe.Check("cumple las reglas de edificación", rules.Count == 0, string.Join(" · ", rules.GetRange(0, Mathf.Min(8, rules.Count))));
            var floorRules = MicroFloors.Validate(root, scene, false);
            TestProbe.Check("cumple las reglas de los microcosmos", floorRules.Count == 0, string.Join(" · ", floorRules));
            TestProbe.Check(row.rep ? $"sobre su piso representativo ({row.platform})" : $"marcado «pendiente de piso representativo» (sobre {row.platform})",
                floor.platform == row.platform && floor.representative == row.rep && (row.rep || floor.pendingFloor.Length > 0), $"{floor.platform}: «{floor.pendingFloor}»");
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
