using System.Linq;
using UnityEngine;

/// <summary>
/// PANEL DE LA RUTINA DEL GIMNASIO (docs/gimnasio-maquinas.md §15.7): para la persona elegida, su plan (reparto, días, objetivo,
/// semana y si es de descarga), la rutina de hoy (cada ejercicio con su máquina, series × repeticiones, carga, descanso y RPE),
/// dónde va ahora (ejercicio, serie i/n, repetición) y las últimas líneas del registro de series de todo el gimnasio.
/// Consola: <c>rutina</c> (abre/cierra) o <c>rutina(Nombre)</c>; ◀ ▶ cambian de persona.
/// </summary>
public class GymRoutinePanel : MonoBehaviour
{
    public static bool Show;
    public static string Who = "";
    static GymRoutinePanel _inst;
    GUIStyle _st, _small;
    Vector2 _scroll;

    /// <summary>Abre o cierra el panel; con <paramref name="who"/>, lo abre en esa persona.</summary>
    public static string Toggle(string who = null)
    {
        if (_inst == null) _inst = new GameObject("GymRoutinePanel").AddComponent<GymRoutinePanel>();
        if (!string.IsNullOrEmpty(who)) { Who = who; Show = true; }
        else Show = !Show;
        if (string.IsNullOrEmpty(Who)) Who = WorkdayDirector.Staff.FirstOrDefault(n => n != "Player") ?? "";
        if (!Show) return "Rutina del gimnasio: cerrado.";
        GymProgramming.TrainingPlan p = GymProgramming.For(Who, Today);
        return p == null ? $"«{Who}» no tiene plan de gimnasio (solo los humanos del santuario)." : $"Rutina del gimnasio: {p}";
    }

    static int Today => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    void OnGUI()
    {
        if (!Show) return;
        if (_st == null) { _st = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true, wordWrap = true }; _small = new GUIStyle(GUI.skin.label) { fontSize = 11, richText = true, wordWrap = true }; }
        int day = Today;
        var r = new Rect(10f, 60f, 470f, Mathf.Min(Screen.height - 80f, 560f));
        GUI.Box(r, ""); GUI.Box(r, "");
        string[] staff = WorkdayDirector.Staff;
        int i = Mathf.Max(0, System.Array.IndexOf(staff, Who));
        if (GUI.Button(new Rect(r.x + 8f, r.y + 6f, 26f, 22f), "◀")) Who = staff[(i + staff.Length - 1) % staff.Length];
        if (GUI.Button(new Rect(r.x + 38f, r.y + 6f, 26f, 22f), "▶")) Who = staff[(i + 1) % staff.Length];
        if (GUI.Button(new Rect(r.xMax - 70f, r.y + 6f, 62f, 22f), "cerrar")) Show = false;
        GUI.Label(new Rect(r.x + 72f, r.y + 6f, r.width - 150f, 24f), $"<b><size=15>Gimnasio · {(Who == "Player" ? "Kushal" : Who)}</size></b>", _st);
        GymProgramming.TrainingPlan p = GymProgramming.For(Who, day);
        float y = r.y + 34f;
        void Line(string s, GUIStyle st = null) { GUIStyle u = st ?? _st; float h = u.CalcHeight(new GUIContent(s), r.width - 20f); GUI.Label(new Rect(r.x + 10f, y, r.width - 20f, h), s, u); y += h + 2f; }
        if (p == null) { Line("Sin plan de gimnasio (los humanos-insecto de los microcosmos no tienen: en sus eras no hay gimnasio)."); return; }
        Line($"<b>{GymProgramming.SplitName(p.split)}</b> · {p.goal} · {GymProgramming.Days(p.days)} · cada músculo {p.PerMuscleFrequency}×/sem");
        Line($"<color=#9AA0A6>{p.why}</color>", _small);
        Line($"Semana {p.week + 1}{(GymProgramming.IsDeload(p.week) ? " · <color=#E0B050>DESCARGA</color>" : "")} · sesiones hechas {p.SessionsDone}{(p.adjustedBy != null ? $" · revisado por {p.adjustedBy} ({p.reviews})" : "")}", _small);
        GymProgramming.DayRoutine rt = GymProgramming.Prescribe(Who, day);
        Line($"<b>Hoy ({HorarioPanel.DayName(day)}): {rt.name}</b>");
        GymTrainingSession s = null;
        WorkdayDirector wd = WorkdayDirector.Instance;
        GameObject body = WorkdayDirector.Body(Who);
        if (wd != null && body != null)
        {
            if (wd.GymSessions.TryGetValue(body, out GymTrainingSession x)) s = x;
            else if (wd.Coach != null && wd.Coach.g == body) { s = wd.Coach.own; Line($"<color=#7FC97F>{wd.Coach.Status} · clase {wd.Coach.attended} alumnos · {wd.Coach.corrections} correcciones · {wd.Coach.spots} de spotter</color>", _small); }
        }
        foreach (GymProgramming.Prescription x in s != null ? s.queue : rt.items)
        {
            bool now = s != null && s.Current == x && !s.Done;
            Line($"{(now ? "▶ " : "· ")}{x}", _small);
        }
        if (s != null) Line($"<b>Ahora:</b> {s.Status()}");
        y += 4f;
        Line("<b>Registro de series</b>", _small);
        var recent = GymTrainingSession.Recent;
        for (int k = Mathf.Max(0, recent.Count - 10); k < recent.Count && y < r.yMax - 16f; k++) Line(recent[k].Replace("[Gimnasio] ", ""), _small);
    }
}
