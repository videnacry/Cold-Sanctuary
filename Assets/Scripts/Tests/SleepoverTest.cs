using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LAS PIJAMADAS (grupo 92; docs/pijamadas.md; Berón 2026-11-04). En SampleScene:
///  1. LAS CASAS: cada casa del árbol duerme a 7 (cama de 4 y litera de 3); la litera no tiene escalera (se trepa por su poste) y
///     queda a ≥ 2 m de la cama y de la alfombra-esterilla.
///  2. EL HORARIO: la pijamada del viernes a las 21 h va con su anfitrión (y su pareja) y no pasa de las plazas de la casa; el
///     anfitrión cambia de una semana a la siguiente.
///  3. EN VIVO: en esa franja los invitados van a la casa del anfitrión (andando y en alfombra, sin saltos), se apuntan a dormir allí,
///     y esa noche se acuestan en su plaza (cama o litera); a la mañana siguiente cada uno vuelve a tener su casa.
/// Todas las esperas, acotadas.
/// </summary>
public class SleepoverTest : MonoBehaviour, ITestUnit
{
    public int Group => 92;
    public bool ParallelSafe => false;
    public float speed = 6f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        if (FindAnyObjectByType<Bedroom>() == null || FindAnyObjectByType<PillowRing>() == null) return;   // (la arboleda de SampleScene)
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<SleepoverTest>() == null) tr.gameObject.AddComponent<SleepoverTest>();
    }

    static void Log(string s) => Debug.Log("[Pijamada] " + s);

    public IEnumerator Run()
    {
        TestProbe.Begin("Pijamadas: camas de 4 y literas en las casas del árbol, anfitrión que rota y dormir en su casa");
        Houses();
        yield return Live();
        TestProbe.End();
    }

    // ── 1. Las casas ──────────────────────────────────────────────────────────────────────────────
    void Houses()
    {
        var rooms = Bedroom.All.Where(b => b != null && b.transform.parent != null && b.transform.parent.name == "ArboledaResidencial").ToList();
        TestProbe.Check("hay casas del árbol", rooms.Count > 0, $"{rooms.Count}");
        var small = rooms.Where(b => b.Capacity < 7).Select(b => $"{b.name} {b.Capacity}").ToList();
        TestProbe.Check("cada casa duerme a 7 (cama de 4 y litera de 3)", small.Count == 0, string.Join(", ", small));
        int noClimb = 0, stairs = 0, tight = 0; string why = "";
        foreach (Bedroom b in rooms)
        {
            Transform bunk = b.transform.Find("Litera");
            if (bunk == null) { noClimb++; continue; }
            if (bunk.GetComponentInChildren<Climbable>() == null) noClimb++;
            foreach (Transform x in b.GetComponentsInChildren<Transform>(true)) if (x.name.StartsWith("Escalera") || x.name.StartsWith("Peldano")) stairs++;
            Bounds bb = Bounds(bunk), bed = Bounds(b.bed);
            float dBed = DistXZ(bb, bed);
            CarpetPad mat = b.GetComponentsInChildren<CarpetPad>().FirstOrDefault(p => p.kind == CarpetPad.Kind.Habitacion);
            float dMat = mat != null ? DistXZ(bb, new Bounds(mat.transform.position, new Vector3(3f, 1f, 3.6f))) : 99f;
            if (dBed < 1.95f || dMat < 1.95f) { tight++; if (why.Length == 0) why = $"{b.name}: cama {dBed:0.00} m, alfombra {dMat:0.00} m"; }
        }
        TestProbe.Check("cada litera se sube trepando por su poste (Climbable)", noClimb == 0, $"{noClimb} sin poste");
        TestProbe.Check("sin escaleras en las casas", stairs == 0, $"{stairs}");
        TestProbe.Check("la litera queda a ≥ 2 m de la cama y de la alfombra-esterilla", tight == 0, why);
    }

    static Bounds Bounds(Transform t)
    {
        Bounds b = new Bounds(t.position, Vector3.zero); bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return b;
    }
    static float DistXZ(Bounds a, Bounds b)
    {
        float dx = Mathf.Max(0f, Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x)), dz = Mathf.Max(0f, Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z));
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    // ── 2 y 3. El horario y la noche ─────────────────────────────────────────────────────────────
    IEnumerator Live()
    {
        LeisureDirector ld = LeisureDirector.Instance;
        TimeController tc = TimeController.timeController;
        WorkdayDirector wd = WorkdayDirector.Instance;
        if (!TestProbe.Check("hay director del horario libre y reloj", ld != null && tc != null)) yield break;
        Sleepover.Clear();
        int day = tc.Day + 1;
        while (day - LeisureRoster.WeekOf(day) * LeisureRoster.DaysPerWeek != Sleepover.Day) day++;
        float start = LeisureRoster.SlotHours[Sleepover.Slot];
        Rest(wd != null ? wd.staff : new string[0]);
        tc.SetClock(day, start - 0.05f);
        if (wd != null) wd.PlanDay(day);
        ld.Scan(true);
        GameObject any = WorkdayDirector.Body("Linduarda") ?? WorkdayDirector.Body("Goluis");
        LeisureDirector.Group gr = any != null ? ld.GroupOf(any) : ld.groups.FirstOrDefault(g => !g.micro);
        if (!TestProbe.Check("el santuario tiene horario libre", gr != null && gr.plan != null)) yield break;
        LeisureRoster.Activity act = gr.catalog.FirstOrDefault(a => a.id == Sleepover.ActivityId);
        if (!TestProbe.Check("la pijamada está en el catálogo (etiqueta de actividad de la casa del anfitrión)", act != null && act.together && act.hosts != null && act.hosts.Length > 0, string.Join(", ", gr.catalog.Select(a => a.title)))) yield break;
        Bedroom host = gr.venues.TryGetValue(act, out var hv) && hv.anchor != null ? hv.anchor.GetComponent<Bedroom>() : null;
        var inSlot = gr.plan.entries.Where(e => e.day == day && e.slot == Sleepover.Slot && e.activity == act).ToList();
        TestProbe.Check("van el anfitrión y su pareja", act.hosts.All(h => inSlot.Any(e => e.person.name == h)), $"{string.Join(", ", act.hosts)} · van {string.Join(", ", inSlot.Select(e => e.person.name))}");
        TestProbe.Check("no pasa de las plazas de la casa (7)", host != null && inSlot.Count <= host.Capacity && inSlot.Count > act.hosts.Length, $"{inSlot.Count} de {(host != null ? host.Capacity : 0)}");
        // El anfitrión rota: el de la semana siguiente es otro.
        var people = new HashSet<string>(gr.bodies.Where(b => b != null).Select(b => b.name.Replace("_Post", "")));
        List<Bedroom> hosts = Sleepover.Hosts(gr.scene, people);
        int w = LeisureRoster.WeekOf(day);
        TestProbe.Check("el anfitrión cambia cada semana (rota por las casas)", hosts.Count > 1 && hosts[LeisureRoster.HostIndex(w, hosts.Count)] != hosts[LeisureRoster.HostIndex(w + 1, hosts.Count)] && hosts[LeisureRoster.HostIndex(w, hosts.Count)] == host,
            $"semana {w}: {(host != null ? host.owner : "—")}; semana {w + 1}: {(hosts.Count > 0 ? hosts[LeisureRoster.HostIndex(w + 1, hosts.Count)].owner : "—")}");
        if (host == null || inSlot.Count == 0) yield break;

        // La franja de las 21 h, en vivo.
        var names = inSlot.Select(e => e.person.name).ToList();
        var guests = names.Where(n => { Anima a = Body(n); return a != null && !host.Lives(a); }).ToList();
        int rec0 = LeisureDirector.Records.Count;
        LeisureDirector.ForceRun = true;
        float oldScale = Time.timeScale; Time.timeScale = speed;
        float perHour = tc.SecondsPerGameDay / 24f;
        float end = Time.realtimeSinceStartup + Mathf.Max(300f, perHour * 1.2f / speed * 4f);
        while (tc.Hour < start + 0.95f && tc.Day == day && Time.realtimeSinceStartup < end) yield return null;
        ld.Tick();
        var recs = LeisureDirector.Records.Skip(rec0).Concat(ld.Snapshot()).Where(x => !x.micro && x.day == day && x.slot == Sleepover.Slot && x.activity == act.title).ToList();
        var free = recs.Where(r => r.blocked < r.since * 0.5f).ToList();
        int arrived = free.Count(r => r.arrived), walked = free.Count(r => r.arrived && !r.fallback), jumps = recs.Sum(r => r.jumps);
        TestProbe.Check("van a la casa del anfitrión (≥ 80 %)", free.Count > 0 && arrived >= 0.8f * free.Count, $"{arrived}/{free.Count}");
        TestProbe.Check("llegan andando y en alfombra, no puestos allí", arrived == 0 || walked == arrived, $"{walked}/{arrived} · {free.Sum(r => r.flown):0} m en alfombra");
        TestProbe.Check("sin saltos ni teletransportes", jumps == 0, $"{jumps}");
        int joined = guests.Count(n => { Anima a = Body(n); return a != null && Bedroom.Of(a) == host; });
        TestProbe.Check("los invitados se apuntan a dormir en la casa del anfitrión", guests.Count > 0 && joined >= 0.8f * guests.Count, $"{joined}/{guests.Count}");
        var spotIdx = guests.Select(n => { Anima a = Body(n); return a != null ? host.SpotIndex(a) : -1; }).Where(i => i >= 0).ToList();
        TestProbe.Check("cada invitado tiene su plaza (ninguna repetida, ni la de los que viven allí)", spotIdx.Distinct().Count() == spotIdx.Count && !spotIdx.Contains((host.sleeps - 1) / 2), string.Join(", ", spotIdx));
        LeisureDirector.ForceRun = false; ld.ReleaseAll(gr);

        // ESA NOCHE: con sueño, cada uno a su plaza en la casa del anfitrión (acotado).
        tc.SetClock(day, 22.05f);
        foreach (string n in names) { Anima a = Body(n); if (a != null) a.sleepiness = 1f; }
        float until = Time.realtimeSinceStartup + 240f;
        int inBed = 0;
        while (Time.realtimeSinceStartup < until)
        {
            inBed = names.Count(n => { Anima a = Body(n); return a != null && a.asleep && (a.transform.position - (host.LieSpot(a) + Vector3.up * 0.5f)).magnitude < 0.8f; });
            if (inBed >= names.Count) break;
            yield return new WaitForSecondsRealtime(1f);
        }
        TestProbe.Check("esa noche duermen en su plaza de la casa del anfitrión (cama o litera; ≥ 80 %)", inBed >= 0.8f * names.Count, $"{inBed}/{names.Count}");
        int bunk = guests.Count(n => { Anima a = Body(n); return a != null && host.SpotIndex(a) >= host.sleeps; });
        Log($"anfitrión {host.owner} ({host.name}): {names.Count} en la pijamada, {bunk} en la litera, {inBed} dormidos en su plaza");

        // A LA MAÑANA SIGUIENTE: cada uno vuelve a tener su casa.
        tc.SetClock(day + 1, Sleepover.WakeHour + 1f);
        int back = guests.Count(n => { Anima a = Body(n); return a != null && Bedroom.Of(a) == Bedroom.Own(a) && Bedroom.Own(a) != host; });
        TestProbe.Check("a la mañana siguiente cada invitado vuelve a su casa", back == guests.Count, $"{back}/{guests.Count}");
        foreach (string n in names) { Anima a = Body(n); if (a != null) { a.sleepiness = 0.05f; if (a.asleep) { a.asleep = false; DailyPoses.Stand().ApplyFinal(a); } } }
        Time.timeScale = oldScale;
        Sleepover.Clear();
    }

    static Anima Body(string n) { GameObject g = WorkdayDirector.Body(n); return g != null ? g.GetComponent<Anima>() : null; }

    static void Rest(IEnumerable<string> staff)
    {
        foreach (string who in staff)
        {
            GameObject g = WorkdayDirector.Body(who);
            if (g == null || !g.TryGetComponent(out Anima a) || a.death) continue;
            if (g.TryGetComponent(out BodyMetabolism b)) { b.hunger = 0.1f; b.toilet = 0.05f; b.dirt = 0.1f; }
            a.sleepiness = 0.05f; a.aware = false;
            bool sleeping = g.TryGetComponent(out ActionQueue q) && q.Own.Any(s => s is SleepSpell);
            if (a.asleep && !sleeping) { a.asleep = false; DailyPoses.Stand().ApplyFinal(a); }
        }
    }
}
