using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL ÁRBOL DE LOS CAMPOS v2 (Berón 2026-10-30; docs/edificio-arbol.md):
/// 1. FORMA: tronco CUADRADO; 8 plataformas CUADRADAS (4 pisos pegados al tronco y 4 campos de punta); en cada nivel de la
///    hélice, pisos en dos lados opuestos y ramas en los otros dos, ALTERNANDO de un nivel al siguiente; nada tapa a nada en
///    planta; el primer piso por encima del árbol más alto del santuario (medido en la escena); el Huerto en el piso 5; cada
///    plataforma con sus plazas del microcosmos y sus 2 m de paso.
/// 2. TRONCO: ≥ 20 m y tan ancho como el techo de alfombras; transparente como el tanque; aguanta (StructuralLimits.RootCheck)
///    y con 30 niveles avisaría.
/// 3. PUERTAS: suelo, 4 pisos, 4 campos y 4 anillos con su campo magnético; se pasa por el hueco y el agua no sale; la
///    trampilla del techo, abierta y con el agua a una altura de trepar.
/// 4. NADAR: un personaje va del suelo a cada plataforma, a un anillo y de vuelta, al techo y del techo abajo, sin teletransportes,
///    sin dar vueltas y sin quedarse sin aire.
/// 5. SALIR (el fallo de Berón): un cuerpo con PlayerController sale nadando de la columna de una rama al campo y al anillo, y
///    por la trampilla al techo; y entra andando desde el campo.
/// 6. GIRO: el hechizo «arbol», un cuerpo de pie gira con su piso y el NavMesh también.
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
        TestProbe.Begin("Árbol de los Campos v2: tronco cuadrado, hélice de pisos y ramas, puertas, nado, salidas y giro");
        ArbolCampos t = ArbolCampos.Instance;
        TreeRotator rot = TreeRotator.Instance;
        if (t == null || rot == null) { TestProbe.Check("hay un Árbol de los Campos (con su giro) en la escena", false); TestProbe.End(); yield break; }
        Transform tree = t.transform;
        HelixTower h = TreeSpec.Helix;
        float oldHours = rot.hoursPerTurn, oldScale = Time.timeScale;
        rot.hoursPerTurn = 0f;   // quieto mientras se mide y se nada (el giro se prueba al final)

        // ── 1. FORMA ──
        List<TreeFloor> plats = tree.GetComponentsInChildren<TreeFloor>().OrderBy(f => f.index).ToList();
        TestProbe.Check("el árbol tiene 8 plataformas de cultivo (4 pisos pegados al tronco y 4 campos de punta)", plats.Count == 8, $"{plats.Count}");
        // Cuadradas: la losa (sus piezas) mide lo mismo de ancho que de largo, en el marco de la plataforma.
        string sq = ""; bool squares = plats.Count == 8;
        var planOf = new Dictionary<TreeFloor, Rect>();
        foreach (TreeFloor f in plats)
        {
            Bounds lb = new Bounds(); bool any = false;
            foreach (Transform c in f.GetComponentsInChildren<Transform>())
                if (c.name == "Losa_arbol" || c.name == "Losa_punta")
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 p = f.transform.InverseTransformPoint(c.TransformPoint(new Vector3((i & 1) - 0.5f, ((i >> 1) & 1) - 0.5f, ((i >> 2) & 1) - 0.5f)));
                        if (!any) { lb = new Bounds(p, Vector3.zero); any = true; } else lb.Encapsulate(p);
                    }
                }
            Vector3 c0 = tree.InverseTransformPoint(f.transform.position);
            planOf[f] = new Rect(c0.x - lb.extents.x, c0.z - lb.extents.z, 2f * lb.extents.x, 2f * lb.extents.z);   // (giros de 90°: el rectángulo vale en el árbol)
            bool ok = any && Mathf.Abs(lb.size.x - lb.size.z) < 0.1f && lb.size.x > 15f;
            if (!ok) squares = false;
            sq += $"P{f.index} {lb.size.x:0.#}×{lb.size.z:0.#} ";
        }
        TestProbe.Check("las 8 plataformas son cuadradas (32 m pegadas al tronco, 16 m en las puntas)", squares, sq);
        // Lados alternos: por nivel, pisos en 2 lados opuestos y ramas (campos de punta) en los otros 2; al subir, al revés.
        var floorsLv = t.levels.Where(l => l.kind == TreeLevel.Kind.Floor).ToList();
        string sides = ""; bool alt = floorsLv.Count == 8;
        string Side(int s) => "abcd"[Mathf.Clamp(s, 0, 3)].ToString();
        for (int lv = 1; lv <= h.levels; lv++)
        {
            var fl = floorsLv.Where(l => l.level == lv && !l.tip).Select(l => l.side).OrderBy(s => s).ToList();
            var tp = floorsLv.Where(l => l.level == lv && l.tip).Select(l => l.side).OrderBy(s => s).ToList();
            sides += $"nivel {lv}: pisos {string.Join("", fl.Select(Side))}, ramas {string.Join("", tp.Select(Side))}; ";
            bool even = lv % 2 == 0;
            if (fl.Count != 2 || tp.Count != 2 || fl.Any(s => (s % 2 == 0) == even) || tp.Any(s => (s % 2 == 0) != even)) alt = false;
            // y de verdad están en ese lado: el piso, delante de su cara (y corrido a la derecha); la punta, en la esquina libre.
            foreach (TreeLevel l in floorsLv.Where(l => l.level == lv))
            {
                Vector3 c = Quaternion.Inverse(HelixTower.SideRot(l.side)) * tree.InverseTransformPoint(l.deck.position);
                Vector3 want = l.tip ? h.TipCenterS : h.FloorCenterS;
                if (new Vector2(c.x - want.x, c.z - want.z).magnitude > 0.2f) { alt = false; sides += $"(P{l.index} fuera de su sitio) "; }
            }
        }
        TestProbe.Check("en cada nivel, 2 pisos en lados opuestos y 2 ramas en los otros dos; el nivel siguiente, al revés (hélice)", alt, sides);
        // Nada tapa a nada: ninguna plataforma (ni anillo) encima de otra en planta (salvo cada anillo sobre su propio campo).
        var rects = plats.Select(f => (name: $"P{f.index}", r: planOf[f], y: f.transform.position.y, own: f.transform)).ToList();
        foreach (TreeLevel rl in t.levels.Where(l => l.kind == TreeLevel.Kind.Ring))
        {
            Vector3 c = tree.InverseTransformPoint(rl.deck.position); float rh = rl.half;
            rects.Add(($"anillo{rl.index}", new Rect(c.x - rh, c.z - rh, 2f * rh, 2f * rh), rl.deck.position.y, rl.deck.parent));
        }
        string cover = ""; int covers = 0;
        for (int i = 0; i < rects.Count; i++)
            for (int j = i + 1; j < rects.Count; j++)
            {
                if (rects[i].own == rects[j].own) continue;   // el anillo sobre su propio campo (20 % encima: así es un anillo)
                float o = HelixTower.Overlap(rects[i].r, rects[j].r);
                if (o > 0.01f) { covers++; if (cover.Length < 200) cover += $"{rects[i].name}/{rects[j].name} {o:0.0} m²; "; }
            }
        TestProbe.Check("ningún piso, campo o anillo tapa a otro en planta (molinete: lados alternos)", covers == 0, covers == 0 ? $"{rects.Count} piezas sin solaparse" : cover);
        // El primer piso, por encima del árbol más alto del santuario (medido en la escena: los árboles de hielo que crecen).
        float tallest = 0f; string tallestName = "", guard = "";
        foreach (Transform tr in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            string n = tr.name;
            bool growing = n.StartsWith("ArbolCarnivoroHielo_") || n.StartsWith("ArbolHielo_") || n.StartsWith("ArbolResidencial_");
            if (!growing && n != "CasaArbolGuardia") continue;
            float top = 0f;
            foreach (Renderer r in tr.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y - tr.position.y);
            if (n == "CasaArbolGuardia") { guard = $"; el de la guardia (único, fijo en su isla): {top:0.0} m"; continue; }
            if (top > tallest) { tallest = top; tallestName = n; }
        }
        float lowest = float.MaxValue;
        foreach (Renderer r in tree.GetComponentsInChildren<Renderer>())
        {
            Transform p = r.transform; bool helix = false;
            for (Transform q = p; q != null && q != tree; q = q.parent)
                if (q.GetComponent<TreeFloor>() != null || q.name.StartsWith("Rama_") || q.name.StartsWith("Campana_rama_")) { helix = true; break; }
            if (helix) lowest = Mathf.Min(lowest, r.bounds.min.y - tree.position.y);
        }
        float first = plats.Count > 0 ? plats.Min(f => f.transform.position.y) - tree.position.y : 0f;
        TestProbe.Check("el primer piso está a ≥ 50 m y todo el árbol de pisos pasa por encima del árbol más alto del santuario (el suelo queda para los árboles)",
            first >= TreeSpec.FirstFloorMin - 0.01f && lowest > tallest + 2f && TreeSpec.TallestTree >= tallest - 0.5f,
            $"primer piso {first:0.0} m, lo más bajo (tornapuntas, ramas) {lowest:0.0} m; el árbol más alto que crece: {tallest:0.0} m ({tallestName}; previsto {TreeSpec.TallestTree:0.0}){guard}");
        GameObject garden = GameObject.Find("Garden_Area");
        TreeFloor p5 = plats.Find(f => f.index == 5);
        TestProbe.Check("el Huerto (con sus hortalizas) está en el piso 5 del árbol, pegado al tronco", garden != null && p5 != null && garden.transform.IsChildOf(p5.transform)
            && garden.GetComponentsInChildren<GardenPlot>().Any(g => g.guild == GardenPlot.Guild.Hortalizas) && !t.Floor(5).tip);
        int plots = tree.GetComponentsInChildren<GardenPlot>().Length, slotsOk = plats.Count(f => f.TryGetComponent(out PlatformSlots ps) && ps.characters.Count >= PlatformSlots.MinCharacters && ps.flora.Count >= PlatformSlots.MinFlora && ps.story.Count >= 1 && ps.fauna.Count >= PlatformSlots.MinFauna);
        TestProbe.Check("los 8 cultivos están en el árbol, y cada plataforma con sus plazas reservadas del microcosmos", plots == 8 && slotsOk == 8, $"{plots} bancales, {slotsOk}/8 plataformas con plazas");
        int viol = 0; string vs = "";
        foreach (TreeFloor f in plats)
        {
            var badSp = FurnitureSpacing.Violations(f.transform);
            viol += badSp.Count;
            foreach (var b in badSp.Take(2)) if (vs.Length < 200) vs += $"P{f.index}: {b.a.name}↔{b.b.name} {b.gap:0.0}/{b.need:0.0} m; ";
        }
        TestProbe.Check("2 m de paso en cada plataforma del árbol (regla 2)", viol == 0, viol == 0 ? "0 violaciones" : vs);
        float reach = 0f;
        foreach (Renderer r in tree.GetComponentsInChildren<Renderer>())
        {
            if (r.GetComponentInParent<CarpetPad>() != null || r.GetComponentInParent<WorldLabel>() != null) continue;
            Bounds b = r.localBounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 c = tree.InverseTransformPoint(r.transform.TransformPoint(b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f))));
                reach = Mathf.Max(reach, new Vector2(c.x, c.z).magnitude);
            }
        }
        TestProbe.Check("el alcance del árbol (lo que barre al girar) no pasa del previsto", reach <= h.Reach + 0.6f, $"{reach:0.0} m (previsto {h.Reach:0.0} m)");

        // ── 2. TRONCO ──
        MeshFilter wmf = t.wall != null ? t.wall.GetComponent<MeshFilter>() : null;
        Mesh wmesh = wmf != null ? wmf.sharedMesh : null;
        Vector3 wsz = wmesh != null ? wmesh.bounds.size : Vector3.zero;
        int offSquare = 0;
        if (wmesh != null) foreach (Vector3 v in wmesh.vertices) { float m = Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.z)); if (Mathf.Abs(m - TreeSpec.WaterHalf) > 0.02f && Mathf.Abs(m - TreeSpec.OuterHalf) > 0.02f) offSquare++; }
        float topW = 0f;
        if (t.top != null) foreach (Transform c in t.top.GetComponentsInChildren<Transform>()) if (c.name == "Losa_copa") topW = Mathf.Max(topW, Mathf.Max(c.localScale.x, c.localScale.z));
        TestProbe.Check($"el tronco es CUADRADO, de ≥ {StructuralLimits.RootMinDiameter:0} m y tan ancho como el techo de alfombras",
            wmesh != null && Mathf.Abs(wsz.x - wsz.z) < 0.05f && wsz.x >= StructuralLimits.RootMinDiameter && wsz.x >= topW - 0.05f && offSquare == 0,
            $"{wsz.x:0.0} × {wsz.z:0.0} m, techo {topW:0.0} m, {offSquare} vértices fuera del cuadrado");
        Renderer wr = t.wall != null ? t.wall.GetComponent<Renderer>() : null;
        Material wm = wr != null ? wr.sharedMaterial : null;
        Material tankGlass = null;
        foreach (VirtualizationTank vt in FindObjectsByType<VirtualizationTank>(FindObjectsSortMode.None))
        { Transform c = vt.transform.Find("Cristal"); if (c != null && c.TryGetComponent(out Renderer cr)) { tankGlass = cr.sharedMaterial; break; } }
        float wAlpha = wm != null ? wm.color.a : 1f, tAlpha = tankGlass != null ? tankGlass.color.a : VirtualizationTank.GlassColor.a;
        bool wTransparent = wm != null && wm.renderQueue >= 3000 && (!wm.HasProperty("_Surface") || wm.GetFloat("_Surface") > 0.5f);
        TestProbe.Check("la pared del tronco es transparente como el cristal del tanque de virtualización (se ve nadar dentro)",
            wTransparent && wAlpha <= tAlpha + 0.05f && (tankGlass == null || (tankGlass.shader == wm.shader && tankGlass.renderQueue == wm.renderQueue)),
            wm != null ? $"cola {wm.renderQueue}, alfa {wAlpha:0.00} (tanque {tAlpha:0.00})" : "sin pared");
        StructuralLimits.RootReport rep = StructuralLimits.RootCheck(h, 4, 4, TreeSpec.RootHeight, TreeSpec.Ribs, TreeSpec.HoopSpacing, TreeSpec.HoopArea, TreeSpec.StrutDrop);
        StructuralLimits.RootReport more = StructuralLimits.RootCheck(h, 60, 60, TreeSpec.RootHeight, TreeSpec.Ribs, TreeSpec.HoopSpacing, TreeSpec.HoopArea, TreeSpec.StrutDrop);
        TestProbe.Check("el tronco aguanta los 4 pisos en voladizo, las 4 ramas con sus puntas, el techo y 92 m de agua (bajo el 80 %)", !rep.Warn && !rep.Fail, rep.ToString());
        TestProbe.Check("con 30 niveles de hélice la comprobación del tronco avisa (habría que reforzarlo)", more.Warn, more.ToString());

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
            if (field && through) open++; else if (bad.Length < 240) bad += $"{lv.name}: campo {field}, paso {(through ? "libre" : hit.collider.name)}; ";
            bool water = WaterZone.Underwater(i) && !WaterZone.Underwater(dr) && !WaterZone.Underwater(o);
            if (water) dry++; else if (bad.Length < 240) bad += $"{lv.name}: agua dentro {WaterZone.Underwater(i)}, en la puerta {WaterZone.Underwater(dr)}, fuera {WaterZone.Underwater(o)}; ";
        }
        TestProbe.Check("cada nivel (suelo, 4 pisos, 4 campos de punta y 4 anillos) tiene su puerta con el campo magnético, y por ella se pasa", doors == 13 && open == doors, $"{open}/{doors} puertas; {bad}");
        TestProbe.Check("el agua se queda dentro: el hueco de cada puerta es aire (campana) y fuera no hay agua", dry == doors, $"{dry}/{doors}");
        TreeLevel g0 = t.Ground;
        Vector3 aside = t.World(g0.outside + HelixTower.FromSide(g0.side, Vector3.left) * 4f) + Vector3.up * 1.3f, asideIn = t.World(g0.inside + HelixTower.FromSide(g0.side, Vector3.left) * 4f) + Vector3.up * 1.1f;
        bool wallStops = Physics.Linecast(aside, asideIn, out RaycastHit wh, ~0, QueryTriggerInteraction.Ignore) && wh.transform.IsChildOf(tree);
        TestProbe.Check("fuera de las puertas, la pared del tronco no deja pasar", wallStops, wallStops ? wh.collider.name : "nada");
        TreeLevel topL = t.Top;
        Vector3 hatchAbove = t.World(TreeSpec.HatchLocal) + Vector3.up * 2f;
        RaycastHit hh2 = default; bool hatchOpen = topL != null && !Physics.Raycast(hatchAbove, Vector3.down, out hh2, 5f, ~0, QueryTriggerInteraction.Ignore);
        float sink = WaterZone.SurfaceAt(t.World(TreeSpec.HatchLocal) + Vector3.down * 1f, out float surf) ? tree.position.y + TreeSpec.RootHeight - surf : -1f;
        TestProbe.Check("ENTRADA DEL TECHO: la trampilla está abierta sobre el agua, con su campo, y el agua 0,3–0,75 m bajo la cubierta (se trepa)",
            hatchOpen && topL.field != null && sink >= 0.3f && sink <= 0.75f, $"abierta {hatchOpen}{(hatchOpen ? "" : $" (choca con {(hh2.collider != null ? hh2.collider.name : "?")})")}, agua {sink:0.00} m bajo el techo");

        // ── 4. NADAR ENTRE NIVELES ──
        GameObject body = new GameObject("Nadador_TEST");
        CapsuleCollider cap = body.AddComponent<CapsuleCollider>(); cap.height = 1.8f; cap.radius = 0.3f; cap.center = new Vector3(0f, -0.1f, 0f);
        body.transform.position = PathMover.Of(body.transform).OnGround(t.World(g0.outside + TreeSpec.Dir(g0.doorAngle) * 4f) + Vector3.up);
        Time.timeScale = 3f;
        Transform gArea = garden != null ? garden.transform : null;
        System.Func<Vector3> At(TreeLevel lv) => () => t.World(lv.outside + TreeSpec.Dir(lv.doorAngle) * 1.2f) + Vector3.up;
        var legs = new List<(string name, System.Func<Vector3> target, TreeLevel lv)>
        {
            ("del suelo al piso 5 (el Huerto)", () => gArea != null ? gArea.TransformPoint(new Vector3(8f, 1f, 3.5f)) : t.World(t.Floor(5).outside) + Vector3.up, t.Floor(5)),
            ("del piso 5 al piso 1", At(t.Floor(1)), t.Floor(1)),
            ("del piso 1 al piso 2", At(t.Floor(2)), t.Floor(2)),
            ("del piso 2 al campo de punta 3 (por su rama)", At(t.Floor(3)), t.Floor(3)),
            ("del campo 3 a su anillo", At(t.Ring(1)), t.Ring(1)),
            ("del anillo 1 a su campo", At(t.Floor(3)), t.Floor(3)),
            ("del campo 3 al campo 4 (de rama a rama)", At(t.Floor(4)), t.Floor(4)),
            ("del campo 4 al piso 6", At(t.Floor(6)), t.Floor(6)),
            ("del piso 6 al campo 7", At(t.Floor(7)), t.Floor(7)),
            ("del campo 7 al campo 8", At(t.Floor(8)), t.Floor(8)),
            ("del campo 8 al techo (por la trampilla)", At(topL), topL),
            ("del techo al campo 3 (entrando por arriba)", At(t.Floor(3)), t.Floor(3)),
            ("del campo 3 al suelo (saliendo de la rama)", () => t.World(g0.outside + TreeSpec.Dir(g0.doorAngle) * 1.5f) + Vector3.up, g0),
        };
        int trips0 = TreeTrip.Trips, stuck0 = TreeTrip.Stuck, legsOk = 0, jumpsAll = 0, circled = 0; float minBreathAll = 1f;
        string jumpsWhy = "", circleWhy = "";
        TreeTrip.Verbose = true;
        foreach (var leg in legs)
        {
            if (leg.lv == null) { TestProbe.Check($"un personaje va {leg.name}", false, "falta el nivel"); continue; }
            PathMover pm = PathMover.Of(body.transform); pm.Restart();
            Vector3 from = body.transform.position, last = from, lastDir = Vector3.zero;
            float walked = 0f; int reversals = 0; bool swam = false;
            float end = Time.time + 240f, endReal = Time.realtimeSinceStartup + 150f, lastT = Time.time, prevDt = 0f;
            while (Time.time < end && Time.realtimeSinceStartup < endReal)
            {
                Vector3 to = leg.target();
                Vector3 d = to - body.transform.position;
                if (new Vector2(d.x, d.z).magnitude < 1.2f && Mathf.Abs(d.y) < 1.6f) break;
                int st0 = pm.Steps; Vector3 pre = body.transform.position;
                pm.StepToward(to, 3f);
                float mine = Vector3.Distance(pre, body.transform.position);
                yield return null;
                Vector3 p = body.transform.position, step = p - last;
                float dt = Time.time - lastT; lastT = Time.time;
                float span = Mathf.Max(dt, prevDt); prevDt = dt;
                if (step.magnitude > Mathf.Max(1.5f, 4f * span)) { jumpsAll++; if (jumpsWhy.Length < 240) jumpsWhy += $"{leg.name}: {step.magnitude:0.0} m en {dt:0.00} s{(TreeTrip.Busy(body) ? " nadando" : "")} [paso propio {mine:0.0} m, {pm.Steps - st0} pasos]; "; }
                walked += step.magnitude;
                if (step.magnitude > 0.03f)
                {
                    Vector3 dir = step.normalized;
                    if (lastDir != Vector3.zero && Vector3.Angle(lastDir, dir) > 120f) reversals++;
                    lastDir = dir;
                }
                last = p;
                if (TreeTrip.Busy(body) && t.InRootWater(p)) swam = true;
                if (body.TryGetComponent(out SwimBody sb)) minBreathAll = Mathf.Min(minBreathAll, sb.Breath01);
            }
            Vector3 left = leg.target() - body.transform.position;
            TreeLevel at = t.LevelAt(body.transform.position);
            bool arrived = new Vector2(left.x, left.z).magnitude < 1.6f && Mathf.Abs(left.y) < 1.8f && at == leg.lv;
            float straight = Vector3.Distance(from, body.transform.position);
            var rec = new Orchestra.WalkRecord { who = body.name, label = leg.name, walked = walked, straight = straight, reversals = reversals };
            if (Orchestra.Circled(rec)) { circled++; if (circleWhy.Length < 300) circleWhy += $"{leg.name}: {walked:0} m para {straight:0} en recta, {reversals} medias vueltas; "; }
            if (arrived && swam) legsOk++;
            TestProbe.Check($"un personaje va {leg.name} nadando", arrived && swam, $"llega {arrived} (a {left.magnitude:0.0} m, en {at?.name ?? "ningún nivel"}), nadó {swam}, {walked:0} m");
        }
        TestProbe.Check("ningún viaje teletransporta ni da saltos", jumpsAll == 0, $"{jumpsAll} saltos {jumpsWhy}");
        TestProbe.Check("ningún viaje da vueltas (≤ 1,6× la recta + 3 m, ≤ 2 medias vueltas)", circled == 0, circled == 0 ? $"{legs.Count} viajes" : circleWhy);
        TestProbe.Check("nadie se queda sin aire en el tronco ni en las ramas", minBreathAll > 0f, $"aliento mínimo {minBreathAll:0.00}");
        TestProbe.Check("los viajes se hicieron nadando (TreeTrip), sin atascos", TreeTrip.Trips - trips0 >= legs.Count && TreeTrip.Stuck == stuck0, $"{TreeTrip.Trips - trips0} viajes, {TreeTrip.Stuck - stuck0} atascos");
        Destroy(body); TreeTrip.Verbose = false;
        Time.timeScale = oldScale;

        // ── 5. SALIR DEL AGUA (y entrar) COMO EL JUGADOR: por las puertas de la rama y por la trampilla ──
        GameObject swimmer = new GameObject("Jugador_TEST");
        CharacterController scc = swimmer.AddComponent<CharacterController>(); scc.height = 1.8f; scc.radius = 0.35f; scc.center = Vector3.zero;
        PlayerController spc = swimmer.AddComponent<PlayerController>();
        yield return null; yield return null;
        IEnumerator Try(string what, Vector3 at, Vector3 face, Vector3 input, System.Func<bool> done, float seconds)
        {
            scc.enabled = false; swimmer.transform.SetPositionAndRotation(at, Quaternion.LookRotation(face)); scc.enabled = true;
            Physics.SyncTransforms();
            spc.scriptedInput = input;
            float t0 = Time.time, r0 = Time.realtimeSinceStartup;
            while (Time.time - t0 < seconds && Time.realtimeSinceStartup - r0 < seconds * 3f + 2f && !done()) yield return null;
            spc.scriptedInput = Vector3.zero;
        }
        TreeLevel f3 = t.Floor(3), r1 = t.Ring(1);
        int exits0 = TreeDoorExit.Exits;
        // a) Nadando en la columna de la rama, hacia la puerta del campo: sale de pie al campo.
        Vector3 fOut = t.World(f3.outside - f3.inside); fOut.y = 0f;
        yield return Try("campo", t.World(f3.inside) + Vector3.up * 1.2f, fOut, new Vector3(0f, 0f, 1f), () => !spc.IsSwimming && t.LevelAt(swimmer.transform.position) == f3, 6f);
        TreeLevel atA = t.LevelAt(swimmer.transform.position);
        TestProbe.Check("el jugador, nadando por la rama, sale por la puerta al campo de punta (el fallo de Berón)", !spc.IsSwimming && atA == f3, $"en {atA?.name ?? "ningún nivel"}, nadando {spc.IsSwimming}");
        // b) …y vuelve a entrar andando hacia la puerta: al agua.
        Vector3 fIn = -fOut;
        yield return Try("entrar", t.World(f3.outside) + Vector3.up * 1.0f, fIn, new Vector3(0f, 0f, 1f), () => spc.IsSwimming, 6f);
        TestProbe.Check("desde el campo, andando hacia la puerta, se entra al agua de la rama", spc.IsSwimming, $"nadando {spc.IsSwimming}, en {swimmer.transform.position}");
        // c) Más arriba en la columna, hacia la puerta del anillo: sale al anillo.
        Vector3 rOut = t.World(r1.outside - r1.inside); rOut.y = 0f;
        yield return Try("anillo", t.World(r1.inside) + Vector3.up * 1.2f, rOut, new Vector3(0f, 0f, 1f), () => !spc.IsSwimming && t.LevelAt(swimmer.transform.position) == r1, 6f);
        TreeLevel atC = t.LevelAt(swimmer.transform.position);
        TestProbe.Check("el jugador sale de la columna de la rama al anillo de encima del campo", !spc.IsSwimming && atC == r1, $"en {atC?.name ?? "ningún nivel"}");
        // d) En el tronco, bajo la trampilla: sube y sale al techo.
        Vector3 under = t.World(TreeSpec.HatchLocal + Vector3.down * 1.4f);
        yield return Try("techo", under, tree.TransformDirection(Vector3.forward), new Vector3(0f, 1f, 1f), () => !spc.IsSwimming && t.LevelAt(swimmer.transform.position) == topL, 8f);
        TreeLevel atD = t.LevelAt(swimmer.transform.position);
        TestProbe.Check("ENTRADA DEL TECHO: el jugador sube nadando por el tronco y sale por la trampilla al techo", !spc.IsSwimming && atD == topL, $"en {atD?.name ?? "ningún nivel"}");
        // e) Por la puerta de un piso del tronco (la del piso 1).
        TreeLevel f1 = t.Floor(1);
        Vector3 pOut = t.World(f1.outside - f1.inside); pOut.y = 0f;
        yield return Try("piso", t.World(f1.inside) + Vector3.up * 1.2f, pOut, new Vector3(0f, 0f, 1f), () => !spc.IsSwimming && t.LevelAt(swimmer.transform.position) == f1, 6f);
        TreeLevel atE = t.LevelAt(swimmer.transform.position);
        TestProbe.Check("el jugador sale del tronco por la puerta de un piso", !spc.IsSwimming && atE == f1, $"en {atE?.name ?? "ningún nivel"}; {TreeDoorExit.Exits - exits0} salidas por las puertas");
        Destroy(swimmer);

        // ── 6. GIRO ──
        rot.hoursPerTurn = oldHours;
        SpellConsole sc = FindAnyObjectByType<SpellConsole>();
        if (sc != null) sc.Enter("arbol({vueltaHoras:1})"); else TreeRotator.Set(1f);
        TestProbe.Check("el hechizo «arbol({vueltaHoras:1})» cambia la velocidad del giro", Mathf.Approximately(rot.hoursPerTurn, 1f), $"{rot.hoursPerTurn} h por vuelta");
        TreeFloor fl1 = plats.Count > 0 ? plats[0] : null;
        GameObject stand = new GameObject("De_pie_TEST");
        CharacterController cc = stand.AddComponent<CharacterController>(); cc.height = 1.8f; cc.radius = 0.3f; cc.center = Vector3.zero;
        Vector3 standL = new Vector3(0f, 0.95f, -10f);
        if (fl1 != null) { cc.enabled = false; stand.transform.position = fl1.transform.TransformPoint(standL); cc.enabled = true; }
        { float w0 = Time.time; while (Time.time - w0 < 0.5f) yield return null; }   // (el giro busca los cuerpos nuevos cada 0,2 s)
        if (fl1 != null) standL = fl1.transform.InverseTransformPoint(stand.transform.position);
        int nav0 = rot.NavRefreshes, car0 = rot.Carried; float deg0 = rot.TotalDegrees, t0g = Time.time;
        { float r0 = Time.realtimeSinceStartup; while (Time.time - t0g < 30f && Time.realtimeSinceStartup - r0 < 60f) yield return null; }
        float turned = Mathf.Abs(rot.TotalDegrees - deg0), expected = 360f * (Time.time - t0g) / (60f * TimeController.timeController.TimeSpeedMinuteSecs);
        TestProbe.Check("a 1 vuelta por hora del juego, en 30 s gira lo que toca", Mathf.Abs(turned - expected) < expected * 0.1f, $"{turned:0.0}° (esperado {expected:0.0}°)");
        Vector3 nowL = fl1 != null ? fl1.transform.InverseTransformPoint(stand.transform.position) : Vector3.zero;
        bool onFloor = Physics.Raycast(stand.transform.position, Vector3.down, out RaycastHit fh, 2f, ~0, QueryTriggerInteraction.Ignore) && fh.transform.IsChildOf(tree);
        float drift = new Vector2(nowL.x - standL.x, nowL.z - standL.z).magnitude;
        TestProbe.Check("un cuerpo de pie en un piso gira con él (lo lleva): no resbala y sigue sobre el piso", fl1 != null && drift < 0.3f && onFloor && rot.Carried > car0,
            $"se movió {drift:0.00} m sobre el piso; {(onFloor ? "sobre " + fh.collider.name : "sin suelo debajo")}; {rot.Carried - car0} arrastres");
        Vector3 navL = new Vector3(8f, 0f, -12f);
        bool nav = fl1 != null && UnityEngine.AI.NavMesh.SamplePosition(fl1.transform.TransformPoint(navL), out UnityEngine.AI.NavMeshHit nh, 1f, UnityEngine.AI.NavMesh.AllAreas)
                   && Vector3.Distance(nh.position, fl1.transform.TransformPoint(navL)) < 0.7f;
        TestProbe.Check("el NavMesh del árbol gira con él", nav && rot.NavRefreshes > nav0, $"{rot.NavRefreshes - nav0} recolocaciones");
        Destroy(stand);
        if (sc != null) sc.Enter("arbol({vueltaHoras:0})"); else TreeRotator.Set(0f);
        float deg1 = rot.TotalDegrees; float t1 = Time.time;
        { float r0 = Time.realtimeSinceStartup; while (Time.time - t1 < 2f && Time.realtimeSinceStartup - r0 < 10f) yield return null; }
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
