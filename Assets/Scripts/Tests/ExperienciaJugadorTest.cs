using System.Collections;
using UnityEngine;

/// <summary>
/// TEST DE EXPERIENCIA DEL JUGADOR (Berón 2026-10-26: «el más importante: emular lo que puede hacer el jugador y capturar
/// el resultado»). Juega el primer recorrido de punta a punta, como lo haría un jugador nuevo:
///  1. Entra al juego sin iniciar: el trabajador de turno va hacia él, lo lleva de la mano y le hace el examen de llegada
///     (los 10 pasos de <see cref="ArrivalExam"/>), sentado en 1.ª persona, con cada herramienta en la mano.
///  2. Lo mete en el tanque y lo envía al microcosmos de Ambrosio.
///  3. Ambrosio va hacia él y le pide ayuda; empuja a Shanidar; Shanidar se derrumba; toma la melaza de Ambrosio y se la
///     da; vuelve a empujar; llega el depredador y lo echa con 5 empujones; empuja hasta la cueva; Ambrosio muere; Sakshi
///     lo manda de vuelta; vuelve al mesocosmos y queda apto.
/// El jugador se emula como lo haría alguien con el teclado: se mueve junto a lo que toca (como andar) y pulsa E
/// (<see cref="AmbrosioStory.Act"/>). Va acelerado (×<see cref="speed"/>); para verlo a ritmo real, speed = 1.
/// Es el último grupo: deja el mundo cambiado (el jugador ya iniciado).
/// </summary>
public class ExperienciaJugadorTest : MonoBehaviour, ITestUnit
{
    public int Group => 40;
    public bool ParallelSafe => false;
    [Tooltip("Aceleración del tiempo durante el test (1 = ritmo real, para mirarlo).")]
    public float speed = 8f;

    GameObject _player;

    public IEnumerator Run()
    {
        TestProbe.Begin("Experiencia del jugador: iniciación → Ambrosio → vuelta");
        _player = GameObject.FindGameObjectWithTag("Player");
        SanctuaryOnboarding ob = _player != null ? _player.GetComponent<SanctuaryOnboarding>() : null;
        HealthRecord hr = _player != null ? _player.GetComponent<HealthRecord>() : null;
        GameObject hosp = GameObject.Find("Infirmary_Area");
        if (_player == null || ob == null || hr == null || hosp == null) { TestProbe.Check("hay jugador recién llegado y hospital", false); TestProbe.End(); yield break; }

        // ── Entra al juego: recién llegado, en el hospital ──
        foreach (NewcomerInitiator x in FindObjectsByType<NewcomerInitiator>(FindObjectsSortMode.None)) x.ResetState();   // nada a medias de antes
        NewcomerInitiator.AllowDuringTests = true;
        Orchestra.ResetStats();
        ob.apt = false; ob.initiated = false; hr.analyzed = false; hr.vaccinated = false;
        WorkdayDirector.Teleport(_player.transform, hosp.transform.TransformPoint(new Vector3(-11.8f, 1.3f, 3.6f)));
        float oldScale = Time.timeScale; Time.timeScale = speed;

        NewcomerInitiator ni = null;
        yield return Until(() => (ni = NewcomerInitiator.Active) != null && ni.Now == NewcomerInitiator.Phase.Approach, 10f);
        TestProbe.Check("al entrar, el trabajador de turno empieza la iniciación del recién llegado", ni != null && ni.InProgress, ni != null ? ni.Who : "nadie");
        if (ni == null) { Time.timeScale = oldScale; NewcomerInitiator.AllowDuringTests = false; TestProbe.End(); yield break; }

        float startGap = Dist(ni.transform, _player.transform);
        float closest = float.MaxValue; float limitA = Time.time + 120f;
        while (ni.Now == NewcomerInitiator.Phase.Approach && Time.time < limitA) { closest = Mathf.Min(closest, Vector3.Distance(ni.transform.position, _player.transform.position)); yield return null; }
        TestProbe.Check("el trabajador va hacia el recién llegado (no lo espera en la silla)", ni.GreetedAtDistance >= 0f && ni.GreetedAtDistance < 2f, $"de {startGap:0.0} m a {ni.GreetedAtDistance:0.0} m");

        // ── El examen de llegada, paso a paso ──
        string seen = ""; int tools = 0; bool firstPerson = false, seated = false;
        Transform chair = VirtualInPlace.FindDeep(hosp.transform, "Silla_examen");
        float limit = Time.time + 600f;
        // LA ORQUESTA (Berón 2026-10-29): nadie se pisa, nadie salta (salvo el teletransporte de emergencia registrado), las
        // piernas se mueven al andar y en el lavabotas los pies están en el suelo.
        var watch = new OrchestraWatch(_player.transform, ni.transform);
        float feetGap = float.NaN; string feetWhy = "no llegó a hacerse";
        while (ni.InProgress && ni.Now != NewcomerInitiator.Phase.ToTank && Time.time < limit)
        {
            watch.Sample(!ni.PatientDown);
            if (float.IsNaN(feetGap) && ni.Now == NewcomerInitiator.Phase.Perform && ni.CurrentBeat != null && ni.CurrentBeat.id == "botas")
                feetGap = FeetAboveFloor(_player.transform, out feetWhy);
            ArrivalExam.Beat b = ni.CurrentBeat;
            if (b != null && ni.Now == NewcomerInitiator.Phase.Perform && !seen.Contains("|" + b.id))
            {
                seen += "|" + b.id;
                if (string.IsNullOrEmpty(b.tool) || HasChild(ni.gameObject, "Herramienta")) tools++;
                if (b.patient == ArrivalExam.Spot.Sit && chair != null && Dist(chair, _player.transform) < 1.2f) seated = true;
                if (b.patient != ArrivalExam.Spot.Stand && CameraManager.Instance != null && CameraManager.Instance.CurrentMode == CameraMode.FirstPerson) firstPerson = true;
            }
            yield return null;
        }
        Debug.Log($"[Experiencia] fin del examen: InProgress={ni.InProgress} Now={ni.Now} tiempo={(limit - Time.time):0} s de margen");
        int steps = seen.Split('|').Length - 1;
        TestProbe.Check("el examen de llegada recorre sus 10 pasos en orden", steps == ArrivalExam.Beats.Length && seen.StartsWith("|expediente|botas|manos|temperatura|tension|auscultacion|hisopo|ecg|vacuna|apto"), seen);
        TestProbe.Check("en cada paso el trabajador tiene su herramienta en la mano", tools == steps, $"{tools}/{steps}");
        TestProbe.Check("el recién llegado se sienta en la silla de exámenes", seated);
        TestProbe.Check("sentado o tumbado, la cámara pasa a 1.ª persona", firstPerson || CameraManager.Instance == null);
        TestProbe.Check("tras el examen está analizado (frotis) y vacunado", hr.analyzed && hr.vaccinated);

        // ── Al tanque y al microcosmos de Ambrosio ── (la orquesta sigue hasta que lo mete dentro)
        float tankLimit = Time.time + 150f;
        while (!ob.initiated && ni.InProgress && Time.time < tankLimit) { watch.Sample(!ni.PatientDown); yield return null; }
        CheckOrchestra("examen de llegada", watch);
        TestProbe.Check("en el lavabotas los pies del recién llegado están en el suelo", !float.IsNaN(feetGap) && Mathf.Abs(feetGap) < 0.15f, feetWhy);
        yield return Until(() => MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 150f);   // ir al tanque (sin escaleras ni barreras, a veces rodea) + la secuencia del tanque
        TestProbe.Check("el trabajador lo mete en el tanque y aparece en el microcosmos", MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld);
        yield return Until(() => AmbrosioStory.Instance != null, 10f);
        AmbrosioStory st = AmbrosioStory.Instance;
        TestProbe.Check("es el microcosmos de Ambrosio, con su historia", st != null, NewcomerInitiator.FirstMicrocosmos);
        // LA FLORA NO SE PIERDE del suelo del área gigante (se repetía: Berón 2026-10-28).
        int flora = FloraOnAreaFloor(out string floraWhy);
        TestProbe.Check("en el microcosmos crece flora sobre el suelo del área gigante", flora >= 20, floraWhy);
        // …y crece en sus POSICIONES RESERVADAS (regla 8, PlatformSlots): la plantilla pone la flora del área en las plazas que
        // dejó su piso. Si el microcosmos deja de usarlas (o el piso cambia y ya no caben), baja de la mitad.
        float onSlots = FloraOnSlots(out string slotsWhy);
        TestProbe.Check("en el microcosmos la flora del área ocupa sus posiciones reservadas (≥ 50 %)", onSlots >= 0.5f, slotsWhy);
        if (st == null) { Time.timeScale = oldScale; NewcomerInitiator.AllowDuringTests = false; TestProbe.End(); yield break; }
        Vector3 toFam = st.Shanidar.position - _player.transform.position; toFam.y = 0f;
        TestProbe.Check("al aparecer, Ambrosio y su familia están delante", toFam.magnitude < 12f && Vector3.Dot(_player.transform.forward, toFam.normalized) > 0.3f, $"{toFam.magnitude:0.0} m");

        // Ambrosio viene a pedir ayuda.
        yield return Until(() => st.AskedAtDistance >= 0f, 30f);
        TestProbe.Check("Ambrosio va hacia el jugador a pedirle que salve a su familia", st.AskedAtDistance >= 0f && st.AskedAtDistance < 3.5f, $"a {st.AskedAtDistance:0.0} m");
        yield return Until(() => st.Now == AmbrosioStory.Beat.Push1, 30f);

        // Empuja a Shanidar un minuto.
        yield return Push(st, st.Shanidar);
        TestProbe.Check("el jugador empuja a Shanidar (E junto a él)", st.PlayerPushing);
        yield return Until(() => st.Now == AmbrosioStory.Beat.Collapse, AmbrosioStory.PushPhaseSeconds + 30f);
        TestProbe.Check("tras un minuto empujando, Shanidar se derrumba sin energía", st.Now == AmbrosioStory.Beat.Collapse && !st.PlayerPushing);

        // Toma la melaza de Ambrosio y se la da.
        yield return Press(st, st.Ambrosio);
        TestProbe.Check("toma la melaza de Ambrosio", st.CarryingHoneydew);
        yield return Press(st, st.Shanidar);
        TestProbe.Check("se la da a Shanidar y Shanidar se levanta", !st.CarryingHoneydew && st.Now == AmbrosioStory.Beat.Push2);

        // Sigue empujando: otro minuto y llega el depredador.
        yield return Push(st, st.Shanidar);
        yield return Until(() => st.Now == AmbrosioStory.Beat.Predator, AmbrosioStory.PushPhaseSeconds + 30f);
        TestProbe.Check("tras otro minuto llega un depredador y los demás van a por él", st.Now == AmbrosioStory.Beat.Predator && st.Predator != null);

        // Cinco empujones de 10 s.
        for (int i = 0; i < AmbrosioStory.PredatorPushesNeeded + 3 && st.Now == AmbrosioStory.Beat.Predator; i++)   // (si una E cae lejos, otra)
        {
            yield return Press(st, st.Predator);
            yield return Until(() => !st.PlayerPushingPredator, AmbrosioStory.PredatorPushSeconds + 10f);
            yield return null; yield return null;
        }
        TestProbe.Check("con 5 empujones de 10 s el depredador se va", st.PredatorPushes >= AmbrosioStory.PredatorPushesNeeded && st.Now == AmbrosioStory.Beat.Push3, $"{st.PredatorPushes} empujones");

        // El último minuto, hasta la cueva; Ambrosio muere; Sakshi lo manda de vuelta.
        yield return Push(st, st.Shanidar);
        yield return Until(() => st.Now == AmbrosioStory.Beat.Death, AmbrosioStory.PushPhaseSeconds + 30f);
        TestProbe.Check("empujando un minuto más llegan a la entrada de la cueva", st.Now == AmbrosioStory.Beat.Death && Dist(st.Shanidar, st.Cave) < 6f, $"{Dist(st.Shanidar, st.Cave):0.0} m de la cueva");
        yield return Until(() => st.Now == AmbrosioStory.Beat.Sakshi, 40f);   // (la despedida dura ~21 s: Medea, Atlas, Momo, Héspero)
        TestProbe.Check("Ambrosio se derrumba y muere en la entrada", st.Now == AmbrosioStory.Beat.Sakshi && Vector3.Angle(st.Ambrosio.up, Vector3.up) > 45f, $"inclinado {Vector3.Angle(st.Ambrosio.up, Vector3.up):0}°");   // cae de lado (90°) y Momo lo alza a medias (60°)
        yield return Until(() => MobWorldLoader.HasInstance && !MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 60f);
        TestProbe.Check("Sakshi se acerca y lo manda de vuelta: vuelve al mesocosmos", MobWorldLoader.HasInstance && !MobWorldLoader.Instance.IsInMobWorld);
        yield return Until(() => ob.apt, 15f);
        TestProbe.Check("de vuelta, queda apto (iniciación completa)", ob.apt);
        TestProbe.Check("el checklist de progreso marca la historia de Ambrosio", Progress.StoryDone(NewcomerInitiator.FirstMicrocosmos));

        Time.timeScale = oldScale; NewcomerInitiator.AllowDuringTests = false;
        TestProbe.End();
    }

    // ── El jugador, emulado: andar hasta algo y pulsar E ──
    IEnumerator Press(AmbrosioStory st, Transform target)
    {
        if (target == null) yield break;
        Vector3 d = _player.transform.position - target.position; d.y = 0f;
        Vector3 to = target.position + (d.sqrMagnitude > 0.01f ? d.normalized : Vector3.back) * 1.6f; to.y = _player.transform.position.y;
        WorkdayDirector.Teleport(_player.transform, to);   // (andaría hasta ahí)
        yield return null;
        Debug.Log($"[Experiencia] E junto a «{target.name}»: {st.Act()}");
        yield return null;
    }

    IEnumerator Push(AmbrosioStory st, Transform old)
    {
        if (st.PlayerPushing) yield break;
        yield return Press(st, old);
    }

    /// <summary>Plantas de «Flora_a_escala» sobre el suelo (Mat) del área gigante del microcosmos cargado: dentro de su planta y a
    /// menos de 2 m de su altura. Si una construcción descarta la flora del suelo (otra vez), esto baja a 0.</summary>
    public static int FloraOnAreaFloor(out string why)
    {
        Transform giant = null, flora = null;
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (giant == null && t.name.EndsWith("_gigante")) giant = t;
            if (flora == null && t.name == "Flora_a_escala") flora = t;
        }
        Transform mat = giant != null ? giant.Find("Mat") : null;
        if (giant == null || flora == null || mat == null) { why = $"área gigante {(giant != null)}, flora {(flora != null)}, suelo {(mat != null)}"; return 0; }
        Bounds b = mat.GetComponent<Renderer>().bounds; int n = 0;
        foreach (Transform p in flora)
            if (p.position.x > b.min.x && p.position.x < b.max.x && p.position.z > b.min.z && p.position.z < b.max.z && Mathf.Abs(p.position.y - b.max.y) < 2f) n++;
        why = $"{n} plantas sobre el suelo de «{giant.name}» ({flora.childCount} en total)";
        return n;
    }

    /// <summary>Fracción de las plazas de flora del área (la copia de <see cref="PlatformSlots"/> en el área gigante, ya ×10) que
    /// tienen una planta de «Flora_a_escala» a menos de 1 m (× la escala); no cuentan las que tapa la historia (su claro).</summary>
    public static float FloraOnSlots(out string why)
    {
        PlatformSlots ps = null; Transform flora = null;
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (ps == null && t.name.EndsWith("_gigante")) ps = t.GetComponent<PlatformSlots>();
            if (flora == null && t.name == "Flora_a_escala") flora = t;
        }
        if (ps == null || flora == null) { why = $"plazas en el área gigante {(ps != null)}, flora {(flora != null)} (¿microcosmos sin reconstruir?)"; return 0f; }
        float reach = 1f * ps.transform.lossyScale.x;
        int eligible = 0, planted = 0;
        for (int i = 0; i < ps.flora.Count; i++)
        {
            if (ps.floraUnderStory.Contains(i)) continue;
            eligible++;
            Vector3 w = ps.World(PlatformSlots.Kind.Flora, i);
            foreach (Transform p in flora)
            {
                Vector3 d = p.position - w; d.y = 0f;
                if (d.magnitude < reach) { planted++; break; }
            }
        }
        why = $"{planted}/{eligible} plazas de flora con planta en «{ps.name}» ({ps.floraUnderStory.Count} bajo la historia, {ps.flora.Count} en total)";
        return eligible > 0 ? planted / (float)eligible : 0f;
    }

    /// <summary>Mira dos participantes de una orquesta fotograma a fotograma: lo más cerca que han estado de pie, y los saltos
    /// (más de 1,5 m en un fotograma) que no sean un teletransporte de emergencia registrado.</summary>
    public class OrchestraWatch
    {
        readonly Transform _a, _b; Vector3 _pa, _pb; int _fallbacks; float _lastT;
        public float minGap = float.MaxValue; public int jumps; public string jumpWhy = "";
        public OrchestraWatch(Transform a, Transform b) { _a = a; _b = b; _pa = a.position; _pb = b.position; _fallbacks = Orchestra.Fallbacks; _lastT = Time.time; }
        public void Sample(bool bothStanding)
        {
            if (_a == null || _b == null) return;
            bool emergency = Orchestra.Fallbacks != _fallbacks; _fallbacks = Orchestra.Fallbacks;
            if (!emergency) { Jump(_a, _pa); Jump(_b, _pb); }
            _pa = _a.position; _pb = _b.position; _lastT = Time.time;
            if (!bothStanding) return;
            Vector3 d = _a.position - _b.position; d.y = 0f;
            if (Mathf.Abs(_a.position.y - _b.position.y) < 1.5f) minGap = Mathf.Min(minGap, d.magnitude);
        }
        void Jump(Transform t, Vector3 before)
        {
            float d = Vector3.Distance(t.position, before);
            // (Acelerado ×8 y con pocos fotogramas, un fotograma son hasta 2,7 s de juego: andando a paso ligero se avanzan
            // varios metros. Un salto es más de lo que da de sí andar: 1,5 m o 3,5 m/s × el fotograma, lo que sea mayor.)
            if (d <= Mathf.Max(1.5f, 3.5f * (Time.time - _lastT))) return;   // (desde la muestra anterior: el test a veces se salta fotogramas)
            jumps++;
            if (jumpWhy.Length < 200) jumpWhy += $"{t.name} {d:0.0} m; ";
            Debug.LogWarning($"[Experiencia] salto de «{t.name}»: {d:0.0} m en un fotograma, de {before} a {t.position}.");
        }
    }

    /// <summary>Altura de los pies (lo más bajo del cuerpo de cubos, sin herramientas) sobre el suelo que tiene debajo.</summary>
    public static float FeetAboveFloor(Transform who, out string why)
    {
        Transform body = who.Find(CubeHumanoid.BodyName);
        float feet = float.MaxValue;
        if (body != null)
            foreach (Renderer r in body.GetComponentsInChildren<Renderer>())
                if (r.enabled && !InTool(r.transform, body)) feet = Mathf.Min(feet, r.bounds.min.y);
        if (feet == float.MaxValue) { why = "sin cuerpo de cubos"; return float.NaN; }
        float floor = float.NegativeInfinity;
        Vector3 o = who.position + Vector3.up * 0.5f;
        foreach (RaycastHit h in Physics.RaycastAll(o, Vector3.down, 6f, ~0, QueryTriggerInteraction.Ignore))
            if (!h.transform.IsChildOf(who) && h.collider.GetComponentInParent<Anima>() == null && h.point.y <= feet + 0.6f && h.point.y > floor) floor = h.point.y;
        if (float.IsNegativeInfinity(floor)) { why = $"sin suelo bajo los pies (pies a y = {feet:0.00})"; return float.NaN; }
        why = $"pies a y = {feet:0.00}, suelo a y = {floor:0.00} ({feet - floor:+0.00;-0.00} m)";
        return feet - floor;
    }

    static bool InTool(Transform t, Transform stop) { for (; t != null && t != stop; t = t.parent) if (t.name.StartsWith("Herramienta")) return true; return false; }

    /// <summary>Las comprobaciones comunes de una ORQUESTA (examen, yoga, cocina, huerto): nadie se solapa, nadie salta ni
    /// es arrastrado, las piernas andan y ningún TRABAJADOR da vueltas en círculo (anduvo ≤ 1,6× la recta + 3 m, ≤ 2 medias
    /// vueltas de más de 120°; Berón 2026-10-29: «dan vueltas en círculo en el suelo»).
    public static void CheckOrchestra(string what, OrchestraWatch watch)
    {
        TestProbe.Check($"orquesta ({what}): no se solapan de pie (≥ 0,5 m)", watch.minGap >= 0.5f, $"mínimo {watch.minGap:0.00} m");
        TestProbe.Check($"orquesta ({what}): sin saltos ni teletransportes de emergencia ni empujones de otros", watch.jumps == 0 && Orchestra.Fallbacks == 0 && Orchestra.Displaced == 0, $"{watch.jumps} saltos ({watch.jumpWhy}), {Orchestra.Fallbacks} de emergencia, {Orchestra.Displaced} veces movido de su sitio");
        TestProbe.Check($"orquesta ({what}): al andar a cada sitio se mueven las piernas", Orchestra.WalksDone > 0 && Orchestra.WalksAnimated == Orchestra.WalksDone, $"{Orchestra.WalksAnimated}/{Orchestra.WalksDone} tramos con las piernas animadas");
        int workers = 0; string bad = "";
        foreach (Orchestra.WalkRecord w in Orchestra.Walks)
        {
            if (w.player) continue;
            workers++;
            if (Orchestra.Circled(w) && bad.Length < 300) bad += $"{w.who} «{w.label}»: {w.walked:0.0} m para {w.straight:0.0} en recta, {w.reversals} medias vueltas; ";
        }
        TestProbe.Check($"orquesta ({what}): ningún trabajador da vueltas (≤ 1,6× la recta + 3 m, ≤ 2 medias vueltas)", bad.Length == 0, bad.Length > 0 ? bad : $"{workers} tramos de trabajadores");
    }

    static IEnumerator Until(System.Func<bool> ok, float maxGameSeconds)
    {
        float end = Time.time + maxGameSeconds;
        while (!ok() && Time.time < end) yield return null;
    }

    static float Dist(Transform a, Transform b) { if (a == null || b == null) return float.MaxValue; Vector3 d = a.position - b.position; d.y = 0f; return d.magnitude; }

    static bool HasChild(GameObject g, string n) { foreach (Transform t in g.GetComponentsInChildren<Transform>()) if (t.name == n) return true; return false; }
}
