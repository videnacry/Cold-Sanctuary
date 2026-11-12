using System.Collections;
using UnityEngine;

/// <summary>
/// TEST DE LA INICIACIÓN DE LA SALA DE YOGA COMO ORQUESTA (Berón 2026-10-29): el trabajador de turno va hasta 2 m del recién
/// llegado; después cada uno anda SOLO a su sitio (el alumno a la esterilla, el maestro delante) y allí la clase de upa-yoga
/// de cuello (<see cref="UpaYoga.NeckSequence"/>). Comprueba: (1) en cada paso la CABEZA del maestro llega a la postura que
/// pide (barbilla al pecho, cara a un lado, oreja al hombro…), y la del alumno también — el jugador lanza cada paso y uno se
/// lo deja pasar: su cuerpo lo hace solo; (2) la cámara está en 3.ª persona durante la clase (no metida en la cabeza) y la
/// «V» la cambia; (3) la orquesta: no se solapan, no hay saltos ni teletransportes de emergencia y las piernas andan.
/// No entra al río (se corta al acabar la clase). Se añade solo al orquestador cuando se piden las pruebas.
/// </summary>
public class YogaInitiationTest : MonoBehaviour, ITestUnit
{
    public int Group => 42;   // tras la cocina (41)
    public bool ParallelSafe => false;
    public float speed = 8f;
    [Tooltip("Grados de margen para dar por hecha la postura de la cabeza en un paso.")]
    public float tolerance = 32f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<YogaInitiationTest>() == null) tr.gameObject.AddComponent<YogaInitiationTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Iniciación de la sala de yoga: orquesta → upa-yoga de cuello con cabeza y cámara");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        SanctuaryOnboarding ob = player != null ? player.GetComponent<SanctuaryOnboarding>() : null;
        Anima pa = player != null ? player.GetComponent<Anima>() : null;
        GameObject sala = GameObject.Find("SalaYoga");
        if (player == null || ob == null || pa == null || sala == null) { TestProbe.Check("hay jugador y sala de yoga", false); TestProbe.End(); yield break; }

        Magnate[] mags = FindObjectsByType<Magnate>(FindObjectsSortMode.None);
        KitchenBoss[] bosses = FindObjectsByType<KitchenBoss>(FindObjectsSortMode.None);
        foreach (Magnate m in mags) m.enabled = false;
        foreach (KitchenBoss b in bosses) b.enabled = false;
        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) { MobWorldLoader.Instance.ExitMobWorld(); yield return Until(() => !MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 20f); }
        foreach (AreaInitiator x in FindObjectsByType<AreaInitiator>(FindObjectsSortMode.None)) Destroy(x);
        foreach (NewcomerInitiator x in FindObjectsByType<NewcomerInitiator>(FindObjectsSortMode.None)) x.ResetState();   // (jugadas sueltas: el examen de llegada del arranque no se cruza)
        ob.apt = true;
        if (player.TryGetComponent(out HealthRecord hrec)) { hrec.analyzed = true; hrec.vaccinated = true; }   // (ya pasó el examen de llegada: la puerta de la enfermería no lo retiene)
        foreach (TeachingSession x in FindObjectsByType<TeachingSession>(FindObjectsSortMode.None)) Destroy(x);
        yield return null;

        ob.apt = ob.presented = ob.initiated = true;
        AreaInitiator.AllowDuringTests = true;
        float oldScale = Time.timeScale; Time.timeScale = speed;
        // El recién llegado, a unos metros de la esterilla (la Magnate lo dejaría en la puerta).
        // En la SALA (su piso), a unos metros de la esterilla: el primer punto del NavMesh a la misma altura (a 6 m ya quedaba
        // fuera del edificio y caía al hielo, 12 m más abajo).
        Vector3 start = sala.transform.position;
        foreach (float dist in new[] { 8f, 7f, 6f, 5f, 4f, 3f, 2f })
            if (UnityEngine.AI.NavMesh.SamplePosition(sala.transform.position + ArrivalExam.Front(sala.transform) * dist, out UnityEngine.AI.NavMeshHit h, 1f, UnityEngine.AI.NavMesh.AllAreas) && Mathf.Abs(h.position.y - sala.transform.position.y) < 1f) { start = h.position; break; }
        WorkdayDirector.Teleport(player.transform, PathMover.Of(player.transform).OnGround(start));
        CameraMode camBefore = CameraManager.Instance != null ? CameraManager.Instance.preferredMode : CameraMode.ThirdPerson;
        AreaInitiator ai = AreaInitiator.Begin(AreaInitiator.Kind.Yoga, ob, mags.Length > 0 ? mags[0].GetComponent<Anima>() : null);
        TestProbe.Check("un trabajador de la sala de yoga empieza la iniciación", ai != null, ai != null ? ai.Who : "nadie");
        if (ai == null) { Restore(oldScale, mags, bosses); TestProbe.End(); yield break; }


        // Hasta 2 m (andando): ahí empieza la orquesta.
        yield return Until(() => ai == null || (ai.Now != AreaInitiator.Phase.None && ai.Now != AreaInitiator.Phase.Approach), 400f);   // (None: aún no arrancó)
        float startGap = ai != null ? Flat(ai.transform.position - player.transform.position) : -1f;
        TestProbe.Check("el trabajador llega a 2 m del recién llegado antes de empezar", ai != null && startGap <= 2.3f, $"{startGap:0.0} m");
        Orchestra.ResetStats();
        var watch = new ExperienciaJugadorTest.OrchestraWatch(player.transform, ai != null ? ai.transform : player.transform);

        // La clase deja escrito cómo acabó cada paso (TeachingSession.Results): cuánto le faltaba a la cabeza del maestro y a
        // la del alumno al darlo por hecho. (Medirlo desde aquí, a ×8 y con pocos fotogramas, llegaba tarde: el paso ya era otro.)
        TeachingSession.Results.Clear();
        int lastIdx = -1, skip = 3;
        int classFrames = 0, thirdFrames = 0; bool vToFirst = false, vToThird = false, toggling = false;
        float limit = Time.time + 1500f;
        while (ai != null && AreaInitiator.Owns(ob) && ai.Now != AreaInitiator.Phase.ToTank && Time.time < limit)
        {
            watch.Sample(true);
            TeachingSession ts = TeachingSession.For(pa);
            if (ts != null)
            {
                if (ts.StepIndex != lastIdx)
                {
                    lastIdx = ts.StepIndex;
                    if (lastIdx != skip) ts.CastCurrentStep(pa);   // el jugador lanza el paso que le piden (uno se lo deja pasar)
                    // La «V» durante la clase: a 1.ª persona en un paso y de vuelta a 3.ª en el siguiente.
                    if (CameraManager.Instance != null && lastIdx == 6) { CameraManager.Instance.SwitchTo(CameraMode.FirstPerson); toggling = true; }
                    if (CameraManager.Instance != null && lastIdx == 7) { CameraManager.Instance.SwitchTo(CameraMode.ThirdPerson); toggling = false; }
                }
                if (CameraManager.Instance != null)
                {
                    if (toggling) { if (CameraManager.Instance.CurrentMode == CameraMode.FirstPerson) vToFirst = true; }
                    else
                    {
                        classFrames++;
                        if (lastIdx == 7 && CameraManager.Instance.CurrentMode == CameraMode.ThirdPerson && CameraOutsideHead(player)) vToThird = true;
                        if (CameraManager.Instance.CurrentMode == CameraMode.ThirdPerson && CameraOutsideHead(player)) thirdFrames++;
                    }
                }
            }
            yield return null;
        }
        int steps = 0, teacherOk = 0, studentOk = 0, autos = 0, capped = 0;
        string tBad = "", sBad = "";
        foreach (TeachingSession.StepResult s in TeachingSession.Results)
        {
            if (s.student != pa) continue;
            steps++;
            if (s.auto) autos++;
            if (s.timedOut) capped++;
            if (s.teacherOff >= 0f && s.teacherOff <= tolerance) teacherOk++; else if (tBad.Length < 240) tBad += $"#{s.index} ({s.teacherOff:0}°) ";
            if (s.studentOff >= 0f && s.studentOff <= tolerance) studentOk++; else if (sBad.Length < 240) sBad += $"#{s.index} ({s.studentOff:0}°) ";
        }
        int expected = UpaYoga.NeckSequence().Count;
        TestProbe.Check("la clase de upa-yoga de cuello recorre todos sus pasos (ninguno por el tope)", steps == expected && capped == 0, $"{steps}/{expected}, {capped} por el tope");
        TestProbe.Check("en cada paso la cabeza del MAESTRO hace lo que pide (barbilla, giro, oreja al hombro…)", steps > 0 && teacherOk == steps, $"{teacherOk}/{steps}{(tBad.Length > 0 ? " · fallan: " + tBad : "")}");
        TestProbe.Check("en cada paso la cabeza del ALUMNO también (lanzado o, si no lo lanza, solo)", steps > 0 && studentOk == steps, $"{studentOk}/{steps}{(sBad.Length > 0 ? " · fallan: " + sBad : "")}");
        TestProbe.Check("el paso que el jugador no lanzó lo hace su cuerpo solo al vencer el tiempo", autos == 1, $"{autos} automáticos");
        TestProbe.Check("durante la clase la cámara está en 3.ª persona (fuera de la cabeza)", CameraManager.Instance == null || (classFrames > 0 && thirdFrames >= classFrames * 0.95f), $"{thirdFrames}/{classFrames} fotogramas");
        TestProbe.Check("la «V» cambia la cámara durante la clase (a 1.ª y de vuelta a 3.ª)", CameraManager.Instance == null || (vToFirst && vToThird), $"a 1.ª {vToFirst}, a 3.ª {vToThird}");
        ExperienciaJugadorTest.CheckOrchestra("sala de yoga", watch);

        if (ai != null) ai.Cancel("prueba: la clase está comprobada, no va al río");
        yield return null;
        if (CameraManager.Instance != null) CameraManager.Instance.SwitchTo(camBefore, forced: true);
        Restore(oldScale, mags, bosses);
        TestProbe.End();
    }

    // En 3.ª persona la cámara queda detrás del cuerpo, no en los ojos (antes, con el cuello bloqueado por el yoga, se metía
    // en la cabeza aunque estuviera en 3.ª).
    static bool CameraOutsideHead(GameObject player)
    {
        Camera c = Camera.main;
        CreatureRig rig = player.GetComponent<CreatureRig>();
        Transform head = rig != null ? rig.Get(BodyPart.Head) : null;
        return c == null || head == null || Vector3.Distance(c.transform.position, head.position) > 0.8f;
    }

    static void Restore(float scale, Magnate[] mags, KitchenBoss[] bosses)
    {
        Time.timeScale = scale; AreaInitiator.AllowDuringTests = false;
        foreach (Magnate m in mags) if (m != null) m.enabled = true;
        foreach (KitchenBoss b in bosses) if (b != null) b.enabled = true;
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }

    static IEnumerator Until(System.Func<bool> cond, float seconds)
    {
        float end = Time.time + seconds;
        while (!cond() && Time.time < end) yield return null;
    }
}
