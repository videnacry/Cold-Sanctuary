using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TEST DEL BLOQUEO DE HUMANOS (docs/microcosmos-civilizaciones.md §3, §5–7). Cada espera, acotada:
/// (1) un humano de cubos que pasa por <see cref="InsectOnly.Admit"/> queda con cuerpo de insecto (nada humano visible), a su
///     estatura (±25 cm), con cara humana y pelo; <see cref="InsectOnly.Release"/> le devuelve su cuerpo;
/// (2) en una escena de microcosmos (con <see cref="MicroEra"/>), un humano que aparece después se convierte solo en ≤ 4 s;
/// (3) el insecto de cada personaje (Bruce Lee → mantis, Nightingale → luciérnaga, Kushal → hormiga…);
/// (4) la genealogía entre especies: el hijo sale casi siempre como uno de los padres, pero un antepasado lejano puede volver
///     (la mariquita y la araña con un ciempiés tatarabuelo).
/// </summary>
public class InsectOnlyTest : MonoBehaviour, ITestUnit
{
    public int Group => 61;
    public bool ParallelSafe => false;   // crea y descarga una escena

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<InsectOnlyTest>() == null) tr.gameObject.AddComponent<InsectOnlyTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Bloqueo de humanos en el microcosmos: conversión, escena, insecto de cada uno y genealogía");
        yield return Convert();
        yield return SceneGuard();
        Cast();
        Genealogy();
        TestProbe.End();
    }

    static GameObject Human(string name, Vector3 at)
    {
        var g = new GameObject(name);
        g.transform.position = at;
        CubeHumanoid.Dress(g, Color.gray, Color.white, 1.75f);
        return g;
    }

    static bool HumanVisible(GameObject g)
    {
        Transform body = g.transform.Find(CubeHumanoid.BodyName);
        if (body == null) return false;
        foreach (Renderer r in body.GetComponentsInChildren<Renderer>()) if (r.enabled) return true;
        return false;
    }

    static float InsectHeight(GameObject g)
    {
        Transform root = g.transform.Find("Insecto");
        bool any = false; Bounds b = default;
        if (root != null) foreach (Renderer r in root.GetComponentsInChildren<Renderer>()) { if (!r.enabled) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return any ? b.size.y : 0f;
    }

    IEnumerator Convert()
    {
        GameObject g = Human("Florence Nightingale (prueba)", new Vector3(9400f, 500f, 9400f));
        yield return null;
        TestProbe.Check("de partida es humano", InsectOnly.IsHuman(g));
        InsectOnly.Result r = InsectOnly.Admit(g);
        TestProbe.Check("pasa el bloqueo convertido", r == InsectOnly.Result.Converted, r.ToString());
        TestProbe.Check("ya no se ve nada humano", !InsectOnly.IsHuman(g) && !HumanVisible(g));
        CubeInsect ci = g.GetComponent<CubeInsect>();
        TestProbe.Check("es una luciérnaga erguida (insecto-humano de era civilizada)", ci != null && ci.species == "Firefly" && ci.folk);
        TestProbe.Near("a su estatura (1,75 m ± 25 cm)", InsectHeight(g), 1.75f, 0.25f);
        Transform root = g.transform.Find("Insecto");
        bool face = false, hair = false;
        if (root != null) foreach (Transform t in root.GetComponentsInChildren<Transform>()) { if (t.name == "Nariz") face = true; if (t.name == "Marco_cabello" && t.childCount > 0) hair = true; }
        TestProbe.Check("cara humana (nariz) y cabello", face && hair);
        TestProbe.Check("volver a pasar no hace nada (ya es insecto)", InsectOnly.Admit(g) == InsectOnly.Result.AlreadyInsect);
        InsectOnly.Release(g);
        yield return null;
        TestProbe.Check("al salir recupera su cuerpo humano", HumanVisible(g) && g.transform.Find("Insecto") == null);
        Destroy(g);
    }

    IEnumerator SceneGuard()
    {
        Scene prev = SceneManager.GetActiveScene();
        Scene s = SceneManager.CreateScene("Microcosmos_PruebaBloqueo");
        var root = new GameObject("Microcosmos_PruebaBloqueo");
        SceneManager.MoveGameObjectToScene(root, s);
        root.AddComponent<MicroEra>().year = 1964;
        TestProbe.Check("la escena cuenta como mundo de insectos", InsectOnly.IsInsectWorld(s));
        yield return null;
        GameObject g = Human("Bruce Lee", new Vector3(9420f, 500f, 9400f));
        SceneManager.MoveGameObjectToScene(g, s);
        float until = Time.realtimeSinceStartup + 4f;
        while (InsectOnly.IsHuman(g) && Time.realtimeSinceStartup < until) yield return null;
        CubeInsect ci = g.GetComponent<CubeInsect>();
        TestProbe.Check("un humano que aparece en el microcosmos se convierte solo (≤ 4 s) en su insecto: Bruce Lee, mantis", !InsectOnly.IsHuman(g) && ci != null && ci.species == "Mantis", ci != null ? ci.species : "sigue humano");
        SceneManager.SetActiveScene(prev);
        AsyncOperation op = SceneManager.UnloadSceneAsync(s);
        float u2 = Time.realtimeSinceStartup + 10f;
        while (op != null && !op.isDone && Time.realtimeSinceStartup < u2) yield return null;
    }

    static void Cast()
    {
        var want = new (string who, string sp)[] { ("Bruce Lee", "Mantis"), ("Yip Man (Shanidar)", "Mantis"), ("Florence Nightingale", "Firefly"), ("Siddhārtha", "Cicada"),
            ("Eratóstenes", "DungBeetle"), ("Wilbur Wright", "Grasshopper"), ("Kushal", "Ant"), ("player", "Ant"), ("Patanjali", "Snail") };
        string bad = "";
        foreach (var (who, sp) in want) if (InsectCast.SpeciesFor(who) != sp) bad += $"{who} → {InsectCast.SpeciesFor(who)} (debía {sp}); ";
        TestProbe.Check("el insecto de cada personaje", bad.Length == 0, bad);
        string none = "";
        foreach (string sp in InsectCast.Folk) if (InsectMorphology.Named(sp) == null) none += sp + "; ";
        TestProbe.Check("las especies de las eras civilizadas tienen ficha morfológica", none.Length == 0, none);
    }

    static void Genealogy()
    {
        // Una mariquita y una araña; un ciempiés entre los tatarabuelos (por la madre).
        var momAnc = new List<InsectGenealogy.Ancestor> { new InsectGenealogy.Ancestor("abuela", "Ladybug", 1), new InsectGenealogy.Ancestor("abuelo", "Ladybug", 1),
                                                         new InsectGenealogy.Ancestor("bisabuelo", "Ladybug", 2), new InsectGenealogy.Ancestor("ciempiés", "Centipede", 3) };
        var dadAnc = new List<InsectGenealogy.Ancestor> { new InsectGenealogy.Ancestor("abuela", "Spider", 1), new InsectGenealogy.Ancestor("abuelo", "Spider", 1) };
        var kid = InsectGenealogy.ChildAncestry("Mariquita", "Ladybug", momAnc, "Araña", "Spider", dadAnc);
        var odds = InsectGenealogy.Odds(kid);
        float sum = 0f; foreach (float v in odds.Values) sum += v;
        TestProbe.Near("las probabilidades suman 1", sum, 1f, 0.001f);
        TestProbe.Check("el ciempiés tatarabuelo puede volver (probabilidad > 0, pequeña)", odds.TryGetValue("Centipede", out float c) && c > 0f && c < 0.05f, $"{c:P1}");
        TestProbe.Check("lo más probable es salir como uno de los padres", odds["Ladybug"] > 0.4f && odds["Spider"] > 0.4f, $"mariquita {odds["Ladybug"]:P0}, araña {odds["Spider"]:P0}");
        int centipedes = 0; var rnd = new System.Random(7);
        for (int i = 0; i < 4000; i++) if (InsectGenealogy.ChildSpecies(kid, (float)rnd.NextDouble()) == "Centipede") centipedes++;
        TestProbe.Check("en 4000 hijos sale algún ciempiés", centipedes > 0, $"{centipedes}");
        TestProbe.Check("el tatarabuelo es la 4.ª generación del hijo", kid.Exists(a => a.species == "Centipede" && a.depth == 4));
    }
}
