using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// REGLA DE CAPACIDAD (Berón 2026-11-04; docs/reglas/manual-de-diseno.md, regla S13 = J16: ES LA MISMA REGLA y este validador es su ÚNICA
/// fuente; quien compruebe J16 —el horario libre, el ring de almohadas— llama a <see cref="Check"/> o <see cref="CheckActivities"/>). Cada piso o área tiene máquinas e infraestructura
/// para que VARIAS personas hagan la misma tarea a la vez, como en el restaurante o el hospital.
/// J16 REESCRITA (D1 de Berón 2026-11-25, docs/areas/rediseno-pisos-arboles.md): un piso por actividad. Los centros de ENTRENAMIENTO
/// ya no sirven cada uno a todos: la SUMA de sus plazas (las de los centros terminados) llega a todos los miembros, y el
/// entrenamiento de las 8 y las 14 reparte a la gente por plazas (<see cref="WorkRoster.SeatTraining"/>). Siguen siendo para TODOS a
/// la vez el ring de almohadas (la fiesta, J36), la sala de estudio (el Aula) y las Asambleas.
/// <see cref="Check"/> cuenta los puestos de cada área (los <see cref="AreaSeat"/>; en el gimnasio sus <see cref="GymStation"/>;
/// en el yoga sus esterillas «Esterilla_N») y los compara con lo que pide la regla: trabajo ≥ <see cref="MinWorkSeats"/>;
/// cada centro de entrenamiento ≥ 1 y todos juntos ≥ el número de miembros (<see cref="Members"/>; línea <see cref="TrainingTotal"/>);
/// estudio y ocio ≥ el número de miembros.
/// </summary>
public static class AreaCapacity
{
    /// <summary>«Varias personas a la vez»: lo mínimo por actividad de trabajo.</summary>
    public const int MinWorkSeats = 2;
    /// <summary>Miembros sin escena: la lista del personal (<see cref="WorkdayDirector.Staff"/>; 18 con el jugador el 2026-11-04).</summary>
    public static int DefaultMembers => WorkdayDirector.Staff.Length;

    /// <summary>Los miembros del santuario: el personal de la jornada (con el jugador).</summary>
    public static int Members()
    {
        WorkdayDirector wd = Object.FindAnyObjectByType<WorkdayDirector>();
        return wd != null && wd.staff != null && wd.staff.Length > 0 ? wd.staff.Length : WorkdayDirector.Staff.Length;
    }


    /// <summary>Lo que pide la regla para una actividad de esa categoría: trabajo, varias personas; entrenamiento, estudio y ocio,
    /// todos los miembros.</summary>
    public static int Need(AreaSeat.Category c, int members) => c == AreaSeat.Category.Trabajo ? MinWorkSeats : c == AreaSeat.Category.Entrenamiento ? 1 : members;
    /// <summary>La línea de la SUMA de plazas de entrenamiento (J16 reescrita): todos los centros juntos, para todos los miembros.</summary>
    public const string TrainingTotal = "Entrenamiento (suma de plazas)";

    /// <summary>Las PLAZAS de entrenamiento de un área (J16, D1): las máquinas del gimnasio (con su piso de cardio), las esterillas del
    /// yoga y los <see cref="AreaSeat"/> de entrenamiento, sin los pisos en obras. La usa la jornada para repartir por plazas, una
    /// vez al día (<c>WorkdayDirector.OpenTraining</c>).</summary>
    public static int SeatsOf(Transform a)
    {
        if (a == null) return 0;
        int n = 0;
        foreach (AreaSeat s in a.GetComponentsInChildren<AreaSeat>(true))
        {
            if (s.category != AreaSeat.Category.Entrenamiento) continue;
            ConstructionProject site = s.GetComponentInParent<ConstructionProject>(true);
            if (site == null || site.Finished) n += s.seats;
        }
        if (a.name == "Gym_Area")
        {
            n += a.GetComponentsInChildren<GymStation>(true).Length;   // (F1b, 2026-11-27) el cardio va en el mismo piso: ya cuenta aquí
        }
        if (a.name == "YogaRoom_Area")
            foreach (Transform t in a.GetComponentsInChildren<Transform>(true)) if (t.name.StartsWith("Esterilla_")) n++;
        return n;
    }

    /// <summary>LA MISMA REGLA para listas de actividades sin escena (el catálogo del horario libre, LeisureRoster: título y
    /// capacidad; <paramref name="unlimited"/> = sin tope). La usan quienes comprueban J16: AreaCapacity es la única fuente
    /// (manual S13 = J16).</summary>
    public static List<Line> CheckActivities(IEnumerable<(string title, int capacity, bool unlimited)> activities, int members, AreaSeat.Category category = AreaSeat.Category.Ocio)
    {
        var lines = new List<Line>();
        foreach (var (title, capacity, unlimited) in activities)
            lines.Add(new Line { area = title, activity = title, category = category, have = unlimited ? int.MaxValue : capacity, need = Need(category, members) });
        return lines;
    }
    public struct Line
    {
        public string area, activity; public AreaSeat.Category category; public int have, need;
        /// <summary>(2026-11-05) Puestos de los pisos del mismo centro que siguen EN OBRAS: aún no dan servicio (no suman en
        /// <see cref="have"/>), pero la regla mide el DISEÑO: un centro repartido en dos pisos, con uno terminado y otro en obras,
        /// cumple. Lo que falta sale en <c>pending</c>.</summary>
        public int planned;
        public bool ok => have + planned >= need;
        public override string ToString() => $"{area} · {activity} ({category}): {have}{(planned > 0 ? $"+{planned} en obras" : "")}/{need}{(ok ? "" : " ✗")}";
    }

    /// <summary>Lo que hay y lo que hace falta en cada área (<paramref name="areas"/>: raíces de área). Las líneas que fallan son
    /// las que incumplen la regla. <paramref name="pending"/>: los centros que la regla nombra y aún no existen (el ring de almohadas).</summary>
    public static List<Line> Check(IEnumerable<Transform> areas, int members, out List<string> pending)
    {
        var lines = new List<Line>(); pending = new List<string>();
        bool gym = false, hatha = false, ashtanga = false, ring = false, study = false;
        foreach (Transform a in areas)
        {
            if (a == null) continue;
            string name = a.GetComponent<SanctuaryArea>() is SanctuaryArea sa ? sa.displayName : a.name;
            // Los AreaSeat del área, por actividad.
            var sum = new Dictionary<string, (AreaSeat.Category c, int n)>();
            var building = new Dictionary<string, int>();   // los puestos de los pisos en obras, por actividad
            var buildingCat = new Dictionary<string, AreaSeat.Category>();
            foreach (AreaSeat s in a.GetComponentsInChildren<AreaSeat>(true))
            {
                // (2026-11-04) Un piso EN OBRAS no da servicio: sus puestos no cuentan hasta que se termine (el ballet, el dojo).
                ConstructionProject site = s.GetComponentInParent<ConstructionProject>(true);
                if (site != null && !site.Finished) { string p = $"{name} · {s.activity} (en obras)"; if (!pending.Contains(p)) pending.Add(p); building.TryGetValue(s.activity, out int b); building[s.activity] = b + s.seats; buildingCat[s.activity] = s.category; continue; }
                sum.TryGetValue(s.activity, out var v);
                sum[s.activity] = (s.category, v.n + s.seats);
                if (s.category == AreaSeat.Category.Estudio) study = true;
                if (s.activity.Contains("almohada")) ring = true;
            }
            foreach (var kv in sum)
            {
                building.TryGetValue(kv.Key, out int planned);
                lines.Add(new Line { area = name, activity = kv.Key, category = kv.Value.c, have = kv.Value.n, planned = planned, need = Need(kv.Value.c, members) });
            }
            // (2026-11-25) Una actividad con TODOS sus puestos en obras (el Aula, Informática… al empezar la partida) también lleva su línea:
            // 0 terminados y los de la obra, que cuentan para la regla (como el ballet y el dojo). Antes no salía y la prueba 75 no la encontraba.
            foreach (var kv in building)
                if (!sum.ContainsKey(kv.Key))
                    lines.Add(new Line { area = name, activity = kv.Key, category = buildingCat[kv.Key], have = 0, planned = kv.Value, need = Need(buildingCat[kv.Key], members) });
            // El GIMNASIO: sus máquinas, con el cardio (F1b, 2026-11-27: en el mismo piso, hijo del área).
            if (a.name == "Gym_Area")
            {
                int n = a.GetComponentsInChildren<GymStation>(true).Length;
                lines.Add(new Line { area = "Gimnasio (con cardio)", activity = "máquinas", category = AreaSeat.Category.Entrenamiento, have = n, need = Need(AreaSeat.Category.Entrenamiento, members) });
                gym = true;
            }
            // El YOGA: dos centros (Hatha, el piso de la Sala de Yoga; Ashtanga, su planta propia); desde la fase F1, un piso de cada uno (J16 reescrita: la suma llega a todos).
            if (a.name == "YogaRoom_Area")
            {
                // Por el ESTILO de su sala (YogaHall), en TODOS los pisos del estilo: si la suma de los centros no llega, el constructor
                // añade un piso (TrainingCentres.FloorsFor; «Planta_hatha_2», «Planta_ashtanga_2»…).
                int h = 0, s2 = 0;
                foreach (Transform t in a.GetComponentsInChildren<Transform>(true))
                {
                    if (!t.name.StartsWith("Esterilla_")) continue;
                    YogaHall hall = t.GetComponentInParent<YogaHall>(true);
                    if (hall != null && hall.style == YogaHall.Style.Ashtanga) s2++; else h++;
                }
                lines.Add(new Line { area = "Yoga Hatha", activity = "esterillas", category = AreaSeat.Category.Entrenamiento, have = h, need = Need(AreaSeat.Category.Entrenamiento, members) });
                lines.Add(new Line { area = "Yoga Ashtanga", activity = "esterillas", category = AreaSeat.Category.Entrenamiento, have = s2, need = Need(AreaSeat.Category.Entrenamiento, members) });
                hatha = true; ashtanga = s2 > 0;
            }
        }
        // (J16 reescrita, D1 2026-11-25) LA SUMA de las plazas de entrenamiento: entre todos los centros caben todos los miembros.
        int tHave = 0, tPlanned = 0;
        foreach (Line l in lines) if (l.category == AreaSeat.Category.Entrenamiento) { tHave += l.have; tPlanned += l.planned; }
        if (gym || hatha || tHave + tPlanned > 0)
            lines.Add(new Line { area = TrainingTotal, activity = "plazas", category = AreaSeat.Category.Entrenamiento, have = tHave, planned = tPlanned, need = members });
        // El RING DE ALMOHADAS (ocio de todos a la vez; docs/areas/guerra-de-almohadas.md): su capacidad, la de su colchón.
        foreach (PillowRing r in Object.FindObjectsByType<PillowRing>(FindObjectsInactive.Include, FindObjectsSortMode.None))   // (no PillowRing.All: se llena en OnEnable, y esta regla corre también fuera de Play)
        {
            if (r == null) continue;
            lines.Add(new Line { area = "Ring de almohadas", activity = PillowRing.ActivityId, category = AreaSeat.Category.Ocio, have = r.Capacity, need = members });
            ring = true;
        }
        if (!gym) pending.Add("Gimnasio (no está en la escena)");
        if (!hatha) pending.Add("Sala de Yoga Hatha (no está en la escena)");
        if (!ashtanga) pending.Add("Sala de Yoga Ashtanga (no está en la escena)");
        if (!ring) pending.Add("Ring de almohadas (no está en la escena)");
        if (!study) pending.Add("Sala de estudio (no está en la escena)");
        return lines;
    }

    /// <summary>Lo mismo, con las áreas de la escena, y lo escribe en la consola («[Capacidad] …»). Devuelve las líneas que fallan.</summary>
    public static List<Line> LogScene()
    {
        var roots = new List<Transform>();
        foreach (SanctuaryArea a in Object.FindObjectsByType<SanctuaryArea>(FindObjectsInactive.Include, FindObjectsSortMode.None)) roots.Add(a.transform);
        int members = Members();
        var lines = Check(roots, members, out var pending);
        var bad = lines.FindAll(l => !l.ok);
        Debug.Log($"[Capacidad] {members} miembros; {lines.Count} actividades con puestos, {bad.Count} por debajo de la regla" + (bad.Count > 0 ? ": " + string.Join(" · ", bad) : ".")
                  + (pending.Count > 0 ? $" Pendientes: {string.Join(", ", pending)}." : ""));
        return bad;
    }
}
