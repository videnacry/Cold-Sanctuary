using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LOS GRUPOS DE ANIMALES (Berón 2026-11-03; docs/grupos-de-animales.md): peces, kril, medusas y abejas como un PADRE con
/// sus individuos.
/// 1. FORMA: el padre no es una plataforma (sin malla, escala 1, trigger del tamaño del grupo) y sus hijos son peces, kril, medusas o
///    abejas de verdad (cuerpo, aletas o patas, umbrela y tentáculos, alas), de su talla.
/// 2. MOVIMIENTO: al moverse el grupo, todos sus individuos van con él.
/// 3. DEPREDADOR: un animal que come peces choca con el banco y cada mordisco quita UN individuo, al momento.
/// 4. SIN CICLO DE VIDA: el grupo se rehace solo con el tiempo.
/// 5. MEDUSAS: hay grupos de Diplulmaris antarctica y Desmonema glaciale, que laten.
/// 6. EN LOS ÁRBOLES: un grupo marino sube al tronco de un edificio-árbol por su fondo abierto.
/// 7. ABEJAS: el enjambre de una colmena vuela de día de árbol en árbol (varios árboles distintos), visible como grupo.
/// </summary>
public class GroupAnimalsTest : MonoBehaviour, ITestUnit
{
    public int Group => 71;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<GroupAnimalsTest>() == null) tr.gameObject.AddComponent<GroupAnimalsTest>();
    }

    static IEnumerator Wait(float s) { float t0 = Time.time, r0 = Time.realtimeSinceStartup; while (Time.time - t0 < s && Time.realtimeSinceStartup - r0 < s * 3f + 2f) yield return null; }

    static bool Parts(Swarm s, params string[] parts)
    {
        if (s.Visible == 0) return false;
        foreach (Transform c in s.transform)
        {
            if (c.GetComponent<Collider>() != null) return false;
            foreach (string p in parts) if (c.Find(p) == null) return false;
            return true;
        }
        return false;
    }

    static float Length(Swarm s)
    {
        foreach (Transform c in s.transform)
        {
            Renderer[] rs = c.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) continue;
            Bounds b = rs[0].bounds; foreach (Renderer r in rs) b.Encapsulate(r.bounds);
            return Mathf.Max(b.size.x, b.size.y, b.size.z);
        }
        return 0f;
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Grupos de animales: peces, kril, medusas y abejas como un padre con sus individuos");
        Swarm.AnimateAll = true;   // (lejos de la cámara no se animan: aquí se miden todos)
        yield return Wait(1f);   // (los grupos crean sus individuos en su Start)
        Swarm.All.RemoveAll(s => s == null);
        var fish = Swarm.All.Where(s => s.kind == SwarmKind.Fish).ToList();
        var krill = Swarm.All.Where(s => s.kind == SwarmKind.Krill).ToList();
        var jelly = Swarm.All.Where(s => s.kind == SwarmKind.Jellyfish).ToList();
        var bees = Swarm.All.OfType<BeeSwarm>().ToList();

        // ── 1. FORMA ──
        string notPlatform = "";
        foreach (Swarm s in Swarm.All)
        {
            bool mesh = s.TryGetComponent(out MeshRenderer mr) && mr.enabled;
            bool scale = (s.transform.localScale - Vector3.one).sqrMagnitude > 1e-4f;
            float rad = s.TryGetComponent(out SphereCollider sc) ? sc.radius * s.transform.lossyScale.x : 0f;
            if ((mesh || scale || rad > 10f) && notPlatform.Length < 200) notPlatform += $"{s.name}: malla {mesh}, escala {s.transform.localScale}, trigger {rad:0} m; ";
        }
        TestProbe.Check("ningún grupo es una «plataforma»: el padre no tiene malla, va a escala 1 y su trigger es del tamaño del grupo", notPlatform.Length == 0, notPlatform.Length == 0 ? $"{Swarm.All.Count} grupos" : notPlatform);
        Swarm f0 = fish.FirstOrDefault(s => s.Visible > 3);
        TestProbe.Check("los bancos de peces son peces de verdad (cuerpo, lomo, aletas y ojos; pez plateado antártico de ~28 cm a la vista)",
            f0 != null && Parts(f0, "Cuerpo", "Lomo", "Aletas", "Ojos") && Length(f0) > 0.15f && Length(f0) < 0.5f, f0 != null ? $"{fish.Count} bancos, {f0.name}: {f0.Visible} peces de {Length(f0):0.00} m" : "sin bancos");
        Swarm k0 = krill.FirstOrDefault(s => s.Visible > 3);
        TestProbe.Check("los enjambres de kril son kril de verdad (cuerpo, patas y ojos; ~12 cm a la vista)", k0 != null && Parts(k0, "Cuerpo", "Patas", "Ojos") && Length(k0) < 0.3f,
            k0 != null ? $"{krill.Count} enjambres, {k0.name}: {k0.Visible} kril de {Length(k0):0.00} m" : "sin kril");

        // ── 2. MOVIMIENTO DEL GRUPO ──
        if (f0 != null)
        {
            Vector3 p0 = f0.transform.position;
            var kids = f0.transform.Cast<Transform>().ToList();
            // (2026-11-04) En el marco del banco: al moverse el banco también GIRA hacia donde va, y un pez a 4 m del centro se desplaza
            // en el mundo metros de más (6/40 «con él» midiendo en el mundo). Lo que cuenta: sigue siendo hijo del banco, en su sitio del
            // grupo (su vaivén es de ±0,3 m) y cerca del centro.
            var w0 = kids.Select(c => c.localPosition).ToList();
            float oldSpeed = f0.driftSpeed, oldNight = f0.nightDriftFactor; f0.driftSpeed = 3f; f0.nightDriftFactor = 1f; float oldFlee = f0.fleeRange; f0.fleeRange = 0f;   // (de noche derivan a 0,3; y huyendo de una foca iba y venía: el banco se movía 1,5–2,8 m)
            Vector3 goal = p0 + new Vector3(6f, 0f, 3f);
            f0.GoTo(goal);
            // (2026-11-04) Se mide al LLEGAR a su destino: después vuelve a derivar por su zona (y medido a los 5 s ya había vuelto: 1,5 m).
            { float t0m = Time.time, r0m = Time.realtimeSinceStartup; while (Vector3.Distance(f0.transform.position, goal) > 1.2f && Time.time - t0m < 10f && Time.realtimeSinceStartup - r0m < 30f) yield return null; }
            Vector3 moved = f0.transform.position - p0;
            int with = 0;
            for (int i = 0; i < kids.Count; i++)
            {
                if (kids[i] == null) continue;
                Vector3 m = kids[i].localPosition - w0[i];
                if (kids[i].parent == f0.transform && m.magnitude < 1.2f && Vector3.Distance(kids[i].position, f0.transform.position) < f0.schoolSpread * 2f) with++;
            }
            int alive = kids.Count(c => c != null);
            f0.driftSpeed = oldSpeed; f0.nightDriftFactor = oldNight; f0.fleeRange = oldFlee;
            TestProbe.Check("al moverse el banco, todos sus peces van con él (el grupo es un solo cuerpo)", moved.magnitude > 3f && with == alive, $"el banco se movió {moved.magnitude:0.0} m; {with}/{alive} peces con él");
        }

        // ── 3. DEPREDADOR: un mordisco = un pez menos ──
        {
            Animal pred = null;
            foreach (GameObject g in Animal.wholePopulation)
                if (g != null && g.TryGetComponent(out Animal a) && !a.death && a.Forage != null && a.Forage.eatsFish && a.lifeStage == LifeStage.adult) { pred = a; break; }
            Swarm target = fish.FirstOrDefault(s => s.Visible >= 6);
            if (pred == null || target == null) TestProbe.Check("un depredador que come peces muerde el banco y desaparece un pez por mordisco", false, $"depredador {pred?.name ?? "ninguno"}, banco {target?.name ?? "ninguno"}");
            else
            {
                float g0 = target.growthPerSecond, gr0 = target.grazeRate; target.growthPerSecond = 0f; target.grazeRate = 0f;
                Vector3 back = pred.transform.position;
                // Su agente de NavMesh lo devolvía a tierra cada fotograma antes de que el banco mirase (ronda 24: una foca de un iceberg, con el
                // agente activo: «0 mordiscos»). Mientras se le pone en el banco, sin agente.
                bool navWas = pred.nav != null && pred.nav.enabled; if (navWas) pred.nav.enabled = false;
                int before = target.Visible, bites0 = Swarm.Bites; float cnt0 = target.count;
                float t0 = Time.time, r0 = Time.realtimeSinceStartup; int minVisible = before;
                while (Time.time - t0 < 2.5f && Time.realtimeSinceStartup - r0 < 10f)
                {
                    pred.hungry = 1f;
                    pred.transform.position = target.transform.position;
                    Physics.SyncTransforms();
                    yield return null;
                    minVisible = Mathf.Min(minVisible, target.Visible);
                }
                pred.transform.position = back;
                if (navWas) pred.nav.enabled = true;
                int after = target.Visible, bites = Mathf.RoundToInt(cnt0 - target.count);   // (los de ESTE banco: Swarm.Bites cuenta los de todos)
                TestProbe.Check($"un depredador ({pred.SpeciesName}) choca con el banco: cada mordisco quita UN pez, al momento", bites >= 1 && before - after == bites && Mathf.Abs(cnt0 - target.count - bites) < 0.01f && Swarm.Bites - bites0 >= bites,
                    $"{bites} mordiscos: {before} → {after} peces (cuenta {cnt0:0.0} → {target.count:0.0})");
                // ── 4. SE REHACE ──
                target.count = Mathf.Min(target.count, 6f); target.ReconcileChildren();
                int low = target.Visible;
                target.growthPerSecond = 1.5f / Mathf.Max(1, (int)(TimeController.timeController != null ? TimeController.timeController.TimeSpeed : 1));
                yield return Wait(4f);
                int high = target.Visible;
                target.growthPerSecond = g0; target.grazeRate = gr0;
                TestProbe.Check("sin ciclo de vida: el banco se rehace solo, un pez nuevo cada cierto tiempo", high >= low + 3, $"{low} → {high} peces en 4 s (a {target.growthPerSecond:0.00}/s de juego normalmente)");
            }
        }

        // ── 5. MEDUSAS ──
        {
            var dip = jelly.Where(s => s.species == SwarmModels.Species.Diplulmaris).ToList();
            var des = jelly.Where(s => s.species == SwarmModels.Species.Desmonema).ToList();
            Swarm j0 = jelly.FirstOrDefault(s => s.Visible > 0);
            float pulse = 0f;
            if (j0 != null)
            {
                Transform c = j0.transform.GetChild(0);
                float a = c.localScale.y; yield return Wait(0.6f); pulse = c != null ? Mathf.Abs(c.localScale.y - a) : 0f;
            }
            TestProbe.Check("hay grupos de medusas de McMurdo: Diplulmaris antarctica y Desmonema glaciale (umbrela, brazos y tentáculos) que laten",
                dip.Count > 0 && des.Count > 0 && j0 != null && Parts(j0, "Umbrela", "Brazos", "Tentaculos") && pulse > 0.001f,
                $"{dip.Count} grupos de Diplulmaris, {des.Count} de Desmonema; latido {pulse:0.000}");
        }

        // ── 6. EN LOS ÁRBOLES (fondo abierto) ──
        {
            ArbolCampos t = ArbolCampos.Instance;
            Swarm under = t != null ? Swarm.All.Where(s => s.kind != SwarmKind.Bee).OrderBy(s => Vector3.Distance(s.transform.position, t.transform.position + Vector3.down * 6f)).FirstOrDefault() : null;
            if (t == null || under == null) TestProbe.Check("un grupo marino sube al tronco de un edificio-árbol por su fondo abierto", false, "sin árbol o sin grupos");
            else
            {
                float sp = under.driftSpeed; under.driftSpeed = 3f;
                Vector3 from = under.transform.position;
                under.GoTo(t.World(new Vector3(0f, 8f, 0f)));
                float t0 = Time.time, r0 = Time.realtimeSinceStartup;
                while (Swarm.TreeAround(under.transform.position, true) != t && Time.time - t0 < 20f && Time.realtimeSinceStartup - r0 < 30f) yield return null;
                bool inTree = Swarm.TreeAround(under.transform.position, true) == t;
                under.driftSpeed = sp;
                TestProbe.Check("un grupo marino (peces, kril o medusas) sube desde el mar al tronco de un edificio-árbol por su fondo abierto", inTree,
                    $"{under.name} de {from} a {under.transform.position} en {Time.time - t0:0} s");
                under.GoTo(under.Origin);
            }
        }

        // ── 7. ABEJAS ──
        {
            BeeSwarm b = bees.FirstOrDefault(s => s.TreeCount >= 3) ?? bees.FirstOrDefault();
            if (b == null) TestProbe.Check("el enjambre de una colmena vuela de árbol en árbol durante el día", false, $"{Hive.All.Count} colmenas, ningún enjambre");
            else
            {
                bool day = TimeController.timeController == null || TimeController.timeController.IsDay;
                TestProbe.Check("cada colmena tiene su enjambre de pecoreadoras: abejas de verdad (cuerpo a bandas y alas) en un grupo visible", b.Visible >= 10 && Parts(b, "Cuerpo", "Bandas", "Ala_izq", "Ala_der"),
                    $"{bees.Count} enjambres; {b.name}: {b.Visible} abejas");
                if (!day) TestProbe.Check("el enjambre vuela de árbol en árbol durante el día (ahora es de noche: está en su colmena)", b.AtHive || Vector3.Distance(b.transform.position, b.hive.transform.position) < 3f);
                else
                {
                    float d0 = b.dwellSeconds, f0s = b.flySpeed; b.dwellSeconds = 0.5f; b.flySpeed = 12f;
                    b.visited.Clear(); var seen = b.visited;
                    float t0 = Time.time, r0 = Time.realtimeSinceStartup;
                    int want = Mathf.Min(3, b.TreeCount);
                    while (seen.Count < want && Time.time - t0 < 90f && Time.realtimeSinceStartup - r0 < 90f) yield return null;
                    b.dwellSeconds = d0; b.flySpeed = f0s;
                    TestProbe.Check($"de día, el enjambre vuela de árbol en árbol: visita varios árboles distintos (≥ {want})", seen.Count >= want,
                        $"{seen.Count} árboles distintos ({b.TreeCount} a su alcance) en {Time.time - t0:0} s, {b.Visible} abejas juntas");
                }
            }
        }
        Swarm.AnimateAll = false;
        TestProbe.End();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Grupos de animales (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloGrupos"); DontDestroyOnLoad(go);
        GroupAnimalsTest t = go.AddComponent<GroupAnimalsTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(GroupAnimalsTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
