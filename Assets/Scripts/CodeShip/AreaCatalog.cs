using System.Collections.Generic;
using UnityEngine;

/// <summary>Un área de la ventana «Áreas» (CodeShip areas.ts: un nodo con su sitio, su nombre y adónde ir): qué es, qué se hace allí y si
/// ella está «Aquí».</summary>
public class AreaEntry
{
    public enum Kind { Historia, Edificio, Santuario }
    public string id = "", title = "", subtitle = "";
    public Kind kind;
    public Transform target;
    /// <summary>Lo que se puede hacer allí (las funciones del edificio, los pasos de la historia que van allí).</summary>
    public readonly List<string> canDo = new List<string>();
    public float distance;
    public bool here;
    /// <summary>Un puesto del edificio donde se hace algo (al llegar, el menú «Trabajar» lo usa); null si no hay.</summary>
    public AreaFunctionStation station;
}

/// <summary>
/// LAS ÁREAS DE LA ESCENA (docs/plan-microcosmos-realismo.md §4.2): los sitios de la historia (los rótulos grandes bajo la
/// <see cref="StoryMission"/>: la nave de montaje, el control de misión…), los edificios del pueblo (<see cref="AreaFunctionSite"/>, con
/// lo que el registro dice que se hace en ellos este año) y, en el santuario, sus áreas (<see cref="SanctuaryArea"/>). En orden: lo de la
/// historia, y lo demás de cerca a lejos. «Aquí» = a menos de <see cref="HereRadius"/> m.
/// </summary>
public static class AreaCatalog
{
    public const float HereRadius = 8f;

    public static List<AreaEntry> Collect(Vector3 player)
    {
        var l = new List<AreaEntry>();
        var seen = new HashSet<Transform>();

        StoryMission sm = StoryMission.Instance;
        if (sm != null && sm.steps != null)
        {
            // Los sitios de la historia: los rótulos grandes (≥ 2 m) que no son personas ni edificios del pueblo.
            foreach (WorldLabel wl in sm.GetComponentsInChildren<WorldLabel>())
            {
                Transform t = wl.transform;
                if (wl.height < 2f || t.GetComponent<Anima>() != null || t.GetComponentInParent<AreaFunctionSite>() != null || !seen.Add(t)) continue;
                var e = new AreaEntry { id = "historia-" + t.name, kind = AreaEntry.Kind.Historia, title = TaskSources.PlaceName(t), subtitle = "Sitio de la historia", target = t };
                for (int i = 0; i < sm.steps.Length; i++)
                {
                    StoryMission.StoryStep s = sm.steps[i];
                    if (s.targets == null || !System.Array.Exists(s.targets, x => x == t || (x != null && x.IsChildOf(t)))) continue;
                    e.canDo.Add((i < sm.Step ? "✓ " : i == sm.Step ? "▶ " : "· ") + s.goal);
                }
                l.Add(e);
            }
        }

        foreach (AreaFunctionSite site in Object.FindObjectsByType<AreaFunctionSite>(FindObjectsSortMode.None))
        {
            if (site == null || site.unreachable || !seen.Add(site.transform)) continue;
            var e = new AreaEntry
            {
                id = "edificio-" + site.GetEntityId(), kind = AreaEntry.Kind.Edificio, target = site.transform,
                title = string.IsNullOrEmpty(site.title) ? site.building : site.title,
                subtitle = site.building + (string.IsNullOrEmpty(site.activity) ? "" : $" · {site.activity}"),
            };
            if (!string.IsNullOrEmpty(site.interaction)) e.canDo.Add(site.interaction);
            var names = new HashSet<string>();
            foreach (var f in site.Functions()) if (names.Add(f.title) && names.Count <= 4) e.canDo.Add("· " + f.title);
            foreach (AreaFunctionStation st in site.GetComponentsInChildren<AreaFunctionStation>()) if (st.CanInteract) { e.station = st; break; }
            l.Add(e);
        }

        foreach (SanctuaryArea a in Object.FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
        {
            if (a == null || !seen.Add(a.transform)) continue;
            var e = new AreaEntry { id = "santuario-" + a.GetEntityId(), kind = AreaEntry.Kind.Santuario, target = a.transform, title = string.IsNullOrEmpty(a.displayName) ? a.name : a.displayName, subtitle = a.areaType.ToString() };
            if (!string.IsNullOrEmpty(a.description)) e.canDo.Add(a.description);
            l.Add(e);
        }

        foreach (AreaEntry e in l) { e.distance = TaskSources.Flat(e.target.position - player); e.here = e.distance <= HereRadius && Mathf.Abs(e.target.position.y - player.y) < 4f; }
        return Sort(l);
    }

    /// <summary>Lo de la historia primero; luego de cerca a lejos; a igualdad, por nombre.</summary>
    public static List<AreaEntry> Sort(List<AreaEntry> l)
    {
        l.Sort((a, b) =>
        {
            int c = (a.kind == AreaEntry.Kind.Historia ? 0 : 1).CompareTo(b.kind == AreaEntry.Kind.Historia ? 0 : 1); if (c != 0) return c;
            c = a.distance.CompareTo(b.distance); if (c != 0) return c;
            return string.CompareOrdinal(a.title, b.title);
        });
        return l;
    }
}
