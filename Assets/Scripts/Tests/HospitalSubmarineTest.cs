using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL SUBMARINO-AMBULANCIA (Berón 2026-11-03; docs/submarinos-medicos.md), en vivo y a ×4:
/// 1. existe, aparcado junto a la puerta del suelo del árbol del hospital, cabe por las puertas de los edificios-árbol y lleva su
///    equipo de urgencias (bodega para oso o elefante, grúa, O₂, monitor, desfibrilador, ventilador, sangre O−, fluidos);
/// 2. UNA PERSONA herida en el hielo (pérdida de sangre de clase III): sube por el tronco a por el equipo del hospital, va, la carga,
///    le pone sangre O− por el camino, vuelve por el tronco a urgencias, la descarga en el puesto, la atienden y vuelve a aparcar;
/// 3. UN OSO herido: igual, pero al piso veterinario (fluidos, no sangre humana).
/// </summary>
public class HospitalSubmarineTest : MonoBehaviour, ITestUnit
{
    public int Group => 81;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<HospitalSubmarineTest>() == null) tr.gameObject.AddComponent<HospitalSubmarineTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Submarino-ambulancia del hospital: anfibio, por dentro de los árboles, con su equipo; recoge a una persona y a un oso y los lleva al hospital");
        HospitalSubmarine sub = HospitalSubmarine.All.FirstOrDefault(s => s != null);
        TestProbe.Check("hay un submarino-ambulancia", sub != null);
        if (sub == null) { TestProbe.End(); yield break; }
        bool fits = sub.FitsDoor(out string why);
        TestProbe.Check("cabe por las puertas de los edificios-árbol (con 0,2 m a cada lado)", fits, why);
        string[] kit = { "Bodega/Trineo_carga", "Bodega/Viga_grua_8t", "Bodega/Camilla", "Bodega/Equipos/Concentrador_O2", "Bodega/Equipos/Bombona_O2", "Bodega/Equipos/Monitor_constantes",
                         "Bodega/Equipos/Desfibrilador", "Bodega/Equipos/Ventilador", "Bodega/Equipos/Nevera_sangre_O_neg", "Bodega/Equipos/Fluidos_y_bombas", "Cabina/Asiento_1", "Cabina/Asiento_2", "Oruga", "Helice", "Rampa_popa" };
        var lack = kit.Where(k => sub.transform.Find(k) == null).ToList();
        TestProbe.Check("equipado para urgencias de personas y de grandes animales terrestres", lack.Count == 0 && HospitalSubmarine.Bay.x >= 3.5f && HospitalSubmarine.Bay.z >= 7f,
            lack.Count == 0 ? $"bodega {HospitalSubmarine.Bay.x:0.0} × {HospitalSubmarine.Bay.y:0.0} × {HospitalSubmarine.Bay.z:0.0} m, {sub.bloodUnits} O−, {sub.fluidBags} sueros, {sub.oxygenLitres:0} L de O₂" : "falta " + string.Join(", ", lack));
        bool urgOk = HospitalSubmarine.FindFloor(HospitalService.Urgencias, out ArbolCampos ac, out TreeLevel urg, out _);
        bool vetOk = HospitalSubmarine.FindFloor(HospitalService.Veterinaria, out ArbolCampos acV, out TreeLevel vet, out _);
        TestProbe.Check("el hospital está en un árbol: pisos de urgencias y de veterinaria", urgOk && vetOk && ac == acV, $"{(urg != null ? urg.name : "?")} · {(vet != null ? vet.name : "?")}");
        if (!urgOk) { TestProbe.End(); yield break; }
        float park = Vector3.Distance(sub.parkPos, ac.World(ac.Ground.outside));
        TestProbe.Check("aparcado junto a la puerta del suelo de su árbol", park < 40f, $"a {park:0} m de la puerta del suelo de «{ac.building}»");

        // ── 2. UNA PERSONA ──
        Vector3? spot = IceSpot(ac.transform.position, sub.transform);
        TestProbe.Check("hay un sitio de hielo para el accidente (50–90 m del árbol)", spot.HasValue);
        if (!spot.HasValue) { TestProbe.End(); yield break; }
        GameObject body = WorkdayDirector.SpawnBodyCopy("Irosene", "Herida en el hielo (prueba)");
        body.transform.position = spot.Value;
        ChecklistProbeAnima pa = body.AddComponent<ChecklistProbeAnima>();
        BloodLoss bl = body.AddComponent<BloodLoss>(); bl.lost = 0.35f;
        yield return null;
        pa.sickness = 0.5f; pa.stress = 0.8f;
        float s0 = pa.sickness, lost0 = bl.lost;
        float old = Time.timeScale; Time.timeScale = 4f;
        bool went = sub.Dispatch(pa);
        float t0 = Time.realtimeSinceStartup;
        while (went && (sub.phase != HospitalSubmarine.Phase.Aparcado || !sub.delivered) && Time.realtimeSinceStartup - t0 < 480f) yield return null;
        Time.timeScale = old;
        TestProbe.Check("sale a por la persona (con el equipo del hospital)", went && sub.crew.Count >= 1, $"equipo: {string.Join(", ", sub.crew.Select(g => g != null ? g.name : "?"))}; {string.Join(" | ", sub.log.Take(4))}");
        ArbolCampos at = ArbolCampos.At(body.transform.position, out TreeLevel lvAt, out _, false);
        TestProbe.Check("la entrega en el piso de urgencias, en su puesto", sub.delivered && at == ac && lvAt == urg, $"{sub.deliveredTo}; está en {(lvAt != null ? lvAt.name : "?")} ({body.transform.position.y:0.0} m), a los {sub.deliveredAfter:0} s de juego");
        TestProbe.Check("por el camino le pone sangre O− y, en el hospital, el banco de sangre (deja la clase III)", bl.lost < 0.3f && bl.unitsReceived >= 1, $"pérdida {lost0 * 100f:0} % → {bl.lost * 100f:0} % ({bl.unitsReceived} unidades; {sub.transfusedOnBoard} a bordo)");
        TestProbe.Check("la atienden: estabilizada (menos enfermedad y estrés)", pa.sickness < s0 - 0.1f && pa.stress < 0.6f, $"enfermedad {s0:0.00} → {pa.sickness:0.00}, estrés {pa.stress:0.00}");
        TestProbe.Check("vuelve a aparcar", sub.phase == HospitalSubmarine.Phase.Aparcado && Vector3.Distance(sub.transform.position, sub.parkPos) < 3f, $"{sub.missionSeconds:0} s de juego en total ({(Time.realtimeSinceStartup - t0):0} s reales a ×4)");
        Debug.Log("[Submarino] Registro de la misión (persona): " + string.Join(" | ", sub.log));
        Destroy(body);

        // ── 3. UN OSO ──
        Animal bear = FindObjectsByType<Animal>(FindObjectsSortMode.None).Where(a => a != null && !a.death && (a.SpeciesName == "PolarBear" || a.SpeciesName == "Bear"))
            .OrderBy(a => Vector3.Distance(a.transform.position, sub.transform.position)).FirstOrDefault();
        TestProbe.Check("hay un oso en el santuario", bear != null);
        if (bear != null)
        {
            Vector3 home = bear.transform.position; Quaternion homeR = bear.transform.rotation;
            BloodLoss bb = bear.gameObject.AddComponent<BloodLoss>(); bb.lost = 0.25f;
            bear.sickness = 0.45f; bear.stress = 0.6f; float bs0 = bear.sickness;
            Time.timeScale = 4f;
            bool went2 = sub.Dispatch(bear);
            float t1 = Time.realtimeSinceStartup;
            while (went2 && (sub.phase != HospitalSubmarine.Phase.Aparcado || !sub.delivered) && Time.realtimeSinceStartup - t1 < 480f) yield return null;
            Time.timeScale = old;
            ArbolCampos atB = ArbolCampos.At(bear.transform.position, out TreeLevel lvB, out _, false);
            TestProbe.Check($"el oso ({bear.SpeciesName}) va en la bodega y lo entrega en el piso veterinario", went2 && sub.delivered && atB == ac && lvB == vet, $"{sub.deliveredTo}; está en {(lvB != null ? lvB.name : "?")}, a los {sub.deliveredAfter:0} s de juego");
            TestProbe.Check("al oso, fluidos (no sangre humana) y la consulta veterinaria", bear.sickness <= bs0 && sub.log.Any(l => l.Contains("trineo")), $"enfermedad {bs0:0.00} → {bear.sickness:0.00}; pérdida {bb.lost * 100f:0} %");
            TestProbe.Check("vuelve a aparcar", sub.phase == HospitalSubmarine.Phase.Aparcado, $"{sub.missionSeconds:0} s de juego");
            Debug.Log("[Submarino] Registro de la misión (oso): " + string.Join(" | ", sub.log));
            Destroy(bb);
            WorkdayDirector.Teleport(bear.transform, home); bear.transform.rotation = homeR;   // (la prueba lo devuelve a su sitio)
        }
        TestProbe.End();
    }

    /// <summary>Un sitio de hielo despejado a 50–90 m del árbol (para el accidente).</summary>
    public static Vector3? IceSpot(Vector3 axis, Transform ignore)
    {
        var hits = new RaycastHit[8];
        foreach (float r in new[] { 55f, 70f, 85f })
            for (int a = 0; a < 360; a += 20)
            {
                Vector3 p = axis + Quaternion.Euler(0f, a, 0f) * Vector3.forward * r;
                bool ok = true; float y = 0f;
                for (int k = 0; k < 5 && ok; k++)
                {
                    Vector3 q = p + (k == 0 ? Vector3.zero : Quaternion.Euler(0f, k * 90f, 0f) * Vector3.forward * 3f);
                    int n = Physics.RaycastNonAlloc(new Vector3(q.x, 30f, q.z), Vector3.down, hits, 40f, ~0, QueryTriggerInteraction.Ignore);
                    float top = float.NegativeInfinity;
                    for (int i = 0; i < n; i++) { if (ignore != null && hits[i].collider.transform.IsChildOf(ignore)) continue; top = Mathf.Max(top, hits[i].point.y); }
                    if (top < -0.3f || top > 0.6f) ok = false; else y = top;
                }
                if (ok) return new Vector3(p.x, y + 0.05f, p.z);
            }
        return null;
    }
}
