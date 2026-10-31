using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LOS EDIFICIOS-ÁRBOL (Berón 2026-10-31; docs/edificio-arbol.md §Migración): las torres (Servicios, Entrenamiento,
/// Ciencias, Producción) migradas al modelo del Árbol de los Campos.
/// 1. FORMA: hélice (en cada nivel, pisos en dos lados opuestos y ramas en los otros dos, alternando); primer piso a ≥ 50 m y por
///    encima del árbol más alto que crece en el santuario; cada área en su piso pegado al tronco.
/// 2. PUERTAS Y ENTRADAS: cada piso, cada anillo de piso, cada punta y su anillo, el suelo y el techo con su puerta de 3 × 3 m y su
///    campo magnético; el umbral de cada puerta, libre de muebles; alfombras SOLO en el suelo y en el techo (ninguna en los pisos).
/// 3. HOLGURA: entre dos árboles (sus losas, ramas, columnas y troncos) hay ≥ 16 m en 3D; el Árbol de los Campos, girando.
/// 4. NADAR: un personaje entra por el techo (de frente), baja nadando a un piso, va a otro y sale por el suelo.
/// 5. EL ÁRBOL DE LA GUARDIA crece tan alto como el edificio más alto (la excepción a «por encima del árbol más alto»).
/// Se añade solo al orquestador cuando se piden las pruebas.
/// </summary>
public class TreeBuildingsTest : MonoBehaviour, ITestUnit
{
    public int Group => 57;   // tras la fachada de fotobiorreactores (56)
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<TreeBuildingsTest>() == null) tr.gameObject.AddComponent<TreeBuildingsTest>();
    }

    static readonly string[] Walls = { "Losa", "Mat", "Suelo", "Viga", "Zona", "Mensula", "Tornapunta", "Wall_Left", "Wall_Right", "Label" };

    public IEnumerator Run()
    {
        TestProbe.Begin("Edificios-árbol: hélice, puertas, entradas solo arriba y abajo, holgura entre árboles, nado y el árbol de la guardia");
        var all = ArbolCampos.All.Where(t => t != null).ToList();
        var towers = all.Where(t => t.building != TreeSpec.Building).ToList();
        TestProbe.Check("las torres son edificios-árbol", towers.Count > 0, string.Join(", ", towers.Select(t => t.building)));
        float minFirst = Mathf.Max(TreeSpec.FirstFloorMin, TreeSpec.TallestTree + TreeSpec.TreeClearance + TreeSpec.StrutDrop);

        foreach (ArbolCampos t in towers)
        {
            string b = t.building;
            Transform root = t.transform;
            var floors = t.levels.Where(l => l.kind == TreeLevel.Kind.Floor && !l.tip).ToList();
            var tips = t.levels.Where(l => l.kind == TreeLevel.Kind.Floor && l.tip).ToList();
            var rings = t.levels.Where(l => l.kind == TreeLevel.Kind.Ring).ToList();
            // ── 1. FORMA ──
            float lowest = floors.Count > 0 ? floors.Min(l => l.deck.position.y - root.position.y) : 0f;
            TestProbe.Check($"{b}: el primer piso a ≥ {minFirst:0} m (50 m y por encima del árbol más alto que crece)", floors.Count > 0 && lowest >= minFirst - 0.1f, $"{lowest:0.0} m, {floors.Count} pisos, {tips.Count} puntas, {rings.Count} anillos");
            bool helix = true; string why = "";
            foreach (var g in floors.GroupBy(l => l.level))
            {
                var sides = g.Select(l => l.side).OrderBy(s => s).ToArray();
                bool odd = g.Key % 2 == 1;
                if (sides.Length != 2 || sides[0] != (odd ? 0 : 1) || sides[1] != (odd ? 2 : 3)) { helix = false; why += $"nivel {g.Key}: lados {string.Join("/", sides)}; "; }
                var br = tips.Where(l => l.level == g.Key).Select(l => l.side).OrderBy(s => s).ToArray();
                if (br.Length != 2 || br[0] != (odd ? 1 : 0) || br[1] != (odd ? 3 : 2)) { helix = false; why += $"nivel {g.Key}: puntas {string.Join("/", br)}; "; }
            }
            TestProbe.Check($"{b}: en cada nivel, pisos en dos lados opuestos y ramas a las puntas en los otros dos, alternando (hélice)", helix, helix ? $"{floors.Select(l => l.level).Distinct().Count()} niveles" : why);
            var areas = root.GetComponentsInChildren<SanctuaryArea>().Where(a => floors.Any(l => l.deck == a.transform)).ToList();
            TestProbe.Check($"{b}: sus áreas, cada una en su piso pegado al tronco", areas.Count > 0 && areas.All(a => a.Contains(t.World(floors.First(l => l.deck == a.transform).outside) + Vector3.up)),
                string.Join(", ", areas.Select(a => a.displayName)));

            // ── 2. PUERTAS Y ENTRADAS ──
            int doors = 0, ok = 0; string bad = "";
            foreach (TreeLevel lv in t.levels)
            {
                doors++;
                Transform f = lv.field;
                bool big = f != null && f.GetComponent<CampoMagnetico>() != null && f.lossyScale.x >= 2.99f && f.lossyScale.y >= 2.99f;
                if (big) ok++; else if (bad.Length < 200) bad += $"{lv.name}; ";
            }
            TestProbe.Check($"{b}: cada nivel (suelo, pisos, anillos, puntas y techo) con su puerta de 3 × 3 m y su campo magnético", doors == ok && doors >= floors.Count + tips.Count * 2 + 2, $"{ok}/{doors}; {bad}");
            int blocked = 0; string blockedWhy = "";
            foreach (TreeLevel lv in floors.Concat(rings.Where(r => !r.via.Any())))
            {
                Vector3 c = t.World(lv.outside) + Vector3.up * 1.2f;
                foreach (Collider col in Physics.OverlapBox(c, new Vector3(1.2f, 0.9f, 1.2f), root.rotation, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (col.GetComponentInParent<Anima>() != null || col.GetComponent<CharacterController>() != null) continue;
                    if (Walls.Any(w => col.name.StartsWith(w))) continue;
                    blocked++; if (blockedWhy.Length < 240) blockedWhy += $"{lv.name}: {col.name}; ";
                    break;
                }
            }
            TestProbe.Check($"{b}: el umbral de cada puerta de piso y de anillo, libre de muebles", blocked == 0, blocked == 0 ? $"{floors.Count + rings.Count(r => !r.via.Any())} puertas" : blockedWhy);
            int padsOnFloors = CarpetPad.All.Count(p => p != null && p.kind != CarpetPad.Kind.Habitacion && t.levels.Any(l => l.deck != null && l.kind != TreeLevel.Kind.Top && p.transform.IsChildOf(l.deck) && !p.building.Contains("esterillas")));
            var mine = CarpetPad.All.Where(p => p != null && p.building == b).Select(p => p.floor).Distinct().OrderBy(x => x).ToList();
            TestProbe.Check($"{b}: se entra solo por el suelo y por el techo (alfombras ahí y en ningún piso)", padsOnFloors == 0 && mine.Count == 2 && mine[0] == 0 && t.Top != null && mine[1] == t.Top.index,
                $"{padsOnFloors} áreas de alfombras en pisos; pisos con alfombras: {string.Join(", ", mine)}");
        }

        // ── 3. HOLGURA ENTRE ÁRBOLES (≥ 16 m en 3D) ──
        var pieces = new Dictionary<ArbolCampos, List<Bounds>>();
        foreach (ArbolCampos t in all)
        {
            var l = new List<Bounds>();
            foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
            {
                string n = r.name;
                if (n.StartsWith("Losa_arbol") || n.StartsWith("Losa_punta") || n.StartsWith("Losa_anillo") || n.StartsWith("Losa_copa") || n == "Pared_rama" || n == "Pared_raiz" || n.StartsWith("Campana_rama"))
                    l.Add(r.bounds);
            }
            pieces[t] = l;
        }
        float worst = float.MaxValue; string worstWho = "";
        TreeRotator rot = TreeRotator.Instance;
        for (int i = 0; i < all.Count; i++)
            for (int j = i + 1; j < all.Count; j++)
            {
                ArbolCampos A = all[i], B = all[j];
                bool aRot = A.GetComponent<TreeRotator>() != null, bRot = B.GetComponent<TreeRotator>() != null;
                if (aRot && !bRot) { var x = A; A = B; B = x; aRot = false; bRot = true; }
                foreach (Bounds pa in pieces[A])
                    foreach (Bounds pb in pieces[B])
                    {
                        float dy = Mathf.Max(0f, Mathf.Max(pa.min.y - pb.max.y, pb.min.y - pa.max.y)), dp;
                        if (bRot)
                        {
                            Vector3 c = B.transform.position;
                            float R = 0f; foreach (float x in new[] { pb.min.x, pb.max.x }) foreach (float z in new[] { pb.min.z, pb.max.z }) R = Mathf.Max(R, new Vector2(x - c.x, z - c.z).magnitude);
                            Vector2 nearest = new Vector2(Mathf.Clamp(c.x, pa.min.x, pa.max.x), Mathf.Clamp(c.z, pa.min.z, pa.max.z));
                            dp = Mathf.Max(0f, (nearest - new Vector2(c.x, c.z)).magnitude - R);
                        }
                        else
                        {
                            float dx = Mathf.Max(0f, Mathf.Max(pa.min.x - pb.max.x, pb.min.x - pa.max.x)), dz = Mathf.Max(0f, Mathf.Max(pa.min.z - pb.max.z, pb.min.z - pa.max.z));
                            dp = Mathf.Sqrt(dx * dx + dz * dz);
                        }
                        float d = Mathf.Sqrt(dp * dp + dy * dy);
                        if (d < worst) { worst = d; worstWho = $"{A.building} – {B.building}"; }
                    }
            }
        TestProbe.Check("entre dos edificios-árbol hay ≥ 16 m en 3D (el Árbol de los Campos, girando)", worst >= 15.5f, $"lo más cerca: {worst:0.0} m ({worstWho})");

        // ── 4. NADAR: del techo (de frente) a un piso, a otro y al suelo ──
        float oldScale = Time.timeScale;
        foreach (ArbolCampos t in towers)
        {
            var floors = t.levels.Where(l => l.kind == TreeLevel.Kind.Floor && !l.tip).OrderBy(l => l.index).ToList();
            if (floors.Count < 2 || t.Top == null || t.Ground == null) continue;
            GameObject body = new GameObject("Nadador_arbol_TEST");
            CapsuleCollider cap = body.AddComponent<CapsuleCollider>(); cap.height = 1.8f; cap.radius = 0.3f; cap.center = new Vector3(0f, -0.1f, 0f);
            PathMover pm = PathMover.Of(body.transform);
            body.transform.position = pm.OnGround(t.World(t.Top.outside) + Vector3.up);
            Time.timeScale = 3f;
            var legs = new (TreeLevel lv, string name)[] { (floors[floors.Count - 1], "del techo al último piso"), (floors[0], "del último piso al primero"), (t.Ground, "del primer piso al suelo") };
            int okLegs = 0; string res = "";
            foreach (var (lv, name) in legs)
            {
                pm.Restart();
                Vector3 goal() => t.World(lv.outside) + Vector3.up;
                bool swam = false; Vector3 last = body.transform.position; int jumps = 0;
                float end = Time.time + 300f, endR = Time.realtimeSinceStartup + 150f, lastT = Time.time;
                while (Time.time < end && Time.realtimeSinceStartup < endR)
                {
                    Vector3 d = goal() - body.transform.position;
                    if (new Vector2(d.x, d.z).magnitude < 1.2f && Mathf.Abs(d.y) < 1.6f) break;
                    pm.StepToward(goal(), 3f);
                    yield return null;
                    Vector3 p = body.transform.position; float dt = Mathf.Max(0.02f, Time.time - lastT); lastT = Time.time;
                    if ((p - last).magnitude > Mathf.Max(1.5f, 4f * dt)) jumps++;
                    last = p;
                    if (TreeTrip.Busy(body) && t.InRootWater(p)) swam = true;
                }
                TreeLevel at = t.LevelAt(body.transform.position);
                bool arrived = at == lv;
                if (arrived && swam && jumps == 0) okLegs++;
                res += $"{name}: {(arrived ? "llega" : $"en {at?.name ?? "ningún nivel"}")}{(swam ? ", nadando" : ", sin nadar")}{(jumps > 0 ? $", {jumps} saltos" : "")}; ";
            }
            TestProbe.Check($"{t.building}: se entra por el techo, se nada de piso a piso y se sale por el suelo, sin saltos", okLegs == legs.Length, res);
            Time.timeScale = oldScale;
            Destroy(body);
            yield return null;
        }
        Time.timeScale = oldScale;

        // ── 5. EL ÁRBOL DE LA GUARDIA ──
        GameObject guard = GameObject.Find("CasaArbolGuardia");
        float tallest = all.Count > 0 ? all.Max(t => t.rootHeight) : 0f;
        float gh = 0f;
        if (guard != null) foreach (Renderer r in guard.GetComponentsInChildren<Renderer>()) if (r.name == "Tronco") gh = Mathf.Max(gh, r.bounds.max.y - guard.transform.position.y);
        TestProbe.Check("el árbol de la guardia (la excepción: un árbol modificado) crece tan alto como el edificio más alto", guard != null && gh >= tallest - 0.5f, $"tronco {gh:0.0} m, edificio más alto {tallest:0.0} m");
        TestProbe.End();
    }
}
