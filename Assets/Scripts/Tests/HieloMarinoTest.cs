using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA VIDA BAJO EL HIELO (Berón 2026-11-05; docs/hielo-y-respiraderos.md):
/// 1. FAUNA: bajo una plataforma de 300 m no se nada a −1,5 m: ningún marino está dentro del hielo; la plataforma es una pared.
/// 2. RESPIRADEROS: están en hielo fijo fino (o sea, donde los hay de verdad), abiertos al mar, junto a la grieta de marea, nunca en la plataforma.
/// 3. TÚNELES: la ruta del submarino de obra entre árboles no cruza la roca de la isla.
/// </summary>
public class HieloMarinoTest : MonoBehaviour, ITestUnit
{
    public int Group => 107;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<HieloMarinoTest>() == null) tr.gameObject.AddComponent<HieloMarinoTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Bajo el hielo: fauna fuera de la plataforma, respiraderos reales y túneles bajo la isla");
        Territorio ter = FindAnyObjectByType<Territorio>();
        if (ter == null) { Debug.Log("[TEST] SKIP · Hielo marino — esta escena no es el santuario"); TestProbe.End(); yield break; }

        // ── 1. FAUNA: unos segundos mirándolos ──
        int samples = 0, inside = 0; string who = "";
        for (int t = 0; t < 10; t++)
        {
            foreach (MarineLife m in FindObjectsByType<MarineLife>(FindObjectsSortMode.None))
            {
                if (m == null || m.OnIce) continue;
                Vector3 p = m.transform.position;
                if (p.y > -0.3f) continue;
                samples++;
                if (MarineLife.ThickAbove(p) && !MarineLife.UnderTree(p, 2f)) { inside++; who = $"{m.name} en ({p.x:0},{p.y:0.0},{p.z:0})"; }
            }
            yield return new WaitForSeconds(0.5f);
        }
        TestProbe.Check("ningún marino nada dentro de la plataforma de hielo", samples > 0 && inside == 0, $"{samples} muestras, {inside} dentro {who}");

        // ── 2. RESPIRADEROS ──
        var holes = FindObjectsByType<Affordance>(FindObjectsSortMode.None).Where(a => a.kind == AffordanceKind.BreathingHole).ToList();
        TestProbe.Greater("hay respiraderos", holes.Count, 3f, $"{holes.Count}");
        foreach (Affordance h in holes)
        {
            Vector3 p = h.transform.position;
            TestProbe.Check($"{h.name}: abierto al agua", !MarineLife.IceAbove(p));
            bool thin = Physics.OverlapSphere(p, 2.5f).Any(c => c.name == MarineLife.ThinIce);
            TestProbe.Check($"{h.name}: en hielo fijo fino", thin);
        }
        Collider sheet = FindObjectsByType<Collider>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == MarineLife.ThinIce);
        TestProbe.Check("el hielo fijo es fino (≤ 3 m) y un oso puede acechar encima", sheet != null && sheet.bounds.size.y <= 3f && sheet.bounds.max.y > -0.1f);
        TestProbe.Check("bajo el hielo fijo sí se nada", sheet != null && MarineLife.IceAbove(sheet.bounds.center) && !MarineLife.ThickAbove(sheet.bounds.center));

        // ── 3. TÚNELES ──
        var trees = ArbolCampos.All.Where(a => a != null).ToList();
        int routes = 0, blocked = 0; string what = "";
        for (int i = 0; i < trees.Count; i++)
            for (int j = i + 1; j < trees.Count; j++)
            {
                Vector3 a = trees[i].transform.position + Vector3.down * (SeaRule.ShelfThickness + 12f), b = trees[j].transform.position + Vector3.down * (SeaRule.ShelfThickness + 12f);
                routes++;
                foreach (RaycastHit h in Physics.SphereCastAll(a, 2.5f, (b - a).normalized, Vector3.Distance(a, b), ~0, QueryTriggerInteraction.Ignore))
                    if (h.collider.name == "Zocalo_rocoso") { blocked++; what = $"{trees[i].building} → {trees[j].building}"; break; }
            }
        TestProbe.Check("el submarino de obra no cruza la roca: hay túneles", routes > 0 && blocked == 0, $"{routes} rutas, {blocked} por la roca {what}");
        TestProbe.End();
    }
}
