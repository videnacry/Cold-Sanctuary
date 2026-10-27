using System;
using System.Collections.Generic;
using UnityEngine;
using P = BodyPart;

/// <summary>
/// EL EXAMEN DE LLEGADA (docs/iniciacion-medica-investigacion.md, «Propuesta para el juego»): lo que de verdad se hace al
/// llegar a una base polar. La analítica, la radiografía y el dentista se hacen ANTES del viaje (USAP, BAS, AAD), así que
/// aquí no se saca sangre. Cada paso es una pequeña ANIMACIÓN: el sitio, la postura del que examina y la del examinado
/// (dos fotogramas que se alternan, A y B, para que se vea el gesto), la herramienta en la mano, lo que dice y cuánto dura.
/// Lo representa <see cref="NewcomerInitiator"/>; el test de experiencia (ExperienciaJugadorTest) lo recorre paso a paso.
/// </summary>
public static class ArrivalExam
{
    public enum Spot { Sit, Stand, Lie }

    public class Beat
    {
        public string id, title, place, line, protects;
        public Spot patient = Spot.Sit;
        public float seconds = 10f, period = 2f;   // cada «period» s se alterna el fotograma A/B
        public Func<AsanaPoseSpell> exA, exB, ptA, ptB;
        public string tool;                         // en la mano derecha del que examina
        public string patientTool; public P patientBone = P.HandRight;   // en el cuerpo del examinado (pinza, manguito…)
    }

    static AsanaPoseSpell M(string n, float root, params (P, Vector3)[] s) => DailyPoses.Make(n, root, s);
    static readonly (P, Vector3)[] SitLegs = { (P.ThighLeft, new Vector3(-90, 0, 0)), (P.ThighRight, new Vector3(-90, 0, 0)), (P.ShinLeft, new Vector3(90, 0, 0)), (P.ShinRight, new Vector3(90, 0, 0)) };
    static AsanaPoseSpell Seated(string n, params (P, Vector3)[] s) { var l = new List<(P, Vector3)>(SitLegs); l.AddRange(s); return M(n, 0.5f, l.ToArray()); }

    // ── Posturas del EXAMINADO ──
    static AsanaPoseSpell PtSit() => DailyPoses.Sit();
    static AsanaPoseSpell PtStand() => DailyPoses.Stand();
    static AsanaPoseSpell PtFootLeft() => M("levanta el pie izquierdo", -1f, (P.ThighLeft, new Vector3(-45, 0, 0)), (P.ShinLeft, new Vector3(80, 0, 0)), (P.ShoulderLeft, new Vector3(0, 0, 25)), (P.ShoulderRight, new Vector3(0, 0, -25)));
    static AsanaPoseSpell PtFootRight() => M("levanta el pie derecho", -1f, (P.ThighRight, new Vector3(-45, 0, 0)), (P.ShinRight, new Vector3(80, 0, 0)), (P.ShoulderLeft, new Vector3(0, 0, 25)), (P.ShoulderRight, new Vector3(0, 0, -25)));
    static AsanaPoseSpell PtWashA() => M("se frota las manos", -1f, (P.Back, new Vector3(10, 0, 0)), (P.ShoulderLeft, new Vector3(-50, 0, 12)), (P.ShoulderRight, new Vector3(-50, 0, -12)), (P.ForearmLeft, new Vector3(-40, 0, -25)), (P.ForearmRight, new Vector3(-40, 0, 25)));
    static AsanaPoseSpell PtWashB() => M("se frota las manos (dorso)", -1f, (P.Back, new Vector3(10, 0, 0)), (P.ShoulderLeft, new Vector3(-55, 0, 6)), (P.ShoulderRight, new Vector3(-45, 0, -6)), (P.ForearmLeft, new Vector3(-35, 0, -35)), (P.ForearmRight, new Vector3(-45, 0, 35)));
    static AsanaPoseSpell PtHandRest() => Seated("mano apoyada y quieta", (P.ShoulderRight, new Vector3(-30, 0, -10)), (P.ForearmRight, new Vector3(-60, 0, 0)), (P.Neck, new Vector3(0, 0, 18)));
    static AsanaPoseSpell PtArmHeart() => Seated("brazo a la altura del corazón", (P.ShoulderRight, new Vector3(-35, 0, -20)), (P.ForearmRight, new Vector3(-55, 0, 0)), (P.Back, new Vector3(-8, 0, 0)));
    static AsanaPoseSpell PtBreatheIn() => Seated("respira hondo", (P.Chest, new Vector3(-8, 0, 0)), (P.Neck, new Vector3(-6, 0, 0)), (P.ShoulderLeft, new Vector3(-20, 0, 10)), (P.ShoulderRight, new Vector3(-20, 0, -10)));
    static AsanaPoseSpell PtBreatheOut() => Seated("suelta el aire", (P.Chest, new Vector3(4, 0, 0)), (P.ShoulderLeft, new Vector3(-25, 0, 8)), (P.ShoulderRight, new Vector3(-25, 0, -8)));
    static AsanaPoseSpell PtHeadBack() => Seated("cabeza atrás (70°) y ojos cerrados", (P.Neck, new Vector3(-40, 0, 0)), (P.Head, new Vector3(-25, 0, 0)), (P.ShoulderLeft, new Vector3(-30, 0, 0)), (P.ShoulderRight, new Vector3(-30, 0, 0)));
    static AsanaPoseSpell PtLie() => DailyPoses.Lie();
    static AsanaPoseSpell PtShoulder() => DailyPoses.PatientShoulder();
    static AsanaPoseSpell PtHandshake() => M("le da la mano", -1f, (P.ShoulderRight, new Vector3(-55, 0, 0)), (P.ForearmRight, new Vector3(-15, 0, 0)));

    // ── Posturas del que EXAMINA ──
    static AsanaPoseSpell ExTabletA() => M("lee el expediente en la tablet", -1f, (P.Back, new Vector3(6, 0, 0)), (P.Neck, new Vector3(18, 0, 0)), (P.ShoulderLeft, new Vector3(-50, 0, 15)), (P.ShoulderRight, new Vector3(-50, 0, -15)), (P.ForearmLeft, new Vector3(-70, 0, 0)), (P.ForearmRight, new Vector3(-70, 0, 0)));
    static AsanaPoseSpell ExTabletB() => M("pregunta y anota", -1f, (P.Back, new Vector3(4, 0, 0)), (P.Neck, new Vector3(0, 0, 0)), (P.ShoulderLeft, new Vector3(-50, 0, 15)), (P.ShoulderRight, new Vector3(-62, 0, -10)), (P.ForearmLeft, new Vector3(-70, 0, 0)), (P.ForearmRight, new Vector3(-55, 0, 0)));
    static AsanaPoseSpell ExCrouchA() => M("en cuclillas, cepilla la bota", 0.55f, (P.ThighLeft, new Vector3(-110, 0, 10)), (P.ThighRight, new Vector3(-110, 0, -10)), (P.ShinLeft, new Vector3(120, 0, 0)), (P.ShinRight, new Vector3(120, 0, 0)), (P.Back, new Vector3(30, 0, 0)), (P.ShoulderRight, new Vector3(-45, 0, 0)), (P.ForearmRight, new Vector3(-20, 0, 0)), (P.ShoulderLeft, new Vector3(-40, 0, 10)));
    static AsanaPoseSpell ExCrouchB() => M("en cuclillas, cepillo hacia atrás", 0.55f, (P.ThighLeft, new Vector3(-110, 0, 10)), (P.ThighRight, new Vector3(-110, 0, -10)), (P.ShinLeft, new Vector3(120, 0, 0)), (P.ShinRight, new Vector3(120, 0, 0)), (P.Back, new Vector3(30, 0, 0)), (P.ShoulderRight, new Vector3(-20, 0, 0)), (P.ForearmRight, new Vector3(-45, 0, 0)), (P.ShoulderLeft, new Vector3(-40, 0, 10)));
    static AsanaPoseSpell ExThermo() => M("termómetro en el oído", -1f, (P.Back, new Vector3(10, 0, 0)), (P.ShoulderRight, new Vector3(-100, 0, -15)), (P.ForearmRight, new Vector3(-55, 0, 0)), (P.ShoulderLeft, new Vector3(-55, 0, 10)), (P.ForearmLeft, new Vector3(-50, 0, 0)));
    static AsanaPoseSpell ExReadOxi() => M("mira la pinza del dedo", -1f, (P.Back, new Vector3(22, 0, 0)), (P.Neck, new Vector3(15, 0, 0)), (P.ShoulderRight, new Vector3(-55, 0, -5)), (P.ForearmRight, new Vector3(-30, 0, 0)), (P.ShoulderLeft, new Vector3(-30, 0, 0)));
    static AsanaPoseSpell ExCuffA() => M("coloca el manguito", -1f, (P.Back, new Vector3(18, 0, 0)), (P.ShoulderLeft, new Vector3(-65, 0, 10)), (P.ShoulderRight, new Vector3(-65, 0, -10)), (P.ForearmLeft, new Vector3(-35, 0, 0)), (P.ForearmRight, new Vector3(-35, 0, 0)));
    static AsanaPoseSpell ExCuffB() => M("infla y mira el monitor", -1f, (P.Back, new Vector3(10, 0, 0)), (P.Neck, new Vector3(10, 0, 0)), (P.ShoulderRight, new Vector3(-40, 0, -10)), (P.ForearmRight, new Vector3(-95, 0, 0)), (P.ShoulderLeft, new Vector3(-60, 0, 10)), (P.ForearmLeft, new Vector3(-30, 0, 0)));
    static AsanaPoseSpell ExStethFront() => M("estetoscopio en el pecho", -1f, (P.Back, new Vector3(22, 0, 0)), (P.ShoulderRight, new Vector3(-75, 0, -5)), (P.ForearmRight, new Vector3(-25, 0, 0)), (P.ShoulderLeft, new Vector3(-30, 0, 15)), (P.Neck, new Vector3(10, 30, 0)));
    static AsanaPoseSpell ExStethBack() => M("estetoscopio en la espalda", -1f, (P.Back, new Vector3(25, 0, 0)), (P.Chest, new Vector3(0, -25, 0)), (P.ShoulderRight, new Vector3(-80, 0, -30)), (P.ForearmRight, new Vector3(-30, 0, 0)), (P.ShoulderLeft, new Vector3(-50, 0, 20)));
    static AsanaPoseSpell ExSwabA() => M("sujeta la frente e introduce el hisopo", -1f, (P.Back, new Vector3(14, 0, 0)), (P.ShoulderLeft, new Vector3(-100, 0, 20)), (P.ForearmLeft, new Vector3(-30, 0, 0)), (P.ShoulderRight, new Vector3(-85, 0, -10)), (P.ForearmRight, new Vector3(-35, 0, 0)));
    static AsanaPoseSpell ExSwabB() => M("gira el hisopo", -1f, (P.Back, new Vector3(14, 0, 0)), (P.ShoulderLeft, new Vector3(-100, 0, 20)), (P.ForearmLeft, new Vector3(-30, 0, 0)), (P.ShoulderRight, new Vector3(-85, 0, -10)), (P.ForearmRight, new Vector3(-35, 0, 0)), (P.HandRight, new Vector3(0, 0, 60)));
    static AsanaPoseSpell ExElectrodes() => M("pega los electrodos", -1f, (P.Back, new Vector3(40, 0, 0)), (P.ShoulderLeft, new Vector3(-70, 0, 10)), (P.ShoulderRight, new Vector3(-70, 0, -10)), (P.ForearmLeft, new Vector3(-20, 0, 0)), (P.ForearmRight, new Vector3(-20, 0, 0)));
    static AsanaPoseSpell ExEcgWatch() => M("mira el trazado", -1f, (P.Back, new Vector3(8, 0, 0)), (P.Neck, new Vector3(5, -30, 0)), (P.ShoulderRight, new Vector3(-50, 0, -10)), (P.ForearmRight, new Vector3(-40, 0, 0)));
    static AsanaPoseSpell ExSyringe() => DailyPoses.ExaminerSyringe();
    static AsanaPoseSpell ExGauze() => M("presiona con la gasa", -1f, (P.Back, new Vector3(10, 0, 0)), (P.ShoulderRight, new Vector3(-75, 0, 10)), (P.ForearmRight, new Vector3(-50, 0, 0)), (P.ShoulderLeft, new Vector3(-30, 0, 0)));
    static AsanaPoseSpell ExHandshake() => M("da la mano y la credencial", -1f, (P.ShoulderRight, new Vector3(-55, 0, 0)), (P.ForearmRight, new Vector3(-15, 0, 0)), (P.ShoulderLeft, new Vector3(-45, 0, 10)), (P.ForearmLeft, new Vector3(-45, 0, 0)));

    /// <summary>Los pasos, en orden (docs/iniciacion-medica-investigacion.md).</summary>
    public static readonly Beat[] Beats =
    {
        new Beat { id = "expediente", title = "Expediente y cuestionario", place = "Silla_examen", patient = Spot.Sit, seconds = 30f, period = 3f, tool = "Tablet",
            exA = ExTabletA, exB = ExTabletB, ptA = PtSit, ptB = PtSit, protects = "recién llegado y personal",
            line = "Tu expediente médico llegó antes que tú: la analítica, la radiografía y el dentista se hicieron antes del viaje. ¿Fiebre, tos o diarrea estos días? ¿Alergias? ¿Tétanos, gripe y triple vírica al día?" },
        new Beat { id = "botas", title = "Biocontrol: botas y ropa", place = "Lavabotas", patient = Spot.Stand, seconds = 45f, period = 2.5f, tool = "Cepillo",
            exA = ExCrouchA, exB = ExCrouchB, ptA = PtFootLeft, ptB = PtFootRight, protects = "fauna y flora nativas",
            line = "Ahora, las botas: una semilla o un poco de tierra de fuera puede traer plantas, hongos o bichos que aquí no existen. Levanta un pie… y el otro. Revisamos también el velcro y los bolsillos." },
        new Beat { id = "manos", title = "Lavado de manos", place = "Lavamanos", patient = Spot.Stand, seconds = 20f, period = 1.5f,
            exA = PtWashA, exB = PtWashB, ptA = PtWashA, ptB = PtWashB, protects = "personal y fauna",
            line = "Manos: jabón, veinte segundos, entre los dedos y el dorso. Yo también." },
        new Beat { id = "temperatura", title = "Temperatura y saturación", place = "Silla_examen", patient = Spot.Sit, seconds = 20f, period = 4f, tool = "Termometro", patientTool = "Pulsioximetro", patientBone = P.HandRight,
            exA = ExThermo, exB = ExReadOxi, ptA = PtHandRest, ptB = PtHandRest, protects = "personal (y fauna, si hubiera fiebre)",
            line = "Termómetro en el oído… 36,6 °C. Y la pinza en el dedo: saturación 98 %, pulso 68." },
        new Beat { id = "tension", title = "Tensión arterial", place = "Silla_examen", patient = Spot.Sit, seconds = 40f, period = 5f, tool = "Pera", patientTool = "Manguito", patientBone = P.ShoulderRight,
            exA = ExCuffA, exB = ExCuffB, ptA = PtArmHeart, ptB = PtArmHeart, protects = "recién llegado",
            line = "La espalda apoyada y el brazo a la altura del corazón, sin hablar… Inflo el manguito… 118/76: perfecto." },
        new Beat { id = "auscultacion", title = "Auscultación de corazón y pulmones", place = "Silla_examen", patient = Spot.Sit, seconds = 45f, period = 4.5f, tool = "Estetoscopio",
            exA = ExStethFront, exB = ExStethBack, ptA = PtBreatheIn, ptB = PtBreatheOut, protects = "recién llegado y personal",
            line = "Respira hondo por la boca… y suelta. Otra vez, por la espalda. Corazón y pulmones limpios." },
        new Beat { id = "hisopo", title = "Frotis nasal (PCR respiratoria)", place = "Silla_examen", patient = Spot.Sit, seconds = 15f, period = 2f, tool = "Hisopo",
            exA = ExSwabA, exB = ExSwabB, ptA = PtHeadBack, ptB = PtHeadBack, protects = "personal y fauna (gripe aviar, zoonosis inversa)",
            line = "Cabeza atrás y ojos cerrados: el hisopo por la nariz, unas vueltas… listo. Si tuvieras gripe, aquí podrías contagiar a pingüinos y focas." },
        new Beat { id = "ecg", title = "Electrocardiograma", place = "Camilla", patient = Spot.Lie, seconds = 60f, period = 6f, tool = "Electrodo", patientTool = "Electrodos", patientBone = P.Chest,
            exA = ExElectrodes, exB = ExEcgWatch, ptA = PtLie, ptB = PtLie, protects = "recién llegado",
            line = "Túmbate boca arriba y quieto: diez electrodos… el trazado sale regular, ritmo sinusal." },
        new Beat { id = "vacuna", title = "Vacuna en el deltoides", place = "Silla_examen", patient = Spot.Sit, seconds = 15f, period = 5f, tool = "Jeringa",
            exA = ExSyringe, exB = ExGauze, ptA = PtShoulder, ptB = PtShoulder, protects = "recién llegado y personal",
            line = "La gripe de esta temporada, en el hombro: a 90°, un pinchazo… y presiono con la gasa. Quédate un momento sentado." },
        new Beat { id = "apto", title = "Apto: credencial y tres normas", place = "Mesa del medico", patient = Spot.Stand, seconds = 20f, period = 10f, tool = "Credencial",
            exA = ExHandshake, exB = ExHandshake, ptA = PtHandshake, ptB = PtHandshake, protects = "fauna y flora",
            line = "Apto. Tres normas: a cinco metros de los animales; si enfermas, no te acerques a ellos; y botas limpias siempre. Ahora, tu primera virtualización." },
    };

    // ── LA MISIÓN DE VIRTUALIZACIÓN «enf_iniciacion» (el jugador examina a un turista): el MISMO examen ──────────
    // Pasos = recibirlo, los pasos del examen (con el frotis llevado al analizador justo tras tomarlo) y el tanque.
    public const int StepEntrance = -1, StepAnalyzer = -2, StepTank = -3;

    /// <summary>Qué hace cada paso de la misión: el índice del paso del examen, o StepEntrance/StepAnalyzer/StepTank.</summary>
    public static int[] MissionPlan()
    {
        var l = new List<int> { StepEntrance };
        for (int i = 0; i < Beats.Length; i++) { l.Add(i); if (Beats[i].id == "hisopo") l.Add(StepAnalyzer); }
        l.Add(StepTank);
        return l.ToArray();
    }

    public static string[] MissionSteps() => Map(MissionPlan(), i => i >= 0 ? Beats[i].title : i == StepEntrance ? "Recibir al turista en la entrada"
        : i == StepAnalyzer ? "Llevar el frotis al analizador" : "Acompañarle al tanque: su primera virtualización");

    public static string[] MissionAnchors() => Map(MissionPlan(), i => i >= 0 ? Beats[i].place : i == StepEntrance ? "@entrada" : i == StepAnalyzer ? "Analizador clinico" : "@maquina");

    static string[] Map(int[] plan, Func<int, string> f) { var r = new string[plan.Length]; for (int i = 0; i < plan.Length; i++) r[i] = f(plan[i]); return r; }

    /// <summary>El lado «de uso» de un mueble: de sus cuatro lados, el que da al interior del área (hacia su centro), que es
    /// por donde se le acerca uno. No depende de cómo se construyó cada mueble.</summary>
    public static Vector3 Front(Transform place)
    {
        SanctuaryArea a = place.GetComponentInParent<SanctuaryArea>();
        Vector3 toC = Flat(a != null ? a.transform.position - place.position : -place.forward).normalized;
        Vector3 best = place.forward; float bd = -2f;
        foreach (Vector3 c in new[] { place.forward, -place.forward, place.right, -place.right }) { Vector3 f = Flat(c).normalized; float d = Vector3.Dot(f, toC); if (d > bd) { bd = d; best = f; } }
        return best;
    }

    /// <summary>Dónde y cómo queda el examinado en el sitio del paso: sentado en la silla, tumbado en la camilla o de pie delante.
    /// <paramref name="who"/> (opcional) es el examinado: su escala ajusta la altura de la cadera al sentarse.</summary>
    public static void PatientSpot(Transform place, Spot spot, out Vector3 at, out Quaternion rot, Transform who = null)
    {
        Vector3 fr = Front(place);
        if (spot == Spot.Stand) { at = place.position + fr * 0.7f; rot = Quaternion.LookRotation(-fr); return; }
        // Escala del cuerpo de cubos (1,9 u nativas) y desfase de su raíz: 0,974 es el de 1,85 m (docs/medidas-muebles.md).
        Transform body = who != null ? who.Find(CubeHumanoid.BodyName) : null;
        float s = body != null ? body.lossyScale.y : 0.974f, dy = body != null ? body.position.y - who.position.y : 0f;
        if (spot == Spot.Lie)
        {
            // Tumbado (Lie: cadera a 0,12 u y girada −90°): la espalda apoya en la superficie del acolchado, sin hundirse.
            at = new Vector3(place.position.x, TopOf(place, "Acolchado", place.position.y + 0.8f) - dy, place.position.z);
            rot = place.rotation;
            return;
        }
        // Sentado (Sit: cadera a 0,5 u): la cadera queda 0,12·s sobre el asiento, 0,10 m hacia el respaldo, y de cara al
        // frente de la silla (su +z local: el respaldo de «Silla_examen» está en −z), no hacia el centro del área.
        Vector3 seatFr = Flat(place.forward).normalized;
        Transform seat = FindPart(place, "Asiento");
        Vector3 c = seat != null ? seat.position : place.position;
        float hip = TopOf(place, "Asiento", place.position.y + 0.45f) + SitButt * s;
        at = new Vector3(c.x, hip - SitHip * s - dy, c.z) - seatFr * 0.10f;
        rot = Quaternion.LookRotation(seatFr);
    }

    const float SitHip = 0.5f, SitButt = 0.12f;   // Sit: cadera a 0,5 u de la raíz; de la cadera a la nalga, 0,12 u

    static Transform FindPart(Transform place, string prefix)
    {
        foreach (Transform t in place.GetComponentsInChildren<Transform>(true)) if (t != place && t.name.StartsWith(prefix)) return t;
        return null;
    }

    /// <summary>Altura (mundo) de la cara de arriba de la pieza <paramref name="prefix"/> del mueble, por su renderer (o su
    /// escala si no lo tiene); <paramref name="fallback"/> si el mueble no la tiene.</summary>
    static float TopOf(Transform place, string prefix, float fallback)
    {
        Transform p = FindPart(place, prefix);
        if (p == null) return fallback;
        return p.TryGetComponent(out Renderer r) && r.enabled ? r.bounds.max.y : p.position.y + p.lossyScale.y * 0.5f;
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v : Vector3.forward; }

    // ── HERRAMIENTAS (de cubos, a su tamaño real) ──────────────────────────────────────────────────────────────

    /// <summary>Pone la herramienta <paramref name="tool"/> en el hueso <paramref name="bone"/> de <paramref name="body"/> (o la
    /// quita si es null). Devuelve el objeto creado.</summary>
    public static GameObject Hold(GameObject body, string tool, P bone, string slot)
    {
        if (body == null) return null;
        Transform b = Bone(body.transform, bone);
        foreach (Transform t in body.GetComponentsInChildren<Transform>(true)) if (t.name == slot) { UnityEngine.Object.Destroy(t.gameObject); break; }
        if (string.IsNullOrEmpty(tool) || b == null) return null;
        var root = new GameObject(slot);
        root.transform.SetParent(b, false);
        root.transform.localPosition = bone == P.Chest ? new Vector3(0f, 0.1f, 0.13f) : bone == P.ShoulderRight ? new Vector3(0f, -0.15f, 0f) : new Vector3(0f, -0.08f, 0.04f);
        Build(root.transform, tool);
        return root;
    }

    public static Transform Bone(Transform body, P part)
    {
        foreach (CubeJoint j in body.GetComponentsInChildren<CubeJoint>(true)) if (j.part == part) return j.transform;
        return null;
    }

    static void Build(Transform r, string tool)
    {
        Color steel = new Color(0.75f, 0.77f, 0.8f), dark = new Color(0.15f, 0.16f, 0.18f), white = new Color(0.95f, 0.95f, 0.95f), blue = new Color(0.25f, 0.45f, 0.8f);
        switch (tool)
        {
            case "Tablet": Part(r, Vector3.zero, new Vector3(0.25f, 0.012f, 0.17f), dark); Part(r, new Vector3(0f, 0.007f, 0f), new Vector3(0.22f, 0.002f, 0.14f), new Color(0.5f, 0.75f, 0.95f)); break;
            case "Cepillo": Part(r, new Vector3(0f, -0.08f, 0f), new Vector3(0.03f, 0.18f, 0.03f), new Color(0.8f, 0.3f, 0.2f)); Part(r, new Vector3(0f, -0.2f, 0f), new Vector3(0.09f, 0.05f, 0.05f), new Color(0.3f, 0.3f, 0.3f)); break;
            case "Termometro": Part(r, Vector3.zero, new Vector3(0.035f, 0.15f, 0.03f), white); Part(r, new Vector3(0f, 0.08f, 0f), new Vector3(0.015f, 0.03f, 0.015f), blue); break;
            case "Pulsioximetro": Part(r, Vector3.zero, new Vector3(0.035f, 0.03f, 0.06f), new Color(0.2f, 0.45f, 0.8f)); Part(r, new Vector3(0f, 0.017f, 0f), new Vector3(0.025f, 0.004f, 0.03f), new Color(0.2f, 0.9f, 0.4f)); break;
            case "Pera": Part(r, Vector3.zero, new Vector3(0.06f, 0.08f, 0.06f), dark); break;
            case "Manguito": Part(r, Vector3.zero, new Vector3(0.13f, 0.14f, 0.13f), new Color(0.2f, 0.3f, 0.55f)); break;
            case "Estetoscopio": Part(r, Vector3.zero, new Vector3(0.04f, 0.015f, 0.04f), steel); Part(r, new Vector3(0f, 0.12f, 0f), new Vector3(0.008f, 0.25f, 0.008f), dark); break;
            case "Hisopo": Part(r, new Vector3(0f, 0f, 0.07f), new Vector3(0.004f, 0.004f, 0.15f), white); break;
            case "Electrodo": Part(r, Vector3.zero, new Vector3(0.03f, 0.005f, 0.03f), white); break;
            case "Electrodos": for (int i = 0; i < 6; i++) Part(r, new Vector3(-0.1f + i * 0.04f, (i % 2) * 0.04f, 0f), new Vector3(0.025f, 0.025f, 0.004f), white); break;
            case "Jeringa": Part(r, Vector3.zero, new Vector3(0.012f, 0.012f, 0.08f), white); Part(r, new Vector3(0f, 0f, 0.06f), new Vector3(0.003f, 0.003f, 0.025f), steel); break;
            case "Credencial": Part(r, Vector3.zero, new Vector3(0.085f, 0.054f, 0.004f), new Color(0.95f, 0.85f, 0.4f)); break;
        }
    }

    static void Part(Transform r, Vector3 pos, Vector3 size, Color c)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        UnityEngine.Object.Destroy(g.GetComponent<Collider>());
        g.name = "pieza"; g.transform.SetParent(r, false); g.transform.localPosition = pos; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
    }
}
