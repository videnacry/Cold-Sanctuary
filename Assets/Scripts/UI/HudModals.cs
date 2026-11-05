using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA PILA DE BOTONES DE LA DERECHA Y SUS VENTANAS (Berón 2026-11-05, docs/interfaz-botones.md): encima del botón de F10, y con su
/// mismo ancho, van «Horario libre», «Recursos» y «Misiones y jornada». Cada uno abre una ventana centrada, de una en una; se cierra
/// con Esc, con su botón de la pila, con su tecla o con «cerrar». Las teclas F2, F3 y F4 estaban libres (F1 es HudGate y F10 la pausa).
/// Las ventanas leen las mismas fuentes que los paneles que ya había (<see cref="LeisureDirector"/>, <see cref="SanctuaryResources"/>,
/// <see cref="WorkdayDirector.Itinerary"/>), así que no llevan copia de nada: nunca se desincronizan.
/// Se instala solo en cualquier escena, sin reconstruir nada; si la escena no tiene el sistema de un botón, ese botón no sale y los
/// de encima bajan un hueco.
/// </summary>
public class HudModals : MonoBehaviour
{
    public enum Modal { None, Horario, Recursos, Misiones }

    public const KeyCode HorarioKey = KeyCode.F2, RecursosKey = KeyCode.F3, MisionesKey = KeyCode.F4;
    /// <summary>Lo que mide la pila: como el botón de F10 (SceneLauncher), que está a 72 px del borde de abajo.</summary>
    public const float Width = 185f, Height = 24f, Gap = 4f, MarginRight = 190f, PauseButtonTop = 72f;

    /// <summary>La ventana abierta (una como mucho).</summary>
    public static Modal Current;

    static HudModals _inst;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        Current = Modal.None;
        if (_inst != null) return;
        var go = new GameObject("HudModals");
        DontDestroyOnLoad(go);
        _inst = go.AddComponent<HudModals>();
    }

    /// <summary>El sitio de la pila número <paramref name="slot"/>: el 0, justo encima del botón de F10.</summary>
    public static Rect Slot(int slot) => new Rect(Screen.width - MarginRight, Screen.height - PauseButtonTop - (slot + 1) * (Height + Gap), Width, Height);

    /// <summary>¿Tiene esta escena lo que enseña la ventana?</summary>
    public static bool Available(Modal m)
    {
        switch (m)
        {
            case Modal.Horario: return LeisureDirector.Instance != null && LeisureDirector.Instance.groups.Count > 0;
            case Modal.Recursos: return SanctuaryResourceHUD.Instance != null && SanctuaryResources.HasInstance;
            case Modal.Misiones: return WorkdayDirector.Instance != null || StoryMission.Instance != null || MissionTracker.Instance != null;
            default: return false;
        }
    }

    /// <summary>Abre <paramref name="m"/> o la cierra si ya estaba abierta; si había otra, la cierra.</summary>
    public static void Toggle(Modal m)
    {
        Current = Current == m ? Modal.None : m;
        if (Current == Modal.Horario && LeisureDirector.Instance != null) LeisureDirector.Instance.Scan(true);
        if (Current == Modal.Misiones) Lines.Clear();
    }

    public static void Close() { Current = Modal.None; }

    /// <summary>Lo que hace Esc: cierra la ventana abierta. Si está abierto el lanzador, Esc se lo queda él (GameKeys lo enmudece); si hay un
    /// diálogo, primero el diálogo. true = cerró una.</summary>
    public static bool Escape()
    {
        if (Current == Modal.None || GameKeys.DialogOpen) return false;
        Close();
        return true;
    }

    static readonly Modal[] Order = { Modal.Horario, Modal.Recursos, Modal.Misiones };
    static string Label(Modal m) => m == Modal.Horario ? "F2: Horario libre" : m == Modal.Recursos ? "F3: Recursos" : "F4: Misiones y jornada";

    float _nextRefresh;
    Vector2 _scroll;
    GUIStyle _st;
    static readonly List<string> Lines = new List<string>();

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running) return;
        if (GameKeys.GetKeyDown(KeyCode.Escape) && Escape()) return;
        if (GameKeys.GetKeyDown(HorarioKey) && Available(Modal.Horario)) Toggle(Modal.Horario);
        if (GameKeys.GetKeyDown(RecursosKey) && Available(Modal.Recursos)) Toggle(Modal.Recursos);
        if (GameKeys.GetKeyDown(MisionesKey) && Available(Modal.Misiones)) Toggle(Modal.Misiones);
    }

    void OnGUI()
    {
        if (!ColdSanctuary.TestRunner.Running && !GameKeys.MenuOpen)
        {
            int slot = 0;
            foreach (Modal m in Order)
            {
                if (!Available(m)) continue;
                if (GUI.Button(Slot(slot), Current == m ? $"▸ {Label(m)}" : Label(m))) Toggle(m);
                slot++;
            }
        }
        if (Current == Modal.Misiones) DrawMisiones();
    }

    // ── La ventana de misiones y jornada ─────────────────────────────────────────────────────────

    /// <summary>Lo que enseña la ventana: las misiones aceptadas y la jornada de hoy. Público para la prueba (grupo 93).</summary>
    public static List<string> MisionesLines()
    {
        var l = new List<string>();
        int accepted = 0;
        l.Add("<b>Misiones aceptadas</b>");
        StoryMission sm = StoryMission.Instance;
        if (sm != null && sm.steps != null && sm.steps.Length > 0 && sm.Step < sm.steps.Length)
        {
            l.Add($"   Historia: <b>{sm.title}</b>  ({sm.Step}/{sm.steps.Length}) · {sm.steps[sm.Step].goal}");
            accepted++;
        }
        PlayerTaskRunner tr = PlayerTaskRunner.Instance;
        if (tr != null && tr.Active != null && !tr.Active.Finished)
        {
            l.Add($"   Tarea en curso: <b>{tr.Active.plan.title}</b>  ({tr.Active.index}/{tr.Active.plan.steps.Count})");
            accepted++;
        }
        if (MissionTracker.Instance != null)
            foreach (var (name, progress, target) in MissionTracker.Instance.Accepted())
            {
                l.Add($"   <b>{name}</b>  ({progress}/{target})");
                accepted++;
            }
        if (accepted == 0) l.Add("   <color=#9AA0A6>(ninguna: las historias y las tareas salen aquí al aceptarlas)</color>");
        l.Add("");
        l.Add("<b>Jornada de hoy</b>");
        List<string> day = WorkdayDirector.Instance != null ? WorkdayDirector.Instance.Itinerary(true) : null;
        if (day == null) l.Add("   <color=#9AA0A6>(sin jornada: aún no es miembro, o esta escena no la tiene)</color>");
        else foreach (string s in day) l.Add("   " + s);
        return l;
    }

    void DrawMisiones()
    {
        if (_st == null) _st = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 13, wordWrap = false };
        if (Lines.Count == 0 || Time.unscaledTime >= _nextRefresh) { Lines.Clear(); Lines.AddRange(MisionesLines()); _nextRefresh = Time.unscaledTime + 1f; }
        float w = Mathf.Min(640f, Screen.width - 40f), h = Mathf.Min(Screen.height - 120f, 50f + Lines.Count * 19f);
        var r = new Rect((Screen.width - w) * 0.5f, 60f, w, h);
        GUI.Box(r, ""); GUI.Box(r, "");
        GUI.Label(new Rect(r.x + 12f, r.y + 6f, w - 110f, 22f), "<b>✦ Misiones y jornada</b>  <size=11>(Esc o F4 para cerrar)</size>", _st);
        if (GUI.Button(new Rect(r.xMax - 80f, r.y + 6f, 68f, 22f), "cerrar")) Close();
        var view = new Rect(0f, 0f, w - 40f, Lines.Count * 19f);
        _scroll = GUI.BeginScrollView(new Rect(r.x + 8f, r.y + 32f, w - 16f, h - 40f), _scroll, view);
        for (int i = 0; i < Lines.Count; i++) GUI.Label(new Rect(4f, i * 19f, view.width, 19f), Lines[i], _st);
        GUI.EndScrollView();
    }
}
