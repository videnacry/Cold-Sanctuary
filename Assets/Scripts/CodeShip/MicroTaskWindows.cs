using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LAS VENTANAS «TAREAS» Y «ÁREAS» (portadas de CodeShip: src/ui/task-board.ts y src/ui/areas-window.ts; docs/plan-microcosmos-realismo.md
/// §4.1–4.2). Van en el sistema de ventanas de <see cref="HudModals"/> (de una en una, Esc las cierra) y se abren desde el menú árbol
/// (<see cref="MicroRootMenu"/>) o con F6 / F7.
///  · TAREAS: todas las tareas de ahora para el jugador en esta escena (<see cref="TaskSources"/>), por grupos, cada una con su distancia,
///    lo que lleva y «Ir», que cierra la ventana y lo lleva allí (<see cref="AutoTravel"/>). El engranaje cambia la lista por sus ajustes
///    (Andando / Teletransporte y «al llegar, hacer la acción») y pasa a «Lista» para volver, como en CodeShip.
///  · ÁREAS: los sitios de la escena (<see cref="AreaCatalog"/>) con «Aquí» donde está ella, qué se hace en cada uno e «Ir».
/// Móvil primero: en la web ligera, filas y botones ≥ 44 px y la ventana a todo lo ancho.
/// </summary>
public class MicroTaskWindows : MonoBehaviour
{
    static MicroTaskWindows _inst;
    /// <summary>¿Hay algo en la lista de tareas / de áreas de esta escena? (lo leen la pila y el menú; se refresca cada segundo)</summary>
    public static bool HasTasks, HasAreas;
    public static List<TaskEntry> Tasks = new List<TaskEntry>();
    public static List<AreaEntry> Areas = new List<AreaEntry>();

    bool _settings;
    float _next;
    Vector2 _scroll;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (_inst != null) return;
        var go = new GameObject("MicroTaskWindows");
        DontDestroyOnLoad(go);
        _inst = go.AddComponent<MicroTaskWindows>();
    }

    /// <summary>Vuelve a leer las tareas y las áreas ya (las pruebas y el menú, sin esperar al segundo).</summary>
    public static void Refresh()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        Vector3 at = p != null ? p.transform.position : Vector3.zero;
        Tasks = TaskSources.Collect(at);
        Areas = AreaCatalog.Collect(at);
        _taskCounts = Counts(Tasks.ConvertAll(t => (int)t.group), System.Enum.GetNames(typeof(TaskEntry.Group)).Length);
        _areaCounts = Counts(Areas.ConvertAll(a => (int)a.kind), System.Enum.GetNames(typeof(AreaEntry.Kind)).Length);
        HasTasks = Tasks.Count > 0;
        HasAreas = Areas.Count > 0;
    }

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running) return;
        bool open = HudModals.Current == HudModals.Modal.Tareas || HudModals.Current == HudModals.Modal.Areas;
        if (Time.unscaledTime < _next) return;
        _next = Time.unscaledTime + (open ? 0.5f : 1.5f);
        Refresh();
        if (HudModals.Current == HudModals.Modal.Tareas && !HasTasks || HudModals.Current == HudModals.Modal.Areas && !HasAreas) HudModals.Close();
    }

    /// <summary>«Ir» a una tarea: cierra la ventana y echa a andar.</summary>
    public static bool Go(TaskEntry t)
    {
        if (t == null || !t.CanGo) return false;
        HudModals.Close();
        return AutoTravel.Ensure().Go(t.target, t.title, t.arrive, t.onArrive);
    }

    /// <summary>«Ir» a un área.</summary>
    public static bool Go(AreaEntry a)
    {
        if (a == null || a.target == null) return false;
        HudModals.Close();
        return AutoTravel.Ensure().Go(a.target, a.title, a.kind == AreaEntry.Kind.Historia ? 3f : 5f);
    }

    // ── El dibujo (2026-11-20, PB-169): la ventana a pantalla completa de CodeShip (.nav-map + .map-window, areas-window.css): cabecera
    // con sus botones de 44 px y la ✕, los chips de grupo con su cuenta, y cada entrada una fila con su botón cian «Ir». ───────────────

    bool _laid;
    int _taskChip = -1, _areaChip = -1;   // −1 = «Todas»

    void OnGUI()
    {
        CsTheme.Use();   // (2026-11-19) el tema de CodeShip para los menús (UI/CsTheme.cs)
        bool open = !ColdSanctuary.TestRunner.Running && !CodeChallengeSheet.IsOpen && (HudModals.Current == HudModals.Modal.Tareas || HudModals.Current == HudModals.Modal.Areas);
        if (!CsUi.Draw(ref _laid, open)) return;
        GUI.depth = -800;
        bool tareas = HudModals.Current == HudModals.Modal.Tareas;
        // Lo que se toca en este evento cambia el dibujo en el siguiente Layout, no a mitad (GUILayout pide el mismo número de piezas).
        bool settings = _settings;
        int taskChip = _taskChip, areaChip = _areaChip;
        Rect r = CsUi.WindowFrame();
        Rect body;
        int hit = tareas
            ? CsUi.WindowHead(r, _settings ? "Ajustes de tareas" : "Tareas", out body, _settings ? "☰ Lista" : "⚙ Ajustes")
            : CsUi.WindowHead(r, "Áreas", out body);
        if (hit == (tareas ? 1 : 0)) { HudModals.Close(); _settings = false; return; }
        if (tareas && hit == 0) { _settings = !_settings; _scroll = Vector2.zero; }

        GUILayout.BeginArea(body);
        if (tareas && !settings) Chips(_taskCounts, Tasks.Count, g => TaskEntry.GroupLabel((TaskEntry.Group)g), ref _taskChip);
        if (!tareas) Chips(_areaCounts, Areas.Count, k => AreaGroup((AreaEntry.Kind)k), ref _areaChip);
        _scroll = GUILayout.BeginScrollView(_scroll);
        if (tareas) { if (settings) DrawSettings(); else DrawTasks(taskChip); }
        else DrawAreas(areaChip);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    /// <summary>Los chips de grupo (.ar-chips): «Todas» y cada grupo que tenga algo, con su cuenta; el puesto, en cian.</summary>
    static void Chips(int[] count, int total, System.Func<int, string> label, ref int chosen)
    {
        int n = count.Length;
        if (chosen >= 0 && count[chosen] == 0 && Event.current.type == EventType.Layout) chosen = -1;
        GUILayout.BeginHorizontal();
        if (CsUi.Chip($"Todas  <size={CsUi.Fs(11f)}>{total}</size>", chosen < 0)) chosen = -1;
        for (int g = 0; g < n; g++)
        {
            if (count[g] == 0) continue;
            GUILayout.Space(CsUi.Px(6f));
            if (CsUi.Chip($"{label(g)}  <size={CsUi.Fs(11f)}>{count[g]}</size>", chosen == g)) chosen = g;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(CsUi.Px(8f));
    }

    /// <summary>Cuántas entradas hay de cada grupo (se cuentan al recoger la lista, no en cada evento de dibujo).</summary>
    static int[] Counts(List<int> groups, int n) { var c = new int[n]; foreach (int g in groups) c[g]++; return c; }
    static int[] _taskCounts = new int[0], _areaCounts = new int[0];

    static string AreaGroup(AreaEntry.Kind k) => k == AreaEntry.Kind.Historia ? "Sitios de la historia" : k == AreaEntry.Kind.Edificio ? "Edificios" : "Áreas del santuario";

    void DrawTasks(int chip)
    {
        if (Tasks.Count == 0) { GUILayout.Label("Nada pendiente aquí.", CsUi.RowDesc); return; }
        TaskEntry.Group? last = null; TaskEntry clicked = null;
        foreach (TaskEntry t in Tasks)
        {
            if (chip >= 0 && (int)t.group != chip) continue;
            if (last != t.group) { CsUi.SectionLabel(TaskEntry.GroupLabel(t.group)); last = t.group; }
            string title = (t.later ? "<color=#9fb3bd>" : "") + t.title + (t.progress.Length > 0 ? $"  <size={CsUi.Fs(12f)}>({t.progress})</size>" : "") + (t.later ? "</color>" : "");
            string where = t.hasPlace ? TaskEntry.DistanceText(t.distance) : null;
            if (CsUi.Row(title, t.detail, t.later ? "⊘ Luego" + (where != null ? " · " + where : "") : where, t.CanGo ? "Ir" : t.later ? "Luego" : "—", t.CanGo)) clicked = t;
        }
        GUILayout.Space(CsUi.Px(6f));
        GUILayout.Label($"«Ir» te lleva andando ({(AutoTravel.Teleport ? "ahora: teletransporte" : "o teletransporte en ⚙ Ajustes")}). Tocar el joystick o WASD para el viaje.", CsUi.NoteStyle);
        if (clicked != null) Go(clicked);   // después de cerrar los grupos del dibujo
    }

    void DrawSettings()
    {
        CsUi.SectionLabel("Viaje: cómo te lleva «Ir»");
        if (CsUi.SetRow("Andando", "Por el camino real", !AutoTravel.Teleport ? "Sí" : "No", !AutoTravel.Teleport)) AutoTravel.Teleport = false;
        if (CsUi.SetRow("Teletransporte", "Aparece allí al momento", AutoTravel.Teleport ? "Sí" : "No", AutoTravel.Teleport)) AutoTravel.Teleport = true;
        CsUi.SectionLabel("Al llegar");
        if (CsUi.SetRow("Hacer la acción de la tarea", "Hablar, abrir el reto…; si no, solo te lleva", AutoTravel.ActOnArrive ? "Sí" : "No", AutoTravel.ActOnArrive)) AutoTravel.ActOnArrive = !AutoTravel.ActOnArrive;
        CsUi.SectionLabel("Guía");
        if (CsUi.SetRow("Ver otra vez la guía de inicio", null, "", true)) { MicroOnboarding.Restart(); HudModals.Close(); _settings = false; }
    }

    void DrawAreas(int chip)
    {
        if (Areas.Count == 0) { GUILayout.Label("Esta escena no tiene áreas.", CsUi.RowDesc); return; }
        AreaEntry.Kind? last = null; AreaEntry clickedArea = null;
        foreach (AreaEntry a in Areas)
        {
            if (chip >= 0 && (int)a.kind != chip) continue;
            if (last != a.kind) { CsUi.SectionLabel(AreaGroup(a.kind)); last = a.kind; }
            string title = a.title + (a.here ? $"  <size={CsUi.Fs(11f)}><color=#7dffb0>● Aquí</color></size>" : $"  <size={CsUi.Fs(11f)}><color=#9fc6d6>· {TaskEntry.DistanceText(a.distance)}</color></size>");
            string can = a.canDo.Count == 0 ? null : string.Join("\n", a.canDo.GetRange(0, Mathf.Min(4, a.canDo.Count)));
            if (CsUi.Row(title, a.subtitle, can, a.here ? "Aquí" : "Ir", !a.here)) clickedArea = a;
        }
        if (clickedArea != null) Go(clickedArea);   // después de cerrar los grupos del dibujo
    }
}
