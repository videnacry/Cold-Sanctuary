using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PIJAMADAS EN LAS CASAS DEL ÁRBOL (Berón 2026-11-04: «las camas son grandes para permitir que los residentes puedan hacer
/// pijamadas, se podría incluso crear literas para incrementar el número de personas que pueden participar en la pijamada»;
/// docs/pijamadas.md).
///  · Una vez por SEMANA (el viernes, en la franja libre de las 21 h) hay pijamada en la casa de UN anfitrión. El anfitrión ROTA cada
///    semana por las casas del árbol (<see cref="LeisureRoster.HostIndex"/>): una casa distinta cada semana, en su orden.
///  · Van el anfitrión (y su pareja, que vive allí) y los que quepan en la casa (<see cref="Bedroom.Capacity"/>: la cama-litera, de 4
///    con su piso de arriba: 8 plazas), elegidos por el horario libre como cualquier actividad: los que menos pijamadas llevan.
///  · Esa NOCHE duermen allí: <see cref="Bedroom.Of"/> devuelve la casa del anfitrión y cada invitado tiene su plaza (las de la cama
///    de abajo que no usan los que viven allí y después las de arriba). A la mañana siguiente (desde las <see cref="WakeHour"/>) cada uno vuelve a
///    su casa: la pijamada se olvida sola.
/// </summary>
public static class Sleepover
{
    public const string ActivityId = "pijamada";
    /// <summary>Día de la semana (0–6) y franja (índice de <see cref="LeisureRoster.SlotHours"/>): el viernes a las 21 h.</summary>
    public const int Day = 4, Slot = 4;
    /// <summary>Hora del día siguiente en que la pijamada acaba (cada uno vuelve a su casa).</summary>
    public const float WakeHour = 8f;

    struct Stay { public Bedroom room; public int spot, night; }
    static readonly Dictionary<Anima, Stay> _stays = new Dictionary<Anima, Stay>();

    /// <summary>Cuántos invitados duermen esta noche fuera de su casa.</summary>
    public static int Guests { get { Prune(); return _stays.Count; } }

    static bool Active(Stay s)
    {
        TimeController tc = TimeController.timeController;
        if (tc == null) return true;
        int d = tc.Day; float h = tc.Hour;
        return (d == s.night && h >= LeisureRoster.SlotHours[Slot] - 0.5f) || (d == s.night + 1 && h < WakeHour);
    }

    static void Prune()
    {
        var gone = new List<Anima>();
        foreach (var kv in _stays) if (kv.Key == null || kv.Value.room == null || !Active(kv.Value)) gone.Add(kv.Key);
        foreach (Anima a in gone) _stays.Remove(a);
    }

    /// <summary>La casa de la pijamada en la que <paramref name="a"/> duerme esta noche (null si esta noche duerme en la suya).</summary>
    public static Bedroom RoomOf(Anima a)
    {
        if (a == null || _stays.Count == 0) return null;
        if (!_stays.TryGetValue(a, out Stay s)) return null;
        if (s.room == null || !Active(s)) { _stays.Remove(a); return null; }
        return s.room;
    }

    /// <summary>Su plaza en <paramref name="room"/> si es invitado allí esta noche.</summary>
    public static bool SpotIn(Anima a, Bedroom room, out int spot)
    {
        spot = -1;
        if (a == null || !_stays.TryGetValue(a, out Stay s) || s.room != room || !Active(s)) return false;
        spot = s.spot; return true;
    }

    /// <summary>Apunta a <paramref name="a"/> a la pijamada de la noche <paramref name="night"/> en <paramref name="room"/>: le da la
    /// primera plaza libre (las de abajo que no usan los que viven allí, y luego las de arriba). Los que viven allí duermen en la
    /// suya. false = no queda plaza.</summary>
    public static bool Join(Anima a, Bedroom room, int night)
    {
        if (a == null || room == null) return false;
        if (room.Lives(a)) return true;
        if (_stays.TryGetValue(a, out Stay mine) && mine.room == room && mine.night == night) return true;
        Prune();
        var taken = new HashSet<int>();
        if (!string.IsNullOrEmpty(room.owner)) taken.Add((room.sleeps - 1) / 2);   // la plaza del dueño
        if (!string.IsNullOrEmpty(room.partner)) taken.Add((room.sleeps - 1) / 2 + 1);   // y la de su pareja
        foreach (var kv in _stays) if (kv.Value.room == room && kv.Value.night == night) taken.Add(kv.Value.spot);
        for (int i = 0; i < room.Capacity; i++)
        {
            if (taken.Contains(i)) continue;
            _stays[a] = new Stay { room = room, spot = i, night = night };
            Debug.Log($"[Pijamada] «{a.name}» duerme esta noche en {room.name} (plaza {i + 1} de {room.Capacity}).");
            return true;
        }
        return false;
    }

    /// <summary>Olvida todas las pijamadas (las pruebas).</summary>
    public static void Clear() => _stays.Clear();

    // ── En el horario libre (etiqueta de actividad) ─────────────────────────────────────────────────────────────────────

    /// <summary>Las casas que pueden ser anfitrionas en una escena: las del árbol con dueño que está en el horario libre, en su orden
    /// («Casa del árbol 1», 2…).</summary>
    public static List<Bedroom> Hosts(UnityEngine.SceneManagement.Scene s, ICollection<string> people)
    {
        var l = new List<Bedroom>();
        foreach (Bedroom b in Bedroom.All)
            if (b != null && b.gameObject.scene == s && !string.IsNullOrEmpty(b.owner) && b.Capacity > 1 && people.Contains(b.owner)) l.Add(b);
        l.Sort((x, y) => Number(x.name).CompareTo(Number(y.name)));
        return l;
    }
    static int Number(string n) { int i = n.Length; while (i > 0 && char.IsDigit(n[i - 1])) i--; return i < n.Length && int.TryParse(n.Substring(i), out int k) ? k : int.MaxValue; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        if (_registered) return; _registered = true;
        LeisureTags.Providers.Add((gr, sites) =>
        {
            if (gr.micro) return;
            var people = new HashSet<string>();
            foreach (GameObject b in gr.bodies) if (b != null) people.Add(b.name.Replace("_Post", ""));
            List<Bedroom> hosts = Hosts(gr.scene, people);
            if (hosts.Count == 0) return;
            TimeController tc = TimeController.timeController;
            int week = LeisureRoster.WeekOf(tc != null ? tc.Day : 0);
            Bedroom host = hosts[LeisureRoster.HostIndex(week, hosts.Count)];
            string venue = $"la casa de {host.owner}";
            var act = new LeisureRoster.Activity(ActivityId, $"Pijamada en la casa de {host.owner}", LeisureRoster.Kind.Ocio, venue, host.Capacity, ActivityId)
            { together = true, togetherDay = Day, togetherSlot = Slot, hosts = string.IsNullOrEmpty(host.partner) ? new[] { host.owner } : new[] { host.owner, host.partner } };
            gr.catalog.Add(act);
            gr.venues[act] = new LeisureDirector.Venue { title = venue, anchor = host.transform, capacity = host.Capacity, index = gr.catalog.Count - 1 };
        });
    }
    static bool _registered;
}
