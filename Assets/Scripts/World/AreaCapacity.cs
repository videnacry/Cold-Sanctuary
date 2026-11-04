using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// REGLA DE CAPACIDAD (Berón 2026-11-04; docs/manual-de-diseno.md, regla S13 = J16: ES LA MISMA REGLA y este validador es su ÚNICA
/// fuente; quien compruebe J16 —el horario libre, el ring de almohadas— llama a <see cref="Check"/> o <see cref="CheckActivities"/>). Cada piso o área tiene máquinas e infraestructura
/// para que VARIAS personas hagan la misma tarea a la vez, como en el restaurante o el hospital. Los centros de ENTRENAMIENTO y
/// de ESTUDIO son su propia categoría y deben servir a TODOS los miembros del santuario a la vez: el gimnasio, las dos salas de
/// yoga (Hatha y Ashtanga), el ring de almohadas y la sala de estudio (el Aula).
/// <see cref="Check"/> cuenta los puestos de cada área (los <see cref="AreaSeat"/>; en el gimnasio sus <see cref="GymStation"/>;
/// en el yoga sus esterillas «Esterilla_N») y los compara con lo que pide la regla: trabajo ≥ <see cref="MinWorkSeats"/>;
/// entrenamiento y estudio ≥ el número de miembros (<see cref="Members"/>).
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
    public static int Need(AreaSeat.Category c, int members) => c == AreaSeat.Category.Trabajo ? MinWorkSeats : members;

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
        public bool ok => have >= need;
        public override string ToString() => $"{area} · {activity} ({category}): {have}/{need}{(ok ? "" : " ✗")}";
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
            foreach (AreaSeat s in a.GetComponentsInChildren<AreaSeat>(true))
            {
                sum.TryGetValue(s.activity, out var v);
                sum[s.activity] = (s.category, v.n + s.seats);
                if (s.category == AreaSeat.Category.Estudio) study = true;
                if (s.activity.Contains("almohada")) ring = true;
            }
            foreach (var kv in sum)
                lines.Add(new Line { area = name, activity = kv.Key, category = kv.Value.c, have = kv.Value.n, need = Need(kv.Value.c, members) });
            // El GIMNASIO: sus máquinas (el piso del gimnasio y el de cardio cuentan juntos: es un mismo centro).
            if (a.name == "Gym_Area")
            {
                int n = a.GetComponentsInChildren<GymStation>(true).Length;
                Transform cardio = GameObject.Find("Cardio y zona libre")?.transform;
                if (cardio != null) n += cardio.GetComponentsInChildren<GymStation>(true).Length;
                lines.Add(new Line { area = "Gimnasio (con cardio)", activity = "máquinas", category = AreaSeat.Category.Entrenamiento, have = n, need = members });
                gym = true;
            }
            // El YOGA: dos salas (Hatha, el piso de la Sala de Yoga; Ashtanga, su planta propia), cada una para todos.
            if (a.name == "YogaRoom_Area")
            {
                Transform ash = a.Find("Planta_ashtanga");
                int h = 0, s2 = 0;
                foreach (Transform t in a.GetComponentsInChildren<Transform>(true))
                {
                    if (!t.name.StartsWith("Esterilla_")) continue;
                    if (ash != null && t.IsChildOf(ash)) s2++; else h++;
                }
                lines.Add(new Line { area = "Yoga Hatha", activity = "esterillas", category = AreaSeat.Category.Entrenamiento, have = h, need = members });
                lines.Add(new Line { area = "Yoga Ashtanga", activity = "esterillas", category = AreaSeat.Category.Entrenamiento, have = s2, need = members });
                hatha = true; ashtanga = ash != null;
            }
        }
        // El RING DE ALMOHADAS (ocio de todos a la vez; docs/guerra-de-almohadas.md): su capacidad, la de su colchón.
        foreach (PillowRing r in PillowRing.All)
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
