using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EL HORARIO DE ACTIVIDADES LIBRE (docs/horario-libre.md; Berón 2026-11-03: «además de la jornada de trabajo que es diaria vamos
/// a crear el horario de actividades libre, dicho horario se fabrica semanalmente llenando las horas libres del humano/humano-insecto,
/// se sigue la lógica de distribución de tareas de jornada»). Lógica pura (sin escena), como <see cref="WorkRoster"/>:
///  1. Las HORAS LIBRES de cada día son las que no cubren la jornada (8–16), las comidas (cena 19–20) ni el sueño (22–7):
///     16, 17, 18, 20 y 21 h → <see cref="SlotHours"/>, 5 franjas de 1 h; 35 por semana.
///  2. Una vez por SEMANA (7 días) se reparte cada franja de cada día. En cada franja se recorre el CATÁLOGO de actividades de ocio
///     y cuidado en su orden del momento y a cada actividad se le da UNA persona: la que MENOS veces la ha hecho (a igualdad, la que
///     hace más franjas que no la hace; y después, el orden fijo del personal). Luego la siguiente actividad, y así en vueltas
///     hasta que todos tienen la suya o no cabe nadie más (cada actividad tiene su CAPACIDAD: plazas del sitio).
///  3. El ORDEN DE ACTIVIDADES ROTA una posición en cada franja (la última pasa a ser la primera), como el orden de áreas de la
///     jornada rota cada día: así ninguna actividad se queda siempre con los «primeros».
///  4. Las cuentas de cada persona se guardan de una semana a otra (como <see cref="WorkRoster.Worker.count"/>): la rotación sigue.
/// Resultado: cada actividad se reparte por igual entre los personajes de la escena (<see cref="Spread"/>: la diferencia entre
/// quien más y quien menos veces la hizo; 0–1 con plazas suficientes).
/// </summary>
public class LeisureRoster
{
    public const int DaysPerWeek = 7;
    /// <summary>Hora de inicio de cada franja libre (1 h cada una): después de la jornada (16:00), sin la cena (19–20) ni el sueño (desde las 22).</summary>
    public static readonly float[] SlotHours = { 16f, 17f, 18f, 20f, 21f };
    public const float SlotLength = 1f;
    public const float DinnerHour = 19f, BedHour = 22f;

    /// <summary>Clases de actividad (las etiquetas de los edificios: trabajo, descanso, recuperación u ocio).</summary>
    public enum Kind { Ocio, Cuidado, Descanso, Recuperacion }

    public class Activity
    {
        public string id, title, venue;   // venue: dónde se hace (área del santuario, plantilla de edificio del microcosmos o «casa»)
        public Kind kind;
        public int capacity;              // plazas a la vez (≤ 0 = sin límite: cada uno en su casa)
        public string pose = "pie";       // cómo se hace: pie, sentado, meditar, ejercicio, baile, tumbado
        public Activity(string id, string title, Kind kind, string venue, int capacity, string pose = "pie")
        { this.id = id; this.title = title; this.kind = kind; this.venue = venue; this.capacity = capacity; this.pose = pose; }
        public bool Unlimited => capacity <= 0;
        public override string ToString() => $"{title} ({venue})";
    }

    public class Person
    {
        public string name;
        public readonly Dictionary<string, int> count = new Dictionary<string, int>();   // franjas que ha hecho cada actividad
        public readonly Dictionary<string, int> last = new Dictionary<string, int>();    // última franja (global) en que la hizo
        public Person(string name) { this.name = name; }
        public int Count(string id) => count.TryGetValue(id, out int c) ? c : 0;
        public int Last(string id) => last.TryGetValue(id, out int d) ? d : int.MinValue;
    }

    public class Entry
    {
        public Person person; public Activity activity;
        public int day, slot;   // día absoluto y franja (índice en SlotHours)
        public float Start => SlotHours[slot];
        public override string ToString() => $"día {day} {Start:00}h {person.name} → {activity.title}";
    }

    public class WeekPlan
    {
        public int week, firstDay;
        public readonly List<Entry> entries = new List<Entry>();
        public readonly List<string> unfilled = new List<string>();   // «persona día h»: franjas sin actividad (no cabía nadie más)
        public readonly List<Activity> activities = new List<Activity>();
        public readonly List<Person> people = new List<Person>();
        public Entry At(Person p, int day, int slot) => entries.FirstOrDefault(e => e.person == p && e.day == day && e.slot == slot);
        public IEnumerable<Entry> Of(Person p) => entries.Where(e => e.person == p).OrderBy(e => e.day).ThenBy(e => e.slot);
        /// <summary>Cuántos hacen <paramref name="a"/> en esa franja (para la capacidad).</summary>
        public int Load(Activity a, int day, int slot) => entries.Count(e => e.activity == a && e.day == day && e.slot == slot);
    }

    public readonly List<Activity> activities = new List<Activity>();
    public readonly List<Person> people = new List<Person>();   // orden fijo (el del personal)

    public Person Add(string name) { var p = new Person(name); people.Add(p); return p; }
    public Person Find(string name) => people.FirstOrDefault(p => p.name == name);

    public static int WeekOf(int day) => day >= 0 ? day / DaysPerWeek : (day - DaysPerWeek + 1) / DaysPerWeek;
    public static int GlobalSlot(int day, int slot) => day * SlotHours.Length + slot;

    /// <summary>La franja libre que empieza a la hora <paramref name="h"/> (−1 si es jornada, cena o sueño).</summary>
    public static int SlotAt(float h)
    {
        for (int i = 0; i < SlotHours.Length; i++) if (h >= SlotHours[i] && h < SlotHours[i] + SlotLength) return i;
        return -1;
    }

    /// <summary>Qué toca a la hora <paramref name="h"/> en el horario de un día (para el panel): jornada, cena, sueño o libre.</summary>
    public static string BlockAt(float h)
    {
        if (h >= WorkRoster.StartHour && h < WorkRoster.StartHour + 8f) return "jornada";
        if (h >= DinnerHour && h < DinnerHour + 1f) return "cena";
        if (h >= BedHour || h < 7f) return "sueño";
        if (SlotAt(h) >= 0) return "libre";
        return "desayuno";
    }

    /// <summary>Orden del catálogo en la franja global <paramref name="g"/>: rota una posición por franja (la última pasa a ser la primera).</summary>
    public List<Activity> Order(int g)
    {
        int n = activities.Count; var l = new List<Activity>(n);
        if (n == 0) return l;
        int shift = ((g % n) + n) % n;
        for (int i = 0; i < n; i++) l.Add(activities[((i - shift) % n + n) % n]);
        return l;
    }

    /// <summary>Reparte la semana <paramref name="week"/>: todas sus franjas libres, para todos. Registra las cuentas de cada uno.</summary>
    public WeekPlan Plan(int week)
    {
        var plan = new WeekPlan { week = week, firstDay = week * DaysPerWeek };
        plan.activities.AddRange(activities); plan.people.AddRange(people);
        for (int d = plan.firstDay; d < plan.firstDay + DaysPerWeek; d++)
            for (int s = 0; s < SlotHours.Length; s++) PlanSlot(plan, d, s);
        return plan;
    }

    void PlanSlot(WeekPlan plan, int day, int slot)
    {
        int g = GlobalSlot(day, slot);
        var free = new List<Person>(people);
        var left = activities.ToDictionary(a => a, a => a.Unlimited ? int.MaxValue : a.capacity);
        List<Activity> order = Order(g);
        bool progress = true;
        while (free.Count > 0 && progress)   // vueltas por el catálogo
        {
            progress = false;
            foreach (Activity a in order)
            {
                if (free.Count == 0) break;
                if (left[a] <= 0) continue;
                Person pick = Choose(a, free);
                if (pick == null) continue;
                free.Remove(pick); left[a]--;
                pick.count[a.id] = pick.Count(a.id) + 1; pick.last[a.id] = g;
                plan.entries.Add(new Entry { person = pick, activity = a, day = day, slot = slot });
                progress = true;
            }
        }
        foreach (Person p in free) plan.unfilled.Add($"{p.name} día {day} {SlotHours[slot]:00}h");
    }

    // La que menos veces la ha hecho → la que hace más franjas que no la hace → el orden fijo del personal.
    static Person Choose(Activity a, List<Person> free)
    {
        Person best = null;
        foreach (Person p in free)
        {
            if (best == null) { best = p; continue; }
            int cp = p.Count(a.id), cb = best.Count(a.id);
            if (cp < cb || (cp == cb && p.Last(a.id) < best.Last(a.id))) best = p;
        }
        return best;
    }

    /// <summary>UNIFORMIDAD de la semana: para cada actividad, la diferencia entre quien más y quien menos veces la hizo (la mayor de
    /// todas; 0 = perfecto). <paramref name="perPerson"/>: para cada persona, la diferencia entre su actividad más y menos hecha.</summary>
    public static int Spread(WeekPlan p, out int perPerson, out string worst)
    {
        int s = 0; perPerson = 0; worst = "";
        foreach (Activity a in p.activities)
        {
            if (p.people.Count == 0) break;
            int mx = int.MinValue, mn = int.MaxValue;
            foreach (Person q in p.people) { int c = p.entries.Count(e => e.person == q && e.activity == a); mx = System.Math.Max(mx, c); mn = System.Math.Min(mn, c); }
            if (mx - mn > s) { s = mx - mn; worst = $"{a.title}: de {mn} a {mx} veces"; }
        }
        foreach (Person q in p.people)
        {
            if (p.activities.Count == 0) break;
            int mx = int.MinValue, mn = int.MaxValue;
            foreach (Activity a in p.activities) { int c = p.entries.Count(e => e.person == q && e.activity == a); mx = System.Math.Max(mx, c); mn = System.Math.Min(mn, c); }
            perPerson = System.Math.Max(perPerson, mx - mn);
        }
        return s;
    }

    /// <summary>¿Alguna actividad con más gente que plazas en alguna franja? (lista de excesos; vacía = se respetan).</summary>
    public static List<string> OverCapacity(WeekPlan p)
    {
        var l = new List<string>();
        foreach (var g in p.entries.GroupBy(e => (e.activity, e.day, e.slot)))
            if (!g.Key.activity.Unlimited && g.Count() > g.Key.activity.capacity) l.Add($"{g.Key.activity.title} día {g.Key.day} {SlotHours[g.Key.slot]:00}h: {g.Count()}/{g.Key.activity.capacity}");
        return l;
    }
}
