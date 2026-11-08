using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// QUIEN PASA EL DÍA EN OTRO SANTUARIO NO SE SIMULA AQUÍ (Berón 2026-11-08, por rendimiento; docs/jornada-y-rotacion.md §Reparto por santuarios).
/// La jornada reparte a cada uno en un sitio (<see cref="WorkRoster.DayPlan.place"/>). Si su día entero es en el Marino o en el Aéreo, su cuerpo se APAGA
/// en esta escena (SetActive false: ni se dibuja, ni anda, ni piensa), como el vuelo de <see cref="AereoLink"/> y la bajada de <see cref="SiteLine"/>
/// apagan al que viaja; sus misiones corren por el reloj de la jornada, igual que las de ellos. Vuelve al andén de su santuario (el muelle aéreo o la
/// estación de la línea) cuando un día le toca la tierra o en una FIESTA (<see cref="Feast"/>: una actividad de todos a la vez del horario libre),
/// porque en las fiestas se juntan todos en un santuario. Kushal nunca se apaga. Mientras corren las pruebas no se apaga a nadie (salvo <see cref="InTests"/>).
/// </summary>
public static class RemoteStaff
{
    /// <summary>El santuario de esta escena (SampleScene = 1).</summary>
    public const int SceneSanctuary = SanctuaryPlacement.Land;
    /// <summary>¿Hay una fiesta ahora? (lo pone <see cref="LeisureDirector"/> en la franja de una actividad de todos a la vez)</summary>
    public static bool Feast;
    /// <summary>Las pruebas lo encienden para probar el apagado.</summary>
    public static bool InTests;
    public static bool Enabled => !ColdSanctuary.TestRunner.Running || InTests;

    static readonly Dictionary<GameObject, int> _hidden = new Dictionary<GameObject, int>();
    public static int Count => _hidden.Count;
    public static bool Hidden(GameObject g) => g != null && _hidden.ContainsKey(g);

    /// <summary>Lógica pura: ¿se apaga hoy quien pasa el día en <paramref name="place"/>?</summary>
    public static bool ShouldHide(string name, SanctuaryPlacement.Home place, bool feast) => name != "Player" && !feast && place.sanctuary != SceneSanctuary;

    /// <summary>Aplica el reparto del día: apaga a quien está fuera y enciende a quien vuelve. <paramref name="body"/>: el cuerpo de cada uno.</summary>
    public static void Apply(WorkRoster.DayPlan plan, System.Func<string, GameObject> body, bool feast)
    {
        if (plan == null) return;
        var keep = new HashSet<GameObject>();
        foreach (var kv in plan.place)
        {
            if (kv.Key.clone) continue;
            GameObject g = body(kv.Key.name);
            if (g == null || g.CompareTag("Player")) continue;
            if (Enabled && ShouldHide(kv.Key.name, kv.Value, feast)) { Hide(g, kv.Value.sanctuary); keep.Add(g); }
        }
        foreach (GameObject g in new List<GameObject>(_hidden.Keys)) if (!keep.Contains(g)) Restore(g);
    }

    public static void Hide(GameObject g, int sanctuary)
    {
        if (g == null || _hidden.ContainsKey(g)) return;
        _hidden[g] = sanctuary;
        g.SetActive(false);
    }

    /// <summary>Vuelve: al andén de su santuario si existe (el muelle aéreo, la estación de la línea); si no, donde estaba.</summary>
    public static void Restore(GameObject g)
    {
        if (g == null) { _hidden.Remove(g); return; }
        if (!_hidden.TryGetValue(g, out int s)) return;
        _hidden.Remove(g);
        g.SetActive(true);
        Transform gate = s == SanctuaryPlacement.Aerial ? (AereoLink.Instance != null ? AereoLink.Instance.transform : null)
                       : s == SanctuaryPlacement.Marine ? (SiteLine.Instance != null ? SiteLine.Instance.transform : null) : null;
        if (gate != null) WorkdayDirector.Teleport(g.transform, gate.position + Vector3.up);
    }

    /// <summary>Todos de vuelta (al acabar las pruebas, o si se apaga la jornada).</summary>
    public static void RestoreAll() { foreach (GameObject g in new List<GameObject>(_hidden.Keys)) Restore(g); _hidden.Clear(); }
}
