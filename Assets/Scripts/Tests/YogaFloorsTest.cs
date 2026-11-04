using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// LOS PISOS DE YOGA (Berón 2026-11-04; docs/yoga-pisos.md): se quitó el anillo de yoga (desde él no se veía al maestro) y el yoga
/// pasó a DOS PISOS de la torre del Entrenamiento —Hatha (el de la Sala de Yoga) y Ashtanga (su planta propia)—, cada uno con su
/// sala OCCIDENTAL y su sala TRADICIONAL. Comprueba:
///  1. SIN ANILLO: ni «Anillo_yoga» ni losas de anillo en la Sala de Yoga.
///  2. LOS DOS PISOS y sus CUATRO SALAS, cada una en su piso, con sus elementos: occidental (suelo de madera, estante de accesorios,
///     Buda o Ganesha, sin espejo) y tradicional (suelo de tierra, kund con su agua y sus 4 bobinas, lámparas de aceite, y el linga
///     en Hatha o Adiyogi y Patanjali en Ashtanga); 2 m entre muebles y nada de escaleras.
///  3. LÍNEAS DE VISIÓN: desde cada esterilla se ve al maestro de pie en la tarima (nada en medio, ≤ 9 m).
///  4. EL REPARTO: la Ashtanga Primaria va al piso de Ashtanga y el resto al de Hatha; la versión cambia de un día a otro; el ocio
///     (LeisureDirector) apunta a salas que existen.
///  5. UNA CLASE EN CADA PISO A LA VEZ: Hatha en su sala TRADICIONAL y Ashtanga en la occidental; las dos empiezan a guiar y avanzan.
///  6. LA INMERSIÓN: en la clase tradicional, cada practicante entra en el kund y se sumerge ANTES de que empiece la clase.
/// Sola: «Tools/Cold Sanctuary/Pruebas/Pisos de yoga (sola)».
/// </summary>
public class YogaFloorsTest : MonoBehaviour, ITestUnit
{
    public int Group => 64;   // tras la clase de yoga (51) y los edificios-árbol (57)
    public bool ParallelSafe => false;
    public float speed = 4f;
    [Tooltip("Practicantes por clase.")]
    public int perClass = 2;
    [Tooltip("Tope (s de juego) para que empiecen las dos clases (en la tradicional, antes pasan por el kund).")]
    public float gatherLimit = 300f;
    [Tooltip("Pasos que deben avanzar las dos clases.")]
    public int stepsToSee = 3;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<YogaFloorsTest>() == null) tr.gameObject.AddComponent<YogaFloorsTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Pisos de yoga: sin anillo, Hatha y Ashtanga (occidental y tradicional), líneas de visión, una clase por piso y el kund");
        Transform room = YogaClass.Room;
        Transform ash = room != null ? room.Find("Planta_ashtanga") : null;

        // ── 1. Sin anillo ──
        bool ringLeft = GameObject.Find("Anillo_yoga") != null || (room != null && room.GetComponentsInChildren<Transform>(true).Any(t => t.name == "Losa_anillo" || t.name == "Balcon_tronco"));
        TestProbe.Check("sin anillo de yoga (ni «Anillo_yoga» ni losas ni balcón de anillo en la Sala de Yoga)", room != null && !ringLeft);

        // ── 2. Los dos pisos y sus cuatro salas ──
        TestProbe.Check("dos pisos de yoga: el de Hatha (Sala de Yoga) y el de Ashtanga (Planta_ashtanga), pisos distintos de la torre",
            room != null && ash != null && room.GetComponent<TreeFloor>() != null && ash.GetComponent<TreeFloor>() != null && Mathf.Abs(ash.position.y - room.position.y) > 10f,
            room != null && ash != null ? $"Hatha a {room.position.y:0} m, Ashtanga a {ash.position.y:0} m" : "falta alguno");
        var halls = YogaHall.All.Where(h => h != null).ToList();
        foreach (YogaHall.Style st in new[] { YogaHall.Style.Hatha, YogaHall.Style.Ashtanga })
            foreach (YogaHall.Version v in new[] { YogaHall.Version.Occidental, YogaHall.Version.Tradicional })
            {
                YogaHall h = YogaHall.Find(st, v);
                string tag = $"{st} {(v == YogaHall.Version.Occidental ? "occidental" : "tradicional")}";
                if (h == null) { TestProbe.Check($"sala {tag}", false, "no está"); continue; }
                Transform floor = st == YogaHall.Style.Hatha ? room : ash;
                bool onFloor = floor != null && h.transform.IsChildOf(floor) && (st == YogaHall.Style.Ashtanga || ash == null || !h.transform.IsChildOf(ash));
                TestProbe.Check($"sala {tag}: en su piso, con tarima y ≥ 8 esterillas", onFloor && h.tarima != null && h.Mats().Count >= 8, $"{h.Mats().Count} esterillas");
                var names = new HashSet<string>(h.GetComponentsInChildren<Transform>(true).Select(t => t.name));
                bool ok; string what;
                if (v == YogaHall.Version.Occidental)
                {
                    ok = names.Contains("Suelo_madera") && names.Contains("Estante_accesorios") && names.Contains(st == YogaHall.Style.Hatha ? "Estatua_Buda" : "Estatua_Ganesha")
                         && !names.Any(n => n.StartsWith("Espejo")) && h.kund == null;
                    what = "suelo de madera, estante de accesorios, " + (st == YogaHall.Style.Hatha ? "Buda" : "Ganesha") + ", sin espejo ni kund";
                }
                else
                {
                    int coils = h.kund != null ? h.kund.GetComponentsInChildren<Transform>().Count(t => t.name == "Bobina_campo") : 0;
                    int lamps = h.GetComponentsInChildren<Transform>().Count(t => t.name == "Lampara_aceite");
                    bool icon = st == YogaHall.Style.Hatha ? names.Contains("Linga_Dhyanalinga") : names.Contains("Busto_Adiyogi") && names.Contains("Estatua_Patanjali");
                    ok = names.Contains("Suelo_tierra") && h.kund != null && h.kund.Find("Agua_kund") != null && coils == 4 && lamps >= 5 && icon && names.Contains("Asiento_kusha");
                    what = $"suelo de tierra, kund (agua {(h.kund != null && h.kund.Find("Agua_kund") != null)}, {coils}/4 bobinas), {lamps} lámparas, asiento de kusha, " + (st == YogaHall.Style.Hatha ? "linga" : "Adiyogi y Patanjali");
                }
                TestProbe.Check($"sala {tag}: sus elementos ({what})", ok);
                // ── 3. Líneas de visión ──
                int blocked = h.BlockedSightLines(out string why);
                TestProbe.Check($"sala {tag}: todas las esterillas ven al maestro (nada en medio, ≤ {YogaHall.MaxSight:0} m)", blocked == 0, why);
            }
        // (regla J16) Tantas salas como pisos pide el número de miembros: dos por piso (occidental y tradicional) de cada estilo.
        int mem = AreaCapacity.Members(), expectHalls = 2 * (TrainingCentres.FloorsFor("Yoga Hatha", mem) + TrainingCentres.FloorsFor("Yoga Ashtanga", mem));
        TestProbe.Check($"{expectHalls} salas de yoga (2 por piso; los pisos de cada estilo, ⌈{mem} miembros / 16 esterillas⌉ por la regla J16)", halls.Count == expectHalls, string.Join(", ", halls.Select(h => $"{h.Label} (piso {h.floorNo})")));
        if (room != null)
        {
            var bad = FurnitureSpacing.Violations(room);
            TestProbe.Check("pisos de yoga: 2 m entre muebles (y ninguno pisa una esterilla)", bad.Count == 0, bad.Count > 0 ? FurnitureSpacing.Worst(bad) : "0 violaciones");
            TestProbe.Check("pisos de yoga: sin escaleras", !room.GetComponentsInChildren<Transform>(true).Any(t => t.name.StartsWith("Escalera")));
        }

        // ── 4. El reparto ──
        bool styles = YogaHall.StyleOf(WorkdayDirector.AshtangaSession) == YogaHall.Style.Ashtanga && YogaHall.StyleOf(WorkdayDirector.HathaSession) == YogaHall.Style.Hatha
                      && YogaHall.StyleOf("Yoga Namaskar") == YogaHall.Style.Hatha && YogaHall.StyleOf("Upa-yoga de cuello") == YogaHall.Style.Hatha;
        bool days = YogaHall.VersionOn(0) != YogaHall.VersionOn(1) && YogaHall.VersionOn(0) == YogaHall.VersionOn(2);
        YogaHall a0 = YogaHall.For(WorkdayDirector.AshtangaSession, 0), h1 = YogaHall.For(WorkdayDirector.HathaSession, 1);
        TestProbe.Check("la Ashtanga Primaria va al piso de Ashtanga y lo demás al de Hatha; la versión se alterna por días",
            styles && days && a0 != null && a0.style == YogaHall.Style.Ashtanga && h1 != null && h1.style == YogaHall.Style.Hatha,
            $"día 0 Ashtanga → {a0?.Label}; día 1 Hatha → {h1?.Label}");
        var venues = LeisureDirector.SanctuaryActivities.SelectMany(d => d.where).Where(w => w.StartsWith(LeisureDirector.YogaVenue)).ToList();
        TestProbe.Check("el ocio apunta a salas de los pisos de yoga que existen (Hatha y Ashtanga)",
            venues.Count >= 2 && venues.All(w => halls.Any(h => LeisureDirector.YogaVenue + h.Label == w)) && venues.Any(w => w.Contains("Hatha")) && venues.Any(w => w.Contains("Ashtanga")),
            string.Join(", ", venues));

        // ── 5 y 6. Una clase en cada piso a la vez, y el kund antes de la tradicional ──
        YogaHall hathaT = YogaHall.Find(YogaHall.Style.Hatha, YogaHall.Version.Tradicional), ashO = YogaHall.Find(YogaHall.Style.Ashtanga, YogaHall.Version.Occidental);
        if (hathaT == null || ashO == null) { TestProbe.Check("hay salas para las dos clases", false); TestProbe.End(); yield break; }
        float mEnd = Time.realtimeSinceStartup + 120f;   // que el personal ya sea del santuario
        while (Time.realtimeSinceStartup < mEnd && People().Count < 2 * (perClass + 1)) yield return new WaitForSecondsRealtime(1f);
        List<Anima> people = People();
        TestProbe.Check($"hay {2 * (perClass + 1)} del personal para dos clases", people.Count >= 2 * (perClass + 1), string.Join(", ", people.Select(p => p.name)));
        if (people.Count < 4) { TestProbe.End(); yield break; }
        int per = Mathf.Max(1, Mathf.Min(perClass, people.Count / 2 - 1));
        Anima tH = people.FirstOrDefault(p => p.name == WorkdayDirector.YogaTeacher) ?? people[0];
        people.Remove(tH);
        Anima tA = people.FirstOrDefault(p => p.name == WorkdayDirector.SecondYogaTeacher) ?? people[0]; people.Remove(tA);   // la segunda maestra (Irosene)
        List<Anima> sH = people.Take(per).ToList(), sA = people.Skip(per).Take(per).ToList();

        WorkdayDirector wd = WorkdayDirector.Instance;
        bool wdWas = wd != null && wd.enabled; if (wd != null) wd.enabled = false;
        float oldScale = Time.timeScale; Time.timeScale = speed;
        YogaClass.EndAll();
        yield return null;
        var all = new List<Anima> { tH, tA }; all.AddRange(sH); all.AddRange(sA);
        var held = all.ToDictionary(x => x, Hold);
        int immersionsBefore = YogaClass.Immersions.Count;

        YogaClass cH = YogaClass.Begin(tH, WorkdayDirector.HathaSession, hathaT), cA = YogaClass.Begin(tA, WorkdayDirector.AshtangaSession, ashO);
        TestProbe.Check("empiezan a la vez la clase de Hatha (sala tradicional) y la de Ashtanga (sala occidental), cada una con su maestro",
            cH != null && cA != null && cH != cA && cH.teacher == tH && cA.teacher == tA && cH.hall == hathaT && cA.hall == ashO, $"{tH.name} · {tA.name}");
        if (cH == null || cA == null) { Finish(held, oldScale, wd, wdWas); TestProbe.End(); yield break; }
        int jH = sH.Count(s => cH.Join(s)), jA = sA.Count(s => cA.Join(s));
        TestProbe.Check("cada practicante se une a su clase (uno no puede estar en las dos)", jH == sH.Count && jA == sA.Count && (sH.Count == 0 || !cA.Join(sH[0])), $"Hatha {jH}/{sH.Count}, Ashtanga {jA}/{sA.Count}");
        TestProbe.Check("en la tradicional todos van antes al kund; en la occidental, no", cH.Practitioners.All(p => p.immerse) && cA.Practitioners.All(p => !p.immerse));

        float t0 = Time.time; bool sawBathing = false;
        while (Time.time - t0 < gatherLimit && (cH.Now == YogaClass.Phase.Gathering || cA.Now == YogaClass.Phase.Gathering))
        {
            foreach (YogaClass.Practitioner p in cH.Practitioners) if (p.bathing && hathaT.InKund(p.a.transform.position)) sawBathing = true;
            yield return null;
        }
        bool bothTeach = cH.Now == YogaClass.Phase.Teaching && cA.Now == YogaClass.Phase.Teaching;
        TestProbe.Check("las dos clases empiezan a guiar (maestro en su tarima, alumnos en su esterilla)", bothTeach,
            $"{Time.time - t0:0} s · Hatha {cH.Now} (en la tarima {cH.TeacherOnTarima}, {cH.Practitioners.Count(p => p.onMat)}/{cH.Practitioners.Count}) · Ashtanga {cA.Now} (en la tarima {cA.TeacherOnTarima}, {cA.Practitioners.Count(p => p.onMat)}/{cA.Practitioners.Count})");
        // La inmersión, antes de la clase tradicional.
        var baths = cH.Practitioners.Select(p => (p.a.name, p.bathed, p.bathSkipped, p.bathedAt)).ToList();
        bool allBathedFirst = baths.Count > 0 && baths.All(b => b.bathed && !b.bathSkipped && b.bathedAt > 0f && (cH.TeachingSince < 0f || b.bathedAt <= cH.TeachingSince + 0.01f));
        TestProbe.Check("en la clase tradicional cada practicante se sumerge en el kund ANTES de que empiece", allBathedFirst && sawBathing && YogaClass.Immersions.Count - immersionsBefore >= baths.Count,
            string.Join("; ", baths.Select(b => $"{b.name}: {(b.bathSkipped ? "sin kund" : b.bathed ? $"a los {b.bathedAt - t0:0} s" : "no")}")) + $" · clase a los {(cH.TeachingSince > 0f ? cH.TeachingSince - t0 : -1f):0} s · se le vio en el agua {sawBathing}");
        // Cada maestro en su piso (a la altura de su tarima, no en el otro piso).
        TestProbe.Check("cada maestro en la tarima de su piso", bothTeach && Mathf.Abs(YogaClass.FeetY(tH.gameObject) - hathaT.TarimaTop) < 0.5f && Mathf.Abs(YogaClass.FeetY(tA.gameObject) - ashO.TarimaTop) < 0.5f,
            $"{tH.name} pies a {YogaClass.FeetY(tH.gameObject):0.0} (tarima {hathaT.TarimaTop:0.0}); {tA.name} pies a {YogaClass.FeetY(tA.gameObject):0.0} (tarima {ashO.TarimaTop:0.0})");
        if (bothTeach)
        {
            float t1 = Time.time, lim = stepsToSee * (cH.stepTimeout + 8f) + 30f;
            while (Time.time - t1 < lim && (cH.Now == YogaClass.Phase.Teaching || cA.Now == YogaClass.Phase.Teaching) && (Progress(cH) < stepsToSee || Progress(cA) < stepsToSee)) yield return null;
            TestProbe.Check($"las dos clases avanzan a la vez (≥ {stepsToSee} pasos cada una)", Progress(cH) >= stepsToSee && Progress(cA) >= stepsToSee,
                $"Hatha {Progress(cH)}/{cH.StepCount} · Ashtanga {Progress(cA)}/{cA.StepCount} en {Time.time - t1:0} s");
        }
        Finish(held, oldScale, wd, wdWas);
        TestProbe.End();
    }

    // (Una clase ya terminada cuenta como acabada: todos sus pasos.)
    static int Progress(YogaClass c) => c == null ? int.MaxValue : c.Now == YogaClass.Phase.Done ? c.StepCount : c.StepIndex + 1;

    static List<Anima> People()
    {
        WorkdayDirector wd = WorkdayDirector.Instance;
        string[] names = wd != null ? wd.staff : new[] { "Linduarda", "Lizmibet", "Goluis", "Panterilia", "Gohageneis", "Irosene", "JefeConstruccion", "Andrea", "Nesvitor", "Amahuz", "Karen", "Alicita", "Ducristin", "Josedrilo", "Maite", "Shalva", "Adaguchi", "Gallarle" };
        var people = new List<Anima>();
        foreach (string n in names)
        {
            GameObject g = WorkdayDirector.Body(n);
            if (g == null || g.CompareTag("Player") || g.GetComponent<Magnate>() != null || g.GetComponent<CreatureRig>() == null) continue;
            if (g.TryGetComponent(out Linduarda lin) && lin.enabled) continue;
            if (g.TryGetComponent(out NewcomerInitiator ni) && ni.InProgress) continue;
            if (RestaurantService.Drives(g) || Orchestra.Busy(g)) continue;
            Anima a = g.GetComponent<Anima>(); if (a != null && Magnate.IsMember(a)) people.Add(a);
        }
        return people;
    }

    class Held { public Vector3 pos; public Quaternion rot; public IdleBehavior idle; public bool idleWas; public ActionQueue q; public bool qWas; public UnityEngine.AI.NavMeshAgent agent; public bool agentWas; public CarnivoreKeeper keeper; public bool keeperWas; }

    static Held Hold(Anima a)
    {
        GameObject g = a.gameObject;
        var w = new Held { pos = g.transform.position, rot = g.transform.rotation };
        w.idle = g.GetComponent<IdleBehavior>(); w.idleWas = w.idle != null && w.idle.enabled; if (w.idle != null) w.idle.enabled = false;
        w.q = g.GetComponent<ActionQueue>(); w.qWas = w.q != null && w.q.enabled; if (w.q != null) { w.q.ClearOwn(); w.q.enabled = false; }
        w.agent = g.GetComponent<UnityEngine.AI.NavMeshAgent>(); w.agentWas = w.agent != null && w.agent.enabled; if (w.agent != null) w.agent.enabled = false;
        w.keeper = g.GetComponent<CarnivoreKeeper>(); w.keeperWas = w.keeper != null && w.keeper.enabled; if (w.keeper != null) w.keeper.enabled = false;
        a.asleep = false;
        PathMover.Of(g.transform).Restart();
        return w;
    }

    static void Finish(Dictionary<Anima, Held> held, float scale, WorkdayDirector wd, bool wdWas)
    {
        YogaClass.EndAll();
        foreach (var kv in held)
        {
            Anima a = kv.Key; Held w = kv.Value; if (a == null) continue;
            DailyPoses.Stand().ApplyFinal(a);
            a.transform.SetPositionAndRotation(w.pos, w.rot);
            if (w.agent != null) { w.agent.enabled = w.agentWas; if (w.agent.enabled && w.agent.isOnNavMesh) w.agent.Warp(w.pos); }
            if (w.q != null) w.q.enabled = w.qWas;
            if (w.idle != null) w.idle.enabled = w.idleWas;
            if (w.keeper != null) w.keeper.enabled = w.keeperWas;
        }
        Time.timeScale = scale;
        if (wd != null) wd.enabled = wdWas;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Pisos de yoga (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloPisosYoga"); DontDestroyOnLoad(go);
        YogaFloorsTest t = go.AddComponent<YogaFloorsTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(YogaFloorsTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
