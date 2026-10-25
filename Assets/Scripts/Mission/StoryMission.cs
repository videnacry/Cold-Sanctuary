using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MISIÓN DE HISTORIA POR PASOS (docs/microcosmos-historias.md): la misión genérica de los microcosmos que recrean la vida
/// de un personaje histórico (Patanjali, Nightingale, los Wright, Eratóstenes, Buda, la tregua de Navidad…). Es una lista
/// de <see cref="StoryStep"/> que se cumplen en orden con lo que hay en la escena: hablar con alguien, llegar a un sitio,
/// recoger cosas (y llevarlas), atender uno a uno a varios, o quedarse quieto un rato junto a algo (meditar, escuchar,
/// medir una sombra). Una sola tecla (E), con el objetivo en el HUD. Al terminar, la <see cref="HistoryCard"/> cuenta lo que
/// pasó de verdad, con fuentes. Las historias con mecánica propia (Egina, compasión) siguen con su misión.
/// </summary>
[RequireComponent(typeof(HistoryCard))]
public class StoryMission : MonoBehaviour
{
    public enum Kind { Talk, Reach, Collect, VisitAll, Hold }
    public enum Marga { Stats, Yoga, Bonds }

    [System.Serializable]
    public class StoryStep
    {
        [Tooltip("Objetivo en el HUD («Habla con Patanjali»).")]
        public string goal = "";
        public Kind kind;
        [Tooltip("Talk/Reach/Hold: el primero. Collect: lo que se recoge. VisitAll: cada uno, una vez.")]
        public Transform[] targets = new Transform[0];
        [Tooltip("Collect: cuántos hay que recoger (0 = todos).")]
        public int count;
        [Tooltip("Collect: adónde llevarlos (vacío = basta con recogerlos).")]
        public Transform deliverTo;
        [Tooltip("Hold: segundos quieto junto al objetivo.")]
        public float seconds = 10f;
        [Tooltip("Lo que se dice al cumplirlo (quién y qué).")]
        public string speaker = "", line = "";
        [Tooltip("Al cumplirlo aparecen / desaparecen (la bici arreglada, el avión, la ventana abierta…).")]
        public GameObject[] reveal = new GameObject[0], hide = new GameObject[0];
    }

    public string title = "";
    public StoryStep[] steps = new StoryStep[0];
    public Marga marga = Marga.Bonds;
    public float xp = 50f;
    public float reach = 2.5f;
    public string finalSpeaker = "", finalLine = "";

    public static StoryMission Instance { get; private set; }

    HistoryCard _card;
    Transform _player;
    int _step;
    float _held;
    Vector3 _lastPos;
    readonly HashSet<Transform> _done = new HashSet<Transform>();
    readonly List<Transform> _carried = new List<Transform>();
    string _note; float _noteUntil;

    public int Step => _step;
    public bool Finished => _step >= steps.Length;
    StoryStep Current => Finished ? null : steps[_step];
    bool CardOpen => _card != null && _card.IsOpen;

    void OnEnable() => Instance = this;
    void OnDisable() { if (Instance == this) Instance = null; }

    void Start()
    {
        _card = GetComponent<HistoryCard>();
        if (string.IsNullOrEmpty(_card.title)) _card.title = title;
        foreach (StoryStep s in steps) foreach (GameObject g in s.reveal) if (g != null) g.SetActive(false);
    }

    public void Say(string who, string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        _note = string.IsNullOrEmpty(who) ? line : $"{who}: «{line}»";
        _noteUntil = Time.time + 6f;
        Debug.Log($"[Historia] {title} · {_note}");
    }

    Transform Player()
    {
        if (_player == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p != null) _player = p.transform; }
        return _player;
    }

    float Dist(Transform t) => t == null || _player == null ? float.MaxValue : Vector2.Distance(new Vector2(t.position.x, t.position.z), new Vector2(_player.position.x, _player.position.z));

    Transform NearestPending(StoryStep s)
    {
        Transform best = null; float bd = reach;
        foreach (Transform t in s.targets)
        {
            if (t == null || _done.Contains(t) || !t.gameObject.activeInHierarchy) continue;
            float d = Dist(t);
            if (d < bd) { bd = d; best = t; }
        }
        return best;
    }

    int Needed(StoryStep s) => s.count > 0 ? Mathf.Min(s.count, s.targets.Length) : s.targets.Length;
    int Pending(StoryStep s) { int n = 0; foreach (Transform t in s.targets) if (t != null && !_done.Contains(t)) n++; return n; }

    void Update()
    {
        if (CardOpen || Finished || Player() == null) return;
        StoryStep s = Current;
        Transform first = s.targets.Length > 0 ? s.targets[0] : null;
        switch (s.kind)
        {
            case Kind.Reach:
                if (Dist(first) <= reach * 1.6f) Complete(s);
                return;
            case Kind.Hold:
                bool still = (_player.position - _lastPos).sqrMagnitude < 0.0004f;
                _lastPos = _player.position;
                if (Dist(first) <= reach * 1.6f && still) _held += Time.deltaTime; else _held = Mathf.Max(0f, _held - Time.deltaTime * 2f);
                if (_held >= s.seconds) Complete(s);
                return;
        }
        if (GameKeys.DialogOpen || !GameKeys.GetKeyDown(KeyCode.E)) return;
        if (s.kind == Kind.Talk) { if (Dist(first) <= reach) Complete(s); return; }
        if (s.kind == Kind.Collect && s.deliverTo != null && _carried.Count >= Needed(s) && Dist(s.deliverTo) <= reach * 1.4f) { Drop(s.deliverTo); Complete(s); return; }
        Transform t = NearestPending(s);
        if (t == null) return;
        _done.Add(t);
        if (s.kind == Kind.Collect)
        {
            if (_carried.Count >= Needed(s)) { _done.Remove(t); return; }
            Carry(t);
            if (s.deliverTo == null && _carried.Count >= Needed(s)) { foreach (Transform c in _carried) c.gameObject.SetActive(false); _carried.Clear(); Complete(s); }
            return;
        }
        WorldLabel wl = t.GetComponent<WorldLabel>(); if (wl != null) wl.text += " ✓";
        if (Pending(s) == 0) Complete(s);
    }

    // Lo recogido va SOBRE LA CABEZA (como las recolectoras de la tribu), a la vista.
    void Carry(Transform t)
    {
        foreach (Collider c in t.GetComponentsInChildren<Collider>()) c.enabled = false;
        t.SetParent(_player, true);
        t.localPosition = Vector3.up * (2.2f + _carried.Count * 0.5f);
        _carried.Add(t);
    }

    void Drop(Transform at)
    {
        for (int i = 0; i < _carried.Count; i++)
        {
            Transform t = _carried[i];
            t.SetParent(at, true);
            t.position = at.position + Quaternion.Euler(0f, i * 40f, 0f) * Vector3.forward * 1.2f + Vector3.up * 0.4f;
        }
        _carried.Clear();
    }

    void Complete(StoryStep s)
    {
        foreach (GameObject g in s.reveal) if (g != null) g.SetActive(true);
        foreach (GameObject g in s.hide) if (g != null) g.SetActive(false);
        Say(s.speaker, s.line);
        _step++; _held = 0f;
        if (Finished) Finish();
    }

    void Finish()
    {
        CharacterLevel lv = FindFirstObjectByType<CharacterLevel>();
        if (lv != null)
        {
            if (marga == Marga.Yoga) lv.GainYogaXp(xp);
            else if (marga == Marga.Stats) lv.GainStatsXp(xp);
            else lv.GainBondXp(xp);
        }
        Say(finalSpeaker, finalLine);
        _card.ShowFinal();
        Progress.MarkStory(gameObject.scene.name);   // el checklist de progreso (hechizo «progreso»)
    }

    void OnGUI()
    {
        if (CardOpen) return;
        GUIStyle wrap = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 15 };
        GUI.Box(new Rect(10f, 10f, 460f, 78f), "");
        GUI.Label(new Rect(20f, 14f, 440f, 22f), title, wrap);
        StoryStep s = Current;
        if (s == null) GUI.Label(new Rect(20f, 36f, 440f, 44f), "Historia completada.", wrap);
        else
        {
            string prog = s.kind == Kind.Collect ? (s.deliverTo != null && _carried.Count >= Needed(s) ? $" (llévalo a {s.deliverTo.name})" : $" ({_carried.Count}/{Needed(s)})")
                        : s.kind == Kind.VisitAll ? $" ({s.targets.Length - Pending(s)}/{s.targets.Length})"
                        : s.kind == Kind.Hold ? $" ({_held:0}/{s.seconds:0} s, quieto)" : "";
            GUI.Label(new Rect(20f, 36f, 440f, 22f), $"{_step + 1}/{steps.Length} · {s.goal}{prog}", wrap);
            bool canE = s.kind == Kind.Talk ? Dist(s.targets.Length > 0 ? s.targets[0] : null) <= reach : (s.kind == Kind.Collect || s.kind == Kind.VisitAll) && NearestPending(s) != null;
            if (canE) GUI.Label(new Rect(20f, 58f, 440f, 22f), "E · " + (s.kind == Kind.Talk ? "hablar" : s.kind == Kind.Collect ? "recoger" : "atender"), wrap);
        }
        if (Time.time < _noteUntil) GUI.Label(new Rect(Screen.width * 0.5f - 300f, Screen.height - 96f, 600f, 70f), _note, new GUIStyle(wrap) { alignment = TextAnchor.MiddleCenter });
    }
}
