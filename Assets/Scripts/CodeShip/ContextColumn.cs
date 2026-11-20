using UnityEngine;

/// <summary>
/// LA COLUMNA DE CONTEXTO (CodeShip ux-acciones.md §1 reglas 1 y 5: «una sola acción principal, siempre en el mismo sitio»; abajo al centro,
/// encima de los mandos en los modos de caminar). Responde siempre a «¿qué hago ahora?»:
///  · si el paso de la historia se puede hacer aquí: «E · hablar / resolver el reto…»;
///  · si no, si hay una misión de puesto a mano (<see cref="StationMissions"/>): «R · Hacer: Cocinar huevos revueltos»;
///  · si no, lo siguiente de la lista de tareas, con su distancia y hacia dónde: «R · Ir: Revisa las tres cosas · 23 m ↗».
/// Encima, una línea con el contexto (dónde estás, qué es). R hace siempre la acción principal (E sigue siendo la de la historia). Además, un
/// MARCADOR en pantalla sobre el destino de lo siguiente (▼ nombre · metros), pegado al borde con una flecha si queda fuera de la vista.
/// Se oculta con un reto, una ventana grande o la tarjeta de la historia abiertos.
/// </summary>
public class ContextColumn : MonoBehaviour
{
    public const KeyCode ActKey = KeyCode.R;
    static ContextColumn _inst;

    /// <summary>Lo que se ve ahora (pruebas): la línea de contexto y la etiqueta de la acción.</summary>
    public static string Line { get; private set; } = "";
    public static string Action { get; private set; } = "";
    System.Action _act;
    Transform _marker; string _markerName = "";
    float _next;
    GUIStyle _line, _btn, _mk;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (_inst != null) return;
        var go = new GameObject("ContextColumn");
        DontDestroyOnLoad(go);
        _inst = go.AddComponent<ContextColumn>();
    }

    static bool Hidden()
    {
        if (ColdSanctuary.TestRunner.Running || CodeChallengeSheet.IsOpen || GameKeys.MenuOpen) return true;
        if (HudModals.Current == HudModals.Modal.Tareas || HudModals.Current == HudModals.Modal.Areas || HudModals.Current == HudModals.Modal.Misiones) return true;
        StoryMission sm = StoryMission.Instance;
        return sm != null && sm.CardShowing;
    }

    void Update()
    {
        if (Hidden()) return;
        if (Time.unscaledTime >= _next) { _next = Time.unscaledTime + 0.25f; Decide(); }
        if (_act != null && GameKeys.GetKeyDown(ActKey) && !GameKeys.DialogOpen) _act();
    }

    /// <summary>Decide la acción principal de ahora (pública para las pruebas).</summary>
    public void Decide()
    {
        Line = ""; Action = ""; _act = null; _marker = null;
        GameObject pgo = GameObject.FindGameObjectWithTag("Player");
        if (pgo == null) return;
        Vector3 at = pgo.transform.position;
        StoryMission sm = StoryMission.Instance;
        AutoTravel tv = AutoTravel.Instance;
        if (tv != null && tv.Traveling) { Line = $"Yendo a {tv.Label}"; Action = "Parar el viaje"; _act = () => tv.Cancel(); return; }

        // 1) La historia, aquí.
        if (sm != null && !sm.Finished && sm.CurrentStep != null && sm.InReach && sm.ActionVerb().Length > 0)
        {
            StoryMission.StoryStep s = sm.CurrentStep;
            Line = $"Historia · {s.goal}";
            Action = $"E · {Cap(sm.ActionVerb())}";
            _act = () => { if (s.kind == StoryMission.Kind.Code) sm.OpenChallenge(); else AutoTravel.Tap(KeyCode.E); };
            return;
        }
        // 2) Una misión de puesto, aquí.
        StationMissions st = StationMissions.Instance;
        StationMission here = st != null ? st.NearPlayer(at) : null;
        if (here != null)
        {
            Line = $"{StationMissions.CategoryLabel(here.template.category)} · {here.Where}{(here.fallback ? " (supuesto)" : "")}";
            Action = $"R · Hacer: {here.template.title}";
            _act = () => st.Begin(here);
            return;
        }
        // 3) Lo siguiente de la lista: ir.
        TaskEntry next = MicroTaskWindows.Tasks.Find(t => t.CanGo);
        if (next != null && next.target != null)
        {
            float d = TaskSources.Flat(next.target.position - at);
            Line = next.group == TaskEntry.Group.Historia ? $"Siguiente · {next.title}" : $"{TaskEntry.GroupLabel(next.group)} · {next.title}";
            Action = $"R · Ir · {TaskEntry.DistanceText(d)} {HudGate.Arrow(at, next.target.position)}";
            TaskEntry go = next;
            _act = () => MicroTaskWindows.Go(go);
            _marker = next.target; _markerName = TaskSources.PlaceName(next.target);
        }
        else if (sm != null && sm.CurrentStep != null && sm.CurrentStep.kind == StoryMission.Kind.Hold && sm.InReach)
            Line = $"Historia · {sm.CurrentStep.goal}{sm.ProgressText()}";
    }

    static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

    /// <summary>Dónde va la columna: abajo al centro, encima de los mandos táctiles en el móvil y encima del aviso de la historia en el ordenador.</summary>
    public static Rect Area(float h)
    {
        float w = Mathf.Min(460f, Screen.width - 16f);
        float bottom;
        if (WebLigera.Active) { float u = Mathf.Max(44f, Mathf.Min(Screen.width, Screen.height) * 0.13f); bottom = Screen.height - 2f * (u * 1.1f + u * 0.3f) - 8f; }
        else bottom = Screen.height - 104f;
        return new Rect((Screen.width - w) * 0.5f, bottom - h, w, h);
    }

    void OnGUI()
    {
        CsTheme.Use();   // (2026-11-19) el tema de CodeShip para los menús (UI/CsTheme.cs)
        if (Hidden() || (Line.Length == 0 && Action.Length == 0)) return;
        if (_line == null)
        {
            int f = WebLigera.Active ? 15 : 13;
            _line = new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter, fontSize = f, wordWrap = true };
            _btn = new GUIStyle(GUI.skin.button) { richText = true, fontSize = f + 1, fontStyle = FontStyle.Bold };
            _mk = new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter, fontSize = f - 1 };
        }
        DrawMarker();
        float row = WebLigera.Active ? Mathf.Max(44f, Mathf.Min(Screen.width, Screen.height) * 0.075f) : 30f;
        Rect r = Area(row + (Line.Length > 0 ? 26f : 0f) + 8f);
        GUI.Box(r, "");
        if (Line.Length > 0) GUI.Label(new Rect(r.x + 6f, r.y + 2f, r.width - 12f, 24f), Line, _line);
        if (Action.Length > 0 && _act != null)
        {
            string label = WebLigera.Active ? Action.Replace("R · ", "").Replace("E · ", "") : Action;
            if (GUI.Button(new Rect(r.x + 6f, r.yMax - row - 4f, r.width - 12f, row), label, _btn)) _act();
        }
    }

    /// <summary>▼ sobre el destino; si queda detrás o fuera de la vista, en el borde con una flecha.</summary>
    void DrawMarker()
    {
        Camera cam = Camera.main;
        if (_marker == null || cam == null) return;
        Vector3 wp = _marker.position + Vector3.up * 2.2f;
        Vector3 sp = cam.WorldToScreenPoint(wp);
        bool behind = sp.z < 0f;
        float x = sp.x, y = Screen.height - sp.y;
        if (behind) { x = Screen.width - x; y = Screen.height - y; }
        float m = 40f;
        bool off = behind || x < m || x > Screen.width - m || y < m || y > Screen.height - m;
        x = Mathf.Clamp(x, m, Screen.width - m); y = Mathf.Clamp(y, m, Screen.height - m);
        string glyph = !off ? "▼" : x <= m ? "◀" : x >= Screen.width - m ? "▶" : y <= m ? "▲" : "▼";
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        float d = p != null ? TaskSources.Flat(_marker.position - p.transform.position) : 0f;
        Color c = GUI.color; GUI.color = new Color(1f, 0.82f, 0.35f);
        GUI.Label(new Rect(x - 110f, y - 34f, 220f, 22f), $"<b>{_markerName}</b> · {TaskEntry.DistanceText(d)}", _mk);
        GUI.Label(new Rect(x - 20f, y - 14f, 40f, 28f), $"<size={_mk.fontSize + 8}>{glyph}</size>", _mk);
        GUI.color = c;
    }
}
