using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TEST DEL PISO DE LOS MICROCOSMOS (docs/reglas-plataformas.md, regla 11; Berón 2026-10-29): el mapa de un microcosmos es SOLO
/// el piso elegido ×10, con MUROS en su borde, y todo —flora, fauna, personajes y estructuras de la historia— vive DENTRO. Mira
/// los microcosmos que ya estén cargados (el de la experiencia del jugador) y, uno a uno, los de Build Settings («Microcosmos_…»):
/// los carga en aditivo, los revisa y los descarga (cada espera, acotada). Por cada uno comprueba:
/// (1) tiene su piso apuntado (<see cref="MicroFloor"/>, lo pone <c>AreaMicrocosmos</c>);
/// (2) hay muro en los cuatro lados, cubriendo el lado entero, más alto que la fauna más grande y con collider;
/// (3) cada renderer de la flora, la fauna, los personajes y la historia cae DENTRO del rectángulo de los muros;
/// (4) dentro crece al menos <see cref="minFlora"/> plantas;
/// (5) hay flora y fauna ENCIMA de los muebles gigantes (sus caras de arriba, a ×10 mesetas), dentro de los muros.
/// Se añade solo al orquestador cuando se piden las pruebas (como <see cref="KitchenInitiationTest"/>).
/// </summary>
public class MicrocosmosFloorTest : MonoBehaviour, ITestUnit
{
    public int Group => 43;   // tras las iniciaciones (40–42): cargar y descargar escenas no les toca nada
    public bool ParallelSafe => false;

    [Tooltip("Plantas mínimas dentro del piso (las plazas de flora de un piso justo).")]
    public int minFlora = PlatformSlots.MinFlora;
    [Tooltip("Altura mínima de los muros (m): la mantis, el bicho más alto, mide 10 m; con la tapa, nada vuela fuera.")]
    public float minWallHeight = 40f;
    [Tooltip("Holgura (m) al mirar si algo está dentro: la fauna anda y sus patas pueden rozar el muro.")]
    public float tolerance = 1.5f;
    [Tooltip("Cargar también los microcosmos de Build Settings que no estén cargados.")]
    public bool loadFromBuildSettings = true;
    public int maxScenes = 70;   // (2026-11-04) ya hay ~31 microcosmos y 7 más de los famosos (docs/microcosmos-famosos.md): con 30 se saltaba los últimos
    public float loadTimeout = 120f;   // (2026-11-03) los pueblos ×60 tardan más en cargar (40 s no bastaban: Bagdad, Berkeley, Angostura, Palenque)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<MicrocosmosFloorTest>() == null) tr.gameObject.AddComponent<MicrocosmosFloorTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Microcosmos: el mapa es solo el piso ×10, con muros, y todo vive dentro");
        int seen = 0;
        var done = new HashSet<string>();
        foreach (MicroFloor f in FindObjectsByType<MicroFloor>(FindObjectsSortMode.None))
        {
            Inspect(f, f.gameObject.scene.name);
            done.Add(f.gameObject.scene.name);
            seen++;
        }

        if (loadFromBuildSettings)
        {
            int loaded = 0;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings && loaded < maxScenes; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (!name.StartsWith("Microcosmos_") || done.Contains(name) || SceneManager.GetSceneByPath(path).isLoaded) continue;
                AsyncOperation op = SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
                float until = Time.realtimeSinceStartup + loadTimeout;
                while (op != null && !op.isDone && Time.realtimeSinceStartup < until) yield return null;
                if (op == null || !op.isDone) { TestProbe.Check($"{name}: se carga", false, $"no cargó en {loadTimeout:0} s"); continue; }
                loaded++;
                Scene s = SceneManager.GetSceneByPath(path);
                MicroFloor f = null;
                foreach (GameObject g in s.GetRootGameObjects()) { f = g.GetComponentInChildren<MicroFloor>(true); if (f != null) break; }
                if (f == null) TestProbe.Check($"{name}: tiene su piso con muros (MicroFloor)", false, "¿escena de antes de la regla 11? Reconstruye los microcosmos");
                else { Inspect(f, name); seen++; }
                AsyncOperation un = SceneManager.UnloadSceneAsync(s);
                until = Time.realtimeSinceStartup + loadTimeout;
                while (un != null && !un.isDone && Time.realtimeSinceStartup < until) yield return null;
            }
        }
        TestProbe.Check("hay al menos un microcosmos que revisar", seen > 0, $"{seen} revisados");
        TestProbe.End();
    }

    /// <summary>Las comprobaciones de un microcosmos (también se puede llamar desde otras pruebas con el ya cargado).</summary>
    public void Inspect(MicroFloor f, string scene)
    {
        Bounds rect = f.WorldRect();
        TestProbe.Check($"{scene}: el piso tiene tamaño (×10)", f.size.x > 20f && f.size.y > 20f, $"{f.size.x:0} × {f.size.y:0} m");

        // (2) Los cuatro muros: cada uno cubre su lado entero, alto y sólido.
        Transform walls = f.transform.Find("Muros_del_piso");
        int ok = 0; string why = walls == null ? "sin «Muros_del_piso»" : "";
        if (walls != null)
            foreach (string wn in MicroFloor.WallNames)
            {
                Transform w = walls.Find(wn);
                Renderer r = w != null ? w.GetComponent<Renderer>() : null;
                Collider c = w != null ? w.GetComponent<Collider>() : null;
                if (r == null || c == null || !c.enabled || c.isTrigger) { why += $"{wn}: falta o sin collider; "; continue; }
                Bounds b = r.bounds;
                bool alongX = wn == "Muro_N" || wn == "Muro_S";
                bool covers = alongX ? b.min.x <= rect.min.x + 0.5f && b.max.x >= rect.max.x - 0.5f : b.min.z <= rect.min.z + 0.5f && b.max.z >= rect.max.z - 0.5f;
                bool atEdge = wn == "Muro_N" ? Mathf.Abs(b.min.z - rect.max.z) < 1f : wn == "Muro_S" ? Mathf.Abs(b.max.z - rect.min.z) < 1f
                            : wn == "Muro_E" ? Mathf.Abs(b.min.x - rect.max.x) < 1f : Mathf.Abs(b.max.x - rect.min.x) < 1f;
                bool tall = b.max.y - rect.center.y >= minWallHeight;
                if (covers && atEdge && tall) ok++;
                else why += $"{wn}: {(covers ? "" : "no cubre el lado ")}{(atEdge ? "" : "no está en el borde ")}{(tall ? "" : $"bajo ({b.max.y - rect.center.y:0} m)")}; ";
            }
        TestProbe.Check($"{scene}: muros en los cuatro lados del piso", ok == 4, ok == 4 ? $"altura {f.wallHeight:0} m" : why);
        bool lid = walls != null && walls.Find("Tapa_invisible") != null && walls.Find("Tapa_invisible").GetComponent<Collider>() != null;
        TestProbe.Check($"{scene}: tapa arriba (nada vuela fuera)", lid);

        // (3) Todo dentro: flora, fauna, personajes e historia.
        int outside = 0, total = 0; string first = "";
        void Look(Transform g, string what)
        {
            if (g == null) return;
            foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled) continue;
                total++;
                if (f.Inside(r.bounds, tolerance)) continue;
                outside++;
                if (first.Length == 0) first = $" (p. ej. {what} «{r.transform.parent?.name}/{r.name}» en {r.bounds.center.x - rect.center.x:0}, {r.bounds.center.z - rect.center.z:0})";
            }
        }
        Transform flora = f.transform.Find("Flora_a_escala"), fauna = f.transform.Find("Fauna_a_escala"), story = f.transform.Find("Historia");
        Look(flora, "planta"); Look(fauna, "bicho"); Look(story, "historia");
        // Los personajes y bichos que anden sueltos por la raíz (fuera de esos grupos) también.
        foreach (Anima a in f.GetComponentsInChildren<Anima>())
            if ((story == null || !a.transform.IsChildOf(story)) && (fauna == null || !a.transform.IsChildOf(fauna))) Look(a.transform, "ánima");
        TestProbe.Check($"{scene}: flora, fauna, personajes e historia DENTRO de los muros", outside == 0, $"{total - outside}/{total} piezas dentro{first}");

        // (4) Flora dentro, de sobra.
        int plants = 0;
        if (flora != null) foreach (Transform p in flora) if (p.position.x >= rect.min.x && p.position.x <= rect.max.x && p.position.z >= rect.min.z && p.position.z <= rect.max.z) plants++;
        TestProbe.Check($"{scene}: crece flora dentro del piso (≥ {minFlora})", plants >= minFlora, $"{plants} plantas (al construir: {f.floraInside}; fauna {f.faunaInside}, personajes {f.charactersInside}, fuera {f.outside})");
        TestProbe.Check($"{scene}: al construirlo no quedó nada fuera", f.outside == 0, $"{f.outside} piezas fuera");

        // (5) ENCIMA DE LOS MUEBLES (Berón 2026-10-29): a ×10 sus caras de arriba son mesetas y también se pueblan. Plantas por
        //     encima del suelo (≥ 1 m sobre él) y dentro de los muros ahora; los bichos, como se pusieron al construirlo (andan).
        if (f.furnitureTops > 0)
        {
            int up = 0;
            if (flora != null) foreach (Transform p in flora) if (p.position.y > rect.center.y + 1f && f.Inside(new Bounds(p.position, Vector3.zero), 0f)) up++;
            TestProbe.Check($"{scene}: crece flora encima de los muebles gigantes", up > 0 && f.floraOnTops > 0, $"{up} plantas en alto ahora; al construirlo {f.floraOnTops} en {f.furnitureTops} muebles");
            TestProbe.Check($"{scene}: hay fauna encima de los muebles gigantes", f.faunaOnTops > 0, $"{f.faunaOnTops} bichos puestos encima");
        }
        else TestProbe.Check($"{scene}: el piso tiene muebles con cara de arriba que poblar (o ninguno, y se dice)", true, "ningún mueble a nivel de ≥ 6 m²");

        // (6) EL PUEBLO (eras avanzadas, regla 5 de docs/microcosmos-reglas-edificacion.md; Berón 2026-11-01: «en Bruce Lee las flores
        //     gigantes tapan el pueblo; en Berna el musgo son cubos verdes enormes»): la flora es la de un pueblo humano — nada más
        //     alto que un árbol de calle (25 m), nada flotando, nada plantado en una calle ni en un solar — y hay animales.
        if (f.town && flora != null) TownChecks(f, scene, rect, flora, story);

        // (6b) LAS REGLAS DE LOS MICROCOSMOS (2026-11-03, MicroFloors): pertenece a un piso y va montado ENCIMA de él, sobre su forma
        //      FINALIZADA (nada de obra), las estructuras del piso pobladas, el pueblo entero en una era avanzada y, si el piso no lo
        //      representa, «pendiente de piso representativo» marcado.
        var floorRules = MicroFloors.Validate(f.transform, scene, false);
        TestProbe.Check($"{scene}: reglas de los microcosmos (piso, forma finalizada, estructuras pobladas, pueblo entero, piso pendiente)", floorRules.Count == 0, floorRules.Count == 0 ? $"sobre {f.platform}{(f.representative ? "" : $", pendiente: {f.pendingFloor}")}" : string.Join(" · ", floorRules));

        // (7) LAS MISIONES DE COMPASIÓN: desde donde empieza cada refugiado se llega a cada refugio y escondite andando por el NavMesh
        //     y cruzando los pasos que admiten acompañantes (la puerta del gueto no: el pase era solo de Irena).
        if (f.GetComponentInChildren<CompassionMission>(true) != null) CompassionPaths(f, scene);
    }

    void TownChecks(MicroFloor f, string scene, Bounds rect, Transform flora, Transform story)
    {
        int tall = 0, floating = 0, onTown = 0, plants = 0; string firstTall = "", firstFloat = "", firstOn = "";
        var lots = new List<Bounds>(); var streets = new List<Bounds>();
        if (story != null)
        {
            foreach (AreaFunctionSite s in story.GetComponentsInChildren<AreaFunctionSite>(true))
            {
                bool any = false; Bounds b = default;
                foreach (Renderer r in s.GetComponentsInChildren<Renderer>(true))
                {
                    if (r.GetComponentInParent<CubeInsect>(true) != null) continue;
                    if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
                }
                // (2026-11-04) Un edificio del barrio alto, sobre la meseta de un mueble gigante: lo de debajo (la planta bajo el tablero) no está en su solar.
                if (any && b.min.y - rect.center.y <= 2f) lots.Add(b);
            }
            foreach (Renderer r in story.GetComponentsInChildren<Renderer>(true))
                for (Transform p = r.transform; p != null && p != story; p = p.parent) if (p.name.StartsWith("Calle")) { streets.Add(r.bounds); break; }
        }
        foreach (Transform p in flora)
        {
            bool any = false; Bounds b = default;
            foreach (Renderer r in p.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
            if (!any) continue;
            if (p.name.EndsWith("(sobre mueble)")) continue;   // (2026-11-03) sobre la meseta de un mueble gigante fuera del pueblo: la mira la regla (5)
            plants++;
            if (b.max.y - rect.center.y > 25f) { tall++; if (firstTall.Length == 0) firstTall = $" (p. ej. «{p.name}», {b.max.y - rect.center.y:0} m)"; }
            if (b.min.y - rect.center.y > 1f) { floating++; if (firstFloat.Length == 0) firstFloat = $" (p. ej. «{p.name}» a {b.min.y - rect.center.y:0.0} m del suelo)"; }
            Vector3 at = p.position; bool on = false;
            foreach (Bounds l in lots) if (at.x > l.min.x && at.x < l.max.x && at.z > l.min.z && at.z < l.max.z) { on = true; break; }
            if (!on) foreach (Bounds s in streets) if (at.x > s.min.x && at.x < s.max.x && at.z > s.min.z && at.z < s.max.z) { on = true; break; }
            if (on) { onTown++; if (firstOn.Length == 0) firstOn = $" (p. ej. «{p.name}» en {at.x - rect.center.x:0}, {at.z - rect.center.z:0})"; }
        }
        TestProbe.Check($"{scene}: pueblo — la flora a escala de pueblo (≤ 25 m)", tall == 0, $"{tall}/{plants} plantas más altas{firstTall}");
        TestProbe.Check($"{scene}: pueblo — ninguna planta flota", floating == 0, $"{floating}/{plants} flotando{firstFloat}");
        TestProbe.Check($"{scene}: pueblo — ninguna planta en una calle ni en un solar", onTown == 0, $"{onTown}/{plants} en {streets.Count} tramos de calle y {lots.Count} solares{firstOn}");
        TestProbe.Check($"{scene}: pueblo — tiene sus animales (los del pueblo o los del lugar)", f.faunaInside > 0, $"{f.faunaInside} animales");
    }

    void CompassionPaths(MicroFloor f, string scene)
    {
        Refugee[] refugees = f.GetComponentsInChildren<Refugee>(true);
        Passage[] passages = f.GetComponentsInChildren<Passage>(true);
        var goals = new List<(string name, Vector3 at)>();
        foreach (SafeHaven h in f.GetComponentsInChildren<SafeHaven>(true)) goals.Add(($"refugio «{h.label}»", h.transform.position));
        foreach (HidingPlace h in f.GetComponentsInChildren<HidingPlace>(true)) goals.Add(($"escondite «{h.label}»", h.transform.position));
        // Los nodos: los refugiados, la entrada y la salida de cada paso, y cada destino. Las aristas: un camino COMPLETO del NavMesh
        // entre dos nodos, o el propio paso (entrada → salida) si deja cruzar acompañantes.
        var pts = new List<Vector3>();
        foreach (Refugee r in refugees) pts.Add(r.transform.position);
        int pass0 = pts.Count;
        foreach (Passage p in passages) { pts.Add(p.transform.position); pts.Add(p.exit != null ? p.exit.position : p.transform.position); }
        int goal0 = pts.Count;
        foreach (var g in goals) pts.Add(g.at);
        int n = pts.Count;
        var on = new bool[n]; var snapped = new Vector3[n];
        for (int i = 0; i < n; i++) { on[i] = UnityEngine.AI.NavMesh.SamplePosition(pts[i], out UnityEngine.AI.NavMeshHit h, 4f, UnityEngine.AI.NavMesh.AllAreas); snapped[i] = on[i] ? h.position : pts[i]; }
        var path = new UnityEngine.AI.NavMeshPath();
        var link = new bool[n, n];
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
                if (on[i] && on[j] && UnityEngine.AI.NavMesh.CalculatePath(snapped[i], snapped[j], UnityEngine.AI.NavMesh.AllAreas, path) && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    link[i, j] = link[j, i] = true;
        for (int k = 0; k < passages.Length; k++) if (!passages[k].alone && passages[k].exit != null) link[pass0 + 2 * k, pass0 + 2 * k + 1] = true;
        int bad = 0; string first = "";
        for (int r = 0; r < refugees.Length; r++)
        {
            var seen = new bool[n]; var q = new Queue<int>(); q.Enqueue(r); seen[r] = true;
            while (q.Count > 0) { int a = q.Dequeue(); for (int b = 0; b < n; b++) if (link[a, b] && !seen[b]) { seen[b] = true; q.Enqueue(b); } }
            for (int g = 0; g < goals.Count; g++)
                if (!seen[goal0 + g]) { bad++; if (first.Length == 0) first = $" (p. ej. {refugees[r].displayName} → {goals[g].name})"; }
        }
        int offMesh = 0; for (int i = 0; i < n; i++) if (!on[i]) offMesh++;
        TestProbe.Check($"{scene}: compasión — cada refugiado llega a cada refugio y escondite (andando y por los pasos)", bad == 0 && refugees.Length > 0 && goals.Count > 0,
                        $"{refugees.Length} refugiados, {passages.Length} pasos, {goals.Count} destinos; {bad} sin camino{first}; {offMesh} puntos fuera del NavMesh");
        TestProbe.Check($"{scene}: compasión — tiene al menos un paso (Passage) con salida", System.Array.Exists(passages, p => p.exit != null), $"{passages.Length} pasos");
    }
}
