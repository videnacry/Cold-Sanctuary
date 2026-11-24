using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL MENÚ ÁRBOL (portado de CodeShip, src/ui/root-menu.ts y reglas.md §5.8; docs/microcosmos/plan-microcosmos-realismo.md §4.3). Un botón raíz «☰ Menú»
/// (F5) en el hueco siguiente de la pila de ventanas (<see cref="HudModals"/>): abajo a la derecha en el ordenador, arriba a la izquierda en el
/// móvil. Al pulsarlo se vuelve «✕ Menú» y brotan las ramas; tocar una rama la abre y enseña sus hojas (las acciones de ahora):
///  · Misiones: el objetivo de ahora (hacerlo aquí o ir), Tareas (F6), Misiones y jornada (F4), la guía de inicio.
///  · Moverse: Áreas (F7), Parar el viaje, Andando / Teletransporte, las áreas más cercanas.
///  · Trabajar: los puestos cercanos donde se hace algo (ir y hacerlo) y la tarea de la jornada.
///  · Necesidades: comer, dormir, ir al baño y lavarse en la casa más cercana (<see cref="Household"/>, <see cref="HomeNeedSpell"/>).
///  · Hablar: las personas cercanas (ir y pulsar E).
///  · Ventanas: Horario libre y Recursos, si la escena los tiene.
/// Las teclas sueltas siguen funcionando (E, F2–F4…): el menú solo las reúne. Reglas de CodeShip: solo se cierra con su ✕ (o F5), se oculta
/// un momento mientras hay un reto o una ventana grande abierta, y vuelve tal como estaba. Toques ≥ 44 px en la web.
/// </summary>
public class MicroRootMenu : MonoBehaviour
{
    public const KeyCode ToggleKey = KeyCode.F5;
    public static string KeyHint => WebLigera.Active ? "☰ Menú" : "F5 ☰ Menú";

    /// <summary>¿Está desplegado?</summary>
    public static bool Open { get; private set; }
    /// <summary>La rama abierta (−1 = ninguna).</summary>
    public static int OpenBranch = -1;

    static MicroRootMenu _inst;

    public class Leaf { public string label; public System.Action act; public bool enabled = true; }
    public class Branch { public string label; public readonly List<Leaf> leaves = new List<Leaf>(); }

    /// <summary>El árbol de ahora (se rehace cada segundo mientras está abierto): pruebas.</summary>
    public static readonly List<Branch> Tree = new List<Branch>();

    float _next;
    GUIStyle _btn, _leaf, _label;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        Open = false; OpenBranch = -1;
        if (_inst != null) return;
        var go = new GameObject("MicroRootMenu");
        DontDestroyOnLoad(go);
        _inst = go.AddComponent<MicroRootMenu>();
    }

    /// <summary>¿Sale el menú en esta escena? (hay tareas, áreas o algo de la pila)</summary>
    public static bool Available => MicroTaskWindows.HasTasks || MicroTaskWindows.HasAreas || HudModals.StackCount > 0;

    public static void Toggle(bool? on = null)
    {
        Open = on ?? !Open;
        if (Open) { Rebuild(); MicroOnboarding.Notice("menu"); }
    }

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running) return;
        if (GameKeys.GetKeyDown(ToggleKey) && Available) Toggle();
        if (Open && Time.unscaledTime >= _next) { _next = Time.unscaledTime + 1f; Rebuild(); }
    }

    /// <summary>El botón raíz: el hueco de la pila siguiente a los que hay.</summary>
    public static Rect RootRect() => HudModals.Slot(HudModals.StackCount);

    // ── El árbol ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    public static void Rebuild()
    {
        Tree.Clear();
        GameObject pgo = GameObject.FindGameObjectWithTag("Player");
        Transform player = pgo != null ? pgo.transform : null;
        Vector3 at = player != null ? player.position : Vector3.zero;

        // MISIONES
        var mis = new Branch { label = "Misiones" };
        StoryMission sm = StoryMission.Instance;
        if (sm != null && !sm.Finished && sm.CurrentStep != null)
        {
            StoryMission.StoryStep s = sm.CurrentStep;
            string verb = sm.ActionVerb();
            if (sm.InReach && verb.Length > 0)
                mis.leaves.Add(new Leaf { label = $"▶ {Cap(verb)}: {s.goal}", act = () => { if (s.kind == StoryMission.Kind.Code) sm.OpenChallenge(); else AutoTravel.Tap(KeyCode.E); } });
            else
            {
                TaskEntry first = MicroTaskWindows.Tasks.Find(t => t.group == TaskEntry.Group.Historia && t.CanGo);
                mis.leaves.Add(new Leaf { label = $"▶ Ir: {s.goal}", enabled = first != null, act = () => MicroTaskWindows.Go(first) });
            }
        }
        if (HudModals.Available(HudModals.Modal.Tareas)) mis.leaves.Add(new Leaf { label = Key("Tareas…", "F6"), act = () => HudModals.Toggle(HudModals.Modal.Tareas) });
        if (HudModals.Available(HudModals.Modal.Misiones)) mis.leaves.Add(new Leaf { label = Key("Misiones y jornada…", "F4"), act = () => HudModals.Toggle(HudModals.Modal.Misiones) });
        if (sm != null) mis.leaves.Add(new Leaf { label = "Guía: qué hago aquí", act = MicroOnboarding.Restart });
        Add(mis);

        // MOVERSE
        var mov = new Branch { label = "Moverse" };
        AutoTravel tv = AutoTravel.Instance;
        if (tv != null && tv.Traveling) mov.leaves.Add(new Leaf { label = $"■ Parar el viaje a {tv.Label}", act = () => tv.Cancel() });
        if (HudModals.Available(HudModals.Modal.Areas)) mov.leaves.Add(new Leaf { label = Key("Áreas…", "F7"), act = () => HudModals.Toggle(HudModals.Modal.Areas) });
        mov.leaves.Add(new Leaf { label = AutoTravel.Teleport ? "Viaje: teletransporte (cambiar a andando)" : "Viaje: andando (cambiar a teletransporte)", act = () => AutoTravel.Teleport = !AutoTravel.Teleport });
        int n = 0;
        foreach (AreaEntry a in MicroTaskWindows.Areas)
        {
            if (a.here || n >= 3) continue;
            AreaEntry e = a; n++;
            mov.leaves.Add(new Leaf { label = $"Ir: {a.title} · {TaskEntry.DistanceText(a.distance)}", act = () => MicroTaskWindows.Go(e) });
        }
        Add(mov);

        // TRABAJAR: los puestos cercanos donde se hace algo, y la tarea de la jornada.
        var work = new Branch { label = "Trabajar" };
        TaskEntry job = MicroTaskWindows.Tasks.Find(t => t.group == TaskEntry.Group.Jornada && t.CanGo);
        if (job != null) work.leaves.Add(new Leaf { label = $"▶ {job.title} · {TaskEntry.DistanceText(job.distance)}", act = () => MicroTaskWindows.Go(job) });
        Stations(work, at, c => c != ChoreCategory.Salud && c != ChoreCategory.Vinculos, 6);
        if (player != null)
        {
            var stations = new List<(AreaFunctionStation st, float d)>();
            foreach (AreaFunctionStation st in Object.FindObjectsByType<AreaFunctionStation>(FindObjectsSortMode.None))
            {
                if (st == null || !st.isActiveAndEnabled || Mathf.Abs(st.transform.position.y - at.y) > 4f) continue;
                float d = TaskSources.Flat(st.transform.position - at);
                if (d <= 60f && st.CanInteract) stations.Add((st, d));
            }
            stations.Sort((x, y) => x.d.CompareTo(y.d));
            for (int i = 0; i < Mathf.Min(4, stations.Count); i++)
            {
                AreaFunctionStation st = stations[i].st;
                string what = st.InteractLabel;
                work.leaves.Add(new Leaf { label = $"{what} · {TaskEntry.DistanceText(stations[i].d)}", act = () => AutoTravel.Ensure().Go(st.transform, what, 1.8f, () => { if (st != null && st.CanInteract) st.Interact(); }) });
            }
        }
        Add(work);

        // PERSONAS (2026-11-19, PB-166): hacerse amigo, ser estudiante, luchar a su lado o protegerle; cada conversación, retos de diálogo.
        var personas = new Branch { label = "Personas" };
        Stations(personas, at, c => c == ChoreCategory.Vinculos, 6);
        Add(personas);

        // NECESIDADES: en la casa más cercana.
        var needs = new Branch { label = "Necesidades" };
        Stations(needs, at, c => c == ChoreCategory.Salud, 6);
        Household home = null; float hd = float.MaxValue;
        foreach (Household h in Household.All)
        {
            if (h == null) continue;
            float d = TaskSources.Flat(h.transform.position - at) + Mathf.Abs(h.transform.position.y - at.y) * 3f;
            if (d < hd) { hd = d; home = h; }
        }
        if (home != null && player != null)
        {
            Need(needs, home, player.gameObject, HomeNeedSpell.Need.Comer, "Comer", home.SeatOf(player.gameObject));
            Need(needs, home, player.gameObject, HomeNeedSpell.Need.Dormir, "Dormir", home.BedOf(player.gameObject));
            Need(needs, home, player.gameObject, HomeNeedSpell.Need.Bano, "Ir al baño", home.toilet);
            Need(needs, home, player.gameObject, HomeNeedSpell.Need.Lavarse, "Lavarse", home.washbasin);
        }
        Add(needs);

        // HABLAR: las personas cercanas.
        var talk = new Branch { label = "Hablar" };
        if (player != null)
        {
            var people = new List<(Transform t, string name, float d)>();
            foreach (WorldLabel wl in Object.FindObjectsByType<WorldLabel>(FindObjectsSortMode.None))
            {
                if (wl == null || wl.transform == player || wl.GetComponent<Anima>() == null) continue;
                Anima an = wl.GetComponent<Anima>();
                if (an.death || Mathf.Abs(wl.transform.position.y - at.y) > 4f) continue;
                if (wl.GetComponent<MicroJornada>() == null && wl.GetComponent<PersonLife>() == null && !IsStoryPerson(sm, wl.transform)) continue;   // personas, no fauna
                float d = TaskSources.Flat(wl.transform.position - at);
                if (d <= 40f) people.Add((wl.transform, TaskSources.PlaceName(wl.transform), d));
            }
            people.Sort((x, y) => x.d.CompareTo(y.d));
            for (int i = 0; i < Mathf.Min(5, people.Count); i++)
            {
                Transform t = people[i].t; string who = people[i].name;
                talk.leaves.Add(new Leaf { label = $"{who} · {TaskEntry.DistanceText(people[i].d)}", act = () => AutoTravel.Ensure().Go(t, who, 2f, () => AutoTravel.Tap(KeyCode.E)) });
            }
        }
        Add(talk);

        // VENTANAS
        var win = new Branch { label = "Ventanas" };
        if (HudModals.Available(HudModals.Modal.Horario)) win.leaves.Add(new Leaf { label = Key("Horario libre…", "F2"), act = () => HudModals.Toggle(HudModals.Modal.Horario) });
        if (HudModals.Available(HudModals.Modal.Recursos)) win.leaves.Add(new Leaf { label = Key("Recursos…", "F3"), act = () => HudModals.Toggle(HudModals.Modal.Recursos) });
        Add(win);

        if (OpenBranch >= Tree.Count) OpenBranch = -1;
    }

    /// <summary>(2026-11-18) Las misiones de los puestos de esa clase, de la más cercana a la más lejana: ir y hacerla (o hacerla ya, si está aquí).</summary>
    static void Stations(Branch b, Vector3 at, System.Predicate<ChoreCategory> which, int max)
    {
        StationMissions st = StationMissions.Instance;
        if (st == null) return;
        int n = 0;
        foreach (StationMission m in st.Pending(at))
        {
            if (!which(m.template.category) || n >= max) continue;
            StationMission mm = m; n++;
            float d = TaskSources.Flat(m.target.position - at);
            bool here = d <= StationMissions.Reach;
            b.leaves.Add(new Leaf
            {
                label = $"{(here ? "▶ " : "")}{StationMissions.CategoryLabel(m.template.category)}: {m.template.title} · {TaskEntry.DistanceText(d)}",
                act = () => { if (here) st.Begin(mm); else AutoTravel.Ensure().Go(mm.target, mm.template.title, 2f, () => st.Begin(mm)); },
            });
        }
    }

    static void Need(Branch b, Household home, GameObject player, HomeNeedSpell.Need need, string label, Transform spot)
    {
        if (spot == null) return;
        b.leaves.Add(new Leaf
        {
            label = $"{label} ({TaskSources.PlaceName(home.transform)})",
            act = () => AutoTravel.Ensure().Go(spot, label.ToLowerInvariant(), 1.6f, () =>
            {
                if (player == null || !player.TryGetComponent(out Anima a)) return;
                ActionQueue q = player.GetOrAdd<ActionQueue>();
                q.EnqueuePlayer(new HomeNeedSpell { need = need, home = home });
            }),
        });
    }

    static bool IsStoryPerson(StoryMission sm, Transform t)
    {
        if (sm == null || sm.steps == null) return false;
        foreach (StoryMission.StoryStep s in sm.steps) if (s.targets != null && System.Array.IndexOf(s.targets, t) >= 0) return true;
        return false;
    }

    static void Add(Branch b) { if (b.leaves.Count > 0) Tree.Add(b); }
    static string Key(string label, string key) => WebLigera.Active ? label : $"{label}  ({key})";
    static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

    /// <summary>Ejecuta la hoja <paramref name="leaf"/> de la rama <paramref name="branch"/> (el dibujo y las pruebas). true = la hizo.</summary>
    public static bool Run(int branch, int leaf)
    {
        if (branch < 0 || branch >= Tree.Count || leaf < 0 || leaf >= Tree[branch].leaves.Count) return false;
        Leaf l = Tree[branch].leaves[leaf];
        if (!l.enabled || l.act == null) return false;
        l.act();
        Rebuild();
        return true;
    }

    // ── El dibujo ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    void OnGUI()
    {
        CsTheme.Use();   // (2026-11-19) el tema de CodeShip para los menús (UI/CsTheme.cs)
        if (ColdSanctuary.TestRunner.Running || GameKeys.MenuOpen || !Available) return;
        if (CodeChallengeSheet.IsOpen) return;   // con un reto, solo la hoja y la escena
        if (_btn == null)
        {
            int f = WebLigera.Active ? 15 : 13;
            _btn = new GUIStyle(GUI.skin.button) { richText = true, fontSize = f, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(10, 8, 2, 2) };
            _leaf = new GUIStyle(_btn) { wordWrap = true, fontSize = f - 1 };
            _label = new GUIStyle(GUI.skin.label) { richText = true, fontSize = f - 1 };
        }
        Rect root = RootRect();
        bool big = HudModals.Current == HudModals.Modal.Tareas || HudModals.Current == HudModals.Modal.Areas || HudModals.Current == HudModals.Modal.Misiones;
        if (big) return;   // la ventana grande ocupa la pantalla y tiene su ✕ (2026-11-20: también en el escritorio, como CodeShip)
        if (GUI.Button(root, Open ? "✕ Menú" : (WebLigera.Active ? "☰ Menú" : "F5: ☰ Menú"))) Toggle();
        MicroOnboarding.MenuButton = root;
        if (!Open || big) return;   // se oculta un momento con una ventana grande y vuelve tal como estaba

        float row = WebLigera.Active ? Mathf.Max(44f, root.height) : 26f, gap = 4f;
        float w = WebLigera.Active ? Mathf.Min(Screen.width - root.xMax - 16f, 340f) : 300f;
        if (WebLigera.Active && w < 200f) w = Mathf.Min(320f, Screen.width - 16f);
        // Altura: las ramas y las hojas de la abierta.
        int rows = Tree.Count + (OpenBranch >= 0 ? Tree[OpenBranch].leaves.Count : 0);
        float h = rows * (row + gap) + 8f;
        float x = WebLigera.Active ? (root.xMax + 8f + w <= Screen.width ? root.xMax + 8f : 8f) : root.x - w - 8f;
        float y = WebLigera.Active ? (root.xMax + 8f + w <= Screen.width ? root.y : root.yMax + 8f) : Mathf.Max(8f, root.yMax - h);
        h = Mathf.Min(h, Screen.height - y - 8f);
        var panel = new Rect(x, y, w, h);
        GUI.Box(panel, ""); GUI.Box(panel, "");
        if (Tree.Count == 0) { GUI.Label(new Rect(x + 8f, y + 4f, w - 16f, row), "<color=#9AA0A6>Nada que hacer aquí ahora.</color>", _label); return; }
        float cy = y + 4f;
        for (int b = 0; b < Tree.Count && cy + row <= panel.yMax; b++)
        {
            bool open = b == OpenBranch;
            if (GUI.Button(new Rect(x + 4f, cy, w - 8f, row), $"{(open ? "▾" : "▸")} <b>{Tree[b].label}</b>  <size={_btn.fontSize - 2}>({Tree[b].leaves.Count})</size>", _btn)) { OpenBranch = open ? -1 : b; return; }
            cy += row + gap;
            if (!open) continue;
            for (int i = 0; i < Tree[b].leaves.Count && cy + row <= panel.yMax; i++)
            {
                Leaf l = Tree[b].leaves[i];
                GUI.enabled = l.enabled;
                if (GUI.Button(new Rect(x + 20f, cy, w - 24f, row), l.label, _leaf)) { GUI.enabled = true; Run(b, i); return; }
                GUI.enabled = true;
                cy += row + gap;
            }
        }
    }
}
