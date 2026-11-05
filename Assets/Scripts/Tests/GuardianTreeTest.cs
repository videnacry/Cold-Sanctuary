using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL ÁRBOL GIGANTE Y DE LA SALA DE GUARDIA (Berón 2026-11-05; docs/arbol-guardian.md, docs/sala-de-guardia.md; grupo 91):
/// 1. ALTO: ≥ lo mayor del récord (142 m) y del edificio-árbol más alto (sin la lanzadera, que va por encima por su regla).
/// 2. ANCHO Y VOLUMEN: la base, ≥ 18 m, acanalada y con contrafuertes; el tronco, ≥ los 2550 m³ de Lindsey Creek.
/// 3. SIN TÚNEL: la base es maciza y no quedan pasillos.
/// 4. HIELO Y PANALES: posaderos y ≥ 4 panales con su colmena; no caza.
/// 5. SIN BOSQUE: solo el gigante y la residencia, y las abejas siguen teniendo dónde anidar.
/// 6. LA SALA DE GUARDIA: en Servicios, con sus piezas, sus pantallas y su misión; sus lecturas no fallan aunque falte un sistema.
/// 7. LANZADERA: la regla de altura (T13) sigue cumpliéndose con el gigante.
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
        TestProbe.Begin("Lecturas de la sala de guardia y récords (datos)");
        foreach (GuardWatch.Feed f in GuardWatch.Feeds)
            TestProbe.Check($"la pantalla «{GuardWatch.Title(f)}» siempre dice algo", !string.IsNullOrEmpty(GuardWatch.Read(f)), GuardWatch.Read(f));
        TestProbe.Check("un sismo fuerte es raro y el mar solo avisa tras un sismo fuerte", Enumerable.Range(0, 3650).Count(d => GuardWatchRule.Magnitude(d) >= GuardWatchRule.TsunamiWatchMagnitude) < 200
            && Enumerable.Range(0, 3650).All(d => GuardWatchRule.SeaState(d) == GuardWatchRule.Sea.Normal || GuardWatchRule.Magnitude(d) >= GuardWatchRule.TsunamiWatchMagnitude));
        TestProbe.Check("el tronco supera el volumen de Lindsey Creek", GuardianTree.TrunkVolume(GuardianTree.RecordHeight) >= GuardianTree.RecordVolume, $"{GuardianTree.TrunkVolume(GuardianTree.RecordHeight):0} m³");
        TestProbe.Check("la misión de la sala existe, con su efecto", AreaFunctions.Get("gua_sala") != null && AreaFunctions.Get("gua_sala").effect != null);
        TestProbe.Check("la sala cumple la mezcla (S14)", AreaDesign.Of("Sala de guardia")?.state == AreaDesign.State.Cumple);
        TestProbe.End();

        TestProbe.Begin("Árbol gigante: el más grande de la historia, sin túnel; sin bosque; sala de guardia");
        GameObject g = GameObject.Find(GuardianTree.ObjectName);
        if (g == null) { TestProbe.Check("existe el árbol gigante", false); TestProbe.End(); yield break; }
        Transform t = g.transform; Vector3 o = t.position;
        var trunk = g.GetComponentsInChildren<Collider>().Where(c => c.name == "Tronco").ToList();   // (colliders: sus cubos ya no dibujan, están en mallas combinadas)

        // ── 1. ALTO ──
        float top = trunk.Count > 0 ? trunk.Max(c => c.bounds.max.y) - o.y : 0f;
        float tallestBuilding = ArbolCampos.All.Where(a => a != null && a.building != LaunchHeightRule.Building).Select(a => a.rootHeight).DefaultIfEmpty(0f).Max();
        float want = Mathf.Max(GuardianTree.RecordHeight, tallestBuilding);
        TestProbe.Check($"el tronco es el más alto: ≥ máx(récord {GuardianTree.RecordHeight:0.0} m, edificio-árbol más alto {tallestBuilding:0.0} m)", top >= want - 0.5f,
            $"tronco {top:0.0} m (Hyperion, el más alto vivo, {GuardianTree.LivingRecordHeight:0.0} m)");

        // ── 2. ANCHO: 2 × la esquina más alejada del eje de las duelas de abajo (con su giro real) ──
        float rMax = 0f;
        foreach (Collider b in trunk) if (b.transform.position.y - o.y < 2f)
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = b.transform.TransformPoint(new Vector3((i & 1) == 0 ? -0.5f : 0.5f, (i & 2) == 0 ? -0.5f : 0.5f, (i & 4) == 0 ? -0.5f : 0.5f)) - o;
                rMax = Mathf.Max(rMax, new Vector2(p.x, p.z).magnitude);
            }
        int buttress = g.GetComponentsInChildren<Transform>().Count(x => x.name == "Raiz");
        TestProbe.Check($"la base es la del árbol más ancho registrado (≥ 18 m; el castaño de los cien caballos, {GuardianTree.RecordGirth:0.0} m de perímetro), con contrafuertes", 2f * rMax >= 18f && buttress >= 12,
            $"{2f * rMax:0.0} m, {buttress} contrafuertes");

        // ── 3. SIN TÚNEL ──
        bool solid = Physics.OverlapBox(o + Vector3.up * 3.5f, Vector3.one * 2f).Any(c => c.transform.IsChildOf(t));
        TestProbe.Check("sin túnel: la base es maciza y no quedan pasadizos", solid && t.Find("Pasadizos") == null);

        // ── 4. HIELO Y PANALES ──
        int hives = g.GetComponentsInChildren<Hive>(true).Length, perches = g.GetComponentsInChildren<Transform>().Count(x => x.name == "Rama_Plataforma");
        TestProbe.Check("posaderos con escarcha y panales de las abejas de las nieves; no caza", hives >= 4 && perches >= 10 && g.GetComponent<IceCarnivorousTree>() == null, $"{hives} colmenas, {perches} posaderos");

        // ── 5. SIN BOSQUE ──
        int nestTrees = FindObjectsByType<NestTree>(FindObjectsSortMode.None).Length, hunters = FindObjectsByType<IceCarnivorousTree>(FindObjectsSortMode.None).Length;
        TestProbe.Check("solo quedan el gigante y los 6 de la residencia, y ninguno caza", nestTrees == 7 && hunters == 0, $"{nestTrees} árboles con nidos, {hunters} cazadores");
        TestProbe.Check("las abejas de las nieves siguen anidando", FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None).Any(b => b.hive != null));

        // ── 6. LA SALA DE GUARDIA ──
        GameObject room = GameObject.Find(GuardRoom.FloorName);
        TestProbe.NotNull("la sala de guardia existe", room);
        if (room != null)
        {
            string names = string.Join(" ", room.GetComponentsInChildren<Transform>(true).Select(x => x.name));
            foreach (string piece in new[] { "Muro_de_pantallas", "Consolas_fila_1", "Mesa de radar", "Carta de hielo", "Sismógrafo", "Mareógrafo y boya", "Radio VHF", "Tablero de guardia", "Puesto de respaldo" })
                TestProbe.Check($"tiene {piece}", names.Contains(piece));
            TestProbe.Check("una pantalla por lectura", room.GetComponentsInChildren<GuardScreen>(true).Length == GuardWatch.Feeds.Length);
            TestProbe.Check("el tablero de guardia sigue, ya no en un árbol", room.GetComponentInChildren<GuardBoard>(true) != null);
            int seats = room.GetComponentsInChildren<AreaSeat>(true).Where(s => s.activity == "vigilancia").Sum(s => s.seats);
            TestProbe.Check("varias personas a la vez (S13)", seats >= AreaCapacity.MinWorkSeats, $"{seats} puestos");
            TestProbe.Check("la jornada conoce el sitio", WorkdayDirector.AreaPoint(AreaWork.GuardRoom, "Prueba") != Vector3.zero);
            TestProbe.Check("los puestos de vigía siguen para la patrulla", FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(x => x.name.StartsWith("PuestoVigia_")) == 4);
        }

        // ── 7. LANZADERA ──
        LaunchSite ls = FindAnyObjectByType<LaunchSite>();
        float need = LaunchHeightRule.RequiredPadHeight(out string w);
        TestProbe.Check("la regla de la lanzadera (T13) se cumple con el gigante", ls == null || ls.PadY >= need - 0.01f, ls != null ? $"plataforma a {ls.PadY:0} m; {w}" : "sin lanzadera en esta escena");
        yield return null;
        TestProbe.End();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Árbol gigante y sala de guardia (sola)")]
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
