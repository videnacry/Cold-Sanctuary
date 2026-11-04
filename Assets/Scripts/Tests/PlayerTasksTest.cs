using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TEST DE LAS TAREAS DEL JUGADOR CON «E» (Berón 2026-11-04: «para todas las tareas e iniciaciones vamos a utilizar el modelo de
/// Ambrosio, de modo que se pueden resolver pulsando "E" en los lugares indicados»; docs/tareas-del-jugador.md). Simula la E
/// (<see cref="PlayerTaskRunner.Press"/>, lo mismo que llama la tecla) con el jugador en cada sitio:
///  1. CATÁLOGO Y MARCADORES: toda misión que puede tocarle (el catálogo de la jornada, las del día, las obras, las
///     elaboraciones, las reparaciones) tiene pasos (≥ 2, con acción) y un marcador «E — acción» por paso; las del catálogo,
///     con pasos escritos.
///  2. TRES MISIONES EN TRES ÁREAS (Construcción: barro con paja; Estudio Textil: cuerda; Huerto: guadua): la E lejos del
///     marcador no hace nada; en cada sitio hace el paso (gesto, objeto en la mano y en el inventario) y pasa al siguiente;
///     al último, la misión se cierra y su efecto llega al almacén de material.
///  3. CADUCAR: una tarea que no se hace se quita sin efecto.
///  4. DOS INICIACIONES (huerto y cocina): cada paso que pide el de turno sale como marcador en vivo y la E en el sitio lo
///     lanza; todos los pasos los hace el jugador (ninguno «ayudado») y el suelo queda preparado / el stock sube.
/// Se añade solo al orquestador cuando se piden las pruebas.
/// </summary>
public class PlayerTasksTest : MonoBehaviour, ITestUnit
{
    public int Group => 96;   // (libres al escribirlo: 93+ sin usar; 64, 67–69, 74–76, 88–92 tomados)
    public bool ParallelSafe => false;
    public float speed = 4f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<PlayerTasksTest>() == null) tr.gameObject.AddComponent<PlayerTasksTest>();
    }

    GameObject _player;
    PlayerTaskRunner _run;

    public IEnumerator Run()
    {
        TestProbe.Begin("Tareas del jugador con E (modelo de Ambrosio): catálogo, marcadores, 3 misiones y 2 iniciaciones");
        _player = GameObject.FindGameObjectWithTag("Player");
        _run = PlayerTaskRunner.Ensure();
        if (_player == null) { TestProbe.Check("hay jugador", false); TestProbe.End(); yield break; }
        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) { MobWorldLoader.Instance.ExitMobWorld(); yield return Until(() => !MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 20f); }
        // La jornada de Kushal, quieta: si no, al llegar la hora de una misión suya empezaría su tarea encima de la de la prueba.
        var paused = new List<PlayerWorkday>();
        foreach (PlayerWorkday w in FindObjectsByType<PlayerWorkday>(FindObjectsSortMode.None)) if (w.enabled) { w.enabled = false; paused.Add(w); }
        if (_run.Active != null) _run.Lapse(null, "empieza la prueba");
        Vector3 home = _player.transform.position;

        yield return Catalog();
        yield return Missions();
        yield return LapseCheck();
        yield return Initiation(AreaInitiator.Kind.Garden);
        yield return Initiation(AreaInitiator.Kind.Kitchen);

        if (_run.Active != null) _run.Lapse(null, "fin de la prueba");
        WorkdayDirector.Teleport(_player.transform, home);
        foreach (PlayerWorkday w in paused) if (w != null) w.enabled = true;
        TestProbe.End();
    }

    // ── 1. Todas las misiones tienen pasos y marcadores ──────────────────────────────────────────────────────────────
    IEnumerator Catalog()
    {
        var missions = new List<WorkRoster.Mission>(AreaFunctions.SanctuaryMissions());
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        foreach (WorkRoster.Mission m in AreaWork.Today(day, ConstructionProject.All)) if (!missions.Exists(x => x.id == m.id)) missions.Add(m);
        foreach (string id in new[] { "mat_guadua", "mat_bahareque", "mat_micelio", "mat_cuerda" }) if (!missions.Exists(x => x.id == id) && AreaFunctions.Get(id) != null) missions.Add(AreaFunctions.Get(id).ToMission());
        missions.Add(new WorkRoster.Mission("Cocina", "prep_queso", "Elaborar queso (cuba)", 1f));
        missions.Add(new WorkRoster.Mission("Construcción", "obra_material_Prueba", "Llevar el material del día a la obra «Prueba»", 1f) { site = "Prueba" });
        missions.Add(new WorkRoster.Mission("Construcción", "obra_trabajo_Prueba", "Trabajar la obra «Prueba»", 2f) { site = "Prueba" });
        missions.Add(new WorkRoster.Mission("Construcción", "rep_Gimnasio", "Reparación en Gimnasio: foco fundido", 1f) { also = new[] { "Gimnasio" } });

        var catalogIds = new HashSet<string>(); foreach (WorkRoster.Mission m in AreaFunctions.SanctuaryMissions()) catalogIds.Add(m.id);
        var bad = new List<string>(); var notAuthored = new List<string>(); var noMarker = new List<string>();
        int found = 0, spots = 0;
        foreach (WorkRoster.Mission m in missions)
        {
            PlayerTasks.Plan p = PlayerTasks.For(m);
            if (p == null || p.steps.Count < 2 || p.steps.Exists(s => string.IsNullOrEmpty(s.action))) { bad.Add(m.id); continue; }
            if ((catalogIds.Contains(m.id) || m.id.StartsWith("mat_")) && !p.authored) notAuthored.Add(m.id);
            PlayerTasks.Spot[] sp = PlayerTaskRunner.Spots(p, _player.transform);
            List<GameObject> marks = PlayerTaskRunner.BuildMarkers(p, sp);
            bool ok = marks.Count == p.steps.Count;
            for (int i = 0; i < marks.Count; i++)
            {
                WorldLabel wl = marks[i] != null ? marks[i].GetComponent<WorldLabel>() : null;
                ok &= wl != null && wl.text.StartsWith("E — ") && !float.IsNaN(sp[i].pos.x);
                spots++; if (sp[i].found) found++;
            }
            if (!ok) noMarker.Add(m.id);
            foreach (GameObject g in marks) if (g != null) Destroy(g);
        }
        TestProbe.Check($"las {missions.Count} misiones que pueden tocarle tienen pasos (≥ 2, con acción)", bad.Count == 0, bad.Count > 0 ? string.Join(", ", bad) : "");
        TestProbe.Check("las del catálogo de la jornada y las del material tienen pasos escritos (no genéricos)", notAuthored.Count == 0, notAuthored.Count > 0 ? string.Join(", ", notAuthored) : $"{catalogIds.Count} del catálogo");
        TestProbe.Check("cada paso de cada misión tiene su marcador «E — acción» en un sitio", noMarker.Count == 0, noMarker.Count > 0 ? string.Join(", ", noMarker) : $"{spots} marcadores; {found} en un mueble del puesto, {spots - found} en un punto del área");
        yield return null;
    }

    // ── 2. Tres misiones en tres áreas, paso a paso con E ────────────────────────────────────────────────────────────
    IEnumerator Missions()
    {
        float oldScale = Time.timeScale; Time.timeScale = speed;
        MaterialStore store = MaterialStore.Instance;
        GameObject tmp = null;
        if (store == null) { tmp = new GameObject("MaterialStore_PRUEBA"); store = tmp.AddComponent<MaterialStore>(); }
        int grove = store.groveMature; store.groveMature = Mathf.Max(grove, 12);

        var cases = new (string id, string what, System.Func<int> measure)[]
        {
            ("mat_bahareque", "barro con paja en el almacén", () => store.Have(MaterialStore.Barro)),
            ("mat_cuerda", "cuerda de cáñamo en el almacén", () => store.Have(MaterialStore.Cuerda)),
            ("mat_guadua", "guadua cortada curándose", () => store.Processing(MaterialStore.Guadua)),
        };
        bool first = true;
        foreach (var c in cases)
        {
            AreaFunctions.AreaFunction f = AreaFunctions.Get(c.id);
            if (f == null) { TestProbe.Check($"existe la misión {c.id}", false); continue; }
            WorkRoster.Mission m = f.ToMission();
            int before = c.measure();
            bool done = false;
            PlayerTaskRunner.Task t = _run.Begin(PlayerTasks.For(m), this, _ => done = true);
            string tag = $"{m.area} · «{m.title}»";
            TestProbe.Check($"{tag}: empieza con un marcador por paso", t != null && t.markers.Count == t.Count && t.Count >= 2, t != null ? $"{t.Count} pasos" : "sin tarea");
            if (t == null) continue;
            if (first)
            {
                first = false;
                WorkdayDirector.Teleport(_player.transform, t.CurrentSpot.pos + Vector3.right * 15f + Vector3.up);
                yield return null; yield return null;
                string r0 = _run.Press();
                TestProbe.Check($"{tag}: la E lejos del marcador no hace el paso (pide acercarse)", t.index == 0 && !t.acting && !_run.NearCurrent, r0);
            }
            int steps = t.Count, okSteps = 0;
            var itemsOk = new List<string>();
            for (int i = 0; i < steps && !t.completed; i++)
            {
                PlayerTasks.Step s = t.Current;
                WorkdayDirector.Teleport(_player.transform, PathMover.Of(_player.transform).OnGround(t.CurrentSpot.pos));
                yield return null; yield return null;
                bool near = _run.NearCurrent;
                string r = _run.Press();
                bool acting = t.acting;
                bool held = string.IsNullOrEmpty(s.item) || _run.HeldItem() != null;
                yield return Until(() => t.completed || t.index > i, PlayerTaskRunner.ActionSeconds + 3f);
                bool advanced = t.completed || t.index == i + 1;
                if (near && acting && held && advanced) okSteps++;
                else Debug.LogWarning($"[PlayerTasksTest] {tag} paso {i + 1} «{s.action}»: cerca={near} haciendo={acting} objeto={held} avanzó={advanced} · {r}");
                if (!string.IsNullOrEmpty(s.item)) itemsOk.Add(s.item);
            }
            TestProbe.Check($"{tag}: cada E en su sitio hace el paso (gesto y objeto en la mano) y pasa al siguiente", okSteps == steps, $"{okSteps}/{steps}");
            bool inv = itemsOk.TrueForAll(it => _run.Inventory.ContainsKey(it));
            TestProbe.Check($"{tag}: le dan los objetos de cada paso", inv && t.given.Count == itemsOk.Count, string.Join(", ", t.given));
            int after = c.measure();
            TestProbe.Check($"{tag}: la misión se cierra y su efecto llega ({c.what})", t.completed && done && t.effectRan && after > before && _run.Completed.Contains(c.id) && _run.Active == null, $"{before} → {after}");
            yield return null;
        }
        store.groveMature = Mathf.Max(store.groveMature, grove);
        if (tmp != null) Destroy(tmp);
        Time.timeScale = oldScale;
    }

    // ── 3. Caducar: sin hacerla, se quita y no da su efecto ──────────────────────────────────────────────────────────
    IEnumerator LapseCheck()
    {
        WorkRoster.Mission m = AreaFunctions.Get("hue_riego").ToMission();
        PlayerTaskRunner.Task t = _run.Begin(PlayerTasks.For(m), this, null);
        int n = t != null ? t.markers.Count : 0;
        _run.Lapse(this, "prueba: se acabó su hora");
        yield return null;
        bool gone = t != null && t.markers.TrueForAll(g => g == null);
        TestProbe.Check("una tarea ignorada caduca: fuera los marcadores, sin cerrarse ni efecto", t != null && n > 0 && t.lapsed && !t.completed && gone && _run.Active == null, $"{n} marcadores");
    }

    // ── 4. Iniciaciones con E en vivo ────────────────────────────────────────────────────────────────────────────────
    IEnumerator Initiation(AreaInitiator.Kind kind)
    {
        string name = kind == AreaInitiator.Kind.Garden ? "huerto" : "cocina";
        SanctuaryOnboarding ob = _player.GetComponent<SanctuaryOnboarding>();
        Anima pa = _player.GetComponent<Anima>();
        if (ob == null || pa == null) { TestProbe.Check($"iniciación de {name}: hay jugador recién llegado", false); yield break; }
        bool[] saved = { ob.apt, ob.presented, ob.initiated, ob.kitchenTrained, ob.gardenTrained };

        Magnate[] mags = FindObjectsByType<Magnate>(FindObjectsSortMode.None);
        KitchenBoss[] bosses = FindObjectsByType<KitchenBoss>(FindObjectsSortMode.None);
        foreach (Magnate mg in mags) mg.enabled = false;
        foreach (KitchenBoss b in bosses) b.enabled = false;
        foreach (AreaInitiator x in FindObjectsByType<AreaInitiator>(FindObjectsSortMode.None)) Destroy(x);
        foreach (NewcomerInitiator x in FindObjectsByType<NewcomerInitiator>(FindObjectsSortMode.None)) x.ResetState();
        if (_player.TryGetComponent(out HealthRecord hrec)) { hrec.analyzed = true; hrec.vaccinated = true; }
        yield return null;
        ob.apt = ob.presented = ob.initiated = true;
        if (kind == AreaInitiator.Kind.Garden) { ob.kitchenTrained = true; ob.gardenTrained = false; } else ob.kitchenTrained = false;
        AreaInitiator.AllowDuringTests = true;
        float oldScale = Time.timeScale; Time.timeScale = speed;

        RestaurantKitchen rk = FindAnyObjectByType<RestaurantKitchen>();
        if (kind == AreaInitiator.Kind.Garden)
        {
            GameObject area = GameObject.Find("Garden_Area");
            if (area != null) WorkdayDirector.Teleport(_player.transform, PathMover.Of(_player.transform).OnGround(area.transform.position + area.transform.right * 4f));
        }
        else
        {
            GameObject spot = GameObject.Find("CocinaSpot");
            if (spot != null) WorkdayDirector.Teleport(_player.transform, spot.transform.position + Vector3.up * 0.5f);
        }
        AreaInitiator ai = AreaInitiator.Begin(kind, ob, mags.Length > 0 ? mags[0].GetComponent<Anima>() : null);
        TestProbe.Check($"iniciación de {name}: la empieza un trabajador", ai != null, ai != null ? ai.Who : "nadie");
        if (ai == null) { Restore(oldScale, mags, bosses, ob, saved); yield break; }
        yield return Until(() => ai == null || (ai.Now != AreaInitiator.Phase.None && ai.Now != AreaInitiator.Phase.Approach), 400f);

        int asked = 0, pressed = 0, marked = 0;
        string lastAction = null;
        float limit = Time.time + 600f;
        while (ai != null && AreaInitiator.Owns(ob) && ai.Now != AreaInitiator.Phase.ToTank && Time.time < limit)
        {
            if (ai.WaitingForStudent)
            {
                yield return null;   // (el marcador en vivo lo pone su Update)
                if (ai == null || !ai.WaitingForStudent) continue;
                asked++;
                if (_run.LiveActive && _run.LiveMarker != null) marked++;
                lastAction = _run.LiveAction;
                if (_run.LiveActive && !_run.NearLive) { WorkdayDirector.Teleport(_player.transform, PathMover.Of(_player.transform).OnGround(_run.LiveSpot)); yield return null; }
                string r = _run.Press();
                if (r.StartsWith("haces")) pressed++;
                else Debug.LogWarning($"[PlayerTasksTest] iniciación de {name}, paso {asked}: {r}");
                yield return Until(() => ai == null || !ai.WaitingForStudent, 12f);
                for (int k = 0; k < 3; k++) yield return null;
            }
            yield return null;
        }
        bool reachedTank = ai != null && ai.Now == AreaInitiator.Phase.ToTank;
        if (ai != null) ai.Cancel("prueba de tareas con E: sin tanque");   // (el tanque y el microcosmos ya los miran 41 y 43)
        yield return null;
        AreaInitiator.Report rep = AreaInitiator.Last;
        int total = kind == AreaInitiator.Kind.Garden ? AreaInitiator.SoilSteps.Length : (rep != null && rk != null && rk.Of(rep.dish) != null && rk.Of(rep.dish).recipe != null ? rk.Of(rep.dish).recipe.steps.Length : asked);
        TestProbe.Check($"iniciación de {name}: cada paso pedido sale como marcador «E — paso» en su sitio", asked > 0 && marked == asked, $"{marked}/{asked} (último: «{lastAction}»)");
        TestProbe.Check($"iniciación de {name}: la E en el sitio lanza cada paso; ninguno lo hace el de turno por él", rep != null && pressed == asked && rep.stepsByStudent >= total && rep.stepsHelped == 0 && reachedTank,
            rep != null ? $"{rep.stepsByStudent} con E, {rep.stepsHelped} ayudados de {total}" : "sin informe");
        if (kind == AreaInitiator.Kind.Garden)
            TestProbe.Check("iniciación del huerto: el suelo del bancal queda preparado", rep != null && rep.soilPrepared, rep != null ? rep.plot : "");
        else
            TestProbe.Check("iniciación de la cocina: la tanda por mayor llega al stock", rep != null && rep.cooked && rep.stockAfter > rep.stockBefore, rep != null ? $"«{rep.dish}» {rep.stockBefore} → {rep.stockAfter}" : "");
        Restore(oldScale, mags, bosses, ob, saved);
    }

    static void Restore(float scale, Magnate[] mags, KitchenBoss[] bosses, SanctuaryOnboarding ob, bool[] saved)
    {
        Time.timeScale = scale; AreaInitiator.AllowDuringTests = false;
        foreach (Magnate m in mags) if (m != null) m.enabled = true;
        foreach (KitchenBoss b in bosses) if (b != null) b.enabled = true;
        if (ob != null) { ob.apt = saved[0]; ob.presented = saved[1]; ob.initiated = saved[2]; ob.kitchenTrained = saved[3]; ob.gardenTrained = saved[4]; }
    }

    static IEnumerator Until(System.Func<bool> cond, float seconds)
    {
        float end = Time.time + seconds;
        while (!cond() && Time.time < end) yield return null;
    }
}
