using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CHECKLIST DE PROGRESO (hechizo «progreso»): una ventana con lo hecho (✓) y lo pendiente (☐):
///  · la historia en curso, paso a paso (si estás dentro de un microcosmos);
///  · los microcosmos: su historia terminada (HistoryCard) o, en los de área, una misión cumplida;
///  · tus misiones de hoy (de la jornada);
///  · las misiones de trabajo de cada área (las que ya has hecho alguna vez);
///  · las prácticas y lecciones completadas.
/// Se abre y se cierra con el mismo hechizo o con Esc; la rueda del ratón desplaza la lista.
/// </summary>
public class ProgressPanel : MonoBehaviour
{
    static ProgressPanel _instance;
    public static bool Open => _instance != null && _instance.enabled;

    readonly List<string> _lines = new List<string>();
    Vector2 _scroll;
    float _nextRefresh;
    GUIStyle _st;

    /// <summary>Abre la ventana (o la cierra si ya estaba abierta). Devuelve el resumen para la consola.</summary>
    public static string Toggle()
    {
        if (_instance == null)
        {
            var go = new GameObject("ProgresoPanel");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ProgressPanel>();
            _instance.enabled = false;
        }
        _instance.enabled = !_instance.enabled;
        _instance.Refresh();
        return _instance.enabled ? _instance.Summary : "Progreso cerrado.";
    }

    string Summary = "";

    static string Box(bool done) => done ? "<color=#8FE08F>✓</color>" : "<color=#9AA0A6>☐</color>";

    void Refresh()
    {
        _lines.Clear();
        _nextRefresh = Time.unscaledTime + 1f;
        // 1) La historia en curso.
        StoryMission sm = StoryMission.Instance;
        if (sm != null && sm.steps != null && sm.steps.Length > 0)
        {
            _lines.Add($"<b>Historia en curso: {sm.title}</b>  ({Mathf.Min(sm.Step, sm.steps.Length)}/{sm.steps.Length})");
            for (int i = 0; i < sm.steps.Length; i++)
                _lines.Add($"   {Box(i < sm.Step)} {(i == sm.Step ? "<b>" + sm.steps[i].goal + "</b>  ← ahora" : sm.steps[i].goal)}");
            _lines.Add("");
        }
        // 2) Las historias de los microcosmos.
        List<string> stories = Progress.StoryScenes();
        int sd = 0; foreach (string s in stories) if (Progress.StoryDone(s)) sd++;
        _lines.Add($"<b>Microcosmos</b>  ({sd}/{stories.Count})  <size=11>(historia terminada o, en los de área, una misión cumplida)</size>");
        foreach (string s in stories) _lines.Add($"   {Box(Progress.StoryDone(s))} {s.Replace("Microcosmos_", "").Replace('_', ' ')}");
        _lines.Add("");
        // 3) Hoy (la jornada).
        WorkdayDirector wd = WorkdayDirector.Instance;
        WorkRoster.Worker me = wd != null ? wd.roster.Find("Player") : null;
        if (wd != null && wd.today != null && me != null)
        {
            _lines.Add($"<b>Hoy · día {wd.today.day}</b>");
            foreach (var a in wd.today.Of(me))
            {
                bool done = me.lastDay.TryGetValue(a.mission.id, out int d) && d == wd.today.day;
                _lines.Add($"   {Box(done)} {a.start:00}:00  {a.mission.title} <size=11>({a.mission.area})</size>");
            }
            _lines.Add("");
        }
        // 4) Las misiones de trabajo de cada área.
        var byArea = new SortedDictionary<string, List<WorkRoster.Mission>>(System.StringComparer.Ordinal);
        foreach (WorkRoster.Mission m in AreaWork.Catalog) { if (!byArea.TryGetValue(m.area, out var l)) byArea[m.area] = l = new List<WorkRoster.Mission>(); l.Add(m); }
        int md = 0, mt = 0;
        foreach (var kv in byArea) foreach (var m in kv.Value) { mt++; if (MissionDone(me, m)) md++; }
        _lines.Add($"<b>Misiones de trabajo</b>  ({md}/{mt})");
        foreach (var kv in byArea)
        {
            int d = 0; foreach (var m in kv.Value) if (MissionDone(me, m)) d++;
            _lines.Add($"   <b>{kv.Key}</b>  ({d}/{kv.Value.Count})");
            foreach (var m in kv.Value) _lines.Add($"      {Box(MissionDone(me, m))} {m.title}");
        }
        _lines.Add("");
        // 5) Prácticas y lecciones.
        var pr = new List<string>(Progress.Practices()); pr.Sort(System.StringComparer.Ordinal);
        _lines.Add($"<b>Prácticas y lecciones completadas</b>  ({pr.Count})");
        if (pr.Count == 0) _lines.Add("   (ninguna todavía)");
        foreach (string p in pr) _lines.Add($"   {Box(true)} {p}");
        Summary = $"Progreso: historias {sd}/{stories.Count}, misiones de trabajo {md}/{mt}, prácticas {pr.Count}" + (sm != null ? $", historia en curso «{sm.title}» {Mathf.Min(sm.Step, sm.steps.Length)}/{sm.steps.Length}" : "") + ".";
    }

    static bool MissionDone(WorkRoster.Worker me, WorkRoster.Mission m) => Progress.MissionDone(m.id) || (me != null && me.Knows(m.id));

    void Update()
    {
        if (GameKeys.GetKeyDown(KeyCode.Escape)) { enabled = false; return; }
        if (Time.unscaledTime >= _nextRefresh) Refresh();
    }

    void OnGUI()
    {
        if (_st == null) _st = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 13, wordWrap = false };
        float w = Mathf.Min(620f, Screen.width - 40f), h = Mathf.Min(Screen.height - 120f, 22f + _lines.Count * 19f + 30f);
        var r = new Rect((Screen.width - w) * 0.5f, 60f, w, h);
        GUI.Box(r, "");
        GUI.Label(new Rect(r.x + 12f, r.y + 6f, w - 24f, 22f), "<b>✦ Progreso</b>  <size=11>(Esc o «progreso» para cerrar · rueda para desplazar)</size>", _st);
        var view = new Rect(0f, 0f, w - 40f, _lines.Count * 19f);
        _scroll = GUI.BeginScrollView(new Rect(r.x + 8f, r.y + 30f, w - 16f, h - 38f), _scroll, view);
        for (int i = 0; i < _lines.Count; i++) GUI.Label(new Rect(4f, i * 19f, view.width, 19f), _lines[i], _st);
        GUI.EndScrollView();
    }
}
