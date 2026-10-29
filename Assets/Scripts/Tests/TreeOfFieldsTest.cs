using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL ÁRBOL DE LOS CAMPOS (Berón 2026-10-29; docs/edificio-arbol.md):
/// 1. FORMA: 8 pisos inclinados 3° con la inclinación ALTERNA y cada uno girado 45° respecto al de abajo; el Huerto en el piso 5.
/// 2. RAÍZ: Ø ≥ 20 m y al menos tan ancha como la copa de alfombras; pared TRANSPARENTE como el cristal del tanque de
///    virtualización; aguanta (StructuralLimits.RootCheck) y con 30 pisos avisaría.
/// 3. PUERTAS: cada nivel (suelo, 8 pisos, 6 anillos) tiene su puerta con el campo magnético encendido; por el hueco se pasa
///    (la pared no) y el AGUA NO SALE: el hueco es aire (campana) y fuera no hay agua.
/// 4. NADAR ENTRE PISOS: un personaje de prueba va del suelo al piso 5 y del piso 5 a un anillo de nidos, nadando por la raíz
///    (y por la rama), sin teletransportes, sin dar vueltas (la métrica de la orquesta) y sin quedarse sin aire.
/// 5. GIRO: el hechizo «arbol» cambia la velocidad; a 1 vuelta por hora del juego, en 30 s gira lo que debe, un cuerpo de pie
///    en un piso sigue en su sitio del piso y el NavMesh gira con el árbol; con 0 se para.
/// Se añade solo al orquestador cuando se piden las pruebas.
/// </summary>
public class TreeOfFieldsTest : MonoBehaviour, ITestUnit
{
    public int Group => 53;   // tras la voz de Kushal (52); antes del nado (54)
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<TreeOfFieldsTest>() == null) tr.gameObject.AddComponent<TreeOfFieldsTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Árbol de los Campos: raíz de agua, pisos en acordeón, puertas, nado entre pisos y giro");
        ArbolCampos t = ArbolCampos.Instance;
        TreeRotator rot = TreeRotator.Instance;
        if (t == null || rot == null) { TestProbe.Check("hay un Árbol de los Campos (con su giro) en la escena", false); TestProbe.End(); yield break; }
        Transform tree = t.transform;
        float oldHours = rot.hoursPerTurn, oldScale = Time.timeScale;

        // ── 1. FORMA ──
        List<TreeFloor> floors = tree.GetComponentsInChildren<TreeFloor>().OrderBy(f => f.index).ToList();
        TestProbe.Check("el árbol tiene 8 pisos de cultivo", floors.Count == 8, $"{floors.Count} pisos");
        string tilts = "", twists = ""; bool tiltOk = floors.Count == 8, altOk = tiltOk, twistOk = tiltOk, upOk = tiltOk;
        float lastSign = 0f, lastYaw = 0f, lastY = float.MinValue;
        foreach (TreeFloor f in floors)
        {
            Vector3 up = tree.InverseTransformDirection(f.transform.up), fw = tree.InverseTransformDirection(f.transform.forward);
            float tilt = Vector3.Angle(up, Vector3.up), sign = Mathf.Sign(Vector3.Dot(new Vector3(up.x, 0f, up.z), new Vector3(fw.x, 0f, fw.z)));
            float yaw = TreeSpec.Angle(new Vector3(fw.x, 0f, fw.z));
            tilts += $"{sign * tilt:+0.0;-0.0}° "; twists += $"{yaw:0}° ";
            if (Mathf.Abs(tilt - TreeSpec.Tilt) > 0.3f) tiltOk = false;
            if (f.index > 1 && (sign == lastSign || Mathf.Abs(Mathf.Abs(Mathf.DeltaAngle(lastYaw, yaw)) - TreeSpec.Twist) > 1f)) { if (sign == lastSign) altOk = false; else twistOk = false; }
            if (f.transform.position.y <= lastY) upOk = false;
            lastSign = sign; lastYaw = yaw; lastY = f.transform.position.y;
        }
        TestProbe.Check("cada piso está inclinado 3°", tiltOk, tilts);
        TestProbe.Check("la inclinación alterna de un piso al siguiente (acordeón)", altOk, tilts);
        TestProbe.Check("cada piso está girado 45° respecto al de abajo", twistOk, twists);
        TestProbe.Check("los pisos van de abajo arriba (de ~6 a ~62 m)", upOk && floors.Count == 8 && floors[0].transform.position.y - tree.position.y < 8f && floors[7].transform.position.y - tree.position.y > 55f,
            floors.Count == 8 ? $"{floors[0].transform.position.y - tree.position.y:0} … {floors[7].transform.position.y - tree.position.y:0} m" : "");
        GameObject garden = GameObject.Find("Garden_Area");
        TestProbe.Check("el Huerto (con sus hortalizas) está en el piso 5 del árbol", garden != null && floors.Count == 8 && garden.transform.IsChildOf(floors[4].transform)
            && garden.GetComponentsInChildren<GardenPlot>().Any(g => g.guild == GardenPlot.Guild.Hortalizas));
        int plots = tree.GetComponentsInChildren<GardenPlot>().Length, slotsOk = floors.Count(f => f.TryGetComponent(out PlatformSlots ps) && ps.characters.Count >= PlatformSlots.MinCharacters && ps.flora.Count >= PlatformSlots.MinFlora);
        TestProbe.Check("los 8 cultivos están en el árbol, y cada piso con sus plazas reservadas del microcosmos", plots == 8 && slotsOk == 8, $"{plots} bancales, {slotsOk}/8 pisos con plazas");

        // ── 2. RAÍZ ──
        MeshFilter wmf = t.wall != null ? t.wall.GetComponent<MeshFilter>() : null;
        float rootD = wmf != null && wmf.sharedMesh != null ? Mathf.Min(wmf.sharedMesh.bounds.size.x, wmf.sharedMesh.bounds.size.z) : 0f;
        float topW = 0f;
        if (t.top != null) foreach (Renderer r in t.top.GetComponentsInChildren<Renderer>()) if (r.name == "Losa_copa" && Quaternion.Angle(r.transform.localRotation, Quaternion.identity) < 1f) topW = Mathf.Max(topW, Mathf.Max(r.transform.localScale.x, r.transform.localScale.z));
        TestProbe.Check($"la raíz mide Ø ≥ {StructuralLimits.RootMinDiameter:0} m y es al menos tan ancha como la copa de alfombras", rootD >= StructuralLimits.RootMinDiameter && rootD >= topW - 0.05f, $"raíz Ø {rootD:0.0} m, copa {topW:0.0} m");
        Renderer wr = t.wall != null ? t.wall.GetComponent<Renderer>() : null;
        Material wm = wr != null ? wr.sharedMaterial : null;
        Material tankGlass = null;
        foreach (VirtualizationTank vt in FindObjectsByType<VirtualizationTank>(FindObjectsSortMode.None))
        { Transform c = vt.transform.Find("Cristal"); if (c != null && c.TryGetComponent(out Renderer cr)) { tankGlass = cr.sharedMaterial; break; } }
        float wAlpha = wm != null ? wm.color.a : 1f, tAlpha = tankGlass != null ? tankGlass.color.a : VirtualizationTank.GlassColor.a;
        bool wTransparent = wm != null && wm.renderQueue >= 3000 && (!wm.HasProperty("_Surface") || wm.GetFloat("_Surface") > 0.5f);
        TestProbe.Check("la pared de la raíz es transparente como el cristal del tanque de virtualización (se ve nadar dentro)",
            wTransparent && wAlpha <= tAlpha + 0.05f && (tankGlass == null || (tankGlass.shader == wm.shader && tankGlass.renderQueue == wm.renderQueue)),
            wm != null ? $"cola {wm.renderQueue}, alfa {wAlpha:0.00} (tanque {tAlpha:0.00}{(tankGlass != null ? $", cola {tankGlass.renderQueue}" : ", sin tanque en escena")})" : "sin pared");
        StructuralLimits.RootReport rep = StructuralLimits.RootCheck(8, 6, TreeSpec.WaterRadius, 100f, TreeSpec.WallThickness, TreeSpec.Ribs);
        StructuralLimits.RootReport more = StructuralLimits.RootCheck(30, 6, TreeSpec.WaterRadius, 100f, TreeSpec.WallThickness, TreeSpec.Ribs);
        TestProbe.Check("la raíz aguanta 8 pisos, 6 ramas, la copa y 100 m de agua (sin aviso)", !rep.Warn && !rep.Fail, rep.ToString());
        TestProbe.Check("con 30 pisos la comprobación de la raíz avisa (habría que reforzarla)", more.Warn, more.ToString());

        // ── 3. PUERTAS: se pasa por el hueco, el agua no sale ──
        Physics.SyncTransforms();
        int doors = 0, open = 0, dry = 0; string bad = "";
        foreach (TreeLevel lv in t.levels)
        {
            if (lv.kind == TreeLevel.Kind.Top) continue;
            doors++;
            Vector3 o = t.World(lv.outside) + Vector3.up * 1.3f, i = t.World(lv.inside) + Vector3.up * 1.1f, dr = t.World(lv.door) + Vector3.up * 1.3f;
            bool field = lv.field != null && lv.field.gameObject.activeInHierarchy;
            bool through = !Physics.Linecast(o, i, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore) || !hit.transform.IsChildOf(tree);
            if (field && through) open++; else if (bad.Length < 200) bad += $"{lv.name}: campo {field}, paso {(through ? "libre" : hit.collider.name)}; ";
            bool water = WaterZone.Underwater(i) && !WaterZone.Underwater(dr) && !WaterZone.Underwater(o);
            if (water) dry++; else if (bad.Length < 200) bad += $"{lv.name}: agua dentro {WaterZone.Underwater(i)}, en la puerta {WaterZone.Underwater(dr)}, fuera {WaterZone.Underwater(o)}; ";
        }
        Vector3 aside = t.World(TreeSpec.Dir(TreeSpec.DoorAngle(3) + 25f) * 13.5f + Vector3.up * (TreeSpec.DoorBottom(3) + 1.3f)), asideIn = t.World(TreeSpec.Dir(TreeSpec.DoorAngle(3) + 25f) * 8.5f + Vector3.up * (TreeSpec.DoorBottom(3) + 1.3f));
        bool wallStops = Physics.Linecast(aside, asideIn, out RaycastHit wh, ~0, QueryTriggerInteraction.Ignore) && wh.transform.IsChildOf(tree);
        TestProbe.Check("cada nivel (suelo, 8 pisos, 6 anillos) tiene su puerta con el campo magnético, y por ella se pasa", doors == 15 && open == doors, $"{open}/{doors} puertas; {bad}");
        TestProbe.Check("fuera de las puertas, la pared de la raíz no deja pasar", wallStops, wallStops ? wh.collider.name : "nada");
        TestProbe.Check("el agua se queda dentro: el hueco de cada puerta es aire (campana) y fuera no hay agua", dry == doors, $"{dry}/{doors}");

        // ── 4. NADAR ENTRE PISOS (del suelo al piso 5 y de ahí a un anillo de nidos) ──
        GameObject body = new GameObject("Nadador_TEST");
        CapsuleCollider cap = body.AddComponent<CapsuleCollider>(); cap.height = 1.8f; cap.radius = 0.3f; cap.center = new Vector3(0f, -0.1f, 0f);
        TreeLevel ground = t.Ground, f5 = t.Floor(5), ring = t.Ring(3);
        Vector3 startL = ground.outside + TreeSpec.Dir(ground.doorAngle) * 4f;
        body.transform.position = PathMover.Of(body.transform).OnGround(t.World(startL) + Vector3.up);
        Time.timeScale = 3f;
        Transform gArea = garden != null ? garden.transform : null;
        var legs = new List<(string name, System.Func<Vector3> target, TreeLevel lv)>
        {
            ("del suelo al piso 5 (el Huerto)", () => gArea != null ? gArea.TransformPoint(new Vector3(12f, 1f, 3.5f)) : t.World(f5.outside) + Vector3.up, f5),
            ("del piso 5 al anillo de nidos 3", () => t.World(ring.outside + TreeSpec.Dir(ring.doorAngle) * 1.2f) + Vector3.up, ring),
        };
        int trips0 = TreeTrip.Trips;
        foreach (var leg in legs)
        {
            PathMover pm = PathMover.Of(body.transform); pm.Restart();
            Vector3 from = body.transform.position, last = from, lastDir = Vector3.zero;
            float walked = 0f, minBreath = 1f; int reversals = 0, jumps = 0; bool swam = false; string jumpWhy = "";
            float end = Time.time + 240f, lastT = Time.time;
            while (Time.time < end)
            {
                Vector3 to = leg.target();
                Vector3 d = to - body.transform.position;
                if (new Vector2(d.x, d.z).magnitude < 1.2f && Mathf.Abs(d.y) < 1.6f) break;
                pm.StepToward(to, 3f);
                yield return null;
                Vector3 p = body.transform.position, step = p - last;
                float dt = Time.time - lastT; lastT = Time.time;
                if (step.magnitude > Mathf.Max(1.5f, 4f * dt)) { jumps++; if (jumpWhy.Length < 120) jumpWhy += $"{step.magnitude:0.0} m; "; }
                walked += step.magnitude;
                if (step.magnitude > 0.03f)
                {
                    Vector3 dir = step.normalized;
                    if (lastDir != Vector3.zero && Vector3.Angle(lastDir, dir) > 120f) reversals++;
                    lastDir = dir;
                }
                last = p;
                if (TreeTrip.Busy(body) && t.InRootWater(p)) swam = true;
                if (body.TryGetComponent(out SwimBody sb)) minBreath = Mathf.Min(minBreath, sb.Breath01);
            }
            Vector3 left = leg.target() - body.transform.position;
            bool arrived = new Vector2(left.x, left.z).magnitude < 1.6f && Mathf.Abs(left.y) < 1.8f && t.LevelAt(body.transform.position) == leg.lv;
            float straight = Vector3.Distance(from, body.transform.position);
            var rec = new Orchestra.WalkRecord { who = body.name, label = leg.name, walked = walked, straight = straight, reversals = reversals };
            TestProbe.Check($"un personaje va {leg.name} nadando por la raíz", arrived && swam, $"llega {arrived} (a {left.magnitude:0.0} m, en {t.LevelAt(body.transform.position)?.name ?? "ningún nivel"}), nadó {swam}");
            TestProbe.Check($"{leg.name}: sin teletransportes ni saltos", jumps == 0, $"{jumps} saltos {jumpWhy}");
            TestProbe.Check($"{leg.name}: sin dar vueltas (≤ 1,6× la recta + 3 m, ≤ 2 medias vueltas)", !Orchestra.Circled(rec), $"{walked:0.0} m para {straight:0.0} en recta, {reversals} medias vueltas");
            TestProbe.Check($"{leg.name}: no se queda sin aire", minBreath > 0f, $"aliento mínimo {minBreath:0.00}");
        }
        TestProbe.Check("los viajes por la raíz se hicieron nadando (TreeTrip), sin atascos", TreeTrip.Trips - trips0 >= 2 && TreeTrip.Stuck == 0, $"{TreeTrip.Trips - trips0} viajes, {TreeTrip.Stuck} atascos");
        Destroy(body);
        Time.timeScale = oldScale;

        // ── 5. GIRO ──
        SpellConsole sc = FindAnyObjectByType<SpellConsole>();
        if (sc != null) sc.Enter("arbol({vueltaHoras:1})"); else TreeRotator.Set(1f);
        TestProbe.Check("el hechizo «arbol({vueltaHoras:1})» cambia la velocidad del giro", Mathf.Approximately(rot.hoursPerTurn, 1f), $"{rot.hoursPerTurn} h por vuelta");
        TreeFloor f3 = floors.Count >= 3 ? floors[2] : null;
        GameObject stand = new GameObject("De_pie_TEST");
        CharacterController cc = stand.AddComponent<CharacterController>(); cc.height = 1.8f; cc.radius = 0.3f; cc.center = Vector3.zero;
        Vector3 standL = new Vector3(0f, 0.95f, -17f);
        if (f3 != null) { cc.enabled = false; stand.transform.position = f3.transform.TransformPoint(standL); cc.enabled = true; }
        int nav0 = rot.NavRefreshes; float deg0 = rot.TotalDegrees, t0 = Time.time;
        while (Time.time - t0 < 30f) yield return null;
        float turned = Mathf.Abs(rot.TotalDegrees - deg0), expected = 360f * (Time.time - t0) / (60f * TimeController.timeController.TimeSpeedMinuteSecs);
        TestProbe.Check("a 1 vuelta por hora del juego, en 30 s gira lo que toca", Mathf.Abs(turned - expected) < expected * 0.1f, $"{turned:0.0}° (esperado {expected:0.0}°)");
        Vector3 nowL = f3 != null ? f3.transform.InverseTransformPoint(stand.transform.position) : Vector3.zero;
        bool onFloor = Physics.Raycast(stand.transform.position, Vector3.down, out RaycastHit fh, 2f, ~0, QueryTriggerInteraction.Ignore) && fh.transform.IsChildOf(tree);
        float drift = new Vector2(nowL.x - standL.x, nowL.z - standL.z).magnitude;
        TestProbe.Check("un cuerpo de pie en un piso gira con él: no resbala y sigue sobre el piso", f3 != null && drift < 0.3f && onFloor, $"se movió {drift:0.00} m sobre el piso; {(onFloor ? "sobre " + fh.collider.name : "sin suelo debajo")}");
        bool nav = f3 != null && UnityEngine.AI.NavMesh.SamplePosition(f3.transform.TransformPoint(new Vector3(8f, 0f, -14f)), out UnityEngine.AI.NavMeshHit nh, 1f, UnityEngine.AI.NavMesh.AllAreas)
                   && Vector3.Distance(nh.position, f3.transform.TransformPoint(new Vector3(8f, 0f, -14f))) < 0.7f;
        TestProbe.Check("el NavMesh del árbol gira con él", nav && rot.NavRefreshes > nav0, $"{rot.NavRefreshes - nav0} recolocaciones");
        Destroy(stand);
        if (sc != null) sc.Enter("arbol({vueltaHoras:0})"); else TreeRotator.Set(0f);
        float deg1 = rot.TotalDegrees; t0 = Time.time;
        while (Time.time - t0 < 2f) yield return null;
        TestProbe.Check("con «arbol({vueltaHoras:0})» el árbol se queda quieto", Mathf.Abs(rot.TotalDegrees - deg1) < 0.01f, $"{rot.TotalDegrees - deg1:0.000}°");
        rot.hoursPerTurn = oldHours;
        Time.timeScale = oldScale;
        TestProbe.End();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Árbol de los Campos (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloArbol"); DontDestroyOnLoad(go);
        TreeOfFieldsTest t = go.AddComponent<TreeOfFieldsTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(TreeOfFieldsTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
