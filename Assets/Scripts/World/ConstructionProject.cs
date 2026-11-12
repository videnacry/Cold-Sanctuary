using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OBRA EN CURSO (docs/jornada-y-rotacion.md, docs/construction-simulation.md): un área que aún se está construyendo (ballet,
/// artes marciales, calistenia…). Cada día el equipo de Construcción hace dos misiones: LLEVAR el material del día (palés al
/// pie de la obra) y TRABAJARLO. Cuando se completan los días de obra, el área queda TERMINADA y sus misiones (entrenamientos)
/// se abren a todos.
///
/// PROGRESO VISIBLE (Berón 2026-11-04: «el progreso de las obras podría ser visual? da la impresión de que mágicamente se
/// termina cuando ha llegado el día»): las piezas del «Acabado» se reparten en ETAPAS por los días de obra, en orden de
/// construcción (cimientos y suelo → estructura → instalaciones → muebles y máquinas → acabados y decoración). Cada turno de
/// «Trabajar la obra» va colocando las piezas de ese día (crecen y bajan a su sitio mientras el equipo martillea y se levanta
/// polvo), los palés del material del día menguan según se gastan, el andamio se mueve a la parte que se construye y el
/// cartel dice la etapa y el porcentaje. Lo que se ve antes de terminar es una MAQUETA sin colisiones ni conductas
/// (<see cref="PreviewName"/>): nada se puede usar hasta que la obra acaba y aparece el «Acabado» de verdad.
/// El estado (días hechos y material en la obra) se guarda entre partidas; al cargar, la obra enseña las piezas de los días hechos.
/// </summary>
public class ConstructionProject : MonoBehaviour
{
    public static readonly List<ConstructionProject> All = new List<ConstructionProject>();

    /// <summary>Las ETAPAS de una obra, en orden de construcción.</summary>
    public static readonly string[] StageNames = { "Cimientos y suelo", "Estructura", "Instalaciones", "Muebles y máquinas", "Acabados y decoración" };
    public const string PreviewName = "Obra_en_progreso";

    public string title = "Obra";
    [Min(1)] public int daysRequired = 5;
    [Tooltip("Cuántas personas del equipo de Construcción trabajan a la vez cada día de obra: el día cuenta cuando han trabajado todas.")]
    [Min(1)] public int crewNeeded = 1;
    public int daysDone;
    public bool materialOnSite = true;   // el primer día ya hay material en la obra
    public float wallHeight = 3f;
    [Tooltip("Guardar el estado de la obra entre partidas (PlayerPrefs). Nunca mientras corren las pruebas.")]
    public bool persist = true;
    [Tooltip("(Berón 2026-11-09) Un área no básica vuelta a obra (AreasEnObra): sin «Acabado»; enseña su forma en construcción (FloorForms) hasta terminar, y sus tareas de obra son opcionales.")]
    public bool reform;

    public bool Finished => daysDone >= daysRequired;
    public float Progress => Mathf.Clamp01(daysDone / (float)daysRequired);
    public SanctuaryArea Area => GetComponent<SanctuaryArea>();

    readonly List<Transform> _walls = new List<Transform>();
    readonly List<float> _wallBaseY = new List<float>();
    Transform _material, _scaffold, _done;   // _done: el área ACABADA (oculta hasta terminar la obra)
    readonly HashSet<string> _crewToday = new HashSet<string>(); int _crewDay = int.MinValue;
    /// <summary>Quién ha trabajado ya hoy en la obra (el día avanza cuando son <see cref="crewNeeded"/>).</summary>
    public int CrewToday => _crewToday.Count;
    WorldLabel _label;
    bool _collected, _loaded;

    // ── Las PIEZAS de la maqueta, repartidas por días ──
    /// <summary>Una pieza del acabado en la maqueta de obra: su etapa, el día de obra en que se coloca (1..daysRequired) y
    /// cuánto está colocada (0 = aún no, 1 = en su sitio).</summary>
    public class Piece
    {
        public string name; public int stage, day; public Transform group; public Vector3 home; public float height; public float shown;
        public bool Placed => shown >= 1f;
    }
    readonly List<Piece> _pieces = new List<Piece>();
    Transform _preview;
    public IReadOnlyList<Piece> Pieces { get { Collect(); return _pieces; } }
    /// <summary>La maqueta de obra (piezas a la vista antes de terminar; sin colisiones ni conductas).</summary>
    public Transform Preview { get { Collect(); return _preview; } }

    // ── El TURNO de hoy ──
    float _dayProgress;          // cuánto del día de obra siguiente lleva hecho el equipo (0..1)
    int _progressFor = -1;       // para qué daysDone vale _dayProgress
    class Shift { public string who; public GameObject body; public float start, hours; public Transform hammer; public Vector3 handOff; }
    readonly List<Shift> _shifts = new List<Shift>();
    readonly List<Transform> _materialBits = new List<Transform>();
    readonly List<(Transform t, float born)> _dust = new List<(Transform, float)>();
    readonly List<Transform> _carpets = new List<Transform>();
    float _nextDust;
    Vector3 _scaffoldHome; bool _scaffoldHomeSet;

    /// <summary>Lo que se ha avanzado del día de obra que se está trabajando (0..1; las piezas de ese día van saliendo).</summary>
    public float DayProgress => Finished ? 0f : (_progressFor == daysDone ? _dayProgress : 0f);
    /// <summary>Porcentaje de obra hecho, contando el turno en curso.</summary>
    public float Percent => Finished ? 100f : 100f * Mathf.Clamp01((daysDone + DayProgress) / daysRequired);
    /// <summary>La etapa que se está construyendo (la de las piezas del día siguiente; terminada, la última).</summary>
    public int CurrentStage
    {
        get
        {
            Collect();
            if (Finished) return StageNames.Length - 1;
            foreach (Piece p in _pieces) if (p.day == daysDone + 1) return p.stage;
            foreach (Piece p in _pieces) if (p.day > daysDone) return p.stage;
            return Mathf.Min(StageNames.Length - 1, daysDone * StageNames.Length / Mathf.Max(1, daysRequired));
        }
    }
    public string CurrentStageName => StageNames[CurrentStage];
    /// <summary>¿Hay alguien trabajando la obra ahora mismo (un turno en curso)?</summary>
    public bool Working => _shifts.Count > 0;

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Start() { Load(); Collect(); Refresh(); }

    void Collect()
    {
        if (_collected) return;
        _collected = true;
        var stale = new List<GameObject>();
        foreach (Transform t in transform)
        {
            if (t.name.StartsWith("Wall_")) { _walls.Add(t); _wallBaseY.Add(t.localScale.y); }
            else if (t.name == "Material_del_dia") _material = t;
            else if (t.name == "Andamio_obra") _scaffold = t;
            else if (t.name == FloorForms.FinishedName) _done = t;
            else if (t.name == PreviewName) stale.Add(t.gameObject);   // (copiada con el piso: se rehace)
        }
        foreach (GameObject g in stale) Destroy(g);
        if (_material != null)
            foreach (Transform b in _material) if (b.name != "Pale" && b.GetComponent<Renderer>() != null) _materialBits.Add(b);
        if (_scaffold != null) { _scaffoldHome = _scaffold.localPosition; _scaffoldHomeSet = true; }
        foreach (CarpetPad c in GetComponentsInChildren<CarpetPad>(true)) _carpets.Add(c.transform);
        _label = GetComponent<WorldLabel>();
        if (_label == null) { _label = gameObject.AddComponent<WorldLabel>(); _label.height = 4.5f; }
        BuildPreview();
    }

    // ── PLAN DE OBRA: las piezas del acabado, por etapas y días ──────────────────────────────────────────────────

    /// <summary>La etapa de una pieza por su nombre (y, si el nombre no lo dice, por su forma: lo plano y ancho es suelo).</summary>
    public static int StageOf(string name, Vector3 size)
    {
        string n = Plain(name);
        if (Has(n, "suelo", "base", "lona", "tatami", "tarima", "parquet", "piso", "pavimento", "solera", "cimiento", "moqueta", "linoleo")) return 0;
        if (Has(n, "cartel", "planta", "maceta", "cuadro", "espejo", "decor", "cortina", "flor", "estatua", "poster", "bandera", "cojin", "jarron", "adorno", "tapiz", "mural", "letrero", "placa", "vinilo", "barniz")) return 4;
        if (Has(n, "luz", "lampara", "foco", "tubo", "cable", "grifo", "ducha", "lavabo", "fregadero", "enchufe", "ventil", "agua", "altavoz", "calefac", "radiador", "tuberia", "desague", "bombilla", "aspersor", "extractor", "climat")) return 2;
        if (Has(n, "poste", "pilar", "columna", "viga", "muro", "pared", "tabique", "marco", "jaula", "ring", "red_", "cuerda", "esquina", "barandilla", "estructura", "celosia", "arco", "portico", "travesa", "larguero", "armazon")) return 1;
        if (size.y < 0.2f && size.x * size.z >= 2f) return 0;   // plano y ancho: es suelo
        return 3;
    }

    static bool Has(string n, params string[] keys) { foreach (string k in keys) if (n.Contains(k)) return true; return false; }

    static string Plain(string s)
    {
        s = (s ?? "").ToLowerInvariant();
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (char c in s)
            switch (c)
            {
                case 'á': sb.Append('a'); break;
                case 'é': sb.Append('e'); break;
                case 'í': sb.Append('i'); break;
                case 'ó': sb.Append('o'); break;
                case 'ú': case 'ü': sb.Append('u'); break;
                case 'ñ': sb.Append('n'); break;
                default: sb.Append(c); break;
            }
        return sb.ToString();
    }

    /// <summary>El día de obra (1..<paramref name="days"/>) de la pieza <paramref name="i"/> de <paramref name="n"/> en orden de
    /// construcción: reparte las piezas por igual y, si hay al menos tantas piezas como días, cada día tiene alguna.</summary>
    public static int DayOf(int i, int n, int days) => Mathf.Clamp(i * days / Mathf.Max(1, n) + 1, 1, days);

    static bool Drawn(Transform t) => t.GetComponentsInChildren<Renderer>(true).Length > 0;

    /// <summary>Rehace la MAQUETA de obra desde el «Acabado»: una copia solo visual (malla y material, sin colisiones ni
    /// conductas) de cada pieza, ordenada por etapa y altura, con su día de obra.</summary>
    void BuildPreview()
    {
        _pieces.Clear();
        if (_done == null) return;
        // Las piezas: los hijos del acabado; si son pocas para los días de obra, se abren los grupos más grandes (la jaula →
        // lona, red y postes) para que cada día tenga lo suyo y cada pieza caiga en su etapa.
        var roots = new List<Transform>();
        var opened = new HashSet<Transform>();
        foreach (Transform c in _done) if (Drawn(c)) roots.Add(c);
        for (int guard = 0; guard < 64 && roots.Count < daysRequired * 3; guard++)
        {
            int best = -1, most = 1;
            for (int i = 0; i < roots.Count; i++)
            {
                if (opened.Contains(roots[i])) continue;
                int kids = 0;
                foreach (Transform k in roots[i]) if (Drawn(k)) kids++;
                if (kids >= 2 && kids > most) { most = kids; best = i; }
            }
            if (best < 0) break;
            Transform open = roots[best]; roots.RemoveAt(best); opened.Add(open);
            int at = best;
            if (open.GetComponent<Renderer>() != null) roots.Insert(at++, open);   // lo que tenga el grupo en sí va como pieza propia
            foreach (Transform k in open) if (Drawn(k)) roots.Insert(at++, k);
        }
        var rootSet = new HashSet<Transform>(roots);

        _preview = new GameObject(PreviewName).transform;
        _preview.SetParent(transform, false);
        _preview.localPosition = _done.localPosition; _preview.localRotation = _done.localRotation; _preview.localScale = _done.localScale;
        Matrix4x4 toDone = _done.worldToLocalMatrix;
        var order = new List<(Piece p, float bottom, int idx)>();
        for (int i = 0; i < roots.Count; i++)
        {
            Transform root = roots[i];
            Vector3 pivot = toDone.MultiplyPoint3x4(root.position);
            Transform g = new GameObject("Pieza_" + root.name).transform;
            g.SetParent(_preview, false); g.localPosition = pivot;
            bool any = false; Bounds b = new Bounds(pivot, Vector3.zero);
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!(r is MeshRenderer) || r.GetComponent<TMPro.TMP_Text>() != null) continue;
                if (OpenedBelow(r.transform, root, rootSet)) continue;   // es de otra pieza (un hijo que se abrió aparte)
                MeshFilter mf = r.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                Matrix4x4 m = toDone * r.transform.localToWorldMatrix;
                Transform v = new GameObject(r.name).transform;
                v.SetParent(g, false);
                v.localPosition = (Vector3)m.GetColumn(3) - pivot; v.localRotation = m.rotation; v.localScale = m.lossyScale;
                v.gameObject.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
                MeshRenderer mr = v.gameObject.AddComponent<MeshRenderer>();
                mr.sharedMaterials = r.sharedMaterials; mr.shadowCastingMode = r.shadowCastingMode; mr.receiveShadows = r.receiveShadows;
                Bounds mb = mf.sharedMesh.bounds;
                for (int cx = -1; cx <= 1; cx += 2) for (int cy = -1; cy <= 1; cy += 2) for (int cz = -1; cz <= 1; cz += 2)
                    b.Encapsulate(m.MultiplyPoint3x4(mb.center + Vector3.Scale(mb.extents, new Vector3(cx, cy, cz))));
                any = true;
            }
            if (!any) { Destroy(g.gameObject); continue; }
            var p = new Piece { name = root.name, group = g, home = pivot, height = b.size.y, stage = StageOf(root.name, b.size) };
            order.Add((p, b.min.y, i));
        }
        // ORDEN DE OBRA: etapa, luego de abajo arriba, luego como se diseñó.
        order.Sort((a, c) => a.p.stage != c.p.stage ? a.p.stage.CompareTo(c.p.stage)
            : Mathf.Abs(a.bottom - c.bottom) > 0.05f ? a.bottom.CompareTo(c.bottom) : a.idx.CompareTo(c.idx));
        for (int i = 0; i < order.Count; i++) { order[i].p.day = DayOf(i, order.Count, daysRequired); _pieces.Add(order[i].p); }
        _preview.gameObject.SetActive(false);
    }

    // ¿Está t dentro de otra pieza (él o un antepasado suyo, por debajo de root, es pieza propia)?
    static bool OpenedBelow(Transform t, Transform root, HashSet<Transform> roots)
    {
        for (Transform x = t; x != null && x != root; x = x.parent) if (roots.Contains(x)) return true;
        return false;
    }

    // ── Las MISIONES ─────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Misión «Llevar material»: dejan en la obra los palés de un día de trabajo.</summary>
    public void DeliverMaterial(string who)
    {
        if (Finished || materialOnSite) return;
        // Del ALMACÉN de Construcción (guadua, bahareque, micelio, cuerda); sin almacén (escenas de prueba), llega sin más.
        if (MaterialStore.Instance != null && !MaterialStore.Instance.TakeDayKit(who, title)) return;
        materialOnSite = true;
        Debug.Log($"[Obra] {who} lleva el material del día a «{title}».");
        Save();
        Refresh();
    }

    /// <summary>EMPIEZA un turno de «Trabajar la obra» (lo llama la jornada al empezar la misión): mientras dura, las piezas del
    /// día salen poco a poco, <paramref name="body"/> martillea, se levanta polvo y el material del día mengua.</summary>
    public void BeginShift(string who, GameObject body, float startHour, float hours)
    {
        if (Finished) return;
        Collect();
        EndShift(who);
        var sh = new Shift { who = who, body = body, start = startHour, hours = Mathf.Max(0.1f, hours) };
        if (body != null)
        {
            Bounds bb = new Bounds(body.transform.position, Vector3.one * 1.7f);
            bool first = true;
            foreach (Renderer r in body.GetComponentsInChildren<Renderer>()) { if (first) { bb = r.bounds; first = false; } else bb.Encapsulate(r.bounds); }
            sh.handOff = new Vector3(0f, bb.min.y + bb.size.y * 0.55f - body.transform.position.y, 0f);
            sh.hammer = MakeHammer();
        }
        _shifts.Add(sh);
        Debug.Log($"[Obra] {who} empieza su turno en «{title}» ({CurrentStageName}, {Percent:0} %).");
    }

    /// <summary>Lleva el día de obra en curso hasta <paramref name="f"/> (0..1): las piezas del día van saliendo en orden y el
    /// material mengua. Nunca retrocede; sin material en la obra no se avanza.</summary>
    public void AdvanceDay(float f)
    {
        if (Finished || !materialOnSite) return;
        Collect();
        if (_progressFor != daysDone) { _progressFor = daysDone; _dayProgress = 0f; }
        _dayProgress = Mathf.Max(_dayProgress, Mathf.Clamp01(f));
        ShowPieces();
        ShrinkMaterial();
        UpdateLabel();
    }

    /// <summary>Misión «Trabajar la obra»: <paramref name="who"/> trabaja su turno. Cuando ha trabajado todo el equipo del día
    /// (<see cref="crewNeeded"/> personas distintas) se gasta el material del día y la obra avanza un día. Devuelve true al
    /// terminarla.</summary>
    public bool WorkDay(string who) => WorkDay(who, null);

    /// <summary>El TURNO de una misión de equipo: «obra_trabajo_X» es el #1; «obra_trabajo_X#3», el #3 (Berón 2026-11-08: el día cuenta cuando se han hecho
    /// todos los turnos del equipo, los haga quien los haga; con una sola persona, ella los encadena).</summary>
    public static string ShiftOf(string missionId) { int h = missionId != null ? missionId.LastIndexOf('#') : -1; return h > 0 ? missionId.Substring(h) : "#1"; }

    /// <summary><paramref name="shift"/>: el turno (<see cref="ShiftOf"/>); sin él, cada persona cuenta como un turno (como antes).</summary>
    public bool WorkDay(string who, string shift)
    {
        if (Finished) return false;
        EndShift(who);
        if (!materialOnSite) { Debug.Log($"[Obra] {who} no puede avanzar «{title}»: falta el material del día."); return false; }
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        if (day != _crewDay) { _crewDay = day; _crewToday.Clear(); }
        _crewToday.Add(shift ?? who ?? "?");   // (los turnos del equipo, no las personas: una persona puede hacer varios)
        if (_crewToday.Count < crewNeeded) { Debug.Log($"[Obra] {who} trabaja en «{title}»: {_crewToday.Count}/{crewNeeded} del equipo de hoy."); Refresh(); return false; }
        _crewToday.Clear(); _crewDay = int.MinValue;   // (un día de obra por equipo completo, aunque caiga en el mismo día de juego)
        string stage = CurrentStageName;
        materialOnSite = false;
        daysDone++;
        _progressFor = -1; _dayProgress = 0f;
        Debug.Log($"[Obra] El equipo ({crewNeeded}) trabaja en «{title}» ({stage}): {daysDone}/{daysRequired} días.");
        Save();
        Refresh();
        if (Finished) Debug.Log($"[Obra] ¡«{title}» terminada! Se abren sus misiones de entrenamiento.");
        return Finished;
    }

    void EndShift(string who)
    {
        for (int i = _shifts.Count - 1; i >= 0; i--)
            if (_shifts[i].who == who) { if (_shifts[i].hammer != null) Destroy(_shifts[i].hammer.gameObject); _shifts.RemoveAt(i); }
    }

    void EndAllShifts()
    {
        foreach (Shift s in _shifts) if (s.hammer != null) Destroy(s.hammer.gameObject);
        _shifts.Clear();
        foreach (var d in _dust) if (d.t != null) Destroy(d.t.gameObject);
        _dust.Clear();
    }

    // ── Lo que se VE ─────────────────────────────────────────────────────────────────────────────────────────────

    public void Refresh()
    {
        Collect();
        // Las paredes suben con la obra (de un murete a su altura); terminada, quedan enteras.
        float k = Finished ? 1f : Mathf.Lerp(0.15f, 0.9f, Progress);
        for (int i = 0; i < _walls.Count; i++)
        {
            Transform w = _walls[i];
            Vector3 s = w.localScale; s.y = _wallBaseY[i] * k; w.localScale = s;
            Vector3 p = w.localPosition; p.y = s.y * 0.5f; w.localPosition = p;
        }
        if (_material != null) _material.gameObject.SetActive(materialOnSite && !Finished);
        if (_scaffold != null) _scaffold.gameObject.SetActive(!Finished);
        if (_done != null) _done.gameObject.SetActive(Finished);   // terminada: aparece el área acabada de verdad (docs/obras-dojo-ballet.md)
        if (reform && TryGetComponent(out FloorForms ff)) ff.ShowConstruction(!Finished);   // (una reforma: su andamio y sus palés a la vista mientras dure)
        if (Finished) EndAllShifts();
        ShowPieces();
        ShrinkMaterial();
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (_label == null) return;
        _label.text = Finished ? $"{title} (abierta)"
            : $"{title} — EN OBRAS · {CurrentStageName} · {Percent:0} % ({daysDone}/{daysRequired} días · equipo de {crewNeeded})";
    }

    // Las piezas de los días hechos, en su sitio; las del día en curso, según el turno; el resto, aún no.
    void ShowPieces()
    {
        if (_preview == null) return;
        if (Finished) { foreach (Piece p in _pieces) p.shown = 1f; _preview.gameObject.SetActive(false); return; }   // ya está el acabado de verdad
        var today = new List<Piece>();
        foreach (Piece p in _pieces)
        {
            if (p.day <= daysDone) Place(p, 1f);
            else if (p.day == daysDone + 1) today.Add(p);
            else Place(p, 0f);
        }
        float f = DayProgress;
        for (int i = 0; i < today.Count; i++)
        {
            // Cada pieza del día tiene su rato del turno, una tras otra (con un poco de solape); al acabar el turno, todas puestas.
            float a = i / (float)today.Count * 0.9f, b = Mathf.Min(1f, a + 1.5f / today.Count);
            Place(today[i], Mathf.Clamp01((f - a) / Mathf.Max(0.01f, b - a)));
        }
        bool any = false;
        foreach (Piece p in _pieces) if (p.shown > 0f) { any = true; break; }
        _preview.gameObject.SetActive(any);
    }

    // Una pieza a su punto de obra t: crece desde una lámina y baja a su sitio (como si la colocaran).
    static void Place(Piece p, float t)
    {
        p.shown = t;
        if (p.group == null) return;
        if (t <= 0f) { p.group.gameObject.SetActive(false); return; }
        p.group.gameObject.SetActive(true);
        float e = 1f - (1f - t) * (1f - t);   // sale rápido y se asienta
        p.group.localScale = t >= 1f ? Vector3.one : new Vector3(Mathf.Lerp(0.35f, 1f, e), Mathf.Lerp(0.05f, 1f, e), Mathf.Lerp(0.35f, 1f, e));
        p.group.localPosition = p.home + Vector3.up * (1f - e) * Mathf.Clamp(p.height, 0.5f, 1.5f);
    }

    // Los palés: las piezas de material se gastan según avanza el día.
    void ShrinkMaterial()
    {
        if (_materialBits.Count == 0) return;
        float f = DayProgress;
        int keep = f <= 0f ? _materialBits.Count : Mathf.CeilToInt(_materialBits.Count * (1f - f) - 0.001f);
        for (int i = 0; i < _materialBits.Count; i++) if (_materialBits[i] != null) _materialBits[i].gameObject.SetActive(i < keep);
    }

    /// <summary>Cuántas piezas de material quedan en los palés (las que se ven) y cuántas trae un día de obra.</summary>
    public int MaterialLeft { get { int n = 0; foreach (Transform t in _materialBits) if (t != null && t.gameObject.activeSelf) n++; return n; } }
    public int MaterialFull => _materialBits.Count;

    /// <summary>Dónde se construye hoy: el centro (en el mundo) de las piezas del día en curso.</summary>
    public Vector3 WorkPoint
    {
        get
        {
            Collect();
            Vector3 s = Vector3.zero; int n = 0;
            if (_preview != null) foreach (Piece p in _pieces) if (p.day == daysDone + 1 && p.group != null) { s += _preview.TransformPoint(p.home); n++; }
            return n > 0 ? s / n : transform.position;
        }
    }
    /// <summary>Dónde está el andamio (en el mundo).</summary>
    public Vector3 ScaffoldPosition => _scaffold != null ? _scaffold.position : transform.position;

    static Transform MakeHammer()
    {
        Transform h = new GameObject("Martillo_obra").transform;
        KitchenProps.C(h, new Vector3(0f, 0.15f, 0f), new Vector3(0.04f, 0.32f, 0.04f), new Color(0.55f, 0.4f, 0.25f), 0f, "Mango");
        KitchenProps.C(h, new Vector3(0f, 0.32f, 0f), new Vector3(0.16f, 0.07f, 0.07f), new Color(0.45f, 0.45f, 0.48f), 0f, "Cabeza");
        return h;
    }

    void Update()
    {
        if (!_collected || Finished) return;
        TimeController tc = TimeController.timeController;
        float hour = tc != null ? tc.Hour : 0f;
        float best = -1f;
        for (int i = _shifts.Count - 1; i >= 0; i--)
        {
            Shift s = _shifts[i];
            float el = Mathf.Repeat(hour - s.start, 24f);
            if (el > s.hours + 0.5f) { if (s.hammer != null) Destroy(s.hammer.gameObject); _shifts.RemoveAt(i); continue; }   // turno acabado sin aviso
            best = Mathf.Max(best, Mathf.Clamp01(el / s.hours));
            // El MARTILLO: en la mano, golpeando.
            if (s.hammer != null && s.body != null)
            {
                Transform bt = s.body.transform;
                Vector3 fwd = bt.forward; fwd.y = 0f; if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
                s.hammer.position = bt.position + s.handOff + fwd.normalized * 0.35f + bt.right * 0.22f;
                s.hammer.rotation = Quaternion.LookRotation(fwd) * Quaternion.Euler(Mathf.Sin(Time.time * 9f + i) * 55f - 20f, 0f, 0f);
            }
            else if (s.hammer != null && s.body == null) { Destroy(s.hammer.gameObject); s.hammer = null; }
        }
        if (best >= 0f && materialOnSite) AdvanceDay(best);
        MoveScaffold();
        Dust(best >= 0f && materialOnSite);
    }

    // El ANDAMIO va junto a la parte que se construye (sin pisar las alfombras), despacio.
    void MoveScaffold()
    {
        if (_scaffold == null || !_scaffoldHomeSet) return;
        _scaffold.localPosition = Vector3.MoveTowards(_scaffold.localPosition, ScaffoldTarget(), 2f * Time.deltaTime);
    }

    /// <summary>Lleva el andamio ya a su sitio de hoy (las pruebas no esperan a que llegue andando).</summary>
    public void SnapScaffold()
    {
        Collect();
        if (_scaffold != null && _scaffoldHomeSet) _scaffold.localPosition = ScaffoldTarget();
    }

    Vector3 ScaffoldTarget()
    {
        Vector3 target = _scaffoldHome;
        if (daysDone > 0 || DayProgress > 0f || Working)
        {
            Vector3 w = transform.InverseTransformPoint(WorkPoint);
            Vector3 side = new Vector3(w.x, 0f, w.z); side = side.sqrMagnitude > 0.01f ? side.normalized : Vector3.right;
            target = new Vector3(w.x - side.x * 1.6f, _scaffoldHome.y, w.z - side.z * 1.6f);
            foreach (Transform c in _carpets)
            {
                if (c == null) continue;
                Vector3 cl = transform.InverseTransformPoint(c.position); cl.y = target.y;
                Vector3 d = target - cl;
                if (d.magnitude < 3f) target = cl + (d.sqrMagnitude > 0.01f ? d.normalized : Vector3.right) * 3f;
            }
        }
        return target;
    }

    // POLVO de obra donde se trabaja (pocas motas, sin colisiones, se deshacen solas).
    void Dust(bool working)
    {
        for (int i = _dust.Count - 1; i >= 0; i--)
        {
            var d = _dust[i];
            float age = Time.time - d.born;
            if (d.t == null || age > 1.4f) { if (d.t != null) Destroy(d.t.gameObject); _dust.RemoveAt(i); continue; }
            d.t.position += Vector3.up * 0.5f * Time.deltaTime;
            d.t.localScale = Vector3.one * Mathf.Lerp(0.12f, 0.45f, age / 1.4f);
        }
        if (!working || Time.time < _nextDust || _dust.Count >= 16) return;
        _nextDust = Time.time + 0.3f;
        Vector3 at = WorkPoint + new Vector3(Random.Range(-1f, 1f), 0.2f, Random.Range(-1f, 1f));
        Transform m = KitchenProps.C(null, at, Vector3.one * 0.12f, new Color(0.8f, 0.74f, 0.62f), Random.Range(0f, 90f), "Polvo_obra");
        m.position = at;
        _dust.Add((m, Time.time));
    }

    void OnDestroy() { EndAllShifts(); }

    // ── GUARDAR por etapa: días hechos y material en la obra ─────────────────────────────────────────────────────

    string Key => "cs_obra_" + title;
    bool CanSave => persist && Application.isPlaying && !ColdSanctuary.TestRunner.Running;

    void Save()
    {
        if (!CanSave) return;
        PlayerPrefs.SetString(Key, $"{daysDone}|{(materialOnSite ? 1 : 0)}");
        PlayerPrefs.Save();
    }

    void Load()
    {
        if (_loaded) return;
        _loaded = true;
        if (!CanSave) return;
        string[] parts = PlayerPrefs.GetString(Key, "").Split('|');
        if (parts.Length == 2 && int.TryParse(parts[0], out int d)) Restore(d, parts[1] == "1");
    }

    /// <summary>Deja la obra en <paramref name="days"/> días hechos (al cargar la partida, o en las pruebas): enseña las piezas
    /// de esas etapas, ni una más.</summary>
    public void Restore(int days, bool material)
    {
        daysDone = Mathf.Clamp(days, 0, daysRequired);
        materialOnSite = material;
        _progressFor = -1; _dayProgress = 0f;
        _crewToday.Clear(); _crewDay = int.MinValue;
        if (_collected) Refresh();
    }

    /// <summary>Olvida el estado guardado de todas las obras (empezar de cero).</summary>
    public static void ClearSaved()
    {
        foreach (ConstructionProject p in All) PlayerPrefs.DeleteKey(p.Key);
        PlayerPrefs.Save();
    }
}
