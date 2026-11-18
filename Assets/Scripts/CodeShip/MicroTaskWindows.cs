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
    GUIStyle _txt, _small, _head, _btn, _go;

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

    // ── El dibujo ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    static float Row => WebLigera.Active ? Mathf.Max(44f, Mathf.Min(Screen.width, Screen.height) * 0.075f) : 28f;

    void Styles()
    {
        if (_txt != null) return;
        int f = WebLigera.Active ? Mathf.Max(15, Mathf.RoundToInt(Row * 0.34f)) : 13;
        _txt = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true, fontSize = f };
        _small = new GUIStyle(_txt) { fontSize = f - 2 };
        _head = new GUIStyle(_txt) { fontSize = f + 3 };
        _btn = new GUIStyle(GUI.skin.button) { richText = true, fontSize = f };
        _go = new GUIStyle(_btn) { fontStyle = FontStyle.Bold };
    }

    /// <summary>La caja de la ventana: centrada como la de misiones; en el móvil, casi toda la pantalla (CodeShip: Tareas toma el ancho).</summary>
    public static Rect WindowRect()
    {
        if (WebLigera.Active) return new Rect(6f, 6f, Screen.width - 12f, Screen.height - 12f);
        float w = Mathf.Min(680f, Screen.width - 40f);
        return new Rect((Screen.width - w) * 0.5f, 60f, w, Mathf.Min(Screen.height - 120f, 620f));
    }

    void OnGUI()
    {
        if (ColdSanctuary.TestRunner.Running || CodeChallengeSheet.IsOpen) return;
        if (HudModals.Current != HudModals.Modal.Tareas && HudModals.Current != HudModals.Modal.Areas) return;
        Styles();
        GUI.depth = -800;
        Rect r = WindowRect();
        float row = Row;
        GUI.Box(r, ""); GUI.Box(r, ""); GUI.Box(r, "");
        bool tareas = HudModals.Current == HudModals.Modal.Tareas;
        string title = tareas ? (_settings ? "Ajustes de tareas" : "Tareas") : "Áreas";
        string keys = WebLigera.Active ? "" : $"  <size=11>({(tareas ? "F6" : "F7")} o Esc para cerrar)</size>";
        GUI.Label(new Rect(r.x + 12f, r.y + 6f, r.width - row * 4.5f, row), $"<b>{title}</b>{keys}", _head);
        if (GUI.Button(new Rect(r.xMax - row - 8f, r.y + 6f, row, row), "✕")) { HudModals.Close(); _settings = false; return; }
        if (tareas && GUI.Button(new Rect(r.xMax - row * 3.6f - 12f, r.y + 6f, row * 2.6f, row), _settings ? "☰ Lista" : "⚙ Ajustes")) { _settings = !_settings; _scroll = Vector2.zero; }

        var area = new Rect(r.x + 10f, r.y + row + 14f, r.width - 20f, r.height - row - 22f);
        GUILayout.BeginArea(area);
        _scroll = GUILayout.BeginScrollView(_scroll);
        if (tareas) { if (_settings) DrawSettings(row); else DrawTasks(row); }
        else DrawAreas(row);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void DrawTasks(float row)
    {
        if (Tasks.Count == 0) { GUILayout.Label("<color=#9AA0A6>Nada pendiente aquí.</color>", _txt); return; }
        TaskEntry.Group? last = null; TaskEntry clicked = null;
        foreach (TaskEntry t in Tasks)
        {
            if (last != t.group) { GUILayout.Space(4f); GUILayout.Label($"<b>{TaskEntry.GroupLabel(t.group)}</b>", _txt); last = t.group; }
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.BeginVertical();
            string col = t.later ? "<color=#9AA0A6>" : "<color=#FFFFFF>";
            GUILayout.Label($"{col}{(t.later ? "· " : "▶ ")}<b>{t.title}</b>{(t.progress.Length > 0 ? $"  ({t.progress})" : "")}</color>", _txt);
            GUILayout.Label($"<color=#B8C0C8>{t.detail}{(t.hasPlace ? $" · {TaskEntry.DistanceText(t.distance)}" : "")}</color>", _small);
            GUILayout.EndVertical();
            GUI.enabled = t.CanGo;
            if (GUILayout.Button(t.CanGo ? "Ir" : t.later ? "luego" : "—", _go, GUILayout.Width(row * 2f), GUILayout.MinHeight(row))) clicked = t;
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(6f);
        GUILayout.Label($"<color=#9AA0A6>«Ir» te lleva andando ({(AutoTravel.Teleport ? "ahora: teletransporte" : "o teletransporte en ⚙ Ajustes")}). Tocar el joystick o WASD para el viaje.</color>", _small);
        if (clicked != null) Go(clicked);   // después de cerrar los grupos del dibujo
    }

    void DrawSettings(float row)
    {
        GUILayout.Label("<b>Viaje</b>: cómo te lleva «Ir».", _txt);
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(!AutoTravel.Teleport, " Andando (por el camino real)", _btn, GUILayout.MinHeight(row))) AutoTravel.Teleport = false;
        if (GUILayout.Toggle(AutoTravel.Teleport, " Teletransporte", _btn, GUILayout.MinHeight(row))) AutoTravel.Teleport = true;
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.Label("<b>Al llegar</b>", _txt);
        bool act = GUILayout.Toggle(AutoTravel.ActOnArrive, AutoTravel.ActOnArrive ? " ✓ Hacer la acción de la tarea (hablar, abrir el reto…)" : " ✗ Solo llevarme allí", _btn, GUILayout.MinHeight(row));
        if (act != AutoTravel.ActOnArrive) AutoTravel.ActOnArrive = act;
        GUILayout.Space(8f);
        if (GUILayout.Button("Ver otra vez la guía de inicio", _btn, GUILayout.MinHeight(row))) { MicroOnboarding.Restart(); HudModals.Close(); _settings = false; }
    }

    void DrawAreas(float row)
    {
        if (Areas.Count == 0) { GUILayout.Label("<color=#9AA0A6>Esta escena no tiene áreas.</color>", _txt); return; }
        AreaEntry.Kind? last = null; AreaEntry clickedArea = null;
        foreach (AreaEntry a in Areas)
        {
            string group = a.kind == AreaEntry.Kind.Historia ? "Sitios de la historia" : a.kind == AreaEntry.Kind.Edificio ? "Edificios" : "Áreas del santuario";
            if (last != a.kind) { GUILayout.Space(4f); GUILayout.Label($"<b>{group}</b>", _txt); last = a.kind; }
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.BeginVertical();
            GUILayout.Label($"<b>{a.title}</b>{(a.here ? "  <color=#8EF29A>● Aquí</color>" : $"  <color=#B8C0C8>· {TaskEntry.DistanceText(a.distance)}</color>")}", _txt);
            GUILayout.Label($"<color=#B8C0C8>{a.subtitle}</color>", _small);
            for (int i = 0; i < Mathf.Min(4, a.canDo.Count); i++) GUILayout.Label(a.canDo[i], _small);
            GUILayout.EndVertical();
            GUI.enabled = !a.here;
            if (GUILayout.Button(a.here ? "Aquí" : "Ir", _go, GUILayout.Width(row * 2f), GUILayout.MinHeight(row))) clickedArea = a;
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
        if (clickedArea != null) Go(clickedArea);   // después de cerrar los grupos del dibujo
    }
}
