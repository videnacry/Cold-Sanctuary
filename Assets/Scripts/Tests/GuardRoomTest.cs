using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA SALA DE GUARDIA Y DE LA ARBOLEDA (Berón 2026-11-05; docs/sala-de-guardia.md, docs/ice-sanctuary-ecology.md §Sin bosque, docs/territorio-polar.md; grupo 91):
/// 1. LA SALA DE GUARDIA: en Servicios, con sus piezas, sus pantallas y su misión; sus lecturas no fallan aunque falte un sistema.
/// 2. EL CAZADOR: solo los 6 árboles de la residencia, y caza animales, no personas ni sus abejas; no crece, porque sostiene casas.
/// 3. LOS CASQUETES SILVESTRES: sus estructuras, sus nidos y sus descansaderos, en una malla por material.
/// 4. LANZADERA: la regla de altura (T13) sigue cumpliéndose.
/// </summary>
public class GuardRoomTest : MonoBehaviour, ITestUnit
{
    public int Group => 91;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<GuardRoomTest>() == null) tr.gameObject.AddComponent<GuardRoomTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Lecturas de la sala de guardia y récords (datos)");
        foreach (GuardWatch.Feed f in GuardWatch.Feeds)
            TestProbe.Check($"la pantalla «{GuardWatch.Title(f)}» siempre dice algo", !string.IsNullOrEmpty(GuardWatch.Read(f)), GuardWatch.Read(f));
        TestProbe.Check("un sismo fuerte es raro y el mar solo avisa tras un sismo fuerte", Enumerable.Range(0, 3650).Count(d => GuardWatchRule.Magnitude(d) >= GuardWatchRule.TsunamiWatchMagnitude) < 200
            && Enumerable.Range(0, 3650).All(d => GuardWatchRule.SeaState(d) == GuardWatchRule.Sea.Normal || GuardWatchRule.Magnitude(d) >= GuardWatchRule.TsunamiWatchMagnitude));
        TestProbe.Check("la misión de la sala existe, con su efecto", AreaFunctions.Get("gua_sala") != null && AreaFunctions.Get("gua_sala").effect != null);
        TestProbe.Check("la sala cumple la mezcla (S14)", AreaDesign.Of("Sala de guardia")?.state == AreaDesign.State.Cumple);
        TestProbe.Check("la sala es de los tres guardias, también de Tote", new[] { "Gohageneis", "Nesvitor", "JefeConstruccion" }.All(w => Professions.Practises(w, AreaWork.GuardRoom, "gua_sala")));
        TestProbe.End();

        TestProbe.Begin("Sala de guardia, cazador y casquetes silvestres");

        // ── EL CAZADOR ──
        IceCarnivorousTree[] hunters = FindObjectsByType<IceCarnivorousTree>(FindObjectsSortMode.None);
        HunterCell hc = HunterCell.Instance;
        TestProbe.Check("un solo árbol cazador, y en su celda", hunters.Length == 1 && hc != null && hc.tree == hunters[0], $"{hunters.Length} cazadores");
        if (hc != null && hunters.Length == 1)
        {
            IceCarnivorousTree h = hunters[0];
            TestProbe.Check("conserva sus rasgos: señuelo, sopor y crecimiento", h.lureRadius > 20f && h.killRadius > 4f && h.growthPerKill > 0f && h.maxScale > 1f, $"{h.lureRadius:0} m, {h.killRadius:0.0} m, ×{h.maxScale:0.0}");
            TestProbe.Check("la celda contiene su mayor tamaño", hc.Contains(h.transform.position + Vector3.up * 2f));
            TestProbe.Check("sellada, lo sujeta; en brecha, no", hc.Holds(h));
            hc.Breach("prueba");
            bool loose = !hc.Holds(h) && GuardWatch.Alert(GuardWatch.Feed.Contencion) && HunterCell.Missions(1).All(m => m.id.StartsWith(HunterCell.RecaptureId));
            hc.Recapture("Prueba 1"); bool one = hc.Now == HunterCell.State.Brecha; hc.Recapture("Prueba 2");
            TestProbe.Check("la brecha avisa a la guardia, solo deja la recaptura y pide dos personas", loose && one && hc.Holds(h));
            TestProbe.Check("cada mañana, su ronda; el simulacro, semanal", HunterCell.Missions(1).Any(m => m.id == HunterCell.RoundId) && HunterCell.Missions(HunterRule.DrillEvery).Any(m => m.id == HunterCell.DrillId));
        }
        Bedroom[] rooms = FindObjectsByType<Bedroom>(FindObjectsSortMode.None);
        TestProbe.Check("la Residencia está en su edificio-árbol: sin casas de árbol en el santuario 1", rooms.Length >= 30 && rooms.All(r => r.GetComponentInParent<TreeFloor>() != null) && GameObject.Find("ArboledaResidencial") == null, $"{rooms.Length} habitaciones");
        TestProbe.Check("los árboles de la isla son solo flora", FindObjectsByType<NestTree>(FindObjectsSortMode.None).Length >= 3 && FindObjectsByType<NestTree>(FindObjectsSortMode.None).All(n => n.GetComponentInParent<TreeFloor>() == null));
        TestProbe.Check("las abejas de las nieves siguen anidando", FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None).Any(b => b.hive != null));

        // ── LOS CASQUETES SILVESTRES ──
        foreach (string cap in new[] { "BosqueOeste", "BosqueNorte" })
        {
            GameObject c = GameObject.Find("Casquete_silvestre_" + cap);
            TestProbe.NotNull($"el casquete {cap} tiene sus estructuras", c);
            if (c == null) continue;
            var kinds = c.GetComponentsInChildren<Affordance>(true).Select(a => a.kind).Distinct().ToList();
            TestProbe.Check($"{cap}: abrigo, posadero, liquen, descansadero y nidos", kinds.Contains(AffordanceKind.Shelter) && kinds.Contains(AffordanceKind.Perch) && kinds.Contains(AffordanceKind.Lichen)
                && kinds.Contains(AffordanceKind.HaulOut) && c.GetComponentsInChildren<SnowPetrelNest>(true).Length >= 2, string.Join(", ", kinds));
            int renderers = c.GetComponentsInChildren<Renderer>(true).Length;
            TestProbe.Check($"{cap}: una malla por material, no un objeto por cubo", renderers <= 4, $"{renderers} mallas");
        }

        // ── LA SALA DE GUARDIA ──
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

        // ── LANZADERA ──
        LaunchSite ls = FindAnyObjectByType<LaunchSite>();
        float need = LaunchHeightRule.RequiredPadHeight(out string w);
        TestProbe.Check("la regla de la lanzadera (T13) se cumple", ls == null || ls.PadY >= need - 0.01f, ls != null ? $"plataforma a {ls.PadY:0} m; {w}" : "sin lanzadera en esta escena");
        yield return null;
        TestProbe.End();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Sala de guardia y arboleda (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloGuardia"); DontDestroyOnLoad(go);
        GuardRoomTest t = go.AddComponent<GuardRoomTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(GuardRoomTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
