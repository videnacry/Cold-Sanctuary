using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TEST DE LOS MICROCOSMOS DE ERAS CIVILIZADAS (docs/microcosmos-civilizaciones.md §8–9). Para cada escena de historia que
/// esté en Build Settings (Bruce Lee, Escútari, Dayton, Kapilavastu, Alejandría, Patanjali, Tregua 1914, Planta Atómica, Irena
/// Sendler, Le Chambon, Mesopotamia —MobWorld_Mesopotamia— y el Huerto, la primera aldea), cargada en aditivo
/// con espera acotada:
/// - ningún cuerpo humano visible (bloqueo de humanos) y Kushal no hace falta: lo prueba InsectOnlyTest;
/// - sin la cuadrilla genérica de recolectoras con guarida (era el error);
/// - sus EDIFICIOS (AreaFunctionSite): al menos 2, cada uno con puestos interactuables y funciones del registro para su año;
/// - su gente: insectos-humanos de ≥ 3 especies, de estatura parecida (1,4–2,1 m), con jornada (MicroJornada) que elige
///   función y puesto en cada bloque del horario;
/// - al menos una pareja que tendrá hijos (FolkCouple) y un hijo de prueba sale con una especie de su árbol.
/// Y el horario del día (MicroJornada.BlockAt) es el del santuario.
/// </summary>
public class CivilizedErasTest : MonoBehaviour, ITestUnit
{
    public int Group => 62;   // tras Bruce Lee (60) y el bloqueo (61): carga y descarga escenas
    public bool ParallelSafe => false;
    public float loadTimeout = 180f;   // (2026-11-03) los pueblos ×60 (hasta 150 edificios y 13 000 plantas) tardan más de 60 s en cargar en aditivo

    static readonly string[] Scenes = { "Microcosmos_BruceLee", "Microcosmos_Nightingale", "Microcosmos_Wright", "Microcosmos_Buda", "Microcosmos_Eratostenes", "Microcosmos_Patanjali", "Microcosmos_Tregua1914",
        "Microcosmos_PlantaAtomica", "Microcosmos_IrenaSendler", "Microcosmos_LeChambon", "MobWorld_Mesopotamia", "Microcosmos_Huerto",
        "Microcosmos_Pitagoras", "Microcosmos_Herodoto", "Microcosmos_AlJuarismi", "Microcosmos_Newton", "Microcosmos_Jenner", "Microcosmos_Bolivar", "Microcosmos_Nietzsche", "Microcosmos_Curie", "Microcosmos_Tesla", "Microcosmos_Einstein", "Microcosmos_Patterson", "Microcosmos_Chernobil", "Microcosmos_Pakal" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<CivilizedErasTest>() == null) tr.gameObject.AddComponent<CivilizedErasTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Eras civilizadas: edificios reales e interactivos, jornadas, especies de tamaño parecido y familias");
        TestProbe.Check("el horario: 8–11 trabajo, 11–12 comida, 12–14 trabajo, 14–16 práctica, luego casa",
            MicroJornada.BlockAt(9f) == "trabajo" && MicroJornada.BlockAt(11.5f) == "comida" && MicroJornada.BlockAt(13f) == "trabajo" && MicroJornada.BlockAt(15f) == "practica" && MicroJornada.BlockAt(20f) == "casa");
        foreach (string sc in Scenes) yield return One(sc);
        TestProbe.End();
    }

    IEnumerator One(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName)) { TestProbe.Check($"{sceneName} está en Build Settings", false, "genera «Tools/Cold Sanctuary/Build Microcosmos de historias»"); yield break; }
        Scene s = SceneManager.GetSceneByName(sceneName);
        bool loadedHere = false;
        if (!s.isLoaded)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            float until = Time.realtimeSinceStartup + loadTimeout;
            while (op != null && !op.isDone && Time.realtimeSinceStartup < until) yield return null;
            s = SceneManager.GetSceneByName(sceneName);
            if (!TestProbe.Check($"{sceneName} se carga", s.isLoaded)) yield break;
            loadedHere = true;
        }
        yield return null; yield return null;

        var sites = new List<AreaFunctionSite>(); var jornadas = new List<MicroJornada>(); var couples = new List<FolkCouple>();
        int humans = 0, lairs = 0; var species = new HashSet<string>(); string tall = "";
        foreach (GameObject root in s.GetRootGameObjects())
        {
            sites.AddRange(root.GetComponentsInChildren<AreaFunctionSite>(true));
            jornadas.AddRange(root.GetComponentsInChildren<MicroJornada>(true));
            couples.AddRange(root.GetComponentsInChildren<FolkCouple>(true));
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == CubeHumanoid.BodyName && InsectOnly.IsHuman(t.parent.gameObject)) humans++;
                if (t.name == "Guarida" && t.GetComponent<MicroLair>() != null) lairs++;
            }
            foreach (CubeInsect ci in root.GetComponentsInChildren<CubeInsect>(true))
            {
                if (!ci.folk || (ci.TryGetComponent(out BodyAging grow) && grow.growth < BodyAging.Adult)) continue;   // los niños nacidos aquí (crecen con BodyAging)
                CapsuleCollider cc = ci.GetComponent<CapsuleCollider>(); if (cc != null && cc.height < 1.5f) { species.Add(ci.species); continue; }   // los niños (refugiados del 42) son pequeños a propósito
                species.Add(ci.species);
                Transform ins = ci.transform.Find("Insecto"); if (ins == null) continue;
                bool any = false; Bounds b = default;
                foreach (Renderer r in ins.GetComponentsInChildren<Renderer>()) { if (!r.enabled) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
                if (any && (b.size.y < 1.3f || b.size.y > 2.3f) && !ci.name.StartsWith("Herido")) tall += $"{ci.name} {b.size.y:0.0} m; ";
            }
        }
        TestProbe.Check($"{sceneName}: nadie con cuerpo humano", humans == 0, $"{humans}");
        TestProbe.Check($"{sceneName}: sin la cuadrilla de recolectoras con guarida", lairs == 0, $"{lairs} guaridas");
        TestProbe.Check($"{sceneName}: al menos 2 edificios de la era", sites.Count >= 2, $"{sites.Count}");
        int year = Eras.Now; string bad = "";
        foreach (AreaFunctionSite site in sites)
        {
            if (site.Stations.Length == 0) bad += $"{site.title} sin puestos; ";
            else if (site.Functions(year).Count == 0) bad += $"{site.title} ({site.building}) sin funciones en {Eras.Format(year)}; ";
            else
            {
                AreaFunctionStation st = site.Stations[0];
                int u = st.uses; var here = st.Here();
                if (here.Count > 0) { st.Do(here[0], null); if (st.uses != u + 1) bad += $"{site.title}: el puesto no responde; "; }
            }
        }
        TestProbe.Check($"{sceneName}: cada edificio tiene puestos y funciones del registro para {Eras.Format(year)}", bad.Length == 0, bad);
        // (En el frente de 1914 son dos colonias de HORMIGAS: una especie, dos colores.)
        TestProbe.Check($"{sceneName}: insectos-humanos de ≥ 3 especies", species.Count >= (sceneName == "Microcosmos_Tregua1914" ? 1 : 3), string.Join(", ", species));
        TestProbe.Check($"{sceneName}: todos de estatura parecida (1,3–2,3 m)", tall.Length == 0, tall);
        string jbad = "";
        foreach (MicroJornada j in jornadas) { j.ChooseNow("trabajo"); if (j.work != null && j.Station == null) jbad += j.name + "; "; }
        TestProbe.Check($"{sceneName}: con jornada (≥ 3) y cada uno encuentra puesto en su edificio", jornadas.Count >= 3 && jbad.Length == 0, jbad.Length > 0 ? jbad : $"{jornadas.Count} con jornada");
        if (couples.Count > 0)
        {
            FolkLineage mom = couples[0].GetComponent<FolkLineage>();
            if (TestProbe.Check($"{sceneName}: una pareja con linaje", mom != null && mom.partner != null))
            {
                int before = mom.children;
                GameObject kid = FolkCouple.Birth(mom, mom.partner, 0.3f);
                FolkLineage kl = kid != null ? kid.GetComponent<FolkLineage>() : null;
                bool inTree = kl != null && kl.ancestors.Exists(a => a.species == kl.species);
                TestProbe.Check($"{sceneName}: nace un hijo con una especie de su árbol", inTree && mom.children == before + 1, kl != null ? kl.species : "no nació");
                if (kid != null) { mom.children--; mom.partner.children--; Destroy(kid); }
            }
        }
        else if (sceneName != "Microcosmos_Tregua1914") TestProbe.Check($"{sceneName}: al menos una pareja (FolkCouple)", false);

        // REGLA 6 · HOGARES (Berón 2026-11-02): cada insecto-humano tiene hogar —cama (una por persona), retrete, lavabo, mesa con
        // sillas y despensa—, las parejas comparten el suyo, y todos llevan el ciclo de vida de una persona (PersonLife: necesidades,
        // edad, familia). Y el pueblo tiene LUGARES DE TRABAJO (edificios que no son casas, donde trabaja cada jornada) y SERVICIOS
        // BÁSICOS (las seis necesidades de la regla 4, que mira el validador de abajo).
        var homeBad = new List<string>(); int folk = 0, alive = 0, paired = 0;
        foreach (GameObject root in s.GetRootGameObjects())
        {
            homeBad.AddRange(Household.Check(root.transform));
            foreach (FolkLineage f in root.GetComponentsInChildren<FolkLineage>(true))
            {
                folk++;
                if (f.TryGetComponent(out PersonLife pl) && f.GetComponent<NeedsDriver>() != null && f.GetComponent<BodyAging>() != null && f.GetComponent<BodyMetabolism>() != null) alive++;
                if (pl != null && pl.partner != null && pl.home != null && pl.home == pl.partner.home) paired++;
            }
        }
        TestProbe.Check($"{sceneName}: cada insecto-humano tiene hogar con cama, retrete, lavabo, mesa con sillas y despensa", homeBad.Count == 0,
            homeBad.Count == 0 ? $"{folk} personas" : string.Join(" · ", homeBad.GetRange(0, Mathf.Min(8, homeBad.Count))));
        TestProbe.Check($"{sceneName}: todos con el ciclo de vida de una persona (PersonLife, NeedsDriver, BodyMetabolism, BodyAging)", alive == folk, $"{alive}/{folk}");
        if (couples.Count > 0) TestProbe.Check($"{sceneName}: cada pareja comparte su hogar", paired >= 2 * couples.Count, $"{paired} personas en pareja con casa común, {couples.Count} parejas");
        int workplaces = 0; string jobless = "";
        foreach (AreaFunctionSite site in sites) if (site.building != "Casa" && site.building != "Orfanato" && site.Stations.Length > 0) workplaces++;
        foreach (MicroJornada j in jornadas) if (j.follow == null && j.work != null && j.work.Stations.Length == 0) jobless += j.name + "; ";
        TestProbe.Check($"{sceneName}: hay lugares de trabajo (edificios que no son casas) y cada jornada tiene el suyo", workplaces >= 1 && jobless.Length == 0, jobless.Length > 0 ? jobless : $"{workplaces} lugares de trabajo");

        // Las REGLAS DE EDIFICACIÓN (docs/microcosmos-reglas-edificacion.md): paso de 2 m, sin muros ni techos, muebles
        // detallados y las necesidades básicas, en cada escena.
        var rules = new List<string>();
        foreach (GameObject root in s.GetRootGameObjects()) if (root.GetComponentInChildren<AreaFunctionSite>(true) != null) rules.AddRange(MicroBuildRules.Validate(root.transform, sceneName, true, false));
        TestProbe.Check($"{sceneName}: cumple las reglas de edificación", rules.Count == 0, string.Join(" · ", rules.GetRange(0, Mathf.Min(8, rules.Count))));

        // (2026-11-04) REGLA 15: un pueblo de historia tiene trabajo, ocio, entrenamiento y recuperación; lo que el plano histórico no
        // traía sale de un mapa actual (y lo dice), recreado con lo nativo de la época.
        foreach (GameObject root in s.GetRootGameObjects())
            foreach (MicroFloor mf in root.GetComponentsInChildren<MicroFloor>(true))
            {
                if (!mf.town) continue;
                var cat = MicroFloors.ValidateCategories(mf.transform);
                var kinds = new List<string>();
                foreach (AreaFunctionSite site in mf.GetComponentsInChildren<AreaFunctionSite>(true)) if (site.activity == AreaFunctionSite.Entrenamiento) kinds.Add($"{site.title} [{site.training}]{(site.mapSource.Length > 0 ? " · " + site.mapSource : "")}");
                TestProbe.Check($"{sceneName}: el pueblo tiene trabajo, ocio, entrenamiento y recuperación (y cada posición de un mapa actual dice cuál)", cat.Count == 0,
                    cat.Count > 0 ? string.Join(" · ", cat.GetRange(0, Mathf.Min(6, cat.Count))) : "entrenamiento: " + string.Join("; ", kinds));
            }

        if (loadedHere)
        {
            AsyncOperation un = SceneManager.UnloadSceneAsync(s);
            float u2 = Time.realtimeSinceStartup + 20f;
            while (un != null && !un.isDone && Time.realtimeSinceStartup < u2) yield return null;
        }
    }
}
