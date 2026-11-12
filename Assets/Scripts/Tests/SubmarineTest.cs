using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL SUBMARINO DE OBRA Y DE LAS MEDIDAS DE LA VÍA MARINA (Berón 2026-11-03; docs/submarino-de-obra.md):
/// 1. MEDIDAS: las puertas son cuadradas y caben 8 personas y el vehículo de diseño con su holgura; las ramas, también; el tronco
///    deja pasar el submarino junto a un banco de peces; los anillos de los pisos van por encima de la puerta del piso.
/// 2. GARAJE: hay un garaje (un piso de Producción) con el submarino aparcado.
/// 3. ENTREGA: con una obra sin material, el submarino carga en el garaje, va por tierra a la puerta, nada por el tronco, sale al
///    mar por el fondo, sube por el tronco del árbol de la obra, sale por su puerta, aparca en el piso y descarga: la obra tiene su
///    material. Con un banco de peces y un grupo de medusas puestos en su camino, no toca a ningún animal.
/// 4. VUELTA: vuelve al garaje y aparca.
/// </summary>
public class SubmarineTest : MonoBehaviour, ITestUnit
{
    public int Group => 73;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<SubmarineTest>() == null) tr.gameObject.AddComponent<SubmarineTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Submarino de obra: medidas de puertas, ramas y tronco, garaje, entrega por la vía marina sin tocar animales");
        // ── 1. MEDIDAS ──
        float people = TreeSpec.PeopleAtOnce * TreeSpec.PersonWidth + (TreeSpec.PeopleAtOnce - 1f) * TreeSpec.PersonGap;
        float vehicle = Mathf.Max(TreeSpec.VehicleBeam, TreeSpec.VehicleHeight) + 2f * TreeSpec.VehicleClearance;
        HelixTower h = TreeSpec.Helix;
        float tube = h.TubeInnerH, trunk = 2f * h.waterHalf, school = 7f;
        TestProbe.Check($"puertas cuadradas de {TreeSpec.DoorWidth:0} × {TreeSpec.DoorHeight:0} m: caben 8 personas ({people:0.00} m) y el vehículo de diseño con su holgura ({vehicle:0} m)",
            Mathf.Approximately(TreeSpec.DoorWidth, TreeSpec.DoorHeight) && TreeSpec.DoorWidth >= people && TreeSpec.DoorWidth >= vehicle - 0.01f);
        TestProbe.Check($"las ramas ({tube:0} × {tube:0} m) son cuadradas y caben el vehículo y su holgura, y gira en el codo (14 × 5 m pide ≥ 8,5 m)",
            tube >= vehicle && tube >= (TreeSpec.VehicleLength * 0.5f + TreeSpec.VehicleBeam) / Mathf.Sqrt(2f));
        TestProbe.Check($"el tronco ({trunk:0} m) deja pasar el submarino junto a un banco de peces (5 + 7 m + 3 holguras de 1 m)", trunk >= TreeSpec.VehicleBeam + school + 3f * TreeSpec.VehicleClearance);
        int rings = 0, highRings = 0; string low = "";
        foreach (ArbolCampos t in ArbolCampos.All)
            foreach (Transform r in t.GetComponentsInChildren<Transform>())
            {
                if (r.Find("Balcon_tronco") == null) continue;
                TreeFloor f = r.GetComponentInParent<TreeFloor>(); if (f == null) continue;
                rings++; float y = r.position.y - f.transform.position.y;
                if (y >= TreeSpec.DoorHeight + 0.5f) highRings++; else if (low.Length < 200) low += $"{t.building}/{r.name} a {y:0.0} m; ";
            }
        TestProbe.Check($"los anillos de los pisos van por encima de la puerta del piso (≥ {TreeSpec.DoorHeight + 0.5f:0.#} m): el submarino sale sin tocar el balcón", rings > 0 && highRings == rings, $"{highRings}/{rings} anillos; {low}");

        // ── 2. GARAJE ──
        ConstructionSubmarine sub = ConstructionSubmarine.Instance;
        bool garage = sub != null && sub.transform.parent != null && sub.transform.parent.name == "Garaje_submarinos";
        TestProbe.Check("hay un GARAJE de submarinos (un piso de la torre de Producción) con el submarino de obra aparcado", garage, sub != null ? $"en «{sub.transform.parent?.name}»" : "sin submarino");
        if (sub == null) { TestProbe.End(); yield break; }

        // ── 3. ENTREGA ──
        // Si está llevando material a otra obra, se espera a que aparque, como en 90 y 94: Send no acepta encargos de viaje («en 0 s»).
        { float old = Time.timeScale; Time.timeScale = 4f; float tw = Time.time, rw = Time.realtimeSinceStartup; while (sub.Now != ConstructionSubmarine.State.Parked && Time.time - tw < 900f && Time.realtimeSinceStartup - rw < 300f) yield return null; Time.timeScale = old; }
        ArbolCampos home = ArbolCampos.At(sub.transform.position + Vector3.up * 0.5f, out _, out _, false);
        // Las obras se guardan: en una partida con todas terminadas (ronda 12: las 4 en 6/6) no queda ninguna abierta. La prueba prepara la suya, sin tocar lo guardado, y la deja como estaba.
        ConstructionProject job = ConstructionProject.All.Where(p => p != null).OrderBy(p => p.Finished)
            .ThenByDescending(p => ArbolCampos.At(p.transform.position + Vector3.up * 0.5f, out _, out _, false) != home)
            .FirstOrDefault(p => ArbolCampos.At(p.transform.position + Vector3.up * 0.5f, out _, out _, false) != null);
        if (job == null) { TestProbe.Check("hay una obra en un edificio-árbol a la que llevar material", false); TestProbe.End(); yield break; }
        ArbolCampos dest = ArbolCampos.At(job.transform.position + Vector3.up * 0.5f, out TreeLevel dl, out _, false);
        if (MaterialStore.Instance != null) foreach (var k in MaterialStore.DayKit) if (MaterialStore.Instance.Have(k.item) < k.n) MaterialStore.Instance.Add(k.item, k.n);
        int jobDays = job.daysDone; bool jobMaterial = job.materialOnSite, jobPersist = job.persist;
        job.persist = false;
        if (job.Finished) job.Restore(0, false);
        job.materialOnSite = false; job.Refresh();
        // Animales en su camino: un banco de peces y un grupo de medusas bajo el árbol de la obra y en su tronco.
        Swarm fish = Swarm.All.FirstOrDefault(s => s != null && s.kind == SwarmKind.Fish);
        Swarm jelly = Swarm.All.FirstOrDefault(s => s != null && s.kind == SwarmKind.Jellyfish);
        Vector3 seaMid = (home != null && home != dest ? Vector3.Lerp(home.transform.position, dest.transform.position, 0.6f) : dest.transform.position + new Vector3(12f, 0f, 0f)) + Vector3.up * sub.seaDepth;
        if (fish != null) { fish.transform.position = seaMid; fish.GoTo(seaMid); }   // en medio de su camino por el mar
        if (jelly != null) { jelly.transform.position = dest.World(new Vector3(-3f, dl.inside.y * 0.5f, 2f)); jelly.GoTo(jelly.transform.position); }
        int d0 = sub.Deliveries;
        Vector3 start = sub.transform.position;
        sub.ResetTrip();   // (lo que se mide es ESTE viaje, no los de toda la batería)
        bool sent = sub.Send(job);
        float oldScale = Time.timeScale; Time.timeScale = 3f;
        float t0 = Time.time, r0 = Time.realtimeSinceStartup; bool wasWet = false, wasOnDeck = false;
        while (sub.Deliveries == d0 && sent && Time.time - t0 < 600f && Time.realtimeSinceStartup - r0 < 300f)
        {
            yield return null;
            if (WaterZone.Underwater(sub.transform.position + Vector3.up * 2.5f)) wasWet = true;
        }
        float tookS = Time.time - t0;
        Vector3 at = sub.transform.position;
        TreeLevel atLv = dest.LevelAt(at + Vector3.up * 0.5f);
        wasOnDeck = atLv == dl;
        TestProbe.Check($"el submarino lleva la maquinaria y el material de la obra «{job.title}» ({dest.building}) por la vía marina: la obra tiene su material",
            sent && sub.Deliveries > d0 && job.materialOnSite, $"en {tookS:0} s de juego; {sub.doorsUsed.Count} puertas: {string.Join(", ", sub.doorsUsed)}");
        TestProbe.Check("nada por el tronco y el mar y anda por tierra (sus orugas) hasta aparcar en el piso de la obra", wasWet && sub.SwamMeters > 20f && sub.DrivenMeters > 5f && wasOnDeck,
            $"nadó {sub.SwamMeters:0} m, anduvo {sub.DrivenMeters:0} m; aparcado en {atLv?.name ?? "ningún nivel"}");
        TestProbe.Check("no toca a ningún animal (bancos, kril, medusas y fauna marina) en su trayecto: los esquiva", sub.MinClearance > 0f,
            $"holgura mínima {sub.MinClearance:0.0} m, {sub.Avoidances} esquivas");
        // ── 4. VUELTA ──
        { float t1 = Time.time, r1 = Time.realtimeSinceStartup; while (sub.Now != ConstructionSubmarine.State.Parked && Time.time - t1 < 600f && Time.realtimeSinceStartup - r1 < 300f) yield return null; }
        Time.timeScale = oldScale;
        job.Restore(jobDays, jobMaterial); job.persist = jobPersist;
        TestProbe.Check("vuelve al garaje y aparca en su plaza", sub.Now == ConstructionSubmarine.State.Parked && Vector3.Distance(sub.transform.position, sub.home.position) < 2f,
            $"a {Vector3.Distance(sub.transform.position, sub.home.position):0.0} m de su plaza; holgura mínima con los animales en todo el viaje {sub.MinClearance:0.0} m");
        TestProbe.End();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Submarino de obra (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloSubmarino"); DontDestroyOnLoad(go);
        SubmarineTest t = go.AddComponent<SubmarineTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(SubmarineTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
