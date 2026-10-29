using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// CLASE DE YOGA EN VIVO (Berón 2026-10-29): en la Sala de Yoga, un maestro del personal sube a la «Tarima del maestro» y
/// guía el Saludo al Sol con <see cref="YogaClass"/>; al menos 4 practicantes andan cada uno a SU esterilla (alfombras
/// «Esterilla_N» de la sala) y repiten cada paso. Comprueba: cada practicante llega andando a una esterilla DISTINTA y se
/// queda en ella mirando al maestro; el maestro se queda en la tarima; en cada paso, los huesos de todos los practicantes
/// coinciden con la postura del maestro (la del paso y la de su cuerpo, como <see cref="TeachingSession.PoseMatch"/>)
/// en pocos segundos; y nadie se teletransporta (ningún fotograma recorre más de lo que se anda, salvo en alfombra).
/// Se añade solo al orquestador cuando se piden las pruebas (tras el gimnasio). Sola: «Tools/Cold Sanctuary/Pruebas/Clase
/// de yoga (sola)».
/// </summary>
public class YogaClassTest : MonoBehaviour, ITestUnit
{
    public int Group => 51;   // tras el gimnasio (50)
    public bool ParallelSafe => false;
    public float speed = 4f;
    public int practitioners = 4;
    [Tooltip("Tope (s de juego) para que todos lleguen (pueden venir de otro piso o edificio en alfombra).")]
    public float gatherLimit = 240f;
    [Tooltip("Margen (s de juego) para copiar un paso, además de lo que tarda la postura en llegar (reachFraction × duración).")]
    public float copyLimit = 3f;
    public float tolerance = 30f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<YogaClassTest>() == null) tr.gameObject.AddComponent<YogaClassTest>();
    }

    class Who
    {
        public Anima a; public Vector3 pos; public Quaternion rot; public Vector3 last;
        public IdleBehavior idle; public bool idleWas; public ActionQueue q; public bool qWas;
        public UnityEngine.AI.NavMeshAgent agent; public bool agentWas;
        public CarnivoreKeeper keeper; public bool keeperWas;
        public float maxJump; public bool rode;
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Clase de yoga: maestro en la tarima, cada practicante en su esterilla, repiten los pasos");
        if (YogaClass.Room == null || YogaClass.Tarima == null)
        { TestProbe.Check("hay Sala de Yoga con la tarima del maestro", false); TestProbe.End(); yield break; }
        List<CarpetPad> mats = YogaClass.Mats();
        TestProbe.Check($"la sala tiene esterillas para {practitioners} practicantes", mats.Count >= practitioners, $"{mats.Count} esterillas");

        float mEnd = Time.realtimeSinceStartup + 120f;   // que el personal ya sea del santuario (al hacerse miembro lo colocan de golpe en su área)
        while (Time.realtimeSinceStartup < mEnd && (FindObjectsByType<Anima>(FindObjectsSortMode.None).Count(a0 => a0 != null && !a0.CompareTag("Player") && Magnate.IsMember(a0) && a0.GetComponent<CreatureRig>() != null) < practitioners + 2 || GymUsabilityTest.Arriving()))
            yield return new WaitForSecondsRealtime(1f);
        yield return new WaitForSecondsRealtime(1f);
        WorkdayDirector wd = WorkdayDirector.Instance;
        string[] names = wd != null ? wd.staff : new[] { "Linduarda", "Lizmibet", "Goluis", "Panterilia", "Gohageneis", "Irosene", "JefeConstruccion", "Sadhguru", "Andrea" };
        var people = new List<Anima>();
        foreach (string n in names)
        {
            GameObject g = WorkdayDirector.Body(n);
            if (g == null || g.CompareTag("Player") || g.GetComponent<Magnate>() != null || g.GetComponent<CreatureRig>() == null) continue;
            if (g.TryGetComponent(out Linduarda lin) && lin.enabled) continue;
            if (g.TryGetComponent(out NewcomerInitiator ni) && ni.InProgress) continue;
            if (RestaurantService.Drives(g) || Orchestra.Busy(g)) continue;   // los lleva el servicio del restaurante o una orquesta
            Anima a = g.GetComponent<Anima>(); if (a != null && Magnate.IsMember(a)) people.Add(a);   // (quien aún no es miembro: al serlo, SanctuaryDirector lo coloca de golpe en su área)
        }
        TestProbe.Check($"hay maestro y {practitioners} practicantes en el personal", people.Count >= practitioners + 1, string.Join(", ", people.Select(p => p.name)));
        if (people.Count < 2) { TestProbe.End(); yield break; }
        // El maestro: Sadhguru si está (el yogui del santuario); si no, el primero.
        Anima teacher = people.FirstOrDefault(p => p.name == "Sadhguru") ?? people[0];
        List<Anima> studs = people.Where(p => p != teacher).Take(practitioners).ToList();

        WorkdayDirector.LogTeleports = true;
        bool wdWas = wd != null && wd.enabled; if (wd != null) wd.enabled = false;
        float oldScale = Time.timeScale; Time.timeScale = speed;
        if (YogaClass.Current != null) YogaClass.Current.End();
        yield return null;
        var all = new List<Anima> { teacher }; all.AddRange(studs);
        var who = all.ToDictionary(a => a, Hold);

        YogaClass c = YogaClass.Begin(teacher, "Saludo al Sol");
        TestProbe.Check("empieza la clase con el maestro", c != null && c.teacher == teacher, teacher.name);
        if (c == null) { Finish(who, oldScale, wd, wdWas); TestProbe.End(); yield break; }
        int joined = studs.Count(s => c.Join(s));
        TestProbe.Check($"los {studs.Count} practicantes se unen y a cada uno le toca una esterilla", joined == studs.Count, $"{joined}/{studs.Count}");
        var assigned = studs.Select(s => c.MatOf(s)).Where(m => m != null).ToList();
        TestProbe.Check("una persona por esterilla (todas distintas)", assigned.Distinct().Count() == assigned.Count && assigned.Count == studs.Count,
            string.Join(", ", studs.Select(s => $"{s.name}→{c.MatOf(s)?.name ?? "—"}")));

        // ── Llegar: el maestro a la tarima y cada uno a su esterilla, andando ──
        float t0 = Time.time;
        while (c != null && c.Now == YogaClass.Phase.Gathering && Time.time - t0 < gatherLimit) { Track(who); yield return null; }
        bool started = c != null && c.Now == YogaClass.Phase.Teaching;
        TestProbe.Check("todos llegan andando y empieza la clase", started && c.Practitioners.All(p => p.onMat),
            c != null ? $"{Time.time - t0:0} s · maestro en la tarima {c.TeacherOnTarima} · en su esterilla {c.Practitioners.Count(p => p.onMat)}/{c.Practitioners.Count}" : "la clase se terminó");
        if (!started) { Finish(who, oldScale, wd, wdWas); TestProbe.End(); yield break; }

        // ── Los pasos ──
        string errTxt = "";
        AsanaPoseSpell prev = null;
        int steps = c.StepCount, copied = 0, lastStep = -1; float stepAt = 0f; bool stepCopied = false;
        float worstMat = 0f, worstFace = 0f; string matWho = "", faceWho = "", slow = ""; int offTarima = 0, frames = 0;
        float worstCopy = 0f;
        float tEnd = Time.time + steps * 40f + 30f;
        while (c != null && c.Now == YogaClass.Phase.Teaching && Time.time < tEnd)
        {
            Track(who); frames++;
            if (c.StepIndex != lastStep)
            {
                // La clase solo pasa de paso antes de su tope cuando TODOS lo hacen: si cambió entre fotogramas, también vale.
                if (lastStep >= 0 && !stepCopied && prev != null && Time.time - stepAt < Mathf.Min(c.stepTimeout, copyLimit + prev.duration * prev.reachFraction)) { stepCopied = true; copied++; worstCopy = Mathf.Max(worstCopy, Time.time - stepAt); }
                if (lastStep >= 0 && !stepCopied) slow += $"{lastStep + 1}({errTxt}, {Time.time - stepAt:0} s)·";
                prev = c.CurrentStep;
                lastStep = c.StepIndex; stepAt = Time.time; stepCopied = false;
            }
            AsanaPoseSpell step = c.CurrentStep;
            if (!stepCopied && step != null) errTxt = WorstError(c, teacher, step);
            if (!stepCopied && step != null && Time.time - stepAt > c.reaction)
            {
                bool all2 = c.Practitioners.Any(p => p.onMat) && c.Practitioners.Where(p => p.onMat).All(p => YogaClass.Matches(p.a, step, tolerance) && MatchesTeacher(p.a, teacher, step, tolerance));
                if (all2) { stepCopied = Time.time - stepAt <= copyLimit + step.duration * step.reachFraction; if (stepCopied) { copied++; worstCopy = Mathf.Max(worstCopy, Time.time - stepAt); } }
            }
            if (!YogaClass.OnTarima(teacher.gameObject)) offTarima++;
            foreach (YogaClass.Practitioner p in c.Practitioners)
            {
                Vector3 d = p.mat.transform.position - p.a.transform.position; d.y = 0f;
                float off = d.magnitude + (Mathf.Abs(YogaClass.FeetY(p.a.gameObject) - p.mat.transform.position.y) > 0.5f ? 10f : 0f);
                if (off > worstMat) { worstMat = off; matWho = p.a.name; }
                Vector3 f = teacher.transform.position - p.a.transform.position; f.y = 0f;
                float ang = Vector3.Angle(p.a.transform.forward, f);
                if (ang > worstFace) { worstFace = ang; faceWho = p.a.name; }
            }
            yield return null;
        }
        if (lastStep >= 0 && !stepCopied) slow += $"{lastStep + 1}·";
        TestProbe.Check("la clase llega al final de la secuencia", c == null || c.Now == YogaClass.Phase.Done, $"{steps} pasos");
        TestProbe.Check($"en cada paso todos los practicantes copian la postura del maestro (al llegar la postura + {copyLimit:0} s) (±{tolerance:0}°)", copied == steps,
            $"{copied}/{steps} pasos (el más lento {worstCopy:0.0} s){(slow.Length > 0 ? " · sin copiar a tiempo: " + slow : "")}");
        TestProbe.Check("cada practicante se queda en su esterilla durante toda la clase (≤ 1 m del centro)", worstMat <= 1f, $"peor {worstMat:0.00} m ({matWho})");
        TestProbe.Check("todos miran al maestro (≤ 30°)", worstFace <= 30f, $"peor {worstFace:0}° ({faceWho})");
        TestProbe.Check("el maestro se queda en la tarima", offTarima == 0, $"{offTarima}/{frames} fotogramas fuera");
        foreach (var kv in who) TestProbe.Check($"«{kv.Key.name}» no se teletransporta", kv.Value.maxJump <= 0f, kv.Value.maxJump > 0f ? $"salto de {kv.Value.maxJump:0.00} m" : "");

        Finish(who, oldScale, wd, wdWas);
        TestProbe.End();
    }

    /// <summary>El hueso que más se aparta (de la postura del paso o del cuerpo del maestro) entre los que están en su esterilla.</summary>
    static string WorstError(YogaClass c, Anima t, AsanaPoseSpell step)
    {
        float worst = 0f; string w = "";
        CreatureRig rt = t.GetComponent<CreatureRig>();
        foreach (YogaClass.Practitioner p in c.Practitioners)
        {
            if (!p.onMat) continue;
            CreatureRig rs = p.a.GetComponent<CreatureRig>(); if (rs == null) continue;
            foreach (var part in step.parts)
            {
                if (YogaClass.ByIK(step, part.Item1)) continue;
                Transform a = rs.Get(part.Item1); if (a == null) continue;
                float e1 = Quaternion.Angle(a.localRotation, Quaternion.Euler(part.Item2));
                Transform b = rt != null ? rt.Get(part.Item1) : null;
                float e2 = b != null ? Quaternion.Angle(a.localRotation, b.localRotation) : 0f;
                if (e1 > worst) { worst = e1; w = $"{p.a.name} {part.Item1} {e1:0}° de la postura"; }
                if (e2 > worst) { worst = e2; w = $"{p.a.name} {part.Item1} {e2:0}° del maestro"; }
            }
        }
        return w;
    }

    /// <summary>¿Los huesos de la pose del practicante coinciden con los del cuerpo del maestro (no solo con la pose)?</summary>
    static bool MatchesTeacher(Anima s, Anima t, AsanaPoseSpell step, float tol)
    {
        CreatureRig rs = s.GetComponent<CreatureRig>(), rt = t.GetComponent<CreatureRig>();
        if (rs == null || rt == null) return false;
        foreach (var p in step.parts)
        {
            if (YogaClass.ByIK(step, p.Item1)) continue;
            Transform a = rs.Get(p.Item1), b = rt.Get(p.Item1);
            if (a == null || b == null) continue;
            if (Quaternion.Angle(a.localRotation, b.localRotation) > tol) return false;
        }
        return true;
    }

    static Who Hold(Anima a)
    {
        GameObject g = a.gameObject;
        var w = new Who { a = a, pos = g.transform.position, rot = g.transform.rotation, last = g.transform.position };
        w.idle = g.GetComponent<IdleBehavior>(); w.idleWas = w.idle != null && w.idle.enabled; if (w.idle != null) w.idle.enabled = false;
        w.q = g.GetComponent<ActionQueue>(); w.qWas = w.q != null && w.q.enabled; if (w.q != null) { w.q.ClearOwn(); w.q.enabled = false; }
        w.agent = g.GetComponent<UnityEngine.AI.NavMeshAgent>(); w.agentWas = w.agent != null && w.agent.enabled; if (w.agent != null) w.agent.enabled = false;
        w.keeper = g.GetComponent<CarnivoreKeeper>(); w.keeperWas = w.keeper != null && w.keeper.enabled; if (w.keeper != null) w.keeper.enabled = false;
        a.asleep = false;
        PathMover.Of(g.transform).Restart();
        return w;
    }

    static void Track(Dictionary<Anima, Who> who)
    {
        float walk = WorkdayDirector.Instance != null ? WorkdayDirector.Instance.walkSpeed : 2.6f;
        foreach (Who w in who.Values)
        {
            if (w.a == null) continue;
            Vector3 p = w.a.transform.position;
            bool riding = FlyingCarpet.Riding(w.a.gameObject);
            float jump = Vector3.Distance(p, w.last), lim = Mathf.Max(0.6f, walk * 3f * Time.deltaTime);
            if (!riding && !w.rode && jump > lim && jump > w.maxJump) w.maxJump = jump;
            w.rode = riding; w.last = p;
        }
    }

    static void Finish(Dictionary<Anima, Who> who, float scale, WorkdayDirector wd, bool wdWas)
    {
        if (YogaClass.Current != null) YogaClass.Current.End();
        foreach (Who w in who.Values)
        {
            if (w.a == null) continue;
            DailyPoses.Stand().ApplyFinal(w.a);
            w.a.transform.SetPositionAndRotation(w.pos, w.rot);
            if (w.agent != null) { w.agent.enabled = w.agentWas; if (w.agent.enabled && w.agent.isOnNavMesh) w.agent.Warp(w.pos); }
            if (w.q != null) w.q.enabled = w.qWas;
            if (w.idle != null) w.idle.enabled = w.idleWas;
            if (w.keeper != null) w.keeper.enabled = w.keeperWas;
        }
        WorkdayDirector.LogTeleports = false;
        Time.timeScale = scale;
        if (wd != null) wd.enabled = wdWas;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Clase de yoga (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloClaseYoga"); DontDestroyOnLoad(go);
        YogaClassTest t = go.AddComponent<YogaClassTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(YogaClassTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
