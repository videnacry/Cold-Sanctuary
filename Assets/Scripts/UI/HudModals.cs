using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA PILA DE BOTONES DE LA DERECHA Y SUS VENTANAS (Berón 2026-11-05, docs/sistemas/interfaz-botones.md): encima del botón de F10, y con su
/// mismo ancho, van «Horario libre», «Recursos» y «Misiones y jornada». Cada uno abre una ventana centrada, de una en una; se cierra
/// con Esc, con su botón de la pila, con su tecla o con «cerrar». Las teclas F2, F3 y F4 estaban libres (F1 es HudGate y F10 la pausa).
/// Las ventanas leen las mismas fuentes que los paneles que ya había (<see cref="LeisureDirector"/>, <see cref="SanctuaryResources"/>,
/// <see cref="WorkdayDirector.Itinerary"/>), así que no llevan copia de nada: nunca se desincronizan.
/// Se instala solo en cualquier escena, sin reconstruir nada; si la escena no tiene el sistema de un botón, ese botón no sale y los
/// de encima bajan un hueco.
/// </summary>
public class HudModals : MonoBehaviour
{
    /// <summary>(2026-11-18, CodeShip) Tareas y Áreas no van en la pila: se abren desde el menú árbol (<see cref="MicroRootMenu"/>) o con F6 y F7;
    /// las dibuja <see cref="MicroTaskWindows"/>.</summary>
    public enum Modal { None, Horario, Recursos, Misiones, Tareas, Areas }

    public const KeyCode HorarioKey = KeyCode.F2, RecursosKey = KeyCode.F3, MisionesKey = KeyCode.F4, TareasKey = KeyCode.F6, AreasKey = KeyCode.F7;
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
    public static Rect Slot(int slot) => WebLigera.Active ? WebSlot(slot) : new Rect(Screen.width - MarginRight, Screen.height - PauseButtonTop - (slot + 1) * (Height + Gap), Width, Height);
    /// <summary>(2026-11-13, web ligera) En el móvil, la pila va arriba a la izquierda (abajo están los joysticks) y a la medida del dedo.</summary>
    static Rect WebSlot(int slot) { float s = Mathf.Min(Screen.width, Screen.height), h = Mathf.Max(32f, s * 0.07f), w = Mathf.Min(h * 4.2f, s * 0.48f); return new Rect(8f, 8f + h * 1.1f + slot * (h + 6f), w, h); }
    /// <summary>(2026-11-18) Cuántos botones de la pila salen en esta escena: el menú árbol va en el hueco siguiente.</summary>
    public static int StackCount { get { int n = 0; foreach (Modal m in Order) if (Available(m)) n++; return n; } }

    /// <summary>¿Tiene esta escena lo que enseña la ventana?</summary>
    public static bool Available(Modal m)
    {
        switch (m)
        {
            case Modal.Horario: return LeisureDirector.Instance != null && LeisureDirector.Instance.groups.Count > 0;
            case Modal.Recursos: return SanctuaryResourceHUD.Instance != null && SanctuaryResources.HasInstance;
            case Modal.Misiones: return WorkdayDirector.Instance != null || StoryMission.Instance != null || MissionTracker.Instance != null;
            case Modal.Tareas: return MicroTaskWindows.HasTasks;
            case Modal.Areas: return MicroTaskWindows.HasAreas;
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
    static string Label(Modal m) => WebLigera.Active ? (m == Modal.Horario ? "Horario libre" : m == Modal.Recursos ? "Recursos" : "Misiones y jornada") : m == Modal.Horario ? "F2: Horario libre" : m == Modal.Recursos ? "F3: Recursos" : "F4: Misiones y jornada";

    float _nextRefresh;
    bool _laid;
    Vector2 _scroll;
    static readonly List<string> Lines = new List<string>();

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running) return;
        if (GameKeys.GetKeyDown(KeyCode.Escape) && Escape()) return;
        if (GameKeys.GetKeyDown(HorarioKey) && Available(Modal.Horario)) Toggle(Modal.Horario);
        if (GameKeys.GetKeyDown(RecursosKey) && Available(Modal.Recursos)) Toggle(Modal.Recursos);
        if (GameKeys.GetKeyDown(MisionesKey) && Available(Modal.Misiones)) Toggle(Modal.Misiones);
        if (GameKeys.GetKeyDown(TareasKey) && Available(Modal.Tareas)) Toggle(Modal.Tareas);
        if (GameKeys.GetKeyDown(AreasKey) && Available(Modal.Areas)) Toggle(Modal.Areas);
    }

    void OnGUI()
    {
        CsTheme.Use();   // (2026-11-19) el tema de CodeShip para los menús (UI/CsTheme.cs)
        bool big = Current == Modal.Misiones || Current == Modal.Tareas || Current == Modal.Areas;   // a pantalla completa, con su ✕
        if (!ColdSanctuary.TestRunner.Running && !GameKeys.MenuOpen && !big)
        {
            int slot = 0;
            foreach (Modal m in Order)
            {
                if (!Available(m)) continue;
                if (GUI.Button(Slot(slot), Current == m ? $"▸ {Label(m)}" : Label(m))) Toggle(m);
                slot++;
            }
        }
        if (CsUi.Draw(ref _laid, Current == Modal.Misiones)) DrawMisiones();
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
        // (2026-11-18) Las misiones de los puestos (CodeShip): hechas y pendientes, por grupo; y los últimos avisos.
        if (StationMissions.Instance != null && StationMissions.Instance.All.Count > 0)
        {
            var ms = StationMissions.Instance.All;
            l.Add($"   <b>Trabajo y necesidades</b>: {ms.FindAll(m => m.timesDone > 0).Count} hechas de {ms.Count}");
            foreach (ChoreCategory c in System.Enum.GetValues(typeof(ChoreCategory)))
            {
                var g = ms.FindAll(m => m.template.category == c);
                if (g.Count == 0) continue;
                l.Add($"      {StationMissions.CategoryLabel(c)}: " + string.Join(" · ", g.ConvertAll(m => (m.timesDone > 0 ? "✓ " : "") + m.template.title)));
            }
            accepted++;
        }
        if (HudNotices.Recent.Count > 0) { l.Add("   <b>Últimos avisos</b>"); for (int i = HudNotices.Recent.Count - 1; i >= 0; i--) l.Add("      " + HudNotices.Recent[i]); }
        if (accepted == 0) l.Add("   <color=#9AA0A6>(ninguna: las historias y las tareas salen aquí al aceptarlas)</color>");
        l.Add("");
        l.Add("<b>Jornada de hoy</b>");
        List<string> day = WorkdayDirector.Instance != null ? WorkdayDirector.Instance.Itinerary(true) : null;
        if (day == null) l.Add("   <color=#9AA0A6>(sin jornada: aún no es miembro, o esta escena no la tiene)</color>");
        else foreach (string s in day) l.Add("   " + s);
        return l;
    }

    /// <summary>(2026-11-20, PB-169) La ventana a pantalla completa de CodeShip (Misiones del santuario 1: .nav-map + .map-window), con su ✕.</summary>
    void DrawMisiones()
    {
        if (Event.current.type == EventType.Layout && (Lines.Count == 0 || Time.unscaledTime >= _nextRefresh)) { Lines.Clear(); Lines.AddRange(MisionesLines()); _nextRefresh = Time.unscaledTime + 1f; }
        GUI.depth = -800;
        Rect r = CsUi.WindowFrame();
        if (CsUi.WindowHead(r, "Misiones y jornada", out Rect body) == 0) { Close(); return; }
        GUILayout.BeginArea(body);
        _scroll = GUILayout.BeginScrollView(_scroll);
        foreach (string line in Lines)
        {
            if (line.Length == 0) { GUILayout.Space(CsUi.Px(6f)); continue; }
            bool head = line.StartsWith("<b>") && line.EndsWith("</b>");   // «Misiones aceptadas», «Jornada de hoy»
            if (head) CsUi.SectionLabel(line.Substring(3, line.Length - 7));
            else GUILayout.Label(line.TrimStart(), line.StartsWith("      ") ? CsUi.RowSmall : CsUi.Body);
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
