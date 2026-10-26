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
        TestProbe.Check("el trabajador va hacia el recién llegado (no lo espera en la silla)", closest < 2f && ni.Now == NewcomerInitiator.Phase.Escort, $"de {startGap:0.0} m a {closest:0.0} m");

        // ── El examen de llegada, paso a paso ──
        string seen = ""; int tools = 0; bool firstPerson = false, seated = false;
        Transform chair = VirtualInPlace.FindDeep(hosp.transform, "Silla_examen");
        float limit = Time.time + 600f;
        while (ni.InProgress && ni.Now != NewcomerInitiator.Phase.ToTank && Time.time < limit)
        {
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
        int steps = seen.Split('|').Length - 1;
        TestProbe.Check("el examen de llegada recorre sus 10 pasos en orden", steps == ArrivalExam.Beats.Length && seen.StartsWith("|expediente|botas|manos|temperatura|tension|auscultacion|hisopo|ecg|vacuna|apto"), seen);
        TestProbe.Check("en cada paso el trabajador tiene su herramienta en la mano", tools == steps, $"{tools}/{steps}");
        TestProbe.Check("el recién llegado se sienta en la silla de exámenes", seated);
        TestProbe.Check("sentado o tumbado, la cámara pasa a 1.ª persona", firstPerson || CameraManager.Instance == null);
        TestProbe.Check("tras el examen está analizado (frotis) y vacunado", hr.analyzed && hr.vaccinated);

        // ── Al tanque y al microcosmos de Ambrosio ──
        yield return Until(() => MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 60f);
        TestProbe.Check("el trabajador lo mete en el tanque y aparece en el microcosmos", MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld);
        yield return Until(() => AmbrosioStory.Instance != null, 10f);
        AmbrosioStory st = AmbrosioStory.Instance;
        TestProbe.Check("es el microcosmos de Ambrosio, con su historia", st != null, NewcomerInitiator.FirstMicrocosmos);
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
        for (int i = 0; i < AmbrosioStory.PredatorPushesNeeded && st.Now == AmbrosioStory.Beat.Predator; i++)
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
        yield return Until(() => st.Now == AmbrosioStory.Beat.Sakshi, 20f);
        TestProbe.Check("Ambrosio se derrumba y muere en la entrada", st.Now == AmbrosioStory.Beat.Sakshi && Vector3.Angle(st.Ambrosio.up, Vector3.up) > 90f);
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

    static IEnumerator Until(System.Func<bool> ok, float maxGameSeconds)
    {
        float end = Time.time + maxGameSeconds;
        while (!ok() && Time.time < end) yield return null;
    }

    static float Dist(Transform a, Transform b) { if (a == null || b == null) return float.MaxValue; Vector3 d = a.position - b.position; d.y = 0f; return d.magnitude; }

    static bool HasChild(GameObject g, string n) { foreach (Transform t in g.GetComponentsInChildren<Transform>()) if (t.name == n) return true; return false; }
}
