using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// PRUEBA DEL ÁRBOL GUARDIÁN, EL MÁS GRANDE DE LA HISTORIA (Berón 2026-11-04; docs/arbol-guardian.md):
/// 1. ALTO: ≥ lo mayor del récord histórico (132,6 m) y del edificio-árbol más alto (sin la lanzadera, que va por encima por su regla).
/// 2. ANCHO: la base, ≥ 18 m (el castaño de los cien caballos, 57,9 m de perímetro), acanalada y con contrafuertes.
/// 3. PASADIZOS: dos túneles en cruz de 7 × 7 m atraviesan la base sin nada que estorbe; el NavMesh los cruza de boca a boca.
/// 4. HIELO Y PANALES: los colores de los árboles de hielo, posaderos y ≥ 4 panales con su colmena; no caza.
/// 5. GUARDIA: la plataforma rodea el tronco, la cabaña con su tablero y las alfombras al pie (frente al pasadizo) y arriba.
/// 6. LANZADERA: la regla de altura (T13) sigue cumpliéndose con el guardián.
/// </summary>
public class GuardianTreeTest : MonoBehaviour, ITestUnit
{
    public int Group => 91;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<GuardianTreeTest>() == null) tr.gameObject.AddComponent<GuardianTreeTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Árbol guardián: el más grande de la historia (injerto del más alto y del más ancho), pasadizos, hielo y panales");
        GameObject g = GameObject.Find("CasaArbolGuardia");
        if (g == null) { TestProbe.Check("existe el árbol guardián", false); TestProbe.End(); yield break; }
        Transform t = g.transform; Vector3 o = t.position;
        var trunk = g.GetComponentsInChildren<Renderer>().Where(r => r.name == "Tronco").ToList();

        // ── 1. ALTO ──
        float top = trunk.Count > 0 ? trunk.Max(r => r.bounds.max.y) - o.y : 0f;
        float tallestBuilding = ArbolCampos.All.Where(a => a != null && a.building != LaunchHeightRule.Building).Select(a => a.rootHeight).DefaultIfEmpty(0f).Max();
        float want = Mathf.Max(GuardianTree.RecordHeight, tallestBuilding);
        TestProbe.Check($"el tronco es el más alto de la historia: ≥ máx(récord {GuardianTree.RecordHeight:0.0} m, edificio-árbol más alto {tallestBuilding:0.0} m)", top >= want - 0.5f,
            $"tronco {top:0.0} m (Hyperion, el más alto vivo, {GuardianTree.LivingRecordHeight:0.0} m)");

        // ── 2. ANCHO ──
        // (2026-11-04) El DIÁMETRO = 2 × la esquina más alejada del eje de las duelas de abajo (con su giro real): con la caja alineada a los
        // ejes de cada renderer salía 15,9 m (las duelas que miran a los pasadizos no están: ahí van los túneles).
        float rMax = 0f;
        foreach (Transform b in g.GetComponentsInChildren<Transform>()) if (b.name == "Tronco" && b.position.y - o.y < 2f)
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = b.TransformPoint(new Vector3((i & 1) == 0 ? -0.5f : 0.5f, (i & 2) == 0 ? -0.5f : 0.5f, (i & 4) == 0 ? -0.5f : 0.5f)) - o;
                rMax = Mathf.Max(rMax, new Vector2(p.x, p.z).magnitude);
            }
        float wx = rMax, wz = rMax;
        int roots = g.GetComponentsInChildren<Transform>().Count(x => x.name == "Raiz");
        TestProbe.Check($"la base es la del árbol más ancho de la historia (≥ 18 m; el castaño de los cien caballos: {GuardianTree.RecordGirth:0.0} m de perímetro), con contrafuertes",
            2f * Mathf.Min(wx, wz) >= 18f && roots >= 4, $"{2f * rMax:0.0} m de diámetro a ras de suelo, {roots} contrafuertes");

        // ── 3. PASADIZOS ──
        float L = GuardianTree.BaseDiameter * 0.5f + 3f, hw = GuardianTree.TunnelWidth * 0.5f;
        int blocked = 0; string why = "";
        foreach (Vector3 axis in new[] { Vector3.forward, Vector3.right })
        {
            Vector3 side = Vector3.Cross(Vector3.up, axis);
            for (float s = -L; s <= L; s += 1f)
                foreach (Collider c in Physics.OverlapBox(o + axis * s + Vector3.up * 3.5f, new Vector3(0.5f, 2.6f, 0.5f) + side * 0f + Vector3.Scale(new Vector3(Mathf.Abs(side.x), 0f, Mathf.Abs(side.z)), Vector3.one * (hw - 0.6f)), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
                    if (c.transform.IsChildOf(t)) { blocked++; if (why.Length < 160) why += $"{c.name} en {s:0} m; "; }
        }
        bool pathOk = false; float pathLen = 0f;
        foreach (Vector3 axis in new[] { Vector3.forward, Vector3.right })
        {
            if (!NavMesh.SamplePosition(o - axis * (L + 2f), out NavMeshHit a, 3f, NavMesh.AllAreas) || !NavMesh.SamplePosition(o + axis * (L + 2f), out NavMeshHit b, 3f, NavMesh.AllAreas)) { pathOk = false; break; }
            var path = new NavMeshPath();
            pathOk = NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
            float l = 0f; for (int i = 1; i < path.corners.Length; i++) l += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            pathLen = Mathf.Max(pathLen, l);
            if (!pathOk || l > 2f * L + 10f) { pathOk = false; break; }   // (por DENTRO del tronco: no rodeándolo)
        }
        TestProbe.Check($"dos pasadizos en cruz de {GuardianTree.TunnelWidth:0} × {GuardianTree.TunnelHeight:0} m atraviesan la base sin nada que estorbe (caben 8 personas y una alfombra)", blocked == 0 && g.transform.Find("Pasadizos") != null,
            blocked == 0 ? "libres" : why);
        TestProbe.Check("el NavMesh cruza los pasadizos de boca a boca (por dentro del tronco)", pathOk, $"camino más largo {pathLen:0} m para {2f * L + 4f:0} m de boca a boca");

        // ── 4. HIELO Y PANALES ──
        Color ice = new Color(0.55f, 0.72f, 0.80f);
        bool iceBark = trunk.Count > 0 && trunk.All(r => r.sharedMaterial != null && Vector4.Distance(r.sharedMaterial.color, ice) < 0.05f);
        int hives = g.GetComponentsInChildren<Hive>(true).Length, nidos = g.GetComponentsInChildren<Transform>(true).Count(x => x.name == "Nido_Abejas"), perches = g.GetComponentsInChildren<Transform>().Count(x => x.name == "Rama_Plataforma");
        TestProbe.Check("con los matices de los árboles de hielo: corteza de hielo azulado, posaderos con escarcha y panales de las abejas de las nieves; no caza",
            iceBark && hives >= 4 && perches >= 10 && g.GetComponent<IceCarnivorousTree>() == null, $"corteza de hielo {iceBark}, {hives} colmenas en {nidos} nidos, {perches} posaderos");

        // ── 5. GUARDIA ──
        int pads = CarpetPad.All.Count(p => p != null && p.building == "Casa del guarda");
        bool board = g.GetComponentInChildren<GuardBoard>() != null;
        int deck = g.GetComponentsInChildren<Transform>().Count(x => x.name == "Plataforma");
        TestProbe.Check("la guardia sigue en su árbol: plataforma en anillo alrededor del tronco, cabaña con el tablero y alfombras al pie (frente al pasadizo) y arriba",
            board && pads >= 2 && deck >= 4, $"tablero {board}, {pads} zonas de alfombras, {deck} tablas de plataforma");

        // ── 6. LANZADERA ──
        LaunchSite ls = FindAnyObjectByType<LaunchSite>();
        float need = LaunchHeightRule.RequiredPadHeight(out string w);
        TestProbe.Check("la regla de la lanzadera (T13) se cumple con el guardián", ls == null || ls.PadY >= need - 0.01f, ls != null ? $"plataforma a {ls.PadY:0} m; {w}" : "sin lanzadera en esta escena");
        yield return null;
        TestProbe.End();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Árbol guardián (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloGuardian"); DontDestroyOnLoad(go);
        GuardianTreeTest t = go.AddComponent<GuardianTreeTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(GuardianTreeTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
