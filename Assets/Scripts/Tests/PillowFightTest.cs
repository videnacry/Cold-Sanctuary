using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA GUERRA DE ALMOHADAS (grupo 89; docs/guerra-de-almohadas.md; Berón 2026-11-04). En SampleScene:
///  1. EL RING: existe en la arboleda, su colchón mide lo que manda la regla (n × n celdas de 2,5 m, n = ⌈√residentes⌉), caben todos
///     los residentes y todo el personal (PillowRing.Validate), está lleno de almohadas (2 por luchador) y tiene su pista de alfombras.
///  2. LAS CAMAS DE 4: cada casa del árbol tiene cama de 4 plazas y la pareja duerme en plazas distintas.
///  3. EL HORARIO: la guerra es la actividad de todos a la vez en su franja (sábado, 20 h) y en esa franja la tienen todos.
///  4. EN VIVO (reloj ×<see cref="speed"/>): en esa franja todos van al ring (andando y en alfombra), cada uno a su celda, cogen
///     almohada y se dan almohadazos (golpes y risas), el vínculo sube; sin saltos ni teletransportes.
/// Todas las esperas, acotadas. Se añade solo al orquestador cuando la escena tiene ring.
/// </summary>
public class PillowFightTest : MonoBehaviour, ITestUnit
{
    public int Group => 89;
    public bool ParallelSafe => false;
    public float speed = 6f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        if (FindAnyObjectByType<PillowRing>() == null) return;
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<PillowFightTest>() == null) tr.gameObject.AddComponent<PillowFightTest>();
    }

    static void Log(string s) => Debug.Log("[Almohadas] " + s);
    const string ResidenciaName = "Residencia";   // (ResidentialBuilder.Name, en el Editor)

    public IEnumerator Run()
    {
        TestProbe.Begin("Guerra de almohadas: el ring de la cama colosal para todos los residentes, y las camas de 4");
        PillowRing ring = FindAnyObjectByType<PillowRing>();
        if (!TestProbe.Check("hay ring de almohadas en la arboleda", ring != null)) { TestProbe.End(); yield break; }
        Ring(ring);
        Beds();
        yield return Live(ring);
        TestProbe.End();
    }

    // ── 1. El ring y su regla ──────────────────────────────────────────────────────────────────────
    void Ring(PillowRing ring)
    {
        int residents = PillowRing.Residents(ring.gameObject.scene);
        TestProbe.Check("la regla del ring se cumple (caben todos los residentes y el personal; colchón y almohadas según la regla)", ring.Validate(out string why), why);
        TestProbe.Check($"el colchón tiene ⌈√{residents}⌉ = {PillowRing.CellsFor(residents)} celdas por lado", ring.cells == PillowRing.CellsFor(Mathf.Max(residents, ring.builtFor)), $"{ring.cells} × {ring.cells} ({ring.Side:0.0} m)");
        TestProbe.Check("la regla crece con los residentes (con uno más de lo que cabe, pide más celdas)", PillowRing.CellsFor(ring.Capacity + 1) > ring.cells && PillowRing.CellsFor(ring.Capacity) == ring.cells);
        CarpetPad pad = CarpetPad.All.FirstOrDefault(p => p != null && p.transform.IsChildOf(ring.transform));
        TestProbe.Check("el ring tiene su pista de alfombras (red de la Residencia)", pad != null && pad.kind == CarpetPad.Kind.Habitacion, pad != null ? $"{pad.displayName} ({pad.building}, piso {pad.floor})" : "ninguna");
        if (pad != null)
        {
            Vector3 d = pad.transform.position - ring.Spot(0); d.y = 0f;
            Bounds zb = new Bounds(pad.transform.position, new Vector3(7.6f, 1f, 4.4f));
            float gap = float.MaxValue;
            for (int i = 0; i < ring.Capacity; i++) { Vector3 s = ring.Spot(i); s.y = zb.center.y; gap = Mathf.Min(gap, Mathf.Sqrt(zb.SqrDistance(s)) - PillowRing.CellSize * 0.5f - PillowRing.Bumper); }
            TestProbe.Check("la pista queda a ≥ 2 m de la cama colosal (regla de paso)", gap >= 1.9f, $"{gap:0.00} m");
            TestProbe.Check("se llega en alfombra desde la plataforma central", CarpetPad.All.Any(p => p != null && p != pad && pad.FliesTo(p) && p.building == pad.building && p.floor == 1));
        }
        // LAS ALFOMBRAS NO LO ATRAVIESAN: de cada pista de la plataforma central (debajo del ring) a cada casa del árbol y a la pista
        // del propio ring, ningún punto del vuelo (FlyingCarpet.Legs) cae dentro de su caja.
        Bounds box = ring.Box();
        var central = CarpetPad.All.Where(p => p != null && p.building == ResidenciaName && p.floor == 1).ToList();
        var targets = CarpetPad.All.Where(p => p != null && (p.kind == CarpetPad.Kind.Habitacion || p.transform.IsChildOf(ring.transform))).ToList();
        Transform ringPadT = CarpetPad.All.Where(p => p != null && p.transform.IsChildOf(ring.transform)).Select(p => p.transform).FirstOrDefault();
        Vector3 ringPad = ringPadT != null ? ringPadT.position : Vector3.zero;
        int flights = 0, through = 0; string firstBad = "";
        foreach (CarpetPad a in central)
            foreach (CarpetPad b in targets)
            {
                flights++;
                foreach (var (l0, l1, arc) in FlyingCarpet.Legs(a.transform.position, b.transform.position, 26f))
                {
                    float top = Mathf.Max(l0.y, l1.y) + (arc ? 26f : 0f);
                    bool hit = false;
                    for (int i = 0; i <= 60 && !hit; i++)
                    {
                        float u = i / 60f, s = Mathf.SmoothStep(0f, 1f, u);
                        Vector3 p = Vector3.Lerp(l0, l1, s);
                        if (arc) p.y = Mathf.Lerp(l0.y, l1.y, s) + Mathf.Sin(u * Mathf.PI) * (top - Mathf.Max(l0.y, l1.y));
                        // (ronda 6: los 3 «cruces» eran el ATERRIZAJE en la pista del propio ring, en vertical desde encima) La columna
                        // de la pista del ring (≤ 4,5 m de ella) no cuenta: aterrizar en el ring no es cruzarlo.
                        Vector3 toPad = p - ringPad; toPad.y = 0f;
                        hit = box.Contains(p) && (ringPadT == null || toPad.magnitude > 4.5f);
                    }
                    if (hit) { through++; if (firstBad.Length == 0) firstBad = $"{a.displayName} → {b.displayName}"; break; }
                }
            }
        TestProbe.Check("las alfombras no atraviesan el ring (salen por debajo, suben por fuera o por encima)", flights > 0 && through == 0, $"{through}/{flights} vuelos lo cruzan{(firstBad.Length > 0 ? ": " + firstBad : "")}");
        int spotsOn = Enumerable.Range(0, ring.Capacity).Count(i => ring.OnMattress(ring.Spot(i)));
        TestProbe.Check("todas las celdas están sobre el colchón", spotsOn == ring.Capacity, $"{spotsOn}/{ring.Capacity}");
        Log($"ring: {ring.cells} × {ring.cells}, caben {ring.Capacity}, residentes {residents}, almohadas {(ring.pillows != null ? ring.pillows.childCount : 0)}");
    }

    // ── 2. Las camas de 4 ────────────────────────────────────────────────────────────────────────
    void Beds()
    {
        var rooms = Bedroom.All.Where(b => b != null).ToList();
        TestProbe.Check("hay casas del árbol", rooms.Count > 0, $"{rooms.Count}");
        var small = rooms.Where(b => b.sleeps < 4 || b.laneWidth * b.sleeps < 3.5f).Select(b => b.name).ToList();
        TestProbe.Check("cada cama de la arboleda es de 4 plazas (≥ 3,5 m de ancho)", small.Count == 0, string.Join(", ", small));
        Bedroom couple = rooms.FirstOrDefault(b => !string.IsNullOrEmpty(b.partner));
        if (couple != null)
        {
            GameObject o = WorkdayDirector.Body(couple.owner), p = WorkdayDirector.Body(couple.partner);
            Anima ao = o != null ? o.GetComponent<Anima>() : null, ap = p != null ? p.GetComponent<Anima>() : null;
            bool apart = ao != null && ap != null && couple.LaneOf(ao) != couple.LaneOf(ap) && (couple.LieSpot(ao) - couple.LieSpot(ap)).magnitude >= couple.laneWidth * 0.9f;
            TestProbe.Check($"la pareja ({couple.owner} y {couple.partner}) duerme en plazas distintas de su cama", apart);
        }
    }

    // ── 3 y 4. El horario y la guerra en vivo ────────────────────────────────────────────────────
    IEnumerator Live(PillowRing ring)
    {
        LeisureDirector ld = LeisureDirector.Instance;
        TimeController tc = TimeController.timeController;
        WorkdayDirector wd = WorkdayDirector.Instance;
        if (!TestProbe.Check("hay director del horario libre y reloj", ld != null && tc != null)) yield break;
        // El próximo sábado (día 5 de su semana), justo antes de las 20 h.
        int day = tc.Day + 1;
        while (day - LeisureRoster.WeekOf(day) * LeisureRoster.DaysPerWeek != PillowRing.Day) day++;
        float start = LeisureRoster.SlotHours[PillowRing.Slot];
        Rest(wd != null ? wd.staff : new string[0]);
        tc.SetClock(day, start - LeisureDirector.TogetherLead - 0.02f);   // (se sale con tiempo hacia las de todos a la vez)
        Rest(wd != null ? wd.staff : new string[0]);   // (ronda 7: el salto de reloj les deja hambre, casi todos iban a comer y quedaba 1 de 26 a quién medir)
        if (wd != null) wd.PlanDay(day);
        ld.Scan(true);
        GameObject any = WorkdayDirector.Body("Linduarda") ?? WorkdayDirector.Body("Goluis");
        LeisureDirector.Group gr = any != null ? ld.GroupOf(any) : ld.groups.FirstOrDefault(g => !g.micro);
        if (!TestProbe.Check("el santuario tiene horario libre", gr != null && gr.plan != null)) yield break;
        LeisureRoster.Activity act = gr.catalog.FirstOrDefault(a => a.id == PillowRing.ActivityId);
        if (!TestProbe.Check("la guerra de almohadas está en el catálogo del santuario (etiqueta de ocio de todos a la vez)", act != null && act.together, string.Join(", ", gr.catalog.Select(a => a.title)))) yield break;
        var inSlot = gr.plan.entries.Where(e => e.day == day && e.slot == PillowRing.Slot).ToList();
        int fighters = inSlot.Count(e => e.activity == act);
        TestProbe.Check("en su franja (sábado, 20 h) la tienen todos", fighters == gr.plan.people.Count && fighters > 0, $"{fighters}/{gr.plan.people.Count}");
        TestProbe.Check("caben todos a la vez en el ring", fighters <= ring.Capacity, $"{fighters} de {ring.Capacity}");
        TestProbe.Check("fuera de su franja no se reparte", gr.plan.entries.Count(e => e.activity == act) == fighters);
        // REGLA J16 (= S13; Berón 2026-11-04: los centros de entrenamiento y de ocio dan servicio a TODOS los miembros a la vez), con el
        // único validador, AreaCapacity.CheckActivities: el ring y cada actividad de ocio «para todos». Las de por turnos (crías, textil;
        // docs/centros-para-todos.md), al registro.
        var forAll = gr.catalog.Where(a => a.id == PillowRing.ActivityId || LeisureDirector.AllMembers.Contains(a.id)).Select(a => (a.title, a.capacity, a.Unlimited));
        var j16 = AreaCapacity.CheckActivities(forAll, AreaCapacity.Members());
        TestProbe.Check("regla J16: el ring y el ocio «para todos» tienen plazas para todos los miembros (AreaCapacity)", j16.Count > 0 && j16.All(l => l.ok), string.Join(" · ", j16));
        foreach (var a in gr.catalog.Where(a => a.id != PillowRing.ActivityId && !LeisureDirector.AllMembers.Contains(a.id) && !a.Unlimited))
            Log($"por turnos (justificado en docs/centros-para-todos.md): {a.title} {a.capacity}");

        // En vivo: toda la franja de las 20 h (acotado en tiempo real).
        PillowFight.ResetCounters();
        var names = inSlot.Where(e => e.activity == act).Select(e => e.person.name).ToList();
        var bond0 = new Dictionary<string, float>();
        foreach (string n in names) { Anima a = Body(n); Anima b = Body(names[(names.IndexOf(n) + 1) % names.Count]); if (a != null && b != null) bond0[n] = ConversationDirector.Bond(a, b); }
        int rec0 = LeisureDirector.Records.Count;
        LeisureDirector.ForceRun = true;
        float oldScale = Time.timeScale; Time.timeScale = speed;
        float perHour = tc.SecondsPerGameDay / 24f;
        float end = Time.realtimeSinceStartup + Mathf.Max(300f, perHour * 1.2f / speed * 4f);
        int maxOn = 0, maxHolding = 0;
        while (tc.Hour < start + 0.95f && tc.Day == day && Time.realtimeSinceStartup < end)
        {
            Rest(names);   // (el reloj va ×speed: sin esto, el hambre vuelve a pasar delante de la franja)
            int on = names.Count(n => { Anima a = Body(n); return a != null && ring.OnMattress(a.transform.position); });
            maxOn = Mathf.Max(maxOn, on);
            maxHolding = Mathf.Max(maxHolding, PillowFight.Fighters.Count(PillowFight.Holding));
            yield return null;
        }
        ld.Tick();
        var recs = LeisureDirector.Records.Skip(rec0).Concat(ld.Snapshot()).Where(x => !x.micro && x.day == day && x.slot == PillowRing.Slot && x.activity == act.title).ToList();
        var free = recs.Where(r => r.blocked < r.since * 0.5f).ToList();
        int arrived = free.Count(r => r.arrived), walked = free.Count(r => r.arrived && !r.fallback), jumps = recs.Sum(r => r.jumps);
        TestProbe.Check("van todos al ring (≥ 80 % de los que no atendían una necesidad)", free.Count > 0 && arrived >= 0.8f * free.Count, $"{arrived}/{free.Count} (de {recs.Count}); sin llegar: {string.Join(", ", free.Where(r => !r.arrived).Select(r => r.who))}; ocupados: {string.Join(", ", recs.Except(free).GroupBy(r => r.blockedBy ?? "?").Select(g => $"{g.Key} ×{g.Count()}"))}");
        TestProbe.Check("llegan andando y en alfombra, no puestos allí", arrived == 0 || walked == arrived, $"{walked}/{arrived} · {free.Sum(r => r.flown):0} m en alfombra");
        TestProbe.Check("están a la vez sobre el colchón (≥ 80 %)", maxOn >= 0.8f * Mathf.Max(1, free.Count), $"como mucho {maxOn} a la vez");
        TestProbe.Check("cogen almohada", maxHolding >= 0.8f * Mathf.Max(1, arrived), $"{maxHolding} con almohada a la vez; {PillowFight.PickedUp} cogidas");
        TestProbe.Check("se dan almohadazos y ríen", PillowFight.Hits >= arrived && PillowFight.Laughs >= 2 * arrived, $"{PillowFight.Hits} golpes, {PillowFight.Laughs} risas");
        TestProbe.Check("los golpes están acotados (≤ 40 por persona)", PillowFight.Hits <= PillowFight.MaxHitsPerSlot * Mathf.Max(1, names.Count));
        int grew = 0; foreach (string n in names) { Anima a = Body(n); Anima b = Body(names[(names.IndexOf(n) + 1) % names.Count]); if (a != null && b != null && bond0.TryGetValue(n, out float b0) && ConversationDirector.Bond(a, b) > b0) grew++; }
        TestProbe.Check("el vínculo sube entre los que luchan", grew > 0, $"{grew} parejas con más vínculo");
        TestProbe.Check("sin saltos ni teletransportes", jumps == 0, $"{jumps}: {string.Join(", ", recs.Where(r => r.jumps > 0).Select(r => r.who))}");
        LeisureDirector.ForceRun = false; ld.ReleaseAll(gr);
        TestProbe.Check("al acabar dejan las almohadas en el ring", !PillowFight.Fighters.Any() && (ring.pillows == null || ring.pillows.Cast<Transform>().All(p => p.gameObject.activeSelf)));
        Time.timeScale = oldScale;
        Log($"en vivo: {arrived}/{free.Count} llegan, {maxOn} sobre el colchón, {PillowFight.Hits} golpes, {jumps} saltos");
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
