using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// ESCENARIO: UNA ORCA HERIDA (Berón 2026-11-03; docs/submarinos-medicos.md). En vivo y a ×4:
/// 1. el Leviatán existe, atracado a ras del hielo con su pasarela en la orilla, y su pozo médico cabe la orca (con margen);
/// 2. una orca herida (enfermedad, estrés y una herida que sangra) queda a la deriva en mar abierto;
/// 3. la lanzadera (el submarino-ambulancia) trae al equipo veterinario del hospital; el Leviatán se sumerge, va, emerge a su
///    lado, la guía al pozo, sube el suelo hasta que el espiráculo sale del agua con el cuerpo apoyado, la trata (sin anestesia
///    general), baja el suelo, la libera sana en el mar y vuelve a su atraque.
/// </summary>
public class WhaleHospitalTest : MonoBehaviour, ITestUnit
{
    public int Group => 82;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<WhaleHospitalTest>() == null) tr.gameObject.AddComponent<WhaleHospitalTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Leviatán: una orca herida, tratada a bordo en el pozo médico y liberada");
        WhaleHospitalSubmarine lev = WhaleHospitalSubmarine.All.FirstOrDefault(s => s != null);
        TestProbe.Check("hay un Leviatán (hospital submarino de animales gigantes)", lev != null);
        if (lev == null) { TestProbe.End(); yield break; }
        float deck = lev.berthPos.y + WhaleHospitalSubmarine.Height * 0.5f;
        Transform gw = lev.Gangway;
        bool shore = gw != null && Physics.Raycast(gw.position + Vector3.up * 5f, Vector3.down, out RaycastHit h, 8f, ~0, QueryTriggerInteraction.Ignore) && h.point.y > -0.3f && !h.collider.transform.IsChildOf(lev.transform);
        TestProbe.Check("atracado a ras del suelo: la cubierta al nivel del hielo y la pasarela en la orilla", Mathf.Abs(deck) < 0.6f && shore, $"cubierta a {deck:0.00} m; pasarela {(shore ? "sobre el hielo" : "sin orilla")}");

        Anima orca = FindObjectsByType<Anima>(FindObjectsSortMode.None).Where(a => a != null && !a.death && a.SpeciesName == "Orca").OrderBy(a => Vector3.Distance(a.transform.position, lev.transform.position)).FirstOrDefault();
        TestProbe.Check("hay una orca en el santuario", orca != null);
        if (orca == null) { TestProbe.End(); yield break; }
        Bounds ob = WhaleHospitalSubmarine.BodyBounds(orca.transform);
        float olen = Mathf.Max(ob.size.x, ob.size.z), owid = Mathf.Min(ob.size.x, ob.size.z);
        TestProbe.Check("su pozo médico cabe la orca (y una ballena de 16 m)", WhaleHospitalSubmarine.Well.y >= Mathf.Max(olen, 16f) + 4f && WhaleHospitalSubmarine.Well.x >= owid + 2f,
            $"pozo {WhaleHospitalSubmarine.Well.y:0} × {WhaleHospitalSubmarine.Well.x:0} m; orca de {olen:0.0} × {owid:0.0} m");

        // La orca herida, a la deriva en mar abierto (a 50–150 m del atraque).
        Vector3? sea = null;
        foreach (float r in new[] { 60f, 90f, 120f, 150f })
        {
            for (int a = 0; a < 360 && !sea.HasValue; a += 15)
            {
                Vector3 p = lev.berthPos + Quaternion.Euler(0f, a, 0f) * Vector3.forward * r; p.y = -1f;
                if (Mathf.Abs(p.x) < 240f && Mathf.Abs(p.z) < 240f && !lev.IceOver(p, 14f) && !lev.IceOver(p + (lev.berthPos - p).normalized * 40f, 10f)) sea = p;
            }
            if (sea.HasValue) break;
        }
        TestProbe.Check("hay mar abierto cerca para el escenario", sea.HasValue);
        if (!sea.HasValue) { TestProbe.End(); yield break; }
        Vector3 home = orca.transform.position; Quaternion homeR = orca.transform.rotation;
        MarineLife ml = orca.GetComponent<MarineLife>(); bool mlOn = ml != null && ml.enabled;
        if (ml != null) ml.enabled = false;   // herida: a la deriva, no nada
        orca.transform.position = sea.Value;
        BloodLoss bl = orca.gameObject.AddComponent<BloodLoss>(); bl.lost = 0.2f;
        orca.sickness = 0.7f; orca.stress = 0.8f;
        float old = Time.timeScale; Time.timeScale = 4f;
        bool went = lev.Dispatch(orca);
        float t0 = Time.realtimeSinceStartup; float maxBlow = float.NegativeInfinity; bool sawInWell = false;
        while (went && !(lev.released && lev.phase == WhaleHospitalSubmarine.Phase.Atracado) && Time.realtimeSinceStartup - t0 < 720f)
        {
            if (lev.inWell) sawInWell = true;
            yield return null;
        }
        Time.timeScale = old;
        TestProbe.Check("la lanzadera trae al equipo veterinario del hospital", lev.crew.Count >= 1, $"{string.Join(", ", lev.crew.Select(g => g != null ? g.name : "?"))}");
        TestProbe.Check("va a la orca y la guía al pozo médico", sawInWell, string.Join(" | ", lev.log.Take(6)));
        TestProbe.Check("sube el suelo: el espiráculo fuera del agua y el cuerpo apoyado (respira sola)", lev.blowholeAboveWater >= 0.1f && lev.blowholeAboveWater < 2.5f && lev.supportGap < 1.2f,
            $"lomo {lev.blowholeAboveWater:0.00} m sobre el agua; vientre a {lev.supportGap:0.00} m del suelo");
        TestProbe.Check("la trata a bordo: enfermedad y herida mejoran", lev.sicknessAfter <= lev.sicknessBefore - 0.3f && bl.lost < 0.2f, $"enfermedad {lev.sicknessBefore:0.00} → {lev.sicknessAfter:0.00}; pérdida {bl.lost * 100f:0} %");
        TestProbe.Check("la libera viva en el mar", lev.released && !orca.death && orca.transform.position.y < -0.3f, $"en ({orca.transform.position.x:0}, {orca.transform.position.y:0.0}, {orca.transform.position.z:0})");
        TestProbe.Check("y vuelve a su atraque", lev.phase == WhaleHospitalSubmarine.Phase.Atracado && Vector3.Distance(lev.transform.position, lev.berthPos) < 3f, $"{lev.missionSeconds:0} s de juego ({Time.realtimeSinceStartup - t0:0} s reales a ×4)");
        Debug.Log("[Leviatán] Registro del escenario: " + string.Join(" | ", lev.log));
        Destroy(bl);
        if (ml != null) ml.enabled = mlOn;
        TestProbe.End();
    }
}
