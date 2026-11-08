using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PRUEBA DE LA PLANTA NUCLEAR BAJO EL MAR (Berón 2026-11-08; docs/sitio-nuclear.md §25): la Central Nuclear y la Planta Atómica viven enteras en el
/// santuario 2 (el Marino), junto a las cavernas; en el santuario 1 solo queda su ESTACIÓN DE ALFOMBRAS con la Línea, como el muelle del Aéreo.
/// 1. ARRIBA: ninguna pieza del sitio por encima del agua (y > 0) salvo la estación; ni el árbol del sitio ni sus áreas en el santuario 1.
/// 2. LA ESTACIÓN: una pista de la red del santuario (como el muelle aéreo), a mano del andén de la Línea; sin escaleras.
/// 3. LA JORNADA: las dos áreas apuntan a la estación; quien trabaja en ellas baja por la Línea sin cargar nada y vuelve; el viaje cuenta en la salida.
/// 4. LA ENERGÍA: los pisos de la planta siguen pagando su demanda desde el santuario 1.
/// 5. ABAJO: con el Santuario Marino cargado, las dos áreas y su maquinaria están allí, todo por debajo de y 0, y CADA PUESTO DE TRABAJO se alcanza
///    andando desde el andén de abajo (2 m de paso, sin escaleras).
/// </summary>
public class NuclearSubmarinaTest : MonoBehaviour, ITestUnit
{
    public int Group => 114;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<NuclearSubmarinaTest>() == null) tr.gameObject.AddComponent<NuclearSubmarinaTest>();
    }

    static float Flat(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    static bool PlantArea(SanctuaryArea a) => a != null && NuclearSite.Underwater(a.displayName);

    public IEnumerator Run()
    {
        TestProbe.Begin("La planta nuclear bajo el mar: arriba solo la estación de alfombras; abajo, las áreas y cada puesto alcanzable");
        NuclearSite ns = FindAnyObjectByType<NuclearSite>();
        SiteLine line = SiteLine.Instance;
        if (ns == null || line == null) { TestProbe.Check("hay sitio nuclear y Línea en el santuario 1", false); TestProbe.End(); yield break; }
        Scene s1 = ns.gameObject.scene;
        Transform site = ns.transform;
        Transform station = site.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == NuclearSiteRule.StationName);

        // ── 1. ARRIBA, SOLO LA ESTACIÓN ──
        TestProbe.Check("las dos áreas son del santuario 2 y van bajo el agua", NuclearSite.Underwater(NuclearSite.PlantArea) && NuclearSite.Underwater(NuclearSite.PowerArea)
            && SanctuaryPlacement.SanctuaryOfArea(NuclearSite.PlantArea) == SanctuaryPlacement.Marine && SanctuaryPlacement.SanctuaryOfArea(NuclearSite.PowerArea) == SanctuaryPlacement.Marine);
        var upAreas = FindObjectsByType<SanctuaryArea>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(a => PlantArea(a) && a.gameObject.scene == s1).ToList();
        TestProbe.Check("ni la Central Nuclear ni la Planta Atómica en el santuario 1", upAreas.Count == 0, string.Join(", ", upAreas.Select(a => a.name)));
        TestProbe.Check("ni el árbol del sitio", !ArbolCampos.All.Any(a => a != null && a.building == NuclearSiteRule.Building && a.gameObject.scene == s1));
        TestProbe.Check("hay estación", station != null, NuclearSiteRule.StationName);
        var above = new List<string>();
        if (station != null)
            foreach (Renderer r in site.GetComponentsInChildren<Renderer>(true))
            {
                Bounds b = r.bounds;
                if (b.max.y <= 0.05f) continue;
                if (r.name.StartsWith("Boya_")) continue;   // (las boyas flotan: marcan la zona y la línea, no son edificios)
                Vector3 c = b.center;
                bool inStation = Mathf.Abs(c.x - station.position.x) <= NuclearSiteRule.StationHalf + 1f && Mathf.Abs(c.z - station.position.z) <= NuclearSiteRule.StationHalf + 1f;
                if (!inStation) above.Add($"{r.name} ({c.x:0}, {b.max.y:0.0}, {c.z:0})");
            }
        TestProbe.Check($"nada del sitio por encima del agua salvo la estación (±{NuclearSiteRule.StationHalf:0} m)", station != null && above.Count == 0, above.Count == 0 ? "solo la estación" : $"{above.Count}: {string.Join(", ", above.Take(6))}");
        TestProbe.Check("la estación es pequeña", NuclearSiteRule.StationHalf <= 20f, $"{2f * NuclearSiteRule.StationHalf:0} × {2f * NuclearSiteRule.StationHalf:0} m");
        TestProbe.Check("en el sitio de siempre o cerca (≤ 85 m), dentro de la zona de exclusión", station != null && Flat(station.position, NuclearSiteRule.Center) <= 85f && NuclearSiteRule.InZone(station.position));

        // ── 2. LA PISTA: de la red del santuario, como el muelle aéreo; sin escaleras ──
        var pads = FindObjectsByType<CarpetPad>(FindObjectsSortMode.None).Where(p => p != null && p.gameObject.scene == s1).ToList();
        CarpetPad pad = pads.FirstOrDefault(p => p.surface == $"{NuclearSiteRule.Building}#linea");
        CarpetPad other = pads.FirstOrDefault(p => p != pad && p.kind == CarpetPad.Kind.Santuario);
        TestProbe.Check("su pista es de la red del santuario y se vuela a ella desde las demás", pad != null && pad.kind == CarpetPad.Kind.Santuario && other != null && other.Destinations().Contains(pad) && pad.Destinations().Contains(other),
            pad != null ? $"«{pad.displayName}» ({pad.kind})" : "sin pista");
        TestProbe.Check("la pista, a mano del andén de la Línea", pad != null && Vector3.Distance(pad.transform.position, line.transform.position) < 12f, pad != null ? $"{Vector3.Distance(pad.transform.position, line.transform.position):0.0} m" : "");
        int stairs = site.GetComponentsInChildren<Transform>(true).Count(t => t.name.StartsWith("Escalera") || t.name.StartsWith("Peldano"));
        TestProbe.Check("sin escaleras", stairs == 0, $"{stairs}");

        // ── 3. LA JORNADA ──
        foreach (string area in new[] { NuclearSite.PlantArea, NuclearSite.PowerArea })
        {
            Vector3 p = WorkdayDirector.AreaPoint(area, "Prueba");
            TestProbe.Check($"«{area}»: la jornada lleva a la estación", p != Vector3.zero && Vector3.Distance(p, line.transform.position) < 15f && WorkdayDirector.InWorkspace(null, area, line.transform.position + Vector3.right), $"{p}");
        }
        TestProbe.Check("el viaje de la Línea cuenta al salir hacia la planta", SiteLine.TripSeconds(NuclearSite.PowerArea) >= SiteLineRule.OneWay(SiteLineRule.Drop) - 0.1f && SiteLine.TripSeconds("Cocina") == 0f, $"{SiteLine.TripSeconds(NuclearSite.PowerArea):0} s");
        GameObject body = new GameObject("Prueba_planta_marina");
        MeshRenderer probe = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshRenderer>(); Destroy(probe.GetComponent<Collider>()); probe.transform.SetParent(body.transform, false);
        body.transform.position = line.transform.position + Vector3.up;
        SiteLine.Note(body, NuclearSite.PowerArea);
        bool went = SiteLine.BoardWork(body);
        yield return null;
        bool hidden = SiteLine.IsAway(body) && !probe.enabled && MarineBase.Instance == null;
        SiteLine.Note(body, null);   // su jornada lo lleva a otra parte: vuelve
        float oldTs = Time.timeScale; Time.timeScale = 4f;
        float t0 = Time.time, r0 = Time.realtimeSinceStartup;
        while (SiteLine.IsAway(body) && Time.time - t0 < 3f * SiteLineRule.OneWay(SiteLineRule.Drop) && Time.realtimeSinceStartup - r0 < 120f) yield return null;
        Time.timeScale = oldTs;
        TestProbe.Check("quien trabaja en la planta baja por la Línea, oculto y sin cargar la escena, y vuelve a la estación", went && hidden && !SiteLine.IsAway(body) && probe.enabled && Vector3.Distance(body.transform.position, line.ExitPoint) < 3f,
            $"bajó {went}, oculto {hidden}; tardó {Time.time - t0:0} s");
        Destroy(body);

        // ── 4. LA ENERGÍA ──
        FloorEnergy fe = FloorEnergy.All.FirstOrDefault(f => f != null && f.floor == NuclearSiteRule.PlantFloorsName);
        TestProbe.Check("los pisos de la planta pagan su demanda desde el santuario 1", fe != null && Mathf.Abs(fe.KW - NuclearSiteRule.PlantFloorsKW) < 0.5f, fe != null ? $"{fe.KW:0} kW" : "sin FloorEnergy");

        // ── 5. ABAJO ──
        if (!Application.CanStreamedLevelBeLoaded(SiteLine.MarineScene)) { TestProbe.Check("el Santuario Marino está construido y en Build Settings", false, "Tools/Cold Sanctuary/Build Santuario Marino"); TestProbe.End(); yield break; }
        bool loadedHere = MarineBase.Instance == null;
        if (loadedHere)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(SiteLine.MarineScene, LoadSceneMode.Additive);
            float rl = Time.realtimeSinceStartup;
            while (op != null && !op.isDone && Time.realtimeSinceStartup - rl < 30f) yield return null;
        }
        MarineBase mb = MarineBase.Instance;
        if (mb == null) { TestProbe.Check("el Santuario Marino carga", false); TestProbe.End(); yield break; }
        Scene s2 = mb.gameObject.scene;
        var down = FindObjectsByType<SanctuaryArea>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(a => PlantArea(a) && a.gameObject.scene == s2).ToList();
        TestProbe.Check("abajo, las dos áreas", down.Any(a => a.displayName == NuclearSite.PlantArea) && down.Any(a => a.displayName == NuclearSite.PowerArea), string.Join(", ", down.Select(a => a.displayName)));
        Transform plant = down.FirstOrDefault(a => a.displayName == NuclearSite.PlantArea)?.transform, power = down.FirstOrDefault(a => a.displayName == NuclearSite.PowerArea)?.transform;
        TestProbe.Check("con su maquinaria: reactor, consola, ciclotrón, equipo, sala de física, control y conversión de la central y el centro de emergencias",
            plant != null && power != null && plant.GetComponentInChildren<ResearchReactor>(true) != null && plant.GetComponentInChildren<ReactorConsole>(true) != null && plant.GetComponentInChildren<Cyclotron>(true) != null
            && plant.GetComponentsInChildren<PlantEquipment>(true).Length >= 5 && plant.Find("Sala_experimentos_fisica") != null
            && power.GetComponentsInChildren<Transform>(true).Any(t => t.name == "Sala_control") && power.GetComponentsInChildren<Transform>(true).Any(t => t.name == "Contenedor_turbina")
            && mb.GetComponentsInChildren<Transform>(true).Any(t => t.name == NuclearSiteRule.EmergencyName));
        var high = new List<string>();
        foreach (SanctuaryArea a in down) foreach (Renderer r in a.GetComponentsInChildren<Renderer>(true)) if (r.bounds.max.y > 0f) high.Add($"{a.displayName}/{r.name} ({r.bounds.max.y:0})");
        TestProbe.Check("todo por debajo del agua (y < 0)", high.Count == 0, high.Count == 0 ? $"lo más alto a {down.SelectMany(a => a.GetComponentsInChildren<Renderer>(true)).Select(r => r.bounds.max.y).DefaultIfEmpty(0f).Max():0} m" : string.Join(", ", high.Take(6)));
        TestProbe.Check("junto a las cavernas", plant != null && power != null && Flat(plant.position, NuclearSiteRule.Center) < 200f && Flat(power.position, NuclearSiteRule.Center) < 200f);

        // CADA PUESTO ALCANZABLE: rejilla de 1 m sobre el suelo andable, desde el andén de abajo; un paso vale si no sube más de 0,4 m y una persona
        // (cápsula de 0,35 m) cabe. Los puestos: los AreaSeat, las consolas y los puestos de las dos áreas, y los del centro de emergencias.
        var posts = new List<Transform>();
        foreach (SanctuaryArea a in down)
        {
            posts.AddRange(a.GetComponentsInChildren<AreaSeat>(true).Select(s => s.transform));
            posts.AddRange(a.GetComponentsInChildren<ReactorConsole>(true).Select(s => s.transform));
            posts.AddRange(a.GetComponentsInChildren<CyclotronConsole>(true).Select(s => s.transform));
            posts.AddRange(a.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("Consola_operador") || t.name.StartsWith("Bloque_")));
        }
        posts.AddRange(mb.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Puesto_emergencias"));
        Transform start = mb.arrival;
        var reach = start != null ? Walkable(start.position, posts.Select(p => p.position).ToList()) : null;
        var cut = new List<string>();
        for (int i = 0; i < posts.Count; i++) if (reach == null || !reach[i]) cut.Add(posts[i].name);
        TestProbe.Check("cada puesto de trabajo de la planta se alcanza andando desde el andén de abajo", posts.Count >= 10 && cut.Count == 0, cut.Count == 0 ? $"{posts.Count} puestos" : $"{cut.Count} de {posts.Count} sin camino: {string.Join(", ", cut.Distinct().Take(8))}");

        if (loadedHere) { AsyncOperation un = SceneManager.UnloadSceneAsync(s2); float ru = Time.realtimeSinceStartup; while (un != null && !un.isDone && Time.realtimeSinceStartup - ru < 30f) yield return null; }
        TestProbe.End();
    }

    /// <summary>Relleno por inundación (acotado) del suelo andable desde <paramref name="from"/>: ¿llega a menos de 2 m de cada punto?</summary>
    static bool[] Walkable(Vector3 from, List<Vector3> goals)
    {
        const float step = 1f, rise = 0.4f, maxR = 260f;
        var ok = new bool[goals.Count];
        float Ground(float x, float z, float near)
        {
            Vector3 o = new Vector3(x, near + 2.5f, z);
            foreach (RaycastHit h in Physics.RaycastAll(o, Vector3.down, 5f, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance))
                if (h.point.y <= near + rise + 0.05f) return h.point.y;
            return float.NaN;
        }
        bool Fits(Vector3 p) => !Physics.CheckCapsule(p + Vector3.up * 0.45f, p + Vector3.up * 1.5f, 0.35f, ~0, QueryTriggerInteraction.Ignore);
        float g0 = Ground(from.x, from.z, from.y);
        if (float.IsNaN(g0)) return ok;
        var seen = new Dictionary<(int, int), float> { [(0, 0)] = g0 };
        var q = new Queue<(int, int)>(); q.Enqueue((0, 0));
        int guard = 0;
        while (q.Count > 0 && guard++ < 120000)
        {
            var (i, j) = q.Dequeue(); float y = seen[(i, j)];
            Vector3 p = new Vector3(from.x + i * step, y, from.z + j * step);
            for (int k = 0; k < goals.Count; k++) if (!ok[k] && Flat(p, goals[k]) < 2f && Mathf.Abs(goals[k].y - y) < 3f) ok[k] = true;
            foreach (var (di, dj) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
            {
                var n = (i + di, j + dj);
                if (seen.ContainsKey(n) || Mathf.Abs(n.Item1) * step > maxR || Mathf.Abs(n.Item2) * step > maxR) continue;
                float gy = Ground(from.x + n.Item1 * step, from.z + n.Item2 * step, y);
                if (float.IsNaN(gy) || Mathf.Abs(gy - y) > rise || !Fits(new Vector3(from.x + n.Item1 * step, gy, from.z + n.Item2 * step))) continue;
                seen[n] = gy; q.Enqueue(n);
            }
        }
        return ok;
    }
}
