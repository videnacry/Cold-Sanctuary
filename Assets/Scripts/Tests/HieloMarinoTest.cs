using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA VIDA BAJO EL HIELO (Berón 2026-11-05; docs/hielo-y-respiraderos.md):
/// 1. FAUNA: ningún marino está dentro de una placa de hielo ni pasa bajo ella: las placas y la isla son paredes (P20).
/// 2. RESPIRADEROS: están en hielo fijo fino (o sea, donde los hay de verdad), abiertos al mar, junto a la grieta de marea, nunca en las placas.
/// 3. SUBMARINO: su ruta entre árboles, a −12 m, pasa por debajo de la falda de roca (−6 m).
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
        TestProbe.Begin("Bajo el hielo: fauna fuera de las placas, respiraderos reales y paso bajo la isla");
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
        TestProbe.Check("ningún marino nada bajo una placa de hielo ni dentro de ella", samples > 0 && inside == 0, $"{samples} muestras, {inside} dentro {who}");

        // ── 2. RESPIRADEROS ──
        var holes = FindObjectsByType<Affordance>(FindObjectsSortMode.None).Where(a => a.kind == AffordanceKind.BreathingHole).ToList();
        TestProbe.Greater("hay respiraderos", holes.Count, 3f, $"{holes.Count}");
        foreach (Affordance h in holes)
        {
            Vector3 p = h.transform.position;
            TestProbe.Check($"{h.name}: abierto al agua", !MarineLife.IceAbove(p));
            bool thin = Physics.OverlapSphere(p, 2.5f).Any(c => c.name == MarineLife.ThinIce);
            TestProbe.Check($"{h.name}: en hielo fijo", thin);
        }
        Collider sheet = FindObjectsByType<Collider>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == MarineLife.ThinIce);
        TestProbe.Check("el hielo fijo mide lo que las placas y un oso puede acechar encima", sheet != null && Mathf.Abs(sheet.bounds.size.y - SeaRule.PlateThickness) < 0.1f && sheet.bounds.max.y > -0.1f);
        TestProbe.Check("bajo el hielo fijo sí se nada", sheet != null && MarineLife.IceAbove(sheet.bounds.center) && !MarineLife.ThickAbove(sheet.bounds.center), sheet != null ? string.Join(", ", Physics.RaycastAll(new Vector3(sheet.bounds.center.x, 8f, sheet.bounds.center.z), Vector3.down, 14f, ~0, QueryTriggerInteraction.Ignore).Select(h => $"{h.collider.name} a {h.point.y:0.0}")) : "");

        // ── 3. SUBMARINO ──
        var trees = ArbolCampos.All.Where(a => a != null).ToList();
        int routes = 0, blocked = 0; string what = "";
        for (int i = 0; i < trees.Count; i++)
            for (int j = i + 1; j < trees.Count; j++)
            {
                Vector3 a = trees[i].transform.position + Vector3.down * 12f, b = trees[j].transform.position + Vector3.down * 12f;
                routes++;
                foreach (RaycastHit h in Physics.SphereCastAll(a, 2.5f, (b - a).normalized, Vector3.Distance(a, b), ~0, QueryTriggerInteraction.Ignore))
                    if (h.collider.name == "Zocalo_rocoso") { blocked++; what = $"{trees[i].building} → {trees[j].building}"; break; }
            }
        TestProbe.Check("el submarino de obra pasa por debajo de la falda de roca", routes > 0 && blocked == 0, $"{routes} rutas, {blocked} por la roca {what}");
        TestProbe.End();
    }
}
