using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Arranca una VIRTUALIZACIÓN EN EL ÁREA REAL (la de la máquina): busca dónde se hace cada paso entre los muebles del
/// área, y monta lo propio de cada misión: en la enfermería, el TURISTA (una copia del cuerpo de alguien del santuario)
/// que te sigue y se sienta en la silla de exámenes con la postura de cada prueba; en el restaurante, el SERVICIO en la
/// cocina y el comedor de verdad (RestaurantServiceGame).
/// </summary>
public static class VirtualInPlace
{
    public static void Begin(WorkRoster.Mission m, Transform area, Transform machineFront)
    {
        if (m.id == "coc_servicio" && RestaurantServiceGame.StartInRestaurant(area, machineFront, m) != null) return;
        Transform[] anchors = FindAnchors(m.id, area, machineFront, VirtualMissions.Steps(m.id).Length);
        VirtualMissionRunner r = VirtualMissionRunner.StartInPlace(m, anchors, machineFront);
        if (m.id == "enf_iniciacion") SetupInitiation(r, area);
    }

    static Transform[] FindAnchors(string id, Transform area, Transform machineFront, int n)
    {
        var res = new Transform[n];
        string[] names = VirtualMissions.Anchors(id);
        var auto = AutoAnchors(area, machineFront);
        int a = 0;
        for (int i = 0; i < n; i++)
        {
            string nm = names != null && i < names.Length ? names[i] : null;
            Transform t = null;
            if (nm == "@maquina") t = machineFront;
            else if (nm == "@entrada") t = Entrance(area);
            else if (nm != null) t = FindDeep(area, nm);
            if (t == null && auto.Count > 0) t = auto[a++ % auto.Count];
            res[i] = t != null ? t : machineFront;
        }
        return res;
    }

    // Los muebles con nombre (WorldLabel) del área, repartidos alrededor de su centro (para pasos sin sitio fijo).
    static List<Transform> AutoAnchors(Transform area, Transform machineFront)
    {
        var l = new List<Transform>();
        foreach (WorldLabel wl in area.GetComponentsInChildren<WorldLabel>())
        {
            if (wl.transform == area || wl.GetComponentInParent<VirtualizationMachine>() != null) continue;
            l.Add(wl.transform);
        }
        l.Sort((x, y) => Angle(area, x).CompareTo(Angle(area, y)));
        if (l.Count > 8) { var spread = new List<Transform>(); for (int i = 0; i < 8; i++) spread.Add(l[i * l.Count / 8]); l = spread; }
        return l;
    }

    static float Angle(Transform area, Transform t) { Vector3 p = area.InverseTransformPoint(t.position); return Mathf.Atan2(p.z, p.x); }

    public static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    // La entrada del área: lo alto de su escalera (lado +Z), un poco hacia dentro.
    static Transform Entrance(Transform area)
    {
        Transform mat = area.Find("Mat");
        float hz = mat != null ? mat.localScale.z * 0.5f : 8f;
        var e = new GameObject("Entrada_virtual").transform;
        e.SetParent(area, false); e.localPosition = new Vector3(0f, 0.2f, hz - 2.5f);
        return e;
    }

    // ── INICIACIÓN: el turista (copia del cuerpo de alguien del santuario) y las posturas de cada prueba ──
    static void SetupInitiation(VirtualMissionRunner r, Transform area)
    {
        string[] who = { "Goluis", "Panterilia", "Gohageneis", "Irosene", "Andrea" };
        string src = who[Random.Range(0, who.Length)];
        GameObject tourist = WorkdayDirector.SpawnBodyCopy(src, "Turista (virtual)");
        tourist.transform.SetParent(r.transform, true);   // se va con la virtualización
        Transform entrance = r.stations[0].transform;
        tourist.transform.position = entrance.position + Vector3.right * 1.2f;
        var follow = tourist.AddComponent<VirtualFollower>();
        follow.enabled = false;   // espera en la entrada hasta que lo recibes
        Transform body = tourist.transform.Find(CubeHumanoid.BodyName);
        // La copia trae el cuerpo colocado para un origen a media altura (el de su dueño); el turista se pone por los pies
        // (en el suelo, la silla o la camilla), así que se le sube hasta que sus pies toquen su origen (si no, medio hundido).
        if (body != null)
        {
            Bounds bb = new Bounds(body.position, Vector3.zero); bool any = false;
            foreach (Renderer rd in body.GetComponentsInChildren<Renderer>()) { if (!any) { bb = rd.bounds; any = true; } else bb.Encapsulate(rd.bounds); }
            if (any) body.position += Vector3.up * (tourist.transform.position.y - bb.min.y);
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Anima me = player != null ? player.GetComponent<Anima>() : null;
        int[] plan = ArrivalExam.MissionPlan();   // los mismos pasos que el examen de llegada (NewcomerInitiator)

        // Cada prueba: el turista en su sitio (sentado, de pie o tumbado) con su postura, y tú con la tuya y la herramienta.
        void Perform(ArrivalExam.Beat b, Transform place)
        {
            follow.enabled = false;
            ArrivalExam.PatientSpot(place, b.patient, out Vector3 at, out Quaternion rot);
            tourist.transform.SetPositionAndRotation(at, rot);
            ClonePose.Apply(body, b.ptA());
            ArrivalExam.Hold(tourist, b.patientTool, b.patientBone, "Herramienta_paciente");
            if (me != null) { b.exA().ApplyFinal(me); r.StartCoroutine(StandLater(me, 2.5f)); }
            if (player != null) ArrivalExam.Hold(player, b.tool, BodyPart.HandRight, "Herramienta");
            Say($"{b.title}: {b.line}");
        }

        r.onStep = i =>
        {
            if (i < 0 || i >= plan.Length) return;
            int k = plan[i];
            Transform place = i < r.stations.Length ? r.stations[i].transform : entrance;
            if (k >= 0) { Perform(ArrivalExam.Beats[k], place); return; }
            ClonePose.Rest(body);
            ArrivalExam.Hold(tourist, null, BodyPart.HandRight, "Herramienta_paciente");
            if (player != null) ArrivalExam.Hold(player, null, BodyPart.HandRight, "Herramienta");
            if (k == ArrivalExam.StepEntrance) { follow.enabled = true; Say("El turista te sigue: empieza el examen de llegada."); }
            else if (k == ArrivalExam.StepAnalyzer)
            {
                follow.enabled = true;
                Say("El analizador procesa el frotis (PCR respiratoria: gripe, COVID, VRS): negativo. Sigue con el electrocardiograma.");
            }
            else { follow.enabled = false; Say("El turista entra en el tanque para su primera virtualización: ¡iniciación completada!"); }
        };
        r.onEnd = () => { if (tourist != null) Object.Destroy(tourist); if (player != null) ArrivalExam.Hold(player, null, BodyPart.HandRight, "Herramienta"); };
        Debug.Log($"[Iniciación] Virtualización en la enfermería real: el turista (cuerpo copiado de «{src}») espera en la entrada.");
    }

    static System.Collections.IEnumerator StandLater(Anima a, float s)
    {
        yield return new WaitForSeconds(s);
        if (a != null) DailyPoses.Stand().ApplyFinal(a);
    }

    static void Say(string s) { LessonPrompt.Show(s, null, 6f); Debug.Log("[Iniciación] " + s); }
}
