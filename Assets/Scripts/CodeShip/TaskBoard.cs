using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UNA TAREA DE LA LISTA «TAREAS» (portado de CodeShip, src/modules/tasks/application/task-list.ts; docs/plan-microcosmos-realismo.md §4.1):
/// qué hay que hacer, dónde, a qué distancia y cuánto lleva, con «Ir» si tiene sitio. Datos puros: <see cref="TaskOrdering"/> los ordena y
/// <see cref="TaskSources"/> los saca de la escena.
/// </summary>
public class TaskEntry
{
    /// <summary>Los grupos, en el orden de la lista (CodeShip TASK_GROUPS: lo de la historia primero).</summary>
    public enum Group { Historia, Jornada, Mision }
    public static string GroupLabel(Group g) => g == Group.Historia ? "Historia" : g == Group.Jornada ? "Tarea de la jornada" : "Misiones aceptadas";

    public string id = "", title = "", detail = "", progress = "";
    public Group group;
    /// <summary>Adónde lleva «Ir» (null si no tiene sitio).</summary>
    public Transform target;
    public bool hasPlace;
    /// <summary>Metros en planta desde el jugador (lo pone quien la recoge).</summary>
    public float distance;
    /// <summary>Un paso que aún no toca (la historia va en orden): se ve, pero sin «Ir».</summary>
    public bool later;
    /// <summary>A cuántos metros del objetivo se considera que ha llegado (la E de la historia llega a 2,5 m).</summary>
    public float arrive = 2f;
    /// <summary>Lo que hace al llegar, si «al llegar, hacer» está puesto (pulsar E, abrir el reto…); null = nada.</summary>
    public System.Action onArrive;

    public bool CanGo => hasPlace && !later;

    /// <summary>La distancia escrita para el jugador: «aquí», «12 m», «1,5 km».</summary>
    public static string DistanceText(float m)
    {
        if (m < 1f) return "aquí";
        if (m < 1000f) return $"{Mathf.RoundToInt(m)} m";
        return (m / 1000f).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture).Replace('.', ',') + " km";
    }
}

/// <summary>El orden de la lista de tareas: por grupo (historia → jornada → misiones); dentro, lo de ahora antes que lo de después, lo
/// que tiene sitio antes y lo más cerca primero; a igualdad, por título (estable: ordenar dos veces da lo mismo).</summary>
public static class TaskOrdering
{
    public static List<TaskEntry> Sort(IEnumerable<TaskEntry> tasks)
    {
        var l = new List<TaskEntry>(tasks);
        var idx = new Dictionary<TaskEntry, int>();
        for (int i = 0; i < l.Count; i++) idx[l[i]] = i;
        l.Sort((a, b) =>
        {
            int c = a.group.CompareTo(b.group); if (c != 0) return c;
            c = a.later.CompareTo(b.later); if (c != 0) return c;
            c = b.hasPlace.CompareTo(a.hasPlace); if (c != 0) return c;
            c = a.distance.CompareTo(b.distance); if (c != 0) return c;
            c = string.CompareOrdinal(a.title, b.title); if (c != 0) return c;
            return idx[a].CompareTo(idx[b]);
        });
        return l;
    }
}

/// <summary>
/// DE DÓNDE SALEN LAS TAREAS DE LA ESCENA: la historia (<see cref="StoryMission"/>: el paso de ahora con cada sitio que falta, y los dos
/// siguientes como «después»), la tarea de la jornada en marcha (<see cref="PlayerTaskRunner"/>: su paso y su marcador) y las misiones
/// aceptadas (<see cref="MissionTracker"/>, sin sitio). Lo lee la ventana «Tareas» y el menú árbol.
/// </summary>
public static class TaskSources
{
    public static List<TaskEntry> Collect(Vector3 player)
    {
        var l = new List<TaskEntry>();
        StoryMission sm = StoryMission.Instance;
        if (sm != null && sm.steps != null && !sm.Finished)
        {
            StoryMission.StoryStep s = sm.CurrentStep;
            List<Transform> pending = sm.PendingTargets();
            string verb = sm.ActionVerb();
            for (int i = 0; i < pending.Count; i++)
            {
                Transform t = pending[i];
                l.Add(new TaskEntry
                {
                    id = $"historia-{sm.Step}-{i}", group = TaskEntry.Group.Historia,
                    title = s.goal + (pending.Count > 1 ? $": {PlaceName(t)}" : ""),
                    detail = $"Paso {sm.Step + 1} de {sm.steps.Length} · {PlaceName(t)}" + (verb.Length > 0 ? $" · al llegar: {verb}" : s.kind == StoryMission.Kind.Hold ? $" · al llegar: quédate quieto {s.seconds:0} s" : ""),
                    progress = sm.ProgressText().Trim(), target = t, hasPlace = true, distance = Flat(t.position - player),
                    arrive = s.kind == StoryMission.Kind.Code || s.kind == StoryMission.Kind.Reach || s.kind == StoryMission.Kind.Hold ? sm.reach * 1.2f : sm.reach * 0.8f,   // la E de hablar, recoger y atender llega a «reach»
                    onArrive = verb.Length == 0 ? null : s.kind == StoryMission.Kind.Code ? (System.Action)(() => sm.OpenChallenge()) : () => AutoTravel.Tap(KeyCode.E),
                });
            }
            if (pending.Count == 0) l.Add(new TaskEntry { id = $"historia-{sm.Step}", group = TaskEntry.Group.Historia, title = s.goal, detail = $"Paso {sm.Step + 1} de {sm.steps.Length}", progress = sm.ProgressText().Trim() });
            for (int k = sm.Step + 1; k < Mathf.Min(sm.steps.Length, sm.Step + 3); k++)
            {
                StoryMission.StoryStep n = sm.steps[k];
                Transform t = n.targets != null && n.targets.Length > 0 ? n.targets[0] : null;
                l.Add(new TaskEntry { id = $"historia-{k}", group = TaskEntry.Group.Historia, title = n.goal, detail = $"Después · paso {k + 1} de {sm.steps.Length}", target = t, hasPlace = t != null, later = true, distance = t != null ? Flat(t.position - player) : 0f });
            }
        }

        PlayerTaskRunner tr = PlayerTaskRunner.Instance;
        if (tr != null && tr.Active != null && !tr.Active.Finished)
        {
            GameObject mk = tr.Active.CurrentMarker;
            l.Add(new TaskEntry
            {
                id = "jornada-" + tr.Active.plan.title, group = TaskEntry.Group.Jornada, title = tr.Active.plan.title,
                detail = $"Paso {tr.Active.index + 1} de {tr.Active.Count}" + (mk != null ? $" · {PlaceName(mk.transform)}" : ""),
                progress = $"{tr.Active.index}/{tr.Active.Count}", target = mk != null ? mk.transform : null, hasPlace = mk != null,
                distance = mk != null ? Flat(mk.transform.position - player) : 0f, arrive = PlayerTaskRunner.Reach * 0.8f,
                onArrive = () => AutoTravel.Tap(KeyCode.E),
            });
        }

        if (MissionTracker.Instance != null)
            foreach (var (name, progress, target) in MissionTracker.Instance.Accepted())
                l.Add(new TaskEntry { id = "mision-" + name, group = TaskEntry.Group.Mision, title = name, progress = $"{progress}/{target}", detail = "Sin sitio fijo: se cumple haciendo" });
        return TaskOrdering.Sort(l);
    }

    /// <summary>El nombre que se ve de un sitio: su rótulo en el mundo, sin marcas («✓»), o el nombre del objeto.</summary>
    public static string PlaceName(Transform t)
    {
        if (t == null) return "";
        WorldLabel wl = t.GetComponent<WorldLabel>();
        string s = wl != null && !string.IsNullOrEmpty(wl.text) ? wl.text : t.name;
        s = s.Replace(" ✓", "");
        int nl = s.IndexOf('\n'); if (nl > 0) s = s.Substring(0, nl);
        return s;
    }

    public static float Flat(Vector3 d) { d.y = 0f; return d.magnitude; }
}
