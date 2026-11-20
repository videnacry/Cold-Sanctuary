using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Una misión de la biblioteca puesta en un mueble de una escena (<see cref="StationMissions"/>).</summary>
public class StationMission
{
    public ChoreTemplate template;
    public Transform target;
    /// <summary>¿Su sitio es un marcador «(supuesto)» porque la escena aún no tiene el mueble?</summary>
    public bool fallback;
    public string scene = "";
    public int timesDone, timesFailed;
    /// <summary>El día del juego (o el momento real) en que se hizo por última vez: hasta el día siguiente no vuelve a pedirse.</summary>
    public int doneDay = int.MinValue;
    public float doneAt = -1e9f;

    public string Where => target != null ? TaskSources.PlaceName(target) : "";

    /// <summary>¿Está pendiente? (como en CodeShip, una tarea sale cuando hace falta: aquí, una vez por día del juego)</summary>
    public bool Available
    {
        get
        {
            if (target == null) return false;
            if (timesDone == 0) return true;
            TimeController tc = TimeController.timeController;
            return tc != null ? tc.Day > doneDay : Time.time - doneAt > StationMissions.RepeatSeconds;
        }
    }
}

/// <summary>
/// LAS MISIONES DE LOS PUESTOS (docs/plan-microcosmos-realismo.md §4.4; Berón 2026-11-18: «las misiones se crean como las de codeship»). En cada
/// escena cargada que tenga misiones en <see cref="MissionSites"/>, pone cada plantilla de <see cref="MissionLibrary"/> en su mueble, y:
///  · las enseña en la lista de Tareas (grupo «Trabajo y necesidades», con su categoría de CodeShip) y en el menú (Trabajar, Necesidades), con «Ir»;
///  · al llegar (o con R junto al puesto: la acción principal de la columna de contexto, <see cref="ContextColumn"/>) abre la hoja con sus retos en
///    orden (<see cref="ChoreRun"/>; plantar una idea, un <see cref="IdeaDuel"/> con la curva de dificultad);
///  · hecha: aviso, experiencia (marga de Stats; yoga en la de Yoga) y no vuelve a pedirse hasta el día siguiente; fallada: aviso y se reintenta.
/// </summary>
public class StationMissions : MonoBehaviour
{
    public const float RepeatSeconds = 300f;
    public const float Reach = 3f;
    public static StationMissions Instance { get; private set; }

    public readonly List<StationMission> All = new List<StationMission>();
    readonly HashSet<string> _bound = new HashSet<string>();
    float _nextScan;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() { Ensure(); }

    public static StationMissions Ensure()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("StationMissions_AUTO");
        DontDestroyOnLoad(go);
        return Instance = go.AddComponent<StationMissions>();
    }

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    public List<StationMission> In(Scene s) => All.FindAll(m => m.scene == s.name);

    /// <summary>Las pendientes ahora, de la más cercana a la más lejana.</summary>
    public List<StationMission> Pending(Vector3 from)
    {
        var l = All.FindAll(m => m.Available);
        l.Sort((a, b) => TaskSources.Flat(a.target.position - from).CompareTo(TaskSources.Flat(b.target.position - from)));
        return l;
    }

    void Update()
    {
        if (Time.unscaledTime < _nextScan) return;
        _nextScan = Time.unscaledTime + 2f;
        All.RemoveAll(m => m.target == null);   // su escena se descargó
        var loaded = new HashSet<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            loaded.Add(s.name);
            if (!_bound.Contains(s.name) && MissionSites.ForScene(s.name).Count > 0) BindScene(s);
        }
        _bound.RemoveWhere(n => !loaded.Contains(n));
    }

    /// <summary>Pone las misiones de la escena en sus muebles (una vez por carga). Devuelve cuántas.</summary>
    public int BindScene(Scene s)
    {
        All.RemoveAll(m => m.scene == s.name);
        _bound.Add(s.name);
        var roots = s.GetRootGameObjects();
        Vector3 center = Vector3.zero;
        StoryMission story = null;
        foreach (GameObject g in roots) { story = g.GetComponentInChildren<StoryMission>(true); if (story != null) break; }
        if (story != null) center = story.transform.position;
        var all = new List<Transform>();
        foreach (GameObject g in roots) all.AddRange(g.GetComponentsInChildren<Transform>(true));
        int fb = 0;
        foreach (MissionSites.Binding b in MissionSites.ForScene(s.name))
        {
            ChoreTemplate t = MissionLibrary.Find(b.template);
            if (t == null) { Debug.LogWarning($"[Misiones] {s.name}: la plantilla «{b.template}» no está en la biblioteca."); continue; }
            Transform target = Resolve(all, b, center, out bool fallback);
            if (target == null) { target = Marker(s, all, b, center); fallback = true; all.AddRange(target.GetComponentsInChildren<Transform>(true)); }
            if (fallback) fb++;
            All.Add(new StationMission { template = t, target = target, fallback = fallback, scene = s.name });
        }
        Debug.Log($"[Misiones] {s.name}: {All.FindAll(m => m.scene == s.name).Count} misiones en sus puestos ({fb} con marcador «supuesto»).");
        return All.FindAll(m => m.scene == s.name).Count;
    }

    /// <summary>El mueble de una misión: por nombre (exacto, luego contenido), por edificio y su puesto, por clave de puesto, por clase de pieza;
    /// a igualdad, el más cercano al centro de la historia. Un marcador «supuesto» ya puesto cuenta como hallado por nombre (fallback = true).</summary>
    static Transform Resolve(List<Transform> all, MissionSites.Binding b, Vector3 center, out bool fallback)
    {
        fallback = false;
        Transform Best(System.Predicate<Transform> ok)
        {
            Transform best = null; float bd = float.MaxValue;
            foreach (Transform t in all)
            {
                if (t == null || !t.gameObject.activeInHierarchy || !ok(t)) continue;
                float d = (t.position - center).sqrMagnitude + (t.GetComponent<AreaFunctionStation>() != null ? 0f : 0.01f);   // a igualdad, el que es puesto
                if (d < bd) { bd = d; best = t; }
            }
            return best;
        }
        foreach (string n in b.names) { Transform t = Best(x => x.name == n); if (t != null) { fallback = t.name == b.fallback; return t; } }
        foreach (string n in b.names) { Transform t = Best(x => x.name.Contains(n)); if (t != null) { fallback = t.name == b.fallback; return t; } }
        if (b.buildings.Length > 0)
        {
            Transform site = Best(x => x.TryGetComponent(out AreaFunctionSite a) && !a.unreachable && System.Array.IndexOf(b.buildings, a.building) >= 0);
            if (site != null)
            {
                foreach (AreaFunctionStation st in site.GetComponentsInChildren<AreaFunctionStation>()) if (b.stationKeys.Length == 0 || System.Array.IndexOf(b.stationKeys, st.station) >= 0) return st.transform;
                return site;
            }
        }
        if (b.stationKeys.Length > 0) { Transform t = Best(x => x.TryGetComponent(out AreaFunctionStation st) && System.Array.IndexOf(b.stationKeys, st.station) >= 0); if (t != null) return t; }
        if (b.pieceKinds.Length > 0) { Transform t = Best(x => x.TryGetComponent(out MicroPiece p) && System.Array.IndexOf(b.pieceKinds, p.kind) >= 0); if (t != null) return t; }
        if (!string.IsNullOrEmpty(b.fallback)) { Transform t = Best(x => x.name == b.fallback); if (t != null) { fallback = true; return t; } }
        return null;
    }

    /// <summary>El marcador «(supuesto)» de un mueble que la escena aún no tiene: una caja pequeña con su rótulo junto al ancla.</summary>
    static Transform Marker(Scene s, List<Transform> all, MissionSites.Binding b, Vector3 center)
    {
        Vector3 at = center;
        foreach (string a in b.anchor)
        {
            Transform hit = all.Find(t => t != null && t.name == a);
            if (hit != null) { at = hit.position; break; }
        }
        at += b.offset;
        if (UnityEngine.AI.NavMesh.SamplePosition(at, out UnityEngine.AI.NavMeshHit h, 4f, UnityEngine.AI.NavMesh.AllAreas)) at = h.position;
        var go = new GameObject(string.IsNullOrEmpty(b.fallback) ? b.template : b.fallback);
        SceneManager.MoveGameObjectToScene(go, s);
        go.transform.position = at;
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(box.GetComponent<Collider>());
        box.name = "Marcador_supuesto";
        box.transform.SetParent(go.transform, false);
        box.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        box.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        box.GetComponent<Renderer>().sharedMaterial = CheapMaterials.Opaque(new Color(0.95f, 0.75f, 0.25f));
        WorldLabel wl = go.AddComponent<WorldLabel>(); wl.text = go.name; wl.height = 1.2f;
        Debug.Log($"[Misiones] {s.name}: falta «{go.name}» en la escena: marcador puesto en ({at.x:0.0}, {at.y:0.0}, {at.z:0.0}). Que el constructor de la escena lo añada de verdad.");
        return go.transform;
    }

    // ── Hacerla ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Empieza la misión: un recorrido nuevo por sus retos (o un duelo de ideas) y, si <paramref name="openSheet"/>, la hoja.</summary>
    public ChoreRun Begin(StationMission m, bool openSheet = true)
    {
        if (m == null || m.template == null) return null;
        ChoreRun run = m.template.duel
            ? new IdeaDuel(MissionLibrary.IdeaBank, DifficultyProfile.For(DifficultyProfile.Level(m.template.duelLevel, m.timesDone)), (int)ChoreKit.Hash(m.template.id) + m.timesDone * 7919 + m.timesFailed, m.template)
            : new ChoreRun(m.template);
        if (openSheet) CodeChallengeSheet.OpenRun(run, m.template.title + (m.fallback ? " (supuesto)" : ""), m.template.scenario, r => End(m, r), () => Begin(m, false),
            (r, i, text) => StepAnimator.Play(m, i, r.Steps, text));   // cada paso acertado, animado en el puesto (CodeShip §4.2)
        Debug.Log($"[Misiones] Empieza «{m.template.title}» en «{m.Where}» ({run.Steps} retos).");
        return run;
    }

    /// <summary>Cierra una misión con su recorrido: hecha (experiencia, aviso) o fallada (aviso; se reintenta). Lo llama la hoja al acabar.</summary>
    public void End(StationMission m, ChoreRun run)
    {
        if (m == null || run == null) return;
        if (run.Status == ChoreRun.State.Done)
        {
            m.timesDone++;
            m.doneAt = Time.time;
            m.doneDay = TimeController.timeController != null ? TimeController.timeController.Day : int.MinValue;
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            CharacterLevel lv = p != null ? p.GetComponent<CharacterLevel>() : null;
            if (lv == null) lv = FindFirstObjectByType<CharacterLevel>();
            if (lv != null) { float xp = 10f + 4f * run.Steps; if (m.template.kind == "yoga") lv.GainYogaXp(xp); else if (m.template.category == ChoreCategory.Vinculos) lv.GainBondXp(xp); else lv.GainStatsXp(xp); }   // (PB-166) amistad, estudiante, combatir al lado, proteger: marga de Vínculos
            HudNotices.Push($"✓ Misión hecha: {m.template.title}", HudNotices.Kind.Good);
        }
        else if (run.Status == ChoreRun.State.Failed)
        {
            m.timesFailed++;
            HudNotices.Push($"✗ «{m.template.title}»: demasiados fallos. Vuelve a intentarlo.", HudNotices.Kind.Bad);
        }
    }

    /// <summary>La misión pendiente más cercana al jugador, si está a menos de <see cref="Reach"/> m (la columna de contexto).</summary>
    public StationMission NearPlayer(Vector3 at)
    {
        StationMission best = null; float bd = Reach;
        foreach (StationMission m in All)
        {
            if (!m.Available || Mathf.Abs(m.target.position.y - at.y) > 2.5f) continue;
            float d = TaskSources.Flat(m.target.position - at);
            if (d < bd) { bd = d; best = m; }
        }
        return best;
    }

    public static string CategoryLabel(ChoreCategory c) => c == ChoreCategory.Comida ? "Comida" : c == ChoreCategory.Plantas ? "Campo" : c == ChoreCategory.Animales ? "Animales"
        : c == ChoreCategory.Taller ? "Taller" : c == ChoreCategory.Salud ? "Salud" : c == ChoreCategory.Ciencia ? "Ciencia" : c == ChoreCategory.Seguridad ? "Seguridad"
        : c == ChoreCategory.Vinculos ? "Personas" : "Ideas";
}
